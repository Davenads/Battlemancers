using System;
using System.Collections.Generic;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Core.Simulation.StatusEffects;

namespace Battlemancers.Simulation.Status
{
    /// <summary>
    /// Manages all active status effects across every unit in a simulation.
    ///
    /// StatusManager is the single authority for applying, ticking, and removing
    /// <see cref="StatusEffect"/> instances. It enforces the per-type stacking rules
    /// defined in <see cref="StatusType"/>, keeps <see cref="UnitState.ActiveStatusTypes"/>
    /// in sync, and publishes status events to <see cref="SimulationEventBus"/>.
    ///
    /// Per-tick damage values:
    /// <list type="bullet">
    ///   <item><description><see cref="StatusType.Burning"/>  — 5 HP per tick (flat).</description></item>
    ///   <item><description><see cref="StatusType.Poisoned"/> — 3 HP × current stack count per tick (max 15 HP/turn at 5 stacks).</description></item>
    ///   <item><description>All other status types — 0 HP per tick (behavioural effects only).</description></item>
    /// </list>
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class StatusManager
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        /// <summary>Flat HP damage dealt per tick by <see cref="StatusType.Burning"/>.</summary>
        private const int BurningDamagePerTick = 5;

        /// <summary>HP damage dealt per stack per tick by <see cref="StatusType.Poisoned"/>.</summary>
        private const int PoisonDamagePerStack = 3;

        /// <summary>Maximum stack count for <see cref="StatusType.Poisoned"/>.</summary>
        private const int PoisonMaxStacks = 5;

        // ---------------------------------------------------------------------------
        // State
        // ---------------------------------------------------------------------------

        /// <summary>
        /// All active status effects, keyed by unit runtime ID.
        /// Each unit maps to a list of its current statuses (one entry per StatusType at most,
        /// except Poisoned which uses StackCount to represent multiple applications).
        /// </summary>
        private readonly Dictionary<string, List<StatusEffect>> _unitStatuses
            = new Dictionary<string, List<StatusEffect>>();

        /// <summary>
        /// Concrete <see cref="IStatusEffect"/> behavioural objects for the six typed status effects
        /// (Burning, Wet, Poisoned, Charged, Silenced, Cursed), keyed by unit runtime ID.
        /// Populated by <see cref="SyncConcreteEffect"/> after each <see cref="ApplyStatus"/> call;
        /// consulted by <see cref="TickStatuses"/> to invoke per-tick side-effects such as DoT damage
        /// and fire spreading. Entries are removed when the corresponding <see cref="StatusEffect"/>
        /// expires or is cleansed.
        /// </summary>
        private readonly Dictionary<string, List<IStatusEffect>> _concreteEffects
            = new Dictionary<string, List<IStatusEffect>>();

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Applies a status effect to the specified unit, enforcing type-specific stacking rules:
        ///
        /// <list type="bullet">
        ///   <item><description><b>Burning, Wet, Charged, Cursed, Silenced, Slowed</b> — Duration stacks:
        ///     if already present, the incoming duration is added to the current remaining duration.</description></item>
        ///   <item><description><b>Frozen</b> — Replace if longer: existing status is only overwritten
        ///     when the new duration strictly exceeds the current duration.</description></item>
        ///   <item><description><b>Poisoned</b> — Count stacks: each call adds one stack (up to max 5);
        ///     if at max stacks, duration is refreshed to whichever value is longer.</description></item>
        ///   <item><description><b>Stunned</b> — Cannot stack: only one instance ever; duration is reset
        ///     to the incoming value on re-apply but stack count stays at 1.</description></item>
        ///   <item><description><b>Panicked, Charmed</b> — Cannot stack: if already active, the
        ///     incoming application is silently ignored (must expire first).</description></item>
        /// </list>
        ///
        /// After mutation, <see cref="UnitState.ActiveStatusTypes"/> is updated and a
        /// <see cref="StatusAppliedEvent"/> is published via <see cref="SimulationEventBus"/>.
        /// </summary>
        /// <param name="unitId">Runtime ID of the target unit.</param>
        /// <param name="effect">
        /// The status to apply. Must not be null. Ownership of this object is transferred to
        /// the manager — do not mutate it after calling this method.
        /// </param>
        /// <param name="unitState">
        /// The <see cref="UnitState"/> of the target unit, used to keep
        /// <see cref="UnitState.ActiveStatusTypes"/> in sync. Must not be null.
        /// </param>
        /// <param name="turnNumber">Current simulation turn number, used to stamp published events.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="effect"/> or <paramref name="unitState"/> is null.</exception>
        public void ApplyStatus(string unitId, StatusEffect effect, UnitState unitState, int turnNumber = 0)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect));
            if (unitState == null) throw new ArgumentNullException(nameof(unitState));

            if (!_unitStatuses.TryGetValue(unitId, out List<StatusEffect> list))
            {
                list = new List<StatusEffect>();
                _unitStatuses[unitId] = list;
            }

            StatusEffect existing = FindEffect(list, effect.Type);

            switch (effect.Type)
            {
                // --- Duration-stacking types ---
                case StatusType.Burning:
                case StatusType.Wet:
                case StatusType.Charged:
                case StatusType.Cursed:
                case StatusType.Silenced:
                case StatusType.Slowed:
                case StatusType.Rooted:
                case StatusType.TimeSlow:
                    if (existing != null)
                    {
                        existing.Duration += effect.Duration;
                    }
                    else
                    {
                        list.Add(effect);
                        AddActiveStatusType(unitState, effect.Type);
                    }
                    break;

                // --- Replace if longer ---
                case StatusType.Frozen:
                    if (existing != null)
                    {
                        if (effect.Duration > existing.Duration)
                            existing.Duration = effect.Duration;
                        // Otherwise no change — shorter or equal duration is ignored.
                    }
                    else
                    {
                        list.Add(effect);
                        AddActiveStatusType(unitState, effect.Type);
                    }
                    break;

                // --- Count stacks ---
                case StatusType.Poisoned:
                    if (existing != null)
                    {
                        if (existing.StackCount < PoisonMaxStacks)
                        {
                            existing.StackCount++;
                            // Also take the longer duration so the stack isn't cut short.
                            if (effect.Duration > existing.Duration)
                                existing.Duration = effect.Duration;
                        }
                        else
                        {
                            // Already at max stacks — refresh to whichever duration is longer.
                            if (effect.Duration > existing.Duration)
                                existing.Duration = effect.Duration;
                        }
                    }
                    else
                    {
                        list.Add(effect);
                        AddActiveStatusType(unitState, effect.Type);
                    }
                    break;

                // --- Cannot stack; duration reset on re-apply ---
                case StatusType.Stunned:
                    if (existing != null)
                    {
                        existing.Duration = effect.Duration;
                        // Stack count stays at 1 by definition.
                    }
                    else
                    {
                        list.Add(effect);
                        AddActiveStatusType(unitState, effect.Type);
                    }
                    break;

                // --- Cannot stack; silently ignore while active ---
                case StatusType.Panicked:
                case StatusType.Charmed:
                case StatusType.Confused:
                case StatusType.Haste:
                    if (existing != null)
                    {
                        // Already active — ignore incoming application entirely.
                        return;
                    }
                    list.Add(effect);
                    AddActiveStatusType(unitState, effect.Type);
                    break;

                default:
                    // Fallback: treat unknown types as duration-stacking.
                    if (existing != null)
                        existing.Duration += effect.Duration;
                    else
                    {
                        list.Add(effect);
                        AddActiveStatusType(unitState, effect.Type);
                    }
                    break;
            }

            // Publish after mutation so subscribers see the final state.
            StatusEffect applied = FindEffect(list, effect.Type);
            SimulationEventBus.Publish(new StatusAppliedEvent(
                turnNumber,
                unitId,
                applied.Type,
                applied.Duration,
                applied.StackCount));

            // Keep the concrete IStatusEffect representation in sync with the data model.
            SyncConcreteEffect(unitId, applied);

            // Apply any cross-status side-effects triggered by this type (e.g. Wet → extinguish Burning).
            HandleApplySideEffects(unitId, effect.Type, unitState, turnNumber);
        }

        /// <summary>
        /// Processes one end-of-turn tick for every active status on every living unit in
        /// <paramref name="state"/>.
        ///
        /// For each status tick:
        /// <list type="number">
        ///   <item><description>Per-tick damage is computed and applied to <see cref="UnitState.CurrentHP"/> (clamped at 0).</description></item>
        ///   <item><description>Duration is decremented by 1.</description></item>
        ///   <item><description>If duration reaches 0, the status is removed and a <see cref="StatusRemovedEvent"/> is published with reason "expired".</description></item>
        ///   <item><description>A <see cref="StatusTickedEvent"/> is published for every tick.</description></item>
        /// </list>
        ///
        /// Dead units (CurrentHP == 0) are skipped — their statuses are not ticked and
        /// are cleaned up when the unit is deregistered.
        /// </summary>
        /// <param name="state">
        /// The current <see cref="SimulationState"/>. All living units are iterated.
        /// Must not be null.
        /// </param>
        /// <returns>
        /// An array of <see cref="StatusTickResult"/> — one entry per status per living unit
        /// that had at least one active status. Empty array if no statuses were active.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="state"/> is null.</exception>
        public StatusTickResult[] TickStatuses(SimulationState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            List<StatusTickResult> results = new List<StatusTickResult>();
            int turnNumber = state.TurnNumber;

            foreach (UnitState unit in state.GetLivingUnits())
            {
                if (!_unitStatuses.TryGetValue(unit.Id, out List<StatusEffect> statuses))
                    continue;

                // Collect statuses to remove after iteration to avoid mutating the list mid-loop.
                List<StatusType> toRemove = null;

                // Pre-compute whether the unit is frozen so we can skip Poisoned ticking.
                bool unitIsFrozen = HasStatus(unit.Id, StatusType.Frozen);

                foreach (StatusEffect effect in statuses)
                {
                    // Design rule: POISONED duration does not decay while the unit also carries
                    // FROZEN ("no decay timer while frozen"). Skip the tick entirely for Poisoned
                    // — this pauses both damage and duration while the freeze holds.
                    if (unitIsFrozen && effect.Type == StatusType.Poisoned)
                        continue;

                    // --- Compute damage via concrete IStatusEffect if available, else legacy path ---
                    int damage = 0;
                    IStatusEffect concrete = FindConcreteEffect(unit.Id, effect.Type);
                    if (concrete != null)
                    {
                        // Concrete implementation handles damage internally via Tick().
                        // Measure the HP delta so we can report it in StatusTickResult/events.
                        int hpBefore = unit.CurrentHP;
                        concrete.Tick(unit, state);
                        damage = Math.Max(0, hpBefore - unit.CurrentHP);
                    }
                    else
                    {
                        // Legacy: StatusManager computes and applies damage directly.
                        damage = ComputeTickDamage(effect);
                        if (damage > 0)
                        {
                            int actualDamage = Math.Min(damage, unit.CurrentHP);
                            unit.CurrentHP -= actualDamage;
                            damage = actualDamage;
                        }
                    }

                    // --- Decrement duration ---
                    effect.Duration--;
                    // Keep the concrete RemainingDuration in sync with the StatusEffect Duration.
                    if (concrete != null)
                        concrete.RemainingDuration = effect.Duration;

                    bool expired = effect.Duration <= 0;

                    if (expired)
                    {
                        if (toRemove == null) toRemove = new List<StatusType>();
                        toRemove.Add(effect.Type);
                    }

                    // --- Publish tick event ---
                    SimulationEventBus.Publish(new StatusTickedEvent(
                        turnNumber, unit.Id, effect.Type, damage, expired));

                    results.Add(new StatusTickResult(
                        unit.Id, effect.Type, damage,
                        statusExpired: expired,
                        statusModified: true));
                }

                // --- Remove expired statuses ---
                if (toRemove != null)
                {
                    foreach (StatusType expiredType in toRemove)
                    {
                        RemoveEffectFromList(statuses, expiredType);
                        RemoveActiveStatusType(unit, expiredType);
                        RemoveConcreteEffect(unit.Id, expiredType);

                        SimulationEventBus.Publish(new StatusRemovedEvent(
                            turnNumber, unit.Id, expiredType, "expired"));
                    }

                    // Clean up the dictionary entry if the unit now has no statuses.
                    if (statuses.Count == 0)
                        _unitStatuses.Remove(unit.Id);
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// Force-removes a status effect from a unit regardless of its remaining duration.
        /// Used by cleanse spells (e.g., Hydromancer Cleanse, Chronomancer Rewind).
        ///
        /// If the unit does not have the specified status, this is a no-op.
        /// Publishes a <see cref="StatusRemovedEvent"/> with reason "cleansed" when removed.
        /// </summary>
        /// <param name="unitId">Runtime ID of the target unit.</param>
        /// <param name="type">The status type to remove.</param>
        /// <param name="unitState">
        /// The <see cref="UnitState"/> of the target unit, used to keep
        /// <see cref="UnitState.ActiveStatusTypes"/> in sync. Must not be null.
        /// </param>
        /// <param name="turnNumber">Current simulation turn number, used to stamp published events.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="unitState"/> is null.</exception>
        public void RemoveStatus(string unitId, StatusType type, UnitState unitState, int turnNumber = 0)
        {
            if (unitState == null) throw new ArgumentNullException(nameof(unitState));

            if (!_unitStatuses.TryGetValue(unitId, out List<StatusEffect> list))
                return;

            bool removed = RemoveEffectFromList(list, type);
            if (!removed)
                return;

            RemoveActiveStatusType(unitState, type);
            RemoveConcreteEffect(unitId, type);

            if (list.Count == 0)
                _unitStatuses.Remove(unitId);

            SimulationEventBus.Publish(new StatusRemovedEvent(turnNumber, unitId, type, "cleansed"));
        }

        /// <summary>
        /// Returns true if the specified unit currently has an active status of the given type.
        /// </summary>
        /// <param name="unitId">Runtime ID of the unit to query.</param>
        /// <param name="type">The status type to check for.</param>
        /// <returns><c>true</c> if the status is active on this unit; otherwise <c>false</c>.</returns>
        public bool HasStatus(string unitId, StatusType type)
        {
            if (!_unitStatuses.TryGetValue(unitId, out List<StatusEffect> list))
                return false;

            return FindEffect(list, type) != null;
        }

        /// <summary>
        /// Returns a read-only view of all active status effects on the specified unit.
        /// Returns an empty list if the unit has no active statuses or is not tracked.
        /// </summary>
        /// <param name="unitId">Runtime ID of the unit to query.</param>
        /// <returns>
        /// A <see cref="IReadOnlyList{StatusEffect}"/> of active effects.
        /// The returned list must not be modified by callers.
        /// </returns>
        public IReadOnlyList<StatusEffect> GetStatuses(string unitId)
        {
            if (_unitStatuses.TryGetValue(unitId, out List<StatusEffect> list))
                return list.AsReadOnly();

            return Array.Empty<StatusEffect>();
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Computes the HP damage this status deals in a single tick.
        /// Only used for status types that do NOT have a concrete <see cref="IStatusEffect"/>
        /// implementation — for those types, <see cref="IStatusEffect.Tick"/> handles damage.
        /// </summary>
        private static int ComputeTickDamage(StatusEffect effect)
        {
            switch (effect.Type)
            {
                case StatusType.Burning:
                    return BurningDamagePerTick;
                case StatusType.Poisoned:
                    return PoisonDamagePerStack * effect.StackCount;
                default:
                    return 0;
            }
        }

        // ---------------------------------------------------------------------------
        // Concrete IStatusEffect factory and helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Factory that constructs the appropriate concrete <see cref="IStatusEffect"/> for the
        /// given <paramref name="type"/>. This is the only switch on <see cref="StatusType"/>
        /// outside of the six concrete class files — callers never need their own dispatch.
        /// Returns <c>null</c> for types that do not have a concrete implementation.
        /// </summary>
        private static IStatusEffect CreateConcreteEffect(
            StatusType type, int duration, int stackCount, string sourceId)
        {
            switch (type)
            {
                case StatusType.Burning:  return new BurningStatus(duration, sourceId);
                case StatusType.Wet:      return new WetStatus(duration, sourceId);
                case StatusType.Poisoned: return new PoisonedStatus(duration, stackCount, sourceId);
                case StatusType.Charged:  return new ChargedStatus(duration, sourceId);
                case StatusType.Silenced: return new SilencedStatus(duration, sourceId);
                case StatusType.Cursed:   return new CursedStatus(duration, sourceId);
                default:                  return null;
            }
        }

        /// <summary>
        /// Creates or updates the concrete <see cref="IStatusEffect"/> in <see cref="_concreteEffects"/>
        /// to mirror the state of <paramref name="updatedEffect"/> after an <see cref="ApplyStatus"/> call.
        /// If the type has no concrete implementation, this is a no-op.
        /// </summary>
        private void SyncConcreteEffect(string unitId, StatusEffect updatedEffect)
        {
            IStatusEffect existing = FindConcreteEffect(unitId, updatedEffect.Type);

            if (existing == null)
            {
                IStatusEffect newConcrete = CreateConcreteEffect(
                    updatedEffect.Type,
                    updatedEffect.Duration,
                    updatedEffect.StackCount,
                    updatedEffect.SourceId);

                if (newConcrete == null)
                    return; // No concrete for this type.

                if (!_concreteEffects.TryGetValue(unitId, out List<IStatusEffect> concretes))
                {
                    concretes = new List<IStatusEffect>();
                    _concreteEffects[unitId] = concretes;
                }

                concretes.Add(newConcrete);
            }
            else
            {
                // Sync duration and, for Poisoned, the stack count.
                existing.RemainingDuration = updatedEffect.Duration;
                if (existing is PoisonedStatus poisoned)
                    poisoned.StackCount = updatedEffect.StackCount;
            }
        }

        /// <summary>
        /// Handles cross-status side-effects triggered when a status of the given
        /// <paramref name="type"/> is applied. Currently:
        /// <list type="bullet">
        ///   <item><description><b>Wet</b> — extinguishes any active Burning on the same unit.</description></item>
        /// </list>
        /// All side-effect logic lives here so that no callers of <see cref="ApplyStatus"/>
        /// need their own switch on <see cref="StatusType"/>.
        /// </summary>
        private void HandleApplySideEffects(
            string unitId, StatusType type, UnitState unitState, int turnNumber)
        {
            if (type == StatusType.Wet && HasStatus(unitId, StatusType.Burning))
                RemoveStatus(unitId, StatusType.Burning, unitState, turnNumber);
        }

        /// <summary>
        /// Returns the concrete <see cref="IStatusEffect"/> for the given unit and status type,
        /// or <c>null</c> if none exists.
        /// Matches by <see cref="IStatusEffect.DisplayName"/> == <paramref name="type"/>.ToString().
        /// </summary>
        private IStatusEffect FindConcreteEffect(string unitId, StatusType type)
        {
            if (!_concreteEffects.TryGetValue(unitId, out List<IStatusEffect> concretes))
                return null;

            string displayName = type.ToString();
            foreach (IStatusEffect c in concretes)
            {
                if (c.DisplayName == displayName)
                    return c;
            }

            return null;
        }

        /// <summary>
        /// Removes the concrete <see cref="IStatusEffect"/> for the given unit and type from
        /// <see cref="_concreteEffects"/>. No-op if no matching concrete exists.
        /// Cleans up the outer dictionary entry when the unit's concrete list becomes empty.
        /// </summary>
        private void RemoveConcreteEffect(string unitId, StatusType type)
        {
            if (!_concreteEffects.TryGetValue(unitId, out List<IStatusEffect> concretes))
                return;

            string displayName = type.ToString();
            for (int i = concretes.Count - 1; i >= 0; i--)
            {
                if (concretes[i].DisplayName == displayName)
                {
                    concretes.RemoveAt(i);
                    break;
                }
            }

            if (concretes.Count == 0)
                _concreteEffects.Remove(unitId);
        }

        /// <summary>Finds the first effect of the given type in the list, or null.</summary>
        private static StatusEffect FindEffect(List<StatusEffect> list, StatusType type)
        {
            foreach (StatusEffect e in list)
            {
                if (e.Type == type)
                    return e;
            }
            return null;
        }

        /// <summary>
        /// Removes the first effect matching <paramref name="type"/> from <paramref name="list"/>.
        /// Returns true if an effect was removed; false if none was found.
        /// </summary>
        private static bool RemoveEffectFromList(List<StatusEffect> list, StatusType type)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == type)
                {
                    list.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds the string representation of <paramref name="type"/> to
        /// <see cref="UnitState.ActiveStatusTypes"/> if not already present.
        /// </summary>
        private static void AddActiveStatusType(UnitState unitState, StatusType type)
        {
            string key = type.ToString();
            if (!unitState.ActiveStatusTypes.Contains(key))
                unitState.ActiveStatusTypes.Add(key);
        }

        /// <summary>
        /// Removes the string representation of <paramref name="type"/> from
        /// <see cref="UnitState.ActiveStatusTypes"/>.
        /// </summary>
        private static void RemoveActiveStatusType(UnitState unitState, StatusType type)
        {
            unitState.ActiveStatusTypes.Remove(type.ToString());
        }
    }
}

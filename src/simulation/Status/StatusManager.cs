using System;
using System.Collections.Generic;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;

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

                foreach (StatusEffect effect in statuses)
                {
                    // --- Compute damage ---
                    int damage = ComputeTickDamage(effect);

                    if (damage > 0)
                    {
                        int actualDamage = Math.Min(damage, unit.CurrentHP);
                        unit.CurrentHP -= actualDamage;
                        damage = actualDamage;
                    }

                    // --- Decrement duration ---
                    effect.Duration--;
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

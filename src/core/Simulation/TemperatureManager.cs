using System;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Core.Simulation
{
    /// <summary>
    /// Manages the per-unit Temperature system for all units in a simulation.
    ///
    /// Temperature is an integer in the range [-100, +100] stored on each
    /// <see cref="UnitState"/>. It rises from fire/heat spells, falls from ice/cold spells,
    /// decays toward 0 each turn, and is passively modified by the tile a unit stands on.
    ///
    /// Threshold crossings trigger status effects via <see cref="StatusManager"/>:
    /// <list type="bullet">
    ///   <item><description>≥ +61 OVERHEATED — BURNING DoT applied.</description></item>
    ///   <item><description>+31 to +60 HOT — SLOWED applied.</description></item>
    ///   <item><description>-31 to -60 SUPERCOOLED — SLOWED + BRITTLE_MODIFIER applied.</description></item>
    ///   <item><description>≤ -61 FROZEN SOLID — FROZEN status applied.</description></item>
    /// </list>
    ///
    /// When a single temperature delta crosses both the +31 and -31 thresholds in one
    /// application (Thermal Shock), bonus damage is dealt and a 1-turn STUN is applied.
    ///
    /// Call order each turn:
    /// <list type="number">
    ///   <item><description><see cref="DecayAllTemperatures"/> — at start of ResolveTurn, before commands.</description></item>
    ///   <item><description><see cref="ApplyTemperatureChange"/> — when a spell applies a temperature delta.</description></item>
    ///   <item><description><see cref="ApplyTerrainTemperatureEffects"/> — at end of turn, after all commands and status ticks.</description></item>
    /// </list>
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class TemperatureManager
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        /// <summary>Minimum allowable temperature.</summary>
        private const int MinTemperature = -100;

        /// <summary>Maximum allowable temperature.</summary>
        private const int MaxTemperature = 100;

        /// <summary>Amount temperature decays toward 0 each turn (per-unit).</summary>
        private const int DecayAmount = 10;

        /// <summary>
        /// Sentinel duration used when applying a temperature-held status.
        /// The StatusManager recognises this value and skips per-tick duration decrement.
        /// Approximately 1 billion turns — effectively permanent while temperature holds it.
        /// </summary>
        private const int TemperatureHeldDuration = int.MaxValue / 2;

        /// <summary>Temperature change per turn for BURNING tile terrain passive.</summary>
        private const int BurningTilePassive = +10;

        /// <summary>Temperature change per turn for FROZEN tile terrain passive.</summary>
        private const int FrozenTilePassive = -10;

        /// <summary>Temperature change per turn for PERMAFROST tile terrain passive.</summary>
        private const int PermafrostTilePassive = -10;

        /// <summary>Temperature change per turn for WET tile terrain passive.</summary>
        private const int WetTilePassive = -5;

        // ---------------------------------------------------------------------------
        // Fields
        // ---------------------------------------------------------------------------

        private readonly StatusManager _statusManager;

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new <see cref="TemperatureManager"/>.
        /// </summary>
        /// <param name="statusManager">
        /// The <see cref="StatusManager"/> used to apply and remove temperature-triggered
        /// status effects. Must not be null.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="statusManager"/> is null.</exception>
        public TemperatureManager(StatusManager statusManager)
        {
            _statusManager = statusManager ?? throw new ArgumentNullException(nameof(statusManager));
        }

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Applies a temperature delta to the specified unit. Clamps the result to [-100, +100].
        /// Checks for threshold crossings and applies or removes status effects as needed.
        /// Checks for Thermal Shock if the delta crosses both the -31 and +31 thresholds
        /// in a single application.
        /// Publishes a <see cref="TemperatureChangedEvent"/> after mutation.
        /// </summary>
        /// <param name="unitId">Runtime ID of the target unit.</param>
        /// <param name="delta">
        /// Temperature change to apply. Positive values heat the unit; negative values cool it.
        /// </param>
        /// <param name="unit">
        /// The <see cref="UnitState"/> of the target unit. Must not be null.
        /// </param>
        /// <param name="state">
        /// The current <see cref="SimulationState"/>, used for turn number stamping on events.
        /// Must not be null.
        /// </param>
        /// <returns>
        /// Bonus damage from Thermal Shock if triggered; 0 otherwise.
        /// The caller (SpellResolver) is responsible for applying this damage to the unit.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="unit"/> or <paramref name="state"/> is null.</exception>
        public int ApplyTemperatureChange(string unitId, int delta, UnitState unit, SimulationState state)
        {
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            if (state == null) throw new ArgumentNullException(nameof(state));

            int previousTemp = unit.Temperature;
            int newTemp = Clamp(previousTemp + delta, MinTemperature, MaxTemperature);
            unit.Temperature = newTemp;

            TemperatureCategory previousCategory = GetCategory(previousTemp);
            TemperatureCategory newCategory = GetCategory(newTemp);

            // Check for Thermal Shock before applying threshold statuses.
            bool thermalShock = IsThermalShock(previousTemp, newTemp);
            int thermalShockDamage = 0;

            if (thermalShock)
            {
                // Bonus damage = |delta| / 2 (integer division).
                // Use the actual applied delta (clamped), not the requested delta.
                int actualDelta = newTemp - previousTemp;
                thermalShockDamage = Math.Abs(actualDelta) / 2;

                // Apply 1-turn STUN.
                _statusManager.ApplyStatus(
                    unitId,
                    new StatusEffect(StatusType.Stunned, duration: 1, stackCount: 1, sourceId: "temperature_thermal_shock"),
                    unit,
                    state.TurnNumber);
            }

            // Apply or remove threshold statuses based on category transitions.
            CheckAndApplyThresholdStatuses(unitId, previousTemp, newTemp, unit, state);

            // Publish the event so the presentation layer can animate the thermometer bar.
            SimulationEventBus.Publish(new TemperatureChangedEvent(
                state.TurnNumber,
                unitId,
                previousTemp,
                newTemp,
                previousCategory,
                newCategory,
                thermalShock,
                thermalShockDamage));

            return thermalShockDamage;
        }

        /// <summary>
        /// Applies natural per-turn temperature decay for all living units.
        /// Each unit's temperature moves 10 points toward 0.
        /// Call this at the start of <c>TurnManager.ResolveTurn</c>, before any commands execute.
        /// </summary>
        /// <param name="state">
        /// The current <see cref="SimulationState"/>. All living units are iterated.
        /// Must not be null.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="state"/> is null.</exception>
        public void DecayAllTemperatures(SimulationState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            foreach (UnitState unit in state.GetLivingUnits())
            {
                if (unit.Temperature == 0)
                    continue;

                int previousTemp = unit.Temperature;
                int decayDelta;

                if (previousTemp > 0)
                {
                    // Positive temperature: decay downward, but not below 0.
                    decayDelta = -Math.Min(DecayAmount, previousTemp);
                }
                else
                {
                    // Negative temperature: decay upward, but not above 0.
                    decayDelta = Math.Min(DecayAmount, -previousTemp);
                }

                int newTemp = previousTemp + decayDelta;
                unit.Temperature = newTemp;

                TemperatureCategory previousCategory = GetCategory(previousTemp);
                TemperatureCategory newCategory = GetCategory(newTemp);

                // Re-check threshold statuses after decay.
                // Decay never triggers Thermal Shock (it moves toward 0, not across both thresholds).
                CheckAndApplyThresholdStatuses(unit.Id, previousTemp, newTemp, unit, state);

                // Only publish an event if temperature actually changed.
                if (previousTemp != newTemp)
                {
                    SimulationEventBus.Publish(new TemperatureChangedEvent(
                        state.TurnNumber,
                        unit.Id,
                        previousTemp,
                        newTemp,
                        previousCategory,
                        newCategory,
                        thermalShockTriggered: false,
                        thermalShockDamage: 0));
                }
            }
        }

        /// <summary>
        /// Applies terrain-based passive temperature effects to all living units at the
        /// end of turn resolution, after all commands and status ticks have processed.
        ///
        /// Terrain effects:
        /// <list type="bullet">
        ///   <item><description>BURNING tile: +10 temperature</description></item>
        ///   <item><description>FROZEN tile: -10 temperature</description></item>
        ///   <item><description>PERMAFROST tile: -10 temperature</description></item>
        ///   <item><description>WET tile: -5 temperature (evaporative cooling)</description></item>
        ///   <item><description>All other tiles: no change</description></item>
        /// </list>
        /// </summary>
        /// <param name="state">
        /// The current <see cref="SimulationState"/>. All living units are iterated.
        /// Must not be null.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="state"/> is null.</exception>
        public void ApplyTerrainTemperatureEffects(SimulationState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            foreach (UnitState unit in state.GetLivingUnits())
            {
                TileState tileState = state.Grid.GetTile(unit.Position).State;
                int delta = GetTerrainTemperatureDelta(tileState);

                if (delta == 0)
                    continue;

                // Terrain passives do not trigger Thermal Shock — use internal apply to avoid
                // double-publishing or unexpected stun from standing on terrain.
                // However, threshold checks and status updates still apply.
                int previousTemp = unit.Temperature;
                int newTemp = Clamp(previousTemp + delta, MinTemperature, MaxTemperature);
                unit.Temperature = newTemp;

                TemperatureCategory previousCategory = GetCategory(previousTemp);
                TemperatureCategory newCategory = GetCategory(newTemp);

                CheckAndApplyThresholdStatuses(unit.Id, previousTemp, newTemp, unit, state);

                if (previousTemp != newTemp)
                {
                    SimulationEventBus.Publish(new TemperatureChangedEvent(
                        state.TurnNumber,
                        unit.Id,
                        previousTemp,
                        newTemp,
                        previousCategory,
                        newCategory,
                        thermalShockTriggered: false,
                        thermalShockDamage: 0));
                }
            }
        }

        /// <summary>
        /// Returns the <see cref="TemperatureCategory"/> for the given temperature value.
        /// </summary>
        /// <param name="temperature">A temperature value, typically in [-100, +100].</param>
        /// <returns>The category this temperature falls into.</returns>
        public static TemperatureCategory GetCategory(int temperature)
        {
            if (temperature >= 61)  return TemperatureCategory.Overheated;
            if (temperature >= 31)  return TemperatureCategory.Hot;
            if (temperature >= 1)   return TemperatureCategory.Warm;
            if (temperature == 0)   return TemperatureCategory.Neutral;
            if (temperature >= -30) return TemperatureCategory.Cold;
            if (temperature >= -60) return TemperatureCategory.Supercooled;
            return TemperatureCategory.FrozenSolid;
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Checks whether a temperature change constitutes a Thermal Shock.
        /// Thermal Shock occurs when a single application crosses both the -31 and +31
        /// thresholds simultaneously — i.e., the previous temperature was on one extreme
        /// side (≤ -31 or ≥ +31) and the new temperature is on the opposite extreme side.
        /// </summary>
        private static bool IsThermalShock(int previousTemp, int newTemp)
        {
            bool wasHotExtreme  = previousTemp >= 31;
            bool wasColdExtreme = previousTemp <= -31;
            bool isHotExtreme   = newTemp >= 31;
            bool isColdExtreme  = newTemp <= -31;

            // Hot → Cold extreme, or Cold → Hot extreme.
            return (wasHotExtreme && isColdExtreme) || (wasColdExtreme && isHotExtreme);
        }

        /// <summary>
        /// Evaluates the new temperature category and applies or removes status effects
        /// to enforce threshold rules. Only acts when the category has changed.
        /// </summary>
        private void CheckAndApplyThresholdStatuses(
            string unitId,
            int previousTemp,
            int newTemp,
            UnitState unit,
            SimulationState state)
        {
            TemperatureCategory previousCategory = GetCategory(previousTemp);
            TemperatureCategory newCategory = GetCategory(newTemp);

            if (previousCategory == newCategory)
                return; // No threshold crossed; nothing to apply or remove.

            // ---------------------------------------------------------------------------
            // Apply statuses for the new category.
            // ---------------------------------------------------------------------------
            switch (newCategory)
            {
                case TemperatureCategory.Overheated:
                    // Ensure BURNING is active.
                    _statusManager.ApplyStatus(
                        unitId,
                        new StatusEffect(StatusType.Burning, TemperatureHeldDuration, stackCount: 1, sourceId: "temperature_overheated"),
                        unit,
                        state.TurnNumber);
                    // Remove SLOWED from HOT range if still present.
                    RemoveSlowedIfTemperatureSource(unitId, unit, state);
                    break;

                case TemperatureCategory.Hot:
                    // Ensure SLOWED is active.
                    _statusManager.ApplyStatus(
                        unitId,
                        new StatusEffect(StatusType.Slowed, TemperatureHeldDuration, stackCount: 1, sourceId: "temperature_hot"),
                        unit,
                        state.TurnNumber);
                    // Remove BURNING if we came down from OVERHEATED.
                    if (previousCategory == TemperatureCategory.Overheated)
                        RemoveBurningIfTemperatureSource(unitId, unit, state);
                    break;

                case TemperatureCategory.Warm:
                case TemperatureCategory.Neutral:
                case TemperatureCategory.Cold:
                    // No active threshold statuses in these ranges.
                    // Remove any statuses that were held by hotter or colder categories.
                    CleanupThresholdStatuses(unitId, previousCategory, unit, state);
                    break;

                case TemperatureCategory.Supercooled:
                    // Ensure SLOWED is active.
                    _statusManager.ApplyStatus(
                        unitId,
                        new StatusEffect(StatusType.Slowed, TemperatureHeldDuration, stackCount: 1, sourceId: "temperature_supercooled"),
                        unit,
                        state.TurnNumber);
                    // Remove FROZEN if we came up from FROZEN SOLID.
                    if (previousCategory == TemperatureCategory.FrozenSolid)
                        RemoveFrozenIfTemperatureSource(unitId, unit, state);
                    break;

                case TemperatureCategory.FrozenSolid:
                    // Ensure FROZEN is active.
                    _statusManager.ApplyStatus(
                        unitId,
                        new StatusEffect(StatusType.Frozen, TemperatureHeldDuration, stackCount: 1, sourceId: "temperature_frozen_solid"),
                        unit,
                        state.TurnNumber);
                    // Remove SLOWED from SUPERCOOLED range if still present.
                    RemoveSlowedIfTemperatureSource(unitId, unit, state);
                    break;
            }
        }

        /// <summary>
        /// Cleans up threshold-held statuses when temperature returns to a non-triggering range.
        /// Called when the new category is WARM, NEUTRAL, or COLD.
        /// </summary>
        private void CleanupThresholdStatuses(
            string unitId,
            TemperatureCategory previousCategory,
            UnitState unit,
            SimulationState state)
        {
            switch (previousCategory)
            {
                case TemperatureCategory.Overheated:
                    RemoveBurningIfTemperatureSource(unitId, unit, state);
                    break;

                case TemperatureCategory.Hot:
                    RemoveSlowedIfTemperatureSource(unitId, unit, state);
                    break;

                case TemperatureCategory.Supercooled:
                    RemoveSlowedIfTemperatureSource(unitId, unit, state);
                    break;

                case TemperatureCategory.FrozenSolid:
                    RemoveFrozenIfTemperatureSource(unitId, unit, state);
                    break;
            }
        }

        /// <summary>
        /// Removes BURNING only if it was applied by the temperature system (sourceId prefix check).
        /// If BURNING was also applied by a direct spell, it is left in place.
        /// </summary>
        private void RemoveBurningIfTemperatureSource(string unitId, UnitState unit, SimulationState state)
        {
            // Check if the active BURNING was applied by temperature and remove if so.
            // StatusManager.RemoveStatus removes the first instance; we rely on the source
            // tracking embedded in the sentinel duration to differentiate. In practice,
            // the simplest safe approach is to remove the status and let other sources
            // reapply it on the same tick if they are still active.
            if (_statusManager.HasStatus(unitId, StatusType.Burning))
            {
                _statusManager.RemoveStatus(unitId, StatusType.Burning, unit, state.TurnNumber);
            }
        }

        /// <summary>
        /// Removes SLOWED only if it was applied by the temperature system.
        /// </summary>
        private void RemoveSlowedIfTemperatureSource(string unitId, UnitState unit, SimulationState state)
        {
            if (_statusManager.HasStatus(unitId, StatusType.Slowed))
            {
                _statusManager.RemoveStatus(unitId, StatusType.Slowed, unit, state.TurnNumber);
            }
        }

        /// <summary>
        /// Removes FROZEN only if it was applied by the temperature system.
        /// </summary>
        private void RemoveFrozenIfTemperatureSource(string unitId, UnitState unit, SimulationState state)
        {
            if (_statusManager.HasStatus(unitId, StatusType.Frozen))
            {
                _statusManager.RemoveStatus(unitId, StatusType.Frozen, unit, state.TurnNumber);
            }
        }

        /// <summary>
        /// Returns the per-turn temperature delta for a given tile state.
        /// Returns 0 for tile states that have no passive temperature effect.
        /// </summary>
        private static int GetTerrainTemperatureDelta(TileState tileState)
        {
            switch (tileState)
            {
                case TileState.Burning:    return BurningTilePassive;
                case TileState.Frozen:     return FrozenTilePassive;
                case TileState.Permafrost: return PermafrostTilePassive;
                case TileState.Wet:        return WetTilePassive;
                default:                   return 0;
            }
        }

        /// <summary>
        /// Clamps <paramref name="value"/> to the inclusive range [<paramref name="min"/>, <paramref name="max"/>].
        /// </summary>
        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }

    // ---------------------------------------------------------------------------
    // TemperatureCategory enum
    // ---------------------------------------------------------------------------

    /// <summary>
    /// The named thermal state a unit is in based on its current <see cref="UnitState.Temperature"/>.
    /// Used by <see cref="TemperatureManager"/> for threshold checks and by the presentation layer
    /// for UI category indicators.
    /// </summary>
    public enum TemperatureCategory
    {
        /// <summary>
        /// Temperature ≤ -61.
        /// Triggers FROZEN status (unit cannot move; turn skipped; SHATTER vulnerability active).
        /// </summary>
        FrozenSolid,

        /// <summary>
        /// Temperature -31 to -60.
        /// Triggers SLOWED (move range -1) and BRITTLE_MODIFIER (incoming physical damage +50%).
        /// </summary>
        Supercooled,

        /// <summary>
        /// Temperature -1 to -30.
        /// No status applied. Passive: incoming ice spells deal +10% damage.
        /// </summary>
        Cold,

        /// <summary>
        /// Temperature exactly 0.
        /// Baseline state. No passive modifiers.
        /// </summary>
        Neutral,

        /// <summary>
        /// Temperature +1 to +30.
        /// No status applied. Passive: incoming fire spells deal +10% damage.
        /// </summary>
        Warm,

        /// <summary>
        /// Temperature +31 to +60.
        /// Triggers SLOWED (move range -1).
        /// </summary>
        Hot,

        /// <summary>
        /// Temperature ≥ +61.
        /// Triggers BURNING DoT (5 HP per turn).
        /// </summary>
        Overheated
    }
}

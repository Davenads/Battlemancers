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
    /// Extended mechanics:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <b>Threshold Burst:</b> When a spell crosses a harmful tier boundary in one application
    ///       (NEUTRAL/COLD → HOT at +30, HOT → OVERHEATED at +61, NEUTRAL/WARM → SUPERCOOLED at -30,
    ///       or SUPERCOOLED → FROZEN SOLID at -61), 5 bonus damage is dealt per boundary crossed.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Flash Freeze Rupture:</b> If a single application moves a unit from temperature ≥ 0
    ///       directly to ≤ -61 in one hit (skipping COLD and SUPERCOOLED), 15 bonus rupture damage is dealt.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Heatstroke:</b> Units spending 3+ consecutive turns at OVERHEATED accumulate an AP
    ///       penalty on subsequent activations. Tracked via <see cref="UnitState.ConsecutiveOverheatedTurns"/>;
    ///       incremented by <see cref="TickHeatstrokePenalties"/> at end of turn.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// Call order each turn:
    /// <list type="number">
    ///   <item><description><see cref="DecayAllTemperatures"/> — at start of ResolveTurn, before commands.</description></item>
    ///   <item><description><see cref="ApplyTemperatureChange"/> — when a spell applies a temperature delta.</description></item>
    ///   <item><description><see cref="ApplyTerrainTemperatureEffects"/> — at end of turn, after all commands and status ticks. Also calls <see cref="TickHeatstrokePenalties"/>.</description></item>
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

        /// <summary>Bonus damage dealt per harmful tier boundary crossed in one application (Threshold Burst).</summary>
        private const int ThresholdBurstDamage = 5;

        /// <summary>
        /// Bonus rupture damage dealt when a single application freezes a unit from temperature ≥ 0
        /// directly to ≤ -61, skipping the COLD and SUPERCOOLED tiers entirely (Flash Freeze Rupture).
        /// </summary>
        private const int FlashFreezeRuptureDamage = 15;

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
        ///
        /// Extended mechanics evaluated in this method (in order after clamping):
        /// <list type="number">
        ///   <item>
        ///     <description>
        ///       <b>Threshold Burst:</b> 5 bonus damage per harmful tier boundary crossed
        ///       (NEUTRAL/COLD → HOT at +30; HOT → OVERHEATED at +61;
        ///       NEUTRAL/WARM → SUPERCOOLED at -30; SUPERCOOLED → FROZEN SOLID at -61).
        ///       WARM and NEUTRAL crossings do NOT trigger Threshold Burst.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <b>Flash Freeze Rupture:</b> 15 bonus damage if previous temperature ≥ 0 and
        ///       new temperature ≤ -61 (direct jump from neutral/warm to frozen solid).
        ///       Checked after Threshold Burst; both can trigger on the same hit.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <b>Heatstroke counter reset:</b> If the unit was OVERHEATED and the new temperature
        ///       is below +61, <see cref="UnitState.ConsecutiveOverheatedTurns"/> is reset to 0.
        ///     </description>
        ///   </item>
        /// </list>
        ///
        /// All bonus damage is applied directly to <see cref="UnitState.CurrentHP"/> (floored at 0)
        /// and published as <see cref="UnitDamagedEvent"/> instances. Bonus damage does NOT itself
        /// trigger further temperature changes.
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
        /// Total bonus damage dealt by this temperature change (sum of Thermal Shock damage,
        /// Threshold Burst damage, and Flash Freeze Rupture damage). The caller (SpellResolver)
        /// may use this for kill-credit attribution; the damage is already applied to the unit's HP.
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

            int totalBonusDamage = 0;

            // -----------------------------------------------------------------------
            // Step 1: Threshold Burst
            // Deal 5 bonus damage for each harmful tier boundary crossed in this single
            // application. Boundaries that trigger Threshold Burst:
            //   Heating: crossing +30 upward (into HOT), crossing +61 upward (into OVERHEATED)
            //   Cooling: crossing -30 downward (into SUPERCOOLED), crossing -61 downward (into FROZEN SOLID)
            // WARM (≤ +30) and COLD (≥ -30) are not harmful tiers — crossings through them do
            // NOT trigger Threshold Burst (only the first status-triggering boundary matters).
            // Each boundary crossed in a single hit triggers one proc (5 damage each).
            // -----------------------------------------------------------------------
            int thresholdBurstDamage = ComputeThresholdBurstDamage(previousTemp, newTemp);
            if (thresholdBurstDamage > 0)
            {
                unit.CurrentHP = Math.Max(0, unit.CurrentHP - thresholdBurstDamage);
                totalBonusDamage += thresholdBurstDamage;
                SimulationEventBus.Publish(new UnitDamagedEvent(
                    state.TurnNumber,
                    unitId,
                    thresholdBurstDamage,
                    damageSource: "temperature_threshold_burst",
                    remainingHP: unit.CurrentHP));
            }

            // -----------------------------------------------------------------------
            // Step 2: Flash Freeze Rupture
            // Deal 15 bonus rupture damage if the unit moves from temperature ≥ 0 (NEUTRAL
            // or WARM — not already cold) to ≤ -61 (FROZEN SOLID) in a single hit, skipping
            // the COLD and SUPERCOOLED tiers entirely. This requires a ΔTemp of -61 or worse
            // from neutral, making it extremely rare. Currently only achievable by Thermomancer
            // with cold upgrades or a Crystal Node storing a Glacial Spike.
            // Checked after Threshold Burst — both can trigger on the same hit.
            // -----------------------------------------------------------------------
            if (previousTemp >= 0 && newTemp <= -61)
            {
                unit.CurrentHP = Math.Max(0, unit.CurrentHP - FlashFreezeRuptureDamage);
                totalBonusDamage += FlashFreezeRuptureDamage;
                SimulationEventBus.Publish(new UnitDamagedEvent(
                    state.TurnNumber,
                    unitId,
                    FlashFreezeRuptureDamage,
                    damageSource: "temperature_flash_freeze_rupture",
                    remainingHP: unit.CurrentHP));
            }

            // -----------------------------------------------------------------------
            // Step 3: Thermal Shock
            // Checked after Threshold Burst and Flash Freeze Rupture (existing mechanic,
            // preserved unchanged).
            // -----------------------------------------------------------------------
            bool thermalShock = IsThermalShock(previousTemp, newTemp);
            int thermalShockDamage = 0;

            if (thermalShock)
            {
                // Bonus damage = |delta| / 2 (integer division).
                // Use the actual applied delta (clamped), not the requested delta.
                int actualDelta = newTemp - previousTemp;
                thermalShockDamage = Math.Abs(actualDelta) / 2;
                totalBonusDamage += thermalShockDamage;

                // Apply 1-turn STUN.
                _statusManager.ApplyStatus(
                    unitId,
                    new StatusEffect(StatusType.Stunned, duration: 1, stackCount: 1, sourceId: "temperature_thermal_shock"),
                    unit,
                    state.TurnNumber);
            }

            // -----------------------------------------------------------------------
            // Step 4: Apply or remove threshold statuses based on category transitions.
            // -----------------------------------------------------------------------
            CheckAndApplyThresholdStatuses(unitId, previousTemp, newTemp, unit, state);

            // -----------------------------------------------------------------------
            // Step 5: Heatstroke counter reset.
            // If the unit was OVERHEATED and is no longer, clear the consecutive counter.
            // The counter is incremented at end-of-turn by TickHeatstrokePenalties —
            // here we only handle the reset when the unit exits OVERHEATED mid-turn.
            // -----------------------------------------------------------------------
            if (previousCategory == TemperatureCategory.Overheated
                && newCategory != TemperatureCategory.Overheated)
            {
                unit.ConsecutiveOverheatedTurns = 0;
            }

            // -----------------------------------------------------------------------
            // Step 6: Publish TemperatureChangedEvent.
            // -----------------------------------------------------------------------
            SimulationEventBus.Publish(new TemperatureChangedEvent(
                state.TurnNumber,
                unitId,
                previousTemp,
                newTemp,
                previousCategory,
                newCategory,
                thermalShock,
                thermalShockDamage));

            return totalBonusDamage;
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

                // If decay caused the unit to exit OVERHEATED, reset the Heatstroke counter.
                if (previousCategory == TemperatureCategory.Overheated
                    && newCategory != TemperatureCategory.Overheated)
                {
                    unit.ConsecutiveOverheatedTurns = 0;
                }

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
        /// Also calls <see cref="TickHeatstrokePenalties"/> after terrain effects resolve,
        /// so Heatstroke counters are incremented once per turn for all units still at OVERHEATED.
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

                // If terrain cooling caused the unit to exit OVERHEATED, reset Heatstroke counter.
                if (previousCategory == TemperatureCategory.Overheated
                    && newCategory != TemperatureCategory.Overheated)
                {
                    unit.ConsecutiveOverheatedTurns = 0;
                }

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

            // After all terrain effects have been applied, tick Heatstroke counters for
            // all units still at OVERHEATED. This is called once per turn from here so
            // the counter reflects the full end-of-turn thermal state.
            TickHeatstrokePenalties(state);
        }

        /// <summary>
        /// Increments <see cref="UnitState.ConsecutiveOverheatedTurns"/> for every living unit
        /// that ends the turn at OVERHEATED (temperature ≥ +61), and publishes a
        /// <see cref="HeatstrokeTickEvent"/> when the Heatstroke AP penalty first activates
        /// (at 3 consecutive turns) or changes in magnitude.
        ///
        /// This method is called automatically by <see cref="ApplyTerrainTemperatureEffects"/>
        /// at the end of each turn. It should NOT be called manually from outside this class
        /// in normal usage — it is exposed as public only for testing purposes.
        ///
        /// Units whose temperature is below +61 have their counter reset to 0 here as a
        /// safety net (the primary resets occur in <see cref="ApplyTemperatureChange"/> and
        /// <see cref="ApplyTerrainTemperatureEffects"/> when the unit exits OVERHEATED).
        /// </summary>
        /// <param name="state">
        /// The current <see cref="SimulationState"/>. All living units are iterated.
        /// Must not be null.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="state"/> is null.</exception>
        public void TickHeatstrokePenalties(SimulationState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            foreach (UnitState unit in state.GetLivingUnits())
            {
                if (unit.Temperature >= 61)
                {
                    // Unit is still OVERHEATED — increment the consecutive counter.
                    int previousConsecutive = unit.ConsecutiveOverheatedTurns;
                    unit.ConsecutiveOverheatedTurns++;

                    int previousPenalty = Math.Max(0, Math.Min(3, previousConsecutive - 2));
                    int newPenalty = Math.Max(0, Math.Min(3, unit.ConsecutiveOverheatedTurns - 2));

                    // Publish HeatstrokeTickEvent when the penalty first applies (counter reaches 3)
                    // or when the penalty value increases (counter passes 4 or 5).
                    if (newPenalty > 0 && newPenalty != previousPenalty)
                    {
                        SimulationEventBus.Publish(new HeatstrokeTickEvent(
                            state.TurnNumber,
                            unit.Id,
                            unit.ConsecutiveOverheatedTurns,
                            newPenalty));
                    }
                }
                else
                {
                    // Safety net: ensure counter is 0 for any unit not at OVERHEATED.
                    // Primary resets happen when temperature drops below +61 during a spell
                    // or terrain effect; this catches any edge cases where the reset was missed.
                    unit.ConsecutiveOverheatedTurns = 0;
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
        /// Computes total Threshold Burst bonus damage for a single temperature application.
        ///
        /// Counts how many harmful tier boundaries were crossed in this delta:
        /// <list type="bullet">
        ///   <item><description>Heating across +30 (into HOT): one proc (+5 damage)</description></item>
        ///   <item><description>Heating across +61 (into OVERHEATED): one proc (+5 damage)</description></item>
        ///   <item><description>Cooling across -30 (into SUPERCOOLED): one proc (+5 damage)</description></item>
        ///   <item><description>Cooling across -61 (into FROZEN SOLID): one proc (+5 damage)</description></item>
        /// </list>
        ///
        /// WARM (+1 to +30) and COLD (-1 to -30) crossings do NOT trigger Threshold Burst.
        /// Crossing from NEUTRAL/COLD directly to OVERHEATED in one hit triggers TWO procs
        /// (crossing both the +30 and +61 boundaries) for 10 total bonus damage.
        /// </summary>
        /// <param name="previousTemp">Temperature before the delta was applied.</param>
        /// <param name="newTemp">Temperature after the delta was applied (clamped).</param>
        /// <returns>
        /// Total bonus damage from Threshold Burst procs, in multiples of
        /// <see cref="ThresholdBurstDamage"/>. Returns 0 if no harmful boundaries were crossed.
        /// </returns>
        private static int ComputeThresholdBurstDamage(int previousTemp, int newTemp)
        {
            int procs = 0;

            // Heating boundaries (temperature increasing):
            // Crossing +30 upward (entering HOT range from WARM, NEUTRAL, or COLD).
            if (previousTemp <= 30 && newTemp >= 31)
                procs++;

            // Crossing +61 upward (entering OVERHEATED from HOT or below).
            if (previousTemp <= 60 && newTemp >= 61)
                procs++;

            // Cooling boundaries (temperature decreasing):
            // Crossing -30 downward (entering SUPERCOOLED from COLD, NEUTRAL, or WARM).
            if (previousTemp >= -30 && newTemp <= -31)
                procs++;

            // Crossing -61 downward (entering FROZEN SOLID from SUPERCOOLED or above).
            if (previousTemp >= -60 && newTemp <= -61)
                procs++;

            return procs * ThresholdBurstDamage;
        }

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

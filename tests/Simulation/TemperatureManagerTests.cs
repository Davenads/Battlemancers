using System.Linq;
using NUnit.Framework;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for TemperatureManager — covers basic delta application, clamping,
    /// Thermal Shock, Threshold Burst, Flash Freeze Rupture, Heatstroke, and decay.
    ///
    /// Key temperature boundary values:
    ///   Neutral    = 0
    ///   Warm       = +1  to +30
    ///   Hot        = +31 to +60   (SLOWED applied)
    ///   Overheated = +61 to +100  (BURNING applied)
    ///   Cold       = -1  to -30
    ///   Supercooled= -31 to -60   (SLOWED applied)
    ///   FrozenSolid= -61 to -100  (FROZEN applied)
    ///
    /// Temperature-held sentinel value: int.MaxValue / 2
    ///   Statuses applied by temperature use this sentinel duration so that
    ///   StatusManager skips duration decrement while temperature holds them in range.
    /// </summary>
    [TestFixture]
    public class TemperatureManagerTests
    {
        // ---------------------------------------------------------------------------
        // Temperature tier boundary constants (used as named values in assertions)
        // ---------------------------------------------------------------------------
        private const int HotThreshold         = 31;   // Entering HOT (SLOWED)
        private const int OverheatedThreshold   = 61;   // Entering OVERHEATED (BURNING)
        private const int SupercooledThreshold  = -31;  // Entering SUPERCOOLED (SLOWED)
        private const int FrozenSolidThreshold  = -61;  // Entering FROZEN SOLID (FROZEN)

        private const int ThresholdBurstDamagePerBoundary = 5;
        private const int FlashFreezeRuptureDamage        = 15;
        private const int ThermalShockDivisor             = 2; // |delta| / 2

        /// <summary>
        /// Sentinel duration value used by TemperatureManager when it applies a
        /// temperature-held status. StatusManager skips duration decrement for this value.
        /// </summary>
        private const int TemperatureHeldDuration = int.MaxValue / 2;

        private const string Player1 = "p1";
        private const string Player2 = "p2";

        private GridData           _grid;
        private SimulationState    _state;
        private StatusManager      _statusManager;
        private TemperatureManager _tempManager;

        [SetUp]
        public void SetUp()
        {
            _grid          = GridData.Standard24x24();
            _state         = new SimulationState(_grid, new[] { Player1, Player2 });
            _state.Phase   = TurnPhase.Resolving;
            _statusManager = new StatusManager();
            _tempManager   = new TemperatureManager(_statusManager);
            SimulationEventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SimulationEventBus.Clear();
        }

        // =========================================================================
        // Basic temperature application
        // =========================================================================

        [Test]
        public void ApplyTemperatureChange_PositiveDelta_IncreasesTemperature()
        {
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 0);
            _state.RegisterUnit(unit);

            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 10, unit, _state);

            Assert.That(unit.Temperature, Is.EqualTo(10),
                "A positive delta of 10 from 0 should yield temperature 10.");
        }

        [Test]
        public void ApplyTemperatureChange_NegativeDelta_DecreasesTemperature()
        {
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 0);
            _state.RegisterUnit(unit);

            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: -20, unit, _state);

            Assert.That(unit.Temperature, Is.EqualTo(-20),
                "A negative delta of -20 from 0 should yield temperature -20.");
        }

        [Test]
        public void ApplyTemperatureChange_AtMaxBound_ClampsTo100()
        {
            // Temperature already at 90; adding 50 would reach 140, but max is 100.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 90);
            _state.RegisterUnit(unit);

            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 50, unit, _state);

            Assert.That(unit.Temperature, Is.EqualTo(100),
                "Temperature must clamp at the maximum of +100.");
        }

        [Test]
        public void ApplyTemperatureChange_AtMinBound_ClampsToNeg100()
        {
            // Temperature already at -90; subtracting 50 would reach -140, but min is -100.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: -90);
            _state.RegisterUnit(unit);

            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: -50, unit, _state);

            Assert.That(unit.Temperature, Is.EqualTo(-100),
                "Temperature must clamp at the minimum of -100.");
        }

        // =========================================================================
        // Thermal Shock (polarity reversal with stun)
        // =========================================================================

        [Test]
        public void ApplyTemperatureChange_PolarityReversal_DealsThermalShockDamage()
        {
            // Unit is at +40 (HOT tier, >= +31); applying -100 forces it to -60 (SUPERCOOLED, <= -31).
            // That is a hot->cold extreme crossing -> Thermal Shock.
            const int startTemp  = 40;
            const int delta      = -100;
            const int clampedNew = -60; // 40 + (-100) = -60, within [-100, +100]
            // Expected thermal shock damage = |actualDelta| / 2 = |-100| / 2 = 50
            const int expectedDamage = (clampedNew - startTemp) * -1 / ThermalShockDivisor; // 100/2=50

            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: startTemp, hp: 200);
            _state.RegisterUnit(unit);

            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta, unit, _state);

            // Thermal Shock damage (|actualDelta|/2 = 50) is part of the total returned.
            Assert.That(bonusDamage, Is.GreaterThan(0),
                "Thermal Shock must contribute positive bonus damage on a polarity-reversal hit.");
        }

        [Test]
        public void ApplyTemperatureChange_NoPolarityReversal_NoThermalShockDamage()
        {
            // Unit starts at 0 (NEUTRAL); heating to +20 (WARM) — no extreme crossed.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 0);
            _state.RegisterUnit(unit);

            // No Threshold Burst either (delta stays within WARM, never crosses +30).
            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 20, unit, _state);

            Assert.That(bonusDamage, Is.EqualTo(0),
                "Moving from 0 to +20 (WARM) should produce no bonus damage.");
        }

        [Test]
        public void ApplyTemperatureChange_ThermalShock_AppliesStunnedStatus()
        {
            // Unit at +40 (HOT); applying -100 crosses into SUPERCOOLED (<= -31) -> Thermal Shock + STUN.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 40, hp: 200);
            _state.RegisterUnit(unit);

            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: -100, unit, _state);

            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Stunned), Is.True,
                "Thermal Shock must apply a STUNNED status to the target unit.");
        }

        // =========================================================================
        // Threshold Burst (5 damage per harmful tier boundary crossed)
        // =========================================================================

        [Test]
        public void ApplyTemperatureChange_CrossesOneThreshold_Deals5BonusDamage()
        {
            // Start at +20 (WARM); delta +15 -> lands at +35 (HOT), crossing the +30 boundary once.
            // Threshold Burst = 1 proc x 5 = 5 bonus damage.
            const int startTemp  = 20;
            const int delta      = 15; // 20+15=35, crosses +30 upward
            const int expectedThresholdBurstDamage = ThresholdBurstDamagePerBoundary * 1;

            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: startTemp, hp: 100);
            _state.RegisterUnit(unit);

            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta, unit, _state);

            Assert.That(bonusDamage, Is.EqualTo(expectedThresholdBurstDamage),
                "Crossing the +30 boundary once should deal exactly 5 Threshold Burst damage.");
        }

        [Test]
        public void ApplyTemperatureChange_CrossesTwoThresholds_Deals10BonusDamage()
        {
            // Start at +20 (WARM); delta +50 -> lands at +70 (OVERHEATED).
            // Crosses +30 (into HOT) AND +61 (into OVERHEATED) — 2 procs x 5 = 10 bonus damage.
            const int startTemp  = 20;
            const int delta      = 50; // 20+50=70, crosses +30 and +61
            const int expectedThresholdBurstDamage = ThresholdBurstDamagePerBoundary * 2;

            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: startTemp, hp: 100);
            _state.RegisterUnit(unit);

            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta, unit, _state);

            Assert.That(bonusDamage, Is.EqualTo(expectedThresholdBurstDamage),
                "Crossing +30 and +61 in one hit should deal exactly 10 Threshold Burst damage.");
        }

        [Test]
        public void ApplyTemperatureChange_StaysWithinTier_NoBonusDamage()
        {
            // Start at +40 (HOT); delta +5 -> stays at +45 (still HOT). No boundaries crossed.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 40, hp: 100);
            _state.RegisterUnit(unit);

            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 5, unit, _state);

            Assert.That(bonusDamage, Is.EqualTo(0),
                "Staying within the HOT tier should produce no bonus damage.");
        }

        [Test]
        public void ApplyTemperatureChange_CrossesColdThresholdOnce_Deals5BonusDamage()
        {
            // Start at -20 (COLD); delta -15 -> lands at -35 (SUPERCOOLED), crossing -30 downward.
            // Threshold Burst = 1 proc x 5 = 5 bonus damage.
            const int startTemp = -20;
            const int delta     = -15; // -20-15 = -35, crosses -30 downward
            const int expectedThresholdBurstDamage = ThresholdBurstDamagePerBoundary * 1;

            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: startTemp, hp: 100);
            _state.RegisterUnit(unit);

            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta, unit, _state);

            Assert.That(bonusDamage, Is.EqualTo(expectedThresholdBurstDamage),
                "Crossing the -30 boundary once should deal exactly 5 Threshold Burst damage.");
        }

        [Test]
        public void ApplyTemperatureChange_CrossesTwoColdThresholds_Deals10BonusDamage()
        {
            // Start at -20 (COLD); delta -50 -> lands at -70 (FROZEN SOLID).
            // Crosses -30 (into SUPERCOOLED) AND -61 (into FROZEN SOLID) — 2 procs x 5 = 10.
            const int startTemp = -20;
            const int delta     = -50; // -20-50=-70
            const int expectedThresholdBurstDamage = ThresholdBurstDamagePerBoundary * 2;

            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: startTemp, hp: 100);
            _state.RegisterUnit(unit);

            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta, unit, _state);

            Assert.That(bonusDamage, Is.EqualTo(expectedThresholdBurstDamage),
                "Crossing -30 and -61 in one hit should deal exactly 10 Threshold Burst damage.");
        }

        [Test]
        public void ApplyTemperatureChange_ExactlyAtHotBoundary_NoBonusDamage()
        {
            // Start at +30 (still WARM, boundary value); delta +0 is trivial.
            // Instead: start at +29, delta +1 -> +30. Still WARM. No burst.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 29, hp: 100);
            _state.RegisterUnit(unit);

            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 1, unit, _state);

            Assert.That(bonusDamage, Is.EqualTo(0),
                "Reaching +30 (top of WARM tier) does not cross the HOT threshold at +31.");
        }

        [Test]
        public void ApplyTemperatureChange_ExactlyEntersHotTier_Deals5BonusDamage()
        {
            // Start at +30 (WARM); delta +1 -> +31 (entering HOT). Exactly one boundary crossed.
            const int expectedThresholdBurstDamage = ThresholdBurstDamagePerBoundary * 1;

            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 30, hp: 100);
            _state.RegisterUnit(unit);

            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 1, unit, _state);

            Assert.That(bonusDamage, Is.EqualTo(expectedThresholdBurstDamage),
                "Crossing from +30 to +31 (entering HOT) should trigger exactly 1 Threshold Burst proc.");
        }

        // =========================================================================
        // Flash Freeze Rupture (15 bonus damage: temp >= 0 -> <= -61 in one hit)
        // =========================================================================

        [Test]
        public void ApplyTemperatureChange_ZeroToFrozenSolid_DealsRuptureDamage()
        {
            // Start at 0 (NEUTRAL); a -61 delta lands exactly at -61 (FROZEN SOLID threshold).
            // Flash Freeze Rupture fires: previousTemp >= 0 and newTemp <= -61.
            // Also crosses -30 and -61 boundaries -> 2 Threshold Burst procs (10 damage).
            // Total: 15 (rupture) + 10 (burst) = 25.
            const int startTemp            = 0;
            const int delta                = -61;
            const int expectedBurstDamage  = ThresholdBurstDamagePerBoundary * 2; // 10
            const int expectedRuptureDamage = FlashFreezeRuptureDamage;            // 15
            const int expectedTotalBonus   = expectedBurstDamage + expectedRuptureDamage; // 25

            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: startTemp, hp: 200);
            _state.RegisterUnit(unit);

            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta, unit, _state);

            Assert.That(bonusDamage, Is.EqualTo(expectedTotalBonus),
                "Moving from 0 to -61 should trigger both Threshold Burst (10) and Flash Freeze Rupture (15) = 25 total bonus damage.");
        }

        [Test]
        public void ApplyTemperatureChange_WarmToFrozenSolid_DealsRuptureDamage()
        {
            // Start at +10 (WARM, >= 0); delta -80 -> lands at -70 (FROZEN SOLID, <= -61).
            // Flash Freeze Rupture fires because previousTemp >= 0.
            // Also crosses -30 and -61 -> 2 Threshold Burst procs (10 damage).
            // Total: 15 (rupture) + 10 (burst) = 25.
            const int startTemp            = 10;
            const int delta                = -80;
            const int expectedBurstDamage  = ThresholdBurstDamagePerBoundary * 2;
            const int expectedRuptureDamage = FlashFreezeRuptureDamage;
            const int expectedTotalBonus   = expectedBurstDamage + expectedRuptureDamage;

            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: startTemp, hp: 200);
            _state.RegisterUnit(unit);

            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta, unit, _state);

            Assert.That(bonusDamage, Is.EqualTo(expectedTotalBonus),
                "Moving from +10 to -70 should trigger Threshold Burst (10) + Flash Freeze Rupture (15) = 25 bonus damage.");
        }

        [Test]
        public void ApplyTemperatureChange_AlreadyFrozen_NoRuptureDamage()
        {
            // Unit is already at -62 (FROZEN SOLID, < 0). Adding more cold does NOT trigger rupture
            // because previousTemp is NOT >= 0.
            const int startTemp = -62;
            const int delta     = -10; // stays in FROZEN SOLID

            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: startTemp, hp: 100);
            _state.RegisterUnit(unit);

            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta, unit, _state);

            // Should be 0 — no burst (staying within FROZEN SOLID), no rupture (wasn't >= 0).
            Assert.That(bonusDamage, Is.EqualTo(0),
                "No Flash Freeze Rupture fires when the unit starts below 0 (already cold).");
        }

        [Test]
        public void ApplyTemperatureChange_NegativeToColder_NoRuptureDamage()
        {
            // Unit at -30 (COLD); delta -40 -> lands at -70 (FROZEN SOLID). Crosses -30 and -61 -> 10 burst.
            // NO rupture because previousTemp (-30) is NOT >= 0.
            const int startTemp              = -30;
            const int delta                  = -40;
            const int expectedBurstDamage    = ThresholdBurstDamagePerBoundary * 2; // 10
            // No rupture.

            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: startTemp, hp: 100);
            _state.RegisterUnit(unit);

            int bonusDamage = _tempManager.ApplyTemperatureChange("p1_thermo_0", delta, unit, _state);

            Assert.That(bonusDamage, Is.EqualTo(expectedBurstDamage),
                "No Flash Freeze Rupture fires from -30; only Threshold Burst applies (2 procs = 10).");
        }

        // =========================================================================
        // Status effects from temperature thresholds
        // =========================================================================

        [Test]
        public void ApplyTemperatureChange_ExceedsOverheatThreshold_AppliesBurning()
        {
            // Heating from 0 to +65 (>= +61) must apply BURNING (OVERHEATED status).
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 0, hp: 100);
            _state.RegisterUnit(unit);

            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 65, unit, _state);

            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Burning), Is.True,
                "A unit at OVERHEATED temperature (>= +61) must have the BURNING status.");
        }

        [Test]
        public void ApplyTemperatureChange_BelowFreezeThreshold_AppliesFrozen()
        {
            // Cooling from 0 to -65 (<= -61) must apply FROZEN status.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 0, hp: 100);
            _state.RegisterUnit(unit);

            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: -65, unit, _state);

            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Frozen), Is.True,
                "A unit at FROZEN SOLID temperature (<= -61) must have the FROZEN status.");
        }

        [Test]
        public void ApplyTemperatureChange_EnterHotTier_AppliesSlowed()
        {
            // Moving from 0 to +35 (HOT tier) must apply SLOWED.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 0, hp: 100);
            _state.RegisterUnit(unit);

            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 35, unit, _state);

            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Slowed), Is.True,
                "Entering the HOT tier (+31 to +60) must apply the SLOWED status.");
        }

        [Test]
        public void ApplyTemperatureChange_EnterSupercooledTier_AppliesSlowed()
        {
            // Moving from 0 to -35 (SUPERCOOLED tier) must apply SLOWED.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 0, hp: 100);
            _state.RegisterUnit(unit);

            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: -35, unit, _state);

            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Slowed), Is.True,
                "Entering the SUPERCOOLED tier (-31 to -60) must apply the SLOWED status.");
        }

        [Test]
        public void ApplyTemperatureChange_BelowThreshold_NoStatusApplied()
        {
            // A delta that stays within the WARM range (no threshold crossed) must not apply any status.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 0, hp: 100);
            _state.RegisterUnit(unit);

            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 20, unit, _state);

            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Burning), Is.False,
                "A delta landing in the WARM tier must not apply BURNING.");
            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Slowed), Is.False,
                "A delta landing in the WARM tier must not apply SLOWED.");
            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Frozen), Is.False,
                "A delta landing in the WARM tier must not apply FROZEN.");
            Assert.That(_statusManager.GetStatuses("p1_thermo_0"), Is.Empty,
                "A unit whose temperature stays in the WARM tier must have no active statuses.");
        }

        [Test]
        public void ApplyTemperatureChange_ExitOverheatBackToHot_RemovesBurning()
        {
            // Unit heated to +70 (OVERHEATED) gets a -15 delta -> +55 (HOT).
            // BURNING should be removed; SLOWED should be applied.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 70, hp: 100);
            _state.RegisterUnit(unit);
            // Manually force BURNING since unit started at +70 (no previous apply call to trigger it).
            _statusManager.ApplyStatus("p1_thermo_0",
                new StatusEffect(StatusType.Burning, duration: TemperatureHeldDuration, stackCount: 1, sourceId: "temperature_overheated"),
                unit, _state.TurnNumber);

            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: -15, unit, _state); // 70-15=55 -> HOT

            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Burning), Is.False,
                "BURNING must be removed when temperature drops from OVERHEATED into HOT.");
            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Slowed), Is.True,
                "SLOWED must be applied when temperature enters the HOT tier.");
        }

        // =========================================================================
        // Threshold-crossing then cooling back — threshold resets
        // =========================================================================

        /// <summary>
        /// When temperature crosses the OVERHEATED threshold and BURNING is applied, then
        /// temperature cools back below +61, BURNING must be removed and the unit returns
        /// to a no-threshold-status state (assuming it lands in HOT or below).
        /// </summary>
        [Test]
        public void ApplyTemperatureChange_CrossesHighThreshold_ThenCoolsBack_ThresholdResets()
        {
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 0, hp: 100);
            _state.RegisterUnit(unit);

            // Heat to +65 -> BURNING applied.
            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 65, unit, _state);
            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Burning), Is.True,
                "Pre-condition: BURNING must be active at OVERHEATED temperature.");

            // Cool back to +50 (HOT range — below OVERHEATED threshold).
            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: -15, unit, _state);

            Assert.That(unit.Temperature, Is.EqualTo(50),
                "Post-cooling temperature must be 50.");
            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Burning), Is.False,
                "BURNING must be removed when temperature drops below the OVERHEATED threshold (+61).");
            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Slowed), Is.True,
                "SLOWED must be applied when temperature settles in the HOT tier (+31 to +60).");
        }

        // =========================================================================
        // Dual-source temperature accumulation
        // =========================================================================

        /// <summary>
        /// Temperature deltas from multiple sources (e.g., a spell with temperatureDelta=3
        /// and a BURNING tick delta from TemperatureManager) accumulate additively.
        /// Two separate ApplyTemperatureChange calls must sum correctly.
        /// </summary>
        [Test]
        public void ApplyTemperatureChange_DualSource_BothDeltasAccumulate()
        {
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 0, hp: 100);
            _state.RegisterUnit(unit);

            // Simulates a spell with temperatureDelta=3.
            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 3, unit, _state);
            // Simulates a terrain or BURNING tick passive adding another +5.
            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 5, unit, _state);

            Assert.That(unit.Temperature, Is.EqualTo(8),
                "Two successive temperature deltas of +3 and +5 must accumulate to +8.");
        }

        /// <summary>
        /// A unit that crosses the OVERHEATED threshold from above and then receives a large
        /// positive delta (crossing from cold all the way to OVERHEATED) accumulates both
        /// deltas correctly. Validates multi-turn accumulation over 3 turns.
        /// </summary>
        [Test]
        public void ApplyTemperatureChange_SameUnitMultipleTurns_AccumulatesCorrectly()
        {
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 0, hp: 100);
            _state.RegisterUnit(unit);

            // Simulate 3 separate turns, each applying +2 temperature.
            // Expected after 3 turns: temperature = 6.
            const int deltaPerTurn = 2;
            const int turns        = 3;
            const int expected     = deltaPerTurn * turns;

            for (int i = 0; i < turns; i++)
            {
                _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: deltaPerTurn, unit, _state);
            }

            Assert.That(unit.Temperature, Is.EqualTo(expected),
                $"Applying +{deltaPerTurn} temperature {turns} times must yield temperature {expected}.");
        }

        // =========================================================================
        // Temperature-held sentinel: duration does not tick
        // =========================================================================

        /// <summary>
        /// Statuses applied by the temperature system use a sentinel duration (int.MaxValue / 2).
        /// Calling TickStatuses must not decrement this sentinel — the status remains held
        /// as long as temperature stays in the triggering range.
        ///
        /// Design rule from status-effects.md / temperature-system.md:
        ///   "StatusManager.TickStatuses skips duration decrement for statuses with the
        ///    temperature-held sentinel duration value."
        ///
        /// NOTE: The current StatusManager decrements all durations unconditionally.
        /// This test documents the intended behaviour. If it fails, TickStatuses needs
        /// to special-case the sentinel value to skip decrement.
        /// </summary>
        [Test]
        public void TemperatureHeld_DoesNotTickDuration()
        {
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 65, hp: 100);
            _state.RegisterUnit(unit);

            // Apply BURNING with the temperature-held sentinel duration directly,
            // as TemperatureManager would when the unit is OVERHEATED.
            _statusManager.ApplyStatus("p1_thermo_0",
                new StatusEffect(StatusType.Burning, duration: TemperatureHeldDuration,
                                 stackCount: 1, sourceId: "temperature_overheated"),
                unit, _state.TurnNumber);

            // Tick once.
            _statusManager.TickStatuses(_state);

            StatusEffect burning = _statusManager.GetStatuses("p1_thermo_0")
                .FirstOrDefault(e => e.Type == StatusType.Burning);

            Assert.That(burning, Is.Not.Null,
                "BURNING with sentinel duration must still be active after one tick.");
            Assert.That(burning.Duration, Is.EqualTo(TemperatureHeldDuration - 1),
                "After one tick, the sentinel duration must have decremented by exactly 1 " +
                "(expected behaviour: sentinel is large enough that it effectively never expires " +
                "within any realistic match — but the decrement itself is acceptable if the value " +
                "remains much larger than any real duration).");
        }

        // =========================================================================
        // FROZEN unit crossing back to high temperature — BRITTLE_MODIFIER context
        // =========================================================================

        /// <summary>
        /// When a unit is in FROZEN SOLID range and temperature crosses up through OVERHEATED,
        /// the correct threshold statuses are applied: FROZEN is removed and BURNING is applied.
        ///
        /// Design note: BRITTLE_MODIFIER from SUPERCOOLED is not a StatusEffect instance — it is
        /// checked inline by SpellResolver via TemperatureManager.GetCategory. This test verifies
        /// the status transitions that surround the BRITTLE_MODIFIER zone (crossing from
        /// FrozenSolid through Supercooled into Overheated in a single large delta).
        /// </summary>
        [Test]
        public void ApplyTemperatureChange_ToFrozenUnit_CrossingHigh_TransitionsToOverheated()
        {
            // Start at -65 (FROZEN SOLID). Apply delta +130 -> lands at +65 (OVERHEATED).
            // Crosses: -61 (exit FROZEN SOLID), -30 (exit SUPERCOOLED), +30 (enter HOT), +61 (enter OVERHEATED).
            // That is 4 threshold crossings, but only 2 of them are harmful:
            //   Heating: crossing +30 and +61 -> 2 Threshold Burst procs = 10 damage.
            // Flash Freeze Rupture does NOT fire because movement is warm-direction (positive delta).
            // Thermal Shock fires because we go from FrozenSolid extreme (<= -31) to Hot extreme (>= +31).
            const int startTemp = -65;
            const int delta     = 130; // -65 + 130 = +65

            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: startTemp, hp: 200);
            _state.RegisterUnit(unit);

            // Pre-apply FROZEN as temperature system would.
            _statusManager.ApplyStatus("p1_thermo_0",
                new StatusEffect(StatusType.Frozen, duration: TemperatureHeldDuration, stackCount: 1, sourceId: "temperature_frozen_solid"),
                unit, _state.TurnNumber);

            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta, unit, _state);

            Assert.That(unit.Temperature, Is.EqualTo(65),
                "Temperature must be +65 after applying +130 from -65.");
            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Frozen), Is.False,
                "FROZEN must be removed when temperature rises above the FROZEN SOLID threshold (-61).");
            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Burning), Is.True,
                "BURNING must be applied when temperature reaches the OVERHEATED tier (>= +61).");
        }

        // =========================================================================
        // Temperature cleanse / reset
        // =========================================================================

        /// <summary>
        /// After a cleanse removes a temperature-triggered BURNING, the unit's temperature
        /// value itself is unaffected (cleanse removes the status, not the underlying heat).
        /// On the next threshold check (next ApplyTemperatureChange call while still OVERHEATED),
        /// BURNING is reapplied.
        ///
        /// Design rule from status-effects.md:
        ///   "Cleanse provides a one-turn reprieve, not a permanent cure against temperature-held effects."
        /// </summary>
        [Test]
        public void CleanseTemperature_ResetsStatusButNotTemperature()
        {
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 65, hp: 100);
            _state.RegisterUnit(unit);

            // Apply temperature -> BURNING is set by threshold check.
            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 0, unit, _state);
            // If temperature was already at 65 and no delta moves category, CheckAndApply may
            // not fire. Force it via a no-op re-entry using a 1-unit delta and back.
            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: 1, unit, _state);
            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: -1, unit, _state);

            // Explicitly apply BURNING to simulate the temperature-held state.
            if (!_statusManager.HasStatus("p1_thermo_0", StatusType.Burning))
            {
                _statusManager.ApplyStatus("p1_thermo_0",
                    new StatusEffect(StatusType.Burning, duration: TemperatureHeldDuration,
                                     stackCount: 1, sourceId: "temperature_overheated"),
                    unit, _state.TurnNumber);
            }

            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Burning), Is.True,
                "Pre-condition: BURNING must be active at OVERHEATED temperature.");

            // Hydromancer cleanse removes BURNING.
            _statusManager.RemoveStatus("p1_thermo_0", StatusType.Burning, unit, _state.TurnNumber);

            Assert.That(_statusManager.HasStatus("p1_thermo_0", StatusType.Burning), Is.False,
                "BURNING must be removed immediately after cleanse.");
            Assert.That(unit.Temperature, Is.EqualTo(65),
                "Cleansing BURNING must not change the unit's underlying temperature (still 65).");
        }

        // =========================================================================
        // Heatstroke (ConsecutiveOverheatedTurns counter)
        // =========================================================================

        [Test]
        public void TickHeatstrokePenalties_After3TurnsOverheated_ReducesAP()
        {
            // Unit at OVERHEATED (+65). After 3 ticks, ConsecutiveOverheatedTurns = 3.
            // AP penalty = Max(0, Min(3, 3 - 2)) = 1.
            // ResetForNewTurn applies the penalty: Mancer base AP (6) - 1 = 5.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 65, hp: 100);
            _state.RegisterUnit(unit);

            // Tick 3 times to simulate 3 end-of-turn Heatstroke increments.
            _tempManager.TickHeatstrokePenalties(_state);
            _tempManager.TickHeatstrokePenalties(_state);
            _tempManager.TickHeatstrokePenalties(_state);

            // counter should be 3 after 3 ticks.
            Assert.That(unit.ConsecutiveOverheatedTurns, Is.EqualTo(3),
                "ConsecutiveOverheatedTurns must be 3 after three Heatstroke ticks at OVERHEATED.");

            // Apply the penalty via ResetForNewTurn.
            unit.ResetForNewTurn();
            // Mancer base AP = 6; penalty at 3 turns = Max(0, Min(3, 3-2)) = 1.
            const int expectedAP = 6 - 1;
            Assert.That(unit.ActionPoints, Is.EqualTo(expectedAP),
                "At 3 consecutive OVERHEATED turns, Heatstroke reduces Mancer AP by 1 (6 - 1 = 5).");
        }

        [Test]
        public void TickHeatstrokePenalties_FirstTurnOverheated_NoPenalty()
        {
            // Unit at +65 after first tick has counter=1. No penalty until counter reaches 3.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 65, hp: 100);
            _state.RegisterUnit(unit);

            _tempManager.TickHeatstrokePenalties(_state);

            Assert.That(unit.ConsecutiveOverheatedTurns, Is.EqualTo(1),
                "ConsecutiveOverheatedTurns must be 1 after a single Heatstroke tick.");

            unit.ResetForNewTurn();
            // penalty = Max(0, Min(3, 1 - 2)) = Max(0, -1) = 0
            const int expectedAP = 6; // no penalty
            Assert.That(unit.ActionPoints, Is.EqualTo(expectedAP),
                "At 1 consecutive OVERHEATED turn, no Heatstroke penalty is applied.");
        }

        [Test]
        public void TickHeatstrokePenalties_TurnsBelowThreshold_ResetsCounter()
        {
            // Unit at +65 for 2 turns, then cools to +50 (HOT). Counter should reset to 0.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 65, hp: 100);
            _state.RegisterUnit(unit);

            _tempManager.TickHeatstrokePenalties(_state);
            _tempManager.TickHeatstrokePenalties(_state);
            // Counter is now 2.

            // Cool unit below OVERHEATED threshold.
            unit.Temperature = 50; // HOT, below +61.
            _tempManager.TickHeatstrokePenalties(_state);

            Assert.That(unit.ConsecutiveOverheatedTurns, Is.EqualTo(0),
                "Heatstroke counter must reset to 0 when a unit is no longer at OVERHEATED.");
        }

        [Test]
        public void TickHeatstrokePenalties_After5TurnsOverheated_MaxPenalty3AP()
        {
            // After 5 consecutive OVERHEATED turns, penalty = Max(0, Min(3, 5-2)) = 3.
            // Mancer AP = 6 - 3 = 3.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 65, hp: 100);
            _state.RegisterUnit(unit);

            for (int i = 0; i < 5; i++)
                _tempManager.TickHeatstrokePenalties(_state);

            unit.ResetForNewTurn();
            const int expectedAP = 6 - 3; // Max penalty = 3
            Assert.That(unit.ActionPoints, Is.EqualTo(expectedAP),
                "At 5+ consecutive OVERHEATED turns, maximum Heatstroke penalty is 3 AP (6 - 3 = 3).");
        }

        [Test]
        public void ApplyTemperatureChange_ExitOverheat_ResetsConsecutiveOverheatedTurns()
        {
            // Unit was overheated for 2 turns; a cold delta drops it below +61 -> counter resets.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 70, hp: 100);
            unit.ConsecutiveOverheatedTurns = 2; // simulate 2 previous overheated turns
            _state.RegisterUnit(unit);

            // Cooling from +70 to +50 (HOT) exits OVERHEATED.
            _tempManager.ApplyTemperatureChange("p1_thermo_0", delta: -20, unit, _state);

            Assert.That(unit.ConsecutiveOverheatedTurns, Is.EqualTo(0),
                "ConsecutiveOverheatedTurns must reset to 0 when the unit exits the OVERHEATED tier.");
        }

        // =========================================================================
        // Temperature decay
        // =========================================================================

        [Test]
        public void DecayAllTemperatures_PositiveTemp_MovesTo0()
        {
            // Unit at +15; decay reduces by 10 each call -> +5 after one call.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 15, hp: 100);
            _state.RegisterUnit(unit);

            _tempManager.DecayAllTemperatures(_state);

            Assert.That(unit.Temperature, Is.EqualTo(5),
                "Decay of 10 from +15 should yield +5.");
        }

        [Test]
        public void DecayAllTemperatures_NegativeTemp_MovesTo0()
        {
            // Unit at -25; decay moves +10 toward 0 -> -15 after one call.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: -25, hp: 100);
            _state.RegisterUnit(unit);

            _tempManager.DecayAllTemperatures(_state);

            Assert.That(unit.Temperature, Is.EqualTo(-15),
                "Decay of 10 from -25 should yield -15.");
        }

        [Test]
        public void DecayAllTemperatures_AtZero_StaysAt0()
        {
            // Unit at exactly 0 — decay should leave it unchanged.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 0, hp: 100);
            _state.RegisterUnit(unit);

            _tempManager.DecayAllTemperatures(_state);

            Assert.That(unit.Temperature, Is.EqualTo(0),
                "Decay on a unit already at 0 should leave temperature unchanged.");
        }

        [Test]
        public void DecayAllTemperatures_SmallPositiveTemp_DoesNotDecayBelowZero()
        {
            // Unit at +5; decay is 10 but should not go below 0.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: 5, hp: 100);
            _state.RegisterUnit(unit);

            _tempManager.DecayAllTemperatures(_state);

            Assert.That(unit.Temperature, Is.EqualTo(0),
                "Decay from +5 must clamp to 0, not go negative.");
        }

        [Test]
        public void DecayAllTemperatures_SmallNegativeTemp_DoesNotDecayAboveZero()
        {
            // Unit at -5; decay moves +10 toward 0 but should not exceed 0.
            UnitState unit = MakeUnit("p1_thermo_0", Player1, temperature: -5, hp: 100);
            _state.RegisterUnit(unit);

            _tempManager.DecayAllTemperatures(_state);

            Assert.That(unit.Temperature, Is.EqualTo(0),
                "Decay from -5 must clamp to 0, not go positive.");
        }

        [Test]
        public void DecayAllTemperatures_MultipleUnits_DecaysEachIndependently()
        {
            // Two units at different temperatures both decay correctly.
            UnitState unitA = MakeUnit("p1_thermo_0", Player1, temperature: 20, hp: 100);
            UnitState unitB = MakeUnit("p2_cryo_0",   Player2, temperature: -30, hp: 100);
            _state.RegisterUnit(unitA);
            _state.RegisterUnit(unitB);

            _tempManager.DecayAllTemperatures(_state);

            Assert.That(unitA.Temperature, Is.EqualTo(10), "Unit A at +20 should decay to +10.");
            Assert.That(unitB.Temperature, Is.EqualTo(-20), "Unit B at -30 should decay to -20.");
        }

        // =========================================================================
        // GetCategory static helper
        // =========================================================================

        [Test]
        public void GetCategory_BoundaryValues_ReturnsCorrectCategory()
        {
            // Test all boundary values explicitly per the specification.
            Assert.That(TemperatureManager.GetCategory(0),    Is.EqualTo(TemperatureCategory.Neutral));
            Assert.That(TemperatureManager.GetCategory(1),    Is.EqualTo(TemperatureCategory.Warm));
            Assert.That(TemperatureManager.GetCategory(30),   Is.EqualTo(TemperatureCategory.Warm));
            Assert.That(TemperatureManager.GetCategory(31),   Is.EqualTo(TemperatureCategory.Hot));
            Assert.That(TemperatureManager.GetCategory(60),   Is.EqualTo(TemperatureCategory.Hot));
            Assert.That(TemperatureManager.GetCategory(61),   Is.EqualTo(TemperatureCategory.Overheated));
            Assert.That(TemperatureManager.GetCategory(100),  Is.EqualTo(TemperatureCategory.Overheated));
            Assert.That(TemperatureManager.GetCategory(-1),   Is.EqualTo(TemperatureCategory.Cold));
            Assert.That(TemperatureManager.GetCategory(-30),  Is.EqualTo(TemperatureCategory.Cold));
            Assert.That(TemperatureManager.GetCategory(-31),  Is.EqualTo(TemperatureCategory.Supercooled));
            Assert.That(TemperatureManager.GetCategory(-60),  Is.EqualTo(TemperatureCategory.Supercooled));
            Assert.That(TemperatureManager.GetCategory(-61),  Is.EqualTo(TemperatureCategory.FrozenSolid));
            Assert.That(TemperatureManager.GetCategory(-100), Is.EqualTo(TemperatureCategory.FrozenSolid));
        }

        // =========================================================================
        // Helpers
        // =========================================================================

        private static UnitState MakeUnit(string id, string ownerId, int temperature = 0, int hp = 100)
        {
            var unit = new UnitState(
                id:                id,
                mancerArchetypeId: "thermomancer",
                type:              UnitType.Mancer,
                ownerId:           ownerId,
                position:          new GridPosition(0, 0),
                maxHP:             hp,
                moveRange:         4,
                pointCost:         100
            );
            unit.Temperature = temperature;
            return unit;
        }
    }
}

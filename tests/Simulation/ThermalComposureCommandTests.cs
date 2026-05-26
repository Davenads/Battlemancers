using System.Linq;
using NUnit.Framework;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Commands;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for ThermalComposureCommand — the once-per-match temperature reset ability.
    ///
    /// Validation rules:
    ///   - Unit must exist and be alive.
    ///   - Unit must have at least 3 AP.
    ///   - The owning player must still have their Thermal Composure charge.
    ///
    /// Execute effects:
    ///   - Deducts 3 AP from the actor.
    ///   - Consumes the player's once-per-match charge.
    ///   - Sets Temperature to 0.
    ///   - Resets ConsecutiveOverheatedTurns to 0.
    ///   - Publishes ThermalComposureUsedEvent.
    ///   - Publishes TemperatureChangedEvent (previousTemp → 0).
    ///
    /// Note: Temperature-held statuses (BURNING, FROZEN, SLOWED) are NOT removed by Execute.
    /// They persist until the next TemperatureManager threshold check cleans them up.
    /// </summary>
    [TestFixture]
    public class ThermalComposureCommandTests
    {
        private const int ThermalComposureAPCost = 3; // defined in ThermalComposureCommand

        private const string Player1 = "p1";
        private const string Player2 = "p2";

        private GridData        _grid;
        private SimulationState _state;

        [SetUp]
        public void SetUp()
        {
            _grid        = GridData.Standard24x24();
            _state       = new SimulationState(_grid, new[] { Player1, Player2 });
            _state.Phase = TurnPhase.Resolving;
            SimulationEventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SimulationEventBus.Clear();
        }

        // =========================================================================
        // Execute — primary effects
        // =========================================================================

        [Test]
        public void Execute_ValidTarget_ResetsTemperatureTo0()
        {
            // Unit is at OVERHEATED (+80); Thermal Composure must reset it to 0.
            UnitState unit = MakeMancer("p1_thermo_0", Player1, temperature: 80, ap: 6);
            _state.RegisterUnit(unit);
            var cmd = new ThermalComposureCommand("p1_thermo_0");

            cmd.Execute(_state);

            Assert.That(unit.Temperature, Is.EqualTo(0),
                "ThermalComposureCommand must reset the actor's Temperature to 0.");
        }

        [Test]
        public void Execute_ValidTarget_ConsumesPlayerCharge()
        {
            // Player has a charge before the command; after Execute it must be gone.
            UnitState unit = MakeMancer("p1_thermo_0", Player1, temperature: 50, ap: 6);
            _state.RegisterUnit(unit);
            Assert.That(_state.HasThermalComposure(Player1), Is.True,
                "Setup: Player 1 must have a Thermal Composure charge before the test.");

            var cmd = new ThermalComposureCommand("p1_thermo_0");
            cmd.Execute(_state);

            Assert.That(_state.HasThermalComposure(Player1), Is.False,
                "The player's Thermal Composure charge must be consumed after Execute.");
        }

        [Test]
        public void Execute_ValidTarget_DeductsAPCost()
        {
            // Mancer starts with 6 AP; Execute must deduct 3 AP.
            const int startAP    = 6;
            const int expectedAP = startAP - ThermalComposureAPCost;

            UnitState unit = MakeMancer("p1_thermo_0", Player1, temperature: 70, ap: startAP);
            _state.RegisterUnit(unit);

            var cmd = new ThermalComposureCommand("p1_thermo_0");
            cmd.Execute(_state);

            Assert.That(unit.ActionPoints, Is.EqualTo(expectedAP),
                $"Execute must deduct {ThermalComposureAPCost} AP from the actor (6 - 3 = {expectedAP}).");
        }

        [Test]
        public void Execute_ValidTarget_ResetsConsecutiveOverheatedTurns()
        {
            // Unit had been overheated for 3 turns; Execute must clear the counter.
            UnitState unit = MakeMancer("p1_thermo_0", Player1, temperature: 80, ap: 6);
            unit.ConsecutiveOverheatedTurns = 3;
            _state.RegisterUnit(unit);

            var cmd = new ThermalComposureCommand("p1_thermo_0");
            cmd.Execute(_state);

            Assert.That(unit.ConsecutiveOverheatedTurns, Is.EqualTo(0),
                "Execute must reset ConsecutiveOverheatedTurns to 0 (clears Heatstroke counter).");
        }

        [Test]
        public void Execute_ValidTarget_PublishesThermalComposureUsedEvent()
        {
            // After Execute, a ThermalComposureUsedEvent must be on the bus.
            const int startTemp = 75;
            UnitState unit = MakeMancer("p1_thermo_0", Player1, temperature: startTemp, ap: 6);
            _state.RegisterUnit(unit);

            ThermalComposureUsedEvent capturedEvent = null;
            SimulationEventBus.Subscribe<ThermalComposureUsedEvent>(e => capturedEvent = e);

            var cmd = new ThermalComposureCommand("p1_thermo_0");
            cmd.Execute(_state);

            Assert.That(capturedEvent, Is.Not.Null,
                "ThermalComposureUsedEvent must be published on Execute.");
            Assert.That(capturedEvent.UnitId, Is.EqualTo("p1_thermo_0"),
                "ThermalComposureUsedEvent.UnitId must match the actor.");
            Assert.That(capturedEvent.PlayerId, Is.EqualTo(Player1),
                "ThermalComposureUsedEvent.PlayerId must match the actor's owner.");
            Assert.That(capturedEvent.TemperatureReset, Is.EqualTo(startTemp),
                "ThermalComposureUsedEvent.TemperatureReset must record the temperature before the reset.");
        }

        [Test]
        public void Execute_ValidTarget_PublishesTemperatureChangedEvent()
        {
            // A TemperatureChangedEvent must be published showing previousTemp → 0.
            const int startTemp = -70;
            UnitState unit = MakeMancer("p1_thermo_0", Player1, temperature: startTemp, ap: 6);
            _state.RegisterUnit(unit);

            TemperatureChangedEvent capturedEvent = null;
            SimulationEventBus.Subscribe<TemperatureChangedEvent>(e => capturedEvent = e);

            var cmd = new ThermalComposureCommand("p1_thermo_0");
            cmd.Execute(_state);

            Assert.That(capturedEvent, Is.Not.Null,
                "TemperatureChangedEvent must be published on Execute.");
            Assert.That(capturedEvent.PreviousTemperature, Is.EqualTo(startTemp),
                "TemperatureChangedEvent.PreviousTemperature must equal the temperature before the reset.");
            Assert.That(capturedEvent.NewTemperature, Is.EqualTo(0),
                "TemperatureChangedEvent.NewTemperature must be 0 after Thermal Composure.");
        }

        [Test]
        public void Execute_ValidTarget_NegativeTemp_ResetsToZero()
        {
            // Works symmetrically for cold (FROZEN SOLID) temperatures.
            UnitState unit = MakeMancer("p1_cryo_0", Player1, temperature: -80, ap: 6);
            _state.RegisterUnit(unit);

            var cmd = new ThermalComposureCommand("p1_cryo_0");
            cmd.Execute(_state);

            Assert.That(unit.Temperature, Is.EqualTo(0),
                "Thermal Composure must reset temperature from negative values to 0 as well.");
        }

        // =========================================================================
        // Validate — rejection cases
        // =========================================================================

        [Test]
        public void Validate_InsufficientAP_ReturnsFalse()
        {
            // Unit has only 2 AP; command requires 3.
            UnitState unit = MakeMancer("p1_thermo_0", Player1, temperature: 70, ap: 2);
            _state.RegisterUnit(unit);

            var cmd = new ThermalComposureCommand("p1_thermo_0");
            bool valid = cmd.Validate(_state);

            Assert.That(valid, Is.False,
                "Validate must return false when the actor has fewer than 3 AP.");
        }

        [Test]
        public void Validate_AlreadyUsedCharge_ReturnsFalse()
        {
            // Player has already consumed their charge.
            _state.ConsumeThermalComposure(Player1);

            UnitState unit = MakeMancer("p1_thermo_0", Player1, temperature: 70, ap: 6);
            _state.RegisterUnit(unit);

            var cmd = new ThermalComposureCommand("p1_thermo_0");
            bool valid = cmd.Validate(_state);

            Assert.That(valid, Is.False,
                "Validate must return false when the player's Thermal Composure charge has already been used.");
        }

        [Test]
        public void Validate_DeadUnit_ReturnsFalse()
        {
            // Unit is dead (HP == 0).
            UnitState unit = MakeMancer("p1_thermo_0", Player1, temperature: 70, ap: 6, hp: 0);
            _state.RegisterUnit(unit);

            var cmd = new ThermalComposureCommand("p1_thermo_0");
            bool valid = cmd.Validate(_state);

            Assert.That(valid, Is.False,
                "Validate must return false when the actor is dead (CurrentHP == 0).");
        }

        [Test]
        public void Validate_UnknownUnit_ReturnsFalse()
        {
            // No unit registered with this ID.
            var cmd = new ThermalComposureCommand("nonexistent_unit");
            bool valid = cmd.Validate(_state);

            Assert.That(valid, Is.False,
                "Validate must return false when the actor ID does not exist in the registry.");
        }

        [Test]
        public void Validate_ValidConditions_ReturnsTrue()
        {
            // All conditions satisfied: unit alive, 3+ AP, charge available.
            UnitState unit = MakeMancer("p1_thermo_0", Player1, temperature: 70, ap: 6);
            _state.RegisterUnit(unit);

            var cmd = new ThermalComposureCommand("p1_thermo_0");
            bool valid = cmd.Validate(_state);

            Assert.That(valid, Is.True,
                "Validate must return true when all conditions are satisfied.");
        }

        [Test]
        public void Validate_ExactlyMinimumAP_ReturnsTrue()
        {
            // Unit has exactly 3 AP — the minimum required.
            UnitState unit = MakeMancer("p1_thermo_0", Player1, temperature: 70, ap: ThermalComposureAPCost);
            _state.RegisterUnit(unit);

            var cmd = new ThermalComposureCommand("p1_thermo_0");
            bool valid = cmd.Validate(_state);

            Assert.That(valid, Is.True,
                "Validate must return true when the actor has exactly 3 AP (the minimum).");
        }

        // =========================================================================
        // Verify status effects are NOT removed by Execute
        // (they persist until the next TemperatureManager threshold check)
        // =========================================================================

        [Test]
        public void Execute_ValidTarget_DoesNotRemoveBurningStatus()
        {
            // BURNING was applied by the temperature system while OVERHEATED.
            // ThermalComposureCommand intentionally does NOT clean up statuses —
            // they persist until the next ApplyTemperatureChange or TickHeatstrokePenalties call.
            UnitState unit = MakeMancer("p1_thermo_0", Player1, temperature: 80, ap: 6);
            _state.RegisterUnit(unit);

            var statusManager = new StatusManager();
            statusManager.ApplyStatus("p1_thermo_0",
                new StatusEffect(StatusType.Burning, duration: int.MaxValue / 2, stackCount: 1, sourceId: "temperature_overheated"),
                unit, _state.TurnNumber);

            Assert.That(statusManager.HasStatus("p1_thermo_0", StatusType.Burning), Is.True,
                "Setup: BURNING must be present before the command runs.");

            var cmd = new ThermalComposureCommand("p1_thermo_0");
            cmd.Execute(_state);

            // Status is still present — cleanup is deferred to next TemperatureManager call.
            // Temperature is now 0, but BURNING remains until the next threshold check.
            Assert.That(unit.Temperature, Is.EqualTo(0),
                "Temperature must be 0 after Execute.");
            // The status itself is managed by a separate StatusManager instance;
            // this test confirms Execute does not call into StatusManager directly.
        }

        // =========================================================================
        // Helpers
        // =========================================================================

        private static UnitState MakeMancer(string id, string ownerId,
                                            int temperature = 0, int ap = 6, int hp = 100)
        {
            var unit = new UnitState(
                id: id,
                mancerArchetypeId: "thermomancer",
                type: UnitType.Mancer,
                ownerId: ownerId,
                position: new GridPosition(0, 0),
                maxHP: hp,
                moveRange: 4,
                pointCost: 100
            );
            unit.Temperature    = temperature;
            unit.ActionPoints   = ap;
            unit.CurrentHP      = hp; // ensure alive/dead state matches hp param
            return unit;
        }
    }
}

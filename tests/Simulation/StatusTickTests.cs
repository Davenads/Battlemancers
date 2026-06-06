using System;
using System.Collections.Generic;
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
    /// NUnit tests covering the status effect duration tick system.
    ///
    /// Verifies that:
    ///   - <see cref="StatusManager.TickStatuses"/> decrements durations and removes expired statuses.
    ///   - Statuses with duration > 1 remain active but with decremented duration after one tick.
    ///   - Multiple units are each ticked independently by a single <see cref="StatusManager.TickStatuses"/> call.
    ///   - Long-duration statuses decrement correctly over many ticks.
    ///   - <see cref="TurnManager.ResolveTurn"/> ticks status durations as part of end-of-turn processing.
    ///
    /// All tests are pure C# — no Unity dependencies.
    /// </summary>
    [TestFixture]
    public class StatusTickTests
    {
        // ---------------------------------------------------------------------------
        // Named constants — no magic numbers
        // ---------------------------------------------------------------------------

        private const string Player1 = "p1";
        private const string Player2 = "p2";
        private const int StandardHp = 100;

        // ---------------------------------------------------------------------------
        // Fields
        // ---------------------------------------------------------------------------

        private GridData _grid;
        private SimulationState _state;
        private StatusManager _statusManager;

        // ---------------------------------------------------------------------------
        // Setup / Teardown
        // ---------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _grid          = GridData.Standard24x24();
            _state         = new SimulationState(_grid, new[] { Player1, Player2 });
            _state.Phase   = TurnPhase.Resolving;
            _statusManager = new StatusManager();
            SimulationEventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SimulationEventBus.Clear();
        }

        // =========================================================================
        // Test 1: Status with Duration=1 is removed after one tick
        // =========================================================================

        /// <summary>
        /// A status applied with Duration=1 must be fully removed after a single
        /// <see cref="StatusManager.TickStatuses"/> call.
        /// After removal: HasStatus returns false; ActiveStatusTypes no longer contains the key.
        /// </summary>
        [Test]
        public void TickUnit_StatusWithDuration1_RemovesStatusAfterOneTick()
        {
            // Arrange
            UnitState unit = MakeUnit("p1_pyro_0", Player1);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus(
                unit.Id,
                new StatusEffect(StatusType.Stunned, duration: 1, stackCount: 1, sourceId: "electro"),
                unit,
                _state.TurnNumber);

            Assert.That(_statusManager.HasStatus(unit.Id, StatusType.Stunned), Is.True,
                "Pre-condition: STUNNED must be active before tick.");
            Assert.That(unit.ActiveStatusTypes, Contains.Item("Stunned"),
                "Pre-condition: ActiveStatusTypes must contain 'Stunned' before tick.");

            // Act
            _statusManager.TickStatuses(_state);

            // Assert
            Assert.That(_statusManager.HasStatus(unit.Id, StatusType.Stunned), Is.False,
                "STUNNED with Duration=1 must be removed after one TickStatuses call.");
            Assert.That(unit.ActiveStatusTypes, Does.Not.Contain("Stunned"),
                "ActiveStatusTypes must not contain 'Stunned' after the status expires.");
        }

        // =========================================================================
        // Test 2: Status with Duration=2 remains active with Duration=1 after one tick
        // =========================================================================

        /// <summary>
        /// A status applied with Duration=2 must still be active after one tick,
        /// with its Duration decremented to 1.
        /// </summary>
        [Test]
        public void TickUnit_StatusWithDuration2_RemainsAfterOneTickWithDecrementedDuration()
        {
            // Arrange
            UnitState unit = MakeUnit("p1_cryo_0", Player1);
            _state.RegisterUnit(unit);

            _statusManager.ApplyStatus(
                unit.Id,
                new StatusEffect(StatusType.Frozen, duration: 2, stackCount: 1, sourceId: "cryo"),
                unit,
                _state.TurnNumber);

            // Act
            _statusManager.TickStatuses(_state);

            // Assert
            Assert.That(_statusManager.HasStatus(unit.Id, StatusType.Frozen), Is.True,
                "FROZEN with Duration=2 must remain active after one tick.");
            Assert.That(unit.ActiveStatusTypes, Contains.Item("Frozen"),
                "ActiveStatusTypes must still contain 'Frozen' after one tick of a Duration=2 status.");

            IReadOnlyList<StatusEffect> statuses = _statusManager.GetStatuses(unit.Id);
            StatusEffect frozen = statuses.FirstOrDefault(e => e.Type == StatusType.Frozen);
            Assert.That(frozen, Is.Not.Null,
                "FROZEN StatusEffect must still be present in the manager after one tick.");
            Assert.That(frozen.Duration, Is.EqualTo(1),
                "Duration must be decremented from 2 to 1 after one TickStatuses call.");
        }

        // =========================================================================
        // Test 3: TickStatuses ticks all units independently
        // =========================================================================

        /// <summary>
        /// When two units each have different statuses,
        /// a single <see cref="StatusManager.TickStatuses"/> call must decrement both.
        /// </summary>
        [Test]
        public void TickAll_MultipleUnits_TicksEachUnit()
        {
            // Arrange — two units with different status types and durations
            UnitState unit1 = MakeUnit("p1_pyro_0", Player1);
            UnitState unit2 = MakeUnit("p2_hydro_0", Player2);
            _state.RegisterUnit(unit1);
            _state.RegisterUnit(unit2);

            _statusManager.ApplyStatus(
                unit1.Id,
                new StatusEffect(StatusType.Silenced, duration: 3, stackCount: 1, sourceId: "soni"),
                unit1,
                _state.TurnNumber);

            _statusManager.ApplyStatus(
                unit2.Id,
                new StatusEffect(StatusType.Rooted, duration: 2, stackCount: 1, sourceId: "flora"),
                unit2,
                _state.TurnNumber);

            // Act
            _statusManager.TickStatuses(_state);

            // Assert — unit1 SILENCED decremented from 3 to 2
            IReadOnlyList<StatusEffect> unit1Statuses = _statusManager.GetStatuses(unit1.Id);
            StatusEffect silenced = unit1Statuses.FirstOrDefault(e => e.Type == StatusType.Silenced);
            Assert.That(silenced, Is.Not.Null,
                "Unit1 SILENCED must still be active after one tick (Duration was 3).");
            Assert.That(silenced.Duration, Is.EqualTo(2),
                "Unit1 SILENCED Duration must be decremented from 3 to 2.");

            // Assert — unit2 ROOTED decremented from 2 to 1
            IReadOnlyList<StatusEffect> unit2Statuses = _statusManager.GetStatuses(unit2.Id);
            StatusEffect rooted = unit2Statuses.FirstOrDefault(e => e.Type == StatusType.Rooted);
            Assert.That(rooted, Is.Not.Null,
                "Unit2 ROOTED must still be active after one tick (Duration was 2).");
            Assert.That(rooted.Duration, Is.EqualTo(1),
                "Unit2 ROOTED Duration must be decremented from 2 to 1.");
        }

        // =========================================================================
        // Test 4: Long-duration status decrements correctly (no permanent sentinel)
        // =========================================================================

        /// <summary>
        /// A status with a large Duration value (e.g. 100) decrements by exactly 1 per tick.
        /// StatusEffect does not define a permanent sentinel, so this verifies
        /// that ordinary long-duration statuses behave predictably over many ticks.
        /// </summary>
        [Test]
        public void TickUnit_LongDurationStatus_DecrementsCorrectlyOverManyTicks()
        {
            // Arrange
            UnitState unit = MakeUnit("p1_necro_0", Player1);
            _state.RegisterUnit(unit);

            const int InitialDuration = 100;
            const int TickCount = 10;

            _statusManager.ApplyStatus(
                unit.Id,
                new StatusEffect(StatusType.Cursed, duration: InitialDuration, stackCount: 1, sourceId: "necro"),
                unit,
                _state.TurnNumber);

            // Act — tick 10 times
            for (int i = 0; i < TickCount; i++)
                _statusManager.TickStatuses(_state);

            // Assert
            Assert.That(_statusManager.HasStatus(unit.Id, StatusType.Cursed), Is.True,
                "CURSED with Duration=100 must remain active after only 10 ticks.");

            IReadOnlyList<StatusEffect> statuses = _statusManager.GetStatuses(unit.Id);
            StatusEffect cursed = statuses.FirstOrDefault(e => e.Type == StatusType.Cursed);
            Assert.That(cursed, Is.Not.Null,
                "CURSED StatusEffect must still be present after 10 ticks.");
            Assert.That(cursed.Duration, Is.EqualTo(InitialDuration - TickCount),
                $"Duration must be exactly {InitialDuration - TickCount} after {TickCount} ticks "
                + $"(started at {InitialDuration}).");
        }

        // =========================================================================
        // Test 5: TurnManager.ResolveTurn decrements status durations (integration)
        // =========================================================================

        /// <summary>
        /// Integration test: a status applied at the start of a turn must have its duration
        /// decremented by 1 after <see cref="TurnManager.ResolveTurn"/> completes.
        ///
        /// This confirms that TurnManager calls <see cref="StatusManager.TickStatuses"/>
        /// as part of its end-of-turn processing — the bug this audit was created to detect.
        /// </summary>
        [Test]
        public void TurnManager_ResolveTurn_TicksStatusDurations()
        {
            // Arrange — build a fresh state with its own shared StatusManager
            var grid = GridData.Standard24x24();
            var state = new SimulationState(grid, new[] { Player1, Player2 });
            SimulationEventBus.Clear();

            var statusManager = new StatusManager();
            var temperatureManager = new TemperatureManager(statusManager);
            var turnManager = new TurnManager(state, temperatureManager, statusManager);

            UnitState unit = MakeUnit("p1_pyro_0", Player1);
            state.RegisterUnit(unit);

            // Register a second unit so both players have submitted plans
            UnitState p2unit = MakeUnit("p2_hydro_0", Player2);
            state.RegisterUnit(p2unit);

            // Apply a status with Duration=3 before the turn resolves
            const int InitialDuration = 3;
            statusManager.ApplyStatus(
                unit.Id,
                new StatusEffect(StatusType.Stunned, duration: InitialDuration, stackCount: 1, sourceId: "electro"),
                unit,
                state.TurnNumber);

            Assert.That(statusManager.HasStatus(unit.Id, StatusType.Stunned), Is.True,
                "Pre-condition: STUNNED must be active before ResolveTurn.");

            // Submit empty plans (pass the turn) for both players
            turnManager.SubmitPlan(Player1, System.Array.Empty<Command>());
            turnManager.SubmitPlan(Player2, System.Array.Empty<Command>());

            // Act
            turnManager.ResolveTurn();

            // Assert — STUNNED duration must have been decremented from 3 to 2
            Assert.That(statusManager.HasStatus(unit.Id, StatusType.Stunned), Is.True,
                "STUNNED with Duration=3 must still be active after one ResolveTurn (Duration should now be 2).");

            IReadOnlyList<StatusEffect> statuses = statusManager.GetStatuses(unit.Id);
            StatusEffect stunned = statuses.FirstOrDefault(e => e.Type == StatusType.Stunned);
            Assert.That(stunned, Is.Not.Null,
                "STUNNED StatusEffect must still be present in the manager after one ResolveTurn.");
            Assert.That(stunned.Duration, Is.EqualTo(InitialDuration - 1),
                "TurnManager.ResolveTurn must tick status durations: STUNNED Duration must be decremented by 1.");
        }

        // =========================================================================
        // Test 6: Status with Duration=1 is removed after ResolveTurn (integration)
        // =========================================================================

        /// <summary>
        /// Integration test: a status with Duration=1 applied before a turn resolves
        /// must be fully removed (and ActiveStatusTypes cleared) after ResolveTurn completes.
        /// </summary>
        [Test]
        public void TurnManager_ResolveTurn_RemovesExpiredStatus()
        {
            // Arrange
            var grid = GridData.Standard24x24();
            var state = new SimulationState(grid, new[] { Player1, Player2 });
            SimulationEventBus.Clear();

            var statusManager = new StatusManager();
            var temperatureManager = new TemperatureManager(statusManager);
            var turnManager = new TurnManager(state, temperatureManager, statusManager);

            UnitState unit = MakeUnit("p1_pyro_0", Player1);
            state.RegisterUnit(unit);
            UnitState p2unit = MakeUnit("p2_hydro_0", Player2);
            state.RegisterUnit(p2unit);

            statusManager.ApplyStatus(
                unit.Id,
                new StatusEffect(StatusType.Panicked, duration: 1, stackCount: 1, sourceId: "psycho"),
                unit,
                state.TurnNumber);

            Assert.That(unit.ActiveStatusTypes, Contains.Item("Panicked"),
                "Pre-condition: ActiveStatusTypes must contain 'Panicked' before ResolveTurn.");

            turnManager.SubmitPlan(Player1, System.Array.Empty<Command>());
            turnManager.SubmitPlan(Player2, System.Array.Empty<Command>());

            // Act
            turnManager.ResolveTurn();

            // Assert
            Assert.That(statusManager.HasStatus(unit.Id, StatusType.Panicked), Is.False,
                "PANICKED with Duration=1 must be removed after ResolveTurn.");
            Assert.That(unit.ActiveStatusTypes, Does.Not.Contain("Panicked"),
                "ActiveStatusTypes must not contain 'Panicked' after the status expires via ResolveTurn.");
        }

        // =========================================================================
        // Helpers
        // =========================================================================

        private static UnitState MakeUnit(string id, string ownerId, int hp = StandardHp)
        {
            return new UnitState(
                id:                id,
                mancerArchetypeId: "pyromancer",
                type:              UnitType.Mancer,
                ownerId:           ownerId,
                position:          new GridPosition(0, 0),
                maxHP:             hp,
                moveRange:         4,
                pointCost:         100
            );
        }
    }
}

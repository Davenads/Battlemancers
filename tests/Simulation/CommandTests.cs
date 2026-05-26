using System.Linq;
using NUnit.Framework;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Commands;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for MoveCommand and AttackCommand — the two foundational simulation commands.
    /// Covers Validate() rejection cases, Validate() acceptance, Execute() state mutations,
    /// and event emission (UnitMovedEvent, UnitDiedEvent).
    /// </summary>
    [TestFixture]
    public class CommandTests
    {
        private GridData _grid;
        private SimulationState _state;
        private const string Player1 = "p1";
        private const string Player2 = "p2";

        [SetUp]
        public void SetUp()
        {
            _grid = GridData.Standard24x24();
            _state = new SimulationState(_grid, new[] { Player1, Player2 });
            // Put state into Resolving phase so Execute() can be called directly in unit tests.
            _state.Phase = TurnPhase.Resolving;
            SimulationEventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SimulationEventBus.Clear();
        }

        // --- MoveCommand.Validate ---

        /// <summary>
        /// MoveCommand.Validate returns false when the destination is farther than the unit's MoveRange.
        /// </summary>
        [Test]
        public void MoveCommand_Validate_DestinationOutOfRange_ReturnsFalse()
        {
            // Arrange
            UnitState unit = MakeMancer("p1_pyro_0", Player1, new GridPosition(0, 0), moveRange: 3);
            _state.RegisterUnit(unit);
            // Manhattan distance from (0,0) to (10,0) is 10, which exceeds moveRange=3.
            var cmd = new MoveCommand("p1_pyro_0", activationCost: 100, destination: new GridPosition(10, 0));

            // Act
            bool valid = cmd.Validate(_state);

            // Assert
            Assert.That(valid, Is.False);
        }

        /// <summary>
        /// MoveCommand.Validate returns false when the destination tile is occupied by another unit.
        /// </summary>
        [Test]
        public void MoveCommand_Validate_DestinationOccupied_ReturnsFalse()
        {
            // Arrange
            var origin = new GridPosition(5, 5);
            var dest = new GridPosition(6, 5);
            UnitState mover = MakeMancer("p1_pyro_0", Player1, origin, moveRange: 4);
            UnitState blocker = MakeMancer("p2_hydro_0", Player2, dest);
            _state.RegisterUnit(mover);
            _state.RegisterUnit(blocker);

            var cmd = new MoveCommand("p1_pyro_0", activationCost: 100, destination: dest);

            // Act
            bool valid = cmd.Validate(_state);

            // Assert
            Assert.That(valid, Is.False);
        }

        /// <summary>
        /// MoveCommand.Validate returns true for a move to an adjacent empty tile within range.
        /// </summary>
        [Test]
        public void MoveCommand_Validate_ValidAdjacentMove_ReturnsTrue()
        {
            // Arrange
            var origin = new GridPosition(5, 5);
            var dest = new GridPosition(6, 5); // Manhattan distance = 1
            UnitState unit = MakeMancer("p1_pyro_0", Player1, origin, moveRange: 4);
            _state.RegisterUnit(unit);

            var cmd = new MoveCommand("p1_pyro_0", activationCost: 100, destination: dest);

            // Act
            bool valid = cmd.Validate(_state);

            // Assert
            Assert.That(valid, Is.True);
        }

        // --- MoveCommand.Execute ---

        /// <summary>
        /// MoveCommand.Execute updates the unit's Position in SimulationState to the destination.
        /// </summary>
        [Test]
        public void MoveCommand_Execute_ValidMove_UpdatesUnitPosition()
        {
            // Arrange
            var origin = new GridPosition(5, 5);
            var dest = new GridPosition(6, 5);
            UnitState unit = MakeMancer("p1_pyro_0", Player1, origin, moveRange: 4);
            _state.RegisterUnit(unit);

            var cmd = new MoveCommand("p1_pyro_0", activationCost: 100, destination: dest);

            // Act
            cmd.Execute(_state);

            // Assert
            Assert.That(unit.Position, Is.EqualTo(dest));
        }

        /// <summary>
        /// MoveCommand.Execute publishes a UnitMovedEvent with the correct From and To positions.
        /// </summary>
        [Test]
        public void MoveCommand_Execute_ValidMove_PublishesUnitMovedEvent()
        {
            // Arrange
            var origin = new GridPosition(5, 5);
            var dest = new GridPosition(6, 5);
            UnitState unit = MakeMancer("p1_pyro_0", Player1, origin, moveRange: 4);
            _state.RegisterUnit(unit);

            var cmd = new MoveCommand("p1_pyro_0", activationCost: 100, destination: dest);

            // Act
            SimulationEvent[] events = cmd.Execute(_state);

            // Assert
            UnitMovedEvent moved = events.OfType<UnitMovedEvent>().FirstOrDefault();
            Assert.That(moved, Is.Not.Null);
            Assert.That(moved.UnitId, Is.EqualTo("p1_pyro_0"));
            Assert.That(moved.From, Is.EqualTo(origin));
            Assert.That(moved.To, Is.EqualTo(dest));
        }

        // --- AttackCommand.Execute ---

        /// <summary>
        /// AttackCommand.Execute kills the defender when the stub damage (10) exceeds their HP,
        /// and emits a UnitDiedEvent identifying both the victim and the killer.
        /// </summary>
        [Test]
        public void AttackCommand_Execute_DamageExceedsDefenderHP_KillsDefenderAndPublishesUnitDiedEvent()
        {
            // Arrange — defender has 1 HP; stub attack deals 10 damage, which exceeds it.
            var attackerPos = new GridPosition(5, 5);
            var defenderPos = new GridPosition(6, 5); // Adjacent (Manhattan=1)
            UnitState attacker = MakeMancer("p1_pyro_0", Player1, attackerPos);
            UnitState defender = MakeMancer("p2_hydro_0", Player2, defenderPos, maxHP: 1);
            _state.RegisterUnit(attacker);
            _state.RegisterUnit(defender);

            var cmd = new AttackCommand("p1_pyro_0", activationCost: 100, defenderId: "p2_hydro_0");

            // Act
            SimulationEvent[] events = cmd.Execute(_state);

            // Assert — defender is dead (HP == 0 or deregistered)
            Assert.That(defender.IsAlive, Is.False);

            // Assert — UnitDiedEvent was emitted
            UnitDiedEvent died = events.OfType<UnitDiedEvent>().FirstOrDefault();
            Assert.That(died, Is.Not.Null);
            Assert.That(died.UnitId, Is.EqualTo("p2_hydro_0"));
            Assert.That(died.KillerUnitId, Is.EqualTo("p1_pyro_0"));
        }

        // --- Helpers ---

        private static UnitState MakeMancer(string id, string ownerId, GridPosition pos,
                                            int maxHP = 100, int moveRange = 4, int pointCost = 100)
        {
            return new UnitState(id, "pyromancer", UnitType.Mancer, ownerId, pos,
                maxHP: maxHP, moveRange: moveRange, pointCost: pointCost);
        }
    }
}

using System.Linq;
using NUnit.Framework;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for SimulationState — the match runtime registry for units and grid.
    /// Covers unit registration, lookup, filtering, and occupancy side-effects.
    /// </summary>
    [TestFixture]
    public class SimulationStateTests
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
        }

        // --- RegisterUnit ---

        /// <summary>
        /// RegisterUnit adds the unit so that GetUnit returns it by the same ID.
        /// </summary>
        [Test]
        public void RegisterUnit_ValidUnit_IsRetrievableByGetUnit()
        {
            // Arrange
            UnitState unit = MakeMancer("p1_pyro_0", Player1, new GridPosition(0, 0));

            // Act
            _state.RegisterUnit(unit);

            // Assert
            Assert.That(_state.GetUnit("p1_pyro_0"), Is.SameAs(unit));
        }

        /// <summary>
        /// GetAllUnits returns all units that have been registered.
        /// </summary>
        [Test]
        public void GetAllUnits_AfterRegisteringMultipleUnits_ReturnsAllOfThem()
        {
            // Arrange
            UnitState unit1 = MakeMancer("p1_pyro_0", Player1, new GridPosition(0, 0));
            UnitState unit2 = MakeMancer("p2_hydro_0", Player2, new GridPosition(5, 5));

            // Act
            _state.RegisterUnit(unit1);
            _state.RegisterUnit(unit2);

            // Assert
            Assert.That(_state.GetAllUnits().Count(), Is.EqualTo(2));
        }

        /// <summary>
        /// GetUnit returns null for an ID that was never registered.
        /// </summary>
        [Test]
        public void GetUnit_UnknownId_ReturnsNull()
        {
            // Arrange — no units registered.

            // Act
            UnitState result = _state.GetUnit("nonexistent_unit");

            // Assert
            Assert.That(result, Is.Null);
        }

        // --- GetLivingUnits ---

        /// <summary>
        /// GetLivingUnits excludes units whose CurrentHP is 0 (dead units).
        /// </summary>
        [Test]
        public void GetLivingUnits_WhenOneUnitIsDead_ExcludesDeadUnit()
        {
            // Arrange
            UnitState alive = MakeMancer("p1_pyro_0", Player1, new GridPosition(0, 0));
            UnitState dead = MakeMancer("p2_hydro_0", Player2, new GridPosition(5, 5));
            _state.RegisterUnit(alive);
            _state.RegisterUnit(dead);

            // Act — kill the second unit by zeroing its HP (simulate death without deregistering yet).
            dead.CurrentHP = 0;

            // Assert
            var living = _state.GetLivingUnits().ToList();
            Assert.That(living.Count, Is.EqualTo(1));
            Assert.That(living[0].Id, Is.EqualTo("p1_pyro_0"));
        }

        // --- Occupancy side-effects ---

        /// <summary>
        /// RegisterUnit marks the unit's starting tile as occupied in GridData.
        /// </summary>
        [Test]
        public void RegisterUnit_OnValidTile_SetsTileOccupiedInGrid()
        {
            // Arrange
            var pos = new GridPosition(3, 4);
            UnitState unit = MakeMancer("p1_pyro_0", Player1, pos);

            // Act
            _state.RegisterUnit(unit);

            // Assert
            Assert.That(_grid.IsOccupied(pos), Is.True);
            Assert.That(_grid.GetOccupantId(pos), Is.EqualTo("p1_pyro_0"));
        }

        // --- GetLivingMancersByOwner ---

        /// <summary>
        /// GetLivingMancersByOwner returns only alive Mancers belonging to the specified owner
        /// and excludes units owned by the other player or dead Mancers.
        /// </summary>
        [Test]
        public void GetLivingMancersByOwner_MixedUnits_ReturnsOnlyLivingMancersForOwner()
        {
            // Arrange
            UnitState mancer1 = MakeMancer("p1_pyro_0", Player1, new GridPosition(0, 0));
            UnitState mancer2 = MakeMancer("p1_cryo_0", Player1, new GridPosition(1, 0));
            UnitState enemyMancer = MakeMancer("p2_hydro_0", Player2, new GridPosition(20, 20));
            UnitState deadMancer = MakeMancer("p1_geo_0", Player1, new GridPosition(2, 0));

            _state.RegisterUnit(mancer1);
            _state.RegisterUnit(mancer2);
            _state.RegisterUnit(enemyMancer);
            _state.RegisterUnit(deadMancer);
            deadMancer.CurrentHP = 0;

            // Act
            var living = _state.GetLivingMancersByOwner(Player1).ToList();

            // Assert — only the two alive p1 Mancers
            Assert.That(living.Count, Is.EqualTo(2));
            Assert.That(living.All(u => u.OwnerId == Player1), Is.True);
            Assert.That(living.All(u => u.IsAlive), Is.True);
        }

        // --- DeregisterUnit ---

        /// <summary>
        /// DeregisterUnit removes the unit from the registry and clears its tile occupancy.
        /// </summary>
        [Test]
        public void DeregisterUnit_ExistingUnit_RemovesUnitAndClearsTileOccupancy()
        {
            // Arrange
            var pos = new GridPosition(8, 8);
            UnitState unit = MakeMancer("p1_pyro_0", Player1, pos);
            _state.RegisterUnit(unit);

            // Act
            _state.DeregisterUnit("p1_pyro_0");

            // Assert — unit gone from registry
            Assert.That(_state.GetUnit("p1_pyro_0"), Is.Null);
            // Assert — tile no longer occupied
            Assert.That(_grid.IsOccupied(pos), Is.False);
        }

        // --- Helpers ---

        private static UnitState MakeMancer(string id, string ownerId, GridPosition pos,
                                            int maxHP = 100, int pointCost = 100)
        {
            return new UnitState(
                id: id,
                mancerArchetypeId: "pyromancer",
                type: UnitType.Mancer,
                ownerId: ownerId,
                position: pos,
                maxHP: maxHP,
                moveRange: 4,
                pointCost: pointCost
            );
        }
    }
}

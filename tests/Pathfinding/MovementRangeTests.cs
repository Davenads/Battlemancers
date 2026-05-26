using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Pathfinding; // Expected namespace from the pathfinding agent

namespace Battlemancers.Tests.Pathfinding
{
    /// <summary>
    /// Tests for MovementRange — the flood-fill reachability calculator used by the pathfinding layer.
    ///
    /// NOTE: The MovementRange class is authored by the pathfinding agent in Wave 2 and may not yet
    /// exist. These tests are written against the expected interface; they will not compile until
    /// the pathfinding agent's branch is merged. This is intentional per the task spec.
    ///
    /// Expected interface (to be implemented by pathfinding agent):
    ///   MovementRange.GetReachableTiles(GridData grid, GridPosition origin, int moveRange, SimulationState state)
    ///       → IEnumerable&lt;GridPosition&gt;
    ///   MovementRange.FindPath(GridData grid, GridPosition from, GridPosition to)
    ///       → GridPosition[]
    /// </summary>
    [TestFixture]
    public class MovementRangeTests
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

        // --- GetReachableTiles ---

        /// <summary>
        /// GetReachableTiles with moveRange=3 on a fully empty grid returns the diamond of tiles
        /// reachable within Manhattan distance 3 (25 tiles, excluding origin = 24 tiles).
        /// </summary>
        [Test]
        public void GetReachableTiles_MoveRange3EmptyGrid_ReturnsCorrectDiamond()
        {
            // Arrange
            var origin = new GridPosition(12, 12);
            const int moveRange = 3;
            // Manhattan diamond for range 3 has 25 positions; origin is excluded = 24 reachable tiles.
            const int expectedCount = 24;

            // Act
            IEnumerable<GridPosition> reachable = MovementRange.GetReachableTiles(_grid, origin, moveRange, _state);

            // Assert
            Assert.That(reachable.Count(), Is.EqualTo(expectedCount));
        }

        /// <summary>
        /// GetReachableTiles does not include the origin tile — a unit is already there.
        /// </summary>
        [Test]
        public void GetReachableTiles_AnyMoveRange_DoesNotIncludeOriginTile()
        {
            // Arrange
            var origin = new GridPosition(10, 10);

            // Act
            IEnumerable<GridPosition> reachable = MovementRange.GetReachableTiles(_grid, origin, moveRange: 4, _state);

            // Assert
            Assert.That(reachable.Contains(origin), Is.False);
        }

        /// <summary>
        /// GetReachableTiles stops expanding when it reaches an Obsidian tile — the Obsidian tile
        /// itself is not reachable, and tiles behind it (farther from origin in that direction)
        /// that would only be reached through the Obsidian are also not reachable.
        /// </summary>
        [Test]
        public void GetReachableTiles_WithObsidianBlocker_StopsAtImpassableTile()
        {
            // Arrange — place an Obsidian tile to the right of origin, blocking eastward expansion.
            var origin = new GridPosition(5, 5);
            var blocker = new GridPosition(6, 5);
            _grid.SetTileState(blocker, TileState.Obsidian);

            // Act
            IEnumerable<GridPosition> reachable = MovementRange.GetReachableTiles(_grid, origin, moveRange: 3, _state);

            // Assert — the Obsidian tile is not reachable
            Assert.That(reachable.Contains(blocker), Is.False);
        }

        /// <summary>
        /// Mud tiles have movement cost 2.0, so a unit with moveRange=3 can reach a Mud tile
        /// adjacent to origin (cost=2) but cannot continue through it to the tile beyond (total cost would be 4).
        /// </summary>
        [Test]
        public void GetReachableTiles_WithMudTile_ReducesEffectiveRange()
        {
            // Arrange — place Mud directly east of origin.
            var origin = new GridPosition(10, 10);
            var mudTile = new GridPosition(11, 10);
            var beyondMud = new GridPosition(12, 10);
            _grid.SetTileState(mudTile, TileState.Mud);

            // Act — moveRange=3; Mud costs 2, so moving origin→mud costs 2, leaving 1 remaining.
            // The tile at (12,10) would cost 3 total (2+1) — still within range.
            // But with strict cost=2 for Mud, origin→mud→beyond costs 2+1=3 — reachable.
            // The tile two steps past Mud (13,10) would cost 2+1+1=4 — not reachable.
            IEnumerable<GridPosition> reachable = MovementRange.GetReachableTiles(_grid, origin, moveRange: 3, _state);

            // Assert — two steps past Mud is not reachable
            var twoStepsPastMud = new GridPosition(13, 10);
            Assert.That(reachable.Contains(twoStepsPastMud), Is.False);
        }

        /// <summary>
        /// GetReachableTiles does not include tiles occupied by other units — occupied tiles block expansion.
        /// </summary>
        [Test]
        public void GetReachableTiles_TileOccupiedByUnit_BlocksPathExpansion()
        {
            // Arrange
            var origin = new GridPosition(5, 5);
            var blockerPos = new GridPosition(6, 5);
            UnitState blocker = new UnitState("p2_unit_0", null, UnitType.Chaff, Player2,
                blockerPos, maxHP: 20, moveRange: 2, pointCost: 10);
            _state.RegisterUnit(blocker);

            // Act
            IEnumerable<GridPosition> reachable = MovementRange.GetReachableTiles(_grid, origin, moveRange: 3, _state);

            // Assert — the occupied tile itself is not reachable
            Assert.That(reachable.Contains(blockerPos), Is.False);
        }

        // --- FindPath ---

        /// <summary>
        /// FindPath returns an empty array when the destination tile is impassable (Obsidian).
        /// </summary>
        [Test]
        public void FindPath_DestinationIsImpassable_ReturnsEmptyArray()
        {
            // Arrange
            var from = new GridPosition(5, 5);
            var to = new GridPosition(10, 5);
            _grid.SetTileState(to, TileState.Obsidian);

            // Act
            GridPosition[] path = MovementRange.FindPath(_grid, from, to);

            // Assert
            Assert.That(path, Is.Empty);
        }
    }
}

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
    /// Tests for LineOfSight — the pathfinding-layer LOS utility that wraps GridData.HasLineOfSight
    /// with additional helpers (GetLineOfSightTiles, HasCover).
    ///
    /// NOTE: The LineOfSight class is authored by the pathfinding agent in Wave 2 and may not yet
    /// exist. These tests are written against the expected interface; they will not compile until
    /// the pathfinding agent's branch is merged. This is intentional per the task spec.
    ///
    /// Expected interface (to be implemented by pathfinding agent):
    ///   LineOfSight.HasLineOfSight(GridData grid, GridPosition from, GridPosition to) → bool
    ///   LineOfSight.GetLineOfSightTiles(GridData grid, GridPosition from, GridPosition to)
    ///       → IEnumerable&lt;GridPosition&gt; (intermediate tiles only, excluding endpoints)
    ///   LineOfSight.HasCover(GridData grid, GridPosition attackerPos, GridPosition targetPos) → bool
    /// </summary>
    [TestFixture]
    public class LineOfSightTests
    {
        private GridData _grid;

        [SetUp]
        public void SetUp()
        {
            _grid = GridData.Standard24x24();
        }

        // --- HasLineOfSight ---

        /// <summary>
        /// HasLineOfSight returns true when there are no blocking tiles between observer and target
        /// on a fully empty grid.
        /// </summary>
        [Test]
        public void HasLineOfSight_EmptyGridDistantTiles_ReturnsTrue()
        {
            // Arrange
            var from = new GridPosition(2, 2);
            var to = new GridPosition(20, 2);
            // All tiles are Normal (default) — no blockers.

            // Act
            bool result = LineOfSight.HasLineOfSight(_grid, from, to);

            // Assert
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// HasLineOfSight returns false when a Destroyed tile sits between the observer and target.
        /// Destroyed tiles are impassable and block LOS per the terrain rules.
        /// </summary>
        [Test]
        public void HasLineOfSight_DestroyedTileOnLine_ReturnsFalse()
        {
            // Arrange
            var from = new GridPosition(2, 5);
            var blocker = new GridPosition(8, 5);
            var to = new GridPosition(15, 5);
            _grid.SetTileState(blocker, TileState.Destroyed);

            // Act
            bool result = LineOfSight.HasLineOfSight(_grid, from, to);

            // Assert
            Assert.That(result, Is.False);
        }

        // --- GetLineOfSightTiles ---

        /// <summary>
        /// GetLineOfSightTiles returns all intermediate grid positions on a horizontal line,
        /// not including the start or end positions.
        /// </summary>
        [Test]
        public void GetLineOfSightTiles_HorizontalLine_ReturnsAllIntermediateTiles()
        {
            // Arrange
            var from = new GridPosition(3, 7);
            var to = new GridPosition(8, 7);
            // Intermediate positions: (4,7), (5,7), (6,7), (7,7) — 4 tiles between start and end.
            const int expectedIntermediateCount = 4;

            // Act
            IEnumerable<GridPosition> intermediate = LineOfSight.GetLineOfSightTiles(_grid, from, to);

            // Assert
            Assert.That(intermediate.Count(), Is.EqualTo(expectedIntermediateCount));
            Assert.That(intermediate.All(p => p.Y == 7), Is.True);
            // Must not include the endpoints.
            Assert.That(intermediate.Contains(from), Is.False);
            Assert.That(intermediate.Contains(to), Is.False);
        }

        // --- HasCover ---

        /// <summary>
        /// HasCover returns true when an impassable tile (e.g., Obsidian) is adjacent to the
        /// target on the attacker's side, providing cover.
        /// </summary>
        [Test]
        public void HasCover_ImpassableTileAdjacentToTargetOnAttackerSide_ReturnsTrue()
        {
            // Arrange — attacker at (0,5), target at (10,5), Obsidian cover at (9,5) (one step
            // toward the attacker from the target's side).
            var attackerPos = new GridPosition(0, 5);
            var targetPos = new GridPosition(10, 5);
            var coverTile = new GridPosition(9, 5);
            _grid.SetTileState(coverTile, TileState.Obsidian);

            // Act
            bool hasCover = LineOfSight.HasCover(_grid, attackerPos, targetPos);

            // Assert
            Assert.That(hasCover, Is.True);
        }

        // --- LOS is geometry-only (not blocked by elemental states) ---

        /// <summary>
        /// HasLineOfSight through Wet tiles returns true — Wet is a non-blocking elemental state.
        /// LOS is determined by geometry (impassable blockers and elevation), not elemental states.
        /// </summary>
        [Test]
        public void HasLineOfSight_ThroughWetTiles_IsNotBlocked()
        {
            // Arrange — fill the line from→to with Wet tiles (should not block LOS).
            var from = new GridPosition(1, 1);
            var to = new GridPosition(10, 1);
            for (int x = 2; x < 10; x++)
            {
                _grid.SetTileState(new GridPosition(x, 1), TileState.Wet);
            }

            // Act
            bool result = LineOfSight.HasLineOfSight(_grid, from, to);

            // Assert — Wet does not block LOS
            Assert.That(result, Is.True);
        }
    }
}

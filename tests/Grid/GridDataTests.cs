using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Battlemancers.Core.Grid;

namespace Battlemancers.Tests.Grid
{
    /// <summary>
    /// Tests for GridData — the core battlefield data structure.
    /// Covers construction, tile access, mutation, range queries, line queries, and LOS.
    /// </summary>
    [TestFixture]
    public class GridDataTests
    {
        private GridData _grid;

        [SetUp]
        public void SetUp()
        {
            // Fresh 24×24 grid for each test — standard small-match map size.
            _grid = GridData.Standard24x24();
        }

        // --- Construction ---

        /// <summary>
        /// A 24×24 grid must report Width=24 and Height=24 after construction.
        /// </summary>
        [Test]
        public void Standard24x24_AfterConstruction_HasCorrectDimensions()
        {
            // Arrange / Act — done in SetUp.

            // Assert
            Assert.That(_grid.Width, Is.EqualTo(24));
            Assert.That(_grid.Height, Is.EqualTo(24));
        }

        // --- Tile access ---

        /// <summary>
        /// GetTile at a known in-bounds position returns a non-null Tile at that exact position.
        /// </summary>
        [Test]
        public void GetTile_AtKnownPosition_ReturnsTileAtThatPosition()
        {
            // Arrange
            var pos = new GridPosition(5, 10);

            // Act
            Tile tile = _grid.GetTile(pos);

            // Assert
            Assert.That(tile, Is.Not.Null);
            Assert.That(tile.Position, Is.EqualTo(pos));
        }

        /// <summary>
        /// GetTile at an out-of-bounds position returns null instead of throwing.
        /// </summary>
        [Test]
        public void GetTile_OutOfBoundsPosition_ReturnsNull()
        {
            // Arrange
            var outOfBounds = new GridPosition(100, 100);

            // Act
            Tile tile = _grid.GetTile(outOfBounds);

            // Assert
            Assert.That(tile, Is.Null);
        }

        // --- Tile state mutation ---

        /// <summary>
        /// SetTileState on an in-bounds tile updates that tile's State property to the new value.
        /// </summary>
        [Test]
        public void SetTileState_OnInBoundsTile_UpdatesStateCorrectly()
        {
            // Arrange
            var pos = new GridPosition(3, 3);

            // Act
            _grid.SetTileState(pos, TileState.Burning);

            // Assert
            Assert.That(_grid.GetTile(pos).State, Is.EqualTo(TileState.Burning));
        }

        /// <summary>
        /// IsPassable returns false after setting a tile state to Destroyed.
        /// </summary>
        [Test]
        public void IsPassable_AfterSetStateDestroyed_ReturnsFalse()
        {
            // Arrange
            var pos = new GridPosition(7, 7);

            // Act
            _grid.SetTileState(pos, TileState.Destroyed);

            // Assert
            Assert.That(_grid.IsPassable(pos), Is.False);
        }

        /// <summary>
        /// IsPassable returns false after setting a tile state to Obsidian.
        /// </summary>
        [Test]
        public void IsPassable_AfterSetStateObsidian_ReturnsFalse()
        {
            // Arrange
            var pos = new GridPosition(12, 8);

            // Act
            _grid.SetTileState(pos, TileState.Obsidian);

            // Assert
            Assert.That(_grid.IsPassable(pos), Is.False);
        }

        // --- Occupancy ---

        /// <summary>
        /// SetOccupant followed by IsOccupied returns true; ClearOccupant resets it to false.
        /// </summary>
        [Test]
        public void SetOccupantAndClearOccupant_Roundtrip_UpdatesIsOccupiedCorrectly()
        {
            // Arrange
            var pos = new GridPosition(6, 6);
            const string unitId = "p1_pyromancer_0";

            // Act — set
            _grid.SetOccupant(pos, unitId);

            // Assert — occupied
            Assert.That(_grid.IsOccupied(pos), Is.True);
            Assert.That(_grid.GetOccupantId(pos), Is.EqualTo(unitId));

            // Act — clear
            _grid.ClearOccupant(pos);

            // Assert — unoccupied
            Assert.That(_grid.IsOccupied(pos), Is.False);
        }

        // --- Range queries ---

        /// <summary>
        /// GetTilesInRange with range=3 centered far from edges returns the correct Manhattan diamond count
        /// (1 + 2*(1+2+3) = 13 tiles for range 3: the formula is 1 + 4*(1+2+...+r) = 1 + 4*(r*(r+1)/2) = 2r^2+2r+1).
        /// For r=3: 2*9+6+1 = 25 tiles.
        /// </summary>
        [Test]
        public void GetTilesInRange_Range3CenteredOnGrid_ReturnsCorrectDiamondCount()
        {
            // Arrange
            // Place origin away from all edges so no boundary clipping occurs.
            var origin = new GridPosition(12, 12);
            const int range = 3;
            const int expectedCount = 25; // 2*r^2 + 2*r + 1 = 18 + 6 + 1

            // Act
            List<Tile> tiles = _grid.GetTilesInRange(origin, range).ToList();

            // Assert
            Assert.That(tiles.Count, Is.EqualTo(expectedCount));
        }

        /// <summary>
        /// GetTilesInRadius with radius=2 centered far from edges returns the correct Chebyshev square count
        /// ((2r+1)^2 = 25 tiles for r=2).
        /// </summary>
        [Test]
        public void GetTilesInRadius_Radius2CenteredOnGrid_ReturnsCorrectSquareCount()
        {
            // Arrange
            var origin = new GridPosition(12, 12);
            const int radius = 2;
            // Chebyshev square: (2*radius+1)^2 = 5^2 = 25
            const int expectedCount = 25;

            // Act
            List<Tile> tiles = _grid.GetTilesInRadius(origin, radius).ToList();

            // Assert
            Assert.That(tiles.Count, Is.EqualTo(expectedCount));
        }

        // --- Line queries ---

        /// <summary>
        /// GetLine on a horizontal line from (2,5) to (7,5) returns exactly 6 positions
        /// (start inclusive, end inclusive) all sharing the same Y coordinate.
        /// </summary>
        [Test]
        public void GetLine_HorizontalLine_ReturnsCorrectIntermediateTiles()
        {
            // Arrange
            var from = new GridPosition(2, 5);
            var to = new GridPosition(7, 5);

            // Act
            List<GridPosition> line = _grid.GetLine(from, to).ToList();

            // Assert — 6 positions: x=2,3,4,5,6,7 all at y=5
            Assert.That(line.Count, Is.EqualTo(6));
            Assert.That(line.All(p => p.Y == 5), Is.True);
            Assert.That(line.First(), Is.EqualTo(from));
            Assert.That(line.Last(), Is.EqualTo(to));
        }

        // --- Line of sight ---

        /// <summary>
        /// HasLineOfSight returns false when an Obsidian tile sits on the line between observer and target.
        /// </summary>
        [Test]
        public void HasLineOfSight_ObsidianBlockerOnLine_ReturnsFalse()
        {
            // Arrange
            var from = new GridPosition(5, 5);
            var blocker = new GridPosition(7, 5);
            var to = new GridPosition(10, 5);
            _grid.SetTileState(blocker, TileState.Obsidian);

            // Act
            bool hasLos = _grid.HasLineOfSight(from, to);

            // Assert
            Assert.That(hasLos, Is.False);
        }

        /// <summary>
        /// HasLineOfSight returns true on a clear horizontal line with no intermediate blockers.
        /// </summary>
        [Test]
        public void HasLineOfSight_ClearLine_ReturnsTrue()
        {
            // Arrange
            var from = new GridPosition(1, 1);
            var to = new GridPosition(10, 1);
            // No blockers placed — all tiles are Normal by default.

            // Act
            bool hasLos = _grid.HasLineOfSight(from, to);

            // Assert
            Assert.That(hasLos, Is.True);
        }
    }
}

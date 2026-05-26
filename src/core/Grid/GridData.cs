using System.Collections.Generic;

namespace Battlemancers.Core.Grid
{
    /// <summary>
    /// The core grid data structure. This is the authoritative source of truth for the
    /// battlefield state during simulation.
    ///
    /// Pure C# — zero Unity dependencies. All game logic that reads or modifies the
    /// battlefield (turn resolution, spell targeting, pathfinding cost queries, LOS) must
    /// go through this class. MonoBehaviours read GridData to drive visuals but never mutate
    /// it directly — mutations happen only through the simulation layer.
    ///
    /// GridData is serializable by the replay system: record all mutations as events,
    /// replay them from a fresh GridData to reconstruct any historical state.
    /// </summary>
    public class GridData
    {
        // Internal 2D array indexed as [x, y] — x is column, y is row.
        private readonly Tile[,] _tiles;

        // --- Constructor and factory methods ---

        /// <summary>
        /// Initializes a new GridData with all tiles set to Normal state at elevation 0.
        /// </summary>
        /// <param name="width">Number of columns (X axis).</param>
        /// <param name="height">Number of rows (Y axis).</param>
        public GridData(int width, int height)
        {
            Width = width;
            Height = height;
            _tiles = new Tile[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _tiles[x, y] = new Tile(new GridPosition(x, y));
                }
            }
        }

        /// <summary>Creates a standard 24×24 grid (small/fast match map).</summary>
        public static GridData Standard24x24() => new GridData(24, 24);

        /// <summary>Creates a standard 32×32 grid (default competitive map size).</summary>
        public static GridData Standard32x32() => new GridData(32, 32);

        /// <summary>Creates a standard 48×48 grid (large/extended match map).</summary>
        public static GridData Standard48x48() => new GridData(48, 48);

        // --- Dimensions ---

        /// <summary>Number of columns on the grid (X axis size).</summary>
        public int Width { get; }

        /// <summary>Number of rows on the grid (Y axis size).</summary>
        public int Height { get; }

        // --- Bounds checking ---

        /// <summary>
        /// Returns true if the given position falls within the grid boundaries.
        /// Use this before any tile access when the position may be from an unchecked source
        /// (e.g., neighbor lookups at the edge of the map, spell AoE that extends past borders).
        /// </summary>
        /// <param name="pos">The position to test.</param>
        public bool IsInBounds(GridPosition pos)
        {
            return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height;
        }

        // --- Tile access ---

        /// <summary>
        /// Returns the Tile at the specified position.
        /// Returns null if the position is out of bounds.
        /// All internal methods that receive positions from external sources should
        /// guard with IsInBounds before calling GetTile.
        /// </summary>
        /// <param name="pos">The grid position to retrieve.</param>
        public Tile GetTile(GridPosition pos)
        {
            if (!IsInBounds(pos))
                return null;
            return _tiles[pos.X, pos.Y];
        }

        // --- Tile mutation ---

        /// <summary>
        /// Sets the elemental state of the tile at the given position.
        /// Also updates the tile's IsPassable based on the new state.
        /// No-op if the position is out of bounds.
        /// </summary>
        /// <param name="pos">Target tile position.</param>
        /// <param name="state">The new TileState to apply.</param>
        public void SetTileState(GridPosition pos, TileState state)
        {
            Tile tile = GetTile(pos);
            tile?.SetState(state);
        }

        /// <summary>
        /// Sets the elevation of the tile at the given position.
        /// Elevation: 0 = ground, 1 = raised/hill, 2 = high ground, -1 = pit.
        /// No-op if the position is out of bounds.
        /// </summary>
        /// <param name="pos">Target tile position.</param>
        /// <param name="elevation">New elevation level.</param>
        public void SetElevation(GridPosition pos, int elevation)
        {
            Tile tile = GetTile(pos);
            if (tile != null)
                tile.Elevation = elevation;
        }

        /// <summary>
        /// Marks the tile at the given position as occupied by the specified unit.
        /// Overwrites any existing occupant — the caller is responsible for clearing
        /// the previous tile when a unit moves.
        /// No-op if the position is out of bounds.
        /// </summary>
        /// <param name="pos">Target tile position.</param>
        /// <param name="unitId">The ID of the unit now occupying this tile.</param>
        public void SetOccupant(GridPosition pos, string unitId)
        {
            Tile tile = GetTile(pos);
            if (tile != null)
                tile.OccupantId = unitId;
        }

        /// <summary>
        /// Removes any occupant from the tile at the given position, setting it to unoccupied.
        /// No-op if the position is out of bounds or already unoccupied.
        /// </summary>
        /// <param name="pos">Target tile position.</param>
        public void ClearOccupant(GridPosition pos)
        {
            Tile tile = GetTile(pos);
            if (tile != null)
                tile.OccupantId = null;
        }

        // --- State queries ---

        /// <summary>
        /// Returns true if the tile at the given position is passable.
        /// Out-of-bounds positions are treated as impassable.
        /// </summary>
        public bool IsPassable(GridPosition pos)
        {
            Tile tile = GetTile(pos);
            return tile != null && tile.IsPassable;
        }

        /// <summary>
        /// Returns true if the tile at the given position has a unit on it.
        /// Out-of-bounds positions return false.
        /// </summary>
        public bool IsOccupied(GridPosition pos)
        {
            Tile tile = GetTile(pos);
            return tile != null && tile.IsOccupied;
        }

        /// <summary>
        /// Returns the occupant ID of the unit on the tile at the given position.
        /// Returns null if the tile is unoccupied or out of bounds.
        /// </summary>
        public string GetOccupantId(GridPosition pos)
        {
            return GetTile(pos)?.OccupantId;
        }

        // --- Neighbor queries ---

        /// <summary>
        /// Returns the four cardinal neighbors (N, S, E, W) of the given position.
        /// Skips positions that are out of bounds — the returned set may have 2, 3, or 4
        /// tiles depending on whether the origin is at a grid edge or corner.
        /// </summary>
        /// <param name="pos">The center position.</param>
        public IEnumerable<Tile> GetNeighbors(GridPosition pos)
        {
            GridPosition[] cardinals = {
                pos + GridPosition.Up,
                pos + GridPosition.Down,
                pos + GridPosition.Left,
                pos + GridPosition.Right
            };

            foreach (GridPosition neighbor in cardinals)
            {
                Tile tile = GetTile(neighbor);
                if (tile != null)
                    yield return tile;
            }
        }

        /// <summary>
        /// Returns the four diagonal neighbors (NE, NW, SE, SW) of the given position.
        /// Skips positions that are out of bounds.
        /// </summary>
        /// <param name="pos">The center position.</param>
        public IEnumerable<Tile> GetDiagonalNeighbors(GridPosition pos)
        {
            GridPosition[] diagonals = {
                new GridPosition(pos.X + 1, pos.Y + 1),  // NE
                new GridPosition(pos.X - 1, pos.Y + 1),  // NW
                new GridPosition(pos.X + 1, pos.Y - 1),  // SE
                new GridPosition(pos.X - 1, pos.Y - 1)   // SW
            };

            foreach (GridPosition neighbor in diagonals)
            {
                Tile tile = GetTile(neighbor);
                if (tile != null)
                    yield return tile;
            }
        }

        /// <summary>
        /// Returns all eight neighbors (4 cardinal + 4 diagonal) of the given position.
        /// Skips positions that are out of bounds.
        /// Used for AoE spell effect propagation, fire spread, and flood fill.
        /// </summary>
        /// <param name="pos">The center position.</param>
        public IEnumerable<Tile> GetAllNeighbors(GridPosition pos)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    // Skip the center tile itself.
                    if (dx == 0 && dy == 0)
                        continue;

                    Tile tile = GetTile(new GridPosition(pos.X + dx, pos.Y + dy));
                    if (tile != null)
                        yield return tile;
                }
            }
        }

        // --- Range queries ---

        /// <summary>
        /// Returns all tiles within the given Manhattan distance from the origin.
        /// Manhattan distance = |dx| + |dy|. This produces a diamond shape on the grid.
        /// Used for: movement range display, spell targeting with Manhattan range,
        /// aura effects that don't wrap around corners.
        /// </summary>
        /// <param name="origin">The center of the range.</param>
        /// <param name="range">The maximum Manhattan distance to include.</param>
        public IEnumerable<Tile> GetTilesInRange(GridPosition origin, int range)
        {
            // Iterate only the bounding box of the diamond to avoid checking the
            // full grid width. Clamp to grid boundaries for edge tiles.
            int xMin = System.Math.Max(0, origin.X - range);
            int xMax = System.Math.Min(Width - 1, origin.X + range);
            int yMin = System.Math.Max(0, origin.Y - range);
            int yMax = System.Math.Min(Height - 1, origin.Y + range);

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    GridPosition pos = new GridPosition(x, y);
                    // Include only tiles within the diamond (Manhattan distance check).
                    if (origin.ManhattanDistance(pos) <= range)
                        yield return _tiles[x, y];
                }
            }
        }

        /// <summary>
        /// Returns all tiles within the given Chebyshev (chessboard king-move) radius.
        /// This produces a square shape on the grid, treating diagonal moves as distance 1.
        /// Used for: AoE explosions and effects that spread equally in all 8 directions.
        /// </summary>
        /// <param name="origin">The center of the radius.</param>
        /// <param name="radius">The maximum Chebyshev distance to include.</param>
        public IEnumerable<Tile> GetTilesInRadius(GridPosition origin, int radius)
        {
            int xMin = System.Math.Max(0, origin.X - radius);
            int xMax = System.Math.Min(Width - 1, origin.X + radius);
            int yMin = System.Math.Max(0, origin.Y - radius);
            int yMax = System.Math.Min(Height - 1, origin.Y + radius);

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    // Chebyshev distance: max of absolute deltas in each axis.
                    int dx = System.Math.Abs(x - origin.X);
                    int dy = System.Math.Abs(y - origin.Y);
                    if (System.Math.Max(dx, dy) <= radius)
                        yield return _tiles[x, y];
                }
            }
        }

        /// <summary>
        /// Returns the sequence of GridPositions along the line from 'from' to 'to'
        /// using Bresenham's line algorithm.
        ///
        /// Bresenham's algorithm steps through integer grid coordinates that best approximate
        /// the ideal straight line between two points. This is used for:
        /// - Line-of-sight checks (HasLineOfSight iterates this and checks blockers)
        /// - Linear spell targeting (lightning bolt, piercing shot — tiles the projectile passes through)
        /// - Visualizing targeting lines in the UI
        ///
        /// The sequence includes the 'from' position and ends at 'to'. Both endpoints
        /// are in bounds by definition if called from within valid grid queries, but
        /// the algorithm will skip out-of-bounds positions gracefully.
        /// </summary>
        /// <param name="from">The start position (typically the caster's tile).</param>
        /// <param name="to">The end position (typically the target tile).</param>
        public IEnumerable<GridPosition> GetLine(GridPosition from, GridPosition to)
        {
            int x0 = from.X;
            int y0 = from.Y;
            int x1 = to.X;
            int y1 = to.Y;

            // Delta in each axis.
            int dx = System.Math.Abs(x1 - x0);
            int dy = System.Math.Abs(y1 - y0);

            // Step direction for each axis: +1 or -1 depending on which way we are walking.
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            // Error accumulator. Starts at dx - dy.
            // When |err| exceeds the threshold we step in the minor axis.
            int err = dx - dy;

            while (true)
            {
                GridPosition current = new GridPosition(x0, y0);

                // Yield this position if it is within grid bounds.
                // (Out-of-bounds only happens if 'from' or 'to' is invalid, which
                // should not occur in normal gameplay but is handled defensively.)
                if (IsInBounds(current))
                    yield return current;

                // Stop once we reach the destination.
                if (x0 == x1 && y0 == y1)
                    break;

                // Double the error term to avoid fractional arithmetic.
                int e2 = 2 * err;

                // If the doubled error exceeds -dy, we step in the X direction.
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                // If the doubled error is less than dx, we also step in the Y direction.
                // (Both conditions can trigger in the same iteration for diagonal steps.)
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        // --- State filter queries ---

        /// <summary>
        /// Returns all tiles that currently have the specified TileState.
        /// Iterates the full grid — use sparingly on hot paths.
        /// Primarily used for: terrain tick resolution (find all Burning tiles to spread fire),
        /// status effect processing, and debug inspection.
        /// </summary>
        /// <param name="state">The TileState to search for.</param>
        public IEnumerable<Tile> GetTilesByState(TileState state)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_tiles[x, y].State == state)
                        yield return _tiles[x, y];
                }
            }
        }

        /// <summary>
        /// Returns all tiles that are currently occupied by a unit.
        /// Iterates the full grid — use sparingly on hot paths.
        /// Primarily used for: turn resolution (find all active unit positions),
        /// AoE targeting that hits all units in range.
        /// </summary>
        public IEnumerable<Tile> GetOccupiedTiles()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (_tiles[x, y].IsOccupied)
                        yield return _tiles[x, y];
                }
            }
        }

        // --- Line of sight ---

        /// <summary>
        /// Returns true if there is a clear line of sight from 'from' to 'to'.
        ///
        /// LOS is blocked if any tile along the Bresenham line (excluding the endpoints) is:
        /// <list type="bullet">
        ///   <item><description>Obsidian state — a solid elemental barrier.</description></item>
        ///   <item><description>Has an elevation difference greater than 1 compared to the
        ///   'from' tile — elevated terrain blocks LOS to tiles behind it at lower elevation.</description></item>
        /// </list>
        ///
        /// Note: This is a foundational LOS implementation for the simulation layer.
        /// Additional blocking rules (ice walls don't block LOS, steam clouds reduce vision range,
        /// etc.) are applied by the spell/targeting resolver on top of this base check.
        /// </summary>
        /// <param name="from">The observer's position (caster or unit).</param>
        /// <param name="to">The target position.</param>
        /// <returns>True if LOS is unobstructed; false if blocked.</returns>
        public bool HasLineOfSight(GridPosition from, GridPosition to)
        {
            // Degenerate case: same tile always has LOS to itself.
            if (from == to)
                return true;

            Tile originTile = GetTile(from);
            if (originTile == null)
                return false;

            int originElevation = originTile.Elevation;
            bool first = true;

            foreach (GridPosition pos in GetLine(from, to))
            {
                // Skip the starting tile — we never block ourselves.
                if (first)
                {
                    first = false;
                    continue;
                }

                // Also skip the destination tile — we want to check intermediate tiles only.
                if (pos == to)
                    break;

                Tile tile = GetTile(pos);
                if (tile == null)
                    continue;

                // Obsidian is a solid elemental wall — blocks both movement and LOS.
                if (tile.State == TileState.Obsidian)
                    return false;

                // Elevated terrain blocks LOS if it is more than 1 elevation level higher
                // than the observer's tile. A unit on ground (0) cannot see past a hill (1)
                // that stands directly in the way — they need to be on a hill themselves.
                if (tile.Elevation - originElevation > 1)
                    return false;
            }

            return true;
        }
    }
}

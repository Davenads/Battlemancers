using Battlemancers.Core.Grid;

namespace Battlemancers.Core.Pathfinding
{
    /// <summary>
    /// Static utility helpers for tactical pathfinding queries in the simulation layer.
    /// Zero Unity dependencies — all methods operate on pure C# types.
    ///
    /// This class provides the shared building blocks used by <see cref="MovementRange"/>
    /// and <see cref="LineOfSight"/>: movement cost lookup, passability checks, and
    /// cardinal neighbor enumeration.
    /// </summary>
    public static class PathfindingUtils
    {
        // --- Movement cost table ---

        /// <summary>
        /// Returns the movement cost for a tile with the given <see cref="TileState"/>.
        /// Movement cost represents how many movement points a unit spends to enter a tile.
        /// A unit with a move range of 3 can traverse three Normal tiles, or one Mud tile
        /// and one Normal tile (total cost 3), etc.
        ///
        /// Cost table:
        /// <list type="bullet">
        ///   <item><description><see cref="TileState.Normal"/>     → 1.0  (default unmodified terrain)</description></item>
        ///   <item><description><see cref="TileState.Wet"/>        → 1.5  (surface moisture — slight slow)</description></item>
        ///   <item><description><see cref="TileState.Mud"/>        → 2.0  (heavily penalizes non-flying units)</description></item>
        ///   <item><description><see cref="TileState.Burning"/>    → 1.5  (passable but deals 5 HP/turn on entry — damage handled by sim)</description></item>
        ///   <item><description><see cref="TileState.Frozen"/>     → 0.5  (slippery ice — reduced cost but sliding rules apply)</description></item>
        ///   <item><description><see cref="TileState.Poisoned"/>   → 1.0  (toxic but not physically obstructive)</description></item>
        ///   <item><description><see cref="TileState.Charged"/>    → 1.0  (trap tile — triggers on entry, no movement penalty)</description></item>
        ///   <item><description><see cref="TileState.Obsidian"/>   → 999  (impassable hardened barrier)</description></item>
        ///   <item><description><see cref="TileState.Destroyed"/>  → 999  (impassable pit/void)</description></item>
        ///   <item><description><see cref="TileState.Permafrost"/> → 1.5  (elevated frozen — slippery and slower than Normal)</description></item>
        ///   <item><description><see cref="TileState.Vines"/>      → 2.0  (entangling growth zone)</description></item>
        ///   <item><description><see cref="TileState.Spores"/>     → 1.0  (spore cloud — no physical obstruction)</description></item>
        ///   <item><description><see cref="TileState.Steam"/>      → 1.0  (obscures vision but normal movement)</description></item>
        ///   <item><description><see cref="TileState.Natural"/>    → 1.0  (forest/grass — qualifies for Verdant Pact Terrain Bond)</description></item>
        ///   <item><description><see cref="TileState.Corrupted"/>  → 1.5  (death-soaked earth — slight movement penalty)</description></item>
        /// </list>
        ///
        /// Note: Impassable tile states (Obsidian, Destroyed) return a sentinel cost of 999f.
        /// Callers should check <see cref="IsPassableTileState"/> before using this value
        /// for cost comparisons, or treat any cost ≥ 999f as impassable.
        /// </summary>
        /// <param name="state">The elemental/terrain state of the tile being entered.</param>
        /// <returns>The movement point cost to enter a tile with this state.</returns>
        public static float GetMovementCost(TileState state)
        {
            switch (state)
            {
                case TileState.Normal:     return 1.0f;
                case TileState.Wet:        return 1.5f;
                case TileState.Mud:        return 2.0f;
                case TileState.Burning:    return 1.5f;
                case TileState.Frozen:     return 0.5f;
                case TileState.Poisoned:   return 1.0f;
                case TileState.Charged:    return 1.0f;
                case TileState.Obsidian:   return 999f;
                case TileState.Destroyed:  return 999f;
                case TileState.Permafrost: return 1.5f;
                case TileState.Vines:      return 2.0f;
                case TileState.Spores:     return 1.0f;
                case TileState.Steam:      return 1.0f;
                case TileState.Natural:    return 1.0f;
                case TileState.Corrupted:  return 1.5f;
                default:                   return 1.0f;
            }
        }

        // --- Passability helpers ---

        /// <summary>
        /// Returns whether a tile with the given <see cref="TileState"/> can be entered
        /// at all, ignoring unit occupancy.
        ///
        /// Two states are impassable:
        /// <list type="bullet">
        ///   <item><description><see cref="TileState.Obsidian"/>  — hardened lava barrier; blocks movement and LOS.</description></item>
        ///   <item><description><see cref="TileState.Destroyed"/> — pit/void; entering causes KO.</description></item>
        /// </list>
        ///
        /// All other states are passable by default. Hazardous states (Burning, Poisoned, etc.)
        /// may apply damage or status effects on entry, but those are resolved by the simulation
        /// layer — not by this method.
        ///
        /// Note: This mirrors the logic in <see cref="Tile.SetState"/> and is consistent with
        /// <see cref="Tile.IsPassable"/>. Always prefer checking <see cref="Tile.IsPassable"/>
        /// directly when you have a <see cref="Tile"/> reference; use this method when you only
        /// have a <see cref="TileState"/> value (e.g., when computing costs before tile lookup).
        /// </summary>
        /// <param name="state">The TileState to test.</param>
        /// <returns>False for Obsidian and Destroyed; true for all other states.</returns>
        public static bool IsPassableTileState(TileState state)
        {
            return state != TileState.Obsidian && state != TileState.Destroyed;
        }

        // --- Neighbor enumeration ---

        /// <summary>
        /// Returns all valid (in-bounds) orthogonal neighbors of <paramref name="pos"/>
        /// on the given <paramref name="grid"/>.
        ///
        /// Movement in Battlemancers is 4-directional (N, S, E, W). Diagonal movement is
        /// not permitted for unit pathing. The four cardinal offsets are checked in order:
        /// North (+Y), South (−Y), East (+X), West (−X).
        ///
        /// Only positions that pass <see cref="GridData.IsInBounds"/> are included in the
        /// result. Tiles at grid corners return 2 neighbors; edge tiles return 3; interior
        /// tiles return 4.
        ///
        /// Example — neighbors of (0,0) on a 4×4 grid:
        /// <code>
        /// // Returns (0,1) and (1,0) only — North and East are in-bounds.
        /// GridPosition[] neighbors = PathfindingUtils.GetNeighbors(new GridPosition(0, 0), grid);
        /// </code>
        /// </summary>
        /// <param name="pos">The center position whose neighbors to enumerate.</param>
        /// <param name="grid">The grid used for bounds checking.</param>
        /// <returns>
        /// An array of in-bounds cardinal neighbor positions.
        /// May contain 2, 3, or 4 elements depending on proximity to grid edges.
        /// </returns>
        public static GridPosition[] GetNeighbors(GridPosition pos, GridData grid)
        {
            // Pre-allocate the four cardinal candidates.
            GridPosition north = pos + GridPosition.Up;
            GridPosition south = pos + GridPosition.Down;
            GridPosition east  = pos + GridPosition.Right;
            GridPosition west  = pos + GridPosition.Left;

            // Count valid neighbors first to size the result array exactly.
            int count = 0;
            if (grid.IsInBounds(north)) count++;
            if (grid.IsInBounds(south)) count++;
            if (grid.IsInBounds(east))  count++;
            if (grid.IsInBounds(west))  count++;

            GridPosition[] result = new GridPosition[count];
            int i = 0;
            if (grid.IsInBounds(north)) result[i++] = north;
            if (grid.IsInBounds(south)) result[i++] = south;
            if (grid.IsInBounds(east))  result[i++] = east;
            if (grid.IsInBounds(west))  result[i++] = west;

            return result;
        }
    }
}

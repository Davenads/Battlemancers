using System;
using System.Collections.Generic;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;

namespace Battlemancers.Simulation
{
    /// <summary>
    /// The shape of a spell's target area, used by
    /// <see cref="TargetingUtils.GetTargetTiles"/> to enumerate which tiles a spell affects.
    ///
    /// These shapes correspond to the <see cref="Battlemancers.Data.SpellTargetType"/> values
    /// used in <c>SpellData</c> but are defined here as a pure-C# enum so the simulation
    /// layer has no dependency on the Unity data assembly.
    /// </summary>
    public enum SpellTargetingShape
    {
        /// <summary>
        /// Affects only the single tile that was targeted. Used for focused, high-value
        /// spells (e.g., single-target freeze, direct heal).
        /// </summary>
        Single,

        /// <summary>
        /// Affects all tiles in a straight line from the caster through the target tile,
        /// continuing until <paramref name="range"/> tiles from the origin have been
        /// traversed. Uses Bresenham's line algorithm. Hits all units along the path.
        /// </summary>
        Line,

        /// <summary>
        /// Affects all tiles within <paramref name="range"/> Manhattan distance of the
        /// target tile, producing a diamond-shaped blast zone. This matches the movement
        /// range model used throughout the rest of the simulation.
        /// </summary>
        AoECircle,

        /// <summary>
        /// Affects a 90-degree cone of tiles extending from the caster in the direction
        /// of the target tile, up to <paramref name="range"/> tiles deep. Width at each
        /// depth step equals <c>2 * depth - 1</c> tiles centred on the axis.
        /// </summary>
        Cone,

        /// <summary>
        /// Affects the four cardinal arms (N, S, E, W) radiating from the target tile,
        /// each arm extending <paramref name="range"/> tiles. The centre tile is included.
        /// </summary>
        Cross,

        /// <summary>
        /// Affects all tiles at exactly <paramref name="range"/> Manhattan distance from
        /// the target tile — a hollow diamond ring. The centre tile is NOT included.
        /// Used for spells that create surrounding barriers or delayed-detonation rings.
        /// </summary>
        Ring
    }

    /// <summary>
    /// Static utility methods for resolving spell targeting on the simulation grid.
    ///
    /// All methods are pure functions — they read from <see cref="GridData"/> and
    /// <see cref="SimulationState"/> but never mutate them.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public static class TargetingUtils
    {
        // -----------------------------------------------------------------------------------------
        // GetTargetTiles
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the list of grid positions affected by a spell cast from
        /// <paramref name="origin"/> toward <paramref name="targetPos"/> with the given
        /// <paramref name="shape"/> and <paramref name="range"/>.
        ///
        /// All returned positions are guaranteed to be within the grid bounds of
        /// <paramref name="grid"/> (out-of-bounds positions are silently excluded).
        /// </summary>
        /// <param name="origin">
        /// The caster's current grid position. Used as the starting point for directional
        /// shapes (<see cref="SpellTargetingShape.Line"/>, <see cref="SpellTargetingShape.Cone"/>).
        /// </param>
        /// <param name="targetPos">
        /// The tile the player aimed at. Used as the centre/end-point depending on shape.
        /// </param>
        /// <param name="shape">The geometric pattern of the spell's area of effect.</param>
        /// <param name="range">
        /// Maximum extent of the shape in tiles. For <see cref="SpellTargetingShape.Single"/>
        /// this parameter is unused. For <see cref="SpellTargetingShape.Ring"/> it defines
        /// the exact ring radius.
        /// </param>
        /// <param name="grid">
        /// The battlefield grid used for bounds checking and line tracing. Must not be null.
        /// </param>
        /// <returns>
        /// A new <see cref="List{GridPosition}"/> containing every affected in-bounds tile.
        /// Never null; may be empty if all computed positions fall outside grid bounds.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="grid"/> is null.</exception>
        public static List<GridPosition> GetTargetTiles(
            GridPosition origin,
            GridPosition targetPos,
            SpellTargetingShape shape,
            int range,
            GridData grid)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            switch (shape)
            {
                case SpellTargetingShape.Single:
                    return GetSingleTiles(targetPos, grid);

                case SpellTargetingShape.Line:
                    return GetLineTiles(origin, targetPos, grid);

                case SpellTargetingShape.AoECircle:
                    return GetAoECircleTiles(targetPos, range, grid);

                case SpellTargetingShape.Cone:
                    return GetConeTiles(origin, targetPos, range, grid);

                case SpellTargetingShape.Cross:
                    return GetCrossTiles(targetPos, range, grid);

                case SpellTargetingShape.Ring:
                    return GetRingTiles(targetPos, range, grid);

                default:
                    return GetSingleTiles(targetPos, grid);
            }
        }

        // -----------------------------------------------------------------------------------------
        // IsInRange
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Returns <c>true</c> if <paramref name="target"/> is within
        /// <paramref name="spellRange"/> tiles of <paramref name="caster"/>, measured as
        /// Manhattan distance (consistent with the movement range model).
        /// </summary>
        /// <param name="caster">The caster's grid position.</param>
        /// <param name="target">The tile being targeted.</param>
        /// <param name="spellRange">Maximum allowed Manhattan distance.</param>
        /// <returns>
        /// <c>true</c> when <c>|caster.X - target.X| + |caster.Y - target.Y| &lt;= spellRange</c>.
        /// </returns>
        public static bool IsInRange(GridPosition caster, GridPosition target, int spellRange)
        {
            return caster.ManhattanDistance(target) <= spellRange;
        }

        // -----------------------------------------------------------------------------------------
        // GetUnitsInTiles
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Returns all living units in <paramref name="state"/> whose current grid position
        /// is contained in <paramref name="tiles"/>.
        ///
        /// The returned list preserves the order in which matching units are encountered
        /// while iterating <see cref="SimulationState.GetAllUnits"/> — no specific ordering
        /// is guaranteed.
        /// </summary>
        /// <param name="tiles">
        /// The set of positions to check for unit occupancy. Must not be null.
        /// </param>
        /// <param name="state">
        /// The current simulation state. Must not be null.
        /// </param>
        /// <returns>
        /// A new <see cref="List{UnitState}"/> containing every alive unit whose
        /// <see cref="Battlemancers.Core.Simulation.UnitState.Position"/> appears in
        /// <paramref name="tiles"/>. Never null; may be empty.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="tiles"/> or <paramref name="state"/> is null.
        /// </exception>
        public static List<UnitState> GetUnitsInTiles(List<GridPosition> tiles, SimulationState state)
        {
            if (tiles == null) throw new ArgumentNullException(nameof(tiles));
            if (state == null) throw new ArgumentNullException(nameof(state));

            // Build a hash set for O(1) position lookup when the tile list is large.
            var tileSet = new HashSet<GridPosition>(tiles);
            var result = new List<UnitState>();

            foreach (UnitState unit in state.GetAllUnits())
            {
                if (unit.IsAlive && tileSet.Contains(unit.Position))
                    result.Add(unit);
            }

            return result;
        }

        // -----------------------------------------------------------------------------------------
        // Private shape helpers
        // -----------------------------------------------------------------------------------------

        /// <summary>Returns just the target tile if it is in bounds.</summary>
        private static List<GridPosition> GetSingleTiles(GridPosition targetPos, GridData grid)
        {
            var result = new List<GridPosition>(1);
            if (grid.IsInBounds(targetPos))
                result.Add(targetPos);
            return result;
        }

        /// <summary>
        /// Returns all tiles along the Bresenham line from <paramref name="origin"/> to
        /// <paramref name="targetPos"/>, inclusive of both endpoints.
        /// Out-of-bounds positions are excluded (handled by GridData.GetLine).
        /// </summary>
        private static List<GridPosition> GetLineTiles(
            GridPosition origin, GridPosition targetPos, GridData grid)
        {
            var result = new List<GridPosition>();
            foreach (GridPosition pos in grid.GetLine(origin, targetPos))
                result.Add(pos);
            return result;
        }

        /// <summary>
        /// Returns all in-bounds tiles within <paramref name="range"/> Manhattan distance
        /// of <paramref name="centre"/>, producing a diamond shape.
        /// </summary>
        private static List<GridPosition> GetAoECircleTiles(
            GridPosition centre, int range, GridData grid)
        {
            var result = new List<GridPosition>();
            foreach (Tile tile in grid.GetTilesInRange(centre, range))
                result.Add(tile.Position);
            return result;
        }

        /// <summary>
        /// Returns all in-bounds tiles within a 90-degree cone extending from
        /// <paramref name="origin"/> toward <paramref name="targetPos"/>.
        ///
        /// The dominant axis (X or Y) is determined by comparing |dx| and |dy|.
        /// At each depth step d (1..<paramref name="range"/>), the cone covers tiles
        /// in the dominant axis direction by d steps, and in the minor axis by up to
        /// (d-1) steps in either direction — giving a widening wedge.
        /// </summary>
        private static List<GridPosition> GetConeTiles(
            GridPosition origin, GridPosition targetPos, int range, GridData grid)
        {
            var result = new List<GridPosition>();

            int dx = targetPos.X - origin.X;
            int dy = targetPos.Y - origin.Y;

            // Degenerate case: caster and target on the same tile — treat as Single.
            if (dx == 0 && dy == 0)
            {
                if (grid.IsInBounds(origin))
                    result.Add(origin);
                return result;
            }

            // Determine the primary direction (step) and which axis is dominant.
            int stepX = Math.Sign(dx);
            int stepY = Math.Sign(dy);
            bool xDominant = Math.Abs(dx) >= Math.Abs(dy);

            for (int depth = 1; depth <= range; depth++)
            {
                // Centre tile at this depth along the primary axis.
                int cx, cy;
                if (xDominant)
                {
                    cx = origin.X + stepX * depth;
                    cy = origin.Y;
                }
                else
                {
                    cx = origin.X;
                    cy = origin.Y + stepY * depth;
                }

                // The cone widens by 1 tile on each side per depth step in the minor axis.
                int halfWidth = depth - 1;

                for (int offset = -halfWidth; offset <= halfWidth; offset++)
                {
                    GridPosition pos;
                    if (xDominant)
                        pos = new GridPosition(cx, cy + (stepY != 0 ? stepY * Math.Abs(offset) : offset));
                    else
                        pos = new GridPosition(cx + (stepX != 0 ? stepX * Math.Abs(offset) : offset), cy);

                    if (grid.IsInBounds(pos) && !result.Contains(pos))
                        result.Add(pos);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all in-bounds tiles on the four cardinal arms radiating from
        /// <paramref name="centre"/>, each <paramref name="range"/> tiles long.
        /// The centre tile itself is also included.
        /// </summary>
        private static List<GridPosition> GetCrossTiles(
            GridPosition centre, int range, GridData grid)
        {
            var result = new List<GridPosition>();

            // Centre tile.
            if (grid.IsInBounds(centre))
                result.Add(centre);

            GridPosition[] directions = {
                GridPosition.Up,
                GridPosition.Down,
                GridPosition.Left,
                GridPosition.Right
            };

            foreach (GridPosition dir in directions)
            {
                for (int i = 1; i <= range; i++)
                {
                    GridPosition pos = new GridPosition(
                        centre.X + dir.X * i,
                        centre.Y + dir.Y * i);

                    if (grid.IsInBounds(pos))
                        result.Add(pos);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all in-bounds tiles at exactly <paramref name="range"/> Manhattan
        /// distance from <paramref name="centre"/> — a hollow diamond ring.
        /// The centre tile is NOT included.
        /// </summary>
        private static List<GridPosition> GetRingTiles(
            GridPosition centre, int range, GridData grid)
        {
            var result = new List<GridPosition>();

            if (range <= 0)
                return result;

            // Walk the perimeter of the Manhattan diamond at the given radius.
            // Start at the top (centre.X, centre.Y + range) and walk clockwise.
            GridPosition[] offsets = {
                new GridPosition( 1, -1),   // down-right
                new GridPosition(-1, -1),   // down-left
                new GridPosition(-1,  1),   // up-left
                new GridPosition( 1,  1)    // up-right
            };

            int x = centre.X;
            int y = centre.Y + range;

            for (int side = 0; side < 4; side++)
            {
                for (int step = 0; step < range; step++)
                {
                    GridPosition pos = new GridPosition(x, y);
                    if (grid.IsInBounds(pos))
                        result.Add(pos);
                    x += offsets[side].X;
                    y += offsets[side].Y;
                }
            }

            return result;
        }
    }
}

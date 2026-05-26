using System.Collections.Generic;
using Battlemancers.Core.Grid;

namespace Battlemancers.Core.Pathfinding
{
    /// <summary>
    /// Tactical line-of-sight utilities for the simulation layer.
    /// Implements Bresenham's line algorithm to check visibility and enumerate
    /// tiles along a targeting line between two grid positions.
    ///
    /// Zero Unity dependencies — all methods operate on pure C# types.
    ///
    /// LoS is about geometry only. Unit occupancy never blocks LoS — only terrain
    /// features do. Spell effects that reduce vision range (Steam clouds, etc.) are
    /// additional modifiers applied by the spell resolver on top of these base checks.
    /// </summary>
    public static class LineOfSight
    {
        // -----------------------------------------------------------------------
        //  Line-of-sight check
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns true if there is a clear line of sight from <paramref name="from"/>
        /// to <paramref name="to"/> on the given <paramref name="grid"/>.
        ///
        /// Uses Bresenham's line algorithm to enumerate intermediate tiles between the
        /// two endpoints. LoS is evaluated on the intermediate tiles only — source and
        /// destination tiles are never treated as blockers.
        ///
        /// LoS is BLOCKED if any intermediate tile satisfies either condition:
        /// <list type="bullet">
        ///   <item><description>The tile's <see cref="Tile.IsPassable"/> is false (Obsidian or Destroyed
        ///   tile states are solid barriers that block both movement and sight).</description></item>
        ///   <item><description>The tile's elevation exceeds the observer's elevation by more than 1
        ///   (elevated terrain physically rises above the sightline to a lower target).</description></item>
        /// </list>
        ///
        /// LoS is NOT blocked by:
        /// <list type="bullet">
        ///   <item><description>Units occupying tiles — units do not block sight for other units.</description></item>
        ///   <item><description>Status tile states such as Wet, Burning, Poisoned, Charged — LoS
        ///   is purely geometric; hazardous surfaces do not obscure vision.</description></item>
        ///   <item><description>Steam — Steam reduces effective vision range (handled by the spell
        ///   resolver layer) but does not cause this method to return false.</description></item>
        /// </list>
        ///
        /// Example:
        /// <code>
        /// // Check if a Pyromancer at (2, 2) can target a unit at (5, 4).
        /// bool canSee = LineOfSight.HasLineOfSight(grid, new GridPosition(2, 2), new GridPosition(5, 4));
        /// </code>
        /// </summary>
        /// <param name="grid">The battlefield grid to query.</param>
        /// <param name="from">The observer's grid position (caster or targeting unit).</param>
        /// <param name="to">The target grid position.</param>
        /// <returns>True if the line of sight is unobstructed; false if it is blocked.</returns>
        public static bool HasLineOfSight(GridData grid, GridPosition from, GridPosition to)
        {
            // Same tile always has LoS to itself.
            if (from == to)
                return true;

            Tile originTile = grid.GetTile(from);
            if (originTile == null)
                return false;

            int originElevation = originTile.Elevation;

            List<GridPosition> line = BresenhamLine(from, to);

            // Skip first (from) and last (to) positions — only intermediate tiles block.
            for (int i = 1; i < line.Count - 1; i++)
            {
                GridPosition pos = line[i];
                Tile tile = grid.GetTile(pos);

                // Out-of-bounds positions on the line are treated as clear
                // (this only happens if from/to are at unusual positions, handled defensively).
                if (tile == null)
                    continue;

                // Impassable terrain (Obsidian, Destroyed) is a solid physical barrier.
                if (!tile.IsPassable)
                    return false;

                // Elevated terrain blocks LoS if it rises more than 1 level above the observer.
                // A ground-level observer (0) cannot see past a hill (2) standing between them
                // and their target. They can see past or over a tile only 1 level higher (1).
                if (tile.Elevation - originElevation > 1)
                    return false;
            }

            return true;
        }

        // -----------------------------------------------------------------------
        //  Line tile enumeration
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns all tiles the line from <paramref name="from"/> to <paramref name="to"/>
        /// passes through, excluding the source and destination endpoints.
        ///
        /// Uses Bresenham's line algorithm. The returned tiles are ordered from
        /// <paramref name="from"/> toward <paramref name="to"/>.
        ///
        /// Use this for line-targeting spells that affect all tiles (and units on them)
        /// along their path:
        /// <list type="bullet">
        ///   <item><description>Lightning bolt — chains through every tile on the line.</description></item>
        ///   <item><description>Piercing projectiles — hit all units in a straight line.</description></item>
        ///   <item><description>Beam spells — continuous damage along the trajectory.</description></item>
        /// </list>
        ///
        /// Example:
        /// <code>
        /// // Get all tiles a lightning bolt passes through from (1, 1) to (5, 3).
        /// List&lt;GridPosition&gt; hitTiles = LineOfSight.GetLineOfSightTiles(
        ///     grid,
        ///     new GridPosition(1, 1),
        ///     new GridPosition(5, 3));
        /// // Apply spell effects to each tile and any unit occupying it.
        /// foreach (GridPosition pos in hitTiles)
        ///     ApplyLightningEffect(pos);
        /// </code>
        /// </summary>
        /// <param name="grid">The battlefield grid (used for bounds validation).</param>
        /// <param name="from">The origin of the line (caster position).</param>
        /// <param name="to">The end of the line (target position).</param>
        /// <returns>
        /// An ordered list of intermediate <see cref="GridPosition"/> values between
        /// <paramref name="from"/> and <paramref name="to"/>, exclusive of both endpoints.
        /// Returns an empty list if the two positions are the same or adjacent with no
        /// intermediate tiles.
        /// </returns>
        public static List<GridPosition> GetLineOfSightTiles(
            GridData grid,
            GridPosition from,
            GridPosition to)
        {
            List<GridPosition> fullLine = BresenhamLine(from, to);
            List<GridPosition> intermediate = new List<GridPosition>(System.Math.Max(0, fullLine.Count - 2));

            // Exclude first (from) and last (to) positions.
            for (int i = 1; i < fullLine.Count - 1; i++)
            {
                GridPosition pos = fullLine[i];
                // Only include positions that are in-bounds on the provided grid.
                if (grid.IsInBounds(pos))
                    intermediate.Add(pos);
            }

            return intermediate;
        }

        // -----------------------------------------------------------------------
        //  Cover detection
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns true if the <paramref name="target"/> is in partial cover relative
        /// to the <paramref name="attacker"/>.
        ///
        /// Partial cover is defined as: at least one tile adjacent to the target on the
        /// attacker's side is impassable (i.e., its <see cref="Tile.IsPassable"/> is false).
        /// This represents a wall, obsidian pillar, or similar solid obstacle that the
        /// defender is sheltering behind.
        ///
        /// "Attacker's side" means the adjacent tile whose position falls between the
        /// attacker and the target — specifically, the cardinal neighbor of the target
        /// whose direction vector most closely aligns with the vector from target to attacker.
        ///
        /// Partial cover provides a damage reduction to the target (the exact reduction —
        /// e.g., 50% for Rubble cover as per design — is applied by the simulation resolver).
        /// This method only detects whether cover exists.
        ///
        /// Example:
        /// <code>
        /// // Check if a unit at (4, 4) has cover from an attacker at (1, 4).
        /// // If tile (3, 4) is Obsidian or otherwise impassable, HasCover returns true.
        /// bool covered = LineOfSight.HasCover(grid, new GridPosition(1, 4), new GridPosition(4, 4));
        /// </code>
        /// </summary>
        /// <param name="grid">The battlefield grid to query.</param>
        /// <param name="attacker">The attacking unit's position.</param>
        /// <param name="target">The defending unit's position.</param>
        /// <returns>True if the target has at least one impassable cover tile on the attacker's side.</returns>
        public static bool HasCover(GridData grid, GridPosition attacker, GridPosition target)
        {
            // Compute the direction vector from attacker to target.
            int dx = target.X - attacker.X;
            int dy = target.Y - attacker.Y;

            // Determine which cardinal neighbors of the target face toward the attacker.
            // A neighbor faces the attacker if moving from the target toward that neighbor
            // brings us closer to the attacker (dot product with attack direction > 0,
            // rephrased: neighbor is on the attacker-facing side of target).
            //
            // We check the two most relevant cardinal neighbors based on the dominant axis,
            // then include the other axis if the shot is diagonal.

            GridPosition[] coverCandidates = GetCoverCandidates(target, dx, dy);

            foreach (GridPosition candidate in coverCandidates)
            {
                if (!grid.IsInBounds(candidate))
                    continue;

                Tile tile = grid.GetTile(candidate);
                if (tile != null && !tile.IsPassable)
                    return true;
            }

            return false;
        }

        // -----------------------------------------------------------------------
        //  Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Runs Bresenham's line algorithm from <paramref name="from"/> to <paramref name="to"/>,
        /// returning an ordered list of all grid positions on the line including both endpoints.
        ///
        /// The algorithm steps through integer coordinates that best approximate the ideal
        /// straight line between the two points. It uses an error-accumulator to decide
        /// when to step in the secondary axis, producing the most natural-looking discrete
        /// line on a grid.
        ///
        /// This is the canonical Bresenham implementation used by both
        /// <see cref="HasLineOfSight"/> and <see cref="GetLineOfSightTiles"/>. It matches
        /// the implementation in <see cref="GridData.GetLine"/> but returns a list rather
        /// than an IEnumerable to allow direct index-based access (needed for excluding
        /// endpoints efficiently).
        /// </summary>
        private static List<GridPosition> BresenhamLine(GridPosition from, GridPosition to)
        {
            List<GridPosition> line = new List<GridPosition>();

            int x0 = from.X;
            int y0 = from.Y;
            int x1 = to.X;
            int y1 = to.Y;

            int dx = System.Math.Abs(x1 - x0);
            int dy = System.Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                line.Add(new GridPosition(x0, y0));

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;

                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }

            return line;
        }

        /// <summary>
        /// Returns the cardinal neighbor positions of <paramref name="target"/> that face
        /// toward the attacker, given the attack direction vector (dx, dy).
        ///
        /// The cover candidates are the neighbors of the target that lie in the direction
        /// of the attacker — i.e., if the attacker is to the west, the cover tile would be
        /// the tile directly west of the target (between attacker and target).
        ///
        /// For purely horizontal or vertical shots, returns 1 candidate.
        /// For diagonal shots, returns 2 candidates (both relevant axes).
        /// </summary>
        private static GridPosition[] GetCoverCandidates(GridPosition target, int dx, int dy)
        {
            // Normalise direction components to -1, 0, or +1.
            int nx = dx == 0 ? 0 : (dx > 0 ? -1 : 1);  // face toward attacker: invert
            int ny = dy == 0 ? 0 : (dy > 0 ? -1 : 1);  // face toward attacker: invert

            if (nx != 0 && ny != 0)
            {
                // Diagonal attack — two cover candidates (one per axis).
                return new GridPosition[]
                {
                    new GridPosition(target.X + nx, target.Y),
                    new GridPosition(target.X,      target.Y + ny)
                };
            }
            else if (nx != 0)
            {
                return new GridPosition[] { new GridPosition(target.X + nx, target.Y) };
            }
            else if (ny != 0)
            {
                return new GridPosition[] { new GridPosition(target.X, target.Y + ny) };
            }
            else
            {
                // Attacker and target are on the same tile — no cover direction.
                return System.Array.Empty<GridPosition>();
            }
        }
    }
}

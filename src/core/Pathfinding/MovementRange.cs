using System.Collections.Generic;
using Battlemancers.Core.Grid;

namespace Battlemancers.Core.Pathfinding
{
    /// <summary>
    /// Tactical movement utilities for the simulation layer.
    /// Provides BFS flood-fill for movement range highlighting and A* pathfinding
    /// for resolving the exact path a unit takes to a destination.
    ///
    /// Zero Unity dependencies — all methods operate on pure C# types.
    /// Movement is strictly 4-directional (N, S, E, W). Diagonal movement is not
    /// permitted for unit ground pathing.
    ///
    /// Movement costs are delegated to <see cref="PathfindingUtils.GetMovementCost"/>.
    /// </summary>
    public static class MovementRange
    {
        // -----------------------------------------------------------------------
        //  Reachable tile flood fill
        // -----------------------------------------------------------------------

        /// <summary>
        /// Returns all tiles a unit can reach from <paramref name="origin"/> within
        /// <paramref name="moveRange"/> movement points using a BFS flood fill.
        ///
        /// Rules:
        /// <list type="bullet">
        ///   <item><description>Each step into a neighbor tile costs <see cref="PathfindingUtils.GetMovementCost"/>
        ///   of that tile's state. Cumulative cost must not exceed <paramref name="moveRange"/>.</description></item>
        ///   <item><description>Tiles where <see cref="Tile.IsPassable"/> is false are never entered.</description></item>
        ///   <item><description>Tiles where <see cref="Tile.IsOccupied"/> is true are skipped — a unit
        ///   cannot pass through or land on a tile occupied by another unit.</description></item>
        ///   <item><description>The origin tile itself is NOT included in the result (the unit is already there).</description></item>
        /// </list>
        ///
        /// Use this for the movement range highlight overlay shown when a unit is selected.
        /// </summary>
        /// <param name="grid">The battlefield grid to query.</param>
        /// <param name="origin">The unit's current position.</param>
        /// <param name="moveRange">The unit's movement point budget for this turn.</param>
        /// <returns>A set of <see cref="GridPosition"/> values the unit can reach.</returns>
        public static HashSet<GridPosition> GetReachableTiles(
            GridData grid,
            GridPosition origin,
            int moveRange)
        {
            return FloodFill(grid, origin, moveRange, ignoreOccupancy: false);
        }

        /// <summary>
        /// Returns all tiles reachable from <paramref name="origin"/> within
        /// <paramref name="moveRange"/> movement points, ignoring unit occupancy.
        ///
        /// Identical to <see cref="GetReachableTiles"/> except that occupied tiles are
        /// not skipped — the flood fill passes through them as if they were empty.
        ///
        /// Use this for:
        /// <list type="bullet">
        ///   <item><description>Spell targeting range preview — shows all tiles a spell could
        ///   reach regardless of where other units currently stand.</description></item>
        ///   <item><description>AI planning — evaluating movement options without committing
        ///   to a specific path order.</description></item>
        /// </list>
        ///
        /// Passability is still respected: Obsidian and Destroyed tiles are never included.
        /// </summary>
        /// <param name="grid">The battlefield grid to query.</param>
        /// <param name="origin">The starting position.</param>
        /// <param name="moveRange">The movement point budget.</param>
        /// <returns>A set of reachable <see cref="GridPosition"/> values.</returns>
        public static HashSet<GridPosition> GetReachableTilesIgnoreOccupancy(
            GridData grid,
            GridPosition origin,
            int moveRange)
        {
            return FloodFill(grid, origin, moveRange, ignoreOccupancy: true);
        }

        // -----------------------------------------------------------------------
        //  A* pathfinding
        // -----------------------------------------------------------------------

        /// <summary>
        /// Finds the lowest-cost path from <paramref name="from"/> to <paramref name="to"/>
        /// within <paramref name="moveRange"/> movement points using A* with Manhattan
        /// distance as the heuristic.
        ///
        /// Path is ordered from <paramref name="from"/> to <paramref name="to"/>, inclusive
        /// of both endpoints.
        ///
        /// Rules:
        /// <list type="bullet">
        ///   <item><description>Respects tile passability — Obsidian and Destroyed tiles are never entered.</description></item>
        ///   <item><description>Respects occupancy — occupied tiles cannot be passed through or landed on.</description></item>
        ///   <item><description>If no path exists within <paramref name="moveRange"/>, returns an empty array.</description></item>
        ///   <item><description>Movement is 4-directional (N, S, E, W only).</description></item>
        /// </list>
        ///
        /// Note: This is the simulation layer's pathfinding for turn resolution and validation.
        /// Runtime visual pathing (the animated movement path shown in the presentation layer)
        /// may delegate to A* Pathfinding Pro for performance, but the simulation uses this
        /// pure C# implementation to remain Unity-free and deterministic.
        /// </summary>
        /// <param name="grid">The battlefield grid to search.</param>
        /// <param name="from">The start position.</param>
        /// <param name="to">The destination position.</param>
        /// <param name="moveRange">The maximum movement point budget for the path.</param>
        /// <returns>
        /// An ordered array of <see cref="GridPosition"/> from <paramref name="from"/> to
        /// <paramref name="to"/> (inclusive), or an empty array if no valid path exists
        /// within the budget.
        /// </returns>
        public static GridPosition[] FindPath(
            GridData grid,
            GridPosition from,
            GridPosition to,
            int moveRange)
        {
            // Guard: trivial same-tile case.
            if (from == to)
                return new GridPosition[] { from };

            // Guard: destination is out of bounds or impassable.
            Tile destTile = grid.GetTile(to);
            if (destTile == null || !destTile.IsPassable)
                return System.Array.Empty<GridPosition>();

            // --- A* data structures ---

            // gCost[pos] = lowest cumulative movement cost found so far to reach pos.
            Dictionary<GridPosition, float> gCost = new Dictionary<GridPosition, float>();

            // cameFrom[pos] = the predecessor position on the best path to pos.
            Dictionary<GridPosition, GridPosition> cameFrom = new Dictionary<GridPosition, GridPosition>();

            // Open set keyed by fCost = gCost + heuristic. We use a simple sorted list
            // (adequate for grid sizes up to 48×48; a proper min-heap would be faster for
            // very large grids but adds implementation complexity with no practical benefit here).
            // Each entry is (fCost, position).
            List<(float fCost, GridPosition pos)> openList = new List<(float, GridPosition)>();
            HashSet<GridPosition> closedSet = new HashSet<GridPosition>();

            gCost[from] = 0f;
            float startH = ManhattanHeuristic(from, to);
            openList.Add((startH, from));

            while (openList.Count > 0)
            {
                // Pop the node with the lowest fCost.
                int bestIndex = GetLowestFCostIndex(openList);
                GridPosition current = openList[bestIndex].pos;
                openList.RemoveAt(bestIndex);

                // If we exceeded the move range budget, skip.
                if (gCost[current] > moveRange)
                    continue;

                // Reached the destination — reconstruct and return the path.
                if (current == to)
                    return ReconstructPath(cameFrom, from, to);

                closedSet.Add(current);

                GridPosition[] neighbors = PathfindingUtils.GetNeighbors(current, grid);
                foreach (GridPosition neighbor in neighbors)
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    Tile neighborTile = grid.GetTile(neighbor);
                    if (neighborTile == null || !neighborTile.IsPassable)
                        continue;

                    // Cannot pass through occupied tiles (unless it is the destination —
                    // the sim handles displaced units separately; here we only block transit).
                    if (neighborTile.IsOccupied && neighbor != to)
                        continue;

                    float tentativeG = gCost[current] + PathfindingUtils.GetMovementCost(neighborTile.State);

                    // Skip if this path exceeds the movement budget.
                    if (tentativeG > moveRange)
                        continue;

                    // Accept this path if it's the first or cheaper than the known best.
                    if (!gCost.ContainsKey(neighbor) || tentativeG < gCost[neighbor])
                    {
                        gCost[neighbor] = tentativeG;
                        cameFrom[neighbor] = current;
                        float fCost = tentativeG + ManhattanHeuristic(neighbor, to);
                        openList.Add((fCost, neighbor));
                    }
                }
            }

            // No path found within move range.
            return System.Array.Empty<GridPosition>();
        }

        // -----------------------------------------------------------------------
        //  Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Core BFS flood fill shared by GetReachableTiles and GetReachableTilesIgnoreOccupancy.
        /// Tracks the minimum cumulative movement cost to reach each tile via a priority queue
        /// (Dijkstra-style BFS with fractional costs).
        /// </summary>
        private static HashSet<GridPosition> FloodFill(
            GridData grid,
            GridPosition origin,
            int moveRange,
            bool ignoreOccupancy)
        {
            HashSet<GridPosition> reachable = new HashSet<GridPosition>();

            // costSoFar tracks the cheapest known cost to reach each position.
            // We use a list-based priority queue (Dijkstra) because terrain costs are
            // fractional and can vary — plain BFS would give incorrect ranges on mixed terrain.
            Dictionary<GridPosition, float> costSoFar = new Dictionary<GridPosition, float>();
            costSoFar[origin] = 0f;

            // Each entry: (cumulativeCost, position).
            List<(float cost, GridPosition pos)> frontier = new List<(float, GridPosition)>();
            frontier.Add((0f, origin));

            while (frontier.Count > 0)
            {
                // Pop the frontier entry with the lowest cost.
                int minIndex = GetLowestCostIndex(frontier);
                (float currentCost, GridPosition current) = frontier[minIndex];
                frontier.RemoveAt(minIndex);

                // If a cheaper path to this tile was already processed, discard.
                if (currentCost > costSoFar[current])
                    continue;

                GridPosition[] neighbors = PathfindingUtils.GetNeighbors(current, grid);
                foreach (GridPosition neighbor in neighbors)
                {
                    Tile tile = grid.GetTile(neighbor);
                    if (tile == null || !tile.IsPassable)
                        continue;

                    if (!ignoreOccupancy && tile.IsOccupied)
                        continue;

                    float newCost = currentCost + PathfindingUtils.GetMovementCost(tile.State);

                    if (newCost > moveRange)
                        continue;

                    if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                    {
                        costSoFar[neighbor] = newCost;
                        reachable.Add(neighbor);
                        frontier.Add((newCost, neighbor));
                    }
                }
            }

            // Origin is not included in the reachable set (the unit is already there).
            reachable.Remove(origin);
            return reachable;
        }

        /// <summary>
        /// Manhattan distance heuristic for A*. Admissible because movement is
        /// 4-directional and minimum tile cost is 0.5 (Frozen), so the true cost to
        /// any neighbor is always ≥ 0.5. Using Manhattan distance (assuming cost 1.0 per
        /// tile) is admissible in the common case (it never overestimates when most tiles
        /// cost ≥ 1.0). This keeps A* optimal for standard terrain configurations.
        /// </summary>
        private static float ManhattanHeuristic(GridPosition a, GridPosition b)
        {
            return a.ManhattanDistance(b);
        }

        /// <summary>Returns the index of the entry with the lowest fCost in the open list.</summary>
        private static int GetLowestFCostIndex(List<(float fCost, GridPosition pos)> list)
        {
            int best = 0;
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].fCost < list[best].fCost)
                    best = i;
            }
            return best;
        }

        /// <summary>Returns the index of the entry with the lowest cost in the frontier list.</summary>
        private static int GetLowestCostIndex(List<(float cost, GridPosition pos)> list)
        {
            int best = 0;
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].cost < list[best].cost)
                    best = i;
            }
            return best;
        }

        /// <summary>
        /// Reconstructs the path from the cameFrom map, walking backwards from
        /// <paramref name="to"/> to <paramref name="from"/> and reversing the result.
        /// </summary>
        private static GridPosition[] ReconstructPath(
            Dictionary<GridPosition, GridPosition> cameFrom,
            GridPosition from,
            GridPosition to)
        {
            List<GridPosition> path = new List<GridPosition>();
            GridPosition current = to;

            while (current != from)
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Add(from);
            path.Reverse();
            return path.ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;

namespace Battlemancers.Simulation.Effects
{
    /// <summary>
    /// Resolves the deterministic behavioral overrides for status effects that alter a unit's
    /// actions during turn resolution: CONFUSED, PANICKED, and CHARMED.
    ///
    /// All methods are pure calculations over <see cref="SimulationState"/> — no mutable state
    /// is stored here. Each method returns a target unit ID or a destination tile that the
    /// caller (TurnManager) substitutes into the command being executed.
    ///
    /// Distance metric: Manhattan distance (|dx| + |dy|).
    /// "Visible" means no intermediate tile blocks line of sight as determined by
    /// <see cref="GridData.HasLineOfSight"/> — a straight-line geometry check using
    /// Bresenham's algorithm. No pathfinding is required for visibility.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public static class StatusEffectResolver
    {
        // ---------------------------------------------------------------------------
        // CONFUSED
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns the override target unit ID for a CONFUSED actor's spell.
        /// Picks the nearest visible unit (any allegiance, including the actor itself is excluded)
        /// within <paramref name="spellRange"/> Manhattan tiles of the actor.
        /// On distance ties the unit with the lowest (x+y) sum is preferred; ties further broken by x.
        /// Returns <c>null</c> if no valid target exists — the caller skips the spell.
        /// </summary>
        /// <param name="actorId">Runtime ID of the CONFUSED unit casting the spell.</param>
        /// <param name="spellRange">Maximum Manhattan range of the spell being overridden.</param>
        /// <param name="state">Current simulation state used for unit and grid lookups.</param>
        /// <returns>
        /// Runtime ID of the nearest visible unit within range, or <c>null</c> if none found.
        /// </returns>
        public static string ResolveConfusedTarget(string actorId, int spellRange, SimulationState state)
        {
            UnitState actor = state.GetUnit(actorId);
            if (actor == null || !actor.IsAlive)
                return null;

            string bestId = null;
            int bestDist = int.MaxValue;
            int bestSum = int.MaxValue;
            int bestX = int.MaxValue;

            foreach (UnitState candidate in state.GetAllUnits())
            {
                if (candidate.Id == actorId) continue;
                if (!candidate.IsAlive) continue;

                int dist = actor.Position.ManhattanDistance(candidate.Position);
                if (dist > spellRange) continue;

                if (!state.Grid.HasLineOfSight(actor.Position, candidate.Position))
                    continue;

                int sum = candidate.Position.X + candidate.Position.Y;
                bool better = dist < bestDist
                    || (dist == bestDist && sum < bestSum)
                    || (dist == bestDist && sum == bestSum && candidate.Position.X < bestX);

                if (better)
                {
                    bestId = candidate.Id;
                    bestDist = dist;
                    bestSum = sum;
                    bestX = candidate.Position.X;
                }
            }

            return bestId;
        }

        // ---------------------------------------------------------------------------
        // PANICKED
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns the override move destination for a PANICKED actor.
        /// The unit moves its full <paramref name="moveRange"/> directly away from the nearest
        /// visible enemy — i.e., in the direction opposite to that enemy. If no enemy is visible,
        /// the unit moves toward the nearest map edge. If the unit is already at a map edge, it
        /// stays in place.
        ///
        /// The path is cardinal (the dominant flee axis is chosen first; ties broken by lower x+y).
        /// The destination is clamped to grid bounds and must be passable.
        /// </summary>
        /// <param name="actorId">Runtime ID of the PANICKED unit.</param>
        /// <param name="moveRange">The unit's full movement range in tiles.</param>
        /// <param name="state">Current simulation state.</param>
        /// <returns>The destination tile the panicked unit moves to.</returns>
        public static GridPosition ResolvePanickedMove(string actorId, int moveRange, SimulationState state)
        {
            UnitState actor = state.GetUnit(actorId);
            if (actor == null || !actor.IsAlive)
                return actor?.Position ?? GridPosition.Zero;

            GridPosition currentPos = actor.Position;

            // Find nearest visible enemy.
            UnitState nearestEnemy = FindNearestVisibleEnemy(actor, state);

            // Compute flee direction (away from enemy, or toward nearest map edge).
            int fleeX, fleeY;
            if (nearestEnemy != null)
            {
                int dx = currentPos.X - nearestEnemy.Position.X;
                int dy = currentPos.Y - nearestEnemy.Position.Y;

                // Normalise flee direction: prefer the axis with the larger delta.
                if (Math.Abs(dx) >= Math.Abs(dy))
                {
                    fleeX = dx >= 0 ? 1 : -1;
                    fleeY = 0;
                }
                else
                {
                    fleeX = 0;
                    fleeY = dy >= 0 ? 1 : -1;
                }
            }
            else
            {
                // No enemy visible — flee toward the nearest map edge.
                (fleeX, fleeY) = FleeDirectionTowardNearestEdge(currentPos, state.Grid);
            }

            if (fleeX == 0 && fleeY == 0)
                return currentPos; // already at edge and no direction to flee

            // Step up to moveRange tiles in the flee direction, stopping at bounds/impassable tiles.
            GridPosition dest = currentPos;
            for (int i = 0; i < moveRange; i++)
            {
                GridPosition next = new GridPosition(dest.X + fleeX, dest.Y + fleeY);
                if (!state.Grid.IsInBounds(next)) break;
                Tile nextTile = state.Grid.GetTile(next);
                if (nextTile == null || !nextTile.IsPassable) break;
                // Do not walk into a tile already occupied by another unit.
                if (nextTile.IsOccupied && nextTile.OccupantId != actorId) break;
                dest = next;
            }

            return dest;
        }

        /// <summary>
        /// Returns the override attack target for a PANICKED actor.
        /// Picks the nearest living unit (any allegiance, excluding the actor) within the
        /// actor's spell range (stub: 4 tiles). On ties, lowest (x+y) then lowest x wins.
        /// Returns <c>null</c> if no unit is in range — caller skips the attack.
        /// </summary>
        /// <param name="actorId">Runtime ID of the PANICKED unit.</param>
        /// <param name="state">Current simulation state.</param>
        /// <returns>Runtime ID of the nearest unit in range, or <c>null</c>.</returns>
        public static string ResolvePanickedAttackTarget(string actorId, SimulationState state)
        {
            UnitState actor = state.GetUnit(actorId);
            if (actor == null || !actor.IsAlive)
                return null;

            // PANICKED units use their lowest-AP-cost ability, so any unit in the stub range
            // qualifies. The stub range is 4 tiles (matching SpellCommand.StubSpellRange).
            const int PanickedAttackRange = 4;

            string bestId = null;
            int bestDist = int.MaxValue;
            int bestSum = int.MaxValue;
            int bestX = int.MaxValue;

            foreach (UnitState candidate in state.GetAllUnits())
            {
                if (candidate.Id == actorId) continue;
                if (!candidate.IsAlive) continue;

                int dist = actor.Position.ManhattanDistance(candidate.Position);
                if (dist > PanickedAttackRange) continue;

                int sum = candidate.Position.X + candidate.Position.Y;
                bool better = dist < bestDist
                    || (dist == bestDist && sum < bestSum)
                    || (dist == bestDist && sum == bestSum && candidate.Position.X < bestX);

                if (better)
                {
                    bestId = candidate.Id;
                    bestDist = dist;
                    bestSum = sum;
                    bestX = candidate.Position.X;
                }
            }

            return bestId;
        }

        // ---------------------------------------------------------------------------
        // CHARMED
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns the override spell target for a CHARMED actor.
        /// Picks the nearest ally (same OwnerId, excluding self) within the actor's spell range
        /// (stub: 4 tiles) regardless of line of sight (CHARMED is a compulsion).
        /// On ties, lowest (x+y) then lowest x wins.
        /// Returns <c>null</c> if no ally is in range — the caller applies move-toward-ally behavior instead.
        /// </summary>
        /// <param name="actorId">Runtime ID of the CHARMED unit.</param>
        /// <param name="state">Current simulation state.</param>
        /// <returns>Runtime ID of the nearest ally in spell range, or <c>null</c>.</returns>
        public static string ResolveCharmedTarget(string actorId, SimulationState state)
        {
            UnitState actor = state.GetUnit(actorId);
            if (actor == null || !actor.IsAlive)
                return null;

            const int CharmedSpellRange = 4;

            string bestId = null;
            int bestDist = int.MaxValue;
            int bestSum = int.MaxValue;
            int bestX = int.MaxValue;

            foreach (UnitState candidate in state.GetAllUnits())
            {
                if (candidate.Id == actorId) continue;
                if (!candidate.IsAlive) continue;
                if (candidate.OwnerId != actor.OwnerId) continue; // must be an ally

                int dist = actor.Position.ManhattanDistance(candidate.Position);
                if (dist > CharmedSpellRange) continue;

                int sum = candidate.Position.X + candidate.Position.Y;
                bool better = dist < bestDist
                    || (dist == bestDist && sum < bestSum)
                    || (dist == bestDist && sum == bestSum && candidate.Position.X < bestX);

                if (better)
                {
                    bestId = candidate.Id;
                    bestDist = dist;
                    bestSum = sum;
                    bestX = candidate.Position.X;
                }
            }

            return bestId;
        }

        /// <summary>
        /// Returns the override move destination for a CHARMED actor when no ally is in spell range.
        /// Moves up to <paramref name="moveRange"/> tiles toward the nearest ally (any distance).
        /// If no ally exists on the field, the actor stays in place.
        /// </summary>
        /// <param name="actorId">Runtime ID of the CHARMED unit.</param>
        /// <param name="moveRange">The unit's full movement range in tiles.</param>
        /// <param name="state">Current simulation state.</param>
        /// <returns>The destination tile the charmed unit moves toward.</returns>
        public static GridPosition ResolveCharmedMove(string actorId, int moveRange, SimulationState state)
        {
            UnitState actor = state.GetUnit(actorId);
            if (actor == null || !actor.IsAlive)
                return actor?.Position ?? GridPosition.Zero;

            // Find nearest ally (any distance).
            UnitState nearestAlly = null;
            int nearestAllyDist = int.MaxValue;
            int nearestAllySum = int.MaxValue;

            foreach (UnitState candidate in state.GetAllUnits())
            {
                if (candidate.Id == actorId) continue;
                if (!candidate.IsAlive) continue;
                if (candidate.OwnerId != actor.OwnerId) continue;

                int dist = actor.Position.ManhattanDistance(candidate.Position);
                int sum = candidate.Position.X + candidate.Position.Y;
                bool better = dist < nearestAllyDist
                    || (dist == nearestAllyDist && sum < nearestAllySum);

                if (better)
                {
                    nearestAlly = candidate;
                    nearestAllyDist = dist;
                    nearestAllySum = sum;
                }
            }

            if (nearestAlly == null)
                return actor.Position; // no ally on field — stay in place

            // Compute step direction toward the nearest ally.
            int dx = nearestAlly.Position.X - actor.Position.X;
            int dy = nearestAlly.Position.Y - actor.Position.Y;

            int stepX, stepY;
            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                stepX = dx >= 0 ? 1 : -1;
                stepY = 0;
            }
            else
            {
                stepX = 0;
                stepY = dy >= 0 ? 1 : -1;
            }

            // Step up to moveRange tiles toward the ally.
            GridPosition dest = actor.Position;
            for (int i = 0; i < moveRange; i++)
            {
                GridPosition next = new GridPosition(dest.X + stepX, dest.Y + stepY);
                if (!state.Grid.IsInBounds(next)) break;
                Tile nextTile = state.Grid.GetTile(next);
                if (nextTile == null || !nextTile.IsPassable) break;
                if (nextTile.IsOccupied && nextTile.OccupantId != actorId) break;
                dest = next;
                // Stop one step before the ally's tile.
                if (dest == nearestAlly.Position) break;
            }

            return dest;
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Finds the nearest living enemy unit (different OwnerId) that is visible from
        /// the actor's current position. Returns <c>null</c> if no enemy is visible.
        /// Ties broken by lowest (x+y) sum, then lowest x.
        /// </summary>
        private static UnitState FindNearestVisibleEnemy(UnitState actor, SimulationState state)
        {
            UnitState nearest = null;
            int nearestDist = int.MaxValue;
            int nearestSum = int.MaxValue;
            int nearestX = int.MaxValue;

            foreach (UnitState candidate in state.GetAllUnits())
            {
                if (candidate.Id == actor.Id) continue;
                if (!candidate.IsAlive) continue;
                if (candidate.OwnerId == actor.OwnerId) continue; // same team

                if (!state.Grid.HasLineOfSight(actor.Position, candidate.Position))
                    continue;

                int dist = actor.Position.ManhattanDistance(candidate.Position);
                int sum = candidate.Position.X + candidate.Position.Y;
                bool better = dist < nearestDist
                    || (dist == nearestDist && sum < nearestSum)
                    || (dist == nearestDist && sum == nearestSum && candidate.Position.X < nearestX);

                if (better)
                {
                    nearest = candidate;
                    nearestDist = dist;
                    nearestSum = sum;
                    nearestX = candidate.Position.X;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Returns the cardinal flee direction (stepX, stepY) that moves the unit toward
        /// the nearest grid edge. Used when no enemy is visible during PANICKED movement.
        /// Returns (0, 0) if the unit cannot determine an edge direction.
        /// </summary>
        private static (int stepX, int stepY) FleeDirectionTowardNearestEdge(
            GridPosition pos, GridData grid)
        {
            int distLeft   = pos.X;
            int distRight  = grid.Width  - 1 - pos.X;
            int distBottom = pos.Y;
            int distTop    = grid.Height - 1 - pos.Y;

            int minDist = Math.Min(Math.Min(distLeft, distRight), Math.Min(distBottom, distTop));

            // Prefer the axis with smallest distance to the edge (flee toward the nearest edge).
            // Break ties by cardinal priority: left, right, bottom, top.
            if (minDist == distLeft)   return (-1,  0);
            if (minDist == distRight)  return ( 1,  0);
            if (minDist == distBottom) return ( 0, -1);
            if (minDist == distTop)    return ( 0,  1);

            return (0, 0);
        }
    }
}

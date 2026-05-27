using System;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Simulation.Commands
{
    /// <summary>
    /// Distinguishes whether this move is voluntary (player-initiated) or forced
    /// (knockback, push, pull, or other displacement effects).
    /// ICE_TILE rules differ between the two modes.
    /// </summary>
    public enum MoveKind
    {
        /// <summary>Player-planned movement. Costs +1 AP per ICE_TILE traversed.</summary>
        Voluntary,

        /// <summary>
        /// Forced displacement (knockback, push, pull). Extends by exactly 1 extra tile
        /// in the displacement direction when the endpoint or the tile one step further
        /// is an ICE_TILE — guaranteed, no roll.
        /// </summary>
        Forced
    }
    /// <summary>
    /// Command that moves a unit from its current tile to a destination tile.
    ///
    /// Validation checks destination bounds, passability, occupancy, and movement range.
    /// Execution updates the unit's position on SimulationState and the occupancy
    /// markers on GridData, then emits a UnitMovedEvent.
    ///
    /// Move range is calculated as Manhattan distance — diagonal movement costs 2 (X+Y),
    /// matching the grid's tile-counting rules for the movement flood fill.
    /// </summary>
    public sealed class MoveCommand : Command
    {
        // Extra AP cost charged per ICE_TILE traversed during voluntary movement.
        private const int IceTileVoluntaryApPenalty = 1;

        // Number of extra tiles a forced displacement extends when it ends on or just past an ICE_TILE.
        private const int IceTileForcedExtension = 1;

        /// <summary>The grid tile the unit is attempting to move to.</summary>
        public GridPosition Destination { get; }

        /// <summary>
        /// Whether this move is voluntary (player-planned) or forced (knockback/push/pull).
        /// Determines which ICE_TILE rule applies.
        /// </summary>
        public MoveKind Kind { get; }

        /// <summary>
        /// Creates a MoveCommand for the given actor targeting the specified destination.
        /// </summary>
        /// <param name="actorId">Runtime ID of the unit to move.</param>
        /// <param name="activationCost">Budget cost of this unit's activation.</param>
        /// <param name="destination">The tile the unit is moving to.</param>
        /// <param name="kind">Whether this is voluntary or forced movement. Defaults to Voluntary.</param>
        public MoveCommand(string actorId, int activationCost, GridPosition destination,
                           MoveKind kind = MoveKind.Voluntary)
            : base(actorId, activationCost)
        {
            Destination = destination;
            Kind = kind;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Validates:
        /// <list type="bullet">
        ///   <item>Unit exists in the simulation registry.</item>
        ///   <item>Unit is alive (CurrentHP > 0).</item>
        ///   <item>Destination is within grid bounds.</item>
        ///   <item>Destination tile is passable (not Destroyed or Obsidian).</item>
        ///   <item>Destination is not occupied by another unit (the unit may "move" to its own tile).</item>
        ///   <item>Destination is within the unit's MoveRange (Manhattan distance).</item>
        /// </list>
        /// </remarks>
        public override bool Validate(SimulationState state)
        {
            UnitState actor = state.GetUnit(ActorId);

            // Unit must exist and be alive.
            if (actor == null || !actor.IsAlive)
                return false;

            // Destination must be inside the grid.
            if (!state.Grid.IsInBounds(Destination))
                return false;

            // Moving to the current position is always valid (pass/hold action).
            if (actor.Position == Destination)
                return true;

            // Destination tile must be passable.
            if (!state.Grid.IsPassable(Destination))
                return false;

            // Destination must be unoccupied or occupied only by this unit itself.
            string occupantId = state.Grid.GetOccupantId(Destination);
            if (occupantId != null && occupantId != ActorId)
                return false;

            // Destination must be within the unit's movement range.
            int distance = actor.Position.ManhattanDistance(Destination);
            if (distance > actor.MoveRange)
                return false;

            return true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Clears the unit's old tile occupancy, sets the new tile occupancy, and
        /// updates the unit's Position. Returns a single UnitMovedEvent.
        ///
        /// ICE_TILE rules applied during execution:
        /// <list type="bullet">
        ///   <item>
        ///     <description>
        ///       <b>Voluntary movement (Kind == MoveKind.Voluntary):</b> Each tile in the path
        ///       from the unit's current position to <see cref="Destination"/> that has
        ///       <see cref="TileState.Frozen"/> state costs +1 AP from the actor's
        ///       <see cref="UnitState.ActionPoints"/>. AP is reduced but never goes below 0.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <description>
        ///       <b>Forced displacement (Kind == MoveKind.Forced):</b> If the destination tile
        ///       or the tile one step further in the same displacement direction has
        ///       <see cref="TileState.Frozen"/> state, the unit slides exactly
        ///       <see cref="IceTileForcedExtension"/> extra tile(s) in the same direction —
        ///       guaranteed, no random roll. If the extended tile is out of bounds or
        ///       impassable, the unit stops at the last valid tile.
        ///     </description>
        ///   </item>
        /// </list>
        /// </remarks>
        public override SimulationEvent[] Execute(SimulationState state)
        {
            UnitState actor = state.GetUnit(ActorId);
            GridPosition from = actor.Position;
            GridPosition actualDestination = Destination;

            if (Kind == MoveKind.Voluntary)
            {
                // Charge extra AP for each ICE_TILE traversed along the path.
                // We approximate the path as the Bresenham line from origin to destination.
                int icePenalty = CountIceTilesOnPath(state, from, Destination);
                int newAP = actor.ActionPoints - icePenalty * IceTileVoluntaryApPenalty;
                actor.ActionPoints = System.Math.Max(0, newAP);
            }
            else
            {
                // Forced displacement: extend if the destination or one step further is ICE_TILE.
                actualDestination = ResolveIceTileDisplacementExtension(state, from, Destination, ActorId);
            }

            // Clear the old tile's occupant marker.
            state.Grid.ClearOccupant(from);

            // Update the unit's position.
            actor.Position = actualDestination;

            // Mark the new tile as occupied by this unit.
            state.Grid.SetOccupant(actualDestination, ActorId);

            return new SimulationEvent[]
            {
                new UnitMovedEvent(state.TurnNumber, ActorId, from, actualDestination)
            };
        }

        // ---------------------------------------------------------------------------
        // ICE_TILE helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Counts how many tiles in the straight-line path from <paramref name="origin"/>
        /// to <paramref name="dest"/> (excluding the origin, including the destination)
        /// have <see cref="TileState.Frozen"/> state.
        /// Uses Bresenham's line to enumerate the path tiles.
        /// </summary>
        private static int CountIceTilesOnPath(SimulationState state, GridPosition origin, GridPosition dest)
        {
            int count = 0;
            bool first = true;
            foreach (GridPosition pos in state.Grid.GetLine(origin, dest))
            {
                if (first) { first = false; continue; } // skip origin tile
                Tile tile = state.Grid.GetTile(pos);
                if (tile != null && tile.State == TileState.Frozen)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Resolves the actual landing tile for a forced displacement considering ICE_TILE extension.
        /// If the intended destination tile or the tile one step further in the displacement direction
        /// is <see cref="TileState.Frozen"/>, the unit extends exactly <see cref="IceTileForcedExtension"/>
        /// tile(s) in the same direction. The extended tile is clamped to grid bounds; if impassable,
        /// the unit stops at the original destination.
        /// </summary>
        private static GridPosition ResolveIceTileDisplacementExtension(
            SimulationState state, GridPosition origin, GridPosition destination, string actorId)
        {
            // Compute the unit displacement vector (clamped to a cardinal direction).
            int dx = destination.X - origin.X;
            int dy = destination.Y - origin.Y;

            // Normalise to a single-tile direction step. Clamp each axis to [-1,0,1].
            int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
            int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

            // Tile one step past destination in the same direction.
            GridPosition onePastDest = new GridPosition(
                destination.X + stepX,
                destination.Y + stepY);

            // Check if destination or onePastDest is an ICE_TILE.
            Tile destTile = state.Grid.GetTile(destination);
            Tile pastTile = state.Grid.IsInBounds(onePastDest) ? state.Grid.GetTile(onePastDest) : null;

            bool destinationIsIce = destTile != null && destTile.State == TileState.Frozen;
            bool pastIsIce = pastTile != null && pastTile.State == TileState.Frozen;

            if (destinationIsIce || pastIsIce)
            {
                // Extend by IceTileForcedExtension tiles in the same direction.
                GridPosition extended = destination;
                for (int i = 0; i < IceTileForcedExtension; i++)
                {
                    GridPosition next = new GridPosition(extended.X + stepX, extended.Y + stepY);
                    if (!state.Grid.IsInBounds(next))
                        break;
                    Tile nextTile = state.Grid.GetTile(next);
                    if (nextTile == null || !nextTile.IsPassable)
                        break;
                    // Do not land on a tile occupied by another unit.
                    if (nextTile.IsOccupied && nextTile.OccupantId != actorId)
                        break;
                    extended = next;
                }
                return extended;
            }

            return destination;
        }
    }
}

using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Simulation.Commands
{
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
        /// <summary>The grid tile the unit is attempting to move to.</summary>
        public GridPosition Destination { get; }

        /// <summary>
        /// Creates a MoveCommand for the given actor targeting the specified destination.
        /// </summary>
        /// <param name="actorId">Runtime ID of the unit to move.</param>
        /// <param name="activationCost">Budget cost of this unit's activation.</param>
        /// <param name="destination">The tile the unit is moving to.</param>
        public MoveCommand(string actorId, int activationCost, GridPosition destination)
            : base(actorId, activationCost)
        {
            Destination = destination;
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
        /// </remarks>
        public override SimulationEvent[] Execute(SimulationState state)
        {
            UnitState actor = state.GetUnit(ActorId);
            GridPosition from = actor.Position;

            // Clear the old tile's occupant marker.
            state.Grid.ClearOccupant(from);

            // Update the unit's position.
            actor.Position = Destination;

            // Mark the new tile as occupied by this unit.
            state.Grid.SetOccupant(Destination, ActorId);

            return new SimulationEvent[]
            {
                new UnitMovedEvent(state.TurnNumber, ActorId, from, Destination)
            };
        }
    }
}

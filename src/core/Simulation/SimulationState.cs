using System;
using System.Collections.Generic;
using System.Linq;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Simulation
{
    /// <summary>
    /// The complete runtime state of an in-progress match.
    ///
    /// SimulationState is the single source of truth for everything that changes during a game:
    /// unit positions, HP, cooldowns, tile states, turn number, and turn phase. All simulation
    /// systems (TurnManager, SpellResolver, StatusManager) read from and write to this object.
    ///
    /// SimulationState is mutated only by the simulation layer. The presentation layer reads
    /// state indirectly through SimulationEvents emitted by the simulation systems.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class SimulationState
    {
        // ---------------------------------------------------------------------------
        // Core state
        // ---------------------------------------------------------------------------

        /// <summary>
        /// The battlefield grid. Contains all tile state (elemental, elevation, passability,
        /// occupancy). Mutated by MoveCommand and SpellResolver.
        /// </summary>
        public GridData Grid { get; }

        /// <summary>
        /// Array of all player IDs participating in this match.
        /// Typically two IDs for a standard match. Order is preserved across the match lifetime.
        /// </summary>
        public string[] PlayerIds { get; }

        /// <summary>
        /// The current turn number. Starts at 1 and increments at the end of each resolved turn.
        /// TurnManager checks this against the turn limit (50) to evaluate draw conditions.
        /// </summary>
        public int TurnNumber { get; private set; }

        /// <summary>
        /// The current phase of the ongoing turn.
        /// Transitions: Planning → Locked → Resolving → Ended → (next turn) Planning.
        /// </summary>
        public TurnPhase Phase { get; internal set; }

        // ---------------------------------------------------------------------------
        // Unit registry
        // ---------------------------------------------------------------------------

        // Keyed by unit runtime ID. All living and recently dead units are tracked here;
        // units are removed via DeregisterUnit after death processing is complete.
        private readonly Dictionary<string, UnitState> _units = new Dictionary<string, UnitState>();

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initializes a fresh SimulationState for a new match.
        /// </summary>
        /// <param name="grid">The battlefield GridData for this match. Must not be null.</param>
        /// <param name="playerIds">
        /// Array of player IDs. Must contain exactly 2 entries for a standard match.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if grid or playerIds is null.</exception>
        /// <exception cref="ArgumentException">Thrown if playerIds is empty.</exception>
        public SimulationState(GridData grid, string[] playerIds)
        {
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            if (playerIds == null) throw new ArgumentNullException(nameof(playerIds));
            if (playerIds.Length == 0) throw new ArgumentException("At least one player ID is required.", nameof(playerIds));

            PlayerIds = playerIds;
            TurnNumber = 1;
            Phase = TurnPhase.Planning;
        }

        // ---------------------------------------------------------------------------
        // Unit registry methods
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Registers a unit into the simulation. The unit's starting tile is also marked
        /// as occupied on the GridData.
        /// </summary>
        /// <param name="unit">The unit to register. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if unit is null.</exception>
        /// <exception cref="ArgumentException">Thrown if a unit with the same ID is already registered.</exception>
        public void RegisterUnit(UnitState unit)
        {
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            if (_units.ContainsKey(unit.Id))
                throw new ArgumentException($"A unit with ID '{unit.Id}' is already registered.", nameof(unit));

            _units[unit.Id] = unit;
            Grid.SetOccupant(unit.Position, unit.Id);
        }

        /// <summary>
        /// Removes a unit from the simulation registry and clears its tile occupancy.
        /// Called after a unit's death has been fully processed.
        /// No-op if no unit with that ID is registered.
        /// </summary>
        /// <param name="unitId">Runtime ID of the unit to remove.</param>
        public void DeregisterUnit(string unitId)
        {
            if (_units.TryGetValue(unitId, out UnitState unit))
            {
                Grid.ClearOccupant(unit.Position);
                _units.Remove(unitId);
            }
        }

        /// <summary>
        /// Returns the UnitState for the given unit ID.
        /// </summary>
        /// <param name="unitId">Runtime ID of the unit to look up.</param>
        /// <returns>The UnitState, or null if no unit with that ID is registered.</returns>
        public UnitState GetUnit(string unitId)
        {
            if (unitId == null) return null;
            _units.TryGetValue(unitId, out UnitState unit);
            return unit;
        }

        /// <summary>
        /// Returns all currently registered units (including dead units still in death processing).
        /// Enumeration order is unspecified.
        /// </summary>
        public IEnumerable<UnitState> GetAllUnits()
        {
            return _units.Values;
        }

        /// <summary>
        /// Returns all registered units belonging to the specified player.
        /// </summary>
        /// <param name="ownerId">The player ID to filter by.</param>
        public IEnumerable<UnitState> GetUnitsByOwner(string ownerId)
        {
            return _units.Values.Where(u => u.OwnerId == ownerId);
        }

        /// <summary>
        /// Returns all registered units whose CurrentHP is greater than zero.
        /// </summary>
        public IEnumerable<UnitState> GetLivingUnits()
        {
            return _units.Values.Where(u => u.IsAlive);
        }

        /// <summary>
        /// Returns all living units that are of type Mancer across all players.
        /// Win condition checks use this to determine if a player still has Mancers standing.
        /// </summary>
        public IEnumerable<UnitState> GetLivingMancers()
        {
            return _units.Values.Where(u => u.IsAlive && u.Type == UnitType.Mancer);
        }

        /// <summary>
        /// Returns all living Mancers belonging to the specified player.
        /// </summary>
        /// <param name="ownerId">The player ID to filter by.</param>
        public IEnumerable<UnitState> GetLivingMancersByOwner(string ownerId)
        {
            return _units.Values.Where(u => u.IsAlive && u.Type == UnitType.Mancer && u.OwnerId == ownerId);
        }

        // ---------------------------------------------------------------------------
        // Turn state management (internal — called by TurnManager)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Increments the turn counter. Called by TurnManager after all resolution and end-of-turn
        /// processing is complete.
        /// </summary>
        internal void AdvanceTurn()
        {
            TurnNumber++;
        }

        /// <summary>
        /// Resets all living units for the start of a new planning phase.
        /// Calls ResetForNewTurn() on every living unit — clears ActivatedThisTurn,
        /// restores ActionPoints. Dead units are not reset.
        /// </summary>
        internal void ResetUnitsForNewTurn()
        {
            foreach (UnitState unit in _units.Values)
            {
                if (unit.IsAlive)
                    unit.ResetForNewTurn();
            }
        }
    }
}

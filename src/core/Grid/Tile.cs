namespace Battlemancers.Core.Grid
{
    /// <summary>
    /// The mutable data container for a single grid cell on the battlefield.
    /// Each tile tracks its position, elemental state, elevation, passability, and occupant.
    ///
    /// Tiles are owned by GridData and mutated only through GridData methods.
    /// The simulation reads and writes Tile data; the presentation layer reads it to drive visuals.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public class Tile
    {
        // --- Immutable identity ---

        /// <summary>
        /// The grid coordinates of this tile. Set at construction and never changed.
        /// </summary>
        public GridPosition Position { get; }

        // --- Mutable simulation state ---

        /// <summary>
        /// The current elemental or terrain state of this tile.
        /// Changing state also updates IsPassable via SetState().
        /// </summary>
        public TileState State { get; private set; }

        /// <summary>
        /// The elevation level of this tile.
        /// 0 = ground level, 1 = raised/hill, 2 = high ground, -1 = pit/depression.
        /// Elevation affects line-of-sight, fall damage, and range bonuses.
        /// </summary>
        public int Elevation { get; internal set; }

        /// <summary>
        /// Whether a unit can stand on or move through this tile.
        /// Automatically set by SetState() based on the new TileState.
        /// Can be overridden directly for terrain features (walls, etc.).
        /// </summary>
        public bool IsPassable { get; internal set; }

        /// <summary>
        /// The ID of the unit currently occupying this tile.
        /// Null if the tile is unoccupied. The simulation layer uses string IDs to avoid
        /// coupling unit objects into the grid data structure.
        /// </summary>
        public string OccupantId { get; internal set; }

        // --- Computed properties ---

        /// <summary>Returns true if a unit is currently on this tile.</summary>
        public bool IsOccupied => OccupantId != null;

        // --- Constructor ---

        /// <summary>
        /// Initializes a new Tile at the given position with optional initial state and elevation.
        /// Passability is derived from the initial TileState.
        /// </summary>
        /// <param name="position">The grid coordinates of this tile.</param>
        /// <param name="initialState">The starting elemental state. Defaults to Normal.</param>
        /// <param name="elevation">The starting elevation level. Defaults to 0 (ground).</param>
        public Tile(GridPosition position, TileState initialState = TileState.Normal, int elevation = 0)
        {
            Position = position;
            Elevation = elevation;
            OccupantId = null;

            // Use SetState to ensure IsPassable is correctly initialized from the state.
            SetState(initialState);
        }

        // --- State mutation ---

        /// <summary>
        /// Sets the tile's elemental state and updates IsPassable accordingly.
        /// Certain states make a tile impassable:
        /// <list type="bullet">
        ///   <item><description>Destroyed — the tile is a void/pit; entering causes KO.</description></item>
        ///   <item><description>Obsidian — hardened lava barrier; blocks movement and LOS.</description></item>
        /// </list>
        /// All other states leave the tile passable by default (individual tile features
        /// such as walls are handled separately via IsPassable override).
        /// </summary>
        /// <param name="newState">The new TileState to apply.</param>
        public void SetState(TileState newState)
        {
            State = newState;

            // Destroyed and Obsidian are the two ground states that make a tile impassable.
            // All other tile states (Burning, Frozen, Mud, etc.) may apply damage or
            // movement penalties but do not block entry — units choose to enter at their peril.
            IsPassable = newState != TileState.Destroyed && newState != TileState.Obsidian;
        }

        // --- Entry check ---

        /// <summary>
        /// Returns true if the specified unit is allowed to enter this tile.
        /// A unit can enter if:
        /// <list type="bullet">
        ///   <item><description>The tile is passable (not Destroyed or Obsidian).</description></item>
        ///   <item><description>The tile is unoccupied, OR is already occupied by this unit (standing still).</description></item>
        /// </list>
        /// Note: This is a basic entry check. Movement range, elevation transitions,
        /// and flying/special movement rules are evaluated by the pathfinding layer above this.
        /// </summary>
        /// <param name="unitId">The ID of the unit attempting to enter.</param>
        /// <returns>True if the unit may enter; false otherwise.</returns>
        public bool CanEnter(string unitId)
        {
            return IsPassable && (!IsOccupied || OccupantId == unitId);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string occupant = IsOccupied ? $" [{OccupantId}]" : string.Empty;
            return $"Tile{Position} State={State} Elev={Elevation} Passable={IsPassable}{occupant}";
        }
    }
}

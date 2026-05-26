namespace Battlemancers.Core.Map
{
    /// <summary>
    /// Represents the definition of a single tile as stored in a map layout JSON file.
    /// MapTile is a pure data transfer object — it carries the initial configuration
    /// for one grid cell and is consumed by <see cref="MapLoader"/> to populate a
    /// <see cref="Battlemancers.Core.Grid.GridData"/> at match start.
    ///
    /// MapTile is not used at runtime after the GridData has been built. All live
    /// tile state during simulation is stored in <see cref="Battlemancers.Core.Grid.Tile"/>.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class MapTile
    {
        /// <summary>
        /// The column coordinate of this tile on the grid.
        /// Must be in the range [0, mapWidth - 1].
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The row coordinate of this tile on the grid.
        /// Must be in the range [0, mapHeight - 1].
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// The initial elemental or terrain state of this tile, expressed as the string name
        /// of a <see cref="Battlemancers.Core.Grid.TileState"/> enum value.
        /// Examples: "Normal", "Wet", "Burning", "Frozen", "Natural", "Obsidian".
        /// If null or empty, MapLoader defaults to "Normal".
        /// Must match a TileState enum member name exactly (case-sensitive).
        /// </summary>
        public string TileState { get; set; }

        /// <summary>
        /// The starting elevation of this tile.
        /// <list type="bullet">
        ///   <item><description>0 — ground level (default).</description></item>
        ///   <item><description>1 — raised terrain / hill. Grants +1 range bonus; affects LOS.</description></item>
        ///   <item><description>2 — high ground. Maximum elevation in standard maps.</description></item>
        ///   <item><description>-1 — pit / depression (reserved for future use).</description></item>
        /// </list>
        /// </summary>
        public int Elevation { get; set; }

        /// <summary>
        /// Whether units can enter this tile at match start.
        /// Impassable tiles (walls, lava fields, voids) block movement and cannot be occupied.
        /// Note: certain TileState values (Obsidian, Destroyed) also force impassability at the
        /// simulation layer regardless of this field.
        /// </summary>
        public bool IsPassable { get; set; } = true;

        /// <summary>
        /// Designates this tile as a valid spawn location for a specific player at match start.
        /// <list type="bullet">
        ///   <item><description>"player1" — valid spawn for player 1.</description></item>
        ///   <item><description>"player2" — valid spawn for player 2.</description></item>
        ///   <item><description>null — not a spawn tile (default for most tiles).</description></item>
        /// </list>
        /// A tile marked as a spawn zone must also have IsPassable = true.
        /// Impassable spawn tiles are flagged as errors by MapLoader.ValidateLayout.
        /// </summary>
        public string SpawnZone { get; set; }
    }
}

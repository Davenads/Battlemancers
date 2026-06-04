using System.Collections.Generic;

namespace Battlemancers.Core.Maps
{
    /// <summary>
    /// Data transfer object that mirrors the structure of a preset map JSON file.
    /// Populated by <see cref="MapLoader.LoadFromJson"/> and converted to a live
    /// <see cref="Battlemancers.Core.Grid.GridData"/> via <see cref="MapLoader.ToGridData"/>.
    ///
    /// Pure C# — zero Unity dependencies. All fields must match the JSON property names
    /// exactly (case-sensitive) because System.Text.Json default naming policy is used.
    /// </summary>
    public class MapData
    {
        /// <summary>Unique machine identifier for this map (e.g., "crossroads").</summary>
        public string Id { get; set; }

        /// <summary>Human-readable display name shown in the UI (e.g., "The Crossroads").</summary>
        public string Name { get; set; }

        /// <summary>Flavour description shown in the map selection screen.</summary>
        public string Description { get; set; }

        /// <summary>Number of columns on the grid (X axis).</summary>
        public int Width { get; set; }

        /// <summary>Number of rows on the grid (Y axis).</summary>
        public int Height { get; set; }

        /// <summary>
        /// Explicit tile state overrides. Tiles not listed here default to
        /// <see cref="Battlemancers.Core.Grid.TileState.Normal"/>.
        /// </summary>
        public List<TileEntry> Tiles { get; set; } = new List<TileEntry>();

        /// <summary>Starting positions for each team, indexed by <see cref="SpawnPoint.Team"/>.</summary>
        public List<SpawnPoint> SpawnPoints { get; set; } = new List<SpawnPoint>();
    }

    /// <summary>
    /// Describes the initial <see cref="Battlemancers.Core.Grid.TileState"/> of a single
    /// grid cell. Only cells that differ from Normal need an entry.
    ///
    /// JSON field names match C# property names exactly — no camelCase conversion.
    /// </summary>
    public class TileEntry
    {
        /// <summary>Column coordinate. Range: [0, MapData.Width - 1].</summary>
        public int X { get; set; }

        /// <summary>Row coordinate. Range: [0, MapData.Height - 1].</summary>
        public int Y { get; set; }

        /// <summary>
        /// String name of the <see cref="Battlemancers.Core.Grid.TileState"/> enum value to apply.
        /// Must match an enum member name exactly (e.g., "Wet", "Burning", "Frozen", "Mud").
        /// </summary>
        public string TileState { get; set; }
    }

    /// <summary>
    /// Marks a single tile as a valid unit spawn position for the given team at match start.
    /// </summary>
    public class SpawnPoint
    {
        /// <summary>Team index. 0 = team one, 1 = team two.</summary>
        public int Team { get; set; }

        /// <summary>Column coordinate of the spawn tile.</summary>
        public int X { get; set; }

        /// <summary>Row coordinate of the spawn tile.</summary>
        public int Y { get; set; }
    }
}

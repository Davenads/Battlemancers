using System;
using System.IO;
using System.Text.Json;
using Battlemancers.Core.Grid;

namespace Battlemancers.Core.Maps
{
    /// <summary>
    /// Converts preset map JSON into <see cref="MapData"/> DTOs and then into live
    /// <see cref="GridData"/> objects ready for the simulation layer.
    ///
    /// MapLoader is the ONLY class permitted to deserialize map JSON. No other class
    /// may call <c>JsonSerializer.Deserialize&lt;MapData&gt;</c> directly.
    ///
    /// Pure C# — zero Unity dependencies. Fully usable in headless tests and server contexts.
    ///
    /// Expected JSON format:
    /// <code>
    /// {
    ///   "Id": "crossroads",
    ///   "Name": "The Crossroads",
    ///   "Description": "...",
    ///   "Width": 10,
    ///   "Height": 10,
    ///   "Tiles": [ { "X": 2, "Y": 2, "TileState": "Wet" } ],
    ///   "SpawnPoints": [ { "Team": 0, "X": 1, "Y": 1 } ]
    /// }
    /// </code>
    /// Tiles absent from the Tiles array default to <see cref="TileState.Normal"/>.
    /// </summary>
    public static class MapLoader
    {
        // Shared deserializer options — case-insensitive to tolerate minor authoring drift,
        // trailing commas, and inline comments in JSON source files.
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling        = JsonCommentHandling.Skip,
            AllowTrailingCommas        = true,
        };

        // -----------------------------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Deserializes a raw JSON string into a <see cref="MapData"/> DTO.
        /// </summary>
        /// <param name="json">The raw JSON string. Must not be null or whitespace.</param>
        /// <returns>A populated <see cref="MapData"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null or empty.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
        public static MapData LoadFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentNullException(nameof(json), "Map JSON must not be null or empty.");

            return JsonSerializer.Deserialize<MapData>(json, JsonOptions)
                   ?? throw new JsonException("Deserialized MapData was null.");
        }

        /// <summary>
        /// Reads a JSON file from disk and delegates to <see cref="LoadFromJson"/>.
        /// </summary>
        /// <param name="filePath">Absolute path to the .json map file.</param>
        /// <returns>A populated <see cref="MapData"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
        /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
        public static MapData LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "File path must not be null or empty.");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Map file not found: {filePath}", filePath);

            string json = File.ReadAllText(filePath);
            return LoadFromJson(json);
        }

        /// <summary>
        /// Converts a <see cref="MapData"/> DTO into a fully populated <see cref="GridData"/>
        /// ready for simulation.
        ///
        /// Algorithm:
        /// <list type="number">
        ///   <item><description>Allocates a <see cref="GridData"/> of Width × Height tiles, all initialised to Normal.</description></item>
        ///   <item><description>Applies every <see cref="TileEntry"/> override: parses <see cref="TileEntry.TileState"/>
        ///   to the <see cref="TileState"/> enum and calls <see cref="GridData.SetTileState"/>.</description></item>
        ///   <item><description>Returns the populated grid.</description></item>
        /// </list>
        /// Spawn point data is stored on <see cref="MapData.SpawnPoints"/> and is not embedded in
        /// GridData — callers that need spawn positions must read MapData.SpawnPoints directly.
        /// </summary>
        /// <param name="mapData">The source DTO. Must not be null.</param>
        /// <returns>A <see cref="GridData"/> with all tile state overrides applied.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapData"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a <see cref="TileEntry.TileState"/> string does not match any
        /// <see cref="TileState"/> enum member.
        /// </exception>
        public static GridData ToGridData(MapData mapData)
        {
            if (mapData == null)
                throw new ArgumentNullException(nameof(mapData));

            var grid = new GridData(mapData.Width, mapData.Height);

            if (mapData.Tiles == null)
                return grid;

            foreach (TileEntry entry in mapData.Tiles)
            {
                if (!Enum.TryParse<TileState>(entry.TileState, ignoreCase: false, out TileState tileState))
                {
                    throw new InvalidOperationException(
                        $"Unknown TileState '{entry.TileState}' at tile ({entry.X}, {entry.Y}). " +
                        "Value must exactly match a TileState enum member name.");
                }

                grid.SetTileState(new GridPosition(entry.X, entry.Y), tileState);
            }

            return grid;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Battlemancers.Core.Grid;

namespace Battlemancers.Core.Map
{
    /// <summary>
    /// Converts a map layout JSON string into a <see cref="GridData"/> ready for simulation.
    ///
    /// MapLoader is the bridge between the data layer (JSON files on disk) and the simulation
    /// layer (GridData in memory). It is pure C# with zero Unity dependencies, making it
    /// fully usable in headless simulation, unit tests, and server-side match setup.
    ///
    /// Supported JSON formats:
    /// <list type="bullet">
    ///   <item><description>
    ///     <b>Compact format</b> — top-level object with "mapId", "width", "height",
    ///     "defaults" (default tile properties), and "tiles" (array of non-default overrides).
    ///     Only tiles that differ from the defaults need to be listed.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Full array format</b> — a bare JSON array of tile objects, one per grid cell.
    ///     Every tile must be present; the array length must equal width * height.
    ///   </description></item>
    /// </list>
    /// </summary>
    public static class MapLoader
    {
        // -----------------------------------------------------------------------------------------
        // Internal deserialization helpers
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Internal DTO for the compact map layout format with defaults.
        /// </summary>
        private class CompactLayout
        {
            [JsonPropertyName("mapId")]
            public string MapId { get; set; }

            [JsonPropertyName("width")]
            public int Width { get; set; }

            [JsonPropertyName("height")]
            public int Height { get; set; }

            [JsonPropertyName("defaults")]
            public DefaultTile Defaults { get; set; }

            [JsonPropertyName("tiles")]
            public List<RawTile> Tiles { get; set; }
        }

        /// <summary>
        /// Default tile values used for all positions not listed in the compact tiles array.
        /// </summary>
        private class DefaultTile
        {
            [JsonPropertyName("tileState")]
            public string TileState { get; set; } = "Normal";

            [JsonPropertyName("elevation")]
            public int Elevation { get; set; } = 0;

            [JsonPropertyName("isPassable")]
            public bool IsPassable { get; set; } = true;

            [JsonPropertyName("spawnZone")]
            public string SpawnZone { get; set; } = null;
        }

        /// <summary>
        /// Raw tile DTO that matches both the compact-format override entries and
        /// the full-array-format tile entries. All fields are nullable so that
        /// missing fields fall back to defaults in compact mode.
        /// </summary>
        private class RawTile
        {
            [JsonPropertyName("x")]
            public int X { get; set; }

            [JsonPropertyName("y")]
            public int Y { get; set; }

            [JsonPropertyName("tileState")]
            public string TileState { get; set; }

            [JsonPropertyName("elevation")]
            public int? Elevation { get; set; }

            [JsonPropertyName("isPassable")]
            public bool? IsPassable { get; set; }

            [JsonPropertyName("spawnZone")]
            public string SpawnZone { get; set; }
        }

        // -----------------------------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Deserializes a map layout JSON string and builds a populated <see cref="GridData"/>.
        ///
        /// Accepts both the compact format (object with "defaults" and "tiles" array) and the
        /// full-array format (bare JSON array of all tiles). In compact format, tiles not listed
        /// in the "tiles" array receive the values from "defaults".
        ///
        /// For each tile in the layout, this method:
        /// <list type="number">
        ///   <item><description>Sets the tile's <see cref="TileState"/> via <c>GridData.SetTileState</c>.</description></item>
        ///   <item><description>Sets the tile's elevation via <c>GridData.SetElevation</c>.</description></item>
        ///   <item><description>If <c>isPassable</c> is explicitly false and the tile state would otherwise
        ///   be passable, forces <c>IsPassable = false</c> on the Tile directly.</description></item>
        /// </list>
        /// </summary>
        /// <param name="json">The raw JSON string to deserialize.</param>
        /// <param name="expectedWidth">The expected grid width. Used for validation and GridData construction.</param>
        /// <param name="expectedHeight">The expected grid height. Used for validation and GridData construction.</param>
        /// <returns>A fully populated <see cref="GridData"/> ready to hand to the simulation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null or empty.</exception>
        /// <exception cref="JsonException">Thrown if the JSON is malformed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a tile references an unknown TileState name.</exception>
        public static GridData LoadFromJson(string json, int expectedWidth, int expectedHeight)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentNullException(nameof(json), "Map layout JSON must not be null or empty.");

            var grid = new GridData(expectedWidth, expectedHeight);
            var options = BuildJsonOptions();

            // Determine format by sniffing the first non-whitespace character.
            bool isCompact = IsCompactFormat(json);

            if (isCompact)
            {
                ApplyCompactLayout(json, grid, options);
            }
            else
            {
                ApplyFullArrayLayout(json, grid, options);
            }

            return grid;
        }

        /// <summary>
        /// Parses a map layout JSON string and returns the grid positions designated as spawn
        /// zones for each player.
        ///
        /// Works with both the compact and full-array JSON formats.
        /// </summary>
        /// <param name="json">The raw JSON string to parse.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        ///   <item><description><c>player1Spawns</c> — positions tagged "player1".</description></item>
        ///   <item><description><c>player2Spawns</c> — positions tagged "player2".</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is null or empty.</exception>
        public static (GridPosition[] player1Spawns, GridPosition[] player2Spawns) GetSpawnZones(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentNullException(nameof(json), "Map layout JSON must not be null or empty.");

            var options = BuildJsonOptions();
            var tiles = CollectAllRawTiles(json, options, out _, out _);

            var p1 = new List<GridPosition>();
            var p2 = new List<GridPosition>();

            foreach (var tile in tiles)
            {
                if (tile.SpawnZone == null)
                    continue;

                if (tile.SpawnZone == "player1")
                    p1.Add(new GridPosition(tile.X, tile.Y));
                else if (tile.SpawnZone == "player2")
                    p2.Add(new GridPosition(tile.X, tile.Y));
            }

            return (p1.ToArray(), p2.ToArray());
        }

        /// <summary>
        /// Validates a map layout JSON string against the expected grid dimensions and logical
        /// requirements. Collects all errors rather than stopping at the first failure.
        ///
        /// Checks performed:
        /// <list type="bullet">
        ///   <item><description>Total tile count equals <paramref name="width"/> × <paramref name="height"/>.</description></item>
        ///   <item><description>All tile positions are within bounds [0, width-1] × [0, height-1].</description></item>
        ///   <item><description>Player 1 has at least 4 designated spawn tiles.</description></item>
        ///   <item><description>Player 2 has at least 4 designated spawn tiles.</description></item>
        ///   <item><description>No tile is simultaneously impassable and marked as a spawn zone.</description></item>
        /// </list>
        /// </summary>
        /// <param name="json">The raw JSON string to validate.</param>
        /// <param name="width">Expected grid width.</param>
        /// <param name="height">Expected grid height.</param>
        /// <returns>A <see cref="MapLayoutValidationResult"/> with IsValid and any error messages.</returns>
        public static MapLayoutValidationResult ValidateLayout(string json, int width, int height)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(json))
            {
                errors.Add("JSON is null or empty.");
                return new MapLayoutValidationResult(false, errors.ToArray());
            }

            List<RawTile> tiles;
            try
            {
                var options = BuildJsonOptions();
                tiles = CollectAllRawTiles(json, options, out _, out _);
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to parse JSON: {ex.Message}");
                return new MapLayoutValidationResult(false, errors.ToArray());
            }

            int expectedCount = width * height;
            if (tiles.Count != expectedCount)
            {
                errors.Add(
                    $"Tile count mismatch: expected {expectedCount} ({width}x{height}) but found {tiles.Count}.");
            }

            int p1Count = 0;
            int p2Count = 0;

            foreach (var tile in tiles)
            {
                // Bounds check.
                if (tile.X < 0 || tile.X >= width || tile.Y < 0 || tile.Y >= height)
                {
                    errors.Add($"Tile at ({tile.X}, {tile.Y}) is out of bounds for a {width}x{height} grid.");
                }

                // Spawn zone + impassable conflict.
                bool isImpassable = tile.IsPassable.HasValue && !tile.IsPassable.Value;
                bool hasSpawn = !string.IsNullOrEmpty(tile.SpawnZone);
                if (isImpassable && hasSpawn)
                {
                    errors.Add(
                        $"Tile at ({tile.X}, {tile.Y}) is both impassable and designated as a spawn zone ('{tile.SpawnZone}'). Spawn tiles must be passable.");
                }

                if (tile.SpawnZone == "player1") p1Count++;
                else if (tile.SpawnZone == "player2") p2Count++;
            }

            if (p1Count < 4)
                errors.Add($"Player 1 has only {p1Count} spawn tile(s). Minimum required is 4.");

            if (p2Count < 4)
                errors.Add($"Player 2 has only {p2Count} spawn tile(s). Minimum required is 4.");

            return new MapLayoutValidationResult(errors.Count == 0, errors.ToArray());
        }

        // -----------------------------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------------------------

        /// <summary>Returns true if the JSON begins with '{' (compact object format).</summary>
        private static bool IsCompactFormat(string json)
        {
            foreach (char c in json)
            {
                if (char.IsWhiteSpace(c)) continue;
                return c == '{';
            }
            return false;
        }

        /// <summary>Applies a compact-format layout to the grid.</summary>
        private static void ApplyCompactLayout(string json, GridData grid, JsonSerializerOptions options)
        {
            var layout = JsonSerializer.Deserialize<CompactLayout>(json, options);
            if (layout == null)
                throw new JsonException("Failed to deserialize compact map layout.");

            // Seed the default tile values.
            var defaults = layout.Defaults ?? new DefaultTile();

            // Build an override lookup keyed by (x, y).
            var overrides = new Dictionary<(int, int), RawTile>();
            if (layout.Tiles != null)
            {
                foreach (var t in layout.Tiles)
                    overrides[(t.X, t.Y)] = t;
            }

            // Walk every cell. Apply overrides where present; defaults elsewhere.
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if (overrides.TryGetValue((x, y), out var over))
                    {
                        string stateName = over.TileState ?? defaults.TileState ?? "Normal";
                        int elevation   = over.Elevation ?? defaults.Elevation;
                        bool passable   = over.IsPassable ?? defaults.IsPassable;
                        ApplyTile(grid, x, y, stateName, elevation, passable);
                    }
                    else
                    {
                        ApplyTile(grid, x, y, defaults.TileState ?? "Normal", defaults.Elevation, defaults.IsPassable);
                    }
                }
            }
        }

        /// <summary>Applies a full-array-format layout to the grid.</summary>
        private static void ApplyFullArrayLayout(string json, GridData grid, JsonSerializerOptions options)
        {
            var rawTiles = JsonSerializer.Deserialize<List<RawTile>>(json, options);
            if (rawTiles == null)
                throw new JsonException("Failed to deserialize full-array map layout.");

            foreach (var t in rawTiles)
            {
                string stateName = t.TileState ?? "Normal";
                int elevation    = t.Elevation ?? 0;
                bool passable    = t.IsPassable ?? true;
                ApplyTile(grid, t.X, t.Y, stateName, elevation, passable);
            }
        }

        /// <summary>
        /// Collects the full set of resolved RawTile objects from either JSON format,
        /// expanding compact defaults so that every cell has an entry.
        /// </summary>
        private static List<RawTile> CollectAllRawTiles(
            string json,
            JsonSerializerOptions options,
            out int width,
            out int height)
        {
            bool isCompact = IsCompactFormat(json);

            if (isCompact)
            {
                var layout = JsonSerializer.Deserialize<CompactLayout>(json, options);
                if (layout == null)
                    throw new JsonException("Failed to deserialize compact map layout.");

                width  = layout.Width;
                height = layout.Height;

                var defaults = layout.Defaults ?? new DefaultTile();
                var overrides = new Dictionary<(int, int), RawTile>();
                if (layout.Tiles != null)
                {
                    foreach (var t in layout.Tiles)
                        overrides[(t.X, t.Y)] = t;
                }

                var result = new List<RawTile>(width * height);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (overrides.TryGetValue((x, y), out var over))
                        {
                            result.Add(new RawTile
                            {
                                X         = x,
                                Y         = y,
                                TileState = over.TileState ?? defaults.TileState,
                                Elevation = over.Elevation ?? defaults.Elevation,
                                IsPassable = over.IsPassable ?? defaults.IsPassable,
                                SpawnZone = over.SpawnZone ?? defaults.SpawnZone
                            });
                        }
                        else
                        {
                            result.Add(new RawTile
                            {
                                X         = x,
                                Y         = y,
                                TileState = defaults.TileState,
                                Elevation = defaults.Elevation,
                                IsPassable = defaults.IsPassable,
                                SpawnZone = defaults.SpawnZone
                            });
                        }
                    }
                }
                return result;
            }
            else
            {
                var rawTiles = JsonSerializer.Deserialize<List<RawTile>>(json, options);
                if (rawTiles == null)
                    throw new JsonException("Failed to deserialize full-array map layout.");

                // Full array format doesn't embed width/height — callers that need them
                // must get them from MapData. Return 0,0 as sentinel.
                width  = 0;
                height = 0;
                return rawTiles;
            }
        }

        /// <summary>
        /// Applies a single tile's initial values to the GridData.
        /// Parses the TileState name, sets state and elevation, then overrides passability
        /// if the JSON explicitly marks the tile as impassable.
        /// </summary>
        private static void ApplyTile(GridData grid, int x, int y, string stateName, int elevation, bool passable)
        {
            var pos = new GridPosition(x, y);

            // Parse TileState enum.
            if (!Enum.TryParse<TileState>(stateName, ignoreCase: false, out var tileState))
            {
                throw new InvalidOperationException(
                    $"Unknown TileState name '{stateName}' at tile ({x}, {y}). " +
                    $"Value must match a TileState enum member exactly.");
            }

            grid.SetTileState(pos, tileState);
            grid.SetElevation(pos, elevation);

            // If the JSON explicitly requests impassable but SetState made it passable
            // (e.g. a wall tile with state Normal), force the override directly.
            if (!passable)
            {
                Tile tile = grid.GetTile(pos);
                if (tile != null)
                    tile.IsPassable = false;
            }
        }

        /// <summary>Builds the shared JsonSerializerOptions used for all deserialization calls.</summary>
        private static JsonSerializerOptions BuildJsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
        }
    }

    // -----------------------------------------------------------------------------------------
    // Result type
    // -----------------------------------------------------------------------------------------

    /// <summary>
    /// The result of a <see cref="MapLoader.ValidateLayout"/> call.
    /// Contains a validity flag and an array of human-readable error messages.
    /// If <see cref="IsValid"/> is true, <see cref="Errors"/> is an empty array.
    /// </summary>
    public class MapLayoutValidationResult
    {
        /// <summary>True if the layout passed all validation checks; false if any errors were found.</summary>
        public bool IsValid { get; }

        /// <summary>
        /// Human-readable descriptions of all validation failures.
        /// Empty when <see cref="IsValid"/> is true.
        /// </summary>
        public string[] Errors { get; }

        /// <summary>
        /// Initializes a new MapLayoutValidationResult.
        /// </summary>
        /// <param name="isValid">Whether the layout is valid.</param>
        /// <param name="errors">Array of error messages (may be empty).</param>
        public MapLayoutValidationResult(bool isValid, string[] errors)
        {
            IsValid = isValid;
            Errors  = errors ?? Array.Empty<string>();
        }
    }
}

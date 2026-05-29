using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Battlemancers.Core.Data
{
    /// <summary>
    /// Runtime POCO representing the full definition of a map loaded from JSON.
    /// Indexed by MapId in the dictionary returned by <see cref="MapLoader"/>.
    /// Zero Unity dependencies — works headless for tests.
    /// </summary>
    public class MapData
    {
        /// <summary>
        /// Unique identifier for this map (e.g., "crossroads", "frozen_wastes").
        /// Used as the dictionary key in <see cref="MapLoader.LoadAll"/>.
        /// </summary>
        public string MapId { get; set; } = "";

        /// <summary>Human-readable name shown in the UI (e.g., "The Crossroads").</summary>
        public string DisplayName { get; set; } = "";

        /// <summary>Width of the grid in tiles (columns).</summary>
        public int Width { get; set; }

        /// <summary>Height of the grid in tiles (rows).</summary>
        public int Height { get; set; }

        /// <summary>
        /// All tiles that make up the map, one entry per grid cell.
        /// Must contain exactly Width × Height entries.
        /// </summary>
        public List<TileData> Tiles { get; set; } = new List<TileData>();

        /// <summary>
        /// Spawn zones for each player, keyed by player identifier (e.g., "player1", "player2").
        /// </summary>
        public List<SpawnZone> SpawnZones { get; set; } = new List<SpawnZone>();

        /// <summary>
        /// Biome tag that drives visual theming (e.g., "ruins", "frozen_wastes", "ember_ridge").
        /// Matched against art asset bundles by the presentation layer.
        /// </summary>
        public string BiomeTag { get; set; } = "";
    }

    /// <summary>
    /// POCO representing the initial state of a single grid cell.
    /// All fields reflect the map's starting configuration before any spells are cast.
    /// Zero Unity dependencies.
    /// </summary>
    public class TileData
    {
        /// <summary>Column coordinate. Range: [0, MapData.Width - 1].</summary>
        public int X { get; set; }

        /// <summary>Row coordinate. Range: [0, MapData.Height - 1].</summary>
        public int Y { get; set; }

        /// <summary>
        /// Structural terrain type of this tile.
        /// Must match a <c>TerrainType</c> enum member name exactly (case-sensitive).
        /// Examples: "Grass", "Stone", "Water", "LavaChannel", "IceField", "Forest",
        /// "Corrupted", "Wall", "Rubble", "Sand", "Void".
        /// </summary>
        public string TerrainType { get; set; } = "Grass";

        /// <summary>
        /// Starting elevation level.
        /// 0 = ground, 1 = raised, 2 = high ground, 3 = peak (rare).
        /// See design/maps/map-design.md for full elevation rules.
        /// </summary>
        public int Elevation { get; set; }

        /// <summary>
        /// Whether this tile can be physically destroyed by spells or explosions.
        /// False for Void, Rubble, LavaChannel, Sand, and Water.
        /// </summary>
        public bool IsDestructible { get; set; }

        /// <summary>
        /// The pre-applied elemental or status state at match start.
        /// Empty string or null means no initial state.
        /// Must match a <c>TileState</c> enum member name (e.g., "Burning", "Frozen", "Wet", "Poisoned").
        /// </summary>
        public string InitialElementState { get; set; } = "";
    }

    /// <summary>
    /// Designates a group of tiles as the spawn area for a specific player.
    /// Spawn tiles must be passable and free of hazard ElementStates in competitive maps.
    /// Zero Unity dependencies.
    /// </summary>
    public class SpawnZone
    {
        /// <summary>
        /// Player identifier this zone belongs to.
        /// Canonical values: "player1", "player2".
        /// Future expansion: "neutral" for scenario objectives.
        /// </summary>
        public string PlayerId { get; set; } = "";

        /// <summary>
        /// Grid coordinates of all tiles in this spawn zone.
        /// Each value tuple represents (X column, Y row).
        /// JSON format: array of objects with "X" and "Y" integer fields,
        /// e.g. [{"X":0,"Y":0},{"X":1,"Y":0}].
        /// </summary>
        [JsonConverter(typeof(SpawnTileListConverter))]
        public List<(int X, int Y)> Tiles { get; set; } = new List<(int X, int Y)>();
    }

    /// <summary>
    /// Custom JSON converter for <c>List&lt;(int X, int Y)&gt;</c>.
    /// Reads and writes an array of objects with "X" and "Y" integer fields.
    /// Required because <c>System.Text.Json</c> does not natively support C# value tuples.
    /// </summary>
    internal sealed class SpawnTileListConverter : JsonConverter<List<(int X, int Y)>>
    {
        public override List<(int X, int Y)> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var result = new List<(int X, int Y)>();

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected start of array for SpawnZone.Tiles.");

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    return result;

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Expected object element in SpawnZone.Tiles array.");

                int x = 0;
                int y = 0;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException("Expected property name inside SpawnZone tile object.");

                    string propName = reader.GetString();
                    reader.Read();

                    if (string.Equals(propName, "X", StringComparison.OrdinalIgnoreCase))
                        x = reader.GetInt32();
                    else if (string.Equals(propName, "Y", StringComparison.OrdinalIgnoreCase))
                        y = reader.GetInt32();
                    // Unknown properties are silently skipped for forward compatibility.
                }

                result.Add((x, y));
            }

            return result;
        }

        public override void Write(
            Utf8JsonWriter writer,
            List<(int X, int Y)> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var (x, y) in value)
            {
                writer.WriteStartObject();
                writer.WriteNumber("X", x);
                writer.WriteNumber("Y", y);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}

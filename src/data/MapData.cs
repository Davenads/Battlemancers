using UnityEngine;

namespace Battlemancers.Data
{
    /// <summary>
    /// Static configuration for a single battlefield map.
    /// One MapData asset exists per preset or custom map in the game.
    ///
    /// MapData is a Unity ScriptableObject that holds descriptive metadata and references for
    /// a map — dimensions, biome identity, layout JSON path, game mode support, environmental
    /// hazards, and the biome's visual tile set. It does NOT hold live simulation state;
    /// that lives in <see cref="Battlemancers.Core.Grid.GridData"/> after
    /// <see cref="Battlemancers.Core.Map.MapLoader"/> builds it at match start.
    ///
    /// To load a map at runtime:
    /// <code>
    ///   string json = Resources.Load&lt;TextAsset&gt;(mapData.LayoutJsonPath).text;
    ///   GridData grid = MapLoader.LoadFromJson(json, mapData.GridWidth, mapData.GridHeight);
    /// </code>
    /// </summary>
    [CreateAssetMenu(fileName = "New Map", menuName = "Battlemancers/Map Data")]
    public class MapData : ScriptableObject
    {
        // -----------------------------------------------------------------------------------------
        // Identity
        // -----------------------------------------------------------------------------------------

        [Header("Identity")]

        /// <summary>
        /// Unique string identifier for this map used internally by the match system.
        /// Must be lowercase, hyphen-separated, and globally unique across all maps.
        /// Examples: "ashfields", "frostveil-crossing", "verdant-ruins", "stormspire-summit".
        /// </summary>
        [Tooltip("Unique internal identifier. Lowercase, hyphen-separated. Must be globally unique.")]
        public string MapId;

        /// <summary>
        /// Human-readable map name displayed in the map selection UI and loading screen.
        /// Example: "The Ashfields", "Frostveil Crossing".
        /// </summary>
        [Tooltip("Map name shown to players in the UI.")]
        public string DisplayName;

        /// <summary>
        /// One or two sentences of flavor text describing the map's setting and tactical character.
        /// Displayed on the map selection screen below the map name.
        /// </summary>
        [Tooltip("Short flavor description shown on the map selection screen.")]
        [TextArea(2, 4)]
        public string Description;

        // -----------------------------------------------------------------------------------------
        // Dimensions
        // -----------------------------------------------------------------------------------------

        [Header("Dimensions")]

        /// <summary>
        /// Number of columns (X axis) on this map's grid.
        /// Must match the width declared in the layout JSON file referenced by LayoutJsonPath.
        /// Standard sizes: 24 (small), 32 (standard competitive), 48 (large/extended).
        /// </summary>
        [Tooltip("Grid width (columns). Must match the width field in the map's layout JSON.")]
        public int GridWidth;

        /// <summary>
        /// Number of rows (Y axis) on this map's grid.
        /// Must match the height declared in the layout JSON file referenced by LayoutJsonPath.
        /// Standard sizes: 24 (small), 32 (standard competitive), 48 (large/extended).
        /// </summary>
        [Tooltip("Grid height (rows). Must match the height field in the map's layout JSON.")]
        public int GridHeight;

        // -----------------------------------------------------------------------------------------
        // Biome
        // -----------------------------------------------------------------------------------------

        [Header("Biome")]

        /// <summary>
        /// Short string tag identifying the map's environmental biome.
        /// Used by the visual layer to select ambient lighting, skybox, and environmental audio.
        /// Expected values: "volcanic", "arctic", "forest", "storm", "desert", "coastal", "undead".
        /// </summary>
        [Tooltip("Biome tag used to select ambient visuals and audio. E.g. 'volcanic', 'arctic', 'forest'.")]
        public string BiomeTag;

        /// <summary>
        /// Path to the JSON layout file containing the tile definitions for this map,
        /// relative to a Unity Resources or StreamingAssets folder.
        /// Example (Resources): "Maps/ashfields" (loaded via Resources.Load&lt;TextAsset&gt;).
        /// Example (StreamingAssets): "Maps/ashfields.json" (loaded via File.ReadAllText).
        /// </summary>
        [Tooltip("Path to the layout JSON file. Relative to Resources (no extension) or StreamingAssets (with extension).")]
        public string LayoutJsonPath;

        /// <summary>
        /// ScriptableObject references for the TileTypeData assets belonging to this biome's tile set.
        /// The visual layer uses these to drive material swaps and VFX for each tile state on this map.
        /// Biome tile sets may include custom material variants for Normal, Burning, Frozen, etc. that
        /// match the map's environmental palette (e.g., volcanic obsidian vs. arctic ice).
        /// </summary>
        [Tooltip("TileTypeData assets for this biome's tile visual set. Drives material/VFX selection in the presentation layer.")]
        public TileTypeData[] BiomeTileTypes;

        // -----------------------------------------------------------------------------------------
        // Player and Game Mode
        // -----------------------------------------------------------------------------------------

        [Header("Player and Game Mode")]

        /// <summary>
        /// Minimum number of players required to play this map.
        /// Most skirmish and campaign maps require exactly 2 players (1v1).
        /// Reserved for future co-op or multi-faction modes.
        /// </summary>
        [Tooltip("Minimum number of players needed to play this map.")]
        [Range(1, 4)]
        public int RecommendedMinPlayers = 1;

        /// <summary>
        /// Maximum number of players supported on this map.
        /// Most skirmish and campaign maps support exactly 2 players (1v1).
        /// Reserved for future co-op or multi-faction modes.
        /// </summary>
        [Tooltip("Maximum number of players this map supports.")]
        [Range(1, 4)]
        public int RecommendedMaxPlayers = 2;

        /// <summary>
        /// List of game mode identifiers this map is playable in.
        /// Known modes: "Skirmish", "Campaign", "Draft", "Ranked".
        /// Maps not listed for a mode will not appear in that mode's map pool.
        /// </summary>
        [Tooltip("Game modes this map is available in. E.g. [\"Skirmish\", \"Campaign\"].")]
        public string[] SupportedGameModes;

        // -----------------------------------------------------------------------------------------
        // Environmental Hazards
        // -----------------------------------------------------------------------------------------

        [Header("Environmental Hazards")]

        /// <summary>
        /// IDs of environmental hazard systems that are active on this map throughout a match.
        /// Hazard systems are registered with the simulation's HazardRegistry and triggered
        /// on their defined schedule (e.g., every N turns, in phase X of the round).
        /// Examples: "lava_surge" (periodic lava expansion on volcanic maps),
        /// "blizzard" (arctic wind that applies Frozen to outer tiles),
        /// "lightning_strike" (random Charged tile generation on storm maps).
        /// An empty array means no environmental hazards are active.
        /// </summary>
        [Tooltip("IDs of periodic environmental hazards active on this map. Resolved by HazardRegistry at runtime.")]
        public string[] EnvironmentalHazardIds;

        // -----------------------------------------------------------------------------------------
        // Design Notes
        // -----------------------------------------------------------------------------------------

        [Header("Design Notes")]

        /// <summary>Internal design notes for the development team. Not shown to players.</summary>
        [TextArea(2, 6)]
        public string DesignNotes;
    }
}

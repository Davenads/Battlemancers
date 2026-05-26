using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Battlemancers.Simulation
{
    /// <summary>
    /// Resolves elemental interactions between incoming spell elements and the current
    /// state of a target tile. This is the core of the element combo system.
    ///
    /// The resolver maintains a lookup table keyed by "{TileStateName}+{ElementTypeName}"
    /// (e.g., "Wet+Lightning", "Burning+Fire"). Entries are loaded from the JSON interaction
    /// table at assets/data/element-interactions.json. New interactions can be authored in
    /// JSON without modifying this class.
    ///
    /// This class has zero Unity dependencies and is fully testable in headless C#.
    ///
    /// Usage:
    /// <code>
    ///   var resolver = ElementResolver.CreateDefault();
    ///   Interaction result = resolver.Resolve("Wet", ElementType.Lightning);
    ///   // result.ResultingTileState == "Wet" (chain arc does not change the tile state)
    ///   // result.Effects contains CHAIN_TO_ADJACENT and STATUS_APPLY STUNNED effects
    /// </code>
    /// </summary>
    public class ElementResolver
    {
        // -----------------------------------------------------------------------
        // Internal data structures for JSON deserialization
        // -----------------------------------------------------------------------

        private sealed class JsonEffect
        {
            public string effectType { get; set; }
            public string target { get; set; }
            public string statusId { get; set; }
            public int value { get; set; }
        }

        private sealed class JsonInteraction
        {
            public string tileState { get; set; }
            public string element { get; set; }
            public string resultingTileState { get; set; }
            public JsonEffect[] effects { get; set; }
            public string vfxHint { get; set; }
        }

        private sealed class JsonInteractionTable
        {
            public JsonInteraction[] interactions { get; set; }
        }

        // -----------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------

        /// <summary>
        /// The interaction lookup table. Keys are formatted as "{TileStateName}+{ElementTypeName}",
        /// e.g. "Wet+Lightning". Populated by <see cref="LoadFromJson"/>.
        /// </summary>
        private readonly Dictionary<string, Interaction> _table;

        /// <summary>
        /// Singleton no-op interaction returned when no entry exists in the table.
        /// Represents "this element has no special interaction with this tile state."
        /// ResultingTileState is "Normal" and Effects is empty.
        /// </summary>
        private static readonly Interaction _noInteraction =
            new Interaction("Normal", Array.Empty<Effect>(), string.Empty);

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        /// <summary>
        /// Initializes an empty ElementResolver. Call <see cref="LoadFromJson"/> to populate
        /// the interaction table, or use <see cref="CreateDefault"/> to get a fully loaded instance.
        /// </summary>
        public ElementResolver()
        {
            _table = new Dictionary<string, Interaction>(StringComparer.OrdinalIgnoreCase);
        }

        // -----------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------

        /// <summary>
        /// Looks up the interaction result for the given tile state and incoming element type.
        ///
        /// Returns the matching <see cref="Interaction"/> if one is registered in the table.
        /// If no entry exists, returns a default no-op Interaction with an empty Effects array
        /// and a ResultingTileState of "Normal". Callers should use <see cref="HasInteraction"/>
        /// to determine whether a meaningful interaction was found before applying effects.
        /// </summary>
        /// <param name="tileState">
        /// The current primary state of the tile, as a string matching a TileState enum name
        /// (e.g., "Wet", "Burning", "Frozen", "Normal"). Case-insensitive.
        /// </param>
        /// <param name="incoming">The element type of the incoming spell.</param>
        /// <returns>
        /// The registered <see cref="Interaction"/> for this combination, or a no-op
        /// Interaction if no entry is found.
        /// </returns>
        public Interaction Resolve(string tileState, ElementType incoming)
        {
            string key = BuildKey(tileState, incoming);
            if (_table.TryGetValue(key, out Interaction interaction))
                return interaction;

            return _noInteraction;
        }

        /// <summary>
        /// Populates the interaction table from a JSON string.
        ///
        /// The JSON must conform to the schema in assets/data/element-interactions.json:
        /// a root object with an "interactions" array, where each entry has:
        /// tileState, element, resultingTileState, effects[], and vfxHint.
        ///
        /// Existing entries in the table are preserved; duplicate keys are overwritten
        /// with the new value (last write wins). This allows partial override tables
        /// to be layered on top of the default table.
        ///
        /// Throws <see cref="JsonException"/> if the JSON is malformed.
        /// Throws <see cref="ArgumentNullException"/> if <paramref name="json"/> is null.
        /// </summary>
        /// <param name="json">
        /// The full JSON content of the interaction table file.
        /// </param>
        public void LoadFromJson(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var table = JsonSerializer.Deserialize<JsonInteractionTable>(json, options);
            if (table?.interactions == null) return;

            foreach (var entry in table.interactions)
            {
                if (string.IsNullOrWhiteSpace(entry.tileState) ||
                    string.IsNullOrWhiteSpace(entry.element))
                    continue;

                if (!Enum.TryParse<ElementType>(entry.element, ignoreCase: true, out var elementType))
                    continue;

                Effect[] effects = ConvertEffects(entry.effects);
                var interaction = new Interaction(
                    entry.resultingTileState ?? "Normal",
                    effects,
                    entry.vfxHint ?? string.Empty
                );

                string key = BuildKey(entry.tileState, elementType);
                _table[key] = interaction;
            }
        }

        /// <summary>
        /// Returns true if a non-trivial interaction is registered for the given tile state
        /// and incoming element. A "non-trivial" interaction is any entry in the table that
        /// has at least one effect OR a ResultingTileState different from "Normal".
        ///
        /// Returns false if no entry exists or if the entry is explicitly a no-op
        /// (empty effects, ResultingTileState is "Normal").
        /// </summary>
        /// <param name="tileState">
        /// The current primary state of the tile, as a string matching a TileState enum name.
        /// Case-insensitive.
        /// </param>
        /// <param name="incoming">The element type of the incoming spell.</param>
        /// <returns>
        /// True if a meaningful interaction exists and will produce effects or a state change.
        /// </returns>
        public bool HasInteraction(string tileState, ElementType incoming)
        {
            string key = BuildKey(tileState, incoming);
            if (!_table.TryGetValue(key, out Interaction interaction))
                return false;

            return interaction.Effects.Length > 0 ||
                   !string.Equals(interaction.ResultingTileState, "Normal",
                       StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates a fully initialized ElementResolver pre-loaded with the complete
        /// element interaction table from the hardcoded embedded JSON fallback.
        ///
        /// This factory is used when the external JSON file is unavailable (e.g., during
        /// unit tests, headless simulation, or when the asset pipeline hasn't run).
        /// In production, the caller should load from the JSON file at runtime using
        /// <see cref="LoadFromJson"/> and pass the file contents after instantiation.
        ///
        /// The embedded table covers all combinations from the CLAUDE.md interaction matrix:
        /// 7 element types × 6 reactive tile states (Wet, Burning, Frozen, Poisoned, Charged,
        /// Normal) plus additional states (Mud, Vines, Spores, Natural, Steam, Corrupted,
        /// Obsidian, Permafrost, Destroyed).
        /// </summary>
        /// <returns>
        /// A new <see cref="ElementResolver"/> instance with the full interaction table loaded.
        /// </returns>
        public static ElementResolver CreateDefault()
        {
            var resolver = new ElementResolver();
            resolver.LoadFromJson(EmbeddedInteractionTableJson);
            return resolver;
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        private static string BuildKey(string tileState, ElementType element)
        {
            return tileState.Trim() + "+" + element.ToString();
        }

        private static Effect[] ConvertEffects(JsonEffect[] jsonEffects)
        {
            if (jsonEffects == null || jsonEffects.Length == 0)
                return Array.Empty<Effect>();

            var effects = new Effect[jsonEffects.Length];
            for (int i = 0; i < jsonEffects.Length; i++)
            {
                var je = jsonEffects[i];
                effects[i] = new Effect(
                    je.effectType ?? string.Empty,
                    je.target ?? string.Empty,
                    je.statusId,
                    je.value
                );
            }
            return effects;
        }

        // -----------------------------------------------------------------------
        // Embedded fallback JSON
        // -----------------------------------------------------------------------

        /// <summary>
        /// Embedded copy of the full interaction table as a JSON literal string.
        /// Kept in sync with assets/data/element-interactions.json.
        /// Used by <see cref="CreateDefault"/> when the external file is unavailable.
        /// </summary>
        private static readonly string EmbeddedInteractionTableJson = @"
{
  ""interactions"": [
    { ""tileState"": ""Wet"", ""element"": ""Fire"", ""resultingTileState"": ""Steam"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""BURNING"", ""value"": 2 }, { ""effectType"": ""VISION_REDUCE"", ""target"": ""AOE_2"", ""statusId"": null, ""value"": 3 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""steam_cloud"" },
    { ""tileState"": ""Wet"", ""element"": ""Water"", ""resultingTileState"": ""Wet"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Wet"", ""element"": ""Ice"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""FROZEN"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""freeze_tile"" },
    { ""tileState"": ""Wet"", ""element"": ""Lightning"", ""resultingTileState"": ""Wet"", ""effects"": [{ ""effectType"": ""CHAIN_TO_ADJACENT"", ""target"": ""ADJACENT_WET_UNITS"", ""statusId"": null, ""value"": 100 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""STUNNED"", ""value"": 1 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""ADJACENT_WET_UNITS"", ""statusId"": ""STUNNED"", ""value"": 1 }], ""vfxHint"": ""chain_arc"" },
    { ""tileState"": ""Wet"", ""element"": ""Earth"", ""resultingTileState"": ""Mud"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""SLOWED"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""mud_splash"" },
    { ""tileState"": ""Wet"", ""element"": ""Wind"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""VISION_REDUCE"", ""target"": ""AOE_2"", ""statusId"": null, ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""mist_dispersal"" },
    { ""tileState"": ""Wet"", ""element"": ""Poison"", ""resultingTileState"": ""Poisoned"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 3 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""infected_water"" },

    { ""tileState"": ""Burning"", ""element"": ""Fire"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""ADJACENT_UNITS"", ""statusId"": null, ""value"": 0 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""BURNING"", ""value"": 3 }], ""vfxHint"": ""fire_spread"" },
    { ""tileState"": ""Burning"", ""element"": ""Water"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""extinguish"" },
    { ""tileState"": ""Burning"", ""element"": ""Ice"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""FROZEN"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""flash_freeze"" },
    { ""tileState"": ""Burning"", ""element"": ""Lightning"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""AOE_2"", ""statusId"": null, ""value"": 12 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""AOE_2"", ""statusId"": ""BURNING"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""firestorm_burst"" },
    { ""tileState"": ""Burning"", ""element"": ""Earth"", ""resultingTileState"": ""Obsidian"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""obsidian_form"" },
    { ""tileState"": ""Burning"", ""element"": ""Wind"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""ADJACENT_UNITS"", ""statusId"": null, ""value"": 0 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""ADJACENT_UNITS"", ""statusId"": ""BURNING"", ""value"": 2 }], ""vfxHint"": ""fan_flames"" },
    { ""tileState"": ""Burning"", ""element"": ""Poison"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""AOE_2"", ""statusId"": null, ""value"": 8 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""AOE_2"", ""statusId"": ""POISONED"", ""value"": 2 }], ""vfxHint"": ""toxic_fire"" },

    { ""tileState"": ""Frozen"", ""element"": ""Fire"", ""resultingTileState"": ""Wet"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""melt_ice"" },
    { ""tileState"": ""Frozen"", ""element"": ""Water"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""HIT_UNIT"", ""statusId"": null, ""value"": 6 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""crack_ice"" },
    { ""tileState"": ""Frozen"", ""element"": ""Ice"", ""resultingTileState"": ""Permafrost"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""SLOWED"", ""value"": 3 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""deep_freeze"" },
    { ""tileState"": ""Frozen"", ""element"": ""Lightning"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""AOE_2"", ""statusId"": null, ""value"": 18 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""ice_shatter"" },
    { ""tileState"": ""Frozen"", ""element"": ""Earth"", ""resultingTileState"": ""Permafrost"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""permafrost_cover"" },
    { ""tileState"": ""Frozen"", ""element"": ""Wind"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""AOE_2"", ""statusId"": null, ""value"": 7 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""AOE_2"", ""statusId"": ""BLINDED"", ""value"": 1 }], ""vfxHint"": ""ice_shard_spray"" },
    { ""tileState"": ""Frozen"", ""element"": ""Poison"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 3 }], ""vfxHint"": ""preserved_poison"" },

    { ""tileState"": ""Poisoned"", ""element"": ""Fire"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""AOE_2"", ""statusId"": null, ""value"": 10 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""toxic_fumes"" },
    { ""tileState"": ""Poisoned"", ""element"": ""Water"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""dilute_poison"" },
    { ""tileState"": ""Poisoned"", ""element"": ""Ice"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 3 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""preserve_freeze"" },
    { ""tileState"": ""Poisoned"", ""element"": ""Lightning"", ""resultingTileState"": ""Poisoned"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""HIT_UNIT"", ""statusId"": null, ""value"": 14 }, { ""effectType"": ""STACK_MULTIPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 2 }], ""vfxHint"": ""toxin_shock"" },
    { ""tileState"": ""Poisoned"", ""element"": ""Earth"", ""resultingTileState"": ""Poisoned"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""ADJACENT_UNITS"", ""statusId"": null, ""value"": 0 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""ADJACENT_UNITS"", ""statusId"": ""POISONED"", ""value"": 2 }], ""vfxHint"": ""contaminate_ground"" },
    { ""tileState"": ""Poisoned"", ""element"": ""Wind"", ""resultingTileState"": ""Spores"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""AOE_2"", ""statusId"": ""POISONED"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""disperse_spores"" },
    { ""tileState"": ""Poisoned"", ""element"": ""Poison"", ""resultingTileState"": ""Poisoned"", ""effects"": [{ ""effectType"": ""STACK_MULTIPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 2 }], ""vfxHint"": ""stack_multiplier"" },

    { ""tileState"": ""Charged"", ""element"": ""Fire"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""AOE_2"", ""statusId"": null, ""value"": 15 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""AOE_2"", ""statusId"": ""BURNING"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""arc_explosion"" },
    { ""tileState"": ""Charged"", ""element"": ""Water"", ""resultingTileState"": ""Wet"", ""effects"": [{ ""effectType"": ""CHAIN_TO_ADJACENT"", ""target"": ""ADJACENT_UNITS"", ""statusId"": null, ""value"": 100 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""ADJACENT_UNITS"", ""statusId"": ""STUNNED"", ""value"": 1 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""chain_stun"" },
    { ""tileState"": ""Charged"", ""element"": ""Ice"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""FROZEN"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""freeze_conductor"" },
    { ""tileState"": ""Charged"", ""element"": ""Lightning"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""AOE_2"", ""statusId"": null, ""value"": 20 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""AOE_2"", ""statusId"": ""STUNNED"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""overload_burst"" },
    { ""tileState"": ""Charged"", ""element"": ""Earth"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""PUSH"", ""target"": ""ADJACENT_UNITS"", ""statusId"": null, ""value"": 1 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""magnetize"" },
    { ""tileState"": ""Charged"", ""element"": ""Wind"", ""resultingTileState"": ""Charged"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""STUNNED"", ""value"": 1 }], ""vfxHint"": ""static_buildup"" },
    { ""tileState"": ""Charged"", ""element"": ""Poison"", ""resultingTileState"": ""Poisoned"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""corroded_conductor"" },

    { ""tileState"": ""Normal"", ""element"": ""Fire"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""BURNING"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""fire_ignite"" },
    { ""tileState"": ""Normal"", ""element"": ""Water"", ""resultingTileState"": ""Wet"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""wet_splash"" },
    { ""tileState"": ""Normal"", ""element"": ""Ice"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""SLOWED"", ""value"": 1 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""ice_coat"" },
    { ""tileState"": ""Normal"", ""element"": ""Lightning"", ""resultingTileState"": ""Charged"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""HIT_UNIT"", ""statusId"": null, ""value"": 8 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""lightning_strike"" },
    { ""tileState"": ""Normal"", ""element"": ""Earth"", ""resultingTileState"": ""Normal"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Normal"", ""element"": ""Wind"", ""resultingTileState"": ""Normal"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Normal"", ""element"": ""Poison"", ""resultingTileState"": ""Poisoned"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""poison_pool"" },

    { ""tileState"": ""Mud"", ""element"": ""Fire"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""dry_mud"" },
    { ""tileState"": ""Mud"", ""element"": ""Water"", ""resultingTileState"": ""Mud"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""SLOWED"", ""value"": 2 }], ""vfxHint"": """" },
    { ""tileState"": ""Mud"", ""element"": ""Ice"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""freeze_mud"" },
    { ""tileState"": ""Mud"", ""element"": ""Lightning"", ""resultingTileState"": ""Mud"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""STUNNED"", ""value"": 1 }], ""vfxHint"": ""mud_arc"" },
    { ""tileState"": ""Mud"", ""element"": ""Earth"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""compact_mud"" },
    { ""tileState"": ""Mud"", ""element"": ""Wind"", ""resultingTileState"": ""Mud"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Mud"", ""element"": ""Poison"", ""resultingTileState"": ""Poisoned"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""toxic_mud"" },

    { ""tileState"": ""Vines"", ""element"": ""Fire"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""BURNING"", ""value"": 2 }], ""vfxHint"": ""vine_burn"" },
    { ""tileState"": ""Vines"", ""element"": ""Water"", ""resultingTileState"": ""Vines"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Vines"", ""element"": ""Ice"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""freeze_vines"" },
    { ""tileState"": ""Vines"", ""element"": ""Lightning"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""HIT_UNIT"", ""statusId"": null, ""value"": 10 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""vine_ignite"" },
    { ""tileState"": ""Vines"", ""element"": ""Earth"", ""resultingTileState"": ""Vines"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Vines"", ""element"": ""Wind"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""shred_vines"" },
    { ""tileState"": ""Vines"", ""element"": ""Poison"", ""resultingTileState"": ""Spores"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""poison_vines"" },

    { ""tileState"": ""Spores"", ""element"": ""Fire"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""AOE_2"", ""statusId"": null, ""value"": 8 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""spore_ignite"" },
    { ""tileState"": ""Spores"", ""element"": ""Water"", ""resultingTileState"": ""Poisoned"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""wet_spores"" },
    { ""tileState"": ""Spores"", ""element"": ""Ice"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""freeze_spores"" },
    { ""tileState"": ""Spores"", ""element"": ""Lightning"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""AOE_2"", ""statusId"": null, ""value"": 12 }, { ""effectType"": ""STATUS_APPLY"", ""target"": ""AOE_2"", ""statusId"": ""POISONED"", ""value"": 3 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""spore_shock"" },
    { ""tileState"": ""Spores"", ""element"": ""Earth"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""bury_spores"" },
    { ""tileState"": ""Spores"", ""element"": ""Wind"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""AOE_2"", ""statusId"": ""POISONED"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""disperse_spores"" },
    { ""tileState"": ""Spores"", ""element"": ""Poison"", ""resultingTileState"": ""Spores"", ""effects"": [{ ""effectType"": ""STACK_MULTIPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 2 }], ""vfxHint"": ""dense_spores"" },

    { ""tileState"": ""Natural"", ""element"": ""Fire"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""BURNING"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""ADJACENT_UNITS"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""fast_burn"" },
    { ""tileState"": ""Natural"", ""element"": ""Water"", ""resultingTileState"": ""Natural"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Natural"", ""element"": ""Ice"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""frost_nature"" },
    { ""tileState"": ""Natural"", ""element"": ""Lightning"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""HIT_UNIT"", ""statusId"": null, ""value"": 8 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""nature_strike"" },
    { ""tileState"": ""Natural"", ""element"": ""Earth"", ""resultingTileState"": ""Natural"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Natural"", ""element"": ""Wind"", ""resultingTileState"": ""Natural"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Natural"", ""element"": ""Poison"", ""resultingTileState"": ""Spores"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 2 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""nature_spores"" },

    { ""tileState"": ""Steam"", ""element"": ""Fire"", ""resultingTileState"": ""Steam"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""BURNING"", ""value"": 1 }], ""vfxHint"": """" },
    { ""tileState"": ""Steam"", ""element"": ""Water"", ""resultingTileState"": ""Wet"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""condense_steam"" },
    { ""tileState"": ""Steam"", ""element"": ""Ice"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""freeze_steam"" },
    { ""tileState"": ""Steam"", ""element"": ""Lightning"", ""resultingTileState"": ""Charged"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""STUNNED"", ""value"": 1 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""steam_arc"" },
    { ""tileState"": ""Steam"", ""element"": ""Earth"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""absorb_steam"" },
    { ""tileState"": ""Steam"", ""element"": ""Wind"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""disperse_steam"" },
    { ""tileState"": ""Steam"", ""element"": ""Poison"", ""resultingTileState"": ""Steam"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""AOE_2"", ""statusId"": ""POISONED"", ""value"": 1 }], ""vfxHint"": ""toxic_steam"" },

    { ""tileState"": ""Corrupted"", ""element"": ""Fire"", ""resultingTileState"": ""Burning"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""corrupt_burn"" },
    { ""tileState"": ""Corrupted"", ""element"": ""Water"", ""resultingTileState"": ""Corrupted"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Corrupted"", ""element"": ""Ice"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""freeze_corruption"" },
    { ""tileState"": ""Corrupted"", ""element"": ""Lightning"", ""resultingTileState"": ""Charged"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""corrupt_charge"" },
    { ""tileState"": ""Corrupted"", ""element"": ""Earth"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""cleanse_corruption"" },
    { ""tileState"": ""Corrupted"", ""element"": ""Wind"", ""resultingTileState"": ""Corrupted"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Corrupted"", ""element"": ""Poison"", ""resultingTileState"": ""Corrupted"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 2 }], ""vfxHint"": ""corrupt_poison"" },

    { ""tileState"": ""Obsidian"", ""element"": ""Fire"", ""resultingTileState"": ""Obsidian"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Obsidian"", ""element"": ""Water"", ""resultingTileState"": ""Obsidian"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Obsidian"", ""element"": ""Ice"", ""resultingTileState"": ""Obsidian"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Obsidian"", ""element"": ""Lightning"", ""resultingTileState"": ""Obsidian"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Obsidian"", ""element"": ""Earth"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""shatter_obsidian"" },
    { ""tileState"": ""Obsidian"", ""element"": ""Wind"", ""resultingTileState"": ""Obsidian"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Obsidian"", ""element"": ""Poison"", ""resultingTileState"": ""Obsidian"", ""effects"": [], ""vfxHint"": """" },

    { ""tileState"": ""Permafrost"", ""element"": ""Fire"", ""resultingTileState"": ""Wet"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""melt_permafrost"" },
    { ""tileState"": ""Permafrost"", ""element"": ""Water"", ""resultingTileState"": ""Frozen"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""water_permafrost"" },
    { ""tileState"": ""Permafrost"", ""element"": ""Ice"", ""resultingTileState"": ""Permafrost"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Permafrost"", ""element"": ""Lightning"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""DAMAGE"", ""target"": ""AOE_2"", ""statusId"": null, ""value"": 22 }, { ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""permafrost_shatter"" },
    { ""tileState"": ""Permafrost"", ""element"": ""Earth"", ""resultingTileState"": ""Permafrost"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Permafrost"", ""element"": ""Wind"", ""resultingTileState"": ""Permafrost"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""SLOWED"", ""value"": 2 }], ""vfxHint"": ""permafrost_gust"" },
    { ""tileState"": ""Permafrost"", ""element"": ""Poison"", ""resultingTileState"": ""Permafrost"", ""effects"": [{ ""effectType"": ""STATUS_APPLY"", ""target"": ""HIT_UNIT"", ""statusId"": ""POISONED"", ""value"": 3 }], ""vfxHint"": ""preserved_poison"" },

    { ""tileState"": ""Destroyed"", ""element"": ""Fire"", ""resultingTileState"": ""Destroyed"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Destroyed"", ""element"": ""Water"", ""resultingTileState"": ""Destroyed"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Destroyed"", ""element"": ""Ice"", ""resultingTileState"": ""Destroyed"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Destroyed"", ""element"": ""Lightning"", ""resultingTileState"": ""Destroyed"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Destroyed"", ""element"": ""Earth"", ""resultingTileState"": ""Normal"", ""effects"": [{ ""effectType"": ""TERRAIN_CHANGE"", ""target"": ""TILE"", ""statusId"": null, ""value"": 0 }], ""vfxHint"": ""fill_crater"" },
    { ""tileState"": ""Destroyed"", ""element"": ""Wind"", ""resultingTileState"": ""Destroyed"", ""effects"": [], ""vfxHint"": """" },
    { ""tileState"": ""Destroyed"", ""element"": ""Poison"", ""resultingTileState"": ""Destroyed"", ""effects"": [], ""vfxHint"": """" }
  ]
}
";
    }
}

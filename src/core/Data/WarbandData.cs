using System.Collections.Generic;

namespace Battlemancers.Core.Data
{
    /// <summary>
    /// A complete saved warband list. One instance = one player-created list.
    /// Serialized to JSON in Application.persistentDataPath/warbands/{WarbandId}.json
    /// </summary>
    public class WarbandData
    {
        /// <summary>Unique identifier — generated once on creation, never changed.</summary>
        public string WarbandId { get; set; }

        /// <summary>Player-facing display name (max 32 chars).</summary>
        public string Name { get; set; }

        /// <summary>Faction identifier: "gilded_throne", "verdant_pact", or "ashen_covenant".</summary>
        public string FactionId { get; set; }

        /// <summary>Up to 3 Mancer slots. Each slot specifies the archetype and chosen upgrades.</summary>
        public List<WarbandMancerSlot> Mancers { get; set; } = new();

        /// <summary>Support unit allocation for this warband.</summary>
        public WarbandSupportUnits SupportUnits { get; set; } = new();

        /// <summary>UTC timestamp of creation. ISO 8601 string.</summary>
        public string CreatedAt { get; set; }

        /// <summary>UTC timestamp of last save. ISO 8601 string.</summary>
        public string LastModifiedAt { get; set; }

        /// <summary>
        /// Total point cost. Computed property — always re-calculate from components rather than storing.
        /// This stored value is a cache; call WarbandValidator.CalculateTotalCost for authoritative value.
        /// </summary>
        public int CachedTotalCost { get; set; }
    }

    /// <summary>One Mancer slot in a warband — archetype plus chosen upgrades.</summary>
    public class WarbandMancerSlot
    {
        /// <summary>Mancer archetype identifier (e.g., "pyromancer", "hydromancer").</summary>
        public string MancerId { get; set; }

        /// <summary>
        /// IDs of chosen upgrades for this Mancer.
        /// Each upgrade ID maps to an UpgradeOption defined in the Mancer's data file.
        /// </summary>
        public List<string> SelectedUpgradeIds { get; set; } = new();

        /// <summary>Total cost of this slot: 100 (base) + sum of selected upgrade costs.</summary>
        public int TotalCost { get; set; }
    }

    /// <summary>Support unit counts for a warband (non-Mancer units).</summary>
    public class WarbandSupportUnits
    {
        /// <summary>Number of Tier 1 Chaff units (faction-specific; e.g., Conscript Spearmen).</summary>
        public int ChaffT1Count { get; set; }

        /// <summary>Number of Tier 2 veteran Chaff units (e.g., Iron Vanguard).</summary>
        public int ChaffT2Count { get; set; }

        /// <summary>Number of Tier 1 Ranged units (e.g., Crossbow Corps).</summary>
        public int RangedT1Count { get; set; }

        /// <summary>Number of Tier 2 veteran Ranged units (e.g., Siege Arbalest).</summary>
        public int RangedT2Count { get; set; }
    }

    /// <summary>
    /// Manifest file stored at persistentDataPath/warbands/manifest.json.
    /// Lists all saved warband IDs and names for fast loading of the warband list screen
    /// without deserializing every full warband file.
    /// </summary>
    public class WarbandManifest
    {
        public List<WarbandManifestEntry> Entries { get; set; } = new();
    }

    /// <summary>A lightweight summary of a saved warband, stored in the manifest.</summary>
    public class WarbandManifestEntry
    {
        public string WarbandId { get; set; }
        public string Name { get; set; }
        public string FactionId { get; set; }
        public int CachedTotalCost { get; set; }
        public string LastModifiedAt { get; set; }
    }
}

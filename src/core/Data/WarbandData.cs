using System;
using System.Collections.Generic;

namespace Battlemancers.Core.Data
{
    /// <summary>
    /// Serializable representation of a saved player warband.
    /// Loaded and saved by WarbandRepository. Never mutated at runtime during a match.
    /// </summary>
    public class WarbandData
    {
        public string WarbandId { get; set; } = "";          // GUID generated on creation
        public string Name { get; set; } = "New Warband";
        public string FactionId { get; set; } = "";          // "gilded_throne" | "verdant_pact" | "ashen_covenant"
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public List<WarbandMancerEntry> Mancers { get; set; } = new();
        public List<WarbandSupportEntry> SupportUnits { get; set; } = new();
        public int TotalPoints { get; set; }                  // cached sum for quick display
        public bool IsValid { get; set; }                     // cached result of last validation
    }

    public class WarbandMancerEntry
    {
        public string MancerId { get; set; } = "";
        public List<string> UpgradeIds { get; set; } = new();
        public int TotalCost { get; set; }  // 100 base + sum of upgrade costs
    }

    public class WarbandSupportEntry
    {
        public string UnitTypeId { get; set; } = "";  // e.g. "conscript_spearmen"
        public int Tier { get; set; } = 1;            // 1 or 2
        public int Count { get; set; }
        public int CostPerUnit { get; set; }
        public int TotalCost => CostPerUnit * Count;  // computed property, not serialized separately
    }
}

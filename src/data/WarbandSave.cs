using System;
using System.Collections.Generic;

namespace Battlemancers.Data
{
    /// <summary>
    /// Complete warband configuration for one player. Serialized to JSON for save and load.
    ///
    /// This is a pure C# class with zero Unity dependencies. It uses only base C# types so it
    /// can be serialized by both Unity's JsonUtility (simple cases) and Newtonsoft.Json (full
    /// fidelity, including nested lists and calculated properties).
    ///
    /// Usage:
    ///   - Create: WarbandSave.CreateNew(factionId, displayName)
    ///   - Validate: save.IsValid
    ///   - Serialize: JsonConvert.SerializeObject(save) (Newtonsoft) or JsonUtility.ToJson(save) (Unity)
    ///   - Deserialize: JsonConvert.DeserializeObject&lt;WarbandSave&gt;(json)
    ///
    /// Runtime references to ScriptableObjects (MancerData, FactionData, SpellData, UpgradeOption) are
    /// resolved by WarbandLoader at game start using the string ID fields in this class.
    /// WarbandSave itself never holds Unity asset references — only string keys.
    ///
    /// Point budget rules enforced by IsValid:
    ///   - Total cost must not exceed 1,000 pts
    ///   - At least 1 Mancer required (max 3)
    ///   - No duplicate Mancer archetypes
    ///   - factionId must be non-empty
    /// </summary>
    [Serializable]
    public class WarbandSave
    {
        // -----------------------------------------------------------------------------------------
        // Metadata
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Unique save identifier. Generated as a GUID when the warband is first created.
        /// Used as the filename on disk (e.g., "{saveId}.json") and as the primary key in the save index.
        /// </summary>
        public string saveId;

        /// <summary>Player-chosen name for this warband (e.g., "Flame Rush Build", "Defensive Pact").</summary>
        public string displayName;

        /// <summary>
        /// Faction this warband belongs to. Must match a FactionData.factionId.
        /// Valid values: "gilded_throne", "verdant_pact", "ashen_covenant".
        /// </summary>
        public string factionId;

        /// <summary>Unix timestamp (seconds since epoch, UTC) when this warband was first created.</summary>
        public long createdTimestamp;

        /// <summary>Unix timestamp (seconds since epoch, UTC) of the most recent modification.</summary>
        public long lastModifiedTimestamp;

        /// <summary>
        /// Schema version for forward-compatibility. Increment when the save format changes.
        /// WarbandLoader checks this against the current version and migrates older saves if needed.
        /// </summary>
        public int schemaVersion = 1;

        // -----------------------------------------------------------------------------------------
        // Roster
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// All Mancer loadouts included in this warband. 1 to 3 entries.
        /// No duplicate mancerArchetypeId values allowed.
        /// Each Mancer has a base cost of 100 pts plus the sum of its upgrade additionalCosts.
        /// </summary>
        public List<MancerLoadout> mancers = new List<MancerLoadout>();

        /// <summary>
        /// Support unit (Chaff and Ranged) entries included in this warband.
        /// Each entry specifies a unit type and how many are purchased.
        /// Point cost per unit is cached at save time in SupportUnitCount.unitPointCost.
        /// </summary>
        public List<SupportUnitCount> supportUnits = new List<SupportUnitCount>();

        // -----------------------------------------------------------------------------------------
        // Computed Properties
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Total warband point cost: sum of all Mancer base costs (100 each) + all upgrade costs
        /// + all support unit costs (unitPointCost * count). Must not exceed 1,000.
        /// </summary>
        public int TotalPointCost
        {
            get
            {
                int total = 0;

                foreach (var mancer in mancers)
                {
                    total += 100; // Base Mancer cost is always 100 pts regardless of upgrades
                    if (mancer.upgradeIds != null)
                    {
                        foreach (var upgrade in mancer.upgradeIds)
                        {
                            total += upgrade.additionalCost;
                        }
                    }
                }

                if (supportUnits != null)
                {
                    foreach (var unit in supportUnits)
                    {
                        total += unit.unitPointCost * unit.count;
                    }
                }

                return total;
            }
        }

        /// <summary>
        /// Total warband point cost minus the Mancer base costs.
        /// Useful for the warband builder's "support unit budget remaining" display.
        /// </summary>
        public int SupportUnitPointCost
        {
            get
            {
                int total = 0;
                if (supportUnits != null)
                {
                    foreach (var unit in supportUnits)
                    {
                        total += unit.unitPointCost * unit.count;
                    }
                }
                return total;
            }
        }

        /// <summary>Remaining points available to spend. Non-negative means the warband is within budget.</summary>
        public int RemainingPoints => 1000 - TotalPointCost;

        /// <summary>
        /// Total number of individual support units (sum of all SupportUnitCount.count values).
        /// Informational; displayed in the warband builder summary.
        /// </summary>
        public int TotalSupportUnitCount
        {
            get
            {
                int count = 0;
                if (supportUnits != null)
                {
                    foreach (var unit in supportUnits)
                    {
                        count += unit.count;
                    }
                }
                return count;
            }
        }

        // -----------------------------------------------------------------------------------------
        // Validation
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// True if this warband meets all construction rules and can be submitted for a match.
        /// Checks: at least 1 Mancer, no more than 3 Mancers, total cost within 1,000 pts,
        /// no duplicate Mancer archetypes, and a valid factionId.
        /// </summary>
        public bool IsValid =>
            mancers != null &&
            mancers.Count >= 1 &&
            mancers.Count <= 3 &&
            TotalPointCost <= 1000 &&
            TotalPointCost >= 0 &&
            !string.IsNullOrEmpty(factionId) &&
            !string.IsNullOrEmpty(saveId) &&
            !HasDuplicateMancers();

        /// <summary>
        /// Returns a human-readable validation failure reason, or null if the warband is valid.
        /// Used by the warband builder to display specific error messages.
        /// </summary>
        public string GetValidationError()
        {
            if (mancers == null || mancers.Count < 1)
                return "Warband must include at least one Mancer.";
            if (mancers.Count > 3)
                return "Warband cannot include more than three Mancers.";
            if (TotalPointCost > 1000)
                return $"Warband exceeds the 1,000-point limit by {TotalPointCost - 1000} pts.";
            if (string.IsNullOrEmpty(factionId))
                return "A faction must be selected.";
            if (HasDuplicateMancers())
                return "Duplicate Mancer archetypes are not allowed in the same warband.";
            return null;
        }

        /// <summary>Returns true if any two entries in mancers share the same mancerArchetypeId.</summary>
        public bool HasDuplicateMancers()
        {
            if (mancers == null) return false;
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var m in mancers)
            {
                if (string.IsNullOrEmpty(m.mancerArchetypeId)) continue;
                if (!seen.Add(m.mancerArchetypeId)) return true;
            }
            return false;
        }

        // -----------------------------------------------------------------------------------------
        // Factory
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Creates a new, empty warband save with a generated GUID and current timestamps.
        /// The warband is not valid until at least one Mancer is added.
        /// </summary>
        /// <param name="factionId">The faction this warband belongs to.</param>
        /// <param name="displayName">Player-chosen name for the warband.</param>
        /// <returns>A new WarbandSave ready to be populated in the warband builder.</returns>
        public static WarbandSave CreateNew(string factionId, string displayName = "My Warband")
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return new WarbandSave
            {
                saveId = Guid.NewGuid().ToString(),
                displayName = displayName,
                factionId = factionId,
                createdTimestamp = now,
                lastModifiedTimestamp = now,
                schemaVersion = 1,
                mancers = new List<MancerLoadout>(),
                supportUnits = new List<SupportUnitCount>()
            };
        }

        /// <summary>
        /// Creates a deep copy of this warband save with a new GUID.
        /// Used by "Duplicate Warband" in the warband manager screen.
        /// </summary>
        /// <param name="newDisplayName">Display name for the copy. Defaults to "{original name} (Copy)".</param>
        public WarbandSave Duplicate(string newDisplayName = null)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var copy = new WarbandSave
            {
                saveId = Guid.NewGuid().ToString(),
                displayName = newDisplayName ?? $"{displayName} (Copy)",
                factionId = factionId,
                createdTimestamp = now,
                lastModifiedTimestamp = now,
                schemaVersion = schemaVersion,
                mancers = new List<MancerLoadout>(),
                supportUnits = new List<SupportUnitCount>()
            };

            if (mancers != null)
            {
                foreach (var m in mancers)
                {
                    var mCopy = new MancerLoadout { mancerArchetypeId = m.mancerArchetypeId, upgradeIds = new List<UpgradeRef>() };
                    if (m.upgradeIds != null)
                    {
                        foreach (var u in m.upgradeIds)
                            mCopy.upgradeIds.Add(new UpgradeRef { upgradeId = u.upgradeId, additionalCost = u.additionalCost });
                    }
                    copy.mancers.Add(mCopy);
                }
            }

            if (supportUnits != null)
            {
                foreach (var s in supportUnits)
                    copy.supportUnits.Add(new SupportUnitCount { unitId = s.unitId, unitPointCost = s.unitPointCost, count = s.count });
            }

            return copy;
        }

        /// <summary>Updates lastModifiedTimestamp to now. Call whenever the warband is changed.</summary>
        public void MarkModified()
        {
            lastModifiedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    // =============================================================================================
    // Supporting Classes
    // =============================================================================================

    /// <summary>
    /// A single Mancer's configuration within a warband: which archetype and which upgrades are purchased.
    /// No Unity asset references — only string IDs resolved at runtime by WarbandLoader.
    /// </summary>
    [Serializable]
    public class MancerLoadout
    {
        /// <summary>
        /// ID of the Mancer archetype. Must match a MancerData.mancerId.
        /// Examples: "pyromancer", "hydromancer", "cryomancer", "electromancer".
        /// </summary>
        public string mancerArchetypeId;

        /// <summary>
        /// All upgrade options purchased for this Mancer.
        /// Each entry references an UpgradeOption by ID and caches the point cost for fast TotalPointCost calculation.
        /// The order of entries is not meaningful.
        /// </summary>
        public List<UpgradeRef> upgradeIds = new List<UpgradeRef>();

        /// <summary>Total additional cost from all upgrades on this Mancer (sum of all UpgradeRef.additionalCost).</summary>
        public int TotalUpgradeCost
        {
            get
            {
                int total = 0;
                if (upgradeIds != null)
                {
                    foreach (var u in upgradeIds)
                        total += u.additionalCost;
                }
                return total;
            }
        }

        /// <summary>Full warband cost of this Mancer: 100 (base) + all upgrade costs.</summary>
        public int TotalCost => 100 + TotalUpgradeCost;
    }

    /// <summary>
    /// A reference to a purchased upgrade, stored within a MancerLoadout.
    /// The additionalCost is cached at save time so TotalPointCost can be calculated without
    /// loading ScriptableObject assets (e.g., in menus that list saves without fully resolving them).
    /// </summary>
    [Serializable]
    public class UpgradeRef
    {
        /// <summary>
        /// ID of the purchased upgrade. Must match an UpgradeOption.upgradeId on the parent Mancer.
        /// </summary>
        public string upgradeId;

        /// <summary>
        /// Point cost of this upgrade cached at the time of purchase.
        /// Matches UpgradeOption.additionalPointCost at the time this save was written.
        /// Used by TotalPointCost to avoid requiring ScriptableObject lookup.
        /// </summary>
        public int additionalCost;
    }

    /// <summary>
    /// Represents a quantity of one support unit type purchased for the warband.
    /// </summary>
    [Serializable]
    public class SupportUnitCount
    {
        /// <summary>
        /// ID of the support unit type. Must match a SupportUnitData.unitId within the selected faction.
        /// Example: "gilded_throne_chaff_t1", "verdant_pact_ranged_t2", "ashen_covenant_chaff_t1".
        /// </summary>
        public string unitId;

        /// <summary>
        /// Per-unit point cost cached at save time. Used by TotalPointCost without needing asset lookup.
        /// T1 Chaff = 10, T2 Chaff = 20, T1 Ranged = 25, T2 Ranged = 50.
        /// </summary>
        public int unitPointCost;

        /// <summary>
        /// Number of units of this type purchased. Must be at least 1 if this entry exists.
        /// Total cost contribution: unitPointCost * count.
        /// </summary>
        public int count;

        /// <summary>Combined point cost of this unit type entry (unitPointCost * count).</summary>
        public int TotalCost => unitPointCost * count;
    }

    // =============================================================================================
    // Save Index
    // =============================================================================================

    /// <summary>
    /// Lightweight index of all warband saves for a player. Serialized as a separate JSON file
    /// ("warband-index.json") so the save browser can list warbands without loading all save files.
    /// Full WarbandSave data is loaded on demand when a warband is selected.
    /// </summary>
    [Serializable]
    public class WarbandSaveIndex
    {
        /// <summary>All warband entries in this player's save collection.</summary>
        public List<WarbandSaveEntry> entries = new List<WarbandSaveEntry>();

        /// <summary>Returns the entry for a given saveId, or null if not found.</summary>
        public WarbandSaveEntry GetEntry(string saveId)
        {
            if (entries == null) return null;
            foreach (var entry in entries)
            {
                if (string.Equals(entry.saveId, saveId, StringComparison.OrdinalIgnoreCase))
                    return entry;
            }
            return null;
        }
    }

    /// <summary>
    /// Minimal summary data for a single warband save. Stored in WarbandSaveIndex for fast listing
    /// without loading the full WarbandSave from disk.
    /// </summary>
    [Serializable]
    public class WarbandSaveEntry
    {
        /// <summary>Warband GUID — matches WarbandSave.saveId and the save file name.</summary>
        public string saveId;

        /// <summary>Player-chosen warband name for list display.</summary>
        public string displayName;

        /// <summary>Faction ID for the faction icon in the list.</summary>
        public string factionId;

        /// <summary>Total point cost for quick display in the list (e.g., "780 / 1,000 pts").</summary>
        public int totalPointCost;

        /// <summary>Number of Mancers in this warband (1-3) for the list summary.</summary>
        public int mancerCount;

        /// <summary>Unix timestamp of last modification for sorting the list by recently modified.</summary>
        public long lastModifiedTimestamp;

        /// <summary>Whether this warband passes IsValid at the time it was last saved.</summary>
        public bool isValid;
    }
}

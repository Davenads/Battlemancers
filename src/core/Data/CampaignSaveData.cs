using System.Collections.Generic;

namespace Battlemancers.Core.Data
{
    /// <summary>
    /// Persisted state for a single campaign save slot.
    /// Loaded and saved by CampaignRepository. Never mutated at runtime during a match.
    /// Zero Unity dependencies — works headless for tests.
    /// </summary>
    public class CampaignSaveData
    {
        /// <summary>Unique identifier for this save slot. Used as the filename key.</summary>
        public string SaveId { get; set; } = "";

        /// <summary>Display name the player entered for this save file.</summary>
        public string PlayerDisplayName { get; set; } = "Player";

        /// <summary>
        /// Index of the chapter the player is currently in (1-based).
        /// Advances automatically when all scenarios in the current chapter are completed.
        /// </summary>
        public int CurrentChapterIndex { get; set; } = 1;

        /// <summary>IDs of all scenarios the player has completed at least once.</summary>
        public List<string> CompletedScenarioIds { get; set; } = new();

        /// <summary>IDs of all Mancers the player has unlocked through campaign rewards.</summary>
        public List<string> UnlockedMancerIds { get; set; } = new();

        /// <summary>
        /// Saved warband slot IDs referencing WarbandRepository entries.
        /// Campaign allows up to 3 saved warband configurations.
        /// </summary>
        public List<string> WarbandSlots { get; set; } = new();

        /// <summary>ISO 8601 timestamp of the last time this save was written.</summary>
        public string LastPlayedAt { get; set; } = "";

        /// <summary>Total number of campaign matches played on this save.</summary>
        public int TotalMatchesPlayed { get; set; }

        /// <summary>Total number of campaign matches won on this save.</summary>
        public int TotalMatchesWon { get; set; }

        /// <summary>Per-scenario result records keyed by ScenarioId.</summary>
        public List<CampaignScenarioResult> ScenarioResults { get; set; } = new();
    }

    /// <summary>
    /// Immutable record of the outcome of a single scenario attempt.
    /// Stored inside CampaignSaveData.ScenarioResults.
    /// </summary>
    public class CampaignScenarioResult
    {
        /// <summary>ID of the scenario this result belongs to.</summary>
        public string ScenarioId { get; set; } = "";

        /// <summary>ISO 8601 timestamp of when the scenario was completed.</summary>
        public string CompletedAt { get; set; } = "";

        /// <summary>Star rating earned (1–3). 0 = not yet completed.</summary>
        public int StarRating { get; set; }

        /// <summary>
        /// JSON snapshot of the warband used when completing this scenario.
        /// Stored as a raw string so replay / post-match review can reconstruct the list.
        /// </summary>
        public string WarbandUsedSnapshot { get; set; } = "";
    }
}

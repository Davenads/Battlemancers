namespace Battlemancers.Core.Data
{
    /// <summary>
    /// Static definition of a single campaign scenario loaded from JSON.
    /// Matches assets/data/campaign/*.json — field names are PascalCase to align with
    /// System.Text.Json default naming policy (no camelCase conversion).
    /// Never mutated at runtime.
    /// </summary>
    public class ScenarioData
    {
        /// <summary>Unique identifier used in save data and unlock chains (e.g., "ch1_s1_ambush").</summary>
        public string ScenarioId { get; set; } = "";

        /// <summary>Human-readable name shown in the campaign map UI.</summary>
        public string DisplayName { get; set; } = "";

        /// <summary>Chapter this scenario belongs to (1-based).</summary>
        public int ChapterIndex { get; set; }

        /// <summary>Map ID referencing assets/data/maps/*.json.</summary>
        public string MapId { get; set; } = "";

        /// <summary>Faction ID of the enemy force (e.g., "ashen_covenant").</summary>
        public string EnemyFaction { get; set; } = "";

        /// <summary>Warband ID or template key for the enemy force composition.</summary>
        public string EnemyWarbandId { get; set; } = "";

        /// <summary>Primary win condition the player must fulfil to complete the scenario.</summary>
        public WinCondition WinCondition { get; set; }

        /// <summary>
        /// Optional secondary objectives. Completing them awards bonus stars.
        /// Empty array if there are no optional objectives.
        /// </summary>
        public string[] OptionalObjectives { get; set; } = System.Array.Empty<string>();

        /// <summary>
        /// Mancer ID to unlock when the player completes this scenario for the first time.
        /// Empty string if this scenario grants no Mancer unlock.
        /// </summary>
        public string RewardMancerUnlockId { get; set; } = "";

        /// <summary>Narrative text displayed before the battle begins.</summary>
        public string NarrativeIntroText { get; set; } = "";

        /// <summary>Narrative text displayed after the player wins.</summary>
        public string NarrativeOutroText { get; set; } = "";
    }

    /// <summary>
    /// Primary win condition type for a campaign scenario.
    /// </summary>
    public enum WinCondition
    {
        /// <summary>Eliminate all enemy Mancers.</summary>
        Eliminate,

        /// <summary>Survive for a set number of turns without losing all Mancers.</summary>
        Survive,

        /// <summary>Capture and hold a designated control point for a set number of turns.</summary>
        Control
    }
}

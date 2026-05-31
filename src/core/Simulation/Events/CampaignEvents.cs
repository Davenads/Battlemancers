namespace Battlemancers.Core.Simulation.Events
{
    /// <summary>
    /// Fired when a player successfully completes a campaign scenario for the first time,
    /// or improves their star rating on a previously completed scenario.
    /// </summary>
    public sealed class ScenarioCompletedEvent : SimulationEvent
    {
        /// <summary>ID of the scenario that was completed.</summary>
        public string ScenarioId { get; }

        /// <summary>Star rating earned this run (1–3).</summary>
        public int StarRating { get; }

        /// <summary>True if this is the first time the player has completed this scenario.</summary>
        public bool IsFirstCompletion { get; }

        public ScenarioCompletedEvent(int turnNumber, string scenarioId, int starRating, bool isFirstCompletion)
            : base(turnNumber)
        {
            ScenarioId = scenarioId;
            StarRating = starRating;
            IsFirstCompletion = isFirstCompletion;
        }
    }

    /// <summary>
    /// Fired when a campaign reward grants the player access to a new Mancer archetype.
    /// </summary>
    public sealed class MancerUnlockedEvent : SimulationEvent
    {
        /// <summary>ID of the Mancer that was unlocked (e.g., "electromancer").</summary>
        public string MancerId { get; }

        /// <summary>ID of the scenario whose reward triggered this unlock.</summary>
        public string SourceScenarioId { get; }

        public MancerUnlockedEvent(int turnNumber, string mancerId, string sourceScenarioId)
            : base(turnNumber)
        {
            MancerId = mancerId;
            SourceScenarioId = sourceScenarioId;
        }
    }

    /// <summary>
    /// Fired when all scenarios in a chapter are completed and the player advances
    /// to the next chapter.
    /// </summary>
    public sealed class ChapterAdvancedEvent : SimulationEvent
    {
        /// <summary>The chapter the player just completed.</summary>
        public int CompletedChapterIndex { get; }

        /// <summary>The new current chapter index after advancement.</summary>
        public int NewChapterIndex { get; }

        public ChapterAdvancedEvent(int turnNumber, int completedChapterIndex, int newChapterIndex)
            : base(turnNumber)
        {
            CompletedChapterIndex = completedChapterIndex;
            NewChapterIndex = newChapterIndex;
        }
    }
}

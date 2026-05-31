using System;
using System.Collections.Generic;
using System.Linq;
using Battlemancers.Core.Data;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Simulation
{
    /// <summary>
    /// Manages campaign progression: scenario unlock chains, star rating persistence,
    /// Mancer unlock rewards, and chapter advancement.
    ///
    /// Pure C# — zero Unity dependencies. Fires events via SimulationEventBus so the
    /// presentation layer can react without coupling to this manager.
    ///
    /// Scenario unlock rules:
    ///   - The first scenario in each chapter (sorted by list order) is unlocked when
    ///     that chapter is the current chapter or an earlier one.
    ///   - Subsequent scenarios within a chapter unlock sequentially: scenario N unlocks
    ///     when scenario N-1 in the same chapter is completed.
    ///   - Chapter 1 scenario 1 is always unlocked.
    ///
    /// Chapter advance rule:
    ///   - When the last incomplete scenario in CurrentChapterIndex is completed,
    ///     CurrentChapterIndex increments and ChapterAdvanced is fired.
    /// </summary>
    public class CampaignProgressManager
    {
        // TurnNumber sentinel for campaign events (outside a live match context).
        private const int CampaignEventTurnNumber = 0;

        private readonly CampaignSaveData _save;
        private readonly List<ScenarioData> _scenarios;
        private readonly Action<string> _logger;

        /// <summary>
        /// Creates a new manager bound to the given save data and scenario catalogue.
        /// </summary>
        /// <param name="saveData">Mutable save data that will be updated by progression calls.</param>
        /// <param name="scenarios">
        ///   Full ordered list of scenario definitions. Within each chapter, the order of entries
        ///   determines the sequential unlock chain.
        /// </param>
        /// <param name="logger">Optional log sink; defaults to Console.WriteLine.</param>
        public CampaignProgressManager(
            CampaignSaveData saveData,
            List<ScenarioData> scenarios,
            Action<string> logger = null)
        {
            _save = saveData ?? throw new ArgumentNullException(nameof(saveData));
            _scenarios = scenarios ?? throw new ArgumentNullException(nameof(scenarios));
            _logger = logger ?? Console.WriteLine;
        }

        // ---------------------------------------------------------------------------
        // Queries
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns true when the player may attempt <paramref name="scenarioId"/>.
        /// A scenario is unlocked when:
        ///   - Its chapter index is less than or equal to CurrentChapterIndex, AND
        ///   - All scenarios in the same chapter that appear before it in the scenario
        ///     list are already completed.
        /// </summary>
        public bool IsScenarioUnlocked(string scenarioId)
        {
            ScenarioData target = FindScenario(scenarioId);
            if (target == null) return false;

            // Cannot access scenarios from future chapters.
            if (target.ChapterIndex > _save.CurrentChapterIndex)
                return false;

            // Collect all scenarios in the same chapter, preserving list order.
            List<ScenarioData> chapterScenarios = ScenariosForChapter(target.ChapterIndex);
            int positionInChapter = chapterScenarios.IndexOf(target);

            // The first scenario in the chapter is always unlocked (for current/past chapters).
            if (positionInChapter == 0)
                return true;

            // All preceding scenarios in the chapter must be completed.
            for (int i = 0; i < positionInChapter; i++)
            {
                if (!_save.CompletedScenarioIds.Contains(chapterScenarios[i].ScenarioId))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the IDs of all Mancers the player has unlocked on this save.
        /// </summary>
        public IReadOnlyList<string> GetUnlockedMancers() =>
            _save.UnlockedMancerIds.AsReadOnly();

        /// <summary>Returns the player's current chapter index (1-based).</summary>
        public int GetCurrentChapter() => _save.CurrentChapterIndex;

        // ---------------------------------------------------------------------------
        // Mutations
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Records completion of <paramref name="scenarioId"/> with the given star rating.
        ///
        /// Effects (in order):
        ///   1. Adds scenarioId to CompletedScenarioIds if not already present.
        ///   2. Upserts the ScenarioResult (keeps best star rating).
        ///   3. Fires <see cref="ScenarioCompletedEvent"/>.
        ///   4. If the scenario grants a Mancer unlock and it isn't already owned,
        ///      adds it and fires <see cref="MancerUnlockedEvent"/>.
        ///   5. If all scenarios in CurrentChapterIndex are now complete, advances the
        ///      chapter and fires <see cref="ChapterAdvancedEvent"/>.
        /// </summary>
        /// <param name="scenarioId">The scenario being completed.</param>
        /// <param name="starRating">Stars earned this run (1–3).</param>
        /// <exception cref="ArgumentException">Thrown when scenarioId is unknown.</exception>
        public void CompleteScenario(string scenarioId, int starRating)
        {
            ScenarioData scenario = FindScenario(scenarioId)
                ?? throw new ArgumentException($"Unknown scenario '{scenarioId}'.", nameof(scenarioId));

            bool isFirstCompletion = !_save.CompletedScenarioIds.Contains(scenarioId);

            // 1. Mark completed.
            if (isFirstCompletion)
                _save.CompletedScenarioIds.Add(scenarioId);

            // 2. Upsert result — keep the best star rating.
            UpsertResult(scenarioId, starRating);

            // 3. Fire ScenarioCompleted.
            SimulationEventBus.Publish(new ScenarioCompletedEvent(
                CampaignEventTurnNumber, scenarioId, starRating, isFirstCompletion));

            _logger($"[CampaignProgressManager] Scenario '{scenarioId}' completed ({starRating}★, firstCompletion={isFirstCompletion}).");

            // 4. Mancer unlock reward (first completion only).
            if (isFirstCompletion && !string.IsNullOrEmpty(scenario.RewardMancerUnlockId))
                TryUnlockMancer(scenario.RewardMancerUnlockId, scenarioId);

            // 5. Chapter advance check.
            TryAdvanceChapter();
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        private void UpsertResult(string scenarioId, int starRating)
        {
            CampaignScenarioResult existing = _save.ScenarioResults
                .FirstOrDefault(r => r.ScenarioId == scenarioId);

            if (existing == null)
            {
                _save.ScenarioResults.Add(new CampaignScenarioResult
                {
                    ScenarioId = scenarioId,
                    CompletedAt = DateTime.UtcNow.ToString("O"),
                    StarRating = starRating,
                    WarbandUsedSnapshot = ""
                });
            }
            else if (starRating > existing.StarRating)
            {
                existing.StarRating = starRating;
                existing.CompletedAt = DateTime.UtcNow.ToString("O");
            }
        }

        private void TryUnlockMancer(string mancerId, string sourceScenarioId)
        {
            if (_save.UnlockedMancerIds.Contains(mancerId))
                return;

            _save.UnlockedMancerIds.Add(mancerId);
            SimulationEventBus.Publish(new MancerUnlockedEvent(
                CampaignEventTurnNumber, mancerId, sourceScenarioId));

            _logger($"[CampaignProgressManager] Mancer '{mancerId}' unlocked via '{sourceScenarioId}'.");
        }

        private void TryAdvanceChapter()
        {
            List<ScenarioData> currentChapterScenarios = ScenariosForChapter(_save.CurrentChapterIndex);
            if (currentChapterScenarios.Count == 0)
                return;

            bool allComplete = currentChapterScenarios
                .All(s => _save.CompletedScenarioIds.Contains(s.ScenarioId));

            if (!allComplete)
                return;

            int completedChapter = _save.CurrentChapterIndex;
            _save.CurrentChapterIndex++;

            SimulationEventBus.Publish(new ChapterAdvancedEvent(
                CampaignEventTurnNumber, completedChapter, _save.CurrentChapterIndex));

            _logger($"[CampaignProgressManager] Chapter {completedChapter} complete — advanced to chapter {_save.CurrentChapterIndex}.");
        }

        private ScenarioData FindScenario(string scenarioId) =>
            _scenarios.FirstOrDefault(s => s.ScenarioId == scenarioId);

        private List<ScenarioData> ScenariosForChapter(int chapterIndex) =>
            _scenarios.Where(s => s.ChapterIndex == chapterIndex).ToList();
    }
}

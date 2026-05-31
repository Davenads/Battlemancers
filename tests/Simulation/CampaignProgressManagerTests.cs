using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Battlemancers.Core.Data;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for CampaignProgressManager and CampaignRepository.
    /// Covers: unlock chain, locked scenario, star rating persistence, Mancer unlock
    /// event, chapter advance threshold, and save round-trip.
    /// </summary>
    [TestFixture]
    public class CampaignProgressManagerTests
    {
        // =========================================================================
        // Per-test state
        // =========================================================================

        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(
                Path.GetTempPath(),
                $"CampaignProgressManagerTests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDir);

            // Clear event bus between tests to prevent handler pollution.
            SimulationEventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SimulationEventBus.Clear();
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        // =========================================================================
        // Helpers
        // =========================================================================

        /// <summary>Returns a standard Chapter 1 scenario catalogue in order.</summary>
        private static List<ScenarioData> MakeChapter1Scenarios() => new List<ScenarioData>
        {
            new ScenarioData
            {
                ScenarioId     = "ch1_s1_ambush",
                DisplayName    = "The Ambush at Ashford Crossing",
                ChapterIndex   = 1,
                MapId          = "crossroads",
                EnemyFaction   = "ashen_covenant",
                EnemyWarbandId = "ashen_patrol_t1",
                WinCondition   = WinCondition.Eliminate,
                OptionalObjectives = Array.Empty<string>(),
                RewardMancerUnlockId = "electromancer"
            },
            new ScenarioData
            {
                ScenarioId     = "ch1_s2_holdout",
                DisplayName    = "Holdout at Ember Ridge",
                ChapterIndex   = 1,
                MapId          = "ember_ridge",
                EnemyFaction   = "ashen_covenant",
                EnemyWarbandId = "ashen_assault_t1",
                WinCondition   = WinCondition.Survive,
                OptionalObjectives = Array.Empty<string>(),
                RewardMancerUnlockId = "geomancer"
            },
            new ScenarioData
            {
                ScenarioId     = "ch1_s3_siege",
                DisplayName    = "The Siege of Frostgate",
                ChapterIndex   = 1,
                MapId          = "frozen_wastes",
                EnemyFaction   = "ashen_covenant",
                EnemyWarbandId = "ashen_siege_t2",
                WinCondition   = WinCondition.Control,
                OptionalObjectives = Array.Empty<string>(),
                RewardMancerUnlockId = "cryomancer"
            }
        };

        private static CampaignSaveData MakeFreshSave(string displayName = "TestPlayer") =>
            new CampaignSaveData
            {
                PlayerDisplayName = displayName,
                CurrentChapterIndex = 1
            };

        private CampaignProgressManager MakeManager(
            CampaignSaveData save = null,
            List<ScenarioData> scenarios = null) =>
            new CampaignProgressManager(
                save ?? MakeFreshSave(),
                scenarios ?? MakeChapter1Scenarios(),
                logger: _ => { }); // suppress console output in tests

        private CampaignRepository MakeRepo() =>
            new CampaignRepository(_tempDir, logger: _ => { });

        // =========================================================================
        // Test 1 — Unlock chain: first scenario is always unlocked
        // =========================================================================

        [Test]
        public void IsScenarioUnlocked_FirstScenarioInChapter_ReturnsTrue()
        {
            // Arrange
            var manager = MakeManager();

            // Act + Assert — ch1_s1 is the first scenario in chapter 1 and must always
            // be unlocked when CurrentChapterIndex == 1, even with no completions.
            Assert.That(manager.IsScenarioUnlocked("ch1_s1_ambush"), Is.True,
                "The first scenario in chapter 1 must be unlocked on a fresh save.");
        }

        // =========================================================================
        // Test 2 — Locked scenario: second scenario locked until first is complete
        // =========================================================================

        [Test]
        public void IsScenarioUnlocked_SequentialScenario_LockedUntilPreviousComplete()
        {
            // Arrange — fresh save, ch1_s1 not yet completed.
            var manager = MakeManager();

            // Act
            bool s2UnlockedBefore = manager.IsScenarioUnlocked("ch1_s2_holdout");

            // Complete the prerequisite.
            manager.CompleteScenario("ch1_s1_ambush", starRating: 2);
            bool s2UnlockedAfter = manager.IsScenarioUnlocked("ch1_s2_holdout");

            // Assert
            Assert.That(s2UnlockedBefore, Is.False,
                "ch1_s2 must be locked when its prerequisite ch1_s1 is not yet complete.");
            Assert.That(s2UnlockedAfter, Is.True,
                "ch1_s2 must unlock once ch1_s1 is completed.");
        }

        // =========================================================================
        // Test 3 — Star rating persistence: best rating is kept on re-completion
        // =========================================================================

        [Test]
        public void CompleteScenario_RecompletionWithHigherRating_PersistsBestStarRating()
        {
            // Arrange
            var save = MakeFreshSave();
            var manager = MakeManager(save: save);

            // Act — first completion with 1 star.
            manager.CompleteScenario("ch1_s1_ambush", starRating: 1);
            int ratingAfterFirst = save.ScenarioResults
                .Find(r => r.ScenarioId == "ch1_s1_ambush")?.StarRating ?? 0;

            // Re-complete with 3 stars.
            manager.CompleteScenario("ch1_s1_ambush", starRating: 3);
            int ratingAfterSecond = save.ScenarioResults
                .Find(r => r.ScenarioId == "ch1_s1_ambush")?.StarRating ?? 0;

            // Re-complete with 2 stars — should NOT downgrade from 3.
            manager.CompleteScenario("ch1_s1_ambush", starRating: 2);
            int ratingAfterThird = save.ScenarioResults
                .Find(r => r.ScenarioId == "ch1_s1_ambush")?.StarRating ?? 0;

            // Assert
            Assert.That(ratingAfterFirst, Is.EqualTo(1), "First completion should record 1 star.");
            Assert.That(ratingAfterSecond, Is.EqualTo(3), "Higher re-completion should upgrade to 3 stars.");
            Assert.That(ratingAfterThird, Is.EqualTo(3), "Lower re-completion must not downgrade the star rating.");
        }

        // =========================================================================
        // Test 4 — Mancer unlock: completing a scenario fires MancerUnlockedEvent
        // =========================================================================

        [Test]
        public void CompleteScenario_ScenarioWithReward_FiresMancerUnlockedEvent()
        {
            // Arrange
            var save = MakeFreshSave();
            var manager = MakeManager(save: save);

            MancerUnlockedEvent capturedEvent = null;
            SimulationEventBus.Subscribe<MancerUnlockedEvent>(e => capturedEvent = e);

            // Act
            manager.CompleteScenario("ch1_s1_ambush", starRating: 2);

            // Assert — event was fired with correct data.
            Assert.That(capturedEvent, Is.Not.Null,
                "MancerUnlockedEvent must be published when a scenario with a reward is completed.");
            Assert.That(capturedEvent.MancerId, Is.EqualTo("electromancer"),
                "MancerUnlockedEvent.MancerId must match the scenario's RewardMancerUnlockId.");
            Assert.That(capturedEvent.SourceScenarioId, Is.EqualTo("ch1_s1_ambush"),
                "MancerUnlockedEvent.SourceScenarioId must match the completed scenario.");

            // Also verify the mancer is in the save's unlocked list.
            Assert.That(save.UnlockedMancerIds, Does.Contain("electromancer"),
                "The unlocked Mancer ID must be added to CampaignSaveData.UnlockedMancerIds.");

            // Completing again must NOT fire a second unlock event (already owned).
            capturedEvent = null;
            manager.CompleteScenario("ch1_s1_ambush", starRating: 3);
            Assert.That(capturedEvent, Is.Null,
                "MancerUnlockedEvent must not fire again if the Mancer is already unlocked.");
        }

        // =========================================================================
        // Test 5 — Chapter advance: completing all chapter scenarios advances chapter
        // =========================================================================

        [Test]
        public void CompleteScenario_AllChapterScenariosComplete_AdvancesChapterIndex()
        {
            // Arrange
            var save = MakeFreshSave();
            var manager = MakeManager(save: save);

            ChapterAdvancedEvent capturedEvent = null;
            SimulationEventBus.Subscribe<ChapterAdvancedEvent>(e => capturedEvent = e);

            // Act — complete all three Chapter 1 scenarios.
            manager.CompleteScenario("ch1_s1_ambush", starRating: 2);
            Assert.That(save.CurrentChapterIndex, Is.EqualTo(1),
                "Chapter must not advance after only the first scenario.");
            Assert.That(capturedEvent, Is.Null,
                "ChapterAdvancedEvent must not fire until all scenarios in the chapter are done.");

            manager.CompleteScenario("ch1_s2_holdout", starRating: 2);
            Assert.That(save.CurrentChapterIndex, Is.EqualTo(1),
                "Chapter must not advance after only two of three scenarios.");

            manager.CompleteScenario("ch1_s3_siege", starRating: 1);

            // Assert — chapter should have advanced to 2 now.
            Assert.That(save.CurrentChapterIndex, Is.EqualTo(2),
                "CurrentChapterIndex must increment to 2 when all Chapter 1 scenarios are complete.");
            Assert.That(capturedEvent, Is.Not.Null,
                "ChapterAdvancedEvent must be published when the chapter advances.");
            Assert.That(capturedEvent.CompletedChapterIndex, Is.EqualTo(1),
                "ChapterAdvancedEvent.CompletedChapterIndex must be 1.");
            Assert.That(capturedEvent.NewChapterIndex, Is.EqualTo(2),
                "ChapterAdvancedEvent.NewChapterIndex must be 2.");
        }

        // =========================================================================
        // Test 6 — Save round-trip: CampaignRepository persists and reloads correctly
        // =========================================================================

        [Test]
        public void CampaignRepository_SaveAndLoad_RoundTripPreservesAllFields()
        {
            // Arrange
            var repo = MakeRepo();
            var save = MakeFreshSave("HeroPlayer");
            save.CurrentChapterIndex = 2;
            save.CompletedScenarioIds = new List<string> { "ch1_s1_ambush", "ch1_s2_holdout" };
            save.UnlockedMancerIds = new List<string> { "electromancer", "geomancer" };
            save.TotalMatchesPlayed = 5;
            save.TotalMatchesWon = 3;

            // Act — persist then load from a fresh repository instance (verifies disk I/O).
            repo.Save(save);
            string savedId = save.SaveId;

            Assert.That(savedId, Is.Not.Null.And.Not.Empty,
                "Save must assign a non-empty SaveId.");

            var freshRepo = MakeRepo();
            CampaignSaveData loaded = freshRepo.Load(savedId);

            // Assert — all fields round-trip correctly.
            Assert.That(loaded, Is.Not.Null, "Load must return a non-null result for a valid SaveId.");
            Assert.That(loaded.SaveId, Is.EqualTo(savedId), "SaveId must survive round-trip.");
            Assert.That(loaded.PlayerDisplayName, Is.EqualTo("HeroPlayer"), "PlayerDisplayName must survive.");
            Assert.That(loaded.CurrentChapterIndex, Is.EqualTo(2), "CurrentChapterIndex must survive.");
            Assert.That(loaded.CompletedScenarioIds, Is.EquivalentTo(new[] { "ch1_s1_ambush", "ch1_s2_holdout" }),
                "CompletedScenarioIds must survive.");
            Assert.That(loaded.UnlockedMancerIds, Is.EquivalentTo(new[] { "electromancer", "geomancer" }),
                "UnlockedMancerIds must survive.");
            Assert.That(loaded.TotalMatchesPlayed, Is.EqualTo(5), "TotalMatchesPlayed must survive.");
            Assert.That(loaded.TotalMatchesWon, Is.EqualTo(3), "TotalMatchesWon must survive.");

            // Verify LoadManifest returns the slot.
            List<CampaignSaveSlot> manifest = freshRepo.LoadManifest();
            Assert.That(manifest.Count, Is.EqualTo(1), "Manifest must list exactly one save slot.");
            Assert.That(manifest[0].SaveId, Is.EqualTo(savedId), "Manifest SaveId must match.");
            Assert.That(manifest[0].PlayerDisplayName, Is.EqualTo("HeroPlayer"),
                "Manifest PlayerDisplayName must match.");
        }
    }
}

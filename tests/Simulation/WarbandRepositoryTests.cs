using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Battlemancers.Core.Data;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for WarbandRepository — covers save/load round-trips, deep-copy duplication,
    /// the MaxWarbands=20 capacity cap, null return on missing IDs, and LastModified
    /// timestamp updates on re-save.
    /// </summary>
    [TestFixture]
    public class WarbandRepositoryTests
    {
        // =========================================================================
        // Per-test temp directory management
        // =========================================================================

        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"WarbandRepositoryTests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }

        // =========================================================================
        // Helpers
        // =========================================================================

        private WarbandRepository MakeRepo() =>
            new WarbandRepository(_tempDir, logger: _ => { }); // suppress console output in tests

        private static WarbandData MakeWarband(string name, string factionId = "gilded_throne") =>
            new WarbandData
            {
                Name     = name,
                FactionId = factionId,
                TotalPoints = 100,
                IsValid = true,
                Mancers = new List<WarbandMancerEntry>
                {
                    new WarbandMancerEntry { MancerId = "pyromancer", TotalCost = 100 }
                }
            };

        // =========================================================================
        // Test 1: Save + LoadAll manifest round-trip
        // =========================================================================

        [Test]
        public void Save_ThenLoadAll_ReturnsPersistedWarband()
        {
            // Arrange
            var repo = MakeRepo();
            repo.LoadAll(); // initialise internal list

            var warband = MakeWarband("Iron Brigade");
            repo.Save(warband);

            // Act — create a fresh repository instance pointing at the same directory to
            // verify the data was actually persisted to disk (not just held in memory).
            var freshRepo = MakeRepo();
            List<WarbandData> loaded = freshRepo.LoadAll();

            // Assert
            Assert.That(loaded, Is.Not.Null, "LoadAll must not return null.");
            Assert.That(loaded.Count, Is.EqualTo(1), "Exactly one warband should be on disk after one Save.");
            Assert.That(loaded[0].Name, Is.EqualTo("Iron Brigade"),
                "The persisted warband name must match what was saved.");
            Assert.That(loaded[0].WarbandId, Is.Not.Null.And.Not.Empty,
                "A WarbandId must have been assigned during Save.");
        }

        // =========================================================================
        // Test 2: Duplicate produces a deep copy
        // =========================================================================

        [Test]
        public void Duplicate_ExistingWarband_ProducesDeepCopy()
        {
            // Arrange
            var repo = MakeRepo();
            repo.LoadAll();

            var original = MakeWarband("Thunder Regiment");
            repo.Save(original);
            string originalId = original.WarbandId;

            // Act
            WarbandData copy = repo.Duplicate(originalId);

            // Assert — copy has a different ID
            Assert.That(copy.WarbandId, Is.Not.EqualTo(originalId),
                "Duplicate must assign a new WarbandId.");

            // Assert — copy name has the '(Copy)' suffix
            Assert.That(copy.Name, Is.EqualTo("Thunder Regiment (Copy)"),
                "Duplicate must append ' (Copy)' to the warband name.");

            // Assert — deep copy: mutating the copy's Mancers list must not affect the original.
            copy.Mancers.Add(new WarbandMancerEntry { MancerId = "hydromancer", TotalCost = 100 });
            WarbandData reloadedOriginal = repo.GetById(originalId);
            Assert.That(reloadedOriginal.Mancers.Count, Is.EqualTo(1),
                "Adding a Mancer to the copy must not affect the original — they must not share references.");

            // Assert — both entries exist in the repository
            Assert.That(repo.GetAll().Count, Is.EqualTo(2),
                "After Duplicate, the repository must contain both the original and the copy.");
        }

        // =========================================================================
        // Test 3: MaxWarbands = 20 cap is enforced
        // =========================================================================

        [Test]
        public void Save_BeyondMaxWarbandsCap_ThrowsInvalidOperationException()
        {
            // Arrange — fill the repository to the maximum allowed capacity.
            var repo = MakeRepo();
            repo.LoadAll();

            for (int i = 0; i < WarbandRepository.MaxWarbands; i++)
                repo.Save(MakeWarband($"Warband {i + 1}"));

            Assert.That(repo.GetAll().Count, Is.EqualTo(WarbandRepository.MaxWarbands),
                "Pre-condition: repository must be at max capacity before the overflow save.");

            // Act + Assert — saving one more new warband must throw.
            var overflow = MakeWarband("One Too Many");
            Assert.Throws<InvalidOperationException>(() => repo.Save(overflow),
                $"Saving a {WarbandRepository.MaxWarbands + 1}th warband must throw InvalidOperationException.");
        }

        // =========================================================================
        // Test 4: Load (GetById) returns null on missing ID
        // =========================================================================

        [Test]
        public void GetById_MissingId_ReturnsNull()
        {
            // Arrange
            var repo = MakeRepo();
            repo.LoadAll(); // empty list

            // Act
            WarbandData result = repo.GetById("non-existent-guid-00000000");

            // Assert
            Assert.That(result, Is.Null,
                "GetById must return null when no warband with the given ID exists.");
        }

        // =========================================================================
        // Test 5: LastModified updates on re-save
        // =========================================================================

        [Test]
        public void Save_ResavingExistingWarband_UpdatesLastModified()
        {
            // Arrange
            var repo = MakeRepo();
            repo.LoadAll();

            var warband = MakeWarband("Storm Watch");
            repo.Save(warband);
            DateTime firstModified = warband.LastModified;

            // Ensure at least 1 ms passes so the timestamps are guaranteed to differ.
            Thread.Sleep(millisecondsTimeout: 10);

            // Act — re-save with a name change.
            warband.Name = "Storm Watch Elite";
            repo.Save(warband);
            DateTime secondModified = warband.LastModified;

            // Assert
            Assert.That(secondModified, Is.GreaterThan(firstModified),
                "LastModified must be updated to a later timestamp on every re-save.");

            // Also verify the name change was persisted.
            var freshRepo = MakeRepo();
            freshRepo.LoadAll();
            WarbandData reloaded = freshRepo.GetById(warband.WarbandId);
            Assert.That(reloaded?.Name, Is.EqualTo("Storm Watch Elite"),
                "The updated name must be persisted to disk.");
        }
    }
}

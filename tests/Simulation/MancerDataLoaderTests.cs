using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Battlemancers.Core.Data;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for MancerDataLoader — verifies JSON loading from directory,
    /// graceful handling of empty/missing directories, malformed file skipping,
    /// and last-write-wins behaviour on duplicate MancerIds.
    /// </summary>
    [TestFixture]
    public class MancerDataLoaderTests
    {
        // =========================================================================
        // Per-test temp directory management
        // =========================================================================

        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            // Each test gets its own isolated temp directory so fixture files never bleed across tests.
            _tempDir = Path.Combine(Path.GetTempPath(), $"MancerDataLoaderTests_{Guid.NewGuid():N}");
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

        /// <summary>Writes a minimal valid Mancer JSON file into the temp directory.</summary>
        private void WriteValidJson(string fileName, string mancerId, string displayName = "Test Mancer")
        {
            string json = $@"{{
  ""MancerId"": ""{mancerId}"",
  ""DisplayName"": ""{displayName}"",
  ""MaxHP"": 80,
  ""MoveRange"": 3,
  ""BaseCost"": 100,
  ""Spells"": []
}}";
            File.WriteAllText(Path.Combine(_tempDir, fileName), json);
        }

        // =========================================================================
        // LoadAll — valid directory with multiple mancer files
        // =========================================================================

        [Test]
        public void LoadAll_ValidDirectory_ReturnsAllMancers()
        {
            // Arrange — write 3 valid mancer JSON files to the temp directory.
            WriteValidJson("pyromancer.json",  "pyromancer",  "Pyromancer");
            WriteValidJson("hydromancer.json", "hydromancer", "Hydromancer");
            WriteValidJson("cryomancer.json",  "cryomancer",  "Cryomancer");

            var loader = new MancerDataLoader(_tempDir);

            // Act
            Dictionary<string, MancerRuntimeData> result = loader.LoadAll();

            // Assert
            Assert.That(result, Is.Not.Null, "LoadAll must never return null.");
            Assert.That(result.Count, Is.EqualTo(3), "Expected 3 Mancer definitions loaded.");
            Assert.That(result.ContainsKey("pyromancer"),  Is.True, "pyromancer must be present.");
            Assert.That(result.ContainsKey("hydromancer"), Is.True, "hydromancer must be present.");
            Assert.That(result.ContainsKey("cryomancer"),  Is.True, "cryomancer must be present.");
        }

        // =========================================================================
        // LoadAll — empty directory
        // =========================================================================

        [Test]
        public void LoadAll_EmptyDirectory_ReturnsEmptyDictionary()
        {
            // Arrange — _tempDir exists but contains no .json files.
            var loader = new MancerDataLoader(_tempDir);

            // Act
            Dictionary<string, MancerRuntimeData> result = loader.LoadAll();

            // Assert
            Assert.That(result, Is.Not.Null, "LoadAll must return an empty dictionary, not null.");
            Assert.That(result.Count, Is.EqualTo(0), "Empty directory must yield 0 entries.");
        }

        // =========================================================================
        // LoadAll — missing directory
        // =========================================================================

        [Test]
        public void LoadAll_MissingDirectory_ReturnsEmptyDictionaryWithoutCrashing()
        {
            // Arrange — point loader at a path that does not exist.
            string missingDir = Path.Combine(_tempDir, "does_not_exist");
            Assert.That(Directory.Exists(missingDir), Is.False,
                "Pre-condition: the directory must not exist before the test.");

            var loader = new MancerDataLoader(missingDir);

            // Act + Assert (no exception thrown)
            Dictionary<string, MancerRuntimeData> result = null;
            Assert.DoesNotThrow(() => result = loader.LoadAll(),
                "LoadAll must not throw when the directory does not exist.");

            Assert.That(result, Is.Not.Null,
                "LoadAll must return an empty dictionary for a missing directory.");
            Assert.That(result.Count, Is.EqualTo(0),
                "A missing directory must yield 0 entries.");
        }

        // =========================================================================
        // LoadAll — malformed JSON is skipped; valid files still load
        // =========================================================================

        [Test]
        public void LoadAll_MalformedJson_SkipsFileAndContinues()
        {
            // Arrange — one valid file and one broken file in the same directory.
            WriteValidJson("pyromancer.json", "pyromancer", "Pyromancer");
            File.WriteAllText(Path.Combine(_tempDir, "broken.json"), "{ mancerId: }"); // invalid JSON

            var loader = new MancerDataLoader(_tempDir);

            // Act + Assert (no exception thrown)
            Dictionary<string, MancerRuntimeData> result = null;
            Assert.DoesNotThrow(() => result = loader.LoadAll(),
                "LoadAll must not throw on malformed JSON files — it should skip and continue.");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1),
                "Only the valid file should be loaded; the malformed file must be skipped.");
            Assert.That(result.ContainsKey("pyromancer"), Is.True,
                "The valid pyromancer entry must be present despite the broken file.");
        }

        // =========================================================================
        // LoadAll — duplicate MancerId (last write wins)
        // =========================================================================

        [Test]
        public void LoadAll_DuplicateMancerId_LastWriteWins()
        {
            // Arrange — two files that both declare MancerId = "pyromancer".
            // The loader uses result[data.MancerId] = data so the second file parsed
            // overwrites the first; the dictionary ends up with exactly one entry.
            WriteValidJson("pyromancer_a.json", "pyromancer", "Pyromancer Alpha");
            WriteValidJson("pyromancer_b.json", "pyromancer", "Pyromancer Beta");

            var loader = new MancerDataLoader(_tempDir);

            // Act
            Dictionary<string, MancerRuntimeData> result = loader.LoadAll();

            // Assert — duplicate keys collapse to one entry (last write wins).
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1),
                "Two files with the same MancerId should result in exactly one dictionary entry.");
            Assert.That(result.ContainsKey("pyromancer"), Is.True,
                "The surviving entry must still be keyed under 'pyromancer'.");
        }
    }
}

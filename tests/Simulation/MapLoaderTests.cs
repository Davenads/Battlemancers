using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Battlemancers.Core.Data;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for <see cref="MapLoader"/> — verifies JSON loading from a directory,
    /// graceful handling of missing or empty directories, malformed file skipping,
    /// and successful loading of all 3 preset map files.
    /// </summary>
    [TestFixture]
    public class MapLoaderTests
    {
        // =========================================================================
        // Per-test temp directory management
        // =========================================================================

        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), $"MapLoaderTests_{Guid.NewGuid():N}");
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

        /// <summary>Writes a minimal valid map JSON file into the temp directory.</summary>
        private void WriteValidMapJson(string fileName, string mapId, string displayName = "Test Map")
        {
            string json = $@"{{
  ""MapId"": ""{mapId}"",
  ""DisplayName"": ""{displayName}"",
  ""Width"": 4,
  ""Height"": 4,
  ""BiomeTag"": ""ruins"",
  ""SpawnZones"": [
    {{
      ""PlayerId"": ""player1"",
      ""Tiles"": [{{""X"": 0, ""Y"": 0}}, {{""X"": 1, ""Y"": 0}}, {{""X"": 0, ""Y"": 1}}, {{""X"": 1, ""Y"": 1}}]
    }},
    {{
      ""PlayerId"": ""player2"",
      ""Tiles"": [{{""X"": 2, ""Y"": 2}}, {{""X"": 3, ""Y"": 2}}, {{""X"": 2, ""Y"": 3}}, {{""X"": 3, ""Y"": 3}}]
    }}
  ],
  ""Tiles"": []
}}";
            File.WriteAllText(Path.Combine(_tempDir, fileName), json);
        }

        // =========================================================================
        // LoadAll — valid file returns populated MapData entry
        // =========================================================================

        [Test]
        public void LoadAll_ValidFile_ReturnsPopulatedMapData()
        {
            // Arrange
            WriteValidMapJson("crossroads.json", "crossroads", "The Crossroads");
            var loader = new MapLoader(_tempDir);

            // Act
            Dictionary<string, MapData> result = loader.LoadAll();

            // Assert
            Assert.That(result, Is.Not.Null, "LoadAll must never return null.");
            Assert.That(result.Count, Is.EqualTo(1), "Expected exactly 1 map loaded.");
            Assert.That(result.ContainsKey("crossroads"), Is.True, "'crossroads' key must be present.");

            MapData map = result["crossroads"];
            Assert.That(map.MapId,      Is.EqualTo("crossroads"),    "MapId must match file content.");
            Assert.That(map.DisplayName, Is.EqualTo("The Crossroads"), "DisplayName must match file content.");
            Assert.That(map.Width,      Is.EqualTo(4),               "Width must match file content.");
            Assert.That(map.Height,     Is.EqualTo(4),               "Height must match file content.");
            Assert.That(map.BiomeTag,   Is.EqualTo("ruins"),         "BiomeTag must match file content.");
            Assert.That(map.SpawnZones, Is.Not.Null,                 "SpawnZones must not be null.");
            Assert.That(map.SpawnZones.Count, Is.EqualTo(2),         "Two spawn zones must be loaded.");
        }

        // =========================================================================
        // LoadAll — malformed JSON is skipped; valid files still load
        // =========================================================================

        [Test]
        public void LoadAll_MalformedJson_SkipsFileAndContinues()
        {
            // Arrange — one valid file and one broken file in the same directory.
            WriteValidMapJson("crossroads.json", "crossroads", "The Crossroads");
            File.WriteAllText(Path.Combine(_tempDir, "broken.json"), "{ mapId: }"); // invalid JSON

            var loader = new MapLoader(_tempDir);

            // Act + Assert (no exception thrown)
            Dictionary<string, MapData> result = null;
            Assert.DoesNotThrow(
                () => result = loader.LoadAll(),
                "LoadAll must not throw on malformed JSON files — it should skip and continue.");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1),
                "Only the valid file should be loaded; the malformed file must be skipped.");
            Assert.That(result.ContainsKey("crossroads"), Is.True,
                "The valid crossroads entry must be present despite the broken file.");
        }

        // =========================================================================
        // LoadAll — missing directory returns empty dictionary without throwing
        // =========================================================================

        [Test]
        public void LoadAll_MissingDirectory_ReturnsEmptyDictionaryWithoutCrashing()
        {
            // Arrange — point loader at a path that does not exist.
            string missingDir = Path.Combine(_tempDir, "does_not_exist");
            Assert.That(Directory.Exists(missingDir), Is.False,
                "Pre-condition: the directory must not exist before the test.");

            var loader = new MapLoader(missingDir);

            // Act + Assert (no exception thrown)
            Dictionary<string, MapData> result = null;
            Assert.DoesNotThrow(
                () => result = loader.LoadAll(),
                "LoadAll must not throw when the directory does not exist.");

            Assert.That(result, Is.Not.Null,
                "LoadAll must return an empty dictionary for a missing directory.");
            Assert.That(result.Count, Is.EqualTo(0),
                "A missing directory must yield 0 entries.");
        }

        // =========================================================================
        // LoadAll — all 3 preset maps load without exception
        // =========================================================================

        [Test]
        public void LoadAll_AllPresetMaps_LoadWithoutException()
        {
            // Arrange — resolve path to the committed preset maps.
            // Walks up from the test assembly's location to find assets/data/maps/.
            string assemblyDir = Path.GetDirectoryName(typeof(MapLoaderTests).Assembly.Location)
                                 ?? Directory.GetCurrentDirectory();

            // Navigate from build output up to the project root and then into assets/data/maps.
            // Typical layout: <root>/tests/bin/Debug/netX/ → walk up 4 levels.
            string projectRoot = assemblyDir;
            for (int i = 0; i < 6; i++)
            {
                string candidate = Path.Combine(projectRoot, "assets", "data", "maps");
                if (Directory.Exists(candidate))
                {
                    projectRoot = candidate;
                    break;
                }
                string parent = Directory.GetParent(projectRoot)?.FullName;
                if (parent == null) break;
                projectRoot = parent;
            }

            // If the asset directory was not found by walking up, fall back to searching
            // by the well-known CLAUDE.md anchor file.
            if (!Directory.Exists(projectRoot) || !File.Exists(Path.Combine(projectRoot, "crossroads.json")))
            {
                string search = assemblyDir;
                while (search != null)
                {
                    if (File.Exists(Path.Combine(search, "CLAUDE.md")))
                    {
                        projectRoot = Path.Combine(search, "assets", "data", "maps");
                        break;
                    }
                    search = Directory.GetParent(search)?.FullName;
                }
            }

            Assume.That(Directory.Exists(projectRoot),
                $"Preset maps directory not found. Searched up from: {assemblyDir}. " +
                "Ensure assets/data/maps/ exists relative to the repository root.");

            var loader = new MapLoader(projectRoot);

            // Act + Assert
            Dictionary<string, MapData> result = null;
            Assert.DoesNotThrow(
                () => result = loader.LoadAll(),
                "LoadAll must not throw when loading the preset maps directory.");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ContainsKey("crossroads"),    Is.True, "'crossroads' preset must load.");
            Assert.That(result.ContainsKey("frozen_wastes"), Is.True, "'frozen_wastes' preset must load.");
            Assert.That(result.ContainsKey("ember_ridge"),   Is.True, "'ember_ridge' preset must load.");

            // Verify basic structural integrity of each preset.
            Assert.That(result["crossroads"].Width,    Is.EqualTo(10), "crossroads must be 10 wide.");
            Assert.That(result["crossroads"].Height,   Is.EqualTo(10), "crossroads must be 10 tall.");
            Assert.That(result["frozen_wastes"].Width, Is.EqualTo(12), "frozen_wastes must be 12 wide.");
            Assert.That(result["frozen_wastes"].Height,Is.EqualTo(12), "frozen_wastes must be 12 tall.");
            Assert.That(result["ember_ridge"].Width,   Is.EqualTo(10), "ember_ridge must be 10 wide.");
            Assert.That(result["ember_ridge"].Height,  Is.EqualTo(10), "ember_ridge must be 10 tall.");
        }
    }
}

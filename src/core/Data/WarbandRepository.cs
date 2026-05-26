using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Battlemancers.Core.Data
{
    /// <summary>
    /// Persists and retrieves WarbandData to/from the local filesystem.
    /// One warband = one JSON file. Manifest tracks all saved warbands for fast list loading.
    ///
    /// Thread safety: not thread-safe — call from main thread only.
    /// Zero Unity dependencies — works headless for tests.
    /// </summary>
    public class WarbandRepository
    {
        private const string ManifestFileName = "manifest.json";
        private const int MaxWarbands = 20;

        private readonly string _saveDirectory;
        private readonly Action<string> _logger;

        /// <param name="saveDirectory">Directory where warband JSON files are stored.
        /// In Unity: Application.persistentDataPath + "/warbands".
        /// In tests: any temp directory.</param>
        /// <param name="logger">Optional log sink. Defaults to Console.WriteLine.</param>
        public WarbandRepository(string saveDirectory, Action<string> logger = null)
        {
            _saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));
            _logger = logger ?? Console.WriteLine;
        }

        // --- Public API ---

        /// <summary>Load the manifest (list of all saved warband names/IDs). Returns empty manifest if none exists.</summary>
        public WarbandManifest LoadManifest()
        {
            string path = GetManifestFilePath();
            if (!File.Exists(path))
                return new WarbandManifest();

            try
            {
                return BattlemancersJsonHelper.DeserializeFile<WarbandManifest>(path)
                       ?? new WarbandManifest();
            }
            catch (Exception ex)
            {
                _logger($"[WarbandRepository] Failed to load manifest: {ex.Message}");
                return new WarbandManifest();
            }
        }

        /// <summary>Load a single warband by ID. Returns null if not found.</summary>
        public WarbandData Load(string warbandId)
        {
            if (string.IsNullOrEmpty(warbandId))
                return null;

            string path = GetWarbandFilePath(warbandId);
            if (!File.Exists(path))
                return null;

            try
            {
                return BattlemancersJsonHelper.DeserializeFile<WarbandData>(path);
            }
            catch (Exception ex)
            {
                _logger($"[WarbandRepository] Failed to load warband '{warbandId}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Save a warband. Generates a new WarbandId if one doesn't exist.
        /// Updates the manifest automatically.
        /// Throws InvalidOperationException if MaxWarbands is reached and this is a new warband.
        /// </summary>
        public void Save(WarbandData warband)
        {
            if (warband == null) throw new ArgumentNullException(nameof(warband));

            bool isNew = string.IsNullOrEmpty(warband.WarbandId);

            if (isNew)
            {
                WarbandManifest existingManifest = LoadManifest();
                if (existingManifest.Entries.Count >= MaxWarbands)
                    throw new InvalidOperationException(
                        $"Cannot save warband: maximum of {MaxWarbands} warbands reached.");

                warband.WarbandId = GenerateNewId();
            }

            if (string.IsNullOrEmpty(warband.CreatedAt))
                warband.CreatedAt = DateTime.UtcNow.ToString("o");

            warband.LastModifiedAt = DateTime.UtcNow.ToString("o");

            EnsureDirectoryExists();

            string path = GetWarbandFilePath(warband.WarbandId);
            try
            {
                string json = JsonSerializer.Serialize(warband, BattlemancersJsonHelper.GetOptions());
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                _logger($"[WarbandRepository] Failed to write warband '{warband.WarbandId}': {ex.Message}");
                throw;
            }

            UpdateManifestEntry(warband);
        }

        /// <summary>Delete a warband by ID. Updates manifest. No-op if ID doesn't exist.</summary>
        public void Delete(string warbandId)
        {
            if (string.IsNullOrEmpty(warbandId))
                return;

            string path = GetWarbandFilePath(warbandId);
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    _logger($"[WarbandRepository] Failed to delete warband file '{warbandId}': {ex.Message}");
                }
            }

            RemoveManifestEntry(warbandId);
        }

        /// <summary>Rename a warband (updates both the file and the manifest entry).</summary>
        public void Rename(string warbandId, string newName)
        {
            if (string.IsNullOrEmpty(warbandId)) throw new ArgumentNullException(nameof(warbandId));
            if (newName == null) throw new ArgumentNullException(nameof(newName));

            WarbandData warband = Load(warbandId);
            if (warband == null)
            {
                _logger($"[WarbandRepository] Rename failed: warband '{warbandId}' not found.");
                return;
            }

            warband.Name = newName;
            Save(warband);
        }

        /// <summary>
        /// Duplicate a warband. Creates a new file with a new WarbandId,
        /// name = "{original.Name} (Copy)", and current timestamp.
        /// Returns the new WarbandId.
        /// </summary>
        public string Duplicate(string warbandId)
        {
            if (string.IsNullOrEmpty(warbandId)) throw new ArgumentNullException(nameof(warbandId));

            WarbandData original = Load(warbandId);
            if (original == null)
            {
                _logger($"[WarbandRepository] Duplicate failed: warband '{warbandId}' not found.");
                return null;
            }

            // Serialize then deserialize to produce a deep copy without referencing original objects.
            string json = JsonSerializer.Serialize(original, BattlemancersJsonHelper.GetOptions());
            WarbandData copy = BattlemancersJsonHelper.Deserialize<WarbandData>(json);

            copy.WarbandId = null;  // cleared so Save() assigns a new ID
            copy.Name = $"{original.Name} (Copy)";
            copy.CreatedAt = null;  // cleared so Save() stamps a fresh creation time

            Save(copy);
            return copy.WarbandId;
        }

        // --- Private helpers ---

        private string GetWarbandFilePath(string warbandId) =>
            Path.Combine(_saveDirectory, $"{warbandId}.json");

        private string GetManifestFilePath() =>
            Path.Combine(_saveDirectory, ManifestFileName);

        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);
        }

        private void UpdateManifestEntry(WarbandData warband)
        {
            WarbandManifest manifest = LoadManifest();

            // Remove stale entry if it exists, then append the fresh one.
            manifest.Entries.RemoveAll(e => e.WarbandId == warband.WarbandId);
            manifest.Entries.Add(new WarbandManifestEntry
            {
                WarbandId = warband.WarbandId,
                Name = warband.Name,
                FactionId = warband.FactionId,
                CachedTotalCost = warband.CachedTotalCost,
                LastModifiedAt = warband.LastModifiedAt
            });

            SaveManifest(manifest);
        }

        private void RemoveManifestEntry(string warbandId)
        {
            WarbandManifest manifest = LoadManifest();
            int removed = manifest.Entries.RemoveAll(e => e.WarbandId == warbandId);
            if (removed > 0)
                SaveManifest(manifest);
        }

        private void SaveManifest(WarbandManifest manifest)
        {
            EnsureDirectoryExists();
            string path = GetManifestFilePath();
            try
            {
                string json = JsonSerializer.Serialize(manifest, BattlemancersJsonHelper.GetOptions());
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                _logger($"[WarbandRepository] Failed to write manifest: {ex.Message}");
            }
        }

        private static string GenerateNewId() =>
            Guid.NewGuid().ToString("N"); // 32-char hex, no hyphens
    }
}

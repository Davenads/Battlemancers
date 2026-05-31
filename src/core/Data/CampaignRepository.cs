using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Battlemancers.Core.Data
{
    /// <summary>
    /// Loads and persists campaign save data to disk as JSON.
    /// Each save slot is stored as a separate file named {saveId}.json in the save directory.
    /// Zero Unity dependencies — works headless for tests.
    /// </summary>
    public class CampaignRepository
    {
        private const string ManifestFileName = "campaign_manifest.json";

        /// <summary>Maximum number of concurrent campaign save slots.</summary>
        public const int MaxSaveSlots = 3;

        private readonly string _saveDirectory;
        private readonly Action<string> _logger;

        public CampaignRepository(string saveDirectory, Action<string> logger = null)
        {
            _saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));
            _logger = logger ?? Console.WriteLine;
        }

        // ---------------------------------------------------------------------------
        // Save / Load / Delete
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Persists a campaign save to disk. Generates a SaveId if one is not set.
        /// Throws <see cref="InvalidOperationException"/> when trying to create a new slot
        /// beyond <see cref="MaxSaveSlots"/>.
        /// </summary>
        public void Save(CampaignSaveData saveData)
        {
            if (saveData == null) throw new ArgumentNullException(nameof(saveData));

            bool isNew = string.IsNullOrEmpty(saveData.SaveId);
            if (isNew)
            {
                var manifest = LoadManifest();
                if (manifest.Count >= MaxSaveSlots)
                    throw new InvalidOperationException(
                        $"Cannot create save '{saveData.PlayerDisplayName}': maximum of {MaxSaveSlots} save slots reached.");

                saveData.SaveId = Guid.NewGuid().ToString("N");
            }

            saveData.LastPlayedAt = DateTime.UtcNow.ToString("O"); // ISO 8601 round-trip format

            Directory.CreateDirectory(_saveDirectory);
            string path = SlotPath(saveData.SaveId);
            string json = System.Text.Json.JsonSerializer.Serialize(saveData, BattlemancersJsonHelper.GetOptions());
            File.WriteAllText(path, json);
            _logger($"[CampaignRepository] Saved slot '{saveData.SaveId}' ({saveData.PlayerDisplayName}).");
        }

        /// <summary>
        /// Loads a full campaign save by its SaveId. Returns null if the slot does not exist.
        /// </summary>
        public CampaignSaveData Load(string saveId)
        {
            if (string.IsNullOrEmpty(saveId)) throw new ArgumentNullException(nameof(saveId));

            string path = SlotPath(saveId);
            if (!File.Exists(path))
            {
                _logger($"[CampaignRepository] Slot '{saveId}' not found at {path}.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(path);
                var data = BattlemancersJsonHelper.Deserialize<CampaignSaveData>(json);
                _logger($"[CampaignRepository] Loaded slot '{saveId}'.");
                return data;
            }
            catch (Exception ex)
            {
                _logger($"[CampaignRepository] Failed to load slot '{saveId}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes a campaign save slot by SaveId. No-op if the slot does not exist.
        /// </summary>
        public void Delete(string saveId)
        {
            if (string.IsNullOrEmpty(saveId)) return;

            string path = SlotPath(saveId);
            if (File.Exists(path))
            {
                File.Delete(path);
                _logger($"[CampaignRepository] Deleted slot '{saveId}'.");
            }
        }

        // ---------------------------------------------------------------------------
        // Manifest
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns lightweight metadata for all save slots found in the save directory.
        /// Results are sorted by LastPlayedAt descending (most recent first).
        /// Does not load full save data — suitable for displaying the save-select screen.
        /// </summary>
        public List<CampaignSaveSlot> LoadManifest()
        {
            Directory.CreateDirectory(_saveDirectory);

            var slots = new List<CampaignSaveSlot>();
            foreach (string file in Directory.GetFiles(_saveDirectory, "*.json"))
            {
                // Skip the manifest file itself if it exists as a legacy artifact.
                if (Path.GetFileName(file).Equals(ManifestFileName, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    string json = File.ReadAllText(file);
                    var data = BattlemancersJsonHelper.Deserialize<CampaignSaveData>(json);
                    if (data != null && !string.IsNullOrEmpty(data.SaveId))
                    {
                        slots.Add(new CampaignSaveSlot
                        {
                            SaveId = data.SaveId,
                            PlayerDisplayName = data.PlayerDisplayName,
                            CurrentChapterIndex = data.CurrentChapterIndex,
                            LastPlayedAt = data.LastPlayedAt,
                            TotalMatchesPlayed = data.TotalMatchesPlayed,
                            TotalMatchesWon = data.TotalMatchesWon
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger($"[CampaignRepository] Skipping corrupt file '{file}': {ex.Message}");
                }
            }

            // Sort most-recently-played first; empty timestamps sort to the end.
            slots.Sort((a, b) => string.Compare(b.LastPlayedAt, a.LastPlayedAt, StringComparison.Ordinal));
            return slots;
        }

        // ---------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------

        private string SlotPath(string saveId) =>
            Path.Combine(_saveDirectory, $"{saveId}.json");
    }

    /// <summary>
    /// Lightweight save-slot summary returned by <see cref="CampaignRepository.LoadManifest"/>.
    /// Contains only the fields needed for the save-select UI; avoids loading full scenario history.
    /// </summary>
    public class CampaignSaveSlot
    {
        public string SaveId { get; set; } = "";
        public string PlayerDisplayName { get; set; } = "";
        public int CurrentChapterIndex { get; set; }
        public string LastPlayedAt { get; set; } = "";
        public int TotalMatchesPlayed { get; set; }
        public int TotalMatchesWon { get; set; }
    }
}

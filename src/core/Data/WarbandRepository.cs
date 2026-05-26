using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Battlemancers.Core.Data
{
    /// <summary>
    /// Loads and persists player warband lists to disk as JSON.
    /// One instance per application session. Call LoadAll() once at startup.
    /// Zero Unity dependencies — works headless for tests.
    /// </summary>
    public class WarbandRepository
    {
        private const string FileName = "warbands.json";

        private readonly string _saveDirectory;
        private readonly Action<string> _logger;
        private List<WarbandData> _warbands;

        public WarbandRepository(string saveDirectory, Action<string> logger = null)
        {
            _saveDirectory = saveDirectory ?? throw new ArgumentNullException(nameof(saveDirectory));
            _logger = logger ?? Console.WriteLine;
        }

        /// <summary>Loads all saved warbands from disk. Returns empty list if file doesn't exist.</summary>
        public List<WarbandData> LoadAll()
        {
            string path = Path.Combine(_saveDirectory, FileName);
            if (!File.Exists(path))
            {
                _logger($"[WarbandRepository] No save file found at {path} — starting fresh.");
                _warbands = new List<WarbandData>();
                return _warbands;
            }

            try
            {
                string json = File.ReadAllText(path);
                _warbands = BattlemancersJsonHelper.Deserialize<List<WarbandData>>(json)
                            ?? new List<WarbandData>();
                _warbands.Sort((a, b) => a.CreatedAt.CompareTo(b.CreatedAt));
                _logger($"[WarbandRepository] Loaded {_warbands.Count} warband(s).");
            }
            catch (Exception ex)
            {
                _logger($"[WarbandRepository] Failed to load warbands: {ex.Message}. Starting fresh.");
                _warbands = new List<WarbandData>();
            }

            return _warbands;
        }

        /// <summary>Returns all loaded warbands. Call LoadAll() first.</summary>
        public IReadOnlyList<WarbandData> GetAll() =>
            _warbands ?? throw new InvalidOperationException("Call LoadAll() before GetAll().");

        /// <summary>Returns warband by ID, or null if not found.</summary>
        public WarbandData GetById(string warbandId) =>
            _warbands?.FirstOrDefault(w => w.WarbandId == warbandId);

        /// <summary>
        /// Saves or updates a warband. Generates a new WarbandId if empty.
        /// Persists the full list to disk immediately.
        /// </summary>
        public void Save(WarbandData warband)
        {
            if (warband == null) throw new ArgumentNullException(nameof(warband));
            if (string.IsNullOrEmpty(warband.WarbandId))
                warband.WarbandId = Guid.NewGuid().ToString();

            warband.LastModified = DateTime.UtcNow;

            _warbands ??= new List<WarbandData>();
            int idx = _warbands.FindIndex(w => w.WarbandId == warband.WarbandId);
            if (idx >= 0)
                _warbands[idx] = warband;
            else
            {
                warband.CreatedAt = DateTime.UtcNow;
                _warbands.Add(warband);
            }

            Persist();
            _logger($"[WarbandRepository] Saved warband '{warband.Name}' ({warband.WarbandId}).");
        }

        /// <summary>Deletes a warband by ID. No-op if not found.</summary>
        public void Delete(string warbandId)
        {
            int removed = _warbands?.RemoveAll(w => w.WarbandId == warbandId) ?? 0;
            if (removed > 0)
            {
                Persist();
                _logger($"[WarbandRepository] Deleted warband {warbandId}.");
            }
        }

        /// <summary>Creates a deep copy of an existing warband with a new ID and "(Copy)" suffix.</summary>
        public WarbandData Duplicate(string warbandId)
        {
            var original = GetById(warbandId);
            if (original == null)
                throw new ArgumentException($"Warband {warbandId} not found.", nameof(warbandId));

            // Serialize + deserialize for a true deep copy with no shared references.
            string json = System.Text.Json.JsonSerializer.Serialize(original, BattlemancersJsonHelper.GetOptions());
            var copy = BattlemancersJsonHelper.Deserialize<WarbandData>(json);
            copy.WarbandId = Guid.NewGuid().ToString();
            copy.Name = original.Name + " (Copy)";
            copy.CreatedAt = DateTime.UtcNow;
            copy.LastModified = DateTime.UtcNow;

            Save(copy);
            return copy;
        }

        private void Persist()
        {
            try
            {
                Directory.CreateDirectory(_saveDirectory);
                string path = Path.Combine(_saveDirectory, FileName);
                string json = System.Text.Json.JsonSerializer.Serialize(_warbands, BattlemancersJsonHelper.GetOptions());
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                _logger($"[WarbandRepository] Failed to persist warbands: {ex.Message}");
            }
        }
    }
}

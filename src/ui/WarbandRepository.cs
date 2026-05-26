using System;
using System.IO;
using UnityEngine;
using Battlemancers.Data;

namespace Battlemancers.UI
{
    /// <summary>
    /// Persists <see cref="WarbandSave"/> instances to and from JSON files on disk.
    ///
    /// Each warband is stored as "{saveId}.json" inside the configured save directory.
    /// Uses Unity's <c>JsonUtility</c> for serialization (no additional dependencies).
    ///
    /// The save directory is created on first write if it does not already exist.
    ///
    /// Unity only — depends on <c>Application.persistentDataPath</c> indirectly via
    /// the <paramref name="savePath"/> passed to the constructor.
    /// </summary>
    public class WarbandRepository
    {
        // ---------------------------------------------------------------------------
        // State
        // ---------------------------------------------------------------------------

        private readonly string _saveDirectory;

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Creates a repository that reads and writes warbands in <paramref name="saveDirectory"/>.
        /// </summary>
        /// <param name="saveDirectory">
        /// Absolute path to the directory in which warband JSON files are stored.
        /// Typically <c>Application.persistentDataPath + "/warbands"</c>.
        /// </param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="saveDirectory"/> is null or empty.</exception>
        public WarbandRepository(string saveDirectory)
        {
            if (string.IsNullOrEmpty(saveDirectory))
                throw new ArgumentException("saveDirectory must not be null or empty.", nameof(saveDirectory));

            _saveDirectory = saveDirectory;
        }

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Saves <paramref name="warband"/> to disk as "{saveId}.json".
        /// Generates a new GUID save ID if <c>warband.saveId</c> is null or empty.
        /// Creates the save directory if it does not exist.
        /// </summary>
        /// <param name="warband">The warband to persist. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="warband"/> is null.</exception>
        public void Save(WarbandSave warband)
        {
            if (warband == null) throw new ArgumentNullException(nameof(warband));

            if (string.IsNullOrEmpty(warband.saveId))
                warband.saveId = Guid.NewGuid().ToString();

            warband.MarkModified();

            if (!Directory.Exists(_saveDirectory))
                Directory.CreateDirectory(_saveDirectory);

            string filePath = BuildFilePath(warband.saveId);
            string json     = JsonUtility.ToJson(warband, prettyPrint: true);
            File.WriteAllText(filePath, json);

            Debug.Log($"[WarbandRepository] Saved warband '{warband.displayName}' ({warband.saveId}) to: {filePath}");
        }

        /// <summary>
        /// Loads and returns the warband with <paramref name="saveId"/>, or null if the file does not exist.
        /// </summary>
        /// <param name="saveId">The GUID string identifying the warband to load.</param>
        /// <returns>The deserialized <see cref="WarbandSave"/>, or null if not found.</returns>
        public WarbandSave Load(string saveId)
        {
            if (string.IsNullOrEmpty(saveId)) return null;

            string filePath = BuildFilePath(saveId);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[WarbandRepository] Warband file not found: {filePath}");
                return null;
            }

            string json = File.ReadAllText(filePath);
            WarbandSave warband = JsonUtility.FromJson<WarbandSave>(json);
            Debug.Log($"[WarbandRepository] Loaded warband '{warband?.displayName}' ({saveId}).");
            return warband;
        }

        /// <summary>
        /// Deletes the save file for <paramref name="saveId"/> if it exists.
        /// </summary>
        /// <param name="saveId">The GUID string identifying the warband to delete.</param>
        public void Delete(string saveId)
        {
            if (string.IsNullOrEmpty(saveId)) return;

            string filePath = BuildFilePath(saveId);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"[WarbandRepository] Deleted warband file: {filePath}");
            }
        }

        // ---------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------

        private string BuildFilePath(string saveId) =>
            Path.Combine(_saveDirectory, $"{saveId}.json");
    }
}

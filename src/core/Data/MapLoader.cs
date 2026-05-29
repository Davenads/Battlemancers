using System;
using System.Collections.Generic;
using System.IO;

namespace Battlemancers.Core.Data
{
    /// <summary>
    /// Loads <see cref="MapData"/> from JSON files in a given directory.
    /// One instance per game session; call <see cref="LoadAll"/> once at startup.
    /// Zero Unity dependencies — works headless for tests.
    ///
    /// File naming convention: one .json file per map (e.g., crossroads.json).
    /// The <c>MapId</c> field inside the JSON is the canonical key; the file name
    /// is used only for logging.
    /// </summary>
    public class MapLoader
    {
        private readonly string _dataDirectory;
        private readonly Action<string> _logger;

        public MapLoader(string dataDirectory, Action<string> logger = null)
        {
            _dataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));
            _logger = logger ?? Console.WriteLine;
        }

        /// <summary>
        /// Loads all .json files in the data directory and indexes them by MapId.
        /// Files that fail to deserialize are logged and skipped without throwing.
        /// Files with a null or empty MapId are also skipped with a warning.
        /// </summary>
        /// <returns>
        /// A dictionary keyed by <see cref="MapData.MapId"/> (case-insensitive).
        /// Returns an empty dictionary if the directory does not exist or contains no valid maps.
        /// </returns>
        public Dictionary<string, MapData> LoadAll()
        {
            var result = new Dictionary<string, MapData>(StringComparer.OrdinalIgnoreCase);

            if (!Directory.Exists(_dataDirectory))
            {
                _logger($"[MapLoader] Directory not found: {_dataDirectory}");
                return result;
            }

            foreach (string file in Directory.GetFiles(_dataDirectory, "*.json"))
            {
                try
                {
                    var data = BattlemancersJsonHelper.DeserializeFile<MapData>(file);
                    if (data?.MapId == null || data.MapId.Length == 0)
                    {
                        _logger($"[MapLoader] Skipping {file} — missing MapId");
                        continue;
                    }
                    result[data.MapId] = data;
                }
                catch (Exception ex)
                {
                    _logger($"[MapLoader] Failed to load {file}: {ex.Message}");
                }
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;

namespace Battlemancers.Core.Data
{
    /// <summary>
    /// Loads MancerRuntimeData from JSON files in a given directory.
    /// One instance per game session; call LoadAll() once at startup.
    /// Zero Unity dependencies — works headless for tests.
    /// </summary>
    public class MancerDataLoader
    {
        private readonly string _dataDirectory;
        private readonly Action<string> _logger;

        public MancerDataLoader(string dataDirectory, Action<string> logger = null)
        {
            _dataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));
            _logger = logger ?? Console.WriteLine;
        }

        /// <summary>
        /// Loads all .json files in the data directory and indexes them by MancerId.
        /// Files that fail to deserialize are skipped with a logged warning.
        /// </summary>
        public Dictionary<string, MancerRuntimeData> LoadAll()
        {
            var result = new Dictionary<string, MancerRuntimeData>(StringComparer.OrdinalIgnoreCase);

            if (!Directory.Exists(_dataDirectory))
            {
                _logger($"[MancerDataLoader] Directory not found: {_dataDirectory}");
                return result;
            }

            foreach (string file in Directory.GetFiles(_dataDirectory, "*.json"))
            {
                try
                {
                    var data = BattlemancersJsonHelper.DeserializeFile<MancerRuntimeData>(file);
                    if (data?.MancerId == null)
                    {
                        _logger($"[MancerDataLoader] Skipping {file} — missing MancerId");
                        continue;
                    }
                    result[data.MancerId] = data;
                }
                catch (Exception ex)
                {
                    _logger($"[MancerDataLoader] Failed to load {file}: {ex.Message}");
                }
            }

            return result;
        }
    }
}

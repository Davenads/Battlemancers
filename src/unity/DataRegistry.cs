using System;
using System.Collections.Generic;
using UnityEngine;
using Battlemancers.Core.Data;

namespace Battlemancers.Unity
{
    /// <summary>
    /// Unity-side adapter that wraps MancerDataLoader and provides a simple keyed lookup
    /// for Mancer runtime data. This is the single Unity-layer access point for all
    /// Mancer definitions — no other MonoBehaviour should call MancerDataLoader directly.
    ///
    /// Initialized by SimulationBootstrapper.Awake() before any other system reads from it.
    /// Not intended to be added to a GameObject manually; SimulationBootstrapper adds it
    /// via AddComponent and calls Initialize() immediately.
    /// </summary>
    public class DataRegistry : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // State
        // ---------------------------------------------------------------------------

        private Dictionary<string, MancerRuntimeData> _mancers;

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Loads all Mancer JSON files from the specified directory.
        /// Must be called exactly once before any GetMancer() or AllMancers access.
        /// </summary>
        /// <param name="dataDirectory">
        /// Absolute path to the directory containing Mancer JSON files.
        /// Typically Application.streamingAssetsPath + "/data/mancers".
        /// Must not be null or empty.
        /// </param>
        /// <exception cref="ArgumentException">Thrown if dataDirectory is null or empty.</exception>
        public void Initialize(string dataDirectory)
        {
            if (string.IsNullOrEmpty(dataDirectory))
                throw new ArgumentException("dataDirectory must not be null or empty.", nameof(dataDirectory));

            var loader = new MancerDataLoader(dataDirectory, Debug.LogWarning);
            _mancers = loader.LoadAll();
        }

        /// <summary>
        /// Returns the MancerRuntimeData for the given Mancer ID (case-insensitive).
        /// Returns null if no data was loaded for that ID or if Initialize() has not been called.
        /// </summary>
        /// <param name="mancerId">
        /// The Mancer archetype ID (e.g., "pyromancer", "hydromancer").
        /// Case-insensitive — matches the MancerId field in the JSON file.
        /// </param>
        /// <returns>The MancerRuntimeData, or null if not found.</returns>
        public MancerRuntimeData GetMancer(string mancerId)
        {
            if (_mancers == null || mancerId == null)
                return null;

            _mancers.TryGetValue(mancerId, out MancerRuntimeData data);
            return data;
        }

        /// <summary>
        /// Read-only view of all loaded Mancer data, keyed by MancerId.
        /// Returns an empty dictionary if Initialize() has not been called.
        /// </summary>
        public IReadOnlyDictionary<string, MancerRuntimeData> AllMancers =>
            (IReadOnlyDictionary<string, MancerRuntimeData>)_mancers
            ?? new Dictionary<string, MancerRuntimeData>();
    }
}

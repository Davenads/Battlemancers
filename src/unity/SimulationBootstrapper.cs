using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Battlemancers.Core.Data;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Simulation;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Unity
{
    /// <summary>
    /// Single entry point for initializing a match's pure C# simulation from the Unity layer.
    ///
    /// On Awake, SimulationBootstrapper:
    ///   1. Constructs all pure C# managers via constructor injection.
    ///   2. Creates a SimulationState with two players and a 10×10 grid.
    ///   3. Loads Mancer data from StreamingAssets via MancerDataLoader.
    ///   4. Spawns a Pyromancer for p1 at (2,5) and a Hydromancer for p2 at (7,5).
    ///   5. Starts the first turn (phase is Planning by default after construction).
    ///
    /// All managers are exposed as public readonly properties. Other MonoBehaviours
    /// (GridRenderer, UnitViewManager, PlayerInputController, etc.) must hold a
    /// [SerializeField] reference to this bootstrapper and read managers from it —
    /// never use FindObjectOfType or static access.
    /// </summary>
    public class SimulationBootstrapper : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        private const string Player1Id = "p1";
        private const string Player2Id = "p2";

        private const int GridWidth = 10;
        private const int GridHeight = 10;

        private const string PyromancerId = "pyromancer";
        private const string HydromancerId = "hydromancer";

        // Default stats used when JSON data is unavailable for a given Mancer.
        private const int DefaultMaxHp = 80;
        private const int DefaultMoveRange = 3;
        private const int DefaultPointCost = 100;

        // ---------------------------------------------------------------------------
        // Inspector config
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Sub-path under Application.streamingAssetsPath where Mancer JSON files live.
        /// Default: "data/mancers"
        /// </summary>
        [SerializeField] private string _mancerDataSubPath = "data/mancers";

        // ---------------------------------------------------------------------------
        // Public simulation surface
        // ---------------------------------------------------------------------------

        /// <summary>The complete runtime state of the current match.</summary>
        public SimulationState State { get; private set; }

        /// <summary>Drives blind simultaneous turn resolution.</summary>
        public TurnManager TurnManager { get; private set; }

        /// <summary>Manages per-unit temperature and thermal status transitions.</summary>
        public TemperatureManager TemperatureManager { get; private set; }

        /// <summary>Manages all active status effects for all units.</summary>
        public StatusManager StatusManager { get; private set; }

        /// <summary>Resolves element-vs-tile interactions.</summary>
        public ElementResolver ElementResolver { get; private set; }

        /// <summary>Resolves spell casts — damage, tile effects, statuses, temperature.</summary>
        public SpellResolver SpellResolver { get; private set; }

        /// <summary>Unity-side Mancer data registry. Available after Awake.</summary>
        public DataRegistry DataRegistry { get; private set; }

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            InitializeSimulation();
        }

        // ---------------------------------------------------------------------------
        // Initialization
        // ---------------------------------------------------------------------------

        private void InitializeSimulation()
        {
            Debug.Log("[SimulationBootstrapper] Initializing simulation...");

            // ------------------------------------------------------------------
            // Step 1: Build managers via constructor injection chain.
            // Order matters: StatusManager has no deps; TemperatureManager needs
            // StatusManager; ElementResolver is standalone; SpellResolver needs all three.
            // ------------------------------------------------------------------
            StatusManager = new StatusManager();
            Debug.Log("[SimulationBootstrapper] StatusManager created.");

            TemperatureManager = new TemperatureManager(StatusManager);
            Debug.Log("[SimulationBootstrapper] TemperatureManager created.");

            ElementResolver = ElementResolver.CreateDefault();
            Debug.Log("[SimulationBootstrapper] ElementResolver created (default interaction table loaded).");

            // ------------------------------------------------------------------
            // Step 2: Build the DataRegistry and load Mancer JSON data.
            // ------------------------------------------------------------------
            string dataDirectory = Path.Combine(Application.streamingAssetsPath, _mancerDataSubPath);
            DataRegistry = gameObject.AddComponent<DataRegistry>();
            DataRegistry.Initialize(dataDirectory);
            Debug.Log($"[SimulationBootstrapper] DataRegistry initialized. Loaded {DataRegistry.AllMancers.Count} Mancer(s) from: {dataDirectory}");

            // ------------------------------------------------------------------
            // Step 3: Create SimulationState with a 10×10 grid and two players.
            // ------------------------------------------------------------------
            var grid = new GridData(GridWidth, GridHeight);
            State = new SimulationState(grid, new[] { Player1Id, Player2Id });
            Debug.Log($"[SimulationBootstrapper] SimulationState created. Grid: {GridWidth}x{GridHeight}, Players: {Player1Id}, {Player2Id}");

            // ------------------------------------------------------------------
            // Step 4: Wire SpellResolver (needs State-aware managers + ElementResolver).
            // ------------------------------------------------------------------
            SpellResolver = new SpellResolver(ElementResolver, StatusManager, TemperatureManager);
            Debug.Log("[SimulationBootstrapper] SpellResolver created.");

            // ------------------------------------------------------------------
            // Step 5: Wire TurnManager (needs State + TemperatureManager).
            // ------------------------------------------------------------------
            TurnManager = new TurnManager(State, TemperatureManager);
            Debug.Log("[SimulationBootstrapper] TurnManager created.");

            // ------------------------------------------------------------------
            // Step 6: Spawn test units — Pyromancer for p1, Hydromancer for p2.
            // ------------------------------------------------------------------
            SpawnUnit(PyromancerId, Player1Id, new GridPosition(2, 5), index: 0);
            SpawnUnit(HydromancerId, Player2Id, new GridPosition(7, 5), index: 0);

            Debug.Log("[SimulationBootstrapper] Simulation initialized. Turn 1 — Planning phase.");
        }

        /// <summary>
        /// Creates a UnitState for the named Mancer archetype, populating stats from JSON
        /// data when available and falling back to safe defaults when the data is missing.
        /// Registers the unit into SimulationState.
        /// </summary>
        private void SpawnUnit(string mancerId, string ownerId, GridPosition position, int index)
        {
            MancerRuntimeData data = DataRegistry.GetMancer(mancerId);

            int maxHp = data != null ? data.MaxHP : DefaultMaxHp;
            int moveRange = data != null ? data.MoveRange : DefaultMoveRange;
            int pointCost = data != null ? data.BaseCost : DefaultPointCost;

            if (data == null)
            {
                Debug.LogWarning($"[SimulationBootstrapper] No JSON data found for '{mancerId}'. Using defaults: MaxHP={maxHp}, MoveRange={moveRange}, PointCost={pointCost}");
            }
            else
            {
                Debug.Log($"[SimulationBootstrapper] Spawning {data.DisplayName} — HP:{maxHp} Move:{moveRange} Cost:{pointCost} at {position}");
            }

            string unitId = $"{ownerId}_{mancerId}_{index}";

            var unit = new UnitState(
                id:                unitId,
                mancerArchetypeId: mancerId,
                type:              UnitType.Mancer,
                ownerId:           ownerId,
                position:          position,
                maxHP:             maxHp,
                moveRange:         moveRange,
                pointCost:         pointCost
            );

            State.RegisterUnit(unit);
            Debug.Log($"[SimulationBootstrapper] Unit registered: {unit}");
        }
    }
}

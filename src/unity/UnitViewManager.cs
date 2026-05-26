using System.Collections.Generic;
using UnityEngine;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Unity
{
    /// <summary>
    /// Manages all <see cref="UnitView"/> instances on the battlefield.
    ///
    /// Subscribes to <see cref="SimulationEventBus"/> events at Awake and unsubscribes
    /// at OnDestroy. All visual updates are event-driven — this class never polls
    /// SimulationState in Update().
    ///
    /// Responsibilities:
    ///   - Spawning UnitView GameObjects for each unit in the initial state.
    ///   - Routing UnitDamagedEvent → flash + HP bar update.
    ///   - Routing TemperatureChangedEvent → temperature tint update.
    ///   - Routing UnitDiedEvent → death animation and view removal.
    ///   - Routing UnitMovedEvent → world position update.
    /// </summary>
    public class UnitViewManager : MonoBehaviour
    {
        // World units per grid tile. Must match the value used by any GridRenderer.
        private const float TileSize = 1.0f;

        // ---------------------------------------------------------------------------
        // Serialized fields
        // ---------------------------------------------------------------------------

        [SerializeField] private GameObject _unitViewPrefab;

        /// <summary>
        /// The SimulationBootstrapper provides access to the running SimulationState.
        /// Set via the Inspector — never use FindObjectOfType.
        /// </summary>
        [SerializeField] private SimulationBootstrapper _sim;

        [SerializeField] private Sprite _pyromancerSprite;
        [SerializeField] private Sprite _hydromancerSprite;
        [SerializeField] private Sprite _defaultMancerSprite;

        // ---------------------------------------------------------------------------
        // Internal state
        // ---------------------------------------------------------------------------

        // Single source of truth: unitId → the UnitView representing that unit.
        private readonly Dictionary<string, UnitView> _views = new Dictionary<string, UnitView>();

        // ---------------------------------------------------------------------------
        // MonoBehaviour lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            SimulationEventBus.Subscribe<UnitDamagedEvent>(OnUnitDamaged);
            SimulationEventBus.Subscribe<TemperatureChangedEvent>(OnTemperatureChanged);
            SimulationEventBus.Subscribe<UnitDiedEvent>(OnUnitDied);
            SimulationEventBus.Subscribe<UnitMovedEvent>(OnUnitMoved);
        }

        private void OnDestroy()
        {
            SimulationEventBus.Unsubscribe<UnitDamagedEvent>(OnUnitDamaged);
            SimulationEventBus.Unsubscribe<TemperatureChangedEvent>(OnTemperatureChanged);
            SimulationEventBus.Unsubscribe<UnitDiedEvent>(OnUnitDied);
            SimulationEventBus.Unsubscribe<UnitMovedEvent>(OnUnitMoved);
        }

        // ---------------------------------------------------------------------------
        // Unit spawning
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Instantiates a <see cref="UnitView"/> for a single unit and registers it.
        /// </summary>
        /// <param name="unitState">The simulation state of the unit to spawn.</param>
        public void SpawnUnit(UnitState unitState)
        {
            if (_unitViewPrefab == null)
            {
                Debug.LogWarning($"[UnitViewManager] _unitViewPrefab is not assigned. Cannot spawn unit {unitState.Id}.");
                return;
            }

            Sprite sprite = GetSpriteForMancer(unitState.MancerArchetypeId);
            Vector3 worldPos = GridPositionToWorld(unitState.Position);
            GameObject go = Instantiate(_unitViewPrefab, worldPos, Quaternion.identity, transform);

            UnitView view = go.GetComponent<UnitView>();
            if (view == null)
            {
                Debug.LogError($"[UnitViewManager] Prefab '{_unitViewPrefab.name}' has no UnitView component.");
                Destroy(go);
                return;
            }

            view.Initialize(unitState, sprite);
            _views[unitState.Id] = view;
        }

        /// <summary>
        /// Spawns <see cref="UnitView"/> instances for every unit currently registered in the
        /// simulation state. Call this once after the simulation has been fully initialized.
        /// </summary>
        /// <param name="state">The current simulation state containing all units.</param>
        public void SpawnAllUnits(SimulationState state)
        {
            foreach (UnitState unit in state.GetAllUnits())
                SpawnUnit(unit);
        }

        // ---------------------------------------------------------------------------
        // Event handlers
        // ---------------------------------------------------------------------------

        private void OnUnitDamaged(UnitDamagedEvent e)
        {
            if (!_views.TryGetValue(e.UnitId, out UnitView view)) return;

            view.PlayDamageFlash();

            // Update HP bar using the remaining HP from the event and the unit's MaxHP.
            // We need the MaxHP from the simulation state — ask the bootstrapper if available.
            if (_sim != null)
            {
                UnitState unit = _sim.State?.GetUnit(e.UnitId);
                if (unit != null)
                    view.UpdateFromState(unit);
            }
        }

        private void OnTemperatureChanged(TemperatureChangedEvent e)
        {
            if (!_views.TryGetValue(e.UnitId, out UnitView view)) return;
            if (_sim == null) return;

            UnitState unit = _sim.State?.GetUnit(e.UnitId);
            if (unit != null)
                view.UpdateFromState(unit);
        }

        private void OnUnitDied(UnitDiedEvent e)
        {
            if (!_views.TryGetValue(e.UnitId, out UnitView view)) return;

            view.PlayDeathAnimation();
            // Remove from the registry immediately — the view's coroutine will destroy the GO.
            _views.Remove(e.UnitId);
        }

        private void OnUnitMoved(UnitMovedEvent e)
        {
            if (!_views.TryGetValue(e.UnitId, out UnitView view)) return;

            Vector3 targetWorldPos = GridPositionToWorld(e.To);
            view.transform.position = targetWorldPos;
        }

        // ---------------------------------------------------------------------------
        // Coordinate conversion
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Converts a grid position to a Unity world position.
        /// Grid origin (0, 0) maps to world (0, 0, 0).
        /// Tile (x, y) maps to world (x * TileSize, 0, y * TileSize) for a top-down layout.
        /// </summary>
        /// <param name="gridPos">The grid position to convert.</param>
        /// <returns>The corresponding world space position.</returns>
        private static Vector3 GridPositionToWorld(GridPosition gridPos)
        {
            return new Vector3(gridPos.X * TileSize, 0f, gridPos.Y * TileSize);
        }

        // ---------------------------------------------------------------------------
        // Sprite selection
        // ---------------------------------------------------------------------------

        private Sprite GetSpriteForMancer(string mancerArchetypeId) => mancerArchetypeId switch
        {
            "pyromancer"  => _pyromancerSprite  ?? _defaultMancerSprite,
            "hydromancer" => _hydromancerSprite ?? _defaultMancerSprite,
            _             => _defaultMancerSprite
        };
    }
}

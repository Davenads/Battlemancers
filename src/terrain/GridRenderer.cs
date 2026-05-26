using System.Collections.Generic;
using UnityEngine;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Terrain
{
    /// <summary>
    /// Unity MonoBehaviour responsible for spawning and managing the visual tile grid.
    ///
    /// Responsibilities:
    ///   - Instantiate one TileView prefab per cell when BuildGrid() is called.
    ///   - React to TileStateChangedEvent from SimulationEventBus to keep visuals in sync
    ///     without polling SimulationState in Update().
    ///   - Expose ShowMovementRange(), ShowAoePreview(), and ClearHighlights() for the
    ///     input/selection layer to call when planning a move or targeting a spell.
    ///
    /// Architecture rules:
    ///   - Never poll SimulationState in Update(). All updates are event-driven.
    ///   - SimulationState is injected via [SerializeField] — no FindObjectOfType.
    ///   - BuildGrid() is the only place TileView objects are created; it must be called
    ///     exactly once per match (call ClearGrid() first if re-using the renderer).
    /// </summary>
    public class GridRenderer : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Layout constants
        // ---------------------------------------------------------------------------

        private const float TileSize    = 1.0f;
        private const float TileSpacing = 0.05f;

        // ---------------------------------------------------------------------------
        // Inspector fields
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Prefab that carries a TileView component plus tile/overlay child renderers.
        /// Assigned in the Inspector.
        /// </summary>
        [SerializeField] private GameObject _tilePrefab;

        /// <summary>
        /// Reference to the simulation state for the current match.
        /// Assigned in the Inspector (or wired by a SimulationBootstrapper at runtime).
        /// GridRenderer reads Grid data from this object when refreshing tile visuals.
        /// </summary>
        [SerializeField] private SimulationState _simulationState;

        // ---------------------------------------------------------------------------
        // Runtime state
        // ---------------------------------------------------------------------------

        private TileView[,] _tiles;
        private int _gridWidth;
        private int _gridHeight;

        // Tracks which positions currently have highlights (movement or AoE) so
        // ClearHighlights() does not have to iterate the full grid.
        private readonly List<Vector2Int> _currentHighlights = new List<Vector2Int>();

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            SimulationEventBus.Subscribe<TileStateChangedEvent>(OnTileStateChanged);
        }

        private void OnDestroy()
        {
            SimulationEventBus.Unsubscribe<TileStateChangedEvent>(OnTileStateChanged);
        }

        // ---------------------------------------------------------------------------
        // Grid construction
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Instantiates TileView prefabs for a grid of the given dimensions,
        /// centering the grid on this transform's world origin.
        /// Calls RefreshTileStates() after construction so tiles immediately reflect
        /// the current SimulationState (useful when BuildGrid is called mid-match or in tests).
        /// </summary>
        /// <param name="width">Number of columns (X axis).</param>
        /// <param name="height">Number of rows (Y axis).</param>
        public void BuildGrid(int width, int height)
        {
            if (_tilePrefab == null)
            {
                Debug.LogError("[GridRenderer] _tilePrefab is not assigned. Cannot build grid.");
                return;
            }

            // Tear down any previous grid before building a new one.
            ClearGrid();

            _gridWidth  = width;
            _gridHeight = height;
            _tiles      = new TileView[width, height];

            float step   = TileSize + TileSpacing;
            float xOrigin = -(width  - 1) * step * 0.5f;
            float zOrigin = -(height - 1) * step * 0.5f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 worldPos = new Vector3(
                        xOrigin + x * step,
                        0f,
                        zOrigin + y * step
                    );

                    GameObject tileGo = Instantiate(_tilePrefab, worldPos, Quaternion.identity, transform);
                    tileGo.name = $"Tile_{x}_{y}";

                    TileView view = tileGo.GetComponent<TileView>();
                    if (view == null)
                    {
                        Debug.LogError($"[GridRenderer] Tile prefab at ({x},{y}) is missing a TileView component.");
                        continue;
                    }

                    view.Initialize(x, y);
                    _tiles[x, y] = view;
                }
            }

            RefreshTileStates();
        }

        /// <summary>
        /// Destroys all instantiated TileView children and resets internal state.
        /// Safe to call even when no grid has been built yet.
        /// </summary>
        public void ClearGrid()
        {
            // Destroy all child GameObjects spawned by a previous BuildGrid call.
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            _tiles = null;
            _gridWidth  = 0;
            _gridHeight = 0;
            _currentHighlights.Clear();
        }

        // ---------------------------------------------------------------------------
        // State synchronization
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Reads the full GridData from SimulationState and pushes TileState to every TileView.
        /// Called once after BuildGrid() and can be called manually to force a full repaint
        /// (e.g., after loading a saved match or replaying history).
        /// </summary>
        public void RefreshTileStates()
        {
            if (_tiles == null || _simulationState == null)
                return;

            GridData grid = _simulationState.Grid;

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    Tile tile = grid.GetTile(new GridPosition(x, y));
                    if (tile != null && _tiles[x, y] != null)
                        _tiles[x, y].SetTileState(tile.State);
                }
            }
        }

        // ---------------------------------------------------------------------------
        // SimulationEventBus handler
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Reacts to a single-tile state change event published by the simulation layer.
        /// Updates only the affected TileView — no full grid refresh needed.
        /// </summary>
        private void OnTileStateChanged(TileStateChangedEvent e)
        {
            int x = e.Position.X;
            int y = e.Position.Y;

            if (_tiles == null || x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight)
                return;

            TileView view = _tiles[x, y];
            if (view != null)
                view.SetTileState(e.NewState);
        }

        // ---------------------------------------------------------------------------
        // Highlight API — called by the input / selection layer
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Highlights all positions in <paramref name="reachableTiles"/> as movement range
        /// (yellow semi-transparent overlay). Clears any previously active highlights first.
        /// </summary>
        public void ShowMovementRange(List<Vector2Int> reachableTiles)
        {
            ClearHighlights();

            foreach (Vector2Int pos in reachableTiles)
            {
                TileView view = GetTile(pos);
                if (view != null)
                {
                    view.SetHighlight(isMovementRange: true);
                    _currentHighlights.Add(pos);
                }
            }
        }

        /// <summary>
        /// Highlights all positions in <paramref name="affectedTiles"/> as AoE preview
        /// (red semi-transparent overlay). Clears any previously active highlights first.
        /// </summary>
        public void ShowAoePreview(List<Vector2Int> affectedTiles)
        {
            ClearHighlights();

            foreach (Vector2Int pos in affectedTiles)
            {
                TileView view = GetTile(pos);
                if (view != null)
                {
                    view.SetAoePreview(show: true);
                    _currentHighlights.Add(pos);
                }
            }
        }

        /// <summary>
        /// Removes all active movement-range and AoE-preview overlays.
        /// Only iterates the tracked highlight positions, not the full grid.
        /// </summary>
        public void ClearHighlights()
        {
            foreach (Vector2Int pos in _currentHighlights)
            {
                TileView view = GetTile(pos);
                view?.ClearOverlays();
            }

            _currentHighlights.Clear();
        }

        // ---------------------------------------------------------------------------
        // Tile accessors
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns the TileView at the given grid coordinates.
        /// Returns null if the grid has not been built or the position is out of bounds.
        /// </summary>
        public TileView GetTile(int x, int y)
        {
            if (_tiles == null || x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight)
                return null;

            return _tiles[x, y];
        }

        /// <summary>
        /// Returns the TileView at the given grid position.
        /// Returns null if the grid has not been built or the position is out of bounds.
        /// </summary>
        public TileView GetTile(Vector2Int pos) => GetTile(pos.x, pos.y);
    }
}

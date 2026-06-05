using System.Collections.Generic;
using UnityEngine;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;

namespace Battlemancers.Presentation
{
    /// <summary>
    /// Color intent used when highlighting tiles for player input feedback.
    /// </summary>
    public enum HighlightColor
    {
        /// <summary>Valid movement destination.</summary>
        Move,
        /// <summary>Valid spell target.</summary>
        SpellTarget,
        /// <summary>Currently selected unit's tile.</summary>
        Selected,
        /// <summary>Hostile unit or danger zone.</summary>
        Danger
    }


    /// <summary>
    /// MonoBehaviour that renders the game grid by reading TileState from SimulationState.
    ///
    /// In Start(), reads SimulationState.Board dimensions and instantiates a tile GameObject
    /// for each cell. Exposes RefreshTile() and RefreshAll() for the BattleSceneController
    /// to call after simulation events arrive.
    ///
    /// Does NOT contain any game logic. Does NOT call TurnManager.
    /// All Unity dependencies are confined to this class — zero pure-C# simulation impact.
    /// </summary>
    public class GridRenderer : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Tile color constants — overridable via Inspector arrays
        // ---------------------------------------------------------------------------

        private static readonly Color ColorNormal   = new Color(0.15f, 0.15f, 0.15f); // dark grey
        private static readonly Color ColorBurning  = new Color(0.9f, 0.3f, 0.05f);   // orange
        private static readonly Color ColorFrozen   = new Color(0.5f, 0.85f, 1.0f);   // light blue
        private static readonly Color ColorToxic    = new Color(0.2f, 0.7f, 0.1f);    // sickly green
        private static readonly Color ColorWet      = new Color(0.1f, 0.3f, 0.8f);    // blue
        private static readonly Color ColorObsidian = new Color(0.15f, 0.1f, 0.2f);   // dark purple
        private static readonly Color ColorMud      = new Color(0.4f, 0.25f, 0.1f);   // brown
        private static readonly Color ColorCharged  = new Color(0.9f, 0.9f, 0.2f);    // bright yellow
        private static readonly Color ColorCorrupt  = new Color(0.3f, 0.0f, 0.3f);    // dark magenta
        private static readonly Color ColorPermafrost = new Color(0.7f, 0.9f, 1.0f);  // pale blue
        private static readonly Color ColorVines    = new Color(0.1f, 0.5f, 0.05f);   // deep green
        private static readonly Color ColorSpores   = new Color(0.45f, 0.6f, 0.05f);  // yellow-green
        private static readonly Color ColorDestroyed = new Color(0.05f, 0.05f, 0.05f); // near black
        private static readonly Color ColorSteam    = new Color(0.75f, 0.75f, 0.75f); // light grey
        private static readonly Color ColorNatural  = new Color(0.2f, 0.5f, 0.15f);   // grass green

        // Highlight overlay colors for player input feedback.
        private static readonly Color ColorHighlightMove     = new Color(0.15f, 0.9f, 0.3f, 0.85f);  // bright green
        private static readonly Color ColorHighlightSpell    = new Color(0.9f, 0.4f, 0.1f, 0.85f);   // orange-red
        private static readonly Color ColorHighlightSelected = new Color(1.0f, 1.0f, 0.2f, 0.9f);    // bright yellow
        private static readonly Color ColorHighlightDanger   = new Color(0.9f, 0.1f, 0.1f, 0.85f);   // red

        // World units per grid tile.
        private const float TileWorldSize = 1.0f;

        // ---------------------------------------------------------------------------
        // Inspector fields
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Optional tile prefab. If assigned, this prefab is instantiated for each cell.
        /// If null, a default primitive quad is created instead.
        /// </summary>
        [SerializeField] private GameObject _tilePrefab;

        /// <summary>
        /// The simulation state for the current match. Assigned in the Inspector or
        /// wired at runtime by SimulationBootstrapper.
        /// </summary>
        [SerializeField] private SimulationBootstrapper _sim;

        // ---------------------------------------------------------------------------
        // Runtime state
        // ---------------------------------------------------------------------------

        // Maps grid position to the GameObject representing that tile.
        private readonly Dictionary<Vector2Int, GameObject> _tileObjects
            = new Dictionary<Vector2Int, GameObject>();

        // Tracks tiles currently showing a highlight overlay so ClearHighlights can restore them.
        private readonly HashSet<Vector2Int> _highlightedTiles = new HashSet<Vector2Int>();

        private int _gridWidth;
        private int _gridHeight;

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Start()
        {
            if (_sim == null)
            {
                Debug.LogError("[GridRenderer] SimulationBootstrapper is not assigned.");
                return;
            }

            GridData grid = _sim.State.Grid;
            _gridWidth  = grid.Width;
            _gridHeight = grid.Height;

            BuildGrid();
            RefreshAll(_sim.State);
        }

        // ---------------------------------------------------------------------------
        // Grid construction
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Instantiates a tile GameObject for every cell in the grid.
        /// Called once in Start(). Safe to call again after ClearGrid().
        /// </summary>
        private void BuildGrid()
        {
            ClearGrid();

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    Vector3 worldPos = GridToWorld(new Vector2Int(x, y));
                    worldPos.y = -0.01f; // Slightly below units to prevent z-fighting.

                    GameObject tileGo;
                    if (_tilePrefab != null)
                    {
                        tileGo = Instantiate(_tilePrefab, worldPos, Quaternion.Euler(90f, 0f, 0f), transform);
                    }
                    else
                    {
                        // Fallback: plain quad primitive.
                        tileGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        tileGo.transform.SetParent(transform, worldPositionStays: false);
                        tileGo.transform.position = worldPos;
                        tileGo.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                        tileGo.transform.localScale = Vector3.one * TileWorldSize;

                        // Remove the collider from the primitive to avoid physics overhead.
                        Collider col = tileGo.GetComponent<Collider>();
                        if (col != null) Destroy(col);
                    }

                    tileGo.name = $"Tile_{x}_{y}";
                    _tileObjects[new Vector2Int(x, y)] = tileGo;
                }
            }
        }

        /// <summary>
        /// Destroys all instantiated tile GameObjects and clears the internal map.
        /// </summary>
        private void ClearGrid()
        {
            foreach (GameObject go in _tileObjects.Values)
            {
                if (go != null) Destroy(go);
            }
            _tileObjects.Clear();
        }

        // ---------------------------------------------------------------------------
        // Public refresh API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Updates the visual of a single tile based on the provided TileState.
        /// Called by BattleSceneController when a TileStateChangedEvent is processed.
        /// </summary>
        /// <param name="pos">Grid position of the tile to refresh.</param>
        /// <param name="state">The new TileState to visualize.</param>
        public void RefreshTile(Vector2Int pos, TileState state)
        {
            if (!_tileObjects.TryGetValue(pos, out GameObject tileGo) || tileGo == null)
                return;

            Color tileColor = TileStateToColor(state);
            ApplyColorToTile(tileGo, tileColor);
        }

        /// <summary>
        /// Reads the full GridData from SimulationState and refreshes every tile's visual.
        /// Called by BattleSceneController after a full turn resolves.
        /// </summary>
        /// <param name="state">The current SimulationState to read tile data from.</param>
        public void RefreshAll(SimulationState state)
        {
            if (state == null) return;

            GridData grid = state.Grid;
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    var gridPos = new GridPosition(x, y);
                    Tile tile = grid.GetTile(gridPos);
                    if (tile == null) continue;

                    RefreshTile(new Vector2Int(x, y), tile.State);
                }
            }
        }

        /// <summary>
        /// Applies a highlight overlay color to a set of tiles, replacing their current visual.
        /// Replaces any existing highlight set — call ClearHighlights() first if needed.
        /// Used by MoveSelectionUI and SpellSelectionUI to show valid targets.
        /// </summary>
        /// <param name="tiles">Grid positions to highlight.</param>
        /// <param name="color">The intent color to apply.</param>
        public void HighlightTiles(IEnumerable<Vector2Int> tiles, HighlightColor color)
        {
            ClearHighlights();

            Color highlightColor = HighlightColorToColor(color);

            foreach (Vector2Int pos in tiles)
            {
                if (!_tileObjects.TryGetValue(pos, out GameObject tileGo) || tileGo == null)
                    continue;

                ApplyColorToTile(tileGo, highlightColor);
                _highlightedTiles.Add(pos);
            }
        }

        /// <summary>
        /// Removes all highlight overlays, restoring tiles to their SimulationState-driven colors.
        /// Safe to call when no highlights are active.
        /// </summary>
        public void ClearHighlights()
        {
            if (_highlightedTiles.Count == 0) return;

            // Restore each highlighted tile to its correct TileState color.
            GridData grid = _sim != null ? _sim.State?.Grid : null;

            foreach (Vector2Int pos in _highlightedTiles)
            {
                if (!_tileObjects.TryGetValue(pos, out GameObject tileGo) || tileGo == null)
                    continue;

                TileState state = TileState.Normal;
                if (grid != null)
                {
                    Tile tile = grid.GetTile(new GridPosition(pos.x, pos.y));
                    if (tile != null) state = tile.State;
                }

                ApplyColorToTile(tileGo, TileStateToColor(state));
            }

            _highlightedTiles.Clear();
        }

        // ---------------------------------------------------------------------------
        // Coordinate conversion (public for other Presentation classes)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Converts a grid position to a Unity world position.
        /// Each tile is TileWorldSize Unity units; origin at (0, 0, 0).
        /// Formula: (gridPos.x, 0, gridPos.y) — XZ plane for isometric readiness.
        /// </summary>
        /// <param name="gridPos">The grid position to convert.</param>
        /// <returns>World space position for the center of that tile.</returns>
        public static Vector3 GridToWorld(Vector2Int gridPos)
        {
            return new Vector3(gridPos.x * TileWorldSize, 0f, gridPos.y * TileWorldSize);
        }

        // ---------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Maps a HighlightColor intent to the actual Color used to tint the tile.
        /// </summary>
        private static Color HighlightColorToColor(HighlightColor color)
        {
            switch (color)
            {
                case HighlightColor.Move:        return ColorHighlightMove;
                case HighlightColor.SpellTarget: return ColorHighlightSpell;
                case HighlightColor.Selected:    return ColorHighlightSelected;
                case HighlightColor.Danger:      return ColorHighlightDanger;
                default:                         return ColorHighlightMove;
            }
        }

        /// <summary>
        /// Maps a TileState enum value to the baseline display color for that terrain type.
        /// </summary>
        private static Color TileStateToColor(TileState state)
        {
            switch (state)
            {
                case TileState.Normal:     return ColorNormal;
                case TileState.Wet:        return ColorWet;
                case TileState.Burning:    return ColorBurning;
                case TileState.Frozen:     return ColorFrozen;
                case TileState.Poisoned:   return ColorToxic;
                case TileState.Charged:    return ColorCharged;
                case TileState.Mud:        return ColorMud;
                case TileState.Corrupted:  return ColorCorrupt;
                case TileState.Obsidian:   return ColorObsidian;
                case TileState.Permafrost: return ColorPermafrost;
                case TileState.Vines:      return ColorVines;
                case TileState.Spores:     return ColorSpores;
                case TileState.Destroyed:  return ColorDestroyed;
                case TileState.Steam:      return ColorSteam;
                case TileState.Natural:    return ColorNatural;
                default:                   return ColorNormal;
            }
        }

        /// <summary>
        /// Applies the given color to the primary renderer on a tile GameObject.
        /// Works with both prefab renderers and the default primitive quad renderer.
        /// </summary>
        private static void ApplyColorToTile(GameObject tileGo, Color color)
        {
            Renderer rend = tileGo.GetComponent<Renderer>();
            if (rend == null) return;

            // Use MaterialPropertyBlock to avoid creating new material instances per tile.
            var block = new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);   // URP Lit shader property
            block.SetColor("_Color", color);        // Fallback for Standard shader
            rend.SetPropertyBlock(block);
        }
    }
}

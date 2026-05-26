using UnityEngine;
using Battlemancers.Core.Grid;

namespace Battlemancers.Terrain
{
    /// <summary>
    /// Unity-side view for a single battlefield tile.
    /// Receives a TileState from GridRenderer and updates its material color accordingly.
    /// Manages two separate render layers: the base tile color and an overlay (highlight / AoE preview).
    /// All color constants are named — no inline color literals outside this block.
    /// </summary>
    public class TileView : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Color constants — one per TileState plus overlay modes
        // ---------------------------------------------------------------------------

        private static readonly Color ColorNormal     = new Color(0.35f, 0.35f, 0.35f);
        private static readonly Color ColorWet        = new Color(0.20f, 0.40f, 0.90f);
        private static readonly Color ColorBurning    = new Color(1.00f, 0.40f, 0.00f);
        private static readonly Color ColorFrozen     = new Color(0.60f, 0.90f, 1.00f);
        private static readonly Color ColorPoisoned   = new Color(0.30f, 0.70f, 0.20f);
        private static readonly Color ColorCharged    = new Color(0.90f, 0.85f, 0.10f);
        private static readonly Color ColorMud        = new Color(0.45f, 0.32f, 0.18f);
        private static readonly Color ColorCorrupted  = new Color(0.28f, 0.10f, 0.35f);
        private static readonly Color ColorObsidian   = new Color(0.12f, 0.08f, 0.15f);
        private static readonly Color ColorPermafrost = new Color(0.75f, 0.90f, 0.95f);
        private static readonly Color ColorVines      = new Color(0.15f, 0.55f, 0.15f);
        private static readonly Color ColorSpores     = new Color(0.55f, 0.80f, 0.25f);
        private static readonly Color ColorDestroyed  = new Color(0.10f, 0.10f, 0.10f);
        private static readonly Color ColorSteam      = new Color(0.80f, 0.80f, 0.85f);
        private static readonly Color ColorNatural    = new Color(0.20f, 0.60f, 0.25f);

        private static readonly Color ColorHighlight  = new Color(1.00f, 1.00f, 0.50f, 0.50f);
        private static readonly Color ColorAoePreview = new Color(1.00f, 0.30f, 0.30f, 0.40f);
        private static readonly Color ColorOverlayClear = new Color(0f, 0f, 0f, 0f);

        // ---------------------------------------------------------------------------
        // Inspector references
        // ---------------------------------------------------------------------------

        [SerializeField] private Renderer _tileRenderer;

        /// <summary>
        /// Semi-transparent overlay quad rendered above the tile surface.
        /// Used for movement-range highlight and AoE preview — kept separate from
        /// the base tile renderer so the two passes do not interfere.
        /// </summary>
        [SerializeField] private Renderer _overlayRenderer;

        // ---------------------------------------------------------------------------
        // State
        // ---------------------------------------------------------------------------

        /// <summary>The tile's logical grid coordinates, set once by Initialize().</summary>
        public Vector2Int GridPosition { get; private set; }

        private bool _highlightActive;
        private bool _aoePreviewActive;

        // ---------------------------------------------------------------------------
        // Initialization
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Called by GridRenderer immediately after instantiation.
        /// Records the grid coordinates and hides the overlay to start clean.
        /// </summary>
        public void Initialize(int x, int y)
        {
            GridPosition = new Vector2Int(x, y);
            ClearOverlays();
        }

        // ---------------------------------------------------------------------------
        // Base tile state
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Maps a TileState enum value to the corresponding named color and applies it
        /// to the base tile renderer material. Unrecognized states fall back to ColorNormal.
        /// </summary>
        public void SetTileState(TileState tileState)
        {
            if (_tileRenderer == null)
                return;

            _tileRenderer.material.color = StateToColor(tileState);
        }

        private static Color StateToColor(TileState state)
        {
            switch (state)
            {
                case TileState.Normal:     return ColorNormal;
                case TileState.Wet:        return ColorWet;
                case TileState.Burning:    return ColorBurning;
                case TileState.Frozen:     return ColorFrozen;
                case TileState.Poisoned:   return ColorPoisoned;
                case TileState.Charged:    return ColorCharged;
                case TileState.Mud:        return ColorMud;
                case TileState.Corrupted:  return ColorCorrupted;
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

        // ---------------------------------------------------------------------------
        // Overlay — movement highlight
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Shows or hides the movement-range highlight overlay.
        /// Pass <c>true</c> for reachable movement tiles; the color is always
        /// <see cref="ColorHighlight"/> (yellow-tint, semi-transparent).
        /// </summary>
        public void SetHighlight(bool isMovementRange)
        {
            _highlightActive = isMovementRange;
            RefreshOverlay();
        }

        // ---------------------------------------------------------------------------
        // Overlay — AoE preview
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Shows or hides the AoE-preview overlay (red tint, semi-transparent).
        /// AoE preview takes priority over movement highlight when both are active.
        /// </summary>
        public void SetAoePreview(bool show)
        {
            _aoePreviewActive = show;
            RefreshOverlay();
        }

        // ---------------------------------------------------------------------------
        // Overlay — clear both passes
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Hides both the highlight and AoE preview overlays.
        /// Called by GridRenderer when clearing all active highlights.
        /// </summary>
        public void ClearOverlays()
        {
            _highlightActive  = false;
            _aoePreviewActive = false;
            RefreshOverlay();
        }

        // ---------------------------------------------------------------------------
        // Internal overlay compositing
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Applies the correct overlay color based on current highlight/preview flags.
        /// AoE preview beats movement highlight when both are active simultaneously.
        /// When neither is active the overlay is set fully transparent.
        /// </summary>
        private void RefreshOverlay()
        {
            if (_overlayRenderer == null)
                return;

            Color overlayColor;
            if (_aoePreviewActive)
                overlayColor = ColorAoePreview;
            else if (_highlightActive)
                overlayColor = ColorHighlight;
            else
                overlayColor = ColorOverlayClear;

            _overlayRenderer.enabled   = _aoePreviewActive || _highlightActive;
            _overlayRenderer.material.color = overlayColor;
        }
    }
}

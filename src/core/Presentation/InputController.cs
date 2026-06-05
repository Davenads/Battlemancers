// BOOTSTRAPPER WIRING REQUIRED (add to BattleSceneBootstrapper.Start() after all SetDependencies calls):
// inputController.SetDependencies(actionMenuUI, moveSelectionUI, spellSelectionUI, gridRenderer, sim, hotseatOrchestrator);

using System;
using System.Collections.Generic;
using UnityEngine;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Unity;

namespace Battlemancers.Presentation
{
    /// <summary>
    /// MonoBehaviour that converts mouse clicks into game actions by routing
    /// to the appropriate UI system (ActionMenuUI, MoveSelectionUI, SpellSelectionUI).
    ///
    /// Responsibilities:
    ///   - Raycasting mouse clicks onto the XZ ground plane.
    ///   - Converting world-space hit positions to grid coordinates.
    ///   - Maintaining the InputMode state machine.
    ///   - Routing click events to the correct UI system based on current mode.
    ///
    /// This class performs NO game logic. It does NOT mutate SimulationState.
    /// All dependencies are injected via SetDependencies() — no FindObjectOfType.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        /// <summary>
        /// World units per grid tile. Must match GridRenderer.TileWorldSize.
        /// Used for the WorldToGrid inverse calculation.
        /// </summary>
        private const float TileWorldSize = 1.0f;

        // ---------------------------------------------------------------------------
        // Injected dependencies (set by BattleSceneBootstrapper)
        // ---------------------------------------------------------------------------

        private ActionMenuUI        _actionMenuUI;
        private MoveSelectionUI     _moveSelectionUI;
        private SpellSelectionUI    _spellSelectionUI;
        private GridRenderer        _gridRenderer;
        private SimulationBootstrapper _sim;
        private HotseatOrchestrator _hotseatOrchestrator;

        // ---------------------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------------------

        /// <summary>Current input routing mode.</summary>
        public InputMode Mode { get; private set; } = InputMode.Idle;

        /// <summary>The unit currently selected by the player. Null when no unit is selected.</summary>
        private UnitState _selectedUnit;

        /// <summary>Whether input is fully disabled (e.g., match has ended).</summary>
        private bool _inputDisabled;

        // ---------------------------------------------------------------------------
        // Dependency injection — called by BattleSceneBootstrapper
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Injects all dependencies. Must be called by BattleSceneBootstrapper before the
        /// first Update() frame processes input.
        /// </summary>
        public void SetDependencies(
            ActionMenuUI actionMenuUI,
            MoveSelectionUI moveSelectionUI,
            SpellSelectionUI spellSelectionUI,
            GridRenderer gridRenderer,
            SimulationBootstrapper sim,
            HotseatOrchestrator hotseatOrchestrator)
        {
            _actionMenuUI        = actionMenuUI;
            _moveSelectionUI     = moveSelectionUI;
            _spellSelectionUI    = spellSelectionUI;
            _gridRenderer        = gridRenderer;
            _sim                 = sim;
            _hotseatOrchestrator = hotseatOrchestrator;

            SubscribeToEvents();
        }

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Update()
        {
            if (_inputDisabled) return;
            if (!Input.GetMouseButtonDown(0)) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Raycast against the XZ ground plane (y = 0).
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 worldHit = ray.GetPoint(distance);
                OnMouseClick(worldHit);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        // ---------------------------------------------------------------------------
        // Core routing method
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Central click routing method. Called from Update() on left mouse button down.
        /// Converts a world position to a grid coordinate and routes to the correct UI system
        /// based on the current InputMode.
        /// </summary>
        /// <param name="worldPosition">World-space hit point from the ground plane raycast.</param>
        internal void OnMouseClick(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldToGrid(worldPosition);

            switch (Mode)
            {
                case InputMode.Idle:
                case InputMode.UnitSelected:
                    HandleIdleOrSelectedClick(gridPos);
                    break;

                case InputMode.MovePending:
                    _moveSelectionUI?.OnTileClicked(gridPos);
                    break;

                case InputMode.SpellTargeting:
                    _spellSelectionUI?.OnTargetConfirmed(gridPos);
                    Mode = InputMode.Idle;
                    break;
            }
        }

        // ---------------------------------------------------------------------------
        // Click handlers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Handles a click while in Idle or UnitSelected mode.
        /// Selects a friendly unit if one is at the clicked tile, or deselects if in UnitSelected mode.
        /// </summary>
        private void HandleIdleOrSelectedClick(Vector2Int gridPos)
        {
            SimulationState state = _sim?.State;
            if (state == null) return;

            UnitState unitAtTile = GetUnitAtGridPos(state, gridPos);
            string currentPlayerId = GetCurrentPlayerId(state);

            if (unitAtTile != null && unitAtTile.OwnerId == currentPlayerId && unitAtTile.IsAlive)
            {
                // Friendly living unit clicked — select it.
                _selectedUnit = unitAtTile;
                _actionMenuUI?.ShowForUnit(unitAtTile);
                _gridRenderer?.HighlightTiles(new[] { gridPos }, HighlightColor.Selected);
                Mode = InputMode.UnitSelected;
            }
            else if (Mode == InputMode.UnitSelected)
            {
                // Clicked empty/enemy tile while a unit was selected — deselect.
                Deselect();
            }
        }

        /// <summary>
        /// Clears current selection, hides the action menu, and returns to Idle mode.
        /// </summary>
        private void Deselect()
        {
            _actionMenuUI?.Hide();
            _gridRenderer?.ClearHighlights();
            _selectedUnit = null;
            Mode = InputMode.Idle;
        }

        // ---------------------------------------------------------------------------
        // Event subscriptions — wired in SetDependencies, removed in OnDestroy
        // ---------------------------------------------------------------------------

        private void SubscribeToEvents()
        {
            if (_actionMenuUI != null)
                _actionMenuUI.ActionCommitted += OnActionCommitted;

            if (_moveSelectionUI != null)
                _moveSelectionUI.MoveConfirmed += OnMoveConfirmed;

            if (_spellSelectionUI != null)
                _spellSelectionUI.SpellIntentReady += OnSpellIntentReady;

            if (_hotseatOrchestrator != null)
                _hotseatOrchestrator.MatchOver += OnMatchOver;
        }

        private void UnsubscribeFromEvents()
        {
            if (_actionMenuUI != null)
                _actionMenuUI.ActionCommitted -= OnActionCommitted;

            if (_moveSelectionUI != null)
                _moveSelectionUI.MoveConfirmed -= OnMoveConfirmed;

            if (_spellSelectionUI != null)
                _spellSelectionUI.SpellIntentReady -= OnSpellIntentReady;

            if (_hotseatOrchestrator != null)
                _hotseatOrchestrator.MatchOver -= OnMatchOver;
        }

        // ---------------------------------------------------------------------------
        // Event handlers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Handles ActionMenuUI.ActionCommitted. Routes to the appropriate pending mode
        /// based on the ActionType the player chose.
        /// </summary>
        private void OnActionCommitted(UnitActionIntent intent)
        {
            if (intent == null) return;

            switch (intent.Type)
            {
                case ActionType.Move:
                    Mode = InputMode.MovePending;
                    break;

                case ActionType.CastSpell:
                    Mode = InputMode.SpellTargeting;
                    break;

                case ActionType.EndActivation:
                    Deselect();
                    break;
            }
        }

        /// <summary>
        /// Handles MoveSelectionUI.MoveConfirmed. Resets to Idle and clears selection state.
        /// </summary>
        private void OnMoveConfirmed(UnitState unit, Vector2Int tile)
        {
            _gridRenderer?.ClearHighlights();
            _selectedUnit = null;
            Mode = InputMode.Idle;
        }

        /// <summary>
        /// Handles SpellSelectionUI.SpellIntentReady. Resets to Idle and clears selection state.
        /// </summary>
        private void OnSpellIntentReady(SpellCastIntent intent)
        {
            _gridRenderer?.ClearHighlights();
            _selectedUnit = null;
            Mode = InputMode.Idle;
        }

        /// <summary>
        /// Handles HotseatOrchestrator.MatchOver. Disables all further input.
        /// </summary>
        private void OnMatchOver(string winnerId)
        {
            _inputDisabled = true;
            Deselect();
            Mode = InputMode.Idle;
        }

        // ---------------------------------------------------------------------------
        // Coordinate conversion
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Converts a Unity world-space position to a grid coordinate.
        /// Inverse of GridRenderer.GridToWorld: gridPos = (worldPos.x / TileWorldSize, worldPos.z / TileWorldSize).
        /// Rounds to nearest integer so clicks between tiles snap to the closest tile.
        /// </summary>
        /// <param name="worldPos">World-space position (XZ plane, y is ignored).</param>
        /// <returns>Grid coordinate as Vector2Int.</returns>
        internal static Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / TileWorldSize);
            int y = Mathf.RoundToInt(worldPos.z / TileWorldSize);
            return new Vector2Int(x, y);
        }

        // ---------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns the living unit at the given grid position, or null if unoccupied.
        /// Uses GridData.GetOccupantId() for O(1) tile lookup, then resolves via SimulationState.
        /// </summary>
        private static UnitState GetUnitAtGridPos(SimulationState state, Vector2Int gridPos)
        {
            string occupantId = state.Grid.GetOccupantId(new GridPosition(gridPos.x, gridPos.y));
            if (occupantId == null) return null;

            UnitState unit = state.GetUnit(occupantId);
            return unit != null && unit.IsAlive ? unit : null;
        }

        /// <summary>
        /// Resolves the player ID string for the current hotseat player index.
        /// HotseatOrchestrator.CurrentPlayer is a 0-based index; PlayerIds maps it to a string.
        /// Returns null if the index is out of range.
        /// </summary>
        private string GetCurrentPlayerId(SimulationState state)
        {
            if (_hotseatOrchestrator == null) return null;
            int idx = _hotseatOrchestrator.CurrentPlayer;
            if (idx < 0 || idx >= state.PlayerIds.Length) return null;
            return state.PlayerIds[idx];
        }
    }

    // ---------------------------------------------------------------------------
    // InputMode enum
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Describes the current input routing state of InputController.
    /// </summary>
    public enum InputMode
    {
        /// <summary>No unit selected, no action in progress.</summary>
        Idle,

        /// <summary>A unit is selected and the ActionMenu is showing.</summary>
        UnitSelected,

        /// <summary>Move mode active — next tile click confirms the move destination.</summary>
        MovePending,

        /// <summary>Spell targeting mode — next tile click confirms the spell target.</summary>
        SpellTargeting
    }
}

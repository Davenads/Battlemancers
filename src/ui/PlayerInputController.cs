using System;
using System.Collections.Generic;
using UnityEngine;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Pathfinding;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Commands;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.UI
{
    /// <summary>
    /// Unity MonoBehaviour that handles all player input during a match.
    ///
    /// Responsibilities:
    /// <list type="bullet">
    ///   <item>Raycasting mouse clicks onto the tile layer to identify clicked grid positions.</item>
    ///   <item>Managing the input state machine (WaitingForTurn → Idle → UnitSelected → SpellTargeting).</item>
    ///   <item>Submitting activation plans to TurnManager via SubmitPlan().</item>
    ///   <item>Subscribing to SimulationEventBus for turn-boundary notifications.</item>
    ///   <item>Exposing a public API for HUDManager spell buttons (SelectSpell, ClearSelection, etc.).</item>
    /// </list>
    ///
    /// Architecture notes:
    /// <list type="bullet">
    ///   <item>Uses Legacy Input (Input.GetMouseButtonDown) — no Input System package required.</item>
    ///   <item>Never calls SpellResolver or TemperatureManager directly — all actions are submitted as Commands.</item>
    ///   <item>Does NOT poll SimulationState in Update — state changes arrive via SimulationEventBus.</item>
    ///   <item>Command construction is isolated in dedicated private methods, not inline in Update/switch.</item>
    /// </list>
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        // -----------------------------------------------------------------------------------------
        // Input state machine
        // -----------------------------------------------------------------------------------------

        private enum InputPhase
        {
            /// <summary>Not this player's planning turn; all input is suppressed.</summary>
            WaitingForTurn,

            /// <summary>This player's turn is active; no unit or spell is selected.</summary>
            Idle,

            /// <summary>A friendly unit is selected; awaiting a move-target click or spell selection.</summary>
            UnitSelected,

            /// <summary>A spell has been chosen; awaiting a target tile click.</summary>
            SpellTargeting,
        }

        // -----------------------------------------------------------------------------------------
        // Named constants
        // -----------------------------------------------------------------------------------------

        /// <summary>Name of the Unity physics layer that tile colliders live on.</summary>
        private const string TileLayerName = "TileLayer";

        /// <summary>Maximum raycast distance when testing mouse clicks against tile colliders.</summary>
        private const float RaycastMaxDistance = 500f;

        /// <summary>
        /// Stub spell range used for pre-submission targeting validation.
        /// Must stay in sync with SpellCommand.StubSpellRange (currently 4).
        /// Wave 2 will replace both with per-spell range values from SpellData.
        /// </summary>
        private const int StubSpellRange = 4;

        // -----------------------------------------------------------------------------------------
        // Inspector fields
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Provides access to SimulationState and TurnManager.
        /// Assigned in the Inspector; populated before the first frame.
        /// </summary>
        [SerializeField] private SimulationBootstrapper _sim;

        /// <summary>Camera used to convert screen-space mouse position into world-space rays.</summary>
        [SerializeField] private Camera _gameCamera;

        /// <summary>
        /// The player ID this controller manages (e.g., "p1" or "p2").
        /// Must match a player ID registered in SimulationState.PlayerIds.
        /// Assigned in the Inspector — no hardcoded string in game logic.
        /// </summary>
        [SerializeField] private string _controlledPlayerId = "p1";

        // -----------------------------------------------------------------------------------------
        // Runtime state
        // -----------------------------------------------------------------------------------------

        private InputPhase _inputPhase = InputPhase.WaitingForTurn;

        /// <summary>Runtime ID of the currently selected unit. Null when no unit is selected.</summary>
        private string _selectedUnitId;

        /// <summary>Spell ID of the spell currently staged for targeting. Null when not in targeting phase.</summary>
        private string _selectedSpellId;

        /// <summary>
        /// Reachable tile set for the currently selected unit.
        /// Populated when a unit is selected; cleared on deselection.
        /// </summary>
        private HashSet<GridPosition> _reachableTiles = new HashSet<GridPosition>();

        /// <summary>
        /// Commands accumulated during this planning phase, to be submitted as a single plan.
        /// </summary>
        private readonly List<Command> _pendingCommands = new List<Command>();

        /// <summary>Cached LayerMask for tile raycasts. Built once in Awake.</summary>
        private LayerMask _tileLayerMask;

        // -----------------------------------------------------------------------------------------
        // Public events (consumed by HUDManager and other presentation MonoBehaviours)
        // -----------------------------------------------------------------------------------------

        /// <summary>Fired when a friendly unit is selected. Carries the unit's runtime ID.</summary>
        public event Action<string> OnUnitSelected;

        /// <summary>Fired when a spell is staged for targeting. Carries the spell's definition ID.</summary>
        public event Action<string> OnSpellSelected;

        /// <summary>Fired when selection is cleared (unit deselected or action committed).</summary>
        public event Action OnSelectionCleared;

        /// <summary>Fired when this player's plan has been submitted and the turn is waiting for the opponent.</summary>
        public event Action OnPlanSubmitted;

        // -----------------------------------------------------------------------------------------
        // Unity lifecycle
        // -----------------------------------------------------------------------------------------

        private void Awake()
        {
            _tileLayerMask = LayerMask.GetMask(TileLayerName);

            SimulationEventBus.Subscribe<TurnResolvedEvent>(OnTurnResolved);
            SimulationEventBus.Subscribe<MatchEndedEvent>(OnMatchEnded);
        }

        private void OnDestroy()
        {
            SimulationEventBus.Unsubscribe<TurnResolvedEvent>(OnTurnResolved);
            SimulationEventBus.Unsubscribe<MatchEndedEvent>(OnMatchEnded);
        }

        private void Update()
        {
            if (_inputPhase == InputPhase.WaitingForTurn)
                return;

            if (Input.GetMouseButtonDown(0))
                HandleMouseClick();
        }

        // -----------------------------------------------------------------------------------------
        // Mouse input entry point
        // -----------------------------------------------------------------------------------------

        private void HandleMouseClick()
        {
            GridPosition? clickedTile = RaycastTilePosition();
            if (!clickedTile.HasValue)
                return;

            switch (_inputPhase)
            {
                case InputPhase.Idle:
                case InputPhase.UnitSelected:
                    HandleTileClick(clickedTile.Value);
                    break;

                case InputPhase.SpellTargeting:
                    HandleSpellTargetClick(clickedTile.Value);
                    break;
            }
        }

        // -----------------------------------------------------------------------------------------
        // Click handlers (one per meaningful input phase)
        // -----------------------------------------------------------------------------------------

        private void HandleTileClick(GridPosition tilePos)
        {
            SimulationState state = _sim.State;
            UnitState unitAtTile = GetUnitAt(state, tilePos);

            if (unitAtTile != null && unitAtTile.OwnerId == _controlledPlayerId && unitAtTile.IsAlive)
            {
                // Friendly unit clicked — select it.
                SelectUnit(unitAtTile.Id);
            }
            else if (_inputPhase == InputPhase.UnitSelected && _reachableTiles.Contains(tilePos))
            {
                // Valid move destination clicked — stage a move command.
                StageMoveCommand(tilePos);
            }
            else
            {
                // Clicked empty or enemy tile with no relevant action — clear selection.
                ClearSelection();
            }
        }

        private void HandleSpellTargetClick(GridPosition tilePos)
        {
            if (IsValidSpellTarget(tilePos))
            {
                StageSpellCommand(tilePos);
            }
            // Invalid target: stay in SpellTargeting phase so the player can re-aim.
        }

        // -----------------------------------------------------------------------------------------
        // Public API — called by HUDManager spell buttons and action panel
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Selects the specified unit, caching its reachable tiles and transitioning to UnitSelected.
        /// Safe to call from HUDManager or external systems; no-ops if the unit is not found.
        /// </summary>
        public void SelectUnit(string unitId)
        {
            SimulationState state = _sim.State;
            UnitState unit = state.GetUnit(unitId);
            if (unit == null || !unit.IsAlive)
                return;

            _selectedUnitId = unitId;
            _selectedSpellId = null;
            _inputPhase = InputPhase.UnitSelected;

            // Pre-compute reachable tiles for move-target highlighting.
            _reachableTiles = MovementRange.GetReachableTiles(state.Grid, unit.Position, unit.MoveRange);

            OnUnitSelected?.Invoke(unitId);
        }

        /// <summary>
        /// Stages the specified spell for targeting, transitioning to SpellTargeting.
        /// Called by HUDManager when the player clicks a spell button on the action panel.
        /// A unit must already be selected; no-ops if none is selected.
        /// </summary>
        public void SelectSpell(string spellId)
        {
            if (_selectedUnitId == null)
                return;

            _selectedSpellId = spellId;
            _inputPhase = InputPhase.SpellTargeting;

            OnSpellSelected?.Invoke(spellId);
        }

        /// <summary>
        /// Clears the current unit and spell selection, returning to Idle (or WaitingForTurn
        /// if it is not currently this player's turn).
        /// </summary>
        public void ClearSelection()
        {
            _selectedUnitId = null;
            _selectedSpellId = null;
            _reachableTiles.Clear();

            _inputPhase = _inputPhase == InputPhase.WaitingForTurn
                ? InputPhase.WaitingForTurn
                : InputPhase.Idle;

            OnSelectionCleared?.Invoke();
        }

        /// <summary>
        /// Stages a ThermalComposureCommand for the currently selected unit and queues it
        /// into the pending plan. No-op if no unit is selected.
        /// </summary>
        public void RequestThermalComposure()
        {
            if (_selectedUnitId == null)
                return;

            var cmd = new ThermalComposureCommand(_selectedUnitId);
            _pendingCommands.Add(cmd);

            ClearSelection();
        }

        /// <summary>
        /// Submits the accumulated pending commands as this player's activation plan for the turn,
        /// then transitions to WaitingForTurn. An empty plan (pass) is legal per the rules.
        /// </summary>
        public void SubmitPlan()
        {
            _sim.TurnManager.SubmitPlan(_controlledPlayerId, _pendingCommands.ToArray());
            _pendingCommands.Clear();

            _inputPhase = InputPhase.WaitingForTurn;
            ClearSelection();

            OnPlanSubmitted?.Invoke();
        }

        // -----------------------------------------------------------------------------------------
        // Command staging (private — each builds exactly one Command and adds it to _pendingCommands)
        // -----------------------------------------------------------------------------------------

        private void StageMoveCommand(GridPosition destination)
        {
            SimulationState state = _sim.State;
            UnitState actor = state.GetUnit(_selectedUnitId);
            if (actor == null)
                return;

            var cmd = new MoveCommand(_selectedUnitId, actor.ActivationCost, destination);
            _pendingCommands.Add(cmd);

            ClearSelection();
        }

        private void StageSpellCommand(GridPosition targetTile)
        {
            SimulationState state = _sim.State;
            UnitState actor = state.GetUnit(_selectedUnitId);
            if (actor == null || _selectedSpellId == null)
                return;

            var cmd = new SpellCommand(_selectedUnitId, actor.ActivationCost, _selectedSpellId, targetTile);
            _pendingCommands.Add(cmd);

            ClearSelection();
        }

        // -----------------------------------------------------------------------------------------
        // Validation helpers
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the given tile is a valid target for the staged spell.
        /// Checks grid bounds and stub range (Manhattan distance ≤ 4).
        /// Wave 2 will expand this with LOS, per-spell range, and targetType constraints.
        /// </summary>
        private bool IsValidSpellTarget(GridPosition tilePos)
        {
            if (_selectedUnitId == null || _selectedSpellId == null)
                return false;

            SimulationState state = _sim.State;

            if (!state.Grid.IsInBounds(tilePos))
                return false;

            UnitState actor = state.GetUnit(_selectedUnitId);
            if (actor == null)
                return false;

            // Stub range check — must stay in sync with SpellCommand's internal StubSpellRange (= 4).
            // Wave 2 will replace this with a per-spell range lookup via SpellData.
            int distance = actor.Position.ManhattanDistance(tilePos);
            return distance <= StubSpellRange;
        }

        // -----------------------------------------------------------------------------------------
        // Raycast
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Casts a ray from the game camera through the mouse cursor and returns the grid position
        /// of the tile collider hit, or null if no tile was hit.
        /// </summary>
        private GridPosition? RaycastTilePosition()
        {
            Ray ray = _gameCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, RaycastMaxDistance, _tileLayerMask))
                return null;

            // The tile collider's GameObject is expected to carry a TileView component that
            // exposes its GridPosition. If the component is absent, fall back to rounding
            // world-space coordinates to integer tile indices.
            TileView tileView = hit.collider.GetComponent<TileView>();
            if (tileView != null)
                return tileView.GridPosition;

            // Fallback: derive tile coordinates from world position (assumes 1-unit tile spacing).
            Vector3 worldPos = hit.point;
            return new GridPosition(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.z));
        }

        // -----------------------------------------------------------------------------------------
        // SimulationState query helpers
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the first living unit whose Position matches tilePos, or null if the tile is empty.
        /// SimulationState does not expose GetUnitAt(GridPosition) directly, so we iterate living units.
        /// </summary>
        private static UnitState GetUnitAt(SimulationState state, GridPosition tilePos)
        {
            foreach (UnitState unit in state.GetLivingUnits())
            {
                if (unit.Position == tilePos)
                    return unit;
            }
            return null;
        }

        // -----------------------------------------------------------------------------------------
        // SimulationEventBus handlers
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Called after every turn resolves. Checks whether it is now this player's planning turn
        /// and transitions accordingly.
        ///
        /// In the simultaneous blind turn system both players plan at the same time, so on every
        /// TurnResolvedEvent this controller moves to Idle, clearing any stale selection state
        /// and pending commands from the previous turn.
        /// </summary>
        private void OnTurnResolved(TurnResolvedEvent e)
        {
            // Both players plan simultaneously — activate input for this player unconditionally.
            _pendingCommands.Clear();
            ClearSelection();
            _inputPhase = InputPhase.Idle;
        }

        /// <summary>
        /// Called when the match ends. Suppresses all further input.
        /// </summary>
        private void OnMatchEnded(MatchEndedEvent e)
        {
            _pendingCommands.Clear();
            ClearSelection();
            _inputPhase = InputPhase.WaitingForTurn;
        }
    }
}

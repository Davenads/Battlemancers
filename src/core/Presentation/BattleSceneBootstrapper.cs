using System;
using System.IO;
using UnityEngine;
using Battlemancers.Core.Data;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Unity;
using Battlemancers.UI.Battle;

namespace Battlemancers.Presentation
{
    /// <summary>
    /// Root MonoBehaviour that wires the full dependency graph at scene start.
    ///
    /// This is the ONLY class that performs scene wiring. All other classes receive their
    /// dependencies via constructor parameters or public property setters called from here.
    ///
    /// Attach this to a root GameObject in the BattleScene. Scene GameObjects (GridRenderer,
    /// UnitViewController, etc.) are assigned via [SerializeField] Inspector references.
    ///
    /// Execution order: Awake runs before any other MonoBehaviour in the scene.
    ///
    /// No FindObjectOfType calls anywhere in this class or its descendants.
    /// </summary>
    public class BattleSceneBootstrapper : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        private const string CrossroadsMapId = "crossroads";

        /// <summary>
        /// Fallback spawn position for Pyromancer (Player 0 / "p1") when the map
        /// provides no SpawnPoints. Matches the first valid player1 spawn zone tile.
        /// </summary>
        private static readonly GridPosition FallbackSpawnPlayer0 = new GridPosition(1, 1);

        /// <summary>
        /// Fallback spawn position for Hydromancer (Player 1 / "p2") when the map
        /// provides no SpawnPoints. Matches the first valid player2 spawn zone tile.
        /// </summary>
        private static readonly GridPosition FallbackSpawnPlayer1 = new GridPosition(8, 8);

        // ---------------------------------------------------------------------------
        // Inspector references — assign these in the Unity Inspector
        // ---------------------------------------------------------------------------

        /// <summary>
        /// The SimulationBootstrapper that owns TurnManager, SimulationState, and all
        /// pure C# simulation managers. Must be present in the scene and assigned here.
        /// </summary>
        [SerializeField] private SimulationBootstrapper _sim;

        /// <summary>
        /// The BattleSceneController that accepts submitted activation plans and drives
        /// resolution. Assigned via Inspector.
        /// </summary>
        [SerializeField] private BattleSceneController _battleController;

        /// <summary>
        /// The GridRenderer responsible for rendering tile states.
        /// Assigned via Inspector.
        /// </summary>
        [SerializeField] private GridRenderer _gridRenderer;

        /// <summary>
        /// The UnitViewController that manages unit GameObjects.
        /// Assigned via Inspector.
        /// </summary>
        [SerializeField] private UnitViewController _unitViewController;

        /// <summary>
        /// The SimulationEventDispatcher that bridges SimulationEventBus to Unity events.
        /// Assigned via Inspector.
        /// </summary>
        [SerializeField] private SimulationEventDispatcher _eventDispatcher;

        /// <summary>
        /// The PlanningPhaseUI that shows the activation list for each player.
        /// Assigned via Inspector.
        /// </summary>
        [SerializeField] private PlanningPhaseUI _planningPhaseUI;

        /// <summary>
        /// The SpellButtonPanel that renders spell buttons for the active caster.
        /// Assigned via Inspector.
        /// </summary>
        [SerializeField] private SpellButtonPanel _spellButtonPanel;

        /// <summary>
        /// The HotseatOrchestrator that manages turn ownership between Player 0 and Player 1.
        /// Assigned via Inspector.
        /// </summary>
        [SerializeField] private HotseatOrchestrator _hotseatOrchestrator;

        /// <summary>
        /// The SpellSelectionUI that handles spell targeting flow.
        /// Assigned via Inspector.
        /// </summary>
        [SerializeField] private SpellSelectionUI _spellSelectionUI;

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Start()
        {
            WireEventDispatcherToPresentation();
            WireHotseatOrchestrator();
            WireSpellSelectionUI();

            Debug.Log("[BattleSceneBootstrapper] Scene wiring complete. HotseatOrchestrator starting.");
            _hotseatOrchestrator?.StartPlanningPhase();
        }

        // ---------------------------------------------------------------------------
        // Wiring helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Subscribes SimulationEventDispatcher's typed Unity events to the appropriate
        /// presentation handlers (UnitViewController and GridRenderer). This is the only
        /// place where event subscriptions between the dispatcher and the view layer occur.
        /// </summary>
        private void WireEventDispatcherToPresentation()
        {
            if (_eventDispatcher == null)
            {
                Debug.LogWarning("[BattleSceneBootstrapper] SimulationEventDispatcher is not assigned. Event-driven visuals will not update.");
                return;
            }

            if (_unitViewController != null)
            {
                _eventDispatcher.UnitMoved  += _unitViewController.MoveUnit;
                _eventDispatcher.UnitDied   += _unitViewController.RemoveUnit;
                _eventDispatcher.StatusApplied += _unitViewController.ApplyStatusVisual;
            }

            if (_gridRenderer != null)
            {
                _eventDispatcher.TileStateChanged += _gridRenderer.RefreshTile;
            }

            if (_battleController != null)
            {
                _eventDispatcher.MatchEnded += OnMatchEnded;
            }

            Debug.Log("[BattleSceneBootstrapper] SimulationEventDispatcher wired to presentation layer.");
        }

        /// <summary>
        /// Injects dependencies into HotseatOrchestrator via its SetDependencies method.
        /// The orchestrator is a MonoBehaviour but receives all references explicitly from
        /// this bootstrapper — no self-lookup.
        /// </summary>
        private void WireHotseatOrchestrator()
        {
            if (_hotseatOrchestrator == null)
            {
                Debug.LogWarning("[BattleSceneBootstrapper] HotseatOrchestrator is not assigned.");
                return;
            }

            _hotseatOrchestrator.SetDependencies(_battleController, _planningPhaseUI, _sim);
        }

        /// <summary>
        /// Injects dependencies into SpellSelectionUI via its SetDependencies method.
        /// </summary>
        private void WireSpellSelectionUI()
        {
            if (_spellSelectionUI == null)
            {
                Debug.LogWarning("[BattleSceneBootstrapper] SpellSelectionUI is not assigned.");
                return;
            }

            _spellSelectionUI.SetDependencies(_spellButtonPanel, _gridRenderer, _sim);
        }

        // ---------------------------------------------------------------------------
        // Event handlers wired from dispatcher
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Called via SimulationEventDispatcher.MatchEnded when the simulation signals
        /// that the match is over. Logs the result; further handling (show UI, etc.) is
        /// the responsibility of HotseatOrchestrator.OnRoundComplete.
        /// </summary>
        private void OnMatchEnded(string winnerId, Core.Simulation.Events.MatchEndReason reason)
        {
            string outcome = winnerId != null
                ? $"Winner: {winnerId}"
                : "Draw";
            Debug.Log($"[BattleSceneBootstrapper] Match ended — {outcome} ({reason}).");
        }

        // ---------------------------------------------------------------------------
        // OnDestroy — unsubscribe to prevent stale delegate errors
        // ---------------------------------------------------------------------------

        private void OnDestroy()
        {
            if (_eventDispatcher == null) return;

            if (_unitViewController != null)
            {
                _eventDispatcher.UnitMoved    -= _unitViewController.MoveUnit;
                _eventDispatcher.UnitDied     -= _unitViewController.RemoveUnit;
                _eventDispatcher.StatusApplied -= _unitViewController.ApplyStatusVisual;
            }

            if (_gridRenderer != null)
            {
                _eventDispatcher.TileStateChanged -= _gridRenderer.RefreshTile;
            }

            _eventDispatcher.MatchEnded -= OnMatchEnded;
        }
    }
}

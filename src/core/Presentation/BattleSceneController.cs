using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Commands;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Core.Warband;
using Battlemancers.Unity;

namespace Battlemancers.Presentation
{
    /// <summary>
    /// Represents a single unit activation choice within a player's turn plan.
    /// Carries the unit ID to activate and the commands that unit will execute.
    /// </summary>
    public class PlannedActivation
    {
        /// <summary>Runtime ID of the unit being activated.</summary>
        public string UnitId { get; }

        /// <summary>The commands this unit will execute when activated.</summary>
        public Command[] Commands { get; }

        /// <summary>Activation budget cost charged for including this unit in the plan.</summary>
        public int ActivationCost { get; }

        /// <summary>
        /// Initializes a PlannedActivation for a unit with the given commands.
        /// </summary>
        /// <param name="unitId">Runtime ID of the unit to activate.</param>
        /// <param name="commands">Commands the unit will execute. May be empty.</param>
        /// <param name="activationCost">Budget cost charged for this unit's activation.</param>
        public PlannedActivation(string unitId, Command[] commands, int activationCost)
        {
            UnitId = unitId ?? throw new ArgumentNullException(nameof(unitId));
            Commands = commands ?? Array.Empty<Command>();
            ActivationCost = activationCost;
        }
    }

    /// <summary>
    /// The broad phase of the battle scene, used by UI and other MonoBehaviours
    /// to determine what controls to display.
    /// </summary>
    public enum BattlePhase
    {
        /// <summary>Both players are building activation plans.</summary>
        WaitingForPlans,

        /// <summary>Both plans are submitted; awaiting resolution.</summary>
        PlansLocked,

        /// <summary>Turn is actively being resolved and animated.</summary>
        Resolving,

        /// <summary>Resolution complete; round summary is shown.</summary>
        RoundComplete
    }

    /// <summary>
    /// Top-level MonoBehaviour orchestrator for the battle scene.
    ///
    /// Owns the TurnManager lifecycle, tracks BattlePhase, validates activation budget,
    /// and drives event dispatch to GridRenderer and UnitViewController via a coroutine.
    ///
    /// Does NOT manipulate any GameObject directly — all visual work is delegated to
    /// GridRenderer, UnitViewController, and SimulationEventDispatcher via events.
    ///
    /// Dependencies injected via Inspector [SerializeField] references.
    /// </summary>
    public class BattleSceneController : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        private const int ActivationBudgetLimit = ActivationPlanValidator.ActivationBudget;

        /// <summary>
        /// Delay in seconds between processing each SimulationEvent during resolution,
        /// allowing other MonoBehaviours time to animate before the next event arrives.
        /// </summary>
        private const float EventProcessingInterval = 0.05f;

        // ---------------------------------------------------------------------------
        // Inspector references
        // ---------------------------------------------------------------------------

        /// <summary>
        /// The SimulationBootstrapper that owns TurnManager and SimulationState.
        /// Assigned in the Inspector.
        /// </summary>
        [SerializeField] private SimulationBootstrapper _sim;

        /// <summary>
        /// The GridRenderer responsible for rendering tile states.
        /// Notified after each resolved turn to refresh visuals.
        /// </summary>
        [SerializeField] private GridRenderer _gridRenderer;

        /// <summary>
        /// The UnitViewController responsible for unit GameObjects.
        /// Notified of unit events (move, death, status) after resolution.
        /// </summary>
        [SerializeField] private UnitViewController _unitViewController;

        // ---------------------------------------------------------------------------
        // Public state
        // ---------------------------------------------------------------------------

        /// <summary>The current turn number, sourced from SimulationState after each resolve.</summary>
        public int CurrentRound { get; private set; }

        /// <summary>The current broad phase of the battle scene.</summary>
        public BattlePhase Phase { get; private set; }

        // ---------------------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------------------

        // Tracks which players have submitted plans this round.
        // Maps playerId → flattened Command[] derived from their PlannedActivations.
        private readonly Dictionary<string, Command[]> _submittedPlans = new Dictionary<string, Command[]>();

        // Events produced by the most recent ResolveTurn call, awaiting animation.
        private SimulationEvent[] _pendingEvents;

        // ---------------------------------------------------------------------------
        // C# events — subscribed to by PlanningPhaseUI and other MonoBehaviours
        // ---------------------------------------------------------------------------

        /// <summary>Raised when resolution completes and the next planning phase begins.</summary>
        public event Action<int> RoundCompleted;

        /// <summary>Raised when a plan submission is rejected (e.g. over budget).</summary>
        public event Action<int, string> PlanRejected;

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Start()
        {
            Phase = BattlePhase.WaitingForPlans;
            CurrentRound = _sim != null ? _sim.State.TurnNumber : 1;
        }

        // ---------------------------------------------------------------------------
        // Plan submission
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Submits a player's activation plan for the current turn.
        ///
        /// Validates the total activation cost does not exceed 100 pts before forwarding
        /// to TurnManager. If validation fails, raises <see cref="PlanRejected"/> and
        /// returns without modifying state.
        ///
        /// When both players have submitted valid plans, transitions to
        /// <see cref="BattlePhase.PlansLocked"/> and starts resolution automatically.
        /// </summary>
        /// <param name="playerId">The player submitting the plan (e.g. "p1" or "p2").</param>
        /// <param name="plan">The list of planned unit activations for this turn.</param>
        public void SubmitPlan(int playerId, List<PlannedActivation> plan)
        {
            SubmitPlanByStringId(playerId.ToString(), plan);
        }

        /// <summary>
        /// Overload that accepts the player ID as a string, matching the simulation's convention.
        /// </summary>
        /// <param name="playerId">The player ID string (e.g. "p1").</param>
        /// <param name="plan">The list of planned unit activations for this turn.</param>
        public void SubmitPlanByStringId(string playerId, List<PlannedActivation> plan)
        {
            if (_sim == null)
            {
                Debug.LogError("[BattleSceneController] SimulationBootstrapper is not assigned.");
                return;
            }

            if (Phase != BattlePhase.WaitingForPlans)
            {
                Debug.LogWarning($"[BattleSceneController] Plan rejected for '{playerId}': not in WaitingForPlans phase (current: {Phase}).");
                PlanRejected?.Invoke(0, $"Cannot submit plan during {Phase} phase.");
                return;
            }

            // Validate budget.
            int totalCost = 0;
            var allCommands = new List<Command>();
            if (plan != null)
            {
                foreach (PlannedActivation activation in plan)
                {
                    totalCost += activation.ActivationCost;
                    allCommands.AddRange(activation.Commands);
                }
            }

            if (totalCost > ActivationBudgetLimit)
            {
                string msg = $"Plan for '{playerId}' costs {totalCost} pts, exceeding the {ActivationBudgetLimit}-pt budget.";
                Debug.LogWarning($"[BattleSceneController] {msg}");
                PlanRejected?.Invoke(totalCost, msg);
                return;
            }

            // Forward to TurnManager.
            try
            {
                _sim.TurnManager.SubmitPlan(playerId, allCommands.ToArray());
                _submittedPlans[playerId] = allCommands.ToArray();
                Debug.Log($"[BattleSceneController] Plan accepted for '{playerId}' — {totalCost} pts, {allCommands.Count} command(s).");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BattleSceneController] TurnManager.SubmitPlan failed for '{playerId}': {ex.Message}");
                PlanRejected?.Invoke(totalCost, ex.Message);
                return;
            }

            // Check if all players have submitted.
            if (_sim.TurnManager.AllPlansSubmitted())
            {
                Phase = BattlePhase.PlansLocked;
                Debug.Log("[BattleSceneController] All plans submitted. Starting resolution.");
                StartCoroutine(ResolvePhaseCoroutine());
            }
        }

        // ---------------------------------------------------------------------------
        // Resolution coroutine
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Drives the resolution phase as a Unity coroutine.
        ///
        /// Calls TurnManager.ResolveTurn() on the first frame, then processes each
        /// returned SimulationEvent with a small yield between them so that subscribers
        /// (animators, VFX, audio) have time to react frame-by-frame.
        ///
        /// On completion: transitions to RoundComplete and raises <see cref="RoundCompleted"/>.
        /// </summary>
        private IEnumerator ResolvePhaseCoroutine()
        {
            Phase = BattlePhase.Resolving;

            SimulationEvent[] events = null;
            try
            {
                events = _sim.TurnManager.ResolveTurn();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BattleSceneController] ResolveTurn failed: {ex.Message}");
                Phase = BattlePhase.WaitingForPlans;
                yield break;
            }

            _pendingEvents = events;

            // Dispatch each event with a frame gap so Unity-side subscribers can animate.
            if (events != null)
            {
                foreach (SimulationEvent simEvent in events)
                {
                    DispatchEventToPresentation(simEvent);
                    yield return new WaitForSeconds(EventProcessingInterval);
                }
            }

            // Refresh all tile visuals once after resolution (catches any tile changes not
            // individually dispatched during the event stream).
            _gridRenderer?.RefreshAll(_sim.State);

            _submittedPlans.Clear();
            CurrentRound = _sim.State.TurnNumber;
            Phase = BattlePhase.RoundComplete;

            Debug.Log($"[BattleSceneController] Round complete. New turn number: {CurrentRound}.");
            RoundCompleted?.Invoke(CurrentRound);
        }

        // ---------------------------------------------------------------------------
        // Event dispatch
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Routes a single SimulationEvent to the appropriate presentation handler.
        /// GridRenderer and UnitViewController are called directly here; other systems
        /// subscribe to SimulationEventBus directly.
        /// </summary>
        private void DispatchEventToPresentation(SimulationEvent simEvent)
        {
            switch (simEvent)
            {
                case UnitMovedEvent moved:
                    _unitViewController?.MoveUnit(moved.UnitId,
                        new Vector2Int(moved.To.X, moved.To.Y));
                    break;

                case UnitDiedEvent died:
                    _unitViewController?.RemoveUnit(died.UnitId);
                    break;

                case UnitStatusAppliedEvent statusApplied:
                    if (Enum.TryParse(statusApplied.StatusType,
                            out Battlemancers.Simulation.Status.StatusType parsedStatus))
                    {
                        _unitViewController?.ApplyStatusVisual(statusApplied.UnitId, parsedStatus);
                    }
                    break;

                case TileStateChangedEvent tileChanged:
                    _gridRenderer?.RefreshTile(
                        new Vector2Int(tileChanged.Position.X, tileChanged.Position.Y),
                        tileChanged.NewState);
                    break;

                case MatchEndedEvent matchEnded:
                    Debug.Log($"[BattleSceneController] Match ended — Winner: {matchEnded.WinnerId ?? "draw"}, Reason: {matchEnded.Reason}.");
                    break;

                case TurnResolvedEvent turnResolved:
                    Debug.Log($"[BattleSceneController] Turn {turnResolved.TurnNumber} resolved — {turnResolved.TotalActionsResolved} action(s).");
                    break;
            }
        }

        // ---------------------------------------------------------------------------
        // Round reset
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Resets the battle state for a new planning phase.
        /// Called by PlanningPhaseUI after the player clicks "Next Turn".
        /// Transitions from RoundComplete back to WaitingForPlans.
        /// </summary>
        public void BeginNextRound()
        {
            if (Phase != BattlePhase.RoundComplete)
            {
                Debug.LogWarning($"[BattleSceneController] BeginNextRound called in unexpected phase: {Phase}.");
                return;
            }

            _submittedPlans.Clear();
            Phase = BattlePhase.WaitingForPlans;
            Debug.Log($"[BattleSceneController] Planning phase started for round {CurrentRound}.");
        }
    }
}

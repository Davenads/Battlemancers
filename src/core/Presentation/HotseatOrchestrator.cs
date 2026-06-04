using System;
using System.Collections.Generic;
using UnityEngine;
using Battlemancers.Core.Simulation;
using Battlemancers.Unity;
using Battlemancers.UI.Battle;

namespace Battlemancers.Presentation
{
    /// <summary>
    /// Manages the hotseat planning loop between Player 0 ("p1") and Player 1 ("p2").
    ///
    /// Responsibilities:
    ///   - Tracks which player is currently in the planning seat (CurrentPlayer).
    ///   - Calls StartPlanningPhase() to show the correct player's UI via PlanningPhaseUI.
    ///   - Receives OnPlayerLocked() when a player submits their plan.
    ///   - When both players have locked, delegates resolution to BattleSceneController.
    ///   - After resolution, checks win condition and fires MatchOver when the game ends.
    ///   - Otherwise swaps CurrentPlayer and starts the next planning phase.
    ///
    /// All dependencies are injected by BattleSceneBootstrapper via SetDependencies().
    /// No FindObjectOfType. No static access. No singletons.
    ///
    /// SpellSelectionUI and this class must not import UnityEngine.UI or TMPro directly.
    /// This class only depends on BattleSceneController and the simulation layer.
    /// </summary>
    public class HotseatOrchestrator : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        private const int PlayerCount  = 2;
        private const int Player0Index = 0;
        private const int Player1Index = 1;

        private const string Player0Id = "p1";
        private const string Player1Id = "p2";

        // ---------------------------------------------------------------------------
        // Public state
        // ---------------------------------------------------------------------------

        /// <summary>Index of the player currently in the planning seat (0 or 1).</summary>
        public int CurrentPlayer { get; private set; }

        // ---------------------------------------------------------------------------
        // Events
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Raised when the match ends — either a player has no living Mancers or the
        /// turn limit is reached. The string argument is the winning player ID, or null for a draw.
        /// </summary>
        public event Action<string> MatchOver;

        // ---------------------------------------------------------------------------
        // Injected dependencies (set by BattleSceneBootstrapper)
        // ---------------------------------------------------------------------------

        private BattleSceneController _battleController;
        private PlanningPhaseUI       _planningPhaseUI;
        private SimulationBootstrapper _sim;

        // ---------------------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------------------

        // Holds plans indexed by player index (0 or 1). Non-null once a player has locked.
        private readonly List<PlannedActivation>[] _lockedPlans = new List<PlannedActivation>[PlayerCount];

        private bool _matchOver;

        // ---------------------------------------------------------------------------
        // Dependency injection — called by BattleSceneBootstrapper
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Injects all dependencies. Must be called by BattleSceneBootstrapper before
        /// StartPlanningPhase() is invoked.
        /// </summary>
        public void SetDependencies(
            BattleSceneController battleController,
            PlanningPhaseUI planningPhaseUI,
            SimulationBootstrapper sim)
        {
            _battleController = battleController;
            _planningPhaseUI  = planningPhaseUI;
            _sim              = sim;

            if (_battleController != null)
            {
                _battleController.RoundCompleted += OnRoundResolved;
            }
        }

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void OnDestroy()
        {
            if (_battleController != null)
            {
                _battleController.RoundCompleted -= OnRoundResolved;
            }
        }

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Starts the planning phase for the current player.
        /// Shows the PlanningPhaseUI for CurrentPlayer. Call this once at scene start
        /// (via BattleSceneBootstrapper) and again after each round resolves.
        /// </summary>
        public void StartPlanningPhase()
        {
            if (_matchOver)
                return;

            // Reset locked plans from the previous round.
            _lockedPlans[Player0Index] = null;
            _lockedPlans[Player1Index] = null;

            CurrentPlayer = Player0Index;

            Debug.Log($"[HotseatOrchestrator] Planning phase started. Player {CurrentPlayer + 1} plans first.");

            // PlanningPhaseUI internally handles the hotseat swap (Player 1 → Player 2)
            // via its OnLockPlanClicked flow. We call BeginNextRound to put the controller
            // in WaitingForPlans state when starting a fresh round (Start() already does this).
            // On first call the controller is already in WaitingForPlans from Start().
        }

        /// <summary>
        /// Called when a player has locked in their activation plan.
        ///
        /// When both players have locked, submits both plans to BattleSceneController
        /// and triggers resolution. The PlanningPhaseUI handles the in-UI handoff between
        /// Player 1 and Player 2; this method handles the cross-layer lock tracking.
        /// </summary>
        /// <param name="playerIndex">0-based index of the player locking (0 or 1).</param>
        /// <param name="plan">The list of PlannedActivations the player chose.</param>
        public void OnPlayerLocked(int playerIndex, List<PlannedActivation> plan)
        {
            if (playerIndex < 0 || playerIndex >= PlayerCount)
            {
                Debug.LogError($"[HotseatOrchestrator] Invalid playerIndex: {playerIndex}.");
                return;
            }

            _lockedPlans[playerIndex] = plan ?? new List<PlannedActivation>();
            Debug.Log($"[HotseatOrchestrator] Player {playerIndex + 1} locked plan ({_lockedPlans[playerIndex].Count} activation(s)).");

            if (BothPlayersLocked())
                OnBothPlayersLocked();
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns true when both player slots have a non-null locked plan.
        /// </summary>
        private bool BothPlayersLocked()
        {
            return _lockedPlans[Player0Index] != null && _lockedPlans[Player1Index] != null;
        }

        /// <summary>
        /// Called when both players have submitted their plans. Submits both to
        /// BattleSceneController, which triggers resolution automatically once both are in.
        /// </summary>
        private void OnBothPlayersLocked()
        {
            Debug.Log("[HotseatOrchestrator] Both players locked. Submitting plans to BattleSceneController.");

            _battleController?.SubmitPlan(Player0Index, _lockedPlans[Player0Index]);
            _battleController?.SubmitPlan(Player1Index, _lockedPlans[Player1Index]);
        }

        /// <summary>
        /// Handles BattleSceneController.RoundCompleted. Checks win condition via the
        /// simulation state directly and either fires MatchOver or starts the next planning phase.
        /// </summary>
        private void OnRoundResolved(int newRound)
        {
            Debug.Log($"[HotseatOrchestrator] Round {newRound - 1} resolved. Checking win condition.");

            if (_sim != null && _sim.TurnManager != null)
            {
                bool ended = _sim.TurnManager.CheckWinCondition(out string winnerId);
                if (ended)
                {
                    OnRoundComplete(winnerId);
                    return;
                }
            }

            // Match continues — the PlanningPhaseUI already shows the "Next Turn" button via
            // its own RoundCompleted subscription. After the player clicks it, BeginNextRound
            // is called on BattleSceneController and BeginPlayerTurn(1) resets the UI.
            // HotseatOrchestrator resets plan slots so the next StartPlanningPhase is clean.
            _lockedPlans[Player0Index] = null;
            _lockedPlans[Player1Index] = null;
        }

        /// <summary>
        /// Fires the MatchOver event and logs the result. Called when the win condition check
        /// determines the game has ended.
        /// </summary>
        /// <param name="winnerId">
        /// The winning player ID (e.g., "p1"), or null for a draw.
        /// </param>
        public void OnRoundComplete(string winnerId)
        {
            _matchOver = true;

            string outcome = winnerId != null
                ? $"Winner: {winnerId}"
                : "Draw";

            Debug.Log($"[HotseatOrchestrator] Match over — {outcome}.");
            MatchOver?.Invoke(winnerId);
        }
    }
}

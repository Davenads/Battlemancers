using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Commands;
using Battlemancers.Presentation;

namespace Battlemancers.UI.Battle
{
    /// <summary>
    /// MonoBehaviour that drives the hotseat Planning Phase UI for both players.
    ///
    /// Responsibilities:
    ///   - Displays the current player's activation budget remaining (starts at 100 pts/turn).
    ///   - Lists available units for the current player with their point costs.
    ///   - Allows clicking a unit to assign it to this turn's activation plan.
    ///   - "Lock Plan" button: validates budget, calls BattleSceneController.SubmitPlanByStringId().
    ///   - "Clear" button: resets the current player's plan without submitting.
    ///   - After Player 1 locks: shows "Waiting for Player 2..." then switches to Player 2 UI.
    ///   - After both players lock: hides planning UI and shows "Resolving..." overlay.
    ///   - After resolution: shows "Next Turn" button that resets both plans.
    ///
    /// All text uses TMPro.TMP_Text. All buttons use UnityEngine.UI.Button.
    /// Dependencies are injected via [SerializeField] Inspector references.
    /// No singletons. No FindObjectOfType.
    /// </summary>
    public class PlanningPhaseUI : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        private const int TotalActivationBudget = 100;
        private const string Player1Id = "p1";
        private const string Player2Id = "p2";

        // ---------------------------------------------------------------------------
        // Inspector references
        // ---------------------------------------------------------------------------

        /// <summary>
        /// The battle scene controller that accepts submitted plans and drives resolution.
        /// Assigned in the Inspector.
        /// </summary>
        [SerializeField] private BattleSceneController _controller;

        /// <summary>
        /// The SimulationBootstrapper that owns the SimulationState (unit roster, costs).
        /// Assigned in the Inspector.
        /// </summary>
        [SerializeField] private SimulationBootstrapper _sim;

        /// <summary>Label showing the remaining budget — format: "Budget: 47 / 100 pts".</summary>
        [SerializeField] private TMP_Text _budgetLabel;

        /// <summary>Label showing whose turn it is — format: "Player 1 — Planning".</summary>
        [SerializeField] private TMP_Text _statusLabel;

        /// <summary>Confirms and submits the current player's plan.</summary>
        [SerializeField] private Button _lockPlanButton;

        /// <summary>Clears the current player's plan and resets budget display.</summary>
        [SerializeField] private Button _clearButton;

        /// <summary>Parent transform for the dynamically generated unit entry buttons.</summary>
        [SerializeField] private Transform _unitListContainer;

        /// <summary>
        /// Prefab for a single unit entry row. Must contain at least a TMP_Text (unit name/cost)
        /// and a Button component. Assigned in the Inspector.
        /// </summary>
        [SerializeField] private GameObject _unitEntryPrefab;

        /// <summary>Overlay panel shown while resolution is in progress.</summary>
        [SerializeField] private GameObject _resolvingOverlay;

        /// <summary>Button that starts the next round, shown after resolution completes.</summary>
        [SerializeField] private Button _nextTurnButton;

        // ---------------------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------------------

        // Which player is currently in the planning seat (1 or 2).
        private int _currentPlayer = 1;

        // Remaining activation budget for the player currently planning.
        private int _budget = TotalActivationBudget;

        // The plan being built by the current player.
        private readonly List<PlannedActivation> _currentPlan = new List<PlannedActivation>();

        // Instantiated unit entry rows, kept so we can destroy them when rebuilding the list.
        private readonly List<GameObject> _unitEntryInstances = new List<GameObject>();

        // Player 1's locked plan (held until Player 2 finishes, then both are submitted together).
        private List<PlannedActivation> _player1LockedPlan;

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            if (_lockPlanButton != null)
                _lockPlanButton.onClick.AddListener(OnLockPlanClicked);

            if (_clearButton != null)
                _clearButton.onClick.AddListener(OnClearClicked);

            if (_nextTurnButton != null)
                _nextTurnButton.onClick.AddListener(OnNextTurnClicked);

            if (_controller != null)
            {
                _controller.RoundCompleted += OnRoundCompleted;
                _controller.PlanRejected   += OnPlanRejected;
            }
        }

        private void Start()
        {
            ShowResolvingOverlay(false);
            ShowNextTurnButton(false);
            BeginPlayerTurn(_currentPlayer);
        }

        private void OnDestroy()
        {
            if (_lockPlanButton != null)
                _lockPlanButton.onClick.RemoveListener(OnLockPlanClicked);

            if (_clearButton != null)
                _clearButton.onClick.RemoveListener(OnClearClicked);

            if (_nextTurnButton != null)
                _nextTurnButton.onClick.RemoveListener(OnNextTurnClicked);

            if (_controller != null)
            {
                _controller.RoundCompleted -= OnRoundCompleted;
                _controller.PlanRejected   -= OnPlanRejected;
            }
        }

        // ---------------------------------------------------------------------------
        // Planning phase flow
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Resets the UI for the given player's planning turn.
        /// Rebuilds the unit list and resets the budget display.
        /// </summary>
        private void BeginPlayerTurn(int player)
        {
            _currentPlayer = player;
            _budget = TotalActivationBudget;
            _currentPlan.Clear();

            string playerId = player == 1 ? Player1Id : Player2Id;
            UpdateBudgetLabel();
            UpdateStatusLabel($"Player {player} — Planning");

            RebuildUnitList(playerId);

            SetPlanningControlsInteractable(true);
        }

        /// <summary>
        /// Destroys any existing unit entry rows and instantiates new ones for the given player.
        /// Each entry shows the unit's ID and point cost, with a button to add it to the plan.
        /// </summary>
        private void RebuildUnitList(string playerId)
        {
            // Destroy old entries.
            foreach (GameObject entry in _unitEntryInstances)
            {
                if (entry != null) Destroy(entry);
            }
            _unitEntryInstances.Clear();

            if (_sim == null || _unitListContainer == null || _unitEntryPrefab == null)
                return;

            foreach (UnitState unit in _sim.State.GetUnitsByOwner(playerId))
            {
                if (!unit.IsAlive) continue;

                GameObject entryGo = Instantiate(_unitEntryPrefab, _unitListContainer);
                _unitEntryInstances.Add(entryGo);

                TMP_Text label = entryGo.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.text = $"{unit.Id} ({unit.ActivationCost} pts)";

                Button btn = entryGo.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    // Capture loop variable for the closure.
                    UnitState capturedUnit = unit;
                    btn.onClick.AddListener(() => OnUnitEntryClicked(capturedUnit));
                }
            }
        }

        // ---------------------------------------------------------------------------
        // Button handlers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Called when the player clicks a unit entry to add it to their activation plan.
        /// Deducts the unit's activation cost from the budget display.
        /// Rejects the selection if the unit is already in the plan or the budget would be exceeded.
        /// </summary>
        private void OnUnitEntryClicked(UnitState unit)
        {
            // Prevent double-adding the same unit.
            foreach (PlannedActivation existing in _currentPlan)
            {
                if (existing.UnitId == unit.Id)
                {
                    Debug.LogWarning($"[PlanningPhaseUI] Unit '{unit.Id}' is already in the plan.");
                    return;
                }
            }

            int cost = unit.ActivationCost;
            if (_budget - cost < 0)
            {
                UpdateStatusLabel($"Not enough budget for {unit.Id} ({cost} pts).");
                return;
            }

            // Add to plan with an empty command set — commands are constructed by the input
            // layer (PlayerInputController) in a future iteration. Placeholder empty array.
            _currentPlan.Add(new PlannedActivation(unit.Id, System.Array.Empty<Command>(), cost));
            _budget -= cost;

            UpdateBudgetLabel();
            UpdateStatusLabel($"Player {_currentPlayer} — Planning ({_currentPlan.Count} unit(s) selected)");
        }

        /// <summary>
        /// Validates the plan and submits it to BattleSceneController.
        /// For Player 1: locks the plan locally and switches UI to Player 2.
        /// For Player 2: submits both plans and shows the resolving overlay.
        /// </summary>
        private void OnLockPlanClicked()
        {
            if (_currentPlayer == 1)
            {
                // Hold Player 1's plan until Player 2 is done.
                _player1LockedPlan = new List<PlannedActivation>(_currentPlan);
                UpdateStatusLabel("Player 1 plan locked. Waiting for Player 2...");
                SetPlanningControlsInteractable(false);

                // Briefly show the "waiting" message, then switch to Player 2.
                BeginPlayerTurn(2);
            }
            else
            {
                // Both players have planned — submit to the controller.
                List<PlannedActivation> player2Plan = new List<PlannedActivation>(_currentPlan);

                SetPlanningControlsInteractable(false);
                ShowResolvingOverlay(true);
                UpdateStatusLabel("Resolving...");

                // Submit Player 1's plan first, then Player 2's.
                // BattleSceneController starts resolution when both are in.
                _controller?.SubmitPlanByStringId(Player1Id, _player1LockedPlan ?? new List<PlannedActivation>());
                _controller?.SubmitPlanByStringId(Player2Id, player2Plan);
            }
        }

        /// <summary>
        /// Resets the current player's plan without submitting.
        /// </summary>
        private void OnClearClicked()
        {
            _currentPlan.Clear();
            _budget = TotalActivationBudget;
            UpdateBudgetLabel();
            UpdateStatusLabel($"Player {_currentPlayer} — Planning (cleared)");
        }

        /// <summary>
        /// Starts the next planning round after resolution completes.
        /// </summary>
        private void OnNextTurnClicked()
        {
            ShowNextTurnButton(false);
            ShowResolvingOverlay(false);
            _player1LockedPlan = null;
            _controller?.BeginNextRound();
            BeginPlayerTurn(1);
        }

        // ---------------------------------------------------------------------------
        // BattleSceneController event handlers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Called by BattleSceneController when a round finishes resolving.
        /// Hides the resolving overlay and shows the "Next Turn" button.
        /// </summary>
        private void OnRoundCompleted(int newRound)
        {
            ShowResolvingOverlay(false);
            ShowNextTurnButton(true);
            UpdateStatusLabel($"Round {newRound - 1} complete. Press Next Turn to continue.");
        }

        /// <summary>
        /// Called by BattleSceneController when a plan is rejected (budget over-spend or
        /// other validation failure).
        /// </summary>
        private void OnPlanRejected(int costAttempted, string reason)
        {
            Debug.LogWarning($"[PlanningPhaseUI] Plan rejected: {reason}");
            UpdateStatusLabel($"Plan rejected: {reason}");
            SetPlanningControlsInteractable(true);
            ShowResolvingOverlay(false);
        }

        // ---------------------------------------------------------------------------
        // UI helpers
        // ---------------------------------------------------------------------------

        private void UpdateBudgetLabel()
        {
            if (_budgetLabel != null)
                _budgetLabel.text = $"Budget: {_budget} / {TotalActivationBudget} pts";
        }

        private void UpdateStatusLabel(string message)
        {
            if (_statusLabel != null)
                _statusLabel.text = message;
        }

        private void SetPlanningControlsInteractable(bool interactable)
        {
            if (_lockPlanButton != null)  _lockPlanButton.interactable = interactable;
            if (_clearButton != null)     _clearButton.interactable    = interactable;

            // Also toggle unit entry buttons.
            foreach (GameObject entry in _unitEntryInstances)
            {
                if (entry == null) continue;
                Button btn = entry.GetComponentInChildren<Button>();
                if (btn != null) btn.interactable = interactable;
            }
        }

        private void ShowResolvingOverlay(bool visible)
        {
            if (_resolvingOverlay != null)
                _resolvingOverlay.SetActive(visible);
        }

        private void ShowNextTurnButton(bool visible)
        {
            if (_nextTurnButton != null)
                _nextTurnButton.gameObject.SetActive(visible);
        }
    }
}

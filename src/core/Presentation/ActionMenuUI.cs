using System;
using UnityEngine;
using Battlemancers.Core.Simulation;
using Battlemancers.Unity;
using Battlemancers.UI.Battle;

namespace Battlemancers.Presentation
{
    // BOOTSTRAPPER WIRING REQUIRED:
    // actionMenuUI.SetDependencies(actionMenuPanel, moveSelectionUI, spellSelectionUI, hotseatOrchestrator);
    // moveSelectionUI.SetDependencies(gridRenderer, sim);
    // actionMenuUI.ActionCommitted += hotseatOrchestrator.OnUnitActionCommitted;

    // =========================================================================
    // Supporting data types
    // =========================================================================

    /// <summary>
    /// Identifies which action the player has chosen for the active unit.
    /// </summary>
    public enum ActionType
    {
        /// <summary>The unit will move to a new tile this activation.</summary>
        Move,

        /// <summary>The unit will cast a spell this activation.</summary>
        CastSpell,

        /// <summary>The unit takes no action and ends its activation immediately.</summary>
        EndActivation
    }

    /// <summary>
    /// Packages the player's complete action decision for a single unit activation.
    /// Raised by ActionMenuUI.ActionCommitted so the planning layer can translate it
    /// into a PlannedActivation and submit it to BattleSceneController.
    /// </summary>
    public sealed class UnitActionIntent
    {
        /// <summary>The unit whose activation this intent describes.</summary>
        public UnitState Unit { get; set; }

        /// <summary>Which action type the player chose.</summary>
        public ActionType Type { get; set; }

        /// <summary>
        /// The destination tile chosen by the player.
        /// Non-null only when <see cref="Type"/> is <see cref="ActionType.Move"/>.
        /// </summary>
        public Vector2Int? MoveTarget { get; set; }

        /// <summary>
        /// The spell cast decision from SpellSelectionUI.
        /// Non-null only when <see cref="Type"/> is <see cref="ActionType.CastSpell"/>.
        /// </summary>
        public SpellCastIntent SpellIntent { get; set; }
    }

    // =========================================================================
    // ActionMenuUI
    // =========================================================================

    /// <summary>
    /// Per-unit action chooser shown during the planning phase.
    ///
    /// Flow:
    ///   1. ShowForUnit(unit) — shows the action menu with available options.
    ///   2. Player clicks Move, Cast Spell, or End Activation.
    ///   3. OnMoveSelected() / OnCastSpellSelected() / OnEndActivationSelected()
    ///      route to the appropriate sub-system or commit a no-op intent.
    ///   4. ActionCommitted fires with the final UnitActionIntent.
    ///
    /// Move is disabled when the unit has already moved this activation (_hasMoved flag).
    /// Cast Spell is disabled when the unit has the SILENCED status.
    /// End Activation is always available.
    ///
    /// All dependencies injected via SetDependencies() — no FindObjectOfType, no singletons.
    /// This class does not import UnityEngine.UI or TMPro; that is ActionMenuPanel's job.
    /// </summary>
    public class ActionMenuUI : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        /// <summary>
        /// String key matching StatusType.Silenced used to check UnitState.ActiveStatusTypes.
        /// </summary>
        private const string StatusKeySilenced = "Silenced";

        // ---------------------------------------------------------------------------
        // Events
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Raised when the player has committed a complete action for the active unit.
        /// Subscribers (HotseatOrchestrator or a future PlayerInputController) should
        /// translate this into a PlannedActivation and submit it.
        /// </summary>
        public event Action<UnitActionIntent> ActionCommitted;

        // ---------------------------------------------------------------------------
        // Injected dependencies (set by BattleSceneBootstrapper)
        // ---------------------------------------------------------------------------

        private ActionMenuPanel  _actionMenuPanel;
        private MoveSelectionUI  _moveSelectionUI;
        private SpellSelectionUI _spellSelectionUI;
        private HotseatOrchestrator _hotseatOrchestrator;
        private SimulationBootstrapper _sim;

        // ---------------------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------------------

        private UnitState _activeUnit;

        /// <summary>
        /// Tracks whether the current unit has already used its move this activation.
        /// Reset each time ShowForUnit is called for a new unit.
        /// </summary>
        private bool _hasMoved;

        // ---------------------------------------------------------------------------
        // Dependency injection — called by BattleSceneBootstrapper
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Injects all dependencies. Must be called by BattleSceneBootstrapper before
        /// ShowForUnit() is invoked.
        /// </summary>
        public void SetDependencies(
            ActionMenuPanel actionMenuPanel,
            MoveSelectionUI moveSelectionUI,
            SpellSelectionUI spellSelectionUI,
            HotseatOrchestrator hotseatOrchestrator,
            SimulationBootstrapper sim = null)
        {
            _actionMenuPanel     = actionMenuPanel;
            _moveSelectionUI     = moveSelectionUI;
            _spellSelectionUI    = spellSelectionUI;
            _hotseatOrchestrator = hotseatOrchestrator;
            _sim                 = sim;

            WireButtonDelegates();
            WireMoveConfirmedCallback();
            WireSpellIntentCallback();
        }

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Shows the action menu for the given unit during the planning phase.
        /// Enables/disables Move and Cast Spell based on the unit's current state.
        /// </summary>
        /// <param name="unit">The unit the player clicked. Must not be null.</param>
        public void ShowForUnit(UnitState unit)
        {
            if (unit == null)
            {
                Debug.LogWarning("[ActionMenuUI] ShowForUnit called with null unit.");
                return;
            }

            _activeUnit = unit;
            _hasMoved   = false;

            bool canMove = CanUnitMove(unit);
            bool canCast = CanUnitCast(unit);

            _actionMenuPanel?.Show(unit.Id, canMove, canCast);
            Debug.Log($"[ActionMenuUI] Showing action menu for '{unit.Id}'. canMove={canMove} canCast={canCast}");
        }

        /// <summary>
        /// Hides the action menu and clears the active unit reference.
        /// </summary>
        public void Hide()
        {
            _activeUnit = null;
            _actionMenuPanel?.Hide();
            _moveSelectionUI?.ExitMoveMode();
        }

        // ---------------------------------------------------------------------------
        // Button handlers — called by ActionMenuPanel delegates
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Called when the player clicks the Move button.
        /// Enters move-selection mode via MoveSelectionUI. The menu stays visible
        /// so the player can cancel by clicking elsewhere (future cancel support).
        /// MoveConfirmed on MoveSelectionUI will fire ActionCommitted when a tile is chosen.
        /// </summary>
        public void OnMoveSelected()
        {
            if (_activeUnit == null) return;

            if (!CanUnitMove(_activeUnit))
            {
                Debug.LogWarning($"[ActionMenuUI] Move attempted but unit '{_activeUnit.Id}' cannot move.");
                return;
            }

            Debug.Log($"[ActionMenuUI] Move selected for '{_activeUnit.Id}'. Entering move mode.");

            SimulationState state = _sim?.State;
            if (state == null)
            {
                Debug.LogWarning("[ActionMenuUI] SimulationState unavailable — cannot enter move mode.");
                return;
            }

            _actionMenuPanel?.Hide();
            _moveSelectionUI?.EnterMoveMode(_activeUnit, state);
        }

        /// <summary>
        /// Called when the player clicks the Cast Spell button.
        /// Delegates to SpellSelectionUI.ShowSpells(unit) and hides the action menu.
        /// SpellSelectionUI.SpellIntentReady will fire ActionCommitted when the intent is complete.
        /// </summary>
        public void OnCastSpellSelected()
        {
            if (_activeUnit == null) return;

            if (!CanUnitCast(_activeUnit))
            {
                Debug.LogWarning($"[ActionMenuUI] Cast attempted but unit '{_activeUnit.Id}' is silenced.");
                return;
            }

            Debug.Log($"[ActionMenuUI] Cast Spell selected for '{_activeUnit.Id}'.");
            _actionMenuPanel?.Hide();
            _spellSelectionUI?.ShowSpells(_activeUnit);
        }

        /// <summary>
        /// Called when the player clicks End Activation.
        /// Fires ActionCommitted with an EndActivation intent and hides the menu.
        /// </summary>
        public void OnEndActivationSelected()
        {
            if (_activeUnit == null) return;

            Debug.Log($"[ActionMenuUI] End Activation selected for '{_activeUnit.Id}'.");

            var intent = new UnitActionIntent
            {
                Unit = _activeUnit,
                Type = ActionType.EndActivation
            };

            Hide();
            ActionCommitted?.Invoke(intent);
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the unit is allowed to move: it has not already moved this
        /// activation and does not have the ROOTED status (ROOTED is enforced by
        /// MoveSelectionUI computing an empty valid-tile set, but we also gate the button).
        /// </summary>
        private bool CanUnitMove(UnitState unit)
        {
            if (_hasMoved)
                return false;

            // ROOTED status prevents movement — disable the button as a UX affordance.
            if (unit.ActiveStatusTypes.Contains("Rooted"))
                return false;

            return true;
        }

        /// <summary>
        /// Returns true if the unit is allowed to cast spells (not SILENCED).
        /// </summary>
        private static bool CanUnitCast(UnitState unit)
        {
            return !unit.ActiveStatusTypes.Contains(StatusKeySilenced);
        }

        /// <summary>
        /// Wires the three ActionMenuPanel delegates to this class's handler methods.
        /// Called once in SetDependencies. If the panel is replaced, re-call this method.
        /// </summary>
        private void WireButtonDelegates()
        {
            if (_actionMenuPanel == null) return;

            _actionMenuPanel.OnMoveClicked          = OnMoveSelected;
            _actionMenuPanel.OnCastSpellClicked      = OnCastSpellSelected;
            _actionMenuPanel.OnEndActivationClicked  = OnEndActivationSelected;
        }

        /// <summary>
        /// Subscribes to MoveSelectionUI.MoveConfirmed so that a confirmed tile
        /// translates into an ActionCommitted event with a Move intent.
        /// </summary>
        private void WireMoveConfirmedCallback()
        {
            if (_moveSelectionUI == null) return;

            _moveSelectionUI.MoveConfirmed += OnMoveConfirmed;
        }

        /// <summary>
        /// Subscribes to SpellSelectionUI.SpellIntentReady so that a confirmed spell
        /// translates into an ActionCommitted event with a CastSpell intent.
        /// </summary>
        private void WireSpellIntentCallback()
        {
            if (_spellSelectionUI == null) return;

            _spellSelectionUI.SpellIntentReady += OnSpellIntentReady;
        }

        /// <summary>
        /// Called when MoveSelectionUI confirms a move destination.
        /// Sets _hasMoved, builds a Move intent, and fires ActionCommitted.
        /// </summary>
        private void OnMoveConfirmed(UnitState unit, Vector2Int tile)
        {
            _hasMoved = true;

            var intent = new UnitActionIntent
            {
                Unit       = unit,
                Type       = ActionType.Move,
                MoveTarget = tile
            };

            ActionCommitted?.Invoke(intent);
        }

        /// <summary>
        /// Called when SpellSelectionUI fires SpellIntentReady.
        /// Builds a CastSpell intent and fires ActionCommitted.
        /// </summary>
        private void OnSpellIntentReady(SpellCastIntent spellIntent)
        {
            if (_activeUnit == null) return;

            var intent = new UnitActionIntent
            {
                Unit        = _activeUnit,
                Type        = ActionType.CastSpell,
                SpellIntent = spellIntent
            };

            Hide();
            ActionCommitted?.Invoke(intent);
        }

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void OnDestroy()
        {
            if (_moveSelectionUI != null)
                _moveSelectionUI.MoveConfirmed -= OnMoveConfirmed;

            if (_spellSelectionUI != null)
                _spellSelectionUI.SpellIntentReady -= OnSpellIntentReady;
        }
    }
}

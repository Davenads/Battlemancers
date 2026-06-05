using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Battlemancers.UI.Battle
{
    /// <summary>
    /// Unity UI component that renders the per-unit action menu.
    ///
    /// Responsibilities:
    ///   - Show(unitName, canMove, canCast): displays the panel, updates the unit name label,
    ///     and enables/disables Move and Cast Spell buttons accordingly.
    ///   - Hide(): collapses the panel.
    ///   - Three public Action delegates (OnMoveClicked, OnCastSpellClicked,
    ///     OnEndActivationClicked) are set by ActionMenuUI and called by each button's onClick.
    ///
    /// This is the ONLY class permitted to import UnityEngine.UI or TMPro in the
    /// Presentation / UI.Battle layer for this component. All logic lives in ActionMenuUI.
    ///
    /// If no prefab buttons are assigned in the Inspector, a minimal button hierarchy is
    /// created procedurally at Awake(), matching the fallback pattern used by SpellButtonPanel.
    ///
    /// No FindObjectOfType. No singletons. Dependencies are set by ActionMenuUI.
    /// </summary>
    public class ActionMenuPanel : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        private const string LabelMove          = "Move";
        private const string LabelCastSpell     = "Cast Spell";
        private const string LabelEndActivation = "End Activation";

        /// <summary>Color for buttons that are currently available.</summary>
        private static readonly Color ColorAvailable   = Color.white;

        /// <summary>Color applied to buttons that are grayed-out (disabled).</summary>
        private static readonly Color ColorUnavailable = new Color(0.5f, 0.5f, 0.5f, 1.0f);

        // ---------------------------------------------------------------------------
        // Inspector references — assign in Unity Inspector or leave null for procedural fallback
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Optional container for all three buttons. If null, <see cref="transform"/> is used.
        /// A VerticalLayoutGroup on this object is recommended.
        /// </summary>
        [SerializeField] private Transform _buttonContainer;

        /// <summary>
        /// Optional label showing the active unit's name. If null, no name is displayed.
        /// </summary>
        [SerializeField] private TMP_Text _unitNameLabel;

        /// <summary>
        /// Optional pre-built Move button. If null, one is created procedurally.
        /// </summary>
        [SerializeField] private Button _moveButton;

        /// <summary>
        /// Optional pre-built Cast Spell button. If null, one is created procedurally.
        /// </summary>
        [SerializeField] private Button _castSpellButton;

        /// <summary>
        /// Optional pre-built End Activation button. If null, one is created procedurally.
        /// </summary>
        [SerializeField] private Button _endActivationButton;

        // ---------------------------------------------------------------------------
        // Public action delegates — set by ActionMenuUI
        // ---------------------------------------------------------------------------

        /// <summary>Called when the Move button is clicked. Set by ActionMenuUI.</summary>
        public Action OnMoveClicked;

        /// <summary>Called when the Cast Spell button is clicked. Set by ActionMenuUI.</summary>
        public Action OnCastSpellClicked;

        /// <summary>Called when the End Activation button is clicked. Set by ActionMenuUI.</summary>
        public Action OnEndActivationClicked;

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            EnsureButtonsExist();
            WireButtonListeners();
            gameObject.SetActive(false);
        }

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Shows the action menu panel for the given unit.
        /// Enables/disables Move and Cast Spell based on the supplied flags.
        /// End Activation is always enabled.
        /// </summary>
        /// <param name="unitName">Display name / ID of the active unit.</param>
        /// <param name="canMove">True if the Move button should be interactable.</param>
        /// <param name="canCast">True if the Cast Spell button should be interactable.</param>
        public void Show(string unitName, bool canMove, bool canCast)
        {
            if (_unitNameLabel != null)
                _unitNameLabel.text = unitName ?? string.Empty;

            SetButtonState(_moveButton,       canMove);
            SetButtonState(_castSpellButton,  canCast);
            SetButtonState(_endActivationButton, interactable: true);

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Hides the action menu panel.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// If any of the three required buttons are null (no Inspector assignment),
        /// creates minimal button GameObjects procedurally under <see cref="_buttonContainer"/>.
        /// Matches the fallback pattern in SpellButtonPanel.
        /// </summary>
        private void EnsureButtonsExist()
        {
            Transform container = _buttonContainer != null ? _buttonContainer : transform;

            if (_moveButton == null)
                _moveButton = CreateButton(LabelMove, container);

            if (_castSpellButton == null)
                _castSpellButton = CreateButton(LabelCastSpell, container);

            if (_endActivationButton == null)
                _endActivationButton = CreateButton(LabelEndActivation, container);
        }

        /// <summary>
        /// Subscribes each button's onClick to the appropriate public Action delegate.
        /// Delegates may be null when wired; they are invoked null-safely at click time.
        /// </summary>
        private void WireButtonListeners()
        {
            if (_moveButton != null)
                _moveButton.onClick.AddListener(()          => OnMoveClicked?.Invoke());

            if (_castSpellButton != null)
                _castSpellButton.onClick.AddListener(()     => OnCastSpellClicked?.Invoke());

            if (_endActivationButton != null)
                _endActivationButton.onClick.AddListener(() => OnEndActivationClicked?.Invoke());
        }

        /// <summary>
        /// Sets a button's interactability and tints its label to signal availability.
        /// </summary>
        private static void SetButtonState(Button button, bool interactable)
        {
            if (button == null) return;

            button.interactable = interactable;

            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.color = interactable ? ColorAvailable : ColorUnavailable;
        }

        /// <summary>
        /// Creates a minimal button GameObject with an Image, Button, and TMP_Text label.
        /// </summary>
        private static Button CreateButton(string labelText, Transform parent)
        {
            var go = new GameObject($"ActionBtn_{labelText.Replace(" ", "")}");
            go.transform.SetParent(parent, worldPositionStays: false);

            var image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            var button = go.AddComponent<Button>();

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, worldPositionStays: false);

            var text = labelGo.AddComponent<TextMeshProUGUI>();
            text.text = labelText;
            text.color = ColorAvailable;

            return button;
        }
    }
}

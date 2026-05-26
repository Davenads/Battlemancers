using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Battlemancers.Core.Data;

namespace Battlemancers.UI
{
    /// <summary>
    /// One clickable Mancer portrait card in the roster browser panel.
    ///
    /// Self-contained: knows its own <see cref="MancerId"/>, renders all visible fields
    /// from <see cref="MancerRuntimeData"/>, and fires <see cref="OnCardClicked"/> upward
    /// so <see cref="WarbandBuilderManager"/> can react without polling.
    ///
    /// Unity only — do not reference from pure-C# simulation code.
    /// </summary>
    public class MancerCardSelectable : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        private const string MancerBaseCostLabel = "100 pts";
        private const float  DimAlpha            = 0.5f;
        private const float  FullAlpha           = 0f;

        // ---------------------------------------------------------------------------
        // Inspector references
        // ---------------------------------------------------------------------------

        [SerializeField] private Button       _button;
        [SerializeField] private Image        _portrait;
        [SerializeField] private TMP_Text     _nameLabel;
        [SerializeField] private TMP_Text     _elementLabel;
        [SerializeField] private TMP_Text     _costLabel;        // "100 pts"
        [SerializeField] private GameObject   _selectedBadge;    // checkmark + slot-number overlay
        [SerializeField] private TMP_Text     _slotNumberLabel;  // "Slot 1", "Slot 2", "Slot 3"
        [SerializeField] private CanvasGroup  _dimOverlay;       // set to DimAlpha when unavailable

        // ---------------------------------------------------------------------------
        // Public state
        // ---------------------------------------------------------------------------

        /// <summary>The Mancer archetype ID this card represents. Set during <see cref="Setup"/>.</summary>
        public string MancerId { get; private set; }

        // ---------------------------------------------------------------------------
        // Events
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Fired when the player clicks this card. Passes <see cref="MancerId"/> as the argument.
        /// Subscribe in <see cref="WarbandBuilderManager"/> to react to roster clicks.
        /// </summary>
        public event Action<string> OnCardClicked;

        // ---------------------------------------------------------------------------
        // Setup
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initialises this card from <paramref name="data"/>. Must be called once after instantiation.
        /// </summary>
        /// <param name="data">Runtime data for the Mancer archetype this card represents.</param>
        public void Setup(MancerRuntimeData data)
        {
            MancerId = data.MancerId;

            if (_nameLabel    != null) _nameLabel.text    = data.DisplayName;
            if (_elementLabel != null) _elementLabel.text = data.PrimaryElement;
            if (_costLabel    != null) _costLabel.text    = MancerBaseCostLabel;

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => OnCardClicked?.Invoke(MancerId));
            }

            SetSelected(false, -1);
        }

        // ---------------------------------------------------------------------------
        // State display
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Shows or hides the selected-badge overlay and updates the slot-number label.
        /// </summary>
        /// <param name="selected">True if this Mancer is currently assigned to a warband slot.</param>
        /// <param name="slotIndex">Zero-based slot index; ignored when <paramref name="selected"/> is false.</param>
        public void SetSelected(bool selected, int slotIndex)
        {
            if (_selectedBadge != null)
                _selectedBadge.SetActive(selected);

            if (selected && slotIndex >= 0 && _slotNumberLabel != null)
                _slotNumberLabel.text = $"Slot {slotIndex + 1}";
        }

        /// <summary>
        /// Enables or disables interaction with this card and adjusts the dim overlay alpha.
        /// Cards are unavailable when all Mancer slots are filled and this card is not selected.
        /// </summary>
        /// <param name="available">True to allow clicks; false to dim and block interaction.</param>
        public void SetAvailable(bool available)
        {
            if (_button     != null) _button.interactable = available;
            if (_dimOverlay != null) _dimOverlay.alpha    = available ? FullAlpha : DimAlpha;
        }
    }
}

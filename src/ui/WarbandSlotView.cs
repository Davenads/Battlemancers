using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Battlemancers.Core.Data;

namespace Battlemancers.UI
{
    /// <summary>
    /// Represents one Mancer slot in the warband panel (right side of the builder).
    ///
    /// Displays two states — empty and filled — by toggling child GameObjects.
    /// Filled state shows the assigned Mancer name, selected upgrades, and total cost,
    /// plus remove and edit-upgrades buttons.
    ///
    /// All interactions fire C# events upward; no direct coupling to
    /// <see cref="WarbandBuilderManager"/>.
    ///
    /// Unity only — do not reference from pure-C# simulation code.
    /// </summary>
    public class WarbandSlotView : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector references
        // ---------------------------------------------------------------------------

        [SerializeField] private GameObject _emptyState;       // shown when slot is empty
        [SerializeField] private GameObject _filledState;      // shown when Mancer is assigned

        [SerializeField] private TMP_Text   _mancerNameLabel;
        [SerializeField] private TMP_Text   _costLabel;        // "125 pts (100 + upgrades)"
        [SerializeField] private TMP_Text   _upgradesLabel;    // comma-list of upgrade IDs

        [SerializeField] private Button     _removeButton;
        [SerializeField] private Button     _editUpgradesButton;

        // ---------------------------------------------------------------------------
        // Public state
        // ---------------------------------------------------------------------------

        /// <summary>Zero-based index of this slot within the warband panel (0–2).</summary>
        public int SlotIndex { get; private set; }

        /// <summary>
        /// The Mancer archetype ID currently occupying this slot, or null when empty.
        /// </summary>
        public string OccupiedMancerId { get; private set; }

        // ---------------------------------------------------------------------------
        // Events
        // ---------------------------------------------------------------------------

        /// <summary>Fired when the remove button is clicked. Passes <see cref="SlotIndex"/>.</summary>
        public event Action<int> OnRemoveClicked;

        /// <summary>Fired when the edit-upgrades button is clicked. Passes <see cref="SlotIndex"/>.</summary>
        public event Action<int> OnEditUpgradesClicked;

        // ---------------------------------------------------------------------------
        // Lifecycle
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Binds this view to a slot index and wires the action buttons.
        /// Must be called once after instantiation before any other method.
        /// </summary>
        /// <param name="slotIndex">Zero-based position of this slot (0–2).</param>
        public void Initialize(int slotIndex)
        {
            SlotIndex = slotIndex;

            if (_removeButton != null)
            {
                _removeButton.onClick.RemoveAllListeners();
                _removeButton.onClick.AddListener(() => OnRemoveClicked?.Invoke(SlotIndex));
            }

            if (_editUpgradesButton != null)
            {
                _editUpgradesButton.onClick.RemoveAllListeners();
                _editUpgradesButton.onClick.AddListener(() => OnEditUpgradesClicked?.Invoke(SlotIndex));
            }

            SetEmpty();
        }

        // ---------------------------------------------------------------------------
        // State display
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Populates the filled state with the given Mancer's information.
        /// Switches the visible child state from empty to filled.
        /// </summary>
        /// <param name="data">Runtime data for the assigned Mancer.</param>
        /// <param name="selectedUpgradeIds">Upgrade IDs currently selected for this Mancer.</param>
        /// <param name="totalCost">Pre-computed total cost (100 base + upgrade costs).</param>
        public void SetMancer(MancerRuntimeData data, List<string> selectedUpgradeIds, int totalCost)
        {
            OccupiedMancerId = data.MancerId;

            if (_mancerNameLabel != null)
                _mancerNameLabel.text = data.DisplayName;

            if (_costLabel != null)
            {
                _costLabel.text = selectedUpgradeIds != null && selectedUpgradeIds.Count > 0
                    ? $"{totalCost} pts (100 + upgrades)"
                    : "100 pts";
            }

            if (_upgradesLabel != null)
            {
                _upgradesLabel.text = selectedUpgradeIds != null && selectedUpgradeIds.Count > 0
                    ? string.Join(", ", selectedUpgradeIds)
                    : "No upgrades";
            }

            if (_emptyState  != null) _emptyState.SetActive(false);
            if (_filledState != null) _filledState.SetActive(true);
        }

        /// <summary>
        /// Resets this slot to the empty state and clears <see cref="OccupiedMancerId"/>.
        /// </summary>
        public void SetEmpty()
        {
            OccupiedMancerId = null;

            if (_emptyState  != null) _emptyState.SetActive(true);
            if (_filledState != null) _filledState.SetActive(false);
        }
    }
}

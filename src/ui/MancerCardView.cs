using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Battlemancers.Core.Data;

namespace Battlemancers.UI
{
    /// <summary>
    /// A card in the Roster Browser panel representing one Mancer archetype.
    ///
    /// Displays name, point cost, primary element, tactical identity, and an element-tinted border.
    /// Fires the supplied <see cref="System.Action{T}"/> callback when the add button is clicked.
    /// The in-warband indicator is shown and the add button disabled while the Mancer is already
    /// present in the current draft.
    ///
    /// Unity only — do not reference from pure-C# simulation code.
    /// </summary>
    public class MancerCardView : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Inspector references
        // ---------------------------------------------------------------------------

        [SerializeField] private TMP_Text _nameLabel;
        [SerializeField] private TMP_Text _costLabel;          // "100 pts"
        [SerializeField] private TMP_Text _elementLabel;       // primary element name
        [SerializeField] private TMP_Text _tacticalIdentity;   // one-line description
        [SerializeField] private Image    _elementColorBorder; // border tinted by element color
        [SerializeField] private Button   _addButton;
        [SerializeField] private Image    _inWarbandIndicator; // shown when Mancer is in current draft

        // ---------------------------------------------------------------------------
        // State
        // ---------------------------------------------------------------------------

        private Action<string> _onAdd;

        /// <summary>The Mancer archetype ID this card represents. Set during <see cref="Setup"/>.</summary>
        public string MancerId { get; private set; }

        // ---------------------------------------------------------------------------
        // Setup
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initialises the card from <paramref name="data"/> and wires the add-button callback.
        /// Must be called once after instantiation.
        /// </summary>
        /// <param name="data">Runtime data for this Mancer archetype.</param>
        /// <param name="onAdd">
        /// Callback invoked with <see cref="MancerId"/> when the add button is clicked.
        /// Typically <c>WarbandBuilderManager.AddMancer</c>.
        /// </param>
        public void Setup(MancerRuntimeData data, Action<string> onAdd)
        {
            MancerId = data.MancerId;
            _onAdd   = onAdd;

            if (_nameLabel         != null) _nameLabel.text         = data.DisplayName;
            if (_costLabel         != null) _costLabel.text         = $"{data.BaseCost} pts";
            if (_elementLabel      != null) _elementLabel.text      = data.PrimaryElement;
            if (_tacticalIdentity  != null) _tacticalIdentity.text  = data.TacticalIdentity;
            if (_elementColorBorder != null) _elementColorBorder.color = GetElementColor(data.PrimaryElement);

            if (_addButton != null)
            {
                _addButton.onClick.RemoveAllListeners();
                _addButton.onClick.AddListener(() => _onAdd?.Invoke(MancerId));
            }

            SetInWarband(false);
        }

        // ---------------------------------------------------------------------------
        // State display
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Toggles the in-warband indicator and disables/enables the add button accordingly.
        /// Call this whenever the draft changes.
        /// </summary>
        /// <param name="inWarband">True if this Mancer is already in the current draft.</param>
        public void SetInWarband(bool inWarband)
        {
            if (_inWarbandIndicator != null)
                _inWarbandIndicator.gameObject.SetActive(inWarband);

            if (_addButton != null)
                _addButton.interactable = !inWarband;
        }

        // ---------------------------------------------------------------------------
        // Element color lookup
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns a display color for the given element name.
        /// Each element maps to exactly one color; defaults to white for unknown elements.
        /// </summary>
        private static Color GetElementColor(string element) => element switch
        {
            "Fire"      => new Color(1.0f,  0.35f, 0.1f),
            "Water"     => new Color(0.2f,  0.5f,  0.9f),
            "Ice"       => new Color(0.6f,  0.9f,  1.0f),
            "Lightning" => new Color(0.95f, 0.9f,  0.1f),
            "Earth"     => new Color(0.55f, 0.38f, 0.18f),
            "Wind"      => new Color(0.7f,  0.95f, 0.7f),
            "Poison"    => new Color(0.3f,  0.75f, 0.2f),
            "Necrotic"  => new Color(0.45f, 0.1f,  0.55f),
            "Light"     => new Color(1.0f,  0.95f, 0.6f),
            "Sound"     => new Color(0.6f,  0.8f,  0.9f),
            "Gravity"   => new Color(0.4f,  0.3f,  0.6f),
            "Time"      => new Color(0.8f,  0.7f,  1.0f),
            "Crystal"   => new Color(0.85f, 0.95f, 1.0f),
            "Psychic"   => new Color(0.9f,  0.4f,  0.8f),
            "Thermal"   => new Color(1.0f,  0.5f,  0.2f),
            _           => Color.white
        };
    }
}

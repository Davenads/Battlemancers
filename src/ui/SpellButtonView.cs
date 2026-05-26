using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Battlemancers.Core.Data;

namespace Battlemancers.UI
{
    /// <summary>
    /// View for a single spell button in the in-game action bar.
    ///
    /// Displays spell name, AP cost, tier indicator color, and cooldown state.
    /// Greys out and disables interactivity when the spell cannot be cast (insufficient AP or
    /// on cooldown). Self-contained — HUDManager calls Setup() once per unit selection and
    /// SetAvailable() whenever AP or cooldown state changes.
    ///
    /// Unity only — do not use in pure-C# simulation code.
    /// </summary>
    public class SpellButtonView : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Tier indicator colors — named constants, never inline
        // ---------------------------------------------------------------------------

        private static readonly Color TierColorQuick    = new Color(0.3f, 0.9f,  0.3f);  // green
        private static readonly Color TierColorStandard = new Color(0.3f, 0.5f,  0.9f);  // blue
        private static readonly Color TierColorHeavy    = new Color(0.9f, 0.55f, 0.2f);  // orange
        private static readonly Color TierColorUltimate = new Color(0.7f, 0.1f,  0.9f);  // purple

        private const float AlphaUsable   = 1.0f;
        private const float AlphaDisabled = 0.4f;

        // ---------------------------------------------------------------------------
        // Inspector references
        // ---------------------------------------------------------------------------

        [SerializeField] private Button      _button;
        [SerializeField] private TMP_Text    _spellNameLabel;
        [SerializeField] private TMP_Text    _apCostLabel;
        [SerializeField] private TMP_Text    _cooldownLabel;   // shows "CD: 2" when on cooldown
        [SerializeField] private Image       _tierIndicator;   // colored dot per spell tier
        [SerializeField] private CanvasGroup _canvasGroup;     // alpha for disabled state

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Binds this button to a spell. Call once per unit selection or spell list refresh.
        /// </summary>
        /// <param name="spell">The spell definition to display.</param>
        /// <param name="spellIndex">Zero-based index of this spell in the unit's spell list. Passed back to <paramref name="onClicked"/>.</param>
        /// <param name="onClicked">Callback invoked with <paramref name="spellIndex"/> when the button is pressed.</param>
        public void Setup(SpellRuntimeData spell, int spellIndex, Action<int> onClicked)
        {
            _spellNameLabel.text = spell.DisplayName;
            _apCostLabel.text    = $"{spell.ApCost} AP";
            _tierIndicator.color = GetTierColor(spell.Tier);

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onClicked(spellIndex));
        }

        /// <summary>
        /// Updates interactability and overlay labels based on current AP and cooldown state.
        /// Call whenever ActionPoints or SpellCooldowns change for the displayed unit.
        /// </summary>
        /// <param name="available">Whether the unit is in a state that allows spell use at all.</param>
        /// <param name="canAfford">True if the unit has enough AP to cast this spell right now.</param>
        /// <param name="cooldownLeft">Turns remaining on this spell's cooldown. 0 means ready.</param>
        public void SetAvailable(bool available, bool canAfford, int cooldownLeft = 0)
        {
            bool notOnCooldown = cooldownLeft == 0;
            bool usable        = available && canAfford && notOnCooldown;

            _button.interactable = usable;
            _canvasGroup.alpha   = usable ? AlphaUsable : AlphaDisabled;

            _cooldownLabel.text = cooldownLeft > 0 ? $"CD: {cooldownLeft}" : string.Empty;
            _cooldownLabel.gameObject.SetActive(cooldownLeft > 0);
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        private static Color GetTierColor(string tier) => tier switch
        {
            "Quick"    => TierColorQuick,
            "Standard" => TierColorStandard,
            "Heavy"    => TierColorHeavy,
            "Ultimate" => TierColorUltimate,
            _          => Color.white
        };
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Battlemancers.Core.Data;
using Battlemancers.Presentation;

namespace Battlemancers.UI.Battle
{
    /// <summary>
    /// Unity UI component that renders a Mancer's spell list as clickable buttons.
    ///
    /// Responsibilities:
    ///   - Populate(spells, selectionUI): instantiates one Button per spell, showing
    ///     display name, AP cost, and cooldown.
    ///   - SetSpellAvailable(spell, available): grays out unavailable spells
    ///     (on cooldown or insufficient AP).
    ///   - Clear(): destroys all instantiated buttons.
    ///
    /// This is the ONLY class permitted to import UnityEngine.UI or TMPro in the
    /// Presentation / UI.Battle layer. All text rendering uses TMP_Text.
    ///
    /// Dependencies are wired by BattleSceneBootstrapper via SpellSelectionUI.SetDependencies.
    /// SpellSelectionUI owns a reference to this panel and passes itself as the callback
    /// target so buttons can call SpellSelectionUI.OnSpellSelected.
    ///
    /// No FindObjectOfType. No singletons.
    /// </summary>
    public class SpellButtonPanel : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        /// <summary>Color applied to available spell buttons.</summary>
        private static readonly Color ColorAvailable   = Color.white;

        /// <summary>Color applied to unavailable (grayed-out) spell buttons.</summary>
        private static readonly Color ColorUnavailable = new Color(0.5f, 0.5f, 0.5f, 1.0f);

        /// <summary>
        /// Label format for a spell button with no active cooldown.
        /// {0} = DisplayName, {1} = AP cost.
        /// </summary>
        private const string LabelFormatReady = "{0}  [{1} AP]";

        /// <summary>
        /// Label format for a spell button currently on cooldown.
        /// {0} = DisplayName, {1} = AP cost, {2} = turns remaining.
        /// </summary>
        private const string LabelFormatCooldown = "{0}  [{1} AP]  (CD: {2})";

        // ---------------------------------------------------------------------------
        // Inspector references
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Prefab for a single spell button row.
        /// Must contain at least a Button component and a TMP_Text child for the label.
        /// Assigned in the Inspector.
        /// </summary>
        [SerializeField] private GameObject _spellButtonPrefab;

        /// <summary>
        /// Parent transform for instantiated spell buttons (a VerticalLayoutGroup is recommended).
        /// Assigned in the Inspector.
        /// </summary>
        [SerializeField] private Transform _buttonContainer;

        // ---------------------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------------------

        // Maps spell ID → instantiated button GameObject so we can find them for
        // SetSpellAvailable and Clear calls.
        private readonly Dictionary<string, GameObject> _buttonInstances
            = new Dictionary<string, GameObject>();

        // Maps spell ID → SpellRuntimeData for lookup in SetSpellAvailable.
        private readonly Dictionary<string, SpellRuntimeData> _spellIndex
            = new Dictionary<string, SpellRuntimeData>();

        // The SpellSelectionUI that owns this panel; buttons call back into it.
        private SpellSelectionUI _selectionUI;

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Destroys any existing buttons and instantiates one per spell in
        /// <paramref name="spells"/>. Each button's onClick calls
        /// <paramref name="selectionUI"/>.OnSpellSelected with that spell's data.
        /// </summary>
        /// <param name="spells">The spell list to render. May be empty.</param>
        /// <param name="selectionUI">
        /// The SpellSelectionUI that will handle button clicks. Must not be null.
        /// </param>
        public void Populate(List<SpellRuntimeData> spells, SpellSelectionUI selectionUI)
        {
            Clear();

            if (selectionUI == null)
            {
                Debug.LogWarning("[SpellButtonPanel] Populate called with null selectionUI.");
                return;
            }

            _selectionUI = selectionUI;

            if (spells == null || spells.Count == 0)
                return;

            foreach (SpellRuntimeData spell in spells)
            {
                if (spell == null)
                    continue;

                GameObject buttonGo = InstantiateButtonForSpell(spell);
                if (buttonGo == null)
                    continue;

                _buttonInstances[spell.SpellId] = buttonGo;
                _spellIndex[spell.SpellId]      = spell;
            }
        }

        /// <summary>
        /// Grays out the button for the given spell when <paramref name="available"/> is
        /// false (e.g., the caster lacks AP or the spell is on cooldown).
        /// Re-enables it when <paramref name="available"/> is true.
        /// </summary>
        /// <param name="spell">The spell whose button to update.</param>
        /// <param name="available">Whether the player can currently cast this spell.</param>
        public void SetSpellAvailable(SpellRuntimeData spell, bool available)
        {
            if (spell == null)
                return;

            if (!_buttonInstances.TryGetValue(spell.SpellId, out GameObject buttonGo) || buttonGo == null)
                return;

            Button btn = buttonGo.GetComponentInChildren<Button>();
            if (btn != null)
            {
                btn.interactable = available;

                // Tint the button label to signal availability.
                TMP_Text label = buttonGo.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.color = available ? ColorAvailable : ColorUnavailable;
            }
        }

        /// <summary>
        /// Destroys all instantiated spell buttons and clears internal state.
        /// Safe to call when no buttons exist.
        /// </summary>
        public void Clear()
        {
            foreach (GameObject go in _buttonInstances.Values)
            {
                if (go != null)
                    Destroy(go);
            }

            _buttonInstances.Clear();
            _spellIndex.Clear();
            _selectionUI = null;
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Instantiates a spell button from the prefab (or a fallback GameObject when
        /// no prefab is assigned), configures its label and onClick listener.
        /// Returns the instantiated GameObject, or null if instantiation failed.
        /// </summary>
        private GameObject InstantiateButtonForSpell(SpellRuntimeData spell)
        {
            Transform container = _buttonContainer != null ? _buttonContainer : transform;
            GameObject buttonGo;

            if (_spellButtonPrefab != null)
            {
                buttonGo = Instantiate(_spellButtonPrefab, container);
            }
            else
            {
                // Fallback: create a minimal button inline when no prefab is assigned.
                buttonGo = new GameObject($"SpellBtn_{spell.SpellId}");
                buttonGo.transform.SetParent(container, worldPositionStays: false);

                var image  = buttonGo.AddComponent<Image>();
                image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

                buttonGo.AddComponent<Button>();

                var labelGo = new GameObject("Label");
                labelGo.transform.SetParent(buttonGo.transform, worldPositionStays: false);
                labelGo.AddComponent<TMP_Text>();
            }

            if (buttonGo == null)
                return null;

            buttonGo.name = $"SpellBtn_{spell.SpellId}";

            // Set label text.
            TMP_Text text = buttonGo.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = BuildButtonLabel(spell, cooldownTurns: 0);

            // Wire onClick.
            Button button = buttonGo.GetComponentInChildren<Button>();
            if (button != null)
            {
                // Capture loop variable for closure safety.
                SpellRuntimeData capturedSpell = spell;
                button.onClick.AddListener(() => OnSpellButtonClicked(capturedSpell));
            }

            return buttonGo;
        }

        /// <summary>
        /// Called when a spell button is clicked. Delegates to SpellSelectionUI.
        /// </summary>
        private void OnSpellButtonClicked(SpellRuntimeData spell)
        {
            if (_selectionUI == null)
            {
                Debug.LogWarning("[SpellButtonPanel] Button clicked but no SpellSelectionUI is registered.");
                return;
            }

            _selectionUI.OnSpellSelected(spell);
        }

        /// <summary>
        /// Builds the display string for a spell button.
        /// Shows name, AP cost, and cooldown if applicable.
        /// </summary>
        private static string BuildButtonLabel(SpellRuntimeData spell, int cooldownTurns)
        {
            return cooldownTurns > 0
                ? string.Format(LabelFormatCooldown, spell.DisplayName, spell.ApCost, cooldownTurns)
                : string.Format(LabelFormatReady, spell.DisplayName, spell.ApCost);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Battlemancers.Core.Data;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.UI
{
    /// <summary>
    /// Orchestrates all in-game HUD elements: AP pips, thermometer bar, spell buttons,
    /// unit info panel, and turn controls.
    ///
    /// All updates are event-driven — subscribes to SimulationEventBus on Awake and
    /// refreshes only the relevant panels when events arrive. Never polls in Update().
    ///
    /// Dependencies are injected via [SerializeField] inspector references (no FindObjectOfType,
    /// no singletons). The SimulationBootstrapper provides access to SimulationState and the
    /// loaded MancerRuntimeData roster.
    ///
    /// Unity only — do not reference from pure-C# simulation code.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        private const int   MaxApPips       = 6;
        private const int   MaxSpellButtons = 5;
        private const float HpBarMinFill    = 0f;

        // ---------------------------------------------------------------------------
        // Inspector references — simulation layer
        // ---------------------------------------------------------------------------

        [Header("Simulation")]
        [SerializeField] private SimulationBootstrapper _sim;
        [SerializeField] private PlayerInputController  _input;

        // ---------------------------------------------------------------------------
        // Inspector references — AP display
        // ---------------------------------------------------------------------------

        [Header("AP Display")]
        [SerializeField] private List<Image> _apPips;                         // 6 pip images
        [SerializeField] private Color       _pipActiveColor   = new Color(0.9f, 0.85f, 0.2f); // yellow
        [SerializeField] private Color       _pipInactiveColor = new Color(0.25f, 0.25f, 0.25f); // dark grey

        // ---------------------------------------------------------------------------
        // Inspector references — temperature
        // ---------------------------------------------------------------------------

        [Header("Temperature")]
        [SerializeField] private ThermometerBar _thermometerBar;
        [SerializeField] private Button         _thermalComposureButton; // once-per-match reset
        [SerializeField] private TMP_Text       _thermalComposureLabel;  // "COMPOSURE (1)" / "COMPOSURE (used)"

        // ---------------------------------------------------------------------------
        // Inspector references — spell buttons
        // ---------------------------------------------------------------------------

        [Header("Spells")]
        [SerializeField] private List<SpellButtonView> _spellButtons; // up to 5 buttons

        // ---------------------------------------------------------------------------
        // Inspector references — unit info
        // ---------------------------------------------------------------------------

        [Header("Unit Info")]
        [SerializeField] private TMP_Text _unitNameLabel;
        [SerializeField] private TMP_Text _hpLabel;       // "HP: 87 / 95"
        [SerializeField] private Image    _hpBarFill;

        // ---------------------------------------------------------------------------
        // Inspector references — turn controls
        // ---------------------------------------------------------------------------

        [Header("Turn Controls")]
        [SerializeField] private Button   _endTurnButton;
        [SerializeField] private TMP_Text _turnPhaseLabel; // "YOUR TURN" / "WAITING..."

        // ---------------------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------------------

        private string _displayedUnitId;
        private string _controlledPlayerId;

        // Cached spell lists for the currently displayed unit (avoids repeated registry lookups).
        private SpellRuntimeData[] _displayedUnitSpells;

        // Cached event handlers so they can be unsubscribed in OnDestroy.
        private Action<TemperatureChangedEvent>    _onTemperatureChanged;
        private Action<UnitDamagedEvent>           _onUnitDamaged;
        private Action<UnitHealedEvent>            _onUnitHealed;
        private Action<ThermalComposureUsedEvent>  _onThermalComposureUsed;
        private Action<HeatstrokeTickEvent>        _onHeatstrokeTick;
        private Action<TurnResolvedEvent>          _onTurnResolved;

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            // Determine controlled player. In a real implementation this comes from a
            // session/lobby system; placeholder uses the first player ID for local play.
            _controlledPlayerId = "p1";

            // Build handler delegates and subscribe to the event bus.
            _onTemperatureChanged   = OnTemperatureChanged;
            _onUnitDamaged          = OnUnitDamaged;
            _onUnitHealed           = OnUnitHealed;
            _onThermalComposureUsed = OnThermalComposureUsed;
            _onHeatstrokeTick       = OnHeatstrokeTick;
            _onTurnResolved         = OnTurnResolved;

            SimulationEventBus.Subscribe(_onTemperatureChanged);
            SimulationEventBus.Subscribe(_onUnitDamaged);
            SimulationEventBus.Subscribe(_onUnitHealed);
            SimulationEventBus.Subscribe(_onThermalComposureUsed);
            SimulationEventBus.Subscribe(_onHeatstrokeTick);
            SimulationEventBus.Subscribe(_onTurnResolved);

            // Wire turn-control buttons.
            if (_endTurnButton != null)
                _endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);

            if (_thermalComposureButton != null)
                _thermalComposureButton.onClick.AddListener(OnThermalComposureButtonClicked);
        }

        private void Start()
        {
            // Subscribe to unit selection from the input controller.
            if (_input != null)
                _input.OnUnitSelected += ShowUnitInfo;

            // Set default turn phase label.
            SetTurnPhaseLabel(TurnPhase.Planning);
        }

        private void OnDestroy()
        {
            // Unsubscribe all handlers to prevent stale callbacks after scene unload.
            SimulationEventBus.Unsubscribe(_onTemperatureChanged);
            SimulationEventBus.Unsubscribe(_onUnitDamaged);
            SimulationEventBus.Unsubscribe(_onUnitHealed);
            SimulationEventBus.Unsubscribe(_onThermalComposureUsed);
            SimulationEventBus.Unsubscribe(_onHeatstrokeTick);
            SimulationEventBus.Unsubscribe(_onTurnResolved);

            if (_input != null)
                _input.OnUnitSelected -= ShowUnitInfo;
        }

        // ---------------------------------------------------------------------------
        // Public API — called by PlayerInputController on unit selection
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Populates all HUD panels to reflect the selected unit.
        /// </summary>
        /// <param name="unitId">Runtime ID of the newly selected unit.</param>
        public void ShowUnitInfo(string unitId)
        {
            if (_sim == null) return;

            _displayedUnitId = unitId;
            UnitState unit = _sim.State.GetUnit(unitId);
            if (unit == null) return;

            // Resolve the Mancer's spell list via the bootstrapper's data registry.
            _displayedUnitSpells = ResolveMancerSpells(unit.MancerArchetypeId);

            RefreshUnitPanel(unit);
            RefreshApDisplay(unit.ActionPoints);
            _thermometerBar.SetTemperature(unit.Temperature);
            RefreshThermalComposureButton();
            RefreshSpellButtons(unit);
        }

        // ---------------------------------------------------------------------------
        // SimulationEventBus handlers
        // ---------------------------------------------------------------------------

        private void OnTemperatureChanged(TemperatureChangedEvent e)
        {
            if (e.UnitId != _displayedUnitId) return;
            _thermometerBar.SetTemperature(e.NewTemperature);
        }

        private void OnUnitDamaged(UnitDamagedEvent e)
        {
            if (e.UnitId != _displayedUnitId) return;
            UnitState unit = _sim?.State.GetUnit(_displayedUnitId);
            if (unit == null) return;
            RefreshHpDisplay(unit);
        }

        private void OnUnitHealed(UnitHealedEvent e)
        {
            if (e.UnitId != _displayedUnitId) return;
            UnitState unit = _sim?.State.GetUnit(_displayedUnitId);
            if (unit == null) return;
            RefreshHpDisplay(unit);
        }

        private void OnThermalComposureUsed(ThermalComposureUsedEvent e)
        {
            if (e.PlayerId != _controlledPlayerId) return;
            RefreshThermalComposureButton();
        }

        private void OnHeatstrokeTick(HeatstrokeTickEvent e)
        {
            if (e.UnitId != _displayedUnitId) return;
            // AP penalty has been applied; refresh the pip display from live unit state.
            UnitState unit = _sim?.State.GetUnit(_displayedUnitId);
            if (unit == null) return;
            RefreshApDisplay(unit.ActionPoints);
        }

        private void OnTurnResolved(TurnResolvedEvent e)
        {
            // After turn resolution the planning phase begins again.
            SetTurnPhaseLabel(TurnPhase.Planning);

            // Refresh spell button cooldown state for the displayed unit.
            UnitState unit = _sim?.State.GetUnit(_displayedUnitId);
            if (unit != null)
                RefreshSpellButtons(unit);
        }

        // ---------------------------------------------------------------------------
        // Button handlers
        // ---------------------------------------------------------------------------

        private void OnEndTurnButtonClicked()
        {
            // Delegate to input controller or a TurnManager adapter once those exist.
            // Placeholder: set phase label to waiting.
            SetTurnPhaseLabel(TurnPhase.Locked);
        }

        private void OnThermalComposureButtonClicked()
        {
            if (_displayedUnitId == null) return;
            if (_sim == null) return;

            // Issue the ThermalComposureCommand via the input controller / command dispatcher.
            // The actual command execution is handled by the simulation layer; the HUD only
            // requests it. Concrete wiring happens when the command dispatcher is available.
            _input?.RequestThermalComposure(_displayedUnitId);
        }

        // ---------------------------------------------------------------------------
        // Panel refresh helpers
        // ---------------------------------------------------------------------------

        private void RefreshUnitPanel(UnitState unit)
        {
            if (_unitNameLabel != null)
                _unitNameLabel.text = unit.Id; // Display ID until DisplayName is surfaced on UnitState.

            RefreshHpDisplay(unit);
        }

        private void RefreshHpDisplay(UnitState unit)
        {
            if (_hpLabel != null)
                _hpLabel.text = $"HP: {unit.CurrentHP} / {unit.MaxHP}";

            if (_hpBarFill != null)
                _hpBarFill.fillAmount = unit.MaxHP > 0
                    ? Mathf.Max(HpBarMinFill, (float)unit.CurrentHP / unit.MaxHP)
                    : HpBarMinFill;
        }

        private void RefreshApDisplay(int currentAp)
        {
            for (int i = 0; i < _apPips.Count && i < MaxApPips; i++)
                _apPips[i].color = i < currentAp ? _pipActiveColor : _pipInactiveColor;
        }

        private void RefreshSpellButtons(UnitState unit)
        {
            SpellRuntimeData[] spells = _displayedUnitSpells ?? Array.Empty<SpellRuntimeData>();

            for (int i = 0; i < _spellButtons.Count && i < MaxSpellButtons; i++)
            {
                SpellButtonView button = _spellButtons[i];
                if (button == null) continue;

                if (i < spells.Length)
                {
                    SpellRuntimeData spell = spells[i];
                    int capturedIndex = i;
                    button.gameObject.SetActive(true);
                    button.Setup(spell, capturedIndex, OnSpellButtonClicked);

                    bool canAfford = unit.ActionPoints >= spell.ApCost;
                    int  cooldown  = unit.SpellCooldowns.TryGetValue(spell.SpellId, out int cd) ? cd : 0;
                    button.SetAvailable(available: true, canAfford: canAfford, cooldownLeft: cooldown);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
        }

        private void RefreshThermalComposureButton()
        {
            if (_sim == null) return;

            bool available = _sim.State.HasThermalComposure(_controlledPlayerId);

            if (_thermalComposureButton != null)
                _thermalComposureButton.interactable = available && _displayedUnitId != null;

            if (_thermalComposureLabel != null)
                _thermalComposureLabel.text = available ? "COMPOSURE (1)" : "COMPOSURE (used)";
        }

        private void SetTurnPhaseLabel(TurnPhase phase)
        {
            if (_turnPhaseLabel == null) return;
            _turnPhaseLabel.text = phase switch
            {
                TurnPhase.Planning   => "YOUR TURN",
                TurnPhase.Locked     => "WAITING...",
                TurnPhase.Resolving  => "RESOLVING",
                TurnPhase.Ended      => "TURN ENDED",
                _                    => string.Empty
            };
        }

        // ---------------------------------------------------------------------------
        // Spell input
        // ---------------------------------------------------------------------------

        private void OnSpellButtonClicked(int spellIndex)
        {
            if (_displayedUnitSpells == null || spellIndex >= _displayedUnitSpells.Length) return;
            SpellRuntimeData spell = _displayedUnitSpells[spellIndex];
            _input?.RequestSpellCast(_displayedUnitId, spell.SpellId);
        }

        // ---------------------------------------------------------------------------
        // Data resolution
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Retrieves the spell list for a Mancer archetype via the bootstrapper's data registry.
        /// Returns an empty array if the archetype ID is null or not found.
        /// </summary>
        private SpellRuntimeData[] ResolveMancerSpells(string mancerArchetypeId)
        {
            if (string.IsNullOrEmpty(mancerArchetypeId)) return Array.Empty<SpellRuntimeData>();
            MancerRuntimeData mancerData = _sim?.DataRegistry?.GetMancer(mancerArchetypeId);
            return mancerData?.Spells ?? Array.Empty<SpellRuntimeData>();
        }
    }
}

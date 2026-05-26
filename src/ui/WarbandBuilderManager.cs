using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Battlemancers.Core.Data;
using Battlemancers.Data;
using Battlemancers.Unity;

namespace Battlemancers.UI
{
    /// <summary>
    /// Main orchestrator for the warband list-builder interface.
    ///
    /// Manages an in-memory <see cref="WarbandSave"/> working copy, drives all sub-views
    /// (<see cref="MancerCardSelectable"/>, <see cref="WarbandSlotView"/>,
    /// <see cref="PointBudgetDisplay"/>), validates the warband on every change, and
    /// delegates persistence to <see cref="WarbandRepository"/>.
    ///
    /// Call <see cref="StartNew"/> or <see cref="LoadForEdit"/> from the scene that opens
    /// the builder. On success, <see cref="OnWarbandSaved"/> fires with the saved warband.
    ///
    /// Architecture rules:
    /// - No Update() polling — all state changes are mutation-driven.
    /// - Single <see cref="RefreshAll"/> call after every mutation; no partial refreshes.
    /// - Cards destroyed/recreated only in <see cref="RebuildCardGrid"/>, not on every refresh.
    /// - No FindObjectOfType — all dependencies wired via [SerializeField].
    ///
    /// Unity only — do not reference from pure-C# simulation code.
    /// </summary>
    public class WarbandBuilderManager : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Point cost constants — single source of truth
        // ---------------------------------------------------------------------------

        private const int MancerBaseCost = 100;
        private const int ChaffT1Cost    = 10;
        private const int ChaffT2Cost    = 20;
        private const int RangedT1Cost   = 25;
        private const int RangedT2Cost   = 50;
        private const int MaxMancerSlots = 3;
        private const int MaxBudget      = 1000;

        // Unit ID suffixes matching the convention "{factionId}_{role}_{tier}".
        private const string ChaffT1Suffix  = "_chaff_t1";
        private const string ChaffT2Suffix  = "_chaff_t2";
        private const string RangedT1Suffix = "_ranged_t1";
        private const string RangedT2Suffix = "_ranged_t2";

        private const string DefaultFactionId   = "gilded_throne";
        private const string DefaultWarbandName = "New Warband";

        // ---------------------------------------------------------------------------
        // Inspector references
        // ---------------------------------------------------------------------------

        [Header("Simulation / Data")]
        [SerializeField] private DataRegistry _dataRegistry;

        [Header("Budget")]
        [SerializeField] private PointBudgetDisplay _budgetDisplay;

        [Header("Mancer Slots")]
        [SerializeField] private WarbandSlotView[] _mancerSlots; // length 3; assign in Inspector

        [Header("Roster Grid")]
        [SerializeField] private Transform  _mancerCardContainer;
        [SerializeField] private GameObject _mancerCardPrefab; // must have MancerCardSelectable component

        [Header("Support Unit Steppers")]
        [SerializeField] private TMP_Text _chaffT1CountLabel;
        [SerializeField] private TMP_Text _chaffT2CountLabel;
        [SerializeField] private TMP_Text _rangedT1CountLabel;
        [SerializeField] private TMP_Text _rangedT2CountLabel;

        [Header("Controls")]
        [SerializeField] private TMP_InputField _warbandNameField;
        [SerializeField] private Button         _saveButton;
        [SerializeField] private TMP_Text       _validationMessageLabel;

        // ---------------------------------------------------------------------------
        // Public events
        // ---------------------------------------------------------------------------

        /// <summary>Fired after a successful save. Passes the saved <see cref="WarbandSave"/>.</summary>
        public event Action<WarbandSave> OnWarbandSaved;

        /// <summary>Fired when the player cancels out of the builder without saving.</summary>
        public event Action OnCancelled;

        // ---------------------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------------------

        private WarbandSave       _workingCopy;
        private WarbandRepository _repository;

        private readonly List<MancerCardSelectable> _mancerCards = new List<MancerCardSelectable>();

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            // Wire Mancer slot events. Slots must be pre-assigned in the Inspector.
            if (_mancerSlots != null)
            {
                for (int i = 0; i < _mancerSlots.Length; i++)
                {
                    if (_mancerSlots[i] == null) continue;
                    _mancerSlots[i].Initialize(i);
                    _mancerSlots[i].OnRemoveClicked       += OnSlotRemoveClicked;
                    _mancerSlots[i].OnEditUpgradesClicked += OnSlotEditUpgradesClicked;
                }
            }

            if (_saveButton != null)
                _saveButton.onClick.AddListener(OnSaveClicked);
        }

        private void OnDestroy()
        {
            if (_mancerSlots != null)
            {
                foreach (var slot in _mancerSlots)
                {
                    if (slot == null) continue;
                    slot.OnRemoveClicked       -= OnSlotRemoveClicked;
                    slot.OnEditUpgradesClicked -= OnSlotEditUpgradesClicked;
                }
            }

            foreach (var card in _mancerCards)
            {
                if (card != null)
                    card.OnCardClicked -= OnMancerCardClicked;
            }
        }

        // ---------------------------------------------------------------------------
        // Public entry points
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Begins building a brand-new warband from scratch.
        /// </summary>
        /// <param name="savePath">
        /// Absolute path to the directory in which warband JSON files are stored.
        /// Typically <c>Application.persistentDataPath + "/warbands"</c>.
        /// </param>
        public void StartNew(string savePath)
        {
            _repository  = new WarbandRepository(savePath);
            _workingCopy = WarbandSave.CreateNew(DefaultFactionId, DefaultWarbandName);

            if (_warbandNameField != null)
                _warbandNameField.text = _workingCopy.displayName;

            RebuildCardGrid();
            RefreshAll();
        }

        /// <summary>
        /// Opens an existing warband for editing.
        /// </summary>
        /// <param name="existing">
        /// The save to edit in-place. The caller should keep their own copy if rollback is required.
        /// </param>
        /// <param name="savePath">Absolute path to the warband save directory.</param>
        public void LoadForEdit(WarbandSave existing, string savePath)
        {
            _repository  = new WarbandRepository(savePath);
            _workingCopy = existing;

            if (_warbandNameField != null)
                _warbandNameField.text = _workingCopy.displayName;

            RebuildCardGrid();
            RefreshAll();
        }

        // ---------------------------------------------------------------------------
        // Card grid — rebuilt once on entry, not on every refresh
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Destroys all existing roster cards and instantiates one
        /// <see cref="MancerCardSelectable"/> per entry in <see cref="DataRegistry.AllMancers"/>.
        /// </summary>
        private void RebuildCardGrid()
        {
            foreach (var card in _mancerCards)
            {
                if (card != null)
                {
                    card.OnCardClicked -= OnMancerCardClicked;
                    Destroy(card.gameObject);
                }
            }
            _mancerCards.Clear();

            if (_dataRegistry == null || _mancerCardPrefab == null || _mancerCardContainer == null)
            {
                Debug.LogWarning("[WarbandBuilderManager] Cannot build card grid — missing DataRegistry, prefab, or container.");
                return;
            }

            foreach (var kvp in _dataRegistry.AllMancers)
            {
                MancerRuntimeData data = kvp.Value;
                GameObject go = Instantiate(_mancerCardPrefab, _mancerCardContainer);
                var card = go.GetComponent<MancerCardSelectable>();
                if (card == null)
                {
                    Debug.LogWarning($"[WarbandBuilderManager] MancerCardPrefab is missing a MancerCardSelectable component. Skipping '{data.MancerId}'.");
                    Destroy(go);
                    continue;
                }

                card.Setup(data);
                card.OnCardClicked += OnMancerCardClicked;
                _mancerCards.Add(card);
            }
        }

        // ---------------------------------------------------------------------------
        // Mancer slot interaction
        // ---------------------------------------------------------------------------

        private void OnMancerCardClicked(string mancerId)
        {
            if (_workingCopy == null) return;

            // If already in a slot, remove it (toggle off).
            int existingIndex = _workingCopy.mancers.FindIndex(
                m => string.Equals(m.mancerArchetypeId, mancerId, StringComparison.OrdinalIgnoreCase));

            if (existingIndex >= 0)
            {
                _workingCopy.mancers.RemoveAt(existingIndex);
            }
            else if (_workingCopy.mancers.Count < MaxMancerSlots)
            {
                // Assign to the next available slot.
                _workingCopy.mancers.Add(new MancerLoadout
                {
                    mancerArchetypeId = mancerId,
                    upgradeIds        = new List<UpgradeRef>()
                });
            }
            // If all slots are filled and card is unselected, do nothing —
            // the card is dimmed and non-interactable via RefreshCardAvailability.

            RefreshAll();
        }

        private void OnSlotRemoveClicked(int slotIndex)
        {
            if (_workingCopy == null) return;
            if (slotIndex < 0 || slotIndex >= _workingCopy.mancers.Count) return;

            _workingCopy.mancers.RemoveAt(slotIndex);
            RefreshAll();
        }

        private void OnSlotEditUpgradesClicked(int slotIndex)
        {
            // Upgrade editing is handled by a separate panel outside this class's scope.
            // A future UpgradeEditorManager will listen for this log or a paired event.
            Debug.Log($"[WarbandBuilderManager] Edit upgrades requested for slot {slotIndex}.");
        }

        // ---------------------------------------------------------------------------
        // Refresh pipeline — called after every mutation
        // ---------------------------------------------------------------------------

        private void RefreshAll()
        {
            RefreshSlots();
            RefreshCardAvailability();
            RefreshBudget();
            RefreshSupportUnitLabels();
            RefreshValidation();
        }

        private void RefreshSlots()
        {
            if (_mancerSlots == null) return;

            for (int i = 0; i < MaxMancerSlots; i++)
            {
                if (i >= _mancerSlots.Length || _mancerSlots[i] == null) continue;

                if (i < _workingCopy.mancers.Count)
                {
                    MancerLoadout loadout = _workingCopy.mancers[i];
                    MancerRuntimeData data = _dataRegistry != null
                        ? _dataRegistry.GetMancer(loadout.mancerArchetypeId)
                        : null;

                    if (data != null)
                    {
                        List<string> upgradeIds = BuildUpgradeIdList(loadout);
                        _mancerSlots[i].SetMancer(data, upgradeIds, loadout.TotalCost);
                    }
                    else
                    {
                        // Data not found (JSON missing) — show empty to avoid a broken display.
                        _mancerSlots[i].SetEmpty();
                    }
                }
                else
                {
                    _mancerSlots[i].SetEmpty();
                }
            }
        }

        private void RefreshCardAvailability()
        {
            bool slotsAvailable = _workingCopy.mancers.Count < MaxMancerSlots;

            foreach (var card in _mancerCards)
            {
                if (card == null) continue;

                int slotIndex = _workingCopy.mancers.FindIndex(
                    m => string.Equals(m.mancerArchetypeId, card.MancerId, StringComparison.OrdinalIgnoreCase));

                bool isSelected = slotIndex >= 0;
                card.SetSelected(isSelected, slotIndex);
                card.SetAvailable(isSelected || slotsAvailable);
            }
        }

        private void RefreshBudget()
        {
            if (_budgetDisplay != null)
                _budgetDisplay.SetCost(ComputeTotalCost());
        }

        private void RefreshSupportUnitLabels()
        {
            if (_chaffT1CountLabel  != null) _chaffT1CountLabel.text  = GetSupportUnitCount(ChaffT1Suffix).ToString();
            if (_chaffT2CountLabel  != null) _chaffT2CountLabel.text  = GetSupportUnitCount(ChaffT2Suffix).ToString();
            if (_rangedT1CountLabel != null) _rangedT1CountLabel.text = GetSupportUnitCount(RangedT1Suffix).ToString();
            if (_rangedT2CountLabel != null) _rangedT2CountLabel.text = GetSupportUnitCount(RangedT2Suffix).ToString();
        }

        private void RefreshValidation()
        {
            List<string> errors = ValidateWarband();
            bool isValid = errors.Count == 0;

            if (_saveButton != null)
                _saveButton.interactable = isValid;

            if (_validationMessageLabel != null)
            {
                _validationMessageLabel.text = isValid ? string.Empty : string.Join("\n", errors);
                _validationMessageLabel.gameObject.SetActive(!isValid);
            }
        }

        // ---------------------------------------------------------------------------
        // Cost calculation
        // ---------------------------------------------------------------------------

        private int ComputeTotalCost()
        {
            int total = 0;

            foreach (var loadout in _workingCopy.mancers)
                total += loadout.TotalCost; // MancerBaseCost (100) + sum of upgrade additionalCosts

            total += GetSupportUnitCount(ChaffT1Suffix)  * ChaffT1Cost;
            total += GetSupportUnitCount(ChaffT2Suffix)  * ChaffT2Cost;
            total += GetSupportUnitCount(RangedT1Suffix) * RangedT1Cost;
            total += GetSupportUnitCount(RangedT2Suffix) * RangedT2Cost;

            return total;
        }

        // ---------------------------------------------------------------------------
        // Validation
        // ---------------------------------------------------------------------------

        private List<string> ValidateWarband()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(_workingCopy.displayName))
                errors.Add("Warband must have a name.");

            if (_workingCopy.mancers == null || _workingCopy.mancers.Count == 0)
                errors.Add("At least one Mancer is required.");

            if (ComputeTotalCost() > MaxBudget)
                errors.Add($"Total cost exceeds {MaxBudget} pts.");

            return errors;
        }

        // ---------------------------------------------------------------------------
        // Save / Cancel
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Validates and persists the working warband. Fires <see cref="OnWarbandSaved"/> on success.
        /// Bound to the save button in <see cref="Awake"/>.
        /// </summary>
        public void OnSaveClicked()
        {
            if (_workingCopy == null || _repository == null) return;
            if (ValidateWarband().Count > 0) return;

            // Sync the name field into the working copy before saving.
            if (_warbandNameField != null)
            {
                string trimmed = _warbandNameField.text.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                    _workingCopy.displayName = trimmed;
            }

            _repository.Save(_workingCopy);
            OnWarbandSaved?.Invoke(_workingCopy);
        }

        /// <summary>Fires <see cref="OnCancelled"/> without saving.</summary>
        public void OnCancelClicked()
        {
            OnCancelled?.Invoke();
        }

        // ---------------------------------------------------------------------------
        // Stepper button handlers — one increment / decrement per support unit type
        // ---------------------------------------------------------------------------

        /// <summary>Adds one T1 Chaff unit to the warband.</summary>
        public void OnChaffT1Increment() { AdjustSupportUnitCount(ChaffT1Suffix,  ChaffT1Cost,  +1); RefreshAll(); }

        /// <summary>Removes one T1 Chaff unit from the warband (minimum 0).</summary>
        public void OnChaffT1Decrement() { AdjustSupportUnitCount(ChaffT1Suffix,  ChaffT1Cost,  -1); RefreshAll(); }

        /// <summary>Adds one T2 Chaff unit to the warband.</summary>
        public void OnChaffT2Increment() { AdjustSupportUnitCount(ChaffT2Suffix,  ChaffT2Cost,  +1); RefreshAll(); }

        /// <summary>Removes one T2 Chaff unit from the warband (minimum 0).</summary>
        public void OnChaffT2Decrement() { AdjustSupportUnitCount(ChaffT2Suffix,  ChaffT2Cost,  -1); RefreshAll(); }

        /// <summary>Adds one T1 Ranged unit to the warband.</summary>
        public void OnRangedT1Increment() { AdjustSupportUnitCount(RangedT1Suffix, RangedT1Cost, +1); RefreshAll(); }

        /// <summary>Removes one T1 Ranged unit from the warband (minimum 0).</summary>
        public void OnRangedT1Decrement() { AdjustSupportUnitCount(RangedT1Suffix, RangedT1Cost, -1); RefreshAll(); }

        /// <summary>Adds one T2 Ranged unit to the warband.</summary>
        public void OnRangedT2Increment() { AdjustSupportUnitCount(RangedT2Suffix, RangedT2Cost, +1); RefreshAll(); }

        /// <summary>Removes one T2 Ranged unit from the warband (minimum 0).</summary>
        public void OnRangedT2Decrement() { AdjustSupportUnitCount(RangedT2Suffix, RangedT2Cost, -1); RefreshAll(); }

        // ---------------------------------------------------------------------------
        // Support unit helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Adjusts the count of the support unit type identified by <paramref name="unitIdSuffix"/>
        /// by <paramref name="delta"/>. Clamps to a minimum of 0.
        /// Creates a new <see cref="SupportUnitCount"/> entry if none exists for that unit type.
        /// Removes the entry cleanly when the count reaches 0.
        /// </summary>
        private void AdjustSupportUnitCount(string unitIdSuffix, int unitPointCost, int delta)
        {
            string unitId = BuildUnitId(unitIdSuffix);

            int index = _workingCopy.supportUnits.FindIndex(
                u => string.Equals(u.unitId, unitId, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                SupportUnitCount entry = _workingCopy.supportUnits[index];
                int newCount = Mathf.Max(0, entry.count + delta);
                if (newCount == 0)
                {
                    _workingCopy.supportUnits.RemoveAt(index);
                }
                else
                {
                    entry.count = newCount;
                    _workingCopy.supportUnits[index] = entry;
                }
            }
            else if (delta > 0)
            {
                _workingCopy.supportUnits.Add(new SupportUnitCount
                {
                    unitId        = unitId,
                    unitPointCost = unitPointCost,
                    count         = delta
                });
            }
            // delta <= 0 with no existing entry: nothing to do.
        }

        /// <summary>Returns the current count for the support unit type matching the given ID suffix, or 0.</summary>
        private int GetSupportUnitCount(string unitIdSuffix)
        {
            string unitId = BuildUnitId(unitIdSuffix);
            int index = _workingCopy.supportUnits.FindIndex(
                u => string.Equals(u.unitId, unitId, StringComparison.OrdinalIgnoreCase));
            return index >= 0 ? _workingCopy.supportUnits[index].count : 0;
        }

        /// <summary>
        /// Builds the full unit ID from the working copy's faction ID and the given suffix.
        /// Example: faction "gilded_throne" + suffix "_chaff_t1" → "gilded_throne_chaff_t1".
        /// </summary>
        private string BuildUnitId(string suffix) =>
            (_workingCopy?.factionId ?? DefaultFactionId) + suffix;

        /// <summary>
        /// Extracts a flat list of upgrade ID strings from a <see cref="MancerLoadout"/>.
        /// Used when populating <see cref="WarbandSlotView.SetMancer"/>.
        /// </summary>
        private static List<string> BuildUpgradeIdList(MancerLoadout loadout)
        {
            var ids = new List<string>();
            if (loadout.upgradeIds == null) return ids;
            foreach (var upgrade in loadout.upgradeIds)
            {
                if (!string.IsNullOrEmpty(upgrade.upgradeId))
                    ids.Add(upgrade.upgradeId);
            }
            return ids;
        }
    }
}

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
    /// Main orchestrator for the warband builder screen.
    ///
    /// Owns the in-memory <see cref="WarbandDraft"/> and coordinates all sub-components:
    /// roster browser, warband slot list, point budget display, and bottom-bar controls.
    ///
    /// All UI refreshes are driven by user actions — no Update() polling.
    /// Unity only — do not reference from pure-C# simulation code.
    /// </summary>
    public class WarbandBuilderManager : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        private const int MaxMancers  = 3;
        private const int PointBudget = 1000;

        // ---------------------------------------------------------------------------
        // Inspector references
        // ---------------------------------------------------------------------------

        [SerializeField] private DataRegistry _dataRegistry;

        // WarbandRepository is a plain C# class (not a MonoBehaviour) — injected in Awake.
        private WarbandRepository _repository;

        [Header("Sub-components")]
        [SerializeField] private PointBudgetDisplay _budgetDisplay;
        [SerializeField] private Transform          _mancerRosterContainer; // parent for MancerCardView prefabs
        [SerializeField] private Transform          _warbandSlotsContainer; // parent for WarbandSlotView prefabs
        [SerializeField] private GameObject         _mancerCardPrefab;      // must have MancerCardView component
        [SerializeField] private GameObject         _warbandSlotPrefab;     // must have WarbandSlotView component

        [Header("Bottom Bar")]
        [SerializeField] private TMP_InputField _warbandNameField;
        [SerializeField] private Button         _btnSave;
        [SerializeField] private Button         _btnNew;
        [SerializeField] private Button         _btnLoad;
        [SerializeField] private Button         _btnBack;
        [SerializeField] private TMP_Text       _validationMessage;

        // ---------------------------------------------------------------------------
        // State
        // ---------------------------------------------------------------------------

        private WarbandDraft _draft;

        // Cached card views for roster browser so SetSelected/SetAvailable can be applied
        // without re-instantiating cards on every draft change.
        private readonly List<MancerCardView> _rosterCards = new List<MancerCardView>();

        // Cached slot views for the active warband panel.
        private readonly List<WarbandSlotView> _slotViews = new List<WarbandSlotView>();

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            _repository = new WarbandRepository(
                System.IO.Path.Combine(Application.persistentDataPath, "warbands"));

            _btnSave.onClick.AddListener(SaveWarband);
            _btnNew.onClick.AddListener(NewWarband);
            _btnLoad.onClick.AddListener(OpenLoadOverlay);
            _btnBack.onClick.AddListener(GoBack);
            _warbandNameField.onEndEdit.AddListener(OnNameChanged);
        }

        private void Start()
        {
            NewWarband();
            PopulateRosterBrowser();
        }

        // ---------------------------------------------------------------------------
        // Draft lifecycle
        // ---------------------------------------------------------------------------

        private void NewWarband()
        {
            _draft = new WarbandDraft();
            if (_warbandNameField != null)
                _warbandNameField.text = _draft.Name;
            RefreshAllUI();
        }

        // ---------------------------------------------------------------------------
        // Roster browser
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Clears the roster container and instantiates one <see cref="MancerCardView"/>
        /// per entry in <see cref="DataRegistry.AllMancers"/>. Safe to call once at startup.
        /// </summary>
        private void PopulateRosterBrowser()
        {
            if (_mancerRosterContainer == null || _mancerCardPrefab == null) return;

            // Destroy any existing cards (e.g. if called again after a data reload).
            foreach (var card in _rosterCards)
            {
                if (card != null) Destroy(card.gameObject);
            }
            _rosterCards.Clear();

            foreach (var kvp in _dataRegistry.AllMancers)
            {
                MancerRuntimeData data = kvp.Value;
                GameObject go = Instantiate(_mancerCardPrefab, _mancerRosterContainer);
                var cardView = go.GetComponent<MancerCardView>();
                if (cardView == null)
                {
                    Debug.LogError($"[WarbandBuilderManager] MancerCardPrefab is missing a MancerCardView component on '{go.name}'.");
                    Destroy(go);
                    continue;
                }
                cardView.Setup(data, AddMancer);
                _rosterCards.Add(cardView);
            }
        }

        // ---------------------------------------------------------------------------
        // Public draft mutation API (called by card/slot views)
        // ---------------------------------------------------------------------------

        /// <summary>Adds a Mancer to the draft. No-op if the draft already contains it or is full.</summary>
        public void AddMancer(string mancerId)
        {
            if (_draft.Mancers.Count >= MaxMancers) return;
            if (_draft.Mancers.Exists(m => m.MancerId == mancerId)) return;
            _draft.Mancers.Add(new WarbandMancerDraft { MancerId = mancerId });
            RefreshAllUI();
        }

        /// <summary>Removes the Mancer with <paramref name="mancerId"/> from the draft. No-op if not present.</summary>
        public void RemoveMancer(string mancerId)
        {
            _draft.Mancers.RemoveAll(m => m.MancerId == mancerId);
            RefreshAllUI();
        }

        /// <summary>
        /// Sets the count for a support unit type. Creates a new entry if none exists.
        /// A count of 0 removes the entry.
        /// </summary>
        /// <param name="unitTypeId">The unit type ID (e.g., "gilded_throne_chaff_t1").</param>
        /// <param name="tier">Tier of the unit (1 or 2).</param>
        /// <param name="count">Number of units; must be >= 0.</param>
        public void SetSupportCount(string unitTypeId, int tier, int count)
        {
            if (count < 0) count = 0;

            var existing = _draft.SupportUnits.Find(s => s.UnitTypeId == unitTypeId && s.Tier == tier);

            if (existing != null)
            {
                if (count == 0)
                    _draft.SupportUnits.Remove(existing);
                else
                    existing.Count = count;
            }
            else if (count > 0)
            {
                _draft.SupportUnits.Add(new WarbandSupportDraft
                {
                    UnitTypeId   = unitTypeId,
                    Tier         = tier,
                    Count        = count,
                    CostPerUnit  = ResolveSupportUnitCost(unitTypeId, tier)
                });
            }

            RefreshAllUI();
        }

        // ---------------------------------------------------------------------------
        // UI refresh
        // ---------------------------------------------------------------------------

        private void RefreshAllUI()
        {
            int  total = ComputeTotalPoints();
            bool valid = Validate(out string reason);

            if (_budgetDisplay != null)
                _budgetDisplay.SetCost(total);

            if (_btnSave != null)
                _btnSave.interactable = valid;

            if (_validationMessage != null)
            {
                _validationMessage.text = valid ? "" : reason;
                _validationMessage.gameObject.SetActive(!valid);
            }

            RefreshRosterCardStates();
            RefreshWarbandSlots();
        }

        /// <summary>
        /// Updates each roster card's in-warband badge and add-button state.
        /// Unselected cards in a full warband remain clickable in the view but AddMancer
        /// guards against over-filling via its early-exit check — no double-disabling needed.
        /// One SetInWarband call per card per refresh.
        /// </summary>
        private void RefreshRosterCardStates()
        {
            foreach (var card in _rosterCards)
            {
                if (card == null) continue;
                bool inDraft = _draft.Mancers.Exists(m => m.MancerId == card.MancerId);
                card.SetInWarband(inDraft);
            }
        }

        /// <summary>
        /// Destroys and re-creates the warband slot views to match the current draft.
        /// Slot views are lightweight and infrequently rebuilt, so full reconstruction is acceptable.
        /// WarbandSlotView uses an event-based API: Initialize(slotIndex) then SetMancer/SetEmpty,
        /// with OnRemoveClicked/OnEditUpgradesClicked events subscribed here.
        /// </summary>
        private void RefreshWarbandSlots()
        {
            if (_warbandSlotsContainer == null || _warbandSlotPrefab == null) return;

            foreach (var slotView in _slotViews)
            {
                if (slotView != null) Destroy(slotView.gameObject);
            }
            _slotViews.Clear();

            for (int i = 0; i < MaxMancers; i++)
            {
                GameObject go = Instantiate(_warbandSlotPrefab, _warbandSlotsContainer);
                var slotView = go.GetComponent<WarbandSlotView>();
                if (slotView == null)
                {
                    Debug.LogError($"[WarbandBuilderManager] WarbandSlotPrefab is missing a WarbandSlotView component on '{go.name}'.");
                    Destroy(go);
                    continue;
                }

                slotView.Initialize(i);

                slotView.OnRemoveClicked       += slotIndex => OnSlotRemoveClicked(slotIndex);
                slotView.OnEditUpgradesClicked  += slotIndex => OnSlotEditUpgradesClicked(slotIndex);

                if (i < _draft.Mancers.Count)
                {
                    WarbandMancerDraft mancerDraft = _draft.Mancers[i];
                    MancerRuntimeData  data        = _dataRegistry.GetMancer(mancerDraft.MancerId);
                    if (data == null)
                    {
                        Debug.LogWarning($"[WarbandBuilderManager] No MancerRuntimeData found for id '{mancerDraft.MancerId}'. Showing empty slot.");
                        slotView.SetEmpty();
                    }
                    else
                    {
                        int upgradeCost = 0;
                        // TODO: sum upgrade costs from mancerDraft.SelectedUpgradeIds when upgrade system is wired
                        slotView.SetMancer(data, mancerDraft.SelectedUpgradeIds, 100 + upgradeCost);
                    }
                }
                else
                {
                    slotView.SetEmpty();
                }

                _slotViews.Add(slotView);
            }
        }

        private void OnSlotRemoveClicked(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _draft.Mancers.Count) return;
            string mancerId = _draft.Mancers[slotIndex].MancerId;
            RemoveMancer(mancerId);
        }

        private void OnSlotEditUpgradesClicked(int slotIndex)
        {
            // TODO: Open upgrade panel overlay for the Mancer in this slot when upgrade system is wired.
            Debug.Log($"[WarbandBuilderManager] EditUpgrades requested for slot {slotIndex} — not yet wired.");
        }

        // ---------------------------------------------------------------------------
        // Point calculation
        // ---------------------------------------------------------------------------

        private int ComputeTotalPoints()
        {
            int total = 0;
            foreach (var m in _draft.Mancers)
            {
                total += 100; // Base Mancer cost is always 100 pts
                // TODO: add upgrade costs when upgrade system is wired (sum m.SelectedUpgradeIds costs)
            }
            foreach (var s in _draft.SupportUnits)
                total += s.CostPerUnit * s.Count;
            return total;
        }

        /// <summary>Returns the per-unit point cost for a given support unit type and tier.</summary>
        private static int ResolveSupportUnitCost(string unitTypeId, int tier)
        {
            // Tier costs per the warband design spec:
            //   T1 Chaff = 10 pts, T2 Chaff = 20 pts
            //   T1 Ranged = 25 pts, T2 Ranged = 50 pts
            bool isRanged = unitTypeId.Contains("ranged");
            if (isRanged)
                return tier == 2 ? 50 : 25;
            return tier == 2 ? 20 : 10; // Chaff / default
        }

        // ---------------------------------------------------------------------------
        // Validation
        // ---------------------------------------------------------------------------

        private bool Validate(out string reason)
        {
            if (_draft.Mancers.Count == 0)
            {
                reason = "At least 1 Mancer required.";
                return false;
            }
            int total = ComputeTotalPoints();
            if (total > PointBudget)
            {
                reason = $"Over budget by {total - PointBudget} pts.";
                return false;
            }
            reason = "";
            return true;
        }

        // ---------------------------------------------------------------------------
        // Bottom-bar actions
        // ---------------------------------------------------------------------------

        private void SaveWarband()
        {
            var save = WarbandSave.CreateNew(_draft.FactionId, _draft.Name);

            foreach (var m in _draft.Mancers)
            {
                var loadout = new MancerLoadout { mancerArchetypeId = m.MancerId };
                // TODO: populate loadout.upgradeIds from m.SelectedUpgradeIds when upgrade system is wired
                save.mancers.Add(loadout);
            }

            foreach (var s in _draft.SupportUnits)
            {
                save.supportUnits.Add(new SupportUnitCount
                {
                    unitId       = s.UnitTypeId,
                    unitPointCost = s.CostPerUnit,
                    count        = s.Count
                });
            }

            _repository.Save(save);
        }

        private void OpenLoadOverlay()
        {
            // TODO: Show saved warband list overlay when WarbandRepository is wired.
            Debug.Log("[WarbandBuilderManager] OpenLoadOverlay — WarbandRepository not yet wired.");
        }

        private void GoBack()
        {
            // TODO: Navigate back via GameModeManager if wired; fall back to SceneManager.
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            Debug.Log("[WarbandBuilderManager] GoBack — scene navigation not yet wired.");
        }

        private void OnNameChanged(string name)
        {
            _draft.Name = name;
        }
    }

    // ================================================================================
    // Internal draft types — mutable working state, separate from serialised WarbandSave
    // ================================================================================

    /// <summary>
    /// Mutable in-memory editing model for a warband being built or edited.
    /// Converted to <c>WarbandSave</c> only on Save. Not a MonoBehaviour.
    /// </summary>
    public class WarbandDraft
    {
        public string Name      { get; set; } = "New Warband";
        public string FactionId { get; set; } = "gilded_throne";
        public List<WarbandMancerDraft>  Mancers      { get; set; } = new List<WarbandMancerDraft>();
        public List<WarbandSupportDraft> SupportUnits { get; set; } = new List<WarbandSupportDraft>();
    }

    /// <summary>One Mancer slot in the draft, including any selected upgrade IDs.</summary>
    public class WarbandMancerDraft
    {
        public string       MancerId           { get; set; }
        public List<string> SelectedUpgradeIds { get; set; } = new List<string>();
    }

    /// <summary>One support unit entry in the draft.</summary>
    public class WarbandSupportDraft
    {
        public string UnitTypeId  { get; set; }
        public int    Tier        { get; set; }
        public int    Count       { get; set; }
        public int    CostPerUnit { get; set; }
    }
}

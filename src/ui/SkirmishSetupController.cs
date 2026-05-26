using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Battlemancers.Core.Data;

namespace Battlemancers.UI
{
    public class SkirmishSetupController : MonoBehaviour
    {
        [SerializeField] private GameModeManager _gameModeManager;
        [SerializeField] private Button _btnBack;
        [SerializeField] private Button _btnStartMatch;

        [Header("AI Difficulty")]
        [SerializeField] private Button _btnRecruit;
        [SerializeField] private Button _btnVeteran;
        [SerializeField] private Button _btnArchmage;

        [Header("Map Selection")]
        [SerializeField] private TMP_Dropdown _mapDropdown;

        [Header("Selected Warband Display")]
        [SerializeField] private TMP_Text _selectedWarbandName;
        [SerializeField] private Button _btnChangeWarband;   // opens warband picker overlay

        private AiDifficulty _selectedDifficulty = AiDifficulty.Veteran;
        private string _selectedMapId = "map_ashfields"; // default map
        private WarbandData _selectedWarband;             // set by warband picker

        private static readonly string[] DefaultMapIds = { "map_ashfields", "map_frostpeak", "map_swamplands" };

        private void Awake()
        {
            _btnBack.onClick.AddListener(_gameModeManager.GoToModeSelect);
            _btnStartMatch.onClick.AddListener(OnStartMatch);
            _btnRecruit.onClick.AddListener(() => SelectDifficulty(AiDifficulty.Recruit));
            _btnVeteran.onClick.AddListener(() => SelectDifficulty(AiDifficulty.Veteran));
            _btnArchmage.onClick.AddListener(() => SelectDifficulty(AiDifficulty.Archmage));
            _btnChangeWarband.onClick.AddListener(OnChangeWarband);

            PopulateMapDropdown();
        }

        private void SelectDifficulty(AiDifficulty difficulty)
        {
            _selectedDifficulty = difficulty;
            // Update button visual states (active/inactive)
        }

        private void PopulateMapDropdown()
        {
            _mapDropdown.ClearOptions();
            _mapDropdown.AddOptions(new System.Collections.Generic.List<string>(DefaultMapIds));
            _mapDropdown.onValueChanged.AddListener(i => _selectedMapId = DefaultMapIds[i]);
        }

        private void OnChangeWarband()
        {
            // TODO: Open warband picker overlay — wire to WarbandRepository when available
        }

        private void OnStartMatch()
        {
            if (_selectedWarband == null)
            {
                Debug.LogWarning("[SkirmishSetupController] No warband selected — cannot start match.");
                return;
            }
            _gameModeManager.StartSkirmishMatch(_selectedWarband, _selectedMapId, _selectedDifficulty);
        }

        // Called by warband picker overlay when player selects a warband
        public void SetSelectedWarband(WarbandData warband)
        {
            _selectedWarband = warband;
            _selectedWarbandName.text = warband?.Name ?? "None";
            _btnStartMatch.interactable = warband != null;
        }
    }
}

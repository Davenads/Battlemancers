using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Battlemancers.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameModeManager _gameModeManager;
        [SerializeField] private Button _btnPlay;            // -> ModeSelect
        [SerializeField] private Button _btnWarbandBuilder;  // -> WarbandBuilder
        [SerializeField] private Button _btnCampaign;        // -> CampaignSelect
        [SerializeField] private Button _btnSettings;        // -> Settings
        [SerializeField] private Button _btnQuit;
        [SerializeField] private TMP_Text _savedWarbandCount; // "3 Warbands Saved"
        [SerializeField] private TMP_Text _versionLabel;

        private void Awake()
        {
            _btnPlay.onClick.AddListener(_gameModeManager.GoToModeSelect);
            _btnWarbandBuilder.onClick.AddListener(_gameModeManager.GoToWarbandBuilder);
            _btnCampaign.onClick.AddListener(_gameModeManager.GoToCampaignSelect);
            _btnSettings.onClick.AddListener(_gameModeManager.GoToSettings);
            _btnQuit.onClick.AddListener(_gameModeManager.QuitGame);
        }

        private void Start()
        {
            _versionLabel.text = $"v{Application.version}";
            RefreshWarbandCount();
        }

        private void RefreshWarbandCount()
        {
            // Load warband count from WarbandRepository if available
            // For now, show a placeholder until WarbandRepository is accessible
            _savedWarbandCount.text = ""; // will be populated once WarbandRepository is wired in
        }
    }
}

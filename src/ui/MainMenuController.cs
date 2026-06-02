using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Battlemancers.Core.Data;

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

        /// <summary>Sub-path under Application.persistentDataPath for warband saves.</summary>
        [SerializeField] private string _warbandSaveSubPath = "saves/warbands";

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
            try
            {
                string saveDirectory = Path.Combine(Application.persistentDataPath, _warbandSaveSubPath);
                var repo = new WarbandRepository(saveDirectory, Debug.LogWarning);
                var warbands = repo.LoadAll();
                int count = warbands.Count;
                _savedWarbandCount.text = count == 1 ? "1 Warband Saved" : $"{count} Warbands Saved";
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MainMenuController] Could not load warband count: {ex.Message}");
                _savedWarbandCount.text = "";
            }
        }
    }
}

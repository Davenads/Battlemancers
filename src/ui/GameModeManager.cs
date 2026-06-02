using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Battlemancers.Core.Data;

namespace Battlemancers.UI
{
    public class GameModeManager : MonoBehaviour
    {
        // Scene name constants — must match Unity Build Settings scene names
        private const string SceneMainMenu        = "MainMenu";
        private const string SceneWarbandBuilder   = "WarbandBuilder";
        private const string SceneModeSelect       = "ModeSelect";
        private const string SceneSkirmishSetup    = "SkirmishSetup";
        private const string SceneMultiplayerLobby = "MultiplayerLobby";
        private const string SceneCampaignSelect   = "CampaignSelect";
        private const string SceneBattle           = "BattleScene";
        private const string SceneSettings         = "Settings";

        /// <summary>
        /// Sub-path under Application.persistentDataPath where campaign save files are stored.
        /// </summary>
        [SerializeField] private string _campaignSaveSubPath = "saves/campaign";

        public void GoToMainMenu()         => LoadScene(SceneMainMenu);
        public void GoToWarbandBuilder()   => LoadScene(SceneWarbandBuilder);
        public void GoToModeSelect()       => LoadScene(SceneModeSelect);
        public void GoToSkirmishSetup()    => LoadScene(SceneSkirmishSetup);
        public void GoToMultiplayerLobby() => LoadScene(SceneMultiplayerLobby);
        public void GoToSettings()         => LoadScene(SceneSettings);

        /// <summary>
        /// Loads the campaign save manifest and stores it in SceneTransitionData, then
        /// navigates to the CampaignSelect scene so the slot picker can display existing saves.
        /// </summary>
        public void GoToCampaignSelect()
        {
            string saveDirectory = Path.Combine(Application.persistentDataPath, _campaignSaveSubPath);
            var repo = new CampaignRepository(saveDirectory, Debug.LogWarning);
            var manifest = repo.LoadManifest();

            var ctx = GetOrCreateTransitionData();
            ctx.CampaignManifest = manifest;

            Debug.Log($"[GameModeManager] Campaign manifest loaded: {manifest.Count} slot(s). Loading {SceneCampaignSelect}.");
            LoadScene(SceneCampaignSelect);
        }

        public void StartSkirmishMatch(WarbandData warband, string mapId, AiDifficulty difficulty)
        {
            var ctx = GetOrCreateTransitionData();
            ctx.SelectedMode           = GameMode.Skirmish;
            ctx.Player1Warband         = warband;
            ctx.SelectedMapId          = mapId;
            ctx.SelectedAiDifficulty   = difficulty;
            LoadScene(SceneBattle);
        }

        public void StartCampaignMission(WarbandData warband, int chapterIndex, string saveSlot)
        {
            var ctx = GetOrCreateTransitionData();
            ctx.SelectedMode            = GameMode.Campaign;
            ctx.Player1Warband          = warband;
            ctx.CampaignChapterIndex    = chapterIndex;
            ctx.CampaignSaveSlot        = saveSlot;
            LoadScene(SceneBattle);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static SceneTransitionData GetOrCreateTransitionData()
        {
            if (SceneTransitionData.Instance != null) return SceneTransitionData.Instance;
            var go = new GameObject("SceneTransitionData");
            return go.AddComponent<SceneTransitionData>();
        }

        private static void LoadScene(string sceneName)
        {
            Debug.Log($"[GameModeManager] Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
    }
}

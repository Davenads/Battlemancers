using System.Collections.Generic;
using UnityEngine;
using Battlemancers.Core.Data;

namespace Battlemancers.UI
{
    /// <summary>
    /// Persists across scene loads to carry match setup data and pre-loaded campaign state.
    /// One instance exists for the application lifetime.
    /// </summary>
    public class SceneTransitionData : MonoBehaviour
    {
        public static SceneTransitionData Instance { get; private set; }

        // Game mode context
        public GameMode SelectedMode { get; set; }
        public AiDifficulty SelectedAiDifficulty { get; set; }
        public string SelectedMapId { get; set; }

        // Warband context — set before loading BattleScene
        public WarbandData Player1Warband { get; set; }
        public WarbandData Player2Warband { get; set; } // null for online / AI (server provides)

        // Campaign context
        public int CampaignChapterIndex { get; set; }
        public string CampaignSaveSlot { get; set; }

        /// <summary>
        /// Pre-loaded campaign save-slot summaries, populated by GameModeManager.GoToCampaignSelect()
        /// before the CampaignSelect scene loads. The slot picker screen reads this list
        /// instead of loading the manifest itself. Null until GoToCampaignSelect() has been called.
        /// </summary>
        public List<CampaignSaveSlot> CampaignManifest { get; set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public enum GameMode { None, Skirmish, Multiplayer, Campaign, CustomGame }
    public enum AiDifficulty { Recruit, Veteran, Archmage }
}

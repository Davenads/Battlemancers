using UnityEngine;
using UnityEngine.UI;

namespace Battlemancers.UI
{
    public class ModeSelectController : MonoBehaviour
    {
        [SerializeField] private GameModeManager _gameModeManager;
        [SerializeField] private Button _btnSkirmish;        // vs AI
        [SerializeField] private Button _btnMultiplayer;     // online
        [SerializeField] private Button _btnCustomGame;      // local hot seat
        [SerializeField] private Button _btnBack;

        private void Awake()
        {
            _btnSkirmish.onClick.AddListener(_gameModeManager.GoToSkirmishSetup);
            _btnMultiplayer.onClick.AddListener(_gameModeManager.GoToMultiplayerLobby);
            _btnCustomGame.onClick.AddListener(OnCustomGame);
            _btnBack.onClick.AddListener(_gameModeManager.GoToMainMenu);
        }

        private void OnCustomGame()
        {
            // Custom game uses same SkirmishSetup scene but sets GameMode.CustomGame
            if (SceneTransitionData.Instance != null)
                SceneTransitionData.Instance.SelectedMode = GameMode.CustomGame;
            _gameModeManager.GoToSkirmishSetup();
        }
    }
}

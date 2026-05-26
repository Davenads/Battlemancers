using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Battlemancers.UI
{
    /// <summary>
    /// Manages the online multiplayer lobby screen.
    /// Handles create/join lobby flow and transitions to match when both players are ready.
    ///
    /// STUB: UGS package integration is commented out until Unity Gaming Services packages
    /// are installed (Unity Lobby, Unity Relay, Netcode for GameObjects).
    /// This scaffold shows the intended flow and wiring points.
    /// </summary>
    public class LobbyController : MonoBehaviour
    {
        // -----------------------------------------------------------------------------------------
        // Inspector references
        // -----------------------------------------------------------------------------------------

        [SerializeField] private GameModeManager _gameModeManager;

        [Header("Lobby Actions")]
        [SerializeField] private Button _createLobbyButton;
        [SerializeField] private Button _joinLobbyButton;
        [SerializeField] private Button _readyButton;
        [SerializeField] private Button _backButton;

        [Header("Lobby Code")]
        [SerializeField] private TMP_InputField _lobbyCodeField;   // join by code — player types here
        [SerializeField] private TMP_Text       _lobbyCodeDisplay; // shows own code after creating

        [Header("Status Display")]
        [SerializeField] private TMP_Text _playerStatusLabel;      // "Host: Ready  |  Guest: Waiting..."
        [SerializeField] private TMP_Text _connectionStatusLabel;

        // -----------------------------------------------------------------------------------------
        // Private state
        // -----------------------------------------------------------------------------------------

        private bool _isHost;
        private bool _localPlayerReady;
        private bool _remotePlayerReady;

        // -----------------------------------------------------------------------------------------
        // Unity lifecycle
        // -----------------------------------------------------------------------------------------

        private void Awake()
        {
            _createLobbyButton.onClick.AddListener(OnCreateLobby);
            _joinLobbyButton.onClick.AddListener(OnJoinLobby);
            _readyButton.onClick.AddListener(OnReady);
            _backButton.onClick.AddListener(_gameModeManager.GoToMainMenu);
        }

        private void Start()
        {
            _readyButton.interactable = false;
            _connectionStatusLabel.text = "Not connected";
            _playerStatusLabel.text = string.Empty;
            _lobbyCodeDisplay.text = string.Empty;
        }

        // -----------------------------------------------------------------------------------------
        // Button handlers
        // -----------------------------------------------------------------------------------------

        private async void OnCreateLobby()
        {
            _createLobbyButton.interactable = false;
            _joinLobbyButton.interactable   = false;
            _connectionStatusLabel.text     = "Creating lobby...";

            // TODO: await Unity.Services.Core.UnityServices.InitializeAsync();
            // TODO: await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
            // TODO: var lobby = await Unity.Services.Lobbies.LobbyService.Instance
            //           .CreateLobbyAsync("BattlemancersLobby", maxPlayers: 2, options);
            // TODO: var allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(maxConnections: 1);
            // TODO: var joinCode  = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            // TODO: Store allocation and join code; configure NetworkManager transport with allocation data.

            _isHost = true;
            _lobbyCodeDisplay.text  = "CODE: XXXXXX"; // replace with real join code from Relay
            _connectionStatusLabel.text = "Lobby created. Waiting for opponent...";
            _readyButton.interactable   = true;

            Debug.Log("[LobbyController] UGS Lobby — create not yet implemented. Requires UGS packages.");

            await System.Threading.Tasks.Task.CompletedTask; // keeps compiler happy until real async calls are added
        }

        private async void OnJoinLobby()
        {
            string code = _lobbyCodeField.text.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(code))
            {
                _connectionStatusLabel.text = "Enter a lobby code first.";
                return;
            }

            _createLobbyButton.interactable = false;
            _joinLobbyButton.interactable   = false;
            _connectionStatusLabel.text     = $"Joining lobby {code}...";

            // TODO: await Unity.Services.Core.UnityServices.InitializeAsync();
            // TODO: await Unity.Services.Authentication.AuthenticationService.Instance.SignInAnonymouslyAsync();
            // TODO: var lobby = await Unity.Services.Lobbies.LobbyService.Instance.JoinLobbyByCodeAsync(code);
            // TODO: Retrieve Relay join code from lobby data.
            // TODO: var joinAllocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            // TODO: Configure NetworkManager transport with join allocation data; call NetworkManager.StartClient().

            _isHost = false;
            _connectionStatusLabel.text = "Joined lobby. Press Ready when your warband is set.";
            _readyButton.interactable   = true;

            Debug.Log("[LobbyController] UGS Lobby — join not yet implemented. Requires UGS packages.");

            await System.Threading.Tasks.Task.CompletedTask;
        }

        private void OnReady()
        {
            _localPlayerReady         = true;
            _readyButton.interactable = false;

            // TODO: Broadcast ready status to opponent via lobby data update or NetworkManager RPC.
            // TODO: Unity.Services.Lobbies.LobbyService.Instance.UpdatePlayerAsync(lobbyId, playerId, updateOptions)

            RefreshStatusDisplay();
            CheckBothReady();
        }

        // -----------------------------------------------------------------------------------------
        // Status helpers
        // -----------------------------------------------------------------------------------------

        private void RefreshStatusDisplay()
        {
            string hostStatus  = (_isHost  ? _localPlayerReady  : _remotePlayerReady) ? "Ready" : "Not Ready";
            string guestStatus = (_isHost  ? _remotePlayerReady : _localPlayerReady)  ? "Ready" : "Not Ready";
            _playerStatusLabel.text = $"Host: {hostStatus}  |  Guest: {guestStatus}";
        }

        private void CheckBothReady()
        {
            if (!_localPlayerReady || !_remotePlayerReady)
                return;

            _connectionStatusLabel.text = "Both ready! Starting match...";

            // TODO: Host calls NetworkManager.Singleton.StartHost(); client calls StartClient().
            // TODO: Load BattleScene via NetworkManager to ensure both sides load simultaneously.
            // TODO: Set SceneTransitionData.Instance.SelectedMode = GameMode.Multiplayer before loading.

            Debug.Log("[LobbyController] Both players ready — match start pending UGS implementation.");
        }

        /// <summary>
        /// Called by the network layer (e.g., lobby polling or an RPC) when the remote player's
        /// ready status changes. Triggers a status display refresh and checks whether to start.
        /// </summary>
        public void OnRemotePlayerReadyChanged(bool isReady)
        {
            _remotePlayerReady = isReady;
            RefreshStatusDisplay();
            CheckBothReady();
        }
    }
}

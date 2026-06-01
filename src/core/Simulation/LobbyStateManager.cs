using System;
using System.Collections.Generic;
using System.Linq;
using Battlemancers.Core.Data;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Simulation
{
    /// <summary>
    /// Manages the lifecycle of a pre-match lobby session — player joining, ready states,
    /// and warband lock-in — leading up to match start.
    ///
    /// Phase order: Assembling → WarbandSelect → Ready → InGame.
    /// <see cref="CanStartMatch"/> returns true only when every player in the lobby has
    /// both locked a warband and marked themselves ready.
    ///
    /// Events published via <see cref="SimulationEventBus"/>:
    /// <list type="bullet">
    ///   <item><see cref="PlayerJoinedEvent"/> — after a new player successfully joins.</item>
    ///   <item><see cref="PlayerReadyChangedEvent"/> — after any player's ready state changes.</item>
    ///   <item><see cref="WarbandLockedEvent"/> — after a player locks their warband selection.</item>
    ///   <item><see cref="LobbyReadyEvent"/> — when all players are ready and warbands are locked.</item>
    /// </list>
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class LobbyStateManager
    {
        // ---------------------------------------------------------------------------
        // Configuration constants
        // ---------------------------------------------------------------------------

        /// <summary>Maximum number of players allowed in a standard lobby.</summary>
        public const int MaxPlayers = 2;

        // ---------------------------------------------------------------------------
        // State
        // ---------------------------------------------------------------------------

        private LobbySessionData _lobby;

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// True when all players in the lobby are ready and have locked a warband.
        /// Returns false if the lobby has not been created or has no players.
        /// </summary>
        public bool CanStartMatch()
        {
            if (_lobby == null || _lobby.PlayerIds.Count == 0)
                return false;

            foreach (string pid in _lobby.PlayerIds)
            {
                if (!_lobby.PlayerReadyStates.TryGetValue(pid, out bool ready) || !ready)
                    return false;
                if (!_lobby.WarbandLockIns.ContainsKey(pid))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a new lobby and registers the host as the first player.
        /// Must be called before any other method.
        /// </summary>
        /// <param name="hostId">Player ID of the lobby creator. Must not be null or empty.</param>
        /// <param name="lobbyId">Optional lobby ID; a GUID is generated if null.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="hostId"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a lobby already exists.</exception>
        public void CreateLobby(string hostId, string lobbyId = null)
        {
            if (string.IsNullOrEmpty(hostId))
                throw new ArgumentException("Host player ID must not be null or empty.", nameof(hostId));
            if (_lobby != null)
                throw new InvalidOperationException("A lobby is already active.");

            string resolvedId = lobbyId ?? Guid.NewGuid().ToString("N");

            _lobby = new LobbySessionData
            {
                LobbyId           = resolvedId,
                HostPlayerId      = hostId,
                PlayerIds         = new List<string> { hostId },
                PlayerReadyStates = new Dictionary<string, bool> { { hostId, false } },
                WarbandLockIns    = new Dictionary<string, string>(),
                Phase             = LobbyPhase.Assembling
            };
        }

        /// <summary>
        /// Adds a player to the lobby. No-op if the player is already in the lobby.
        /// </summary>
        /// <param name="playerId">The ID of the joining player. Must not be null or empty.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="playerId"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the lobby has not been created, is full, or is not in the
        /// <see cref="LobbyPhase.Assembling"/> phase.
        /// </exception>
        public void JoinLobby(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                throw new ArgumentException("Player ID must not be null or empty.", nameof(playerId));

            AssertLobbyActive();

            if (_lobby.Phase != LobbyPhase.Assembling)
                throw new InvalidOperationException($"Cannot join during the {_lobby.Phase} phase.");

            if (_lobby.PlayerIds.Contains(playerId))
                return; // idempotent

            if (_lobby.PlayerIds.Count >= MaxPlayers)
                throw new InvalidOperationException($"Lobby is full ({MaxPlayers} players maximum).");

            _lobby.PlayerIds.Add(playerId);
            _lobby.PlayerReadyStates[playerId] = false;

            SimulationEventBus.Publish(new PlayerJoinedEvent(playerId, _lobby.LobbyId));
        }

        /// <summary>
        /// Sets a player's ready state. Can toggle a player from ready back to unready.
        /// If all players become ready and all warbands are locked, also fires
        /// <see cref="LobbyReadyEvent"/> and advances phase to <see cref="LobbyPhase.Ready"/>.
        /// </summary>
        /// <param name="playerId">The player changing their ready state.</param>
        /// <param name="ready">True to mark ready; false to un-ready.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="playerId"/> is not in the lobby.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the lobby has not been created or is InGame.</exception>
        public void SetReady(string playerId, bool ready)
        {
            AssertLobbyActive();
            AssertPlayerInLobby(playerId);

            if (_lobby.Phase == LobbyPhase.InGame)
                throw new InvalidOperationException("Cannot change ready state while match is in progress.");

            _lobby.PlayerReadyStates[playerId] = ready;

            SimulationEventBus.Publish(new PlayerReadyChangedEvent(playerId, ready));

            if (CanStartMatch() && _lobby.Phase != LobbyPhase.Ready)
            {
                _lobby.Phase = LobbyPhase.Ready;
                SimulationEventBus.Publish(new LobbyReadyEvent(_lobby.LobbyId));
            }
            else if (!ready && _lobby.Phase == LobbyPhase.Ready)
            {
                // Revert to WarbandSelect if someone un-readies after all were ready.
                _lobby.Phase = LobbyPhase.WarbandSelect;
            }
        }

        /// <summary>
        /// Locks in a player's warband choice. Once locked, the warband cannot be changed.
        /// Advances phase to <see cref="LobbyPhase.WarbandSelect"/> once at least one player
        /// has locked. If all players are now ready and locked, fires <see cref="LobbyReadyEvent"/>.
        /// </summary>
        /// <param name="playerId">The player locking their warband.</param>
        /// <param name="warbandId">The ID of the warband being locked in.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="playerId"/> is not in the lobby, or if <paramref name="warbandId"/>
        /// is null or empty.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the player has already locked a warband, or the lobby is InGame.
        /// </exception>
        public void LockWarband(string playerId, string warbandId)
        {
            AssertLobbyActive();
            AssertPlayerInLobby(playerId);

            if (string.IsNullOrEmpty(warbandId))
                throw new ArgumentException("Warband ID must not be null or empty.", nameof(warbandId));

            if (_lobby.Phase == LobbyPhase.InGame)
                throw new InvalidOperationException("Cannot change warband while match is in progress.");

            if (_lobby.WarbandLockIns.ContainsKey(playerId))
                throw new InvalidOperationException($"Player '{playerId}' has already locked a warband.");

            _lobby.WarbandLockIns[playerId] = warbandId;

            if (_lobby.Phase == LobbyPhase.Assembling)
                _lobby.Phase = LobbyPhase.WarbandSelect;

            SimulationEventBus.Publish(new WarbandLockedEvent(playerId, warbandId));

            if (CanStartMatch() && _lobby.Phase != LobbyPhase.Ready)
            {
                _lobby.Phase = LobbyPhase.Ready;
                SimulationEventBus.Publish(new LobbyReadyEvent(_lobby.LobbyId));
            }
        }

        /// <summary>
        /// Returns a snapshot of the current lobby data, or null if no lobby has been created.
        /// Callers should not mutate the returned object.
        /// </summary>
        public LobbySessionData GetLobbySnapshot() => _lobby;

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        private void AssertLobbyActive()
        {
            if (_lobby == null)
                throw new InvalidOperationException("No lobby is active. Call CreateLobby first.");
        }

        private void AssertPlayerInLobby(string playerId)
        {
            if (!_lobby.PlayerIds.Contains(playerId))
                throw new ArgumentException($"Player '{playerId}' is not in the lobby.", nameof(playerId));
        }
    }
}

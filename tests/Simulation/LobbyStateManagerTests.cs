using System;
using NUnit.Framework;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Core.Data;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for <see cref="LobbyStateManager"/>.
    /// Covers: ready gating, warband lock requirement for <see cref="LobbyStateManager.CanStartMatch"/>,
    /// LobbyReady event firing when all players are ready, and PlayerIds update on join.
    /// </summary>
    [TestFixture]
    public class LobbyStateManagerTests
    {
        private const string Host    = "host";
        private const string Player2 = "p2";

        private const string Warband1 = "warband_alpha";
        private const string Warband2 = "warband_beta";

        private LobbyStateManager _lobby;

        [SetUp]
        public void SetUp()
        {
            _lobby = new LobbyStateManager();
            SimulationEventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SimulationEventBus.Clear();
        }

        // ---------------------------------------------------------------------------
        // 1. Ready gating: not all players ready → CanStartMatch is false
        // ---------------------------------------------------------------------------

        /// <summary>
        /// <see cref="LobbyStateManager.CanStartMatch"/> must return false when at least one player
        /// has not marked themselves ready, even if warbands are locked.
        /// </summary>
        [Test]
        public void CanStartMatch_NotAllPlayersReady_ReturnsFalse()
        {
            _lobby.CreateLobby(Host);
            _lobby.JoinLobby(Player2);

            _lobby.LockWarband(Host,    Warband1);
            _lobby.LockWarband(Player2, Warband2);

            // Only host marks ready; Player2 does not.
            _lobby.SetReady(Host, true);

            Assert.That(_lobby.CanStartMatch(), Is.False,
                "CanStartMatch must be false when any player has not marked ready.");
        }

        // ---------------------------------------------------------------------------
        // 2. Warband lock required before CanStartMatch
        // ---------------------------------------------------------------------------

        /// <summary>
        /// <see cref="LobbyStateManager.CanStartMatch"/> must return false when all players are ready
        /// but at least one has not locked a warband.
        /// </summary>
        [Test]
        public void CanStartMatch_AllReadyButWarbandNotLocked_ReturnsFalse()
        {
            _lobby.CreateLobby(Host);
            _lobby.JoinLobby(Player2);

            // Only the host locks a warband; Player2 does not.
            _lobby.LockWarband(Host, Warband1);

            _lobby.SetReady(Host,    true);
            _lobby.SetReady(Player2, true);

            Assert.That(_lobby.CanStartMatch(), Is.False,
                "CanStartMatch must be false when any player has not locked a warband.");
        }

        // ---------------------------------------------------------------------------
        // 3. All-ready + all-locked fires LobbyReadyEvent and advances phase
        // ---------------------------------------------------------------------------

        /// <summary>
        /// When the last player becomes ready and all warbands are already locked,
        /// <see cref="LobbyReadyEvent"/> must be fired exactly once and the lobby phase
        /// must advance to <see cref="LobbyPhase.Ready"/>.
        /// </summary>
        [Test]
        public void SetReady_AllPlayersReadyAndLocked_FiresLobbyReadyEvent()
        {
            _lobby.CreateLobby(Host);
            _lobby.JoinLobby(Player2);

            _lobby.LockWarband(Host,    Warband1);
            _lobby.LockWarband(Player2, Warband2);

            int lobbyReadyCount = 0;
            SimulationEventBus.Subscribe<LobbyReadyEvent>(_ => lobbyReadyCount++);

            _lobby.SetReady(Host,    true);
            _lobby.SetReady(Player2, true);

            Assert.That(lobbyReadyCount, Is.EqualTo(1), "LobbyReadyEvent should fire exactly once.");
            Assert.That(_lobby.GetLobbySnapshot().Phase, Is.EqualTo(LobbyPhase.Ready),
                "Lobby phase should advance to Ready.");
            Assert.That(_lobby.CanStartMatch(), Is.True);
        }

        // ---------------------------------------------------------------------------
        // 4. Joining player updates PlayerIds
        // ---------------------------------------------------------------------------

        /// <summary>
        /// After <see cref="LobbyStateManager.JoinLobby"/> succeeds the joining player's ID
        /// must appear in <see cref="LobbySessionData.PlayerIds"/> and a
        /// <see cref="PlayerJoinedEvent"/> must be published.
        /// </summary>
        [Test]
        public void JoinLobby_NewPlayer_UpdatesPlayerIds()
        {
            _lobby.CreateLobby(Host);

            string joinedPlayerId = null;
            SimulationEventBus.Subscribe<PlayerJoinedEvent>(e => joinedPlayerId = e.PlayerId);

            _lobby.JoinLobby(Player2);

            LobbySessionData snapshot = _lobby.GetLobbySnapshot();
            Assert.That(snapshot.PlayerIds, Contains.Item(Player2),
                "PlayerIds should contain the newly joined player.");
            Assert.That(snapshot.PlayerIds.Count, Is.EqualTo(2));
            Assert.That(joinedPlayerId, Is.EqualTo(Player2),
                "PlayerJoinedEvent should carry the joining player's ID.");
        }
    }
}

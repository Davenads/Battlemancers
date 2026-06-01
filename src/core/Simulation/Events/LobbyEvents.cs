namespace Battlemancers.Core.Simulation.Events
{
    /// <summary>
    /// Fired after a new player successfully joins the lobby.
    /// Subscribers: lobby UI (add player row), multiplayer relay (notify all clients).
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class PlayerJoinedEvent : SimulationEvent
    {
        /// <summary>ID of the player who just joined.</summary>
        public string PlayerId { get; }

        /// <summary>Lobby ID the player joined.</summary>
        public string LobbyId { get; }

        public PlayerJoinedEvent(string playerId, string lobbyId)
            : base(turnNumber: 0)
        {
            PlayerId = playerId;
            LobbyId = lobbyId;
        }
    }

    /// <summary>
    /// Fired whenever a player's ready state changes (either toggled ready or unready).
    /// Subscribers: lobby UI (update ready indicator), host client (check start eligibility).
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class PlayerReadyChangedEvent : SimulationEvent
    {
        /// <summary>ID of the player whose ready state changed.</summary>
        public string PlayerId { get; }

        /// <summary>The player's new ready state after the change.</summary>
        public bool IsReady { get; }

        public PlayerReadyChangedEvent(string playerId, bool isReady)
            : base(turnNumber: 0)
        {
            PlayerId = playerId;
            IsReady = isReady;
        }
    }

    /// <summary>
    /// Fired when a player locks in a warband selection, preventing further changes.
    /// Subscribers: lobby UI (show locked indicator), host client (track lock-in count).
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class WarbandLockedEvent : SimulationEvent
    {
        /// <summary>ID of the player who locked their warband.</summary>
        public string PlayerId { get; }

        /// <summary>ID of the warband that was locked in.</summary>
        public string WarbandId { get; }

        public WarbandLockedEvent(string playerId, string warbandId)
            : base(turnNumber: 0)
        {
            PlayerId = playerId;
            WarbandId = warbandId;
        }
    }

    /// <summary>
    /// Fired when all players in the lobby are ready AND have locked their warbands,
    /// indicating the match can start immediately.
    /// Subscribers: host client (enable Start Match button), matchmaking service (begin countdown).
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class LobbyReadyEvent : SimulationEvent
    {
        /// <summary>ID of the lobby that is now ready to start.</summary>
        public string LobbyId { get; }

        public LobbyReadyEvent(string lobbyId)
            : base(turnNumber: 0)
        {
            LobbyId = lobbyId;
        }
    }
}

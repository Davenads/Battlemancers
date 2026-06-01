namespace Battlemancers.Core.Simulation.Events
{
    /// <summary>
    /// Fired after a player successfully picks a Mancer during the draft Picking phase.
    /// Subscribers: lobby UI (highlight picked Mancer, advance turn indicator), multiplayer
    /// relay (broadcast pick to remote clients).
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class MancerPickedEvent : SimulationEvent
    {
        /// <summary>ID of the player who made the pick.</summary>
        public string PlayerId { get; }

        /// <summary>ID of the Mancer that was picked (e.g., "pyromancer").</summary>
        public string MancerId { get; }

        /// <summary>
        /// Zero-based index of this pick within the Picking phase.
        /// Useful for replaying draft sequences deterministically.
        /// </summary>
        public int PickIndex { get; }

        public MancerPickedEvent(string playerId, string mancerId, int pickIndex)
            : base(turnNumber: 0)
        {
            PlayerId = playerId;
            MancerId = mancerId;
            PickIndex = pickIndex;
        }
    }

    /// <summary>
    /// Fired after a player successfully bans a Mancer during the draft Banning phase.
    /// Subscribers: lobby UI (grey out banned Mancer card), multiplayer relay (broadcast ban).
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class MancerBannedEvent : SimulationEvent
    {
        /// <summary>ID of the player who issued the ban.</summary>
        public string PlayerId { get; }

        /// <summary>ID of the Mancer that was banned.</summary>
        public string MancerId { get; }

        /// <summary>
        /// Zero-based index of this ban within the Banning phase.
        /// </summary>
        public int BanIndex { get; }

        public MancerBannedEvent(string playerId, string mancerId, int banIndex)
            : base(turnNumber: 0)
        {
            PlayerId = playerId;
            MancerId = mancerId;
            BanIndex = banIndex;
        }
    }

    /// <summary>
    /// Fired once all picks are complete and the draft transitions to the Complete phase.
    /// Subscribers: lobby state machine (advance to WarbandSelect), UI (show draft summary).
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class DraftCompletedEvent : SimulationEvent
    {
        /// <summary>Session ID of the draft that just finished.</summary>
        public string SessionId { get; }

        public DraftCompletedEvent(string sessionId)
            : base(turnNumber: 0)
        {
            SessionId = sessionId;
        }
    }
}

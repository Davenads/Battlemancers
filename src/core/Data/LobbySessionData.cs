using System.Collections.Generic;

namespace Battlemancers.Core.Data
{
    /// <summary>
    /// Immutable-by-convention POCO that captures the full state of one pre-match lobby session.
    /// Owned exclusively by <see cref="Battlemancers.Core.Simulation.LobbyStateManager"/>;
    /// the presentation layer should treat this as a read-only snapshot.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class LobbySessionData
    {
        /// <summary>Unique identifier for this lobby (e.g., a GUID or service-assigned code).</summary>
        public string LobbyId { get; set; }

        /// <summary>Player ID of the player who created the lobby.</summary>
        public string HostPlayerId { get; set; }

        /// <summary>
        /// All player IDs currently in the lobby, including the host.
        /// Players are appended in join order; no duplicates.
        /// </summary>
        public List<string> PlayerIds { get; set; }

        /// <summary>
        /// Maps player ID → whether that player has marked themselves ready.
        /// All players start as not ready when they join.
        /// </summary>
        public Dictionary<string, bool> PlayerReadyStates { get; set; }

        /// <summary>
        /// Maps player ID → the warband ID that player has locked in.
        /// A player must lock a warband before <see cref="Battlemancers.Core.Simulation.LobbyStateManager.CanStartMatch"/>
        /// returns true.
        /// </summary>
        public Dictionary<string, string> WarbandLockIns { get; set; }

        /// <summary>The current phase of the lobby lifecycle.</summary>
        public LobbyPhase Phase { get; set; }
    }

    /// <summary>Phase of a lobby session.</summary>
    public enum LobbyPhase
    {
        /// <summary>The lobby is open; players are joining and configuring their warbands.</summary>
        Assembling,

        /// <summary>All players have joined; each is finalising their warband selection.</summary>
        WarbandSelect,

        /// <summary>All players have locked their warbands and marked themselves ready.</summary>
        Ready,

        /// <summary>The match has started; the lobby is no longer accepting changes.</summary>
        InGame
    }
}

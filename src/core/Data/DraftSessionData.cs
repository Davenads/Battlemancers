using System.Collections.Generic;

namespace Battlemancers.Core.Data
{
    /// <summary>
    /// Immutable-by-convention POCO that captures the full state of one draft session.
    /// Owned exclusively by <see cref="Battlemancers.Core.Simulation.DraftStateManager"/>;
    /// the presentation layer should treat this as a read-only snapshot.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class DraftSessionData
    {
        /// <summary>Unique identifier for this draft session (e.g., a GUID string).</summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Ordered array of participating player IDs.
        /// Turn order alternates through this array during both the Banning and Picking phases.
        /// </summary>
        public string[] PlayerIds { get; set; }

        /// <summary>
        /// Mancer IDs that have been banned and are unavailable for picking.
        /// Elements are appended in ban order; no duplicates.
        /// </summary>
        public string[] BanList { get; set; }

        /// <summary>
        /// Maps mancer ID → the player ID that picked it.
        /// Populated during the Picking phase. Iteration order is unspecified;
        /// use sorted keys or a list when order matters.
        /// </summary>
        public Dictionary<string, string> PickList { get; set; }

        /// <summary>The current phase of the draft.</summary>
        public DraftPhase Phase { get; set; }

        /// <summary>
        /// Zero-based index of the current action within the active phase.
        /// Increments after each successful ban or pick.
        /// </summary>
        public int TurnIndex { get; set; }

        /// <summary>
        /// Maximum seconds a player has to make each ban or pick decision.
        /// Zero means no timer is enforced.
        /// </summary>
        public int PickTimerSeconds { get; set; }
    }

    /// <summary>Phase of an active draft session.</summary>
    public enum DraftPhase
    {
        /// <summary>Players are banning Mancers from the pool.</summary>
        Banning,

        /// <summary>Players are picking Mancers from the remaining pool.</summary>
        Picking,

        /// <summary>All bans and picks are finalised; the draft is over.</summary>
        Complete
    }
}

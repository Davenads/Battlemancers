using System;
using System.Collections.Generic;
using System.Linq;
using Battlemancers.Core.Data;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Simulation
{
    /// <summary>
    /// Manages the state machine for a pre-match Mancer draft session.
    ///
    /// Phase order: Banning → Picking → Complete.
    /// Players alternate turns within each phase. All banning must complete before
    /// any picking begins. Publishes events via <see cref="SimulationEventBus"/> after
    /// each successful action.
    ///
    /// Invariants enforced:
    /// <list type="bullet">
    ///   <item>A banned Mancer cannot be picked.</item>
    ///   <item>A Mancer already picked by any player cannot be picked again.</item>
    ///   <item>Only the player whose turn it is may act.</item>
    ///   <item>Bans are not accepted during the Picking phase; picks are not accepted during Banning.</item>
    /// </list>
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class DraftStateManager
    {
        // ---------------------------------------------------------------------------
        // Configuration constants
        // ---------------------------------------------------------------------------

        /// <summary>Number of bans each player is allocated before picking begins.</summary>
        public const int BansPerPlayer = 1;

        /// <summary>Number of Mancer picks each player makes during the Picking phase.</summary>
        public const int PicksPerPlayer = 3;

        /// <summary>
        /// Complete roster of all 19 Mancer archetype IDs available for drafting.
        /// These IDs correspond to the keys used in Mancer JSON data files.
        /// </summary>
        public static readonly IReadOnlyList<string> FullRoster = new List<string>
        {
            "pyromancer",
            "hydromancer",
            "cryomancer",
            "geomancer",
            "aeromancer",
            "electromancer",
            "necromancer",
            "chronomancer",
            "photomancer",
            "psychomancer",
            "floramancer",
            "faunamancer",
            "toximancer",
            "osteomancer",
            "gravimancer",
            "sonimancer",
            "crystalomancer",
            "echomancer",
            "thermomancer"
        }.AsReadOnly();

        // ---------------------------------------------------------------------------
        // State
        // ---------------------------------------------------------------------------

        private DraftSessionData _session;

        // Tracks how many bans each player has issued this session.
        private readonly Dictionary<string, int> _banCountByPlayer = new Dictionary<string, int>();

        // Tracks how many picks each player has made this session.
        private readonly Dictionary<string, int> _pickCountByPlayer = new Dictionary<string, int>();

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// True when the draft has reached the <see cref="DraftPhase.Complete"/> phase.
        /// Safe to read at any time; returns false before <see cref="StartDraft"/> is called.
        /// </summary>
        public bool IsDraftComplete =>
            _session != null && _session.Phase == DraftPhase.Complete;

        /// <summary>
        /// Initialises a new draft session for the given players.
        /// Must be called exactly once before any other method.
        /// </summary>
        /// <param name="playerIds">
        /// Ordered array of exactly 2 player IDs. Turn order follows this array.
        /// </param>
        /// <param name="sessionId">Optional session identifier; a GUID is generated if null.</param>
        /// <param name="pickTimerSeconds">
        /// Seconds allowed per decision. 0 disables the timer.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="playerIds"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if fewer than 2 player IDs are provided.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a draft is already in progress.</exception>
        public void StartDraft(string[] playerIds, string sessionId = null, int pickTimerSeconds = 0)
        {
            if (playerIds == null) throw new ArgumentNullException(nameof(playerIds));
            if (playerIds.Length < 2)
                throw new ArgumentException("A draft requires at least 2 players.", nameof(playerIds));
            if (_session != null)
                throw new InvalidOperationException("A draft session is already in progress.");

            string resolvedId = sessionId ?? Guid.NewGuid().ToString("N");

            _session = new DraftSessionData
            {
                SessionId    = resolvedId,
                PlayerIds    = (string[])playerIds.Clone(),
                BanList      = Array.Empty<string>(),
                PickList     = new Dictionary<string, string>(),
                Phase        = DraftPhase.Banning,
                TurnIndex    = 0,
                PickTimerSeconds = pickTimerSeconds
            };

            foreach (string pid in playerIds)
            {
                _banCountByPlayer[pid]  = 0;
                _pickCountByPlayer[pid] = 0;
            }
        }

        /// <summary>
        /// Bans a Mancer from the draft pool during the <see cref="DraftPhase.Banning"/> phase.
        /// </summary>
        /// <param name="playerId">The player issuing the ban. Must be the active player this turn.</param>
        /// <param name="mancerId">The Mancer to ban. Must exist in <see cref="FullRoster"/> and not already be banned.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the draft has not started, is not in the Banning phase, it is not the
        /// player's turn, or the Mancer is already banned.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="mancerId"/> is not in the roster.</exception>
        public void BanMancer(string playerId, string mancerId)
        {
            AssertSessionActive();
            if (_session.Phase != DraftPhase.Banning)
                throw new InvalidOperationException($"Cannot ban during the {_session.Phase} phase.");

            AssertActiveTurn(playerId);
            AssertInRoster(mancerId);

            if (_session.BanList.Contains(mancerId))
                throw new InvalidOperationException($"Mancer '{mancerId}' is already banned.");

            int banIndex = _session.TurnIndex;

            // Append to BanList (array must be rebuilt since it's a plain array POCO).
            var newBanList = new string[_session.BanList.Length + 1];
            _session.BanList.CopyTo(newBanList, 0);
            newBanList[newBanList.Length - 1] = mancerId;
            _session.BanList = newBanList;

            _banCountByPlayer[playerId]++;
            _session.TurnIndex++;

            SimulationEventBus.Publish(new MancerBannedEvent(playerId, mancerId, banIndex));

            TryAdvanceFromBanning();
        }

        /// <summary>
        /// Picks a Mancer for the given player during the <see cref="DraftPhase.Picking"/> phase.
        /// </summary>
        /// <param name="playerId">The player making the pick. Must be the active player this turn.</param>
        /// <param name="mancerId">The Mancer to pick. Must be available (not banned or already picked).</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the draft has not started, is not in the Picking phase, it is not the
        /// player's turn, or the Mancer is unavailable (banned or already picked).
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="mancerId"/> is not in the roster.</exception>
        public void PickMancer(string playerId, string mancerId)
        {
            AssertSessionActive();
            if (_session.Phase != DraftPhase.Picking)
                throw new InvalidOperationException($"Cannot pick during the {_session.Phase} phase.");

            AssertActiveTurn(playerId);
            AssertInRoster(mancerId);

            if (_session.BanList.Contains(mancerId))
                throw new InvalidOperationException($"Mancer '{mancerId}' is banned and cannot be picked.");
            if (_session.PickList.ContainsKey(mancerId))
                throw new InvalidOperationException($"Mancer '{mancerId}' has already been picked.");

            int pickIndex = _session.TurnIndex - TotalBansExpected();

            _session.PickList[mancerId] = playerId;
            _pickCountByPlayer[playerId]++;
            _session.TurnIndex++;

            SimulationEventBus.Publish(new MancerPickedEvent(playerId, mancerId, pickIndex));

            TryCompleteDraft();
        }

        /// <summary>
        /// Returns the ordered list of Mancer IDs that are still available for banning or picking
        /// (not yet banned and not yet picked). Returns the full roster if the draft has not started.
        /// Returned order matches <see cref="FullRoster"/> insertion order.
        /// </summary>
        public IReadOnlyList<string> GetAvailableMancers()
        {
            if (_session == null)
                return FullRoster;

            var result = new List<string>(FullRoster.Count);
            foreach (string id in FullRoster)
            {
                if (!_session.BanList.Contains(id) && !_session.PickList.ContainsKey(id))
                    result.Add(id);
            }

            return result.AsReadOnly();
        }

        /// <summary>
        /// Returns a snapshot copy of the current draft session data, or null if not started.
        /// Callers should not mutate the returned object.
        /// </summary>
        public DraftSessionData GetSessionSnapshot() => _session;

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        private int TotalBansExpected() => BansPerPlayer * _session.PlayerIds.Length;
        private int TotalPicksExpected() => PicksPerPlayer * _session.PlayerIds.Length;

        private string ActivePlayerId()
        {
            // Alternate through PlayerIds array based on TurnIndex within the current phase.
            int phaseOffset = _session.Phase == DraftPhase.Picking ? TotalBansExpected() : 0;
            int localIndex  = _session.TurnIndex - phaseOffset;
            return _session.PlayerIds[localIndex % _session.PlayerIds.Length];
        }

        private void TryAdvanceFromBanning()
        {
            if (_session.TurnIndex >= TotalBansExpected())
            {
                _session.Phase     = DraftPhase.Picking;
                // TurnIndex continues incrementing across phases — no reset needed.
            }
        }

        private void TryCompleteDraft()
        {
            int totalPicksSoFar = TotalBansExpected() + TotalPicksExpected();
            if (_session.TurnIndex >= totalPicksSoFar)
            {
                _session.Phase = DraftPhase.Complete;
                SimulationEventBus.Publish(new DraftCompletedEvent(_session.SessionId));
            }
        }

        private void AssertSessionActive()
        {
            if (_session == null)
                throw new InvalidOperationException("No draft session is active. Call StartDraft first.");
            if (_session.Phase == DraftPhase.Complete)
                throw new InvalidOperationException("The draft session is already complete.");
        }

        private void AssertActiveTurn(string playerId)
        {
            string expected = ActivePlayerId();
            if (playerId != expected)
                throw new InvalidOperationException(
                    $"It is not player '{playerId}''s turn. Expected player '{expected}'.");
        }

        private static void AssertInRoster(string mancerId)
        {
            if (string.IsNullOrEmpty(mancerId) || !FullRoster.Contains(mancerId))
                throw new ArgumentException(
                    $"Mancer ID '{mancerId}' is not in the draft roster.", nameof(mancerId));
        }
    }
}

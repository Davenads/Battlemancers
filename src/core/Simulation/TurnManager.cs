using System;
using System.Collections.Generic;
using System.Linq;
using Battlemancers.Core.Simulation.Commands;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Simulation
{
    /// <summary>
    /// Orchestrates the blind simultaneous turn system.
    ///
    /// Each turn follows this sequence:
    /// <list type="number">
    ///   <item>Both players submit activation plans via SubmitPlan().</item>
    ///   <item>Once both plans are in (AllPlansSubmitted returns true), the caller invokes ResolveTurn().</item>
    ///   <item>ResolveTurn sorts all commands into initiative order and executes them, collecting events.</item>
    ///   <item>End-of-turn processing (cooldown ticks, unit resets) runs; TurnNumber advances.</item>
    ///   <item>TurnResolvedEvent is published and returned with all other events.</item>
    ///   <item>If a win condition is met, MatchEndedEvent is included in the returned events.</item>
    /// </list>
    ///
    /// Initiative order: Mancers → Ranged → Chaff.
    /// Ties within the same unit type are broken by GridPosition: lower X resolves first,
    /// then lower Y (top-left of the grid resolves before bottom-right).
    ///
    /// Turn limit: 50 turns. If neither player has eliminated the other's Mancers by turn 50,
    /// the match ends in a draw (MatchEndReason.TurnLimitReached, WinnerId = null).
    ///
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public class TurnManager
    {
        private const int TurnLimit = 50;

        private readonly SimulationState _state;
        private readonly TemperatureManager _temperatureManager;

        // Maps playerId → the commands that player submitted this turn.
        // Cleared at the end of ResolveTurn so the next turn starts clean.
        private readonly Dictionary<string, Command[]> _pendingPlans;

        /// <summary>
        /// Creates a new TurnManager bound to the given simulation state.
        /// </summary>
        /// <param name="state">The simulation state this manager will drive.</param>
        /// <param name="temperatureManager">
        /// The temperature manager used for per-turn decay and terrain thermal effects.
        /// Must not be null.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if state or temperatureManager is null.</exception>
        public TurnManager(SimulationState state, TemperatureManager temperatureManager)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _temperatureManager = temperatureManager ?? throw new ArgumentNullException(nameof(temperatureManager));
            _pendingPlans = new Dictionary<string, Command[]>();
        }

        // ---------------------------------------------------------------------------
        // Plan submission
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Submits a player's activation plan for the current turn.
        ///
        /// Validates that:
        /// <list type="bullet">
        ///   <item>The playerId is a recognized participant in the match.</item>
        ///   <item>The total ActivationCost of all commands does not exceed 100 pts.</item>
        ///   <item>Each individual command passes its own Validate() check.</item>
        /// </list>
        ///
        /// Replaces any previously submitted plan from the same player (allows re-submission
        /// during Planning phase before lock-in).
        /// </summary>
        /// <param name="playerId">The player submitting the plan.</param>
        /// <param name="commands">
        /// The commands the player wishes to execute. May be empty (pass the turn).
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if playerId is not a match participant, the budget is exceeded,
        /// or any individual command fails validation.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if called while the turn is not in the Planning or Locked phase.
        /// </exception>
        public void SubmitPlan(string playerId, Command[] commands)
        {
            if (_state.Phase != TurnPhase.Planning && _state.Phase != TurnPhase.Locked)
                throw new InvalidOperationException($"Cannot submit plans during phase {_state.Phase}. Expected Planning or Locked.");

            // Verify the player is a participant.
            bool knownPlayer = false;
            foreach (string id in _state.PlayerIds)
            {
                if (id == playerId)
                {
                    knownPlayer = true;
                    break;
                }
            }
            if (!knownPlayer)
                throw new ArgumentException($"Player '{playerId}' is not a participant in this match.", nameof(playerId));

            Command[] planCommands = commands ?? Array.Empty<Command>();

            // Validate budget: sum of activation costs must not exceed 100 pts.
            int totalCost = 0;
            foreach (Command cmd in planCommands)
                totalCost += cmd.ActivationCost;

            if (totalCost > 100)
                throw new ArgumentException(
                    $"Plan for player '{playerId}' exceeds the 100-pt activation budget (total: {totalCost} pts).",
                    nameof(commands));

            // Validate each command individually.
            for (int i = 0; i < planCommands.Length; i++)
            {
                Command cmd = planCommands[i];
                if (!cmd.Validate(_state))
                    throw new ArgumentException(
                        $"Command [{i}] ({cmd.GetType().Name} by actor '{cmd.ActorId}') failed validation.",
                        nameof(commands));
            }

            _pendingPlans[playerId] = planCommands;

            // Transition to Locked once all plans are submitted.
            if (AllPlansSubmitted())
                _state.Phase = TurnPhase.Locked;
        }

        /// <summary>
        /// Returns true when every match participant has submitted a plan for the current turn.
        /// This is the trigger condition for the caller to invoke ResolveTurn().
        /// </summary>
        public bool AllPlansSubmitted() => _pendingPlans.Count == _state.PlayerIds.Length;

        // ---------------------------------------------------------------------------
        // Turn resolution
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Resolves all submitted plans and returns every SimulationEvent generated this turn.
        ///
        /// Execution order:
        /// <list type="number">
        ///   <item>All commands from all players are collected into a flat list.</item>
        ///   <item>Sorted by initiative: Mancers first (priority 0), Ranged second (1), Chaff last (2).</item>
        ///   <item>Within the same priority, units with lower X resolve first; ties broken by lower Y.</item>
        ///   <item>Commands execute in order; each returns events that accumulate into the result list.</item>
        ///   <item>Cooldowns tick on all living units; unit per-turn state is reset.</item>
        ///   <item>Win condition is checked; if the match ends, MatchEndedEvent is appended.</item>
        ///   <item>Pending plans are cleared; TurnNumber increments.</item>
        ///   <item>TurnResolvedEvent is published to SimulationEventBus.</item>
        /// </list>
        /// </summary>
        /// <returns>All SimulationEvents generated during this turn's resolution, in order.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if not all players have submitted plans, or if the turn is not in the correct phase.
        /// </exception>
        public SimulationEvent[] ResolveTurn()
        {
            if (!AllPlansSubmitted())
                throw new InvalidOperationException(
                    $"Cannot resolve: only {_pendingPlans.Count} of {_state.PlayerIds.Length} plans submitted.");

            _state.Phase = TurnPhase.Resolving;

            var allEvents = new List<SimulationEvent>();

            // Temperature decay — runs at start of resolution, before any commands execute.
            _temperatureManager.DecayAllTemperatures(_state);

            // Step 1: Collect all commands from all plans.
            var allCommands = new List<Command>();
            foreach (Command[] plan in _pendingPlans.Values)
            {
                allCommands.AddRange(plan);
            }

            // Step 2: Sort by initiative order.
            // Primary key: unit type priority (Mancer=0, Ranged=1, Chaff=2).
            // Secondary key: actor's GridPosition X (lower = resolves first).
            // Tertiary key: actor's GridPosition Y (lower = resolves first).
            allCommands.Sort((a, b) =>
            {
                UnitState actorA = _state.GetUnit(a.ActorId);
                UnitState actorB = _state.GetUnit(b.ActorId);

                // Dead or deregistered units' commands go last (edge case: unit died mid-turn).
                int priorityA = actorA != null ? GetInitiativePriority(actorA.Type) : int.MaxValue;
                int priorityB = actorB != null ? GetInitiativePriority(actorB.Type) : int.MaxValue;

                int cmp = priorityA.CompareTo(priorityB);
                if (cmp != 0) return cmp;

                // Same unit type — break tie by X position (top-left first).
                if (actorA != null && actorB != null)
                {
                    cmp = actorA.Position.X.CompareTo(actorB.Position.X);
                    if (cmp != 0) return cmp;

                    // Same X — break by Y position.
                    return actorA.Position.Y.CompareTo(actorB.Position.Y);
                }

                return 0;
            });

            // Step 3: Execute commands in sorted order.
            int actionsResolved = 0;
            foreach (Command cmd in allCommands)
            {
                // Skip commands whose actor has died during this turn's resolution.
                UnitState actor = _state.GetUnit(cmd.ActorId);
                if (actor == null || !actor.IsAlive)
                    continue;

                SimulationEvent[] cmdEvents = cmd.Execute(_state);
                if (cmdEvents != null)
                    allEvents.AddRange(cmdEvents);

                actionsResolved++;
            }

            // Step 4: End-of-turn processing — tick cooldowns on all living units.
            // (Terrain state ticking is a stub here; the TerrainSystem in Wave 2 handles it.)
            foreach (UnitState unit in _state.GetLivingUnits())
            {
                unit.TickCooldowns();
            }

            // Apply terrain-based temperature passives and tick Heatstroke penalties.
            // (ApplyTerrainTemperatureEffects internally calls TickHeatstrokePenalties.)
            _temperatureManager.ApplyTerrainTemperatureEffects(_state);

            // Step 5: Check win condition before advancing turn counter.
            bool matchEnded = CheckWinCondition(out string winnerId);

            // Step 6: Clear pending plans and advance the turn.
            _pendingPlans.Clear();
            _state.AdvanceTurn();
            _state.ResetUnitsForNewTurn();
            _state.Phase = TurnPhase.Planning;

            // Step 7: Append win/draw event if the match is over.
            if (matchEnded)
            {
                MatchEndReason reason = winnerId != null
                    ? MatchEndReason.AllEnemyMancersEliminated
                    : MatchEndReason.Draw;

                // If we exceeded the turn limit (TurnNumber after advance > TurnLimit), mark as TurnLimitReached.
                // We check the pre-advance turn number stored via the turn we just resolved.
                if (_state.TurnNumber - 1 >= TurnLimit && winnerId == null)
                    reason = MatchEndReason.TurnLimitReached;

                allEvents.Add(new MatchEndedEvent(_state.TurnNumber - 1, winnerId, reason));
            }

            // Step 8: Build and publish the TurnResolvedEvent.
            var resolvedEvent = new TurnResolvedEvent(_state.TurnNumber - 1, actionsResolved);
            allEvents.Add(resolvedEvent);
            SimulationEventBus.Publish(resolvedEvent);

            return allEvents.ToArray();
        }

        // ---------------------------------------------------------------------------
        // Win condition
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Checks whether the match has ended.
        ///
        /// Win conditions (evaluated in this priority):
        /// <list type="number">
        ///   <item>Turn limit reached (TurnNumber &gt;= TurnLimit): draw, winnerId = null.</item>
        ///   <item>Both players have zero living Mancers: draw, winnerId = null.</item>
        ///   <item>One player has zero living Mancers, the other has at least one: the surviving player wins.</item>
        ///   <item>Both players still have living Mancers: match continues, returns false.</item>
        /// </list>
        /// </summary>
        /// <param name="winnerId">
        /// Set to the winning player's ID if there is a winner; null for a draw or ongoing match.
        /// </param>
        /// <returns>True if the match has ended; false if it is ongoing.</returns>
        public bool CheckWinCondition(out string winnerId)
        {
            winnerId = null;

            // Turn limit: use the turn number about to be completed (TurnNumber has not incremented yet).
            if (_state.TurnNumber >= TurnLimit)
            {
                // Turn limit reached — draw regardless of Mancer count.
                winnerId = null;
                return true;
            }

            // Evaluate each player's living Mancer count.
            string[] playerIds = _state.PlayerIds;
            int[] livingMancerCounts = new int[playerIds.Length];

            for (int i = 0; i < playerIds.Length; i++)
            {
                int count = 0;
                foreach (UnitState _ in _state.GetLivingMancersByOwner(playerIds[i]))
                    count++;
                livingMancerCounts[i] = count;
            }

            // Count how many players have been eliminated (0 living Mancers).
            int eliminatedPlayers = 0;
            int survivingPlayerIndex = -1;
            for (int i = 0; i < livingMancerCounts.Length; i++)
            {
                if (livingMancerCounts[i] == 0)
                    eliminatedPlayers++;
                else
                    survivingPlayerIndex = i;
            }

            // No players eliminated — match continues.
            if (eliminatedPlayers == 0)
                return false;

            // All players eliminated simultaneously — draw.
            if (eliminatedPlayers == playerIds.Length)
            {
                winnerId = null;
                return true;
            }

            // Exactly one player remains — they win.
            if (survivingPlayerIndex >= 0)
            {
                winnerId = playerIds[survivingPlayerIndex];
                return true;
            }

            // Fallback: should not be reached in a 2-player match.
            return false;
        }

        // ---------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns the initiative priority value for a given unit type.
        /// Lower values resolve earlier in the turn.
        /// Mancers (0) → Ranged (1) → Chaff (2).
        /// </summary>
        private static int GetInitiativePriority(UnitType type)
        {
            return type switch
            {
                UnitType.Mancer => 0,
                UnitType.Ranged => 1,
                UnitType.Chaff  => 2,
                _               => 99
            };
        }
    }
}

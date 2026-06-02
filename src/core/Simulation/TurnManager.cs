using System;
using System.Collections.Generic;
using System.Linq;
using Battlemancers.Core.Simulation.Commands;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Simulation.Effects;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Core.Simulation
{
    /// <summary>
    /// Orchestrates the blind simultaneous turn system.
    ///
    /// Each turn follows this sequence:
    /// <list type="number">
    ///   <item>Both players submit activation plans via SubmitPlan() — simultaneously and secretly.</item>
    ///   <item>Once both plans are in (AllPlansSubmitted returns true), the caller invokes ResolveTurn().</item>
    ///   <item>ResolveTurn sorts all commands by resolution order and executes them, collecting events.</item>
    ///   <item>End-of-turn processing (cooldown ticks, unit resets) runs; TurnNumber advances.</item>
    ///   <item>TurnResolvedEvent is published and returned with all other events.</item>
    ///   <item>If a win condition is met, MatchEndedEvent is included in the returned events.</item>
    /// </list>
    ///
    /// Resolution order: Mancers (priority 0) → Ranged (priority 1) → Chaff (priority 2).
    /// Within the same unit type, HASTE units resolve first (sub-key –1), TIME_SLOW units resolve
    /// last (sub-key +1), and normal units resolve by board position: lowest (x+y) sum first,
    /// then lowest x on tie. This is fully deterministic from board state — no random rolls.
    ///
    /// Status overrides during execution (applied per-actor before each command runs):
    /// <list type="bullet">
    ///   <item>STUNNED or FROZEN: entire command set for this actor is skipped.</item>
    ///   <item>ROOTED: MoveCommands are cancelled; SpellCommands execute.</item>
    ///   <item>SILENCED: SpellCommands are cancelled; MoveCommands execute.</item>
    ///   <item>CONFUSED: SpellCommand target is overridden to nearest visible unit regardless of allegiance.</item>
    ///   <item>PANICKED: MoveCommand overrides to flee; SpellCommand targets nearest unit regardless of allegiance.</item>
    ///   <item>CHARMED: SpellCommand targets nearest ally with highest-base-damage spell; MoveCommand moves toward ally if no ally in range.</item>
    /// </list>
    ///
    /// Turn limit: 50 turns. If neither player has eliminated the other's Mancers by turn 50,
    /// the match ends in a draw (MatchEndReason.TurnLimitReached, WinnerId = null).
    ///
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public class TurnManager
    {
        private const int TurnLimit = 50;

        // Sub-priority keys for HASTE/TIME_SLOW within the same unit-type window.
        private const int HasteSubPriority = -1;
        private const int NormalSubPriority = 0;
        private const int TimeSlowSubPriority = 1;

        private readonly SimulationState _state;
        private readonly TemperatureManager _temperatureManager;

        // Maps playerId → the commands that player submitted this turn.
        // Cleared at the end of ResolveTurn so the next turn starts clean.
        private readonly Dictionary<string, Command[]> _pendingPlans;

        /// <summary>
        /// Creates a new TurnManager bound to the given simulation state.
        /// A default TemperatureManager (with a default StatusManager) is created internally.
        /// </summary>
        /// <param name="state">The simulation state this manager will drive.</param>
        /// <exception cref="ArgumentNullException">Thrown if state is null.</exception>
        public TurnManager(SimulationState state)
            : this(state, new TemperatureManager(new StatusManager()))
        {
        }

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
        ///   <item>Sorted by resolution order: type priority, then HASTE/TIME_SLOW sub-priority, then board position.</item>
        ///   <item>Commands execute in order; status overrides are applied per actor before each command executes.</item>
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

            // Step 2: Sort by resolution order.
            // Primary key:     unit type priority (Mancer=0, Ranged=1, Chaff=2).
            // Secondary key:   HASTE/TIME_SLOW sub-priority (HASTE=−1, normal=0, TIME_SLOW=+1).
            //                  Within HASTE group and within TIME_SLOW group, still sort by position.
            // Tertiary key:    (x+y) board position sum — lower sum resolves first within same type+sub-priority.
            // Quaternary key:  x coordinate — lower x resolves first when x+y sums are equal.
            allCommands.Sort((a, b) =>
            {
                UnitState actorA = _state.GetUnit(a.ActorId);
                UnitState actorB = _state.GetUnit(b.ActorId);

                // Dead or deregistered units' commands go last (edge case: unit died mid-turn).
                int priorityA = actorA != null ? GetResolutionPriority(actorA.Type) : int.MaxValue;
                int priorityB = actorB != null ? GetResolutionPriority(actorB.Type) : int.MaxValue;

                int cmp = priorityA.CompareTo(priorityB);
                if (cmp != 0) return cmp;

                // Same unit type — apply HASTE/TIME_SLOW sub-priority.
                if (actorA != null && actorB != null)
                {
                    int subA = GetHasteSubPriority(actorA);
                    int subB = GetHasteSubPriority(actorB);
                    cmp = subA.CompareTo(subB);
                    if (cmp != 0) return cmp;

                    // Same sub-priority — break tie by board position (x+y sum, then x).
                    int sumA = actorA.Position.X + actorA.Position.Y;
                    int sumB = actorB.Position.X + actorB.Position.Y;
                    cmp = sumA.CompareTo(sumB);
                    if (cmp != 0) return cmp;

                    // Same x+y sum — break by x coordinate.
                    return actorA.Position.X.CompareTo(actorB.Position.X);
                }

                return 0;
            });

            // Step 3: Group commands by actor so we can apply per-actor status overrides.
            // We must preserve the resolution order of actors (as determined by the sort above).
            // Build an ordered list of distinct actors, and a map of their commands.
            var actorOrder = new List<string>();
            var commandsByActor = new Dictionary<string, List<Command>>();
            foreach (Command cmd in allCommands)
            {
                if (!commandsByActor.ContainsKey(cmd.ActorId))
                {
                    actorOrder.Add(cmd.ActorId);
                    commandsByActor[cmd.ActorId] = new List<Command>();
                }
                commandsByActor[cmd.ActorId].Add(cmd);
            }

            // Step 4: Execute commands in actor resolution order, applying status overrides per actor.
            int actionsResolved = 0;

            foreach (string actorId in actorOrder)
            {
                // Skip actors who died during this turn's resolution.
                UnitState actor = _state.GetUnit(actorId);
                if (actor == null || !actor.IsAlive)
                    continue;

                // Apply status overrides and execute all commands for this actor.
                List<SimulationEvent> overrideEvents = ApplyStatusOverrides(
                    actor, commandsByActor[actorId], ref actionsResolved);
                allEvents.AddRange(overrideEvents);
            }

            // Step 5: End-of-turn processing — tick cooldowns on all living units.
            foreach (UnitState unit in _state.GetLivingUnits())
            {
                unit.TickCooldowns();
            }

            // Apply terrain-based temperature passives and tick Heatstroke penalties.
            _temperatureManager.ApplyTerrainTemperatureEffects(_state);

            // Step 6: Check win condition before advancing turn counter.
            bool matchEnded = CheckWinCondition(out string winnerId);

            // Step 7: Clear pending plans and advance the turn.
            _pendingPlans.Clear();
            _state.AdvanceTurn();
            _state.ResetUnitsForNewTurn();
            _state.Phase = TurnPhase.Planning;

            // Step 8: Append win/draw event if the match is over.
            if (matchEnded)
            {
                MatchEndReason reason = winnerId != null
                    ? MatchEndReason.AllEnemyMancersEliminated
                    : MatchEndReason.Draw;

                if (_state.TurnNumber - 1 >= TurnLimit && winnerId == null)
                    reason = MatchEndReason.TurnLimitReached;

                allEvents.Add(new MatchEndedEvent(_state.TurnNumber - 1, winnerId, reason));
            }

            // Step 9: Build and publish the TurnResolvedEvent.
            var resolvedEvent = new TurnResolvedEvent(_state.TurnNumber - 1, actionsResolved);
            allEvents.Add(resolvedEvent);
            SimulationEventBus.Publish(resolvedEvent);

            return allEvents.ToArray();
        }

        // ---------------------------------------------------------------------------
        // Status override resolution
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Applies status-based overrides to all commands for a single actor and executes them.
        /// Returns the events generated. Increments <paramref name="actionsResolved"/> for each
        /// command that actually executes (not skipped).
        ///
        /// Override priority (first matching status wins for skip decisions):
        /// STUNNED / FROZEN → skip all commands.
        /// ROOTED → cancel MoveCommands; execute SpellCommands.
        /// SILENCED → cancel SpellCommands; execute MoveCommands.
        /// CONFUSED → execute SpellCommands with overridden target (nearest visible unit).
        /// PANICKED → override MoveCommand (flee) and SpellCommand (attack nearest).
        /// CHARMED → override SpellCommand (attack nearest ally); override MoveCommand if no ally in range.
        /// </summary>
        private List<SimulationEvent> ApplyStatusOverrides(
            UnitState actor,
            List<Command> actorCommands,
            ref int actionsResolved)
        {
            var events = new List<SimulationEvent>();

            // STUNNED or FROZEN: skip entire command set.
            if (HasStatus(actor, StatusType.Stunned) || HasStatus(actor, StatusType.Frozen))
            {
                SimulationEventBus.Publish(new UnitStatusAppliedEvent(
                    _state.TurnNumber, actor.Id,
                    HasStatus(actor, StatusType.Stunned) ? "Stunned" : "Frozen",
                    duration: 0, stackCount: 1));
                // No commands execute; return empty events.
                return events;
            }

            bool isRooted = HasStatus(actor, StatusType.Rooted);
            bool isSilenced = HasStatus(actor, StatusType.Silenced);
            bool isConfused = HasStatus(actor, StatusType.Confused);
            bool isPanicked = HasStatus(actor, StatusType.Panicked);
            bool isCharmed = HasStatus(actor, StatusType.Charmed);

            // Determine if CHARMED+SILENCED interaction applies: unit moves toward ally, no spell.
            bool charmedAndSilenced = isCharmed && isSilenced;

            foreach (Command cmd in actorCommands)
            {
                // Re-check actor alive status (another command might have killed them — though
                // the actor is being processed now, this is defensive).
                UnitState liveActor = _state.GetUnit(actor.Id);
                if (liveActor == null || !liveActor.IsAlive)
                    break;

                bool isMove = cmd is MoveCommand;
                bool isSpell = cmd is SpellCommand;

                // --- ROOTED: cancel MoveCommands ---
                if (isRooted && isMove)
                    continue; // skip this move

                // --- SILENCED: cancel SpellCommands ---
                if (isSilenced && isSpell)
                    continue; // skip this spell

                // --- CHARMED+SILENCED: move toward ally, skip spell ---
                // (SILENCED already cancels spells above; here we override the move.)
                if (charmedAndSilenced && isMove)
                {
                    Grid.GridPosition charmedMoveTarget = StatusEffectResolver.ResolveCharmedMove(
                        actor.Id, actor.MoveRange, _state);
                    var overrideMove = new MoveCommand(actor.Id, cmd.ActivationCost, charmedMoveTarget);
                    SimulationEvent[] cmdEvents = overrideMove.Execute(_state);
                    if (cmdEvents != null) events.AddRange(cmdEvents);
                    actionsResolved++;
                    continue;
                }

                // --- CHARMED: override SpellCommand to attack nearest ally ---
                if (isCharmed && isSpell)
                {
                    SpellCommand originalSpell = (SpellCommand)cmd;
                    string charmedTarget = StatusEffectResolver.ResolveCharmedTarget(actor.Id, _state);
                    if (charmedTarget == null)
                    {
                        // No ally in range — override MoveCommand instead (handled separately below).
                        // Skip the spell.
                        continue;
                    }
                    UnitState targetUnit = _state.GetUnit(charmedTarget);
                    if (targetUnit != null && targetUnit.IsAlive)
                    {
                        var overrideSpell = new SpellCommand(
                            actor.Id, cmd.ActivationCost, originalSpell.SpellId, targetUnit.Position);
                        SimulationEvent[] cmdEvents = overrideSpell.Execute(_state);
                        if (cmdEvents != null) events.AddRange(cmdEvents);
                        actionsResolved++;
                    }
                    continue;
                }

                // --- CHARMED: override MoveCommand to move toward nearest ally when no ally in spell range ---
                if (isCharmed && isMove)
                {
                    // Only override move when there's no ally in range (charmed spell target null).
                    string charmedSpellTarget = StatusEffectResolver.ResolveCharmedTarget(actor.Id, _state);
                    if (charmedSpellTarget == null)
                    {
                        Grid.GridPosition charmedMoveDest = StatusEffectResolver.ResolveCharmedMove(
                            actor.Id, actor.MoveRange, _state);
                        var overrideMove = new MoveCommand(actor.Id, cmd.ActivationCost, charmedMoveDest);
                        SimulationEvent[] cmdEventsC = overrideMove.Execute(_state);
                        if (cmdEventsC != null) events.AddRange(cmdEventsC);
                        actionsResolved++;
                    }
                    // If there is an ally in range, the actor casts at the ally (handled by SpellCommand).
                    // Skip the move regardless (charmed actors focus on attacking ally, not moving).
                    continue;
                }

                // --- CONFUSED: override SpellCommand target ---
                if (isConfused && isSpell)
                {
                    SpellCommand originalSpell = (SpellCommand)cmd;
                    // Use the fallback spell range (4 tiles) for confused targeting.
                    const int ConfusedSpellRange = 4;
                    string confusedTarget = StatusEffectResolver.ResolveConfusedTarget(
                        actor.Id, ConfusedSpellRange, _state);
                    if (confusedTarget == null)
                        continue; // no valid target — skip spell
                    UnitState confusedTargetUnit = _state.GetUnit(confusedTarget);
                    if (confusedTargetUnit == null || !confusedTargetUnit.IsAlive)
                        continue;
                    var overrideSpell = new SpellCommand(
                        actor.Id, cmd.ActivationCost, originalSpell.SpellId, confusedTargetUnit.Position);
                    SimulationEvent[] cmdEvents = overrideSpell.Execute(_state);
                    if (cmdEvents != null) events.AddRange(cmdEvents);
                    actionsResolved++;
                    continue;
                }

                // --- PANICKED: override MoveCommand (flee) and SpellCommand (attack nearest) ---
                if (isPanicked && isMove)
                {
                    Grid.GridPosition panickedDest = StatusEffectResolver.ResolvePanickedMove(
                        actor.Id, actor.MoveRange, _state);
                    var overrideMove = new MoveCommand(actor.Id, cmd.ActivationCost, panickedDest);
                    SimulationEvent[] cmdEvents = overrideMove.Execute(_state);
                    if (cmdEvents != null) events.AddRange(cmdEvents);
                    actionsResolved++;
                    continue;
                }

                if (isPanicked && isSpell)
                {
                    SpellCommand originalSpell = (SpellCommand)cmd;
                    string panickedTarget = StatusEffectResolver.ResolvePanickedAttackTarget(actor.Id, _state);
                    if (panickedTarget == null)
                        continue; // no unit in range — skip attack
                    UnitState panickedTargetUnit = _state.GetUnit(panickedTarget);
                    if (panickedTargetUnit == null || !panickedTargetUnit.IsAlive)
                        continue;
                    var overrideSpell = new SpellCommand(
                        actor.Id, cmd.ActivationCost, originalSpell.SpellId, panickedTargetUnit.Position);
                    SimulationEvent[] cmdEvents = overrideSpell.Execute(_state);
                    if (cmdEvents != null) events.AddRange(cmdEvents);
                    actionsResolved++;
                    continue;
                }

                // --- Normal execution (no override applicable) ---
                SimulationEvent[] normalEvents = cmd.Execute(_state);
                if (normalEvents != null) events.AddRange(normalEvents);
                actionsResolved++;
            }

            return events;
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

            // Turn limit: use the turn number about to be completed.
            if (_state.TurnNumber >= TurnLimit)
            {
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

            return false;
        }

        // ---------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Returns the resolution priority value for a given unit type.
        /// Lower values resolve earlier within a turn's resolution phase.
        /// Mancers (0) → Ranged (1) → Chaff (2).
        /// </summary>
        private static int GetResolutionPriority(UnitType type)
        {
            return type switch
            {
                UnitType.Mancer => 0,
                UnitType.Ranged => 1,
                UnitType.Chaff  => 2,
                _               => 99
            };
        }

        /// <summary>
        /// Returns the HASTE/TIME_SLOW sub-priority for a unit within its type window.
        /// HASTE = −1 (resolves first), normal = 0, TIME_SLOW = +1 (resolves last).
        /// Within the HASTE group and within the TIME_SLOW group, board position still applies.
        /// </summary>
        private static int GetHasteSubPriority(UnitState unit)
        {
            if (HasStatus(unit, StatusType.Haste))
                return HasteSubPriority;
            if (HasStatus(unit, StatusType.TimeSlow))
                return TimeSlowSubPriority;
            return NormalSubPriority;
        }

        /// <summary>
        /// Returns true if the given unit currently has an active status of the specified type.
        /// Checks <see cref="UnitState.ActiveStatusTypes"/> for the string representation of the type.
        /// </summary>
        /// <param name="unit">The unit to query.</param>
        /// <param name="type">The status type to check for.</param>
        /// <returns>True if the status is active on this unit; otherwise false.</returns>
        public static bool HasStatus(UnitState unit, StatusType type)
        {
            if (unit == null) return false;
            string key = type.ToString();
            return unit.ActiveStatusTypes.Contains(key);
        }
    }
}

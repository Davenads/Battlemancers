# Win Conditions & Match End Rules

## Overview

Every mode of Battlemancers has a clearly defined end state. This document specifies exactly when matches end, what triggers a win or loss, how draws and edge cases are handled, and how the simulation layer detects and communicates all of these. The C# interface specification at the end of this document is authoritative for implementation.

---

## Section 1: Standard Skirmish Win Condition

### The Core Rule

**A player wins a Skirmish match when all enemy Mancers are eliminated.**

Chaff and Ranged units — regardless of quantity, tier, or faction — cannot win a match on their own. Once a player's last Mancer is dead, they have lost, even if dozens of supporting units remain on the field.

### Design Rationale

Mancers are the strategic core of every warband. They are the only units capable of casting spells, chaining element interactions, and driving the combinatorial depth that defines Battlemancers as a game. A warband without Mancers retains no meaningful path to strategic victory: Chaff cannot execute combos, Ranged cannot reshape terrain states, and no non-Mancer unit can ever threaten to close out a match against an opponent who still has spellcasting capacity.

This rule keeps matches focused. If Chaff and Ranged could win by attrition alone, every game would threaten to devolve into an extended mop-up phase after the real tactical engagement concludes. By making Mancer elimination the sole win condition, the game always ends at the moment of decisive strategic resolution — when the last spell-capable unit goes down. The fight that mattered is the fight over the Mancers.

The 100-point activation cost of each Mancer reflects this design intent: a Mancer is worth 10 T1 Chaff as a warband investment. If a 10-to-1 fodder trade could win the game, the entire Mancer-centric design collapses.

### Non-Mancer Units After Mancer Death

Chaff and Ranged units continue to function after their Mancers die. They do not vanish, freeze, or surrender automatically. The opposing player must still deal with them — leaderless infantry can block movement, contest tile control, interdict retreat paths, and continue attacking. They simply cannot win.

A surviving blob of Chaff is a genuine obstacle and a potential source of annoyance. This is intentional. Warbands that invest heavily in fodder are making a real budget trade-off; that investment does not become worthless the moment a Mancer falls. However, once a player's last Mancer is dead, the match outcome is determined. The non-Mancer units become **leaderless**.

### Leaderless State — Mechanical Definition

When a player's last Mancer is eliminated, all surviving non-Mancer units belonging to that player enter the **Leaderless** state. The following rules apply:

- **Player retains full control.** The losing player continues to activate and command their Leaderless units normally. This is the recommended design: forced passivity would be frustrating and removing control creates a worse player experience than simply acknowledging the outcome.
- **Leaderless units cannot alter the match outcome.** They are flagged in `SimulationState` as non-winning units. `IWinConditionEvaluator` ignores them when determining victory.
- **Visual indicator applied.** All Leaderless units display a gray banner icon (see Section 7) signaling their status to both players.
- **The match end is declared immediately** when the last enemy Mancer falls — the remaining Leaderless mop-up is acknowledged but not forced to play out. The opposing player is declared the winner at that moment.

In practice, the losing player will typically concede rather than play through a leaderless mop-up. See Section 6 for concession mechanics.

---

## Section 2: Mancer Death and Match Continuity

### Single Mancer Loss

If a player began the match with multiple Mancers and loses one, the match continues normally. The surviving Mancers remain fully capable. No special state is triggered. The losing player is now at a numerical and strategic disadvantage but retains a valid path to victory.

Example: Player A runs a triple-Mancer warband and loses one. Player A now has two Mancers. The match continues. Player A can still win by eliminating all of Player B's Mancers.

### Last Mancer Death — Match End Timing

When a player's **last remaining Mancer** is killed, the match end is evaluated at the end of the current turn's full resolution phase — not mid-resolution. All commands queued for this turn complete first, then `IWinConditionEvaluator.EvaluateWinCondition()` is called. If the evaluation confirms all enemy Mancers are eliminated, `matchEnded = true` and the `MatchEndedEvent` is published.

**Rationale for waiting until end-of-turn:** Simultaneous resolution means both players' commands execute in the same turn. It is possible for both players to lose their last Mancer in the same turn resolution (see Draw Conditions in Section 3). Evaluating mid-resolution would create ambiguity. Full resolution completes, then win conditions are checked once against the final state.

### Match Does Not Immediately End on Last Mancer Death

As stated in Section 1: the game does not cut to a win screen the instant the last Mancer dies. Resolution continues to completion. This ensures:
- Spells already in flight resolve fully
- Status effects tick normally
- Terrain state changes from this turn apply
- The full `SimulationEvent[]` for this turn is emitted in correct order

Only after `ResolveTurn()` completes does `TurnManager` call `IWinConditionEvaluator`. If `matchEnded` is true, `MatchEndedEvent` is published and the presentation layer handles the win screen transition.

### Concession Note

Players will commonly concede when their last Mancer dies rather than play out remaining Leaderless units. The concession mechanic (Section 6) is available at the start of any Planning phase. A player who has just lost their last Mancer can concede at the top of the next turn, triggering an immediate `MatchEndedEvent` with reason `Concession` rather than waiting for `AllEnemyMancersEliminated` to be confirmed redundantly.

---

## Section 3: Turn Limit and Draw Conditions

### 50-Turn Hard Limit

Matches have a hard limit of **50 turns**.

**Justification:** 50 simultaneous turns equals up to 100 individual unit activations per player across the match (assuming maximum activation budget each turn, which is rarely achieved). In practice, a typical match involving 3 Mancers per side, with supporting units engaged, produces a full tactical engagement and conclusion well within 50 turns. The limit exists to prevent pathological edge cases — turtling strategies, stall compositions, or opponent-harassment — from making matches unplayable.

50 was chosen over a lower limit (e.g., 30) to allow for deliberate, patient strategies that the turn system rewards. Into the Breach uses a strict turn limit as the primary pressure mechanism; Battlemancers uses it only as a ceiling, not a constant threat. The limit should feel like a boundary players never approach in a normal game, not a ticking clock that defines every decision.

### At Turn 50: Sudden Death

If both players still have living Mancers when Turn 50 resolves, the match enters **Sudden Death**.

Sudden Death rules:
- Turn 51 and beyond proceed normally.
- After each turn's full resolution, `IWinConditionEvaluator` checks: has either player lost a Mancer this turn?
- The **first player to lose a Mancer in Sudden Death loses the match**, regardless of how many Mancers their opponent has remaining.
- Standard simultaneous-kill draw rules still apply during Sudden Death (see below).
- There is no turn limit in Sudden Death — but in practice, both players are now under extreme pressure to be aggressive, and matches resolve quickly.

The Sudden Death announcement is surfaced to both players via HUD at the start of Turn 51 (see Section 7).

### True Draw: Simultaneous Last Mancer Kill

If both players lose their final Mancer on the **same turn's resolution phase**, the match is a draw.

This occurs when, for example:
- Player A's Pyromancer casts a spell that kills Player B's last Mancer.
- In the same turn's resolution, Player B's Electromancer — resolving earlier in initiative order — had already cast a spell that kills Player A's last Mancer.
- After full resolution: neither player has living Mancers. Neither player won.

`IWinConditionEvaluator` detects this by checking that `player1MancerCount == 0 AND player2MancerCount == 0` simultaneously after resolution. `WinConditionResult.winnerId` is `null`, `reason` is `Draw_SimultaneousKill`.

**Competitive tiebreaker for draws:** In ranked or tournament play, draws are replayed on a different map selected randomly from the competitive map pool. The result of the original match is not recorded. Both players restart with fresh warbands (they may rebuild — this is not a continuous game).

### Stall Detection

If the following conditions are all true for 3 consecutive turns:
1. Neither player activated any Mancer (only Chaff/Ranged or no units at all were activated).
2. No terrain state changes occurred anywhere on the board (no `TileStateChanged` events emitted).

Then the stall condition is triggered:

- Both players receive a **5-turn warning**: "Match will enter Sudden Death in 5 turns if no Mancer is activated."
- This countdown is surfaced in the HUD and remains visible until dismissed by either Mancer activation or the countdown reaching zero.
- If 5 additional turns pass without a Mancer activation or terrain state change, the match immediately enters Sudden Death regardless of current turn number.

**Design intent:** The stall rule punishes passive hiding strategies without punishing deliberate infantry-heavy play. A player activating Ranged units every turn but avoiding Mancers will not trigger stall detection as long as terrain states are changing (e.g., the ranged units are contesting tile control, status effects are ticking, etc.). The rule only triggers on completely static play — nothing moving, nothing changing.

`TurnManager` tracks a `consecutiveStallTurns` counter. It resets to 0 whenever any Mancer activates or any `TileStateChanged` event is emitted. It increments by 1 at end of each turn where neither condition is met.

---

## Section 4: Campaign Mission Win/Loss Conditions

Campaign missions use the same simulation layer as Skirmish but with mission-specific `IWinConditionEvaluator` implementations. Each mission type below defines its win and loss conditions precisely.

### Elimination

The standard mission type. Mirrors the Skirmish win condition.

**Win:** All enemy Mancers are eliminated. Surviving enemy Chaff/Ranged do not block the win condition.

**Loss:**
- All friendly Mancers are eliminated (enemy has surviving Mancers), OR
- The turn limit for this mission is reached before the win condition is met. Campaign missions have individual turn limits defined in their mission config JSON; the default is 30 turns for Elimination missions.

**Notes:** Chaff and Ranged on either side do not affect win/loss evaluation. If the player loses all Mancers, the mission is failed even if the player has dozens of surviving infantry.

### Escort

A specific non-Mancer VIP unit (the Escort Target) must reach a designated exit tile.

**Win:** The Escort Target enters the exit tile at any point during resolution. Win is evaluated mid-resolution when the `UnitMoved` event for the Escort Target fires — if the destination is the exit tile, `matchEnded = true` is returned immediately. The turn does not need to complete.

**Loss:**
- The Escort Target's HP reaches 0 (unit is killed), OR
- All friendly Mancers are eliminated before the target reaches the exit, OR
- The mission turn limit is reached without the target reaching the exit.

**Critical note:** The Escort Target is a non-Mancer VIP unit. Its existence does not affect the Mancer-elimination win condition. A mission CAN be won with friendly Mancers still alive — the Escort win condition is independent of Mancer status. However, once all friendly Mancers are dead, the mission is failed regardless of the Escort Target's position (the player has lost the ability to protect the target).

**Loss priority:** If the Escort Target is killed and all friendly Mancers are eliminated in the same turn resolution, both loss conditions are true. The result is a mission failure regardless of which triggered first. `MatchEndReason` reports `AllEnemyMancersEliminated` is not applicable — report `MissionObjectiveComplete` is not applicable — the `reason` field is `MissionObjectiveComplete` with a null `winnerId` when the loss is confirmed. (Implementation note: define a `MissionFailed` enum value in `MatchEndReason` for campaign-specific losses.)

### Hold

The player must maintain control of one or more designated tiles for N consecutive turns.

**Control definition:** A tile is considered controlled by the player if:
- At least one friendly unit (Mancer, Chaff, or Ranged) is present on that tile, AND
- Zero enemy units are present on that tile.

All designated tiles must be controlled simultaneously. If even one designated tile is not controlled, the consecutive turn counter resets to 0.

**Win:** The consecutive control counter reaches N. Win is checked at the end of each turn after terrain effects and status effects resolve but before the next planning phase begins.

**Loss:**
- All friendly Mancers are eliminated before the consecutive control counter reaches N, OR
- The mission turn limit is reached without achieving N consecutive turns of full control.

**Implementation note:** `HoldMissionEvaluator` tracks `consecutiveControlTurns` (int) and `requiredTiles` (list of tile positions). Each end-of-turn evaluation: check all tiles in `requiredTiles`. If all are controlled, increment counter. If any is not controlled, reset counter to 0. If counter equals N, set `matchEnded = true`, `reason = MissionObjectiveComplete`.

### Survival

The player must keep at least one Mancer alive for N turns.

**Win:** The turn counter reaches N without all friendly Mancers being eliminated. Evaluated at end of Turn N after full resolution. If at least one friendly Mancer has HP > 0 at that point, the mission is won.

**Loss:** All friendly Mancers are eliminated before the turn counter reaches N.

**Surviving without Mancers does not count:** A player whose last Mancer dies on Turn N-1 but has 30 surviving Chaff on Turn N has NOT won the Survival mission. The win condition explicitly requires Mancer survival. `SimulationState` must confirm `livingMancerCount[player] > 0` at time of evaluation.

**Enemy waves:** Survival missions typically feature escalating enemy waves (spawned at designated spawn tiles at defined turn intervals). The `SurvivalMissionEvaluator` is responsible for triggering wave spawns at the correct turns. Wave spawn is a `SimulationEvent` (`WaveSpawned`) that the presentation layer can use to trigger cinematic wave-arrival effects.

### Assassination

The player must eliminate a specific high-value enemy unit — the Commander — without necessarily clearing all other enemy units.

**Win:** The Commander unit's HP reaches 0. The `UnitDied` event for the Commander triggers `matchEnded = true` regardless of how many other enemy units remain. Win is evaluated mid-resolution when the Commander death event fires.

**Loss:** All friendly Mancers are eliminated before the Commander is killed.

**Behavior of enemy units:** Enemy units in an Assassination mission retaliate normally and do not behave differently because the Commander is present. The Commander is a Mancer-tier unit with enhanced stats and potentially unique abilities; other enemy Mancers and infantry remain active threats. The mission does not require those units to be eliminated.

**Design note:** Assassination missions reward decisive play — get to the Commander quickly before your own forces are attrited down. Enemy non-Commander Mancers are dangerous distractions. Players who try to clear all opposition before the Commander will likely run out of turns or sustain too many losses.

### Puzzle

A scripted scenario with a specific defined solution chain. The win condition is the execution of the correct sequence of actions leading to the stated outcome.

**Win:** The stated solution condition is met. This is mission-specific and defined in the mission config. Common puzzle win conditions:
- A specific unit on a specific tile at end of a specific turn.
- A specific chain of element interactions producing a defined terrain state.
- A specific unit's HP reaching 0 via a chain that demonstrates the taught mechanic.

Puzzle win conditions are evaluated by a mission-specific evaluator configured from the puzzle's JSON definition. No generic evaluator applies.

**Loss:**
- An action is taken that makes the puzzle solution impossible (wrong sequence) — the puzzle evaluator can define "fail states" that immediately end the mission as a failure.
- The turn limit is reached without the solution condition being met. Puzzles typically have a strict turn limit (often 1–3 turns) that forces the player to find the efficient solution.

**On failure:** Puzzle missions immediately offer a retry. They do not count as a campaign loss. Persistent state (roster, progression) is not affected by puzzle mission failure.

---

## Section 5: Win Condition Interface Specification

This section defines the exact C# interface `TurnManager` must use to evaluate win conditions after each turn resolves. All implementations — Skirmish, each campaign mission type — implement this interface. `TurnManager` holds a reference to the current `IWinConditionEvaluator` and calls it once per turn after `ResolveTurn()` completes.

### Interface Definition

```csharp
// IWinConditionEvaluator.cs
// Lives in: src/core/
// Zero Unity dependencies — pure C#

public interface IWinConditionEvaluator
{
    /// <summary>
    /// Called by TurnManager after ResolveTurn() completes each turn.
    /// Evaluates the current SimulationState against this match's win/loss conditions.
    /// Returns a WinConditionResult describing the match outcome (or continuation).
    /// </summary>
    WinConditionResult EvaluateWinCondition(SimulationState state);
}

public struct WinConditionResult
{
    /// <summary>
    /// True if the match has ended (win, loss, or draw). False if the match continues.
    /// TurnManager publishes MatchEndedEvent only when this is true.
    /// </summary>
    public bool matchEnded;

    /// <summary>
    /// The player ID of the winner. Null if the match continues OR if the result is a draw.
    /// In campaign missions, the winning "player" is always the human player ID.
    /// </summary>
    public string winnerId;

    /// <summary>
    /// The reason the match ended. Only meaningful when matchEnded is true.
    /// </summary>
    public MatchEndReason reason;

    /// <summary>
    /// Array of Mancer unit IDs eliminated during this turn's resolution.
    /// Populated from UnitDied events filtered to Mancer-type units.
    /// Empty array if no Mancers died this turn.
    /// Used by VFXDirector to trigger per-Mancer death cinematics before the win screen.
    /// </summary>
    public string[] eliminatedMancerIds;
}

public enum MatchEndReason
{
    /// <summary>Skirmish win: all enemy Mancers are dead.</summary>
    AllEnemyMancersEliminated,

    /// <summary>Turn limit reached and Sudden Death was not triggered; see context.</summary>
    TurnLimitReached,

    /// <summary>Campaign mission objective completed successfully.</summary>
    MissionObjectiveComplete,

    /// <summary>Campaign mission failed (all friendly Mancers dead, or objective missed).</summary>
    MissionFailed,

    /// <summary>A player voluntarily conceded during the Planning phase.</summary>
    Concession,

    /// <summary>Both players lost their last Mancer on the same turn resolution.</summary>
    Draw_SimultaneousKill,

    /// <summary>Stall detection countdown reached zero; match forced into Sudden Death.</summary>
    StallSuddenDeath
}
```

### Evaluation Order (Called Each Turn, After Full Resolution)

`TurnManager` calls `EvaluateWinCondition(state)` once per turn in this order of internal checks. Implementations should follow this evaluation sequence to ensure consistent behavior:

1. **Count living Mancers per player.** Query `SimulationState.UnitCollection` for all units where `unitType == UnitType.Mancer && hp > 0`, grouped by `ownerId`. Store as `int p1MancerCount` and `int p2MancerCount`.

2. **Check mission-specific objective (if applicable).** If this evaluator is a campaign mission type, check the mission objective first — some missions (Escort, Assassination) can be won mid-resolution and their flag is already set by the time `EvaluateWinCondition` is called.

3. **Check standard Mancer elimination.** If `p1MancerCount == 0 && p2MancerCount == 0`: draw. If `p1MancerCount == 0`: player 2 wins. If `p2MancerCount == 0`: player 1 wins.

4. **Check turn limit.** If `state.TurnNumber >= matchTurnLimit`: if still in normal mode, trigger Sudden Death (set `isSuddenDeath = true`, return `matchEnded = false` — the match continues but the mode changes). If already in Sudden Death, this check does not apply (no limit in Sudden Death).

5. **Check stall condition.** If `consecutiveStallTurns >= 3` and warning has been issued, and 5 additional turns have passed since warning without Mancer activation or terrain state change: set `isSuddenDeath = true`, emit `StallSuddenDeath` event, return `matchEnded = false` (match continues in Sudden Death).

6. **If none of the above triggered match end:** return `WinConditionResult { matchEnded = false }`.

### Standard Skirmish Evaluator — Pseudocode Implementation

```csharp
public class SkirmishWinConditionEvaluator : IWinConditionEvaluator
{
    private readonly string player1Id;
    private readonly string player2Id;
    private readonly int turnLimit = 50;
    private bool isSuddenDeath = false;
    private int consecutiveStallTurns = 0;
    private int stallWarningTurnIssued = -1;

    public WinConditionResult EvaluateWinCondition(SimulationState state)
    {
        // Step 1: Count living Mancers per player
        int p1Mancers = CountLivingMancers(state, player1Id);
        int p2Mancers = CountLivingMancers(state, player2Id);

        // Collect eliminated Mancer IDs for this turn
        string[] eliminatedThisTurn = GetEliminatedMancerIds(state);

        // Step 2: No mission objective for standard Skirmish — skip.

        // Step 3: Check Mancer elimination
        if (p1Mancers == 0 && p2Mancers == 0)
        {
            // Simultaneous kill — draw
            return new WinConditionResult
            {
                matchEnded = true,
                winnerId = null,
                reason = MatchEndReason.Draw_SimultaneousKill,
                eliminatedMancerIds = eliminatedThisTurn
            };
        }
        if (p1Mancers == 0)
        {
            return new WinConditionResult
            {
                matchEnded = true,
                winnerId = player2Id,
                reason = MatchEndReason.AllEnemyMancersEliminated,
                eliminatedMancerIds = eliminatedThisTurn
            };
        }
        if (p2Mancers == 0)
        {
            return new WinConditionResult
            {
                matchEnded = true,
                winnerId = player1Id,
                reason = MatchEndReason.AllEnemyMancersEliminated,
                eliminatedMancerIds = eliminatedThisTurn
            };
        }

        // Step 4: Sudden Death check — was a Mancer lost this turn while in Sudden Death?
        if (isSuddenDeath && eliminatedThisTurn.Length > 0)
        {
            // At least one Mancer died this turn during Sudden Death.
            // Both losing simultaneously is caught above. If we reach here, only one side lost.
            // Determine which player lost a Mancer and return the other as winner.
            string loser = GetOwnerOfEliminatedMancer(state, eliminatedThisTurn[0]);
            string winner = (loser == player1Id) ? player2Id : player1Id;
            return new WinConditionResult
            {
                matchEnded = true,
                winnerId = winner,
                reason = MatchEndReason.AllEnemyMancersEliminated,
                eliminatedMancerIds = eliminatedThisTurn
            };
        }

        // Step 4 continued: Trigger Sudden Death at turn limit
        if (!isSuddenDeath && state.TurnNumber >= turnLimit)
        {
            isSuddenDeath = true;
            // Publish SuddenDeathBeganEvent for HUD to display
            state.EventBus.Publish(new SuddenDeathBeganEvent());
            // Match continues — return no end
            return new WinConditionResult { matchEnded = false };
        }

        // Step 5: Stall detection
        bool mancerActivatedThisTurn = state.LastTurnEvents
            .Any(e => e is MancerActivatedEvent);
        bool terrainChangedThisTurn = state.LastTurnEvents
            .Any(e => e is TileStateChangedEvent);

        if (!mancerActivatedThisTurn && !terrainChangedThisTurn)
        {
            consecutiveStallTurns++;
        }
        else
        {
            consecutiveStallTurns = 0;
            stallWarningTurnIssued = -1;
        }

        if (consecutiveStallTurns >= 3 && stallWarningTurnIssued == -1)
        {
            stallWarningTurnIssued = state.TurnNumber;
            state.EventBus.Publish(new StallWarningEvent { turnsRemaining = 5 });
        }

        if (stallWarningTurnIssued != -1 &&
            state.TurnNumber >= stallWarningTurnIssued + 5 &&
            consecutiveStallTurns >= 3)
        {
            isSuddenDeath = true;
            state.EventBus.Publish(new SuddenDeathBeganEvent
            {
                reason = MatchEndReason.StallSuddenDeath
            });
        }

        // Step 6: No end condition met — match continues
        return new WinConditionResult { matchEnded = false };
    }

    private int CountLivingMancers(SimulationState state, string playerId)
    {
        return state.UnitCollection
            .Where(u => u.ownerId == playerId
                        && u.unitType == UnitType.Mancer
                        && u.hp > 0)
            .Count();
    }

    private string[] GetEliminatedMancerIds(SimulationState state)
    {
        return state.LastTurnEvents
            .OfType<UnitDiedEvent>()
            .Where(e => e.unitType == UnitType.Mancer)
            .Select(e => e.unitId)
            .ToArray();
    }

    private string GetOwnerOfEliminatedMancer(SimulationState state, string mancerId)
    {
        // Mancers that just died are still in UnitCollection with hp == 0
        return state.UnitCollection
            .First(u => u.unitId == mancerId)
            .ownerId;
    }
}
```

### Integration with TurnManager

```csharp
// In TurnManager.cs (excerpt):

public SimulationEvent[] ResolveTurn(PlayerPlan[] plans)
{
    ValidatePlans(plans);
    var events = ResolveActivations(plans);
    TickStatusEffects();
    TickTerrainEffects();

    // Evaluate win condition after full resolution
    var result = winConditionEvaluator.EvaluateWinCondition(currentState);
    if (result.matchEnded)
    {
        events = events.Append(new MatchEndedEvent
        {
            winnerId = result.winnerId,
            reason = result.reason,
            eliminatedMancerIds = result.eliminatedMancerIds
        }).ToArray();
    }

    return events;
}
```

`TurnManager` holds `IWinConditionEvaluator winConditionEvaluator` as a constructor parameter, injected at match initialization. The `MatchInitializer` provides the correct evaluator implementation based on mode (Skirmish, campaign mission type, etc.).

---

## Section 6: Concession

### When Concession Is Available

Any player may concede during their **Planning phase**, before they lock in their activation plan for the current turn. Concession is not available once both players have locked in — the reveal and resolution must complete before concession becomes available again.

This window is enforced by `TurnManager`: the concession action is only accepted when `TurnState == TurnState.PLANNING`. Attempting to concede during `RESOLVING` is rejected and queued for the next Planning phase if the player attempts it again.

### Effect of Concession

Concession is immediate and unconditional:

- The opposing player is declared the winner.
- A `MatchEndedEvent` is published with `reason = MatchEndReason.Concession`, `winnerId` set to the opposing player's ID.
- No turn resolution occurs for the turn in which concession was submitted. The planning phase ends; the match ends.
- In ranked play, a concession counts as a full loss for the conceding player. There is no partial outcome.

### Competitive Restriction

In competitive/ranked play, concession during the **Reveal and Resolution phase** is disabled. Once both players have locked in their plans and the reveal begins, the turn plays out fully. This prevents a player from conceding to deny their opponent a recorded kill (e.g., conceding before a dramatic spell resolves to avoid visual defeat).

Concession resumes availability at the top of the next Planning phase after resolution completes.

### Implementation

```csharp
// Called by input layer when player clicks "Concede"
public void SubmitConcession(string concedingPlayerId)
{
    if (currentState.TurnState != TurnState.PLANNING)
    {
        // In competitive mode: reject. In casual: queue.
        return;
    }

    var result = new WinConditionResult
    {
        matchEnded = true,
        winnerId = GetOpponent(concedingPlayerId),
        reason = MatchEndReason.Concession,
        eliminatedMancerIds = new string[0]
    };

    PublishMatchEnd(result);
}
```

---

## Section 7: UI/UX Implications

### Persistent Match State Display

The in-game HUD must display match state clearly at all times. Required elements:

**Mancer Status Panel (both players):**
- Display the count of living Mancers: "2 of 3 Mancers remaining" for each player.
- When a Mancer dies, the counter updates immediately (tied to `UnitDiedEvent` in the presentation layer, not to `EvaluateWinCondition` — the HUD should update in real time as events fire during resolution playback).
- Portrait icons for each Mancer in the warband; eliminated Mancers go grayscale with a death indicator.

**Turn Counter:**
- Current turn number visible at all times: "Turn 12 / 50."
- At Turn 45 (5 turns before limit), the counter gains a warning highlight (amber).
- At Turn 50 and beyond (Sudden Death), the counter changes to "SUDDEN DEATH" in red. No turn number displayed in Sudden Death — the urgency is the message.

**Stall Warning:**
- When the stall warning triggers, a banner appears: "No Mancers active — Sudden Death in 5 turns."
- The warning persists and counts down turn by turn until reset by Mancer activation.

### Last Enemy Mancer Death — Win Sequence

When `MatchEndedEvent` fires with `reason = AllEnemyMancersEliminated`:

1. `VFXDirector` intercepts the `MatchEndedEvent` before the win screen is shown.
2. For each Mancer ID in `eliminatedMancerIds`: play the Mancer's death cinematic (close-up camera, death dissolve VFX, faction-specific death audio).
3. After all death cinematics complete, transition to the win screen.
4. Win screen displays: winning faction banner, surviving Mancer portraits, turn count, and any notable combo or interaction that occurred during the match (tracked via `SimulationEvent` log).

This sequence is handled entirely by the presentation layer. The simulation layer publishes `MatchEndedEvent` once; `VFXDirector` and `UIDirector` consume it and manage the timing.

### Leaderless Unit Visual Indicator

When a player's last Mancer dies and the opposing player is declared the winner:

- All surviving units belonging to the defeated player receive the **Leaderless** flag in `SimulationState`.
- The presentation layer (UnitViewController) renders a gray banner icon above all Leaderless units — a desaturated version of their faction banner, indicating loss of Mancer command.
- Leaderless units are still controllable (see Section 1) but the banner communicates to both players that the match outcome is determined.
- In practice, the win screen transition begins shortly after the last Mancer death cinematic — Leaderless units are rarely visible for long.

### Concession UI

- A "Concede" button is accessible in the pause menu during the Planning phase.
- The button is grayed out (non-interactive) during Reveal and Resolution phases.
- Clicking Concede opens a confirmation dialog: "Concede this match? You will be recorded as the losing player." with Confirm and Cancel options.
- No concession prompt is shown automatically — it is player-initiated only.

# Game Modes

## Overview

Battlemancers supports three core modes at launch, designed to serve different player types: story-driven campaign players, competitive tacticians, and replayability-focused skirmishers.

---

## 1. Campaign Mode

### Structure
- Linear story-driven progression through a series of tactical battles
- Player builds a roster of Mancers, unlocking new types as they progress
- Battles are scenario-driven with unique objectives beyond "defeat all enemies"

### Mancer Progression
- Mancers gain XP from combat; level up unlocks stat improvements and optional spell variant customization
- Permadeath optional setting: fallen Mancers are lost for subsequent missions (hardcore mode)
- Roster grows from 3 available Mancers at start to all 19 over campaign arc
- Campaign warbands follow the same 1,000-point / 3-Mancer cap as Skirmish; players choose their faction at campaign start

### Mission Types
- **Elimination:** Defeat all enemies
- **Escort:** Get a specific unit to an exit tile within N rounds
- **Hold:** Control designated tiles for N consecutive rounds
- **Survival:** Survive N rounds against escalating enemy waves
- **Assassination:** Defeat the enemy commander (other enemies are secondary)
- **Puzzle:** Scripted scenario with a specific solution chain (teaches mechanics)

### Map Design in Campaign
- Maps are hand-crafted with designer intent
- Each map introduces or emphasizes specific mechanics (one map per new terrain state as tutorial)
- Environmental storytelling through terrain: a burned village has pre-set ON_FIRE tiles; a flooded fortress has FLOODED zones

---

## 2. Skirmish Mode

### Structure
- Single-battle mode; no progression, no stakes, pure tactics
- Choose team → choose map → fight
- Local play vs. AI or vs. second player on same screen
- Eventually: online ranked/casual

### Team Selection
- Standard **blind pick:** both players build their Warband independently (up to 3 Mancers + supporting units), then lists are revealed simultaneously at match start
- **Draft mode:** alternate Mancer picks from full roster; first-picked team selects side, second picks last. Supporting unit quantities are chosen post-draft.
- **Mirror mode:** both teams play identical Mancer compositions with identical faction (test of pure execution and activation reads)

### Difficulty (vs. AI)
- `Recruit:` AI takes suboptimal positioning; does not prioritize combo setup
- `Veteran:` AI sets up basic combos; uses terrain; targets weaknesses
- `Archmage:` AI optimizes turn order; plans 2 turns ahead; executes complex chains

### Map Selection
- Preset maps (curated) and procedurally varied maps (randomized terrain states + biome)
- Player chooses biome + size; generator places terrain features with balance rules

---

## 3. Multiplayer Mode

### Overview

Online multiplayer is built on Unity Gaming Services (UGS): Lobby for room management, Relay for NAT traversal, and Netcode for GameObjects for state synchronization. All simulation logic runs identically on both clients — the server synchronizes command logs, not game state.

> Full technical integration detail in `design/multiplayer.md`

---

### Casual Matchmaking

- **Unranked queue:** open to all players; no rating impact; intended for learning and experimentation
- **Ranked queue:** ELO-based MMR; visible rank tiers (Iron → Bronze → Silver → Gold → Archmage); placement matches on first entry
- Estimated queue wait time displayed before confirming; player can cancel at any point before match found
- Warband is locked in before entering queue (no last-second swap after opponent found)

### Private Lobby

1. Player A creates a lobby; receives a 6-character room code
2. Player B enters the code from the main menu and joins
3. Both players see a lobby screen: warband selection, map vote (preset list), ready-up toggle
4. Match begins when both players have clicked Ready with a valid warband selected
5. Host can kick a player before ready-up; no kick after match has started

### Online Turn Flow

The simultaneous blind turn system maps cleanly to a server-mediated lock-in model — no real-time action prediction is required between clients.

```
Planning phase:
  Both clients plan locally — no data sent to server during planning
  Timer counts down (configurable; default 90 seconds per turn in ranked)

Lock-in:
  Client sends PlanPacket { PlayerId, TurnNumber, Commands[] } to server
  Server marks that player as locked
  Locked player sees a "Waiting for opponent..." state

Resolution:
  When both PlanPackets received (or timeout elapsed):
    Server broadcasts ResolutionPacket { P1Commands[], P2Commands[] } to both clients
  Both clients independently simulate turn resolution
  Results must be identical (deterministic simulation)

Turn complete:
  State hash exchanged — see desync detection in multiplayer.md
  Next planning phase begins
```

**Planning timer rules:**
- Ranked: 90 seconds; opponent is notified when you lock in early
- Unranked: 120 seconds
- If a player's timer expires, their current plan is auto-submitted (partial plan is valid)

### Reconnect Handling

- If a client disconnects mid-match, the server holds the match open for up to **90 seconds**
- Opponent sees a "Waiting for reconnect..." banner; their own planning timer is paused
- Reconnecting client receives the last 3 turns of command history and re-simulates to catch up
- If the disconnected player does not reconnect within 90 seconds, the match is awarded to the opponent by default
- Intentional disconnect (force-quit) is treated identically to timeout — no grace period distinction

### Async Challenge Mode

An alternative online format for players who do not want to commit to a real-time session.

- Player A submits their turn plan; server stores it
- Player B receives an in-app push notification when A's plan is submitted
- Player B has up to **24 hours** to submit their plan; if they do not, they forfeit that turn
- Server resolves the turn once both plans are in and notifies both players
- Match can span multiple real-world days; no session required
- Not ranked (unranked MMR only); intended for casual long-distance play
- Active async matches are listed in a dedicated "Challenges" screen on the main menu

### Spectator Mode

- Players can spectate live ranked matches from the match browser
- Spectator feed is delayed by **2 turns** to prevent stream sniping
- Spectators see full board state including both players' resolved plans for the visible turns
- Maximum 50 spectators per match (UGS Relay bandwidth cap)
- Spectator count is visible to match participants

### Replay System

- Every completed match (online or local) generates a **replay file**: a serialized command log of all turns
- Replay files are small (commands only, not state snapshots); typical match < 200 KB
- Replays are saved locally and optionally uploaded to UGS cloud storage (opt-in)
- Shareable via a replay code; recipient downloads and plays back locally
- Playback controls: play, pause, step forward/back one turn, jump to turn N
- Viewable from Main Menu → Replays

### Warband Lock-in

- Both players finalize their Warband before entering any queue or lobby
- Warbands are sealed at lock-in; no changes are permitted after the queue is confirmed
- In blind format (casual and ranked default): both lists are hidden from the opponent until match start, at which point both Warbands are revealed simultaneously
- In draft format (available in ranked): Mancer selections are visible during the pick-ban phase; supporting unit quantities are locked after draft concludes
- A player who queues without a saved Warband is prompted to build one before entering; the queue button is disabled until a valid list exists

### Ranked System

- **Placement matches:** new ranked players complete 5 placement matches before receiving an initial rank; performance during placement determines starting tier
- **Seven tiers:** Bronze → Silver → Gold → Platinum → Diamond → Master → Archmage
- Each tier (except Master and Archmage) has three divisions (III, II, I); promotion requires winning the final match of division I
- **ELO-based MMR** runs behind rank display: visible rank is a smoothed representation; matchmaking uses raw MMR for opponent selection
- **Ranked decay:** players at Diamond and above who do not complete a ranked match within 14 days lose one division per additional week of inactivity; decay stops at Diamond III
- **Season resets:** at the end of each ranked season (approximately quarterly), all players soft-reset to a lower tier (e.g., Archmage → Platinum I); season history and peak rank are preserved on profile
- **Promotion:** automatic on reaching the required LP threshold — no best-of-3 promotion series gates

### Anti-Cheat / Validation

- All command submissions are validated server-side against the current simulation state before being forwarded to the opponent
- Illegal commands (out-of-range targeting, commanding a unit that has already acted, spending more AP than available) are rejected; the submitting client receives a rejection notice and must resubmit a corrected plan
- The simulation is deterministic — both clients independently compute resolution from the same command log; outcome divergence is treated as a client fault, not a server fault
- After each turn, both clients send a **state hash** to the server; if hashes do not match, the server flags a desync event, saves the command log for review, and suspends the match pending investigation
- Client code does not have authoritative access to the opponent's planning state during the planning phase; `PlanPacket` data is transmitted only to the server, never peer-to-peer

### Post-Match

- **ELO update:** MMR and visible rank are updated within seconds of match completion; both players see their delta on the results screen
- **Match history:** every completed match is recorded on the player profile (opponent name, map, result, turn count, date); last 100 matches stored server-side
- **Replay download:** completed match replay is available immediately from the results screen; also auto-saved locally (last 20 replays retained; oldest overwritten when limit is reached)
- **Post-match summary:** results screen shows total damage dealt/received per unit, spell usage frequency, temperature peaks, and element interactions triggered

### Online Infrastructure — Technical Note

| Concern | Implementation |
|---|---|
| Room management | UGS Lobby — create/join/list rooms, store metadata (map choice, warband hashes) |
| NAT traversal | UGS Relay — all traffic routed through Relay server; no peer-to-peer direct connection required |
| State transport | Netcode for GameObjects (NGO) — client-server topology; command packets only, not full state |
| Sync model | Pure command log synchronization — each client simulates independently; server delivers both plans simultaneously at resolution |
| Determinism | All simulation logic runs identically on both clients; no `System.Random` without seed; no dictionary iteration without sorted keys |
| Anti-cheat | After each turn, each client sends a state hash (MD5 of key `SimulationState` fields); server compares hashes; mismatch triggers disconnect and flags the match for review |

> Full implementation spec in `design/multiplayer.md`

---

## 4. Custom Game / Sandbox Mode

### Purpose

Sandbox mode gives players full control over match setup. It is not a ranked or progression mode — it exists for testing, content creation, community scenarios, and puzzle design.

### Setup Options

- **Map:** choose any preset map or open the map editor to build from scratch
- **Units:** place any Mancer or non-Mancer unit on either side; mix factions freely; no point limit enforced
- **Control:** player can control both sides (single-player testing), one side vs. AI, or two local players
- **Pre-configuration:**
  - Individual tile terrain states can be set before match start (WET, BURNING, FROZEN, etc.)
  - Unit HP, status effects, and temperature values can be pre-set
  - Turn number can be set to a specific value (for testing late-game board states)

### Use Cases

| Use case | How to set up |
|---|---|
| Test warband synergy | Load your actual warband vs. a known opponent composition; fast-forward setup |
| Record content | Choreograph a board state; capture spell combos for showcase video |
| Create puzzle scenario | Arrange a specific board; set "Player 2" as non-interactive; define win condition |
| Teach a mechanic | Pre-set element states on terrain; configure a board that forces a specific interaction |

### Scenario Files

- Any sandbox setup can be saved as a **scenario file** (JSON format)
- Scenario files encode: map ID or custom tile array, all unit placements, terrain states, turn metadata
- Shareable via file or scenario code
- Community-created scenarios can be imported from the main menu → Custom Game → Load Scenario
- Scenario format is documented for modders and content creators

---

## 5. Team Draft Mode (Competitive)

### Structure
- Competitive format for experienced players
- Full 19-Mancer pool available
- **Sequential pick-ban:** 1 ban each → alternate picks until both sides have selected up to 3 Mancers; faction and supporting unit budget allocated after draft concludes

### Pick-Ban Phase
```
BAN phase: each side bans 1 Mancer (2 bans total)
PICK phase (up to 3 Mancers per side):
  Side A picks 1
  Side B picks 2 (back-to-back)
  Side A picks 2
  Side B picks 1
  (snake draft - classic tactics format)
WARBAND phase: each player selects faction + distributes remaining 700 pts among Chaff and Ranged
```

**Rationale for bans:** Some Mancer pairs are extremely high-value (Hydromancer + Electromancer); banning prevents degenerate always-pick combos from dominating competitive play without hard-coding forbidden pairings.

### Map Selection in Draft
- Maps are pre-defined competitive maps only (hand-balanced; no procedural)
- Loser of previous match picks map for next (standard competitive format)
- Maps are designed to not heavily favor specific Mancer types (no volcanic map that auto-wins Pyromancer)

---

## 6. Future Modes (Post-Launch Consideration)

### Gauntlet
- Single player: fight a chain of battles with one persistent team, resources carry over
- Roguelike structure: pick upgrades between battles
- Mancer HP carries over between fights (no full reset)

### Puzzle Challenges
- Standalone tactical puzzles: given a specific board state, find the correct combo sequence to win in 1 turn
- "Into the Breach-style" read-the-board puzzles — teaches advanced interactions
- Leaderboards for fastest solve

### Custom Game (Expanded — Post-Launch Additions)

The core Custom Game / Sandbox mode ships at launch (see Section 4). The following features are planned as post-launch additions:

- **Full map editor tool:** tile-by-tile placement on a grid canvas; paint terrain type (grass, stone, water, lava, etc.), initial terrain state (WET, BURNING, FROZEN, POISONED, etc.), and elevation; position unit spawn points for both sides
- **Scenario parameters:** configurable round limit, VP-based win conditions (control points, elimination targets, escort goals), and pre-match weather/terrain modifiers (e.g., "acid rain" that periodically applies WET to all tiles, "volcanic vents" that pulse BURNING on set tiles)
- **Share and import via scenario code:** any custom scenario can be exported to a short alphanumeric share code; recipients enter the code in Custom Game → Load Scenario to download and play the scenario locally
- **Host-defined rule variants:** lobby creator can toggle rule modifications — no-upgrades mode (all units play at base tier), draft-only mode (Mancer selection forced into pick-ban format regardless of normal mode), mirror mode (both sides receive identical Mancer rosters), and time limit overrides
- **Community scenario browser (post-launch):** an in-game browser listing community-uploaded scenarios; filterable by map size, Mancer tags, and rating; players can rate and comment on scenarios
- **Puzzle scenario format:** a scenario type where Player 2 is non-interactive and the board is pre-arranged; win condition is defined as a specific outcome (kill target in 1 turn, reach tile, etc.); puzzles are shareable and can be submitted to community browser
- **Modding SDK access:** scenario format documentation published openly; external tools can generate valid scenario JSON for import; no in-engine modding tools required
- **Curated official scenarios:** Battlemancers ships a library of designer-authored sandbox scenarios covering mechanic demonstrations, challenge boards, and "What if?" compositions

---

## Onboarding / Tutorial Design

The tutorial should be integrated into campaign, not isolated:
1. **Mission 1:** 2 Mancers, no terrain interactions, basic movement + single spell
2. **Mission 2:** First terrain state introduced (Hydromancer floods tiles)
3. **Mission 3:** First element combo demonstrated (Hydromancer + Electromancer)
4. **Mission 4–5:** Destruction mechanics (Pyromancer burns terrain, creates pits)
5. **By Mission 6:** Player has all core systems; remaining campaign complexity builds naturally

**No text walls.** Teach by doing. Show → let player try → confirm success. Each new mechanic introduced via a scenario where it's the obvious solution, not via a rules screen.

---

## Scene Flow

This section documents the intended Unity scene structure and transitions for all game modes. Scenes are loaded additively where noted; otherwise a full scene load is performed.

### Scene Graph

```
MainMenu
├── → WarbandBuilder        (edit or create a saved Warband list)
│   └── → MainMenu          (back)
├── → ModeSelect
│   ├── → SkirmishSetup     (map select, AI difficulty, warband select)
│   │   └── → BattleScene   [SimulationBootstrapper loaded]
│   ├── → MultiplayerLobby  (matchmaking queue or private lobby)
│   │   └── → BattleScene   [SimulationBootstrapper loaded]
│   └── → CampaignChapter   (chapter/mission select)
│       └── → BattleScene   [SimulationBootstrapper loaded]
└── → Settings
```

### Scene Descriptions

#### `MainMenu`
- **Key MonoBehaviours:** `MainMenuController`, `AudioManager` (persistent), `DataRegistry` (persistent singleton, loaded once)
- **Data passed out:** `SceneTransitionData.SelectedMode` — set when player chooses a mode button
- **SimulationBootstrapper:** not present

#### `WarbandBuilder`
- **Key MonoBehaviours:** `WarbandBuilderController`, `WarbandSaveManager`, `RosterDisplayView`
- **Data passed in:** `SceneTransitionData.EditingWarbandId` (null = new list)
- **Data passed out:** `SceneTransitionData.ConfirmedWarband` — the locked and validated `WarbandDefinition`
- **SimulationBootstrapper:** not present

#### `ModeSelect`
- **Key MonoBehaviours:** `ModeSelectController`
- **Data passed out:** `SceneTransitionData.SelectedMode` (Skirmish, Multiplayer, Campaign, Custom)
- **SimulationBootstrapper:** not present

#### `SkirmishSetup`
- **Key MonoBehaviours:** `SkirmishSetupController`, `MapSelectView`, `AIDifficultySelector`
- **Data passed in:** `SceneTransitionData.ConfirmedWarband`
- **Data passed out:** `SceneTransitionData.MapId`, `SceneTransitionData.AIDifficulty`, `SceneTransitionData.MatchConfig`
- **SimulationBootstrapper:** not present

#### `MultiplayerLobby`
- **Key MonoBehaviours:** `LobbyController`, `MatchmakingService`, `LobbyUIView`
- **Data passed in:** `SceneTransitionData.ConfirmedWarband`
- **Data passed out:** `SceneTransitionData.MatchConfig` (populated once both players are ready and server returns match parameters), `SceneTransitionData.NetworkSession`
- **SimulationBootstrapper:** not present

#### `CampaignChapter`
- **Key MonoBehaviours:** `CampaignController`, `ChapterSelectView`, `CampaignSaveManager`
- **Data passed in:** `SceneTransitionData.CampaignSaveSlot`
- **Data passed out:** `SceneTransitionData.MapId`, `SceneTransitionData.ScenarioId`, `SceneTransitionData.MatchConfig`
- **SimulationBootstrapper:** not present

#### `BattleScene`
- **Key MonoBehaviours:** `SimulationBootstrapper`, `BattleSceneController`, `HUDManager`, `CameraRig`, `UnitViewManager`, `InputRouter`, `PlayerInputController`, `AudioManager` (if not already persistent)
- **Data passed in:** `SceneTransitionData.MatchConfig` — contains map ID, both Warband definitions, AI difficulty (if applicable), turn timer settings, and network session reference (if multiplayer)
- **Data passed out:** `SceneTransitionData.MatchResult` — win/loss/draw, turn count, command log path (for replay)
- **SimulationBootstrapper:** PRESENT — instantiates `SimulationState`, wires all managers (`TemperatureManager`, `StatusManager`, `SpellResolver`, `TurnManager`, etc.), and hands off to `BattleSceneController`

### `SceneTransitionData`

`SceneTransitionData` is a static class (no `MonoBehaviour`) that holds cross-scene state. It is never serialized to disk mid-transition; it lives only in memory for the duration of a navigation chain. Fields are nulled after `BattleScene` reads them to prevent stale data leaking into subsequent loads.

```csharp
// src/ui/SceneTransitionData.cs
public static class SceneTransitionData
{
    public static GameMode SelectedMode;
    public static WarbandDefinition ConfirmedWarband;
    public static string MapId;
    public static AIDifficulty AIDifficulty;
    public static MatchConfig MatchConfig;
    public static NetworkSession NetworkSession;
    public static MatchResult MatchResult;
    public static string EditingWarbandId;
    public static string CampaignSaveSlot;
    public static string ScenarioId;

    public static void Clear() { /* null all fields */ }
}
```

# Agent Orchestration — Battlemancers

## Overview

Battlemancers uses a **multi-agent development model** during its design and early prototype phases. An orchestrator Claude instance assigns parallel workstreams to subagent Claude instances, each operating in an isolated git worktree on its own branch. Agents push their completed work, and the orchestrator merges waves sequentially.

This approach enables 10–12 workstreams to proceed simultaneously without merge conflicts, dramatically compressing design + scaffolding time that would otherwise be sequential.

---

## Repository Structure

```
C:/Projects/Battlemancers/          ← main branch (orchestrator's workspace)
C:/Projects/BattlemancersAgents/    ← all worktrees live here
  ├── win-conditions/               ← agent/win-conditions branch
  ├── pyromancer-design/            ← agent/pyromancer-design branch
  ├── hydromancer-design/           ← agent/hydromancer-design branch
  ├── grid-system/                  ← agent/grid-system branch
  ├── sim-core/                     ← agent/sim-core branch
  ├── data-schemas/                 ← agent/data-schemas branch
  ├── mancers-group-a/              ← agent/mancers-group-a branch
  ├── mancers-group-b/              ← agent/mancers-group-b branch
  ├── element-resolver/             ← agent/element-resolver branch
  ├── status-system/                ← agent/status-system branch
  ├── pathfinding/                  ← agent/pathfinding branch
  └── test-framework/               ← agent/test-framework branch
```

### Branch Naming Convention
- Agent branches: `agent/[agent-name]`
- Main integration branch: `main`
- Orchestrator never works directly in agent worktrees

---

## Worktree Setup (Run Once)

```bash
# From C:/Projects/Battlemancers (main branch)

# Create all agent branches
git branch agent/win-conditions
git branch agent/pyromancer-design
git branch agent/hydromancer-design
git branch agent/grid-system
git branch agent/sim-core
git branch agent/data-schemas
git branch agent/mancers-group-a
git branch agent/mancers-group-b
git branch agent/element-resolver
git branch agent/status-system
git branch agent/pathfinding
git branch agent/test-framework

# Create worktrees
git worktree add ../BattlemancersAgents/win-conditions agent/win-conditions
git worktree add ../BattlemancersAgents/pyromancer-design agent/pyromancer-design
git worktree add ../BattlemancersAgents/hydromancer-design agent/hydromancer-design
git worktree add ../BattlemancersAgents/grid-system agent/grid-system
git worktree add ../BattlemancersAgents/sim-core agent/sim-core
git worktree add ../BattlemancersAgents/data-schemas agent/data-schemas
git worktree add ../BattlemancersAgents/mancers-group-a agent/mancers-group-a
git worktree add ../BattlemancersAgents/mancers-group-b agent/mancers-group-b
git worktree add ../BattlemancersAgents/element-resolver agent/element-resolver
git worktree add ../BattlemancersAgents/status-system agent/status-system
git worktree add ../BattlemancersAgents/pathfinding agent/pathfinding
git worktree add ../BattlemancersAgents/test-framework agent/test-framework

# Push all branches to remote
git push origin agent/win-conditions agent/pyromancer-design agent/hydromancer-design
git push origin agent/grid-system agent/sim-core agent/data-schemas
git push origin agent/mancers-group-a agent/mancers-group-b
git push origin agent/element-resolver agent/status-system agent/pathfinding agent/test-framework
```

---

## Agent Roster

### Wave 1 — Fully Parallel (No Dependencies)

All Wave 1 agents can be spawned simultaneously. None depend on each other's output.

---

#### Agent 1: `win-conditions`
**Worktree:** `C:/Projects/BattlemancersAgents/win-conditions`
**Branch:** `agent/win-conditions`

**Owns (creates):**
- `design/combat/win-conditions.md`

**Do not modify:** Any existing file

**Task:** Design and document the complete match end rule system for Battlemancers. Cover:
- Standard Skirmish win condition (all enemy Mancers eliminated vs. all units eliminated — decide and justify)
- What happens when a player's Mancers are all dead but Chaff/Ranged remain
- Turn limit / draw conditions and tiebreaker rules
- How Campaign mission objectives (Elimination, Escort, Hold, Survival, Assassination, Puzzle) map to the same simulation end-state system
- The "last Mancer standing" edge case (one Mancer on each side, no support)
- What information the `TurnManager` needs to receive to evaluate win conditions each turn

**Context files to read first:**
- `design/game-modes.md`
- `design/warbands.md`
- `CLAUDE.md` (Mancer roster, design pillars)

**Interface contract:** Must define a `WinCondition` evaluation signature that `TurnManager` can call at end of each resolution phase — describe it clearly enough that the `sim-core` agent can implement against it.

---

#### Agent 2: `pyromancer-design`
**Worktree:** `C:/Projects/BattlemancersAgents/pyromancer-design`
**Branch:** `agent/pyromancer-design`

**Owns (creates):**
- `design/mancers/pyromancer.md`

**Do not modify:** Any existing file

**Task:** Design the Pyromancer completely. This will serve as the template format for all 19 Mancers. Cover:
- Tactical identity summary (1 paragraph)
- Base stats: HP, Move Range, Spell Range, armor class
- Base spell kit (3–4 spells): for each spell — name, AP cost, targeting type (single/line/AoE/cone), range in tiles, base damage, element, effects applied, terrain interaction triggered, cooldown
- How Pyromancer interacts with each terrain state (wet, burning, frozen, poisoned, charged)
- Upgrade options — at least 2 options per upgrade category (Spell Variant, Passive Trait, Stat Enhancement, Signature Ability) with point costs
- Faction synergy notes (which faction does Pyromancer excel with and why)
- Combo notes (which other Mancers create strong chains with Pyromancer)

**Context files to read first:**
- `CLAUDE.md` (element interaction matrix, design pillars)
- `design/combat/spell-system.md`
- `design/combat/status-effects.md`
- `design/warbands.md` (upgrade cost ranges)

**Interface contract:** The spell kit format defined here becomes the canonical template. Every field in each spell entry must be present and clearly defined so the `data-schemas` agent can build `SpellData` ScriptableObject fields from it.

---

#### Agent 3: `hydromancer-design`
**Worktree:** `C:/Projects/BattlemancersAgents/hydromancer-design`
**Branch:** `agent/hydromancer-design`

**Owns (creates):**
- `design/mancers/hydromancer.md`

**Do not modify:** Any existing file

**Task:** Design the Hydromancer completely. Follow the same format established in `design/mancers/pyromancer.md` (check that file if available; if not, infer format from context). Cover:
- Tactical identity as the push/pull/wet terrain/healing Mancer
- Base stats
- Base spell kit (3–4 spells): include at minimum a wet terrain applicator, a push/pull effect, and a healing spell
- Terrain state interactions across all states
- Upgrade options (at least 2 per category)
- Faction synergy
- Combo notes — specifically note the Hydromancer + Electromancer chain (wet → lightning = chain stun) which is the game's first demonstrated core combo

**Context files to read first:**
- Same as Pyromancer agent + `design/mancers/pyromancer.md` if committed

**Interface contract:** Same as Pyromancer — spell entries must have all fields needed for `SpellData`.

---

#### Agent 4: `grid-system`
**Worktree:** `C:/Projects/BattlemancersAgents/grid-system`
**Branch:** `agent/grid-system`

**Owns (creates):**
- `src/core/Grid/GridData.cs`
- `src/core/Grid/Tile.cs`
- `src/core/Grid/TileState.cs`
- `src/core/Grid/GridManager.cs` *(stub only — MonoBehaviour wrapper)*

**Do not modify:** Any existing file

**Task:** Implement the pure C# grid system. **Zero Unity dependencies** — `GridData`, `Tile`, and `TileState` must compile and run without Unity. `GridManager` may reference Unity types (it's a MonoBehaviour stub) but contains no logic.

Requirements:
- `TileState` enum: NORMAL, WET, BURNING, FROZEN, POISONED, CHARGED, MUD, CORRUPTED, OBSIDIAN, PERMAFROST (extensible — use partial or well-documented enum region)
- `Tile` class/struct: `Vector2Int position`, `TileState state`, `int elevation`, `bool passable`, `bool occupied`, nullable `string occupantId`
- `GridData` class: 2D array of Tiles, constructors for standard sizes (24×24, 32×32, 48×48), query methods (`GetTile`, `SetTileState`, `IsPassable`, `IsOccupied`, `GetNeighbors`, `GetTilesInRange`), full XML doc comments
- `GridManager` stub: holds a `GridData` instance, `[SerializeField]` fields for grid size, placeholder `void Start()` and `void Update()`

**Context files to read first:**
- `design/combat/terrain-system.md`
- `design/tech-stack.md` (Section 5 — simulation engine architecture)

**Interface contract:**
```csharp
// Other agents will call:
GridData.GetTile(Vector2Int pos) → Tile
GridData.SetTileState(Vector2Int pos, TileState state)
GridData.GetNeighbors(Vector2Int pos) → IEnumerable<Tile>
GridData.GetTilesInRange(Vector2Int origin, int range) → IEnumerable<Tile>
GridData.IsPassable(Vector2Int pos) → bool
GridData.IsOccupied(Vector2Int pos) → bool
```

---

#### Agent 5: `sim-core`
**Worktree:** `C:/Projects/BattlemancersAgents/sim-core`
**Branch:** `agent/sim-core`

**Owns (creates):**
- `src/core/Simulation/SimulationState.cs`
- `src/core/Simulation/TurnManager.cs`
- `src/core/Simulation/SimulationEvent.cs`
- `src/core/Simulation/SimulationEventBus.cs`
- `src/core/Simulation/Commands/Command.cs`
- `src/core/Simulation/Commands/MoveCommand.cs`
- `src/core/Simulation/Commands/SpellCommand.cs`
- `src/core/Simulation/Commands/AttackCommand.cs`

**Do not modify:** Any existing file

**Task:** Implement the pure C# simulation core. **Zero Unity dependencies throughout.**

Requirements:
- `SimulationEvent` abstract base + concrete event types: `UnitMoved`, `SpellCast`, `SpellHit`, `TileStateChanged`, `UnitStatusApplied`, `UnitDied`, `TurnResolved`, `MatchEnded`
- `SimulationEventBus`: static or singleton pub/sub bus; `Subscribe<T>`, `Publish<T>` where T is a SimulationEvent subtype
- `SimulationState`: holds `GridData` reference (type reference, not instantiation — pass in via constructor), `Dictionary<string, UnitState>` unit registry, `int turnNumber`, `TurnPhase` enum (PLANNING, LOCKED, RESOLVING, END), `string[] playerIds`
- `Command` abstract base: `string actorId`, `abstract bool Validate(SimulationState state)`, `abstract SimulationEvent[] Execute(SimulationState state)`
- `MoveCommand`, `SpellCommand`, `AttackCommand` as concrete implementations — stub `Execute` bodies are fine; full logic comes later
- `TurnManager`: `SubmitPlan(string playerId, Command[] commands)`, `bool AllPlansSubmitted()`, `SimulationEvent[] ResolveTurn()` — implement resolution ordering (Mancers first, then Ranged, then Chaff based on unit type in UnitState)
- `UnitState` inner class or separate file: `string id`, `string mancerId`, `UnitType type` (enum: MANCER, CHAFF, RANGED), `Vector2Int position`, `int currentHP`, `int maxHP`, `List<StatusEffect> activeStatuses` (StatusEffect is a string enum reference — full type defined by status-system agent)

**Context files to read first:**
- `design/tech-stack.md` (Section 5 — simulation engine architecture, Section 9 — data architecture)
- `design/warbands.md` (activation economy, resolution order)

**Interface contract:**
```csharp
// Other agents will call:
SimulationState.GetUnit(string id) → UnitState
SimulationState.GetAllUnits() → IEnumerable<UnitState>
SimulationEventBus.Subscribe<T>(Action<T> handler)
SimulationEventBus.Publish<T>(T simEvent)
TurnManager.SubmitPlan(string playerId, Command[] commands)
TurnManager.ResolveTurn() → SimulationEvent[]
```

---

#### Agent 6: `data-schemas`
**Worktree:** `C:/Projects/BattlemancersAgents/data-schemas`
**Branch:** `agent/data-schemas`

**Owns (creates):**
- `src/data/MancerData.cs`
- `src/data/SpellData.cs`
- `src/data/UpgradeOption.cs`
- `src/data/TileTypeData.cs`
- `src/data/FactionData.cs`
- `src/data/WarbandSave.cs` *(plain C# serializable — no Unity dependency)*
- `assets/data/schema-notes.md` *(explains ScriptableObject vs JSON boundary)*

**Do not modify:** Any existing file

**Task:** Define all data schemas. `MancerData`, `SpellData`, `UpgradeOption`, `TileTypeData`, `FactionData` are Unity ScriptableObjects (inherit from `ScriptableObject`). `WarbandSave` is a plain C# serializable class with no Unity dependency (used for JSON save/load).

Requirements:
- `SpellData` fields must exactly match the spell kit format from the Pyromancer and Hydromancer design docs (check those if committed; infer from `design/combat/spell-system.md` otherwise)
- All ScriptableObjects must have `[CreateAssetMenu]` attributes with sensible menu paths
- `WarbandSave` must be fully JSON-serializable (no UnityEngine types)
- Include XML doc comments on all public fields
- `UpgradeOption` must reference upgrade categories from `design/warbands.md`

**Context files to read first:**
- `design/warbands.md` (upgrade categories, unit costs)
- `design/combat/spell-system.md`
- `design/tech-stack.md` (Section 10 — data architecture)

**Interface contract:** `SpellData` fields become the canonical data contract that `SpellResolver` (Wave 2) reads from. All fields must be public and clearly named.

---

### Wave 2 — Parallel After Wave 1 Merges

Wave 2 agents are spawned once the orchestrator has merged all Wave 1 branches into `main` and updated the agent worktrees with `git merge main` or by recreating them from the updated `main`.

---

#### Agent 7: `mancers-group-a`
**Worktree:** `C:/Projects/BattlemancersAgents/mancers-group-a`
**Branch:** `agent/mancers-group-a`

**Owns (creates):**
- `design/mancers/cryomancer.md`
- `design/mancers/geomancer.md`
- `design/mancers/aeromancer.md`
- `design/mancers/electromancer.md`

**Dependencies:** `design/mancers/pyromancer.md` (use as format template)

**Task:** Design all four Mancers fully using the Pyromancer doc as the exact format template. Each doc must have identical structure: tactical identity, base stats, spell kit (3–4 spells fully specified), terrain interactions, upgrade options (2+ per category), faction synergy, combo notes. Particular attention for Electromancer: explicitly document the wet-tile chain interaction (core combo).

---

#### Agent 8: `mancers-group-b`
**Worktree:** `C:/Projects/BattlemancersAgents/mancers-group-b`
**Branch:** `agent/mancers-group-b`

**Owns (creates):**
- `design/mancers/necromancer.md`
- `design/mancers/chronomancer.md`
- `design/mancers/photomancer.md`
- `design/mancers/psychomancer.md`

**Dependencies:** Same as Group A

**Task:** Same as Group A for these four Mancers. For Necromancer: explicitly document the Ashen Covenant Remnant token interaction and corpse economy mechanics. For Psychomancer: document interaction with Gilded Throne's Iron Discipline immunity.

---

#### Agent 9: `element-resolver`
**Worktree:** `C:/Projects/BattlemancersAgents/element-resolver`
**Branch:** `agent/element-resolver`

**Owns (creates):**
- `src/simulation/ElementResolver.cs`
- `src/simulation/Interaction.cs`
- `src/simulation/ElementType.cs`
- `assets/data/element-interactions.json`

**Dependencies:** `GridData` interface (from grid-system), `SimulationEvent` types (from sim-core), `TileState` enum (from grid-system)

**Task:** Implement the element interaction system. **Zero Unity dependencies.**

- `ElementType` enum: FIRE, WATER, ICE, LIGHTNING, EARTH, WIND, POISON (extensible)
- `Interaction` class: `TileState resultingState`, `Effect[] effects`, `VFXTag vfxHint` (VFXTag is a string enum — just define the string constants)
- `ElementResolver` class: `Dictionary<(TileState, ElementType), Interaction>` table, `Resolve(TileState existing, ElementType incoming) → Interaction` method, `LoadFromJson(string json)` to populate table from `element-interactions.json`
- `element-interactions.json`: full interaction matrix from `CLAUDE.md`, structured as `{ "WET+FIRE": { "resultingState": "BURNING", "effects": [...], "vfxHint": "steam_cloud" }, ... }`
- Every cell in the 7×6 matrix from `CLAUDE.md` must be represented

**Context files:** `CLAUDE.md` (interaction matrix), `design/tech-stack.md` (Section 5)

**Interface contract:**
```csharp
ElementResolver.Resolve(TileState, ElementType) → Interaction
ElementResolver.LoadFromJson(string json)
```

---

#### Agent 10: `status-system`
**Worktree:** `C:/Projects/BattlemancersAgents/status-system`
**Branch:** `agent/status-system`

**Owns (creates):**
- `src/simulation/Status/StatusEffect.cs`
- `src/simulation/Status/StatusManager.cs`
- `src/simulation/Status/StatusTick.cs`
- `src/simulation/Status/StatusEvents.cs`

**Dependencies:** `SimulationState`, `SimulationEventBus` (from sim-core)

**Task:** Implement the status effect system. **Zero Unity dependencies.**

- `StatusEffect` class: `StatusType type` (enum: BURNING, WET, FROZEN, POISONED, CHARGED, SLOWED, STUNNED, PANICKED, CHARMED, CURSED, SILENCED), `int duration`, `int stackCount`, `string sourceId`
- `StatusManager`: `ApplyStatus(string unitId, StatusEffect effect, SimulationState state)` with stacking rules (some stack by count, some by duration — document the rule per type), `TickStatuses(SimulationState state) → StatusTickResult[]`
- `StatusTick`: result of one tick — damage dealt, status removed, status modified
- `StatusEvents`: `StatusApplied`, `StatusRemoved`, `StatusTicked` concrete SimulationEvent subclasses (inherit from `SimulationEvent` defined by sim-core agent)
- Stacking rules: BURNING stacks duration; POISONED stacks count (amplifies damage per stack); FROZEN replaces duration if longer; STUNNED cannot stack; PANICKED cannot stack; CHARMED cannot stack

**Context files:** `design/combat/status-effects.md`, `CLAUDE.md` (interaction matrix)

---

#### Agent 11: `pathfinding`
**Worktree:** `C:/Projects/BattlemancersAgents/pathfinding`
**Branch:** `agent/pathfinding`

**Owns (creates):**
- `src/core/Pathfinding/MovementRange.cs`
- `src/core/Pathfinding/LineOfSight.cs`
- `src/core/Pathfinding/PathfindingUtils.cs`

**Dependencies:** `GridData`, `Tile`, `TileState` (from grid-system)

**Task:** Implement pure C# pathfinding utilities. **Zero Unity dependencies.** A* Pathfinding Pro handles runtime pathfinding; these classes handle tactical game queries.

- `MovementRange`: `GetReachableTiles(GridData grid, Vector2Int origin, int moveRange, int moveCostOverrides) → HashSet<Vector2Int>` using BFS/flood fill; respects `Tile.passable`, elevation rules, and terrain movement cost (MUD = 2 cost, ICE = 0.5 cost, etc.)
- `LineOfSight`: `HasLineOfSight(GridData grid, Vector2Int from, Vector2Int to) → bool` using Bresenham's line; blocked by elevation difference > 1, SOLID tile states, full walls; `GetLineOfSightTiles(from, to) → List<Vector2Int>` returning the intermediate tiles checked
- `PathfindingUtils`: terrain movement cost lookup `GetMovementCost(TileState state) → float`, neighbor enumeration helpers

**Context files:** `design/combat/terrain-system.md`, `design/tech-stack.md` (Section 5 — grid architecture)

---

#### Agent 12: `test-framework`
**Worktree:** `C:/Projects/BattlemancersAgents/test-framework`
**Branch:** `agent/test-framework`

**Owns (creates):**
- `tests/Battlemancers.Tests.asmdef`
- `tests/Grid/GridDataTests.cs`
- `tests/Simulation/SimulationStateTests.cs`
- `tests/Simulation/TurnManagerTests.cs`
- `tests/Simulation/ElementResolverTests.cs`
- `tests/Pathfinding/MovementRangeTests.cs`
- `tests/Pathfinding/LineOfSightTests.cs`

**Dependencies:** All Wave 1 code agents (grid-system, sim-core, element-resolver)

**Task:** Scaffold NUnit test suites for all pure C# simulation classes. Tests do not need to pass (implementations may be stubbed); tests must compile and clearly express the intended behavior as assertions.

Each test class should include:
- At least 5 meaningful test cases per class
- `[SetUp]` methods initializing test fixtures
- Tests for edge cases (empty grid, no units, invalid inputs)
- Tests for the key game logic (wet + lightning = chain stun, movement range respects terrain cost, etc.)

---

## Task Specification Format

When the orchestrator spawns a subagent, provide this exact format:

```
BATTLEMANCERS AGENT TASK
========================
Agent ID: [agent-name]
Worktree: C:/Projects/BattlemancersAgents/[agent-name]
Branch: agent/[agent-name]
Wave: [1 or 2]

TASK SUMMARY:
[2-3 sentence description]

READ THESE FILES FIRST:
[list of context files]

CREATE THESE FILES:
[file path] — [description]

DO NOT MODIFY any existing files.

INTERFACE CONTRACT:
[What this agent's output must expose for other agents]

WHEN COMPLETE:
cd C:/Projects/BattlemancersAgents/[agent-name]
git add [specific files only — never git add -A]
git commit -m "[task summary]"
git push origin agent/[agent-name]
```

---

## Orchestrator Runbook

### Phase 1: Wave 1 Launch

1. Verify worktrees are set up (all 12 branches and worktrees exist)
2. Spawn all 6 Wave 1 agents simultaneously using the Agent tool
3. Monitor for completion signals (each agent pushes and reports done)
4. When all 6 Wave 1 agents report complete: review each branch

### Phase 2: Wave 1 Merge

For each completed Wave 1 branch:
```bash
cd C:/Projects/Battlemancers
git merge --no-ff agent/[name] -m "Merge agent/[name]"
```
Merge order suggestion: design docs first, then code (no dependency conflicts either way).

After all 6 merged:
```bash
git push origin main
```

### Phase 3: Update Wave 2 Worktrees

Wave 2 worktrees were created from the old `main`. Update them:
```bash
for each Wave 2 worktree:
  cd C:/Projects/BattlemancersAgents/[agent-name]
  git merge main
```

### Phase 4: Wave 2 Launch

Spawn all 6 Wave 2 agents simultaneously. They now have access to all Wave 1 output in their worktrees.

### Phase 5: Wave 2 Merge

Same as Phase 2.

### Phase 6: Cleanup

After all agents are merged:
```bash
# Remove worktrees
git worktree remove ../BattlemancersAgents/[name]  # repeat for each

# Delete agent branches (optional — keep for history if preferred)
git branch -d agent/[name]
git push origin --delete agent/[name]
```

---

## Conflict Prevention Rules

These rules are mandatory for all agents:

1. **Each agent only creates new files.** No agent modifies a file that existed in `main` before the agent branch was created.
2. **No two agents own the same file path.** The ownership table in this document is the authority.
3. **Code agents reference other agents' types by name** (e.g., `GridData`, `SimulationState`) without importing a project — the assembly will resolve at Unity import time, not at C# compile time in the agent's context.
4. **Design agents produce pure Markdown.** No code. No cross-references to files that don't exist yet.
5. **Agents never run `git add -A` or `git add .`** — always add specific files by name to avoid accidentally staging unintended files.

---

## File Ownership Map

| Agent | Owns |
|---|---|
| win-conditions | `design/combat/win-conditions.md` |
| pyromancer-design | `design/mancers/pyromancer.md` |
| hydromancer-design | `design/mancers/hydromancer.md` |
| grid-system | `src/core/Grid/*.cs` |
| sim-core | `src/core/Simulation/**/*.cs` |
| data-schemas | `src/data/*.cs`, `assets/data/schema-notes.md` |
| mancers-group-a | `design/mancers/{cryo,geo,aero,electro}mancer.md` |
| mancers-group-b | `design/mancers/{necro,chrono,photo,psycho}mancer.md` |
| element-resolver | `src/simulation/ElementResolver.cs`, `src/simulation/Interaction.cs`, `src/simulation/ElementType.cs`, `assets/data/element-interactions.json` |
| status-system | `src/simulation/Status/*.cs` |
| pathfinding | `src/core/Pathfinding/*.cs` |
| test-framework | `tests/**` |

---

## Status Tracking

| Agent | Wave | Status | Branch | Notes |
|---|---|---|---|---|
| win-conditions | 1 | COMPLETE | agent/win-conditions | Merged to master |
| pyromancer-design | 1 | COMPLETE | agent/pyromancer-design | Merged to master |
| hydromancer-design | 1 | COMPLETE | agent/hydromancer-design | Merged to master |
| grid-system | 1 | COMPLETE | agent/grid-system | Merged to master |
| sim-core | 1 | COMPLETE | agent/sim-core | Merged to master; AP fix applied (2→6) |
| data-schemas | 1 | COMPLETE | agent/data-schemas | Merged to master |
| mancers-group-a | 2 | RUNNING | agent/mancers-group-a | cryo, geo, aero, electromancer |
| mancers-group-b | 2 | RUNNING | agent/mancers-group-b | necro, chrono, photo, psychomancer |
| element-resolver | 2 | RUNNING | agent/element-resolver | ElementResolver.cs + interaction JSON |
| status-system | 2 | RUNNING | agent/status-system | StatusEffect, StatusManager, StatusTick |
| pathfinding | 2 | RUNNING | agent/pathfinding | MovementRange, LineOfSight, utils |
| test-framework | 2 | RUNNING | agent/test-framework | NUnit suites for all simulation code |

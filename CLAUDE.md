# Battlemancers — CLAUDE.md

## Agent Orchestration

This project uses **multi-agent parallel development** with git worktrees. Before starting any development work, read:

> **`design/agent-orchestration.md`** — full agent roster, worktree setup, task specification format, file ownership map, and orchestrator runbook

Subagents operate across multiple waves. Each agent owns exclusive file paths (no overlap) and pushes to its own branch. The orchestrator merges waves and spawns the next. **Do not modify files outside your assigned ownership domain.**

---

## Code Quality Standards

These standards apply to **all code in this project**, regardless of which agent writes it. Violations must be fixed before merging.

### DRY (Don't Repeat Yourself)

- **No duplicate logic.** If two places do the same thing, extract a shared method or class.
- **Named constants only.** No magic numbers in simulation code. Use `const` or `static readonly` fields with descriptive names (e.g., `TemperatureManager.ThresholdBurstDamage = 5`, not `damage += 5`).
- **One loader per data type.** A single class owns JSON parsing for each data type (e.g., `MancerDataLoader`, `MapLoader`). No inline `JsonSerializer.Deserialize` calls scattered across the codebase.
- **No copy-paste JSON parsing.** Use `BattlemancersJsonHelper` for shared deserialization patterns. Add helpers there rather than duplicating serializer options.

### Modularity

- **Single responsibility per class.** A class does one thing. `TemperatureManager` manages temperature. `SpellResolver` resolves spells. They do not do each other's jobs.
- **Constructor injection for dependencies.** Systems receive `TemperatureManager`, `StatusManager`, `ElementResolver`, etc. via constructor parameters — never via static access, singletons, or `FindObjectOfType`.
- **Event-based system communication.** Systems that need to notify others publish to `SimulationEventBus`. They do not hold references to presentation-layer objects or call Unity APIs directly.
- **No circular dependencies.** If A depends on B, B must not depend on A. Draw the dependency graph before introducing a new cross-system reference.
- **Interface boundaries between layers.** The simulation layer (`src/core/`) must never import anything from Unity (`UnityEngine`, `UnityEngine.UI`, `TMPro`, etc.). All Unity-coupled code lives in presentation/adapter layers only.

### Pure C# Simulation Layer

- **Zero Unity dependencies in `src/core/` and `src/simulation/`.** No `MonoBehaviour`, `ScriptableObject`, `Vector2`, `Vector3`, `Debug.Log`, or any `UnityEngine.*` namespace. Use `System.Numerics` if vector math is needed.
- **All simulation state lives in `SimulationState` and `UnitState`.** Do not store mutable game state in manager classes, static fields, or ScriptableObjects at runtime.
- **Determinism is required.** No `System.Random` without a seeded instance. No `DateTime.Now` for logic. No dictionary iteration where order matters (use sorted keys or lists).

### Data Layer Conventions

- **JSON field names must exactly match C# property/field names** (case-sensitive). The data pipeline uses `System.Text.Json` with default naming policy — no camelCase conversion.
- **ScriptableObjects** (`MancerData`, `SpellData`) are the Unity-side static definition layer. They are never mutated at runtime.
- **JSON runtime files** live in `assets/data/mancers/`. One file per Mancer archetype (e.g., `assets/data/mancers/pyromancer.json`). Spell data is nested inside the Mancer file under a `spells` array.
- **`DataRegistry`** is the single Unity-side indexing point for all ScriptableObject lookups. No other MonoBehaviour calls `Resources.Load` or `Addressables.LoadAsset` for game data.
- **`temperatureDelta`** is an `int` field on `SpellData` (added by `simulation-wiring` agent). Positive = heats target, negative = cools. 0 = no temperature effect. All Mancer JSON files must include this field on every spell entry.

### Testing Conventions

- All simulation tests extend `SimulationTestBase` (provides a pre-wired `SimulationState` with two players, standard grid, and all managers injected).
- Test method naming: `MethodName_Condition_ExpectedResult` (e.g., `ApplyTemperatureChange_CrossesThreshold_AppliesStatusEffect`).
- No Unity test runner for simulation tests — these are plain NUnit tests runnable headless.
- Each new system must ship with at least 3 tests: happy path, boundary/edge case, and failure/validation case.

### C# Naming Conventions

- **Classes / structs / enums:** `PascalCase`
- **Public properties and fields:** `PascalCase`
- **Private fields:** `_camelCase` (underscore prefix)
- **Constants:** `PascalCase` (not `ALL_CAPS`)
- **Local variables and parameters:** `camelCase`
- **Interfaces:** `IPascalCase` (e.g., `ICommand`, `IStatusEffect`)
- **Events / delegates:** `PascalCase`, past-tense or noun (e.g., `TemperatureChanged`, `UnitDied`)

---

## Project Overview

**Battlemancers** is a skill-based, turn-based tactical strategy game built around teams of elemental mages called Mancers. Players build squads from a roster of 19 Mancer archetypes and battle on destructible isometric battlefields where spells reshape terrain, create persistent elemental states, and chain into devastating cross-element combos.

**Core inspiration triad:**
- **Tactics (structure):** Fire Emblem / Final Fantasy Tactics — grid-based, per-unit movement + action economy
- **Physics/destruction (feel):** Worms Armageddon — terrain is a weapon, destruction creates tactical opportunity, positioning punishes and rewards
- **Aesthetic (visual):** Octopath Traveler — HD-2D: pixel sprite characters in real 3D environments with cinematic lighting, depth of field, bloom, and particle FX

---

## Design Pillars

1. **Elements reshape the battlefield.** Every spell can interact with terrain and existing elemental states. Fire burns ground, ice freezes water tiles, lightning chains through wet surfaces. The field evolves turn by turn.

2. **Synergy is the win condition.** Solo Mancers are limited; combos between Mancer types create exponential power. Team building is about designing combo chains, not stacking the strongest individuals.

3. **Skill lives in reading the board.** Initiative, positioning, range, line-of-sight, terrain state, and combo windows are all legible to a skilled player. Outcome should feel earned, not random.

4. **Visual clarity through spectacle.** HD-2D allows the game to look cinematic without sacrificing tactical legibility. Spell effects must be beautiful and readable simultaneously.

5. **Depth through interaction density.** 19 Mancers × terrain states × element combos creates a combinatorial space that rewards mastery without requiring memorization of explicit counters.

---

## Mancer Roster (19 types)

| Mancer | Domain | Tactical Identity |
|---|---|---|
| Pyromancer | Fire, heat, burning | DoT, area denial, spreading terrain fire |
| Hydromancer | Water, tides, fluids | Push/pull, wet terrain, healing, flow |
| Cryomancer | Ice, frost, cold | Slows, freezes, brittle armor, slippery tiles |
| Geomancer | Earth, stone, terrain | Walls, elevation, cover, terrain reshaping |
| Aeromancer | Wind, air, pressure | Displacement, evasion, projectiles, mobility |
| Electromancer | Lightning, electricity | Chains, stuns, conductivity, burst damage |
| Necromancer | Corpses, undead, death | Summons, corpse economy, attrition |
| Chronomancer | Time | Haste, delay, rewind, cooldown manipulation |
| Photomancer | Light, radiance | Vision, blinding, reveals, beams |
| Psychomancer | Mind, emotion, will | Charm, panic, confusion, morale damage |
| Floramancer | Plants, vines, pollen | Roots, growth zones, poison pollen, barriers |
| Faunamancer | Beasts, instincts, animals | Companion units, pack tactics, tracking |
| Toximancer | Poison, venom, toxins | Poison stacks, debuffs, contamination |
| Osteomancer | Bones, skeletons, structure | Bone armor, spikes, skeletal constructs |
| Gravimancer | Gravity, weight, force | Pulls, crushes, immobilizes, fall damage |
| Sonimancer | Sound, vibration, resonance | Cone attacks, disruption, silence, shatter |
| Crystalomancer | Crystals, prisms, resonance | Refraction, barriers, stored energy |
| Echomancer | Echoes, repetition, afterimages | Repeat casts, delayed duplicates, positional echoes |
| Thermomancer | Temperature, heat exchange | Gradients, overheat/chill combos, zone control |

---

## Warbands & Factions

Players build a **Warband** before each match — a 1,000-point list of Mancers and supporting infantry drawn from their chosen faction. This is the primary pre-game strategy layer, analogous to list-building in Warhammer 40k.

### Unit Costs
| Unit | T1 Cost | T2 (Veteran) Cost | Cap |
|---|---|---|---|
| Mancer | 100 pts base (upgradeable) | varies | 3 max |
| Chaff | 10 pts | 20 pts | budget only |
| Ranged | 25 pts | 50 pts | budget only |

T2 costs divide cleanly into the 100-pt activation budget (5 T2 Chaff = 100 pts; 2 T2 Ranged = 100 pts). Mancer upgrades increase warband cost but **activation cost stays fixed at 100 pts** regardless of upgrades.

### Three Factions

| Faction | Identity | Chaff | Ranged | Faction Trait |
|---|---|---|---|---|
| **The Gilded Throne** | Human empire, militaristic order | Conscript Spearmen | Crossbow Corps | Iron Discipline — immune to Panic and Charm |
| **The Verdant Pact** | Ancient nature covenant | Thornback Sentinels | Glade Archers | Terrain Bond — bonus movement + regen on natural tiles |
| **The Ashen Covenant** | Death cult, undead legion | Grave Husks | Wailing Shades | Deathless Ranks — no morale; deaths generate Necromancer fuel |

Mancers are **faction-agnostic** — any of the 19 Mancers can serve any faction.

### Simultaneous Blind Turns
- Both players plan activations secretly, then reveal simultaneously
- Each turn, a player activates up to **100 pts** of their warband (e.g., 1 Mancer, or 10 Chaff, or 4 Ranged, or any mix under 100)
- Partial activation is allowed; unactivated units hold position
- Resolution order on reveal: Mancers → Ranged → Chaff (ties broken by board position)

> Full detail in `design/warbands.md`

---

## Key Element Interaction Matrix

| Trigger \ State | Wet | Burning | Frozen | Poisoned | Charged |
|---|---|---|---|---|---|
| **Fire spell** | Steam cloud (blinds, burns) | Spreads fire | Melts → wet | Toxic fumes (AoE dmg) | Arc explosion |
| **Water spell** | — | Extinguishes fire | Cracks ice, deals dmg | Dilutes poison | Conducts → chain stun |
| **Ice spell** | Freeze tiles | Extinguish + flash freeze | Deeper freeze | Preserves/stacks | Freeze conductor |
| **Lightning** | Chain arc to adj. units | Firestorm burst | Shatter (high dmg) | Toxin shock (amplify) | Overload (AoE) |
| **Earth spell** | Mud (movement penalty) | Hardens into obsidian wall | Permafrost cover | Contaminates ground | Magnetize (pulls metal) |
| **Wind spell** | Mist dispersal | Fan flames (spread) | Ice shard spray | Disperses spores | Static buildup |
| **Poison** | Infected water | Toxic fire (DoT+poison) | Preserved state | Stack multiplier | Corroded conductor |

> Full interaction table in `design/combat/status-effects.md`

---

## Tech Stack

| Layer | Choice |
|---|---|
| Engine | Unity LTS + Universal Render Pipeline (URP) |
| Language | C# |
| VFX | Unity VFX Graph |
| Camera | Cinemachine |
| Pathfinding | A* Pathfinding Pro (custom grid layer above) |
| Simulation | Pure C# — fully decoupled from Unity (deterministic, testable, headless) |
| Audio | FMOD Studio |
| UI | UI Toolkit (menus/warband builder) + Canvas (in-game HUD) |
| Multiplayer | Unity Gaming Services — Relay + Lobby + Netcode for GameObjects |
| Data | ScriptableObjects (static definitions) + JSON (saves, interaction table) |
| Art | Aseprite (sprites) + Blender (3D terrain) |
| Distribution | Steam via Steamworks.NET |

**Critical architectural rule:** The simulation engine is pure C# with zero Unity dependencies. All game logic, turn resolution, element interactions, and spell effects run in plain C# classes. MonoBehaviours only handle rendering, input, and audio. This enables deterministic multiplayer (lockstep), unit testing, and replay support.

> Full reasoning and implementation detail in `design/tech-stack.md`

---

## Directory Structure

```
Battlemancers/
├── CLAUDE.md                        # This file — AI project context
├── README.md                        # Project pitch and overview
├── design/
│   ├── mancers/
│   │   └── overview.md              # Full roster, tactical identities, synergies
│   ├── combat/
│   │   ├── terrain-system.md        # Tile types, destruction, element-terrain
│   │   ├── spell-system.md          # AP economy, targeting, cooldowns, combos
│   │   ├── turn-structure.md        # Initiative, action phases, win conditions
│   │   └── status-effects.md        # Full status library, stacking rules
│   ├── visual/
│   │   └── art-direction.md         # HD-2D spec, VFX guidelines, camera rules
│   ├── maps/
│   │   └── map-design.md            # Map design principles, biome tile sets
│   ├── game-modes.md                # Campaign, skirmish, draft, multiplayer
│   ├── warbands.md                  # List building, factions, points, activation economy
│   ├── tech-stack.md                # Full engine/library/architecture recommendation
│   └── agent-orchestration.md      # Multi-agent dev model, worktrees, agent roster, runbook
├── src/
│   ├── core/                        # Grid, simulation loop, turn manager
│   ├── mancers/                     # Mancer base class, per-mancer spell data
│   ├── terrain/                     # Tile state machine, destruction, pathfinding
│   ├── simulation/                  # Effect resolver, interaction engine
│   ├── ui/                          # HUD, team builder, action menus
│   └── vfx/                         # Spell visuals, terrain FX, camera events
├── assets/
│   ├── sprites/                     # Mancer sprite sheets, tile sprites
│   ├── environments/                # 3D terrain meshes, biome assets
│   ├── vfx/                         # Particle systems, shader effects
│   ├── audio/                       # Spell SFX, ambient, music
│   └── ui/                          # UI sprites, fonts, icons
└── prototype/                       # Rapid prototyping scripts / throwaway code
```

---

## Development Priority Order

1. **Grid + terrain state** — core data model. Everything depends on this.
2. **Turn manager + action economy** — the simulation skeleton
3. **Two Mancers playable** (Pyromancer + Hydromancer) — validate interaction loop
4. **Element interaction resolver** — the cross-element combo engine
5. **Five more Mancers** — enough variety to test team building
6. **Basic map editor / preset maps** — content pipeline
7. **HD-2D visual pipeline** — sprite-in-3D world setup, lighting, VFX
8. **Full roster** — all 19 Mancers implemented
9. **Team builder UI + campaign skeleton**
10. **Polish pass** — camera work, VFX density, audio

---

## Key Design References

| Reference | What to borrow |
|---|---|
| Worms Armageddon | Terrain as weapon; destruction creates tactical options; skill in aim/physics |
| Into the Breach | Legibility; every enemy intent visible; positioning is king |
| Divinity: Original Sin 2 | Element combo depth; surface interactions; environmental storytelling |
| Octopath Traveler | HD-2D visual language; pixel+3D integration; cinematic camera on 2D sprites |
| Fire Emblem: Three Houses | Squad building depth; unit identity; tactical grid movement |
| Hades | Upgrade/combo clarity; ability synergy communication to player |

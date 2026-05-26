# Tech Stack — Battlemancers

## Executive Summary

| Layer | Choice |
|---|---|
| Engine | Unity LTS + Universal Render Pipeline (URP) |
| Language | C# |
| HD-2D Rendering | URP 2D Lighting + Sprite Renderer + Shader Graph + Post-Processing |
| VFX | Unity VFX Graph |
| Camera | Cinemachine |
| Grid & Pathfinding | Custom grid system + A* Pathfinding Pro |
| Simulation | Pure C# decoupled simulation layer |
| Audio | FMOD Studio |
| UI | Hybrid: UI Toolkit (menus/warband builder) + Canvas (in-game HUD) |
| Animation | Unity 2D Animation (sprite sheets) |
| Multiplayer | Unity Gaming Services — Relay + Lobby + Netcode for GameObjects |
| Data | ScriptableObjects (definitions) + JSON (saves/presets) |
| Art Tools | Aseprite (sprites) + Blender (3D environments) |
| Version Control | Git + Git LFS |
| Distribution | Steam (Steamworks.NET) |
| IDE | JetBrains Rider |

---

## 1. Engine — Unity LTS + URP

### Why Unity

Battlemancers has three demanding technical requirements that drive the engine choice:

1. **HD-2D rendering** — pixel sprite characters composited into a real 3D world with cinematic post-processing
2. **Simulation-heavy game logic** — deterministic turn resolution, element interaction matrix, simultaneous plan evaluation
3. **High-density VFX** — spell effects that must be both beautiful and clearly readable at a tactics camera distance

Unity (LTS) satisfies all three better than any alternative at indie scale:

- **VFX Graph** is the most capable GPU particle system available outside of AAA engines. Spell effects at the complexity this game demands — chained lightning, spreading fire, steam clouds, ice shard sprays — need GPU-simulated particles. VFX Graph is the right tool.
- **2D Sprite Renderer within 3D world space** is well-understood in Unity and has established HD-2D precedents (including a widely-referenced Unity HD-2D sample project)
- **Tilemap + Grid component** gives a solid foundation for the isometric battlefield, though a custom grid layer will sit above it (see Section 5)
- **URP** gives access to Shader Graph, 2D lighting, and the full post-processing stack without HDRP's expense
- **C#** is strongly typed and excellent for simulation code. The type system enforces correctness on complex rules systems

### Why LTS Over Latest

Unity LTS (Long-Term Support) releases receive only bug and stability fixes, no feature additions, for two years. A multi-year tactics game project should not be riding a Unity version that adds breaking changes mid-development. Pin to the most recent LTS and only upgrade on major version boundaries.

### Why URP Over HDRP

HDRP targets photorealistic rendering at high per-pixel cost. The visual language of Battlemancers is intentionally stylized — pixel sprites are inherently non-photorealistic, and the battlefield environment should be readable, not hyperdetailed. URP gives:

- Full post-processing (bloom, depth of field, color grading, tone mapping)
- 2D Lighting that applies per-sprite lighting from world-space light sources
- Shader Graph for custom sprite and terrain shaders
- Lower per-frame cost, leaving GPU budget for VFX Graph particle simulation
- Better cross-platform headroom

HDRP would be overengineered overhead that fights against the art direction rather than supporting it.

### Why Not Godot 4

Godot 4 is a legitimate engine and the alternative if the team is committed to open-source tooling. The specific gap for this project:

- **VFX tooling is not yet mature enough** for the spell effect density this game needs. Unity's VFX Graph is several years ahead.
- The HD-2D shader pipeline in Godot requires more custom shader work with less community reference material
- Netcode ecosystem is less mature; Unity's UGS stack is more complete out of the box

Godot 4 would be the correct choice if the project had a smaller team with deep Godot experience, or if VFX ambition were significantly reduced.

### Why Not Unreal Engine 5

UE5 is the right engine for cinematic 3D AAA games. It is wrong for this project:

- Blueprint + C++ is a more complex workflow than pure C# for simulation-heavy code
- Sprite-in-3D rendering requires more configuration against UE5's grain
- Asset pipeline is heavier than needed
- Licensing (royalty above certain revenue thresholds) adds business complexity

---

## 2. Language — C#

Unity's native language. No decision needed here. The relevant architectural choice is how C# is *used*:

- **Pure C# classes** for all simulation logic (see Section 5 — this is the most important architectural decision in the project)
- **MonoBehaviours** only for Unity integration points (rendering, input, audio triggers)
- **ScriptableObjects** for static game data (Mancer definitions, spell parameters, terrain type properties)
- **Interfaces** for the boundary between simulation and presentation layers

Rider (JetBrains) is strongly recommended over Visual Studio for C# in Unity. Better Unity integration, faster indexing, superior refactoring tools.

---

## 3. HD-2D Rendering Pipeline

The visual target is Octopath Traveler / Triangle Strategy: pixel sprite characters on a real 3D battlefield with cinematic depth, atmospheric lighting, and pronounced post-processing. Achieving this in Unity URP requires several cooperating systems:

### Camera Setup

- **Isometric projection:** Fixed-angle camera looking down at approximately 60° elevation with an orthographic or near-orthographic projection. True perspective is optional for cinematic moments but the gameplay view should be isometric.
- **Cinemachine Virtual Cameras:** Multiple virtual cameras for different states — standard gameplay overview, action close-up (unit ability cams), cinematic spell intros, match-start establishing shot.
- **Camera Z-sorting:** Sprites must sort correctly against 3D terrain. Unity's Transparency Sort Mode set to Custom Axis with the isometric projection axis.

### Sprite Rendering

- **SpriteRenderer** component with `Sprites-Default` material replaced by a custom URP Sprite Lit material
- Sprites rendered in 3D world space at billboard orientation (always face camera)
- **Sprite Atlas** for batching: all units in one atlas, terrain overlays in another
- **Sorting Layers:** Background → Terrain → Units → Effects → UI. Each layer sub-sorted by Y position for isometric depth correctness.

### URP 2D Lighting

- **Global Light** for ambient battlefield illumination (matches biome — blue tint in ice maps, orange in fire maps, neutral in default)
- **Point Lights** attached to spell VFX: a Fireball creates a warm point light as it travels; lightning chains pulse a white-blue burst
- **Sprite Normal Maps** for depth response to lighting — even pixel art benefits from subtle normal map lighting; makes units feel grounded in the 3D environment

### Shader Graph — Key Shaders

| Shader | Purpose |
|---|---|
| Sprite Outline | Unit selection highlight; faction-colored ring around selected/hovered unit |
| Status Effect Overlay | Color-tint pass for status conditions: blue shimmer (frozen), red pulse (burning), green dither (poisoned) |
| Dissolve / Disintegrate | Death animation — unit dissolves from bottom up with elemental color |
| Terrain State | Ground tile state visuals: cracked ice, wet shine, ember glow, vine coverage |
| Depth Fade | VFX particles that fade as they approach terrain (prevents hard clip) |
| Fresnel Rim Light | Faint rim light on sprites to separate them from background |

### Post-Processing Stack (URP Volume)

| Effect | Usage |
|---|---|
| Bloom | Spell hit flashes, status effect auras, magic charging animations |
| Depth of Field | Background terrain out-of-focus; spell close-up cameras use foreground blur |
| Color Grading | Per-biome LUTs: warm desaturated for volcanic maps, cool oversaturated for arctic, vivid greens for forest |
| Tonemapping | ACES tonemapping for consistent HDR-like look without HDRP |
| Vignette | Subtle vignette on gameplay view; heavy vignette during dramatic spell moments |
| Chromatic Aberration | Brief hit on large spell impacts — restrained, 1–2 frames max |

### The 3D Environment

Terrain is 3D geometry, not sprite tiles. This is what makes HD-2D work — the environment has real depth and casts real shadows on sprite units. Terrain tiles are:

- 3D mesh tiles modeled in Blender (grass block, dirt block, stone block, elevated platform, water plane)
- PBR materials under URP Lit shader (the environment is physically rendered; sprites are lit to match)
- Destructible tiles swap meshes on state change (a standing stone tile swaps to a rubble mesh when destroyed)
- Animated terrain states: water tiles have animated normal map scrolling; fire tiles have a particle system parented to them; ice tiles get a reflective material swap

---

## 4. Grid System Architecture

### Do Not Use Unity's NavMesh

NavMesh is designed for 3D free-movement agents. Battlemancers is a discrete grid game. NavMesh baking, sampling, and pathing on a grid will produce incorrect results in edge cases and add complexity for no benefit. Build a custom grid.

### Custom Grid Layer

```
GridManager (MonoBehaviour)
  └── GridData (pure C# class — the simulation layer)
        ├── Tile[,] grid  (2D array indexed by [x, y])
        └── Tile (pure C# struct/class)
              ├── Vector2Int position
              ├── TileState state  (enum: NORMAL, WET, BURNING, FROZEN, etc.)
              ├── int elevation    (0 = ground, 1+ = raised platform)
              ├── bool passable
              ├── bool occupied
              └── UnitID occupant  (null if empty)
```

- `GridData` is a pure C# class with zero Unity dependencies — fully testable
- `GridManager` MonoBehaviour owns a `GridData` instance and handles the Unity representation (instantiating 3D tile meshes, updating them when state changes)
- The separation means the simulation can query and mutate `GridData` without touching Unity APIs

### Pathfinding

**A* Pathfinding Pro** (Arongranberg, ~$100 on Unity Asset Store) is the industry standard for Unity grid pathfinding. It supports:

- Grid graphs with custom node connections
- Custom cost functions (mud tiles cost more movement; ice tiles cost less but apply a slide)
- Multiple agent types (Mancer movement range vs. projectile path vs. line-of-sight)
- Excellent performance at the grid sizes this game uses (24×24 to 48×48 tiles)

Implementing A* from scratch is straightforward but adds time with no practical upside — this is one of the few areas where an asset purchase is clearly justified.

### Line of Sight

Custom implementation, not A* Pro's LOS (which is optimized for 3D, not discrete grids):

- **Bresenham's line algorithm** from caster to target, stepping through intermediate tiles
- Each intermediate tile checks: elevation blocks LOS if taller than the line, SOLID terrain state blocks LOS, full walls block LOS
- Spell targeting highlights valid targets in real time — this runs every frame during targeting mode, must be fast

### Movement Range

- **Flood fill from unit position** up to movement range (accounting for passability and terrain cost)
- Cache the reachable set each time a unit is selected; invalidate on any tile state change
- Visualized as a tile highlight overlay (colored tiles in the movement layer)

---

## 5. Simulation Engine — The Most Important Architecture Decision

**The simulation engine must be entirely decoupled from Unity.** This is non-negotiable.

### Why

1. **Testability:** The interaction between element states, terrain, spells, and unit positions is extraordinarily complex. If the simulation lives in MonoBehaviours, you cannot unit test it without running the Unity editor. Pure C# classes can be tested with standard NUnit tests.

2. **Determinism:** The simultaneous blind turn system requires that given identical inputs, the simulation always produces identical outputs. Unity's physics and rendering systems introduce floating-point non-determinism. The simulation must never touch them.

3. **Multiplayer:** The simultaneous turn system maps naturally to a **lockstep network model** — both clients run the same deterministic simulation; only player inputs are synced. This only works if the simulation is deterministic and headless-executable.

4. **Replay:** A decoupled simulation means replays are trivially achievable by recording input events and replaying them through the simulation. No special replay system needed.

### Architecture

```
Simulation Layer (pure C#, no Unity dependencies)
  ├── SimulationState
  │     ├── GridData
  │     ├── UnitCollection
  │     └── TurnState (PLANNING | RESOLVING | END)
  ├── TurnManager
  │     ├── CollectActivationPlans(PlayerPlan[])
  │     ├── ValidatePlans()
  │     └── ResolveActivations() → SimulationEvent[]
  ├── ElementResolver
  │     └── Resolve(TileState existing, ElementType incoming) → Interaction
  ├── SpellResolver
  │     ├── CalculateTargets(SpellData, origin, gridData) → Tile[]
  │     └── ApplyEffect(SpellData, Tile[], SimulationState) → SimulationEvent[]
  └── StatusManager
        ├── ApplyStatus(UnitID, StatusEffect)
        └── TickStatuses(SimulationState) → SimulationEvent[]

Presentation Layer (Unity MonoBehaviours, reads from simulation)
  ├── SimulationRunner — drives simulation, consumes SimulationEvent[]
  ├── UnitViewController — animates units based on events
  ├── TileViewController — updates tile meshes/materials on state change
  ├── VFXDirector — spawns VFX Graph effects triggered by events
  └── AudioDirector — triggers FMOD events from simulation events
```

The **SimulationEvent** is the critical boundary. The simulation emits events (`UnitMoved`, `SpellHit`, `TileStateChanged`, `UnitDied`, etc.) and the presentation layer consumes them. The simulation does not know or care about what happens visually.

### Element Interaction Matrix

A 2D lookup table indexed by `[existing TileState, incoming ElementType]`:

```csharp
// ElementResolver.cs
Dictionary<(TileState, ElementType), Interaction> interactionTable;

struct Interaction {
    TileState resultingState;
    Effect[] effects;   // damage, status, AoE, etc.
    VFXTag vfxHint;     // tells VFXDirector what to play
}
```

Populated from a JSON or ScriptableObject-defined data table, editable without code changes. New interactions can be added by designers without touching the resolver logic.

### Command Pattern for Actions

Player activations are serialized as **Commands** before execution:

```csharp
abstract class Command { }
class MoveCommand : Command { UnitID unit; Vector2Int destination; }
class SpellCommand : Command { UnitID caster; SpellID spell; Vector2Int target; }
class AttackCommand : Command { UnitID attacker; UnitID defender; }
```

Commands are validated before resolution (can this unit reach that tile? does this spell have range?). Invalid commands are rejected at lock-in, not at execution. In multiplayer, Commands are what gets synced over the network.

---

## 6. VFX Stack

### Unity VFX Graph

Every major spell effect uses VFX Graph (GPU-simulated particles). Advantages:

- GPU particles: thousands of particles per effect at low CPU cost
- Visual scripting in the VFX Graph editor (non-programmers can author effects)
- Sub-graph system for reusable effect modules (fire, smoke, spark — composited into different spell effects)
- Deep Unity integration: VFX Graph can read from C# exposed properties, animates with Cinemachine, syncs to audio via event bindings

Key VFX categories:

| Category | Examples |
|---|---|
| Spell projectile | Fireball travel, ice lance, lightning bolt |
| Spell impact | Hit burst, AoE ring, shockwave ripple |
| Terrain state | Fire spread particles, ice crystal growth, poison cloud |
| Unit status | Burn aura, freeze crystallization, poison drip |
| Tile transition | State change flash, elemental interaction burst (steam cloud, arc explosion) |
| Unit action | Cast wind-up, movement dust, death dissolve |

### Shader Graph

Used for all custom surface and overlay shaders (see Section 3 — HD-2D pipeline). Shader Graph enables:

- Non-programmer authoring of shaders (artist/technical-artist tool)
- URP-compatible output
- Sub-graph reuse (a "pulsing emissive" sub-graph reused across all status shaders)

### Cinemachine

Virtual camera system for all camera behavior:

- **CM FreeLook / Tracked** camera follows the active area of play during resolution
- **CM State-Driven Camera** switches between overview / close-up / cinematic rigs based on game state
- **CM Impulse** for camera shake on major spell impacts (scaled by spell power)
- **CM Timeline** integration for any scripted cinematic moments (campaign cutscenes, tutorial sequences)

Camera shake is reserved for high-impact moments only — overuse destroys the tactical readability that's a core design pillar.

---

## 7. Audio — FMOD Studio

### Why FMOD Over Unity Audio

Unity's built-in audio (AudioSource + AudioMixer) is sufficient for simple games. Battlemancers needs:

- **Adaptive music** that responds to game state (tension increases as both players lock in activations, resolves on turn end)
- **Event-driven sound design** that maps cleanly to simulation events
- **Layered audio** for spells (a fireball has an ignite layer, a travel layer, and an impact layer — each can vary independently)
- **Real-time parameter control** (a burning tile's crackle volume scales with how many tiles are on fire)

FMOD Studio handles all of this. Wwise is the direct alternative and equally capable — FMOD is chosen for slightly simpler Unity integration and a more accessible Studio interface.

### FMOD + Simulation Events

The `AudioDirector` MonoBehaviour subscribes to simulation events and maps them to FMOD event triggers:

```
SpellHit → FMOD: "spell/{element}/impact"
TileStateChanged BURNING → FMOD: "terrain/fire/ignite"
UnitDied → FMOD: "unit/{faction}/death"
TurnResolveBegin → FMOD: "music/tension_peak"
```

FMOD's parameter system allows continuous variation (the music builds through the planning phase) without managing complex audio state in code.

---

## 8. UI Architecture

### Hybrid Strategy: UI Toolkit + Canvas

Two different UI systems serve different needs:

**UI Toolkit (UIElements/UXML/USS)** for:
- Warband builder / list editor
- Faction selection screen
- Main menu and settings
- Match setup screens

UI Toolkit is Unity's modern, CSS-like UI framework. For complex, data-driven screens (the warband builder is essentially a filtered list builder with live point budget tracking), UI Toolkit's data binding and styling system is far more maintainable than nested Canvas GameObjects.

**Canvas (uGUI)** for:
- In-game HUD (HP bars, turn order display, action menu, status icons)
- In-world unit labels and range overlays
- Tile state indicators

Canvas integrates more naturally with 3D world space for in-game overlays. HP bars positioned above units in world space, tile overlays rendered in world space — Canvas handles this well. UI Toolkit's world-space support is less mature.

### Action Menu Design

The in-game action menu (what a selected unit can do) should be **positioned in world space near the unit**, not in a fixed screen-space panel. This keeps the player's eyes on the board during decision-making. Implemented as a Canvas World Space component.

---

## 9. Multiplayer Architecture

### The Simultaneous Turn System and Networking

Battlemancers' blind simultaneous turn system is unusually well-suited to networking: the amount of data synced per turn is minimal (each player sends a set of Commands — typically 1–10 small objects), and the natural "planning phase" provides a hide-behind for network latency.

### Recommended: Unity Gaming Services (UGS)

UGS provides:
- **Lobby:** Matchmaking lobby for players to find each other
- **Relay:** Peer-to-peer relay servers — clients connect through Unity's relay rather than exposing IPs; Unity hosts the relay infrastructure
- **Netcode for GameObjects (NGO):** Unity's official netcode library; handles connection management, object sync, and RPCs

This stack is free below significant player counts and scales without self-hosted infrastructure. Correct choice for an indie project pre-launch.

### Turn Resolution Model

```
PLANNING PHASE:
  Client A: builds CommandSet_A (hidden)
  Client B: builds CommandSet_B (hidden)

LOCK-IN:
  Client A sends CommandSet_A to relay
  Client B sends CommandSet_B to relay

  (neither client receives the other's commands until both submitted)

REVEAL:
  Relay delivers both CommandSets to both clients simultaneously

RESOLUTION:
  Both clients run identical deterministic simulation:
    SimulationState.Resolve(CommandSet_A, CommandSet_B)
    → identical SimulationEvent[] on both clients

PRESENTATION:
  Both clients play back events (animation, VFX, audio)
  State is verified to be identical before next planning phase
```

The simultaneous delivery on lock-in can be implemented as: server holds both command sets and releases them only once both are received. NGO's RPC system handles this pattern cleanly.

### Determinism Requirements

For lockstep to work, the simulation must be bit-for-bit deterministic:
- **No floating-point math in the simulation** — use integer or fixed-point arithmetic for all unit stats, damage values, range calculations, and effect propagation. Reserve floats for visual/audio layers only.
- **No Random in the simulation** — Battlemancers is designed as a skill game; randomness is actively avoided. If any randomness is ever added, it must use a seeded deterministic RNG synced between clients.
- **No Time.deltaTime, Physics, or any Unity API** in the simulation layer — guaranteed by the architecture in Section 5.

### Local / Offline Play

Multiplayer is the target, but local and AI play must work without a network connection. The simulation layer is already fully decoupled — running a local match is just: feed both players' commands from local input rather than the network relay. No code changes needed between local and networked play.

---

## 10. Data Architecture

### ScriptableObjects — Static Definitions

ScriptableObjects (SOs) are Unity's serializable data container assets. Use them for **data that doesn't change at runtime**:

```
MancerData (SO)
  ├── string mancerName
  ├── int baseHP
  ├── int moveRange
  ├── Sprite portrait
  ├── SpellData[] baseSpells
  └── UpgradeOption[] upgrades

SpellData (SO)
  ├── string spellName
  ├── ElementType element
  ├── TargetType targetType
  ├── int range
  ├── int baseDamage
  └── Effect[] effects

TileTypeData (SO)
  ├── TileState state
  ├── bool passable
  ├── int movementCost
  └── Sprite[] visualVariants
```

SOs are inspectable and editable in the Unity Inspector — designers can tune stats without touching code.

### JSON — Saves and Presets

- **Warband saves:** Player's saved warband configurations serialized to JSON
- **Preset warbands:** Pre-built example warbands for tutorial / quick play
- **Element interaction table:** The full interaction matrix (see Section 5) defined in JSON so it can be edited without recompile
- **Campaign mission configs:** Mission objectives, enemy warband definitions, starting tile states

Unity's `JsonUtility` for simple cases; **Newtonsoft.Json (Json.NET for Unity)** for complex nested structures. Json.NET is available free on the Unity Asset Store.

### Runtime Game State

Live game state (current HP, tile states, unit positions, status effects, turn counter) is held in pure C# classes — never in SOs (SOs are shared assets; mutating them during play creates state leakage between play sessions).

---

## 11. Asset Pipeline

### Sprites (2D Characters)

- **Aseprite** for all pixel art creation and animation — industry standard, ~$20, exports sprite sheets and JSON animation data natively
- Export process: Aseprite → sprite sheet PNG + JSON animation data → Unity Sprite Editor slice → Unity 2D Animation rig
- Sprite resolution target: ~64×64 to 128×128 pixels per character frame (HD-2D scales well — bigger sprites allow more detail)
- Each Mancer needs: idle, move, attack wind-up, spell cast, hit, death animations. Supporting units need: idle, move, attack, death.

### 3D Environments

- **Blender** for all 3D terrain tile meshes (free, industry standard for indie)
- Tile design philosophy: modular 1×1 unit blocks that can be stacked for elevation, placed edge-to-edge for flat ground, combined for environment variation
- Export: FBX to Unity, PBR materials re-applied in URP Lit shader
- Destructible tile variants: each tile type needs a "destroyed" mesh variant (crumbled stone, scorched earth, shattered ice)

### Version Control for Binary Assets

- **Git LFS (Large File Storage)** for all binary files: sprite sheets, 3D meshes, audio files, VFX asset files, textures
- Without LFS, Git history bloats to unusable sizes as binary assets iterate

### Audio Assets

- Sound design: **Reaper** (DAW) or similar for editing/layering
- FMOD Studio builds banks from raw audio files — source audio kept in `assets/audio/source/`, compiled FMOD banks in `assets/audio/fmod-banks/` (Git LFS tracked)

---

## 12. Third-Party Assets and Libraries

| Asset | Source | Cost | Purpose |
|---|---|---|---|
| A* Pathfinding Pro | Unity Asset Store | ~$100 | Grid pathfinding |
| Newtonsoft.Json | Unity Asset Store | Free | Complex JSON parsing |
| DOTween | Unity Asset Store | Free (Pro ~$15) | UI animations, movement tweens |
| Steamworks.NET | GitHub (free) | Free | Steam API integration |
| FMOD for Unity | FMOD website | Free (royalty model) | Audio middleware |
| TextMeshPro | Built into Unity | Free | All in-game text |
| Cinemachine | Built into Unity | Free | Camera system |
| Unity VFX Graph | Built into Unity | Free | Particle VFX |
| Unity 2D Animation | Built into Unity | Free | Sprite animation |
| Unity Gaming Services | Unity website | Free tier | Lobby, Relay, Netcode |

Total estimated third-party cost: **~$115** (A* Pro + DOTween Pro). Everything else is free or built-in.

---

## 13. Build Targets

### Primary: PC (Windows + macOS)

- Windows x64 via Unity IL2CPP build
- macOS via Unity IL2CPP build (Universal Binary for Apple Silicon + Intel)
- Distributed via **Steam** — Steamworks.NET for achievements, cloud saves, multiplayer invites

### Secondary Targets (post-launch consideration)

| Platform | Feasibility | Notes |
|---|---|---|
| Linux | High | Unity IL2CPP supports Linux; Steam Deck compatibility is valuable |
| Nintendo Switch | Medium | Unity has Switch support; tactics games excel on Switch; requires Nintendo developer license |
| iOS / Android | Lower | URP mobile compatibility is good; touch UI redesign required; monetization model shift |
| Xbox / PlayStation | Medium | Unity console ports require platform certification; feasible but adds significant QA overhead |

---

## 14. Development Setup Order

Set up these systems in this sequence — each one is required by the next:

1. **Unity LTS project creation** — URP template, configure post-processing stack, set up isometric camera with Cinemachine
2. **Grid system** — `GridData` class, `GridManager` MonoBehaviour, tile mesh instantiation, isometric sort mode
3. **Simulation skeleton** — `SimulationState`, `TurnManager`, `SimulationEvent` bus, `SimulationRunner` MonoBehaviour
4. **A* Pathfinding Pro** — integrate, configure grid graph, verify pathfinding on the test grid
5. **Two unit types rendered** — Mancer sprite on grid, basic movement, animation state machine
6. **First spell + element interaction** — Pyromancer fireball, tile state change, VFX Graph fire impact
7. **Turn resolution loop** — blind planning input, simultaneous reveal, resolution playback
8. **UI Toolkit warband builder** — basic list with point budget counter (validates the data architecture)
9. **FMOD integration** — audio director, first sound events wired to simulation events
10. **UGS multiplayer** — Lobby + Relay + NGO, networked turn resolution
11. **HD-2D visual pass** — URP 2D Lighting, post-processing volumes per biome, Shader Graph status overlays
12. **Full VFX Graph suite** — all spell effects, terrain state particles, camera shake

# Terrain System

## Philosophy

Terrain is not just the floor. Terrain is a weapon, an obstacle, a resource, and a timer. Every spell that hits the ground should leave something behind. Skilled players read the terrain state two turns ahead — they set fires knowing the wind will spread them, flood zones knowing the Electromancer will chain through next turn.

Reference: **Worms Armageddon** — terrain destruction creates opportunities. A crater is a new choke point. A missing floor tile is a fall hazard. Nothing on the battlefield is inert.

---

## Grid Specification

- **Grid type:** Isometric square grid (viewed at ~45 degrees, standard tactics angle)
- **Tile size:** 1 unit × 1 unit (movement costs are measured in tiles)
- **Elevation levels:** 0 (ground), 1 (raised/hill), 2 (high ground), -1 (pit/depression)
- **Adjacency:** 4-directional for movement; 8-directional for area effects and AoE
- **Line of sight:** computed via ray from attacker tile center to target tile center; elevated tiles block LOS to lower tiles behind them

---

## Tile Types

### Base Terrain Types

| Tile Type | Movement Cost | LOS | Special |
|---|---|---|---|
| `GROUND` | 1 | Normal | Default tile |
| `RUBBLE` | 2 | Provides cover (50% damage reduction from ranged) | Created by terrain destruction |
| `ELEVATED` (+1) | 1 | Blocks LOS to behind it; grants +1 range to standing unit | Created by Geomancer, natural |
| `PIT` (-1) | 2 (to exit) | No cover | Fall damage if unit pushed into; created by explosions |
| `VOID` | Impassable | N/A | Tile destroyed entirely (heavy explosion); unit that enters is KO'd |
| `WATER_SHALLOW` | 2 | Normal | Conductive; becomes ICE_TILE if frozen |
| `WATER_DEEP` | Impassable (unless flying/swimming) | Normal | Conductive; drowning if pushed in without flight |
| `LAVA` | Impassable | Normal | 20 HP/turn for adjacent units |

### Destructible Terrain Features

These are placed on top of base tiles and can be destroyed.

| Feature | HP | Destroyed By | Blocks Movement | Blocks LOS |
|---|---|---|---|---|
| `STONE_WALL` | 40 | Physical, explosive, Gravimancer crush | Yes | Yes |
| `ICE_WALL` | 20 | Fire, Sonic, Pyromancer, physical | Yes | No (translucent) |
| `BONE_WALL` | 25 | Sonic, physical | Yes | No |
| `THORN_WALL` | 15 | Fire, any AoE | Yes | No |
| `CRYSTAL` | 30 | Sonic | No | No |
| `BONE_SPIKE` | 5 | Any | No (hazard) | No |

---

## Elemental Terrain States

Terrain tiles can hold one `PRIMARY_STATE` and one `SECONDARY_STATE` at a time. Some states expire after N turns; some are permanent until acted upon.

### Primary States

| State | How Applied | Effect on Units | Duration | Spread? |
|---|---|---|---|---|
| `ON_FIRE` | Pyromancer spells, explosion on flammable tile | 5 HP/turn to units on tile | Until extinguished | Yes — spreads to adjacent GROUND/GRASS each turn |
| `FLOODED` | Hydromancer flood spells, water tile adjacent overflow | Movement cost +1; conductive | Until dried/frozen | Slow — 1 tile/2 turns to adjacent |
| `ICE_TILE` | Cryomancer spells on GROUND/FLOODED | Voluntary movement costs +1 AP per tile; forced displacement (knockback/push/pull) extends +1 tile (guaranteed); conductive | Until melted | No |
| `MUD` | Water + Earth (Hydromancer + Geomancer) | Movement cost +2 | 4 turns | No |
| `TOXIC_TERRAIN` | Toximancer, Floramancer, corpse explosion | POISONED on enter | 3 turns | No |
| `CHARGED` | Electromancer | Lightning strike on unit entering | 1 use per tile | No |
| `OVERGROWTH` | Floramancer seed growth | Movement cost +2; ranged penalty | Until burned | Slow growth |
| `OBSIDIAN` | Earth tile + fire (hardened) | Impassable, indestructible by most means | Permanent | No |
| `STEAM_CLOUD` | Fire + WET/FLOODED | Blocks vision; light heat dmg per turn | 2 turns | No |
| `POISON_POLLEN` | Floramancer | POISONED on enter/stay | 3 turns | No |
| `PERMAFROST` | Deep Cryomancer effect or Geomancer + Ice | Movement cost +1; permanent ICE_TILE | Semi-permanent | No |
| `BONE_SPIKE` | Osteomancer | Damage + ROOT on stepping | Until destroyed | No |
| `THERMAL_GRADIENT` | Thermomancer | OVERHEATED (hot side) or OVERCOOLED (cold side) on unit | 4 turns | No |

### Secondary States

Secondary states layer on top of primary:
- `WET` (surface moisture — from rain, nearby water, Hydromancer mist) — makes tile conductive even if not fully FLOODED
- `FLAMMABLE` (dried grass, wood elements) — ON_FIRE spreads faster on FLAMMABLE tiles
- `BRITTLE` (Cryomancer ice-treated surfaces) — physical damage to structures on BRITTLE tiles is doubled

---

## Element-Terrain Interactions (Full Table)

When a spell element hits a tile with an existing state:

| Incoming \ Existing State | ON_FIRE | FLOODED / WET | ICE_TILE | TOXIC_TERRAIN | CHARGED | ON_FIRE + TOXIC |
|---|---|---|---|---|---|---|
| **Fire** | Spreads faster | STEAM_CLOUD (blind + heat dmg) | Melt → FLOODED + steam burst | TOXIC_FIRE (heavy DoT + poison) | Arc explosion (AoE fire) | Firestorm (large AoE) |
| **Water** | Extinguish → WET residue | — | Crack ice (dmg adj. units) | Dilute → halve stacks | Conduct + short (chain arc) | Extinguish toxic fire |
| **Ice** | Extinguish + FLASH_FREEZE (units in range frozen) | FREEZE (all units on FLOODED = FROZEN) | Thicken (more HP) | Preserve (stacks locked in, no expiry) | Freeze conductor (brittle) | Flash freeze toxic zone |
| **Lightning** | Arc explosion (burst AoE + fire spread) | Chain arc all adjacent WET units | SHATTER burst (highest dmg in game) | Toxin shock (all POISONED units take 2x) | OVERLOAD (AoE) | Chain arc + toxic shock |
| **Earth** | Smother (extinguish, creates rubble) | MUD (slows) | Permafrost + spikes | Contaminate (spread TOXIC to adj) | Ground (dissipate charge) | Obsidian trap |
| **Wind** | Fan (spread direction + speed) | Mist dispersal (AoE WET removed) | Ice shard spray (AoE dmg) | Disperse cloud (reduced range) | Static buildup (+ charge) | Toxic mist dispersal (AoE poison) |
| **Poison** | Toxic fire (ON_FIRE becomes toxic) | Contaminate water | Preserved + stacked | Stack multiplier | Corrode conductor (–duration) | Stack amplification |
| **Sonic** | Shockwave dispersal (extinguish) | Ripple knockback (all WET units pushed) | Shatter (immediate burst) | Cloud burst (AoE toxic pulse) | Resonance overload | — |

---

## Destruction and Terrain Deformation

Tiles can be physically destroyed or deformed by high-energy spells.

**Destruction rules:**
- AoE explosions above a damage threshold reduce base tile type:
  - `GROUND` → `RUBBLE` (first hit) → `PIT` (second heavy hit) → `VOID` (third)
  - `ELEVATED` → `GROUND` (loses elevation, becomes rubble)
  - `STONE_WALL` → `RUBBLE` pile (becomes passable difficult terrain)

**Geomancer exceptions:**
- Geomancer can `RESTORE` a PIT to GROUND (fill it)
- Geomancer can `RAISE` GROUND to ELEVATED
- Geomancer can `HARDEN` RUBBLE into STONE_WALL

**Worms principle:**
- Craters change sight lines, create new cover positions, and alter pathing
- Destroying a wall opens flanking angles
- Destroying a floor tile creates a hazard for pushback abilities
- Smart players destroy terrain offensively, not just to clear obstacles

---

## Elevation and Line of Sight

**High ground advantage:**
- Unit on ELEVATED tile has +1 range on all ranged spells
- Unit on HIGH_GROUND (level 2) has +2 range and bonus damage vs. lower targets
- Projectile spells fired downhill have increased blast radius (gravity assist)

**LOS rules:**
- ELEVATED tiles block LOS to tiles directly behind them at ground level
- ICE_WALL does NOT block LOS (translucent) but blocks movement
- STONE_WALL blocks both LOS and movement
- STEAM_CLOUD reduces effective vision range to 2 tiles for all units in/adjacent to it
- OVERGROWTH provides 25% cover (reduces ranged damage) but doesn't fully block LOS

**Fall damage:**
- Units pushed off an ELEVATED tile take `fall_distance × 8` HP damage
- HEAVY (Gravimancer status) doubles fall damage taken
- WEIGHTLESS (Gravimancer status) eliminates fall damage but makes unit more easily displaced

---

## Persistent State Turn Resolution

At the start of each round (before initiative), terrain resolves in this order:

1. `ON_FIRE` spreads to adjacent FLAMMABLE tiles
2. `ON_FIRE` deals damage to units standing on burning tiles
3. `STEAM_CLOUD` ticks duration down; deals heat damage to units inside
4. `FLOODED` expands to adjacent tiles (1 tile every 2 turns if still being fed by water source)
5. `TOXIC_TERRAIN` deals poison to units on tile
6. `OVERGROWTH` grows from SEED tiles
7. `CHARGED` tiles remain (no tick — they wait for unit contact)

---

## Map Biome Tile Sets (Planned)

| Biome | Base Tiles | Special Properties |
|---|---|---|
| Ruins | Stone, rubble, pit | Lots of pre-placed cover, collapsed walls, pit hazards |
| Swamp | Water shallow, mud, overgrowth | Pre-flooded zones, organic terrain easy to poison |
| Volcanic | Lava borders, obsidian, elevated rock | Lava = constant hazard; obsidian = indestructible cover |
| Crystal Cavern | Crystal terrain, stone, elevated | Pre-placed crystal structures for Photomancer/Crystalomancer synergy |
| Frozen Tundra | Ice tile, permafrost, elevated ice | Slippery everywhere; already WET terrain for Electromancer |
| Forest | Overgrowth, ground, elevation | Dense foliage (cover + Floramancer heaven), fire spreads fast |

> Full map design principles in `design/maps/map-design.md`

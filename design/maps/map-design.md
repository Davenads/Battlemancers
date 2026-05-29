# Map Design

## Core Principles

1. **Every map is a puzzle.** The terrain layout should create interesting decisions before a single spell is cast. Choke points, high ground, flanking routes, and hazard zones are deliberate design choices.

2. **Maps should be neutral but dynamic.** No map should strongly favor one Mancer type. Volcanic maps may have lava borders but Pyromancer shouldn't auto-win on them — neutral does not mean featureless.

3. **Destructibility creates narrative.** A map should look different at the end of a fight than the start. A clean ruins map becomes a crater field after a Geomancer/Gravimancer duel. This history should be readable.

4. **Size scales with team size.** 3v3: 10×10 tiles. 4v4: 12×12. 5v5: 14×14. Maps too large slow pacing; too small removes strategy.

---

## Map Size Conventions

| Format | Grid Dimensions | Team Size | Notes |
|---|---|---|---|
| **Standard** | 10×10 (100 tiles) | 3v3 | Default competitive size. Fast engagements, tight positioning. |
| **Large** | 12×12 (144 tiles) | 4v4 | Additional flanking space; biome variety encouraged. |
| **Extended** | 14×14 (196 tiles) | 5v5 | Reserved for campaign and special modes; rare in competitive pool. |

All preset maps ship in either Standard (10×10) or Large (12×12). The simulation engine accepts any Width × Height but competitive rulesets enforce the above.

---

## Tile Type Catalog

Each tile has a **TerrainType** (structural identity) and an optional **ElementState** (applied at runtime or preset). TerrainType is fixed at map load; ElementState changes during play.

### Ground Tiles (passable by default)

| TerrainType | Description | Default Element State | Notes |
|---|---|---|---|
| `Grass` | Open field, soft earth | None | Most common tile. Flammable — converts to `Burning` on fire contact. |
| `Stone` | Cobblestone, flagstone, hardpacked rock | None | Fire-resistant. Does not combust. Rubble state available after destruction. |
| `Sand` | Loose desert sand, beach | None | Slows movement by 1. Absorbs water (Wet state drains after 1 turn). |
| `Forest` | Dense undergrowth with canopy cover | None | +1 movement cost to enter. Provides cover (+15% dodge). Highly flammable. |
| `Corrupted` | Necrotic, darkened, magically tainted earth | Poisoned | Applies 1 Poison stack on entry. Floramancer growth is stunted here. Spreads slowly to adjacent Grass. |

### Hazard Tiles (passable unless noted)

| TerrainType | Description | Default Element State | Passable | Notes |
|---|---|---|---|---|
| `Water` | Shallow water, puddles, streams | Wet | Yes | Applies Wet status on entry. Conducts lightning to all adjacent Wet tiles. |
| `IceField` | Frozen-over ground, glacial flats | Frozen | Yes | Slippery: movement ends 1 tile further than intended (50% chance). Shatters under heavy impact. |
| `LavaChannel` | Slow-moving lava flow, volcanic fissure | Burning | No | Impassable. Deals 15 damage per turn to adjacent units. Spreads `Burning` to adjacent Grass/Forest. |

### Structural Tiles (impassable)

| TerrainType | Description | Destructible | Notes |
|---|---|---|---|
| `Wall` | Stone wall segment, ruin face | Yes (via Geomancer/Gravimancer/explosion) | Blocks movement and LOS. Elevation 2 for LOS calculation. Becomes `Rubble` on destruction. |
| `Rubble` | Collapsed wall debris | No (already destroyed) | Impassable but does NOT block LOS. Created from destroyed `Wall` tiles. |
| `Void` | Map edge, pit, chasm | No | Impassable permanently. Units knocked into Void are KO'd instantly. |

---

## Destructibility Rules Per Tile Type

Destruction converts a tile to a new state. Not all tiles can be destroyed.

| TerrainType | Destructible? | Trigger | Result State | Special |
|---|---|---|---|---|
| `Grass` | Yes | Fire (burn 3+ turns), explosion | `Ash` (treated as Stone, no longer flammable) | Ash cannot re-ignite. |
| `Stone` | Partial | Heavy explosion (Geomancer, Gravimancer), Sonimancer Shatter | `Rubble` (impassable) | Stone resists normal fire. Requires physical force. |
| `Wall` | Yes | Any explosion; Geomancer Reshape; Gravimancer Crush | `Rubble` | Blocked LOS is removed on destruction. |
| `Forest` | Yes | Fire (1 turn); Aeromancer Gale | `Ash` (passable) | Burning Forest spreads to adjacent Forest tiles immediately. |
| `Water` | No | — | Converts state only (Frozen, Electrified) | Cannot be physically destroyed; states transition. |
| `IceField` | Yes | Fire spell, Hydromancer Water spell | `Water` (temporary puddle) | Steam is released (1-turn Steam cloud on tile). |
| `LavaChannel` | No | — | — | Permanent map fixture. Can only be state-changed by large water volumes (creates `Obsidian` — impassable stone). |
| `Sand` | No | — | — | Absorbs elemental states instead of spreading them. |
| `Corrupted` | Partial | Holy/Light magic (Photomancer purification) | `Grass` | Requires dedicated purification spell; standard damage does not cleanse. |
| `Rubble` | No | — | — | Terminal state. Cannot be further destroyed. |
| `Void` | No | — | — | Terminal and permanent. |

---

## Elevation Levels

Elevation is an integer property on each tile. Standard maps use levels 0–3. Level 3 is rare and reserved for large maps and special biomes.

| Level | Name | Description | Tactical Effects |
|---|---|---|---|
| `0` | Ground | Default flat terrain | No modifiers. |
| `1` | Raised | Low hill, platform, rubble mound | +1 spell range for units on this tile. LOS to/from Level 0 is unobstructed. |
| `2` | High Ground | Cliff edge, tower base, elevated ruin | +2 spell range. Units below cannot see over this tile (LOS blocked unless adjacent). Melee attacks against uphill targets suffer −10% hit chance. |
| `3` | Peak | Volcanic summit, fortress parapet | +3 spell range. Extremely rare. Considered impassable wind zone: Aeromancer gains +1 AP on this tile. Accessible only by dedicated climbing routes. |

**Elevation change rules:**
- Units can move up or down 1 elevation level per move action with no cost penalty.
- Moving up 2 elevation levels in one step costs +1 AP.
- Elevation 3 requires a dedicated ramp/path tile; cannot be entered by normal movement from Elevation 1.
- Fall damage: A unit knocked off a tile to one 2+ levels below takes `2 × elevation_difference` damage.

---

## Spawn Zone Rules

Spawn zones define where each player places their Warband at match start. Every competitive map must satisfy these requirements:

1. **Minimum tiles:** Each player requires at least 4 spawn tiles, enough for a 3-Mancer roster with room for positioning choice.
2. **Deployment depth:** Spawn zones occupy the rear 2 rows of each player's side (rows 0–1 for Player 1, rows Height−2 to Height−1 for Player 2 on a standard orientation).
3. **Passability:** All spawn tiles must be passable (TerrainType ≠ Wall/LavaChannel/Void).
4. **Elevation parity:** If one player's spawn zone contains an elevated tile, the opposing player must have an equal or equivalent elevated tile. Asymmetry is permitted only in asymmetric campaign maps.
5. **No pre-placed hazard states in spawn zones:** Competitive maps must not have Burning, Poisoned, Electrified, or other hazard ElementStates on spawn tiles at match start.
6. **Mirror symmetry preferred:** Competitive maps should be rotationally symmetric (180° rotation maps Player 1's spawn to Player 2's spawn). Non-symmetric layouts are allowed for campaign/scenario maps only.

**SpawnZone identifiers:** `"player1"` and `"player2"`. Future expansion reserves `"neutral"` for scenario objectives.

---

## Line-of-Sight Blocking Rules Per Tile

LOS is computed via tile center-to-tile center ray. A tile blocks LOS if the ray passes through its space and the tile has a blocking attribute.

| TerrainType / State | Blocks LOS? | Notes |
|---|---|---|
| `Grass` (any state) | No | Open ground never blocks. |
| `Stone` | No | Flat stone does not block. |
| `Sand` | No | — |
| `Water` | No | — |
| `Forest` | Partial | Blocks LOS for targets 3+ tiles away through Forest. Adjacent targets visible. |
| `IceField` | No | Flat surface; does not block. |
| `LavaChannel` | No | Hazard but not a visual blocker. |
| `Wall` | Yes | Full LOS block regardless of elevation. |
| `Rubble` | No | Destroyed walls no longer block LOS. |
| `Void` | Yes | Map edge treated as hard blocker. |
| `Corrupted` | No | Tainted but flat ground. |
| **Elevation 2+ tile** | Yes (for lower units) | A tile at Elevation 2 blocks LOS for any unit at Elevation 0 trying to see past it. Units at Elevation 1 can see over Elevation 2 tiles only if within 2 tiles. |
| **Steam cloud** | Partial | Temporary state. Blocks LOS for all units not adjacent to the steam tile (cleared at end of turn). |

**Adjacency exception:** A unit adjacent (1 tile) to any LOS-blocking tile always has a clear line to that tile and any unit occupying it.

---

## Map Zones

Every map should have:
- **Deployment zone:** Where teams start (opposite sides, 2-tile deep zone)
- **Central contest zone:** Tiles that both sides can reach by turn 2 (high value for Hold objectives)
- **Flank routes:** 1-2 paths around the central zone; reward mobile Mancers
- **High ground:** At least 2 elevated tiles per map (incentivizes Geomancer; creates natural sniper positions)
- **Hazard features:** 1-3 pre-placed destructible features or terrain states (creates early tension)

---

## Biome Presets

Five curated biome templates. Each provides recommended tile composition, elevation profile, and hazard features. Maps need not use a single biome — mixed biomes are encouraged for large maps.

### 1. Ruins (Standard / Large)
- **Tile composition:** 60% Stone, 25% Grass, 10% Rubble (pre-placed), 5% Void (pit hazards)
- **Elevation:** 2–4 tiles at Elevation 1; 1–2 tiles at Elevation 2 (collapsed floors, tower stumps)
- **Pre-placed walls:** 3–5 Wall segments creating corridors; some already partially Rubble
- **Hazard features:** None by default — ruins start cold; combat creates history
- **LOS character:** Dense and varied; multiple chokepoints reward controlled engagements
- **Elemental identity:** Neutral. No elemental affinity; all 19 Mancers perform equally here.
- **Competitive suitability:** High. Default map pool candidate.

### 2. Frozen Wastes (Large — 12×12)
- **Tile composition:** 50% IceField, 20% Stone, 15% Water (frozen ponds, passability = false), 10% Grass (frosted), 5% Void (crevasse edges)
- **Elevation:** 1–2 tiles at Elevation 1; no Elevation 2 (flat tundra)
- **Pre-placed walls:** 2–3 IceWall features (treated as Wall, destructible by Fire spells)
- **Hazard features:** Frozen ponds (impassable until melted; Water after Cryomancer/Hydromancer interaction); all units start with 1 Chilled stack
- **LOS character:** Open sightlines; limited cover except ice formations
- **Elemental identity:** Cold-dominant. Cryomancer is strongest; Pyromancer melts terrain for tactical reshaping; Hydromancer can freeze water tiles further.
- **Competitive suitability:** Medium. Legal in competitive pool with awareness of Cryomancer advantage.

### 3. Ember Ridge (Standard — 10×10)
- **Tile composition:** 40% Stone, 25% Grass, 15% LavaChannel (borders and central fissure), 10% Sand (ash dunes), 10% Corrupted (volcanic corruption zone)
- **Elevation:** 3–4 tiles at Elevation 1 (obsidian ridges), 1–2 tiles at Elevation 2 (summit vantage)
- **Pre-placed walls:** 2 obsidian Wall clusters flanking the central lava fissure
- **Hazard features:** Central LavaChannel fissure (impassable, lethal adjacency); 2 pre-placed Burning Grass tiles near center
- **LOS character:** Broken by ridges; elevated tiles dominate central sightlines
- **Elemental identity:** Fire-dominant. Pyromancer spreads fire rapidly; Hydromancer can extinguish hazards for positional control; Cryomancer flash-freezes lava channels into Obsidian walls.
- **Competitive suitability:** Medium-low. High variance due to lava. Legal in competitive pool; banned in Draft Mode Season 1.

### 4. Flooded Marshlands (Standard / Large)
- **Tile composition:** 35% Water, 30% Grass, 20% Forest, 10% Sand (elevated islets), 5% Corrupted
- **Elevation:** 2–3 Sand tiles at Elevation 1 (natural islets); rest at Elevation 0
- **Pre-placed walls:** None — fully open; tall trees function as soft LOS blockers
- **Hazard features:** Water tiles covering center; Floramancer vines pre-rooted in Forest zones
- **LOS character:** Open across water; broken by Forest clusters near edges
- **Elemental identity:** Water/nature dominant. Hydromancer flood control; Floramancer terrain lock; Electromancer chains across all water tiles.
- **Competitive suitability:** Medium. Highly dynamic; balanced if both spawn zones include islet footing.

### 5. Crystal Cavern (Standard)
- **Tile composition:** 50% Stone, 20% Corrupted (cavern floor), 15% Void (underground pits), 10% Grass (moss patches), 5% IceField (underground cold seeps)
- **Elevation:** 4–6 tiles at Elevation 1 (crystal outcroppings); 2 tiles at Elevation 2 (upper ledges)
- **Pre-placed walls:** 4–6 Crystal Wall segments (treat as Wall; Sonimancer can shatter in chain)
- **Hazard features:** Low ambient light (all units start with Vision −1); Crystal Walls that chain-shatter; underground cold (1 Chilled stack gained if ending turn on IceField)
- **LOS character:** Dense. Crystal formations block almost all long-range sightlines. Combat is close-quarters.
- **Elemental identity:** Light/sound dominant. Photomancer beam bounces off crystals; Sonimancer shatters terrain rapidly; Cryomancer leverages cold seeps.
- **Competitive suitability:** Low. Asymmetric sightlines and vision penalties create high variance. Campaign / special modes preferred.

---

## Competitive Balance Principles

Rules enforced for all maps in the ranked and draft competitive pool.

### Layout Requirements

1. **Rotational symmetry (180°).** Both players must have equivalent positional access to high ground, cover, and hazards. A map is symmetric if rotating it 180° around the center produces an identical layout.

2. **Minimum three approach paths.** At least 3 distinct routes from one deployment zone to the other. No map may funnel all movement through a single chokepoint.

3. **High-ground parity.** Each player's side of the map must contain an equal count of Elevation 1 and Elevation 2 tiles accessible from their spawn zone within 3 turns.

4. **Hazard parity.** Hazardous tiles (LavaChannel, Corrupted, Void) must appear in equal quantity on both halves of the map, equidistant from both spawn zones, OR be placed only in the central zone (accessible by both players on equal terms).

5. **Clean spawn zones.** No ElementState pre-applied to any spawn tile. No tile within 2 tiles of a spawn may have LavaChannel or Void adjacency.

### Tile Composition Limits (Standard 10×10)

| Category | Minimum | Maximum | Rationale |
|---|---|---|---|
| Passable tiles | 60 | — | Ensures adequate movement space for 6+ units. |
| Impassable tiles (Wall/Rubble/Void) | — | 20 | Prevents degenerate chokepoint maps. |
| LavaChannel tiles | 0 | 8 | Caps fire-spread snowball potential. |
| Corrupted tiles | 0 | 6 | Limits passive poison pressure. |
| Forest tiles | 0 | 15 | Prevents total LOS denial. |
| Spawn tiles per player | 4 | 10 | Sufficient deployment choice without overly safe starts. |

For Large (12×12) maps, multiply thresholds by 1.44 (tile count ratio).

### Playtesting Validation Checklist

Before a map enters the competitive pool, it must pass:

- [ ] Both players can reach the central contest zone by turn 2 with a 3-Mancer, 7-movement roster
- [ ] No single Mancer archetype achieves >65% win rate on the map across 20+ test games
- [ ] At least two distinct winning strategies are viable (e.g., rush through center vs. flank route)
- [ ] Destruction of all pre-placed Wall tiles does not create impassable dead zones
- [ ] The map renders correctly in HD-2D with no tile Z-fighting or floating elevation artifacts
- [ ] All 4+ spawn tiles per player pass `MapLoader.ValidateLayout` without errors

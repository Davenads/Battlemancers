# Geomancer — Full Design Document

---

## 1. Tactical Identity

The Geomancer is the battlefield architect — the only Mancer that permanently reshapes the terrain itself. While every other Mancer applies temporary states that expire or get overwritten, the Geomancer raises walls, drops cover, changes elevation, creates pits, and converts fire into stone. Its most powerful outputs are not damage numbers but geometry: a wall placed in the right corridor at the right moment can redirect an entire battle, force enemy units into kill zones, and create elevated positions that transform every other Mancer's range and sight lines. The Geomancer wins matches not by dealing the most damage but by structurally refusing to let the opponent play the board they planned to play.

Playing the Geomancer well requires thinking about the board five turns ahead. Walls placed now shape movement in future turns. Elevation raised now creates line-of-sight advantages that pay dividends across multiple activations. Cover blocks created now deny safe positions for the opponent's ranged screen. This is not a Mancer that reacts to the board — it defines what the board looks like. Its primary weakness is output immediacy: a Geomancer that spends its first two activations building walls has dealt zero damage, and an opponent who closes the gap quickly before walls are established can prevent the Geomancer from doing anything useful. Managing the timing tension between building structure and applying damage is the core skill challenge.

**Primary win condition:** The Geomancer wins by constructing a board state where its team's movement paths are unobstructed while the opponent's movement is funneled through narrow lanes covered by allied Mancer spell ranges. Two or three well-placed walls, a raised elevation point for a Pyromancer or Electromancer, and a pit trap near a displacement-capable ally creates a board where the opponent cannot reposition without taking significant punishment. The Geomancer's team wins when the map looks like the Geomancer built it.

**Core weakness:** The Geomancer's construction phase is expensive in AP and, critically, in turns. Raise Terrain (5 AP) costs almost a full activation — a turn that the opponent can use to advance, position, or apply damage. A Geomancer facing an aggressive melee rush that closes before walls are established is a Geomancer that has burned AP on permanent-but-useless wall placement. Its direct damage output is moderate, not exceptional. Against a warband focused on eliminating the Geomancer quickly before its terrain work accumulates, the Geomancer is dangerously reactive. It must be kept at range or behind a chaff screen during its construction turns.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 100 | Average durability; expected to be in a protected position, not front-line |
| **Move Range** | 3 tiles per activation | Modest — Geomancer shapes the terrain rather than navigating it |
| **Base Armor** | 2 | Slightly above average; a partial concession to its slow repositioning |
| **Spell Range** | 5 tiles (base) | Medium range; terrain creation spells require moderate proximity to target zone |
| **AP per Activation** | 6 | Standard; movement costs 1 AP per tile |
| **Element** | Earth | All base spells deal Earth/Physical damage and apply earth-element terrain/status interactions |

**AP budget example:** With 6 AP, the Geomancer can move 1 tile (1 AP) and cast Raise Terrain (5 AP) — its heavy construction spell uses nearly the full activation. Or it can move 2 tiles and cast Rock Throw twice (2 + 2 + 2 AP), applying MUD terrain and CHILLED-equivalent slow effects while repositioning.

---

## 3. Base Spell Kit

The Geomancer's four base spells cover distinct functions:
- **Rock Throw** — repeatable physical damage and terrain disruption
- **Stone Wall** — permanent terrain feature creation; blocks movement and LoS
- **Raise Terrain** — elevation manipulation; reshapes ground geometry
- **Earthen Smash** — AoE terrain conversion and unit disruption

---

### Spell 1: Rock Throw

| Field | Value |
|---|---|
| **Name** | Rock Throw |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (projectile — travels in a line; can be blocked by walls and terrain features) |
| **Range** | 5 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 25 |
| **Element** | Earth / Physical |
| **Effects Applied** | Deals 25 physical damage. Target tile state changes based on what it hits (see terrain table). If the target is standing on or adjacent to loose terrain (GROUND, MUD, or rubble), the impact kicks up debris: target receives `SLOWED` (–1 move, 1 turn). If target has `BRITTLE_ARMOR` status (Cryomancer-applied): the physical hit triggers BRITTLE_ARMOR for +50% damage (25 × 1.5 = 37 HP total). If target is `FROZEN`: SHATTER triggers (25 × 2.5 = 62 HP; FROZEN removed). |
| **Temperature Effects** | **0 temperature change** (earth is thermally inert). However: if target is FROZEN SOLID (≤ -61), Rock Throw triggers SHATTER (×2.5 bonus damage). If target is SUPERCOOLED (-60 to -31), Rock Throw triggers BRITTLE modifier (×1.5 bonus damage). Rock Throw is the premier physical spell for exploiting cold-temperature thresholds without needing a separate physical attacker. |
| **Special Interactions** | Against `ON_FIRE` terrain: Rock Throw smothers fire on the target tile — tile becomes `OBSIDIAN` (permanent, impassable). This is the game's primary Obsidian creation mechanic, used deliberately in the Pyromancer + Geomancer Obsidian Trap combo. Against `ICE_TILE`: physical impact shatters the ice surface — tile becomes `WET` (ice fragments melt from impact heat); unit on the tile takes 8 additional cold-shard damage. Against `FLOODED`: Rock impacts and displaces water — tile becomes `MUD` (water + earth impact = wet earth); MUD imposes movement cost +2. |

**Design note:** Rock Throw is the Geomancer's primary damage spell and its combo trigger. At 2 AP with no cooldown, it can be cast three times in a single activation. Its 25 base damage is the highest single-hit Quick spell in the roster, reflecting the Geomancer's role as the premier physical damage dealer for SHATTER combo execution. The smother interaction (Rock Throw on ON_FIRE = Obsidian) is one of the most consequential terrain transformations in the game — it converts Pyromancer's temporary terrain into permanent structures. Combined with Backdraft (Pyromancer passive), each rock-throw smother triggers a fire explosion. The FROZEN SHATTER (62 HP from a 2-AP spell) is the most AP-efficient kill-confirmation in the game when paired with a Cryomancer freeze.

**Spell answers YES to (design rule check):**
1. Applies terrain state (OBSIDIAN from ON_FIRE, MUD from FLOODED, WET from ICE_TILE) — YES
2. Applies unit status (SLOWED, triggers BRITTLE_ARMOR, triggers SHATTER) — YES
3. Synergizes with Cryomancer (SHATTER), Pyromancer (Obsidian smother), Hydromancer (MUD creation) — YES
4. Skill expression: target selection for SHATTER; smother placement for Obsidian corridor creation — YES

---

### Spell 2: Stone Wall

| Field | Value |
|---|---|
| **Name** | Stone Wall |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Terrain Placement — targets a specific tile or a line of up to 3 contiguous tiles |
| **Range** | 4 tiles (to nearest wall tile) |
| **AoE Radius** | N/A (line placement; 1-tile wide, up to 3-tile long) |
| **Base Damage** | 0 (terrain feature placement; no direct damage) |
| **Element** | Earth |
| **Effects Applied** | Places a `STONE_WALL` terrain feature on target tiles. STONE_WALL properties: completely blocks movement (impassable); blocks line of sight for all spells requiring LOS; absorbs 80 HP of damage before being destroyed (structural HP pool); persists indefinitely until destroyed. A 3-tile wall costs 3 AP and creates 3 contiguous impassable LoS-blocking tiles simultaneously. |
| **Temperature Effects** | **0 temperature change** (stone is thermally inert). However: walls can block line-of-sight to prevent temperature-modifying spells from reaching their targets — a Stone Wall placed between an enemy Pyromancer and an ally protects the ally from fire spells that would otherwise apply BURNING and heat temperature. Tactical LoS denial indirectly prevents temperature manipulation. |
| **Special Interactions** | Placement restriction: Stone Wall cannot be placed on a tile already occupied by a unit. If a unit stands adjacent to a placed wall, the wall is valid and the unit is simply now adjacent to solid stone. Against `ON_FIRE` tiles targeted for wall placement: the Geomancer cannot place walls directly on fire (molten ground is unstable) — the fire must be smothered first (Rock Throw → Obsidian) before a wall can be placed on an adjacent tile. Against incoming Pyromancer Ember Shot aimed past the wall: the projectile is blocked by the wall — the unit behind it does not take damage. The wall absorbs the hit and its structural HP drops (18 HP absorbed; wall has 80 HP total). Walls on `CHARGED` tiles: the placement process discharges the tile — arc fires from the Charged tile before the wall completes, potentially hitting the Geomancer itself. |

**Design note:** Stone Wall is the Geomancer's signature terrain construction ability. A 3-tile wall placed in a corridor creates a permanent blocking structure that neither side can pass through without destroying it, and destroying it costs other Mancers significant AP (explosive spells, Pillar of Flame, etc.). The strategic implications of Stone Wall placement are extensive:

- **Line-of-sight blocking:** Any spell requiring LoS cannot target through a Stone Wall. A wall placed between an enemy Pyromancer and the Geomancer's allied formation prevents Ember Shot, Fireball, and Pillar of Flame from reaching those targets. Ranged suppression becomes melee pressure.
- **Movement forcing:** A 3-tile wall across a corridor forces all movement to go around it, adding 2–5 AP of movement cost to any unit trying to reposition through the formerly open space. This creates predictable movement paths that the Geomancer (and allies) can aim spells at.
- **Cover creation:** Units positioned directly behind a Stone Wall gain soft cover from any attack that cannot go around or over it. The Geomancer can create a defensive position for a low-HP ally (Pyromancer with 85 HP) by placing a wall ahead of them.

**Elevation interaction:** Stone Walls placed adjacent to ELEVATED tiles function as blocking terrain for units on the elevated position — a unit on high ground behind a Stone Wall has cover from below AND elevated spell-range bonuses. This is the primary defensive position the Geomancer constructs for Mancer allies.

**Spell answers YES to (design rule check):**
1. Creates a permanent terrain feature (STONE_WALL) — YES
2. Blocks movement and LoS (repositioning a key map feature) — YES
3. Synergizes with all Mancers (LoS manipulation benefits any ranged Mancer) — YES
4. Skill expression: wall placement geometry relative to current and future movement paths — YES

---

### Spell 3: Raise Terrain

| Field | Value |
|---|---|
| **Name** | Raise Terrain |
| **AP Cost** | 5 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Ground Target — targets a specific tile or a 2×2 tile cluster |
| **Range** | 3 tiles (to nearest affected tile; short range — requires proximity) |
| **AoE Radius** | Up to 2×2 tile area (Geomancer chooses: single tile or 2×2 cluster) |
| **Base Damage** | 12 (units on targeted tiles are displaced upward or take impact damage from rising ground) |
| **Element** | Earth |
| **Effects Applied** | Targeted tiles become `ELEVATED` (raised 1 level; elevation persists permanently). Units on the raised tiles are displaced upward with the terrain — they are now on ELEVATED ground, gaining the standard Elevated tile bonus (+1 range to all spells). If a unit is displaced upward against a blocking structure (wall, obstacle), they are instead pushed to an adjacent tile at the base elevation and take 12 impact damage. If the Geomancer raises terrain directly beneath an enemy unit without warning (the tile was not previously elevated), the enemy unit takes 12 impact damage and is displaced: if there is open space adjacent at the new elevation, the unit is placed there; if not, the unit is ejected to the nearest passable tile at base elevation (fall damage if ejection distance is 2+ tiles). |
| **Temperature Effects** | **0 temperature change** (raised earth is thermally neutral). Elevated tiles created by Raise Terrain are NOT BURNING or FROZEN by default — the Geomancer creates thermally neutral high ground even in temperature-affected areas. An enemy standing on BURNING terrain can be lifted away from it by raising their tile, ending the +10/turn temperature gain from ground contact. |
| **Special Interactions** | Elevation effects on gameplay: ELEVATED tiles grant +1 range to all spells cast from them. ELEVATED tiles grant +1 movement cost to enter (must spend 1 additional AP to climb). ELEVATED tiles increase fall damage when units are pushed off them: fall_distance × 8 HP. Creating ELEVATED terrain near a displacement Mancer (Aeromancer, Hydromancer, Gravimancer) creates combined hazard: Aeromancer pushes enemy onto elevated tile → Geomancer raises adjacent tile higher (now 2 levels up) → next push off is 2-level fall damage. Against `FLOODED` tile targeted for raise: the water drains as the ground rises — FLOODED tile becomes `ELEVATED GROUND` (elevated, dry, normal terrain). Against a `STONE_WALL` adjacent to the raised tile: the wall remains; the elevation change may make the wall shorter relative to the new ground level (tactical note for LoS blocking recalculation). |

**Design note:** Raise Terrain is the Geomancer's most strategically powerful spell and its highest AP cost. Creating an ELEVATED tile permanently alters the map's geometry in multiple ways: it creates a vantage point for allied Mancers to gain +1 spell range (a Pyromancer on a Geomancer-raised platform reaches Ember Shot at 7 tiles), it creates a fall-damage hazard for displacement combos, and it increases the AP cost for enemies to traverse the area. Three Raise Terrain casts across a match can create a staircase platform structure that dominates a map quadrant — every other Mancer benefits from the height advantage.

**Cover creation with elevation:** An ELEVATED tile with a Stone Wall adjacent to its edge creates a fortified position: the unit on elevation has +1 spell range AND is behind cover for attacks coming from below the wall. This is the Geomancer's "fortress" configuration — a well-protected fire platform that the Pyromancer or Electromancer can use as a long-range base.

**LoS blocking by elevation:** Raising terrain to a 2-tile-high block next to a critical area blocks all LoS that passes through or over the raised area for units at base elevation. A raised terrain block in the center of the map can deny LoS across an entire engagement zone — forcing all combat into the flanks.

**Spell answers YES to (design rule check):**
1. Creates a permanent terrain feature (ELEVATED tile) — YES
2. Moves units (displaces them upward; ejects off the raise) — YES
3. Creates new tactical possibilities (fall hazard, range advantage, cover creation) — YES
4. Synergizes with Aeromancer, Hydromancer (displacement + fall damage), all Mancers (range bonus) — YES
5. Skill expression: elevation placement relative to displacement Mancer positions; map quadrant control — YES

---

### Spell 4: Earthen Smash

| Field | Value |
|---|---|
| **Name** | Earthen Smash |
| **AP Cost** | 4 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Ground Target — AoE Radial |
| **Range** | 4 tiles (to center of AoE) |
| **AoE Radius** | 2 tiles |
| **Base Damage** | 20 (to all units in AoE) |
| **Element** | Earth / Physical |
| **Effects Applied** | Deals 20 physical damage to all units in 2-tile radius. Units hit are knocked back 1 tile away from the center (displacement — pushed outward from impact point). All tiles in the AoE become `MUD` (movement cost +2; persists 3 turns). If units are knocked back into a wall, obstacle, or another unit: collision damage = 8 HP additionally. |
| **Temperature Effects** | **0 temperature change** (earth is thermally inert). However: if any unit in the AoE is SUPERCOOLED or FROZEN SOLID, Earthen Smash is the premium damage spell to use — BRITTLE modifier (×1.5) applies to SUPERCOOLED targets and SHATTER (×2.5) applies to FROZEN SOLID targets, making Earthen Smash (20 physical base × 2.5 = 50 HP) the highest AoE physical burst available to the Geomancer when the Cryomancer has done its work. |
| **Special Interactions** | Against `ON_FIRE` tiles in AoE: each fire tile is smothered by the earth impact — becomes `OBSIDIAN` (permanent; all fire in the AoE smothered simultaneously). This is a mass-smother: Geomancer can convert an entire Pyromancer fire zone to Obsidian in one Earthen Smash, permanently reshaping the area. Against `ICE_TILE` in AoE: physical impact shatters ice surfaces — all ICE_TILE in the AoE become `WET`; units on those tiles take additional 8 cold-shard damage. Against `FLOODED` in AoE: earth displaced into water creates `MUD` across the entire flooded area (displaces water). Against `ELEVATED` tiles adjacent to center: the smash can destabilize a 1-tile elevated position — roll stability check; on failure the elevated tile collapses to ground level, any unit on it takes 12 fall impact damage. Against `FROZEN` units hit by the smash: SHATTER triggers (20 × 2.5 = 50 HP; FROZEN removed). |

**Design note:** Earthen Smash is the Geomancer's versatile mid-cost tool. At 4 AP it is a Heavy spell — significant resource but not a full-turn commitment. Its value is in combining: knockback disrupts formations, MUD creation slows the entire disrupted group, and the mass-smother against ON_FIRE is the fastest Obsidian-creation play the Geomancer has access to (converting a large fire zone at once rather than one Rock Throw at a time). The physical knockback is also the Geomancer's primary displacement ability — pushing units 1 tile outward from center, combined with fall-hazard placement, creates fall damage without requiring Aeromancer partnership.

**Spell answers YES to (design rule check):**
1. Applies terrain state (MUD, OBSIDIAN from fire) — YES
2. Moves units (knockback 1 tile from center) — YES
3. Synergizes with Pyromancer (fire → Obsidian), Cryomancer (SHATTER on FROZEN), Hydromancer (FLOODED → MUD) — YES
4. Skill expression: center point selection for knockback direction; pre-positioned fall hazards for knockback destination — YES

---

## 4. Terrain Interaction Table

### Earth Spell Impact on Existing Terrain States

The following describes what happens when any Geomancer spell strikes a tile in the listed terrain state. All Geomancer base spells deal Earth/Physical damage; these interactions apply universally unless a spell's individual entry overrides them.

| Existing Terrain State | What Happens When Earth Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **Normal (GROUND)** | Earth impact — standard physical damage | `GROUND` (churned/disrupted but functional) | Takes spell damage + `SLOWED` (debris kick) | Rock Throw additionally may apply MUD depending on water content of soil |
| **WET** | Wet earth is churned into mud | `MUD` (movement cost +2; persists 3 turns) | Takes spell damage; no additional status | MUD zone created — synergizes with Hydromancer Aqua Lance to re-wet MUD back to WET for conductivity |
| **FLOODED** | Earth displaced into water | `MUD` (water absorbed by displaced earth) | Takes spell damage + SLOWED (wet mud displacement) | Large FLOODED areas become MUD zones; removes FLOODED connectivity for Electromancer chains |
| **ON_FIRE** | Earth smothers the fire | `OBSIDIAN` (permanent, impassable; fire extinguished permanently) | Takes spell damage; BURNING extinguished | Obsidian cannot be destroyed by normal means; permanently reshapes the map. Pyromancer Backdraft passive triggers on each smother if active |
| **ICE_TILE** | Physical impact shatters ice surface | `WET` (ice fragments melt) | Takes spell damage + 8 cold-shard damage | FROZEN units on ICE_TILE shattered by earth spell take the full SHATTER bonus (×2.5) |
| **TOXIC_TERRAIN** | Earth buries the poison | `GROUND` (toxic material buried beneath surface; poison neutralized) | Takes spell damage + SLOWED | TOXIC_TERRAIN cleared — Geomancer can remove enemy Toximancer terrain investment |
| **CHARGED** | Earth is not conductive — safely grounds the charge | `GROUND` (discharge absorbed; charge dissipated harmlessly) | Takes spell damage; no arc chain | Earth spell is the only elemental spell that safely discharges CHARGED terrain without triggering an arc — useful when the Geomancer needs to clear a CHARGED tile without risk |
| **MUD** | Earth compacts mud | `GROUND` (mud compressed into solid earth; faster than waiting for expiry) | Takes spell damage | Removes movement penalty — Geomancer can clean up its own MUD zones after they have served their purpose |
| **OVERGROWTH** | Earth crushes the organic growth | `GROUND` (growth flattened; Floramancer barriers destroyed) | Takes spell damage + ROOTED removed (if unit was rooted by growth) | Counter to Floramancer barriers; Geomancer can clear vine walls and root zones with earth spells |
| **OBSIDIAN** | Earth on obsidian — no state change | `OBSIDIAN` (unchanged; obsidian is too hard for normal earth pressure) | Takes spell damage | Obsidian is indestructible by standard earth spells; only extreme burst damage (e.g., Pillar of Flame) can chip it |
| **PERMAFROST** | Heavy earth pressure cracks frozen mud | `MUD` (thawed by pressure; permafrost broken into wet mud) | Takes spell damage + CHILLED removed | Geomancer can reverse Cryomancer PERMAFROST tiles by earth compression |

### Permanent Terrain Features Created by Geomancer

The Geomancer is unique in creating terrain features that do not expire:

| Feature | How Created | Properties | Removal |
|---|---|---|---|
| `STONE_WALL` | Stone Wall spell | Impassable; blocks LoS; 80 HP structural pool | Destroyed by 80+ HP of explosive damage |
| `OBSIDIAN` | Rock Throw / Earthen Smash on ON_FIRE tile | Impassable; LoS-blocking; indestructible by standard means | Only Pillar of Flame or equivalent extreme burst |
| `ELEVATED` | Raise Terrain | +1 spell range to caster; +1 move cost to enter; fall damage multiplier | Cannot be lowered once raised (permanent) |
| `PIT` | Pillar of Flame creates PIT; Geomancer can create pits at 5+ AP heavy spells (design future scope) | Impassable from base; units pushed into pit take fall damage; traps melee units | Cannot be filled without heavy construction AP |

### Terrain States Beneficial to the Geomancer

| State | Benefit |
|---|---|
| `ON_FIRE` tiles (Pyromancer-created) | Rock Throw on any ON_FIRE tile = OBSIDIAN (permanent map restructuring at 2 AP) |
| `ELEVATED` tiles (self-created) | Geomancer on its own elevated platform gains +1 range to all spells, extending Rock Throw to 6 tiles |
| `FLOODED` / `WET` tiles | Geomancer earth spells convert these to MUD — relevant for disrupting Hydromancer-Electromancer chain setups |
| `ICE_TILE` | Rock Throw shatters ice, deals bonus cold-shard damage, applies SHATTER on FROZEN units — Geomancer is the most efficient shatter partner for Cryomancer |

### Terrain States Hazardous to the Geomancer

| State | Hazard |
|---|---|
| `ON_FIRE` tiles the Geomancer must cross | Geomancer has no fire immunity; 5 HP/turn DoT compounds its moderate HP pool |
| `ICE_TILE` slip hazards | Geomancer has no cold immunity; slip checks apply to it normally — can be slid into its own constructed pits |
| `MUD` (created by Geomancer) | Geomancer is not immune to its own MUD terrain; movement cost +2 applies if the Geomancer must cross MUD zones it created |
| `CHARGED` tiles | No defensive advantage; Geomancer takes normal arc damage |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

#### Variant A: Boulder Hurl (replaces Rock Throw) — +20 pts

**Description:** Rock Throw is replaced by Boulder Hurl — a slower, larger projectile. Boulder Hurl deals 38 physical damage (up from 25) and has a 1-tile splash radius (units adjacent to the impact tile take 15 splash damage and receive `SLOWED`). Impact tile becomes `MUD` regardless of prior state (except OBSIDIAN, which is unchanged). AP cost is 3 AP; cooldown is 1 turn. If the impact tile is `ON_FIRE`, the smother creates OBSIDIAN but the splash radius also smothers — up to 5 tiles of ON_FIRE converted to OBSIDIAN in one Boulder Hurl.

**Trade-off:** Higher burst damage, AoE smother, splash radius — at the cost of Rock Throw's no-cooldown repeatable nature. Best for Geomancers focused on mass Obsidian creation and burst physical damage rather than rapid-fire SHATTER combos.

#### Variant B: Rampart (replaces Stone Wall) — +20 pts

**Description:** Stone Wall is replaced by Rampart — an L-shaped or T-shaped wall configuration that covers a 3-tile L-corner or T-intersection rather than a straight line. Rampart creates 3 wall segments in a non-linear arrangement, allowing corner walls and enclosed pocket positions that straight-line walls cannot create. AP cost is 4 AP; cooldown is 2 turns. Structural HP is 120 HP per segment (sturdier than the standard 80 HP Stone Wall). Cannot create Obsidian with Rampart directly.

**Trade-off:** More flexible wall geometry (corners block flanking paths that straight walls cannot) at higher AP cost. Best for Geomancers focused on creating enclosed defensive positions for ally Mancers rather than straight-corridor denial.

#### Variant C: Terrain Shatter (replaces Earthen Smash) — +25 pts

**Description:** Earthen Smash is replaced by Terrain Shatter — a targeted ground detonation that destroys existing terrain features as its primary function. Terrain Shatter deals 28 physical damage to all units in a 2-tile radius, applies knockback (2 tiles instead of 1), and destroys any terrain feature in the AoE (Stone Walls, Obsidian blocks, elevated terrain, Floramancer vine barriers, all permanent features). Destroyed Stone Walls collapse — units adjacent take 15 collapse damage; the tile becomes RUBBLE (impassable for 2 turns, then clears to GROUND). AP cost is 5 AP; cooldown is 3 turns.

**Trade-off:** Grants the Geomancer the ability to remove structures it created (or enemy-created structures) — useful in long engagements where the tactical need changes. Also enables dismantling enemy Floramancer barriers and Obsidian that the Geomancer itself created incorrectly. The 5 AP cost is high; this is not a casual investment.

---

### Passive Traits

#### Passive A: Stonehide — +20 pts

**Description:** The Geomancer's armor increases from 2 to 4 (total effective armor). Additionally, while standing on any ELEVATED tile (including ones it created itself), the Geomancer gains an additional +1 armor (total 5) and reduces knockback effects by 1 tile (1-tile push becomes 0; 2-tile push becomes 1). Stonehide does not protect against spell damage, only physical damage reduction through armor and displacement resistance.

**Trade-off:** Significant durability improvement — especially on self-constructed elevated positions where 5 armor makes the Geomancer nearly immune to Chaff-tier physical attacks. Best for Geomancers built as a forward anchor rather than a rear architect.

#### Passive B: Earth Mastery — +25 pts

**Description:** All Geomancer earth-element spells have their range increased by 1 tile. Additionally, ON_FIRE tiles within 3 tiles of the Geomancer are subject to passive smother: at the end of each Geomancer activation, 1 ON_FIRE tile within 3 tiles is automatically converted to OBSIDIAN (no AP cost; passive; Geomancer chooses which tile). This passive smother does not trigger Pyromancer Backdraft (it is a gentle conversion, not an explosive extinguish). The Geomancer effectively "eats" fire zones slowly from proximity.

**Trade-off:** Constant incremental Obsidian creation at zero AP cost. Against a Pyromancer-heavy opponent, Earth Mastery denies the Pyromancer's terrain investment by passively converting fire to stone each turn the Geomancer is near. The range bonus compounds with the elevated-tile bonus for extended reach. Best for long-game Geomancer builds focused on permanent terrain control.

#### Passive C: Seismic Sense — +15 pts

**Description:** The Geomancer can detect units on ELEVATED tiles (even if LoS from below would normally be blocked by the elevation itself). Additionally, units on ICE_TILE adjacent to the Geomancer's constructed ELEVATED or STONE_WALL terrain make slip checks with a more severe outcome: if they slip, they are pushed 2 tiles instead of 1 (the terrain geometry amplifies the slide). The Geomancer has LoS to all units within 3 tiles regardless of terrain blocking (tremor sense — it feels movement through the ground).

**Trade-off:** Niche information and control amplification. Most useful in maps with complex multi-level terrain where the Geomancer's constructed elevation creates blind spots. The enhanced slip effect pairs with Cryomancer ICE_TILE placement for devastating involuntary displacement.

#### Passive D: Obsidian Skin — +20 pts

**Description:** Whenever the Geomancer creates an OBSIDIAN tile (via Rock Throw smother or Earthen Smash smother), it gains a 10-HP shield absorbing the next damage instance it receives. Shields do not stack — only 1 shield is active at a time — but each new Obsidian creation refreshes the shield. A Geomancer actively creating Obsidian during combat is continuously generating damage-absorbing shield charges.

**Trade-off:** Incentivizes active Obsidian creation rather than passive architecture. Best for Geomancers in close-range fights where both Pyromancer fire zones and Obsidian creation are happening simultaneously.

---

### Stat Enhancements

#### Stat A: Earthen Constitution (+20 HP) — +10 pts

**Description:** Max HP increases from 100 to 120. Brings the Geomancer firmly into the durable Mancer tier, where it can absorb more burst before requiring defensive positioning. Most relevant in scenarios where the Geomancer is used as a forward anchor (Stonehide builds) rather than a rear architect.

#### Stat B: Stone Stride (+1 Move Range) — +15 pts

**Description:** Move Range increases from 3 to 4 tiles per activation. The Geomancer can access construction positions and SHATTER targets more reliably within its 6 AP budget. Most useful for Geomancers that need to both construct terrain and apply physical pressure in the same activation — reaching a Rock Throw position with 1 AP more of movement freedom.

---

### Signature Ability

The Signature Ability is unlocked as an additional spell slot — it does not replace any base spell.

#### Signature: Titan's Ascent — +40 pts

| Field | Value |
|---|---|
| **Name** | Titan's Ascent |
| **AP Cost** | 6 AP (entire activation; Geomancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Self-centered — the Geomancer is the origin point |
| **Range** | 5 tiles (effect radius from the Geomancer's current position) |
| **AoE Radius** | 5 tiles |
| **Base Damage** | 30 (all units within 5 tiles; tremor/seismic burst) |
| **Element** | Earth / Physical |
| **Effects Applied** | A massive seismic surge radiates outward from the Geomancer. All GROUND tiles within 5 tiles become `ELEVATED` by 1 level simultaneously (mass elevation — the Geomancer raises the ground around itself). All units within 5 tiles take 30 physical damage and are knocked back 2 tiles away from the Geomancer (outward displacement). All `ON_FIRE` tiles within 5 tiles are smothered and converted to `OBSIDIAN` (mass smother). All `FLOODED` tiles within 5 tiles become `MUD` (earth surge absorbs water). The Geomancer's own tile is raised to a 2-level elevation (double elevated) — it gains +2 spell range from this position. |
| **Special Interactions** | Against FROZEN units hit by the seismic burst: SHATTER on all FROZEN units in the 5-tile radius simultaneously (30 × 2.5 = 75 HP on every FROZEN unit — devastating AoE shatter). Against units knocked back off existing elevated terrain: fall damage applies from the knockback distance (each tile past an edge = fall_distance × 8 HP). Against `STONE_WALL` segments within the radius: walls are reinforced by the seismic uplift, gaining +40 HP to their structural pool. The Geomancer effectively fortifies its own walls while raising the surrounding terrain. |

**Design note:** Titan's Ascent is the Geomancer's map-redefining ability — it simultaneously creates a fortified elevated command position for the Geomancer, mass-smothers enemy fire zones, mass-converts flooding to mud, disperses surrounding enemy formations with 2-tile knockback, and deals 30 physical burst to the 5-tile radius. When executed after 2–3 turns of terrain building (fire zones established by Pyromancer, MUD set up by prior Earth spells, FROZEN units staged by Cryomancer), Titan's Ascent converts all of those temporary states into permanent Obsidian features simultaneously while also dealing AoE burst damage and confirming SHATTER kills on every FROZEN target in range.

This is the ability that justifies the "architect" identity fully — the Geomancer does not just add a wall here or there; it rewrites the entire local terrain topology in a single activation.

**Synergy note:** Titan's Ascent paired with a Cryomancer Glacier's Wrath on the prior turn is the game's most destructive sequential combo: Glacier's Wrath mass-freezes the zone; Titan's Ascent seismic-bursts every FROZEN unit in the radius for ×2.5 (75 HP each). Three or more simultaneously shattered Mancers in a single Geomancer activation is achievable and match-deciding.

---

## 6. Faction Synergy

### Best Faction: The Verdant Pact

The Verdant Pact is the Geomancer's natural home, primarily through the Terrain Bond mechanic. The Geomancer is the one Mancer that consistently creates ELEVATED terrain and MUD terrain — both of which interact with the Verdant Pact's passive.

**Terrain Bond with Geomancer:**
- **MUD tiles (earth-origin):** MUD created by the Geomancer counts as natural terrain for Terrain Bond purposes. Verdant Pact Thornback Sentinels and Rootwardens standing on MUD gain Terrain Bond movement bonus (+1 move) and passive regen. A Geomancer creating MUD with Earthen Smash or Rock Throw on WET tiles simultaneously creates regen terrain for allied Pact chaff.
- **ELEVATED tiles:** ELEVATED terrain is natural earth — Terrain Bond triggers on elevated tiles created by the Geomancer. Pact units on a Geomancer-raised elevated position gain both the elevation spell-range bonus AND the Terrain Bond movement bonus and regen. The combination of elevated advantage and passive healing makes Pact units on Geomancer platforms substantially harder to dislodge than they would be on equivalent terrain in other factions.

**Rootwardens synergy:** Rootwardens (T2 Chaff) can entrench — becoming immovable and generating a natural tile beneath themselves. Combined with the Geomancer raising terrain to the Rootwarden's position, an entrenched Rootwarden on a Geomancer elevated tile is an extremely durable position: immune to displacement, regen from Terrain Bond, elevated spell-range bonus on any Pact Mancer nearby, and on natural terrain for continuous Terrain Bond regen.

**Glade Archer synergy:** Glade Archers can fire from dense cover without accuracy penalty. A Stone Wall with Glade Archers behind it combines OVERGROWTH adjacency (if available) with stone cover — Archers behind Geomancer walls fire through adjacent OVERGROWTH cover without penalty while the wall blocks enemy LoS to them.

### The Gilded Throne — SHATTER Artillery

The Gilded Throne's Siege Arbalest (T2 Ranged) is the Geomancer's most efficient SHATTER partner in a non-Mancer context. Siege Arbalest fires every turn (no reload), has armor-piercing bolts, and can brace for +1 range. A Geomancer applying BRITTLE_ARMOR or staging a Cryomancer FROZEN target next to a Siege Arbalest position completes SHATTER combos without requiring a third Mancer activation.

Iron Vanguard (T2 Chaff) in Shield Wall behind a Geomancer Stone Wall creates a durable front line: the wall blocks enemy ranged fire, the Vanguard holds the chokepoint with Shield Wall damage reduction, and the Geomancer can raise terrain on the Vanguard's side of the wall to give friendly units height advantage for approach.

### The Ashen Covenant — Obsidian and Deathless Advances

The Ashen Covenant's primary Geomancer interaction is with OBSIDIAN terrain. Grave Husks advance through any terrain (BURNING, POISONED, TOXIC) that would deter other chaff. Obsidian is one terrain they cannot pass through — but in the Geomancer's tactical framework, Obsidian is placed to block enemies, not allies. A Geomancer that smothers fire (Pyromancer fire zones) into Obsidian creates impassable barriers that funnel enemy movement into predictable lanes. Grave Husks advance through adjacent ON_FIRE terrain (healing from it) while the Obsidian walls funnel enemies into the Husks' approach path.

Wailing Shades (phase-through ranged) pass through physical cover — Stone Walls do not stop their projectiles. This makes the Geomancer + Ashen Covenant combination particularly asymmetric: Geomancer walls block enemy ranged units' LoS while allied Wailing Shades fire through those same walls without penalty. The opponent cannot see through the walls; the Shades can fire through them.

---

## 7. Combo Chains

### Combo 1 — The Obsidian Trap (Geomancer + Pyromancer) [SIGNATURE]

**Mancers involved:** Geomancer + Pyromancer

**Step-by-step execution:**

1. **Turns N and N+1, Pyromancer activates:** Pyromancer uses Scorched Earth and Conflagration Wave to establish a cluster of ON_FIRE tiles across a corridor or central engagement zone. Target: 4–6 adjacent ON_FIRE tiles in a tactically relevant location.
2. **Turn N+1 or N+2, Geomancer activates:** Geomancer uses Rock Throw (2 AP) on each ON_FIRE tile it wants to convert. Each Rock Throw on an ON_FIRE tile = OBSIDIAN (permanent, impassable). Geomancer can Rock Throw 3 times in one activation (6 AP; no movement) — converting 3 fire tiles to Obsidian in a single turn. Alternatively, Earthen Smash (4 AP) on an ON_FIRE cluster converts up to 5 tiles in the AoE simultaneously.
3. **Tactical result:** Permanent Obsidian walls now exist where fire once burned. The former fire corridor is now a permanent obstacle field. Neither side can pass through Obsidian normally — but the Geomancer planned this placement to funnel enemies into alternate paths where additional Pyromancer fire zones are active.

**Without Backdraft passive:**
Pyromancer fire → Geomancer smothers → Obsidian barriers created. Permanent map restructuring.

**With Pyromancer Backdraft passive (+25 pts):**
Each tile the Geomancer smothers triggers a 15-fire-damage explosion in 1-tile radius. Three Rock Throws in one activation = three 15-damage AoE bursts automatically. The construction turn deals 15+ HP to every unit adjacent to the smothered fire tiles passively.

**Why this is strong:** Obsidian is the only truly permanent terrain state in the game (other than Raise Terrain elevation). A Geomancer + Pyromancer team that converts a large fire zone to Obsidian has permanently added impassable obstacles to the map — obstacles that cannot be removed by normal play, that block LoS indefinitely, and that force the opponent's entire warband to route around them for the rest of the match. In a blind-turn game where movement planning depends on available paths, eliminating paths is one of the highest-value strategic plays.

---

### Combo 2 — Freeze-Shatter (Geomancer + Cryomancer)

**Mancers involved:** Geomancer + Cryomancer

**Step-by-step execution:**

1. **Cryomancer activates:** Applies FROZEN to priority target (via double Frost Bolt CHILLED→FROZEN, or Ice Lance direct FROZEN). Target is FROZEN (skip turn + SHATTER vulnerability ×2.5).
2. **Geomancer activates (same or following turn):** Rock Throw at the FROZEN unit. 25 base damage × 2.5 SHATTER = 62 HP.

**AP efficiency:** 2 AP for Rock Throw; 62 HP of confirmed damage. The most AP-efficient SHATTER delivery in the game (Geomancer beats all other physical attackers at the same AP cost due to Rock Throw's 0-cooldown and 25 base damage).

**Mass shatter variant (with Titan's Ascent or Blizzard Field setup):** Cryomancer uses Blizzard Field to mass-CHILL then FREEZE multiple units. Geomancer uses Earthen Smash (4 AP, 20 physical damage) in the center of the frozen cluster. All FROZEN units in the 2-tile radius take 20 × 2.5 = 50 HP SHATTER damage simultaneously from one spell.

---

### Combo 3 — The Mud Trap (Geomancer + Hydromancer)

**Mancers involved:** Geomancer + Hydromancer

**Setup:** Hydromancer casts any water spell onto GROUND tiles — tiles become WET (2-turn duration). Or Geomancer uses Earthen Smash on FLOODED tiles — tiles become MUD directly.
**Execution:** Geomancer casts Rock Throw or Earthen Smash on WET tiles — WET + earth spell = MUD (movement cost +2).
**Result:** A zone of MUD terrain forces all enemy movement through the area to cost +2 AP per tile — effectively halving their movement range through that zone.

**Follow-up:** Hydromancer can re-wet MUD tiles (MUD + water spell = WET) to reset the terrain for Electromancer chain arc conductivity. The cycle: WET → Geomancer creates MUD (denial) → Hydromancer re-wets (conductivity) → Electromancer chains (stun) → Geomancer next turn creates MUD again (denial resumed). A 3-Mancer cycling terrain state sequence that permanently threatens the zone with either movement denial or chain stuns.

---

### Combo 4 — The High Ground (Geomancer + Any Ranged Mancer)

**Mancers involved:** Geomancer + Pyromancer / Electromancer / Photomancer / any long-range Mancer

**Setup:** Geomancer uses Raise Terrain (5 AP) to create an ELEVATED tile in a position that covers the central engagement zone.
**Execution:** Allied Mancer moves onto the ELEVATED tile (1 AP to climb). From elevation: all spells gain +1 range. Pyromancer Ember Shot: 6 + 1 elevation = 7 tiles. Electromancer Arc Bolt: standard range + 1. Photomancer Illuminate: +1 vision range.
**Defensive layering:** Geomancer places a Stone Wall at the edge of the elevated platform facing the enemy. The elevated Mancer is now behind the wall (physical cover + LOS blocker from below) AND on high ground (increased range).

**Tactical impact:** The fortified elevation position is the single most powerful position the Geomancer can create for an ally Mancer. A Pyromancer on Geomancer elevation with a Stone Wall frontage can reach targets at 7+ tiles without being reachable by standard Pyromancer spells from below. A well-constructed elevated fortress effectively removes the Pyromancer's core weakness (low HP + short range) by extending its threat range while covering it from melee approach.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Geomancer

| Mancer | Counter Mechanism |
|---|---|
| **Aeromancer** | Aeromancer's wind displacement can push units off elevated terrain (fall damage cancels the Geomancer's elevation advantage). Additionally, wind can redirect Geomancer Rock Throw trajectories mid-flight in design (future scope). Most critically: Aeromancer's `UPDRAFT` zone applies WEIGHTLESS to units inside, making them immune to ground terrain effects — MUD movement penalty, ICE_TILE slip, and TOXIC_TERRAIN all bypassed. Geomancer terrain control is ground-dependent; WEIGHTLESS opponents ignore it. |
| **Sonimancer** | Sonimancer spells pass through walls. Stone Walls that block LoS for all other Mancers do not block Sonimancer sonic attacks — the Geomancer's primary defensive investment (wall cover) does not protect against sonic damage. A Sonimancer on the opponent team means Geomancer walls provide zero cover against that specific threat. |
| **Floramancer** | OVERGROWTH (Floramancer organic terrain) rapidly spreads on GROUND tiles and can entangle the Geomancer (ROOTED status). A Geomancer ROOTED cannot move to its construction positions. Floramancer vine barriers serve the same function as Stone Walls (LoS + movement blocking) at lower AP cost — if the opponent has a Floramancer, the Geomancer must compete with cheaper organic terrain construction that it cannot outpace in AP economy. |

### Warband Compositions That Prey on Geomancer

| Warband Type | Exploitation |
|---|---|
| **Aeromancer + aggressive melee rush** | Aeromancer provides WEIGHTLESS to the melee force (bypassing Geomancer terrain control), then displacement attacks push the Geomancer off its own elevated positions. The Geomancer's construction turns are worthless against terrain-immune units and its position is not stable once Aeromancer displacement starts targeting it. |
| **Sonimancer-focused list** | All Geomancer walls are transparent to sonic attacks. The Geomancer's construction provides zero cover benefit against sonic damage sources. Wall investment is partially wasted. |
| **Fast aggro (triple chaff + Aeromancer)** | The Geomancer needs 2–3 turns to establish useful terrain. A fast chaff rush can reach and pressure the Geomancer before any walls are up — and chaff flooding a 3-move Geomancer's position forces it to spend AP on Rock Throw defense instead of construction. |

---

## 9. Temperature Interaction Notes

Earth is thermally inert — no Geomancer spell directly alters temperature. This is not a limitation; it is a design identity. The Geomancer is the physical finisher whose damage is amplified by temperature thresholds set by other Mancers, and the terrain architect who indirectly controls temperature by shaping which tiles enemies can stand on.

### Geomancer as the Freeze-Shatter Finisher

The entire temperature system creates a setup → payoff loop where a Cryomancer drives enemy temperature down and the Geomancer delivers the massive physical finisher. The explicit combo sequence:

1. **Cryomancer casts Frost Bolt (-20 temperature):** Target at 0 → -20 (COLD).
2. **Cryomancer casts Ice Lance (-25 temperature):** Target at -20 → -45 (SUPERCOOLED — SLOWED + BRITTLE modifier active).
3. **Geomancer casts Earthen Smash (4 AP, 20 base physical):** SUPERCOOLED target = ×1.5 BRITTLE = 30 HP. Or if Cryomancer has driven the target to FROZEN SOLID (≤ -61): ×2.5 SHATTER = 50 HP from the 20 base. Rock Throw variant: 25 base × 2.5 = 62 HP from a single 2 AP spell.

This is the highest single-hit damage in the game for a supported two-Mancer combo — and it requires zero direct damage investment from the Geomancer until the finisher moment. The Geomancer spends its prior turns building terrain; the physical payoff arrives at the moment the Cryomancer delivers FROZEN SOLID.

### Obsidian Terrain and Temperature

When a Geomancer converts burning ground to OBSIDIAN (via Rock Throw or Earthen Smash smother), the obsidian itself applies no temperature — it is cooled, hardened stone. However, it blocks access to BURNING terrain tiles beneath it. Enemies that were standing on ON_FIRE tiles and gaining +10 temperature per turn are no longer on those tiles once Obsidian covers them. The Geomancer can "seal off" active heating zones by smothering fire into Obsidian, cutting off the opponent's access to thermal DoT ground.

This is a strategic temperature play even though no temperature value is applied: the Geomancer removes an enemy's ability to USE BURNING terrain as a self-heating weapon, which is relevant if enemies are deliberately standing on fire to reach HOT or OVERHEATED threshold faster for some unusual game state.

### Counter-Temperature Terrain Shaping

Geomancer can wall off BURNING terrain to prevent enemies from entering it (blocking temperature gain), or wall off FROZEN terrain to stop Cryomancer cooling from reaching isolated enemy units. This indirect temperature control is subtle but meaningful: a Stone Wall placed across the boundary of a fire zone prevents enemies who are at COLD (-20) from stepping into BURNING tiles to warm up, which would push them back toward NEUTRAL and away from the SUPERCOOLED threshold the Geomancer is building toward.

The same logic applies in reverse: a Stone Wall blocking access to Cryomancer ice fields prevents enemies from cooling themselves by retreating to cold ground. The Geomancer's terrain architecture dictates which thermal zones are accessible to which units — a form of temperature control through geometry rather than elemental application.

---

## 10. Augmentation Spell

### Earthen Mantle

**AP Cost:** 3 | **Range:** 2 tiles | **Targeting:** Single allied unit | **Cooldown:** 4 turns

Raises stone and compacted earth through the ground, encasing an ally's lower body in living stone -- making them an immovable fortress and terrain-shaping weapon.

**Effects (3 turns):**
- Ally is immune to all displacement -- pushes, pulls, teleports, and knockbacks have no effect on their position
- Ally's tile is treated as +1 elevation for line-of-sight purposes (the stone raises their effective height)
- Ally's movement range is halved (round down) -- stone is heavy
- Once per turn as a free action, the ally can shatter one adjacent wall tile, converting it to passable rubble (2 movement cost, provides light cover)

**Tactical intent:** Anti-displacement fortress mode for a single ally. Directly counters Aeromancer, Gravimancer, and Hydromancer push/pull kits -- the mantled ally simply does not move when forced. The LoS elevation advantage simulates high ground without changing terrain. The movement penalty is real and intended: this buff plants a unit, it does not enhance mobility. The wall-shatter free action is the strategic wrinkle -- the Geomancer can pre-wall an area, drop Earthen Mantle on an ally, and the ally bulldozes through those same walls on subsequent turns, reshaping the Geomancer's own terrain investment.

**Notable interactions:** On MUD tiles, the movement penalty compounds (MUD costs 2 per tile plus halved move range). On elevated terrain with clear sightlines, a mantled ranged unit becomes a near-immovable high-ground firing platform. Earthen Mantle + Gravimancer Gravitational Anchor on adjacent allies creates a locked formation that warps the entire flank's movement economy for enemies.

*End of Geomancer design document.*

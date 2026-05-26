# Electromancer — Full Design Document

---

## 1. Tactical Identity

The Electromancer is the game's premier combo-detonator — the Mancer that converts board states set up by every other member of its team into devastating chain-damage and mass incapacitation. In isolation, the Electromancer is a functional mid-range damage dealer. In a team, it is a force multiplier that transforms the Hydromancer's Wet tiles, the Cryomancer's ice fields, and the Geomancer's elevated positions into multi-unit stun combos and cascading burst damage that other Mancers cannot replicate. The Electromancer is the reason teams invest in WET terrain — because WET terrain under an Electromancer's spell range is not a terrain state, it is a loaded weapon.

The Electromancer's defining mechanic — the wet-tile chain arc — is the game's flagship combo and requires dedicated understanding. When an Electromancer lightning spell hits a WET unit, the arc does not stop at the primary target. It automatically chains to all adjacent WET units, and from each of those units, chains again to their adjacent WET units, propagating outward through the entire connected WET network. Every unit caught in this propagation is STUNNED (1 turn — skip entire activation). At maximum reach (Hydromancer Flood Zone saturation of a large area), a single Electromancer spell can STUN an entire enemy warband cluster simultaneously, removing their activations for the next turn entirely. This is the highest single-action swing in the game.

Playing the Electromancer well means managing two skills simultaneously: reading when the WET network is large enough to justify the chain (the value is proportional to the number of WET units the arc will reach), and positioning the Electromancer so it has LoS to initiate the chain without being exposed to the counter-pressure that telegraphed chain setups invite. The Electromancer must also track which of its own allied units are WET — chain arcs do not distinguish friendly from enemy, and a poorly timed Arc Bolt into a WET cluster that includes allied units stuns them too.

**Primary win condition:** The Electromancer wins when it fires a single lightning spell into a WET network containing 3 or more enemy units and STUNs all of them simultaneously. In the blind-turn system, STUNNED units cannot activate on their next turn — 3 simultaneous STUNs removes the opponent's entire activation budget for one full turn, creating an action economy gap that a well-prepared team can translate into multiple confirmed kills. The Electromancer's team wins in the turn after the chain fires.

**Core weakness:** The Electromancer is dependent on setup. Without WET terrain or WET units provided by a Hydromancer or other water source, Arc Bolt is a respectable single-target damage spell — nothing more. Against an opponent who never allows their units to become WET, or who spreads their formation to keep WET units non-adjacent, the chain arc is minimized or nullified. The Electromancer has no terrain-creation ability and no self-setup for chain arcs — it entirely relies on team coordination for its defining value. Additionally, STUNNED units last only 1 turn — if the Electromancer's allied team cannot follow up on the stun window with kills or major board changes, the opponent recovers and regains full control.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 90 | Below average; the Electromancer is a mid-range combatant, not a front-line unit |
| **Move Range** | 4 tiles per activation | Above average; needs repositioning to reach LoS angles for chain arc initiation |
| **Base Armor** | 1 | Minimal; the Electromancer survives on range management and team protection |
| **Spell Range** | 6 tiles (base) | Long range; enables chain arc initiation from safe rear positions |
| **AP per Activation** | 6 | Standard; movement costs 1 AP per tile |
| **Element** | Lightning | All base spells deal Lightning damage and apply lightning-element terrain/status interactions |

**AP budget example:** With 6 AP, the Electromancer can move 2 tiles (2 AP) and cast Arc Bolt twice (2 + 2 AP) to apply CHARGED to two different tiles, or move 3 tiles and cast Chain Lightning (3 AP) for the primary chain-arc detonation spell.

---

## 3. Base Spell Kit

The Electromancer's four base spells cover distinct electrical functions:
- **Arc Bolt** — repeatable single-target lightning; chain trigger if target is WET
- **Static Field** — ground-target CHARGED terrain placement; a delayed chain trap
- **Chain Lightning** — primary chain-arc spell; follows WET networks for maximum reach
- **Overload** — heavy AoE; consumes all adjacent CHARGED tiles for burst damage

---

### Spell 1: Arc Bolt

| Field | Value |
|---|---|
| **Name** | Arc Bolt |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (projectile — travels in a line) |
| **Range** | 6 tiles |
| **AoE Radius** | N/A (primary target only; chain arcs are secondary effects) |
| **Base Damage** | 18 |
| **Element** | Lightning |
| **Effects Applied** | Deals 18 Lightning damage to primary target. Target tile becomes `CHARGED` (1-turn duration; units entering or on a CHARGED tile take 12 arc damage). **WET CHAIN ARC:** If the primary target unit is `WET`, the arc chains to all adjacent WET units automatically (see WET Chain Arc documentation below). Each chained unit takes 12 arc damage and receives `STUNNED` (1 turn — skip entire turn, no AP). |
| **Temperature Effects** | **+10 temperature** (electrical current passing through the target generates resistive heat in their body). |
| **Special Interactions** | Against a `BURNING` target: Lightning hitting a BURNING unit triggers Firestorm Burst — an explosive combination dealing the arc bolt damage plus a 1-tile radius AoE fire burst (20 Fire damage) that spreads `ON_FIRE` to burst tiles. Against a `FROZEN` target: Lightning shatters ice explosively — SHATTER triggers (18 × 2.5 = 45 HP total) and additionally sends ice shards in a 1-tile radius (8 cold damage to all units adjacent to the FROZEN target). Against a `POISONED` target: Toxin shock — POISONED stacks are detonated (amplified): each stack deals 6 HP instead of 3 HP for this turn, then all stacks are consumed (removed). Net result: POISONED at 3 stacks detonated = 18 HP instant burst from toxin shock instead of the normal 3/turn decay. Against an `ON_FIRE` tile: Lightning + Fire = Firestorm Burst as above — the tile interaction not the unit interaction triggers if the target tile is ON_FIRE even without a BURNING unit. |

**Design note:** Arc Bolt is the Electromancer's workhorse and chain initiator. At 2 AP with no cooldown, three Arc Bolts in a single activation (6 AP, no movement) apply CHARGED to three separate tiles while hitting three separate targets for 18 HP each — a 54 HP total damage pass for the full 6 AP at range. But the 18 direct damage is secondary value. Primary value: Arc Bolt hitting a WET target triggers the chain arc. The chain is the Electromancer's entire identity, and Arc Bolt is the cheapest, fastest trigger for it.

**Spell answers YES to (design rule check):**
1. Applies terrain state (CHARGED) — YES
2. Applies unit status (STUNNED on WET chain targets) — YES
3. Synergizes with every water Mancer (WET chain), Pyromancer (Firestorm Burst), Cryomancer (SHATTER burst) — YES
4. Skill expression: WET target identification; chain network size assessment before casting — YES

---

### Spell 2: Static Field

| Field | Value |
|---|---|
| **Name** | Static Field |
| **AP Cost** | 2 AP |
| **Cooldown** | 1 turn |
| **Targeting Type** | Ground Target — Terrain Placement |
| **Range** | 5 tiles |
| **AoE Radius** | N/A (single tile placement; or up to 2 adjacent tiles for 3 AP) |
| **Base Damage** | 0 (placement spell; no direct damage on cast) |
| **Element** | Lightning |
| **Effects Applied** | Target tile becomes `CHARGED` (2-turn duration; persists longer than the CHARGED from Arc Bolt). Units entering or standing on a `CHARGED` tile at the start of their turn take 12 arc damage and may be `STUNNED` (1 turn; triggers only if the unit was also `WET` at the moment of arc discharge, or if a second CHARGED arc triggers simultaneously adjacent to them). The CHARGED tile discharges the arc once per unit contact; it does not discharge repeatedly on the same unit (1 discharge per unit per CHARGED tile contact, then the unit is immune to that specific tile for 1 turn). |
| **Temperature Effects** | **+10 temperature per tick to all units in the field** (sustained electrical exposure heats units standing in the charged zone over time). |
| **Special Interactions** | Against a `WET` tile targeted for Static Field placement: the water conducts the charge immediately — the CHARGED tile placed on WET terrain has enhanced arc range. When a unit steps onto this `CHARGED + WET` tile, the arc fires AND propagates as a full chain arc through all adjacent WET units (not just the tile contact unit, but the entire WET network from that point). This is the Electromancer's "trap and chain" mechanic — the opponent doesn't see the chain potential until they step on the tile. Against an `ICE_TILE` (FREEZE_CONDUCTOR from Cryomancer-Electromancer interaction): a Static Field placed on an ICE_TILE creates a FREEZE_CONDUCTOR tile with +50% chain arc range. Against `FLOODED` terrain: the charge distributes through the entire connected FLOODED zone — all FLOODED tiles in the connected zone become CHARGED simultaneously (water distributes the field). If any unit enters the FLOODED zone while it is CHARGED, they trigger the discharge for the entire zone (chain arc from their entry point through all adjacent WET units in the zone). |

**Design note:** Static Field is the Electromancer's terrain denial and trap tool. Unlike Arc Bolt which triggers immediately, Static Field is a delayed threat — enemies must move onto or through the CHARGED tile to trigger it, which creates a zone of psychological pressure. An opponent who cannot afford to walk into CHARGED terrain must route around it (paying AP movement cost to avoid) or accept the arc discharge risk. The FLOODED zone variant (Static Field on FLOODED = entire zone becomes CHARGED) is the setup tool for the game's highest-value chain stun: once every FLOODED tile in a large zone is CHARGED, the next unit who steps onto any tile in the zone triggers a chain arc through every WET unit in the zone simultaneously.

**Spell answers YES to (design rule check):**
1. Applies terrain state (CHARGED, FREEZE_CONDUCTOR, FLOODED-zone-CHARGED) — YES
2. Applies unit status (STUNNED on arc trigger by WET unit) — YES
3. Creates movement-denial zone (enemies avoid CHARGED terrain) — YES
4. Synergizes with Hydromancer (FLOODED zone charging), Cryomancer (FREEZE_CONDUCTOR) — YES
5. Skill expression: trap placement on predicted enemy movement paths; FLOODED zone charging for deferred chain setup — YES

---

### Spell 3: Chain Lightning

| Field | Value |
|---|---|
| **Name** | Chain Lightning |
| **AP Cost** | 3 AP |
| **Cooldown** | 1 turn |
| **Targeting Type** | Single Target (projectile; but chain resolution extends reach — see below) |
| **Range** | 6 tiles (to primary target) |
| **AoE Radius** | N/A for primary; chain arcs extend from primary target outward |
| **Base Damage** | 25 (primary target) |
| **Element** | Lightning |
| **Effects Applied** | Deals 25 Lightning damage to primary target. Target tile becomes `CHARGED` (1 turn). **WET CHAIN ARC (FULL DOCUMENTATION):** If the primary target is `WET`, the chain arc fires. Chain arc rules: 1) The arc propagates from the primary target to all adjacent `WET` units (within 1 tile). 2) Each chained unit takes 18 chain arc damage and receives `STUNNED` (1 turn). 3) From each chained unit, the arc propagates again to all of that unit's adjacent `WET` units not yet hit. 4) This propagation continues outward until no more adjacent WET units exist to chain to. The chain does not have a maximum hop count — it will traverse the entire WET network from the initial target. 5) Chain arc does not distinguish friendly from enemy — all WET units in the propagation path are affected equally. 6) The arc travels through `WET` tiles as well as WET units: a WET tile (not a unit on a WET tile, but the tile itself being in WET terrain state) that is adjacent to a WET unit extends the chain arc through it, allowing the arc to "hop" across empty WET tiles to reach WET units on the far side. This means a line of WET tiles between two WET unit groups connects them for chain arc purposes even if no unit stands on the intermediate tiles. |
| **Temperature Effects** | **+10 temperature to EACH unit in the chain** (every link in the arc — primary target and all chain-hop targets — receives electrical heating from the current passing through them). |
| **Special Interactions** | Against a `FLOODED` zone: if the primary target is in a FLOODED zone, all units in the zone are WET (FLOODED grants WET to all units inside). Chain Lightning hitting any unit in the zone chains to every other unit in the zone. A fully saturated FLOODED zone hit by Chain Lightning from one Electromancer spell is a total zone chain stun. This is the Hydromancer + Electromancer Flagship Combo — see Section 7. Against a `BURNING` primary target: Firestorm Burst triggers simultaneously with the chain arc (BURNING + Lightning at primary target = explosion) AND the chain arc fires from the BURNING target to all adjacent WET units. Both interactions resolve: the primary target takes arc damage + Firestorm Burst (bonus fire AoE); adjacent WET units take chain arc damage and STUN. Against a `FROZEN` primary target: SHATTER triggers at primary target (25 × 2.5 = 62.5 HP) and the chain arc fires from the FROZEN/SHATTERED target — the shatter explosion sends debris that applies 10 cold-shard damage to adjacent units in addition to the chain arc damage. |

**Design note:** Chain Lightning is the Electromancer's signature spell — the one that fully expresses its kit identity. At 3 AP, it is the optimal AP/value trade: a Standard spell that deals 25 HP direct plus potentially unlimited chain arc hits at 18 HP each and STUNNED on every chain target. Against a fully WET formation of 4 enemies, Chain Lightning costs 3 AP and delivers 25 + (18 × 3) = 79 HP total damage AND applies STUNNED to 3 enemies. No other 3-AP spell in the game approaches this value ceiling — but that ceiling requires setup. Against dry targets with no WET units, Chain Lightning is 25 HP at 3 AP with a CHARGED tile leftover — inferior to Arc Bolt's efficiency at 18 HP for 2 AP.

**Spell answers YES to (design rule check):**
1. Exploits terrain states (WET network triggers chain arc; FLOODED zone = full zone chain) — YES
2. Applies unit status (STUNNED on all chain arc targets) — YES
3. Creates terrain state (CHARGED on primary tile) — YES
4. Synergizes with Hydromancer (WET and FLOODED), Cryomancer (SHATTER + cold shard chain), Pyromancer (Firestorm Burst + chain) — YES
5. Skill expression: WET network size assessment; chain arc path geometry prediction; friendly-fire WET tracking — YES

---

### Spell 4: Overload

| Field | Value |
|---|---|
| **Name** | Overload |
| **AP Cost** | 5 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Ground Target — AoE Radial |
| **Range** | 4 tiles (to center of AoE) |
| **AoE Radius** | 2 tiles |
| **Base Damage** | 30 (all units in AoE) |
| **Element** | Lightning |
| **Effects Applied** | Deals 30 Lightning damage to all units in 2-tile radius. All units in the AoE receive `STUNNED` (1 turn) if they are `WET` at the time of impact. All `CHARGED` tiles within 3 tiles of the AoE center are consumed: each CHARGED tile discharges simultaneously, dealing its 12 arc damage to any units on those tiles as a bonus burst. After the CHARGED tiles discharge, they are converted to `OVERLOADED_TILE` — a temporary terrain state where any Lightning spell hitting the tile in the next 1 turn deals +50% bonus damage (overloaded electrical surface). |
| **Temperature Effects** | **+15 temperature** (massive electrical discharge delivers substantially more heat than a standard arc — the overloaded current saturates the target zone with electrical heat). |
| **Special Interactions** | Against a FLOODED zone in the AoE: all WET units in the zone are STUNNED (full zone stun applies — FLOODED WET network connects to the AoE WET chain on impact). Against a zone pre-seeded with Static Field CHARGED tiles: Overload consuming 3+ CHARGED tiles in a single cast deals 12 arc damage per CHARGED tile consumed (36+ bonus burst) in addition to the base 30 AoE damage. A Electromancer who pre-seeds 3 CHARGED tiles adjacent to the Overload target zone and then fires Overload deals 30 + 36 = 66 HP burst across the AoE. Against `BURNING` units in the AoE: Firestorm Burst triggers on each BURNING unit hit (adds 1-tile AoE fire burst per BURNING unit). Against a `FROZEN` target in the AoE: SHATTER triggers on FROZEN (30 × 2.5 = 75 HP). |

**Design note:** Overload is the Electromancer's "end this formation" ability. At 5 AP, it consumes almost the full activation. Its value is maximum when the Electromancer has pre-staged the field with Static Field CHARGED tiles adjacent to the target zone AND the Hydromancer has applied WET to the zone. In that scenario: 30 AoE base + STUN on all WET units in radius + consumed CHARGED tile discharges (12 HP each) + OVERLOADED_TILE bonus for follow-up. The 3-turn cooldown ensures Overload is a decisive but non-spammable tool. Use it when the setup is fully staged; do not use it as a panic button on empty terrain.

**Spell answers YES to (design rule check):**
1. Applies terrain state (OVERLOADED_TILE; consumes CHARGED) — YES
2. Applies unit status (STUNNED on WET units in AoE) — YES
3. Synergizes with Static Field pre-staging, Hydromancer WET, Pyromancer BURNING — YES
4. Skill expression: pre-staged CHARGED tile placement for bonus discharge; WET timing for mass stun — YES

---

## 4. Terrain Interaction Table

### WET CHAIN ARC — Full Documentation

This section fully documents the Electromancer's flagship mechanic. All Electromancer spells that deal Lightning damage trigger chain arc effects when hitting WET units. Chain arcs are the defining mechanic of the entire Electromancer kit.

**What triggers a chain arc:**
Any Lightning spell (Arc Bolt, Chain Lightning, Overload, the Electromancer's signature ability) that hits a unit with the `WET` status on it, OR hits a tile in `WET`, `FLOODED`, or `CHARGED + WET` terrain state.

**Chain arc propagation rules:**
1. The initial Lightning bolt hits the primary WET target. Full spell damage applies to the primary target.
2. The arc then fires to every unit within 1 tile of the primary target that is also `WET`. These are "Chain Hop 1" targets.
3. Each Chain Hop 1 target takes 18 chain arc damage and receives `STUNNED` (1 turn).
4. From each Chain Hop 1 target, the arc fires again to every unit within 1 tile of that Chain Hop 1 target that is also WET and has not yet been hit in this chain. These are "Chain Hop 2" targets.
5. Chain Hop 2 targets take 18 chain arc damage and `STUNNED` (1 turn).
6. Propagation continues outward (Chain Hop 3, 4, etc.) until no more adjacent WET units exist that haven't already been hit.
7. WET tile propagation: an empty WET tile between two groups of WET units acts as a bridge — the arc hops through the WET tile to reach WET units on the far side. The WET tile itself is not damaged, but it becomes `CHARGED` (1 turn) as the arc passes through it.
8. **Friendly fire:** The chain arc does not distinguish allied from enemy units. Any WET allied unit adjacent to the chain path is also hit for 18 arc damage and STUNNED. The Electromancer player must track all WET units on the board — both enemy AND allied — before initiating a chain.

**Chain arc damage values by hop:**
| Hop | Damage | Status Applied |
|---|---|---|
| Primary target | Full spell damage (Arc Bolt: 18; Chain Lightning: 25; Overload: 30 within AoE) | No STUN on primary (primary takes full spell damage, not chain damage) |
| Chain Hop 1–N | 18 arc damage per hop | `STUNNED` (1 turn) per hit |

**Chain arc range modifiers:**
| Condition | Effect on Chain Arc |
|---|---|
| `FLOODED` zone (all tiles WET) | Chain arc propagates through the entire FLOODED zone — every unit in the zone is in the WET network |
| `FREEZE_CONDUCTOR` tile (Cryomancer ICE_TILE + CHARGED) | Chain arc range +50% from that tile (arc jumps 1.5 tiles rather than 1 from FREEZE_CONDUCTOR) |
| `BLUSTERED` zone (Aeromancer) | Chain arc slightly disrupted — range reduced by 0.5 tile from BLUSTERED tiles (partial counter) |
| Target has `HEAVY` status | No effect on chain arc (HEAVY only resists displacement, not electrical conductivity) |
| Target has `WEIGHTLESS` status | Chain arc still applies — WEIGHTLESS units are still electrically conductive |

**The Hydromancer + Electromancer Flagship Combo (The Shock Network):**

This is the game's most famous two-Mancer combo. Full execution documentation follows because this combo is the core reason the Electromancer is a roster-defining piece and the reason the Hydromancer is universally considered the best team Mancer.

*Step 1 — Hydromancer setup:*
Hydromancer casts Aqua Lance (2 AP) at the highest-value enemy unit in a cluster. The hit unit receives `WET` (2 turns). If 2 AP remain and a second adjacent enemy is in range, Hydromancer casts Aqua Lance again (2 more AP). After 4 AP, 1–2 enemies are WET and adjacent to each other.

*Step 2 (optimal setup variant) — Hydromancer Flood Zone:*
If the map position supports it and AP budget allows, Hydromancer casts Flood Zone (5 AP) centered on the enemy cluster instead of Aqua Lance. All tiles in the 3-tile radius become FLOODED; all units in the zone receive WET (3 turns). Multiple enemies are simultaneously WET in a connected conductive zone. This turn is a near-full Hydromancer commitment.

*Step 3 — Electromancer activation (same turn, Mancer initiative order after Hydromancer):*
Electromancer casts Arc Bolt (2 AP) or Chain Lightning (3 AP) at any WET unit in the cluster.

*Step 4 — Chain arc resolution:*
Arc hits primary WET target (full damage). Arc then chains to all adjacent WET units. Each chained unit takes 18 arc damage and is STUNNED (1 turn). Arc continues propagating outward through the WET network until no more WET adjacencies remain.

*Outcomes by scenario:*

| Setup | Units Affected | Result |
|---|---|---|
| Aqua Lance on 1 target, adjacent ally also WET | Primary + 1 chain | Primary takes full Arc Bolt damage; 1 unit STUNNED |
| Aqua Lance on 2 adjacent targets | Primary + 1 chain | Primary takes full damage; 1 STUN |
| Flood Zone (3-tile radius, 4 enemies in zone) | Primary + 3 chains | Primary takes full damage; 3 enemies STUNNED |
| Flood Zone (3-tile radius, 6 enemies in zone) | Primary + 5 chains | Primary takes full damage; 5 enemies STUNNED simultaneously |

*Tactical outcome:*
At minimum (2 WET targets), one enemy is STUNNED for the next turn. At maximum (Flood Zone with 6 enemies), 5 enemies are simultaneously STUNNED, removing the opponent's entire activation pool for the following turn. In the blind-turn system, where both players plan simultaneously, STUNNED units cannot be meaningfully planned around — the opponent's activations for STUNNED units are wasted. A 5-unit simultaneous STUN at the critical turn of a match is effectively match-decisive.

*Counter-play:*
The Shock Network is readable. A WET enemy visible on the board signals the chain setup. Skilled opponents will:
1. **Disperse formation** — keep WET units non-adjacent to prevent chain propagation.
2. **Retreat off WET tiles** before Electromancer activation — units that move off WET terrain lose the WET status when they leave and the chain cannot follow them.
3. **Advance out of FLOODED zone** — units that exit the FLOODED zone before Electromancer acts are no longer WET-connected.
4. **Prioritize killing the Electromancer** — no Electromancer = no chain detonation. The Electromancer is the highest-value target in a WET-network team.
5. **Invest in WEIGHTLESS or terrain-avoidance** — WEIGHTLESS units are immune to WET terrain contact (they float above the water surface) and thus do not become WET from standing on WET tiles. This does not help against direct Aqua Lance WET application (the spell applies WET to the unit directly), but prevents passive WET accumulation from terrain.

The STUNNED duration is only 1 turn. The Electromancer's team must capitalize on the stun window decisively — kills confirmed, positions advanced — or the opponent recovers.

---

### Lightning Spell Impact on Existing Terrain States

| Existing Terrain State | What Happens When Lightning Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **Normal (GROUND)** | Lightning grounds normally | `CHARGED` (1 turn) | Takes spell damage | CHARGED tile persists; units entering take 12 arc damage next turn |
| **WET** | Water conducts the charge massively | `CHARGED` (1 turn; full chain arc triggers through all connected WET tiles/units) | Takes spell damage + STUNNED if hit by chain arc | This is the core WET chain interaction — documented fully above |
| **FLOODED** | Entire connected FLOODED zone conducts the charge | `CHARGED` across all connected FLOODED tiles | All units in FLOODED zone take chain arc damage + STUNNED | The highest-value terrain interaction in the game; the Shock Network combo detonation surface |
| **ICE_TILE (FREEZE_CONDUCTOR)** | Cold electrical surface amplifies arc range | `CHARGED ICE_TILE` (FREEZE_CONDUCTOR; chain arc range +50%) | Takes spell damage + enhanced chain arc origin | The Cryomancer + Electromancer FREEZE_CONDUCTOR combo — ice plus electricity amplifies both effects |
| **ICE_TILE (standard)** | Lightning shatters frozen surface | `WET` (ice shattered by arc) | Takes spell damage + if FROZEN: SHATTER (×2.5); plus ice shard spray (8 cold damage to adjacent units) | FROZEN units are SHATTERED by lightning — a Cryomancer + Electromancer two-Mancer SHATTER option |
| **ON_FIRE** | Firestorm Burst — electrical arc detonates fire | `ON_FIRE` (intensified; spreads 1 additional tile this turn) | Takes spell damage + Firestorm Burst: 1-tile AoE fire burst (20 Fire damage to adjacent units) | The Pyromancer + Electromancer combo: fire terrain amplified by lightning into an explosive AoE |
| **TOXIC_TERRAIN** | Electrical toxin shock | `TOXIC_TERRAIN` (unchanged) + `CHARGED` overlay | Takes spell damage + POISONED stacks amplified (each POISONED stack deals 6 HP on shock; then all stacks consumed) | Toximancer + Electromancer toxin detonation: POISONED stacks instantly detonated rather than ticking |
| **CHARGED** | Arc overloads the existing charge | `OVERLOADED_TILE` (next Lightning spell hitting this tile deals +50% damage) | Takes spell damage + 12 arc damage (tile discharges on hit) | Double-charging creates an overloaded zone; subsequent Lightning hits deal +50% for 1 turn |
| **MUD** | Wet mud conducts partially | `CHARGED MUD` (half-conductivity: chain arc range –1 tile through MUD) | Takes spell damage + chain arc fires with reduced range | MUD conducts but not as efficiently as WET; chain arcs lose 1 hop distance through MUD tiles |
| **OBSIDIAN** | Arc bounces off obsidian | `OBSIDIAN` (unchanged) | Takes spell damage; arc reflects 1 tile back toward caster direction | Arc reflection is unique: the bolt bounces — the reflected arc hits the tile behind the obsidian in the original bolt trajectory and applies CHARGED there |
| **PERMAFROST** | Ice on frozen mud — FREEZE_CONDUCTOR variant | `CHARGED PERMAFROST` (similar to FREEZE_CONDUCTOR; chain arc +25% range) | Takes spell damage + enhanced chain | Lesser than ICE_TILE FREEZE_CONDUCTOR but provides partial range bonus |

### Terrain States Beneficial to the Electromancer

| State | Benefit |
|---|---|
| `WET` tiles and units | The entire value stack of the Electromancer's kit — chain arcs, STUN propagation, combo finishes |
| `FLOODED` zones | Mass chain arc potential; Static Field on FLOODED = entire zone CHARGED simultaneously |
| `ON_FIRE` tiles (Pyromancer) | Firestorm Burst combo; Arc Bolt on BURNING target = AoE fire expansion |
| `FROZEN` units (Cryomancer) | SHATTER via Chain Lightning — 25 × 2.5 = 62 HP from a 3 AP spell |
| `CHARGED` tiles (pre-staged) | Overload consuming CHARGED tiles deals bonus burst; Static Field chains compound |

### Terrain States Hazardous to the Electromancer

| State | Hazard |
|---|---|
| `ON_FIRE` | No fire immunity; 5 HP/turn DoT on burning ground |
| `ICE_TILE` | Slip checks apply; at 90 HP with 1 armor, involuntary slides into hazard terrain are punishing |
| `WET` tiles the Electromancer itself stands on | If an enemy Electromancer (or ally's misfire) chains an arc through the Electromancer's position, it takes the chain damage and is STUNNED. An Electromancer caught in its own Shock Network is incapacitated for a turn. |
| `TOXIC_TERRAIN` | POISONED stacks apply; 90 HP makes DoT compounding relevant |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

#### Variant A: Thunderbolt (replaces Arc Bolt) — +20 pts

**Description:** Arc Bolt is replaced by Thunderbolt — a more powerful, wider-radius single bolt. Thunderbolt deals 28 damage (up from 18) and applies a 1-tile AoE stun on impact: all units adjacent to the primary target receive `STUNNED` (1 turn) regardless of their WET status (the concussive lightning impact stuns nearby units). Chain arc still fires from the primary target if it is WET. AP cost is 3 AP; cooldown is 1 turn.

**Trade-off:** Significantly higher burst and guaranteed adjacent stun at the cost of Arc Bolt's 0-cooldown repeatable nature and +1 AP. The adjacent stun is notable: Thunderbolt can stun non-WET units through proximity to the WET primary target, reducing the chain setup requirement. Best for Electromancers built as primary damage dealers rather than pure chain-detonators.

#### Variant B: Lightning Trap (replaces Static Field) — +20 pts

**Description:** Static Field is replaced by Lightning Trap — a more powerful and longer-lasting trap. Lightning Trap places a CHARGED tile with a 3-turn duration (up from 2) that stores a more powerful discharge: units triggering the trap take 20 arc damage (up from 12) and are automatically STUNNED (regardless of WET status — the trap's concentrated charge stuns any unit that steps on it). AP cost remains 2 AP; cooldown is 1 turn. Only 1 Lightning Trap can be active at a time (placing a second removes the first).

**Trade-off:** Guaranteed stun on any unit (not WET-dependent) at the cost of having only 1 active trap at a time vs. the Static Field's ability to CHARGE terrain broadly. Best for Electromancers focused on area denial and trap-play rather than FLOODED zone charging.

#### Variant C: Ball Lightning (replaces Overload) — +25 pts

**Description:** Overload is replaced by Ball Lightning — a rolling spherical lightning projectile that travels 5 tiles in a chosen direction, striking all units in its 5-tile path for 22 damage each and applying CHARGED to each tile it passes through. If the Ball Lightning passes over a WET tile or WET unit at any point, it detonates immediately at that position: full Overload-equivalent AoE burst (30 damage, 2-tile radius, STUNNED on WET units in radius) centered on the detonation tile. If no WET tile is encountered, it travels the full 5 tiles and dissipates. AP cost is 4 AP; cooldown is 2 turns.

**Trade-off:** Longer reach and guaranteed CHARGED tile seeding across a 5-tile path. WET detonation is situational — the ball lightning acts as both a terrain charger and an opportunistic chain trigger. Best in maps where the Electromancer cannot guarantee proximity to the WET cluster but can seed CHARGED terrain along known movement paths.

---

### Passive Traits

#### Passive A: Conductive Body — +20 pts

**Description:** The Electromancer itself is immune to chain arc damage (it does not take arc damage from its own chain arcs — it is the source, not a target). Additionally, the Electromancer always counts as `WET` for chain arc propagation purposes — chain arcs passing near the Electromancer can use it as a propagation node. Note: this does not mean the Electromancer takes chain damage from enemy arcs; its own conductivity is controlled. The immunity is specifically to its own chain propagation.

**Trade-off:** Eliminates the risk of self-chain from Arc Bolt fired near WET allied units. Most relevant for Electromancers in tight formation play where WET terrain surrounds the Electromancer's own position. Also enables the Electromancer to be a chain relay node — positioning the Electromancer between two disconnected WET clusters allows the chain to pass through it and reach the far cluster.

#### Passive B: Storm Charge — +25 pts

**Description:** Whenever the Electromancer's chain arc STUNS 2 or more units in a single activation, the Electromancer immediately restores 2 AP (recovered once per activation, not per stun pair). Additionally, the first Lightning spell the Electromancer casts after a rest turn (a turn in which it did not activate) costs 0 AP — the stored energy from rest converts to a free initial cast.

**Trade-off:** High-value passive for chain-focused play. The 2 AP recovery from a multi-STUN chain effectively means successful chain executions partially pay for themselves (Chain Lightning at 3 AP nets 3 – 2 = 1 AP effective cost on a 2+ STUN chain). Best for Electromancers in teams with reliable WET setup who activate frequently.

#### Passive C: Voltaic Aura — +15 pts

**Description:** Allied units within 2 tiles of the Electromancer gain a persistent +8% resistance to physical damage (the electrical field around the Electromancer partially deflects kinetic impacts). Additionally, enemy units within 2 tiles of the Electromancer that are also `WET` take 4 arc damage at the start of each Electromancer activation (ambient discharge — the Electromancer leaks electrical energy passively when WET conductors are near).

**Trade-off:** Modest defensive aura for allies + passive WET-punish damage. Neither effect is individually strong, but the combination of protecting allies and passively damaging WET enemies without spending AP makes Voltaic Aura a consistent background value source in any WET-heavy engagement.

#### Passive D: Arc Mastery — +20 pts

**Description:** The Electromancer's chain arc damage per hop increases from 18 to 22. Additionally, chain arcs from the Electromancer's spells can hop to units 2 tiles away from the previous node (instead of the standard 1 tile) when those units are on FLOODED terrain. This extends the effective chain reach through large FLOODED zones and allows the arc to skip over empty tiles between WET unit clusters.

**Trade-off:** Significant chain arc amplification — 22 vs. 18 is a 22% damage increase per hop, and the extended range through FLOODED terrain allows the chain to span larger zones. Best for Electromancers in Hydromancer-heavy teams that frequently create large FLOODED combat areas.

---

### Stat Enhancements

#### Stat A: Capacitor Core (+15 HP) — +10 pts

**Description:** Max HP increases from 90 to 105. Brings the Electromancer close to average Mancer HP. At 90 HP with 1 armor, the Electromancer is eliminated by coordinated burst — two Crossbow Corps shots and an enemy Arc Bolt can reach critical HP thresholds. 105 HP provides one additional burst exchange survival window.

#### Stat B: Conductor's Range (+1 Spell Range) — +15 pts

**Description:** All Electromancer spell ranges increase by 1 tile. Arc Bolt: 6 → 7. Chain Lightning: 6 → 7. Static Field: 5 → 6. Overload: 4 → 5. At 7 tiles, the Electromancer can initiate chain arcs from a significantly safer rear position, reducing its exposure to melee and medium-range counter-threats.

**Design note:** The Electromancer's 6-tile base range is already competitive. The +1 mostly matters on maps where 6 tiles of range is 1 tile short of a safe initiating position — which is a map-specific value rather than a universal upgrade.

---

### Signature Ability

The Signature Ability is unlocked as an additional spell slot — it does not replace any base spell.

#### Signature: Judgement Arc — +40 pts

| Field | Value |
|---|---|
| **Name** | Judgement Arc |
| **AP Cost** | 6 AP (entire activation; Electromancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Single Target (massive lightning strike; projectile targeting) |
| **Range** | 8 tiles (extended range — the Electromancer calls down a sky-strike) |
| **AoE Radius** | 1 tile (tight impact zone) |
| **Base Damage** | 50 (primary target tile) |
| **Element** | Lightning |
| **Effects Applied** | Deals 50 Lightning damage to the primary target. The primary target tile becomes `CHARGED` (3-turn duration; no expiry when unit walks on it — the charge is too dense). **JUDGMENT ARC CHAIN:** If the primary target is WET, or if the primary target tile is WET or FLOODED, the arc fires with maximum range: chain arcs propagate as normal but with double hop range (2 tiles instead of 1) and full Chain Lightning damage per hop (25 HP per chain target, rather than the standard 18 HP chain). All chained targets receive STUNNED (1 turn) as normal. All CHARGED tiles within 5 tiles of the primary impact are consumed simultaneously (all discharge at once for 12 HP each to units on those tiles). |
| **Special Interactions** | Against a FLOODED zone in the primary target area: the Judgment Arc's double-hop range means the chain can reach units 2 tiles apart across the FLOODED zone. A Flood Zone saturated with Wet enemies hit by Judgment Arc propagates the chain across the entire zone AND reaches enemies at the zone's periphery who are up to 2 tiles from the nearest WET unit. This is the maximum chain reach in the game. Against BURNING primary target: Firestorm Burst at 2-tile radius (doubled from standard Burst). Against FROZEN primary target: SHATTER (50 × 2.5 = 125 HP — confirmed kill on all but the most HP-upgraded Mancers) plus the standard ice shard spray at 2-tile radius. If the primary target has both WET and FROZEN status (ice Cryomancer freeze + Hydromancer wet, simultaneously possible): SHATTER triggers AND chain arc fires from the SHATTERED unit simultaneously — the shatter explosion spreads the chain. |

**Design note:** Judgement Arc is the Electromancer's "the preparation was worth it" ability. The 40-point premium, full-turn commitment, and 5-turn cooldown restrict it to once-per-fight use at most. But executed correctly — on a Flood Zone-saturated board with 4–6 WET enemy units, after 2–3 turns of Static Field staging — Judgment Arc does more damage in one activation than most Mancers deal in an entire match. The double-hop range means WET units don't need to be directly adjacent to chain; they just need to be within 2 tiles. On a FLOODED zone with enemies at moderate spacing, the chain reaches every unit in the zone.

**Synergy note:** Judgment Arc + Hydromancer Flood Zone + a turn of Cryomancer mass-CHILL is the game's highest-damage single-activation combo chain. Cryomancer's Blizzard Field CHILLS the zone; Hydromancer's Flood Zone saturates it with WET; Electromancer fires Judgment Arc into the zone for 50 HP primary + 25 HP to every CHILLED WET unit in range (each a STUN) plus the bonus on CHILLED targets from any interaction with the FREEZE_CONDUCTOR state.

---

## 6. Faction Synergy

### Best Faction: Any (Universal Value)

The Electromancer is explicitly noted in warbands.md as having high universal value across all factions. Its chain arc is the only Mancer ability that scales with warband composition outside the Mancer pool — the more WET units exist on the board, the more devastating the chain. All three factions benefit from this equally.

**That said, faction trait nuances:**

### The Gilded Throne — Iron Discipline Chain Safety

Iron Discipline (immunity to Panic and Charm) does not interact directly with the Electromancer's mechanics. However, Gilded Throne's Wailing Shades interaction is relevant for the Ashen Covenant faction instead (see below). The Throne's primary pairing benefit is Siege Arbalest physical follow-up: after the Electromancer STUNs multiple enemies, Siege Arbalests (firing every turn) have zero-AP-cost attack positions against STUNNED enemies who cannot evade. STUNNED duration is 1 turn — Siege Arbalests activate the same turn the STUN resolves (simultaneous activation in blind-turn; Mancers resolve before Ranged, so STUN applies before Arbalest fires).

**Crossbow Corps nuance:** Crossbow Corps alternate fire (attack/reload turns). On turns they reload, they cannot exploit STUN windows. Siege Arbalests (T2) fire every turn — they are the correct ranged unit for capitalizing on Electromancer STUN timing. A list with Electromancer + maximum Siege Arbalests punishes every STUN window with consistent physical fire.

### The Verdant Pact — WET and Poison Combo Access

Verdant Pact's Glade Archers apply POISONED on hit. A POISONED WET unit hit by Chain Lightning detonates its POISONED stacks (Toxin Shock: each stack deals 6 HP instead of 3 HP, then stacks consumed) AND receives chain arc STUN. This is a 3-element interaction without a third Mancer: Glade Archer poisons → Hydromancer wets → Electromancer chain = stun + toxin detonation on every POISONED WET unit in the chain.

Terrain Bond on natural tiles does not conflict with WET tile creation — but the Verdant Pact Electromancer team must be careful: Glade Archers on natural tiles adjacent to WET terrain are within Electromancer chain range if they themselves get WET (from Hydromancer spillover). The Verdant Pact player managing an Electromancer must track which natural tiles are safe from WET contamination.

### The Ashen Covenant — Wailing Shades Synergy

Wailing Shades have a Silence aura (enemy on-death effects silenced within 1 tile). This is directly valuable against the Electromancer's most dangerous scenario: an enemy unit killed by chain arc damage that has DEATH_MARK (Necromancer ability: explode on death). A DEATH_MARK enemy killed by chain arc would normally explode, potentially hitting the Electromancer's own chaff. Wailing Shades adjacent to DEATH_MARK targets suppress the death explosion — the Ashen Covenant can run Electromancer safely against DEATH_MARK threats.

Grave Husks advancing through BURNING terrain (regen via Deathless absorption) create an interesting positioning scenario: if the Electromancer wets those BURNING ground tiles (Hydromancer ally) while Husks are on them, the Husks become WET and are chain-arc-eligible. The Electromancer player must ensure Husks are not WET during chain activations, or position chain arcs to avoid the Husk line. The Deathless Ranks' fearlessness does not protect against STUN — a STUNNED Grave Husk cannot activate.

Void Wraiths (T2 Ranged) fire projectiles that bypass magical barriers. Combined with the Electromancer's STUN making enemies defenseless, Void Wraiths can target STUNNED units regardless of any cover or barrier state — a reliable kill delivery on stun windows.

---

## 7. Combo Chains

### Combo 1 — The Shock Network (Electromancer + Hydromancer) [GAME'S FLAGSHIP COMBO]

**Mancers involved:** Electromancer + Hydromancer

This is the game's primary advertised cross-Mancer combo and the defining reason both Mancers earn their roster slots in competitive play. Full documentation is provided here and cross-referenced in the Hydromancer's design document.

**Complete step-by-step execution:**

*Option A — Aqua Lance Setup (efficient; lower AP cost):*

1. **Turn N, Hydromancer activates first (same turn as Electromancer, per Mancer initiative):** Hydromancer casts Aqua Lance (2 AP) at the highest-value enemy target within a cluster of 2+ adjacent enemies. The hit unit receives `WET` (2 turns). If the Hydromancer has remaining AP (2+ AP after Aqua Lance), it casts Aqua Lance a second time at a second adjacent enemy (another 2 AP). Result: 1–2 enemies are `WET`, adjacent to each other.

   Total Hydromancer investment: 4 AP for 2 WET applications (with 2 AP remaining for 2 tiles of movement).

2. **Turn N, Electromancer activates (resolves after Hydromancer per initiative, same activation turn):** Electromancer casts Arc Bolt (2 AP) at any WET unit in the group. This is the chain initiator.

3. **Chain arc resolution:**
   - Arc Bolt deals 18 damage to the primary WET target.
   - Arc fires from primary to all adjacent WET units (Chain Hop 1). Each takes 18 chain damage + `STUNNED` (1 turn).
   - Arc propagates from each Chain Hop 1 target to their adjacent WET units (Chain Hop 2+). Propagation continues until no more adjacent WET units.

4. **Minimum outcome (1 WET target, Arc Bolt):** Primary takes 18 damage; 1 adjacent WET unit takes 18 + STUN.
   **Strong outcome (2 WET adjacent targets):** Primary takes 18 damage; 2 adjacent WET units each take 18 + STUN.
   **Total Electromancer AP spent: 2 AP.**

*Option B — Flood Zone Setup (dominant; highest value):*

1. **Turn N, Hydromancer activates:** Hydromancer casts Flood Zone (5 AP) centered on the enemy cluster. All tiles in the 3-tile radius become `FLOODED` (FLOODED grants WET to all units in the zone). All units in the zone (3–8 enemies) are now WET.

   Total Hydromancer investment: 5 AP (leaves 1 AP for 1 tile of movement).

2. **Turn N, Electromancer activates:** Electromancer casts Chain Lightning (3 AP) at any WET unit in the FLOODED zone.

3. **Chain arc resolution:**
   - Chain Lightning deals 25 damage to primary WET target.
   - Chain propagates through the entire FLOODED zone (all tiles are WET; all connected — the chain hops through empty WET tiles).
   - Every unit in the zone takes 18 chain arc damage + `STUNNED` (1 turn).

4. **Maximum outcome (6 enemies in FLOODED zone):** Primary takes 25 damage; 5 enemies each take 18 + STUN. Total AP: 5 (Hydromancer Flood Zone) + 3 (Electromancer Chain Lightning) = 8 AP across two Mancers.

   **Effective action economy impact:** 5 enemy units STUNNED (cannot activate next turn) vs. 8 AP spent. In the blind-turn system, 5 opponent activations forfeited = catastrophic action swing. Even at 3-unit STUN, the swap is favorable: 3 opponent turns lost for 8 AP of two-Mancer investment.

**Why this combo defines the game:**

The Shock Network is the reason team building in Battlemancers revolves around WET terrain management. Any warband that includes both a Hydromancer and an Electromancer is implicitly asking: "how many WET units can we create before the Electromancer fires?" Every Hydromancer spell, every allied Aqua Lance, every Flood Zone cast is evaluated against the Electromancer's potential chain value. The combo's output is not linear — it scales superlinearly with the number of WET targets because each additional WET unit adds both more chain damage AND more STUN turns. 1 WET target = 1 stun; 5 WET targets = 5 stuns. The jump from 4 to 5 stuns is as meaningful as the jump from 0 to 1.

**Counter-play (full documentation):**

A. **Formation spread:** Keep units non-adjacent. WET units that are 2+ tiles apart cannot be connected by the chain (standard chain range is 1 tile). An opponent who always moves units at 2-tile spacing prevents multi-hop chains.

*Counter-counter:* Arc Mastery passive (+20 pts) and Judgment Arc (Signature) extend chain range to 2 tiles per hop, eliminating the safety of 2-tile spacing.

B. **Retreat off WET terrain:** Units that move off WET tiles before the Electromancer activates lose their WET status when they step onto dry terrain. The blind-turn system means the opponent must predict the Electromancer will fire BEFORE the turn locks in — a judgment call, not a reaction.

*Counter-counter:* Hydromancer Flood Zone creates WET directly on units, not just terrain. A unit WET from Aqua Lance is WET regardless of which tile they stand on (WET is a unit status, not purely terrain-derived). Moving to a dry tile removes the FLOODED terrain WET, but the unit-applied WET from Aqua Lance persists for its duration. Only time expiry or a water-adjacent cleanse removes unit-applied WET.

C. **Target the Electromancer:** No Electromancer = no chain. The Electromancer (90 HP, 1 armor) is a priority kill target in any WET-network team. Opponent's fastest/hardest-hitting Mancer should always be evaluating the Electromancer's HP.

*Counter-counter:* Electromancer's 4-tile move range (highest among offensive Mancers outside Aeromancer) and 6-tile spell range allow safe positioning. Conductor's Range upgrade extends to 7 tiles. Keeping the Electromancer at maximum range and behind allied formation is the primary defensive play.

D. **Deny WET — keep units out of Hydromancer range:** A player who permanently keeps their formation outside Hydromancer's 6-tile Aqua Lance range and 4-tile Flood Zone range prevents WET application. This requires map-wide formation discipline and restricts the opponent's warband to slow approach paths.

*Counter-counter:* Difficult to maintain across a full match on most maps. The Hydromancer has 4-tile move range and its Flood Zone has 4-tile cast range — it can reach most engagement zones. Flood Zone's 3-tile radius plus the Hydromancer's mobility makes denying WET application indefinitely nearly impossible unless the opponent does not contest objectives.

---

### Combo 2 — Arc Firestorm (Electromancer + Pyromancer)

**Mancers involved:** Electromancer + Pyromancer

**Execution:**
1. Pyromancer applies BURNING to a target (Ember Shot; target takes 18 damage + BURNING; tile becomes ON_FIRE).
2. Electromancer fires Arc Bolt at the BURNING target.
3. BURNING + Lightning = Firestorm Burst: Arc Bolt damage (18) + 1-tile AoE fire burst (20 Fire damage to adjacent units) + ON_FIRE spreads to burst tiles.

**Tactical outcome:** Two-Mancer combo that converts a single BURNING target into an AoE fire-burst source. The adjacent units take fire AoE and new ON_FIRE tiles spread. The Pyromancer's terrain investment from ON_FIRE + BURNING converts into bonus fire damage via the Electromancer's chain — without requiring WET terrain setup. A BURNING target hit by the Electromancer is the combo entry point; WET nearby units from prior Hydromancer play can be STUNNED by the same Arc Bolt if adjacent.

---

### Combo 3 — Freeze-Shatter Via Lightning (Electromancer + Cryomancer)

**Mancers involved:** Electromancer + Cryomancer

**Execution:**
1. Cryomancer applies FROZEN to a target.
2. Electromancer fires Chain Lightning at the FROZEN target.
3. SHATTER triggers: 25 × 2.5 = 62 HP. Ice shard spray in 1-tile radius deals 8 cold damage to adjacent units.
4. If the FROZEN target was also WET (simultaneously WET and FROZEN — possible if Hydromancer applied WET and Cryomancer applied FROZEN consecutively): SHATTER triggers AND chain arc fires from the FROZEN/SHATTERED unit's position.

**Tactical outcome:** The FROZEN SHATTER via Chain Lightning is noteworthy because Chain Lightning's 3 AP cost and 6-tile range means the Electromancer can confirm a SHATTER kill from long range without the Geomancer needing Rock Throw proximity. The chain arc component on a WET+FROZEN target is the combo's peak: simultaneous SHATTER + chain stun on adjacent WET units = one spell delivering both the kill-confirmation AND the multi-unit chain stun setup.

---

### Combo 4 — CHARGED Trap Network (Electromancer solo / Electromancer + Hydromancer)

**Mancers involved:** Electromancer (solo setup) or Electromancer + Hydromancer

**Execution:**
1. Electromancer pre-stages multiple Static Field casts (2 AP each) across predicted enemy movement paths over 2–3 turns. Result: 3 CHARGED tiles distributed across the board.
2. Hydromancer (if available) casts Flood Zone over a CHARGED area — the water distributes the charge through the FLOODED zone (Static Field on FLOODED = entire zone CHARGED).
3. Electromancer waits until the opponent's units move onto CHARGED tiles.
4. On the turn the opponent's units trigger CHARGED tiles: if the unit is also WET (from Hydromancer contamination), the CHARGED arc fires AND chains to adjacent WET units. Multiple CHARGED tiles triggering the same turn create simultaneous arc discharges across the board.
5. Electromancer casts Overload (5 AP) consuming all 3 pre-staged CHARGED tiles simultaneously: 30 AoE damage + consumed CHARGED bonus (12 HP each × 3 = 36 bonus HP) + STUN on WET units in AoE.

**Tactical outcome:** The CHARGED Trap Network is the Electromancer's long-game setup play — trading AP over multiple turns for a deferred burst that the opponent walks into rather than being aimed at. Overload consuming 3 CHARGED tiles deals a total of 30 + 36 = 66 HP AoE burst in one activation — the highest non-setup AoE burst available to the Electromancer.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Electromancer

| Mancer | Counter Mechanism |
|---|---|
| **Geomancer** | Rock Throw on WET tiles converts them to MUD (WET + Earth = MUD). MUD is still partially conductive (chain arc range –1 tile through MUD) but significantly less effective than WET terrain. A Geomancer that actively converts WET terrain to MUD whenever the Electromancer's team tries to build a WET network degrades the chain arc's reach and STUN coverage. Additionally, OBSIDIAN tiles reflect arc bolts — the Geomancer's permanent walls can redirect Electromancer projectiles if placed correctly. |
| **Aeromancer** | Cyclone Blast scatters WET unit clusters radially — breaking the adjacency that chain arcs require. An Aeromancer that disperses WET units before the Electromancer fires reduces the chain to single-target. UPDRAFT also grants WEIGHTLESS to allied units, making them immune to WET terrain contact (they float above the FLOODED surface and don't pick up the WET status from terrain). |
| **Psychomancer** | CHARMED Electromancer — the worst case. An opponent controlling the Electromancer for 1 turn can fire Chain Lightning into a WET allied formation, STUNNING the Electromancer's own team. The Electromancer's chain arc is a team resource; Psychomancer's Charm converts it into a team liability. CONFUSED Electromancer randomly targets — random targeting could hit WET allies as readily as WET enemies. |

### Terrain Compositions That Shut Electromancer Down

| Terrain Setup | Why It's Punishing |
|---|---|
| **No WET terrain on the board** | The Electromancer becomes a single-target damage dealer. Arc Bolt at 18 HP for 2 AP is functional but not exceptional. Chain Lightning at 25 HP for 3 AP is inferior in AP efficiency to most other Mancers' standard spells. Zero chain arcs means zero STUN windows. |
| **All-MUD board (Geomancer counter)** | MUD reduces chain arc range by 1 tile per hop through MUD terrain. A board where all WET terrain has been converted to MUD by enemy Geomancer significantly limits chain propagation. |
| **OBSIDIAN corridors** | Arc Bolt reflection off OBSIDIAN can deflect shots into unintended directions, potentially hitting allies. Obsidian-heavy maps also break the connected terrain that FLOODED zones need for chain conductivity. |

### Warband Compositions That Prey on Electromancer

| Warband Type | Exploitation |
|---|---|
| **Aeromancer + WET-immune team (WEIGHTLESS)** | UPDRAFT over the opponent's formation prevents WET terrain contamination (WEIGHTLESS = immune to WET terrain contact). The Electromancer's Aqua Lance-applied WET (unit-applied) still works, but terrain-contact WET from FLOODED zones is negated. Chain scale is dramatically reduced. |
| **Geomancer + MUD conversion** | Systematic conversion of WET to MUD degrades chain range. A dedicated anti-chain Geomancer build (Earth Mastery passive for range + Rock Throw spam on WET tiles) can prevent WET networks from forming. |
| **Psychomancer-first priority** | CHARM the Electromancer when WET friends are nearby. The Electromancer's chain arc is the highest-value Charm target in the game — no other Mancer's abilities are as catastrophically self-harmful when Charmed. An opponent who opens with Psychomancer targeting the Electromancer forces the Electromancer's player to keep allies non-adjacent, limiting chain formation. |
| **Spread formation (no adjacent units ever)** | A disciplined opponent who permanently keeps all their units 2+ tiles apart prevents multi-hop chains. This requires significant formation discipline and reduces the opponent's own synergy potential, but it is a valid counter-strategy. The trade-off: spread formations are more vulnerable to AoE spells (Scorched Earth, Blizzard Field) and to individual targeting — the opponent sacrifices formation benefits to deny the chain. |

---

## 10. Temperature Interaction Notes

Lightning is a secondary heating element — not as hot as fire, but consistent and multi-target through chain arcs. Every Electromancer spell applies +10 to +15 temperature to each target hit, including every unit in a chain propagation. Over several turns of Static Field zone exposure or repeated Arc Bolt casts, the Electromancer can meaningfully push enemies toward HOT (+31) without a Pyromancer present.

### Electromancer as Secondary Heater

Electromancer can slowly push multiple enemies toward HOT over several turns via Static Field. A unit that starts a turn in a Static Field zone takes +10 temperature per tick. Combined with a Pyromancer's direct heating spells, the Electromancer can help reach OVERHEATED (+61) faster than either Mancer could alone. Even without a Pyromancer, three turns of Static Field exposure on a neutral unit pushes them to +30 (WARM) — meaning fire spells from any source deal +10% bonus damage against them from that point forward. This passive heating is a meaningful team contribution even when the Electromancer is not firing chain arcs.

### WET + LIGHTNING + TEMPERATURE

The existing wet-chain-arc combo now carries a temperature dimension that was not present before the temperature system. When an Electromancer chains through a WET network, every unit in the chain receives +10 temperature in addition to the arc damage and STUN. If enemies in that WET network were already at WARM (+15) from prior Static Field exposure or Pyromancer terrain, the chain arc pushes them to HOT (+25). A unit that reaches HOT after being STUNNED cannot move in their next turn due to STUN — and when the STUN expires, they are SLOWED from HOT (+31 to +60 = -1 move range).

This creates a compounding sequence: Hydromancer WETs → Electromancer chains (STUN + HOT) → enemy activates STUNNED (skip turn) → enemy recovers into SLOWED (HOT). Two turns of action denial from a single chain arc.

### SUPERCOOLED Enemy + Lightning Anti-Synergy

An enemy at -40 SUPERCOOLED (SLOWED + BRITTLE modifier, +50% physical damage taken) hit by Chain Lightning (+10 temperature) moves to -30 — just above the SUPERCOOLED threshold (-31). This actually removes the SLOWED and BRITTLE modifiers that SUPERCOOLED provides. The enemy is now at COLD (-30) rather than SUPERCOOLED (-40), and the Geomancer or Osteomancer who was planning to deliver a BRITTLE physical finisher loses that multiplier.

**Anti-synergy ruling:** Do not use Electromancer spells on an enemy a Cryomancer is actively freezing, unless the intent is specifically to halt their temperature descent at SUPERCOOLED/COLD rather than drive them to FROZEN SOLID. If a Cryomancer has pushed an enemy to -50 and is planning to finish the freeze next turn, an Electromancer Chain Lightning for +10 temperature moves the target from -50 to -40 — still SUPERCOOLED, damage done, but the freeze progress is set back one effective Cryomancer cast. Communicate freeze targets with allied Cryomancers before using lightning on cold enemies.

*End of Electromancer design document.*

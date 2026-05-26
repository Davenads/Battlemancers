# Osteomancer — Full Design Document

---

## 1. Tactical Identity

The Osteomancer is the roster's primary tank and structural engineer — a Mancer that fights by erecting physical obstacles from conjured bone and hardening allies against incoming damage while simultaneously making enemies brittle enough that a single well-timed hit becomes catastrophic. Unlike most Mancers who win by optimizing their own offense, the Osteomancer wins by changing the rules of the engagement zone: what terrain the opponent can cross, what their attacks are worth, and how long their formations hold together under the sustained punishment of bone constructs blocking every angle.

The Osteomancer's central identity is the tension between its two kit modes. In fortify mode, it raises Bone Spires and Bone Walls to channel enemy movement, applies BONE_ARMOR to allies for flat damage absorption, and positions itself as a damage sponge that outlasts the opponent's burst. In brittle mode, it debuffs enemies with the BRITTLE debuff — making incoming physical damage 50% higher — and coordinates devastating single-hit kills with allied Mancers who can deliver that physical damage while BRITTLE is active. A FROZEN + BRITTLE enemy is the most fragile unit state in the game, and the Osteomancer is the only Mancer that can create BRITTLE while Cryomancer creates FROZEN for the SHATTER interaction.

The Osteomancer has above-average HP and is the tankiest offensive Mancer on the roster. It can absorb several hits that would eliminate other Mancers, and this survivability is an active resource — the Osteomancer stays alive to maintain construct pressure and refresh BONE_ARMOR on wounded allies. Its damage output is moderate: bone constructs are terrain objects that block and occupy without directly dealing damage each turn, and the Osteomancer's direct spells deal solid but not exceptional numbers. The Osteomancer wins through sustained presence, not burst peaks.

**Primary win condition:** The Osteomancer wins by establishing a construct network that channels the opponent into kill zones while applying BRITTLE to high-priority targets for allied Mancers to detonate. In a long engagement, 2-3 active bone constructs occupying chokepoints plus a BRITTLE-debuffed enemy Mancer represents a tactical stranglehold — the opponent cannot advance through the constructs efficiently and cannot afford to ignore the BRITTLE detonation threat. The Osteomancer team wins the turn that a BRITTLE + FROZEN enemy is hit by a physical spell for ×2.5 damage. SHATTER kill of a Mancer is the peak combo.

**Core weakness:** Bone constructs are terrain objects — they are not active units and cannot reposition. A construct placed incorrectly is a wasted AP investment that the opponent simply routes around. The Osteomancer has no pursuit tools and no ability to project direct damage beyond 5 tiles reliably. An opponent who keeps mobile and avoids the construct-lined corridors entirely can deny the Osteomancer's terrain investments indefinitely. Additionally, Pyromancer fire converts OVERGROWTH to ON_FIRE but does not destroy Bone Spires and Bone Walls (bone resists fire more than organic matter), however Sonimancer sonic attacks can SHATTER bone constructs — the Sonimancer is the Osteomancer's hard counter at the construct level. Finally, the Osteomancer has the worst single-activation AP efficiency when it spends primarily on constructs — a 4-AP Bone Spire placement is half the turn's budget on a tile object that cannot fight.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 120 | Highest HP in the offensive Mancer tier; most durable non-dedicated-support Mancer |
| **Move Range** | 3 tiles per activation | Slow; designed to anchor near its own constructs |
| **Base Armor** | 2 | Above-average; takes meaningful damage mitigation from all physical sources |
| **Spell Range** | 5 tiles (base) | Each spell lists its own range; this is the fallback reference |
| **AP per Activation** | 6 | Standard for all Mancers; movement costs 1 AP per tile |
| **Element** | Bone/Physical | All base spells deal physical damage or apply structural/bone-state interactions |

**AP budget example:** With 6 AP, the Osteomancer can move 2 tiles (2 AP), place a Bone Spire (4 AP) with no remaining AP for spells — or move 1 tile (1 AP), cast Bone Shard (2 AP), cast Calcify (2 AP), and cast Brittle Touch (1 AP) for a spell-heavy activation without any terrain placement. The construct/combat tradeoff is the primary AP-allocation decision every activation.

---

## 3. Base Spell Kit

The Osteomancer's four base spells cover distinct combat functions:
- **Bone Shard** — repeatable physical projectile damage with terrain generation
- **Calcify** — ally buff (BONE_ARMOR) or enemy debuff (BRITTLE) depending on targeting
- **Brittle Touch** — melee BRITTLE applicator with guaranteed direct contact
- **Bone Spire** — primary terrain feature placement; construct creation

---

### Bone Constructs — Core System

Bone constructs are terrain objects placed by the Osteomancer. They are **not units** — they do not take turns, have no AI, cannot move, and do not consume the activation budget. They occupy tiles, block movement and LOS, and persist until destroyed or removed by the Osteomancer.

**Construct types:**

**Bone Spire:**
- Appearance: a 2-tile-tall column of fused bone rising from the ground
- Properties: blocks LOS (units cannot target through a Bone Spire); blocks movement (impassable tile); grants +1 physical armor to units standing adjacent (the jagged bone edges provide cover)
- HP: 40 (can be targeted and destroyed by spells dealing physical, sonic, or magical damage)
- Destroyed by: Sonimancer sonic attacks (bone resonance shatters; takes full damage + no cover vs. sonic), Gravimancer crushing spells (physical crush), Pyromancer fire (reduced effectiveness — bone chars but requires multiple hits; takes 50% of fire spell damage)
- Placement cost: 4 AP (see Bone Spire Placement spell below)
- Maximum simultaneous constructs: 2 (increases to 3 with Skeleton Crew upgrade)

**Bone Wall:**
- Appearance: a low wall of layered bones spanning 3 tiles in a line
- Properties: blocks LOS and movement identically to Bone Spire but covers 3 tiles simultaneously with a single placement; height equivalent to Bone Spire (2-tile-tall) — cannot be leaped by standard movement
- HP: 30 per tile (3 tiles, 30 HP each = 90 HP total to destroy a full Bone Wall)
- Destroyed by: destroying one tile of a Bone Wall destroys only that segment; the remaining two segments persist independently
- Placement cost: 5 AP (the full Bone Wall is placed in one cast but costs more than a single Bone Spire)
- Shares the construct cap with Bone Spires: 2 constructs maximum (Bone Wall counts as 1 construct regardless of its 3-tile span)

**Ossified Ground:**
- Appearance: patches of bone-reinforced terrain — bone fragments jutting through the ground surface
- Properties: does NOT block movement or LOS; movement cost on Ossified Ground is 1.5 (moderate slow); units adjacent to Ossified Ground gain +1 physical armor passively (the bone fragments serve as debris cover)
- HP: N/A (terrain state, not a unit; persists 4 turns before the bone fragments degrade)
- Created by: Bone Shard spell impact (leaves Ossified Ground on hit tile); Bone Spire destruction (the shattered spire leaves Ossified Ground in a 1-tile radius); cannot be directly placed
- Does not count toward construct cap (it is a terrain state, not a placed construct)

---

### BRITTLE ARMOR Mechanic — Full Rules

The Osteomancer can interact with armor in two directions:

**BONE_ARMOR (ally buff):**
- Applied via Calcify (allied targeting) or Fortified Bones upgrade
- Grants the target a flat damage absorption shield (temporary HP): incoming physical damage is reduced by the shield amount before HP loss; when the shield is depleted, it is removed and normal armor applies
- The shield is NOT regenerating — it absorbs a fixed amount and then is consumed
- Example: BONE_ARMOR (20 shield) reduces incoming physical hits by 20 total. A 30-damage hit becomes a 10-damage hit; the shield is now at 0 and removed.
- BONE_ARMOR does NOT protect against elemental damage (fire, lightning, ice, poison) — only physical damage

**BRITTLE debuff (enemy debuff):**
- Applied via Calcify (enemy targeting) or Brittle Touch (melee)
- Increases incoming physical damage to the target by 50% for its duration
- Duration: until 2 physical hits are absorbed (BRITTLE crumbles after 2 hits — whether the hits are from spells, constructs, or physical attacks)
- Example: a 20-damage physical hit against a BRITTLE unit deals 30 damage (20 × 1.5). After 2 such hits, BRITTLE is removed.
- BRITTLE applies only to physical damage — elemental spells (fire, lightning, ice, poison, necrotic, sonic) deal normal damage to BRITTLE units

**SHATTER combo (BRITTLE + FROZEN):**
When a unit is simultaneously BRITTLE and FROZEN:
- The first physical damage hit after FROZEN is applied triggers SHATTER
- SHATTER damage: base physical damage × 1.5 (BRITTLE) × 2.5 (FROZEN SHATTER multiplier) = effective ×3.75 total multiplier
- Example: a 20-damage physical hit on a BRITTLE FROZEN unit = 20 × 3.75 = 75 damage
- SHATTER removes both BRITTLE and FROZEN simultaneously (both are consumed on the SHATTER hit)
- SHATTER is the highest single-hit damage multiplier in the game; the Osteomancer is the primary BRITTLE source; Cryomancer is the primary FROZEN source

---

### Spell 1: Bone Shard

| Field | Value |
|---|---|
| **Name** | Bone Shard |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (projectile; travels in line; can hit intervening units) |
| **Range** | 5 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 14 (physical; benefits from BRITTLE × 1.5) |
| **Element** | Physical/Bone |
| **Effects Applied** | Deals 14 physical damage. Tile beneath target becomes `OSSIFIED_GROUND` (4-turn duration; movement cost 1.5; +1 adjacent armor to nearby units). If the target is BRITTLE: deals 21 damage (14 × 1.5) and BRITTLE absorbs one hit (1 of 2 BRITTLE charges consumed). If the target is FROZEN: deals 35 damage (14 × 2.5 SHATTER multiplier; FROZEN removed; SHATTER triggers). If the target is both BRITTLE and FROZEN: deals 52 damage (14 × 3.75 SHATTER) and both BRITTLE and FROZEN are removed. |
| **Terrain Interaction** | Hitting BONE_SPIRE: bone-on-bone resonance — the Spire takes 7 damage (50% of Bone Shard's base; physical hitting bone construct). Hitting OSSIFIED_GROUND tile (target on existing Ossified): extra bone fragments erupt — unit takes +4 bonus damage. Hitting FROZEN unit: SHATTER triggers as above. Hitting WET terrain: Ossified Ground formed on WET tile — bone on wet ground; Ossified Ground has reduced duration (2 turns instead of 4). |

**Design note:** Bone Shard is the Osteomancer's workhorse damage spell — repeatable, no cooldown, and capable of dealing 52 damage in the BRITTLE + FROZEN SHATTER scenario. Against un-BRITTLE targets it is a steady 14-damage poke, comparable to other 2-AP Quick spells. Its primary value is threefold: generating Ossified Ground terrain for the armor bonus, consuming BRITTLE charges to deal amplified damage, and serving as the SHATTER delivery vehicle when Cryomancer has established FROZEN. Two Bone Shards per activation (2 + 2 = 4 AP) plus 2 AP movement is a standard activation pattern.

---

### Spell 2: Calcify

| Field | Value |
|---|---|
| **Name** | Calcify |
| **AP Cost** | 2 AP |
| **Cooldown** | 1 turn |
| **Targeting Type** | Single Target (targeted status; LOS required; can target ally or enemy) |
| **Range** | 4 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 0 (status application) |
| **Element** | Physical/Bone |
| **Effects Applied** | **If targeting an ally (including self):** Applies `BONE_ARMOR` — a shield absorbing 15 flat physical damage (shield depletes before HP loss; refreshes to 15 if re-cast while still active). Duration: until depleted or 4 turns maximum. **If targeting an enemy:** Applies `BRITTLE` — incoming physical damage +50% for the next 2 physical hits. Also applies `CALCIFIED` status (per status-effects.md: movement –2, physical armor +15% as a double-edged effect; 2-turn duration). Duration: 2 turns or until 2 BRITTLE triggers resolve, whichever comes first. |
| **Terrain Interaction** | Casting on self while standing on OSSIFIED_GROUND: BONE_ARMOR shield value increases to 20 (the bone fragments reinforce the armor). Casting BRITTLE on an enemy standing on ICE_TILE: BRITTLE + FROZEN prerequisite — if a Cryomancer freezes this BRITTLE unit next, SHATTER triggers. Casting on an ally standing in NECROTIC_ASH: the BONE_ARMOR shield has no interaction with necrotic terrain — the ally still takes necrotic DoT but their physical damage is absorbed. |

**Design note:** Calcify is the Osteomancer's multi-function utility spell. Its dual targeting nature — ally armor buff or enemy brittle debuff — makes every cast a decision: does the team need damage reduction right now (protect a low-HP ally) or does the next turn's damage spike need amplification (brittle a key enemy). At 2 AP with 1-turn cooldown, it can be used every other activation. The CALCIFIED status applied alongside BRITTLE is double-edged deliberately: CALCIFIED enemies are slower (easier to catch and re-apply stacks to) but also tankier (the +15% physical armor partially offsets the BRITTLE +50% physical vulnerability). Net effect: BRITTLE still wins by a wide margin (+50% vs. +15% mitigation = net +35% physical damage).

---

### Spell 3: Brittle Touch

| Field | Value |
|---|---|
| **Name** | Brittle Touch |
| **AP Cost** | 1 AP |
| **Cooldown** | 1 turn |
| **Targeting Type** | Single Target (melee contact; 1-tile range; requires adjacency) |
| **Range** | 1 tile |
| **AoE Radius** | N/A |
| **Base Damage** | 8 (physical; a bone-spike protrusion from the Osteomancer's hand) |
| **Element** | Physical/Bone |
| **Effects Applied** | Deals 8 physical damage. Applies `BRITTLE` immediately (no CALCIFIED; this is a pure brittle application). BRITTLE from Brittle Touch lasts for 2 physical hits OR 3 turns (slightly longer duration than Calcify's BRITTLE — the direct skeletal injection makes it more persistent). |
| **Terrain Interaction** | If used on a unit standing on OSSIFIED_GROUND: the bone fragments on the ground channel additional bone energy into the Brittle Touch — applies BRITTLE for 3 physical hits instead of 2 (3-hit BRITTLE). If used on a FROZEN unit: Brittle Touch is a physical hit — SHATTER triggers immediately on the touch (8 × 2.5 = 20 damage from SHATTER; FROZEN removed), then BRITTLE is also applied to the now-unfrozen unit. |

**Design note:** Brittle Touch is the cheapest BRITTLE application — 1 AP for guaranteed BRITTLE at melee range. Its low cost means it can be used in the same activation as Calcify (2 AP Calcify on an ally + 1 AP Brittle Touch on an adjacent enemy + 1 AP movement + 2 AP Bone Shard = 6 AP: full-turn efficient). The melee requirement limits it to scenarios where the Osteomancer is already adjacent to an enemy — which, given the Osteomancer's slow 3-tile move range, usually means the enemy has advanced to melee range (not ideal) or the Osteomancer has closed deliberately for a high-value BRITTLE application (the target is worth the risk).

---

### Spell 4: Bone Spire Placement

| Field | Value |
|---|---|
| **Name** | Bone Spire |
| **AP Cost** | 4 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Ground Target — places a Bone Spire construct on target tile |
| **Range** | 3 tiles (must be placed close; the Osteomancer physically raises it from nearby) |
| **AoE Radius** | N/A |
| **Base Damage** | 6 (erupting bone spike damages unit on target tile at placement) |
| **Element** | Physical/Bone |
| **Effects Applied** | Places a `BONE_SPIRE` construct on the target tile (see Bone Constructs section for full properties). If a unit is on the target tile at placement, they take 6 damage and are displaced 1 tile away from the Spire (the bone eruption pushes them back). Units adjacent to the placed Spire gain +1 physical armor immediately (cover bonus activates on placement). Cannot place on OBSIDIAN, FLOODED, or existing construct tiles. |
| **Terrain Interaction** | Placing on WET terrain: the Spire roots in wet ground but has reduced HP (30 instead of 40) — moisture weakens the bone fusion. Placing on TOXIC_TERRAIN: the Spire is infused with toxin — bone is tainted; units adjacent to the TOXIC Bone Spire take 1 POISONED stack per turn from proximity (the toxin seeps through the bone into the air). Placing on ON_FIRE: the Spire chars — it has full HP but also emits heat; units moving adjacent to BURNING Bone Spire take 3 fire damage per tile entered adjacent. Placing on MUD: the Spire sinks slightly — reduced LOS block (1-tile height instead of 2) but gains additional HP (50 instead of 40) from reinforced ground anchoring. Placing on ELEVATED tile: full 2-tile height becomes 3-tile effective (the elevated terrain adds to the Spire height), extending its LOS block and granting +2 physical armor to adjacent units. |

**Design note:** Bone Spire is the Osteomancer's primary terrain investment. At 4 AP and a 2-turn cooldown, it is not spammable but is deployable every 2-3 activations while still casting other spells. Two Bone Spires side by side create a 2-tile impassable bone wall with full LOS block — effectively a narrow chokepoint the opponent must route around or destroy. Positioning Bone Spires to channel enemy movement toward the Osteomancer's allied Mancers (or toward TOXIC_TERRAIN, CHARGED tiles, or other hazards) is the core spatial skill of Osteomancer play.

**Note — Bone Wall placement:** Placing a Bone Wall (3-tile line) follows the same rules as Bone Spire but costs 5 AP and has a 3-turn cooldown. Bone Wall behaves as described in the Construct System above. Bone Wall placement is represented by this same spell with the "Wall" variant selected at cast — the UI presents a choice of Spire or Wall when the spell is activated. Both share the construct cap.

---

## 4. Terrain Interaction Table

### Physical/Bone Spell Impact on Existing Terrain States

| Existing Terrain State | What Happens When Bone Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **GROUND (normal)** | Bone fragments scatter on impact | `OSSIFIED_GROUND` (4-turn duration; 1.5 movement cost; adjacent +1 armor) | Takes spell damage; BRITTLE/BONE_ARMOR applied per spell | Standard interaction; Ossified Ground is a secondary terrain benefit from most bone spells |
| **OSSIFIED_GROUND** | More bone fragments pile on | `OSSIFIED_GROUND` refreshed (duration reset to 4 turns); cover bonus active | Unit on Ossified Ground takes +4 bonus physical damage from bone spike concentration | Refreshing Ossified Ground maintains the armor bonus longer; a common Osteomancer sustained-play pattern |
| **WET** | Bone fragments dissolve in water; Ossified Ground fails to form | Tile remains WET; no Ossified Ground created | Takes spell damage normally; no terrain change | Osteomancer cannot seed Ossified Ground on WET tiles; Hydromancer water negates the terrain-building benefit |
| **ON_FIRE** | Bone chars but does not burn; fire state maintained | ON_FIRE remains; Ossified Ground chars beneath (CHAR_GROUND: dark but functional; no meaningful game effect beyond aesthetics) | Takes spell damage; BRITTLE applied if Calcify; BURNING if already burning | Bone constructs take 50% fire damage (fire-resistant); Ossified Ground on ON_FIRE behaves as GROUND |
| **ICE_TILE** | Bone fragments freeze into the ice — reinforcement | `OSSIFIED_ICE` (Ossified Ground properties + ice movement; units take 1 bonus physical damage per tile; 3-turn duration) | Takes spell damage; if FROZEN: SHATTER triggers | OSSIFIED_ICE is a unique combined state that provides armor bonus (adjacent cover) while maintaining ice movement effects |
| **FROZEN (unit status)** | Physical contact triggers SHATTER | Tile unchanged | Takes full SHATTER damage (×2.5 base; ×3.75 if BRITTLE also active); FROZEN removed | The Osteomancer's primary combo delivery; Bone Shard on FROZEN = reliable high damage |
| **BRITTLE (unit status)** | Physical hit consumes one BRITTLE charge | Tile unchanged | Takes ×1.5 physical damage; BRITTLE charge count reduced by 1 (removed at 0 charges) | Each Bone Shard against BRITTLE consumes one of the 2 charges; second Bone Shard in same activation consumes both and clears BRITTLE |
| **TOXIC_TERRAIN** | Bone fragments absorb toxin | `OSSIFIED_GROUND` + TOXIC_RESIDUE (combined: units on the tile take 1 POISONED stack per 2 turns + movement cost 1.5) | Takes spell damage + 1 POISONED stack | Osteomancer + Toximancer terrain: Ossified Ground and TOXIC_TERRAIN layer into a slow + poison zone |
| **CHARGED** | Bone conducts electricity moderately | CHARGED arc fires — bone is a partial conductor; arc deals 60% of normal chain damage (reduced but present); CHARGED tile consumed | Takes spell damage + 10 Lightning from partial bone-conducted arc | Bone constructs on CHARGED tiles take arc damage as well as units; Bone Spire HP drops from arc |
| **MUD** | Bone fragments do not improve mud traction | MUD remains; Ossified Ground fails to form on MUD | Takes spell damage | MUD tiles cannot become Ossified Ground; Geomancer MUD + Osteomancer are territorially incompatible for terrain stacking |
| **OBSIDIAN** | Bone fractures against obsidian | OBSIDIAN unchanged; Bone Shard ricochets — projectile may hit secondary target if geometry allows | Takes spell damage | Bone constructs cannot be placed on OBSIDIAN; direct spell projectiles may ricochet off obsidian at adjacent targets |
| **NECROTIC_ASH** | Bone absorbs necrotic energy; ossified bone becomes necrotic | `NECROTIC_OSSIFIED` terrain (OSSIFIED_GROUND properties + 2 Necrotic damage/turn to living units; undead ignore) | Takes spell damage + 2 Necrotic from terrain absorption | Necromancer + Osteomancer terrain interaction; undead constructs (if Necromancer reanimates bone somehow) would be immune |
| **STEAM_CLOUD** | Bone fragments obscured in steam | STEAM_CLOUD remains; Ossified Ground may form beneath (cannot see it from outside cloud) | Takes spell damage; BRITTLE application still fires through steam | Osteomancer can "blind-fire" Calcify/Bone Shard into STEAM_CLOUD — spell effects apply even inside cloud |

### Terrain States Beneficial to the Osteomancer

| State | Benefit |
|---|---|
| `OSSIFIED_GROUND` | Osteomancer's own terrain; it and adjacent allies gain +1 armor; movement cost 1.5 slows enemies trying to advance; Bone Spire on OSSIFIED gets enhanced HP if on elevated variant |
| `ICE_TILE` / `FROZEN` | FROZEN units are SHATTER-vulnerable; Osteomancer is the second half of the SHATTER combo after Cryomancer |
| `ELEVATED` tiles | +1 to all spell ranges; +1 to Bone Spire effective height; a Bone Spire on ELEVATED has range 6 for Bone Shard and covers higher LOS angles |
| `BRITTLE` (active on enemy) | Every Bone Shard hits for 21 instead of 14; SHATTER on a BRITTLE FROZEN unit is 52 damage from a single 2-AP spell |

### Terrain States Hazardous to the Osteomancer

| State | Hazard |
|---|---|
| `FLOODED` / `WET` | Cannot place Bone Spires reliably (Wet Spire: 30 HP instead of 40); Ossified Ground cannot form on WET; Hydromancer flooding negates Osteomancer terrain building |
| `ON_FIRE` | Bone constructs take 50% fire damage (partial fire vulnerability); even at 50%, a Pyromancer Pillar of Flame (55 damage, 50% = 27 to construct) can eliminate a Bone Spire in 1-2 casts |
| `SONIC` attacks (Sonimancer) | Sonimancer sonic attacks deal full damage to Bone Spires with no resistance reduction (bone resonance vulnerability); a single Sonimancer heavy spell can destroy a Bone Spire outright; Osteomancer + Sonimancer opponent is the hardest counter to construct-heavy play |
| `NECROTIC_ASH` | Osteomancer takes necrotic DoT damage on NECROTIC_ASH terrain like any other living Mancer; its 120 HP makes it more survivable than most, but it must exit necrotic terrain to avoid sustained damage |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

#### Variant A: Osseous Volley (replaces Bone Shard) — +20 pts

**Description:** Replaces Bone Shard with a multi-projectile burst. Osseous Volley fires 3 bone shards simultaneously at a single target — each deals 8 damage (3 × 8 = 24 total) and the three hits count as 3 separate BRITTLE triggers. Against a BRITTLE target: all 3 BRITTLE charges are consumed in one cast if using the 3-hit Brittle Touch upgrade variant; against standard 2-hit BRITTLE, the first 2 shards clear BRITTLE (2 × 12 = 24 from BRITTLE) and the third hits normally (8 damage). AP cost: 3 AP; cooldown: 1 turn.

**Trade-off:** Higher total damage against BRITTLE targets (24 base vs. Bone Shard's 14; same burst potential via BRITTLE + FROZEN on all 3 hits if SHATTER triggers on first hit). Higher AP cost and cooldown. Best in dedicated BRITTLE-detonation builds where the Osteomancer exists to consume BRITTLE charges rapidly.

#### Variant B: Living Wall (replaces Bone Spire Placement) — +25 pts

**Description:** Replaces Bone Spire and Bone Wall placement with a hybrid animated construct. The Living Bone Wall is a 3-tile construct (like a Bone Wall) with 50 HP per tile — but it can slowly reposition. Once per Osteomancer activation, as a free action (no AP cost), the Living Wall may shift one tile in any direction (the entire wall moves as a unit, maintaining its 3-tile line configuration). This allows the Osteomancer to gradually redirect a barrier toward incoming enemies rather than committing to a fixed position. AP cost to initially place: 5 AP; cooldown: 3 turns.

**Trade-off:** Far more flexible than a static Bone Wall but costs the same AP and cooldown. The free-action repositioning means it always needs to be tracked — the opponent must account for a barrier that can move. Best for fluid engagements where the Osteomancer does not know which approach path the opponent will use.

---

### Passive Traits

#### Passive A: Fortified Bones — +20 pts

**Description:** The Osteomancer's own Max HP increases by 15 (135 total), and it passively generates `BONE_ARMOR` at the start of each activation: a 5-HP shield (in addition to any Calcify-applied BONE_ARMOR). The passive shield stacks additively with Calcify-applied BONE_ARMOR — a Calcify BONE_ARMOR (15) + Fortified Bones (5) = 20-HP shield total. The passive 5-HP shield regenerates each activation (if depleted, it resets to 5 at the start of the next activation). This upgrade defines the pure-tank Osteomancer build path: maximum survivability, enduring presence.

**Synergy note:** Fortified Bones makes the Osteomancer the hardest single unit to kill with physical damage in the game. 135 HP + 2 base armor + 5-HP regenerating shield + Calcify BONE_ARMOR = the equivalent of a 155+ effective HP unit against physical attacks. Against physical damage-focused warbands (Osteomancer mirror, Faunamancer beasts), this Osteomancer is functionally unkillable without elemental or high-burst spell damage.

#### Passive B: Skeleton Crew — +25 pts

**Description:** The maximum number of active Bone constructs increases from 2 to 3. A third construct slot is available. The third construct can be any combination of Bone Spire and Bone Wall (within the 3-total cap). With 3 active constructs, the Osteomancer can create full corridors: Bone Wall on one side + 2 Bone Spires flanking = a 5-tile-wide impassable barrier. The Bone Spire Placement cooldown does not change — gaining a third slot does not speed up construction, just raises the concurrent ceiling.

**Trade-off:** The Osteomancer must invest significantly more AP over more turns to fill three construct slots, requiring it to prioritize construct-building activations more frequently. In exchange, it can establish map-defining terrain networks that fundamentally redirect the entire board's movement for the Osteomancer's team. Best in long-engagement maps where there are 5+ turns to establish constructs before the decisive fight.

#### Passive C: Ossified Aura — +15 pts

**Description:** All allied units within 2 tiles of the Osteomancer gain +1 physical armor passively (the Osteomancer's bone-hardening aura extends to nearby allies). This stacks with BONE_ARMOR applied by Calcify but is a different mechanic: Calcify BONE_ARMOR absorbs a flat shield amount; Ossified Aura provides a consistent +1 armor reduction to every incoming physical hit. The aura is always active within 2 tiles — no AP cost, no duration.

**Trade-off:** Incentivizes tight formation play with the Osteomancer at center — all units within the aura range become meaningfully harder to kill. The Osteomancer positioned in a chokepoint with 2-3 Chaff units adjacent and Ossified Aura active is a difficult position to crack with physical-damage units.

#### Passive D: Shatter Expertise — +20 pts

**Description:** The Osteomancer's physical spells deal +10 bonus damage on SHATTER (in addition to the ×2.5 SHATTER multiplier). This is a flat post-multiplier bonus: damage = (base × SHATTER modifier) + 10. On a Bone Shard SHATTER against a BRITTLE FROZEN unit: (14 × 3.75) + 10 = 62.5 → 63 damage. On Osseous Volley (if taken) hitting SHATTER: each shard contributes (8 × 3.75) + 10 = 40 per shard; 3 shards = 120 total — the highest single-activation physical damage burst in the game.

**Synergy note:** Shatter Expertise combined with Cryomancer is the highest-peak physical damage combination in Battlemancers, surpassing Pyromancer single-target at maximum setup.

---

### Stat Enhancements

#### Enhancement A: Iron Marrow (+20 HP) — +15 pts

**Description:** Max HP increases from 120 to 140. The Osteomancer becomes the highest-HP Mancer in the entire roster. At 140 HP with 2 base armor, it can withstand sustained damage from most Mancers for 3-4 turns without reaching critical HP. Primarily valuable in matchups where the Osteomancer is the team anchor and must stay alive while allies deal with the opponent's high-burst threats.

#### Enhancement B: Long Bones (+1 Spell Range) — +15 pts

**Description:** All Osteomancer spell ranges increase by 1 tile. Bone Shard: 5 → 6. Calcify: 4 → 5. Brittle Touch: 1 → 2 (now reaches to adjacent-diagonal tiles, not just the 4 orthogonal adjacents). Bone Spire Placement: 3 → 4. The Bone Spire Placement range increase is particularly valuable — it allows the Osteomancer to seed constructs 4 tiles ahead of its position, establishing a forward barrier without having to advance into threat range to place it.

---

### Signature Ability

#### Signature: Bone Fortress — +40 pts

| Field | Value |
|---|---|
| **Name** | Bone Fortress |
| **AP Cost** | 6 AP (entire activation; Osteomancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Self-centered AoE — no targeting cursor; originates from the Osteomancer's position |
| **Range** | N/A (self-centered) |
| **AoE Radius** | 2 tiles (constructs placed within 2 tiles of Osteomancer) |
| **Base Damage** | 0 (terrain construction; no direct damage) |
| **Element** | Physical/Bone |
| **Effects Applied** | The Osteomancer raises a complete bone fortress ring. Simultaneously places Bone Spires on up to 4 tiles in the cardinal directions around the Osteomancer (North, South, East, West; each 1 tile away from the Osteomancer — the Osteomancer is at center). Additionally: the Osteomancer and all allies within 2 tiles receive BONE_ARMOR (20 HP shield). All enemies within 2 tiles at cast time take 12 physical damage and are pushed 2 tiles away from the Osteomancer (the bone eruption blasts them back). The Bone Spires created by Bone Fortress start with 50 HP each (10 more than standard Bone Spires). The construct cap is temporarily exceeded for the Bone Fortress cast only — all 4 spires are placed regardless of current construct count. At end of Bone Fortress turn, any constructs beyond the normal cap (2, or 3 with Skeleton Crew) must be removed by the Osteomancer player's choice (choose which to dismiss; the rest remain). |
| **Special Interactions** | If Skeleton Crew is also taken: the Osteomancer can retain all 4 Bone Fortress spires because the cap is 3, not 2 — but the 4th still exceeds the cap and must be dismissed. Wait — **ruling:** Skeleton Crew cap is 3; Bone Fortress places 4; with Skeleton Crew, the player chooses which 3 to keep. Without Skeleton Crew, keep 2. Against WET terrain: Bone Fortress Spires on WET tiles have 40 HP (standard WET Spire penalty applies even during Bone Fortress). Bone Fortress constructs can be targeted immediately on the opponent's next turn — the fortress is not invulnerable. The push effect (2 tiles from center) can shove enemies into hazardous terrain or off elevated positions. |

**Design note:** Bone Fortress is the Osteomancer's "anchor the position" ability — it converts a full activation into a fortified position that forces the opponent to spend several turns either routing around the fortress ring or systematically destroying 50-HP bone structures. Combined with the BONE_ARMOR application to all nearby allies and the push that clears the immediate area, it is simultaneously an offensive ability (push + damage), a defensive ability (BONE_ARMOR shield for allies), and a terrain construction ability (4 Bone Spires). The 5-turn cooldown and stationary activation mean it is a commitment: the Osteomancer telegraphs its intent, and the opponent who was already adjacent when Bone Fortress resolves has been pushed out of the zone. Bone Fortress planted at a key chokepoint on turn 3-4 of an engagement can define which areas are contested for the rest of the fight.

---

## 6. Faction Synergy

### Best Pairing: All Factions — Broad Viability

The Osteomancer is unusual in that it synergizes well with all three factions at roughly equal levels, with modest differences between them. Its core mechanics (BONE_ARMOR buffs, BRITTLE debuffs, construct placement) do not key off any single faction trait.

**Gilded Throne — Consistent and Efficient:**

| Mechanism | Effect |
|---|---|
| Osteomancer BONE_ARMOR on Iron Vanguard | Iron Vanguard already has high HP and Shield Wall formation bonus; BONE_ARMOR from Calcify adds another physical damage absorption layer on the most durable non-Mancer unit in the game. An Iron Vanguard in Shield Wall formation + BONE_ARMOR is extremely difficult to remove with physical damage |
| Siege Arbalest BRITTLE exploitation | Siege Arbalest fires every turn; every shot against a BRITTLE target deals ×1.5 physical damage. The Osteomancer applies BRITTLE while Siege Arbalests deliver sustained fire — 2 BRITTLE-amplified Arbalest shots equals significant bonus damage on key targets without the Osteomancer needing to personally deliver the BRITTLE detonation |
| Iron Discipline + BONE_ARMOR | Iron Discipline protects Throne infantry from Psychomancer disruption; BONE_ARMOR protects them from physical burst. Combined, Throne infantry with Osteomancer support is resistant to both psychological and physical damage types — the most durable infantry screen configuration |

**Verdant Pact — High Terrain Synergy:**

| Mechanism | Effect |
|---|---|
| Bone Spires as blocking for Floramancer vines | Bone Spires channel movement into vine corridors — the Floramancer's VINES ROOTED trap is far more effective when flanked by bone construct walls that prevent routing around them |
| Thornback Sentinel death Thorn Patch + Ossified Ground | Thorn Patches from dead Sentinels + Ossified Ground from Bone Shard = a multi-terrain-penalty ground state (movement cost + thorn damage + armor bonus for adjacent allies) |
| Osteomancer BONE_ARMOR on Rootwardens | Rootwarden entrenched in natural terrain + BONE_ARMOR from Calcify + CALCIFIED (from combat) = effectively immovable armor-heavy unit in the middle of the enemy approach. Rootwarden entrenching is normally a positional commitment; BONE_ARMOR makes the commitment significantly safer |

**Ashen Covenant — Thematic but Potentially Competing:**

Bone constructs and Necromancer corpse economies do not directly interact — bone constructs do not generate corpses and are not affected by Necrotic spells (bone already has necrotic affinity, so NECROTIC_ASH deals partial damage to constructs at 50% of standard rather than full — a minor defensive bonus). Grave Husks advancing through an Osteomancer construct-channeled corridor benefit from the +1 physical armor cover adjacent to Bone Spires — the Husks are slightly harder to range-fire down when they advance through bone-lined chokepoints.

The primary Ashen Covenant synergy is structural: the Osteomancer creates the chokepoints; Grave Husks die in those chokepoints to generate Remnant tokens for the Necromancer; the Necromancer raises more units. The Osteomancer is the physical architect of the arena in which the Necromancer's economy runs.

---

## 7. Combo Chains

### Combo 1 — The SHATTER Protocol (Osteomancer + Cryomancer) [PRIMARY]

This is the game's highest single-hit physical damage combo and the explicit design purpose of the BRITTLE debuff.

**Step-by-step execution:**

1. **Turn N, Osteomancer activates:** Move to within Calcify range (4 tiles) of highest-value enemy target. Cast Calcify on target (2 AP — BRITTLE applied; CALCIFIED also applied — target is now slower). Cast Bone Shard at the same target (2 AP — 21 physical damage, consuming 1 BRITTLE charge). Move 1 tile to safe position (1 AP). 1 AP unspent (held for reactive BRITTLE Touch if enemy closes). Result: target has BRITTLE (1 charge remaining) and CALCIFIED.
2. **Turn N, same activation window — or Turn N+1, Cryomancer activates:** Cryomancer applies FROZEN to the BRITTLE target. The BRITTLE target cannot dodge (CALCIFIED reduces movement; FROZEN skips their turn). The BRITTLE charge is intact (only 1 of 2 charges was consumed by Bone Shard).
3. **Turn N+1, Osteomancer activates:** Cast Bone Shard on the BRITTLE FROZEN target. SHATTER triggers: 14 × 3.75 = 52.5 → 53 damage. Both BRITTLE and FROZEN are consumed.

**Result:** 21 + 53 = 74 total damage from 4 AP of Bone Shard investment (across 2 activations) plus the initial Calcify cost (2 AP). Against a 100-HP un-upgraded enemy Mancer, this is a 74% HP elimination in 1.5 activations — plus the 15-HP CALCIFIED passive damage (CALCIFIED: Cryomancer applying FROZEN to CALCIFIED unit takes no additional interaction, but CALCIFIED movement debuff kept the target in range) and any intervening BURNING or POISONED DoTs.

**With Shatter Expertise upgrade:** Bone Shard SHATTER step delivers 63 damage instead of 53. Total: 21 + 63 = 84 damage. Against 100 HP: dead.

---

### Combo 2 — Bone and Stone (Osteomancer + Geomancer)

**Setup:** Geomancer creates ELEVATED terrain (Raise Terrain) in a flanking position. Osteomancer places a Bone Spire on the ELEVATED tile.
**Result:** ELEVATED + Bone Spire = 3-tile effective height (standard 2-tile Spire + 1 elevated tile = 3), maximum LOS block with +2 adjacent armor bonus.
**Execution:** Allied ranged units on the same ELEVATED tile benefit from the +1 range bonus (elevated) AND the +1 adjacent armor (Bone Spire cover) simultaneously. The Osteomancer casts Bone Shard from the elevated position: spell range extended to 6 tiles.

**Tactical outcome:** An ELEVATED Bone Spire is the most defensible ranged position achievable without Signature abilities. Geomancer positions the platform; Osteomancer fortifies it; allied ranged units hold a position with extended range, maximum cover, and +2 effective armor. The opponent must invest significant spell AP to dislodge or destroy.

---

### Combo 3 — Toxic Brittle (Osteomancer + Toximancer)

**Setup:** Toximancer applies POISONED stacks to a target (CALCIFIED enemies from Calcify are slower, increasing the number of turns Toximancer can safely apply more stacks). Osteomancer applies BRITTLE.
**Result:** BRITTLE + POISONED unit: the POISONED stacks deal 9-15 HP/turn DoT; BRITTLE amplifies any physical hits by ×1.5. The unit is simultaneously taking sustained attrition damage AND is brittle against the next physical hit.
**Execution:** POISONED stacks run down HP; the Osteomancer waits for the right moment to deliver a SHATTER-equivalent physical hit (or has Cryomancer FREEZE the POISONED BRITTLE unit for full SHATTER multiplier).

**Tactical outcome:** The POISONED + BRITTLE combination is particularly cruel because neither status cancels the other — they compound. POISONED at 4 stacks does 12 HP/turn; BRITTLE makes the next physical hit deal ×1.5 (or ×3.75 with FROZEN). The combination punishes any healing or cleanse delay: if the opponent cannot cleanse POISONED and BRITTLE simultaneously, both damage vectors continue. Hydromancer can cleanse POISONED, but not BRITTLE (BRITTLE is not a DoT — no cleanse method removes it; it expires on hit count or duration).

---

### Combo 4 — The Bone Cage (Osteomancer + Gravimancer)

**Setup:** Osteomancer places Bone Spires in a ring around a contested central area. Gravimancer uses Gravity Well to pull enemies into the center of the ring.
**Result:** Pulled enemies inside the Bone Spire ring cannot exit without moving through the Bone Spire tiles (blocked) or destroying the Spires. They are effectively caged in the construct ring with the Osteomancer adjacent.
**Execution:** Osteomancer inside or at the cage entrance applies BRITTLE to trapped units and delivers Bone Shard hits. Cryomancer can FREEZE cage-trapped units for full SHATTER. Toximancer can seed TOXIC_TERRAIN inside the cage.

**Tactical outcome:** The Bone Cage is a setup-intensive combo requiring both construct investment and Gravimancer coordination, but the payoff — enemies trapped inside a bone ring surrounded by allied Mancers — is the most spatially dominant position achievable in the game. The cage converts the Osteomancer's 3-tile slow movement from a liability into an asset: it does not need to chase because the Gravimancer brings enemies to it.

---

## 8. Counters and Weaknesses

### What Shuts Down the Osteomancer

**Sonimancer sonic attacks:** Sonimancer's sonic spells deal full damage to Bone Spires with no fire-resistance reduction. A Sonimancer that prioritizes construct destruction can clear Bone Spires faster than the Osteomancer replaces them (Bone Spire Placement: 4 AP, 2-turn cooldown; Sonimancer medium spell: 3 AP vs. 40 HP Spire = 1-2 casts to destroy). An Osteomancer facing a Sonimancer opponent cannot reliably maintain construct fields — the terrain investment is consistently undercut.

**High-mobility opponents routing constructs:** The Osteomancer's constructs only matter if the opponent advances through them. An Aeromancer or fast-moving Faunamancer with Sprint can bypass construct chokepoints by moving around rather than through them. The Osteomancer's 3-tile move range cannot reposition constructs quickly enough to intercept mobile opponents.

**FLOODED approach paths:** Bone Spires on WET terrain have 30 HP (from the 40 standard). A Hydromancer who pre-floods the Osteomancer's preferred construct placement zones creates Spires that are destroyed in 1-2 fewer hits. Additionally, Ossified Ground cannot form on WET tiles, removing the secondary terrain benefit from Bone Shard. Against a Hydromancer opponent, the Osteomancer's terrain building is consistently partially undermined.

**Silenced Osteomancer:** BONE_ARMOR cannot be applied to allies and BRITTLE cannot be applied to enemies if the Osteomancer is Silenced. Bone Spire Placement is also disabled. A Silenced Osteomancer can only move — its entire kit is spell-delivery. Sonimancer Silence (ironic: the primary construct-counter also has the best Silence) is the most devastating status against the Osteomancer. Unlike the Floramancer (which at least maintains existing terrain while silenced), the Osteomancer's existing constructs remain but no new constructs can be placed and no BRITTLE can be applied during silence.

**HEAVY enemies (Gravimancer):** HEAVY status increases fall damage ×2 and prevents displacement — it also reduces the push effect from Bone Fortress and other knockback interactions. HEAVY enemies cannot be repositioned by Bone Fortress's push, meaning the construct-placement + push combo does not clear the area around the fortress if the opponent has HEAVY units in proximity. This is a moderate rather than hard counter — HEAVY makes the push less effective, not the constructs themselves.

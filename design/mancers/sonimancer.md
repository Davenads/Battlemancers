# Sonimancer — Full Design Document

---

## 1. Tactical Identity

The Sonimancer is the roster's most positioning-dependent Mancer and, when positioned correctly, its most devastating AoE damage dealer. Every spell it casts is a cone — a 90-degree arc extending 3 tiles from the Sonimancer's position in a chosen facing direction. This is the Sonimancer's defining constraint and its defining power: no Mancer in the roster can match its area coverage per AP when enemies cluster, and no Mancer punishes poor positioning as severely when they scatter. A Sonimancer facing a spread enemy formation is a liability; a Sonimancer facing a clustered formation is a catastrophe. The entire tactical identity flows from that tension.

Playing the Sonimancer well means thinking one turn ahead about where the cone will point next activation. It means reading enemy movement patterns, knowing which allied tools (Gravity Well, Flood Zone, Glacial Spike) will cluster enemies before the Sonimancer's cone arrives, and setting up cone angles from positions the enemy cannot easily scatter away from. RESONANCE is the Sonimancer's two-turn combo with itself: one spell seeds the terrain with RESONATING tiles, the following cone through those tiles doubles damage. This requires patience — the Sonimancer cannot just fire indiscriminately. It sets up, then detonates. The SILENCED status shuts down opposing spellcasters for a full turn, making the Sonimancer a meaningful control tool as well as a damage dealer. Its greatest weakness is mobility: at 3-tile move range and cone-only targeting, a Sonimancer that loses its position angle cannot simply aim a different way — it must spend AP to reposition, which costs the turn it needed to fire.

**Primary win condition:** The Sonimancer wins when it fires a cone into a cluster of 3 or more enemies, dealing full damage and SHATTER multipliers to every FROZEN unit in the arc simultaneously. The secondary win condition is a sequence of RESONANCE seeding followed by a doubled-damage cone pass that eliminates a priority target. The Sonimancer thrives in warbands that have at least one clustering tool — Gravimancer, Hydromancer Flood Zone, or Cryomancer Blizzard Field — that ensures the cone hits maximum targets.

**Core weakness:** The Sonimancer's cone geometry is its only targeting mode. Against an opponent who spreads their units into a wide line (spacing at least 2 tiles between each unit), the cone can hit at most one unit per cast. Spread formation is the hardest counter to the Sonimancer in the game — the opponent sacrifices dense positioning in exchange for minimizing cone efficiency. Additionally, the Sonimancer has no ranged single-target options; it cannot pick off an isolated high-value target without wasting the cone's area. SILENCED status removes the Sonimancer's ability to cast entirely for 1 turn — if the Sonimancer is SILENCED, it becomes completely passive that activation.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 85 | Low; the Sonimancer positions behind allies and fires forward |
| **Move Range** | 3 tiles per activation | Limited; repositioning for cone angles is the primary AP drain |
| **Base Armor** | 1 | Minimal; survival depends on positioning not absorption |
| **Spell Range** | 3 tiles (cone length — all spells are cones; range = cone length) | Shorter than most Mancers; requires closer positioning than ranged alternatives |
| **AP per Activation** | 6 | Standard; movement costs 1 AP per tile |
| **Element** | Sonic | All base spells deal Sonic damage; sonic interactions bypass walls and physical barriers |

**Cone geometry:** All Sonimancer spells are cones. A cone originates at the Sonimancer's tile, extends 3 tiles in the chosen direction, and spreads at a 90-degree arc. Width at maximum range: 3 tiles (1 tile at origin, growing to 3 at max range). The Sonimancer chooses facing direction at cast time (one of 8 cardinal/diagonal directions). The cone covers all tiles within the 90-degree arc at up to 3 tiles distance. No other Mancer in the roster uses cone-only targeting.

**AP budget example:** With 6 AP, the Sonimancer can move 2 tiles (2 AP) and fire a 3 AP cone spell plus a 1 AP Quick sonic pulse, or move 3 tiles and fire one 3 AP standard cone, or spend all 6 AP on its signature ability without moving.

---

## 3. Base Spell Kit

The Sonimancer's four base spells are designed to cover distinct sonic functions:
- **Sonic Pulse** — repeatable quick cone; DEAFENED applicator and light damage
- **Resonance Cone** — standard damage cone; applies RESONATING to terrain for follow-up amplification
- **Shatter Scream** — high-cost targeted sonic burst; primary SHATTERED status trigger and SILENCED applicator
- **Dissonance Wave** — AoE cone that specifically targets unit statuses; amplifies existing debuffs

---

### Spell 1: Sonic Pulse

| Field | Value |
|---|---|
| **Name** | Sonic Pulse |
| **AP Cost** | 1 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Cone (originates at Sonimancer; 3-tile range; 90-degree arc; chosen facing direction) |
| **Range** | 3 tiles (cone length) |
| **AoE Radius** | Cone width — 1 tile at origin, 3 tiles wide at max range |
| **Base Damage** | 10 |
| **Element** | Sonic |
| **Effects Applied** | Deals 10 Sonic damage to all units in cone. Applies `DEAFENED` to all hit units (no audio cues; –1 AP effective — removes 1 AP from next action; 2-turn duration). Sonic damage passes through all physical barriers — walls, OBSIDIAN, STONE_WALL do not block the cone. Units behind a wall are still hit if they are within the cone's geometry. `RESONATING` terrain tiles in the cone have the Sonic Pulse amplified to 20 damage instead of 10 (double damage on RESONATING tiles). |
| **Special Interactions** | See terrain interaction table in Section 4. Against a `RESONATING` tile: damage on that tile doubled (20 HP). Against units with `RESONANCE_CHARGE` status (already accrued from a prior Sonimancer spell): Sonic Pulse adds 1 RESONANCE_CHARGE stack; at maximum 3 stacks, the next sonic hit (including this Pulse if it is the triggering hit) auto-STUNS and delivers a sonic burst (+15 HP bonus damage; SHATTER triggers if target is also FROZEN). Against `CRYSTAL` terrain (Crystalomancer-created): Sonic Pulse propagates through connected crystal tiles at reduced damage (8 HP per crystal hop; max 3 hops). |

**Design note:** Sonic Pulse is the Sonimancer's workhorse Quick spell. At 1 AP with no cooldown, it can be used up to four times in a single activation with no movement (4 × 1 AP = 4 AP, leaving 2 AP for movement or a higher-tier spell). The low damage (10 HP per hit) is intentional — this is not a kill spell, it is a DEAFENED applicator and RESONANCE_CHARGE builder. Three Sonic Pulses on the same target across one activation cost 3 AP and apply DEAFENED (refreshed each time) while building toward a RESONANCE_CHARGE maximum-stack burst. The wall-piercing mechanic is the Sonimancer's unique distinction from every other ranged Mancer — the Sonimancer can fire at units behind a Geomancer Stone Wall without penalty.

**Spell answers YES to (design rule check):**
1. Applies unit status (DEAFENED; RESONANCE_CHARGE stacking) — YES
2. Exploits terrain state (RESONATING tiles double damage) — YES
3. Synergizes with Cryomancer (RESONANCE_CHARGE max-stack SHATTER on FROZEN), Crystalomancer (crystal propagation) — YES
4. Skill expression: RESONANCE_CHARGE stacking to trigger burst; RESONATING tile placement for double-damage timing — YES

---

### Spell 2: Resonance Cone

| Field | Value |
|---|---|
| **Name** | Resonance Cone |
| **AP Cost** | 3 AP |
| **Cooldown** | 1 turn |
| **Targeting Type** | Cone (originates at Sonimancer; 3-tile range; 90-degree arc) |
| **Range** | 3 tiles (cone length) |
| **AoE Radius** | Cone width — standard cone geometry |
| **Base Damage** | 25 |
| **Element** | Sonic |
| **Effects Applied** | Deals 25 Sonic damage to all units in cone. Applies 1 stack of `RESONANCE_CHARGE` to all hit units. All tiles in the cone become `RESONATING` (sonic resonance embedded in terrain; 2-turn duration). On the next Sonimancer sonic spell that hits a `RESONATING` tile, damage is doubled (2× base). This is the Sonimancer's primary two-turn self-combo: Resonance Cone seeds the terrain, the following activation's cone through those tiles deals 2× damage. |
| **Special Interactions** | Against FROZEN units in cone: 25 Sonic damage triggers SHATTER (25 × 2.5 = 62 HP; FROZEN removed). Against a RESONATING tile (already from a prior Resonance Cone cast on the same tiles): the second Resonance Cone hitting an already-RESONATING tile increases the resonance to `DEEP_RESONANCE` — next sonic spell hitting those tiles deals 3× damage instead of 2×. The DEEP_RESONANCE state lasts only 1 turn. Against `CRYSTAL` terrain (Crystalomancer-created): all crystal tiles within the cone that are connected to Crystal Nodes or Crystal Walls undergo Crystal Resonance — sonic damage propagates through connected crystal terrain at 15 HP per hop (3 hops maximum), hitting units on crystal tiles even if they are outside the cone. This is the Crystalomancer + Sonimancer signature cross-Mancer interaction. |

**Design note:** Resonance Cone is the Sonimancer's primary setup-and-detonate tool. Turn N: cast Resonance Cone through the enemy formation — 25 damage to all cone units, RESONATING tiles applied, RESONANCE_CHARGE stacks built. Turn N+1: the same cone direction through the same RESONATING tiles now deals 50 damage (2× base) to all units who are still in or moved back into the cone area. The 1-turn cooldown on Resonance Cone means the follow-up must come from a different cone spell (Sonic Pulse, Shatter Scream, or Dissonance Wave) — the Sonimancer cannot double-cast Resonance Cone back-to-back. This forces the player to vary their spell selection rather than spamming a single high-damage cone.

**Spell answers YES to (design rule check):**
1. Applies terrain state (RESONATING tiles) — YES
2. Applies unit status (RESONANCE_CHARGE stacking) — YES
3. Creates a self-combo (setup → amplified follow-up) — YES
4. Synergizes with Cryomancer (SHATTER on FROZEN), Crystalomancer (Crystal Resonance chain) — YES
5. Skill expression: RESONATING tile geometry planning; follow-up cone angle matching — YES

---

### Spell 3: Shatter Scream

| Field | Value |
|---|---|
| **Name** | Shatter Scream |
| **AP Cost** | 4 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Cone (originates at Sonimancer; 3-tile range; 90-degree arc) |
| **Range** | 3 tiles (cone length) |
| **AoE Radius** | Cone width — standard cone geometry |
| **Base Damage** | 35 |
| **Element** | Sonic |
| **Effects Applied** | Deals 35 Sonic damage to all units in cone. Applies `SILENCED` to all hit units (cannot cast spells for 1 turn; movement still allowed). Units with 3 stacks of `RESONANCE_CHARGE` that are hit by Shatter Scream immediately trigger the RESONANCE_CHARGE max-stack burst: +15 bonus damage, automatic STUN (1 turn), SHATTER if also FROZEN. Units with `RESONATING` status (terrain-based) hit by Shatter Scream take 70 damage (35 × 2 = 70) — Shatter Scream is the highest-value detonator for RESONATING terrain. Units that have taken sonic damage this activation (any prior Sonic Pulse or Resonance Cone this same turn) take an additional `SHATTERED` bonus: +10 HP bonus damage from cumulative sonic resonance within the unit's body. |
| **Special Interactions** | Against FROZEN units in cone: 35 Sonic damage triggers SHATTER (35 × 2.5 = 87 HP; FROZEN removed). On RESONATING tile against FROZEN unit: 70 × 2.5 SHATTER = 175 HP — the highest achievable single-hit damage from a Sonimancer on a FROZEN target without additional status modifiers. Against `CRYSTAL_WALL` (Crystalomancer): Shatter Scream at a Crystal Wall shatters it entirely — Crystal Wall (which normally blocks movement but not LoS) is destroyed, dealing 20 AoE damage to all units within 1 tile of the wall position from the crystalline explosion. This interaction is a direct Crystalomancer counter and an explosive removal of crystal cover. Against `SILENCED` targets already in cone: SILENCED refreshes duration (does not stack; just resets the 1-turn timer). |

**Design note:** Shatter Scream is the Sonimancer's "confirm the kill" spell. At 4 AP, it costs two-thirds of the activation budget, but the combination of 35 base damage, SILENCED crowd control, and RESONANCE_CHARGE burst trigger makes it the highest-value single-cast the Sonimancer has access to. The SHATTER interaction (35 × 2.5 = 87 HP) confirms kills on FROZEN targets at any HP level below 90. The RESONATING amplification (70 × 2.5 = 175 HP) is technically achievable but requires a 2-turn setup — seeding RESONATING with Resonance Cone on turn N, FREEZING the target with a Cryomancer partner, and then Shatter Screaming on turn N+1. This 3-piece combo (Cryomancer freeze + Sonimancer RESONATING setup + Shatter Scream follow-up) represents the Tier 3 combo system's highest achievable AoE burst.

**Spell answers YES to (design rule check):**
1. Applies unit status (SILENCED; triggers RESONANCE_CHARGE burst; STUN at max stacks) — YES
2. Exploits terrain state (RESONATING double damage) — YES
3. Destroys terrain feature (Crystal Wall explosion) — YES
4. Synergizes with Cryomancer (SHATTER on FROZEN + RESONATING), Crystalomancer (Crystal Wall destruction) — YES
5. Skill expression: RESONANCE_CHARGE timing; RESONATING terrain detonation; combo sequencing with Cryomancer freeze — YES

---

### Spell 4: Dissonance Wave

| Field | Value |
|---|---|
| **Name** | Dissonance Wave |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Cone (originates at Sonimancer; 3-tile range; 90-degree arc) |
| **Range** | 3 tiles (cone length) |
| **AoE Radius** | Cone width — standard cone geometry |
| **Base Damage** | 18 |
| **Element** | Sonic |
| **Effects Applied** | Deals 18 Sonic damage to all units in cone. Dissonance Wave is specifically designed to interact with existing unit statuses: each debuff status a unit is currently carrying when hit by Dissonance Wave adds +6 damage to that unit's hit (one extra 6 HP per unique debuff status; maximum 3 debuffs = +18 bonus; maximum total 36 damage). Debuffs that count: CHILLED, BURNING, POISONED (counts as 1 regardless of stacks), DEAFENED, HEAVY, SLOWED, BLINDED, BRITTLE_ARMOR, CALCIFIED, ROOTED. Additionally, the Dissonance Wave disrupts concentration: any unit in the cone that had an active ability or timed effect queued (e.g., the opponent had pre-planned a spell this activation in the blind-turn phase) has that queued action DISRUPTED — the unit's spell is wasted (AP refunded but the spell effect does not resolve). |
| **Special Interactions** | Against units with `RESONANCE_CHARGE` stacks: Dissonance Wave counts each RESONANCE_CHARGE stack as a debuff for the damage bonus (1 stack = +6; 3 stacks = +18). Additionally, if a unit has exactly 3 RESONANCE_CHARGE stacks, Dissonance Wave triggers the RESONANCE_CHARGE burst in addition to its own damage — the burst (+15 HP, auto-STUN) resolves after Dissonance Wave damage. Against `SILENCED` units in cone: SILENCED counts as a debuff (+6 damage); additionally, SILENCED units that take Dissonance Wave damage have their SILENCED duration extended by 1 turn. Against BURNING + POISONED targets: Dissonance Wave is the spell that best exploits multi-debuff stacking — a unit with BURNING (1) + POISONED (1) + CHILLED (1) = +18 bonus damage for 36 total from a 3 AP spell, competitive with Shatter Scream. |

**Design note:** Dissonance Wave is the Sonimancer's tactical swing tool — a 3 AP cone that scales dramatically with the number of debuffs the opponent is carrying. In a warband with a Toximancer (POISONED stacks), Pyromancer (BURNING), and Cryomancer (CHILLED), a single Dissonance Wave hitting a target with all three debuffs deals 18 + 18 = 36 HP from a 3 AP spell, competitive with Shatter Scream at half the cost. The disruption mechanic (canceling queued actions in blind-turn planning) is the most directly system-exploiting mechanic in the Sonimancer's kit — it requires understanding that the opponent is likely committing to specific planned actions, and that the Sonimancer can disrupt those plans if it fires into the right tile at the right time.

**Spell answers YES to (design rule check):**
1. Applies unit status (exploits existing debuffs for bonus damage) — YES
2. Disrupts opponent's planned actions (unique mechanic — blind-turn disruption) — YES
3. Synergizes with debuff-heavy Mancers (Toximancer, Pyromancer, Cryomancer) — YES
4. Skill expression: debuff counting before cast; disruption timing against predictable enemy activation sequences — YES

---

## 4. Terrain Interaction Table

### Sonic Spell Impact on Existing Terrain States

The following describes what happens when any Sonimancer spell strikes a tile in the listed terrain state. All Sonimancer spells are Sonic element; these interactions apply universally unless a spell's individual entry overrides them.

| Existing Terrain State | What Happens When Sonic Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **Normal (GROUND)** | Sound waves travel through; no terrain state change | `GROUND` (unchanged; sonic passes through) | Takes spell damage | Sonic spells do not transform basic ground terrain; they affect units and selected states |
| **RESONATING** | Sonic energy amplifies existing resonance | `RESONATING` consumed (expiry accelerated; resonance discharged into the spell hit) | Takes 2× base damage from the sonic hit (resonance amplification) | RESONATING tile is cleared after amplified hit; ready to be re-seeded next Resonance Cone cast |
| **CRYSTAL (any Crystalomancer construct)** | Sonic resonance propagates through connected crystal | Crystal terrain unchanged (sonic passes through crystal, not destroying it from this alone) | Units on crystal tiles: 15 HP per crystal hop from propagation; up to 3 hops | Crystal tiles adjacent to hit tile also propagate sonic damage; Crystalomancer + Sonimancer chain is triggered |
| **CRYSTAL_WALL (Crystalomancer)** | High-amplitude sonic shatters wall structure | `GROUND` (Crystal Wall destroyed by Shatter Scream specifically; other sonic spells deal structural damage — 20 HP per cone hit to wall HP pool) | Units within 1 tile of destroyed Crystal Wall take 20 AoE shrapnel damage | Crystal Wall has a limited HP pool; repeated sonic hits destroy it even without Shatter Scream |
| **ON_FIRE** | Sonic pressure waves feed the fire briefly | `ON_FIRE` (unchanged; fire fanned slightly — spreads 1 tile this turn in the cone direction) | Takes spell damage + `BURNING` from fire contact | Sonic waves momentarily fan fire in the cone direction; unintentional Pyromancer assist |
| **FLOODED** | Sonic resonates through water — wave propagation | `FLOODED` (unchanged; water transmits sonic efficiently) | Takes spell damage + 1 additional sonic hop to adjacent FLOODED units (like lightning chain through WET, but sonic; 8 HP per hop; 1 hop max) | Flooded terrain transmits sonic; units in FLOODED zones take sonic splash damage from nearby hits |
| **ICE_TILE** | Sonic vibration cracks ice surface | `WET` (ICE_TILE fractured by sonic resonance; melts to wet residue) | Takes spell damage + SHATTER triggered if unit was FROZEN on ICE_TILE (physical resonance qualifies) | FROZEN units on ICE_TILE hit by sonic = SHATTER (×2.5); ICE_TILE also destroyed |
| **PERMAFROST** | High-amplitude sonic fractures permanent frozen mud | `MUD` (PERMAFROST cracks and thaws; 2-turn duration) | Takes spell damage + `CHILLED` removed if applicable | Sonimancer is the only non-Geomancer that can remove PERMAFROST through sonic fracturing |
| **CHARGED** | Sonic waves through CHARGED tiles create electromagnetic interference | `CHARGED` (unchanged; EMI added) | Takes spell damage + `DEAFENED` extended by 1 turn (EMI disrupts audio/magical signals) | CHARGED + sonic creates DEAFENED extension — radio interference analog |
| **TOXIC_TERRAIN** | Sonic disperses toxic surface particles into the air | `TOXIC_TERRAIN` (unchanged ground state; spores/toxins dispersed upward) | Units in cone above TOXIC_TERRAIN take spell damage + 1 stack `POISONED` (airborne dispersal) | Sonimancer unwillingly aerates TOXIC_TERRAIN tiles — toxic spores hit any unit in the cone above |
| **MUD** | Sound travels poorly through mud; cone is dampened | `MUD` (unchanged) | Takes spell damage –30% (mud absorbs sonic energy) | Sonic damage reduced through MUD; Sonimancer should avoid firing cones that must pass through MUD tiles |
| **OBSIDIAN** | Dense crystalline structure — sonic reflective | `OBSIDIAN` (unchanged; sonic reflects) | Sonic cone hitting OBSIDIAN reflects 1 tile back — units directly behind the Sonimancer in the reflected path take 5 HP reflected damage | Obsidian reflects rather than absorbs sonic; minor self-damage risk from bad angles |
| **STEAM_CLOUD** | Sonic waves travel through steam with no degradation | `STEAM_CLOUD` (unchanged) | Takes full spell damage through STEAM_CLOUD; BLINDED is not removed | Sonic ignores STEAM_CLOUD visual blocking; Sonimancer can fire accurately through its own team's steam obscuration |
| **OVERGROWTH** | Sonic shatters dried or crystallized plant matter | `GROUND` (OVERGROWTH shattered by sonic vibration; Floramancer barriers destroyed) | Takes spell damage; ROOTED status removed from units in OVERGROWTH | Sonimancer is one of the most efficient Floramancer counter-tools — sonic shatters vine barriers |

### RESONATING Tile Mechanics (Extended)

`RESONATING` is a terrain state unique to the Sonimancer:
- Applied by: Resonance Cone (to all tiles in the cone)
- Duration: 2 turns (the seed and the follow-up window)
- Effect on next sonic spell: 2× damage to units on that tile; 3× if DEEP_RESONANCE
- DEEP_RESONANCE: applied when Resonance Cone hits a tile already RESONATING; lasts 1 turn
- After amplified hit: RESONATING is consumed (returned to base terrain state)

### Terrain States Beneficial to the Sonimancer

| State | Benefit |
|---|---|
| `RESONATING` tiles (self-created) | Any sonic hit through RESONATING tiles deals 2× damage; the Sonimancer's primary self-combo setup tool |
| `FLOODED` zones | Sonic propagation through water hits multiple targets; free sonic splash to adjacent FLOODED units |
| `ICE_TILE` | Sonic shatters ice and triggers SHATTER on FROZEN units — Cryomancer's frozen field becomes a SHATTER delivery zone for the Sonimancer |
| `CRYSTAL` terrain (Crystalomancer-created) | Crystal propagation extends sonic damage through connected crystal tiles without additional AP cost |

### Terrain States Hazardous to the Sonimancer

| State | Hazard |
|---|---|
| `MUD` | Sonic damage through MUD tiles is reduced by 30%; Sonimancer cones that must pass through MUD zones lose effectiveness |
| `OBSIDIAN` | Sonic reflection from OBSIDIAN can damage the Sonimancer itself at certain angles; Geomancer Obsidian walls are a positional hazard to the Sonimancer |
| `VINES / OVERGROWTH` | While sonic shatters OVERGROWTH, the ROOTED status from Floramancer vines prevents the Sonimancer from repositioning for optimal cone angles |
| `CHARGED` | No defensive advantage; the Sonimancer takes normal electrical arc damage from CHARGED terrain |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

Spell variants replace a base spell entirely with a more powerful or specialized version.

#### Variant A: Sonic Barrage (replaces Sonic Pulse) — +15 pts

**Description:** Sonic Pulse is replaced by Sonic Barrage — a rapid triple-pulse burst. Sonic Barrage fires three sequential sonic pulses in the chosen cone direction, each dealing 8 damage (24 total) and each independently building RESONANCE_CHARGE stacks. All three pulses apply DEAFENED. AP cost is 3 AP; cooldown is 1 turn. A unit hit by all three pulses receives 3 RESONANCE_CHARGE stacks in a single cast — immediately triggering the max-stack burst (auto-STUN + 15 HP bonus + SHATTER if FROZEN).

**Trade-off:** Triple RESONANCE_CHARGE application in one cast (3 AP burst trigger) at the cost of Sonic Pulse's 1 AP no-cooldown accessibility. Best for Sonimancers built around the RESONANCE_CHARGE burst as a primary control tool rather than incremental DEAFENED accumulation.

#### Variant B: Amplified Scream (replaces Shatter Scream) — +25 pts

**Description:** Shatter Scream is replaced by Amplified Scream — a larger, longer cone. Amplified Scream uses a 4-tile cone length (up from 3) and a 120-degree arc (up from 90). Base damage is 30 (down from 35 — power is traded into area coverage). SILENCED still applied to all hit units. AP cost remains 4 AP; cooldown remains 2 turns.

**Trade-off:** Larger cone area (catches more enemies and extends range by 1 tile) at slightly lower base damage per hit. Best in warbands where the primary challenge is hitting clustered-but-slightly-spread enemies that the standard 90-degree cone would miss on one edge. The 120-degree arc converts the Sonimancer from a narrow-beam specialist to a wide-arc sweeper.

---

### Passive Traits

#### Passive A: Resonance Master — +20 pts

**Description:** All RESONATING tiles created by the Sonimancer have their duration extended from 2 turns to 3 turns. Additionally, the Sonimancer itself is immune to the damaging effects of its own RESONATING tiles — when the Sonimancer stands on a RESONATING tile and fires a cone, the reflected amplification does not apply to the Sonimancer's position. This allows the Sonimancer to fire through RESONATING zones it has created without risk of self-amplification confusion.

**Trade-off:** Extended RESONATING duration gives the Sonimancer a larger follow-up window (3 turns instead of 2 to land the amplified hit). Most valuable in slow-paced engagements where RESONATING terrain seeded on turn N may not be detonated until turn N+2.

**Synergy note:** Resonance Master combined with Crystalomancer crystal terrain creates a 3-turn window for Crystal Resonance propagation to amplify sonic damage through connected crystal nodes on the 2nd or 3rd turn rather than requiring an immediate follow-up.

#### Passive B: Brutal Harmonics — +25 pts

**Description:** When the Sonimancer triggers SHATTER on a FROZEN unit (via any sonic spell), the SHATTER damage multiplier increases from ×2.5 to ×3.0 for that specific hit. Additionally, after a SHATTER is triggered, the Sonimancer generates a free Sonic Pulse (no AP cost; automatically fires) in the same direction the SHATTER occurred — a sonic aftershock from the shattering impact. The aftershock deals 8 damage to everything in the cone (the same cone as the triggering spell) but does not apply additional RESONANCE_CHARGE.

**Trade-off:** The SHATTER multiplier upgrade (×3.0) is a meaningful kill-confirmation boost. Resonance Cone SHATTER: 25 × 3.0 = 75 HP. Shatter Scream SHATTER: 35 × 3.0 = 105 HP. Shatter Scream on RESONATING tile SHATTER: 70 × 3.0 = 210 HP (achievable maximum against a 2-turn setup). The aftershock adds modest AoE chip damage post-SHATTER. Best in warbands where the primary role of the Sonimancer is SHATTER execution alongside a Cryomancer.

#### Passive C: Wall of Sound — +20 pts

**Description:** The Sonimancer can set up a `SOUND_BARRIER` zone (1 AP; self-centered; 1-tile radius; no cooldown; max 1 active at a time). Within the SOUND_BARRIER zone, all enemy unit movement costs 1 additional AP per tile (the sonic pressure creates resistance). The SOUND_BARRIER persists as long as the Sonimancer remains in the zone; if the Sonimancer moves out of the zone, the barrier collapses instantly. The SOUND_BARRIER is invisible to the opponent until they attempt to enter it (they see movement resistance highlighted as they target the zone).

**Trade-off:** A free persistent area-denial zone that costs only 1 AP to establish and maintains itself with no cooldown — the trade is that the Sonimancer must remain stationary in the zone to keep it active. Best for positional Sonimancer builds that find a good cone angle and hold it for multiple turns rather than repositioning frequently.

---

### Stat Enhancements

#### Stat A: Resonant Constitution (+15 HP) — +10 pts

**Description:** Max HP increases from 85 to 100. Brings the Sonimancer to the baseline survival range for glass-cannon Mancers. At 100 HP, the Sonimancer can survive one Ember Shot (18 HP) plus the resulting BURNING DoT through an entire turn without immediately entering critical HP. Critical for Sonimancers that hold positions in the open (as cone geometry requires) rather than behind cover.

#### Stat B: Extended Harmonics (+1 Cone Length) — +20 pts

**Description:** All Sonimancer cone spells have their length extended from 3 tiles to 4 tiles. The cone's area increases substantially: a 4-tile cone at 90 degrees covers 4 tiles wide at maximum range instead of 3. This is the most impactful range upgrade available to any single-targeting-mode Mancer in the roster — it converts the Sonimancer from a close-quarters AoE dealer to a medium-range area sweeper, giving it additional distance from melee threats while maintaining the same cone arc.

**Design note:** At 3-tile cone length, a Sonimancer must position 3 tiles from its intended target — close enough to be reached by most melee Mancers in a single activation. At 4 tiles, that threat range increases to 4 tiles from targets, creating a meaningful safety buffer. Combined with the Aeromancer's 5-tile base Move Range, a 4-tile Sonimancer cone can sweep the full threat envelope of most approaching enemies before they close the gap.

---

### Signature Ability

The Signature Ability is unlocked as an additional spell slot — it does not replace any base spell.

#### Signature: Sonic Obliteration — +40 pts

| Field | Value |
|---|---|
| **Name** | Sonic Obliteration |
| **AP Cost** | 6 AP (entire activation; Sonimancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Full-radius cone — instead of a 90-degree arc in one direction, Sonic Obliteration fires simultaneously in all 4 cardinal directions (N, S, E, W) as four simultaneous cones |
| **Range** | 3 tiles per cone direction |
| **AoE Radius** | 4 independent cones at 90-degree arcs, covering N/S/E/W from the Sonimancer's position (together, this covers a large cross-shaped plus (+) area of sonic saturation) |
| **Base Damage** | 40 per cone hit (each unit is only hit once regardless of how many cones overlap their tile) |
| **Element** | Sonic |
| **Effects Applied** | Every unit within 3 tiles of the Sonimancer in any of the 4 cardinal directions takes 40 Sonic damage and receives `SILENCED` (1 turn; cannot cast spells). All `RESONATING` tiles in all four cone areas are simultaneously detonated for 2× damage (80 HP) on their amplified hit. The Sonimancer itself is protected from the central resonance discharge (sonic immunity in self-tile during this cast). If the Sonimancer has previously seeded RESONATING tiles in multiple directions, Sonic Obliteration detonates all of them simultaneously. |
| **Special Interactions** | Against FROZEN units in any cone direction: SHATTER (40 × 2.5 = 100 HP; 80 × 2.5 = 200 HP on RESONATING tile). Against Crystal terrain in multiple cone directions: Crystal Resonance propagates in all directions simultaneously — multiple crystal chains activating at once from a single cast. Against units with max RESONANCE_CHARGE stacks (3) hit by any of the 4 cones: RESONANCE_CHARGE burst triggers (auto-STUN + 15 HP bonus + SHATTER if FROZEN). |

**Design note:** Sonic Obliteration is the Sonimancer's "the board is my instrument" ability. It converts the Sonimancer from a directional cone specialist to a 360-degree sound detonation at the cost of a full activation. Its primary scenario is a Sonimancer surrounded (by design or misfortune) — Sonic Obliteration turns encirclement from a disadvantage into mass AoE punishment. Its secondary use is as a RESONATING detonation — if the Sonimancer spent prior turns seeding RESONATING tiles in multiple directions (using Resonance Cone twice on different angles, or with Resonance Master's extended duration), Sonic Obliteration detonates all four directions simultaneously. On a board with pre-seeded RESONATING tiles in three directions, that is three simultaneous 80 HP hits to any unit in those directions.

**Synergy note:** After GRAVITATIONAL COLLAPSE (Gravimancer signature), all enemies are clustered at the center point. The Sonimancer activates, uses Sonic Obliteration centered on (or adjacent to) that cluster, and all four cardinal cones radiate outward from the cluster's position — every unit pulled by the Collapse is in range of at least one cone direction, and all take 40 HP (or 80 HP on RESONATING tiles).

---

## 6. Faction Synergy

### Best Faction: The Verdant Pact

The Verdant Pact's Glade Archers apply POISONED on hit. Every POISONED unit hit by the Sonimancer's Dissonance Wave takes +6 bonus damage. Dissonance Wave at 3 AP hitting a target with POISONED + BURNING (from Pact Floramancer toxic spores) + DEAFENED (from prior Sonic Pulse) = 18 + 18 = 36 HP total from a single 3 AP cone spell. The Verdant Pact's multi-debuff potential from archer volleys and natural terrain effects directly scales Dissonance Wave's damage ceiling.

Thornback Sentinels entrenching in position create a stable front line that prevents enemies from scattering easily out of cone range — enemies bottlenecked by Sentinel positioning are easier to cone. The Terrain Bond regen keeps Sentinels alive through the DoT pressure they absorb while the Sonimancer fires over them.

### The Gilded Throne — Coordinated SHATTER Barrage

The Gilded Throne's Crossbow Corps fire physical bolts. SILENCED enemy Mancers (from Shatter Scream or Dissonance Wave) cannot spend AP on counterspells during the Crossbow volley — the Sonimancer SILENCES, the Crossbow Corps fires into the silenced cluster. The Corps' physical damage also triggers SHATTER on FROZEN targets alongside the Sonimancer's sonic SHATTER — both simultaneously available in the same turn if Crossbow Corps is adjacent.

Iron Discipline (Charm and Panic immunity) matters because the Sonimancer frequently positions its own team units in the cone's "backstop" position — an Iron Discipline warband ensures no allied unit behind the cone accidentally gets CONFUSED and moves into the fire zone.

### The Ashen Covenant — Death into Sonic

Grave Husks generate Necromancer fuel on death. The Sonimancer's AoE cones that sweep Husk clusters (whether friendly or enemy) accelerate the kill-count for Necromancer economy. In a Sonimancer + Necromancer warband, the Sonimancer kills enemy Husks at scale, generating Necromancer fuel, which summons undead reinforcements, which the Sonimancer can then SILENCE to prevent any spellcasting responses. Wailing Shades (phase-through ranged) can fire through Geomancer walls — and the Sonimancer can fire its sonic cones through those same walls (sonic ignores physical barriers), creating a coordinated two-unit through-wall assault.

---

## 7. Combo Chains

### Combo 1 — Resonance Detonation (Sonimancer self-combo) [CORE MECHANIC]

**Mancers involved:** Sonimancer solo

**Step-by-step execution:**

1. **Turn N, Sonimancer activates:** Resonance Cone (3 AP) in direction of enemy cluster. 25 damage to all units in cone; RESONATING applied to all tiles in cone; RESONANCE_CHARGE stacks begin building.
2. **Turn N+1, Sonimancer activates:** Any sonic spell through the same cone direction. RESONATING tiles double the damage of the follow-up hit — a Shatter Scream through RESONATING tiles deals 70 HP (35 × 2) instead of 35.

**Damage math:** Turn N: 25 HP to 3 units = 75 total. Turn N+1: 70 HP (on RESONATING) to same 3 units = 210 total. Two-turn investment: 285 total damage across the formation. Against a standard 85–100 HP Mancer, the 70 HP hit on turn N+1 removes the vast majority of their HP (only units above ~95 HP survive a full-RESONATING Shatter Scream).

---

### Combo 2 — Gravity Cluster Cone (Sonimancer + Gravimancer) [SIGNATURE]

**Mancers involved:** Sonimancer + Gravimancer

**Step-by-step execution:**

1. **Gravimancer activates:** Gravity Well placed at chokepoint center (3 AP). Enemy units within 3 tiles begin being pulled toward center (1 tile per enemy turn).
2. **2 turns of passive pull:** Enemy units converge on the center.
3. **Sonimancer activates (Turn N+2):** Resonance Cone aimed at the Gravity Well center. All clustered enemies hit by the full 25 HP cone simultaneously. RESONATING seeded.
4. **Sonimancer activates (Turn N+3):** Shatter Scream through the same angle — 70 HP (RESONATING amplification) hits every unit still at the center.

**Result:** A 4-turn sequence that converts spread enemies into a kill zone without requiring any of the enemies to voluntarily cluster.

---

### Combo 3 — SHATTER Through Ice (Sonimancer + Cryomancer) [FLAGSHIP]

**Mancers involved:** Sonimancer + Cryomancer

**Step-by-step execution:**

1. **Cryomancer activates:** Ice Lance at priority target (FROZEN applied; 1-turn SHATTER vulnerability).
2. **Sonimancer activates (same turn — Mancer initiative):** Shatter Scream cone through the FROZEN target's position. 35 × 2.5 = 87 HP SHATTER damage. FROZEN removed.
3. **Optional (if Resonance Cone seeded prior turn):** RESONATING tile + FROZEN target = 70 × 2.5 = 175 HP. Eliminates every non-upgraded Mancer in the game from full HP.

**Tactical note:** This is the Sonimancer's cleanest kill-confirmation sequence. Cryomancer FROZEN is the setup; Sonimancer Shatter Scream is the execution. The advantage over Geomancer SHATTER: the Sonimancer's cone hits all FROZEN units in the cone arc simultaneously, not just a single target.

---

### Combo 4 — GRAVITATIONAL COLLAPSE into Sonic Obliteration (Gravimancer + Sonimancer)

**Mancers involved:** Gravimancer + Sonimancer

**Step-by-step execution:**

1. **Confirm ally positions:** No allies within 5 tiles of the chosen Collapse center.
2. **Gravimancer activates (6 AP):** GRAVITATIONAL COLLAPSE. All enemies within 5 tiles pulled to center; travel damage applied.
3. **Sonimancer activates (same turn — Mancer initiative):** Sonic Obliteration centered adjacent to the collapse point. All four cardinal cones radiate outward from the cluster. Every enemy unit pulled to center is in range of at least one cone direction; all take 40 HP sonic damage (or 80 HP on any pre-seeded RESONATING tiles).

**Combined damage:** Travel damage from Collapse + 10 HP collision + 40 HP Sonic Obliteration = 58–98 HP to every enemy unit in the engagement simultaneously. Against any enemy Mancer below 110 HP, this single two-Mancer activation sequence is lethal or near-lethal across the entire enemy roster.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Sonimancer

| Mancer | Counter Mechanism |
|---|---|
| **Psychomancer** | SILENCED status applied to the Sonimancer shuts down its entire kit for 1 turn — the Sonimancer has no non-cone options, and SILENCED removes all spellcasting. Additionally, CONFUSED status on the Sonimancer redirects its cone randomly, potentially hitting allies. A Psychomancer that SILENCES the Sonimancer exactly when it was about to fire into a RESONATING cluster wastes a 2-turn setup investment entirely. |
| **Aeromancer** | UPDRAFT zones grant WEIGHTLESS to allies inside. WEIGHTLESS units hovering 1 tile above ground are less affected by ground-level RESONATING terrain (the sonic resonance is ground-embedded; floating units receive only 50% of the RESONATING amplification). Additionally, Aeromancer's wind can push enemies out of the cone arc between the Sonimancer's RESONATING seed turn and the follow-up detonation turn. |
| **Gravimancer (opponent)** | An enemy Gravimancer applying HEAVY to the Sonimancer's allies pulls them toward a different center, dragging them out of the cone's arc before the Sonimancer fires. The Gravimancer also threatens to Crush the Sonimancer directly — at 90 HP and 1 armor, the Sonimancer is highly vulnerable to a 5 AP Crush (45 HP + potential fall damage). |

---

*End of Sonimancer design document.*

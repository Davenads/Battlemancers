# Floramancer — Full Design Document

---

## 1. Tactical Identity

The Floramancer is the battlefield's patient architect — a Mancer who treats open terrain as raw material and fights by slowly converting neutral ground into a strangling garden that punishes every movement the opponent makes. Its spells do not burst, do not chain, and do not dominate the opening turns of any engagement. What they do is establish a creeping territorial claim: VINES that tangle and root, SPORES clouds that seep poison into units caught inside, and organic barriers that redirect enemy movement toward the Floramancer's preferred killing ground. A skilled Floramancer player wins by making the opponent's plan fall apart three turns after it was committed to.

Playing the Floramancer well demands accurate prediction of enemy movement routes. Vine seeds planted in the wrong location accomplish nothing; planted across the only viable approach, they root an entire formation in place for an allied Mancer to punish. The Floramancer's poison is plant-origin — pollen and spore-based rather than the venom Toximancer applies — and this distinction matters mechanically: Floramancer poison is shorter-duration but delivered through persistent terrain rather than direct application, meaning it punishes hesitation and clustering rather than requiring direct hits. Floramancer never needs to stand where the fighting happens; it shapes where the fighting happens and lets the terrain fight on its behalf.

**Primary win condition:** The Floramancer wins when it has converted the central approach paths of the map into VINES and SPORES terrain while allied Mancers hold a cleaner zone. The opponent's units are ROOTED, POISONED, or slowed to crawl while trying to navigate the growth. A paired Toximancer can amplify the SPORES cloud into a virulent contamination zone that accelerates the poison curve dramatically.

**Core weakness:** The Floramancer is the softest Mancer in terms of direct damage output. An opponent who abandons contested terrain entirely and seeks the Floramancer across open ground where vines have not been planted can close the distance before the terrain traps are established. This Mancer has no burst, no displacement, and no answer to fast-aggression openings beyond hoping the early growth holds. It is also hard-countered by Pyromancer — ON_FIRE terrain destroys VINES and OVERGROWTH instantly, burning the Floramancer's entire turn investment down in one cast. Any warband including a Floramancer should either include a Hydromancer to suppress enemy fire or invest in infantry screening to prevent the Pyromancer from reaching its terrain.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 80 | Lowest HP on the roster; must stay protected behind its own growth |
| **Move Range** | 4 tiles per activation | Above-average mobility — needs to seed terrain broadly |
| **Base Armor** | 1 | Minimal; relies on vine barriers and infantry for protection |
| **Spell Range** | 5 tiles (base) | Each spell lists its own range; this is the fallback reference |
| **AP per Activation** | 6 | Standard for all Mancers; movement costs 1 AP per tile |
| **Element** | Plant/Nature | All base spells apply plant-origin terrain states or plant-element effects |

**AP budget example:** With 6 AP, the Floramancer can move 2 tiles (2 AP), cast Vine Surge (2 AP), and cast Pollen Burst (2 AP) — seeding both VINES and SPORES terrain in a single activation. Alternatively, it can spend 4 AP on Overgrowth Barrier plus 2 AP on a Vine Surge, sacrificing mobility for terrain investment.

---

## 3. Base Spell Kit

The Floramancer's four base spells cover distinct combat functions:
- **Vine Surge** — rapid VINES terrain placement with ROOTED threat
- **Pollen Burst** — SPORES terrain creation; the Floramancer's primary poison delivery
- **Overgrowth Barrier** — structural organic wall; terrain feature creation
- **Entangling Bloom** — heavy-AP root + SPORES combination on a single target zone

---

### Spell 1: Vine Surge

| Field | Value |
|---|---|
| **Name** | Vine Surge |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Ground Target — places VINES terrain on a target tile |
| **Range** | 6 tiles |
| **AoE Radius** | 1 tile (target tile only; no splash) |
| **Base Damage** | 6 (impact of vine eruption — minor) |
| **Element** | Plant/Nature |
| **Effects Applied** | Target tile becomes `VINES` terrain (movement cost 2.0; persists 4 turns). Any unit ending their turn on a VINES tile is immediately `ROOTED` (cannot move; can still cast; duration 1 turn). Unit on the tile at cast time takes 6 damage and must make an immediate ROOTED check — if the cast lands while the unit is standing still (not mid-move), ROOTED applies immediately at 1-turn duration. |
| **Temperature Effects** | **0 temperature change** (vines do not transfer heat). The VINES terrain itself has no thermal properties. |
| **Terrain Interaction** | Casting on `GROUND`: creates VINES (standard). Casting on `WET`: vines grow faster — VINES terrain persists 5 turns instead of 4; unit movement cost on WET VINES is 2.5 (waterlogged growth). Casting on `MUD`: VINES grow through mud and merge — VINES + MUD combined state (movement cost 3.0; impassable to most infantry without spending 3 AP per tile). Casting on `ON_FIRE`: vines are instantly destroyed; no terrain state created; Floramancer wastes the cast. Casting on `SPORES`: SPORES terrain is not replaced — VINES grow beneath the cloud, creating a stacked VINES + SPORES tile (units stepping on it are both ROOTED and exposed to SPORES simultaneously). Casting on `OBSIDIAN`: vines cannot grow on hardened obsidian; cast fails. |

**Design note:** Vine Surge is the Floramancer's workhorse — low cost, no cooldown, highly spammable. Two uses in one activation (2 AP + 2 AP) seed two VINES tiles, potentially covering both sides of a chokepoint. The key skill in using Vine Surge is lead placement: vines should be placed where the enemy will be, not where they are now. A ROOTED unit cannot move but can still cast spells — the root is a positioning lock, not a full incapacitation. Floramancer players must pair vine placement with allied spells that punish stationary targets.

---

### Spell 2: Pollen Burst

| Field | Value |
|---|---|
| **Name** | Pollen Burst |
| **AP Cost** | 2 AP |
| **Cooldown** | 1 turn |
| **Targeting Type** | Ground Target — AoE Radial; creates SPORES terrain cloud |
| **Range** | 5 tiles (to center of AoE) |
| **AoE Radius** | 2 tiles |
| **Base Damage** | 4 (pollen irritant; minor on-cast damage) |
| **Element** | Plant/Nature |
| **Effects Applied** | All tiles in radius become `SPORES` terrain (persists 3 turns; does not spread naturally). Any unit that moves through or ends their turn in SPORES terrain receives 1 stack of `POISONED` (3 HP/turn; maximum 5 stacks). Moving through SPORES (not just ending turn) is sufficient — any movement action that crosses a SPORES tile triggers the POISONED stack. POISONED stacks from SPORES terrain are plant-origin: duration is 2 turns per stack (shorter than Toximancer's venom-based 3-turn stacks). |
| **Temperature Effects** | **0 temperature direct** (spore particles have no inherent thermal property). However, SPORES terrain takes on thermal characteristics from adjacent terrain states — see Temperature Interaction Notes for HOT SPORES and COLD SPORES emergent combinations. |
| **Terrain Interaction** | Casting on `GROUND`: creates SPORES (standard). Casting on `WET`: SPORES form a wet spore cloud — unit movement cost in the zone increases; POISONED stacks applied are refreshed (not additional) but WET + SPORES combo is conductive (Electromancer chain works through wet spore terrain). Casting on `VINES`: SPORES settle onto VINES — stacked VINES + SPORES tile created (see Vine Surge entry). Casting on `ON_FIRE`: spores combust — brief TOXIC_FIRE flash (all units in radius take 8 fire damage + 1 POISONED stack, then terrain clears to GROUND; the cloud is burned away instantly). Casting on `WET` terrain already containing SPORES: Toximancer-amplified virulent cloud is created if Toximancer has cast Venomous Ground nearby — see Combo Section 7. Casting on `CHARGED`: spores conduct charge; chain arc fires immediately to all adjacent units (spore particles are conductive), then SPORES terrain forms normally. |

**Design note:** Pollen Burst is the Floramancer's distinctive poison delivery — it creates a hazard zone that persists independently of whether the Floramancer remains alive or in range. The shorter-duration POISONED stacks (2 turns vs. Toximancer's 3 turns) reflect plant-pollen origin vs. concentrated venom: the Floramancer's poison is ambient and environmental, not precision-applied. SPORES terrain does not spread on its own, making precise placement critical. Once a SPORES cloud expires or is burned away, those poison stacks on affected units do not linger unless renewed.

---

### Spell 3: Overgrowth Barrier

| Field | Value |
|---|---|
| **Name** | Overgrowth Barrier |
| **AP Cost** | 4 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Ground Target — places a line of OVERGROWTH terrain features |
| **Range** | 4 tiles (to start tile of barrier) |
| **AoE Radius** | Line — 3 tiles long in chosen direction from target tile |
| **Base Damage** | 0 (terrain placement; no direct damage) |
| **Element** | Plant/Nature |
| **Effects Applied** | Creates a 3-tile-long line of `OVERGROWTH` terrain features. OVERGROWTH functions as a soft barrier: blocks line-of-sight, imposes movement cost 2.0 to enter or pass through, does not fully block movement (unlike a wall — units CAN move into OVERGROWTH, they just pay double movement cost). Units in OVERGROWTH tiles are partially concealed: ranged attacks against OVERGROWTH-occupying units have a 25% miss chance (cover property). OVERGROWTH persists 6 turns (longest persistence of any Floramancer terrain). |
| **Temperature Effects** | **0 temperature** (organic plant barrier, no heat transfer). |
| **Terrain Interaction** | On `ON_FIRE`: OVERGROWTH is instantly destroyed and replaced by ON_FIRE (organic matter burns); this is the Pyromancer's primary counter to Floramancer barrier play. On `WET`: OVERGROWTH grows thicker — movement cost through WET OVERGROWTH is 2.5; miss chance from cover increases to 35%. On `GROUND`: standard creation. On `FROZEN` (ice tile): OVERGROWTH cannot root in frozen ground — cast fails; tile remains FROZEN. On `OBSIDIAN`: cannot grow on obsidian. On `NECROTIC_ASH` (Necromancer): organic matter withers in necrotic soil — OVERGROWTH lasts only 2 turns instead of 6; degrades rapidly. |

**Design note:** Overgrowth Barrier is the Floramancer's map-editing tool. At 4 AP, it costs more than half the activation budget and cannot be spammed. The value is long-duration cover and line-of-sight denial — a 3-tile Overgrowth line placed across a central corridor can redirect enemy movement for 6 full turns, forcing opponents into less favorable approach paths. Unlike stone walls, Overgrowth Barriers do not fully block movement — they slow and conceal. Combined with VINES on the far side of the barrier, an approach path can be: enter overgrowth (half movement speed + cover), exit into vines (ROOTED), stand in pollen cloud (POISONED). The 3-turn cooldown prevents the Floramancer from covering the entire board in barriers — it must commit to a specific approach-denial direction.

---

### Spell 4: Entangling Bloom

| Field | Value |
|---|---|
| **Name** | Entangling Bloom |
| **AP Cost** | 4 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Ground Target — AoE Radial |
| **Range** | 4 tiles (to center) |
| **AoE Radius** | 2 tiles |
| **Base Damage** | 14 (on-cast; vines erupt through the ground violently) |
| **Element** | Plant/Nature |
| **Effects Applied** | All tiles in radius become `VINES` terrain (movement cost 2.0; persists 4 turns). All units in the radius at cast time take 14 damage and are immediately `ROOTED` (2-turn duration — longer than the passive ROOTED from Vine Surge). The 2-turn ROOTED is the primary feature: units caught in a full-bloom root cannot move for 2 turns, making them trivial targets for allied follow-up spells. The center tile additionally becomes `SPORES` terrain stacked beneath the VINES (combined VINES + SPORES). |
| **Temperature Effects** | **0 temperature direct** (plant eruption has no thermal component). However: ROOTED units on BURNING tiles cannot move away, suffering the full +10 temperature/turn from BURNING terrain contact with no escape. A unit ROOTED by Entangling Bloom on a BURNING tile gains +10 temperature per turn for the 2-turn root duration — a minimum of +20 temperature applied passively, which can push a WARM unit to HOT or a HOT unit to OVERHEATED. |
| **Terrain Interaction** | On `GROUND`: standard bloom creation. On `WET`: vines flourish — bloom persists 5 turns; ROOTED duration extends to 3 turns for units on WET tiles within the radius. On `SPORES` (existing): the spore concentration in the target zone intensifies — all POISONED stacks applied by the existing SPORES in the AoE are doubled (2 stacks per movement through the zone instead of 1) for the remainder of their duration. On `ON_FIRE`: bloom is destroyed instantly before it establishes; cast fails; Floramancer loses 4 AP with no effect. On `TOXIC_TERRAIN` (Toximancer): Toximancer's ground toxins feed the bloom — vines grow tainted; ROOTED units on TOXIC_TERRAIN within the bloom take 1 additional POISONED stack at the start of each turn. On `CHARGED`: electrical discharge shatters the vine eruption; bloom creates a 1-tile ring of VINES with a 1-tile-radius lightning burst at center (10 lightning damage AoE); CHARGED is consumed. |

**Design note:** Entangling Bloom is the Floramancer's commitment spell — it costs 4 AP and demands a 3-turn cooldown in exchange for the most powerful root effect in the base roster. A ROOTED unit with a 2-turn duration cannot move for two entire turns, making it a guaranteed setup for any allied Mancer with AoE or high-damage single-target spells. The combined VINES + SPORES at center means a unit trapped directly in the bloom epicenter is simultaneously ROOTED and receiving POISONED stacks every turn. Against clustered enemies, Entangling Bloom can lock multiple units in place and begin the poison accumulation that becomes lethal once Toximancer or repeat Pollen Bursts stack higher.

---

## 4. Terrain Interaction Table

### Floramancer Spell Impact on Existing Terrain States

| Existing Terrain State | What Happens When Floramancer Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **GROUND (normal)** | Vines or spores take root normally | `VINES` or `SPORES` per spell cast | Takes spell damage (if any); ROOTED if VINES, POISONED if SPORES | Standard interaction — all Floramancer spells work optimally on bare GROUND |
| **WET** | Moisture accelerates plant growth | VINES: 5-turn duration; SPORES: wet spore cloud (conductor); OVERGROWTH: 2.5 movement cost | Takes spell damage; WET unit in VINES is ROOTED; WET + SPORES conducts lightning | Electromancer can chain through WET SPORES terrain as if WET; valuable triple-Mancer combination |
| **ON_FIRE** | Fire destroys organic matter instantly | Terrain cleared to `GROUND` (or remains ON_FIRE); plant spell fails | Takes spell damage (if any); no terrain state created | The most punishing counter-interaction for Floramancer; Pyromancer burns VINES, OVERGROWTH, and SPORES in one cast |
| **FLOODED** | Plants cannot root in standing water | Plant spell fails for VINES/OVERGROWTH; SPORES: drift into water and dissipate (no SPORES terrain created) | Takes minor spell damage; no terrain change | FLOODED zones are permanently safe from Floramancer terrain influence; Hydromancer flooding negates Floramancer's board control |
| **ICE_TILE** | Frozen ground blocks root growth | VINES: fail; SPORES: ice suspends spores (SPORES terrain created but inert — no POISONED applied while ICE_TILE state is active) | Takes spell damage | Spore terrain on ice "activates" when ice melts — Floramancer can pre-seed spores on ice tiles for a delayed trap when Cryomancer thaws |
| **FROZEN (unit status)** | FROZEN unit is immobile — ROOTED is redundant | VINES created if VINES spell; no mechanical change from ROOTED (FROZEN already skips turn) | ROOTED status applied but overridden by FROZEN priority; POISONED applies normally from SPORES | When FROZEN ends, ROOTED immediately engages if VINES is still active — back-to-back immobility is possible |
| **MUD** | Mud provides nutrients; vines grow enhanced | VINES + MUD combined state (movement cost 3.0; extreme slow) | Takes spell damage; ROOTED applied if VINES | Combined MUD + VINES is the most movement-punishing ground state Floramancer can create; Geomancer MUD + Floramancer VINES combo |
| **OBSIDIAN** | Hardened obsidian resists organic growth | No terrain change; plant spell fails | Takes spell damage | Floramancer cannot overwrite Geomancer obsidian; Osteomancer bone constructs on obsidian are also immune to vine growth |
| **TOXIC_TERRAIN** | Plant roots absorb toxins and become toxic themselves | VINES terrain created; ROOTED units on TOXIC VINES take 1 POISONED stack/turn passively | Takes spell damage + 1 POISONED stack immediately | The TOXIC VINES state is a unique interaction available only when Toximancer has pre-seeded the area; highly punishing for ROOTED units |
| **CHARGED** | Electrical charge discharges through conductive spore particles or moist vines | VINES: lightning burst (10 AoE) then VINES terrain; SPORES: chain arc then SPORES terrain | Takes spell damage + 10 lightning damage | CHARGED is consumed; the lightning discharge is an unintended benefit that punishes enemy Electromancer setups |
| **NECROTIC_ASH** | Necrotic energy poisons organic growth | VINES: 2-turn duration only (withers fast); SPORES: SPORES created but POISONED stacks are necrotic-tainted (deal 3 HP/turn necrotic instead of plant-poison) | Takes spell damage | Necrotic terrain degrades Floramancer's terrain investments significantly; avoid casting into Necromancer-poisoned ground |
| **OVERGROWTH** | Floramancer's own growth — vines layer beneath | VINES beneath OVERGROWTH (combined state); movement cost 3.0; full concealment maintained | ROOTED if entering VINES layer; miss chance still applies from OVERGROWTH cover | The Floramancer can layer VINES under its own Overgrowth Barriers for a maximum-denial approach corridor |
| **SPORES** | SPORES + VINES stacking — the Floramancer's intended combination | VINES + SPORES combined tile | ROOTED on entry; POISONED on any movement through | The core duo-state Floramancer builds toward; ROOTED + ongoing POISONED is a slow death for any unit caught inside |

### Floramancer-Specific Terrain Traits (Inherent, Always Active)

- The Floramancer ignores movement cost penalties from its own VINES and OVERGROWTH terrain — it moves through its own growth at standard cost (1 AP per tile). It is NOT immune to SPORES terrain — it can poison itself if it moves carelessly through its own clouds.
- NATURAL tiles (forest, earth/mud, vine-covered tiles) boost Floramancer spell range by +1 when the Floramancer stands on them. This is a passive positional bonus — stand in natural terrain, cast farther.
- VINES terrain the Floramancer has placed counts as a NATURAL tile classification. This means Verdant Pact Terrain Bond triggers on VINES tiles, and the Floramancer itself benefits from its own VINES spread if it steps on them (range bonus, not just immunity).

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

#### Variant A: Thorned Vine Surge (replaces Vine Surge) — +20 pts

**Description:** Replaces Vine Surge with a barbed variant. Thorned Vine Surge creates `THORN_VINES` terrain instead of standard VINES. THORN_VINES retain all standard VINES properties (movement cost 2.0, ROOTED on turn-end) but additionally deal 6 damage to any unit that passes through them — the thorns slash on movement. Any unit entering a THORN_VINES tile from any direction takes 6 physical damage. AP cost increases to 3 AP; cooldown 1 turn (no longer spammable at 0 cooldown).

**Trade-off:** More powerful terrain with active damage output, but costs 1 additional AP and can no longer be double-cast in one activation. Best for Floramancers who want their terrain to deal meaningful attrition damage rather than pure movement denial. Pairs well with Osteomancer's BRITTLE debuff — thorns hitting BRITTLE units deal +50% bonus damage per thorn impact.

#### Variant B: Spore Storm (replaces Pollen Burst) — +25 pts

**Description:** Replaces Pollen Burst with a long-range wind-dispersed spore cloud. Spore Storm targets a direction and fires a 6-tile-long cone of spores (cone width: 1 tile at origin, 3 tiles wide at max range). All tiles in the cone become `SPORES` terrain. AP cost: 4 AP; cooldown: 2 turns. The directional cone allows the Floramancer to seed a long corridor of SPORES rather than a central cluster — useful for covering entire approach lanes in a single cast.

**Trade-off:** Higher AP cost and longer cooldown for dramatically expanded coverage. Sacrifices the ability to precisely place SPORES at a specific tile in favor of sweeping multiple tiles simultaneously.

---

### Passive Traits

#### Passive A: Living Growth — +20 pts

**Description:** The Floramancer's VINES terrain slowly spreads at the start of each of its activations. Once per activation, one VINES tile (chosen randomly from all active VINES tiles on the board) extends to one adjacent GROUND tile, creating a new VINES tile. Spread is blocked by OBSIDIAN, FLOODED, and ON_FIRE terrain. This passive gives the Floramancer's terrain investment a compounding quality — early VINES placements grow into late-game denial networks without additional spell investment.

**Synergy note:** Living Growth combined with Entangling Bloom creates a bloom epicenter that slowly expands outward turn by turn. In long engagements where the Floramancer survives into the mid-game, Living Growth can make an entire map quadrant impassable.

#### Passive B: Symbiosis — +25 pts

**Description:** When the Floramancer or an allied unit stands on a VINES or OVERGROWTH tile (the Floramancer's natural terrain), they gain a passive regeneration of 3 HP at the start of each turn. This applies to all allied Mancers and all non-Mancer units. The Floramancer's own VINES terrain becomes a sustain field in addition to a denial tool — allies who position in the vine zones are healed rather than harmed (only enemies are harmed by ROOTED and POISONED effects, since allies are aware of where the traps are placed).

**Trade-off:** Effective only when the team actively fights in the Floramancer's terrain zones rather than neutral ground. Requires coordination — allies must move into VINES territory intentionally. Verdant Pact Terrain Bond (movement bonus) + Symbiosis (HP regen) on VINES tiles creates a self-sustaining line in the overgrown zone.

#### Passive C: Spore Sensitivity — +15 pts

**Description:** POISONED stacks applied by Floramancer SPORES terrain deal 4 HP/turn instead of the standard 3 HP/turn. This upgrade exclusively affects plant-origin POISONED stacks — stacks from Toximancer, Glade Archers, or Wyrmwood Striders are unaffected. Simple damage amplification for the Floramancer's primary DoT delivery method.

#### Passive D: Root Network — +20 pts

**Description:** When the Floramancer casts any plant spell, all existing VINES tiles within 3 tiles of the target tile pulse — any unit standing on those VINES tiles at the moment of cast is immediately ROOTED (1 turn) regardless of whether they already triggered the end-of-turn ROOTED check. This converts the passive end-of-turn ROOTED trigger into a reactive one: every time the Floramancer acts, the vine network tightens around all units already caught in it. The pulse has no AP cost.

---

### Stat Enhancements

#### Enhancement A: Reinforced Bark (+15 HP, +1 Armor) — +15 pts

**Description:** Max HP increases from 80 to 95; Base Armor increases from 1 to 2. The Floramancer remains fragile but can survive one additional hit before entering critical HP range. The armor increase is particularly meaningful against ranged unit damage — Crossbow Corps and Glade Archers lose one effective point of damage per shot against this upgraded Floramancer.

#### Enhancement B: Deep Roots (+1 Move Range) — +10 pts

**Description:** Move Range increases from 4 to 5 tiles per activation. The Floramancer is already relatively mobile; this upgrade allows it to seed terrain across a wider front in the early turns before the opponent closes distance. Most valuable on open maps where terrain must be established quickly before enemy formations advance.

---

### Signature Ability

#### Signature: The Verdant Surge — +40 pts

| Field | Value |
|---|---|
| **Name** | The Verdant Surge |
| **AP Cost** | 6 AP (entire activation; Floramancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Self-centered AoE — no targeting cursor; originates from the Floramancer's position |
| **Range** | N/A (self-centered) |
| **AoE Radius** | 5 tiles in all directions |
| **Base Damage** | 0 (terrain transformation; no direct damage) |
| **Element** | Plant/Nature |
| **Effects Applied** | Every GROUND and MUD tile within 5 tiles of the Floramancer immediately becomes `VINES` terrain (movement cost 2.0; duration 5 turns). Every unit in the 5-tile radius that is not on OBSIDIAN, FLOODED, or ON_FIRE terrain is immediately `ROOTED` (2-turn duration). The Floramancer's own position and OVERGROWTH tiles are not converted (the Floramancer stands in the eye of the surge, and its existing Overgrowth Barriers remain). |
| **Special Interactions** | The Verdant Surge does NOT create SPORES terrain — it creates pure VINES only. To add SPORES to the resulting vine field, the Floramancer must follow up with Pollen Burst on subsequent activations. Against WET tiles in the radius: VINES grow on WET tiles with 6-turn duration instead of 5 (moisture bonus). Against ON_FIRE tiles: no effect (fire blocks growth). Against FLOODED tiles: no effect (water blocks rooting). Against OVERGROWTH tiles: VINES layer beneath OVERGROWTH (combined state as described in terrain table). Verdant Pact faction interaction: because The Verdant Surge creates VINES terrain (classified as natural), Terrain Bond movement bonus and regen activate on every tile the surge creates — a full 5-tile-radius natural terrain field that grants all Verdant Pact units +1 movement and passive regen simultaneously. |

**Design note:** The Verdant Surge is the Floramancer's "this is what the garden was always building toward." It roots every unit on the field in a 5-tile radius simultaneously — including enemy Mancers — turning the mid-game board into an impassable thicket. Cast correctly after establishing SPORES terrain through prior activations, the Surge roots enemies onto existing SPORES tiles, guaranteeing POISONED stacks accumulate during the turns they cannot move. This is the setup for the entire Floramancer + Toximancer closing sequence.

---

## 6. Faction Synergy

### Best Pairing: The Verdant Pact

The Verdant Pact is the Floramancer's natural and most synergistic home. The Terrain Bond faction trait grants +1 movement and passive HP regeneration on **natural tiles**, and VINES tiles created by the Floramancer are explicitly classified as natural terrain. This creates a foundational loop: the Floramancer plants VINES, converting neutral ground to natural terrain; Verdant Pact units advance through that terrain gaining the Terrain Bond movement bonus and regenerating HP.

**Specific Verdant Pact interactions:**

| Mechanism | Effect |
|---|---|
| VINES tiles created by Floramancer | Count as natural terrain — Terrain Bond triggers movement bonus (+1) and regen on VINES tiles for all Verdant Pact units |
| Thornback Sentinels advancing through VINES | Gain Terrain Bond regen (3 HP/turn) while enemies entering the same VINES are ROOTED — asymmetric terrain: ally sustains, enemy freezes |
| Rootwarden (T2 Chaff) Entrenching | Rootwarden entrenching generates a natural tile beneath itself; this natural tile is adjacent to existing VINES and can trigger Floramancer's passive range bonus if the Floramancer stands nearby |
| Glade Archers in OVERGROWTH | Glade Archers fire from cover without accuracy penalty (their base trait); inside Overgrowth Barrier, they also benefit from the 25% miss-chance cover — effectively double-concealed |
| Wyrmwood Striders (T2 Ranged) | Leave Spore Trails on tiles moved through (1 POISONED stack on enemies); combined with Floramancer SPORES terrain, an enemy advancing through a Wyrmwood trail into a Pollen Burst zone receives 2 POISONED stacks per tile of movement — rapid poisoning without the Floramancer spending AP |
| The Verdant Surge Signature + Terrain Bond | The Surge creates VINES across a 5-tile radius, converting the entire zone to natural terrain simultaneously. Every Verdant Pact unit in the zone immediately gains Terrain Bond movement bonus and regen. This is the highest single-activation Terrain Bond activation possible in the game — the Floramancer creates natural terrain for an entire formation in one cast. |

**Critical ruling — what Floramancer VINES do NOT provide to Verdant Pact:** The Terrain Bond regen on VINES tiles applies only to non-Mancer Verdant Pact units. Mancers do not benefit from Terrain Bond regen (it is a faction infantry trait). The movement bonus does apply to all units, including Mancers.

### The Gilded Throne — Functional But Suboptimal

Gilded Throne Iron Discipline (immunity to Panic and Charm) does not interact with any Floramancer terrain state. VINES ROOTED and SPORES POISONED are not morale-based effects — they are physical/elemental effects. Iron Discipline provides no protection against the Floramancer's kit. This means Gilded Throne units are fully vulnerable to vine rooting and spore poisoning exactly like any other faction. The Floramancer works in a Gilded Throne warband as a terrain control specialist, but the faction provides no amplifying synergy.

Conscript Spearmen gain the Terrain Bond movement bonus when advancing through VINES tiles... except they do not: Terrain Bond is a Verdant Pact trait only. Gilded Throne units receive no benefit from VINES terrain beyond the Floramancer's Symbiosis passive (if taken), which applies faction-agnostically.

### The Ashen Covenant — Interesting but Conflicted

The Ashen Covenant relationship with the Floramancer is mechanically awkward. OVERGROWTH terrain created by the Floramancer is destroyed by Necrotic spells (necrotic energy withers organic matter). A Necromancer ally who casts Necrotic Eruption or Necrotic Bolt into the Floramancer's Overgrowth Barrier zones destroys them. The two Mancers' terrain types are naturally opposed.

However, Grave Husks (Covenant Chaff) regenerate HP in Poisoned terrain — SPORES tiles that the Floramancer creates apply POISONED status to units, and if Husks advance through SPORES territory, the POISONED terrain stat applies to them as well. However, Grave Husks' Deathless Ranks regen is from standing in POISONED terrain, not from having POISONED status applied — the SPORES tile makes the terrain "poisoned" in state, not a `TOXIC_TERRAIN` classification. This is a borderline interaction: Husks advancing through SPORES clouds are in "spore-clouded terrain" which may or may not qualify as POISONED terrain for Husk regen. **Ruling: SPORES terrain does NOT qualify as POISONED terrain for Deathless Ranks Husk regen.** The Husk regen triggers only on `TOXIC_TERRAIN`, `ON_FIRE`, and `CORRUPTED` classifications.

The Floramancer in an Ashen Covenant warband is functional but gains nothing from the faction's signature mechanics.

---

## 7. Combo Chains

### Combo 1 — The Strangling Garden (Floramancer + Toximancer) [PRIMARY]

This is the Floramancer's most natural and devastating two-Mancer combination, exploiting the specific interaction between plant-origin spores and venom amplification.

**Step-by-step execution:**

1. **Turn N, Floramancer activates:** Cast Pollen Burst on the primary enemy approach corridor (2 AP). Cast Vine Surge on the far edge of the SPORES zone (2 AP). Move 2 tiles toward safety (2 AP). Result: SPORES terrain (3 turns) + VINES terrain (4 turns) covering the enemy approach.
2. **Turn N+1, enemy units advance:** Any enemy moving through the approach takes 1 POISONED stack per SPORES tile crossed. Units that reach the VINES tile are ROOTED.
3. **Turn N+1 or N+2, Toximancer activates:** Toximancer casts Venomous Ground on the SPORES zone. **Virulent Cloud interaction:** Toximancer's venom amplifies spore toxicity — SPORES terrain in the Venomous Ground zone becomes `VIRULENT_SPORES` (hybrid state). Units in VIRULENT_SPORES receive POISONED stacks at 2 per turn (instead of 1) and the stacks are venom-enhanced (3-turn duration instead of 2-turn). ROOTED units on VIRULENT_SPORES tiles begin accumulating 2 stacks per turn: at cap (5 stacks) in 2.5 turns of standing still.
4. **Turn N+3, closing:** ROOTED units at 4-5 POISONED stacks are DEBILITATED (-1 move, -1 spell range). Toximancer confirms kills; Floramancer seeds additional VINES to prevent escape.

**Why this works:** ROOTED units cannot escape the VIRULENT_SPORES. The Floramancer physically prevents movement while the Toximancer escalates poison stacks to the DEBILITATED threshold. Any unit caught in this combo at turn 3 is either dead or stat-reduced to the point of being non-threatening without a team heal or poison cleanse.

---

### Combo 2 — Root and Freeze (Floramancer + Cryomancer)

**Setup:** Floramancer uses Entangling Bloom to ROOT a cluster of enemies (2-turn ROOTED).
**Execution:** Cryomancer targets the ROOTED cluster with a freeze spell — ROOTED units cannot move to break out; they stand in the freeze zone for the full cast window.
**Result:** ROOTED units become FROZEN (combined immobility: FROZEN skips turn, ROOTED was already preventing movement). FROZEN units are SHATTER-vulnerable — physical follow-up deals ×2.5 damage.

**Tactical outcome:** The 2-turn ROOTED from Entangling Bloom guarantees the Cryomancer's freeze applies to stationary targets, maximizing the FROZEN duration and SHATTER vulnerability window. Osteomancer physical construct attacks or Gravimancer CRUSH spells can then SHATTER frozen units for massive burst. A full Floramancer + Cryomancer + Osteomancer three-Mancer combo can eliminate an entire infantry cluster in two activations.

---

### Combo 3 — The Wet Garden (Floramancer + Hydromancer)

**Setup:** Hydromancer applies WET terrain to a zone; Floramancer casts Vine Surge into the WET tiles.
**Result:** Vines on WET terrain persist 5 turns instead of 4 and have movement cost 2.5 (waterlogged growth). WET VINES tiles also retain their conductivity — Electromancer chains still propagate through them.

**Tactical outcome:** The Floramancer extends its terrain investment by 1 turn per VINES cast when Hydromancer pre-floods the zone. A WET VINES zone is simultaneously a ROOTED trap, a movement penalty zone, and an Electromancer chain conductor — three threat vectors from two Mancers. Adding Pollen Burst to WET VINES creates a conductive poison garden that also chains Electromancer arcs.

---

### Combo 4 — Thorn and Bone (Floramancer + Osteomancer)

**Setup:** Floramancer uses Thorned Vine Surge (Variant A) to create THORN_VINES across a zone. Osteomancer applies BRITTLE debuff to enemy units in that zone.
**Result:** Enemy units attempting to move through THORN_VINES take 6 physical damage per tile. With BRITTLE active, each thorn damage instance deals +50% damage = 9 per tile traversal. A unit moving 3 tiles through THORN_VINES with BRITTLE takes 27 physical damage just from the terrain.

**Tactical outcome:** BRITTLE enemies are terrorized by any physical damage source, and THORN_VINES creates a zone of continuous physical damage on every tile entered. The combination makes the THORN_VINES zone essentially impassable for BRITTLE units — they cannot traverse it without taking enough damage to consume BRITTLE and likely reach low HP.

---

## 8. Counters and Weaknesses

### What Shuts Down the Floramancer

**Pyromancer fire coverage:** ON_FIRE terrain instantly destroys VINES, SPORES, and OVERGROWTH. A Pyromancer that seeds fire across the Floramancer's terrain investment burns down turns of AP in seconds. Floramancer + Pyromancer opponents are the hardest possible matchup. The Floramancer's only real counter is keeping its terrain far enough from enemy fire range that it can't be burned before units step into it.

**FLOODED terrain:** The Floramancer cannot plant VINES or OVERGROWTH on FLOODED tiles. An enemy Hydromancer flooding the central approach prevents the Floramancer from seeding that area at all. If the Floramancer's vines depend on a specific corridor, Flood Zone invalidates that corridor in one 5-AP cast.

**Fast aggression:** The Floramancer needs time to establish terrain. An opponent that activates fast-moving Mancers (Aeromancer, Faunamancer beasts) and reaches the Floramancer before VINES are planted exposes its 80 HP and 1 armor to focused melee pressure it cannot weather.

**Dispel and cleanse:** Units with access to ROOTED removal (Aeromancer wind, Geomancer earth) or POISONED cleanse (Hydromancer Mending Current, Photomancer Sunburst) can undo the Floramancer's kit from outside the vine zone. The Floramancer has no answer to reliable cleanse loops beyond stacking multiple terrain states simultaneously and overwhelming the cleanse rate.

**Silenced Floramancer:** A Silenced Floramancer cannot plant VINES or SPORES. Its entire turn becomes useless — it can only move. Sonimancer Silence and Psychomancer Silence are the most dangerous status effects against a Floramancer because, unlike burst Mancers who can at least use movement aggressively, the Floramancer with no spells is simply wasted AP.

---

## 9. Temperature Interaction Notes

Plants are thermally neutral — no Floramancer spell directly modifies temperature. However, the SPORES terrain and ROOTED status create unique temperature interactions when combined with external heat or cold sources. The Floramancer's temperature role is indirect but powerful: it controls enemy mobility, and mobility is how enemies escape thermal hazards.

### HOT SPORES (SPORES + BURNING Terrain)

If a Floramancer creates SPORES terrain and a Pyromancer subsequently heats the area (Scorched Earth or Conflagration Wave on nearby tiles), units moving through SPORES in a HOT area take POISONED stacks AND gain +5 temperature per turn from the heated spore cloud. The spores absorb ambient heat and carry it, making the cloud a dual-threat zone: poisonous AND warming. A unit spending multiple turns in HOT SPORES territory will accumulate POISONED stacks while being pushed toward OVERHEATED.

This is an emergent two-Mancer combination (Floramancer + Pyromancer, available in any faction). Neither Mancer needs to coordinate precisely — once SPORES terrain and BURNING terrain coexist in the same zone, the HOT SPORES state activates automatically.

### COLD SPORES (SPORES + FROZEN Terrain)

If a Cryomancer freezes tiles adjacent to SPORES terrain, units in the SPORES zone lose -5 temperature per turn (cold air condenses the spore cloud into a denser, colder toxic mist that slows movement AND cools). This makes SUPERCOOLED + POISONED a realistic combined status achievable from just two Mancers. A unit caught in COLD SPORES terrain is simultaneously:
- Taking POISONED stacks (3 HP/turn per stack)
- Losing -5 temperature per turn toward SUPERCOOLED (SLOWED + BRITTLE modifier)

At SUPERCOOLED with 3+ POISONED stacks, the unit is SLOWED, BRITTLE (vulnerable to physical hits at ×1.5), and taking 9+ HP/turn from poison. The Floramancer + Cryomancer pairing is stronger than it first appears.

### ROOTED on BURNING — The Primary Temperature Contribution

Floramancer's most powerful temperature play is not a direct effect — it is trapping enemies on BURNING tiles. ROOTED units cannot escape BURNING terrain. A ROOTED enemy on a BURNING tile gains +10 temperature per turn with no ability to move away. Two turns of being ROOTED on BURNING terrain = +20 temperature minimum. Three turns = +30 temperature minimum.

If the enemy starts WARM (+20), three turns of ROOTED-BURNING = +30, reaching +50 (HOT → moving toward OVERHEATED at +61). If they were already HOT (+40), one turn of ROOTED-BURNING pushes them to +50, and two turns reaches OVERHEATED (+61) and the BURNING DoT begins stacking.

Entangling Bloom (2-turn ROOTED) placed in an area where Pyromancer has established BURNING tiles is a guaranteed +20 temperature application with no additional Floramancer AP investment. This is a powerful zone-denial combination with any Mancer that creates BURNING terrain — Pyromancer, Thermomancer, or even Electromancer-triggered Firestorm Burst.

### Floramancer Thrives in Cold Environments

VINES and natural terrain have no temperature penalty, so a Cryomancer's frozen-field strategy does not degrade Floramancer's terrain the way it harms BURNING terrain (fire is extinguished by ice; VINES can be suspended but not destroyed by cold). Floramancer + Cryomancer is a surprisingly viable pairing:

- Cryomancer drives enemy temperature down toward SUPERCOOLED or FROZEN SOLID
- Floramancer ROOTS enemies in the cold zone, preventing escape
- ROOTED enemies in cold terrain cannot move to warmer tiles, locking in the temperature descent
- Enemies frozen to FROZEN SOLID while ROOTED are helpless targets for physical finishers (Geomancer, Osteomancer)

The Floramancer's VINES on ICE_TILE have a pre-seed trap value as well — SPORES placed on frozen tiles lie dormant (no POISONED applied while ICE_TILE is active) but activate when the ice thaws. A Floramancer can plant SPORES on ice tiles and wait — when the Cryomancer thaws or melts the ice (or the ice expires naturally), the dormant SPORES cloud activates, applying POISONED stacks to any unit standing on what was, a moment ago, safe frozen ground.

---

## 10. Augmentation Spell

### Verdant Embrace

**AP Cost:** 2 | **Range:** 3 tiles | **Targeting:** Single allied unit | **Cooldown:** 3 turns

Channels rapid plant growth through an allied unit, weaving vines through their physiology — rooting them in place and turning their position into a living denial zone.

**Effects (3 turns):**
- Ally is ROOT-tethered on their current tile — immune to all displacement (pushes, pulls, knockbacks)
- Ally regenerates 2 HP per turn (plant nutrients cycling through them)
- Each turn, 1 adjacent tile becomes a GROWTH tile (vines spread outward; direction chosen by the active player)
- GROWTH tiles apply SLOWED to enemies who enter them; allied units on GROWTH tiles gain +1 HP regen per turn
- The ally may spend 1 AP to shed the vines early, ending the buff but leaving all created GROWTH tiles in place on the board

**Tactical intent:** Defensive anchor with organic area denial that grows over time. The ROOT tether prevents displacement — countering Aeromancer, Gravimancer, and Hydromancer push/pull — but it is entirely a choice: the ally selected this position. GROWTH tile spread means standing still for 3 turns creates a 3-tile vine zone, making the unit progressively harder to approach. The shed mechanic is design-critical — it prevents the buff from becoming a trap if circumstances change. Shedding after 2 turns and leaving 2 GROWTH tiles behind is a valid play: take mobility back, keep the denial zone.

**Notable interactions:** GROWTH tiles + Floramancer's own pollen/poison spells create a compounding zone — enemies are SLOWED traversing the vines and receiving DoT simultaneously. Tidal Blessing (Hydromancer) regen stacks additively with Verdant Embrace regen on the same ally (2 + 2 = 4 HP per turn), forcing opponents to prioritize eliminating the buffed unit before attrition wins.

*End of Floramancer design document.*

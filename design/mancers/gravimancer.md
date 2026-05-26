# Gravimancer — Full Design Document

---

## 1. Tactical Identity

The Gravimancer is a force-multiplier and positional executioner — a Mancer that does not deal its own killing blows but creates the conditions in which allies deliver them at maximum efficiency. Its domain is gravity itself: pulling enemies toward hazards rather than chasing them, compressing formations into kill zones, and imposing weight-based status effects that either strand units in place or leave them dangerously exposed to displacement. The Gravimancer does not win by dealing the highest damage in any given activation; it wins by ensuring every other Mancer on the team deals more damage than they otherwise could. A Gravity Well placed in a chokepoint pulls enemies into a tight cluster turn after turn — where a Sonimancer's cone, a Cryomancer's mass freeze, or an Electromancer's chain arc becomes devastating. GRAVITATIONAL COLLAPSE, its signature ability, is one of the most dramatic board-state transformations available: the Gravimancer does not care who is near the center, only that the resulting cluster becomes everyone else's problem.

Playing the Gravimancer well requires understanding the entire board geometry before committing to a pull direction. Gravity Well is a persistent field — placing it incorrectly pulls allies toward hazards as readily as enemies. HEAVY status is a double-edged debuff: it protects the target from knockback (useful when applied to allies near drop-offs), but makes it lethal when combined with an Aeromancer wind push or a Geomancer elevation trap. The Gravimancer thrives in maps with elevated terrain, fall hazards, and dense enemy formations. It struggles when the opponent spreads wide, avoids clustering, and denies the board geometry the Gravimancer needs. Its biggest skill expression is learning to read the opponent's planned movement paths and placing Gravity Wells where the pull will gather enemies without also entrapping allies.

**Primary win condition:** The Gravimancer wins by compressing enemy formations into states where allied AoE and chain spells deal damage they otherwise could not — a 3-tile scatter becomes a 1-tile cluster, a single Sonimancer cone hits four units instead of one, and a Cryomancer Blizzard Field catches everyone simultaneously. Secondary win condition: HEAVY + elevation + Aeromancer displacement sequences that convert positional advantages into lethal fall damage.

**Core weakness:** The Gravimancer's entire kit depends on the board having hazards to pull into. On open, flat maps with no elevated terrain, no fire zones, and no environmental threats, PULL and Gravity Well are nuisance effects rather than kill tools. An opponent who spreads wide and avoids clustering denies the compression value of GRAVITATIONAL COLLAPSE. The Gravimancer also has the second-lowest base damage in the roster — it relies entirely on ally follow-up for kills. If the Gravimancer's team has no AoE follow-up available, GRAVITATIONAL COLLAPSE pulls everyone to the same point and then... nothing decisive happens.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 90 | Below average; compensates with positional control over proximity |
| **Move Range** | 4 tiles per activation | Above average mobility — needs to reposition for optimal pull angles |
| **Base Armor** | 1 | Minimal physical mitigation; Gravimancer is a positioning piece, not a tank |
| **Spell Range** | 5 tiles (base) | Medium; most pull effects are placed at range rather than on the Gravimancer itself |
| **AP per Activation** | 6 | Standard; movement costs 1 AP per tile |
| **Element** | Gravity | All base spells deal Physical/Force damage; gravity interactions are unique to this element |

**AP budget example:** With 6 AP, the Gravimancer can move 2 tiles (2 AP) and cast Gravity Well (3 AP) plus a Quick pull spell (1 AP), or move 1 tile and spend 5 AP on CRUSH (its heaviest single-target spell), or move 4 tiles and spend 2 AP on a PULL to redirect a unit mid-pursuit.

---

## 3. Base Spell Kit

The Gravimancer's four base spells cover distinct gravitational functions:
- **Graviton Bolt** — repeatable single-target damage + HEAVY application
- **Pull** — targeted displacement toward a chosen point; no damage, pure repositioning
- **Gravity Well** — persistent terrain field that drags units toward its center each turn
- **Crush** — single-target compression burst; maximum single-hit damage in the Gravimancer's kit

---

### Spell 1: Graviton Bolt

| Field | Value |
|---|---|
| **Name** | Graviton Bolt |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (projectile — travels in a line) |
| **Range** | 5 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 15 |
| **Element** | Gravity / Physical |
| **Effects Applied** | Deals 15 physical damage. Applies `HEAVY` status to hit unit (cannot be displaced by wind or water; fall damage ×2; movement range –0 from HEAVY itself, but the ×2 fall damage on displacement is the primary threat; 2-turn duration). Target tile beneath the unit is briefly compressed — if the tile is `ELEVATED`, the hit unit is "pinned" to that elevation (cannot voluntarily move down to lower ground this turn; must spend 1 additional AP to break the gravitational anchor). |
| **Special Interactions** | See terrain interaction table in Section 4. If the target is already `HEAVY`, a second Graviton Bolt does not re-apply (HEAVY is a non-stacking status; instead, deal 8 bonus compression damage — the target resists the pull but takes internal pressure damage). If target is `WEIGHTLESS`: WEIGHTLESS is immediately cancelled by HEAVY (the two cancel each other; unit returns to normal weight state and takes 10 impact damage from the abrupt weight restoration). |

**Design note:** Graviton Bolt is the Gravimancer's workhorse — cheap, no cooldown, and the HEAVY status it applies is a setup for every other piece of the kit. A HEAVY unit hit by Aeromancer's Gale Push takes double fall damage. A HEAVY unit on an elevated platform that the Gravimancer later uses Crush on takes fall damage as if it had dropped from a higher elevation. Two Graviton Bolts in a row (4 AP) pressure the enemy into acting cautiously near any elevation or hazard. The no-cooldown nature means the Gravimancer can apply HEAVY to multiple targets across multiple turns without burning its heavier spells.

**Spell answers YES to (design rule check):**
1. Applies unit status (HEAVY — fall damage amplifier) — YES
2. Applies terrain interaction (elevation pin) — YES
3. Synergizes with Aeromancer (HEAVY + wind push = double fall damage), Geomancer (HEAVY on elevated platforms) — YES
4. Skill expression: HEAVY application before a displacement combo; target priority for maximum fall damage potential — YES

---

### Spell 2: Pull

| Field | Value |
|---|---|
| **Name** | Pull |
| **AP Cost** | 2 AP |
| **Cooldown** | 1 turn |
| **Targeting Type** | Single Target (targeted displacement — pulls one unit up to 3 tiles toward a chosen destination point within the Gravimancer's line of sight) |
| **Range** | 6 tiles (to target unit); target is moved up to 3 tiles toward the chosen destination |
| **AoE Radius** | N/A |
| **Base Damage** | 0 (no direct damage — pure displacement) |
| **Element** | Gravity |
| **Effects Applied** | Target unit is displaced up to 3 tiles toward the chosen destination point. If the path is blocked by a wall or unit, the pulled unit stops at the last unoccupied tile before the obstacle (collision damage: 6 HP per blocked tile × remaining pull distance). The pulled unit cannot resist the displacement unless it has `HEAVY` status — HEAVY units resist Pull; the Gravimancer must spend an additional 2 AP (total 4 AP) to Pull a HEAVY unit the same distance. Allies can be pulled with their consent (tactical repositioning is valid). |
| **Special Interactions** | Against a `FROZEN` unit: Pull does not trigger SHATTER (gravitational pull is not classified as physical sonic impact). Against a unit on `ICE_TILE`: the pull through ice creates a slip continuation — the pulled unit slides 1 additional tile past their destination on the ice, the same as a voluntary movement slip check. Against a unit with `ROOTED` status: Pull cannot move the unit at all — ROOTED prevents all forced displacement. Against a `WEIGHTLESS` unit: Pull range on WEIGHTLESS units is doubled (6 tiles of pull instead of 3) — lighter units are dramatically easier to reposition gravitationally. |

**Design note:** Pull is the Gravimancer's primary tactical tool. Unlike Aeromancer's directional Gale Push (which moves in a fixed cardinal direction), Pull moves a target toward a specific chosen destination — meaning the Gravimancer can pull an enemy backward, sideways, diagonally, or onto any tile within the 3-tile pull range. This precision displacement is more versatile than any other repositioning spell in the roster. Two Pull casts in two activations can relocate an enemy 6 tiles from their starting position without the Gravimancer closing the gap at all. The 1-turn cooldown prevents consecutive pulls that would teleport units off the map edge, but allows reliable every-other-turn repositioning.

**Spell answers YES to (design rule check):**
1. Moves a unit (primary function) — YES
2. Creates fall damage potential (pull toward edge) — YES
3. Synergizes with every trap-setting Mancer (pull into fire, ice, toxic zones) — YES
4. Skill expression: destination selection to maximize hazard exposure; WEIGHTLESS target prioritization — YES

---

### Spell 3: Gravity Well

| Field | Value |
|---|---|
| **Name** | Gravity Well |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Ground Target — places a persistent terrain field at a chosen tile |
| **Range** | 5 tiles (to center of well) |
| **AoE Radius** | 3-tile effect radius (the well pulls toward its center from up to 3 tiles away) |
| **Base Damage** | 0 at placement; 8 per turn to any unit standing on the center tile of the well |
| **Element** | Gravity |
| **Effects Applied** | Creates a `GRAVITY_WELL` terrain field (persists 3 turns). At the start of each enemy turn, all enemy units within 3 tiles of the well's center are pulled 1 tile toward the center (involuntary gravitational drag). Ally units within range are not affected unless the Gravimancer chooses to extend the well to allies (costs 1 additional AP at cast time). A unit on the center tile takes 8 Force damage per turn from gravity compression. `HEAVY` units within the well's radius resist the pull — they require the well to "spend" extra force: HEAVY units are only pulled on every other turn tick (alternate turns instead of every turn). `WEIGHTLESS` units within the range are pulled 2 tiles toward center instead of 1 per turn (gravity affects them more dramatically). |
| **Special Interactions** | Against `FLOATING` terrain or units with `UPDRAFT` (Aeromancer): WEIGHTLESS from UPDRAFT is partially overridden — units in UPDRAFT are pulled 1 tile per turn instead of being immune (the Gravity Well is powerful enough to overcome minor UPDRAFT; only Aeromancer's full UPDRAFT zone reduces the pull). Against multiple Gravity Wells active simultaneously: units within range of both wells are pulled toward the geometrically closer well (no stacking — one pull direction per unit per turn). Against units already on the center tile: they cannot be pulled further; they take the 8 HP compression damage and must spend 2 AP (instead of 1) to move off the center tile against the gravity field. |

**Design note:** Gravity Well is the Gravimancer's defining area-control tool. Unlike every other terrain field in the game (which applies damage or status passively), Gravity Well actively repositions units over time. Placed in a doorway or chokepoint, it pulls every enemy who approaches toward the center, clustering them in a predictable location for allied AoE follow-up. The 3-turn duration is long enough to persist across multiple activations, giving the Gravimancer time to set up other spells while the well does its work passively. The critical design tension: Gravity Well does not discriminate between ally and enemy without the optional upgrade, meaning the Gravimancer must place it where allied movement paths do not intersect its pull radius. The 3-turn cooldown (starting after the well expires) means only one well can be active at a time.

**Spell answers YES to (design rule check):**
1. Creates a persistent terrain field with ongoing mechanical effect — YES
2. Moves units each turn (involuntary pull toward center) — YES
3. Applies terrain state (GRAVITY_WELL zone) — YES
4. Synergizes with Sonimancer (clusters enemies for cones), Cryomancer (clusters for mass freeze), Electromancer (clusters for chain arc) — YES
5. Skill expression: well placement relative to allied AoE positions; hazard selection for center tile — YES

---

### Spell 4: Crush

| Field | Value |
|---|---|
| **Name** | Crush |
| **AP Cost** | 5 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Single Target |
| **Range** | 4 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 45 (primary target); 12 splash to all units in 1 tile adjacent to target |
| **Element** | Gravity / Physical |
| **Effects Applied** | Primary target takes 45 Force damage. The target is compressed into its tile — it cannot move or be moved by any displacement effect for 1 turn (`ROOTED` effect from gravitational pinning; this specific ROOTED cannot be cleared by Aeromancer wind, as it is gravitational not physical). Adjacent units within 1 tile take 12 splash force damage from the compression shockwave. If the target has `HEAVY` status, Crush damage increases to 60 (gravity amplifies on an already-weighted unit). If the target is on an `ELEVATED` tile, the Crush forces them down — they are pulled off the elevation and land on the nearest lower tile, taking fall damage (elevation level × 8 HP) in addition to Crush damage. |
| **Special Interactions** | Against a `FROZEN` unit: Crush triggers SHATTER (45 × 2.5 = 112 HP on base; 60 × 2.5 = 150 HP with HEAVY; Crush is classified as physical/force damage and fully triggers the SHATTER multiplier). This is the single highest-damage single-target SHATTER combo available from a 5 AP investment. Against `WEIGHTLESS` unit: Crush removes WEIGHTLESS instantly (gravity overrides) and the unit takes 20 additional impact damage from the abrupt weight restoration before the standard Crush damage is applied. Against a unit on a `GRAVITY_WELL` center tile: Crush damage is increased by 15 (to 60 base; 75 with HEAVY) — the existing well amplifies the compression force. Against `MUD` terrain: Crush on a unit standing in MUD sinks them further — the tile becomes `QUICKSAND` (unit cannot move for 1 additional turn beyond the Crush root; requires 3 AP to escape). |

**Design note:** Crush is the Gravimancer's "I need this unit to die" spell. At 5 AP, it costs nearly an entire activation and has a 3-turn cooldown — this is not a casual cast. But 45 base force damage with a guaranteed ROOTED effect, AoE splash, and HEAVY-enhanced damage (60 HP) makes it a confirmed kill-range tool on any unit below about 75 HP. The SHATTER interaction (45 × 2.5 = 112 HP) is exceptional — almost no un-upgraded Mancer in the roster survives Crush on a FROZEN target. The fall-damage dismount from elevated positions makes it a counter to Geomancer elevation setups: a HEAVY unit perched on a Geomancer platform can be Crushed off the platform, taking both Crush damage and fall damage simultaneously.

**Spell answers YES to (design rule check):**
1. Applies unit status (ROOTED from gravitational pin) — YES
2. Moves a unit (dismounts from elevation, takes fall damage) — YES
3. Applies terrain state (QUICKSAND on MUD interaction) — YES
4. Synergizes with Cryomancer (SHATTER combo — the highest-damage SHATTER available), Aeromancer (HEAVY setup before dismount) — YES
5. Skill expression: HEAVY setup before Crush for +15 damage; FROZEN combo timing; elevation positioning exploitation — YES

---

## 4. Terrain Interaction Table

### Gravity Spell Impact on Existing Terrain States

The following describes what happens when any Gravimancer spell strikes a tile in the listed terrain state. All Gravimancer spells are Gravity/Force element; these interactions apply universally unless a spell's individual entry overrides them.

| Existing Terrain State | What Happens When Gravity Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **Normal (GROUND)** | Gravitational compression ripples through the ground | `GROUND` (churned slightly; no state change) | Takes spell damage + `HEAVY` (if spell applies it) | Standard gravity effects; no terrain transformation on base GROUND |
| **ELEVATED** | Gravitational pull destabilizes the elevation anchor | `ELEVATED` (unchanged terrain; unit displaced downward by Crush only) | On Crush: unit pulled off elevation, takes fall damage × level | Elevation remains for future use; only the unit is displaced, not the terrain |
| **MUD** | Gravity compresses wet earth into a sinkhole | `QUICKSAND` (unit cannot move for 1 additional turn; 3 AP to escape; 2-turn duration) | Takes spell damage + `ROOTED` from quicksand | QUICKSAND state ends after 2 turns, returning to MUD; Geomancer can compact it to GROUND |
| **WET** | Gravitational pressure compresses water | `WET` (unchanged; water redistributes) | Takes spell damage; HEAVY applied | No terrain state change; water resists compression unlike solid matter |
| **FLOODED** | Gravity Well placed in FLOODED zone creates a vortex | `FLOODED` (central tile becomes a gravity vortex; units in FLOODED zone are pulled toward it at 2 tiles/turn instead of 1) | Takes spell damage | Vortex in FLOODED water amplifies pull speed; effective trap for aquatic setups |
| **ON_FIRE** | Gravitational compression forces fire downward | `ON_FIRE` (unchanged; fire spreads normally) | Takes spell damage + `BURNING` from fire contact during pull | No terrain state change from gravity alone; fire continues to spread |
| **ICE_TILE** | Gravity compresses ice, creating a denser surface | `ICE_TILE` (unchanged; slip check still applies) | Takes spell damage + slip check triggered by compression | Pulled units moving through ICE_TILE during gravitational displacement still make slip checks |
| **CHARGED** | Gravitational force through electrical field arcs | `CHARGED` (unchanged; discharge adds) | Takes spell damage + 15 Lightning arc damage from forced contact with CHARGED surface | Arc chain to adjacent units on WET terrain if applicable; electrical and gravitational discharge simultaneously |
| **TOXIC_TERRAIN** | Gravity forces unit deeper into toxic matter | `TOXIC_TERRAIN` (unchanged) | Takes spell damage + 2 stacks `POISONED` (forced exposure rather than 1 stack) | Double POISONED application on pulls/Crush into TOXIC_TERRAIN; gravity magnifies exposure |
| **OBSIDIAN** | Gravity cannot break obsidian's crystalline structure | `OBSIDIAN` (unchanged) | Takes spell damage; no terrain state change | Obsidian resists gravitational deformation; impassable as before |
| **PERMAFROST** | Gravitational pressure fractures frozen mud | `MUD` (permafrost cracked open; 2-turn duration) | Takes spell damage + `CHILLED` | PERMAFROST broken — tile thaws to MUD; Cryomancer can re-freeze if needed |
| **VINES / OVERGROWTH** | Gravitational force compresses plant matter | `GROUND` (vines crushed flat by compression; Floramancer barriers destroyed) | Takes spell damage + `ROOTED` removed if applicable | Gravity clears vine barriers — counter to Floramancer zone control |
| **STEAM_CLOUD** | Gravity compresses steam downward | `WET` (steam forced to condense; cloud collapses) | BLINDED removed; unit takes 6 heat damage from condensing steam contact | Steam cloud eliminated; converts to wet residue tile |

### Terrain States Beneficial to the Gravimancer

| State | Benefit |
|---|---|
| `ELEVATED` tiles | Every elevation level is a fall damage multiplier the Gravimancer can exploit with Pull or Crush; elevated enemy positions become kill setups rather than advantages |
| `GRAVITY_WELL` center tile | Crush on units at the center tile deals +15 bonus damage; the combined compression amplifies the effect |
| `MUD` (Geomancer or Hydromancer-created) | Gravitational Crush converts MUD to QUICKSAND — additional ROOTED extension at no extra AP cost |
| `FLOODED` zones | Gravity Well placed in FLOODED zones increases pull speed to 2 tiles/turn — accelerates enemy compression into the vortex |

### Terrain States Hazardous to the Gravimancer

| State | Hazard |
|---|---|
| `CHARGED` tiles | No defensive advantage; the Gravimancer takes normal electrical arc damage and has no resistance to CHARGED terrain |
| `ON_FIRE` tiles | Gravimancer has no fire immunity; 5 HP/turn DoT compounds its moderate HP pool |
| `VINES / OVERGROWTH` | ROOTED status prevents the Gravimancer from repositioning for optimal pull angles — Floramancer is the Gravimancer's hardest counter because its kit requires precise movement to set up |
| `ICE_TILE` | Slip checks apply to the Gravimancer like any other unit; a slip on a pull approach can send the Gravimancer itself into a hazard |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

Spell variants replace a base spell entirely with a more powerful or specialized version.

#### Variant A: Gravity Spike (replaces Graviton Bolt) — +20 pts

**Description:** Graviton Bolt is replaced by Gravity Spike — a heavier projectile that creates a 1-tile gravity anchor on impact. Gravity Spike deals 22 damage (up from 15) and applies `HEAVY` status. Additionally, the impact tile becomes a mini-GRAVITY_WELL for 1 turn only (1-tile pull range; pulls adjacent units 1 tile toward the anchor at the start of their next turn). This is a single-pulse compression rather than a sustained well — cheaper and more precise, but no duration. AP cost is 3 AP; cooldown is 1 turn.

**Trade-off:** Higher damage and micro-pull effect at the cost of the no-cooldown nature of Graviton Bolt. Best for Gravimancers who want to compress small groups with frequent mini-wells rather than sustaining a large Gravity Well separately.

#### Variant B: Mass Pull (replaces Pull) — +20 pts

**Description:** Pull is replaced by Mass Pull — a 2-tile AoE pull that affects all units within 2 tiles of the chosen destination, not just one. All units in the 2-tile radius are pulled up to 2 tiles (instead of 3) toward the destination. `HEAVY` units still require additional AP to move. AP cost is 3 AP; cooldown is 2 turns.

**Trade-off:** Affects multiple targets simultaneously (ideal for clustering before a Sonimancer or Electromancer follow-up) at the cost of reduced per-unit pull distance and higher AP investment. Best for warbands built around AoE follow-up combos rather than precise single-unit repositioning.

---

### Passive Traits

#### Passive A: Gravitational Mastery — +20 pts

**Description:** All Gravimancer pull effects (Pull spell, Gravity Well drag, GRAVITATIONAL COLLAPSE) have their range extended by 1 tile (Pull: 4-tile drag instead of 3; Gravity Well: 4-tile radius instead of 3; GRAVITATIONAL COLLAPSE: 6-tile radius instead of 5). Additionally, when the Gravimancer applies `HEAVY` to a unit, that unit also receives a hidden gravity mark — if the HEAVY unit is ever displaced (by any source, including ally spells), the Gravimancer is notified via UI flash and the displacement distance is treated as 1 tile longer than the actual push for fall damage calculation purposes.

**Trade-off:** Range extension with a passive fall-damage-amplification rider on HEAVY targets. Best for Gravimancers built around wide-field well control on large maps where standard 3-tile well radius covers insufficient area.

#### Passive B: Weightless Affinity — +15 pts

**Description:** The Gravimancer can apply `WEIGHTLESS` to one ally per activation at 1 AP cost (targeted status; ally floats 1 tile above ground, immune to ground terrain effects — VINES, MUD, ICE_TILE slip, TOXIC_TERRAIN ground contact, PERMAFROST movement penalty; 2-turn duration). WEIGHTLESS allies are also immune to the Gravimancer's own Gravity Well pull effect (by design — the Gravimancer controls its own floating allies precisely). WEIGHTLESS does not affect the ally's spell range or damage.

**Trade-off:** Significant terrain-immunity buff for a single ally at minimal AP cost, but the WEIGHTLESS ally becomes vulnerable to Aeromancer wind displacement at doubled range. Best in maps with heavy ground hazard terrain (TOXIC_TERRAIN, PERMAFROST fields, heavy MUD zones).

#### Passive C: Anchor — +25 pts

**Description:** The Gravimancer itself is immune to all forced displacement effects. Wind pushes (Aeromancer), water displacement (Hydromancer), Sonimancer knockback, Geomancer Earthen Smash knockback, and any other forced movement — all fail against the Gravimancer. The Gravimancer plants itself in position and cannot be repositioned against its will. However, the Gravimancer's own voluntary movement costs 1 additional AP per tile while Anchor is in effect (gravitational anchoring creates resistance to its own movement). Anchor can be toggled off at the start of the Gravimancer's activation (free action) for a full-AP-cost turn.

**Trade-off:** Total displacement immunity at the cost of reduced personal mobility. Best for Gravimancers that set up Gravity Well and then hold a fixed position while the well does the work — the Gravimancer becomes a gravity anchor itself, immovable by the opponent.

---

### Stat Enhancements

#### Stat A: Dense Core (+20 HP) — +10 pts

**Description:** Max HP increases from 90 to 110. Brings the Gravimancer to the mid-tier HP range, where it can absorb one additional burst without reaching critical HP. Most relevant in Gravimancer builds that hold a forward position near a Gravity Well center rather than operating from maximum range.

**Design note:** At 90 HP, the Gravimancer is one Geomancer Rock Throw SHATTER away from elimination if it ever gets FROZEN. 110 HP survives most single-turn burst combinations and provides a meaningful buffer in close-range Gravity Well maintenance positions.

#### Stat B: Gravitational Reach (+1 Spell Range) — +15 pts

**Description:** All Gravimancer spell ranges increase by 1 tile. Graviton Bolt: 5 → 6. Pull target range: 6 → 7 (pull distance remains 3 tiles, but the Gravimancer can target units 7 tiles away to initiate the pull). Gravity Well center: 5 → 6. Crush: 4 → 5. The Gravimancer can place and trigger all of its effects from one additional tile of safety distance.

**Design note:** Range is the Gravimancer's primary survivability tool — at 90 HP with 1 armor, every tile of additional range is a tile further from melee threats. Gravitational Reach shifts the Gravimancer from "medium-range controller" to "long-range gravity artillery," placing wells and applying HEAVY from positions where most melee Mancers cannot reach it without crossing the gravity fields it creates.

---

### Signature Ability

The Signature Ability is unlocked as an additional spell slot — it does not replace any base spell.

#### Signature: Gravitational Collapse — +40 pts

| Field | Value |
|---|---|
| **Name** | Gravitational Collapse |
| **AP Cost** | 6 AP (entire activation; Gravimancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Ground Target — designates a center point; effect radiates outward |
| **Range** | 5 tiles (to chosen center point) |
| **AoE Radius** | 5 tiles (all units within 5 tiles of the chosen center are affected) |
| **Base Damage** | Variable — equal to 8 HP per tile the unit traveled to reach the center (minimum 8; maximum 40 for a unit pulled 5 tiles) |
| **Element** | Gravity / Physical |
| **Effects Applied** | Every unit (ally and enemy alike) within 5 tiles of the chosen center point is simultaneously pulled to the center tile. Each unit takes damage equal to 8 HP × the number of tiles it traveled. All units that arrive at the center tile take an additional 10 collision damage from simultaneous impact (every unit smashes into every other unit). Units with `HEAVY` status take double travel damage (the weight amplification doubles force impact). Units with `WEIGHTLESS` status are pulled but take only half travel damage (floating units are less violently impacted). After resolution, all units at the center tile are `SLOWED` (–1 move; 1-turn duration) from the gravitational aftershock. |
| **Special Interactions** | Against FROZEN units pulled to center: Pull is classified as gravitational displacement — does NOT trigger SHATTER (same as standard Pull). However, the collision damage at the center (10 HP) IS a physical impact and DOES trigger SHATTER on FROZEN units: if a FROZEN unit arrives at the center and collides with another unit, the 10 collision damage becomes 25 (10 × 2.5 SHATTER). Against units pulled through hazard tiles: units pulled across ON_FIRE tiles receive BURNING mid-pull; pulled through TOXIC_TERRAIN receive 1 POISONED stack per tile crossed; pulled through CHARGED tiles trigger arc discharge at their last position before continuing. Against ally units within 5 tiles: allies are also pulled unless the Gravimancer purchased the "Selective Collapse" upgrade — this is a core design tension. GRAVITATIONAL COLLAPSE is double-edged. It will pull allies toward the center if they are in range. |

**Design note:** Gravitational Collapse is the Gravimancer's moment-of-decision ability. Its primary value is clustering — compressing an enemy team from a spread formation into a single tile, where every AoE spell in the Gravimancer team becomes a full-sweep hit. A Sonimancer's cone aimed at the center after Gravitational Collapse hits every unit that was pulled there. A Cryomancer's Blizzard Field centered on the collapse point catches everyone simultaneously. An Electromancer's chain arc has eight targets instead of one. The 6 AP full-activation cost and 5-turn cooldown mean this is a declared investment that shapes two or three subsequent turns. The all-or-nothing ally-pull risk is intentional and irreducible without the Selective Collapse upgrade — a Gravimancer player who triggers Collapse without verifying ally positions will pull their own team into the kill zone. This is the game's most dramatic "read the board or lose" spell.

**Synergy note:** Gravitational Collapse is explicitly designed to synergize with Sonimancer cones and Cryomancer mass freeze. After Collapse, the Sonimancer fires its cone directly into the cluster — guaranteed maximum target count. After Collapse, the Cryomancer drops Blizzard Field centered on the collapse point — mass FROZEN on every unit that was pulled in. The Gravimancer's job is to ensure allies are positioned outside the 5-tile radius before committing.

---

## 6. Faction Synergy

### Best Faction: The Gilded Throne

The Gilded Throne's Conscript Spearmen have a 1-tile melee range extension — they can attack the tile directly ahead without entering it. This pairs precisely with the Gravimancer's Pull: enemies pulled 3 tiles toward the Spearmen line are suddenly within 1-tile spear range without the Spearmen spending movement AP. The Gravimancer does the repositioning; the Spearmen execute the physical damage. HEAVY targets pulled into Spearmen range take physical melee hits — if those targets are FROZEN (Cryomancer setup), the Spearmen's physical melee triggers SHATTER.

Iron Discipline (Charm and Panic immunity) is relevant because GRAVITATIONAL COLLAPSE clusters allies and enemies alike near the center, where a CONFUSED ally (from enemy Psychomancer) would be a disaster. Iron Discipline prevents this worst-case scenario — Throne units pulled to the collapse center will not use their confusion to harm allies.

### The Verdant Pact — Terrain Bond and WEIGHTLESS

Verdant Pact Glade Archers on WEIGHTLESS (Gravimancer Passive B) bypass MUD and VINES terrain created by allied Floramancers — they can advance through Floramancer barriers without cost. This creates a separation of lanes: OVERGROWTH tiles deny enemy movement while allied WEIGHTLESS Archers cross them freely.

Thornback Sentinels ROOTED by Floramancer vines become gravitational anchors — they cannot be displaced by enemy push abilities, and Gravity Well pull does not force them to move. They hold position while the well pulls enemies toward them. The Verdant Pact's Terrain Bond regen on natural tiles means Sentinels holding inside a Gravity Well radius can sustain their HP while enemies are dragged toward them each turn.

### The Ashen Covenant — Fall Damage and Deathless Ranks

Grave Husks advance through any terrain without fear of DoT (they regen in BURNING terrain). WEIGHTLESS Husks (Gravimancer Passive B) that float over TOXIC_TERRAIN and VINES while the Gravimancer pulls enemies into those zones create a devastating asymmetry: enemies are dragged into poison ground that the Husks cross freely. The Deathless Ranks trait (deaths generate Necromancer fuel) means units pulled and killed by Gravitational Collapse contribute to Necromancer economy — every gravity kill fuels the undead advance.

---

## 7. Combo Chains

### Combo 1 — The Heavy Drop (Gravimancer + Aeromancer) [SIGNATURE]

**Mancers involved:** Gravimancer + Aeromancer

**Step-by-step execution:**

1. **Gravimancer activates:** Graviton Bolt hits target (HEAVY applied; fall damage ×2; 2-turn duration).
2. **Gravimancer continues or next turn — Aeromancer activates:** Aeromancer's Gale Push (or equivalent wind displacement) pushes the HEAVY unit 2 tiles in a chosen direction. If the direction passes an elevated drop-off, the unit falls. Fall damage is doubled by HEAVY: standard 1-level fall = 8 HP × 2 (HEAVY) = 16 HP. 2-level fall = 16 HP × 2 = 32 HP, plus full Gale Push base damage. Combined with the HEAVY status's impact damage, a 2-level fall on a HEAVY unit can deal 40–60 HP in a single push sequence.
3. **Result:** A single-turn sequence of HEAVY + push kills or cripples a unit without the Gravimancer needing to commit its heavier spells.

**Tactical note:** This combo works in reverse — Aeromancer can push a unit onto an elevated tile, and then the Gravimancer Crushes them off it in the same turn (if both Mancers activate on the same turn via Mancer initiative). Aeromancer provides height; Gravimancer exploits height.

---

### Combo 2 — The Gravity Cluster (Gravimancer + Sonimancer)

**Mancers involved:** Gravimancer + Sonimancer

**Step-by-step execution:**

1. **Gravimancer activates (Turn N):** Places Gravity Well (3 AP) at a chokepoint center. All enemy units within 3 tiles begin being pulled 1 tile toward center on each of their turns.
2. **Turns N+1 to N+2:** Enemy units are pulled inward — after 2 turns, units that were 3 tiles away are now adjacent to the center or on it.
3. **Turn N+2, Sonimancer activates:** Sonimancer fires Resonance Cone (or any cone spell) directly into the center of the Gravity Well. All units clustered there are hit by the full cone area. Without the Gravity Well, those same units would have been spread across a 5-tile area — only 1 would have been in cone range.

**Damage math:** A Sonimancer Resonance Burst at 30 damage hitting 4 clustered units = 120 total damage vs. the 30 damage it would have dealt to 1 unit in normal spread. The Gravity Well converts a single-target Sonimancer activation into an AoE sweep.

---

### Combo 3 — Gravitational Collapse into Sonimancer Cone [HIGH SIGNATURE]

**Mancers involved:** Gravimancer + Sonimancer (or any AoE Mancer)

**Step-by-step execution:**

1. **Verify ally positions:** Confirm no allied Mancers are within 5 tiles of the chosen collapse center.
2. **Gravimancer activates (6 AP):** GRAVITATIONAL COLLAPSE at the chosen center. All enemy units within 5 tiles are simultaneously pulled to the center, taking 8 HP × travel distance + 10 HP collision.
3. **Sonimancer activates (same turn — Mancer initiative allows both to act):** Sonimancer aims its cone at the center point. All units clustered there are caught in the cone. Full AoE damage applied to every enemy unit in the warband simultaneously.

**Risk note:** If any ally unit was within 5 tiles of the collapse center when it triggers, that ally is also pulled to the center and takes travel + collision damage, and is caught in the Sonimancer cone. This is the documented risk of GRAVITATIONAL COLLAPSE — the Gravimancer player must clear the radius before triggering.

---

### Combo 4 — Crush SHATTER (Gravimancer + Cryomancer)

**Mancers involved:** Gravimancer + Cryomancer

**Step-by-step execution:**

1. **Gravimancer activates (Turn N):** Graviton Bolt hits target (HEAVY applied).
2. **Cryomancer activates (Turn N or N+1):** Ice Lance freezes the HEAVY target (FROZEN applied on top of HEAVY — both active simultaneously).
3. **Gravimancer activates (Turn N+1 or N+2):** Crush (5 AP) at the FROZEN + HEAVY target. Crush damage: 60 HP (HEAVY bonus) × 2.5 SHATTER (FROZEN) = 150 HP. This eliminates every non-upgraded Mancer in the game outright from full HP.

**AP efficiency:** This combo requires 2 AP (Graviton Bolt) + 3 AP (Ice Lance) + 5 AP = 10 AP total across 2 activations, for a guaranteed kill on any standard Mancer. The 150 HP SHATTER damage is the highest confirmed single-hit kill value in the roster.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Gravimancer

| Mancer | Counter Mechanism |
|---|---|
| **Floramancer** | ROOTED status blocks all forced displacement — the Floramancer can root key allies near fall hazards (protecting them from Pull), root itself (making it immovable in Gravity Well radius), and convert ground terrain to OVERGROWTH so fast that the Gravimancer cannot plan stable well placement. ROOTED also means Gravimancer's Pull is wasted AP against Floramancer's rooted units. |
| **Aeromancer** | The UPDRAFT zone grants allies WEIGHTLESS — WEIGHTLESS is the hardest counter to HEAVY (they cancel each other). An Aeromancer pre-emptively applying UPDRAFT to allied units before the Gravimancer can apply HEAVY removes the fall damage amplification entirely. Aeromancer can also push the Gravimancer itself — and without the Anchor passive, the Gravimancer has no displacement resistance. |
| **Geomancer** | OBSIDIAN terrain cannot be affected by gravitational compression. More critically: Geomancer walls block Pull paths — if a wall is between the Gravimancer's pull target and the destination, the pull is interrupted. Geomancer's terrain architecture limits the Gravimancer's freedom to place wells and pull lines. |

---

*End of Gravimancer design document.*

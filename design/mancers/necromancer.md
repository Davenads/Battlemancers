# Necromancer — Full Design Document

---

## 1. Tactical Identity

The Necromancer is the game's primary attrition and resource-conversion specialist. Where every other Mancer watches units die and treats those deaths as losses, the Necromancer treats them as deposits into a spell economy. Every corpse on the battlefield — ally or enemy — is a potential asset: raw fuel for a Risen Shambler, a high-value Remnant token from a fallen Covenant Husk, or a detonation target via Death Mark. The Necromancer doesn't race to win; it wins by outlasting, by converting the opponent's violence into its own escalating advantage.

Playing the Necromancer well means understanding two distinct timelines simultaneously. In the short term, the Necromancer is a fragile, moderate-damage Mancer with strong single-target debuffs. In the medium and long term — once corpses accumulate and the corpse economy is stocked — the Necromancer becomes a force-multiplier that puts the opponent in an impossible position: stop killing units (and lose board control) or keep killing (and feed the Necromancer's engine). The skill floor is managing the spatial corpse economy: knowing which corpses to reanimate immediately, which to hold for bigger reanimates, and which to detonate via Death Mark for burst value instead.

**Primary win condition:** The Necromancer wins through attrition leverage. Its reanimated units don't cost warband points to activate — they are bonus pieces. In a long engagement, two or three active reanimated units alongside the base warband represent an activation-economy advantage the opponent cannot match without eliminating the Necromancer itself. Death Mark forces difficult defensive choices: protect heavily or hand the Necromancer a free AoE explosion on death. The Necromancer team wins the moment the corpse economy surpasses the opponent's ability to maintain unit count.

**Core weakness:** The Necromancer has no corpse economy until units die — it is weakest in the first two turns of any engagement, before any significant deaths. Against fast-aggression warbands that win before a body count accumulates, the Necromancer's full toolkit never comes online. Additionally, the Necromancer's reanimated units are weaker than the original — they are attrition tools, not power spikes. A warband built around three high-upgrade Mancers may simply eliminate Necromancer summons faster than they can be replaced. Pyromancer fire zones also threaten the corpse economy: burned corpses produce degraded reanimation fuel (see Corpse Quality table in Section 3).

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 90 | Below average; meant to stay behind its summons and infantry screen |
| **Move Range** | 3 tiles per activation | Slow; designed to anchor in position and let summons project forward |
| **Base Armor** | 1 | Minimal physical mitigation; relies on meat shields |
| **Spell Range** | 5 tiles (base) | Each spell lists its own range; this is the fallback reference |
| **AP per Activation** | 6 | Standard for all Mancers; movement costs 1 AP per tile |
| **Element** | Death/Necrotic | All base spells deal Necrotic damage or apply death-state terrain/status interactions |

**AP budget example:** With 6 AP, the Necromancer can move 2 tiles (2 AP), reanimate a corpse (3 AP), and apply Death Mark (1 AP), or spend the full 6 AP on a Necrotic Eruption without moving.

---

## 3. Base Spell Kit

The Necromancer's four base spells are designed to cover distinct combat functions:
- **Necrotic Bolt** — reliable single-target damage + corpse enhancement
- **Raise Shambler** — core reanimation spell; the engine of the corpse economy
- **Death Mark** — single-target debuff that converts an enemy's death into an AoE event
- **Necrotic Eruption** — AoE burst that consumes corpses in the area for damage scaling

---

### Corpse Economy — Core System

The Necromancer's entire mid-game strategy revolves around corpse quality. All deceased units on the battlefield leave a corpse object on their tile for 3 turns before decaying. Corpses vary in quality depending on their origin:

| Corpse Source | Corpse Type | Reanimate Into | Notes |
|---|---|---|---|
| T1 Chaff (any faction) | **Standard Corpse** | Risen Shambler (T1 equivalent) | 1 Standard Corpse consumed per Shambler |
| T1 Ranged (any faction) | **Standard Corpse** | Risen Shambler (T1 equivalent) | Same as Chaff corpse; ranged unit doesn't grant better fuel |
| T2 Veteran (any faction) | **Veteran Corpse** | Risen Veteran (T2 equivalent; faster, higher HP) | 1 Veteran Corpse consumed; noticeably stronger summon |
| Ashen Covenant Chaff dying adjacent to any friendly Mancer | **Remnant Token** | Abyssal Horror (T2+; see below) | Created by Deathless Ranks trait even without Necromancer present |
| Ashen Covenant Abyssal Revenant (T2 Chaff) dying anywhere | **Remnant Token** | Abyssal Horror (T2+; see below) | Abyssal Revenants generate Remnant tokens unconditionally |
| Burned corpse (unit died on or was standing in ON_FIRE terrain) | **Charred Remains** | Cinder Wraith (fragile but applies BURNING on attack) | Lower base HP than Risen Shambler; fire DoT attack is the trade-off |
| Mancer corpse | **Soul Vessel** | Cannot reanimate; consumed by Necrotic Eruption for +40 bonus damage | Mancers are too powerful to reanimate; their soul fuels the detonation instead |

**Remnant Tokens (Ashen Covenant Synergy — detailed):**
When any Ashen Covenant Chaff unit (Grave Husk T1 or Abyssal Revenant T2) dies adjacent to a friendly Mancer, the Deathless Ranks faction trait converts their death into a Remnant token rather than a standard corpse. Abyssal Revenants generate a Remnant token on death regardless of proximity to a Mancer.

A Remnant token is a supercharged corpse resource that the Necromancer can consume to reanimate an **Abyssal Horror** — a T2+ quality summon with significantly higher HP and a necrotic cleave attack that hits all adjacent units. The Abyssal Horror costs 4 AP to reanimate (compared to 3 AP for a Risen Shambler from a standard corpse) and consumes 1 Remnant token. It cannot be created from standard corpses.

**Why this combination is the game's primary attrition synergy:** In an Ashen Covenant warband, every Grave Husk or Abyssal Revenant that dies is not just a unit lost — it is a Remnant token deposited. A Necromancer supported by a screen of Covenant Chaff is continuously banking fuel. The opponent faces a compounding return on investment: killing Covenant Chaff is necessary to maintain board control, but each kill feeds the Necromancer's Abyssal Horror production. At capacity (3 active summons on field), the Necromancer is operating an entirely separate undead line atop the warband it arrived with.

**Corpse limits and decay:**
- Maximum 3 companion-type summons on field simultaneously (global summon cap per Mancer)
- Corpses decay after 3 turns if not consumed; Remnant tokens decay after 4 turns
- Corpses consumed by Necrotic Eruption are destroyed and cannot be raised
- Necromancer can inspect corpse quality by targeting the corpse tile (0 AP — passive read action)

---

### Spell 1: Necrotic Bolt

| Field | Value |
|---|---|
| **Name** | Necrotic Bolt |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (projectile) |
| **Range** | 6 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 16 |
| **Element** | Necrotic |
| **Effects Applied** | Applies `DEATH_MARK` to hit unit (see status-effects.md — on death: explodes for AoE damage scaling with max HP). If target is already `DEATH_MARK`ed, the bolt instead deals +8 bonus damage and refreshes the mark duration. Additionally: if target dies while `DEATH_MARK` is active, the corpse left behind is upgraded to a **Veteran Corpse** quality regardless of original unit tier. |
| **Terrain Interaction** | Hitting a tile with an existing corpse causes the corpse to pulse with necrotic energy: all units adjacent to the corpse tile take 4 Necrotic damage. The corpse quality is not degraded by this interaction. |
| **Special Interactions** | See terrain interaction table in Section 4. |

**Design note:** Necrotic Bolt is the Necromancer's workhorse and its primary DEATH_MARK applicator. At 2 AP with no cooldown, it can be used twice per activation, applying DEATH_MARK to two separate targets or refreshing an existing mark and dealing bonus damage on the second cast. The corpse upgrade on Death Mark kill is a meaningful mid-match economic incentive: the Necromancer wants to ensure that its Death Mark targets die while marked, so it will typically mark a high-threat unit (one the rest of the warband is targeting anyway), guaranteeing the Veteran Corpse bonus when that unit falls.

---

### Spell 2: Raise Shambler

| Field | Value |
|---|---|
| **Name** | Raise Shambler |
| **AP Cost** | 3 AP |
| **Cooldown** | 1 turn |
| **Targeting Type** | Ground Target — targets a tile containing a corpse within range |
| **Range** | 4 tiles |
| **AoE Radius** | N/A (single corpse consumed) |
| **Base Damage** | 0 (summon spell; no direct damage) |
| **Element** | Necrotic |
| **Effects Applied** | Consumes 1 corpse at target tile; summons a reanimated unit based on corpse quality (see Corpse Economy table). Risen Shambler (from Standard Corpse): 30 HP, 3 Move, melee attack 8 damage. Risen Veteran (from Veteran Corpse): 50 HP, 4 Move, melee attack 12 damage. Abyssal Horror (from Remnant Token, costs 4 AP instead): 65 HP, 4 Move, necrotic cleave 15 damage to all adjacent. Cinder Wraith (from Charred Remains): 20 HP, 5 Move, melee 6 damage + applies BURNING on hit. Summon activates on the Necromancer's next turn. |
| **Terrain Interaction** | Raising a summon on a TOXIC_TERRAIN tile grants the summon 1 POISONED stack immunity — it takes no poison tick damage on that tile for 2 turns (necrotic affinity resists toxins briefly). Raising on an ON_FIRE tile costs the Necromancer 5 HP (casting through fire) but does not degrade the corpse quality. |
| **Special Interactions** | See terrain interaction table in Section 4. |

**Design note:** Raise Shambler is the Necromancer's defining spell and the reason corpse management matters. The 1-turn cooldown means the Necromancer can reanimate at most every other activation, creating a deliberate pacing that rewards sequencing: turn 1, combat begins, Necrotic Bolts applied; turn 2-3, first deaths, first Raise; turn 4+, corpse pipeline sustaining 2-3 active summons. The Abyssal Horror variant at 4 AP is the Necromancer's most powerful summon but requires a Remnant Token — Ashen Covenant exclusive fuel that fundamentally changes the Necromancer's power ceiling.

---

### Spell 3: Death Mark

| Field | Value |
|---|---|
| **Name** | Death Mark |
| **AP Cost** | 1 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (targeted status; LOS required) |
| **Range** | 5 tiles |
| **AoE Radius** | N/A (mark is on unit; death explosion is AoE) |
| **Base Damage** | 0 (no direct damage on application) |
| **Element** | Necrotic |
| **Effects Applied** | Applies `DEATH_MARK` status to target. On target's death: explosion centered on death tile, AoE radius 2, deals damage = target's max HP × 0.4 (e.g., a 100-HP Mancer explodes for 40 damage in 2-tile radius). The explosion is Necrotic element. DEATH_MARK cannot be dispelled (per status-effects.md). |
| **Terrain Interaction** | When a DEATH_MARK explosion triggers on a TOXIC_TERRAIN tile: explosion damage increases by 25% (necrotic and poison energies combine). When triggered on an ON_FIRE tile: a 1-tile NECROTIC_ASH patch is created at the death location — units on NECROTIC_ASH take 3 Necrotic damage per turn and cannot regenerate HP for 2 turns. |
| **Special Interactions** | Death Mark stacks with Necrotic Bolt's mark — if the same unit has been hit by Necrotic Bolt (which also applies DEATH_MARK), the mark's explosion AoE radius increases from 2 to 3 tiles. The Necromancer should use Death Mark (1 AP) for the base application and Necrotic Bolt (2 AP) for the upgrade when a high-value kill is anticipated. |

**Design note:** Death Mark is the cheapest AP spend in the Necromancer's kit and the most psychologically impactful. A 1 AP investment on a 100-HP target creates a 40-damage threat radius on that unit's death. This forces the opponent to either protect the marked unit (burning AP on defense) or accept that killing it hands the Necromancer a free 40-damage AoE. At 1 AP, the Necromancer can apply Death Mark and still have 5 AP for movement and other spells — it is almost always worth casting at the start of an activation before spending heavier AP.

---

### Spell 4: Necrotic Eruption

| Field | Value |
|---|---|
| **Name** | Necrotic Eruption |
| **AP Cost** | 5 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Ground Target — AoE Radial; targets a point, affects all corpses in range |
| **Range** | 4 tiles (to center of AoE) |
| **AoE Radius** | 3 tiles |
| **Base Damage** | 20 (to all living units in AoE) + 15 per corpse consumed in the area |
| **Element** | Necrotic |
| **Effects Applied** | Deals 20 Necrotic base damage to all living units in the 3-tile radius. Each corpse within the radius is consumed (regardless of quality): +15 bonus damage distributed to all living units in range per corpse consumed. Soul Vessel corpses (Mancer corpses): +40 bonus damage each instead of +15. Surviving units in the blast zone receive `CHILLED` (necrotic cold; 2 turns) as cold death energy lingers. Tiles in the radius become `NECROTIC_ASH` terrain (3-turn duration; 3 Necrotic dmg/turn to living units standing on the tile; undead summons ignore this damage). |
| **Terrain Interaction** | Against WET tiles in the AoE: the necrotic energy corrupts the water — NECROTIC_ASH layered under the WET state; the WET state remains but units are exposed to both conductive AND necrotic terrain simultaneously. Against ON_FIRE tiles: fire extinguished, NECROTIC_ASH replaces ON_FIRE. Against TOXIC_TERRAIN: amplified — TOXIC_TERRAIN + Necrotic Eruption creates TOXIC_NECROTIC ground, dealing 3 Necrotic + 1 POISONED stack per turn to living units. |
| **Special Interactions** | See terrain interaction table in Section 4. If the Necromancer's own summons are in the AoE, they are not damaged (undead ignore Necrotic damage from this spell), but they are also not boosted by the corpse consumption — they simply survive the eruption. |

**Design note:** Necrotic Eruption is the Necromancer's "cash out" spell — it converts accumulated corpse wealth into immediate burst damage. On a clean board with no corpses, it deals 20 damage flat to a 3-tile radius: underwhelming for 5 AP. With 3 Standard Corpses in range, that becomes 20 + 45 = 65 damage to all units in range: one of the highest raw AoE damage outputs in the game. The 3-turn cooldown ensures this cannot be spammed, and the corpse consumption means using it aggressively early destroys the Necromancer's future Raise Shambler fuel. Timing the Eruption for maximum corpse density — typically after a melee clash — is the high-skill expression of the Necromancer.

---

## 4. Terrain Interaction Table

### Necrotic Spell Impact on Existing Terrain States

| Existing Terrain State | What Happens When Necrotic Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **Normal (GROUND)** | Necrotic energy seeps into the earth | `NECROTIC_ASH` (2 turns; 3 Necrotic dmg/turn to living units) | Takes spell damage | Undead summons ignore NECROTIC_ASH damage |
| **WET** | Necrotic energy corrupts water; WET state retained | `WET` (secondary `NECROTIC_ASH` beneath) | Takes spell damage + 1 Necrotic dmg/turn from tainted water | Electromancer chain still works on WET layer; necrotic damage is additive |
| **ON_FIRE** | Cold death energy extinguishes fire | `NECROTIC_ASH` (fire removed) | Takes spell damage; BURNING is NOT applied | Creates NECROTIC_ASH rather than STEAM_CLOUD — no blind effect, pure necrotic hazard |
| **FLOODED** | Massive water body resists necrotic corruption | `FLOODED` remains; `NECROTIC_ASH` underlay added | Takes spell damage; moves through both FLOODED movement penalty and NECROTIC_ASH DoT | The NECROTIC_ASH underlay persists even as FLOODED expands further |
| **TOXIC_TERRAIN** | Poison and death energy merge | `TOXIC_NECROTIC` hybrid (3 Necrotic dmg/turn + 1 POISONED stack/turn) | Takes spell damage + CHILLED + enters TOXIC_NECROTIC terrain | Most punishing sustained ground state in the Necromancer's kit; difficult to create without Toximancer assistance |
| **ICE_TILE** | Necrotic cold intensifies ice | `PERMAFROST` (ice hardened by death energy; permanent until Fire spell hits) | Takes spell damage + FROZEN immediately (no CHILLED progression needed) | PERMAFROST from necrotic interaction is ice laced with death energy — Fire spells still melt it but leave NECROTIC_ASH residue rather than FLOODED |
| **MUD** | Necrotic energy animates organic matter in mud | `NECROTIC_ASH` (MUD removed; replaced by ash) | Takes spell damage | MUD movement penalty removed; replaced by Necrotic DoT |
| **OBSIDIAN** | Obsidian is impervious | `OBSIDIAN` (unchanged) | Takes spell damage | No terrain change |
| **OVERGROWTH** | Organic matter withers and decays | `NECROTIC_ASH` (OVERGROWTH destroyed) | Takes spell damage + 1 POISONED stack (decomposition gases) | Floramancer structures on OVERGROWTH destroyed |
| **CHARGED** | Electrical and necrotic energy interfere destructively | Tile cleared of CHARGED; becomes `NECROTIC_ASH` | Takes spell damage + 10 Lightning damage (arc discharge) | The arc is not a full chain — it is single-tile only; CHARGED is consumed |
| **PERMAFROST** | Necrotic cold deepens permafrost | `PERMAFROST` (reinforced; 2 additional turns duration) | Takes spell damage + FROZEN (same as ICE_TILE interaction above) | FROZEN from permafrost interaction applies even if unit was already CHILLED |

### Terrain States Beneficial to the Necromancer

| State | Benefit |
|---|---|
| `NECROTIC_ASH` | Necromancer's own undead summons take no damage from this terrain; the Necromancer itself also ignores NECROTIC_ASH damage (necrotic immunity to own element) |
| `TOXIC_TERRAIN` | TOXIC_TERRAIN + Necrotic Eruption = TOXIC_NECROTIC; the Necromancer rewards Toximancer allies who pre-seed toxin ground |
| `BURNING` terrain (enemy units) | Burned corpses leave Charred Remains — different from standard corpses but still fuel for Cinder Wraith reanimation |
| Any tile with a corpse object | The Necromancer can read corpse quality for 0 AP; corpse tiles are visible tactical resources to the Necromancer player |

### Terrain States Hazardous to the Necromancer

| State | Hazard |
|---|---|
| `ON_FIRE` | Burning corpses become Charred Remains (degraded fuel). The Necromancer standing in fire risks HP loss on its already-low 90 HP pool |
| `FLOODED` / `WET` | Raise Shambler through water requires targeting through the terrain state — no obstacle, but enemy Hydromancer flooding a corpse tile before the Necromancer can raise it denies the reanimate by potentially displacing the corpse object |
| `STEAM_CLOUD` | BLINDED Necromancer cannot target distant corpses for Raise Shambler (range reduced to 1) |
| `CHARGED` | Same lethal risk as any other Mancer at low HP |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

#### Variant A: Raise Abomination (replaces Raise Shambler) — +25 pts

**Description:** Replaces Raise Shambler with a higher-tier reanimation spell. Raise Abomination consumes **2 Standard Corpses** (or 1 Veteran Corpse) to summon a Flesh Abomination: 80 HP, 2 Move, melee attack that deals 20 damage to ALL adjacent units simultaneously (full cleave). The Abomination is slow but tanky — designed to anchor a position rather than chase. AP cost: 4 AP. Cooldown: 2 turns. Cannot create Abominations from Charred Remains.

**Trade-off:** Consumes 2 corpses per use instead of 1, significantly raising the investment per summon. The Abomination's slow speed means it must be placed carefully — adjacent to enemies at raise time, or blocking a chokepoint the enemies must push through. Best for warbands with heavy Covenant Chaff and high corpse generation throughput.

**Synergy note:** Abomination + Death Mark is a deceptively strong combination: the Abomination is large (80 HP) so its Death Mark explosion (32 damage AoE on 80 HP) is meaningful. Opponents face a dilemma — the Abomination is blocking a lane, killing it is necessary but detonates for significant splash damage on adjacent units.

#### Variant B: Soul Drain (replaces Necrotic Bolt) — +20 pts

**Description:** Replaces Necrotic Bolt with a life-steal projectile. Soul Drain deals 14 damage (slightly less than Necrotic Bolt's 16) but heals the Necromancer for 8 HP on hit. Cooldown: 1 turn (cannot spam like Necrotic Bolt). Does NOT apply DEATH_MARK — the soul is drained rather than marked. Instead, if Soul Drain kills the target, the corpse left behind is automatically upgraded to Veteran Corpse quality regardless of original unit tier.

**Trade-off:** Trading DEATH_MARK application and no-cooldown spam for passive sustain. A Necromancer with Soul Drain can recover up to 8 HP per activation on average, meaningfully extending its survivability without spending Hydromancer heals. Best for solo-Mancer warbands where the Necromancer cannot rely on allied support.

---

### Passive Traits

#### Passive A: Grave Tide — +20 pts

**Description:** When any unit (ally or enemy) dies within 3 tiles of the Necromancer, it automatically generates a Necrotic Pulse: 3-HP necrotic damage to all living units adjacent to the death tile (not including the Necromancer's own summons). This is passive — no AP cost. At high unit-density engagements, Grave Tide generates constant chip damage whenever anything dies in the Necromancer's immediate zone, punishing tight formations that cluster around kills.

**Synergy note:** Grave Tide + Ashen Covenant Chaff spam is particularly potent: every Husk that dies generates a free 3-HP pulse to surrounding enemies. Opponents who kill through a Husk screen are also absorbing Grave Tide pulses on every kill, making the attritional cost of forcing through a Covenant line measurably higher.

#### Passive B: Necrotic Resilience — +20 pts

**Description:** The Necromancer gains immunity to DEATH_MARK (cannot have its own ability used against it by a reflected or enemy Necromancer) and reduces the duration of all incapacitation effects (STUNNED, FROZEN, SILENCED, CHARMED, CONFUSED) by 1 turn. The minimum duration after reduction is 0 — meaning 1-turn incapacitations (STUNNED) simply do not apply to the Necromancer.

**Design note:** This directly addresses the Necromancer's core vulnerability: losing a turn means losing a Raise Shambler timing window, a critical corpse before it decays, or a Death Mark application. Necrotic Resilience makes the Necromancer significantly harder to shut down with status effects.

#### Passive C: Corpse Preservation — +15 pts

**Description:** Corpses within 4 tiles of the Necromancer decay after 5 turns instead of 3. Remnant tokens within 4 tiles decay after 6 turns instead of 4. This passive has no AP cost and represents the Necromancer's death-energy preservation field.

**Trade-off:** Pure logistics upgrade — doesn't add power, adds flexibility. The extra decay time allows the Necromancer to bank corpses through a turn where it is Silenced, blocked from casting, or spending AP on movement instead of reanimation. Most valuable in longer engagements.

#### Passive D: Undying Command — +25 pts

**Description:** The Necromancer's active summons gain +5 HP and +2 Necrotic damage to their attacks. Additionally, when a summon is destroyed, the Necromancer immediately heals 10 HP (the death energy returns to its master). The summon cap remains at 3.

**Synergy note:** Undying Command makes Risen Shamblers (30 HP base) into 35-HP units and increases their combat output. More importantly, the on-destruction heal means a Necromancer that keeps its summon slots full and actively uses them as shields will recover HP consistently — mitigating its low 90 HP maximum through summon turnover.

---

### Stat Enhancements

#### Enhancement A: Iron Shroud (+20 HP) — +15 pts

**Description:** Max HP increases from 90 to 110. Brings the Necromancer out of glass-cannon territory and into survivable range for most single-target spells. Most valuable in warbands without dedicated frontline screening, where the Necromancer must occasionally absorb hits before reanimation resources protect it.

#### Enhancement B: Death March (+1 Move Range) — +10 pts

**Description:** Move Range increases from 3 to 4 tiles per activation. The Necromancer's slow mobility is often the constraint that prevents it from reaching a valuable corpse tile (a Veteran Corpse 5 tiles away costs 5 AP to reach, consuming the raise's AP budget). One extra tile of movement allows the Necromancer to manage a larger corpse collection radius without burning full AP on repositioning.

---

### Signature Ability

#### Signature: Army of the Dead — +40 pts

| Field | Value |
|---|---|
| **Name** | Army of the Dead |
| **AP Cost** | 6 AP (entire activation; Necromancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Self-centered AoE — no targeting cursor |
| **Range** | N/A (self-centered) |
| **AoE Radius** | 5 tiles |
| **Base Damage** | 0 (reanimation ability; no direct damage) |
| **Element** | Necrotic |
| **Effects Applied** | Every corpse within 5 tiles is simultaneously raised as the appropriate quality summon (Standard → Risen Shambler, Veteran → Risen Veteran, Remnant Token → Abyssal Horror, Charred Remains → Cinder Wraith, Soul Vessel → consumed for 40 HP heal to Necromancer instead of reanimate). All summons raised by Army of the Dead activate on the **current turn** rather than waiting until next turn. Summon cap is temporarily raised to 5 for 1 turn; at the end of the turn, summons beyond the normal cap of 3 are dismissed (they still acted this turn). |
| **Special Interactions** | Army of the Dead only raises what exists — if no corpses are present in 5 tiles, it has no effect but still consumes 6 AP and triggers the 5-turn cooldown (a skill trap identical to Pyromancer's World Conflagration on a clear board). Against a board with 4-6 corpses, Army of the Dead can deploy a full undead line in a single activation, completely shifting the board state. Remnant tokens within range generate Abyssal Horrors (4 AP cost waived — Army of the Dead raises all at once at flat cost). |

**Design note:** Army of the Dead is the Necromancer's "this is what we have been building toward" ability — a mirror of Pyromancer's World Conflagration in that it converts accumulated resource investment into an instant board transformation. Cast into a corpse-rich late-game board, it can simultaneously raise 4-5 undead units and immediately attack, effectively adding a full warband activation worth of units to the board in one turn. Its design as a 0-damage spell means it is useless against a board with no corpses — the opponent who aggressively cleanses or blocks corpse generation (Photomancer light cleanse, preventing Death Marks from executing) can deny this ability entirely. Costs 40 upgrade points by design — it is a warband build-around commitment.

---

## 6. Faction Synergy

### Best Faction: The Ashen Covenant

The Ashen Covenant is the Necromancer's natural and intended home. The synergy between Deathless Ranks and the Necromancer's corpse economy is the most deliberately designed faction-Mancer interaction in the game.

**The Remnant Token loop:** Every Grave Husk or Abyssal Revenant that dies adjacent to any friendly Mancer leaves a Remnant token. In a standard 700–1,000-pt warband with 20–40 Covenant Chaff units, deaths are inevitable and frequent. A Necromancer that spends several turns raiding the Remnant token supply of a battle-worn Covenant line can sustain 2-3 Abyssal Horrors in the field simultaneously — units that are strictly stronger than their T2 Chaff predecessors.

**Specific interactions that make Ashen Covenant the clear best pairing:**

| Mechanism | Effect |
|---|---|
| Grave Husk death adjacent to Necromancer | Remnant token generated; Necromancer's best fuel |
| Abyssal Revenant death (anywhere) | Remnant token generated without proximity requirement; more reliable fuel pipeline |
| Wailing Shades fire through NECROTIC_ASH | Wailing Shades ignore cover; they can fire through NECROTIC_ASH zones the Necromancer creates without LOS penalty |
| Deathless Ranks (no morale loss) | Covenant Chaff does not break under psychological pressure from Psychomancer disruption; the Necromancer's own allies are not susceptible to the morale-based disruption the Psychomancer inflicts on enemy non-Mancer units |
| Army of the Dead (Signature) + Covenant Husk screen | With 6+ Husks having died mid-fight, Army of the Dead raises all their Remnant tokens simultaneously into Abyssal Horrors — adding 5 fresh Horrors to the board in a single turn. Against a depleted opponent warband, this is often match-winning |

### Verdant Pact — Functional but Suboptimal

Verdant Pact Thornback Sentinels leave Thorn Patches on death — standard corpses, not Remnant tokens. Rootwardens leave Thorn Fields. Neither generates enhanced fuel. The Necromancer can still reanimate Pact chaff as standard Risen Shamblers, but the lack of Remnant tokens means the Abyssal Horror pipeline is completely unavailable. The Necromancer functions in a Verdant Pact warband but operates below its design ceiling.

**One marginal Pact synergy:** Wyrmwood Striders apply 2 POISONED stacks per hit. If those poisoned units die and the Necromancer raises them, the reanimated unit inherits 0 stacks (the status does not transfer to undead). However, raising a unit that died on TOXIC_TERRAIN or NECROTIC_TERRAIN creates a Risen unit with a minor elemental echo — this is a niche interaction not worth building around.

### The Gilded Throne — Adequate Infantry, No Remnant Economy

Gilded Throne provides Conscript Spearmen and Iron Vanguard — both generate standard or veteran corpses when killed. No Remnant tokens. The Necromancer's summoning pipeline is functional but capped at Risen Veteran quality. Iron Vanguard veteran corpses are useful (50-HP Risen Veterans are worth raising) but cannot produce the Abyssal Horrors that define the Necromancer's peak power.

**Marginal positive:** Throne's Iron Discipline protects Chaff from Psychomancer Charm and Panic. If the opponent is running a Psychomancer, Throne's discipline keeps infantry bodies intact longer, feeding more consistent corpses to the Necromancer.

---

## 7. Combo Chains

### Combo 1: Necromancer + Pyromancer — "Ashen Revival"

**Mancers involved:** Necromancer + Pyromancer

**Sequence:**
1. Pyromancer converts a zone to ON_FIRE terrain; enemy T1 Chaff advances through it and die to the 5 HP/turn DoT, leaving Charred Remains on the tiles.
2. Necromancer raises Cinder Wraiths from the Charred Remains (3 AP per raise; Cinder Wraith applies BURNING on attack).
3. Cinder Wraiths attack surviving enemies; each hit applies BURNING (refreshed 5 HP/turn).
4. Pyromancer's next activation converts all BURNING-tagged units via Ember Shot into full ON_FIRE tile saturation, and the Cinder Wraiths' BURNING application keeps the fire economy fed.

**Why this works:** Cinder Wraiths are a unique unit type the Necromancer can only produce when the Pyromancer has been active — a deliberate secondary corpse pipeline. The BURNING-on-attack creates a positive feedback loop where the Necromancer's fire-born summons continue enabling the Pyromancer's terrain engine.

**Risk note:** ON_FIRE terrain that the Necromancer needs to raise from damages it (5 HP/turn standing in the zone). Sequencing matters: raise first, then move, avoiding prolonged exposure.

---

### Combo 2: Necromancer + Toximancer — "The Poison Farm"

**Mancers involved:** Necromancer + Toximancer

**Sequence:**
1. Toximancer seeds TOXIC_TERRAIN across an approach path.
2. Enemy Chaff advances through TOXIC_TERRAIN and dies from accumulated POISONED stacks, leaving Standard Corpses on poisoned tiles.
3. Necromancer targets TOXIC_TERRAIN tiles with Necrotic Eruption: Fire + Necrotic = TOXIC_NECROTIC ground; corpses consumed for +15 bonus damage each.
4. Surviving units in the Eruption zone are now on TOXIC_NECROTIC terrain — taking 3 Necrotic + 1 POISONED stack per turn passively.
5. Necromancer raises remaining corpses as Risen Shamblers; Toximancer continues stacking POISONED on any unit the Shamblers attack.

**Why this works:** The Toximancer is essentially generating both corpse fuel (enemies dying to POISON) and terrain that amplifies the Necromancer's Eruption. The TOXIC_NECROTIC ground state is among the most punishing passive terrain in the game and is only accessible through this combination.

---

### Combo 3: Necromancer + Electromancer — "Corpse Lightning"

**Mancers involved:** Necromancer + Electromancer (Hydromancer optional third)

**Sequence:**
1. Necromancer applies DEATH_MARK to a clustered enemy group.
2. Electromancer kills the marked unit with a Lightning bolt.
3. DEATH_MARK triggers: explosion at death tile, 2-tile radius, ~40 damage AoE (on a 100-HP unit).
4. The Lightning bolt, if units were WET, chains to adjacent units — those adjacent units also take the Death Mark explosion splash.
5. Any corpses created in the explosion radius remain for Necromancer to raise on its next activation.

**Why this works:** The Electromancer's chain stun setup and the Necromancer's Death Mark explosion naturally overlap: a cluster of WET, STUNNED units adjacent to a Death Mark target is an unusually efficient combined kill zone, with the STUNNED units unable to scatter before the explosion resolves.

---

### Combo 4: Necromancer + Gravimancer — "The Pit Feed"

**Mancers involved:** Necromancer + Gravimancer

**Sequence:**
1. Gravimancer uses Gravity Well or push abilities to cluster enemies onto a group of tiles.
2. Necromancer casts Necrotic Eruption on the cluster; if prior deaths have seeded that area with corpses, the Eruption scales heavily.
3. Gravimancer applies DEATH_MARK to high-HP survivors (via Necromancer's prior application or its own displacement threats forcing retreats through marked units).
4. Necromancer raises corpses generated by the Eruption and Gravimancer damage.

**Why this works:** Gravimancer's clustering abilities set up Necrotic Eruption's area-based scaling in the same way Hydromancer's Flood Zone sets up Electromancer chains. The Necromancer's "damage scales with corpse count" mechanic rewards being adjacent to a kill zone — Gravimancer drags enemies into that zone.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Necromancer

| Mancer | Counter Mechanism |
|---|---|
| **Pyromancer** | Burned corpses become Charred Remains — degraded fuel. A Pyromancer that aggressively fires on the Necromancer's corpse tiles can deny standard reanimate fuel. Worse: ON_FIRE terrain on a corpse tile means the Necromancer takes HP damage (from fire) to raise there. Pyromancer + heavy fire coverage of the corpse field is the most direct counter to the Necromancer's economy. |
| **Photomancer** | Photomancer's Sunburst (area cleanse ability) removes NECROTIC_ASH terrain and destroys weaker undead (see Photomancer doc). Light purify effects counter the necrotic terrain states and disrupt undead summon HP thresholds. |
| **Hydromancer** | A Hydromancer flooding a tile with a key corpse (e.g., a REMNANT_TOKEN tile) via Tidal Surge pushes the corpse object off the tile, potentially out of the Necromancer's raise range before it decays. The Necromancer cannot raise a corpse that has been displaced to a tile outside its 4-tile range. |
| **Chronomancer** | TIME_SLOW applied to the Necromancer pauses its cooldown recovery — effectively delaying Raise Shambler availability. A REWIND on an enemy unit the Necromancer killed restores that unit to life, removing the corpse it left behind. |

### Terrain Compositions That Shut Necromancer Down

| Terrain Setup | Why It's Punishing |
|---|---|
| **ON_FIRE coverage of engagement zone** | Every death produces Charred Remains (degraded fuel) instead of Standard/Veteran Corpses. The Necromancer's Risen Shamblers are unavailable; it can only produce Cinder Wraiths, which are weaker and harder to use strategically. |
| **FLOODED tiles over corpse locations** | Corpse objects on FLOODED tiles are technically accessible for Raise Shambler (the spell can target through terrain states) but the Necromancer must enter or target into water zones — Hydromancer opponents may position to displace those corpses further. |
| **OBSIDIAN walls blocking access to corpse tiles** | If an enemy Geomancer creates OBSIDIAN barriers between the Necromancer and its target corpses, the Necromancer cannot walk through those tiles and must go around, often burning the 3-turn decay timer on corpses beyond its range. |

### Warband Compositions That Prey on Necromancer

| Warband Type | Exploitation |
|---|---|
| **Fast-aggression triple Mancer (Aeromancer + Electromancer + Pyromancer)** | Fast movement + mass damage ends the fight before sufficient corpses accumulate. Necromancer's economy needs 3-4 turns minimum to become self-sustaining. Aggression that ends engagements in 2-3 turns denies the ramp-up entirely. |
| **Photomancer + Pyromancer** | Photomancer light effects purify necrotic terrain; Pyromancer burns corpse fields. Between them, the two Mancers actively dismantle the Necromancer's economy from both ends — no usable corpses and no usable NECROTIC terrain. |
| **Gilded Throne dense ranged screen** | High-damage Crossbow Corps or Siege Arbalests can pick off Risen Shamblers before they reach melee engagement — the Necromancer's summons have low HP and are easy to clear at range. If the Necromancer cannot protect its summons long enough for them to reach contact, the entire reanimate AP investment is wasted. |

---

*End of Necromancer design document.*

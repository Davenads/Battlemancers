# Spell System

## Core Design Goals

1. **Every spell does something to the board** — minimum viable: apply a terrain state, move a unit, or create a persistent effect. Spells that only deal direct damage are weak design.
2. **Resource scarcity forces prioritization** — limited AP per turn means players must choose. Big combo spells cost more. Positioning setup spells are cheap. Detonation spells are expensive.
3. **Readability** — any observer should be able to understand what a spell does from watching its visual execution. The VFX communicates the mechanic.

---

## Action Points (AP)

Each Mancer has an AP pool per turn. AP resets fully at the start of their turn.

**Base AP per turn:** 6

**AP costs by spell tier:**
| Tier | AP Cost | Description |
|---|---|---|
| Quick | 1–2 AP | Minor applications: single target, small area, low damage |
| Standard | 3 AP | Core spells: reliable damage + state application |
| Heavy | 4–5 AP | Major spells: large AoE, significant terrain changes, powerful states |
| Ultimate | 6 AP | Entire turn spent; transformative effect; cooldown applies |

**Movement cost:** Moving 1 tile costs 1 AP. A Mancer with 6 AP can move 6 tiles and cast nothing, or move 3 tiles and cast a Standard spell.

**Bonus AP:** Chronomancer's HASTE grants +6 AP for that turn (double action). Some map objectives grant +1 AP on capture.

---

## Spell Categories

### 1. Projectile
- Travels from caster to target in a straight line
- Can be blocked by walls, terrain features
- Some projectiles bounce off CRYSTAL terrain
- Target is determined at cast time; projectile path matters (can hit intervening units if in line)
- **Example:** Ember Shot, Arc Bolt, Frost Bolt

### 2. Area of Effect (AoE)
- Targets a point; effect radiates outward N tiles
- Always affects terrain, not just units
- Sub-types: **Radial** (circle from point), **Cone** (directional spread), **Line** (row/column)
- **Example:** Fireball (radial), Shout (cone), Shockwave (line)

### 3. Terrain Placement
- Targets an empty or occupied tile; places a terrain feature or state
- No travel time — instantaneous placement
- Cannot place on occupied tile unless specifically a "ground effect" spell
- **Example:** Stone Wall, Flood Zone, Seed, Place Crystal

### 4. Targeted Status
- Applies a status effect to a specific unit (ally or enemy)
- Some have range restrictions (melee only, e.g. Bone Shield)
- **Example:** Charm, Freeze, Haste, Illuminate

### 5. Summon
- Spawns a companion or construct on a target tile
- Companion acts on Mancer's initiative in subsequent turns
- Summons count toward unit cap (max 3 companion units per Mancer on field)
- **Example:** Raise Dead, Summon Wolf, Bone Golem

### 6. Self / Reactive
- Affects caster only, or triggers automatically on a condition
- Some are "stances" (persist for multiple turns)
- **Example:** Afterimage, Bone Armor (self-cast), Updraft (creates zone around self)

---

## Targeting System

### Range
- All spells have a **range value** in tiles (e.g., Range 4 = can target any tile within 4 tiles)
- Range is measured as Manhattan distance on the grid for simplicity
- Mancers standing on ELEVATED tiles add +1 to range for all spells
- BLINDED status reduces effective range to 1 for all targeting

### Targeting Rules
- **Requires LOS:** Most spells need unobstructed line of sight to target tile
- **No LOS required:** Sonic spells travel through walls; some Psychomancer spells are LOS-independent
- **Self-target only:** Some spells (Bone Armor) can only be cast on the caster
- **Ground target:** Some spells target a tile, not a unit — valid even if tile is empty

### Area Resolution
- AoE effects apply to all units and terrain within their area simultaneously
- If an AoE would destroy terrain (e.g., stone wall) and a unit is behind that wall, the unit is NOT hit — terrain absorbs the effect and is destroyed
- Secondary effects (e.g., fire spreading from an explosion) resolve after the primary effect completes

---

## Cooldowns and Resource Management

### Per-Spell Cooldown
- Most spells have an individual cooldown measured in turns after use
- Cooldown starts after the spell resolves
- Quick spells: 0–1 turn cooldown (spammable)
- Standard spells: 1–2 turn cooldown
- Heavy spells: 2–3 turn cooldown
- Ultimate spells: 4–5 turn cooldown (once per long fight)

### Mancer-Level Resource (optional mechanic — for design iteration)
- Some Mancer types may have a secondary resource (e.g., Necromancer's SOUL_ENERGY from kills)
- This adds a secondary optimization layer for experienced players
- Designed as an opt-in layer, not required to play the base game

### Cooldown Reset Rules
- Cooldowns decrement by 1 at the **start of the owning Mancer's turn** (not at end of turn)
- A spell becomes castable again when its cooldown counter reaches 0
- **Kill bonus:** Landing the killing blow on an enemy unit reduces all of the caster's current cooldowns by 1 (minimum 0)
- **Terrain capture:** Capturing a marked objective tile reduces the capturing Mancer's longest active cooldown by 1
- **Silenced status:** A SILENCED Mancer's cooldown timers do NOT decrement — the Mancer is frozen in time magically but time still passes for their body
- Death resets all cooldowns (unit removed from play; cooldown state is discarded)

### Chronomancer Interaction
- TIME_SLOW on an enemy pauses their cooldown timers
- REWIND on a caster does NOT restore cooldowns (temporal body rewind, not magical energy rewind)
- HASTE does not reduce cooldowns; it just grants extra AP this turn

---

## Cross-Element Combo System

Combos occur when one spell interacts with a terrain state or unit status created by another spell. Combos are the highest-value plays in the game.

### Combo Tiers

**Tier 1 — Basic (one state, immediate)**
- Setup: apply a state (WET, BURNING, FROZEN)
- Detonate: use compatible spell next turn or same turn with second Mancer
- Bonus: 25-50% amplified damage or secondary effect
- Example: Hydromancer → WET → Electromancer Arc Bolt = chain stun

**Tier 2 — Chain (two states, two Mancers)**
- Setup: two terrain states co-exist
- Detonate: third spell triggers both simultaneously
- Bonus: Primary + secondary effect combined
- Example: Flooded + Toxic_Terrain → Electromancer arc = chain stun to all WET units, each also receives POISONED from toxic water contact

**Tier 3 — Full Combo (three Mancers, pre-planned board state)**
- Example: Hydromancer FLOOD → Cryomancer mass FREEZE → Sonimancer SHATTER = entire flooded zone units shattered for massive burst
- Requires turn sequencing and positioning investment
- Highest damage possible; most telegraphed and interruptible

### Combo Trigger Specification

A combo triggers when **all** of the following conditions are true at the moment of spell resolution:

1. **Target tile or target unit carries an active elemental state** (e.g., WET, BURNING, FROZEN, POISONED, CHARGED)
2. **The incoming spell's element is listed in the interaction table** as a trigger for that state (see `design/combat/status-effects.md` interaction matrix)
3. **The interaction table entry has a non-null combo effect** for that trigger+state pair

Combo resolution order within a single spell cast:
1. Primary spell damage/effect resolves first
2. Combo effect (from interaction table) resolves second
3. New terrain/unit states resulting from the combo are applied
4. Secondary spread (e.g., fire spreading to adjacent tiles) resolves last

Multiple combos from a single cast (e.g., hitting a tile that is both WET and POISONED with a Lightning spell) resolve sequentially in the order: WET → BURNING → FROZEN → POISONED → CHARGED (alphabetical fallback).

A unit or tile can only trigger **one** combo per incoming spell (the highest-tier matching interaction wins). Multiple simultaneous combos on different targets in an AoE resolve independently.

### Combo Communication to Player
- Terrain states are visually distinct (color coding, particle overlay)
- When a spell that would trigger a combo is aimed at a state tile, a **combo indicator** (glowing UI hint) shows before confirmation
- Post-match: Combo counter shows how many combos triggered. High combos = skilled play metric.

---

## Spell Design Rules (for future Mancer spell authoring)

Every spell should answer YES to at least 3 of these:

1. Does it apply or exploit a terrain state?
2. Does it move a unit (self, ally, or enemy)?
3. Does it create or destroy a terrain feature?
4. Does it apply a status to a unit?
5. Does it synergize with at least 2 other Mancer types?
6. Does it have a skill-expression component (timing, aim, positioning)?

A spell that only deals flat damage with no state interaction should be revised until it answers YES to at least one of the above.

---

## Spell Loadout System

**At game start:** Each Mancer has a fixed spell set of 5 spells (1 per tier: 2 Quick, 2 Standard, 1 Heavy or Ultimate). Balance tested as default kit.

**Customization (late design / post-launch):** Potential for spell slot customization — swap one spell per tier from a small unlockable pool per Mancer. Keeps a core identity intact while allowing tailoring to team comp.

**Spell naming convention:** `[MancerType]_[Effect]_[Tier]` internally for code; display names are evocative (e.g., Pyromancer_AoE_Standard = "Fireball")

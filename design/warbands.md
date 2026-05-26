# Warbands — List Building & Faction System

## Overview

A **Warband** is a player's assembled fighting force for a match of Battlemancers. Before every game, each player builds a warband from their chosen faction's available units and the shared pool of 19 Mancer archetypes, subject to a hard **1,000-point cap**.

Warband building is the primary pre-game expression of player identity and strategy. Like a Warhammer list, every point spent is a deliberate choice — more Mancers means less fodder, a deep chaff screen means fewer spell casters, heavy ranged investment trades melee board control for sustained fire.

---

## Unit Types & Point Costs

### Base Costs (Tier 1)

| Unit Type | Cost | Activation cost | Max per Warband | Notes |
|---|---|---|---|---|
| Mancer | 100 pts | 100 pts | 3 | Chosen from the shared 19-Mancer pool |
| Chaff (T1) | 10 pts | 10 pts | unlimited | Frontline melee; faction-specific unit |
| Ranged (T1) | 25 pts | 25 pts | unlimited | Archers/crossbows; faction-specific unit |

### Veteran Costs (Tier 2)

| Unit Type | Cost | Activation cost | Per full activation | Notes |
|---|---|---|---|---|
| Chaff (T2) | 20 pts | 20 pts | 5 exactly | Enhanced T1 Chaff; faction-specific veteran variant |
| Ranged (T2) | 50 pts | 50 pts | 2 exactly | Enhanced T1 Ranged; faction-specific veteran variant |

T2 units are purchased individually during warband construction alongside T1 units. A warband may mix T1 and T2 units freely within budget. T2 variants for each faction are defined in the faction entries below.

**Activation cost rule:** a unit's activation cost always equals its purchase cost. A T2 Chaff unit that cost 20 pts to buy activates for 20 pts. **The exception is Mancers** — see Mancer Upgrades below.

### Warband Construction Rules
- Total points must not exceed **1,000**
- At least **1 Mancer** must be included (no Mancer-less warbands)
- No Mancer may be duplicated within a single warband
- Faction determines which Chaff and Ranged unit types (T1 and T2) are available
- All three factions are available in all modes from the start — no unlock gating
- There is no minimum Chaff or Ranged requirement; a pure Mancer warband is legal (the math heavily disincentivizes it)

### Example Budget Distributions

All examples use T1 units; T2 veterans can be substituted anywhere at 2× cost for the unit type.

| Loadout Style | Mancers | Chaff | Ranged | Total |
|---|---|---|---|---|
| Triple Mancer Lite | 3 × 100 = 300 | 20 × 10 = 200 | 8 × 25 = 200 | 700 pts |
| Full Triple Mancer | 3 × 100 = 300 | 28 × 10 = 280 | 8 × 25 = 200 | 780 pts |
| Solo Mancer Horde | 1 × 100 = 100 | 54 × 10 = 540 | 14 × 25 = 350 | 990 pts |
| Dual Mancer Balanced | 2 × 100 = 200 | 30 × 10 = 300 | 18 × 25 = 450 | 950 pts |
| Triple Mancer Max | 3 × 100 = 300 | 40 × 10 = 400 | 12 × 25 = 300 | 1,000 pts |
| Upgraded Mancer + Veterans | 1 × 150 = 150 | 10 T2 × 20 = 200 | 6 T2 × 50 = 300 + 15 T1 × 10 = 150 | ~800 pts |

---

## Factions

Players choose one of three factions when building their warband. The faction determines:
- The identity and stats of your Chaff and Ranged units
- Passive faction-wide traits that apply to all non-Mancer units
- Thematic and aesthetic identity (sprites, banners, UI color scheme)

Mancers are **faction-agnostic** — any Mancer can serve any faction. The Pyromancer leads a regiment of Grave Husks just as readily as a line of Imperial Spearmen.

---

### Faction 1: The Gilded Throne

> *"Order. Discipline. The line holds."*

A human empire built on martial tradition and strict hierarchy. The Gilded Throne fields professional soldiers — drilled, equipped, and expendable. Their strength is reliability: predictable stats, no exotic weaknesses, and the best armor on non-Mancer units in the game. They have no elemental affinity, making them the most flexible faction for any Mancer composition.

**Faction Trait — Iron Discipline:** Chaff and Ranged units cannot be Panicked or Charmed. Morale-based debuffs (Psychomancer effects) have reduced duration on Throne units.

**Chaff Unit — Conscript Spearmen**
- Cost: 10 pts
- Frontline infantry with spear and buckler
- Solid melee damage, medium HP, no special ability
- Spear reach: can attack enemies in the tile directly ahead without moving into it (1-tile melee range extension)

**Ranged Unit — Crossbow Corps**
- Cost: 25 pts
- Armored crossbowmen with high single-shot damage
- Slow reload: fires every other turn (alternates attack/reload)
- Armor piercing: damage ignores a portion of physical defense

**Veteran Chaff — Iron Vanguard** *(T2 — 20 pts)*
- All Conscript Spearmen stats plus: significantly increased HP and armor
- Shield Wall: when two or more Iron Vanguard are adjacent, both gain a damage reduction aura
- Spear reach retained from T1

**Veteran Ranged — Siege Arbalest** *(T2 — 50 pts)*
- Fires every turn (no reload mechanic); loses the alternating restriction of the Crossbow Corps
- Higher armor-piercing value; can target units behind full cover at a 50% damage penalty
- Can brace (skip movement) to grant +1 tile of range that turn

**Aesthetic:** Gold, iron, deep crimson. Polished plate. Regimental banners. Marching formations.

---

### Faction 2: The Verdant Pact

> *"The forest remembers every scar."*

An ancient covenant between mortals and the living wilderness. The Verdant Pact fields warriors who fight as extensions of the terrain — rooting, entangling, and dissolving into undergrowth. Their non-Mancer units are weaker in open-field slugfests but thrive in complex terrain and interact directly with elemental ground states in ways other faction units do not.

**Faction Trait — Terrain Bond:** Verdant Pact units gain +1 movement when passing through or adjacent to natural terrain (forest tiles, earth/mud, vine-covered tiles, frozen water). Chaff units standing on natural terrain gain a passive regeneration tick at the start of each turn.

**Chaff Unit — Thornback Sentinels**
- Cost: 10 pts
- Bark-armored forest warriors with thorn-edged blades
- Lower HP than Conscript Spearmen but higher evasion in natural terrain
- On death: leaves a Thorn Patch on their tile (enemies moving through take minor damage)

**Ranged Unit — Glade Archers**
- Cost: 25 pts
- Elven-lineage archers with enchanted shortbows; can fire from dense cover without accuracy penalty
- Apply Poison on hit (1 stack)
- Lower raw damage than Crossbow Corps; compensate with consistent utility output

**Veteran Chaff — Rootwarden** *(T2 — 20 pts)*
- All Thornback Sentinel stats plus: notably higher HP; Thorn Patch on death upgraded to Thorn Field (2-tile radius)
- Can spend action to entrench into terrain: becomes immovable but gains a full evasion bonus and generates a natural tile beneath itself (activates Terrain Bond regen immediately)
- Entrenched Rootwardens count as natural terrain for adjacent units' Terrain Bond triggers

**Veteran Ranged — Wyrmwood Strider** *(T2 — 50 pts)*
- All Glade Archer traits plus: Poison stacks increased to 2 on hit
- Can move after firing (unlike most Ranged who fire from position)
- Leaves a brief Spore Trail on tiles moved through: enemies stepping on trail tiles receive 1 Poison stack

**Aesthetic:** Deep greens, amber, bark-brown. Living armor. Glowing sigils. Roots and vines trailing from gear.

---

### Faction 3: The Ashen Covenant

> *"Death is not the end. It is the resource."*

A death cult built on the principle that a fallen soldier is still useful. The Ashen Covenant fields undead legions — tireless, fearless, and replaceable. Their units do not rout, cannot be Panicked, and interact uniquely with the Necromancer's corpse economy. A Necromancer in an Ashen Covenant warband is particularly powerful: Covenant corpses count as enhanced fuel for reanimation.

**Faction Trait — Deathless Ranks:** Covenant Chaff units have no morale — they cannot flee, cannot be Charmed, and do not suffer Panic. When a Chaff unit dies adjacent to a friendly Mancer, it leaves a Remnant token (functions as a 0-HP corpse for Necromancer abilities).

**Chaff Unit — Grave Husks**
- Cost: 10 pts
- Shambling undead infantry; slow movement speed but high HP for their cost
- Regenerate 1 HP per turn while standing in Poisoned, Corrupted, or Burning terrain (necrotic absorption)
- On death: applies Cursed to adjacent enemies (reduces their healing received for 2 turns)

**Ranged Unit — Wailing Shades**
- Cost: 25 pts
- Spectral archers that phase partially out of the physical plane
- Their projectiles ignore physical cover (low walls, barricades), but deal reduced damage vs. full armor
- Emit a Silence aura on the tile they occupy: enemy units within 1 tile cannot trigger on-death effects

**Veteran Chaff — Abyssal Revenant** *(T2 — 20 pts)*
- All Grave Husk stats plus: movement speed penalty removed; Revenants move at normal speed
- On death: Cursed effect upgraded — enemies also take a burst of necrotic damage in addition to healing reduction
- Deathless Ranks trait retained; generates a Remnant token on death regardless of proximity to a Mancer

**Veteran Ranged — Void Wraith** *(T2 — 50 pts)*
- All Wailing Shade traits plus: projectiles now also ignore magical barriers (not just physical cover)
- On hit: target unit cannot trigger on-death effects for 1 turn (Silence aura becomes a targeted debuff on the hit unit, not just a tile aura)
- Can phase through one solid tile per turn as part of movement

**Aesthetic:** Ash grey, bone white, deep violet. Tattered wrappings. Spectral glow. Cracked skull motifs.

---

## Turn Structure — Simultaneous Blind Activation

Battlemancers uses a **simultaneous blind turn system** rather than alternating turns. Both players plan and commit their activations at the same time; plans are then revealed and resolved together.

### The Activation Budget

Each turn, a player may activate units totaling up to **100 points** of their warband. A unit's activation cost equals its purchase cost — with one exception: **Mancers always activate for 100 pts regardless of upgrades** (see Mancer Upgrades).

| Activation choice | pts used |
|---|---|
| 1 Mancer (any upgrade level) | 100 pts |
| 10 T1 Chaff | 100 pts (10 × 10) |
| 5 T2 Chaff | 100 pts (5 × 20) |
| 4 T1 Ranged | 100 pts (4 × 25) |
| 2 T2 Ranged | 100 pts (2 × 50) |
| 1 T2 Ranged + 2 T2 Chaff + 1 T1 Chaff | 100 pts (50 + 40 + 10) |
| 1 T1 Ranged + 3 T1 Chaff | 55 pts (partial budget — valid) |

- A player is **not required** to spend the full 100 pts — partial activation is allowed
- Unactivated units hold position and remain eligible to activate in a future turn
- A unit that activates can **move and act** (attack, cast, or use ability) in any order
- Activation is binary — a unit either activates fully or not at all; no partial-cost partial-actions
- A unit that is not activated is not defenseless — it holds its position and any passive abilities remain active

### The Blind Phase

1. **Planning:** Both players simultaneously select which units to activate and assign them actions (move targets, attack targets, spell targets). This is hidden from the opponent.
2. **Lock-in:** Both players confirm their activation plan. Neither player can change selections after lock-in.
3. **Reveal:** Both plans are revealed simultaneously.
4. **Resolution:** Activations are resolved using an **initiative order** among the activated units (Mancers resolve before Ranged, Ranged before Chaff; ties broken by unit position on board from top-left).
5. **End of turn:** Terrain effects tick, status effects decrement, and the next turn's Planning phase begins.

### Strategic Implications

- You cannot react to your opponent's activation plan — you must predict it
- Activating fewer units means tighter action economy but more information advantage (you plan less; opponent plans more)
- Mancers always resolve first among same-type units — prioritizing a Mancer activation guarantees their action lands before most enemy units move
- A player with many cheap Chaff units can "flood" multiple activations across many turns while an opponent burns Mancer budget on single powerful actions

---

## Mancer Upgrades

Mancers can be upgraded at warband construction, increasing their point cost beyond the base 100 pts. Upgrades represent a **warband budget trade-off only** — the activation cost of any Mancer remains fixed at 100 pts regardless of upgrades purchased.

**Example:** A Pyromancer upgraded to 150 pts costs 150 pts from the 1,000-pt warband budget, but still activates for 100 pts per turn. The 50-pt premium buys a better Mancer, not more activations.

### Upgrade Categories (to be specified per Mancer)

The following upgrade *types* are established. Specific options per Mancer archetype will be designed in a dedicated Mancer design pass:

| Upgrade Type | Description | Rough Cost |
|---|---|---|
| **Spell Variant** | Replaces a standard spell with a more powerful or situationally specialized version | +15–25 pts |
| **Passive Trait** | Adds a new passive ability (e.g., resistance to a status, aura effect, terrain interaction) | +20–30 pts |
| **Stat Enhancement** | Improves HP, move range, or a core stat meaningfully | +10–20 pts |
| **Signature Ability** | Unlocks a powerful unique ability not available at base; typically the Mancer's "ult" | +25–50 pts |

A single Mancer may take multiple upgrades; total cost is the sum of all upgrades added to the 100-pt base. No hard cap on upgrade spend per Mancer, but the 1,000-pt warband ceiling is the natural constraint.

> Specific upgrade options for all 19 Mancers are TBD — requires a dedicated design pass per archetype.

---

## Warband Building — Design Considerations

### Mancer Count vs. Fodder Depth
- **3 Mancers** maximizes raw spell power and combo potential but leaves only 700 pts for supporting units
- **1–2 Mancers** funds deep unit screens that protect them and contest objectives without relying on magic
- Mancers are worth protecting: at 100 pts, losing one is equivalent to losing 10 Chaff

### Faction + Mancer Synergy
Some Mancer archetypes synergize strongly with specific factions:

| Mancer | Best Faction Synergy | Reason |
|---|---|---|
| Necromancer | The Ashen Covenant | Remnant tokens from Covenant Chaff enhance reanimation |
| Floramancer / Geomancer | The Verdant Pact | Terrain Bond maximizes their terrain-creation value |
| Psychomancer | The Gilded Throne or Verdant Pact | Iron Discipline is the only counter to Psychomancer debuffs; Throne players are immune |
| Electromancer | Any | High universal value; Wailing Shades' silence aura prevents enemy chain-reaction on death |
| Pyromancer | The Ashen Covenant | Grave Husks absorb Burning terrain rather than suffering from it |
| Toximancer | The Verdant Pact | Glade Archers stack Poison on hit, multiplying Toximancer's synergy surfaces |

### Activation Budget Reads
A player who spent 700 pts on units has 7× more "activation turns" of Chaff available than a triple-Mancer player has Mancer turns. Reading how many points your opponent has available across different unit types — and predicting what they'll activate — is a core skill expression of the blind-turn system.


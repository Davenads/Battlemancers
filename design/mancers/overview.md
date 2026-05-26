# Mancer Roster — Design Overview

## Design Philosophy

Each Mancer should have a clear **tactical identity** that answers: what does this unit do to the board state? Not just damage/heal — but how does it reshape the fight. A Geomancer changes the geometry. A Chronomancer changes the sequence. A Necromancer changes unit economy.

**Every Mancer should:**
- Create or exploit at least one terrain state
- Have at least one cross-element synergy with 2+ other Mancers
- Have one high-skill-ceiling ability that rewards board reading
- Be weak to at least one identifiable counter strategy

---

## Full Roster

### Pyromancer
**Domain:** Fire, combustion, heat
**Tactical Identity:** Area denial and attrition. Pyromancer doesn't end fights — it makes the board hostile and forces enemy movement.

**Core mechanics:**
- Spells apply `BURNING` status to units and `ON_FIRE` to terrain tiles
- `ON_FIRE` terrain spreads to adjacent flammable tiles each turn (spreads like Worms' fire)
- Fire on `WET` tiles creates a `STEAM` cloud: blocks vision, deals light heat damage
- Fire on `POISONED` terrain creates `TOXIC_FUMES`: heavy AoE DoT, lingers 2 turns
- Fire on `FROZEN` tiles: instant melt to `WET`, releases burst steam
- High AP-cost spells create persistent `INFERNO` zones — impassable terrain for 3 turns

**Key spells (conceptual):**
- `Ember Shot` — single target, applies BURNING, low cost
- `Fireball` — AoE projectile, ignites terrain on impact
- `Flame Wall` — creates a line of ON_FIRE tiles, acts as barrier
- `Conflagration` — massive AoE, spreads fire 2 tiles from point of impact, high cost

**Synergies:** Hydromancer (steam combos), Toximancer (toxic fumes), Aeromancer (fan flames), Electromancer (fire burst when charged terrain ignites)
**Counters:** Hydromancer (extinguish), Cryomancer (freeze everything), Geomancer (obsidian walls block spread)

---

### Hydromancer
**Domain:** Water, tides, fluid dynamics
**Tactical Identity:** Setup and disruption. Hydromancer makes the board wet — priming it for Electromancer chains, Cryomancer freezes, or direct knockbacks.

**Core mechanics:**
- Spells apply `WET` to units (amplifies lightning damage, slippery movement)
- Terrain tiles become `FLOODED` — slow movement, conduct electricity
- Push/pull spells displace units across the grid (fall damage if pushed off ledges)
- One healing ability — unique among aggressive Mancers
- Can create `WATER_WALL` temporary terrain feature

**Key spells (conceptual):**
- `Torrent` — line AoE, pushes all units in path 2 tiles, applies WET
- `Flood Zone` — creates 3x3 FLOODED terrain area
- `Tidal Pull` — pulls target toward caster 3 tiles
- `Cleanse` — removes BURNING and POISONED from adjacent ally, small heal

**Synergies:** Electromancer (WET = lightning chain), Cryomancer (FLOODED = mass freeze), Gravimancer (pull + push combos), Aeromancer (mist dispersal)
**Counters:** Thermomancer (evaporates water), Geomancer (absorbs into mud, blocking flood spread)

---

### Cryomancer
**Domain:** Ice, frost, absolute cold
**Tactical Identity:** Crowd control specialist. Freezes enemies in place, creates slippery terrain that causes unintended movement, makes armor brittle.

**Core mechanics:**
- Spells apply `CHILLED` (slow movement/AP regen) or full `FROZEN` (skip turn, take shattering bonus damage)
- `FROZEN` units that take physical/earth/sonic damage receive `SHATTER` bonus: massively amplified damage
- Creates `ICE_TILE` terrain: high slip chance on movement (unit may slide further than intended)
- Creates `ICE_WALL` — blocks movement, can be shattered by sonic/fire
- `BRITTLE_ARMOR` debuff: next physical hit deals 50% bonus damage

**Key spells (conceptual):**
- `Frost Bolt` — single target CHILLED, moderate range
- `Blizzard` — wide AoE, applies CHILLED to all in zone + creates ICE_TILE terrain
- `Flash Freeze` — single target FROZEN (1 turn), expensive
- `Glacial Wall` — creates 3-tile ICE_WALL barrier
- `Brittle Touch` — close range, applies BRITTLE_ARMOR

**Synergies:** Sonimancer (ice + shatter = massive burst), Geomancer (ice + earth = permafrost), Electromancer (freeze conductor for arc explosion), Hydromancer (flood → freeze = mass FROZEN)
**Counters:** Pyromancer (melts everything), Thermomancer (raises temperature, prevents freezing)

---

### Geomancer
**Domain:** Earth, stone, structural terrain
**Tactical Identity:** Board architect. Geomancer changes what is and isn't passable. Creates cover, elevation changes, traps, and permanent battlefield features.

**Core mechanics:**
- Raises/lowers terrain elevation (creates hills for LOS advantage, pits for fall damage)
- Summons stone walls (permanent until destroyed)
- Creates `MUD` terrain by mixing earth with water — heavy movement penalty
- Can bury units partially (immobilize) if standing on soft ground
- Earth tiles struck by fire harden into `OBSIDIAN` — indestructible cover

**Key spells (conceptual):**
- `Stone Wall` — creates 2-tile stone barrier, blocks movement and projectiles
- `Tremor` — AoE shockwave, knocks back adjacent units and damages terrain
- `Raise Ground` — elevates a 2x2 area by 1 level (creates high ground)
- `Bury` — single target on earth tile: IMMOBILIZED for 1 turn
- `Rockslide` — targeted downhill: units on elevated tiles take fall + impact damage

**Synergies:** Hydromancer (earth + water = MUD), Pyromancer (earth + fire = obsidian walls), Gravimancer (elevation + gravity = enhanced fall combos), Osteomancer (both create physical terrain structures)
**Counters:** Aeromancer (bypasses walls via flight/displacement), Gravimancer (crushes walls)

---

### Aeromancer
**Domain:** Wind, air currents, atmospheric pressure
**Tactical Identity:** Mobility controller. Pushes enemies out of position, kites, denies areas with wind barriers, and gains exceptional movement range.

**Core mechanics:**
- Displacement spells: push enemies, pull allies, create wind corridors
- Own movement is enhanced (can cross gaps, reduced fall damage)
- `WINDWALL` terrain feature: projectiles and ranged spells are deflected through it
- `UPDRAFT` zone: units in it are lifted slightly — prevents ground effects (mud, ice, poison ground)
- Fans fire, disperses mist/clouds, sends ice shards as shrapnel

**Key spells (conceptual):**
- `Gust` — single target push 3 tiles
- `Cyclone` — AoE pull toward center point, moderate range
- `Windwall` — creates 3-tile wind barrier, deflects projectiles
- `Updraft Zone` — 2x2 area: units hovering, immune to ground states but vulnerable to aerial effects
- `Storm of Blades` — ice shards scatter in AoE if ICE terrain present (Cryomancer synergy)

**Synergies:** Pyromancer (fans flames for spread), Cryomancer (ice shards scatter via wind), Sonimancer (resonance amplified by air), Electromancer (static from wind builds charge)
**Counters:** Geomancer (walls too heavy to move), Gravimancer (grounds anything airborne)

---

### Electromancer
**Domain:** Lightning, electricity, electromagnetic force
**Tactical Identity:** Chain burst and stun. Low direct damage per bolt, but chains through conductive states (WET, FLOODED, METALLIC) to hit multiple targets.

**Core mechanics:**
- Bolts chain to adjacent WET or FLOODED units/tiles automatically
- `STUNNED` status: target loses next action
- `CHARGED` tile: next unit that steps on it receives a free lightning bolt
- Electrocuting FROZEN units causes `SHATTER` bonus (ice shatters from electrical surge)
- Can `OVERLOAD` a CHARGED zone — radius electrical explosion

**Key spells (conceptual):**
- `Arc Bolt` — single target, chains to up to 3 WET adjacent targets
- `Static Field` — applies CHARGED to a 2x2 terrain area
- `Lightning Rod` — plant a conductor; draws all lightning-type spells to it within range
- `Thunderclap` — AoE stun + knockback, no chain but reliable
- `Overload` — detonates CHARGED terrain in radius, massive AoE

**Synergies:** Hydromancer (WET = free chain to multiple), Cryomancer (FROZEN + lightning = SHATTER), Thermomancer (overheated units are more conductive), Faunamancer (metallic beast companions conduct)
**Counters:** Geomancer (ground the charge), Crystalomancer (insulating crystal barriers)

---

### Necromancer
**Domain:** Death, corpses, undead essence
**Tactical Identity:** Attrition and economy. Every enemy death is a resource. Summons are disposable frontline that delays, absorbs, and enables.

**Core mechanics:**
- Fallen units (ally or enemy) become `CORPSES` on the tile where they died
- Can raise corpses as `SKELETON` companions (low HP, melee only, expendable)
- `BONE_SPIKE` terrain: raise bone spikes from the ground, hazard tiles
- Can sacrifice own summons for spell amplification
- `DEATH_MARK`: debuff that causes corpse explosion on death

**Key spells (conceptual):**
- `Raise Dead` — targets a CORPSE tile, raises 1 Skeleton for 3 turns
- `Corpse Explosion` — detonates a CORPSE: AoE damage proportional to unit's max HP
- `Death Mark` — debuff: target explodes on death
- `Bone Wall` — raises bone barrier from ground (weaker than stone wall but costs less)
- `Soul Drain` — steals HP from nearest enemy, heals self

**Synergies:** Osteomancer (bone structures), Toximancer (poison the corpses before explosion for toxic burst), Psychomancer (DEATH_MARK + panic causes enemies to flee into each other)
**Counters:** Photomancer (holy light destroys undead), Pyromancer (burns corpses before they can be raised)

---

### Chronomancer
**Domain:** Time, temporal flow, sequence manipulation
**Tactical Identity:** Sequence breaker. Chronomancer warps initiative order, grants extra turns, delays enemies, and can partially "undo" board states.

**Core mechanics:**
- `HASTE`: target acts twice in one round (or acts before normal initiative)
- `TIME_SLOW`: target's action is delayed by 1 full turn
- `REWIND` (limited): restore a single unit to its position/HP from 1 turn ago
- Can view "ghost" of where enemies will move (telegraphed moves shown 1 turn ahead — see Into the Breach)
- `STASIS`: freezes a unit in time — invulnerable but cannot act

**Key spells (conceptual):**
- `Haste` — ally acts again this turn (limited uses)
- `Time Slow` — enemy loses next action
- `Stasis Field` — single target STASIS: immune but locked for 2 turns
- `Temporal Echo` — Echomancer synergy: repeats last spell cast by ally
- `Rewind` — restore target to state from 1 turn ago (position, HP, statuses removed)

**Synergies:** Echomancer (time echoes duplicate spell chains), any burst Mancer (Haste doubles their output), Photomancer (Stasis + reveal combos)
**Counters:** Psychomancer (confused Chronomancer may waste HASTE on wrong target), Sonimancer (disruption breaks concentration spells)

---

### Photomancer
**Domain:** Light, radiance, illumination
**Tactical Identity:** Vision controller and precision striker. Reveals hidden units, blinds enemies, and fires high-damage laser beams that punch through terrain.

**Core mechanics:**
- `BLINDED`: target cannot see beyond 1 tile range (spells and attacks penalized heavily)
- `ILLUMINATED`: removes stealth/invisibility, marks target for bonus damage by all allies
- `LIGHT_BEAM` spells pierce through cover (ignores walls, hits through terrain if in line)
- Can create `MIRROR` terrain: reflects beam spells
- Area denial via blinding flash (AoE blind)

**Key spells (conceptual):**
- `Flash` — AoE blind, 2-turn BLINDED status in cone
- `Illuminate` — marks target; all allies gain +damage against it
- `Solar Beam` — long range line attack through all obstacles, high damage
- `Place Mirror` — creates reflective tile; bounces light-type spells
- `Sunburst` — massive radial AoE, blinds all in range + light damage

**Synergies:** Crystalomancer (crystal barriers refract beams for bounce damage), Psychomancer (BLINDED + confusion = total incapacitation), Necromancer (Illuminate destroys undead)
**Counters:** Geomancer (walls block beam paths), Aeromancer (smoke/mist reduces beam range)

---

### Psychomancer
**Domain:** Mind, emotion, mental influence
**Tactical Identity:** Control and disruption. Turns enemy Mancers' abilities against them. Hardest to predict; highest skill ceiling.

**Core mechanics:**
- `CHARMED`: enemy acts on your behalf for 1 turn (turn their own spells against allies)
- `PANICKED`: enemy moves in random direction and attacks nearest unit (ally or foe)
- `CONFUSED`: enemy targets are randomized
- `MORALE_DAMAGE`: pseudo-HP pool that, when depleted, applies PANICKED automatically
- Can sense enemy intentions (see next planned action)

**Key spells (conceptual):**
- `Charm` — high cost, single target CHARMED for 1 turn
- `Terror` — cone AoE PANICKED, 1 turn
- `Mind Fog` — 3x3 area confusion zone; all units in zone have randomized targeting
- `Psychic Scream` — AoE morale damage; units at 0 morale immediately panic
- `Empathy Link` — damage one unit also damages a linked enemy (mirror damage)

**Synergies:** Necromancer (DEATH_MARK + PANICKED = enemy runs into own allies), Pyromancer (panicked enemies run into fire zones), Chronomancer (CHARMED ally can be Hasted for double action)
**Counters:** Osteomancer (bone skull helmet trait — immune to mind effects), Crystalomancer (crystal resonance blocks psychic waves)

---

### Floramancer
**Domain:** Plants, vines, botanical growth
**Tactical Identity:** Zone control and denial. Grows terrain features that restrict movement and deal passive damage over time.

**Core mechanics:**
- `ROOTED`: unit cannot move (but can still act)
- `OVERGROWTH` terrain: movement cost doubled, ranged attacks penalized
- `POISON_POLLEN` cloud: entering the zone applies POISONED
- Plants grow passively each turn if seeded (Floramancer can place seed tiles)
- Fire destroys plant terrain instantly

**Key spells (conceptual):**
- `Entangle` — single target ROOTED, 2 turns
- `Seed` — plants a SEED tile; grows into OVERGROWTH next turn
- `Pollen Cloud` — 2x2 POISON_POLLEN area, lasts 3 turns
- `Thorn Wall` — creates vine barrier; damages any unit attempting to cross
- `Bloom` — massively accelerates all plant growth on field

**Synergies:** Toximancer (POISON_POLLEN + toxin stacks), Hydromancer (water accelerates plant growth), Necromancer (overgrowth terrain preserves corpses for raise)
**Counters:** Pyromancer (burns everything instantly), Aeromancer (blows pollen clouds away)

---

### Faunamancer
**Domain:** Beasts, animal instincts, pack behavior
**Tactical Identity:** Numbers and flanking. Summons animal companions that act as additional units. Pack positioning bonuses reward smart spacing.

**Core mechanics:**
- Summons `COMPANION` units (wolf, eagle, bear variants at different tiers)
- `PACK_BONUS`: each additional companion adjacent to target adds bonus to attack
- Eagle companions have aerial movement (ignore ground terrain)
- Bear companion can body-block (takes damage meant for Faunamancer)
- `TRACKING` ability: marks an enemy; companions gain bonus movement toward marked target

**Key spells (conceptual):**
- `Summon Wolf` — fast low-HP companion, pack bonus oriented
- `Summon Eagle` — aerial scout/striker, bypasses terrain
- `Summon Bear` — tank companion, body-block
- `Pack Howl` — AoE buff: all companions gain +movement and +damage this turn
- `Track Prey` — marks target; companions ignore terrain cost when pursuing

**Synergies:** Necromancer (fallen companions become corpses for raising), Electromancer (metallic companions conduct charge), Geomancer (bear can push boulders / trigger rockslide)
**Counters:** Psychomancer (CHARMED companions turn on Faunamancer), Pyromancer (AoE fire destroys groups of companions)

---

### Toximancer
**Domain:** Poison, venom, chemical toxins
**Tactical Identity:** Attrition and debuff stacking. Damage is deferred but multiplies. Contaminates terrain for persistent zone damage.

**Core mechanics:**
- `POISONED` status: deals DoT each turn; stacks (up to 5 stacks, each stack increases damage)
- `TOXIC_TERRAIN`: ground tile deals poison DoT to units standing on it
- Poison interacts with water (contaminates flood zones), fire (toxic fumes), ice (preserved stacks)
- `ANTIDOTE_DENIAL`: one ability prevents healing from removing poison for 2 turns
- Can create `CORROSIVE` variant — damages armor/shields over time

**Key spells (conceptual):**
- `Venom Dart` — single target, 2 POISON stacks
- `Toxic Spill` — creates TOXIC_TERRAIN in AoE
- `Venomous Cloud` — moving cloud of gas, applies POISONED to units it passes through
- `Virulent Toxin` — high cost, 4 stacks + ANTIDOTE_DENIAL
- `Cascade Poison` — if target dies with 3+ stacks, spreads stacks to adjacent units

**Synergies:** Floramancer (pollen + toxin = compound poison), Necromancer (poisoned corpse explosion is AoE toxic burst), Hydromancer (contaminate flood zone = all FLOODED units get POISONED)
**Counters:** Chronomancer (REWIND removes poison stacks), Photomancer (light purifies toxins in area)

---

### Osteomancer
**Domain:** Bones, skeletal structure, calcification
**Tactical Identity:** Tank and spike trap specialist. Builds bone fortifications, hardens allies, creates hazard terrain from enemy corpses.

**Core mechanics:**
- `BONE_ARMOR`: temporary HP shield made of hardened calcium
- Creates `BONE_SPIKE` hazard tiles — deal damage + ROOT to units that step on them
- Can extract bone from CORPSE tiles, destroying corpse but gaining material for constructs
- `CALCIFY` debuff: reduces target movement speed (joints hardening)
- Bone constructs are brittle (sonic damage destroys them; physical is normal)

**Key spells (conceptual):**
- `Bone Shield` — apply BONE_ARMOR to ally (absorbs damage)
- `Spike Field` — create 3-tile BONE_SPIKE hazard line
- `Ossify` — CALCIFY debuff on target: -2 movement for 2 turns
- `Bone Golem` — summon slow but high-HP bone construct (requires CORPSE tile nearby)
- `Skeleton Rain` — ranged AoE: bone shards fall on area, dealing damage + creating BONE_SPIKE randomly

**Synergies:** Necromancer (shared corpse economy; bone construct + skeleton frontline), Cryomancer (frozen BONE_ARMOR = extra brittle but twice the HP), Geomancer (combined terrain fortification)
**Counters:** Sonimancer (shatters bone constructs and BONE_ARMOR), Gravimancer (crushes bone constructs via weight)

---

### Gravimancer
**Domain:** Gravity, mass, gravitational force
**Tactical Identity:** Displacement and burst damage. Manipulates weight and fall physics. Pairs with elevation-heavy maps and Geomancer terrain changes.

**Core mechanics:**
- `HEAVY` status: target cannot be displaced by wind/water, takes extra fall damage
- `WEIGHTLESS` status: target floats — immune to ground terrain but vulnerable to being flung
- Can pull objects (rocks, walls) as projectiles
- `CRUSH` mechanic: slam two units together for collision damage to both
- Gravity wells and repulsion fields as terrain features

**Key spells (conceptual):**
- `Gravity Well` — pulls all units in 3-tile radius toward center point
- `Repulsion Burst` — pushes all units away from caster (AoE knockback)
- `Crush` — forces two targets toward each other; collision damage scales with distance
- `Heaviness` — apply HEAVY to target: immobile + incoming fall damage amplified
- `Zero Gravity Zone` — 2x2 area: all units WEIGHTLESS, ground effects ignored

**Synergies:** Geomancer (elevation + gravity = maximum fall damage), Electromancer (gather WET units together for chain stun), Hydromancer (gravity well + torrent = whirlpool)
**Counters:** Aeromancer (fights gravity manipulation with updrafts), Osteomancer (too heavy to be easily moved)

---

### Sonimancer
**Domain:** Sound, vibration, acoustic resonance
**Tactical Identity:** Disruption and structure destroyer. Shatters ice, destroys bone/crystal constructs, interrupts spells in cast, creates deafening denial zones.

**Core mechanics:**
- `DEAFENED`: unit cannot hear (no tactical audio cues, reduced reaction speed — mechanical AP penalty)
- `SILENCED`: unit cannot cast spells for 1 turn
- `RESONANCE_CHARGE`: stacks on a target; at 3 stacks, target STUNNED and takes burst sonic damage
- Sonic waves propagate through walls (hits units in cover)
- Shatters FROZEN units if they have SHATTER vulnerability

**Key spells (conceptual):**
- `Shout` — cone AoE DEAFENED + 1 RESONANCE_CHARGE
- `Silence` — single target SILENCED for 1 turn (high value vs. spellcasters)
- `Shockwave` — line AoE, passes through walls, applies RESONANCE_CHARGE to all hit
- `Sonic Shatter` — single target: instant detonate RESONANCE_CHARGE + shatter FROZEN
- `Bass Roar` — massive AoE: destroys all ICE_WALL, BONE constructs, CRYSTAL barriers in range

**Synergies:** Cryomancer (FROZEN + SONIC SHATTER = burst damage), Osteomancer counter or ally (shatters bone enemies, but also own bone terrain), Crystalomancer (crystal amplifies sonic resonance into larger AoE)
**Counters:** Aeromancer (wind disperses sound waves, reducing range), Geomancer (thick stone walls absorb vibration)

---

### Crystalomancer
**Domain:** Crystals, prisms, resonance structures
**Tactical Identity:** Amplifier and reflector. Sets up persistent crystal structures that redirect and amplify spells, creating geometric kill zones.

**Core mechanics:**
- Places `CRYSTAL` terrain structures that persist until destroyed
- Crystal reflects LIGHT-type spells (Photomancer beams bounce)
- Crystal amplifies SONIC effects (Sonimancer resonance chain through crystals)
- Crystal `RESONATES` with stored energy — can store a spell's energy and release it on trigger
- `PRISM_BARRIER`: crystal dome that refracts incoming spells into multiple smaller beams hitting adjacent tiles

**Key spells (conceptual):**
- `Conjure Crystal` — place CRYSTAL terrain at target tile
- `Crystal Shard` — ranged attack, bounces between targets like billiards
- `Store Energy` — crystal absorbs next spell cast at it; releases when unit steps adjacent
- `Prism Barrier` — crystal dome around ally; incoming damage refracted outward
- `Resonance Pulse` — all CRYSTAL tiles on field pulse outward, dealing light damage to adjacent units

**Synergies:** Photomancer (beams bounce between crystals for multi-target hit), Sonimancer (crystals amplify shockwaves), Electromancer (crystals conduct electricity between structures)
**Counters:** Sonimancer (Bass Roar shatters all crystal terrain), Gravimancer (crushes crystal structures)

---

### Echomancer
**Domain:** Echoes, temporal repetition, afterimages
**Tactical Identity:** Multiplier and setup. Repeats previous spells (own or ally's) with delay. Punishes predictable opponents; rewards pre-planned combo chains.

**Core mechanics:**
- `ECHO` mechanic: stores the last spell cast (by any nearby Mancer) and can replay it with 1-turn delay
- `AFTERIMAGE`: leaves a false copy of self at previous position; enemies may target the afterimage
- Delayed spell triggers — can pre-load an echo to fire at a set trigger condition
- `RESONANCE_ECHO`: chain echoes of the same spell (echo of an echo) — diminishing power but multiple hits
- Cannot echo the same spell more than twice per combat (entropy mechanic)

**Key spells (conceptual):**
- `Echo` — repeat last cast spell from new position at 1-turn delay
- `Afterimage` — leave a decoy at current tile; move freely, enemy may target decoy
- `Temporal Loop` — pre-load: repeat last ally spell automatically next turn when triggered
- `Double Cast` — Chronomancer synergy: casts two echoes simultaneously this turn
- `Resonance Chain` — chain echo through 3 CRYSTAL tiles for amplified multi-hit

**Synergies:** Chronomancer (time manipulation + echo = powerful sequence control), Crystalomancer (echo chains through crystal structures), any heavy-damage Mancer (doubling their burst via echo)
**Counters:** Sonimancer (SILENCED Echomancer cannot echo), Psychomancer (CONFUSED echoes target wrong units)

---

### Thermomancer
**Domain:** Temperature, heat exchange, thermal gradients
**Tactical Identity:** State transition specialist. Governs transitions between heat and cold states. Can push Pyromancer fire hotter or Cryomancer ice colder. Combo accelerator.

**Core mechanics:**
- `OVERHEATED`: unit takes ongoing heat damage; fire damage amplified 50%; susceptible to electrical arc
- `OVERCOOLED`: movement heavily reduced; frost damage amplified; brittle (physical damage bonus)
- Creates `THERMAL_GRADIENT` zones: one side hot, one cold — crossing applies respective status
- Raises or lowers temperature of terrain tiles, enabling or disabling element interactions
- `HEAT_EXCHANGE`: transfers temperature status from one unit to another

**Key spells (conceptual):**
- `Superheat` — apply OVERHEATED to target; fire spells from allies deal +50% vs target
- `Deep Chill` — apply OVERCOOLED; cold spells amplified, movement -3 tiles
- `Thermal Zone` — create 2x4 THERMAL_GRADIENT (left hot, right cold)
- `Heat Exchange` — transfer BURNING status from ally to enemy; remove FROZEN from ally, apply to enemy
- `Thermal Collapse` — detonate when two temperature extremes meet: steam explosion / ice burst

**Synergies:** Pyromancer (OVERHEATED = their DoT ticks faster), Cryomancer (OVERCOOLED = faster freeze accumulation), Electromancer (OVERHEATED units arc electricity more readily), Hydromancer (steam generation combos)
**Counters:** Chronomancer (REWIND removes temperature status), Hydromancer (water normalizes temperature extremes)

---

## Team Building Notes

**Team size:** 3-5 Mancers per squad (recommended sweet spot: 4)

**Archetype balance goals:**
- At least 1 "setup" Mancer (applies terrain states: Hydromancer, Geomancer, Floramancer)
- At least 1 "detonator" (exploits states: Electromancer, Cryomancer, Sonimancer)
- 1 flex/utility (Chronomancer, Psychomancer, Aeromancer)
- 1 frontline/tank role (Osteomancer, Geomancer, Necromancer summons)

**Draft consideration:** Counter-picking is valid but combo-picking is more powerful. A team built around a 3-Mancer chain combo (Hydromancer → Electromancer → Aeromancer vacuum-gathers for mass chain) beats a team of individually strong picks.

---

## Synergy Quick Reference

| If you have... | Add... | For... |
|---|---|---|
| Hydromancer | Electromancer | WET chain stun AoE |
| Hydromancer | Cryomancer | Mass freeze |
| Pyromancer | Toximancer | Toxic fumes from burning terrain |
| Cryomancer | Sonimancer | FROZEN → shatter burst |
| Necromancer | Osteomancer | Shared corpse/bone economy |
| Geomancer | Gravimancer | Maximum fall damage combos |
| Photomancer | Crystalomancer | Beam bounce network |
| Echomancer | any burst Mancer | Double your damage output |
| Chronomancer | any Mancer | Free extra turn on key turn |

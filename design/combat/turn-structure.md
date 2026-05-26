# Turn Structure

## Combat Flow Overview

```
PRE-COMBAT PHASE
  → Team selection + positioning
  → Map briefing (objectives shown)

COMBAT LOOP
  → Turn order determined (initiative)
  → Each unit acts in order:
      → Move (AP)
      → Cast spell(s) (AP)
      → End turn (pass remaining AP)
  → Terrain resolution (fire spreads, clouds tick, etc.)
  → Check win condition
  → Next unit's turn

POST-COMBAT
  → Results screen
  → Mancer XP / unlocks (campaign)
```

---

## Initiative Order

**Base initiative** is a stat per Mancer type. Higher initiative = acts first.

| Mancer | Base Initiative |
|---|---|
| Aeromancer | 10 |
| Chronomancer | 9 |
| Electromancer | 8 |
| Psychomancer | 8 |
| Photomancer | 7 |
| Cryomancer | 7 |
| Echomancer | 7 |
| Pyromancer | 6 |
| Hydromancer | 6 |
| Toximancer | 6 |
| Floramancer | 5 |
| Sonimancer | 5 |
| Faunamancer | 5 |
| Thermomancer | 5 |
| Crystalomancer | 4 |
| Gravimancer | 4 |
| Osteomancer | 4 |
| Necromancer | 3 |
| Geomancer | 3 |

**Tiebreakers:** On equal initiative, player's Mancer acts before AI enemy; if PvP, random tiebreak at round start.

**Chronomancer effects on initiative:**
- `HASTE` on a unit: that unit acts as if initiative +5 this round (moves up in order)
- `TIME_SLOW` on a unit: acts as if initiative –5 (moves down in order)
- These modifications apply for current round only, then revert

**Initiative is fixed per Mancer type.** No equipment or stat scaling changes base initiative (keeps design predictable). Chronomancer is the only initiative manipulator.

---

## A Single Unit's Turn

On a unit's turn, they have 6 AP to spend. AP can be spent in any order.

### Movement
- Moving 1 tile = 1 AP
- Moving into DIFFICULT terrain (MUD, RUBBLE, OVERGROWTH) = 2 AP per tile
- Moving onto a HAZARD tile (BONE_SPIKE, ON_FIRE, TOXIC_TERRAIN) = triggers that hazard on entry
- Jumping off an ELEVATED tile costs 0 AP but triggers fall damage check
- Flying/aerial units (Eagle companion, WEIGHTLESS units) ignore ground terrain costs

### Spellcasting
- Casting a spell costs AP equal to spell tier
- Multiple spells can be cast in one turn as long as AP allows
- A 6 AP turn could be: move 2 tiles (2 AP) + Standard spell (3 AP) + Quick spell (1 AP)
- Spell resolution is immediate (no travel delay for targeted spells; projectiles travel visually but resolve on cast)

### Action Restrictions
- `STUNNED`: skip entire turn (no AP)
- `ROOTED`: cannot move, but can cast spells normally
- `SILENCED`: cannot cast spells, but can move
- `FROZEN`: skip entire turn (like STUNNED); additionally receive SHATTER vulnerability
- `CHARMED`: controlled by opponent for this turn
- `CONFUSED`: must cast but targeting is randomized within range

### Passing
- A unit can "end turn" and pass remaining AP at any time
- Unused AP is lost (no carry-over)
- Passing early signals to opponent that you have no more high-value actions this turn

---

## Terrain Resolution Phase

After all units have taken their turns, terrain resolves in order:

1. `ON_FIRE` spread check (spread to adjacent FLAMMABLE tiles)
2. `ON_FIRE` damage tick (units on burning tiles take damage)
3. `STEAM_CLOUD` damage tick and duration decrement
4. `TOXIC_TERRAIN` damage tick
5. `POISON` status tick on all POISONED units
6. `BURNING` status tick on all BURNING units
7. `FLOODED` expansion check (slow spread if active water source)
8. `OVERGROWTH` growth from SEED tiles
9. All other status duration decrements
10. Dead units become CORPSE tiles if Necromancer is on the field

---

## Win Conditions

### Standard Mode: Last Team Standing
- All enemy Mancers reduced to 0 HP
- Companions do not count toward win condition (only player-controlled Mancers)

### Objective Mode (map-dependent): Hold the Point
- Designated capture zones on map
- Controlling a zone (having a unit on it, uncontested) for N rounds wins
- Forces aggressive play; turtling is punished

### Scenario Mode (campaign): Specific Objectives
- Examples: Protect the relic (don't let enemies reach tile X), Survive N rounds, Defeat the boss unit
- Scenario objectives are communicated in pre-combat map briefing

---

## Round Limit

**No hard round limit** in standard mode (play until victory condition met).

**Soft pressure mechanic:** Starting from round 8, terrain begins to degrade passively — one random tile per turn gains `ON_FIRE` or `TOXIC_TERRAIN` state. This prevents ultra-defensive stalling and keeps late-game tense.

---

## Team Size

**Recommended competitive:** 4 Mancers per side
**Skirmish options:** 3v3, 4v4, 5v5
**Campaign:** 3v3 up to 5v5 depending on mission

**Unit cap for summons/companions:** 3 summons active per Mancer at a time. Global field cap: 12 total units per side (Mancers + summons). Prevents Necromancer/Faunamancer from filling the board.

---

## Reactions and Interrupts (Design Consideration)

**Current design:** No interrupts during another unit's turn. Turns are atomic.

**Future consideration:** Add a limited `REACTION` system:
- Each Mancer has one REACTION per round (outside their turn)
- Example reactions: Aeromancer "Gust Redirect" (redirect an incoming projectile), Crystalomancer "Absorb" (store incoming damage in crystal for later release)
- Adds depth but risks slowing down turns; scope for post-launch or late design pass

---

## Death and Corpses

When a Mancer or companion reaches 0 HP:
- Unit is removed from the field
- Their tile becomes a `CORPSE` tile (persists until used or end of combat)
- CORPSE tiles are a resource for Necromancer (raise, explode)
- CORPSE tiles are destroyed by ON_FIRE (burned) unless Necromancer's Bone Shield interaction applies
- Pyromancer can deny Necromancer by burning corpse before raise

**No revive mechanic:** Chronomancer's REWIND restores a unit to prior position/HP but cannot revive a dead unit. Once a Mancer is dead, they stay dead for the round. Campaign may have permadeath options.

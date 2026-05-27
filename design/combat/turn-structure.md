# Turn Structure

## Combat Flow Overview

```
PRE-COMBAT PHASE
  → Warband selection + map preview
  → Starting position placement

COMBAT LOOP  (each turn = one round of simultaneous activations)

  PLANNING PHASE  [both players, simultaneously and secretly]
    → Select up to 100 pts of units to activate this turn
    → Assign each activated unit: movement path, spell targets, attack targets
    → Submit plan — locked once both players submit

  RESOLUTION PHASE  [both plans execute automatically]
    → Mancers:  resolve in board position order (lowest x+y first; lowest x on tie)
    → Ranged:   resolve in board position order
    → Chaff:    resolve in board position order
    (units not in the activation plan hold position — no actions this turn)

  TERRAIN RESOLUTION  [fire spreads, clouds tick, status DoTs, duration decrements]
  WIN CONDITION CHECK
  → Next turn's Planning Phase begins

POST-COMBAT
  → Results screen
  → Mancer XP / unlocks (campaign)
```

---

## Planning Phase

At the start of each turn, both players simultaneously and secretly build their **activation plan**. Neither player sees the other's plan until resolution begins.

### The 100-Point Activation Budget

Each player may activate up to **100 points** of units per turn. Activation cost equals purchase cost: a Mancer costs 100 pts to activate, a T1 Chaff 10 pts, a T1 Ranged 25 pts, T2 variants at their full purchase cost. See `warbands.md` for the complete cost table.

- Spending fewer than 100 pts is valid — activating less is a deliberate tactical choice.
- Units **not included** in the activation plan **hold position**: no actions this turn, cooldowns still tick.
- Mancer upgrades increase purchase price but **do not change activation cost** — an upgraded Mancer still activates for 100 pts.

### What You Assign Per Unit

For each activated unit, you specify upfront:
- **Movement path** — the tile sequence the unit moves through (subject to AP budget and terrain costs)
- **Spell targets** — which spells to cast and at which targets or tile positions
- **Attack targets** — for Chaff and Ranged units, the target unit or tile

Plans are written against the **current board state**. If a target has moved or died by the time its command executes during resolution, the command resolves against the updated state at that moment: tile-targeted spells still fire at the chosen tile; unit-targeted spells with no valid target at execution time are cancelled (no AP refund).

### Submitting and Locking

Once submitted, a plan is locked. Both plans are held until the opponent also submits, at which point the turn moves to Resolution Phase. Plans can be **resubmitted freely** until the opponent locks — corrections are allowed up until both are committed.

### Status Effects That Affect Planning

The following statuses take effect during resolution, but knowing them changes what makes sense to plan:
- **STUNNED / FROZEN** — planned actions are fully cancelled at resolution. Activating these units wastes activation budget.
- **ROOTED** — planned movement cancelled at resolution; spell actions still execute from current position.
- **SILENCED** — planned spells cancelled; movement still executes.
- **CONFUSED** — planned target is overridden at resolution; unit attacks the nearest visible unit within range regardless of allegiance.
- **CHARMED** — all planned actions overridden at resolution; unit attacks the nearest ally (own team) using its highest-base-damage available spell. If no ally is in range, unit moves toward the nearest ally instead.

---

## Resolution Order

Once both plans are locked, resolution executes automatically. There is no per-Mancer initiative stat — resolution order is determined by **unit type** and **board position** only.

### Unit Type Priority

All commands from both plans resolve in three sequential windows:

1. **Mancers** — all Mancer actions from both plans, interleaved in board position order
2. **Ranged** — all Ranged unit actions from both plans, in board position order
3. **Chaff** — all Chaff unit actions from both plans, in board position order

A Mancer on either side always resolves before any Ranged or Chaff unit, regardless of positioning. This order is fixed.

### Within Each Window: Board Position Order

Within the same unit type window, the unit with the **lowest x+y coordinate sum** resolves first. On ties (equal x+y), the unit with the **lower x coordinate** resolves first. This produces a consistent, board-readable sequence from top-left toward bottom-right.

**Example:** Mancer A at (1,3) has x+y=4. Mancer B at (3,2) has x+y=5. Mancer A resolves first regardless of which player controls either.

Board position is a real resource: advancing toward lower coordinates grants a marginal resolution-order advantage within the Mancer window.

### Chronomancer Effects on Resolution Order

The Chronomancer's HASTE and TIME_SLOW spells modify a unit's position within its type's resolution window:

- **HASTE** — the HASTE'd unit resolves **first** in its type's resolution window, before all position-ordered units of the same type. If multiple units are HASTE'd in the same window, they resolve among themselves in board position order. HASTE also grants +6 AP for that activation (both effects apply simultaneously).
- **TIME_SLOW** — the TIME_SLOW'd unit resolves **last** in its type's resolution window, after all non-TIME_SLOW units of the same type. This stacks with TIME_SLOW's –2 AP penalty and cooldown-pause effect.

### Dead Units Mid-Resolution

If a unit is killed during resolution before its own action executes, its planned action is cancelled. Resolution continues in order without pause or rewind.

---

## Planned Activations

During the Planning Phase, each activated unit's actions are fully specified upfront. During the Resolution Phase, those actions execute automatically in resolution order. Each activated unit has **6 AP** to spend on actions.

### Movement
- Moving 1 tile = 1 AP
- Moving into DIFFICULT terrain (MUD, RUBBLE, OVERGROWTH) = 2 AP per tile
- Moving onto an ICE_TILE = +1 AP per tile (voluntary movement friction)
- Moving onto a HAZARD tile (BONE_SPIKE, ON_FIRE, TOXIC_TERRAIN) = triggers that hazard on entry
- Jumping off an ELEVATED tile costs 0 AP but triggers fall damage check
- Flying/aerial units (Eagle companion, WEIGHTLESS units) ignore ground terrain costs

### Spellcasting
- Casting a spell costs AP equal to spell tier
- Multiple spells can be cast in one activation as long as AP allows
- A 6 AP activation could be: move 2 tiles (2 AP) + Standard spell (3 AP) + Quick spell (1 AP)
- Spell resolution is immediate (no travel delay for targeted spells; projectiles travel visually but resolve on cast)

### Action Restrictions During Resolution

Status effects that override or cancel planned actions when they execute:
- `STUNNED`: entire planned activation skipped — no movement, no spells, no attacks
- `ROOTED`: planned movement cancelled; planned spells still execute from current position
- `SILENCED`: planned spells cancelled; planned movement still executes
- `FROZEN`: entire planned activation skipped (like STUNNED); unit also has SHATTER vulnerability
- `CHARMED`: all planned actions overridden — unit attacks the nearest ally (own team) using its highest-base-damage available spell; if no ally is in range, moves toward the nearest ally instead. Fully deterministic from board state.
- `CONFUSED`: planned target overridden — unit attacks the nearest visible unit within range regardless of allegiance. Fully deterministic from board state.

### Unused AP

Any AP not consumed during resolution is lost. There is no carry-over between turns and no benefit to withholding planned actions.

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

**Soft pressure mechanic:** Starting from round 8, terrain degradation escalates — each existing `ON_FIRE` or `TOXIC_TERRAIN` tile spreads to all eligible adjacent tiles each turn (instead of its standard 1-tile-per-2-turns spread rate). If no such states exist on the field when round 8 begins, this escalation has no effect until combat creates one. This prevents passive stalling by rapidly expanding contested fire zones and poison fields, while keeping late-game outcomes entirely tied to player actions earlier in the match. No random tile selection — degradation is a direct consequence of terrain already present on the board.

---

## Team Size

**Recommended competitive:** 4 Mancers per side
**Skirmish options:** 3v3, 4v4, 5v5
**Campaign:** 3v3 up to 5v5 depending on mission

**Unit cap for summons/companions:** 3 summons active per Mancer at a time. Global field cap: 12 total units per side (Mancers + summons). Prevents Necromancer/Faunamancer from filling the board.

---

## Reactions and Interrupts (Design Consideration)

**Current design:** No interrupts during resolution. Once both plans are locked, resolution executes uninterrupted — no player input is accepted between individual unit actions.

**Future consideration:** Add a limited `REACTION` system:
- Each Mancer has one REACTION per round, declared as part of the planning phase (pre-committed, not reactive)
- Example reactions: Aeromancer "Gust Redirect" (redirect an incoming projectile if one resolves toward a target tile), Crystalomancer "Absorb" (store incoming damage in crystal for later release)
- Adds depth but requires careful scoping — reactions must be deterministic and pre-declared to fit the simultaneous planning model; scope for post-launch or late design pass

---

## Death and Corpses

When a Mancer or companion reaches 0 HP:
- Unit is removed from the field
- Their tile becomes a `CORPSE` tile (persists until used or end of combat)
- CORPSE tiles are a resource for Necromancer (raise, explode)
- CORPSE tiles are destroyed by ON_FIRE (burned) unless Necromancer's Bone Shield interaction applies
- Pyromancer can deny Necromancer by burning corpse before raise

**No revive mechanic:** Chronomancer's REWIND restores a unit to prior position/HP but cannot revive a dead unit. Once a Mancer is dead, they stay dead for the round. Campaign may have permadeath options.

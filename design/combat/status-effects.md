# Status Effects

Status effects are categorized as either **Unit Statuses** (applied to a Mancer or companion) or **Terrain States** (applied to a tile). See `terrain-system.md` for terrain states. This document covers unit statuses.

---

## Unit Status Reference

### Incapacitation Statuses

| Status | Applied By | Effect | Duration | Removed By | Notes |
|---|---|---|---|---|---|
| `STUNNED` | Electromancer, Sonimancer, Gravimancer | Skip entire turn, no AP | 1 turn | Expires naturally | Strongest control; expensive spells to apply |
| `FROZEN` | Cryomancer | Skip turn + SHATTER vulnerability (incoming physical/sonic dmg ×2.5) | 1 turn | Fire spell (melts to WET) | Physical burst combo bait |
| `ROOTED` | Floramancer, Osteomancer spike | Cannot move; can still cast | 1-2 turns | Aeromancer wind, Geomancer | Does not prevent spellcasting |
| `SILENCED` | Sonimancer, Psychomancer | Cannot cast spells; can move | 1 turn | Expires, Chronomancer rewind | Movement still allowed |
| `STASIS` | Chronomancer | Cannot act OR be affected; invulnerable | 2 turns | Expires only | Strategic not punish: use to save an ally |
| `CHARMED` | Psychomancer | Controlled by opponent for this turn | 1 turn | Expires | Rare; opponent uses your spells |
| `CONFUSED` | Psychomancer | Targeting randomized within range | 2 turns | Expires, Photomancer purify | Still uses AP/spells; unpredictable |

### Damage-Over-Time Statuses

| Status | Applied By | Effect | Duration | Removed By | Stacks? |
|---|---|---|---|---|---|
| `BURNING` | Pyromancer, ON_FIRE terrain | 5 HP/turn | Until extinguished | Water spell, Hydromancer | No (single instance; refresh on re-apply) |
| `POISONED` | Toximancer, Floramancer, TOXIC_TERRAIN | 3 HP/turn per stack | Until removed | Cleanse, Hydromancer cleanse, Chronomancer REWIND | Yes — up to 5 stacks (5×3 = 15 HP/turn at max) |
| `OVERHEATED` | Thermomancer | 4 HP/turn; fire dmg received +50% | 3 turns | Cryomancer chill, Hydromancer | No |
| `OVERCOOLED` | Thermomancer | 3 HP/turn; cold dmg +50%; move –2 | 3 turns | Pyromancer heat, Thermomancer | No |
| `BLEEDING` | (future) Physical attacks | 2 HP/turn | Until healed | Heal abilities | Yes — up to 3 stacks |

### Debuff Statuses

| Status | Applied By | Effect | Duration | Notes |
|---|---|---|---|---|
| `CHILLED` | Cryomancer | Movement –2; AP regen from Chronomancer halved | 2 turns | Precursor to FROZEN |
| `SLOWED` | Mud terrain, Hydromancer | Movement –1 | While on terrain or 1 turn off | General movement penalty |
| `BLINDED` | Photomancer, bright explosions | Targeting range reduced to 1; ranged attacks penalized heavily | 2 turns | Big debuff vs. ranged Mancers |
| `DEAFENED` | Sonimancer | No audio cues; –1 AP effective (represents disrupted focus) | 2 turns | Mechanical: removes 1 AP from next action |
| `CALCIFIED` | Osteomancer | Movement –2; physical armor +15% (double-edged debuff) | 2 turns | Slow but tankier |
| `BRITTLE_ARMOR` | Cryomancer | Next physical hit deals +50% damage to this unit | Until triggered (1 hit) | Single-trigger debuff |
| `HEAVY` | Gravimancer | Cannot be displaced by wind/water; fall damage ×2 | 2 turns | Protects from knockback; harms when falling |
| `WEIGHTLESS` | Gravimancer | Floating; immune to ground terrain states; displacement easier | 2 turns | Immune to mud/ice/toxic ground |
| `DEATH_MARK` | Necromancer | On death: explode for AoE damage (scales with max HP) | Until death triggers | Powerful but requires target to die to resolve |

### Resonance Statuses (Stacking)

| Status | Applied By | Effect | Max Stacks | On Max Stacks |
|---|---|---|---|---|
| `RESONANCE_CHARGE` | Sonimancer | +dmg per stack when sonic spell hits | 3 | Auto-STUN + sonic burst (SHATTER on FROZEN) |
| `MORALE_DAMAGE` | Psychomancer | Psychological pressure; at 0 morale → PANICKED | Scaled (0-100 pool) | Auto-PANICKED (random movement + attack) |

### Buff Statuses

| Status | Applied By | Effect | Duration |
|---|---|---|---|
| `HASTE` | Chronomancer | +6 AP this turn (double action) | 1 turn |
| `BONE_ARMOR` | Osteomancer | Absorb X damage (temporary HP shield) | Until depleted |
| `ILLUMINATED` | Photomancer | All allies deal +20% damage to this target | 2 turns |
| `PACK_BONUS` | Faunamancer | Per adjacent companion: +2 attack | While adjacent |
| `UPDRAFT` | Aeromancer zone | Immune to ground terrain effects | While in zone |
| `STEALTHED` | (future) | Cannot be targeted until attacking | Until attack |

---

## Status Interaction Matrix

Key interactions between unit statuses when multiple apply simultaneously:

| Status A | Status B | Interaction |
|---|---|---|
| `WET` | `BURNING` | BURNING is extinguished; unit loses 1 BURNING stack, becomes WET |
| `WET` | `LIGHTNING spell` | Lightning chains to all adjacent WET units (no extra status; just spread) |
| `FROZEN` | `BURNING spell` | Melt: FROZEN removed, unit becomes WET briefly |
| `FROZEN` | `Physical/Sonic dmg` | SHATTER: damage ×2.5 (one-time bonus; FROZEN removed) |
| `POISONED` | `FROZEN` | Stacks preserved (no decay timer while frozen) |
| `POISONED` | `BURNING` | Toxic combustion: POISONED converts to BURNING (higher dmg) + AoE toxic splash |
| `POISONED` | `BURNING terrain nearby` | Fumes rise: nearby units gain 1 POISONED stack |
| `OVERHEATED` | `BURNING` | Combined heat DoT: +3 HP/turn bonus damage (stacks additively) |
| `OVERCOOLED` | `CHILLED` | CHILLED replaced by OVERCOOLED (the stronger state) |
| `OVERCOOLED` | `FROZEN spell` | Immediate FROZEN skip (no CHILLED progression needed) |
| `CHARMED` | `SILENCED` | SILENCED takes priority over CHARMED: charmed unit cannot cast but also won't |
| `STASIS` | anything | Nothing affects STASIS unit until it expires (fully immune state) |
| `WEIGHTLESS` | `HEAVY` | Cancel each other; unit returns to normal weight |
| `HEAVY` | `Gravity Well` | HEAVY units resist pull; Gravimancer needs higher AP cost to move them |
| `BLINDED` | `CONFUSED` | Both apply; stacking worst-case for target (random aim + close range only) |
| `RESONANCE_CHARGE` (max 3) | next sonic dmg | STUN + burst damage (and SHATTER if also FROZEN) |
| `DEATH_MARK` | unit dies | Explosion resolved immediately on death tile; AoE scales with max HP |

---

## Removal / Cleanse Methods

Not all statuses should be easily removable. Priority removal targets are DoTs and incapacitations.

| Method | Removes |
|---|---|
| Hydromancer `Cleanse` | BURNING, POISONED (all stacks), WET (self) |
| Photomancer `Sunburst` | BURNING, POISONED (area), undead debuffs |
| Chronomancer `Rewind` | All statuses on target (reverts to prior state) |
| Geomancer `Earthshield` | BURNING (smothers with earth) |
| Cryomancer on BURNING unit | Extinguish BURNING |
| Water spell on BURNING unit | Extinguish BURNING |
| Expires | Most statuses after their duration |
| Thermomancer `Heat Exchange` | Transfers status to another unit (not removes; redirects) |

**Intentionally non-removable:**
- `DEATH_MARK` — resolves only on death; cannot be dispelled
- `RESONANCE_CHARGE` — must be expended by sonic damage or expires after 3 turns unused
- `STASIS` — expires on its own; cannot be removed early (by design — it's a commitment)

---

## Status Stacking Rules Summary

| Stacks? | Statuses |
|---|---|
| **Stacks (multiple instances active)** | POISONED (up to 5), RESONANCE_CHARGE (up to 3), BLEEDING (up to 3) |
| **Refreshes (duration reset, no stack)** | BURNING, CHILLED, STUNNED, ROOTED, BLINDED |
| **Replaced by stronger version** | CHILLED → FROZEN; SLOWED → ROOTED; CHILLED + OVERCOOLED |
| **Cannot stack or refresh while active** | CHARMED, STASIS, CONFUSION (must expire first) |
| **Persists through other states** | POISONED through FROZEN; DEATH_MARK through all |

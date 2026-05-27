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
| `CONFUSED` | Psychomancer | Targets the nearest visible unit within range regardless of allegiance (friend or foe) | 2 turns | Expires, Photomancer purify | Fully deterministic from board state; skilled opponents position to exploit friendly-fire risk |

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
| `MORALE_DAMAGE` | Psychomancer | Psychological pressure; at 0 morale → PANICKED | Scaled (0-100 pool) | Auto-PANICKED: unit flees from nearest enemy (full move range, toward map edge if no enemy visible); attacks nearest unit in range regardless of allegiance using lowest-AP-cost ability. Fully deterministic from board state. |

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
| `BLINDED` | `CONFUSED` | Both apply; stacking worst-case for target: CONFUSED nearest-unit targeting restricted to BLINDED's reduced range (1 tile), guaranteeing friendly-fire at close range |
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

---

## Temperature-Triggered Statuses

The **Temperature system** (see `temperature-system.md`) is a per-unit integer variable [-100, +100] that applies and removes statuses automatically when threshold boundaries are crossed. Three existing status types are used as temperature-triggered statuses: `BURNING`, `SLOWED`, and `FROZEN`.

### Which Statuses Temperature Can Trigger

| Temperature Range | State | Status Triggered |
|---|---|---|
| ≥ +61 | OVERHEATED | `BURNING` — 5 HP/turn DoT |
| +31 to +60 | HOT | `SLOWED` — move range -1 |
| -31 to -60 | SUPERCOOLED | `SLOWED` — move range -1; also applies BRITTLE_MODIFIER (incoming physical +50%) |
| ≤ -61 | FROZEN SOLID | `FROZEN` — unit cannot move; turn skipped; SHATTER vulnerability (×2.5 physical/sonic damage) |

The WARM (+1 to +30) and COLD (-1 to -30) ranges have no triggered statuses — they apply passive damage modifiers instead (incoming fire/ice spells deal +10% damage respectively) and are handled entirely inside `TemperatureManager`, not via the status system.

### The Temperature-Held Rule

Status effects applied by temperature use a **temperature-held** mechanism: their duration does not decrement each turn as long as temperature keeps them in the triggering range.

- **`StatusManager.TickStatuses`** skips duration decrement for statuses with the temperature-held sentinel duration value.
- The status is **removed immediately** when `TemperatureManager` detects that temperature has left the triggering range (e.g., BURNING is removed when temperature drops below +61).
- If temperature returns to the triggering range, the status is **reapplied** on the next threshold check.

This means temperature-triggered statuses behave more like **persistent conditions** than expiring debuffs, for as long as temperature stays in range.

### Duration Expiry Without Temperature

If a temperature-held status was force-removed by a cleanse spell (e.g., Hydromancer `Cleanse` removes BURNING), the unit is temporarily free of that status. However, if temperature remains in the triggering range at the next threshold check (end of activation or end of turn), the status is **reapplied**. Cleanse provides a one-turn reprieve, not a permanent cure against temperature-held effects.

### Dual-Source Stack Interaction

When the same status type is applied both by temperature AND by an independent source (a direct spell, terrain effect, or AoE):

- **BURNING from OVERHEATED + BURNING from direct fire spell / BURNING terrain:** The temperature-held application takes precedence for duration purposes. The status persists as long as **either** source is active. Specifically:
  - While temperature remains ≥ +61, BURNING persists regardless of what any direct spell's duration would have been.
  - When temperature drops below +61, BURNING is removed by `TemperatureManager`. If a direct spell application also had remaining duration, that application is also removed — the temperature system's removal takes precedence.
  - **Exception:** If BURNING was applied by both temperature and a direct spell simultaneously, and the direct spell has remaining duration after temperature drops, the behavior is: BURNING is removed by temperature exit, then the direct-source BURNING is immediately reapplied by the spell's own source tracking. This maintains correct behavior without requiring `StatusEffect` to track multiple simultaneous sources.

- **SLOWED from HOT or SUPERCOOLED + SLOWED from terrain (Mud tile, Hydromancer):** Both are active simultaneously. The status persists as long as **either** source remains. When temperature exits the HOT/SUPERCOOLED range, `TemperatureManager` removes SLOWED. If the terrain source still applies SLOWED on the same end-of-turn pass, it will be reapplied from the terrain source.

- **FROZEN from FROZEN SOLID + FROZEN from Cryomancer spell:** The temperature-held application has the sentinel (effectively infinite) duration. The FROZEN status persists. When temperature rises above -61, the temperature-held FROZEN is removed. If the Cryomancer spell had remaining duration, that FROZEN instance was overwritten by the replace-if-longer stacking rule when temperature-held was applied (since `int.MaxValue / 2` is longer than any spell duration). The unit is no longer FROZEN once temperature exits FROZEN SOLID range, unless the Cryomancer reapplies it directly.

### BRITTLE_MODIFIER in SUPERCOOLED Range

BRITTLE_MODIFIER is not tracked as a `StatusEffect` instance. Instead, `TemperatureManager.GetCategory` is called during incoming physical damage resolution inside `SpellResolver`. If the category returns `Supercooled`, a ×1.5 multiplier is applied to the damage calculation.

This differs from spell-applied `BRITTLE_ARMOR` (which is a single-trigger `StatusEffect` that removes itself on the first hit). Temperature-driven BRITTLE_MODIFIER reapplies each time a physical hit lands while the unit remains SUPERCOOLED — it is a persistent zone modifier, not a one-time trigger.

# Temperature System

## 1. Overview

Temperature is a per-unit integer variable that tracks the thermal state of every unit on the battlefield. It ranges from **-100** (absolute cold) to **+100** (maximum heat), with **0** representing neutral/ambient temperature. Temperature changes based on spells cast against a unit, the terrain the unit stands on, and natural per-turn decay back toward neutral.

Temperature serves two core design pillars:

**Skill lives in reading the board.** A unit's temperature is always visible to both players. This transforms thermal state into a legible strategic resource: a unit sitting at +45 is one heavy ice spell away from being locked down in FROZEN SOLID, and a player who understands that can plan around it. A unit at -55 is one fire spell away from triggering Thermal Shock. Skilled players manage temperatures proactively; unskilled players react too late.

**Depth through interaction density.** Temperature adds a continuous numeric axis to every unit interaction. Rather than binary state flips (burning/not burning), every spell nudges a unit along a spectrum that has multiple threshold effects. A Pyromancer's repeated hits don't just deal damage — they accumulate heat that opens up Thermomancer synergies and makes the unit vulnerable to further fire combos. Temperature also cross-links with the existing element combo system: a WET unit hit by cold spells drops faster toward FROZEN SOLID, while a BURNING unit's temperature resists cold.

Temperature is **not a replacement** for the existing elemental status system. It is an additional layer that interacts with it. A unit can be BURNING (a status applied by a direct spell or terrain) while also having temperature 0; conversely, a unit can reach OVERHEATED (temperature ≥ +61) and acquire BURNING via temperature thresholds while not standing on a BURNING tile.

---

## 2. The Temperature Scale

The full range is clamped to [-100, +100]. Threshold checks occur at **end of each unit activation** and **end of turn**.

| Temperature Range | State Name | Status Effect Triggered | Gameplay Effect |
|---|---|---|---|
| ≥ +61 | OVERHEATED | BURNING (DoT 5 dmg/turn) | Unit is critically hot; fire DoT applies. Status held while temp stays ≥ +61. |
| +31 to +60 | HOT | SLOWED (move range -1) | Heat fatigue slows movement. Status held while temp stays in range. |
| +1 to +30 | WARM | None | Passive modifier: incoming fire spells deal +10% damage. No status applied. |
| 0 | NEUTRAL | None | Baseline. No passive modifiers. |
| -1 to -30 | COLD | None | Passive modifier: incoming ice spells deal +10% damage. No status applied. |
| -31 to -60 | SUPERCOOLED | SLOWED (move range -1) + BRITTLE_MODIFIER | Movement impaired; next physical hit deals +50% damage (same as BRITTLE_ARMOR status trigger). |
| ≤ -61 | FROZEN SOLID | FROZEN (cannot move, turn skipped) | Unit is critically cold; FROZEN status applied, enabling SHATTER combos. |

**BRITTLE_MODIFIER** in the SUPERCOOLED range uses the existing `BRITTLE_ARMOR` status behavior: the modifier applies once per hit and resets after triggering. Unlike spell-applied BRITTLE_ARMOR (which is a single-trigger that removes itself), the temperature-held version reapplies each time a physical hit lands while the unit remains in the SUPERCOOLED range.

**Threshold persistence:** Status effects triggered by temperature thresholds do not expire by duration countdown while temperature keeps them in the triggering range. See section 7 (Status Integration) for full details.

---

## 3. Natural Decay

Each turn, during `TurnManager.ResolveTurn`, **before** any command executes:

- Every living unit's temperature moves **10 points toward 0**.
- If temperature is already 0, no change.
- Positive temperatures decrease by 10 (minimum 0 if less than 10 remains).
- Negative temperatures increase by 10 (maximum 0 if less than 10 remains in magnitude).

**Why decay exists:** Without decay, temperature would be a pure accumulator — one Pyromancer could permanently lock an enemy into OVERHEATED by casting one hot spell per turn to offset decay. Decay creates a **tension** between heating/cooling investment and natural equilibration. A temperature of +55 (HOT) requires active maintenance; if the Pyromancer gets pushed back or silenced, the enemy unit will naturally cool over 5-6 turns back to neutral. This prevents permanent lockdown and ensures temperature effects are earned each turn.

**Strategic implication:** Decay means temporary temperature states are recoverable. A unit that reaches FROZEN SOLID (-61) will return to COLD (-11) within 5 turns without any warming spells. This gives the defending player a path out of thermal lockdown, rewarding the attacker for pressing the advantage while the window is open.

---

## 4. Temperature Changes by Element

These are baseline reference values. Specific spell AP tiers produce different deltas within these ranges. Mancer-specific spell implementation applies these values to their spell definitions.

| Element / Source | Quick (1-2AP) | Standard (3AP) | Heavy (4-5AP) | Ultimate (6AP) | Notes |
|---|---|---|---|---|---|
| **Fire** | +15 | +20 to +25 | +30 to +35 | +40 to +50 | Primary heat source |
| **Ice** | -15 | -20 to -25 | -30 to -35 | -40 to -50 | Primary cooling source |
| **Water** | -5 | -10 | -15 | -20 | Evaporative cooling; also extinguishes |
| **Wind** | -5 | -5 | -5 | -10 | Airflow cooling; minimal temp effect |
| **Lightning** | +10 | +10 | +10 | +15 | Electrical resistance heating |
| **Earth** | 0 | 0 | 0 | 0 | Thermal insulator; no temp change |
| **Poison** | 0 | 0 | 0 | 0 | Chemical, not thermal |
| **Light** | +5 | +5 | +5 | +10 | Mild radiant warming |
| **Necrotic** | -5 | -5 | -5 | -10 | Death chill |
| **Thermal (Thermomancer)** | ±30 | ±35 | ±45 | ±50 | Most powerful; can be heating OR cooling |
| **Sound** | 0 | 0 | 0 | 0 | Vibration at this scale doesn't transfer heat |
| **Crystal** | 0 | 0 | 0 | 0 | Stores energy; doesn't generate heat itself |
| **Psychic** | 0 | 0 | 0 | 0 | Mental force; no physical temperature effect |
| **Gravity** | 0 | 0 | 0 | 0 | Tidal force; no thermal component |

**Thermomancer note:** Thermal element spells can be either heating or cooling depending on the specific spell. `Heat Exchange` can transfer temperature between units rather than adding or subtracting from the global pool. `Thermal Spike` applies maximum positive delta; `Deep Freeze Pulse` applies maximum negative delta. Thermomancer is the primary practitioner of deliberate Thermal Shock (see section 6).

---

## 5. Terrain Passive Temperature Effects

Applied at **end of turn resolution**, after all commands have executed, after `StatusManager.TickStatuses()`. Applied to all living units based on their current tile's state.

| Tile State | Temperature Change per Turn | Notes |
|---|---|---|
| `TileState.Burning` | +10 | Standing in fire heats the unit |
| `TileState.Frozen` | -10 | Ice floor conducts cold upward |
| `TileState.Permafrost` | -10 | Deep freeze; same delta as Frozen but harder to remove |
| `TileState.Wet` | -5 | Evaporative cooling from water contact |
| All other states | 0 | No passive temperature change |

**Interaction with decay:** Terrain temperature changes apply **after** natural decay has already occurred. This means a unit standing on a BURNING tile in equilibrium (where the +10 terrain matches the decay) will stabilize around temperature +10 to +20 rather than constantly rising — the decay absorbs part of the terrain's heat input. A unit standing on both a BURNING tile AND taking fire spells will accumulate heat much faster as both sources push temperature up before decay has time to offset.

**Terrain chain note:** When terrain temperature pushes a unit across a threshold (e.g., standing on PERMAFROST drops a SUPERCOOLED unit to -61 or below), the threshold check fires immediately at the point of application, same as any other temperature change. This means standing on certain tiles can trigger status effects at end of turn without any spells being cast.

---

## 6. Thermal Shock

**Trigger condition:** A single spell application that moves a unit's temperature from one side of ±31 to the other side — specifically:

- Temperature was ≤ -31 (SUPERCOOLED or FROZEN SOLID) and the new temperature is ≥ +31 (HOT or OVERHEATED) after the delta is applied; OR
- Temperature was ≥ +31 (HOT or OVERHEATED) and the new temperature is ≤ -31 (SUPERCOOLED or FROZEN SOLID) after the delta is applied.

This means crossing through the full WARM/COLD band in a single hit — a jump of at least 62 temperature points that crosses the 0 boundary.

**Effect:**
1. **Bonus damage:** `|temperature_delta| / 2` (integer division). For a delta of 80 (e.g., from +35 to -45), bonus damage = 40.
2. **1-turn STUN:** Applied via `StatusManager.ApplyStatus` using `StatusType.Stunned`, duration 1.
3. **Event:** `TemperatureChangedEvent` is published with `ThermalShockTriggered = true` and `ThermalShockDamage` set to the computed bonus.

**Design intent:** Thermal Shock creates a high-risk, high-reward combo opportunity specifically for the **Thermomancer**, who has the only spells with large enough temperature deltas (±30 to ±50) to cross both thresholds in one hit. It also creates a soft deterrent against stacking cold effects on a unit that a friendly Pyromancer is also targeting — if both land in the same resolution window, the combined delta could trigger a self-inflicted Thermal Shock on the target that neither player planned.

**Thermomancer interaction:** Thermomancer's `Heat Exchange` mechanic transfers temperature from one unit to another. If the donor unit was at +50 and the receiver was at -40, the exchange could push the receiver from -40 to +15 (delta of +55) — crossing both thresholds if the exchange amount is large enough. This is intentional; Thermomancer mastery includes knowing how to trigger Thermal Shock via indirect temperature manipulation.

---

## 7. Status Integration

### Temperature-held vs. Duration-held

Status effects triggered by temperature thresholds operate under a **temperature-held** rule that overrides the normal duration-countdown behavior:

- While a unit's temperature remains in the triggering range, the corresponding status effect **does not lose duration turns**.
- The `StatusManager` will not decrement duration for temperature-held statuses during `TickStatuses`.
- When temperature exits the triggering range (either by decay, cooling, or warming spells), the status:
  - Is **removed immediately** if it had no independent duration beyond the temperature hold.
  - Continues for its remaining duration if it was also independently applied by a spell or terrain.

### Dual-source status stacking

When the same status type is applied both by temperature and by an independent source (spell, terrain, or AoE):

| Source A | Source B | Resolution |
|---|---|---|
| Temperature OVERHEATED → BURNING | Direct fire spell or BURNING terrain also applies BURNING | Use the **higher-source duration**. If temperature hold is active (no duration countdown), the BURNING persists until temp drops below +61 regardless of spell duration. |
| Temperature HOT → SLOWED | Mud terrain also applies SLOWED | Both sources are active simultaneously. Status persists as long as **either** source is active. Duration expires only when temperature exits HOT range AND the terrain source expires. |
| Temperature FROZEN SOLID → FROZEN | Cryomancer spell applies FROZEN | The temperature-held FROZEN takes precedence. The spell-applied FROZEN duration provides a floor: if temperature rises above -61 but the spell FROZEN still has turns remaining, FROZEN persists for those turns. |

### Removal interactions

- **Water spell on OVERHEATED unit:** Water applies -5 to -15 temperature. If this drops temperature below +61, the temperature-held BURNING is removed. The water spell does NOT independently extinguish BURNING in this case — the temperature drop is the removal trigger.
- **Cryomancer chill on HOT unit:** Drops temperature by -20 to -25. If temperature falls below +31, the temperature-held SLOWED is removed.
- **Cleanse (Hydromancer, etc.):** Force-removes the BURNING or SLOWED status regardless of temperature. The unit may immediately reacquire the status at the next threshold check if temperature remains in the triggering range. Cleanse is a one-turn reprieve against temperature-held statuses, not a permanent cure.

### SUPERCOOLED BRITTLE_MODIFIER interaction

BRITTLE_MODIFIER in the SUPERCOOLED range is not tracked as a full `StatusEffect` instance (no duration, no stacks). Instead, `TemperatureManager` checks the category during incoming physical damage resolution and applies the +50% modifier directly if the unit's current category is SUPERCOOLED. This differs from the spell-applied `BRITTLE_ARMOR` status, which is a one-time trigger that removes itself on the first hit.

---

## 8. Frontend Display Specification

### Thermometer Bar

Each unit's HUD element includes a small **thermometer bar** displayed below the unit portrait/health bar.

**Visual spec:**
- Horizontal gradient bar, approximately 60px wide × 6px tall at standard resolution.
- Color gradient (left to right): `#1A4ECC` (deep blue) → `#4FC3F7` (cyan) → `#9E9E9E` (neutral gray) → `#FF8C00` (orange) → `#D32F2F` (deep red).
- The fill marker (a small vertical tick, 2px wide, 10px tall) indicates the current temperature position on the gradient.
- Center point of the bar corresponds to temperature 0 (NEUTRAL).
- Full left corresponds to -100; full right corresponds to +100.
- Threshold markers at -61, -31, +31, and +61 are shown as faint notches on the bar border.

**Colorblind-friendly mode:**
- In addition to the color gradient, threshold zones use **icons** overlaid at the threshold markers:
  - ≤ -61: Snowflake icon (crystalline, white)
  - -31 to -60: Icicle icon (light blue)
  - +31 to +60: Flame low icon (amber)
  - ≥ +61: Flame high icon (red)
- The fill marker in colorblind mode also uses a distinct shape: filled circle for hot, empty circle for cold, diamond for neutral.

**Numeric readout:**
- When a unit is selected (or hovered), the HUD expands to show the numeric temperature value (e.g., "+47" or "-33") next to the bar.
- In compact mode (unselected unit), the number is hidden; only the bar position is visible.

**Threshold-crossing flash animation:**
- When temperature crosses a threshold boundary (in either direction), the thermometer bar flashes the appropriate zone color for 0.4 seconds.
- Specifically: crossing into OVERHEATED or FROZEN SOLID plays a more dramatic full-bar pulse animation (duration 0.6s, two pulses) to signal the status trigger.
- The VFX layer is triggered by `TemperatureChangedEvent.PreviousCategory != NewCategory` — the presentation layer subscribes to this event and plays the appropriate animation.

**World-space indicator (in-game):**
- A small colored ring at the base of the unit sprite reflects current temperature state:
  - NEUTRAL: no ring (or very faint gray)
  - WARM: faint orange tint
  - HOT: orange ring with heat shimmer particles
  - OVERHEATED: bright red ring with fire embers
  - COLD: faint blue tint
  - SUPERCOOLED: blue ring with frost particles
  - FROZEN SOLID: solid white-blue ring with ice crystal effect

---

## 9. Interaction with Existing Systems

### ElementResolver integration

The existing `ElementResolver` (interaction matrix: WET + LIGHTNING = chain stun, etc.) continues to operate on elemental **states** and **statuses** as defined. Temperature is an orthogonal axis that does not replace or short-circuit element combos.

Key intersection points:

- **WET + LIGHTNING chain:** Still triggers normally. If the WET unit is also OVERHEATED (via temperature), the BURNING status does not prevent the chain — both fire off independently.
- **FROZEN + physical/sonic = SHATTER:** FROZEN applied via temperature (FROZEN SOLID state) grants the full SHATTER vulnerability, identical to spell-applied FROZEN. SHATTER triggering removes the FROZEN status, which then triggers temperature recheck — if temperature is still ≤ -61, FROZEN is immediately reapplied at the next threshold check.
- **BURNING (status) extinguish by water:** The water spell interaction check looks for the `StatusType.Burning` flag on the unit. Temperature-held BURNING carries the same flag. Extinguishing via water removes the status, but if temperature remains ≥ +61, BURNING will be reapplied at next threshold check. The water spell still applies its cooling delta (-5 to -15), which may pull temperature below the +61 threshold and prevent reapplication.

### StatusManager threshold interaction

`TemperatureManager` calls `StatusManager.ApplyStatus` and `StatusManager.RemoveStatus` to enforce threshold statuses. The StatusManager's stacking rules apply:

- For **Burning** (duration-stacking): `TemperatureManager` applies it with a sentinel duration value of `int.MaxValue / 2` (approximately 1 billion turns) to indicate temperature-hold. The `StatusManager.TickStatuses` path checks for this sentinel and skips duration decrement for temperature-held instances.
- For **Slowed** (duration-stacking): Same sentinel approach.
- For **Frozen** (replace-if-longer): `TemperatureManager` applies it with the same sentinel value, ensuring it is never replaced by a shorter spell-duration FROZEN while temperature holds it.

*Implementation note: The sentinel value approach avoids adding a new field to `StatusEffect`. An alternative implementation would use a `IsTemperatureHeld` boolean on `StatusEffect`; this is left as a future refactor if the sentinel becomes unwieldy.*

### SpellResolver damage computation

SpellResolver calculates damage for a spell hit. Temperature modifiers are applied as multipliers during this computation:

1. Check target unit's `TemperatureCategory` via `TemperatureManager.GetCategory(unit.Temperature)`.
2. If category is WARM (+1 to +30) and spell element is Fire: multiply base damage by 1.10.
3. If category is COLD (-1 to -30) and spell element is Ice: multiply base damage by 1.10.
4. Apply after all other modifiers (BRITTLE_ARMOR, ILLUMINATED, etc.) but before damage flooring.

BRITTLE_MODIFIER for SUPERCOOLED is checked in the physical damage path: if `GetCategory` returns `Supercooled`, apply ×1.5 to physical/sonic incoming damage.

---

## 10. Design Intent

Temperature rewards skilled play in three interconnected ways:

**Temperature as a strategic resource:** Before casting an expensive Ultimate spell, a skilled player sets up the temperature condition that amplifies it. A Pyromancer spends two turns applying +20 to +25 fire spells to push an enemy into WARM (+15 to +20), then lands the Ultimate (+50) to hit OVERHEATED (+65). The +10% damage bonus in WARM helped make each setup hit more efficient, and the Ultimate hits for full bonus. An unskilled player just casts the Ultimate without setup and gets baseline damage.

**Cooling enemies before ice combos:** Cryomancer's FROZEN SOLID state requires reaching ≤ -61. A Thermomancer teammate applies a -35 Thermal chill; natural decay brings it to -25 the next turn; then the Cryomancer's standard -25 freeze spell pushes it to -50, and the Cryomancer's Quick spell (-15) finishes at -65 (FROZEN SOLID). Without temperature, the Cryomancer would need 3-4 spells targeting one unit to reach FROZEN. With temperature setup, it takes 1 teammate action + 2 Cryomancer actions — the combo window is tighter and the payoff is clearer.

**Overheating enemies before fire synergy:** The flip side of cooling — a unit at +55 (HOT, SLOWED) is vulnerable to Thermal Shock from a single Cryomancer hit that pushes from +55 to -10 (delta -65, crossing both thresholds). A unit at +30 requires a much larger delta for the same Thermal Shock. Teams that invest in heating enemies create fragile states where a single large cold hit can trigger a chain of: Thermal Shock damage + 1-turn STUN + temperature reset to cold range (potentially triggering SUPERCOOLED SLOWED). Managing which units are in what temperature range — keeping your own units near neutral, pushing enemies toward extremes — is the core mastery expression of the temperature system.

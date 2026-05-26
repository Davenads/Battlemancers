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

## 7. Extended Temperature Mechanics

Four additional mechanics layer on top of the core temperature system. All four are implemented in `TemperatureManager` and evaluated in the order listed below within `ApplyTemperatureChange`.

---

### 7.1 Threshold Burst

**Rule:** When a single temperature application crosses one or more harmful tier boundaries in one hit, deal 5 bonus damage per boundary crossed to the affected unit.

**Harmful tier boundaries (Threshold Burst triggers):**

| Boundary | Direction | Condition |
|---|---|---|
| Entering HOT | Heating | Previous temp ≤ +30, new temp ≥ +31 |
| Entering OVERHEATED | Heating | Previous temp ≤ +60, new temp ≥ +61 |
| Entering SUPERCOOLED | Cooling | Previous temp ≥ -30, new temp ≤ -31 |
| Entering FROZEN SOLID | Cooling | Previous temp ≥ -60, new temp ≤ -61 |

**Not Threshold Burst triggers:**
- NEUTRAL → WARM (+1 to +30): WARM has no status effect; crossing into it is not harmful.
- WARM → NEUTRAL: Moving back toward center; not a harmful tier entry.
- COLD → NEUTRAL: Same reason.

**Multi-boundary example:** A Thermomancer's Thermal Spike hits a unit at temperature 0 for +70 delta, pushing it to +70 (clamped to +70). The spike crosses both the +30 boundary (into HOT) and the +61 boundary (into OVERHEATED) in one application — 2 procs × 5 damage = **10 total Threshold Burst damage**.

**Implementation details:**
- Damage applied directly to `UnitState.CurrentHP`, floored at 0.
- Published as `UnitDamagedEvent` with `DamageSource = "temperature_threshold_burst"`.
- Evaluated before Flash Freeze Rupture and Thermal Shock in `ApplyTemperatureChange`.
- Does NOT itself trigger further temperature changes.

**Design intent:** Threshold Burst creates a tactically meaningful cost to dramatic temperature swings. A spell that barely crosses into OVERHEATED deals 5 bonus damage; a spell that crosses two boundaries at once deals 10. This rewards deliberate temperature setup (pushing an enemy just past a threshold rather than spiking from neutral) and makes each tier crossing feel impactful beyond the status effect it confers.

---

### 7.2 Heatstroke

**Rule:** If a unit spends 3 or more consecutive turns at OVERHEATED (temperature ≥ +61), they accumulate an increasing AP penalty on subsequent activations.

**Penalty schedule:**

| Consecutive OVERHEATED Turns | AP Penalty |
|---|---|
| 1 | 0 |
| 2 | 0 |
| 3 | -1 AP |
| 4 | -2 AP |
| 5+ | -3 AP (maximum) |

Formula: `penalty = Max(0, Min(3, ConsecutiveOverheatedTurns - 2))`

**Counter mechanics:**
- `ConsecutiveOverheatedTurns` is incremented once per turn by `TemperatureManager.TickHeatstrokePenalties`, called at the end of `ApplyTerrainTemperatureEffects`.
- The counter resets to 0 **immediately** the moment the unit's temperature drops below +61, regardless of turn timing — this reset fires in `ApplyTemperatureChange` and `ApplyTerrainTemperatureEffects` whenever the unit exits OVERHEATED.
- The AP penalty is applied at the start of each turn in `UnitState.ResetForNewTurn()`, after restoring base ActionPoints but before the unit can spend AP. ActionPoints are never reduced below 0.

**Events:**
- `HeatstrokeTickEvent` is published when the penalty first activates (counter reaches 3) and whenever the penalty value increases (counter passes 4, then 5).
- Contains `UnitId`, `ConsecutiveTurns`, and `APPenalty` for HUD display.

**Design intent:** Heatstroke creates urgency around managing OVERHEATED duration. A unit trapped at OVERHEATED not only suffers the BURNING DoT (5 HP/turn) but progressively loses action economy — at 5 turns OVERHEATED, a Mancer's 6 AP becomes 3 AP, effectively halving their action capacity. This gives the defending player an incentive to spend resources cooling their unit before the AP penalty compounds the pressure, and rewards the attacker for maintaining heat over multiple turns rather than a single burst.

---

### 7.3 Flash Freeze Rupture

**Rule:** If a single temperature application moves a unit from temperature ≥ 0 (NEUTRAL or WARM — not already cold) directly to ≤ -61 (FROZEN SOLID) in one hit, deal 15 bonus rupture damage. This represents the catastrophic structural shock of instant crystallization.

**Trigger conditions (all must be true):**
- Previous temperature ≥ 0 (unit was NEUTRAL or WARM; not already COLD)
- New temperature ≤ -61 (unit is now FROZEN SOLID)
- Both conditions checked after clamping to [-100, +100]

**Rarity note:** To trigger Flash Freeze Rupture from temperature 0, a spell must apply a ΔTemp of -61 or worse in a single hit. With baseline spell values (Ice Heavy = -30 to -35; Thermomancer Calcify = -35; Blizzard Field = -15/tick), no standard spell can reach this threshold from neutral alone. Realistic triggers require:
- Thermomancer with cold upgrade stack (Calcify upgraded to -40+ ΔTemp) applied to a WARM unit
- Crystal Node storing a Glacial Spike then releasing it in combination with a Thermomancer chill
- Coordinated dual-cast: Thermomancer standard (-35) + Cryomancer Quick (-15) targeting the same unit in the same resolution window, landing on a unit at +5 to +10 temperature

Flash Freeze Rupture is intentionally an edge-case reward for extraordinary cold coordination, not a routine combo trigger.

**Implementation details:**
- Checked after Threshold Burst in `ApplyTemperatureChange` — both can trigger on the same hit.
- Damage applied directly to `UnitState.CurrentHP`, floored at 0.
- Published as `UnitDamagedEvent` with `DamageSource = "temperature_flash_freeze_rupture"`.
- Does NOT trigger further temperature changes.

**Design intent:** Flash Freeze Rupture gives the Cryomancer + Thermomancer pairing a dramatic payoff for engineering an extreme cold spike. The 15 bonus damage is on top of Threshold Burst procs (which would also fire for crossing the -30 and -61 boundaries), and on top of the FROZEN SOLID status — a successful Flash Freeze Rupture simultaneously deals ~25 total bonus damage (10 Threshold Burst + 15 Rupture), locks the unit in FROZEN SOLID, and enables SHATTER combos. The rarity of the setup prevents it from being a routine opener.

---

### 7.4 Thermal Composure

**Rule:** Once per match, any unit may spend 3 AP to instantly reset their temperature to 0. Each player has exactly 1 charge per match, shared across all their units. The charge cannot be replenished.

**Mechanics:**
- Activated via `ThermalComposureCommand`, which validates: unit exists and is alive, unit has ≥ 3 AP, and the owning player still has their charge (`SimulationState.HasThermalComposure`).
- Execution: deducts 3 AP from the unit; consumes the player's charge via `SimulationState.ConsumeThermalComposure`; sets `unit.Temperature = 0`; resets `unit.ConsecutiveOverheatedTurns = 0`.
- `ActivationCost = 0`: the 3 AP is charged against the unit's own ActionPoints pool, not the activation budget. This follows the same pattern as SpellCommand.
- Status effects held by temperature thresholds (BURNING from OVERHEATED, FROZEN from FROZEN SOLID, SLOWED from HOT or SUPERCOOLED) are **not immediately removed**. They expire at the next threshold check in `ApplyTemperatureChange` or `TickHeatstrokePenalties`, when the unit's temperature of 0 causes them to be cleaned up. A BURNING DoT that was separately applied by a direct spell (not temperature-held) also remains active — Thermal Composure closes the temperature source, not the current status effect stack.

**Events published:**
- `ThermalComposureUsedEvent` (contains `PlayerId`, `UnitId`, `TemperatureReset` — the temperature value before reset)
- `TemperatureChangedEvent` (previousTemp → 0, for thermometer bar animation)

**Design intent:** Thermal Composure is a tactical safety valve. A player pinned in OVERHEATED for 3+ turns (suffering Heatstroke AP loss and BURNING DoT) can spend this charge to escape the thermal trap — but only once. The cost of 3 AP means using it removes roughly half a Mancer's action economy for that turn. The once-per-match constraint prevents it from becoming a routine cooldown and instead makes it a high-stakes decision: spend the charge now, or hold it for a worse situation later? This mirrors similar limited-use abilities in tactical games (Limit Breaks, Overdrives, tactical retreats) where scarcity creates dramatic decision-making.

**The once-per-match constraint reasoning:** If Thermal Composure were per-turn or per-cooldown, it would nullify the entire Heatstroke system (just use Composure every time you hit OVERHEATED) and trivialize sustained temperature pressure as a strategy. Once-per-match ensures temperature management remains meaningful throughout the game; the Composure charge is insurance, not a counter.

**Design note — warband composition bonus (Planned, not yet implemented):** A warband with zero cold/water Mancers (no Cryomancer, Hydromancer, or Thermomancer) receives 2 Thermal Composure charges instead of 1. This compensates for the reduced self-cooling options in pure-hot or non-temperature warbands, ensuring that cold-focused opponents cannot freely lock down a fire-only warband with zero counterplay available. This bonus is recorded here as a design intent and will be implemented in a future pass when warband composition is tracked in `SimulationState`.

---

## 8. Status Integration

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

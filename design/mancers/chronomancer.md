# Chronomancer — Full Design Document

---

## 1. Tactical Identity

The Chronomancer is the game's tempo controller. Where most Mancers operate on their own AP budget, the Chronomancer operates on everyone's AP budget — amplifying allies, suppressing enemies, and occasionally reaching backward in time to undo a catastrophic outcome. It does not win fights through damage output; it wins by creating moments where one side gets to act twice or the other side doesn't act at all. In a game built around the blind simultaneous turn system, the Chronomancer's power is the ability to fundamentally warp the activation economy.

Playing the Chronomancer well demands a complete understanding of turn sequencing and cooldown management. Its most powerful spell (Rewind) is intentionally limited — it reverts one unit's position and current HP to their state at the start of the previous turn, not the entire board state. Used correctly, it can save a key Mancer from a killing blow or reposition a misplaced ally with no AP expenditure on their end. Used incorrectly, it is a 5 AP investment on a unit that didn't need saving. HASTE, by contrast, is always good — doubling an ally's AP for one turn is a force multiplier that compounds with the ally's own spell power and scales hardest with high-damage-ceiling Mancers like the Pyromancer or Electromancer.

**Primary win condition:** The Chronomancer team wins by creating asymmetric action economy. A HASTE-boosted Electromancer with 12 AP can apply WET with Aqua Lance multiple times AND fire a chain stun in the same activation. A TIME_SLOW applied to an enemy Mancer delays its cooldown recovery — a Pyromancer whose Pillar of Flame cooldown is paused for an extra turn is effectively removed from the threat calculation for that window. The Chronomancer's win condition is making its team play faster and the opponent's team play slower simultaneously.

**Core weakness:** The Chronomancer is entirely support-dependent — it has no meaningful damage output of its own. A solo Chronomancer in a warband without strong Mancer allies is almost useless; its spells amplify power that isn't there. Against aggressive melee pressure, the Chronomancer has no escape tools beyond Stasis (which protects it but removes it from the action) and no way to threaten a pursuing unit. It also struggles against warbands with naturally short cooldown cycles — TIME_SLOW is most valuable against Mancers who just used a Heavy spell and are waiting 3 turns; against a Mancer who primarily uses 0-cooldown Quick spells, TIME_SLOW provides minimal value.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 80 | The lowest HP of any Mancer — fragile by design; the Chronomancer should never be in threat range |
| **Move Range** | 4 tiles per activation | Above-average mobility; needs to reposition frequently to maintain support range on allies |
| **Base Armor** | 1 | No meaningful mitigation; relies entirely on positioning and Stasis |
| **Spell Range** | 6 tiles (base) | Long support range; can Haste or Rewind from far behind the frontline |
| **AP per Activation** | 6 | Standard for all Mancers; movement costs 1 AP per tile |
| **Element** | Time | All base spells interact with time states, cooldown timers, and the activation order |

**AP budget example:** With 6 AP, the Chronomancer can move 3 tiles (3 AP) and apply HASTE to an ally (3 AP), or move 2 tiles, apply TIME_SLOW (2 AP) and Cooldown Steal (2 AP), or hold position and cast Rewind (5 AP) after a devastating hit lands on a key ally.

---

## 3. Base Spell Kit

The Chronomancer's four base spells are designed to cover distinct combat functions:
- **HASTE** — primary buff; doubles an ally's AP for one turn
- **Time Slow** — primary enemy control; freezes an enemy's cooldown timers and reduces their AP next turn
- **Rewind** — emergency single-unit reversal; limited scope by design
- **Stasis** — extreme commitment spell; makes one unit fully invulnerable but also fully inactive

---

### Spell 1: Haste

| Field | Value |
|---|---|
| **Name** | Haste |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Targeted Status — Single target (ally only; cannot self-cast) |
| **Range** | 6 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 0 (buff spell) |
| **Element** | Time |
| **Effects Applied** | Target ally gains `HASTE` status: their next activation grants +6 AP (total 12 AP for that turn). Consistent with spell-system.md: HASTE does not reduce cooldowns; it only grants extra AP. The ally can move further, cast more spells, or apply multiple status effects that would normally require spreading across multiple turns. |
| **Terrain Interaction** | None — Haste is a pure unit buff. |
| **Special Interactions** | If the target ally is also CHILLED (from Cryomancer — halves AP regen from Chronomancer), the HASTE +6 AP is reduced to +3 AP (CHILLED halves the Chronomancer contribution; see status-effects.md). Chronomancer players must check for CHILLED status on allies before casting Haste. HASTE applied to a unit that is SILENCED is wasted — SILENCED units have movement only, so the extra AP results only in extended movement range. |

**Temperature Effects:** **0 temperature change** (temporal manipulation is thermally neutral — accelerating time does not generate or remove heat).

**Design note:** HASTE is the Chronomancer's most universally useful spell and should be cast on the highest-AP-efficiency ally available. The candidates, ranked by HASTE value: (1) Electromancer — can apply WET twice via Aqua Lance and still fire a chain stun in one HASTE turn; (2) Pyromancer — can Scorched Earth + Conflagration Wave + Ember Shot in one turn (normally a 3-turn sequence); (3) Necromancer — can Death Mark, Raise Shambler, AND Necrotic Eruption in one activation if corpses are in range. The 2-turn cooldown means HASTE can be reused every 3 activations on average, keeping it consistently available rather than a once-per-match bomb.

---

### Spell 2: Time Slow

| Field | Value |
|---|---|
| **Name** | Time Slow |
| **AP Cost** | 2 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Targeted Status — Single target (enemy only) |
| **Range** | 6 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 0 (debuff spell) |
| **Element** | Time |
| **Effects Applied** | Target receives `TIME_SLOW` status (2 turns): all of the target's cooldown timers pause for the duration (do not decrement each turn). Additionally, the target loses 2 AP on their next activation (minimum 0 — cannot go below 0 AP). The target can still act, but with diminished AP and frozen cooldowns. |
| **Terrain Interaction** | None — Time Slow is a pure unit debuff. |
| **Special Interactions** | TIME_SLOW interacts with the Chronomancer Interaction rules from spell-system.md: TIME_SLOW on an enemy pauses their cooldown timers. This is most valuable applied to an enemy Mancer immediately after they use a Heavy or Ultimate spell (Pillar of Flame, Flood Zone, etc.) — pausing the cooldown timer means the enemy must wait an additional 2 turns beyond their normal cooldown before reuse. Against a Mancer with 0-cooldown spammable spells, TIME_SLOW's cooldown-pause effect is wasted; only the –2 AP component provides value. TIME_SLOW does NOT apply STUNNED — the target can still act with reduced AP. |

**Temperature Effects:** **0 temperature change** (slowing time does not affect thermodynamic state — the target's temperature neither rises nor falls from the spell itself).

**Design note:** Time Slow is the Chronomancer's AP-efficient harassment tool. At 2 AP, it can be used alongside Haste in the same activation (2 AP + 3 AP + 1 AP movement = 6 AP), simultaneously accelerating an ally and decelerating an enemy in one turn. The cooldown pause component rewards precise timing: cast it the same turn an enemy used their most powerful ability. A Necromancer whose Necrotic Eruption has a 3-turn cooldown hit by TIME_SLOW will effectively wait 5 turns between uses — a significant attrition disadvantage.

---

### Spell 3: Rewind

| Field | Value |
|---|---|
| **Name** | Rewind |
| **AP Cost** | 5 AP |
| **Cooldown** | 4 turns |
| **Targeting Type** | Targeted Status — Single target (ally or enemy) |
| **Range** | 5 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 0 (reversal spell) |
| **Element** | Time |
| **Effects Applied** | **When cast on an ally:** Reverts the target to their position and HP at the start of the previous turn. The ally is placed at their previous tile (LOS not required for teleport component); HP is restored to what it was at the start of that turn. All statuses currently on the ally are removed (Rewind clears statuses, per status-effects.md). The ally's cooldown timers are NOT restored (spell-system.md: REWIND does not restore cooldowns). The ally loses any AP remaining this turn after the Rewind resolves — the reversal is their action. **When cast on an enemy:** Reverts the enemy's position to start of previous turn (repositions them backward) and removes all buffs/statuses from them (including HASTE, BONE_ARMOR, etc.). Their HP is NOT restored — Rewind on enemies is positional displacement + debuff strip, not a heal. |
| **Terrain Interaction** | If the tile the rewound ally or enemy would return to is now occupied or destroyed (OBSIDIAN, PIT, VOID): Rewind fails gracefully — unit stays in current position but all statuses are still removed and HP is still restored (for ally cast). The positional component is blocked; the status/HP component resolves. |
| **Special Interactions** | Rewind on a CHARMED unit: removes CHARMED immediately (the rewind strips all statuses). A Charmed Hydromancer about to use Flood Zone on allied units can be Rewound to stop the action — the Charm is stripped and the Hydromancer returns to its pre-Charm position. This is the Chronomancer's primary defensive response to Psychomancer disruption. Rewind does NOT undo deaths — a dead unit cannot be Rewound back to life. Rewind also cannot be used on summons or companion units; it only targets Mancers and faction infantry units. |

**Temperature Effects:** **0 temperature change from the spell itself.** However, Rewind restores the target unit's temperature to its value at the start of the previous turn. If an ally was OVERHEATED last turn and you Rewind them, they return to their previous position AND their previous (lower) temperature. Rewind can effectively undo temperature damage — it is the Chronomancer's hidden thermal reset tool.

**Design note:** Rewind is deliberately and intentionally limited. A frequently requested interpretation — "Rewind the whole board state" — would be unplayable in a blind simultaneous turn system. The single-unit, position-and-HP scope is the correct design: it is an emergency save tool, not a time travel mechanic. Its 4-turn cooldown and 5 AP cost make it available approximately once per long fight, which is the right frequency for an ability that can negate a kill. The enemy-targeting use (positional displacement + status strip) is a secondary use case that rewards Chronomancer players who already used Rewind on an ally this fight and need a different value from the ability on its next cycle.

**Clarification on scope:** Rewind reverts: position, current HP. Rewind does NOT revert: spent AP this turn, cooldown timers, objects placed on the board (terrain, summons, corpses), other units' positions. It is a localized reversal on one unit only.

---

### Spell 4: Stasis

| Field | Value |
|---|---|
| **Name** | Stasis |
| **AP Cost** | 4 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Targeted Status — Single target (ally or enemy) |
| **Range** | 5 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 0 (state application) |
| **Element** | Time |
| **Effects Applied** | Target receives `STASIS` status (2 turns): cannot act OR be acted upon; fully invulnerable (per status-effects.md — nothing affects a STASIS unit until it expires). STASIS expires only naturally; cannot be removed early by any means. |
| **Terrain Interaction** | A unit in STASIS is immune to all terrain effects — BURNING tiles, NECROTIC_ASH DoT, FLOODED movement penalties, CHARGED arc triggers. The unit is simply frozen in time on that tile. |
| **Special Interactions** | **Strategic ally use:** Cast on a critically low-HP ally Mancer to preserve them for 2 turns while the situation stabilizes. The Chronomancer can then Rewind them on turn 3 (if Rewind is available) to also restore their HP before they return to action. Stasis + Rewind on the same ally across consecutive uses is the strongest defensive sequence in the Chronomancer's kit. **Enemy use:** Cast on an enemy Mancer that is about to use a high-damage ability (predicted from their positioning). The STASIS prevents them from acting for 2 turns — equivalent to 2 STUNNED turns but without the damage output required to apply stun. Enemy use requires predicting the opponent's plan, which the blind-turn system makes non-trivial. **Risk:** A unit in STASIS occupies its tile but cannot contest objectives, can be surrounded (enemies can position adjacent to a STASIS unit and resolve combat the moment it expires), and is not available to the player for 2 full turns. Using Stasis defensively is sometimes the right call; it can also be a waste if the enemy abandons the attack vector. |

**Temperature Effects:** **0 temperature change from the spell itself.** During STASIS, the unit's temperature does NOT change — they are outside normal time flow, so natural temperature decay (10 per turn toward 0) does not apply and no terrain-based temperature accumulation occurs. A FROZEN SOLID unit placed in STASIS can emerge after 2 turns with the same frozen temperature (STASIS suspends the natural thaw process). An OVERHEATED ally Stasised will maintain their OVERHEATED state through the duration — though preserving an ally who is burning is rarely desirable. Note: STASIS is ally-only; an OVERHEATED enemy cannot be Stasised.

**Design note:** Stasis is the most commitment-heavy spell in the Chronomancer's kit. On an ally, it is sometimes the correct "save" — 2 turns of invulnerability buys time for the rest of the warband to recover position. On an enemy, it is a 4 AP gamble that the opponent was going to do something devastating. The 3-turn cooldown allows Stasis to be used approximately every 4 turns in a long fight, giving the Chronomancer roughly 2-3 uses of it over a full engagement.

---

## 3b. Temperature Interaction Notes

**Time manipulation is thermally neutral.** No Chronomancer spell generates or removes heat directly. However, temporal mechanics create significant indirect interactions with the temperature system — particularly through REWIND's temperature restoration, STASIS's thermal suspension, and HASTE's ability to accelerate temperature management.

- **REWIND + temperature:** The Rewind spell reverts a unit's position AND restores their HP to the previous turn's value. It also restores that unit's temperature to the previous turn's value. If an ally was OVERHEATED last turn and Rewind is applied, they return to their previous position AND their previous (lower) temperature — effectively undoing temperature damage. This is the most direct thermal utility in the Chronomancer's kit and makes Rewind particularly valuable when allies are approaching dangerous temperature thresholds from sustained fire or terrain exposure.

- **HASTE + temperature management:** A unit under HASTE (12 AP) can both cast a cooling or heating spell AND move away from temperature-hazard terrain in the same activation. Normally a unit on BURNING terrain accumulates +10 temperature at turn end even if they move, but a HASTED unit can reposition to safe ground AND still use their action — making HASTE doubly valuable for temperature management. HASTE on a Cryomancer lets it apply a cooling spell and reposition the same turn; HASTE on a Pyromancer pushes multiple enemies to high temperature thresholds in one activation.

- **STASIS + temperature:** A unit in STASIS is invulnerable for 2 turns. During STASIS, their temperature does NOT change — the unit is outside normal time flow. Natural temperature decay (10 per turn toward 0) does not occur, and terrain-based temperature accumulation is suspended. Key implications:
  - A FROZEN SOLID unit (≤-61) in STASIS does not naturally thaw during the duration — they emerge at the same frozen temperature. Stasis does not help thaw allies; it merely suspends their state.
  - An OVERHEATED ally can be Stasised as a last resort to buy time, though ideally the warband addresses the heat source rather than suspending a burning unit for 2 turns.
  - An OVERHEATED enemy cannot be Stasised — Stasis is ally-only. This interaction should be noted explicitly: the Chronomancer cannot freeze an OVERHEATED enemy in Stasis to prevent them from continuing to burn.

- **Temporal Acceleration passive (cooldown reduction):** No direct temperature interaction, but Temporal Acceleration allows Pyromancers and Cryomancers to reuse their heavy spells sooner — indirectly accelerating temperature buildup on enemies or providing faster thermal support to allies.

---

## 4. Terrain Interaction Table

### Time Spells and Terrain

The Chronomancer's spells are unit-targeted (buff, debuff, reversal, stasis) rather than terrain-modifying. However, the Chronomancer interacts with terrain indirectly through its repositioning effects (Rewind) and through its allies' amplified casts (HASTE amplifies whatever terrain effects the hastened Mancer generates).

| Existing Terrain State | Chronomancer Interaction | Result |
|---|---|---|
| **ON_FIRE** | Rewind of an ally currently on fire: ally is repositioned to prior tile, which may have been off fire | If the prior tile was safe ground, ally escapes ON_FIRE and BURNING is stripped (status removed by Rewind). Most common Rewind use case. |
| **FLOODED / WET** | HASTE ally on a FLOODED tile: Hydromancer partner benefits — HASTE Hydromancer can cast Flood Zone AND Aqua Lance twice in same turn | Indirect amplification; no direct terrain change from the Chronomancer |
| **CHARGED** | Rewind ally who walked onto CHARGED tile and triggered arc: ally is repositioned back to pre-arc tile; STUNNED status from the arc is stripped by Rewind | Most valuable emergency Rewind use case; undoes a positioning mistake |
| **ICE_TILE / PERMAFROST** | TIME_SLOW enemy Mancer standing on ICE_TILE: freezes their cooldown timers — the enemy benefits less from standing on high ground since they cannot efficiently cycle their spells | Indirect; TIME_SLOW is the Chronomancer's terrain-sensitive interaction only through cooldown pausing |
| **NECROTIC_ASH** | Chronomancer standing in NECROTIC_ASH: takes 3 Necrotic dmg/turn (not immune); should avoid these tiles | Hazard, not interaction |
| **STEAM_CLOUD** | BLINDED Chronomancer: targeting range reduced to 1; Haste and Time Slow cannot be applied beyond 1 tile | Severe debuff on a support Mancer that needs 6-tile range to stay safe |
| **STASIS tile** | A STASIS unit occupying a tile is immune to terrain DoT but cannot use that tile for any action | Terrain DoT fully suspended for STASIS duration |
| **TOXIC_TERRAIN** | No special Chronomancer interaction; Chronomancer takes POISONED stacks as normal | Hazard |
| **ELEVATED tile** | Chronomancer on high ground: +1 range on all spells (Haste reaches 7 tiles, Time Slow reaches 7 tiles) — substantial safety margin | Strong positional benefit for a support unit that wants maximum range with minimal exposure |

### Terrain States Beneficial to the Chronomancer

| State | Benefit |
|---|---|
| `ELEVATED` | +1 range on all spells; reaches allied Mancers in deep positions without moving into threat range |
| `GROUND` (open clear board) | No terrain hazards means the Chronomancer can position for range without managing terrain navigation |

### Terrain States Hazardous to the Chronomancer

| State | Hazard |
|---|---|
| `ON_FIRE` | 5 HP/turn to the Chronomancer's lowest HP pool (80); one BURNING tick is ~6% of max HP |
| `CHARGED` | Arc trigger on a 80-HP Mancer with 1 armor is potentially lethal in combination with any other damage source |
| `STEAM_CLOUD` | Reduces targeting range to 1 — makes Haste, Time Slow, Rewind, and Stasis nearly useless until escaped |
| `TOXIC_TERRAIN` | POISONED stacks accumulate quickly on the Chronomancer's low HP pool; 3-5 stacks (9–15 HP/turn) are threatening |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

#### Variant A: Mass Haste (replaces Haste) — +25 pts

**Description:** Replaces single-target Haste with a broader version: targets a point within 5 tiles; all allied Mancers within 2 tiles of that point receive `HASTE` (+6 AP next activation). AP Cost: 5 AP. Cooldown: 4 turns. Cannot self-apply.

**Trade-off:** Near full-turn investment (5 AP), 4-turn cooldown, and requires allies to be within a 2-tile cluster. Mass Haste is a high-commitment warband build-around — it rewards formations where two or three Mancers position together and trigger simultaneously. With three HASTE-boosted Mancers all activating on the same turn, the simultaneous 12-AP action burst from two or three Mancers can resolve more spells than the opponent's plan anticipated, overwhelming the blind-turn system's prediction assumptions.

**Synergy note:** Mass Haste with a Hydromancer + Electromancer pair clustered behind the frontline: both receive HASTE; Hydromancer Flood Zones and double-Aqua Lances in one turn, Electromancer chains and double-lightning in one turn. The combined board-state impact of two 12-AP activations in the same resolution phase is among the most powerful single-turn events in the game.

#### Variant B: Temporal Trap (replaces Time Slow) — +20 pts

**Description:** Replaces Time Slow with a ground-targeted time distortion zone. Temporal Trap places a 2-tile radius zone on a target point within 5 tiles. The zone persists for 3 turns. Any enemy unit that enters the zone or starts a turn inside it is automatically `TIME_SLOW`ed (cooldown pause + –2 AP on next activation). AP Cost: 3 AP. Cooldown: 3 turns.

**Trade-off:** Trades direct, reliable application on a specific target for a persistent area-based soft control zone. Temporal Trap is strongest when placed on a narrow approach path — it forces enemies to either avoid the route (surrendering board position) or walk through and lose 2 AP. Against mobile enemies with alternate pathing, the trap may be bypassed entirely. Best in map configurations with chokepoints.

---

### Passive Traits

#### Passive A: Temporal Acceleration — +20 pts

**Description:** When the Chronomancer applies HASTE to an ally, that ally's cooldown timers also advance by 1 turn (i.e., their spells on cooldown recover 1 turn faster). This does not stack with the HASTE effect — it is a flat cooldown recovery bonus on HASTE application. A Pyromancer that just used Pillar of Flame (3-turn cooldown) and receives HASTE from the Chronomancer reduces that cooldown to 2 turns while also gaining +6 AP.

**Synergy note:** Temporal Acceleration is the upgrade that most directly enables the Chronomancer's support role. Combined with heavy-cooldown Mancers (Necromancer's Necrotic Eruption at 3 turns, Geomancer's Raise Terrain at 3 turns), it consistently accelerates the team's spell cycle without requiring the opponent to do anything favorable to trigger it.

#### Passive B: Overclock — +25 pts

**Description:** Once per fight (resetting at the start of the match only — this is a once-per-game passive), the Chronomancer can cast any single spell at 0 AP cost. The spell goes on its normal cooldown after use. The Overclock is declared and resolved before the normal AP expenditure phase — it does not cost anything but is consumed immediately.

**Design note:** Overclock is designed as a once-per-game decisive moment tool rather than a repeatable mechanic. Using it to cast Rewind for free after a key ally is killed, or to apply Haste without spending the 3 AP when the Chronomancer needs to move and Haste in the same turn, represents the highest skill expression of the upgrade.

#### Passive C: Time Ward — +20 pts

**Description:** All allied Mancers within 3 tiles of the Chronomancer gain +1 turn reduction on all active cooldown timers at the start of the Chronomancer's activation (passive aura — no AP cost). This is a slow but consistent cooldown acceleration field. In a long fight, Time Ward can reduce a Signature Ability's effective cooldown from 5 turns to 3 if the Chronomancer maintains proximity.

**Trade-off:** Requires the Chronomancer to stay near allies to be useful — counterintuitive for a support unit that typically wants to stay at range. Increases risk of the Chronomancer taking incidental damage from AoE spells targeting the grouped formation.

#### Passive D: Temporal Shield — +15 pts

**Description:** When the Chronomancer would take damage that would reduce it to 0 HP, it automatically enters STASIS for 1 turn instead of dying (once per fight). After STASIS expires, it has 1 HP. This is a panic survival trigger — not a reliable sustain tool. The one-per-fight limit is hard.

**Design note:** Temporal Shield addresses the Chronomancer's core vulnerability: 80 HP with 1 armor means any concentrated burst can eliminate it. The auto-Stasis on lethal hit gives the warband one extra turn to respond (a Hydromancer heal, retreating out of threat range) rather than immediately losing the tempo-control Mancer at a critical moment.

---

### Stat Enhancements

#### Enhancement A: Expanded Temporal Field (+1 Spell Range) — +15 pts

**Description:** All Chronomancer spell ranges increase by 1 tile. Haste: 6 → 7. Time Slow: 6 → 7. Rewind: 5 → 6. Stasis: 5 → 6. This allows the Chronomancer to support from a safer position — staying an additional tile further from the frontline while maintaining full spell coverage.

**Design note:** The Chronomancer's value is entirely dependent on survival. A dead Chronomancer applies no HASTE or Rewind. Every upgrade that extends its safe operating distance is a survivability upgrade in disguise.

#### Enhancement B: Quickened Reflexes (+1 Move Range) — +10 pts

**Description:** Move Range increases from 4 to 5 tiles per activation. The Chronomancer can reposition more aggressively to maintain optimal range on a changing frontline — or retreat from pressure more efficiently. Most valuable in open-field maps with few natural cover positions.

---

### Signature Ability

#### Signature: Temporal Reversal — +40 pts

| Field | Value |
|---|---|
| **Name** | Temporal Reversal |
| **AP Cost** | 6 AP (entire activation) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Self-centered AoE — 4-tile radius around Chronomancer |
| **Range** | N/A (self-centered) |
| **AoE Radius** | 4 tiles |
| **Base Damage** | 0 (reversal ability) |
| **Element** | Time |
| **Effects Applied** | All allied units (Mancers and faction units) within 4 tiles are simultaneously Rewound: each is repositioned to their location at the start of the previous turn and restored to the HP they had at that point. All debuffs and negative statuses on rewound allies are stripped. All buffs are also stripped (HASTE is removed, BONE_ARMOR reset, etc.). Cooldowns are NOT restored (consistent with spell-system.md — Rewind does not restore cooldowns). Enemy units within 4 tiles are NOT rewound; they are unaffected. This ability only targets the Chronomancer's side. |
| **Special Interactions** | Temporal Reversal cannot undo deaths — units that have already been killed before the cast cannot be rewound back to life. Units that died during the turn Temporal Reversal is cast are also ineligible (they were at 0 HP at start of that turn). The 4-tile radius means the Chronomancer must be near its allies when it casts — positioning the Chronomancer in the center of its formation for this ability directly conflicts with its survival need to stay at range. This is a deliberate commitment: to use Temporal Reversal at full value, the Chronomancer takes positional risk. |

**Design note:** Temporal Reversal is the Chronomancer's highest-skill-ceiling ability and the most philosophically resonant expression of its time theme. A team that took severe damage from a predicted attack — say, an enemy Pyromancer World Conflagration or a Necromancer Necrotic Eruption — can be restored to pre-damage HP and position simultaneously, effectively negating the opponent's most powerful turn. The 5-turn cooldown and all-or-nothing AP cost mean it is used once per long engagement at most. Its 40-point price tag reflects that this ability, correctly timed, can reverse a loss into a win position. Incorrectly timed (cast before the critical damage lands, or when allies have already scattered out of the 4-tile radius), it provides no value and wastes a full turn plus a 5-turn cooldown.

---

## 6. Faction Synergy

### Best Pairing: The Verdant Pact

The Verdant Pact pairs best with the Chronomancer because Terrain Bond — the Pact's faction trait — grants bonus movement and passive regen on natural tiles. HASTE amplifies this: a HASTE-boosted Terrain Bond unit with extra movement can cover dramatically more ground in one turn, reaching key contested tiles or flanking positions that would normally take two turns to reach. The Chronomancer's HASTE is most valuable when the target already has strong baseline movement, and Verdant Pact chaff consistently exceeds base movement on natural tiles.

**Thornback Sentinel + HASTE:** Normally a short-range melee unit, a HASTE-boosted Thornback Sentinel with Terrain Bond movement gains enough reach to close and engage ranged units in a single activation. This collapses the typical ranged advantage that the opponent expected from formation separation.

**Rootwarden + Time Slow synergy:** Rootwardens (T2 Chaff) can entrench — spending their action to become immovable and generate a natural tile. A TIME_SLOW on an enemy targeting a Rootwarden reduces that enemy's AP on the next turn, potentially preventing them from spending enough AP to reach and dislodge the entrenched Rootwarden before the Terrain Bond regen activates.

### Gilded Throne — Strong Complementary Pairing

Gilded Throne's Iron Discipline protects Chaff and Ranged units from Psychomancer Panic and Charm. The Chronomancer paired with Throne creates a lineup where the frontline infantry cannot be disrupted by morale-based crowd control AND the Mancer activations are tempo-amplified by HASTE. Iron Vanguard veterans in Shield Wall with a HASTE-boosted allied Mancer creating AoE spells behind them is a powerful combination: the Vanguard tanks while the Mancer spends 12 AP turning the engagement in one activation.

**Crossbow Corps interaction:** Crossbow Corps fire every other turn (alternating attack/reload). The Chronomancer's TIME_SLOW on an enemy Mancer that would threaten the Crossbow Corps on the turn they are reloading protects the most vulnerable reload window. This is a specific, narrow use case but is genuinely valuable in coordinated play.

### Ashen Covenant — Functional but Misaligned

Ashen Covenant benefits most from Necromancer pairing (Remnant tokens). The Chronomancer's tempo tools interact neutrally with Covenant mechanics — HASTE on a Necromancer is excellent (see Combo 1 below), but the Covenant's Deathless Ranks and Remnant system don't have direct Chronomancer interaction. HASTE is always good; the Covenant isn't the optimal recipient.

---

## 7. Combo Chains

### Combo 1: Chronomancer + Necromancer — "The Death Economy Overdrive"

**Mancers involved:** Chronomancer + Necromancer

**Sequence:**
1. Necromancer has accumulated 3+ corpses on the field (mid-game state).
2. Necromancer also has DEATH_MARK applied to two targets and is waiting on Necrotic Eruption cooldown.
3. Chronomancer applies HASTE to the Necromancer (3 AP spend for Chronomancer).
4. Necromancer activates with 12 AP: Death Mark (1 AP), Necrotic Bolt (2 AP), Raise Shambler (3 AP), Necrotic Eruption (5 AP) — exactly 11 AP; 1 AP remaining for movement.
5. In a single turn, the Necromancer: debuffs a priority target, damages another, raises a summon, AND detonates a corpse-scaled AoE burst.

**Why this works:** The Necromancer's most common constraint is AP budget — it wants to Death Mark, raise, and Erupt in sequence but normally this spans 2-3 turns. HASTE collapses that sequence into one activation, producing a board-state change that the opponent planned around spreading out over multiple turns.

**Temporal Acceleration upgrade note:** If the Chronomancer has Temporal Acceleration passive, the HASTE also advances Necromancer's cooldown timers by 1 turn, potentially making a previously-unavailable Raise Shambler (on 1-turn cooldown) immediately usable in the same activation.

---

### Combo 2: Chronomancer + Electromancer — "The Double Arc"

**Mancers involved:** Chronomancer + Electromancer (Hydromancer assists with Wet setup)

**Sequence:**
1. Hydromancer (if present) applies WET to an enemy cluster via Aqua Lance.
2. Chronomancer applies HASTE to Electromancer (3 AP).
3. Electromancer activates with 12 AP: fires Chain Lightning into the WET cluster (chain stun to all adjacent WET units), then still has 6+ AP remaining for movement + a second major spell cast. The Electromancer can stun a cluster AND still fire a second lightning bolt at a separate target in one turn.
4. Result: two separate lightning events in one activation — effectively doubling the Electromancer's turn output.

**Why this works:** The Electromancer's 6-AP base normally permits one major spell with some movement. At 12 AP, it can use its most expensive ability (which might cost 4-5 AP) and still have sufficient AP remaining for a second major ability or full repositioning. HASTE doubles the Electromancer's output ceiling for one turn.

---

### Combo 3: Chronomancer + Pyromancer — "The Arson Surge"

**Mancers involved:** Chronomancer + Pyromancer

**Sequence:**
1. Pyromancer has used Scorched Earth last turn (1-turn cooldown; available again this turn).
2. Chronomancer applies HASTE (3 AP) to Pyromancer; also applies Temporal Acceleration passive — Scorched Earth's cooldown advanced 1 turn.
3. Pyromancer activates with 12 AP: Scorched Earth (3 AP), Conflagration Wave (3 AP), Ember Shot (2 AP), Ember Shot (2 AP) — 10 AP; 2 AP for movement. In one turn: large fire zone created, fanned in a direction, and two BURNING applications on separate targets.
4. This 4-spell activation would normally span 3 full Pyromancer turns.

**Why this works:** The Pyromancer's fire economy is time-dependent — the more ON_FIRE terrain it creates before the opponent can react, the larger the World Conflagration payoff. HASTE compresses the ramp-up phase from 3 turns to 1, establishing fire coverage before the opponent's warband has moved into position to contest it.

---

### Combo 4: Chronomancer + Psychomancer — "The Mind Lock"

**Mancers involved:** Chronomancer + Psychomancer

**Sequence:**
1. Psychomancer applies CONFUSED to a high-value enemy Mancer.
2. Chronomancer applies TIME_SLOW to the same target (2 AP) — the CONFUSED Mancer now has –2 AP AND paused cooldown timers.
3. CONFUSED unit acts with 4 AP (from –2 penalty) with randomized targeting — they may waste AP on misdirected actions.
4. On the following turn, Chronomancer STASISes the CONFUSED enemy (if still active) — the CONFUSED state persists inside STASIS, applying for 1 more turn when they emerge.

**Why this works:** CONFUSED + TIME_SLOW creates a compounding debuff stack: the enemy Mancer has reduced AP and frozen cooldowns, AND their reduced AP is spent randomly. The Stasis extension means the CONFUSED state gets to apply for a second effective turn (1 inside Stasis doesn't consume the duration, Stasis eats a turn while CONFUSED duration is paused). Note: STASIS prevents all effects; the CONFUSED status is preserved inside STASIS but does not trigger during Stasis.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Chronomancer

| Mancer | Counter Mechanism |
|---|---|
| **Psychomancer** | Charmed Chronomancer is the worst-case scenario: the opponent uses HASTE on one of their own Mancers or STASISes the Chronomancer's key ally at a critical moment. CONFUSED Chronomancer applies TIME_SLOW or Haste to random targets — potentially buffing enemies. SILENCED Chronomancer cannot apply any spells; given it has zero physical capability, Silenced Chronomancer is completely useless for the turn. |
| **Aeromancer** | Displacement abilities move the Chronomancer out of its optimal support range — or push it into hazard terrain. Given 80 HP and no escape tools, a displaced Chronomancer may spend 1-2 turns repositioning when it should be applying HASTE. |
| **Cryomancer** | CHILLED halves AP regen from Chronomancer (per status-effects.md). If the Cryomancer CHILLS the Chronomancer, it's a minor but real debuff. More critically, CHILLED or FROZEN applied to an ally that the Chronomancer HASTE-boosted: FROZEN skips the ally's turn entirely, wasting the 3 AP Haste investment. |
| **Sonimancer** | SILENCED Chronomancer cannot cast any spells. On a Mancer with zero combat ability, SILENCED = completely inactive for 1 turn. The Silenced Chronomancer cannot Haste, cannot Rewind, cannot protect itself in any way. |

### Terrain Compositions That Shut Chronomancer Down

| Terrain Setup | Why It's Punishing |
|---|---|
| **STEAM_CLOUD coverage of support position** | BLINDED Chronomancer can only target within 1 tile — Haste, Time Slow, Rewind, and Stasis are all 5-6 tile range abilities rendered useless. The Chronomancer's entire kit is range-dependent. |
| **Chokepoints between Chronomancer and allies** | Chronomancer needs LOS for most spells; if terrain (OBSIDIAN barriers, elevation) separates it from its allies, its support range is functionally blocked even if the tile distance is within spec. |
| **ON_FIRE paths blocking approach to allies** | The Chronomancer's 80 HP cannot safely cross burning terrain (5 HP/turn DoT) to maintain proximity; it may be forced to stay at ineffective distance while allies are out of spell range. |

### Warband Compositions That Prey on Chronomancer

| Warband Type | Exploitation |
|---|---|
| **Fast single-Mancer rush (Faunamancer companion swarm)** | Companion swarms reach and threaten the Chronomancer's rear position before it can reposition. With 80 HP and 1 armor, the Chronomancer cannot survive focused melee. Faunamancer companion units specifically are fast and numerous. |
| **Psychomancer-lead (Charm + SILENCE combo)** | Charming or Silencing the Chronomancer for even one turn removes the tempo tool from the team at the moment it's needed most. The Psychomancer against a Chronomancer + Verdant Pact warband has full value (no Iron Discipline protection for the Mancer itself). |
| **Opponent running 0-cooldown spam Mancers** | If the enemy primarily uses Quick spells with 0-1 turn cooldowns, TIME_SLOW's cooldown-pause component is nearly worthless. The Chronomancer's control tool loses half its value in this matchup. |

---

*End of Chronomancer design document.*

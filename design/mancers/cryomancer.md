# Cryomancer — Full Design Document

---

## 1. Tactical Identity

The Cryomancer is a control and denial specialist whose power derives from slowing the game down — literally. Where aggressive Mancers try to accelerate the board state toward their win condition, the Cryomancer wins by refusing to let the opponent act. Its CHILLED and FROZEN statuses are the hardest incapacitation tools available to a single Mancer, and BRITTLE_ARMOR is the game's most efficient setup for physical burst damage. The Cryomancer is not primarily a damage dealer; it is a tempo thief. Every AP the opponent spends repositioning away from ice zones, thawing frozen allies, or waiting out a FROZEN status is an AP the Cryomancer has effectively stolen from them.

Playing the Cryomancer well demands patience and positioning discipline. Its spells are designed to progressively apply states — CHILLED first, then FROZEN — and the payoff arrives when allied Mancers convert that FROZEN target into massive burst damage through the freeze-shatter combo. A Cryomancer operating alone chips away at enemies and controls terrain but rarely secures kills outright. Paired with a Geomancer, Osteomancer, Sonimancer, or any physical-damage dealer, it becomes a kill-confirmation machine: the Cryomancer applies FROZEN, the ally shatters. The skill floor is understanding that FROZEN is not the win condition — it is the setup. The win condition is the coordinated follow-up.

**Primary win condition:** The Cryomancer wins by locking down the enemy's highest-threat Mancer for exactly one turn, then having an ally convert that FROZEN state into a SHATTER kill or near-kill. Secondary win condition: converting a large FLOODED zone into a full ICE_TILE field using mass-freeze, then following up with physical burst across the board. The Cryomancer team wins in the turn after the freeze resolves.

**Core weakness:** The Cryomancer is heavily countered by fire. Fire spells applied to a FROZEN unit immediately melt the freeze (converting it to WET), negating the SHATTER setup entirely. A Pyromancer on the opposing team can undo the Cryomancer's entire control investment with one Ember Shot. Additionally, the Cryomancer's AP economy is tight: applying CHILLED then FROZEN on a single target takes multiple activations unless upgraded spells are used, and a patient opponent can simply move the CHILLED target out of follow-up range before it freezes fully. BRITTLE_ARMOR is a single-trigger debuff that requires precise coordination to exploit — a missed physical attack consumes it harmlessly. Managing these windows while keeping the Cryomancer out of melee range (it has low armor and no self-sustain) is the consistent execution challenge.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 90 | Below average; the Cryomancer survives on range control, not HP |
| **Move Range** | 3 tiles per activation | Deliberate — ice control is about positioning, not mobility |
| **Base Armor** | 1 | Minimal physical mitigation; keep at range |
| **Spell Range** | 6 tiles (base) | Slightly longer reach than Pyromancer; enables safe freeze application |
| **AP per Activation** | 6 | Standard; movement costs 1 AP per tile |
| **Element** | Ice | All base spells deal Ice damage and apply cold-element terrain/status interactions |

**AP budget example:** With 6 AP, the Cryomancer can move 2 tiles (2 AP) and cast Frost Bolt twice (2 + 2 AP) to apply CHILLED to two separate targets, or move 2 tiles and cast Glacial Spike (4 AP) for heavy terrain freeze.

---

## 3. Base Spell Kit

The Cryomancer's four base spells are designed to cover a progression of cold application:
- **Frost Bolt** — repeatable CHILLED applicator; the combo primer
- **Ice Lance** — single-target FROZEN anchor; the main kill-setup tool
- **Glacial Spike** — terrain freeze and BRITTLE_ARMOR application
- **Blizzard Field** — heavy AP cost, mass terrain freeze and area denial

---

### Spell 1: Frost Bolt

| Field | Value |
|---|---|
| **Name** | Frost Bolt |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (projectile — travels in a line; can hit intervening units) |
| **Range** | 6 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 12 |
| **Element** | Ice |
| **Effects Applied** | Applies `CHILLED` status to hit unit (Movement –2, AP regen from Chronomancer halved; 2-turn duration). Target tile becomes `ICE_TILE` (1-turn duration; persists longer if already cold or FROZEN terrain is adjacent). |
| **Temperature Effects** | **−20 temperature** to the hit unit. A neutral unit (0) drops to −20 COLD — immediately in the ice damage bonus range (+10% to ice spells). Two Frost Bolts in one activation (4 AP): −40 total, deep SUPERCOOLED (−31 threshold crossed), triggering SLOWED + BRITTLE modifier. The CHILLED-to-FROZEN upgrade still resolves on the second Frost Bolt hit regardless of the temperature number, but the temperature shift independently applies SUPERCOOLED penalties before the FROZEN status is checked. |
| **Special Interactions** | See terrain interaction table in Section 4. If the target is already `CHILLED`, the Frost Bolt upgrades the status: the CHILLED unit immediately receives `FROZEN` instead (FROZEN: skip entire turn + SHATTER vulnerability — incoming physical/sonic damage ×2.5). This CHILLED-to-FROZEN upgrade is the Cryomancer's primary single-target control pathway; two consecutive Frost Bolts at a cost of 4 AP lock a unit for a full turn. |

**Design note:** Frost Bolt is the Cryomancer's workhorse and its most AP-efficient tool. At 2 AP with no cooldown, it can be cast three times in a single activation (with no movement). The low damage (12) is intentional — this spell is not meant to kill; it is meant to CHILL. The double-cast FROZEN upgrade (CHILLED target hit again by any Ice spell = FROZEN) enables efficient single-target lockdown without requiring the heavier Ice Lance. The ICE_TILE it creates persists briefly but creates slippery terrain consequences: units moving onto ICE_TILE may slide past their intended destination (1 tile of involuntary continuation in the direction of movement unless they spend 1 additional AP to brake). This can push moving units into hazard tiles, off elevated edges, or into the range of follow-up attacks.

**Slippery tile mechanic:** When a unit attempts to move onto or through an `ICE_TILE`, roll a slip check. On slip: the unit continues 1 additional tile in their movement direction involuntarily. The additional tile cannot be cancelled and may trigger fall damage (off elevated terrain), collision damage (into a wall), or terrain state exposure (into `ON_FIRE`, `TOXIC_TERRAIN`, etc.). The Cryomancer can pre-position ICE_TILEs as invisible traps for predictable enemy movement paths.

**Spell answers YES to (design rule check):**
1. Applies terrain state (ICE_TILE) — YES
2. Applies unit status (CHILLED, upgrades to FROZEN on second hit) — YES
3. Synergizes with Geomancer (physical shatter), Sonimancer (sonic shatter), Osteomancer — YES
4. Skill expression: predict enemy movement paths for ice trap placement; sequence double-CHILL for FROZEN — YES

---

### Spell 2: Ice Lance

| Field | Value |
|---|---|
| **Name** | Ice Lance |
| **AP Cost** | 3 AP |
| **Cooldown** | 1 turn (skip 1 turn before reuse) |
| **Targeting Type** | Single Target (projectile) |
| **Range** | 5 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 22 |
| **Element** | Ice |
| **Effects Applied** | Deals 22 Ice damage. Applies `FROZEN` directly (no CHILLED prerequisite required — Ice Lance freezes in a single cast). Target tile becomes `ICE_TILE` (2-turn duration). If the target was already `CHILLED`, the FROZEN lasts 2 turns instead of 1 (deeper freeze from existing cold). |
| **Temperature Effects** | **−25 temperature** to the hit unit. A neutral unit (0) drops to −25 COLD (ice damage +10% applies). A unit already COLD (−10 from prior Aqua Lance or Frost Bolt) reaches −35 SUPERCOOLED (SLOWED + BRITTLE modifier active). Combined Frost Bolt (−20) into Ice Lance (−25) sequence = −45 total temperature, well into SUPERCOOLED. A unit pre-cooled to −20 COLD by the Hydromancer (Aqua Lance × 2) then hit by Ice Lance reaches −45 SUPERCOOLED with only one Cryomancer spell spent. |
| **Special Interactions** | Against a `WET` unit: Ice Lance flash-freezes the moisture — the unit is `FROZEN` AND the tile becomes `ICE_TILE` (2-turn duration), and any adjacent WET units within 1 tile take 8 cold damage from the cold propagation (the freeze radiates briefly through wet-connected positions). Against a `BURNING` unit: Fire meets ice — BURNING extinguished, `FROZEN` status applied, tile becomes `WET` (thermal exchange produces water residue). Against a `CHARGED` tile: Ice on Charged surface = `FREEZE_CONDUCTOR` — the tile becomes an `ICE_TILE` that retains its electrical charge; the next Lightning spell hitting that tile triggers an enhanced chain arc with +50% chain range. |

**Design note:** Ice Lance is the Cryomancer's definitive single-target freeze tool. The 22 base damage and direct FROZEN application make it the preferred opener against priority targets when a kill or disable needs to happen this turn rather than next. Its 1-turn cooldown means the Cryomancer cannot spam hard freezes consecutively — it must pair Ice Lance with Frost Bolt sequencing to lock two targets in rapid succession. The WET interaction (instant FROZEN on wet units) is particularly important for Cryomancer-Hydromancer teams: Hydromancer applies WET, Cryomancer fires a single Ice Lance for an instant guaranteed freeze with no CHILLED prerequisite — a 2 AP + 3 AP combo that locks a target spending only one Cryomancer activation.

**Spell answers YES to (design rule check):**
1. Applies unit status (FROZEN directly) — YES
2. Applies terrain state (ICE_TILE) — YES
3. Synergizes with Hydromancer (instant freeze on WET units), Pyromancer (BURNING extinguish) — YES
4. Skill expression: prioritize target selection for freeze-shatter setup; identify WET targets for instant freeze — YES

---

### Spell 3: Glacial Spike

| Field | Value |
|---|---|
| **Name** | Glacial Spike |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Ground Target — AoE Radial |
| **Range** | 5 tiles (to center of AoE) |
| **AoE Radius** | 2 tiles |
| **Base Damage** | 14 (to all units in AoE at cast — ice shards erupting from the ground) |
| **Element** | Ice |
| **Effects Applied** | Deals 14 Ice damage to all units in radius. All tiles in the 2-tile radius become `ICE_TILE` (2-turn duration). All units in radius receive `BRITTLE_ARMOR` (next physical hit on this unit deals +50% damage; single-trigger, consumed on first physical hit). Does NOT apply FROZEN or CHILLED directly — this spell coats units in ice shards rather than encasing them. |
| **Temperature Effects** | **−30 temperature** to all units in the 2-tile radius. This is the Cryomancer's heaviest single-cast temperature application. A neutral unit (0) drops to −30 COLD (approaching SUPERCOOLED threshold at −31). A unit already at −10 COLD reaches −40 SUPERCOOLED (SLOWED + BRITTLE modifier). Combined Frost Bolt (−20) + Glacial Spike (−30) in the same or consecutive activation = −50 total on one target — deep SUPERCOOLED, one Frost Bolt away from FROZEN SOLID. Glacial Spike's AoE means this −30 applies to every unit in the radius simultaneously, making it the most efficient group temperature-suppression tool in the Cryomancer's kit. |
| **Special Interactions** | Against a `FLOODED` zone: Glacial Spike hitting a FLOODED tile converts it to `ICE_TILE` permanently (no duration — FLOODED water freezes solid into permanent ice surface). If the FLOODED zone is large (3+ tiles), the freeze propagates outward — each FLOODED tile adjacent to the primary AoE also converts to `ICE_TILE` in a freeze cascade. This Flood-to-Ice conversion is a key Hydromancer + Cryomancer combo trigger. Against `ON_FIRE` terrain in the AoE: fire quenches from cold air — `ON_FIRE` tiles become `WET` (steam quench rather than ice); units on those tiles take cold-steam damage (8 HP) and receive `CHILLED`. Against `MUD` terrain: the mud freezes solid — becomes `PERMAFROST` (permanent frozen mud; movement cost +3, impassable to most units). |

**Design note:** Glacial Spike serves a dual purpose: BRITTLE_ARMOR application to set up physical burst kills, and terrain freeze for area control. The ICE_TILE field it creates simultaneously applies the slippery movement trap across a large area and creates the freeze surface for Flood Zone conversion combos. BRITTLE_ARMOR is a subtle but high-value debuff — any subsequent physical attack from any source (allied infantry, Geomancer stone throw, Osteomancer spike) deals +50% damage on the next hit. The Cryomancer player must communicate to their partner which units carry BRITTLE_ARMOR so they are not wasted on suboptimal attacks.

**Spell answers YES to (design rule check):**
1. Applies terrain state (ICE_TILE field, FLOODED → ICE_TILE, MUD → PERMAFROST) — YES
2. Applies unit status (BRITTLE_ARMOR on all units in AoE) — YES
3. Synergizes with every physical-damage dealer in the roster — YES
4. Skill expression: FLOODED zone targeting for freeze cascade; BRITTLE_ARMOR timing with physical follow-up — YES

---

### Spell 4: Blizzard Field

| Field | Value |
|---|---|
| **Name** | Blizzard Field |
| **AP Cost** | 5 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Ground Target — AoE Radial |
| **Range** | 4 tiles (to center) |
| **AoE Radius** | 3 tiles |
| **Base Damage** | 10 (all units in zone on cast) |
| **Element** | Ice |
| **Effects Applied** | All tiles in the 3-tile radius become `ICE_TILE` (3-turn duration). All units in radius take 10 Ice damage and receive `CHILLED` (2 turns; Movement –2). Units already `CHILLED` when Blizzard Field hits are immediately upgraded to `FROZEN` (1 turn skip + SHATTER vulnerability). The zone also persists as a blizzard overlay: each turn the Blizzard Field zone is active, units that begin their turn inside the zone take 4 cold damage and refresh their `CHILLED` status (preventing it from expiring naturally while inside the field). |
| **Temperature Effects** | **−15 temperature per tick** to all units in the zone. On initial cast, all units in the 3-tile radius take −15 temperature immediately. Each subsequent turn a unit begins its turn inside the blizzard zone, it takes another −15 temperature from the ongoing zone effect (in addition to the 4 cold damage). A unit that spends two full turns inside Blizzard Field takes −30 total temperature from zone ticks alone. Combined with the initial cast: a neutral unit entering the zone immediately (−15) and remaining for one additional turn (−15 more) = −30 COLD, approaching SUPERCOOLED. A unit forced to stay inside for the zone's full 3-turn duration (if they cannot or will not leave) takes −45 total from zone ticks — reaching SUPERCOOLED (−31 threshold) after turn 2 and approaching FROZEN SOLID (−61) by turn 3. Blizzard Field is not just damage and CHILLED status; it is a sustained temperature drain that forces enemies to leave the zone or slowly freeze. |
| **Special Interactions** | Against a `FLOODED` zone: Mass freeze — all `FLOODED` tiles in the AoE become `ICE_TILE` simultaneously, and every unit on those tiles is immediately `FROZEN` (not merely CHILLED — direct FROZEN from FLOODED + Ice is a tier-up). This is the Hydromancer + Cryomancer flagship mass-freeze combo: Hydromancer's Flood Zone followed by Blizzard Field = entire zone hard-locked in one turn. Against `BURNING` units in the zone: BURNING extinguished, unit becomes `CHILLED` (fire-ice trade; they lose their DoT but gain mobility penalty). Against `ON_FIRE` terrain in the zone: fire extinguished — tiles become `WET` (cold kills the blaze), not `ICE_TILE` (the residual heat from fire prevents full freezing). |

**Design note:** Blizzard Field is the Cryomancer's commitment tool — 5 AP spent means almost no movement on that turn, and the 3-turn cooldown prevents casual use. Its value is in mass application: CHILLED across a 3-tile radius, with upgrades to FROZEN for already-CHILLED targets, is potentially locking 3-4 units simultaneously. The persistent zone damage (4 cold/turn, CHILLED refreshed) means enemies cannot simply wait out the field from inside it — they must reposition out or accept compounding control. The Flood Zone + Blizzard Field mass-freeze combo (every unit on FLOODED tiles instantly FROZEN) is the game's definitive two-Mancer hard-lockdown. An enemy team with 3–5 units on a FLOODED zone, all simultaneously FROZEN, loses multiple activations at once — in the blind-turn system, this can be match-deciding.

**Spell answers YES to (design rule check):**
1. Applies terrain state (ICE_TILE field, FLOODED → ICE_TILE mass freeze) — YES
2. Applies unit status (mass CHILLED, FROZEN for pre-CHILLED targets) — YES
3. Creates persistent zone effect (ongoing cold DoT and CHILLED refresh) — YES
4. Synergizes with Hydromancer (mass freeze combo), Geomancer, Sonimancer (SHATTER on FROZEN) — YES
5. Skill expression: sequence CHILLED units before Blizzard Field cast for direct FROZEN; Flood Zone placement for mass freeze — YES

---

## 3b. Temperature Interaction Notes

The Cryomancer is the most extreme COOLING Mancer on the roster — the only Mancer capable of reaching FROZEN SOLID (≤ −61) territory on a single target without ally support, and the fastest at applying sustained cold to groups via Blizzard Field. The temperature system directly maps onto and amplifies the Cryomancer's existing CHILLED/FROZEN status escalation.

**Temperature values summary:**
- Frost Bolt: −20 temperature (spammable, 0 cooldown)
- Ice Lance: −25 temperature (direct FROZEN, 1-turn cooldown)
- Glacial Spike: −30 temperature to all units in 2-tile radius (AoE cooling)
- Blizzard Field: −15 temperature per tick to all units in zone (initial cast + each turn spent inside)

**Solo freeze chain via temperature:**
Frost Bolt (−20) → Ice Lance (−25) = −45 total temperature from neutral, landing squarely in SUPERCOOLED (−31 threshold). The SUPERCOOLED state applies SLOWED (move range −1) and the BRITTLE modifier (+50% incoming physical damage) simultaneously with the CHILLED and FROZEN statuses. One more ice hit (any) pushes the target below −61 FROZEN SOLID. Full solo sequence from neutral to FROZEN SOLID: Frost Bolt (−20) → Ice Lance (−25, now −45 SUPERCOOLED) → second Frost Bolt (−20, now −65 FROZEN SOLID). Total AP cost: 2 + 3 + 2 = 7 AP across two activations. This requires no ally support; the Cryomancer alone can reach FROZEN SOLID territory on a priority target.

**The COLD range bonus — Frost Bolt primer:**
A target at COLD (−1 to −30) takes +10% ice damage from all ice spells. Use Frost Bolt first (−20 temperature, reaches COLD) to prime the target, then Ice Lance deals 22 × 1.10 = ~24 HP (instead of 22) on the enhanced follow-up hit. The damage increase is modest individually but compounds with BRITTLE_ARMOR: a COLD target hit by Glacial Spike (−30, BRITTLE_ARMOR applied) then by a physical attack = physical hit × 1.5 BRITTLE × 1.10 cold bonus if ice-element physical qualifies. Ice Lance into a COLD target also directly applies FROZEN, locking the unit while it is still at elevated vulnerability.

**Freeze-Shatter timing via temperature:**
The freeze-shatter window requires the FROZEN status to survive long enough for an allied physical attacker to trigger it. Temperature context: a unit at FROZEN SOLID (≤ −61) has the FROZEN status AND the temperature penalty both active. Natural decay is 10 temperature per turn — a unit at −65 will be at −55 (still SUPERCOOLED) after one turn, and −45 (still SUPERCOOLED) after two turns. The temperature threshold alone does not remove FROZEN status (that's a separate status mechanic), but being SUPERCOOLED adds the BRITTLE modifier which stacks with SHATTER — a unit that is both FROZEN and SUPERCOOLED-BRITTLE hit by a physical attack takes: base × 2.5 (SHATTER) × 1.5 (BRITTLE) = base × 3.75 effective multiplier. Geomancer Rock Throw (25 base): 25 × 3.75 = 93 HP — effectively a one-shot against any un-upgraded Mancer. Communicate this window to physical-damage allies.

**Blizzard Field as area denial:**
The −15 temperature per turn tick is not just damage support — it is sustained temperature pressure that forces enemies to either leave the zone (costing movement AP and repositioning them away from their objectives) or remain and slowly approach FROZEN SOLID. An enemy cluster that refuses to leave a Blizzard Field zone for 3 turns loses 45 temperature from zone ticks alone — reaching SUPERCOOLED even from a warm starting state (+30 HOT would go to −15 COLD over 3 turns, neutralizing any prior heating). Blizzard Field effectively reverses opponent Pyromancer/Thermomancer heating in the same area.

---

## 4. Terrain Interaction Table

### Ice Spell Impact on Existing Terrain States

The following describes what happens when any Cryomancer spell strikes a tile in the listed terrain state. All Cryomancer spells are Ice element; these interactions apply universally unless a spell's individual entry overrides them.

| Existing Terrain State | What Happens When Ice Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **Normal (GROUND)** | Cold air and ice shards coat the surface | `ICE_TILE` (1–2 turn duration based on spell used) | Takes spell damage + `CHILLED` | ICE_TILE creates slip hazard; units moving through may slide 1 extra tile involuntarily |
| **WET** | Moisture flash-freezes on contact | `ICE_TILE` (2-turn duration; water freezes solid) | Takes spell damage + `FROZEN` immediately (no CHILLED prerequisite — wet units freeze instantly) | Adjacent WET units take 8 cold splash damage from freeze propagation |
| **FLOODED** | Large water mass freezes en masse | `ICE_TILE` (3-turn duration; permanent if adjacent Cryomancer ability reinforces) | Takes spell damage + `FROZEN` directly (FLOODED → immediate FROZEN) | If 3+ connected FLOODED tiles are hit, freeze cascades to all connected FLOODED tiles simultaneously — the Hydromancer + Cryomancer mass-freeze combo |
| **ON_FIRE** | Cold quenches fire; temperature clash | `WET` (residue — cold kills fire, moisture remains; cannot become ICE_TILE directly due to residual heat) | Takes spell damage + BURNING extinguished + `CHILLED` | The fire is eliminated but no ice tile forms; the WET residue can be re-frozen on the following turn |
| **ICE_TILE (already frozen)** | Deeper freeze reinforcement | `ICE_TILE` (duration extended by 2 turns) | Takes spell damage + if unit was `CHILLED`, upgrades to `FROZEN` | No slip check needed — unit is on solid ice already; FROZEN upgrade is the primary outcome |
| **TOXIC_TERRAIN** | Cold preserves poison state | `ICE_TILE` overlaid on TOXIC_TERRAIN (both states active: cold surface with poisoned substrate) | Takes spell damage + `CHILLED` + `POISONED` (1 stack — toxic is preserved under ice) | Units entering the tile later receive both the slip check and a POISONED stack; combined terrain state |
| **CHARGED** | Cold on electrical surface creates conductive ice | `ICE_TILE` retaining `CHARGED` state (`FREEZE_CONDUCTOR`) | Takes spell damage + if hit by Ice Lance: unit is `FROZEN` AND the tile retains charge | Next Lightning spell hitting the FREEZE_CONDUCTOR tile triggers enhanced chain arc (+50% chain range, per Ice Lance entry) |
| **MUD** | Cold freezes mud solid | `PERMAFROST` (permanent frozen mud; movement cost +3; impassable terrain) | Takes spell damage + `CHILLED` | PERMAFROST does not expire naturally — requires Fire spell (3+ hits) to thaw; creates durable movement barriers |
| **OBSIDIAN** | Cold cannot affect obsidian | `OBSIDIAN` (unchanged) | Takes spell damage; no terrain state change | Obsidian is thermally inert; Cryomancer cannot freeze or interact with Geomancer-hardened tiles |
| **OVERGROWTH** | Plants freeze and become brittle | `ICE_TILE` overlaid on OVERGROWTH (growth frozen in place) | Takes spell damage + `CHILLED` | Frozen overgrowth is fragile: any physical damage to the tile shatters the frozen growth, dealing 10 AoE cold-shard damage to all units within 1 tile |
| **STEAM_CLOUD** | Cold condenses steam into freezing mist | `ICE_TILE` (steam crystallizes; 1-turn duration) | Takes spell damage; BLINDED status from STEAM_CLOUD is replaced by `CHILLED` | The blinding mist solidifies into an icy fog — no longer blinding, now cold; a meaningful state transition |

### Freeze-Shatter Combo Documentation

The **freeze-shatter combo** is the Cryomancer's core kill-confirmation mechanic, second in importance only to the Shock Network (Hydromancer + Electromancer) as a signature two-Mancer combo.

**Mechanic:** A unit with `FROZEN` status receives ×2.5 damage multiplier on any incoming physical or sonic damage. This damage is labeled `SHATTER` and destroys the `FROZEN` status on resolution (one-time multiplier — cannot stack FROZEN for multiple shatter applications).

**Execution:**
1. Cryomancer applies `FROZEN` to a target (via double Frost Bolt, Ice Lance, or Blizzard Field on a CHILLED target).
2. Any source of physical or sonic damage hits the FROZEN unit on the same or subsequent turn.
3. The damage is multiplied ×2.5. FROZEN is removed.

**Damage math example:** A FROZEN unit (90 HP, 1 armor) hit by a Geomancer Rock Throw (base 25 damage) takes: 25 × 2.5 = 62.5 HP (rounded to 62). At 90 HP, one shatter from a single Geomancer spell leaves the target at 28 HP — near-lethal in a single hit. A Sonimancer Resonance Burst (base 30 sonic) shatters for 75 HP. A FROZEN un-upgraded Pyromancer (85 HP) is eliminated outright by a single shatter hit from most physical or sonic spells.

**Slippery Terrain Documentation:**

`ICE_TILE` created by all Cryomancer spells creates the following movement hazard: any unit (friendly or enemy) that moves onto or through an `ICE_TILE` must make a slip check. On slip, the unit continues 1 additional tile in their current movement direction involuntarily. The Cryomancer player should pre-position `ICE_TILE` on tiles adjacent to:
- Elevated drop-offs (involuntary slide = fall damage)
- `ON_FIRE` tiles (involuntary slide into burning ground)
- `TOXIC_TERRAIN` tiles (involuntary slide into poison ground)
- Wall collisions (slide into wall = collision damage equal to remaining forced-movement tiles × 4 HP)
- Another unit (slide into unit = collision; both units take 6 HP)

The Cryomancer's `ICE_TILE` pattern is a predictive tool: if the Cryomancer knows where enemies will move next turn, pre-placed ice on the approach path punishes that movement without requiring further Cryomancer AP on the follow-up turn.

### Terrain States Beneficial to the Cryomancer

| State | Benefit |
|---|---|
| `ICE_TILE` tiles adjacent to Cryomancer | Cryomancer ignores ICE_TILE slip checks — it moves on ice without any movement penalty or slip risk (cold immunity to its own terrain) |
| `WET` tiles | Instant-FROZEN when hit by Ice Lance — highly efficient single-target lockdown with no CHILLED prerequisite |
| `FLOODED` zones | Mass-freeze potential with Blizzard Field; the highest-value terrain state the Cryomancer can interact with |
| `PERMAFROST` tiles (created by Glacial Spike on MUD) | Permanently restricted movement — the Cryomancer converts Geomancer MUD zones into lasting obstacles |

### Terrain States Hazardous to the Cryomancer

| State | Hazard |
|---|---|
| `ON_FIRE` tiles | The Cryomancer is not immune to fire terrain DoT (5 HP/turn from BURNING terrain); its 90 HP makes fire terrain more punishing relative to its HP pool than it would be for higher-HP Mancers |
| `CHARGED` tiles | No defensive advantage; electrostatic discharge damages the Cryomancer normally |
| `TOXIC_TERRAIN` | POISONED stacks apply normally; the Cryomancer has no poison immunity or resistance |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

Spell variants replace a base spell entirely with a more powerful or specialized version.

#### Variant A: Permafrost Bolt (replaces Frost Bolt) — +20 pts

**Description:** Frost Bolt is replaced with a slower-moving but heavier bolt of permanent cold. Permafrost Bolt deals 16 damage (up from 12) and applies `FROZEN` directly on first hit — no CHILLED prerequisite. The tile becomes `PERMAFROST` instead of `ICE_TILE`: PERMAFROST does not expire naturally (unlike ICE_TILE) and imposes movement cost +3. AP cost increases to 3 AP; cooldown is 1 turn.

**Trade-off:** Significantly more control per cast (direct FROZEN + permanent terrain), but loses the 0-cooldown spam of Frost Bolt and cannot double-cast in one activation cheaply. Best for Cryomancers built around single-target lockdown and permanent terrain denial rather than rapid CHILLED application across multiple targets.

#### Variant B: Glacial Prison (replaces Ice Lance) — +25 pts

**Description:** Ice Lance is replaced by Glacial Prison, which deals the same 22 damage and applies FROZEN, but additionally creates a 1-tile ICE_TILE cage around the FROZEN unit — all 8 adjacent tiles become `ICE_TILE` (1-turn duration). The cage effect traps the frozen unit's allies: any unit adjacent to the frozen target who attempts to move through the ICE_TILE cage makes a slip check, and the ICE_TILE cage costs +1 AP to traverse. AP cost is 4 AP; cooldown is 2 turns.

**Trade-off:** Harder lockdown (the frozen unit is isolated from allied support) at a higher AP cost. Best used against a high-value Mancer to isolate it from its supporting units during the shatter window.

#### Variant C: Absolute Zero (replaces Blizzard Field) — +25 pts

**Description:** Blizzard Field is replaced by Absolute Zero — a smaller (2-tile radius), more concentrated freeze that applies `FROZEN` directly to all units in the AoE without requiring them to be pre-CHILLED. All tiles in the 2-tile radius become `ICE_TILE` (4-turn duration). Base damage is 20 to all units in the zone. Units FROZEN by Absolute Zero receive BRITTLE_ARMOR simultaneously (unlike base Blizzard Field, which requires Glacial Spike for BRITTLE_ARMOR). AP cost is 5 AP; cooldown is 3 turns.

**Trade-off:** More reliable mass control (direct FROZEN, no CHILLED prerequisite) at the cost of a smaller AoE footprint. Best against dense enemy formations where the direct-FROZEN guarantee is worth more than the wider CHILLED coverage of the base version.

---

### Passive Traits

#### Passive A: Deep Freeze — +20 pts

**Description:** When the Cryomancer applies FROZEN to any unit, that unit's FROZEN duration is extended by 1 turn (FROZEN lasts 2 turns instead of 1). This does not change the SHATTER vulnerability — physical/sonic attacks still shatter for ×2.5 and remove FROZEN normally. However, if a shatter opportunity is missed on the first FROZEN turn (enemy has no physical attacker in range), the FROZEN remains for a second turn rather than expiring. Effectively: Deep Freeze acts as a one-turn insurance window on every freeze application.

**Trade-off:** Purely a timing safety net — it does not increase FROZEN damage or apply new statuses. Best for Cryomancers whose allied shatter dealers are slower (high-cooldown spells, low movement) and need the extra turn to reach position.

**Synergy note:** Deep Freeze with Absolute Zero (Signature) means FROZEN units stay frozen for 2 turns — a substantial control anchor in long engagements.

#### Passive B: Cold Conduct — +25 pts

**Description:** All ICE_TILE terrain created by the Cryomancer is conductive for Lightning spells. Any Lightning spell hitting a unit on a Cryomancer-created ICE_TILE chains to all adjacent units also on ICE_TILE as if those tiles were WET. This is not identical to the WET chain (which requires units to be WET) — it requires units to be standing on ICE_TILE terrain. Combined with the Shock Network combo, Cold Conduct enables: Cryomancer creates an ICE_TILE field → Electromancer hits one unit on ICE_TILE → chain arc propagates through all adjacent ICE_TILE-standing units, potentially for a multi-unit stun across the ice field.

**Trade-off:** Requires both Cryomancer and Electromancer in the warband to realize full value. Without an Electromancer partner, Cold Conduct is passive terrain enhancement only (no active chain trigger). Adds a significant offensive dimension to the Cryomancer's terrain setup.

#### Passive C: Glacial Ward — +15 pts

**Description:** The Cryomancer is immune to the CHILLED and FROZEN statuses (its own element cannot affect it). Additionally, when the Cryomancer takes Fire damage, it loses 20% less HP than the base calculation (cold body partially resists thermal shock). Does not grant Fire immunity — merely reduces Fire damage received.

**Trade-off:** Pure defensive investment. Does not enhance the Cryomancer's offensive output. Best for maps with heavy Pyromancer counter-threat, or in blind-turn environments where the Cryomancer expects to be the primary focus-fire target.

#### Passive D: Shattering Touch — +20 pts

**Description:** When the Cryomancer itself deals physical damage (e.g., in a tile-adjacent situation, or via a close-range Ice Lance that qualifies as a physical contact), it triggers SHATTER on FROZEN targets as if it were a physical attacker. More practically: the Cryomancer's own Ice spells hitting a FROZEN unit apply ×2.0 damage multiplier (not the full ×2.5 of a physical attacker, but meaningful self-contained burst). This allows the Cryomancer to be its own shatter trigger in single-Mancer lists — less efficient than a dedicated physical attacker but removes the reliance on a combo partner.

**Trade-off:** Reduces dependency on a second Mancer for kill confirmation. Most valuable in lists with only one Mancer where the Cryomancer must both freeze and shatter.

---

### Stat Enhancements

#### Stat A: Glacial Constitution (+15 HP) — +10 pts

**Description:** Max HP increases from 90 to 105. Brings the Cryomancer closer to the median Mancer HP range and allows it to absorb one additional burst before reaching critical HP. No other stat changes.

**Design note:** 90 HP is the Cryomancer's primary vulnerability. At 90 HP with 1 armor, a SHATTER combo (from an enemy Cryomancer freezing the Cryomancer and a physical attacker shattering) can eliminate it in a single paired activation. 105 HP survives most single-activation burst thresholds and buys an additional turn.

#### Stat B: Arctic Reach (+1 Spell Range) — +10 pts

**Description:** All Cryomancer spell ranges increase by 1 tile. Frost Bolt: 6 → 7. Ice Lance: 5 → 6. Glacial Spike center: 5 → 6. Blizzard Field center: 4 → 5. Combined with Elevated tile bonus, a Cryomancer on high ground reaches 8 tiles with Frost Bolt — enough to CHILL enemies before they can close to threat range.

**Design note:** The Cryomancer's primary defensive mechanism is applying CHILLED (–2 Move) before enemies can close to melee range. Arctic Reach extends the safe application window, giving the Cryomancer an extra tile of range buffer before it needs to reposition.

---

### Signature Ability

The Signature Ability is unlocked as an additional spell slot — it does not replace any base spell.

#### Signature: Glacier's Wrath — +40 pts

| Field | Value |
|---|---|
| **Name** | Glacier's Wrath |
| **AP Cost** | 6 AP (entire activation; Cryomancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Self-centered AoE — no targeting cursor; Cryomancer is the origin |
| **Range** | N/A (self-centered) |
| **AoE Radius** | 5 tiles in all directions from the Cryomancer's current position |
| **Base Damage** | 18 (all units in AoE on cast) |
| **Element** | Ice |
| **Effects Applied** | All units in the 5-tile radius receive `FROZEN` directly (no CHILLED prerequisite; the sheer cold magnitude skips the progression). All tiles in the 5-tile radius become `ICE_TILE` (4-turn duration). All `FLOODED` tiles in the radius become `ICE_TILE` immediately (flash-freeze of the entire water mass). The Cryomancer itself is immune to the FROZEN effect of this cast. |
| **Special Interactions** | Against units already `CHILLED` in the zone: FROZEN lasts 2 turns for pre-CHILLED units (Deep Freeze mechanic baseline, built into Glacier's Wrath regardless of upgrade purchase). Against `BURNING` units in the zone: BURNING extinguished; unit is FROZEN. Against `ON_FIRE` terrain in the zone: fire extinguished — `WET` tiles formed (residual heat prevents full ice tile formation on fire-active tiles). If Deep Freeze passive is also purchased, all FROZEN applied by Glacier's Wrath lasts 3 turns. |

**Design note:** Glacier's Wrath is the Cryomancer's "the board is ours now" ability. Mass FROZEN across a 5-tile radius is potentially 6–8 units simultaneously unable to act on their next turn. In the blind-turn system, where activating a unit that cannot act is a wasted activation, Glacier's Wrath can effectively neutralize the opponent's entire planning phase for one full turn if their units are caught in the radius. The 5-turn cooldown, full-AP cost, and no-movement-on-cast constraints mean it is a declared commitment — but its impact justifies the investment when the board position supports it. Best executed in the same turn Hydromancer casts Flood Zone: Flood Zone saturates the area with FLOODED terrain, then Glacier's Wrath mass-FREEZES every unit and every tile in range simultaneously.

**Synergy note:** Glacier's Wrath is the ultimate setup for a Sonimancer or Geomancer activation that follows immediately. Every unit frozen by Glacier's Wrath is a SHATTER-eligible target: a Sonimancer's Resonance Burst hitting the densest cluster on the following turn deals ×2.5 to every FROZEN unit in the blast. A Geomancer's Rock Throw on a frozen high-value target confirms a kill at nearly any HP level.

---

## 6. Faction Synergy

### Best Faction: The Gilded Throne

The Gilded Throne is the Cryomancer's most functional faction match. The combination is structural: Conscript Spearmen and Iron Vanguard are physical-damage melee units with reliable attack values. A FROZEN enemy hit by a Spearman or Iron Vanguard attack triggers SHATTER (×2.5 physical). The Cryomancer provides freeze; the Throne's entire melee roster provides the physical hits that convert freeze into kills.

Iron Vanguard in Shield Wall formation (adjacent Vanguard granting damage reduction aura) can hold a front line while the Cryomancer CHILL-freezes enemies beyond the wall. The Vanguard advances on frozen enemies; enemies frozen while attempting to close melee are killed on the Vanguard's first contact. The flow is: Vanguard screens → enemy approaches and becomes CHILLED (movement halved) → Cryomancer freezes → Vanguard shatters on advance.

The Throne's Iron Discipline (immunity to Panic and Charm) also protects the Cryomancer's physical-damage support from Psychomancer disruption — in a list where the freeze-shatter sequence requires precise timing between Cryomancer and infantry, having Charm-immune infantry ensures the shatter step cannot be stolen.

**Specific upgrade combinations that peak with Gilded Throne:**
- **Deep Freeze passive + Iron Vanguard:** FROZEN lasts 2 turns, giving slow-moving Vanguard time to reach frozen targets that are out of immediate range.
- **Glacial Spike + Siege Arbalest (T2 Ranged):** BRITTLE_ARMOR applied by Glacial Spike works for ranged physical attacks. Siege Arbalest fires armor-piercing bolts every turn — BRITTLE_ARMOR on a target hit by Arbalest deals +50% ranged physical damage. A long-range physical burst option the Cryomancer enables purely through its terrain kit.

### The Verdant Pact — Terrain Bond Interaction

Verdant Pact's Terrain Bond provides movement bonuses and regen on natural tiles. ICE_TILE terrain created by the Cryomancer is **not a natural tile** — it is a created terrain state. Thornback Sentinels and Rootwardens standing on Cryomancer ice do not receive Terrain Bond movement or regen bonuses.

However, the Cryomancer's FROZEN on FLOODED tiles converts those to ICE_TILE — and FLOODED terrain is also not natural. No interaction between the Cryomancer's standard kit and Terrain Bond is active.

**What does interact:** The Cryomancer's PERMAFROST (created from MUD via Glacial Spike) is derived from MUD, which is earth-origin. However, once MUD becomes PERMAFROST, it has been transformed — PERMAFROST is not classified as natural terrain for Terrain Bond purposes. The Cryomancer's ice transformations remove natural tile classification regardless of origin tile.

**Functional pairing:** The Verdant Pact's Glade Archers apply POISONED on hit. A FROZEN enemy with POISONED stacks has preserved POISONED (POISONED does not decay while frozen — status interactions table). When the FROZEN expires, the POISONED stacks resume ticking simultaneously. The Cryomancer + Glade Archer combination creates enemies with suspended DoT timers that restart at full stack count when the freeze breaks — a meaningful attrition combo that requires no special coordination, just temporal sequencing.

### The Ashen Covenant — Deathless Ranks and Ice Zones

Grave Husks regenerate 1 HP/turn in POISONED, CORRUPTED, or BURNING terrain. Cryomancer ice terrain (ICE_TILE, PERMAFROST) is none of these — Grave Husks do not regen on ice terrain.

The Ashen Covenant does benefit from a different interaction: Wailing Shades are phase-through ranged units that ignore physical cover. If the Cryomancer creates an ICE_TILE field that obscures LOS through its visual overlay, Wailing Shades still target through it without penalty. More importantly: Wailing Shades' Silence aura (enemy on-death effects silenced within 1 tile) prevents enemies from triggering on-death effects when adjacent to them. A FROZEN unit that would trigger an on-death explosion (DEATH_MARK from Necromancer) or on-death buff when killed by a shatter hit can have that death effect suppressed by adjacent Wailing Shades — giving the Ashen Covenant a niche way to exploit frozen kills without triggering retaliation.

**Specific note:** Abyssal Revenants (T2 Chaff) move at normal speed (no movement penalty). `ICE_TILE` slip checks apply regardless of unit type — Revenants are not immune to slip. However, since the Cryomancer creates ICE_TILE with predictable geometry, the Cryomancer player can position ice zones to create slip hazards only on enemy movement paths, keeping ally Revenants on non-ice tiles.

---

## 7. Combo Chains

### Combo 1 — The Ice Lock (Cryomancer + Hydromancer) [FLAGSHIP MASS-FREEZE]

**Mancers involved:** Cryomancer + Hydromancer

**Step-by-step execution:**

1. **Turn N, Hydromancer activates:** Hydromancer casts Flood Zone (5 AP) over the enemy cluster. All tiles in the 3-tile radius become `FLOODED`; all units in the zone receive `WET` (3 turns). Hydromancer has spent most of its activation — 1 AP remains for 1 tile of movement.
2. **Turn N, Cryomancer activates (same turn, Mancer initiative):** Cryomancer casts Blizzard Field (5 AP) centered on the FLOODED zone.
3. **Resolution:** All FLOODED tiles in the Blizzard Field AoE convert to `ICE_TILE` simultaneously (FLOODED + Ice spell = mass freeze). Every unit on those tiles is immediately `FROZEN` (FLOODED → FROZEN on Ice spell impact, as documented in the terrain table). The blizzard zone overlay persists — units attempting to enter the zone on subsequent turns are CHILLED.
4. **Turn N+1:** Any allied physical or sonic Mancer activates and targets FROZEN units for SHATTER (×2.5 damage).

**Tactical outcome:** An entire enemy cluster simultaneously unable to act, with guaranteed next-turn kill windows on each frozen unit. In the blind-turn system, the opponent cannot plan activations for FROZEN units — those activations are forfeit. Three or more simultaneously FROZEN Mancers effectively ends the match by removing the opponent's ability to respond while taking SHATTER hits from every physical attacker in the Cryomancer team.

**Counter-play:** The Ice Lock requires Hydromancer to commit a full activation to Flood Zone (visible, telegraphed) and Cryomancer to position within range of the zone and spend 5 AP. An opponent who keeps units spread — not clustering on contiguous tiles — limits the zone's coverage. Retreat off the flood zone before the Cryomancer activates is the primary counter, but the blind-turn system makes this reactive response unreliable.

---

### Combo 2 — Freeze-Shatter (Cryomancer + Geomancer / Sonimancer / Osteomancer)

**Mancers involved:** Cryomancer + any physical or sonic damage dealer

**Step-by-step execution:**

1. **Cryomancer activates:** Frost Bolt hits target (CHILLED applied). Second Frost Bolt hits same target (CHILLED → FROZEN: the upgrade triggers immediately on second Ice spell to a CHILLED unit). 4 AP spent total; 2 AP remain for movement.
   *Alternative:* Ice Lance at 3 AP for direct FROZEN in a single cast.
2. **Physical or sonic Mancer activates (same or next turn):** Any physical or sonic damage spell targets the FROZEN unit.
3. **SHATTER resolution:** Incoming physical/sonic damage is multiplied ×2.5 (SHATTER). FROZEN status is consumed.

**Damage math for common SHATTER combos:**
- Cryomancer + **Geomancer Rock Throw** (base 25): 25 × 2.5 = 62 HP
- Cryomancer + **Sonimancer Resonance Burst** (base 30): 30 × 2.5 = 75 HP
- Cryomancer + **Osteomancer Bone Spike** (base 20): 20 × 2.5 = 50 HP
- Cryomancer + **Iron Vanguard melee attack** (~18 base): 18 × 2.5 = 45 HP
- Cryomancer + **Siege Arbalest bolt** (~28 base armor-piercing): 28 × 2.5 = 70 HP

Against an 85–100 HP Mancer (typical base range), a single SHATTER from Geomancer, Sonimancer, or Arbalest is either lethal or leaves the target at critically low HP that a follow-up Frost Bolt or Aqua Lance confirms the kill.

**Counter-play:** A Fire spell on the FROZEN target before the shatter breaks the freeze (BURNING extinguishes FROZEN, converting it to WET). An opponent who activates a Pyromancer before the physical attacker can denature the combo. The blind-turn system makes this reactive counter possible but not guaranteed — a skilled Cryomancer player will time the freeze for a turn when the opponent's Pyromancer is on cooldown or out of AP.

---

### Combo 3 — Brittle Barrage (Cryomancer + Multiple Physical Units)

**Mancers involved:** Cryomancer + Gilded Throne ranged or melee infantry

**Setup:** Cryomancer casts Glacial Spike (3 AP) over a group of 3–5 enemy units. All units in the 2-tile radius receive `BRITTLE_ARMOR` (next physical hit deals +50%).
**Execution:** Multiple physical units (Crossbow Corps, Iron Vanguard, Siege Arbalest) all target BRITTLE_ARMOR units in the same turn.
**Result:** Each physical hit consumes its target's BRITTLE_ARMOR for a +50% damage bonus.

**Tactical outcome:** A Glacial Spike followed by a Crossbow Corps volley (Siege Arbalests firing on their off-turns) effectively provides a +50% damage multiplier across an entire ranged salvo for no additional AP cost on the ranged units' side. At 3 AP for Glacial Spike, this is the Cryomancer's most AP-efficient damage amplification play.

---

### Combo 4 — The Slippery Trap (Cryomancer + Aeromancer / Geomancer)

**Mancers involved:** Cryomancer + Aeromancer or Geomancer (or any unit with displacement abilities)

**Setup:** Cryomancer creates an ICE_TILE field adjacent to a drop-off (elevated terrain edge), a pit, an ON_FIRE zone, or a wall. The ICE_TILE field is positioned along a predictable enemy movement path.
**Execution:** An enemy unit moves toward the Cryomancer's position and enters the ICE_TILE field. Slip check triggers — the unit slides 1 additional tile past its target in movement direction. The slide lands on the hazard (drop = fall damage; pit = trapped; fire = BURNING; wall = collision).
**Amplification:** Aeromancer pushes a unit onto the ICE_TILE field (2-tile gust push). The push movement enters the ICE_TILE, triggering a slip check during the forced displacement. The pushed unit now slides a total of 3 tiles from their original position — predictable, devastating, and entirely passive once the ice is placed.

**Tactical note:** This is the highest skill-expression combo in the Cryomancer's kit — it requires reading enemy movement paths 2–3 turns ahead, positioning ice accurately, and having a partner displacement tool to guarantee the push lands on ice. When it works, it deals large positional damage (fall, collision, terrain DoT) at zero additional AP cost from the Cryomancer.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Cryomancer

| Mancer | Counter Mechanism |
|---|---|
| **Pyromancer** | Ember Shot on any FROZEN unit immediately melts the freeze (ICE + FIRE = WET; FROZEN removed; SHATTER window negated). A Pyromancer on the opposing team invalidates the Cryomancer's entire kill-confirmation strategy — every freeze the Cryomancer applies is one Ember Shot away from dissolution. The Pyromancer also converts ICE_TILE terrain to WET (residue), eliminating the slip hazard field. Hard counter. |
| **Thermomancer** | OVERHEATED status (4 HP/turn + fire dmg +50%) applied to a FROZEN unit cancels FROZEN and applies BURNING instead (thermal forcing). A Thermomancer can undo freezes while simultaneously applying its own DoT, and the heat gradient abilities counter cold zones by raising tile temperature above the Cryomancer's ice threshold. |
| **Aeromancer** | UPDRAFT zones grant WEIGHTLESS to allies inside — WEIGHTLESS units are immune to ground terrain effects including ICE_TILE slip checks and CHILLED from standing in Blizzard Field zones. The Cryomancer's terrain control is ground-dependent; Aeromancer lifts the opponent's team out of that dependency. |

### Warband Compositions That Prey on Cryomancer

| Warband Type | Exploitation |
|---|---|
| **Pyromancer + high-burst physical screen** | Pyromancer cancels every freeze; physical screen cannot be SHATTERED because freezes never hold. Cryomancer is left as a low-damage unit with minimal burst output and no combo access. |
| **Aeromancer + Gravimancer (WEIGHTLESS warband)** | Entire force immune to ICE_TILE. The Cryomancer's terrain control is nullified; it becomes a weak single-target FROZEN applicator with no terrain value. |
| **Hydromancer mirror team (enemy)** | Enemy Hydromancer can WET-prime their own units intentionally — but WET units are Cryomancer INSTANT-FROZEN targets, making this a double-edged choice. The threat here is that the enemy Hydromancer floods the Cryomancer's own position, applies WET to the Cryomancer, and an enemy Electromancer chains through it. At 90 HP, the Cryomancer caught in a Shock Network chain stun is in serious danger. |

---

*End of Cryomancer design document.*

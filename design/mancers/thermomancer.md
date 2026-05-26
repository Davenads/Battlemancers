# Thermomancer — Full Design Document

---

## 1. Tactical Identity

The Thermomancer is the 19th and final Mancer — the capstone of the roster and the bridge between the game's two most foundational temperature extremes. Where the Pyromancer burns and the Cryomancer freezes, the Thermomancer controls the spectrum between them: applying heat and cold in deliberate sequences to force THERMAL SHOCK, maintaining persistent temperature gradient zones that punish any movement crossing the thermal midline, and creating the game's most mechanically complex status escalation path (SUPERCOOL → OVERHEAT in the same turn = THERMAL SHOCK burst + STUN). The Thermomancer is not a better Pyromancer or a better Cryomancer; it is something neither of them can be — a single Mancer that can apply both temperature extremes and then detonate both simultaneously.

Playing the Thermomancer at its ceiling means understanding temperature state transitions deeply. OVERHEAT and SUPERCOOL are not merely damage-over-time effects; they are setups for the most dangerous single-turn burst in the game. A unit that passes through the midline of a THERMAL GRADIENT ZONE from the cold half into the hot half takes THERMAL SHOCK on arrival — equal to double the combined sum of both active temperature effects. Managing the Thermal Gradient's orientation so that enemy movement paths cross the midline is the Thermomancer's equivalent of the Pyromancer's fire placement: patient terrain investment that pays off when the opponent's movement exposes them to the worst-case interaction. Its role in team synergy is equally significant — it amplifies Pyromancer output (OVERHEAT first makes DoT tick harder) and amplifies Cryomancer output (SUPERCOOL deepens the freeze faster). The Thermomancer does not fight alongside one elemental Mancer; it bridges every element of the thermal spectrum and makes the whole team's temperature abilities more dangerous.

**Primary win condition:** The Thermomancer wins by engineering a THERMAL SHOCK moment: a unit transitions from SUPERCOOL to OVERHEAT (or vice versa) in a single turn, takes the THERMAL SHOCK burst damage and 1-turn STUN, and is then immediately shattered, incinerated, or finished by an allied Mancer while stunned. Secondary win condition: THERMAL GRADIENT ZONE placed across a critical movement corridor where enemies must cross the midline to reach allied positions — every crossing takes THERMAL SHOCK, converting enemy mobility into a kill vector.

**Core weakness:** The Thermomancer's highest-damage output (THERMAL SHOCK) requires a precise temperature-state sequence: SUPERCOOL applied first, OVERHEAT applied second (or OVERHEAT first, SUPERCOOL second) within the same turn. An opponent who clears temperature statuses between turns — by moving off heat or cold zones, letting states expire naturally, or using an allied Hydromancer to cleanse — interrupts the THERMAL SHOCK chain. The Thermomancer is also extremely AP-intensive: applying SUPERCOOL and OVERHEAT in the same turn costs heavy AP investment, leaving the Mancer with little movement and no defense. At 90 HP with moderate armor, it is a priority elimination target for opponents who recognize the THERMAL SHOCK threat developing.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 90 | Below average; the Thermomancer is a specialist that operates from medium range |
| **Move Range** | 3 tiles per activation | Modest; zone placement requires positioning but the zones do the sustained work |
| **Base Armor** | 2 | Slightly above average; compensates for the Thermomancer's tendency to be a priority target |
| **Spell Range** | 5 tiles (base) | Medium; consistent with the precision temperature targeting required |
| **AP per Activation** | 6 | Standard; movement costs 1 AP per tile |
| **Element** | Thermal | The Thermomancer applies both Heat and Cold; its spells are classified as either Heat (fire-element interactions) or Cold (ice-element interactions) depending on which end of the gradient they represent |

**AP budget example:** With 6 AP, the Thermomancer can move 1 tile (1 AP) and cast Heat Lance (2 AP) + Cold Lance (2 AP) + a Quick Thermal Pulse (1 AP) to apply both temperature extremes and begin the THERMAL SHOCK setup sequence, or spend 4 AP on Thermal Gradient Zone placement and 2 AP on movement, or use the full 6 AP on its signature THERMOCLASM ability.

---

## 3. Base Spell Kit

The Thermomancer's four base spells are designed to cover both temperature extremes and their interaction:
- **Heat Lance** — repeatable heat spell; primary OVERHEAT applicator
- **Cold Lance** — repeatable cold spell; primary SUPERCOOL applicator (mirror of Heat Lance)
- **Thermal Inversion** — status transfer: converts one temperature extreme on a target to its opposite (OVERHEAT → SUPERCOOL, or vice versa) in a single cast; primary THERMAL SHOCK trigger in one spell
- **Thermal Gradient Zone** — heavy terrain placement; creates a persistent hot/cold split zone with midline THERMAL SHOCK trigger

---

### Spell 1: Heat Lance

| Field | Value |
|---|---|
| **Name** | Heat Lance |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (projectile) |
| **Range** | 5 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 14 |
| **Element** | Heat / Fire |
| **Effects Applied** | Deals 14 Heat damage. Applies `OVERHEATED` status to hit unit (4 HP/turn fire DoT; fire damage received +50%; 3-turn duration; no stacking — refreshes on re-apply). If the target already has `OVERHEATED`: refreshes duration and deals 8 bonus compression damage (the unit is pushed to maximum heat overload). If the target already has `SUPERCOOLED`: applying OVERHEATED on top of SUPERCOOLED in the same activation triggers `THERMAL SHOCK` immediately — see THERMAL SHOCK documentation. The tile beneath the target becomes a minor heat zone: HEAT_RESIDUE (1-tile; 1-turn duration; any unit on this tile next turn takes 4 HP heat damage; not as severe as ON_FIRE). |
| **Temperature Effects** | **+35 temperature** to the hit unit. This is the strongest single-cast heating available outside of Pillar of Flame. From neutral (0), Heat Lance reaches +35 — immediately HOT (SLOWED, move range −1). From COLD (−30), it reaches +5 WARM, swinging the target from cold vulnerability to fire vulnerability in one cast. From SUPERCOOLED (−40), it reaches −5, pulling the unit out of the SUPERCOOLED threshold in a single hit — this is the THERMAL SHOCK setup: if a second heat application follows in the same activation (or immediately after Calcify/Cold Lance primed them to −40), the crossing of both the −31 and +31 thresholds in rapid sequence triggers THERMAL SHOCK. |
| **Special Interactions** | See terrain interaction table in Section 4. Against `ICE_TILE`: thermal energy melts the tile — ICE_TILE becomes `WET` (same as Fire spell impact; thermal heat counts as Fire-element for terrain interactions). Against `FROZEN` unit status: OVERHEATED on a FROZEN unit — FROZEN is cancelled (fire-ice cancellation), leaving the unit WET. If an enemy Cryomancer has FROZEN a priority target and the Thermomancer applies OVERHEATED before the SHATTER ally acts, the freeze is undone. Against `PERMAFROST`: Heat Lance on PERMAFROST tile removes the frozen mud — tile becomes `MUD` (same thaw as other heat interactions). Against `ON_FIRE` terrain (Pyromancer-created): Heat Lance striking a unit on ON_FIRE terrain applies OVERHEATED while the ON_FIRE DoT is already ticking — combined heat DoT: OVERHEATED (4 HP/turn) + ON_FIRE tile DoT (5 HP/turn) = 9 HP/turn combined. |

**Design note:** Heat Lance is the Thermomancer's reliable OVERHEAT applicator — the first half of the THERMAL SHOCK setup. At 2 AP with no cooldown, it can be used multiple times per activation. Its primary strategic use is not its 14 HP direct damage (moderate), but the OVERHEATED status it applies: fire damage received +50% means any allied Pyromancer spell that follows against an OVERHEATED target deals 1.5× its base damage. Two Heat Lances (4 AP) apply OVERHEATED and refresh it, setting up the maximum-heat state before a Pyromancer's Pillar of Flame becomes 55 × 1.5 = 82.5 HP — within kill range for most un-upgraded Mancers from significant HP remaining.

**Spell answers YES to (design rule check):**
1. Applies unit status (OVERHEATED — fire DoT + damage vulnerability) — YES
2. Applies terrain state (HEAT_RESIDUE tile; melts ICE_TILE, PERMAFROST) — YES
3. Synergizes with Pyromancer (OVERHEAT amplifies fire damage), Cryomancer counter-play (destroys FROZEN) — YES
4. Skill expression: OVERHEAT setup before Pyromancer burst; THERMAL SHOCK sequence initiation — YES

---

### Spell 2: Cold Lance

| Field | Value |
|---|---|
| **Name** | Cold Lance |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (projectile) |
| **Range** | 5 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 14 |
| **Element** | Cold / Ice |
| **Effects Applied** | Deals 14 Cold damage. Applies `OVERCOOLED` status to hit unit (3 HP/turn cold DoT; cold damage received +50%; movement –2; 3-turn duration; no stacking — refreshes on re-apply). If the target already has `OVERCOOLED`: refreshes duration and deals 8 bonus compression damage. If the target already has `OVERHEATED`: applying OVERCOOLED on top of OVERHEATED in the same activation triggers `THERMAL SHOCK` immediately — see THERMAL SHOCK documentation. The tile beneath the target becomes a minor cold zone: COLD_RESIDUE (1-tile; 1-turn duration; any unit on this tile next turn takes 3 HP cold damage and receives a brief CHILLED movement penalty — –1 move for 1 turn). If the target is already `CHILLED` (Cryomancer-applied): Cold Lance hitting a CHILLED target upgrades to `OVERCOOLED` immediately (no SUPERCOOLED prerequisite needed — Cold Lance acts as an accelerated cold escalation from CHILLED). |
| **Temperature Effects** | **−35 temperature** to the hit unit — the mirror of Heat Lance. From neutral (0), Cold Lance reaches −35 SUPERCOOLED (SLOWED + BRITTLE modifier active). From WARM (+30), it reaches −5, swinging the target from fire vulnerability to cold vulnerability in one cast. From HOT (+40), it reaches +5 — pulling the unit out of the HOT threshold and below WARM in a single hit. The **THERMAL SHOCK setup via Cold Lance:** use Cold Lance to bring a neutral unit to −35 SUPERCOOLED, then immediately use Heat Lance (+35) in the same activation — delta = +70 total change, crossing from ≤ −31 (SUPERCOOLED) to ≥ +31 (HOT) in one turn. Magnitude of delta: 70 / 2 = 35 bonus THERMAL SHOCK damage + 1-turn STUN. |
| **Special Interactions** | Against `WET` unit status: Cold Lance flash-freezes a WET unit to `FROZEN` directly — same interaction as any Ice spell on WET (identical to Cryomancer's Ice Lance behavior on WET). Against `OVERCOOLED` unit that is also `BRITTLE_ARMOR` (Cryomancer-applied): Cold Lance hits the unit and the cold damage triggers BRITTLE_ARMOR (+50% damage) — 14 × 1.5 = 21 HP. Cold damage qualifies as the trigger for BRITTLE_ARMOR? By design decision: Cold Lance is classified as Cold/Ice, and BRITTLE_ARMOR is triggered by physical hits — resolution note: Cold Lance specifically does NOT trigger BRITTLE_ARMOR (Cold/Ice is not Physical/Sonic; BRITTLE_ARMOR requires Physical or Sonic impact). Against `ON_FIRE` terrain: Cold Lance hitting a unit on ON_FIRE terrain applies OVERCOOLED while the tile DoT is still present — the cold status and the fire DoT run simultaneously; this is thermally incoherent but mechanically consistent with the temperature gradient concept (the unit is being burned from below and frozen from outside simultaneously — ideal THERMAL SHOCK setup). |

**Design note:** Cold Lance is the perfect mirror of Heat Lance, and together they form the Thermomancer's core setup toolkit. Two Cold Lances (4 AP) apply OVERCOOLED and refresh it, establishing the maximum-cold state before a Cryomancer's freeze goes deeper. OVERCOOLED + Cryomancer Frost Bolt = FROZEN with no CHILLED prerequisite — Cold Lance effectively bypasses the Cryomancer's multi-cast freeze escalation, turning a 4 AP (double Frost Bolt) sequence into a 2 AP (Cold Lance) + 2 AP (Frost Bolt) sequence that is equivalent in outcome but more AP-efficient per Mancer.

**Spell answers YES to (design rule check):**
1. Applies unit status (OVERCOOLED — cold DoT + damage vulnerability + move penalty) — YES
2. Applies terrain state (COLD_RESIDUE; flash-freezes WET) — YES
3. Synergizes with Cryomancer (OVERCOOLED + Frost Bolt = FROZEN faster), Osteomancer (BRITTLE from Cryomancer + OVERCOOLED compounds vulnerability) — YES
4. Skill expression: OVERCOOLED setup before Cryomancer freeze; THERMAL SHOCK sequence initiation — YES

---

### Spell 3: Thermal Inversion

| Field | Value |
|---|---|
| **Name** | Thermal Inversion |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Single Target |
| **Range** | 5 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 10 (base hit) |
| **Element** | Thermal |
| **Effects Applied** | Deals 10 Thermal damage. Then: **Temperature Inversion** — the target's current temperature status is immediately replaced by its opposite extreme at maximum intensity. `OVERHEATED` → `SUPERCOOLED` (full OVERCOOLED + additional 8 thermal shock bonus from the sudden reversal; all fire damage vulnerability replaced by cold damage vulnerability). `OVERCOOLED` → `OVERHEATED` (full OVERHEATED + additional 8 thermal shock bonus from the sudden reversal; cold damage vulnerability replaced by fire damage vulnerability). `CHILLED` → `OVERHEATED` (CHILLED is a milder cold state; the inversion brings it to the opposite extreme). If neither OVERHEATED nor OVERCOOLED is active: Thermal Inversion simply applies mild CHILLED (1-turn; it finds whatever the ambient temperature is and inverts it — the ambient temperature in a non-special-state target is "baseline neutral," which inverts to mild cold). **THERMAL SHOCK trigger:** If a target was OVERHEATED and the Inversion applies OVERCOOLED, or was OVERCOOLED and the Inversion applies OVERHEATED, the transition in a single cast triggers `THERMAL SHOCK` immediately: see THERMAL SHOCK documentation. This is the primary single-spell THERMAL SHOCK trigger — one 3 AP spell applies both extremes in sequence on a pre-conditioned target. |
| **Temperature Effects** | **Flips the target's temperature** — multiplies current temperature by −1.5 (clamped to ±100). A unit at +40 HOT becomes −60 SUPERCOOLED. A unit at −50 SUPERCOOLED becomes +75 OVERHEATED. A unit at +20 WARM becomes −30 COLD. The flip always triggers THERMAL SHOCK if the result crosses both the −31 and +31 thresholds in one application (i.e., target was in HOT/OVERHEATED range and lands in SUPERCOOLED/FROZEN SOLID, or vice versa). **Examples:** Target at +40 → flipped to −60: delta = 100 total; THERMAL SHOCK bonus damage = 100/2 = 50 bonus damage + 1-turn STUN. Target at −50 → flipped to +75: delta = 125 total (capped at relevant threshold crossing); THERMAL SHOCK bonus = 62 bonus damage + 1-turn STUN. This is the highest single-cast temperature swing in the entire roster. |
| **Special Interactions** | Against a `FROZEN` unit (Cryomancer-applied): Thermal Inversion on a FROZEN unit — FROZEN is overridden by OVERHEATED (inversion converts extreme cold → extreme heat). FROZEN is removed; OVERHEATED applied; THERMAL SHOCK triggers (the FROZEN state counts as the "maximum cold" prior state for THERMAL SHOCK purposes: FROZEN → OVERHEATED = THERMAL SHOCK). Against an `OVERHEATED` unit: Inversion applies OVERCOOLED + THERMAL SHOCK. Against a unit under both BURNING DoT (Pyromancer fire) and no temperature status: Thermal Inversion reads BURNING as a heat indicator — the unit is inverted from heat to cold, BURNING is extinguished, OVERCOOLED is applied. Against `STEAM_CLOUD` terrain: A unit in STEAM_CLOUD hit by Thermal Inversion — the steam cloud collapses into either HEAT_RESIDUE (if OVERCOOLED → OVERHEATED inversion) or ICE_TILE (if OVERHEATED → OVERCOOLED inversion). |

**Design note:** Thermal Inversion is the Thermomancer's highest-skill-expression spell. Used on an already-temperature-conditioned target (OVERHEATED from Heat Lance, or OVERCOOLED from Cold Lance), a single 3 AP Thermal Inversion triggers THERMAL SHOCK without requiring both temperature extremes to be applied on the same turn separately. The two-turn setup (Heat Lance turn N → Thermal Inversion turn N+1) is the Thermomancer's most efficient THERMAL SHOCK delivery method: 2 AP on turn N, 3 AP on turn N+1 = 5 AP total across two activations for a THERMAL SHOCK burst. The direct same-turn method (Heat Lance + Cold Lance in one activation) costs 4 AP but applies both simultaneously. Thermal Inversion at 3 AP with a 1-turn setup is the AP-efficient route; same-turn application (4 AP) is the all-in-one-activation route.

**Spell answers YES to (design rule check):**
1. Applies unit status (OVERHEATED or OVERCOOLED — whichever is the inversion) — YES
2. Triggers THERMAL SHOCK (primary in-kit trigger) — YES
3. Removes enemy status (BURNING extinguished, FROZEN cancelled on heat inversion) — YES
4. Synergizes with Pyromancer (convert OVERHEATED to OVERCOOLED, setting up Cryomancer follow-up) — YES
5. Skill expression: pre-conditioning the target with one temperature extreme; timing the Inversion for THERMAL SHOCK — YES

---

### Spell 4: Thermal Gradient Zone

| Field | Value |
|---|---|
| **Name** | Thermal Gradient Zone |
| **AP Cost** | 4 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Ground Target — places a 6-tile zone (3 tiles wide × 2 tiles deep, oriented by the Thermomancer at cast time with a chosen midline direction) |
| **Range** | 4 tiles (to the nearest edge of the zone) |
| **AoE Radius** | 6 tiles total (3-tile hot half + 3-tile cold half) |
| **Base Damage** | 0 at placement; ongoing DoT from zone entry (see below) |
| **Element** | Thermal |
| **Effects Applied** | Creates a persistent zone (4-turn duration) divided by a midline: one half is `BURNING` terrain (hot side; units on this half take 5 HP/turn fire DoT and receive `OVERHEATED` on entry); the other half is `PERMAFROST` terrain (cold side; units on this half take 3 HP/turn cold DoT and receive `OVERCOOLED` on entry). **Midline crossing:** Any unit that moves from the cold half to the hot half (or hot to cold) in a single movement action triggers `THERMAL SHOCK` at the midline tile — see THERMAL SHOCK documentation. Units can choose to stay on one side and only take that side's DoT; crossing is the dangerous action. The Thermomancer itself is immune to the temperature DoT from zones it creates (thermal resistance from its own mastery) but is NOT immune to THERMAL SHOCK from crossing its own midline (it cannot cross without risk). |
| **Temperature Effects** | **Hot side: +15 temperature/turn** to all units on the hot half (end of turn, terrain passive from BURNING tiles). **Cold side: −15 temperature/turn** to all units on the cold half (end of turn, terrain passive from FROZEN/PERMAFROST tiles). A unit forced to remain on the hot side for 3 turns accumulates +45 temperature from terrain passives alone — from neutral (0), this reaches +45 HOT (SLOWED); from any prior warming, it reaches OVERHEATED faster. A unit on the cold side for 3 turns loses −45 — from neutral, reaches −45 SUPERCOOLED (SLOWED + BRITTLE). The midline boundary applies THERMAL SHOCK on crossing: a unit at −35 (SUPERCOOLED on cold side) that crosses to the hot side is immediately subjected to the zone's +15 entry temperature plus any OVERHEATED status applied on entry — the transition from SUPERCOOLED (≤ −31) crossing to HOT (≥ +31 within the same action) triggers THERMAL SHOCK with bonus damage = |ΔTemp| / 2. |
| **Special Interactions** | Against `FLOODED` tiles in the hot half of the zone: fire-water interaction — FLOODED tiles become `STEAM_CLOUD` in the hot half (standard fire on water, but the zone's persistent heat maintains the STEAM_CLOUD for the zone's remaining duration instead of the standard 2-turn expiry). Against `ICE_TILE` in the cold half: cold reinforcement — ICE_TILE duration is extended by 2 turns in the cold half (zone's cold maintains the ice structure). Against units with `BURNING` DoT entering the cold half: BURNING is extinguished on entry into the cold side (cold zone quenches fire status as any cold interaction would). Against units with `OVERCOOLED` status entering the hot half: this is the THERMAL SHOCK trigger scenario — OVERCOOLED → hot side = THERMAL SHOCK. Pyromancer fire spells cast into the hot half: spread faster (hot terrain fans fire — standard fire-on-ON_FIRE intensification). Cryomancer ice spells cast into the cold half: penetrate deeper — CHILLED in the cold half upgrades to OVERCOOLED in 1 application instead of 2. |

**Design note:** Thermal Gradient Zone is the Thermomancer's most complex and potentially most devastating placement. A zone placed across a chokepoint forces the opponent into a decision: stay on one side (taking 3–5 HP/turn DoT and accruing a temperature status) or cross the midline (taking THERMAL SHOCK). Neither option is safe. An enemy that stays on the cold side eventually becomes OVERCOOLED — vulnerable to Cryomancer freeze. An enemy that stays on the hot side eventually becomes OVERHEATED — vulnerable to Pyromancer burst. An enemy that crosses takes THERMAL SHOCK immediately. The zone creates a constant threat that shapes every movement decision the opponent makes within or around it.

**Spell answers YES to (design rule check):**
1. Creates persistent terrain states (BURNING + PERMAFROST in same zone) — YES
2. Applies unit status (OVERHEATED or OVERCOOLED on zone entry; THERMAL SHOCK on midline crossing) — YES
3. Synergizes with Pyromancer (hot side amplification), Cryomancer (cold side amplification), Gravimancer (pulling units across midline) — YES
4. Skill expression: zone orientation relative to movement paths; zone placement to force midline crossings — YES

---

## 3b. Temperature Interaction Notes

The Thermomancer is the roster's TEMPERATURE MASTER — the only Mancer with both a strong heating spell and a strong cooling spell in the same base kit. It is also the only Mancer whose signature mechanic (THERMAL SHOCK) is explicitly derived from the temperature system's threshold-crossing rule.

**Temperature values summary:**
- Heat Lance: +35 temperature (strong heating — HOT from neutral in one hit)
- Cold Lance: −35 temperature (mirror of Heat Lance — SUPERCOOLED from neutral in one hit)
- Thermal Inversion: flips current temperature × −1.5 (clamped to ±100) — the highest single-cast temperature swing in the game
- Thermal Gradient Zone: +15 temperature/turn (hot side) and −15 temperature/turn (cold side) as ongoing terrain passives

**THERMAL SHOCK setup — Cold Lance into Heat Lance (same activation):**
Cold Lance (−35 → SUPERCOOLED, ≤ −31) immediately followed by Heat Lance (+35) in the same activation: the target's temperature swings from −35 to +0. However, because the SUPERCOOLED threshold (≤ −31) was crossed by Cold Lance and then the subsequent Heat Lance application pushes through the −31 boundary in the opposite direction and continues to HOT (+0 is within WARM, but the OVERCOOLED status triggers the THERMAL SHOCK rule when OVERHEATED is applied on top of it in the same activation via the Heat Lance OVERHEATED application): THERMAL SHOCK triggers. Bonus damage = |ΔTemp| / 2 = 70 / 2 = 35 bonus damage + 1-turn STUN. Total AP cost: 2 AP (Cold Lance) + 2 AP (Heat Lance) = 4 AP for a THERMAL SHOCK trigger with the STUN, leaving 2 AP for movement on the same activation.

**More optimal THERMAL SHOCK: Cold Lance (−35) → second Cold Lance (−35 → −70) → Heat Lance (+35 → −35 still SUPERCOOLED) — actually:** The cleanest two-step same-activation THERMAL SHOCK: Cold Lance to reach −35 SUPERCOOLED (applying OVERCOOLED status), then Heat Lance applying OVERHEATED on top of OVERCOOLED — the OVERHEATED+OVERCOOLED simultaneous state triggers THERMAL SHOCK by the core mechanic. The ΔTemp for the shock bonus = the full swing: starting temperature before Cold Lance (0) through SUPERCOOLED (−35) then Heat Lance flipping to OVERHEATED application (+35 from SUPERCOOLED = back to 0 temperature, but both OVERCOOLED and OVERHEATED statuses exist simultaneously for one moment) — bonus damage = |−35 to OVERHEATED crossing| / 2 = 35 bonus damage + STUN.

**Thermal Inversion combo with Pyromancer:**
When allied Pyromancer and Thermomancer are operating together, enemies near Pyromancer fire terrain accumulate +10 temperature/turn from standing on ON_FIRE tiles. After 3–4 turns, a unit standing near burning terrain reaches +30 to +50 HOT. Thermomancer then uses Thermal Inversion: +40 × −1.5 = −60 SUPERCOOLED. In a single action, the enemy goes from HOT (near Pyromancer terrain) to deep SUPERCOOLED — THERMAL SHOCK triggers, the unit is STUNNED for 1 turn, and is now at −60 SUPERCOOLED with BRITTLE modifier. Any Cryomancer follow-up (or even a single Frost Bolt at −20) pushes below −61 FROZEN SOLID: the unit goes from HOT → STUNNED/SUPERCOOLED → FROZEN SOLID in one Thermomancer + one Cryomancer activation.

**The "boiling point" strategy:**
Use Thermal Gradient Zone to establish a hot-side area. Force or funnel enemies into the hot side — they accumulate +15 temperature/turn from the BURNING terrain. After 3 turns on the hot side, a neutral unit is at +45 HOT (SLOWED, cannot easily escape). Thermomancer then uses Thermal Inversion: +45 × −1.5 = −67 FROZEN SOLID. The unit goes from HOT (SLOWED) to FROZEN SOLID in one cast, simultaneously triggering THERMAL SHOCK (bonus damage = 112/2 = 56 bonus damage + STUN) because the crossing goes from ≥ +31 (HOT) to ≤ −61 (FROZEN SOLID), crossing both the +31 and −31 thresholds in a single application. The combined effect: 10 Thermal Inversion base damage + 56 THERMAL SHOCK bonus + FROZEN SOLID status + 1-turn STUN — all from a single 3 AP spell.

**Thermomancer as the bridge between Pyromancer and Cryomancer:**
All three Mancers in the same warband creates the "temperature whiplash" strategy: Pyromancer heats enemies via fire terrain and spell application (+20 to +35 temperature per fire spell), Thermomancer amplifies or inverts that heat (either adding +35 via Heat Lance to push past OVERHEATED, or inverting an enemy at +50 to −75 FROZEN SOLID via Thermal Inversion), and Cryomancer exploits the resulting SUPERCOOLED/FROZEN SOLID state for freeze-shatter kill confirmation. The opponent faces three simultaneous temperature threats: fire heating from the Pyromancer's terrain (cannot be ignored — OVERHEATED DoT is punishing), inversion risk from the Thermomancer (standing HOT near a Thermomancer means one Thermal Inversion away from FROZEN SOLID), and hard freeze from the Cryomancer (every SUPERCOOLED unit is one Ice Lance from FROZEN and a SHATTER kill). Juggling all three simultaneously is nearly impossible, forcing the opponent to choose which temperature threat to respect — and leaving the others unaddressed.

---

## 4. THERMAL SHOCK Documentation

### THERMAL SHOCK Status — Core Mechanic

`THERMAL SHOCK` triggers when a unit transitions from SUPERCOOLED (maximum cold state) to OVERHEATED (maximum heat state), or vice versa, within the same turn.

**Trigger conditions:**
- Same activation: OVERCOOLED unit receives OVERHEATED (from Heat Lance, Thermal Inversion, or hot side of Thermal Gradient Zone)
- Same activation: OVERHEATED unit receives OVERCOOLED (from Cold Lance, Thermal Inversion, or cold side of Thermal Gradient Zone)
- Midline crossing: unit crosses the Thermal Gradient Zone midline while carrying the opposite side's temperature status
- FROZEN → OVERHEATED: Thermal Inversion on a FROZEN unit triggers THERMAL SHOCK (FROZEN counts as maximum cold for shock purposes)

**THERMAL SHOCK effect:**
- Immediate burst damage: equivalent to double the combined sum of both temperature status DoT values at trigger time
  - OVERCOOLED (3 HP/turn) + OVERHEATED (4 HP/turn) = 7 HP/turn combined; THERMAL SHOCK deals 7 × 2 = 14 HP burst damage as baseline
  - Plus: an additional 20 HP base burst damage from the thermal detonation itself
  - Total minimum THERMAL SHOCK: 14 HP (status-derived) + 20 HP (base burst) = 34 HP baseline
  - If statuses have been refreshed/amplified: up to 8 HP/turn per status = 16 HP + 20 HP = 36 HP baseline; higher with damage multipliers
- 1-turn STUN (the unit's next activation is lost — skip entire turn, no AP)
- Both temperature statuses are consumed (OVERHEATED and OVERCOOLED both removed on THERMAL SHOCK trigger)
- `BRITTLE_ARMOR` interaction: if the THERMALLY SHOCKED unit also has BRITTLE_ARMOR (Cryomancer-applied), the THERMAL SHOCK burst counts as a physical impact and triggers BRITTLE_ARMOR — THERMAL SHOCK burst × 1.5 = ~51 HP total from THERMAL SHOCK + BRITTLE_ARMOR trigger simultaneously
- `OSTEOMANCER BRITTLE` interaction: OVERCOOLED status applies BRITTLE to bone structures — OVERCOOLED + Osteomancer BRITTLE stacks additively, making the THERMAL SHOCK burst hit harder on BRITTLE-affected targets

---

## 5. Terrain Interaction Table

### Thermal Spell Impact on Existing Terrain States

The Thermomancer's Heat spells follow Fire-element terrain interactions and Cold spells follow Ice-element terrain interactions. The following covers Thermal-specific and edge-case interactions.

| Existing Terrain State | What Happens When Thermal Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **Normal (GROUND)** | Temperature residue left on tile | `HEAT_RESIDUE` (Heat spell; 1-turn DoT tile) or `COLD_RESIDUE` (Cold spell; 1-turn chill tile) | Takes spell damage | Residue tiles are weak but real terrain states — persistent heat or cold at the foot-level |
| **ON_FIRE** | Heat Lance amplifies existing fire | `ON_FIRE` (spread rate increased this turn — fans 1 extra adjacent tile) | Takes spell damage + OVERHEATED; existing BURNING refreshed | Heat amplifies fire; Cold Lance on ON_FIRE = fire quenched → WET (standard cold-on-fire) |
| **ICE_TILE** | Heat Lance melts ice | `WET` (residue — heat melts ice to water) | Takes spell damage; FROZEN cancelled if applicable; OVERCOOLED replaced by OVERHEATED | Cold Lance on ICE_TILE = duration extended; ICE_TILE becomes PERMAFROST if Cold Lance hits twice in same activation |
| **FLOODED** | Heat Lance creates steam | `STEAM_CLOUD` (3-tile radius; 2-turn duration; same as Fire spell on FLOODED) | Takes reduced damage (–20% water absorption) + `BLINDED` | Cold Lance on FLOODED = mass ICE_TILE (same as Cryomancer on FLOODED) |
| **WET** | Heat Lance on WET = evaporation | `STEAM_CLOUD` (1-tile; 1-turn; smaller than FLOODED interaction) | Takes spell damage; no OVERHEATED (wet surface resists heat partially) | Cold Lance on WET = ICE_TILE (instant freeze; standard ice on wet) |
| **CHARGED** | Thermal energy interacts with electrical charge | `CHARGED` (unchanged; CHARGED adds 10 arc damage to thermal hit) | Takes spell damage + 10 Lightning arc damage | Thermal-CHARGED interaction creates a combined arc; units adjacent to CHARGED tile hit by a Thermal Lance also take 5 arc damage |
| **TOXIC_TERRAIN** | Heat Lance: toxic fire creation | `TOXIC_FIRE` hybrid (TOXIC_TERRAIN + ON_FIRE; same as Pyromancer on TOXIC_TERRAIN) | Takes spell damage + `BURNING` + 1 `POISONED` stack | Cold Lance: toxic preservation — TOXIC_TERRAIN freezes, creating ICE_TILE with preserved poison (POISONED stacks don't decay while frozen) |
| **MUD** | Heat Lance dries mud | `GROUND` (dried out; mud removed; same as Pyromancer on MUD) | Takes spell damage | Cold Lance on MUD = PERMAFROST (frozen mud; same as Cryomancer on MUD) |
| **OBSIDIAN** | Thermal energy cannot affect obsidian | `OBSIDIAN` (unchanged) | Takes spell damage; no terrain state change | Obsidian resists thermal manipulation as it resists fire and ice |
| **PERMAFROST** | Heat Lance thaws permafrost | `MUD` (permafrost cracked open; 2-turn duration; same as fire on permafrost) | Takes spell damage + `CHILLED` removed | Cold Lance on PERMAFROST = duration extended by 2 turns |
| **OVERGROWTH** | Heat Lance ignites organic matter | `ON_FIRE` (fast-spreading; same as fire on overgrowth) | Takes spell damage + `BURNING` | Cold Lance on OVERGROWTH = frozen overgrowth (ICE_TILE over OVERGROWTH; brittle; shatters on physical damage) |
| **HEAT_RESIDUE** | Heat Lance on HEAT_RESIDUE = escalation | `ON_FIRE` (residue escalated to full fire tile by additional heat input) | Takes spell damage + `OVERHEATED` | Cold Lance on HEAT_RESIDUE = neutralization → `GROUND` (opposing temperatures cancel) |
| **COLD_RESIDUE** | Heat Lance on COLD_RESIDUE = neutralization | `GROUND` (opposing temperatures cancel) | Takes spell damage | Cold Lance on COLD_RESIDUE = escalation → `ICE_TILE` (residue escalated to full ice tile) |
| **STEAM_CLOUD** | Heat Lance through steam = superheated steam | `STEAM_CLOUD` (maintained; duration extended; now superheated — units inside take 6 HP/turn instead of 3) | Takes spell damage + `OVERHEATED` from superheated steam contact | Cold Lance on STEAM_CLOUD = condensation → `WET` tile (cold condenses steam to liquid water residue) |

### Thermal Gradient Zone: Detailed Terrain Resolution

When Thermal Gradient Zone is active, its hot and cold halves overwrite underlying terrain states:
- Hot half: underlying terrain becomes BURNING (or intensifies if already BURNING)
- Cold half: underlying terrain becomes PERMAFROST (or intensifies if already ICE_TILE)
- When the zone expires (4-turn duration): hot half leaves HEAT_RESIDUE (1 additional turn); cold half leaves COLD_RESIDUE (1 additional turn); then returns to original terrain state

### Terrain States Beneficial to the Thermomancer

| State | Benefit |
|---|---|
| `OVERHEATED` units (self-applied) | Heat Lance follow-up refreshes OVERHEATED and deals 8 bonus compression damage; OVERHEATED also sets up Pyromancer burst amplification (+50% fire damage received) |
| `OVERCOOLED` units (self-applied) | Cold Lance follow-up refreshes and deals 8 bonus compression damage; OVERCOOLED sets up Cryomancer freeze escalation |
| `ON_FIRE` tiles (Pyromancer-created) | Thermomancer immune to its own Thermal Gradient Zone fire DoT; Pyromancer ON_FIRE serves as pre-existing heat that compounds with OVERHEAT for 9 HP/turn combined |
| `ICE_TILE` / `PERMAFROST` (Cryomancer-created) | Cold residue tiles support SUPERCOOL buildup; Cryomancer-created cold terrain complements the Thermomancer's cold half zone |

### Terrain States Hazardous to the Thermomancer

| State | Hazard |
|---|---|
| `CHARGED` tiles | CHARGED terrain adds arc damage to all thermal hits; the Thermomancer is not immune to arc chain damage |
| `TOXIC_TERRAIN` | No poison immunity; POISONED stacks compound with temperature DoT to create a lethal multi-DoT scenario |
| `GRAVITY_WELL` | Pull toward center disrupts precise positioning required for Thermal Gradient Zone maintenance; the zone's value depends on the Thermomancer holding position for midline management |
| `FLOODED` (enemy Hydromancer-created in hot zone) | Enemy Hydromancer flooding the hot half of the Thermal Gradient Zone converts it to STEAM_CLOUD (fire meets water), eliminating the hot terrain investment |

---

## 6. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

Spell variants replace a base spell entirely with a more powerful or specialized version.

#### Variant A: Searing Bolt (replaces Heat Lance) — +20 pts

**Description:** Heat Lance is replaced by Searing Bolt — a heavier heat projectile. Searing Bolt deals 22 damage (up from 14), creates a `BURNING` tile at the impact point (full ON_FIRE state, not just HEAT_RESIDUE; standard ON_FIRE duration and spread), and applies OVERHEATED. AP cost is 3 AP; cooldown is 1 turn.

**Trade-off:** Significantly higher damage and proper ON_FIRE terrain creation (instead of weak HEAT_RESIDUE), at the cost of the no-cooldown economy of Heat Lance. Best for Thermomancers that need direct damage output alongside temperature manipulation — Searing Bolt contributes to the Pyromancer's fire economy by creating ON_FIRE tiles directly.

#### Variant B: Deep Freeze Bolt (replaces Cold Lance) — +20 pts

**Description:** Cold Lance is replaced by Deep Freeze Bolt — a heavier cold projectile that deals 20 damage (up from 14), creates a `PERMAFROST` tile at the impact point (permanent cold terrain, not just COLD_RESIDUE), and applies OVERCOOLED. AP cost is 3 AP; cooldown is 1 turn.

**Trade-off:** Higher damage and permanent cold terrain creation at the cost of Cold Lance's no-cooldown availability. PERMAFROST creation at range (Cryomancer only creates PERMAFROST from MUD with Glacial Spike; Deep Freeze Bolt creates it directly on GROUND) is a unique terrain placement no other Mancer can replicate — the Thermomancer adds permanent cold terrain to the board independently.

---

### Passive Traits

#### Passive A: Thermal Amplifier — +25 pts

**Description:** The Thermomancer's temperature statuses interact with allied Pyromancer and Cryomancer spells at enhanced levels. When an OVERHEATED unit is hit by any Pyromancer spell: fire damage is amplified by an additional 25% (total +75% instead of +50% from OVERHEATED alone). When an OVERCOOLED unit is hit by any Cryomancer spell: cold damage is amplified by +25% beyond OVERCOOLED's standard +50%. Additionally, THERMAL SHOCK burst damage increases by 15 HP (from 34 HP baseline to 49 HP baseline).

**Trade-off:** This is the Thermomancer's "make other Mancers stronger" passive — it enhances the damage amplification it provides to Pyromancer and Cryomancer while also upgrading THERMAL SHOCK's burst. Best for warbands explicitly built around Pyromancer + Thermomancer + Cryomancer synergy where all three Mancers are amplifying each other.

**Synergy note:** With Thermal Amplifier, Pyromancer Pillar of Flame (55 HP base) on an OVERHEATED target becomes 55 × 1.75 = 96 HP — near-lethal against any un-upgraded Mancer from any HP value. Cryomancer Blizzard Field (10 HP per unit) on an OVERCOOLED target becomes 10 × 1.75 = 17.5 HP per unit in the AoE, meaningfully increasing mass-freeze damage output.

#### Passive B: Heat Exchange — +20 pts

**Description:** The Thermomancer can redirect temperature statuses between units (as documented in the status removal table in status-effects.md). Once per activation (1 AP; range 3 tiles): transfer one temperature status (OVERHEATED or OVERCOOLED) from its current host to any other unit within 3 tiles of the original host. The original host loses the status; the new host gains it at full duration. This can be used offensively (transfer an ally's OVERCOOLED to an enemy unit, then apply OVERHEATED to that enemy for THERMAL SHOCK), defensively (transfer an enemy's OVERHEATED back to the enemy themselves or to another enemy), or tactically (give an ally OVERHEATED to prime them for a Pyromancer damage amplification play without the ally having to be hit by the Thermomancer directly).

**Trade-off:** Status redirection is an extremely versatile defensive and offensive tool, but costs 1 AP per use and has a short range (3 tiles; requires the Thermomancer to be relatively close to the transfer). Best for Thermomancers that act as the team's temperature state manager — constantly moving heat and cold between units to create the configurations needed for THERMAL SHOCK.

#### Passive C: Thermal Immunity — +15 pts

**Description:** The Thermomancer is fully immune to both OVERHEATED and OVERCOOLED status effects applied by any source. It cannot be THERMALLY SHOCKED — since it is immune to both temperature extremes, it can never be in the state required for THERMAL SHOCK to trigger on itself. Additionally, the Thermomancer takes 50% reduced damage from Fire and Ice spells (Pyromancer, Cryomancer, Thermomancer — all thermal elements).

**Trade-off:** Pure defensive investment. The Thermomancer becomes significantly harder to temperature-counter — enemy Pyromancers and Cryomancers deal half damage to it, and neither temperature status affects it. The cost is that the Thermomancer cannot be a THERMAL SHOCK victim, but it also cannot accidentally THERMAL SHOCK itself (removing the risk of Heat Lance into an existing OVERCOOLED on itself). Best in matchups where the opponent has both fire and ice elements in their warband.

---

### Stat Enhancements

#### Stat A: Thermal Constitution (+20 HP) — +10 pts

**Description:** Max HP increases from 90 to 110. Brings the Thermomancer to solid mid-tier durability, appropriate for its role as a target-priority piece in most matchups (opponents want to stop THERMAL SHOCK before it reaches full deployment).

**Design note:** At 90 HP, the Thermomancer is a THERMAL SHOCK victim if caught by an enemy Thermomancer or Cryomancer-FROZEN + SHATTER. 110 HP survives most single-turn burst sequences without enhancement — critical for a Mancer that must stay on the field long enough to set up 2-turn THERMAL SHOCK sequences.

#### Stat B: Thermal Efficiency (+1 Spell Range) — +15 pts

**Description:** All Thermomancer spell ranges increase by 1 tile. Heat Lance: 5 → 6. Cold Lance: 5 → 6. Thermal Inversion: 5 → 6. Thermal Gradient Zone: 4 → 5. Combined with Elevated tile bonus, the Thermomancer on elevated terrain reaches 7 tiles with both Heat and Cold Lance — enough to OVERHEAT and OVERCOOL targets from a position where most melee Mancers cannot close the gap in a single activation.

---

### Signature Ability

The Signature Ability is unlocked as an additional spell slot — it does not replace any base spell.

#### Signature: Thermoclasm — +40 pts

| Field | Value |
|---|---|
| **Name** | Thermoclasm |
| **AP Cost** | 6 AP (entire activation; Thermomancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Self-centered AoE — the Thermomancer is the origin; thermal energy radiates outward |
| **Range** | N/A (self-centered) |
| **AoE Radius** | 4 tiles in all directions from the Thermomancer's current position |
| **Base Damage** | Variable — half the radius (inner 2 tiles) receives OVERHEATED + 20 HP fire burst; outer half (tiles 3–4 from center) receives OVERCOOLED + 15 HP cold burst; units on exactly the 2-tile boundary receive BOTH simultaneously — THERMAL SHOCK is triggered on them regardless of their prior temperature state |
| **Element** | Thermal |
| **Effects Applied** | The Thermomancer releases a radial thermoclasm: inner ring (0–2 tiles from Thermomancer) is bathed in searing heat (20 HP fire burst; all units OVERHEATED; all tiles become HEAT_RESIDUE). Outer ring (3–4 tiles from Thermomancer) is swept with extreme cold (15 HP cold burst; all units OVERCOOLED; all tiles become COLD_RESIDUE). Units exactly at the 2-tile boundary between the rings take both the heat burst (20 HP) and the cold burst (15 HP) sequentially in the same instant — THERMAL SHOCK triggers on them (35 HP burst + 20 HP THERMAL SHOCK burst = 55 HP total to boundary units, plus 1-turn STUN). Terrain at the boundary tiles becomes `THERMAL_FRACTURE` — a unique terrain state that deals 8 HP to any unit entering the tile for the following 3 turns (the ground itself is thermally unstable). The Thermomancer itself is immune to all effects of Thermoclasm within its own radius. |
| **Special Interactions** | Against units that were already OVERHEATED when Thermoclasm fires: inner ring units who were OVERHEATED receive the outer ring's cold burst as a secondary pulse (they are OVERCOOLED to match the outer ring on the same cast), triggering THERMAL SHOCK as a secondary detonation on top of the inner ring's primary heat burst. Against units that were already OVERCOOLED when Thermoclasm fires: inner ring units receive the heat burst and THERMAL SHOCK triggers. Against Pyromancer ON_FIRE terrain in the inner ring: heat amplifies fire — ON_FIRE tiles spread 2 additional tiles this turn (the thermoclasm fans existing fire). Against Cryomancer ICE_TILE in the outer ring: cold deepens — ICE_TILE becomes PERMAFROST (the thermoclasm super-freezes existing ice). |

**Design note:** Thermoclasm is the Thermomancer's capstone ability and the defining moment of the 19-Mancer roster. No other ability in the game creates THERMAL SHOCK on all units at a specific range simultaneously from a self-centered pulse. The boundary band (exactly 2 tiles from the Thermomancer) is the kill zone — units at that precise range take both the heat and cold bursts in sequence, triggering THERMAL SHOCK for 55 HP total plus 1-turn STUN. A formation where multiple enemy units are positioned at the 2-tile boundary is essentially eliminated by a single Thermoclasm activation. This requires the Thermomancer to be in the center of a formation — deliberately surrounded — which is the inversion of standard Mancer positioning. An allied Gravimancer's Gravitational Collapse that pulls enemies to a center point, with the Thermomancer at the center, sets up Thermoclasm perfectly: enemies at 2 tiles from the Thermomancer's position take THERMAL SHOCK; enemies within 2 tiles take the heat burst; enemies beyond 3 tiles take the cold burst.

**Synergy note:** Thermoclasm + Gravimancer GRAVITATIONAL COLLAPSE is the game's most demanding and most devastating two-Mancer combo sequence. Gravimancer pulls all enemies to a center. Thermomancer positions at that center and fires Thermoclasm. All enemies that were pulled to within 2 tiles of the Thermomancer are now on the boundary — they take THERMAL SHOCK (55 HP total) and are STUNNED. The Cryomancer then fires Glacier's Wrath on the STUNNED cluster — FROZEN applied to STUNNED units persists through the STUN duration. On the following turn, any physical or sonic Mancer shatters the FROZEN cluster. This four-Mancer sequence (Gravimancer → Thermomancer → Cryomancer → physical attacker) is the highest-tier Tier 3 combo available — five coordinated Mancer activations for guaranteed elimination of an entire enemy warband cluster.

---

## 6. Faction Synergy

### Best Faction: The Gilded Throne

The Gilded Throne's Iron Discipline (Charm and Panic immunity) is the most relevant faction trait for the Thermomancer's complex activation sequences. THERMAL SHOCK setups require 2-turn precision — a Psychomancer CONFUSING or CHARMING the Thermomancer on turn N before the THERMAL SHOCK fires on turn N+1 wastes the entire investment. Iron Discipline protects the Thermomancer's critical activation from this disruption.

Crossbow Corps and Siege Arbalests fire physical bolts. STUNNED targets (from THERMAL SHOCK) cannot dodge or respond — a Crossbow Corps volley into a THERMALLY SHOCKED, STUNNED cluster deals full damage to motionless, defenseless targets. The Gilded Throne's sustained physical damage from multiple Crossbow units converts every THERMAL SHOCK STUN into a guaranteed multi-hit physical barrage. Iron Vanguard SHATTER on FROZEN + THERMALLY SHOCKED targets — if Cryomancer follows Thermomancer's THERMAL SHOCK with a FROZEN application before the STUN expires — is the most reliable kill confirmation the Throne has access to.

**Specific synergies:**
- Thermomancer OVERHEAT → Pyromancer Pillar of Flame (amplified) = 96 HP (with Thermal Amplifier passive)
- Thermomancer THERMAL SHOCK STUN → Siege Arbalest volley into STUNNED target = armor-piercing physical on a motionless target
- Thermomancer Thermal Gradient Zone midline across a corridor → Conscript Spearmen holding the midline's edge → enemies crossing take THERMAL SHOCK and then face Spearmen with 1-tile spear reach without the Spearmen needing to advance

### The Verdant Pact — Temperature and Terrain Bond

Verdant Pact's Terrain Bond provides movement and regen on natural tiles. The Thermomancer's HEAT_RESIDUE and COLD_RESIDUE are not classified as natural terrain — they are created thermal states, not earth or organic origin. BURNING tiles (from Thermal Gradient Zone hot side) are fire terrain — not natural. PERMAFROST (from Thermal Gradient Zone cold side or Cold Lance) is frozen modified earth — classified as a natural-origin terrain for Terrain Bond? Design ruling: PERMAFROST is EARTH origin (Geomancer creates PERMAFROST from MUD which is earth-based); Thermomancer-created PERMAFROST from Cold Lance is also earth-based in composition — Verdant Pact Terrain Bond triggers on Thermomancer-created PERMAFROST tiles. Thornback Sentinels regenerating on PERMAFROST while the Thermomancer maintains the cold side of a Thermal Gradient Zone is a subtle but real benefit.

The Glade Archer's POISONED application plus OVERCOOLED (Thermomancer) preserves POISONED stacks while frozen — OVERCOOLED leads to FROZEN (via Cryomancer follow-up), and POISONED is preserved through FROZEN. The Glade Archer + Thermomancer combination creates a POISONED-preserved, temperature-conditioned target that takes DoT from three simultaneous sources (POISONED stacks + OVERCOOLED cold DoT + eventual BURNING from THERMAL SHOCK resolution) when the temperature states resolve.

### The Ashen Covenant — Thermal Death Economy

Grave Husks regen in BURNING terrain. The Thermomancer's Thermal Gradient Zone hot side is BURNING terrain — Husks advancing through the hot side of the zone regenerate rather than taking DoT. Meanwhile, enemies entering the hot side from outside take OVERHEAT + 5 HP/turn. This creates the same fire-advance asymmetry as the Pyromancer + Ashen Covenant combination, but now the hot zone is guaranteed to be adjacent to a cold zone (the other half of the Thermal Gradient), creating a midline kill zone that enemies cannot escape by moving to the cold side either.

THERMAL SHOCK kills — enemies who take THERMAL SHOCK burst and are subsequently killed — generate Necromancer fuel (deaths in any circumstances generate fuel for Deathless Ranks). A Thermomancer that THERMAL SHOCKs multiple enemies simultaneously (via Thermoclasm boundary band) generates multiple simultaneous deaths if the STUN period makes them easy to finish — mass fuel generation in a single coordinated Thermoclasm turn.

---

## 7. Combo Chains

### Combo 1 — Thermal Primer (Thermomancer + Pyromancer) [SIGNATURE]

**Mancers involved:** Thermomancer + Pyromancer

**Step-by-step execution:**

1. **Thermomancer activates (Turn N):** Heat Lance (2 AP) at target — OVERHEATED applied (fire damage received +50%; 4 HP/turn DoT). Thermomancer moves remaining AP.
2. **Pyromancer activates (Turn N or N+1):** Pillar of Flame (5 AP) at OVERHEATED target. Damage: 55 HP × 1.5 (OVERHEATED amplification) = 82.5 HP. The target also becomes `BURNING` (5 HP/turn) on top of existing OVERHEATED DoT (4 HP/turn) = 9 HP/turn combined.

**Result:** A standard Pyromancer Pillar of Flame deals 55 HP. With Thermomancer OVERHEAT setup: 82 HP. The difference — 27 HP — is enough to extend the Pillar's effective kill range from units under ~55 HP to units under ~82 HP. Most un-upgraded Mancers in the game have 85–100 HP. The Thermomancer's 2 AP investment converts Pillar of Flame from a "doesn't quite kill" hit into a near-certain kill on most targets.

---

### Combo 2 — Thermal Shock Setup (Thermomancer solo, 2-turn)

**Mancers involved:** Thermomancer solo

**Step-by-step execution:**

1. **Turn N, Thermomancer activates:** Heat Lance (2 AP) at target — OVERHEATED applied.
2. **Turn N+1, Thermomancer activates:** Thermal Inversion (3 AP) at OVERHEATED target — instantly inverts OVERHEATED to OVERCOOLED, triggering THERMAL SHOCK: 34 HP burst damage + 1-turn STUN.
3. **Thermomancer remaining AP:** Can cast Cold Lance (2 AP) into the STUNNED target for additional OVERCOOLED buildup or an Arcane/Cold hit.

**THERMAL SHOCK from THERMAL SHOCK:** Is the THERMALLY SHOCKED unit now OVERCOOLED (from the Thermal Inversion)? Yes — after THERMAL SHOCK resolves, the unit retains the OVERCOOLED status (the shock consumed both statuses that triggered it, but the Thermal Inversion applied OVERCOOLED as its output, not as the trigger input). The OVERCOOLED applied by Thermal Inversion persists post-shock. The unit is STUNNED and OVERCOOLED — setup for Cryomancer FROZEN on the following turn.

---

### Combo 3 — Cold Chain (Thermomancer + Cryomancer)

**Mancers involved:** Thermomancer + Cryomancer

**Step-by-step execution:**

1. **Thermomancer activates (Turn N):** Cold Lance (2 AP) at target — OVERCOOLED applied (cold damage received +50%; 3 HP/turn DoT; movement –2).
2. **Cryomancer activates (Turn N):** Frost Bolt (2 AP) at OVERCOOLED target — Cold Lance's OVERCOOLED means the target is at "maximum cold state," so a single Frost Bolt upgrades OVERCOOLED → FROZEN (OVERCOOLED replaces CHILLED as the "already cold" prerequisite for ice-spell freeze escalation). The target is FROZEN with 1 Frost Bolt instead of 2.

**AP efficiency:** Standard Cryomancer FROZEN via double Frost Bolt = 4 AP (2 × 2 AP). Thermomancer + Cryomancer FROZEN = 2 AP (Cold Lance) + 2 AP (Frost Bolt) = 4 AP total but split between two Mancers. Net effect: the same outcome at the same total AP cost, but each Mancer spent only 2 AP on the setup, leaving 4 AP each for movement and follow-up. The Cryomancer has 4 AP left for SHATTER setup; the Thermomancer has 4 AP left for Thermal Gradient Zone placement or another Cold Lance.

---

### Combo 4 — Gradient + Gravitational (Thermomancer + Gravimancer)

**Mancers involved:** Thermomancer + Gravimancer

**Step-by-step execution:**

1. **Thermomancer activates (Turn N):** Thermal Gradient Zone (4 AP) across a corridor. Midline set perpendicular to the expected enemy advance direction. Hot side faces the enemies' starting side; cold side faces the allies' side.
2. **Enemy advances into zone (Turn N+1):** Enemies enter the cold half of the zone, taking OVERCOOLED + 3 HP/turn DoT. They are now OVERCOOLED and want to push through to the allied side — but to do so they must cross the midline.
3. **Gravimancer activates (Turn N+2):** Gravity Well placed at the hot side of the Thermal Gradient Zone. The Gravity Well begins pulling OVERCOOLED enemies across the midline toward the hot side. Every enemy pulled across the midline by the Gravity Well takes THERMAL SHOCK as they pass from cold to hot territory (OVERCOOLED → hot half entry = THERMAL SHOCK trigger).

**Result:** The Gravity Well converts passive zone setup into forced midline crossings — enemies who resist advancing through the zone are pulled through it by gravity. Each pull-through crossing triggers THERMAL SHOCK (34 HP burst + STUN) automatically, with no additional Thermomancer AP required after the initial zone placement.

---

### Combo 5 — Thermoclasm Apex (Thermomancer + Gravimancer + Cryomancer) [TIER 3 COMBO]

**Mancers involved:** Thermomancer + Gravimancer + Cryomancer (3-Mancer Tier 3)

**Step-by-step execution:**

1. **Gravimancer activates (6 AP):** GRAVITATIONAL COLLAPSE at the Thermomancer's position (the center). All enemies within 5 tiles pulled to the Thermomancer. Travel damage applied. All enemies now clustered at ~2 tiles from the Thermomancer's position.
2. **Thermomancer activates (6 AP):** THERMOCLASM. Boundary ring at 2 tiles — all enemies that were pulled to the Collapse center are on the boundary (exactly 2 tiles from the Thermomancer). Boundary units take THERMAL SHOCK: 55 HP total + 1-turn STUN. All enemies near the center take heat burst (20 HP). All enemies at 3–4 tiles take cold burst (15 HP).
3. **Cryomancer activates (same turn — Mancer initiative):** Glacier's Wrath on the STUNNED cluster. FROZEN applied to all STUNNED units in the 5-tile radius. STUNNED + FROZEN simultaneously.
4. **Physical Mancer activates (Turn N+1):** Any physical or sonic attacker fires into the FROZEN cluster. SHATTER on every FROZEN unit simultaneously.

**Combined damage math (on boundary-ring unit, 100 HP baseline):** GRAVITATIONAL COLLAPSE travel damage (assume 4 tiles: 32 HP) + THERMAL SHOCK burst (55 HP) = 87 HP before the Cryomancer step. Unit is at 13 HP, STUNNED, and FROZEN. Any physical hit (even a Conscript Spearman's ~15 HP melee) triggers SHATTER: 15 × 2.5 = 37 HP — overkill at this HP level. Every unit on the boundary ring that survives the Collapse + THERMAL SHOCK is eliminated by the smallest physical hit on the following turn.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Thermomancer

| Mancer | Counter Mechanism |
|---|---|
| **Hydromancer** | Hydromancer's Cleanse removes BURNING (which counters OVERHEATED application by the Thermomancer — BURNING and OVERHEATED are related fire statuses; Cleanse removes the fire component). A Hydromancer flooding the hot half of the Thermomancer's Thermal Gradient Zone converts it to STEAM_CLOUD (fire on water), eliminating the hot terrain investment. Additionally, the Hydromancer can apply WET to OVERHEATED units — WET + OVERHEATED is not a THERMAL SHOCK (WET is not OVERCOOLED); but Hydromancer can break the OVERHEAT chain by keeping targets WET (wet units resist heat application more). |
| **Aeromancer** | UPDRAFT zones grant WEIGHTLESS — WEIGHTLESS units float above ground terrain effects, including the DoT and temperature status-application of Thermal Gradient Zone ground tiles. Units in UPDRAFT crossing the midline of the Thermal Gradient Zone at floating height do not trigger THERMAL SHOCK (they are not in contact with the ground-level temperature states). This directly counters the Thermal Gradient Zone's primary THERMAL SHOCK-forcing mechanic. |
| **Chronomancer** | REWIND on a THERMALLY SHOCKED ally resets their status state (removes OVERHEATED, OVERCOOLED, and the STUN from THERMAL SHOCK). The Chronomancer can undo THERMAL SHOCK's effects on an ally before the follow-up SHATTER fires. Additionally, TIME_SLOW on the Thermomancer pauses the cooldown timers on HEAT_RESIDUE and COLD_RESIDUE terrain, effectively maintaining them longer — but this is a minor interaction. The critical counter is REWIND as a THERMAL SHOCK insurance: the Chronomancer player identifies when THERMAL SHOCK is about to chain into a kill and Rewinds the target out of the sequence. |

---

*End of Thermomancer design document.*

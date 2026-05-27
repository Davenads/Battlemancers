# Psychomancer — Full Design Document

---

## 1. Tactical Identity

The Psychomancer is the game's most asymmetric Mancer — a mind-control and morale-disruption specialist whose effectiveness swings wildly depending on who it faces. Against the right warband, it is the most disruptive Mancer in the roster: a Charmed Hydromancer that uses Flood Zone on its own allies, a Panicked unit breaking formation at the worst possible moment, a CONFUSED Electromancer chain-stunning its own chaff screen. Against the wrong warband — specifically, a Gilded Throne force protected by Iron Discipline — a Psychomancer loses most of its toolkit against non-Mancer units and must pivot entirely to a different gameplan. This matchup dependency is intentional: the Psychomancer is the game's hardest Mancer to play well precisely because its power varies more than any other.

Playing the Psychomancer well requires warband-read skill above all else. The moment the opponent reveals their faction, the Psychomancer's plan changes. Against Verdant Pact or Ashen Covenant units (no Iron Discipline), the Psychomancer has full access to Charm, Panic, and Confusion across the entire enemy warband. Against Gilded Throne, those tools work only on enemy Mancers (who are never protected by Iron Discipline — it is a faction trait for Chaff and Ranged, not for Mancers of any faction). The Psychomancer player who enters a Gilded Throne matchup expecting to PANIC the Spearmen screen will lose; the one who immediately pivots to Mancer-targeting with CONFUSED, CHARMED, and stun-hybrid debuffs has a fighting chance.

**Primary win condition:** The Psychomancer wins by subverting the opponent's action economy. A CHARMED Mancer spends its activation on behalf of the Psychomancer's player. A PANICKED unit may move randomly into hazardous terrain or out of a defensive position. A CONFUSED Mancer with 6 AP spent on randomized spell casts is a self-destructing resource. If these effects apply to the opponent's highest-AP units, the Psychomancer has effectively stolen activations — which in the blind simultaneous turn system is the most powerful disruption available.

**Core weakness:** The Psychomancer's morale-based toolkit (Panic, Charm) is specifically countered by one faction. Against a Gilded Throne opponent, the Psychomancer cannot Panic or Charm Chaff or Ranged units (Iron Discipline immunity). This leaves it relying on Confusion and Silence (which work regardless of Iron Discipline), targeting exclusively enemy Mancers with morale tools (Mancers are not protected by Iron Discipline), and supplementing with direct morale damage to attrition the opponent's will to engage. In a Gilded Throne matchup, the Psychomancer functions but at reduced effectiveness — see Section 3 (Gilded Throne Counter) for the adapted gameplan.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 85 | Glass-cannon support tier; the Psychomancer cannot afford to take direct hits |
| **Move Range** | 4 tiles per activation | Above-average; needs to reposition to maintain range on shifting targets |
| **Base Armor** | 1 | Minimal physical mitigation; survives on range and disruption, not durability |
| **Spell Range** | 6 tiles (base) | Each spell lists its own range; this is the fallback reference |
| **AP per Activation** | 6 | Standard for all Mancers; movement costs 1 AP per tile |
| **Element** | Psychic | All base spells deal Psychic damage or apply mind-state statuses; some spells have LOS-independent targeting (per spell-system.md) |

**AP budget example:** With 6 AP, the Psychomancer can move 2 tiles (2 AP), apply CONFUSED (3 AP) to a priority Mancer target, and spend 1 AP applying a free MORALE_DAMAGE pulse from Psychic Pressure (1 AP), or move 3 tiles and use CHARM (4 AP) on a high-value target.

---

## 3. Base Spell Kit

The Psychomancer's four base spells are designed to cover distinct combat functions:
- **Mind Spike** — reliable single-target damage + MORALE_DAMAGE (the non-Iron-Discipline-countered damage type)
- **Confusion** — primary non-morale control; randomizes targeting without triggering Iron Discipline immunity
- **Phantom Dread** — AoE morale damage + PANICKED application (full value vs non-Throne; reduced vs Throne)
- **Charm** — highest-value single-target control; temporarily seizes enemy unit control

---

### Gilded Throne Counter — Explicit Matchup Documentation

**Iron Discipline:** Gilded Throne Chaff (Conscript Spearmen, Iron Vanguard) and Ranged units (Crossbow Corps, Siege Arbalests) are immune to `PANICKED` and `CHARMED`. Morale-based debuffs (`MORALE_DAMAGE`, `PANICKED`) have reduced duration on Throne units. This is a permanent, passive, unbreakable immunity — the Psychomancer cannot bypass Iron Discipline through upgrades, spell combinations, or any other mechanism.

**Practical impact against full Gilded Throne warband:**
- Phantom Dread (PANICKED application): cannot PANIC Throne non-Mancer units; MORALE_DAMAGE component deals morale damage but reduced duration means faster recovery. The AoE damage component (from Mind Spike fired into Phantom Dread) still applies. Phantom Dread loses ~60% of its value.
- Charm: cannot CHARM Throne non-Mancer units. Charm is useless against the infantry screen; only viable target is an enemy Mancer (who is never Iron Discipline protected).
- Mind Spike: MORALE_DAMAGE applies to Throne units at reduced duration. Still deals direct Psychic damage (not morale-based — direct damage ignores Iron Discipline). Full damage output retained; MORALE_DAMAGE component diminished.
- Confusion: CONFUSED is NOT a morale-based debuff — it randomizes targeting mechanically, not through willpower or fear. Iron Discipline does not protect against CONFUSED. Confusion is the Psychomancer's primary non-morale tool and retains full effectiveness against Gilded Throne units.
- Silence: SILENCED is not a morale-based debuff either. Iron Discipline does not protect against SILENCED.

**Psychomancer vs Gilded Throne — Adapted Gameplan:**

1. **Abandon Charm on Chaff/Ranged.** Redirect all Charm casts exclusively to enemy Mancers. An enemy Mancer (Electromancer, Pyromancer, Hydromancer — whatever the opponent brought) is NEVER Iron Discipline protected. Charm on a Hydromancer is still one of the most devastating single-turn plays in the game.

2. **Pivot Phantom Dread to pure AoE damage tool.** Against Throne, Phantom Dread's PANICKED application is wasted on non-Mancers. Use it for the direct Psychic AoE damage component alone when the AP cost is worth it; otherwise, deprioritize.

3. **Maximize Confusion and Silence.** These are the Psychomancer's Iron Discipline-immune tools. CONFUSED Crossbow Corps fire randomly — potentially at their own allies. SILENCED Iron Vanguard cannot use their Shield Wall ability (which requires communication — a passive ability, but one that may rely on formation-active behavior, pending implementation ruling). Against Gilded Throne, the Psychomancer becomes a Confusion-primary, Mancer-Charm-secondary threat.

4. **Mind Spike for sustained Mancer pressure.** Mind Spike's direct Psychic damage and MORALE_DAMAGE still apply to enemy Mancers in full (Mancers are never Iron Discipline protected). Target enemy Mancers with Mind Spike to stack MORALE_DAMAGE toward PANICKED threshold, where even a Mancer's morale can break.

5. **Accept diminished non-Mancer control output.** Against Gilded Throne, the Psychomancer's effective toolkit vs non-Mancer units is: Confusion, direct Psychic damage from Mind Spike, and the Silence status. This is narrower than the full toolkit but not non-functional.

**Verdant Pact synergy (opposite case):**
Against Verdant Pact, the Psychomancer has no Iron Discipline to worry about. Thornback Sentinels can be Charmed and used against their own formation. Panicked Rootwardens may abandon their entrench positions and move randomly, breaking the Terrain Bond regen structure the opponent has built. Glade Archers who are CONFUSED fire POISONED arrows at random targets — including their own units. The Psychomancer against Verdant Pact has full value across its entire toolkit and is among the most impactful Mancer choices possible.

---

### Spell 1: Mind Spike

| Field | Value |
|---|---|
| **Name** | Mind Spike |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (no LOS required — psychic projection; per spell-system.md: some Psychomancer spells are LOS-independent) |
| **Range** | 6 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 14 (Psychic damage — not morale-based; bypasses Iron Discipline damage immunity aspects; deals direct HP damage) |
| **Element** | Psychic |
| **Effects Applied** | Deals 14 Psychic damage. Applies 10 `MORALE_DAMAGE` (contributes toward PANICKED threshold at 0 morale from 100 pool). MORALE_DAMAGE is the primary mechanic driving toward PANICKED — at 0 morale, a unit becomes PANICKED (random movement and attack). Against Gilded Throne Chaff/Ranged: MORALE_DAMAGE has reduced duration (recovers faster); the 14 Psychic direct damage still applies fully. |
| **Terrain Interaction** | Mind Spike has no LOS requirement — it can be cast through walls, STEAM_CLOUD, OVERGROWTH, and terrain features without penalty. This is the Psychomancer's primary way to reach targets hidden behind cover. Against a unit on ILLUMINATED_GROUND: the psychic interference is amplified — MORALE_DAMAGE increased to 15 (light reveals the target's vulnerabilities; mind-effects amplified by illumination). |
| **Special Interactions** | Mind Spike stacks MORALE_DAMAGE across multiple casts. At 2 AP with no cooldown, the Psychomancer can fire Mind Spike three times in a single activation (if not moving: 2 + 2 + 2 = 6 AP) for 30 MORALE_DAMAGE total on one target, or spread across multiple targets. Against a Mancer (100-pt base morale pool): three Mind Spikes = 30 MORALE_DAMAGE from 100 — not PANICKED yet, but significant pressure. Against a Mancer already at 70 morale from prior damage, three Mind Spikes brings them to 40 — approaching the danger threshold. |

**Temperature Effects:** **0 temperature** (pure psychic damage — psychic energy is thermally neutral and generates no heat on impact).

**Design note:** Mind Spike is the Psychomancer's workhorse in both the normal toolkit and the Iron Discipline countergame. Because it deals direct Psychic damage (not Iron Discipline immune) AND MORALE_DAMAGE simultaneously, it forces the Gilded Throne player to accept chip damage that accumulates independently of their faction immunity. The no-LOS targeting makes it the Psychomancer's most reliable spell in vision-obstructed environments.

---

### Spell 2: Confusion

| Field | Value |
|---|---|
| **Name** | Confusion |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Targeted Status — Single target (LOS required) |
| **Range** | 5 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 0 (no direct damage — pure status application) |
| **Element** | Psychic |
| **Effects Applied** | Applies `CONFUSED` (2 turns): target's targeting is randomized within their spell/attack range (per status-effects.md). The target still uses AP and casts spells normally — they just cannot choose their target. Iron Discipline does NOT protect against CONFUSED (it is a mechanical targeting disruption, not a morale effect). CONFUSED interacts with Photomancer purify (Sunburst removes CONFUSED). |
| **Terrain Interaction** | No terrain interaction — CONFUSED is a unit status spell. However, a CONFUSED unit standing on CHARGED terrain who fires a lightning-type spell at a random target may arc into their own allies if adjacent WET units are present (the CONFUSED randomized targeting doesn't account for chain rules, which resolve after the target is selected). |
| **Special Interactions** | CONFUSED + BLINDED (from Photomancer): both statuses apply simultaneously — the worst-case combination for the target. BLINDED reduces targeting range to 1; CONFUSED randomizes targeting within that 1-tile range, meaning the target can only randomly hit adjacent units. Against a ranged Mancer, this effectively neutralizes its entire spell range for the duration. CONFUSED + TIME_SLOW (from Chronomancer): the CONFUSED unit has –2 AP AND randomized targeting — severely limited ability to do anything useful. CHARMED takes priority over CONFUSED: if CHARMED is applied while CONFUSED, the CHARMED player controls the CONFUSED unit's actions — the randomization is suppressed by direct control. SILENCED takes priority over CHARMED (per status-effects.md). |

**Temperature Effects:** **0 temperature direct** (psychic targeting disruption carries no thermal energy). However, a CONFUSED unit cannot intentionally move to cooler or warmer tiles — their movement is semi-random. If an enemy is OVERHEATED and CONFUSED, they cannot choose to run to a FROZEN tile to cool down; they may accidentally move further onto BURNING terrain, worsening their thermal state.

**Design note:** Confusion is the Psychomancer's most reliable disruptive tool against Iron Discipline warbands. Because Iron Discipline does not protect against CONFUSED, Crossbow Corps who are CONFUSED may fire their high-damage armor-piercing shot at a random allied target. A single CONFUSED Siege Arbalest firing into Iron Vanguard is one of the most damaging own-team events possible. The Psychomancer player should apply Confusion to the highest-damage-output unit in the enemy formation — what that unit does to random targets is the entire value of the spell.

---

### Spell 3: Phantom Dread

| Field | Value |
|---|---|
| **Name** | Phantom Dread |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Ground Target — AoE Radial (centered on target point) |
| **Range** | 5 tiles (to center) |
| **AoE Radius** | 2 tiles |
| **Base Damage** | 6 (minor Psychic pressure damage to all units in AoE) |
| **Element** | Psychic |
| **Effects Applied** | Deals 6 Psychic damage to all units in the 2-tile radius. Applies 15 `MORALE_DAMAGE` to all units hit. If a unit's morale reaches 0 after this hit, they immediately become `PANICKED` (random movement + attack; duration 2 turns). PANICKED units with Iron Discipline (Gilded Throne): immunity applies — no PANICKED; MORALE_DAMAGE still applied at reduced duration. PANICKED Verdant Pact or Ashen Covenant units: full PANICKED effect. A PANICKED unit that moves randomly into hazardous terrain (ON_FIRE, CHARGED, edge of elevated terrain) takes the terrain damage and cannot correct their path mid-panic. |
| **Terrain Interaction** | Against ILLUMINATED_GROUND tiles in the AoE: the fear amplifies through illuminated space — MORALE_DAMAGE increased to 20 per unit. Against NECROTIC_ASH: the death energy amplifies psychological dread — MORALE_DAMAGE increased to 20; duration extended by 1 turn (the psychological effect lingers in the presence of death terrain). Against STEAM_CLOUD: psychic projection partially obscured — MORALE_DAMAGE reduced to 10. |
| **Special Interactions** | Phantom Dread triggering PANICKED on multiple units simultaneously creates a "panic cascade" — PANICKED units who move randomly may collide with each other (movement collision damage: 4 HP per unit), creating chain-reaction chaos in a dense formation. This is particularly punishing against Verdant Pact Chaff clusters near natural terrain (Terrain Bond regen positions) and Ashen Covenant Husk screens (where random movement may take Husks off their necrotic terrain regen tiles). |

**Temperature Effects:** **-5 temperature** to all units hit (fear response triggers cold sweats and mild body temperature drop — the dread evokes cold). This mild cooling is rarely enough to push units toward COLD threshold on its own but can combine with Cryomancer spells already lowering a target's temperature.

**Design note:** Phantom Dread is the Psychomancer's area-denial and morale-attrition tool. Against Verdant Pact or Ashen Covenant, it is a powerful crowd-control ability that can PANIC entire formations simultaneously. Against Gilded Throne, it degrades to: 6 Psychic AoE damage + reduced-duration MORALE_DAMAGE that recovers quickly. In that matchup, the Psychomancer should use Phantom Dread only when the 6 direct damage is worth 3 AP — typically not, unless the cluster contains an enemy Mancer (who receives full MORALE_DAMAGE and can PANIC). The 2-turn cooldown makes it a consistent rotation piece, not a one-time bomb.

---

### Spell 4: Charm

| Field | Value |
|---|---|
| **Name** | Charm |
| **AP Cost** | 4 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Targeted Status — Single target (LOS required) |
| **Range** | 5 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 0 (no direct damage) |
| **Element** | Psychic |
| **Effects Applied** | Applies `CHARMED` (1 turn): the target is controlled by the Psychomancer's player for their next activation. The Psychomancer player may use any of the CHARMED unit's abilities or movement. Iron Discipline: CHARMED cannot be applied to Gilded Throne Chaff or Ranged units — immune. CHARMED can always be applied to any Mancer regardless of faction (Mancers are not covered by Iron Discipline). SILENCED takes priority over CHARMED (per status-effects.md): a SILENCED CHARMED unit cannot cast spells, limiting the Psychomancer's control to movement only. |
| **Terrain Interaction** | No terrain interaction at application. The CHARMED unit's actions during the controlled turn interact with terrain normally — a Charmed Hydromancer using Flood Zone generates FLOODED terrain as it normally would, benefiting the Psychomancer's player. |
| **Special Interactions** | Charm is the Psychomancer's highest-risk, highest-reward single action. The 4 AP cost leaves only 2 AP for the Psychomancer's own actions (1 tile movement + 1 AP ability, or 2 tiles movement). The 3-turn cooldown means Charm is available approximately twice per long engagement. Against Gilded Throne: Charm is exclusively used on enemy Mancers. Against Verdant Pact or Ashen Covenant: Charm is most valuable on the enemy's support or control unit (Hydromancer, Chronomancer, or the opposing Psychomancer). Charm on an enemy Necromancer allows the Psychomancer to raise the Necromancer's own undead against it — or more devastatingly, use Necrotic Eruption to destroy the Necromancer's own corpse field. |

**Temperature Effects:** **0 temperature** (Charm itself carries no thermal energy). However, a CHARMED unit is now controlled by the Psychomancer's player — who can choose to move the CHARMED unit onto BURNING tiles (to heat them up) or FROZEN tiles (to cool them down). Charm gives the Psychomancer an indirect temperature manipulation tool: not by changing temperature directly, but by repositioning enemies into thermally hostile terrain.

**Design note:** Charm is the Psychomancer's most telegraphed and most memorable ability. Every opponent who knows the matchup will plan around Charm — keeping Mancers spread, positioning Silenced units between the Psychomancer and key targets, or spending Chronomancer Stasis to protect a critical ally. The 1-turn duration is intentionally brief — a permanently Charmed unit would be unplayable to counteract. The 3-turn cooldown and 4 AP cost make Charm a carefully chosen pivot, not a rotation tool.

---

## 3b. Temperature Interaction Notes

**Psychic energy is thermally neutral.** None of the Psychomancer's base spells generate or remove heat directly. However, mental states interact with the temperature system in indirect and tactically significant ways.

- **CONFUSION + temperature management:** A CONFUSED unit has impaired decision-making and cannot intentionally move to cooler or warmer tiles (their movement is semi-random). If an enemy is OVERHEATED and CONFUSED, they cannot choose to run to a FROZEN tile to cool down — they may accidentally move further onto BURNING terrain, making their thermal situation worse. This is a passive synergy: the Psychomancer denies the opponent's ability to self-regulate temperature.

- **PANIC + OVERHEATED (Panic Attack):** A unit that is both PANICKED and OVERHEATED suffers from "Panic Attack" — they move erratically (existing PANIC behavior) while also losing an additional +5 temperature per turn from stress-induced heat. This is a minor secondary effect but adds flavor and small mechanical pressure against OVERHEATED enemies who the Psychomancer panics.

- **CHARM + temperature:** A CHARMED unit is controlled by the Psychomancer's player. The Psychomancer can choose to move CHARMED units onto BURNING tiles deliberately (to heat them up) or FROZEN tiles (to cool them down). This gives the Psychomancer an unusual indirect temperature manipulation tool — not by changing temperature directly, but by repositioning enemies into thermally hostile terrain.

- **Iron Discipline and temperature:** Gilded Throne units are immune to PANIC and CHARM, but NOT immune to temperature effects. Temperature gives the Psychomancer a secondary pressure tool even against Iron Discipline armies. If the Psychomancer cannot panic or charm the Throne infantry, it can at least displace them with CONFUSION (which Iron Discipline does NOT negate) — preventing them from self-correcting their thermal state.

---

## 4. Terrain Interaction Table

### Psychic Spell Impact on Existing Terrain States

Psychomancer spells are primarily unit-targeted (status applications, direct damage). They interact with terrain through unit positioning and through specific psychic resonance properties.

| Existing Terrain State | Psychomancer Interaction | Result |
|---|---|---|
| **Normal (GROUND)** | No terrain interaction — spells target units | Standard resolution |
| **ON_FIRE** | PANICKED units who move randomly into ON_FIRE terrain: take 5 HP/turn DoT, cannot redirect away | PANICKED movement into fire is compounding punishment; Psychomancer should position Phantom Dread to direct panic-movement toward existing fire zones |
| **CHARGED** | PANICKED or CONFUSED units may randomly move onto CHARGED tiles: arc triggers immediately | Unintended arc triggers on PANICKED units — can damage adjacent allies if the PANICKED unit is WET |
| **ILLUMINATED_GROUND** | MORALE_DAMAGE from Mind Spike and Phantom Dread increased to 15–20 on illuminated tiles | Photomancer + Psychomancer combination; see Combo 3 |
| **NECROTIC_ASH** | Phantom Dread MORALE_DAMAGE increased by 5 on NECROTIC_ASH tiles; PANICKED duration +1 turn | Death energy amplifies psychological dread |
| **STEAM_CLOUD** | Mind Spike retains full effectiveness (no LOS required). Phantom Dread's MORALE_DAMAGE reduced to 10 (psychic projection partially obscured). Charm requires LOS — cannot be cast into STEAM_CLOUD | LOS-independent spells retain value; LOS-dependent spells blocked |
| **FLOODED / WET** | No direct interaction; PANICKED units moving through FLOODED tiles have movement penalty (+1 cost per tile) — this may truncate their random panic movement path | Terrain movement penalties affect PANICKED unit pathing |
| **TOXIC_TERRAIN** | PANICKED units who move onto TOXIC_TERRAIN gain POISONED stacks passively — they are already acting randomly, stacking poison compounds their situation | Compounding debuff against PANICKED units on toxic ground |
| **ICE_TILE / PERMAFROST** | PANICKED units on ICE_TILE have movement randomized further (slip chance) — their panic movement may slide 1 additional tile from the random direction chosen | Ice amplifies panic by making movement even more unpredictable |
| **OVERGROWTH** | Confusion cast into OVERGROWTH-covered tiles: the dense vegetation doesn't block the psychic projection (no LOS required for Mind Spike; LOS required for Charm/Confusion) | Phantom Dread and Confusion require LOS; Mind Spike can pierce OVERGROWTH |
| **ELEVATED** | Standard elevated tile bonus: +1 tile range on all Psychomancer spells. From high ground, Mind Spike reaches 7 tiles without LOS restriction — exceptional position for pressure | Strong positional benefit for a long-range support/control Mancer |

### Terrain States Beneficial to the Psychomancer

| State | Benefit |
|---|---|
| `ON_FIRE` tiles adjacent to enemy formation | PANICKED units from Phantom Dread may randomly move into existing fire zones — the Psychomancer benefits from fire the Pyromancer set without directly needing it |
| `NECROTIC_ASH` | Increases MORALE_DAMAGE from Phantom Dread; extends PANICKED duration |
| `ILLUMINATED_GROUND` | Increases MORALE_DAMAGE from Mind Spike; Charm into an illuminated zone has the CHARMED unit visible to all allies regardless of subsequent terrain |
| `ELEVATED` | +1 range on all spells; Mind Spike at 7 tiles LOS-free is almost unreachable for the opponent to prevent |

### Terrain States Hazardous to the Psychomancer

| State | Hazard |
|---|---|
| `STEAM_CLOUD` | Blocks LOS for Charm and Confusion; reduces Phantom Dread MORALE_DAMAGE. The Psychomancer's LOS-dependent high-value spells are severely limited |
| `ON_FIRE` | 5 HP/turn DoT on 85 HP pool — significant exposure risk |
| `CHARGED` | Arc trigger on 85 HP / 1 armor is potentially lethal combined with any other damage source |
| `TOXIC_TERRAIN` | POISONED stacks accumulate quickly on the Psychomancer's glass-cannon HP pool |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

#### Variant A: Mass Confusion (replaces Confusion) — +25 pts

**Description:** Replaces single-target Confusion with an AoE version. Mass Confusion targets a point within 4 tiles and applies CONFUSED to all units within a 2-tile radius. AP Cost: 5 AP. Cooldown: 3 turns. CONFUSED duration: 1 turn per unit (reduced from 2 — the psychic energy is spread across multiple targets, weakening per-unit effect). Does not apply to Gilded Throne Chaff/Ranged? **No — Iron Discipline does NOT protect against CONFUSED. Mass Confusion applies fully to ALL Gilded Throne units in the AoE, including Chaff and Ranged.**

**Trade-off:** Significantly higher AP cost (nearly a full turn) and 3-turn cooldown in exchange for AoE application. Mass Confusion is the Psychomancer's most impactful ability against Gilded Throne — it disrupts the entire unit category that Iron Discipline otherwise protects via non-morale means. A Mass Confusion applied to a formation of Iron Vanguard + Siege Arbalest will have every unit randomly targeting for 1 turn — potentially dealing massive friendly-fire damage.

**Synergy note:** Mass Confusion + Electromancer chain in the same turn resolution: CONFUSED units may randomly fire into WET allies, triggering chains that the Electromancer exploits — the Psychomancer and Electromancer accidentally (or deliberately) chain off of psychically-triggered friendly fire. High chaos ceiling.

#### Variant B: Domination (replaces Charm) — +25 pts

**Description:** Replaces Charm with a 2-turn duration version. Domination costs 5 AP, has a 4-turn cooldown, and applies CHARMED for 2 turns instead of 1. The Psychomancer's player controls the CHARMED unit for 2 consecutive turns. Iron Discipline still prevents Domination from applying to Gilded Throne Chaff/Ranged.

**Trade-off:** The extra turn of control doubles the window in which the Psychomancer can use the Charmed unit's abilities — meaning a Charmed Hydromancer can Flood Zone one turn and Mending Current the next, all benefiting the Psychomancer's team. The 5 AP cost means the Psychomancer's own activation is completely consumed by the cast plus 1 tile of movement maximum. Most powerful against enemy Mancers with long-cooldown abilities; less impactful against low-AP chaff units.

---

### Passive Traits

#### Passive A: Psychic Drain — +20 pts

**Description:** Whenever the Psychomancer deals MORALE_DAMAGE to any unit, it recovers 3 HP (psychic energy feeds back into the caster). At maximum Mind Spike usage (three casts per activation = 30 MORALE_DAMAGE applied), this generates 9 HP per activation from drain alone. Against a warband where MORALE_DAMAGE applies frequently (Verdant Pact or Ashen Covenant), Psychic Drain provides meaningful sustain on an 85 HP Mancer.

**Synergy note:** Psychic Drain scales with how often the Psychomancer can apply MORALE_DAMAGE. Against Gilded Throne (reduced-duration MORALE_DAMAGE), the drain still triggers on application — the reduced duration doesn't reduce the drain trigger. In a Gilded Throne matchup, Psychic Drain provides 3 HP per Mind Spike regardless, as long as the morale damage component is applied.

#### Passive B: Iron Will Breaker — +25 pts

**Description:** The Psychomancer's morale-based effects (MORALE_DAMAGE, PANICKED) against Gilded Throne units are no longer reduced-duration — they apply at the same duration as against other factions. Additionally, Gilded Throne Chaff and Ranged units are no longer immune to PANICKED — Iron Discipline is partially overcome by the Psychomancer's enhanced psychic potency. **CHARMED immunity on Gilded Throne units is NOT affected — they remain Charm-immune.** This upgrade specifically addresses the Gilded Throne counter by converting PANICKED immunity and MORALE_DAMAGE reduction into normal effectiveness. Charm immunity is intentionally preserved as the remaining faction advantage.

**Design note:** Iron Will Breaker is the most faction-specific upgrade in the game. Against Verdant Pact or Ashen Covenant, it provides zero value (those factions have no Iron Discipline to break). It is exclusively useful when the opponent is running Gilded Throne. A Psychomancer player who consistently faces Gilded Throne opponents should take Iron Will Breaker; one who rarely faces Throne should skip it. This intentional faction-specific utility is a deliberate warband-building decision point.

#### Passive C: Morale Cascade — +20 pts

**Description:** When a unit becomes PANICKED (from any source — Phantom Dread, accumulated MORALE_DAMAGE reaching 0), all non-Iron-Discipline units within 2 tiles of the newly PANICKED unit take 10 MORALE_DAMAGE immediately (a morale cascade from watching an ally break). This can chain — if that 10 MORALE_DAMAGE pushes another unit to 0 morale, they also PANIC, potentially triggering another cascade for their neighbors.

**Design note:** Morale Cascade is the Psychomancer's "win more" passive — it amplifies already-successful PANIC applications into a spreading event. In dense formations (Verdant Pact Chaff clusters, Ashen Covenant Husk screens) where units are within 2 tiles of each other, a single PANIC trigger from Phantom Dread can cascade into 3-4 simultaneous PANICKED units, collapsing the formation in one activation. Against Gilded Throne (Iron Discipline), Cascade does not spread to Chaff or Ranged — it only spreads to other PANIC-eligible units, meaning if the opponent's entire warband is Gilded Throne Chaff/Ranged + 1 Mancer, the Cascade can only propagate to the enemy Mancer (if within range).

#### Passive D: Psychic Barrier — +15 pts

**Description:** The Psychomancer generates a passive mental ward that reduces MORALE_DAMAGE dealt to allied Mancers within 3 tiles by 5 per application. Additionally, the Psychomancer is immune to CONFUSED status (its own enhanced mental architecture prevents targeting randomization). It is not immune to CHARMED or PANICKED — those remain applicable.

**Design note:** Psychic Barrier provides both a defensive ward for allied Mancers and targeted protection for the Psychomancer itself against the one counter-status it is most vulnerable to (being CONFUSED would randomize its own Charm/Confusion targeting, potentially debuffing allies). Most valuable in mirror-matchup scenarios where the opponent also runs a Psychomancer.

---

### Stat Enhancements

#### Enhancement A: Hardened Psyche (+20 HP) — +15 pts

**Description:** Max HP increases from 85 to 105. Brings the Psychomancer to near-average survivability and allows it to absorb one additional hit before entering critical territory. Most valuable in warbands where the Psychomancer operates at shorter range (e.g., taking Radiant Presence analog builds where it pushes closer for MORALE_DAMAGE pulses) or where allied screening is unreliable.

#### Enhancement B: Swift Mind (+1 Move Range) — +10 pts

**Description:** Move Range increases from 4 to 5 tiles per activation. The extra movement allows the Psychomancer to maintain optimal range on a moving frontline — retreating from melee pressure while keeping Charm and Confusion within effective range. Most valuable on open-field maps where the Psychomancer cannot rely on terrain to shelter its position.

---

### Signature Ability

#### Signature: Mass Hysteria — +40 pts

| Field | Value |
|---|---|
| **Name** | Mass Hysteria |
| **AP Cost** | 6 AP (entire activation) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Self-centered AoE — no targeting cursor; Psychomancer is origin |
| **Range** | N/A (self-centered) |
| **AoE Radius** | 5 tiles (all enemy units within 5 tiles of the Psychomancer) |
| **Base Damage** | 0 (psychic effect; no direct HP damage) |
| **Element** | Psychic |
| **Effects Applied** | All enemy units within 5 tiles simultaneously receive: (1) 30 MORALE_DAMAGE; (2) `CONFUSED` for 2 turns. Against Gilded Throne Chaff/Ranged (Iron Discipline): the PANICKED immunity applies if MORALE_DAMAGE from this + prior damage reaches 0 morale, but CONFUSED still applies in full. Against Mancers (no Iron Discipline): full effect. If any unit is already at low morale (30 or below) when Mass Hysteria hits, that unit immediately becomes PANICKED (the 30 MORALE_DAMAGE pushes them to 0 or below). The CONFUSED effect means every affected unit then acts randomly for 2 turns — a formation-wide disruption event. |
| **Special Interactions** | Mass Hysteria's 5-tile radius requires the Psychomancer to be within 5 tiles of the enemies it wants to affect — closer to the frontline than its standard operating position. This is the primary risk: using Mass Hysteria exposes the Psychomancer to retaliation. A Psychomancer at 85 HP (or 105 with enhancement) within 5 tiles of an intact enemy formation is in significant danger. Mass Hysteria is most safely used when allied units have reduced the enemy formation's offensive capacity — or when the Psychomancer itself used Stasis (if paired with Chronomancer) to protect itself on a prior turn before closing the distance. Against Gilded Throne: CONFUSED applies fully to all units. PANICKED applies fully to Mancers; Chaff and Ranged become PANICKED only if morale reaches 0, which may not happen (Iron Discipline + reduced MORALE_DAMAGE duration means Throne units recover morale faster). In a Gilded Throne matchup, Mass Hysteria is primarily a Mass Confusion delivery device (2-turn CONFUSED across a 5-tile radius) — still powerful, but not the complete formation-collapse it represents against other factions. |

**Design note:** Mass Hysteria is the Psychomancer's "this is what we were always building toward" ability and one of the most visually spectacular events in the game — an entire enemy formation simultaneously staggering, firing randomly, and breaking formation. The 5-tile proximity requirement is a design constraint that prevents it from being a safe long-range nuke. Its matchup-dependent ceiling (devastating against Verdant Pact or Ashen Covenant; powerful but narrower against Gilded Throne) reflects the Psychomancer's fundamental design identity: a Mancer whose peak output requires reading the opponent and adapting. At 40 upgrade points, it is a warband commitment — a player taking Mass Hysteria is declaring that they plan to close the distance for the decisive moment.

---

## 6. Faction Synergy

### Best Pairing: The Verdant Pact

The Verdant Pact is the Psychomancer's strongest faction pairing because Verdant Pact units have no Iron Discipline and no natural morale resistance. Every morale-based tool in the Psychomancer's kit applies at full value and full duration.

**Verdant Pact-specific interactions:**

| Mechanism | Effect |
|---|---|
| Charmed Thornback Sentinel | The Sentinel's on-death Thorn Patch is placed wherever the Charmed Psychomancer directs it — the Psychomancer places a hazard tile exactly where it needs one |
| Panicked Glade Archers | PANICKED Archers fire POISONED arrows at random targets including allies — friendly-fire POISONED stacks compound with Toximancer or other allies' poison synergies |
| Panicked Rootwardens | PANICKED Rootwardens abandon their entrench positions — the Terrain Bond regeneration that entrenched Rootwardens were providing to adjacent units is lost, breaking the Pact's sustain cycle |
| Charmed Wyrmwood Strider | The Strider applies 2 POISONED stacks on hit; a Charmed Strider fires at the opponent's own units — each hit stacks two POISONED on enemy targets, giving the Psychomancer's Pyromancer or Toximancer ally free combo fuel |
| Morale Cascade passive | Verdant Pact lacks any morale protection; a single Panicked Thornback Sentinel adjacent to 3 others triggers Cascade — potentially collapsing the forward screen in one Phantom Dread cast |

**Summary:** Against Verdant Pact, the Psychomancer is the highest-value Mancer choice in the game if the opponent's list is Chaff-heavy. A Verdant Pact player facing a Psychomancer must spread formations (preventing Cascade and Phantom Dread clustering), prioritize eliminating the Psychomancer early, or bring their own Psychomancer to counter.

### The Gilded Throne — Structural Counter

Documented in detail in Section 3 above. Summary: Gilded Throne Chaff and Ranged are immune to PANICKED and CHARMED; morale debuffs have reduced duration. The Psychomancer's adapted gameplan pivots to Confusion (fully effective), Silence (fully effective), Mancer-exclusive Charm (Mancers have no Iron Discipline), and Mind Spike direct damage (Psychic damage is not morale-based — applies fully).

Iron Will Breaker upgrade partially addresses this by removing PANICKED immunity and MORALE_DAMAGE reduction. With Iron Will Breaker, the Psychomancer recovers most of its toolkit against Throne — but Charm on Chaff/Ranged remains permanently unavailable (this is the intentional remaining faction advantage).

**Psychomancer + Gilded Throne (same warband):** If the Psychomancer is IN a Gilded Throne warband, it benefits from Iron Discipline on its allied Chaff and Ranged — they cannot be Charmed or Panicked by the enemy Psychomancer. This creates a one-sided morale disruption dynamic: the Psychomancer can disrupt enemy Mancers with Charm and Confusion, while its own infantry screen is immune to morale-based retaliation from an opposing Psychomancer.

### Ashen Covenant — Strong but Specialized

Ashen Covenant units (Grave Husks, Abyssal Revenants, Wailing Shades, Void Wraiths) have the Deathless Ranks faction trait: no morale, cannot Flee, cannot be Charmed, cannot be Panicked. This means the Psychomancer cannot use morale tools on its own Covenant allies — which doesn't matter for the Psychomancer's offense, but it means the Psychomancer's Mass Hysteria signature cannot accidentally PANIC its own Covenant Chaff units in the 5-tile AoE (they are immune).

Against enemy units in an Ashen Covenant warband run by the opponent: if the opponent runs a Necromancer in Ashen Covenant, the Psychomancer can Charm the Necromancer and use its Raise Shambler ability on the Psychomancer's behalf — reanimating enemy corpses as allied undead. This is one of the most disruptive Charm uses in the game. The Psychomancer can also PANIC enemy Verdant Pact or other non-Throne chaff in a mixed opponent scenario.

**Wailing Shades interaction:** Wailing Shades emit a Silence aura. If an enemy Wailing Shade is within 1 tile of the Psychomancer, the Psychomancer is effectively SILENCED from that tile's Silence aura — a critical threat to a Mancer whose entire kit is spellcasting. The Psychomancer must maintain distance from Wailing Shades or eliminate them before closing range.

---

## 7. Combo Chains

### Combo 1: Psychomancer + Electromancer — "Mind Shock"

**Mancers involved:** Psychomancer + Electromancer

**Sequence:**
1. Psychomancer applies CONFUSION to an enemy Mancer (or PANICKED from Phantom Dread pushes a unit into random movement).
2. CONFUSED unit fires a random spell — if it is a lightning-type spell (Electromancer CONFUSED), it may chain into adjacent WET allies.
3. If the CONFUSED unit is NOT the Electromancer: Psychomancer applies CONFUSION, CONFUSED enemy fires randomly away from intent; Electromancer then fires at the repositioned/misfired enemy with a chain arc.
4. Combined effect: the CONFUSED unit wastes its activation; the Electromancer fires uncontested at vulnerable targets the CONFUSED unit failed to threaten.

**Why this works:** The blind simultaneous turn system assumes opponents act optimally. CONFUSION breaks that assumption — a CONFUSED unit's activation is partially or fully wasted, creating an "extra activation's worth" of AP advantage for the Psychomancer's team. The Electromancer follows up in the same turn with its uncontested chain.

---

### Combo 2: Psychomancer + Necromancer — "Death Mark Hijack"

**Mancers involved:** Psychomancer + Necromancer

**Sequence:**
1. Necromancer applies DEATH_MARK to a high-value enemy unit.
2. Psychomancer CHARMS the DEATH_MARK target's most adjacent ally.
3. CHARMED allied unit (now controlled by Psychomancer's player) is directed to attack or move toward the DEATH_MARK target.
4. If the CHARMED unit has an ability that can kill the DEATH_MARK target — or pushes it into hazardous terrain — the DEATH_MARK explosion triggers; the CHARMED unit may be caught in the blast radius.
5. Alternatively: CHARMED unit retreats away from the DEATH_MARK target deliberately, isolating the marked unit from healing support that would have been provided.

**Why this works:** DEATH_MARK's explosion only triggers on the target's death. The Psychomancer accelerates that death by co-opting an enemy ally to do the killing, or by removing the healing support that would have kept the target alive. Either way, the Necromancer's DEATH_MARK investment pays off faster.

---

### Combo 3: Psychomancer + Photomancer — "Sight and Mind"

**Mancers involved:** Psychomancer + Photomancer

**Sequence:**
1. Photomancer casts Solar Flash into an enemy formation: all units BLINDED (2 turns) + ILLUMINATED_GROUND terrain created.
2. Psychomancer casts Phantom Dread into the same zone: MORALE_DAMAGE increased by 5 per unit on ILLUMINATED_GROUND tiles (per terrain interaction table); 6 Psychic damage to all.
3. BLINDED + MORALE_DAMAGE compounding: units who cannot see (BLINDED) and have taken psychic fear damage are closer to PANICKED threshold with reduced ability to respond.
4. If Morale Cascade passive (Psychomancer): PANICKED trigger from Phantom Dread cascades to adjacent units already weakened by accumulated MORALE_DAMAGE.
5. Photomancer fires Illuminate on surviving enemy Mancer: all allied damage amplified +20%. CHARMED unit (from Charm on the next activation) benefits from ILLUMINATED marking too — attacks while Charmed are amplified.

**Why this works:** Photomancer provides the BLINDED (vision denial) and ILLUMINATED_GROUND (amplified MORALE_DAMAGE) setup; Psychomancer provides the MORALE_DAMAGE accumulation and PANIC trigger. BLINDED enemies cannot effectively retreat or coordinate their response; the Morale Cascade, if triggered, collapses the formation simultaneously.

---

### Combo 4: Psychomancer + Hydromancer — "Turned Tide"

**Mancers involved:** Psychomancer + Hydromancer (Psychomancer CHARMS an enemy Hydromancer)

**Sequence:**
1. Psychomancer identifies an enemy Hydromancer as the primary CHARM target.
2. Psychomancer uses CHARM (4 AP) on the enemy Hydromancer.
3. CHARMED Hydromancer is controlled by the Psychomancer's player for 1 turn. The Psychomancer's player directs it to: (a) cast Flood Zone over the enemy's own Chaff formation (creating FLOODED terrain that the Psychomancer's allied Electromancer can chain through), or (b) cast Tidal Surge pushing enemy units into each other or into hazardous terrain, or (c) cast Mending Current healing the Psychomancer's own injured Mancer.
4. The enemy Hydromancer, for one turn, has performed entirely on behalf of the opponent it was fighting.

**Why this works:** The Hydromancer is the most "value-per-charm" target because all of its spells are positive or disruptive at significant effect. A Flood Zone cast over the enemy's own formation is a devastating self-inflicted setup. This combo is the reason the Psychomancer is the most feared support Mancer against Hydromancer-centric teams.

**Counter note:** Chronomancer Rewind applied to the Charmed Hydromancer (before it acts in the resolution phase — requiring precise timing read) can strip the CHARMED status and reposition it to safety. See Chronomancer doc, Rewind interaction with CHARMED.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Psychomancer

| Mancer | Counter Mechanism |
|---|---|
| **Chronomancer** | Rewind on a CHARMED unit strips the Charm immediately (Rewind removes all statuses). A Chronomancer that correctly predicts a Charm attempt and Rewinds the targeted ally removes the Psychomancer's highest-AP spell mid-resolution. Additionally, STASIS applied to a key unit the Psychomancer is targeting prevents CHARM from being applied (STASIS makes the unit invulnerable and untargetable — Charm cannot attach). |
| **Sonimancer** | SILENCED Psychomancer cannot cast any spells. Given the Psychomancer has zero physical attack capability and 85 HP, a SILENCED Psychomancer is completely inactive for 1 turn. Sonimancer SILENCE is the single hardest counter in the game. |
| **Photomancer** | Sunburst removes CONFUSED from units in its AoE (per status-effects.md: Photomancer purify removes CONFUSED). The Psychomancer's primary Iron Discipline-immune control tool is counterable by the Photomancer. Additionally, Photomancer Sunburst's BLINDED effect applies to Psychomancer units — a BLINDED Psychomancer cannot use LOS-required Charm or Confusion. |
| **Opposing Psychomancer (mirror)** | CHARM on the Psychomancer redirects its own Charm ability at allied units; CONFUSION on the Psychomancer means it applies Phantom Dread or Mind Spike to random targets. The Psychomancer is highly vulnerable to its own toolkit. Psychic Barrier passive helps against this. |

### Terrain Compositions That Shut Psychomancer Down

| Terrain Setup | Why It's Punishing |
|---|---|
| **STEAM_CLOUD coverage** | Charm and Confusion require LOS. A board filled with STEAM_CLOUD denies the Psychomancer its two highest-impact abilities — only Mind Spike (no LOS) retains full function. |
| **OBSIDIAN walls between Psychomancer and targets** | LOS required for Charm, Confusion, and Phantom Dread. OBSIDIAN walls can completely separate the Psychomancer from its target list, particularly on maps with natural chokepoints where an enemy Geomancer has hardened the approach. |
| **ELEVATED terrain clusters far from Psychomancer** | High-ground enemies beyond 6-tile range cannot be targeted by Charm (5-tile range) or Confusion (5-tile range). Mind Spike (6 tiles, no LOS) remains the only tool. |

### Warband Compositions That Prey on Psychomancer

| Warband Type | Exploitation |
|---|---|
| **Gilded Throne + Sonimancer** | Iron Discipline denies morale toolkit vs Chaff/Ranged; Sonimancer SILENCES the Psychomancer before it can Charm the enemy Mancer. The Psychomancer's gameplan is essentially shut down: cannot PANIC Throne infantry, cannot Charm them, and cannot cast when SILENCED. |
| **Heavy melee pressure + Faunamancer companion swarm** | The Psychomancer at 85 HP cannot survive sustained melee contact. Fast-closing companion swarms from Faunamancer overwhelm its 4-tile movement escape capability before it can apply Charm from range. |
| **Chronomancer + any Mancer** | The Chronomancer's Rewind on any CHARMED ally strips the Charm mid-turn (if the Chronomancer activates before the CHARMED unit resolves). A warband that includes a Chronomancer has effectively purchased Charm immunity for one ally per Rewind cooldown cycle — and STASIS immunity for one ally per Stasis cooldown. Against Chronomancer support, the Psychomancer's Charm success rate drops significantly. |

---

---

## 9. Augmentation Spell

### Will Surge

**AP Cost:** 3 | **Range:** 3 tiles | **Targeting:** Single allied unit | **Cooldown:** 3 turns

The Psychomancer floods an allied unit's psyche with weaponized aggression, stripping emotional inhibitors and driving them into a controlled frenzy.

**Effects (2 turns):**
- Ally enters BERSERK -- +2 damage on all attacks, immune to PANIC and CHARM
- While BERSERK, the ally cannot voluntarily disengage from melee -- if adjacent to an enemy, they must use an attack action before spending movement to move away (costs 1 AP to forcibly disengage instead)
- Gilded Throne faction: Iron Discipline overrides the disengagement lock -- the +2 damage and PANIC/CHARM immunity apply, but the ally retains free movement (military discipline controls the surge)
- Verdant Pact and Ashen Covenant receive the full effect including the engagement lock

**Tactical intent:** Power with behavioral cost. The +2 damage makes the unit a genuine threat; PANIC/CHARM immunity counters the Psychomancer's own enemy toolkit in mirror scenarios. The engagement lock is the meaningful constraint -- a berserked unit adjacent to multiple enemies must fight through them, not disengage. This creates positioning risk opponents can exploit by surrounding the berserked unit. The Gilded Throne interaction is deliberately faction-rewarding: Iron Discipline partially neutralizes a mental override, fitting the lore and making faction composition matter in augmentation planning.

**Notable interactions:** Will Surge + Faunamancer Pack Bond on the same unit: the BERSERK unit triggers a reaction attack every time the bonded companion attacks, and the engagement lock guarantees the berserked unit is reliably in melee range for Pack Bond procs every turn. Psychomancer applying Will Surge to an Osteomancer Bone Lattice recipient: the BERSERK +2 damage also applies to the bone shard burst if the lattice shatters, making the explosion more dangerous to nearby enemies.

*End of Psychomancer design document.*

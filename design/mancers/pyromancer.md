# Pyromancer — Full Design Document

> **Template note:** This document is the canonical format for all 19 Mancer design docs.
> Every section and field defined here must be reproduced (with Mancer-appropriate content)
> in all subsequent Mancer documents. Do not omit sections.

---

## 1. Tactical Identity

The Pyromancer is an aggressive area-denial specialist who wins by turning the battlefield itself into a weapon. Where other Mancers deal damage directly, the Pyromancer's deepest value lies in the fire it leaves behind — terrain that punishes repositioning, forces enemies into narrow corridors, and converts into catastrophic combo material for allied Mancers. A well-played Pyromancer is less a sniper and more an arsonist: patient enough to seed a threat, aggressive enough to exploit the panic it causes. Every turn it is alive without spending AP is a missed opportunity; the Pyromancer is meant to be activated constantly, applying pressure and layering terrain states even when no kill is available.

Playing the Pyromancer well means reading four or five turns ahead. Its spells are not individually devastating — Ember Shot at 18 base damage is survivable — but the board states they leave accumulate into conditions that become lethal when a second Mancer detonates them. The Pyromancer is the opener in most combos it participates in, not the closer. This demands a specific mindset: success is not measured in kills scored but in how effectively the Pyromancer converted neutral terrain into a burning gauntlet the opponent had to navigate. Its core weakness is survivability — at 85 HP with minimal armor, a single poor positioning decision exposes it to focus fire it cannot weather. Managing range and keeping blocking units between the Pyromancer and melee threats is not optional; it is the skill floor.

**Primary win condition:** The Pyromancer wins by converting large sections of the battlefield to ON_FIRE terrain and positioning such that every approach path for the enemy runs through burning tiles. Combined with an allied Electromancer, Toximancer, or Geomancer, the Pyromancer's terrain becomes a detonation field for cascading high-damage combinations. Victory for a Pyromancer player looks like: two or more ON_FIRE zones active, at least one enemy unit carrying the BURNING status, and a second Mancer positioned to exploit the elemental state on the same turn.

**Core weakness:** Wet terrain is the Pyromancer's nemesis in two directions — it converts its own fire into Steam Clouds (which blind but do not sustain the ON_FIRE state), and Hydromancer flood coverage eliminates the terrain investment the Pyromancer spent AP building. An enemy Hydromancer is the most efficient counter in the game. Additionally, the Pyromancer has no mobility tools, no self-sustain, and no way to escape a tight corridor. Cryomancers that CHILL and FREEZE the Pyromancer before it acts can shut down an entire activation entirely. The Pyromancer also struggles against flying units or any unit with WEIGHTLESS status, as those units ignore ground terrain effects and sidestep the Pyromancer's primary method of passive damage application.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 85 | Glass-cannon tier; lowest HP among offensive Mancers |
| **Move Range** | 3 tiles per activation | Short legs; must be protected or positioned preemptively |
| **Base Armor** | 1 | Nearly no physical mitigation; survives on positioning |
| **Spell Range** | 5 tiles (base) | Each spell lists its own range; this is the fallback reference |
| **AP per Activation** | 6 | Standard for all Mancers; movement costs 1 AP per tile |
| **Element** | Fire | All base spells deal Fire damage and apply fire-element terrain/status interactions |

**AP budget example:** With 6 AP, a Pyromancer can move 3 tiles (3 AP) and cast a Standard spell (3 AP), or move 2 tiles and cast a Standard spell plus a Quick spell (2 + 3 + 1 = 6), or spend the full 6 AP on an Ultimate cast without moving.

---

## 3. Base Spell Kit

The Pyromancer's four base spells are designed to cover distinct combat functions:
- **Ember Shot** — repeatable single-target pressure + BURNING application
- **Scorched Earth** — area denial and terrain state creation
- **Conflagration Wave** — terrain-manipulation cone that spreads existing fire
- **Pillar of Flame** — high-cost single-target burst with major environmental consequence

---

### Spell 1: Ember Shot

| Field | Value |
|---|---|
| **Name** | Ember Shot |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (projectile — travels in a line; can hit intervening units) |
| **Range** | 6 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 18 |
| **Element** | Fire |
| **Effects Applied** | Applies `BURNING` status to hit unit (5 HP/turn, refreshes on re-apply, no stack). Tile beneath target unit becomes `ON_FIRE` (persists; spreads each turn to adjacent FLAMMABLE/GROUND tiles). |
| **Temperature Effects** | **+15 temperature** to the hit unit. From neutral (0), a single Ember Shot reaches WARM (+15), making the target take +10% fire damage from subsequent fire spells. Two consecutive Ember Shots in one activation push a neutral unit to +30 (top of WARM), one more to HOT. |
| **Special Interactions** | See terrain interaction table in Section 4. |

**Design note:** This is the Pyromancer's workhorse. Low cost and no cooldown mean it can be used twice in a single activation alongside a single-tile movement (2 AP move + 2 AP shot + 2 AP shot = 6 AP). The projectile hitting intervening units rewards positioning — enemies hiding behind a closer unit can still receive splash fire if the Pyromancer aims through them. The primary value is twofold: direct BURNING DoT applied to the target, and the `ON_FIRE` tile state left underfoot, which persists and grows independently of whether the target remains in place.

**Spell answers YES to (design rule check):**
1. Applies terrain state (ON_FIRE) — YES
2. Applies unit status (BURNING) — YES
3. Synergizes with Electromancer (fire on WET = steam; BURNING + lightning = firestorm) — YES
4. Skill expression: aim through intervening units, tile selection for fire spread direction — YES

---

### Spell 2: Scorched Earth

| Field | Value |
|---|---|
| **Name** | Scorched Earth |
| **AP Cost** | 3 AP |
| **Cooldown** | 1 turn (skip 1 turn before reuse) |
| **Targeting Type** | Ground Target — AoE Radial |
| **Range** | 5 tiles (to center of AoE) |
| **AoE Radius** | 2 tiles |
| **Base Damage** | 12 (to all units in AoE at cast) |
| **Element** | Fire |
| **Effects Applied** | Every tile in the 2-tile radius becomes `ON_FIRE`. All units on affected tiles take 12 Fire damage and receive `BURNING` (5 HP/turn). Existing `FLOODED` tiles within the radius convert to `STEAM_CLOUD` instead of `ON_FIRE`. |
| **Temperature Effects** | **+20 temperature** to each unit directly hit at cast time. Additionally, any unit that remains standing on an `ON_FIRE` tile created by Scorched Earth receives **+10 temperature per turn** from the burning terrain passive (this stacks with the terrain-passive rule in the core system). A unit standing on Scorched Earth fire for two turns gains +20 on top of the initial +20 hit — reaching +40 HOT from just this one spell across two turns. |
| **Special Interactions** | See terrain interaction table in Section 4. If any tile in the AoE is already `ON_FIRE`, that tile's spread rate increases (fans into adjacent tile this turn instead of waiting). If the AoE center is `TOXIC_TERRAIN`, all tiles in the AoE instead become `TOXIC_FIRE` hybrid state — applies both `BURNING` and 1 POISONED stack to units each turn until extinguished. |

**Design note:** Scorched Earth is the Pyromancer's area-denial cornerstone. Its primary use is not the 12-point hit on enemies in range (though that is useful) but the 5-tile radius of `ON_FIRE` terrain it stamps onto the board. This forces enemy movement, blocks natural pathing routes, and creates a platform for follow-up Electromancer or Toximancer interactions. The conversion of FLOODED tiles to STEAM_CLOUD is both a strength (blinds enemies inside the cloud) and a limitation (the Pyromancer cannot set water on fire — it becomes steam instead, removing the ON_FIRE state from those tiles). Players using Pyromancer must be aware that a Hydromancer opponent can counter Scorched Earth by pre-flooding the targeted zone.

**Spell answers YES to (design rule check):**
1. Applies terrain state (ON_FIRE × radius) — YES
2. Creates terrain effects that persist and spread — YES
3. Synergizes with Toximancer, Electromancer, Geomancer — YES
4. Skill expression: placement to avoid FLOODED tiles; predict spread direction — YES

---

### Spell 3: Conflagration Wave

| Field | Value |
|---|---|
| **Name** | Conflagration Wave |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Cone (directional; originates from Pyromancer's tile, extends 4 tiles in chosen direction at 90-degree spread) |
| **Range** | 4 tiles (length of cone) |
| **AoE Radius** | Cone width: 1 tile at origin, 3 tiles wide at maximum range |
| **Base Damage** | 20 to units caught in cone |
| **Element** | Fire |
| **Effects Applied** | Deals 20 Fire damage to all units in the cone. Applies `BURNING` to all hit units. Existing `ON_FIRE` tiles within the cone are fanned — each `ON_FIRE` tile immediately spreads to one additional adjacent tile in the cone direction (the wave "pushes" fire forward). Non-fire tiles struck become `ON_FIRE`. |
| **Temperature Effects** | **+25 temperature** to all units caught in the cone. This is the Pyromancer's broadest single-cast temperature application — hitting three or more clustered enemies with Conflagration Wave pushes all of them toward HOT simultaneously. A neutral unit at 0 reaches +25 WARM from one cast; a unit already WARM from an Ember Shot reaches +40 HOT (SLOWED). |
| **Special Interactions** | If any tile in the cone is `ICE_TILE` or `PERMAFROST`, the fire-ice reaction causes a burst of steam at that location: creates a 1-tile `STEAM_CLOUD` centered on the ice tile (blind + heat damage 2 turns) and the `ICE_TILE` is converted to `FLOODED` (melt residue). Units on the melted ice tile take an additional 8 damage from the thermal shock. If the cone passes through `OVERGROWTH`, it converts those tiles to `ON_FIRE` instantly (organic matter burns faster — no spread timer needed). |

**Design note:** Conflagration Wave is the Pyromancer's terrain-sculpting tool. Its core function is directional fire propagation — an existing cluster of `ON_FIRE` tiles, struck by the Wave, extends and reshapes in a chosen direction. This gives the Pyromancer agency over where the fire moves, not just how much burns. The cone shape punishes clustered enemies, while the fire-spreading mechanic rewards preplanning — ideally, the Pyromancer seeds an `ON_FIRE` zone with Scorched Earth and then fans it toward the enemy with Conflagration Wave on a subsequent activation. The 2-turn cooldown prevents spamming this "fire director" tool and forces the Pyromancer to commit to a spread direction rather than resteer every turn.

**Spell answers YES to (design rule check):**
1. Exploits existing terrain state (fans ON_FIRE tiles forward) — YES
2. Creates new terrain state (ON_FIRE in cone) — YES
3. Moves the effective location of a threat (fire propagation = repositioning a hazard) — YES
4. Synergizes with Geomancer (obsidian creation from fanned fire + earth tile), Aeromancer (wind spell can further fan flames) — YES
5. Skill expression: cone direction selection, fire-fan geometry prediction — YES

---

### Spell 4: Pillar of Flame

| Field | Value |
|---|---|
| **Name** | Pillar of Flame |
| **AP Cost** | 5 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Ground Target — AoE Radial |
| **Range** | 4 tiles (to center) |
| **AoE Radius** | 1 tile (tight impact zone) |
| **Base Damage** | 55 (primary target tile unit); 25 (adjacent tile units in 1-tile radius) |
| **Element** | Fire |
| **Effects Applied** | Primary tile: unit takes 55 Fire damage; `BURNING` applied; tile becomes `ON_FIRE` permanently until extinguished (no natural expiry — this fire does not go out on its own). Adjacent tiles: 25 Fire damage; `BURNING` applied; tiles become `ON_FIRE` (standard expiry behavior). The primary tile is also physically deformed: if `GROUND`, it becomes a `PIT` (scorched into the earth). If already `PIT`, it becomes `VOID`. |
| **Temperature Effects** | **+35 temperature** to the primary tile unit — the most powerful single-cast heating in the base roster. A unit at neutral (0) is pushed to +35 HOT (SLOWED) in one hit. A unit already WARM (+15 from a prior Ember Shot) reaches +50 HOT. A unit already HOT (+35) reaches +70 OVERHEATED, triggering the BURNING DoT status. **Intentional setup path:** Ember Shot (+15) → Ember Shot (+15 → +30) → Pillar of Flame (+35 → +65) pushes a neutral enemy to OVERHEATED in a single activation if all three spells land: 3 AP (two Ember Shots) + 5 AP (Pillar) = 8 AP total, requiring 2 activations but creating a guaranteed OVERHEATED state with full BURNING DoT. Alternatively, a target pre-pushed to +60 HOT (by a Thermomancer's Heat Lance or prior Ember Shot sequence) reaches +95 OVERHEATED from a single Pillar of Flame. |
| **Special Interactions** | Against a `FROZEN` or `ICE_TILE` primary target: the flash-melt releases a steam burst in 2-tile radius — all units in range receive `BLINDED` (2 turns) in addition to normal damage. Against a `WET` primary tile: massive `STEAM_CLOUD` (3-tile radius, 3-turn duration) rather than persistent ON_FIRE — the water absorbs the fire but the steam cloud is proportionally larger. Against a `CHARGED` primary tile: the arc explosion is triggered simultaneously with the Pillar — units on and adjacent to the primary tile also take 20 Lightning damage and are knocked back 1 tile away from the center. Against a unit with `POISONED` status on the primary tile: Toxic combustion triggers — in addition to normal damage, a 2-tile `TOXIC_FIRE` cloud (ON_FIRE + POISONED effect combined) erupts centered on the impact, lasting 3 turns. |

**Design note:** Pillar of Flame is the Pyromancer's answer to "what happens when you need a kill confirmed right now." At 5 AP, it consumes almost the entire activation budget (leaving only 1 AP for a single tile of movement before casting). Its 55 base damage is sufficient to threaten most un-upgraded Mancers from full HP when combo interactions are factored in, and the permanent ON_FIRE tile at the impact point is a lasting tactical consequence that outlives the spell itself. The terrain deformation — creating a PIT at the impact point — is a meaningful side effect: it creates a new fall hazard for push abilities, eliminates the tile as a cover position, and can strand units whose pathing assumed that tile was passable. The tight 4-tile range forces the Pyromancer to close distance before using it, creating meaningful risk-reward tension. This is not a safe long-range finisher; it demands commitment.

**Spell answers YES to (design rule check):**
1. Applies terrain state (permanent ON_FIRE; terrain deformation to PIT) — YES
2. Creates terrain feature destruction — YES
3. Applies unit status (BURNING) — YES
4. Synergizes with every elemental Mancer via the interaction variants — YES
5. Skill expression: close-range commitment, terrain deformation placement, status combo exploitation — YES

---

## 3b. Temperature Interaction Notes

The Pyromancer is the roster's primary HEATING Mancer. Every fire spell pushes the target's temperature significantly positive, and the temperature system creates a layered damage escalation path that rewards sequenced play.

**Temperature thresholds recap (for Pyromancer context):**
- WARM (+1 to +30): fire spells deal +10% damage vs this unit — Ember Shot into a pre-warmed enemy hits for ~20 instead of 18
- HOT (+31 to +60): SLOWED (move range –1) — enemy repositioning is impaired; the Pyromancer can dictate range
- OVERHEATED (≥ +61): BURNING DoT (5 dmg/turn) — passive damage even when the Pyromancer spends AP elsewhere
- BURNING tile passive: +10 temperature/turn to any unit standing on an ON_FIRE tile (terrain passive, end of turn)

**Pyromancer + Thermomancer combo (temperature amplification):**
The Thermomancer's Heat Lance applies +35 temperature (OVERHEAT status) independently of fire damage. When the Thermomancer pre-heats a target to +35 or higher (HOT), the Pyromancer's subsequent fire spells immediately benefit from the +10% fire damage bonus of the WARM→HOT range, plus the SLOWED penalty limits the enemy's ability to retreat. If the Thermomancer pushes a target past +61 OVERHEATED before the Pyromancer activates, the BURNING DoT is already ticking — the Pyromancer's follow-up spell then lands on a unit taking passive fire damage each turn, maximizing attrition. The ideal sequence: Thermomancer Heat Lance twice in one activation (+70 temperature, OVERHEATED, BURNING DoT) → Pyromancer Pillar of Flame next turn (55 base × no multiplier from OVERHEATED, but target is already at risk; DoT compounds).

**Benefiting from pre-warmed targets:**
Any ally that has applied heat (Thermomancer Heat Lance, standing on burning terrain) before the Pyromancer acts grants the Pyromancer +10% fire damage if the target is WARM. Track enemies standing on ON_FIRE tiles — every turn they stand there, their temperature rises by +10. A unit standing on Scorched Earth for two turns before the Pyromancer's next activation is at +40+ HOT, taking +10% fire damage from any fire spell. Ember Shot against a HOT target deals ~20 HP (18 × 1.10) — not a large multiplier individually, but it compounds with the SLOWED penalty the HOT status imposes, keeping the enemy in range for follow-up casts.

**The +60 HOT setup / Pillar of Flame OVERHEATED finish:**
A deliberate single-Pyromancer escalation path: push a target to exactly +60 HOT (within the HOT threshold, SLOWED but not yet OVERHEATED) using Ember Shots and terrain passive. On the following activation, cast Pillar of Flame (+35 temperature) → the target goes from +60 to +95, crossing the +61 OVERHEATED threshold and immediately triggering the BURNING DoT. The target simultaneously receives Pillar of Flame's 55 base damage, is BURNING from the spell effect, and is OVERHEATED (an additional 5 HP/turn from the temperature status). This creates a multi-source DoT stack: Pillar's BURNING (5/turn) and OVERHEATED temperature DoT (5/turn) both tick simultaneously for 10 HP/turn combined until the temperature decays below +61.

**WET tile interaction and temperature:**
When a fire spell hits a WET tile, it produces STEAM_CLOUD — the water partially neutralizes the heat. Net temperature effect: the +15 temperature from Ember Shot is reduced to approximately **+5** when hitting a WET target (the moisture absorbs 10 points of thermal energy in the conversion to steam). This means Hydromancer opponents can partially negate the Pyromancer's heating by pre-wetting their units. The Pyromancer must dry the terrain or target un-wetted units for full temperature escalation. Full formula for WET tile fire hits: base temperature change minus 10 (wet resistance) = net temperature change.

---

## 4. Terrain Interaction Table

### Fire Spell Impact on Existing Terrain States

The following describes specifically what happens when any Pyromancer spell strikes a tile in the listed terrain state. All Pyromancer spells are Fire element; these interactions apply universally across the kit unless a spell's individual entry overrides them.

| Existing Terrain State | What Happens When Fire Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **Normal (GROUND)** | Fire ignites the tile normally | `ON_FIRE` | Takes spell damage + `BURNING` | None; ON_FIRE spreads each turn. **Temperature note:** Full temperature value applies (no reduction). Unit gains +10 temperature/turn passively while standing on the resulting ON_FIRE tile. |
| **WET** | Fire meets moisture — water flash-evaporates | `STEAM_CLOUD` (2 turns; replaces WET state; tile is no longer ON_FIRE) | Takes spell damage + `BLINDED` (2 turns) from steam; no BURNING applied | STEAM_CLOUD occupies the tile; blocks vision in and out; adjacent units take 3 heat dmg/turn while cloud is active. **Temperature note:** WET tile reduces the fire spell's temperature impact — net temp change is base temperature value minus 10 (wet resistance). Example: Ember Shot on WET tile = +15 − 10 from wet resistance = **+5 net temperature** to the target. |
| **ON_FIRE (already burning)** | Fire feeds the existing blaze | `ON_FIRE` (spread rate doubled this turn — spreads to 2 adjacent tiles instead of 1) | Takes spell damage + BURNING refreshed | Intensification: if fire has already been burning for 2+ turns, the tile additionally scorches — adjacent units take 3 heat dmg from proximity even without entering |
| **FLOODED** | Large water mass — steam explosion | `STEAM_CLOUD` (3-tile radius; 2 turns) | Takes reduced spell damage (–30% — water partially absorbs) + `BLINDED` (2 turns) | FLOODED state is removed; three-tile STEAM_CLOUD forms centered on impact; large blind zone created |
| **ICE_TILE** | Thermal shock — ice melts explosively | `FLOODED` (melt residue) + 1-tile `STEAM_CLOUD` burst | Takes spell damage + 8 bonus thermal-shock damage + `BLINDED` (1 turn) | ICE_TILE is completely removed; tile left as FLOODED (wet residue); any unit that was FROZEN on this tile is immediately un-frozen (melt) |
| **FROZEN (unit status, any tile)** | Fire melts the frozen unit | Tile becomes `WET` (residue) | FROZEN status removed; unit takes spell damage + `WET` effect (conductive) | No BURNING applied — fire and ice cancel; unit is now WET and vulnerable to chain-lightning follow-up |
| **TOXIC_TERRAIN** | Fire ignites the poison | `TOXIC_FIRE` hybrid (ON_FIRE + POISONED-on-contact; 3 turns) | Takes spell damage + `BURNING` + 1 stack `POISONED` | `TOXIC_FIRE` persists as a combined state; units on the tile each turn take 5 HP fire DoT and gain 1 POISONED stack; extinguishing the fire (water) removes the fire component but leaves TOXIC_TERRAIN residue |
| **CHARGED** | Electrical and fire energy discharge together | Tile cleared of CHARGED state; becomes `ON_FIRE` | Takes spell damage + 20 Lightning damage from arc explosion + pushed back 1 tile from center | Arc explosion is 1-tile AoE — adjacent units also take the 20 Lightning damage; this is a powerful unintentional combo, but also can harm allies |
| **MUD** | Steam and heat dry the mud | Tile becomes `GROUND` (dried out; MUD removed) | Takes spell damage; no BURNING (wet surface, fire extinguished by moisture) | Mud is cleared, removing the movement penalty — this can help ally movement through previously muddy areas |
| **OVERGROWTH** | Organic matter ignites instantly | `ON_FIRE` (fast-spreading: spreads 2 tiles this turn instead of 1) | Takes spell damage + `BURNING` | OVERGROWTH is destroyed and replaced by ON_FIRE; Floramancer structures on OVERGROWTH tiles are destroyed by this interaction |
| **OBSIDIAN** | Fire cannot alter obsidian | `OBSIDIAN` (unchanged) | Takes spell damage; no terrain state change | Obsidian is the one terrain the Pyromancer cannot burn or interact with meaningfully; a reminder that Geomancer-hardened terrain counters fire zone creation |

### Terrain States Beneficial to the Pyromancer

| State | Benefit |
|---|---|
| `ON_FIRE` tiles adjacent to Pyromancer | Pyromancer takes no HP damage from ON_FIRE terrain (Fire immunity to its own element); adjacent fire also serves as a deterrent against melee approach — enemies must take DoT to reach it |
| `FLAMMABLE` tiles (dried grass, wood) | Pyromancer's fire spreads faster — Conflagration Wave on FLAMMABLE terrain creates cascading fire that the Pyromancer does not need to manually maintain |
| `ELEVATED` tiles | Standard elevated tile bonus: +1 to all spell ranges. A Pyromancer on high ground extends Ember Shot to 7 tiles, keeping it at maximum distance from melee threats |
| `TOXIC_TERRAIN` (Toximancer-created) | Pyromancer's spells convert TOXIC_TERRAIN to TOXIC_FIRE — one of the highest-value terrain interactions in its kit; Toximancer pre-seeding the board massively amplifies Pyromancer output |

### Terrain States Hazardous to the Pyromancer

| State | Hazard |
|---|---|
| `FLOODED` / `WET` | Extinguishes or converts ON_FIRE to STEAM_CLOUD, undermining terrain investment; Pyromancer cannot maintain fire zones in wet areas |
| `ICE_TILE` / `PERMAFROST` | Movement penalty (slip chance); a CHILL or FROZEN applied to the Pyromancer locks it down completely for a turn, wasting its activation |
| `CHARGED` | Walking onto a CHARGED tile stuns or damages the Pyromancer like any other unit; with only 85 HP and no armor, Charged tile traps are particularly dangerous |
| `TOXIC_TERRAIN` | Pyromancer is not immune to ground poison; entering TOXIC_TERRAIN grants POISONED stacks that compound its already-low HP survivability |
| `STEAM_CLOUD` | Ironic: the Pyromancer creates STEAM_CLOUDS when hitting WET tiles, but walking into one blinds itself too — BLINDED reduces targeting range to 1 tile, making most of the Pyromancer's spell kit unusable |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost. A fully upgraded Pyromancer with all options would be prohibitively expensive by design — players choose a focused upgrade path, not the whole menu.

---

### Spell Variants

Spell variants replace a base spell entirely with a more powerful or specialized version.

#### Variant A: Magma Shot (replaces Ember Shot) — +20 pts

**Description:** Ember Shot's projectile is superheated into a slow-moving magma bolt. The bolt travels in an arc (not blocked by intervening units — passes over them). On impact, the primary tile becomes `LAVA` (1-tile LAVA patch; 20 HP/turn to adjacent units; lasts 3 turns before cooling to OBSIDIAN). The unit hit takes 24 damage (up from 18) and BURNING status. The LAVA tile then cools naturally into OBSIDIAN after 3 turns — leaving permanent impassable terrain at the impact site.

**Trade-off:** Higher damage, permanent terrain consequence (OBSIDIAN creation), arc trajectory ignores cover. Cost: the 0-cooldown repeatable nature of Ember Shot is gone — Magma Shot has a 1-turn cooldown, reducing spammability. Best for: map control, permanent obstacle creation, punishing clustered enemies near the impact zone during the LAVA phase.

**Synergy note:** LAVA tiles created by Magma Shot are the only way (other than Thermomancer effects) to create LAVA terrain mid-game. Aeromancer's wind abilities interact with LAVA adjacency by creating toxic-heat updrafts (heat column — 1-tile AoE, minor fire dmg, BLINDED — centered on the LAVA tile when wind crosses it).

#### Variant B: Volcanic Surge (replaces Pillar of Flame) — +25 pts

**Description:** Pillar of Flame is replaced by Volcanic Surge — a two-stage eruption. Stage 1 (cast): identical to Pillar of Flame (55 damage primary, 25 AoE, PIT creation). Stage 2 (triggers at end of the following turn, automatically): the PIT created by Stage 1 erupts — a secondary 2-tile radius AoE burst deals 30 Fire damage to all units in range, applies BURNING, and converts all affected tiles to ON_FIRE. This eruption cannot be stopped once the initial cast is made.

**Trade-off:** Massive total damage output (55 + 30 AoE across two turns) and the delayed secondary eruption punishes enemies who move into the PIT area (thinking it safe after the initial blast). Cooldown increases to 4 turns (from 3). The delayed eruption is telegraphed — a visual indicator (glowing pit, rumbling effect) warns that a secondary detonation is coming, rewarding defensive play from the opponent.

**Synergy note:** The PIT created becomes a LAVA eruption point for 1 tile radius on Stage 2 — specifically, units on the PIT tile during Stage 2 are treated as standing in LAVA for that turn (full 20 HP damage + BURNING regardless of Stage 2 AoE damage). Gravimancer who pushes enemies into the PIT after Stage 1 and before Stage 2 is an intentional combo.

---

### Passive Traits

Passive traits are always-on abilities added to the Pyromancer.

#### Passive A: Fireskin — +20 pts

**Description:** The Pyromancer absorbs ambient heat from any ON_FIRE tile within 2 tiles. At the start of each of its activations, it recovers 4 HP for each ON_FIRE tile adjacent to it (up to a maximum of 3 tiles, capping at 12 HP/activation). Additionally, the Pyromancer is immune to all Fire terrain damage — it can stand in ON_FIRE tiles, walk through LAVA borders, and ignore BURNING applied by enemy sources (Thermomancer fire, Toximancer toxic fire, reflected fire from interactions). Enemy BURNING is the only fire effect it remains immune to; enemy Fire spells still deal full damage.

**Trade-off:** The HP regen requires the Pyromancer to stay near its own fire terrain, which also keeps it close to the front lines — a positioning risk for a 85-HP glass cannon. Fireskin also enables a playstyle where the Pyromancer walks through its own fire zones freely, opening movement paths that enemies cannot follow.

**Synergy note:** Fireskin combined with Conflagration Wave creates a mobile fire anchor: the Pyromancer fans fire forward, then follows it, recovering HP from the burning tiles it just walked past. With Scorched Earth establishing a zone and Fireskin providing sustain, the Pyromancer becomes meaningfully harder to grind down through attrition.

#### Passive B: Backdraft — +25 pts

**Description:** Whenever a Fire terrain state (ON_FIRE, STEAM_CLOUD, TOXIC_FIRE, LAVA) is removed or extinguished within 5 tiles of the Pyromancer — by any means, including enemy Hydromancer or Ice spells — the extinguishing causes a backdraft explosion. The backdraft deals 15 Fire damage in a 1-tile radius centered on the extinguished tile and applies BURNING to units in the burst. This is a passive, automatic reaction — the Pyromancer does not spend AP.

**Trade-off:** Backdraft punishes the opponent for countering the Pyromancer's terrain work. An enemy Hydromancer flooding out the Pyromancer's fire zone will trigger multiple backdraft bursts. However, the Pyromancer must be within 5 tiles for the passive to trigger — it cannot be a safe rear-line piece if it wants Backdraft to activate. Also, Backdraft triggers even if the Pyromancer itself extinguishes fire (e.g., via a MUD interaction from an ally Geomancer), so map awareness is required to avoid friendly-fire surprises.

**Synergy note:** Backdraft pairs aggressively with Geomancer's Earth-on-fire smother ability (Smother = extinguish + rubble). If Geomancer smothers a Pyromancer fire zone (intentionally, as a combo setup), each tile extinguished triggers a Backdraft burst — converting Geomancer's "cleanup" action into a distributed AoE fire damage pass across the former fire zone.

---

### Stat Enhancements

Stat enhancements improve a specific base stat meaningfully.

#### Stat A: Hardened Resolve (+15 HP) — +10 pts

**Description:** Max HP increases from 85 to 100. This is the minimum investment to bring the Pyromancer out of pure glass-cannon territory and into something capable of absorbing one additional hit before reaching critical HP thresholds. No other stat changes.

**Design note:** At 100 HP, the Pyromancer survives a Pillar of Flame from an enemy Pyromancer if it has its back to the wall (55 primary + BURNING tick = 60 before next activation). At 85 HP, it does not. This upgrade is almost mandatory in matchups where the enemy has high-burst fire options.

#### Stat B: Scorched Stride (+1 Move Range) — +15 pts

**Description:** Move Range increases from 3 to 4 tiles per activation. The Pyromancer gains 1 additional AP-free movement tile — effectively, movement costs remain 1 AP per tile, but the Pyromancer has access to 4 tiles of repositioning within its 6 AP budget instead of 3. In practice: can move 4 tiles and cast a 2-AP Quick spell (4 + 2 = 6 AP), or move 4 tiles and cast a 1-AP Quick — wait, this stat functions as a flat move-range reference for patrol/hold behaviors and for abilities that reference Move Range, not as free AP. AP costs for movement remain unchanged.

**Correction:** Scorched Stride adds 1 tile to the Pyromancer's movement range — this means it can move up to 4 tiles in a single action by spending 4 AP, or use its 6 AP to move 4 tiles + cast a 2-AP spell (exact budget). Primarily valuable in maps where the Pyromancer needs to reposition behind terrain frequently and the 3-tile base range leaves it a turn short of a safe fallback position.

**Design note:** The Pyromancer with 3-tile move range is particularly constrained on open maps with no natural cover. Scorched Stride addresses this without changing its AP economy or spell power — it is a survivability upgrade in disguise, giving the Pyromancer the reach to retreat behind a wall or ally screen when threatened.

#### Stat C: Pyromantic Focus (+1 Spell Range) — +15 pts

**Description:** All Pyromancer spell ranges increase by 1 tile. Ember Shot: 6 → 7. Scorched Earth center: 5 → 6. Conflagration Wave: 4 → 5. Pillar of Flame: 4 → 5. Combined with Elevated tile bonus (+1 range when on high ground), a Pyromancer on elevated terrain reaches 8 tiles with Ember Shot — effectively screen-spanning on smaller maps.

**Design note:** This upgrade shifts the Pyromancer from "close-range arsonist" to "mid-range fire artillery." The extra range allows it to seed ON_FIRE terrain before enemies can close to dangerous proximity, giving it more time to build the fire economy before it becomes a priority target. Most valuable in open-field maps where terrain provides no natural range-extending elevation.

---

### Signature Ability

The Signature Ability is unlocked as an additional spell slot — it does not replace any base spell. It represents the Pyromancer's tactical ceiling.

#### Signature: World Conflagration — +40 pts

| Field | Value |
|---|---|
| **Name** | World Conflagration |
| **AP Cost** | 6 AP (entire activation) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Self-centered AoE — no targeting cursor; the Pyromancer is the origin point |
| **Range** | N/A (self-centered) |
| **AoE Radius** | 5 tiles in all directions from the Pyromancer's current position |
| **Base Damage** | 0 (no direct damage — this is a terrain transformation ability, not an attack) |
| **Element** | Fire |
| **Effects Applied** | Every tile within 5 tiles of the Pyromancer that is currently `ON_FIRE` undergoes simultaneous Intensification: each ON_FIRE tile immediately spreads fire to ALL adjacent GROUND/FLAMMABLE/OVERGROWTH tiles within the 5-tile radius (not just one per turn — all of them, instantly). Every unit within the 5-tile radius standing on or adjacent to any ON_FIRE tile takes 30 Fire damage and BURNING immediately. The Pyromancer itself takes 15 Fire damage (even with Fireskin — World Conflagration's backlash bypasses Fire immunity). |
| **Special Interactions** | World Conflagration does NOT create new ON_FIRE tiles — it only spreads and intensifies existing ones. If there are zero ON_FIRE tiles within 5 tiles when cast, the ability has no effect but still consumes 6 AP and triggers cooldown. This is a skill trap — do not use on a bare board. Against WET/FLOODED tiles in the radius: the spread-fire that would hit those tiles creates STEAM_CLOUD at each, generating a massive multi-tile blind zone simultaneously with the fire spread. Against TOXIC_TERRAIN tiles in the radius: each TOXIC_TERRAIN tile ignited becomes TOXIC_FIRE hybrid simultaneously. |

**Design note:** World Conflagration is the Pyromancer's "this is what we've been building toward" ability. It is useless without prior terrain investment — a Pyromancer who casts it on a clean board wasted a full activation and a 5-turn cooldown. But a Pyromancer who seeded the map with 4–5 ON_FIRE tiles across two or three prior activations and then triggers World Conflagration can convert half the battlefield to ON_FIRE in a single instant, potentially triggering 20–30 damage on every enemy unit simultaneously while also creating the multi-tile STEAM_CLOUD that blinds them for the follow-up turn.

This ability rewards patient, setup-oriented play. The 40-point upgrade cost is the highest gate in the Pyromancer's kit by design — it is a warband build-around, not a casual pickup.

**Synergy note:** World Conflagration is the single highest-value Pyromancer action for setting up Electromancer, Geomancer, and Toximancer follow-up combos. On a board where the Pyromancer has seeded fire, Hydromancer has partially flooded sections, and Electromancer is positioned: World Conflagration spreads fire into WET tiles (creating STEAM_CLOUD grid-wide), and the Electromancer's next activation finds a board of WET and BURNING units — a perfect chain stun + firestorm setup across the entire engagement zone simultaneously.

---

## 6. Faction Synergy

### Best Faction: The Ashen Covenant

The Ashen Covenant is the Pyromancer's natural home. Grave Husks — the Covenant's chaff — are unique in the game in that they **regenerate 1 HP per turn while standing in ON_FIRE terrain**. This means that every tile the Pyromancer converts to burning ground is not just a denial zone but an active healing resource for allied Husks. The Pyromancer creates fire; the Husks advance through it and sustain themselves.

Tactically, this enables an "advance through fire" strategy: the Pyromancer sets the ground ahead of the Husk line ablaze with Scorched Earth or Conflagration Wave, then the Husks march through it — enemies cannot march through the same burning approach without taking 5 HP/turn DoT with no regen offset. The Husks do. This creates a fundamental asymmetry in what both sides can sustain.

Additionally, Wailing Shades (Covenant ranged) are phase-through units whose projectiles ignore physical cover. This matters for Pyromancer because the fire zones and STEAM_CLOUDs it creates can obstruct ally sight lines — but Wailing Shades ignore cover restrictions entirely, meaning they can fire from inside a STEAM_CLOUD the Pyromancer created without LOS penalty.

The Deathless Ranks trait (no morale loss) also matters when the Pyromancer's fire spreads toward friendly units on a bad turn — Covenant chaff units do not break from the psychological pressure of walking through their own side's fire zone.

**Specific upgrade combinations that peak with Ashen Covenant:**
- **Fireskin + Grave Husks:** The Pyromancer can stand in its own fire zones to recover HP while Husks also recover nearby. The fire zone becomes a healing biome for the Covenant force, flipping the danger calculus entirely.
- **World Conflagration (Signature) + Grave Husk screen:** The Pyromancer drops World Conflagration in the middle of a board it has been burning all game. Every Husk in the 5-tile radius is standing in ON_FIRE tiles and begins healing. Every enemy in the same zone takes 30 damage and BURNING. The net HP swing on a tight engagement can be 40+ HP per unit in the Covenant's favor.

### Worst Faction: The Verdant Pact

The Verdant Pact is the most synergetically hostile faction for the Pyromancer. The problem is structural:

1. **Terrain Bond** benefits from natural terrain (forest, earth, mud, vine). The Pyromancer destroys natural terrain by burning it — OVERGROWTH becomes ON_FIRE (removing the Terrain Bond tile), forest biome tiles burn away. Every fire zone the Pyromancer creates destroys the tiles that give the Verdant Pact its passive benefits.

2. **Glade Archers** apply POISONED on hit. POISONED + Pyromancer fire = TOXIC_FIRE (a powerful combo). However, this forces Pact units to remain adjacent to active fire zones to continuously poison targets for the Pyromancer to ignite — and Pact units, unlike Covenant Husks, do not regenerate in fire. They take the same 5 HP/turn as everyone else.

3. **Thornback Sentinels** leave Thorn Patches on death. Those patches are destroyed instantly by any fire spell that hits the tile, denying the Pact the death-consequence utility they rely on for board control.

4. **Rootwardens** (T2 Chaff) can generate natural tiles beneath themselves — but a Pyromancer's Conflagration Wave will immediately burn those organic tiles away, turning the Rootwarden's entrench action into wasted AP.

The one functional synergy is the Toximancer flavor: Glade Archer poison stacks on enemies, then the Pyromancer ignites them for TOXIC_FIRE. This is a genuine combo but requires more coordination than the Covenant's passive regen advantage provides — and it still leaves the Pact's terrain economy in conflict with the Pyromancer's fire agenda.

### Specific Unit Interactions

| Unit | Interaction with Pyromancer |
|---|---|
| **Grave Husks (Ashen Covenant Chaff)** | Regenerate 1 HP/turn in ON_FIRE terrain — uniquely benefit from Pyromancer fire zones. Can advance through burning ground without net HP loss (at low enough damage thresholds), creating a front line the enemy cannot match. |
| **Conscript Spearmen (Gilded Throne Chaff)** | Neutral. Take fire DoT normally. Iron Discipline's morale immunity does not help against BURNING. Functional screen units but no special fire synergy. Best used as a shield wall to protect the Pyromancer from melee reach rather than advancing into fire zones. |
| **Thornback Sentinels (Verdant Pact Chaff)** | Thorn Patches on death are destroyed by fire — the Pyromancer cancels its own allied unit's death effect. Anti-synergy. |
| **Wailing Shades (Ashen Covenant Ranged)** | Phase through physical cover; fire through STEAM_CLOUD without penalty. Enables sustained ranged fire even when the Pyromancer has created vision-blocking steam zones. Natural partner unit. |
| **Crossbow Corps (Gilded Throne Ranged)** | High single-shot, alternating fire. On turns the Crossbow unit is not firing, it can reposition through Pyromancer fire zones (taking DoT) — sub-optimal. Work best when kept off the fire grid entirely. |
| **Glade Archers (Verdant Pact Ranged)** | Apply POISONED on hit. Any POISONED enemy is a TOXIC_FIRE trigger for the Pyromancer. This combo is strong but requires Archer proximity to burning zones, which damages them. |

---

## 7. Combo Chains

### Combo 1: Pyromancer + Hydromancer — "Scalding Fog"

**Mancers involved:** Pyromancer + Hydromancer

**Sequence:**
1. Pyromancer casts **Scorched Earth** on a 2-tile radius zone in the center of the engagement — establishes 5 ON_FIRE tiles.
2. (Same turn or following turn) Hydromancer casts **Tidal Force** (Hydromancer flood ability) across the ON_FIRE zone — water hits fire tiles.
3. Interaction: Fire + FLOODED = STEAM_CLOUD (3-tile radius, 3-turn duration) for each flooded fire tile. A 5-tile ON_FIRE zone flooded creates overlapping STEAM_CLOUD coverage.
4. All enemies in the STEAM_CLOUD zone take heat damage (3/turn while inside), are BLINDED (targeting range reduced to 1; their ranged Mancers are now useless), and are unable to see threats outside the cloud.
5. Pyromancer — now outside the cloud — uses **Ember Shot** to continue targeting visible enemies adjacent to cloud edges. Enemies inside the cloud cannot effectively return fire.

**Resulting elemental interaction:** Fire + Water = Steam (area blind zone + persistent heat DoT).

**Why this is strong:** BLINDED reduces a high-range Mancer to 1-tile targeting. A Photomancer, Electromancer, or any ranged-oriented Mancer behind a 3-tile STEAM_CLOUD is effectively neutralized for 3 turns. The Pyromancer sacrifices its terrain investment (fire converted to steam) for an arguably more valuable crowd-control output. The timing window — Hydromancer must flood the fire before it spreads too far — requires coordination but is executable in the same turn if both Mancers activate simultaneously.

**What it counters:** Ranged-heavy warbands, Photomancer vision reveals, Electromancer chain-fire positions. Forces the opponent to move blind units out of the cloud (burning AP on repositioning) or take sustained heat damage while effectively neutered.

---

### Combo 2: Pyromancer + Electromancer — "Arc Firestorm"

**Mancers involved:** Pyromancer + Electromancer

**Sequence:**
1. Pyromancer applies **Ember Shot** to a target unit — unit receives BURNING, tile becomes ON_FIRE.
2. Hydromancer (optional third party, if available) or existing WET terrain means another unit is WET.
3. Electromancer targets the BURNING unit with **Arc Bolt** (Electromancer lightning projectile).
4. BURNING + Lightning spell interaction: **Firestorm Burst** — Lightning hitting a BURNING tile/unit causes an explosive combination. The BURNING unit takes full Arc Bolt damage plus a 1-tile radius AoE fire burst (20 Fire damage) that spreads ON_FIRE to the burst tiles.
5. If any adjacent units were WET before the burst, the fire-on-wet triggers STEAM_CLOUD simultaneously at those tiles.
6. Result: BURNING target takes burst Lightning + bonus Fire AoE; adjacent WET units may take Steam blind; new ON_FIRE tiles created by the burst, which the Pyromancer can Conflagration Wave on the following turn.

**Without WET third party:**
- Pyromancer BURNS; Electromancer detonates for Firestorm Burst. Net effect: Arc Bolt damage + 20 AoE fire + new ON_FIRE tiles in burst radius. Extremely high combined damage output for 2 AP (Ember Shot) + whatever Arc Bolt costs.

**Resulting elemental interaction:** Fire (BURNING status on unit) + Lightning = Firestorm Burst (AoE fire explosion).

**Why this is strong:** This is a Tier 1 combo with Tier 2 potential. Pyromancer applies BURNING (1 activation investment — often Ember Shot was cast to do damage anyway), Electromancer detonates for free AoE fire expansion. The Pyromancer gets to "spend" its terrain-building budget while the Electromancer provides the detonation. The combo does not require pre-existing terrain states — it creates them. Best used mid-game when the Pyromancer has a BURNING unit and the Electromancer has line of sight.

**What it counters:** Multi-unit formations (the Firestorm Burst hits adjacent units); units that grouped up assuming one BURNING unit is manageable; high-HP single targets where combined burst damage is needed to breach HP thresholds.

---

### Combo 3: Pyromancer + Geomancer — "Obsidian Trap"

**Mancers involved:** Pyromancer + Geomancer

**Sequence:**
1. Pyromancer casts **Scorched Earth** or **Conflagration Wave** to establish a cluster of ON_FIRE tiles in a corridor or chokepoint.
2. Geomancer uses **Earth Smash** or any earth-element attack on the ON_FIRE tiles.
3. Interaction: Earth spell on ON_FIRE tile = Smother (extinguish) and then Harden: the tile becomes **OBSIDIAN** — permanently impassable, indestructible by most means.
4. With multiple ON_FIRE tiles converted to OBSIDIAN, Geomancer has constructed a permanent barrier from the Pyromancer's terrain investment — one that neither side can remove.
5. Pyromancer uses **Backdraft** passive (if purchased) to trigger explosion bursts when each fire tile is extinguished by Geomancer's smothering — dealing 15 fire damage per tile extinguished to units in 1-tile radius.

**Without Backdraft:**
- Pyromancer + Geomancer still creates OBSIDIAN barriers as permanent map control. The Pyromancer creates fire; Geomancer selectively smothers tiles to build walls exactly where needed. This converts a temporary terrain state into a permanent architectural obstacle, reshaping the map.

**Resulting elemental interaction:** Fire terrain + Earth spell = OBSIDIAN (permanent impassable tile).

**Why this is strong:** Permanent terrain reshaping is among the most powerful board-state manipulations in the game. Once OBSIDIAN exists, it cannot be removed by normal play — the map has been fundamentally altered. A Pyromancer and Geomancer who establish two or three OBSIDIAN pillars in a chokepoint have created a funnel that forces the entire enemy warband through a narrow lane, where subsequent Pyromancer fire zones are maximally punishing (no way around them). The Backdraft variant also means every tile Geomancer smothers generates a reactive explosion, turning the "construction" phase into a damage pass simultaneously.

**What it counters:** Wide-formation armies that rely on flanking; units with displacement abilities (Aeromancer, Gravimancer) that would push enemies around fire zones — you can't push through OBSIDIAN.

---

### Combo 4: Pyromancer + Toximancer — "Toxic Inferno"

**Mancers involved:** Pyromancer + Toximancer

**Sequence:**
1. Toximancer applies POISONED to target units (ideally stacking to 3–5 stacks across multiple turns).
2. Toximancer also seeds TOXIC_TERRAIN on approach tiles (from corpse explosion, ground-poison ability, or Floramancer pollen if combined).
3. Pyromancer casts **Ember Shot** or **Scorched Earth** into the TOXIC_TERRAIN zones.
4. Interaction: Fire + TOXIC_TERRAIN = TOXIC_FIRE hybrid (ON_FIRE + POISONED-on-contact combined state, 3 turns). Every unit on the tile each turn takes 5 HP fire DoT AND gains 1 POISONED stack.
5. For units already POISONED (Toximancer pre-applied stacks): POISONED + BURNING (from Pyromancer fire) triggers **Toxic Combustion** — POISONED converts to BURNING (higher dmg), plus an AoE toxic splash that spreads 1 POISONED stack to adjacent units. At high POISONED stacks (4–5), the combustion effect is devastating.
6. **Scorched Earth** targeting a TOXIC_TERRAIN zone creates TOXIC_FIRE across its full 2-tile radius — an area of poison-fire that stacks DoT on all units who enter or remain.

**Resulting elemental interaction:** Fire + POISONED unit = Toxic Combustion (BURNING upgrade + AoE poison spread). Fire + TOXIC_TERRAIN = TOXIC_FIRE (combined DoT ground state).

**Why this is strong:** This combo creates a feedback loop of DoT. POISONED units hit by fire become BURNING (higher DoT) and spread poison to neighbors; those neighbors now have both POISONED and are standing in TOXIC_FIRE terrain, gaining more POISONED stacks. The Pyromancer is generating BURNING; the Toximancer is generating POISONED stacks; the terrain is doing both simultaneously. Against a non-sustaining warband, this attrition loop is extremely hard to outpace. The Ashen Covenant faction amplifies this further because Grave Husks absorb fire terrain instead of suffering from it — they can advance through TOXIC_FIRE zones (taking the POISONED stacks but healing from the fire component via their passive) in ways no other faction's chaff can.

**What it counters:** High-HP tanky warbands that assume raw durability will outpace burst; Necromancer corpse-economy warbands (BURNING kills units cleanly without leaving clean corpses — burned corpses may count as lower-quality fuel for Necromancer reanimation, pending balance); units bunched around a single zone (TOXIC_FIRE punishes formation density).

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Pyromancer

| Mancer | Counter Mechanism |
|---|---|
| **Hydromancer** | Directly extinguishes ON_FIRE terrain with flood spells; converts the Pyromancer's entire terrain investment to WET (inert) or STEAM_CLOUD (non-spreading). A Hydromancer can undo 3 turns of Pyromancer fire zone building in a single activation. Also applies WET to Pyromancer itself, making it vulnerable to Electromancer chain — but Hydromancer's primary value is simply erasing fire. The hardest counter in the game. |
| **Cryomancer** | CHILL on Pyromancer reduces its Move Range (already the lowest at 3), and FROZEN skips its entire turn. Given the Pyromancer's 3-tile movement, Cryomancer can often CHILL from range before the Pyromancer can close to effective fire range. A FROZEN Pyromancer is a wasted activation — given its 85 HP and 1 armor, a FROZEN Pyromancer then hit by physical burst damage (SHATTER modifier) is close to lethal in one combo. Cryomancer also converts terrain to ICE_TILE and PERMAFROST, which Pyromancer fire melts (triggering STEAM_CLOUD) rather than burning — disrupting fire zone plans. |
| **Aeromancer** | Wind spells fan fire in uncontrolled directions (Conflagration Wave's spread can be redirected by subsequent Aeromancer wind), potentially turning the Pyromancer's fire spread against its own forces. More critically, Aeromancer's displacement abilities push the Pyromancer itself — a 3-Move Pyromancer on the edge of a pit that gets pushed 2 tiles by a wind gust is in VOID territory. Aeromancer also grants WEIGHTLESS status to allies, making them immune to ground terrain effects — directly negating the Pyromancer's primary damage-over-time terrain strategy. |
| **Gravimancer** | WEIGHTLESS applied to allies bypasses ON_FIRE terrain DoT (WEIGHTLESS units are immune to ground terrain effects). A Gravimancer that keeps its team airborne nullifies the Pyromancer's entire terrain economy. Also, Gravimancer's gravity wells pull the Pyromancer toward dangerous positions, overcoming its 3-tile range safety buffer. |

### Terrain Compositions That Shut Pyromancer Down

| Terrain Setup | Why It's Punishing |
|---|---|
| **Pre-flooded board (FLOODED tiles throughout)** | Every Pyromancer fire spell hits water and creates STEAM_CLOUD instead of ON_FIRE. The Pyromancer cannot build persistent fire terrain at all. Ironically, the STEAM_CLOUD zones blind enemies, but without ON_FIRE terrain, the Pyromancer's World Conflagration and terrain-spread strategies have nothing to feed. |
| **Heavy ICE_TILE / PERMAFROST coverage** | ICE_TILE does convert to FLOODED when hit by fire (generating useful steam), but PERMAFROST requires multiple fire interactions to remove and still generates STEAM_CLOUD not ON_FIRE. A Cryomancer pre-loading the map with permafrost forces the Pyromancer to spend AP clearing terrain states rather than building its own. |
| **OBSIDIAN-heavy map (Geomancer-created)** | OBSIDIAN cannot be burned. If an enemy Geomancer has created OBSIDIAN barriers, they block Pyromancer projectiles (Ember Shot, Pillar of Flame line of sight) and physically prevent fire spread across those tiles. The Pyromancer's fire cannot jump OBSIDIAN, breaking spread continuity. |
| **All-elevated terrain (no ground-level tiles near enemies)** | ON_FIRE cannot spread vertically between elevation levels without Conflagration Wave explicitly targeting elevated tiles. A Pyromancer trying to burn a high-ground position must spend AoE spells to reach it rather than letting natural fire spread do the work. |

### Warband Compositions That Prey on Pyromancer

| Warband Type | Exploitation |
|---|---|
| **Hydromancer + Aeromancer dual-Mancer list** | Hydromancer counters fire terrain; Aeromancer displaces the Pyromancer into hazards. The Pyromancer has no terrain output and no movement safety — it is picked apart before it can build momentum. |
| **Cryomancer + high-burst ranged screen** | Cryomancer freezes the Pyromancer; Crossbow Corps (or Siege Arbalests) fire on the FROZEN Pyromancer for SHATTER burst damage. Given 85 HP and 1 armor, this combination can eliminate the Pyromancer in a single coordinated activation. |
| **Gravimancer + Aeromancer (WEIGHTLESS warband)** | The entire enemy force is immune to ground terrain effects. Pyromancer fire zones deal zero passive damage. The Pyromancer is left casting spells that damage on cast but leave no lasting economic advantage on the terrain. |
| **Dense chaff screen + Hydromancer rear support** | Mass Chaff absorbs Scorched Earth damage (multiple low-HP units across the AoE dilute the 12-damage per unit, potentially not killing any), while Hydromancer in rear extinguishes any fire that threatens the chaff line. The Pyromancer cannot clear chaff efficiently and cannot build terrain investment through the screen. |

---

---

## 9. Augmentation Spell

### Combustion Brand

**AP Cost:** 3 | **Range:** 3 tiles | **Targeting:** Single allied unit | **Cooldown:** 3 turns

Brands an allied unit with volatile fire energy, converting their attacks into a vector for flame spread and making them dangerous to engage directly.

**Effects:**
- Ally's attacks apply 1 BURNING stack to targets on hit for 2 turns
- When the ally takes damage from any source, they release a minor fire burst — 1 damage to all adjacent tiles and units, applying BURNING to affected tiles
- While branded, WET cannot be applied to the ally; if WET was already active, it is removed and a Steam Cloud generates on the ally's tile

**Tactical intent:** Turns any allied unit into a walking BURNING spreader — particularly effective on Chaff who close to melee clusters. The damage-reactive burst punishes enemies who engage the branded unit in close combat. The WET immunity cuts both ways: the ally is protected from Electromancer chain-stun setup but also cannot benefit from Hydromancer's WET-based regen. Pyromancer fans the BURNING tiles created by the ally, generating a feedback loop of terrain denial that rewards tight activation sequencing.

**Notable interactions:** If the branded ally stands on a CHARGED tile when the reactive burst fires, the element matrix triggers an Arc Explosion around them. Requires setup but is extremely punishing in a tight formation.

*End of Pyromancer design document.*
*Format version: 1.0 — canonical template for all 19 Mancer docs.*

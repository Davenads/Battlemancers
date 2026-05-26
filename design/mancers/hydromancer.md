# Hydromancer

**Element:** Water, Tides, Fluids
**Tactical Role:** Battlefield Controller / Support Hybrid
**Difficulty:** Intermediate — rewards board reading and team coordination

---

## 1. Tactical Identity

The Hydromancer is the team's primary enabler. Where most Mancers measure their contribution in damage dealt, the Hydromancer measures it in conditions created. Every Wet tile on the board is a loaded weapon waiting for an ally to fire it. Every enemy knocked out of position is an opening that another Mancer can exploit. The Hydromancer rarely tops the damage charts in isolation — but in a well-coordinated team, it is the reason a single Electromancer turn ends a fight.

This Mancer occupies a role unique among the base roster: it is the only Mancer with access to reliable healing. That alone does not define it, but it does make the Hydromancer the closest thing Battlemancers has to a support anchor. The key tension in playing the Hydromancer well is deciding when to spend AP on setup (applying Wet, displacing enemies) versus conservation (healing, repositioning). A Hydromancer that only heals is wasted potential; a Hydromancer that ignores its team's HP is gambling with attrition.

**Primary win condition:** The Hydromancer-focused team wins by controlling the tempo of elemental state accumulation. A board saturated with Wet terrain and displaced enemies is a board where a single follow-up activation from an Electromancer or Cryomancer ends multiple threats simultaneously. The Hydromancer's team wins in the moment after setup resolves — not the moment the Hydromancer acts.

**Core weakness:** The Hydromancer is built for mid-range engagement and formation management. Up close, its displacement spells lose effectiveness (pushing an adjacent enemy away grants them positional freedom rather than punishing them) and it lacks burst damage to defend itself. Against aggressive melee pressure — particularly from Osteomancer constructs or Faunamancer companion swarms — the Hydromancer needs infantry screening or it becomes a liability. Its spells are also highly positioning-dependent: a Wet status applied to an isolated enemy in open terrain is far less valuable than Wet applied to a cluster. The Hydromancer rewards players who can read formations before they commit.

---

## 2. Base Stats

| Stat | Value |
|---|---|
| Max HP | 100 |
| Move Range | 4 tiles |
| Base Armor | 1 |
| Action Points (per activation) | 6 AP |

The Hydromancer's HP and armor make it the most durable of the support-oriented Mancers — it is designed to remain in the fight long enough to sustain its setup role across multiple turns. Its move range of 4 is intentionally modest; the Hydromancer should be repositioning to stay in range of allies rather than diving forward into threat range.

---

## 3. Base Spell Kit

### Spell 1 — Aqua Lance
**Type:** Projectile
**AP Cost:** 2 AP
**Cooldown:** 0 turns (spammable)
**Targeting:** Single target
**Range:** 6 tiles
**AoE Radius:** None
**Base Damage:** 10
**Element:** Water
**Effects Applied:** `WET` (2 turns) on hit unit; `WET` (1 turn) on target tile

**Description:** A piercing bolt of pressurized water fired at a single enemy. Low damage by itself, but Aqua Lance is the Hydromancer's primary combo primer — reliable, repeatable, and fast. The short cooldown means a Hydromancer can apply Wet to multiple targets across consecutive turns without spending heavy AP, keeping Electromancer chain opportunities stacked across the board.

**Special Interaction — Wet + Lightning (FLAGSHIP COMBO):** Any unit struck by Aqua Lance and left in the `WET` status is primed for Electromancer chain arcs. When an Electromancer fires a Lightning spell into a `WET` unit, the arc does not stop at the primary target: it chains to all adjacent `WET` units automatically. One Aqua Lance applied to a grouped cluster of enemies, followed by a single Electromancer bolt, can stun an entire formation simultaneously. This is the game's flagship two-Mancer combo. See Section 7 for full chain documentation.

---

### Spell 2 — Tidal Surge
**Type:** Area of Effect (Line)
**AP Cost:** 3 AP
**Cooldown:** 2 turns
**Targeting:** Directional line; target a direction from caster's position
**Range:** 5 tiles (line length); affects all units in line
**AoE Radius:** 1 tile wide (the line itself)
**Base Damage:** 8
**Element:** Water
**Effects Applied:** `WET` (1 turn) on all units hit; displacement — each unit in the line is pushed 2 tiles away from caster along the line direction

**Description:** A surging wave of water that blasts outward in a straight line from the Hydromancer. Tidal Surge is the Hydromancer's primary crowd control spell — it breaks enemy formations, separates grouped threats, pushes units into hazardous terrain, and applies Wet to every unit it hits as a secondary consequence. Tactically, Tidal Surge rewards map reading: pushing an enemy into a pit deals fall damage; pushing them through an ON_FIRE tile applies BURNING; pushing them into a wall stops displacement at the wall and the unit takes collision damage equal to the remaining push distance.

**Special Interaction — Pushing into Charged Tile:** If Tidal Surge pushes a unit onto a `CHARGED` tile, that unit immediately triggers the tile's lightning arc as if they moved onto it voluntarily. This interaction is particularly dangerous because the arc resolves while the unit is now WET from Tidal Surge itself, potentially chaining to nearby allies the Hydromancer already Wet from previous turns.

**Special Interaction — Pushing off Elevated Terrain:** Units displaced off an ELEVATED tile by Tidal Surge take fall damage (`fall_distance × 8` HP) in addition to the spell's base damage. Hydromancer + Geomancer teams will frequently use Raise Terrain followed by Tidal Surge to punish enemies on high ground.

---

### Spell 3 — Mending Current
**Type:** Targeted Status / Heal
**AP Cost:** 3 AP
**Cooldown:** 2 turns
**Targeting:** Single target (ally or self); LOS required
**Range:** 5 tiles
**AoE Radius:** None
**Base Heal:** 22 HP
**Wet Tile Bonus:** +10 HP (heals 32 total if target is standing on a WET or FLOODED tile)
**Element:** Water
**Effects Applied:** Removes `BURNING` from target; removes all `POISONED` stacks from target (consistent with `Hydromancer Cleanse` noted in status-effects.md)

**Description:** The only base heal in the Mancer roster. Mending Current channels a stabilizing flow of water energy toward an injured ally, knitting wounds and flushing toxins. The flat heal of 22 HP is meaningful against a 100 HP baseline — roughly 22% of a Mancer's total HP in a single cast — but not so large that a Hydromancer can undo sustained focus-fire indefinitely. The Wet tile bonus rewards proactive terrain setup: a Hydromancer that has already flooded the area around its injured ally receives an enhanced heal, incentivizing terrain-first play even in support scenarios.

**Design note — the only base heal:** Mending Current is intentionally limited to single-target and requires LOS. It is not a mass heal. A Hydromancer cannot sustain an entire warband alone; it can save a key Mancer from death once or twice per fight. The 2-turn cooldown means timing matters — burning the heal on minor chip damage leaves the Hydromancer unable to respond to a burst spike.

---

### Spell 4 — Flood Zone
**Type:** Terrain Placement (AoE Ground Effect)
**AP Cost:** 5 AP (Heavy — entire turn likely spent if Hydromancer moves at all)
**Cooldown:** 3 turns
**Targeting:** Target a central tile; LOS required to center tile
**Range:** 4 tiles to center
**AoE Radius:** 3 tiles (creates a 3-tile radius FLOODED zone around target point)
**Base Damage:** 6 (on initial cast — the rushing water deals minor impact damage to all units in zone)
**Element:** Water
**Effects Applied:** All tiles in radius become `FLOODED` (persists — expands 1 tile every 2 turns if not frozen or dried); all units in zone on cast receive `WET` (3 turns); units in zone have movement cost +1 (from FLOODED terrain state)

**Description:** Flood Zone is the Hydromancer's defining battlefield transformation spell. It converts a large area of terrain into a persistent conductive water surface — every tile in the zone becomes a chain-arc conductor for Electromancer Lightning, every unit within it becomes a potential chain target. The FLOODED state expands slowly over subsequent turns, meaning a well-placed Flood Zone does not just reward the turn it is cast but continues to shape the engagement for the rest of the fight.

Flood Zone costs the Hydromancer nearly its full turn (5 AP leaves only 1 AP, which allows 1 tile of movement). This is a deliberate commitment. A player casting Flood Zone is declaring: this area of the board is ours. The follow-up from Electromancer, Cryomancer, or Geomancer can justify the investment; cast into an area the opponent abandons immediately, and it is wasted.

**Special Interaction — Flood Zone + Electromancer:** Any unit in the FLOODED zone is automatically WET. A single Lightning spell hitting any unit in the zone chains to all adjacent WET units. With a full zone, this can stun or damage every unit in the flooded area with one Electromancer activation. This is the game's highest-value Tier 2 combo setup.

**Special Interaction — Flood Zone + Cryomancer:** Cryomancer mass freeze spells hitting a FLOODED zone convert all FLOODED tiles to `ICE_TILE` simultaneously, and every unit on those tiles is FROZEN. This is a Tier 2 combo that hard-locks an entire zone's worth of units.

**Special Interaction — Flood Zone + Pyromancer (adverse):** If a Pyromancer spell hits a FLOODED zone, the water + fire interaction creates `STEAM_CLOUD` across the entire zone, blinding all units (including allies) inside. The Hydromancer player must account for this risk when allies include a Pyromancer.

---

## 4. Terrain Interaction Table

Water spells (from any Hydromancer ability) interacting with existing terrain states:

| Existing State | Water Spell Hits | Result |
|---|---|---|
| `WET` | Any water spell | No state change — tile remains WET; if the incoming spell has a duration, the remaining WET duration is refreshed to whichever is longer |
| `ON_FIRE` | Any water spell | Extinguishes fire — tile becomes WET (1 turn residue). If the burning area was 3+ tiles, the rapid quench creates a `STEAM_CLOUD` (2-turn duration) that blinds all units in and adjacent to the former fire zone and deals 3 HP/turn |
| `FLOODED` | Any water spell | No terrain change; reinforces FLOODED; conductive properties active |
| `ICE_TILE` | Any water spell | Cracks ice — tile reverts to WET (not re-frozen); units on that tile take 6 HP from ice shard spray; if unit was standing on `ICE_TILE` that cracks, they are briefly knocked prone (lose 1 AP next turn) |
| `MUD` | Any water spell | Converts MUD to WET — removes the movement penalty of MUD, replacing it with the lighter conductivity state of WET. This is a meaningful upgrade: Hydromancer can clean up Geomancer's MUD zones that are no longer tactically useful |
| `TOXIC_TERRAIN` | Any water spell | Dilutes poison — removes 1 POISONED stack from all units on that tile; tile state shifts from `TOXIC_TERRAIN` to `WET` (the water flushes the toxins without fully eliminating them from units who accumulated stacks) |
| `CHARGED` | Any water spell | Water conducts the stored charge — chain arc fires immediately through all adjacent WET tiles and units, replicating the Electromancer chain effect. The `CHARGED` tile is consumed on resolution. This is an extremely high-value interaction when the Hydromancer deliberately irrigates a Charged zone |
| `STEAM_CLOUD` | Any water spell | Reinforces steam cloud duration by 1 turn (more moisture = more steam); does not otherwise change the blinding effect |
| `GROUND` (normal) | Any water spell | Tile becomes `WET` for 2 turns (secondary state), making it conductive without the movement penalty of FLOODED |
| `OBSIDIAN` | Any water spell | No state change — obsidian is impervious; water runs off without effect |
| `OVERGROWTH` | Any water spell | Overgrowth becomes `WET` (secondary state layered on top); if Electromancer later hits, the chain arc propagates through the Wet overgrowth. The growth itself is not damaged by water |

**Hydromancer-specific terrain traits (inherent, always active):**
- The Hydromancer ignores the movement cost penalty of `FLOODED` and `WET` tiles — it moves through water at standard cost (1 AP per tile).
- Mending Current heals +10 HP when the target stands on a `WET`, `FLOODED`, or `ICE_TILE` tile.

---

## 5. Upgrade Options

### Spell Variants

**Variant A — Aqua Lance: Torrent Bolt** (+20 pts)
Replaces Aqua Lance with a wider-spread version that fires a 3-tile-wide cone of pressurized water instead of a single bolt. Base damage is reduced to 7 per unit hit, but applies `WET` to all units in the cone. AP cost increases to 3 AP; cooldown is 1 turn. Torrent Bolt sacrifices single-target reliability for the ability to Wet multiple clustered enemies in a single cast — a direct Electromancer chain amplifier.

**Variant B — Tidal Surge: Undertow** (+20 pts)
Replaces Tidal Surge with a pull variant rather than a push. Undertow targets a point within 5 tiles and pulls all units within a 2-tile radius toward that point. Units pulled together deal collision damage to one another (4 HP per unit collided). Wet is still applied. Undertow is a formation-collapsing tool rather than a formation-dispersing one — used to cluster enemies for AoE follow-ups from Electromancer, Cryomancer, or Sonimancer.

**Variant C — Flood Zone: Torrent Surge** (+25 pts)
Replaces Flood Zone with a faster, smaller version. Torrent Surge creates a 2-tile radius FLOODED zone but costs only 3 AP and has a 2-turn cooldown. Sacrifices the massive zone size for the ability to cast it more frequently and with enough AP remaining to follow up with Aqua Lance in the same activation. Best for Hydromancers built around consistent zone denial rather than one large setup.

---

### Passive Traits

**Trait A — Fluid Form** (+20 pts)
The Hydromancer treats all WET, FLOODED, MUD, and WATER_SHALLOW tiles as standard GROUND for movement purposes (movement cost 1). Additionally, the Hydromancer cannot be displaced by push/pull effects while standing on a WET or FLOODED tile — it roots itself in the water. This makes the Hydromancer significantly harder to remove from contested water zones and resistant to Aeromancer or Gravimancer displacement.

**Trait B — Tidal Empathy** (+25 pts)
Allied units standing on WET or FLOODED tiles adjacent to the Hydromancer gain +10% damage on all spells or attacks. This is a passive aura with no AP cost — it simply rewards the Hydromancer for flooding the area its team fights in. Encourages tight formation play around the Hydromancer and creates a meaningful incentive for allies to stand in the water rather than retreat to dry terrain.

**Trait C — Water Absorption** (+15 pts)
The Hydromancer's armor reduces Fire damage by an additional 3 HP (total effective armor of 4 vs. Fire). Additionally, whenever the Hydromancer takes Fire damage, it automatically applies WET to itself on the same turn, potentially triggering the Burning extinguish interaction on the BURNING status it received. This is a reactive defensive trait — it does not prevent being BURNING but creates an immediate counter-process.

**Trait D — Tidal Cleanse** (+20 pts)
Mending Current gains an expanded cleanse list: in addition to removing BURNING and POISONED, it also removes CHILLED, SLOWED, and CALCIFIED from the target. Additionally, using Mending Current on a target that was BURNING generates a small STEAM_CLOUD (1-tile radius, 1 turn) on the healed unit's tile — the quench creates local steam cover as a bonus.

---

### Stat Enhancements

**Enhancement A — Deep Reservoir** (+15 pts)
Max HP increased to 120. The Hydromancer can withstand more sustained pressure before requiring self-preservation decisions. Best for Hydromancers built around the support anchor role in triple-Mancer lists where the team depends on the heal staying available.

**Enhancement B — Swift Currents** (+10 pts)
Move Range increased from 4 to 5. The extra tile of movement allows the Hydromancer to reposition more aggressively — reaching injured allies faster, withdrawing from melee threat, or extending its Flood Zone reach by 1 tile (since the spell requires the caster to be within range of the zone's center).

---

### Signature Ability

**Signature — The Great Tide** (+40 pts)
**AP Cost:** 6 AP (entire turn; Hydromancer cannot move on this turn)
**Cooldown:** 5 turns
**Targeting:** Choose a target direction (North, South, East, or West relative to caster)
**Effect:** A massive wall of water sweeps across the entire board in the chosen direction, hitting every unit and tile in its path. Each unit hit takes 14 damage and receives `WET` for 3 turns. Each tile the wave passes through becomes `FLOODED`. Units hit by the wave are pushed 3 tiles in the wave direction; units already on FLOODED tiles when the wave hits are pushed 4 tiles instead. After the wave resolves, the FLOODED terrain it creates begins expanding from any water source on its path.

This is one of the most tactically transformative abilities in the game. Executed correctly — ideally with an Electromancer activation queued for the same turn — The Great Tide floods half the board, Wets every enemy unit simultaneously, and creates conditions for a full-team chain combo. Its 5-turn cooldown and all-or-nothing AP cost mean it is used once per long engagement at most. Telegraphed to the opponent by the Hydromancer being stationary for a full turn, which skilled players will exploit.

---

## 6. Faction Synergy

### Best Pairing: The Gilded Throne

The Hydromancer pairs most cleanly with The Gilded Throne faction. Conscript Spearmen form a melee screen that keeps enemies from closing on the Hydromancer, and Iron Vanguard veterans in Shield Wall formation become exceptionally hard to dislodge when the Hydromancer can sustain their HP with Mending Current. The Gilded Throne's Iron Discipline trait (immunity to Panic and Charm) also protects the Hydromancer's supporting infantry from the Psychomancer disruptions that otherwise threaten support-focused teams.

**Crossbow Corps synergy:** Crossbow Corps fire every other turn. In the turns they are not firing, the Hydromancer can flood their positions, keeping the zone WET. When the Electromancer subsequently chains through a WET tile containing a Crossbow Corps unit, the Arc damage does not distinguish friendly from enemy — a reminder that the Hydromancer player must be deliberate about which tiles are Wet and which allies are standing where.

### Verdant Pact — Terrain Bond Ruling

Verdant Pact's Terrain Bond grants bonus movement and passive regeneration on **natural tiles** (forest, earth/mud, vine-covered tiles, frozen water per the faction description). Wet tiles created by Hydromancer spells are **not natural tiles** — they are magically created water surface states applied to pre-existing terrain. Wet tiles do NOT trigger Terrain Bond movement bonuses or regeneration. This ruling is intentional and prevents a Hydromancer + Verdant Pact combination from becoming self-sustaining: if Wet tiles triggered Terrain Bond regen, the Hydromancer could trivially sustain an entire Verdant Pact infantry screen indefinitely through passive healing loops, compounding with Mending Current to create near-unkillable chaff.

FLOODED terrain similarly does not qualify as a natural tile for Terrain Bond purposes — it is a water state, not forest or earth.

**What does qualify:** If the Hydromancer's Flood Zone covers existing MUD tiles (which are earth-origin), the MUD itself is natural; but once converted to WET by water interaction, the tile loses its natural classification for Terrain Bond purposes.

### Ashen Covenant — Grave Husks and Wet Terrain

Grave Husks regenerate 1 HP per turn while standing in **Poisoned, Corrupted, or Burning terrain** (necrotic absorption from their unit description). Wet terrain is not necrotic. Grave Husks do NOT regenerate on Wet or Flooded tiles. The Ashen Covenant's sustain mechanic is specifically tied to dark-energy terrain states, and Hydromancer water is elemental, not necrotic.

This makes the Hydromancer a neutral pairing with Ashen Covenant — functional but not synergistic at the faction trait level. The Hydromancer + Necromancer spell combo (Necromancer using corpse fuel near a Flooded zone) remains viable, but the Covenant's passive infantry trait does not benefit from Hydromancer terrain work.

---

## 7. Combo Chains

### Combo 1 — The Shock Network (Hydromancer + Electromancer) [FLAGSHIP]

This is the game's primary advertised cross-Mancer combo and the main reason the Hydromancer earns its roster slot in competitive play.

**Step-by-step execution:**

1. **Turn N, Hydromancer activates:** Hydromancer casts Aqua Lance at the highest-value enemy target in a group (1 AP move to position + 2 AP Aqua Lance = 3 AP). Aqua Lance applies `WET` to the hit unit. If 2+ AP remain, Hydromancer casts Aqua Lance again at a second adjacent enemy (2 more AP). Result: 1-2 enemies are `WET`.
2. *(Optional setup turn)* **Turn N, Flood Zone cast instead:** If positioning allows, Hydromancer casts Flood Zone (5 AP) over the enemy cluster. All enemies in the zone are automatically `WET`. Superior setup but costs the Hydromancer a near-full turn and signals intent.
3. **Turn N (same turn, Electromancer activates after Hydromancer per Mancer initiative):** Electromancer fires any Lightning spell at any `WET` unit in the group.
4. **Chain arc resolution:** The Lightning bolt deals its base damage to the primary target. Because the target is `WET`, the arc chains to all adjacent `WET` units automatically. Each chained unit takes arc damage and receives `STUNNED` (1 turn — skip entire turn, no AP).

**Tactical outcome:** At minimum, 2 enemies are STUNNED simultaneously in a single turn. With a Flood Zone setup, potentially 4-6 or more units are STUNNED. STUNNED enemies cannot activate on their next turn — which in the blind-turn system means the opponent effectively loses those units' activations, a catastrophic action-economy swing. This combo at its peak can effectively end a match by removing the opponent's ability to respond.

**Counter-play:** The combo is readable. A Wet enemy visible on the board signals the Electromancer follow-up. Savvy opponents will disperse their units out of chain range, retreat off Wet tiles before the Electromancer activates, or invest in units that resist the Wet status (currently none, but terrain avoidance is the primary counter). The STUNNED duration is only 1 turn — so the combo must be followed up decisively or the stunned enemies recover.

---

### Combo 2 — The Ice Lock (Hydromancer + Cryomancer)

**Setup:** Hydromancer casts Flood Zone over an enemy cluster (or applies Wet via Aqua Lance to multiple adjacent targets).
**Execution:** Cryomancer casts any mass freeze spell (or multiple targeted freeze spells) into the FLOODED zone.
**Result:** All `FLOODED` tiles in the zone convert to `ICE_TILE`. All units on those tiles are immediately `FROZEN` (skip turn + SHATTER vulnerability — incoming physical or sonic damage deals ×2.5).

**Tactical outcome:** The Ice Lock hard-locks an enemy cluster. FROZEN units cannot act on their next turn and are critically vulnerable to physical follow-up — Osteomancer constructs, Faunamancer companion swarms, or Sonimancer shatter spells can then SHATTER each frozen unit for massive burst damage. This is a Tier 2 combo (two Mancers contributing two distinct states) and requires turn sequencing: Hydromancer floods first, Cryomancer freezes second.

---

### Combo 3 — The Blind Boil (Hydromancer + Pyromancer)

**Setup:** Hydromancer floods a tile cluster (Flood Zone or accumulated Aqua Lance Wet state on terrain).
**Execution:** Pyromancer casts any Fire spell (Fireball, Ember Shot, etc.) into the WET or FLOODED zone.
**Result:** Water + Fire = `STEAM_CLOUD`. A 2-turn Steam Cloud forms over the flooded area, blinding all units inside (targeting range reduced to 1 for the duration) and dealing 3 HP/turn.

**Tactical outcome:** The Steam Cloud forces enemies out of the zone or renders them near-unable to target the Hydromancer or its allies from range. Enemy ranged units (Crossbow Corps, Wailing Shades) are effectively neutralized for 2 turns while inside the cloud. This combo also functions defensively — if the Hydromancer's team is losing the ranged fight, a deliberate Blind Boil obscures the board and resets the engagement.

**Risk note:** If the Hydromancer's own allies are inside the Steam Cloud, they are equally blinded. This is one of the primary execution risks of running Hydromancer + Pyromancer — the two Mancers' interactions can punish poor formation management as readily as they punish the opponent.

---

### Combo 4 — The Mud Trap (Hydromancer + Geomancer)

**Setup:** Hydromancer casts any water spell (Aqua Lance, Flood Zone) onto a tile that a Geomancer then converts with an Earth spell — or Geomancer uses Earth spell on existing WET terrain.
**Execution:** Earth spell hitting a WET tile converts it to `MUD`.
**Result:** MUD terrain imposes movement cost +2 on all units entering or moving through it. Enemies caught in the Mud Trap are slowed, cannot reposition quickly, and are vulnerable to follow-up spells requiring the enemy to stand still.

**Tactical outcome:** The Mud Trap is a denial and control combo rather than a damage combo. It makes a central area of the board extremely costly to traverse, funneling enemy movement into predictable paths. The Hydromancer can then use Tidal Surge to push enemies into the mud zone (shoving them from outside in) or use Flood Zone to flood the entire area and upgrade the Mud to a conductive water surface for the Electromancer follow-up.

**Reverse combo note:** If the Hydromancer later casts a water spell on MUD tiles, the MUD converts to WET (cleaning the mud but creating a conductive surface). This gives the Hydromancer + Geomancer pairing flexible sequencing: Mud first for movement denial, then convert to Wet for Electromancer chain setup, cycling the terrain state through two different denial phases in one match.

---

## 8. Counters and Weaknesses

### What Shuts Down the Hydromancer

**Melee pressure:** The Hydromancer's displacement spells (Tidal Surge) become less effective at point-blank range — pushing an adjacent enemy 2 tiles is rarely a punishment when they simply walk back next turn. Mancers or chaff that can close the gap before the Hydromancer establishes Wet terrain will consistently deny its setup phase. Faunamancer companion swarms are a particular threat — multiple fast companion units can simultaneously threaten the Hydromancer from multiple directions, making Tidal Surge insufficient to protect it.

**Psychomancer disruption:** A Charmed Hydromancer is the opponent's most dangerous tool against a Hydromancer-centric team. When Charmed, the Hydromancer's player loses control of it for 1 turn — meaning the opponent can use Mending Current to heal one of their own units, or use Flood Zone to prime their own Electromancer. The Hydromancer's support role makes Charm disproportionately rewarding compared to Charming a pure damage Mancer. Confused status (randomized targeting) is similarly damaging: a Confused Hydromancer applying Wet randomly may prime the opponent's units for the Hydromancer's own Electromancer ally to chain into — until the chain reaches an ally who also became Wet.

**Electromancer friendly fire:** If the Hydromancer gets its own allied units Wet — through Flood Zone covering an allied formation, or Aqua Lance missing and hitting terrain behind an ally — and the team includes an Electromancer, a poorly aimed Lightning spell can chain into those WET allies. This is the most common self-inflicted mistake in Hydromancer play. The Hydromancer player must track every Wet unit and tile on the board and communicate implicit safe zones to the Electromancer activation plan before lock-in.

**Steam Cloud misfire:** Running Hydromancer + Pyromancer and failing to coordinate positioning can trigger a Blind Boil over the Hydromancer's own chaff, blinding allied units and negating the ranged advantage the team invested in. Blind Boil is a powerful combo but requires the Hydromancer's player to not have allied units inside the flood zone when the Pyromancer fires.

**Silenced or Stunned Hydromancer:** The Hydromancer has no reactive or self-defensive spells. A Silenced Hydromancer cannot cast Mending Current, Aqua Lance, or Tidal Surge. If the team's healing source is Silenced at a critical moment and an ally is at low HP, the loss of the heal can cascade into a losing attrition position. Sonimancer Silence and Psychomancer Silence are the primary threats. Gilded Throne's Iron Discipline partially mitigates this by protecting infantry from morale debuffs, but the Hydromancer itself is a Mancer and is not protected by Iron Discipline.

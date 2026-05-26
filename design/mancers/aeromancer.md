# Aeromancer — Full Design Document

---

## 1. Tactical Identity

The Aeromancer is the board's most mobile and most disruptive force — not through raw damage, but through control of where everyone stands. Every other Mancer in the roster measures threats by what spells can reach them from where they currently stand. The Aeromancer simply changes where they stand. Wind displacement is the Aeromancer's primary power: pushing enemy units off elevated platforms for fall damage, into fire and toxic terrain for DoT, into walls for collision damage, and into each other for mutual disruption. The Aeromancer turns the board's hazards into delivery systems, and its own allied Mancers' terrain investments into weapon systems by pushing enemies onto them.

Playing the Aeromancer well requires a thorough understanding of the entire board at all times — not just the current tile states but the hazards a 2-tile push would expose a target to. The Aeromancer does not broadcast its kills through dramatic spell effects; it kills by asking, "what happens to this enemy if I push them 2 tiles to the left?" The answer to that question, consistently answered correctly, is what separates Aeromancer players. The Mancer's own survivability comes from mobility: its 5-tile move range is the highest in the roster, and UPDRAFT grants its team WEIGHTLESS immunity to ground terrain that threatens slower Mancers. Evasion and repositioning are the Aeromancer's defensive tools — it has no meaningful armor or HP to absorb punishment.

**Primary win condition:** The Aeromancer wins by guaranteeing that every combat exchange occurs on terms favorable to its team. Enemies are pushed into allied spell ranges; allies are protected from melee engagement by consistent enemy displacement; fall hazards, fire zones, and terrain barriers are all weaponized through pushing. The Aeromancer team wins not by out-damaging but by out-positioning so thoroughly that every opposing Mancer action is spent recovering ground rather than threatening.

**Core weakness:** The Aeromancer has the lowest base damage output of any offensive Mancer. Its spells deal modest damage in isolation; without allies to deliver the actual killing blows on repositioned enemies, it generates tremendous positional pressure but insufficient lethal follow-through. An opponent who can absorb or ignore displacement (HEAVY status from Gravimancer, WEIGHTLESS counter via their own Aeromancer, ROOTED units that cannot be pushed) neutralizes the Aeromancer's primary toolkit. At 80 HP and 0 armor, it is the squishiest Mancer in the roster — one coordinated burst from a mid-range Mancer eliminates it if its displacement game fails to keep enemies far enough away.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 80 | Lowest HP in the roster; relies entirely on positional safety, not durability |
| **Move Range** | 5 tiles per activation | Highest move range in the roster; repositioning is the Aeromancer's defense |
| **Base Armor** | 0 | Zero physical mitigation; one hard hit threatens lethality |
| **Spell Range** | 6 tiles (base) | Long reach — displacement is applied from safe distances |
| **AP per Activation** | 6 | Standard; movement costs 1 AP per tile |
| **Element** | Wind | All base spells deal Wind/Physical damage and apply wind-element interactions |

**AP budget example:** With 6 AP, the Aeromancer can move 4 tiles (4 AP) and cast Gust Strike once (2 AP) — repositioning aggressively and still applying a displacement. Or move 2 tiles and cast Cyclone Blast (4 AP) for a major AoE displacement followed by rapid repositioning using the 2 remaining AP of free move range.

---

## 3. Base Spell Kit

The Aeromancer's four base spells cover distinct displacement functions:
- **Gust Strike** — repeatable single-target push; primary displacement tool
- **Cyclone Blast** — AoE displacement; breaks formations and scatters enemies
- **Updraft** — zone ability; grants WEIGHTLESS to allies; denies ground terrain to enemies in zone
- **Wind Wall** — redirecting barrier; deflects projectiles; creates forced movement lane

---

### Spell 1: Gust Strike

| Field | Value |
|---|---|
| **Name** | Gust Strike |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (directional push; target a unit; choose push direction — can push in any of 4 cardinal directions regardless of Aeromancer's facing) |
| **Range** | 6 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 10 |
| **Element** | Wind / Physical |
| **Effects Applied** | Deals 10 Wind damage to target. Pushes target 2 tiles in the chosen direction. If the pushed unit hits a wall, obstacle, or another unit at any point during the push: stop displacement and deal collision damage equal to remaining push tiles × 6 HP (1 tile remaining = 6 HP; 2 tiles remaining = 12 HP if stopped immediately). The target is also `SLOWED` (–1 move, 1 turn) from the disorienting impact. |
| **Temperature Effects** | **-5 temperature** (localized wind chill — the concentrated gust strips surface heat from the target). |
| **Special Interactions** | Against a unit on `ELEVATED` terrain: if the 2-tile push carries the unit off the elevated edge, fall damage is applied additionally: fall_distance × 8 HP. A unit on a 1-level ELEVATED tile pushed 1 tile off the edge takes 8 HP fall damage. A unit on a 2-level ELEVATED tile (Geomancer Titan's Ascent) pushed off takes 16 HP fall damage. Fall damage is applied AFTER push-path collision is resolved. Against a unit being pushed into `ON_FIRE` terrain: the unit lands on the fire tile and receives `BURNING` (5 HP/turn) at the end of the push. Against a unit being pushed into `ICE_TILE`: slip check triggers on landing — the pushed unit may slide 1 additional involuntary tile (extending the effective push to 3 tiles if slip triggers). Against a unit being pushed into `TOXIC_TERRAIN`: unit receives `POISONED` (1 stack) on landing. Against a unit being pushed into `CHARGED` tile: the charged arc fires immediately on the unit's landing (resolves as if the unit stepped onto the tile voluntarily — arc damage + chain to adjacent WET units). Against a unit with `HEAVY` status: push distance reduced by 1 (HEAVY units resist displacement — 2-tile push becomes 1-tile push). Against a unit with `WEIGHTLESS` status: push distance increased by 1 (WEIGHTLESS units displace more easily — 2-tile push becomes 3-tile push). |

**Design note:** Gust Strike is the Aeromancer's workhorse — cheap, no cooldown, high utility. Two Gust Strikes in a single activation (4 AP total) push two different targets 2 tiles each. The damage is low (10 HP) but the push is the point. The key execution element is direction selection: the Aeromancer does not push toward the nearest wall — it pushes toward the nearest meaningful hazard. A player who consistently maps hazard tiles before activating Gust Strike will convert each 2 AP into environmental damage that dwarfs the spell's base damage output. A 2-tile push into ON_FIRE terrain gives the target Burning (5 HP/turn for free); a push off a Geomancer elevated tile deals 8–16 HP fall damage at no additional cost.

**Displacement mechanics summary (applicable to all Aeromancer push spells):**

When a unit is displaced by any Aeromancer push, the displacement resolves tile by tile in the push direction. At each tile step during displacement:
1. If the next tile is blocked (wall, obstacle, another unit): displacement stops; collision damage applies.
2. If the next tile is passable: unit moves to that tile; any terrain state on the arrival tile applies on landing.
3. If the next tile is a drop-off (edge of elevated terrain, pit, void): fall damage applies.
4. ICE_TILE on the landing tile triggers a slip check (involuntary 1-tile extension of movement in push direction).
5. WEIGHTED status: –1 push tile. WEIGHTLESS status: +1 push tile.

**Spell answers YES to (design rule check):**
1. Moves a unit (2-tile push with full terrain interaction on landing) — YES
2. Exploits existing terrain states (fall damage, fire, ice, toxic, charged on landing) — YES
3. Applies unit status (SLOWED on collision) — YES
4. Skill expression: direction selection for hazard landing; push path collision calculation — YES

---

### Spell 2: Cyclone Blast

| Field | Value |
|---|---|
| **Name** | Cyclone Blast |
| **AP Cost** | 4 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Ground Target — AoE Radial (targets a point; units in radius are pushed outward from center) |
| **Range** | 5 tiles (to center) |
| **AoE Radius** | 2 tiles |
| **Base Damage** | 14 (to all units in AoE) |
| **Element** | Wind / Physical |
| **Effects Applied** | Deals 14 Wind damage to all units in 2-tile radius. All units in the radius are pushed 2 tiles directly away from the center point (radial displacement — each unit is pushed in the direction from center to their current tile). If a pushed unit hits a wall or another unit during displacement: collision damage (remaining push tiles × 6 HP). All tiles in the AoE become `BLUSTERED` (temporary wind state, 1-turn duration): units entering or beginning their turn on BLUSTERED tiles are pushed 1 additional tile in a random cardinal direction (the residual gust is chaotic). |
| **Temperature Effects** | **-5 temperature to all units hit** (turbulent air disperses heat across the entire affected radius). |
| **Special Interactions** | Against a tightly grouped formation (3+ units in the AoE): Cyclone Blast scatters the formation radially. Units pushed into each other from radial displacement deal mutual collision damage (6 HP each — two units displaced into the same tile collide). The combination of unit-into-unit collisions across a full formation scatter can deal significant incidental damage without any additional AP investment. Against `ON_FIRE` terrain in the AoE: wind fans the fire — each `ON_FIRE` tile in the AoE immediately spreads to 2 additional adjacent tiles in the radial direction (wind carries fire outward). This interaction is powerful but risky — the Aeromancer must account for where fire spreads before using Cyclone Blast near Pyromancer fire zones. Against `FLOODED` terrain in the AoE: wind disrupts the water surface but does not remove the FLOODED state — FLOODED tiles in the AoE instead have their conductivity temporarily reduced (Electromancer chain-arc range through FLOODED reduced by 1 tile for 1 turn, until the BLUSTERED state expires). Against `STEAM_CLOUD` in the AoE: wind disperses the steam — STEAM_CLOUD tiles are removed (wind clears the blind zone). This can be used intentionally to clear a Pyromancer-created blind cloud that is affecting allies. |

**Design note:** Cyclone Blast is the formation-breaker. An enemy warband that clusters (Crossbow Corps adjacent to each other for combined fire, Iron Vanguard in Shield Wall formation, Grave Husks advancing in a block) is a Cyclone Blast target. The radial displacement scatters all units outward and collides them into each other. After a Cyclone Blast into a tight formation, the opponent's units are separated, potentially on hazard terrain, and the Shield Wall / Deathless Ranks formation bonuses that depend on adjacency are disrupted. The AP cost (4 AP) makes it a committed play — the Aeromancer likely cannot move significantly and cast Cyclone Blast in the same activation. Position the Aeromancer first; cast on the following turn.

**Spell answers YES to (design rule check):**
1. Moves multiple units (radial displacement of all units in AoE) — YES
2. Applies terrain state (BLUSTERED zone) — YES
3. Synergizes with Pyromancer (fire spread), Electromancer (disrupts cluster setups), Geomancer (fans fire toward walls for Obsidian combo) — YES
4. Skill expression: center point selection for maximum radial collision; fire direction control — YES

---

### Spell 3: Updraft

| Field | Value |
|---|---|
| **Name** | Updraft |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Ground Target — AoE Radial (zone creation; targets a point to center the updraft zone) |
| **Range** | 4 tiles (to center of zone) |
| **AoE Radius** | 2 tiles |
| **Base Damage** | 0 (terrain effect only; no direct damage) |
| **Element** | Wind |
| **Effects Applied** | Creates an `UPDRAFT` zone centered on the target point (2-tile radius). While inside the UPDRAFT zone: all units (ally and enemy alike) gain `WEIGHTLESS` status (floating; immune to all ground terrain effects — WET, ICE_TILE, TOXIC_TERRAIN, MUD, ON_FIRE DoT, CHARGED arc — while WEIGHTLESS). WEIGHTLESS units inside the zone are also displaced +1 tile further by any push effect targeting them. The UPDRAFT zone persists for 3 turns, then dissipates. |
| **Temperature Effects** | **-5 temperature** (rapid altitude change in the updraft column causes immediate adiabatic cooling for any unit entering the zone). |
| **Special Interactions** | Ally use (primary strategic use): Aeromancer targets the UPDRAFT zone over its own allied formation. Allied Mancers and chaff inside the zone are immune to all ground terrain effects — Pyromancer ON_FIRE DoT, Hydromancer FLOODED movement penalty, Cryomancer ICE_TILE slip, Toximancer TOXIC_TERRAIN POISONED stacks, Electromancer CHARGED arcs. The allied force can advance through hazardous terrain without penalty. Enemy use (secondary effect): enemy units that enter the UPDRAFT zone also become WEIGHTLESS — they become more vulnerable to the Aeromancer's own push spells (+1 tile push distance). The Aeromancer can exploit the zone symmetry by pushing WEIGHTLESS enemies even further than normal Gust Strike range. Against `ICE_TILE` terrain in the zone: WEIGHTLESS units in the zone are immune to ICE_TILE slip (they float above the ice surface). This removes the Cryomancer's slip-trap utility for any unit in the zone — a relevant trade-off when using Updraft near Cryomancer terrain. Against `ON_FIRE` terrain: WEIGHTLESS units float above the ground — they take zero fire terrain DoT from standing on ON_FIRE tiles. They CAN still be hit by the Pyromancer's direct spell damage; only the passive ground-contact DoT is negated. Against `MUD` or `FLOODED`: movement cost penalties are ignored for WEIGHTLESS units — the Aeromancer can create an air corridor through difficult terrain that its allies traverse at full speed. |

**Design note:** Updraft is simultaneously the Aeromancer's best defensive tool and its most nuanced tactical investment. Creating a WEIGHTLESS zone over allied formations neutralizes an entire category of enemy terrain tactics — Pyromancer cannot deny the approach with fire DoT; Toximancer cannot poison through ground contact; Cryomancer slip traps are void. This forces the opponent to use direct-damage spells against the Aeromancer's team rather than terrain leverage, which is typically less efficient. The trade-off is that the zone also benefits enemies who enter it — pushing enemies INTO the Updraft zone makes them easier to displace further (WEIGHTLESS +1 push tile), but also grants them terrain immunity. Careful zone placement away from enemy formation positions and over allied approach paths maximizes the defensive benefit while limiting enemy exploitation.

**Spell answers YES to (design rule check):**
1. Applies unit status to zone occupants (WEIGHTLESS) — YES
2. Creates persistent zone effect (3-turn UPDRAFT terrain feature) — YES
3. Synergizes with nearly every Mancer (counters ground-terrain reliant enemies, protects allied ground-terrain-immune advance) — YES
4. Skill expression: zone placement relative to allied approach paths vs. enemy terrain investment; enemy WEIGHTLESS push exploitation — YES

---

### Spell 4: Wind Wall

| Field | Value |
|---|---|
| **Name** | Wind Wall |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Terrain Placement — line placement (1 tile wide, up to 3 tiles long) |
| **Range** | 5 tiles (to nearest Wind Wall segment) |
| **AoE Radius** | N/A (line feature) |
| **Base Damage** | 0 (placement spell; no direct damage) |
| **Element** | Wind |
| **Effects Applied** | Creates a `WIND_WALL` — an invisible air pressure barrier on the target tile-line. WIND_WALL properties: does not block physical movement (units can walk through it). Does NOT block line of sight. DOES redirect projectile spells: any projectile passing through a WIND_WALL is deflected 1 tile in the direction of the wall's wind current (chosen by the Aeromancer at placement; left or right of travel direction). Does NOT deflect sonic or psychic spells. WIND_WALL persists 3 turns. Any unit walking through the WIND_WALL is pushed 1 tile in the wind direction (lateral push — 1 tile sideways from their direction of movement, potentially onto a hazard tile). |
| **Temperature Effects** | **-5 temperature per turn to any unit in or passing through the wall** (persistent airflow strips heat from units that linger in or cross the pressure barrier). |
| **Special Interactions** | Against incoming Pyromancer Ember Shot passing through Wind Wall: deflected 1 tile — the projectile may now miss its intended target and instead hit an adjacent unit or tile. Deflection can cause friendly-fire if the Aeromancer is not careful about wind direction relative to allied positions behind the wall. Against incoming Electromancer Arc Bolt passing through Wind Wall: deflected — however, Arc Bolt chain arcs (secondary chain from WET units) are NOT deflected (only the initial projectile beam is). So a Wind Wall can deflect the primary bolt but not prevent chain-arc propagation if the bolt still lands on a WET unit. Against `ON_FIRE` tiles adjacent to Wind Wall: wind from the wall fans fire in the wind direction — ON_FIRE tiles adjacent to the wall spread each turn toward the wind direction (fire carried by the gust). This can direct Pyromancer fire spread with predictable geometry. Against units walking through the Wind Wall and landing on `ICE_TILE`: lateral push from Wind Wall + ICE_TILE slip check both apply — potentially 2-tile total involuntary displacement from a single Wind Wall crossing. |

**Design note:** Wind Wall is the Aeromancer's most unique ability — a projectile deflection system that changes the threat geometry of the entire range engagement. A Wind Wall placed between an enemy Pyromancer and the Aeromancer's allied formation causes every Ember Shot and Fireball aimed through that path to deflect. The opponent must reposition around the Wind Wall to get clear LoS or spend AP targeting from a non-deflected angle. Wind Wall also creates a lateral-push movement hazard — enemies walking through it expecting to reach close range are sidestepped 1 tile into potentially dangerous positions. The 3-turn duration means it lasts long enough to significantly shape the mid-game positioning phase.

**Spell answers YES to (design rule check):**
1. Creates a terrain feature with persistent effects (WIND_WALL) — YES
2. Moves units (lateral push on passage) — YES
3. Redirects projectile spells (unique mechanic — no other Mancer does this) — YES
4. Synergizes with Pyromancer (fire direction with wall), Electromancer (partial deflection), Geomancer (fall-hazard landing) — YES
5. Skill expression: wind direction selection for deflection angle; placement relative to likely projectile paths — YES

---

## 4. Terrain Interaction Table

### Wind Spell Impact on Existing Terrain States

The following describes what happens when any Aeromancer spell interacts with a tile in the listed terrain state. All Aeromancer base spells are Wind element; these interactions apply universally unless a spell's individual entry overrides them.

| Existing Terrain State | What Happens When Wind Spell Hits / Interacts | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **Normal (GROUND)** | Wind displaces the unit; no terrain state change | `GROUND` (unchanged) | Takes spell damage (if applicable); displaced per spell | No terrain alteration — wind acts on units, not ground |
| **ON_FIRE** | Wind fans the fire | `ON_FIRE` (fire spreads faster — immediately spreads to 1 additional adjacent tile in wind direction this turn) | Takes spell damage + if unit is pushed through fire tile: `BURNING` applied | Wind and fire are complementary: the Aeromancer can directionally accelerate Pyromancer fire spread using Gust Strike or Cyclone Blast near fire zones |
| **FLOODED** | Wind disrupts water surface | `FLOODED` (state maintained; conductivity temporarily reduced) | Takes spell damage + `SLOWED` (wave impact; 1 turn) | FLOODED chain-arc range for Electromancer reduced by 1 tile for 1 turn (surface disruption) |
| **ICE_TILE** | Wind carries ice shards | `ICE_TILE` (unchanged; wind cannot melt ice) | Takes spell damage + 6 cold-shard damage from ice fragment spray | Units pushed onto or through ICE_TILE by wind apply the slip check normally |
| **STEAM_CLOUD** | Wind disperses steam rapidly | `GROUND` (steam cloud cleared — wind removes the blinding effect entirely) | BLINDED status from STEAM_CLOUD removed on units in the cloud | The Aeromancer can intentionally clear Pyromancer-created or Hydromancer-fire-interaction steam clouds as a utility action |
| **TOXIC_TERRAIN** | Wind disperses airborne toxins | `GROUND` (TOXIC_TERRAIN removed — wind clears the poison ground) | Units on the cleared tile: 1 POISONED stack removed | Wind uniquely removes TOXIC_TERRAIN entirely — the Aeromancer can clear Toximancer ground investment |
| **CHARGED** | Wind carries the charge through air | `GROUND` (CHARGED tile discharged by atmospheric dispersion) | Takes spell damage + the arc fires in the wind direction (not radially — the wind carries the discharge directionally along the push path) | Wind-directed arc is a unique Electromancer interaction: the discharge travels in the wind direction rather than radially, potentially reaching units the standard arc would not |
| **MUD** | Wind dries surface mud | `MUD` → `GROUND` (wind partially dries the mud surface after 1 turn of exposure; not immediate) | Takes spell damage + SLOWED from existing MUD maintained during wind turn | The drying effect is delayed by 1 turn — the Aeromancer cannot instantly clear MUD, only accelerate its natural expiry |
| **OBSIDIAN** | Wind cannot affect obsidian | `OBSIDIAN` (unchanged) | Takes spell damage (if applicable) | Obsidian is immovable; wind displacement of units off or into obsidian walls triggers collision normally |
| **OVERGROWTH** | Wind parts the growth briefly | `OVERGROWTH` (unchanged; growth is resilient to wind) | Takes spell damage; ROOTED status removed from unit (wind tears the vines free) | Wind can free ROOTED units — the Aeromancer can rescue ROOTED allies from Floramancer vine traps |
| **BLUSTERED** (Aeromancer-created) | Wind reinforces existing air current | `BLUSTERED` (duration refreshed by 1 turn) | Takes spell damage; displaced per the wind spell | Stacking wind effects in a zone increases chaos — BLUSTERED extension from Wind Wall or Updraft placement in the same area |

### Displacement Mechanics Reference Table

All Aeromancer push/displacement effects follow these rules universally:

| Condition | Effect on Displacement |
|---|---|
| Target has `HEAVY` status | Push distance –1 tile (2-tile push becomes 1-tile; 1-tile push = 0, no displacement) |
| Target has `WEIGHTLESS` status | Push distance +1 tile (2-tile push becomes 3-tile) |
| Target hits a wall or obstacle mid-push | Displacement stops; collision damage = remaining tiles × 6 HP |
| Target hits another unit mid-push | Displacement stops for both; both take 6 HP collision; secondary unit is pushed 1 tile in push direction if passable |
| Target pushed off elevated edge | Fall damage = fall_distance × 8 HP (applied after push resolves) |
| Target lands on `ICE_TILE` | Slip check: on slip, 1 additional tile in push direction involuntarily |
| Target lands on `ON_FIRE` | `BURNING` applied (5 HP/turn) |
| Target lands on `TOXIC_TERRAIN` | `POISONED` (1 stack) applied |
| Target lands on `CHARGED` tile | Charged arc triggers immediately from landing tile |
| Target lands in `FLOODED` zone | Unit becomes `WET` (1 turn) — Electromancer chain prime |

### Terrain States Beneficial to the Aeromancer

| State | Benefit |
|---|---|
| `ELEVATED` tiles (Geomancer-created) | All elevated edges are potential fall-damage landing zones for Aeromancer push targets; more elevation = more fall damage per push |
| `ON_FIRE` tiles | Gust Strike can push enemies into them (BURNING on landing); Cyclone Blast fans them directionally |
| `ICE_TILE` tiles (Cryomancer-created) | Push onto ICE_TILE triggers slip check — effectively extends push range by 1 tile on a slip result |
| `CHARGED` tiles | Push enemies onto CHARGED tiles for immediate arc discharge on landing |
| `TOXIC_TERRAIN` | Push enemies into TOXIC_TERRAIN for POISONED stack application |
| `FLOODED` tiles | Push enemies into FLOODED for WET status — direct Electromancer chain setup |

### Terrain States Hazardous to the Aeromancer

| State | Hazard |
|---|---|
| `ON_FIRE` | No fire immunity; 5 HP/turn DoT. At 80 HP with 0 armor, the Aeromancer's HP pool is consumed rapidly by fire terrain DoT |
| `CHARGED` tiles | Arc damage applies normally; with 0 armor, chain arcs threaten significantly |
| `MUD` | Movement cost +2 penalizes the Aeromancer's primary defensive tool (5-tile move range) by reducing effective tiles per AP |
| `ICE_TILE` | Slip checks apply to the Aeromancer; with 0 armor and 80 HP, an involuntary slide into hazard terrain is potentially lethal |
| Any DoT terrain | With 0 armor and 80 HP, any passive ground damage is a high-percentage threat; the Aeromancer relies on never standing in hazardous terrain, which its 5-tile move range makes feasible but requires constant attention |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

#### Variant A: Hurricane Strike (replaces Gust Strike) — +20 pts

**Description:** Gust Strike is replaced by Hurricane Strike — a more powerful directional blast. Hurricane Strike deals 16 damage (up from 10) and pushes the target 3 tiles instead of 2. If the target is WEIGHTLESS, the push distance is 4 tiles. If the target is HEAVY, the push distance is 2 tiles (reduced from 3 rather than from 2). AP cost is 3 AP; cooldown is 1 turn.

**Trade-off:** Significantly higher push distance (2 → 3 tiles, enabling fall-damage plays from greater distances) at the cost of a 1-turn cooldown and +1 AP. The extra push tile dramatically increases the range of hazard-tiles that can be reached from a given push. Cannot double-cast in one activation at 3 AP vs. the base Gust Strike's 2 AP double-cast. Best for Geomancer/Cryomancer team comps where elevated terrain and fall hazards are prominent.

#### Variant B: Maelstrom (replaces Cyclone Blast) — +25 pts

**Description:** Cyclone Blast is replaced by Maelstrom — a pulling rather than pushing AoE. Maelstrom targets a point within 5 tiles; all units within a 3-tile radius are pulled 2 tiles toward the center point. Units pulled into the center tile (if multiple units converge) collide — each takes 12 collision damage. All tiles in the 3-tile radius become `BLUSTERED` (2-turn duration). Base damage is 12 to all units in radius. AP cost remains 4 AP; cooldown is 2 turns.

**Trade-off:** Pull rather than push — collects enemies into a cluster (ideal for follow-up AoE from Electromancer, Pyromancer, Sonimancer, or Gravimancer) rather than dispersing them. Loses the formation-scatter function of Cyclone Blast but gains formation-gathering capability. Fundamentally shifts the Aeromancer's playstyle: instead of denying enemy clustering, it forces clustering for allied AoE follow-up.

#### Variant C: Shear Wind (replaces Wind Wall) — +20 pts

**Description:** Wind Wall is replaced by Shear Wind — a directional cutting blast that travels in a line (5-tile range) and deals 20 damage to all units in the line, pushing each 1 tile perpendicular to the line direction (lateral displacement). Each unit in the line is independently pushed perpendicular — to the left or right of the line, Aeromancer's choice at cast for each unit individually. Units pushed by Shear Wind into a wall take collision damage (1 remaining tile × 6 HP). Units pushed onto hazard tiles receive those terrain effects. AP cost is 3 AP; cooldown is 1 turn. No WIND_WALL terrain feature is created.

**Trade-off:** Replaces the persistent defensive barrier of Wind Wall with a direct-damage lateral-displacement line attack. Higher throughput damage, precise individual lateral control, but no projectile deflection utility and no 3-turn persistent feature. Best for Aeromancers prioritizing offensive displacement over defensive barrier maintenance.

---

### Passive Traits

#### Passive A: Gale Form — +20 pts

**Description:** The Aeromancer's Move Range increases from 5 to 6. Additionally, the Aeromancer ignores movement cost penalties from all terrain states (MUD +2 cost, FLOODED +1 cost, ICE_TILE +1 to enter elevated terrain — all waived). The Aeromancer moves as if all terrain is standard GROUND for movement purposes. Gale Form also makes the Aeromancer immune to the ROOTED status — wind pulls it free of any attempted vine/ground restriction.

**Trade-off:** Pure mobility enhancement. Does not improve the Aeromancer's offensive output or survivability. Best in lists where the Aeromancer needs to reach extreme repositioning distances in a single activation to apply displacement from a critical angle — or in maps with complex terrain where movement penalties would significantly constrain the Aeromancer's defensive positioning.

#### Passive B: Windseeker — +25 pts

**Description:** Whenever the Aeromancer pushes a unit into a hazard tile (ON_FIRE, TOXIC_TERRAIN, CHARGED, PIT, or off an elevated edge for fall damage), the Aeromancer recovers 2 AP at the end of that push resolution. Maximum 2 AP can be recovered per activation from this passive. In effect: two successful hazard-landing pushes in an activation restore enough AP for one additional Quick spell or one additional tile of movement.

**Trade-off:** High-skill passive that rewards exact hazard placement reads. A player who consistently routes pushes into hazard tiles gains significant AP efficiency over time. A player who pushes into open ground gets no value from Windseeker. This is the highest mastery-floor upgrade in the Aeromancer's kit.

**Synergy note:** Windseeker pairs maximally with Hurricane Strike (3-tile push range = larger hazard-landing opportunity) and with Geomancer elevated terrain (more fall-hazard edges to push into).

#### Passive C: Storm Shroud — +15 pts

**Description:** The Aeromancer cannot be targeted by projectile spells on any turn in which it has moved 3 or more tiles. If the Aeromancer moves 3+ tiles in its activation, it generates a wind-blur evasion effect that causes all projectile spells aimed at it to miss for the rest of that turn. Non-projectile spells (AoE ground targets, self-centered AoE) still target normally. This passive incentivizes high-mobility play — the Aeromancer is safest when it moves the most.

**Trade-off:** Significant defensive value in a Mancer with 0 armor and 80 HP. Missing projectile spells (Ember Shot, Arc Bolt, Ice Lance, Aqua Lance, Frost Bolt) that would otherwise threaten the Aeromancer is critical for survival. The trade is that the Aeromancer must spend 3+ AP on movement each activation to activate Storm Shroud — reducing its spell-casting AP budget. Best for Aeromancers that primarily cast 2-AP spells (Gust Strike) and use remaining AP for movement.

#### Passive D: Atmospheric Pressure — +20 pts

**Description:** All terrain states created by allied Mancers within 3 tiles of the Aeromancer have their duration extended by 1 turn (Aeromancer's local air pressure slows state expiry). WET tiles last 3 turns instead of 2; ON_FIRE spreads slower but stays active longer (1 additional turn before natural expiry); ICE_TILE lasts 3 turns instead of 1–2; MUD persists 4 turns instead of 3. Additionally, the Aeromancer can, once per activation as a free action (no AP cost), push any terrain state timer by 1 turn — effectively refreshing a single expiring terrain state within 3 tiles without casting a spell.

**Trade-off:** Pure terrain state management utility. Extends the value window of every other Mancer's terrain investment. Best in slow, attrition-based team comps where prolonged terrain pressure is the win condition.

---

### Stat Enhancements

#### Stat A: Tempest Constitution (+15 HP) — +10 pts

**Description:** Max HP increases from 80 to 95. The most important defensive upgrade the Aeromancer can take. At 80 HP with 0 armor, the Aeromancer is eliminated by a single Pillar of Flame (55 + BURNING tick) or two Ember Shots (18 × 2 = 36 HP per turn of damage from DoT). At 95 HP, it survives one additional burst exchange.

**Design note:** This is almost mandatory in any Aeromancer list. The 0-armor, 80-HP baseline is the lowest combined survivability in the roster. Even 15 additional HP meaningfully changes which burst thresholds eliminate the Aeromancer versus leave it alive.

#### Stat B: High Altitude (+1 Move Range) — +10 pts

**Description:** Move Range increases from 5 to 6. Combined with Gale Form passive, the Aeromancer would reach 7 tiles of base move range — the highest in the game by a significant margin. Even without Gale Form, 6 tiles allows extreme repositioning across open maps.

**Design note:** The Aeromancer's move range is already the highest in the roster. This enhancement is most useful for covering very large maps or reaching elevated positions that require 2 sequential movement-heavy turns to access.

---

### Signature Ability

The Signature Ability is unlocked as an additional spell slot — it does not replace any base spell.

#### Signature: Eye of the Storm — +40 pts

| Field | Value |
|---|---|
| **Name** | Eye of the Storm |
| **AP Cost** | 6 AP (entire activation; Aeromancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Self-centered AoE — Aeromancer is the origin |
| **Range** | N/A (self-centered) |
| **AoE Radius** | 5 tiles in all directions from the Aeromancer |
| **Base Damage** | 20 (all units within 5 tiles) |
| **Element** | Wind / Physical |
| **Effects Applied** | A massive cyclone erupts centered on the Aeromancer. All units within 5 tiles take 20 Wind damage. All units within 3 tiles are pushed 3 tiles directly away from the Aeromancer (inner storm — maximum displacement force). Units within 4–5 tiles are pushed 2 tiles away (outer ring — reduced force). All `ON_FIRE` tiles within 5 tiles are fanned — each spreads immediately in the radial-outward direction (fire spreads away from the center). All `TOXIC_TERRAIN` within 5 tiles is dispersed (removed). All `STEAM_CLOUD` within 5 tiles is cleared. The entire 5-tile radius becomes `BLUSTERED` (2-turn duration). The Aeromancer itself is not pushed (eye of the storm — stationary at center). |
| **Special Interactions** | All displacement interactions apply fully to all pushed units (hazard-tile landing, fall damage, collision, ICE_TILE slip). Against `WEIGHTLESS` units in the radius: pushed 4 tiles in inner ring (WEIGHTLESS +1 push). Against `HEAVY` units: pushed 2 tiles in inner ring (HEAVY –1 push). Against `FLOODED` zones: wind stirs the water dramatically but does not remove FLOODED — however, every unit in the flooded zone is pushed outward (off the flooded zone), effectively clearing the flood zone of all enemy units simultaneously. This makes Eye of the Storm the hard counter to Hydromancer Flood Zone + enemy clustering. |

**Design note:** Eye of the Storm is the Aeromancer's "the board resets to what I want" ability. A 5-tile radius displacement affecting every unit on the board within range, simultaneously fanning fire outward, clearing toxic terrain, dispersing steam, and scattering formations — all at once. The 6 AP cost and no-movement constraint make the turn predictable (opponent sees the Aeromancer is stationary and braced), but the radius is so large that positioning outside the 5-tile radius while still being engaged is difficult on most maps. Eye of the Storm is not best used as a burst damage ability — the 20 HP is modest. It is best used as a total formation reset: the Aeromancer identifies a turn when the opponent's formation is dangerous, braces, and scatters it into the terrain hazards the Aeromancer has spent prior turns preparing.

**Synergy note:** Eye of the Storm executed on a board where Pyromancer has established 4–6 ON_FIRE tiles fans every fire zone simultaneously outward from the Aeromancer. If the Aeromancer is positioned centrally with fire on the edges, the fire fans toward the opponent's units — a Conflagration Wave equivalent applied to all fire zones simultaneously.

---

## 6. Faction Synergy

### Best Faction: The Verdant Pact

The Verdant Pact is the Aeromancer's strongest faction match in terms of tactical synergy. Verdant Pact's Terrain Bond grants movement and regen on natural terrain, and the Aeromancer's Updraft zone does not disqualify natural terrain from Terrain Bond (UPDRAFT is an air zone, not a terrain state — the ground beneath it remains natural terrain for Terrain Bond purposes). Pact units inside an UPDRAFT zone on natural terrain receive BOTH Terrain Bond movement bonuses AND WEIGHTLESS immunity to hazardous terrain.

**Practical result:** A Verdant Pact chaff screen in an Aeromancer UPDRAFT zone on MUD or forest tiles is immune to ON_FIRE DoT, TOXIC_TERRAIN POISONED stacks, and ICE_TILE slip (from WEIGHTLESS), while simultaneously gaining Terrain Bond regen and movement bonus. This is the game's most durable forward chaff position in terms of combined passive benefit layers.

**Thornback Sentinels:** When pushed off by an enemy Aeromancer, Thornback Sentinels leave a Thorn Patch on their death tile. The allied Aeromancer can intentionally push Thornback Sentinels into enemy formation positions — not to kill the Sentinels, but to use Gust Strike as a taxi. A Sentinel pushed 2 tiles into an enemy formation grants an instant Thorn Patch adjacent to multiple enemy units (if the Sentinel takes incidental collision damage and falls below HP threshold during the push, the Thorn Patch triggers at the collision point — a calculated sacrifice play).

**Glade Archers:** Glade Archers apply POISONED on hit. Wind Wall deflects incoming enemy projectiles — protecting Glade Archers from ranged counterfire while they accumulate POISONED stacks on priority targets. The Aeromancer creates the safe fire position; the Archers add POISONED for follow-up Pyromancer TOXIC_FIRE combos.

### The Gilded Throne — Precision Displacement

The Gilded Throne's primary Aeromancer synergy is structural: Conscript Spearmen and Iron Vanguard have reliable physical melee attacks. A Gust Strike pushing an enemy unit off a Geomancer elevated platform (fall damage) and landing it adjacent to a waiting Iron Vanguard (melee follow-up) chains positioning, fall damage, and physical strike into a single coordinated sequence. Iron Vanguard in Shield Wall formation has damage reduction bonuses — the Aeromancer keeps enemies off the Shield Wall's position while the Vanguard holds the chokepoint.

Crossbow Corps and Siege Arbalest benefit from Aeromancer Wind Wall: the wind deflects incoming spell projectiles while the Crossbow units fire their physical bolts through the same zone (physical projectiles are not affected by Wind Wall — only spell projectiles are deflected). The Gilded Throne's ranged screen can fire through the Aeromancer's Wind Wall while the Aeromancer's wall blocks enemy Mancer spells attempting to respond.

### The Ashen Covenant — Deathless Advance Support

Grave Husks are slow (base stat before T2 upgrade) but fearless. The Aeromancer's Updraft over a Husk advance makes the Husks WEIGHTLESS — eliminating the normal tactical constraints of slow units. WEIGHTLESS Husks cross ON_FIRE terrain (which would damage other chaff normally) without DoT, advance through ICE_TILE without slip risk, and cross MUD zones without movement penalty. The Husks' inherent Deathless Ranks immunity to morale already makes them resistant to disruption; WEIGHTLESS removes their terrain-traversal limitations.

Wailing Shades (phase-through ranged) do not benefit from Wind Wall deflection (their projectiles phase through physical barriers already). However, the Aeromancer's Cyclone Blast dispersing an enemy formation causes units to scatter — Wailing Shades' Silence aura (tile-based) applies to the new scattered positions, potentially silencing on-death effects across multiple separated targets simultaneously.

---

## 7. Combo Chains

### Combo 1 — The Wind-Fall (Aeromancer + Geomancer) [SIGNATURE DISPLACEMENT COMBO]

**Mancers involved:** Aeromancer + Geomancer

**Step-by-step execution:**

1. **Turn N, Geomancer activates:** Geomancer uses Raise Terrain (5 AP) to create an ELEVATED tile at a tactically central position. The elevated tile provides +1 spell range to whoever stands on it and creates a fall edge.
2. **Turn N+1, both active:** Enemy units move toward the allied formation and some approach or stand on the elevated tile. OR enemy units are near the elevated edge at ground level.
3. **Aeromancer activates:** Gust Strike (2 AP) targets a unit on the ELEVATED tile. Push direction: off the elevated edge. The unit is pushed 2 tiles, 1 of which carries it off the edge. Fall damage: fall_distance × 8 HP (minimum 8 HP for 1-level drop, up to 16 HP for a 2-level drop from Titan's Ascent position).
4. **Collision cascade (if applicable):** If the falling unit collides with another unit at the base level, both take 6 HP collision damage.
5. **Repeat:** The Aeromancer can Gust Strike twice in 4 AP — pushing two separate elevated units off the platform. With Hurricane Strike (+20 pts), the 3-tile push guarantees clearing units from elevated positions even if they're not at the edge.

**Tactical outcome:** Elevated terrain created by Geomancer becomes a death-trap with Aeromancer displacement. Units that climb to the elevated position for its spell-range benefit can be pushed off by a distant Aeromancer that never needed to enter the danger zone. Creates a board state where elevated terrain is dangerous rather than advantageous — forcing the opponent to choose between using elevation (and risking the fall) or staying at ground level (losing the range bonus).

---

### Combo 2 — The Shock Delivery (Aeromancer + Electromancer + Hydromancer)

**Mancers involved:** Aeromancer + Electromancer (and optionally Hydromancer for WET priming)

**Step-by-step execution:**

1. **Setup:** Hydromancer (or accumulated WET terrain) has applied WET to a cluster of targets.
2. **Aeromancer activates:** Cyclone Blast (4 AP) targeted at the WET cluster. Cyclone Blast pushes all WET units toward the outer edges of the AoE (radially outward). HOWEVER — the Maelstrom variant is more powerful here: Maelstrom pulls all units toward the center, clustering them tightly together. Each unit in the pulled cluster is also WET. The cluster is now at a single central point.
3. **Electromancer activates:** Any lightning spell hits any WET unit in the cluster. The chain arc propagates to all adjacent WET units — in the clustered formation, all units are adjacent. Chain arc hits all of them simultaneously.
4. **Result:** STUNNED across the entire cluster from chain arc propagation. The Aeromancer gathered; the Electromancer detonated.

**Without Maelstrom (base Cyclone Blast):** Cyclone Blast scatters the WET cluster outward — chain arc range may not reach all scattered targets. Instead, use Gust Strike to push a non-WET unit into the WET cluster (landing them on WET terrain → they become WET → expand the chain arc's reach by one more WET target).

---

### Combo 3 — The Fire Drive (Aeromancer + Pyromancer)

**Mancers involved:** Aeromancer + Pyromancer

**Step-by-step execution:**

1. **Pyromancer activates:** Scorched Earth or Ember Shot establishes ON_FIRE tiles in a directional formation — a corridor of fire.
2. **Aeromancer activates:** Gust Strike pushes enemy units toward the ON_FIRE zone. On landing in an ON_FIRE tile, unit receives BURNING. Alternatively, Cyclone Blast fans the fire in the AoE outward (fire spreads to 1 additional adjacent tile per ON_FIRE tile in the AoE, in the radial-outward direction — expanding the fire zone in the same turn it pushes enemies toward it).

**Best execution (same turn):** Pyromancer establishes fire; Aeromancer expands it outward toward enemy units AND pushes units into the expanded zone. The fire was 3 tiles wide; after Cyclone Blast fanning, it is 5–7 tiles wide; units pushed 2 tiles now land in the expanded fire zone.

**Wind Wall + fire direction:** Wind Wall placed perpendicular to a fire zone causes ON_FIRE tiles adjacent to the wall to spread in the wall's wind direction. The Aeromancer directs fire spread using the wall — the Pyromancer creates the fire; the Aeromancer steers it without spending additional AP.

---

### Combo 4 — The Weightless Army (Aeromancer + Allied Advance)

**Mancers involved:** Aeromancer + allied chaff / second Mancer

**Step-by-step execution:**

1. **Aeromancer activates:** Updraft (3 AP) centered on the allied formation's advance path. All allies in the zone become WEIGHTLESS (immune to all ground terrain effects).
2. **Allied chaff activate:** Chaff advances through ON_FIRE terrain (no DoT), ICE_TILE (no slip), MUD zones (no movement penalty), TOXIC_TERRAIN (no POISONED stacks). The Aeromancer has created a terrain-immune strike force.
3. **Second Mancer (if any):** The second Mancer's spell effects (Pyromancer fire, Cryomancer ice, Toximancer poison) damage enemies while the WEIGHTLESS allied chaff is immune to those same states. The Aeromancer's UPDRAFT zone is a safe corridor through any terrain environment.

**Tactical impact:** The Aeromancer eliminates the opponent's terrain investment as a relevant factor for the allied advance turn. A Pyromancer-heavy opponent who has spent 3 activations building fire zones finds that the Aeromancer's single 3-AP Updraft makes all that terrain investment zero-effective against the advancing chaff for 3 turns. This is the Aeromancer's highest-value defensive play in attrition matchups.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Aeromancer

| Mancer | Counter Mechanism |
|---|---|
| **Gravimancer** | HEAVY status applied to friendly units makes them resist Aeromancer push (–1 tile). Gravimancer's HEAVY applied to the Aeromancer itself doubles its fall damage (HEAVY: fall damage ×2) — a Geomancer pushing the HEAVY Aeromancer off its own elevated platforms doubles the damage it takes from its own terrain. Additionally, Gravimancer's gravity pulls ignore the Aeromancer's Storm Shroud projectile immunity (gravity pulls are not projectile spells). |
| **Cryomancer** | FROZEN on the Aeromancer eliminates its defensive mobility advantage entirely (FROZEN: skip entire turn). The Aeromancer's core defense is its 5-tile move range and Storm Shroud (requires movement to activate). FROZEN prevents both. At 80 HP with 0 armor, a FROZEN Aeromancer is a prime SHATTER target — 25+ HP from any physical attacker at ×2.5 = near-lethal in one hit. The Aeromancer's worst-case scenario is being FROZEN adjacent to a physical Mancer. |
| **Osteomancer** | CALCIFIED status (applied by Osteomancer) gives the target –2 move AND +15% physical armor. An Aeromancer hit by CALCIFIED loses its primary defensive tool (mobility: 5 tiles → effectively 3) while its 0 armor receives a marginal boost that still leaves it fragile. More critically, Bone constructs (ROOTED, immovable) cannot be displaced by Gust Strike or Cyclone Blast — the Aeromancer's push mechanics are nullified against Osteomancer summons. |

### Warband Compositions That Prey on Aeromancer

| Warband Type | Exploitation |
|---|---|
| **Gravimancer + any Mancer (HEAVY warband)** | HEAVY-buffed units resist push. The Aeromancer's entire displacement toolkit is partially negated by –1 push tile on every affected target. The Gravimancer can selectively HEAVY its highest-priority units, making them resistant to the Aeromancer while leaving expendable units displaceable as distractions. |
| **Cryomancer + physical burst (freeze-shatter focus)** | Cryomancer FREEZES the Aeromancer; the physical attacker SHATTERS it for ×2.5 damage. At 80 HP with 0 armor, the Aeromancer does not survive a SHATTER from any meaningful physical attacker. The Aeromancer must keep 6+ tiles from the Cryomancer at all times, which its 5-tile move range and 6-tile Cryomancer spell range makes difficult. |
| **Ranged-heavy triple-missile warband** | The Aeromancer has 0 armor. Storm Shroud (passive) helps when the Aeromancer moves 3+ tiles but leaves gaps on construction turns (Updraft, Cyclone Blast positioning) when movement is lower. A warband with 3 high-frequency ranged attackers (Wailing Shades, Siege Arbalests, Glade Archers) can chip the Aeromancer to death faster than its repositioning keeps it safe. |

---

## 9. Temperature Interaction Notes

Wind is the coolest element in the roster — every Aeromancer spell applies -5 temperature to affected units. This is modest compared to fire (+10–20) or direct Cryomancer applications (-20 to -25), but it is consistent, costs no additional AP, and is applied on nearly every cast. Over multiple activations, the Aeromancer is a passive cooling force on anything it targets.

### Wind as OVERHEATED Counter

Aeromancer is the fastest way to cool a HOT enemy short of water. Gust Strike (-5 temperature) can nudge a unit at +65 (OVERHEATED, suffering 5 HP/turn BURNING DoT) down to +60 (HOT threshold), removing the BURNING DoT entirely at the cost of 2 AP. This is relevant when an ally Pyromancer has pushed an enemy to OVERHEATED but the team lacks a clean kill — the Aeromancer can strip the DoT off an enemy to prevent a kill that would deny the team a BURNING body for further exploitation. Conversely, if the Aeromancer does NOT want to strip the DoT, it should avoid using wind spells on OVERHEATED targets.

### Wind + FROZEN Combo

A FROZEN SOLID enemy (temperature ≤ -61, FROZEN status, cannot move) is an interesting Cyclone Blast target. The wind displacement does not help the enemy thaw — displacement is positional, not thermal, and does not apply temperature change on its own. However, Cyclone Blast still deals -5 temperature to FROZEN units it hits, cooling them slightly further (irrelevant for the FROZEN threshold, which is already triggered). The primary value is positional: Cyclone Blast can push a FROZEN enemy off elevation for fall damage while they are completely immobilized. A FROZEN unit cannot self-rescue from fall positioning — the Aeromancer can end a FROZEN unit's placement as a tactical concern while they stand helpless.

### Positioning for Thermal Combos

The Aeromancer's primary temperature value is indirect: displacing enemies onto thermally hazardous terrain rather than applying temperature directly. Key displacement targets:

- **Push onto BURNING terrain (+10 temperature/turn):** A unit at +20 (WARM) pushed onto a BURNING tile will reach HOT (+31) in one turn and OVERHEATED (+61) in three to four turns if they cannot escape. The Aeromancer creates the trap; the terrain does the heating.
- **Push away from FROZEN terrain:** If an enemy is self-healing on a cold zone, displacing them off of it stops the temperature accumulation.
- **Push onto FROZEN tiles:** Displacing an enemy onto ICE_TILE or PERMAFROST contributes to their temperature drop each turn they remain there.

Aeromancer players should read the board's thermal hazard distribution before each push — where an enemy lands is as important as the push damage itself.

### Friendly Temperature Play Caution

If an ally Cryomancer is actively working to freeze an enemy down to FROZEN SOLID (≤ -61), and that enemy is currently at -30 (COLD, close to SUPERCOOLED threshold), Aeromancer should **avoid** using Cyclone Blast on that enemy. The -5 temperature from Cyclone Blast accelerates the enemy toward SUPERCOOLED and eventually FROZEN SOLID — which sounds helpful but means the Aeromancer is contributing to the Cryomancer's freeze work, not exploiting it. The correct play is to wait for the enemy to reach FROZEN SOLID (Cryomancer's job) and then use displacement to push the FROZEN target into fall-damage or physical-attacker range.

The exception: if the Aeromancer is using Gust Strike at -5 on a unit at -25 (COLD, approaching SUPERCOOLED at -31), and the team wants to accelerate the SUPERCOOLED threshold for the BRITTLE modifier, the -5 is intentional and the Cryomancer combo is being set up cooperatively.

*End of Aeromancer design document.*

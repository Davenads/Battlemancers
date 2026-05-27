# Crystalomancer — Full Design Document

---

## 1. Tactical Identity

The Crystalomancer is the roster's foremost battlefield architect of light and energy — a Mancer that wins by creating a network of crystal constructs that redirect, store, and amplify any element that passes through them. Where the Geomancer builds walls to block and funnel, the Crystalomancer builds prisms to redirect and multiply. Its three construct types — Crystal Node (energy storage), Crystal Wall (movement block and beam refractor), and Crystal Prism (beam redirector) — interact with the entire roster's spell library in ways that dramatically exceed what a single Mancer could do alone. The Crystalomancer's primary mechanic is STORED ENERGY: it builds a Node, an ally fires a powerful elemental spell into it, and the Crystalomancer releases that stored element as a second AoE from the Node's position. Every Crystal Node the Crystalomancer places is an offer to every other Mancer on the team: put your most damaging spell here, and I will fire it again for you.

Playing the Crystalomancer well demands communication with the team's intent. Unlike most Mancers whose kit is self-contained, the Crystalomancer's ceiling is defined by how powerful the ally spells are that it stores. A Pyromancer Pillar of Flame stored in a Crystal Node and released as a 70% AoE burst (0.7 × 55 = 38 HP AoE) across a packed formation is more efficient than the Crystalomancer's own direct damage output. The REFRACTION mechanic converts line-of-sight limitations into opportunities — beam spells that cannot reach a target around a corner can be aimed at a Crystal Prism and redirected. This makes the Crystalomancer a force multiplier for Photomancer, Electromancer, and any other spell that travels in a line. Its weakness is construct fragility: Crystal Nodes, Walls, and Prisms all have 15–20 HP and die quickly to any AoE that sweeps across them. The Crystalomancer must rebuild frequently, and each rebuild costs AP that could have been spent on direct action.

**Primary win condition:** The Crystalomancer wins by establishing a network of 2–3 active constructs during the early turns, having allies charge Crystal Nodes with their most powerful spells, and releasing stored charges at maximum-impact moments. Secondary win condition: Crystal Prism refraction sequences that allow line spells to bypass cover and reach otherwise-protected enemy positions.

**Core weakness:** Crystal constructs have 15–20 HP and die to any AoE that hits the tile they occupy. A single Scorched Earth (Pyromancer) or Earthen Smash (Geomancer) that covers the Node tile destroys the Crystalomancer's setup investment. Against AoE-heavy opponents, the Crystalomancer is constantly rebuilding rather than leveraging its stored charges. Additionally, its own direct damage output is the lowest of any Mancer in the roster — the Crystalomancer is entirely dependent on the value of its network. If that network is destroyed, the Crystalomancer is a low-damage repositioner with no independent kill threat.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 95 | Below average; the Crystalomancer works at range behind its own constructs |
| **Move Range** | 3 tiles per activation | Modest; construct placement requires positioning but not constant movement |
| **Base Armor** | 1 | Minimal; Crystal Walls are the Crystalomancer's armor in practice |
| **Spell Range** | 5 tiles (base) | Medium; construct placement and Node trigger both operate at this range |
| **AP per Activation** | 6 | Standard; movement costs 1 AP per tile |
| **Element** | Crystal | All base spells deal Crystal/Physical damage; crystal interactions are unique to this element |

**AP budget example:** With 6 AP, the Crystalomancer can move 1 tile (1 AP) and place a Crystal Node (2 AP) and Crystal Prism (2 AP) while firing a Crystal Shard (1 AP), or move 2 tiles and release a stored Node charge (3 AP) and place a Crystal Wall (3 AP).

---

## 3. Base Spell Kit

The Crystalomancer's four base spells cover distinct crystal functions:
- **Crystal Shard** — repeatable single-target damage; cheapest direct attack
- **Place Construct** — terrain placement; deploys Crystal Node, Crystal Wall, or Crystal Prism
- **Energy Release** — triggers a stored Crystal Node; the primary combo-with-allies mechanic
- **Crystal Cascade** — heavy AoE that uses connected crystal constructs as relay points

---

### Spell 1: Crystal Shard

| Field | Value |
|---|---|
| **Name** | Crystal Shard |
| **AP Cost** | 1 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (projectile — travels in a line; can bounce off Crystal Prisms in its path) |
| **Range** | 6 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 12 |
| **Element** | Crystal / Physical |
| **Effects Applied** | Deals 12 physical damage. If the Crystal Shard's travel path passes through a Crystal Prism, the projectile bounces at 90 degrees toward the nearest valid target within 3 tiles of the Prism (same as all REFRACTION interactions). If the Crystal Shard hits a Crystal Node, it does not charge the Node (Shards do not store energy — only elemental spells from other Mancers or the Crystalomancer's own elemental spells do). If the target has `BRITTLE_ARMOR` (Cryomancer-applied): Crystal Shard triggers BRITTLE_ARMOR for +50% damage (12 × 1.5 = 18 HP). |
| **Special Interactions** | Against `ICE_TILE`: Crystal shard impacts with ice, sending ice-shard fragments to adjacent tiles — all adjacent units to the impact take 5 HP cold-shard splash. Against `RESONATING` tile (Sonimancer-created): the crystal projectile interacts with sonic resonance — Crystal Shard deals 20 damage (instead of 12) when hitting a RESONATING tile, as crystal resonance amplifies with sonic resonance. Against `CRYSTAL` terrain (same Crystalomancer's own nodes and walls): Crystal Shard cannot damage Crystalomancer-owned constructs (friendly fire protection on constructs; enemy Crystalomancer constructs are valid targets). |

**Design note:** Crystal Shard is intentionally the Crystalomancer's lowest-investment tool — at 1 AP with no cooldown, it is a filler between construct placement and Node release. The bounce mechanic (Crystal Shard deflects off Crystal Prisms) allows the Crystalomancer to fire indirectly around corners once a Prism is placed — a 12 HP indirect projectile that ignores LoS requirements (as long as a Prism is positioned correctly). This is a niche but consistent benefit: once the crystal network is established, Crystal Shard becomes an LoS-bypass tool.

**Spell answers YES to (design rule check):**
1. Synergizes with construct placement (bounces off Crystal Prisms) — YES
2. Triggers BRITTLE_ARMOR (Cryomancer setup) — YES
3. Creates indirect targeting option (bouncing off Prisms) — YES
4. Skill expression: Prism angle calculation for bounce to reach protected targets — YES

---

### Spell 2: Place Construct

| Field | Value |
|---|---|
| **Name** | Place Construct |
| **AP Cost** | 2 AP (Crystal Node); 2 AP (Crystal Prism); 3 AP (Crystal Wall — 3 tiles long) |
| **Cooldown** | 1 turn per construct type (Crystal Node 1-turn cooldown; Crystal Prism 1-turn cooldown; Crystal Wall 2-turn cooldown) |
| **Targeting Type** | Terrain Placement — targets a specific tile or line of tiles |
| **Range** | 4 tiles (to nearest placed tile) |
| **AoE Radius** | N/A for Node and Prism; Crystal Wall is a 3-tile line |
| **Base Damage** | 0 (placement spell; no direct damage) |
| **Element** | Crystal |
| **Effects Applied** | Places one of three crystal constructs on the target tile(s). Maximum 3 total constructs active simultaneously (combined total of all types). If the Crystalomancer places a 4th construct, the oldest existing construct collapses (it shatters, dealing 10 AoE damage in 1 tile radius and disappearing). Constructs have limited HP — if damaged beyond their HP pool they shatter identically (10 AoE damage in 1 tile radius). **Crystal Node:** 1-tile construct; 15 HP; stores the element of the last elemental spell to hit it (any element from any Mancer); visible to both players as a glowing crystal; one charge at a time. **Crystal Prism:** 1-tile construct; 15 HP; redirects any beam/line spell that passes through its tile at 90 degrees toward the nearest valid target (player-chosen 90-degree direction at Prism placement time); also bounces Crystal Shard projectiles. **Crystal Wall:** 3-tile line construct; 20 HP per tile; blocks movement but not LoS — units cannot pass through a Crystal Wall but spells can travel through it normally; sonic spells propagate through Crystal Wall with amplification (sonic resonance in crystal structure = +5 HP per tile the sonic spell passes through the wall). |
| **Special Interactions** | Crystal Node HP thresholds: at 15+ HP, functioning normally; at 1–14 HP (damaged), still stores energy but releasing the charge also destroys the Node (unstable crystal). At 0 HP: shatters as described. Crystal Prism on `CHARGED` terrain: the Prism stores electrical charge in its structure — the Prism's refraction adds 8 Lightning damage to the next spell it bounces (in addition to that spell's normal damage). Crystal Wall adjacent to `RESONATING` tile: sonic energy from RESONATING propagates into the Wall structure — if a sonic spell hits the Wall, all connected Wall tiles simultaneously resonate at the end of that turn, dealing 8 HP to units adjacent to any Wall tile. |

**Design note:** Place Construct is the Crystalomancer's defining action. Every activation that begins without 3 active constructs is an activation where the Crystalomancer should consider whether to place before or after spending AP on other actions. The 3-construct limit forces prioritization: Node for energy storage, Prism for refraction coverage, Wall for movement denial — the Crystalomancer typically cannot maintain all three types simultaneously. The AP costs are deliberately low (2 AP each) to allow the Crystalomancer to place and then act in the same activation.

**Spell answers YES to (design rule check):**
1. Creates terrain features (Crystal Node, Prism, Wall) — YES
2. Synergizes with the entire roster (any elemental Mancer can charge a Node) — YES
3. Blocks movement (Crystal Wall), redirects spells (Crystal Prism), stores energy (Crystal Node) — YES
4. Skill expression: network geometry design; Prism angle selection for refraction coverage — YES

---

### Spell 3: Energy Release

| Field | Value |
|---|---|
| **Name** | Energy Release |
| **AP Cost** | 3 AP |
| **Cooldown** | 0 (usable every activation; once per active charged Node) |
| **Targeting Type** | Ground Target — triggers a specific Crystal Node the Crystalomancer can see |
| **Range** | 5 tiles (to the Crystal Node being triggered; Node must be within this range) |
| **AoE Radius** | 2 tiles (centered on the Crystal Node's position) |
| **Base Damage** | Variable — 70% of the base damage of the stored spell, applied as that spell's element in a 2-tile AoE |
| **Element** | Varies (matches the element of the stored spell) |
| **Effects Applied** | The Crystal Node releases the stored element as a 2-tile AoE burst centered on itself. The element's standard terrain interactions apply (fire release = ON_FIRE tiles; ice release = ICE_TILE + CHILLED; lightning release = chain arc to adjacent WET/ICE units; etc.). The Node is consumed on release (destroyed after discharge; must be rebuilt). The damage is 70% of the stored spell's base damage, applied as AoE to all units within 2 tiles of the Node. If the stored spell had secondary effects (e.g., Pyromancer Pillar of Flame's BURNING status), those secondary effects are included in the release at reduced probability: each secondary effect has a 70% chance to apply per target hit. |
| **Special Interactions** | Against a charged Node that has been damaged (1–14 HP): release is still valid, but the unstable crystal adds 15 explosive shrapnel damage to all units within 1 tile of the Node (in addition to the stored element release). Against a Node on a `RESONATING` tile (Sonimancer-seeded): the sonic resonance amplifies the crystal release — stored element damage increases to 85% (not 70%) of base spell damage. Against a Node on `CHARGED` terrain: electrical charge in the tile adds 12 Lightning damage to the release burst. Against releasing stored Electromancer lightning: the 2-tile AoE lightning release chains to all WET or CHARGED units within the 2-tile radius (the same chain mechanic that applies to direct Lightning spells; Node acts as a lightning chain point). |

**Design note:** Energy Release is the Crystalomancer's primary value proposition. The mechanic is simple to explain and complex to execute: an ally fires an expensive, powerful spell into the Crystal Node (intentionally or as part of a planned combination), and the Crystalomancer then fires that spell again for 3 AP as a 2-tile AoE. A Pillar of Flame (55 HP base) stored in a Node releases at 38 HP AoE (55 × 0.70) across a 2-tile radius — a 3 AP spell that deals Pyromancer-tier AoE damage at reduced scale. The Crystalomancer is a repeater: it takes the most powerful element that entered the battlefield and applies it a second time, at lower power, in a location the Crystalomancer controls (the Node's position) rather than the original caster's target. This creates a second-strike capability from every powerful spell cast by any allied Mancer.

**Spell answers YES to (design rule check):**
1. Applies terrain state (release element creates terrain states matching the stored element) — YES
2. Exploits ally spell investment (repeats ally spells at 70% power) — YES
3. Synergizes with every elemental Mancer in the roster — YES
4. Skill expression: Node placement for maximum AoE coverage on release; timing the release relative to the stored charge value — YES

---

### Spell 4: Crystal Cascade

| Field | Value |
|---|---|
| **Name** | Crystal Cascade |
| **AP Cost** | 4 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Ground Target — targets one of the Crystalomancer's active constructs; the cascade propagates from that construct to all connected crystal constructs within 3 tiles |
| **Range** | 4 tiles (to the first target construct) |
| **AoE Radius** | 1 tile around each construct in the cascade chain |
| **Base Damage** | 20 (at initial target construct's position; 15 at each subsequent chained construct) |
| **Element** | Crystal / Physical |
| **Effects Applied** | Crystal energy cascades through the Crystalomancer's network. Starting at the targeted construct, energy propagates to every other Crystalomancer construct within 3 tiles of each prior construct in the chain. Each construct in the chain pulses a 1-tile AoE: 20 HP at the first construct, 15 HP at each subsequent construct in the chain. Units within 1 tile of any construct in the chain take the appropriate damage. Each construct that participates in a Crystal Cascade temporarily gains increased HP (10 HP added to their HP pool until the Crystalomancer's next activation) from the energy reinforcement — the cascade also repairs and reinforces the network. Crystal Nodes that participate in the cascade release a fraction of their stored charge (if any): 20% of stored element damage fires as an additional hit at the Node's location alongside the cascade pulse. |
| **Special Interactions** | Against a network with 3 active constructs: the full cascade hits 3 locations simultaneously — 20 HP at first, 15 HP at second, 15 HP at third. Maximum total direct damage from a 3-construct network: 50 HP distributed across the three sites, each with a 1-tile AoE radius. Against constructs adjacent to `RESONATING` terrain: sonic resonance amplifies the cascade pulse at that construct — the pulse at that location deals 2× damage (30 HP at cascade point instead of 15). Against constructs adjacent to `CHARGED` terrain: the cascade adds an arc discharge at that construct's location — 10 Lightning damage chains to adjacent WET units. Against Crystal Walls in the cascade path: the wall acts as both a cascade relay and a structural battery — the Wall pulse hits units on both sides of the Wall simultaneously (2 HP damage per adjacent unit per Wall tile from the energy pulse traveling through the wall material). |

**Design note:** Crystal Cascade is the Crystalomancer's "I've built something and now I use it" payoff spell. Its value scales directly with how many constructs are active and how well-positioned they are. A single construct = 20 HP pulse at 4 AP (poor efficiency). Three constructs in a chain = 50 HP distributed AoE across three locations + construct repair + fractional Node releases — meaningful AoE for 4 AP across multiple tactical positions. The construct-reinforcement effect (temporary +10 HP) rewards the Crystalomancer for using Crystal Cascade proactively: a network hit by Crystal Cascade is harder to destroy this turn, making the cascade turn a defensive network maintenance action as well as an offensive pulse.

**Spell answers YES to (design rule check):**
1. Exploits existing terrain features (uses constructs as relay points) — YES
2. Repairs terrain features (+10 HP to constructs) — YES
3. Applies AoE damage across multiple positions simultaneously — YES
4. Synergizes with Sonimancer (RESONATING amplification), Electromancer (CHARGED tile arc discharge) — YES
5. Skill expression: construct positioning for maximum cascade coverage; chain geometry calculation — YES

---

## 4. Terrain Interaction Table

### Crystal Spell Impact on Existing Terrain States

The following describes what happens when any Crystalomancer spell or construct interaction strikes a tile in the listed terrain state. Crystal interactions apply universally unless a spell's individual entry overrides them.

| Existing Terrain State | What Happens When Crystal Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **Normal (GROUND)** | Crystal energy passes through; minor crystallization of surface particles | `GROUND` (unchanged) | Takes spell damage | No terrain state change; crystal spells do not transform basic ground |
| **RESONATING** | Crystal resonates with sonic resonance — amplification chain | `RESONATING` (maintained; crystal energy extends duration by 1 turn) | Takes spell damage + 2× from resonance amplification | RESONATING duration extended; Crystal Cascade on RESONATING tile deals 2× damage; Sonimancer + Crystalomancer resonance cascade is active |
| **CHARGED** | Crystal structure captures electrical charge | Construct on tile stores CHARGED state (Prism adds 8 Lightning to bounced spells; Node adds 12 Lightning to Energy Release) | Takes spell damage + 10 Lightning discharge from capacitor | CHARGED terrain empowers crystal constructs; Electromancer + Crystalomancer synergy trigger |
| **ON_FIRE** | Fire cracks crystal structures — dangerous interaction | Construct on tile loses 8 HP per turn while ON_FIRE (fire weakens crystal integrity); Crystal Shard through ON_FIRE tiles deals 8 bonus fire damage | Takes spell damage + BURNING from fire contact | Crystal constructs on ON_FIRE tiles are at risk of destruction; Crystalomancer should avoid placing constructs on fire tiles |
| **ICE_TILE** | Ice and crystal share structural resonance — reinforcement | Construct on ICE_TILE gains +5 HP; Crystal Shard through ICE_TILE deals 5 cold splash to adjacent units | Takes spell damage + ice-shard splash (5 HP) to adjacent units | ICE_TILE reinforces crystal constructs; Cryomancer + Crystalomancer naturally protect each other's structures |
| **WET** | Water conducts crystal energy slightly | `WET` (unchanged) | Takes spell damage; no additional effect | Minor interaction; WET tiles do not meaningfully interact with crystal spells |
| **FLOODED** | Crystal energy fragments underwater — reduced range | Crystal Shard through FLOODED: loses 2 tiles of effective range (range 4 instead of 6 through water); Node release on FLOODED tile: AoE radius reduced to 1 tile (water absorbs crystal energy) | Takes spell damage –20% (water absorption) | Flooded terrain weakens crystal projections; Crystalomancer should position Nodes away from flood zones |
| **TOXIC_TERRAIN** | Crystal structure repels toxic particles | Crystal constructs on TOXIC_TERRAIN are immune to poison degradation (the crystalline surface rejects toxic absorption) | Takes spell damage; crystal construct on this tile takes no DoT from toxic terrain | Unique construct immunity — Crystalomancer can place Nodes in TOXIC_TERRAIN safely |
| **OBSIDIAN** | Crystal shatters against obsidian's density | Crystal Shard: 0 damage (shattered on impact); Crystal Prism cannot redirect through OBSIDIAN (deflection absorbed) | No damage delivered; Crystal Shard wasted | OBSIDIAN is the Crystalomancer's hard counter terrain — spells cannot pass through it |
| **PERMAFROST** | Crystal resonates with deep-freeze structure | Crystal construct on PERMAFROST gains passive cold aura: adjacent units take 3 HP cold/turn | Takes spell damage + `CHILLED` | PERMAFROST empowers crystal constructs with a cold radiation aura |
| **MUD** | Crystal construct sinks into mud — instability | Crystal construct placed in MUD has reduced HP (10 HP instead of 15/20); sinks 1 turn after placement unless the Crystalomancer places it in a non-MUD tile | Takes spell damage | MUD-placed constructs are fragile; Crystalomancer should avoid MUD zones |
| **STEAM_CLOUD** | Crystal refracts through steam — diffused scatter | Crystal Shard through STEAM_CLOUD: scatters into 3 mini-shards, each dealing 5 HP in random directions within 2 tiles of exit point | Takes 5 HP from mini-shard scatter (random) | Steam diffuses crystal projectiles; Prism behind STEAM_CLOUD cannot aim the bounce precisely — destination randomized |
| **OVERGROWTH** | Crystal pierces organic matter without deflection | `OVERGROWTH` (unchanged; crystal spells pass through) | Takes spell damage; no additional effect | Crystal spells are not blocked or deflected by OVERGROWTH — they pierce vegetation |

### REFRACTION Mechanic (Extended)

The REFRACTION mechanic applies to any beam or line-traveling spell (not just Crystalomancer spells) that passes through a Crystal Prism:

- **Eligible spell types:** Any spell with Targeting Type "Single Target (projectile)" or "Line" that travels in a straight path
- **Refraction trigger:** When such a spell passes through a Crystal Prism tile on its travel path
- **Refraction result:** The spell's path bends 90 degrees at the Prism's position. The Crystalomancer player chooses the 90-degree bounce direction when placing the Prism (it is set at placement time, not at cast time)
- **Bounce target:** The bounced spell travels in the new direction for up to 3 tiles, hitting the first valid target or tile in that direction
- **Damage on bounce:** Full spell damage (no reduction from refraction — the crystal amplifies, not absorbs)
- **Multi-Mancer applications:** Photomancer light beams, Electromancer Arc Bolts, enemy Pyromancer Ember Shots (yes — enemy spells are also refracted, which can be turned to the Crystalomancer's advantage)

**Spells confirmed to refract through Crystal Prisms:**
- Electromancer Arc Bolt — bounces and chains to adjacent WET units at bounce destination
- Photomancer Illuminate beam — bounces and applies ILLUMINATED to bounce target
- Pyromancer Ember Shot — bounces (even enemy Ember Shots can be turned against their caster's allies)
- Crystalomancer Crystal Shard — self-bouncing
- Necromancer Bone Bolt — bounces, applying DEATH_MARK at bounce destination
- Gravimancer Graviton Bolt — bounces, applying HEAVY at bounce destination

### Crystal Constructs: HP, Shattering, and Rebuild

| Construct | HP | Shattering AoE | Rebuild Cost | Notes |
|---|---|---|---|---|
| Crystal Node | 15 HP | 10 HP in 1-tile radius | 2 AP, 1-turn cooldown | Shatter releases partial stored charge (50% release damage) |
| Crystal Prism | 15 HP | 10 HP in 1-tile radius | 2 AP, 1-turn cooldown | Shatter adds 1 random refraction bounce in a random direction (8 HP; uncontrolled) |
| Crystal Wall | 20 HP per tile | 10 HP in 1-tile radius per tile | 3 AP, 2-turn cooldown | Each tile shatters independently when its HP pool reaches 0 |

### Terrain States Beneficial to the Crystalomancer

| State | Benefit |
|---|---|
| `ICE_TILE` | Crystal constructs on ICE_TILE gain +5 HP (ice reinforces crystal); Cryomancer terrain naturally protects Crystalomancer infrastructure |
| `RESONATING` tiles | Crystal Cascade through RESONATING tiles deals 2× damage; Sonimancer pre-seeding enables Crystalomancer burst; RESONATING extends its own duration near crystal constructs |
| `CHARGED` terrain | Crystal constructs capture CHARGED state — Prisms add 8 Lightning to bounced spells; Nodes add 12 Lightning to Energy Release; Electromancer CHARGED terrain directly powers the crystal network |
| `TOXIC_TERRAIN` | Crystal constructs are immune to poison degradation — the only terrain type where constructs take no environment-based damage; useful in Toximancer-heavy maps |

### Terrain States Hazardous to the Crystalomancer

| State | Hazard |
|---|---|
| `ON_FIRE` tiles | Crystal constructs on ON_FIRE tiles lose 8 HP per turn — a Pyromancer opponent can systematically destroy the crystal network by seeding fire under constructs |
| `OBSIDIAN` | Crystal Shard and Prism refraction cannot penetrate OBSIDIAN; Geomancer Obsidian walls create blind spots in the crystal network that Crystal Prisms cannot route around |
| `MUD` | Constructs placed in MUD have 10 HP instead of 15/20 — reduced durability in Hydromancer or Geomancer terrain-heavy matches |
| `STEAM_CLOUD` | Crystal Shard projectiles scatter randomly through STEAM_CLOUD; Crystal Prism bounce destinations become unpredictable through steam |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

Spell variants replace a base spell entirely with a more powerful or specialized version.

#### Variant A: Crystal Volley (replaces Crystal Shard) — +15 pts

**Description:** Crystal Shard is replaced by Crystal Volley — three simultaneous crystal projectiles fired in a narrow 30-degree spread. Each projectile deals 8 damage independently (24 HP total if all three hit the same target; 8 HP if they spread). Each projectile bounces off Crystal Prisms independently, potentially hitting three different targets from one Prism. AP cost is 2 AP; cooldown is 1 turn.

**Trade-off:** Triple-projectile spread for multiple simultaneous Prism bounces at the cost of Shard's no-cooldown accessibility. Best for Crystalomancers with 2+ Crystal Prisms active — each volley can bounce off multiple Prisms in sequence for complex multi-target coverage.

#### Variant B: Overcharge (replaces Energy Release) — +25 pts

**Description:** Energy Release is replaced by Overcharge — an enhanced release that fires at 100% of the stored spell's base damage (up from 70%) in a 2-tile AoE, but immediately destroys the Crystal Node after firing (whether or not the Node is already at low HP). Additionally, Overcharge adds a crystal shard burst on top of the stored element: 15 physical damage in the 1-tile radius centered on the Node, in addition to the stored element's AoE. AP cost is 4 AP; cooldown is 1 turn.

**Trade-off:** Full stored spell power (100%) + bonus physical burst at higher AP cost and guaranteed Node destruction. Best for Crystalomancers built around single high-value Node charges rather than persistent sustained networks — charge once, fire once at full power, rebuild.

---

### Passive Traits

#### Passive A: Crystalline Lattice — +20 pts

**Description:** All Crystalomancer constructs have their HP increased by 10 (Crystal Node: 25 HP; Crystal Prism: 25 HP; Crystal Wall: 30 HP per tile). Additionally, when a construct is destroyed (reaches 0 HP), the shatter burst's radius increases from 1 tile to 2 tiles, dealing 15 HP to all units in the 2-tile radius (up from 10 HP in 1 tile). Sturdy constructs shatter more dramatically.

**Trade-off:** Significantly more resilient network (25 HP constructs survive two standard AoE hits instead of one) and amplified shatter burst when they finally fall. Best in matchups against AoE-heavy opponents (Pyromancer, Geomancer) that specifically target constructs.

**Synergy note:** Crystalline Lattice combined with Crystal Cascade's +10 HP repair effect means a Crystalomancer that uses Crystal Cascade once per 3-turn cooldown maintains its constructs at 25–35 HP throughout the match — extremely durable against sustained attack.

#### Passive B: Crystal Conduit — +25 pts

**Description:** The Crystalomancer's Crystal Nodes can store 2 charges simultaneously instead of 1. Each charge stores a different element (the last two distinct elemental spells that hit the Node). When Energy Release is triggered, both charges are released simultaneously — the first at 70% of its base damage (standard), the second at 50% of its base damage (secondary release). Each release has its own 2-tile AoE centered on the Node. A Node charged with Lightning + Fire releases both simultaneously: Lightning chain AoE + Fire terrain ignition in the same action.

**Trade-off:** Double charge storage allows for complex two-element releases from a single Node, dramatically increasing the Crystalomancer's combo potential. The AP economy is the same (3 AP for Energy Release regardless of charge count). Best in warbands with 3 Mancers of different elements where the Node naturally accumulates multiple charge types.

#### Passive C: Prismatic Shield — +20 pts

**Description:** The Crystalomancer can place Crystal Prisms in an arrangement that creates a Prismatic Shield — instead of one Prism redirecting spells that pass through, the Prismatic Shield configuration (3 Prisms placed in a triangle configuration within 2 tiles of each other) creates a reflective zone. Any projectile spell that enters the triangular zone is reflected back toward its origin point at 50% damage. This reflects both enemy and ally projectiles — the Crystalomancer must be careful about the shield orientation.

**Trade-off:** A 3-Prism triangle shield configuration uses the Crystalomancer's entire construct limit (3 of 3 slots) for Prisms only — no Crystal Nodes or Crystal Walls. This is a full defensive investment that eliminates the Energy Release mechanic for that game. Best in specific scenarios where reflecting enemy projectile fire is worth abandoning the storage combo.

---

### Stat Enhancements

#### Stat A: Reinforced Crystal (+20 HP) — +10 pts

**Description:** Max HP increases from 95 to 115. Brings the Crystalomancer to solid mid-tier durability. Critical in matchups where the opponent targets the Crystalomancer directly to destroy its construct-placement capacity — a dead Crystalomancer's constructs cannot be rebuilt.

#### Stat B: Extended Reach (+1 Spell Range) — +10 pts

**Description:** All Crystalomancer spell ranges increase by 1 tile. Crystal Shard: 6 → 7. Place Construct: 4 → 5. Energy Release: 5 → 6. Crystal Cascade: 4 → 5. Extended range allows the Crystalomancer to place Nodes in the opponent's half of the map from the safety of the mid-field, charging them from allied spells that reach deep into enemy territory.

---

### Signature Ability

The Signature Ability is unlocked as an additional spell slot — it does not replace any base spell.

#### Signature: Grand Prism Array — +40 pts

| Field | Value |
|---|---|
| **Name** | Grand Prism Array |
| **AP Cost** | 6 AP (entire activation; Crystalomancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Self-centered AoE — the Crystalomancer is the origin; the array generates in the surrounding area |
| **Range** | N/A (self-centered) |
| **AoE Radius** | 4 tiles (generates crystal infrastructure throughout this radius) |
| **Base Damage** | 0 at placement; all subsequent spells through the array deal bonus damage (see below) |
| **Element** | Crystal |
| **Effects Applied** | The Crystalomancer generates a temporary Grand Prism Array: 6 Crystal Prisms are automatically placed at equidistant points around the Crystalomancer within the 4-tile radius (the Crystalomancer chooses the rotation angle). These 6 Grand Prisms function identically to standard Crystal Prisms (refraction, bounce mechanic) but additionally have a special property: any beam/line spell that enters the Array is automatically bounced between multiple Prisms before arriving at its final target — each bounce adds 10% of the original spell's base damage as bonus damage (up to 3 bounces = +30% bonus). The Grand Prism Array persists for 3 turns, then collapses (all 6 Prisms shatter simultaneously, dealing 10 HP AoE per Prism in their respective positions). The Array is immune to destruction by normal spell damage during these 3 turns — only the Crystalomancer's own construct limit applies (the 6 Array Prisms count toward the 3-construct limit, immediately overriding any existing constructs; the oldest 3 standard constructs collapse on Array placement if the Crystalomancer had any active). |
| **Special Interactions** | Electromancer Arc Bolt through the Grand Prism Array: bounces between 3 Prisms before reaching its target — each bounce adds 10% base damage (total +30%) and the Arc Bolt chains from each Prism position to any WET adjacent units as if the Arc had been cast from that Prism. A single Arc Bolt through the Array can chain-stun 4–6 WET units in sequence. Photomancer light beams through the Array: the beam bounces between Prisms and applies ILLUMINATED to every unit it crosses on each bounce path. Pyromancer Ember Shot through the Array: bounces up to 3 times, applies BURNING and ON_FIRE state at each bounce destination (+10% dmg per bounce). |

**Design note:** Grand Prism Array is the Crystalomancer's statement ability — it does not deal direct damage but converts every single-target line spell on the entire battlefield into a multi-bounce AoE for 3 turns. In a warband with an Electromancer and a Photomancer, those 3 turns of Grand Prism Array transform their otherwise single-target spells into networked chain attacks. The full-activation cost and 5-turn cooldown reflect the significant power of converting the board into an energy relay system for 3 consecutive turns. The 6-Prism auto-placement removes the need for manual Prism positioning — the Array is symmetric and immediate. The trade-off is the construct cap override: the Crystalomancer cannot maintain its Crystal Node storage while the Array is active (the 6 Array Prisms consume all 3 construct slots). Grand Prism Array is an offensive and synergy mode, not a defensive maintenance mode.

**Synergy note:** Grand Prism Array combined with Electromancer is the most powerful implementation. Every Arc Bolt becomes a 3-bounce chain arc that potentially STUNS 4–6 units across the Array's radius simultaneously. In a match where the opponent has clustered around a FLOODED zone (Hydromancer-created), the Array converts a single Electromancer activation into a screen-wide chain stun sequence — every WET unit in the radius hit by a bounced Arc Bolt takes STUN.

---

## 6. Faction Synergy

### Best Faction: The Gilded Throne

The Gilded Throne's Crossbow Corps fires physical bolts — these do not refract through Crystal Prisms (physical projectiles are not elemental beams; refraction applies to elemental line spells only). However, the Gilded Throne has a deeper benefit: Iron Discipline (Charm and Panic immunity) protects the precisely-positioned Crossbow Corps from Psychomancer disruption while they hold the angles needed to exploit Crystal Prism refraction coverage zones.

The primary Gilded Throne benefit is Siege Arbalest (T2 Ranged) which fires armor-piercing bolts. While Siege Arbalest bolts don't refract, the Crystalomancer's Crystal Wall blocks enemy movement, creating chokepoints where Siege Arbalests fire into a predictable kill zone. The Crystalomancer provides geometry; the Arbalests provide sustained physical damage into that geometry. A Crystal Wall across a corridor + Siege Arbalests positioned behind it = enemies bottlenecked by the wall and taking Arbalest fire from range. The wall's movement block is non-elemental (physical construct) — Arbalest bolts don't need to refract through it; the wall just holds the enemy in place.

### The Verdant Pact — Organic and Crystal Convergence

Crystal constructs are immune to TOXIC_TERRAIN degradation — if a Verdant Pact Toximancer (Mancer) creates TOXIC_TERRAIN around the position of a Crystal Node, the Node remains fully functional in that toxic ground while enemy units that approach to destroy it take POISONED stacks. The Crystalomancer + Toximancer combination creates a poisoned defensive perimeter around crystal infrastructure.

Glade Archers apply POISONED on hit — if the Crystal Prism bounces a Necromancer Bone Bolt onto a target, and that target was already POISONED by Glade Archer fire, the stacking POISONED + DEATH_MARK interaction becomes possible through the Prism's redirection without requiring direct LoS from the Necromancer.

### The Ashen Covenant — Wailing Shade Refraction

Wailing Shades are phase-through ranged units — their projectiles ignore physical cover. Crystal Walls (which block movement but not LoS) interact with Wailing Shades uniquely: Shade projectiles pass through Crystal Walls freely, but the crystal structure subtly refracts the Shade's wailing projectile — it deals 5 bonus HP when passing through a Crystal Wall tile (the sonic wail resonates with crystal). This is a minor but consistent bonus for Ashen Covenant warbands with both Crystalomancer constructs and Wailing Shades.

More impactfully: Grave Husks advancing toward an enemy position can be directed through Crystal Prism positions where enemy projectile spells are being refracted — the Prism bounces enemy fire away from the Husk advance path. Instead of the Husk line walking into direct Ember Shot fire, the Pyromancer's Ember Shot is refracted sideways by a Prism, allowing the Husks to advance through formerly dangerous approaches unscathed (for 1 or 2 turns until the Prism is destroyed).

---

## 7. Combo Chains

### Combo 1 — Stored Lightning Burst (Crystalomancer + Electromancer) [SIGNATURE]

**Mancers involved:** Crystalomancer + Electromancer

**Step-by-step execution:**

1. **Crystalomancer activates (Turn N):** Places Crystal Node (2 AP) at a central position. Node is empty.
2. **Electromancer activates (Turn N):** Arc Bolt (or equivalent Lightning spell) targeted at the Crystal Node tile. The bolt hits the Node — Node stores Lightning element (1 charge). Electromancer has used its damage AP; Node is charged.
3. **Turn N+1, Crystalomancer activates:** Energy Release (3 AP) at the charged Node. The Node releases stored Lightning as a 2-tile AoE: 70% of Arc Bolt base damage as Lightning AoE. The Lightning release chains to all WET or CHARGED units within the 2-tile radius (standard chain arc mechanic). The Node is consumed.

**Tactical value:** The Electromancer spent 2–3 AP on the Arc Bolt (which charged the Node rather than hitting an enemy directly). The Crystalomancer then spent 3 AP to release that charge as an AoE burst centered exactly where the Node is placed — which the Crystalomancer chose for optimal AoE coverage. The Electromancer's spell has effectively been redirected to an optimized position. Best used when the Electromancer's direct line of fire is blocked but the Crystal Node is in a better AoE position.

---

### Combo 2 — Prism Refraction Network (Crystalomancer + Photomancer / Electromancer)

**Mancers involved:** Crystalomancer + any beam Mancer (Photomancer, Electromancer)

**Step-by-step execution:**

1. **Crystalomancer activates (Turn N):** Place Crystal Prism (2 AP) at the corner position between the allied beam Mancer's position and a target that is otherwise around a corner (no direct LoS). Set the Prism bounce direction to redirect the beam at the target.
2. **Allied beam Mancer activates (Turn N or N+1):** Fires beam spell toward the Crystal Prism (not at the actual target — aims at the Prism). The beam passes through the Prism tile, bounces 90 degrees, and travels to the target that had no LoS from the original position.

**Tactical value:** The Crystalomancer converts a no-LoS situation into a LoS-available situation. A Pyromancer hiding behind a STONE_WALL can be reached by a refracted Photomancer light beam — the beam goes around the wall. An Electromancer whose direct path to a WET enemy cluster is blocked by Obsidian can bounce the Arc Bolt through a Crystal Prism to reach the WET cluster on the other side.

---

### Combo 3 — Cryomancer Node Freeze (Crystalomancer + Cryomancer)

**Mancers involved:** Crystalomancer + Cryomancer

**Step-by-step execution:**

1. **Crystal Node placed at the center of an expected enemy cluster position.**
2. **Cryomancer fires Blizzard Field into the Node tile** (the Node is charged with Ice/FROZEN element).
3. **Crystalomancer releases: Energy Release** on the charged Node — 70% Blizzard Field damage as Ice AoE (2-tile radius). ICE_TILE terrain applied; CHILLED status on all units in radius. If any units were already CHILLED (from prior Cryomancer casts), the Node release applies FROZEN directly (WET or CHILLED → FROZEN on Ice spell, per Cryomancer interaction rules).

**Tactical note:** This effectively gives the Cryomancer two mass-freeze broadcasts in one engagement: Blizzard Field charges the Node (first freeze), then the Crystalomancer releases the Node (second freeze from a different position). The Node positions the second freeze anywhere on the map the Crystalomancer placed the Node — not necessarily where the Cryomancer can aim.

---

### Combo 4 — Crystal Resonance (Crystalomancer + Sonimancer) [SONIC PROPAGATION CHAIN]

**Mancers involved:** Crystalomancer + Sonimancer

**Step-by-step execution:**

1. **Crystalomancer places Crystal Wall (3 AP)** across a corridor leading to the enemy.
2. **Sonimancer activates:** Resonance Cone through the Crystal Wall. Sonic spells penetrate physical barriers — the cone passes through the Crystal Wall. As it does, sonic energy resonates with the crystal structure (+5 HP sonic damage per Wall tile the cone passes through). Additionally, connected crystal tiles (Crystal Node if placed within the Wall network) undergo Crystal Resonance — sonic damage propagates from each connected crystal tile at 15 HP per hop, hitting units adjacent to the Node.
3. **Result:** A single Sonimancer cone through a Crystal Wall generates the standard cone damage + Crystal Wall sonic bonus + Crystal Resonance propagation to the Node. Units around the Node (potentially in a different position from the cone) take 15 HP from the resonance hop.

**Why this works:** The Sonimancer fires one cone; the Crystal Wall amplifies it and the Crystal Node propagates a secondary hit. Two Mancers create a three-vector damage distribution from one cone cast.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Crystalomancer

| Mancer | Counter Mechanism |
|---|---|
| **Pyromancer** | ON_FIRE terrain deals 8 HP/turn to crystal constructs. Two turns of fire on a Node tile = 16 HP — enough to destroy a standard 15 HP Node. A Pyromancer who seeds fire under the Crystalomancer's constructs systematically destroys the network faster than the Crystalomancer can rebuild it. Scorched Earth (2-tile AoE) can destroy all three constructs simultaneously if they are clustered. |
| **Geomancer** | Earthen Smash (4 AP, 2-tile AoE) destroys all constructs in its radius simultaneously (20+ HP burst damage). OBSIDIAN placed by the Geomancer creates blind spots in the crystal network that Crystal Prisms cannot route around. Geomancer Raise Terrain can physically bury construct positions — elevated terrain on a Node tile makes the Node inaccessible from ground level. |
| **Aeromancer** | UPDRAFT zones grant WEIGHTLESS to allies — WEIGHTLESS units floating above ground do not trigger ground-based crystal terrain interactions (Crystal Wall still blocks their movement, but the Crystalomancer's construct network is less effective against floating units that bypass ground-level energy relay). Aeromancer's displacement spells can push the Crystalomancer away from its own construct network, removing its ability to trigger Energy Release within the 5-tile range limit. |

---

## 9. Temperature Effects

### Temperature Effects per Spell

| Spell | Temperature Change | Notes |
|---|---|---|
| **Crystal Shard** (1 AP) | **0** | Physical projectile — no thermal component |
| **Place Construct** (2–3 AP) | **0 direct** | The construct itself inherits temperature from its tile: a Crystal Node placed on BURNING terrain (ON_FIRE tile) starts with +10 temperature stored; a Crystal Node placed on ICE_TILE starts with -10 temperature stored |
| **Energy Release** (3 AP) | **Releases stored temperature alongside the spell effect** | HOT NODE (stored fire spell): release applies +15 temperature to all units in the 2-tile AoE. COLD NODE (stored ice spell): release applies -15 temperature. The exact value scales with what was stored — see Crystal Node temperature storage rules below |
| **Crystal Cascade** (4 AP) | **0 direct; transmits stored temperature per node** | Each HOT NODE in the cascade chain applies +10 temperature to all units hit at that chain point; each COLD NODE applies -10 temperature |

---

### Temperature Interaction Notes

**Crystal Node temperature storage — full rules:**
Crystal is an excellent thermal conductor. When a spell hits a Crystal Node, the Node stores BOTH the spell's element AND its associated temperature change:
- **HOT NODE** (node absorbed a fire spell): the node glows orange. Energy Release from a HOT NODE applies the stored fire spell effects AND a temperature change of **+15 to +35** to all units in the AoE (the exact value depends on the spell stored — a weak fire spell stores +15; a high-power fire spell like Pillar of Flame stores +35).
- **COLD NODE** (node absorbed an ice spell): the node glows blue. Energy Release from a COLD NODE applies the stored ice spell effects AND a temperature change of **-15 to -35** to all units in the AoE.
- **NEUTRAL NODE** (node absorbed an earth, wind, or non-thermal spell): no temperature effect on release.
- A Crystal Node stores ONE thermal state at a time. Hitting a HOT NODE with an ice spell **converts** it to a COLD NODE — the thermal state is overwritten, not summed. This creates a tactical vulnerability: an opponent who fires a cold spell into a HOT NODE neutralizes the Crystalomancer's stored thermal payload.

**Crystal Prism and temperature refraction:**
When a thermal spell (fire, ice, or any spell carrying a temperature change) refracts through a Crystal Prism, the temperature change refracts with it — **each refracted path carries the full thermal payload, not a split portion**. A fire beam spell with a +20 temperature change that bounces through a Crystal Prism hits two targets — BOTH targets take +20 temperature (the crystal amplifies rather than divides the thermal energy). This makes Crystal Prism refraction a temperature-multiplier as well as a LoS bypass: one high-temperature spell directed through a Prism applies its full thermal effect to both the original target and the refracted target simultaneously.

**FROZEN terrain and Crystal Wall — frost corridors:**
Crystal Walls placed on FROZEN tiles (tiles in FROZEN terrain state, typically from Cryomancer activity) become **FROST CRYSTAL WALLS**. A FROST CRYSTAL WALL applies **-5 temperature per turn** to all units adjacent to it. Any unit that spends an activation adjacent to a FROST CRYSTAL WALL is temperature-drained by the crystalline cold radiating from the frozen substrate. Combined with a Cryomancer's FROZEN terrain, a well-placed Crystal Wall across a corridor creates a cold corridor that temperature-drains any unit moving through it. A unit moving through a 3-tile FROST CRYSTAL WALL corridor over 2 turns loses -10 temperature from wall proximity — enough to push a NEUTRAL unit into COLD (-10), or push a COLD unit toward SUPERCOOLED territory.

**The Thermal Cascade — Crystal Cascade as temperature amplifier:**
In a Crystal Cascade, each node in the chain transmits its stored temperature alongside the cascade pulse. A chain through **3 HOT NODES** applies +10 temperature per node = **+30 total temperature** to enemy units caught in the full cascade. This is the Crystalomancer's most powerful temperature-escalation tool, and requires deliberate setup:
1. **Crystalomancer** places 3 Crystal Nodes at tactically spaced positions
2. **Pyromancer ally** fires into each Node (3 activations, or one Scorched Earth that covers all Node tiles) — all 3 Nodes become HOT NODES (+25 to +35 temperature stored each)
3. **Crystalomancer** triggers Crystal Cascade through all 3 HOT NODES — each Node transmits +10 temperature, pushing a cluster of NEUTRAL enemies to +30 temperature (entering HOT — SLOWED) in a single action

This combo is documented as **"The Thermal Cascade."** Against enemies already in WARM state (+1 to +30), a +30 temperature spike from the Thermal Cascade pushes them directly to OVERHEATED (+61), triggering BURNING DoT (5 HP/turn) and opening them to THERMAL SHOCK vulnerability on the following turn.

---

## 10. Augmentation Spell

### Prismatic Shell

**AP Cost:** 3 | **Range:** 3 tiles | **Targeting:** Single allied unit | **Cooldown:** 3 turns

Grows a prismatic crystal lattice around an ally that absorbs incoming damage and stores it as coherent light energy for a targeted release.

**Effects (up to 4 turns):**
- Ally begins with a PRISMATIC CHARGE counter at 0; each time the ally takes damage from any source, the counter increases by 1 (maximum 3)
- At any point, either the ally or the Crystalomancer (as a free action on the Crystalomancer's own turn) can release the stored charge
- Release: a light beam fires from the ally's position in a chosen direction, dealing 2 x PRISMATIC CHARGE damage (2, 4, or 6) and applying CHARGED to the first unit struck
- Decay: if the ally takes no damage for 2 consecutive turns, the shell dissipates unused -- stored energy bleeds off harmlessly

**Tactical intent:** Reactive damage conversion that rewards absorbing hits deliberately. The more the ally absorbs, the larger the eventual release -- but they must genuinely take damage to charge it. The decay mechanic prevents turtling: the ally cannot charge up safely behind cover; they must invite damage or the shell is wasted. The Crystalomancer's ability to trigger the release (not just the ally) enables coordinated timing -- set up a fully charged ally, wait for the ideal firing line, release as a free action from across the board. Maximum release (6 damage) applies CHARGED to the target, enabling Electromancer chain-stun follow-up.

**Notable interactions:** If the ally takes LIGHTNING damage while CHARGED (from an enemy Electromancer), the element matrix triggers Overload (AoE explosion); Prismatic Shell then stores the damage from that explosion -- potentially jumping to high charge from one enemy mistake. CHARGED applied to the release target opens a chain-stun window for an allied Electromancer on the same activation phase.

*End of Crystalomancer design document.*

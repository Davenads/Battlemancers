# Mancer Upgrade Catalog

Each Mancer may purchase any combination of upgrades at warband construction. Upgrades increase a Mancer's point cost beyond the 100-pt base but **do not change their 100-pt activation cost**. The 1,000-pt warband cap is the natural constraint on upgrade spend.

Upgrade types follow the cost ranges established in `design/warbands.md`:

| Type | Cost Range |
|---|---|
| Spell Variant | +15–25 pts |
| Passive Trait | +20–30 pts |
| Stat Enhancement | +10–20 pts |
| Signature Ability | +25–50 pts |

A Mancer may take multiple upgrades. Total cost = 100 (base) + sum of all purchased upgrades.

---

## Pyromancer

*DoT specialist, area denial, spreading terrain fire.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Inferno Shot | Spell Variant | +20 pts | Replaces Ember Shot. Fires a superheated bolt that applies 2 BURNING stacks (vs. 1) on hit, extends range to 7 tiles, and leaves a persistent ON_FIRE tile one size larger than standard. No cooldown retained. |
| Ashborn | Passive Trait | +25 pts | The Pyromancer is immune to BURNING and ON_FIRE terrain damage. Additionally, whenever the Pyromancer moves through an ON_FIRE tile, they leave a second ON_FIRE tile behind them (trail blazing). |
| Firewalker | Stat Enhancement | +15 pts | moveRange +1 (4 total). The Pyromancer ignores movement penalties from all fire-related terrain (ON_FIRE, BURNING_OVERGROWTH, LAVA). |
| Firestorm | Signature Ability | +40 pts | *5 AP, 4-turn cooldown.* Summons a self-propagating firestorm: creates a 3-tile radius ON_FIRE zone that expands outward by 1 tile per turn for 3 turns, applying BURNING (2 stacks) to all units caught in the spreading ring. Terrain converted to ON_FIRE persists after the storm ends. |

---

## Hydromancer

*Push/pull, wet terrain, healing, flow.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Mending Torrent | Spell Variant | +20 pts | Replaces Mending Current. Heals a primary target for 18 HP and simultaneously heals a second allied target within 3 tiles of the first for 10 HP. Also removes FROZEN in addition to BURNING and POISONED. Same AP cost and cooldown as Mending Current. |
| Floodkeeper | Passive Trait | +25 pts | FLOODED terrain created by Flood Zone persists 2 additional turns (5 total). Additionally, enemy units that begin their turn standing in FLOODED terrain created by this Hydromancer receive 1 CHILLED stack — enabling Cryomancer freeze setups from distance. |
| Deep Reserves | Stat Enhancement | +15 pts | Max HP +20 (120 total), making the Hydromancer one of the most durable enablers on the roster. Base armor +1 (2 total). |
| Tsunami | Signature Ability | +45 pts | *5 AP, 4-turn cooldown.* Releases a massive wave along a 7-tile line from the Hydromancer's facing. All units in the line are pushed 4 tiles (with full collision damage) and receive WET. Every tile in the line becomes FLOODED terrain for 3 turns. The wave's displacement ignores WEIGHTED status. |

---

## Cryomancer

*Slows, freezes, brittle armor, slippery tiles.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Glacial Cascade | Spell Variant | +20 pts | Replaces Glacial Spike. Erupts ice in a 3-tile radius (vs. 2), applying both BRITTLE_ARMOR and CHILLED simultaneously to every unit hit. Converts impacted tiles to ICE terrain. Cooldown unchanged. |
| Deep Freeze | Passive Trait | +25 pts | Any FROZEN unit struck by the Cryomancer's spells has their FROZEN duration extended by 1 turn. Additionally, FROZEN units that take physical damage from non-Cryomancer sources automatically generate a SLIPPERY_ICE tile beneath them that persists 2 turns. |
| Arctic Shell | Stat Enhancement | +15 pts | baseArmor +2 (3 total). The Cryomancer is immune to CHILLED self-application from their own spells and is unaffected by SLIPPERY_ICE movement penalties. |
| Absolute Zero | Signature Ability | +40 pts | *5 AP, 4-turn cooldown.* Drops all temperature in a 3-tile radius to absolute minimum. All units in range are immediately FROZEN regardless of prior temperature state. All tiles in the radius become permanent ICE terrain. FROZEN units already BRITTLE immediately trigger SHATTER. |

---

## Geomancer

*Walls, elevation, cover, terrain reshaping.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Boulder Hurl | Spell Variant | +20 pts | Replaces Rock Throw. Deals 25 damage (vs. 17), displaces the target 1 tile on impact, and converts the impact tile to RUBBLE terrain (impassable for 2 turns). SHATTER interaction retained. Zero cooldown retained. |
| Tectonic Resonance | Passive Trait | +25 pts | Whenever the Geomancer raises a Stone Wall, all enemy units within 2 tiles of the wall take 5 earth damage and are SLOWED for 1 turn from the tremors. Additionally, the Geomancer's own Stone Walls have +20 HP and cannot be destroyed in a single hit. |
| Stone Skin | Stat Enhancement | +15 pts | baseArmor +2 (4 total — highest on roster). The Geomancer treats all terrain they stand on as natural stone for the purposes of their own spells (terrain cost reduction and elevation). |
| Cataclysm | Signature Ability | +45 pts | *5 AP, 4-turn cooldown.* Destroys all terrain features in a 4-tile radius — walls, spires, constructs, overgrowth, ice — and simultaneously raises the entire area 1 height level. Deals 20 earth damage to all units in the radius and displaces them 1 tile outward. Creates an elevated plateau with no impassable obstacles. |

---

## Aeromancer

*Displacement, evasion, projectiles, mobility.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Gale Force | Spell Variant | +20 pts | Replaces Gust Strike. Pushes the target 3 tiles (vs. 2), deals 14 damage (vs. 10), and applies DEAFENED for 1 turn from the impact pressure. Zero cooldown retained. |
| Tailwind | Passive Trait | +20 pts | Whenever the Aeromancer grants Updraft to an ally, they also receive 1 turn of UPDRAFT themselves. Additionally, Wind Wall now redirects non-wind ranged spells that pass through it, deflecting them at 90 degrees toward the nearest enemy. |
| Slipstream | Stat Enhancement | +15 pts | moveRange +2 (7 total — fastest Mancer on the roster). The Aeromancer may pass through allied unit tiles during movement without blocking. Gain 1 free Gust Strike displacement per turn when ending movement adjacent to an enemy. |
| Eye of the Storm | Signature Ability | +40 pts | *4 AP, 4-turn cooldown.* Places a persistent tornado vortex at a target location (range 5). The vortex lasts 3 turns. At the start of each turn, all enemy units within 3 tiles of the vortex are pushed 2 tiles away from its center. Allied units in the zone are unaffected. The vortex itself cannot be destroyed. |

---

## Electromancer

*Chains, stuns, conductivity, burst damage.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Supercharged Arc | Spell Variant | +25 pts | Replaces Arc Bolt. Deals 20 damage (vs. 13) and applies STUNNED for 1 turn even without WET terrain. Additionally chains to 1 adjacent unit regardless of WET state (extra arc damage at 50% of primary damage). Target tile is CHARGED for 2 turns. Zero cooldown retained. |
| Residual Charge | Passive Trait | +25 pts | Enemy units that recover from STUNNED (applied by this Electromancer) become CHARGED for 1 turn immediately upon recovery. CHARGED units stepping into WET terrain trigger an arc chain automatically without requiring Electromancer activation. |
| Faraday Body | Stat Enhancement | +15 pts | The Electromancer cannot be hit by their own chain arcs or Overload discharge — friendly fire from their own spells is eliminated. Max HP +10 (100 total). Immune to STUNNED from external electrical sources. |
| Thunderstorm | Signature Ability | +45 pts | *5 AP, 4-turn cooldown.* Calls down a lingering electrical storm across a 5-tile radius lasting 2 turns. Each turn: all WET units in the zone take 15 arc damage and become STUNNED for 1 turn; all CHARGED tiles in the zone discharge simultaneously. The storm itself prevents enemies from using AP on movement — they must spend 1 extra AP to take any move action within the zone. |

---

## Necromancer

*Summons, corpse economy, attrition.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Raise Horror | Spell Variant | +25 pts | Replaces Raise Shambler. Summons a HORROR (25 HP, 12 melee damage, on death applies DEATH_MARK to all units within 1 tile) rather than a Shambler. Horrors cost 1 more AP to raise but count within the existing 3-summon cap. |
| Soul Siphon | Passive Trait | +20 pts | Each kill scored by the Necromancer or any of their active summons restores 3 HP to the Necromancer. Additionally, any unit killed while DEATH_MARKED grants 1 bonus Soul Energy and raises a free Shambler at its location (does not count toward summon cap if the cap is already full — oldest summon is replaced). |
| Undying Vigor | Stat Enhancement | +15 pts | Max HP +20 (110 total) and baseArmor +1 (2 total). The Necromancer's low-HP vulnerability in early turns is significantly reduced. |
| Army of the Dead | Signature Ability | +50 pts | *5 AP, 5-turn cooldown.* Simultaneously raises up to 5 Shamblers from all available corpse locations and DEATH_MARK sites within 6 tiles. The active summon cap is temporarily raised to 6 for 3 turns. All newly raised summons also inherit DEATH_MARK, so their deaths will chain-mark survivors. |

---

## Chronomancer

*Haste, delay, rewind, cooldown manipulation.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Mass Slow | Spell Variant | +20 pts | Replaces Time Slow. Applies Time Slow's AP drain and movement penalty to all enemy units in a 2-tile radius simultaneously. Cooldown increased to 3 turns; AP cost unchanged. |
| Temporal Intuition | Passive Trait | +30 pts | All of the Chronomancer's own spell cooldowns are reduced by 1 (minimum 1 turn). Rewind's cooldown becomes 3 turns; Stasis becomes 2 turns; Timestep becomes 3 turns. The most impactful passive in the time kit. |
| Quickened Frame | Stat Enhancement | +20 pts | moveRange +1 (5 total). baseActionPoints +1 (7 total), giving the Chronomancer the most AP of any Mancer without Haste applied — enough to cast Haste and still use both Stasis and Time Slow in a single activation. |
| Chrono Rupture | Signature Ability | +45 pts | *5 AP, 4-turn cooldown.* Overloads local spacetime in a 3-tile radius. All enemy units in the zone are simultaneously STUNNED for 1 turn (cannot act), lose 2 AP on their next activation (Time Slow effect), and are SLOWED for 2 turns. Effectively eliminates an entire cluster from one turn of play. |

---

## Photomancer

*Vision, blinding, reveals, beams.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Solar Flare | Spell Variant | +20 pts | Replaces Solar Flash. Deals 14 damage (vs. 8) in a 2-tile radius AND leaves ILLUMINATED terrain for 2 turns, denying enemy concealment in the blast area. BLINDED duration unchanged at 2 turns. |
| Afterburn | Passive Trait | +25 pts | BLINDED units that are hit by any subsequent Light-element spell have their BLINDED duration extended by 1 turn per hit. Light Beam can therefore maintain BLINDED on a single target indefinitely if the Photomancer focuses fire. Additionally, BLINDED enemies adjacent to ILLUMINATED terrain cannot benefit from cover. |
| Radiant Aura | Stat Enhancement | +15 pts | All Light Beam and Illuminate range extended by 1 (8 tiles total — longest ranged Mancer on the roster). Max HP +10 (100 total). The Photomancer gains immunity to BLINDED from enemy sources. |
| Solar Convergence | Signature Ability | +45 pts | *5 AP, 4-turn cooldown.* Channels a sustained prismatic beam simultaneously through every Crystal Prism construct on the battlefield (regardless of owner) and the Photomancer's own position. Each unit intersected by any beam path takes 30 Light damage and is BLINDED for 3 turns. Pairs devastatingly with the Crystalomancer's network. |

---

## Psychomancer

*Charm, panic, confusion, morale damage.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Mass Confusion | Spell Variant | +20 pts | Replaces Confusion. Applies CONFUSED for 2 turns to all enemy units in a 2-tile radius simultaneously. Same AP cost and cooldown as Confusion. Iron Discipline units remain partially affected (50% chance to misfire rather than full random movement). |
| Psychic Resonance | Passive Trait | +25 pts | Mind Spike bypasses all physical armor (deals full damage regardless of armor value). Additionally, units that recover from CHARMED or CONFUSED applied by this Psychomancer take 5 Psychic damage and are PANICKED for 1 turn as the mental fog clears. |
| Iron Will | Stat Enhancement | +20 pts | Max HP +15 (100 total). The Psychomancer is immune to CHARM and PANIC from all sources — they cannot be turned by enemy Psychomancers or broken by morale effects. Immune to Sonimancer SILENCE (force of will maintains focus). |
| Puppet Master | Signature Ability | +50 pts | *4 AP, 4-turn cooldown.* CHARMs the targeted enemy Mancer for 1 full turn AND simultaneously PANICS all non-Mancer enemy units within 3 tiles of that Mancer, forcing them to flee from the now-player-controlled Mancer. Cannot affect Iron Discipline Mancers; reduced to 1-turn PANIC only on Gilded Throne non-Mancer units. |

---

## Floramancer

*Roots, growth zones, poison pollen, barriers.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Strangling Vine | Spell Variant | +20 pts | Replaces Vine Surge. Applies ROOT for 2 turns (vs. 1) and deals 14 damage (vs. 8). The OVERGROWTH terrain created is upgraded to VIRULENT_SPORES immediately (2 POISONED stacks per movement through). Zero cooldown retained. |
| Bountiful Bloom | Passive Trait | +25 pts | SPORES and VIRULENT_SPORES terrain created by the Floramancer persists 2 additional turns. OVERGROWTH barriers created by Overgrowth Barrier automatically apply 1 POISONED stack to any enemy who attempts to cross (regardless of movement outcome). Pollen cloud radius on death expands to 2 tiles. |
| Nature's Ward | Stat Enhancement | +15 pts | Max HP +15 (100 total). The Floramancer is fully immune to POISONED from all sources (own pollen clouds, Toximancer venom, Flooded SPORES terrain). moveRange +1 (5 total) to better chase root targets. |
| World Tree | Signature Ability | +45 pts | *5 AP, 4-turn cooldown.* Grows a colossal tree at a target location. The tree creates a 4-tile radius OVERGROWTH zone, automatically applies VERDANT EMBRACE healing to all allied units in range at the start of each turn, and blocks all LoS entering the zone from outside. OVERGROWTH inside is VIRULENT_SPORES quality. Persists 4 turns or until 60 HP of damage is dealt to the tree. |

---

## Faunamancer

*Companion units, pack tactics, tracking.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Alpha Call | Spell Variant | +20 pts | Replaces Call of the Wild. Summoned companions arrive with 20% bonus HP and automatically apply BESTIAL_MARK on their first successful attack. Additionally, the Faunamancer may call a fourth companion type: the Alpha Dire Wolf (high damage, BLEEDING on hit, pack bonus applies at +3 damage vs. marked targets). |
| Pack Instinct | Passive Trait | +25 pts | When a companion within 4 tiles of the Faunamancer is killed, the Faunamancer immediately gains 2 AP (maximum once per turn). Additionally, surviving companions within 3 tiles of a fallen companion gain +1 damage for 2 turns from pack fury. |
| Wild Empathy | Stat Enhancement | +20 pts | Maximum active companion count increased to 4 (vs. 3). Faunamancer moveRange +1 (5 total). Predator's Sense cooldown reduced to 1 turn, allowing constant tremorsense coverage across activations. |
| Blood Frenzy | Signature Ability | +45 pts | *4 AP, 3-turn cooldown.* All active companions and allied Chaff units within 4 tiles simultaneously move toward and attack the current BESTIAL_MARK target as a free coordinated action outside the normal activation sequence. Companions may each attack once; Chaff units each move up to 3 tiles and attack once. Damage dealt is normal (no modifier). Effective as a sudden multi-source strike in the same resolution window. |

---

## Toximancer

*Poison stacks, debuffs, contamination.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Lethal Injection | Spell Variant | +25 pts | Replaces Virulent Injection. Range extended from 1 to 3 (no longer requires adjacency). Retains 20 damage and 3 POISONED stacks at 4-turn duration. Cooldown unchanged. The primary weakness of the base spell (adjacency commitment) is eliminated. |
| Lingering Toxin | Passive Trait | +25 pts | POISONED stacks applied by the Toximancer's venom-origin spells last 1 additional turn (4-turn duration at base; 5 turns on Virulent Injection venom). This does not apply to Floramancer pollen origin. Additionally, the DEBILITATED threshold is reduced to 4 stacks (vs. 5) for the Toximancer's own venom only. |
| Toxin Resistance | Stat Enhancement | +15 pts | Max HP +15 (100 total). The Toximancer is immune to all POISONED stacks from any source. Units within 1 tile of the Toximancer take 1 passive POISON damage per turn from ambient toxin cloud (does not apply stacks, just chip damage). |
| Pandemic | Signature Ability | +45 pts | *5 AP, 4-turn cooldown.* Converts all TOXIC_TERRAIN and SPORES terrain on the entire battlefield into VIRULENT_TOXIN terrain simultaneously (2 POISONED stacks per tile movement entered). All enemy units currently carrying any POISONED stack receive 3 additional stacks instantly. Contamination checks trigger on all affected units immediately after. |

---

## Osteomancer

*Bone armor, spikes, skeletal constructs.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Bone Volley | Spell Variant | +25 pts | Replaces Bone Shard. Fires 3 shards in a short forward cone, each dealing 11 damage (33 total vs. 16). Each shard independently evaluates SHATTER and BRITTLE conditions against its target. If all 3 hit the same BRITTLE+FROZEN target, combined SHATTER damage is 68. Zero cooldown retained. |
| Ossified Fortress | Passive Trait | +25 pts | At the start of each turn, if the Osteomancer has at least one Bone Spire within 2 tiles, they automatically gain 5 Bone Armor points (stacks up to 10). Additionally, Bone Spires now also apply BRITTLE to any enemy unit that strikes them in melee. |
| Reinforced Frame | Stat Enhancement | +15 pts | baseArmor +2 (4 total). Max HP +10 (130 total — highest HP on the roster). The Osteomancer becomes fully immune to BLEEDING status. |
| Skeletal Ascension | Signature Ability | +50 pts | *4 AP, 4-turn cooldown.* The Osteomancer channels bone mass from the battlefield into their own frame for 2 turns: gain +3 armor (7 total), +30 temp Bone Armor HP, and all enemy units adjacent to the Osteomancer at start of each turn immediately receive BRITTLE. During Ascension, Bone Shard hits all enemies adjacent to the primary target as a 1-tile splash (in addition to the targeted hit). |

---

## Gravimancer

*Pulls, crushes, immobilizes, fall damage.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Singularity Pull | Spell Variant | +20 pts | Replaces Pull. Pulls the target 5 tiles toward the Gravimancer (vs. 3) and applies ROOT for 1 turn upon landing — the target cannot immediately retreat. Collision damage retained at 8 per impacted unit. Cooldown increased to 2 turns. |
| Gravitational Echo | Passive Trait | +25 pts | After Crush resolves, all enemy units within 2 tiles of the struck target are pulled 1 tile toward that target by the gravitational ripple. Additionally, Gravity Well's duration is extended to 4 turns (vs. 3) and its pull-toward radius expands to 4 tiles. |
| Dense Matter | Stat Enhancement | +15 pts | Max HP +15 (105 total). The Gravimancer is immune to WEIGHTED from external sources (their own gravitational field prevents accumulation). CRUSH cooldown reduced by 1 (2 turns). |
| Event Horizon | Signature Ability | +50 pts | *5 AP, 4-turn cooldown.* Creates a gravitational singularity at a target location (range 4). Duration 3 turns. At the start of each turn: all units within 5 tiles are pulled 2 tiles toward the center; units occupying the center tile take 20 Gravity damage. Units that cannot be displaced (EARTHEN_MANTLE, GRAVITATIONAL_ANCHOR) are instead STUNNED for 1 turn. The singularity cannot be destroyed. |

---

## Sonimancer

*Cone attacks, disruption, silence, shatter.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Deafening Roar | Spell Variant | +20 pts | Replaces Sonic Pulse. Applies DEAFENED for 2 turns (vs. 1) AND SILENCED for 1 turn simultaneously in the same cone. Damage increased to 8 (vs. 4). AP cost raised to 2; zero cooldown retained. |
| Resonant Body | Passive Trait | +25 pts | The Sonimancer is immune to DEAFENED and SILENCED from all sources. After using Shatter Scream, the Sonimancer's next Sonic Pulse within 1 turn costs 0 AP. Additionally, Dissonance Wave SILENCE duration extended to 3 turns. |
| Armored Vocalist | Stat Enhancement | +15 pts | Max HP +15 (105 total). moveRange +1 (4 total), significantly improving the Sonimancer's ability to get into their short-range cone attack range. Resonance Cone cooldown reduced to 0 turns. |
| Sonic Annihilation | Signature Ability | +50 pts | *5 AP, 4-turn cooldown.* Releases maximum-intensity omni-directional sonic burst (full 3-tile radius around the Sonimancer, not a cone). Deals 50 Sound damage. SHATTERS all FROZEN and BRITTLE_ARMOR targets in range. Destroys all bone constructs and Crystal constructs within 3 tiles. All units (ally and enemy) except the Sonimancer are DEAFENED for 2 turns. |

---

## Crystalomancer

*Refraction, barriers, stored energy.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Overcharged Release | Spell Variant | +25 pts | Replaces Energy Release. Triggers at 100% stored spell power (vs. 70%) with element-appropriate AoE interactions. The Node is consumed on use as normal. If 3 Nodes are active simultaneously, Overcharged Release detonates all 3 at once (each at 80% power). |
| Lattice Resilience | Passive Trait | +25 pts | All Crystal constructs (Nodes, Prisms, Walls) have +10 HP. When any construct is destroyed by enemy action, it releases a 10-damage AoE shrapnel burst in a 2-tile radius (vs. 1-tile base). Destroyed Nodes still release their stored element at 40% power before detonating. |
| Crystal Form | Stat Enhancement | +15 pts | baseArmor +1 (2 total). Whenever an enemy destroys one of the Crystalomancer's constructs, the Crystalomancer gains 2 Prismatic Charge stacks on the currently shelled ally (or themselves if no shell is active). Maximum active constructs increased to 4. |
| Crystal Fortress | Signature Ability | +45 pts | *4 AP, 4-turn cooldown.* Instantly places one Crystal Node, one Crystal Prism, and one Crystal Wall simultaneously in an arc around the Crystalomancer's position (Crystalomancer selects facing). The Wall forms a 3-tile arc at LoS range. All 3 constructs gain the Lattice Resilience HP bonus immediately. Each construct also begins charged with the last element stored in any prior Node. |

---

## Echomancer

*Repeat casts, delayed duplicates, positional echoes.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Perfect Echo | Spell Variant | +25 pts | Replaces Echo. Fires at 80% power (vs. 60%). Cooldown reduced to 0 turns (vs. 1 turn), allowing the Echomancer to echo the same activation's ally cast immediately rather than the prior turn's. Cannot echo healing or another Echo. |
| Resonant Memory | Passive Trait | +20 pts | Echo can copy spells from up to 2 turns ago rather than only the most recent (Echomancer remembers the last 2 allied casts). The Echomancer chooses which stored spell to echo at cast time. Additionally, Afterimage collapse damage increased to 10 (vs. 5) and applies PANICKED for 1 turn to the attacker. |
| Phantom Frame | Stat Enhancement | +15 pts | Max HP +20 (100 total). Phase Step cooldown reduced to 1 turn (vs. 2), allowing repositioning on nearly every other activation. Maximum simultaneous Afterimages increased to 3. |
| Mirror Array | Signature Ability | +50 pts | *4 AP, 4-turn cooldown.* Spawns 3 Afterimages simultaneously at chosen positions within 4 tiles. For 2 turns, whenever any allied Mancer casts a spell, all 3 active Afterimages simultaneously echo that spell at 35% power each toward the same target (105% combined output from "free" sources). Each Afterimage is destroyed by 1 hit; surviving Afterimages continue echoing. |

---

## Thermomancer

*Gradients, overheat/chill combos, zone control.*

| Upgrade Name | Type | Point Cost Increase | Description |
|---|---|---|---|
| Thermal Blast | Spell Variant | +20 pts | Replaces Heat Lance. Deals 20 damage (vs. 14) and applies both OVERHEATED and BURNING simultaneously in a single hit. Range reduced to 4 tiles (vs. 5). Zero cooldown retained. On a SUPERCOOLED target: THERMAL SHOCK triggers with amplified bonus damage (+10). |
| Temperature Mastery | Passive Trait | +25 pts | THERMAL SHOCK triggered by the Thermomancer deals 10 additional bonus damage (on top of the standard delta-based calculation). Additionally, Thermal Inversion has its cooldown reduced to 1 turn (vs. 2), enabling Heat Lance → Thermal Inversion → Cold Lance in a single activation for two guaranteed THERMAL SHOCK triggers. |
| Thermal Insulation | Stat Enhancement | +20 pts | Max HP +10 (100 total). The Thermomancer is immune to temperature DoT from all sources, not just their own Thermal Gradient Zone. OVERHEATED and OVERCOOLED status effects cannot be applied to the Thermomancer by external spells — only their own controlled temperature shifts apply. |
| Absolute Inversion | Signature Ability | +50 pts | *5 AP, 4-turn cooldown.* Targets a 4-tile radius. Every unit in the zone instantly undergoes Thermal Inversion — their current temperature state flips to its opposite extreme. This triggers THERMAL SHOCK on all pre-conditioned units simultaneously. The zone then becomes a Thermal Gradient Zone (hot half / cold half) for 4 turns, centered at the impact point. |

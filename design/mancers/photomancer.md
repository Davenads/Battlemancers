# Photomancer — Full Design Document

---

## 1. Tactical Identity

The Photomancer is the battlefield's information controller and area-denial specialist through the medium of light itself. Where the Pyromancer denies space with fire and the Aeromancer denies with displacement, the Photomancer denies with vision — creating zones of brilliant radiance that blind enemies to their surroundings, illuminating high-value targets to make them magnets for allied fire, and using precisely aimed light beams to deal concentrated burst damage at maximum range. In a game where tactical legibility is a design pillar, the Photomancer attacks that legibility directly: a BLINDED Mancer cannot target at range; an ILLUMINATED target draws more punishment from everything that hits it; a Dark Zone created by light-absorption tears a hole in the opponent's ability to coordinate.

Playing the Photomancer well requires understanding that its two primary tools — BLINDED and ILLUMINATED — pull in opposite directions that it must balance deliberately. BLINDED makes enemies useless at range. ILLUMINATED makes a specific target take more damage from everything. The Photomancer player decides each turn whether to blind the cluster (protecting the team from ranged fire) or to illuminate the priority target (amplifying the team's damage output). Doing both in one turn — via the spell kit's AP management — is the Photomancer's highest-expression play.

**Primary win condition:** The Photomancer wins by shutting down enemy ranged threat and concentrating allied burst onto illuminated priority targets. An enemy warband where the key Mancer is ILLUMINATED (taking +20% damage from all sources) and the supporting Crossbow Corps are BLINDED (unable to fire at range) has lost its primary offense and support simultaneously. The Photomancer does not need to deal the killing blow — it needs to create 2-turn windows where the opponent cannot effectively respond to allied damage.

**Core weakness:** The Photomancer has moderate damage and moderate control, but excels at neither to the degree that dedicated damage or dedicated control Mancers do. Its value is entirely reliant on allied follow-through: an ILLUMINATED target that no ally attacks on the same turn is a wasted 2 AP. A BLINDED formation that the Photomancer's team doesn't press with a melee advance is terrain advantage that evaporates after 2 turns. The Photomancer also has no answer to close-range melee pressure — its Light Beam requires LOS and range, both of which become impossible when a melee unit closes to adjacent tiles.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 90 | Below average; mid-range durability; fragile under sustained focus |
| **Move Range** | 4 tiles per activation | Above-average; needs to reposition for LOS and optimal beam angles |
| **Base Armor** | 1 | Minimal physical mitigation; relies on range and BLINDED enemies |
| **Spell Range** | 7 tiles (base) | Highest base range of any Mancer — the Photomancer is a long-range fire-support unit |
| **AP per Activation** | 6 | Standard for all Mancers; movement costs 1 AP per tile |
| **Element** | Light | All base spells deal Light damage or apply light-state terrain/status interactions |

**AP budget example:** With 6 AP, the Photomancer can move 2 tiles (2 AP), Illuminate a target (2 AP), and fire Light Beam (2 AP), or move 3 tiles and apply Solar Flash to a cluster (3 AP + 3 AP move = 6 AP, no beam this turn).

---

## 3. Base Spell Kit

The Photomancer's four base spells are designed to cover distinct combat functions:
- **Light Beam** — primary single-target damage; highest base range in any Mancer's kit
- **Solar Flash** — area BLINDED application; the Photomancer's crowd-control tool
- **Illuminate** — single-target buff/debuff marking that amplifies allied damage
- **Sunburst** — high-cost AoE light explosion that damages and purifies terrain/status simultaneously

---

### Spell 1: Light Beam

| Field | Value |
|---|---|
| **Name** | Light Beam |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (line projectile — travels in a straight line; can hit intervening units; bounces off CRYSTAL terrain tiles) |
| **Range** | 7 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 20 |
| **Element** | Light |
| **Effects Applied** | Deals 20 Light damage to primary target. If target is `ILLUMINATED` (from Illuminate spell or Solar Flash secondary), damage is increased by 20% (24 damage instead of 20). If target is `BLINDED`, Light Beam deals +10 bonus damage (30 total vs BLINDED targets — blinded units cannot react to the beam). |
| **Terrain Interaction** | If the beam path passes through a STEAM_CLOUD tile: the beam is diffracted — it loses focus, reducing damage to 12 on the primary target (light scatters in steam). If the beam path passes through a CRYSTAL terrain tile: the beam bounces off the crystal at a 45-degree angle. The bounced beam continues with the same damage (20) and can hit additional targets in the new direction. |
| **Special Interactions** | Light Beam is blocked by OBSIDIAN and solid walls (no LOS through opaque terrain). It is NOT blocked by units — it can pierce through intervening units if aimed at a target behind them, hitting both. Intervening units take 10 damage (half the full damage). Against a FROZEN target: Light Beam's focused light energy acts as thermal input — the FROZEN status has its remaining duration reduced by 1 turn. This is a minor interaction (it does not melt the freeze outright) but is notable in competitive play. |

**Design note:** Light Beam is the Photomancer's workhorse. At 7-tile range and no cooldown, it operates at distances that most Mancers cannot respond to from their own spell ranges. The 2-AP cost means it can be fired twice in a single activation alongside 2 tiles of movement (2 move + 2 beam + 2 beam = 6 AP). The ILLUMINATED synergy is the primary reason to invest in Illuminate before firing — the +20% damage amplifier turns Light Beam from a moderate-damage repeatable into a consistent high-output threat. The crystal bounce mechanic rewards positional play: a Photomancer who identifies CRYSTAL terrain on the map and angles Light Beams to bounce into a cluster behind cover is one of the highest-skill expressions in the game.

---

### Spell 2: Solar Flash

| Field | Value |
|---|---|
| **Name** | Solar Flash |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Ground Target — AoE Radial (centered on target point) |
| **Range** | 5 tiles (to center of AoE) |
| **AoE Radius** | 2 tiles |
| **Base Damage** | 8 (minor light burst damage) |
| **Element** | Light |
| **Effects Applied** | All units in the 2-tile radius take 8 Light damage and receive `BLINDED` (2 turns). BLINDED units have targeting range reduced to 1 tile; ranged attacks are penalized heavily (per status-effects.md). Additionally, the tiles in the radius become `ILLUMINATED_GROUND` (2-turn duration): tiles that glow with residual light energy — allies standing on ILLUMINATED_GROUND tiles have +1 tile of effective vision range, and enemies on those tiles are treated as having the `ILLUMINATED` unit status (all incoming damage to units on ILLUMINATED_GROUND is +20%). |
| **Terrain Interaction** | Against STEAM_CLOUD tiles: Solar Flash partially dissipates the steam — the STEAM_CLOUD duration is reduced by 1 turn per tile within the AoE. The BLINDED effect still applies (the flash penetrates dissipating steam). Against ON_FIRE terrain: the fire and light interact to create a SEARING_BRIGHT tile — BLINDED duration increases to 3 turns for units on or adjacent to the ON_FIRE tile when flashed. Against ICE_TILE: light refraction off the ice amplifies the flash in a cone from the ice tile — a bonus 1-tile BLINDED burst radiates from the ice tile in the direction away from the Photomancer (secondary fragmentation flash). |
| **Special Interactions** | Solar Flash BLINDED does NOT distinguish between ally and enemy — units friendly to the Photomancer in the AoE are also BLINDED. The Photomancer player must ensure allied units are out of the flash radius or accept the penalty. ILLUMINATED_GROUND tiles created by Solar Flash function as the terrain version of the ILLUMINATED status — this means Geomancer walls and Floramancer overgrowth on those tiles are also visible to allies at extended range (ward effect: allies can "see through" the illuminated zone more clearly). |

**Design note:** Solar Flash is the Photomancer's primary crowd-control tool. Its 8 base damage is minimal — this is not a damage spell. Its value is 100% in the BLINDED application and ILLUMINATED_GROUND zone it creates. The most impactful use is against a dense ranged formation: Solar Flash into a cluster of Crossbow Corps (or Glade Archers, or Wailing Shades) reduces their effective targeting range to 1 tile, turning them from a ranged threat into adjacency-dependent melee fighters for 2 turns. The 2-turn cooldown means the Photomancer can keep the BLINDED effect refreshed on the same cluster every other turn — potentially neutering an entire ranged wing for most of a fight.

---

### Spell 3: Illuminate

| Field | Value |
|---|---|
| **Name** | Illuminate |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Targeted Status — Single target (enemy or ally) |
| **Range** | 7 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 0 (marking spell — no direct damage) |
| **Element** | Light |
| **Effects Applied** | Applies `ILLUMINATED` status (2 turns): all allies deal +20% damage to the marked target (per status-effects.md). The target is also visible to all allies regardless of terrain obstructions — even inside STEAM_CLOUD or DARK_ZONE, an ILLUMINATED target can be targeted by allied spells. **When applied to an ally:** the ally gains a radiance aura — adjacent enemies take 3 Light damage per turn while adjacent. The ally ILLUMINATED status functions as a light-emitter, deterring melee approach. |
| **Terrain Interaction** | Illuminating a unit standing on NECROTIC_ASH tiles: the light energy partially cleanses the necrotic terrain — the NECROTIC_ASH tile the ILLUMINATED unit stands on has its duration reduced by 1 turn (light cleanses death energy). Illuminating a unit standing on ICE_TILE: ice refracts the radiance, creating a rainbow shimmer effect — allies within 2 tiles of the ILLUMINATED+ICE_TILE unit also receive the +20% damage bonus against that unit (the refraction spreads the targeting advantage). |
| **Special Interactions** | ILLUMINATED applied to a unit already under `CONFUSED` status: the Photomancer can see where the CONFUSED unit will randomly target on their turn (the illumination reveals their erratic movement prediction for the Photomancer player — a UI indicator shows the likely random target range). This is primarily a tactical information tool. ILLUMINATED cannot be applied to units in STASIS — they are frozen in time and the light mark cannot attach to a temporally inert unit. |

**Design note:** Illuminate is the Photomancer's damage amplification tool and is best used immediately before allied Mancers activate. At 2 AP with no cooldown, the Photomancer can apply Illuminate and still have 4 AP for a Light Beam (2 AP) and 2 tiles of movement — creating a turn where it marks a priority target and deals bonus damage to it simultaneously. In multi-Mancer warbands, the real value is the +20% ally damage bonus: an Electromancer chain into an ILLUMINATED cluster deals 20% more per arc hit, an ILLUMINATED Mancer that the Necromancer has Death Marked will have a correspondingly amplified Death Mark explosion on death.

---

### Spell 4: Sunburst

| Field | Value |
|---|---|
| **Name** | Sunburst |
| **AP Cost** | 5 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Ground Target — AoE Radial |
| **Range** | 4 tiles (to center) |
| **AoE Radius** | 2 tiles |
| **Base Damage** | 35 (all units in AoE) |
| **Element** | Light |
| **Effects Applied** | Deals 35 Light damage to all units in the 2-tile radius. Applies `BLINDED` (2 turns) to all units hit (including allies in the zone — same friendly-fire rule as Solar Flash). Each tile in the AoE is cleansed: `NECROTIC_ASH` is removed; `BURNING` status on units in the zone is removed (light cleanses fire); `POISONED` stacks are reduced by 2 per unit in the zone; undead summons (Risen Shamblers, Abyssal Horrors) within the AoE take +50% damage from this spell (light is devastating to undead — 35 × 1.5 = 52.5, rounded to 53, which typically one-shots Risen Shamblers). |
| **Terrain Interaction** | Against NECROTIC_ASH: tiles fully cleansed — become standard GROUND. Against TOXIC_TERRAIN: Sunburst purifies poison from 2 tiles within the AoE (closest tiles to center first). Against ON_FIRE: the blast of light and fire energy together creates SEARING_BRIGHT terrain (2 tiles; 2-turn duration: 5 light DoT + 5 fire DoT per turn). Against FLOODED/WET: the light energizes the water — FLOODED tiles in the AoE become ILLUMINATED_GROUND for 2 turns on top of their WET state (stacked: conductive AND marking terrain simultaneously). Against OVERGROWTH: the high-intensity light desiccates plant matter — OVERGROWTH in the AoE is reduced to GROUND (removed) without catching fire (light kills it without burning). |
| **Special Interactions** | Sunburst is the primary counter to Necromancer undead armies. A Photomancer that lands Sunburst into a cluster of Risen Shamblers (30 HP base; 53 damage from Sunburst) destroys them all instantly while simultaneously cleansing the NECROTIC_ASH they stand in and removing POISONED stacks from allied units caught in friendly fire. The tradeoff — 5 AP, 3-turn cooldown, 4-tile range requiring commitment — makes it a dedicated anti-undead tool rather than a general rotation spell. |

**Design note:** Sunburst is the Photomancer's power spell and primary terrain-cleansing ability. Its value scales with the board state: on a clean board against living enemies, it is 35 AoE damage and BLINDED — solid but not exceptional for 5 AP. On a NECROTIC_ASH-covered board full of undead summons and TOXIC_TERRAIN, it is a multi-target kill, terrain reset, and status cleanse simultaneously. Learning to identify the right moment for Sunburst — when the enemy board state gives maximum cleanse value AND the AoE positions well against clustered targets — is the Photomancer's most demanding skill expression.

---

## 4. Terrain Interaction Table

### Light Spell Impact on Existing Terrain States

| Existing Terrain State | What Happens When Light Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **Normal (GROUND)** | Light illuminates the tile | `ILLUMINATED_GROUND` (2 turns) | Takes spell damage + `BLINDED` (if Solar Flash or Sunburst) | Tiles glow; allies gain targeting advantage; enemies on tile take +20% damage from all sources |
| **ON_FIRE** | Light and fire combine | `SEARING_BRIGHT` (2 turns; 5 fire DoT + 5 light DoT per turn) | Takes spell damage + `BLINDED` (3 turns, extended by heat intensity) | SEARING_BRIGHT is more punishing than either ON_FIRE or ILLUMINATED_GROUND alone; melee units cannot comfortably engage through it |
| **FLOODED** | Energized water scatters light | `FLOODED` + `ILLUMINATED_GROUND` (layered) | Takes spell damage + `BLINDED` | Conductive AND marking — Electromancer chains through FLOODED; allied damage bonus active simultaneously |
| **WET** | Light diffuses in moisture | `WET` + `ILLUMINATED_GROUND` (layered) | Takes spell damage + `BLINDED` | Same stack as FLOODED interaction; WET state not removed |
| **ICE_TILE** | Refraction amplifies the flash | `ICE_TILE` (unchanged) + 1-tile secondary BLINDED burst directed away from Photomancer | Takes spell damage + `BLINDED` (3 turns — refracted light compounds) | Ice becomes a light amplifier; BLINDED duration extended; secondary burst hits units on the opposite side of the ice tile |
| **NECROTIC_ASH** | Light purifies death energy | `GROUND` (NECROTIC_ASH removed) | Takes reduced spell damage (–20%: light partially absorbed by necrotic) | Terrain is cleansed; undead in the area take +50% bonus damage (light anti-undead interaction) |
| **TOXIC_TERRAIN** | Light partially burns off poison | `GROUND` (2 adjacent TOXIC_TERRAIN tiles purified first; this tile cleared) | Takes spell damage; 2 POISONED stacks removed | Light does not fully counter poison but reduces it; strongest purification is from Sunburst |
| **STEAM_CLOUD** | Light partially dissipates steam | `STEAM_CLOUD` (duration –1 turn) | Takes spell damage; BLINDED NOT applied through steam (light scatters — see Light Beam notes) | Steam reduces beam coherence; Light Beam loses 8 damage if passing through; BLINDED not applied from inside steam if spell originates from steam |
| **CHARGED** | Light energy interferes with stored charge | `ILLUMINATED_GROUND` (CHARGED removed) | Takes spell damage + 10 Lightning damage (discharge triggered) | Same arc discharge as other trigger interactions; CHARGED consumed |
| **MUD** | Light dries and illuminates mud | `ILLUMINATED_GROUND` (MUD removed; dried by intense light) | Takes spell damage | MUD movement penalty removed; ILLUMINATED_GROUND replaces |
| **OBSIDIAN** | Light cannot penetrate or affect obsidian | `OBSIDIAN` (unchanged) | Takes spell damage (direct hit) | No terrain state change; LOS blocked beyond obsidian |
| **OVERGROWTH** | High-intensity light desiccates plant matter | `GROUND` (OVERGROWTH removed; not burned) | Takes spell damage | Floramancer structures destroyed (light kills without fire — desiccation); NECROTIC_ASH is NOT created (unlike fire); clean removal |
| **PERMAFROST** | Light warms the deep ice gradually | `ICE_TILE` (PERMAFROST partially degraded; requires 2 light hits to fully remove) | Takes spell damage + `BLINDED` (refraction amplified) | PERMAFROST is more resistant than ICE_TILE to light; 2 applications of any light spell will convert to ICE_TILE; a third removes the ice entirely |

### Terrain States Beneficial to the Photomancer

| State | Benefit |
|---|---|
| `ILLUMINATED_GROUND` | Photomancer's own created terrain; allies gain vision advantage and +20% damage bonus on units standing in it |
| `CRYSTAL` terrain | Light Beam bounces off CRYSTAL tiles — the Photomancer gains indirect targeting angles to reach units behind cover |
| `ICE_TILE` | Ice amplifies light flash into extended-duration BLINDED (3 turns instead of 2); useful prep for Solar Flash |
| `ELEVATED` | +1 range on all spells; Light Beam reaches 8 tiles; Solar Flash center point reaches 6 tiles |

### Terrain States Hazardous to the Photomancer

| State | Hazard |
|---|---|
| `STEAM_CLOUD` | Light Beam loses damage when passing through steam; the Photomancer's own high-AP spells are weakened if the board is full of steam |
| `NECROTIC_ASH` | 3 Necrotic dmg/turn to the Photomancer standing in it; not immune |
| `ON_FIRE` | 5 HP/turn DoT on 90 HP pool; creates SEARING_BRIGHT which the Photomancer itself could be blinded in if it enters |
| `DARK_ZONE` | If an enemy Mancer creates magical darkness (a future mechanic or specific upgrade interaction), the Photomancer's light spells are the counter — but DARK_ZONE also reduces its own range until cleared |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

#### Variant A: Piercing Beam (replaces Light Beam) — +20 pts

**Description:** Replaces Light Beam with a fully piercing variant. Piercing Beam travels the full 7-tile range and hits every unit in the line at full damage (20 Light damage per unit, not 10 half-damage for intervening units). Crystal bounce still works. AP Cost: 3 AP (up from 2). Cooldown: 1 turn (Light Beam has 0 cooldown; Piercing Beam trades spammability for reliability and full-line damage).

**Trade-off:** Sacrifices the spam potential of Light Beam for guaranteed full damage on formation-penetrating shots. Best against tightly packed enemy lines where a 7-tile-long arc of full-damage hits could touch 3-4 units simultaneously.

#### Variant B: Blinding Radiance (replaces Solar Flash) — +20 pts

**Description:** Replaces Solar Flash with a directional cone version. Blinding Radiance fires a cone 5 tiles long and 3 tiles wide at maximum range, applying BLINDED to all units in the cone. AoE Radius: cone (not radial). Base Damage: 6 (lower than Solar Flash's 8). Does NOT create ILLUMINATED_GROUND terrain — the light is directional, not residual. Cooldown: 1 turn. AP Cost: 3 AP.

**Trade-off:** Loses the ILLUMINATED_GROUND terrain creation but gains the cone's superior reach and narrowness — better for hitting a line of ranged units arranged in depth rather than a cluster. The 1-turn cooldown (improved from Solar Flash's 2-turn) means BLINDED can be reapplied more frequently.

#### Variant C: Focused Sunburst (replaces Sunburst) — +25 pts

**Description:** Replaces Sunburst with a single-target concentrated version. Focused Sunburst targets one unit within 6 tiles (up from Sunburst's 4 tiles) for 65 Light damage — the highest single-target burst in the Photomancer's kit. BLINDED applied. Undead damage bonus retained (+50%, = 97 damage, which eliminates virtually all undead summons). No AoE. AP Cost: 4 AP (down from 5). Cooldown: 3 turns.

**Trade-off:** Gains range and loses the AoE, terrain cleanse, and multi-target damage. Focused Sunburst is a dedicated sniper ability rather than a crowd-clearing one. Best in warbands where allies handle AoE and the Photomancer is expected to confirm kills on specific high-priority targets.

---

### Passive Traits

#### Passive A: Radiant Presence — +20 pts

**Description:** The Photomancer emits a passive light aura with a 2-tile radius. All units within 2 tiles of the Photomancer that are enemies are treated as having the ILLUMINATED status (+20% damage from all sources). This is always-on — no AP cost — but requires the Photomancer to be within 2 tiles of enemies to apply it, which is dangerous for a 90-HP Mancer with 1 armor. The trade-off is that Radiant Presence allows the Photomancer to maintain ILLUMINATED on all adjacent enemies without spending AP on Illuminate, freeing its action budget for Light Beams and Solar Flashes.

**Design note:** Radiant Presence rewards aggressive Photomancer positioning but punishes misplacement severely. Best used in combination with defensive allies (Geomancer walls, Osteomancer bone constructs) that keep enemies at bay while the Photomancer sits in range of the illumination zone without being directly attackable.

#### Passive B: Eclipse Burn — +25 pts

**Description:** Whenever a BLINDED enemy unit takes damage from any source (including non-Photomancer allies), the damage is increased by 15% (stacks additively with ILLUMINATED: BLINDED + ILLUMINATED = +35% total damage from all allies). Eclipse Burn is the Photomancer's primary damage amplification passive — it rewards applying BLINDED before allies attack rather than after.

**Synergy note:** Eclipse Burn is the Photomancer's strongest upgrade for multi-Mancer warbands. A Pyromancer applying Ember Shot to a BLINDED, ILLUMINATED enemy benefits from +35% damage amplification entirely passively. In a triple-Mancer list with the Photomancer as the control piece, Eclipse Burn makes every allied spell deal significantly more damage against maintained BLINDED targets.

#### Passive C: Photon Absorption — +15 pts

**Description:** The Photomancer is immune to BLINDED status — its own light affinity prevents its vision from being overwhelmed by bright effects. Additionally, the Photomancer takes no damage from SEARING_BRIGHT terrain (the Photomancer can move through the combined fire-light hazard without HP loss). This makes it possible for the Photomancer to follow its own Solar Flash into a zone it just blinded, without taking the friendly-fire BLINDED effect.

**Design note:** Photon Absorption mostly removes an inconvenience rather than adding power — the Photomancer rarely blinds itself, but when it does (Solar Flash aimed at adjacent enemies, or an enemy Photomancer in a mirror match), the immunity matters. The SEARING_BRIGHT immunity is more contextually valuable: in Pyromancer + Photomancer warbands where fire zones overlap with light zones, the Photomancer can advance through its own SEARING_BRIGHT tiles freely.

#### Passive D: Targeting Brilliance — +20 pts

**Description:** All ally spells targeting an ILLUMINATED unit (not just Light Beam) receive an additional +1 tile of range on that cast. If a Pyromancer's Ember Shot has 6-tile range and the target is ILLUMINATED, the Pyromancer can fire that specific shot from 7 tiles. The range bonus applies per cast, per ILLUMINATED target — it does not persist or generalize. This passive encourages the Photomancer to maintain consistent ILLUMINATED application, rewarding allied Mancers with access to priority targets they would otherwise be slightly out of range to hit.

---

### Stat Enhancements

#### Enhancement A: Luminous Constitution (+20 HP) — +15 pts

**Description:** Max HP increases from 90 to 110. Brings the Photomancer to the same durability tier as the Hydromancer, allowing it to absorb one additional moderate hit before reaching critical HP. Most valuable in warbands where the Photomancer operates at shorter range (Radiant Presence builds) and cannot rely on maximum range as its only survival tool.

#### Enhancement B: Photon Speed (+1 Move Range) — +10 pts

**Description:** Move Range increases from 4 to 5 tiles per activation. The Photomancer's repositioning game becomes more fluid — reaching optimal LOS angles for Light Beam bounces, escaping melee threats, or advancing to maintain Solar Flash coverage on a moving enemy formation. One of the most efficient upgrades for the activation cost.

---

### Signature Ability

#### Signature: Solar Convergence — +40 pts

| Field | Value |
|---|---|
| **Name** | Solar Convergence |
| **AP Cost** | 6 AP (entire activation) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Ground Target — massive AoE Radial centered on target point |
| **Range** | 5 tiles (to center) |
| **AoE Radius** | 4 tiles |
| **Base Damage** | 0 (no direct damage on cast) |
| **Element** | Light |
| **Effects Applied** | A massive convergence of solar energy descends on the target area. Every tile in the 4-tile radius becomes `ILLUMINATED_GROUND` (4-turn duration — double the standard 2 turns). All units in the zone are `BLINDED` (3 turns) and `ILLUMINATED` simultaneously — a unit that is both BLINDED and ILLUMINATED is maximally vulnerable: its own targeting is impaired (range reduced to 1) while all incoming damage is amplified (+20%, plus any Eclipse Burn passive bonus). After the initial convergence, at the start of every subsequent turn that ILLUMINATED_GROUND tiles remain in the zone, each tile fires a minor light pulse (5 Light damage) to any unit still standing on it. This is persistent area pressure — a 4-tile radius zone that continuously damages units who stand in it for its duration. Undead units in the zone take the light pulse for +50% (7.5, rounded to 8 Light damage per turn). |
| **Special Interactions** | Solar Convergence does NOT distinguish allies from enemies — the persistent light pulses affect allied units in the zone too. Using Solar Convergence on a zone the Photomancer's own warband occupies is a self-harm play that requires immediate repositioning out of the zone. Best cast into a zone the enemy is committing to (a chokepoint they must hold, an objective tile cluster) where they cannot disengage without surrendering position. ILLUMINATED_GROUND created by Solar Convergence decays like normal ILLUMINATED_GROUND but at the doubled 4-turn duration — it can be extended further only if the Photomancer casts another ILLUMINATED_GROUND spell over the zone before it expires. Solar Convergence into a zone with existing NECROTIC_ASH tiles cleanses them all simultaneously on cast (Sunburst cleanse property scales to the full AoE). |

**Design note:** Solar Convergence is the Photomancer's "this is the battlefield we prepared" ability. Unlike Pyromancer's World Conflagration or Necromancer's Army of the Dead, it does not require prior board-state investment — it generates its own zone on cast. Its value comes from the 4-turn persistence and per-tile DoT: once cast, the Photomancer has denied a large area to the enemy for 4 turns with continuous light pressure, universal BLINDED application, and ILLUMINATED marking that amplifies all allied damage into the zone. Against a Necromancer opponent, it simultaneously destroys undead (light pulses at +50% to undead) and cleanses NECROTIC_ASH terrain — the most complete counter to the Necromancer's board state in a single ability. Its 40-pt upgrade cost is justified by how completely it reshapes the engagement zone for 4 turns.

---

## 6. Faction Synergy

### Best Pairing: The Gilded Throne

The Gilded Throne's disciplined ranged units (Crossbow Corps and Siege Arbalests) deal high sustained damage from range. The Photomancer's ILLUMINATED marking amplifies that already-high single-shot damage by an additional 20%. A Siege Arbalest (Gilded Throne T2 Ranged, fires every turn, armor-piercing) against an ILLUMINATED target is one of the most efficient damage-per-turn combinations available to non-Mancer units in the game.

**Iron Discipline + Photomancer:** The Gilded Throne's Iron Discipline makes Chaff and Ranged units immune to Panic and Charm. The Photomancer's friendly-fire BLINDED risk (Solar Flash in a formation that includes allied units) is partially mitigated because Iron Discipline units with BLINDED can still act without morale collapse — they advance blind if needed. BLINDED Conscript Spearmen lose ranged targeting (they have none — they're melee) and experience no practical disadvantage from BLINDED status compared to other factions' units. This makes Gilded Throne Chaff uniquely immune to the Photomancer's friendly-fire downside.

**Crossbow Corps + ILLUMINATED targets:** Crossbow Corps fire every other turn (alternating attack/reload). On their attack turn, if the Photomancer has ILLUMINATED their target, they deal full armor-piercing damage × 1.2. Over a fight, this consistent amplification effectively gives the Crossbow Corps a 20% DPS increase against Photomancer-marked targets without any change to their behavior.

### Verdant Pact — Moderate Synergy

Verdant Pact's Glade Archers apply POISONED on hit. ILLUMINATED targets that are also POISONED give allied Archers and other Mancers two compounding damage amplifiers: ILLUMINATED (+20% from all sources) and POISONED DoT (3 HP/turn per stack). The Photomancer does not directly interact with POISONED, but applying ILLUMINATED to POISONED enemies creates compounded pressure.

Terrain Bond (Verdant Pact's faction trait) is partially disrupted by the Photomancer: ILLUMINATED_GROUND tiles are not natural tiles for Terrain Bond purposes. The Photomancer's terrain creation (ILLUMINATED_GROUND) does not enable Terrain Bond regen. This is not a hard anti-synergy but means the Photomancer and Verdant Pact bonus systems operate in parallel rather than compounding.

### Ashen Covenant — Counter Relationship

The Photomancer and Ashen Covenant Necromancer have an adversarial design relationship. Sunburst and Solar Convergence are the most efficient hard counters to Necromancer undead armies in the game — light spells deal +50% to undead and cleanse NECROTIC_ASH terrain simultaneously. An Ashen Covenant opponent who fields a Necromancer should expect the opponent's Photomancer to spend most of its Sunburst and Solar Convergence uses on destroying the undead line.

**Within-faction use (Photomancer + Ashen Covenant):** If the Photomancer is in an Ashen Covenant warband, it must never Sunburst or Solar Convergence into tiles containing allied undead summons (Risen Shamblers, Abyssal Horrors). The anti-undead damage makes friendly-fire catastrophic. ILLUMINATED_GROUND terrain created by the Photomancer also deals light pulse damage to undead on those tiles (per Solar Convergence), so even the terrain setup is self-defeating. Photomancer + Ashen Covenant is the weakest Mancer-faction pairing in the game and should be avoided.

---

## 7. Combo Chains

### Combo 1: Photomancer + Electromancer — "Blinded Lightning"

**Mancers involved:** Photomancer + Electromancer

**Sequence:**
1. Photomancer casts Solar Flash into an enemy cluster: all units BLINDED (2 turns) + ILLUMINATED_GROUND terrain created.
2. Electromancer fires Chain Lightning into any unit on the ILLUMINATED_GROUND zone.
3. ILLUMINATED_GROUND's +20% damage modifier applies to the chain arc hits. BLINDED units cannot effectively respond.
4. Additionally, with Eclipse Burn passive (if purchased): BLINDED + ILLUMINATED_GROUND = +35% total damage amplification on chain arc hits.

**Why this works:** The Electromancer's chain arc normally deals solid area damage. Against BLINDED, ILLUMINATED targets, that damage is amplified by 20-35% passively, turning a standard arc sequence into potentially fight-ending burst. The BLINDED status also means STUNNED units from the chain cannot effectively target for their remaining BLINDED turns even after STUNNED expires — two control layers stacking.

---

### Combo 2: Photomancer + Necromancer — "Light and Death" (Counter-play Mirror)

**Mancers involved:** Photomancer + Necromancer (allied — same warband)

**Sequence:**
1. Necromancer raises undead summons and applies Death Mark to a priority enemy target.
2. Photomancer uses Illuminate on the Death-Marked target (+20% to all incoming damage).
3. Necromancer fires Necrotic Bolt (2 AP) into the ILLUMINATED, DEATH_MARK target: 16 × 1.2 = 19.2 → 20 damage per bolt.
4. When the target dies (DEATH_MARK triggers): the explosion deals 40% of max HP in a 2-tile radius; the ILLUMINATED status is on the explosion epicenter, but the explosion itself is Necrotic element — no light amplification on the detonation, but the Photomancer's ILLUMINATED_GROUND terrain from a prior Solar Flash in the zone amplifies allied follow-up on any survivors.

**Coordination requirement:** The Photomancer must NOT use Sunburst near allied undead summons — this combo only works if Sunburst is held off or aimed away from the Necromancer's summoned units. Role clarity matters: the Necromancer handles undead and corpse economy; the Photomancer handles ILLUMINATED marking and BLINDED control without entering undead-kill-zone territory.

---

### Combo 3: Photomancer + Pyromancer — "Searing Radiance"

**Mancers involved:** Photomancer + Pyromancer

**Sequence:**
1. Pyromancer establishes ON_FIRE terrain in an approach zone.
2. Photomancer casts Solar Flash into the ON_FIRE zone — interaction creates SEARING_BRIGHT terrain (5 fire DoT + 5 light DoT per turn; BLINDED extended to 3 turns for units in the zone).
3. Enemies in the SEARING_BRIGHT zone: BLINDED (3 turns), taking 10 combined DoT per turn, unable to target beyond 1 tile.
4. Pyromancer uses Conflagration Wave to fan the fire in the zone; the BLINDED penalty prevents enemies from retreating effectively (randomized targeting if CONFUSED; reduced AP from BLINDED range restriction combined with having to spend AP to escape).

**Why this works:** SEARING_BRIGHT is only achievable through this specific combination. The 3-turn BLINDED (vs Solar Flash's standard 2-turn) combined with double DoT creates a denial zone more punishing than either Pyromancer ON_FIRE alone or Photomancer ILLUMINATED_GROUND alone. A SEARING_BRIGHT zone on an approach path effectively shuts that lane for 3 turns.

---

### Combo 4: Photomancer + Cryomancer — "Ice Mirror Network"

**Mancers involved:** Photomancer + Cryomancer

**Sequence:**
1. Cryomancer converts water tiles or creates ICE_TILE terrain patches across the engagement zone (via Frost Bolt, Flash Freeze, or Flood Zone followed by a mass freeze).
2. Photomancer fires Light Beam aimed at an enemy cluster, angling through ICE_TILE tiles on the path.
3. ICE_TILE refraction: the Light Beam bounces off each ice tile in its path, hitting additional targets in new directions. A network of 3 ICE_TILE tiles creates a 3-bounce light arc that can reach enemies behind cover in multiple directions simultaneously.
4. Each bounce applies the standard ILLUMINATED interaction to the ice tile (3-turn BLINDED on adjacent units from ice refraction effect).

**Why this works:** ICE_TILE terrain is the Photomancer's natural ally — ice is essentially a free CRYSTAL terrain tile for bounce purposes. A Cryomancer who has converted a zone to ICE_TILE provides the Photomancer with an ad-hoc crystal mirror network. The 3-turn BLINDED from ice-refraction is 50% longer than a standard Light Beam BLINDED, and the bounce angles bypass cover that would normally block LOS entirely.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Photomancer

| Mancer | Counter Mechanism |
|---|---|
| **Necromancer** | NECROTIC_ASH terrain interferes with ILLUMINATED_GROUND — necrotic energy partially suppresses light energy at the terrain level. More practically, a Necromancer with many active undead summons creates a screen the Photomancer must use Sunburst on, spending its heaviest AP ability on a single engagement rather than maintaining board-wide BLINDED and ILLUMINATE control. |
| **Aeromancer** | Displacement pushes the Photomancer out of its optimal 7-tile range position. Given the Photomancer's 90 HP and 1 armor, being shoved toward the enemy line is potentially lethal. Aeromancer wind also disperses STEAM_CLOUD (which the Photomancer creates by interaction, occasionally) and can fan Pyromancer fire into the Photomancer's position. |
| **Cryomancer** | FROZEN Photomancer wastes an entire turn — the Photomancer's value is 100% in its spell casting; a skipped turn denies ILLUMINATE, BLINDED refresh, and Light Beam output simultaneously. The Cryomancer also creates ICE_TILE terrain which (while useful for bounces) creates movement hazards for the Photomancer's repositioning. |
| **Psychomancer** | CHARMED Photomancer uses Solar Flash on allied units (applying BLINDED to the Photomancer's own chaff). CONFUSED Photomancer applies ILLUMINATE or Solar Flash to random targets, potentially illuminating enemy units for the enemy's Mancers to exploit. The Photomancer's non-Mancer unit protection via Gilded Throne's Iron Discipline protects allies — but the Photomancer itself is a Mancer, not covered by Iron Discipline. |

### Terrain Compositions That Shut Photomancer Down

| Terrain Setup | Why It's Punishing |
|---|---|
| **STEAM_CLOUD coverage of engagement zone** | Light Beam loses damage through steam; Solar Flash is weakened; the Photomancer's core projectile is blunted by the same visual obstruction it is theoretically designed to counter. Hydromancer + Pyromancer opponents can generate sustained STEAM_CLOUD that degrades the Photomancer's output. |
| **OBSIDIAN walls blocking LOS** | The Photomancer requires LOS for Light Beam and Illuminate. A board where an enemy Geomancer has created OBSIDIAN barriers forces the Photomancer to reposition constantly to find firing angles — costing AP on movement rather than spells. |
| **Dense OVERGROWTH (Floramancer terrain)** | OVERGROWTH blocks LOS in most configurations. A Floramancer opponent who creates vine barriers and OVERGROWTH coverage across the engagement zone severely limits the Photomancer's 7-tile range by making LOS impossible to most target tiles. |

### Warband Compositions That Prey on Photomancer

| Warband Type | Exploitation |
|---|---|
| **Geomancer + Osteomancer (cover-heavy)** | Creates OBSIDIAN barriers and bone construct screens that block LOS. The Photomancer's range advantage is nullified when LOS cannot be established across the board. |
| **Faunamancer companion swarm** | Fast, numerous companion units can close to the Photomancer before Solar Flash covers the approach path. At 90 HP with 1 armor, the Photomancer loses to sustained melee pressure; the companion swarm specifically bypasses the ranged-unit BLINDED vulnerability by simply not using ranged attacks. |
| **Ashen Covenant + Necromancer (NECROTIC_ASH saturation)** | NECROTIC_ASH dampens ILLUMINATED_GROUND. A board covered in NECROTIC_ASH — from Necromancer spells plus Covenant Chaff death zones — partially suppresses the Photomancer's terrain marking output, forcing it to repeatedly use Sunburst to cleanse terrain instead of using it for burst damage. |

---

*End of Photomancer design document.*

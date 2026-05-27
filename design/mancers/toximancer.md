# Toximancer — Full Design Document

---

## 1. Tactical Identity

The Toximancer is the roster's premier attrition specialist — a Mancer that wins not by dealing decisive burst damage but by establishing a mounting toxic debt that eventually overwhelms any unit's capacity to survive it. Every spell the Toximancer casts either adds POISONED stacks to a target unit, applies TOXIC_TERRAIN that accumulates stacks passively, or creates conditions that spread poison exponentially through Contamination mechanics. In isolation, one POISONED stack is a minor irritant (3 HP/turn). Five stacks are a death sentence (15 HP/turn + DEBILITATED). The Toximancer's entire game plan is navigating from one to five as efficiently as possible while denying the opponent access to cleanse mechanics.

Playing the Toximancer well requires understanding the stack economy at a granular level. Each spell has a specific stack-per-AP efficiency; each interaction with terrain or allied Mancers produces additional stacks for free. The Toximancer that mechanically applies one or two stacks per turn and waits is weak — the Toximancer that creates TOXIC_TERRAIN across approach paths, triggers Contamination spreads via Hydromancer WET interaction, and stacks POISONED faster than the opponent can cleanse or mitigate it is one of the most dangerous Mancers in the game. The DEBILITATED threshold (5 stacks) is the skill target. Every activation should advance at least one unit toward 5 stacks or set up a Contamination event that does so in bulk.

**Primary win condition:** The Toximancer wins by pushing multiple enemy units to 3+ POISONED stacks simultaneously, creating a passive damage output that exceeds what the opponent's healing and cleanse capacity can address each turn. At 3 stacks (9 HP/turn) on three units simultaneously, the board is dealing 27 HP of damage every turn with zero additional Toximancer involvement. The Hydromancer Contamination spread — pushing one 4-stack unit into WET terrain to spread 1 stack each to 4 adjacent units — is the most efficient single-action stack distribution available to any two-Mancer combination in the game.

**Core weakness:** POISONED is the most cleanable status in the game. Hydromancer Mending Current removes all stacks; Photomancer Sunburst clears POISONED from an area; Chronomancer Rewind reverts all statuses on a target. A warband with access to reliable cleanse mechanics can undo the Toximancer's stack economy turn by turn. Additionally, POISONED is physical — it is not morale-based. This means Iron Discipline (Gilded Throne) provides no protection, and the Toximancer is one of the few Mancers that works identically against all factions. The Toximancer's weakness is timing-dependent: if the opponent cleanses at exactly the right moment, multiple turns of stack investment is erased.

**Toximancer vs. Floramancer poison (design distinction):** Toximancer's POISONED stacks are venom-based — concentrated toxins applied via direct delivery (spells, TOXIC_TERRAIN contact, Contamination spread). Venom stacks last 3 turns per stack. Floramancer's POISONED stacks are plant-origin — pollen and spore contact, applied by SPORES terrain. Pollen stacks last 2 turns per stack. Both contribute to the same 5-stack cap and the same 3 HP/turn-per-stack damage, but venom stacks persist longer, making Toximancer the superior sustained-stack builder and Floramancer the superior terrain-seeder for single-exposure bursts.

**Toximancer vs. Iron Discipline confirmation:** POISONED stacks are physical venom. They are not morale-based, fear-based, or charm-based. Gilded Throne's Iron Discipline (immunity to Panic and Charm; reduced duration on morale debuffs) provides zero protection against POISONED stacks. Conscript Spearmen and Iron Vanguard veterans are fully vulnerable to Toximancer poison. This is an intentional design decision — the one faction with the clearest counter to Psychomancer has no counter to Toximancer.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 85 | Glass-cannon tier adjacent; expects to stay out of melee |
| **Move Range** | 4 tiles per activation | Slightly above average; needs mobility to stay out of range while stacks accumulate |
| **Base Armor** | 1 | Light; survives on poison pressure keeping enemies back |
| **Spell Range** | 5 tiles (base) | Each spell lists its own range; this is the fallback reference |
| **AP per Activation** | 6 | Standard for all Mancers; movement costs 1 AP per tile |
| **Element** | Poison/Venom | All base spells deal Poison damage and apply POISONED status or TOXIC_TERRAIN |

**AP budget example:** With 6 AP, the Toximancer can move 2 tiles (2 AP), cast Venom Dart twice (2 + 2 = 4 AP) to apply 2 stacks to two targets or 4 stacks to one target (two darts, two stacks per dart). Or: move 1 tile (1 AP), cast Venomous Ground (3 AP), cast Venom Dart (2 AP) — terrain seeding plus single-target stacking in one activation.

---

## 3. Base Spell Kit

The Toximancer's four base spells cover distinct combat functions:
- **Venom Dart** — primary stack applicator; reliable and repeatable
- **Venomous Ground** — TOXIC_TERRAIN creation; passive stack accumulation on approach paths
- **Toxic Surge** — AoE stack distributor; the Contamination engine
- **Virulent Injection** — heavy single-target stacker; confirms DEBILITATED threshold on key targets

---

### POISONED Stack Progression — Full Reference

| Stacks | HP/Turn Damage | Additional Effect |
|---|---|---|
| 1 stack | 3 HP/turn | Minor DoT; negligible threat to full-HP units |
| 2 stacks | 6 HP/turn | Noticeable; over 10 turns equals 60 damage |
| 3 stacks | 9 HP/turn | Significant sustained damage; 100-HP Mancer takes lethal damage in ~11 turns |
| 4 stacks | 12 HP/turn | Urgent for opponent; forces cleanse or retreat |
| 5 stacks (cap) | 15 HP/turn + `DEBILITATED` | DEBILITATED: Move Range –1, Spell Range –1 for all spells. Maximum stack state. |

**Tick timing:** POISONED ticks deal damage equal to `3 × stackCount` HP at the start of the affected unit's activation. This value matches the StatusManager.cs implementation: each tick calls `3 × currentStacks` damage to the unit.

**Stack duration:** Each venom-origin POISONED stack lasts 3 turns from application. Stacks decay independently — the oldest stack expires first. If the Toximancer applies 2 stacks on turn 1 and 2 more stacks on turn 3, the first 2 stacks expire at turn 4 while the second 2 stacks still have 2 turns remaining. Managing the decay curve by re-applying stacks before the earliest ones expire is the Toximancer's primary mechanical skill challenge.

---

### CONTAMINATION Mechanic — Full Rules

**Trigger:** When a unit with at least 1 POISONED stack is hit by any Water-element spell, OR moves through or ends their turn on `WET` terrain, **Contamination** triggers.

**Resolution:** All units adjacent (orthogonal and diagonal; 8-tile adjacency) to the triggering unit receive 1 POISONED stack each (venom-origin; 3-turn duration). The triggering unit does not gain additional stacks. Contamination spread requires physical proximity — only units within 1 tile of the POISONED unit at the moment of the WET interaction receive the spread stack.

**Stack cap compliance:** Contamination spread respects the 5-stack cap. Units already at 5 stacks do not receive additional stacks from Contamination. Units at 4 stacks receive 1 stack and reach cap (and become DEBILITATED if not already).

**Contamination does not chain:** A unit that receives 1 stack from Contamination does not itself trigger another Contamination event if it subsequently encounters WET terrain, unless it was already at 1+ stacks before the spread. In other words, receiving a spread-stack alone does not make a unit a Contamination vector — it must then accumulate stacks through normal means OR the original spread-stack puts it over 1 total (it already had stacks).

**Practical example:** Toximancer poisons a Siege Boar to 4 stacks. The Boar walks onto a WET tile. Contamination: all 8 adjacent units receive 1 POISONED stack. If 4 of those adjacent units were already at 2 stacks, they become 3 stacks (9 HP/turn). If 1 was at 4 stacks, it becomes 5 stacks (DEBILITATED). One Contamination event from one 4-stack Boar can push multiple units to higher stack tiers simultaneously — this is the highest AP-efficiency poison spread in the game.

---

### Spell 1: Venom Dart

| Field | Value |
|---|---|
| **Name** | Venom Dart |
| **AP Cost** | 2 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (projectile; travels in line; can hit intervening units) |
| **Range** | 7 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 8 |
| **Element** | Poison |
| **Effects Applied** | Applies 2 stacks of `POISONED` (3 HP/turn per stack; 3-turn venom-origin duration) to hit unit. Tile beneath target becomes `TOXIC_RESIDUE` — a minor terrain marker (not full TOXIC_TERRAIN; movement cost unchanged; units on TOXIC_RESIDUE tile take 1 POISONED stack if they end their turn there for up to 2 turns). |
| **Terrain Interaction** | Firing into `WET` terrain: if the projectile hits a WET tile/unit, Contamination check triggers immediately on the unit struck (the dart venom + water = instant spread). The Contamination fires before additional stacks are counted, meaning the struck unit's current stacks spread, then the dart's 2 stacks are added. Firing into `SPORES` terrain: the dart disrupts the spore cloud — SPORES terrain in the tile hit converts from SPORES to `TOXIC_SPORES` (hybrid: 1 venom POISONED stack per movement through, longer-duration spore effect). |

**Temperature Effects:** **0 temperature change.** Venom is thermally neutral — the dart delivers concentrated toxins with no heat or cold component. Exception: if the target is WARM (+1 to +30 temperature), the ambient heat slightly potentiates the venom — apply an extra 0.5 effective stack, rounded up to 1 additional stack when the target is at +20 temperature or higher. At +20 or above WARM, a single Venom Dart effectively applies 3 stacks instead of 2.

**Design note:** Venom Dart is the Toximancer's workhorse — 2 stacks for 2 AP, no cooldown, longest range on the spell list (7 tiles). It can be cast twice per activation (2 + 2 = 4 AP) to apply 4 stacks total, split between two targets or combined on one. Two darts on one target applies all 4 stacks: at turn start they deal 12 HP/turn (4 × 3). Double-darting one high-value target and walking one tile is a standard Toximancer activation that establishes a kill threat within 2 turns. The TOXIC_RESIDUE terrain marker is a secondary benefit — units who stand still on the dart's impact tile take 1 extra stack, punishing stationary enemies.

---

### Spell 2: Venomous Ground

| Field | Value |
|---|---|
| **Name** | Venomous Ground |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Ground Target — AoE Radial; creates TOXIC_TERRAIN |
| **Range** | 5 tiles (to center of AoE) |
| **AoE Radius** | 2 tiles |
| **Base Damage** | 5 (toxin eruption on-cast; minor) |
| **Element** | Poison |
| **Effects Applied** | All tiles in radius become `TOXIC_TERRAIN` (persists 5 turns; does not spread naturally). Any unit that enters or ends their turn on TOXIC_TERRAIN receives 1 POISONED stack per turn. Units who move through TOXIC_TERRAIN receive 1 stack per tile crossed (not just per end-of-turn). |
| **Terrain Interaction** | On `GROUND`: TOXIC_TERRAIN created (standard). On `WET`: water-diluted poison — terrain becomes `WET_TOXIC` (TOXIC_TERRAIN properties + WET conductivity); units on WET_TOXIC terrain are both in poison ground AND conductive (Electromancer chains propagate). Additionally: WET dilution reduces stack accumulation to 1 stack every 2 turns instead of every turn (water weakens concentrated venom). On `SPORES` (Floramancer): Floramancer's plant-origin spores amplify with ground venom — SPORES terrain becomes `VIRULENT_SPORES` (2 POISONED stacks per movement through, venom-duration 3 turns). This is the Floramancer + Toximancer VIRULENT_SPORES interaction. On `ON_FIRE`: ground venom ignites — creates `TOXIC_FIRE` hybrid (both BURNING and 1 POISONED stack per turn; standard Pyromancer interaction). On `ICE_TILE`: venom freezes into the ice — `TOXIC_ICE` terrain (venom preserved until ice melts; when a Fire spell or thaw effect hits TOXIC_ICE, the venom releases: all units within 2 tiles receive 2 POISONED stacks simultaneously from the venom burst). On `CHARGED`: venom conducts charge — CHARGED is consumed; chain arc fires through all adjacent units; TOXIC_TERRAIN forms normally after discharge. |

**Temperature Effects:** **0 temperature change.** Ground venom is chemically inert thermally — seeding TOXIC_TERRAIN does not heat or cool the environment.

**Design note:** Venomous Ground is the Toximancer's passive-income spell — it seeds an approach zone that generates stacks every turn without requiring the Toximancer to directly target units. Against infantry that must advance through the zone, it creates 1 stack per tile crossed. A 4-tile advance through Venomous Ground creates 4 stacks. Combined with 2 stacks from Venom Dart, a unit advancing through poisoned ground and being dart-targeted reaches 6 stacks in one activation — capped at 5 (DEBILITATED), with 1 wasted. The Toximancer should use Venomous Ground to poison choke points and high-traffic corridors, then use Venom Dart to confirm the DEBILITATED threshold on units that passed through.

---

### Spell 3: Toxic Surge

| Field | Value |
|---|---|
| **Name** | Toxic Surge |
| **AP Cost** | 3 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Single Target (targeted unit; triggers Contamination) |
| **Range** | 5 tiles |
| **AoE Radius** | N/A (single target triggers; adjacency spread is Contamination mechanic) |
| **Base Damage** | 12 |
| **Element** | Poison |
| **Effects Applied** | Deals 12 damage to the target. Applies 1 POISONED stack to the target. Then: if the target is standing on WET terrain OR is already POISONED (any number of stacks), Contamination immediately triggers — all 8 adjacent units receive 1 POISONED stack each. The trigger requires either WET terrain OR prior POISONED status on the target (not both mandatory — either condition fires Contamination). |
| **Terrain Interaction** | The spell itself applies 1 stack to the target; if the target is WET OR was already POISONED, Contamination fires from the target's position. This means Toxic Surge is the Toximancer's Contamination engine — it deliberately enables Contamination spread without requiring the target to physically move through WET terrain (the spell itself triggers the water-venom interaction if the target is WET). Hitting a BURNING target with Toxic Surge: BURNING + POISONED = Toxic Combustion (POISONED converts to BURNING DoT; AoE toxic fumes affect adjacent units — see status-effects.md). This converts the Toximancer's stacks into a Pyromancer-adjacency combo without requiring a Pyromancer ally. |

**Temperature Effects:** **0 temperature change.** Toxic Surge delivers poison via venom fluid — no thermal component.

**Design note:** Toxic Surge is the Toximancer's Contamination activator. Its primary use is not the 12 damage or even the 1 direct stack — it is the forced Contamination event. A Hydromancer who has WET a target (1-2 stacks from Venom Dart) gives the Toximancer a Toxic Surge target that spreads to all 8 adjacent units in one 3-AP cast. Against a clustered formation, Toxic Surge can distribute POISONED stacks to 8 units simultaneously with a single spell. Against an already-POISONED target (any stacks), Toxic Surge triggers Contamination regardless of terrain — the Toximancer does not need the Hydromancer to be active if the target was already poisoned by any prior source.

---

### Spell 4: Virulent Injection

| Field | Value |
|---|---|
| **Name** | Virulent Injection |
| **AP Cost** | 4 AP |
| **Cooldown** | 3 turns |
| **Targeting Type** | Single Target (melee-range injection; requires Toximancer to be adjacent to target — 1-tile range) |
| **Range** | 1 tile (melee contact required) |
| **AoE Radius** | N/A |
| **Base Damage** | 20 |
| **Element** | Poison |
| **Effects Applied** | Deals 20 damage. Applies 3 POISONED stacks immediately. Additionally, the injected venom is virulent — the stacks applied by Virulent Injection each last 4 turns (instead of standard 3-turn venom duration), making them persist longer than any other Toximancer delivery method. If the target was already at 2+ stacks, Virulent Injection pushes them to 5 stacks (DEBILITATED) immediately (2 existing + 3 injected = 5 cap). |
| **Terrain Interaction** | Being at melee range to inject means the Toximancer is in close proximity to the target. If the target is on TOXIC_TERRAIN, the Toximancer also takes 1 POISONED stack (standing on its own ground venom while adjacent). On WET terrain: Virulent Injection + WET = instant Contamination from the target (the 3 injected stacks + WET trigger = 1 stack spreads to all 8 adjacent units immediately). Injecting into a FROZEN target: the virulent venom is preserved in the frozen state (POISONED persists through FROZEN per status-effects.md) — when the FROZEN ends, the virulent stacks tick at full damage. |

**Temperature Effects:** **0 temperature change.** Injected venom is biochemically active but thermally neutral.

**Design note:** Virulent Injection is the Toximancer's high-commitment melee finisher. It requires closing to 1-tile range — a major risk for an 85-HP Mancer with 1 armor — but delivers the fastest DEBILITATED application in the kit: 3 stacks in one cast, with 4-turn duration. Against a unit already at 2 stacks (achievable via two Venom Darts earlier in the fight), Virulent Injection confirms DEBILITATED immediately. The 4-AP cost + melee requirement means it consumes most of the Toximancer's activation: 2 tiles of movement + Virulent Injection = 2 + 4 = 6 AP full turn. It should be used as a closing move against isolated or low-HP targets that the Toximancer can safely reach, not as an opening spell.

---

## 4. Terrain Interaction Table

### Poison/Venom Spell Impact on Existing Terrain States

| Existing Terrain State | What Happens When Poison Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **GROUND (normal)** | Venom seeps into soil; TOXIC_TERRAIN formed if ground-target spells used | `TOXIC_TERRAIN` (from Venomous Ground); `TOXIC_RESIDUE` (from Venom Dart) | Takes spell damage + POISONED stacks per spell | Standard interaction; all Toximancer terrain spells work optimally on bare GROUND |
| **WET** | Water conducts and dilutes venom; Contamination trigger condition met | `WET_TOXIC` (from Venomous Ground) — reduced stack rate (1 stack per 2 turns); WET unit triggers Contamination from any POISONED source | WET unit + POISONED = Contamination fires to 8 adjacent units; unit takes spell damage + stacks | Most important interaction in Toximancer's kit; Hydromancer pre-WET converts Toxic Surge into a mass Contamination event |
| **FLOODED** | Large water mass dilutes venom significantly | FLOODED remains; TOXIC_TERRAIN cannot form on FLOODED tiles (water too deep) | Takes spell damage + 1 POISONED stack only (severe dilution); no terrain change | Toximancer cannot establish TOXIC_TERRAIN on FLOODED tiles; Hydromancer flooding a zone negates Toximancer ground control in that area |
| **ON_FIRE** | Venom ignites — creates toxic fume zone | `TOXIC_FIRE` hybrid (ON_FIRE + 1 POISONED stack/turn to units on tile; lasts 4 turns) | Takes spell damage + BURNING + 1 POISONED stack | TOXIC_FIRE is one of the highest-density passive terrain states: fire DoT + poison stack accumulation. Extinguishing with water removes fire but leaves TOXIC_TERRAIN residue |
| **SPORES (Floramancer)** | Floramancer pollen amplified by venom | `VIRULENT_SPORES` (2 POISONED stacks per movement through; venom-duration 3 turns per stack) | Takes spell damage + 2 POISONED stacks immediately (from virulent cloud) | Core Floramancer + Toximancer interaction; makes SPORES terrain twice as effective at stacking |
| **ICE_TILE** | Venom preserved in frozen substrate | `TOXIC_ICE` — venom inactive until thaw; on melt (Fire spell or water): 2-stack burst to all units within 2 tiles | Takes spell damage; POISONED stacks are stored (do not tick while frozen) | Delayed venom trap; enemy Cryomancer accidentally creating a TOXIC_ICE zone that thaws into a 2-stack burst is a meaningful risk |
| **MUD** | Venom pools in mud; movement through absorbs toxin | `TOXIC_MUD` (movement cost 2.0 from MUD + 1 POISONED stack per tile entered — slow + poison combined) | Takes spell damage + 1 POISONED stack from mud contact | Hydromancer WET + Geomancer MUD + Toximancer Venomous Ground = TOXIC_MUD (slow + poison); a punishing combined control zone |
| **CHARGED** | Venom and electricity discharge together | CHARGED consumed; TOXIC_TERRAIN forms; arc fires to adjacent units (10 Lightning AoE) | Takes spell damage + 10 Lightning + POISONED stacks | CHARGED is consumed in the discharge; TOXIC_TERRAIN remains; adjacent arc targets also receive 1 POISONED stack from venom vapor in the explosion |
| **NECROTIC_ASH (Necromancer)** | Necrotic energy and venom merge | `TOXIC_NECROTIC` hybrid (3 Necrotic/turn + 1 POISONED stack/turn; 4-turn duration) | Takes spell damage + enters TOXIC_NECROTIC terrain | Most punishing combined ground state available; requires both Toximancer and Necromancer to be active on the same battlefield area |
| **OBSIDIAN** | Impervious; venom runs off | OBSIDIAN unchanged | Takes spell damage | Toximancer cannot create TOXIC_TERRAIN on obsidian; Geomancer hardening a path prevents TOXIC_TERRAIN establishment there |
| **OVERGROWTH (Floramancer)** | Venom absorbed by organic material | OVERGROWTH remains + `TOXIC_RESIDUE` beneath (weaker poisoning; 1 stack per 3 turns on tile) | Takes spell damage + 1 POISONED stack from toxic plant contact | OVERGROWTH weakly absorbs venom; less effective poisoning but the concealment property remains |
| **TOXIC_TERRAIN (existing)** | Venom stacks reinforce ground concentration | TOXIC_TERRAIN refreshed (duration reset to 5 turns); stack accumulation rate unchanged | Takes spell damage + immediate 1 POISONED stack from the concentrated venom splash | Refreshing existing TOXIC_TERRAIN extends the passive income from ground poisoning without creating new terrain |

### Terrain States Beneficial to the Toximancer

| State | Benefit |
|---|---|
| `WET` / `FLOODED` | Primary Contamination trigger; Hydromancer pre-WET converts any POISONED unit into a Contamination vector |
| `SPORES` (Floramancer) | Converts to VIRULENT_SPORES when Venomous Ground hits; doubles spore stack rate |
| `TOXIC_TERRAIN` | Passive stack accumulation without Toximancer AP investment; refresh with Venomous Ground for persistent output |
| `NECROTIC_ASH` (Necromancer) | Merges into TOXIC_NECROTIC — the deadliest passive ground state in the game |
| `FROZEN` units | POISONED stacks persist through FROZEN (do not decay while unit is frozen); a FROZEN POISONED unit will resume taking DoT immediately when thawed |

### Terrain States Hazardous to the Toximancer

| State | Hazard |
|---|---|
| `FLOODED` | Prevents TOXIC_TERRAIN establishment; Hydromancer flooding negates Toximancer ground control |
| `OBSIDIAN` | Venom cannot penetrate; zones hardened by Geomancer are immune to ground poison seeding |
| `ON_FIRE` | Toximancer is not immune to fire; entering ON_FIRE terrain takes 5 HP/turn. Fire converts TOXIC_TERRAIN to TOXIC_FIRE (different state — removes pure poison ground) |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

#### Variant A: Plague Dart (replaces Venom Dart) — +20 pts

**Description:** Replaces Venom Dart with a slower but more potent delivery. Plague Dart deals 12 damage (vs. 8) and applies 3 POISONED stacks (vs. 2) on hit. However, the stacks are applied over 2 turns: 1 stack on impact, 1 stack at the start of target's next turn, 1 stack one turn after that (progressive application). Cooldown: 1 turn (cannot be double-cast). Range: 7 tiles (unchanged). AP cost: 3 AP.

**Trade-off:** Higher total stack application (3 vs. 2) but spread across 2 turns and costs 1 more AP. The progressive application means a Plague Dart on turn N delivers 1 stack immediately + 2 stacks over turns N+1 and N+2. Best for a sustained single-target focus strategy where the Toximancer targets one high-HP unit for elimination over several turns.

#### Variant B: Acid Cloud (replaces Toxic Surge) — +25 pts

**Description:** Replaces Toxic Surge with a 3-tile-radius AoE toxic cloud. Acid Cloud creates a `TOXIC_CLOUD` that persists 2 turns — all units who enter or remain in the cloud take 10 acid damage per turn and 1 POISONED stack per turn. AP cost: 4 AP; Cooldown: 3 turns; Range: 4 tiles to center. No Contamination trigger — Acid Cloud replaces the single-target precision of Toxic Surge with wide-area persistent cloud coverage. The cloud obscures vision partially (25% miss chance against units inside from outside, and vice versa — like a weaker STEAM_CLOUD).

**Trade-off:** Sacrifices the Contamination activation and single-target precision of Toxic Surge for AoE coverage and sustained damage. Best paired with a Cryomancer or Geomancer who can funnel enemies through the cloud rather than the Hydromancer setup that makes Toxic Surge optimal.

---

### Passive Traits

#### Passive A: Venomous Contact — +20 pts

**Description:** When any unit makes a melee attack against the Toximancer, the attacking unit receives 1 POISONED stack (the Toximancer's skin secretes toxins). This is fully passive — the Toximancer takes the melee damage normally, but the attacker also receives 1 venom stack per melee hit. Over multiple melee attacks, the attacker accumulates stacks organically. This discourages sustained melee focus on the Toximancer without requiring AP investment.

**Synergy note:** Against a melee-heavy opponent (Osteomancer constructs, Faunamancer Siege Boar), Venomous Contact creates a risk-reward tension: continue dealing melee damage and accumulate stacks, or disengage and let the Toximancer freely apply ranged stacks. Both choices favor the Toximancer.

#### Passive B: Contamination Mastery — +25 pts

**Description:** The Contamination mechanic is enhanced: in addition to the standard trigger (WET terrain or WET spell contact), Contamination also triggers when a POISONED unit takes direct damage from any source (not just water contact). The spread is reduced — dealing non-water damage to a POISONED unit spreads only to 4 adjacent units (orthogonal; not diagonal) rather than all 8. This alternate Contamination trigger is the "venom-burst" interaction: physical hits rupture the toxin, spreading it to nearby units.

**Trade-off:** Contamination is now triggered by any damage, not just water. An enemy Mancer trying to eliminate a high-stack POISONED unit with burst damage inadvertently spreads the poison to surrounding allies. This passive fundamentally changes the opponent's optimal strategy — they cannot burst-kill a POISONED unit without considering the spread. Pairs most aggressively with Faunamancer (beast attacks on POISONED units trigger Contamination) and Electromancer (lightning to a POISONED WET unit triggers both water Contamination and Contamination Mastery's physical Contamination simultaneously).

#### Passive C: Virulent Constitution — +15 pts

**Description:** The Toximancer is immune to all POISONED stacks (cannot accumulate its own venom stacks or stacks from any external source). This includes Floramancer SPORES terrain, Verdant Pact Glade Archer poison arrows, and all Contamination spread events. The Toximancer's venom-resistant biology protects it from the primary hazard of operating in its own TOXIC_TERRAIN zones.

**Design note:** Virulent Constitution allows the Toximancer to advance freely through its own TOXIC_TERRAIN and VIRULENT_SPORES zones without taking POISONED stacks, effectively granting free movement through a terrain state that costs everyone else stacks per tile. In late-game TOXIC_TERRAIN-heavy boards, the Toximancer can reposition through its own ground freely while enemies cannot.

#### Passive D: Accelerated Toxins — +20 pts

**Description:** All POISONED stacks applied by the Toximancer through direct spells (not terrain contact) are "accelerated" — they tick at the start of the afflicted unit's activation AND at the end of their activation (double tick per round). Terrain-contact stacks (from TOXIC_TERRAIN) tick normally (once per activation). This effectively doubles the DoT output of directly-applied stacks without changing the stack count.

**Example:** A unit with 3 Toximancer-applied stacks normally takes 9 HP/turn. With Accelerated Toxins, they take 9 HP at activation start + 9 HP at activation end = 18 HP per full round. At 5 stacks with DEBILITATED: 15 HP × 2 = 30 HP per round from poison alone.

**Trade-off:** Very powerful — but only applies to spell-delivered stacks. TOXIC_TERRAIN passive accumulation is not accelerated. Best in a "direct delivery" Toximancer build using Venom Dart and Virulent Injection as the primary stack sources, with terrain used for area denial rather than stack accumulation.

---

### Stat Enhancements

#### Enhancement A: Toxin Immunity Shell (+20 HP) — +15 pts

**Description:** Max HP increases from 85 to 105. The Toximancer remains squishy relative to tanky Mancers, but 20 additional HP is the difference between surviving a Pillar of Flame burst and dying outright. Best for Toximancers who need to close to Virulent Injection range — the extra HP allows one additional failed positioning without a kill.

#### Enhancement B: Venomous Stride (+1 Move Range) — +10 pts

**Description:** Move Range increases from 4 to 5 tiles per activation. The Toximancer's most common frustration is being one tile short of Virulent Injection range or one tile short of Venomous Ground placement. +1 movement resolves both edge cases without altering the AP economy or spell power.

---

### Signature Ability

#### Signature: Pandemic — +40 pts

| Field | Value |
|---|---|
| **Name** | Pandemic |
| **AP Cost** | 6 AP (entire activation; Toximancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Self-centered global — no targeting cursor; affects all POISONED units on the board |
| **Range** | N/A (global; affects every POISONED unit regardless of position) |
| **AoE Radius** | Global (all POISONED units on board) |
| **Base Damage** | 0 (stack amplification; no direct damage) |
| **Element** | Poison |
| **Effects Applied** | Every unit on the board currently carrying at least 1 POISONED stack has their stacks immediately doubled (up to the 5-stack cap). A unit with 2 stacks becomes 4 stacks. A unit with 3 stacks becomes 5 stacks (DEBILITATED). A unit with 4 or 5 stacks reaches 5 stacks (if not already) and remains at DEBILITATED. The doubled stacks are all venom-origin with a fresh 3-turn duration (duration refreshed to maximum regardless of original stack age). Additionally, for 2 turns after Pandemic is cast, all Contamination events spread to 2 stacks per adjacent unit instead of 1. |
| **Special Interactions** | Pandemic is useless if no enemy units have POISONED stacks — like World Conflagration on a clean board, it has zero effect but still consumes the full AP and cooldown. It rewards the Toximancer for sustained stack-building over previous activations. Against a board where the Toximancer has spent 3 turns applying stacks to 4-5 enemy units, Pandemic doubles all of them simultaneously — potentially putting an entire enemy formation at DEBILITATED in one cast. Pandemic does not affect allies with POISONED stacks (friendly units' stacks are not doubled) — it selectively amplifies enemy stacks. The 2-turn enhanced Contamination is particularly powerful if paired with a Hydromancer's water spell the same turn — the Contamination from any WET interaction in those 2 turns spreads 2 stacks instead of 1. |

**Design note:** Pandemic is the Toximancer's "this is what all the stacking was for." Against a board with moderate stack distribution (3 enemy units at 2-3 stacks each), Pandemic converts those units to 4-5 stacks in one instant — pushing most of them to DEBILITATED without the Toximancer having spent AP specifically targeting each one. The 5-turn cooldown and stationary activation cost mean it is a mid-game pivot ability at earliest, and the requirement for existing stacks means the Toximancer must have been active and productive before casting. The enhanced Contamination bonus in the 2 turns after Pandemic incentivizes the Toximancer player to have a Hydromancer ally WET the most clustered enemy group immediately after — doubling Contamination stacks on top of an already-doubled base is potentially fight-ending.

---

## 6. Faction Synergy

### Best Pairing: The Verdant Pact

The Verdant Pact is the Toximancer's strongest faction pairing, and it is a frequently referenced synergy in the warbands.md faction table.

**The Glade Archer amplification loop:**
Glade Archers apply 1 POISONED stack per hit as a base trait. Wyrmwood Striders (T2 Ranged) apply 2 stacks per hit and leave Spore Trails (1 stack per tile). In a Verdant Pact warband, non-Mancer units are continuously applying POISONED stacks to every enemy they hit — the Toximancer enters a board where enemies have 1-3 stacks already accumulated and can use Venom Dart, Toxic Surge, and Virulent Injection to push them from "manageable" to DEBILITATED in one activation rather than building from 0.

| Mechanism | Effect |
|---|---|
| Glade Archer poison hits | 1 stack per hit; multiple Archers targeting the same enemy unit in one activation = 4 stacks before Toximancer acts |
| Wyrmwood Strider (T2) | 2 stacks per hit + Spore Trail (1 stack per tile moved through); combined with Toximancer TOXIC_TERRAIN, the approach corridor has both TOXIC_TERRAIN stack-per-tile and Spore Trail stack-per-tile — entering the corridor is a 2-stack-per-tile experience |
| Floramancer + Toximancer + Verdant Pact | Triple-Mancer Verdant Pact with Floramancer and Toximancer: Floramancer creates SPORES terrain → Toximancer Venomous Ground converts to VIRULENT_SPORES → Glade Archers fire poison into the trapped ROOTED units in VIRULENT_SPORES. A ROOTED unit in VIRULENT_SPORES receiving Glade Archer shots can reach 5 stacks (DEBILITATED) in 2 turns without the Toximancer directly targeting it |
| Rootwarden entrench creates natural terrain | Toximancer TOXIC_TERRAIN cannot overwrite natural terrain tiles (natural tile classification is maintained underneath); however, Rootwarden positions are typically not in the TOXIC_TERRAIN zone — the Toximancer seeds ground ahead of the Rootwarden screen, and the Rootwarden holds behind it |

### The Gilded Throne — Unexpectedly Strong

Gilded Throne is the second-best faction pairing despite having no poison-specific synergy, for a specific structural reason: Iron Discipline protects Throne infantry from Psychomancer disruption but does NOT protect against POISONED stacks. The Toximancer in a Gilded Throne warband uniquely provides a poison vector that the opponent's anti-morale protection entirely ignores.

**The Iron Discipline ruling confirmation:** POISONED stacks are a physical venom status — they deal HP damage and apply a physical stat reduction (DEBILITATED). They are classified as physical/elemental status, not morale-based. Iron Discipline (immunity to Panic, Charm, and reduced morale debuff duration) has zero interaction with POISONED. Toximancer in Gilded Throne is fully effective against any warband, including mirror Gilded Throne opponents. This makes the Toximancer the go-to counter-pick against opponent Gilded Throne warbands that expect to be Psychomancer-resistant — the resistance does not extend to venom.

Crossbow Corps + Toximancer DEBILITATED targets: Siege Arbalests (T2 Ranged) fire every turn and target units behind full cover at 50% penalty. DEBILITATED units (-1 spell range) cannot reach as far with their own spells; DEBILITATED units cannot move as far to get to full cover. Combined: Siege Arbalests continuously chip a DEBILITATED unit even behind partial cover while the Toximancer maintains the stack count.

### The Ashen Covenant — Synergistic but Competing

The Ashen Covenant has two meaningful Toximancer interactions documented in the Necromancer doc:

**Necromancer + Toximancer "Poison Farm":** TOXIC_TERRAIN kills enemies via stacks → Standard Corpses on poisoned tiles → Necromancer raises on TOXIC_TERRAIN tiles (takes minor HP from standing in poison) → Necrotic Eruption on corpse-rich TOXIC_TERRAIN creates TOXIC_NECROTIC ground (most punishing passive terrain state). This is the game's highest-value Necromancer + Toximancer combined play.

**Grave Husk regen on TOXIC_TERRAIN:** Grave Husks (Covenant Chaff) regenerate 1 HP/turn in Poisoned, Corrupted, and Burning terrain. `TOXIC_TERRAIN` created by the Toximancer qualifies as "Poisoned terrain" for Husk regen purposes. **Ruling: Toximancer TOXIC_TERRAIN does qualify as POISONED terrain for Deathless Ranks regen.** This means Grave Husks advancing through the Toximancer's poison ground regenerate 1 HP/turn from the Husk Deathless Ranks trait while enemies take POISONED stacks from the same terrain. Husks are immune to Panic — they advance steadily through the poisoned zone feeding from it.

---

## 7. Combo Chains

### Combo 1 — The Contamination Wave (Toximancer + Hydromancer) [PRIMARY]

This is the highest-efficiency mass-stacking combo in the game. It converts the Hydromancer's water application from a direct electrical chain primer into a POISONED mass-distribution event.

**Step-by-step execution:**

1. **Turn N, Toximancer activates:** Cast Venom Dart on the most clustered enemy unit (2 AP, 2 stacks). Move 2 tiles to maintain range (2 AP). Cast Venom Dart again on a second adjacent enemy (2 AP, 2 stacks on the second target). Result: 2 enemy units at 2 POISONED stacks each, positioned within a cluster.
2. **Turn N or N+1, Hydromancer activates:** Hydromancer casts Aqua Lance or Tidal Surge into the cluster. The WET status is applied to both POISONED units.
3. **Contamination event (automatic):** The POISONED units on WET terrain trigger Contamination. All 8 units adjacent to each POISONED WET unit receive 1 stack. If the two POISONED units are adjacent to each other and share adjacent units, those shared adjacents receive stacks from both Contamination events — 2 stacks total from one Hydromancer cast.
4. **Turn N+2, Toximancer activates:** Cast Toxic Surge on any unit with 3+ stacks. Toxic Surge triggers another Contamination event (target is already POISONED). The 2nd Contamination event from Toxic Surge spreads to 8 more adjacent units. Viralent Injection on the highest-stack target if it has not yet reached DEBILITATED.

**Result:** Over 2 turns with two Mancers, most or all enemy units in the central cluster have 2-4 POISONED stacks. The Toximancer's next activation pushes all of them to DEBILITATED via Pandemic or continued targeted stacking.

---

### Combo 2 — Virulent Garden (Toximancer + Floramancer)

**Setup:** Floramancer places SPORES terrain across an approach corridor. Toximancer casts Venomous Ground on the SPORES zone.
**Result:** SPORES converts to VIRULENT_SPORES (2 stacks per movement through; 3-turn venom duration instead of 2-turn pollen duration).
**Execution:** Enemy units advancing through the VIRULENT_SPORES corridor receive 2 stacks per tile crossed. A 3-tile advance = 6 stacks — capped at 5, so any advance of 3+ tiles through VIRULENT_SPORES immediately applies DEBILITATED. This is the fastest DEBILITATED threshold in the game from terrain alone.

**Floramancer ROOTED synergy:** If Floramancer places VINES beneath the VIRULENT_SPORES and an enemy is ROOTED in the VIRULENT_SPORES zone, they receive 2 stacks per turn while standing still (end-of-turn accumulation for ROOTED units in VIRULENT_SPORES). At 2 stacks per turn: 3 turns to DEBILITATED from 0. A ROOTED unit that cannot escape VIRULENT_SPORES is dead in 3-4 turns without cleanse.

---

### Combo 3 — Frozen Venom (Toximancer + Cryomancer)

**Setup:** Toximancer applies 2-3 POISONED stacks to a target via Venom Dart. Toximancer casts Venomous Ground on a zone adjacent to the target's position.
**Execution:** Cryomancer FREEZES the target. The FROZEN unit's POISONED stacks are preserved (per status-effects.md: POISONED persists through FROZEN; stacks do not decay while frozen). The FROZEN unit cannot move — it remains on the TOXIC_TERRAIN tile (if it was standing on Venomous Ground) or adjacent to it.
**Result:** FROZEN + POISONED = stacks preserved, accumulated during freeze, released all at once when FROZEN ends. When FROZEN expires (1 turn), the unit takes its normal POISONED DoT (which has been accumulating for the FROZEN turn) in a catch-up tick. If the FROZEN unit was on TOXIC_TERRAIN, it also receives a terrain-accumulated stack on the turn FROZEN ends.

**Extended version with Virulent Injection:** Toximancer applies 2 stacks → Cryomancer FREEZES → Toximancer uses Virulent Injection on the FROZEN target (the target cannot fight back or move; melee injection is safe). 3 more virulent stacks injected into a FROZEN target = 5 stacks total = DEBILITATED immediately when FROZEN expires. The FROZEN unit wakes up DEBILITATED and immediately takes 15 HP from the first DEBILITATED tick.

---

### Combo 4 — The Poison Farm (Toximancer + Necromancer)

This combo is fully documented in the Necromancer design document and referenced here for completeness.

**Sequence summary:**
1. Toximancer seeds TOXIC_TERRAIN across an approach path.
2. Enemy Chaff advances through TOXIC_TERRAIN and dies from accumulated POISONED stacks, leaving Standard Corpses on poisoned tiles.
3. Necromancer targets TOXIC_TERRAIN tiles with Necrotic Eruption: Necrotic + Poison = TOXIC_NECROTIC ground; corpses consumed for +15 bonus damage each.
4. Surviving units in the Eruption zone are on TOXIC_NECROTIC terrain — taking 3 Necrotic + 1 POISONED stack per turn passively.
5. Necromancer raises remaining corpses; Toximancer applies more stacks to Shambler-adjacent enemies via Contamination.

**Why it works:** The Toximancer converts approach corridors into killing fields that generate corpses — the Necromancer's primary resource. The TOXIC_NECROTIC hybrid terrain from the combination is the most punishing passive terrain in the game.

---

## 8. Counters and Weaknesses

### What Shuts Down the Toximancer

**Reliable cleanse access:** Hydromancer Mending Current removes all POISONED stacks from one unit per cast (2-turn cooldown). Photomancer Sunburst clears POISONED from an area. Chronomancer Rewind reverts all statuses. An enemy team with a dedicated cleanser can continuously undo the Toximancer's stack economy — the Toximancer builds stacks slower than Mending Current removes them. Against a Hydromancer-supported enemy, the Toximancer must either eliminate the Hydromancer first or apply stacks faster than one Mending Current per 2 turns can address.

**Burst termination before stacks accumulate:** The Toximancer takes 3+ turns to reach full DEBILITATED output on multiple targets. Warbands that can end fights quickly (fast aggression triple-Mancer: Aeromancer mobility + Electromancer burst + Pyromancer area denial) kill the Toximancer before its stack economy becomes threatening. At 85 HP with 1 armor, the Toximancer cannot absorb sustained focus fire.

**FLOODED zone blocking TOXIC_TERRAIN:** A Hydromancer who pre-floods approach corridors prevents TOXIC_TERRAIN from being established there. The Toximancer cannot plant Venomous Ground on FLOODED tiles. If the opponent controls the approach paths with water, the Toximancer's terrain control strategy is disabled in those corridors. The Toximancer must either contest FLOODED zones with Venom Dart single-target stacking (no terrain setup) or find un-flooded paths to seed.

**Photomancer mass cleanse:** Photomancer Sunburst (area cleanse) is the hardest counter to TOXIC_TERRAIN and VIRULENT_SPORES — it removes POISONED from all units in the burst area and can eliminate TOXIC_TERRAIN terrain state simultaneously. A Photomancer who Sunbursts the VIRULENT_SPORES zone that the Floramancer and Toximancer spent multiple activations building destroys the primary terrain investment in one 3-AP cast.

---

## 9. Temperature Interaction Notes

Poison is thermally neutral — venom does not directly heat or cool targets. All Toximancer spells apply **0 temperature change**. However, the Toximancer has significant emergent interactions when temperature effects from other Mancers are present on the board.

### Toxic Fever (OVERHEATED + POISONED)

When a unit is simultaneously OVERHEATED (temperature ≥ +61) AND POISONED, the heat accelerates toxin metabolism — POISONED ticks deal an additional **+2 damage per stack per tick** while OVERHEATED. This bonus stacks with current stack count:

- 1 stack OVERHEATED: (3 + 2) = 5 dmg/tick
- 3 stacks OVERHEATED: (9 + 6) = 15 dmg/tick
- 5 stacks (DEBILITATED) OVERHEATED: (15 + 10) = 25 dmg/tick; the unit is simultaneously DEBILITATED and taking accelerated poison damage

Additionally, a unit that is OVERHEATED also takes the standard 5 dmg/turn BURNING DoT from the OVERHEATED threshold. A unit at 3 POISONED stacks + OVERHEATED therefore takes 15 dmg/tick from accelerated poison + 5 dmg/turn BURNING = **20 dmg/tick total**.

**Primary combo:** Toximancer + Pyromancer or Toximancer + Thermomancer. The Pyromancer overheats the environment (driving enemy temperature to ≥ +61 via ON_FIRE terrain or fire spells), and the Toximancer stacks POISONED. Once the target crosses the OVERHEATED threshold, every poison tick becomes significantly more deadly. This is one of the most punishing sustained damage combinations in the game.

### Preserved Venom (SUPERCOOLED + POISONED)

When a unit is SUPERCOOLED (temperature ≤ -31) AND POISONED, the cold slows their metabolism — POISONED stacks **do not tick down** while SUPERCOOLED. Stack count is frozen at its current value; duration timers pause. When the unit warms above -31 and exits SUPERCOOLED, all remaining stacks resume ticking at normal rate with their original stack durations intact.

**Primary combo:** Toximancer + Cryomancer. The strategic play: stack the target to 3-5 POISONED via Venom Dart and Virulent Injection, then have the Cryomancer drive the target's temperature below -31 (two Frost Bolts or one Glacial Spike). The target's stacks are preserved at full count. When they thaw — whether naturally from temperature decay or by enemy cleanse of the cold status — all remaining stacks resume at full strength. This denies the opponent the ability to manage poison via time; the stacks are waiting for them after any temporary temperature reprieve.

**Preservation window:** If the target is deeply SUPERCOOLED (temperature -50), natural decay (10/turn) takes 2 turns to cross above -31. Two turns of preserved stacks at full count is frequently fight-deciding.

### Contamination + Temperature Interactions

The CONTAMINATION mechanic (POISONED units triggering stack spread via WET terrain or WET spell contact) applies regardless of temperature. However, temperature creates secondary effects on Contamination events:

- **Overheated Contamination chain:** If an OVERHEATED unit triggers Contamination (spreads 1 stack to adjacent units), each adjacent unit that receives a POISONED stack via the spread undergoes a Toxic Fever check. Any adjacent unit already at OVERHEATED (temperature ≥ +61) immediately enters Toxic Fever on the spread stack — even 1 stack on an OVERHEATED unit activates the accelerated tick bonus. In a large OVERHEATED cluster where Hydromancer has applied WET, a single Contamination event can trigger Toxic Fever on multiple units simultaneously, creating a spreading Toxic Fever chain across the cluster.

### Interaction with the Full Temperature Range

| Temperature Threshold | Effect on POISONED Units |
|---|---|
| ≥ +61 OVERHEATED | Toxic Fever: +2 dmg per stack per tick; BURNING DoT also active |
| +31 to +60 HOT | No special poison interaction; unit is SLOWED |
| +1 to +30 WARM | Venom Dart applies +1 bonus stack at +20 or higher |
| 0 NEUTRAL | Standard poison behavior |
| -1 to -30 COLD | No special poison interaction |
| -31 to -60 SUPERCOOLED | Preserved Venom: POISONED stacks do not tick down |
| ≤ -61 FROZEN SOLID | FROZEN status applies; POISONED persists through FROZEN per standard rules (stacks preserved until FROZEN ends) |

---

## 10. Augmentation Spell

### Venom Infusion

**AP Cost:** 3 | **Range:** 3 tiles | **Targeting:** Single allied unit | **Cooldown:** 3 turns

Injects calibrated venom into an allied unit's bloodstream -- lethal to others, tolerated by the host -- turning their attacks into a poison delivery system.

**Effects (2 turns):**
- Ally's attacks apply 1 POISON stack to targets on hit
- Ally is immune to POISONED status for the duration (venom tolerance built during infusion)
- Stack synergy: if the ally attacks a target already carrying POISON stacks applied by the Toximancer's own spells, those stacks are doubled on that hit (synergistic contamination -- the venoms combine chemically)
- After the buff expires, the ally takes 1 unavoidable damage (residual toxin -- always occurs)

**Tactical intent:** Offensive combo-enabler rewarding correct sequencing. The intended play: Toximancer poisons a target from range using its own kit -> sends a Venom Infused ally into melee -> the ally's hit doubles the existing stacks. The stack doubling applies only to the Toximancer's own poison (not Floramancer pollen, not contaminated terrain poison) -- it is a personal chemical brand, not a universal amplifier. The 1-damage exit cost communicates clearly: nothing involving toxins is ever free. The poison immunity prevents the infusion from accidentally poisoning the ally recipient.

**Notable interactions:** On a target that is both POISONED and WET: the element matrix entry Infected Water already exists. Venom Infusion's doubled stacks on that target amplify the base DoT before the Infected Water effect applies -- compounding multiple damage sources simultaneously. Toximancer + Floramancer: Floramancer applies pollen POISON, Toximancer's Venom Infused ally hits the target -- but the doubling does NOT trigger (pollen is Floramancer's poison, not Toximancer's). Teams must communicate whose poison is on the target.

*End of Toximancer design document.*

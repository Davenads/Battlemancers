# Echomancer — Full Design Document

---

## 1. Tactical Identity

The Echomancer is the highest-skill-ceiling Mancer in the roster — a deceptive, recursion-focused specialist that derives nearly all of its power from reading the board correctly and exploiting what has already happened. Its base damage is intentionally low; an Echomancer playing straightforwardly is one of the weakest Mancers available. An Echomancer played by someone who understands its every mechanic is a different creature entirely: one that repeats the best spell cast on the battlefield for half the AP cost, places decoys that bait enemy targeting and punish focus fire, and can retroactively undo two turns of an enemy Mancer's work with TEMPORAL ECHO. Every ability the Echomancer has is a combo with something else. The Echomancer does not have a self-contained win condition — it has an amplification-of-others win condition.

Playing the Echomancer means making every activation count in terms of information: which spell is worth echoing (the highest-cost, highest-impact spell cast by any Mancer last turn), where to place Afterimages to bait enemy targeting, and which enemy Mancer is worth spending the TEMPORAL ECHO ultimate on. The Echomancer is not a "fire and forget" Mancer; it is a "wait, read, respond, amplify" Mancer. Its Afterimage decoys create target confusion in the blind-turn system — an opponent who has spent AP planning to target the Echomancer's position may discover they committed resources to destroying a decoy instead. TEMPORAL ECHO is the most disruptive single-spell in the game when used correctly: forcing a priority enemy Mancer to undo their last 2 turns — including a powerful ultimate they just used — is match-altering.

**Primary win condition:** The Echomancer wins by echoing the highest-value allied spell at a critical moment — repeating a Gravimancer CRUSH on a FROZEN target for 2 AP instead of 5 AP, repeating a Pyromancer Pillar of Flame on the same high-value enemy position for 2 AP, repeating a Chronomancer HASTE on an ally who needs it twice in succession. The Echomancer converts AP-inefficient high-cost spells into AP-efficient follow-ups. Secondary win condition: TEMPORAL ECHO on a priority enemy Mancer who just used their signature ability, rewinding their position and refunding their AP while keeping all damage already dealt.

**Core weakness:** The Echomancer's power scales with what has already happened. In a match where allied Mancers are underperforming — casting low-value spells, hitting empty tiles, wasting AP — the Echomancer has nothing worth echoing. It also has no survivability — at 80 HP with 1 armor, the Echomancer is the lowest-HP Mancer in the roster. An opponent who ignores the Afterimage decoys and focuses the Echomancer directly kills it in two activations. The Echomancer must stay behind its team and use Afterimages aggressively to misdirect targeting. It is also the most complex Mancer to use: the ECHO mechanic requires the player to remember which spell was cast last turn, at what AP cost, on what target — and to know whether the target is still there and whether echoing it is the best play.

---

## 2. Base Stats

| Stat | Value | Notes |
|---|---|---|
| **Max HP** | 80 | Lowest in the roster; survival entirely depends on positioning and Afterimage misdirection |
| **Move Range** | 4 tiles per activation | Above average; the Echomancer must reposition around its own Afterimages and avoid becoming a direct target |
| **Base Armor** | 1 | Minimal; no meaningful damage absorption |
| **Spell Range** | 6 tiles (base) | Long range; the Echomancer should never be in the front line |
| **AP per Activation** | 6 | Standard; movement costs 1 AP per tile |
| **Element** | Echo / Varies | The Echomancer's own spells deal minor Arcane damage; its Echo ability mirrors whatever element it is echoing |

**AP budget example:** With 6 AP, the Echomancer can move 2 tiles (2 AP) and place 2 Afterimages (1 AP each = 2 AP) and trigger an Echo (2 AP), or move 1 tile and use TEMPORAL ECHO (6 AP — entire turn), or move 3 tiles and place 1 Afterimage (1 AP) and fire Echo (2 AP).

---

## 3. Base Spell Kit

The Echomancer's four base spells are designed to cover its recursion-and-deception identity:
- **Arcane Dart** — repeatable single-target filler; the Echomancer's only self-contained damage
- **Afterimage** — places a decoy unit on any tile; the Echomancer's primary misdirection tool
- **Echo** — repeats the last spell cast by any allied or enemy Mancer at 60% power for 2 AP
- **Phase Step** — teleportation short-range; allows the Echomancer to swap positions with an Afterimage it has placed

---

### Spell 1: Arcane Dart

| Field | Value |
|---|---|
| **Name** | Arcane Dart |
| **AP Cost** | 1 AP |
| **Cooldown** | 0 (usable every activation) |
| **Targeting Type** | Single Target (projectile) |
| **Range** | 6 tiles |
| **AoE Radius** | N/A |
| **Base Damage** | 10 |
| **Element** | Arcane |
| **Effects Applied** | Deals 10 Arcane damage. Arcane damage is a pure element that interacts with no terrain states — it applies no secondary status, creates no terrain transformation, and is not amplified or reduced by any terrain interaction. This is by design: Arcane Dart is intentionally the "cleanest" spell in the game — it just deals damage without complication. If the Arcane Dart hits an Afterimage (ally or enemy), the Afterimage takes 10 damage (Afterimages have 10 HP — a single Arcane Dart destroys a damaged Afterimage; two Arcane Darts destroy a fresh Afterimage). |
| **Special Interactions** | Against `RESONATING` tile (Sonimancer-created): Arcane damage is not sonic — no resonance amplification. Against `CHARGED` terrain: no arc chain — Arcane is not Lightning. Against FROZEN units: no SHATTER — Arcane is not Physical/Sonic. Arcane Dart is one of the few spells in the game that is completely resistant to terrain interaction modifications. It always deals exactly 10 HP, always at range 6, always with no secondary effect. |

**Design note:** Arcane Dart is the Echomancer's action when it has nothing better to do — fill remaining AP, chip damage, finish an Afterimage, or apply 10 HP damage pressure without investing in a higher-tier spell. Its intentional "pure damage, no interaction" design makes it unlike any other 1 AP spell in the roster. The Echomancer's value does not come from Arcane Dart; the Dart is just the filler between everything that matters.

**Spell answers YES to (design rule check):**
1. Applies damage reliably in all conditions — YES (pure, unmodifiable)
2. Destroys Afterimage constructs (ally or enemy) — YES
3. Skill expression: fill remaining AP precisely; choose targets where no interaction would waste potential — YES

---

### Spell 2: Afterimage

| Field | Value |
|---|---|
| **Name** | Afterimage |
| **AP Cost** | 1 AP |
| **Cooldown** | 0 (usable every activation); maximum 3 Afterimages active simultaneously |
| **Targeting Type** | Terrain Placement — targets any visible tile within range |
| **Range** | 5 tiles (to target tile) |
| **AoE Radius** | N/A |
| **Base Damage** | 0 at placement; 12 AoE on destruction (see below) |
| **Element** | Arcane |
| **Effects Applied** | Places an Afterimage token on the target tile. The Afterimage has the following properties: **HP:** 10 HP (destroyed by any 10+ damage hit, or two smaller hits). **Appearance:** Identical to the Echomancer visually — the opponent cannot distinguish the Echomancer from an Afterimage through normal play (both display the same sprite; the distinction requires the opponent to examine targeting data or use abilities that reveal hidden units). **Targeting:** Enemy units and spells that use AI targeting or are aimed at "the Echomancer" may target the Afterimage instead if the Echomancer is behind the Afterimage in the projected targeting angle. In the blind-turn system, if the opponent declared an action targeting the Echomancer's last known position and an Afterimage is now on that tile, the spell hits the Afterimage instead. **Destruction:** When destroyed (by any source of damage), the Afterimage explodes for 12 Arcane AoE damage in a 1-tile radius. If the Echomancer itself destroys an Afterimage (via Arcane Dart or Phase Step — see below), the explosion is amplified to 18 HP (Echomancer owns the resonance). |
| **Special Interactions** | Maximum 3 Afterimages simultaneously; placing a 4th collapses the oldest Afterimage (no explosion on natural collapse — only destruction-triggered explosions fire). If an Afterimage is on a GRAVITY_WELL center tile: it is pulled there like any other unit — the well's pull affects Afterimage tokens (this can be used deliberately to position decoys precisely at the well center). If an Afterimage is on OVERGROWTH: Floramancer ROOTED status cannot ROOT an Afterimage (it has no movement to root). |

**Design note:** Afterimage is the Echomancer's most psychologically impactful ability. At 1 AP with no cooldown and a maximum of 3 active, the Echomancer can place 3 decoys per activation (if no movement or other spells are cast). An opponent facing 4 potential Echomancer targets (3 Afterimages + the real Echomancer) must either spend AP identifying the real one or risk wasting targeting resources on a decoy. In the blind-turn system, where commitments are made before resolution, this misdirection is particularly potent: the opponent aims their highest-value spell at what they believe is the Echomancer's tile, and it hits a 10 HP construct instead. The 12 HP explosion on destruction means even a correctly-identified and destroyed Afterimage costs the opponent an AoE hit — not nothing.

**Spell answers YES to (design rule check):**
1. Creates a terrain feature (Afterimage construct) — YES
2. Applies misdirection to opponent targeting — YES
3. Deals damage on destruction (AoE explosion) — YES
4. Skill expression: Afterimage placement to create maximum targeting ambiguity; knowing when to let the opponent destroy one vs. when to Phase Step to it — YES

---

### Spell 3: Echo

| Field | Value |
|---|---|
| **Name** | Echo |
| **AP Cost** | 2 AP |
| **Cooldown** | 1 turn (must wait 1 turn before echoing the same spell again; but a new Echo (new last spell cast) resets the available echo target) |
| **Targeting Type** | No targeting cursor — automatically targets the same location and target as the last spell cast by any Mancer on either team (see Echo Selection below) |
| **Range** | Inherits the original spell's range — Echo fires from the Echomancer's current position using the original target location |
| **AoE Radius** | Inherits the original spell's AoE radius |
| **Base Damage** | 60% of the original spell's base damage (rounded down) |
| **Element** | Inherits the original spell's element; terrain and unit status interactions apply using the inherited element |
| **Effects Applied** | Repeats the last qualifying spell cast at 60% damage, same target location, same element, same secondary effects (each secondary effect applies at a 60% probability per target: a status effect with 100% base apply chance applies at 60% when echoed; status effects with lower base apply chance are reduced proportionally). |
| **Special Interactions** | **Echo Selection rules:** The active Echo target is updated every time any Mancer (ally or enemy) casts a spell. The Echomancer's player can see the "current Echo target" in the UI — it shows what the last spell was, by whom, at what power, and whether it is worth echoing. The Echomancer does not have to echo — it can let the Echo target update (next Mancer cast) and then use Echo on the newer target. Only 1 Echo target is tracked simultaneously. **Excluded spells** (cannot be echoed): Phase Step, Afterimage, TEMPORAL ECHO, Chronomancer REWIND (time-reversal spells cannot be echoed — echoing a time reversal would cause a paradox), and any spell the Echomancer already used this activation. **High-value Echo targets (documented):** Gravimancer Crush (5 AP → echoed at 2 AP for 27 HP base force; can trigger ROOTED); Pyromancer Pillar of Flame (5 AP → echoed at 2 AP for 33 HP + BURNING + ON_FIRE at original target tile); Cryomancer Blizzard Field (5 AP → echoed at 2 AP for 6 HP AoE Ice + CHILLED at original target area); Chronomancer HASTE (AP varies → echoed HASTE gives target ally +3 AP bonus instead of +6, at 2 AP cost — half the HASTE, double the value per AP). |

**Design note:** Echo is the most AP-efficient ability in the game when used correctly. A Gravimancer spending 5 AP on Crush deals 45 HP. The Echomancer then spends 2 AP to repeat that Crush on the same tile for 27 HP. Total: 72 HP at a combined cost of 7 AP. That is better AP efficiency than any single direct-damage spell in the roster. The skill ceiling is identifying which spell — out of everything cast on the battlefield in the last turn — is worth echoing. The 1-turn cooldown on echoing the same spell prevents infinite recursion (can't echo the echo of the echo). The Echo target updating with each new cast creates a decision point: do I echo now, or wait to see if the next Mancer casts something better?

**Spell answers YES to (design rule check):**
1. Applies terrain state (inherits original spell's terrain interaction) — YES
2. Applies unit status (inherits original spell's status effects at 60% probability) — YES
3. Synergizes with every Mancer in the roster (any spell is a potential Echo target) — YES
4. Skill expression: Echo target selection; timing (echo now or wait for a better spell to be cast) — YES

---

### Spell 4: Phase Step

| Field | Value |
|---|---|
| **Name** | Phase Step |
| **AP Cost** | 2 AP |
| **Cooldown** | 2 turns |
| **Targeting Type** | Self / Reactive — the Echomancer teleports to an Afterimage's tile, and the Afterimage moves to the Echomancer's prior tile (swap) |
| **Range** | N/A (targets an Afterimage the Echomancer can see; no range limit — can teleport to any active Afterimage regardless of distance) |
| **AoE Radius** | N/A |
| **Base Damage** | 0 (teleportation only) |
| **Element** | Arcane |
| **Effects Applied** | The Echomancer instantly swaps positions with a chosen Afterimage. The Echomancer teleports to the Afterimage's tile; the Afterimage token moves to the Echomancer's prior tile. No AoE, no damage, no terrain state changes. The Afterimage at the Echomancer's new position is not destroyed by this swap — it remains active at the Echomancer's former position. The swap is instant — no path travel, no collision checks, no fall-damage risk from the teleport itself. However, if the destination tile has a terrain hazard (ON_FIRE, TOXIC_TERRAIN, etc.), the Echomancer takes normal terrain DoT for standing on that tile after the swap. |
| **Special Interactions** | If the Afterimage at the destination tile was about to take damage this turn (opponent declared a spell targeting that tile in the blind-turn phase): the Phase Step puts the Echomancer in that tile and the Afterimage at the old position. The opponent's declared spell now hits the Echomancer, not the Afterimage. This is Phase Step's critical weakness — the Echomancer player must be careful not to Phase Step into an incoming kill. When used correctly (Phase Step to an Afterimage that is not under incoming fire), it is a free teleport that repositions the Echomancer anywhere on the map where an Afterimage is placed. **Afterimage destruction on Phase Step target:** if the Afterimage at the destination tile is at reduced HP (1–9 HP), the Phase Step does not affect its HP — the Echomancer simply occupies that tile, and the Afterimage token is at the old position. The Echomancer is now on a tile that previously held a damaged Afterimage; nothing about the HP state carries over. |

**Design note:** Phase Step is the Echomancer's escape and repositioning tool. Combined with Afterimage placement, it gives the Echomancer the ability to teleport across the entire map in a single action — place an Afterimage at a desired position 5 tiles away (1 AP), then Phase Step to it (2 AP) = 3 AP total to teleport 5 tiles, far more efficient than walking (5 AP for 5 tiles of movement). More importantly, Phase Step lets the Echomancer escape from melee threats that have closed on its position by teleporting to an Afterimage on the far side of the board. The 2-turn cooldown prevents infinite escape loops.

**Spell answers YES to (design rule check):**
1. Moves a unit (the Echomancer itself) — YES
2. Synergizes with Afterimage (Phase Step requires Afterimage placement to function) — YES
3. Skill expression: Afterimage pre-positioning for escape routes; Phase Step timing to avoid incoming fire on target tiles — YES

---

## 4. Terrain Interaction Table

### Arcane / Echo Spell Impact on Existing Terrain States

The Echomancer's own spells (Arcane Dart, Afterimage, Phase Step) interact minimally with terrain — Arcane is a pure element. When the Echomancer triggers Echo, the echoed spell's element determines the terrain interactions (those are documented in each original Mancer's terrain table). The following covers Arcane-element interactions and Afterimage-specific terrain effects.

| Existing Terrain State | What Happens When Arcane Spell Hits | Tile Becomes | Unit on Tile | Secondary Effect |
|---|---|---|---|---|
| **Normal (GROUND)** | Arcane energy leaves no residue | `GROUND` (unchanged) | Takes spell damage | No terrain state change; Arcane is the one pure element |
| **ON_FIRE** | Arcane energy passes through fire | `ON_FIRE` (unchanged) | Takes spell damage + `BURNING` from fire contact | Arcane does not interact with fire; any unit on the tile still burns |
| **WET** | Arcane energy passes through water | `WET` (unchanged) | Takes spell damage | No arc chain — Arcane is not Lightning; no freeze — not Ice |
| **CHARGED** | Arcane energy absorbs the charge | `GROUND` (CHARGED state cleared; arcane energy dissipated the charge harmlessly) | Takes spell damage; no arc chain | Arcane Dart safely discharges CHARGED terrain — useful for clearing Electromancer setups without triggering arcs |
| **ICE_TILE** | Arcane energy passes through ice | `ICE_TILE` (unchanged) | Takes spell damage; no SHATTER (Arcane is not Physical/Sonic) | No SHATTER, no ice shard damage; pure damage only |
| **TOXIC_TERRAIN** | Arcane energy passes through toxins | `TOXIC_TERRAIN` (unchanged) | Takes spell damage + `POISONED` (1 stack; unit is still on toxic ground) | No interaction; poison applies normally |
| **RESONATING** | Arcane is not sonic — no amplification | `RESONATING` (unchanged) | Takes spell damage at base value (no 2× amplification) | Arcane Dart specifically cannot exploit RESONATING tiles |
| **OBSIDIAN** | Arcane energy passes through dense matter | `OBSIDIAN` (unchanged) | Takes spell damage through OBSIDIAN (Arcane has no physical component — it ignores physical matter for damage delivery) | Arcane Dart pierces OBSIDIAN — unique property; only Arcane and Sonic elements pass through physical barriers |
| **STEAM_CLOUD** | Arcane passes through steam | `STEAM_CLOUD` (unchanged) | Takes spell damage; BLINDED not removed | Arcane fires through steam without scatter (unlike Crystal Shard) |
| **GRAVITY_WELL** | Afterimage placed in Gravity Well radius | Afterimage pulled toward well center (1 tile per turn like any unit) | Afterimage takes positioning drag | Afterimage can be deliberately deployed at the well center by using the gravitational pull — place it adjacent, let it be pulled to center |
| **PERMAFROST** | Arcane passes through ice | `PERMAFROST` (unchanged) | Takes spell damage + `CHILLED` (still on permafrost ground) | No terrain state change |
| **MUD** | Arcane passes through mud | `MUD` (unchanged) | Takes spell damage; no additional effect | No terrain interaction |
| **VINES / OVERGROWTH** | Arcane passes through organic matter | `OVERGROWTH` (unchanged; no destruction) | Takes spell damage | Arcane Dart passes through OVERGROWTH without destroying it — unlike crystal and physical spells |

### Echo: Inherited Terrain Interactions

When the Echomancer triggers Echo on a prior spell:
- The echoed spell's element is active for all terrain interaction purposes
- The terrain interactions from the original Mancer's interaction table apply at the echoed spell's reduced damage (60%)
- Example: Echoing a Pyromancer Ember Shot on an ICE_TILE → the echoed Fire spell hits ICE_TILE → ICE_TILE becomes FLOODED + steam burst (identical interaction from Pyromancer terrain table, at 60% base damage)

### Afterimage: Terrain Hazard Summary

| Terrain State | Afterimage Behavior |
|---|---|
| `ON_FIRE` | Afterimage takes 5 HP/turn from fire DoT (fire damages constructs; Afterimage is a construct) |
| `TOXIC_TERRAIN` | Afterimage is immune to terrain poison (it has no biological HP; POISONED cannot be applied to a construct) |
| `GRAVITY_WELL` | Afterimage is pulled toward center (1 tile per turn) — can be used to position decoys into wells deliberately |
| `CHARGED` | Afterimage does not conduct electricity — CHARGED tile does not arc to the Afterimage (constructs are not conductive) |
| `ICE_TILE` | Afterimage is not affected by slip checks (it does not move voluntarily) |
| `VINES / ROOTED` | ROOTED cannot apply to Afterimage (no movement to root) |
| `STEAM_CLOUD` | Afterimage blends with steam visual obscuration — even harder to distinguish from the real Echomancer |

### Terrain States Beneficial to the Echomancer

| State | Benefit |
|---|---|
| `STEAM_CLOUD` | Afterimages hidden in STEAM_CLOUD are nearly indistinguishable from the real Echomancer; BLINDED opponents reduced to 1 tile targeting range cannot identify the real Echomancer from the Afterimages at all |
| `OBSIDIAN` (adjacent tiles) | Arcane Dart pierces OBSIDIAN; the Echomancer can fire through Geomancer walls that block every other Mancer (alongside Sonic spells) |
| Ally spells (any high-AP cost) | Any high-AP spell cast by an ally is a potential Echo target — the Echomancer's "beneficial terrain" is the spell economy of its own team |

### Terrain States Hazardous to the Echomancer

| State | Hazard |
|---|---|
| `ON_FIRE` | At 80 HP and 1 armor, the Echomancer taking 5 HP/turn fire DoT is a severe threat — 16 turns to death on fire terrain |
| `GRAVITY_WELL` | The Echomancer is pulled toward the well center like any other unit; at 4-tile move range, escaping a Gravity Well costs 1 AP per pull counter-step |
| `ICE_TILE` | Slip checks apply to the Echomancer; at 80 HP, a slip into ON_FIRE or off a ledge is potentially lethal |
| `CHARGED` | No defensive advantage; Echomancer takes full arc chain damage from CHARGED terrain |

---

## 5. Upgrade Options

All costs are additions to the base 100-point Mancer cost.

---

### Spell Variants

Spell variants replace a base spell entirely with a more powerful or specialized version.

#### Variant A: Phantom Barrage (replaces Arcane Dart) — +15 pts

**Description:** Arcane Dart is replaced by Phantom Barrage — three Arcane Darts fired in rapid succession at the same target. Each deals 8 damage (24 HP total) and independently checks if the target has any Afterimage positioned between the Echomancer and the target — if so, each dart independently may strike the Afterimage (depends on Afterimage position in the firing line). AP cost is 2 AP; cooldown is 1 turn.

**Trade-off:** Higher damage per cast but a cooldown prevents the no-cooldown Arcane Dart spam. Best for Echomancers that want a meaningful direct damage option between Echo casts, rather than pure filler.

#### Variant B: Deep Echo (replaces Echo) — +25 pts

**Description:** Echo is replaced by Deep Echo — an enhanced version that fires at 80% of the original spell's base damage (up from 60%) and applies status effects at 80% probability (up from 60%). Additionally, Deep Echo can store the echo for an extra turn: the Echomancer can declare "storing this Echo" and use it on turn N+2 instead of N+1 (it becomes a 2-turn saved echo instead of a 1-turn window). The stored Echo is still at 80% damage. Only 1 echo can be stored at a time; if a new spell is cast before the stored echo is released, the stored echo is lost (replaced by the new spell's echo). AP cost remains 2 AP; cooldown remains 1 turn.

**Trade-off:** Higher damage (80%) and extended echo window (2 turns to use) allow for more deliberate combo setup rather than requiring immediate echo use. Best for Echomancers that need to coordinate the echo with an ally's action on a specific subsequent turn.

---

### Passive Traits

#### Passive A: Resonant Afterimages — +20 pts

**Description:** When an Afterimage is destroyed (by enemy damage), it detonates for 18 HP AoE in 1-tile radius instead of 12 HP (upgraded explosion). Additionally, the Echomancer stores a "ghost echo" of the destroyed Afterimage's position — on the following activation, the Echomancer can spend 1 AP (instead of 2) to Echo the spell that destroyed the Afterimage (the echo fires from the Afterimage's former position, not the Echomancer's current position). This "revenge echo" fires from the Afterimage's last known location, effectively creating a delayed second hit at the exact position that destroyed the decoy.

**Trade-off:** Stronger Afterimage destruction payoff and a unique echo-from-distance mechanic that allows the Echomancer to fire from positions it is not standing on. Best for Echomancers using Afterimages aggressively as bait rather than purely as decoys.

#### Passive B: Temporal Sense — +20 pts

**Description:** The Echomancer can see 1 turn into the opponent's planned actions — it receives a UI hint showing the general category of the opponent's highest-cost queued action for the following turn (example hints: "heavy spell planned," "movement + quick spell," "signature ability incoming"). This does not reveal the exact spell or target. Additionally, TEMPORAL ECHO (the signature ability) is usable once per 4 turns instead of once per 5 turns (cooldown reduced by 1 turn).

**Trade-off:** Significant information advantage in the blind-turn system — knowing "a heavy spell is coming" allows the Echomancer to Phase Step out of position or prioritize a counterplay Echo. The TEMPORAL ECHO cooldown reduction compounds with the ability's power (more frequent use of the most disruptive spell in the game). Best in high-level play where reading opponent intent is the primary skill expression.

#### Passive C: Mirror Self — +25 pts

**Description:** The Echomancer can designate one active Afterimage as its "Mirror" (free action; 0 AP; no cooldown). The Mirror Afterimage shares the Echomancer's element resistance — it cannot be damaged by the element that last hit the Echomancer (if the Echomancer was just hit by fire, the Mirror is fire-resistant: fire spells deal only 3 HP to the Mirror instead of normal damage). The Mirror resistance changes each time the Echomancer takes a hit from a new element. Additionally, the Mirror detonates at 25 HP AoE (instead of 12 HP) when destroyed.

**Trade-off:** One Afterimage becomes a substantially more durable and punishing decoy, specifically resistant to the element the opponent is currently using. Forces opponent to switch elements to destroy the Mirror. Best against single-element Mancer opponents who cannot vary their damage type.

---

### Stat Enhancements

#### Stat A: Echo Resilience (+15 HP) — +10 pts

**Description:** Max HP increases from 80 to 95. Critical upgrade — at 80 HP, the Echomancer is eliminated by almost any two-spell combination targeting it. At 95 HP, it survives most single-activation burst sequences and can absorb the first hit before Phase Stepping to safety.

**Design note:** 80 HP is the Echomancer's primary vulnerability. Against a Cryomancer + Geomancer FREEZE-SHATTER (62 HP from Rock Throw SHATTER), the Echomancer at 80 HP is eliminated in one set of actions. At 95 HP, it survives the SHATTER and can Phase Step on its next activation. The difference is life or death.

#### Stat B: Extended Resonance (+1 Afterimage Limit) — +15 pts

**Description:** The Echomancer's maximum simultaneous Afterimages increases from 3 to 4. A fourth Afterimage active simultaneously doubles the opponent's identification challenge. Additionally, Phase Step can now target any Afterimage (including the 4th) without the oldest collapsing first — the Echomancer manages 4 active positions simultaneously.

**Design note:** 4 Afterimages on the field means 5 total Echomancer-like tokens to target (4 fakes + 1 real). In the blind-turn system, where the opponent commits to targeting before knowing the Echomancer's true position, a 5-target identification challenge with 4/5 odds of hitting a fake is functionally close to untargetable by non-AoE spells.

---

### Signature Ability

The Signature Ability is unlocked as an additional spell slot — it does not replace any base spell.

#### Signature: Temporal Echo — +40 pts

| Field | Value |
|---|---|
| **Name** | Temporal Echo |
| **AP Cost** | 6 AP (entire activation; Echomancer cannot move this turn) |
| **Cooldown** | 5 turns |
| **Targeting Type** | Single Target — targets one enemy Mancer within 6 tiles |
| **Range** | 6 tiles |
| **AoE Radius** | N/A (single target) |
| **Base Damage** | 0 (no direct damage — this is a temporal manipulation ability) |
| **Element** | Arcane / Temporal |
| **Effects Applied** | The Echomancer stores the last 2 turns of the target enemy Mancer's movement and action history, then replays it in reverse. The target is forced to undo their last 2 actions and movement steps: they are moved back to their position 2 turns ago, and the AP they spent on the last 2 turns' spells is refunded (but the spells' effects on the board remain — damage already dealt, terrain states already applied, do NOT revert; only the Mancer's position and AP pool is reset). The target begins their next activation from their position 2 turns ago, with full 6 AP. Any statuses the target accumulated in the last 2 turns are removed (BURNING, CHILLED, POISONED stacks applied in those 2 turns are stripped — their physical history is rewound). Statuses applied to the target before the 2-turn window are maintained. |
| **Special Interactions** | This is not a Chronomancer REWIND — terrain states, terrain features, and damage to other units from those 2 turns are NOT undone. If the target Mancer used their Signature Ability in the last 2 turns (e.g., Pyromancer World Conflagration), that ability's effects on the board remain — but the Mancer is repositioned away from where it was, and the signature ability's cooldown is restored (the Mancer gets its cooldown reset as if it had not used it — the AP was refunded but the spell effect happened). Against a target already under `STASIS` (Chronomancer): TEMPORAL ECHO cannot target a STASIS unit (time manipulation cannot affect a unit in temporal suspension). Against a target with `HASTE` active: the HASTE granted AP is refunded along with the rewound turns' AP (no double-counting — the refunded AP simply resets to 6 AP base for the next activation). |

**Design note:** Temporal Echo is the highest-disruption single spell in the game. Its primary use is against a priority enemy Mancer who just used their signature ability on the prior turn and moved into an advantageous position: after Temporal Echo, that Mancer is back where they were 2 turns ago, their position reset, and their signature ability cooldown restored. The board keeps all the damage that was dealt, all the terrain states that were applied — but the Mancer lost those 2 turns of movement, and must re-execute their plan from a position that the Echomancer's team can now counteract (since those 2 turns of positioning are erased).

The 5-turn cooldown, full-activation cost, and no-damage nature of TEMPORAL ECHO make it a strategic nuclear option, not a casual cast. It requires the Echomancer to survive long enough for it to become relevant and to be in 6-tile range of a priority target. The combination of Echomancer Afterimage misdirection (keeping it alive) + TEMPORAL ECHO (undoing the opponent's best play) is the highest-ceiling sequence the Echomancer can execute.

**Synergy note:** Temporal Echo on an enemy Mancer who just repositioned onto a Geomancer-elevated platform (gaining +1 spell range) sends that Mancer back to ground level — losing the elevation advantage and resetting the work it spent AP to achieve. Temporal Echo on a Pyromancer that just used World Conflagration refunds World Conflagration's cooldown (the Pyromancer can use it again) and resets the Pyromancer's position — the board keeps the fire, but the Pyromancer is no longer in position to capitalize on it.

---

## 6. Faction Synergy

### Best Faction: The Verdant Pact

The Verdant Pact's Glade Archers apply POISONED on hit — and POISONED stacks are maintained on a target through FROZEN (per the status interaction table). An Echomancer echoing a Cryomancer Ice Lance on a POISONED target (POISONED from Glade Archer fire) preserves the POISONED stacks through the echoed freeze — when the freeze expires, the POISONED resumes ticking. The Echomancer + Cryomancer + Glade Archer combination creates multi-stacked POISONED targets that are also periodically frozen, with the Echomancer providing echo freezes at 2 AP cost whenever the Cryomancer applies a real freeze.

Thornback Sentinels holding static positions (they do not move often due to Terrain Bond stationary regen) create predictable terrain around which the Echomancer can place Afterimages that are visually among the Sentinels — the opponent must identify the Echomancer among both the Sentinels and the Afterimages.

### The Gilded Throne — Echo Artillery

The Gilded Throne's Siege Arbalest fires every turn — its armor-piercing bolts are strong physical damage. The Echomancer can Echo a Siege Arbalest bolt (if it qualifies as a traceable spell in the echo system — infrastructure decision: ranged unit attacks are categorized as spells in the echo system). If the Echomancer echoes the Siege Arbalest bolt at 60% power (60% of ~28 HP = 16 HP), that is a 16 HP armor-piercing physical hit from the Echomancer for 2 AP — a ranged physical attack that the Echomancer does not normally have access to. More relevantly, the Echomancer echoing a Geomancer Rock Throw SHATTER at 60% power (60% × 62 = 37 HP on a FROZEN target) is a meaningful kill confirmation at 2 AP.

Iron Discipline (Charm and Panic immunity) protects the Gilded Throne force from Psychomancer disruption — relevant because the Echomancer uses TEMPORAL ECHO from close range, and an opponent Psychomancer responding with CONFUSED on Throne infantry would waste allied activation windows.

### The Ashen Covenant — Death Economics

Grave Husks regenerate in BURNING terrain. The Echomancer echoing a Pyromancer Scorched Earth spell (ON_FIRE terrain creation) creates a second fire zone at the same AP cost as the Pyromancer's investment — doubled fire coverage for the Husk advance, doubled Necromancer fuel opportunities from fire kills. Wailing Shades phase through cover and the Echomancer's Arcane Dart also ignores OBSIDIAN — the Ashen Covenant's existing "ignore physical barriers" theme is reinforced by the Echomancer's Arcane pierce mechanic.

The most impactful Ashen Covenant interaction: Wailing Shade kills that generate Necromancer fuel happen from the Shade's natural through-wall shots. The Echomancer echoing those Shade shots (if categorized as echo-eligible spells) from a different angle allows the same kill-zone to be hit from multiple directions simultaneously — Shade fires from east, Echo fires from north.

---

## 7. Combo Chains

### Combo 1 — Echo of HASTE (Echomancer + Chronomancer) [SIGNATURE]

**Mancers involved:** Echomancer + Chronomancer

**Step-by-step execution:**

1. **Chronomancer activates:** HASTE cast on an allied priority Mancer (e.g., Pyromancer or Geomancer). That Mancer gains +6 AP this turn (double action). Chronomancer has spent full activation on HASTE.
2. **Echomancer activates (same turn — Mancer initiative):** Echo (2 AP) — repeats HASTE at 60% power. The target ally gains +3 AP this turn (in addition to the +6 from the original HASTE). Total: 12 AP for the target Mancer this turn. With 12 AP: move 3 tiles + Pillar of Flame (5 AP) + Scorched Earth (3 AP) + Ember Shot (1 AP) = 12 AP (with 0 movement): the Pyromancer casts four spells in one activation.
3. **Result:** An ally Mancer with 12 AP can complete a full activation sequence that would normally require 2 activations in one turn.

**Damage math:** Pyromancer with 12 AP: Pillar of Flame (55 HP) + Scorched Earth (12 HP AoE) + Conflagration Wave (20 HP cone) + Ember Shot (18 HP) = 105 HP of damage plus massive terrain transformation in one activation. Against a priority target in the range of multiple spells, this is match-ending.

---

### Combo 2 — Afterimage Bait and Shatter (Echomancer + Cryomancer)

**Mancers involved:** Echomancer + Cryomancer

**Step-by-step execution:**

1. **Echomancer activates:** Places 3 Afterimages at positions that make identifying the real Echomancer difficult. Positions the real Echomancer behind the safest screen.
2. **Opponent activates:** Spends a heavy spell (e.g., Gravimancer Crush, 5 AP) targeting what appears to be the Echomancer. If targeting an Afterimage: 5 AP wasted on a 10 HP construct that explodes for 12 HP on the attacker.
3. **Cryomancer activates:** The opponent's Crush registered in the Echo system. Echomancer ECHOES the Crush (2 AP) onto the Gravimancer who just used it (or onto a different high-value target), firing at 27 HP base force damage.

**Tactical value:** The opponent wasted 5 AP destroying a decoy; the Echomancer used 2 AP to fire a 27 HP echo of that wasted spell back at the attacker. Net AP exchange: opponent spent 5 AP, Echomancer received 27 HP of damage-free echo. The decoy destruction payoff (12 HP Afterimage explosion) adds a further 12 HP to the exchange.

---

### Combo 3 — Temporal Echo Reversal (Echomancer + any Mancer)

**Mancers involved:** Echomancer solo (uses TEMPORAL ECHO)

**Scenario:** Priority enemy Mancer used their Signature Ability on Turn N (e.g., Pyromancer World Conflagration, spreading fire across 5 tiles and dealing mass damage).

**Step-by-step execution:**

1. **Turn N:** Enemy Pyromancer uses World Conflagration (6 AP). Fire spreads massively; significant damage dealt to allied units. Pyromancer is now deep in the board, positioned to capitalize on the fire zones.
2. **Turn N+1, Echomancer activates:** TEMPORAL ECHO (6 AP) targets the Pyromancer. The Pyromancer is rewound 2 turns: it is now back at its Turn N-1 position (before it moved into the activation zone for World Conflagration). The fire zones remain on the board. World Conflagration's cooldown is reset (the Pyromancer can use it again next time it is in position). But: the Pyromancer is no longer in the position it spent AP reaching on Turn N.

**Result:** The allied team knows the fire zones are present but the Pyromancer that created them is 4+ tiles further back than expected. The Cryomancer can now convert those fire zones to STEAM_CLOUD before the Pyromancer can fan them again, and the rewound Pyromancer must spend another 2+ activations reaching its former position.

---

### Combo 4 — Echo Double-Damage (Echomancer + any heavy-AP Mancer)

**Mancers involved:** Echomancer + Geomancer / Gravimancer / Cryomancer (any Mancer with a 4–5 AP spell)

**General pattern:**

1. **Heavy Mancer activates:** Casts 4–5 AP spell (Crush, Pillar of Flame, Blizzard Field, Rock Throw SHATTER). Deals its full damage.
2. **Echomancer activates (same or next turn):** Echo (2 AP) — repeat the same spell at 60% damage on the same target or same area.

**AP efficiency math:** Pillar of Flame (55 HP) + Echo of Pillar of Flame (33 HP) = 88 HP across two turns for 5 AP + 2 AP = 7 AP combined investment. Standard 7 AP equivalent direct damage across two Mancers is approximately 35–50 HP (two Standard spells at 3 AP each). The Echo combo yields 88 HP vs. the 35–50 HP baseline from equivalent AP investment — 1.75× the damage efficiency of two independent spell casts at equivalent AP cost.

---

## 8. Counters and Weaknesses

### Mancers That Hard-Counter Echomancer

| Mancer | Counter Mechanism |
|---|---|
| **Photomancer** | Photomancer's Illuminate and Reveal abilities identify stealth units and distinguish targets by visibility markers. A Photomancer applying ILLUMINATED to all visible units reveals which one is the real Echomancer (the illuminated marker identifies the target); Afterimages are not stealth units — they are constructs, not hidden units — but a Photomancer's detection abilities can be ruled to pierce the visual disguise of Afterimages and mark the real Echomancer distinctly. Additionally, BLINDED (Photomancer Sunburst) on the Echomancer removes its 6-tile targeting range to 1 tile, making Echo targeting from distance impossible. |
| **Gravimancer** | Gravitational Collapse pulls all units (including Afterimages) to a center point — Afterimage tokens at the center take collision damage and are destroyed by the mass impact. A Gravitational Collapse into a dense Afterimage cluster destroys all active decoys simultaneously (collision at center = 10 HP per unit, which kills 10 HP Afterimages outright). Without its decoy network, the Echomancer is a vulnerable 80 HP unit in the open. |
| **Chronomancer** | REWIND applied to the Echomancer reverses all statuses it accumulated (including ECHO stored targets — a REWIND clears the Echomancer's tracked Echo target). TEMPORAL ECHO (the Echomancer's signature) versus Chronomancer's REWIND is a direct counter-contest: the Echomancer tries to undo the Chronomancer's actions; the Chronomancer can REWIND the Echomancer's own actions including the TEMPORAL ECHO activation (though TEMPORAL ECHO's effects on the target are not undone by REWIND — only the Echomancer's position and AP). The two Mancers are natural rivals in direct conflict. |

---

## 9. Temperature Effects

### Temperature Effects per Spell

| Spell | Temperature Change | Notes |
|---|---|---|
| **Arcane Dart** (1 AP) | **0** | Generic arcane element — no thermal component |
| **Afterimage** (1 AP) | **0 at placement** | Decoy placement has no thermal effect; destruction explosion carries minor thermal burst based on Echomancer's current temperature (see Afterimage temperature below) |
| **Echo** (2 AP) | **60% of the original spell's temperature change** | If echoed spell was +35 temperature, Echo applies +21; if -25, Echo applies -15; if 0, Echo applies 0 |
| **Phase Step** (2 AP) | **0** | Teleportation — no thermal effect |

---

### Temperature Interaction Notes

**Echo temperature rules — full specification:**
The Echo mechanic mirrors temperature at the same 60% ratio it applies to damage and secondary effects. The full rules:
- Echo always applies **60% of the original spell's temperature delta**, rounded down
- If the original spell was **+35 temperature**, Echo applies **+21**
- If the original spell was **-25 temperature**, Echo applies **-15**
- If the original spell was **0 temperature**, Echo applies **0** (no change)
- Echo applies to the **same target position** as the original cast — meaning if the original target's tile still has enemy units when the Echo resolves, they receive the echo temperature change
- Example: a Cryomancer ally casts Glacial Spike (-30 temperature) at position X. Echomancer uses Echo → **-18 temperature** at position X. If the same enemy is still there: -30 + -18 = **-48 total**, reaching SUPERCOOLED (-31 to -60), applying SLOWED and BRITTLE (+50% physical damage taken)

**THERMAL SHOCK setup with Echo:**
Echo can contribute to or follow through on THERMAL SHOCK sequences. The key scenario: a Thermomancer ally uses Heat Lance (+35 temperature) on an enemy at -40 temperature (SUPERCOOLED). That is a +75 delta — the target crosses from ≤-31 to ≥+31 in one hit, triggering **THERMAL SHOCK** (bonus damage = |ΔTemp|/2 = 37 bonus + 1-turn STUN). If the target survives THERMAL SHOCK, they are now at +35 temperature (HOT). The Echomancer then echoes Heat Lance: **+21 temperature** to the same target, now at +35. The target moves from +35 to **+56** (HOT). The full sequence: **THERMAL SHOCK → survival → Echo → HOT debuff** — the echo compounds the Thermomancer's work, pushing the weakened target deeper into the HOT zone.

**Echo loop with sustained temperature buildup:**
An Echomancer repeatedly echoing a fire spell on the same target over consecutive turns creates escalating temperature pressure. Because Echo costs only 2 AP while the original spell cost 3–5 AP, the Echomancer can sustain echo pressure across turns with high AP efficiency. Example: a Pyromancer's Ember Shot applies +25 temperature. Each Echo of Ember Shot applies +15 temperature (+25 × 0.60 = 15). Combined over 3 turns (original cast + 2 echoes): **+25 + +15 + +15 = +55 temperature**, pushing a NEUTRAL target into HOT (+31 to +60) and applying SLOWED. This is slow but AP-efficient — the Echomancer contributes persistent temperature escalation at 2 AP per turn without requiring the Pyromancer to keep investing.

**Echoing Rewind (Chronomancer ally) — temporal resonance:**
If an ally Chronomancer used Rewind to restore a target's temperature to a previous state (e.g., Rewind brought a target from +70 back to +40, restoring +30 temperature toward neutral), and the Echomancer subsequently uses Echo on that Rewind: the Echo applies **60% of the temperature restoration** — if Rewind restored +30 temperature, the Echo restores an additional **+18 temperature** (brings target from +40 to +22, further toward WARM). Echoing temporal effects feels thematically dissonant — this interaction is documented as **"temporal resonance,"** a case where the Echomancer's recursion mechanic bleeds into the Chronomancer's time-manipulation domain. The flavor interpretation: the Echomancer doesn't fully understand what it is doing when it echoes Rewind; it is replaying a thermal memory rather than deliberately manipulating time. Mechanically it works; fictionally it is strange.

**Afterimage temperature — explosion carries Echomancer's thermal state:**
When an Afterimage is destroyed (its 10 HP pool is depleted, triggering the 12 HP AoE explosion), the explosion carries a minor thermal burst based on the Echomancer's current temperature at the moment of destruction:
- If the Echomancer is **OVERHEATED** (+61 or higher): the Afterimage explosion applies **+5 temperature** to all units in the 1-tile blast radius (a hot explosion from a heat-stressed construct)
- If the Echomancer is **SUPERCOOLED** (-31 to -60) or **FROZEN SOLID** (-61 or lower): the explosion applies **-5 temperature** to all units in the 1-tile blast radius (a burst of cold from a cryogenically stressed construct)
- If the Echomancer is at any other temperature state (WARM, NEUTRAL, COLD, or HOT): the explosion has no temperature component (0 change) — only the baseline 12 HP Arcane AoE damage

This is a minor mechanic with primarily flavor impact, but it adds physical coherence to the Afterimage system: the decoys are extensions of the Echomancer, and when the Echomancer is at thermal extremes, that is reflected in the way the decoys detonate.

---

## 10. Augmentation Spell

### Phantom Imprint

**AP Cost:** 3 | **Range:** 4 tiles | **Targeting:** Single allied unit | **Cooldown:** 4 turns

Imprints the Echomancer's echo-weave onto an ally, marking their next action for automatic repetition at reduced power.

**Effects (next turn):**
- The ally's next spell or attack resolves normally, then -- 1 initiative phase later, at end of the current resolution phase -- an echo of that same action repeats automatically at 50% effectiveness (half damage, half duration on status effects applied, rounded down)
- The echo pursues the same target: if the target has moved, the echo follows; if the target has died, the echo redirects to the nearest valid enemy within the original spell's targeting range
- Healing effects cannot be echoed (temporal echo cannot restore what was never spent)
- The imprint expires unused at end of the following turn

**Tactical intent:** Delayed free double-strike at reduced power. The initiative delay on the echo (end-of-phase) means enemies who survive the first hit have a window to react before the echo arrives. Skilled opponents reduce the echo's value; passive opponents eat both. Status effects at 50% duration round down -- a 2-turn effect echoes as 1 turn; a 1-turn effect does not echo at all -- making Imprint most powerful on high-damage or long-duration spells. The redirect on target death prevents AP waste in chaotic resolution phases.

**Notable interactions:** Phantom Imprint on a Chronomancer Timestep target: original action -> Timestep second action -> Phantom echo = three total hits from one unit in one phase. Phantom Imprint on a Toximancer Venom Infused ally: the echo also carries POISON stacks, which can trigger the Toximancer's stack-doubling synergy a second time on a pre-poisoned target in the same phase.

*End of Echomancer design document.*

# Game Modes

## Overview

Battlemancers supports three core modes at launch, designed to serve different player types: story-driven campaign players, competitive tacticians, and replayability-focused skirmishers.

---

## 1. Campaign Mode

### Structure
- Linear story-driven progression through a series of tactical battles
- Player builds a roster of Mancers, unlocking new types as they progress
- Battles are scenario-driven with unique objectives beyond "defeat all enemies"

### Mancer Progression
- Mancers gain XP from combat; level up unlocks stat improvements and optional spell variant customization
- Permadeath optional setting: fallen Mancers are lost for subsequent missions (hardcore mode)
- Roster grows from 3 available Mancers at start to all 19 over campaign arc
- Campaign warbands follow the same 1,000-point / 3-Mancer cap as Skirmish; players choose their faction at campaign start

### Mission Types
- **Elimination:** Defeat all enemies
- **Escort:** Get a specific unit to an exit tile within N rounds
- **Hold:** Control designated tiles for N consecutive rounds
- **Survival:** Survive N rounds against escalating enemy waves
- **Assassination:** Defeat the enemy commander (other enemies are secondary)
- **Puzzle:** Scripted scenario with a specific solution chain (teaches mechanics)

### Map Design in Campaign
- Maps are hand-crafted with designer intent
- Each map introduces or emphasizes specific mechanics (one map per new terrain state as tutorial)
- Environmental storytelling through terrain: a burned village has pre-set ON_FIRE tiles; a flooded fortress has FLOODED zones

---

## 2. Skirmish Mode

### Structure
- Single-battle mode; no progression, no stakes, pure tactics
- Choose team → choose map → fight
- Local play vs. AI or vs. second player on same screen
- Eventually: online ranked/casual

### Team Selection
- Standard **blind pick:** both players build their Warband independently (up to 3 Mancers + supporting units), then lists are revealed simultaneously at match start
- **Draft mode:** alternate Mancer picks from full roster; first-picked team selects side, second picks last. Supporting unit quantities are chosen post-draft.
- **Mirror mode:** both teams play identical Mancer compositions with identical faction (test of pure execution and activation reads)

### Difficulty (vs. AI)
- `Recruit:` AI takes suboptimal positioning; does not prioritize combo setup
- `Veteran:` AI sets up basic combos; uses terrain; targets weaknesses
- `Archmage:` AI optimizes turn order; plans 2 turns ahead; executes complex chains

### Map Selection
- Preset maps (curated) and procedurally varied maps (randomized terrain states + biome)
- Player chooses biome + size; generator places terrain features with balance rules

---

## 3. Team Draft Mode (Competitive)

### Structure
- Competitive format for experienced players
- Full 19-Mancer pool available
- **Sequential pick-ban:** 1 ban each → alternate picks until both sides have selected up to 3 Mancers; faction and supporting unit budget allocated after draft concludes

### Pick-Ban Phase
```
BAN phase: each side bans 1 Mancer (2 bans total)
PICK phase (up to 3 Mancers per side):
  Side A picks 1
  Side B picks 2 (back-to-back)
  Side A picks 2
  Side B picks 1
  (snake draft - classic tactics format)
WARBAND phase: each player selects faction + distributes remaining 700 pts among Chaff and Ranged
```

**Rationale for bans:** Some Mancer pairs are extremely high-value (Hydromancer + Electromancer); banning prevents degenerate always-pick combos from dominating competitive play without hard-coding forbidden pairings.

### Map Selection in Draft
- Maps are pre-defined competitive maps only (hand-balanced; no procedural)
- Loser of previous match picks map for next (standard competitive format)
- Maps are designed to not heavily favor specific Mancer types (no volcanic map that auto-wins Pyromancer)

---

## 4. Future Modes (Post-Launch Consideration)

### Gauntlet
- Single player: fight a chain of battles with one persistent team, resources carry over
- Roguelike structure: pick upgrades between battles
- Mancer HP carries over between fights (no full reset)

### Puzzle Challenges
- Standalone tactical puzzles: given a specific board state, find the correct combo sequence to win in 1 turn
- "Into the Breach-style" read-the-board puzzles — teaches advanced interactions
- Leaderboards for fastest solve

### Custom Game
- Full map editor (player-placed tiles, terrain states, unit positions)
- Share custom scenarios online
- Enable community-created content pipeline

---

## Onboarding / Tutorial Design

The tutorial should be integrated into campaign, not isolated:
1. **Mission 1:** 2 Mancers, no terrain interactions, basic movement + single spell
2. **Mission 2:** First terrain state introduced (Hydromancer floods tiles)
3. **Mission 3:** First element combo demonstrated (Hydromancer + Electromancer)
4. **Mission 4–5:** Destruction mechanics (Pyromancer burns terrain, creates pits)
5. **By Mission 6:** Player has all core systems; remaining campaign complexity builds naturally

**No text walls.** Teach by doing. Show → let player try → confirm success. Each new mechanic introduced via a scenario where it's the obvious solution, not via a rules screen.

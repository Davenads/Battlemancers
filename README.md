# Battlemancers

A skill-based turn-based tactical strategy game. Build teams of elemental mages, reshape the battlefield with spells, and chain cross-element combos on destructible isometric maps.

---

## What It Is

Battlemancers is a tactics game where the terrain is as important as your team. Spells don't just deal damage — they transform the ground. A Hydromancer floods a tile; the Electromancer arcs through the water; the Cryomancer freezes the whole wet zone trapping three enemies. The battlefield evolves every turn.

The aesthetic is HD-2D: pixel-art Mancer sprites placed inside real 3D environments with cinematic lighting, depth of field, bloom, and particle-heavy spell effects — in the style of Octopath Traveler.

---

## Core Features

- **19 Mancer archetypes** — each with distinct domains, spells, and tactical roles
- **Destructible, stateful terrain** — fire spreads, ice forms, mud slows, craters change elevation
- **Cross-element combo system** — wet + lightning chains, fire + poisoned ground = toxic fumes, ice + shatter = burst damage
- **Squad-based play** — build teams of 3-5 Mancers; synergy determines power ceiling
- **Isometric HD-2D battlefield** — retro sprites in fully lit 3D environments
- **Skill-forward design** — no RNG in core combat; outcome is legible and earned

---

## The Mancers

| Mancer | Domain | Role |
|---|---|---|
| Pyromancer | Fire, heat | Damage-over-time, area denial, spreading flames |
| Hydromancer | Water, fluids | Push/pull, wet setup, healing |
| Cryomancer | Ice, frost | Slows, freezes, slippery terrain |
| Geomancer | Earth, stone | Walls, elevation, terrain reshaping |
| Aeromancer | Wind, pressure | Displacement, mobility, evasion |
| Electromancer | Lightning | Chain stuns, conductivity, burst |
| Necromancer | Death, corpses | Summons, corpse economy, attrition |
| Chronomancer | Time | Haste, delay, rewind, cooldown manipulation |
| Photomancer | Light, radiance | Vision, blinding, reveals, beams |
| Psychomancer | Mind, will | Charm, panic, confusion |
| Floramancer | Plants, vines | Roots, growth zones, poison pollen |
| Faunamancer | Beasts | Companion units, pack tactics |
| Toximancer | Poison, venom | Poison stacks, debuffs, contamination |
| Osteomancer | Bones, structure | Bone armor, spikes, skeletal constructs |
| Gravimancer | Gravity, force | Pulls, crushes, fall damage |
| Sonimancer | Sound, vibration | Cone attacks, silence, shatter |
| Crystalomancer | Crystals, prisms | Refraction, barriers, stored energy |
| Echomancer | Echoes, repetition | Repeat casts, delayed duplicates |
| Thermomancer | Temperature | Gradient zones, overheat/chill combos |

---

## Design Documents

- `design/mancers/overview.md` — Full roster mechanics, synergies, spell archetypes
- `design/combat/terrain-system.md` — How terrain states work and interact
- `design/combat/spell-system.md` — AP economy, targeting, cooldowns, combos
- `design/combat/turn-structure.md` — Initiative, action phases, win conditions
- `design/combat/status-effects.md` — Full status library with stacking rules
- `design/visual/art-direction.md` — HD-2D spec and VFX guidelines
- `design/game-modes.md` — Campaign, skirmish, team draft

---

## Inspiration

| Game | What We Take |
|---|---|
| Worms Armageddon | Terrain destruction as skill expression |
| Into the Breach | Perfect tactical legibility |
| Divinity: Original Sin 2 | Element combo depth |
| Octopath Traveler | HD-2D visual language |
| Fire Emblem | Squad identity and team building |

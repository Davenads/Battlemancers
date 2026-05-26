# Map Design Principles

## Core Principles

1. **Every map is a puzzle.** The terrain layout should create interesting decisions before a single spell is cast. Choke points, high ground, flanking routes, and hazard zones are deliberate design choices.

2. **Maps should be neutral but dynamic.** No map should strongly favor one Mancer type. Volcanic maps may have lava borders but Pyromancer shouldn't auto-win on them — neutral does not mean featureless.

3. **Destructibility creates narrative.** A map should look different at the end of a fight than the start. A clean ruins map becomes a crater field after a Geomancer/Gravimancer duel. This history should be readable.

4. **Size scales with team size.** 3v3: 10×10 tiles. 4v4: 12×12. 5v5: 14×14. Maps too large slow pacing; too small removes strategy.

---

## Map Zones

Every map should have:
- **Deployment zone:** Where teams start (opposite sides, 2-tile deep zone)
- **Central contest zone:** Tiles that both sides can reach by turn 2 (high value for Hold objectives)
- **Flank routes:** 1-2 paths around the central zone; reward mobile Mancers
- **High ground:** At least 2 elevated tiles per map (incentivizes Geomancer; creates natural sniper positions)
- **Hazard features:** 1-3 pre-placed destructible features or terrain states (creates early tension)

---

## Biome Tile Sets

### Ruins
- Stone, rubble, elevated platforms (collapsed floors), pit hazards
- Pre-placed STONE_WALL segments creating corridors
- Lots of natural cover; flanking is rewarded
- Neutral biome — no elemental advantage

### Swamp
- Shallow water tiles covering 30% of map
- OVERGROWTH patches throughout
- Low elevation (no high ground)
- Floramancer, Hydromancer, and Toximancer love this map
- Fire spreads slowly (wet environment)
- FLOODED zones can expand if Hydromancer adds more water

### Volcanic
- Lava border tiles (impassable, 20 HP/turn if adjacent)
- ELEVATED obsidian rock formations (natural high ground)
- Pre-placed ON_FIRE tiles in center
- Cracked ground (one explosion → immediate PIT)
- Fire spreads extremely fast (FLAMMABLE terrain everywhere)
- Central lava fissure: falls in = instant KO

### Crystal Cavern
- Crystal terrain clusters pre-placed throughout
- Low natural light → BLINDED units have shorter range penalty (visual darkness)
- ELEVATED crystal formations (line-of-sight blocking)
- Pre-placed crystal structures: Photomancer beams bounce immediately on turn 1
- Sonimancer can chain-shatter crystals for rapid map reshaping

### Frozen Tundra
- ICE_TILE covers 50% of ground (slippery everywhere)
- FROZEN WATER_SHALLOW in corners (deep frozen ponds = impassable until melted)
- Pre-placed ICE_WALL formations
- Everything is already partially CHILLED; Cryomancer can achieve FROZEN easier
- Pyromancer melts ice but the steam obscures vision

### Forest
- Dense OVERGROWTH clusters (movement penalty + cover)
- Tall tree features (destroyable, fall on destruction — create RUBBLE hazard zone)
- FLAMMABLE terrain everywhere — fire spreads devastatingly fast
- Hidden paths through overgrowth (higher movement cost but unexpected angle)
- Floramancer turns this map into a labyrinth within 3 turns

---

## Competitive Map Rules (Draft Mode)

Competitive maps follow strict constraints:
- No starting terrain states (no pre-placed ON_FIRE, TOXIC, FLOODED)
- Symmetric layout (both sides have equal access to high ground and cover)
- At least 3 viable pathing options to central zone
- Destructible features present but not placed such that one team can instantly destroy the other's path
- Pre-approved map pool: 6 maps for competitive season, rotate quarterly

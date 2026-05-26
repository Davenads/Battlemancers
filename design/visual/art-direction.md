# Art Direction — HD-2D

## What HD-2D Means

HD-2D is a specific visual language pioneered by Octopath Traveler (2018) and refined in Triangle Strategy and Octopath Traveler II. It is not simply "pixel art with bloom." It is a precise hybrid:

**The formula:**
- **Pixel-art 2D sprites** for all characters, enemies, and portable objects — hand-crafted, expressive, high-frame-count
- **3D polygonal environments** for terrain, architecture, and world geometry — full depth, real shadows
- **Post-processing layer** applied to the combined 2D+3D image: bloom, depth of field, motion blur, color grading
- **Cinematic camera** that reacts to events — zooms in on spell casts, tilts during cutscenes, holds on impactful moments

The result looks like a diorama: characters exist inside a real world, lit by real light, with physical depth — but retain the charm and readability of sprite art.

---

## Technical Breakdown

### Character Sprites (2D layer)
- **Resolution:** 64px height for standard Mancers; 96px for large enemies/bosses
- **Frame count:** Walk: 8 frames, Idle: 4–6 frames, Cast animation: 12–16 frames per spell tier, Death: 6 frames
- **Shading style:** Cel-shaded with hard light/shadow transitions; no dithering for primary shading (clean edges); optional light dithering at sprite edges for softness
- **Outline:** 1px dark outline on all sprites; outline color shifts based on lighting (darker in shadows, lighter in direct light)
- **Color palette:** No formal restriction but each Mancer has a **primary** and **secondary** palette signature (Pyromancer = reds/oranges, Cryomancer = blues/whites, Necromancer = purples/bone whites)
- **Cast pose:** Each spell tier has a distinct pose animation. Quick spells: short snap. Ultimate spells: full 16-frame wind-up that communicates threat to opponent.

### Environment (3D layer)
- **Tile resolution:** 3D tile meshes approximately 2m × 2m per grid tile
- **Polygon budget:** Medium-detail tile meshes; camera distance is fixed isometric so far-field culling reduces draw calls significantly
- **Materials:** Use URP Lit materials with custom toon/stylized shader — not photorealistic; aim for painterly 3D (similar to Triangle Strategy environment quality)
- **Elevation layers:** Each elevation level is a distinct mesh step; transitions between levels have visible ramp or cliff geometry
- **Destructible geometry:** Tile meshes swap to "damaged" variants on state change (cracked ground for PIT, char texture for ON_FIRE aftermath, ice mesh for ICE_TILE). No actual mesh destruction needed — variant swap.
- **Terrain state overlays:** Particle and decal overlays communicate terrain state (fire particle system, ice frost decal, toxic green shimmer shader, flood water mesh overlay)

### Post-Processing Stack (Unity URP)
- **Bloom:** Heavy. Magic spells should glow. Fire is bright orange-yellow haze. Ice is crisp blue. Lightning is white-to-purple flash. All bloom is HDR-driven — bright emissive elements control bloom intensity.
- **Depth of Field:** Bokeh DoF with focus on the active unit or spell impact point. Background tiles slightly blurred during spell execution for cinematic frame. Reset to full focus during movement and planning.
- **Color Grading:** Per-biome LUT. Volcanic biome: warm orange-red grade. Frozen tundra: desaturated blue-white. Swamp: sickly green-yellow. Each biome has a distinct grade to reinforce atmosphere.
- **Vignette:** Subtle vignette always on; intensifies during high-tension moments (multiple units at low HP).
- **Chromatic Aberration:** Brief, strong CA flash on Ultimate spell casts and big explosions. Off at all other times.
- **Motion Blur:** Camera-motion blur only (not per-object); applies when camera sweeps during spell cinematics.

---

## Spell Visual Language

Each element has a defined visual signature. VFX must be **readable** and **distinct** — player must identify the element from the particle shape and color within 0.5 seconds.

| Element | Primary Color | Secondary | Particle Shape | Key Visual |
|---|---|---|---|---|
| Fire | Orange #FF6B1A | Yellow #FFD700 | Rising embers, smoke plumes | Heat shimmer distortion; spreading ground burn |
| Water | Cyan #00B4D8 | White foam | Droplets, wave arcs, ripple circles | Surface reflection, wet trail on ground |
| Ice | Ice blue #A8DAFF | White #FFFFFF | Crystalline shards, frost fractals | Freeze spread from impact point outward |
| Earth | Brown #8B6914 | Gray #808080 | Rubble chunks, dust clouds | Ground crack decal, tile rise animation |
| Wind | White-transparent | Light blue | Curved lines (stylized air flow), leaves | Bend effect on sprites caught in wind |
| Lightning | Purple-white #C8A0FF | Yellow arc #FFE44D | Jagged arc lines, sparks | Bright flash frame, after-image burn |
| Poison | Green #39D353 | Sickly yellow #C5D300 | Bubbles, dripping liquid, spores | Ground stain decal, particle cloud |
| Ice + Shatter | White #FFFFFF burst | Ice blue fragments | Explosive radial shard spray | Full-screen white flash, then shards |
| Bone | Bone white #E8DCC8 | Dark cavity #3D2B1F | Bone shard polygons, dust | Spike emergence from ground |
| Psychic | Deep violet #4B0082 | Pink #FF69B4 | Wavy distortion field, eye motif | Screen edge color warp, spiral particles |
| Sound | Translucent white | Concentric rings | Expanding ring graphics | Ring emanating from source, visible compression |
| Light | Brilliant white #FFFACD | Gold #FFD700 | Lens flare, ray lines | Bloom spike on impact, shadow cast by beam |
| Time | Blue-gray #4682B4 | Silver shimmer | Clock hand motifs, freeze frame echo | World desaturates around STASIS target |
| Crystal | Prismatic / rainbow | Clear #F0F8FF | Refraction prisms, facet glints | Light dispersion rainbow on hit |
| Gravity | Dark purple #1A0033 | Warped space | Space warp distortion mesh | Lens distortion around gravity well |

---

## Camera System

The camera is isometric with a fixed angle (~45° vertical, 45° horizontal). It does not rotate during normal play (the grid is fixed).

### Camera Behaviors

**Standard play:**
- Camera follows active unit loosely (centered on active Mancer with soft follow lag)
- Full battlefield should be visible on standard map sizes (no scroll during small maps)
- Larger maps: camera scrolls during planning phase; snaps to active unit during action

**Spell cinematics:**
- On Heavy or Ultimate spell cast: camera **zooms in** to caster (~30% zoom), brief DoF softens background, then **snaps to impact** at spell landing, holds for 0.3 seconds, returns to normal
- On chain combo trigger: camera pans to follow the chain sequence, pulling back to show full chain spread
- On SHATTER: ultra-close zoom, white flash frame, sharp pull-back as shards scatter

**Camera rules:**
- Never obscure the battlefield during a unit's turn — player needs full visibility for planning
- Cinematics are fast (under 1 second) and skippable with any button press (no unskippable animations)
- Camera reacts to dramatic moments but never takes control away from the player during their turn

---

## Lighting

**Key light:** Directional light from above-and-to-the-side (consistent with isometric perspective). Casts hard shadows from walls and elevated terrain. Color-graded per biome.

**Spell-reactive lighting:** Spell impacts emit brief point lights in element color. A Fireball impact creates a 0.3-second warm orange point light, casting real dynamic shadows on nearby terrain and sprites.

**Terrain state lighting:**
- `ON_FIRE` tiles: constant flickering orange-red point light (low radius, warm color)
- `CHARGED` tiles: pulsing blue-white light with short period
- `FLOODED`/`ICE_TILE`: specular reflections from overhead light — wet surfaces are reflective, ice is shiny
- `STEAM_CLOUD`: diffuse white-gray ambient in cloud volume

**Ambient occlusion:** Pre-baked AO on 3D terrain; sprites receive dynamic AO from terrain geometry to "ground" them in the world.

---

## UI Visual Language

**HUD:**
- Minimal during play — tile highlights, AP bar per unit, HP bar
- Tile highlights: blue = movement range, red = attack/spell range, yellow = danger zone (enemy can reach), green = heal range
- AP bar is 6 segments; each AP spent removes one segment in real time

**Mancer portraits:**
- Large pixel-art portrait (128×128) in UI panels
- Each element has a portrait frame style (fire frame = ember border, ice = frost crystal border)

**Spell indicators:**
- Before confirming a spell: translucent preview of AoE area with element-colored overlay
- Combo indicator: if spell would trigger a cross-element combo, show a glowing chain icon before confirm

**Aesthetic reference:** Triangle Strategy HUD for minimal + elegant; Octopath Traveler for font style and portrait presentation.

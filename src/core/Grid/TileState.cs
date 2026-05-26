namespace Battlemancers.Core.Grid
{
    /// <summary>
    /// The elemental or terrain state of a single grid tile.
    /// Tile states are set by spells, terrain destruction, and environmental effects.
    /// Multiple states can coexist on a tile via the status layer — this enum represents
    /// the PRIMARY ground state. Secondary states (WET as moisture, FLAMMABLE, BRITTLE)
    /// are tracked separately in the status system.
    ///
    /// See design/combat/terrain-system.md for full state interaction rules.
    /// See design/combat/status-effects.md for secondary state layering rules.
    /// </summary>
    public enum TileState
    {
        /// <summary>
        /// Default unmodified terrain. No elemental overlay.
        /// Base movement cost: 1. Passable. No special effects on units.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Tile is saturated with water. Units on wet tiles are conductive.
        /// Lightning spells chain arc to all adjacent Wet units/tiles.
        /// Created by: Hydromancer flood spells, rain, water tile overflow.
        /// Expires: Until dried or frozen.
        /// </summary>
        Wet = 1,

        /// <summary>
        /// Tile is actively on fire. Units entering or standing here take 5 HP/turn.
        /// Spreads to adjacent Normal/Natural/Flammable tiles each turn.
        /// Created by: Pyromancer spells, explosions on flammable tiles, Fire + Charged arc.
        /// Extinguished by: Water, Earth (smother), Sonic shockwave.
        /// </summary>
        Burning = 2,

        /// <summary>
        /// Tile is covered in ice. Movement has a slip chance; tile is conductive.
        /// Shatters on Lightning impact for heavy AoE damage.
        /// Created by: Cryomancer spells on Ground or Flooded tiles.
        /// Melts on Fire hit, becoming Wet.
        /// </summary>
        Frozen = 3,

        /// <summary>
        /// Tile is contaminated with poison/toxic material.
        /// Units on this tile gain Poison stacks each turn.
        /// Created by: Toximancer, Floramancer, corpse explosions, Necromancer effects.
        /// Duration: 3 turns by default.
        /// </summary>
        Poisoned = 4,

        /// <summary>
        /// Tile carries an electrical charge left by Electromancer.
        /// The next unit to enter triggers a Lightning strike on them.
        /// Lightning hitting a Charged tile causes Overload (AoE burst).
        /// Duration: Until triggered (1 use per tile) or grounded by Earth spell.
        /// </summary>
        Charged = 5,

        /// <summary>
        /// Mud — created by Water + Earth interaction (Hydromancer + Geomancer).
        /// Movement cost +2 (heavily penalizes non-flying units).
        /// Duration: 4 turns. Cannot spread.
        /// </summary>
        Mud = 6,

        /// <summary>
        /// Death/necrotic corruption. Ground soaked with death energy.
        /// Ashen Covenant units regenerate HP while standing on Corrupted tiles.
        /// Necromancers gain bonus fuel from Corrupted tiles.
        /// Created by: Mass death events, Necromancer spells.
        /// </summary>
        Corrupted = 7,

        /// <summary>
        /// Hardened obsidian — created by Fire hitting an Earth tile (Fire + Earth reaction).
        /// Impassable stone barrier. Cannot be destroyed by most means.
        /// Blocks movement AND line of sight.
        /// </summary>
        Obsidian = 8,

        /// <summary>
        /// Deep freeze state, stronger than Frozen.
        /// Units on Permafrost tiles have -2 movement and cannot sprint.
        /// Created by: Deep Cryomancer effects, Geomancer + Ice combo.
        /// Semi-permanent — requires significant heat to remove.
        /// </summary>
        Permafrost = 9,

        /// <summary>
        /// Floramancer-created vine zone. Units that end their turn here become Rooted
        /// (cannot move next turn). Counts as Natural terrain for Verdant Pact Terrain Bond.
        /// Burns quickly when hit by Fire. Spreads slowly if Floramancer maintains.
        /// </summary>
        Vines = 10,

        /// <summary>
        /// Toximancer spore cloud on the ground. Applies Poison stacks faster than
        /// standard Poisoned tiles (stacks per entry rather than per turn).
        /// Wind spells disperse the cloud outward (AoE poison pulse).
        /// Duration: 3 turns.
        /// </summary>
        Spores = 11,

        /// <summary>
        /// Tile is completely destroyed — a pit, void, or crater.
        /// Impassable. Units pushed into a Destroyed tile take fall damage and are knocked out.
        /// Cannot be restored by most means (Geomancer fill spell is the exception).
        /// Created by: Heavy explosions, repeated AoE destruction.
        /// </summary>
        Destroyed = 12,

        /// <summary>
        /// Steam cloud — created by Fire hitting a Wet or Flooded tile.
        /// Obscures vision (reduces effective vision range to 2 tiles for units in/adjacent to it).
        /// Deals minor burn damage each turn to units inside.
        /// Duration: 2 turns.
        /// </summary>
        Steam = 13,

        /// <summary>
        /// Natural terrain: forest, grass, fertile earth. Visually lush.
        /// Qualifies for Verdant Pact Terrain Bond faction trait (bonus movement + regen).
        /// Burns readily when hit by Fire (spreads faster than Normal tiles).
        /// Base movement cost: 1.
        /// </summary>
        Natural = 14
    }
}

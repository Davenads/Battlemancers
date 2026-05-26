namespace Battlemancers.Simulation
{
    /// <summary>
    /// Enumerates the seven elemental damage types that Mancers can wield.
    /// Each element has unique interactions with tile states as defined in the
    /// element interaction matrix (see design/combat/status-effects.md and
    /// assets/data/element-interactions.json).
    ///
    /// Elements are used as the incoming spell type when the ElementResolver
    /// looks up an interaction against the current TileState of the target tile.
    /// </summary>
    public enum ElementType
    {
        /// <summary>
        /// Fire — the element of heat, combustion, and spreading flames.
        /// Wielded by: Pyromancer, Thermomancer.
        /// Key interactions: ignites Wet tiles into Steam, spreads Burning state,
        /// melts Frozen tiles to Wet, detonates Charged tiles, creates toxic fumes on Poisoned tiles.
        /// </summary>
        Fire = 0,

        /// <summary>
        /// Water — the element of flow, saturation, and fluid force.
        /// Wielded by: Hydromancer, Thermomancer.
        /// Key interactions: extinguishes Burning tiles, cracks Frozen tiles for damage,
        /// dilutes Poisoned tiles, conducts electricity through Charged tiles to chain-stun.
        /// </summary>
        Water = 1,

        /// <summary>
        /// Ice — the element of cold, frost, and freezing.
        /// Wielded by: Cryomancer, Thermomancer.
        /// Key interactions: freezes Wet tiles solid, flash-freezes Burning tiles,
        /// deepens existing Frozen tiles to Permafrost, preserves Poisoned stacks,
        /// and freezes Charged conductors.
        /// </summary>
        Ice = 2,

        /// <summary>
        /// Lightning — the element of electricity, arcs, and chain reactions.
        /// Wielded by: Electromancer.
        /// Key interactions: chains through Wet tiles to arc adjacent units,
        /// triggers a Firestorm on Burning tiles, shatters Frozen tiles for heavy AoE damage,
        /// amplifies Toxin Shock on Poisoned tiles, and triggers Overload AoE on Charged tiles.
        /// </summary>
        Lightning = 3,

        /// <summary>
        /// Earth — the element of stone, terrain manipulation, and permanent structures.
        /// Wielded by: Geomancer.
        /// Key interactions: creates Mud on Wet tiles, hardens Burning tiles into impassable Obsidian,
        /// creates Permafrost cover on Frozen tiles, contaminates Poisoned ground further,
        /// and magnetizes Charged tiles to pull metal-bearing units.
        /// </summary>
        Earth = 4,

        /// <summary>
        /// Wind — the element of displacement, pressure, and airborne force.
        /// Wielded by: Aeromancer.
        /// Key interactions: disperses Wet tiles into vision-obscuring mist,
        /// fans Burning flames to spread further, sprays Frozen tiles as ice shards,
        /// disperses Poisoned tiles into a spore cloud, builds static charge on Charged tiles.
        /// </summary>
        Wind = 5,

        /// <summary>
        /// Poison — the element of toxins, venom, and contamination.
        /// Wielded by: Toximancer, Floramancer.
        /// Key interactions: infects Wet tiles with toxic water, creates DoT+Poison on Burning tiles,
        /// preserves state on Frozen tiles while adding poison stacks, multiplies stacks on already
        /// Poisoned tiles, and corrodes Charged conductors.
        /// </summary>
        Poison = 6
    }
}

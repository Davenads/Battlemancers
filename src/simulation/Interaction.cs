namespace Battlemancers.Simulation
{
    /// <summary>
    /// Represents the full result of an elemental interaction — what happens when a spell
    /// of a given ElementType strikes a tile in a particular TileState.
    ///
    /// Interactions are loaded from assets/data/element-interactions.json at startup
    /// and looked up at runtime by ElementResolver. Each Interaction is immutable after
    /// construction; all fields are set once via the constructor.
    ///
    /// See also: ElementResolver, ElementType, TileState (Battlemancers.Core.Grid).
    /// </summary>
    public class Interaction
    {
        /// <summary>
        /// The TileState name (as a string matching the TileState enum) that the tile
        /// transitions to as a result of this interaction.
        ///
        /// Examples: "Burning", "Wet", "Steam", "Obsidian", "Permafrost".
        /// A value of "Normal" indicates the tile is cleansed/reset.
        /// A value equal to the incoming state indicates no state change occurs.
        ///
        /// The ElementResolver caller is responsible for parsing this string back to the
        /// TileState enum when applying the result to GridData.
        /// </summary>
        public string ResultingTileState { get; }

        /// <summary>
        /// Ordered list of game effects that execute when this interaction triggers.
        /// Effects are applied in array order. An empty array indicates no mechanical
        /// effects beyond the tile state change (if any).
        ///
        /// Examples: damage to hit unit, status applied to adjacent units, AoE terrain change.
        /// </summary>
        public Effect[] Effects { get; }

        /// <summary>
        /// Hint string passed to the VFX Director to identify which visual/audio effect
        /// to play when this interaction fires.
        ///
        /// Examples: "steam_cloud", "chain_arc", "ice_shatter", "obsidian_form",
        /// "firestorm_burst", "arc_explosion", "mist_dispersal".
        /// An empty string means no special VFX is required for this interaction.
        /// </summary>
        public string VfxHint { get; }

        /// <summary>
        /// Constructs an Interaction with all fields set.
        /// </summary>
        /// <param name="resultingTileState">
        /// The TileState name the tile transitions to. Must match a valid TileState enum name.
        /// </param>
        /// <param name="effects">
        /// Array of effects to apply. Must not be null; pass an empty array for no effects.
        /// </param>
        /// <param name="vfxHint">
        /// VFX identifier string for the presentation layer. Pass empty string for no VFX.
        /// </param>
        public Interaction(string resultingTileState, Effect[] effects, string vfxHint)
        {
            ResultingTileState = resultingTileState ?? "Normal";
            Effects = effects ?? System.Array.Empty<Effect>();
            VfxHint = vfxHint ?? string.Empty;
        }
    }

    /// <summary>
    /// A single discrete game effect within an elemental Interaction.
    ///
    /// Effects are the atomic mechanical outcomes of a spell striking a tile in a particular
    /// state. Each effect has a type, a target scope, an optional status identifier, and an
    /// integer value whose meaning depends on the EffectType.
    ///
    /// Multiple Effects are bundled in an Interaction.Effects array and applied in sequence.
    /// </summary>
    public class Effect
    {
        /// <summary>
        /// The category of effect to apply.
        ///
        /// Known values and their semantics:
        /// <list type="bullet">
        ///   <item><description>
        ///     "DAMAGE" — deal damage equal to <see cref="Value"/> HP to units matching <see cref="Target"/>.
        ///   </description></item>
        ///   <item><description>
        ///     "STATUS_APPLY" — apply the status named by <see cref="StatusId"/> to units matching
        ///     <see cref="Target"/> for <see cref="Value"/> turns (or <see cref="Value"/> stacks where applicable).
        ///   </description></item>
        ///   <item><description>
        ///     "CHAIN_TO_ADJACENT" — replicate the triggering effect to all adjacent tiles/units
        ///     matching <see cref="Target"/>. <see cref="Value"/> is the chain falloff multiplier
        ///     in percent (100 = full damage, 50 = half).
        ///   </description></item>
        ///   <item><description>
        ///     "TERRAIN_CHANGE" — change the tile state as specified by the parent
        ///     <see cref="Interaction.ResultingTileState"/>. <see cref="Value"/> is unused (pass 0).
        ///   </description></item>
        ///   <item><description>
        ///     "PUSH" — displace units matching <see cref="Target"/> by <see cref="Value"/> tiles
        ///     away from the spell origin.
        ///   </description></item>
        ///   <item><description>
        ///     "VISION_REDUCE" — reduce vision range of units in <see cref="Target"/> area by
        ///     <see cref="Value"/> tiles for the duration of the tile state (e.g., Steam).
        ///   </description></item>
        ///   <item><description>
        ///     "STACK_MULTIPLY" — multiply existing status stacks on <see cref="Target"/> units
        ///     by <see cref="Value"/> (used for Poison stack escalation interactions).
        ///   </description></item>
        /// </list>
        /// </summary>
        public string EffectType { get; }

        /// <summary>
        /// Specifies which units or tiles are affected by this effect.
        ///
        /// Known values:
        /// <list type="bullet">
        ///   <item><description>"HIT_UNIT" — the unit standing on the target tile.</description></item>
        ///   <item><description>"ADJACENT_UNITS" — all units on tiles adjacent to the target tile.</description></item>
        ///   <item><description>"TILE" — the target tile itself (for terrain changes, ground effects).</description></item>
        ///   <item><description>"AOE_2" — all units within 2 tiles of the target (inclusive).</description></item>
        ///   <item><description>"ADJACENT_WET_UNITS" — units on adjacent Wet tiles (lightning chain condition).</description></item>
        /// </list>
        /// </summary>
        public string Target { get; }

        /// <summary>
        /// The string identifier of the status effect to apply when EffectType is "STATUS_APPLY".
        /// Null for all other EffectTypes.
        ///
        /// Must match a status ID recognized by the StatusManager. Examples:
        /// "BURNING", "FROZEN", "STUNNED", "POISONED", "SLOWED", "ROOTED", "BLINDED".
        /// </summary>
        public string StatusId { get; }

        /// <summary>
        /// Numeric parameter whose meaning depends on EffectType:
        /// <list type="bullet">
        ///   <item><description>DAMAGE — HP damage dealt.</description></item>
        ///   <item><description>STATUS_APPLY — duration in turns, or stack count for stackable statuses.</description></item>
        ///   <item><description>CHAIN_TO_ADJACENT — damage percentage passed to chained targets (0–100).</description></item>
        ///   <item><description>TERRAIN_CHANGE — unused; always 0.</description></item>
        ///   <item><description>PUSH — number of tiles displaced.</description></item>
        ///   <item><description>VISION_REDUCE — number of vision range tiles removed.</description></item>
        ///   <item><description>STACK_MULTIPLY — multiplier applied to existing status stacks.</description></item>
        /// </list>
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Constructs an Effect with all fields set.
        /// </summary>
        /// <param name="effectType">The category of effect. See <see cref="EffectType"/> for known values.</param>
        /// <param name="target">Scope of affected units/tiles. See <see cref="Target"/> for known values.</param>
        /// <param name="statusId">Status identifier for STATUS_APPLY effects; null otherwise.</param>
        /// <param name="value">Numeric parameter; interpretation depends on <paramref name="effectType"/>.</param>
        public Effect(string effectType, string target, string statusId, int value)
        {
            EffectType = effectType ?? string.Empty;
            Target = target ?? string.Empty;
            StatusId = statusId;
            Value = value;
        }
    }
}

using UnityEngine;

namespace Battlemancers.Data
{
    /// <summary>
    /// Static configuration for a single Mancer spell. Stored as a Unity ScriptableObject asset
    /// so designers can author and tune spells in the Inspector without touching code.
    ///
    /// SpellData holds only immutable definition data — no runtime state (cooldown counters,
    /// current targets, etc.) lives here. Runtime state belongs in UnitState / SpellRuntimeState.
    /// </summary>
    [CreateAssetMenu(fileName = "New Spell", menuName = "Battlemancers/Spell Data")]
    public class SpellData : ScriptableObject
    {
        // -----------------------------------------------------------------------------------------
        // Identity
        // -----------------------------------------------------------------------------------------

        [Header("Identity")]

        /// <summary>
        /// Unique identifier used internally (e.g., "pyromancer_fireball_standard").
        /// Convention: {mancertype}_{effect}_{tier}. Never shown to players.
        /// </summary>
        public string spellId;

        /// <summary>Player-visible spell name (e.g., "Fireball").</summary>
        public string displayName;

        /// <summary>Short description shown in the action menu tooltip.</summary>
        [TextArea(2, 4)] public string description;

        // -----------------------------------------------------------------------------------------
        // Economy
        // -----------------------------------------------------------------------------------------

        [Header("Economy")]

        /// <summary>
        /// Action Point cost to cast this spell. Mancers have 6 AP per turn base.
        /// Quick = 1-2 AP, Standard = 3 AP, Heavy = 4-5 AP, Ultimate = 6 AP.
        /// </summary>
        [Tooltip("AP cost to cast. Mancers have 6 AP per turn. Quick=1-2, Standard=3, Heavy=4-5, Ultimate=6.")]
        [Range(1, 6)] public int apCost = 3;

        /// <summary>
        /// Number of turns after use before this spell can be cast again.
        /// 0 = no cooldown (spammable). Quick spells: 0-1. Standard: 1-2. Heavy: 2-3. Ultimate: 4-5.
        /// </summary>
        [Tooltip("Turns before this spell can be used again. 0 = no cooldown.")]
        [Range(0, 5)] public int cooldownTurns = 1;

        /// <summary>
        /// Spell tier classification for UI display and balance auditing.
        /// </summary>
        public SpellTier tier;

        // -----------------------------------------------------------------------------------------
        // Targeting
        // -----------------------------------------------------------------------------------------

        [Header("Targeting")]

        /// <summary>Shape and mechanism of spell targeting.</summary>
        public SpellTargetType targetType;

        /// <summary>
        /// Maximum range in tiles, measured as Manhattan distance.
        /// Mancers on elevated tiles add +1 to effective range. BLINDED units have range clamped to 1.
        /// </summary>
        [Tooltip("Maximum range in tiles (Manhattan distance).")]
        [Range(1, 12)] public int range = 4;

        /// <summary>
        /// Blast radius in tiles for AoE spells (AoeCircle, Cone).
        /// 0 for single-tile or line spells.
        /// </summary>
        [Tooltip("Radius of AoE effect. Only used for Circle and Cone types.")]
        [Range(0, 4)] public int aoeRadius = 0;

        /// <summary>
        /// Length of the effect in tiles for Line and Cone targeting types.
        /// Ignored for SingleTarget, AoeCircle, Ground, Self, and AlliesInRange.
        /// </summary>
        [Tooltip("Length of line/cone in tiles. Only used for Line and Cone types.")]
        [Range(1, 8)] public int lineLength = 3;

        /// <summary>
        /// Whether this spell requires unobstructed line of sight to the target tile.
        /// Sonic and some Psychomancer spells ignore LOS.
        /// </summary>
        [Tooltip("If false, spell can target through walls and terrain obstructions.")]
        public bool requiresLineOfSight = true;

        /// <summary>
        /// Whether this spell can target empty tiles (no unit required as target).
        /// Terrain placement spells set this to true.
        /// </summary>
        [Tooltip("If true, spell can target empty tiles without a unit present.")]
        public bool canTargetEmptyTile = false;

        // -----------------------------------------------------------------------------------------
        // Damage
        // -----------------------------------------------------------------------------------------

        [Header("Damage")]

        /// <summary>Primary element of this spell. Determines terrain interaction lookups in ElementResolver.</summary>
        public ElementType element;

        /// <summary>Base damage dealt to units in the target area before armor and modifiers.</summary>
        [Tooltip("Base damage before armor and conditional bonuses.")]
        [Range(0, 60)] public int baseDamage = 10;

        /// <summary>
        /// Optional damage bonuses that activate when the target has a specific tile state or unit status.
        /// Used to express combo damage (e.g., +20 vs WET targets for lightning spells).
        /// </summary>
        [Tooltip("Bonus damage applied when the target tile or unit matches a specific state.")]
        public ConditionalDamageBonus[] conditionalBonuses;

        // -----------------------------------------------------------------------------------------
        // Effects
        // -----------------------------------------------------------------------------------------

        [Header("Effects")]

        /// <summary>
        /// Status effects applied to units hit by this spell.
        /// Multiple entries can apply different statuses to unit vs. tile, or stack multiple effects.
        /// </summary>
        [Tooltip("Status effects applied on hit. Can target the unit, the tile, or both.")]
        public StatusEffectApplication[] appliedEffects;

        /// <summary>
        /// Terrain state changes this spell applies to tiles in the affected area.
        /// Evaluated after damage; can set, clear, or transform tile states.
        /// </summary>
        [Tooltip("Terrain state changes applied to hit tiles.")]
        public TerrainChangeApplication[] terrainChanges;

        /// <summary>
        /// Temperature change applied to units hit by this spell.
        /// Positive values heat the target; negative values cool the target.
        /// 0 means this spell has no thermal effect.
        /// Applied via TemperatureManager.ApplyTemperatureChange after damage resolution.
        /// Typical ranges: Quick spells ±10-15, Standard ±15-20, Heavy/Ultimate ±25-35.
        /// </summary>
        [Tooltip("Temperature change on hit. Positive = heat, negative = cool. 0 = no effect.")]
        [Range(-40, 40)] public int temperatureDelta = 0;

        /// <summary>
        /// Push or pull displacement applied to units hit. Positive = pushed away from caster.
        /// Displacement can chain into fall damage if pushed off elevated tiles.
        /// </summary>
        [Tooltip("Displacement effect applied to units on hit.")]
        public DisplacementEffect displacement;

        /// <summary>
        /// If this spell summons a companion unit, this field names the unit prefab tag to spawn.
        /// Only used by Necromancer, Faunamancer, and Osteomancer summon spells.
        /// </summary>
        [Tooltip("Companion unit tag to summon. Leave empty for non-summon spells.")]
        public string summonUnitTag;

        /// <summary>
        /// Maximum number of companion units of this type that can be on the field simultaneously.
        /// 0 means this spell does not summon units.
        /// </summary>
        [Tooltip("Max companions of this type on field at once. 0 = not a summon spell.")]
        [Range(0, 3)] public int maxSummonCount = 0;

        // -----------------------------------------------------------------------------------------
        // Presentation
        // -----------------------------------------------------------------------------------------

        [Header("VFX / Audio")]

        /// <summary>
        /// Tag used by VFXDirector to look up and play the correct VFX Graph asset.
        /// Convention: "{element}_{spelltype}" e.g., "fire_projectile", "ice_aoe", "lightning_chain".
        /// </summary>
        [Tooltip("Tag used by VFXDirector to play the correct effect.")]
        public string vfxTag;

        /// <summary>
        /// FMOD event path used by AudioDirector for spell cast sound.
        /// Format: "spell/{element}/cast" e.g., "spell/fire/cast".
        /// </summary>
        [Tooltip("FMOD event path for spell cast audio.")]
        public string sfxCastTag;

        /// <summary>
        /// FMOD event path used by AudioDirector for spell impact sound.
        /// Format: "spell/{element}/impact".
        /// </summary>
        [Tooltip("FMOD event path for spell impact audio.")]
        public string sfxImpactTag;

        /// <summary>Camera shake intensity on impact. 0 = no shake. Scale: 0-1 (reserved for Heavy/Ultimate).</summary>
        [Tooltip("Camera shake intensity on impact. 0 = none, 1 = maximum (reserved for Ultimate spells).")]
        [Range(0f, 1f)] public float cameraShakeIntensity = 0f;

        [Header("Design Notes")]

        /// <summary>Internal design notes for team use. Not shown to players.</summary>
        [TextArea(2, 4)] public string designNotes;
    }

    // =============================================================================================
    // Enums
    // =============================================================================================

    /// <summary>Spell tier determines AP cost bracket and cooldown expectations.</summary>
    public enum SpellTier
    {
        /// <summary>1-2 AP, 0-1 turn cooldown. Minor effects, position setup, small applications.</summary>
        Quick,

        /// <summary>3 AP, 1-2 turn cooldown. Core identity spells; reliable damage + state application.</summary>
        Standard,

        /// <summary>4-5 AP, 2-3 turn cooldown. Major AoE or significant terrain manipulation.</summary>
        Heavy,

        /// <summary>6 AP (full turn), 4-5 turn cooldown. Transformative board-state effect.</summary>
        Ultimate
    }

    /// <summary>
    /// How a spell selects its targets. Determines targeting UI, resolution logic, and which tiles are affected.
    /// </summary>
    public enum SpellTargetType
    {
        /// <summary>Targets a single unit. Requires LOS unless overridden.</summary>
        SingleTarget,

        /// <summary>Straight line of tiles from caster through target. Hits all units along the line.</summary>
        Line,

        /// <summary>Fan-shaped area in a chosen direction. Width determined by aoeRadius.</summary>
        Cone,

        /// <summary>Circular blast centered on target tile. Radius determined by aoeRadius.</summary>
        AoeCircle,

        /// <summary>
        /// Targets a tile rather than a unit. Valid even on empty tiles.
        /// Used for terrain placement spells (Stone Wall, Flood Zone, Vine Barrier).
        /// </summary>
        Ground,

        /// <summary>Always targets the caster. No targeting UI shown.</summary>
        Self,

        /// <summary>Automatically targets all allied units within range. No targeting UI shown.</summary>
        AlliesInRange,

        /// <summary>
        /// Targets all enemy units within range simultaneously.
        /// Used for mass-effect spells like Psychomancer's Panic Wave.
        /// </summary>
        AllEnemiesInRange,

        /// <summary>
        /// Projectile travels along a path and can bounce off Crystal terrain.
        /// Target is a direction; impact point determined by physics simulation.
        /// </summary>
        Projectile,

        /// <summary>
        /// Chains to additional targets after the initial hit.
        /// Chain count and max jump distance controlled by aoeRadius and lineLength fields respectively.
        /// </summary>
        Chain
    }

    /// <summary>
    /// Elemental type of a spell or terrain state. Used as the key into ElementResolver's interaction table.
    /// </summary>
    public enum ElementType
    {
        /// <summary>Fire — DoT, area denial, spreading. Strong vs. Frozen; creates steam on Wet.</summary>
        Fire,

        /// <summary>Water — push/pull, applies Wet state, heals allies. Conducts on Charged tiles.</summary>
        Water,

        /// <summary>Ice — Slow, Freeze, creates slippery tiles. Shatters on Charged or Frozen.</summary>
        Ice,

        /// <summary>Lightning — chain damage, Stun, amplified by Wet and Charged states.</summary>
        Lightning,

        /// <summary>Earth — walls, elevation changes, mud terrain, cover creation.</summary>
        Earth,

        /// <summary>Wind — displacement, projectile deflection, fans flames, disperses clouds.</summary>
        Wind,

        /// <summary>Poison — stacking DoT, contaminates water/terrain, synergizes with Toximancer.</summary>
        Poison,

        /// <summary>Necrotic — undead synergy, corpse interaction, healing reduction, Cursed state.</summary>
        Necrotic,

        /// <summary>Light — illumination, blinding, reveals hidden units, Photomancer beams.</summary>
        Light,

        /// <summary>Sound — cone disruption, Silence, Shatter vs. Frozen/Crystal, vibration damage.</summary>
        Sound,

        /// <summary>Gravity — pull/push forces, fall damage amplification, immobilization.</summary>
        Gravity,

        /// <summary>Time — Haste, Slow, cooldown manipulation, Chronomancer domain.</summary>
        Time,

        /// <summary>Crystal — refraction, stored energy, barrier creation, bounces Projectile spells.</summary>
        Crystal,

        /// <summary>
        /// Psychic — charm, panic, morale damage. LOS-independent against some targets.
        /// Countered by Gilded Throne's Iron Discipline faction trait.
        /// </summary>
        Psychic,

        /// <summary>
        /// Arcane — neutral magical damage with no elemental state interaction.
        /// Used as a fallback for spells that should not trigger element combos.
        /// </summary>
        Arcane,

        /// <summary>Thermal — heat gradient manipulation, overheat/chill zone control (Thermomancer).</summary>
        Thermal
    }

    // =============================================================================================
    // Supporting Serializable Classes
    // =============================================================================================

    /// <summary>
    /// Defines a bonus damage value that applies when the target unit or tile matches a specific state.
    /// Used to encode combo damage (e.g., Lightning spell gains +20 vs. WET targets).
    /// </summary>
    [System.Serializable]
    public class ConditionalDamageBonus
    {
        /// <summary>Human-readable description for the inspector (e.g., "Bonus vs. WET targets").</summary>
        public string conditionDescription;

        /// <summary>
        /// String key that must match either a TileState enum value name or a StatusType enum value name.
        /// The SpellResolver checks both the target tile's state and the target unit's status list.
        /// Example values: "Wet", "Burning", "Frozen", "Poisoned", "Charged".
        /// </summary>
        [Tooltip("TileState or StatusType name that triggers this bonus. Must match enum value exactly.")]
        public string triggerState;

        /// <summary>Flat damage added when the trigger condition is met.</summary>
        [Range(0, 50)] public int bonusDamage;

        /// <summary>
        /// If true, the bonus applies multiplicatively (bonusDamage becomes a percentage bonus).
        /// If false, it is a flat addition. False for most cases.
        /// </summary>
        [Tooltip("If true, bonusDamage is treated as a percentage increase rather than flat addition.")]
        public bool isMultiplicative = false;
    }

    /// <summary>
    /// Describes a status effect applied to a unit or tile when a spell hits.
    /// A single spell can apply multiple StatusEffectApplications (e.g., both Frozen to tile and Slowed to unit).
    /// </summary>
    [System.Serializable]
    public class StatusEffectApplication
    {
        /// <summary>
        /// Status type to apply. Must match a StatusType enum value name in the status system.
        /// Example values: "Burning", "Wet", "Frozen", "Poisoned", "Charged", "Slowed", "Stunned",
        /// "Charmed", "Panicked", "Silenced", "Blinded", "Rooted", "Hasted", "Cursed".
        /// </summary>
        public string statusType;

        /// <summary>Duration in turns. -1 = permanent until cleansed.</summary>
        [Tooltip("Duration in turns. -1 = permanent until explicitly removed.")]
        [Range(-1, 10)] public int duration = 2;

        /// <summary>
        /// Number of stacks applied for stackable statuses (e.g., Poison stacks).
        /// Non-stackable statuses ignore this value; set to 1 for them.
        /// </summary>
        [Range(1, 5)] public int stacksApplied = 1;

        /// <summary>
        /// If true, the status is applied to the tile rather than the unit on it.
        /// Tile-applied statuses affect all future units that move through or end turns on that tile.
        /// </summary>
        [Tooltip("True = status applied to tile (terrain state), false = applied directly to the hit unit.")]
        public bool appliesToTile = false;

        /// <summary>
        /// Reserved field — must always be 1.0.
        /// Battlemancers uses fully deterministic simulation; probabilistic effect application
        /// is not supported. SpellResolver ignores values below 1.0 and always applies effects.
        /// If you need conditional application, use a threshold or status-check condition instead.
        /// Setting this below 1.0 in JSON has no gameplay effect and will log a data warning at load time.
        /// </summary>
        [Tooltip("Reserved. Must be 1.0. Probabilistic application is not supported — set conditions in spell logic instead.")]
        [Range(0f, 1f)] public float applicationChance = 1.0f;
    }

    /// <summary>
    /// Describes a terrain state change applied to tiles in a spell's affected area.
    /// </summary>
    [System.Serializable]
    public class TerrainChangeApplication
    {
        /// <summary>
        /// The TileState to set. Must match a TileState enum value name.
        /// Example values: "Burning", "Wet", "Frozen", "Poisoned", "Charged", "Muddy",
        /// "Obsidian", "Permafrost", "ToxicGround", "VineCovered", "ThornyGround".
        /// </summary>
        [Tooltip("TileState enum value name to set on the affected tile(s).")]
        public string targetTileState;

        /// <summary>Which tiles within the spell's area of effect receive this change.</summary>
        [Tooltip("Which tiles get the state change: just the hit tile, adjacent tiles, or all area tiles.")]
        public TerrainChangeTarget changeTarget;

        /// <summary>
        /// Duration in turns before the tile naturally reverts. 0 = permanent until overwritten.
        /// Burning terrain ticks each turn and may spread; Wet terrain dissipates after a few turns.
        /// </summary>
        [Tooltip("Turns before this terrain state reverts naturally. 0 = permanent until overwritten.")]
        [Range(0, 10)] public int duration = 0;

        /// <summary>
        /// If true, this change overwrites any existing terrain state. If false, it only applies
        /// if the current tile state is Normal or the specific expected prior state.
        /// </summary>
        [Tooltip("If true, overwrites any existing terrain state. If false, applies only to Normal tiles.")]
        public bool overwriteExistingState = true;
    }

    /// <summary>Which tiles within a spell's resolved area receive a terrain state change.</summary>
    public enum TerrainChangeTarget
    {
        /// <summary>Only the directly hit tile receives the terrain change.</summary>
        HitTile,

        /// <summary>The hit tile and its 4 orthogonal neighbors receive the change.</summary>
        AdjacentTiles,

        /// <summary>All tiles within the spell's AoE area receive the change.</summary>
        AllAreaTiles,

        /// <summary>
        /// Only tiles that currently match a specific prerequisite state receive the change.
        /// Used for fire spreading to adjacent Burning tiles, or ice freezing adjacent Wet tiles.
        /// </summary>
        MatchingStateTiles
    }

    /// <summary>
    /// Defines a push or pull displacement effect applied to units on hit.
    /// Displacement is resolved after damage. Units pushed off elevated tiles take fall damage.
    /// </summary>
    [System.Serializable]
    public class DisplacementEffect
    {
        /// <summary>Whether this spell applies any displacement at all.</summary>
        public bool hasDisplacement = false;

        /// <summary>
        /// Number of tiles displaced. Positive = pushed away from the caster's position.
        /// Negative = pulled toward the caster's position.
        /// </summary>
        [Tooltip("Tiles displaced. Positive = push away from caster, negative = pull toward caster.")]
        [Range(-6, 6)] public int tiles = 0;

        /// <summary>
        /// If true, all units in the AoE are displaced. If false, only the primary target is displaced.
        /// AoE displacement is used for Aeromancer's Cyclone and Gravimancer's Singularity.
        /// </summary>
        [Tooltip("True = all units in AoE are displaced, false = only the primary target.")]
        public bool affectsAllInArea = false;

        /// <summary>
        /// Damage dealt per tile of forced movement when a unit collides with a wall or terrain feature.
        /// 0 = collision with walls deals no damage.
        /// </summary>
        [Tooltip("Damage per tile if the displaced unit collides with a wall or impassable terrain.")]
        [Range(0, 15)] public int collisionDamagePerTile = 0;
    }
}

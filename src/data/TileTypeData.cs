using UnityEngine;

namespace Battlemancers.Data
{
    /// <summary>
    /// Static configuration for a single tile state. Stored as a Unity ScriptableObject asset.
    /// One TileTypeData asset exists per TileState enum value (e.g., Normal, Burning, Wet, Frozen, etc.).
    ///
    /// TileTypeData defines how a tile state behaves: movement cost, hazard effects, spread rules,
    /// duration, faction interactions, and visual/audio presentation.
    ///
    /// The tileStateId field must match the TileState enum value name exactly so the simulation's
    /// TileTypeRegistry can resolve a TileState enum value to its corresponding TileTypeData at runtime.
    ///
    /// Runtime tile state is held in the simulation's GridData.Tile class — not here.
    /// This asset is immutable configuration only.
    /// </summary>
    [CreateAssetMenu(fileName = "New Tile Type", menuName = "Battlemancers/Tile Type Data")]
    public class TileTypeData : ScriptableObject
    {
        // -----------------------------------------------------------------------------------------
        // Identity
        // -----------------------------------------------------------------------------------------

        [Header("Identity")]

        /// <summary>
        /// Must exactly match the TileState enum value name this asset describes.
        /// Example: "Burning", "Wet", "Frozen", "Poisoned", "Charged", "Muddy", "Obsidian",
        /// "Permafrost", "ToxicGround", "VineCovered", "ThornyGround", "Crystal", "Corrupted",
        /// "Elevated", "Rubble", "Normal".
        /// </summary>
        [Tooltip("Must match the TileState enum value name exactly. Used for runtime lookup.")]
        public string tileStateId;

        /// <summary>Player-visible name shown in the tile inspector tooltip (e.g., "Burning Ground").</summary>
        public string displayName;

        /// <summary>Short description shown on the tile tooltip when the player hovers over it.</summary>
        [TextArea(1, 3)] public string description;

        // -----------------------------------------------------------------------------------------
        // Movement
        // -----------------------------------------------------------------------------------------

        [Header("Movement")]

        /// <summary>
        /// Movement cost multiplier for entering this tile. Applied against a unit's remaining move range.
        /// 1.0 = normal (1 AP to enter). 2.0 = difficult terrain (2 AP). 0.5 = accelerated (ice slide).
        /// Impassable tiles have isPassable = false; this value is ignored for them.
        /// </summary>
        [Tooltip("AP cost multiplier to enter this tile. 1.0 = normal. 2.0 = difficult. 0.5 = accelerated.")]
        [Range(0f, 5f)] public float movementCostMultiplier = 1.0f;

        /// <summary>
        /// Whether units can enter this tile at all.
        /// False for walls, full terrain obstructions, and deep chasms.
        /// </summary>
        public bool isPassable = true;

        /// <summary>
        /// Whether this tile state blocks line-of-sight for spell targeting and ranged attacks.
        /// Walls, tall stone, and dense vine barriers block LOS. Burning and Wet tiles do not.
        /// Steam clouds are a special case: they block LOS but are passable.
        /// </summary>
        [Tooltip("True if this tile state prevents line-of-sight from passing through it.")]
        public bool blocksLineOfSight = false;

        /// <summary>
        /// Whether this tile provides cover to units standing on it.
        /// Cover reduces incoming ranged damage. Low walls, rubble, and crystal formations provide cover.
        /// </summary>
        [Tooltip("True if units on this tile gain a cover damage reduction bonus vs. ranged attacks.")]
        public bool providesCover = false;

        /// <summary>
        /// Elevation level granted by this tile state. 0 = ground level.
        /// Elevated tiles grant +1 to spell range and bonus LOS height.
        /// Used for raised stone platforms (Geomancer Wall), crystal formations, etc.
        /// </summary>
        [Tooltip("Elevation level of this tile. 0 = ground. 1+ = elevated platform with range bonus.")]
        [Range(0, 3)] public int elevationLevel = 0;

        // -----------------------------------------------------------------------------------------
        // Hazard Effects
        // -----------------------------------------------------------------------------------------

        [Header("Hazard Effects")]

        /// <summary>
        /// Damage dealt to a unit the moment they enter (move onto) this tile.
        /// Used for Burning, ThornyGround, and ToxicGround tiles.
        /// 0 = no entry damage.
        /// </summary>
        [Tooltip("Damage dealt to a unit upon entering this tile.")]
        [Range(0, 20)] public int entryDamage = 0;

        /// <summary>
        /// Damage dealt to all units that end their turn standing on this tile.
        /// Applied during the End-of-Turn phase by TileHazardSystem.
        /// 0 = no per-turn damage.
        /// </summary>
        [Tooltip("Damage dealt to units at end of their turn on this tile.")]
        [Range(0, 20)] public int endOfTurnDamage = 0;

        /// <summary>Element type of the hazard damage. Used to check for elemental interactions and resistances.</summary>
        [Tooltip("Element type of any entry or end-of-turn damage from this tile.")]
        public ElementType hazardElement = ElementType.Arcane;

        /// <summary>
        /// Status effects automatically applied to units when they enter this tile.
        /// Example: Wet tiles apply the Wet status; Burning tiles apply the Burning status.
        /// </summary>
        [Tooltip("Status effects applied to units upon entering this tile.")]
        public StatusEffectApplication[] entryStatusEffects;

        /// <summary>
        /// Status effects automatically applied to units at the end of each turn they remain on this tile.
        /// Example: Poisoned tiles apply a Poison stack each turn the unit stays.
        /// </summary>
        [Tooltip("Status effects applied to units at end-of-turn while they remain on this tile.")]
        public StatusEffectApplication[] endOfTurnStatusEffects;

        // -----------------------------------------------------------------------------------------
        // Duration and Spread
        // -----------------------------------------------------------------------------------------

        [Header("Duration and Spread")]

        /// <summary>
        /// How many turns this tile state naturally persists before reverting to Normal.
        /// 0 = permanent until overwritten by another spell or effect.
        /// Burning typically has 3-5 turns. Wet has 2-3. Frozen is permanent until melted.
        /// </summary>
        [Tooltip("Turns before this tile state naturally reverts to Normal. 0 = permanent.")]
        [Range(0, 10)] public int naturalDuration = 0;

        /// <summary>
        /// Whether this tile state spreads to adjacent tiles each turn.
        /// Burning tiles spread fire. ToxicGround can spread to adjacent Wet tiles.
        /// Spread is checked during the End-of-Turn tick by the TileSpreadSystem.
        /// </summary>
        [Tooltip("If true, this tile state spreads to adjacent tiles each turn based on canSpreadInto rules.")]
        public bool spreads = false;

        /// <summary>
        /// If spreads is true, defines which adjacent tile states can receive this spread.
        /// Must contain TileState enum value names.
        /// Example: Burning can spread to "Normal" and "VineCovered" but not "Wet" or "Frozen".
        /// </summary>
        [Tooltip("TileState enum value names that this state can spread into. Only relevant if spreads=true.")]
        public string[] canSpreadInto;

        /// <summary>
        /// Probability (0-1) that spread occurs to each eligible neighbor per turn.
        /// 1.0 = always spreads to all eligible neighbors. 0.5 = 50% chance per neighbor.
        /// Kept at 1.0 for most states to maintain determinism; randomized spread should be avoided.
        /// </summary>
        [Tooltip("Spread probability per eligible neighbor per turn. Keep at 1.0 for deterministic play.")]
        [Range(0f, 1f)] public float spreadChance = 1.0f;

        /// <summary>
        /// Maximum number of tiles this state can spread to simultaneously in one turn.
        /// 0 = unlimited. Prevents chain reactions from becoming uncontrollably large.
        /// </summary>
        [Tooltip("Maximum simultaneous spread tiles per turn. 0 = no limit.")]
        [Range(0, 8)] public int maxSpreadPerTurn = 0;

        // -----------------------------------------------------------------------------------------
        // Faction Interactions
        // -----------------------------------------------------------------------------------------

        [Header("Faction Interactions")]

        /// <summary>
        /// If true, this tile qualifies as "natural terrain" for the Verdant Pact's Terrain Bond
        /// faction trait. Units gain +1 movement and regeneration when on or adjacent to these tiles.
        /// Natural tiles: Normal (grass/earth), VineCovered, Muddy (natural mud), Frozen (natural ice).
        /// </summary>
        [Tooltip("True if Verdant Pact units gain Terrain Bond bonus on or adjacent to this tile type.")]
        public bool isNaturalTerrain = false;

        /// <summary>
        /// If true, Ashen Covenant Grave Husks and Abyssal Revenants regenerate HP while on this tile.
        /// Necrotic tiles: Poisoned, Corrupted, Burning (per the faction's "necrotic absorption" trait).
        /// </summary>
        [Tooltip("True if Ashen Covenant undead units regenerate HP on this tile (Deathless Ranks trait).")]
        public bool isNecroticTerrain = false;

        /// <summary>
        /// HP regenerated per turn by Ashen Covenant undead on this tile when isNecroticTerrain is true.
        /// </summary>
        [Tooltip("HP per turn regenerated by Ashen Covenant undead on this tile. Ignored if isNecroticTerrain=false.")]
        [Range(0, 10)] public int necroticRegenAmount = 1;

        // -----------------------------------------------------------------------------------------
        // Elemental Properties
        // -----------------------------------------------------------------------------------------

        [Header("Elemental Properties")]

        /// <summary>
        /// The element type associated with this tile state, used as the "existing state" key in
        /// ElementResolver's interaction table. Example: Burning = Fire, Wet = Water, Frozen = Ice.
        /// Spells interacting with this tile look up [this element, incoming spell element] in the matrix.
        /// </summary>
        [Tooltip("Element type of this tile state. Used as the existing-state key in ElementResolver.")]
        public ElementType associatedElement = ElementType.Arcane;

        /// <summary>
        /// Whether this tile conducts electricity. Wet, Charged, and water tiles conduct.
        /// Conducting tiles allow Lightning spells to chain from unit to unit across their surface.
        /// </summary>
        [Tooltip("True if Lightning spells chain across this tile to adjacent units (Electromancer synergy).")]
        public bool conductsLightning = false;

        /// <summary>
        /// Whether units standing on this tile are considered "Wet" for Lightning chain purposes
        /// even if they don't have the Wet status explicitly.
        /// Used for tiles like Flooded tiles where it's implied the unit is standing in water.
        /// </summary>
        [Tooltip("True if units on this tile are treated as Wet for Lightning chain calculations.")]
        public bool treatsUnitsAsWet = false;

        // -----------------------------------------------------------------------------------------
        // Presentation
        // -----------------------------------------------------------------------------------------

        [Header("Presentation")]

        /// <summary>
        /// Material applied to tile meshes when they enter this state.
        /// Swapped by TileViewController when the simulation emits a TileStateChanged event.
        /// </summary>
        [Tooltip("URP Lit material applied to the tile mesh in this state.")]
        public Material tileMaterial;

        /// <summary>
        /// Optional mesh variant for this tile state. If assigned, TileViewController swaps the tile mesh
        /// in addition to the material (e.g., stone wall mesh for an Elevated tile, rubble mesh for Rubble).
        /// </summary>
        [Tooltip("Optional mesh to replace the default tile mesh in this state. Leave null to keep the base mesh.")]
        public Mesh tileMeshVariant;

        /// <summary>
        /// Particle effect overlay prefab instantiated above tiles in this state.
        /// Examples: fire particles (Burning), ice crystal growth (Frozen), green miasma (Poisoned).
        /// </summary>
        [Tooltip("VFX prefab overlaid on tiles in this state (fire, ice, poison cloud, etc.).")]
        public GameObject tileOverlayPrefab;

        /// <summary>Tag used by VFXDirector to play the tile-state-change transition effect.</summary>
        [Tooltip("VFXDirector tag for the tile state transition burst effect (e.g., 'tile_freeze', 'tile_ignite').")]
        public string vfxTransitionTag;

        /// <summary>FMOD event for the ambient loop played while this tile state is active.</summary>
        [Tooltip("FMOD event for tile ambient loop (fire crackle, water drip, wind howl, etc.).")]
        public string ambientSfxTag;

        /// <summary>FMOD event played once when this tile state is applied.</summary>
        [Tooltip("FMOD event for the one-shot sound played when this tile state first activates.")]
        public string onApplySfxTag;

        /// <summary>Tint color applied to the tile highlight overlay in the tactical view.</summary>
        [Tooltip("Color tint of the tile state indicator overlay shown during targeting and turn review.")]
        public Color tileOverlayTintColor = Color.white;

        [Header("Design Notes")]

        /// <summary>Internal design notes for team use. Not shown to players.</summary>
        [TextArea(2, 4)] public string designNotes;
    }
}

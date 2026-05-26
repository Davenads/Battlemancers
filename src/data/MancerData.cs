using UnityEngine;

namespace Battlemancers.Data
{
    /// <summary>
    /// Static definition for a single Mancer archetype. Stored as a Unity ScriptableObject asset.
    /// One MancerData asset exists per archetype (19 total). Designers populate all fields in the
    /// Inspector; no code changes are needed to tune Mancer stats or swap spells.
    ///
    /// MancerData holds only immutable definition data. Runtime state (current HP, active cooldowns,
    /// applied status effects, current position) belongs in the simulation's UnitState class, never here.
    /// Mutating a ScriptableObject during play creates state leakage between sessions.
    ///
    /// Base warband cost for all Mancers is 100 pts. Upgrades add to this. Activation cost is
    /// always 100 pts regardless of upgrades purchased.
    /// </summary>
    [CreateAssetMenu(fileName = "New Mancer", menuName = "Battlemancers/Mancer Data")]
    public class MancerData : ScriptableObject
    {
        // -----------------------------------------------------------------------------------------
        // Identity
        // -----------------------------------------------------------------------------------------

        [Header("Identity")]

        /// <summary>
        /// Unique identifier for this archetype. Lowercase, no spaces.
        /// Convention: the archetype name (e.g., "pyromancer", "hydromancer", "cryomancer").
        /// Used as the key in save files (MancerLoadout.mancerArchetypeId).
        /// </summary>
        public string mancerId;

        /// <summary>Player-visible name (e.g., "Pyromancer", "Hydromancer").</summary>
        public string displayName;

        /// <summary>
        /// Short tactical summary displayed in the warband builder.
        /// Describes the Mancer's role and primary mechanics (2-4 sentences).
        /// Example: "DoT specialist. Spreads burning terrain that pressures enemy positioning.
        /// Pairs with Hydromancer for steam combos or Electromancer for arc explosions."
        /// </summary>
        [TextArea(3, 6)] public string tacticalIdentity;

        /// <summary>
        /// The Mancer's primary element. Used as a default element type where unspecified
        /// and for faction UI theming (color coding, portrait border).
        /// </summary>
        public ElementType primaryElement;

        /// <summary>
        /// Optional secondary element for Mancers with cross-element kits.
        /// Example: Thermomancer has Fire as primary and Ice as secondary.
        /// Leave as Arcane if the Mancer only operates in one element.
        /// </summary>
        [Tooltip("Optional secondary element for dual-element Mancers. Set to Arcane if not applicable.")]
        public ElementType secondaryElement = ElementType.Arcane;

        // -----------------------------------------------------------------------------------------
        // Base Stats
        // -----------------------------------------------------------------------------------------

        [Header("Base Stats")]

        /// <summary>
        /// Maximum hit points at base (no upgrades). Typical range: 80-130.
        /// Melee-oriented Mancers (Osteomancer, Geomancer): 110-130.
        /// Mid-range casters (Pyromancer, Hydromancer): 90-100.
        /// Fragile specialists (Chronomancer, Echomancer): 70-85.
        /// </summary>
        [Tooltip("Base maximum HP. Upgrades may increase this via StatEnhancement.")]
        [Range(60, 150)] public int maxHP = 100;

        /// <summary>
        /// Number of tiles this Mancer can move per activation. 1 tile costs 1 AP.
        /// With 6 AP base, a Mancer with moveRange 3 can move 3 tiles and still cast a Standard spell.
        /// Typical range: 2-5. Mobile Mancers (Aeromancer): 5. Slow heavies (Osteomancer): 2.
        /// </summary>
        [Tooltip("Tiles of movement per activation. Each tile costs 1 AP from the 6 AP pool.")]
        [Range(1, 6)] public int moveRange = 3;

        /// <summary>
        /// Flat damage reduction applied to each incoming hit before HP loss.
        /// Most Mancers have low armor (0-2). Osteomancer with Bone Armor stacks more via ability.
        /// </summary>
        [Tooltip("Base flat damage reduction per hit.")]
        [Range(0, 8)] public int baseArmor = 1;

        /// <summary>
        /// Base action points per turn. 6 for all Mancers by default.
        /// Only changed by StatEnhancement upgrades or in-game effects (Haste, Slow, Time_Stop).
        /// </summary>
        [Tooltip("Base AP per turn. Do not change from 6 unless intentionally designing a special archetype.")]
        [Range(4, 8)] public int baseActionPoints = 6;

        /// <summary>
        /// Warband point cost at base (no upgrades). Always 100 for all Mancers.
        /// Upgrades add their additionalPointCost on top. Activation cost never changes.
        /// </summary>
        [Tooltip("Base warband point cost. Should always be 100. Upgrades add to this separately.")]
        public int baseCost = 100;

        // -----------------------------------------------------------------------------------------
        // Spells
        // -----------------------------------------------------------------------------------------

        [Header("Spells")]

        /// <summary>
        /// The Mancer's default spell loadout. All spells here are available at base with no upgrades.
        /// Design target: 2 Quick, 2 Standard, 1 Heavy or Ultimate spell = 5 spells total.
        /// SpellVariant upgrades replace entries from this array (matched by spellId).
        /// SignatureAbility upgrades add a 6th spell slot.
        /// </summary>
        [Tooltip("Base spell kit. Target: 2 Quick, 2 Standard, 1 Heavy/Ultimate. Upgrades may replace or add.")]
        public SpellData[] baseSpells;

        // -----------------------------------------------------------------------------------------
        // Upgrades
        // -----------------------------------------------------------------------------------------

        [Header("Upgrades")]

        /// <summary>
        /// All upgrade options available to this Mancer archetype in the warband builder.
        /// A player may purchase any number of these (subject to mutual exclusivity and budget).
        /// </summary>
        [Tooltip("All available upgrade options for this Mancer archetype.")]
        public UpgradeOption[] availableUpgrades;

        // -----------------------------------------------------------------------------------------
        // Secondary Resources
        // -----------------------------------------------------------------------------------------

        [Header("Secondary Resource (Optional)")]

        /// <summary>
        /// Whether this Mancer uses a secondary resource (e.g., Soul Energy for Necromancer,
        /// Bone Shards for Osteomancer). False for most Mancers; the base game does not require this.
        /// </summary>
        [Tooltip("Whether this Mancer has a secondary resource system beyond AP.")]
        public bool hasSecondaryResource = false;

        /// <summary>
        /// Display name of the secondary resource (e.g., "Soul Energy", "Bone Shards", "Temporal Charge").
        /// Only shown in UI if hasSecondaryResource is true.
        /// </summary>
        [Tooltip("Name of the secondary resource shown in the HUD. Only used if hasSecondaryResource is true.")]
        public string secondaryResourceName;

        /// <summary>Maximum value of the secondary resource pool.</summary>
        [Tooltip("Maximum secondary resource value. Only used if hasSecondaryResource is true.")]
        [Range(0, 20)] public int maxSecondaryResource = 0;

        /// <summary>
        /// String identifier used by the ResourceManager to find the rule for how this resource generates.
        /// Example: "necromancer_soul_on_kill" resolves to a registered resource generation rule.
        /// </summary>
        [Tooltip("Resource generation rule identifier. Must match a registered rule in ResourceManager.")]
        public string secondaryResourceGenerationRuleId;

        // -----------------------------------------------------------------------------------------
        // Faction Interactions
        // -----------------------------------------------------------------------------------------

        [Header("Faction Interactions")]

        /// <summary>
        /// Mancer archetypes that this Mancer has strong synergy with.
        /// Used by the warband builder to display "Synergizes well with" recommendations.
        /// Informational only — no mechanical effect.
        /// </summary>
        [Tooltip("Mancer IDs this archetype synergizes strongly with. Used for warband builder recommendations.")]
        public string[] strongSynergyMancerIds;

        /// <summary>
        /// Faction ID this Mancer benefits most from (for warband builder recommendations).
        /// Informational only — any Mancer can serve any faction mechanically.
        /// </summary>
        [Tooltip("Faction ID this Mancer pairs especially well with. Informational only.")]
        public string recommendedFactionId;

        // -----------------------------------------------------------------------------------------
        // Presentation
        // -----------------------------------------------------------------------------------------

        [Header("Presentation")]

        /// <summary>Portrait sprite displayed in the warband builder and in-game HUD unit card.</summary>
        public Sprite portrait;

        /// <summary>Full-body art shown on the faction selection screen and ability cinematics.</summary>
        public Sprite fullBodyArt;

        /// <summary>Animator controller driving this Mancer's sprite animations on the battlefield.</summary>
        public RuntimeAnimatorController animatorController;

        /// <summary>
        /// VFX tag used by VFXDirector to look up this Mancer's ambient and ability-agnostic particle effects.
        /// Spell-specific VFX are stored on each SpellData asset.
        /// </summary>
        [Tooltip("Tag for Mancer-specific VFX that are not spell-specific (idle glow, death dissolve color).")]
        public string vfxTag;

        /// <summary>Shader tint color applied to the Mancer sprite's elemental glow effect.</summary>
        public Color elementalGlowColor = Color.white;

        // -----------------------------------------------------------------------------------------
        // Audio
        // -----------------------------------------------------------------------------------------

        [Header("Audio")]

        /// <summary>
        /// FMOD event prefix for this Mancer's voice lines.
        /// Convention: "unit/{mancerId}/voice" — suffixed by action type by AudioDirector.
        /// Example: "unit/pyromancer/voice" → AudioDirector appends "/cast", "/hit", "/death", "/idle".
        /// </summary>
        [Tooltip("FMOD event prefix for voice lines. AudioDirector appends action suffixes.")]
        public string voiceTag;

        /// <summary>
        /// FMOD event for this Mancer's idle ambient loop (optional elemental hum or atmospheric sound).
        /// Played at low volume while the unit is on the battlefield and not acting.
        /// </summary>
        [Tooltip("FMOD event for ambient idle sound. Leave empty if this Mancer has no ambient loop.")]
        public string ambientIdleSfxTag;

        // -----------------------------------------------------------------------------------------
        // Design Notes
        // -----------------------------------------------------------------------------------------

        [Header("Design Notes")]

        /// <summary>
        /// Internal design notes for the dev team. Not shown to players anywhere.
        /// Use for: intended combo targets, open balance questions, art status, known issues.
        /// </summary>
        [TextArea(2, 5)] public string designNotes;
    }
}

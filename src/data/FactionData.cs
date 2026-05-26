using UnityEngine;

namespace Battlemancers.Data
{
    /// <summary>
    /// Static definition for a single player faction. Stored as a Unity ScriptableObject asset.
    /// Three FactionData assets exist: The Gilded Throne, The Verdant Pact, The Ashen Covenant.
    ///
    /// Faction determines:
    ///   - Which Chaff and Ranged unit types (T1 and T2) are available
    ///   - The faction-wide passive trait applied to all non-Mancer units
    ///   - Aesthetic identity (color, banner, UI theme)
    ///
    /// Mancers are faction-agnostic — any of the 19 Mancer archetypes can serve any faction.
    /// FactionData is immutable configuration; all runtime state lives in the simulation layer.
    /// </summary>
    [CreateAssetMenu(fileName = "New Faction", menuName = "Battlemancers/Faction Data")]
    public class FactionData : ScriptableObject
    {
        // -----------------------------------------------------------------------------------------
        // Identity
        // -----------------------------------------------------------------------------------------

        [Header("Identity")]

        /// <summary>
        /// Unique identifier. Convention: lowercase, underscored.
        /// Values: "gilded_throne", "verdant_pact", "ashen_covenant".
        /// Used as the key in WarbandSave.factionId.
        /// </summary>
        public string factionId;

        /// <summary>Player-visible faction name (e.g., "The Gilded Throne").</summary>
        public string displayName;

        /// <summary>Faction motto or tagline shown on the faction select screen.</summary>
        public string tagline;

        /// <summary>Lore description shown on the faction selection screen (2-4 sentences).</summary>
        [TextArea(2, 4)] public string lore;

        // -----------------------------------------------------------------------------------------
        // Faction Trait
        // -----------------------------------------------------------------------------------------

        [Header("Faction Trait")]

        /// <summary>Display name of the faction-wide passive trait (e.g., "Iron Discipline").</summary>
        public string factionTraitName;

        /// <summary>
        /// Player-visible description of the faction trait shown in the warband builder.
        /// Should be concrete and specific about what the trait does mechanically.
        /// </summary>
        [TextArea(2, 4)] public string factionTraitDescription;

        /// <summary>
        /// String IDs of passive traits granted to all non-Mancer units in this faction's warband.
        /// Resolved at runtime by the PassiveTraitRegistry. Mancers are not affected by faction traits.
        /// Example for Gilded Throne: ["immune_panic", "immune_charm", "morale_debuff_resist"].
        /// Example for Verdant Pact: ["terrain_bond_movement", "terrain_bond_regen"].
        /// Example for Ashen Covenant: ["immune_panic", "immune_charm", "immune_flee", "death_remnant_token"].
        /// </summary>
        [Tooltip("PassiveTrait IDs applied to all non-Mancer units in this faction. Resolved by PassiveTraitRegistry.")]
        public string[] factionPassiveTraitIds;

        // -----------------------------------------------------------------------------------------
        // Support Units
        // -----------------------------------------------------------------------------------------

        [Header("Support Units")]

        /// <summary>
        /// The faction's Tier 1 frontline melee unit.
        /// Point cost: 10. Activation cost: 10.
        /// Examples: Conscript Spearmen (Gilded Throne), Thornback Sentinels (Verdant Pact), Grave Husks (Ashen Covenant).
        /// </summary>
        [Tooltip("Faction's T1 chaff unit. Point cost and activation cost: 10 pts.")]
        public SupportUnitData t1ChaffUnit;

        /// <summary>
        /// The faction's Tier 2 veteran frontline melee unit.
        /// Point cost: 20. Activation cost: 20. Five T2 Chaff fill a full 100-pt activation slot.
        /// Examples: Iron Vanguard (Gilded Throne), Rootwarden (Verdant Pact), Abyssal Revenant (Ashen Covenant).
        /// </summary>
        [Tooltip("Faction's T2 veteran chaff unit. Point cost and activation cost: 20 pts.")]
        public SupportUnitData t2ChaffUnit;

        /// <summary>
        /// The faction's Tier 1 ranged unit.
        /// Point cost: 25. Activation cost: 25. Four T1 Ranged fill a full 100-pt activation slot.
        /// Examples: Crossbow Corps (Gilded Throne), Glade Archers (Verdant Pact), Wailing Shades (Ashen Covenant).
        /// </summary>
        [Tooltip("Faction's T1 ranged unit. Point cost and activation cost: 25 pts.")]
        public SupportUnitData t1RangedUnit;

        /// <summary>
        /// The faction's Tier 2 veteran ranged unit.
        /// Point cost: 50. Activation cost: 50. Two T2 Ranged fill a full 100-pt activation slot.
        /// Examples: Siege Arbalest (Gilded Throne), Wyrmwood Strider (Verdant Pact), Void Wraith (Ashen Covenant).
        /// </summary>
        [Tooltip("Faction's T2 veteran ranged unit. Point cost and activation cost: 50 pts.")]
        public SupportUnitData t2RangedUnit;

        // -----------------------------------------------------------------------------------------
        // Warband Building Guidance
        // -----------------------------------------------------------------------------------------

        [Header("Warband Builder Guidance")]

        /// <summary>
        /// Mancer archetype IDs that the warband builder highlights as strong pairings with this faction.
        /// Informational only — no mechanical restriction. Used for the "Recommended Mancers" panel.
        /// </summary>
        [Tooltip("Mancer IDs recommended for this faction in the warband builder. Informational only.")]
        public string[] recommendedMancerIds;

        /// <summary>
        /// Short playstyle guidance shown in the warband builder for new players choosing a faction.
        /// Example: "Versatile. No elemental weakness. Best for players who want to focus on Mancer combos."
        /// </summary>
        [TextArea(1, 3)] public string playstyleGuidance;

        /// <summary>Difficulty rating for new players. 1 = most accessible, 3 = most complex.</summary>
        [Tooltip("Suggested difficulty for new players. 1 = easiest, 3 = most complex.")]
        [Range(1, 3)] public int newPlayerDifficulty = 2;

        // -----------------------------------------------------------------------------------------
        // Presentation
        // -----------------------------------------------------------------------------------------

        [Header("Presentation")]

        /// <summary>Primary faction color used for UI accents, portrait borders, and banner tints.</summary>
        public Color factionColor = Color.white;

        /// <summary>Secondary faction color used for highlights and gradient fills.</summary>
        public Color factionSecondaryColor = Color.grey;

        /// <summary>Faction banner sprite displayed on the faction selection screen and match HUD.</summary>
        public Sprite factionBanner;

        /// <summary>Small faction crest icon used in the warband builder list and unit cards.</summary>
        public Sprite factionCrest;

        /// <summary>
        /// UI theme tag used by the UIThemeManager to apply the correct color scheme
        /// to the warband builder and in-game HUD when this faction is selected.
        /// </summary>
        [Tooltip("UI theme identifier applied to all UI elements when this faction is active.")]
        public string uiThemeTag;

        // -----------------------------------------------------------------------------------------
        // Audio
        // -----------------------------------------------------------------------------------------

        [Header("Audio")]

        /// <summary>
        /// FMOD event for the faction's musical theme, played during faction select and warband building.
        /// </summary>
        [Tooltip("FMOD event for the faction selection screen music theme.")]
        public string factionThemeSfxTag;

        /// <summary>FMOD event for the faction's match-start rally cry audio.</summary>
        [Tooltip("FMOD event for the audio played at match start for this faction.")]
        public string matchStartSfxTag;

        [Header("Design Notes")]

        /// <summary>Internal design notes for team use. Not shown to players.</summary>
        [TextArea(2, 4)] public string designNotes;
    }

    // =============================================================================================
    // Supporting Serializable Classes
    // =============================================================================================

    /// <summary>
    /// Complete definition for a support unit (Chaff or Ranged, T1 or T2).
    /// Serialized inline within FactionData as a nested field rather than a separate ScriptableObject,
    /// since support units are always authored as part of their faction and never referenced cross-faction.
    ///
    /// Point cost rules:
    ///   T1 Chaff = 10 pts (activation cost = 10)
    ///   T2 Chaff = 20 pts (activation cost = 20)
    ///   T1 Ranged = 25 pts (activation cost = 25)
    ///   T2 Ranged = 50 pts (activation cost = 50)
    ///
    /// Activation cost always equals point cost for support units (unlike Mancers).
    /// </summary>
    [System.Serializable]
    public class SupportUnitData
    {
        /// <summary>
        /// Unique identifier for this unit type.
        /// Convention: "{faction_id}_{unit_role}_{tier}" e.g., "gilded_throne_chaff_t1", "verdant_pact_ranged_t2".
        /// </summary>
        public string unitId;

        /// <summary>Player-visible unit name (e.g., "Conscript Spearmen", "Iron Vanguard").</summary>
        public string displayName;

        /// <summary>
        /// Warband point cost and activation cost. These are always equal for support units.
        /// T1 Chaff = 10, T2 Chaff = 20, T1 Ranged = 25, T2 Ranged = 50.
        /// </summary>
        [Tooltip("Point cost to include in warband and cost to activate per turn. Always equal for support units.")]
        [Range(10, 50)] public int pointCost;

        /// <summary>Maximum hit points.</summary>
        [Tooltip("Maximum HP for this unit type.")]
        [Range(20, 120)] public int maxHP;

        /// <summary>Number of tiles this unit can move per activation.</summary>
        [Tooltip("Move range in tiles per activation.")]
        [Range(1, 5)] public int moveRange;

        /// <summary>Flat damage reduction per incoming hit.</summary>
        [Tooltip("Base flat armor damage reduction.")]
        [Range(0, 6)] public int baseArmor;

        /// <summary>Base melee damage dealt per attack.</summary>
        [Tooltip("Base melee attack damage.")]
        [Range(0, 25)] public int meleeDamage;

        /// <summary>
        /// Base ranged attack damage. 0 if this unit is melee-only.
        /// Ranged damage is listed for Ranged unit types; 0 for Chaff.
        /// </summary>
        [Tooltip("Base ranged attack damage. 0 for melee-only units.")]
        [Range(0, 30)] public int rangedDamage;

        /// <summary>
        /// Maximum range in tiles for ranged attacks. 0 if this unit is melee-only.
        /// </summary>
        [Tooltip("Range in tiles for ranged attacks. 0 for melee-only units.")]
        [Range(0, 10)] public int rangedAttackRange;

        /// <summary>
        /// Element type of this unit's basic attack damage.
        /// Most support units deal Arcane (no elemental interaction). Some exceptions:
        /// Glade Archers deal Poison, Wailing Shades deal Necrotic.
        /// </summary>
        [Tooltip("Element type of basic attack damage. Arcane = no elemental state interactions.")]
        public ElementType attackElement;

        /// <summary>
        /// Human-readable description of this unit's special ability for the warband builder tooltip.
        /// Not a code-driven field — actual ability logic is registered separately in the passive/ability system.
        /// </summary>
        [Tooltip("Description of the unit's special ability shown in the warband builder. Informational.")]
        [TextArea(1, 4)] public string specialAbilityDescription;

        /// <summary>
        /// String ID of the special ability registered in the AbilityRegistry.
        /// Example: "spear_reach_attack", "shield_wall_aura", "glade_archer_poison_on_hit",
        /// "grave_husk_cursed_on_death", "wailing_shade_silence_aura".
        /// </summary>
        [Tooltip("Ability ID registered in AbilityRegistry. Used by simulation to execute this unit's special.")]
        public string specialAbilityId;

        /// <summary>
        /// Additional passive trait IDs specific to this unit type (beyond the faction-wide traits).
        /// Resolved by PassiveTraitRegistry at runtime.
        /// </summary>
        [Tooltip("Unit-specific passive trait IDs, beyond faction-wide traits.")]
        public string[] unitPassiveTraitIds;

        /// <summary>
        /// Whether this unit type has a "Tier 1 prerequisite" unit it upgrades from.
        /// True for T2 units; false for T1. Used by the warband builder for upgrade indication.
        /// </summary>
        [Tooltip("True if this unit is a T2 veteran variant of a T1 unit.")]
        public bool isTierTwo = false;

        /// <summary>
        /// Unit ID of the T1 counterpart this unit upgrades from.
        /// Only relevant when isTierTwo is true.
        /// </summary>
        [Tooltip("Unit ID of the T1 unit this T2 unit is the veteran version of. Leave empty for T1 units.")]
        public string upgradesFromUnitId;

        /// <summary>
        /// VFX tag used by VFXDirector for unit-specific effects (death, attack impact, special ability).
        /// </summary>
        [Tooltip("VFX tag for unit-specific visual effects.")]
        public string vfxTag;

        /// <summary>Animator controller driving this unit's sprite animations on the battlefield.</summary>
        public RuntimeAnimatorController animatorController;

        /// <summary>Portrait sprite used in the unit card and warband builder list entry.</summary>
        public Sprite portrait;
    }
}

using UnityEngine;

namespace Battlemancers.Data
{
    /// <summary>
    /// Static configuration for a single Mancer upgrade option. Stored as a Unity ScriptableObject
    /// so designers can author upgrade trees in the Inspector and reference SpellData assets directly.
    ///
    /// Upgrades are purchased during warband construction, increasing a Mancer's warband point cost
    /// above the 100-pt base. The Mancer's activation cost remains fixed at 100 pts regardless of
    /// how many upgrades are purchased.
    ///
    /// Cost ranges by category:
    ///   SpellVariant:     +15-25 pts
    ///   PassiveTrait:     +20-30 pts
    ///   StatEnhancement:  +10-20 pts
    ///   SignatureAbility: +25-50 pts
    /// </summary>
    [CreateAssetMenu(fileName = "New Upgrade", menuName = "Battlemancers/Upgrade Option")]
    public class UpgradeOption : ScriptableObject
    {
        // -----------------------------------------------------------------------------------------
        // Identity
        // -----------------------------------------------------------------------------------------

        [Header("Identity")]

        /// <summary>
        /// Unique identifier for this upgrade. Convention: "{mancertype}_{upgrade_description}".
        /// Example: "pyromancer_wildfire_spread", "electromancer_chain_master".
        /// </summary>
        public string upgradeId;

        /// <summary>Player-visible upgrade name shown in the warband builder.</summary>
        public string displayName;

        /// <summary>Which upgrade category this option belongs to. Determines which sub-fields are relevant.</summary>
        public UpgradeCategory category;

        /// <summary>Description shown in the warband builder tooltip explaining what this upgrade does.</summary>
        [TextArea(2, 4)] public string description;

        // -----------------------------------------------------------------------------------------
        // Cost
        // -----------------------------------------------------------------------------------------

        [Header("Cost")]

        /// <summary>
        /// Additional points added to the Mancer's base 100-pt warband cost.
        /// The Mancer still activates for 100 pts regardless of this value.
        /// SpellVariant: 15-25. PassiveTrait: 20-30. StatEnhancement: 10-20. SignatureAbility: 25-50.
        /// </summary>
        [Tooltip("Points added to this Mancer's warband cost. Activation cost stays fixed at 100 pts.")]
        [Range(10, 60)] public int additionalPointCost = 20;

        // -----------------------------------------------------------------------------------------
        // Spell Variant fields
        // -----------------------------------------------------------------------------------------

        [Header("Spell Variant — only populated if Category == SpellVariant")]

        /// <summary>
        /// The spell from the Mancer's base kit that this upgrade replaces.
        /// Must reference a SpellData in this Mancer's MancerData.baseSpells array.
        /// Null if this is not a SpellVariant upgrade.
        /// </summary>
        [Tooltip("Base-kit spell that is replaced by this upgrade. Leave null for non-SpellVariant categories.")]
        public SpellData replacesSpell;

        /// <summary>
        /// The upgraded version of the spell that replaces replacesSpell.
        /// This SpellData asset should be a tuned variant (e.g., wider AoE, additional terrain effect).
        /// </summary>
        [Tooltip("The upgraded spell that replaces the base version. Leave null for non-SpellVariant categories.")]
        public SpellData replacementSpell;

        // -----------------------------------------------------------------------------------------
        // Passive Trait fields
        // -----------------------------------------------------------------------------------------

        [Header("Passive Trait — only populated if Category == PassiveTrait")]

        /// <summary>
        /// String identifier looked up by the PassiveTrait system at runtime.
        /// Must match a value registered in the PassiveTraitRegistry.
        /// Example: "pyromancer_heat_aura", "cryomancer_cold_body".
        /// </summary>
        [Tooltip("PassiveTrait system identifier. Must match a registered trait ID.")]
        public string passiveTraitId;

        /// <summary>One-line effect summary for warband builder display. E.g., "+1 Burning duration on all spells".</summary>
        [TextArea(1, 3)] public string passiveEffectSummary;

        // -----------------------------------------------------------------------------------------
        // Stat Enhancement fields
        // -----------------------------------------------------------------------------------------

        [Header("Stat Enhancement — only populated if Category == StatEnhancement")]

        /// <summary>Which core stat this upgrade improves.</summary>
        [Tooltip("Stat to enhance. Only relevant for StatEnhancement category.")]
        public StatType statToEnhance;

        /// <summary>
        /// The flat value added to the chosen stat.
        /// For MaxHP: typically 20-40. For MoveRange: typically 1.
        /// For SpellRange: typically 1-2. For BaseArmor: typically 1-2.
        /// For ActionPoints: typically 1 (adds 1 AP per turn — rare, expensive upgrade).
        /// </summary>
        [Tooltip("Amount added to the chosen stat.")]
        [Range(1, 50)] public int statBonus = 10;

        // -----------------------------------------------------------------------------------------
        // Signature Ability fields
        // -----------------------------------------------------------------------------------------

        [Header("Signature Ability — only populated if Category == SignatureAbility")]

        /// <summary>
        /// An entirely new spell added to the Mancer's kit by this upgrade.
        /// This is the Mancer's "ultimate" — a powerful, high-cost ability not in the base loadout.
        /// The spell is added as an additional slot; no base spell is replaced.
        /// </summary>
        [Tooltip("New spell unlocked by this upgrade. Added as an extra spell slot. Leave null for other categories.")]
        public SpellData signatureAbility;

        // -----------------------------------------------------------------------------------------
        // Mutual Exclusivity
        // -----------------------------------------------------------------------------------------

        [Header("Mutual Exclusivity")]

        /// <summary>
        /// Upgrade IDs that cannot be purchased alongside this one.
        /// Used to enforce "pick one of two" upgrade trees (e.g., two different Signature Abilities).
        /// </summary>
        [Tooltip("Other upgrade IDs that are incompatible with this one. Player can only take one.")]
        public string[] mutuallyExclusiveWith;

        // -----------------------------------------------------------------------------------------
        // Presentation
        // -----------------------------------------------------------------------------------------

        [Header("Presentation")]

        /// <summary>Icon displayed next to this upgrade in the warband builder list.</summary>
        public Sprite upgradeIcon;

        /// <summary>Color used for the upgrade category badge in the warband builder UI.</summary>
        public Color categoryBadgeColor = Color.white;

        [Header("Design Notes")]

        /// <summary>Internal design notes for team use. Not shown to players.</summary>
        [TextArea(2, 4)] public string designNotes;
    }

    // =============================================================================================
    // Enums
    // =============================================================================================

    /// <summary>
    /// The four categories of Mancer upgrades available during warband construction.
    /// Each category has its own cost range and gameplay implication.
    /// </summary>
    public enum UpgradeCategory
    {
        /// <summary>
        /// Replaces a base-kit spell with a more powerful or situationally different version.
        /// The replaced spell is removed; only the replacement is available.
        /// Cost: +15-25 pts.
        /// </summary>
        SpellVariant,

        /// <summary>
        /// Adds a new passive ability to the Mancer (e.g., terrain interaction, status resistance, aura).
        /// Passive abilities persist the entire match without any activation cost.
        /// Cost: +20-30 pts.
        /// </summary>
        PassiveTrait,

        /// <summary>
        /// Improves a core stat (HP, move range, spell range, armor, or AP).
        /// The simplest upgrade type — direct numerical improvement.
        /// Cost: +10-20 pts.
        /// </summary>
        StatEnhancement,

        /// <summary>
        /// Unlocks a powerful, unique ability that is not available in the base kit.
        /// Typically the Mancer's "ultimate" — transformative in scope; high AP and cooldown cost.
        /// Cost: +25-50 pts.
        /// </summary>
        SignatureAbility
    }

    /// <summary>Core stats that can be improved by a StatEnhancement upgrade.</summary>
    public enum StatType
    {
        /// <summary>Maximum hit points. Flat increase.</summary>
        MaxHP,

        /// <summary>Number of tiles the Mancer can move per activation.</summary>
        MoveRange,

        /// <summary>Maximum range in tiles for all this Mancer's spells.</summary>
        SpellRange,

        /// <summary>Base damage reduction per incoming hit (flat armor).</summary>
        BaseArmor,

        /// <summary>
        /// Action points available per turn. Normally 6 for all Mancers.
        /// A +1 AP upgrade is rare and expensive; allows an additional Quick spell per turn.
        /// </summary>
        ActionPoints,

        /// <summary>
        /// Initiative priority within Mancer resolution. Higher value = resolves before other Mancers.
        /// Used to guarantee a Mancer acts before a specific opponent Mancer.
        /// </summary>
        Initiative
    }
}

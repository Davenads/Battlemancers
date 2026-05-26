namespace Battlemancers.Core.Data
{
    public class MancerRuntimeData
    {
        public string MancerId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string TacticalIdentity { get; set; } = "";
        public string PrimaryElement { get; set; } = "";    // must match ElementType enum names
        public string SecondaryElement { get; set; } = "";  // must match ElementType enum names; empty if none
        public int MaxHP { get; set; }
        public int MoveRange { get; set; }
        public int BaseArmor { get; set; }
        public int BaseActionPoints { get; set; }
        public int BaseCost { get; set; }
        public bool HasSecondaryResource { get; set; }
        public string SecondaryResourceName { get; set; } = "";
        public int MaxSecondaryResource { get; set; }
        public string SecondaryResourceGenerationRuleId { get; set; } = "";
        public string[] StrongSynergyMancerIds { get; set; } = System.Array.Empty<string>();
        public string RecommendedFactionId { get; set; } = "";
        public SpellRuntimeData[] Spells { get; set; } = System.Array.Empty<SpellRuntimeData>();
    }
}

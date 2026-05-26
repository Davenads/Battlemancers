namespace Battlemancers.Core.Data
{
    public class SpellRuntimeData
    {
        public string SpellId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Description { get; set; } = "";
        public int ApCost { get; set; }
        public int CooldownTurns { get; set; }
        public string Tier { get; set; } = "";          // "Quick", "Standard", "Heavy", "Ultimate"
        public string TargetType { get; set; } = "";    // matches SpellTargetType enum names
        public int Range { get; set; }
        public int AoeRadius { get; set; }
        public int LineLength { get; set; }
        public bool RequiresLineOfSight { get; set; }
        public bool CanTargetEmptyTile { get; set; }
        public string Element { get; set; } = "";       // matches ElementType enum names
        public int BaseDamage { get; set; }
        public ConditionalDamageBonusData[] ConditionalBonuses { get; set; } = System.Array.Empty<ConditionalDamageBonusData>();
        public StatusEffectApplicationData[] AppliedEffects { get; set; } = System.Array.Empty<StatusEffectApplicationData>();
        public TerrainChangeApplicationData[] TerrainChanges { get; set; } = System.Array.Empty<TerrainChangeApplicationData>();
        public DisplacementEffectData Displacement { get; set; }
        public string SummonUnitTag { get; set; } = "";
        public int MaxSummonCount { get; set; }
        public int TemperatureDelta { get; set; } // positive = heat, negative = cool
        public string VfxTag { get; set; } = "";
        public string SfxCastTag { get; set; } = "";
        public string SfxImpactTag { get; set; } = "";
        public float CameraShakeIntensity { get; set; }
        public string DesignNotes { get; set; } = "";
    }

    public class ConditionalDamageBonusData
    {
        public string ConditionDescription { get; set; } = "";
        public string TriggerState { get; set; } = "";  // must match StatusType/TerrainState enum names
        public int BonusDamage { get; set; }
        public bool IsMultiplicative { get; set; }
    }

    public class StatusEffectApplicationData
    {
        public string StatusType { get; set; } = "";    // must match StatusType enum names
        public int Duration { get; set; }
        public int StacksApplied { get; set; }
        public bool AppliesToTile { get; set; }
        public float ApplicationChance { get; set; }
    }

    public class TerrainChangeApplicationData
    {
        public string TargetTileState { get; set; } = "";  // must match TileState enum names
        public string ChangeTarget { get; set; } = "";     // matches TerrainChangeTarget enum names
        public int Duration { get; set; }
        public bool OverwriteExistingState { get; set; }
    }

    public class DisplacementEffectData
    {
        public bool HasDisplacement { get; set; }
        public int Tiles { get; set; }
        public bool AffectsAllInArea { get; set; }
        public int CollisionDamagePerTile { get; set; }
    }
}

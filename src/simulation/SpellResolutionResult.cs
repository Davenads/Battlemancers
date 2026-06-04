using System.Collections.Generic;

namespace Battlemancers.Simulation
{
    /// <summary>
    /// Immutable snapshot of every effect produced by a single spell cast, returned by
    /// <see cref="SpellEffectApplicator"/>.
    ///
    /// Unlike <see cref="SpellResult"/> (which is tied to the legacy casterId/spellId/position
    /// API), SpellResolutionResult is the richer, structured output used by any caller that
    /// needs per-category breakdowns: damage events, status applications, temperature changes,
    /// tile mutations, displacements, element-combo effects, and summon requests.
    ///
    /// All lists are non-null and may be empty; they are never null.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class SpellResolutionResult
    {
        // -----------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// <c>false</c> when the caster had a blocking status (SILENCED, STUNNED, or FROZEN)
        /// at the moment the cast was attempted. When this is false every other list is empty
        /// and no mutations were applied to the simulation state.
        /// </summary>
        public bool WasCast { get; }

        /// <summary>
        /// One entry per (target, damage) pair where damage &gt; 0 was applied.
        /// </summary>
        public IReadOnlyList<DamageEvent> DamageDealt { get; }

        /// <summary>
        /// One entry per status effect that was successfully applied to a target unit.
        /// </summary>
        public IReadOnlyList<StatusApplicationEvent> StatusesApplied { get; }

        /// <summary>
        /// One entry per target unit whose temperature was changed by the spell.
        /// </summary>
        public IReadOnlyList<TemperatureEvent> TemperatureChanges { get; }

        /// <summary>
        /// One entry per tile whose elemental state was mutated by this spell.
        /// </summary>
        public IReadOnlyList<TileChangeEvent> TileChanges { get; }

        /// <summary>
        /// One entry per unit that was displaced (pushed or pulled) by this spell.
        /// </summary>
        public IReadOnlyList<DisplacementEvent> Displacements { get; }

        /// <summary>
        /// Combo effects that fired because the spell's element matched a reactive tile state
        /// or unit status on a target (e.g., Lightning + Wet = chain arc).
        /// </summary>
        public IReadOnlyList<ComboEffect> ComboEffects { get; }

        /// <summary>
        /// Summon requests produced by this spell. The caller is responsible for instantiating
        /// companion units described here into the simulation state.
        /// </summary>
        public IReadOnlyList<SummonRequest> Summons { get; }

        // -----------------------------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new <see cref="SpellResolutionResult"/>.
        /// </summary>
        public SpellResolutionResult(
            bool wasCast,
            IReadOnlyList<DamageEvent> damageDealt,
            IReadOnlyList<StatusApplicationEvent> statusesApplied,
            IReadOnlyList<TemperatureEvent> temperatureChanges,
            IReadOnlyList<TileChangeEvent> tileChanges,
            IReadOnlyList<DisplacementEvent> displacements,
            IReadOnlyList<ComboEffect> comboEffects,
            IReadOnlyList<SummonRequest> summons)
        {
            WasCast = wasCast;
            DamageDealt         = damageDealt         ?? new List<DamageEvent>();
            StatusesApplied     = statusesApplied     ?? new List<StatusApplicationEvent>();
            TemperatureChanges  = temperatureChanges  ?? new List<TemperatureEvent>();
            TileChanges         = tileChanges         ?? new List<TileChangeEvent>();
            Displacements       = displacements       ?? new List<DisplacementEvent>();
            ComboEffects        = comboEffects        ?? new List<ComboEffect>();
            Summons             = summons             ?? new List<SummonRequest>();
        }

        /// <summary>
        /// Factory that returns a result representing a blocked cast (caster silenced/stunned/frozen).
        /// All effect lists are empty; <see cref="WasCast"/> is <c>false</c>.
        /// </summary>
        public static SpellResolutionResult Blocked()
        {
            return new SpellResolutionResult(
                wasCast:            false,
                damageDealt:        new List<DamageEvent>(),
                statusesApplied:    new List<StatusApplicationEvent>(),
                temperatureChanges: new List<TemperatureEvent>(),
                tileChanges:        new List<TileChangeEvent>(),
                displacements:      new List<DisplacementEvent>(),
                comboEffects:       new List<ComboEffect>(),
                summons:            new List<SummonRequest>());
        }
    }

    // =============================================================================================
    // Supporting event record types
    // =============================================================================================

    /// <summary>
    /// Records a single instance of direct HP damage dealt to one unit during spell resolution.
    /// </summary>
    public sealed class DamageEvent
    {
        /// <summary>Runtime ID of the unit that took damage.</summary>
        public string TargetId { get; }

        /// <summary>Net damage applied (after armor reduction, clamped to &gt;= 0).</summary>
        public int Amount { get; }

        /// <summary>Initializes a new <see cref="DamageEvent"/>.</summary>
        public DamageEvent(string targetId, int amount)
        {
            TargetId = targetId;
            Amount   = amount;
        }
    }

    /// <summary>
    /// Records a status effect that was successfully applied to a unit during spell resolution.
    /// </summary>
    public sealed class StatusApplicationEvent
    {
        /// <summary>Runtime ID of the unit that received the status.</summary>
        public string TargetId { get; }

        /// <summary>Name of the status type applied (matches <c>StatusType</c> enum value name).</summary>
        public string StatusName { get; }

        /// <summary>Duration in turns of the applied status.</summary>
        public int Duration { get; }

        /// <summary>Initializes a new <see cref="StatusApplicationEvent"/>.</summary>
        public StatusApplicationEvent(string targetId, string statusName, int duration)
        {
            TargetId   = targetId;
            StatusName = statusName;
            Duration   = duration;
        }
    }

    /// <summary>
    /// Records a temperature change applied to one unit during spell resolution.
    /// </summary>
    public sealed class TemperatureEvent
    {
        /// <summary>Runtime ID of the unit whose temperature changed.</summary>
        public string TargetId { get; }

        /// <summary>
        /// The delta applied to the unit's temperature.
        /// Positive values heated the unit; negative values cooled it.
        /// </summary>
        public int Delta { get; }

        /// <summary>The unit's temperature before the delta was applied.</summary>
        public int PreviousTemperature { get; }

        /// <summary>The unit's temperature after the delta was applied (clamped to [-100, +100]).</summary>
        public int NewTemperature { get; }

        /// <summary>Initializes a new <see cref="TemperatureEvent"/>.</summary>
        public TemperatureEvent(string targetId, int delta, int previousTemperature, int newTemperature)
        {
            TargetId            = targetId;
            Delta               = delta;
            PreviousTemperature = previousTemperature;
            NewTemperature      = newTemperature;
        }
    }

    /// <summary>
    /// Records a single tile whose elemental state was changed during spell resolution.
    /// </summary>
    public sealed class TileChangeEvent
    {
        /// <summary>Grid X coordinate of the changed tile.</summary>
        public int X { get; }

        /// <summary>Grid Y coordinate of the changed tile.</summary>
        public int Y { get; }

        /// <summary>The tile's elemental state name before the spell (matches <c>TileState</c> enum).</summary>
        public string OldStateName { get; }

        /// <summary>The tile's elemental state name after the spell (matches <c>TileState</c> enum).</summary>
        public string NewStateName { get; }

        /// <summary>Initializes a new <see cref="TileChangeEvent"/>.</summary>
        public TileChangeEvent(int x, int y, string oldStateName, string newStateName)
        {
            X            = x;
            Y            = y;
            OldStateName = oldStateName;
            NewStateName = newStateName;
        }
    }

    /// <summary>
    /// Records a displacement (push or pull) applied to one unit during spell resolution.
    /// </summary>
    public sealed class DisplacementEvent
    {
        /// <summary>Runtime ID of the displaced unit.</summary>
        public string TargetId { get; }

        /// <summary>Number of tiles the unit was displaced (positive = pushed away from caster).</summary>
        public int TilesMoved { get; }

        /// <summary>Grid X of the unit's position before displacement.</summary>
        public int FromX { get; }

        /// <summary>Grid Y of the unit's position before displacement.</summary>
        public int FromY { get; }

        /// <summary>Grid X of the unit's position after displacement.</summary>
        public int ToX { get; }

        /// <summary>Grid Y of the unit's position after displacement.</summary>
        public int ToY { get; }

        /// <summary>Initializes a new <see cref="DisplacementEvent"/>.</summary>
        public DisplacementEvent(string targetId, int tilesMoved, int fromX, int fromY, int toX, int toY)
        {
            TargetId   = targetId;
            TilesMoved = tilesMoved;
            FromX      = fromX;
            FromY      = fromY;
            ToX        = toX;
            ToY        = toY;
        }
    }

    /// <summary>
    /// Records an element-combo effect that fired during spell resolution.
    ///
    /// A combo is triggered when the incoming spell's element interacts with a reactive
    /// tile state or unit status (e.g., Lightning hitting a Wet tile triggers "chain arc").
    /// </summary>
    public sealed class ComboEffect
    {
        /// <summary>
        /// Human-readable name of the combo that triggered
        /// (e.g., "chain_arc", "flash_freeze", "toxic_fumes").
        /// Matches the VFX hint string from the element interaction table.
        /// </summary>
        public string ComboName { get; }

        /// <summary>
        /// The tile state that was present when the combo triggered
        /// (e.g., "Wet", "Burning", "Frozen").
        /// </summary>
        public string TriggerStateName { get; }

        /// <summary>
        /// The element that triggered the combo (matches <c>ElementType</c> enum value name).
        /// </summary>
        public string TriggerElementName { get; }

        /// <summary>Grid X of the tile where the combo resolved.</summary>
        public int TileX { get; }

        /// <summary>Grid Y of the tile where the combo resolved.</summary>
        public int TileY { get; }

        /// <summary>Initializes a new <see cref="ComboEffect"/>.</summary>
        public ComboEffect(string comboName, string triggerStateName, string triggerElementName, int tileX, int tileY)
        {
            ComboName          = comboName;
            TriggerStateName   = triggerStateName;
            TriggerElementName = triggerElementName;
            TileX              = tileX;
            TileY              = tileY;
        }
    }

    /// <summary>
    /// A request to summon a companion unit as a result of a spell cast.
    ///
    /// <see cref="SpellResolutionResult"/> surfaces summon requests rather than directly
    /// mutating the simulation state so the caller retains control over unit ID generation,
    /// faction alignment, and positioning logic.
    /// </summary>
    public sealed class SummonRequest
    {
        /// <summary>
        /// Tag identifying which companion unit type to summon
        /// (e.g., "skeleton_warrior", "wolf", "bone_spike").
        /// Matches <c>SpellData.summonUnitTag</c>.
        /// </summary>
        public string UnitTag { get; }

        /// <summary>Runtime ID of the Mancer that cast the summon spell.</summary>
        public string SummonerId { get; }

        /// <summary>Preferred spawn grid X coordinate (nearest valid tile to be determined by caller).</summary>
        public int PreferredSpawnX { get; }

        /// <summary>Preferred spawn grid Y coordinate (nearest valid tile to be determined by caller).</summary>
        public int PreferredSpawnY { get; }

        /// <summary>Initializes a new <see cref="SummonRequest"/>.</summary>
        public SummonRequest(string unitTag, string summonerId, int preferredSpawnX, int preferredSpawnY)
        {
            UnitTag          = unitTag;
            SummonerId       = summonerId;
            PreferredSpawnX  = preferredSpawnX;
            PreferredSpawnY  = preferredSpawnY;
        }
    }
}

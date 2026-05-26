namespace Battlemancers.Simulation.Status
{
    /// <summary>
    /// Represents the result of one end-of-turn tick of a single <see cref="StatusEffect"/>
    /// on a single unit.
    ///
    /// <see cref="StatusManager.TickStatuses"/> produces one <see cref="StatusTickResult"/> per
    /// active status per living unit. Callers (e.g. TurnManager) aggregate these results to apply
    /// damage, remove units, and drive VFX events.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class StatusTickResult
    {
        // ---------------------------------------------------------------------------
        // Properties
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Runtime ID of the unit that was ticked.
        /// Matches the key used in <see cref="StatusManager"/>'s internal dictionary.
        /// </summary>
        public string UnitId { get; }

        /// <summary>
        /// The type of status that was ticked.
        /// </summary>
        public StatusType StatusType { get; }

        /// <summary>
        /// Amount of HP damage dealt to the unit by this tick.
        /// 0 for statuses that have no per-tick damage (e.g. Frozen, Wet, Stunned).
        ///
        /// Per-tick damage values:
        /// <list type="bullet">
        ///   <item><description><see cref="StatusType.Burning"/>  — 5 HP per tick (flat).</description></item>
        ///   <item><description><see cref="StatusType.Poisoned"/> — 3 HP × current stack count per tick.</description></item>
        ///   <item><description>All other types — 0 HP (behavioural effects only).</description></item>
        /// </list>
        ///
        /// Damage is already clamped and applied to <c>UnitState.CurrentHP</c> by the time
        /// this result is returned; callers should read the value for event publishing only.
        /// </summary>
        public int DamageDealt { get; }

        /// <summary>
        /// True when this tick was the final tick for this status — its duration reached 0
        /// and the status has been removed from the unit.
        /// </summary>
        public bool StatusExpired { get; }

        /// <summary>
        /// True when the status' duration or stack count changed this tick for any reason
        /// (normal duration decrement, early cleanse signal, or stack change).
        /// Always true unless the tick had no effect whatsoever (which should not occur in practice).
        /// </summary>
        public bool StatusModified { get; }

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new <see cref="StatusTickResult"/>.
        /// </summary>
        /// <param name="unitId">Runtime ID of the unit that was ticked.</param>
        /// <param name="statusType">The status type that was ticked.</param>
        /// <param name="damageDealt">HP damage dealt this tick (0 if none).</param>
        /// <param name="statusExpired">Whether the status expired and was removed this tick.</param>
        /// <param name="statusModified">Whether the status' state (duration or stacks) changed.</param>
        public StatusTickResult(string unitId, StatusType statusType, int damageDealt,
                                bool statusExpired, bool statusModified)
        {
            UnitId = unitId;
            StatusType = statusType;
            DamageDealt = damageDealt;
            StatusExpired = statusExpired;
            StatusModified = statusModified;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"StatusTickResult[Unit={UnitId} Status={StatusType} Dmg={DamageDealt} "
                 + $"Expired={StatusExpired} Modified={StatusModified}]";
        }
    }
}

using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Simulation.Status
{
    /// <summary>
    /// Published when a <see cref="StatusEffect"/> is applied to a unit for the first time,
    /// or when an existing status is refreshed or stacked via
    /// <see cref="StatusManager.ApplyStatus"/>.
    ///
    /// Subscribers: VFX director (apply visual), HUD (show status icon), audio (play apply SFX).
    /// </summary>
    public sealed class StatusAppliedEvent : SimulationEvent
    {
        /// <summary>Runtime ID of the unit that received the status.</summary>
        public string UnitId { get; }

        /// <summary>The type of status that was applied or refreshed.</summary>
        public StatusType StatusType { get; }

        /// <summary>
        /// The duration (turns remaining) of the status after this application.
        /// For duration-stacking types, this is the new total duration.
        /// </summary>
        public int Duration { get; }

        /// <summary>
        /// The stack count of the status after this application.
        /// For non-stacking types, this is always 1.
        /// For <see cref="StatusType.Poisoned"/>, this is the new total stack count (1–5).
        /// </summary>
        public int StackCount { get; }

        /// <summary>
        /// Initializes a new <see cref="StatusAppliedEvent"/>.
        /// </summary>
        /// <param name="turnNumber">The turn on which this event occurred.</param>
        /// <param name="unitId">Runtime ID of the affected unit.</param>
        /// <param name="statusType">The status type applied.</param>
        /// <param name="duration">Duration of the status after application.</param>
        /// <param name="stackCount">Stack count of the status after application.</param>
        public StatusAppliedEvent(int turnNumber, string unitId, StatusType statusType,
                                  int duration, int stackCount)
            : base(turnNumber)
        {
            UnitId = unitId;
            StatusType = statusType;
            Duration = duration;
            StackCount = stackCount;
        }
    }

    /// <summary>
    /// Published when a <see cref="StatusEffect"/> is removed from a unit, either because it
    /// expired naturally or was force-removed by a cleanse spell.
    ///
    /// Subscribers: VFX director (remove visual), HUD (remove status icon), audio (play remove SFX).
    /// </summary>
    public sealed class StatusRemovedEvent : SimulationEvent
    {
        /// <summary>Runtime ID of the unit that lost the status.</summary>
        public string UnitId { get; }

        /// <summary>The type of status that was removed.</summary>
        public StatusType StatusType { get; }

        /// <summary>
        /// Human-readable reason the status was removed.
        /// Common values: <c>"expired"</c> (natural duration expiry), <c>"cleansed"</c> (force-removed by spell).
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Initializes a new <see cref="StatusRemovedEvent"/>.
        /// </summary>
        /// <param name="turnNumber">The turn on which this event occurred.</param>
        /// <param name="unitId">Runtime ID of the affected unit.</param>
        /// <param name="statusType">The status type removed.</param>
        /// <param name="reason">Why the status was removed (e.g. "expired", "cleansed").</param>
        public StatusRemovedEvent(int turnNumber, string unitId, StatusType statusType, string reason)
            : base(turnNumber)
        {
            UnitId = unitId;
            StatusType = statusType;
            Reason = reason ?? string.Empty;
        }
    }

    /// <summary>
    /// Published once per status per unit after each end-of-turn tick processed by
    /// <see cref="StatusManager.TickStatuses"/>.
    ///
    /// Subscribers: VFX director (fire/poison particle pulse), HUD (damage flash),
    /// audio (tick SFX for DoT types).
    /// </summary>
    public sealed class StatusTickedEvent : SimulationEvent
    {
        /// <summary>Runtime ID of the unit that was ticked.</summary>
        public string UnitId { get; }

        /// <summary>The type of status that ticked.</summary>
        public StatusType StatusType { get; }

        /// <summary>
        /// HP damage applied to the unit during this tick.
        /// 0 for statuses that deal no per-tick damage.
        /// </summary>
        public int DamageDealt { get; }

        /// <summary>
        /// True if this tick was the final one for this status — the status expired and
        /// was removed immediately after ticking. A <see cref="StatusRemovedEvent"/> with
        /// reason "expired" will also be published in the same tick pass.
        /// </summary>
        public bool Expired { get; }

        /// <summary>
        /// Initializes a new <see cref="StatusTickedEvent"/>.
        /// </summary>
        /// <param name="turnNumber">The turn on which this tick occurred.</param>
        /// <param name="unitId">Runtime ID of the affected unit.</param>
        /// <param name="statusType">The status type that ticked.</param>
        /// <param name="damageDealt">HP damage dealt this tick (0 if none).</param>
        /// <param name="expired">Whether the status expired after this tick.</param>
        public StatusTickedEvent(int turnNumber, string unitId, StatusType statusType,
                                 int damageDealt, bool expired)
            : base(turnNumber)
        {
            UnitId = unitId;
            StatusType = statusType;
            DamageDealt = damageDealt;
            Expired = expired;
        }
    }
}

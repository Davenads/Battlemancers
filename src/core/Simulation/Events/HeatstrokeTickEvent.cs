namespace Battlemancers.Core.Simulation.Events
{
    /// <summary>
    /// Published when the Heatstroke AP penalty first applies to a unit or changes in magnitude.
    ///
    /// Heatstroke accumulates when a unit spends 3 or more consecutive turns at OVERHEATED
    /// (temperature ≥ +61). The penalty is -1 AP at turn 3, -2 AP at turn 4, and -3 AP at
    /// turn 5 and beyond. This event fires at the moment the penalty activates (3 consecutive
    /// turns) and again whenever the penalty value increases.
    ///
    /// Subscribers: HUD (show Heatstroke warning icon and AP penalty readout), audio (heat-
    /// exhaustion SFX on first trigger), VFX (heat-distortion shimmer on unit portrait).
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class HeatstrokeTickEvent : SimulationEvent
    {
        /// <summary>Runtime ID of the unit suffering the Heatstroke AP penalty.</summary>
        public string UnitId { get; }

        /// <summary>
        /// The number of consecutive turns this unit has been at OVERHEATED (temperature ≥ +61)
        /// at the moment this event is published. Will be 3 or greater when this event fires.
        /// </summary>
        public int ConsecutiveTurns { get; }

        /// <summary>
        /// The AP penalty applied to this unit at the start of its next activation.
        /// Computed as <c>Max(0, Min(3, ConsecutiveTurns - 2))</c>.
        /// Range: 1 on the first tick (turn 3), up to 3 at maximum (turn 5+).
        /// </summary>
        public int APPenalty { get; }

        /// <summary>
        /// Initializes a new <see cref="HeatstrokeTickEvent"/>.
        /// </summary>
        /// <param name="turnNumber">The simulation turn on which this event was generated.</param>
        /// <param name="unitId">Runtime ID of the unit suffering the penalty.</param>
        /// <param name="consecutiveTurns">
        /// Number of consecutive OVERHEATED turns accumulated, including the current turn.
        /// Must be ≥ 3 for the penalty to be active.
        /// </param>
        /// <param name="apPenalty">
        /// The AP penalty value being applied. Derived from <paramref name="consecutiveTurns"/>
        /// via <c>Max(0, Min(3, consecutiveTurns - 2))</c>.
        /// </param>
        public HeatstrokeTickEvent(int turnNumber, string unitId, int consecutiveTurns, int apPenalty)
            : base(turnNumber)
        {
            UnitId = unitId;
            ConsecutiveTurns = consecutiveTurns;
            APPenalty = apPenalty;
        }
    }
}

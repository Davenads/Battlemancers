namespace Battlemancers.Core.Simulation.Events
{
    /// <summary>
    /// Published when a player uses their once-per-match Thermal Composure ability.
    ///
    /// Thermal Composure allows any unit to spend 3 AP to immediately reset its temperature
    /// to 0. Each player has exactly one charge per match. This event is published after the
    /// unit's temperature has been set to 0 and the charge has been consumed from
    /// <see cref="SimulationState"/>.
    ///
    /// Note: Status effects that were temperature-held (e.g., BURNING from OVERHEATED,
    /// FROZEN from FROZEN SOLID) are NOT removed by this event directly. They will be
    /// cleaned up at the next threshold check, which occurs on the next call to
    /// <see cref="TemperatureManager.ApplyTemperatureChange"/> or
    /// <see cref="TemperatureManager.TickHeatstrokePenalties"/>.
    ///
    /// Subscribers: HUD (remove Thermal Composure charge indicator for this player),
    /// audio (cold-exhale / steam-vent SFX), VFX (temperature-reset burst effect on unit).
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class ThermalComposureUsedEvent : SimulationEvent
    {
        /// <summary>
        /// The player ID whose Thermal Composure charge was consumed.
        /// After this event, <c>SimulationState.HasThermalComposure(PlayerId)</c> returns false.
        /// </summary>
        public string PlayerId { get; }

        /// <summary>Runtime ID of the unit that activated Thermal Composure.</summary>
        public string UnitId { get; }

        /// <summary>
        /// The unit's temperature immediately before the reset.
        /// This is the value that was cleared to 0 by the ability.
        /// </summary>
        public int TemperatureReset { get; }

        /// <summary>
        /// Initializes a new <see cref="ThermalComposureUsedEvent"/>.
        /// </summary>
        /// <param name="turnNumber">The simulation turn on which this event was generated.</param>
        /// <param name="playerId">
        /// The player ID whose once-per-match Thermal Composure charge was consumed.
        /// </param>
        /// <param name="unitId">Runtime ID of the unit that used the ability.</param>
        /// <param name="temperatureReset">
        /// The temperature value the unit held before it was reset to 0.
        /// Positive values indicate the unit was overheated; negative values indicate it was cold.
        /// </param>
        public ThermalComposureUsedEvent(int turnNumber, string playerId, string unitId, int temperatureReset)
            : base(turnNumber)
        {
            PlayerId = playerId;
            UnitId = unitId;
            TemperatureReset = temperatureReset;
        }
    }
}

namespace Battlemancers.Core.Simulation.Events
{
    /// <summary>
    /// Published whenever a unit's temperature changes — whether from a spell, terrain passive,
    /// natural decay, or any other source.
    ///
    /// Subscribers: VFX director (thermometer bar animation, threshold flash), HUD (temperature
    /// readout update), audio (thermal state SFX). When <see cref="ThermalShockTriggered"/> is
    /// true, the VFX director should play the Thermal Shock impact effect.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class TemperatureChangedEvent : SimulationEvent
    {
        // ---------------------------------------------------------------------------
        // Properties
        // ---------------------------------------------------------------------------

        /// <summary>Runtime ID of the unit whose temperature changed.</summary>
        public string UnitId { get; }

        /// <summary>The unit's temperature before this change was applied.</summary>
        public int PreviousTemperature { get; }

        /// <summary>The unit's temperature after this change was applied (clamped to [-100, +100]).</summary>
        public int NewTemperature { get; }

        /// <summary>
        /// The <see cref="TemperatureCategory"/> corresponding to <see cref="PreviousTemperature"/>.
        /// Compare to <see cref="NewCategory"/> to detect threshold crossings for UI/VFX purposes.
        /// </summary>
        public TemperatureCategory PreviousCategory { get; }

        /// <summary>
        /// The <see cref="TemperatureCategory"/> corresponding to <see cref="NewTemperature"/>.
        /// When this differs from <see cref="PreviousCategory"/>, a threshold was crossed and
        /// status effects may have been applied or removed.
        /// </summary>
        public TemperatureCategory NewCategory { get; }

        /// <summary>
        /// True if this temperature change triggered a Thermal Shock — the delta crossed both
        /// the -31 and +31 thresholds simultaneously (i.e., temperature moved from ≤ -31 to
        /// ≥ +31, or from ≥ +31 to ≤ -31, in a single application).
        /// </summary>
        public bool ThermalShockTriggered { get; }

        /// <summary>
        /// Bonus damage dealt by the Thermal Shock, computed as |temperature_delta| / 2
        /// (integer division). Zero if <see cref="ThermalShockTriggered"/> is false.
        /// </summary>
        public int ThermalShockDamage { get; }

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new <see cref="TemperatureChangedEvent"/>.
        /// </summary>
        /// <param name="turnNumber">The simulation turn on which this change occurred.</param>
        /// <param name="unitId">Runtime ID of the affected unit.</param>
        /// <param name="previousTemperature">Temperature before this change.</param>
        /// <param name="newTemperature">Temperature after this change (clamped).</param>
        /// <param name="previousCategory">Category derived from <paramref name="previousTemperature"/>.</param>
        /// <param name="newCategory">Category derived from <paramref name="newTemperature"/>.</param>
        /// <param name="thermalShockTriggered">Whether Thermal Shock occurred on this change.</param>
        /// <param name="thermalShockDamage">Bonus damage from Thermal Shock (0 if none).</param>
        public TemperatureChangedEvent(
            int turnNumber,
            string unitId,
            int previousTemperature,
            int newTemperature,
            TemperatureCategory previousCategory,
            TemperatureCategory newCategory,
            bool thermalShockTriggered,
            int thermalShockDamage)
            : base(turnNumber)
        {
            UnitId = unitId;
            PreviousTemperature = previousTemperature;
            NewTemperature = newTemperature;
            PreviousCategory = previousCategory;
            NewCategory = newCategory;
            ThermalShockTriggered = thermalShockTriggered;
            ThermalShockDamage = thermalShockDamage;
        }
    }
}

using System;

namespace Battlemancers.Core.Simulation.StatusEffects
{
    /// <summary>
    /// Concrete <see cref="IStatusEffect"/> for the Wet status.
    ///
    /// Wet marks a unit as soaked with water. Its interactions with other elements:
    /// <list type="bullet">
    ///   <item><description>Amplifies lightning — a Wet unit is conductive; lightning spells
    ///     chain-arc to all adjacent Wet units.</description></item>
    ///   <item><description>Extinguishes Burning — when Wet is applied to a unit that carries
    ///     Burning, <see cref="StatusManager"/> removes the Burning status as a side effect.
    ///     This cross-status interaction is enforced in <c>StatusManager.HandleApplySideEffects</c>
    ///     rather than in this class, since it requires access to the full status registry.</description></item>
    /// </list>
    ///
    /// Stacking rule: duration-stacking — a second application extends the remaining duration.
    /// Default duration: <see cref="DefaultDuration"/> turns.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class WetStatus : IStatusEffect
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        /// <summary>Default duration applied when no explicit value is provided.</summary>
        public const int DefaultDuration = 3;

        // ---------------------------------------------------------------------------
        // Fields
        // ---------------------------------------------------------------------------

        private readonly string _sourceId;

        // ---------------------------------------------------------------------------
        // IStatusEffect implementation
        // ---------------------------------------------------------------------------

        /// <inheritdoc/>
        public string DisplayName => "Wet";

        /// <inheritdoc/>
        public int RemainingDuration { get; set; }

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initialises a new <see cref="WetStatus"/>.
        /// </summary>
        /// <param name="duration">Number of turns this status lasts. Must be at least 1.</param>
        /// <param name="sourceId">ID of the unit or system that applied this status.</param>
        public WetStatus(int duration, string sourceId = "unknown")
        {
            if (duration < 1)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be at least 1 turn.");
            RemainingDuration = duration;
            _sourceId = sourceId ?? "unknown";
        }

        // ---------------------------------------------------------------------------
        // IStatusEffect methods
        // ---------------------------------------------------------------------------

        /// <inheritdoc/>
        /// <remarks>
        /// The Burning-extinguish side effect is handled by <c>StatusManager</c> after
        /// registering this status, not here, because it requires access to the full status
        /// registry. This method is a no-op.
        /// </remarks>
        public void Apply(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <remarks>Wet deals no per-tick damage. Conductivity interactions are evaluated
        /// by the ElementResolver when a lightning spell is cast.</remarks>
        public void Tick(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <remarks>Wet has no special on-remove effect.</remarks>
        public void Remove(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <returns><c>true</c> when <paramref name="other"/> is also a <see cref="WetStatus"/>
        /// (duration-stacking rule).</returns>
        public bool StacksWith(IStatusEffect other) => other is WetStatus;

        /// <inheritdoc/>
        public override string ToString() =>
            $"WetStatus[Duration={RemainingDuration} Source={_sourceId}]";
    }
}

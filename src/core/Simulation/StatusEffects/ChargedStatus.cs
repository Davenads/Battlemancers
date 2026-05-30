using System;

namespace Battlemancers.Core.Simulation.StatusEffects
{
    /// <summary>
    /// Concrete <see cref="IStatusEffect"/> for the Charged status.
    ///
    /// Charged marks a unit as carrying an electrical charge. Its interactions:
    /// <list type="bullet">
    ///   <item><description>Amplifies lightning — a Charged unit is a conductor.
    ///     When struck by a lightning spell it triggers an <c>Overload</c> (AoE burst),
    ///     evaluated by the <c>ElementResolver</c>.</description></item>
    ///   <item><description>Charged tile synergy — units on a Charged tile receive an extra
    ///     Arc Explosion on the first lightning hit.</description></item>
    /// </list>
    ///
    /// Stacking rule: duration-stacking — a second application extends the remaining duration.
    /// Stack count is always 1. No per-tick damage.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class ChargedStatus : IStatusEffect
    {
        // ---------------------------------------------------------------------------
        // Fields
        // ---------------------------------------------------------------------------

        private readonly string _sourceId;

        // ---------------------------------------------------------------------------
        // IStatusEffect implementation
        // ---------------------------------------------------------------------------

        /// <inheritdoc/>
        public string DisplayName => "Charged";

        /// <inheritdoc/>
        public int RemainingDuration { get; set; }

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initialises a new <see cref="ChargedStatus"/>.
        /// </summary>
        /// <param name="duration">Number of turns this status lasts. Must be at least 1.</param>
        /// <param name="sourceId">ID of the unit or system that applied this status.</param>
        public ChargedStatus(int duration, string sourceId = "unknown")
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
        /// <remarks>Charged has no immediate on-apply effect beyond being registered.
        /// Lightning amplification is evaluated by <c>ElementResolver</c> on spell impact.</remarks>
        public void Apply(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <remarks>Charged deals no per-tick damage. It is a passive conductive marker.</remarks>
        public void Tick(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <remarks>Charged has no special on-remove effect.</remarks>
        public void Remove(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <returns><c>true</c> when <paramref name="other"/> is also a <see cref="ChargedStatus"/>
        /// (duration-stacking rule).</returns>
        public bool StacksWith(IStatusEffect other) => other is ChargedStatus;

        /// <inheritdoc/>
        public override string ToString() =>
            $"ChargedStatus[Duration={RemainingDuration} Source={_sourceId}]";
    }
}

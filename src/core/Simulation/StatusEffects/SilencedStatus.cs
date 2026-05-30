using System;

namespace Battlemancers.Core.Simulation.StatusEffects
{
    /// <summary>
    /// Concrete <see cref="IStatusEffect"/> for the Silenced status.
    ///
    /// Silenced prevents a unit from casting spells while still allowing movement.
    /// It additionally blocks on-death effects for its duration — a Silenced unit that
    /// dies while this status is active does not trigger any passive death-effect abilities
    /// (e.g. Necromancer corpse-fuel generation, explosive death spells). The blocking
    /// is enforced by the death-resolution layer, which checks
    /// <see cref="UnitState.ActiveStatusTypes"/> for <c>"Silenced"</c>.
    ///
    /// Stacking rule: duration-stacking — a second application extends the remaining duration.
    /// No per-tick damage.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class SilencedStatus : IStatusEffect
    {
        // ---------------------------------------------------------------------------
        // Fields
        // ---------------------------------------------------------------------------

        private readonly string _sourceId;

        // ---------------------------------------------------------------------------
        // IStatusEffect implementation
        // ---------------------------------------------------------------------------

        /// <inheritdoc/>
        public string DisplayName => "Silenced";

        /// <inheritdoc/>
        public int RemainingDuration { get; set; }

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initialises a new <see cref="SilencedStatus"/>.
        /// </summary>
        /// <param name="duration">Number of turns this status lasts. Must be at least 1.</param>
        /// <param name="sourceId">ID of the unit or system that applied this status.</param>
        public SilencedStatus(int duration, string sourceId = "unknown")
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
        /// <remarks>Silenced has no immediate on-apply effect beyond being registered in
        /// <see cref="UnitState.ActiveStatusTypes"/>. Spell-blocking is enforced by the
        /// action resolver at cast time.</remarks>
        public void Apply(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <remarks>Silenced deals no per-tick damage. It is a passive restriction marker.</remarks>
        public void Tick(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <remarks>Silenced has no special on-remove effect.</remarks>
        public void Remove(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <returns><c>true</c> when <paramref name="other"/> is also a <see cref="SilencedStatus"/>
        /// (duration-stacking rule).</returns>
        public bool StacksWith(IStatusEffect other) => other is SilencedStatus;

        /// <inheritdoc/>
        public override string ToString() =>
            $"SilencedStatus[Duration={RemainingDuration} Source={_sourceId}]";
    }
}

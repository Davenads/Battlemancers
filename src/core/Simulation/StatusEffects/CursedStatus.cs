using System;

namespace Battlemancers.Core.Simulation.StatusEffects
{
    /// <summary>
    /// Concrete <see cref="IStatusEffect"/> for the Cursed status.
    ///
    /// Cursed imposes a persistent hex that reduces all incoming healing by
    /// <see cref="HealingMultiplier"/> (50%) for the duration of the status.
    /// Healing sources (spell resolvers, terrain regen) should call
    /// <see cref="ModifyHealing"/> before applying HP recovery to any unit that
    /// carries this status (detectable via <see cref="UnitState.ActiveStatusTypes"/>
    /// containing <c>"Cursed"</c>).
    ///
    /// Stacking rule: duration-stacking — a second application extends the remaining duration.
    /// No per-tick damage.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class CursedStatus : IStatusEffect
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Fraction of raw healing actually applied to a Cursed unit (0.5 = 50% reduction).
        /// </summary>
        public const float HealingMultiplier = 0.5f;

        // ---------------------------------------------------------------------------
        // Fields
        // ---------------------------------------------------------------------------

        private readonly string _sourceId;

        // ---------------------------------------------------------------------------
        // IStatusEffect implementation
        // ---------------------------------------------------------------------------

        /// <inheritdoc/>
        public string DisplayName => "Cursed";

        /// <inheritdoc/>
        public int RemainingDuration { get; set; }

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initialises a new <see cref="CursedStatus"/>.
        /// </summary>
        /// <param name="duration">Number of turns this status lasts. Must be at least 1.</param>
        /// <param name="sourceId">ID of the unit or system that applied this status.</param>
        public CursedStatus(int duration, string sourceId = "unknown")
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
        /// <remarks>Cursed has no immediate on-apply effect beyond being registered.
        /// The healing reduction is applied by callers via <see cref="ModifyHealing"/>.</remarks>
        public void Apply(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <remarks>Cursed deals no per-tick damage. It is a passive modifier status.</remarks>
        public void Tick(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <remarks>Cursed has no special on-remove effect.</remarks>
        public void Remove(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <returns><c>true</c> when <paramref name="other"/> is also a <see cref="CursedStatus"/>
        /// (duration-stacking rule).</returns>
        public bool StacksWith(IStatusEffect other) => other is CursedStatus;

        // ---------------------------------------------------------------------------
        // Healing modifier
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Applies the Cursed healing reduction to a raw healing amount.
        /// Returns the actual HP that should be restored to the unit (integer division).
        /// Callers should invoke this whenever healing is applied to a unit whose
        /// <see cref="UnitState.ActiveStatusTypes"/> contains <c>"Cursed"</c>.
        /// </summary>
        /// <param name="rawHealing">The full healing amount before the curse penalty.</param>
        /// <returns>
        /// The effective healing after the <see cref="HealingMultiplier"/> is applied
        /// (rounded down via integer truncation).
        /// </returns>
        public int ModifyHealing(int rawHealing) => (int)(rawHealing * HealingMultiplier);

        /// <inheritdoc/>
        public override string ToString() =>
            $"CursedStatus[Duration={RemainingDuration} Source={_sourceId}]";
    }
}

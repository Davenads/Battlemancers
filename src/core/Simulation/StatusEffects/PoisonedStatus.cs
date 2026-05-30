using System;

namespace Battlemancers.Core.Simulation.StatusEffects
{
    /// <summary>
    /// Concrete <see cref="IStatusEffect"/> for the Poisoned status.
    ///
    /// Per-tick behaviour:
    /// <list type="bullet">
    ///   <item><description>Deals <see cref="DamagePerStack"/> × <see cref="StackCount"/> HP
    ///     to the carrier each turn (e.g. 5 stacks = 15 HP/turn).</description></item>
    /// </list>
    ///
    /// Stacking rule: count-stacking — each application adds one stack (cap <see cref="MaxStacks"/>).
    /// Duration freezes while the carrier also carries Frozen (no decay timer while frozen).
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class PoisonedStatus : IStatusEffect
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        /// <summary>HP damage dealt per active stack on each tick.</summary>
        public const int DamagePerStack = 3;

        /// <summary>Maximum number of stacks that can accumulate on a single unit.</summary>
        public const int MaxStacks = 5;

        // ---------------------------------------------------------------------------
        // Fields
        // ---------------------------------------------------------------------------

        private readonly string _sourceId;

        // ---------------------------------------------------------------------------
        // IStatusEffect implementation
        // ---------------------------------------------------------------------------

        /// <inheritdoc/>
        public string DisplayName => "Poisoned";

        /// <inheritdoc/>
        public int RemainingDuration { get; set; }

        /// <summary>
        /// Current number of active poison stacks on this unit (1–<see cref="MaxStacks"/>).
        /// Incremented by <see cref="StatusManager"/> on each re-application up to the cap.
        /// </summary>
        public int StackCount { get; set; }

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initialises a new <see cref="PoisonedStatus"/>.
        /// </summary>
        /// <param name="duration">Number of turns this status lasts. Must be at least 1.</param>
        /// <param name="stackCount">Initial stack count. Must be between 1 and <see cref="MaxStacks"/>.</param>
        /// <param name="sourceId">ID of the unit or system that applied this status.</param>
        public PoisonedStatus(int duration, int stackCount = 1, string sourceId = "unknown")
        {
            if (duration < 1)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be at least 1 turn.");
            if (stackCount < 1 || stackCount > MaxStacks)
                throw new ArgumentOutOfRangeException(nameof(stackCount),
                    $"StackCount must be between 1 and {MaxStacks}.");

            RemainingDuration = duration;
            StackCount = stackCount;
            _sourceId = sourceId ?? "unknown";
        }

        // ---------------------------------------------------------------------------
        // IStatusEffect methods
        // ---------------------------------------------------------------------------

        /// <inheritdoc/>
        /// <remarks>Poisoned has no immediate on-apply effect beyond being registered.</remarks>
        public void Apply(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <remarks>
        /// Deals <see cref="DamagePerStack"/> × <see cref="StackCount"/> HP damage
        /// (floored at 0). Note: <see cref="StatusManager"/> skips ticking Poisoned
        /// while the carrier also carries Frozen ("no decay timer while frozen" rule).
        /// </remarks>
        public void Tick(UnitState unit, SimulationState state)
        {
            if (unit == null)
                return;

            int totalDamage = DamagePerStack * StackCount;
            int actualDamage = Math.Min(totalDamage, unit.CurrentHP);
            unit.CurrentHP -= actualDamage;
        }

        /// <inheritdoc/>
        /// <remarks>Poisoned has no special on-remove effect.</remarks>
        public void Remove(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <returns><c>true</c> when <paramref name="other"/> is also a <see cref="PoisonedStatus"/>
        /// and the current stack count is below <see cref="MaxStacks"/>.</returns>
        public bool StacksWith(IStatusEffect other)
            => other is PoisonedStatus && StackCount < MaxStacks;

        /// <inheritdoc/>
        public override string ToString() =>
            $"PoisonedStatus[Duration={RemainingDuration} Stacks={StackCount} Source={_sourceId}]";
    }
}

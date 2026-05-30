using System;
using Battlemancers.Core.Grid;

namespace Battlemancers.Core.Simulation.StatusEffects
{
    /// <summary>
    /// Concrete <see cref="IStatusEffect"/> for the Burning status.
    ///
    /// Per-tick behaviour:
    /// <list type="bullet">
    ///   <item><description>Deals <see cref="DamagePerTick"/> (5 HP) to the carrier each turn.</description></item>
    ///   <item><description>Spreads fire to adjacent <c>Normal</c> and <c>Natural</c> tiles,
    ///     turning them to <c>Burning</c>.</description></item>
    /// </list>
    ///
    /// Stacking rule: duration-stacking — a second application extends the remaining duration.
    /// Stack count is always 1.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class BurningStatus : IStatusEffect
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        /// <summary>HP damage dealt to the carrying unit each tick.</summary>
        public const int DamagePerTick = 5;

        // ---------------------------------------------------------------------------
        // Fields
        // ---------------------------------------------------------------------------

        private readonly string _sourceId;

        // ---------------------------------------------------------------------------
        // IStatusEffect implementation
        // ---------------------------------------------------------------------------

        /// <inheritdoc/>
        public string DisplayName => "Burning";

        /// <inheritdoc/>
        public int RemainingDuration { get; set; }

        /// <summary>Stack count for Burning is always 1.</summary>
        public int StackCount { get; set; } = 1;

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initialises a new <see cref="BurningStatus"/>.
        /// </summary>
        /// <param name="duration">Number of turns this status lasts. Must be at least 1.</param>
        /// <param name="sourceId">ID of the unit or system that applied this status.</param>
        public BurningStatus(int duration, string sourceId = "unknown")
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
        /// <remarks>Burning has no immediate on-apply effect beyond being registered.</remarks>
        public void Apply(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <remarks>
        /// Deals <see cref="DamagePerTick"/> HP damage (floored at 0) and spreads fire to
        /// adjacent <c>Normal</c> and <c>Natural</c> tiles via <c>state.Grid.GetNeighbors</c>.
        /// Spreading is skipped when <paramref name="state"/> is <c>null</c>.
        /// </remarks>
        public void Tick(UnitState unit, SimulationState state)
        {
            if (unit == null)
                return;

            // --- DoT damage ---
            int damage = Math.Min(DamagePerTick, unit.CurrentHP);
            unit.CurrentHP -= damage;

            // --- Fire spreading to adjacent flammable tiles ---
            if (state == null)
                return;

            foreach (Tile neighbor in state.Grid.GetNeighbors(unit.Position))
            {
                if (neighbor.State == TileState.Normal || neighbor.State == TileState.Natural)
                    state.Grid.SetTileState(neighbor.Position, TileState.Burning);
            }
        }

        /// <inheritdoc/>
        /// <remarks>Burning has no special on-remove effect.</remarks>
        public void Remove(UnitState unit, SimulationState state) { }

        /// <inheritdoc/>
        /// <returns><c>true</c> when <paramref name="other"/> is also a <see cref="BurningStatus"/>
        /// (duration-stacking rule).</returns>
        public bool StacksWith(IStatusEffect other) => other is BurningStatus;

        /// <inheritdoc/>
        public override string ToString() =>
            $"BurningStatus[Duration={RemainingDuration} Source={_sourceId}]";
    }
}

namespace Battlemancers.Core.Simulation
{
    /// <summary>
    /// Defines the behavioral contract for a status effect that can be applied to,
    /// ticked on, and removed from a <see cref="UnitState"/> during simulation.
    ///
    /// Each of the six core status effect types (Burning, Wet, Poisoned, Charged,
    /// Silenced, Cursed) ships with a concrete implementation in
    /// <c>Battlemancers.Core.Simulation.StatusEffects</c>.
    ///
    /// <see cref="StatusManager"/> is the single authority that applies, ticks, and
    /// removes statuses. It constructs the appropriate concrete class via its internal
    /// factory — callers never need a switch on <see cref="Battlemancers.Simulation.Status.StatusType"/>
    /// outside of <c>StatusManager</c>.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public interface IStatusEffect
    {
        /// <summary>
        /// Human-readable name for this status effect.
        /// Used by the presentation layer for HUD labels and by <see cref="StatusManager"/>
        /// to correlate concrete instances with <see cref="Battlemancers.Simulation.Status.StatusType"/>
        /// enum values (must match <c>StatusType.ToString()</c> exactly).
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Turns remaining before this status expires naturally.
        /// Decremented once per turn by <see cref="StatusManager.TickStatuses"/>.
        /// When it reaches zero the status is removed and a <c>StatusRemovedEvent</c> is published.
        /// </summary>
        int RemainingDuration { get; set; }

        /// <summary>
        /// Called once when the status is first applied to <paramref name="unit"/>.
        /// Responsible for any immediate one-time effects (e.g. a warm-up burst, instant slow).
        /// Cross-status side-effects such as Wet extinguishing Burning are handled by
        /// <see cref="StatusManager"/> internally and do not need to be duplicated here.
        /// </summary>
        /// <param name="unit">The unit receiving the status.</param>
        /// <param name="state">The current simulation state. May be <c>null</c> in unit-test contexts
        /// that do not require terrain or grid access.</param>
        void Apply(UnitState unit, SimulationState state);

        /// <summary>
        /// Called once per turn by <see cref="StatusManager.TickStatuses"/> for each unit
        /// that carries this status. Responsible for per-turn effects such as DoT damage,
        /// fire spreading, or contamination.
        ///
        /// Implementations that deal HP damage must apply the reduction directly to
        /// <see cref="UnitState.CurrentHP"/> (floored at 0). <see cref="StatusManager"/>
        /// measures the HP delta and includes it in the <c>StatusTickResult</c> and
        /// published <c>StatusTickedEvent</c>.
        /// </summary>
        /// <param name="unit">The unit being ticked.</param>
        /// <param name="state">The current simulation state. May be <c>null</c> in unit-test contexts.</param>
        void Tick(UnitState unit, SimulationState state);

        /// <summary>
        /// Called when the status is removed from <paramref name="unit"/>, either because
        /// it expired naturally or was force-removed by a cleanse spell.
        /// Responsible for any cleanup effects (e.g. restoring a stat that was modified on Apply).
        /// </summary>
        /// <param name="unit">The unit losing the status.</param>
        /// <param name="state">The current simulation state. May be <c>null</c>.</param>
        void Remove(UnitState unit, SimulationState state);

        /// <summary>
        /// Returns <c>true</c> if a second application of <paramref name="other"/> (the same
        /// status type from any source) should stack with this instance rather than be ignored.
        ///
        /// Stacking behaviour varies by type:
        /// <list type="bullet">
        ///   <item><description>Duration-stacking (Burning, Wet, Charged, Silenced, Cursed) — returns <c>true</c>.</description></item>
        ///   <item><description>Count-stacking (Poisoned) — returns <c>true</c> while below max stacks.</description></item>
        ///   <item><description>Non-stacking types — returns <c>false</c>.</description></item>
        /// </list>
        ///
        /// The final stacking decision is enforced by <see cref="StatusManager"/> using its own
        /// per-type rules; this method provides a hint that the concrete class exposes for testing.
        /// </summary>
        /// <param name="other">The incoming status effect of the same type being re-applied.</param>
        bool StacksWith(IStatusEffect other);
    }
}

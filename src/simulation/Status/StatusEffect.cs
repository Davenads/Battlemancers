using System;

namespace Battlemancers.Simulation.Status
{
    /// <summary>
    /// Identifies the type of a status effect on a unit.
    ///
    /// Stacking rules per type:
    /// <list type="bullet">
    ///   <item><description><see cref="Burning"/>  — DURATION STACKS: re-applying extends duration; stack count stays at 1.</description></item>
    ///   <item><description><see cref="Wet"/>       — DURATION STACKS: re-applying extends duration.</description></item>
    ///   <item><description><see cref="Frozen"/>    — REPLACE IF LONGER: only overwritten when new duration exceeds current; otherwise ignored.</description></item>
    ///   <item><description><see cref="Poisoned"/>  — COUNT STACKS: each application adds one stack (max 5); damage scales with stack count.</description></item>
    ///   <item><description><see cref="Charged"/>   — DURATION STACKS: re-applying extends duration.</description></item>
    ///   <item><description><see cref="Stunned"/>   — CANNOT STACK: only one instance; duration is reset on re-apply but stack count stays at 1.</description></item>
    ///   <item><description><see cref="Panicked"/>  — CANNOT STACK: must expire before re-application takes effect.</description></item>
    ///   <item><description><see cref="Charmed"/>   — CANNOT STACK: must expire before re-application takes effect.</description></item>
    ///   <item><description><see cref="Cursed"/>    — DURATION STACKS: re-applying extends duration.</description></item>
    ///   <item><description><see cref="Silenced"/>  — DURATION STACKS: re-applying extends duration.</description></item>
    ///   <item><description><see cref="Slowed"/>    — DURATION STACKS: re-applying extends duration.</description></item>
    /// </list>
    /// </summary>
    public enum StatusType
    {
        /// <summary>
        /// Unit is on fire. Deals 5 HP per turn. Duration stacks on re-apply.
        /// Extinguished by water spells or Hydromancer cleanse.
        /// </summary>
        Burning,

        /// <summary>
        /// Unit is soaked with water. Enables lightning chain arcs and freezing interactions.
        /// Duration stacks on re-apply. No per-tick damage.
        /// </summary>
        Wet,

        /// <summary>
        /// Unit is encased in ice. Skips next turn and gains SHATTER vulnerability (×2.5 physical/sonic damage).
        /// Replace-if-longer stacking rule. No per-tick damage.
        /// </summary>
        Frozen,

        /// <summary>
        /// Unit is poisoned. Deals 3 HP × stack count per turn (max 5 stacks = 15 HP/turn).
        /// Count stacks on re-apply (up to 5). Removed by cleanse abilities.
        /// </summary>
        Poisoned,

        /// <summary>
        /// Unit carries an electrical charge. Enables lightning overload interactions.
        /// Duration stacks on re-apply. No per-tick damage.
        /// </summary>
        Charged,

        /// <summary>
        /// Unit is stunned and skips its entire turn. Cannot stack; duration resets on re-apply.
        /// No per-tick damage.
        /// </summary>
        Stunned,

        /// <summary>
        /// Unit is panicking. Movement and attack targeting become random.
        /// Cannot stack; must expire before a second application takes effect.
        /// No per-tick damage.
        /// </summary>
        Panicked,

        /// <summary>
        /// Unit is charmed and is controlled by the opposing player for one turn.
        /// Cannot stack; must expire before re-application. No per-tick damage.
        /// </summary>
        Charmed,

        /// <summary>
        /// Unit is cursed with a persistent hex. Duration stacks on re-apply.
        /// No per-tick damage (effect depends on spell source).
        /// </summary>
        Cursed,

        /// <summary>
        /// Unit cannot cast spells but may still move. Duration stacks on re-apply.
        /// No per-tick damage.
        /// </summary>
        Silenced,

        /// <summary>
        /// Unit movement is reduced by 1. Duration stacks on re-apply.
        /// Applied by mud terrain or Hydromancer abilities. No per-tick damage.
        /// </summary>
        Slowed
    }

    /// <summary>
    /// A single status effect instance active on a unit.
    ///
    /// StatusEffect is a pure data object — it stores the type, remaining duration,
    /// current stack count, and the ID of the unit that applied it. All mutation
    /// (stacking, ticking, removal) is performed by <see cref="StatusManager"/>.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public class StatusEffect
    {
        // ---------------------------------------------------------------------------
        // Properties
        // ---------------------------------------------------------------------------

        /// <summary>
        /// The kind of status this effect represents.
        /// Immutable after construction — use <see cref="StatusManager"/> to swap effects.
        /// </summary>
        public StatusType Type { get; }

        /// <summary>
        /// Number of turns remaining before this status expires naturally.
        /// Decremented by <see cref="StatusManager.TickStatuses"/> at end of turn.
        /// When it reaches 0, the status is removed and <c>StatusExpired</c> is set on the tick result.
        ///
        /// Stacking behaviour varies by <see cref="StatusType"/>:
        /// <list type="bullet">
        ///   <item><description>Duration-stacking types: extended by adding new duration to current value.</description></item>
        ///   <item><description>Replace-if-longer types (Frozen): replaced only when new duration is strictly greater.</description></item>
        ///   <item><description>Cannot-stack types (Stunned, Panicked, Charmed): duration is reset to new value.</description></item>
        /// </list>
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// How many stacks of this status are currently active on the unit.
        ///
        /// Only meaningful for <see cref="StatusType.Poisoned"/> (max 5 stacks; damage = 3 × stacks).
        /// All other status types hold a stack count of 1 at all times.
        /// </summary>
        public int StackCount { get; set; }

        /// <summary>
        /// Runtime unit ID of the unit that applied this status effect.
        /// Used for attribution in damage events and cleanse logic.
        /// Immutable after construction.
        /// </summary>
        public string SourceId { get; }

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new <see cref="StatusEffect"/>.
        /// </summary>
        /// <param name="type">The kind of status effect.</param>
        /// <param name="duration">
        /// Initial turn duration. Must be greater than zero.
        /// </param>
        /// <param name="stackCount">
        /// Initial stack count. Must be at least 1.
        /// For <see cref="StatusType.Poisoned"/>, pass the desired starting stack count (1–5).
        /// For all other types, pass 1.
        /// </param>
        /// <param name="sourceId">
        /// Runtime ID of the unit that applied this status. Must not be null or empty.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="duration"/> is less than 1 or <paramref name="stackCount"/> is less than 1.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="sourceId"/> is null or empty.</exception>
        public StatusEffect(StatusType type, int duration, int stackCount, string sourceId)
        {
            if (duration < 1)
                throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be at least 1 turn.");
            if (stackCount < 1)
                throw new ArgumentOutOfRangeException(nameof(stackCount), "StackCount must be at least 1.");
            if (string.IsNullOrEmpty(sourceId))
                throw new ArgumentException("SourceId must not be null or empty.", nameof(sourceId));

            Type = type;
            Duration = duration;
            StackCount = stackCount;
            SourceId = sourceId;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"StatusEffect[{Type} Duration={Duration} Stacks={StackCount} Source={SourceId}]";
        }
    }
}

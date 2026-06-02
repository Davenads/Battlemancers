using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Simulation.Commands
{
    /// <summary>
    /// Command that causes a Mancer to cast a spell at a target grid position.
    ///
    /// Full spell effect resolution (element interactions, AoE targeting, damage
    /// calculation, tile state changes) is handled by SpellResolver, which subscribes
    /// to SpellCastEvent and processes the full effect chain.
    ///
    /// This command validates cast legality, emits a SpellCastEvent, and places the
    /// spell on cooldown. Range validation uses a fallback of 4 tiles Manhattan distance;
    /// per-spell range from SpellData will replace this once spell definitions are wired
    /// into command construction.
    /// </summary>
    public sealed class SpellCommand : Command
    {
        // Fallback range used when per-spell range is not yet supplied from SpellData.
        private const int FallbackSpellRange = 4;

        // Default cooldown applied to all spells; per-spell values from SpellData will
        // take precedence once spell definitions are wired into command construction.
        private const int DefaultSpellCooldownTurns = 1;

        /// <summary>Definition ID of the spell being cast (e.g., "pyromancer_fireball").</summary>
        public string SpellId { get; }

        /// <summary>Grid position the spell is targeted at.</summary>
        public GridPosition Target { get; }

        /// <summary>
        /// Creates a SpellCommand for the specified caster, spell, and target.
        /// </summary>
        /// <param name="actorId">Runtime ID of the Mancer casting the spell.</param>
        /// <param name="activationCost">Budget cost of this unit's activation (always 100 for Mancers).</param>
        /// <param name="spellId">Definition ID of the spell to cast.</param>
        /// <param name="target">Grid position to target.</param>
        public SpellCommand(string actorId, int activationCost, string spellId, GridPosition target)
            : base(actorId, activationCost)
        {
            SpellId = spellId;
            Target = target;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Validates:
        /// <list type="bullet">
        ///   <item>Actor exists and is alive.</item>
        ///   <item>Actor is a Mancer (only Mancers can cast spells).</item>
        ///   <item>The spell is not currently on cooldown for this unit.</item>
        ///   <item>Target is within grid bounds.</item>
        ///   <item>Target is within the fallback range of 4 Manhattan tiles.</item>
        /// </list>
        /// Future: LOS validation, per-spell targeting type constraints, AP cost checks,
        /// and spell-specific target restrictions will be enforced once SpellData is wired
        /// into command construction.
        /// </remarks>
        public override bool Validate(SimulationState state)
        {
            UnitState actor = state.GetUnit(ActorId);

            // Actor must exist and be alive.
            if (actor == null || !actor.IsAlive)
                return false;

            // Only Mancers can cast spells.
            if (actor.Type != UnitType.Mancer)
                return false;

            // Spell must not be on cooldown.
            if (actor.SpellCooldowns.ContainsKey(SpellId))
                return false;

            // Target must be within grid bounds.
            if (!state.Grid.IsInBounds(Target))
                return false;

            // Target must be within the fallback range (Manhattan distance).
            int distance = actor.Position.ManhattanDistance(Target);
            if (distance > FallbackSpellRange)
                return false;

            return true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Places the spell on cooldown and emits a SpellCastEvent. SpellResolver subscribes
        /// to SpellCastEvent and processes the full effect chain (damage, tile state changes,
        /// status applications, element interactions).
        /// </remarks>
        public override SimulationEvent[] Execute(SimulationState state)
        {
            UnitState actor = state.GetUnit(ActorId);

            // Apply cooldown so the spell cannot be cast again immediately.
            actor.SpellCooldowns[SpellId] = DefaultSpellCooldownTurns;

            // Emit the cast event. The presentation layer uses this to trigger the cast
            // animation and spell VFX. SpellResolver responds to apply the spell's effects.
            return new SimulationEvent[]
            {
                new SpellCastEvent(state.TurnNumber, ActorId, SpellId, Target)
            };
        }
    }
}

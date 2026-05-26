using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Simulation.Commands
{
    /// <summary>
    /// Command that causes a Mancer to cast a spell at a target grid position.
    ///
    /// Full spell effect resolution (element interactions, AoE targeting, damage
    /// calculation, tile state changes) is handled by the SpellResolver in Wave 2.
    /// This command validates cast legality, emits a SpellCastEvent, and places
    /// the spell on cooldown. The SpellResolver subscribes to SpellCastEvent and
    /// processes the full effect chain.
    ///
    /// Stub range: 4 tiles Manhattan distance. Wave 2 will replace this with
    /// per-spell range values sourced from SpellData definitions.
    /// </summary>
    public sealed class SpellCommand : Command
    {
        // Placeholder range used until SpellData is wired in Wave 2.
        private const int StubSpellRange = 4;

        // Default cooldown applied to all spells until SpellData defines per-spell values.
        private const int StubSpellCooldownTurns = 1;

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
        ///   <item>Target is within the stub range of 4 Manhattan tiles.</item>
        /// </list>
        /// Wave 2 will add: LOS validation, per-spell targeting type constraints,
        /// AP cost checks, and spell-specific target restrictions.
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

            // Target must be within stub range (Manhattan distance).
            int distance = actor.Position.ManhattanDistance(Target);
            if (distance > StubSpellRange)
                return false;

            return true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Places the spell on cooldown and emits a SpellCastEvent. Full effect resolution
        /// (damage, tile state changes, status applications) is deferred to the SpellResolver
        /// in Wave 2, which will subscribe to SpellCastEvent and process the chain.
        /// </remarks>
        public override SimulationEvent[] Execute(SimulationState state)
        {
            UnitState actor = state.GetUnit(ActorId);

            // Apply cooldown so the spell cannot be cast again next turn.
            actor.SpellCooldowns[SpellId] = StubSpellCooldownTurns;

            // Emit the cast event. The presentation layer uses this to trigger the cast
            // wind-up animation and the start of the spell VFX. SpellResolver (Wave 2)
            // will also respond to this event to apply the spell's effects.
            return new SimulationEvent[]
            {
                new SpellCastEvent(state.TurnNumber, ActorId, SpellId, Target)
            };
        }
    }
}

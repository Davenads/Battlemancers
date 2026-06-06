using System.Collections.Generic;
using System.Linq;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Data;
using Battlemancers.Simulation.Effects;

namespace Battlemancers.Core.Simulation.Commands
{
    /// <summary>
    /// Command that causes a Mancer to cast a spell at a target grid position.
    ///
    /// When constructed with a <see cref="SpellData"/> definition and a
    /// <see cref="SpellEffectApplicator"/>, Execute() resolves full spell effects
    /// (damage, status applications, temperature changes, tile mutations, element combos)
    /// via the applicator and converts the structured result into SimulationEvents.
    ///
    /// When constructed without an applicator (legacy / event-driven path), Execute()
    /// emits only a <see cref="SpellCastEvent"/> and places the spell on cooldown; the
    /// presentation-layer SpellResolver is expected to subscribe to that event and apply
    /// effects downstream.
    ///
    /// Range validation uses a fallback of 4 tiles Manhattan distance; per-spell range
    /// from SpellData will replace this once spell definitions are fully wired.
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

        // Optional wiring for full headless effect resolution.
        private readonly SpellData _spellData;
        private readonly SpellEffectApplicator _applicator;

        /// <summary>
        /// Creates a SpellCommand for the specified caster, spell, and target.
        /// This overload emits only a <see cref="SpellCastEvent"/>; spell effects are
        /// resolved by a downstream SpellResolver that subscribes to the event.
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

        /// <summary>
        /// Creates a SpellCommand that fully resolves spell effects in Execute() using the
        /// supplied <see cref="SpellEffectApplicator"/>. Use this overload in headless tests
        /// and in any context where the event-driven SpellResolver is not present.
        /// </summary>
        /// <param name="actorId">Runtime ID of the Mancer casting the spell.</param>
        /// <param name="activationCost">Budget cost of this unit's activation (always 100 for Mancers).</param>
        /// <param name="spellData">Full spell definition used to compute effects.</param>
        /// <param name="target">Grid position to target.</param>
        /// <param name="applicator">Applicator that resolves damage, statuses, temperature, and terrain.</param>
        public SpellCommand(string actorId, int activationCost, SpellData spellData,
                            GridPosition target, SpellEffectApplicator applicator)
            : base(actorId, activationCost)
        {
            SpellId    = spellData?.spellId ?? string.Empty;
            Target     = target;
            _spellData = spellData;
            _applicator = applicator;
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

            // Target must be within range. Use the spell definition's range when available;
            // fall back to FallbackSpellRange when no SpellData is wired.
            int effectiveRange = _spellData != null ? _spellData.range : FallbackSpellRange;
            int distance = actor.Position.ManhattanDistance(Target);
            if (distance > effectiveRange)
                return false;

            return true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Places the spell on cooldown and emits a <see cref="SpellCastEvent"/>.
        ///
        /// When this command was constructed with a <see cref="SpellEffectApplicator"/>,
        /// Execute() also resolves the full effect chain immediately: damage is applied
        /// to any unit occupying the target tile, status effects and temperature changes
        /// are forwarded to the respective managers, and a <see cref="UnitDiedEvent"/>
        /// is appended for each unit whose HP reaches zero.
        ///
        /// When no applicator is present, only the <see cref="SpellCastEvent"/> is
        /// returned; the presentation-layer SpellResolver is expected to handle effects.
        /// </remarks>
        public override SimulationEvent[] Execute(SimulationState state)
        {
            UnitState actor = state.GetUnit(ActorId);

            // Apply cooldown so the spell cannot be cast again immediately.
            if (!string.IsNullOrEmpty(SpellId))
                actor.SpellCooldowns[SpellId] = DefaultSpellCooldownTurns;

            var events = new List<SimulationEvent>
            {
                new SpellCastEvent(state.TurnNumber, ActorId, SpellId, Target)
            };

            // Full effect resolution path — only active when SpellData and applicator are wired.
            if (_applicator != null && _spellData != null)
            {
                // Collect all living units on or adjacent to the target tile that belong
                // to opposing players. For SingleTarget spells this is at most one unit.
                var targets = new List<UnitState>();
                string occupantId = state.Grid.GetOccupantId(Target);
                if (occupantId != null && occupantId != ActorId)
                {
                    UnitState occupant = state.GetUnit(occupantId);
                    if (occupant != null && occupant.IsAlive)
                        targets.Add(occupant);
                }

                Battlemancers.Simulation.SpellResolutionResult result =
                    _applicator.Apply(_spellData, actor, targets, state);

                // Emit UnitDiedEvent for any targets killed by this spell.
                if (result.WasCast)
                {
                    foreach (Battlemancers.Simulation.DamageEvent dmg in result.DamageDealt)
                    {
                        UnitState killed = state.GetUnit(dmg.TargetId);
                        if (killed != null && !killed.IsAlive)
                        {
                            events.Add(new UnitDiedEvent(
                                state.TurnNumber, killed.Id, killed.Position, ActorId));
                        }
                    }
                }
            }

            return events.ToArray();
        }
    }
}

using System.Collections.Generic;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Simulation.Commands
{
    /// <summary>
    /// Command that performs a basic melee attack from one unit against an adjacent enemy unit.
    ///
    /// Deals flat base damage. Full damage calculation (attacker stats, defender armor,
    /// status modifiers, element weaknesses) is handled by the combat damage resolver
    /// and can be layered on top via SpellResolver for spell-based attacks.
    /// The adjacency rule is strict: Manhattan distance must be exactly 1
    /// (no diagonal attacks in the base system).
    ///
    /// If the attack kills the defender, a UnitDiedEvent is also emitted and the defender
    /// is deregistered from the simulation.
    /// </summary>
    public sealed class AttackCommand : Command
    {
        // Base flat damage per melee attack. The combat resolver applies stat-based
        // modifiers on top of this value for full damage calculation.
        private const int BaseAttackDamage = 10;

        /// <summary>Runtime ID of the unit being attacked.</summary>
        public string DefenderId { get; }

        /// <summary>
        /// Creates an AttackCommand from the attacker targeting the specified defender.
        /// </summary>
        /// <param name="actorId">Runtime ID of the attacking unit.</param>
        /// <param name="activationCost">Budget cost of this unit's activation.</param>
        /// <param name="defenderId">Runtime ID of the unit being attacked.</param>
        public AttackCommand(string actorId, int activationCost, string defenderId)
            : base(actorId, activationCost)
        {
            DefenderId = defenderId;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Validates:
        /// <list type="bullet">
        ///   <item>Attacker exists in the simulation and is alive.</item>
        ///   <item>Defender exists in the simulation and is alive.</item>
        ///   <item>Attacker and defender are owned by different players (no friendly fire).</item>
        ///   <item>Defender is adjacent to the attacker (Manhattan distance == 1).</item>
        /// </list>
        /// </remarks>
        public override bool Validate(SimulationState state)
        {
            UnitState attacker = state.GetUnit(ActorId);
            UnitState defender = state.GetUnit(DefenderId);

            // Both units must exist and be alive.
            if (attacker == null || !attacker.IsAlive)
                return false;

            if (defender == null || !defender.IsAlive)
                return false;

            // Cannot attack your own units.
            if (attacker.OwnerId == defender.OwnerId)
                return false;

            // Defender must be directly adjacent (Manhattan distance == 1, no diagonals).
            if (attacker.Position.ManhattanDistance(defender.Position) != 1)
                return false;

            return true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Deals flat base damage to the defender. If the defender's HP drops to 0,
        /// a UnitDiedEvent is also emitted and the unit is deregistered.
        /// Returns between 1 and 2 events (always UnitDamagedEvent; optionally UnitDiedEvent).
        /// </remarks>
        public override SimulationEvent[] Execute(SimulationState state)
        {
            UnitState attacker = state.GetUnit(ActorId);
            UnitState defender = state.GetUnit(DefenderId);

            // Apply base damage, floor at 0.
            int previousHP = defender.CurrentHP;
            int newHP = previousHP - BaseAttackDamage;
            if (newHP < 0) newHP = 0;
            defender.CurrentHP = newHP;

            var events = new List<SimulationEvent>
            {
                new UnitDamagedEvent(
                    state.TurnNumber,
                    DefenderId,
                    damageAmount: previousHP - newHP,
                    damageSource: ActorId,
                    remainingHP: newHP)
            };

            // If the defender died, record the death and remove it from the simulation.
            if (!defender.IsAlive)
            {
                GridPosition deathPosition = defender.Position;

                // Free up the tile the defender was occupying.
                state.Grid.ClearOccupant(deathPosition);

                // Remove from the unit registry.
                state.DeregisterUnit(DefenderId);

                events.Add(new UnitDiedEvent(
                    state.TurnNumber,
                    DefenderId,
                    deathPosition,
                    killerUnitId: ActorId));
            }

            return events.ToArray();
        }
    }
}

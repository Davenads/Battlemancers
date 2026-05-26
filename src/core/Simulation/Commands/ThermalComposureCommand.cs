using System;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Simulation.Commands
{
    /// <summary>
    /// Command that activates the once-per-match Thermal Composure ability on the acting unit.
    ///
    /// Thermal Composure instantly resets the unit's temperature to 0, clearing accumulated
    /// heat or cold in a single action. Each player has exactly one charge per match. The
    /// ability costs 3 AP charged to the unit's own action point pool (not the activation
    /// budget) — matching the same pattern as SpellCommand.
    ///
    /// Note on status effects: Temperature-held statuses (BURNING from OVERHEATED, FROZEN from
    /// FROZEN SOLID, SLOWED from HOT or SUPERCOOLED) are NOT immediately removed by this command.
    /// They will be cleaned up naturally at the next call to
    /// <see cref="TemperatureManager.ApplyTemperatureChange"/> or
    /// <see cref="TemperatureManager.TickHeatstrokePenalties"/>, which perform threshold checks
    /// against the unit's new temperature of 0. This prevents the command from needing a direct
    /// reference to TemperatureManager and avoids double-cleanup edge cases.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class ThermalComposureCommand : Command
    {
        /// <summary>AP cost deducted from the acting unit's own action point pool.</summary>
        private const int AbilityAPCost = 3;

        /// <summary>
        /// Creates a ThermalComposureCommand for the given actor.
        /// </summary>
        /// <param name="actorId">Runtime ID of the unit activating Thermal Composure.</param>
        public ThermalComposureCommand(string actorId)
            : base(actorId, activationCost: 0)
        {
            // ActivationCost = 0: the 3 AP is charged to the unit's own ActionPoints,
            // not the activation budget. This matches the SpellCommand pattern where
            // spell AP costs are internal to the command and the budget cost is the
            // unit's type-based activation cost (100 for Mancers, etc.).
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Validates:
        /// <list type="bullet">
        ///   <item>The acting unit exists in the simulation registry.</item>
        ///   <item>The acting unit is alive (CurrentHP > 0).</item>
        ///   <item>The acting unit has at least 3 AP available.</item>
        ///   <item>The acting unit's owner still has a Thermal Composure charge available.</item>
        /// </list>
        /// </remarks>
        public override bool Validate(SimulationState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            UnitState actor = state.GetUnit(ActorId);

            // Unit must exist and be alive.
            if (actor == null || !actor.IsAlive)
                return false;

            // Unit must have enough AP for the ability.
            if (actor.ActionPoints < AbilityAPCost)
                return false;

            // The owning player must still have their Thermal Composure charge.
            if (!state.HasThermalComposure(actor.OwnerId))
                return false;

            return true;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Execution sequence:
        /// <list type="number">
        ///   <item>Deducts 3 AP from the actor's ActionPoints.</item>
        ///   <item>Consumes the player's once-per-match Thermal Composure charge.</item>
        ///   <item>Records the unit's current temperature before reset.</item>
        ///   <item>Sets the unit's Temperature to 0.</item>
        ///   <item>Resets ConsecutiveOverheatedTurns to 0 (Heatstroke counter cleared).</item>
        ///   <item>Publishes <see cref="ThermalComposureUsedEvent"/>.</item>
        ///   <item>
        ///     Publishes <see cref="TemperatureChangedEvent"/> (previousTemp → 0) so the
        ///     presentation layer can animate the thermometer bar reset.
        ///   </item>
        /// </list>
        /// Status effects held by temperature thresholds (BURNING, FROZEN, SLOWED) are NOT
        /// removed here. They persist until the next threshold check in
        /// <see cref="TemperatureManager.ApplyTemperatureChange"/> or
        /// <see cref="TemperatureManager.TickHeatstrokePenalties"/>, at which point the
        /// unit's temperature of 0 will cause them to be cleaned up via
        /// CheckAndApplyThresholdStatuses. This is intentional — the reset closes the
        /// temperature source, not the current status effect.
        /// </remarks>
        public override SimulationEvent[] Execute(SimulationState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            UnitState actor = state.GetUnit(ActorId);

            // 1. Deduct AP cost.
            actor.ActionPoints -= AbilityAPCost;

            // 2. Consume the player's once-per-match charge.
            state.ConsumeThermalComposure(actor.OwnerId);

            // 3. Record previous temperature for events.
            int previousTemp = actor.Temperature;

            // 4. Reset temperature to neutral.
            actor.Temperature = 0;

            // 5. Clear the Heatstroke counter — the unit is no longer overheated.
            actor.ConsecutiveOverheatedTurns = 0;

            // 6. Publish Thermal Composure used event.
            SimulationEventBus.Publish(new ThermalComposureUsedEvent(
                state.TurnNumber,
                actor.OwnerId,
                ActorId,
                previousTemp));

            // 7. Publish temperature changed event so the presentation layer animates the bar.
            TemperatureCategory previousCategory = TemperatureManager.GetCategory(previousTemp);
            TemperatureCategory newCategory = TemperatureManager.GetCategory(0);

            SimulationEventBus.Publish(new TemperatureChangedEvent(
                state.TurnNumber,
                ActorId,
                previousTemp,
                newTemperature: 0,
                previousCategory,
                newCategory,
                thermalShockTriggered: false,
                thermalShockDamage: 0));

            return Array.Empty<SimulationEvent>();
        }
    }
}

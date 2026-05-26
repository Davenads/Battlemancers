using System;
using System.Collections.Generic;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Warband
{
    /// <summary>
    /// Validates a player's submitted activation plan for a single turn against the
    /// 100-point activation budget rule defined in the Battlemancers warband spec.
    ///
    /// The activation budget rule:
    /// <list type="bullet">
    ///   <item>Each turn, a player may activate units totaling at most 100 activation points.</item>
    ///   <item>Mancers always cost 100 activation points regardless of upgrade spend.</item>
    ///   <item>Chaff and Ranged units activate at their purchase point cost (T1 Chaff=10, T2 Chaff=20, T1 Ranged=25, T2 Ranged=50).</item>
    ///   <item>Partial activation (spending fewer than 100 pts) is always legal.</item>
    /// </list>
    ///
    /// Zero Unity dependencies. Safe to use in headless simulation, unit tests, and server-side validation.
    /// </summary>
    public static class ActivationPlanValidator
    {
        /// <summary>Maximum activation point budget per turn.</summary>
        public const int ActivationBudget = 100;

        // -----------------------------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Validates a turn activation plan against the 100-point activation budget.
        ///
        /// The plan is valid if:
        /// <list type="number">
        ///   <item>Every unit ID in <paramref name="activatedUnitIds"/> is present in <paramref name="unitActivationCosts"/>.</item>
        ///   <item>The sum of activation costs for all activated units does not exceed 100.</item>
        /// </list>
        ///
        /// An empty activation plan (no units activated) is valid and costs 0 pts.
        /// </summary>
        /// <param name="activatedUnitIds">
        /// The runtime unit IDs the player has chosen to activate this turn.
        /// Must match keys in <paramref name="unitActivationCosts"/>. May be empty but not null.
        /// </param>
        /// <param name="unitActivationCosts">
        /// Lookup of runtime unit ID → activation cost for all units in this player's warband.
        /// Built at match start from <see cref="GetActivationCost"/> and the warband roster.
        /// Must not be null.
        /// </param>
        /// <returns>
        /// <see cref="ValidationResult.IsValid"/> true if the plan is within budget and all IDs
        /// are recognized. False with error messages otherwise.
        /// <see cref="ValidationResult.TotalPointCost"/> contains the sum of activation costs for
        /// the submitted plan (even when invalid).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="activatedUnitIds"/> or <paramref name="unitActivationCosts"/> is null.
        /// </exception>
        public static ValidationResult ValidatePlan(
            string[] activatedUnitIds,
            Dictionary<string, int> unitActivationCosts)
        {
            if (activatedUnitIds == null) throw new ArgumentNullException(nameof(activatedUnitIds));
            if (unitActivationCosts == null) throw new ArgumentNullException(nameof(unitActivationCosts));

            var errors = new List<string>();
            int totalCost = 0;

            foreach (string unitId in activatedUnitIds)
            {
                if (string.IsNullOrWhiteSpace(unitId))
                {
                    errors.Add("Activation plan contains a null or empty unit ID.");
                    continue;
                }

                if (!unitActivationCosts.TryGetValue(unitId, out int cost))
                {
                    errors.Add($"Unit ID '{unitId}' is not recognised in this warband's activation cost table.");
                    continue;
                }

                totalCost += cost;
            }

            // Check budget only if all IDs were valid (avoids a misleading "over budget" error
            // when the real problem is an unknown unit ID).
            if (errors.Count == 0 && totalCost > ActivationBudget)
            {
                errors.Add(
                    $"Activation plan costs {totalCost} pts, which exceeds the {ActivationBudget}-pt turn budget by {totalCost - ActivationBudget} pts.");
            }

            if (errors.Count > 0)
                return ValidationResult.Failure(errors.ToArray(), Array.Empty<string>(), totalCost);

            return ValidationResult.Success(totalCost);
        }

        // -----------------------------------------------------------------------------------------
        // Activation cost helper
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Returns the activation cost for a unit given its type and warband point cost.
        ///
        /// Activation cost rules (from warbands.md):
        /// <list type="bullet">
        ///   <item><see cref="UnitType.Mancer"/> — always 100 pts, regardless of warband point cost or upgrades.</item>
        ///   <item><see cref="UnitType.Chaff"/> — equals <paramref name="pointCost"/> (10 for T1, 20 for T2).</item>
        ///   <item><see cref="UnitType.Ranged"/> — equals <paramref name="pointCost"/> (25 for T1, 50 for T2).</item>
        /// </list>
        /// </summary>
        /// <param name="type">The unit category.</param>
        /// <param name="pointCost">
        /// The unit's warband purchase cost (used directly as the activation cost for non-Mancer units).
        /// For Mancers this parameter is ignored.
        /// </param>
        /// <returns>The activation budget cost charged when this unit is included in a turn plan.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="type"/> is not a recognised <see cref="UnitType"/> value.
        /// </exception>
        public static int GetActivationCost(UnitType type, int pointCost)
        {
            switch (type)
            {
                case UnitType.Mancer:
                    return ActivationBudget; // Always 100 regardless of upgrade spend.

                case UnitType.Chaff:
                    return pointCost;

                case UnitType.Ranged:
                    return pointCost;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type,
                        $"Unrecognised UnitType value '{type}'.");
            }
        }
    }
}

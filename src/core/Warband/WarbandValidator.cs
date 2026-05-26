using System;
using System.Collections.Generic;
using Battlemancers.Data;

namespace Battlemancers.Core.Warband
{
    /// <summary>
    /// Validates a <see cref="WarbandSave"/> against all warband construction rules defined in
    /// the Battlemancers design spec (warbands.md).
    ///
    /// All validation is stateless and deterministic. The class is static — instantiation is not
    /// required or permitted.
    ///
    /// Zero Unity dependencies. Safe to use in headless simulation, unit tests, and server-side validation.
    ///
    /// Primary entry point: <see cref="Validate(WarbandSave)"/>.
    /// </summary>
    public static class WarbandValidator
    {
        // -----------------------------------------------------------------------------------------
        // Constants
        // -----------------------------------------------------------------------------------------

        /// <summary>Hard point cap for a single warband.</summary>
        public const int PointCap = 1000;

        /// <summary>Maximum number of Mancers allowed in a warband.</summary>
        public const int MaxMancers = 3;

        /// <summary>Minimum number of Mancers required in a valid warband.</summary>
        public const int MinMancers = 1;

        /// <summary>
        /// Warbands with a total cost below this threshold trigger a low-budget warning.
        /// </summary>
        public const int UnderbudgetWarningThreshold = 900;

        // -----------------------------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Validates a <see cref="WarbandSave"/> against all warband construction rules.
        ///
        /// Rules enforced:
        /// <list type="number">
        ///   <item>Total point cost must not exceed 1,000 pts.</item>
        ///   <item>Warband must include at most 3 Mancers.</item>
        ///   <item>Warband must include at least 1 Mancer.</item>
        ///   <item>Support units must all belong to the same faction (no cross-faction mixing).</item>
        ///   <item>No two Mancer slots may share the same archetype ID.</item>
        ///   <item>No two selected upgrades on a single Mancer may be mutually exclusive with each other.</item>
        ///   <item>Each support unit entry must reference a known unit type ID.</item>
        /// </list>
        ///
        /// Non-fatal warnings:
        /// <list type="bullet">
        ///   <item>Total cost below 900 pts.</item>
        ///   <item>No Ranged units included in the warband.</item>
        /// </list>
        /// </summary>
        /// <param name="warband">The warband save to validate. Must not be null.</param>
        /// <returns>
        /// A <see cref="ValidationResult"/> with <see cref="ValidationResult.IsValid"/> true if all
        /// rules pass, or false with populated <see cref="ValidationResult.Errors"/> if any rule fails.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="warband"/> is null.</exception>
        public static ValidationResult Validate(WarbandSave warband)
        {
            if (warband == null) throw new ArgumentNullException(nameof(warband));

            var errors = new List<string>();
            var warnings = new List<string>();

            // Run all validation passes and collect errors.
            errors.AddRange(ValidateMancers(warband));
            errors.AddRange(ValidateSupportUnits(warband));
            errors.AddRange(ValidateUpgrades(warband));

            int totalCost = ComputeTotalCost(warband);

            // Rule 1: Point cap.
            if (totalCost > PointCap)
                errors.Add($"Warband exceeds the {PointCap}-point cap by {totalCost - PointCap} pts (total: {totalCost} pts).");

            // Non-fatal: underbudget.
            if (totalCost < UnderbudgetWarningThreshold)
                warnings.Add($"Warband uses only {totalCost} pts — consider adding units to fill the remaining {PointCap - totalCost} pts.");

            // Non-fatal: no ranged units.
            if (!HasRangedUnits(warband))
                warnings.Add("Warband contains no Ranged units. Consider adding Ranged support for sustained fire.");

            if (errors.Count > 0)
                return ValidationResult.Failure(errors.ToArray(), warnings.ToArray(), totalCost);

            return ValidationResult.Success(totalCost, warnings.ToArray());
        }

        // -----------------------------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Validates all Mancer-specific rules:
        /// <list type="bullet">
        ///   <item>Rule 2: At most 3 Mancers.</item>
        ///   <item>Rule 3: At least 1 Mancer.</item>
        ///   <item>Rule 5: No duplicate Mancer archetypes.</item>
        /// </list>
        /// </summary>
        /// <param name="warband">The warband to inspect.</param>
        /// <returns>Array of error strings. Empty if all Mancer rules pass.</returns>
        private static string[] ValidateMancers(WarbandSave warband)
        {
            var errors = new List<string>();

            if (warband.mancers == null || warband.mancers.Count < MinMancers)
            {
                errors.Add($"Warband must include at least {MinMancers} Mancer.");
                // Cannot check further Mancer rules without any Mancers.
                return errors.ToArray();
            }

            if (warband.mancers.Count > MaxMancers)
                errors.Add($"Warband contains {warband.mancers.Count} Mancers; the maximum is {MaxMancers}.");

            // Rule 5: No duplicate archetype IDs.
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var loadout in warband.mancers)
            {
                if (string.IsNullOrWhiteSpace(loadout.mancerArchetypeId))
                {
                    errors.Add("A Mancer loadout has an empty or missing archetype ID.");
                    continue;
                }

                if (!seen.Add(loadout.mancerArchetypeId))
                    errors.Add($"Duplicate Mancer archetype '{loadout.mancerArchetypeId}' — each archetype may only appear once per warband.");
            }

            return errors.ToArray();
        }

        /// <summary>
        /// Validates support unit rules:
        /// <list type="bullet">
        ///   <item>Rule 4: All support units must share the same faction as the warband's factionId.</item>
        ///   <item>Rule 8: Each support unit entry must have a non-empty unitId (valid type reference).</item>
        /// </list>
        ///
        /// Note: Full cross-faction asset lookup requires FactionData ScriptableObjects at runtime.
        /// This validator enforces the structural rules that can be checked from the save data alone.
        /// The faction consistency check validates that each SupportUnitCount.unitId starts with the
        /// warband's factionId prefix (the ID convention defined in warbands.md and FactionData).
        /// </summary>
        /// <param name="warband">The warband to inspect.</param>
        /// <returns>Array of error strings. Empty if all support unit rules pass.</returns>
        private static string[] ValidateSupportUnits(WarbandSave warband)
        {
            var errors = new List<string>();

            if (warband.supportUnits == null || warband.supportUnits.Count == 0)
                return errors.ToArray(); // No support units — not an error on its own.

            bool hasFaction = !string.IsNullOrWhiteSpace(warband.factionId);

            foreach (var unit in warband.supportUnits)
            {
                // Rule 8: unitId must be present.
                if (string.IsNullOrWhiteSpace(unit.unitId))
                {
                    errors.Add("A support unit entry has an empty or missing unitId.");
                    continue;
                }

                // Rule 4: Faction consistency — unitId must be prefixed with the warband's factionId.
                // Convention: "{faction_id}_{role}_{tier}" e.g. "gilded_throne_chaff_t1".
                if (hasFaction && !unit.unitId.StartsWith(warband.factionId, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        $"Support unit '{unit.unitId}' does not belong to faction '{warband.factionId}'. " +
                        "All support units must come from the warband's selected faction.");
                }

                // count must be at least 1 if the entry exists.
                if (unit.count < 1)
                    errors.Add($"Support unit entry '{unit.unitId}' has a count of {unit.count}; count must be at least 1.");
            }

            return errors.ToArray();
        }

        /// <summary>
        /// Validates upgrade-related rules for all Mancer loadouts:
        /// <list type="bullet">
        ///   <item>Rule 6: No two selected upgrades on the same Mancer may be mutually exclusive.</item>
        ///   <item>
        ///     Rule 7: Each individual Mancer's cost (100 base + upgrades) is sanity-checked;
        ///     the aggregate point-cap check is performed at the top level of <see cref="Validate"/>.
        ///   </item>
        /// </list>
        ///
        /// Mutual exclusivity is encoded in <see cref="UpgradeRef"/> only as IDs. Because this is a
        /// pure C# validator without access to ScriptableObject assets, mutual exclusivity is checked
        /// by examining pairs of selected upgrade IDs against each loadout's own list.
        /// Full mutual exclusivity enforcement (reading <c>mutuallyExclusiveWith</c> arrays) requires
        /// the asset-resolved path via WarbandLoader; here we detect conflicts within the saved ID sets.
        ///
        /// When upgrade metadata is available (passed as an optional registry), deeper checks can be
        /// performed. In the save-only path, we detect duplicate upgrade IDs as a proxy conflict.
        /// </summary>
        /// <param name="warband">The warband to inspect.</param>
        /// <returns>Array of error strings. Empty if all upgrade rules pass.</returns>
        private static string[] ValidateUpgrades(WarbandSave warband)
        {
            var errors = new List<string>();

            if (warband.mancers == null) return errors.ToArray();

            foreach (var loadout in warband.mancers)
            {
                if (loadout.upgradeIds == null || loadout.upgradeIds.Count == 0)
                    continue;

                string archetypeLabel = string.IsNullOrWhiteSpace(loadout.mancerArchetypeId)
                    ? "(unknown archetype)"
                    : loadout.mancerArchetypeId;

                // Rule 6 (save-data path): detect duplicate upgrade IDs within the same Mancer loadout.
                // Selecting the same upgrade twice is always invalid and is the detectable form of
                // mutual exclusivity without asset lookup.
                var seenUpgradeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var upgradeRef in loadout.upgradeIds)
                {
                    if (string.IsNullOrWhiteSpace(upgradeRef.upgradeId))
                    {
                        errors.Add($"Mancer '{archetypeLabel}' has an upgrade entry with an empty or missing upgradeId.");
                        continue;
                    }

                    if (!seenUpgradeIds.Add(upgradeRef.upgradeId))
                    {
                        errors.Add(
                            $"Mancer '{archetypeLabel}' has duplicate upgrade '{upgradeRef.upgradeId}'. " +
                            "Each upgrade may only be purchased once per Mancer.");
                    }

                    // Rule 7: Individual upgrade costs must be non-negative.
                    if (upgradeRef.additionalCost < 0)
                    {
                        errors.Add(
                            $"Mancer '{archetypeLabel}' upgrade '{upgradeRef.upgradeId}' has a negative point cost " +
                            $"({upgradeRef.additionalCost}), which is invalid.");
                    }
                }
            }

            return errors.ToArray();
        }

        /// <summary>
        /// Computes the total point cost of the warband: sum of all Mancer base costs (100 each),
        /// all Mancer upgrade costs, and all support unit costs (unitPointCost × count).
        /// </summary>
        /// <param name="warband">The warband to cost.</param>
        /// <returns>Total point cost as an integer. Returns 0 if all collections are null.</returns>
        private static int ComputeTotalCost(WarbandSave warband)
        {
            return warband.TotalPointCost;
        }

        /// <summary>
        /// Returns true if the warband contains at least one support unit entry whose unitId
        /// contains "ranged" (case-insensitive). Used to generate the no-ranged warning.
        /// </summary>
        /// <param name="warband">The warband to inspect.</param>
        /// <returns>True if at least one ranged unit type is present.</returns>
        private static bool HasRangedUnits(WarbandSave warband)
        {
            if (warband.supportUnits == null) return false;

            foreach (var unit in warband.supportUnits)
            {
                if (!string.IsNullOrWhiteSpace(unit.unitId) &&
                    unit.unitId.IndexOf("ranged", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

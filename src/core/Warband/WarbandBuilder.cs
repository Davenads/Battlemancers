using System;
using System.Collections.Generic;
using Battlemancers.Data;

namespace Battlemancers.Core.Warband
{
    /// <summary>
    /// Fluent runtime builder for constructing <see cref="WarbandSave"/> instances step by step.
    ///
    /// Typical usage:
    /// <code>
    /// var (save, result) = new WarbandBuilder()
    ///     .SetFaction("gilded_throne")
    ///     .SetWarbandName("Iron Vanguard Alpha")
    ///     .AddMancer("pyromancer", new[] { "pyromancer_wildfire_spread" })
    ///     .AddMancer("electromancer", Array.Empty&lt;string&gt;())
    ///     .AddSupportUnit("gilded_throne_chaff_t1", isVeteran: false, count: 20, unitPointCost: 10)
    ///     .AddSupportUnit("gilded_throne_ranged_t1", isVeteran: false, count: 8, unitPointCost: 25)
    ///     .Build();
    ///
    /// if (result.IsValid)
    ///     SubmitWarband(save);
    /// </code>
    ///
    /// <see cref="Build"/> always returns both the constructed <see cref="WarbandSave"/> and its
    /// <see cref="ValidationResult"/> — even if invalid — so the caller can inspect what was built.
    ///
    /// Zero Unity dependencies. Safe to use in headless simulation, unit tests, and server-side code.
    /// </summary>
    public sealed class WarbandBuilder
    {
        // -----------------------------------------------------------------------------------------
        // Internal state
        // -----------------------------------------------------------------------------------------

        private string _factionId;
        private string _displayName = "My Warband";

        // Mancer entries: (archetypeId, upgradeIds, upgradeCosts)
        private readonly List<(string archetypeId, string[] upgradeIds, int[] upgradeCosts)> _mancers
            = new List<(string, string[], int[])>();

        // Support unit entries: (unitId, unitPointCost, count)
        private readonly List<(string unitId, int unitPointCost, int count)> _supportUnits
            = new List<(string, int, int)>();

        // Running point total updated as units are added.
        private int _currentPointCost;

        // -----------------------------------------------------------------------------------------
        // Computed properties
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Running total warband point cost as units are added via the fluent API.
        /// Useful for displaying a live budget meter in the warband builder UI.
        /// </summary>
        public int CurrentPointCost => _currentPointCost;

        /// <summary>
        /// Points remaining before the 1,000-point cap is reached.
        /// May be negative if the builder is in an over-budget state.
        /// </summary>
        public int RemainingPoints => WarbandValidator.PointCap - _currentPointCost;

        // -----------------------------------------------------------------------------------------
        // Fluent builder methods
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Sets the faction for the warband being built.
        /// Determines which Chaff and Ranged unit types are legal for this warband.
        /// </summary>
        /// <param name="factionId">
        /// Faction identifier matching a <c>FactionData.factionId</c>.
        /// Valid values: "gilded_throne", "verdant_pact", "ashen_covenant".
        /// </param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="factionId"/> is null.</exception>
        public WarbandBuilder SetFaction(string factionId)
        {
            _factionId = factionId ?? throw new ArgumentNullException(nameof(factionId));
            return this;
        }

        /// <summary>
        /// Sets the player-chosen display name for this warband.
        /// </summary>
        /// <param name="name">Display name shown in the warband manager (e.g., "Flame Rush Build").</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null.</exception>
        public WarbandBuilder SetWarbandName(string name)
        {
            _displayName = name ?? throw new ArgumentNullException(nameof(name));
            return this;
        }

        /// <summary>
        /// Adds a Mancer slot to the warband with zero or more upgrades.
        ///
        /// Each upgrade is specified by its ID and additional point cost. The upgrade cost is
        /// cached in the save (matching the WarbandSave/UpgradeRef contract) so the save file
        /// can compute its total cost without loading ScriptableObject assets.
        ///
        /// Base Mancer cost is always 100 pts. Activation cost remains 100 pts regardless of upgrades.
        /// </summary>
        /// <param name="mancerArchetypeId">
        /// Archetype identifier matching a <c>MancerData.mancerId</c>
        /// (e.g., "pyromancer", "hydromancer").
        /// </param>
        /// <param name="upgradeIds">
        /// IDs of upgrades purchased for this Mancer. Pass an empty array or null for no upgrades.
        /// </param>
        /// <param name="upgradeCosts">
        /// Point cost of each upgrade, parallel-indexed with <paramref name="upgradeIds"/>.
        /// Pass null or empty if <paramref name="upgradeIds"/> is empty.
        /// </param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="mancerArchetypeId"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="upgradeIds"/> and <paramref name="upgradeCosts"/> have different lengths.
        /// </exception>
        public WarbandBuilder AddMancer(string mancerArchetypeId, string[] upgradeIds, int[] upgradeCosts = null)
        {
            if (mancerArchetypeId == null) throw new ArgumentNullException(nameof(mancerArchetypeId));

            upgradeIds = upgradeIds ?? Array.Empty<string>();
            upgradeCosts = upgradeCosts ?? Array.Empty<int>();

            if (upgradeIds.Length != upgradeCosts.Length)
            {
                throw new ArgumentException(
                    $"upgradeIds (length {upgradeIds.Length}) and upgradeCosts (length {upgradeCosts.Length}) must have the same length.");
            }

            _mancers.Add((mancerArchetypeId, upgradeIds, upgradeCosts));

            // Update running cost: 100 base + sum of upgrade costs.
            int mancerCost = 100;
            foreach (int c in upgradeCosts) mancerCost += c;
            _currentPointCost += mancerCost;

            return this;
        }

        /// <summary>
        /// Adds a support unit entry (Chaff or Ranged, T1 or T2) to the warband.
        ///
        /// The <paramref name="unitPointCost"/> is cached in the save so the total cost can be
        /// computed without loading faction ScriptableObject assets.
        /// </summary>
        /// <param name="unitTypeId">
        /// Unit type identifier matching a <c>SupportUnitData.unitId</c> within the selected faction.
        /// Convention: "{faction_id}_{role}_{tier}" e.g., "gilded_throne_chaff_t1".
        /// </param>
        /// <param name="isVeteran">
        /// True if this is a Tier 2 veteran unit; false for Tier 1.
        /// Informational — does not affect how the unit is stored; point cost encodes tier.
        /// </param>
        /// <param name="count">Number of units to purchase. Must be at least 1.</param>
        /// <param name="unitPointCost">
        /// Point cost per unit (T1 Chaff=10, T2 Chaff=20, T1 Ranged=25, T2 Ranged=50).
        /// Cached in the save for cost calculations without asset lookup.
        /// </param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="unitTypeId"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="count"/> is less than 1 or <paramref name="unitPointCost"/> is negative.
        /// </exception>
        public WarbandBuilder AddSupportUnit(string unitTypeId, bool isVeteran, int count = 1, int unitPointCost = 10)
        {
            if (unitTypeId == null) throw new ArgumentNullException(nameof(unitTypeId));
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), "count must be at least 1.");
            if (unitPointCost < 0) throw new ArgumentOutOfRangeException(nameof(unitPointCost), "unitPointCost must be non-negative.");

            // Merge with an existing entry for the same unit type if present.
            int existingIndex = _supportUnits.FindIndex(u =>
                string.Equals(u.unitId, unitTypeId, StringComparison.OrdinalIgnoreCase));

            if (existingIndex >= 0)
            {
                var existing = _supportUnits[existingIndex];
                _supportUnits[existingIndex] = (existing.unitId, existing.unitPointCost, existing.count + count);
            }
            else
            {
                _supportUnits.Add((unitTypeId, unitPointCost, count));
            }

            _currentPointCost += unitPointCost * count;
            return this;
        }

        /// <summary>
        /// Returns true if a unit with the given point cost could be added without exceeding the
        /// 1,000-point cap. Useful for the warband builder UI to grey out unaffordable options.
        /// </summary>
        /// <param name="cost">The point cost of the unit or upgrade being considered.</param>
        /// <returns>True if <see cref="CurrentPointCost"/> + <paramref name="cost"/> ≤ 1,000.</returns>
        public bool CanAfford(int cost)
        {
            return _currentPointCost + cost <= WarbandValidator.PointCap;
        }

        // -----------------------------------------------------------------------------------------
        // Build
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Constructs the <see cref="WarbandSave"/> from all accumulated builder state and
        /// validates it against all warband construction rules.
        ///
        /// The save is returned even when invalid so the caller can inspect what was built and
        /// display granular error messages to the player.
        /// </summary>
        /// <returns>
        /// A tuple of the constructed <see cref="WarbandSave"/> and its <see cref="ValidationResult"/>.
        /// Check <see cref="ValidationResult.IsValid"/> before submitting the warband for a match.
        /// </returns>
        public (WarbandSave warband, ValidationResult result) Build()
        {
            // Assemble the WarbandSave from builder state.
            var save = WarbandSave.CreateNew(_factionId ?? string.Empty, _displayName);

            foreach (var (archetypeId, upgradeIds, upgradeCosts) in _mancers)
            {
                var loadout = new MancerLoadout
                {
                    mancerArchetypeId = archetypeId,
                    upgradeIds = new List<UpgradeRef>()
                };

                for (int i = 0; i < upgradeIds.Length; i++)
                {
                    loadout.upgradeIds.Add(new UpgradeRef
                    {
                        upgradeId = upgradeIds[i],
                        additionalCost = upgradeCosts[i]
                    });
                }

                save.mancers.Add(loadout);
            }

            foreach (var (unitId, unitPointCost, count) in _supportUnits)
            {
                save.supportUnits.Add(new SupportUnitCount
                {
                    unitId = unitId,
                    unitPointCost = unitPointCost,
                    count = count
                });
            }

            save.MarkModified();

            // Validate the assembled save.
            var result = WarbandValidator.Validate(save);
            return (save, result);
        }
    }
}

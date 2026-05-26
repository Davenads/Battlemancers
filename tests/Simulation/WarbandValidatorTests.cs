using System.Collections.Generic;
using NUnit.Framework;
using Battlemancers.Core.Warband;
using Battlemancers.Data;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for WarbandValidator — validates warband construction rules before a match.
    ///
    /// Rules enforced:
    ///   1. Total point cost must not exceed 1,000 pts.
    ///   2. Warband must include at most 3 Mancers.
    ///   3. Warband must include at least 1 Mancer.
    ///   4. Support units must belong to the warband's faction (unitId prefix match).
    ///   5. No duplicate Mancer archetypes.
    ///   6. No duplicate upgrade IDs on the same Mancer (save-data path).
    ///   7. Individual upgrade costs must be non-negative.
    ///   8. Each support unit entry must have a non-empty unitId.
    ///
    /// Non-fatal warnings (IsValid remains true):
    ///   - Total cost below 900 pts.
    ///   - No Ranged units in the warband.
    /// </summary>
    [TestFixture]
    public class WarbandValidatorTests
    {
        private const int PointCap  = WarbandValidator.PointCap;   // 1000
        private const int MaxMancers = WarbandValidator.MaxMancers; // 3
        private const int MinMancers = WarbandValidator.MinMancers; // 1

        // =========================================================================
        // Valid warband passes validation
        // =========================================================================

        [Test]
        public void Validate_ValidMinimalWarband_PassesValidation()
        {
            // Minimal valid warband: 1 Mancer, within budget, valid faction.
            WarbandSave warband = BuildWarband(
                factionId: "gilded_throne",
                mancers: new[] { ("pyromancer", 0) },
                supportUnits: null
            );

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.True,
                "A warband with exactly 1 Mancer and no support units should pass validation.");
        }

        [Test]
        public void Validate_ValidWarbandWithThreeMancers_PassesValidation()
        {
            // Maximum 3 Mancers, all distinct archetypes, within budget.
            WarbandSave warband = BuildWarband(
                factionId: "verdant_pact",
                mancers: new[] { ("pyromancer", 0), ("hydromancer", 0), ("cryomancer", 0) },
                supportUnits: null
            );

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.True,
                "A warband with 3 distinct Mancers within budget should pass validation.");
        }

        [Test]
        public void Validate_ValidWarbandAtExactPointCap_PassesValidation()
        {
            // 3 Mancers (300 pts base) + 7 T2 Chaff × 20 pts each = 300 + 700 * (1 entry of count=35) works.
            // Simpler: 3 Mancers (300) + support unit at 700 pts total.
            // Use a single support unit with unitPointCost=700 and count=1 for simplicity.
            WarbandSave warband = BuildWarband(
                factionId: "gilded_throne",
                mancers: new[] { ("pyromancer", 0), ("hydromancer", 0), ("electromancer", 0) },
                supportUnits: new[] { ("gilded_throne_ranged_t2", 50, 14) } // 50*14 = 700; total = 300+700 = 1000
            );

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.True,
                $"A warband totalling exactly {PointCap} pts should pass validation.");
            Assert.That(result.TotalPointCost, Is.EqualTo(PointCap),
                $"Total cost should be exactly {PointCap}.");
        }

        // =========================================================================
        // Over 1,000 pts fails
        // =========================================================================

        [Test]
        public void Validate_Over1000Pts_FailsValidation()
        {
            // 3 Mancers (300) + support totalling 701 pts → 1001 total > 1000.
            WarbandSave warband = BuildWarband(
                factionId: "gilded_throne",
                mancers: new[] { ("pyromancer", 0), ("hydromancer", 0), ("electromancer", 0) },
                supportUnits: new[] { ("gilded_throne_ranged_t2", 50, 15) } // 50*15=750; 300+750=1050 > 1000
            );

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.False,
                "A warband exceeding 1,000 pts must fail validation.");
            Assert.That(result.Errors.Length, Is.GreaterThan(0),
                "At least one error message must be present when the point cap is exceeded.");
        }

        [Test]
        public void Validate_ExactlyOnePointOver_FailsValidation()
        {
            // 1 Mancer (100) + support at 901 pts → 1001 > 1000.
            WarbandSave warband = BuildWarband(
                factionId: "gilded_throne",
                mancers: new[] { ("pyromancer", 0) },
                supportUnits: new[] { ("gilded_throne_ranged_t2", 901, 1) } // 901; 100+901=1001
            );

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.False,
                "1001 pts (1 over the cap) must fail validation.");
        }

        // =========================================================================
        // More than 3 Mancers fails
        // =========================================================================

        [Test]
        public void Validate_FourMancers_FailsValidation()
        {
            WarbandSave warband = BuildWarband(
                factionId: "ashen_covenant",
                mancers: new[]
                {
                    ("pyromancer", 0),
                    ("hydromancer", 0),
                    ("cryomancer", 0),
                    ("electromancer", 0) // 4th Mancer — illegal
                },
                supportUnits: null
            );

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.False,
                "A warband with 4 Mancers must fail validation (maximum is 3).");
        }

        // =========================================================================
        // Zero Mancers fails
        // =========================================================================

        [Test]
        public void Validate_NoMancers_FailsValidation()
        {
            WarbandSave warband = BuildWarband(
                factionId: "gilded_throne",
                mancers: new (string, int)[0], // empty roster
                supportUnits: null
            );

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.False,
                "A warband with no Mancers must fail validation (minimum is 1).");
        }

        // =========================================================================
        // Duplicate Mancer archetype fails
        // =========================================================================

        [Test]
        public void Validate_DuplicateMancerArchetype_FailsValidation()
        {
            WarbandSave warband = BuildWarband(
                factionId: "verdant_pact",
                mancers: new[] { ("pyromancer", 0), ("pyromancer", 0) }, // duplicate
                supportUnits: null
            );

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.False,
                "A warband with duplicate Mancer archetypes must fail validation.");
        }

        // =========================================================================
        // Cross-faction support unit fails
        // =========================================================================

        [Test]
        public void Validate_CrossFactionSupportUnit_FailsValidation()
        {
            // Faction is "verdant_pact" but support unit belongs to "gilded_throne".
            WarbandSave warband = BuildWarband(
                factionId: "verdant_pact",
                mancers: new[] { ("pyromancer", 0) },
                supportUnits: new[] { ("gilded_throne_chaff_t1", 10, 2) } // wrong faction prefix
            );

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.False,
                "Support units from a different faction must fail validation.");
        }

        // =========================================================================
        // Duplicate upgrade on same Mancer fails
        // =========================================================================

        [Test]
        public void Validate_DuplicateUpgradeOnMancer_FailsValidation()
        {
            var warband = WarbandSave.CreateNew("gilded_throne");
            warband.mancers.Add(new MancerLoadout
            {
                mancerArchetypeId = "pyromancer",
                upgradeIds = new List<UpgradeRef>
                {
                    new UpgradeRef { upgradeId = "pyro_upgrade_inferno", additionalCost = 50 },
                    new UpgradeRef { upgradeId = "pyro_upgrade_inferno", additionalCost = 50 } // duplicate
                }
            });

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.False,
                "A Mancer with duplicate upgrade IDs must fail validation.");
        }

        // =========================================================================
        // Support unit with empty unitId fails
        // =========================================================================

        [Test]
        public void Validate_SupportUnitWithEmptyId_FailsValidation()
        {
            var warband = WarbandSave.CreateNew("gilded_throne");
            warband.mancers.Add(new MancerLoadout { mancerArchetypeId = "pyromancer" });
            warband.supportUnits.Add(new SupportUnitCount
            {
                unitId       = "",  // empty — invalid
                unitPointCost = 10,
                count        = 1
            });

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.False,
                "A support unit entry with an empty unitId must fail validation.");
        }

        // =========================================================================
        // Non-fatal warnings (IsValid still true)
        // =========================================================================

        [Test]
        public void Validate_UnderbudgetWarband_IsValidWithWarning()
        {
            // 1 Mancer = 100 pts. Under the 900-pt warning threshold but still valid.
            WarbandSave warband = BuildWarband(
                factionId: "gilded_throne",
                mancers: new[] { ("pyromancer", 0) },
                supportUnits: null
            );

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.True,
                "An underbudget warband should still pass validation (warning only, not error).");
            Assert.That(result.Warnings.Length, Is.GreaterThan(0),
                "An underbudget warband should produce at least one warning.");
        }

        [Test]
        public void Validate_NoRangedUnits_IsValidWithWarning()
        {
            // Warband with only chaff support units and no ranged — generates a warning.
            WarbandSave warband = BuildWarband(
                factionId: "gilded_throne",
                mancers: new[] { ("pyromancer", 0) },
                supportUnits: new[] { ("gilded_throne_chaff_t1", 10, 5) }
            );

            ValidationResult result = WarbandValidator.Validate(warband);

            Assert.That(result.IsValid, Is.True,
                "A warband without Ranged units should still pass validation.");
            Assert.That(result.Warnings.Length, Is.GreaterThan(0),
                "Absence of Ranged units should generate at least one warning.");
        }

        // =========================================================================
        // ValidationResult properties
        // =========================================================================

        [Test]
        public void Validate_ValidWarband_TotalCostIsCorrect()
        {
            // 2 Mancers (200 pts) + 10 chaff at 10 pts each (100 pts) = 300 pts total.
            WarbandSave warband = BuildWarband(
                factionId: "gilded_throne",
                mancers: new[] { ("pyromancer", 0), ("hydromancer", 0) },
                supportUnits: new[] { ("gilded_throne_chaff_t1", 10, 10) }
            );

            ValidationResult result = WarbandValidator.Validate(warband);

            const int expectedCost = 200 + 100; // 2 Mancers + 10 chaff
            Assert.That(result.TotalPointCost, Is.EqualTo(expectedCost),
                "ValidationResult.TotalPointCost must equal the actual computed warband cost.");
        }

        [Test]
        public void Validate_WarbandWithUpgrades_TotalCostIncludesUpgradeCosts()
        {
            // 1 Mancer (100 base) + 1 upgrade at 50 pts = 150 pts total.
            var warband = WarbandSave.CreateNew("gilded_throne");
            warband.mancers.Add(new MancerLoadout
            {
                mancerArchetypeId = "pyromancer",
                upgradeIds = new List<UpgradeRef>
                {
                    new UpgradeRef { upgradeId = "pyro_upgrade_inferno", additionalCost = 50 }
                }
            });

            ValidationResult result = WarbandValidator.Validate(warband);

            const int expectedCost = 100 + 50; // base + upgrade
            Assert.That(result.TotalPointCost, Is.EqualTo(expectedCost),
                "TotalPointCost must include Mancer upgrade costs.");
        }

        // =========================================================================
        // Null guard
        // =========================================================================

        [Test]
        public void Validate_NullWarband_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => WarbandValidator.Validate(null),
                "Validate(null) must throw ArgumentNullException.");
        }

        // =========================================================================
        // Helpers
        // =========================================================================

        /// <summary>
        /// Builds a WarbandSave from a compact description.
        /// </summary>
        /// <param name="factionId">Faction identifier for the warband.</param>
        /// <param name="mancers">
        /// Array of (archetypeId, upgradeAdditionalCost) tuples.
        /// Each entry generates one MancerLoadout with zero or one upgrade.
        /// </param>
        /// <param name="supportUnits">
        /// Array of (unitId, unitPointCost, count) tuples, or null for no support units.
        /// </param>
        private static WarbandSave BuildWarband(
            string factionId,
            (string archetypeId, int upgradeAdditionalCost)[] mancers,
            (string unitId, int unitPointCost, int count)[] supportUnits)
        {
            var warband = WarbandSave.CreateNew(factionId);

            foreach (var (archetypeId, upgradeCost) in mancers)
            {
                var loadout = new MancerLoadout { mancerArchetypeId = archetypeId };
                if (upgradeCost > 0)
                {
                    loadout.upgradeIds.Add(new UpgradeRef
                    {
                        upgradeId      = $"{archetypeId}_upgrade_default",
                        additionalCost = upgradeCost
                    });
                }
                warband.mancers.Add(loadout);
            }

            if (supportUnits != null)
            {
                foreach (var (unitId, unitPointCost, count) in supportUnits)
                {
                    warband.supportUnits.Add(new SupportUnitCount
                    {
                        unitId        = unitId,
                        unitPointCost = unitPointCost,
                        count         = count
                    });
                }
            }

            return warband;
        }
    }
}

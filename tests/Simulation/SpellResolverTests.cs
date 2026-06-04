using System;
using NUnit.Framework;
using UnityEngine;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Data;
using Battlemancers.Simulation;
using Battlemancers.Simulation.Effects;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// NUnit tests for <see cref="SpellResolver"/> and <see cref="SpellEffectApplicator"/>.
    ///
    /// Tests verify the five core behaviours mandated by the spell-resolver agent spec:
    /// <list type="number">
    ///   <item>Basic damage spells reduce target HP.</item>
    ///   <item>Silenced casters cannot cast (WasCast = false from SpellEffectApplicator).</item>
    ///   <item>Status effects are applied at chance 1.0 and skipped at chance 0.0.</item>
    ///   <item>Temperature delta is captured in the SpellResolutionResult.</item>
    ///   <item>Lightning cast on a Wet target generates a ComboEffect in the result.</item>
    /// </list>
    ///
    /// Setup: caster at (0,0), enemy target at (2,0) within SingleTarget range.
    /// All setups use <see cref="GridData.Standard24x24()"/> and two registered players.
    /// </summary>
    [TestFixture]
    public class SpellResolverTests
    {
        // -----------------------------------------------------------------------------------------
        // Named constants — no magic numbers
        // -----------------------------------------------------------------------------------------

        private const string Player1 = "p1";
        private const string Player2 = "p2";

        private const string CasterId = "p1_mancer_0";
        private const string TargetId = "p2_mancer_0";

        private static readonly GridPosition CasterPos = new GridPosition(0, 0);
        private static readonly GridPosition TargetPos  = new GridPosition(2, 0);

        private const int DefaultMaxHP     = 100;
        private const int DefaultMoveRange = 4;
        private const int DefaultPointCost = 100;

        private const int SpellRange   = 5;
        private const int SpellApCost  = 1;

        /// <summary>Base damage used in damage-path tests.</summary>
        private const int BaseDamage20 = 20;

        /// <summary>Temperature delta for warm spells.</summary>
        private const int TempDelta15 = 15;

        // -----------------------------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------------------------

        private GridData           _grid;
        private SimulationState    _state;
        private StatusManager      _statusManager;
        private TemperatureManager _tempManager;
        private ElementResolver    _elementResolver;

        // The two systems under test.
        private SpellResolver        _spellResolver;
        private SpellEffectApplicator _applicator;

        // -----------------------------------------------------------------------------------------
        // Setup / Teardown
        // -----------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _grid            = GridData.Standard24x24();
            _state           = new SimulationState(_grid, new[] { Player1, Player2 });
            _state.Phase     = TurnPhase.Resolving;
            _statusManager   = new StatusManager();
            _tempManager     = new TemperatureManager(_statusManager);
            _elementResolver = ElementResolver.CreateDefault();

            _spellResolver = new SpellResolver(_elementResolver, _statusManager, _tempManager);

            // SpellEffectApplicator uses a seeded Random for determinism in tests.
            _applicator = new SpellEffectApplicator(
                _statusManager,
                _tempManager,
                _elementResolver,
                new Random(seed: 12345));

            SimulationEventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SimulationEventBus.Clear();
        }

        // -----------------------------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Creates and registers a Mancer unit with full HP at the specified position.
        /// </summary>
        private UnitState MakeAndRegisterUnit(
            string id, string ownerId, GridPosition pos, int hp = DefaultMaxHP)
        {
            var unit = new UnitState(
                id:                id,
                mancerArchetypeId: "testmancer",
                type:              UnitType.Mancer,
                ownerId:           ownerId,
                position:          pos,
                maxHP:             hp,
                moveRange:         DefaultMoveRange,
                pointCost:         DefaultPointCost);

            _state.RegisterUnit(unit);
            return unit;
        }

        /// <summary>
        /// Creates a minimal <see cref="SpellData"/> ScriptableObject suitable for headless
        /// Unity test runner execution. Uses SingleTarget targeting and no AoE radius.
        /// </summary>
        private static SpellData MakeSpell(
            string spellId,
            int baseDamage                 = 0,
            int temperatureDelta           = 0,
            ElementType element            = ElementType.Fire,
            StatusEffectApplication[] effects = null,
            ConditionalDamageBonus[] conditionalBonuses = null)
        {
            var spell = ScriptableObject.CreateInstance<SpellData>();
            spell.spellId            = spellId;
            spell.apCost             = SpellApCost;
            spell.cooldownTurns      = 0;
            spell.targetType         = SpellTargetType.SingleTarget;
            spell.range              = SpellRange;
            spell.aoeRadius          = 0;
            spell.baseDamage         = baseDamage;
            spell.element            = element;
            spell.temperatureDelta   = temperatureDelta;
            spell.appliedEffects     = effects           ?? Array.Empty<StatusEffectApplication>();
            spell.conditionalBonuses = conditionalBonuses ?? Array.Empty<ConditionalDamageBonus>();
            return spell;
        }

        /// <summary>
        /// Creates a <see cref="StatusEffectApplication"/> for a unit-targeted status.
        /// </summary>
        private static StatusEffectApplication MakeStatusApplication(
            string statusType,
            int duration          = 2,
            float applicationChance = 1.0f,
            int stacksApplied     = 1)
        {
            return new StatusEffectApplication
            {
                statusType        = statusType,
                duration          = duration,
                stacksApplied     = stacksApplied,
                appliesToTile     = false,
                applicationChance = applicationChance
            };
        }

        // =========================================================================
        // Test 1 — Basic damage spell reduces target HP
        // =========================================================================

        /// <summary>
        /// A spell with baseDamage = 20 cast on an enemy reduces that enemy's HP by 20.
        /// Verifies the primary damage path in <see cref="SpellResolver.Resolve"/>.
        /// </summary>
        [Test]
        public void Resolve_BasicDamageSpell_ReducesTargetHP()
        {
            // Arrange
            MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId, Player2, TargetPos);

            SpellData spell = MakeSpell("test_dmg_spell", baseDamage: BaseDamage20);

            // Act
            SpellResult result = _spellResolver.Resolve(CasterId, spell, TargetPos, _state);

            // Assert
            Assert.That(result, Is.Not.Null, "SpellResolver must return a non-null SpellResult for a valid cast.");
            Assert.That(target.CurrentHP, Is.EqualTo(DefaultMaxHP - BaseDamage20),
                $"Target HP must be reduced by baseDamage ({BaseDamage20}) from {DefaultMaxHP} to {DefaultMaxHP - BaseDamage20}.");
        }

        // =========================================================================
        // Test 2 — Silenced caster cannot cast
        // =========================================================================

        /// <summary>
        /// When the caster has a SILENCED status, <see cref="SpellEffectApplicator.Apply"/>
        /// returns a blocked result (WasCast = false) and no damage is applied to the target.
        /// </summary>
        [Test]
        public void Resolve_SilencedCaster_ReturnsWasCastFalse()
        {
            // Arrange
            UnitState caster = MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId, Player2, TargetPos);

            // Apply SILENCED to the caster.
            var silenced = new StatusEffect(StatusType.Silenced, duration: 2, stackCount: 1, sourceId: "test");
            _statusManager.ApplyStatus(CasterId, silenced, caster, _state.TurnNumber);

            SpellData spell = MakeSpell("test_silenced_spell", baseDamage: BaseDamage20);

            // Act — use SpellEffectApplicator which gates on caster status.
            var targets = new System.Collections.Generic.List<UnitState> { target };
            SpellResolutionResult result = _applicator.Apply(spell, caster, targets, _state);

            // Assert
            Assert.That(result.WasCast, Is.False,
                "WasCast must be false when the caster is SILENCED.");
            Assert.That(target.CurrentHP, Is.EqualTo(DefaultMaxHP),
                "Target HP must be unchanged when the caster is SILENCED.");
            Assert.That(result.DamageDealt, Is.Empty,
                "No DamageDealt entries must exist in a blocked result.");
        }

        // =========================================================================
        // Test 3 — Status effect application respects applicationChance
        // =========================================================================

        /// <summary>
        /// At applicationChance = 1.0, the status is always applied.
        /// At applicationChance = 0.0, the status is never applied.
        /// Verifies <see cref="SpellEffectApplicator"/> gates on the chance correctly.
        /// </summary>
        [Test]
        public void Resolve_SpellWithStatusEffect_AppliesStatusOnChance()
        {
            // --- Sub-case A: applicationChance = 1.0 → status MUST be applied ---
            {
                UnitState casterA = MakeAndRegisterUnit(CasterId + "_a", Player1, new GridPosition(0, 1));
                UnitState targetA = MakeAndRegisterUnit(TargetId + "_a", Player2, new GridPosition(2, 1));

                StatusEffectApplication application = MakeStatusApplication(
                    statusType:         "Burning",
                    duration:           2,
                    applicationChance:  1.0f);

                SpellData spell = MakeSpell(
                    "test_chance_1",
                    baseDamage: 0,
                    effects: new[] { application });

                var targets = new System.Collections.Generic.List<UnitState> { targetA };
                SpellResolutionResult result = _applicator.Apply(spell, casterA, targets, _state);

                Assert.That(result.WasCast, Is.True,
                    "WasCast must be true for a valid cast (applicationChance=1.0).");
                Assert.That(result.StatusesApplied, Has.Count.EqualTo(1),
                    "One status must be applied when applicationChance=1.0.");
                Assert.That(result.StatusesApplied[0].StatusName, Is.EqualTo("Burning"),
                    "The applied status name must match the configured statusType.");
                Assert.That(
                    _statusManager.HasStatus(targetA.Id, StatusType.Burning),
                    Is.True,
                    "StatusManager must record the Burning status on the target.");
            }

            // --- Sub-case B: applicationChance = 0.0 → status must NOT be applied ---
            {
                UnitState casterB = MakeAndRegisterUnit(CasterId + "_b", Player1, new GridPosition(0, 2));
                UnitState targetB = MakeAndRegisterUnit(TargetId + "_b", Player2, new GridPosition(2, 2));

                StatusEffectApplication application = MakeStatusApplication(
                    statusType:         "Poisoned",
                    duration:           2,
                    applicationChance:  0.0f);

                SpellData spell = MakeSpell(
                    "test_chance_0",
                    baseDamage: 0,
                    effects: new[] { application });

                var targets = new System.Collections.Generic.List<UnitState> { targetB };
                SpellResolutionResult result = _applicator.Apply(spell, casterB, targets, _state);

                Assert.That(result.WasCast, Is.True,
                    "WasCast must be true even when no status is applied (cast still succeeded).");
                Assert.That(result.StatusesApplied, Is.Empty,
                    "No statuses must be applied when applicationChance=0.0.");
                Assert.That(
                    _statusManager.HasStatus(targetB.Id, StatusType.Poisoned),
                    Is.False,
                    "StatusManager must NOT record Poisoned when applicationChance=0.0.");
            }
        }

        // =========================================================================
        // Test 4 — Temperature delta is captured in SpellResolutionResult
        // =========================================================================

        /// <summary>
        /// A spell with temperatureDelta ≠ 0 must produce a <see cref="TemperatureEvent"/>
        /// in <see cref="SpellResolutionResult.TemperatureChanges"/> with the correct delta
        /// and updated temperature values.
        /// </summary>
        [Test]
        public void Resolve_TemperatureDelta_PassedToTemperatureManager()
        {
            // Arrange
            UnitState caster = MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId, Player2, TargetPos);

            const int expectedDelta = TempDelta15;
            SpellData spell = MakeSpell("test_temp_spell", temperatureDelta: expectedDelta);

            var targets = new System.Collections.Generic.List<UnitState> { target };

            // Act
            SpellResolutionResult result = _applicator.Apply(spell, caster, targets, _state);

            // Assert — result contains a temperature event with matching delta.
            Assert.That(result.WasCast, Is.True, "WasCast must be true for a valid cast.");
            Assert.That(result.TemperatureChanges, Has.Count.EqualTo(1),
                "Exactly one TemperatureEvent must be present when temperatureDelta != 0.");

            TemperatureEvent tempEvent = result.TemperatureChanges[0];

            Assert.That(tempEvent.TargetId, Is.EqualTo(TargetId),
                "TemperatureEvent must reference the correct target unit.");
            Assert.That(tempEvent.Delta, Is.EqualTo(expectedDelta),
                $"TemperatureEvent.Delta must equal the spell's temperatureDelta ({expectedDelta}).");
            Assert.That(tempEvent.PreviousTemperature, Is.EqualTo(0),
                "PreviousTemperature must be 0 when the target starts at neutral.");
            Assert.That(tempEvent.NewTemperature, Is.EqualTo(expectedDelta),
                $"NewTemperature must equal the delta ({expectedDelta}) applied to a neutral unit.");

            // Also verify the live UnitState was updated.
            Assert.That(target.Temperature, Is.EqualTo(expectedDelta),
                "UnitState.Temperature must reflect the applied delta.");
        }

        // =========================================================================
        // Test 5 — Lightning on a Wet target generates a ComboEffect
        // =========================================================================

        /// <summary>
        /// When a Lightning spell is cast on a target whose tile is in the Wet state,
        /// the element-interaction resolver fires a "chain_arc" combo and
        /// <see cref="SpellResolutionResult.ComboEffects"/> contains one entry.
        /// Verifies the combo detection path in <see cref="SpellEffectApplicator"/>.
        /// </summary>
        [Test]
        public void Resolve_ElementCombo_LightningOnWetTarget_GeneratesComboEffect()
        {
            // Arrange
            UnitState caster = MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId, Player2, TargetPos);

            // Set the target's tile to Wet so the Lightning+Wet combo triggers.
            _state.Grid.SetTileState(TargetPos, TileState.Wet);

            SpellData spell = MakeSpell(
                "test_lightning_spell",
                baseDamage:  0,
                element:     ElementType.Lightning);

            var targets = new System.Collections.Generic.List<UnitState> { target };

            // Act
            SpellResolutionResult result = _applicator.Apply(spell, caster, targets, _state);

            // Assert
            Assert.That(result.WasCast, Is.True, "WasCast must be true for a valid cast.");
            Assert.That(result.ComboEffects, Is.Not.Empty,
                "A ComboEffect must be generated when Lightning hits a Wet tile.");

            ComboEffect combo = result.ComboEffects[0];

            Assert.That(combo.TriggerStateName, Is.EqualTo("Wet"),
                "The trigger state must be 'Wet'.");
            Assert.That(combo.TriggerElementName, Is.EqualTo("Lightning"),
                "The trigger element must be 'Lightning'.");
            Assert.That(combo.ComboName, Is.EqualTo("chain_arc"),
                "The combo VFX hint for Wet+Lightning must be 'chain_arc'.");
        }
    }
}

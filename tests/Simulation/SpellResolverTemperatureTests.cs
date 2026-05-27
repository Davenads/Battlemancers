using NUnit.Framework;
using UnityEngine;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Data;
using Battlemancers.Simulation;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for the temperature integration path inside SpellResolver.
    ///
    /// Verifies that:
    ///   1. A spell with a positive temperatureDelta heats the target via TemperatureManager.
    ///   2. A spell with temperatureDelta = 0 skips the TemperatureManager call entirely.
    ///   3. A spell with a negative temperatureDelta cools the target.
    ///   4. Casting at a tile occupied only by a dead (HP = 0) unit is a no-op for temperature
    ///      because TargetingUtils.GetUnitsInTiles only returns alive units.
    ///
    /// Setup mirrors TemperatureManagerTests: caster at (0,0), enemy target at (2,0)
    /// (distance = 2, within SingleTarget range 5). All spells use baseDamage = 0 so
    /// damage cannot kill the target and confound temperature assertions.
    /// </summary>
    [TestFixture]
    public class SpellResolverTemperatureTests
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        private const string Player1 = "p1";
        private const string Player2 = "p2";

        private const string CasterId = "p1_thermo_0";
        private const string TargetId = "p2_hydro_0";

        /// <summary>
        /// Position of the caster. Kept near grid origin so spells always reach the target.
        /// </summary>
        private static readonly GridPosition CasterPos = new GridPosition(0, 0);

        /// <summary>
        /// Position of the target — 2 tiles away (Manhattan), safely within spell range.
        /// </summary>
        private static readonly GridPosition TargetPos = new GridPosition(2, 0);

        // ---------------------------------------------------------------------------
        // Fields
        // ---------------------------------------------------------------------------

        private GridData           _grid;
        private SimulationState    _state;
        private StatusManager      _statusManager;
        private TemperatureManager _tempManager;
        private ElementResolver    _elementResolver;
        private SpellResolver      _spellResolver;

        // ---------------------------------------------------------------------------
        // Setup / Teardown
        // ---------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _grid            = GridData.Standard24x24();
            _state           = new SimulationState(_grid, new[] { Player1, Player2 });
            _state.Phase     = TurnPhase.Resolving;
            _statusManager   = new StatusManager();
            _tempManager     = new TemperatureManager(_statusManager);
            _elementResolver = ElementResolver.CreateDefault();
            _spellResolver   = new SpellResolver(_elementResolver, _statusManager, _tempManager);
            SimulationEventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SimulationEventBus.Clear();
        }

        // ---------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Creates and registers a Mancer unit with full HP at the given position.
        /// </summary>
        private UnitState MakeAndRegisterUnit(string id, string ownerId, GridPosition pos, int hp = 100)
        {
            var unit = new UnitState(
                id:                id,
                mancerArchetypeId: "thermomancer",
                type:              UnitType.Mancer,
                ownerId:           ownerId,
                position:          pos,
                maxHP:             hp,
                moveRange:         4,
                pointCost:         100
            );
            _state.RegisterUnit(unit);
            return unit;
        }

        /// <summary>
        /// Creates a minimal SpellData ScriptableObject suitable for headless testing.
        /// Uses SingleTarget (no AoE) with zero base damage so damage cannot confound
        /// temperature assertions.
        /// </summary>
        private static SpellData MakeSpell(string spellId, int temperatureDelta)
        {
            var spell = ScriptableObject.CreateInstance<SpellData>();
            spell.spellId         = spellId;
            spell.apCost          = 1;
            spell.cooldownTurns   = 0;
            spell.targetType      = SpellTargetType.SingleTarget;
            spell.range           = 5;
            spell.aoeRadius       = 0;
            spell.baseDamage      = 0;   // no damage — temperature is the only effect under test
            spell.element         = ElementType.Fire;
            spell.temperatureDelta = temperatureDelta;
            spell.appliedEffects  = System.Array.Empty<StatusEffectApplication>();
            spell.conditionalBonuses = System.Array.Empty<ConditionalDamageBonus>();
            return spell;
        }

        // =========================================================================
        // Test 1: Positive delta heats the target
        // =========================================================================

        [Test]
        public void Resolve_PositiveTemperatureDelta_HeatsTarget()
        {
            // Arrange
            MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId, Player2, TargetPos);

            SpellData spell = MakeSpell("test_heat_bolt", temperatureDelta: 15);

            // Act
            SpellResult result = _spellResolver.Resolve(CasterId, spell, TargetPos, _state);

            // Assert
            Assert.That(result, Is.Not.Null, "Resolve must return a non-null SpellResult when the cast is valid.");
            Assert.That(target.Temperature, Is.EqualTo(15),
                "A spell with temperatureDelta=+15 must raise the target's temperature by 15.");
        }

        // =========================================================================
        // Test 2: Zero delta skips TemperatureManager call
        // =========================================================================

        [Test]
        public void Resolve_ZeroTemperatureDelta_LeavesTargetTemperatureUnchanged()
        {
            // Arrange
            MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId, Player2, TargetPos);

            SpellData spell = MakeSpell("test_no_temp_spell", temperatureDelta: 0);

            // Act
            SpellResult result = _spellResolver.Resolve(CasterId, spell, TargetPos, _state);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(target.Temperature, Is.EqualTo(0),
                "A spell with temperatureDelta=0 must not change the target's temperature.");
        }

        // =========================================================================
        // Test 3: Negative delta cools the target
        // =========================================================================

        [Test]
        public void Resolve_NegativeTemperatureDelta_CoolsTarget()
        {
            // Arrange
            MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId, Player2, TargetPos);

            SpellData spell = MakeSpell("test_chill_bolt", temperatureDelta: -15);

            // Act
            SpellResult result = _spellResolver.Resolve(CasterId, spell, TargetPos, _state);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(target.Temperature, Is.EqualTo(-15),
                "A spell with temperatureDelta=-15 must reduce the target's temperature by 15.");
        }

        // =========================================================================
        // Test 4: Targeting a dead unit is a no-op for temperature
        // =========================================================================

        [Test]
        public void Resolve_TargetingDeadUnit_NoTemperatureApplied()
        {
            // Arrange — caster is alive; the unit at the target tile has 0 HP (dead).
            // TargetingUtils.GetUnitsInTiles filters by unit.IsAlive, so dead units
            // are excluded from hitUnits and temperature is never applied to them.
            MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState deadTarget = MakeAndRegisterUnit(TargetId, Player2, TargetPos);
            deadTarget.CurrentHP = 0; // kill the unit without deregistering it

            Assert.That(deadTarget.IsAlive, Is.False,
                "Pre-condition: the target unit must be dead before the spell resolves.");

            SpellData spell = MakeSpell("test_heat_bolt_dead", temperatureDelta: 15);

            // Act
            _spellResolver.Resolve(CasterId, spell, TargetPos, _state);

            // Assert — dead unit was not returned by GetUnitsInTiles, so no temperature change.
            Assert.That(deadTarget.Temperature, Is.EqualTo(0),
                "Temperature must not be applied to a dead (HP=0) unit — it is excluded from targeting.");
        }
    }
}

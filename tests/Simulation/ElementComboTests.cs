using System;
using System.Collections.Generic;
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
    /// Integration tests that verify the element combo system fires correctly end-to-end
    /// through <see cref="SpellEffectApplicator"/>.
    ///
    /// Concern under test: <see cref="SpellEffectApplicator.Apply"/> calls
    /// <see cref="ElementResolver.HasInteraction"/> when resolving spells against tile states.
    /// These tests confirm that the correct <see cref="ComboEffect"/> entry appears in
    /// <see cref="SpellResolutionResult.ComboEffects"/> for each combo pair, and that
    /// the <see cref="ComboEffect.ComboName"/> matches the exact vfxHint value in
    /// assets/data/element-interactions.json (loaded via the embedded fallback in
    /// <see cref="ElementResolver.CreateDefault"/>).
    ///
    /// ElementResolver state: CreateDefault() uses an embedded JSON string — no file I/O
    /// is required. The resolver is fully operational in headless NUnit tests.
    ///
    /// Combo detection path: SpellEffectApplicator reads the TARGET TILE'S TileState, not
    /// the unit's StatusType. To trigger a combo, the tile at the target's GridPosition must
    /// be set to the matching TileState using Grid.SetTileState() before the spell is cast.
    ///
    /// Setup matches SpellResolverTests.cs exactly: caster at (0,0), target at (2,0),
    /// GridData.Standard24x24(), two registered players, seeded Random(12345).
    /// </summary>
    [TestFixture]
    public class ElementComboTests
    {
        // -----------------------------------------------------------------------------------------
        // Named constants
        // -----------------------------------------------------------------------------------------

        private const string Player1 = "p1";
        private const string Player2 = "p2";

        private const string CasterId = "p1_mancer_0";
        private const string TargetId  = "p2_mancer_0";

        private static readonly GridPosition CasterPos = new GridPosition(0, 0);
        private static readonly GridPosition TargetPos  = new GridPosition(2, 0);

        private const int DefaultMaxHP     = 100;
        private const int DefaultMoveRange = 4;
        private const int DefaultPointCost = 100;

        private const int SpellRange  = 5;
        private const int SpellApCost = 1;

        // -----------------------------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------------------------

        private GridData           _grid;
        private SimulationState    _state;
        private StatusManager      _statusManager;
        private TemperatureManager _tempManager;
        private ElementResolver    _elementResolver;
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

            // CreateDefault() loads the full interaction table from the embedded JSON fallback —
            // no external file access required in headless tests.
            _elementResolver = ElementResolver.CreateDefault();

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
        /// Creates a minimal <see cref="SpellData"/> ScriptableObject for headless tests.
        /// Matches the factory used in SpellResolverTests.cs exactly.
        /// </summary>
        private static SpellData MakeSpell(
            string spellId,
            ElementType element,
            int baseDamage = 0)
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
            spell.temperatureDelta   = 0;
            spell.appliedEffects     = Array.Empty<StatusEffectApplication>();
            spell.conditionalBonuses = Array.Empty<ConditionalDamageBonus>();
            return spell;
        }

        /// <summary>
        /// Runs the applicator and returns the first ComboEffect, or null if none fired.
        /// </summary>
        private ComboEffect ApplyAndGetFirstCombo(SpellData spell, UnitState caster, UnitState target)
        {
            var targets = new List<UnitState> { target };
            SpellResolutionResult result = _applicator.Apply(spell, caster, targets, _state);
            Assert.That(result.WasCast, Is.True, "WasCast must be true for a valid cast.");
            return result.ComboEffects.Count > 0 ? result.ComboEffects[0] : null;
        }

        // =========================================================================
        // Test 1 — Lightning + Wet → chain_arc
        // =========================================================================

        /// <summary>
        /// Lightning spell hitting a Wet tile produces a ComboEffect with name "chain_arc".
        /// JSON entry: tileState="Wet", element="Lightning", vfxHint="chain_arc".
        /// </summary>
        [Test]
        public void Resolve_LightningOnWetTarget_GeneratesChainArcCombo()
        {
            // Arrange
            UnitState caster = MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId,  Player2, TargetPos);

            _state.Grid.SetTileState(TargetPos, TileState.Wet);

            SpellData spell = MakeSpell("lightning_wet", ElementType.Lightning);
            var targets = new List<UnitState> { target };

            // Act
            SpellResolutionResult result = _applicator.Apply(spell, caster, targets, _state);

            // Assert
            Assert.That(result.WasCast, Is.True,
                "WasCast must be true for a valid cast.");
            Assert.That(result.ComboEffects, Is.Not.Empty,
                "A ComboEffect must fire when Lightning strikes a Wet tile.");

            ComboEffect combo = result.ComboEffects[0];

            Assert.That(combo.ComboName, Is.EqualTo("chain_arc"),
                "ComboName must match the vfxHint 'chain_arc' from the interaction table.");
            Assert.That(combo.TriggerStateName, Is.EqualTo("Wet"),
                "TriggerStateName must be 'Wet'.");
            Assert.That(combo.TriggerElementName, Is.EqualTo("Lightning"),
                "TriggerElementName must be 'Lightning'.");
            Assert.That(combo.TileX, Is.EqualTo(TargetPos.X),
                "TileX must match the target position X.");
            Assert.That(combo.TileY, Is.EqualTo(TargetPos.Y),
                "TileY must match the target position Y.");
        }

        // =========================================================================
        // Test 2 — Fire + Frozen → melt_ice
        // =========================================================================

        /// <summary>
        /// Fire spell hitting a Frozen tile produces a ComboEffect with name "melt_ice".
        /// JSON entry: tileState="Frozen", element="Fire", vfxHint="melt_ice".
        /// The resulting tile state transitions to "Wet" (ice melts).
        /// </summary>
        [Test]
        public void Resolve_FireOnFrozenTarget_GeneratesMeltCombo()
        {
            // Arrange
            UnitState caster = MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId,  Player2, TargetPos);

            _state.Grid.SetTileState(TargetPos, TileState.Frozen);

            SpellData spell = MakeSpell("fire_frozen", ElementType.Fire);
            var targets = new List<UnitState> { target };

            // Act
            SpellResolutionResult result = _applicator.Apply(spell, caster, targets, _state);

            // Assert
            Assert.That(result.WasCast, Is.True,
                "WasCast must be true for a valid cast.");
            Assert.That(result.ComboEffects, Is.Not.Empty,
                "A ComboEffect must fire when Fire strikes a Frozen tile.");

            ComboEffect combo = result.ComboEffects[0];

            Assert.That(combo.ComboName, Is.EqualTo("melt_ice"),
                "ComboName must match the vfxHint 'melt_ice' from the interaction table.");
            Assert.That(combo.TriggerStateName, Is.EqualTo("Frozen"),
                "TriggerStateName must be 'Frozen'.");
            Assert.That(combo.TriggerElementName, Is.EqualTo("Fire"),
                "TriggerElementName must be 'Fire'.");
        }

        // =========================================================================
        // Test 3 — Fire + Wet → steam_cloud
        // =========================================================================

        /// <summary>
        /// Fire spell hitting a Wet tile produces a ComboEffect with name "steam_cloud".
        /// JSON entry: tileState="Wet", element="Fire", vfxHint="steam_cloud",
        /// resultingTileState="Steam".
        /// </summary>
        [Test]
        public void Resolve_FireOnWetTarget_GeneratesSteamCombo()
        {
            // Arrange
            UnitState caster = MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId,  Player2, TargetPos);

            _state.Grid.SetTileState(TargetPos, TileState.Wet);

            SpellData spell = MakeSpell("fire_wet", ElementType.Fire);
            var targets = new List<UnitState> { target };

            // Act
            SpellResolutionResult result = _applicator.Apply(spell, caster, targets, _state);

            // Assert
            Assert.That(result.WasCast, Is.True,
                "WasCast must be true for a valid cast.");
            Assert.That(result.ComboEffects, Is.Not.Empty,
                "A ComboEffect must fire when Fire strikes a Wet tile.");

            ComboEffect combo = result.ComboEffects[0];

            Assert.That(combo.ComboName, Is.EqualTo("steam_cloud"),
                "ComboName must match the vfxHint 'steam_cloud' from the interaction table.");
            Assert.That(combo.TriggerStateName, Is.EqualTo("Wet"),
                "TriggerStateName must be 'Wet'.");
            Assert.That(combo.TriggerElementName, Is.EqualTo("Fire"),
                "TriggerElementName must be 'Fire'.");
        }

        // =========================================================================
        // Test 4 — Ice + Wet → freeze_tile
        // =========================================================================

        /// <summary>
        /// Ice spell hitting a Wet tile produces a ComboEffect with name "freeze_tile".
        /// JSON entry: tileState="Wet", element="Ice", vfxHint="freeze_tile",
        /// resultingTileState="Frozen". This verifies the freeze-from-wet combo path.
        /// </summary>
        [Test]
        public void Resolve_IceOnWetTarget_GeneratesFreezeCombo()
        {
            // Arrange
            UnitState caster = MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId,  Player2, TargetPos);

            _state.Grid.SetTileState(TargetPos, TileState.Wet);

            SpellData spell = MakeSpell("ice_wet", ElementType.Ice);
            var targets = new List<UnitState> { target };

            // Act
            SpellResolutionResult result = _applicator.Apply(spell, caster, targets, _state);

            // Assert
            Assert.That(result.WasCast, Is.True,
                "WasCast must be true for a valid cast.");
            Assert.That(result.ComboEffects, Is.Not.Empty,
                "A ComboEffect must fire when Ice strikes a Wet tile.");

            ComboEffect combo = result.ComboEffects[0];

            Assert.That(combo.ComboName, Is.EqualTo("freeze_tile"),
                "ComboName must match the vfxHint 'freeze_tile' from the interaction table.");
            Assert.That(combo.TriggerStateName, Is.EqualTo("Wet"),
                "TriggerStateName must be 'Wet'.");
            Assert.That(combo.TriggerElementName, Is.EqualTo("Ice"),
                "TriggerElementName must be 'Ice'.");
        }

        // =========================================================================
        // Test 5 — Lightning + Burning → firestorm_burst
        // =========================================================================

        /// <summary>
        /// Lightning spell hitting a Burning tile produces a ComboEffect with name "firestorm_burst".
        /// JSON entry: tileState="Burning", element="Lightning", vfxHint="firestorm_burst".
        /// Note: the task description calls this "arc explosion" but the interaction table uses
        /// "firestorm_burst" for Burning+Lightning. The name "arc_explosion" belongs to the
        /// Charged+Fire combo (see Test 6 below).
        /// </summary>
        [Test]
        public void Resolve_LightningOnBurningTarget_GeneratesFirestormBurstCombo()
        {
            // Arrange
            UnitState caster = MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId,  Player2, TargetPos);

            _state.Grid.SetTileState(TargetPos, TileState.Burning);

            SpellData spell = MakeSpell("lightning_burning", ElementType.Lightning);
            var targets = new List<UnitState> { target };

            // Act
            SpellResolutionResult result = _applicator.Apply(spell, caster, targets, _state);

            // Assert
            Assert.That(result.WasCast, Is.True,
                "WasCast must be true for a valid cast.");
            Assert.That(result.ComboEffects, Is.Not.Empty,
                "A ComboEffect must fire when Lightning strikes a Burning tile.");

            ComboEffect combo = result.ComboEffects[0];

            Assert.That(combo.ComboName, Is.EqualTo("firestorm_burst"),
                "ComboName must match the vfxHint 'firestorm_burst' from the interaction table " +
                "(Burning+Lightning — not to be confused with 'arc_explosion' which is Charged+Fire).");
            Assert.That(combo.TriggerStateName, Is.EqualTo("Burning"),
                "TriggerStateName must be 'Burning'.");
            Assert.That(combo.TriggerElementName, Is.EqualTo("Lightning"),
                "TriggerElementName must be 'Lightning'.");
        }

        // =========================================================================
        // Test 6 — Fire + Charged → arc_explosion
        // =========================================================================

        /// <summary>
        /// Fire spell hitting a Charged tile produces a ComboEffect with name "arc_explosion".
        /// JSON entry: tileState="Charged", element="Fire", vfxHint="arc_explosion".
        /// This is the actual "arc explosion" combo referenced in the CLAUDE.md interaction matrix
        /// (row: Fire spell, column: Charged → "Arc explosion").
        /// </summary>
        [Test]
        public void Resolve_FireOnChargedTarget_GeneratesArcExplosionCombo()
        {
            // Arrange
            UnitState caster = MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId,  Player2, TargetPos);

            _state.Grid.SetTileState(TargetPos, TileState.Charged);

            SpellData spell = MakeSpell("fire_charged", ElementType.Fire);
            var targets = new List<UnitState> { target };

            // Act
            SpellResolutionResult result = _applicator.Apply(spell, caster, targets, _state);

            // Assert
            Assert.That(result.WasCast, Is.True,
                "WasCast must be true for a valid cast.");
            Assert.That(result.ComboEffects, Is.Not.Empty,
                "A ComboEffect must fire when Fire strikes a Charged tile.");

            ComboEffect combo = result.ComboEffects[0];

            Assert.That(combo.ComboName, Is.EqualTo("arc_explosion"),
                "ComboName must match the vfxHint 'arc_explosion' from the interaction table.");
            Assert.That(combo.TriggerStateName, Is.EqualTo("Charged"),
                "TriggerStateName must be 'Charged'.");
            Assert.That(combo.TriggerElementName, Is.EqualTo("Fire"),
                "TriggerElementName must be 'Fire'.");
        }

        // =========================================================================
        // Test 7 — No interaction: Earth on Normal tile produces no ComboEffect
        // =========================================================================

        /// <summary>
        /// Earth spell hitting a Normal tile produces no ComboEffect.
        /// JSON entry: tileState="Normal", element="Earth", effects=[], vfxHint="" — both
        /// conditions for HasInteraction() returning false are met (empty effects, no vfxHint).
        /// </summary>
        [Test]
        public void Resolve_EarthOnNormalTarget_NoComboEffect()
        {
            // Arrange
            UnitState caster = MakeAndRegisterUnit(CasterId, Player1, CasterPos);
            UnitState target = MakeAndRegisterUnit(TargetId,  Player2, TargetPos);

            // Tile is Normal by default — no SetTileState call needed.
            // Verify assumption explicitly.
            Assert.That(
                _state.Grid.GetTile(TargetPos).State,
                Is.EqualTo(TileState.Normal),
                "Precondition: target tile must be Normal for this test.");

            SpellData spell = MakeSpell("earth_normal", ElementType.Earth);
            var targets = new List<UnitState> { target };

            // Act
            SpellResolutionResult result = _applicator.Apply(spell, caster, targets, _state);

            // Assert
            Assert.That(result.WasCast, Is.True,
                "WasCast must be true for a valid cast.");
            Assert.That(result.ComboEffects, Is.Empty,
                "No ComboEffect must be generated when Earth strikes a Normal tile " +
                "(Normal+Earth has empty effects and empty vfxHint in the interaction table).");
        }
    }
}

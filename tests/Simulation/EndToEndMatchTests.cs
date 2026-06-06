using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Commands;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Data;
using Battlemancers.Simulation;
using Battlemancers.Simulation.Effects;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// End-to-end match tests that exercise the complete simulation pipeline headlessly:
    /// unit construction, plan submission, turn resolution, win condition checking, and
    /// event emission. No Unity play mode required — all tests run in Edit Mode.
    ///
    /// Three scenarios are covered:
    /// <list type="number">
    ///   <item>
    ///     <see cref="FullMatch_TurnLimit_EndsInDraw"/> — 50 empty turns produce a
    ///     <see cref="MatchEndedEvent"/> with <see cref="MatchEndReason.TurnLimitReached"/>
    ///     and a null WinnerId.
    ///   </item>
    ///   <item>
    ///     <see cref="FullMatch_MoveAndSpell_KillsOpponent_CorrectWinner"/> — Player1 casts a
    ///     lethal spell via the wired <see cref="SpellEffectApplicator"/> path; Player2 submits
    ///     an empty plan. After the kill, <see cref="TurnManager.CheckWinCondition"/> confirms
    ///     Player1 as winner.
    ///   </item>
    ///   <item>
    ///     <see cref="TurnResolution_MoveCommand_UpdatesUnitPosition"/> — A single MoveCommand
    ///     updates the unit's grid position and emits a <see cref="UnitMovedEvent"/>.
    ///   </item>
    /// </list>
    /// </summary>
    [TestFixture]
    public class EndToEndMatchTests
    {
        // -----------------------------------------------------------------------------------------
        // Named constants — no magic numbers
        // -----------------------------------------------------------------------------------------

        private const string Player1 = "p1";
        private const string Player2 = "p2";

        private const string PyromancerId  = "p1_pyromancer_0";
        private const string HydromancerId = "p2_hydromancer_0";

        private static readonly GridPosition PyromancerStartPos  = new GridPosition(2, 5);
        private static readonly GridPosition HydromancerStartPos = new GridPosition(7, 5);

        private const int MancerMaxHP     = 90;
        private const int MancerMoveRange = 4;
        private const int MancerPointCost = 100;

        /// <summary>
        /// Damage high enough to one-shot a unit with <see cref="MancerMaxHP"/> HP.
        /// Chosen well above MaxHP to guarantee a kill regardless of any future armor additions.
        /// </summary>
        private const int LethalDamage = 999;

        private const int SpellRange  = 6;
        private const int SpellApCost = 1;

        private const int TurnLimit = 50;

        // -----------------------------------------------------------------------------------------
        // Fields
        // -----------------------------------------------------------------------------------------

        private GridData           _grid;
        private SimulationState    _state;
        private StatusManager      _statusManager;
        private TemperatureManager _temperatureManager;
        private ElementResolver    _elementResolver;
        private SpellEffectApplicator _applicator;
        private TurnManager        _turnManager;

        // -----------------------------------------------------------------------------------------
        // Setup / Teardown
        // -----------------------------------------------------------------------------------------

        [SetUp]
        public void SetUp()
        {
            _grid               = GridData.Standard24x24();
            _state              = new SimulationState(_grid, new[] { Player1, Player2 });
            _statusManager      = new StatusManager();
            _temperatureManager = new TemperatureManager(_statusManager);
            _elementResolver    = ElementResolver.CreateDefault();

            _applicator = new SpellEffectApplicator(
                _statusManager,
                _temperatureManager,
                _elementResolver,
                new Random(seed: 42));

            _turnManager = new TurnManager(_state, _temperatureManager);

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
        /// Creates and registers a Mancer unit at the specified position with standard stats.
        /// </summary>
        private UnitState MakeAndRegisterMancer(string id, string ownerId, GridPosition pos)
        {
            var unit = new UnitState(
                id:                id,
                mancerArchetypeId: "testmancer",
                type:              UnitType.Mancer,
                ownerId:           ownerId,
                position:          pos,
                maxHP:             MancerMaxHP,
                moveRange:         MancerMoveRange,
                pointCost:         MancerPointCost);

            _state.RegisterUnit(unit);
            return unit;
        }

        /// <summary>
        /// Creates a <see cref="SpellData"/> ScriptableObject suitable for headless Unity
        /// Edit Mode test execution. All non-essential fields are left at their defaults.
        /// </summary>
        private static SpellData MakeSpell(
            string spellId,
            int baseDamage       = 0,
            int temperatureDelta = 0,
            ElementType element  = ElementType.Fire)
        {
            var spell = ScriptableObject.CreateInstance<SpellData>();
            spell.spellId          = spellId;
            spell.apCost           = SpellApCost;
            spell.cooldownTurns    = 0;
            spell.targetType       = SpellTargetType.SingleTarget;
            spell.range            = SpellRange;
            spell.aoeRadius        = 0;
            spell.baseDamage       = baseDamage;
            spell.element          = element;
            spell.temperatureDelta = temperatureDelta;
            spell.appliedEffects   = Array.Empty<StatusEffectApplication>();
            spell.conditionalBonuses = Array.Empty<ConditionalDamageBonus>();
            return spell;
        }

        /// <summary>
        /// Submits an empty (pass) plan for both players, then resolves the turn.
        /// Returns all events produced by ResolveTurn().
        /// </summary>
        private SimulationEvent[] PassBothPlayers()
        {
            _turnManager.SubmitPlan(Player1, Array.Empty<Command>());
            _turnManager.SubmitPlan(Player2, Array.Empty<Command>());
            return _turnManager.ResolveTurn();
        }

        // =========================================================================
        // Test 1 — Turn limit reached produces a draw
        // =========================================================================

        /// <summary>
        /// Simulating 50 consecutive empty turns must produce a <see cref="MatchEndedEvent"/>
        /// with <see cref="MatchEndReason.TurnLimitReached"/> and a null WinnerId.
        ///
        /// Proves that TurnManager correctly enforces the 50-turn draw rule and that the
        /// full turn-resolution pipeline can complete 50 iterations without error.
        /// </summary>
        [Test]
        public void FullMatch_TurnLimit_EndsInDraw()
        {
            // Arrange — register one Mancer per player so neither is eliminated early.
            MakeAndRegisterMancer(PyromancerId,  Player1, PyromancerStartPos);
            MakeAndRegisterMancer(HydromancerId, Player2, HydromancerStartPos);

            SimulationEvent[] lastEvents = null;
            MatchEndedEvent matchEndedEvent = null;

            // Act — resolve TurnLimit turns with empty plans.
            for (int turn = 0; turn < TurnLimit; turn++)
            {
                lastEvents = PassBothPlayers();

                // Capture the first MatchEndedEvent we encounter.
                if (matchEndedEvent == null)
                    matchEndedEvent = lastEvents.OfType<MatchEndedEvent>().FirstOrDefault();
            }

            // Assert — a MatchEndedEvent must have been emitted.
            Assert.That(matchEndedEvent, Is.Not.Null,
                $"A MatchEndedEvent must be emitted after {TurnLimit} turns.");
            Assert.That(matchEndedEvent.Reason, Is.EqualTo(MatchEndReason.TurnLimitReached),
                "Reason must be TurnLimitReached when neither player eliminated the other.");
            Assert.That(matchEndedEvent.WinnerId, Is.Null,
                "WinnerId must be null for a draw.");
        }

        // =========================================================================
        // Test 2 — Lethal spell via wired applicator kills opponent; correct winner
        // =========================================================================

        /// <summary>
        /// Player1 submits a <see cref="SpellCommand"/> wired with a <see cref="SpellEffectApplicator"/>
        /// and a lethal spell (baseDamage = <see cref="LethalDamage"/>). Player2 submits an empty plan.
        ///
        /// After one resolved turn, Player2's Mancer is dead, and
        /// <see cref="TurnManager.CheckWinCondition"/> identifies Player1 as the winner.
        ///
        /// This is the highest-confidence proof that:
        /// <list type="bullet">
        ///   <item>SpellCommand correctly routes through SpellEffectApplicator when wired.</item>
        ///   <item>SpellEffectApplicator.Apply() mutates target HP.</item>
        ///   <item>CheckWinCondition detects the elimination and names the correct winner.</item>
        /// </list>
        /// </summary>
        [Test]
        public void FullMatch_MoveAndSpell_KillsOpponent_CorrectWinner()
        {
            // Arrange
            UnitState pyromancer  = MakeAndRegisterMancer(PyromancerId,  Player1, PyromancerStartPos);
            UnitState hydromancer = MakeAndRegisterMancer(HydromancerId, Player2, HydromancerStartPos);

            SpellData lethalSpell = MakeSpell(
                spellId:    "test_lethal_fireball",
                baseDamage: LethalDamage,
                element:    ElementType.Fire);

            // Player1 casts the lethal spell at the hydromancer's tile using the fully-wired path.
            var spellCmd = new SpellCommand(
                actorId:        PyromancerId,
                activationCost: 100,
                spellData:      lethalSpell,
                target:         HydromancerStartPos,
                applicator:     _applicator);

            // Act — Player1 attacks; Player2 passes.
            _turnManager.SubmitPlan(Player1, new Command[] { spellCmd });
            _turnManager.SubmitPlan(Player2, Array.Empty<Command>());
            SimulationEvent[] events = _turnManager.ResolveTurn();

            // Assert — hydromancer must be dead after the lethal spell resolves.
            Assert.That(hydromancer.IsAlive, Is.False,
                $"Hydromancer HP must reach 0 after a {LethalDamage}-damage spell. Actual HP: {hydromancer.CurrentHP}.");

            // Assert — a UnitDiedEvent must have been emitted for the hydromancer.
            UnitDiedEvent diedEvent = events.OfType<UnitDiedEvent>()
                .FirstOrDefault(e => e.UnitId == HydromancerId);
            Assert.That(diedEvent, Is.Not.Null,
                "A UnitDiedEvent for the hydromancer must appear in the resolved events.");

            // Assert — CheckWinCondition must identify Player1 as the winner.
            bool matchEnded = _turnManager.CheckWinCondition(out string winnerId);
            Assert.That(matchEnded, Is.True,
                "CheckWinCondition must return true when Player2 has no living Mancers.");
            Assert.That(winnerId, Is.EqualTo(Player1),
                "The winner must be Player1 when Player2's only Mancer is dead.");
        }

        // =========================================================================
        // Test 3 — MoveCommand updates unit position and emits UnitMovedEvent
        // =========================================================================

        /// <summary>
        /// A single <see cref="MoveCommand"/> submitted through <see cref="TurnManager"/>
        /// must update the caster's grid position to the destination tile and emit a
        /// <see cref="UnitMovedEvent"/> with the correct From / To positions.
        ///
        /// Verifies that the TurnManager → MoveCommand → SimulationState pipeline
        /// is wired end-to-end for the movement path.
        /// </summary>
        [Test]
        public void TurnResolution_MoveCommand_UpdatesUnitPosition()
        {
            // Arrange
            UnitState pyromancer  = MakeAndRegisterMancer(PyromancerId,  Player1, PyromancerStartPos);
            // Player2 needs a unit registered so the match is not already over.
            MakeAndRegisterMancer(HydromancerId, Player2, HydromancerStartPos);

            // Move one tile to the right — within MancerMoveRange of 4.
            GridPosition destination = new GridPosition(
                PyromancerStartPos.X + 1,
                PyromancerStartPos.Y);

            var moveCmd = new MoveCommand(
                actorId:        PyromancerId,
                activationCost: 100,
                destination:    destination);

            // Act — Player1 moves; Player2 passes.
            _turnManager.SubmitPlan(Player1, new Command[] { moveCmd });
            _turnManager.SubmitPlan(Player2, Array.Empty<Command>());
            SimulationEvent[] events = _turnManager.ResolveTurn();

            // Assert — pyromancer's position must have updated to the destination.
            Assert.That(pyromancer.Position, Is.EqualTo(destination),
                $"Pyromancer position must update to {destination} after MoveCommand resolves.");

            // Assert — a UnitMovedEvent must appear in the turn's events.
            UnitMovedEvent movedEvent = events.OfType<UnitMovedEvent>()
                .FirstOrDefault(e => e.UnitId == PyromancerId);
            Assert.That(movedEvent, Is.Not.Null,
                "A UnitMovedEvent for the pyromancer must be present in the resolved events.");
            Assert.That(movedEvent.From, Is.EqualTo(PyromancerStartPos),
                "UnitMovedEvent.From must equal the pyromancer's starting position.");
            Assert.That(movedEvent.To, Is.EqualTo(destination),
                "UnitMovedEvent.To must equal the move destination.");
        }
    }
}

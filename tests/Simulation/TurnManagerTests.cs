using System;
using System.Linq;
using NUnit.Framework;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Commands;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for TurnManager — the blind simultaneous turn orchestrator.
    /// Covers plan submission validation, activation budget rules, initiative order,
    /// mid-resolution death handling, and the 50-turn draw condition.
    /// </summary>
    [TestFixture]
    public class TurnManagerTests
    {
        private GridData _grid;
        private SimulationState _state;
        private TurnManager _turnManager;
        private const string Player1 = "p1";
        private const string Player2 = "p2";

        [SetUp]
        public void SetUp()
        {
            _grid = GridData.Standard24x24();
            _state = new SimulationState(_grid, new[] { Player1, Player2 });
            _turnManager = new TurnManager(_state);
            SimulationEventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            SimulationEventBus.Clear();
        }

        // --- SubmitPlan budget validation ---

        /// <summary>
        /// SubmitPlan with a plan whose total activation cost is exactly 100 pts is accepted without throwing.
        /// </summary>
        [Test]
        public void SubmitPlan_PlanAtExactly100Pts_IsAccepted()
        {
            // Arrange
            UnitState mancer = MakeMancer("p1_pyro_0", Player1, new GridPosition(0, 0), pointCost: 100);
            _state.RegisterUnit(mancer);
            // A MoveCommand to same tile has the unit's activation cost of 100 (Mancer).
            var cmd = new MoveCommand("p1_pyro_0", activationCost: 100, destination: mancer.Position);

            // Act / Assert — should not throw
            Assert.DoesNotThrow(() => _turnManager.SubmitPlan(Player1, new[] { cmd }));
        }

        /// <summary>
        /// SubmitPlan with a plan whose total activation cost exceeds 100 pts is rejected with ArgumentException.
        /// </summary>
        [Test]
        public void SubmitPlan_PlanOver100Pts_ThrowsArgumentException()
        {
            // Arrange
            UnitState mancer = MakeMancer("p1_pyro_0", Player1, new GridPosition(0, 0));
            UnitState chaff = MakeChaff("p1_chaff_0", Player1, new GridPosition(1, 0), pointCost: 10);
            _state.RegisterUnit(mancer);
            _state.RegisterUnit(chaff);

            // Mancer costs 100, chaff costs 10 — total 110, which exceeds the budget.
            var commands = new Command[]
            {
                new MoveCommand("p1_pyro_0", activationCost: 100, destination: mancer.Position),
                new MoveCommand("p1_chaff_0", activationCost: 10, destination: chaff.Position)
            };

            // Act / Assert
            Assert.Throws<ArgumentException>(() => _turnManager.SubmitPlan(Player1, commands));
        }

        /// <summary>
        /// A Mancer with a 150-pt warband PointCost still uses ActivationCost=100 because
        /// Mancers always activate for 100 pts regardless of upgrade cost.
        /// </summary>
        [Test]
        public void SubmitPlan_UpgradedMancer150Pts_ActivationCostIsAlways100()
        {
            // Arrange — Mancer with pointCost=150 (upgraded) still has ActivationCost == 100.
            UnitState upgradedMancer = MakeMancer("p1_pyro_0", Player1, new GridPosition(0, 0), pointCost: 150);
            _state.RegisterUnit(upgradedMancer);

            // Verify via the property directly.
            Assert.That(upgradedMancer.ActivationCost, Is.EqualTo(100));

            // Submitting it should not exceed the 100-pt cap.
            var cmd = new MoveCommand("p1_pyro_0", activationCost: upgradedMancer.ActivationCost,
                destination: upgradedMancer.Position);

            // Act / Assert — should not throw
            Assert.DoesNotThrow(() => _turnManager.SubmitPlan(Player1, new[] { cmd }));
        }

        // --- AllPlansSubmitted ---

        /// <summary>
        /// AllPlansSubmitted returns false before any player submits a plan.
        /// </summary>
        [Test]
        public void AllPlansSubmitted_BeforeAnySubmission_ReturnsFalse()
        {
            // Arrange / Act — no plans submitted.

            // Assert
            Assert.That(_turnManager.AllPlansSubmitted(), Is.False);
        }

        /// <summary>
        /// AllPlansSubmitted returns false after only one of two players has submitted.
        /// </summary>
        [Test]
        public void AllPlansSubmitted_AfterOnlyOnePlayerSubmits_ReturnsFalse()
        {
            // Arrange
            _turnManager.SubmitPlan(Player1, Array.Empty<Command>());

            // Act
            bool result = _turnManager.AllPlansSubmitted();

            // Assert
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// AllPlansSubmitted returns true after both players have submitted their plans.
        /// </summary>
        [Test]
        public void AllPlansSubmitted_AfterBothPlayersSubmit_ReturnsTrue()
        {
            // Arrange
            _turnManager.SubmitPlan(Player1, Array.Empty<Command>());
            _turnManager.SubmitPlan(Player2, Array.Empty<Command>());

            // Act
            bool result = _turnManager.AllPlansSubmitted();

            // Assert
            Assert.That(result, Is.True);
        }

        // --- ResolveTurn initiative order ---

        /// <summary>
        /// ResolveTurn processes Mancer commands before Ranged commands before Chaff commands
        /// as determined by initiative priority (Mancer=0, Ranged=1, Chaff=2).
        /// </summary>
        [Test]
        public void ResolveTurn_MixedUnitTypes_ResolvesMancersBeforeRangedBeforeChaff()
        {
            // Arrange — one unit of each type; use MoveCommand to same tile to track resolution.
            UnitState mancer = MakeMancer("p1_pyro_0", Player1, new GridPosition(5, 0));
            UnitState ranged = MakeRanged("p1_ranged_0", Player1, new GridPosition(10, 0), pointCost: 25);
            UnitState chaff = MakeChaff("p1_chaff_0", Player1, new GridPosition(15, 0), pointCost: 10);
            _state.RegisterUnit(mancer);
            _state.RegisterUnit(ranged);
            _state.RegisterUnit(chaff);

            // Track resolution order via event subscriptions.
            var resolvedOrder = new System.Collections.Generic.List<string>();
            SimulationEventBus.Subscribe<UnitMovedEvent>(e => resolvedOrder.Add(e.UnitId));

            // Chaff activates for 10, Ranged for 25, Mancer for 100 — each submitted in reverse priority order
            // to confirm TurnManager re-sorts them.
            var p1Commands = new Command[]
            {
                new MoveCommand("p1_chaff_0",  activationCost: 10,  destination: new GridPosition(16, 0)),
                new MoveCommand("p1_ranged_0", activationCost: 25,  destination: new GridPosition(11, 0)),
                new MoveCommand("p1_pyro_0",   activationCost: 100, destination: new GridPosition(6, 0))
            };
            // Total = 10 + 25 + 100 = 135 > 100 — we need separate turns; give them to separate players.
            // Instead, put each unit on a different player.
            // Restructure: one player controls each unit type separately across p1/p2.
            // Simpler: make three separate players — but state has p1/p2. Use p1 for mancer only,
            // p2 for ranged+chaff. Ranged(25)+Chaff(10)=35 ≤ 100.
            // Reset state for this test.
            _grid = GridData.Standard24x24();
            _state = new SimulationState(_grid, new[] { Player1, Player2 });
            _turnManager = new TurnManager(_state);
            resolvedOrder.Clear();

            mancer = MakeMancer("p1_pyro_0", Player1, new GridPosition(5, 0));
            ranged = MakeRanged("p2_ranged_0", Player2, new GridPosition(10, 0), pointCost: 25);
            chaff = MakeChaff("p2_chaff_0", Player2, new GridPosition(15, 0), pointCost: 10);
            _state.RegisterUnit(mancer);
            _state.RegisterUnit(ranged);
            _state.RegisterUnit(chaff);

            SimulationEventBus.Subscribe<UnitMovedEvent>(e => resolvedOrder.Add(e.UnitId));

            var p1Plan = new Command[] { new MoveCommand("p1_pyro_0",   activationCost: 100, destination: new GridPosition(6, 0)) };
            var p2Plan = new Command[]
            {
                new MoveCommand("p2_chaff_0",  activationCost: 10,  destination: new GridPosition(16, 0)),
                new MoveCommand("p2_ranged_0", activationCost: 25,  destination: new GridPosition(11, 0))
            };

            _turnManager.SubmitPlan(Player1, p1Plan);
            _turnManager.SubmitPlan(Player2, p2Plan);

            // Act
            _turnManager.ResolveTurn();

            // Assert — Mancer must be first, then Ranged, then Chaff
            Assert.That(resolvedOrder.Count, Is.EqualTo(3));
            Assert.That(resolvedOrder[0], Is.EqualTo("p1_pyro_0"));
            Assert.That(resolvedOrder[1], Is.EqualTo("p2_ranged_0"));
            Assert.That(resolvedOrder[2], Is.EqualTo("p2_chaff_0"));
        }

        /// <summary>
        /// ResolveTurn skips commands whose actor died during the same turn's resolution.
        /// The dead unit's subsequent command is silently omitted from execution.
        /// </summary>
        [Test]
        public void ResolveTurn_ActorDiedMidTurn_SkipsDeadActorsCommand()
        {
            // Arrange — place two adjacent enemy Mancers so an AttackCommand can kill one.
            UnitState attacker = MakeMancer("p1_pyro_0", Player1, new GridPosition(5, 5));
            // Defender has 1 HP so the stub 10-damage attack kills it.
            UnitState defender = MakeMancer("p2_hydro_0", Player2, new GridPosition(6, 5), maxHP: 1);
            _state.RegisterUnit(attacker);
            _state.RegisterUnit(defender);

            // p1 attacks the defender (Mancer priority=0, lower X resolves first).
            // p2 also submits a move for the defender — but defender will be dead by the time it resolves.
            var p1Plan = new Command[]
            {
                new AttackCommand("p1_pyro_0", activationCost: 100, defenderId: "p2_hydro_0")
            };
            var p2Plan = new Command[]
            {
                new MoveCommand("p2_hydro_0", activationCost: 100, destination: new GridPosition(7, 5))
            };

            _turnManager.SubmitPlan(Player1, p1Plan);
            _turnManager.SubmitPlan(Player2, p2Plan);

            // Act
            SimulationEvent[] events = _turnManager.ResolveTurn();

            // Assert — UnitDiedEvent present; no UnitMovedEvent for the dead unit
            Assert.That(events.OfType<UnitDiedEvent>().Any(e => e.UnitId == "p2_hydro_0"), Is.True);
            Assert.That(events.OfType<UnitMovedEvent>().Any(e => e.UnitId == "p2_hydro_0"), Is.False);
        }

        // --- Turn limit ---

        /// <summary>
        /// ResolveTurn on turn 50 (the limit) publishes a MatchEndedEvent with reason TurnLimitReached
        /// and a null WinnerId (draw).
        /// </summary>
        [Test]
        public void ResolveTurn_OnTurnLimit_PublishesMatchEndedEventWithDraw()
        {
            // Arrange — advance the state to turn 50 by calling AdvanceTurn internally.
            // We need TurnNumber == 50 when ResolveTurn checks win conditions.
            // AdvanceTurn is internal; drive it by resolving 49 empty turns.
            UnitState p1Mancer = MakeMancer("p1_pyro_0", Player1, new GridPosition(0, 0));
            UnitState p2Mancer = MakeMancer("p2_hydro_0", Player2, new GridPosition(23, 23));
            _state.RegisterUnit(p1Mancer);
            _state.RegisterUnit(p2Mancer);

            // Resolve 49 turns with empty plans to advance TurnNumber to 50.
            for (int i = 0; i < 49; i++)
            {
                _turnManager.SubmitPlan(Player1, Array.Empty<Command>());
                _turnManager.SubmitPlan(Player2, Array.Empty<Command>());
                _turnManager.ResolveTurn();
            }

            Assert.That(_state.TurnNumber, Is.EqualTo(50), "Setup: expected TurnNumber=50 before final resolve.");

            // Submit plans for turn 50.
            _turnManager.SubmitPlan(Player1, Array.Empty<Command>());
            _turnManager.SubmitPlan(Player2, Array.Empty<Command>());

            // Act
            SimulationEvent[] events = _turnManager.ResolveTurn();

            // Assert — MatchEndedEvent with TurnLimitReached and no winner
            MatchEndedEvent matchEnd = events.OfType<MatchEndedEvent>().FirstOrDefault();
            Assert.That(matchEnd, Is.Not.Null);
            Assert.That(matchEnd.Reason, Is.EqualTo(MatchEndReason.TurnLimitReached));
            Assert.That(matchEnd.WinnerId, Is.Null);
        }

        // --- Helpers ---

        private static UnitState MakeMancer(string id, string ownerId, GridPosition pos,
                                            int maxHP = 100, int pointCost = 100)
        {
            return new UnitState(id, "pyromancer", UnitType.Mancer, ownerId, pos,
                maxHP: maxHP, moveRange: 4, pointCost: pointCost);
        }

        private static UnitState MakeRanged(string id, string ownerId, GridPosition pos, int pointCost = 25)
        {
            return new UnitState(id, null, UnitType.Ranged, ownerId, pos,
                maxHP: 30, moveRange: 2, pointCost: pointCost);
        }

        private static UnitState MakeChaff(string id, string ownerId, GridPosition pos, int pointCost = 10)
        {
            return new UnitState(id, null, UnitType.Chaff, ownerId, pos,
                maxHP: 20, moveRange: 2, pointCost: pointCost);
        }
    }
}

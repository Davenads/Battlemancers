using System;
using System.Linq;
using NUnit.Framework;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Commands;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Integration tests for the complete turn resolution pipeline.
    /// Covers status-based command overrides (STUNNED, FROZEN, ROOTED, SILENCED, CONFUSED,
    /// CHARMED, HASTE), ICE_TILE movement rules, and full two-player turn resolution.
    ///
    /// All tests use the standard two-player / 24×24 grid setup. Status effects are applied
    /// directly to UnitState.ActiveStatusTypes to simulate what StatusManager would apply.
    /// </summary>
    [TestFixture]
    public class SimulationIntegrationTests
    {
        // ---------------------------------------------------------------------------
        // Setup / teardown
        // ---------------------------------------------------------------------------

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

        // ---------------------------------------------------------------------------
        // STUNNED
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A unit with STUNNED status has all its planned commands skipped.
        /// Other units in the same turn still execute their commands normally.
        /// </summary>
        [Test]
        public void ResolveTurn_StunnedUnit_SkipsAllCommands()
        {
            // Arrange
            UnitState stunned = MakeMancer("p1_stunned", Player1, new GridPosition(0, 0));
            UnitState normal  = MakeMancer("p2_normal",  Player2, new GridPosition(10, 10));
            _state.RegisterUnit(stunned);
            _state.RegisterUnit(normal);

            // Apply STUNNED directly to the unit's active status list.
            stunned.ActiveStatusTypes.Add(StatusType.Stunned.ToString());

            GridPosition stunnedDest = new GridPosition(1, 0);
            GridPosition normalDest  = new GridPosition(11, 10);

            _turnManager.SubmitPlan(Player1, new Command[]
            {
                new MoveCommand("p1_stunned", activationCost: 100, destination: stunnedDest)
            });
            _turnManager.SubmitPlan(Player2, new Command[]
            {
                new MoveCommand("p2_normal", activationCost: 100, destination: normalDest)
            });

            // Act
            SimulationEvent[] events = _turnManager.ResolveTurn();

            // Assert — stunned unit must NOT have moved; normal unit must have moved.
            UnitState stunnedAfter = _state.GetUnit("p1_stunned");
            UnitState normalAfter  = _state.GetUnit("p2_normal");

            Assert.That(stunnedAfter.Position, Is.EqualTo(new GridPosition(0, 0)),
                "STUNNED unit should not have moved.");
            Assert.That(normalAfter.Position, Is.EqualTo(normalDest),
                "Normal unit should have moved to its planned destination.");

            // No UnitMovedEvent for the stunned unit.
            Assert.That(
                events.OfType<UnitMovedEvent>().Any(e => e.UnitId == "p1_stunned"),
                Is.False,
                "STUNNED unit should produce no UnitMovedEvent.");
        }

        // ---------------------------------------------------------------------------
        // FROZEN
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A unit with FROZEN status has all its planned commands skipped, just like STUNNED.
        /// </summary>
        [Test]
        public void ResolveTurn_FrozenUnit_SkipsAllCommands()
        {
            // Arrange
            UnitState frozen = MakeMancer("p1_frozen", Player1, new GridPosition(2, 2));
            UnitState other  = MakeMancer("p2_other",  Player2, new GridPosition(10, 10));
            _state.RegisterUnit(frozen);
            _state.RegisterUnit(other);

            frozen.ActiveStatusTypes.Add(StatusType.Frozen.ToString());

            _turnManager.SubmitPlan(Player1, new Command[]
            {
                new MoveCommand("p1_frozen", activationCost: 100, destination: new GridPosition(3, 2))
            });
            _turnManager.SubmitPlan(Player2, new Command[]
            {
                new MoveCommand("p2_other", activationCost: 100, destination: new GridPosition(11, 10))
            });

            // Act
            SimulationEvent[] events = _turnManager.ResolveTurn();

            // Assert
            Assert.That(_state.GetUnit("p1_frozen").Position, Is.EqualTo(new GridPosition(2, 2)),
                "FROZEN unit should not have moved.");
            Assert.That(
                events.OfType<UnitMovedEvent>().Any(e => e.UnitId == "p1_frozen"),
                Is.False);
        }

        // ---------------------------------------------------------------------------
        // ROOTED
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A ROOTED unit's SpellCommand executes normally; its MoveCommand is cancelled.
        /// </summary>
        [Test]
        public void ResolveTurn_RootedUnit_ExecutesSpellNotMove()
        {
            // Arrange — rooted Mancer with both a move and a spell command.
            UnitState rooted = MakeMancer("p1_rooted", Player1, new GridPosition(5, 5));
            _state.RegisterUnit(rooted);
            rooted.ActiveStatusTypes.Add(StatusType.Rooted.ToString());

            GridPosition spellTarget = new GridPosition(6, 5); // within stub range

            // Both commands share the same 100-pt activation budget for the Mancer.
            // We pass 0 for the second command's activationCost so the budget check passes
            // (the 100-pt cost is charged once per unit, not per command).
            _turnManager.SubmitPlan(Player1, new Command[]
            {
                new MoveCommand("p1_rooted", activationCost: 100, destination: new GridPosition(6, 5)),
                new SpellCommand("p1_rooted", activationCost: 0, spellId: "test_spell", target: spellTarget)
            });
            _turnManager.SubmitPlan(Player2, Array.Empty<Command>());

            // Act
            SimulationEvent[] events = _turnManager.ResolveTurn();

            // Assert — rooted unit should NOT have moved but spell should have fired.
            Assert.That(_state.GetUnit("p1_rooted").Position, Is.EqualTo(new GridPosition(5, 5)),
                "ROOTED unit should not have moved.");
            Assert.That(
                events.OfType<SpellCastEvent>().Any(e => e.CasterId == "p1_rooted"),
                Is.True,
                "ROOTED unit's spell should have executed.");
        }

        // ---------------------------------------------------------------------------
        // SILENCED
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A SILENCED unit's MoveCommand executes normally; its SpellCommand is cancelled.
        /// </summary>
        [Test]
        public void ResolveTurn_SilencedUnit_ExecutesMoveNotSpell()
        {
            // Arrange
            UnitState silenced = MakeMancer("p1_silenced", Player1, new GridPosition(3, 3));
            _state.RegisterUnit(silenced);
            silenced.ActiveStatusTypes.Add(StatusType.Silenced.ToString());

            GridPosition moveDest   = new GridPosition(4, 3);
            GridPosition spellTarget = new GridPosition(5, 3);

            _turnManager.SubmitPlan(Player1, new Command[]
            {
                new MoveCommand("p1_silenced", activationCost: 100, destination: moveDest),
                new SpellCommand("p1_silenced", activationCost: 0, spellId: "test_spell", target: spellTarget)
            });
            _turnManager.SubmitPlan(Player2, Array.Empty<Command>());

            // Act
            SimulationEvent[] events = _turnManager.ResolveTurn();

            // Assert
            Assert.That(_state.GetUnit("p1_silenced").Position, Is.EqualTo(moveDest),
                "SILENCED unit should have moved.");
            Assert.That(
                events.OfType<SpellCastEvent>().Any(e => e.CasterId == "p1_silenced"),
                Is.False,
                "SILENCED unit's spell should have been cancelled.");
        }

        // ---------------------------------------------------------------------------
        // CONFUSED
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A CONFUSED unit's spell targets the nearest visible unit regardless of allegiance.
        /// In this scenario the nearest unit is an ALLY, so the spell fires at the ally.
        /// </summary>
        [Test]
        public void ResolveTurn_ConfusedUnit_TargetsNearestRegardlessOfAllegiance()
        {
            // Arrange — set up so the nearest unit to the confused actor is an ALLY.
            UnitState confused = MakeMancer("p1_confused", Player1, new GridPosition(5, 5));
            UnitState ally     = MakeMancer("p1_ally",     Player1, new GridPosition(6, 5)); // distance 1
            UnitState enemy    = MakeMancer("p2_enemy",    Player2, new GridPosition(10, 5)); // distance 5
            _state.RegisterUnit(confused);
            _state.RegisterUnit(ally);
            _state.RegisterUnit(enemy);

            confused.ActiveStatusTypes.Add(StatusType.Confused.ToString());

            // Confused unit plans to cast at the enemy (would normally be the intended target).
            GridPosition intendedTarget = enemy.Position;

            _turnManager.SubmitPlan(Player1, new Command[]
            {
                new SpellCommand("p1_confused", activationCost: 100, spellId: "test_spell", target: intendedTarget)
            });
            _turnManager.SubmitPlan(Player2, Array.Empty<Command>());

            // Act
            SimulationEvent[] events = _turnManager.ResolveTurn();

            // Assert — spell should have fired at the ALLY (nearest unit), not the enemy.
            SpellCastEvent? castEvent = events.OfType<SpellCastEvent>()
                .FirstOrDefault(e => e.CasterId == "p1_confused");

            Assert.That(castEvent, Is.Not.Null,
                "Confused unit should have cast a spell.");
            Assert.That(castEvent.Target, Is.EqualTo(ally.Position),
                "CONFUSED unit should target the nearest unit (ally), not the planned enemy target.");
        }

        // ---------------------------------------------------------------------------
        // CHARMED
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A CHARMED unit's attack is directed at its own team (nearest ally).
        /// </summary>
        [Test]
        public void ResolveTurn_CharmedUnit_AttacksNearestAlly()
        {
            // Arrange
            UnitState charmed = MakeMancer("p1_charmed", Player1, new GridPosition(5, 5));
            UnitState ally    = MakeMancer("p1_ally",    Player1, new GridPosition(7, 5)); // distance 2 — in spell range
            UnitState enemy   = MakeMancer("p2_enemy",   Player2, new GridPosition(15, 5)); // far enemy
            _state.RegisterUnit(charmed);
            _state.RegisterUnit(ally);
            _state.RegisterUnit(enemy);

            charmed.ActiveStatusTypes.Add(StatusType.Charmed.ToString());

            // Charmed unit plans to cast at the enemy.
            _turnManager.SubmitPlan(Player1, new Command[]
            {
                new SpellCommand("p1_charmed", activationCost: 100, spellId: "test_spell", target: enemy.Position)
            });
            _turnManager.SubmitPlan(Player2, Array.Empty<Command>());

            // Act
            SimulationEvent[] events = _turnManager.ResolveTurn();

            // Assert — spell must have fired at the ally's position.
            SpellCastEvent? castEvent = events.OfType<SpellCastEvent>()
                .FirstOrDefault(e => e.CasterId == "p1_charmed");

            Assert.That(castEvent, Is.Not.Null,
                "CHARMED unit should have cast a spell.");
            Assert.That(castEvent.Target, Is.EqualTo(ally.Position),
                "CHARMED unit should target nearest ally, not the planned enemy.");
        }

        // ---------------------------------------------------------------------------
        // HASTE resolution order
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A HASTE'd Mancer resolves before a non-HASTE Mancer of equivalent position priority.
        /// Both are at equal x+y sums but the HASTE unit should always go first.
        /// </summary>
        [Test]
        public void ResolveTurn_HasteUnit_ResolvesBeforeNormalSameType()
        {
            // Arrange — two Mancers with the same x+y sum (both at sum=6).
            // Normal: (3,3) sum=6. HASTE: (4,2) sum=6, higher x so without HASTE goes later.
            // With HASTE, the (4,2) unit should resolve first regardless of position.
            UnitState normalMancer = MakeMancer("p1_normal", Player1, new GridPosition(3, 3));
            UnitState hasteMancer  = MakeMancer("p2_haste",  Player2, new GridPosition(4, 2));
            _state.RegisterUnit(normalMancer);
            _state.RegisterUnit(hasteMancer);

            hasteMancer.ActiveStatusTypes.Add(StatusType.Haste.ToString());

            var resolveOrder = new System.Collections.Generic.List<string>();
            SimulationEventBus.Subscribe<UnitMovedEvent>(e => resolveOrder.Add(e.UnitId));

            _turnManager.SubmitPlan(Player1, new Command[]
            {
                new MoveCommand("p1_normal", activationCost: 100, destination: new GridPosition(4, 3))
            });
            _turnManager.SubmitPlan(Player2, new Command[]
            {
                new MoveCommand("p2_haste", activationCost: 100, destination: new GridPosition(5, 2))
            });

            // Act
            _turnManager.ResolveTurn();

            // Assert — HASTE unit must resolve first (index 0).
            Assert.That(resolveOrder.Count, Is.EqualTo(2));
            Assert.That(resolveOrder[0], Is.EqualTo("p2_haste"),
                "HASTE unit should resolve before non-HASTE unit in same type window.");
            Assert.That(resolveOrder[1], Is.EqualTo("p1_normal"));
        }

        // ---------------------------------------------------------------------------
        // ICE_TILE voluntary movement
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Moving through an ICE_TILE during voluntary movement costs +1 extra AP per frozen tile.
        /// We test MoveCommand.Execute directly to observe the AP deduction before the turn-end reset.
        /// </summary>
        [Test]
        public void ResolveTurn_IceTile_VoluntaryMovementCostsExtraAP()
        {
            // Arrange — set tile (1,0) to Frozen. Unit moves from (0,0) to (2,0) through it.
            _grid.SetTileState(new GridPosition(1, 0), TileState.Frozen);

            UnitState mancer = MakeMancer("p1_mover", Player1, new GridPosition(0, 0), moveRange: 5);
            _state.RegisterUnit(mancer);

            // Put state into Resolving phase so Execute() can be called directly.
            _state.Phase = TurnPhase.Resolving;

            int apBefore = mancer.ActionPoints; // 6

            // Act — execute the MoveCommand directly (bypasses TurnManager's end-of-turn AP reset).
            var moveCmd = new MoveCommand("p1_mover", activationCost: 100,
                destination: new GridPosition(2, 0), kind: MoveKind.Voluntary);
            moveCmd.Execute(_state);

            // Assert — unit reached destination, and AP was reduced by the ICE_TILE penalty (−1).
            Assert.That(mancer.Position, Is.EqualTo(new GridPosition(2, 0)),
                "Unit should have moved to destination.");
            Assert.That(mancer.ActionPoints, Is.EqualTo(apBefore - 1),
                "ICE_TILE voluntary movement should cost +1 AP for the frozen tile traversed.");
        }

        // ---------------------------------------------------------------------------
        // ICE_TILE forced displacement extension
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A forced displacement (knockback) that ends on an ICE_TILE extends by exactly 1
        /// extra tile in the displacement direction — guaranteed, no roll.
        /// </summary>
        [Test]
        public void ResolveTurn_IceTile_ForcedDisplacementExtendsOneTile()
        {
            // Arrange — unit at (0,0) displaced to (2,0). Tile (2,0) is ICE → extends to (3,0).
            _grid.SetTileState(new GridPosition(2, 0), TileState.Frozen);

            UnitState unit = MakeMancer("p1_unit", Player1, new GridPosition(0, 0), moveRange: 6);
            _state.RegisterUnit(unit);

            // We test MoveCommand directly with MoveKind.Forced.
            // The displacement direction is from (0,0) toward (2,0), i.e., +X.
            // Tile (2,0) is ICE → unit slides to (3,0).
            _state.Phase = TurnPhase.Resolving;
            var forcedMove = new MoveCommand("p1_unit", activationCost: 100,
                destination: new GridPosition(2, 0), kind: MoveKind.Forced);
            forcedMove.Execute(_state);

            // Assert — unit should be at (3,0) due to ICE_TILE extension.
            Assert.That(unit.Position, Is.EqualTo(new GridPosition(3, 0)),
                "Forced displacement ending on ICE_TILE should extend by 1 extra tile.");
        }

        // ---------------------------------------------------------------------------
        // Full two-player turn returns events
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A full two-player turn with both plans submitted produces a non-empty event list
        /// that includes at least a TurnResolvedEvent.
        /// </summary>
        [Test]
        public void ResolveTurn_BothPlansSubmitted_AllEventsReturned()
        {
            // Arrange — two Mancers, one per player, both submit a move.
            UnitState p1Mancer = MakeMancer("p1_mancer", Player1, new GridPosition(0, 0));
            UnitState p2Mancer = MakeMancer("p2_mancer", Player2, new GridPosition(23, 23));
            _state.RegisterUnit(p1Mancer);
            _state.RegisterUnit(p2Mancer);

            _turnManager.SubmitPlan(Player1, new Command[]
            {
                new MoveCommand("p1_mancer", activationCost: 100, destination: new GridPosition(1, 0))
            });
            _turnManager.SubmitPlan(Player2, new Command[]
            {
                new MoveCommand("p2_mancer", activationCost: 100, destination: new GridPosition(22, 23))
            });

            // Act
            SimulationEvent[] events = _turnManager.ResolveTurn();

            // Assert — must return at minimum the TurnResolvedEvent; also check for moves.
            Assert.That(events, Is.Not.Null);
            Assert.That(events.Length, Is.GreaterThan(0), "Event list must not be empty.");
            Assert.That(
                events.OfType<TurnResolvedEvent>().Any(),
                Is.True,
                "ResolveTurn must always emit a TurnResolvedEvent.");
            Assert.That(
                events.OfType<UnitMovedEvent>().Count(),
                Is.EqualTo(2),
                "Both Mancers should have emitted UnitMovedEvents.");
        }

        // ---------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------

        private static UnitState MakeMancer(string id, string ownerId, GridPosition pos,
                                            int maxHP = 100, int moveRange = 4, int pointCost = 100)
        {
            return new UnitState(id, "test_mancer", UnitType.Mancer, ownerId, pos,
                maxHP: maxHP, moveRange: moveRange, pointCost: pointCost);
        }

        private static UnitState MakeChaff(string id, string ownerId, GridPosition pos, int pointCost = 10)
        {
            return new UnitState(id, null, UnitType.Chaff, ownerId, pos,
                maxHP: 20, moveRange: 2, pointCost: pointCost);
        }
    }
}

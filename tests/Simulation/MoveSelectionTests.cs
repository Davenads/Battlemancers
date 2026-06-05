using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Presentation;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Tests for MoveSelectionUI — the tile-selection flow for unit movement.
    ///
    /// These tests exercise the pure logic of valid-tile computation and event emission.
    /// Because MoveSelectionUI is a MonoBehaviour, we drive it via a TestableProxy that
    /// exposes the internal ComputeValidTiles result through EnterMoveMode + a recorded
    /// event list. No Unity TestRunner is required — standard NUnit headless.
    ///
    /// Note: MoveSelectionUI.EnterMoveMode stores valid tiles internally. We test
    /// the public contract by checking which OnTileClicked calls trigger MoveConfirmed.
    /// </summary>
    [TestFixture]
    public class MoveSelectionTests
    {
        // ---------------------------------------------------------------------------
        // Constants matching MoveSelectionUI internals
        // ---------------------------------------------------------------------------

        private const string Player1 = "p1";
        private const string Player2 = "p2";
        private const string StatusRooted = "Rooted";

        // ---------------------------------------------------------------------------
        // Shared setup helpers
        // ---------------------------------------------------------------------------

        private static SimulationState MakeState(int width = 32, int height = 32)
        {
            return new SimulationState(GridData.Standard32x32(), new[] { Player1, Player2 });
        }

        private static UnitState MakeMancer(string id, string ownerId, GridPosition pos, int moveRange = 4)
        {
            return new UnitState(id, "pyromancer", UnitType.Mancer, ownerId,
                pos, maxHP: 100, moveRange: moveRange, pointCost: 100);
        }

        /// <summary>
        /// Creates a MoveSelectionUI and wires up a MoveConfirmed event recorder.
        /// Returns the UI instance and the event record list.
        /// </summary>
        private static (MoveSelectionUI ui, List<(UnitState unit, Vector2Int tile)> events) MakeUI()
        {
            // MoveSelectionUI is a MonoBehaviour; in headless tests we cannot call
            // AddComponent, so we use reflection-free instantiation via the default
            // constructor available through UnityEngine.Object.
            // As a workaround for headless NUnit, we test via a thin subclass that
            // exposes the internal state. However, since we cannot create GameObjects
            // in headless NUnit either, we instead test the externally visible behavior
            // (which tiles trigger/suppress MoveConfirmed) by using a concrete
            // SimulationState as the source of truth.
            //
            // We drive MoveSelectionUI by constructing a real SimulationState, calling
            // EnterMoveMode with it, then probing OnTileClicked for which tiles fire the event.
            // The MonoBehaviour aspect (requiring a GameObject) means we can only test this
            // via the Edit Mode test runner in Unity. For headless NUnit we use a testable
            // wrapper approach by driving the logic directly through the public API.
            //
            // Compromise: use a derived TestMoveSelectionUI that skips the MonoBehaviour
            // requirement for pure-logic testing.
            var ui = new TestMoveSelectionUI();
            var recorded = new List<(UnitState, Vector2Int)>();
            ui.MoveConfirmed += (unit, tile) => recorded.Add((unit, tile));
            return (ui, recorded);
        }

        // ---------------------------------------------------------------------------
        // Test 1: EnterMoveMode_NormalUnit_HighlightsTilesWithinRange
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A unit at (5,5) with MoveRange=4 can reach (5,6), (6,5), and (5,9),
        /// but cannot reach (5,10) (Manhattan distance 5, exceeds range 4).
        /// </summary>
        [Test]
        public void EnterMoveMode_NormalUnit_HighlightsTilesWithinRange()
        {
            // Arrange
            SimulationState state = MakeState();
            UnitState unit = MakeMancer("p1_pyro_0", Player1, new GridPosition(5, 5), moveRange: 4);
            state.RegisterUnit(unit);

            var (ui, events) = MakeUI();
            ui.EnterMoveMode(unit, state);

            // Act — click tiles and check which ones fire MoveConfirmed

            // (5,6) — Manhattan distance 1 — should be valid
            ui.OnTileClicked(new Vector2Int(5, 6));
            Assert.That(events.Count, Is.EqualTo(1), "(5,6) is within range 4 and should be accepted.");
            Assert.That(events[0].tile, Is.EqualTo(new Vector2Int(5, 6)));

            // Reset for next check
            ui.EnterMoveMode(unit, state);
            events.Clear();

            // (6,5) — Manhattan distance 1 — should be valid
            ui.OnTileClicked(new Vector2Int(6, 5));
            Assert.That(events.Count, Is.EqualTo(1), "(6,5) is within range 4 and should be accepted.");

            // Reset for boundary check
            ui.EnterMoveMode(unit, state);
            events.Clear();

            // (5,9) — Manhattan distance 4 — should be valid (exactly at range boundary)
            ui.OnTileClicked(new Vector2Int(5, 9));
            Assert.That(events.Count, Is.EqualTo(1), "(5,9) is exactly at range 4 and should be accepted.");

            // Reset for out-of-range check
            ui.EnterMoveMode(unit, state);
            events.Clear();

            // (5,10) — Manhattan distance 5 — should be out of range
            ui.OnTileClicked(new Vector2Int(5, 10));
            Assert.That(events.Count, Is.EqualTo(0), "(5,10) exceeds range 4 and should be rejected.");
        }

        // ---------------------------------------------------------------------------
        // Test 2: EnterMoveMode_RootedUnit_ReturnsEmptyTileSet
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A unit with the Rooted status has no valid move tiles; clicking any tile
        /// does not fire MoveConfirmed.
        /// </summary>
        [Test]
        public void EnterMoveMode_RootedUnit_ReturnsEmptyTileSet()
        {
            // Arrange
            SimulationState state = MakeState();
            UnitState unit = MakeMancer("p1_pyro_0", Player1, new GridPosition(5, 5), moveRange: 4);
            unit.ActiveStatusTypes.Add(StatusRooted);
            state.RegisterUnit(unit);

            var (ui, events) = MakeUI();
            ui.EnterMoveMode(unit, state);

            // Act — click several tiles that would normally be in range
            ui.OnTileClicked(new Vector2Int(5, 6));
            ui.OnTileClicked(new Vector2Int(6, 5));
            ui.OnTileClicked(new Vector2Int(4, 5));
            ui.OnTileClicked(new Vector2Int(5, 4));

            // Assert — none should fire
            Assert.That(events.Count, Is.EqualTo(0),
                "A Rooted unit has an empty valid-tile set; no MoveConfirmed should be raised.");
        }

        // ---------------------------------------------------------------------------
        // Test 3: OnTileClicked_ValidTile_FiresMoveConfirmed
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Clicking a tile in the valid set fires MoveConfirmed with the correct
        /// UnitState and tile arguments.
        /// </summary>
        [Test]
        public void OnTileClicked_ValidTile_FiresMoveConfirmed()
        {
            // Arrange
            SimulationState state = MakeState();
            var startPos = new GridPosition(3, 3);
            UnitState unit = MakeMancer("p1_pyro_0", Player1, startPos, moveRange: 4);
            state.RegisterUnit(unit);

            var (ui, events) = MakeUI();
            ui.EnterMoveMode(unit, state);

            var targetTile = new Vector2Int(3, 5); // Manhattan distance 2 — valid

            // Act
            ui.OnTileClicked(targetTile);

            // Assert
            Assert.That(events.Count, Is.EqualTo(1), "MoveConfirmed should fire exactly once for a valid tile.");
            Assert.That(events[0].unit, Is.SameAs(unit), "MoveConfirmed should carry the original UnitState.");
            Assert.That(events[0].tile, Is.EqualTo(targetTile), "MoveConfirmed should carry the clicked tile.");
        }

        // ---------------------------------------------------------------------------
        // Test 4: OnTileClicked_OccupiedTile_DoesNotFireMoveConfirmed
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A tile occupied by another unit is excluded from the valid set.
        /// Clicking it does not fire MoveConfirmed.
        /// </summary>
        [Test]
        public void OnTileClicked_OccupiedTile_DoesNotFireMoveConfirmed()
        {
            // Arrange
            SimulationState state = MakeState();
            UnitState mover   = MakeMancer("p1_pyro_0", Player1, new GridPosition(5, 5), moveRange: 4);
            UnitState blocker = MakeMancer("p2_hydro_0", Player2, new GridPosition(5, 6)); // adjacent
            state.RegisterUnit(mover);
            state.RegisterUnit(blocker);

            var (ui, events) = MakeUI();
            ui.EnterMoveMode(mover, state);

            // Act — click the tile occupied by the blocker
            ui.OnTileClicked(new Vector2Int(5, 6));

            // Assert
            Assert.That(events.Count, Is.EqualTo(0),
                "A tile occupied by another unit must not be in the valid set.");
        }

        // ---------------------------------------------------------------------------
        // TestMoveSelectionUI — thin subclass for headless testing
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Exposes MoveSelectionUI behavior for headless NUnit testing without requiring
        /// a Unity scene. Overrides no logic — purely removes the MonoBehaviour constraint
        /// by providing a non-MonoBehaviour entry point for the pure-C# logic.
        ///
        /// In production, MoveSelectionUI is instantiated as a MonoBehaviour by Unity.
        /// In headless tests, this subclass is used instead so the logic can be exercised
        /// without a running Unity player.
        ///
        /// Note: Because MoveSelectionUI is a MonoBehaviour, this class bypasses the
        /// constructor restriction by being declared inside the test fixture. The
        /// EnterMoveMode and OnTileClicked methods are fully pure-C# and do not call
        /// any Unity APIs, making them safe to invoke outside a Unity context.
        /// </summary>
        private class TestMoveSelectionUI : MoveSelectionUI { }
    }
}

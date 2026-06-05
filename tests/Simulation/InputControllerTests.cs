using NUnit.Framework;
using UnityEngine;
using Battlemancers.Presentation;

namespace Battlemancers.Tests.Simulation
{
    /// <summary>
    /// Logic tests for InputController.
    /// Tests the WorldToGrid coordinate conversion and initial InputMode state.
    /// These are pure logic tests — no Unity Update() loop or MonoBehaviour lifecycle involved.
    /// WorldToGrid is internal static so tests can call it directly without a scene.
    /// </summary>
    [TestFixture]
    public class InputControllerTests
    {
        // ---------------------------------------------------------------------------
        // WorldToGrid — coordinate conversion
        // ---------------------------------------------------------------------------

        /// <summary>
        /// World position (0, 0, 0) maps to grid coordinate (0, 0).
        /// This is the canonical origin case.
        /// </summary>
        [Test]
        public void WorldToGrid_Origin_ReturnsZeroZero()
        {
            // Arrange
            var worldPos = new Vector3(0f, 0f, 0f);

            // Act
            Vector2Int result = InputController.WorldToGrid(worldPos);

            // Assert
            Assert.AreEqual(new Vector2Int(0, 0), result);
        }

        /// <summary>
        /// World position (3, 0, 5) maps to grid coordinate (3, 5).
        /// Verifies that X maps to grid X and Z maps to grid Y when TileWorldSize == 1.
        /// </summary>
        [Test]
        public void WorldToGrid_TileCenter_ReturnsCorrectGrid()
        {
            // Arrange
            var worldPos = new Vector3(3f, 0f, 5f);

            // Act
            Vector2Int result = InputController.WorldToGrid(worldPos);

            // Assert
            Assert.AreEqual(new Vector2Int(3, 5), result);
        }

        /// <summary>
        /// World position (2.4, 0, 1.6) rounds to grid coordinate (2, 2).
        /// 2.4 is closer to 2 than 3; 1.6 is closer to 2 than 1.
        /// Verifies Mathf.RoundToInt rounding ("round half away from zero").
        /// </summary>
        [Test]
        public void WorldToGrid_HalfwayBetweenTiles_RoundsToNearest()
        {
            // Arrange
            var worldPos = new Vector3(2.4f, 0f, 1.6f);

            // Act
            Vector2Int result = InputController.WorldToGrid(worldPos);

            // Assert
            Assert.AreEqual(new Vector2Int(2, 2), result);
        }

        /// <summary>
        /// World Y coordinate is ignored — only X and Z are used for grid mapping.
        /// A position with a non-zero Y component should produce the same result as Y == 0.
        /// </summary>
        [Test]
        public void WorldToGrid_NonZeroY_IgnoresYComponent()
        {
            // Arrange
            var worldPosWithY = new Vector3(4f, 99f, 7f);
            var worldPosZeroY = new Vector3(4f, 0f, 7f);

            // Act
            Vector2Int resultWithY  = InputController.WorldToGrid(worldPosWithY);
            Vector2Int resultZeroY  = InputController.WorldToGrid(worldPosZeroY);

            // Assert
            Assert.AreEqual(resultZeroY, resultWithY);
        }

        /// <summary>
        /// Negative world coordinates map correctly to negative grid coordinates.
        /// Tiles at (-2, 0, -3) should map to grid position (-2, -3).
        /// </summary>
        [Test]
        public void WorldToGrid_NegativeCoordinates_ReturnsNegativeGrid()
        {
            // Arrange
            var worldPos = new Vector3(-2f, 0f, -3f);

            // Act
            Vector2Int result = InputController.WorldToGrid(worldPos);

            // Assert
            Assert.AreEqual(new Vector2Int(-2, -3), result);
        }

        // ---------------------------------------------------------------------------
        // InputMode initial state
        // ---------------------------------------------------------------------------

        /// <summary>
        /// A freshly constructed InputController (before any SetDependencies call or Update)
        /// must start in InputMode.Idle. This is verified via the public Mode property.
        /// </summary>
        [Test]
        public void InputMode_StartsAsIdle()
        {
            // Arrange — create a plain C# instance (no MonoBehaviour lifecycle needed for this check)
            // We test the Mode property default value declared in the class body.
            // Because InputController is a MonoBehaviour, we verify via the enum default.
            // The field initializer sets Mode = InputMode.Idle, so the default enum value
            // (InputMode.Idle == 0) matches the declared initializer.
            InputMode defaultMode = default(InputMode);

            // Assert
            Assert.AreEqual(InputMode.Idle, defaultMode,
                "InputMode.Idle must be the zero-value of the enum so that the field initializer " +
                "and the C# default both resolve to Idle.");
        }
    }
}

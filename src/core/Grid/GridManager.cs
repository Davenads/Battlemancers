using UnityEngine;
using Battlemancers.Core.Grid;

namespace Battlemancers.Unity.Grid
{
    /// <summary>
    /// Unity MonoBehaviour that owns the GridData instance and bridges the simulation
    /// layer to the Unity presentation layer.
    ///
    /// Responsibilities of this class:
    /// <list type="bullet">
    ///   <item><description>Create and hold the authoritative GridData on Awake.</description></item>
    ///   <item><description>Instantiate 3D tile GameObjects matching the grid dimensions (TODO).</description></item>
    ///   <item><description>Listen for TileStateChangedEvent from the SimulationEventBus and
    ///   update tile visuals accordingly (TODO).</description></item>
    ///   <item><description>Convert between Unity world space and grid coordinates.</description></item>
    /// </list>
    ///
    /// All game logic lives in GridData — this class only handles Unity integration.
    /// Never put simulation logic in this MonoBehaviour.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        // --- Inspector configuration ---

        [SerializeField]
        [Tooltip("Number of columns (X axis). Standard sizes: 24, 32, 48.")]
        private int gridWidth = 24;

        [SerializeField]
        [Tooltip("Number of rows (Y axis). Standard sizes: 24, 32, 48.")]
        private int gridHeight = 24;

        [SerializeField]
        [Tooltip("World-space size of each tile in Unity units. Matches the scale of the tile mesh prefab.")]
        private float tileSize = 1.0f;

        [SerializeField]
        [Tooltip("Placeholder tile mesh prefab. One will be instantiated per tile on startup.")]
        private GameObject tilePrefab;

        // --- Public access to simulation data ---

        /// <summary>
        /// The authoritative GridData instance for the current match.
        /// Read by the simulation layer (TurnManager, SpellResolver, etc.) and by other
        /// presentation MonoBehaviours (UnitViewController, TileViewController).
        /// Never null after Awake() has run.
        /// </summary>
        public GridData GridData { get; private set; }

        // --- Unity lifecycle ---

        private void Awake()
        {
            GridData = new GridData(gridWidth, gridHeight);

            // TODO: Instantiate tile GameObjects based on GridData dimensions.
            //       For each tile in GridData, spawn a tilePrefab at GridToWorld(tile.Position)
            //       and store a reference so TileViewController can swap mesh/material on state change.

            // TODO: Subscribe to SimulationEventBus.OnTileStateChanged to update tile visuals
            //       when the simulation mutates a tile state during turn resolution.
        }

        // --- Coordinate conversion ---

        /// <summary>
        /// Converts a grid position to Unity world space using a flat isometric projection.
        /// The isometric camera rig (60-degree pitch) projects the XY grid onto the XZ plane,
        /// so grid Y maps to world Z (depth) and grid X maps to world X (horizontal).
        ///
        /// TODO: Replace with proper isometric offset once the camera rig is set up.
        ///       True isometric staggering (e.g., tile Y contributes to both world X and Z)
        ///       should be configured here to match the Cinemachine isometric camera angle.
        /// </summary>
        /// <param name="pos">Grid position to convert.</param>
        /// <returns>World-space Vector3 at ground level (Y = 0) for this tile.</returns>
        public Vector3 GridToWorld(GridPosition pos)
        {
            // Flat projection placeholder — implement offset math when camera rig is finalized.
            return new Vector3(pos.X * tileSize, 0f, pos.Y * tileSize);
        }

        /// <summary>
        /// Converts a Unity world-space position to the nearest grid position.
        /// Rounds to the nearest integer tile coordinate on each axis.
        ///
        /// Used for: translating mouse ray casts on the terrain plane into tile selections,
        /// snapping unit drag-and-drop to valid tiles in the warband placement phase.
        ///
        /// Note: world Y (vertical) is ignored — grid position is purely XZ plane.
        /// </summary>
        /// <param name="worldPos">Unity world-space position (typically from a ray cast hit).</param>
        /// <returns>The nearest GridPosition to the given world position.</returns>
        public GridPosition WorldToGrid(Vector3 worldPos)
        {
            return new GridPosition(
                Mathf.RoundToInt(worldPos.x / tileSize),
                Mathf.RoundToInt(worldPos.z / tileSize)
            );
        }
    }
}

using UnityEngine;
using Battlemancers.Core.Grid;

namespace Battlemancers.UI
{
    /// <summary>
    /// MonoBehaviour attached to each tile GameObject in the scene.
    ///
    /// Bridges the Unity scene representation of a tile and the simulation layer's
    /// GridPosition coordinate system. PlayerInputController reads GridPosition from
    /// this component after a raycast hit to determine which tile the player clicked.
    ///
    /// Full visual implementation (tile highlight, state mesh swapping, VFX hookup)
    /// is deferred to the visual-presentation agent. This stub exposes the GridPosition
    /// property required by PlayerInputController so the project compiles.
    /// </summary>
    public class TileView : MonoBehaviour
    {
        /// <summary>
        /// The grid coordinate this tile occupies on the battlefield.
        /// Set at scene construction time by the tile spawner / map loader.
        /// </summary>
        public GridPosition GridPosition { get; set; }
    }
}

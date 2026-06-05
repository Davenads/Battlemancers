using System;
using System.Collections.Generic;
using UnityEngine;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Unity;

namespace Battlemancers.Presentation
{
    // BOOTSTRAPPER WIRING REQUIRED:
    // moveSelectionUI.SetDependencies(gridRenderer, sim);
    // actionMenuUI.SetDependencies(actionMenuPanel, moveSelectionUI, spellSelectionUI, hotseatOrchestrator);
    // actionMenuUI.ActionCommitted += hotseatOrchestrator.OnUnitActionCommitted;

    /// <summary>
    /// Manages the tile-selection flow when a player chooses to move a unit.
    ///
    /// Flow:
    ///   1. EnterMoveMode(unit, state) — computes valid destination tiles and highlights them.
    ///   2. OnTileClicked(tile)        — fires MoveConfirmed if the tile is valid; ignores it otherwise.
    ///   3. ExitMoveMode()             — clears highlights and resets internal state.
    ///
    /// This class must not mutate SimulationState. It reads state to compute valid tiles and
    /// delegates all visual highlighting to GridRenderer. Actual move execution is handled
    /// downstream via the MoveConfirmed event → ActionMenuUI → PlannedActivation pipeline.
    ///
    /// All dependencies are injected by BattleSceneBootstrapper via SetDependencies().
    /// No FindObjectOfType. No static access.
    /// </summary>
    public class MoveSelectionUI : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Fallback movement range used when a UnitState does not carry a MoveRange value.
        /// In practice UnitState always sets MoveRange at construction, but this guard
        /// prevents a zero-range situation if a unit is constructed without a value.
        /// </summary>
        private const int DefaultMoveRange = 4;

        /// <summary>
        /// String key used to look for the Rooted status in UnitState.ActiveStatusTypes.
        /// Matches the StatusType.Rooted enum name used throughout the simulation layer.
        /// </summary>
        private const string StatusKeyRooted = "Rooted";

        // ---------------------------------------------------------------------------
        // Events
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Raised when the player confirms a valid move destination.
        /// First argument is the unit being moved; second is the target grid tile.
        /// Subscribers (ActionMenuUI) should package this into a MoveCommand / PlannedActivation.
        /// </summary>
        public event Action<UnitState, Vector2Int> MoveConfirmed;

        // ---------------------------------------------------------------------------
        // Injected dependencies (set by BattleSceneBootstrapper)
        // ---------------------------------------------------------------------------

        private GridRenderer           _gridRenderer;
        private SimulationBootstrapper _sim;

        // ---------------------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------------------

        private UnitState          _activeUnit;
        private HashSet<Vector2Int> _validTiles = new HashSet<Vector2Int>();
        private bool               _inMoveMode;

        // ---------------------------------------------------------------------------
        // Dependency injection — called by BattleSceneBootstrapper
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Injects dependencies. Must be called by BattleSceneBootstrapper before any
        /// EnterMoveMode / OnTileClicked calls.
        /// </summary>
        public void SetDependencies(GridRenderer gridRenderer, SimulationBootstrapper sim)
        {
            _gridRenderer = gridRenderer;
            _sim          = sim;
        }

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Enters move-selection mode for the given unit.
        /// Computes all tiles the unit can legally move to:
        /// <list type="bullet">
        ///   <item>Manhattan distance &lt;= unit's MoveRange (or DefaultMoveRange if 0).</item>
        ///   <item>Tile is in bounds.</item>
        ///   <item>Tile is passable (not Destroyed or Obsidian).</item>
        ///   <item>Tile is not occupied by another unit.</item>
        /// </list>
        /// If the unit has the ROOTED status, the valid tile set is empty (unit cannot move).
        ///
        /// GridRenderer.HighlightTiles is not yet implemented on GridRenderer. Once it is added,
        /// call: _gridRenderer.HighlightTiles(_validTiles, HighlightColor.Move) here.
        /// Until then the valid tile set is computed and stored for OnTileClicked validation.
        /// </summary>
        /// <param name="unit">The unit the player is moving.</param>
        /// <param name="state">The current SimulationState to read grid and occupancy from.</param>
        public void EnterMoveMode(UnitState unit, SimulationState state)
        {
            if (unit == null)
            {
                Debug.LogWarning("[MoveSelectionUI] EnterMoveMode called with null unit.");
                return;
            }

            if (state == null)
            {
                Debug.LogWarning("[MoveSelectionUI] EnterMoveMode called with null state.");
                return;
            }

            ExitMoveMode();

            _activeUnit  = unit;
            _inMoveMode  = true;
            _validTiles  = ComputeValidTiles(unit, state);

            // GridRenderer.HighlightTiles is not yet implemented.
            // When added to GridRenderer, wire it here:
            //   _gridRenderer?.HighlightTiles(_validTiles, HighlightColor.Move);
            Debug.Log($"[MoveSelectionUI] Move mode entered for '{unit.Id}'. {_validTiles.Count} valid tile(s).");
        }

        /// <summary>
        /// Called when the player clicks a tile during move-selection mode.
        /// If the tile is in the valid set, fires MoveConfirmed and exits move mode.
        /// If not in the valid set or not in move mode, the click is silently ignored.
        /// </summary>
        /// <param name="tile">The grid position the player clicked.</param>
        public void OnTileClicked(Vector2Int tile)
        {
            if (!_inMoveMode || _activeUnit == null)
                return;

            if (!_validTiles.Contains(tile))
                return;

            Debug.Log($"[MoveSelectionUI] Move confirmed: '{_activeUnit.Id}' → ({tile.x},{tile.y}).");
            UnitState confirmedUnit = _activeUnit;
            ExitMoveMode();
            MoveConfirmed?.Invoke(confirmedUnit, tile);
        }

        /// <summary>
        /// Clears the tile-highlight overlay and resets internal state.
        /// Safe to call at any time, including when not in move mode.
        /// </summary>
        public void ExitMoveMode()
        {
            _inMoveMode = false;
            _activeUnit = null;
            _validTiles.Clear();

            // When GridRenderer.ClearHighlights is implemented, call it here.
            // _gridRenderer?.ClearHighlights();
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Computes the set of tiles the unit may legally move to given the current state.
        /// Returns an empty set if the unit is ROOTED.
        /// </summary>
        private static HashSet<Vector2Int> ComputeValidTiles(UnitState unit, SimulationState state)
        {
            var result = new HashSet<Vector2Int>();

            // ROOTED units cannot move at all.
            if (unit.ActiveStatusTypes.Contains(StatusKeyRooted))
                return result;

            int range      = unit.MoveRange > 0 ? unit.MoveRange : DefaultMoveRange;
            GridData grid  = state.Grid;
            GridPosition origin = unit.Position;

            // Build a fast lookup set of occupied positions (excluding this unit's own tile).
            var occupiedPositions = new HashSet<GridPosition>();
            foreach (UnitState other in state.GetAllUnits())
            {
                if (other.Id != unit.Id)
                    occupiedPositions.Add(other.Position);
            }

            // Iterate the Manhattan diamond around the unit's position.
            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    // Manhattan distance filter (diamond shape).
                    if (Math.Abs(dx) + Math.Abs(dy) > range)
                        continue;

                    // Skip the unit's own position — "move to self" is a pass, handled elsewhere.
                    if (dx == 0 && dy == 0)
                        continue;

                    var candidate = new GridPosition(origin.X + dx, origin.Y + dy);

                    // Must be in grid bounds.
                    if (!grid.IsInBounds(candidate))
                        continue;

                    // Must be passable (Destroyed and Obsidian are impassable per Tile.SetState).
                    if (!grid.IsPassable(candidate))
                        continue;

                    // Must not be occupied by another unit.
                    if (occupiedPositions.Contains(candidate))
                        continue;

                    result.Add(new Vector2Int(candidate.X, candidate.Y));
                }
            }

            return result;
        }
    }
}

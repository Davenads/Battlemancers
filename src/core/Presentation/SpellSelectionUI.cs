using System;
using System.Collections.Generic;
using UnityEngine;
using Battlemancers.Core.Data;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Unity;
using Battlemancers.UI.Battle;

namespace Battlemancers.Presentation
{
    /// <summary>
    /// Manages the spell targeting flow for a single Mancer.
    ///
    /// Flow:
    ///   1. ShowSpells(caster) — populates SpellButtonPanel with the caster's spell list.
    ///   2. OnSpellSelected(spell) — enters targeting mode: highlights valid tiles.
    ///   3. OnTargetConfirmed(tile) — packages intent and raises SpellIntentReady.
    ///   4. ClearSelection() — resets highlighting and hides the panel.
    ///
    /// This class must not import UnityEngine.UI or TMPro directly.
    /// UI rendering is delegated exclusively to SpellButtonPanel.
    ///
    /// All dependencies are injected by BattleSceneBootstrapper via SetDependencies().
    /// No FindObjectOfType. No static access.
    /// </summary>
    public class SpellSelectionUI : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Highlight color applied to tiles inside the selected spell's range.
        /// Rendered via GridRenderer tile tinting (placeholder; production would use
        /// a dedicated highlight layer).
        /// </summary>
        private static readonly Color HighlightInRange  = new Color(1.0f, 1.0f, 0.2f, 0.6f);
        private static readonly Color HighlightSelected = new Color(0.2f, 1.0f, 0.2f, 0.8f);

        // ---------------------------------------------------------------------------
        // Events
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Raised when the player has chosen a spell and confirmed a target tile.
        /// Subscribers (e.g., PlanningPhaseUI or a future PlayerInputController) should
        /// translate this into a PlannedActivation and submit it.
        /// </summary>
        public event Action<SpellCastIntent> SpellIntentReady;

        // ---------------------------------------------------------------------------
        // Injected dependencies (set by BattleSceneBootstrapper)
        // ---------------------------------------------------------------------------

        private SpellButtonPanel       _spellButtonPanel;
        private GridRenderer           _gridRenderer;
        private SimulationBootstrapper _sim;

        // ---------------------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------------------

        private UnitState          _activeCaster;
        private SpellRuntimeData   _selectedSpell;
        private List<Vector2Int>   _highlightedTiles = new List<Vector2Int>();
        private bool               _inTargetingMode;

        // ---------------------------------------------------------------------------
        // Dependency injection — called by BattleSceneBootstrapper
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Injects dependencies. Called by BattleSceneBootstrapper before any
        /// Show/Select/Confirm methods are invoked.
        /// </summary>
        public void SetDependencies(
            SpellButtonPanel spellButtonPanel,
            GridRenderer gridRenderer,
            SimulationBootstrapper sim)
        {
            _spellButtonPanel = spellButtonPanel;
            _gridRenderer     = gridRenderer;
            _sim              = sim;
        }

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Populates the SpellButtonPanel with the given caster's spell list.
        /// Marks spells unavailable when the caster lacks sufficient AP or the spell
        /// is on cooldown.
        /// </summary>
        /// <param name="caster">The UnitState of the Mancer whose spells to display.</param>
        public void ShowSpells(UnitState caster)
        {
            if (caster == null)
            {
                Debug.LogWarning("[SpellSelectionUI] ShowSpells called with null caster.");
                return;
            }

            _activeCaster = caster;
            ClearSelection();

            List<SpellRuntimeData> spells = GetSpellsForCaster(caster);

            if (_spellButtonPanel != null)
            {
                _spellButtonPanel.Populate(spells, this);

                // Mark spells that are unavailable.
                foreach (SpellRuntimeData spell in spells)
                {
                    bool onCooldown = caster.SpellCooldowns.ContainsKey(spell.SpellId);
                    bool tooExpensive = caster.ActionPoints < spell.ApCost;
                    _spellButtonPanel.SetSpellAvailable(spell, !onCooldown && !tooExpensive);
                }
            }
        }

        /// <summary>
        /// Called by SpellButtonPanel when the player clicks a spell button.
        /// Enters targeting mode: computes valid target tiles and highlights them.
        /// </summary>
        /// <param name="spell">The spell the player clicked.</param>
        public void OnSpellSelected(SpellRuntimeData spell)
        {
            if (spell == null || _activeCaster == null)
                return;

            _selectedSpell = spell;
            _inTargetingMode = true;

            HighlightValidTargets(spell);

            Debug.Log($"[SpellSelectionUI] Spell selected: {spell.DisplayName} (range {spell.Range}). Awaiting target.");
        }

        /// <summary>
        /// Called when the player clicks a tile to confirm it as the spell target.
        /// Packages the intent and raises SpellIntentReady. Exits targeting mode.
        /// </summary>
        /// <param name="tile">The grid position of the chosen target tile.</param>
        public void OnTargetConfirmed(Vector2Int tile)
        {
            if (!_inTargetingMode || _selectedSpell == null || _activeCaster == null)
                return;

            var intent = new SpellCastIntent(
                casterId: _activeCaster.Id,
                spellId:  _selectedSpell.SpellId,
                targetX:  tile.x,
                targetY:  tile.y);

            Debug.Log($"[SpellSelectionUI] Target confirmed: {_selectedSpell.DisplayName} → ({tile.x},{tile.y}).");
            SpellIntentReady?.Invoke(intent);

            ClearSelection();
        }

        /// <summary>
        /// Resets the targeting state: clears tile highlights, hides the spell panel,
        /// and sets _inTargetingMode to false.
        /// </summary>
        public void ClearSelection()
        {
            _inTargetingMode = false;
            _selectedSpell   = null;

            ClearHighlights();

            _spellButtonPanel?.Clear();
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Retrieves the spell list for the given caster from the DataRegistry.
        /// Falls back to an empty list when the registry has no data for the archetype.
        /// </summary>
        private List<SpellRuntimeData> GetSpellsForCaster(UnitState caster)
        {
            var result = new List<SpellRuntimeData>();

            if (_sim == null || _sim.DataRegistry == null || caster.MancerArchetypeId == null)
                return result;

            MancerRuntimeData mancerData = _sim.DataRegistry.GetMancer(caster.MancerArchetypeId);
            if (mancerData?.Spells == null)
                return result;

            result.AddRange(mancerData.Spells);
            return result;
        }

        /// <summary>
        /// Computes and highlights all tiles within range of the selected spell.
        /// Uses Manhattan distance from the caster's position.
        /// Only highlights tiles that are in bounds.
        /// </summary>
        private void HighlightValidTargets(SpellRuntimeData spell)
        {
            ClearHighlights();

            if (_sim?.State == null || _activeCaster == null)
                return;

            GridData grid = _sim.State.Grid;
            GridPosition casterPos = _activeCaster.Position;
            int range = spell.Range;

            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    // Manhattan distance filter.
                    if (Math.Abs(dx) + Math.Abs(dy) > range)
                        continue;

                    var candidate = new GridPosition(casterPos.X + dx, casterPos.Y + dy);
                    if (!grid.IsInBounds(candidate))
                        continue;

                    _highlightedTiles.Add(new Vector2Int(candidate.X, candidate.Y));
                }
            }

            // Visual highlight is a future extension — the tile highlight layer would call
            // GridRenderer.HighlightTile (not yet implemented on GridRenderer).
            // For the prototype, we log the count so other systems can subscribe to the event.
            Debug.Log($"[SpellSelectionUI] {_highlightedTiles.Count} tile(s) highlighted for {spell.DisplayName}.");
        }

        /// <summary>
        /// Removes all tile highlights produced by the current targeting session.
        /// </summary>
        private void ClearHighlights()
        {
            // When a dedicated highlight layer is added to GridRenderer, call it here.
            _highlightedTiles.Clear();
        }
    }

    // =========================================================================
    // SpellCastIntent — data class
    // =========================================================================

    /// <summary>
    /// Packages a player's spell cast decision: which caster, which spell, and
    /// which target tile. Raised by SpellSelectionUI.SpellIntentReady so that the
    /// planning layer (PlanningPhaseUI or a future PlayerInputController) can translate
    /// the intent into a SpellCommand and include it in a PlannedActivation.
    /// </summary>
    public sealed class SpellCastIntent
    {
        /// <summary>Runtime ID of the caster unit.</summary>
        public string CasterId { get; }

        /// <summary>ID of the spell being cast.</summary>
        public string SpellId { get; }

        /// <summary>Target tile grid X coordinate.</summary>
        public int TargetX { get; }

        /// <summary>Target tile grid Y coordinate.</summary>
        public int TargetY { get; }

        /// <summary>Initializes a new SpellCastIntent.</summary>
        public SpellCastIntent(string casterId, string spellId, int targetX, int targetY)
        {
            CasterId = casterId;
            SpellId  = spellId;
            TargetX  = targetX;
            TargetY  = targetY;
        }
    }
}

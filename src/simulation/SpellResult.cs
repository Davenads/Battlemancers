using Battlemancers.Core.Grid;

namespace Battlemancers.Simulation
{
    /// <summary>
    /// Immutable record of the full outcome of a single spell cast, returned by
    /// <see cref="SpellResolver.Resolve"/> after all effects have been applied to
    /// <see cref="Battlemancers.Core.Simulation.SimulationState"/>.
    ///
    /// SpellResult is a pure data snapshot — it does not hold references to live
    /// simulation objects. The presentation layer and any post-resolution systems
    /// can safely read and archive these results without worrying about mutation.
    ///
    /// Pure C# — zero Unity dependencies.
    /// </summary>
    public sealed class SpellResult
    {
        // -----------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Runtime ID of the Mancer that cast the spell
        /// (e.g., "p1_pyromancer_0").
        /// </summary>
        public string CasterId { get; }

        /// <summary>
        /// Definition ID of the spell that was cast
        /// (e.g., "pyromancer_fireball_standard").
        /// Matches <c>SpellData.spellId</c>.
        /// </summary>
        public string SpellId { get; }

        /// <summary>
        /// The grid tile the spell was aimed at. For AoE spells this is the blast
        /// centre; for line spells it is the far end of the line.
        /// </summary>
        public GridPosition TargetPosition { get; }

        /// <summary>
        /// One <see cref="HitRecord"/> per unit that was struck by the spell.
        /// Empty if no units were in the affected area.
        /// </summary>
        public HitRecord[] Hits { get; }

        /// <summary>
        /// One <see cref="TileStateChange"/> per tile whose elemental state was
        /// mutated by this spell (including element-interaction results).
        /// Empty if no tiles changed state.
        /// </summary>
        public TileStateChange[] TileChanges { get; }

        /// <summary>
        /// <c>true</c> when the spell was cast but produced no meaningful outcome —
        /// no units were hit and no tile states changed. A fizzle does not refund AP
        /// or cooldown; the spell still resolves and goes on cooldown.
        /// </summary>
        public bool WasFizzled { get; }

        // -----------------------------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new <see cref="SpellResult"/>.
        /// </summary>
        /// <param name="casterId">Runtime ID of the casting Mancer.</param>
        /// <param name="spellId">Definition ID of the spell that was cast.</param>
        /// <param name="targetPosition">Grid position the spell was targeted at.</param>
        /// <param name="hits">
        /// Array of hit records — one per unit struck. Pass an empty array (not null)
        /// when no units were affected.
        /// </param>
        /// <param name="tileChanges">
        /// Array of tile state changes produced by the spell. Pass an empty array (not null)
        /// when no tiles changed state.
        /// </param>
        /// <param name="wasFizzled">
        /// <c>true</c> if the spell had no effect on units or terrain.
        /// </param>
        public SpellResult(
            string casterId,
            string spellId,
            GridPosition targetPosition,
            HitRecord[] hits,
            TileStateChange[] tileChanges,
            bool wasFizzled)
        {
            CasterId = casterId;
            SpellId = spellId;
            TargetPosition = targetPosition;
            Hits = hits ?? System.Array.Empty<HitRecord>();
            TileChanges = tileChanges ?? System.Array.Empty<TileStateChange>();
            WasFizzled = wasFizzled;
        }
    }

    // =============================================================================================
    // Supporting record types
    // =============================================================================================

    /// <summary>
    /// Describes the effect of a single spell hit on one unit.
    ///
    /// One <see cref="HitRecord"/> is produced per unit affected by a spell, regardless
    /// of whether the unit survived. The presentation layer uses these records to trigger
    /// hit animations, damage numbers, and death sequences.
    /// </summary>
    public sealed class HitRecord
    {
        /// <summary>
        /// Runtime ID of the unit that was struck
        /// (e.g., "p2_hydromancer_0").
        /// </summary>
        public string UnitId { get; }

        /// <summary>
        /// Net HP lost by this unit as a result of the spell hit, after armor reduction.
        /// Clamped to 0 — never negative.
        /// </summary>
        public int DamageTaken { get; }

        /// <summary>
        /// Status effect type names applied to this unit during this hit
        /// (e.g., <c>["Burning", "Slowed"]</c>).
        /// Empty if no statuses were applied.
        /// These names match <see cref="Battlemancers.Simulation.Status.StatusType"/> enum values.
        /// </summary>
        public string[] StatusesApplied { get; }

        /// <summary>
        /// <c>true</c> if this hit reduced the unit's HP to zero, causing it to be
        /// removed from the simulation via
        /// <see cref="Battlemancers.Core.Simulation.SimulationState.DeregisterUnit"/>.
        /// </summary>
        public bool WasKilled { get; }

        /// <summary>
        /// Initializes a new <see cref="HitRecord"/>.
        /// </summary>
        /// <param name="unitId">Runtime ID of the unit that was struck.</param>
        /// <param name="damageTaken">Net HP lost after armor. Must be &gt;= 0.</param>
        /// <param name="statusesApplied">
        /// Status type names applied to the unit. Pass an empty array (not null) when none.
        /// </param>
        /// <param name="wasKilled"><c>true</c> if this hit killed the unit.</param>
        public HitRecord(string unitId, int damageTaken, string[] statusesApplied, bool wasKilled)
        {
            UnitId = unitId;
            DamageTaken = damageTaken;
            StatusesApplied = statusesApplied ?? System.Array.Empty<string>();
            WasKilled = wasKilled;
        }
    }

    /// <summary>
    /// Records a single tile whose elemental state changed as a result of a spell cast,
    /// including both the state before the change and the new state.
    ///
    /// Used by the replay system and the presentation layer (tile mesh swap, terrain VFX).
    /// </summary>
    public sealed class TileStateChange
    {
        /// <summary>Grid position of the tile that changed.</summary>
        public GridPosition Position { get; }

        /// <summary>The tile's elemental state immediately before the change.</summary>
        public TileState OldState { get; }

        /// <summary>The tile's elemental state immediately after the change.</summary>
        public TileState NewState { get; }

        /// <summary>
        /// Initializes a new <see cref="TileStateChange"/>.
        /// </summary>
        /// <param name="position">Grid position of the changed tile.</param>
        /// <param name="oldState">State of the tile before the spell resolved.</param>
        /// <param name="newState">State of the tile after the spell resolved.</param>
        public TileStateChange(GridPosition position, TileState oldState, TileState newState)
        {
            Position = position;
            OldState = oldState;
            NewState = newState;
        }
    }
}

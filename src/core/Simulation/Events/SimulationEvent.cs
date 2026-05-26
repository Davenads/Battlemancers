using System;
using Battlemancers.Core.Grid;

namespace Battlemancers.Core.Simulation.Events
{
    /// <summary>
    /// Base class for all simulation events. Events are the communication boundary
    /// between the pure C# simulation layer and the Unity presentation layer.
    /// The simulation emits events; the presentation layer consumes them to drive
    /// animation, VFX, audio, and UI updates. The simulation never references Unity APIs.
    /// </summary>
    public abstract class SimulationEvent
    {
        /// <summary>The turn number during which this event was generated.</summary>
        public int TurnNumber { get; }

        protected SimulationEvent(int turnNumber)
        {
            TurnNumber = turnNumber;
        }
    }

    // ---------------------------------------------------------------------------
    // Enums shared across simulation events and turn management
    // ---------------------------------------------------------------------------

    /// <summary>Reason a match ended.</summary>
    public enum MatchEndReason
    {
        /// <summary>One player's Mancers were all eliminated.</summary>
        AllEnemyMancersEliminated,

        /// <summary>The turn limit was reached and neither player eliminated the other.</summary>
        TurnLimitReached,

        /// <summary>A mission-specific objective was completed before Mancer elimination.</summary>
        MissionObjectiveComplete,

        /// <summary>A player voluntarily surrendered.</summary>
        Concession,

        /// <summary>Both players had their last Mancer die simultaneously.</summary>
        Draw
    }

    /// <summary>Broad category of a unit — determines activation cost and initiative order.</summary>
    public enum UnitType
    {
        /// <summary>Elemental spellcaster. Activates for 100 pts; resolves first in initiative.</summary>
        Mancer,

        /// <summary>Faction-specific melee infantry. Cheap but resolves last.</summary>
        Chaff,

        /// <summary>Faction-specific ranged unit. Resolves between Mancers and Chaff.</summary>
        Ranged
    }

    /// <summary>Phase of the current turn in the simultaneous blind turn system.</summary>
    public enum TurnPhase
    {
        /// <summary>Both players are building their activation plans (hidden from opponent).</summary>
        Planning,

        /// <summary>Both players have confirmed their plans; awaiting resolution trigger.</summary>
        Locked,

        /// <summary>Plans are being executed in initiative order.</summary>
        Resolving,

        /// <summary>All actions have resolved; terrain/status ticks are being processed.</summary>
        Ended
    }

    // ---------------------------------------------------------------------------
    // Concrete event types
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Fired when a unit successfully moves from one tile to another.
    /// The presentation layer uses this to animate the unit along its path.
    /// </summary>
    public sealed class UnitMovedEvent : SimulationEvent
    {
        /// <summary>Runtime ID of the unit that moved.</summary>
        public string UnitId { get; }

        /// <summary>Grid position the unit moved from.</summary>
        public GridPosition From { get; }

        /// <summary>Grid position the unit moved to.</summary>
        public GridPosition To { get; }

        public UnitMovedEvent(int turnNumber, string unitId, GridPosition from, GridPosition to)
            : base(turnNumber)
        {
            UnitId = unitId;
            From = from;
            To = to;
        }
    }

    /// <summary>
    /// Fired when a Mancer begins casting a spell. Published before spell effects resolve.
    /// The VFX layer uses this to start the cast wind-up animation.
    /// </summary>
    public sealed class SpellCastEvent : SimulationEvent
    {
        /// <summary>Runtime ID of the Mancer casting the spell.</summary>
        public string CasterId { get; }

        /// <summary>Definition ID of the spell being cast (e.g., "pyromancer_fireball").</summary>
        public string SpellId { get; }

        /// <summary>Grid position the spell is targeting.</summary>
        public GridPosition Target { get; }

        public SpellCastEvent(int turnNumber, string casterId, string spellId, GridPosition target)
            : base(turnNumber)
        {
            CasterId = casterId;
            SpellId = spellId;
            Target = target;
        }
    }

    /// <summary>
    /// Fired when a spell connects with its target area and applies its effects.
    /// Published after SpellCastEvent; contains impact data for VFX and audio.
    /// </summary>
    public sealed class SpellHitEvent : SimulationEvent
    {
        /// <summary>Runtime ID of the Mancer who cast the spell.</summary>
        public string CasterId { get; }

        /// <summary>Definition ID of the spell that hit.</summary>
        public string SpellId { get; }

        /// <summary>Tile at the center of the spell's impact.</summary>
        public GridPosition HitPosition { get; }

        /// <summary>Total damage dealt by this hit (summed across all targets).</summary>
        public int Damage { get; }

        /// <summary>Runtime IDs of all units affected by this spell hit.</summary>
        public string[] AffectedUnitIds { get; }

        public SpellHitEvent(int turnNumber, string casterId, string spellId,
                             GridPosition hitPosition, int damage, string[] affectedUnitIds)
            : base(turnNumber)
        {
            CasterId = casterId;
            SpellId = spellId;
            HitPosition = hitPosition;
            Damage = damage;
            AffectedUnitIds = affectedUnitIds ?? Array.Empty<string>();
        }
    }

    /// <summary>
    /// Fired when a tile's elemental state changes (e.g., Normal → Burning, Wet → Frozen).
    /// The TileViewController uses this to swap tile meshes and trigger terrain VFX.
    /// </summary>
    public sealed class TileStateChangedEvent : SimulationEvent
    {
        /// <summary>Grid position of the tile that changed.</summary>
        public GridPosition Position { get; }

        /// <summary>The tile's state before the change.</summary>
        public TileState PreviousState { get; }

        /// <summary>The tile's new state after the change.</summary>
        public TileState NewState { get; }

        /// <summary>
        /// Hint to the VFX director about which effect to play for this transition.
        /// Examples: "steam_burst", "fire_ignite", "ice_shatter", "obsidian_form".
        /// Empty string means no special VFX for this transition.
        /// </summary>
        public string VfxHint { get; }

        public TileStateChangedEvent(int turnNumber, GridPosition position,
                                     TileState previousState, TileState newState, string vfxHint)
            : base(turnNumber)
        {
            Position = position;
            PreviousState = previousState;
            NewState = newState;
            VfxHint = vfxHint ?? string.Empty;
        }
    }

    /// <summary>
    /// Fired when a status effect is applied to a unit or its stack count increases.
    /// </summary>
    public sealed class UnitStatusAppliedEvent : SimulationEvent
    {
        /// <summary>Runtime ID of the unit receiving the status.</summary>
        public string UnitId { get; }

        /// <summary>String key identifying the status type (e.g., "Burning", "Poisoned", "Frozen").</summary>
        public string StatusType { get; }

        /// <summary>How many turns this application lasts (or was refreshed to).</summary>
        public int Duration { get; }

        /// <summary>Total stack count of this status on the unit after application.</summary>
        public int StackCount { get; }

        public UnitStatusAppliedEvent(int turnNumber, string unitId, string statusType,
                                      int duration, int stackCount)
            : base(turnNumber)
        {
            UnitId = unitId;
            StatusType = statusType;
            Duration = duration;
            StackCount = stackCount;
        }
    }

    /// <summary>
    /// Fired when a status effect expires or is cleansed from a unit.
    /// </summary>
    public sealed class UnitStatusRemovedEvent : SimulationEvent
    {
        /// <summary>Runtime ID of the unit that lost the status.</summary>
        public string UnitId { get; }

        /// <summary>String key identifying the status type that was removed.</summary>
        public string StatusType { get; }

        public UnitStatusRemovedEvent(int turnNumber, string unitId, string statusType)
            : base(turnNumber)
        {
            UnitId = unitId;
            StatusType = statusType;
        }
    }

    /// <summary>
    /// Fired whenever a unit takes damage from any source (spells, terrain, attacks).
    /// </summary>
    public sealed class UnitDamagedEvent : SimulationEvent
    {
        /// <summary>Runtime ID of the unit that took damage.</summary>
        public string UnitId { get; }

        /// <summary>Amount of HP lost in this damage instance.</summary>
        public int DamageAmount { get; }

        /// <summary>
        /// Identifier for the damage source. May be a spell ID, a unit ID, or a terrain descriptor
        /// such as "terrain_burning" or "status_poison".
        /// </summary>
        public string DamageSource { get; }

        /// <summary>Unit's HP after applying this damage (floored at 0).</summary>
        public int RemainingHP { get; }

        public UnitDamagedEvent(int turnNumber, string unitId, int damageAmount,
                                string damageSource, int remainingHP)
            : base(turnNumber)
        {
            UnitId = unitId;
            DamageAmount = damageAmount;
            DamageSource = damageSource;
            RemainingHP = remainingHP;
        }
    }

    /// <summary>
    /// Fired whenever a unit recovers HP from any healing source.
    /// </summary>
    public sealed class UnitHealedEvent : SimulationEvent
    {
        /// <summary>Runtime ID of the unit that was healed.</summary>
        public string UnitId { get; }

        /// <summary>Amount of HP recovered.</summary>
        public int HealAmount { get; }

        /// <summary>Unit's HP after applying the heal (capped at MaxHP).</summary>
        public int ResultingHP { get; }

        public UnitHealedEvent(int turnNumber, string unitId, int healAmount, int resultingHP)
            : base(turnNumber)
        {
            UnitId = unitId;
            HealAmount = healAmount;
            ResultingHP = resultingHP;
        }
    }

    /// <summary>
    /// Fired when a unit's HP reaches 0 and it is removed from the simulation.
    /// The presentation layer uses this to trigger death animations and remove the unit visually.
    /// </summary>
    public sealed class UnitDiedEvent : SimulationEvent
    {
        /// <summary>Runtime ID of the unit that died.</summary>
        public string UnitId { get; }

        /// <summary>The grid position the unit occupied when it died.</summary>
        public GridPosition Position { get; }

        /// <summary>
        /// Runtime ID of the unit that dealt the killing blow.
        /// Null if the kill was from terrain damage, a status effect, or an indirect source.
        /// </summary>
        public string KillerUnitId { get; }

        public UnitDiedEvent(int turnNumber, string unitId, GridPosition position, string killerUnitId)
            : base(turnNumber)
        {
            UnitId = unitId;
            Position = position;
            KillerUnitId = killerUnitId;
        }
    }

    /// <summary>
    /// Fired at the end of each turn after all commands have executed and terrain/status ticks
    /// have been processed.
    /// </summary>
    public sealed class TurnResolvedEvent : SimulationEvent
    {
        /// <summary>How many individual command executions occurred this turn.</summary>
        public int TotalActionsResolved { get; }

        public TurnResolvedEvent(int turnNumber, int totalActionsResolved)
            : base(turnNumber)
        {
            TotalActionsResolved = totalActionsResolved;
        }
    }

    /// <summary>
    /// Fired when the match ends. Consumed by the presentation layer to show results screen
    /// and by the multiplayer layer to notify both clients.
    /// </summary>
    public sealed class MatchEndedEvent : SimulationEvent
    {
        /// <summary>
        /// Runtime player ID of the winner. Null in the case of a draw
        /// (both players lost all Mancers simultaneously, or turn limit hit with equal standing).
        /// </summary>
        public string WinnerId { get; }

        /// <summary>The reason the match ended.</summary>
        public MatchEndReason Reason { get; }

        public MatchEndedEvent(int turnNumber, string winnerId, MatchEndReason reason)
            : base(turnNumber)
        {
            WinnerId = winnerId;
            Reason = reason;
        }
    }
}

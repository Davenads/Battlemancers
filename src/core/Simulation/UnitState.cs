using System.Collections.Generic;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation.Events;

namespace Battlemancers.Core.Simulation
{
    /// <summary>
    /// Runtime state of a single unit in the simulation.
    /// Mutable — the simulation mutates these values during turn resolution.
    ///
    /// UnitState is the source of truth for a unit's position, HP, cooldowns,
    /// and activation status. The presentation layer reads UnitState (via events)
    /// but never writes to it directly.
    ///
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public class UnitState
    {
        // ---------------------------------------------------------------------------
        // Identity (immutable after construction)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Unique runtime ID for this unit instance. Generated at warband registration,
        /// stable for the duration of the match.
        /// Format convention: "{ownerId}_{archetypeId}_{index}" e.g. "p1_pyromancer_0".
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// ID of the Mancer archetype definition (e.g., "pyromancer", "hydromancer").
        /// Null for Chaff and Ranged units that have no Mancer archetype.
        /// </summary>
        public string MancerArchetypeId { get; }

        /// <summary>
        /// Which category of unit this is — affects initiative resolution order and
        /// the activation budget cost charged per turn.
        /// </summary>
        public UnitType Type { get; }

        /// <summary>The player ID that controls this unit.</summary>
        public string OwnerId { get; }

        /// <summary>
        /// Warband point cost of this unit. Used to compute activation budget totals
        /// when the player submits their turn plan.
        /// Note: Mancers always count as 100 pts against the activation budget regardless
        /// of upgrade cost — the PointCost here reflects the warband spend, not activation cost.
        /// </summary>
        public int PointCost { get; }

        /// <summary>Maximum HP this unit can have (set at construction; not modified by upgrades mid-match).</summary>
        public int MaxHP { get; }

        // ---------------------------------------------------------------------------
        // Mutable simulation state
        // ---------------------------------------------------------------------------

        /// <summary>Current grid position. Updated by MoveCommand.Execute().</summary>
        public GridPosition Position { get; set; }

        /// <summary>Current hit points. Reaches 0 on death; never goes negative.</summary>
        public int CurrentHP { get; set; }

        /// <summary>
        /// Base movement range in tiles (Manhattan distance).
        /// May be temporarily modified by status effects (Frozen reduces move range).
        /// </summary>
        public int MoveRange { get; set; }

        /// <summary>
        /// Current temperature of this unit on the range [-100, +100].
        /// 0 = neutral (room temperature). Positive values indicate heat accumulation;
        /// negative values indicate cold accumulation.
        /// Temperature decays 10 points toward 0 each turn.
        /// Threshold crossings trigger status effects via TemperatureManager.
        /// </summary>
        public int Temperature { get; set; }

        /// <summary>
        /// Action points available for the current activation.
        /// Mancers start with 6 AP; Chaff and Ranged start with 1 AP.
        /// Reset each turn by ResetForNewTurn().
        /// </summary>
        public int ActionPoints { get; set; }

        /// <summary>Whether this unit has been activated in the current turn's resolution.</summary>
        public bool ActivatedThisTurn { get; set; }

        // ---------------------------------------------------------------------------
        // Computed properties
        // ---------------------------------------------------------------------------

        /// <summary>True if the unit's CurrentHP is above zero.</summary>
        public bool IsAlive => CurrentHP > 0;

        /// <summary>
        /// Activation budget cost this unit charges when included in a turn plan.
        /// Mancers always cost 100 pts regardless of upgrade spend (warband rule).
        /// All other units cost their PointCost.
        /// </summary>
        public int ActivationCost => Type == UnitType.Mancer ? 100 : PointCost;

        // ---------------------------------------------------------------------------
        // Collections (mutable, managed by simulation systems)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// String keys of currently active status effect types on this unit.
        /// Full StatusEffect objects are managed by the StatusManager in Wave 2.
        /// This list provides a lightweight summary readable by other simulation components.
        /// </summary>
        public List<string> ActiveStatusTypes { get; } = new List<string>();

        /// <summary>
        /// Spells currently on cooldown. Maps spellId → turns remaining before usable again.
        /// Decremented each turn by TickCooldowns(). Entry removed when turns reach 0.
        /// </summary>
        public Dictionary<string, int> SpellCooldowns { get; } = new Dictionary<string, int>();

        // ---------------------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new UnitState with full combat-ready defaults.
        /// CurrentHP is set to MaxHP; ActionPoints are set based on UnitType.
        /// </summary>
        /// <param name="id">Unique runtime ID for this unit instance.</param>
        /// <param name="mancerArchetypeId">Archetype key for Mancers; null for Chaff/Ranged.</param>
        /// <param name="type">Unit category (Mancer, Chaff, or Ranged).</param>
        /// <param name="ownerId">Player ID that controls this unit.</param>
        /// <param name="position">Starting grid position.</param>
        /// <param name="maxHP">Maximum (and starting) hit points.</param>
        /// <param name="moveRange">Base movement range in tiles.</param>
        /// <param name="pointCost">Warband point cost used in activation budget calculations.</param>
        public UnitState(string id, string mancerArchetypeId, UnitType type, string ownerId,
                         GridPosition position, int maxHP, int moveRange, int pointCost)
        {
            Id = id;
            MancerArchetypeId = mancerArchetypeId;
            Type = type;
            OwnerId = ownerId;
            Position = position;
            MaxHP = maxHP;
            CurrentHP = maxHP;
            MoveRange = moveRange;
            PointCost = pointCost;

            // Mancers have 6 AP; Chaff and Ranged have 1 AP.
            ActionPoints = type == UnitType.Mancer ? 6 : 1;
            ActivatedThisTurn = false;
            Temperature = 0;
        }

        // ---------------------------------------------------------------------------
        // Turn lifecycle
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Decrements all active spell cooldowns by one turn.
        /// Cooldowns that reach zero are removed from the dictionary (spell becomes usable again).
        /// Called at the end of turn resolution, after all commands have executed.
        /// </summary>
        public void TickCooldowns()
        {
            // Collect keys to remove in a separate list — cannot modify a dictionary during iteration.
            List<string> expired = null;

            foreach (KeyValuePair<string, int> entry in SpellCooldowns)
            {
                int newValue = entry.Value - 1;
                if (newValue <= 0)
                {
                    if (expired == null) expired = new List<string>();
                    expired.Add(entry.Key);
                }
                else
                {
                    SpellCooldowns[entry.Key] = newValue;
                }
            }

            if (expired != null)
            {
                foreach (string key in expired)
                    SpellCooldowns.Remove(key);
            }
        }

        /// <summary>
        /// Resets per-turn transient state at the start of a new planning phase.
        /// Sets ActivatedThisTurn to false and restores ActionPoints to the unit's base value.
        /// Called on all living units by SimulationState.ResetUnitsForNewTurn().
        /// </summary>
        public void ResetForNewTurn()
        {
            ActivatedThisTurn = false;
            ActionPoints = Type == UnitType.Mancer ? 6 : 1;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Unit[{Id}] Type={Type} Owner={OwnerId} HP={CurrentHP}/{MaxHP} Pos={Position} Alive={IsAlive}";
        }
    }
}

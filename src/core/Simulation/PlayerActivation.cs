using Battlemancers.Core.Simulation.Commands;

namespace Battlemancers.Core.Simulation
{
    /// <summary>
    /// Represents the set of commands a player intends to execute in a single turn.
    ///
    /// A PlayerActivation is constructed during the Planning phase and submitted to
    /// TurnManager.SubmitPlan(). The total ActivationCost of all included commands
    /// must not exceed the 100-point activation budget (warbands rule).
    ///
    /// PlayerActivation is immutable after construction — the activation plan is sealed
    /// when it is submitted. In multiplayer, this object is what gets serialized and
    /// transmitted to the opponent's client after both players lock in.
    ///
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public class PlayerActivation
    {
        /// <summary>The player who submitted this activation plan.</summary>
        public string PlayerId { get; }

        /// <summary>
        /// The ordered set of commands in this activation plan.
        /// Commands will be sorted into initiative order by TurnManager during resolution —
        /// the order here does not determine execution order.
        /// </summary>
        public Command[] Commands { get; }

        /// <summary>
        /// Sum of ActivationCost across all commands in this plan.
        /// Computed once at construction. TurnManager checks this against the 100-pt cap.
        /// </summary>
        public int TotalActivationCost { get; }

        /// <summary>True if TotalActivationCost is within the 100-point activation budget.</summary>
        public bool IsWithinBudget => TotalActivationCost <= 100;

        /// <summary>
        /// Initializes a PlayerActivation for the given player with the specified commands.
        /// The total activation cost is computed eagerly at construction.
        /// </summary>
        /// <param name="playerId">The player who owns this plan.</param>
        /// <param name="commands">
        /// The commands the player wishes to execute this turn. May be empty (valid — the player
        /// passes their turn by activating no units).
        /// </param>
        public PlayerActivation(string playerId, Command[] commands)
        {
            PlayerId = playerId;
            Commands = commands ?? System.Array.Empty<Command>();

            TotalActivationCost = 0;
            foreach (Command cmd in Commands)
            {
                TotalActivationCost += cmd.ActivationCost;
            }
        }
    }
}

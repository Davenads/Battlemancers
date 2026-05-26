using UnityEngine;
using Battlemancers.Core.Simulation;

namespace Battlemancers.UI
{
    /// <summary>
    /// MonoBehaviour that owns the SimulationState and TurnManager for a match session.
    ///
    /// Responsible for constructing and wiring the simulation layer before the first frame.
    /// Other MonoBehaviours (PlayerInputController, HUDManager, etc.) hold a [SerializeField]
    /// reference to this component and access State and TurnManager through it.
    ///
    /// Full implementation is deferred to the simulation-wiring agent. This stub exposes
    /// the properties required by PlayerInputController so the project compiles.
    /// </summary>
    public class SimulationBootstrapper : MonoBehaviour
    {
        /// <summary>The live simulation state for the current match.</summary>
        public SimulationState State { get; private set; }

        /// <summary>The turn manager driving the blind simultaneous turn loop.</summary>
        public TurnManager TurnManager { get; private set; }
    }
}

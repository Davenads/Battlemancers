using System;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Events;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;
using Battlemancers.Core.Simulation.Events;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Presentation
{
    /// <summary>
    /// MonoBehaviour adapter that bridges the pure C# <see cref="SimulationEventBus"/>
    /// to Unity MonoBehaviours.
    ///
    /// Subscribes to SimulationEventBus at Awake and queues incoming events into a
    /// thread-safe queue. In Update(), drains the queue and dispatches events as
    /// typed C# events that GridRenderer, UnitViewController, and other MonoBehaviours
    /// can subscribe to.
    ///
    /// This is the ONLY class that crosses the pure-C# / Unity boundary for simulation events.
    /// Simulation systems publish to SimulationEventBus; Unity MonoBehaviours subscribe here.
    /// </summary>
    public class SimulationEventDispatcher : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // C# events — subscribe from other MonoBehaviours
        // ---------------------------------------------------------------------------

        /// <summary>Fired when a unit moves to a new grid position.</summary>
        public event Action<string, Vector2Int> UnitMoved;

        /// <summary>Fired when a unit's HP reaches 0 and it is removed from simulation.</summary>
        public event Action<string> UnitDied;

        /// <summary>Fired when a tile's elemental state changes.</summary>
        public event Action<Vector2Int, TileState> TileStateChanged;

        /// <summary>Fired when a status effect is applied or refreshed on a unit.</summary>
        public event Action<string, StatusType> StatusApplied;

        /// <summary>Fired when a status effect is removed from a unit.</summary>
        public event Action<string, string> StatusRemoved;

        /// <summary>Fired when a unit takes damage.</summary>
        public event Action<string, int> UnitDamaged;

        /// <summary>Fired when a unit is healed.</summary>
        public event Action<string, int> UnitHealed;

        /// <summary>Fired when a turn finishes resolving.</summary>
        public event Action<int> TurnResolved;

        /// <summary>Fired when the match ends.</summary>
        public event Action<string, MatchEndReason> MatchEnded;

        // ---------------------------------------------------------------------------
        // Private event queue
        // ---------------------------------------------------------------------------

        // ConcurrentQueue supports multi-threaded enqueue (simulation could run off main thread
        // in a future async setup). Dequeue always happens on the main thread in Update().
        private readonly ConcurrentQueue<SimulationEvent> _eventQueue
            = new ConcurrentQueue<SimulationEvent>();

        // Cached delegates for Unsubscribe (must match reference used at Subscribe time).
        private Action<UnitMovedEvent>         _onUnitMoved;
        private Action<UnitDiedEvent>          _onUnitDied;
        private Action<TileStateChangedEvent>  _onTileStateChanged;
        private Action<UnitStatusAppliedEvent> _onStatusApplied;
        private Action<UnitStatusRemovedEvent> _onStatusRemoved;
        private Action<UnitDamagedEvent>       _onUnitDamaged;
        private Action<UnitHealedEvent>        _onUnitHealed;
        private Action<TurnResolvedEvent>      _onTurnResolved;
        private Action<MatchEndedEvent>        _onMatchEnded;

        // ---------------------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------------------

        private void Awake()
        {
            _onUnitMoved        = e => _eventQueue.Enqueue(e);
            _onUnitDied         = e => _eventQueue.Enqueue(e);
            _onTileStateChanged = e => _eventQueue.Enqueue(e);
            _onStatusApplied    = e => _eventQueue.Enqueue(e);
            _onStatusRemoved    = e => _eventQueue.Enqueue(e);
            _onUnitDamaged      = e => _eventQueue.Enqueue(e);
            _onUnitHealed       = e => _eventQueue.Enqueue(e);
            _onTurnResolved     = e => _eventQueue.Enqueue(e);
            _onMatchEnded       = e => _eventQueue.Enqueue(e);

            SimulationEventBus.Subscribe(_onUnitMoved);
            SimulationEventBus.Subscribe(_onUnitDied);
            SimulationEventBus.Subscribe(_onTileStateChanged);
            SimulationEventBus.Subscribe(_onStatusApplied);
            SimulationEventBus.Subscribe(_onStatusRemoved);
            SimulationEventBus.Subscribe(_onUnitDamaged);
            SimulationEventBus.Subscribe(_onUnitHealed);
            SimulationEventBus.Subscribe(_onTurnResolved);
            SimulationEventBus.Subscribe(_onMatchEnded);
        }

        private void OnDestroy()
        {
            SimulationEventBus.Unsubscribe(_onUnitMoved);
            SimulationEventBus.Unsubscribe(_onUnitDied);
            SimulationEventBus.Unsubscribe(_onTileStateChanged);
            SimulationEventBus.Unsubscribe(_onStatusApplied);
            SimulationEventBus.Unsubscribe(_onStatusRemoved);
            SimulationEventBus.Unsubscribe(_onUnitDamaged);
            SimulationEventBus.Unsubscribe(_onUnitHealed);
            SimulationEventBus.Unsubscribe(_onTurnResolved);
            SimulationEventBus.Unsubscribe(_onMatchEnded);
        }

        // ---------------------------------------------------------------------------
        // Unity Update — drain the event queue
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Drains all queued simulation events and dispatches them to Unity-side subscribers.
        /// Called once per frame on the main thread.
        /// </summary>
        private void Update()
        {
            while (_eventQueue.TryDequeue(out SimulationEvent simEvent))
            {
                DispatchToUnity(simEvent);
            }
        }

        // ---------------------------------------------------------------------------
        // Dispatch
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Routes a dequeued SimulationEvent to the appropriate typed C# event.
        /// Only event types that have Unity-side subscribers are dispatched.
        /// </summary>
        private void DispatchToUnity(SimulationEvent simEvent)
        {
            switch (simEvent)
            {
                case UnitMovedEvent moved:
                    UnitMoved?.Invoke(moved.UnitId, new Vector2Int(moved.To.X, moved.To.Y));
                    break;

                case UnitDiedEvent died:
                    UnitDied?.Invoke(died.UnitId);
                    break;

                case TileStateChangedEvent tileChanged:
                    TileStateChanged?.Invoke(
                        new Vector2Int(tileChanged.Position.X, tileChanged.Position.Y),
                        tileChanged.NewState);
                    break;

                case UnitStatusAppliedEvent statusApplied:
                    if (Enum.TryParse(statusApplied.StatusType, out StatusType parsedStatus))
                        StatusApplied?.Invoke(statusApplied.UnitId, parsedStatus);
                    break;

                case UnitStatusRemovedEvent statusRemoved:
                    StatusRemoved?.Invoke(statusRemoved.UnitId, statusRemoved.StatusType);
                    break;

                case UnitDamagedEvent damaged:
                    UnitDamaged?.Invoke(damaged.UnitId, damaged.DamageAmount);
                    break;

                case UnitHealedEvent healed:
                    UnitHealed?.Invoke(healed.UnitId, healed.HealAmount);
                    break;

                case TurnResolvedEvent turnResolved:
                    TurnResolved?.Invoke(turnResolved.TurnNumber);
                    break;

                case MatchEndedEvent matchEnded:
                    MatchEnded?.Invoke(matchEnded.WinnerId, matchEnded.Reason);
                    break;
            }
        }
    }
}

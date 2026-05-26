using System;
using System.Collections.Generic;

namespace Battlemancers.Core.Simulation.Events
{
    /// <summary>
    /// Static type-safe pub/sub event bus for simulation events.
    ///
    /// The simulation layer publishes events here after mutating state; the Unity
    /// presentation layer subscribes to drive animation, VFX, audio, and UI.
    ///
    /// Thread-safety: NOT thread-safe by design. The simulation runs entirely on
    /// the main thread. Do not publish or subscribe from background threads.
    ///
    /// Lifetime: Call Clear() when tearing down a match (scene unload, test teardown)
    /// to prevent stale handlers from leaking across sessions.
    /// </summary>
    public static class SimulationEventBus
    {
        // Maps event Type → list of raw Delegate (stored as Action<T> at subscribe time).
        private static readonly Dictionary<Type, List<Delegate>> _handlers
            = new Dictionary<Type, List<Delegate>>();

        // ---------------------------------------------------------------------------
        // Subscribe / Unsubscribe
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Registers <paramref name="handler"/> to be invoked whenever an event of type
        /// <typeparamref name="T"/> is published.
        /// Subscribing the same handler instance twice will cause it to be invoked twice per publish.
        /// </summary>
        /// <typeparam name="T">The concrete SimulationEvent subtype to listen for.</typeparam>
        /// <param name="handler">The callback to invoke on each publish.</param>
        public static void Subscribe<T>(Action<T> handler) where T : SimulationEvent
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Type key = typeof(T);
            if (!_handlers.TryGetValue(key, out List<Delegate> list))
            {
                list = new List<Delegate>();
                _handlers[key] = list;
            }

            list.Add(handler);
        }

        /// <summary>
        /// Removes the first occurrence of <paramref name="handler"/> from the subscriber list
        /// for event type <typeparamref name="T"/>. If the handler is not currently subscribed,
        /// this is a no-op.
        /// </summary>
        /// <typeparam name="T">The concrete SimulationEvent subtype to stop listening for.</typeparam>
        /// <param name="handler">The callback to remove.</param>
        public static void Unsubscribe<T>(Action<T> handler) where T : SimulationEvent
        {
            if (handler == null) return;

            Type key = typeof(T);
            if (_handlers.TryGetValue(key, out List<Delegate> list))
            {
                list.Remove(handler);

                // Clean up the dictionary entry if no handlers remain.
                if (list.Count == 0)
                    _handlers.Remove(key);
            }
        }

        // ---------------------------------------------------------------------------
        // Publish
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Publishes <paramref name="simEvent"/> to all subscribers registered for
        /// <typeparamref name="T"/>. Subscribers are invoked synchronously in registration order.
        ///
        /// The handler list is copied before iteration so that handlers may safely
        /// subscribe or unsubscribe during the publish without causing enumeration errors.
        /// </summary>
        /// <typeparam name="T">The concrete SimulationEvent subtype being published.</typeparam>
        /// <param name="simEvent">The event instance to deliver to all subscribers.</param>
        public static void Publish<T>(T simEvent) where T : SimulationEvent
        {
            if (simEvent == null) throw new ArgumentNullException(nameof(simEvent));

            Type key = typeof(T);
            if (!_handlers.TryGetValue(key, out List<Delegate> list) || list.Count == 0)
                return;

            // Copy the list before iterating — a handler may call Unsubscribe during dispatch.
            Delegate[] snapshot = list.ToArray();
            foreach (Delegate d in snapshot)
            {
                ((Action<T>)d)(simEvent);
            }
        }

        // ---------------------------------------------------------------------------
        // Cleanup
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Removes ALL subscriptions for ALL event types. Call this on match teardown
        /// and in test fixture teardown to prevent cross-test handler pollution.
        /// </summary>
        public static void Clear()
        {
            _handlers.Clear();
        }

        /// <summary>
        /// Removes all subscriptions for a specific event type only.
        /// Useful when a subsystem is being disabled without tearing down the full match.
        /// </summary>
        /// <typeparam name="T">The event type whose handlers should be cleared.</typeparam>
        public static void ClearForType<T>() where T : SimulationEvent
        {
            _handlers.Remove(typeof(T));
        }

        /// <summary>
        /// Returns the current number of subscribers registered for event type
        /// <typeparamref name="T"/>. Primarily useful in unit tests.
        /// </summary>
        public static int SubscriberCount<T>() where T : SimulationEvent
        {
            Type key = typeof(T);
            return _handlers.TryGetValue(key, out List<Delegate> list) ? list.Count : 0;
        }
    }
}

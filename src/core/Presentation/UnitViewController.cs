using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battlemancers.Core.Simulation;
using Battlemancers.Simulation.Status;

namespace Battlemancers.Presentation
{
    /// <summary>
    /// MonoBehaviour that maps UnitState positions to GameObject positions and drives
    /// per-unit visual state (tint, animation, death effects).
    ///
    /// Maintains a dictionary keyed by unit ID. Responds to calls from
    /// BattleSceneController after each SimulationEvent is processed.
    ///
    /// Does NOT contain any game logic. Does NOT call TurnManager.
    /// Grid-to-world formula: new Vector3(gridPos.X, 0, gridPos.Y) — XZ plane.
    /// </summary>
    public class UnitViewController : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        /// <summary>Seconds for the unit move lerp animation.</summary>
        private const float MoveLerpDuration = 0.3f;

        /// <summary>Seconds for the death scale-to-zero animation.</summary>
        private const float DeathAnimDuration = 0.25f;

        // Status tint colors.
        private static readonly Color TintBurning  = new Color(1.0f, 0.4f, 0.0f);  // orange
        private static readonly Color TintFrozen   = new Color(0.5f, 0.85f, 1.0f); // light blue
        private static readonly Color TintPoisoned = new Color(0.3f, 0.9f, 0.2f);  // green
        private static readonly Color TintStunned  = new Color(1.0f, 1.0f, 0.0f);  // yellow
        private static readonly Color TintCharmed  = new Color(1.0f, 0.5f, 0.8f);  // pink
        private static readonly Color TintDefault  = Color.white;

        // ---------------------------------------------------------------------------
        // Inspector fields
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Default unit prefab used when no archetype-specific prefab is available.
        /// Assigned in the Inspector.
        /// </summary>
        [SerializeField] private GameObject _defaultUnitPrefab;

        // ---------------------------------------------------------------------------
        // Runtime state
        // ---------------------------------------------------------------------------

        // Maps unit runtime ID → the GameObject representing that unit in the scene.
        private readonly Dictionary<string, GameObject> _unitObjects
            = new Dictionary<string, GameObject>();

        // Active move coroutines keyed by unit ID (allows cancelling mid-move if needed).
        private readonly Dictionary<string, Coroutine> _moveCoroutines
            = new Dictionary<string, Coroutine>();

        // ---------------------------------------------------------------------------
        // Public API — called by BattleSceneController
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Instantiates a unit's visual representation at its current grid position.
        /// If <paramref name="prefab"/> is null, falls back to <see cref="_defaultUnitPrefab"/>.
        /// </summary>
        /// <param name="unit">The UnitState providing position and identity data.</param>
        /// <param name="prefab">
        /// The prefab to instantiate for this unit. Pass null to use the default.
        /// </param>
        public void SpawnUnit(UnitState unit, GameObject prefab)
        {
            if (unit == null)
            {
                Debug.LogError("[UnitViewController] SpawnUnit called with null UnitState.");
                return;
            }

            GameObject sourcePrefab = prefab != null ? prefab : _defaultUnitPrefab;
            if (sourcePrefab == null)
            {
                // Create a minimal visible stand-in if no prefab is assigned.
                sourcePrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Destroy(sourcePrefab.GetComponent<Collider>());
            }

            Vector3 worldPos = GridToWorld(new Vector2Int(unit.Position.X, unit.Position.Y));
            GameObject go = Instantiate(sourcePrefab, worldPos, Quaternion.identity, transform);
            go.name = $"Unit_{unit.Id}";

            if (_unitObjects.ContainsKey(unit.Id))
            {
                Debug.LogWarning($"[UnitViewController] Unit '{unit.Id}' is already spawned. Replacing.");
                Destroy(_unitObjects[unit.Id]);
            }

            _unitObjects[unit.Id] = go;
        }

        /// <summary>
        /// Moves the unit's GameObject to the new grid position using a smooth lerp over
        /// <see cref="MoveLerpDuration"/> seconds. Any in-progress move for this unit is
        /// cancelled and replaced by the new target.
        /// </summary>
        /// <param name="unitId">Runtime ID of the unit to move.</param>
        /// <param name="newPos">The destination grid position.</param>
        public void MoveUnit(string unitId, Vector2Int newPos)
        {
            if (!_unitObjects.TryGetValue(unitId, out GameObject go) || go == null)
                return;

            // Cancel any currently running move coroutine for this unit.
            if (_moveCoroutines.TryGetValue(unitId, out Coroutine existing) && existing != null)
                StopCoroutine(existing);

            Vector3 target = GridToWorld(newPos);
            Coroutine co = StartCoroutine(LerpMoveCoroutine(go, target, MoveLerpDuration));
            _moveCoroutines[unitId] = co;
        }

        /// <summary>
        /// Plays a death animation (scale to zero over <see cref="DeathAnimDuration"/> seconds)
        /// then destroys the unit's GameObject and removes it from the registry.
        /// </summary>
        /// <param name="unitId">Runtime ID of the unit that died.</param>
        public void RemoveUnit(string unitId)
        {
            if (!_unitObjects.TryGetValue(unitId, out GameObject go) || go == null)
            {
                _unitObjects.Remove(unitId);
                return;
            }

            // Cancel any pending move for a unit that just died.
            if (_moveCoroutines.TryGetValue(unitId, out Coroutine existing) && existing != null)
                StopCoroutine(existing);
            _moveCoroutines.Remove(unitId);

            _unitObjects.Remove(unitId);
            StartCoroutine(DeathAnimCoroutine(go, DeathAnimDuration));
        }

        /// <summary>
        /// Tints the unit's primary sprite renderer to reflect its current status effect.
        /// Only the most visually significant status is applied as a tint (priority order:
        /// FROZEN > BURNING > POISONED > STUNNED > CHARMED > default white).
        /// </summary>
        /// <param name="unitId">Runtime ID of the unit.</param>
        /// <param name="status">The status type to visualize.</param>
        public void ApplyStatusVisual(string unitId, StatusType status)
        {
            if (!_unitObjects.TryGetValue(unitId, out GameObject go) || go == null)
                return;

            Color tint = StatusToTint(status);
            ApplyTintToUnit(go, tint);
        }

        /// <summary>
        /// Clears any status tint on the unit, resetting to the default white color.
        /// Call this when a status effect expires.
        /// </summary>
        /// <param name="unitId">Runtime ID of the unit.</param>
        public void ClearStatusVisual(string unitId)
        {
            if (!_unitObjects.TryGetValue(unitId, out GameObject go) || go == null)
                return;

            ApplyTintToUnit(go, TintDefault);
        }

        // ---------------------------------------------------------------------------
        // Coordinate conversion (public for other Presentation classes)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Converts a grid position to a Unity world position.
        /// Each tile is 1 Unity unit; origin at (0, 0, 0).
        /// Formula: new Vector3(gridPos.x, 0, gridPos.y) — XZ plane for isometric readiness.
        /// </summary>
        /// <param name="gridPos">The grid position to convert.</param>
        /// <returns>The corresponding world space position.</returns>
        public static Vector3 GridToWorld(Vector2Int gridPos)
        {
            return new Vector3(gridPos.x, 0f, gridPos.y);
        }

        // ---------------------------------------------------------------------------
        // Coroutines
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Lerps the position of a GameObject from its current world position to
        /// <paramref name="target"/> over <paramref name="duration"/> seconds.
        /// </summary>
        private static IEnumerator LerpMoveCoroutine(GameObject go, Vector3 target, float duration)
        {
            if (go == null) yield break;

            Vector3 start = go.transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (go == null) yield break;
                go.transform.position = Vector3.Lerp(start, target, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (go != null)
                go.transform.position = target;
        }

        /// <summary>
        /// Scales a GameObject uniformly from its current scale to zero over
        /// <paramref name="duration"/> seconds, then destroys it.
        /// </summary>
        private static IEnumerator DeathAnimCoroutine(GameObject go, float duration)
        {
            if (go == null) yield break;

            Vector3 startScale = go.transform.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (go == null) yield break;
                go.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (go != null)
                Destroy(go);
        }

        // ---------------------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------------------

        /// <summary>Maps a StatusType to the corresponding tint color.</summary>
        private static Color StatusToTint(StatusType status)
        {
            switch (status)
            {
                case StatusType.Burning:  return TintBurning;
                case StatusType.Frozen:   return TintFrozen;
                case StatusType.Poisoned: return TintPoisoned;
                case StatusType.Stunned:  return TintStunned;
                case StatusType.Charmed:  return TintCharmed;
                default:                  return TintDefault;
            }
        }

        /// <summary>
        /// Applies a color tint to all SpriteRenderers and Renderers found on a unit GameObject
        /// (including children). Uses MaterialPropertyBlock to avoid material instance bloat.
        /// </summary>
        private static void ApplyTintToUnit(GameObject go, Color tint)
        {
            // Prefer SpriteRenderer for 2D sprites.
            SpriteRenderer[] spriteRenderers = go.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in spriteRenderers)
                sr.color = tint;

            // Also tint Renderer components (3D meshes).
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            var block = new MaterialPropertyBlock();
            foreach (Renderer rend in renderers)
            {
                rend.GetPropertyBlock(block);
                block.SetColor("_BaseColor", tint);
                block.SetColor("_Color", tint);
                rend.SetPropertyBlock(block);
            }
        }
    }
}

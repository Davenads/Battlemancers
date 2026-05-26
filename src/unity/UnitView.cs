using System.Collections;
using UnityEngine;
using Battlemancers.Core.Grid;
using Battlemancers.Core.Simulation;

namespace Battlemancers.Unity
{
    /// <summary>
    /// Visual representation of a single Mancer unit on the battlefield.
    /// One instance exists per unit. Receives state updates via UnitViewManager
    /// in response to SimulationEventBus events — never polls SimulationState in Update().
    ///
    /// Temperature tint is applied as a piecewise linear interpolation across five
    /// thermal bands. HP bar scale is driven by the HP ratio. A brief white flash
    /// plays on damage hit; a fade-out coroutine plays on death.
    /// </summary>
    public class UnitView : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Temperature tint constants
        // ---------------------------------------------------------------------------

        private static readonly Color TintNeutral     = Color.white;
        private static readonly Color TintHot         = new Color(1.0f, 0.7f, 0.5f); // warm orange
        private static readonly Color TintOverheated  = new Color(1.0f, 0.3f, 0.1f); // deep red-orange
        private static readonly Color TintSupercooled = new Color(0.6f, 0.8f, 1.0f); // light blue
        private static readonly Color TintFrozenSolid = new Color(0.2f, 0.5f, 0.9f); // ice blue

        // Temperature threshold values matching TemperatureManager.GetCategory()
        private const int ThresholdHot         =  31;
        private const int ThresholdOverheated  =  61;
        private const int ThresholdSupercooled = -31;
        private const int ThresholdFrozenSolid = -61;

        // Duration of the damage flash white overlay in seconds.
        private const float DamageFlashDuration = 0.12f;

        // Duration of the death fade-out in seconds.
        private const float DeathFadeDuration = 0.5f;

        // ---------------------------------------------------------------------------
        // Serialized fields
        // ---------------------------------------------------------------------------

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private GameObject _hpBarRoot;
        [SerializeField] private RectTransform _hpBarFill;
        [SerializeField] private GameObject _selectedIndicator;

        // ---------------------------------------------------------------------------
        // Public identity
        // ---------------------------------------------------------------------------

        /// <summary>Runtime ID of the unit this view represents.</summary>
        public string UnitId { get; private set; }

        /// <summary>Owner player ID of the unit this view represents.</summary>
        public string OwnerId { get; private set; }

        // ---------------------------------------------------------------------------
        // Internal state
        // ---------------------------------------------------------------------------

        // Current temperature, kept so the damage flash coroutine can restore the correct tint.
        private int _currentTemperature;

        // True while a damage flash coroutine is running — prevents tint restore from racing.
        private bool _flashInProgress;

        // ---------------------------------------------------------------------------
        // Initialization
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Binds this view to a simulation unit. Sets sprite, world position, and initial HP bar.
        /// Must be called immediately after Instantiate, before any event handlers fire.
        /// </summary>
        /// <param name="unitState">The simulation state of the unit to represent.</param>
        /// <param name="sprite">Sprite to display for this unit.</param>
        public void Initialize(UnitState unitState, Sprite sprite)
        {
            UnitId = unitState.Id;
            OwnerId = unitState.OwnerId;
            _currentTemperature = unitState.Temperature;

            if (_spriteRenderer != null)
                _spriteRenderer.sprite = sprite;

            UpdateHpBar(unitState.CurrentHP, unitState.MaxHP);
            UpdateTemperatureTint(unitState.Temperature);

            if (_selectedIndicator != null)
                _selectedIndicator.SetActive(false);
        }

        // ---------------------------------------------------------------------------
        // State update (called by UnitViewManager on relevant events)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Refreshes all visuals from the provided unit state snapshot.
        /// Called whenever UnitViewManager receives an event that changes unit appearance.
        /// </summary>
        /// <param name="state">Current snapshot of this unit's simulation state.</param>
        public void UpdateFromState(UnitState state)
        {
            UpdateHpBar(state.CurrentHP, state.MaxHP);
            UpdateTemperatureTint(state.Temperature);
        }

        // ---------------------------------------------------------------------------
        // Selection indicator
        // ---------------------------------------------------------------------------

        /// <summary>Shows or hides the selection ring/glow indicator beneath the unit.</summary>
        /// <param name="selected">True to show the indicator; false to hide it.</param>
        public void SetSelected(bool selected)
        {
            if (_selectedIndicator != null)
                _selectedIndicator.SetActive(selected);
        }

        // ---------------------------------------------------------------------------
        // Damage flash
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Plays a brief white flash on the sprite to indicate a damage hit.
        /// If a flash is already in progress it is allowed to complete naturally;
        /// a new call starts a fresh coroutine so overlapping hits each produce a flash.
        /// </summary>
        public void PlayDamageFlash()
        {
            StartCoroutine(DamageFlashRoutine());
        }

        private IEnumerator DamageFlashRoutine()
        {
            _flashInProgress = true;
            if (_spriteRenderer != null)
                _spriteRenderer.color = Color.white;

            yield return new WaitForSeconds(DamageFlashDuration);

            _flashInProgress = false;
            // Restore the correct temperature tint after the flash.
            if (_spriteRenderer != null)
                _spriteRenderer.color = ComputeTemperatureTint(_currentTemperature);
        }

        // ---------------------------------------------------------------------------
        // Death animation
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Fades the sprite alpha to zero over <see cref="DeathFadeDuration"/> seconds,
        /// then destroys this GameObject. Disables the HP bar and selection indicator immediately.
        /// </summary>
        public void PlayDeathAnimation()
        {
            if (_hpBarRoot != null)
                _hpBarRoot.SetActive(false);
            if (_selectedIndicator != null)
                _selectedIndicator.SetActive(false);

            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            float elapsed = 0f;
            Color startColor = _spriteRenderer != null ? _spriteRenderer.color : Color.white;

            while (elapsed < DeathFadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / DeathFadeDuration);
                if (_spriteRenderer != null)
                    _spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            Destroy(gameObject);
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        private void UpdateHpBar(int current, int max)
        {
            if (_hpBarFill == null) return;
            float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            _hpBarFill.localScale = new Vector3(ratio, 1f, 1f);
        }

        private void UpdateTemperatureTint(int temperature)
        {
            _currentTemperature = temperature;
            // Do not overwrite the flash white while a flash coroutine is in progress.
            if (_flashInProgress) return;
            if (_spriteRenderer != null)
                _spriteRenderer.color = ComputeTemperatureTint(temperature);
        }

        /// <summary>
        /// Computes the sprite tint color for the given temperature value using piecewise
        /// linear interpolation across five thermal bands.
        ///
        /// Band mapping:
        ///   temperature ≤ -61  → TintFrozenSolid
        ///   -61 to -31         → lerp(TintFrozenSolid, TintSupercooled)
        ///   -31 to 0           → lerp(TintSupercooled, TintNeutral)
        ///    0  to +31         → lerp(TintNeutral, TintHot)
        ///   +31 to +61         → lerp(TintHot, TintOverheated)
        ///   temperature ≥ +61  → TintOverheated
        /// </summary>
        /// <param name="temperature">Unit temperature in [-100, +100].</param>
        /// <returns>The interpolated tint color for the sprite renderer.</returns>
        private static Color ComputeTemperatureTint(int temperature)
        {
            if (temperature >= ThresholdOverheated)
                return TintOverheated;

            if (temperature >= ThresholdHot)
            {
                // +31 to +60 → lerp Hot → Overheated
                float t = (float)(temperature - ThresholdHot) / (ThresholdOverheated - ThresholdHot);
                return Color.Lerp(TintHot, TintOverheated, t);
            }

            if (temperature >= 0)
            {
                // 0 to +30 → lerp Neutral → Hot
                float t = (float)temperature / ThresholdHot;
                return Color.Lerp(TintNeutral, TintHot, t);
            }

            if (temperature >= ThresholdSupercooled)
            {
                // -30 to -1 → lerp Supercooled → Neutral  (negative range, t approaches 0 as temp → 0)
                float t = (float)(-temperature) / (-ThresholdSupercooled);
                return Color.Lerp(TintNeutral, TintSupercooled, t);
            }

            if (temperature >= ThresholdFrozenSolid)
            {
                // -60 to -31 → lerp FrozenSolid → Supercooled
                float t = (float)(temperature - ThresholdSupercooled) / (ThresholdFrozenSolid - ThresholdSupercooled);
                return Color.Lerp(TintSupercooled, TintFrozenSolid, t);
            }

            // temperature ≤ -61
            return TintFrozenSolid;
        }
    }
}

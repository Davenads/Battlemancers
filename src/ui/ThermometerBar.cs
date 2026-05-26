using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Battlemancers.UI
{
    /// <summary>
    /// Renders a unit's current temperature as a colored fill bar with a category label
    /// and numeric readout. Self-contained — callers only invoke SetTemperature(int).
    ///
    /// Temperature bands and thresholds match TemperatureManager.GetCategory() exactly:
    ///   FROZEN SOLID  : temperature &lt;= -61
    ///   SUPERCOOLED   : temperature -31 to -60
    ///   NEUTRAL       : -30 to +30 (no status)
    ///   HOT           : temperature +31 to +60
    ///   OVERHEATED    : temperature >= +61
    ///
    /// Unity only — do not use in pure-C# simulation code.
    /// </summary>
    public class ThermometerBar : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Temperature band colors — named constants, never inline
        // ---------------------------------------------------------------------------

        private static readonly Color ColorFrozenSolid = new Color(0.15f, 0.35f, 0.95f); // deep blue
        private static readonly Color ColorSupercooled  = new Color(0.5f,  0.75f, 1.0f);  // light blue
        private static readonly Color ColorNeutral      = new Color(0.6f,  0.6f,  0.6f);  // grey
        private static readonly Color ColorHot          = new Color(1.0f,  0.65f, 0.2f);  // orange
        private static readonly Color ColorOverheated   = new Color(1.0f,  0.2f,  0.05f); // red

        // ---------------------------------------------------------------------------
        // Temperature thresholds — must match TemperatureManager.GetCategory() exactly
        // ---------------------------------------------------------------------------

        private const int ThresholdHot         =  31;
        private const int ThresholdOverheated  =  61;
        private const int ThresholdSupercooled = -31;
        private const int ThresholdFrozenSolid = -61;
        private const int TemperatureMin       = -100;
        private const int TemperatureMax       =  100;

        // ---------------------------------------------------------------------------
        // Inspector references
        // ---------------------------------------------------------------------------

        [SerializeField] private Image         _fillImage;             // colored fill
        [SerializeField] private RectTransform _fillRect;              // controls fill width via localScale.x
        [SerializeField] private TMP_Text      _categoryLabel;         // "NEUTRAL", "HOT", "OVERHEATED", etc.
        [SerializeField] private TMP_Text      _valueLabel;            // "+45" or "-23"
        [SerializeField] private Image         _thresholdMarkerHot;    // thin line at +31
        [SerializeField] private Image         _thresholdMarkerOver;   // thin line at +61
        [SerializeField] private Image         _thresholdMarkerCool;   // thin line at -31
        [SerializeField] private Image         _thresholdMarkerFreeze; // thin line at -61

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Updates all visual elements to reflect <paramref name="temperature"/>.
        /// Called by HUDManager whenever the displayed unit's temperature changes.
        /// </summary>
        /// <param name="temperature">Current temperature in the range [-100, +100].</param>
        public void SetTemperature(int temperature)
        {
            // Fill amount: 0.0 = min(-100), 0.5 = neutral(0), 1.0 = max(+100).
            float fillAmount = (temperature - TemperatureMin) / (float)(TemperatureMax - TemperatureMin);
            _fillRect.localScale = new Vector3(fillAmount, 1f, 1f);

            _fillImage.color = GetTemperatureColor(temperature);

            _categoryLabel.text = GetCategoryLabel(temperature);
            _valueLabel.text = temperature > 0 ? $"+{temperature}" : temperature.ToString();
        }

        // ---------------------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------------------

        private static Color GetTemperatureColor(int temperature)
        {
            if (temperature <= ThresholdFrozenSolid)
                return ColorFrozenSolid;

            if (temperature <= ThresholdSupercooled)
                return Color.Lerp(
                    ColorFrozenSolid,
                    ColorSupercooled,
                    (temperature - ThresholdFrozenSolid) / (float)(ThresholdSupercooled - ThresholdFrozenSolid));

            if (temperature < 0)
                return Color.Lerp(
                    ColorSupercooled,
                    ColorNeutral,
                    (temperature - ThresholdSupercooled) / (float)(0 - ThresholdSupercooled));

            if (temperature == 0)
                return ColorNeutral;

            if (temperature < ThresholdHot)
                return Color.Lerp(
                    ColorNeutral,
                    ColorHot,
                    temperature / (float)ThresholdHot);

            if (temperature < ThresholdOverheated)
                return Color.Lerp(
                    ColorHot,
                    ColorOverheated,
                    (temperature - ThresholdHot) / (float)(ThresholdOverheated - ThresholdHot));

            return ColorOverheated;
        }

        private static string GetCategoryLabel(int temperature)
        {
            if (temperature >= ThresholdOverheated)  return "OVERHEATED";
            if (temperature >= ThresholdHot)         return "HOT";
            if (temperature <= ThresholdFrozenSolid) return "FROZEN SOLID";
            if (temperature <= ThresholdSupercooled) return "SUPERCOOLED";
            return "NEUTRAL";
        }
    }
}

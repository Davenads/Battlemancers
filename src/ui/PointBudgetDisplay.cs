using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Battlemancers.UI
{
    /// <summary>
    /// Real-time warband budget tracker. Receives the current point spend via
    /// <see cref="SetCost"/> and updates all display elements accordingly.
    ///
    /// Pure display logic — no state of its own. Driven entirely by
    /// <see cref="WarbandBuilderManager"/> after every mutation.
    ///
    /// Unity only — do not reference from pure-C# simulation code.
    /// </summary>
    public class PointBudgetDisplay : MonoBehaviour
    {
        // ---------------------------------------------------------------------------
        // Constants
        // ---------------------------------------------------------------------------

        private const int MaxBudget = 1000;
        private const int WarningThreshold = 900;

        private static readonly Color ColorHealthy    = new Color(0.3f,  0.85f, 0.3f);  // green  ≤ 900
        private static readonly Color ColorWarning    = new Color(0.95f, 0.8f,  0.1f);  // yellow 901-1000
        private static readonly Color ColorOverBudget = new Color(0.95f, 0.2f,  0.1f);  // red    > 1000

        // ---------------------------------------------------------------------------
        // Inspector references
        // ---------------------------------------------------------------------------

        [SerializeField] private TMP_Text _totalLabel;     // "750 / 1000 pts"
        [SerializeField] private TMP_Text _remainingLabel; // "+ 250 pts remaining"
        [SerializeField] private Image    _fillBar;        // horizontal fill image

        // ---------------------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Updates all budget display elements to reflect <paramref name="currentCost"/>.
        /// Call this after every warband mutation.
        /// </summary>
        /// <param name="currentCost">The total point cost of the warband as currently configured.</param>
        public void SetCost(int currentCost)
        {
            Color color = currentCost <= WarningThreshold ? ColorHealthy
                        : currentCost <= MaxBudget        ? ColorWarning
                        : ColorOverBudget;

            if (_totalLabel != null)
            {
                _totalLabel.text  = $"{currentCost} / {MaxBudget} pts";
                _totalLabel.color = color;
            }

            if (_remainingLabel != null)
            {
                int remaining = MaxBudget - currentCost;
                _remainingLabel.text = remaining >= 0
                    ? $"+ {remaining} pts remaining"
                    : $"OVER BUDGET by {-remaining} pts";
                _remainingLabel.color = color;
            }

            if (_fillBar != null)
            {
                _fillBar.fillAmount = Mathf.Clamp01((float)currentCost / MaxBudget);
                _fillBar.color      = color;
            }
        }
    }
}

using System;

namespace Battlemancers.Core.Warband
{
    /// <summary>
    /// Immutable result of a warband or activation plan validation pass.
    ///
    /// Consumers should check <see cref="IsValid"/> first, then read <see cref="Errors"/> for
    /// blocking failures and <see cref="Warnings"/> for non-fatal advisories.
    ///
    /// Create instances only via the static factory methods <see cref="Success"/> and
    /// <see cref="Failure"/> — the constructor is private.
    ///
    /// Zero Unity dependencies. Safe to use in headless simulation and unit tests.
    /// </summary>
    public sealed class ValidationResult
    {
        // -----------------------------------------------------------------------------------------
        // Properties
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// True if the warband or plan passed all hard validation rules.
        /// False if one or more blocking errors were found; see <see cref="Errors"/> for details.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Human-readable blocking error messages. Empty array when <see cref="IsValid"/> is true.
        /// Each string describes one rule violation (e.g., "Warband exceeds the 1,000-point cap by 50 pts").
        /// </summary>
        public string[] Errors { get; }

        /// <summary>
        /// Non-fatal advisory messages. Present regardless of <see cref="IsValid"/>.
        /// Examples: "Warband is underbudget by more than 100 pts — consider adding units."
        /// Warnings do not prevent a warband from being submitted for a match.
        /// </summary>
        public string[] Warnings { get; }

        /// <summary>
        /// Total point cost computed during validation.
        /// Reflects the sum of all Mancer base costs, upgrade costs, and support unit costs.
        /// May exceed 1,000 if the warband is invalid.
        /// </summary>
        public int TotalPointCost { get; }

        // -----------------------------------------------------------------------------------------
        // Constructor (private — use factory methods)
        // -----------------------------------------------------------------------------------------

        private ValidationResult(bool isValid, string[] errors, string[] warnings, int totalPointCost)
        {
            IsValid = isValid;
            Errors = errors ?? Array.Empty<string>();
            Warnings = warnings ?? Array.Empty<string>();
            TotalPointCost = totalPointCost;
        }

        // -----------------------------------------------------------------------------------------
        // Static Factory Methods
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Creates a successful <see cref="ValidationResult"/> with no errors.
        /// </summary>
        /// <param name="totalCost">The computed total point cost of the validated warband or plan.</param>
        /// <param name="warnings">
        /// Optional non-fatal advisory messages. Pass null or omit for no warnings.
        /// </param>
        /// <returns>A <see cref="ValidationResult"/> where <see cref="IsValid"/> is true.</returns>
        public static ValidationResult Success(int totalCost, string[] warnings = null)
        {
            return new ValidationResult(true, Array.Empty<string>(), warnings ?? Array.Empty<string>(), totalCost);
        }

        /// <summary>
        /// Creates a failed <see cref="ValidationResult"/> carrying one or more blocking errors.
        /// </summary>
        /// <param name="errors">
        /// One or more human-readable error strings describing rule violations. Must not be null or empty.
        /// </param>
        /// <param name="warnings">
        /// Optional non-fatal advisory messages. Pass null for no warnings.
        /// </param>
        /// <param name="totalCost">
        /// The computed total point cost even for invalid warbands (useful for debugging).
        /// </param>
        /// <returns>A <see cref="ValidationResult"/> where <see cref="IsValid"/> is false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="errors"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="errors"/> is empty.</exception>
        public static ValidationResult Failure(string[] errors, string[] warnings, int totalCost)
        {
            if (errors == null) throw new ArgumentNullException(nameof(errors));
            if (errors.Length == 0) throw new ArgumentException("Failure result must contain at least one error.", nameof(errors));

            return new ValidationResult(false, errors, warnings ?? Array.Empty<string>(), totalCost);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (IsValid)
                return $"ValidationResult[Valid, {TotalPointCost} pts, {Warnings.Length} warning(s)]";

            return $"ValidationResult[Invalid, {TotalPointCost} pts, {Errors.Length} error(s), {Warnings.Length} warning(s)]";
        }
    }
}

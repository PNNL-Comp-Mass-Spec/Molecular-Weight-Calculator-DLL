using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    public interface IFormulaParseData
    {
        /// <summary>
        /// The original or user-supplied formula
        /// </summary>
        string FormulaOriginal { get; }

        /// <summary>
        /// The updated formula (updated after parsing formula components)
        /// </summary>
        string Formula { get; }

        /// <summary>
        /// Computed formula mass
        /// </summary>
        double Mass { get; }

        /// <summary>
        /// Computed standard deviation of the formula mass
        /// </summary>
        double StandardDeviation { get; }

        /// <summary>
        /// Computed charge
        /// </summary>
        float Charge { get; }

        /// <summary>
        /// Computation stats for the formula
        /// </summary>
        ComputationStats Stats { get; }

        /// <summary>
        /// The list of caution notes for the formula, as a comma+space-separated string.
        /// </summary>
        string CautionDescription { get; }

        /// <summary>
        /// Error data object for formula parsing
        /// </summary>
        IErrorData ErrorData { get; }
    }

    [ComVisible(false)]
    public interface IErrorData
    {
        /// <summary>
        /// Contains the error number (used in the LookupMessage method)
        /// </summary>
        /// <remarks>
        /// If a program error occurs, ErrorID is set to -10
        /// </remarks>
        int ErrorId { get; }

        /// <summary>
        /// Error position in a formula
        /// </summary>
        int ErrorPosition { get; }

        /// <summary>
        /// Error character at the error position
        /// </summary>
        string ErrorCharacter { get; }

        /// <summary>
        /// Error description
        /// </summary>
        string ErrorDescription { get; }
    }
}

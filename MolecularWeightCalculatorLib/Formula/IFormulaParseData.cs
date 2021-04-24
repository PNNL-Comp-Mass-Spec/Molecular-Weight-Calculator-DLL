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
        /// Error data object for the formula parsing
        /// </summary>
        IErrorData ErrorData { get; }
    }

    [ComVisible(false)]
    public interface IErrorData
    {
        /// <summary>
        /// Contains the error number (used in the LookupMessage method).
        /// In addition, if a program error occurs, ErrorParams.ErrorID = -10
        /// </summary>
        int ErrorId { get; }
        int ErrorPosition { get; }
        string ErrorCharacter { get; }
        string ErrorDescription { get; }
    }
}

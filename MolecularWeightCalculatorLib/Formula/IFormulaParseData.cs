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
        /// The corrected formula (appropriate capitalization, expanded abbreviations, etc.). May not be correct when an error occurs.
        /// </summary>
        string FormulaCorrected { get; }

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
        /// Contains the error number (used in the LookupMessage function).  In addition, if a program error occurs, ErrorParams.ErrorID = -10
        /// </summary>
        int ErrorId { get; }
        int ErrorPosition { get; }
        string ErrorCharacter { get; }
        string ErrorDescription { get; }
    }
}

using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    /// <summary>
    /// Formula parsing options
    /// </summary>
    [ComVisible(false)]
    public class FormulaOptions
    {
        // Ignore Spelling: Arial

        /// <summary>
        /// Abbreviation recognition mode
        /// </summary>
        /// <remarks>
        /// NormalOnly, NormalPlusAminoAcids (default), or NoAbbreviations
        /// </remarks>
        public AbbrevRecognitionMode AbbrevRecognitionMode { get; set; }

        /// <summary>
        /// When true, treat square brackets as parentheses
        /// </summary>
        /// <remarks>Default is true</remarks>
        public bool BracketsAsParentheses { get; set; }

        /// <summary>
        /// Case conversion mode
        /// </summary>
        /// <remarks>
        /// ConvertCaseUp (default), ExactCase, or SmartCase
        /// </remarks>
        public CaseConversionMode CaseConversion { get; set; }

        /// <summary>
        /// Decimal point symbol
        /// </summary>
        /// <remarks>
        /// Typically a period, but if 5,500 matches 5.5 then a comma
        /// </remarks>
        public char DecimalSeparator { get; set; }

        /// <summary>
        /// Rich text font name
        /// </summary>
        /// <remarks>Default is Arial</remarks>
        public string RtfFontName { get; set; }

        /// <summary>
        /// Rich text size
        /// </summary>
        /// <remarks>Default is 10</remarks>
        public short RtfFontSize { get; set; }

        /// <summary>
        /// Standard deviation mode
        /// </summary>
        /// <remarks>
        /// Short, Scientific, or Decimal (default)
        /// </remarks>
        public StdDevMode StdDevMode { get; set; }
    }
}
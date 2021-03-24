using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    public class FormulaOptions
    {
        public AbbrevRecognitionMode AbbrevRecognitionMode { get; set; }
        public bool BracketsAsParentheses { get; set; }
        public CaseConversionMode CaseConversion { get; set; }
        public char DecimalSeparator { get; set; }
        public string RtfFontName { get; set; }
        public short RtfFontSize { get; set; }
        public StdDevMode StdDevMode { get; set; } // Can be 0, 1, or 2 (see StdDevModeConstants)
    }
}
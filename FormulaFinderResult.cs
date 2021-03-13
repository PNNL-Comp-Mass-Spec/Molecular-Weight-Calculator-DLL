using System.Collections.Generic;

namespace MwtWinDll
{
    public class FormulaFinderResult
    {
        public readonly string EmpiricalFormula;

        public Dictionary<string, int> CountsByElement { get; private set; }
        public double Mass { get; set; }
        public double DeltaMass { get; set; }
        public bool DeltaMassIsPPM { get; set; }
        public double MZ { get; set; }
        public int ChargeState { get; set; }

        /// <summary>
    /// Percent composition results (only valid if matching percent compositions)
    /// </summary>
    /// <remarks>Keys are element or abbreviation symbols, values are percent composition, between 0 and 100</remarks>
        public Dictionary<string, double> PercentComposition { get; set; }

        public string SortKey;

        public FormulaFinderResult(string newEmpiricalFormula, Dictionary<string, int> empiricalResultSymbols)
        {
            EmpiricalFormula = newEmpiricalFormula;
            CountsByElement = empiricalResultSymbols;
            SortKey = string.Empty;
            PercentComposition = new Dictionary<string, double>();
        }

        public new string ToString()
        {
            if (DeltaMassIsPPM)
            {
                return EmpiricalFormula + "   MW=" + Mass.ToString("0.0000") + "   dm=" + DeltaMass.ToString("0.00" + " ppm");
            }
            else
            {
                return EmpiricalFormula + "   MW=" + Mass.ToString("0.0000") + "   dm=" + DeltaMass.ToString("0.0000");
            }
        }
    }
}
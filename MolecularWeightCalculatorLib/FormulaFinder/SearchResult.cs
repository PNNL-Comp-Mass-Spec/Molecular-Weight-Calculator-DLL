using System.Collections.Generic;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator.FormulaFinder
{
    [Guid("4ACF7379-E609-4331-856E-45B06F408212"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class SearchResult : IFormulaFinderSearchResult
    {
        // Ignore Spelling: dm, interop

        public string EmpiricalFormula { get; }

        public Dictionary<string, int> CountsByElement { get; }

        public double Mass { get; set; }
        public double DeltaMass { get; set; }
        public bool DeltaMassIsPPM { get; set; }
        public double Mz { get; set; }

        public int ChargeState { get; set; }

        /// <summary>
        /// Percent composition results (only valid if matching percent compositions)
        /// </summary>
        /// <remarks>Keys are element or abbreviation symbols, values are percent composition, between 0 and 100</remarks>
        public Dictionary<string, double> PercentComposition { get; set; }

        public string SortKey;

        public SearchResult(string newEmpiricalFormula, Dictionary<string, int> empiricalResultSymbols)
        {
            EmpiricalFormula = newEmpiricalFormula;
            CountsByElement = empiricalResultSymbols;

            SortKey = string.Empty;
            PercentComposition = new Dictionary<string, double>();
        }

        public override string ToString()
        {
            if (DeltaMassIsPPM)
            {
                return EmpiricalFormula + "   MW=" + Mass.ToString("0.0000") + "   dm=" + DeltaMass.ToString("0.00") + " ppm";
            }

            return EmpiricalFormula + "   MW=" + Mass.ToString("0.0000") + "   dm=" + DeltaMass.ToString("0.0000");
        }

        /// <summary>
        /// Method to get data from CountsByElement that is compatible with COM interop
        /// </summary>
        /// <param name="symbols"></param>
        /// <param name="counts"></param>
        /// <returns>Count of symbols/counts returned</returns>
        public int GetElementCounts(out string[] symbols, out int[] counts)
        {
            symbols = new string[CountsByElement.Count];
            counts = new int[CountsByElement.Count];
            var counter = 0;

            foreach (var entry in CountsByElement)
            {
                symbols[counter] = entry.Key;
                counts[counter] = entry.Value;
                counter++;
            }

            return CountsByElement.Count;
        }

        /// <summary>
        /// Method to get data from PercentComposition that is compatible with COM interop
        /// </summary>
        /// <param name="symbols"></param>
        /// <param name="percentCompositions"></param>
        /// <returns>Count of symbols/percentCompositions returned</returns>
        public int GetPercentCompositions(out string[] symbols, out double[] percentCompositions)
        {
            symbols = new string[PercentComposition.Count];
            percentCompositions = new double[PercentComposition.Count];
            var counter = 0;

            foreach (var entry in PercentComposition)
            {
                symbols[counter] = entry.Key;
                percentCompositions[counter] = entry.Value;
                counter++;
            }

            return PercentComposition.Count;
        }
    }
}
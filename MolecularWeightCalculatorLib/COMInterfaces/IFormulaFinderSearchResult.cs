using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("9FF841FB-A0B1-41D4-B2E3-0C7CE4FD15BC"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IFormulaFinderSearchResult
    {
        string EmpiricalFormula { get; }
        //// Commented out because it's not compatible with COM interop. See GetElementCounts() instead.
        //Dictionary<string, int> CountsByElement { get; }
        double Mass { get; set; }
        double DeltaMass { get; set; }
        bool DeltaMassIsPPM { get; set; }
        double Mz { get; set; }
        int ChargeState { get; set; }

        ///// <summary>
        ///// Percent composition results (only valid if matching percent compositions)
        ///// </summary>
        ///// <remarks>Keys are element or abbreviation symbols, values are percent composition, between 0 and 100</remarks>
        //// Commented out because it's not compatible with COM interop. See GetPercentCompositions() instead.
        //Dictionary<string, double> PercentComposition { get; set; }

        string ToString();

        /// <summary>
        /// Method to get data from CountsByElement that is compatible with COM interop
        /// </summary>
        /// <param name="symbols"></param>
        /// <param name="counts"></param>
        /// <returns>Count of symbols/counts returned</returns>
        int GetElementCounts(out string[] symbols, out int[] counts);

        /// <summary>
        /// Method to get data from PercentComposition that is compatible with COM interop
        /// </summary>
        /// <param name="symbols"></param>
        /// <param name="percentCompositions"></param>
        /// <returns>Count of symbols/percentCompositions returned</returns>
        int GetPercentCompositions(out string[] symbols, out double[] percentCompositions);
    }
}
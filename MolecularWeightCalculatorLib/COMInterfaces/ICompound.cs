using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("9159B749-8285-44D1-995C-93BEBDF2B207"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface ICompound
    {
        // Ignore Spelling: interop

        string ConvertToEmpirical();
        bool ElementPresent(short elementId);
        string ExpandAbbreviations();
        double GetAtomCountForElement(short elementId);
        double GetPercentCompositionForElement(short elementId);
        string GetPercentCompositionForElementAsString(short elementId, bool includeStandardDeviation = true);

        /// <summary>
        /// Get the percent composition for all elements in an empirical formula. This implementation is specifically for COM interop support
        /// </summary>
        /// <returns>
        /// 2D array of percent composition values; data[x,0] is element symbol, data[x,1] is percent composition
        /// </returns>
        string[,] GetPercentCompositionForAllElements2DArray();

        short GetUsedElementCount();
        int SetFormula(string newFormula);
        bool XIsPresentAfterBracket();
        string CautionDescription { get; }
        float Charge { get; set; }
        string ErrorDescription { get; }
        int ErrorId { get; }
        string Formula { get; set; }
        string FormulaCapitalized { get; }
        string FormulaRTF { get; }
        double GetMass(bool recomputeMass = true);
        string GetMassAndStdDevString(bool recomputeMass = true);
        double Mass { get; }
        string MassAndStdDevString { get; }
        double StandardDeviation { get; }
        double ValueForUnknown { get; set; }
    }
}
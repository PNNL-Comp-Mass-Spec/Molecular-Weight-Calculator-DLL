using System.Runtime.InteropServices;
using MolecularWeightCalculator.FormulaFinder;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    // Ignore Spelling: interop

    [Guid("FCAEBB03-02BD-46D5-9E0B-C74D81F625B7"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), ComVisible(true)]
    public interface IFormulaSearcherEvents
    {
        event MessageEventEventHandler MessageEvent;
        event ErrorEventEventHandler ErrorEvent;
        event WarningEventEventHandler WarningEvent;
    }

    [Guid("DCDEBDEB-1B50-4FD9-B42A-D921C450E922"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IFormulaSearcher
    {
        ///// <summary>
        ///// Element symbols to consider when finding empirical formulas
        ///// </summary>
        ///// <value></value>
        ///// <returns></returns>
        ///// <remarks>The values in the dictionary are target percent composition values; only used if you call FindMatchesByPercentComposition</remarks>
        //// Commented out because COM does not support generic types, and the primary use of this is for testing.
        //Dictionary<string, FormulaFinder.CandidateElementTolerances> CandidateElements { get; set; }
        /// <summary>
        /// When true, echo messages to the console
        /// </summary>
        bool EchoMessagesToConsole { get; set; }

        /// <summary>
        /// Maximum number of results to report
        /// </summary>
        int MaximumHits { get; set; }

        /// <summary>
        /// Percent complete, between 0 and 100
        /// </summary>
        double PercentComplete { get; }

        /// <summary>
        /// Abort processing
        /// </summary>
        /// <remarks>Only useful if the formula finder is running on a separate thread from the calling program</remarks>
        void AbortProcessingNow();

        /// <summary>
        /// Add a candidate element, abbreviation, or monoisotopic mass
        /// </summary>
        /// <param name="elementSymbolAbbrevOrMass">Element symbol, abbreviation symbol, or monoisotopic mass</param>
        void AddCandidateElement(string elementSymbolAbbrevOrMass);

        /// <summary>
        /// Add a candidate element, abbreviation, or monoisotopic mass
        /// </summary>
        /// <param name="elementSymbolAbbrevOrMass">Element symbol, abbreviation symbol, or monoisotopic mass</param>
        /// <param name="targetPercentComposition">Target percent composition</param>
        void AddCandidateElement(string elementSymbolAbbrevOrMass, double targetPercentComposition);

        /// <summary>
        /// Add a candidate element, abbreviation, or monoisotopic mass
        /// </summary>
        /// <remarks>This method should be used when defining elements for a bounded search</remarks>
        /// <param name="elementSymbolAbbrevOrMass">Element symbol, abbreviation symbol, or monoisotopic mass</param>
        /// <param name="minimumCount">Minimum occurrence count</param>
        /// <param name="maximumCount">Maximum occurrence count</param>
        void AddCandidateElement(string elementSymbolAbbrevOrMass, int minimumCount, int maximumCount);

        /// <summary>
        /// Add a candidate element, abbreviation, or monoisotopic mass
        /// </summary>
        /// <param name="elementSymbolAbbrevOrMass">Element symbol, abbreviation symbol, or monoisotopic mass</param>
        /// <param name="elementTolerances">Search tolerances, including % composition range and Min/Max count when using a bounded search</param>
        void AddCandidateElement(string elementSymbolAbbrevOrMass, CandidateElementTolerances elementTolerances);

        /// <summary>
        /// Find empirical formulas that match the given target mass, with the given ppm tolerance, getting results in an array for COM interop support
        /// </summary>
        /// <param name="targetMass"></param>
        /// <param name="massTolerancePPM"></param>
        /// <param name="searchOptions">If null, uses default search options</param>
        SearchResult[] FindMatchesByMassPPMGetArray(double targetMass, double massTolerancePPM, SearchOptions searchOptions = null);

        /// <summary>
        /// Find empirical formulas that match the given target mass, with the given tolerance, getting results in an array for COM interop support
        /// </summary>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <param name="searchOptions">If null, uses default search options</param>
        SearchResult[] FindMatchesByMassGetArray(double targetMass, double massToleranceDa, SearchOptions searchOptions = null);

        /// <summary>
        /// Find empirical formulas that match target percent composition values
        /// </summary>
        /// <param name="maximumFormulaMass"></param>
        /// <param name="percentTolerance"></param>
        /// <param name="searchOptions"></param>
        SearchResult[] FindMatchesByPercentCompositionGetArray(double maximumFormulaMass, double percentTolerance, SearchOptions searchOptions);

        /// <summary>
        /// Reset to defaults
        /// </summary>
        void Reset();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    public class ElementAndMassTools
    {
        // Molecular Weight Calculator routines with ActiveX Class interfaces: ElementAndMassTools

        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2003
        // Converted to C# by Bryson Gibbons in 2021
        // E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
        // Website: https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL and https://omics.pnl.gov/
        // -------------------------------------------------------------------------------
        //
        // Licensed under the Apache License, Version 2.0; you may not use this file except
        // in compliance with the License.  You may obtain a copy of the License at
        // http://www.apache.org/licenses/LICENSE-2.0
        //
        // Notice: This computer software was prepared by Battelle Memorial Institute,
        // hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the
        // Department of Energy (DOE).  All rights in the computer software are reserved
        // by DOE on behalf of the United States Government and the Contractor as
        // provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY
        // WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS
        // SOFTWARE.  This notice including this sentence must appear on any copies of
        // this computer software.

        // Ignore Spelling: alph, Convoluting, func, gaussian, Isoelectric, isoStats, Mwt
        // Ignore Spelling: parenth, pos, prepend, struct, xyVals, yyyy-MM-dd, hh:mm:ss, tt

        /// <summary>
        /// Constructor
        /// </summary>
        public ElementAndMassTools()
        {
            mElementAlph = new List<KeyValuePair<string, int>>(ELEMENT_COUNT);
            mElementStats = new ElementInfo[ELEMENT_COUNT + 1];
            mElementStats[0] = new ElementInfo(); // 'Invalid' element at index 0

            mAbbrevStats = new List<AbbrevStatsData>(60);

            mCautionStatements = new Dictionary<string, string>(50);
            mMessageStatements = new Dictionary<int, string>(220);
            mMasterSymbolsList = new List<SymbolLookupInfo>();

            mProgressStepDescription = string.Empty;
            mProgressPercentComplete = 0f;

            mLogFolderPath = string.Empty;
            mLogFilePath = string.Empty;

            ShowErrorMessageDialogs = false;
        }

        #region "Constants and Enums"

        public const int ELEMENT_COUNT = 103;
        public const int MAX_ABBREV_COUNT = 500;

        private const int MESSAGE_STATEMENT_DIM_COUNT = 1600;
        internal const int MAX_ABBREV_LENGTH = 6;
        internal const int MAX_ISOTOPES = 11;
        private const int MAX_CAUTION_STATEMENTS = 100;

        private const char EMPTY_STRING_CHAR = '~';
        private const char RTF_HEIGHT_ADJUST_CHAR = '~'; // A hidden character to adjust the height of RTF Text Boxes when using superscripts

        private enum SymbolMatchMode
        {
            Unknown = 0,
            Element = 1,
            Abbreviation = 2
        }

        private enum MessageType
        {
            Normal = 0,
            Error = 1,
            Warning = 2
        }

        #endregion

        #region "Data classes"

        private class ErrorDescription
        {
            public int ErrorId { get; set; } // Contains the error number (used in the LookupMessage function).  In addition, if a program error occurs, ErrorParams.ErrorID = -10
            public int ErrorPosition { get; set; }
            public string ErrorCharacter { get; set; }

            public override string ToString()
            {
                return "ErrorID " + ErrorId + " at " + ErrorPosition + ": " + ErrorCharacter;
            }
        }

        private class IsoResultsByElement
        {
            public int ElementIndex { get; } // Index of element in ElementStats[] array; look in ElementStats[] to get information on its isotopes
            public bool ExplicitIsotope { get; } // True if an explicitly defined isotope
            public double ExplicitMass { get; }
            public int AtomCount { get; } // Number of atoms of this element in the formula being parsed
            public int ResultsCount { get; set; } // Number of masses in MassAbundances; changed at times for data filtering purposes
            public int StartingResultsMass { get; set; } // Starting mass of the results for this element
            public float[] MassAbundances { get; private set; } // Abundance of each mass, starting with StartingResultsMass; 0-based array (can't change to a list)

            public IsoResultsByElement(int elementIndex, int atomCount, double explicitMass, bool explicitIsotope = false)
            {
                ElementIndex = elementIndex;
                AtomCount = atomCount;
                ExplicitMass = explicitMass;
                ExplicitIsotope = explicitIsotope;

                ResultsCount = 0;
                MassAbundances = new float[1];
            }

            public void SetArraySize(int count)
            {
                MassAbundances = new float[count];
            }

            public override string ToString()
            {
                return $"Element {ElementIndex}: {AtomCount} atoms";
            }
        }

        private class IsoResultsOverallData
        {
            public float Abundance { get; set; }
            public int Multiplicity { get; set; }

            public override string ToString()
            {
                return $"{Abundance:F2}";
            }
        }

        private class AbbrevSymbolStack
        {
            public List<short> SymbolReferenceStack { get; }

            public AbbrevSymbolStack()
            {
                SymbolReferenceStack = new List<short>(1);
            }

            /// <summary>
            /// Update the abbreviation symbol stack
            /// </summary>
            /// <param name="symbolReference"></param>
            public void Add(short symbolReference)
            {
                SymbolReferenceStack.Add(symbolReference);
            }

            /// <summary>
            /// Update the abbreviation symbol stack
            /// </summary>
            public void RemoveMostRecent()
            {
                if (SymbolReferenceStack.Count > 0)
                {
                    SymbolReferenceStack.RemoveAt(SymbolReferenceStack.Count - 1);
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        private class XYData
        {
            public double X { get; set; }
            public double Y { get; set; }

            public override string ToString()
            {
                return $"{X:F2}: {Y:F2}";
            }
        }

        /// <summary>
        /// struct for data for mMasterSymbolsList; using a struct because it means less space, and we don't edit the struct
        /// </summary>
        private struct SymbolLookupInfo : IComparable<SymbolLookupInfo>
        {
            /// <summary>
            /// Symbol to match - can be an abbreviation or a chemical/atomic symbol for an element
            /// </summary>
            public readonly string Symbol;

            /// <summary>
            /// Basically, a reference to which list <see cref="Index"/> contains this symbol
            /// </summary>
            public readonly SymbolMatchMode MatchType;

            /// <summary>
            /// The index of this symbol in the list referred to by <see cref="MatchType"/>
            /// </summary>
            public readonly int Index;

            public SymbolLookupInfo(string symbol, int index, SymbolMatchMode matchType = SymbolMatchMode.Unknown)
            {
                Symbol = symbol;
                Index = index;
                MatchType = matchType;
            }

            public int CompareTo(SymbolLookupInfo other)
            {
                // For sorting: sort longest to shortest, then alphabetically
                // 'other' first to sort by length descending
                var lengthCompare = other.Symbol.Length.CompareTo(Symbol.Length);
                if (lengthCompare == 0)
                {
                    return string.Compare(Symbol, other.Symbol, StringComparison.Ordinal);
                }

                return lengthCompare;
            }

            public override string ToString()
            {
                return $"{Symbol}: {MatchType} index {Index}";
            }
        }
        #endregion

        #region "Class wide Variables"

        public FormulaOptions ComputationOptions { get; } = new FormulaOptions();

        /// <summary>
        /// Stores the elements in alphabetical order, with Key==Symbol, and Value==Index in <see cref="mElementStats"/>
        /// Used for constructing empirical formulas
        /// 0 to ELEMENT_COUNT - 1
        /// </summary>
        private readonly List<KeyValuePair<string, int>> mElementAlph;

        /// <summary>
        /// Element stats
        /// 1 to ELEMENT_COUNT, 0 is basically 'invalid element'
        /// Leaving '0' as an invalid element allows indexing this array with the atomic number of the element
        /// </summary>
        private readonly ElementInfo[] mElementStats;

        /// <summary>
        /// Stores the element symbols, abbreviations, and amino acids in order of longest symbol length to shortest length, non-alphabetized,
        /// for use in symbol matching when parsing a formula
        /// 0 To .Count - 1
        /// </summary>
        private readonly List<SymbolLookupInfo> mMasterSymbolsList;

        /// <summary>
        /// Includes both abbreviations and amino acids
        /// </summary>
        private readonly List<AbbrevStatsData> mAbbrevStats;

        /// <summary>
        /// CautionStatements.Key holds the symbol combo to look for
        /// CautionStatements.Value holds the caution statement
        /// </summary>
        private readonly Dictionary<string, string> mCautionStatements;

        /// <summary>
        /// Error messages
        /// </summary>
        private readonly Dictionary<int, string> mMessageStatements;

        private readonly ErrorDescription mErrorParams = new ErrorDescription();

        /// <summary>
        /// Charge carrier mass
        /// 1.00727649 for monoisotopic mass or 1.00739 for average mass
        /// </summary>
        private double mChargeCarrierMass;

        private ElementMassMode mCurrentElementMode = ElementMassMode.Average;
        private string mCautionDescription;
        private ComputationStats mComputationStatsSaved = new ComputationStats();

        protected bool mAbortProcessing;

        protected bool mLogMessagesToFile;
        protected string mLogFilePath;
        protected System.IO.StreamWriter mLogFile;

        /// <summary>
        /// Log file folder
        /// If blank, mOutputFolderPath will be used
        /// If mOutputFolderPath is also blank,  the log is created in the same folder as the executing assembly
        /// </summary>
        protected string mLogFolderPath;

        public event ProgressResetEventHandler ProgressReset;
        public event ProgressChangedEventHandler ProgressChanged;
        public event ProgressCompleteEventHandler ProgressComplete;

        protected string mProgressStepDescription;

        /// <summary>
        /// Ranges from 0 to 100, but can contain decimal percentage values
        /// </summary>
        protected float mProgressPercentComplete;

        #endregion

        #region "Interface Functions"

        public bool AbortProcessing
        {
            get => mAbortProcessing;
            set => mAbortProcessing = value;
        }

        public ElementMassMode ElementModeInternal
        {
            get => mCurrentElementMode;
            set => SetElementModeInternal(value);
        }

        public string LogFilePath => mLogFilePath;

        public string LogFolderPath
        {
            get => mLogFolderPath;
            set => mLogFolderPath = value;
        }

        public bool LogMessagesToFile
        {
            get => mLogMessagesToFile;
            set => mLogMessagesToFile = value;
        }

        public virtual string ProgressStepDescription => mProgressStepDescription;

        /// <summary>
        /// Percent complete; ranges from 0 to 100, but can contain decimal percentage values
        /// </summary>
        /// <returns></returns>
        public float ProgressPercentComplete => (float)Math.Round(mProgressPercentComplete, 2);

        public bool ShowErrorMessageDialogs { get; set; }

        #endregion

        public virtual void AbortProcessingNow()
        {
            mAbortProcessing = true;
        }

        /// <summary>
        /// Add an abbreviation
        /// </summary>
        /// <param name="abbrevIndex"></param>
        /// <param name="symbol"></param>
        /// <param name="formula"></param>
        /// <param name="charge"></param>
        /// <param name="isAminoAcid"></param>
        /// <param name="oneLetter"></param>
        /// <param name="comment"></param>
        /// <param name="invalidSymbolOrFormula"></param>
        /// <returns><paramref name="formula"/> with format standardized by ParseFormulaPublic</returns>
        private string AddAbbreviationWork(
            short abbrevIndex, string symbol,
            string formula, float charge,
            bool isAminoAcid,
            string oneLetter = "",
            string comment = "",
            bool invalidSymbolOrFormula = false)
        {
            AbbrevStatsData stats;
            if (abbrevIndex < 0 || abbrevIndex >= mAbbrevStats.Count)
            {
                stats = new AbbrevStatsData(symbol, formula, charge, isAminoAcid, oneLetter, comment, invalidSymbolOrFormula);
                mAbbrevStats.Add(stats);
            }
            else
            {
                stats = mAbbrevStats[abbrevIndex];
                stats.InvalidSymbolOrFormula = invalidSymbolOrFormula;
                stats.Symbol = symbol;
                stats.Formula = formula;
                stats.Charge = charge;
                stats.OneLetterSymbol = oneLetter.ToUpper();
                stats.IsAminoAcid = isAminoAcid;
                stats.Comment = comment;
            }

            stats.Mass = ComputeFormulaWeight(ref formula);
            if (stats.Mass < 0d)
            {
                // Error occurred computing mass for abbreviation
                stats.Mass = 0d;
                stats.InvalidSymbolOrFormula = true;
            }

            return formula;
        }

        private void AddToCautionDescription(string textToAdd)
        {
            if (string.IsNullOrWhiteSpace(mCautionDescription))
            {
                mCautionDescription = "";
            }

            mCautionDescription += textToAdd;
        }

        private void CheckCaution(string formulaExcerpt)
        {
            for (var length = 0; length < MAX_ABBREV_LENGTH; length++)
            {
                if (length > formulaExcerpt.Length)
                    break;

                var test = formulaExcerpt.Substring(0, length);
                var newCaution = LookupCautionStatement(test);
                if (!string.IsNullOrEmpty(newCaution))
                {
                    AddToCautionDescription(newCaution);
                    break;
                }
            }
        }

        private void CatchParseNumError(double adjacentNum, int numSizing, int curCharacter, int symbolLength)
        {
            if (adjacentNum < 0d && numSizing == 0)
            {
                switch (adjacentNum)
                {
                    case -1:
                        // No number, but no error
                        // That's OK
                        break;
                    case -3:
                        // Error: No number after decimal point
                        mErrorParams.ErrorId = 12;
                        mErrorParams.ErrorPosition = curCharacter + symbolLength;
                        break;
                    case -4:
                        // Error: More than one decimal point
                        mErrorParams.ErrorId = 27;
                        mErrorParams.ErrorPosition = curCharacter + symbolLength;
                        break;
                    default:
                        // Error: General number error
                        mErrorParams.ErrorId = 14;
                        mErrorParams.ErrorPosition = curCharacter + symbolLength;
                        break;
                }
            }
        }

        /// <summary>
        /// Examines the formula excerpt to determine if it is an element, abbreviation, amino acid, or unknown
        /// </summary>
        /// <param name="formulaExcerpt"></param>
        /// <param name="symbolReference">Output: index of the matched element or abbreviation in mMasterSymbolsList[]</param>
        /// <returns>
        /// Element if matched an element
        /// Abbreviation if matched an abbreviation or amino acid
        /// Unknown if no match
        /// </returns>
        private SymbolMatchMode CheckElemAndAbbrev(string formulaExcerpt, out short symbolReference)
        {
            var symbolMatchType = default(SymbolMatchMode);
            symbolReference = -1;

            // mMasterSymbolsList[] stores the element symbols, abbreviations, & amino acids in order of longest length to
            // shortest length, then by alphabet sorting, for use in symbol matching when parsing a formula

            // Look for match, stepping directly through mMasterSymbolsList[]
            // List is sorted by reverse length, so can do all at once

            foreach (var lookupSymbol in mMasterSymbolsList)
            {
                if (lookupSymbol.Symbol?.Length > 0)
                {
                    if (formulaExcerpt.Substring(0, Math.Min(formulaExcerpt.Length, lookupSymbol.Symbol.Length)) == lookupSymbol.Symbol)
                    {
                        // Matched a symbol
                        symbolMatchType = lookupSymbol.MatchType;
                        if (symbolMatchType == SymbolMatchMode.Unknown)
                        {
                            symbolReference = -1;
                        }
                        else
                        {
                            symbolReference = (short)lookupSymbol.Index;
                        }

                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Zero-length entry found in mMasterSymbolsList[]; this is unexpected");
                }
            }

            return symbolMatchType;
        }

        /// <summary>
        /// Compute the weight of a formula (or abbreviation)
        /// </summary>
        /// <param name="formula">Input</param>
        /// <returns>The formula mass, or -1 if an error occurs</returns>
        /// <remarks>Error information is stored in ErrorParams</remarks>
        public double ComputeFormulaWeight(string formula)
        {
            return ComputeFormulaWeight(ref formula);
        }

        /// <summary>
        /// Compute the weight of a formula (or abbreviation)
        /// </summary>
        /// <param name="formula">Input/output: updated by ParseFormulaPublic</param>
        /// <returns>The formula mass, or -1 if an error occurs</returns>
        /// <remarks>Error information is stored in ErrorParams</remarks>
        public double ComputeFormulaWeight(ref string formula)
        {
            var computationStats = new ComputationStats();

            ParseFormulaPublic(ref formula, computationStats, false);

            if (mErrorParams.ErrorId == 0)
            {
                return computationStats.TotalMass;
            }

            return -1;
        }

        /// <summary>
        /// Computes the Isotopic Distribution for a formula
        /// </summary>
        /// <param name="formulaIn">Input/output: The properly formatted formula to parse</param>
        /// <param name="chargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
        /// <param name="results">Output: Table of results</param>
        /// <param name="convolutedMSData2D">2D array of MSData (mass and intensity pairs)</param>
        /// <param name="convolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
        /// <param name="addProtonChargeCarrier">If addProtonChargeCarrier is False, then still convolute by charge, but doesn't add a proton</param>
        /// <param name="headerIsotopicAbundances">Header to use in <paramref name="results"/></param>
        /// <param name="headerMassToCharge">Header to use in <paramref name="results"/></param>
        /// <param name="headerFraction">Header to use in <paramref name="results"/></param>
        /// <param name="headerIntensity">Header to use in <paramref name="results"/></param>
        /// <param name="useFactorials">Set to true to use Factorial math, which is typically slower; default is False</param>
        /// <returns>0 if success, -1 if an error</returns>
        /// <remarks>
        /// Returns uncharged mass values if <paramref name="chargeState"/>=0,
        /// Returns M+H values if <paramref name="chargeState"/>=1
        /// Returns convoluted m/z if <paramref name="chargeState"/> is &gt; 1
        /// </remarks>
        public short ComputeIsotopicAbundancesInternal(
            ref string formulaIn,
            short chargeState,
            out string results,
            out double[,] convolutedMSData2D,
            out int convolutedMSDataCount,
            bool addProtonChargeCarrier = true,
            string headerIsotopicAbundances = "Isotopic Abundances for",
            string headerMassToCharge = "Mass/Charge",
            string headerFraction = "Fraction",
            string headerIntensity = "Intensity",
            bool useFactorials = false)
        {
            convolutedMSData2D = new double[0, 0];
            convolutedMSDataCount = 0;
            results = "";

            var computationStats = new ComputationStats();

            double nextComboFractionalAbundance = default;

            const string deuteriumEquiv = "^2.014H";

            const double minAbundanceToKeep = 0.000001d;
            const double cutoffForRatioMethod = 0.00001d;

            var explicitIsotopesPresent = default(bool);
            var explicitIsotopeCount = default(short);

            double logRho = default;

            // Make sure formula is not blank
            if (string.IsNullOrEmpty(formulaIn))
            {
                return -1;
            }

            mAbortProcessing = false;
            try
            {
                double temp;
                float percentComplete;
                int predictedCombos;
                int atomCount;
                short isotopeCount;
                // Change headerMassToCharge to "Neutral Mass" if chargeState = 0 and headerMassToCharge is "Mass/Charge"
                if (chargeState == 0)
                {
                    if (headerMassToCharge == "Mass/Charge")
                    {
                        headerMassToCharge = "Neutral Mass";
                    }
                }

                // Parse Formula to determine if valid and number of each element
                var formula = formulaIn;
                var workingFormulaMass = ParseFormulaPublic(ref formula, computationStats, false);

                if (workingFormulaMass < 0d)
                {
                    // Error occurred; information is stored in ErrorParams
                    results = LookupMessage(350) + ": " + LookupMessage(mErrorParams.ErrorId);
                    return -1;
                }

                // See if Deuterium is present by looking for a fractional amount of Hydrogen
                // formula will contain a capital D followed by a number or another letter (or the end of formula)
                // If found, replace each D with ^2.014H and re-compute
                var count = computationStats.Elements[1].Count;
                if (Math.Abs(count - (int)Math.Round(count)) > float.Epsilon)
                {
                    // Deuterium is present
                    var modifiedFormula = "";
                    short index = 0;
                    while (index < formula.Length)
                    {
                        var replaceDeuterium = false;
                        if (formula.Substring(index, 1) == "D")
                        {
                            if (index == formula.Length - 1)
                            {
                                replaceDeuterium = true;
                            }
                            else
                            {
                                var asciiOfNext = (int)formula[index + 1];
                                if (asciiOfNext < 97 || asciiOfNext > 122)
                                {
                                    replaceDeuterium = true;
                                }
                            }

                            if (replaceDeuterium)
                            {
                                if (index > 0)
                                {
                                    modifiedFormula = formula.Substring(0, Math.Min(formula.Length, index - 1));
                                }

                                modifiedFormula += deuteriumEquiv;
                                if (index < formula.Length - 1)
                                {
                                    modifiedFormula += formula.Substring(index + 1);
                                }

                                formula = modifiedFormula;
                                index = 0;
                            }
                        }

                        index++;
                    }

                    // Re-Parse Formula since D's are now ^2.014H
                    workingFormulaMass = ParseFormulaPublic(ref formula, computationStats, false);

                    if (workingFormulaMass < 0d)
                    {
                        // Error occurred; information is stored in ErrorParams
                        results = LookupMessage(350) + ": " + LookupMessage(mErrorParams.ErrorId);
                        return -1;
                    }
                }

                // Make sure there are no fractional atoms present (need to specially handle Deuterium)
                for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                {
                    count = computationStats.Elements[elementIndex].Count;
                    if (Math.Abs(count - (int)Math.Round(count)) > float.Epsilon)
                    {
                        results = LookupMessage(350) + ": " + LookupMessage(805) + ": " + mElementStats[elementIndex].Symbol + count;
                        return -1;
                    }
                }

                // Remove occurrences of explicitly defined isotopes from the formula
                for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                {
                    var element = computationStats.Elements[elementIndex];
                    if (element.Isotopes.Count > 0)
                    {
                        explicitIsotopesPresent = true;
                        explicitIsotopeCount += (short)element.Isotopes.Count;
                        for (var isotopeIndex = 0; isotopeIndex < element.Isotopes.Count; isotopeIndex++)
                            element.Count -= element.Isotopes[isotopeIndex].Count;
                    }
                }

                // Determine the number of elements present in formula
                short elementCount = 0;
                for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                {
                    if (computationStats.Elements[elementIndex].Used)
                    {
                        elementCount++;
                    }
                }

                if (explicitIsotopesPresent)
                {
                    elementCount += explicitIsotopeCount;
                }

                if (elementCount == 0 || Math.Abs(workingFormulaMass) < float.Epsilon)
                {
                    // No elements or no weight
                    return -1;
                }

                // The formula seems valid, so update formulaIn
                formulaIn = formula;

                // Reserve memory for isoStats[] array
                var isoStats = new List<IsoResultsByElement>(elementCount);

                // Step through computationStats.Elements[] again and copy info into isoStats[]
                // In addition, determine minimum and maximum weight for the molecule
                var minWeight = 0;
                var maxWeight = 0;
                for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                {
                    if (computationStats.Elements[elementIndex].Used)
                    {
                        if (computationStats.Elements[elementIndex].Count > 0d)
                        {
                            var stats = mElementStats[elementIndex];
                            // Note: Ignoring .Elements[elementIndex].IsotopicCorrection when getting atom count
                            var isoStat = new IsoResultsByElement(elementIndex, (int)Math.Round(computationStats.Elements[elementIndex].Count), stats.Mass);
                            isoStats.Add(isoStat);

                            minWeight = (int)Math.Round(minWeight + isoStat.AtomCount * Math.Round(stats.Isotopes[0].Mass, 0));
                            maxWeight = (int)Math.Round(maxWeight + isoStat.AtomCount * Math.Round(stats.Isotopes[stats.Isotopes.Count - 1].Mass, 0));
                        }
                    }
                }

                if (explicitIsotopesPresent)
                {
                    // Add the isotopes, pretending they are unique elements
                    for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                    {
                        var element = computationStats.Elements[elementIndex];
                        if (element.Isotopes.Count > 0)
                        {
                            for (var isotopeIndex = 0; isotopeIndex < element.Isotopes.Count; isotopeIndex++)
                            {
                                var isoStat = new IsoResultsByElement(elementIndex, (int)Math.Round(element.Isotopes[isotopeIndex].Count), element.Isotopes[isotopeIndex].Mass, true);
                                isoStats.Add(isoStat);

                                minWeight = (int)Math.Round(minWeight + isoStat.AtomCount * isoStat.ExplicitMass);
                                maxWeight = (int)Math.Round(maxWeight + isoStat.AtomCount * isoStat.ExplicitMass);
                            }
                        }
                    }
                }

                if (minWeight < 0)
                    minWeight = 0;

                // Create an array to hold the Fractional Abundances for all the masses
                convolutedMSDataCount = maxWeight - minWeight + 1;
                var convolutedAbundanceStartMass = minWeight;
                var convolutedAbundances = new IsoResultsOverallData[convolutedMSDataCount]; // Fractional abundance at each mass; 1-based array

                for (var i = 0; i < convolutedAbundances.Length; i++)
                {
                    convolutedAbundances[i] = new IsoResultsOverallData();
                }

                // Predict the total number of computations required; show progress if necessary
                var predictedTotalComboCalcs = 0;
                for (var index = 0; index < isoStats.Count; index++)
                {
                    var masterElementIndex = isoStats[index].ElementIndex;
                    atomCount = isoStats[index].AtomCount;
                    isotopeCount = (short)mElementStats[masterElementIndex].Isotopes.Count;

                    predictedCombos = FindCombosPredictIterations(atomCount, isotopeCount);
                    predictedTotalComboCalcs += predictedCombos;
                }

                ResetProgress("Finding Isotopic Abundances: Computing abundances");

                // For each element, compute all of the possible combinations
                var completedComboCalcs = 0;
                for (var index = 0; index < isoStats.Count; index++)
                {
                    short isotopeStartingMass;
                    short isotopeEndingMass;
                    var isoStat = isoStats[index];
                    var masterElementIndex = isoStat.ElementIndex;
                    atomCount = isoStat.AtomCount;

                    if (isoStat.ExplicitIsotope)
                    {
                        isotopeCount = 1;
                        isotopeStartingMass = (short)Math.Round(isoStat.ExplicitMass);
                        isotopeEndingMass = isotopeStartingMass;
                    }
                    else
                    {
                        var stats = mElementStats[masterElementIndex];
                        isotopeCount = (short)stats.Isotopes.Count;
                        isotopeStartingMass = (short)Math.Round(Math.Round(stats.Isotopes[0].Mass, 0));
                        isotopeEndingMass = (short)Math.Round(Math.Round(stats.Isotopes[isotopeCount - 1].Mass, 0));
                    }

                    predictedCombos = FindCombosPredictIterations(atomCount, isotopeCount);

                    if (predictedCombos > 10000000)
                    {
                        var message = "Too many combinations necessary for prediction of isotopic distribution: " + predictedCombos.ToString("#,##0") + Environment.NewLine + "Please use a simpler formula or reduce the isotopic range defined for the element (currently " + isotopeCount + ")";
                        if (ShowErrorMessageDialogs)
                        {
                            MessageBox.Show(message);
                        }

                        LogMessage(message, MessageType.Error);
                        return -1;
                    }

                    var isoCombos = new int[predictedCombos, isotopeCount];
                    // 2D array: Holds the # of each isotope for each combination
                    // For example, Two chlorine atoms, Cl2, has at most 6 combos since Cl isotopes are 35, 36, and 37
                    // m1  m2  m3
                    // 2   0   0
                    // 1   1   0
                    // 1   0   1
                    // 0   2   0
                    // 0   1   1
                    // 0   0   2

                    var combosFound = FindCombosRecurse(isoCombos, atomCount, isotopeCount) + 1;

                    // The predicted value should always match the actual value, unless explicitIsotopesPresent = True
                    if (!explicitIsotopesPresent)
                    {
                        if (predictedCombos != combosFound)
                        {
                            Console.WriteLine("PredictedCombos doesn't match CombosFound (" + predictedCombos + " vs. " + combosFound + "); this is unexpected");
                        }
                    }

                    // Reserve space for the abundances based on the minimum and maximum weight of the isotopes of the element
                    minWeight = atomCount * isotopeStartingMass;
                    maxWeight = atomCount * isotopeEndingMass;
                    var resultingMassCountForElement = maxWeight - minWeight + 1;
                    isoStat.StartingResultsMass = minWeight;
                    isoStat.ResultsCount = resultingMassCountForElement;
                    isoStat.SetArraySize(resultingMassCountForElement);

                    if (isoStat.ExplicitIsotope)
                    {
                        // Explicitly defined isotope; there is only one "combo" and its abundance = 1
                        isoStat.MassAbundances[0] = 1f;
                    }
                    else
                    {
                        var fractionalAbundanceSaved = 0d;
                        for (var comboIndex = 0; comboIndex < combosFound; comboIndex++)
                        {
                            int indexToStoreAbundance;
                            completedComboCalcs += 1;

                            percentComplete = completedComboCalcs / (float)predictedTotalComboCalcs * 100f;
                            if (completedComboCalcs % 10 == 0)
                            {
                                UpdateProgress(percentComplete);
                            }

                            double thisComboFractionalAbundance = -1;
                            var ratioMethodUsed = false;
                            var rigorousMethodUsed = false;

                            if (useFactorials)
                            {
                                // #######
                                // Rigorous, slow, easily overflowed method
                                // #######
                                //
                                rigorousMethodUsed = true;

                                // AbundDenom and AbundSuffix are only needed if using the easily-overflowed factorial method
                                var abundDenom = 1d;
                                var abundSuffix = 1d;
                                var stats = mElementStats[masterElementIndex];
                                for (var isotopeIndex = 0; isotopeIndex < isotopeCount; isotopeIndex++)
                                {
                                    var isotopeCountInThisCombo = isoCombos[comboIndex, isotopeIndex];
                                    if (isotopeCountInThisCombo > 0)
                                    {
                                        abundDenom *= MathUtils.Factorial(isotopeCountInThisCombo);
                                        abundSuffix *= Math.Pow(stats.Isotopes[isotopeIndex].Abundance, isotopeCountInThisCombo);
                                    }
                                }

                                thisComboFractionalAbundance = MathUtils.Factorial(atomCount) / abundDenom * abundSuffix;
                            }
                            else
                            {
                                if (fractionalAbundanceSaved < cutoffForRatioMethod)
                                {
                                    // #######
                                    // Equivalent of rigorous method, but uses logarithms
                                    // #######
                                    //
                                    rigorousMethodUsed = true;

                                    var logSigma = 0d;
                                    for (var sigma = 1; sigma <= atomCount; sigma++)
                                        logSigma += Math.Log(sigma);

                                    var sumI = 0d;
                                    for (var isotopeIndex = 0; isotopeIndex < isotopeCount; isotopeIndex++)
                                    {
                                        if (isoCombos[comboIndex, isotopeIndex] > 0)
                                        {
                                            var workingSum = 0d;
                                            for (var subIndex = 1; subIndex <= isoCombos[comboIndex, isotopeIndex]; subIndex++)
                                                workingSum += Math.Log(subIndex);

                                            sumI += workingSum;
                                        }
                                    }

                                    var stats = mElementStats[masterElementIndex];
                                    var sumF = 0d;
                                    for (var isotopeIndex = 0; isotopeIndex < isotopeCount; isotopeIndex++)
                                    {
                                        if (stats.Isotopes[isotopeIndex].Abundance > 0f)
                                        {
                                            sumF += isoCombos[comboIndex, isotopeIndex] * Math.Log(stats.Isotopes[isotopeIndex].Abundance);
                                        }
                                    }

                                    var logFreq = logSigma - sumI + sumF;
                                    thisComboFractionalAbundance = Math.Exp(logFreq);

                                    fractionalAbundanceSaved = thisComboFractionalAbundance;
                                }

                                // Use thisComboFractionalAbundance to predict
                                // the Fractional Abundance of the Next Combo
                                if (comboIndex < combosFound - 1 && fractionalAbundanceSaved >= cutoffForRatioMethod)
                                {
                                    // #######
                                    // Third method, determines the ratio of this combo's abundance and the next combo's abundance
                                    // #######
                                    //
                                    var ratioOfFreqs = 1d;

                                    for (var isotopeIndex = 0; isotopeIndex < isotopeCount; isotopeIndex++)
                                    {
                                        double m = isoCombos[comboIndex, isotopeIndex];
                                        double mPrime = isoCombos[comboIndex + 1, isotopeIndex];

                                        if (m > mPrime)
                                        {
                                            var logSigma = 0d;
                                            for (var subIndex = (int)Math.Round(mPrime) + 1; subIndex <= (int)Math.Round(m); subIndex++)
                                                logSigma += Math.Log(subIndex);

                                            logRho = logSigma - (m - mPrime) * Math.Log(mElementStats[masterElementIndex].Isotopes[isotopeIndex].Abundance);
                                        }
                                        else if (m < mPrime)
                                        {
                                            var logSigma = 0d;
                                            for (var subIndex = (int)Math.Round(m) + 1; subIndex <= (int)Math.Round(mPrime); subIndex++)
                                                logSigma += Math.Log(subIndex);

                                            var stats = mElementStats[masterElementIndex];
                                            if (stats.Isotopes[isotopeIndex].Abundance > 0f)
                                            {
                                                logRho = (mPrime - m) * Math.Log(stats.Isotopes[isotopeIndex].Abundance) - logSigma;
                                            }
                                        }
                                        else
                                        {
                                            //m = mPrime
                                            logRho = 0d;
                                        }

                                        var rho = Math.Exp(logRho);
                                        ratioOfFreqs *= rho;
                                    }

                                    nextComboFractionalAbundance = fractionalAbundanceSaved * ratioOfFreqs;

                                    fractionalAbundanceSaved = nextComboFractionalAbundance;
                                    ratioMethodUsed = true;
                                }
                            }

                            if (rigorousMethodUsed)
                            {
                                // Determine nominal mass of this combination; depends on number of atoms of each isotope
                                indexToStoreAbundance = FindIndexForNominalMass(isoCombos, comboIndex, isotopeCount, atomCount, mElementStats[masterElementIndex].Isotopes);

                                // Store the abundance in .MassAbundances[] at location IndexToStoreAbundance
                                // TODO: Use +=
                                isoStat.MassAbundances[indexToStoreAbundance] = (float)(isoStat.MassAbundances[indexToStoreAbundance] + thisComboFractionalAbundance);
                            }

                            if (ratioMethodUsed)
                            {
                                // Store abundance for next Combo
                                indexToStoreAbundance = FindIndexForNominalMass(isoCombos, comboIndex + 1, isotopeCount, atomCount, mElementStats[masterElementIndex].Isotopes);

                                // Store the abundance in .MassAbundances[] at location IndexToStoreAbundance
                                // TODO: Use +=
                                isoStat.MassAbundances[indexToStoreAbundance] = (float)(isoStat.MassAbundances[indexToStoreAbundance] + nextComboFractionalAbundance);
                            }

                            if (ratioMethodUsed && comboIndex + 2 == combosFound)
                            {
                                // No need to compute the last combo since we just did it
                                break;
                            }
                        }
                    }

                    if (mAbortProcessing)
                        break;
                }

                if (mAbortProcessing)
                {
                    // Process Aborted
                    results = LookupMessage(940);
                    return -1;
                }

                // Step Through IsoStats from the end to the beginning, shortening the length to the
                // first value greater than MIN_ABUNDANCE_TO_KEEP
                // This greatly speeds up the convolution
                for (var isoIndex = 0; isoIndex < isoStats.Count; isoIndex++)
                {
                    var stats = isoStats[isoIndex];
                    var index = stats.ResultsCount - 1;
                    while (stats.MassAbundances[index] < minAbundanceToKeep)
                    {
                        index -= 1;
                        if (index == 0)
                            break;
                    }

                    stats.ResultsCount = index + 1;
                }

                // Examine IsoStats[] to predict the number of ConvolutionIterations
                // Commented out because the only calculated value is not used anywhere.
                //long predictedConvIterations = isoStats[0].ResultsCount;
                //for (var index = 1; index < isoStats.Count; index++)
                //    predictedConvIterations *= isoStats[index].ResultsCount;

                ResetProgress("Finding Isotopic Abundances: Convoluting results");

                // Convolute the results for each element using a recursive convolution routine
                for (var index = 0; index < isoStats[0].ResultsCount; index++)
                {
                    ConvoluteMasses(convolutedAbundances, convolutedAbundanceStartMass, isoStats, index);

                    percentComplete = index / (float)isoStats[1].ResultsCount * 100f;
                    UpdateProgress(percentComplete);
                }

                if (mAbortProcessing)
                {
                    // Process Aborted
                    results = LookupMessage(940);
                    return -1;
                }

                // Compute mass defect (difference of initial isotope's mass from integer mass)
                var exactBaseIsoMass = 0d;
                for (var index = 0; index < isoStats.Count; index++)
                {
                    var stats = isoStats[index];
                    if (stats.ExplicitIsotope)
                    {
                        exactBaseIsoMass += stats.AtomCount * stats.ExplicitMass;
                    }
                    else
                    {
                        exactBaseIsoMass += stats.AtomCount * mElementStats[stats.ElementIndex].Isotopes[0].Mass;
                    }
                }

                var massDefect = Math.Round(exactBaseIsoMass - convolutedAbundanceStartMass, 5);

                // Assure that the mass defect is only a small percentage of the total mass
                // This won't be true for very small compounds so temp is set to at least 10
                if (convolutedAbundanceStartMass < 10)
                {
                    temp = 10d;
                }
                else
                {
                    temp = convolutedAbundanceStartMass;
                }

                var maxPercentDifference = Math.Pow(10d, -(3d - Math.Round(Math.Log10(temp), 0)));

                if (Math.Abs(massDefect / exactBaseIsoMass) >= maxPercentDifference)
                {
                    Console.WriteLine("massDefect / exactBaseIsoMass is greater maxPercentDifference: (" + massDefect / exactBaseIsoMass + " vs. " + maxPercentDifference + "); this is unexpected");
                }

                // Step Through convolutedAbundances[], starting at the end, and find the first value above MIN_ABUNDANCE_TO_KEEP
                // Decrease convolutedMSDataCount to remove the extra values below MIN_ABUNDANCE_TO_KEEP
                for (var massIndex = convolutedMSDataCount - 1; massIndex >= 0; massIndex -= 1)
                {
                    if (convolutedAbundances[massIndex].Abundance > minAbundanceToKeep)
                    {
                        convolutedMSDataCount = massIndex + 1;
                        break;
                    }
                }

                var output = headerIsotopicAbundances + " " + formula + Environment.NewLine;
                output += SpacePad("  " + headerMassToCharge, 12) + "\t" + SpacePad(headerFraction, 9) + "\t" + headerIntensity + Environment.NewLine;

                // Initialize convolutedMSData2D[]
                convolutedMSData2D = new double[convolutedMSDataCount, 2];

                // Find Maximum Abundance
                var maxAbundance = 0d;
                for (var massIndex = 0; massIndex < convolutedMSDataCount; massIndex++)
                {
                    if (convolutedAbundances[massIndex].Abundance > maxAbundance)
                    {
                        maxAbundance = convolutedAbundances[massIndex].Abundance;
                    }
                }

                // Populate the results array with the masses and abundances
                // Also, if chargeState is >= 1, then convolute the mass to the appropriate m/z
                if (Math.Abs(maxAbundance) < float.Epsilon)
                    maxAbundance = 1d;
                for (var massIndex = 0; massIndex < convolutedMSDataCount; massIndex++)
                {
                    var mass = convolutedAbundances[massIndex];
                    convolutedMSData2D[massIndex, 0] = convolutedAbundanceStartMass + massIndex + massDefect;
                    convolutedMSData2D[massIndex, 1] = mass.Abundance / maxAbundance * 100d;

                    if (chargeState >= 1)
                    {
                        if (addProtonChargeCarrier)
                        {
                            convolutedMSData2D[massIndex, 0] = ConvoluteMassInternal(convolutedMSData2D[massIndex, 0], 0, chargeState);
                        }
                        else
                        {
                            convolutedMSData2D[massIndex, 0] = convolutedMSData2D[massIndex, 0] / chargeState;
                        }
                    }
                }

                // Step through convolutedMSData2DOneBased[] from the beginning to find the
                // first value greater than MIN_ABUNDANCE_TO_KEEP
                var rowIndex = 0;
                while (convolutedMSData2D[rowIndex, 1] < minAbundanceToKeep)
                {
                    rowIndex += 1;
                    if (rowIndex == convolutedMSDataCount - 1)
                        break;
                }

                // If rowIndex > 0 then remove rows from beginning since value is less than MIN_ABUNDANCE_TO_KEEP
                if (rowIndex > 0 && rowIndex < convolutedMSDataCount - 1)
                {
                    rowIndex -= 1;
                    // Remove rows from the start of convolutedMSData2DOneBased[] since 0 mass
                    for (var massIndex = rowIndex + 1; massIndex < convolutedMSDataCount; massIndex++)
                    {
                        convolutedMSData2D[massIndex - rowIndex, 0] = convolutedMSData2D[massIndex, 0];
                        convolutedMSData2D[massIndex - rowIndex, 1] = convolutedMSData2D[massIndex, 1];
                    }

                    convolutedMSDataCount -= rowIndex;
                }

                // Write to output
                for (var massIndex = 0; massIndex < convolutedMSDataCount; massIndex++)
                {
                    output += SpacePadFront(convolutedMSData2D[massIndex, 0].ToString("#0.00000"), 12) + "\t";
                    output += (convolutedMSData2D[massIndex, 1] * maxAbundance / 100d).ToString("0.0000000") + "\t";
                    output += SpacePadFront(convolutedMSData2D[massIndex, 1].ToString("##0.00"), 7) + Environment.NewLine;
                    //ToDo: Fix Multiplicity
                    //output += ConvolutedAbundances(massIndex).Multiplicity.ToString("##0") + Environment.NewLine
                }

                results = output;
            }
            catch (Exception ex)
            {
                MwtWinDllErrorHandler("MolecularWeightCalculator_ElementAndMassTools|ComputeIsotopicAbundances", ex);
                mErrorParams.ErrorId = 590;
                mErrorParams.ErrorPosition = 0;
                return -1;
            }

            return 0; // Success
        }

        /// <summary>
        /// Compute percent composition of the elements defined in <paramref name="computationStats"/>
        /// </summary>
        /// <param name="computationStats">Input/output</param>
        public void ComputePercentComposition(ComputationStats computationStats)
        {
            // Determine the number of elements in the formula
            for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
            {
                if (computationStats.TotalMass > 0d)
                {
                    var elementTotalMass = computationStats.Elements[elementIndex].Count * mElementStats[elementIndex].Mass + computationStats.Elements[elementIndex].IsotopicCorrection;

                    // Percent is the percent composition
                    var percentComp = elementTotalMass / computationStats.TotalMass * 100.0d;
                    computationStats.PercentCompositions[elementIndex].PercentComposition = percentComp;

                    // Calculate standard deviation
                    double stdDeviation;
                    if (Math.Abs(computationStats.Elements[elementIndex].IsotopicCorrection - 0d) < float.Epsilon)
                    {
                        // No isotopic mass correction factor exists
                        stdDeviation = percentComp * Math.Sqrt(Math.Pow(mElementStats[elementIndex].Uncertainty / mElementStats[elementIndex].Mass, 2d) + Math.Pow(computationStats.StandardDeviation / computationStats.TotalMass, 2d));
                    }
                    else
                    {
                        // Isotopic correction factor exists, assume no error in it
                        stdDeviation = percentComp * Math.Sqrt(Math.Pow(computationStats.StandardDeviation / computationStats.TotalMass, 2d));
                    }

                    if (Math.Abs(elementTotalMass - computationStats.TotalMass) < float.Epsilon && Math.Abs(percentComp - 100d) < float.Epsilon)
                    {
                        stdDeviation = 0d;
                    }

                    computationStats.PercentCompositions[elementIndex].StdDeviation = stdDeviation;
                }
                else
                {
                    computationStats.PercentCompositions[elementIndex].PercentComposition = 0d;
                    computationStats.PercentCompositions[elementIndex].StdDeviation = 0d;
                }
            }
        }

        /// <summary>
        /// Convert the centroided data (stick data) in XYVals to a Gaussian representation
        /// </summary>
        /// <param name="xyVals">XY data, as key-value pairs</param>
        /// <param name="resolution">Effective instrument resolution (e.g. 1000 or 20000)</param>
        /// <param name="resolutionMass">The m/z value at which the resolution applies</param>
        /// <param name="qualityFactor">Gaussian quality factor (between 1 and 75, default is 50)</param>
        /// <returns>Gaussian spectrum data</returns>
        /// <remarks></remarks>
        public List<KeyValuePair<double, double>> ConvertStickDataToGaussian2DArray(List<KeyValuePair<double, double>> xyVals, int resolution, double resolutionMass, int qualityFactor)
        {
            // xyVals is 0-based (thus ranging from 0 to xyVals.count-1)
            // The arrays should contain stick data
            // The original data in the arrays will be replaced with Gaussian peaks in place of each "stick"
            // Note: Assumes xyVals is sorted in the 'x' direction

            const int maxDataPoints = 1000000;
            const short massPrecision = 7;

            var thisDataPoint = new XYData();

            var gaussianData = new List<KeyValuePair<double, double>>();

            try
            {
                double xValRange;
                if (xyVals == null || xyVals.Count == 0)
                {
                    return gaussianData;
                }

                var xySummation = new List<XYData>(xyVals.Count * 10);

                // Determine the data range for xyVals
                if (xyVals.Count > 1)
                {
                    xValRange = xyVals.Last().Key - xyVals.First().Key;
                }
                else
                {
                    xValRange = 1d;
                }

                if (xValRange < 1d)
                    xValRange = 1d;

                if (resolution < 1)
                    resolution = 1;

                if (qualityFactor < 1 || qualityFactor > 75)
                    qualityFactor = 50;

                // Compute deltaX using resolution and resolutionMass
                // Do not allow the DeltaX to be so small that the total points required > MAX_DATA_POINTS
                var deltaX = resolutionMass / resolution / qualityFactor;

                // Make sure DeltaX is a reasonable number
                deltaX = RoundToMultipleOf10(deltaX);

                if (Math.Abs(deltaX) < float.Epsilon)
                    deltaX = 1d;

                // Set the Window Range to 1/10 the magnitude of the midpoint x value
                var rangeWork = xyVals.First().Key + xValRange / 2d;
                rangeWork = RoundToMultipleOf10(rangeWork);

                var sigma = resolutionMass / resolution / Math.Sqrt(5.54d);

                // Set the window range (the xValue window width range) to calculate the Gaussian representation for each data point
                // The width at the base of a peak is 4 sigma
                // Use a width of 2 * 6 sigma
                var xValWindowRange = 2 * 6 * sigma;

                if (xValRange / deltaX > maxDataPoints)
                {
                    // Delta x is too small; change to a reasonable value
                    // This isn't a bug, but it may mean one of the default settings is inappropriate
                    deltaX = xValRange / maxDataPoints;
                }

                var dataToAddCount = (int)Math.Round(xValWindowRange / deltaX);

                // Make sure dataToAddCount is odd
                if (dataToAddCount % 2 == 0)
                {
                    dataToAddCount += 1;
                }

                var dataToAdd = new List<XYData>(dataToAddCount);
                var midPointIndex = (int)Math.Round((dataToAddCount + 1) / 2d - 1d);

                // Compute the Gaussian data for each point in xyVals[]
                for (var stickIndex = 0; stickIndex < xyVals.Count; stickIndex++)
                {
                    if (stickIndex % 25 == 0)
                    {
                        if (AbortProcessing)
                            break;
                    }

                    // Search through xySummation to determine the index of the smallest XValue with which
                    // data in dataToAdd could be combined
                    var minimalSummationIndex = 0;
                    dataToAdd.Clear();

                    var minimalXValOfWindow = xyVals[stickIndex].Key - midPointIndex * deltaX;

                    var searchForMinimumXVal = true;
                    if (xySummation.Count > 0)
                    {
                        if (minimalXValOfWindow > xySummation[xySummation.Count - 1].X)
                        {
                            minimalSummationIndex = xySummation.Count - 1;
                            searchForMinimumXVal = false;
                        }
                    }

                    if (searchForMinimumXVal)
                    {
                        if (xySummation.Count <= 0)
                        {
                            minimalSummationIndex = 0;
                        }
                        else
                        {
                            int summationIndex;
                            for (summationIndex = 0; summationIndex < xySummation.Count; summationIndex++)
                            {
                                if (xySummation[summationIndex].X >= minimalXValOfWindow)
                                {
                                    minimalSummationIndex = summationIndex - 1;
                                    if (minimalSummationIndex < 0)
                                        minimalSummationIndex = 0;
                                    break;
                                }
                            }

                            if (summationIndex >= xySummation.Count)
                            {
                                minimalSummationIndex = xySummation.Count - 1;
                            }
                        }
                    }

                    // Construct the Gaussian representation for this Data Point
                    thisDataPoint.X = xyVals[stickIndex].Key;
                    thisDataPoint.Y = xyVals[stickIndex].Value;

                    // Round ThisDataPoint.XVal to the nearest DeltaX
                    // If .XVal is not an even multiple of DeltaX then bump up .XVal until it is
                    thisDataPoint.X = RoundToEvenMultiple(thisDataPoint.X, deltaX, true);

                    for (var index = 0; index < dataToAddCount; index++)
                    {
                        // Equation for Gaussian is: Amplitude * Exp[ -(x - mu)^2 / (2*sigma^2) ]
                        // Use index, .YVal, and deltaX
                        var xOffSet = (midPointIndex - index) * deltaX;

                        var newPoint = new XYData
                        {
                            X = thisDataPoint.X - xOffSet,
                            Y = thisDataPoint.Y * Math.Exp(-Math.Pow(xOffSet, 2d) / (2d * Math.Pow(sigma, 2d)))
                        };

                        dataToAdd.Add(newPoint);
                    }

                    // Now merge dataToAdd into xySummation
                    // XValues in dataToAdd and those in xySummation have the same DeltaX value
                    // The XValues in dataToAdd might overlap partially with those in xySummation

                    var dataIndex = 0;
                    bool appendNewData;

                    // First, see if the first XValue in dataToAdd is larger than the last XValue in xySummation
                    if (xySummation.Count <= 0)
                    {
                        appendNewData = true;
                    }
                    else if (dataToAdd[dataIndex].X > xySummation.Last().X)
                    {
                        appendNewData = true;
                    }
                    else
                    {
                        appendNewData = false;
                        // Step through xySummation starting at minimalSummationIndex, looking for
                        // the index to start combining data at
                        for (var summationIndex = minimalSummationIndex; summationIndex < xySummation.Count; summationIndex++)
                        {
                            if (Math.Round(dataToAdd[dataIndex].X, massPrecision) <= Math.Round(xySummation[summationIndex].X, massPrecision))
                            {

                                // Within Tolerance; start combining the values here
                                while (summationIndex <= xySummation.Count - 1)
                                {
                                    var currentVal = xySummation[summationIndex];
                                    currentVal.Y += dataToAdd[dataIndex].Y;

                                    xySummation[summationIndex] = currentVal;

                                    summationIndex += 1;
                                    dataIndex += 1;
                                    if (dataIndex >= dataToAddCount)
                                    {
                                        // Successfully combined all of the data
                                        break;
                                    }
                                }

                                if (dataIndex < dataToAddCount)
                                {
                                    // Data still remains to be added
                                    appendNewData = true;
                                }

                                break;
                            }
                        }
                    }

                    if (appendNewData)
                    {
                        while (dataIndex < dataToAddCount)
                        {
                            xySummation.Add(dataToAdd[dataIndex]);
                            dataIndex += 1;
                        }
                    }
                }

                // Assure there is a data point at each 1% point along x range (to give better looking plots)
                // Probably need to add data, but may need to remove some
                var minimalXValSpacing = xValRange / 100d;

                for (var summationIndex = 0; summationIndex < xySummation.Count - 1; summationIndex++)
                {
                    if (xySummation[summationIndex].X + minimalXValSpacing < xySummation[summationIndex + 1].X)
                    {
                        // Need to insert a data point

                        // Choose the appropriate new .XVal
                        rangeWork = xySummation[summationIndex + 1].X - xySummation[summationIndex].X;
                        if (rangeWork < minimalXValSpacing * 2d)
                        {
                            rangeWork /= 2d;
                        }
                        else
                        {
                            rangeWork = minimalXValSpacing;
                        }

                        // The new .YVal is the average of that at summationIndex and that at summationIndex + 1
                        var newDataPoint = new XYData
                        {
                            X = xySummation[summationIndex].X + rangeWork,
                            Y = (xySummation[summationIndex].Y + xySummation[summationIndex + 1].Y) / 2d
                        };

                        xySummation.Insert(summationIndex + 1, newDataPoint);
                    }
                }

                // Copy data from xySummation to gaussianData
                gaussianData.Capacity = Math.Max(gaussianData.Count, xySummation.Count);
                gaussianData.AddRange(xySummation.Select(item => new KeyValuePair<double, double>(item.X, item.Y)));
            }
            catch (Exception ex)
            {
                GeneralErrorHandler("ConvertStickDataToGaussian", ex);
            }

            return gaussianData;
        }

        public void ConstructMasterSymbolsList()
        {
            // Call after loading or changing abbreviations or elements
            // Call after loading or setting abbreviation mode

            mMasterSymbolsList.Clear();
            mMasterSymbolsList.Capacity = ELEMENT_COUNT + mAbbrevStats.Count;

            // Construct search list
            for (var index = 1; index <= ELEMENT_COUNT; index++)
            {
                mMasterSymbolsList.Add(new SymbolLookupInfo(mElementStats[index].Symbol, index, SymbolMatchMode.Element));
            }

            // Note: mAbbrevStats is 0-based
            if (ComputationOptions.AbbrevRecognitionMode != AbbrevRecognitionMode.NoAbbreviations)
            {
                bool includeAmino;
                if (ComputationOptions.AbbrevRecognitionMode == AbbrevRecognitionMode.NormalPlusAminoAcids)
                {
                    includeAmino = true;
                }
                else
                {
                    includeAmino = false;
                }

                for (var index = 0; index < mAbbrevStats.Count; index++)
                {
                    var stats = mAbbrevStats[index];
                    // If includeAmino = False then do not include amino acids
                    if (includeAmino || !stats.IsAminoAcid)
                    {
                        // Do not include if the formula is invalid
                        if (!stats.InvalidSymbolOrFormula)
                        {
                            mMasterSymbolsList.Add(new SymbolLookupInfo(stats.Symbol, index, SymbolMatchMode.Abbreviation));
                        }
                    }
                }
            }

            mMasterSymbolsList.Capacity = mMasterSymbolsList.Count;

            // Sort the search list
            mMasterSymbolsList.Sort(); // Will use the IComparable implementation for longest-to-shortest, then alphabetical.
        }

        /// <summary>
        /// Converts <paramref name="massMz"/> to the MZ that would appear at the given <paramref name="desiredCharge"/>
        /// </summary>
        /// <param name="massMz"></param>
        /// <param name="currentCharge"></param>
        /// <param name="desiredCharge"></param>
        /// <param name="chargeCarrierMass">Charge carrier mass.  If 0, this function will use mChargeCarrierMass instead</param>
        /// <returns>The new m/z value</returns>
        /// <remarks>To return the neutral mass, set <paramref name="desiredCharge"/> to 0</remarks>
        public double ConvoluteMassInternal(
            double massMz,
            short currentCharge,
            short desiredCharge = 1,
            double chargeCarrierMass = 0)
        {
            const double defaultChargeCarrierMassMonoiso = 1.00727649d;

            double newMz;

            if (Math.Abs(chargeCarrierMass - 0d) < float.Epsilon)
                chargeCarrierMass = mChargeCarrierMass;
            if (Math.Abs(chargeCarrierMass - 0d) < float.Epsilon)
                chargeCarrierMass = defaultChargeCarrierMassMonoiso;

            if (currentCharge == desiredCharge)
            {
                newMz = massMz;
            }
            else
            {
                if (currentCharge == 1)
                {
                    newMz = massMz;
                }
                else if (currentCharge > 1)
                {
                    // Convert massMz to M+H
                    newMz = massMz * currentCharge - chargeCarrierMass * (currentCharge - 1);
                }
                else if (currentCharge == 0)
                {
                    // Convert massMz (which is neutral) to M+H and store in newMz
                    newMz = massMz + chargeCarrierMass;
                }
                else
                {
                    // Negative charges are not supported; return 0
                    return 0d;
                }

                if (desiredCharge > 1)
                {
                    newMz = (newMz + chargeCarrierMass * (desiredCharge - 1)) / desiredCharge;
                }
                else if (desiredCharge == 1)
                {
                    // Return M+H, which is currently stored in newMz
                }
                else if (desiredCharge == 0)
                {
                    // Return the neutral mass
                    newMz -= chargeCarrierMass;
                }
                else
                {
                    // Negative charges are not supported; return 0
                    newMz = 0d;
                }
            }

            return newMz;
        }

        /// <summary>
        /// Converts <paramref name="formula"/> to its corresponding empirical formula
        /// </summary>
        /// <param name="formula"></param>
        /// <returns>The empirical formula, or -1 if an error</returns>
        /// <remarks></remarks>
        public string ConvertFormulaToEmpirical(string formula)
        {
            var computationStats = new ComputationStats();

            // Call ParseFormulaPublic to compute the formula's mass and fill computationStats
            var mass = ParseFormulaPublic(ref formula, computationStats);

            if (mErrorParams.ErrorId == 0)
            {
                // Convert to empirical formula
                var empiricalFormula = "";
                // Carbon first, then hydrogen, then the rest alphabetically
                // This is correct to start at -2
                for (var elementIndex = -2; elementIndex < mElementAlph.Count; elementIndex++)
                {
                    int elementIndexToUse;
                    if (elementIndex == -2)
                    {
                        // Do Carbon first
                        elementIndexToUse = 6;
                    }
                    else if (elementIndex == -1)
                    {
                        // Then do Hydrogen
                        elementIndexToUse = 1;
                    }
                    else
                    {
                        // Then do the rest alphabetically
                        if (mElementAlph[elementIndex].Key == "C" || mElementAlph[elementIndex].Key == "H")
                        {
                            // Increment elementIndex when we encounter carbon or hydrogen
                            elementIndex++;
                        }

                        elementIndexToUse = mElementAlph[elementIndex].Value;
                    }

                    // Only display the element if it's in the formula
                    var thisElementCount = mComputationStatsSaved.Elements[elementIndexToUse].Count;
                    if (Math.Abs(thisElementCount - 1.0d) < float.Epsilon)
                    {
                        empiricalFormula += mElementStats[elementIndexToUse].Symbol;
                    }
                    else if (thisElementCount > 0d)
                    {
                        empiricalFormula += mElementStats[elementIndexToUse].Symbol + thisElementCount;
                    }
                }

                return empiricalFormula;
            }

            return (-1).ToString();
        }

        /// <summary>
        /// Expands abbreviations in formula to their elemental equivalent
        /// </summary>
        /// <param name="formula"></param>
        /// <returns>Returns the result, or -1 if an error</returns>
        /// <remarks></remarks>
        public string ExpandAbbreviationsInFormula(string formula)
        {
            var computationStats = new ComputationStats();

            // Call ExpandAbbreviationsInFormula to compute the formula's mass
            var mass = ParseFormulaPublic(ref formula, computationStats, true);

            if (mErrorParams.ErrorId == 0)
            {
                return formula;
            }

            return (-1).ToString();
        }

        private int FindIndexForNominalMass(
            int[,] isoCombos,
            int comboIndex,
            short isotopeCount,
            int atomCount,
            IReadOnlyList<IsotopeInfo> thisElementsIsotopes)
        {
            var workingMass = 0;
            for (var isotopeIndex = 0; isotopeIndex < isotopeCount; isotopeIndex++)
                workingMass = (int)Math.Round(workingMass + isoCombos[comboIndex, isotopeIndex] * Math.Round(thisElementsIsotopes[isotopeIndex].Mass, 0));

            // (workingMass  - IsoStats(ElementIndex).StartingResultsMass) + 1
            return (int)Math.Round(workingMass - atomCount * Math.Round(thisElementsIsotopes[0].Mass, 0));
        }

        /// <summary>
        /// Recursive function to Convolute the Results in <paramref name="isoStats"/> and store in <paramref name="convolutedAbundances"/>; 1-based array
        /// </summary>
        /// <param name="convolutedAbundances"></param>
        /// <param name="convolutedAbundanceStartMass"></param>
        /// <param name="isoStats"></param>
        /// <param name="workingRow"></param>
        /// <param name="workingAbundance"></param>
        /// <param name="workingMassTotal"></param>
        /// <param name="isoStatsIndex"></param>
        /// <param name="iterations"></param>
        private void ConvoluteMasses(
            IsoResultsOverallData[] convolutedAbundances,
            int convolutedAbundanceStartMass,
            List<IsoResultsByElement> isoStats,
            int workingRow,
            float workingAbundance = 1,
            int workingMassTotal = 0,
            int isoStatsIndex = 0,
            long iterations = 0)
        {
            if (mAbortProcessing)
                return;

            iterations += 1L;
            if (iterations % 10000L == 0L)
            {
                Application.DoEvents();
            }

            var newAbundance = workingAbundance * isoStats[isoStatsIndex].MassAbundances[workingRow];
            var newMassTotal = workingMassTotal + isoStats[isoStatsIndex].StartingResultsMass + workingRow;

            if (isoStatsIndex >= isoStats.Count - 1)
            {
                var indexToStoreResult = newMassTotal - convolutedAbundanceStartMass;
                var result = convolutedAbundances[indexToStoreResult];
                if (newAbundance > 0f)
                {
                    result.Abundance += newAbundance;
                    result.Multiplicity += 1;
                }
            }
            else
            {
                for (var rowIndex = 0; rowIndex < isoStats[isoStatsIndex + 1].ResultsCount; rowIndex++)
                    ConvoluteMasses(convolutedAbundances, convolutedAbundanceStartMass, isoStats, rowIndex, newAbundance, newMassTotal, isoStatsIndex + 1, iterations);
            }
        }

        /// <summary>
        /// Determines the number of Combo results (iterations) for the given
        /// number of Atoms for an element with the given number of Isotopes
        /// </summary>
        /// <param name="atomCount"></param>
        /// <param name="isotopeCount"></param>
        /// <returns></returns>
        private int FindCombosPredictIterations(int atomCount, short isotopeCount)
        {
            // Empirically determined the following results and figured out that the RunningSum()
            // method correctly predicts the results

            // IsotopeCount   AtomCount    Total Iterations
            // 2             1               2
            // 2             2               3
            // 2             3               4
            // 2             4               5
            //
            // 3             1               3
            // 3             2               6
            // 3             3               10
            // 3             4               15
            //
            // 4             1               4
            // 4             2               10
            // 4             3               20
            // 4             4               35
            //
            // 5             1               5
            // 5             2               15
            // 5             3               35
            // 5             4               70
            //
            // 6             1               6
            // 6             2               21
            // 6             3               56
            // 6             4               126

            var runningSum = new int[atomCount];
            try
            {
                int predictedCombos;
                if (atomCount == 1 || isotopeCount == 1)
                {
                    predictedCombos = isotopeCount;
                }
                else
                {
                    // Initialize runningSum[]
                    for (var atomIndex = 0; atomIndex < atomCount; atomIndex++)
                        runningSum[atomIndex] = atomIndex + 2;

                    for (var isotopeIndex = 3; isotopeIndex <= isotopeCount; isotopeIndex++)
                    {
                        var previousComputedValue = isotopeIndex;
                        for (var atomIndex = 1; atomIndex < atomCount; atomIndex++)
                        {
                            // Compute new count for this AtomIndex
                            runningSum[atomIndex] = previousComputedValue + runningSum[atomIndex];

                            // Also place result in RunningSum(AtomIndex)
                            previousComputedValue = runningSum[atomIndex];
                        }
                    }

                    predictedCombos = runningSum[atomCount - 1];
                }

                return predictedCombos;
            }
            catch (Exception ex)
            {
                MwtWinDllErrorHandler("MolecularWeightCalculator_ElementAndMassTools.FindCombosPredictIterations", ex);
                mErrorParams.ErrorId = 590;
                mErrorParams.ErrorPosition = 0;
                return -1;
            }
        }

        /// <summary>
        /// Recursive function to find all the combinations
        /// of a number of atoms with the given maximum isotopic count
        /// </summary>
        /// <param name="comboResults"></param>
        /// <param name="atomCount"></param>
        /// <param name="maxIsotopeCount"></param>
        /// <param name="currentIsotopeCount"></param>
        /// <param name="currentRow"></param>
        /// <param name="currentCol"></param>
        /// <param name="atomTrackHistory"></param>
        /// <returns>Last modified index</returns>
        private int FindCombosRecurse(
            int[,] comboResults,
            int atomCount,
            int maxIsotopeCount,
            int currentIsotopeCount = -1,
            int currentRow = 0,
            int currentCol = 0,
            int[] atomTrackHistory = null)
        {
            // IsoCombos[] is a 2D array holding the # of each isotope for each combination
            // For example, Two chlorine atoms, Cl2, has at most 6 combos since Cl isotopes are 35, 36, and 37
            // m1  m2  m3
            // 2   0   0
            // 1   1   0
            // 1   0   1
            // 0   2   0
            // 0   1   1
            // 0   0   2

            if (atomTrackHistory == null)
            {
                atomTrackHistory = new int[maxIsotopeCount];
                atomTrackHistory[0] = atomCount;
            }
            if (currentIsotopeCount < 0)
            {
                currentIsotopeCount = maxIsotopeCount;
            }

            // Returns the number of combinations found, or -1 if an error

            if (currentIsotopeCount == 1 || atomCount == 0)
            {
                // End recursion
                comboResults[currentRow, currentCol] = atomCount;
            }
            else
            {
                var atomTrack = atomCount;

                // Store AtomTrack value at current position
                comboResults[currentRow, currentCol] = atomTrack;

                while (atomTrack > 0)
                {
                    currentRow += 1;

                    // Went to a new row; if CurrentCol > 0 then need to assign previous values to previous columns
                    if (currentCol > 0)
                    {
                        for (var colIndex = 0; colIndex < currentCol; colIndex++)
                            comboResults[currentRow, colIndex] = atomTrackHistory[colIndex];
                    }

                    atomTrack -= 1;
                    comboResults[currentRow, currentCol] = atomTrack;

                    if (currentCol < maxIsotopeCount - 1)
                    {
                        atomTrackHistory[currentCol] = atomTrack;
                        currentRow = FindCombosRecurse(comboResults, atomCount - atomTrack, maxIsotopeCount, currentIsotopeCount - 1, currentRow, currentCol + 1, atomTrackHistory);
                    }
                    else
                    {
                        Console.WriteLine("Program bug in FindCombosRecurse. This line should not be reached.");
                    }
                }

                // Reached AtomTrack = 0; end recursion
            }

            return currentRow;
        }

        public void GeneralErrorHandler(string callingProcedure, Exception ex)
        {
            var message = "Error in " + callingProcedure + ": ";
            if (!string.IsNullOrEmpty(ex.Message))
            {
                message += Environment.NewLine + ex.Message;
            }

            LogMessage(message, MessageType.Error);

            if (ShowErrorMessageDialogs)
            {
                MessageBox.Show(message, "Error in MwtWinDll", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                Console.WriteLine(message);
            }

            LogMessage(message, MessageType.Error);
            try
            {
                var errorFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "ErrorLog.txt");

                // Open the file and append a new error entry
                using (var outFile = new System.IO.StreamWriter(errorFilePath, true))
                {
                    outFile.WriteLine(DateTime.Now + " -- " + message + Environment.NewLine);
                }
            }
            catch
            {
                // Ignore errors here
            }
        }

        /// <summary>
        /// Get the number of abbreviations in memory
        /// </summary>
        /// <returns></returns>
        public int GetAbbreviationCountInternal()
        {
            return mAbbrevStats.Count;
        }

        /// <summary>
        /// Get the abbreviation ID for the given abbreviation symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="aminoAcidsOnly"></param>
        /// <returns>ID if found, otherwise -1</returns>
        public int GetAbbreviationIdInternal(string symbol, bool aminoAcidsOnly = false)
        {
            for (var index = 0; index < mAbbrevStats.Count; index++)
            {
                if ((mAbbrevStats[index].Symbol?.ToLower() ?? "") == (symbol?.ToLower() ?? ""))
                {
                    if (!aminoAcidsOnly || mAbbrevStats[index].IsAminoAcid)
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        public int GetAbbreviationInternal(
            int abbreviationId,
            out string symbol,
            out string formula,
            out float charge,
            out bool isAminoAcid)
        {
            return GetAbbreviationInternal(abbreviationId, out symbol, out formula, out charge, out isAminoAcid, out _, out _, out _);
        }

        /// <summary>
        /// Get an abbreviation, by ID
        /// </summary>
        /// <param name="abbreviationId"></param>
        /// <param name="symbol">Output: symbol</param>
        /// <param name="formula">Output: empirical formula</param>
        /// <param name="charge">Output: charge</param>
        /// <param name="isAminoAcid">Output: true if an amino acid</param>
        /// <param name="oneLetterSymbol">Output: one letter symbol (only used by amino acids)</param>
        /// <param name="comment">Output: comment</param>
        /// <param name="invalidSymbolOrFormula">Output: true if an invalid symbol or formula</param>
        /// <returns> 0 if success, 1 if failure</returns>
        public int GetAbbreviationInternal(
            int abbreviationId,
            out string symbol,
            out string formula,
            out float charge,
            out bool isAminoAcid,
            out string oneLetterSymbol,
            out string comment,
            out bool invalidSymbolOrFormula)
        {
            if (abbreviationId >= 0 && abbreviationId < mAbbrevStats.Count)
            {
                var stats = mAbbrevStats[abbreviationId];
                symbol = stats.Symbol;
                formula = stats.Formula;
                charge = stats.Charge;
                isAminoAcid = stats.IsAminoAcid;
                oneLetterSymbol = stats.OneLetterSymbol;
                comment = stats.Comment;
                invalidSymbolOrFormula = stats.InvalidSymbolOrFormula;

                return 0;
            }

            symbol = string.Empty;
            formula = string.Empty;
            charge = 0f;
            isAminoAcid = false;
            oneLetterSymbol = string.Empty;
            comment = string.Empty;
            invalidSymbolOrFormula = true;

            return 1;
        }

        public double GetAbbreviationMass(int abbreviationId)
        {
            // Returns the mass if success, 0 if failure
            // Could return -1 if failure, but might mess up some calculations

            // This function does not recompute the abbreviation mass each time it is called
            // Rather, it uses the .Mass member of AbbrevStats
            // This requires that .Mass be updated if the abbreviation is changed, if an element is changed, or if the element mode is changed

            if (abbreviationId >= 0 && abbreviationId < mAbbrevStats.Count)
            {
                return mAbbrevStats[abbreviationId].Mass;
            }

            return 0d;
        }

        public string GetAminoAcidSymbolConversionInternal(string symbolToFind, bool oneLetterTo3Letter)
        {
            // If oneLetterTo3Letter = true, then converting 1 letter codes to 3 letter codes
            // Returns the symbol, if found
            // Otherwise, returns ""

            var returnSymbol = "";
            // Use AbbrevStats[] array to lookup code
            for (var index = 0; index < mAbbrevStats.Count; index++)
            {
                if (mAbbrevStats[index].IsAminoAcid)
                {
                    string compareSymbol;
                    if (oneLetterTo3Letter)
                    {
                        compareSymbol = mAbbrevStats[index].OneLetterSymbol;
                    }
                    else
                    {
                        compareSymbol = mAbbrevStats[index].Symbol;
                    }

                    if ((compareSymbol?.ToLower() ?? "") == (symbolToFind?.ToLower() ?? ""))
                    {
                        if (oneLetterTo3Letter)
                        {
                            returnSymbol = mAbbrevStats[index].Symbol;
                        }
                        else
                        {
                            returnSymbol = mAbbrevStats[index].OneLetterSymbol;
                        }

                        break;
                    }
                }
            }

            return returnSymbol;
        }

        public int GetCautionStatementCountInternal()
        {
            return mCautionStatements.Count;
        }

        public List<string> GetCautionStatementSymbolsInternal()
        {
            return mCautionStatements.Keys.ToList();
        }

        /// <summary>
        /// Get a caution statement for the given symbol combo
        /// </summary>
        /// <param name="symbolCombo">symbol combo for the caution statement</param>
        /// <param name="cautionStatement">Output: caution statement text</param>
        /// <returns>0 if success, 1 if an invalid ID</returns>
        public int GetCautionStatementInternal(string symbolCombo, out string cautionStatement)
        {
            if (mCautionStatements.TryGetValue(symbolCombo, out cautionStatement))
            {
                return 0;
            }

            cautionStatement = string.Empty;
            return 1;
        }

        public string GetCautionDescription()
        {
            return mCautionDescription;
        }

        public double GetChargeCarrierMassInternal()
        {
            return mChargeCarrierMass;
        }

        public int GetElementCountInternal()
        {
            return ELEMENT_COUNT;
        }

        /// <summary>
        /// Returns the settings for the element with <paramref name="elementId"/> in the ByRef variables
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="symbol"></param>
        /// <param name="mass"></param>
        /// <param name="uncertainty"></param>
        /// <param name="charge"></param>
        /// <param name="isotopeCount"></param>
        /// <returns>0 if success, 1 if failure</returns>
        public int GetElementInternal(
            short elementId,
            out string symbol,
            out double mass,
            out double uncertainty,
            out float charge,
            out short isotopeCount)
        {
            if (elementId >= 1 && elementId <= ELEMENT_COUNT)
            {
                var stats = mElementStats[elementId];
                symbol = stats.Symbol;
                mass = stats.Mass;
                uncertainty = stats.Uncertainty;
                charge = stats.Charge;
                isotopeCount = (short)stats.Isotopes.Count;

                return 0;
            }

            symbol = string.Empty;
            mass = 0d;
            uncertainty = 0d;
            charge = 0f;
            isotopeCount = 0;
            return 1;
        }

        /// <summary>
        /// Get the element ID for the given symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        public short GetElementIdInternal(string symbol)
        {
            for (var index = 1; index <= ELEMENT_COUNT; index++)
            {
                if (string.Equals(mElementStats[index].Symbol, symbol, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (short)index;
                }
            }

            return 0;
        }

        /// <summary>
        /// Returns the isotope masses and abundances for the element with <paramref name="elementId"/>
        /// </summary>
        /// <param name="elementId">Element ID, or atomic number</param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses">output, 0-based array</param>
        /// <param name="isotopeAbundances">output, 0-based array</param>
        /// <returns>0 if a valid ID, 1 if invalid</returns>
        public int GetElementIsotopesInternal(short elementId, out short isotopeCount, out double[] isotopeMasses, out float[] isotopeAbundances)
        {
            if (elementId >= 1 && elementId <= ELEMENT_COUNT)
            {
                var stats = mElementStats[elementId];
                isotopeCount = (short)stats.Isotopes.Count;
                isotopeMasses = new double[isotopeCount];
                isotopeAbundances = new float[isotopeCount];
                for (var isotopeIndex = 0; isotopeIndex < stats.Isotopes.Count; isotopeIndex++)
                {
                    isotopeMasses[isotopeIndex] = stats.Isotopes[isotopeIndex].Mass;
                    isotopeAbundances[isotopeIndex] = stats.Isotopes[isotopeIndex].Abundance;
                }

                return 0;
            }

            isotopeCount = 0;
            isotopeMasses = new double[1];
            isotopeAbundances = new float[1];

            return 1;
        }

        /// <summary>
        /// Get the current element mode
        /// </summary>
        /// <returns>
        /// ElementMassMode.Average  = 1
        /// ElementMassMode.Isotopic = 2
        /// ElementMassMode.Integer  = 3
        /// </returns>
        public ElementMassMode GetElementModeInternal()
        {
            return mCurrentElementMode;
        }

        /// <summary>
        /// Return the element symbol for the given element ID
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        /// <remarks>1 is Hydrogen, 2 is Helium, etc.</remarks>
        public string GetElementSymbolInternal(short elementId)
        {
            if (elementId >= 1 && elementId <= ELEMENT_COUNT)
            {
                return mElementStats[elementId].Symbol;
            }

            return "";
        }

        public List<ElementInfo> GetElements()
        {
            return mElementStats.ToList();
        }

        /// <summary>
        /// Returns a single bit of information about a single element
        /// </summary>
        /// <param name="elementId">Element ID</param>
        /// <param name="elementStat">Value to obtain: mass, charge, or uncertainty</param>
        /// <returns></returns>
        /// <remarks>Since a value may be negative, simply returns 0 if an error</remarks>
        public double GetElementStatInternal(short elementId, ElementStatsType elementStat)
        {
            if (elementId >= 1 && elementId <= ELEMENT_COUNT)
            {
                return elementStat switch
                {
                    ElementStatsType.Mass => mElementStats[elementId].Mass,
                    ElementStatsType.Charge => mElementStats[elementId].Charge,
                    ElementStatsType.Uncertainty => mElementStats[elementId].Uncertainty,
                    _ => 0d
                };
            }

            return 0d;
        }

        public string GetErrorDescription()
        {
            if (mErrorParams.ErrorId != 0)
            {
                return LookupMessage(mErrorParams.ErrorId);
            }

            return "";
        }

        public int GetErrorId()
        {
            return mErrorParams.ErrorId;
        }

        public string GetErrorCharacter()
        {
            return mErrorParams.ErrorCharacter;
        }

        public int GetErrorPosition()
        {
            return mErrorParams.ErrorPosition;
        }

        public int GetMessageStatementMaxId()
        {
            return mMessageStatements.Keys.Max();
        }

        /// <summary>
        /// Get message text using message ID
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="appendText"></param>
        /// <returns></returns>
        /// <remarks>
        /// GetMessageStringInternal simply returns the message for <paramref name="messageId"/>
        /// LookupMessage formats the message, and possibly combines multiple messages, depending on the message number
        /// </remarks>
        public string GetMessageStatementInternal(int messageId, string appendText = "")
        {
            if (mMessageStatements.TryGetValue(messageId, out var message))
            {
                // Append Prefix to certain strings
                switch (messageId)
                {
                    // Add Error: to the front of certain error codes
                    case var @case when 1 <= @case && @case <= 99:
                    case 120:
                    case 130:
                    case 140:
                    case 260:
                    case 270:
                    case 300:
                        message = GetMessageStatementInternal(350) + ": " + message;
                        break;
                }

                // Now append the text
                return message + appendText;
            }

            return "";
        }

        /// <summary>
        /// Checks for presence of <paramref name="symbolReference"/> in <paramref name="abbrevSymbolStack"/>
        /// If found, returns true
        /// </summary>
        /// <param name="abbrevSymbolStack"></param>
        /// <param name="symbolReference"></param>
        /// <returns></returns>
        private bool IsPresentInAbbrevSymbolStack(AbbrevSymbolStack abbrevSymbolStack, short symbolReference)
        {
            try
            {
                var found = false;
                for (var index = 0; index < abbrevSymbolStack.SymbolReferenceStack.Count; index++)
                {
                    if (abbrevSymbolStack.SymbolReferenceStack[index] == symbolReference)
                    {
                        found = true;
                        break;
                    }
                }

                return found;
            }
            catch (Exception ex)
            {
                GeneralErrorHandler("IsPresentInAbbrevSymbolStack", ex);
                return false;
            }
        }

        /// <summary>
        /// Returns True if the first letter of <paramref name="testChar"/> is a ModSymbol
        /// </summary>
        /// <param name="testChar"></param>
        /// <returns></returns>
        /// <remarks>
        /// Invalid Mod Symbols are letters, numbers, ., -, space, (, or )
        /// Valid Mod Symbols are ! # $ % ampersand ' * + ? ^ ` ~
        /// </remarks>
        public bool IsModSymbolInternal(string testChar)
        {
            bool isModSymbol;

            if (testChar.Length > 0)
            {
                var firstChar = testChar[0];

                switch (Convert.ToInt32(firstChar))
                {
                    case 34: // " is not allowed
                        isModSymbol = false;
                        break;
                    case var @case when 40 <= @case && @case <= 41: // ( and ) are not allowed
                        isModSymbol = false;
                        break;
                    case var case1 when 44 <= case1 && case1 <= 62: // . and - and , and / and numbers and : and ; and < and = and > are not allowed
                        isModSymbol = false;
                        break;
                    case var case2 when 33 <= case2 && case2 <= 43:
                    case var case3 when 63 <= case3 && case3 <= 64:
                    case var case4 when 94 <= case4 && case4 <= 96:
                    case 126:
                        isModSymbol = true;
                        break;
                    default:
                        isModSymbol = false;
                        break;
                }
            }
            else
            {
                isModSymbol = false;
            }

            return isModSymbol;
        }

        /// <summary>
        /// Tests if all of the characters in <paramref name="test"/> are letters
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        private bool IsStringAllLetters(string test)
        {
            return test.All(char.IsLetter);
        }

        public bool IsValidElementSymbol(string elementSymbol, bool caseSensitive = true)
        {
            if (caseSensitive)
            {
                var query = from item in mElementStats where (item.Symbol ?? "") == (elementSymbol ?? "") select item;
                return query.Any();
            }
            else
            {
                var query = from item in mElementStats where (item.Symbol?.ToLower() ?? "") == (elementSymbol?.ToLower() ?? "") select item;
                return query.Any();
            }
        }

        private void LogMessage(string message, MessageType messageType = MessageType.Normal)
        {
            // Note that CleanupFilePaths() will update mOutputFolderPath, which is used here if mLogFolderPath is blank
            // Thus, be sure to call CleanupFilePaths (or update mLogFolderPath) before the first call to LogMessage

            if (mLogFile == null && mLogMessagesToFile)
            {
                try
                {
                    mLogFilePath = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    mLogFilePath += "_log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

                    try
                    {
                        if (mLogFolderPath == null)
                            mLogFolderPath = string.Empty;

                        if (mLogFolderPath.Length > 0)
                        {
                            // Create the log folder if it doesn't exist
                            if (!System.IO.Directory.Exists(mLogFolderPath))
                            {
                                System.IO.Directory.CreateDirectory(mLogFolderPath);
                            }
                        }
                    }
                    catch
                    {
                        mLogFolderPath = string.Empty;
                    }

                    if (mLogFolderPath.Length > 0)
                    {
                        mLogFilePath = System.IO.Path.Combine(mLogFolderPath, mLogFilePath);
                    }

                    var openingExistingFile = System.IO.File.Exists(mLogFilePath);

                    mLogFile = new System.IO.StreamWriter(new System.IO.FileStream(mLogFilePath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read))
                    {
                        AutoFlush = true
                    };

                    if (!openingExistingFile)
                    {
                        mLogFile.WriteLine("Date" + "\t" +
                            "Type" + "\t" +
                            "Message");
                    }
                }
                catch
                {
                    // Error creating the log file; set mLogMessagesToFile to false so we don't repeatedly try to create it
                    mLogMessagesToFile = false;
                }
            }

            string messageTypeText = messageType switch
            {
                MessageType.Normal => "Normal",
                MessageType.Error => "Error",
                MessageType.Warning => "Warning",
                _ => "Unknown"
            };

            if (mLogFile == null)
            {
                Console.WriteLine(messageTypeText + "\t" + message);
            }
            else
            {
                mLogFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt") + "\t" +
                    messageTypeText + "\t" + message);
            }
        }

        private string LookupCautionStatement(string compareText)
        {
            if (mCautionStatements.TryGetValue(compareText, out var message))
            {
                return message;
            }

            return string.Empty;
        }

        /// <summary>
        /// Looks up the message for <paramref name="messageId"/>
        /// Also appends any data in <paramref name="appendText"/> to the message
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="appendText"></param>
        /// <returns>The complete message</returns>
        internal string LookupMessage(int messageId, string appendText = "")
        {
            if (mMessageStatements.Count == 0)
                MemoryLoadMessageStatements();

            // First assume we can't find the message number
            var message = "General unspecified error";

            // Now try to find it
            if (messageId < MESSAGE_STATEMENT_DIM_COUNT)
            {
                if (mMessageStatements[messageId].Length > 0)
                {
                    message = mMessageStatements[messageId];
                }
            }

            // Now prepend Prefix to certain strings
            switch (messageId)
            {
                // Add Error: to the front of certain error codes
                case var @case when 1 <= @case && @case <= 99:
                case 120:
                case 130:
                case 140:
                case 260:
                case 270:
                case 300:
                    {
                        message = LookupMessage(350) + ": " + message;
                        break;
                    }
            }

            // Now append the text
            message += appendText;

            // messageId's 1 and 18 may need to have an addendum added
            if (messageId == 1)
            {
                if (ComputationOptions.CaseConversion == CaseConversionMode.ExactCase)
                {
                    message += " (" + LookupMessage(680) + ")";
                }
            }
            else if (messageId == 18)
            {
                if (!ComputationOptions.BracketsAsParentheses)
                {
                    message += " (" + LookupMessage(685) + ")";
                }
                else
                {
                    message += " (" + LookupMessage(690) + ")";
                }
            }

            return message;
        }

        /// <summary>
        /// Converts <paramref name="massToConvert"/> to ppm, based on the value of <paramref name="currentMz"/>
        /// </summary>
        /// <param name="massToConvert"></param>
        /// <param name="currentMz"></param>
        /// <returns></returns>
        public double MassToPPMInternal(double massToConvert, double currentMz)
        {
            if (currentMz > 0d)
            {
                return massToConvert * 1000000.0d / currentMz;
            }

            return 0d;
        }

        /// <summary>
        /// Convert monoisotopic mass to m/z
        /// </summary>
        /// <param name="monoisotopicMass"></param>
        /// <param name="charge"></param>
        /// <param name="chargeCarrierMass">If this is 0, uses mChargeCarrierMass</param>
        /// <returns></returns>
        public double MonoMassToMzInternal(
            double monoisotopicMass,
            short charge,
            double chargeCarrierMass = 0)
        {
            if (Math.Abs(chargeCarrierMass) < float.Epsilon)
                chargeCarrierMass = mChargeCarrierMass;

            // Call ConvoluteMass to convert to the desired charge state
            return ConvoluteMassInternal(monoisotopicMass + chargeCarrierMass, 1, charge, chargeCarrierMass);
        }

        public void MemoryLoadAll(ElementMassMode elementMode)
        {
            // Make sure this value accurately reflects what's loaded...
            mCurrentElementMode = elementMode;

            // TODO: Consider calling this as part of the ElementAndMassTools constructor
            MemoryLoadElements(elementMode);

            // Reconstruct master symbols list
            // This is needed here to properly load the abbreviations
            ConstructMasterSymbolsList();

            MemoryLoadAbbreviations();

            // Reconstruct master symbols list
            // Needed here to load abbreviations into the list
            ConstructMasterSymbolsList();

            MemoryLoadCautionStatements();

            MemoryLoadMessageStatements();
        }

        public void MemoryLoadAbbreviations()
        {
            mAbbrevStats.Clear();
            // Symbol                            Formula            1 letter abbreviation
            mAbbrevStats.Add(new AbbrevStatsData("Ala", "C3H5NO", 0f, true, "A", "Alanine"));
            mAbbrevStats.Add(new AbbrevStatsData("Arg", "C6H12N4O", 0f, true, "R", "Arginine, (unprotonated NH2)"));
            mAbbrevStats.Add(new AbbrevStatsData("Asn", "C4H6N2O2", 0f, true, "N", "Asparagine"));
            mAbbrevStats.Add(new AbbrevStatsData("Asp", "C4H5NO3", 0f, true, "D", "Aspartic acid (undissociated COOH)"));
            mAbbrevStats.Add(new AbbrevStatsData("Cys", "C3H5NOS", 0f, true, "C", "Cysteine (no disulfide link)"));
            mAbbrevStats.Add(new AbbrevStatsData("Gla", "C6H7NO5", 0f, true, "U", "gamma-Carboxyglutamate"));
            mAbbrevStats.Add(new AbbrevStatsData("Gln", "C5H8N2O2", 0f, true, "Q", "Glutamine"));
            mAbbrevStats.Add(new AbbrevStatsData("Glu", "C5H7NO3", 0f, true, "E", "Glutamic acid (undissociated COOH)"));
            mAbbrevStats.Add(new AbbrevStatsData("Gly", "C2H3NO", 0f, true, "G", "Glycine"));
            mAbbrevStats.Add(new AbbrevStatsData("His", "C6H7N3O", 0f, true, "H", "Histidine (unprotonated NH)"));
            mAbbrevStats.Add(new AbbrevStatsData("Hse", "C4H7NO2", 0f, true, "", "Homoserine"));
            mAbbrevStats.Add(new AbbrevStatsData("Hyl", "C6H12N2O2", 0f, true, "", "Hydroxylysine"));
            mAbbrevStats.Add(new AbbrevStatsData("Hyp", "C5H7NO2", 0f, true, "", "Hydroxyproline"));
            mAbbrevStats.Add(new AbbrevStatsData("Ile", "C6H11NO", 0f, true, "I", "Isoleucine"));
            mAbbrevStats.Add(new AbbrevStatsData("Leu", "C6H11NO", 0f, true, "L", "Leucine"));
            mAbbrevStats.Add(new AbbrevStatsData("Lys", "C6H12N2O", 0f, true, "K", "Lysine (unprotonated NH2)"));
            mAbbrevStats.Add(new AbbrevStatsData("Met", "C5H9NOS", 0f, true, "M", "Methionine"));
            mAbbrevStats.Add(new AbbrevStatsData("Orn", "C5H10N2O", 0f, true, "O", "Ornithine"));
            mAbbrevStats.Add(new AbbrevStatsData("Phe", "C9H9NO", 0f, true, "F", "Phenylalanine"));
            mAbbrevStats.Add(new AbbrevStatsData("Pro", "C5H7NO", 0f, true, "P", "Proline"));
            mAbbrevStats.Add(new AbbrevStatsData("Pyr", "C5H5NO2", 0f, true, "", "Pyroglutamic acid"));
            mAbbrevStats.Add(new AbbrevStatsData("Sar", "C3H5NO", 0f, true, "", "Sarcosine"));
            mAbbrevStats.Add(new AbbrevStatsData("Ser", "C3H5NO2", 0f, true, "S", "Serine"));
            mAbbrevStats.Add(new AbbrevStatsData("Thr", "C4H7NO2", 0f, true, "T", "Threonine"));
            mAbbrevStats.Add(new AbbrevStatsData("Trp", "C11H10N2O", 0f, true, "W", "Tryptophan"));
            mAbbrevStats.Add(new AbbrevStatsData("Tyr", "C9H9NO2", 0f, true, "Y", "Tyrosine"));
            mAbbrevStats.Add(new AbbrevStatsData("Val", "C5H9NO", 0f, true, "V", "Valine"));
            mAbbrevStats.Add(new AbbrevStatsData("Xxx", "C6H12N2O", 0f, true, "X", "Unknown"));

            mAbbrevStats.Add(new AbbrevStatsData("Bpy", "C10H8N2", 0f, false, "", "Bipyridine"));
            mAbbrevStats.Add(new AbbrevStatsData("Bu", "C4H9", 1f, false, "", "Butyl"));
            mAbbrevStats.Add(new AbbrevStatsData("D", "^2.014H", 1f, false, "", "Deuterium"));
            mAbbrevStats.Add(new AbbrevStatsData("En", "C2H8N2", 0f, false, "", "Ethylenediamine"));
            mAbbrevStats.Add(new AbbrevStatsData("Et", "CH3CH2", 1f, false, "", "Ethyl"));
            mAbbrevStats.Add(new AbbrevStatsData("Me", "CH3", 1f, false, "", "Methyl"));
            mAbbrevStats.Add(new AbbrevStatsData("Ms", "CH3SOO", -1, false, "", "Mesyl"));
            mAbbrevStats.Add(new AbbrevStatsData("Oac", "C2H3O2", -1, false, "", "Acetate"));
            mAbbrevStats.Add(new AbbrevStatsData("Otf", "OSO2CF3", -1, false, "", "Triflate"));
            mAbbrevStats.Add(new AbbrevStatsData("Ox", "C2O4", -2, false, "", "Oxalate"));
            mAbbrevStats.Add(new AbbrevStatsData("Ph", "C6H5", 1f, false, "", "Phenyl"));
            mAbbrevStats.Add(new AbbrevStatsData("Phen", "C12H8N2", 0f, false, "", "Phenanthroline"));
            mAbbrevStats.Add(new AbbrevStatsData("Py", "C5H5N", 0f, false, "", "Pyridine"));
            mAbbrevStats.Add(new AbbrevStatsData("Tpp", "(C4H2N(C6H5C)C4H2N(C6H5C))2", 0f, false, "", "Tetraphenylporphyrin"));
            mAbbrevStats.Add(new AbbrevStatsData("Ts", "CH3C6H4SOO", -1, false, "", "Tosyl"));
            mAbbrevStats.Add(new AbbrevStatsData("Urea", "H2NCONH2", 0f, false, "", "Urea"));

            foreach (var stats in mAbbrevStats)
            {
                stats.Mass = ComputeFormulaWeight(stats.Formula);
                if (stats.Mass < 0d)
                {
                    // Error occurred computing mass for abbreviation
                    stats.Mass = 0d;
                    stats.InvalidSymbolOrFormula = true;
                }
            }

            // Note Asx or B is often used for Asp or Asn
            // Note Glx or Z is often used for Glu or Gln
            // Note X is often used for "unknown"
            //
            // Other amino acids without widely agreed upon 1 letter codes
            //
            // FlexGridAddItems .grdAminoAcids, "Aminosuberic Acid", "Asu"     ' A pair of Cys residues bonded by S-S
            // FlexGridAddItems .grdAminoAcids, "Cystine", "Cyn"
            // FlexGridAddItems .grdAminoAcids, "Homocysteine", "Hcy"
            // FlexGridAddItems .grdAminoAcids, "Homoserine", "Hse"            ' 101.04 (C4H7NO2)
            // FlexGridAddItems .grdAminoAcids, "Hydroxylysine", "Hyl"         ' 144.173 (C6H12N2O2)
            // FlexGridAddItems .grdAminoAcids, "Hydroxyproline", "Hyp"        ' 113.116 (C5H7NO2)
            // FlexGridAddItems .grdAminoAcids, "Norleucine", "Nle"            ' 113.06
            // FlexGridAddItems .grdAminoAcids, "Norvaline", "Nva"
            // FlexGridAddItems .grdAminoAcids, "Pencillamine", "Pen"
            // FlexGridAddItems .grdAminoAcids, "Phosphoserine", "Sep"
            // FlexGridAddItems .grdAminoAcids, "Phosphothreonine", "Thp"
            // FlexGridAddItems .grdAminoAcids, "Phosphotyrosine", "Typ"
            // FlexGridAddItems .grdAminoAcids, "Pyroglutamic Acid", "Pyr"     ' 111.03 (C5H5NO2) (also Glp in some tables)
            // FlexGridAddItems .grdAminoAcids, "Sarcosine", "Sar"             ' 71.08 (C3H5NO)
            // FlexGridAddItems .grdAminoAcids, "Statine", "Sta"
            // FlexGridAddItems .grdAminoAcids, "b-[2-Thienyl]Ala", "Thi"

            // Need to explore http://www.abrf.org/ABRF/ResearchCommittees/deltamass/deltamass.html

            // Isoelectric points
            // TYR   Y   C9H9NO2     163.06333  163.1760      0               9.8
            // HIS   H   C6H7N3O     137.05891  137.1411      1               6.8
            // LYS   K   C6H12N2O    128.09496  128.1741      1              10.1
            // ASP   D   C4H5NO3     115.02694  115.0886      1               4.5
            // GLU   E   C5H7NO3     129.04259  129.1155      1               4.5
            // CYS   C   C3H5NOS     103.00919  103.1388      0               8.6
            // ARG   R   C6H12N4O    156.10111  156.1875      1              12.0
        }

        /// <summary>
        /// Define the caution statements
        /// </summary>
        /// <remarks>Use ClearCautionStatements and AddCautionStatement to set these based on language</remarks>
        public void MemoryLoadCautionStatements()
        {
            ElementAndMassInMemoryData.MemoryLoadCautionStatementsEnglish(mCautionStatements);
        }

        /// <summary>
        /// Load elements and isotopes
        /// </summary>
        /// <param name="elementMode">Element mode: 1 for average weights, 2 for monoisotopic weights, 3 for integer weights</param>
        /// <param name="specificElement"></param>
        /// <param name="specificStatToReset"></param>
        /// <remarks>
        /// <paramref name="specificElement"/> and <paramref name="specificStatToReset"/> are zero when updating all of the elements and isotopes.
        /// nonzero <paramref name="specificElement"/> and <paramref name="specificStatToReset"/> values will set just that specific value to the default
        /// </remarks>
        public void MemoryLoadElements(
            ElementMassMode elementMode = ElementMassMode.Average,
            short specificElement = 0,
            ElementStatsType specificStatToReset = ElementStatsType.Mass)
        {
            const double defaultChargeCarrierMassAvg = 1.00739d;
            const double defaultChargeCarrierMassMonoiso = 1.00727649d;

            // Data Load Statements
            // Uncertainties from CRC Handbook of Chemistry and Physics
            // For Radioactive elements, the most stable isotope is NOT used;
            // instead, an average Mol. Weight is used, just like with other elements.
            // Data obtained from the Perma-Chart Science Series periodic table, 1993.
            // Uncertainties from CRC Handbook of Chemistry and Physics, except for
            // Radioactive elements, where uncertainty was estimated to be .n5 where
            // specificElementProperty represents the number digits after the decimal point but before the last
            // number of the molecular weight.
            // For example, for No, MW = 259.1009 (±0.0005)

            // Define the charge carrier mass
            if (elementMode == ElementMassMode.Average)
            {
                SetChargeCarrierMassInternal(defaultChargeCarrierMassAvg);
            }
            else
            {
                SetChargeCarrierMassInternal(defaultChargeCarrierMassMonoiso);
            }

            // elementNames stores the element names
            // elemVals[elementIndex,1] stores the element's weight
            // elemVals[elementIndex,2] stores the element's uncertainty
            // elemVals[elementIndex,3] stores the element's charge
            // Note: We could make this array of type ElementInfo, but the size of this method would increase dramatically
            var elementMemoryData = ElementAndMassInMemoryData.MemoryLoadElements();

            // Set uncertainty to 0 for all elements if using exact isotopic or integer isotopic weights
            // Reduce branching - use Func<> to get the correct value based on the settings
            Func<ElementMem, double> getMass;
            Func<ElementMem, double> getUncertainty;
            switch (elementMode)
            {
                case ElementMassMode.Integer:
                    getMass = elementMem => elementMem.MassInteger;
                    getUncertainty = elementMem => 0;
                    break;
                case ElementMassMode.Isotopic:
                    getMass = elementMem => elementMem.MassIsotopic;
                    getUncertainty = elementMem => 0;
                    break;
                case ElementMassMode.Average:
                default:
                    getMass = elementMem => elementMem.MassAverage;
                    getUncertainty = elementMem => elementMem.UncertaintyAverageMass;
                    break;
            }

            // Set uncertainty to 0 for all elements if using exact isotopic or integer isotopic weights
            if (specificElement == 0)
            {
                // Updating all the elements
                mElementAlph.Clear();
                for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                {
                    var elementMem = elementMemoryData[elementIndex];
                    mElementStats[elementIndex] = new ElementInfo(elementMem.Symbol, elementMem.Charge, getMass(elementMem), getUncertainty(elementMem));

                    mElementAlph.Add(new KeyValuePair<string, int>(elementMem.Symbol, elementIndex));
                }

                // Alphabetize mElementAlph by Key/symbol
                mElementAlph.Sort((x,y) => string.Compare(x.Key, y.Key, StringComparison.Ordinal));

                // Also load the isotopes, since if any were loaded we just cleared them.
                ElementAndMassInMemoryData.MemoryLoadIsotopes(mElementStats);
            }
            else if (specificElement >= 1 && specificElement <= ELEMENT_COUNT)
            {
                var stats = mElementStats[specificElement];
                switch (specificStatToReset)
                {
                    case ElementStatsType.Mass:
                        stats.Mass = getMass(elementMemoryData[specificElement]);
                        break;
                    case ElementStatsType.Uncertainty:
                        stats.Uncertainty = getUncertainty(elementMemoryData[specificElement]);
                        break;
                    case ElementStatsType.Charge:
                        stats.Charge = elementMemoryData[specificElement].Charge;
                        break;
                    default:
                        // Ignore it
                        break;
                }
            }
        }

        public void MemoryLoadMessageStatements()
        {
            ElementAndMassInMemoryData.MemoryLoadMessageStatementsEnglish(mMessageStatements);
        }

        private void MwtWinDllErrorHandler(string callingProcedure, Exception ex)
        {
            string message;

            if (ex is OverflowException)
            {
                message = LookupMessage(590);
                if (ShowErrorMessageDialogs)
                {
                    MessageBox.Show(LookupMessage(590), LookupMessage(350), MessageBoxButtons.OK);
                }

                LogMessage(message, MessageType.Error);
            }
            else
            {
                message = LookupMessage(600) + ": " + ex.Message + Environment.NewLine + " (" + callingProcedure + " handler)";
                message += Environment.NewLine + LookupMessage(605);

                if (ShowErrorMessageDialogs)
                {
                    MessageBox.Show(message, LookupMessage(350), MessageBoxButtons.OK);
                }

                // Call GeneralErrorHandler so that the error gets logged to ErrorLog.txt
                // Note that GeneralErrorHandler will call LogMessage

                // Make sure mShowErrorMessageDialogs is false when calling GeneralErrorHandler

                var showErrorMessageDialogsSaved = ShowErrorMessageDialogs;
                ShowErrorMessageDialogs = false;

                GeneralErrorHandler(callingProcedure, ex);

                ShowErrorMessageDialogs = showErrorMessageDialogsSaved;
            }
        }

        /// <summary>
        /// Determines the molecular weight and elemental composition of <paramref name="formula"/>
        /// </summary>
        /// <param name="formula">Input/output: formula to parse</param>
        /// <param name="computationStats">Output: additional information about the formula</param>
        /// <param name="expandAbbreviations"></param>
        /// <param name="valueForX"></param>
        /// <returns>Computed molecular weight if no error; otherwise -1</returns>
        /// <remarks>
        /// ErrorParams will hold information on errors that occur (previous errors are cleared when this function is called)
        /// Use ComputeFormulaWeight if you only want to know the weight of a formula (it calls this function)
        /// </remarks>
        public double ParseFormulaPublic(
            ref string formula,
            ComputationStats computationStats,
            bool expandAbbreviations = false,
            double valueForX = 1)
        {
            var abbrevSymbolStack = new AbbrevSymbolStack();

            try
            {
                var stdDevSum = 0.0d;

                // Reset ErrorParams to clear any prior errors
                ResetErrorParamsInternal();

                // Reset Caution Description
                mCautionDescription = "";

                if (formula.Length > 0)
                {
                    formula = ParseFormulaRecursive(formula, computationStats, abbrevSymbolStack, expandAbbreviations, out stdDevSum, out _, valueForX);
                }

                // Copy computationStats to mComputationStatsSaved
                mComputationStatsSaved = computationStats.Clone();

                if (mErrorParams.ErrorId == 0)
                {

                    // Compute the standard deviation
                    computationStats.StandardDeviation = Math.Sqrt(stdDevSum);

                    // Compute the total molecular weight
                    computationStats.TotalMass = 0d; // Reset total weight of compound to 0 so we can add to it
                    for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                        // Increase total weight by multiplying the count of each element by the element's mass
                        // In addition, add in the Isotopic Correction value
                        computationStats.TotalMass = computationStats.TotalMass + mElementStats[elementIndex].Mass * computationStats.Elements[elementIndex].Count + computationStats.Elements[elementIndex].IsotopicCorrection;

                    return computationStats.TotalMass;
                }

                return -1;
            }
            catch (Exception ex)
            {
                GeneralErrorHandler("ElementAndMassTools.ParseFormulaPublic", ex);
                return -1;
            }
        }

        /// <summary>
        /// Determine elements in an abbreviation or elements and abbreviations in a formula
        /// Stores results in <paramref name="computationStats"/>
        /// ErrorParams will hold information on errors that occur
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="computationStats"></param>
        /// <param name="abbrevSymbolStack"></param>
        /// <param name="expandAbbreviations"></param>
        /// <param name="stdDevSum">Sum of the squares of the standard deviations</param>
        /// <param name="carbonOrSiliconReturnCount">Tracks the number of carbon and silicon atoms found; used when correcting for charge inside parentheses or inside an abbreviation</param>
        /// <param name="valueForX"></param>
        /// <param name="charCountPrior"></param>
        /// <param name="parenthMultiplier">The value to multiply all values by if inside parentheses</param>
        /// <param name="dashMultiplierPrior"></param>
        /// <param name="bracketMultiplierPrior"></param>
        /// <param name="parenthLevelPrevious"></param>
        /// <returns>Formatted formula</returns>
        private string ParseFormulaRecursive(
            string formula,
            ComputationStats computationStats,
            AbbrevSymbolStack abbrevSymbolStack,
            bool expandAbbreviations,
            out double stdDevSum,
            out int carbonOrSiliconReturnCount,
            double valueForX = 1.0d,
            int charCountPrior = 0,
            double parenthMultiplier = 1.0d,
            double dashMultiplierPrior = 1.0d,
            double bracketMultiplierPrior = 1.0d,
            short parenthLevelPrevious = 0)
        {
            // ( and ) are 40 and 41   - is 45   { and } are 123 and 125
            // Numbers are 48 to 57    . is 46
            // Uppercase letters are 65 to 90
            // Lowercase letters are 97 to 122
            // [ and ] are 91 and 93
            // ^ is 94
            // is 95

            int symbolLength = default;
            var caretPresent = default(bool);

            double caretVal = default;
            var char1 = string.Empty;

            short prevSymbolReference = default;
            int parenthLevel = default;

            stdDevSum = 0;
            carbonOrSiliconReturnCount = 0;

            try
            {
                int charIndex;
                var dashMultiplier = dashMultiplierPrior;
                var bracketMultiplier = bracketMultiplierPrior;
                var insideBrackets = false;

                var dashPos = -1;
                var newFormula = string.Empty;
                var newFormulaRightHalf = string.Empty;

                var loneCarbonOrSilicon = 0;

                // Look for the > symbol
                // If found, this means take First Part minus the Second Part
                var minusSymbolLoc = formula.IndexOf(">", StringComparison.Ordinal);
                if (minusSymbolLoc >= 0)
                {
                    var computationStatsRightHalf = new ComputationStats();
                    // Look for the first occurrence of >
                    charIndex = 0;
                    var matchFound = false;
                    do
                    {
                        if (formula.Substring(charIndex, 1) == ">")
                        {
                            matchFound = true;
                            var leftHalf = formula.Substring(0, Math.Min(formula.Length, charIndex));
                            var rightHalf = formula.Substring(charIndex + 1);

                            // Parse the first half
                            newFormula = ParseFormulaRecursive(leftHalf, computationStats, abbrevSymbolStack, expandAbbreviations, out var leftStdDevSum, out _, valueForX, charCountPrior, parenthMultiplier, dashMultiplier, bracketMultiplier, parenthLevelPrevious);
                            stdDevSum += leftStdDevSum;

                            // Parse the second half
                            var abbrevSymbolStackRightHalf = new AbbrevSymbolStack();
                            newFormulaRightHalf = ParseFormulaRecursive(rightHalf, computationStatsRightHalf, abbrevSymbolStackRightHalf, expandAbbreviations, out _, out _, valueForX, charCountPrior + charIndex, parenthMultiplier, dashMultiplier, bracketMultiplier, parenthLevelPrevious);
                            break;
                        }

                        charIndex++;
                    }
                    while (charIndex < formula.Length);

                    if (matchFound)
                    {
                        // Update formula
                        formula = newFormula + ">" + newFormulaRightHalf;

                        // Update computationStats by subtracting the atom counts of the first half minus the second half
                        // If any atom counts become < 0 then, then raise an error
                        for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                        {
                            var element = computationStats.Elements[elementIndex];
                            if (mElementStats[elementIndex].Mass * element.Count + element.IsotopicCorrection >= mElementStats[elementIndex].Mass * computationStatsRightHalf.Elements[elementIndex].Count + computationStatsRightHalf.Elements[elementIndex].IsotopicCorrection)
                            {
                                element.Count -= -computationStatsRightHalf.Elements[elementIndex].Count;
                                if (element.Count < 0d)
                                {
                                    // This shouldn't happen
                                    Console.WriteLine(".Count is less than 0 in ParseFormulaRecursive; this shouldn't happen");
                                    element.Count = 0d;
                                }

                                if (Math.Abs(computationStatsRightHalf.Elements[elementIndex].IsotopicCorrection) > float.Epsilon)
                                {
                                    element.IsotopicCorrection -= computationStatsRightHalf.Elements[elementIndex].IsotopicCorrection;
                                }
                            }
                            else
                            {
                                // Invalid Formula; raise error
                                mErrorParams.ErrorId = 30;
                                mErrorParams.ErrorPosition = charIndex;
                            }

                            if (mErrorParams.ErrorId != 0)
                                break;
                        }

                        // Adjust the overall charge
                        computationStats.Charge -= computationStatsRightHalf.Charge;
                    }
                }
                else
                {
                    // Formula does not contain >
                    // Parse it
                    charIndex = 0;
                    do
                    {
                        char1 = formula.Substring(charIndex, 1);
                        var char2 = charIndex + 1 < formula.Length ? formula.Substring(charIndex + 1, 1) : "";
                        var char3 = charIndex + 2 < formula.Length ? formula.Substring(charIndex + 2, 1) : "";
                        var charRemain = charIndex + 3 < formula.Length ? formula.Substring(charIndex + 3) : "";
                        if (ComputationOptions.CaseConversion != CaseConversionMode.ExactCase)
                            char1 = char1.ToUpper();

                        if (ComputationOptions.BracketsAsParentheses)
                        {
                            if (char1 == "[")
                                char1 = "(";
                            if (char1 == "]")
                                char1 = ")";
                        }

                        if (string.IsNullOrEmpty(char1))
                            char1 = EMPTY_STRING_CHAR.ToString();
                        if (string.IsNullOrEmpty(char2))
                            char2 = EMPTY_STRING_CHAR.ToString();
                        if (string.IsNullOrEmpty(char3))
                            char3 = EMPTY_STRING_CHAR.ToString();
                        if (string.IsNullOrEmpty(charRemain))
                            charRemain = EMPTY_STRING_CHAR.ToString();

                        var formulaExcerpt = char1 + char2 + char3 + charRemain;

                        // Check for needed caution statements
                        CheckCaution(formulaExcerpt);

                        int numLength;
                        double adjacentNum;
                        int addonCount;
                        switch ((int)char1[0])
                        {
                            case 40:
                            case 123: // (    Record its position
                                // See if a number is present just after the opening parenthesis
                                if (char.IsDigit(char2[0]) || char2 == ".")
                                {
                                    // Misplaced number
                                    mErrorParams.ErrorId = 14;
                                    mErrorParams.ErrorPosition = charIndex;
                                }

                                if (mErrorParams.ErrorId == 0)
                                {
                                    // search for closing parenthesis
                                    parenthLevel = 1;
                                    for (var parenthClose = charIndex + 1; parenthClose < formula.Length; parenthClose++)
                                    {
                                        switch (formula.Substring(parenthClose, 1) ?? "")
                                        {
                                            case "(":
                                            case "{":
                                            case "[":
                                                // Another opening parentheses
                                                // increment parenthLevel
                                                if (!ComputationOptions.BracketsAsParentheses && formula.Substring(parenthClose, 1) == "[")
                                                {
                                                    // Do not count the bracket
                                                }
                                                else
                                                {
                                                    parenthLevel += 1;
                                                }

                                                break;

                                            case ")":
                                            case "}":
                                            case "]":
                                                if (!ComputationOptions.BracketsAsParentheses && formula.Substring(parenthClose, 1) == "]")
                                                {
                                                    // Do not count the bracket
                                                }
                                                else
                                                {
                                                    parenthLevel -= 1;
                                                    if (parenthLevel == 0)
                                                    {
                                                        adjacentNum = ParseNum(formula.Substring(parenthClose + 1), out numLength);
                                                        CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                                        if (adjacentNum < 0d)
                                                        {
                                                            adjacentNum = 1.0d;
                                                            addonCount = 0;
                                                        }
                                                        else
                                                        {
                                                            addonCount = numLength;
                                                        }

                                                        var subFormula = formula.Substring(charIndex + 1, parenthClose - (charIndex + 1));

                                                        // Note, must pass parenthMultiplier * adjacentNum to preserve previous parentheses stuff
                                                        newFormula = ParseFormulaRecursive(subFormula, computationStats, abbrevSymbolStack, expandAbbreviations, out var newStdDevSum, out var carbonSiliconCount, valueForX, charCountPrior + charIndex, parenthMultiplier * adjacentNum, dashMultiplier, bracketMultiplier, (short)(parenthLevelPrevious + 1));
                                                        stdDevSum += newStdDevSum;

                                                        // If expanding abbreviations, then newFormula might be longer than formula, must add this onto charIndex also
                                                        var expandAbbrevAdd = newFormula.Length - subFormula.Length;

                                                        // Must replace the part of the formula parsed with the newFormula part, in case the formula was expanded or elements were capitalized
                                                        formula = formula.Substring(0, charIndex + 1) + newFormula + formula.Substring(parenthClose);
                                                        charIndex = parenthClose + addonCount + expandAbbrevAdd;

                                                        // Correct charge
                                                        if (carbonSiliconCount > 0)
                                                        {
                                                            computationStats.Charge = (float)(computationStats.Charge - 2d * adjacentNum);
                                                            if (adjacentNum > 1d && carbonSiliconCount > 1)
                                                            {
                                                                computationStats.Charge = (float)(computationStats.Charge - 2d * (adjacentNum - 1d) * (carbonSiliconCount - 1));
                                                            }
                                                        }

                                                        // exit the loop;
                                                        parenthClose = formula.Length;
                                                    }
                                                }

                                                break;
                                        }
                                    }
                                }

                                if (parenthLevel > 0 && mErrorParams.ErrorId == 0)
                                {
                                    // Missing closing parenthesis
                                    mErrorParams.ErrorId = 3;
                                    mErrorParams.ErrorPosition = charIndex;
                                }

                                prevSymbolReference = 0;
                                break;

                            case 41:
                            case 125: // )    Repeat a section of a formula
                                // Should have been skipped
                                // Unmatched closing parentheses
                                mErrorParams.ErrorId = 4;
                                mErrorParams.ErrorPosition = charIndex;
                                break;

                            case 45: // -
                                // Used to denote a leading coefficient
                                adjacentNum = ParseNum(char2 + char3 + charRemain, out numLength);
                                CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                if (adjacentNum > 0d)
                                {
                                    dashPos = charIndex + numLength;
                                    dashMultiplier = adjacentNum * dashMultiplierPrior;
                                    charIndex += numLength;
                                }
                                else if (Math.Abs(adjacentNum) < float.Epsilon)
                                {
                                    // Cannot have 0 after a dash
                                    mErrorParams.ErrorId = 5;
                                    mErrorParams.ErrorPosition = charIndex + 1;
                                }
                                else
                                {
                                    // No number is present, that's just fine
                                    // Make sure defaults are set, though
                                    dashPos = -1;
                                    dashMultiplier = dashMultiplierPrior;
                                }

                                prevSymbolReference = 0;
                                break;

                            case 44:
                            case 46:
                            case var @case when 48 <= @case && @case <= 57: // . or , and Numbers 0 to 9
                                // They should only be encountered as a leading coefficient
                                // Should have been bypassed when the coefficient was processed
                                if (charIndex == 0)
                                {
                                    // Formula starts with a number -- multiply section by number (until next dash)
                                    adjacentNum = ParseNum(formulaExcerpt, out numLength);
                                    CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                    if (adjacentNum >= 0d)
                                    {
                                        dashPos = charIndex + numLength - 1;
                                        dashMultiplier = adjacentNum * dashMultiplierPrior;
                                        charIndex += numLength - 1;
                                    }
                                    else
                                    {
                                        // A number less then zero should have been handled by CatchParseNumError above
                                        // Make sure defaults are set, though
                                        dashPos = -1;
                                        dashMultiplier = dashMultiplierPrior;
                                    }
                                }
                                else if (NumberConverter.CDblSafe(formula.Substring(charIndex - 1, 1)) > 0d)
                                {
                                    // Number too large
                                    mErrorParams.ErrorId = 7;
                                    mErrorParams.ErrorPosition = charIndex;
                                }
                                else
                                {
                                    // Misplaced number
                                    mErrorParams.ErrorId = 14;
                                    mErrorParams.ErrorPosition = charIndex;
                                }

                                prevSymbolReference = 0;
                                break;

                            case 91: // [
                                if (char2.ToUpper() == "X")
                                {
                                    if (char3 == "e")
                                    {
                                        adjacentNum = ParseNum(char2 + char3 + charRemain, out numLength);
                                        CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);
                                    }
                                    else
                                    {
                                        adjacentNum = valueForX;
                                        numLength = 1;
                                    }
                                }
                                else
                                {
                                    adjacentNum = ParseNum(char2 + char3 + charRemain, out numLength);
                                    CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);
                                }

                                if (mErrorParams.ErrorId == 0)
                                {
                                    if (insideBrackets)
                                    {
                                        // No Nested brackets.
                                        mErrorParams.ErrorId = 16;
                                        mErrorParams.ErrorPosition = charIndex;
                                    }
                                    else if (adjacentNum < 0d)
                                    {
                                        // No number after bracket
                                        mErrorParams.ErrorId = 12;
                                        mErrorParams.ErrorPosition = charIndex + 1;
                                    }
                                    else
                                    {
                                        // Coefficient for bracketed section.
                                        insideBrackets = true;
                                        bracketMultiplier = adjacentNum * bracketMultiplierPrior; // Take times bracketMultiplierPrior in case it wasn't 1 to start with
                                        charIndex += numLength;
                                    }
                                }

                                prevSymbolReference = 0;
                                break;

                            case 93: // ]
                                adjacentNum = ParseNum(char2 + char3 + charRemain, out numLength);
                                CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                if (adjacentNum >= 0d)
                                {
                                    // Number following bracket
                                    mErrorParams.ErrorId = 11;
                                    mErrorParams.ErrorPosition = charIndex + 1;
                                }
                                else if (insideBrackets)
                                {
                                    if (dashPos >= 0)
                                    {
                                        // Need to set dashPos and dashMultiplier back to defaults, since a dash number goes back to one inside brackets
                                        dashPos = -1;
                                        dashMultiplier = 1d;
                                    }

                                    insideBrackets = false;
                                    bracketMultiplier = bracketMultiplierPrior;
                                }
                                else
                                {
                                    // Unmatched bracket
                                    mErrorParams.ErrorId = 15;
                                    mErrorParams.ErrorPosition = charIndex;
                                }

                                break;

                            case var case1 when 65 <= case1 && case1 <= 90:
                            case var case2 when 97 <= case2 && case2 <= 122:
                            case 43:
                            case 95: // Uppercase A to Z and lowercase a to z, and the plus (+) sign, and the underscore (_)
                                addonCount = 0;
                                adjacentNum = 0d;

                                var symbolMatchType = CheckElemAndAbbrev(formulaExcerpt, out var symbolReference);

                                switch (symbolMatchType)
                                {
                                    case SymbolMatchMode.Element:
                                        // Found an element
                                        // SymbolReference is the elemental number
                                        symbolLength = mElementStats[symbolReference].Symbol.Length;
                                        if (symbolLength == 0)
                                        {
                                            // No elements in ElementStats yet
                                            // Set symbolLength to 1
                                            symbolLength = 1;
                                        }
                                        // Look for number after element
                                        adjacentNum = ParseNum(formula.Substring(charIndex + symbolLength), out numLength);
                                        CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                        if (adjacentNum < 0d)
                                        {
                                            adjacentNum = 1d;
                                        }

                                        // Note that numLength = 0 if adjacentNum was -1 or otherwise < 0
                                        addonCount = numLength + symbolLength - 1;

                                        if (Math.Abs(adjacentNum) < float.Epsilon)
                                        {
                                            // Zero after element
                                            mErrorParams.ErrorId = 5;
                                            mErrorParams.ErrorPosition = charIndex + symbolLength;
                                        }
                                        else
                                        {
                                            double atomCountToAdd;
                                            if (!caretPresent)
                                            {
                                                atomCountToAdd = adjacentNum * bracketMultiplier * parenthMultiplier * dashMultiplier;
                                                var element = computationStats.Elements[symbolReference];
                                                element.Count += atomCountToAdd;
                                                element.Used = true; // Element is present tag
                                                stdDevSum += atomCountToAdd * Math.Pow(mElementStats[symbolReference].Uncertainty, 2d);

                                                var compStats = computationStats;
                                                // Compute charge
                                                if (symbolReference == 1)
                                                {
                                                    // Dealing with hydrogen
                                                    switch (prevSymbolReference)
                                                    {
                                                        case 1:
                                                        case var case3 when 3 <= case3 && case3 <= 6:
                                                        case var case4 when 11 <= case4 && case4 <= 14:
                                                        case var case5 when 19 <= case5 && case5 <= 32:
                                                        case var case6 when 37 <= case6 && case6 <= 50:
                                                        case var case7 when 55 <= case7 && case7 <= 82:
                                                        case var case8 when 87 <= case8 && case8 <= 109:
                                                            // Hydrogen is -1 with metals (non-halides)
                                                            compStats.Charge = (float)(compStats.Charge + atomCountToAdd * -1);
                                                            break;
                                                        default:
                                                            compStats.Charge = (float)(compStats.Charge + atomCountToAdd * mElementStats[symbolReference].Charge);
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    compStats.Charge = (float)(compStats.Charge + atomCountToAdd * mElementStats[symbolReference].Charge);
                                                }

                                                if (symbolReference == 6 || symbolReference == 14)
                                                {
                                                    // Sum up number lone C and Si (not in abbreviations)
                                                    loneCarbonOrSilicon = (int)Math.Round(loneCarbonOrSilicon + adjacentNum);
                                                }
                                            }
                                            else
                                            {
                                                //caretPresent = true;
                                                // Check to make sure isotopic mass is reasonable
                                                double isoDifferenceTop = NumberConverter.CIntSafe(0.63d * symbolReference + 6d);
                                                double isoDifferenceBottom = NumberConverter.CIntSafe(0.008d * Math.Pow(symbolReference, 2d) - 0.4d * symbolReference - 6d);
                                                var caretValDifference = caretVal - symbolReference * 2;

                                                if (caretValDifference >= isoDifferenceTop)
                                                {
                                                    // Probably too high isotopic mass
                                                    AddToCautionDescription(LookupMessage(660) + ": " + mElementStats[symbolReference].Symbol + " - " + caretVal + " " + LookupMessage(665) + " " + mElementStats[symbolReference].Mass);
                                                }
                                                else if (caretVal < symbolReference)
                                                {
                                                    // Definitely too low isotopic mass
                                                    AddToCautionDescription(LookupMessage(670) + ": " + mElementStats[symbolReference].Symbol + " - " + symbolReference + " " + LookupMessage(675));
                                                }
                                                else if (caretValDifference <= isoDifferenceBottom)
                                                {
                                                    // Probably too low isotopic mass
                                                    AddToCautionDescription(LookupMessage(662) + ": " + mElementStats[symbolReference].Symbol + " - " + caretVal + " " + LookupMessage(665) + " " + mElementStats[symbolReference].Mass);
                                                }

                                                // Put in isotopic correction factor
                                                atomCountToAdd = adjacentNum * bracketMultiplier * parenthMultiplier * dashMultiplier;
                                                var element = computationStats.Elements[symbolReference];
                                                // Increment element counting bin
                                                element.Count += atomCountToAdd;

                                                // Store information in .Isotopes[]
                                                // Increment the isotope counting bin
                                                var isotope = new IsotopicAtomInfo();
                                                element.Isotopes.Add(isotope);
                                                isotope.Count += atomCountToAdd;
                                                isotope.Mass = caretVal;

                                                // Add correction amount to computationStats.Elements[SymbolReference].IsotopicCorrection
                                                element.IsotopicCorrection += (caretVal * atomCountToAdd - mElementStats[symbolReference].Mass * atomCountToAdd);

                                                // Set bit that element is present
                                                element.Used = true;

                                                // Assume no error in caret value, no need to change stdDevSum

                                                // Reset caretPresent
                                                caretPresent = false;
                                            }

                                            if (ComputationOptions.CaseConversion == CaseConversionMode.ConvertCaseUp)
                                            {
                                                formula = formula.Substring(0, charIndex) + formula.Substring(charIndex, 1).ToUpper() + formula.Substring(charIndex + 1);
                                            }

                                            charIndex += addonCount;
                                        }

                                        break;

                                    case SymbolMatchMode.Abbreviation:
                                        // Found an abbreviation or amino acid
                                        // SymbolReference is the abbrev or amino acid number

                                        if (IsPresentInAbbrevSymbolStack(abbrevSymbolStack, symbolReference))
                                        {
                                            // Circular Reference: Can't have an abbreviation referencing an abbreviation that depends upon it
                                            // For example, the following is impossible:  Lor = C6H5Tal and Tal = H4O2Lor
                                            // Furthermore, can't have this either: Lor = C6H5Tal and Tal = H4O2Vin and Vin = S3Lor
                                            mErrorParams.ErrorId = 28;
                                            mErrorParams.ErrorPosition = charIndex;
                                        }
                                        // Found an abbreviation
                                        else if (caretPresent)
                                        {
                                            // Cannot have isotopic mass for an abbreviation, including deuterium
                                            if (char1.ToUpper() == "D" && char2 != "y")
                                            {
                                                // Isotopic mass used for Deuterium
                                                mErrorParams.ErrorId = 26;
                                                mErrorParams.ErrorPosition = charIndex;
                                            }
                                            else
                                            {
                                                mErrorParams.ErrorId = 24;
                                                mErrorParams.ErrorPosition = charIndex;
                                            }
                                        }
                                        else
                                        {
                                            // Parse abbreviation
                                            // Simply treat it like a formula surrounded by parentheses
                                            // Thus, find the number after the abbreviation, then call ParseFormulaRecursive, sending it the formula for the abbreviation
                                            // Update the abbrevSymbolStack before calling so that we can check for circular abbreviation references

                                            // Record the abbreviation length
                                            symbolLength = mAbbrevStats[symbolReference].Symbol.Length;

                                            // Look for number after abbrev/amino
                                            adjacentNum = ParseNum(formula.Substring(charIndex + symbolLength), out numLength);
                                            CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                            if (adjacentNum < 0d)
                                            {
                                                adjacentNum = 1d;
                                                addonCount = symbolLength - 1;
                                            }
                                            else
                                            {
                                                addonCount = numLength + symbolLength - 1;
                                            }

                                            // Add this abbreviation symbol to the Abbreviation Symbol Stack
                                            abbrevSymbolStack.Add(symbolReference);

                                            // Compute the charge prior to calling ParseFormulaRecursive
                                            // During the call to ParseFormulaRecursive, computationStats.Charge will be
                                            // modified according to the atoms in the abbreviation's formula
                                            // This is not what we want; instead, we want to use the defined charge for the abbreviation
                                            // We'll use the atomCountToAdd variable here, though instead of an atom count, it's really an abbreviation occurrence count
                                            var atomCountToAdd = adjacentNum * bracketMultiplier * parenthMultiplier * dashMultiplier;
                                            var chargeSaved = (float)(computationStats.Charge + atomCountToAdd * mAbbrevStats[symbolReference].Charge);

                                            // When parsing an abbreviation, do not pass on the value of expandAbbreviations
                                            // This way, an abbreviation containing an abbreviation will only get expanded one level
                                            ParseFormulaRecursive(mAbbrevStats[symbolReference].Formula, computationStats, abbrevSymbolStack, false, out var abbrevStdDevSum, out _, valueForX, charCountPrior + charIndex, parenthMultiplier * adjacentNum, dashMultiplier, bracketMultiplier, parenthLevelPrevious);
                                            stdDevSum += abbrevStdDevSum;

                                            // Update the charge to chargeSaved
                                            computationStats.Charge = chargeSaved;

                                            // Remove this symbol from the Abbreviation Symbol Stack
                                            abbrevSymbolStack.RemoveMostRecent();

                                            if (mErrorParams.ErrorId == 0)
                                            {
                                                if (expandAbbreviations)
                                                {
                                                    // Replace abbreviation with empirical formula
                                                    var replace = mAbbrevStats[symbolReference].Formula;

                                                    // Look for a number after the abbreviation or amino acid
                                                    adjacentNum = ParseNum(formula.Substring(charIndex + symbolLength), out numLength);
                                                    CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                                    if (replace.IndexOf(">", StringComparison.Ordinal) >= 0)
                                                    {
                                                        // The > symbol means take First Part minus the Second Part
                                                        // If we are parsing a sub formula inside parentheses, or if there are
                                                        // symbols (elements, abbreviations, or numbers) after the abbreviation, then
                                                        // we cannot let the > symbol remain in the abbreviation
                                                        // For example, if Jk = C6H5Cl2>HCl
                                                        // and the user enters Jk2 then chooses Expand Abbreviations
                                                        // Then, naively we might replace this with (C6H5Cl2>HCl)2
                                                        // However, this will generate an error because (C6H5Cl2>HCl)2 gets split
                                                        // to (C6H5Cl2 and HCl)2 which will both generate an error
                                                        // The only option is to convert the abbreviation to its empirical formula
                                                        if (parenthLevelPrevious > 0 || parenthLevel > 0 || charIndex + symbolLength <= formula.Length)
                                                        {
                                                            replace = ConvertFormulaToEmpirical(replace);
                                                        }
                                                    }

                                                    if (adjacentNum < 0d)
                                                    {
                                                        // No number after abbreviation
                                                        formula = formula.Substring(0, charIndex) + replace + formula.Substring(charIndex + symbolLength);
                                                        symbolLength = replace.Length;
                                                        adjacentNum = 1d;
                                                        addonCount = symbolLength - 1;
                                                    }
                                                    else
                                                    {
                                                        // Number after abbreviation -- must put abbreviation in parentheses
                                                        // Parentheses can handle integer or decimal number
                                                        replace = "(" + replace + ")";
                                                        formula = formula.Substring(0, charIndex) + replace + formula.Substring(charIndex + symbolLength);
                                                        symbolLength = replace.Length;
                                                        addonCount = numLength + symbolLength - 1;
                                                    }
                                                }

                                                if (ComputationOptions.CaseConversion == CaseConversionMode.ConvertCaseUp)
                                                {
                                                    formula = formula.Substring(0, charIndex) + formula.Substring(charIndex, 1).ToUpper() + formula.Substring(charIndex + 1);
                                                }
                                            }
                                        }

                                        charIndex += addonCount;
                                        break;

                                    default:
                                        // Element not Found
                                        if (char1.ToUpper() == "X")
                                        {
                                            // X for solver but no preceding bracket
                                            mErrorParams.ErrorId = 18;
                                        }
                                        else
                                        {
                                            mErrorParams.ErrorId = 1;
                                        }

                                        mErrorParams.ErrorPosition = charIndex;
                                        break;
                                }

                                prevSymbolReference = symbolReference;
                                break;

                            case 94: // ^ (caret)
                                adjacentNum = ParseNum(char2 + char3 + charRemain, out numLength);
                                CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                if (mErrorParams.ErrorId != 0)
                                {
                                    // Problem, don't go on.
                                }
                                else
                                {
                                    var nextCharIndex = charIndex + 1 + numLength;
                                    var charAsc = 0;
                                    if (nextCharIndex < formula.Length)
                                    {
                                        charAsc = formula[nextCharIndex];
                                    }

                                    if (adjacentNum >= 0d)
                                    {
                                        if (charAsc >= 65 && charAsc <= 90 || charAsc >= 97 && charAsc <= 122) // Uppercase A to Z and lowercase a to z
                                        {
                                            caretPresent = true;
                                            caretVal = adjacentNum;
                                            charIndex += numLength;
                                        }
                                        else
                                        {
                                            // No letter after isotopic mass
                                            mErrorParams.ErrorId = 22;
                                            mErrorParams.ErrorPosition = charIndex + numLength + 1;
                                        }
                                    }
                                    else
                                    {
                                        // Adjacent number is < 0 or not present
                                        // Record error
                                        caretPresent = false;
                                        if (formula.Substring(charIndex + 1, 1) == "-")
                                        {
                                            // Negative number following caret
                                            mErrorParams.ErrorId = 23;
                                            mErrorParams.ErrorPosition = charIndex + 1;
                                        }
                                        else
                                        {
                                            // No number following caret
                                            mErrorParams.ErrorId = 20;
                                            mErrorParams.ErrorPosition = charIndex + 1;
                                        }
                                    }
                                }

                                break;

                            default:
                                // There shouldn't be anything else (except the ~ filler character). If there is, we'll just ignore it
                                break;
                        }

                        if (charIndex == formula.Length - 1)
                        {
                            // Need to make sure compounds are present after a leading coefficient after a dash
                            if (dashMultiplier > 0d)
                            {
                                if (charIndex != dashPos)
                                {
                                    // Things went fine, no need to set anything
                                }
                                else
                                {
                                    // No compounds after leading coefficient after dash
                                    mErrorParams.ErrorId = 25;
                                    mErrorParams.ErrorPosition = charIndex;
                                }
                            }
                        }

                        if (mErrorParams.ErrorId != 0)
                        {
                            charIndex = formula.Length;
                        }

                        charIndex ++;
                    }
                    while (charIndex < formula.Length);
                }

                if (insideBrackets)
                {
                    if (mErrorParams.ErrorId == 0)
                    {
                        // Missing closing bracket
                        mErrorParams.ErrorId = 13;
                        mErrorParams.ErrorPosition = charIndex;
                    }
                }

                if (mErrorParams.ErrorId != 0 && mErrorParams.ErrorCharacter.Length == 0)
                {
                    if (string.IsNullOrEmpty(char1))
                        char1 = EMPTY_STRING_CHAR.ToString();
                    mErrorParams.ErrorCharacter = char1;
                    mErrorParams.ErrorPosition += charCountPrior;
                }

                if (loneCarbonOrSilicon > 1)
                {
                    // Correct Charge for number of C and Si
                    computationStats.Charge -= (loneCarbonOrSilicon - 1) * 2;

                    carbonOrSiliconReturnCount = loneCarbonOrSilicon;
                }
                else
                {
                    carbonOrSiliconReturnCount = 0;
                }

                // Return formula, which is possibly now capitalized correctly
                // It will also contain expanded abbreviations
                return formula;
            }
            catch (Exception ex)
            {
                MwtWinDllErrorHandler("MolecularWeightCalculator_ElementAndMassTools.ParseFormula", ex);
                mErrorParams.ErrorId = -10;
                mErrorParams.ErrorPosition = 0;

                return formula;
            }
        }

        /// <summary>
        /// Looks for a number and returns it if found
        /// </summary>
        /// <param name="work">Input</param>
        /// <param name="numLength">Output: length of the number</param>
        /// <param name="allowNegative"></param>
        /// <returns>
        /// Parsed number if found
        /// If not a number, returns a negative number for the error code and sets numLength = 0
        /// </returns>
        /// <remarks>
        /// Error codes:
        /// -1 = No number
        /// -2 =                                             (unused)
        /// -3 = No number at all or (more likely) no number after decimal point
        /// -4 = More than one decimal point
        /// </remarks>
        private double ParseNum(string work, out int numLength, bool allowNegative = false)
        {
            if (ComputationOptions.DecimalSeparator == default(char))
            {
                ComputationOptions.DecimalSeparator = MolecularWeightTool.DetermineDecimalPoint();
            }

            // Set numLength to -1 for now
            // If it doesn't get set to 0 (due to an error), it will get set to the
            // length of the matched number before exiting the sub
            numLength = -1;
            var foundNum = string.Empty;

            if (string.IsNullOrEmpty(work))
                work = EMPTY_STRING_CHAR.ToString();
            if ((work[0] < 48 || work[0] > 57) && work.Substring(0, 1) != ComputationOptions.DecimalSeparator.ToString() && !(work.Substring(0, 1) == "-" && allowNegative))
            {
                numLength = 0; // No number found
                return -1;
            }

            // Start of string is a number or a decimal point, or (if allowed) a negative sign
            for (var index = 0; index < work.Length; index++)
            {
                var working = work.Substring(index, 1);
                if (char.IsDigit(working[0]) || working == ComputationOptions.DecimalSeparator.ToString() || allowNegative && working == "-")
                {
                    foundNum += working;
                }
                else
                {
                    break;
                }
            }

            if (foundNum.Length == 0 || foundNum == ComputationOptions.DecimalSeparator.ToString())
            {
                // No number at all or (more likely) no number after decimal point
                foundNum = (-3).ToString();
                numLength = 0;
            }
            else
            {
                // Check for more than one decimal point (. or ,)
                var decPtCount = foundNum.Count(c => c == ComputationOptions.DecimalSeparator);

                if (decPtCount > 1)
                {
                    // more than one decPtCount
                    foundNum = (-4).ToString();
                    numLength = 0;
                }
            }

            if (numLength < 0)
                numLength = (short)foundNum.Length;

            return NumberConverter.CDblSafe(foundNum);
        }

        /// <summary>
        /// Converts plain text to formatted RTF text
        /// </summary>
        /// <param name="workText"></param>
        /// <param name="calculatorMode">When true, does not superscript + signs and numbers following + signs</param>
        /// <param name="highlightCharFollowingPercentSign">When true, change the character following a percent sign to red (and remove the percent sign)</param>
        /// <param name="overrideErrorId"></param>
        /// <param name="errorIdOverride"></param>
        /// <returns></returns>
        public string PlainTextToRtfInternal(
            string workText,
            bool calculatorMode = false,
            bool highlightCharFollowingPercentSign = true,
            bool overrideErrorId = false,
            int errorIdOverride = 0)
        {
            // ReSharper disable CommentTypo

            // Rtf string must begin with {{\fonttbl{\f0\fcharset0\fprq2 Times New Roman;}}\pard\plain\fs25

            // and must end with } or {\fs30  }} if superscript was used

            // "{\super 3}C{\sub 6}H{\sub 6}{\fs30  }}"
            // var rtf = "{{\fonttbl{\f0\fcharset0\fprq2 " + rtfFormula(0).font + ";}}\pard\plain\fs25 ";
            // Old: var rtf = "{\rtf1\ansi\deff0\deftab720{\fonttbl{\f0\fswiss MS Sans Serif;}{\f1\froman\fcharset2 Symbol;}{\f2\froman\fcharset2 Times New Roman;}{\f3\froman " + lblMWT[0].FontName + ";}}{\colortbl\red0\green0\blue0;\red255\green0\blue0;}\deflang1033\pard\plain\f3\fs25 ";
            // old: var rtf = "{\rtf1\ansi\deff0\deftab720{\fonttbl{\f0\fswiss MS Sans Serif;}{\f1\froman\fcharset2 Symbol;}{\f2\froman " + lblMWT[0].FontName + ";}{\f3\fswiss\fprq2 System;}}{\colortbl\red0\green0\blue0;\red255\green0\blue0;}\deflang1033\pard\plain\f2\fs25 ";
            // f0                               f1                                 f2                          f3                               f4                      cf0 (black)        cf1 (red)          cf3 (white)
            // ReSharper disable StringLiteralTypo
            var rtf = @"{\rtf1\ansi\deff0\deftab720{\fonttbl{\f0\fswiss MS Sans Serif;}{\f1\froman\fcharset2 Symbol;}{\f2\froman " + ComputationOptions.RtfFontName + @";}{\f3\froman Times New Roman;}{\f4\fswiss\fprq2 System;}}{\colortbl\red0\green0\blue0;\red255\green0\blue0;\red255\green255\blue255;}\deflang1033\pard\plain\f2\fs" + NumberConverter.CShortSafe(ComputationOptions.RtfFontSize * 2.5d) + " ";
            // ReSharper restore StringLiteralTypo

            // ReSharper restore CommentTypo

            if ((workText ?? "") == string.Empty)
            {
                // Return a blank RTF string
                return rtf + "}";
            }

            var superFound = false;
            var workCharPrev = "";
            for (var charIndex = 0; charIndex < workText.Length; charIndex++)
            {
                var workChar = workText.Substring(charIndex, 1);
                if (workChar == "%" && highlightCharFollowingPercentSign)
                {
                    // An error was found and marked by a % sign
                    // Highlight the character at the % sign, and remove the % sign
                    if (charIndex == workText.Length - 1)
                    {
                        // At end of line
                        int errorId;
                        if (overrideErrorId && errorIdOverride != 0)
                        {
                            errorId = errorIdOverride;
                        }
                        else
                        {
                            errorId = mErrorParams.ErrorId;
                        }

                        switch (errorId)
                        {
                            case var @case when 2 <= @case && @case <= 4:
                                // Error involves a parentheses, find last opening parenthesis, (, or opening curly bracket, {
                                for (var charIndex2 = rtf.Length - 1; charIndex2 >= 2; charIndex2 -= 1)
                                {
                                    if (rtf.Substring(charIndex2, 1) == "(")
                                    {
                                        rtf = rtf.Substring(0, charIndex2 - 1) + @"{\cf1 (}" + rtf.Substring(charIndex2 + 1);
                                        break;
                                    }

                                    if (rtf.Substring(charIndex2, 1) == "{")
                                    {
                                        rtf = rtf.Substring(0, charIndex2 - 1) + @"{\cf1 \{}" + rtf.Substring(charIndex2 + 1);
                                        break;
                                    }
                                }

                                break;

                            case 13:
                            case 15:
                                // Error involves a bracket, find last opening bracket, [
                                for (var charIndex2 = rtf.Length - 1; charIndex2 >= 2; charIndex2 -= 1)
                                {
                                    if (rtf.Substring(charIndex2, 1) == "[")
                                    {
                                        rtf = rtf.Substring(0, charIndex2 - 1) + @"{\cf1 [}" + rtf.Substring(charIndex2 + 1);
                                        break;
                                    }
                                }

                                break;

                            default:
                                // Nothing to highlight
                                break;
                        }
                    }
                    else
                    {
                        // Highlight next character and skip % sign
                        workChar = workText.Substring(charIndex + 1, 1);
                        // Handle curly brackets
                        if (workChar == "{" || workChar == "}")
                            workChar = @"\" + workChar;
                        rtf += @"{\cf1 " + workChar + "}";
                        charIndex++;
                    }
                }
                else if (workChar == "^")
                {
                    rtf += @"{\super ^}";
                    superFound = true;
                }
                else if (workChar == "_")
                {
                    rtf += @"{\super}";
                    superFound = true;
                }
                else if (workChar == "+" && !calculatorMode)
                {
                    rtf += @"{\super +}";
                    superFound = true;
                }
                else if (workChar == EMPTY_STRING_CHAR.ToString())
                {
                    // skip it, the tilde sign is used to add additional height to the formula line when isotopes are used
                    // If it's here from a previous time, we ignore it, adding it at the end if needed (if superFound = true)
                }
                else if (char.IsDigit(workChar[0]) || workChar == ComputationOptions.DecimalSeparator.ToString())
                {
                    // Number or period, so super or subscript it if needed
                    if (charIndex == 1)
                    {
                        // at beginning of line, so leave it alone. Probably out of place
                        rtf += workChar;
                    }
                    else if (!calculatorMode && (char.IsLetter(workCharPrev[0]) || workCharPrev == ")" || workCharPrev == @"\}" || workCharPrev == "+" || workCharPrev == "_" || rtf.Substring(rtf.Length - 6, 3) == "sub"))
                    {
                        // subscript if previous character was a character, parentheses, curly bracket, plus sign, or was already subscripted
                        // But, don't use subscripts in calculator
                        rtf += @"{\sub " + workChar + "}";
                    }
                    else if (!calculatorMode && ComputationOptions.BracketsAsParentheses && workCharPrev == "]")
                    {
                        // only subscript after closing bracket, ], if brackets are being treated as parentheses
                        rtf += @"{\sub " + workChar + "}";
                    }
                    else if (rtf.Substring(rtf.Length - 8, 5) == "super")
                    {
                        // if previous character was superscripted, then superscript this number too
                        rtf += @"{\super " + workChar + "}";
                        superFound = true;
                    }
                    else
                    {
                        rtf += workChar;
                    }
                }
                else if (workChar == " ")
                {
                    // Ignore it
                }
                else
                {
                    // Handle curly brackets
                    if (workChar == "{" || workChar == "}")
                        workChar = @"\" + workChar;
                    rtf += workChar;
                }

                workCharPrev = workChar;
            }

            if (superFound)
            {
                // Add an extra tall character, the tilde sign (~, RTF_HEIGHT_ADJUST_CHAR)
                // It is used to add additional height to the formula line when isotopes are used
                // It is colored white so the user does not see it
                rtf += @"{\fs" + NumberConverter.CShortSafe(ComputationOptions.RtfFontSize * 3) + @"\cf2 " + RTF_HEIGHT_ADJUST_CHAR + "}}";
            }
            else
            {
                rtf += "}";
            }

            return rtf;
        }

        /// <summary>
        /// Recomputes the Mass for all of the loaded abbreviations
        /// </summary>
        public void RecomputeAbbreviationMassesInternal()
        {
            for (var index = 0; index < mAbbrevStats.Count; index++)
            {
                var formula = mAbbrevStats[index].Formula;
                mAbbrevStats[index].Mass = ComputeFormulaWeight(ref formula);
                mAbbrevStats[index].Formula = formula;
            }
        }

        public void RemoveAllCautionStatementsInternal()
        {
            mCautionStatements.Clear();
        }

        public void RemoveAllAbbreviationsInternal()
        {
            mAbbrevStats.Clear();
            ConstructMasterSymbolsList();
        }

        /// <summary>
        /// Look for the abbreviation and remove it
        /// </summary>
        /// <param name="abbreviationSymbol"></param>
        /// <returns>0 if found and removed; 1 if error</returns>
        public int RemoveAbbreviationInternal(string abbreviationSymbol)
        {
            var removed = default(bool);

            abbreviationSymbol = abbreviationSymbol?.ToLower();

            for (var index = 0; index < mAbbrevStats.Count; index++)
            {
                if ((mAbbrevStats[index].Symbol?.ToLower() ?? "") == (abbreviationSymbol ?? ""))
                {
                    RemoveAbbreviationByIdInternal(index);
                    removed = true;
                }
            }

            return removed ? 0 : 1;
        }

        /// <summary>
        /// Remove the abbreviation at index <paramref name="abbreviationId"/>
        /// </summary>
        /// <param name="abbreviationId"></param>
        /// <returns>0 if found and removed; 1 if error</returns>
        public int RemoveAbbreviationByIdInternal(int abbreviationId)
        {
            bool removed;

            if (abbreviationId >= 0 && abbreviationId < mAbbrevStats.Count)
            {
                mAbbrevStats.RemoveAt(abbreviationId);

                ConstructMasterSymbolsList();
                removed = true;
            }
            else
            {
                removed = false;
            }

            return removed ? 0 : 1;
        }

        /// <summary>
        /// Look for the caution statement and remove it
        /// </summary>
        /// <param name="cautionSymbol"></param>
        /// <returns>0 if found and removed; 1 if error</returns>
        public int RemoveCautionStatementInternal(string cautionSymbol)
        {
            return mCautionStatements.Remove(cautionSymbol) ? 0 : 1;
        }

        public void ResetErrorParamsInternal()
        {
            mErrorParams.ErrorCharacter = "";
            mErrorParams.ErrorId = 0;
            mErrorParams.ErrorPosition = 0;
        }

        protected void ResetProgress()
        {
            ProgressReset?.Invoke();
        }

        protected void ResetProgress(string progressStepDescription)
        {
            UpdateProgress(progressStepDescription, 0f);
            ProgressReset?.Invoke();
        }

        public string ReturnFormattedMassAndStdDev(double mass,
            double stdDev,
            bool includeStandardDeviation = true,
            bool includePctSign = false)
        {
            // Plan:
            // Round stdDev to 1 final digit.
            // Round mass to the appropriate place based on StdDev.

            // mass is the main number
            // stdDev is the standard deviation

            var result = string.Empty;

            try
            {
                double roundedMain;
                string pctSign;
                // includePctSign is True when formatting Percent composition values
                if (includePctSign)
                {
                    pctSign = "%";
                }
                else
                {
                    pctSign = "";
                }

                if (Math.Abs(stdDev) < float.Epsilon)
                {
                    // Standard deviation value is 0; simply return the result
                    result = mass.ToString("0.0####") + pctSign + " (" + '±' + "0)";

                    // roundedMain is used later, must copy mass to it
                    roundedMain = mass;
                }
                else
                {
                    // First round stdDev to show just one number
                    var roundedStdDev = double.Parse(stdDev.ToString("0E+000"));

                    // Now round mass
                    // Simply divide mass by 10^Exponent of the Standard Deviation
                    // Next round
                    // Now multiply to get back the rounded mass
                    var workText = stdDev.ToString("0E+000");
                    var stdDevShort = workText.Substring(0, 1);

                    var exponentValue = NumberConverter.CShortSafe(workText.Substring(workText.Length - 4));
                    var work = mass / Math.Pow(10d, exponentValue);
                    work = Math.Round(work, 0);
                    roundedMain = work * Math.Pow(10d, exponentValue);

                    workText = roundedMain.ToString("0.0##E+00");

                    switch (ComputationOptions.StdDevMode)
                    {
                        case StdDevMode.Short:
                            // StdDevType Short (Type 0)
                            result = roundedMain.ToString();
                            if (includeStandardDeviation)
                                result += "(" + '±' + stdDevShort + ")";

                            result += pctSign;
                            break;
                        case StdDevMode.Scientific:
                            // StdDevType Scientific (Type 1)
                            result = roundedMain + pctSign;
                            if (includeStandardDeviation)
                                result += " (" + '±' + stdDev.ToString("0.000E+00") + ")";

                            break;
                        default:
                            // StdDevType Decimal
                            result = mass.ToString("0.0####") + pctSign;
                            if (includeStandardDeviation)
                                result += " (" + '±' + roundedStdDev + ")";

                            break;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                MwtWinDllErrorHandler("MolecularWeightCalculator_ElementAndMassTools.ReturnFormattedMassAndStdDev", ex);
                mErrorParams.ErrorId = -10;
                mErrorParams.ErrorPosition = 0;
            }

            if (string.IsNullOrEmpty(result))
                result = string.Empty;
            return result;
        }

        public double RoundToMultipleOf10(double thisNum)
        {
            // Round to nearest 1, 2, or 5 (or multiple of 10 thereof)
            // First, find the exponent of thisNum
            var workText = thisNum.ToString("0E+000");
            var exponentValue = NumberConverter.CIntSafe(workText.Substring(workText.Length - 4));
            var work = thisNum / Math.Pow(10d, exponentValue);
            work = NumberConverter.CIntSafe(work);

            // work should now be between 0 and 9
            switch (work)
            {
                case 0d:
                case 1d:
                    thisNum = 1d;
                    break;
                case var @case when 2d <= @case && @case <= 4d:
                    thisNum = 2d;
                    break;
                default:
                    thisNum = 5d;
                    break;
            }

            // Convert thisNum back to the correct magnitude
            thisNum *= Math.Pow(10d, exponentValue);

            return thisNum;
        }

        public double RoundToEvenMultiple(double valueToRound, double multipleValue, bool roundUp)
        {
            // Find the exponent of MultipleValue
            var workText = multipleValue.ToString("0E+000");
            var exponentValue = NumberConverter.CIntSafe(workText.Substring(workText.Length - 4));

            var loopCount = 0;
            while ((valueToRound / multipleValue).ToString() != Math.Round(valueToRound / multipleValue, 0).ToString())
            {
                var work = valueToRound / Math.Pow(10d, exponentValue);
                work = double.Parse(work.ToString("0"));
                work *= Math.Pow(10d, exponentValue);
                if (roundUp)
                {
                    if (work <= valueToRound)
                    {
                        work += Math.Pow(10d, exponentValue);
                    }
                }
                else if (work >= valueToRound)
                {
                    work -= Math.Pow(10d, exponentValue);
                }

                valueToRound = work;
                loopCount += 1;
                if (loopCount > 500)
                {
                    // Debug.Assert False
                    break;
                }
            }

            return valueToRound;
        }

        /// <summary>
        /// Adds a new abbreviation or updates an existing one (based on <paramref name="symbol"/>)
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="formula"></param>
        /// <param name="charge"></param>
        /// <param name="isAminoAcid"></param>
        /// <param name="oneLetterSymbol"></param>
        /// <param name="comment"></param>
        /// <param name="validateFormula">If true, make sure the formula is valid</param>
        /// <returns>0 if successful, otherwise an error ID</returns>
        /// <remarks>
        /// It is useful to set <paramref name="validateFormula"/> = false when you're defining all of the abbreviations at once,
        /// since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        /// defined, then the parsing of the first abbreviation will fail
        /// </remarks>
        public int SetAbbreviationInternal(
            string symbol, string formula,
            float charge, bool isAminoAcid,
            string oneLetterSymbol = "",
            string comment = "",
            bool validateFormula = true)
        {
            var abbrevId = default(int);

            // See if the abbreviation is already present
            var alreadyPresent = false;
            for (var index = 0; index < mAbbrevStats.Count; index++)
            {
                if ((mAbbrevStats[index].Symbol?.ToUpper() ?? "") == (symbol?.ToUpper() ?? ""))
                {
                    alreadyPresent = true;
                    abbrevId = index;
                    break;
                }
            }

            // AbbrevStats is a 1-based array
            if (!alreadyPresent)
            {
                if (mAbbrevStats.Count < MAX_ABBREV_COUNT)
                {
                    abbrevId = mAbbrevStats.Count;
                }
                else
                {
                    // Too many abbreviations
                    mErrorParams.ErrorId = 196;
                }
            }

            if (abbrevId >= 1)
            {
                SetAbbreviationByIdInternal((short)abbrevId, symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, validateFormula);
            }

            return mErrorParams.ErrorId;
        }

        /// <summary>
        /// Adds a new abbreviation or updates an existing one (based on <paramref name="abbrevId"/>)
        /// </summary>
        /// <param name="abbrevId">If abbrevId is less than 1, adds as a new abbreviation</param>
        /// <param name="symbol"></param>
        /// <param name="formula"></param>
        /// <param name="charge"></param>
        /// <param name="isAminoAcid"></param>
        /// <param name="oneLetterSymbol"></param>
        /// <param name="comment"></param>
        /// <param name="validateFormula"></param>
        /// <returns>0 if successful, otherwise an error ID</returns>
        public int SetAbbreviationByIdInternal(
            short abbrevId, string symbol,
            string formula, float charge,
            bool isAminoAcid,
            string oneLetterSymbol = "",
            string comment = "",
            bool validateFormula = true)
        {
            var computationStats = new ComputationStats();

            var abbrevSymbolStack = new AbbrevSymbolStack();
            var invalidSymbolOrFormula = default(bool);

            ResetErrorParamsInternal();

            if (symbol.Length < 1)
            {
                // Symbol length is 0
                mErrorParams.ErrorId = 192;
            }
            else if (symbol.Length > MAX_ABBREV_LENGTH)
            {
                // Abbreviation symbol too long
                mErrorParams.ErrorId = 190;
            }
            else if (IsStringAllLetters(symbol))
            {
                if (formula.Length > 0)
                {
                    // Convert symbol to proper case mode
                    symbol = symbol.Substring(0, 1).ToUpper() + symbol.Substring(1).ToLower();

                    // If abbrevId is < 1 or larger than AbbrevAllCount, then define it
                    if (abbrevId < 0 || abbrevId >= mAbbrevStats.Count)
                    {
                        if (mAbbrevStats.Count < MAX_ABBREV_COUNT)
                        {
                            abbrevId = (short)mAbbrevStats.Count;
                        }
                        else
                        {
                            // Too many abbreviations
                            mErrorParams.ErrorId = 196;
                            abbrevId = -1;
                        }
                    }

                    if (abbrevId >= 1)
                    {
                        // Make sure the abbreviation doesn't match one of the standard elements
                        var symbolMatchType = CheckElemAndAbbrev(symbol, out var symbolReference);

                        if (symbolMatchType == SymbolMatchMode.Element)
                        {
                            if ((mElementStats[symbolReference].Symbol ?? "") == symbol)
                            {
                                invalidSymbolOrFormula = true;
                            }
                        }

                        if (!invalidSymbolOrFormula && validateFormula)
                        {
                            // Make sure the abbreviation's formula is valid
                            // This will also auto-capitalize the formula if auto-capitalize is turned on
                            formula = ParseFormulaRecursive(formula, computationStats, abbrevSymbolStack, false, out _, out _);

                            if (mErrorParams.ErrorId != 0)
                            {
                                // An error occurred while parsing
                                // Already present in ErrorParams.ErrorID
                                // We'll still add the formula, but mark it as invalid
                                invalidSymbolOrFormula = true;
                            }
                        }

                        AddAbbreviationWork(abbrevId, symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, invalidSymbolOrFormula);

                        ConstructMasterSymbolsList();
                    }
                }
                else
                {
                    // Invalid formula (actually, blank formula)
                    mErrorParams.ErrorId = 160;
                }
            }
            else
            {
                // Symbol does not just contain letters
                mErrorParams.ErrorId = 194;
            }

            return mErrorParams.ErrorId;
        }

        /// <summary>
        /// Adds a new caution statement or updates an existing one (based on <paramref name="symbolCombo"/>)
        /// </summary>
        /// <param name="symbolCombo"></param>
        /// <param name="newCautionStatement"></param>
        /// <returns>0 if successful, otherwise, returns an Error ID</returns>
        public int SetCautionStatementInternal(string symbolCombo, string newCautionStatement)
        {
            ResetErrorParamsInternal();

            if (symbolCombo.Length >= 1 && symbolCombo.Length <= MAX_ABBREV_LENGTH)
            {
                // Make sure all the characters in symbolCombo are letters
                if (IsStringAllLetters(symbolCombo))
                {
                    if (newCautionStatement.Length > 0)
                    {
                        // See if symbolCombo is present in CautionStatements[]
                        var alreadyPresent = mCautionStatements.ContainsKey(symbolCombo);

                        if (!alreadyPresent && mCautionStatements.Count >= MAX_CAUTION_STATEMENTS)
                        {
                            // Too many caution statements
                            mErrorParams.ErrorId = 1215;
                        }
                        else if (!alreadyPresent)
                        {
                            mCautionStatements.Add(symbolCombo, newCautionStatement);
                        }
                        else
                        {
                            mCautionStatements[symbolCombo] = newCautionStatement;
                        }
                    }
                    else
                    {
                        // Caution description length is 0
                        mErrorParams.ErrorId = 1210;
                    }
                }
                else
                {
                    // Caution symbol doesn't just contain letters
                    mErrorParams.ErrorId = 1205;
                }
            }
            else
            {
                // Symbol length is 0 or is greater than MAX_ABBREV_LENGTH
                mErrorParams.ErrorId = 1200;
            }

            return mErrorParams.ErrorId;
        }

        public void SetChargeCarrierMassInternal(double mass)
        {
            mChargeCarrierMass = mass;
        }

        /// <summary>
        /// Update the values for a single element (based on <paramref name="symbol"/>)
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="mass"></param>
        /// <param name="uncertainty"></param>
        /// <param name="charge"></param>
        /// <param name="recomputeAbbreviationMasses">Set to False if updating several elements</param>
        /// <returns></returns>
        public int SetElementInternal(string symbol, double mass,
            double uncertainty, float charge,
            bool recomputeAbbreviationMasses = true)
        {
            var found = default(bool);

            for (var index = 1; index <= ELEMENT_COUNT; index++)
            {
                if ((symbol?.ToLower() ?? "") == (mElementStats[index].Symbol?.ToLower() ?? ""))
                {
                    var stats = mElementStats[index];
                    stats.Mass = mass;
                    stats.Uncertainty = uncertainty;
                    stats.Charge = charge;

                    found = true;
                    break;
                }
            }

            if (found)
            {
                if (recomputeAbbreviationMasses)
                    RecomputeAbbreviationMassesInternal();
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Set the isotopes for the element
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses">0-based array</param>
        /// <param name="isotopeAbundances">0-based array</param>
        /// <returns>0 if successful, 1 if symbol not found</returns>
        public int SetElementIsotopesInternal(string symbol, short isotopeCount, double[] isotopeMasses, float[] isotopeAbundances)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return 1;
            }

            var found = default(bool);

            for (var index = 1; index <= ELEMENT_COUNT; index++)
            {
                if (string.Equals(symbol.ToLower(), mElementStats[index].Symbol.ToLower(), StringComparison.OrdinalIgnoreCase))
                {
                    var stats = mElementStats[index];
                    stats.Isotopes.Clear();
                    for (var isotopeIndex = 0; isotopeIndex < isotopeMasses.Length; isotopeIndex++)
                    {
                        if (isotopeIndex > MAX_ISOTOPES)
                            break;
                        stats.Isotopes.Add(new IsotopeInfo(isotopeMasses[isotopeIndex], isotopeAbundances[isotopeIndex]));
                    }

                    stats.Isotopes.Capacity = stats.Isotopes.Count;

                    found = true;
                    break;
                }
            }

            return found ? 0 : 1;
        }

        /// <summary>
        /// Set the element mode
        /// </summary>
        /// <param name="newElementMode"></param>
        /// <param name="forceMemoryLoadElementValues">Set to true if you want to force a reload of element weights, even if not changing element modes</param>
        public void SetElementModeInternal(ElementMassMode newElementMode, bool forceMemoryLoadElementValues = false)
        {
            try
            {
                if (newElementMode < ElementMassMode.Average || newElementMode > ElementMassMode.Integer)
                {
                    return;
                }

                if (newElementMode != mCurrentElementMode || forceMemoryLoadElementValues || mElementAlph.Count == 0)
                {
                    mCurrentElementMode = newElementMode;

                    MemoryLoadElements(mCurrentElementMode);
                    ConstructMasterSymbolsList();
                    RecomputeAbbreviationMassesInternal();
                }
            }
            catch (Exception ex)
            {
                GeneralErrorHandler("ElementAndMassTools.SetElementModeInternal", ex);
            }
        }

        /// <summary>
        /// Used to replace the default message strings with foreign language equivalent ones
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="newMessage"></param>
        /// <returns>0 if success; 1 if failure</returns>
        public int SetMessageStatementInternal(int messageId, string newMessage)
        {
            if (messageId >= 1 && messageId <= MESSAGE_STATEMENT_DIM_COUNT && newMessage.Length > 0)
            {
                mMessageStatements[messageId] = newMessage;
                return 0;
            }

            return 1;
        }

        public void SetShowErrorMessageDialogs(bool value)
        {
            ShowErrorMessageDialogs = value;
        }

        public void SortAbbreviationsInternal()
        {
            // Default comparison specified in AbbrevStatsData
            mAbbrevStats.Sort();

            // Need to re-construct the master symbols list
            ConstructMasterSymbolsList();
        }

        /// <summary>
        /// Adds spaces to <paramref name="work"/> until the length is <paramref name="length"/>
        /// </summary>
        /// <param name="work"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string SpacePad(string work, short length)
        {
            while (work.Length < length)
                work += " ";

            return work;
        }

        private string SpacePadFront(string work, short length)
        {
            while (work.Length < length)
                work = " " + work;

            return work;
        }

        /// <summary>
        /// Update the progress description
        /// </summary>
        /// <param name="progressStepDescription">Description of the current processing occurring</param>
        /// <remarks></remarks>
        protected void UpdateProgress(string progressStepDescription)
        {
            UpdateProgress(progressStepDescription, mProgressPercentComplete);
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <param name="percentComplete">Value between 0 and 100, indicating percent complete</param>
        /// <remarks></remarks>
        protected void UpdateProgress(float percentComplete)
        {
            UpdateProgress(ProgressStepDescription, percentComplete);
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <param name="progressStepDescription">Description of the current processing occurring</param>
        /// <param name="percentComplete">Value between 0 and 100, indicating percent complete</param>
        /// <remarks></remarks>
        protected void UpdateProgress(string progressStepDescription, float percentComplete)
        {
            var descriptionChanged = !string.Equals(progressStepDescription, mProgressStepDescription);

            mProgressStepDescription = string.Copy(progressStepDescription);
            if (percentComplete < 0f)
            {
                percentComplete = 0f;
            }
            else if (percentComplete > 100f)
            {
                percentComplete = 100f;
            }

            mProgressPercentComplete = percentComplete;

            if (descriptionChanged)
            {
                if (Math.Abs(mProgressPercentComplete) < float.Epsilon)
                {
                    LogMessage(mProgressStepDescription);
                }
                else
                {
                    LogMessage(mProgressStepDescription + " (" + mProgressPercentComplete.ToString("0.0") + "% complete)");
                }
            }

            ProgressChanged?.Invoke(ProgressStepDescription, ProgressPercentComplete);
        }

        protected void OperationComplete()
        {
            ProgressComplete?.Invoke();
        }

        /// <summary>
        /// Checks the formula of all abbreviations to make sure it's valid
        /// Marks any abbreviations as Invalid if a problem is found or a circular reference exists
        /// </summary>
        /// <returns>Count of the number of invalid abbreviations found</returns>
        public int ValidateAllAbbreviationsInternal()
        {
            var invalidAbbreviationCount = default(short);

            for (short abbrevIndex = 0; abbrevIndex < mAbbrevStats.Count; abbrevIndex++)
            {
                var stats = mAbbrevStats[abbrevIndex];
                SetAbbreviationByIdInternal(abbrevIndex, stats.Symbol, stats.Formula, stats.Charge, stats.IsAminoAcid, stats.OneLetterSymbol, stats.Comment, true);
                if (stats.InvalidSymbolOrFormula)
                {
                    invalidAbbreviationCount++;
                }
            }

            return invalidAbbreviationCount;
        }
    }
}
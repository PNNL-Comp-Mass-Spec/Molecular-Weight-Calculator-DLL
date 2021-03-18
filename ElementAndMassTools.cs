using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace MolecularWeightCalculator
{
    public class ElementAndMassTools
    {
        // Molecular Weight Calculator routines with ActiveX Class interfaces: ElementAndMassTools

        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA) in 2003
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

        public ElementAndMassTools()
        {
            Initialize();
        }

        #region "Constants and Enums"

        public const int ELEMENT_COUNT = 103;
        public const int MAX_ABBREV_COUNT = 500;

        private const int MESSAGE_STATEMENT_DIM_COUNT = 1600;
        internal const int MAX_ABBREV_LENGTH = 6;
        internal const int MAX_ISOTOPES = 11;
        private const int MAX_CAUTION_STATEMENTS = 100;

        private const char EMPTY_STRING_CHAR = '~';
        private const char RTF_HEIGHT_ADJUST_CHAR = '~'; // A hidden character to adjust the height of Rtf Text Boxes when using superscripts

        public enum ElementMassMode
        {
            Average = 1,
            Isotopic = 2,
            Integer = 3
        }

        public enum StdDevMode
        {
            Short = 0,
            Scientific = 1,
            Decimal = 2
        }

        public enum CaseConversionMode
        {
            ConvertCaseUp = 0,
            ExactCase = 1,
            SmartCase = 2
        }

        private enum SymbolMatchMode
        {
            Unknown = 0,
            Element = 1,
            Abbreviation = 2
        }

        protected enum MessageType
        {
            Normal = 0,
            Error = 1,
            Warning = 2
        }

        #endregion

        #region "Data classes"

        public class Options
        {
            public MolecularWeightTool.AbbrevRecognitionMode AbbrevRecognitionMode;
            public bool BracketsAsParentheses;
            public CaseConversionMode CaseConversion;
            public char DecimalSeparator;
            public string RtfFontName;
            public short RtfFontSize;
            public StdDevMode StdDevMode; // Can be 0, 1, or 2 (see smStdDevModeConstants)
        }

        public class IsotopicAtomInfo
        {
            public double Count; // Can have non-integer counts of atoms, eg. ^13C5.5
            public double Mass;
        }

        public class ElementUseStats
        {
            public bool Used;
            public double Count; // Can have non-integer counts of atoms, eg. C5.5
            public double IsotopicCorrection;
            public short IsotopeCount; // Number of specific isotopes defined
            public IsotopicAtomInfo[] Isotopes;
        }

        public class PercentCompositionInfo
        {
            public double PercentComposition;
            public double StdDeviation;

            public override string ToString()
            {
                return PercentComposition.ToString("0.0000");
            }
        }

        public class ComputationStats
        {
            public ElementUseStats[] Elements;        // 1-based array, ranging from 1 to ELEMENT_COUNT
            public double TotalMass;
            public PercentCompositionInfo[] PercentCompositions;     // 1-based array, ranging from 1 to ELEMENT_COUNT
            public float Charge;
            public double StandardDeviation;

            // Note: "Initialize" must be called to initialize instances of this structure
            public void Initialize()
            {
                const int elementCount = 104;
                Elements = new ElementUseStats[elementCount];
                PercentCompositions = new PercentCompositionInfo[elementCount];

                for (var i = 0; i < elementCount; i++)
                {
                    Elements[i] = new ElementUseStats();
                    PercentCompositions[i] = new PercentCompositionInfo();
                }
            }
        }

        public class IsotopeInfo
        {
            public double Mass;
            public float Abundance;

            public override string ToString()
            {
                return Mass.ToString("0.0000");
            }
        }

        public class ElementInfo
        {
            public string Symbol;
            public double Mass;
            public double Uncertainty;
            public float Charge;
            public short IsotopeCount; // # of isotopes an element has
            public IsotopeInfo[] Isotopes; // Masses and Abundances of the isotopes; 1-based array, ranging from 1 to MAX_Isotopes

            // Note: "Initialize" must be called to initialize instances of this structure
            public void Initialize()
            {
                Isotopes = new IsotopeInfo[ElementAndMassTools.MAX_ISOTOPES + 1];

                for (var i = 0; i < Isotopes.Length; i++)
                {
                    Isotopes[i] = new IsotopeInfo();
                }
            }

            public override string ToString()
            {
                return Symbol + ": " + Mass.ToString("0.0000");
            }
        }

        public class AbbrevStatsData
        {
            /// <summary>
            /// The symbol for the abbreviation, e.g. Ph for the phenyl group or Ala for alanine (3 letter codes for amino acids)
            /// </summary>
            public string Symbol;

            /// <summary>
            /// Empirical formula
            /// Cannot contain other abbreviations
            /// </summary>
            public string Formula;

            /// <summary>
            /// Computed mass for quick reference
            /// </summary>
            public double Mass;

            /// <summary>
            /// Charge state
            /// </summary>
            public float Charge;

            /// <summary>
            /// True if an amino acid
            /// </summary>
            public bool IsAminoAcid;

            /// <summary>
            /// One letter symbol (only used for amino acids)
            /// </summary>
            public string OneLetterSymbol;

            /// <summary>
            /// Description of the abbreviation
            /// </summary>
            public string Comment;

            /// <summary>
            /// True if this abbreviation has an invalid symbol or formula
            /// </summary>
            public bool InvalidSymbolOrFormula;

            public override string ToString()
            {
                return Symbol + ": " + Formula;
            }
        }

        private class ErrorDescription
        {
            public int ErrorId; // Contains the error number (used in the LookupMessage function).  In addition, if a program error occurs, ErrorParams.ErrorID = -10
            public int ErrorPosition;
            public string ErrorCharacter;

            public override string ToString()
            {
                return "ErrorID " + ErrorId + " at " + ErrorPosition + ": " + ErrorCharacter;
            }
        }

        private class IsoResultsByElement
        {
            public short ElementIndex; // Index of element in ElementStats() array; look in ElementStats() to get information on its isotopes
            public bool ExplicitIsotope; // True if an explicitly defined isotope
            public double ExplicitMass;
            public int AtomCount; // Number of atoms of this element in the formula being parsed
            public int ResultsCount; // Number of masses in MassAbundances
            public int StartingResultsMass; // Starting mass of the results for this element
            public float[] MassAbundances; // Abundance of each mass, starting with StartingResultsMass
        }

        private class IsoResultsOverallData
        {
            public float Abundance;
            public int Multiplicity;
        }

        private class AbbrevSymbolStack
        {
            public short Count;
            public short[] SymbolReferenceStack; // 0-based array
        }

        // ReSharper disable once InconsistentNaming
        private class XYData
        {
            public double X;
            public double Y;
        }
        #endregion

        #region "Classwide Variables"

        public Options gComputationOptions = new Options();

        /// <summary>
        /// Stores the elements in alphabetical order
        /// Used for constructing empirical formulas
        /// 1 to ELEMENT_COUNT
        /// </summary>
        private string[] ElementAlph;

        /// <summary>
        /// Element stats
        /// 1 to ELEMENT_COUNT
        /// </summary>
        private ElementInfo[] ElementStats;

        /// <summary>
        /// Stores the element symbols, abbreviations, and amino acids in order of longest symbol length to shortest length, non-alphabetized,
        /// for use in symbol matching when parsing a formula
        /// 1 To MasterSymbolsListCount
        /// </summary>
        /// <remarks>No number for array size since we dynamically allocate memory for it</remarks>
        private string[,] MasterSymbolsList;
        private short MasterSymbolsListCount;

        /// <summary>
        /// Includes both abbreviations and amino acids; 1-based array
        /// </summary>
        private AbbrevStatsData[] AbbrevStats;
        private short AbbrevAllCount;

        /// <summary>
        /// CautionStatements(x,0) holds the symbol combo to look for
        /// CautionStatements(x, 1) holds the caution statement; 1-based array
        /// </summary>
        private string[,] CautionStatements;
        private int CautionStatementCount;

        /// <summary>
        /// Error messages; 1-based array
        /// </summary>
        private string[] MessageStatements;
        private int MessageStatementCount;

        private readonly ErrorDescription ErrorParams = new ErrorDescription();

        /// <summary>
        /// Charge carrier mass
        /// 1.00727649 for monoisotopic mass or 1.00739 for average mass
        /// </summary>
        private double mChargeCarrierMass;

        private ElementMassMode mCurrentElementMode;
        private string mStrCautionDescription;
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

        public delegate void ProgressResetEventHandler();

        /// <summary>
        /// Progress changed event
        /// </summary>
        /// <param name="taskDescription"></param>
        /// <param name="percentComplete">Ranges from 0 to 100, but can contain decimal percentage values</param>
        public event ProgressChangedEventHandler ProgressChanged;

        public delegate void ProgressChangedEventHandler(string taskDescription, float percentComplete);

        public event ProgressCompleteEventHandler ProgressComplete;

        public delegate void ProgressCompleteEventHandler();

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

        /// <summary>
        /// Update the abbreviation symbol stack
        /// </summary>
        /// <param name="abbrevSymbolStack">Symbol stack; updated by this method</param>
        /// <param name="symbolReference"></param>
        private void AbbrevSymbolStackAdd(ref AbbrevSymbolStack abbrevSymbolStack, short symbolReference)
        {
            try
            {
                abbrevSymbolStack.Count = (short)(abbrevSymbolStack.Count + 1);
                Array.Resize(ref abbrevSymbolStack.SymbolReferenceStack, abbrevSymbolStack.Count);
                abbrevSymbolStack.SymbolReferenceStack[abbrevSymbolStack.Count - 1] = symbolReference;
            }
            catch (Exception ex)
            {
                GeneralErrorHandler("ElementAndMassTools.AbbrevSymbolStackAdd", ex);
            }
        }

        /// <summary>
        /// Update the abbreviation symbol stack
        /// </summary>
        /// <param name="abbrevSymbolStack">Symbol stack; updated by this method</param>
        private void AbbrevSymbolStackAddRemoveMostRecent(ref AbbrevSymbolStack abbrevSymbolStack)
        {
            if (abbrevSymbolStack.Count > 0)
            {
                abbrevSymbolStack.Count = (short)(abbrevSymbolStack.Count - 1);
            }
        }

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
            var stats = AbbrevStats[abbrevIndex];
            stats.InvalidSymbolOrFormula = invalidSymbolOrFormula;
            stats.Symbol = symbol;
            stats.Formula = formula;
            stats.Mass = ComputeFormulaWeight(ref formula);
            if (stats.Mass < 0d)
            {
                // Error occurred computing mass for abbreviation
                stats.Mass = 0d;
                stats.InvalidSymbolOrFormula = true;
            }

            stats.Charge = charge;
            stats.OneLetterSymbol = oneLetter.ToUpper();
            stats.IsAminoAcid = isAminoAcid;
            stats.Comment = comment;

            return formula;
        }

        private void AddToCautionDescription(string textToAdd)
        {
            if (string.IsNullOrWhiteSpace(mStrCautionDescription))
            {
                mStrCautionDescription = "";
            }

            mStrCautionDescription += textToAdd;
        }

        private void CheckCaution(string formulaExcerpt)
        {
            for (var length = 1; length <= MAX_ABBREV_LENGTH; length++)
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
                        ErrorParams.ErrorId = 12;
                        ErrorParams.ErrorPosition = curCharacter + symbolLength;
                        break;
                    case -4:
                        // Error: More than one decimal point
                        ErrorParams.ErrorId = 27;
                        ErrorParams.ErrorPosition = curCharacter + symbolLength;
                        break;
                    default:
                        // Error: General number error
                        ErrorParams.ErrorId = 14;
                        ErrorParams.ErrorPosition = curCharacter + symbolLength;
                        break;
                }
            }
        }

        /// <summary>
        /// Examines the formula excerpt to determine if it is an element, abbreviation, amino acid, or unknown
        /// </summary>
        /// <param name="formulaExcerpt"></param>
        /// <param name="symbolReference">Output: index of the matched element or abbreviation in MasterSymbolsList[]</param>
        /// <returns>
        /// smtElement if matched an element
        /// smtAbbreviation if matched an abbreviation or amino acid
        /// smtUnknown if no match
        /// </returns>
        private SymbolMatchMode CheckElemAndAbbrev(string formulaExcerpt, ref short symbolReference)
        {
            var symbolMatchType = default(SymbolMatchMode);

            // MasterSymbolsList[] stores the element symbols, abbreviations, & amino acids in order of longest length to
            // shortest length, non-alphabetized, for use in symbol matching when parsing a formula

            // MasterSymbolsList[index,0] contains the symbol to be matched
            // MasterSymbolsList[index,1] contains E for element, A for amino acid, or N for normal abbreviation, followed by
            // the reference number in the master list
            // For example for Carbon, MasterSymbolsList[index,0] = "C" and MasterSymbolsList[index,1] = "E6"

            // Look for match, stepping directly through MasterSymbolsList[]
            // List is sorted by reverse length, so can do all at once

            for (var index = 0; index < MasterSymbolsListCount; index++)
            {
                if (MasterSymbolsList[index, 0].Length > 0)
                {
                    if (formulaExcerpt.Substring(0, Math.Min(formulaExcerpt.Length, MasterSymbolsList[index, 0].Length)) == (MasterSymbolsList[index, 0] ?? ""))
                    {
                        // Matched a symbol
                        switch (MasterSymbolsList[index, 1].Substring(0, 1).ToUpper())
                        {
                            case "E": // An element
                                symbolMatchType = SymbolMatchMode.Element;
                                break;
                            case "A": // An abbreviation or amino acid
                                symbolMatchType = SymbolMatchMode.Abbreviation;
                                break;
                            default:
                                // error
                                symbolMatchType = SymbolMatchMode.Unknown;
                                symbolReference = -1;
                                break;
                        }

                        if (symbolMatchType != SymbolMatchMode.Unknown)
                        {
                            symbolReference = (short)Math.Round(double.Parse(MasterSymbolsList[index, 1].Substring(1)));
                        }

                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Zero-length entry found in MasterSymbolsList[]; this is unexpected");
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
            computationStats.Initialize();

            ParseFormulaPublic(ref formula, ref computationStats, false);

            if (ErrorParams.ErrorId == 0)
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
        /// <param name="convolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
        /// <param name="convolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
        /// <param name="addProtonChargeCarrier">If addProtonChargeCarrier is False, then still convolutes by charge, but doesn't add a proton</param>
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
            ref string results,
            ref double[,] convolutedMSData2DOneBased,
            ref int convolutedMSDataCount,
            bool addProtonChargeCarrier = true,
            string headerIsotopicAbundances = "Isotopic Abundances for",
            string headerMassToCharge = "Mass/Charge",
            string headerFraction = "Fraction",
            string headerIntensity = "Intensity",
            bool useFactorials = false)
        {
            var computationStats = new ComputationStats();
            computationStats.Initialize();

            double nextComboFractionalAbundance = default;

            const string deuteriumEquiv = "^2.014H";

            long predictedConvIterations;

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
                short masterElementIndex;
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
                var workingFormulaMass = ParseFormulaPublic(ref formula, ref computationStats, false);

                if (workingFormulaMass < 0d)
                {
                    // Error occurred; information is stored in ErrorParams
                    results = LookupMessage(350) + ": " + LookupMessage(ErrorParams.ErrorId);
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
                    while (index <= formula.Length)
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
                    workingFormulaMass = ParseFormulaPublic(ref formula, ref computationStats, false);

                    if (workingFormulaMass < 0d)
                    {
                        // Error occurred; information is stored in ErrorParams
                        results = LookupMessage(350) + ": " + LookupMessage(ErrorParams.ErrorId);
                        return -1;
                    }
                }

                // Make sure there are no fractional atoms present (need to specially handle Deuterium)
                for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                {
                    count = computationStats.Elements[elementIndex].Count;
                    if (Math.Abs(count - (int)Math.Round(count)) > float.Epsilon)
                    {
                        results = LookupMessage(350) + ": " + LookupMessage(805) + ": " + ElementStats[elementIndex].Symbol + count;
                        return -1;
                    }
                }

                // Remove occurrences of explicitly defined isotopes from the formula
                for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                {
                    var element = computationStats.Elements[elementIndex];
                    if (element.IsotopeCount > 0)
                    {
                        explicitIsotopesPresent = true;
                        explicitIsotopeCount += element.IsotopeCount;
                        for (var isotopeIndex = 1; isotopeIndex <= element.IsotopeCount; isotopeIndex++)
                            element.Count -= element.Isotopes[isotopeIndex].Count;
                    }
                }

                // Determine the number of elements present in formula
                short elementCount = 0;
                for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                {
                    if (computationStats.Elements[elementIndex].Used)
                    {
                        elementCount = (short)(elementCount + 1);
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
                var isoStats = new IsoResultsByElement[elementCount + 1];
                for (var i = 0; i < isoStats.Length; i++)
                {
                    isoStats[i] = new IsoResultsByElement();
                }

                // Step through computationStats.Elements[] again and copy info into isoStats[]
                // In addition, determine minimum and maximum weight for the molecule
                elementCount = 0;
                var minWeight = 0;
                var maxWeight = 0;
                for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                {
                    if (computationStats.Elements[elementIndex].Used)
                    {
                        if (computationStats.Elements[elementIndex].Count > 0d)
                        {
                            elementCount = (short)(elementCount + 1);
                            isoStats[elementCount].ElementIndex = (short)elementIndex;
                            isoStats[elementCount].AtomCount = (int)Math.Round(computationStats.Elements[elementIndex].Count); // Note: Ignoring .Elements[elementIndex].IsotopicCorrection
                            isoStats[elementCount].ExplicitMass = ElementStats[elementIndex].Mass;

                            var stats = ElementStats[elementIndex];
                            minWeight = (int)Math.Round(minWeight + isoStats[elementCount].AtomCount * Math.Round(stats.Isotopes[1].Mass, 0));
                            maxWeight = (int)Math.Round(maxWeight + isoStats[elementCount].AtomCount * Math.Round(stats.Isotopes[stats.IsotopeCount].Mass, 0));
                        }
                    }
                }

                if (explicitIsotopesPresent)
                {
                    // Add the isotopes, pretending they are unique elements
                    for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                    {
                        var element = computationStats.Elements[elementIndex];
                        if (element.IsotopeCount > 0)
                        {
                            for (var isotopeIndex = 1; isotopeIndex <= element.IsotopeCount; isotopeIndex++)
                            {
                                elementCount = (short)(elementCount + 1);

                                isoStats[elementCount].ExplicitIsotope = true;
                                isoStats[elementCount].ElementIndex = (short)elementIndex;
                                isoStats[elementCount].AtomCount = (int)Math.Round(element.Isotopes[isotopeIndex].Count);
                                isoStats[elementCount].ExplicitMass = element.Isotopes[isotopeIndex].Mass;

                                var stats = isoStats[elementCount];
                                minWeight = (int)Math.Round(minWeight + stats.AtomCount * stats.ExplicitMass);
                                maxWeight = (int)Math.Round(maxWeight + stats.AtomCount * stats.ExplicitMass);
                            }
                        }
                    }
                }

                if (minWeight < 0)
                    minWeight = 0;

                // Create an array to hold the Fractional Abundances for all the masses
                convolutedMSDataCount = maxWeight - minWeight + 1;
                var convolutedAbundanceStartMass = minWeight;
                var convolutedAbundances = new IsoResultsOverallData[convolutedMSDataCount + 1]; // Fractional abundance at each mass; 1-based array

                for (var i = 0; i < convolutedAbundances.Length; i++)
                {
                    convolutedAbundances[i] = new IsoResultsOverallData();
                }

                // Predict the total number of computations required; show progress if necessary
                var predictedTotalComboCalcs = 0;
                for (var elementIndex = 1; elementIndex <= elementCount; elementIndex++)
                {
                    masterElementIndex = isoStats[elementIndex].ElementIndex;
                    atomCount = isoStats[elementIndex].AtomCount;
                    isotopeCount = ElementStats[masterElementIndex].IsotopeCount;

                    predictedCombos = FindCombosPredictIterations(atomCount, isotopeCount);
                    predictedTotalComboCalcs += predictedCombos;
                }

                ResetProgress("Finding Isotopic Abundances: Computing abundances");

                // For each element, compute all of the possible combinations
                var completedComboCalcs = 0;
                for (var elementIndex = 1; elementIndex <= elementCount; elementIndex++)
                {
                    short isotopeStartingMass;
                    short isotopeEndingMass;
                    masterElementIndex = isoStats[elementIndex].ElementIndex;
                    atomCount = isoStats[elementIndex].AtomCount;

                    if (isoStats[elementIndex].ExplicitIsotope)
                    {
                        isotopeCount = 1;
                        isotopeStartingMass = (short)Math.Round(isoStats[elementIndex].ExplicitMass);
                        isotopeEndingMass = isotopeStartingMass;
                    }
                    else
                    {
                        var stats = ElementStats[masterElementIndex];
                        isotopeCount = stats.IsotopeCount;
                        isotopeStartingMass = (short)Math.Round(Math.Round(stats.Isotopes[1].Mass, 0));
                        isotopeEndingMass = (short)Math.Round(Math.Round(stats.Isotopes[isotopeCount].Mass, 0));
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

                    var isoCombos = new int[predictedCombos + 1, isotopeCount + 1];
                    // 2D array: Holds the # of each isotope for each combination
                    // For example, Two chlorine atoms, Cl2, has at most 6 combos since Cl isotopes are 35, 36, and 37
                    // m1  m2  m3
                    // 2   0   0
                    // 1   1   0
                    // 1   0   1
                    // 0   2   0
                    // 0   1   1
                    // 0   0   2

                    var atomTrackHistory = new int[isotopeCount + 1];
                    atomTrackHistory[1] = atomCount;

                    var combosFound = FindCombosRecurse(ref isoCombos, atomCount, isotopeCount, isotopeCount, 1, 1, ref atomTrackHistory);

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
                    isoStats[elementIndex].StartingResultsMass = minWeight;
                    isoStats[elementIndex].ResultsCount = resultingMassCountForElement;
                    isoStats[elementIndex].MassAbundances = new float[resultingMassCountForElement + 1];

                    if (isoStats[elementIndex].ExplicitIsotope)
                    {
                        // Explicitly defined isotope; there is only one "combo" and its abundance = 1
                        isoStats[elementIndex].MassAbundances[1] = 1f;
                    }
                    else
                    {
                        var fractionalAbundanceSaved = 0d;
                        for (var comboIndex = 1; comboIndex <= combosFound; comboIndex++)
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

                                // AbundDenom  and  AbundSuffix are only needed if using the easily-overflowed factorial method
                                var abundDenom = 1d;
                                var abundSuffix = 1d;
                                var stats = ElementStats[masterElementIndex];
                                for (var isotopeIndex = 1; isotopeIndex <= isotopeCount; isotopeIndex++)
                                {
                                    var isotopeCountInThisCombo = isoCombos[comboIndex, isotopeIndex];
                                    if (isotopeCountInThisCombo > 0)
                                    {
                                        abundDenom *= Factorial((short)isotopeCountInThisCombo);
                                        abundSuffix *= Math.Pow(stats.Isotopes[isotopeIndex].Abundance, isotopeCountInThisCombo);
                                    }
                                }

                                thisComboFractionalAbundance = Factorial((short)atomCount) / abundDenom * abundSuffix;
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
                                    for (var isotopeIndex = 1; isotopeIndex <= isotopeCount; isotopeIndex++)
                                    {
                                        if (isoCombos[comboIndex, isotopeIndex] > 0)
                                        {
                                            var workingSum = 0d;
                                            for (var subIndex = 1; subIndex <= isoCombos[comboIndex, isotopeIndex]; subIndex++)
                                                workingSum += Math.Log(subIndex);

                                            sumI += workingSum;
                                        }
                                    }

                                    var stats = ElementStats[masterElementIndex];
                                    var sumF = 0d;
                                    for (var isotopeIndex = 1; isotopeIndex <= isotopeCount; isotopeIndex++)
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
                                if (comboIndex < combosFound && fractionalAbundanceSaved >= cutoffForRatioMethod)
                                {
                                    // #######
                                    // Third method, determines the ratio of this combo's abundance and the next combo's abundance
                                    // #######
                                    //
                                    var ratioOfFreqs = 1d;

                                    for (var isotopeIndex = 1; isotopeIndex <= isotopeCount; isotopeIndex++)
                                    {
                                        double m = isoCombos[comboIndex, isotopeIndex];
                                        double mPrime = isoCombos[comboIndex + 1, isotopeIndex];

                                        if (m > mPrime)
                                        {
                                            var logSigma = 0d;
                                            for (var subIndex = (int)Math.Round(mPrime) + 1; subIndex <= (int)Math.Round(m); subIndex++)
                                                logSigma += Math.Log(subIndex);

                                            logRho = logSigma - (m - mPrime) * Math.Log(ElementStats[masterElementIndex].Isotopes[isotopeIndex].Abundance);
                                        }
                                        else if (m < mPrime)
                                        {
                                            var logSigma = 0d;
                                            for (var subIndex = (int)Math.Round(m) + 1; subIndex <= (int)Math.Round(mPrime); subIndex++)
                                                logSigma += Math.Log(subIndex);

                                            var stats = ElementStats[masterElementIndex];
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
                                indexToStoreAbundance = FindIndexForNominalMass(ref isoCombos, comboIndex, isotopeCount, atomCount, ref ElementStats[masterElementIndex].Isotopes);

                                // Store the abundance in .MassAbundances[] at location IndexToStoreAbundance
                                isoStats[elementIndex].MassAbundances[indexToStoreAbundance] = (float)(isoStats[elementIndex].MassAbundances[indexToStoreAbundance] + thisComboFractionalAbundance);
                            }

                            if (ratioMethodUsed)
                            {
                                // Store abundance for next Combo
                                indexToStoreAbundance = FindIndexForNominalMass(ref isoCombos, comboIndex + 1, isotopeCount, atomCount, ref ElementStats[masterElementIndex].Isotopes);

                                // Store the abundance in .MassAbundances[] at location IndexToStoreAbundance
                                isoStats[elementIndex].MassAbundances[indexToStoreAbundance] = (float)(isoStats[elementIndex].MassAbundances[indexToStoreAbundance] + nextComboFractionalAbundance);
                            }

                            if (ratioMethodUsed && comboIndex + 1 == combosFound)
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
                for (var elementIndex = 1; elementIndex <= elementCount; elementIndex++)
                {
                    var stats = isoStats[elementIndex];
                    var index = stats.ResultsCount;
                    while (stats.MassAbundances[index] < minAbundanceToKeep)
                    {
                        index -= 1;
                        if (index == 1)
                            break;
                    }

                    stats.ResultsCount = index;
                }

                // Examine IsoStats[] to predict the number of ConvolutionIterations
                predictedConvIterations = isoStats[1].ResultsCount;
                for (var elementIndex = 2; elementIndex <= elementCount; elementIndex++)
                    predictedConvIterations *= isoStats[2].ResultsCount;

                ResetProgress("Finding Isotopic Abundances: Convoluting results");

                // Convolute the results for each element using a recursive convolution routine
                var convolutionIterations = 0L;
                for (var index = 1; index <= isoStats[1].ResultsCount; index++)
                {
                    ConvoluteMasses(ref convolutedAbundances, convolutedAbundanceStartMass, index, 1f, 0, 1, ref isoStats, elementCount, ref convolutionIterations);

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
                for (var elementIndex = 1; elementIndex <= elementCount; elementIndex++)
                {
                    var stats = isoStats[elementIndex];
                    if (stats.ExplicitIsotope)
                    {
                        exactBaseIsoMass += stats.AtomCount * stats.ExplicitMass;
                    }
                    else
                    {
                        exactBaseIsoMass += stats.AtomCount * ElementStats[stats.ElementIndex].Isotopes[1].Mass;
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
                for (var massIndex = convolutedMSDataCount; massIndex >= 1; massIndex -= 1)
                {
                    if (convolutedAbundances[massIndex].Abundance > minAbundanceToKeep)
                    {
                        convolutedMSDataCount = massIndex;
                        break;
                    }
                }

                var output = headerIsotopicAbundances + " " + formula + Environment.NewLine;
                output = output + SpacePad("  " + headerMassToCharge, 12) + "\t" + SpacePad(headerFraction, 9) + "\t" + headerIntensity + Environment.NewLine;

                // Initialize convolutedMSData2DOneBased[]
                convolutedMSData2DOneBased = new double[convolutedMSDataCount + 1, 3];

                // Find Maximum Abundance
                var maxAbundance = 0d;
                for (var massIndex = 1; massIndex <= convolutedMSDataCount; massIndex++)
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
                for (var massIndex = 1; massIndex <= convolutedMSDataCount; massIndex++)
                {
                    var mass = convolutedAbundances[massIndex];
                    convolutedMSData2DOneBased[massIndex, 0] = convolutedAbundanceStartMass + massIndex - 1 + massDefect;
                    convolutedMSData2DOneBased[massIndex, 1] = mass.Abundance / maxAbundance * 100d;

                    if (chargeState >= 1)
                    {
                        if (addProtonChargeCarrier)
                        {
                            convolutedMSData2DOneBased[massIndex, 0] = ConvoluteMassInternal(convolutedMSData2DOneBased[massIndex, 0], 0, chargeState);
                        }
                        else
                        {
                            convolutedMSData2DOneBased[massIndex, 0] = convolutedMSData2DOneBased[massIndex, 0] / chargeState;
                        }
                    }
                }

                // Step through convolutedMSData2DOneBased[] from the beginning to find the
                // first value greater than MIN_ABUNDANCE_TO_KEEP
                var rowIndex = 1;
                while (convolutedMSData2DOneBased[rowIndex, 1] < minAbundanceToKeep)
                {
                    rowIndex += 1;
                    if (rowIndex == convolutedMSDataCount)
                        break;
                }

                // If rowIndex > 1 then remove rows from beginning since value is less than MIN_ABUNDANCE_TO_KEEP
                if (rowIndex > 1 && rowIndex < convolutedMSDataCount)
                {
                    rowIndex -= 1;
                    // Remove rows from the start of convolutedMSData2DOneBased[] since 0 mass
                    for (var massIndex = rowIndex + 1; massIndex <= convolutedMSDataCount; massIndex++)
                    {
                        convolutedMSData2DOneBased[massIndex - rowIndex, 0] = convolutedMSData2DOneBased[massIndex, 0];
                        convolutedMSData2DOneBased[massIndex - rowIndex, 1] = convolutedMSData2DOneBased[massIndex, 1];
                    }

                    convolutedMSDataCount -= rowIndex;
                }

                // Write to output
                for (var massIndex = 1; massIndex <= convolutedMSDataCount; massIndex++)
                {
                    output = output + SpacePadFront(convolutedMSData2DOneBased[massIndex, 0].ToString("#0.00000"), 12) + "\t";
                    output = output + (convolutedMSData2DOneBased[massIndex, 1] * maxAbundance / 100d).ToString("0.0000000") + "\t";
                    output = output + SpacePadFront(convolutedMSData2DOneBased[massIndex, 1].ToString("##0.00"), 7) + Environment.NewLine;
                    //ToDo: Fix Multiplicity
                    //output = output + ConvolutedAbundances(massIndex).Multiplicity.ToString("##0") + Environment.NewLine
                }

                results = output;
            }
            catch
            {
                MwtWinDllErrorHandler("MwtWinDll|ComputeIsotopicAbundances");
                ErrorParams.ErrorId = 590;
                ErrorParams.ErrorPosition = 0;
                return -1;
            }

            return 0; // Success
        }

        /// <summary>
        /// Compute percent composition of the elements defined in <paramref name="computationStats"/>
        /// </summary>
        /// <param name="computationStats">Input/output</param>
        public void ComputePercentComposition(ref ComputationStats computationStats)
        {
            // Determine the number of elements in the formula
            for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
            {
                if (computationStats.TotalMass > 0d)
                {
                    var elementTotalMass = computationStats.Elements[elementIndex].Count * ElementStats[elementIndex].Mass + computationStats.Elements[elementIndex].IsotopicCorrection;

                    // Percent is the percent composition
                    var percentComp = elementTotalMass / computationStats.TotalMass * 100.0d;
                    computationStats.PercentCompositions[elementIndex].PercentComposition = percentComp;

                    // Calculate standard deviation
                    double stdDeviation;
                    if (Math.Abs(computationStats.Elements[elementIndex].IsotopicCorrection - 0d) < float.Epsilon)
                    {
                        // No isotopic mass correction factor exists
                        stdDeviation = percentComp * Math.Sqrt(Math.Pow(ElementStats[elementIndex].Uncertainty / ElementStats[elementIndex].Mass, 2d) + Math.Pow(computationStats.StandardDeviation / computationStats.TotalMass, 2d));
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

                    if (appendNewData == true)
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

                foreach (var item in xySummation)
                    gaussianData.Add(new KeyValuePair<double, double>(item.X, item.Y));
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

            MasterSymbolsList = new string[ELEMENT_COUNT + AbbrevAllCount + 1, 2];

            // MasterSymbolsList[,0] contains the symbol to be matched
            // MasterSymbolsList[,1] contains E for element, A for amino acid, or N for normal abbreviation, followed by
            // the reference number in the master list
            // For example for Carbon, MasterSymbolsList[index,0] = "C" and MasterSymbolsList[index,1] = "E6"

            // Construct search list
            for (var index = 1; index <= ELEMENT_COUNT; index++)
            {
                MasterSymbolsList[index, 0] = ElementStats[index].Symbol;
                MasterSymbolsList[index, 1] = "E" + index;
            }

            MasterSymbolsListCount = ELEMENT_COUNT;

            // Note: AbbrevStats is 1-based
            if (gComputationOptions.AbbrevRecognitionMode != MolecularWeightTool.AbbrevRecognitionMode.NoAbbreviations)
            {
                bool includeAmino;
                if (gComputationOptions.AbbrevRecognitionMode == MolecularWeightTool.AbbrevRecognitionMode.NormalPlusAminoAcids)
                {
                    includeAmino = true;
                }
                else
                {
                    includeAmino = false;
                }

                for (var index = 1; index <= AbbrevAllCount; index++)
                {
                    var stats = AbbrevStats[index];
                    // If includeAmino = False then do not include amino acids
                    if (includeAmino || !includeAmino && !stats.IsAminoAcid)
                    {
                        // Do not include if the formula is invalid
                        if (!stats.InvalidSymbolOrFormula)
                        {
                            MasterSymbolsListCount = (short)(MasterSymbolsListCount + 1);

                            MasterSymbolsList[MasterSymbolsListCount, 0] = stats.Symbol;
                            MasterSymbolsList[MasterSymbolsListCount, 1] = "A" + index;
                        }
                    }
                }
            }

            // Sort the search list
            ShellSortSymbols(1, MasterSymbolsListCount);
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
            computationStats.Initialize();

            // Call ParseFormulaPublic to compute the formula's mass and fill computationStats
            var mass = ParseFormulaPublic(ref formula, ref computationStats);

            if (ErrorParams.ErrorId == 0)
            {
                // Convert to empirical formula
                var empiricalFormula = "";
                // Carbon first, then hydrogen, then the rest alphabetically
                // This is correct to start at -1
                for (var elementIndex = -1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                {
                    var elementIndexToUse = default(int);
                    if (elementIndex == -1)
                    {
                        // Do Carbon first
                        elementIndexToUse = 6;
                    }
                    else if (elementIndex == 0)
                    {
                        // Then do Hydrogen
                        elementIndexToUse = 1;
                    }
                    else
                    {
                        // Then do the rest alphabetically
                        if (ElementAlph[elementIndex] == "C" || ElementAlph[elementIndex] == "H")
                        {
                            // Increment elementIndex when we encounter carbon or hydrogen
                            elementIndex = (short)(elementIndex + 1);
                        }

                        for (var elementSearchIndex = 2; elementSearchIndex <= ELEMENT_COUNT; elementSearchIndex++) // Start at 2 to since we've already done hydrogen
                        {
                            // find the element in the numerically ordered array that corresponds to the alphabetically ordered array
                            if ((ElementStats[elementSearchIndex].Symbol ?? "") == (ElementAlph[elementIndex] ?? ""))
                            {
                                elementIndexToUse = elementSearchIndex;
                                break;
                            }
                        }
                    }

                    // Only display the element if it's in the formula
                    var thisElementCount = mComputationStatsSaved.Elements[elementIndexToUse].Count;
                    if (Math.Abs(thisElementCount - 1.0d) < float.Epsilon)
                    {
                        empiricalFormula += ElementStats[elementIndexToUse].Symbol;
                    }
                    else if (thisElementCount > 0d)
                    {
                        empiricalFormula = empiricalFormula + ElementStats[elementIndexToUse].Symbol + thisElementCount;
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
            computationStats.Initialize();

            // Call ExpandAbbreviationsInFormula to compute the formula's mass
            var mass = ParseFormulaPublic(ref formula, ref computationStats, true);

            if (ErrorParams.ErrorId == 0)
            {
                return formula;
            }

            return (-1).ToString();
        }

        private int FindIndexForNominalMass(
            ref int[,] isoCombos,
            int comboIndex,
            short isotopeCount,
            int atomCount,
            ref IsotopeInfo[] thisElementsIsotopes)
        {
            var workingMass = 0;
            for (var isotopeIndex = 1; isotopeIndex <= isotopeCount; isotopeIndex++)
                workingMass = (int)Math.Round(workingMass + isoCombos[comboIndex, isotopeIndex] * Math.Round(thisElementsIsotopes[isotopeIndex].Mass, 0));

            // (workingMass  - IsoStats(ElementIndex).StartingResultsMass) + 1
            return (int)Math.Round(workingMass - atomCount * Math.Round(thisElementsIsotopes[1].Mass, 0)) + 1;
        }

        /// <summary>
        /// Recursive function to Convolute the Results in <paramref name="isoStats"/> and store in <paramref name="convolutedAbundances"/>; 1-based array
        /// </summary>
        /// <param name="convolutedAbundances"></param>
        /// <param name="convolutedAbundanceStartMass"></param>
        /// <param name="workingRow"></param>
        /// <param name="workingAbundance"></param>
        /// <param name="workingMassTotal"></param>
        /// <param name="elementTrack"></param>
        /// <param name="isoStats"></param>
        /// <param name="elementCount"></param>
        /// <param name="iterations"></param>
        private void ConvoluteMasses(
            ref IsoResultsOverallData[] convolutedAbundances,
            int convolutedAbundanceStartMass,
            int workingRow,
            float workingAbundance,
            int workingMassTotal,
            short elementTrack,
            ref IsoResultsByElement[] isoStats,
            short elementCount,
            ref long iterations)
        {
            if (mAbortProcessing)
                return;

            iterations += 1L;
            if (iterations % 10000L == 0L)
            {
                Application.DoEvents();
            }

            var newAbundance = workingAbundance * isoStats[elementTrack].MassAbundances[workingRow];
            var newMassTotal = workingMassTotal + (isoStats[elementTrack].StartingResultsMass + workingRow - 1);

            if (elementTrack >= elementCount)
            {
                var indexToStoreResult = newMassTotal - convolutedAbundanceStartMass + 1;
                var result = convolutedAbundances[indexToStoreResult];
                if (newAbundance > 0f)
                {
                    result.Abundance += newAbundance;
                    result.Multiplicity += 1;
                }
            }
            else
            {
                for (var rowIndex = 1; rowIndex <= isoStats[elementTrack + 1].ResultsCount; rowIndex++)
                    ConvoluteMasses(ref convolutedAbundances, convolutedAbundanceStartMass, rowIndex, newAbundance, newMassTotal, (short)(elementTrack + 1), ref isoStats, elementCount, ref iterations);
            }
        }

        /// <summary>
        /// Compute the factorial of a number; uses recursion
        /// </summary>
        /// <param name="number">Integer number between 0 and 170</param>
        /// <returns>The factorial, or -1 if an error</returns>
        /// <remarks></remarks>
        public double Factorial(short number)
        {
            try
            {
                if (number > 170)
                {
                    Console.WriteLine("Cannot compute factorial of a number over 170");
                    return -1;
                }

                if (number < 0)
                {
                    Console.WriteLine("Cannot compute factorial of a negative number");
                    return -1;
                }

                if (number == 0)
                {
                    return 1d;
                }

                return number * Factorial((short)(number - 1));
            }
            catch
            {
                Console.WriteLine("Number too large");
                return -1;
            }
        }

        // Note: This function is unused
        //Private Function FindCombinations(Optional ByRef AtomCount As Integer = 2, Optional ByRef IsotopeCount As Short = 2, Optional ByRef boolPrintOutput As Boolean = False) As Integer
        //    ' Find Combinations of atoms for a given number of atoms and number of potential isotopes
        //    ' Can print results to debug window

        //    Dim ComboResults(,) As Integer
        //    Dim AtomTrackHistory() As Integer
        //    Dim PredictedCombos, CombosFound As Integer

        //    Dim strMessage As String

        //    PredictedCombos = FindCombosPredictIterations(AtomCount, IsotopeCount)

        //    If PredictedCombos > 10000000 Then
        //        strMessage = "Too many combinations necessary for prediction of isotopic distribution: " & PredictedCombos.ToString("#,##0") & Environment.NewLine & "Please use a simpler formula or reduce the isotopic range defined for the element (currently " & IsotopeCount & ")"
        //        If mShowErrorMessageDialogs Then
        //            MsgBox(strMessage)
        //        End If
        //        LogMessage(strMessage, eMessageTypeConstants.ErrorMsg)
        //        Return -1
        //    End If

        //    Try
        //        ReDim ComboResults(PredictedCombos, IsotopeCount)

        //        ReDim AtomTrackHistory(IsotopeCount)
        //        AtomTrackHistory(1) = AtomCount

        //        CombosFound = FindCombosRecurse(ComboResults, AtomCount, IsotopeCount, IsotopeCount, 1, 1, AtomTrackHistory)

        //        Dim strOutput, strHeader As String
        //        Dim RowIndex As Integer
        //        Dim ColIndex As Short
        //        If boolPrintOutput Then

        //            strHeader = CombosFound & " combos found for " & AtomCount & " atoms for element with " & IsotopeCount & " isotopes"
        //            If CombosFound > 5000 Then
        //                strHeader = strHeader & Environment.NewLine & "Only displaying the first 5000 combinations"
        //            End If

        //            System.Diagnostics.Debug.WriteLine(strHeader)

        //            For RowIndex = 1 To CombosFound
        //                strOutput = ""
        //                For ColIndex = 1 To IsotopeCount
        //                    strOutput = strOutput & ComboResults(RowIndex, ColIndex) & vbTab
        //                Next ColIndex
        //                System.Diagnostics.Debug.WriteLine(strOutput)
        //                If RowIndex > 5000 Then Exit For
        //            Next RowIndex

        //            If CombosFound > 50 Then System.Diagnostics.Debug.WriteLine(strHeader)

        //        End If

        //        Return CombosFound
        //    Catch ex As Exception
        //        MwtWinDllErrorHandler("MwtWinDll|FindCombinations")
        //        ErrorParams.ErrorID = 590
        //        ErrorParams.ErrorPosition = 0
        //        Return -1
        //    End Try

        //End Function

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

            var runningSum = new int[atomCount + 1];
            try
            {
                int predictedCombos;
                if (atomCount == 1 || isotopeCount == 1)
                {
                    predictedCombos = isotopeCount;
                }
                else
                {
                    // Initialize RunningSum()
                    for (var atomIndex = 1; atomIndex <= atomCount; atomIndex++)
                        runningSum[atomIndex] = atomIndex + 1;

                    for (var isotopeIndex = 3; isotopeIndex <= isotopeCount; isotopeIndex++)
                    {
                        var previousComputedValue = isotopeIndex;
                        for (var atomIndex = 2; atomIndex <= atomCount; atomIndex++)
                        {
                            // Compute new count for this AtomIndex
                            runningSum[atomIndex] = previousComputedValue + runningSum[atomIndex];

                            // Also place result in RunningSum(AtomIndex)
                            previousComputedValue = runningSum[atomIndex];
                        }
                    }

                    predictedCombos = runningSum[atomCount];
                }

                return predictedCombos;
            }
            catch
            {
                MwtWinDllErrorHandler("MwtWinDll|FindCombosPredictIterations");
                ErrorParams.ErrorId = 590;
                ErrorParams.ErrorPosition = 0;
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
        /// <returns></returns>
        private int FindCombosRecurse(
            ref int[,] comboResults,
            int atomCount,
            short maxIsotopeCount,
            short currentIsotopeCount,
            int currentRow,
            short currentCol,
            ref int[] atomTrackHistory)
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

                    // Went to a new row; if CurrentCol > 1 then need to assign previous values to previous columns
                    if (currentCol > 1)
                    {
                        for (var colIndex = 1; colIndex < currentCol; colIndex++)
                            comboResults[currentRow, colIndex] = atomTrackHistory[colIndex];
                    }

                    atomTrack -= 1;
                    comboResults[currentRow, currentCol] = atomTrack;

                    if (currentCol < maxIsotopeCount)
                    {
                        var newColumn = (short)(currentCol + 1);
                        atomTrackHistory[newColumn - 1] = atomTrack;
                        currentRow = FindCombosRecurse(ref comboResults, atomCount - atomTrack, maxIsotopeCount, (short)(currentIsotopeCount - 1), currentRow, newColumn, ref atomTrackHistory);
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
            GeneralErrorHandler(callingProcedure, 0, ex.Message);
        }

        public void GeneralErrorHandler(string callingProcedure, int errorNumber, string errorDescriptionAdditional = "")
        {
            var message = "Error in " + callingProcedure + ": " + Conversion.ErrorToString(errorNumber) + " (#" + errorNumber + ")";
            if (!string.IsNullOrEmpty(errorDescriptionAdditional))
            {
                message += Environment.NewLine + errorDescriptionAdditional;
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
            return AbbrevAllCount;
        }

        /// <summary>
        /// Get the abbreviation ID for the given abbreviation symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="aminoAcidsOnly"></param>
        /// <returns>ID if found, otherwise 0</returns>
        public int GetAbbreviationIdInternal(string symbol, bool aminoAcidsOnly = false)
        {
            for (var index = 1; index <= AbbrevAllCount; index++)
            {
                if ((AbbrevStats[index].Symbol?.ToLower() ?? "") == (symbol?.ToLower() ?? ""))
                {
                    if (!aminoAcidsOnly || aminoAcidsOnly && AbbrevStats[index].IsAminoAcid)
                    {
                        return index;
                    }
                }
            }

            return 0;
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
            if (abbreviationId >= 1 && abbreviationId <= AbbrevAllCount)
            {
                var stats = AbbrevStats[abbreviationId];
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

            if (abbreviationId >= 1 && abbreviationId <= AbbrevAllCount)
            {
                return AbbrevStats[abbreviationId].Mass;
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
            for (var index = 1; index <= AbbrevAllCount; index++)
            {
                if (AbbrevStats[index].IsAminoAcid)
                {
                    string compareSymbol;
                    if (oneLetterTo3Letter)
                    {
                        compareSymbol = AbbrevStats[index].OneLetterSymbol;
                    }
                    else
                    {
                        compareSymbol = AbbrevStats[index].Symbol;
                    }

                    if ((compareSymbol?.ToLower() ?? "") == (symbolToFind?.ToLower() ?? ""))
                    {
                        if (oneLetterTo3Letter)
                        {
                            returnSymbol = AbbrevStats[index].Symbol;
                        }
                        else
                        {
                            returnSymbol = AbbrevStats[index].OneLetterSymbol;
                        }

                        break;
                    }
                }
            }

            return returnSymbol;
        }

        public int GetCautionStatementCountInternal()
        {
            return CautionStatementCount;
        }

        /// <summary>
        /// Get the caution statement ID for the given symbol combo
        /// </summary>
        /// <param name="symbolCombo"></param>
        /// <returns>Statement ID if found, otherwise -1</returns>
        public int GetCautionStatementIdInternal(string symbolCombo)
        {
            for (var index = 1; index <= CautionStatementCount; index++)
            {
                if ((CautionStatements[index, 0] ?? "") == (symbolCombo ?? ""))
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get a caution statement, by ID
        /// </summary>
        /// <param name="cautionStatementId"></param>
        /// <param name="symbolCombo">Output: symbol combo for the caution statement</param>
        /// <param name="cautionStatement">Output: caution statement text</param>
        /// <returns>0 if success, 1 if an invalid ID</returns>
        public int GetCautionStatementInternal(int cautionStatementId, out string symbolCombo, out string cautionStatement)
        {
            if (cautionStatementId >= 1 && cautionStatementId <= CautionStatementCount)
            {
                symbolCombo = CautionStatements[cautionStatementId, 0];
                cautionStatement = CautionStatements[cautionStatementId, 1];
                return 0;
            }

            symbolCombo = string.Empty;
            cautionStatement = string.Empty;
            return 1;
        }

        public string GetCautionDescription()
        {
            return mStrCautionDescription;
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
                symbol = ElementAlph[elementId];
                var stats = ElementStats[elementId];
                symbol = stats.Symbol;
                mass = stats.Mass;
                uncertainty = stats.Uncertainty;
                charge = stats.Charge;
                isotopeCount = stats.IsotopeCount;

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
                if (string.Equals(ElementStats[index].Symbol, symbol, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (short)index;
                }
            }

            return 0;
        }

        /// <summary>
        /// Returns the isotope masses and abundances for the element with <paramref name="elementId"/>
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses"></param>
        /// <param name="isotopeAbundances"></param>
        /// <returns>0 if a valid ID, 1 if invalid</returns>
        public int GetElementIsotopesInternal(short elementId, ref short isotopeCount, ref double[] isotopeMasses, ref float[] isotopeAbundances)
        {
            if (elementId >= 1 && elementId <= ELEMENT_COUNT)
            {
                var stats = ElementStats[elementId];
                isotopeCount = stats.IsotopeCount;
                for (var isotopeIndex = 1; isotopeIndex <= stats.IsotopeCount; isotopeIndex++)
                {
                    isotopeMasses[isotopeIndex] = stats.Isotopes[isotopeIndex].Mass;
                    isotopeAbundances[isotopeIndex] = stats.Isotopes[isotopeIndex].Abundance;
                }

                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Get the current element mode
        /// </summary>
        /// <returns>
        /// emAverageMass  = 1
        /// emIsotopicMass = 2
        /// emIntegerMass  = 3
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
                return ElementStats[elementId].Symbol;
            }

            return "";
        }

        public List<ElementInfo> GetElements()
        {
            return ElementStats.ToList();
        }

        /// <summary>
        /// Returns a single bit of information about a single element
        /// </summary>
        /// <param name="elementId">Element ID</param>
        /// <param name="elementStat">Value to obtain: mass, charge, or uncertainty</param>
        /// <returns></returns>
        /// <remarks>Since a value may be negative, simply returns 0 if an error</remarks>
        public double GetElementStatInternal(short elementId, MolecularWeightTool.ElementStatsType elementStat)
        {
            if (elementId >= 1 && elementId <= ELEMENT_COUNT)
            {
                switch (elementStat)
                {
                    case MolecularWeightTool.ElementStatsType.Mass:
                        return ElementStats[elementId].Mass;
                    case MolecularWeightTool.ElementStatsType.Charge:
                        return ElementStats[elementId].Charge;
                    case MolecularWeightTool.ElementStatsType.Uncertainty:
                        return ElementStats[elementId].Uncertainty;
                    default:
                        return 0d;
                }
            }

            return 0d;
        }

        public string GetErrorDescription()
        {
            if (ErrorParams.ErrorId != 0)
            {
                return LookupMessage(ErrorParams.ErrorId);
            }

            return "";
        }

        public int GetErrorId()
        {
            return ErrorParams.ErrorId;
        }

        public string GetErrorCharacter()
        {
            return ErrorParams.ErrorCharacter;
        }

        public int GetErrorPosition()
        {
            return ErrorParams.ErrorPosition;
        }

        public int GetMessageStatementCountInternal()
        {
            return MessageStatementCount;
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
            if (messageId > 0 && messageId <= MessageStatementCount)
            {
                var message = MessageStatements[messageId];

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

                // Now append appendText
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
        private bool IsPresentInAbbrevSymbolStack(ref AbbrevSymbolStack abbrevSymbolStack, short symbolReference)
        {
            try
            {
                var found = false;
                for (var index = 0; index < abbrevSymbolStack.Count; index++)
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
            // Assume true until proven otherwise
            var allLetters = true;
            for (var index = 0; index < test.Length; index++)
            {
                if (!char.IsLetter(test[index]))
                {
                    allLetters = false;
                    break;
                }
            }

            return allLetters;
        }

        public bool IsValidElementSymbol(string elementSymbol, bool caseSensitive = true)
        {
            if (caseSensitive)
            {
                var query = from item in ElementStats where (item.Symbol ?? "") == (elementSymbol ?? "") select item;
                return query.Any();
            }
            else
            {
                var query = from item in ElementStats where (item.Symbol?.ToLower() ?? "") == (elementSymbol?.ToLower() ?? "") select item;
                return query.Any();
            }
        }

        protected void LogMessage(string message, MessageType messageType = MessageType.Normal)
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

            string messageTypeText;

            switch (messageType)
            {
                case MessageType.Normal:
                    messageTypeText = "Normal";
                    break;
                case MessageType.Error:
                    messageTypeText = "Error";
                    break;
                case MessageType.Warning:
                    messageTypeText = "Warning";
                    break;
                default:
                    messageTypeText = "Unknown";
                    break;
            }

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
            for (var index = 1; index <= CautionStatementCount; index++)
            {
                if ((compareText ?? "") == (CautionStatements[index, 0] ?? ""))
                {
                    return CautionStatements[index, 1];
                }
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
            if (MessageStatementCount == 0)
                MemoryLoadMessageStatements();

            // First assume we can't find the message number
            var message = "General unspecified error";

            // Now try to find it
            if (messageId < MESSAGE_STATEMENT_DIM_COUNT)
            {
                if (MessageStatements[messageId].Length > 0)
                {
                    message = MessageStatements[messageId];
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

            // Now append appendText
            message += appendText;

            // messageId's 1 and 18 may need to have an addendum added
            if (messageId == 1)
            {
                if (gComputationOptions.CaseConversion == CaseConversionMode.ExactCase)
                {
                    message = message + " (" + LookupMessage(680) + ")";
                }
            }
            else if (messageId == 18)
            {
                if (!gComputationOptions.BracketsAsParentheses)
                {
                    message = message + " (" + LookupMessage(685) + ")";
                }
                else
                {
                    message = message + " (" + LookupMessage(690) + ")";
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
            MemoryLoadElements(elementMode);

            // Reconstruct master symbols list
            ConstructMasterSymbolsList();

            MemoryLoadIsotopes();

            MemoryLoadAbbreviations();

            // Reconstruct master symbols list
            ConstructMasterSymbolsList();

            MemoryLoadCautionStatements();

            MemoryLoadMessageStatements();
        }

        public void MemoryLoadAbbreviations()
        {
            // Symbol                            Formula            1 letter abbreviation
            const short aminoAbbrevCount = 28;

            AbbrevAllCount = aminoAbbrevCount;
            for (var index = 1; index <= AbbrevAllCount; index++)
                AbbrevStats[index].IsAminoAcid = true;

            AddAbbreviationWork(1, "Ala", "C3H5NO", 0f, true, "A", "Alanine");
            AddAbbreviationWork(2, "Arg", "C6H12N4O", 0f, true, "R", "Arginine, (unprotonated NH2)");
            AddAbbreviationWork(3, "Asn", "C4H6N2O2", 0f, true, "N", "Asparagine");
            AddAbbreviationWork(4, "Asp", "C4H5NO3", 0f, true, "D", "Aspartic acid (undissociated COOH)");
            AddAbbreviationWork(5, "Cys", "C3H5NOS", 0f, true, "C", "Cysteine (no disulfide link)");
            AddAbbreviationWork(6, "Gla", "C6H7NO5", 0f, true, "U", "gamma-Carboxyglutamate");
            AddAbbreviationWork(7, "Gln", "C5H8N2O2", 0f, true, "Q", "Glutamine");
            AddAbbreviationWork(8, "Glu", "C5H7NO3", 0f, true, "E", "Glutamic acid (undissociated COOH)");
            AddAbbreviationWork(9, "Gly", "C2H3NO", 0f, true, "G", "Glycine");
            AddAbbreviationWork(10, "His", "C6H7N3O", 0f, true, "H", "Histidine (unprotonated NH)");
            AddAbbreviationWork(11, "Hse", "C4H7NO2", 0f, true, "", "Homoserine");
            AddAbbreviationWork(12, "Hyl", "C6H12N2O2", 0f, true, "", "Hydroxylysine");
            AddAbbreviationWork(13, "Hyp", "C5H7NO2", 0f, true, "", "Hydroxyproline");
            AddAbbreviationWork(14, "Ile", "C6H11NO", 0f, true, "I", "Isoleucine");
            AddAbbreviationWork(15, "Leu", "C6H11NO", 0f, true, "L", "Leucine");
            AddAbbreviationWork(16, "Lys", "C6H12N2O", 0f, true, "K", "Lysine (unprotonated NH2)");
            AddAbbreviationWork(17, "Met", "C5H9NOS", 0f, true, "M", "Methionine");
            AddAbbreviationWork(18, "Orn", "C5H10N2O", 0f, true, "O", "Ornithine");
            AddAbbreviationWork(19, "Phe", "C9H9NO", 0f, true, "F", "Phenylalanine");
            AddAbbreviationWork(20, "Pro", "C5H7NO", 0f, true, "P", "Proline");
            AddAbbreviationWork(21, "Pyr", "C5H5NO2", 0f, true, "", "Pyroglutamic acid");
            AddAbbreviationWork(22, "Sar", "C3H5NO", 0f, true, "", "Sarcosine");
            AddAbbreviationWork(23, "Ser", "C3H5NO2", 0f, true, "S", "Serine");
            AddAbbreviationWork(24, "Thr", "C4H7NO2", 0f, true, "T", "Threonine");
            AddAbbreviationWork(25, "Trp", "C11H10N2O", 0f, true, "W", "Tryptophan");
            AddAbbreviationWork(26, "Tyr", "C9H9NO2", 0f, true, "Y", "Tyrosine");
            AddAbbreviationWork(27, "Val", "C5H9NO", 0f, true, "V", "Valine");
            AddAbbreviationWork(28, "Xxx", "C6H12N2O", 0f, true, "X", "Unknown");

            const short normalAbbrevCount = 16;
            AbbrevAllCount += normalAbbrevCount;
            for (var index = aminoAbbrevCount + 1; index <= AbbrevAllCount; index++)
                AbbrevStats[index].IsAminoAcid = false;

            AddAbbreviationWork(aminoAbbrevCount + 1, "Bpy", "C10H8N2", 0f, false, "", "Bipyridine");
            AddAbbreviationWork(aminoAbbrevCount + 2, "Bu", "C4H9", 1f, false, "", "Butyl");
            AddAbbreviationWork(aminoAbbrevCount + 3, "D", "^2.014H", 1f, false, "", "Deuterium");
            AddAbbreviationWork(aminoAbbrevCount + 4, "En", "C2H8N2", 0f, false, "", "Ethylenediamine");
            AddAbbreviationWork(aminoAbbrevCount + 5, "Et", "CH3CH2", 1f, false, "", "Ethyl");
            AddAbbreviationWork(aminoAbbrevCount + 6, "Me", "CH3", 1f, false, "", "Methyl");
            AddAbbreviationWork(aminoAbbrevCount + 7, "Ms", "CH3SOO", -1, false, "", "Mesyl");
            AddAbbreviationWork(aminoAbbrevCount + 8, "Oac", "C2H3O2", -1, false, "", "Acetate");
            AddAbbreviationWork(aminoAbbrevCount + 9, "Otf", "OSO2CF3", -1, false, "", "Triflate");
            AddAbbreviationWork(aminoAbbrevCount + 10, "Ox", "C2O4", -2, false, "", "Oxalate");
            AddAbbreviationWork(aminoAbbrevCount + 11, "Ph", "C6H5", 1f, false, "", "Phenyl");
            AddAbbreviationWork(aminoAbbrevCount + 12, "Phen", "C12H8N2", 0f, false, "", "Phenanthroline");
            AddAbbreviationWork(aminoAbbrevCount + 13, "Py", "C5H5N", 0f, false, "", "Pyridine");
            AddAbbreviationWork(aminoAbbrevCount + 14, "Tpp", "(C4H2N(C6H5C)C4H2N(C6H5C))2", 0f, false, "", "Tetraphenylporphyrin");
            AddAbbreviationWork(aminoAbbrevCount + 15, "Ts", "CH3C6H4SOO", -1, false, "", "Tosyl");
            AddAbbreviationWork(aminoAbbrevCount + 16, "Urea", "H2NCONH2", 0f, false, "", "Urea");

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
            CautionStatementCount = ElementAndMassInMemoryData.MemoryLoadCautionStatementsEnglish(ref CautionStatements);
        }

        /// <summary>
        /// Load elements
        /// </summary>
        /// <param name="elementMode">Element mode: 1 for average weights, 2 for monoisotopic weights, 3 for integer weights</param>
        /// <param name="specificElement"></param>
        /// <param name="specificStatToReset"></param>
        /// <remarks>
        /// <paramref name="specificElement"/> and <paramref name="specificStatToReset"/> are zero when updating all of the elements
        /// nonzero <paramref name="specificElement"/> and <paramref name="specificStatToReset"/> values will set just that specific value to the default
        /// </remarks>
        public void MemoryLoadElements(
            ElementMassMode elementMode,
            short specificElement = 0,
            MolecularWeightTool.ElementStatsType specificStatToReset = MolecularWeightTool.ElementStatsType.Mass)
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
            // Note: I could make this array of type ElementInfo, but the size of this sub would increase dramatically
            ElementAndMassInMemoryData.MemoryLoadElements(elementMode, out var elementNames, out var elemVals);

            if (specificElement == 0)
            {
                // Updating all the elements
                for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                {
                    var stats = ElementStats[elementIndex];
                    stats.Symbol = elementNames[elementIndex];
                    stats.Mass = elemVals[elementIndex, 1];
                    stats.Uncertainty = elemVals[elementIndex, 2];
                    stats.Charge = (float)elemVals[elementIndex, 3];

                    ElementAlph[elementIndex] = stats.Symbol;
                }

                // Alphabetize ElementAlph[] array via built-in sort; no custom comparator needed.
                Array.Sort(ElementAlph);

                //// Alphabetize ElementAlph[] array via bubble sort
                //for (var compareIndex = ELEMENT_COUNT; compareIndex >= 2; compareIndex += -1) // Sort from end to start
                //{
                //    for (var index = 1; index < compareIndex; index++)
                //    {
                //        if (string.Compare(ElementAlph[index], ElementAlph[index + 1], StringComparison.Ordinal) > 0)
                //        {
                //            // Swap them
                //            var swap = ElementAlph[index];
                //            ElementAlph[index] = ElementAlph[index + 1];
                //            ElementAlph[index + 1] = swap;
                //        }
                //    }
                //}
            }
            else if (specificElement >= 1 && specificElement <= ELEMENT_COUNT)
            {
                var stats = ElementStats[specificElement];
                switch (specificStatToReset)
                {
                    case MolecularWeightTool.ElementStatsType.Mass:
                        stats.Mass = elemVals[specificElement, 1];
                        break;
                    case MolecularWeightTool.ElementStatsType.Uncertainty:
                        stats.Uncertainty = elemVals[specificElement, 2];
                        break;
                    case MolecularWeightTool.ElementStatsType.Charge:
                        stats.Charge = (float)elemVals[specificElement, 3];
                        break;
                    default:
                        // Ignore it
                        break;
                }
            }
        }

        /// <summary>
        /// Stores isotope information in ElementStats()
        /// </summary>
        private void MemoryLoadIsotopes()
        {
            ElementAndMassInMemoryData.MemoryLoadIsotopes(ref ElementStats);
        }

        public void MemoryLoadMessageStatements()
        {
            MessageStatementCount = ElementAndMassInMemoryData.MemoryLoadMessageStatementsEnglish(ref MessageStatements);
        }

        private void MwtWinDllErrorHandler(string sourceForm)
        {
            string message;

            if (Information.Err().Number == 6)
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
                message = LookupMessage(600) + ": " + Information.Err().Description + Environment.NewLine + " (" + sourceForm + " handler)";
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

                GeneralErrorHandler(sourceForm, Information.Err().Number);

                ShowErrorMessageDialogs = showErrorMessageDialogsSaved;
            }
        }

        private void Initialize()
        {
            ElementAlph = new string[104];
            ElementStats = new ElementInfo[104];
            for (var i = 0; i <= ELEMENT_COUNT - 1; i++)
            {
                ElementStats[i] = new ElementInfo();
                ElementStats[i].Initialize();
            }

            AbbrevStats = new AbbrevStatsData[501];
            for (var i = 0; i < AbbrevStats.Length; i++)
            {
                AbbrevStats[i] = new AbbrevStatsData();
            }

            CautionStatements = new string[101, 3];
            MessageStatements = new string[1601];

            mProgressStepDescription = string.Empty;
            mProgressPercentComplete = 0f;

            mLogFolderPath = string.Empty;
            mLogFilePath = string.Empty;

            ShowErrorMessageDialogs = false;
        }

        private void InitializeAbbrevSymbolStack(ref AbbrevSymbolStack abbrevSymbolStack)
        {
            abbrevSymbolStack.Count = 0;
            abbrevSymbolStack.SymbolReferenceStack = new short[1];
        }

        private void InitializeComputationStats(ref ComputationStats computationStats)
        {
            computationStats.Initialize();
            computationStats.Charge = 0.0f;
            computationStats.StandardDeviation = 0.0d;
            computationStats.TotalMass = 0.0d;

            for (var elementIndex = 0; elementIndex < ELEMENT_COUNT; elementIndex++)
            {
                var element = computationStats.Elements[elementIndex];
                element.Used = false; // whether element is present
                element.Count = 0d; // # of each element
                element.IsotopicCorrection = 0d; // isotopic correction
                element.IsotopeCount = 0; // Count of the number of atoms defined as specific isotopes
                element.Isotopes = new IsotopicAtomInfo[3]; // Default to have room for 2 explicitly defined isotopes
                for (var i = 0; i < element.Isotopes.Length; i++)
                {
                    element.Isotopes[i] = new IsotopicAtomInfo();
                }
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
            ref ComputationStats computationStats,
            bool expandAbbreviations = false,
            double valueForX = 1)
        {
            var abbrevSymbolStack = new AbbrevSymbolStack();

            try
            {
                // Initialize the UDTs
                InitializeComputationStats(ref computationStats);
                InitializeAbbrevSymbolStack(ref abbrevSymbolStack);

                var stdDevSum = 0.0d;

                // Reset ErrorParams to clear any prior errors
                ResetErrorParamsInternal();

                // Reset Caution Description
                mStrCautionDescription = "";

                if (formula.Length > 0)
                {
                    var carbonOrSiliconReturnCount = 0;
                    formula = ParseFormulaRecursive(formula, ref computationStats, ref abbrevSymbolStack, expandAbbreviations, ref stdDevSum, ref carbonOrSiliconReturnCount, valueForX);
                }

                // Copy computationStats to mComputationStatsSaved
                mComputationStatsSaved.Initialize();
                mComputationStatsSaved = computationStats;

                if (ErrorParams.ErrorId == 0)
                {

                    // Compute the standard deviation
                    computationStats.StandardDeviation = Math.Sqrt(stdDevSum);

                    // Compute the total molecular weight
                    computationStats.TotalMass = 0d; // Reset total weight of compound to 0 so we can add to it
                    for (var elementIndex = 1; elementIndex <= ELEMENT_COUNT; elementIndex++)
                        // Increase total weight by multiplying the count of each element by the element's mass
                        // In addition, add in the Isotopic Correction value
                        computationStats.TotalMass = computationStats.TotalMass + ElementStats[elementIndex].Mass * computationStats.Elements[elementIndex].Count + computationStats.Elements[elementIndex].IsotopicCorrection;

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
            ref ComputationStats computationStats,
            ref AbbrevSymbolStack abbrevSymbolStack,
            bool expandAbbreviations,
            ref double stdDevSum,
            ref int carbonOrSiliconReturnCount,
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

            var computationStatsRightHalf = new ComputationStats();
            computationStatsRightHalf.Initialize();

            var abbrevSymbolStackRightHalf = new AbbrevSymbolStack();

            var stdDevSumRightHalf = default(double);
            double caretVal = default;
            var char1 = string.Empty;

            short symbolReference = default, prevSymbolReference = default;
            int parenthLevel = default;

            try
            {
                int charIndex;
                var dashMultiplier = dashMultiplierPrior;
                var bracketMultiplier = bracketMultiplierPrior;
                var insideBrackets = false;

                var dashPos = 0;
                var newFormula = string.Empty;
                var newFormulaRightHalf = string.Empty;

                var loneCarbonOrSilicon = 0;
                carbonOrSiliconReturnCount = 0;

                // Look for the > symbol
                // If found, this means take First Part minus the Second Part
                var minusSymbolLoc = formula.IndexOf(">", StringComparison.Ordinal);
                if (minusSymbolLoc >= 0)
                {
                    // Look for the first occurrence of >
                    charIndex = 0;
                    var matchFound = false;
                    do
                    {
                        if (formula.Substring(charIndex, 1) == ">")
                        {
                            matchFound = true;
                            var leftHalf = formula.Substring(0, Math.Min(formula.Length, charIndex - 1));
                            var rightHalf = formula.Substring(charIndex + 1);

                            // Parse the first half
                            newFormula = ParseFormulaRecursive(leftHalf, ref computationStats, ref abbrevSymbolStack, expandAbbreviations, ref stdDevSum, ref carbonOrSiliconReturnCount, valueForX, charCountPrior, parenthMultiplier, dashMultiplier, bracketMultiplier, parenthLevelPrevious);

                            // Parse the second half
                            InitializeComputationStats(ref computationStatsRightHalf);
                            InitializeAbbrevSymbolStack(ref abbrevSymbolStackRightHalf);

                            newFormulaRightHalf = ParseFormulaRecursive(rightHalf, ref computationStatsRightHalf, ref abbrevSymbolStackRightHalf, expandAbbreviations, ref stdDevSumRightHalf, ref carbonOrSiliconReturnCount, valueForX, charCountPrior + charIndex, parenthMultiplier, dashMultiplier, bracketMultiplier, parenthLevelPrevious);
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
                            if (ElementStats[elementIndex].Mass * element.Count + element.IsotopicCorrection >= ElementStats[elementIndex].Mass * computationStatsRightHalf.Elements[elementIndex].Count + computationStatsRightHalf.Elements[elementIndex].IsotopicCorrection)
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
                                    // This assertion is here simply because I want to check the code
                                    element.IsotopicCorrection -= computationStatsRightHalf.Elements[elementIndex].IsotopicCorrection;
                                }
                            }
                            else
                            {
                                // Invalid Formula; raise error
                                ErrorParams.ErrorId = 30;
                                ErrorParams.ErrorPosition = charIndex;
                            }

                            if (ErrorParams.ErrorId != 0)
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
                        var char2 = formula.Substring(charIndex + 1, 1);
                        var char3 = formula.Substring(charIndex + 2, 1);
                        var charRemain = formula.Substring(charIndex + 3);
                        if (gComputationOptions.CaseConversion != CaseConversionMode.ExactCase)
                            char1 = char1.ToUpper();

                        if (gComputationOptions.BracketsAsParentheses)
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
                                    ErrorParams.ErrorId = 14;
                                    ErrorParams.ErrorPosition = charIndex;
                                }

                                if (ErrorParams.ErrorId == 0)
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
                                                if (!gComputationOptions.BracketsAsParentheses && formula.Substring(parenthClose, 1) == "[")
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
                                                if (!gComputationOptions.BracketsAsParentheses && formula.Substring(parenthClose, 1) == "]")
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
                                                        newFormula = ParseFormulaRecursive(subFormula, ref computationStats, ref abbrevSymbolStack, expandAbbreviations, ref stdDevSum, ref carbonOrSiliconReturnCount, valueForX, charCountPrior + charIndex, parenthMultiplier * adjacentNum, dashMultiplier, bracketMultiplier, (short)(parenthLevelPrevious + 1));

                                                        // If expanding abbreviations, then newFormula might be longer than formula, must add this onto charIndex also
                                                        var expandAbbrevAdd = newFormula.Length - subFormula.Length;

                                                        // Must replace the part of the formula parsed with the newFormula part, in case the formula was expanded or elements were capitalized
                                                        formula = formula.Substring(0, charIndex) + newFormula + formula.Substring(parenthClose);
                                                        charIndex = parenthClose + addonCount + expandAbbrevAdd;

                                                        // Correct charge
                                                        if (carbonOrSiliconReturnCount > 0)
                                                        {
                                                            computationStats.Charge = (float)(computationStats.Charge - 2d * adjacentNum);
                                                            if (adjacentNum > 1d && carbonOrSiliconReturnCount > 1)
                                                            {
                                                                computationStats.Charge = (float)(computationStats.Charge - 2d * (adjacentNum - 1d) * (carbonOrSiliconReturnCount - 1));
                                                            }
                                                        }

                                                        break;
                                                    }
                                                }

                                                break;
                                        }
                                    }
                                }

                                if (parenthLevel > 0 && ErrorParams.ErrorId == 0)
                                {
                                    // Missing closing parenthesis
                                    ErrorParams.ErrorId = 3;
                                    ErrorParams.ErrorPosition = charIndex;
                                }

                                prevSymbolReference = 0;
                                break;

                            case 41:
                            case 125: // )    Repeat a section of a formula
                                // Should have been skipped
                                // Unmatched closing parentheses
                                ErrorParams.ErrorId = 4;
                                ErrorParams.ErrorPosition = charIndex;
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
                                    ErrorParams.ErrorId = 5;
                                    ErrorParams.ErrorPosition = charIndex + 1;
                                }
                                else
                                {
                                    // No number is present, that's just fine
                                    // Make sure defaults are set, though
                                    dashPos = 0;
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
                                        charIndex = charIndex + numLength - 1;
                                    }
                                    else
                                    {
                                        // A number less then zero should have been handled by CatchParseNumError above
                                        // Make sure defaults are set, though
                                        dashPos = 0;
                                        dashMultiplier = dashMultiplierPrior;
                                    }
                                }
                                else if (NumberConverter.CDblSafe(formula.Substring(charIndex - 1, 1)) > 0d)
                                {
                                    // Number too large
                                    ErrorParams.ErrorId = 7;
                                    ErrorParams.ErrorPosition = charIndex;
                                }
                                else
                                {
                                    // Misplaced number
                                    ErrorParams.ErrorId = 14;
                                    ErrorParams.ErrorPosition = charIndex;
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

                                if (ErrorParams.ErrorId == 0)
                                {
                                    if (insideBrackets)
                                    {
                                        // No Nested brackets.
                                        ErrorParams.ErrorId = 16;
                                        ErrorParams.ErrorPosition = charIndex;
                                    }
                                    else if (adjacentNum < 0d)
                                    {
                                        // No number after bracket
                                        ErrorParams.ErrorId = 12;
                                        ErrorParams.ErrorPosition = charIndex + 1;
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
                                    ErrorParams.ErrorId = 11;
                                    ErrorParams.ErrorPosition = charIndex + 1;
                                }
                                else if (insideBrackets)
                                {
                                    if (dashPos > 0)
                                    {
                                        // Need to set dashPos and dashMultiplier back to defaults, since a dash number goes back to one inside brackets
                                        dashPos = 0;
                                        dashMultiplier = 1d;
                                    }

                                    insideBrackets = false;
                                    bracketMultiplier = bracketMultiplierPrior;
                                }
                                else
                                {
                                    // Unmatched bracket
                                    ErrorParams.ErrorId = 15;
                                    ErrorParams.ErrorPosition = charIndex;
                                }

                                break;

                            case var case1 when 65 <= case1 && case1 <= 90:
                            case var case2 when 97 <= case2 && case2 <= 122:
                            case 43:
                            case 95: // Uppercase A to Z and lowercase a to z, and the plus (+) sign, and the underscore (_)
                                addonCount = 0;
                                adjacentNum = 0d;

                                var symbolMatchType = CheckElemAndAbbrev(formulaExcerpt, ref symbolReference);

                                switch (symbolMatchType)
                                {
                                    case SymbolMatchMode.Element:
                                        // Found an element
                                        // SymbolReference is the elemental number
                                        symbolLength = ElementStats[symbolReference].Symbol.Length;
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
                                            ErrorParams.ErrorId = 5;
                                            ErrorParams.ErrorPosition = charIndex + symbolLength;
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
                                                stdDevSum += atomCountToAdd * Math.Pow(ElementStats[symbolReference].Uncertainty, 2d);

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
                                                            compStats.Charge = (float)(compStats.Charge + atomCountToAdd * ElementStats[symbolReference].Charge);
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    compStats.Charge = (float)(compStats.Charge + atomCountToAdd * ElementStats[symbolReference].Charge);
                                                }

                                                if (symbolReference == 6 || symbolReference == 14)
                                                {
                                                    // Sum up number lone C and Si (not in abbreviations)
                                                    loneCarbonOrSilicon = (int)Math.Round(loneCarbonOrSilicon + adjacentNum);
                                                    carbonOrSiliconReturnCount = (int)Math.Round(carbonOrSiliconReturnCount + adjacentNum);
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
                                                    AddToCautionDescription(LookupMessage(660) + ": " + ElementStats[symbolReference].Symbol + " - " + caretVal + " " + LookupMessage(665) + " " + ElementStats[symbolReference].Mass);
                                                }
                                                else if (caretVal < symbolReference)
                                                {
                                                    // Definitely too low isotopic mass
                                                    AddToCautionDescription(LookupMessage(670) + ": " + ElementStats[symbolReference].Symbol + " - " + symbolReference + " " + LookupMessage(675));
                                                }
                                                else if (caretValDifference <= isoDifferenceBottom)
                                                {
                                                    // Probably too low isotopic mass
                                                    AddToCautionDescription(LookupMessage(662) + ": " + ElementStats[symbolReference].Symbol + " - " + caretVal + " " + LookupMessage(665) + " " + ElementStats[symbolReference].Mass);
                                                }

                                                // Put in isotopic correction factor
                                                atomCountToAdd = adjacentNum * bracketMultiplier * parenthMultiplier * dashMultiplier;
                                                var element = computationStats.Elements[symbolReference];
                                                // Increment element counting bin
                                                element.Count += atomCountToAdd;

                                                // Store information in .Isotopes[]
                                                // Increment the isotope counting bin
                                                element.IsotopeCount = (short)(element.IsotopeCount + 1);

                                                if (element.Isotopes.Length <= element.IsotopeCount)
                                                {
                                                    Array.Resize(ref element.Isotopes, element.Isotopes.Length + 2);
                                                }

                                                var isotope = element.Isotopes[element.IsotopeCount];
                                                isotope.Count += atomCountToAdd;
                                                isotope.Mass = caretVal;

                                                // Add correction amount to computationStats.Elements[SymbolReference].IsotopicCorrection
                                                element.IsotopicCorrection += (caretVal * atomCountToAdd - ElementStats[symbolReference].Mass * atomCountToAdd);

                                                // Set bit that element is present
                                                element.Used = true;

                                                // Assume no error in caret value, no need to change stdDevSum

                                                // Reset caretPresent
                                                caretPresent = false;
                                            }

                                            if (gComputationOptions.CaseConversion == CaseConversionMode.ConvertCaseUp)
                                            {
                                                formula = formula.Substring(0, charIndex - 1) + formula.Substring(charIndex, 1).ToUpper() + formula.Substring(charIndex + 1);
                                            }

                                            charIndex += addonCount;
                                        }

                                        break;

                                    case SymbolMatchMode.Abbreviation:
                                        // Found an abbreviation or amino acid
                                        // SymbolReference is the abbrev or amino acid number

                                        if (IsPresentInAbbrevSymbolStack(ref abbrevSymbolStack, symbolReference))
                                        {
                                            // Circular Reference: Can't have an abbreviation referencing an abbreviation that depends upon it
                                            // For example, the following is impossible:  Lor = C6H5Tal and Tal = H4O2Lor
                                            // Furthermore, can't have this either: Lor = C6H5Tal and Tal = H4O2Vin and Vin = S3Lor
                                            ErrorParams.ErrorId = 28;
                                            ErrorParams.ErrorPosition = charIndex;
                                        }
                                        // Found an abbreviation
                                        else if (caretPresent)
                                        {
                                            // Cannot have isotopic mass for an abbreviation, including deuterium
                                            if (char1.ToUpper() == "D" && char2 != "y")
                                            {
                                                // Isotopic mass used for Deuterium
                                                ErrorParams.ErrorId = 26;
                                                ErrorParams.ErrorPosition = charIndex;
                                            }
                                            else
                                            {
                                                ErrorParams.ErrorId = 24;
                                                ErrorParams.ErrorPosition = charIndex;
                                            }
                                        }
                                        else
                                        {
                                            // Parse abbreviation
                                            // Simply treat it like a formula surrounded by parentheses
                                            // Thus, find the number after the abbreviation, then call ParseFormulaRecursive, sending it the formula for the abbreviation
                                            // Update the abbrevSymbolStack before calling so that we can check for circular abbreviation references

                                            // Record the abbreviation length
                                            symbolLength = AbbrevStats[symbolReference].Symbol.Length;

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
                                            AbbrevSymbolStackAdd(ref abbrevSymbolStack, symbolReference);

                                            // Compute the charge prior to calling ParseFormulaRecursive
                                            // During the call to ParseFormulaRecursive, computationStats.Charge will be
                                            // modified according to the atoms in the abbreviation's formula
                                            // This is not what we want; instead, we want to use the defined charge for the abbreviation
                                            // We'll use the atomCountToAdd variable here, though instead of an atom count, it's really an abbreviation occurrence count
                                            var atomCountToAdd = adjacentNum * bracketMultiplier * parenthMultiplier * dashMultiplier;
                                            var chargeSaved = (float)(computationStats.Charge + atomCountToAdd * AbbrevStats[symbolReference].Charge);

                                            // When parsing an abbreviation, do not pass on the value of expandAbbreviations
                                            // This way, an abbreviation containing an abbreviation will only get expanded one level
                                            ParseFormulaRecursive(AbbrevStats[symbolReference].Formula, ref computationStats, ref abbrevSymbolStack, false, ref stdDevSum, ref carbonOrSiliconReturnCount, valueForX, charCountPrior + charIndex, parenthMultiplier * adjacentNum, dashMultiplier, bracketMultiplier, parenthLevelPrevious);

                                            // Update the charge to chargeSaved
                                            computationStats.Charge = chargeSaved;

                                            // Remove this symbol from the Abbreviation Symbol Stack
                                            AbbrevSymbolStackAddRemoveMostRecent(ref abbrevSymbolStack);

                                            if (ErrorParams.ErrorId == 0)
                                            {
                                                if (expandAbbreviations)
                                                {
                                                    // Replace abbreviation with empirical formula
                                                    var replace = AbbrevStats[symbolReference].Formula;

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
                                                        formula = formula.Substring(0, charIndex - 1) + replace + formula.Substring(charIndex + symbolLength);
                                                        symbolLength = replace.Length;
                                                        adjacentNum = 1d;
                                                        addonCount = symbolLength - 1;
                                                    }
                                                    else
                                                    {
                                                        // Number after abbreviation -- must put abbreviation in parentheses
                                                        // Parentheses can handle integer or decimal number
                                                        replace = "(" + replace + ")";
                                                        formula = formula.Substring(0, charIndex - 1) + replace + formula.Substring(charIndex + symbolLength);
                                                        symbolLength = replace.Length;
                                                        addonCount = numLength + symbolLength - 1;
                                                    }
                                                }

                                                if (gComputationOptions.CaseConversion == CaseConversionMode.ConvertCaseUp)
                                                {
                                                    formula = formula.Substring(0, charIndex - 1) + formula.Substring(charIndex, 1).ToUpper() + formula.Substring(charIndex + 1);
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
                                            ErrorParams.ErrorId = 18;
                                        }
                                        else
                                        {
                                            ErrorParams.ErrorId = 1;
                                        }

                                        ErrorParams.ErrorPosition = charIndex;
                                        break;
                                }

                                prevSymbolReference = symbolReference;
                                break;

                            case 94: // ^ (caret)
                                adjacentNum = ParseNum(char2 + char3 + charRemain, out numLength);
                                CatchParseNumError(adjacentNum, numLength, charIndex, symbolLength);

                                if (ErrorParams.ErrorId != 0)
                                {
                                    // Problem, don't go on.
                                }
                                else
                                {
                                    int charAsc;
                                    var charVal = formula.Substring(charIndex + 1 + numLength, 1);
                                    if (charVal.Length > 0)
                                        charAsc = charVal[0];
                                    else
                                        charAsc = 0;
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
                                            ErrorParams.ErrorId = 22;
                                            ErrorParams.ErrorPosition = charIndex + numLength + 1;
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
                                            ErrorParams.ErrorId = 23;
                                            ErrorParams.ErrorPosition = charIndex + 1;
                                        }
                                        else
                                        {
                                            // No number following caret
                                            ErrorParams.ErrorId = 20;
                                            ErrorParams.ErrorPosition = charIndex + 1;
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
                                    ErrorParams.ErrorId = 25;
                                    ErrorParams.ErrorPosition = charIndex;
                                }
                            }
                        }

                        if (ErrorParams.ErrorId != 0)
                        {
                            charIndex = formula.Length;
                        }

                        charIndex ++;
                    }
                    while (charIndex < formula.Length);
                }

                if (insideBrackets)
                {
                    if (ErrorParams.ErrorId == 0)
                    {
                        // Missing closing bracket
                        ErrorParams.ErrorId = 13;
                        ErrorParams.ErrorPosition = charIndex;
                    }
                }

                if (ErrorParams.ErrorId != 0 && ErrorParams.ErrorCharacter.Length == 0)
                {
                    if (string.IsNullOrEmpty(char1))
                        char1 = EMPTY_STRING_CHAR.ToString();
                    ErrorParams.ErrorCharacter = char1;
                    ErrorParams.ErrorPosition += charCountPrior;
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
                MwtWinDllErrorHandler("MwtWinDll_clsElementAndMassRoutines|ParseFormula: " + ex.Message);
                ErrorParams.ErrorId = -10;
                ErrorParams.ErrorPosition = 0;

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
            if (gComputationOptions.DecimalSeparator == default(char))
            {
                gComputationOptions.DecimalSeparator = MolecularWeightTool.DetermineDecimalPoint();
            }

            // Set numLength to -1 for now
            // If it doesn't get set to 0 (due to an error), it will get set to the
            // length of the matched number before exiting the sub
            numLength = -1;
            var foundNum = string.Empty;

            if (string.IsNullOrEmpty(work))
                work = EMPTY_STRING_CHAR.ToString();
            if ((work[0] < 48 || work[0] > 57) && work.Substring(0, 1) != gComputationOptions.DecimalSeparator.ToString() && !(work.Substring(0, 1) == "-" && allowNegative == true))
            {
                numLength = 0; // No number found
                return -1;
            }

            // Start of string is a number or a decimal point, or (if allowed) a negative sign
            for (var index = 0; index < work.Length; index++)
            {
                var working = work.Substring(index, 1);
                if (char.IsDigit(working[0]) || working == gComputationOptions.DecimalSeparator.ToString() || allowNegative == true && working == "-")
                {
                    foundNum += working;
                }
                else
                {
                    break;
                }
            }

            if (foundNum.Length == 0 || foundNum == gComputationOptions.DecimalSeparator.ToString())
            {
                // No number at all or (more likely) no number after decimal point
                foundNum = (-3).ToString();
                numLength = 0;
            }
            else
            {
                // Check for more than one decimal point (. or ,)
                var decPtCount = 0;
                for (var index = 0; index < foundNum.Length; index++)
                {
                    if (foundNum.Substring(index, 1) == gComputationOptions.DecimalSeparator.ToString())
                        decPtCount = (short)(decPtCount + 1);
                }

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
        /// Converts plain text to formatted rtf text
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
            var rtf = @"{\rtf1\ansi\deff0\deftab720{\fonttbl{\f0\fswiss MS Sans Serif;}{\f1\froman\fcharset2 Symbol;}{\f2\froman " + gComputationOptions.RtfFontName + @";}{\f3\froman Times New Roman;}{\f4\fswiss\fprq2 System;}}{\colortbl\red0\green0\blue0;\red255\green0\blue0;\red255\green255\blue255;}\deflang1033\pard\plain\f2\fs" + NumberConverter.CShortSafe(gComputationOptions.RtfFontSize * 2.5d) + " ";
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
                            errorId = ErrorParams.ErrorId;
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
                        rtf = rtf + @"{\cf1 " + workChar + "}";
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
                else if (char.IsDigit(workChar[0]) || workChar == gComputationOptions.DecimalSeparator.ToString())
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
                        rtf = rtf + @"{\sub " + workChar + "}";
                    }
                    else if (!calculatorMode && gComputationOptions.BracketsAsParentheses && workCharPrev == "]")
                    {
                        // only subscript after closing bracket, ], if brackets are being treated as parentheses
                        rtf = rtf + @"{\sub " + workChar + "}";
                    }
                    else if (rtf.Substring(rtf.Length - 8, 5) == "super")
                    {
                        // if previous character was superscripted, then superscript this number too
                        rtf = rtf + @"{\super " + workChar + "}";
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
                rtf = rtf + @"{\fs" + NumberConverter.CShortSafe(gComputationOptions.RtfFontSize * 3) + @"\cf2 " + RTF_HEIGHT_ADJUST_CHAR + "}}";
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
            for (var index = 1; index <= AbbrevAllCount; index++)
            {
                AbbrevStats[index].Mass = ComputeFormulaWeight(ref AbbrevStats[index].Formula);
            }
        }

        public void RemoveAllCautionStatementsInternal()
        {
            CautionStatementCount = 0;
        }

        public void RemoveAllAbbreviationsInternal()
        {
            AbbrevAllCount = 0;
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

            for (var index = 1; index <= AbbrevAllCount; index++)
            {
                if ((AbbrevStats[index].Symbol?.ToLower() ?? "") == (abbreviationSymbol ?? ""))
                {
                    RemoveAbbreviationByIdInternal(index);
                    removed = true;
                }
            }

            if (removed)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Remove the abbreviation at index <paramref name="abbreviationId"/>
        /// </summary>
        /// <param name="abbreviationId"></param>
        /// <returns>0 if found and removed; 1 if error</returns>
        public int RemoveAbbreviationByIdInternal(int abbreviationId)
        {
            bool removed;

            if (abbreviationId >= 1 && abbreviationId <= AbbrevAllCount)
            {
                for (var indexRemove = abbreviationId; indexRemove < AbbrevAllCount; indexRemove++)
                    AbbrevStats[indexRemove] = AbbrevStats[indexRemove + 1];

                AbbrevAllCount = (short)(AbbrevAllCount - 1);
                ConstructMasterSymbolsList();
                removed = true;
            }
            else
            {
                removed = false;
            }

            if (removed)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Look for the caution statement and remove it
        /// </summary>
        /// <param name="cautionSymbol"></param>
        /// <returns>0 if found and removed; 1 if error</returns>
        public int RemoveCautionStatementInternal(string cautionSymbol)
        {
            var removed = default(bool);

            for (var index = 1; index <= CautionStatementCount; index++)
            {
                if ((CautionStatements[index, 0] ?? "") == (cautionSymbol ?? ""))
                {
                    for (var indexRemove = index; indexRemove < CautionStatementCount; indexRemove++)
                    {
                        CautionStatements[indexRemove, 0] = CautionStatements[indexRemove + 1, 0];
                        CautionStatements[indexRemove, 1] = CautionStatements[indexRemove + 1, 1];
                    }

                    CautionStatementCount -= 1;
                    removed = true;
                }
            }

            if (removed)
            {
                return 0;
            }

            return 1;
        }

        public void ResetErrorParamsInternal()
        {
            ErrorParams.ErrorCharacter = "";
            ErrorParams.ErrorId = 0;
            ErrorParams.ErrorPosition = 0;
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

                    if (gComputationOptions.StdDevMode == StdDevMode.Short)
                    {
                        // StdDevType Short (Type 0)
                        result = roundedMain.ToString();
                        if (includeStandardDeviation)
                        {
                            result = result + "(" + '±' + stdDevShort + ")";
                        }

                        result += pctSign;
                    }
                    else if (gComputationOptions.StdDevMode == StdDevMode.Scientific)
                    {
                        // StdDevType Scientific (Type 1)
                        result = roundedMain + pctSign;
                        if (includeStandardDeviation)
                        {
                            result += " (" + '±' + stdDev.ToString("0.000E+00") + ")";
                        }
                    }
                    else
                    {
                        // StdDevType Decimal
                        result = mass.ToString("0.0####") + pctSign;
                        if (includeStandardDeviation)
                        {
                            result += " (" + '±' + roundedStdDev + ")";
                        }
                    }
                }

                return result;
            }
            catch
            {
                MwtWinDllErrorHandler("MwtWinDll_clsElementAndMassRoutines|ReturnFormattedMassAndStdDev");
                ErrorParams.ErrorId = -10;
                ErrorParams.ErrorPosition = 0;
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
            for (var index = 1; index <= AbbrevAllCount; index++)
            {
                if ((AbbrevStats[index].Symbol?.ToUpper() ?? "") == (symbol?.ToUpper() ?? ""))
                {
                    alreadyPresent = true;
                    abbrevId = index;
                    break;
                }
            }

            // AbbrevStats is a 1-based array
            if (!alreadyPresent)
            {
                if (AbbrevAllCount < MAX_ABBREV_COUNT)
                {
                    abbrevId = AbbrevAllCount + 1;
                    AbbrevAllCount = (short)(AbbrevAllCount + 1);
                }
                else
                {
                    // Too many abbreviations
                    ErrorParams.ErrorId = 196;
                }
            }

            if (abbrevId >= 1)
            {
                SetAbbreviationByIdInternal((short)abbrevId, symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, validateFormula);
            }

            return ErrorParams.ErrorId;
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
            computationStats.Initialize();

            var abbrevSymbolStack = new AbbrevSymbolStack();
            var invalidSymbolOrFormula = default(bool);
            var symbolReference = default(short);

            ResetErrorParamsInternal();

            // Initialize the UDTs
            InitializeComputationStats(ref computationStats);
            InitializeAbbrevSymbolStack(ref abbrevSymbolStack);

            if (symbol.Length < 1)
            {
                // Symbol length is 0
                ErrorParams.ErrorId = 192;
            }
            else if (symbol.Length > MAX_ABBREV_LENGTH)
            {
                // Abbreviation symbol too long
                ErrorParams.ErrorId = 190;
            }
            else if (IsStringAllLetters(symbol))
            {
                if (formula.Length > 0)
                {
                    // Convert symbol to proper case mode
                    symbol = symbol.Substring(0, 1).ToUpper() + symbol.Substring(1).ToLower();

                    // If abbrevId is < 1 or larger than AbbrevAllCount, then define it
                    if (abbrevId < 1 || abbrevId > AbbrevAllCount + 1)
                    {
                        if (AbbrevAllCount < MAX_ABBREV_COUNT)
                        {
                            AbbrevAllCount = (short)(AbbrevAllCount + 1);
                            abbrevId = AbbrevAllCount;
                        }
                        else
                        {
                            // Too many abbreviations
                            ErrorParams.ErrorId = 196;
                            abbrevId = -1;
                        }
                    }

                    if (abbrevId >= 1)
                    {
                        // Make sure the abbreviation doesn't match one of the standard elements
                        var symbolMatchType = CheckElemAndAbbrev(symbol, ref symbolReference);

                        if (symbolMatchType == SymbolMatchMode.Element)
                        {
                            if ((ElementStats[symbolReference].Symbol ?? "") == symbol)
                            {
                                invalidSymbolOrFormula = true;
                            }
                        }

                        if (!invalidSymbolOrFormula && validateFormula)
                        {
                            // Make sure the abbreviation's formula is valid
                            // This will also auto-capitalize the formula if auto-capitalize is turned on
                            var stdDevSum = 0d;
                            var carbonOrSiliconReturnCount = 0;
                            formula = ParseFormulaRecursive(formula, ref computationStats, ref abbrevSymbolStack, false, ref stdDevSum, ref carbonOrSiliconReturnCount);

                            if (ErrorParams.ErrorId != 0)
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
                    ErrorParams.ErrorId = 160;
                }
            }
            else
            {
                // Symbol does not just contain letters
                ErrorParams.ErrorId = 194;
            }

            return ErrorParams.ErrorId;
        }

        /// <summary>
        /// Adds a new caution statement or updates an existing one (based on <paramref name="symbolCombo"/>)
        /// </summary>
        /// <param name="symbolCombo"></param>
        /// <param name="newCautionStatement"></param>
        /// <returns>0 if successful, otherwise, returns an Error ID</returns>
        public int SetCautionStatementInternal(string symbolCombo, string newCautionStatement)
        {
            var alreadyPresent = default(bool);

            ResetErrorParamsInternal();

            if (symbolCombo.Length >= 1 && symbolCombo.Length <= MAX_ABBREV_LENGTH)
            {
                // Make sure all the characters in symbolCombo are letters
                if (IsStringAllLetters(symbolCombo))
                {
                    if (newCautionStatement.Length > 0)
                    {
                        int index;
                        // See if symbolCombo is present in CautionStatements[]
                        for (index = 1; index <= CautionStatementCount; index++)
                        {
                            if ((CautionStatements[index, 0] ?? "") == symbolCombo)
                            {
                                alreadyPresent = true;
                                break;
                            }
                        }

                        // Caution statements is a 0-based array
                        if (!alreadyPresent)
                        {
                            if (CautionStatementCount < MAX_CAUTION_STATEMENTS)
                            {
                                CautionStatementCount += 1;
                                index = CautionStatementCount;
                            }
                            else
                            {
                                // Too many caution statements
                                ErrorParams.ErrorId = 1215;
                                index = -1;
                            }
                        }

                        if (index >= 1)
                        {
                            CautionStatements[index, 0] = symbolCombo;
                            CautionStatements[index, 1] = newCautionStatement;
                        }
                    }
                    else
                    {
                        // Caution description length is 0
                        ErrorParams.ErrorId = 1210;
                    }
                }
                else
                {
                    // Caution symbol doesn't just contain letters
                    ErrorParams.ErrorId = 1205;
                }
            }
            else
            {
                // Symbol length is 0 or is greater than MAX_ABBREV_LENGTH
                ErrorParams.ErrorId = 1200;
            }

            return ErrorParams.ErrorId;
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
                if ((symbol?.ToLower() ?? "") == (ElementStats[index].Symbol?.ToLower() ?? ""))
                {
                    var stats = ElementStats[index];
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

        public int SetElementIsotopesInternal(string symbol, short isotopeCount, ref double[] isotopeMassesOneBased, ref float[] isotopeAbundancesOneBased)
        {
            var found = default(bool);

            for (var index = 1; index <= ELEMENT_COUNT; index++)
            {
                if ((symbol?.ToLower() ?? "") == (ElementStats[index].Symbol?.ToLower() ?? ""))
                {
                    var stats = ElementStats[index];
                    if (isotopeCount < 0)
                        isotopeCount = 0;
                    stats.IsotopeCount = isotopeCount;
                    for (var isotopeIndex = 1; isotopeIndex <= stats.IsotopeCount; isotopeIndex++)
                    {
                        if (isotopeIndex > MAX_ISOTOPES)
                            break;
                        stats.Isotopes[isotopeIndex].Mass = isotopeMassesOneBased[isotopeIndex];
                        stats.Isotopes[isotopeIndex].Abundance = isotopeAbundancesOneBased[isotopeIndex];
                    }

                    found = true;
                    break;
                }
            }

            if (found)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Set the element mode
        /// </summary>
        /// <param name="newElementMode"></param>
        /// <param name="memoryLoadElementValues"></param>
        /// <remarks>
        /// The only time you would want <paramref name="memoryLoadElementValues"/> to be false is if you're
        /// manually setting element weight values, but want to let the software know that
        /// they're average, isotopic, or integer values
        /// </remarks>
        public void SetElementModeInternal(ElementMassMode newElementMode, bool memoryLoadElementValues = true)
        {
            try
            {
                if (newElementMode >= ElementMassMode.Average && newElementMode <= ElementMassMode.Integer)
                {
                    if (newElementMode != mCurrentElementMode || memoryLoadElementValues)
                    {
                        mCurrentElementMode = newElementMode;

                        if (memoryLoadElementValues)
                        {
                            MemoryLoadElements(mCurrentElementMode);
                        }

                        ConstructMasterSymbolsList();
                        RecomputeAbbreviationMassesInternal();
                    }
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
                MessageStatements[messageId] = newMessage;
                return 0;
            }

            return 1;
        }

        private void ShellSortSymbols(int lowIndex, int highIndex)
        {
            var pointerArray = new int[highIndex + 1];
            var symbolsStore = new string[highIndex + 1, 2];

            // MasterSymbolsList starts at lowIndex
            for (var index = lowIndex; index <= highIndex; index++)
                pointerArray[index] = index;

            ShellSortSymbolsWork(ref pointerArray, lowIndex, highIndex);

            // Reassign MasterSymbolsList array according to PointerArray order
            // First, copy to a temporary array (I know it eats up memory, but I have no choice)
            for (var index = lowIndex; index <= highIndex; index++)
            {
                symbolsStore[index, 0] = MasterSymbolsList[index, 0];
                symbolsStore[index, 1] = MasterSymbolsList[index, 1];
            }

            // Now, put them back into the MasterSymbolsList() array in the correct order
            // Use PointerArray() for this
            for (var index = lowIndex; index <= highIndex; index++)
            {
                MasterSymbolsList[index, 0] = symbolsStore[pointerArray[index], 0];
                MasterSymbolsList[index, 1] = symbolsStore[pointerArray[index], 1];
            }
        }

        /// <summary>
        /// Sort the list using a shell sort
        /// </summary>
        /// <param name="pointerArray"></param>
        /// <param name="lowIndex"></param>
        /// <param name="highIndex"></param>
        private void ShellSortSymbolsWork(ref int[] pointerArray, int lowIndex, int highIndex)
        {
            // Sort PointerArray[lowIndex..highIndex] by comparing
            // Len(MasterSymbolsList(PointerArray(x)) and sorting by decreasing length
            // If same length, sort alphabetically (increasing)

            // Compute largest increment
            var itemCount = highIndex - lowIndex + 1;
            var incrementAmount = 1;
            if (itemCount < 14)
            {
                incrementAmount = 1;
            }
            else
            {
                while (incrementAmount < itemCount)
                    incrementAmount = 3 * incrementAmount + 1;
                incrementAmount /= 3;
                incrementAmount /= 3;
            }

            while (incrementAmount > 0)
            {
                // Sort by insertion in increments of incrementAmount
                for (var index = lowIndex + incrementAmount; index <= highIndex; index++)
                {
                    var pointerSwap = pointerArray[index];
                    int indexCompare;
                    for (indexCompare = index - incrementAmount; indexCompare >= lowIndex; indexCompare += -incrementAmount)
                    {
                        // Use <= to sort ascending; Use > to sort descending
                        // Sort by decreasing length
                        var length1 = MasterSymbolsList[pointerArray[indexCompare], 0].Length;
                        var length2 = MasterSymbolsList[pointerSwap, 0].Length;
                        if (length1 > length2)
                            break;
                        // If same length, sort alphabetically
                        if (length1 == length2)
                        {
                            if (string.Compare(MasterSymbolsList[pointerArray[indexCompare], 0].ToUpper(), MasterSymbolsList[pointerSwap, 0].ToUpper(), StringComparison.Ordinal) <= 0)
                                break;
                        }

                        pointerArray[indexCompare + incrementAmount] = pointerArray[indexCompare];
                    }

                    pointerArray[indexCompare + incrementAmount] = pointerSwap;
                }

                incrementAmount /= 3;
            }
        }

        public void SetShowErrorMessageDialogs(bool value)
        {
            ShowErrorMessageDialogs = value;
        }

        public void SortAbbreviationsInternal()
        {
            int itemCount = AbbrevAllCount;
            const int lowIndex = 1;
            var highIndex = itemCount;

            // sort array[lowIndex..highIndex]

            // compute largest increment
            itemCount = highIndex - lowIndex + 1;
            var incrementAmount = 1;
            if (itemCount < 14)
            {
                incrementAmount = 1;
            }
            else
            {
                while (incrementAmount < itemCount)
                    incrementAmount = 3 * incrementAmount + 1;

                incrementAmount /= 3;
                incrementAmount /= 3;
            }

            while (incrementAmount > 0)
            {
                // sort by insertion in increments of incrementAmount
                for (var index = lowIndex + incrementAmount; index <= highIndex; index++)
                {
                    var compare = AbbrevStats[index];
                    int indexCompare;
                    for (indexCompare = index - incrementAmount; indexCompare >= lowIndex; indexCompare += -incrementAmount)
                    {
                        // Use <= to sort ascending; Use > to sort descending
                        if (string.Compare(AbbrevStats[indexCompare].Symbol, compare.Symbol, StringComparison.Ordinal) <= 0)
                            break;
                        AbbrevStats[indexCompare + incrementAmount] = AbbrevStats[indexCompare];
                    }

                    AbbrevStats[indexCompare + incrementAmount] = compare;
                }

                incrementAmount /= 3;
            }

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

            for (short abbrevIndex = 1; abbrevIndex <= AbbrevAllCount; abbrevIndex++)
            {
                var stats = AbbrevStats[abbrevIndex];
                SetAbbreviationByIdInternal(abbrevIndex, stats.Symbol, stats.Formula, stats.Charge, stats.IsAminoAcid, stats.OneLetterSymbol, stats.Comment, true);
                if (stats.InvalidSymbolOrFormula)
                {
                    invalidAbbreviationCount = (short)(invalidAbbreviationCount + 1);
                }
            }

            return invalidAbbreviationCount;
        }
    }
}
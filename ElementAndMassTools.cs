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

        public class udtOptionsType
        {
            public MolecularWeightTool.AbbrevRecognitionMode AbbrevRecognitionMode;
            public bool BracketsAsParentheses;
            public CaseConversionMode CaseConversion;
            public char DecimalSeparator;
            public string RtfFontName;
            public short RtfFontSize;
            public StdDevMode StdDevMode; // Can be 0, 1, or 2 (see smStdDevModeConstants)
        }

        public class usrIsotopicAtomInfoType
        {
            public double Count; // Can have non-integer counts of atoms, eg. ^13C5.5
            public double Mass;
        }

        public class udtElementUseStatsType
        {
            public bool Used;
            public double Count; // Can have non-integer counts of atoms, eg. C5.5
            public double IsotopicCorrection;
            public short IsotopeCount; // Number of specific isotopes defined
            public usrIsotopicAtomInfoType[] Isotopes;
        }

        public class udtPctCompType
        {
            public double PercentComposition;
            public double StdDeviation;

            public override string ToString()
            {
                return PercentComposition.ToString("0.0000");
            }
        }

        public class udtComputationStatsType
        {
            public udtElementUseStatsType[] Elements;        // 1-based array, ranging from 1 to ELEMENT_COUNT
            public double TotalMass;
            public udtPctCompType[] PercentCompositions;     // 1-based array, ranging from 1 to ELEMENT_COUNT
            public float Charge;
            public double StandardDeviation;

            // Note: "Initialize" must be called to initialize instances of this structure
            public void Initialize()
            {
                Elements = new udtElementUseStatsType[104];
                PercentCompositions = new udtPctCompType[104];
            }
        }

        public class udtIsotopeInfoType
        {
            public double Mass;
            public float Abundance;

            public override string ToString()
            {
                return Mass.ToString("0.0000");
            }
        }

        public class udtElementStatsType
        {
            public string Symbol;
            public double Mass;
            public double Uncertainty;
            public float Charge;
            public short IsotopeCount; // # of isotopes an element has
            public udtIsotopeInfoType[] Isotopes; // Masses and Abundances of the isotopes; 1-based array, ranging from 1 to MAX_Isotopes

            // Note: "Initialize" must be called to initialize instances of this structure
            public void Initialize()
            {
                Isotopes = new udtIsotopeInfoType[ElementAndMassTools.MAX_ISOTOPES + 1];
            }

            public override string ToString()
            {
                return Symbol + ": " + Mass.ToString("0.0000");
            }
        }

        public class udtAbbrevStatsType
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

        private class udtErrorDescriptionType
        {
            public int ErrorID; // Contains the error number (used in the LookupMessage function).  In addition, if a program error occurs, ErrorParams.ErrorID = -10
            public int ErrorPosition;
            public string ErrorCharacter;

            public override string ToString()
            {
                return "ErrorID " + ErrorID + " at " + ErrorPosition + ": " + ErrorCharacter;
            }
        }

        private class udtIsoResultsByElementType
        {
            public short ElementIndex; // Index of element in ElementStats() array; look in ElementStats() to get information on its isotopes
            public bool boolExplicitIsotope; // True if an explicitly defined isotope
            public double ExplicitMass;
            public int AtomCount; // Number of atoms of this element in the formula being parsed
            public int ResultsCount; // Number of masses in MassAbundances
            public int StartingResultsMass; // Starting mass of the results for this element
            public float[] MassAbundances; // Abundance of each mass, starting with StartingResultsMass
        }

        private class udtIsoResultsOverallType
        {
            public float Abundance;
            public int Multiplicity;
        }

        private class udtAbbrevSymbolStackType
        {
            public short Count;
            public short[] SymbolReferenceStack; // 0-based array
        }

        private class udtXYDataType
        {
            public double X;
            public double Y;
        }
        #endregion

        #region "Classwide Variables"

        public udtOptionsType gComputationOptions = new udtOptionsType();

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
        private udtElementStatsType[] ElementStats;

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
        private udtAbbrevStatsType[] AbbrevStats;
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

        private readonly udtErrorDescriptionType ErrorParams = new udtErrorDescriptionType();

        /// <summary>
        /// Charge carrier mass
        /// 1.00727649 for monoisotopic mass or 1.00739 for average mass
        /// </summary>
        private double mChargeCarrierMass;

        private ElementMassMode mCurrentElementMode;
        private string mStrCautionDescription;
        private udtComputationStatsType mComputationStatsSaved = new udtComputationStatsType();

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
        /// <param name="udtAbbrevSymbolStack">Symbol stack; updated by this method</param>
        /// <param name="symbolReference"></param>
        private void AbbrevSymbolStackAdd(ref udtAbbrevSymbolStackType udtAbbrevSymbolStack, short symbolReference)
        {
            try
            {
                udtAbbrevSymbolStack.Count = (short)(udtAbbrevSymbolStack.Count + 1);
                Array.Resize(ref udtAbbrevSymbolStack.SymbolReferenceStack, udtAbbrevSymbolStack.Count);
                udtAbbrevSymbolStack.SymbolReferenceStack[udtAbbrevSymbolStack.Count - 1] = symbolReference;
            }
            catch (Exception ex)
            {
                GeneralErrorHandler("ElementAndMassTools.AbbrevSymbolStackAdd", ex);
            }
        }

        /// <summary>
        /// Update the abbreviation symbol stack
        /// </summary>
        /// <param name="udtAbbrevSymbolStack">Symbol stack; updated by this method</param>
        private void AbbrevSymbolStackAddRemoveMostRecent(ref udtAbbrevSymbolStackType udtAbbrevSymbolStack)
        {
            if (udtAbbrevSymbolStack.Count > 0)
            {
                udtAbbrevSymbolStack.Count = (short)(udtAbbrevSymbolStack.Count - 1);
            }
        }

        public virtual void AbortProcessingNow()
        {
            mAbortProcessing = true;
        }

        /// <summary>
        /// Add an abbreviation
        /// </summary>
        /// <param name="intAbbrevIndex"></param>
        /// <param name="strSymbol"></param>
        /// <param name="strFormula"></param>
        /// <param name="sngCharge"></param>
        /// <param name="blnIsAminoAcid"></param>
        /// <param name="strOneLetter"></param>
        /// <param name="strComment"></param>
        /// <param name="blnInvalidSymbolOrFormula"></param>
        /// <returns><paramref name="strFormula"/> with format standardized by ParseFormulaPublic</returns>
        private string AddAbbreviationWork(
            short intAbbrevIndex, string strSymbol,
            string strFormula, float sngCharge,
            bool blnIsAminoAcid,
            string strOneLetter = "",
            string strComment = "",
            bool blnInvalidSymbolOrFormula = false)
        {
            var stats = AbbrevStats[intAbbrevIndex];
            stats.InvalidSymbolOrFormula = blnInvalidSymbolOrFormula;
            stats.Symbol = strSymbol;
            stats.Formula = strFormula;
            stats.Mass = ComputeFormulaWeight(ref strFormula);
            if (stats.Mass < 0d)
            {
                // Error occurred computing mass for abbreviation
                stats.Mass = 0d;
                stats.InvalidSymbolOrFormula = true;
            }

            stats.Charge = sngCharge;
            stats.OneLetterSymbol = strOneLetter.ToUpper();
            stats.IsAminoAcid = blnIsAminoAcid;
            stats.Comment = strComment;

            return strFormula;
        }

        private void AddToCautionDescription(string strTextToAdd)
        {
            if (string.IsNullOrWhiteSpace(mStrCautionDescription))
            {
                mStrCautionDescription = "";
            }

            mStrCautionDescription += strTextToAdd;
        }

        private void CheckCaution(string strFormulaExcerpt)
        {
            for (var intLength = 1; intLength <= MAX_ABBREV_LENGTH; intLength++)
            {
                if (intLength > strFormulaExcerpt.Length)
                    break;
                var strTest = strFormulaExcerpt.Substring(0, intLength);
                var strNewCaution = LookupCautionStatement(strTest);
                if (!string.IsNullOrEmpty(strNewCaution))
                {
                    AddToCautionDescription(strNewCaution);
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
                        ErrorParams.ErrorID = 12;
                        ErrorParams.ErrorPosition = curCharacter + symbolLength;
                        break;
                    case -4:
                        // Error: More than one decimal point
                        ErrorParams.ErrorID = 27;
                        ErrorParams.ErrorPosition = curCharacter + symbolLength;
                        break;
                    default:
                        // Error: General number error
                        ErrorParams.ErrorID = 14;
                        ErrorParams.ErrorPosition = curCharacter + symbolLength;
                        break;
                }
            }
        }

        /// <summary>
        /// Examines the formula excerpt to determine if it is an element, abbreviation, amino acid, or unknown
        /// </summary>
        /// <param name="strFormulaExcerpt"></param>
        /// <param name="symbolReference">Output: index of the matched element or abbreviation in MasterSymbolsList()</param>
        /// <returns>
        /// smtElement if matched an element
        /// smtAbbreviation if matched an abbreviation or amino acid
        /// smtUnknown if no match
        /// </returns>
        private SymbolMatchMode CheckElemAndAbbrev(string strFormulaExcerpt, ref short symbolReference)
        {
            var eSymbolMatchType = default(SymbolMatchMode);

            // MasterSymbolsList[] stores the element symbols, abbreviations, & amino acids in order of longest length to
            // shortest length, non-alphabetized, for use in symbol matching when parsing a formula

            // MasterSymbolsList[intIndex,0] contains the symbol to be matched
            // MasterSymbolsList[intIndex,1] contains E for element, A for amino acid, or N for normal abbreviation, followed by
            // the reference number in the master list
            // For example for Carbon, MasterSymbolsList[intIndex,0] = "C" and MasterSymbolsList[intIndex,1] = "E6"

            // Look for match, stepping directly through MasterSymbolsList[]
            // List is sorted by reverse length, so can do all at once

            for (var intIndex = 0; intIndex < MasterSymbolsListCount; intIndex++)
            {
                if (MasterSymbolsList[intIndex, 0].Length > 0)
                {
                    if (strFormulaExcerpt.Substring(0, Math.Min(strFormulaExcerpt.Length, MasterSymbolsList[intIndex, 0].Length)) == (MasterSymbolsList[intIndex, 0] ?? ""))
                    {
                        // Matched a symbol
                        switch (MasterSymbolsList[intIndex, 1].Substring(0, 1).ToUpper())
                        {
                            case "E": // An element
                                eSymbolMatchType = SymbolMatchMode.Element;
                                break;
                            case "A": // An abbreviation or amino acid
                                eSymbolMatchType = SymbolMatchMode.Abbreviation;
                                break;
                            default:
                                // error
                                eSymbolMatchType = SymbolMatchMode.Unknown;
                                symbolReference = -1;
                                break;
                        }

                        if (eSymbolMatchType != SymbolMatchMode.Unknown)
                        {
                            symbolReference = (short)Math.Round(double.Parse(MasterSymbolsList[intIndex, 1].Substring(1)));
                        }

                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Zero-length entry found in MasterSymbolsList[]; this is unexpected");
                }
            }

            return eSymbolMatchType;
        }

        /// <summary>
        /// Compute the weight of a formula (or abbreviation)
        /// </summary>
        /// <param name="strFormula">Input</param>
        /// <returns>The formula mass, or -1 if an error occurs</returns>
        /// <remarks>Error information is stored in ErrorParams</remarks>
        public double ComputeFormulaWeight(string strFormula)
        {
            return ComputeFormulaWeight(ref strFormula);
        }

        /// <summary>
        /// Compute the weight of a formula (or abbreviation)
        /// </summary>
        /// <param name="strFormula">Input/output: updated by ParseFormulaPublic</param>
        /// <returns>The formula mass, or -1 if an error occurs</returns>
        /// <remarks>Error information is stored in ErrorParams</remarks>
        public double ComputeFormulaWeight(ref string strFormula)
        {
            var udtComputationStats = new udtComputationStatsType();
            udtComputationStats.Initialize();

            ParseFormulaPublic(ref strFormula, ref udtComputationStats, false);

            if (ErrorParams.ErrorID == 0)
            {
                return udtComputationStats.TotalMass;
            }

            return -1;
        }

        /// <summary>
        /// Computes the Isotopic Distribution for a formula, returns uncharged mass values if intChargeState=0,
        /// M+H values if intChargeState=1, and convoluted m/z if intChargeState is > 1
        /// </summary>
        /// <param name="strFormulaIn">Input/output: The properly formatted formula to parse</param>
        /// <param name="intChargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
        /// <param name="strResults">Output: Table of results</param>
        /// <param name="ConvolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
        /// <param name="ConvolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
        /// <returns>0 if success, -1 if an error</returns>
        /// <remarks></remarks>
        public short ComputeIsotopicAbundances(
            ref string strFormulaIn,
            short intChargeState,
            ref string strResults,
            ref double[,] ConvolutedMSData2DOneBased,
            ref int ConvolutedMSDataCount)
        {
            const bool blnUseFactorials = false;
            const bool blnAddProtonChargeCarrier = true;

            return ComputeIsotopicAbundancesInternal(ref strFormulaIn, intChargeState, ref strResults, ref ConvolutedMSData2DOneBased, ref ConvolutedMSDataCount,
                "Isotopic Abundances for",
                "Mass/Charge",
                "Fraction",
                "Intensity",
                blnUseFactorials,
                blnAddProtonChargeCarrier);
        }

        /// <summary>
        /// Computes the Isotopic Distribution for a formula, returns uncharged mass values if intChargeState=0,
        /// M+H values if intChargeState=1, and convoluted m/z if intChargeState is > 1
        /// </summary>
        /// <param name="strFormulaIn">Input/output: The properly formatted formula to parse</param>
        /// <param name="intChargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
        /// <param name="strResults">Output: Table of results</param>
        /// <param name="ConvolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
        /// <param name="ConvolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
        /// <param name="blnAddProtonChargeCarrier">If blnAddProtonChargeCarrier is False, then still convolutes by charge, but doesn't add a proton</param>
        /// <returns>0 if success, -1 if an error</returns>
        /// <remarks></remarks>
        public short ComputeIsotopicAbundances(
            ref string strFormulaIn,
            short intChargeState,
            ref string strResults,
            ref double[,] ConvolutedMSData2DOneBased,
            ref int ConvolutedMSDataCount,
            bool blnAddProtonChargeCarrier)
        {
            const bool blnUseFactorials = false;

            return ComputeIsotopicAbundancesInternal(ref strFormulaIn, intChargeState, ref strResults, ref ConvolutedMSData2DOneBased, ref ConvolutedMSDataCount,
                "Isotopic Abundances for",
                "Mass/Charge",
                "Fraction",
                "Intensity",
                blnUseFactorials,
                blnAddProtonChargeCarrier);
        }

        /// <summary>
        /// Computes the Isotopic Distribution for a formula
        /// </summary>
        /// <param name="strFormulaIn">Input/output: The properly formatted formula to parse</param>
        /// <param name="intChargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
        /// <param name="strResults">Output: Table of results</param>
        /// <param name="ConvolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
        /// <param name="ConvolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
        /// <param name="strHeaderIsotopicAbundances">Header to use in strResults</param>
        /// <param name="strHeaderMassToCharge">Header to use in strResults</param>
        /// <param name="strHeaderFraction">Header to use in strResults</param>
        /// <param name="strHeaderIntensity">Header to use in strResults</param>
        /// <param name="blnUseFactorials">Set to true to use Factorial math, which is typically slower; default is False</param>
        /// <param name="blnAddProtonChargeCarrier">If blnAddProtonChargeCarrier is False, then still convolutes by charge, but doesn't add a proton</param>
        /// <returns>0 if success, -1 if an error</returns>
        /// <remarks>
        /// Returns uncharged mass values if intChargeState=0,
        /// Returns M+H values if intChargeState=1
        /// Returns convoluted m/z if intChargeState is > 1
        /// </remarks>
        public short ComputeIsotopicAbundancesInternal(
            ref string strFormulaIn,
            short intChargeState,
            ref string strResults,
            ref double[,] ConvolutedMSData2DOneBased,
            ref int ConvolutedMSDataCount,
            string strHeaderIsotopicAbundances,
            string strHeaderMassToCharge,
            string strHeaderFraction,
            string strHeaderIntensity,
            bool blnUseFactorials,
            bool blnAddProtonChargeCarrier)
        {
            var udtComputationStats = new udtComputationStatsType();
            udtComputationStats.Initialize();

            double dblNextComboFractionalAbundance = default;

            const string strDeuteriumEquiv = "^2.014H";

            long PredictedConvIterations;

            const double MIN_ABUNDANCE_TO_KEEP = 0.000001d;
            const double CUTOFF_FOR_RATIO_METHOD = 0.00001d;

            var blnExplicitIsotopesPresent = default(bool);
            var ExplicitIsotopeCount = default(short);

            double dblLogRho = default;

            // Make sure formula is not blank
            if (string.IsNullOrEmpty(strFormulaIn))
            {
                return -1;
            }

            mAbortProcessing = false;
            try
            {
                short MasterElementIndex;
                double dblTemp;
                float sngPercentComplete;
                int PredictedCombos;
                int AtomCount;
                short IsotopeCount;
                // Change strHeaderMassToCharge to "Neutral Mass" if intChargeState = 0 and strHeaderMassToCharge is "Mass/Charge"
                if (intChargeState == 0)
                {
                    if (strHeaderMassToCharge == "Mass/Charge")
                    {
                        strHeaderMassToCharge = "Neutral Mass";
                    }
                }

                // Parse Formula to determine if valid and number of each element
                var strFormula = strFormulaIn;
                var dblWorkingFormulaMass = ParseFormulaPublic(ref strFormula, ref udtComputationStats, false);

                if (dblWorkingFormulaMass < 0d)
                {
                    // Error occurred; information is stored in ErrorParams
                    strResults = LookupMessage(350) + ": " + LookupMessage(ErrorParams.ErrorID);
                    return -1;
                }

                // See if Deuterium is present by looking for a fractional amount of Hydrogen
                // strFormula will contain a capital D followed by a number or another letter (or the end of formula)
                // If found, replace each D with ^2.014H and re-compute
                var dblCount = udtComputationStats.Elements[1].Count;
                if (Math.Abs(dblCount - (int)Math.Round(dblCount)) > float.Epsilon)
                {
                    // Deuterium is present
                    var strModifiedFormula = "";
                    short intIndex = 0;
                    while (intIndex <= strFormula.Length)
                    {
                        var blnReplaceDeuterium = false;
                        if (strFormula.Substring(intIndex, 1) == "D")
                        {
                            if (intIndex == strFormula.Length - 1)
                            {
                                blnReplaceDeuterium = true;
                            }
                            else
                            {
                                var intAsciiOfNext = (int)strFormula.Substring(intIndex + 1, 1)[0];
                                if (intAsciiOfNext < 97 || intAsciiOfNext > 122)
                                {
                                    blnReplaceDeuterium = true;
                                }
                            }

                            if (blnReplaceDeuterium)
                            {
                                if (intIndex > 0)
                                {
                                    strModifiedFormula = strFormula.Substring(0, Math.Min(strFormula.Length, intIndex - 1));
                                }

                                strModifiedFormula += strDeuteriumEquiv;
                                if (intIndex < strFormula.Length - 1)
                                {
                                    strModifiedFormula += strFormula.Substring(intIndex + 1);
                                }

                                strFormula = strModifiedFormula;
                                intIndex = 0;
                            }
                        }

                        intIndex++;
                    }

                    // Re-Parse Formula since D's are now ^2.014H
                    dblWorkingFormulaMass = ParseFormulaPublic(ref strFormula, ref udtComputationStats, false);

                    if (dblWorkingFormulaMass < 0d)
                    {
                        // Error occurred; information is stored in ErrorParams
                        strResults = LookupMessage(350) + ": " + LookupMessage(ErrorParams.ErrorID);
                        return -1;
                    }
                }

                // Make sure there are no fractional atoms present (need to specially handle Deuterium)
                for (var intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                {
                    dblCount = udtComputationStats.Elements[intElementIndex].Count;
                    if (Math.Abs(dblCount - (int)Math.Round(dblCount)) > float.Epsilon)
                    {
                        strResults = LookupMessage(350) + ": " + LookupMessage(805) + ": " + ElementStats[intElementIndex].Symbol + dblCount;
                        return -1;
                    }
                }

                // Remove occurrences of explicitly defined isotopes from the formula
                for (var intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                {
                    var element = udtComputationStats.Elements[intElementIndex];
                    if (element.IsotopeCount > 0)
                    {
                        blnExplicitIsotopesPresent = true;
                        ExplicitIsotopeCount += element.IsotopeCount;
                        for (var IsotopeIndex = 1; IsotopeIndex <= element.IsotopeCount; IsotopeIndex++)
                            element.Count -= element.Isotopes[IsotopeIndex].Count;
                    }
                }

                // Determine the number of elements present in strFormula
                short intElementCount = 0;
                for (var intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                {
                    if (udtComputationStats.Elements[intElementIndex].Used)
                    {
                        intElementCount = (short)(intElementCount + 1);
                    }
                }

                if (blnExplicitIsotopesPresent)
                {
                    intElementCount += ExplicitIsotopeCount;
                }

                if (intElementCount == 0 || Math.Abs(dblWorkingFormulaMass) < float.Epsilon)
                {
                    // No elements or no weight
                    return -1;
                }

                // The formula seems valid, so update strFormulaIn
                strFormulaIn = strFormula;

                // Reserve memory for IsoStats() array
                var IsoStats = new udtIsoResultsByElementType[intElementCount + 1];

                // Step through udtComputationStats.Elements() again and copy info into IsoStats()
                // In addition, determine minimum and maximum weight for the molecule
                intElementCount = 0;
                var MinWeight = 0;
                var MaxWeight = 0;
                for (var intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                {
                    if (udtComputationStats.Elements[intElementIndex].Used)
                    {
                        if (udtComputationStats.Elements[intElementIndex].Count > 0d)
                        {
                            intElementCount = (short)(intElementCount + 1);
                            IsoStats[intElementCount].ElementIndex = (short)intElementIndex;
                            IsoStats[intElementCount].AtomCount = (int)Math.Round(udtComputationStats.Elements[intElementIndex].Count); // Note: Ignoring .Elements(intElementIndex).IsotopicCorrection
                            IsoStats[intElementCount].ExplicitMass = ElementStats[intElementIndex].Mass;

                            var stats = ElementStats[intElementIndex];
                            MinWeight = (int)Math.Round(MinWeight + IsoStats[intElementCount].AtomCount * Math.Round(stats.Isotopes[1].Mass, 0));
                            MaxWeight = (int)Math.Round(MaxWeight + IsoStats[intElementCount].AtomCount * Math.Round(stats.Isotopes[stats.IsotopeCount].Mass, 0));
                        }
                    }
                }

                if (blnExplicitIsotopesPresent)
                {
                    // Add the isotopes, pretending they are unique elements
                    for (var intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                    {
                        var element = udtComputationStats.Elements[intElementIndex];
                        if (element.IsotopeCount > 0)
                        {
                            for (var IsotopeIndex = 1; IsotopeIndex <= element.IsotopeCount; IsotopeIndex++)
                            {
                                intElementCount = (short)(intElementCount + 1);

                                IsoStats[intElementCount].boolExplicitIsotope = true;
                                IsoStats[intElementCount].ElementIndex = (short)intElementIndex;
                                IsoStats[intElementCount].AtomCount = (int)Math.Round(element.Isotopes[IsotopeIndex].Count);
                                IsoStats[intElementCount].ExplicitMass = element.Isotopes[IsotopeIndex].Mass;

                                var stats = IsoStats[intElementCount];
                                MinWeight = (int)Math.Round(MinWeight + stats.AtomCount * stats.ExplicitMass);
                                MaxWeight = (int)Math.Round(MaxWeight + stats.AtomCount * stats.ExplicitMass);
                            }
                        }
                    }
                }

                if (MinWeight < 0)
                    MinWeight = 0;

                // Create an array to hold the Fractional Abundances for all the masses
                ConvolutedMSDataCount = MaxWeight - MinWeight + 1;
                var ConvolutedAbundanceStartMass = MinWeight;
                var ConvolutedAbundances = new udtIsoResultsOverallType[ConvolutedMSDataCount + 1]; // Fractional abundance at each mass; 1-based array

                // Predict the total number of computations required; show progress if necessary
                var PredictedTotalComboCalcs = 0;
                for (var intElementIndex = 1; intElementIndex <= intElementCount; intElementIndex++)
                {
                    MasterElementIndex = IsoStats[intElementIndex].ElementIndex;
                    AtomCount = IsoStats[intElementIndex].AtomCount;
                    IsotopeCount = ElementStats[MasterElementIndex].IsotopeCount;

                    PredictedCombos = FindCombosPredictIterations(AtomCount, IsotopeCount);
                    PredictedTotalComboCalcs += PredictedCombos;
                }

                ResetProgress("Finding Isotopic Abundances: Computing abundances");

                // For each element, compute all of the possible combinations
                var CompletedComboCalcs = 0;
                for (var intElementIndex = 1; intElementIndex <= intElementCount; intElementIndex++)
                {
                    short IsotopeStartingMass;
                    short IsotopeEndingMass;
                    MasterElementIndex = IsoStats[intElementIndex].ElementIndex;
                    AtomCount = IsoStats[intElementIndex].AtomCount;

                    if (IsoStats[intElementIndex].boolExplicitIsotope)
                    {
                        IsotopeCount = 1;
                        IsotopeStartingMass = (short)Math.Round(IsoStats[intElementIndex].ExplicitMass);
                        IsotopeEndingMass = IsotopeStartingMass;
                    }
                    else
                    {
                        var stats = ElementStats[MasterElementIndex];
                        IsotopeCount = stats.IsotopeCount;
                        IsotopeStartingMass = (short)Math.Round(Math.Round(stats.Isotopes[1].Mass, 0));
                        IsotopeEndingMass = (short)Math.Round(Math.Round(stats.Isotopes[IsotopeCount].Mass, 0));
                    }

                    PredictedCombos = FindCombosPredictIterations(AtomCount, IsotopeCount);

                    if (PredictedCombos > 10000000)
                    {
                        var strMessage = "Too many combinations necessary for prediction of isotopic distribution: " + PredictedCombos.ToString("#,##0") + Environment.NewLine + "Please use a simpler formula or reduce the isotopic range defined for the element (currently " + IsotopeCount + ")";
                        if (ShowErrorMessageDialogs)
                        {
                            MessageBox.Show(strMessage);
                        }

                        LogMessage(strMessage, MessageType.Error);
                        return -1;
                    }

                    var IsoCombos = new int[PredictedCombos + 1, IsotopeCount + 1];
                    // 2D array: Holds the # of each isotope for each combination
                    // For example, Two chlorine atoms, Cl2, has at most 6 combos since Cl isotopes are 35, 36, and 37
                    // m1  m2  m3
                    // 2   0   0
                    // 1   1   0
                    // 1   0   1
                    // 0   2   0
                    // 0   1   1
                    // 0   0   2

                    var AtomTrackHistory = new int[IsotopeCount + 1];
                    AtomTrackHistory[1] = AtomCount;

                    var CombosFound = FindCombosRecurse(ref IsoCombos, AtomCount, IsotopeCount, IsotopeCount, 1, 1, ref AtomTrackHistory);

                    // The predicted value should always match the actual value, unless blnExplicitIsotopesPresent = True
                    if (!blnExplicitIsotopesPresent)
                    {
                        if (PredictedCombos != CombosFound)
                        {
                            Console.WriteLine("PredictedCombos doesn't match CombosFound (" + PredictedCombos + " vs. " + CombosFound + "); this is unexpected");
                        }
                    }

                    // Reserve space for the abundances based on the minimum and maximum weight of the isotopes of the element
                    MinWeight = AtomCount * IsotopeStartingMass;
                    MaxWeight = AtomCount * IsotopeEndingMass;
                    var ResultingMassCountForElement = MaxWeight - MinWeight + 1;
                    IsoStats[intElementIndex].StartingResultsMass = MinWeight;
                    IsoStats[intElementIndex].ResultsCount = ResultingMassCountForElement;
                    IsoStats[intElementIndex].MassAbundances = new float[ResultingMassCountForElement + 1];

                    if (IsoStats[intElementIndex].boolExplicitIsotope)
                    {
                        // Explicitly defined isotope; there is only one "combo" and its abundance = 1
                        IsoStats[intElementIndex].MassAbundances[1] = 1f;
                    }
                    else
                    {
                        var dblFractionalAbundanceSaved = 0d;
                        for (var ComboIndex = 1; ComboIndex <= CombosFound; ComboIndex++)
                        {
                            int IndexToStoreAbundance;
                            CompletedComboCalcs += 1;

                            sngPercentComplete = CompletedComboCalcs / (float)PredictedTotalComboCalcs * 100f;
                            if (CompletedComboCalcs % 10 == 0)
                            {
                                UpdateProgress(sngPercentComplete);
                            }

                            double dblThisComboFractionalAbundance = -1;
                            var blnRatioMethodUsed = false;
                            var blnRigorousMethodUsed = false;

                            if (blnUseFactorials)
                            {
                                // #######
                                // Rigorous, slow, easily overflowed method
                                // #######
                                //
                                blnRigorousMethodUsed = true;

                                // AbundDenom  and  AbundSuffix are only needed if using the easily-overflowed factorial method
                                var AbundDenom = 1d;
                                var AbundSuffix = 1d;
                                var stats = ElementStats[MasterElementIndex];
                                for (var IsotopeIndex = 1; IsotopeIndex <= IsotopeCount; IsotopeIndex++)
                                {
                                    var IsotopeCountInThisCombo = IsoCombos[ComboIndex, IsotopeIndex];
                                    if (IsotopeCountInThisCombo > 0)
                                    {
                                        AbundDenom *= Factorial((short)IsotopeCountInThisCombo);
                                        AbundSuffix *= Math.Pow(stats.Isotopes[IsotopeIndex].Abundance, IsotopeCountInThisCombo);
                                    }
                                }

                                dblThisComboFractionalAbundance = Factorial((short)AtomCount) / AbundDenom * AbundSuffix;
                            }
                            else
                            {
                                if (dblFractionalAbundanceSaved < CUTOFF_FOR_RATIO_METHOD)
                                {
                                    // #######
                                    // Equivalent of rigorous method, but uses logarithms
                                    // #######
                                    //
                                    blnRigorousMethodUsed = true;

                                    var dblLogSigma = 0d;
                                    for (var sigma = 1; sigma <= AtomCount; sigma++)
                                        dblLogSigma += Math.Log(sigma);

                                    var dblSumI = 0d;
                                    for (var IsotopeIndex = 1; IsotopeIndex <= IsotopeCount; IsotopeIndex++)
                                    {
                                        if (IsoCombos[ComboIndex, IsotopeIndex] > 0)
                                        {
                                            var dblWorkingSum = 0d;
                                            for (var SubIndex = 1; SubIndex <= IsoCombos[ComboIndex, IsotopeIndex]; SubIndex++)
                                                dblWorkingSum += Math.Log(SubIndex);

                                            dblSumI += dblWorkingSum;
                                        }
                                    }

                                    var stats = ElementStats[MasterElementIndex];
                                    var dblSumF = 0d;
                                    for (var IsotopeIndex = 1; IsotopeIndex <= IsotopeCount; IsotopeIndex++)
                                    {
                                        if (stats.Isotopes[IsotopeIndex].Abundance > 0f)
                                        {
                                            dblSumF += IsoCombos[ComboIndex, IsotopeIndex] * Math.Log(stats.Isotopes[IsotopeIndex].Abundance);
                                        }
                                    }

                                    var dblLogFreq = dblLogSigma - dblSumI + dblSumF;
                                    dblThisComboFractionalAbundance = Math.Exp(dblLogFreq);

                                    dblFractionalAbundanceSaved = dblThisComboFractionalAbundance;
                                }

                                // Use dblThisComboFractionalAbundance to predict
                                // the Fractional Abundance of the Next Combo
                                if (ComboIndex < CombosFound && dblFractionalAbundanceSaved >= CUTOFF_FOR_RATIO_METHOD)
                                {
                                    // #######
                                    // Third method, determines the ratio of this combo's abundance and the next combo's abundance
                                    // #######
                                    //
                                    var dblRatioOfFreqs = 1d;

                                    for (var IsotopeIndex = 1; IsotopeIndex <= IsotopeCount; IsotopeIndex++)
                                    {
                                        double intM = IsoCombos[ComboIndex, IsotopeIndex];
                                        double intMPrime = IsoCombos[ComboIndex + 1, IsotopeIndex];

                                        if (intM > intMPrime)
                                        {
                                            var dblLogSigma = 0d;
                                            for (var SubIndex = (int)Math.Round(intMPrime) + 1; SubIndex <= (int)Math.Round(intM); SubIndex++)
                                                dblLogSigma += Math.Log(SubIndex);

                                            dblLogRho = dblLogSigma - (intM - intMPrime) * Math.Log(ElementStats[MasterElementIndex].Isotopes[IsotopeIndex].Abundance);
                                        }
                                        else if (intM < intMPrime)
                                        {
                                            var dblLogSigma = 0d;
                                            for (var SubIndex = (int)Math.Round(intM) + 1; SubIndex <= (int)Math.Round(intMPrime); SubIndex++)
                                                dblLogSigma += Math.Log(SubIndex);

                                            var stats = ElementStats[MasterElementIndex];
                                            if (stats.Isotopes[IsotopeIndex].Abundance > 0f)
                                            {
                                                dblLogRho = (intMPrime - intM) * Math.Log(stats.Isotopes[IsotopeIndex].Abundance) - dblLogSigma;
                                            }
                                        }
                                        else
                                        {
                                            // intM = intMPrime
                                            dblLogRho = 0d;
                                        }

                                        var dblRho = Math.Exp(dblLogRho);
                                        dblRatioOfFreqs *= dblRho;
                                    }

                                    dblNextComboFractionalAbundance = dblFractionalAbundanceSaved * dblRatioOfFreqs;

                                    dblFractionalAbundanceSaved = dblNextComboFractionalAbundance;
                                    blnRatioMethodUsed = true;
                                }
                            }

                            if (blnRigorousMethodUsed)
                            {
                                // Determine nominal mass of this combination; depends on number of atoms of each isotope
                                IndexToStoreAbundance = FindIndexForNominalMass(ref IsoCombos, ComboIndex, IsotopeCount, AtomCount, ref ElementStats[MasterElementIndex].Isotopes);

                                // Store the abundance in .MassAbundances() at location IndexToStoreAbundance
                                IsoStats[intElementIndex].MassAbundances[IndexToStoreAbundance] = (float)(IsoStats[intElementIndex].MassAbundances[IndexToStoreAbundance] + dblThisComboFractionalAbundance);
                            }

                            if (blnRatioMethodUsed)
                            {
                                // Store abundance for next Combo
                                IndexToStoreAbundance = FindIndexForNominalMass(ref IsoCombos, ComboIndex + 1, IsotopeCount, AtomCount, ref ElementStats[MasterElementIndex].Isotopes);

                                // Store the abundance in .MassAbundances() at location IndexToStoreAbundance
                                IsoStats[intElementIndex].MassAbundances[IndexToStoreAbundance] = (float)(IsoStats[intElementIndex].MassAbundances[IndexToStoreAbundance] + dblNextComboFractionalAbundance);
                            }

                            if (blnRatioMethodUsed && ComboIndex + 1 == CombosFound)
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
                    strResults = LookupMessage(940);
                    return -1;
                }

                // Step Through IsoStats from the end to the beginning, shortening the length to the
                // first value greater than MIN_ABUNDANCE_TO_KEEP
                // This greatly speeds up the convolution
                for (var intElementIndex = 1; intElementIndex <= intElementCount; intElementIndex++)
                {
                    var stats = IsoStats[intElementIndex];
                    var index = stats.ResultsCount;
                    while (stats.MassAbundances[index] < MIN_ABUNDANCE_TO_KEEP)
                    {
                        index -= 1;
                        if (index == 1)
                            break;
                    }

                    stats.ResultsCount = index;
                }

                // Examine IsoStats() to predict the number of ConvolutionIterations
                PredictedConvIterations = IsoStats[1].ResultsCount;
                for (var intElementIndex = 2; intElementIndex <= intElementCount; intElementIndex++)
                    PredictedConvIterations *= IsoStats[2].ResultsCount;

                ResetProgress("Finding Isotopic Abundances: Convoluting results");

                // Convolute the results for each element using a recursive convolution routine
                var ConvolutionIterations = 0L;
                for (var index = 1; index <= IsoStats[1].ResultsCount; index++)
                {
                    ConvoluteMasses(ref ConvolutedAbundances, ConvolutedAbundanceStartMass, index, 1f, 0, 1, ref IsoStats, intElementCount, ref ConvolutionIterations);

                    sngPercentComplete = index / (float)IsoStats[1].ResultsCount * 100f;
                    UpdateProgress(sngPercentComplete);
                }

                if (mAbortProcessing)
                {
                    // Process Aborted
                    strResults = LookupMessage(940);
                    return -1;
                }

                // Compute mass defect (difference of initial isotope's mass from integer mass)
                var dblExactBaseIsoMass = 0d;
                for (var intElementIndex = 1; intElementIndex <= intElementCount; intElementIndex++)
                {
                    var stats = IsoStats[intElementIndex];
                    if (stats.boolExplicitIsotope)
                    {
                        dblExactBaseIsoMass += stats.AtomCount * stats.ExplicitMass;
                    }
                    else
                    {
                        dblExactBaseIsoMass += stats.AtomCount * ElementStats[stats.ElementIndex].Isotopes[1].Mass;
                    }
                }

                var dblMassDefect = Math.Round(dblExactBaseIsoMass - ConvolutedAbundanceStartMass, 5);

                // Assure that the mass defect is only a small percentage of the total mass
                // This won't be true for very small compounds so dblTemp is set to at least 10
                if (ConvolutedAbundanceStartMass < 10)
                {
                    dblTemp = 10d;
                }
                else
                {
                    dblTemp = ConvolutedAbundanceStartMass;
                }

                var dblMaxPercentDifference = Math.Pow(10d, -(3d - Math.Round(Math.Log10(dblTemp), 0)));

                if (Math.Abs(dblMassDefect / dblExactBaseIsoMass) >= dblMaxPercentDifference)
                {
                    Console.WriteLine("dblMassDefect / dblExactBaseIsoMass is greater dblMaxPercentDifference: (" + dblMassDefect / dblExactBaseIsoMass + " vs. " + dblMaxPercentDifference + "); this is unexpected");
                }

                // Step Through ConvolutedAbundances(), starting at the end, and find the first value above MIN_ABUNDANCE_TO_KEEP
                // Decrease ConvolutedMSDataCount to remove the extra values below MIN_ABUNDANCE_TO_KEEP
                for (var massIndex = ConvolutedMSDataCount; massIndex >= 1; massIndex -= 1)
                {
                    if (ConvolutedAbundances[massIndex].Abundance > MIN_ABUNDANCE_TO_KEEP)
                    {
                        ConvolutedMSDataCount = massIndex;
                        break;
                    }
                }

                var strOutput = strHeaderIsotopicAbundances + " " + strFormula + Environment.NewLine;
                strOutput = strOutput + SpacePad("  " + strHeaderMassToCharge, 12) + "\t" + SpacePad(strHeaderFraction, 9) + "\t" + strHeaderIntensity + Environment.NewLine;

                // Initialize ConvolutedMSData2DOneBased()
                ConvolutedMSData2DOneBased = new double[ConvolutedMSDataCount + 1, 3];

                // Find Maximum Abundance
                var dblMaxAbundance = 0d;
                for (var massIndex = 1; massIndex <= ConvolutedMSDataCount; massIndex++)
                {
                    if (ConvolutedAbundances[massIndex].Abundance > dblMaxAbundance)
                    {
                        dblMaxAbundance = ConvolutedAbundances[massIndex].Abundance;
                    }
                }

                // Populate the results array with the masses and abundances
                // Also, if intChargeState is >= 1, then convolute the mass to the appropriate m/z
                if (Math.Abs(dblMaxAbundance) < float.Epsilon)
                    dblMaxAbundance = 1d;
                for (var massIndex = 1; massIndex <= ConvolutedMSDataCount; massIndex++)
                {
                    var mass = ConvolutedAbundances[massIndex];
                    ConvolutedMSData2DOneBased[massIndex, 0] = ConvolutedAbundanceStartMass + massIndex - 1 + dblMassDefect;
                    ConvolutedMSData2DOneBased[massIndex, 1] = mass.Abundance / dblMaxAbundance * 100d;

                    if (intChargeState >= 1)
                    {
                        if (blnAddProtonChargeCarrier)
                        {
                            ConvolutedMSData2DOneBased[massIndex, 0] = ConvoluteMassInternal(ConvolutedMSData2DOneBased[massIndex, 0], 0, intChargeState);
                        }
                        else
                        {
                            ConvolutedMSData2DOneBased[massIndex, 0] = ConvolutedMSData2DOneBased[massIndex, 0] / intChargeState;
                        }
                    }
                }

                // Step through ConvolutedMSData2DOneBased() from the beginning to find the
                // first value greater than MIN_ABUNDANCE_TO_KEEP
                var rowIndex = 1;
                while (ConvolutedMSData2DOneBased[rowIndex, 1] < MIN_ABUNDANCE_TO_KEEP)
                {
                    rowIndex += 1;
                    if (rowIndex == ConvolutedMSDataCount)
                        break;
                }

                // If rowIndex > 1 then remove rows from beginning since value is less than MIN_ABUNDANCE_TO_KEEP
                if (rowIndex > 1 && rowIndex < ConvolutedMSDataCount)
                {
                    rowIndex -= 1;
                    // Remove rows from the start of ConvolutedMSData2DOneBased() since 0 mass
                    for (var massIndex = rowIndex + 1; massIndex <= ConvolutedMSDataCount; massIndex++)
                    {
                        ConvolutedMSData2DOneBased[massIndex - rowIndex, 0] = ConvolutedMSData2DOneBased[massIndex, 0];
                        ConvolutedMSData2DOneBased[massIndex - rowIndex, 1] = ConvolutedMSData2DOneBased[massIndex, 1];
                    }

                    ConvolutedMSDataCount -= rowIndex;
                }

                // Write to strOutput
                for (var massIndex = 1; massIndex <= ConvolutedMSDataCount; massIndex++)
                {
                    strOutput = strOutput + SpacePadFront(ConvolutedMSData2DOneBased[massIndex, 0].ToString("#0.00000"), 12) + "\t";
                    strOutput = strOutput + (ConvolutedMSData2DOneBased[massIndex, 1] * dblMaxAbundance / 100d).ToString("0.0000000") + "\t";
                    strOutput = strOutput + SpacePadFront(ConvolutedMSData2DOneBased[massIndex, 1].ToString("##0.00"), 7) + Environment.NewLine;
                    //ToDo: Fix Multiplicity
                    //strOutput = strOutput + ConvolutedAbundances(massIndex).Multiplicity.ToString("##0") + Environment.NewLine
                }

                strResults = strOutput;
            }
            catch
            {
                MwtWinDllErrorHandler("MwtWinDll|ComputeIsotopicAbundances");
                ErrorParams.ErrorID = 590;
                ErrorParams.ErrorPosition = 0;
                return -1;
            }

            return 0; // Success
        }

        /// <summary>
        /// Compute percent composition of the elements defined in udtComputationStats
        /// </summary>
        /// <param name="udtComputationStats">Input/output</param>
        public void ComputePercentComposition(ref udtComputationStatsType udtComputationStats)
        {
            // Determine the number of elements in the formula
            for (var intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
            {
                if (udtComputationStats.TotalMass > 0d)
                {
                    var dblElementTotalMass = udtComputationStats.Elements[intElementIndex].Count * ElementStats[intElementIndex].Mass + udtComputationStats.Elements[intElementIndex].IsotopicCorrection;

                    // Percent is the percent composition
                    var dblPercentComp = dblElementTotalMass / udtComputationStats.TotalMass * 100.0d;
                    udtComputationStats.PercentCompositions[intElementIndex].PercentComposition = dblPercentComp;

                    // Calculate standard deviation
                    double dblStdDeviation;
                    if (Math.Abs(udtComputationStats.Elements[intElementIndex].IsotopicCorrection - 0d) < float.Epsilon)
                    {
                        // No isotopic mass correction factor exists
                        dblStdDeviation = dblPercentComp * Math.Sqrt(Math.Pow(ElementStats[intElementIndex].Uncertainty / ElementStats[intElementIndex].Mass, 2d) + Math.Pow(udtComputationStats.StandardDeviation / udtComputationStats.TotalMass, 2d));
                    }
                    else
                    {
                        // Isotopic correction factor exists, assume no error in it
                        dblStdDeviation = dblPercentComp * Math.Sqrt(Math.Pow(udtComputationStats.StandardDeviation / udtComputationStats.TotalMass, 2d));
                    }

                    if (Math.Abs(dblElementTotalMass - udtComputationStats.TotalMass) < float.Epsilon && Math.Abs(dblPercentComp - 100d) < float.Epsilon)
                    {
                        dblStdDeviation = 0d;
                    }

                    udtComputationStats.PercentCompositions[intElementIndex].StdDeviation = dblStdDeviation;
                }
                else
                {
                    udtComputationStats.PercentCompositions[intElementIndex].PercentComposition = 0d;
                    udtComputationStats.PercentCompositions[intElementIndex].StdDeviation = 0d;
                }
            }
        }

        /// <summary>
        /// Convert the centroided data (stick data) in XYVals to a Gaussian representation
        /// </summary>
        /// <param name="XYVals">XY data, as key-value pairs</param>
        /// <param name="intResolution">Effective instrument resolution (e.g. 1000 or 20000)</param>
        /// <param name="dblResolutionMass">The m/z value at which the resolution applies</param>
        /// <param name="intQualityFactor">Gaussian quality factor (between 1 and 75, default is 50)</param>
        /// <returns>Gaussian spectrum data</returns>
        /// <remarks></remarks>
        public List<KeyValuePair<double, double>> ConvertStickDataToGaussian2DArray(List<KeyValuePair<double, double>> XYVals, int intResolution, double dblResolutionMass, int intQualityFactor)
        {
            // dblXVals() and dblYVals() are parallel arrays, 0-based (thus ranging from 0 to XYVals.count-1)
            // The arrays should contain stick data
            // The original data in the arrays will be replaced with Gaussian peaks in place of each "stick"
            // Note: Assumes dblXVals() is sorted in the x direction

            const int MAX_DATA_POINTS = 1000000;
            const short MASS_PRECISION = 7;

            var udtThisDataPoint = new udtXYDataType();

            var lstGaussianData = new List<KeyValuePair<double, double>>();

            try
            {
                double dblXValRange;
                if (XYVals == null || XYVals.Count == 0)
                {
                    return lstGaussianData;
                }

                var lstXYSummation = new List<udtXYDataType>(XYVals.Count * 10);

                // Determine the data range for dblXVals() and dblYVals()
                if (XYVals.Count > 1)
                {
                    dblXValRange = XYVals.Last().Key - XYVals.First().Key;
                }
                else
                {
                    dblXValRange = 1d;
                }

                if (dblXValRange < 1d)
                    dblXValRange = 1d;

                if (intResolution < 1)
                    intResolution = 1;

                if (intQualityFactor < 1 || intQualityFactor > 75)
                    intQualityFactor = 50;

                // Compute DeltaX using .intResolution and .intResolutionMass
                // Do not allow the DeltaX to be so small that the total points required > MAX_DATA_POINTS
                var DeltaX = dblResolutionMass / intResolution / intQualityFactor;

                // Make sure DeltaX is a reasonable number
                DeltaX = RoundToMultipleOf10(DeltaX);

                if (Math.Abs(DeltaX) < float.Epsilon)
                    DeltaX = 1d;

                // Set the Window Range to 1/10 the magnitude of the midpoint x value
                var dblRangeWork = XYVals.First().Key + dblXValRange / 2d;
                dblRangeWork = RoundToMultipleOf10(dblRangeWork);

                var dblSigma = dblResolutionMass / intResolution / Math.Sqrt(5.54d);

                // Set the window range (the xValue window width range) to calculate the Gaussian representation for each data point
                // The width at the base of a peak is 4 dblSigma
                // Use a width of 2 * 6 dblSigma
                var dblXValWindowRange = 2 * 6 * dblSigma;

                if (dblXValRange / DeltaX > MAX_DATA_POINTS)
                {
                    // Delta x is too small; change to a reasonable value
                    // This isn't a bug, but it may mean one of the default settings is inappropriate
                    DeltaX = dblXValRange / MAX_DATA_POINTS;
                }

                var intDataToAddCount = (int)Math.Round(dblXValWindowRange / DeltaX);

                // Make sure intDataToAddCount is odd
                if (intDataToAddCount % 2 == 0)
                {
                    intDataToAddCount += 1;
                }

                var lstDataToAdd = new List<udtXYDataType>(intDataToAddCount);
                var intMidPointIndex = (int)Math.Round((intDataToAddCount + 1) / 2d - 1d);

                // Compute the Gaussian data for each point in dblXVals()
                for (var intStickIndex = 0; intStickIndex < XYVals.Count; intStickIndex++)
                {
                    if (intStickIndex % 25 == 0)
                    {
                        if (AbortProcessing)
                            break;
                    }

                    // Search through lstXYSummation to determine the index of the smallest XValue with which
                    // data in lstDataToAdd could be combined
                    var intMinimalSummationIndex = 0;
                    lstDataToAdd.Clear();

                    var dblMinimalXValOfWindow = XYVals[intStickIndex].Key - intMidPointIndex * DeltaX;

                    var blnSearchForMinimumXVal = true;
                    if (lstXYSummation.Count > 0)
                    {
                        if (dblMinimalXValOfWindow > lstXYSummation[lstXYSummation.Count - 1].X)
                        {
                            intMinimalSummationIndex = lstXYSummation.Count - 1;
                            blnSearchForMinimumXVal = false;
                        }
                    }

                    if (blnSearchForMinimumXVal)
                    {
                        if (lstXYSummation.Count <= 0)
                        {
                            intMinimalSummationIndex = 0;
                        }
                        else
                        {
                            int intSummationIndex;
                            for (intSummationIndex = 0; intSummationIndex < lstXYSummation.Count; intSummationIndex++)
                            {
                                if (lstXYSummation[intSummationIndex].X >= dblMinimalXValOfWindow)
                                {
                                    intMinimalSummationIndex = intSummationIndex - 1;
                                    if (intMinimalSummationIndex < 0)
                                        intMinimalSummationIndex = 0;
                                    break;
                                }
                            }

                            if (intSummationIndex >= lstXYSummation.Count)
                            {
                                intMinimalSummationIndex = lstXYSummation.Count - 1;
                            }
                        }
                    }

                    // Construct the Gaussian representation for this Data Point
                    udtThisDataPoint.X = XYVals[intStickIndex].Key;
                    udtThisDataPoint.Y = XYVals[intStickIndex].Value;

                    // Round ThisDataPoint.XVal to the nearest DeltaX
                    // If .XVal is not an even multiple of DeltaX then bump up .XVal until it is
                    udtThisDataPoint.X = RoundToEvenMultiple(udtThisDataPoint.X, DeltaX, true);

                    for (var index = 0; index < intDataToAddCount; index++)
                    {
                        // Equation for Gaussian is: Amplitude * Exp[ -(x - mu)^2 / (2*dblSigma^2) ]
                        // Use index, .YVal, and DeltaX
                        var dblXOffSet = (intMidPointIndex - index) * DeltaX;

                        var udtNewPoint = new udtXYDataType()
                        {
                            X = udtThisDataPoint.X - dblXOffSet,
                            Y = udtThisDataPoint.Y * Math.Exp(-Math.Pow(dblXOffSet, 2d) / (2d * Math.Pow(dblSigma, 2d)))
                        };

                        lstDataToAdd.Add(udtNewPoint);
                    }

                    // Now merge lstDataToAdd into lstXYSummation
                    // XValues in lstDataToAdd and those in lstXYSummation have the same DeltaX value
                    // The XValues in lstDataToAdd might overlap partially with those in lstXYSummation

                    var intDataIndex = 0;
                    bool blnAppendNewData;

                    // First, see if the first XValue in lstDataToAdd is larger than the last XValue in lstXYSummation
                    if (lstXYSummation.Count <= 0)
                    {
                        blnAppendNewData = true;
                    }
                    else if (lstDataToAdd[intDataIndex].X > lstXYSummation.Last().X)
                    {
                        blnAppendNewData = true;
                    }
                    else
                    {
                        blnAppendNewData = false;
                        // Step through lstXYSummation starting at intMinimalSummationIndex, looking for
                        // the index to start combining data at
                        for (var intSummationIndex = intMinimalSummationIndex; intSummationIndex < lstXYSummation.Count; intSummationIndex++)
                        {
                            if (Math.Round(lstDataToAdd[intDataIndex].X, MASS_PRECISION) <= Math.Round(lstXYSummation[intSummationIndex].X, MASS_PRECISION))
                            {

                                // Within Tolerance; start combining the values here
                                while (intSummationIndex <= lstXYSummation.Count - 1)
                                {
                                    var udtCurrentVal = lstXYSummation[intSummationIndex];
                                    udtCurrentVal.Y += lstDataToAdd[intDataIndex].Y;

                                    lstXYSummation[intSummationIndex] = udtCurrentVal;

                                    intSummationIndex += 1;
                                    intDataIndex += 1;
                                    if (intDataIndex >= intDataToAddCount)
                                    {
                                        // Successfully combined all of the data
                                        break;
                                    }
                                }

                                if (intDataIndex < intDataToAddCount)
                                {
                                    // Data still remains to be added
                                    blnAppendNewData = true;
                                }

                                break;
                            }
                        }
                    }

                    if (blnAppendNewData == true)
                    {
                        while (intDataIndex < intDataToAddCount)
                        {
                            lstXYSummation.Add(lstDataToAdd[intDataIndex]);
                            intDataIndex += 1;
                        }
                    }
                }

                // Assure there is a data point at each 1% point along x range (to give better looking plots)
                // Probably need to add data, but may need to remove some
                var dblMinimalXValSpacing = dblXValRange / 100d;

                for (var intSummationIndex = 0; intSummationIndex < lstXYSummation.Count - 1; intSummationIndex++)
                {
                    if (lstXYSummation[intSummationIndex].X + dblMinimalXValSpacing < lstXYSummation[intSummationIndex + 1].X)
                    {
                        // Need to insert a data point

                        // Choose the appropriate new .XVal
                        dblRangeWork = lstXYSummation[intSummationIndex + 1].X - lstXYSummation[intSummationIndex].X;
                        if (dblRangeWork < dblMinimalXValSpacing * 2d)
                        {
                            dblRangeWork /= 2d;
                        }
                        else
                        {
                            dblRangeWork = dblMinimalXValSpacing;
                        }

                        // The new .YVal is the average of that at intSummationIndex and that at intSummationIndex + 1
                        var udtNewDataPoint = new udtXYDataType()
                        {
                            X = lstXYSummation[intSummationIndex].X + dblRangeWork,
                            Y = (lstXYSummation[intSummationIndex].Y + lstXYSummation[intSummationIndex + 1].Y) / 2d
                        };

                        lstXYSummation.Insert(intSummationIndex + 1, udtNewDataPoint);
                    }
                }

                // Copy data from lstXYSummation to lstGaussianData

                foreach (var item in lstXYSummation)
                    lstGaussianData.Add(new KeyValuePair<double, double>(item.X, item.Y));
            }
            catch (Exception ex)
            {
                GeneralErrorHandler("ConvertStickDataToGaussian", ex);
            }

            return lstGaussianData;
        }

        public void ConstructMasterSymbolsList()
        {
            // Call after loading or changing abbreviations or elements
            // Call after loading or setting abbreviation mode

            MasterSymbolsList = new string[ELEMENT_COUNT + AbbrevAllCount + 1, 2];

            // MasterSymbolsList(,0) contains the symbol to be matched
            // MasterSymbolsList(,1) contains E for element, A for amino acid, or N for normal abbreviation, followed by
            // the reference number in the master list
            // For example for Carbon, MasterSymbolsList(intIndex,0) = "C" and MasterSymbolsList(intIndex,1) = "E6"

            // Construct search list
            for (var intIndex = 1; intIndex <= ELEMENT_COUNT; intIndex++)
            {
                MasterSymbolsList[intIndex, 0] = ElementStats[intIndex].Symbol;
                MasterSymbolsList[intIndex, 1] = "E" + intIndex;
            }

            MasterSymbolsListCount = ELEMENT_COUNT;

            // Note: AbbrevStats is 1-based
            if (gComputationOptions.AbbrevRecognitionMode != MolecularWeightTool.AbbrevRecognitionMode.NoAbbreviations)
            {
                bool blnIncludeAmino;
                if (gComputationOptions.AbbrevRecognitionMode == MolecularWeightTool.AbbrevRecognitionMode.NormalPlusAminoAcids)
                {
                    blnIncludeAmino = true;
                }
                else
                {
                    blnIncludeAmino = false;
                }

                for (var intIndex = 1; intIndex <= AbbrevAllCount; intIndex++)
                {
                    var stats = AbbrevStats[intIndex];
                    // If blnIncludeAmino = False then do not include amino acids
                    if (blnIncludeAmino || !blnIncludeAmino && !stats.IsAminoAcid)
                    {
                        // Do not include if the formula is invalid
                        if (!stats.InvalidSymbolOrFormula)
                        {
                            MasterSymbolsListCount = (short)(MasterSymbolsListCount + 1);

                            MasterSymbolsList[MasterSymbolsListCount, 0] = stats.Symbol;
                            MasterSymbolsList[MasterSymbolsListCount, 1] = "A" + intIndex;
                        }
                    }
                }
            }

            // Sort the search list
            ShellSortSymbols(1, MasterSymbolsListCount);
        }

        /// <summary>
        /// Converts dblMassMZ to the MZ that would appear at the given intDesiredCharge
        /// </summary>
        /// <param name="dblMassMZ"></param>
        /// <param name="intCurrentCharge"></param>
        /// <returns>The new m/z value</returns>
        /// <remarks>To return the neutral mass, set intDesiredCharge to 0</remarks>
        public double ConvoluteMassInternal(double dblMassMZ, short intCurrentCharge)
        {
            return ConvoluteMassInternal(dblMassMZ, intCurrentCharge, 1, 0d);
        }

        /// <summary>
        /// Converts dblMassMZ to the MZ that would appear at the given intDesiredCharge
        /// </summary>
        /// <param name="dblMassMZ"></param>
        /// <param name="intCurrentCharge"></param>
        /// <param name="intDesiredCharge"></param>
        /// <returns>The new m/z value</returns>
        /// <remarks>To return the neutral mass, set intDesiredCharge to 0</remarks>
        public double ConvoluteMassInternal(double dblMassMZ, short intCurrentCharge, short intDesiredCharge)
        {
            return ConvoluteMassInternal(dblMassMZ, intCurrentCharge, intDesiredCharge, 0d);
        }

        /// <summary>
        /// Converts dblMassMZ to the MZ that would appear at the given intDesiredCharge
        /// </summary>
        /// <param name="dblMassMZ"></param>
        /// <param name="intCurrentCharge"></param>
        /// <param name="intDesiredCharge"></param>
        /// <param name="dblChargeCarrierMass">Charge carrier mass.  If 0, this function will use mChargeCarrierMass instead</param>
        /// <returns>The new m/z value</returns>
        /// <remarks>To return the neutral mass, set intDesiredCharge to 0</remarks>
        public double ConvoluteMassInternal(
            double dblMassMZ,
            short intCurrentCharge,
            short intDesiredCharge,
            double dblChargeCarrierMass)
        {
            const double DEFAULT_CHARGE_CARRIER_MASS_MONOISO = 1.00727649d;

            double dblNewMZ;

            if (Math.Abs(dblChargeCarrierMass - 0d) < float.Epsilon)
                dblChargeCarrierMass = mChargeCarrierMass;
            if (Math.Abs(dblChargeCarrierMass - 0d) < float.Epsilon)
                dblChargeCarrierMass = DEFAULT_CHARGE_CARRIER_MASS_MONOISO;

            if (intCurrentCharge == intDesiredCharge)
            {
                dblNewMZ = dblMassMZ;
            }
            else
            {
                if (intCurrentCharge == 1)
                {
                    dblNewMZ = dblMassMZ;
                }
                else if (intCurrentCharge > 1)
                {
                    // Convert dblMassMZ to M+H
                    dblNewMZ = dblMassMZ * intCurrentCharge - dblChargeCarrierMass * (intCurrentCharge - 1);
                }
                else if (intCurrentCharge == 0)
                {
                    // Convert dblMassMZ (which is neutral) to M+H and store in dblNewMZ
                    dblNewMZ = dblMassMZ + dblChargeCarrierMass;
                }
                else
                {
                    // Negative charges are not supported; return 0
                    return 0d;
                }

                if (intDesiredCharge > 1)
                {
                    dblNewMZ = (dblNewMZ + dblChargeCarrierMass * (intDesiredCharge - 1)) / intDesiredCharge;
                }
                else if (intDesiredCharge == 1)
                {
                    // Return M+H, which is currently stored in dblNewMZ
                }
                else if (intDesiredCharge == 0)
                {
                    // Return the neutral mass
                    dblNewMZ -= dblChargeCarrierMass;
                }
                else
                {
                    // Negative charges are not supported; return 0
                    dblNewMZ = 0d;
                }
            }

            return dblNewMZ;
        }

        /// <summary>
        /// Converts strFormula to its corresponding empirical formula
        /// </summary>
        /// <param name="strFormula"></param>
        /// <returns>The empirical formula, or -1 if an error</returns>
        /// <remarks></remarks>
        public string ConvertFormulaToEmpirical(string strFormula)
        {
            var udtComputationStats = new udtComputationStatsType();
            udtComputationStats.Initialize();

            // Call ParseFormulaPublic to compute the formula's mass and fill udtComputationStats
            var dblMass = ParseFormulaPublic(ref strFormula, ref udtComputationStats);

            if (ErrorParams.ErrorID == 0)
            {
                // Convert to empirical formula
                var strEmpiricalFormula = "";
                // Carbon first, then hydrogen, then the rest alphabetically
                // This is correct to start at -1
                for (var intElementIndex = -1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                {
                    var intElementIndexToUse = default(int);
                    if (intElementIndex == -1)
                    {
                        // Do Carbon first
                        intElementIndexToUse = 6;
                    }
                    else if (intElementIndex == 0)
                    {
                        // Then do Hydrogen
                        intElementIndexToUse = 1;
                    }
                    else
                    {
                        // Then do the rest alphabetically
                        if (ElementAlph[intElementIndex] == "C" || ElementAlph[intElementIndex] == "H")
                        {
                            // Increment intElementIndex when we encounter carbon or hydrogen
                            intElementIndex = (short)(intElementIndex + 1);
                        }

                        for (var intElementSearchIndex = 2; intElementSearchIndex <= ELEMENT_COUNT; intElementSearchIndex++) // Start at 2 to since we've already done hydrogen
                        {
                            // find the element in the numerically ordered array that corresponds to the alphabetically ordered array
                            if ((ElementStats[intElementSearchIndex].Symbol ?? "") == (ElementAlph[intElementIndex] ?? ""))
                            {
                                intElementIndexToUse = intElementSearchIndex;
                                break;
                            }
                        }
                    }

                    // Only display the element if it's in the formula
                    var dblThisElementCount = mComputationStatsSaved.Elements[intElementIndexToUse].Count;
                    if (Math.Abs(dblThisElementCount - 1.0d) < float.Epsilon)
                    {
                        strEmpiricalFormula += ElementStats[intElementIndexToUse].Symbol;
                    }
                    else if (dblThisElementCount > 0d)
                    {
                        strEmpiricalFormula = strEmpiricalFormula + ElementStats[intElementIndexToUse].Symbol + dblThisElementCount;
                    }
                }

                return strEmpiricalFormula;
            }

            return (-1).ToString();
        }

        /// <summary>
        /// Expands abbreviations in formula to their elemental equivalent
        /// </summary>
        /// <param name="strFormula"></param>
        /// <returns>Returns the result, or -1 if an error</returns>
        /// <remarks></remarks>
        public string ExpandAbbreviationsInFormula(string strFormula)
        {
            var udtComputationStats = new udtComputationStatsType();
            udtComputationStats.Initialize();

            // Call ExpandAbbreviationsInFormula to compute the formula's mass
            var dblMass = ParseFormulaPublic(ref strFormula, ref udtComputationStats, true);

            if (ErrorParams.ErrorID == 0)
            {
                return strFormula;
            }

            return (-1).ToString();
        }

        private int FindIndexForNominalMass(
            ref int[,] IsoCombos,
            int ComboIndex,
            short IsotopeCount,
            int AtomCount,
            ref udtIsotopeInfoType[] ThisElementsIsotopes)
        {
            var workingMass = 0;
            for (var IsotopeIndex = 1; IsotopeIndex <= IsotopeCount; IsotopeIndex++)
                workingMass = (int)Math.Round(workingMass + IsoCombos[ComboIndex, IsotopeIndex] * Math.Round(ThisElementsIsotopes[IsotopeIndex].Mass, 0));

            // (workingMass  - IsoStats(ElementIndex).StartingResultsMass) + 1
            return (int)Math.Round(workingMass - AtomCount * Math.Round(ThisElementsIsotopes[1].Mass, 0)) + 1;
        }

        /// <summary>
        /// Recursive function to Convolute the Results in IsoStats() and store in ConvolutedAbundances(); 1-based array
        /// </summary>
        /// <param name="ConvolutedAbundances"></param>
        /// <param name="ConvolutedAbundanceStartMass"></param>
        /// <param name="WorkingRow"></param>
        /// <param name="WorkingAbundance"></param>
        /// <param name="WorkingMassTotal"></param>
        /// <param name="ElementTrack"></param>
        /// <param name="IsoStats"></param>
        /// <param name="ElementCount"></param>
        /// <param name="Iterations"></param>
        private void ConvoluteMasses(
            ref udtIsoResultsOverallType[] ConvolutedAbundances,
            int ConvolutedAbundanceStartMass,
            int WorkingRow,
            float WorkingAbundance,
            int WorkingMassTotal,
            short ElementTrack,
            ref udtIsoResultsByElementType[] IsoStats,
            short ElementCount,
            ref long Iterations)
        {
            if (mAbortProcessing)
                return;

            Iterations += 1L;
            if (Iterations % 10000L == 0L)
            {
                Application.DoEvents();
            }

            var NewAbundance = WorkingAbundance * IsoStats[ElementTrack].MassAbundances[WorkingRow];
            var NewMassTotal = WorkingMassTotal + (IsoStats[ElementTrack].StartingResultsMass + WorkingRow - 1);

            if (ElementTrack >= ElementCount)
            {
                var IndexToStoreResult = NewMassTotal - ConvolutedAbundanceStartMass + 1;
                var result = ConvolutedAbundances[IndexToStoreResult];
                if (NewAbundance > 0f)
                {
                    result.Abundance += NewAbundance;
                    result.Multiplicity += 1;
                }
            }
            else
            {
                for (var RowIndex = 1; RowIndex <= IsoStats[ElementTrack + 1].ResultsCount; RowIndex++)
                    ConvoluteMasses(ref ConvolutedAbundances, ConvolutedAbundanceStartMass, RowIndex, NewAbundance, NewMassTotal, (short)(ElementTrack + 1), ref IsoStats, ElementCount, ref Iterations);
            }
        }

        /// <summary>
        /// Compute the factorial of a number; uses recursion
        /// </summary>
        /// <param name="Number">Integer number between 0 and 170</param>
        /// <returns>The factorial, or -1 if an error</returns>
        /// <remarks></remarks>
        public double Factorial(short Number)
        {
            try
            {
                if (Number > 170)
                {
                    Console.WriteLine("Cannot compute factorial of a number over 170");
                    return -1;
                }

                if (Number < 0)
                {
                    Console.WriteLine("Cannot compute factorial of a negative number");
                    return -1;
                }

                if (Number == 0)
                {
                    return 1d;
                }

                return Number * Factorial((short)(Number - 1));
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
        /// <param name="AtomCount"></param>
        /// <param name="IsotopeCount"></param>
        /// <returns></returns>
        private int FindCombosPredictIterations(int AtomCount, short IsotopeCount)
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

            var RunningSum = new int[AtomCount + 1];
            try
            {
                int PredictedCombos;
                if (AtomCount == 1 || IsotopeCount == 1)
                {
                    PredictedCombos = IsotopeCount;
                }
                else
                {
                    // Initialize RunningSum()
                    for (var AtomIndex = 1; AtomIndex <= AtomCount; AtomIndex++)
                        RunningSum[AtomIndex] = AtomIndex + 1;

                    for (var IsotopeIndex = 3; IsotopeIndex <= IsotopeCount; IsotopeIndex++)
                    {
                        var PreviousComputedValue = IsotopeIndex;
                        for (var AtomIndex = 2; AtomIndex <= AtomCount; AtomIndex++)
                        {
                            // Compute new count for this AtomIndex
                            RunningSum[AtomIndex] = PreviousComputedValue + RunningSum[AtomIndex];

                            // Also place result in RunningSum(AtomIndex)
                            PreviousComputedValue = RunningSum[AtomIndex];
                        }
                    }

                    PredictedCombos = RunningSum[AtomCount];
                }

                return PredictedCombos;
            }
            catch
            {
                MwtWinDllErrorHandler("MwtWinDll|FindCombosPredictIterations");
                ErrorParams.ErrorID = 590;
                ErrorParams.ErrorPosition = 0;
                return -1;
            }
        }

        /// <summary>
        /// Recursive function to find all the combinations
        /// of a number of atoms with the given maximum isotopic count
        /// </summary>
        /// <param name="ComboResults"></param>
        /// <param name="AtomCount"></param>
        /// <param name="MaxIsotopeCount"></param>
        /// <param name="CurrentIsotopeCount"></param>
        /// <param name="CurrentRow"></param>
        /// <param name="CurrentCol"></param>
        /// <param name="AtomTrackHistory"></param>
        /// <returns></returns>
        private int FindCombosRecurse(
            ref int[,] ComboResults,
            int AtomCount,
            short MaxIsotopeCount,
            short CurrentIsotopeCount,
            int CurrentRow,
            short CurrentCol,
            ref int[] AtomTrackHistory)
        {
            // IsoCombos() is a 2D array holding the # of each isotope for each combination
            // For example, Two chlorine atoms, Cl2, has at most 6 combos since Cl isotopes are 35, 36, and 37
            // m1  m2  m3
            // 2   0   0
            // 1   1   0
            // 1   0   1
            // 0   2   0
            // 0   1   1
            // 0   0   2

            // Returns the number of combinations found, or -1 if an error

            if (CurrentIsotopeCount == 1 || AtomCount == 0)
            {
                // End recursion
                ComboResults[CurrentRow, CurrentCol] = AtomCount;
            }
            else
            {
                var AtomTrack = AtomCount;

                // Store AtomTrack value at current position
                ComboResults[CurrentRow, CurrentCol] = AtomTrack;

                while (AtomTrack > 0)
                {
                    CurrentRow += 1;

                    // Went to a new row; if CurrentCol > 1 then need to assign previous values to previous columns
                    if (CurrentCol > 1)
                    {
                        for (var ColIndex = 1; ColIndex < CurrentCol; ColIndex++)
                            ComboResults[CurrentRow, ColIndex] = AtomTrackHistory[ColIndex];
                    }

                    AtomTrack -= 1;
                    ComboResults[CurrentRow, CurrentCol] = AtomTrack;

                    if (CurrentCol < MaxIsotopeCount)
                    {
                        var intNewColumn = (short)(CurrentCol + 1);
                        AtomTrackHistory[intNewColumn - 1] = AtomTrack;
                        CurrentRow = FindCombosRecurse(ref ComboResults, AtomCount - AtomTrack, MaxIsotopeCount, (short)(CurrentIsotopeCount - 1), CurrentRow, intNewColumn, ref AtomTrackHistory);
                    }
                    else
                    {
                        Console.WriteLine("Program bug in FindCombosRecurse. This line should not be reached.");
                    }
                }

                // Reached AtomTrack = 0; end recursion
            }

            return CurrentRow;
        }

        public void GeneralErrorHandler(string strCallingProcedure, int errorNumber)
        {
            GeneralErrorHandler(strCallingProcedure, errorNumber, string.Empty);
        }

        public void GeneralErrorHandler(string strCallingProcedure, Exception ex)
        {
            GeneralErrorHandler(strCallingProcedure, 0, ex.Message);
        }

        public void GeneralErrorHandler(string strCallingProcedure, int errorNumber, string strErrorDescriptionAdditional)
        {
            var strMessage = "Error in " + strCallingProcedure + ": " + Conversion.ErrorToString(errorNumber) + " (#" + errorNumber + ")";
            if (!string.IsNullOrEmpty(strErrorDescriptionAdditional))
            {
                strMessage += Environment.NewLine + strErrorDescriptionAdditional;
            }

            LogMessage(strMessage, MessageType.Error);

            if (ShowErrorMessageDialogs)
            {
                MessageBox.Show(strMessage, "Error in MwtWinDll", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                Console.WriteLine(strMessage);
            }

            LogMessage(strMessage, MessageType.Error);
            try
            {
                var strErrorFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "ErrorLog.txt");

                // Open the file and append a new error entry
                using (var srOutFile = new System.IO.StreamWriter(strErrorFilePath, true))
                {
                    srOutFile.WriteLine(DateTime.Now + " -- " + strMessage + Environment.NewLine);
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
        /// <param name="strSymbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        public int GetAbbreviationIDInternal(string strSymbol)
        {
            return GetAbbreviationIDInternal(strSymbol, false);
        }

        /// <summary>
        /// Get the abbreviation ID for the given abbreviation symbol
        /// </summary>
        /// <param name="strSymbol"></param>
        /// <param name="blnAminoAcidsOnly"></param>
        /// <returns>ID if found, otherwise 0</returns>
        public int GetAbbreviationIDInternal(string strSymbol, bool blnAminoAcidsOnly)
        {
            for (var index = 1; index <= AbbrevAllCount; index++)
            {
                if ((AbbrevStats[index].Symbol?.ToLower() ?? "") == (strSymbol?.ToLower() ?? ""))
                {
                    if (!blnAminoAcidsOnly || blnAminoAcidsOnly && AbbrevStats[index].IsAminoAcid)
                    {
                        return index;
                    }
                }
            }

            return 0;
        }

        public int GetAbbreviationInternal(
            int abbreviationID,
            out string strSymbol,
            out string strFormula,
            out float sngCharge,
            out bool blnIsAminoAcid)
        {
            return GetAbbreviationInternal(abbreviationID, out strSymbol, out strFormula, out sngCharge, out blnIsAminoAcid, out _, out _, out _);
        }

        /// <summary>
        /// Get an abbreviation, by ID
        /// </summary>
        /// <param name="abbreviationID"></param>
        /// <param name="strSymbol">Output: symbol</param>
        /// <param name="strFormula">Output: empirical formula</param>
        /// <param name="sngCharge">Output: charge</param>
        /// <param name="blnIsAminoAcid">Output: true if an amino acid</param>
        /// <param name="strOneLetterSymbol">Output: one letter symbol (only used by amino acids)</param>
        /// <param name="strComment">Output: comment</param>
        /// <param name="blnInvalidSymbolOrFormula">Output: true if an invalid symbol or formula</param>
        /// <returns> 0 if success, 1 if failure</returns>
        public int GetAbbreviationInternal(
            int abbreviationID,
            out string strSymbol,
            out string strFormula,
            out float sngCharge,
            out bool blnIsAminoAcid,
            out string strOneLetterSymbol,
            out string strComment,
            out bool blnInvalidSymbolOrFormula)
        {
            if (abbreviationID >= 1 && abbreviationID <= AbbrevAllCount)
            {
                var stats = AbbrevStats[abbreviationID];
                strSymbol = stats.Symbol;
                strFormula = stats.Formula;
                sngCharge = stats.Charge;
                blnIsAminoAcid = stats.IsAminoAcid;
                strOneLetterSymbol = stats.OneLetterSymbol;
                strComment = stats.Comment;
                blnInvalidSymbolOrFormula = stats.InvalidSymbolOrFormula;

                return 0;
            }

            strSymbol = string.Empty;
            strFormula = string.Empty;
            sngCharge = 0f;
            blnIsAminoAcid = false;
            strOneLetterSymbol = string.Empty;
            strComment = string.Empty;
            blnInvalidSymbolOrFormula = true;

            return 1;
        }

        public double GetAbbreviationMass(int abbreviationID)
        {
            // Returns the mass if success, 0 if failure
            // Could return -1 if failure, but might mess up some calculations

            // This function does not recompute the abbreviation mass each time it is called
            // Rather, it uses the .Mass member of AbbrevStats
            // This requires that .Mass be updated if the abbreviation is changed, if an element is changed, or if the element mode is changed

            if (abbreviationID >= 1 && abbreviationID <= AbbrevAllCount)
            {
                return AbbrevStats[abbreviationID].Mass;
            }

            return 0d;
        }

        public string GetAminoAcidSymbolConversionInternal(string strSymbolToFind, bool bln1LetterTo3Letter)
        {
            // If bln1LetterTo3Letter = True, then converting 1 letter codes to 3 letter codes
            // Returns the symbol, if found
            // Otherwise, returns ""

            var strReturnSymbol = "";
            // Use AbbrevStats() array to lookup code
            for (var index = 1; index <= AbbrevAllCount; index++)
            {
                if (AbbrevStats[index].IsAminoAcid)
                {
                    string strCompareSymbol;
                    if (bln1LetterTo3Letter)
                    {
                        strCompareSymbol = AbbrevStats[index].OneLetterSymbol;
                    }
                    else
                    {
                        strCompareSymbol = AbbrevStats[index].Symbol;
                    }

                    if ((strCompareSymbol?.ToLower() ?? "") == (strSymbolToFind?.ToLower() ?? ""))
                    {
                        if (bln1LetterTo3Letter)
                        {
                            strReturnSymbol = AbbrevStats[index].Symbol;
                        }
                        else
                        {
                            strReturnSymbol = AbbrevStats[index].OneLetterSymbol;
                        }

                        break;
                    }
                }
            }

            return strReturnSymbol;
        }

        public int GetCautionStatementCountInternal()
        {
            return CautionStatementCount;
        }

        /// <summary>
        /// Get the caution statement ID for the given symbol combo
        /// </summary>
        /// <param name="strSymbolCombo"></param>
        /// <returns>Statement ID if found, otherwise -1</returns>
        public int GetCautionStatementIDInternal(string strSymbolCombo)
        {
            for (var intIndex = 1; intIndex <= CautionStatementCount; intIndex++)
            {
                if ((CautionStatements[intIndex, 0] ?? "") == (strSymbolCombo ?? ""))
                {
                    return intIndex;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get a caution statement, by ID
        /// </summary>
        /// <param name="cautionStatementID"></param>
        /// <param name="strSymbolCombo">Output: symbol combo for the caution statement</param>
        /// <param name="strCautionStatement">Output: caution statement text</param>
        /// <returns>0 if success, 1 if an invalid ID</returns>
        public int GetCautionStatementInternal(int cautionStatementID, out string strSymbolCombo, out string strCautionStatement)
        {
            if (cautionStatementID >= 1 && cautionStatementID <= CautionStatementCount)
            {
                strSymbolCombo = CautionStatements[cautionStatementID, 0];
                strCautionStatement = CautionStatements[cautionStatementID, 1];
                return 0;
            }

            strSymbolCombo = string.Empty;
            strCautionStatement = string.Empty;
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
        /// Returns the settings for the element with intElementID in the ByRef variables
        /// </summary>
        /// <param name="intElementID"></param>
        /// <param name="strSymbol"></param>
        /// <param name="dblMass"></param>
        /// <param name="dblUncertainty"></param>
        /// <param name="sngCharge"></param>
        /// <param name="intIsotopeCount"></param>
        /// <returns>0 if success, 1 if failure</returns>
        public int GetElementInternal(
            short intElementID,
            out string strSymbol,
            out double dblMass,
            out double dblUncertainty,
            out float sngCharge,
            out short intIsotopeCount)
        {
            if (intElementID >= 1 && intElementID <= ELEMENT_COUNT)
            {
                strSymbol = ElementAlph[intElementID];
                var stats = ElementStats[intElementID];
                strSymbol = stats.Symbol;
                dblMass = stats.Mass;
                dblUncertainty = stats.Uncertainty;
                sngCharge = stats.Charge;
                intIsotopeCount = stats.IsotopeCount;

                return 0;
            }

            strSymbol = string.Empty;
            dblMass = 0d;
            dblUncertainty = 0d;
            sngCharge = 0f;
            intIsotopeCount = 0;
            return 1;
        }

        /// <summary>
        /// Get the element ID for the given symbol
        /// </summary>
        /// <param name="strSymbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        public short GetElementIDInternal(string strSymbol)
        {
            for (var intIndex = 1; intIndex <= ELEMENT_COUNT; intIndex++)
            {
                if (string.Equals(ElementStats[intIndex].Symbol, strSymbol, StringComparison.InvariantCultureIgnoreCase))
                {
                    return (short)intIndex;
                }
            }

            return 0;
        }

        /// <summary>
        /// Returns the isotope masses and abundances for the element with intElementID
        /// </summary>
        /// <param name="intElementID"></param>
        /// <param name="intIsotopeCount"></param>
        /// <param name="dblIsotopeMasses"></param>
        /// <param name="sngIsotopeAbundances"></param>
        /// <returns>0 if a valid ID, 1 if invalid</returns>
        public int GetElementIsotopesInternal(short intElementID, ref short intIsotopeCount, ref double[] dblIsotopeMasses, ref float[] sngIsotopeAbundances)
        {
            if (intElementID >= 1 && intElementID <= ELEMENT_COUNT)
            {
                var stats = ElementStats[intElementID];
                intIsotopeCount = stats.IsotopeCount;
                for (var intIsotopeIndex = 1; intIsotopeIndex <= stats.IsotopeCount; intIsotopeIndex++)
                {
                    dblIsotopeMasses[intIsotopeIndex] = stats.Isotopes[intIsotopeIndex].Mass;
                    sngIsotopeAbundances[intIsotopeIndex] = stats.Isotopes[intIsotopeIndex].Abundance;
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
        /// <param name="intElementID"></param>
        /// <returns></returns>
        /// <remarks>1 is Hydrogen, 2 is Helium, etc.</remarks>
        public string GetElementSymbolInternal(short intElementID)
        {
            if (intElementID >= 1 && intElementID <= ELEMENT_COUNT)
            {
                return ElementStats[intElementID].Symbol;
            }

            return "";
        }

        public List<udtElementStatsType> GetElements()
        {
            return ElementStats.ToList();
        }

        /// <summary>
        /// Returns a single bit of information about a single element
        /// </summary>
        /// <param name="intElementID">Element ID</param>
        /// <param name="eElementStat">Value to obtain: mass, charge, or uncertainty</param>
        /// <returns></returns>
        /// <remarks>Since a value may be negative, simply returns 0 if an error</remarks>
        public double GetElementStatInternal(short intElementID, MolecularWeightTool.ElementStatsType eElementStat)
        {
            if (intElementID >= 1 && intElementID <= ELEMENT_COUNT)
            {
                switch (eElementStat)
                {
                    case MolecularWeightTool.ElementStatsType.Mass:
                        return ElementStats[intElementID].Mass;
                    case MolecularWeightTool.ElementStatsType.Charge:
                        return ElementStats[intElementID].Charge;
                    case MolecularWeightTool.ElementStatsType.Uncertainty:
                        return ElementStats[intElementID].Uncertainty;
                    default:
                        return 0d;
                }
            }

            return 0d;
        }

        public string GetErrorDescription()
        {
            if (ErrorParams.ErrorID != 0)
            {
                return LookupMessage(ErrorParams.ErrorID);
            }

            return "";
        }

        public int GetErrorID()
        {
            return ErrorParams.ErrorID;
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

        public string GetMessageStatementInternal(int messageID)
        {
            return GetMessageStatementInternal(messageID, string.Empty);
        }

        /// <summary>
        /// Get message text using message ID
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="strAppendText"></param>
        /// <returns></returns>
        /// <remarks>
        /// GetMessageStringInternal simply returns the message for messageID
        /// LookupMessage formats the message, and possibly combines multiple messages, depending on the message number
        /// </remarks>
        public string GetMessageStatementInternal(int messageID, string strAppendText)
        {
            if (messageID > 0 && messageID <= MessageStatementCount)
            {
                var strMessage = MessageStatements[messageID];

                // Append Prefix to certain strings
                switch (messageID)
                {
                    // Add Error: to the front of certain error codes
                    case var @case when 1 <= @case && @case <= 99:
                    case 120:
                    case 130:
                    case 140:
                    case 260:
                    case 270:
                    case 300:
                        strMessage = GetMessageStatementInternal(350) + ": " + strMessage;
                        break;
                }

                // Now append strAppendText
                return strMessage + strAppendText;
            }

            return "";
        }

        /// <summary>
        /// Checks for presence of SymbolReference in udtAbbrevSymbolStack
        /// If found, returns true
        /// </summary>
        /// <param name="udtAbbrevSymbolStack"></param>
        /// <param name="SymbolReference"></param>
        /// <returns></returns>
        private bool IsPresentInAbbrevSymbolStack(ref udtAbbrevSymbolStackType udtAbbrevSymbolStack, short SymbolReference)
        {
            try
            {
                var blnFound = false;
                for (var intIndex = 0; intIndex < udtAbbrevSymbolStack.Count; intIndex++)
                {
                    if (udtAbbrevSymbolStack.SymbolReferenceStack[intIndex] == SymbolReference)
                    {
                        blnFound = true;
                        break;
                    }
                }

                return blnFound;
            }
            catch (Exception ex)
            {
                GeneralErrorHandler("IsPresentInAbbrevSymbolStack", ex);
                return false;
            }
        }

        /// <summary>
        /// Returns True if the first letter of strTestChar is a ModSymbol
        /// </summary>
        /// <param name="strTestChar"></param>
        /// <returns></returns>
        /// <remarks>
        /// Invalid Mod Symbols are letters, numbers, ., -, space, (, or )
        /// Valid Mod Symbols are ! # $ % ampersand ' * + ? ^ ` ~
        /// </remarks>
        public bool IsModSymbolInternal(string strTestChar)
        {
            bool blnIsModSymbol;

            if (strTestChar.Length > 0)
            {
                var chFirstChar = strTestChar[0];

                switch (Convert.ToInt32(chFirstChar))
                {
                    case 34: // " is not allowed
                        blnIsModSymbol = false;
                        break;
                    case var @case when 40 <= @case && @case <= 41: // ( and ) are not allowed
                        blnIsModSymbol = false;
                        break;
                    case var case1 when 44 <= case1 && case1 <= 62: // . and - and , and / and numbers and : and ; and < and = and > are not allowed
                        blnIsModSymbol = false;
                        break;
                    case var case2 when 33 <= case2 && case2 <= 43:
                    case var case3 when 63 <= case3 && case3 <= 64:
                    case var case4 when 94 <= case4 && case4 <= 96:
                    case 126:
                        blnIsModSymbol = true;
                        break;
                    default:
                        blnIsModSymbol = false;
                        break;
                }
            }
            else
            {
                blnIsModSymbol = false;
            }

            return blnIsModSymbol;
        }

        /// <summary>
        /// Tests if all of the characters in strTest are letters
        /// </summary>
        /// <param name="strTest"></param>
        /// <returns></returns>
        private bool IsStringAllLetters(string strTest)
        {
            // Assume true until proven otherwise
            var blnAllLetters = true;
            for (var intIndex = 0; intIndex < strTest.Length; intIndex++)
            {
                if (!char.IsLetter(strTest[intIndex]))
                {
                    blnAllLetters = false;
                    break;
                }
            }

            return blnAllLetters;
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

        protected void LogMessage(string strMessage)
        {
            LogMessage(strMessage, MessageType.Normal);
        }

        protected void LogMessage(string strMessage, MessageType eMessageType)
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

                    var blnOpeningExistingFile = System.IO.File.Exists(mLogFilePath);

                    mLogFile = new System.IO.StreamWriter(new System.IO.FileStream(mLogFilePath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read))
                    {
                        AutoFlush = true
                    };

                    if (!blnOpeningExistingFile)
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

            string strMessageType;

            switch (eMessageType)
            {
                case MessageType.Normal:
                    strMessageType = "Normal";
                    break;
                case MessageType.Error:
                    strMessageType = "Error";
                    break;
                case MessageType.Warning:
                    strMessageType = "Warning";
                    break;
                default:
                    strMessageType = "Unknown";
                    break;
            }

            if (mLogFile == null)
            {
                Console.WriteLine(strMessageType + "\t" + strMessage);
            }
            else
            {
                mLogFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt") + "\t" +
                    strMessageType + "\t" + strMessage);
            }
        }

        private string LookupCautionStatement(string strCompareText)
        {
            for (var intIndex = 1; intIndex <= CautionStatementCount; intIndex++)
            {
                if ((strCompareText ?? "") == (CautionStatements[intIndex, 0] ?? ""))
                {
                    return CautionStatements[intIndex, 1];
                }
            }

            return string.Empty;
        }

        internal string LookupMessage(int messageID)
        {
            return LookupMessage(messageID, string.Empty);
        }

        /// <summary>
        /// Looks up the message for messageID
        /// Also appends any data in strAppendText to the message
        /// </summary>
        /// <param name="messageID"></param>
        /// <param name="strAppendText"></param>
        /// <returns>The complete message</returns>
        internal string LookupMessage(int messageID, string strAppendText)
        {
            if (MessageStatementCount == 0)
                MemoryLoadMessageStatements();

            // First assume we can't find the message number
            var strMessage = "General unspecified error";

            // Now try to find it
            if (messageID < MESSAGE_STATEMENT_DIM_COUNT)
            {
                if (MessageStatements[messageID].Length > 0)
                {
                    strMessage = MessageStatements[messageID];
                }
            }

            // Now prepend Prefix to certain strings
            switch (messageID)
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
                        strMessage = LookupMessage(350) + ": " + strMessage;
                        break;
                    }
            }

            // Now append strAppendText
            strMessage += strAppendText;

            // messageID's 1 and 18 may need to have an addendum added
            if (messageID == 1)
            {
                if (gComputationOptions.CaseConversion == CaseConversionMode.ExactCase)
                {
                    strMessage = strMessage + " (" + LookupMessage(680) + ")";
                }
            }
            else if (messageID == 18)
            {
                if (!gComputationOptions.BracketsAsParentheses)
                {
                    strMessage = strMessage + " (" + LookupMessage(685) + ")";
                }
                else
                {
                    strMessage = strMessage + " (" + LookupMessage(690) + ")";
                }
            }

            return strMessage;
        }

        /// <summary>
        /// Converts dblMassToConvert to ppm, based on the value of dblCurrentMZ
        /// </summary>
        /// <param name="dblMassToConvert"></param>
        /// <param name="dblCurrentMZ"></param>
        /// <returns></returns>
        public double MassToPPMInternal(double dblMassToConvert, double dblCurrentMZ)
        {
            if (dblCurrentMZ > 0d)
            {
                return dblMassToConvert * 1000000.0d / dblCurrentMZ;
            }

            return 0d;
        }

        public double MonoMassToMZInternal(double dblMonoisotopicMass, short intCharge)
        {
            return MonoMassToMZInternal(dblMonoisotopicMass, intCharge, 0d);
        }

        /// <summary>
        /// Convert monoisotopic mass to m/z
        /// </summary>
        /// <param name="dblMonoisotopicMass"></param>
        /// <param name="intCharge"></param>
        /// <param name="dblChargeCarrierMass">If this is 0, uses mChargeCarrierMass</param>
        /// <returns></returns>
        public double MonoMassToMZInternal(
            double dblMonoisotopicMass,
            short intCharge,
            double dblChargeCarrierMass)
        {
            if (Math.Abs(dblChargeCarrierMass) < float.Epsilon)
                dblChargeCarrierMass = mChargeCarrierMass;

            // Call ConvoluteMass to convert to the desired charge state
            return ConvoluteMassInternal(dblMonoisotopicMass + dblChargeCarrierMass, 1, intCharge, dblChargeCarrierMass);
        }

        public void MemoryLoadAll(ElementMassMode eElementMode)
        {
            MemoryLoadElements(eElementMode);

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
            const short AminoAbbrevCount = 28;

            AbbrevAllCount = AminoAbbrevCount;
            for (var intIndex = 1; intIndex <= AbbrevAllCount; intIndex++)
                AbbrevStats[intIndex].IsAminoAcid = true;

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

            const short NormalAbbrevCount = 16;
            AbbrevAllCount += NormalAbbrevCount;
            for (var intIndex = AminoAbbrevCount + 1; intIndex <= AbbrevAllCount; intIndex++)
                AbbrevStats[intIndex].IsAminoAcid = false;

            AddAbbreviationWork(AminoAbbrevCount + 1, "Bpy", "C10H8N2", 0f, false, "", "Bipyridine");
            AddAbbreviationWork(AminoAbbrevCount + 2, "Bu", "C4H9", 1f, false, "", "Butyl");
            AddAbbreviationWork(AminoAbbrevCount + 3, "D", "^2.014H", 1f, false, "", "Deuterium");
            AddAbbreviationWork(AminoAbbrevCount + 4, "En", "C2H8N2", 0f, false, "", "Ethylenediamine");
            AddAbbreviationWork(AminoAbbrevCount + 5, "Et", "CH3CH2", 1f, false, "", "Ethyl");
            AddAbbreviationWork(AminoAbbrevCount + 6, "Me", "CH3", 1f, false, "", "Methyl");
            AddAbbreviationWork(AminoAbbrevCount + 7, "Ms", "CH3SOO", -1, false, "", "Mesyl");
            AddAbbreviationWork(AminoAbbrevCount + 8, "Oac", "C2H3O2", -1, false, "", "Acetate");
            AddAbbreviationWork(AminoAbbrevCount + 9, "Otf", "OSO2CF3", -1, false, "", "Triflate");
            AddAbbreviationWork(AminoAbbrevCount + 10, "Ox", "C2O4", -2, false, "", "Oxalate");
            AddAbbreviationWork(AminoAbbrevCount + 11, "Ph", "C6H5", 1f, false, "", "Phenyl");
            AddAbbreviationWork(AminoAbbrevCount + 12, "Phen", "C12H8N2", 0f, false, "", "Phenanthroline");
            AddAbbreviationWork(AminoAbbrevCount + 13, "Py", "C5H5N", 0f, false, "", "Pyridine");
            AddAbbreviationWork(AminoAbbrevCount + 14, "Tpp", "(C4H2N(C6H5C)C4H2N(C6H5C))2", 0f, false, "", "Tetraphenylporphyrin");
            AddAbbreviationWork(AminoAbbrevCount + 15, "Ts", "CH3C6H4SOO", -1, false, "", "Tosyl");
            AddAbbreviationWork(AminoAbbrevCount + 16, "Urea", "H2NCONH2", 0f, false, "", "Urea");

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
        /// <remarks>Use objMwtWin.ClearCautionStatements and objMwtWin.AddCautionStatement to set these based on language</remarks>
        public void MemoryLoadCautionStatements()
        {
            CautionStatementCount = ElementAndMassInMemoryData.MemoryLoadCautionStatementsEnglish(ref CautionStatements);
        }

        public void MemoryLoadElements(ElementMassMode eElementMode)
        {
            MemoryLoadElements(eElementMode, 0, MolecularWeightTool.ElementStatsType.Mass);
        }

        /// <summary>
        /// Load elements
        /// </summary>
        /// <param name="eElementMode">Element mode: 1 for average weights, 2 for monoisotopic weights, 3 for integer weights</param>
        /// <param name="intSpecificElement"></param>
        /// <param name="eSpecificStatToReset"></param>
        /// <remarks>
        /// intSpecificElement and intSpecificElementProperty are zero when updating all of the elements
        /// nonzero intSpecificElement and intSpecificElementProperty values will set just that specific value to the default
        /// </remarks>
        public void MemoryLoadElements(
            ElementMassMode eElementMode,
            short intSpecificElement,
            MolecularWeightTool.ElementStatsType eSpecificStatToReset)
        {
            const double DEFAULT_CHARGE_CARRIER_MASS_AVG = 1.00739d;
            const double DEFAULT_CHARGE_CARRIER_MASS_MONOISO = 1.00727649d;

            // Data Load Statements
            // Uncertainties from CRC Handbook of Chemistry and Physics
            // For Radioactive elements, the most stable isotope is NOT used;
            // instead, an average Mol. Weight is used, just like with other elements.
            // Data obtained from the Perma-Chart Science Series periodic table, 1993.
            // Uncertainties from CRC Handbook of Chemistry and Physics, except for
            // Radioactive elements, where uncertainty was estimated to be .n5 where
            // intSpecificElementProperty represents the number digits after the decimal point but before the last
            // number of the molecular weight.
            // For example, for No, MW = 259.1009 (±0.0005)

            // Define the charge carrier mass
            if (eElementMode == ElementMassMode.Average)
            {
                SetChargeCarrierMassInternal(DEFAULT_CHARGE_CARRIER_MASS_AVG);
            }
            else
            {
                SetChargeCarrierMassInternal(DEFAULT_CHARGE_CARRIER_MASS_MONOISO);
            }

            // strElementNames stores the element names
            // dblElemVals[intElementIndex,1] stores the element's weight
            // dblElemVals[intElementIndex,2] stores the element's uncertainty
            // dblElemVals[intElementIndex,3] stores the element's charge
            // Note: I could make this array of type udtElementStatsType, but the size of this sub would increase dramatically
            ElementAndMassInMemoryData.MemoryLoadElements(eElementMode, out var strElementNames, out var dblElemVals);

            if (intSpecificElement == 0)
            {
                // Updating all the elements
                for (var intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                {
                    var stats = ElementStats[intElementIndex];
                    stats.Symbol = strElementNames[intElementIndex];
                    stats.Mass = dblElemVals[intElementIndex, 1];
                    stats.Uncertainty = dblElemVals[intElementIndex, 2];
                    stats.Charge = (float)dblElemVals[intElementIndex, 3];

                    ElementAlph[intElementIndex] = stats.Symbol;
                }

                // Alphabetize ElementAlph[] array via built-in sort; no custom comparator needed.
                Array.Sort(ElementAlph);

                //// Alphabetize ElementAlph[] array via bubble sort
                //for (var intCompareIndex = ELEMENT_COUNT; intCompareIndex >= 2; intCompareIndex += -1) // Sort from end to start
                //{
                //    for (var intIndex = 1; intIndex < intCompareIndex; intIndex++)
                //    {
                //        if (Operators.CompareString(ElementAlph[intIndex], ElementAlph[intIndex + 1], false) > 0)
                //        {
                //            // Swap them
                //            var strSwap = ElementAlph[intIndex];
                //            ElementAlph[intIndex] = ElementAlph[intIndex + 1];
                //            ElementAlph[intIndex + 1] = strSwap;
                //        }
                //    }
                //}
            }
            else if (intSpecificElement >= 1 && intSpecificElement <= ELEMENT_COUNT)
            {
                var stats = ElementStats[intSpecificElement];
                switch (eSpecificStatToReset)
                {
                    case MolecularWeightTool.ElementStatsType.Mass:
                        stats.Mass = dblElemVals[intSpecificElement, 1];
                        break;
                    case MolecularWeightTool.ElementStatsType.Uncertainty:
                        stats.Uncertainty = dblElemVals[intSpecificElement, 2];
                        break;
                    case MolecularWeightTool.ElementStatsType.Charge:
                        stats.Charge = (float)dblElemVals[intSpecificElement, 3];
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

        private void MwtWinDllErrorHandler(string strSourceForm)
        {
            string strMessage;

            if (Information.Err().Number == 6)
            {
                strMessage = LookupMessage(590);
                if (ShowErrorMessageDialogs)
                {
                    MessageBox.Show(LookupMessage(590), LookupMessage(350), MessageBoxButtons.OK);
                }

                LogMessage(strMessage, MessageType.Error);
            }
            else
            {
                strMessage = LookupMessage(600) + ": " + Information.Err().Description + Environment.NewLine + " (" + strSourceForm + " handler)";
                strMessage += Environment.NewLine + LookupMessage(605);

                if (ShowErrorMessageDialogs)
                {
                    MessageBox.Show(strMessage, LookupMessage(350), MessageBoxButtons.OK);
                }

                // Call GeneralErrorHandler so that the error gets logged to ErrorLog.txt
                // Note that GeneralErrorHandler will call LogMessage

                // Make sure mShowErrorMessageDialogs is false when calling GeneralErrorHandler

                var blnShowErrorMessageDialogsSaved = ShowErrorMessageDialogs;
                ShowErrorMessageDialogs = false;

                GeneralErrorHandler(strSourceForm, Information.Err().Number);

                ShowErrorMessageDialogs = blnShowErrorMessageDialogsSaved;
            }
        }

        private void Initialize()
        {
            ElementAlph = new string[104];
            ElementStats = new udtElementStatsType[104];
            for (var i = 0; i <= ELEMENT_COUNT - 1; i++)
                ElementStats[i].Initialize();

            AbbrevStats = new udtAbbrevStatsType[501];
            CautionStatements = new string[101, 3];
            MessageStatements = new string[1601];

            mProgressStepDescription = string.Empty;
            mProgressPercentComplete = 0f;

            mLogFolderPath = string.Empty;
            mLogFilePath = string.Empty;

            ShowErrorMessageDialogs = false;
        }

        private void InitializeAbbrevSymbolStack(ref udtAbbrevSymbolStackType udtAbbrevSymbolStack)
        {
            udtAbbrevSymbolStack.Count = 0;
            udtAbbrevSymbolStack.SymbolReferenceStack = new short[1];
        }

        private void InitializeComputationStats(ref udtComputationStatsType udtComputationStats)
        {
            udtComputationStats.Initialize();
            udtComputationStats.Charge = 0.0f;
            udtComputationStats.StandardDeviation = 0.0d;
            udtComputationStats.TotalMass = 0.0d;

            for (var intElementIndex = 0; intElementIndex <= ELEMENT_COUNT - 1; intElementIndex++)
            {
                var element = udtComputationStats.Elements[intElementIndex];
                element.Used = false; // whether element is present
                element.Count = 0d; // # of each element
                element.IsotopicCorrection = 0d; // isotopic correction
                element.IsotopeCount = 0; // Count of the number of atoms defined as specific isotopes
                element.Isotopes = new usrIsotopicAtomInfoType[3]; // Default to have room for 2 explicitly defined isotopes
            }
        }

        /// <summary>
        /// Determines the molecular weight and elemental composition of strFormula
        /// </summary>
        /// <param name="strFormula">Input/output: formula to parse</param>
        /// <param name="udtComputationStats">Output: additional information about the formula</param>
        /// <returns>Computed molecular weight if no error; otherwise -1</returns>
        public double ParseFormulaPublic(
            ref string strFormula,
            ref udtComputationStatsType udtComputationStats)
        {
            return ParseFormulaPublic(ref strFormula, ref udtComputationStats, false, 1);
        }

        /// <summary>
        /// Determines the molecular weight and elemental composition of strFormula
        /// </summary>
        /// <param name="strFormula">Input/output: formula to parse</param>
        /// <param name="udtComputationStats">Output: additional information about the formula</param>
        /// <param name="blnExpandAbbreviations"></param>
        /// <returns>Computed molecular weight if no error; otherwise -1</returns>
        public double ParseFormulaPublic(
            ref string strFormula,
            ref udtComputationStatsType udtComputationStats,
            bool blnExpandAbbreviations)
        {
            return ParseFormulaPublic(ref strFormula, ref udtComputationStats, blnExpandAbbreviations, 1);
        }

        /// <summary>
        /// Determines the molecular weight and elemental composition of strFormula
        /// </summary>
        /// <param name="strFormula">Input/output: formula to parse</param>
        /// <param name="udtComputationStats">Output: additional information about the formula</param>
        /// <param name="blnExpandAbbreviations"></param>
        /// <param name="dblValueForX"></param>
        /// <returns>Computed molecular weight if no error; otherwise -1</returns>
        /// <remarks>
        /// ErrorParams will hold information on errors that occur (previous errors are cleared when this function is called)
        /// Use ComputeFormulaWeight if you only want to know the weight of a formula (it calls this function)
        /// </remarks>
        public double ParseFormulaPublic(
            ref string strFormula,
            ref udtComputationStatsType udtComputationStats,
            bool blnExpandAbbreviations,
            double dblValueForX)
        {
            var udtAbbrevSymbolStack = new udtAbbrevSymbolStackType();
            try
            {
                // Initialize the UDTs
                InitializeComputationStats(ref udtComputationStats);
                InitializeAbbrevSymbolStack(ref udtAbbrevSymbolStack);

                var dblStdDevSum = 0.0d;

                // Reset ErrorParams to clear any prior errors
                ResetErrorParamsInternal();

                // Reset Caution Description
                mStrCautionDescription = "";

                if (strFormula.Length > 0)
                {
                    var argCarbonOrSiliconReturnCount = 0;
                    strFormula = ParseFormulaRecursive(strFormula, ref udtComputationStats, ref udtAbbrevSymbolStack, blnExpandAbbreviations, ref dblStdDevSum, ref argCarbonOrSiliconReturnCount, dblValueForX);
                }

                // Copy udtComputationStats to mComputationStatsSaved
                mComputationStatsSaved.Initialize();
                mComputationStatsSaved = udtComputationStats;

                if (ErrorParams.ErrorID == 0)
                {

                    // Compute the standard deviation
                    udtComputationStats.StandardDeviation = Math.Sqrt(dblStdDevSum);

                    // Compute the total molecular weight
                    udtComputationStats.TotalMass = 0d; // Reset total weight of compound to 0 so we can add to it
                    for (var intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                        // Increase total weight by multiplying the count of each element by the element's mass
                        // In addition, add in the Isotopic Correction value
                        udtComputationStats.TotalMass = udtComputationStats.TotalMass + ElementStats[intElementIndex].Mass * udtComputationStats.Elements[intElementIndex].Count + udtComputationStats.Elements[intElementIndex].IsotopicCorrection;

                    return udtComputationStats.TotalMass;
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
        /// Stores results in udtComputationStats
        /// ErrorParams will hold information on errors that occur
        /// </summary>
        /// <param name="strFormula"></param>
        /// <param name="udtComputationStats"></param>
        /// <param name="udtAbbrevSymbolStack"></param>
        /// <param name="blnExpandAbbreviations"></param>
        /// <param name="dblStdDevSum">Sum of the squares of the standard deviations</param>
        /// <param name="CarbonOrSiliconReturnCount">Tracks the number of carbon and silicon atoms found; used when correcting for charge inside parentheses or inside an abbreviation</param>
        /// <param name="dblValueForX"></param>
        /// <param name="intCharCountPrior"></param>
        /// <param name="dblParenthMultiplier">The value to multiply all values by if inside parentheses</param>
        /// <param name="dblDashMultiplierPrior"></param>
        /// <param name="dblBracketMultiplierPrior"></param>
        /// <param name="intParenthLevelPrevious"></param>
        /// <returns>Formatted formula</returns>
        private string ParseFormulaRecursive(
            string strFormula,
            ref udtComputationStatsType udtComputationStats,
            ref udtAbbrevSymbolStackType udtAbbrevSymbolStack,
            bool blnExpandAbbreviations,
            ref double dblStdDevSum,
            ref int CarbonOrSiliconReturnCount,
            double dblValueForX = 1.0d,
            int intCharCountPrior = 0,
            double dblParenthMultiplier = 1.0d,
            double dblDashMultiplierPrior = 1.0d,
            double dblBracketMultiplierPrior = 1.0d,
            short intParenthLevelPrevious = 0)
        {
            // ( and ) are 40 and 41   - is 45   { and } are 123 and 125
            // Numbers are 48 to 57    . is 46
            // Uppercase letters are 65 to 90
            // Lowercase letters are 97 to 122
            // [ and ] are 91 and 93
            // ^ is 94
            // is 95

            int intSymbolLength = default;
            var blnCaretPresent = default(bool);

            var udtComputationStatsRightHalf = new udtComputationStatsType();
            udtComputationStatsRightHalf.Initialize();

            var udtAbbrevSymbolStackRightHalf = new udtAbbrevSymbolStackType();

            var dblStdDevSumRightHalf = default(double);
            double dblCaretVal = default;
            var strChar1 = string.Empty;

            short SymbolReference = default, PrevSymbolReference = default;
            int intParenthLevel = default;

            try
            {
                int intCharIndex;
                var dblDashMultiplier = dblDashMultiplierPrior;
                var dblBracketMultiplier = dblBracketMultiplierPrior;
                var blnInsideBrackets = false;

                var intDashPos = 0;
                var strNewFormula = string.Empty;
                var strNewFormulaRightHalf = string.Empty;

                var LoneCarbonOrSilicon = 0;
                CarbonOrSiliconReturnCount = 0;

                // Look for the > symbol
                // If found, this means take First Part minus the Second Part
                var intMinusSymbolLoc = strFormula.IndexOf(">", StringComparison.Ordinal);
                if (intMinusSymbolLoc >= 0)
                {
                    // Look for the first occurrence of >
                    intCharIndex = 0;
                    var blnMatchFound = false;
                    do
                    {
                        if (strFormula.Substring(intCharIndex, 1) == ">")
                        {
                            blnMatchFound = true;
                            var strLeftHalf = strFormula.Substring(0, Math.Min(strFormula.Length, intCharIndex - 1));
                            var strRightHalf = strFormula.Substring(intCharIndex + 1);

                            // Parse the first half
                            strNewFormula = ParseFormulaRecursive(strLeftHalf, ref udtComputationStats, ref udtAbbrevSymbolStack, blnExpandAbbreviations, ref dblStdDevSum, ref CarbonOrSiliconReturnCount, dblValueForX, intCharCountPrior, dblParenthMultiplier, dblDashMultiplier, dblBracketMultiplier, intParenthLevelPrevious);

                            // Parse the second half
                            InitializeComputationStats(ref udtComputationStatsRightHalf);
                            InitializeAbbrevSymbolStack(ref udtAbbrevSymbolStackRightHalf);

                            strNewFormulaRightHalf = ParseFormulaRecursive(strRightHalf, ref udtComputationStatsRightHalf, ref udtAbbrevSymbolStackRightHalf, blnExpandAbbreviations, ref dblStdDevSumRightHalf, ref CarbonOrSiliconReturnCount, dblValueForX, intCharCountPrior + intCharIndex, dblParenthMultiplier, dblDashMultiplier, dblBracketMultiplier, intParenthLevelPrevious);
                            break;
                        }

                        intCharIndex++;
                    }
                    while (intCharIndex < strFormula.Length);

                    if (blnMatchFound)
                    {
                        // Update strFormula
                        strFormula = strNewFormula + ">" + strNewFormulaRightHalf;

                        // Update udtComputationStats by subtracting the atom counts of the first half minus the second half
                        // If any atom counts become < 0 then, then raise an error
                        for (var intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                        {
                            var element = udtComputationStats.Elements[intElementIndex];
                            if (ElementStats[intElementIndex].Mass * element.Count + element.IsotopicCorrection >= ElementStats[intElementIndex].Mass * udtComputationStatsRightHalf.Elements[intElementIndex].Count + udtComputationStatsRightHalf.Elements[intElementIndex].IsotopicCorrection)
                            {
                                element.Count -= -udtComputationStatsRightHalf.Elements[intElementIndex].Count;
                                if (element.Count < 0d)
                                {
                                    // This shouldn't happen
                                    Console.WriteLine(".Count is less than 0 in ParseFormulaRecursive; this shouldn't happen");
                                    element.Count = 0d;
                                }

                                if (Math.Abs(udtComputationStatsRightHalf.Elements[intElementIndex].IsotopicCorrection) > float.Epsilon)
                                {
                                    // This assertion is here simply because I want to check the code
                                    element.IsotopicCorrection -= udtComputationStatsRightHalf.Elements[intElementIndex].IsotopicCorrection;
                                }
                            }
                            else
                            {
                                // Invalid Formula; raise error
                                ErrorParams.ErrorID = 30;
                                ErrorParams.ErrorPosition = intCharIndex;
                            }

                            if (ErrorParams.ErrorID != 0)
                                break;
                        }

                        // Adjust the overall charge
                        udtComputationStats.Charge -= udtComputationStatsRightHalf.Charge;
                    }
                }
                else
                {
                    // Formula does not contain >
                    // Parse it
                    intCharIndex = 0;
                    do
                    {
                        strChar1 = strFormula.Substring(intCharIndex, 1);
                        var strChar2 = strFormula.Substring(intCharIndex + 1, 1);
                        var strChar3 = strFormula.Substring(intCharIndex + 2, 1);
                        var strCharRemain = strFormula.Substring(intCharIndex + 3);
                        if (gComputationOptions.CaseConversion != CaseConversionMode.ExactCase)
                            strChar1 = strChar1.ToUpper();

                        if (gComputationOptions.BracketsAsParentheses)
                        {
                            if (strChar1 == "[")
                                strChar1 = "(";
                            if (strChar1 == "]")
                                strChar1 = ")";
                        }

                        if (string.IsNullOrEmpty(strChar1))
                            strChar1 = EMPTY_STRING_CHAR.ToString();
                        if (string.IsNullOrEmpty(strChar2))
                            strChar2 = EMPTY_STRING_CHAR.ToString();
                        if (string.IsNullOrEmpty(strChar3))
                            strChar3 = EMPTY_STRING_CHAR.ToString();
                        if (string.IsNullOrEmpty(strCharRemain))
                            strCharRemain = EMPTY_STRING_CHAR.ToString();

                        var strFormulaExcerpt = strChar1 + strChar2 + strChar3 + strCharRemain;

                        // Check for needed caution statements
                        CheckCaution(strFormulaExcerpt);

                        int intNumLength;
                        double dblAdjacentNum;
                        int intAddonCount;
                        switch ((int)strChar1[0])
                        {
                            case 40:
                            case 123: // (    Record its position
                                // See if a number is present just after the opening parenthesis
                                if (char.IsDigit(strChar2[0]) || strChar2 == ".")
                                {
                                    // Misplaced number
                                    ErrorParams.ErrorID = 14;
                                    ErrorParams.ErrorPosition = intCharIndex;
                                }

                                if (ErrorParams.ErrorID == 0)
                                {
                                    // search for closing parenthesis
                                    intParenthLevel = 1;
                                    for (var intParenthClose = intCharIndex + 1; intParenthClose < strFormula.Length; intParenthClose++)
                                    {
                                        switch (strFormula.Substring(intParenthClose, 1) ?? "")
                                        {
                                            case "(":
                                            case "{":
                                            case "[":
                                                // Another opening parentheses
                                                // increment parenthLevel
                                                if (!gComputationOptions.BracketsAsParentheses && strFormula.Substring(intParenthClose, 1) == "[")
                                                {
                                                    // Do not count the bracket
                                                }
                                                else
                                                {
                                                    intParenthLevel += 1;
                                                }

                                                break;

                                            case ")":
                                            case "}":
                                            case "]":
                                                if (!gComputationOptions.BracketsAsParentheses && strFormula.Substring(intParenthClose, 1) == "]")
                                                {
                                                    // Do not count the bracket
                                                }
                                                else
                                                {
                                                    intParenthLevel -= 1;
                                                    if (intParenthLevel == 0)
                                                    {
                                                        dblAdjacentNum = ParseNum(strFormula.Substring(intParenthClose + 1), out intNumLength);
                                                        CatchParseNumError(dblAdjacentNum, intNumLength, intCharIndex, intSymbolLength);

                                                        if (dblAdjacentNum < 0d)
                                                        {
                                                            dblAdjacentNum = 1.0d;
                                                            intAddonCount = 0;
                                                        }
                                                        else
                                                        {
                                                            intAddonCount = intNumLength;
                                                        }

                                                        var strSubFormula = strFormula.Substring(intCharIndex + 1, intParenthClose - (intCharIndex + 1));

                                                        // Note, must pass dblParenthMultiplier * dblAdjacentNum to preserve previous parentheses stuff
                                                        strNewFormula = ParseFormulaRecursive(strSubFormula, ref udtComputationStats, ref udtAbbrevSymbolStack, blnExpandAbbreviations, ref dblStdDevSum, ref CarbonOrSiliconReturnCount, dblValueForX, intCharCountPrior + intCharIndex, dblParenthMultiplier * dblAdjacentNum, dblDashMultiplier, dblBracketMultiplier, (short)(intParenthLevelPrevious + 1));

                                                        // If expanding abbreviations, then strNewFormula might be longer than strFormula, must add this onto intCharIndex also
                                                        var intExpandAbbrevAdd = strNewFormula.Length - strSubFormula.Length;

                                                        // Must replace the part of the formula parsed with the strNewFormula part, in case the formula was expanded or elements were capitalized
                                                        strFormula = strFormula.Substring(0, intCharIndex) + strNewFormula + strFormula.Substring(intParenthClose);
                                                        intCharIndex = intParenthClose + intAddonCount + intExpandAbbrevAdd;

                                                        // Correct charge
                                                        if (CarbonOrSiliconReturnCount > 0)
                                                        {
                                                            udtComputationStats.Charge = (float)(udtComputationStats.Charge - 2d * dblAdjacentNum);
                                                            if (dblAdjacentNum > 1d && CarbonOrSiliconReturnCount > 1)
                                                            {
                                                                udtComputationStats.Charge = (float)(udtComputationStats.Charge - 2d * (dblAdjacentNum - 1d) * (CarbonOrSiliconReturnCount - 1));
                                                            }
                                                        }

                                                        break;
                                                    }
                                                }

                                                break;
                                        }
                                    }
                                }

                                if (intParenthLevel > 0 && ErrorParams.ErrorID == 0)
                                {
                                    // Missing closing parenthesis
                                    ErrorParams.ErrorID = 3;
                                    ErrorParams.ErrorPosition = intCharIndex;
                                }

                                PrevSymbolReference = 0;
                                break;

                            case 41:
                            case 125: // )    Repeat a section of a formula
                                // Should have been skipped
                                // Unmatched closing parentheses
                                ErrorParams.ErrorID = 4;
                                ErrorParams.ErrorPosition = intCharIndex;
                                break;

                            case 45: // -
                                // Used to denote a leading coefficient
                                dblAdjacentNum = ParseNum(strChar2 + strChar3 + strCharRemain, out intNumLength);
                                CatchParseNumError(dblAdjacentNum, intNumLength, intCharIndex, intSymbolLength);

                                if (dblAdjacentNum > 0d)
                                {
                                    intDashPos = intCharIndex + intNumLength;
                                    dblDashMultiplier = dblAdjacentNum * dblDashMultiplierPrior;
                                    intCharIndex += intNumLength;
                                }
                                else if (Math.Abs(dblAdjacentNum) < float.Epsilon)
                                {
                                    // Cannot have 0 after a dash
                                    ErrorParams.ErrorID = 5;
                                    ErrorParams.ErrorPosition = intCharIndex + 1;
                                }
                                else
                                {
                                    // No number is present, that's just fine
                                    // Make sure defaults are set, though
                                    intDashPos = 0;
                                    dblDashMultiplier = dblDashMultiplierPrior;
                                }

                                PrevSymbolReference = 0;
                                break;

                            case 44:
                            case 46:
                            case var @case when 48 <= @case && @case <= 57: // . or , and Numbers 0 to 9
                                // They should only be encountered as a leading coefficient
                                // Should have been bypassed when the coefficient was processed
                                if (intCharIndex == 0)
                                {
                                    // Formula starts with a number -- multiply section by number (until next dash)
                                    dblAdjacentNum = ParseNum(strFormulaExcerpt, out intNumLength);
                                    CatchParseNumError(dblAdjacentNum, intNumLength, intCharIndex, intSymbolLength);

                                    if (dblAdjacentNum >= 0d)
                                    {
                                        intDashPos = intCharIndex + intNumLength - 1;
                                        dblDashMultiplier = dblAdjacentNum * dblDashMultiplierPrior;
                                        intCharIndex = intCharIndex + intNumLength - 1;
                                    }
                                    else
                                    {
                                        // A number less then zero should have been handled by CatchParseNumError above
                                        // Make sure defaults are set, though
                                        intDashPos = 0;
                                        dblDashMultiplier = dblDashMultiplierPrior;
                                    }
                                }
                                else if (NumberConverter.CDblSafe(strFormula.Substring(intCharIndex - 1, 1)) > 0d)
                                {
                                    // Number too large
                                    ErrorParams.ErrorID = 7;
                                    ErrorParams.ErrorPosition = intCharIndex;
                                }
                                else
                                {
                                    // Misplaced number
                                    ErrorParams.ErrorID = 14;
                                    ErrorParams.ErrorPosition = intCharIndex;
                                }

                                PrevSymbolReference = 0;
                                break;

                            case 91: // [
                                if (strChar2.ToUpper() == "X")
                                {
                                    if (strChar3 == "e")
                                    {
                                        dblAdjacentNum = ParseNum(strChar2 + strChar3 + strCharRemain, out intNumLength);
                                        CatchParseNumError(dblAdjacentNum, intNumLength, intCharIndex, intSymbolLength);
                                    }
                                    else
                                    {
                                        dblAdjacentNum = dblValueForX;
                                        intNumLength = 1;
                                    }
                                }
                                else
                                {
                                    dblAdjacentNum = ParseNum(strChar2 + strChar3 + strCharRemain, out intNumLength);
                                    CatchParseNumError(dblAdjacentNum, intNumLength, intCharIndex, intSymbolLength);
                                }

                                if (ErrorParams.ErrorID == 0)
                                {
                                    if (blnInsideBrackets)
                                    {
                                        // No Nested brackets.
                                        ErrorParams.ErrorID = 16;
                                        ErrorParams.ErrorPosition = intCharIndex;
                                    }
                                    else if (dblAdjacentNum < 0d)
                                    {
                                        // No number after bracket
                                        ErrorParams.ErrorID = 12;
                                        ErrorParams.ErrorPosition = intCharIndex + 1;
                                    }
                                    else
                                    {
                                        // Coefficient for bracketed section.
                                        blnInsideBrackets = true;
                                        dblBracketMultiplier = dblAdjacentNum * dblBracketMultiplierPrior; // Take times dblBracketMultiplierPrior in case it wasn't 1 to start with
                                        intCharIndex += intNumLength;
                                    }
                                }

                                PrevSymbolReference = 0;
                                break;

                            case 93: // ]
                                dblAdjacentNum = ParseNum(strChar2 + strChar3 + strCharRemain, out intNumLength);
                                CatchParseNumError(dblAdjacentNum, intNumLength, intCharIndex, intSymbolLength);

                                if (dblAdjacentNum >= 0d)
                                {
                                    // Number following bracket
                                    ErrorParams.ErrorID = 11;
                                    ErrorParams.ErrorPosition = intCharIndex + 1;
                                }
                                else if (blnInsideBrackets)
                                {
                                    if (intDashPos > 0)
                                    {
                                        // Need to set intDashPos and dblDashMultiplier back to defaults, since a dash number goes back to one inside brackets
                                        intDashPos = 0;
                                        dblDashMultiplier = 1d;
                                    }

                                    blnInsideBrackets = false;
                                    dblBracketMultiplier = dblBracketMultiplierPrior;
                                }
                                else
                                {
                                    // Unmatched bracket
                                    ErrorParams.ErrorID = 15;
                                    ErrorParams.ErrorPosition = intCharIndex;
                                }

                                break;

                            case var case1 when 65 <= case1 && case1 <= 90:
                            case var case2 when 97 <= case2 && case2 <= 122:
                            case 43:
                            case 95: // Uppercase A to Z and lowercase a to z, and the plus (+) sign, and the underscore (_)
                                intAddonCount = 0;
                                dblAdjacentNum = 0d;

                                var eSymbolMatchType = CheckElemAndAbbrev(strFormulaExcerpt, ref SymbolReference);

                                switch (eSymbolMatchType)
                                {
                                    case SymbolMatchMode.Element:
                                        // Found an element
                                        // SymbolReference is the elemental number
                                        intSymbolLength = ElementStats[SymbolReference].Symbol.Length;
                                        if (intSymbolLength == 0)
                                        {
                                            // No elements in ElementStats yet
                                            // Set intSymbolLength to 1
                                            intSymbolLength = 1;
                                        }
                                        // Look for number after element
                                        dblAdjacentNum = ParseNum(strFormula.Substring(intCharIndex + intSymbolLength), out intNumLength);
                                        CatchParseNumError(dblAdjacentNum, intNumLength, intCharIndex, intSymbolLength);

                                        if (dblAdjacentNum < 0d)
                                        {
                                            dblAdjacentNum = 1d;
                                        }

                                        // Note that intNumLength = 0 if dblAdjacentNum was -1 or otherwise < 0
                                        intAddonCount = intNumLength + intSymbolLength - 1;

                                        if (Math.Abs(dblAdjacentNum) < float.Epsilon)
                                        {
                                            // Zero after element
                                            ErrorParams.ErrorID = 5;
                                            ErrorParams.ErrorPosition = intCharIndex + intSymbolLength;
                                        }
                                        else
                                        {
                                            double dblAtomCountToAdd;
                                            if (!blnCaretPresent)
                                            {
                                                dblAtomCountToAdd = dblAdjacentNum * dblBracketMultiplier * dblParenthMultiplier * dblDashMultiplier;
                                                var element = udtComputationStats.Elements[SymbolReference];
                                                element.Count += dblAtomCountToAdd;
                                                element.Used = true; // Element is present tag
                                                dblStdDevSum += dblAtomCountToAdd * Math.Pow(ElementStats[SymbolReference].Uncertainty, 2d);

                                                var compStats = udtComputationStats;
                                                // Compute charge
                                                if (SymbolReference == 1)
                                                {
                                                    // Dealing with hydrogen
                                                    switch (PrevSymbolReference)
                                                    {
                                                        case 1:
                                                        case var case3 when 3 <= case3 && case3 <= 6:
                                                        case var case4 when 11 <= case4 && case4 <= 14:
                                                        case var case5 when 19 <= case5 && case5 <= 32:
                                                        case var case6 when 37 <= case6 && case6 <= 50:
                                                        case var case7 when 55 <= case7 && case7 <= 82:
                                                        case var case8 when 87 <= case8 && case8 <= 109:
                                                            // Hydrogen is -1 with metals (non-halides)
                                                            compStats.Charge = (float)(compStats.Charge + dblAtomCountToAdd * -1);
                                                            break;
                                                        default:
                                                            compStats.Charge = (float)(compStats.Charge + dblAtomCountToAdd * ElementStats[SymbolReference].Charge);
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    compStats.Charge = (float)(compStats.Charge + dblAtomCountToAdd * ElementStats[SymbolReference].Charge);
                                                }

                                                if (SymbolReference == 6 || SymbolReference == 14)
                                                {
                                                    // Sum up number lone C and Si (not in abbreviations)
                                                    LoneCarbonOrSilicon = (int)Math.Round(LoneCarbonOrSilicon + dblAdjacentNum);
                                                    CarbonOrSiliconReturnCount = (int)Math.Round(CarbonOrSiliconReturnCount + dblAdjacentNum);
                                                }
                                            }
                                            else
                                            {
                                                // blnCaretPresent = True
                                                // Check to make sure isotopic mass is reasonable
                                                double dblIsoDifferenceTop = NumberConverter.CIntSafe(0.63d * SymbolReference + 6d);
                                                double dblIsoDifferenceBottom = NumberConverter.CIntSafe(0.008d * Math.Pow(SymbolReference, 2d) - 0.4d * SymbolReference - 6d);
                                                var dblCaretValDifference = dblCaretVal - SymbolReference * 2;

                                                if (dblCaretValDifference >= dblIsoDifferenceTop)
                                                {
                                                    // Probably too high isotopic mass
                                                    AddToCautionDescription(LookupMessage(660) + ": " + ElementStats[SymbolReference].Symbol + " - " + dblCaretVal + " " + LookupMessage(665) + " " + ElementStats[SymbolReference].Mass);
                                                }
                                                else if (dblCaretVal < SymbolReference)
                                                {
                                                    // Definitely too low isotopic mass
                                                    AddToCautionDescription(LookupMessage(670) + ": " + ElementStats[SymbolReference].Symbol + " - " + SymbolReference + " " + LookupMessage(675));
                                                }
                                                else if (dblCaretValDifference <= dblIsoDifferenceBottom)
                                                {
                                                    // Probably too low isotopic mass
                                                    AddToCautionDescription(LookupMessage(662) + ": " + ElementStats[SymbolReference].Symbol + " - " + dblCaretVal + " " + LookupMessage(665) + " " + ElementStats[SymbolReference].Mass);
                                                }

                                                // Put in isotopic correction factor
                                                dblAtomCountToAdd = dblAdjacentNum * dblBracketMultiplier * dblParenthMultiplier * dblDashMultiplier;
                                                var element = udtComputationStats.Elements[SymbolReference];
                                                // Increment element counting bin
                                                element.Count += dblAtomCountToAdd;

                                                // Store information in .Isotopes()
                                                // Increment the isotope counting bin
                                                element.IsotopeCount = (short)(element.IsotopeCount + 1);

                                                if (element.Isotopes.Length <= element.IsotopeCount)
                                                {
                                                    Array.Resize(ref element.Isotopes, element.Isotopes.Length + 2);
                                                }

                                                var isotope = element.Isotopes[element.IsotopeCount];
                                                isotope.Count += dblAtomCountToAdd;
                                                isotope.Mass = dblCaretVal;

                                                // Add correction amount to udtComputationStats.elements(SymbolReference).IsotopicCorrection
                                                element.IsotopicCorrection += (dblCaretVal * dblAtomCountToAdd - ElementStats[SymbolReference].Mass * dblAtomCountToAdd);

                                                // Set bit that element is present
                                                element.Used = true;

                                                // Assume no error in caret value, no need to change dblStdDevSum

                                                // Reset blnCaretPresent
                                                blnCaretPresent = false;
                                            }

                                            if (gComputationOptions.CaseConversion == CaseConversionMode.ConvertCaseUp)
                                            {
                                                strFormula = strFormula.Substring(0, intCharIndex - 1) + strFormula.Substring(intCharIndex, 1).ToUpper() + strFormula.Substring(intCharIndex + 1);
                                            }

                                            intCharIndex += intAddonCount;
                                        }

                                        break;

                                    case SymbolMatchMode.Abbreviation:
                                        // Found an abbreviation or amino acid
                                        // SymbolReference is the abbrev or amino acid number

                                        if (IsPresentInAbbrevSymbolStack(ref udtAbbrevSymbolStack, SymbolReference))
                                        {
                                            // Circular Reference: Can't have an abbreviation referencing an abbreviation that depends upon it
                                            // For example, the following is impossible:  Lor = C6H5Tal and Tal = H4O2Lor
                                            // Furthermore, can't have this either: Lor = C6H5Tal and Tal = H4O2Vin and Vin = S3Lor
                                            ErrorParams.ErrorID = 28;
                                            ErrorParams.ErrorPosition = intCharIndex;
                                        }
                                        // Found an abbreviation
                                        else if (blnCaretPresent)
                                        {
                                            // Cannot have isotopic mass for an abbreviation, including deuterium
                                            if (strChar1.ToUpper() == "D" && strChar2 != "y")
                                            {
                                                // Isotopic mass used for Deuterium
                                                ErrorParams.ErrorID = 26;
                                                ErrorParams.ErrorPosition = intCharIndex;
                                            }
                                            else
                                            {
                                                ErrorParams.ErrorID = 24;
                                                ErrorParams.ErrorPosition = intCharIndex;
                                            }
                                        }
                                        else
                                        {
                                            // Parse abbreviation
                                            // Simply treat it like a formula surrounded by parentheses
                                            // Thus, find the number after the abbreviation, then call ParseFormulaRecursive, sending it the formula for the abbreviation
                                            // Update the udtAbbrevSymbolStack before calling so that we can check for circular abbreviation references

                                            // Record the abbreviation length
                                            intSymbolLength = AbbrevStats[SymbolReference].Symbol.Length;

                                            // Look for number after abbrev/amino
                                            dblAdjacentNum = ParseNum(strFormula.Substring(intCharIndex + intSymbolLength), out intNumLength);
                                            CatchParseNumError(dblAdjacentNum, intNumLength, intCharIndex, intSymbolLength);

                                            if (dblAdjacentNum < 0d)
                                            {
                                                dblAdjacentNum = 1d;
                                                intAddonCount = intSymbolLength - 1;
                                            }
                                            else
                                            {
                                                intAddonCount = intNumLength + intSymbolLength - 1;
                                            }

                                            // Add this abbreviation symbol to the Abbreviation Symbol Stack
                                            AbbrevSymbolStackAdd(ref udtAbbrevSymbolStack, SymbolReference);

                                            // Compute the charge prior to calling ParseFormulaRecursive
                                            // During the call to ParseFormulaRecursive, udtComputationStats.Charge will be
                                            // modified according to the atoms in the abbreviation's formula
                                            // This is not what we want; instead, we want to use the defined charge for the abbreviation
                                            // We'll use the dblAtomCountToAdd variable here, though instead of an atom count, it's really an abbreviation occurrence count
                                            var dblAtomCountToAdd = dblAdjacentNum * dblBracketMultiplier * dblParenthMultiplier * dblDashMultiplier;
                                            var sngChargeSaved = (float)(udtComputationStats.Charge + dblAtomCountToAdd * AbbrevStats[SymbolReference].Charge);

                                            // When parsing an abbreviation, do not pass on the value of blnExpandAbbreviations
                                            // This way, an abbreviation containing an abbreviation will only get expanded one level
                                            ParseFormulaRecursive(AbbrevStats[SymbolReference].Formula, ref udtComputationStats, ref udtAbbrevSymbolStack, false, ref dblStdDevSum, ref CarbonOrSiliconReturnCount, dblValueForX, intCharCountPrior + intCharIndex, dblParenthMultiplier * dblAdjacentNum, dblDashMultiplier, dblBracketMultiplier, intParenthLevelPrevious);

                                            // Update the charge to sngChargeSaved
                                            udtComputationStats.Charge = sngChargeSaved;

                                            // Remove this symbol from the Abbreviation Symbol Stack
                                            AbbrevSymbolStackAddRemoveMostRecent(ref udtAbbrevSymbolStack);

                                            if (ErrorParams.ErrorID == 0)
                                            {
                                                if (blnExpandAbbreviations)
                                                {
                                                    // Replace abbreviation with empirical formula
                                                    var strReplace = AbbrevStats[SymbolReference].Formula;

                                                    // Look for a number after the abbreviation or amino acid
                                                    dblAdjacentNum = ParseNum(strFormula.Substring(intCharIndex + intSymbolLength), out intNumLength);
                                                    CatchParseNumError(dblAdjacentNum, intNumLength, intCharIndex, intSymbolLength);

                                                    if (strReplace.IndexOf(">", StringComparison.Ordinal) >= 0)
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
                                                        if (intParenthLevelPrevious > 0 || intParenthLevel > 0 || intCharIndex + intSymbolLength <= strFormula.Length)
                                                        {
                                                            strReplace = ConvertFormulaToEmpirical(strReplace);
                                                        }
                                                    }

                                                    if (dblAdjacentNum < 0d)
                                                    {
                                                        // No number after abbreviation
                                                        strFormula = strFormula.Substring(0, intCharIndex - 1) + strReplace + strFormula.Substring(intCharIndex + intSymbolLength);
                                                        intSymbolLength = strReplace.Length;
                                                        dblAdjacentNum = 1d;
                                                        intAddonCount = intSymbolLength - 1;
                                                    }
                                                    else
                                                    {
                                                        // Number after abbreviation -- must put abbreviation in parentheses
                                                        // Parentheses can handle integer or decimal number
                                                        strReplace = "(" + strReplace + ")";
                                                        strFormula = strFormula.Substring(0, intCharIndex - 1) + strReplace + strFormula.Substring(intCharIndex + intSymbolLength);
                                                        intSymbolLength = strReplace.Length;
                                                        intAddonCount = intNumLength + intSymbolLength - 1;
                                                    }
                                                }

                                                if (gComputationOptions.CaseConversion == CaseConversionMode.ConvertCaseUp)
                                                {
                                                    strFormula = strFormula.Substring(0, intCharIndex - 1) + strFormula.Substring(intCharIndex, 1).ToUpper() + strFormula.Substring(intCharIndex + 1);
                                                }
                                            }
                                        }

                                        intCharIndex += intAddonCount;
                                        break;

                                    default:
                                        // Element not Found
                                        if (strChar1.ToUpper() == "X")
                                        {
                                            // X for solver but no preceding bracket
                                            ErrorParams.ErrorID = 18;
                                        }
                                        else
                                        {
                                            ErrorParams.ErrorID = 1;
                                        }

                                        ErrorParams.ErrorPosition = intCharIndex;
                                        break;
                                }

                                PrevSymbolReference = SymbolReference;
                                break;

                            case 94: // ^ (caret)
                                dblAdjacentNum = ParseNum(strChar2 + strChar3 + strCharRemain, out intNumLength);
                                CatchParseNumError(dblAdjacentNum, intNumLength, intCharIndex, intSymbolLength);

                                if (ErrorParams.ErrorID != 0)
                                {
                                    // Problem, don't go on.
                                }
                                else
                                {
                                    int intCharAsc;
                                    var strCharVal = strFormula.Substring(intCharIndex + 1 + intNumLength, 1);
                                    if (strCharVal.Length > 0)
                                        intCharAsc = strCharVal[0];
                                    else
                                        intCharAsc = 0;
                                    if (dblAdjacentNum >= 0d)
                                    {
                                        if (intCharAsc >= 65 && intCharAsc <= 90 || intCharAsc >= 97 && intCharAsc <= 122) // Uppercase A to Z and lowercase a to z
                                        {
                                            blnCaretPresent = true;
                                            dblCaretVal = dblAdjacentNum;
                                            intCharIndex += intNumLength;
                                        }
                                        else
                                        {
                                            // No letter after isotopic mass
                                            ErrorParams.ErrorID = 22;
                                            ErrorParams.ErrorPosition = intCharIndex + intNumLength + 1;
                                        }
                                    }
                                    else
                                    {
                                        // Adjacent number is < 0 or not present
                                        // Record error
                                        blnCaretPresent = false;
                                        if (strFormula.Substring(intCharIndex + 1, 1) == "-")
                                        {
                                            // Negative number following caret
                                            ErrorParams.ErrorID = 23;
                                            ErrorParams.ErrorPosition = intCharIndex + 1;
                                        }
                                        else
                                        {
                                            // No number following caret
                                            ErrorParams.ErrorID = 20;
                                            ErrorParams.ErrorPosition = intCharIndex + 1;
                                        }
                                    }
                                }

                                break;

                            default:
                                // There shouldn't be anything else (except the ~ filler character). If there is, we'll just ignore it
                                break;
                        }

                        if (intCharIndex == strFormula.Length - 1)
                        {
                            // Need to make sure compounds are present after a leading coefficient after a dash
                            if (dblDashMultiplier > 0d)
                            {
                                if (intCharIndex != intDashPos)
                                {
                                    // Things went fine, no need to set anything
                                }
                                else
                                {
                                    // No compounds after leading coefficient after dash
                                    ErrorParams.ErrorID = 25;
                                    ErrorParams.ErrorPosition = intCharIndex;
                                }
                            }
                        }

                        if (ErrorParams.ErrorID != 0)
                        {
                            intCharIndex = strFormula.Length;
                        }

                        intCharIndex ++;
                    }
                    while (intCharIndex < strFormula.Length);
                }

                if (blnInsideBrackets)
                {
                    if (ErrorParams.ErrorID == 0)
                    {
                        // Missing closing bracket
                        ErrorParams.ErrorID = 13;
                        ErrorParams.ErrorPosition = intCharIndex;
                    }
                }

                if (ErrorParams.ErrorID != 0 && ErrorParams.ErrorCharacter.Length == 0)
                {
                    if (string.IsNullOrEmpty(strChar1))
                        strChar1 = EMPTY_STRING_CHAR.ToString();
                    ErrorParams.ErrorCharacter = strChar1;
                    ErrorParams.ErrorPosition += intCharCountPrior;
                }

                if (LoneCarbonOrSilicon > 1)
                {
                    // Correct Charge for number of C and Si
                    udtComputationStats.Charge -= (LoneCarbonOrSilicon - 1) * 2;

                    CarbonOrSiliconReturnCount = LoneCarbonOrSilicon;
                }
                else
                {
                    CarbonOrSiliconReturnCount = 0;
                }

                // Return strFormula, which is possibly now capitalized correctly
                // It will also contain expanded abbreviations
                return strFormula;
            }
            catch (Exception ex)
            {
                MwtWinDllErrorHandler("MwtWinDll_clsElementAndMassRoutines|ParseFormula: " + ex.Message);
                ErrorParams.ErrorID = -10;
                ErrorParams.ErrorPosition = 0;

                return strFormula;
            }
        }

        /// <summary>
        /// Looks for a number and returns it if found
        /// </summary>
        /// <param name="strWork">Input</param>
        /// <param name="intNumLength">Output: length of the number</param>
        /// <param name="blnAllowNegative"></param>
        /// <returns>
        /// Parsed number if found
        /// If not a number, returns a negative number for the error code and sets intNumLength = 0
        /// </returns>
        /// <remarks>
        /// Error codes:
        /// -1 = No number
        /// -2 =                                             (unused)
        /// -3 = No number at all or (more likely) no number after decimal point
        /// -4 = More than one decimal point
        /// </remarks>
        private double ParseNum(string strWork, out int intNumLength, bool blnAllowNegative = false)
        {
            if (gComputationOptions.DecimalSeparator == default(char))
            {
                gComputationOptions.DecimalSeparator = MolecularWeightTool.DetermineDecimalPoint();
            }

            // Set intNumLength to -1 for now
            // If it doesn't get set to 0 (due to an error), it will get set to the
            // length of the matched number before exiting the sub
            intNumLength = -1;
            var strFoundNum = string.Empty;

            if (string.IsNullOrEmpty(strWork))
                strWork = EMPTY_STRING_CHAR.ToString();
            if ((strWork[0] < 48 || strWork[0] > 57) && strWork.Substring(0, 1) != gComputationOptions.DecimalSeparator.ToString() && !(strWork.Substring(0, 1) == "-" && blnAllowNegative == true))
            {
                intNumLength = 0; // No number found
                return -1;
            }

            // Start of string is a number or a decimal point, or (if allowed) a negative sign
            for (var intIndex = 0; intIndex < strWork.Length; intIndex++)
            {
                var strWorking = strWork.Substring(intIndex, 1);
                if (char.IsDigit(strWorking[0]) || strWorking == gComputationOptions.DecimalSeparator.ToString() || blnAllowNegative == true && strWorking == "-")
                {
                    strFoundNum += strWorking;
                }
                else
                {
                    break;
                }
            }

            if (strFoundNum.Length == 0 || strFoundNum == gComputationOptions.DecimalSeparator.ToString())
            {
                // No number at all or (more likely) no number after decimal point
                strFoundNum = (-3).ToString();
                intNumLength = 0;
            }
            else
            {
                // Check for more than one decimal point (. or ,)
                var intDecPtCount = 0;
                for (var intIndex = 0; intIndex < strFoundNum.Length; intIndex++)
                {
                    if (strFoundNum.Substring(intIndex, 1) == gComputationOptions.DecimalSeparator.ToString())
                        intDecPtCount = (short)(intDecPtCount + 1);
                }

                if (intDecPtCount > 1)
                {
                    // more than one intDecPtCount
                    strFoundNum = (-4).ToString();
                    intNumLength = 0;
                }
            }

            if (intNumLength < 0)
                intNumLength = (short)strFoundNum.Length;

            return NumberConverter.CDblSafe(strFoundNum);
        }

        public string PlainTextToRtfInternal(string strWorkText)
        {
            return PlainTextToRtfInternal(strWorkText, false, true, false, 0);
        }

        public string PlainTextToRtfInternal(string strWorkText, bool calculatorMode)
        {
            return PlainTextToRtfInternal(strWorkText, calculatorMode, true, false, 0);
        }

        public string PlainTextToRtfInternal(
            string strWorkText,
            bool calculatorMode,
            bool blnHighlightCharFollowingPercentSign)
        {
            return PlainTextToRtfInternal(strWorkText, calculatorMode, blnHighlightCharFollowingPercentSign, false, 0);
        }

        /// <summary>
        /// Converts plain text to formatted rtf text
        /// </summary>
        /// <param name="strWorkText"></param>
        /// <param name="calculatorMode">When true, does not superscript + signs and numbers following + signs</param>
        /// <param name="blnHighlightCharFollowingPercentSign">When true, change the character following a percent sign to red (and remove the percent sign)</param>
        /// <param name="blnOverrideErrorID"></param>
        /// <param name="errorIDOverride"></param>
        /// <returns></returns>
        public string PlainTextToRtfInternal(
            string strWorkText,
            bool calculatorMode,
            bool blnHighlightCharFollowingPercentSign,
            bool blnOverrideErrorID,
            int errorIDOverride)
        {
            // ReSharper disable CommentTypo

            // Rtf string must begin with {{\fonttbl{\f0\fcharset0\fprq2 Times New Roman;}}\pard\plain\fs25

            // and must end with } or {\fs30  }} if superscript was used

            // "{\super 3}C{\sub 6}H{\sub 6}{\fs30  }}"
            // strRTF = "{{\fonttbl{\f0\fcharset0\fprq2 " + rtfFormula(0).font + ";}}\pard\plain\fs25 ";
            // Old: strRTF = "{\rtf1\ansi\deff0\deftab720{\fonttbl{\f0\fswiss MS Sans Serif;}{\f1\froman\fcharset2 Symbol;}{\f2\froman\fcharset2 Times New Roman;}{\f3\froman " + lblMWT[0].FontName + ";}}{\colortbl\red0\green0\blue0;\red255\green0\blue0;}\deflang1033\pard\plain\f3\fs25 ";
            // old: strRTF = "{\rtf1\ansi\deff0\deftab720{\fonttbl{\f0\fswiss MS Sans Serif;}{\f1\froman\fcharset2 Symbol;}{\f2\froman " + lblMWT[0].FontName + ";}{\f3\fswiss\fprq2 System;}}{\colortbl\red0\green0\blue0;\red255\green0\blue0;}\deflang1033\pard\plain\f2\fs25 ";
            // f0                               f1                                 f2                          f3                               f4                      cf0 (black)        cf1 (red)          cf3 (white)
            // ReSharper disable StringLiteralTypo
            var strRTF = @"{\rtf1\ansi\deff0\deftab720{\fonttbl{\f0\fswiss MS Sans Serif;}{\f1\froman\fcharset2 Symbol;}{\f2\froman " + gComputationOptions.RtfFontName + @";}{\f3\froman Times New Roman;}{\f4\fswiss\fprq2 System;}}{\colortbl\red0\green0\blue0;\red255\green0\blue0;\red255\green255\blue255;}\deflang1033\pard\plain\f2\fs" + NumberConverter.CShortSafe(gComputationOptions.RtfFontSize * 2.5d) + " ";
            // ReSharper restore StringLiteralTypo

            // ReSharper restore CommentTypo

            if ((strWorkText ?? "") == string.Empty)
            {
                // Return a blank RTF string
                return strRTF + "}";
            }

            var blnSuperFound = false;
            var strWorkCharPrev = "";
            for (var intCharIndex = 0; intCharIndex < strWorkText.Length; intCharIndex++)
            {
                var strWorkChar = strWorkText.Substring(intCharIndex, 1);
                if (strWorkChar == "%" && blnHighlightCharFollowingPercentSign)
                {
                    // An error was found and marked by a % sign
                    // Highlight the character at the % sign, and remove the % sign
                    if (intCharIndex == strWorkText.Length - 1)
                    {
                        // At end of line
                        int errorID;
                        if (blnOverrideErrorID && errorIDOverride != 0)
                        {
                            errorID = errorIDOverride;
                        }
                        else
                        {
                            errorID = ErrorParams.ErrorID;
                        }

                        switch (errorID)
                        {
                            case var @case when 2 <= @case && @case <= 4:
                                // Error involves a parentheses, find last opening parenthesis, (, or opening curly bracket, {
                                for (var intCharIndex2 = strRTF.Length - 1; intCharIndex2 >= 2; intCharIndex2 -= 1)
                                {
                                    if (strRTF.Substring(intCharIndex2, 1) == "(")
                                    {
                                        strRTF = strRTF.Substring(0, intCharIndex2 - 1) + @"{\cf1 (}" + strRTF.Substring(intCharIndex2 + 1);
                                        break;
                                    }

                                    if (strRTF.Substring(intCharIndex2, 1) == "{")
                                    {
                                        strRTF = strRTF.Substring(0, intCharIndex2 - 1) + @"{\cf1 \{}" + strRTF.Substring(intCharIndex2 + 1);
                                        break;
                                    }
                                }

                                break;

                            case 13:
                            case 15:
                                // Error involves a bracket, find last opening bracket, [
                                for (var intCharIndex2 = strRTF.Length - 1; intCharIndex2 >= 2; intCharIndex2 -= 1)
                                {
                                    if (strRTF.Substring(intCharIndex2, 1) == "[")
                                    {
                                        strRTF = strRTF.Substring(0, intCharIndex2 - 1) + @"{\cf1 [}" + strRTF.Substring(intCharIndex2 + 1);
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
                        strWorkChar = strWorkText.Substring(intCharIndex + 1, 1);
                        // Handle curly brackets
                        if (strWorkChar == "{" || strWorkChar == "}")
                            strWorkChar = @"\" + strWorkChar;
                        strRTF = strRTF + @"{\cf1 " + strWorkChar + "}";
                        intCharIndex++;
                    }
                }
                else if (strWorkChar == "^")
                {
                    strRTF += @"{\super ^}";
                    blnSuperFound = true;
                }
                else if (strWorkChar == "_")
                {
                    strRTF += @"{\super}";
                    blnSuperFound = true;
                }
                else if (strWorkChar == "+" && !calculatorMode)
                {
                    strRTF += @"{\super +}";
                    blnSuperFound = true;
                }
                else if (strWorkChar == EMPTY_STRING_CHAR.ToString())
                {
                    // skip it, the tilde sign is used to add additional height to the formula line when isotopes are used
                    // If it's here from a previous time, we ignore it, adding it at the end if needed (if blnSuperFound = true)
                }
                else if (char.IsDigit(strWorkChar[0]) || strWorkChar == gComputationOptions.DecimalSeparator.ToString())
                {
                    // Number or period, so super or subscript it if needed
                    if (intCharIndex == 1)
                    {
                        // at beginning of line, so leave it alone. Probably out of place
                        strRTF += strWorkChar;
                    }
                    else if (!calculatorMode && (char.IsLetter(strWorkCharPrev[0]) || strWorkCharPrev == ")" || strWorkCharPrev == @"\}" || strWorkCharPrev == "+" || strWorkCharPrev == "_" || strRTF.Substring(strRTF.Length - 6, 3) == "sub"))
                    {
                        // subscript if previous character was a character, parentheses, curly bracket, plus sign, or was already subscripted
                        // But, don't use subscripts in calculator
                        strRTF = strRTF + @"{\sub " + strWorkChar + "}";
                    }
                    else if (!calculatorMode && gComputationOptions.BracketsAsParentheses && strWorkCharPrev == "]")
                    {
                        // only subscript after closing bracket, ], if brackets are being treated as parentheses
                        strRTF = strRTF + @"{\sub " + strWorkChar + "}";
                    }
                    else if (strRTF.Substring(strRTF.Length - 8, 5) == "super")
                    {
                        // if previous character was superscripted, then superscript this number too
                        strRTF = strRTF + @"{\super " + strWorkChar + "}";
                        blnSuperFound = true;
                    }
                    else
                    {
                        strRTF += strWorkChar;
                    }
                }
                else if (strWorkChar == " ")
                {
                    // Ignore it
                }
                else
                {
                    // Handle curly brackets
                    if (strWorkChar == "{" || strWorkChar == "}")
                        strWorkChar = @"\" + strWorkChar;
                    strRTF += strWorkChar;
                }

                strWorkCharPrev = strWorkChar;
            }

            if (blnSuperFound)
            {
                // Add an extra tall character, the tilde sign (~, RTF_HEIGHT_ADJUST_CHAR)
                // It is used to add additional height to the formula line when isotopes are used
                // It is colored white so the user does not see it
                strRTF = strRTF + @"{\fs" + NumberConverter.CShortSafe(gComputationOptions.RtfFontSize * 3) + @"\cf2 " + RTF_HEIGHT_ADJUST_CHAR + "}}";
            }
            else
            {
                strRTF += "}";
            }

            return strRTF;
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
        /// <param name="strAbbreviationSymbol"></param>
        /// <returns>0 if found and removed; 1 if error</returns>
        public int RemoveAbbreviationInternal(string strAbbreviationSymbol)
        {
            var blnRemoved = default(bool);

            strAbbreviationSymbol = strAbbreviationSymbol?.ToLower();

            for (var index = 1; index <= AbbrevAllCount; index++)
            {
                if ((AbbrevStats[index].Symbol?.ToLower() ?? "") == (strAbbreviationSymbol ?? ""))
                {
                    RemoveAbbreviationByIDInternal(index);
                    blnRemoved = true;
                }
            }

            if (blnRemoved)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Remove the abbreviation at index abbreviationID
        /// </summary>
        /// <param name="abbreviationID"></param>
        /// <returns>0 if found and removed; 1 if error</returns>
        public int RemoveAbbreviationByIDInternal(int abbreviationID)
        {
            bool blnRemoved;

            if (abbreviationID >= 1 && abbreviationID <= AbbrevAllCount)
            {
                for (var indexRemove = abbreviationID; indexRemove < AbbrevAllCount; indexRemove++)
                    AbbrevStats[indexRemove] = AbbrevStats[indexRemove + 1];

                AbbrevAllCount = (short)(AbbrevAllCount - 1);
                ConstructMasterSymbolsList();
                blnRemoved = true;
            }
            else
            {
                blnRemoved = false;
            }

            if (blnRemoved)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// Look for the caution statement and remove it
        /// </summary>
        /// <param name="strCautionSymbol"></param>
        /// <returns>0 if found and removed; 1 if error</returns>
        public int RemoveCautionStatementInternal(string strCautionSymbol)
        {
            var blnRemoved = default(bool);

            for (var intIndex = 1; intIndex <= CautionStatementCount; intIndex++)
            {
                if ((CautionStatements[intIndex, 0] ?? "") == (strCautionSymbol ?? ""))
                {
                    for (var intIndexRemove = intIndex; intIndexRemove < CautionStatementCount; intIndexRemove++)
                    {
                        CautionStatements[intIndexRemove, 0] = CautionStatements[intIndexRemove + 1, 0];
                        CautionStatements[intIndexRemove, 1] = CautionStatements[intIndexRemove + 1, 1];
                    }

                    CautionStatementCount -= 1;
                    blnRemoved = true;
                }
            }

            if (blnRemoved)
            {
                return 0;
            }

            return 1;
        }

        public void ResetErrorParamsInternal()
        {
            ErrorParams.ErrorCharacter = "";
            ErrorParams.ErrorID = 0;
            ErrorParams.ErrorPosition = 0;
        }

        protected void ResetProgress()
        {
            ProgressReset?.Invoke();
        }

        protected void ResetProgress(string strProgressStepDescription)
        {
            UpdateProgress(strProgressStepDescription, 0f);
            ProgressReset?.Invoke();
        }

        public string ReturnFormattedMassAndStdDev(double dblMass, double dblStdDev)
        {
            return ReturnFormattedMassAndStdDev(dblMass, dblStdDev, true, false);
        }

        public string ReturnFormattedMassAndStdDev(double dblMass, double dblStdDev, bool blnIncludeStandardDeviation)
        {
            return ReturnFormattedMassAndStdDev(dblMass, dblStdDev, blnIncludeStandardDeviation, false);
        }

        public string ReturnFormattedMassAndStdDev(double dblMass,
            double dblStdDev,
            bool blnIncludeStandardDeviation,
            bool blnIncludePctSign)
        {
            // Plan:
            // Round dblStdDev to 1 final digit.
            // Round dblMass to the appropriate place based on StdDev.

            // dblMass is the main number
            // dblStdDev is the standard deviation

            var strResult = string.Empty;

            try
            {
                double dblRoundedMain;
                string strPctSign;
                // blnIncludePctSign is True when formatting Percent composition values
                if (blnIncludePctSign)
                {
                    strPctSign = "%";
                }
                else
                {
                    strPctSign = "";
                }

                if (Math.Abs(dblStdDev) < float.Epsilon)
                {
                    // Standard deviation value is 0; simply return the result
                    strResult = dblMass.ToString("0.0####") + strPctSign + " (" + '±' + "0)";

                    // dblRoundedMain is used later, must copy dblMass to it
                    dblRoundedMain = dblMass;
                }
                else
                {
                    // First round dblStdDev to show just one number
                    var dblRoundedStdDev = double.Parse(dblStdDev.ToString("0E+000"));

                    // Now round dblMass
                    // Simply divide dblMass by 10^Exponent of the Standard Deviation
                    // Next round
                    // Now multiply to get back the rounded dblMass
                    var strWork = dblStdDev.ToString("0E+000");
                    var strStdDevShort = strWork.Substring(0, 1);

                    var intExponentValue = NumberConverter.CShortSafe(strWork.Substring(strWork.Length - 4));
                    var dblWork = dblMass / Math.Pow(10d, intExponentValue);
                    dblWork = Math.Round(dblWork, 0);
                    dblRoundedMain = dblWork * Math.Pow(10d, intExponentValue);

                    strWork = dblRoundedMain.ToString("0.0##E+00");

                    if (gComputationOptions.StdDevMode == StdDevMode.Short)
                    {
                        // StdDevType Short (Type 0)
                        strResult = dblRoundedMain.ToString();
                        if (blnIncludeStandardDeviation)
                        {
                            strResult = strResult + "(" + '±' + strStdDevShort + ")";
                        }

                        strResult += strPctSign;
                    }
                    else if (gComputationOptions.StdDevMode == StdDevMode.Scientific)
                    {
                        // StdDevType Scientific (Type 1)
                        strResult = dblRoundedMain + strPctSign;
                        if (blnIncludeStandardDeviation)
                        {
                            strResult += " (" + '±' + dblStdDev.ToString("0.000E+00") + ")";
                        }
                    }
                    else
                    {
                        // StdDevType Decimal
                        strResult = dblMass.ToString("0.0####") + strPctSign;
                        if (blnIncludeStandardDeviation)
                        {
                            strResult += " (" + '±' + dblRoundedStdDev + ")";
                        }
                    }
                }

                return strResult;
            }
            catch
            {
                MwtWinDllErrorHandler("MwtWinDll_clsElementAndMassRoutines|ReturnFormattedMassAndStdDev");
                ErrorParams.ErrorID = -10;
                ErrorParams.ErrorPosition = 0;
            }

            if (string.IsNullOrEmpty(strResult))
                strResult = string.Empty;
            return strResult;
        }

        public double RoundToMultipleOf10(double dblThisNum)
        {
            // Round to nearest 1, 2, or 5 (or multiple of 10 thereof)
            // First, find the exponent of dblThisNum
            var strWork = dblThisNum.ToString("0E+000");
            var intExponentValue = NumberConverter.CIntSafe(strWork.Substring(strWork.Length - 4));
            var dblWork = dblThisNum / Math.Pow(10d, intExponentValue);
            dblWork = NumberConverter.CIntSafe(dblWork);

            // dblWork should now be between 0 and 9
            switch (dblWork)
            {
                case 0d:
                case 1d:
                    dblThisNum = 1d;
                    break;
                case var @case when 2d <= @case && @case <= 4d:
                    dblThisNum = 2d;
                    break;
                default:
                    dblThisNum = 5d;
                    break;
            }

            // Convert dblThisNum back to the correct magnitude
            dblThisNum *= Math.Pow(10d, intExponentValue);

            return dblThisNum;
        }

        public double RoundToEvenMultiple(double dblValueToRound, double MultipleValue, bool blnRoundUp)
        {
            // Find the exponent of MultipleValue
            var strWork = MultipleValue.ToString("0E+000");
            var intExponentValue = NumberConverter.CIntSafe(strWork.Substring(strWork.Length - 4));

            var intLoopCount = 0;
            while ((dblValueToRound / MultipleValue).ToString() != Math.Round(dblValueToRound / MultipleValue, 0).ToString())
            {
                var dblWork = dblValueToRound / Math.Pow(10d, intExponentValue);
                dblWork = double.Parse(dblWork.ToString("0"));
                dblWork *= Math.Pow(10d, intExponentValue);
                if (blnRoundUp)
                {
                    if (dblWork <= dblValueToRound)
                    {
                        dblWork += Math.Pow(10d, intExponentValue);
                    }
                }
                else if (dblWork >= dblValueToRound)
                {
                    dblWork -= Math.Pow(10d, intExponentValue);
                }

                dblValueToRound = dblWork;
                intLoopCount += 1;
                if (intLoopCount > 500)
                {
                    // Debug.Assert False
                    break;
                }
            }

            return dblValueToRound;
        }

        public int SetAbbreviationInternal(string strSymbol, string strFormula,
            float sngCharge, bool blnIsAminoAcid)
        {
            return SetAbbreviationInternal(strSymbol, strFormula, sngCharge, blnIsAminoAcid, "", "", true);
        }

        public int SetAbbreviationInternal(string strSymbol, string strFormula,
            float sngCharge, bool blnIsAminoAcid,
            string strOneLetterSymbol)
        {
            return SetAbbreviationInternal(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, "", true);
        }

        public int SetAbbreviationInternal(string strSymbol, string strFormula,
            float sngCharge, bool blnIsAminoAcid,
            string strOneLetterSymbol,
            string strComment)
        {
            return SetAbbreviationInternal(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, true);
        }

        /// <summary>
        /// Adds a new abbreviation or updates an existing one (based on strSymbol)
        /// </summary>
        /// <param name="strSymbol"></param>
        /// <param name="strFormula"></param>
        /// <param name="sngCharge"></param>
        /// <param name="blnIsAminoAcid"></param>
        /// <param name="strOneLetterSymbol"></param>
        /// <param name="strComment"></param>
        /// <param name="blnValidateFormula">If true, make sure the formula is valid</param>
        /// <returns>0 if successful, otherwise an error ID</returns>
        /// <remarks>
        /// It is useful to set blnValidateFormula = False when you're defining all of the abbreviations at once,
        /// since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        /// defined, then the parsing of the first abbreviation will fail
        /// </remarks>
        public int SetAbbreviationInternal(
            string strSymbol, string strFormula,
            float sngCharge, bool blnIsAminoAcid,
            string strOneLetterSymbol,
            string strComment,
            bool blnValidateFormula)
        {
            var abbrevID = default(int);

            // See if the abbreviation is already present
            var blnAlreadyPresent = false;
            for (var index = 1; index <= AbbrevAllCount; index++)
            {
                if ((AbbrevStats[index].Symbol?.ToUpper() ?? "") == (strSymbol?.ToUpper() ?? ""))
                {
                    blnAlreadyPresent = true;
                    abbrevID = index;
                    break;
                }
            }

            // AbbrevStats is a 1-based array
            if (!blnAlreadyPresent)
            {
                if (AbbrevAllCount < MAX_ABBREV_COUNT)
                {
                    abbrevID = AbbrevAllCount + 1;
                    AbbrevAllCount = (short)(AbbrevAllCount + 1);
                }
                else
                {
                    // Too many abbreviations
                    ErrorParams.ErrorID = 196;
                }
            }

            if (abbrevID >= 1)
            {
                SetAbbreviationByIDInternal((short)abbrevID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, blnValidateFormula);
            }

            return ErrorParams.ErrorID;
        }

        public int SetAbbreviationByIDInternal(short intAbbrevID, string strSymbol,
            string strFormula, float sngCharge,
            bool blnIsAminoAcid)
        {
            return SetAbbreviationByIDInternal(intAbbrevID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, "", "", true);
        }

        public int SetAbbreviationByIDInternal(short intAbbrevID, string strSymbol,
            string strFormula, float sngCharge,
            bool blnIsAminoAcid,
            string strOneLetterSymbol)
        {
            return SetAbbreviationByIDInternal(intAbbrevID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, "", true);
        }

        public int SetAbbreviationByIDInternal(short intAbbrevID, string strSymbol,
            string strFormula, float sngCharge,
            bool blnIsAminoAcid,
            string strOneLetterSymbol,
            string strComment)
        {
            return SetAbbreviationByIDInternal(intAbbrevID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, true);
        }

        /// <summary>
        /// Adds a new abbreviation or updates an existing one (based on intAbbrevID)
        /// </summary>
        /// <param name="intAbbrevID">If intAbbrevID is less than 1, adds as a new abbreviation</param>
        /// <param name="strSymbol"></param>
        /// <param name="strFormula"></param>
        /// <param name="sngCharge"></param>
        /// <param name="blnIsAminoAcid"></param>
        /// <param name="strOneLetterSymbol"></param>
        /// <param name="strComment"></param>
        /// <param name="blnValidateFormula"></param>
        /// <returns>0 if successful, otherwise an error ID</returns>
        public int SetAbbreviationByIDInternal(
            short intAbbrevID, string strSymbol,
            string strFormula, float sngCharge,
            bool blnIsAminoAcid,
            string strOneLetterSymbol,
            string strComment,
            bool blnValidateFormula)
        {
            var udtComputationStats = new udtComputationStatsType();
            udtComputationStats.Initialize();

            var udtAbbrevSymbolStack = new udtAbbrevSymbolStackType();
            var blnInvalidSymbolOrFormula = default(bool);
            var intSymbolReference = default(short);

            ResetErrorParamsInternal();

            // Initialize the UDTs
            InitializeComputationStats(ref udtComputationStats);
            InitializeAbbrevSymbolStack(ref udtAbbrevSymbolStack);

            if (strSymbol.Length < 1)
            {
                // Symbol length is 0
                ErrorParams.ErrorID = 192;
            }
            else if (strSymbol.Length > MAX_ABBREV_LENGTH)
            {
                // Abbreviation symbol too long
                ErrorParams.ErrorID = 190;
            }
            else if (IsStringAllLetters(strSymbol))
            {
                if (strFormula.Length > 0)
                {
                    // Convert symbol to proper case mode
                    strSymbol = strSymbol.Substring(0, 1).ToUpper() + strSymbol.Substring(1).ToLower();

                    // If intAbbrevID is < 1 or larger than AbbrevAllCount, then define it
                    if (intAbbrevID < 1 || intAbbrevID > AbbrevAllCount + 1)
                    {
                        if (AbbrevAllCount < MAX_ABBREV_COUNT)
                        {
                            AbbrevAllCount = (short)(AbbrevAllCount + 1);
                            intAbbrevID = AbbrevAllCount;
                        }
                        else
                        {
                            // Too many abbreviations
                            ErrorParams.ErrorID = 196;
                            intAbbrevID = -1;
                        }
                    }

                    if (intAbbrevID >= 1)
                    {
                        // Make sure the abbreviation doesn't match one of the standard elements
                        var eSymbolMatchType = CheckElemAndAbbrev(strSymbol, ref intSymbolReference);

                        if (eSymbolMatchType == SymbolMatchMode.Element)
                        {
                            if ((ElementStats[intSymbolReference].Symbol ?? "") == strSymbol)
                            {
                                blnInvalidSymbolOrFormula = true;
                            }
                        }

                        if (!blnInvalidSymbolOrFormula && blnValidateFormula)
                        {
                            // Make sure the abbreviation's formula is valid
                            // This will also auto-capitalize the formula if auto-capitalize is turned on
                            var argdblStdDevSum = 0d;
                            var argCarbonOrSiliconReturnCount = 0;
                            strFormula = ParseFormulaRecursive(strFormula, ref udtComputationStats, ref udtAbbrevSymbolStack, false, ref argdblStdDevSum, ref argCarbonOrSiliconReturnCount);

                            if (ErrorParams.ErrorID != 0)
                            {
                                // An error occurred while parsing
                                // Already present in ErrorParams.ErrorID
                                // We'll still add the formula, but mark it as invalid
                                blnInvalidSymbolOrFormula = true;
                            }
                        }

                        AddAbbreviationWork(intAbbrevID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, blnInvalidSymbolOrFormula);

                        ConstructMasterSymbolsList();
                    }
                }
                else
                {
                    // Invalid formula (actually, blank formula)
                    ErrorParams.ErrorID = 160;
                }
            }
            else
            {
                // Symbol does not just contain letters
                ErrorParams.ErrorID = 194;
            }

            return ErrorParams.ErrorID;
        }

        /// <summary>
        /// Adds a new caution statement or updates an existing one (based on strSymbolCombo)
        /// </summary>
        /// <param name="strSymbolCombo"></param>
        /// <param name="strNewCautionStatement"></param>
        /// <returns>0 if successful, otherwise, returns an Error ID</returns>
        public int SetCautionStatementInternal(string strSymbolCombo, string strNewCautionStatement)
        {
            var blnAlreadyPresent = default(bool);

            ResetErrorParamsInternal();

            if (strSymbolCombo.Length >= 1 && strSymbolCombo.Length <= MAX_ABBREV_LENGTH)
            {
                // Make sure all the characters in strSymbolCombo are letters
                if (IsStringAllLetters(strSymbolCombo))
                {
                    if (strNewCautionStatement.Length > 0)
                    {
                        int intIndex;
                        // See if strSymbolCombo is present in CautionStatements[]
                        for (intIndex = 1; intIndex <= CautionStatementCount; intIndex++)
                        {
                            if ((CautionStatements[intIndex, 0] ?? "") == strSymbolCombo)
                            {
                                blnAlreadyPresent = true;
                                break;
                            }
                        }

                        // Caution statements is a 0-based array
                        if (!blnAlreadyPresent)
                        {
                            if (CautionStatementCount < MAX_CAUTION_STATEMENTS)
                            {
                                CautionStatementCount += 1;
                                intIndex = CautionStatementCount;
                            }
                            else
                            {
                                // Too many caution statements
                                ErrorParams.ErrorID = 1215;
                                intIndex = -1;
                            }
                        }

                        if (intIndex >= 1)
                        {
                            CautionStatements[intIndex, 0] = strSymbolCombo;
                            CautionStatements[intIndex, 1] = strNewCautionStatement;
                        }
                    }
                    else
                    {
                        // Caution description length is 0
                        ErrorParams.ErrorID = 1210;
                    }
                }
                else
                {
                    // Caution symbol doesn't just contain letters
                    ErrorParams.ErrorID = 1205;
                }
            }
            else
            {
                // Symbol length is 0 or is greater than MAX_ABBREV_LENGTH
                ErrorParams.ErrorID = 1200;
            }

            return ErrorParams.ErrorID;
        }

        public void SetChargeCarrierMassInternal(double dblMass)
        {
            mChargeCarrierMass = dblMass;
        }

        public int SetElementInternal(string strSymbol, double dblMass, double dblUncertainty, float sngCharge)
        {
            return SetElementInternal(strSymbol, dblMass, dblUncertainty, sngCharge, true);
        }

        /// <summary>
        /// Update the values for a single element (based on strSymbol)
        /// </summary>
        /// <param name="strSymbol"></param>
        /// <param name="dblMass"></param>
        /// <param name="dblUncertainty"></param>
        /// <param name="sngCharge"></param>
        /// <param name="blnRecomputeAbbreviationMasses">Set to False if updating several elements</param>
        /// <returns></returns>
        public int SetElementInternal(string strSymbol, double dblMass,
            double dblUncertainty, float sngCharge,
            bool blnRecomputeAbbreviationMasses)
        {
            var blnFound = default(bool);

            for (var intIndex = 1; intIndex <= ELEMENT_COUNT; intIndex++)
            {
                if ((strSymbol?.ToLower() ?? "") == (ElementStats[intIndex].Symbol?.ToLower() ?? ""))
                {
                    var stats = ElementStats[intIndex];
                    stats.Mass = dblMass;
                    stats.Uncertainty = dblUncertainty;
                    stats.Charge = sngCharge;

                    blnFound = true;
                    break;
                }
            }

            if (blnFound)
            {
                if (blnRecomputeAbbreviationMasses)
                    RecomputeAbbreviationMassesInternal();
                return 0;
            }

            return 1;
        }

        public int SetElementIsotopesInternal(string strSymbol, short intIsotopeCount, ref double[] dblIsotopeMassesOneBased, ref float[] sngIsotopeAbundancesOneBased)
        {
            var blnFound = default(bool);

            for (var intIndex = 1; intIndex <= ELEMENT_COUNT; intIndex++)
            {
                if ((strSymbol?.ToLower() ?? "") == (ElementStats[intIndex].Symbol?.ToLower() ?? ""))
                {
                    var stats = ElementStats[intIndex];
                    if (intIsotopeCount < 0)
                        intIsotopeCount = 0;
                    stats.IsotopeCount = intIsotopeCount;
                    for (var intIsotopeIndex = 1; intIsotopeIndex <= stats.IsotopeCount; intIsotopeIndex++)
                    {
                        if (intIsotopeIndex > MAX_ISOTOPES)
                            break;
                        stats.Isotopes[intIsotopeIndex].Mass = dblIsotopeMassesOneBased[intIsotopeIndex];
                        stats.Isotopes[intIsotopeIndex].Abundance = sngIsotopeAbundancesOneBased[intIsotopeIndex];
                    }

                    blnFound = true;
                    break;
                }
            }

            if (blnFound)
            {
                return 0;
            }

            return 1;
        }

        public void SetElementModeInternal(ElementMassMode NewElementMode)
        {
            SetElementModeInternal(NewElementMode, true);
        }

        /// <summary>
        /// Set the element mode
        /// </summary>
        /// <param name="NewElementMode"></param>
        /// <param name="blnMemoryLoadElementValues"></param>
        /// <remarks>
        /// The only time you would want blnMemoryLoadElementValues to be False is if you're
        /// manually setting element weight values, but want to let the software know that
        /// they're average, isotopic, or integer values
        /// </remarks>
        public void SetElementModeInternal(ElementMassMode NewElementMode, bool blnMemoryLoadElementValues)
        {
            try
            {
                if (NewElementMode >= ElementMassMode.Average && NewElementMode <= ElementMassMode.Integer)
                {
                    if (NewElementMode != mCurrentElementMode || blnMemoryLoadElementValues)
                    {
                        mCurrentElementMode = NewElementMode;

                        if (blnMemoryLoadElementValues)
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
        /// <param name="messageID"></param>
        /// <param name="strNewMessage"></param>
        /// <returns>0 if success; 1 if failure</returns>
        public int SetMessageStatementInternal(int messageID, string strNewMessage)
        {
            if (messageID >= 1 && messageID <= MESSAGE_STATEMENT_DIM_COUNT && strNewMessage.Length > 0)
            {
                MessageStatements[messageID] = strNewMessage;
                return 0;
            }

            return 1;
        }

        private void ShellSortSymbols(int lowIndex, int highIndex)
        {
            var PointerArray = new int[highIndex + 1];
            var SymbolsStore = new string[highIndex + 1, 2];

            // MasterSymbolsList starts at lowIndex
            for (var index = lowIndex; index <= highIndex; index++)
                PointerArray[index] = index;

            ShellSortSymbolsWork(ref PointerArray, lowIndex, highIndex);

            // Reassign MasterSymbolsList array according to PointerArray order
            // First, copy to a temporary array (I know it eats up memory, but I have no choice)
            for (var index = lowIndex; index <= highIndex; index++)
            {
                SymbolsStore[index, 0] = MasterSymbolsList[index, 0];
                SymbolsStore[index, 1] = MasterSymbolsList[index, 1];
            }

            // Now, put them back into the MasterSymbolsList() array in the correct order
            // Use PointerArray() for this
            for (var index = lowIndex; index <= highIndex; index++)
            {
                MasterSymbolsList[index, 0] = SymbolsStore[PointerArray[index], 0];
                MasterSymbolsList[index, 1] = SymbolsStore[PointerArray[index], 1];
            }
        }

        /// <summary>
        /// Sort the list using a shell sort
        /// </summary>
        /// <param name="PointerArray"></param>
        /// <param name="lowIndex"></param>
        /// <param name="highIndex"></param>
        private void ShellSortSymbolsWork(ref int[] PointerArray, int lowIndex, int highIndex)
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
                    var pointerSwap = PointerArray[index];
                    int indexCompare;
                    for (indexCompare = index - incrementAmount; indexCompare >= lowIndex; indexCompare += -incrementAmount)
                    {
                        // Use <= to sort ascending; Use > to sort descending
                        // Sort by decreasing length
                        var Length1 = MasterSymbolsList[PointerArray[indexCompare], 0].Length;
                        var Length2 = MasterSymbolsList[pointerSwap, 0].Length;
                        if (Length1 > Length2)
                            break;
                        // If same length, sort alphabetically
                        if (Length1 == Length2)
                        {
                            if (string.Compare(MasterSymbolsList[PointerArray[indexCompare], 0].ToUpper(), MasterSymbolsList[pointerSwap, 0].ToUpper(), StringComparison.Ordinal) <= 0)
                                break;
                        }

                        PointerArray[indexCompare + incrementAmount] = PointerArray[indexCompare];
                    }

                    PointerArray[indexCompare + incrementAmount] = pointerSwap;
                }

                incrementAmount /= 3;
            }
        }

        public void SetShowErrorMessageDialogs(bool blnValue)
        {
            ShowErrorMessageDialogs = blnValue;
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
                    var udtCompare = AbbrevStats[index];
                    int indexCompare;
                    for (indexCompare = index - incrementAmount; indexCompare >= lowIndex; indexCompare += -incrementAmount)
                    {
                        // Use <= to sort ascending; Use > to sort descending
                        if (string.Compare(AbbrevStats[indexCompare].Symbol, udtCompare.Symbol, StringComparison.Ordinal) <= 0)
                            break;
                        AbbrevStats[indexCompare + incrementAmount] = AbbrevStats[indexCompare];
                    }

                    AbbrevStats[indexCompare + incrementAmount] = udtCompare;
                }

                incrementAmount /= 3;
            }

            // Need to re-construct the master symbols list
            ConstructMasterSymbolsList();
        }

        /// <summary>
        /// Adds spaces to strWork until the length is intLength
        /// </summary>
        /// <param name="strWork"></param>
        /// <param name="intLength"></param>
        /// <returns></returns>
        public string SpacePad(string strWork, short intLength)
        {
            while (strWork.Length < intLength)
                strWork += " ";

            return strWork;
        }

        private string SpacePadFront(string strWork, short intLength)
        {
            while (strWork.Length < intLength)
                strWork = " " + strWork;

            return strWork;
        }

        /// <summary>
        /// Update the progress description
        /// </summary>
        /// <param name="strProgressStepDescription">Description of the current processing occurring</param>
        /// <remarks></remarks>
        protected void UpdateProgress(string strProgressStepDescription)
        {
            UpdateProgress(strProgressStepDescription, mProgressPercentComplete);
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <param name="sngPercentComplete">Value between 0 and 100, indicating percent complete</param>
        /// <remarks></remarks>
        protected void UpdateProgress(float sngPercentComplete)
        {
            UpdateProgress(ProgressStepDescription, sngPercentComplete);
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <param name="strProgressStepDescription">Description of the current processing occurring</param>
        /// <param name="sngPercentComplete">Value between 0 and 100, indicating percent complete</param>
        /// <remarks></remarks>
        protected void UpdateProgress(string strProgressStepDescription, float sngPercentComplete)
        {
            var blnDescriptionChanged = !string.Equals(strProgressStepDescription, mProgressStepDescription);

            mProgressStepDescription = string.Copy(strProgressStepDescription);
            if (sngPercentComplete < 0f)
            {
                sngPercentComplete = 0f;
            }
            else if (sngPercentComplete > 100f)
            {
                sngPercentComplete = 100f;
            }

            mProgressPercentComplete = sngPercentComplete;

            if (blnDescriptionChanged)
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
            var intInvalidAbbreviationCount = default(short);

            for (short intAbbrevIndex = 1; intAbbrevIndex <= AbbrevAllCount; intAbbrevIndex++)
            {
                var stats = AbbrevStats[intAbbrevIndex];
                SetAbbreviationByIDInternal(intAbbrevIndex, stats.Symbol, stats.Formula, stats.Charge, stats.IsAminoAcid, stats.OneLetterSymbol, stats.Comment, true);
                if (stats.InvalidSymbolOrFormula)
                {
                    intInvalidAbbreviationCount = (short)(intInvalidAbbreviationCount + 1);
                }
            }

            return intInvalidAbbreviationCount;
        }
    }
}
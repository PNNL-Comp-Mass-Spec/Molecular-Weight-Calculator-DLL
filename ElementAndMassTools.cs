using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace MwtWinDll
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
        private const int MAX_ABBREV_LENGTH = 6;
        private const int MAX_ISOTOPES = 11;
        private const int MAX_CAUTION_STATEMENTS = 100;

        private const char EMPTY_STRING_CHAR = '~';
        private const char RTF_HEIGHT_ADJUST_CHAR = '~'; // A hidden character to adjust the height of Rtf Text Boxes when using superscripts

        public enum emElementModeConstants
        {
            emAverageMass = 1,
            emIsotopicMass = 2,
            emIntegerMass = 3
        }

        public enum smStdDevModeConstants
        {
            smShort = 0,
            smScientific = 1,
            smDecimal = 2
        }

        public enum ccCaseConversionConstants
        {
            ccConvertCaseUp = 0,
            ccExactCase = 1,
            ccSmartCase = 2
        }

        private enum smtSymbolMatchTypeConstants
        {
            smtUnknown = 0,
            smtElement = 1,
            smtAbbreviation = 2
        }

        protected enum eMessageTypeConstants
        {
            Normal = 0,
            ErrorMsg = 1,
            Warning = 2
        }

        #endregion

        #region "Data classes"

        public class udtOptionsType
        {
            public MolecularWeightTool.arAbbrevRecognitionModeConstants AbbrevRecognitionMode;
            public bool BracketsAsParentheses;
            public ccCaseConversionConstants CaseConversion;
            public char DecimalSeparator;
            public string RtfFontName;
            public short RtfFontSize;
            public smStdDevModeConstants StdDevMode; // Can be 0, 1, or 2 (see smStdDevModeConstants)
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
                Isotopes = new udtIsotopeInfoType[12];
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

        private udtErrorDescriptionType ErrorParams = new udtErrorDescriptionType();

        /// <summary>
        /// Charge carrier mass
        /// 1.00727649 for monoisotopic mass or 1.00739 for average mass
        /// </summary>
        private double mChargeCarrierMass;

        private emElementModeConstants mCurrentElementMode;
        private string mStrCautionDescription;
        private udtComputationStatsType mComputationStatsSaved = new udtComputationStatsType();

        private bool mShowErrorMessageDialogs;
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
            get
            {
                return mAbortProcessing;
            }
            set
            {
                mAbortProcessing = value;
            }
        }

        public emElementModeConstants ElementModeInternal
        {
            get
            {
                return mCurrentElementMode;
            }
            set
            {
                SetElementModeInternal(value);
            }
        }

        public string LogFilePath
        {
            get
            {
                return mLogFilePath;
            }
        }

        public string LogFolderPath
        {
            get
            {
                return mLogFolderPath;
            }
            set
            {
                mLogFolderPath = value;
            }
        }

        public bool LogMessagesToFile
        {
            get
            {
                return mLogMessagesToFile;
            }
            set
            {
                mLogMessagesToFile = value;
            }
        }

        public virtual string ProgressStepDescription
        {
            get
            {
                return mProgressStepDescription;
            }
        }

        /// <summary>
        /// Percent complete; ranges from 0 to 100, but can contain decimal percentage values
        /// </summary>
        /// <returns></returns>
        public float ProgressPercentComplete
        {
            get
            {
                return Conversions.ToSingle(Math.Round(mProgressPercentComplete, 2));
            }
        }

        public bool ShowErrorMessageDialogs
        {
            get
            {
                return mShowErrorMessageDialogs;
            }
            set
            {
                mShowErrorMessageDialogs = value;
            }
        }

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
        /// <param name="strFormula">Input/output; ParseFormulaPublic will standardize the format</param>
        /// <param name="sngCharge"></param>
        /// <param name="blnIsAminoAcid"></param>
        /// <param name="strOneLetter"></param>
        /// <param name="strComment"></param>
        /// <param name="blnInvalidSymbolOrFormula"></param>
        private void AddAbbreviationWork(short intAbbrevIndex, string strSymbol, ref string strFormula, float sngCharge, bool blnIsAminoAcid, string strOneLetter = "", string strComment = "", bool blnInvalidSymbolOrFormula = false)
        {
            {
                var withBlock = AbbrevStats[intAbbrevIndex];
                withBlock.InvalidSymbolOrFormula = blnInvalidSymbolOrFormula;
                withBlock.Symbol = strSymbol;
                withBlock.Formula = strFormula;
                withBlock.Mass = ComputeFormulaWeight(ref strFormula);
                if (withBlock.Mass < 0d)
                {
                    // Error occurred computing mass for abbreviation
                    withBlock.Mass = 0d;
                    withBlock.InvalidSymbolOrFormula = true;
                }

                withBlock.Charge = sngCharge;
                withBlock.OneLetterSymbol = Strings.UCase(strOneLetter);
                withBlock.IsAminoAcid = blnIsAminoAcid;
                withBlock.Comment = strComment;
            }
        }

        private void AddToCautionDescription(string strTextToAdd)
        {
            if (Strings.Len(mStrCautionDescription) > 0)
            {
                mStrCautionDescription = mStrCautionDescription;
            }

            mStrCautionDescription += strTextToAdd;
        }

        private void CheckCaution(string strFormulaExcerpt)
        {
            string strTest;
            string strNewCaution;
            short intLength;
            for (intLength = 1; intLength <= MAX_ABBREV_LENGTH; intLength++)
            {
                if (intLength > strFormulaExcerpt.Length)
                    break;
                strTest = strFormulaExcerpt.Substring(0, intLength);
                strNewCaution = LookupCautionStatement(strTest);
                if (strNewCaution is object && strNewCaution.Length > 0)
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
                        break;
                    // No number, but no error
                    // That's OK
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
        private smtSymbolMatchTypeConstants CheckElemAndAbbrev(string strFormulaExcerpt, ref short symbolReference)
        {
            short intIndex;
            var eSymbolMatchType = default(smtSymbolMatchTypeConstants);

            // MasterSymbolsList() stores the element symbols, abbreviations, & amino acids in order of longest length to
            // shortest length, non-alphabetized, for use in symbol matching when parsing a formula

            // MasterSymbolsList(intIndex,0) contains the symbol to be matched
            // MasterSymbolsList(intIndex,1) contains E for element, A for amino acid, or N for normal abbreviation, followed by
            // the reference number in the master list
            // For example for Carbon, MasterSymbolsList(intIndex,0) = "C" and MasterSymbolsList(intIndex,1) = "E6"

            // Look for match, stepping directly through MasterSymbolsList()
            // List is sorted by reverse length, so can do all at once

            var loopTo = MasterSymbolsListCount;
            for (intIndex = 1; intIndex <= loopTo; intIndex++)
            {
                if (MasterSymbolsList[intIndex, 0].Length > 0)
                {
                    if ((Strings.Left(strFormulaExcerpt, Strings.Len(MasterSymbolsList[intIndex, 0])) ?? "") == (MasterSymbolsList[intIndex, 0] ?? ""))
                    {
                        // Matched a symbol
                        switch (Strings.UCase(Strings.Left(MasterSymbolsList[intIndex, 1], 1)) ?? "")
                        {
                            case "E": // An element
                                eSymbolMatchType = smtSymbolMatchTypeConstants.smtElement;
                                break;
                            case "A": // An abbreviation or amino acid
                                eSymbolMatchType = smtSymbolMatchTypeConstants.smtAbbreviation;
                                break;
                            default:
                                // error
                                eSymbolMatchType = smtSymbolMatchTypeConstants.smtUnknown;
                                symbolReference = -1;
                                break;
                        }

                        if (eSymbolMatchType != smtSymbolMatchTypeConstants.smtUnknown)
                        {
                            symbolReference = (short)Math.Round(Conversion.Val(Strings.Mid(MasterSymbolsList[intIndex, 1], 2)));
                        }

                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Zero-length entry found in MasterSymbolsList(); this is unexpected");
                }
            }

            return eSymbolMatchType;
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
            else
            {
                return -1;
            }
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
        public short ComputeIsotopicAbundances(ref string strFormulaIn, short intChargeState, ref string strResults, ref double[,] ConvolutedMSData2DOneBased, ref int ConvolutedMSDataCount)
        {
            bool blnUseFactorials = false;
            bool blnAddProtonChargeCarrier = true;
            return ComputeIsotopicAbundancesInternal(ref strFormulaIn, intChargeState, ref strResults, ref ConvolutedMSData2DOneBased, ref ConvolutedMSDataCount, "Isotopic Abundances for", "Mass/Charge", "Fraction", "Intensity", blnUseFactorials, blnAddProtonChargeCarrier);
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
        public short ComputeIsotopicAbundances(ref string strFormulaIn, short intChargeState, ref string strResults, ref double[,] ConvolutedMSData2DOneBased, ref int ConvolutedMSDataCount, bool blnAddProtonChargeCarrier)
        {
            const bool blnUseFactorials = false;
            return ComputeIsotopicAbundancesInternal(ref strFormulaIn, intChargeState, ref strResults, ref ConvolutedMSData2DOneBased, ref ConvolutedMSDataCount, "Isotopic Abundances for", "Mass/Charge", "Fraction", "Intensity", blnUseFactorials, blnAddProtonChargeCarrier);
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
        public short ComputeIsotopicAbundancesInternal(ref string strFormulaIn, short intChargeState, ref string strResults, ref double[,] ConvolutedMSData2DOneBased, ref int ConvolutedMSDataCount, string strHeaderIsotopicAbundances, string strHeaderMassToCharge, string strHeaderFraction, string strHeaderIntensity, bool blnUseFactorials, bool blnAddProtonChargeCarrier)
        {
            string strFormula, strModifiedFormula;
            double dblWorkingFormulaMass;
            double dblMassDefect, dblExactBaseIsoMass, dblMaxPercentDifference;
            short intElementIndex, intElementCount;
            int massIndex, rowIndex;
            var udtComputationStats = new udtComputationStatsType();
            udtComputationStats.Initialize();
            double dblTemp;
            udtIsoResultsByElementType[] IsoStats;
            short IsotopeStartingMass, IsotopeCount, IsotopeEndingMass;
            short MasterElementIndex;
            int AtomCount;
            double dblCount;
            int PredictedCombos, CombosFound;
            int ComboIndex;
            short IsotopeIndex, intIndex;
            int IndexToStoreAbundance;
            double dblThisComboFractionalAbundance, dblNextComboFractionalAbundance = default;
            bool blnRatioMethodUsed, blnRigorousMethodUsed;
            const string strDeuteriumEquiv = "^2.014H";
            bool blnReplaceDeuterium;
            int intAsciiOfNext;
            int IsotopeCountInThisCombo;
            string strOutput;
            long PredictedConvIterations;
            int PredictedTotalComboCalcs, CompletedComboCalcs;
            const double MIN_ABUNDANCE_TO_KEEP = 0.000001d;
            const double CUTOFF_FOR_RATIO_METHOD = 0.00001d;

            // AbundDenom  and  AbundSuffix are only needed if using the easily-overflowed factorial method
            double AbundDenom, AbundSuffix;
            int[] AtomTrackHistory;
            int[,] IsoCombos; // 2D array: Holds the # of each isotope for each combination
                              // For example, Two chlorine atoms, Cl2, has at most 6 combos since Cl isotopes are 35, 36, and 37
                              // m1  m2  m3
                              // 2   0   0
                              // 1   1   0
                              // 1   0   1
                              // 0   2   0
                              // 0   1   1
                              // 0   0   2

            udtIsoResultsOverallType[] ConvolutedAbundances; // Fractional abundance at each mass; 1-based array
            int ConvolutedAbundanceStartMass;
            int MaxWeight, MinWeight, ResultingMassCountForElement;
            var blnExplicitIsotopesPresent = default(bool);
            var ExplicitIsotopeCount = default(short);
            int SubIndex, sigma;
            double dblSumI, dblLogSigma, dblSumF;
            double dblWorkingSum;
            double dblLogFreq;
            double dblLogRho = default, dblFractionalAbundanceSaved, dblRho;
            double intM, intMPrime;
            double dblRatioOfFreqs;
            string strMessage;
            float sngPercentComplete;

            // Make sure formula is not blank
            if (strFormulaIn is null || strFormulaIn.Length == 0)
            {
                return -1;
            }

            mAbortProcessing = false;
            try
            {
                // Change strHeaderMassToCharge to "Neutral Mass" if intChargeState = 0 and strHeaderMassToCharge is "Mass/Charge"
                if (intChargeState == 0)
                {
                    if (strHeaderMassToCharge == "Mass/Charge")
                    {
                        strHeaderMassToCharge = "Neutral Mass";
                    }
                }

                // Parse Formula to determine if valid and number of each element
                strFormula = strFormulaIn;
                dblWorkingFormulaMass = ParseFormulaPublic(ref strFormula, ref udtComputationStats, false);
                if (dblWorkingFormulaMass < 0d)
                {
                    // Error occurred; information is stored in ErrorParams
                    strResults = LookupMessage(350) + ": " + LookupMessage(ErrorParams.ErrorID);
                    return -1;
                }

                // See if Deuterium is present by looking for a fractional amount of Hydrogen
                // strFormula will contain a capital D followed by a number or another letter (or the end of formula)
                // If found, replace each D with ^2.014H and re-compute
                dblCount = udtComputationStats.Elements[1].Count;
                if (Math.Abs(dblCount - (int)Math.Round(dblCount)) > float.Epsilon)
                {
                    // Deuterium is present
                    strModifiedFormula = "";
                    intIndex = 1;
                    while (intIndex <= Strings.Len(strFormula))
                    {
                        blnReplaceDeuterium = false;
                        if (Strings.Mid(strFormula, intIndex, 1) == "D")
                        {
                            if (intIndex == Strings.Len(strFormula))
                            {
                                blnReplaceDeuterium = true;
                            }
                            else
                            {
                                intAsciiOfNext = Strings.Asc(Strings.Mid(strFormula, intIndex + 1, 1));
                                if (intAsciiOfNext < 97 || intAsciiOfNext > 122)
                                {
                                    blnReplaceDeuterium = true;
                                }
                            }

                            if (blnReplaceDeuterium)
                            {
                                if (intIndex > 1)
                                {
                                    strModifiedFormula = Strings.Left(strFormula, intIndex - 1);
                                }

                                strModifiedFormula += strDeuteriumEquiv;
                                if (intIndex < Strings.Len(strFormula))
                                {
                                    strModifiedFormula += Strings.Mid(strFormula, intIndex + 1);
                                }

                                strFormula = strModifiedFormula;
                                intIndex = 0;
                            }
                        }

                        intIndex = (short)(intIndex + 1);
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
                for (intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                {
                    dblCount = udtComputationStats.Elements[intElementIndex].Count;
                    if (Math.Abs(dblCount - (int)Math.Round(dblCount)) > float.Epsilon)
                    {
                        strResults = LookupMessage(350) + ": " + LookupMessage(805) + ": " + ElementStats[intElementIndex].Symbol + dblCount;
                        return -1;
                    }
                }

                // Remove occurrences of explicitly defined isotopes from the formula
                for (intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                {
                    {
                        var withBlock = udtComputationStats.Elements[intElementIndex];
                        if (withBlock.IsotopeCount > 0)
                        {
                            blnExplicitIsotopesPresent = true;
                            ExplicitIsotopeCount += withBlock.IsotopeCount;
                            var loopTo = withBlock.IsotopeCount;
                            for (IsotopeIndex = 1; IsotopeIndex <= loopTo; IsotopeIndex++)
                                withBlock.Count = withBlock.Count - withBlock.Isotopes[IsotopeIndex].Count;
                        }
                    }
                }

                // Determine the number of elements present in strFormula
                intElementCount = 0;
                for (intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
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
                IsoStats = new udtIsoResultsByElementType[(intElementCount + 1)];

                // Step through udtComputationStats.Elements() again and copy info into IsoStats()
                // In addition, determine minimum and maximum weight for the molecule
                intElementCount = 0;
                MinWeight = 0;
                MaxWeight = 0;
                for (intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                {
                    if (udtComputationStats.Elements[intElementIndex].Used)
                    {
                        if (udtComputationStats.Elements[intElementIndex].Count > 0d)
                        {
                            intElementCount = (short)(intElementCount + 1);
                            IsoStats[intElementCount].ElementIndex = intElementIndex;
                            IsoStats[intElementCount].AtomCount = (int)Math.Round(udtComputationStats.Elements[intElementIndex].Count); // Note: Ignoring .Elements(intElementIndex).IsotopicCorrection
                            IsoStats[intElementCount].ExplicitMass = ElementStats[intElementIndex].Mass;
                            {
                                var withBlock1 = ElementStats[intElementIndex];
                                MinWeight = (int)Math.Round(MinWeight + IsoStats[intElementCount].AtomCount * Math.Round(withBlock1.Isotopes[1].Mass, 0));
                                MaxWeight = (int)Math.Round(MaxWeight + IsoStats[intElementCount].AtomCount * Math.Round(withBlock1.Isotopes[withBlock1.IsotopeCount].Mass, 0));
                            }
                        }
                    }
                }

                if (blnExplicitIsotopesPresent)
                {
                    // Add the isotopes, pretending they are unique elements
                    for (intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                    {
                        {
                            var withBlock2 = udtComputationStats.Elements[intElementIndex];
                            if (withBlock2.IsotopeCount > 0)
                            {
                                var loopTo1 = withBlock2.IsotopeCount;
                                for (IsotopeIndex = 1; IsotopeIndex <= loopTo1; IsotopeIndex++)
                                {
                                    intElementCount = (short)(intElementCount + 1);
                                    IsoStats[intElementCount].boolExplicitIsotope = true;
                                    IsoStats[intElementCount].ElementIndex = intElementIndex;
                                    IsoStats[intElementCount].AtomCount = (int)Math.Round(withBlock2.Isotopes[IsotopeIndex].Count);
                                    IsoStats[intElementCount].ExplicitMass = withBlock2.Isotopes[IsotopeIndex].Mass;
                                    {
                                        var withBlock3 = IsoStats[intElementCount];
                                        MinWeight = (int)Math.Round(MinWeight + withBlock3.AtomCount * withBlock3.ExplicitMass);
                                        MaxWeight = (int)Math.Round(MaxWeight + withBlock3.AtomCount * withBlock3.ExplicitMass);
                                    }
                                }
                            }
                        }
                    }
                }

                if (MinWeight < 0)
                    MinWeight = 0;

                // Create an array to hold the Fractional Abundances for all the masses
                ConvolutedMSDataCount = MaxWeight - MinWeight + 1;
                ConvolutedAbundanceStartMass = MinWeight;
                ConvolutedAbundances = new udtIsoResultsOverallType[ConvolutedMSDataCount + 1];

                // Predict the total number of computations required; show progress if necessary
                PredictedTotalComboCalcs = 0;
                var loopTo2 = intElementCount;
                for (intElementIndex = 1; intElementIndex <= loopTo2; intElementIndex++)
                {
                    MasterElementIndex = IsoStats[intElementIndex].ElementIndex;
                    AtomCount = IsoStats[intElementIndex].AtomCount;
                    IsotopeCount = ElementStats[MasterElementIndex].IsotopeCount;
                    PredictedCombos = FindCombosPredictIterations(AtomCount, IsotopeCount);
                    PredictedTotalComboCalcs += PredictedCombos;
                }

                ResetProgress("Finding Isotopic Abundances: Computing abundances");

                // For each element, compute all of the possible combinations
                CompletedComboCalcs = 0;
                var loopTo3 = intElementCount;
                for (intElementIndex = 1; intElementIndex <= loopTo3; intElementIndex++)
                {
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
                        {
                            var withBlock4 = ElementStats[MasterElementIndex];
                            IsotopeCount = withBlock4.IsotopeCount;
                            IsotopeStartingMass = (short)Math.Round(Math.Round(withBlock4.Isotopes[1].Mass, 0));
                            IsotopeEndingMass = (short)Math.Round(Math.Round(withBlock4.Isotopes[IsotopeCount].Mass, 0));
                        }
                    }

                    PredictedCombos = FindCombosPredictIterations(AtomCount, IsotopeCount);
                    if (PredictedCombos > 10000000)
                    {
                        strMessage = "Too many combinations necessary for prediction of isotopic distribution: " + PredictedCombos.ToString("#,##0") + ControlChars.NewLine + "Please use a simpler formula or reduce the isotopic range defined for the element (currently " + IsotopeCount + ")";
                        if (mShowErrorMessageDialogs)
                        {
                            Interaction.MsgBox(strMessage);
                        }

                        LogMessage(strMessage, eMessageTypeConstants.ErrorMsg);
                        return -1;
                    }

                    IsoCombos = new int[PredictedCombos + 1, (IsotopeCount + 1)];
                    AtomTrackHistory = new int[(IsotopeCount + 1)];
                    AtomTrackHistory[1] = AtomCount;
                    int argCurrentRow = 1;
                    CombosFound = FindCombosRecurse(ref IsoCombos, AtomCount, IsotopeCount, IsotopeCount, ref argCurrentRow, 1, ref AtomTrackHistory);

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
                    ResultingMassCountForElement = MaxWeight - MinWeight + 1;
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
                        dblFractionalAbundanceSaved = 0d;
                        var loopTo4 = CombosFound;
                        for (ComboIndex = 1; ComboIndex <= loopTo4; ComboIndex++)
                        {
                            CompletedComboCalcs += 1;
                            sngPercentComplete = CompletedComboCalcs / (float)PredictedTotalComboCalcs * 100f;
                            if (CompletedComboCalcs % 10 == 0)
                            {
                                UpdateProgress(sngPercentComplete);
                            }

                            dblThisComboFractionalAbundance = -1;
                            blnRatioMethodUsed = false;
                            blnRigorousMethodUsed = false;
                            if (blnUseFactorials)
                            {
                                // #######
                                // Rigorous, slow, easily overflowed method
                                // #######
                                //
                                blnRigorousMethodUsed = true;
                                AbundDenom = 1d;
                                AbundSuffix = 1d;
                                {
                                    var withBlock5 = ElementStats[MasterElementIndex];
                                    var loopTo5 = IsotopeCount;
                                    for (IsotopeIndex = 1; IsotopeIndex <= loopTo5; IsotopeIndex++)
                                    {
                                        IsotopeCountInThisCombo = IsoCombos[ComboIndex, IsotopeIndex];
                                        if (IsotopeCountInThisCombo > 0)
                                        {
                                            AbundDenom *= Factorial((short)IsotopeCountInThisCombo);
                                            AbundSuffix *= Math.Pow(withBlock5.Isotopes[IsotopeIndex].Abundance, IsotopeCountInThisCombo);
                                        }
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
                                    dblLogSigma = 0d;
                                    var loopTo6 = AtomCount;
                                    for (sigma = 1; sigma <= loopTo6; sigma++)
                                        dblLogSigma += Math.Log(sigma);
                                    dblSumI = 0d;
                                    var loopTo7 = IsotopeCount;
                                    for (IsotopeIndex = 1; IsotopeIndex <= loopTo7; IsotopeIndex++)
                                    {
                                        if (IsoCombos[ComboIndex, IsotopeIndex] > 0)
                                        {
                                            dblWorkingSum = 0d;
                                            var loopTo8 = IsoCombos[ComboIndex, IsotopeIndex];
                                            for (SubIndex = 1; SubIndex <= loopTo8; SubIndex++)
                                                dblWorkingSum += Math.Log(SubIndex);
                                            dblSumI += dblWorkingSum;
                                        }
                                    }

                                    {
                                        var withBlock6 = ElementStats[MasterElementIndex];
                                        dblSumF = 0d;
                                        var loopTo9 = IsotopeCount;
                                        for (IsotopeIndex = 1; IsotopeIndex <= loopTo9; IsotopeIndex++)
                                        {
                                            if (withBlock6.Isotopes[IsotopeIndex].Abundance > 0f)
                                            {
                                                dblSumF += IsoCombos[ComboIndex, IsotopeIndex] * Math.Log(withBlock6.Isotopes[IsotopeIndex].Abundance);
                                            }
                                        }
                                    }

                                    dblLogFreq = dblLogSigma - dblSumI + dblSumF;
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
                                    dblRatioOfFreqs = 1d;
                                    var loopTo10 = IsotopeCount;
                                    for (IsotopeIndex = 1; IsotopeIndex <= loopTo10; IsotopeIndex++)
                                    {
                                        intM = IsoCombos[ComboIndex, IsotopeIndex];
                                        intMPrime = IsoCombos[ComboIndex + 1, IsotopeIndex];
                                        if (intM > intMPrime)
                                        {
                                            dblLogSigma = 0d;
                                            var loopTo11 = (int)Math.Round(intM);
                                            for (SubIndex = (int)Math.Round(intMPrime) + 1; SubIndex <= loopTo11; SubIndex++)
                                                dblLogSigma += Math.Log(SubIndex);
                                            {
                                                var withBlock7 = ElementStats[MasterElementIndex];
                                                dblLogRho = dblLogSigma - (intM - intMPrime) * Math.Log(withBlock7.Isotopes[IsotopeIndex].Abundance);
                                            }
                                        }
                                        else if (intM < intMPrime)
                                        {
                                            dblLogSigma = 0d;
                                            var loopTo12 = (int)Math.Round(intMPrime);
                                            for (SubIndex = (int)Math.Round(intM) + 1; SubIndex <= loopTo12; SubIndex++)
                                                dblLogSigma += Math.Log(SubIndex);
                                            {
                                                var withBlock8 = ElementStats[MasterElementIndex];
                                                if (withBlock8.Isotopes[IsotopeIndex].Abundance > 0f)
                                                {
                                                    dblLogRho = (intMPrime - intM) * Math.Log(withBlock8.Isotopes[IsotopeIndex].Abundance) - dblLogSigma;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // intM = intMPrime
                                            dblLogRho = 0d;
                                        }

                                        dblRho = Math.Exp(dblLogRho);
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
                                {
                                    var withBlock9 = IsoStats[intElementIndex];
                                    withBlock9.MassAbundances[IndexToStoreAbundance] = (float)(withBlock9.MassAbundances[IndexToStoreAbundance] + dblThisComboFractionalAbundance);
                                }
                            }

                            if (blnRatioMethodUsed)
                            {
                                // Store abundance for next Combo
                                IndexToStoreAbundance = FindIndexForNominalMass(ref IsoCombos, ComboIndex + 1, IsotopeCount, AtomCount, ref ElementStats[MasterElementIndex].Isotopes);

                                // Store the abundance in .MassAbundances() at location IndexToStoreAbundance
                                {
                                    var withBlock10 = IsoStats[intElementIndex];
                                    withBlock10.MassAbundances[IndexToStoreAbundance] = (float)(withBlock10.MassAbundances[IndexToStoreAbundance] + dblNextComboFractionalAbundance);
                                }
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
                var loopTo13 = intElementCount;
                for (intElementIndex = 1; intElementIndex <= loopTo13; intElementIndex++)
                {
                    {
                        var withBlock11 = IsoStats[intElementIndex];
                        rowIndex = withBlock11.ResultsCount;
                        while (withBlock11.MassAbundances[rowIndex] < MIN_ABUNDANCE_TO_KEEP)
                        {
                            rowIndex -= 1;
                            if (rowIndex == 1)
                                break;
                        }

                        withBlock11.ResultsCount = rowIndex;
                    }
                }

                // Examine IsoStats() to predict the number of ConvolutionIterations
                PredictedConvIterations = IsoStats[1].ResultsCount;
                var loopTo14 = intElementCount;
                for (intElementIndex = 2; intElementIndex <= loopTo14; intElementIndex++)
                    PredictedConvIterations *= IsoStats[2].ResultsCount;
                ResetProgress("Finding Isotopic Abundances: Convoluting results");

                // Convolute the results for each element using a recursive convolution routine
                long ConvolutionIterations;
                ConvolutionIterations = 0L;
                var loopTo15 = IsoStats[1].ResultsCount;
                for (rowIndex = 1; rowIndex <= loopTo15; rowIndex++)
                {
                    ConvoluteMasses(ref ConvolutedAbundances, ConvolutedAbundanceStartMass, rowIndex, 1f, 0, 1, ref IsoStats, intElementCount, ref ConvolutionIterations);
                    sngPercentComplete = rowIndex / (float)IsoStats[1].ResultsCount * 100f;
                    UpdateProgress(sngPercentComplete);
                }

                if (mAbortProcessing)
                {
                    // Process Aborted
                    strResults = LookupMessage(940);
                    return -1;
                }

                // Compute mass defect (difference of initial isotope's mass from integer mass)
                dblExactBaseIsoMass = 0d;
                var loopTo16 = intElementCount;
                for (intElementIndex = 1; intElementIndex <= loopTo16; intElementIndex++)
                {
                    {
                        var withBlock12 = IsoStats[intElementIndex];
                        if (withBlock12.boolExplicitIsotope)
                        {
                            dblExactBaseIsoMass += withBlock12.AtomCount * withBlock12.ExplicitMass;
                        }
                        else
                        {
                            dblExactBaseIsoMass += withBlock12.AtomCount * ElementStats[withBlock12.ElementIndex].Isotopes[1].Mass;
                        }
                    }
                }

                dblMassDefect = Math.Round(dblExactBaseIsoMass - ConvolutedAbundanceStartMass, 5);

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

                dblMaxPercentDifference = Math.Pow(10d, -(3d - Math.Round(Math.Log10(dblTemp), 0)));
                if (Math.Abs(dblMassDefect / dblExactBaseIsoMass) >= dblMaxPercentDifference)
                {
                    Console.WriteLine("dblMassDefect / dblExactBaseIsoMass is greater dblMaxPercentDifference: (" + dblMassDefect / dblExactBaseIsoMass + " vs. " + dblMaxPercentDifference + "); this is unexpected");
                }

                // Step Through ConvolutedAbundances(), starting at the end, and find the first value above MIN_ABUNDANCE_TO_KEEP
                // Decrease ConvolutedMSDataCount to remove the extra values below MIN_ABUNDANCE_TO_KEEP
                for (massIndex = ConvolutedMSDataCount; massIndex >= 1; massIndex -= 1)
                {
                    if (ConvolutedAbundances[massIndex].Abundance > MIN_ABUNDANCE_TO_KEEP)
                    {
                        ConvolutedMSDataCount = massIndex;
                        break;
                    }
                }

                strOutput = strHeaderIsotopicAbundances + " " + strFormula + ControlChars.NewLine;
                strOutput = strOutput + SpacePad("  " + strHeaderMassToCharge, 12) + Constants.vbTab + SpacePad(strHeaderFraction, 9) + Constants.vbTab + strHeaderIntensity + ControlChars.NewLine;

                // Initialize ConvolutedMSData2DOneBased()
                ConvolutedMSData2DOneBased = new double[ConvolutedMSDataCount + 1, 3];

                // Find Maximum Abundance
                double dblMaxAbundance;
                dblMaxAbundance = 0d;
                var loopTo17 = ConvolutedMSDataCount;
                for (massIndex = 1; massIndex <= loopTo17; massIndex++)
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
                var loopTo18 = ConvolutedMSDataCount;
                for (massIndex = 1; massIndex <= loopTo18; massIndex++)
                {
                    {
                        var withBlock13 = ConvolutedAbundances[massIndex];
                        ConvolutedMSData2DOneBased[massIndex, 0] = ConvolutedAbundanceStartMass + massIndex - 1 + dblMassDefect;
                        ConvolutedMSData2DOneBased[massIndex, 1] = withBlock13.Abundance / dblMaxAbundance * 100d;
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
                }

                // Step through ConvolutedMSData2DOneBased() from the beginning to find the
                // first value greater than MIN_ABUNDANCE_TO_KEEP
                rowIndex = 1;
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
                    var loopTo19 = ConvolutedMSDataCount;
                    for (massIndex = rowIndex + 1; massIndex <= loopTo19; massIndex++)
                    {
                        ConvolutedMSData2DOneBased[massIndex - rowIndex, 0] = ConvolutedMSData2DOneBased[massIndex, 0];
                        ConvolutedMSData2DOneBased[massIndex - rowIndex, 1] = ConvolutedMSData2DOneBased[massIndex, 1];
                    }

                    ConvolutedMSDataCount -= rowIndex;
                }

                // Write to strOutput
                var loopTo20 = ConvolutedMSDataCount;
                for (massIndex = 1; massIndex <= loopTo20; massIndex++)
                {
                    strOutput = strOutput + SpacePadFront(ConvolutedMSData2DOneBased[massIndex, 0].ToString("#0.00000"), 12) + Constants.vbTab;
                    strOutput = strOutput + (ConvolutedMSData2DOneBased[massIndex, 1] * dblMaxAbundance / 100d).ToString("0.0000000") + Constants.vbTab;
                    strOutput = strOutput + SpacePadFront(ConvolutedMSData2DOneBased[massIndex, 1].ToString("##0.00"), 7) + ControlChars.NewLine;
                    //ToDo: Fix Multiplicity
                    //strOutput = strOutput + ConvolutedAbundances(massIndex).Multiplicity.ToString("##0") + ControlChars.NewLine
                }

                strResults = strOutput;
            }
            catch (Exception ex)
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
            short intElementIndex;
            double dblElementTotalMass;
            double dblPercentComp, dblStdDeviation;
            // Determine the number of elements in the formula

            for (intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
            {
                if (udtComputationStats.TotalMass > 0d)
                {
                    dblElementTotalMass = udtComputationStats.Elements[intElementIndex].Count * ElementStats[intElementIndex].Mass + udtComputationStats.Elements[intElementIndex].IsotopicCorrection;

                    // Percent is the percent composition
                    dblPercentComp = dblElementTotalMass / udtComputationStats.TotalMass * 100.0d;
                    udtComputationStats.PercentCompositions[intElementIndex].PercentComposition = dblPercentComp;


                    // Calculate standard deviation
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
            int intDataIndex;
            int intMidPointIndex;
            int intStickIndex;
            double DeltaX;
            double dblXValRange;
            double dblXValWindowRange;
            double dblRangeWork;
            double dblMinimalXValOfWindow;
            double dblMinimalXValSpacing;
            bool blnSearchForMinimumXVal;
            double dblXOffSet;
            double dblSigma;
            List<udtXYDataType> lstXYSummation;
            int intSummationIndex;
            int intMinimalSummationIndex;
            List<udtXYDataType> lstDataToAdd;
            int intDataToAddCount;
            bool blnAppendNewData;
            var udtThisDataPoint = new udtXYDataType();
            var lstGaussianData = new List<KeyValuePair<double, double>>();
            try
            {
                if (XYVals is null || XYVals.Count == 0)
                {
                    return lstGaussianData;
                }

                lstXYSummation = new List<udtXYDataType>(XYVals.Count * 10);

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
                DeltaX = dblResolutionMass / intResolution / intQualityFactor;

                // Make sure DeltaX is a reasonable number
                DeltaX = RoundToMultipleOf10(DeltaX);
                if (Math.Abs(DeltaX) < float.Epsilon)
                    DeltaX = 1d;

                // Set the Window Range to 1/10 the magnitude of the midpoint x value
                dblRangeWork = XYVals.First().Key + dblXValRange / 2d;
                dblRangeWork = RoundToMultipleOf10(dblRangeWork);
                dblSigma = dblResolutionMass / intResolution / Math.Sqrt(5.54d);

                // Set the window range (the xValue window width range) to calculate the Gaussian representation for each data point
                // The width at the base of a peak is 4 dblSigma
                // Use a width of 2 * 6 dblSigma
                dblXValWindowRange = 2 * 6 * dblSigma;
                if (dblXValRange / DeltaX > MAX_DATA_POINTS)
                {
                    // Delta x is too small; change to a reasonable value
                    // This isn't a bug, but it may mean one of the default settings is inappropriate
                    DeltaX = dblXValRange / MAX_DATA_POINTS;
                }

                intDataToAddCount = (int)Math.Round(dblXValWindowRange / DeltaX);

                // Make sure intDataToAddCount is odd
                if (intDataToAddCount % 2 == 0)
                {
                    intDataToAddCount += 1;
                }

                lstDataToAdd = new List<udtXYDataType>(intDataToAddCount);
                intMidPointIndex = (int)Math.Round((intDataToAddCount + 1) / 2d - 1d);

                // Compute the Gaussian data for each point in dblXVals()

                var loopTo = XYVals.Count - 1;
                for (intStickIndex = 0; intStickIndex <= loopTo; intStickIndex++)
                {
                    if (intStickIndex % 25 == 0)
                    {
                        if (AbortProcessing)
                            break;
                    }

                    // Search through lstXYSummation to determine the index of the smallest XValue with which
                    // data in lstDataToAdd could be combined
                    intMinimalSummationIndex = 0;
                    lstDataToAdd.Clear();
                    dblMinimalXValOfWindow = XYVals[intStickIndex].Key - intMidPointIndex * DeltaX;
                    blnSearchForMinimumXVal = true;
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
                            var loopTo1 = lstXYSummation.Count - 1;
                            for (intSummationIndex = 0; intSummationIndex <= loopTo1; intSummationIndex++)
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
                    var loopTo2 = intDataToAddCount - 1;
                    for (intDataIndex = 0; intDataIndex <= loopTo2; intDataIndex++)
                    {
                        // Equation for Gaussian is: Amplitude * Exp[ -(x - mu)^2 / (2*dblSigma^2) ]
                        // Use intDataIndex, .YVal, and DeltaX
                        dblXOffSet = (intMidPointIndex - intDataIndex) * DeltaX;
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

                    intDataIndex = 0;

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
                        var loopTo3 = lstXYSummation.Count - 1;
                        for (intSummationIndex = intMinimalSummationIndex; intSummationIndex <= loopTo3; intSummationIndex++)
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
                dblMinimalXValSpacing = dblXValRange / 100d;
                intSummationIndex = 0;
                while (intSummationIndex < lstXYSummation.Count - 1)
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

                    intSummationIndex += 1;
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
            short intIndex;
            bool blnIncludeAmino;

            // MasterSymbolsList(,0) contains the symbol to be matched
            // MasterSymbolsList(,1) contains E for element, A for amino acid, or N for normal abbreviation, followed by
            // the reference number in the master list
            // For example for Carbon, MasterSymbolsList(intIndex,0) = "C" and MasterSymbolsList(intIndex,1) = "E6"

            // Construct search list
            for (intIndex = 1; intIndex <= ELEMENT_COUNT; intIndex++)
            {
                MasterSymbolsList[intIndex, 0] = ElementStats[intIndex].Symbol;
                MasterSymbolsList[intIndex, 1] = "E" + Strings.Trim(Conversion.Str(intIndex));
            }

            MasterSymbolsListCount = ELEMENT_COUNT;

            // Note: AbbrevStats is 1-based
            if (gComputationOptions.AbbrevRecognitionMode != MolecularWeightTool.arAbbrevRecognitionModeConstants.arNoAbbreviations)
            {
                if (gComputationOptions.AbbrevRecognitionMode == MolecularWeightTool.arAbbrevRecognitionModeConstants.arNormalPlusAminoAcids)
                {
                    blnIncludeAmino = true;
                }
                else
                {
                    blnIncludeAmino = false;
                }

                var loopTo = AbbrevAllCount;
                for (intIndex = 1; intIndex <= loopTo; intIndex++)
                {
                    {
                        var withBlock = AbbrevStats[intIndex];
                        // If blnIncludeAmino = False then do not include amino acids
                        if (blnIncludeAmino || !blnIncludeAmino && !withBlock.IsAminoAcid)
                        {
                            // Do not include if the formula is invalid
                            if (!withBlock.InvalidSymbolOrFormula)
                            {
                                MasterSymbolsListCount = (short)(MasterSymbolsListCount + 1);
                                MasterSymbolsList[MasterSymbolsListCount, 0] = withBlock.Symbol;
                                MasterSymbolsList[MasterSymbolsListCount, 1] = "A" + Strings.Trim(Conversion.Str(intIndex));
                            }
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
        public double ConvoluteMassInternal(double dblMassMZ, short intCurrentCharge, short intDesiredCharge, double dblChargeCarrierMass)
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
                }
                // Return M+H, which is currently stored in dblNewMZ
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
            string strEmpiricalFormula;
            short intElementIndex, intElementSearchIndex;
            var intElementIndexToUse = default(short);

            // Call ParseFormulaPublic to compute the formula's mass and fill udtComputationStats
            double dblMass = ParseFormulaPublic(ref strFormula, ref udtComputationStats);
            if (ErrorParams.ErrorID == 0)
            {
                // Convert to empirical formula
                strEmpiricalFormula = "";
                // Carbon first, then hydrogen, then the rest alphabetically
                // This is correct to start at -1
                for (intElementIndex = -1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                {
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

                        for (intElementSearchIndex = 2; intElementSearchIndex <= ELEMENT_COUNT; intElementSearchIndex++) // Start at 2 to since we've already done hydrogen
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
                    {
                        var withBlock = mComputationStatsSaved;
                        double dblThisElementCount = withBlock.Elements[intElementIndexToUse].Count;
                        if (Math.Abs(dblThisElementCount - 1.0d) < float.Epsilon)
                        {
                            strEmpiricalFormula += ElementStats[intElementIndexToUse].Symbol;
                        }
                        else if (dblThisElementCount > 0d)
                        {
                            strEmpiricalFormula = strEmpiricalFormula + ElementStats[intElementIndexToUse].Symbol + Strings.Trim(dblThisElementCount.ToString());
                        }
                    }
                }

                return strEmpiricalFormula;
            }
            else
            {
                return (-1).ToString();
            }
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
            double dblMass = ParseFormulaPublic(ref strFormula, ref udtComputationStats, true);
            if (ErrorParams.ErrorID == 0)
            {
                return strFormula;
            }
            else
            {
                return (-1).ToString();
            }
        }

        private int FindIndexForNominalMass(ref int[,] IsoCombos, int ComboIndex, short IsotopeCount, int AtomCount, ref udtIsotopeInfoType[] ThisElementsIsotopes)
        {
            int workingMass = 0;
            for (int IsotopeIndex = 1, loopTo = IsotopeCount; IsotopeIndex <= loopTo; IsotopeIndex++)
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
        private void ConvoluteMasses(ref udtIsoResultsOverallType[] ConvolutedAbundances, int ConvolutedAbundanceStartMass, int WorkingRow, float WorkingAbundance, int WorkingMassTotal, short ElementTrack, ref udtIsoResultsByElementType[] IsoStats, short ElementCount, ref long Iterations)
        {
            int IndexToStoreResult, RowIndex;
            float NewAbundance;
            int NewMassTotal;
            if (mAbortProcessing)
                return;
            Iterations += 1L;
            if (Iterations % 10000L == 0L)
            {
                Application.DoEvents();
            }

            NewAbundance = WorkingAbundance * IsoStats[ElementTrack].MassAbundances[WorkingRow];
            NewMassTotal = WorkingMassTotal + (IsoStats[ElementTrack].StartingResultsMass + WorkingRow - 1);
            if (ElementTrack >= ElementCount)
            {
                IndexToStoreResult = NewMassTotal - ConvolutedAbundanceStartMass + 1;
                {
                    var withBlock = ConvolutedAbundances[IndexToStoreResult];
                    if (NewAbundance > 0f)
                    {
                        withBlock.Abundance = withBlock.Abundance + NewAbundance;
                        withBlock.Multiplicity = withBlock.Multiplicity + 1;
                    }
                }
            }
            else
            {
                var loopTo = IsoStats[ElementTrack + 1].ResultsCount;
                for (RowIndex = 1; RowIndex <= loopTo; RowIndex++)
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
                else
                {
                    return Number * Factorial((short)(Number - 1));
                }
            }
            catch (Exception ex)
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
        //        strMessage = "Too many combinations necessary for prediction of isotopic distribution: " & PredictedCombos.ToString("#,##0") & ControlChars.NewLine & "Please use a simpler formula or reduce the isotopic range defined for the element (currently " & IsotopeCount & ")"
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
        //                strHeader = strHeader & ControlChars.NewLine & "Only displaying the first 5000 combinations"
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

            short IsotopeIndex;
            int AtomIndex;
            int PredictedCombos;
            int[] RunningSum;
            int PreviousComputedValue;
            RunningSum = new int[AtomCount + 1];
            try
            {
                if (AtomCount == 1 || IsotopeCount == 1)
                {
                    PredictedCombos = IsotopeCount;
                }
                else
                {
                    // Initialize RunningSum()
                    var loopTo = AtomCount;
                    for (AtomIndex = 1; AtomIndex <= loopTo; AtomIndex++)
                        RunningSum[AtomIndex] = AtomIndex + 1;
                    var loopTo1 = IsotopeCount;
                    for (IsotopeIndex = 3; IsotopeIndex <= loopTo1; IsotopeIndex++)
                    {
                        PreviousComputedValue = IsotopeIndex;
                        var loopTo2 = AtomCount;
                        for (AtomIndex = 2; AtomIndex <= loopTo2; AtomIndex++)
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
            catch (Exception ex)
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
        private int FindCombosRecurse(ref int[,] ComboResults, int AtomCount, short MaxIsotopeCount, short CurrentIsotopeCount, ref int CurrentRow, short CurrentCol, ref int[] AtomTrackHistory)
        {
            int FindCombosRecurseRet = default;

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

            short ColIndex;
            int AtomTrack;
            short intNewColumn;
            if (CurrentIsotopeCount == 1 || AtomCount == 0)
            {
                // End recursion
                ComboResults[CurrentRow, CurrentCol] = AtomCount;
            }
            else
            {
                AtomTrack = AtomCount;

                // Store AtomTrack value at current position
                ComboResults[CurrentRow, CurrentCol] = AtomTrack;
                while (AtomTrack > 0)
                {
                    CurrentRow += 1;

                    // Went to a new row; if CurrentCol > 1 then need to assign previous values to previous columns
                    if (CurrentCol > 1)
                    {
                        var loopTo = (short)(CurrentCol - 1);
                        for (ColIndex = 1; ColIndex <= loopTo; ColIndex++)
                            ComboResults[CurrentRow, ColIndex] = AtomTrackHistory[ColIndex];
                    }

                    AtomTrack -= 1;
                    ComboResults[CurrentRow, CurrentCol] = AtomTrack;
                    if (CurrentCol < MaxIsotopeCount)
                    {
                        intNewColumn = (short)(CurrentCol + 1);
                        AtomTrackHistory[intNewColumn - 1] = AtomTrack;
                        FindCombosRecurse(ref ComboResults, AtomCount - AtomTrack, MaxIsotopeCount, (short)(CurrentIsotopeCount - 1), ref CurrentRow, intNewColumn, ref AtomTrackHistory);
                    }
                    else
                    {
                        Console.WriteLine("Program bug in FindCombosRecurse. This line should not be reached.");
                    }
                }

                // Reached AtomTrack = 0; end recursion
            }

            FindCombosRecurseRet = CurrentRow;
            return FindCombosRecurseRet;
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
            string strMessage;
            string strErrorFilePath;
            strMessage = "Error in " + strCallingProcedure + ": " + Conversion.ErrorToString(errorNumber) + " (#" + Strings.Trim(errorNumber.ToString()) + ")";
            if (strErrorDescriptionAdditional is object && strErrorDescriptionAdditional.Length > 0)
            {
                strMessage += ControlChars.NewLine + strErrorDescriptionAdditional;
            }

            LogMessage(strMessage, eMessageTypeConstants.ErrorMsg);
            if (mShowErrorMessageDialogs)
            {
                Interaction.MsgBox(strMessage, MsgBoxStyle.Exclamation, "Error in MwtWinDll");
            }
            else
            {
                Console.WriteLine(strMessage);
            }

            LogMessage(strMessage, eMessageTypeConstants.ErrorMsg);
            try
            {
                strErrorFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "ErrorLog.txt");

                // Open the file and append a new error entry
                using (var srOutFile = new System.IO.StreamWriter(strErrorFilePath, true))
                {
                    srOutFile.WriteLine(DateTime.Now.ToString() + " -- " + strMessage + ControlChars.NewLine);
                }
            }
            catch (Exception ex)
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
            for (int index = 1, loopTo = AbbrevAllCount; index <= loopTo; index++)
            {
                if ((Strings.LCase(AbbrevStats[index].Symbol) ?? "") == (Strings.LCase(strSymbol) ?? ""))
                {
                    if (!blnAminoAcidsOnly || blnAminoAcidsOnly && AbbrevStats[index].IsAminoAcid)
                    {
                        return index;
                    }
                }
            }

            return 0;
        }

        public int GetAbbreviationInternal(int abbreviationID, out string strSymbol, out string strFormula, out float sngCharge, out bool blnIsAminoAcid)
        {
            string argstrOneLetterSymbol = "";
            string argstrComment = "";
            bool argblnInvalidSymbolOrFormula = false;
            return GetAbbreviationInternal(abbreviationID, out strSymbol, out strFormula, out sngCharge, out blnIsAminoAcid, out argstrOneLetterSymbol, out argstrComment, out argblnInvalidSymbolOrFormula);
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
        public int GetAbbreviationInternal(int abbreviationID, out string strSymbol, out string strFormula, out float sngCharge, out bool blnIsAminoAcid, out string strOneLetterSymbol, out string strComment, out bool blnInvalidSymbolOrFormula)
        {
            if (abbreviationID >= 1 && abbreviationID <= AbbrevAllCount)
            {
                {
                    var withBlock = AbbrevStats[abbreviationID];
                    strSymbol = withBlock.Symbol;
                    strFormula = withBlock.Formula;
                    sngCharge = withBlock.Charge;
                    blnIsAminoAcid = withBlock.IsAminoAcid;
                    strOneLetterSymbol = withBlock.OneLetterSymbol;
                    strComment = withBlock.Comment;
                    blnInvalidSymbolOrFormula = withBlock.InvalidSymbolOrFormula;
                }

                return 0;
            }
            else
            {
                strSymbol = string.Empty;
                strFormula = string.Empty;
                sngCharge = 0f;
                blnIsAminoAcid = false;
                strOneLetterSymbol = string.Empty;
                strComment = string.Empty;
                blnInvalidSymbolOrFormula = true;
                return 1;
            }
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
            else
            {
                return 0d;
            }
        }

        public string GetAminoAcidSymbolConversionInternal(string strSymbolToFind, bool bln1LetterTo3Letter)
        {
            // If bln1LetterTo3Letter = True, then converting 1 letter codes to 3 letter codes
            // Returns the symbol, if found
            // Otherwise, returns ""

            string strReturnSymbol, strCompareSymbol;
            strReturnSymbol = "";
            // Use AbbrevStats() array to lookup code
            for (int index = 1, loopTo = AbbrevAllCount; index <= loopTo; index++)
            {
                if (AbbrevStats[index].IsAminoAcid)
                {
                    if (bln1LetterTo3Letter)
                    {
                        strCompareSymbol = AbbrevStats[index].OneLetterSymbol;
                    }
                    else
                    {
                        strCompareSymbol = AbbrevStats[index].Symbol;
                    }

                    if ((Strings.LCase(strCompareSymbol) ?? "") == (Strings.LCase(strSymbolToFind) ?? ""))
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
            int GetCautionStatementCountInternalRet = default;
            GetCautionStatementCountInternalRet = CautionStatementCount;
            return GetCautionStatementCountInternalRet;
        }

        /// <summary>
        /// Get the caution statement ID for the given symbol combo
        /// </summary>
        /// <param name="strSymbolCombo"></param>
        /// <returns>Statement ID if found, otherwise -1</returns>
        public int GetCautionStatementIDInternal(string strSymbolCombo)
        {
            short intIndex;
            var loopTo = (short)CautionStatementCount;
            for (intIndex = 1; intIndex <= loopTo; intIndex++)
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
            else
            {
                strSymbolCombo = string.Empty;
                strCautionStatement = string.Empty;
                return 1;
            }
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
        public int GetElementInternal(short intElementID, out string strSymbol, out double dblMass, out double dblUncertainty, out float sngCharge, out short intIsotopeCount)
        {
            if (intElementID >= 1 && intElementID <= ELEMENT_COUNT)
            {
                strSymbol = ElementAlph[intElementID];
                {
                    var withBlock = ElementStats[intElementID];
                    strSymbol = withBlock.Symbol;
                    dblMass = withBlock.Mass;
                    dblUncertainty = withBlock.Uncertainty;
                    sngCharge = withBlock.Charge;
                    intIsotopeCount = withBlock.IsotopeCount;
                }

                return 0;
            }
            else
            {
                strSymbol = string.Empty;
                dblMass = 0d;
                dblUncertainty = 0d;
                sngCharge = 0f;
                intIsotopeCount = 0;
                return 1;
            }
        }

        /// <summary>
        /// Get the element ID for the given symbol
        /// </summary>
        /// <param name="strSymbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        public short GetElementIDInternal(string strSymbol)
        {
            short intIndex;
            for (intIndex = 1; intIndex <= ELEMENT_COUNT; intIndex++)
            {
                if (string.Equals(ElementStats[intIndex].Symbol, strSymbol, StringComparison.InvariantCultureIgnoreCase))
                {
                    return intIndex;
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
            short intIsotopeIndex;
            if (intElementID >= 1 && intElementID <= ELEMENT_COUNT)
            {
                {
                    var withBlock = ElementStats[intElementID];
                    intIsotopeCount = withBlock.IsotopeCount;
                    var loopTo = withBlock.IsotopeCount;
                    for (intIsotopeIndex = 1; intIsotopeIndex <= loopTo; intIsotopeIndex++)
                    {
                        dblIsotopeMasses[intIsotopeIndex] = withBlock.Isotopes[intIsotopeIndex].Mass;
                        sngIsotopeAbundances[intIsotopeIndex] = withBlock.Isotopes[intIsotopeIndex].Abundance;
                    }
                }

                return 0;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Get the current element mode
        /// </summary>
        /// <returns>
        /// emAverageMass  = 1
        /// emIsotopicMass = 2
        /// emIntegerMass  = 3
        /// </returns>
        public emElementModeConstants GetElementModeInternal()
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
            else
            {
                return "";
            }
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
        public double GetElementStatInternal(short intElementID, MolecularWeightTool.esElementStatsConstants eElementStat)
        {
            if (intElementID >= 1 && intElementID <= ELEMENT_COUNT)
            {
                switch (eElementStat)
                {
                    case MolecularWeightTool.esElementStatsConstants.esMass:
                        return ElementStats[intElementID].Mass;
                    case MolecularWeightTool.esElementStatsConstants.esCharge:
                        return ElementStats[intElementID].Charge;
                    case MolecularWeightTool.esElementStatsConstants.esUncertainty:
                        return ElementStats[intElementID].Uncertainty;
                    default:
                        return 0d;
                }
            }
            else
            {
                return 0d;
            }
        }

        public string GetErrorDescription()
        {
            if (ErrorParams.ErrorID != 0)
            {
                return LookupMessage(ErrorParams.ErrorID);
            }
            else
            {
                return "";
            }
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
            string strMessage;
            if (messageID > 0 && messageID <= MessageStatementCount)
            {
                strMessage = MessageStatements[messageID];

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
            else
            {
                return "";
            }
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
            short intIndex;
            bool blnFound;
            try
            {
                blnFound = false;
                var loopTo = (short)(udtAbbrevSymbolStack.Count - 1);
                for (intIndex = 0; intIndex <= loopTo; intIndex++)
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
            char chFirstChar;
            bool blnIsModSymbol;
            if (strTestChar.Length > 0)
            {
                chFirstChar = strTestChar[0];
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
            bool IsStringAllLettersRet = default;
            bool blnAllLetters;
            short intIndex;

            // Assume true until proven otherwise
            blnAllLetters = true;
            var loopTo = (short)Strings.Len(strTest);
            for (intIndex = 1; intIndex <= loopTo; intIndex++)
            {
                if (!char.IsLetter(Conversions.ToChar(Strings.Mid(strTest, intIndex, 1))))
                {
                    blnAllLetters = false;
                    break;
                }
            }

            IsStringAllLettersRet = blnAllLetters;
            return IsStringAllLettersRet;
        }

        public bool IsValidElementSymbol(string elementSymbol, bool caseSensitive = true)
        {
            if (caseSensitive)
            {
                var query = from item in ElementStats
                            where (item.Symbol ?? "") == (elementSymbol ?? "")
                            select item;
                return query.Any();
            }
            else
            {
                var query = from item in ElementStats
                            where (item.Symbol.ToLower() ?? "") == (elementSymbol.ToLower() ?? "")
                            select item;
                return query.Any();
            }
        }

        protected void LogMessage(string strMessage)
        {
            LogMessage(strMessage, eMessageTypeConstants.Normal);
        }

        protected void LogMessage(string strMessage, eMessageTypeConstants eMessageType)
        {
            // Note that CleanupFilePaths() will update mOutputFolderPath, which is used here if mLogFolderPath is blank
            // Thus, be sure to call CleanupFilePaths (or update mLogFolderPath) before the first call to LogMessage

            if (mLogFile is null && mLogMessagesToFile)
            {
                try
                {
                    mLogFilePath = System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    mLogFilePath += "_log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                    try
                    {
                        if (mLogFolderPath is null)
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
                    catch (Exception ex)
                    {
                        mLogFolderPath = string.Empty;
                    }

                    if (mLogFolderPath.Length > 0)
                    {
                        mLogFilePath = System.IO.Path.Combine(mLogFolderPath, mLogFilePath);
                    }

                    bool blnOpeningExistingFile = System.IO.File.Exists(mLogFilePath);
                    mLogFile = new System.IO.StreamWriter(new System.IO.FileStream(mLogFilePath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Read)) { AutoFlush = true };
                    if (!blnOpeningExistingFile)
                    {
                        mLogFile.WriteLine("Date" + ControlChars.Tab + "Type" + ControlChars.Tab + "Message");
                    }
                }
                catch (Exception ex)
                {
                    // Error creating the log file; set mLogMessagesToFile to false so we don't repeatedly try to create it
                    mLogMessagesToFile = false;
                }
            }

            string strMessageType;
            switch (eMessageType)
            {
                case eMessageTypeConstants.Normal:
                    strMessageType = "Normal";
                    break;
                case eMessageTypeConstants.ErrorMsg:
                    strMessageType = "Error";
                    break;
                case eMessageTypeConstants.Warning:
                    strMessageType = "Warning";
                    break;
                default:
                    strMessageType = "Unknown";
                    break;
            }

            if (mLogFile is null)
            {
                Console.WriteLine(strMessageType + ControlChars.Tab + strMessage);
            }
            else
            {
                mLogFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt") + ControlChars.Tab + strMessageType + ControlChars.Tab + strMessage);
            }
        }

        private string LookupCautionStatement(string strCompareText)
        {
            short intIndex;
            var loopTo = (short)CautionStatementCount;
            for (intIndex = 1; intIndex <= loopTo; intIndex++)
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
            string LookupMessageRet = default;
            string strMessage;
            if (MessageStatementCount == 0)
                MemoryLoadMessageStatements();

            // First assume we can't find the message number
            strMessage = "General unspecified error";

            // Now try to find it
            if (messageID < MESSAGE_STATEMENT_DIM_COUNT)
            {
                if (Strings.Len(MessageStatements[messageID]) > 0)
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
                if (gComputationOptions.CaseConversion == ccCaseConversionConstants.ccExactCase)
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

            LookupMessageRet = strMessage;
            return LookupMessageRet;
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
            else
            {
                return 0d;
            }
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
        public double MonoMassToMZInternal(double dblMonoisotopicMass, short intCharge, double dblChargeCarrierMass)
        {
            if (Math.Abs(dblChargeCarrierMass) < float.Epsilon)
                dblChargeCarrierMass = mChargeCarrierMass;

            // Call ConvoluteMass to convert to the desired charge state
            return ConvoluteMassInternal(dblMonoisotopicMass + dblChargeCarrierMass, 1, intCharge, dblChargeCarrierMass);
        }

        public void MemoryLoadAll(emElementModeConstants eElementMode)
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
            short intIndex;

            // Symbol                            Formula            1 letter abbreviation
            const short AminoAbbrevCount = 28;
            AbbrevAllCount = AminoAbbrevCount;
            var loopTo = AbbrevAllCount;
            for (intIndex = 1; intIndex <= loopTo; intIndex++)
                AbbrevStats[intIndex].IsAminoAcid = true;
            string argstrFormula = "C3H5NO";
            AddAbbreviationWork(1, "Ala", ref argstrFormula, 0f, true, "A", "Alanine");
            string argstrFormula1 = "C6H12N4O";
            AddAbbreviationWork(2, "Arg", ref argstrFormula1, 0f, true, "R", "Arginine, (unprotonated NH2)");
            string argstrFormula2 = "C4H6N2O2";
            AddAbbreviationWork(3, "Asn", ref argstrFormula2, 0f, true, "N", "Asparagine");
            string argstrFormula3 = "C4H5NO3";
            AddAbbreviationWork(4, "Asp", ref argstrFormula3, 0f, true, "D", "Aspartic acid (undissociated COOH)");
            string argstrFormula4 = "C3H5NOS";
            AddAbbreviationWork(5, "Cys", ref argstrFormula4, 0f, true, "C", "Cysteine (no disulfide link)");
            string argstrFormula5 = "C6H7NO5";
            AddAbbreviationWork(6, "Gla", ref argstrFormula5, 0f, true, "U", "gamma-Carboxyglutamate");
            string argstrFormula6 = "C5H8N2O2";
            AddAbbreviationWork(7, "Gln", ref argstrFormula6, 0f, true, "Q", "Glutamine");
            string argstrFormula7 = "C5H7NO3";
            AddAbbreviationWork(8, "Glu", ref argstrFormula7, 0f, true, "E", "Glutamic acid (undissociated COOH)");
            string argstrFormula8 = "C2H3NO";
            AddAbbreviationWork(9, "Gly", ref argstrFormula8, 0f, true, "G", "Glycine");
            string argstrFormula9 = "C6H7N3O";
            AddAbbreviationWork(10, "His", ref argstrFormula9, 0f, true, "H", "Histidine (unprotonated NH)");
            string argstrFormula10 = "C4H7NO2";
            AddAbbreviationWork(11, "Hse", ref argstrFormula10, 0f, true, "", "Homoserine");
            string argstrFormula11 = "C6H12N2O2";
            AddAbbreviationWork(12, "Hyl", ref argstrFormula11, 0f, true, "", "Hydroxylysine");
            string argstrFormula12 = "C5H7NO2";
            AddAbbreviationWork(13, "Hyp", ref argstrFormula12, 0f, true, "", "Hydroxyproline");
            string argstrFormula13 = "C6H11NO";
            AddAbbreviationWork(14, "Ile", ref argstrFormula13, 0f, true, "I", "Isoleucine");
            string argstrFormula14 = "C6H11NO";
            AddAbbreviationWork(15, "Leu", ref argstrFormula14, 0f, true, "L", "Leucine");
            string argstrFormula15 = "C6H12N2O";
            AddAbbreviationWork(16, "Lys", ref argstrFormula15, 0f, true, "K", "Lysine (unprotonated NH2)");
            string argstrFormula16 = "C5H9NOS";
            AddAbbreviationWork(17, "Met", ref argstrFormula16, 0f, true, "M", "Methionine");
            string argstrFormula17 = "C5H10N2O";
            AddAbbreviationWork(18, "Orn", ref argstrFormula17, 0f, true, "O", "Ornithine");
            string argstrFormula18 = "C9H9NO";
            AddAbbreviationWork(19, "Phe", ref argstrFormula18, 0f, true, "F", "Phenylalanine");
            string argstrFormula19 = "C5H7NO";
            AddAbbreviationWork(20, "Pro", ref argstrFormula19, 0f, true, "P", "Proline");
            string argstrFormula20 = "C5H5NO2";
            AddAbbreviationWork(21, "Pyr", ref argstrFormula20, 0f, true, "", "Pyroglutamic acid");
            string argstrFormula21 = "C3H5NO";
            AddAbbreviationWork(22, "Sar", ref argstrFormula21, 0f, true, "", "Sarcosine");
            string argstrFormula22 = "C3H5NO2";
            AddAbbreviationWork(23, "Ser", ref argstrFormula22, 0f, true, "S", "Serine");
            string argstrFormula23 = "C4H7NO2";
            AddAbbreviationWork(24, "Thr", ref argstrFormula23, 0f, true, "T", "Threonine");
            string argstrFormula24 = "C11H10N2O";
            AddAbbreviationWork(25, "Trp", ref argstrFormula24, 0f, true, "W", "Tryptophan");
            string argstrFormula25 = "C9H9NO2";
            AddAbbreviationWork(26, "Tyr", ref argstrFormula25, 0f, true, "Y", "Tyrosine");
            string argstrFormula26 = "C5H9NO";
            AddAbbreviationWork(27, "Val", ref argstrFormula26, 0f, true, "V", "Valine");
            string argstrFormula27 = "C6H12N2O";
            AddAbbreviationWork(28, "Xxx", ref argstrFormula27, 0f, true, "X", "Unknown");
            const short NormalAbbrevCount = 16;
            AbbrevAllCount += NormalAbbrevCount;
            var loopTo1 = AbbrevAllCount;
            for (intIndex = AminoAbbrevCount + 1; intIndex <= loopTo1; intIndex++)
                AbbrevStats[intIndex].IsAminoAcid = false;
            string argstrFormula28 = "C10H8N2";
            AddAbbreviationWork(AminoAbbrevCount + 1, "Bpy", ref argstrFormula28, 0f, false, "", "Bipyridine");
            string argstrFormula29 = "C4H9";
            AddAbbreviationWork(AminoAbbrevCount + 2, "Bu", ref argstrFormula29, 1f, false, "", "Butyl");
            string argstrFormula30 = "^2.014H";
            AddAbbreviationWork(AminoAbbrevCount + 3, "D", ref argstrFormula30, 1f, false, "", "Deuterium");
            string argstrFormula31 = "C2H8N2";
            AddAbbreviationWork(AminoAbbrevCount + 4, "En", ref argstrFormula31, 0f, false, "", "Ethylenediamine");
            string argstrFormula32 = "CH3CH2";
            AddAbbreviationWork(AminoAbbrevCount + 5, "Et", ref argstrFormula32, 1f, false, "", "Ethyl");
            string argstrFormula33 = "CH3";
            AddAbbreviationWork(AminoAbbrevCount + 6, "Me", ref argstrFormula33, 1f, false, "", "Methyl");
            string argstrFormula34 = "CH3SOO";
            AddAbbreviationWork(AminoAbbrevCount + 7, "Ms", ref argstrFormula34, -1, false, "", "Mesyl");
            string argstrFormula35 = "C2H3O2";
            AddAbbreviationWork(AminoAbbrevCount + 8, "Oac", ref argstrFormula35, -1, false, "", "Acetate");
            string argstrFormula36 = "OSO2CF3";
            AddAbbreviationWork(AminoAbbrevCount + 9, "Otf", ref argstrFormula36, -1, false, "", "Triflate");
            string argstrFormula37 = "C2O4";
            AddAbbreviationWork(AminoAbbrevCount + 10, "Ox", ref argstrFormula37, -2, false, "", "Oxalate");
            string argstrFormula38 = "C6H5";
            AddAbbreviationWork(AminoAbbrevCount + 11, "Ph", ref argstrFormula38, 1f, false, "", "Phenyl");
            string argstrFormula39 = "C12H8N2";
            AddAbbreviationWork(AminoAbbrevCount + 12, "Phen", ref argstrFormula39, 0f, false, "", "Phenanthroline");
            string argstrFormula40 = "C5H5N";
            AddAbbreviationWork(AminoAbbrevCount + 13, "Py", ref argstrFormula40, 0f, false, "", "Pyridine");
            string argstrFormula41 = "(C4H2N(C6H5C)C4H2N(C6H5C))2";
            AddAbbreviationWork(AminoAbbrevCount + 14, "Tpp", ref argstrFormula41, 0f, false, "", "Tetraphenylporphyrin");
            string argstrFormula42 = "CH3C6H4SOO";
            AddAbbreviationWork(AminoAbbrevCount + 15, "Ts", ref argstrFormula42, -1, false, "", "Tosyl");
            string argstrFormula43 = "H2NCONH2";
            AddAbbreviationWork(AminoAbbrevCount + 16, "Urea", ref argstrFormula43, 0f, false, "", "Urea");

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
            CautionStatementCount = 41;
            CautionStatements[1, 0] = "Bi";
            CautionStatements[1, 1] = "Bi means bismuth; BI means boron-iodine.  ";
            CautionStatements[2, 0] = "Bk";
            CautionStatements[2, 1] = "Bk means berkelium; BK means boron-potassium.  ";
            CautionStatements[3, 0] = "Bu";
            CautionStatements[3, 1] = "Bu means the butyl group; BU means boron-uranium.  ";
            CautionStatements[4, 0] = "Cd";
            CautionStatements[4, 1] = "Cd means cadmium; CD means carbon-deuterium.  ";
            CautionStatements[5, 0] = "Cf";
            CautionStatements[5, 1] = "Cf means californium; CF means carbon-fluorine.  ";
            CautionStatements[6, 0] = "Co";
            CautionStatements[6, 1] = "Co means cobalt; CO means carbon-oxygen.  ";
            CautionStatements[7, 0] = "Cs";
            CautionStatements[7, 1] = "Cs means cesium; CS means carbon-sulfur.  ";
            CautionStatements[8, 0] = "Cu";
            CautionStatements[8, 1] = "Cu means copper; CU means carbon-uranium.  ";
            CautionStatements[9, 0] = "Dy";
            CautionStatements[9, 1] = "Dy means dysprosium; DY means deuterium-yttrium.  ";
            CautionStatements[10, 0] = "Hf";
            CautionStatements[10, 1] = "Hf means hafnium; HF means hydrogen-fluorine.  ";
            CautionStatements[11, 0] = "Ho";
            CautionStatements[11, 1] = "Ho means holmium; HO means hydrogen-oxygen.  ";
            CautionStatements[12, 0] = "In";
            CautionStatements[12, 1] = "In means indium; IN means iodine-nitrogen.  ";
            CautionStatements[13, 0] = "Nb";
            CautionStatements[13, 1] = "Nb means niobium; NB means nitrogen-boron.  ";
            CautionStatements[14, 0] = "Nd";
            CautionStatements[14, 1] = "Nd means neodymium; ND means nitrogen-deuterium.  ";
            CautionStatements[15, 0] = "Ni";
            CautionStatements[15, 1] = "Ni means nickel; NI means nitrogen-iodine.  ";
            CautionStatements[16, 0] = "No";
            CautionStatements[16, 1] = "No means nobelium; NO means nitrogen-oxygen.  ";
            CautionStatements[17, 0] = "Np";
            CautionStatements[17, 1] = "Np means neptunium; NP means nitrogen-phosphorus.  ";
            CautionStatements[18, 0] = "Os";
            CautionStatements[18, 1] = "Os means osmium; OS means oxygen-sulfur.  ";
            CautionStatements[19, 0] = "Pd";
            CautionStatements[19, 1] = "Pd means palladium; PD means phosphorus-deuterium.  ";
            CautionStatements[20, 0] = "Ph";
            CautionStatements[20, 1] = "Ph means phenyl, PH means phosphorus-hydrogen.  ";
            CautionStatements[21, 0] = "Pu";
            CautionStatements[21, 1] = "Pu means plutonium; PU means phosphorus-uranium.  ";
            CautionStatements[22, 0] = "Py";
            CautionStatements[22, 1] = "Py means pyridine; PY means phosphorus-yttrium.  ";
            CautionStatements[23, 0] = "Sb";
            CautionStatements[23, 1] = "Sb means antimony; SB means sulfur-boron.  ";
            CautionStatements[24, 0] = "Sc";
            CautionStatements[24, 1] = "Sc means scandium; SC means sulfur-carbon.  ";
            CautionStatements[25, 0] = "Si";
            CautionStatements[25, 1] = "Si means silicon; SI means sulfur-iodine.  ";
            CautionStatements[26, 0] = "Sn";
            CautionStatements[26, 1] = "Sn means tin; SN means sulfur-nitrogen.  ";
            CautionStatements[27, 0] = "TI";
            CautionStatements[27, 1] = "TI means tritium-iodine, Ti means titanium.  ";
            CautionStatements[28, 0] = "Yb";
            CautionStatements[28, 1] = "Yb means ytterbium; YB means yttrium-boron.  ";
            CautionStatements[29, 0] = "BPY";
            CautionStatements[29, 1] = "BPY means boron-phosphorus-yttrium; Bpy means bipyridine.  ";
            CautionStatements[30, 0] = "BPy";
            CautionStatements[30, 1] = "BPy means boron-pyridine; Bpy means bipyridine.  ";
            CautionStatements[31, 0] = "Bpy";
            CautionStatements[31, 1] = "Bpy means bipyridine.  ";
            CautionStatements[32, 0] = "Cys";
            CautionStatements[32, 1] = "Cys means cysteine; CYS means carbon-yttrium-sulfur.  ";
            CautionStatements[33, 0] = "His";
            CautionStatements[33, 1] = "His means histidine; HIS means hydrogen-iodine-sulfur.  ";
            CautionStatements[34, 0] = "Hoh";
            CautionStatements[34, 1] = "HoH means holmium-hydrogen; HOH means hydrogen-oxygen-hydrogen (aka water).  ";
            CautionStatements[35, 0] = "Hyp";
            CautionStatements[35, 1] = "Hyp means hydroxyproline; HYP means hydrogen-yttrium-phosphorus.  ";
            CautionStatements[36, 0] = "OAc";
            CautionStatements[36, 1] = "OAc means oxygen-actinium; Oac means acetate.  ";
            CautionStatements[37, 0] = "Oac";
            CautionStatements[37, 1] = "Oac means acetate.  ";
            CautionStatements[38, 0] = "Pro";
            CautionStatements[38, 1] = "Pro means proline; PrO means praseodymium-oxygen.  ";
            CautionStatements[39, 0] = "PrO";
            CautionStatements[39, 1] = "Pro means proline; PrO means praseodymium-oxygen.  ";
            CautionStatements[40, 0] = "Val";
            CautionStatements[40, 1] = "Val means valine; VAl means vanadium-aluminum.  ";
            CautionStatements[41, 0] = "VAl";
            CautionStatements[41, 1] = "Val means valine; VAl means vanadium-aluminum.  ";
        }

        public void MemoryLoadElements(emElementModeConstants eElementMode)
        {
            MemoryLoadElements(eElementMode, 0, MolecularWeightTool.esElementStatsConstants.esMass);
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
        public void MemoryLoadElements(emElementModeConstants eElementMode, short intSpecificElement, MolecularWeightTool.esElementStatsConstants eSpecificStatToReset)
        {
            const double DEFAULT_CHARGE_CARRIER_MASS_AVG = 1.00739d;
            const double DEFAULT_CHARGE_CARRIER_MASS_MONOISO = 1.00727649d;

            // This array stores the element names
            string[] strElementNames;
            strElementNames = new string[104];

            // dblElemVals(intElementIndex,1) stores the element's weight
            // dblElemVals(intElementIndex,2) stores the element's uncertainty
            // dblElemVals(intElementIndex,3) stores the element's charge
            // Note: I could make this array of type udtElementStatsType, but the size of this sub would increase dramatically
            double[,] dblElemVals;
            dblElemVals = new double[104, 4];
            short intIndex, intElementIndex, intCompareIndex;
            string strSwap;

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
            if (eElementMode == emElementModeConstants.emAverageMass)
            {
                SetChargeCarrierMassInternal(DEFAULT_CHARGE_CARRIER_MASS_AVG);
            }
            else
            {
                SetChargeCarrierMassInternal(DEFAULT_CHARGE_CARRIER_MASS_MONOISO);
            }

            // Assigning element names,        Charges
            strElementNames[1] = "H";
            dblElemVals[1, 3] = 1d;
            strElementNames[2] = "He";
            dblElemVals[2, 3] = 0d;
            strElementNames[3] = "Li";
            dblElemVals[3, 3] = 1d;
            strElementNames[4] = "Be";
            dblElemVals[4, 3] = 2d;
            strElementNames[5] = "B";
            dblElemVals[5, 3] = 3d;
            strElementNames[6] = "C";
            dblElemVals[6, 3] = 4d;
            strElementNames[7] = "N";
            dblElemVals[7, 3] = -3;
            strElementNames[8] = "O";
            dblElemVals[8, 3] = -2;
            strElementNames[9] = "F";
            dblElemVals[9, 3] = -1;
            strElementNames[10] = "Ne";
            dblElemVals[10, 3] = 0d;
            strElementNames[11] = "Na";
            dblElemVals[11, 3] = 1d;
            strElementNames[12] = "Mg";
            dblElemVals[12, 3] = 2d;
            strElementNames[13] = "Al";
            dblElemVals[13, 3] = 3d;
            strElementNames[14] = "Si";
            dblElemVals[14, 3] = 4d;
            strElementNames[15] = "P";
            dblElemVals[15, 3] = -3;
            strElementNames[16] = "S";
            dblElemVals[16, 3] = -2;
            strElementNames[17] = "Cl";
            dblElemVals[17, 3] = -1;
            strElementNames[18] = "Ar";
            dblElemVals[18, 3] = 0d;
            strElementNames[19] = "K";
            dblElemVals[19, 3] = 1d;
            strElementNames[20] = "Ca";
            dblElemVals[20, 3] = 2d;
            strElementNames[21] = "Sc";
            dblElemVals[21, 3] = 3d;
            strElementNames[22] = "Ti";
            dblElemVals[22, 3] = 4d;
            strElementNames[23] = "V";
            dblElemVals[23, 3] = 5d;
            strElementNames[24] = "Cr";
            dblElemVals[24, 3] = 3d;
            strElementNames[25] = "Mn";
            dblElemVals[25, 3] = 2d;
            strElementNames[26] = "Fe";
            dblElemVals[26, 3] = 3d;
            strElementNames[27] = "Co";
            dblElemVals[27, 3] = 2d;
            strElementNames[28] = "Ni";
            dblElemVals[28, 3] = 2d;
            strElementNames[29] = "Cu";
            dblElemVals[29, 3] = 2d;
            strElementNames[30] = "Zn";
            dblElemVals[30, 3] = 2d;
            strElementNames[31] = "Ga";
            dblElemVals[31, 3] = 3d;
            strElementNames[32] = "Ge";
            dblElemVals[32, 3] = 4d;
            strElementNames[33] = "As";
            dblElemVals[33, 3] = -3;
            strElementNames[34] = "Se";
            dblElemVals[34, 3] = -2;
            strElementNames[35] = "Br";
            dblElemVals[35, 3] = -1;
            strElementNames[36] = "Kr";
            dblElemVals[36, 3] = 0d;
            strElementNames[37] = "Rb";
            dblElemVals[37, 3] = 1d;
            strElementNames[38] = "Sr";
            dblElemVals[38, 3] = 2d;
            strElementNames[39] = "Y";
            dblElemVals[39, 3] = 3d;
            strElementNames[40] = "Zr";
            dblElemVals[40, 3] = 4d;
            strElementNames[41] = "Nb";
            dblElemVals[41, 3] = 5d;
            strElementNames[42] = "Mo";
            dblElemVals[42, 3] = 6d;
            strElementNames[43] = "Tc";
            dblElemVals[43, 3] = 7d;
            strElementNames[44] = "Ru";
            dblElemVals[44, 3] = 4d;
            strElementNames[45] = "Rh";
            dblElemVals[45, 3] = 3d;
            strElementNames[46] = "Pd";
            dblElemVals[46, 3] = 2d;
            strElementNames[47] = "Ag";
            dblElemVals[47, 3] = 1d;
            strElementNames[48] = "Cd";
            dblElemVals[48, 3] = 2d;
            strElementNames[49] = "In";
            dblElemVals[49, 3] = 3d;
            strElementNames[50] = "Sn";
            dblElemVals[50, 3] = 4d;
            strElementNames[51] = "Sb";
            dblElemVals[51, 3] = -3;
            strElementNames[52] = "Te";
            dblElemVals[52, 3] = -2;
            strElementNames[53] = "I";
            dblElemVals[53, 3] = -1;
            strElementNames[54] = "Xe";
            dblElemVals[54, 3] = 0d;
            strElementNames[55] = "Cs";
            dblElemVals[55, 3] = 1d;
            strElementNames[56] = "Ba";
            dblElemVals[56, 3] = 2d;
            strElementNames[57] = "La";
            dblElemVals[57, 3] = 3d;
            strElementNames[58] = "Ce";
            dblElemVals[58, 3] = 3d;
            strElementNames[59] = "Pr";
            dblElemVals[59, 3] = 4d;
            strElementNames[60] = "Nd";
            dblElemVals[60, 3] = 3d;
            strElementNames[61] = "Pm";
            dblElemVals[61, 3] = 3d;
            strElementNames[62] = "Sm";
            dblElemVals[62, 3] = 3d;
            strElementNames[63] = "Eu";
            dblElemVals[63, 3] = 3d;
            strElementNames[64] = "Gd";
            dblElemVals[64, 3] = 3d;
            strElementNames[65] = "Tb";
            dblElemVals[65, 3] = 3d;
            strElementNames[66] = "Dy";
            dblElemVals[66, 3] = 3d;
            strElementNames[67] = "Ho";
            dblElemVals[67, 3] = 3d;
            strElementNames[68] = "Er";
            dblElemVals[68, 3] = 3d;
            strElementNames[69] = "Tm";
            dblElemVals[69, 3] = 3d;
            strElementNames[70] = "Yb";
            dblElemVals[70, 3] = 3d;
            strElementNames[71] = "Lu";
            dblElemVals[71, 3] = 3d;
            strElementNames[72] = "Hf";
            dblElemVals[72, 3] = 4d;
            strElementNames[73] = "Ta";
            dblElemVals[73, 3] = 5d;
            strElementNames[74] = "W";
            dblElemVals[74, 3] = 6d;
            strElementNames[75] = "Re";
            dblElemVals[75, 3] = 7d;
            strElementNames[76] = "Os";
            dblElemVals[76, 3] = 4d;
            strElementNames[77] = "Ir";
            dblElemVals[77, 3] = 4d;
            strElementNames[78] = "Pt";
            dblElemVals[78, 3] = 4d;
            strElementNames[79] = "Au";
            dblElemVals[79, 3] = 3d;
            strElementNames[80] = "Hg";
            dblElemVals[80, 3] = 2d;
            strElementNames[81] = "Tl";
            dblElemVals[81, 3] = 1d;
            strElementNames[82] = "Pb";
            dblElemVals[82, 3] = 2d;
            strElementNames[83] = "Bi";
            dblElemVals[83, 3] = 3d;
            strElementNames[84] = "Po";
            dblElemVals[84, 3] = 4d;
            strElementNames[85] = "At";
            dblElemVals[85, 3] = -1;
            strElementNames[86] = "Rn";
            dblElemVals[86, 3] = 0d;
            strElementNames[87] = "Fr";
            dblElemVals[87, 3] = 1d;
            strElementNames[88] = "Ra";
            dblElemVals[88, 3] = 2d;
            strElementNames[89] = "Ac";
            dblElemVals[89, 3] = 3d;
            strElementNames[90] = "Th";
            dblElemVals[90, 3] = 4d;
            strElementNames[91] = "Pa";
            dblElemVals[91, 3] = 5d;
            strElementNames[92] = "U";
            dblElemVals[92, 3] = 6d;
            strElementNames[93] = "Np";
            dblElemVals[93, 3] = 5d;
            strElementNames[94] = "Pu";
            dblElemVals[94, 3] = 4d;
            strElementNames[95] = "Am";
            dblElemVals[95, 3] = 3d;
            strElementNames[96] = "Cm";
            dblElemVals[96, 3] = 3d;
            strElementNames[97] = "Bk";
            dblElemVals[97, 3] = 3d;
            strElementNames[98] = "Cf";
            dblElemVals[98, 3] = 3d;
            strElementNames[99] = "Es";
            dblElemVals[99, 3] = 3d;
            strElementNames[100] = "Fm";
            dblElemVals[100, 3] = 3d;
            strElementNames[101] = "Md";
            dblElemVals[101, 3] = 3d;
            strElementNames[102] = "No";
            dblElemVals[102, 3] = 3d;
            strElementNames[103] = "Lr";
            dblElemVals[103, 3] = 3d;

            // Set uncertainty to 0 for all elements if using exact isotopic or integer isotopic weights
            if (eElementMode == emElementModeConstants.emIsotopicMass || eElementMode == emElementModeConstants.emIntegerMass)
            {
                for (intIndex = 1; intIndex <= ELEMENT_COUNT; intIndex++)
                    dblElemVals[intIndex, 2] = 0d;
            }

            switch (eElementMode)
            {
                case emElementModeConstants.emIntegerMass:
                    // Integer Isotopic Weights
                    dblElemVals[1, 1] = 1d;
                    dblElemVals[2, 1] = 4d;
                    dblElemVals[3, 1] = 7d;
                    dblElemVals[4, 1] = 9d;
                    dblElemVals[5, 1] = 11d;
                    dblElemVals[6, 1] = 12d;
                    dblElemVals[7, 1] = 14d;
                    dblElemVals[8, 1] = 16d;
                    dblElemVals[9, 1] = 19d;
                    dblElemVals[10, 1] = 20d;
                    dblElemVals[11, 1] = 23d;
                    dblElemVals[12, 1] = 24d;
                    dblElemVals[13, 1] = 27d;
                    dblElemVals[14, 1] = 28d;
                    dblElemVals[15, 1] = 31d;
                    dblElemVals[16, 1] = 32d;
                    dblElemVals[17, 1] = 35d;
                    dblElemVals[18, 1] = 40d;
                    dblElemVals[19, 1] = 39d;
                    dblElemVals[20, 1] = 40d;
                    dblElemVals[21, 1] = 45d;
                    dblElemVals[22, 1] = 48d;
                    dblElemVals[23, 1] = 51d;
                    dblElemVals[24, 1] = 52d;
                    dblElemVals[25, 1] = 55d;
                    dblElemVals[26, 1] = 56d;
                    dblElemVals[27, 1] = 59d;
                    dblElemVals[28, 1] = 58d;
                    dblElemVals[29, 1] = 63d;
                    dblElemVals[30, 1] = 64d;
                    dblElemVals[31, 1] = 69d;
                    dblElemVals[32, 1] = 72d;
                    dblElemVals[33, 1] = 75d;
                    dblElemVals[34, 1] = 80d;
                    dblElemVals[35, 1] = 79d;
                    dblElemVals[36, 1] = 84d;
                    dblElemVals[37, 1] = 85d;
                    dblElemVals[38, 1] = 88d;
                    dblElemVals[39, 1] = 89d;
                    dblElemVals[40, 1] = 90d;
                    dblElemVals[41, 1] = 93d;
                    dblElemVals[42, 1] = 98d;
                    dblElemVals[43, 1] = 98d;
                    dblElemVals[44, 1] = 102d;
                    dblElemVals[45, 1] = 103d;
                    dblElemVals[46, 1] = 106d;
                    dblElemVals[47, 1] = 107d;
                    dblElemVals[48, 1] = 114d;
                    dblElemVals[49, 1] = 115d;
                    dblElemVals[50, 1] = 120d;
                    dblElemVals[51, 1] = 121d;
                    dblElemVals[52, 1] = 130d;
                    dblElemVals[53, 1] = 127d;
                    dblElemVals[54, 1] = 132d;
                    dblElemVals[55, 1] = 133d;
                    dblElemVals[56, 1] = 138d;
                    dblElemVals[57, 1] = 139d;
                    dblElemVals[58, 1] = 140d;
                    dblElemVals[59, 1] = 141d;
                    dblElemVals[60, 1] = 142d;
                    dblElemVals[61, 1] = 145d;
                    dblElemVals[62, 1] = 152d;
                    dblElemVals[63, 1] = 153d;
                    dblElemVals[64, 1] = 158d;
                    dblElemVals[65, 1] = 159d;
                    dblElemVals[66, 1] = 164d;
                    dblElemVals[67, 1] = 165d;
                    dblElemVals[68, 1] = 166d;
                    dblElemVals[69, 1] = 169d;
                    dblElemVals[70, 1] = 174d;
                    dblElemVals[71, 1] = 175d;
                    dblElemVals[72, 1] = 180d;
                    dblElemVals[73, 1] = 181d;
                    dblElemVals[74, 1] = 184d;
                    dblElemVals[75, 1] = 187d;
                    dblElemVals[76, 1] = 192d;
                    dblElemVals[77, 1] = 193d;
                    dblElemVals[78, 1] = 195d;
                    dblElemVals[79, 1] = 197d;
                    dblElemVals[80, 1] = 202d;
                    dblElemVals[81, 1] = 205d;
                    dblElemVals[82, 1] = 208d;
                    dblElemVals[83, 1] = 209d;
                    dblElemVals[84, 1] = 209d;
                    dblElemVals[85, 1] = 210d;
                    dblElemVals[86, 1] = 222d;
                    dblElemVals[87, 1] = 223d;
                    dblElemVals[88, 1] = 227d;
                    dblElemVals[89, 1] = 227d;
                    dblElemVals[90, 1] = 232d;
                    dblElemVals[91, 1] = 231d;
                    dblElemVals[92, 1] = 238d;
                    dblElemVals[93, 1] = 237d;
                    dblElemVals[94, 1] = 244d;
                    dblElemVals[95, 1] = 243d;
                    dblElemVals[96, 1] = 247d;
                    dblElemVals[97, 1] = 247d;
                    dblElemVals[98, 1] = 251d;
                    dblElemVals[99, 1] = 252d;
                    dblElemVals[100, 1] = 257d;
                    dblElemVals[101, 1] = 258d;
                    dblElemVals[102, 1] = 269d;
                    dblElemVals[103, 1] = 260d;
                    break;

                // Unused elements
                // data 104,Unq,Unnilquadium,261.11,.05, 105,Unp,Unnilpentium,262.114,005, 106,Unh,Unnilhexium,263.118,.005, 107,Uns,Unnilseptium,262.12,.05

                case emElementModeConstants.emIsotopicMass:
                    // isotopic Element Weights
                    dblElemVals[1, 1] = 1.0078246d;
                    dblElemVals[2, 1] = 4.0026029d;
                    dblElemVals[3, 1] = 7.016005d;
                    dblElemVals[4, 1] = 9.012183d;
                    dblElemVals[5, 1] = 11.009305d;
                    dblElemVals[6, 1] = 12d;
                    dblElemVals[7, 1] = 14.003074d;
                    dblElemVals[8, 1] = 15.994915d;
                    dblElemVals[9, 1] = 18.9984032d;
                    dblElemVals[10, 1] = 19.992439d;
                    dblElemVals[11, 1] = 22.98977d;
                    dblElemVals[12, 1] = 23.98505d;
                    dblElemVals[13, 1] = 26.981541d;
                    dblElemVals[14, 1] = 27.976928d;
                    dblElemVals[15, 1] = 30.973763d;
                    dblElemVals[16, 1] = 31.972072d;
                    dblElemVals[17, 1] = 34.968853d;
                    dblElemVals[18, 1] = 39.962383d;
                    dblElemVals[19, 1] = 38.963708d;
                    dblElemVals[20, 1] = 39.962591d;
                    dblElemVals[21, 1] = 44.955914d;
                    dblElemVals[22, 1] = 47.947947d;
                    dblElemVals[23, 1] = 50.943963d;
                    dblElemVals[24, 1] = 51.94051d;
                    dblElemVals[25, 1] = 54.938046d;
                    dblElemVals[26, 1] = 55.934939d;
                    dblElemVals[27, 1] = 58.933198d;
                    dblElemVals[28, 1] = 57.935347d;
                    dblElemVals[29, 1] = 62.929599d;
                    dblElemVals[30, 1] = 63.929145d;
                    dblElemVals[31, 1] = 68.925581d;
                    dblElemVals[32, 1] = 71.92208d;
                    dblElemVals[33, 1] = 74.921596d;
                    dblElemVals[34, 1] = 79.916521d;
                    dblElemVals[35, 1] = 78.918336d;
                    dblElemVals[36, 1] = 83.911506d;
                    dblElemVals[37, 1] = 84.9118d;
                    dblElemVals[38, 1] = 87.905625d;
                    dblElemVals[39, 1] = 88.905856d;
                    dblElemVals[40, 1] = 89.904708d;
                    dblElemVals[41, 1] = 92.906378d;
                    dblElemVals[42, 1] = 97.905405d;
                    dblElemVals[43, 1] = 98d;
                    dblElemVals[44, 1] = 101.90434d;
                    dblElemVals[45, 1] = 102.905503d;
                    dblElemVals[46, 1] = 105.903475d;
                    dblElemVals[47, 1] = 106.905095d;
                    dblElemVals[48, 1] = 113.903361d;
                    dblElemVals[49, 1] = 114.903875d;
                    dblElemVals[50, 1] = 119.902199d;
                    dblElemVals[51, 1] = 120.903824d;
                    dblElemVals[52, 1] = 129.906229d;
                    dblElemVals[53, 1] = 126.904477d;
                    dblElemVals[54, 1] = 131.904148d;
                    dblElemVals[55, 1] = 132.905433d;
                    dblElemVals[56, 1] = 137.905236d;
                    dblElemVals[57, 1] = 138.906355d;
                    dblElemVals[58, 1] = 139.905442d;
                    dblElemVals[59, 1] = 140.907657d;
                    dblElemVals[60, 1] = 141.907731d;
                    dblElemVals[61, 1] = 145d;
                    dblElemVals[62, 1] = 151.919741d;
                    dblElemVals[63, 1] = 152.921243d;
                    dblElemVals[64, 1] = 157.924111d;
                    dblElemVals[65, 1] = 158.92535d;
                    dblElemVals[66, 1] = 163.929183d;
                    dblElemVals[67, 1] = 164.930332d;
                    dblElemVals[68, 1] = 165.930305d;
                    dblElemVals[69, 1] = 168.934225d;
                    dblElemVals[70, 1] = 173.938873d;
                    dblElemVals[71, 1] = 174.940785d;
                    dblElemVals[72, 1] = 179.946561d;
                    dblElemVals[73, 1] = 180.948014d;
                    dblElemVals[74, 1] = 183.950953d;
                    dblElemVals[75, 1] = 186.955765d;
                    dblElemVals[76, 1] = 191.960603d;
                    dblElemVals[77, 1] = 192.962942d;
                    dblElemVals[78, 1] = 194.964785d;
                    dblElemVals[79, 1] = 196.96656d;
                    dblElemVals[80, 1] = 201.970632d;
                    dblElemVals[81, 1] = 204.97441d;
                    dblElemVals[82, 1] = 207.976641d;
                    dblElemVals[83, 1] = 208.980388d;
                    dblElemVals[84, 1] = 209d;
                    dblElemVals[85, 1] = 210d;
                    dblElemVals[86, 1] = 222d;
                    dblElemVals[87, 1] = 223d;
                    dblElemVals[88, 1] = 227d;
                    dblElemVals[89, 1] = 227d;
                    dblElemVals[90, 1] = 232.038054d;
                    dblElemVals[91, 1] = 231d;
                    dblElemVals[92, 1] = 238.050786d;
                    dblElemVals[93, 1] = 237d;
                    dblElemVals[94, 1] = 244d;
                    dblElemVals[95, 1] = 243d;
                    dblElemVals[96, 1] = 247d;
                    dblElemVals[97, 1] = 247d;
                    dblElemVals[98, 1] = 251d;
                    dblElemVals[99, 1] = 252d;
                    dblElemVals[100, 1] = 257d;
                    dblElemVals[101, 1] = 258d;
                    dblElemVals[102, 1] = 269d;

                    // Unused elements
                    // data 104,Unq,Unnilquadium,261.11,.05, 105,Unp,Unnilpentium,262.114,005, 106,Unh,Unnilhexium,263.118,.005, 107,Uns,Unnilseptium,262.12,.05

                    dblElemVals[103, 1] = 260d;
                    break;

                default:
                    // Weight                           Uncertainty
                    // Average Element Weights
                    dblElemVals[1, 1] = 1.00794d;
                    dblElemVals[1, 2] = 0.00007d;
                    dblElemVals[2, 1] = 4.002602d;
                    dblElemVals[2, 2] = 0.000002d;
                    dblElemVals[3, 1] = 6.941d;
                    dblElemVals[3, 2] = 0.002d;
                    dblElemVals[4, 1] = 9.012182d;
                    dblElemVals[4, 2] = 0.000003d;
                    dblElemVals[5, 1] = 10.811d;
                    dblElemVals[5, 2] = 0.007d;
                    dblElemVals[6, 1] = 12.0107d;
                    dblElemVals[6, 2] = 0.0008d;
                    dblElemVals[7, 1] = 14.00674d;
                    dblElemVals[7, 2] = 0.00007d;
                    dblElemVals[8, 1] = 15.9994d;
                    dblElemVals[8, 2] = 0.0003d;
                    dblElemVals[9, 1] = 18.9984032d;
                    dblElemVals[9, 2] = 0.0000005d;
                    dblElemVals[10, 1] = 20.1797d;
                    dblElemVals[10, 2] = 0.0006d;
                    dblElemVals[11, 1] = 22.98977d;
                    dblElemVals[11, 2] = 0.000002d;
                    dblElemVals[12, 1] = 24.305d;
                    dblElemVals[12, 2] = 0.0006d;
                    dblElemVals[13, 1] = 26.981538d;
                    dblElemVals[13, 2] = 0.000002d;
                    dblElemVals[14, 1] = 28.0855d;
                    dblElemVals[14, 2] = 0.0003d;
                    dblElemVals[15, 1] = 30.973761d;
                    dblElemVals[15, 2] = 0.000002d;
                    dblElemVals[16, 1] = 32.066d;
                    dblElemVals[16, 2] = 0.006d;
                    dblElemVals[17, 1] = 35.4527d;
                    dblElemVals[17, 2] = 0.0009d;
                    dblElemVals[18, 1] = 39.948d;
                    dblElemVals[18, 2] = 0.001d;
                    dblElemVals[19, 1] = 39.0983d;
                    dblElemVals[19, 2] = 0.0001d;
                    dblElemVals[20, 1] = 40.078d;
                    dblElemVals[20, 2] = 0.004d;
                    dblElemVals[21, 1] = 44.95591d;
                    dblElemVals[21, 2] = 0.000008d;
                    dblElemVals[22, 1] = 47.867d;
                    dblElemVals[22, 2] = 0.001d;
                    dblElemVals[23, 1] = 50.9415d;
                    dblElemVals[23, 2] = 0.0001d;
                    dblElemVals[24, 1] = 51.9961d;
                    dblElemVals[24, 2] = 0.0006d;
                    dblElemVals[25, 1] = 54.938049d;
                    dblElemVals[25, 2] = 0.000009d;
                    dblElemVals[26, 1] = 55.845d;
                    dblElemVals[26, 2] = 0.002d;
                    dblElemVals[27, 1] = 58.9332d;
                    dblElemVals[27, 2] = 0.000009d;
                    dblElemVals[28, 1] = 58.6934d;
                    dblElemVals[28, 2] = 0.0002d;
                    dblElemVals[29, 1] = 63.546d;
                    dblElemVals[29, 2] = 0.003d;
                    dblElemVals[30, 1] = 65.39d;
                    dblElemVals[30, 2] = 0.02d;
                    dblElemVals[31, 1] = 69.723d;
                    dblElemVals[31, 2] = 0.001d;
                    dblElemVals[32, 1] = 72.61d;
                    dblElemVals[32, 2] = 0.02d;
                    dblElemVals[33, 1] = 74.9216d;
                    dblElemVals[33, 2] = 0.00002d;
                    dblElemVals[34, 1] = 78.96d;
                    dblElemVals[34, 2] = 0.03d;
                    dblElemVals[35, 1] = 79.904d;
                    dblElemVals[35, 2] = 0.001d;
                    dblElemVals[36, 1] = 83.8d;
                    dblElemVals[36, 2] = 0.01d;
                    dblElemVals[37, 1] = 85.4678d;
                    dblElemVals[37, 2] = 0.0003d;
                    dblElemVals[38, 1] = 87.62d;
                    dblElemVals[38, 2] = 0.01d;
                    dblElemVals[39, 1] = 88.90585d;
                    dblElemVals[39, 2] = 0.00002d;
                    dblElemVals[40, 1] = 91.224d;
                    dblElemVals[40, 2] = 0.002d;
                    dblElemVals[41, 1] = 92.90638d;
                    dblElemVals[41, 2] = 0.00002d;
                    dblElemVals[42, 1] = 95.94d;
                    dblElemVals[42, 2] = 0.01d;
                    dblElemVals[43, 1] = 97.9072d;
                    dblElemVals[43, 2] = 0.0005d;
                    dblElemVals[44, 1] = 101.07d;
                    dblElemVals[44, 2] = 0.02d;
                    dblElemVals[45, 1] = 102.9055d;
                    dblElemVals[45, 2] = 0.00002d;
                    dblElemVals[46, 1] = 106.42d;
                    dblElemVals[46, 2] = 0.01d;
                    dblElemVals[47, 1] = 107.8682d;
                    dblElemVals[47, 2] = 0.0002d;
                    dblElemVals[48, 1] = 112.411d;
                    dblElemVals[48, 2] = 0.008d;
                    dblElemVals[49, 1] = 114.818d;
                    dblElemVals[49, 2] = 0.003d;
                    dblElemVals[50, 1] = 118.71d;
                    dblElemVals[50, 2] = 0.007d;
                    dblElemVals[51, 1] = 121.76d;
                    dblElemVals[51, 2] = 0.001d;
                    dblElemVals[52, 1] = 127.6d;
                    dblElemVals[52, 2] = 0.03d;
                    dblElemVals[53, 1] = 126.90447d;
                    dblElemVals[53, 2] = 0.00003d;
                    dblElemVals[54, 1] = 131.29d;
                    dblElemVals[54, 2] = 0.02d;
                    dblElemVals[55, 1] = 132.90545d;
                    dblElemVals[55, 2] = 0.00002d;
                    dblElemVals[56, 1] = 137.327d;
                    dblElemVals[56, 2] = 0.007d;
                    dblElemVals[57, 1] = 138.9055d;
                    dblElemVals[57, 2] = 0.0002d;
                    dblElemVals[58, 1] = 140.116d;
                    dblElemVals[58, 2] = 0.001d;
                    dblElemVals[59, 1] = 140.90765d;
                    dblElemVals[59, 2] = 0.00002d;
                    dblElemVals[60, 1] = 144.24d;
                    dblElemVals[60, 2] = 0.03d;
                    dblElemVals[61, 1] = 144.9127d;
                    dblElemVals[61, 2] = 0.0005d;
                    dblElemVals[62, 1] = 150.36d;
                    dblElemVals[62, 2] = 0.03d;
                    dblElemVals[63, 1] = 151.964d;
                    dblElemVals[63, 2] = 0.001d;
                    dblElemVals[64, 1] = 157.25d;
                    dblElemVals[64, 2] = 0.03d;
                    dblElemVals[65, 1] = 158.92534d;
                    dblElemVals[65, 2] = 0.00002d;
                    dblElemVals[66, 1] = 162.5d;
                    dblElemVals[66, 2] = 0.03d;
                    dblElemVals[67, 1] = 164.93032d;
                    dblElemVals[67, 2] = 0.00002d;
                    dblElemVals[68, 1] = 167.26d;
                    dblElemVals[68, 2] = 0.03d;
                    dblElemVals[69, 1] = 168.93421d;
                    dblElemVals[69, 2] = 0.00002d;
                    dblElemVals[70, 1] = 173.04d;
                    dblElemVals[70, 2] = 0.03d;
                    dblElemVals[71, 1] = 174.967d;
                    dblElemVals[71, 2] = 0.001d;
                    dblElemVals[72, 1] = 178.49d;
                    dblElemVals[72, 2] = 0.02d;
                    dblElemVals[73, 1] = 180.9479d;
                    dblElemVals[73, 2] = 0.0001d;
                    dblElemVals[74, 1] = 183.84d;
                    dblElemVals[74, 2] = 0.01d;
                    dblElemVals[75, 1] = 186.207d;
                    dblElemVals[75, 2] = 0.001d;
                    dblElemVals[76, 1] = 190.23d;
                    dblElemVals[76, 2] = 0.03d;
                    dblElemVals[77, 1] = 192.217d;
                    dblElemVals[77, 2] = 0.03d;
                    dblElemVals[78, 1] = 195.078d;
                    dblElemVals[78, 2] = 0.002d;
                    dblElemVals[79, 1] = 196.96655d;
                    dblElemVals[79, 2] = 0.00002d;
                    dblElemVals[80, 1] = 200.59d;
                    dblElemVals[80, 2] = 0.02d;
                    dblElemVals[81, 1] = 204.3833d;
                    dblElemVals[81, 2] = 0.0002d;
                    dblElemVals[82, 1] = 207.2d;
                    dblElemVals[82, 2] = 0.1d;
                    dblElemVals[83, 1] = 208.98038d;
                    dblElemVals[83, 2] = 0.00002d;
                    dblElemVals[84, 1] = 208.9824d;
                    dblElemVals[84, 2] = 0.0005d;
                    dblElemVals[85, 1] = 209.9871d;
                    dblElemVals[85, 2] = 0.0005d;
                    dblElemVals[86, 1] = 222.0176d;
                    dblElemVals[86, 2] = 0.0005d;
                    dblElemVals[87, 1] = 223.0197d;
                    dblElemVals[87, 2] = 0.0005d;
                    dblElemVals[88, 1] = 226.0254d;
                    dblElemVals[88, 2] = 0.0001d;
                    dblElemVals[89, 1] = 227.0278d;
                    dblElemVals[89, 2] = 0.00001d;
                    dblElemVals[90, 1] = 232.0381d;
                    dblElemVals[90, 2] = 0.0001d;
                    dblElemVals[91, 1] = 231.03588d;
                    dblElemVals[91, 2] = 0.00002d;
                    dblElemVals[92, 1] = 238.0289d;
                    dblElemVals[92, 2] = 0.0001d;
                    dblElemVals[93, 1] = 237.0482d;
                    dblElemVals[93, 2] = 0.0005d;
                    dblElemVals[94, 1] = 244.0642d;
                    dblElemVals[94, 2] = 0.0005d;
                    dblElemVals[95, 1] = 243.0614d;
                    dblElemVals[95, 2] = 0.0005d;
                    dblElemVals[96, 1] = 247.0703d;
                    dblElemVals[96, 2] = 0.0005d;
                    dblElemVals[97, 1] = 247.0703d;
                    dblElemVals[97, 2] = 0.0005d;
                    dblElemVals[98, 1] = 251.0796d;
                    dblElemVals[98, 2] = 0.0005d;
                    dblElemVals[99, 1] = 252.083d;
                    dblElemVals[99, 2] = 0.005d;
                    dblElemVals[100, 1] = 257.0951d;
                    dblElemVals[100, 2] = 0.0005d;
                    dblElemVals[101, 1] = 258.1d;
                    dblElemVals[101, 2] = 0.05d;
                    dblElemVals[102, 1] = 259.1009d;
                    dblElemVals[102, 2] = 0.0005d;
                    dblElemVals[103, 1] = 262.11d;
                    dblElemVals[103, 2] = 0.05d;
                    break;
                    // Unused elements
                    // data 104,Unq,Unnilquadium,261,1, 105,Unp,Unnilpentium,262,1, 106,Unh,Unnilhexium,263,1
            }

            if (intSpecificElement == 0)
            {
                // Updating all the elements
                for (intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                {
                    {
                        var withBlock = ElementStats[intElementIndex];
                        withBlock.Symbol = strElementNames[intElementIndex];
                        withBlock.Mass = dblElemVals[intElementIndex, 1];
                        withBlock.Uncertainty = dblElemVals[intElementIndex, 2];
                        withBlock.Charge = (float)dblElemVals[intElementIndex, 3];
                        ElementAlph[intElementIndex] = withBlock.Symbol;
                    }
                }

                // Alphabetize ElementAlph() array via bubble sort
                for (intCompareIndex = ELEMENT_COUNT; intCompareIndex >= 2; intCompareIndex += -1) // Sort from end to start
                {
                    var loopTo = (short)(intCompareIndex - 1);
                    for (intIndex = 1; intIndex <= loopTo; intIndex++)
                    {
                        if (Operators.CompareString(ElementAlph[intIndex], ElementAlph[intIndex + 1], false) > 0)
                        {
                            // Swap them
                            strSwap = ElementAlph[intIndex];
                            ElementAlph[intIndex] = ElementAlph[intIndex + 1];
                            ElementAlph[intIndex + 1] = strSwap;
                        }
                    }
                }
            }
            else if (intSpecificElement >= 1 && intSpecificElement <= ELEMENT_COUNT)
            {
                {
                    var withBlock1 = ElementStats[intSpecificElement];
                    switch (eSpecificStatToReset)
                    {
                        case MolecularWeightTool.esElementStatsConstants.esMass:
                            withBlock1.Mass = dblElemVals[intSpecificElement, 1];
                            break;
                        case MolecularWeightTool.esElementStatsConstants.esUncertainty:
                            withBlock1.Uncertainty = dblElemVals[intSpecificElement, 2];
                            break;
                        case MolecularWeightTool.esElementStatsConstants.esCharge:
                            withBlock1.Charge = (float)dblElemVals[intSpecificElement, 3];
                            break;
                        default:
                            break;
                            // Ignore it
                    }
                }
            }
        }

        /// <summary>
        /// Stores isotope information in ElementStats()
        /// </summary>
        private void MemoryLoadIsotopes()
        {
            short intElementIndex, intIsotopeIndex;

            // The dblIsoMasses() array holds the mass of each isotope
            // starting with dblIsoMasses(x,1), dblIsoMasses(x, 2), etc.
            double[,] dblIsoMasses;
            dblIsoMasses = new double[104, 12];

            // The sngIsoAbun() array holds the isotopic abundances of each of the isotopes,
            // starting with sngIsoAbun(x,1) and corresponding to dblIsoMasses()
            float[,] sngIsoAbun;
            sngIsoAbun = new float[104, 12];
            dblIsoMasses[1, 1] = 1.0078246d;
            sngIsoAbun[1, 1] = 0.99985f;
            dblIsoMasses[1, 2] = 2.014d;
            sngIsoAbun[1, 2] = 0.00015f;
            dblIsoMasses[2, 1] = 3.01603d;
            sngIsoAbun[2, 1] = 0.00000137f;
            dblIsoMasses[2, 2] = 4.0026029d;
            sngIsoAbun[2, 2] = 0.99999863f;
            dblIsoMasses[3, 1] = 6.01512d;
            sngIsoAbun[3, 1] = 0.0759f;
            dblIsoMasses[3, 2] = 7.016005d;
            sngIsoAbun[3, 2] = 0.9241f;
            dblIsoMasses[4, 1] = 9.012183d;
            sngIsoAbun[4, 1] = 1f;
            dblIsoMasses[5, 1] = 10.0129d;
            sngIsoAbun[5, 1] = 0.199f;
            dblIsoMasses[5, 2] = 11.009305d;
            sngIsoAbun[5, 2] = 0.801f;
            dblIsoMasses[6, 1] = 12d;
            sngIsoAbun[6, 1] = 0.9893f;
            dblIsoMasses[6, 2] = 13.00335d;
            sngIsoAbun[6, 2] = 0.0107f;
            dblIsoMasses[7, 1] = 14.003074d;
            sngIsoAbun[7, 1] = 0.99632f;
            dblIsoMasses[7, 2] = 15.00011d;
            sngIsoAbun[7, 2] = 0.00368f;
            dblIsoMasses[8, 1] = 15.994915d;
            sngIsoAbun[8, 1] = 0.99757f;
            dblIsoMasses[8, 2] = 16.999131d;
            sngIsoAbun[8, 2] = 0.00038f;
            dblIsoMasses[8, 3] = 17.99916d;
            sngIsoAbun[8, 3] = 0.00205f;
            dblIsoMasses[9, 1] = 18.9984032d;
            sngIsoAbun[9, 1] = 1f;
            dblIsoMasses[10, 1] = 19.992439d;
            sngIsoAbun[10, 1] = 0.9048f;
            dblIsoMasses[10, 2] = 20.99395d;
            sngIsoAbun[10, 2] = 0.0027f;
            dblIsoMasses[10, 3] = 21.99138d;
            sngIsoAbun[10, 3] = 0.0925f;
            dblIsoMasses[11, 1] = 22.98977d;
            sngIsoAbun[11, 1] = 1f;
            dblIsoMasses[12, 1] = 23.98505d;
            sngIsoAbun[12, 1] = 0.7899f;
            dblIsoMasses[12, 2] = 24.98584d;
            sngIsoAbun[12, 2] = 0.1f;
            dblIsoMasses[12, 3] = 25.98259d;
            sngIsoAbun[12, 3] = 0.1101f;
            dblIsoMasses[13, 1] = 26.981541d;
            sngIsoAbun[13, 1] = 1f;
            dblIsoMasses[14, 1] = 27.976928d;
            sngIsoAbun[14, 1] = 0.922297f;
            dblIsoMasses[14, 2] = 28.97649d;
            sngIsoAbun[14, 2] = 0.046832f;
            dblIsoMasses[14, 3] = 29.97376d;
            sngIsoAbun[14, 3] = 0.030871f;
            dblIsoMasses[15, 1] = 30.973763d;
            sngIsoAbun[15, 1] = 1f;
            dblIsoMasses[16, 1] = 31.972072d;
            sngIsoAbun[16, 1] = 0.9493f;
            dblIsoMasses[16, 2] = 32.97146d;
            sngIsoAbun[16, 2] = 0.0076f;
            dblIsoMasses[16, 3] = 33.96786d;
            sngIsoAbun[16, 3] = 0.0429f;
            dblIsoMasses[16, 4] = 35.96709d;
            sngIsoAbun[16, 4] = 0.0002f;
            dblIsoMasses[17, 1] = 34.968853d;
            sngIsoAbun[17, 1] = 0.7578f;
            dblIsoMasses[17, 2] = 36.99999d;
            sngIsoAbun[17, 2] = 0.2422f;
            dblIsoMasses[18, 1] = 35.96755d;
            sngIsoAbun[18, 1] = 0.003365f;
            dblIsoMasses[18, 2] = 37.96272d;
            sngIsoAbun[18, 2] = 0.000632f;
            dblIsoMasses[18, 3] = 39.96999d;
            sngIsoAbun[18, 3] = 0.996003f; // Note: Alternate mass is 39.962383
            dblIsoMasses[19, 1] = 38.963708d;
            sngIsoAbun[19, 1] = 0.932581f;
            dblIsoMasses[19, 2] = 39.963999d;
            sngIsoAbun[19, 2] = 0.000117f;
            dblIsoMasses[19, 3] = 40.961825d;
            sngIsoAbun[19, 3] = 0.067302f;
            dblIsoMasses[20, 1] = 39.962591d;
            sngIsoAbun[20, 1] = 0.96941f;
            dblIsoMasses[20, 2] = 41.958618d;
            sngIsoAbun[20, 2] = 0.00647f;
            dblIsoMasses[20, 3] = 42.958766d;
            sngIsoAbun[20, 3] = 0.00135f;
            dblIsoMasses[20, 4] = 43.95548d;
            sngIsoAbun[20, 4] = 0.02086f;
            dblIsoMasses[20, 5] = 45.953689d;
            sngIsoAbun[20, 5] = 0.00004f;
            dblIsoMasses[20, 6] = 47.952533d;
            sngIsoAbun[20, 6] = 0.00187f;
            dblIsoMasses[21, 1] = 44.959404d;
            sngIsoAbun[21, 1] = 1f; // Note: Alternate mass is 44.955914
            dblIsoMasses[22, 1] = 45.952629d;
            sngIsoAbun[22, 1] = 0.0825f;
            dblIsoMasses[22, 2] = 46.951764d;
            sngIsoAbun[22, 2] = 0.0744f;
            dblIsoMasses[22, 3] = 47.947947d;
            sngIsoAbun[22, 3] = 0.7372f;
            dblIsoMasses[22, 4] = 48.947871d;
            sngIsoAbun[22, 4] = 0.0541f;
            dblIsoMasses[22, 5] = 49.944792d;
            sngIsoAbun[22, 5] = 0.0518f;
            dblIsoMasses[23, 1] = 49.947161d;
            sngIsoAbun[23, 1] = 0.0025f;
            dblIsoMasses[23, 2] = 50.943963d;
            sngIsoAbun[23, 2] = 0.9975f;
            dblIsoMasses[24, 1] = 49.946046d;
            sngIsoAbun[24, 1] = 0.04345f;
            dblIsoMasses[24, 2] = 51.940509d;
            sngIsoAbun[24, 2] = 0.83789f;
            dblIsoMasses[24, 3] = 52.940651d;
            sngIsoAbun[24, 3] = 0.09501f;
            dblIsoMasses[24, 4] = 53.938882d;
            sngIsoAbun[24, 4] = 0.02365f;
            dblIsoMasses[25, 1] = 54.938046d;
            sngIsoAbun[25, 1] = 1f;
            dblIsoMasses[26, 1] = 53.939612d;
            sngIsoAbun[26, 1] = 0.05845f;
            dblIsoMasses[26, 2] = 55.934939d;
            sngIsoAbun[26, 2] = 0.91754f;
            dblIsoMasses[26, 3] = 56.935396d;
            sngIsoAbun[26, 3] = 0.02119f;
            dblIsoMasses[26, 4] = 57.933277d;
            sngIsoAbun[26, 4] = 0.00282f;
            dblIsoMasses[27, 1] = 58.933198d;
            sngIsoAbun[27, 1] = 1f;
            dblIsoMasses[28, 1] = 57.935347d;
            sngIsoAbun[28, 1] = 0.680769f;
            dblIsoMasses[28, 2] = 59.930788d;
            sngIsoAbun[28, 2] = 0.262231f;
            dblIsoMasses[28, 3] = 60.931058d;
            sngIsoAbun[28, 3] = 0.011399f;
            dblIsoMasses[28, 4] = 61.928346d;
            sngIsoAbun[28, 4] = 0.036345f;
            dblIsoMasses[28, 5] = 63.927968d;
            sngIsoAbun[28, 5] = 0.009256f;
            dblIsoMasses[29, 1] = 62.939598d;
            sngIsoAbun[29, 1] = 0.6917f; // Note: Alternate mass is 62.929599
            dblIsoMasses[29, 2] = 64.927793d;
            sngIsoAbun[29, 2] = 0.3083f;
            dblIsoMasses[30, 1] = 63.929145d;
            sngIsoAbun[30, 1] = 0.4863f;
            dblIsoMasses[30, 2] = 65.926034d;
            sngIsoAbun[30, 2] = 0.279f;
            dblIsoMasses[30, 3] = 66.927129d;
            sngIsoAbun[30, 3] = 0.041f;
            dblIsoMasses[30, 4] = 67.924846d;
            sngIsoAbun[30, 4] = 0.1875f;
            dblIsoMasses[30, 5] = 69.925325d;
            sngIsoAbun[30, 5] = 0.0062f;
            dblIsoMasses[31, 1] = 68.925581d;
            sngIsoAbun[31, 1] = 0.60108f;
            dblIsoMasses[31, 2] = 70.9247d;
            sngIsoAbun[31, 2] = 0.39892f;
            dblIsoMasses[32, 1] = 69.92425d;
            sngIsoAbun[32, 1] = 0.2084f;
            dblIsoMasses[32, 2] = 71.922079d;
            sngIsoAbun[32, 2] = 0.2754f;
            dblIsoMasses[32, 3] = 72.923463d;
            sngIsoAbun[32, 3] = 0.0773f;
            dblIsoMasses[32, 4] = 73.921177d;
            sngIsoAbun[32, 4] = 0.3628f;
            dblIsoMasses[32, 5] = 75.921401d;
            sngIsoAbun[32, 5] = 0.0761f;
            dblIsoMasses[33, 1] = 74.921596d;
            sngIsoAbun[33, 1] = 1f;
            dblIsoMasses[34, 1] = 73.922475d;
            sngIsoAbun[34, 1] = 0.0089f;
            dblIsoMasses[34, 2] = 75.919212d;
            sngIsoAbun[34, 2] = 0.0937f;
            dblIsoMasses[34, 3] = 76.919912d;
            sngIsoAbun[34, 3] = 0.0763f;
            dblIsoMasses[34, 4] = 77.919d;
            sngIsoAbun[34, 4] = 0.2377f;
            dblIsoMasses[34, 5] = 79.916521d;
            sngIsoAbun[34, 5] = 0.4961f;
            dblIsoMasses[34, 6] = 81.916698d;
            sngIsoAbun[34, 6] = 0.0873f;
            dblIsoMasses[35, 1] = 78.918336d;
            sngIsoAbun[35, 1] = 0.5069f;
            dblIsoMasses[35, 2] = 80.916289d;
            sngIsoAbun[35, 2] = 0.4931f;
            dblIsoMasses[36, 1] = 77.92d;
            sngIsoAbun[36, 1] = 0.0035f;
            dblIsoMasses[36, 2] = 79.91638d;
            sngIsoAbun[36, 2] = 0.0228f;
            dblIsoMasses[36, 3] = 81.913482d;
            sngIsoAbun[36, 3] = 0.1158f;
            dblIsoMasses[36, 4] = 82.914135d;
            sngIsoAbun[36, 4] = 0.1149f;
            dblIsoMasses[36, 5] = 83.911506d;
            sngIsoAbun[36, 5] = 0.57f;
            dblIsoMasses[36, 6] = 85.910616d;
            sngIsoAbun[36, 6] = 0.173f;
            dblIsoMasses[37, 1] = 84.911794d;
            sngIsoAbun[37, 1] = 0.7217f;
            dblIsoMasses[37, 2] = 86.909187d;
            sngIsoAbun[37, 2] = 0.2783f;
            dblIsoMasses[38, 1] = 83.91343d;
            sngIsoAbun[38, 1] = 0.0056f;
            dblIsoMasses[38, 2] = 85.909267d;
            sngIsoAbun[38, 2] = 0.0986f;
            dblIsoMasses[38, 3] = 86.908884d;
            sngIsoAbun[38, 3] = 0.07f;
            dblIsoMasses[38, 4] = 87.905625d;
            sngIsoAbun[38, 4] = 0.8258f;
            dblIsoMasses[39, 1] = 88.905856d;
            sngIsoAbun[39, 1] = 1f;
            dblIsoMasses[40, 1] = 89.904708d;
            sngIsoAbun[40, 1] = 0.5145f;
            dblIsoMasses[40, 2] = 90.905644d;
            sngIsoAbun[40, 2] = 0.1122f;
            dblIsoMasses[40, 3] = 91.905039d;
            sngIsoAbun[40, 3] = 0.1715f;
            dblIsoMasses[40, 4] = 93.906314d;
            sngIsoAbun[40, 4] = 0.1738f;
            dblIsoMasses[40, 5] = 95.908275d;
            sngIsoAbun[40, 5] = 0.028f;
            dblIsoMasses[41, 1] = 92.906378d;
            sngIsoAbun[41, 1] = 1f;
            dblIsoMasses[42, 1] = 91.906808d;
            sngIsoAbun[42, 1] = 0.1484f;
            dblIsoMasses[42, 2] = 93.905085d;
            sngIsoAbun[42, 2] = 0.0925f;
            dblIsoMasses[42, 3] = 94.90584d;
            sngIsoAbun[42, 3] = 0.1592f;
            dblIsoMasses[42, 4] = 95.904678d;
            sngIsoAbun[42, 4] = 0.1668f;
            dblIsoMasses[42, 5] = 96.90602d;
            sngIsoAbun[42, 5] = 0.0955f;
            dblIsoMasses[42, 6] = 97.905405d;
            sngIsoAbun[42, 6] = 0.2413f;
            dblIsoMasses[42, 7] = 99.907477d;
            sngIsoAbun[42, 7] = 0.0963f;
            dblIsoMasses[43, 1] = 97.9072d;
            sngIsoAbun[43, 1] = 1f;
            dblIsoMasses[44, 1] = 95.907599d;
            sngIsoAbun[44, 1] = 0.0554f;
            dblIsoMasses[44, 2] = 97.905287d;
            sngIsoAbun[44, 2] = 0.0187f;
            dblIsoMasses[44, 3] = 98.905939d;
            sngIsoAbun[44, 3] = 0.1276f;
            dblIsoMasses[44, 4] = 99.904219d;
            sngIsoAbun[44, 4] = 0.126f;
            dblIsoMasses[44, 5] = 100.905582d;
            sngIsoAbun[44, 5] = 0.1706f;
            dblIsoMasses[44, 6] = 101.904348d;
            sngIsoAbun[44, 6] = 0.3155f;
            dblIsoMasses[44, 7] = 103.905424d;
            sngIsoAbun[44, 7] = 0.1862f;
            dblIsoMasses[45, 1] = 102.905503d;
            sngIsoAbun[45, 1] = 1f;
            dblIsoMasses[46, 1] = 101.905634d;
            sngIsoAbun[46, 1] = 0.0102f;
            dblIsoMasses[46, 2] = 103.904029d;
            sngIsoAbun[46, 2] = 0.1114f;
            dblIsoMasses[46, 3] = 104.905079d;
            sngIsoAbun[46, 3] = 0.2233f;
            dblIsoMasses[46, 4] = 105.903475d;
            sngIsoAbun[46, 4] = 0.2733f;
            dblIsoMasses[46, 5] = 107.903895d;
            sngIsoAbun[46, 5] = 0.2646f;
            dblIsoMasses[46, 6] = 109.905167d;
            sngIsoAbun[46, 6] = 0.1172f;
            dblIsoMasses[47, 1] = 106.905095d;
            sngIsoAbun[47, 1] = 0.51839f;
            dblIsoMasses[47, 2] = 108.904757d;
            sngIsoAbun[47, 2] = 0.48161f;
            dblIsoMasses[48, 1] = 105.906461d;
            sngIsoAbun[48, 1] = 0.0125f;
            dblIsoMasses[48, 2] = 107.904176d;
            sngIsoAbun[48, 2] = 0.0089f;
            dblIsoMasses[48, 3] = 109.903005d;
            sngIsoAbun[48, 3] = 0.1249f;
            dblIsoMasses[48, 4] = 110.904182d;
            sngIsoAbun[48, 4] = 0.128f;
            dblIsoMasses[48, 5] = 111.902758d;
            sngIsoAbun[48, 5] = 0.2413f;
            dblIsoMasses[48, 6] = 112.9044d;
            sngIsoAbun[48, 6] = 0.1222f;
            dblIsoMasses[48, 7] = 113.903361d;
            sngIsoAbun[48, 7] = 0.2873f;
            dblIsoMasses[48, 8] = 115.904754d;
            sngIsoAbun[48, 8] = 0.0749f;
            dblIsoMasses[49, 1] = 112.904061d;
            sngIsoAbun[49, 1] = 0.0429f;
            dblIsoMasses[49, 2] = 114.903875d;
            sngIsoAbun[49, 2] = 0.9571f;
            dblIsoMasses[50, 1] = 111.904826d;
            sngIsoAbun[50, 1] = 0.0097f;
            dblIsoMasses[50, 2] = 113.902784d;
            sngIsoAbun[50, 2] = 0.0066f;
            dblIsoMasses[50, 3] = 114.903348d;
            sngIsoAbun[50, 3] = 0.0034f;
            dblIsoMasses[50, 4] = 115.901747d;
            sngIsoAbun[50, 4] = 0.1454f;
            dblIsoMasses[50, 5] = 116.902956d;
            sngIsoAbun[50, 5] = 0.0768f;
            dblIsoMasses[50, 6] = 117.901609d;
            sngIsoAbun[50, 6] = 0.2422f;
            dblIsoMasses[50, 7] = 118.90331d;
            sngIsoAbun[50, 7] = 0.0859f;
            dblIsoMasses[50, 8] = 119.902199d;
            sngIsoAbun[50, 8] = 0.3258f;
            dblIsoMasses[50, 9] = 121.90344d;
            sngIsoAbun[50, 9] = 0.0463f;
            dblIsoMasses[50, 10] = 123.905274d;
            sngIsoAbun[50, 10] = 0.0579f;
            dblIsoMasses[51, 1] = 120.903824d;
            sngIsoAbun[51, 1] = 0.5721f;
            dblIsoMasses[51, 2] = 122.904216d;
            sngIsoAbun[51, 2] = 0.4279f;
            dblIsoMasses[52, 1] = 119.904048d;
            sngIsoAbun[52, 1] = 0.0009f;
            dblIsoMasses[52, 2] = 121.903054d;
            sngIsoAbun[52, 2] = 0.0255f;
            dblIsoMasses[52, 3] = 122.904271d;
            sngIsoAbun[52, 3] = 0.0089f;
            dblIsoMasses[52, 4] = 123.902823d;
            sngIsoAbun[52, 4] = 0.0474f;
            dblIsoMasses[52, 5] = 124.904433d;
            sngIsoAbun[52, 5] = 0.0707f;
            dblIsoMasses[52, 6] = 125.903314d;
            sngIsoAbun[52, 6] = 0.1884f;
            dblIsoMasses[52, 7] = 127.904463d;
            sngIsoAbun[52, 7] = 0.3174f;
            dblIsoMasses[52, 8] = 129.906229d;
            sngIsoAbun[52, 8] = 0.3408f;
            dblIsoMasses[53, 1] = 126.904477d;
            sngIsoAbun[53, 1] = 1f;
            dblIsoMasses[54, 1] = 123.905894d;
            sngIsoAbun[54, 1] = 0.0009f;
            dblIsoMasses[54, 2] = 125.904281d;
            sngIsoAbun[54, 2] = 0.0009f;
            dblIsoMasses[54, 3] = 127.903531d;
            sngIsoAbun[54, 3] = 0.0192f;
            dblIsoMasses[54, 4] = 128.90478d;
            sngIsoAbun[54, 4] = 0.2644f;
            dblIsoMasses[54, 5] = 129.903509d;
            sngIsoAbun[54, 5] = 0.0408f;
            dblIsoMasses[54, 6] = 130.905072d;
            sngIsoAbun[54, 6] = 0.2118f;
            dblIsoMasses[54, 7] = 131.904148d;
            sngIsoAbun[54, 7] = 0.2689f;
            dblIsoMasses[54, 8] = 133.905395d;
            sngIsoAbun[54, 8] = 0.1044f;
            dblIsoMasses[54, 9] = 135.907214d;
            sngIsoAbun[54, 9] = 0.0887f;
            dblIsoMasses[55, 1] = 132.905433d;
            sngIsoAbun[55, 1] = 1f;
            dblIsoMasses[56, 1] = 129.906282d;
            sngIsoAbun[56, 1] = 0.00106f;
            dblIsoMasses[56, 2] = 131.905042d;
            sngIsoAbun[56, 2] = 0.00101f;
            dblIsoMasses[56, 3] = 133.904486d;
            sngIsoAbun[56, 3] = 0.02417f;
            dblIsoMasses[56, 4] = 134.905665d;
            sngIsoAbun[56, 4] = 0.06592f;
            dblIsoMasses[56, 5] = 135.904553d;
            sngIsoAbun[56, 5] = 0.07854f;
            dblIsoMasses[56, 6] = 136.905812d;
            sngIsoAbun[56, 6] = 0.11232f;
            dblIsoMasses[56, 7] = 137.905236d;
            sngIsoAbun[56, 7] = 0.71698f;
            dblIsoMasses[57, 1] = 137.907105d;
            sngIsoAbun[57, 1] = 0.0009f;
            dblIsoMasses[57, 2] = 138.906355d;
            sngIsoAbun[57, 2] = 0.9991f;
            dblIsoMasses[58, 1] = 135.90714d;
            sngIsoAbun[58, 1] = 0.00185f;
            dblIsoMasses[58, 2] = 137.905985d;
            sngIsoAbun[58, 2] = 0.00251f;
            dblIsoMasses[58, 3] = 139.905442d;
            sngIsoAbun[58, 3] = 0.8845f;
            dblIsoMasses[58, 4] = 141.909241d;
            sngIsoAbun[58, 4] = 0.11114f;
            dblIsoMasses[59, 1] = 140.907657d;
            sngIsoAbun[59, 1] = 1f;
            dblIsoMasses[60, 1] = 141.907731d;
            sngIsoAbun[60, 1] = 0.272f;
            dblIsoMasses[60, 2] = 142.90981d;
            sngIsoAbun[60, 2] = 0.122f;
            dblIsoMasses[60, 3] = 143.910083d;
            sngIsoAbun[60, 3] = 0.238f;
            dblIsoMasses[60, 4] = 144.91257d;
            sngIsoAbun[60, 4] = 0.083f;
            dblIsoMasses[60, 5] = 145.913113d;
            sngIsoAbun[60, 5] = 0.172f;
            dblIsoMasses[60, 6] = 147.916889d;
            sngIsoAbun[60, 6] = 0.057f;
            dblIsoMasses[60, 7] = 149.920887d;
            sngIsoAbun[60, 7] = 0.056f;
            dblIsoMasses[61, 1] = 144.9127d;
            sngIsoAbun[61, 1] = 1f;
            dblIsoMasses[62, 1] = 143.911998d;
            sngIsoAbun[62, 1] = 0.0307f;
            dblIsoMasses[62, 2] = 146.914895d;
            sngIsoAbun[62, 2] = 0.1499f;
            dblIsoMasses[62, 3] = 147.91482d;
            sngIsoAbun[62, 3] = 0.1124f;
            dblIsoMasses[62, 4] = 148.917181d;
            sngIsoAbun[62, 4] = 0.1382f;
            dblIsoMasses[62, 5] = 149.917273d;
            sngIsoAbun[62, 5] = 0.0738f;
            dblIsoMasses[62, 6] = 151.919741d;
            sngIsoAbun[62, 6] = 0.2675f;
            dblIsoMasses[62, 7] = 153.922206d;
            sngIsoAbun[62, 7] = 0.2275f;
            dblIsoMasses[63, 1] = 150.919847d;
            sngIsoAbun[63, 1] = 0.4781f;
            dblIsoMasses[63, 2] = 152.921243d;
            sngIsoAbun[63, 2] = 0.5219f;
            dblIsoMasses[64, 1] = 151.919786d;
            sngIsoAbun[64, 1] = 0.002f;
            dblIsoMasses[64, 2] = 153.920861d;
            sngIsoAbun[64, 2] = 0.0218f;
            dblIsoMasses[64, 3] = 154.922618d;
            sngIsoAbun[64, 3] = 0.148f;
            dblIsoMasses[64, 4] = 155.922118d;
            sngIsoAbun[64, 4] = 0.2047f;
            dblIsoMasses[64, 5] = 156.923956d;
            sngIsoAbun[64, 5] = 0.1565f;
            dblIsoMasses[64, 6] = 157.924111d;
            sngIsoAbun[64, 6] = 0.2484f;
            dblIsoMasses[64, 7] = 159.927049d;
            sngIsoAbun[64, 7] = 0.2186f;
            dblIsoMasses[65, 1] = 158.92535d;
            sngIsoAbun[65, 1] = 1f;
            dblIsoMasses[66, 1] = 155.925277d;
            sngIsoAbun[66, 1] = 0.0006f;
            dblIsoMasses[66, 2] = 157.924403d;
            sngIsoAbun[66, 2] = 0.001f;
            dblIsoMasses[66, 3] = 159.925193d;
            sngIsoAbun[66, 3] = 0.0234f;
            dblIsoMasses[66, 4] = 160.92693d;
            sngIsoAbun[66, 4] = 0.1891f;
            dblIsoMasses[66, 5] = 161.926795d;
            sngIsoAbun[66, 5] = 0.2551f;
            dblIsoMasses[66, 6] = 162.928728d;
            sngIsoAbun[66, 6] = 0.249f;
            dblIsoMasses[66, 7] = 163.929183d;
            sngIsoAbun[66, 7] = 0.2818f;
            dblIsoMasses[67, 1] = 164.930332d;
            sngIsoAbun[67, 1] = 1f;
            dblIsoMasses[68, 1] = 161.928775d;
            sngIsoAbun[68, 1] = 0.0014f;
            dblIsoMasses[68, 2] = 163.929198d;
            sngIsoAbun[68, 2] = 0.0161f;
            dblIsoMasses[68, 3] = 165.930305d;
            sngIsoAbun[68, 3] = 0.3361f;
            dblIsoMasses[68, 4] = 166.932046d;
            sngIsoAbun[68, 4] = 0.2293f;
            dblIsoMasses[68, 5] = 167.932368d;
            sngIsoAbun[68, 5] = 0.2678f;
            dblIsoMasses[68, 6] = 169.935461d;
            sngIsoAbun[68, 6] = 0.1493f;
            dblIsoMasses[69, 1] = 168.934225d;
            sngIsoAbun[69, 1] = 1f;
            dblIsoMasses[70, 1] = 167.932873d;
            sngIsoAbun[70, 1] = 0.0013f;
            dblIsoMasses[70, 2] = 169.934759d;
            sngIsoAbun[70, 2] = 0.0304f;
            dblIsoMasses[70, 3] = 170.936323d;
            sngIsoAbun[70, 3] = 0.1428f;
            dblIsoMasses[70, 4] = 171.936387d;
            sngIsoAbun[70, 4] = 0.2183f;
            dblIsoMasses[70, 5] = 172.938208d;
            sngIsoAbun[70, 5] = 0.1613f;
            dblIsoMasses[70, 6] = 173.938873d;
            sngIsoAbun[70, 6] = 0.3183f;
            dblIsoMasses[70, 7] = 175.942564d;
            sngIsoAbun[70, 7] = 0.1276f;
            dblIsoMasses[71, 1] = 174.940785d;
            sngIsoAbun[71, 1] = 0.9741f;
            dblIsoMasses[71, 2] = 175.942679d;
            sngIsoAbun[71, 2] = 0.0259f;
            dblIsoMasses[72, 1] = 173.94004d;
            sngIsoAbun[72, 1] = 0.0016f;
            dblIsoMasses[72, 2] = 175.941406d;
            sngIsoAbun[72, 2] = 0.0526f;
            dblIsoMasses[72, 3] = 176.943217d;
            sngIsoAbun[72, 3] = 0.186f;
            dblIsoMasses[72, 4] = 177.943696d;
            sngIsoAbun[72, 4] = 0.2728f;
            dblIsoMasses[72, 5] = 178.945812d;
            sngIsoAbun[72, 5] = 0.1362f;
            dblIsoMasses[72, 6] = 179.946561d;
            sngIsoAbun[72, 6] = 0.3508f;
            dblIsoMasses[73, 1] = 179.947462d;
            sngIsoAbun[73, 1] = 0.00012f;
            dblIsoMasses[73, 2] = 180.948014d;
            sngIsoAbun[73, 2] = 0.99988f;
            dblIsoMasses[74, 1] = 179.946701d;
            sngIsoAbun[74, 1] = 0.0012f;
            dblIsoMasses[74, 2] = 181.948202d;
            sngIsoAbun[74, 2] = 0.265f;
            dblIsoMasses[74, 3] = 182.95022d;
            sngIsoAbun[74, 3] = 0.1431f;
            dblIsoMasses[74, 4] = 183.950953d;
            sngIsoAbun[74, 4] = 0.3064f;
            dblIsoMasses[74, 5] = 185.954357d;
            sngIsoAbun[74, 5] = 0.2843f;
            dblIsoMasses[75, 1] = 184.952951d;
            sngIsoAbun[75, 1] = 0.374f;
            dblIsoMasses[75, 2] = 186.955765d;
            sngIsoAbun[75, 2] = 0.626f;
            dblIsoMasses[76, 1] = 183.952488d;
            sngIsoAbun[76, 1] = 0.0002f;
            dblIsoMasses[76, 2] = 185.95383d;
            sngIsoAbun[76, 2] = 0.0159f;
            dblIsoMasses[76, 3] = 186.955741d;
            sngIsoAbun[76, 3] = 0.0196f;
            dblIsoMasses[76, 4] = 187.95586d;
            sngIsoAbun[76, 4] = 0.1324f;
            dblIsoMasses[76, 5] = 188.958137d;
            sngIsoAbun[76, 5] = 0.1615f;
            dblIsoMasses[76, 6] = 189.958436d;
            sngIsoAbun[76, 6] = 0.2626f;
            dblIsoMasses[76, 7] = 191.961467d;
            sngIsoAbun[76, 7] = 0.4078f; // Note: Alternate mass is 191.960603
            dblIsoMasses[77, 1] = 190.960584d;
            sngIsoAbun[77, 1] = 0.373f;
            dblIsoMasses[77, 2] = 192.962942d;
            sngIsoAbun[77, 2] = 0.627f;
            dblIsoMasses[78, 1] = 189.959917d;
            sngIsoAbun[78, 1] = 0.00014f;
            dblIsoMasses[78, 2] = 191.961019d;
            sngIsoAbun[78, 2] = 0.00782f;
            dblIsoMasses[78, 3] = 193.962655d;
            sngIsoAbun[78, 3] = 0.32967f;
            dblIsoMasses[78, 4] = 194.964785d;
            sngIsoAbun[78, 4] = 0.33832f;
            dblIsoMasses[78, 5] = 195.964926d;
            sngIsoAbun[78, 5] = 0.25242f;
            dblIsoMasses[78, 6] = 197.967869d;
            sngIsoAbun[78, 6] = 0.07163f;
            dblIsoMasses[79, 1] = 196.966543d;
            sngIsoAbun[79, 1] = 1f;
            dblIsoMasses[80, 1] = 195.965807d;
            sngIsoAbun[80, 1] = 0.0015f;
            dblIsoMasses[80, 2] = 197.966743d;
            sngIsoAbun[80, 2] = 0.0997f;
            dblIsoMasses[80, 3] = 198.968254d;
            sngIsoAbun[80, 3] = 0.1687f;
            dblIsoMasses[80, 4] = 199.9683d;
            sngIsoAbun[80, 4] = 0.231f;
            dblIsoMasses[80, 5] = 200.970277d;
            sngIsoAbun[80, 5] = 0.1318f;
            dblIsoMasses[80, 6] = 201.970632d;
            sngIsoAbun[80, 6] = 0.2986f;
            dblIsoMasses[80, 7] = 203.973467d;
            sngIsoAbun[80, 7] = 0.0687f;
            dblIsoMasses[81, 1] = 202.97232d;
            sngIsoAbun[81, 1] = 0.29524f;
            dblIsoMasses[81, 2] = 204.974401d;
            sngIsoAbun[81, 2] = 0.70476f;
            dblIsoMasses[82, 1] = 203.97302d;
            sngIsoAbun[82, 1] = 0.014f;
            dblIsoMasses[82, 2] = 205.97444d;
            sngIsoAbun[82, 2] = 0.241f;
            dblIsoMasses[82, 3] = 206.975872d;
            sngIsoAbun[82, 3] = 0.221f;
            dblIsoMasses[82, 4] = 207.976641d;
            sngIsoAbun[82, 4] = 0.524f;
            dblIsoMasses[83, 1] = 208.980388d;
            sngIsoAbun[83, 1] = 1f;
            dblIsoMasses[84, 1] = 209d;
            sngIsoAbun[84, 1] = 1f;
            dblIsoMasses[85, 1] = 210d;
            sngIsoAbun[85, 1] = 1f;
            dblIsoMasses[86, 1] = 222d;
            sngIsoAbun[86, 1] = 1f;
            dblIsoMasses[87, 1] = 223d;
            sngIsoAbun[87, 1] = 1f;
            dblIsoMasses[88, 1] = 226d;
            sngIsoAbun[88, 1] = 1f;
            dblIsoMasses[89, 1] = 227d;
            sngIsoAbun[89, 1] = 1f;
            dblIsoMasses[90, 1] = 232.038054d;
            sngIsoAbun[90, 1] = 1f;
            dblIsoMasses[91, 1] = 231d;
            sngIsoAbun[91, 1] = 1f;
            dblIsoMasses[92, 1] = 234.041637d;
            sngIsoAbun[92, 1] = 0.000055f;
            dblIsoMasses[92, 2] = 235.043924d;
            sngIsoAbun[92, 2] = 0.0072f;
            dblIsoMasses[92, 3] = 238.050786d;
            sngIsoAbun[92, 3] = 0.992745f;
            dblIsoMasses[93, 1] = 237d;
            sngIsoAbun[93, 1] = 1f;
            dblIsoMasses[94, 1] = 244d;
            sngIsoAbun[94, 1] = 1f;
            dblIsoMasses[95, 1] = 243d;
            sngIsoAbun[95, 1] = 1f;
            dblIsoMasses[96, 1] = 247d;
            sngIsoAbun[96, 1] = 1f;
            dblIsoMasses[97, 1] = 247d;
            sngIsoAbun[97, 1] = 1f;
            dblIsoMasses[98, 1] = 251d;
            sngIsoAbun[98, 1] = 1f;
            dblIsoMasses[99, 1] = 252d;
            sngIsoAbun[99, 1] = 1f;
            dblIsoMasses[100, 1] = 257d;
            sngIsoAbun[100, 1] = 1f;
            dblIsoMasses[101, 1] = 258d;
            sngIsoAbun[101, 1] = 1f;
            dblIsoMasses[102, 1] = 259d;
            sngIsoAbun[102, 1] = 1f;
            dblIsoMasses[103, 1] = 262d;
            sngIsoAbun[103, 1] = 1f;

            // Note: I chose to store the desired values in the dblIsoMasses() and sngIsoAbun() 2D arrays
            // then copy to the ElementStats() array since this method actually decreases
            // the size of this subroutine
            for (intElementIndex = 1; intElementIndex <= ELEMENT_COUNT - 1; intElementIndex++)
            {
                {
                    var withBlock = ElementStats[intElementIndex];
                    intIsotopeIndex = 1;
                    while (dblIsoMasses[intElementIndex, intIsotopeIndex] > 0d)
                    {
                        withBlock.Isotopes[intIsotopeIndex].Abundance = sngIsoAbun[intElementIndex, intIsotopeIndex];
                        withBlock.Isotopes[intIsotopeIndex].Mass = dblIsoMasses[intElementIndex, intIsotopeIndex];
                        intIsotopeIndex = (short)(intIsotopeIndex + 1);
                        if (intIsotopeIndex > MAX_ISOTOPES)
                            break;
                    }

                    withBlock.IsotopeCount = (short)(intIsotopeIndex - 1);
                }
            }
        }

        public void MemoryLoadMessageStatements()
        {
            MessageStatementCount = 1555;
            MessageStatements[1] = "Unknown element";
            MessageStatements[2] = "Obsolete msg: Cannot handle more than 4 layers of embedded parentheses";
            MessageStatements[3] = "Missing closing parentheses";
            MessageStatements[4] = "Unmatched parentheses";
            MessageStatements[5] = "Cannot have a 0 directly after an element or dash (-)";
            MessageStatements[6] = "Number too large or must only be after [, -, ), or caret (^)";
            MessageStatements[7] = "Number too large";
            MessageStatements[8] = "Obsolete msg: Cannot start formula with a number; use parentheses, brackets, or dash (-)";
            MessageStatements[9] = "Obsolete msg: Decimal numbers cannot be used after parentheses; use a [ or a caret (^)";
            MessageStatements[10] = "Obsolete msg: Decimal numbers less than 1 must be in the form .5 and not 0.5";
            MessageStatements[11] = "Numbers should follow left brackets, not right brackets (unless 'treat brackets' as parentheses is on)";
            MessageStatements[12] = "A number must be present after a bracket and/or after the decimal point";
            MessageStatements[13] = "Missing closing bracket, ]";
            MessageStatements[14] = "Misplaced number; should only be after an element, [, ), -, or caret (^)";
            MessageStatements[15] = "Unmatched bracket";
            MessageStatements[16] = "Cannot handle nested brackets or brackets inside multiple hydrates (unless 'treat brackets as parentheses' is on)";
            MessageStatements[17] = "Obsolete msg: Cannot handle multiple hydrates (extras) in brackets";
            MessageStatements[18] = "Unknown element ";
            MessageStatements[19] = "Obsolete msg: Cannot start formula with a dash (-)";
            MessageStatements[20] = "There must be an isotopic mass number following the caret (^)";
            MessageStatements[21] = "Obsolete msg: Zero after caret (^); an isotopic mass of zero is not allowed";
            MessageStatements[22] = "An element must be present after the isotopic mass after the caret (^)";
            MessageStatements[23] = "Negative isotopic masses are not allowed after the caret (^)";
            MessageStatements[24] = "Isotopic masses are not allowed for abbreviations";
            MessageStatements[25] = "An element must be present after the leading coefficient of the dash";
            MessageStatements[26] = "Isotopic masses are not allowed for abbreviations; D is an abbreviation";
            MessageStatements[27] = "Numbers cannot contain more than one decimal point";
            MessageStatements[28] = "Circular abbreviation reference; can't have an abbreviation referencing a second abbreviation that depends upon the first one";
            MessageStatements[29] = "Obsolete msg: Cannot run percent solver until one or more lines are locked to a value.";
            MessageStatements[30] = "Invalid formula subtraction; one or more atoms (or too many atoms) in the right-hand formula are missing (or less abundant) in the left-hand formula";

            // Cases 50 through 74 are used during the % Solver routine
            MessageStatements[50] = "Target value is greater than 100%, an impossible value.";

            // Cases 75 through 99 are used in frmCalculator
            MessageStatements[75] = "Letters are not allowed in the calculator line";
            MessageStatements[76] = "Missing closing parenthesis";
            MessageStatements[77] = "Unmatched parentheses";
            MessageStatements[78] = "Misplaced number; or number too large, too small, or too long";
            MessageStatements[79] = "Obsolete msg: Misplaced parentheses";
            MessageStatements[80] = "Misplaced operator";
            MessageStatements[81] = "Track variable is less than or equal to 1; program bug; please notify programmer";
            MessageStatements[82] = "Missing operator. Note: ( is not needed OR allowed after a + or -";
            MessageStatements[83] = "Obsolete msg: Brackets not allowed in calculator; simply use nested parentheses";
            MessageStatements[84] = "Obsolete msg: Decimal numbers less than 1 must be in the form .5 and not 0.5";
            MessageStatements[85] = "Cannot take negative numbers to a decimal power";
            MessageStatements[86] = "Cannot take zero to a negative power";
            MessageStatements[87] = "Cannot take zero to the zeroth power";
            MessageStatements[88] = "Obsolete msg: Only a single positive or negative number is allowed after a caret (^)";
            MessageStatements[89] = "A single positive or negative number must be present after a caret (^)";
            MessageStatements[90] = "Numbers cannot contain more than one decimal point";
            MessageStatements[91] = "You tried to divide a number by zero.  Please correct the problem and recalculate.";
            MessageStatements[92] = "Spaces are not allowed in mathematical expressions";

            // Note that tags 93 and 94 are also used on frmMain
            MessageStatements[93] = "Use a period for a decimal point";
            MessageStatements[94] = "Use a comma for a decimal point";
            MessageStatements[95] = "A number must be present after a decimal point";


            // Cases 100 and up are shown when loading data from files and starting application
            MessageStatements[100] = "Error Saving Abbreviation File";
            MessageStatements[110] = "The default abbreviation file has been re-created.";
            MessageStatements[115] = "The old file has been renamed";
            MessageStatements[120] = "[AMINO ACIDS] heading not found in MWT_ABBR.DAT file.  This heading must be located before/above the [ABBREVIATIONS] heading.";
            MessageStatements[125] = "Obsolete msg: Select OK to continue without any abbreviations.";
            MessageStatements[130] = "[ABBREVIATIONS] heading not found in MWT_ABBR.DAT file.  This heading must be located before/above the [AMINO ACIDS] heading.";
            MessageStatements[135] = "Select OK to continue with amino acids abbreviations only.";
            MessageStatements[140] = "The Abbreviations File was not found in the program directory";
            MessageStatements[150] = "Error Loading/Creating Abbreviation File";
            MessageStatements[160] = "Ignoring Abbreviation -- Invalid Formula";
            MessageStatements[170] = "Ignoring Duplicate Abbreviation";
            MessageStatements[180] = "Ignoring Abbreviation; Invalid Character";
            MessageStatements[190] = "Ignoring Abbreviation; too long";
            MessageStatements[192] = "Ignoring Abbreviation; symbol length cannot be 0";
            MessageStatements[194] = "Ignoring Abbreviation; symbol most only contain letters";
            MessageStatements[196] = "Ignoring Abbreviation; Too many abbreviations in memory";
            MessageStatements[200] = "Ignoring Invalid Line";
            MessageStatements[210] = "The default elements file has been re-created.";
            MessageStatements[220] = "Possibly incorrect weight for element";
            MessageStatements[230] = "Possibly incorrect uncertainty for element";
            MessageStatements[250] = "Ignoring Line; Invalid Element Symbol";
            MessageStatements[260] = "[ELEMENTS] heading not found in MWT_ELEM.DAT file.  This heading must be located in the file.";
            MessageStatements[265] = "Select OK to continue with default Element values.";
            MessageStatements[270] = "The Elements File was not found in the program directory";
            MessageStatements[280] = "Error Loading/Creating Elements File";
            MessageStatements[305] = "Continuing with default captions.";
            MessageStatements[320] = "Error Saving Elements File";
            MessageStatements[330] = "Error Loading/Creating Values File";
            MessageStatements[340] = "Select OK to continue without loading default Values and Formulas.";
            MessageStatements[345] = "If using a Read-Only drive, use the /X switch at the command line to prevent this error.";
            MessageStatements[350] = "Error";
            MessageStatements[360] = "Error Saving Default Options File";
            MessageStatements[370] = "Obsolete msg: If using a Read-Only drive, you cannot save the default options.";
            MessageStatements[380] = "Error Saving Values and Formulas File";
            MessageStatements[390] = "Obsolete msg: If using a Read-Only drive, you cannot save the values and formulas.";
            MessageStatements[400] = "Error Loading/Creating Default Options File";
            MessageStatements[410] = "Select OK to continue without loading User Defaults.";
            MessageStatements[420] = "Obsolete msg: The Default Options file was corrupted; it will be re-created.";
            MessageStatements[430] = "Obsolete msg: The Values and Formulas file was corrupted; it will be re-created.";
            MessageStatements[440] = "The language file could not be successfully opened or was formatted incorrectly.";
            MessageStatements[450] = "Unable to load language-specific captions";
            MessageStatements[460] = "The language file could not be found in the program directory";
            MessageStatements[470] = "The file requested for molecular weight processing was not found";
            MessageStatements[480] = "File Not Found";
            MessageStatements[490] = "This file already exists.  Replace it?";
            MessageStatements[500] = "File Exists";
            MessageStatements[510] = "Error Reading/Writing files for batch processing";
            MessageStatements[515] = "Select OK to abort batch file processing.";
            MessageStatements[520] = "Error in program";
            MessageStatements[530] = "These lines of code should not have been encountered.  Please notify programmer.";
            MessageStatements[540] = "Obsolete msg: You can't edit elements because the /X switch was used at the command line.";
            MessageStatements[545] = "Obsolete msg: You can't edit abbreviations because the /X switch was used at the command line.";
            MessageStatements[550] = "Percent solver cannot be used when brackets are being treated as parentheses.  You can change the bracket recognition mode by choosing Change Program Preferences under the Options menu.";
            MessageStatements[555] = "Percent Solver not Available";
            MessageStatements[560] = "Maximum number of formula fields exist.";
            MessageStatements[570] = "Current formula is blank.";
            MessageStatements[580] = "Turn off Percent Solver (F11) before creating a new formula.";
            MessageStatements[590] = "An overflow error has occured.  Please reduce number sizes and recalculate.";
            MessageStatements[600] = "An error has occured";
            MessageStatements[605] = "Please exit the program and report the error to the programmer.  Select About from the Help menu to see the E-mail address.";
            MessageStatements[610] = "Spaces are not allowed in formulas";
            MessageStatements[620] = "Invalid Character";
            MessageStatements[630] = "Cannot copy to new formula.";
            MessageStatements[645] = "Obsolete msg: Maximum number of formulas is 7";
            MessageStatements[650] = "Current formula is blank.";
            MessageStatements[655] = "Percent Solver mode is on (F11 to exit mode).";
            MessageStatements[660] = "Warning, isotopic mass is probably too large for element";
            MessageStatements[662] = "Warning, isotopic mass is probably too small for element";
            MessageStatements[665] = "vs avg atomic wt of";
            MessageStatements[670] = "Warning, isotopic mass is impossibly small for element";
            MessageStatements[675] = "protons";
            MessageStatements[680] = "Note: Exact Mode is on";
            MessageStatements[685] = "Note: for % Solver, a left bracket must precede an x";
            MessageStatements[690] = "Note: brackets are being treated as parentheses";
            MessageStatements[700] = "One or more elements must be checked.";
            MessageStatements[705] = "Maximum hits must be greater than 0.";
            MessageStatements[710] = "Maximum hits must be less than ";
            MessageStatements[715] = "Minimum number of elements must be 0 or greater.";
            MessageStatements[720] = "Minimum number of elements must be less than maximum number of elements.";
            MessageStatements[725] = "Maximum number of elements must be less than 65,025";
            MessageStatements[730] = "An atomic weight must be entered for custom elements.";
            MessageStatements[735] = "Atomic Weight must be greater than 0 for custom elements.";
            MessageStatements[740] = "Target molecular weight must be entered.";
            MessageStatements[745] = "Target molecular weight must be greater than 0.";
            MessageStatements[750] = "Obsolete msg: Weight tolerance must be 0 or greater.";
            MessageStatements[755] = "A maximum molecular weight must be entered.";
            MessageStatements[760] = "Maximum molecular weight must be greater than 0.";
            MessageStatements[765] = "Target percentages must be entered for element";
            MessageStatements[770] = "Target percentage must be greater than 0.";
            MessageStatements[775] = "Custom elemental weights must contain only numbers or only letters.  If letters are used, they must be for a single valid elemental symbol or abbreviation.";
            MessageStatements[780] = "Custom elemental weight is empty.  If letters are used, they must be for a single valid elemental symbol or abbreviation.";
            MessageStatements[785] = "Unknown element or abbreviation for custom elemental weight";
            MessageStatements[790] = "Only single elemental symbols or abbreviations are allowed.";
            MessageStatements[800] = "Caution, no abbreviations were loaded -- Command has no effect.";
            MessageStatements[805] = "Cannot handle fractional numbers of atoms";
            MessageStatements[910] = "Ions are already present in the ion list.  Replace with new ions?";
            MessageStatements[920] = "Replace Existing Ions";
            MessageStatements[930] = "Loading Ion List";
            MessageStatements[940] = "Process aborted";
            MessageStatements[945] = " aborted";
            MessageStatements[950] = "Normalizing ions";
            MessageStatements[960] = "Normalizing by region";
            MessageStatements[965] = "Sorting by Intensity";
            MessageStatements[970] = "Matching Ions";
            MessageStatements[980] = "The clipboard is empty.  No ions to paste.";
            MessageStatements[985] = "No ions";
            MessageStatements[990] = "Pasting ion list";
            MessageStatements[1000] = "Determining number of ions in list";
            MessageStatements[1010] = "Parsing list";
            MessageStatements[1020] = "No valid ions were found on the clipboard.  A valid ion list is a list of mass and intensity pairs, separated by commas, tabs, or spaces.  One mass/intensity pair should be present per line.";
            MessageStatements[1030] = "Error writing data to file";
            MessageStatements[1040] = "Set Range";
            MessageStatements[1050] = "Start Val";
            MessageStatements[1055] = "End Val";
            MessageStatements[1060] = "Set X Axis Range";
            MessageStatements[1065] = "Set Y Axis Range";
            MessageStatements[1070] = "Enter a new Gaussian Representation quality factor.  Higher numbers result in smoother Gaussian curves, but slower updates.  Valid range is 1 to 50, default is 20.";
            MessageStatements[1072] = "Gaussian Representation Quality";
            MessageStatements[1075] = "Enter a new plotting approximation factor. Higher numbers result in faster updates, but give a less accurate graphical representation when viewing a wide mass range (zoomed out).  Valid range is 1 to 50, default is 10.";
            MessageStatements[1077] = "Plotting Approximation Factor";
            MessageStatements[1080] = "Resolving Power Specifications";
            MessageStatements[1090] = "Resolving Power";
            MessageStatements[1100] = "X Value of Specification";
            MessageStatements[1110] = "Please enter the approximate number of ticks to show on the axis";
            MessageStatements[1115] = "Axis Ticks";
            MessageStatements[1120] = "Creating Gaussian Representation";
            MessageStatements[1130] = "Preparing plot";
            MessageStatements[1135] = "Drawing plot";
            MessageStatements[1140] = "Are you sure you want to restore the default plotting options?";
            MessageStatements[1145] = "Restore Default Options";
            MessageStatements[1150] = "Auto Align Ions";
            MessageStatements[1155] = "Maximum Offset";
            MessageStatements[1160] = "Offset Increment";
            MessageStatements[1165] = "Aligning Ions";
            MessageStatements[1200] = "Caution symbol must be 1 to " + MAX_ABBREV_LENGTH + " characters long";
            MessageStatements[1205] = "Caution symbol most only contain letters";
            MessageStatements[1210] = "Caution description length cannot be 0";
            MessageStatements[1215] = "Too many caution statements.  Unable to add another one.";
            MessageStatements[1500] = "All Files";
            MessageStatements[1510] = "Text Files";
            MessageStatements[1515] = "txt";
            MessageStatements[1520] = "Data Files";
            MessageStatements[1525] = "csv";
            MessageStatements[1530] = "Sequence Files";
            MessageStatements[1535] = "seq";
            MessageStatements[1540] = "Ion List Files";
            MessageStatements[1545] = "txt";
            MessageStatements[1550] = "Capillary Flow Info Files";
            MessageStatements[1555] = "cap";
        }

        private void MwtWinDllErrorHandler(string strSourceForm)
        {
            string strMessage;
            bool blnShowErrorMessageDialogsSaved;
            if (Information.Err().Number == 6)
            {
                strMessage = LookupMessage(590);
                if (mShowErrorMessageDialogs)
                {
                    Interaction.MsgBox(LookupMessage(590), MsgBoxStyle.OkOnly, LookupMessage(350));
                }

                LogMessage(strMessage, eMessageTypeConstants.ErrorMsg);
            }
            else
            {
                strMessage = LookupMessage(600) + ": " + Information.Err().Description + ControlChars.NewLine + " (" + strSourceForm + " handler)";
                strMessage += ControlChars.NewLine + LookupMessage(605);
                if (mShowErrorMessageDialogs)
                {
                    Interaction.MsgBox(strMessage, MsgBoxStyle.OkOnly, LookupMessage(350));
                }

                // Call GeneralErrorHandler so that the error gets logged to ErrorLog.txt
                // Note that GeneralErrorHandler will call LogMessage

                // Make sure mShowErrorMessageDialogs is false when calling GeneralErrorHandler

                blnShowErrorMessageDialogsSaved = mShowErrorMessageDialogs;
                mShowErrorMessageDialogs = false;
                GeneralErrorHandler(strSourceForm, Information.Err().Number);
                mShowErrorMessageDialogs = blnShowErrorMessageDialogsSaved;
            }
        }

        private void Initialize()
        {
            ElementAlph = new string[104];
            ElementStats = new udtElementStatsType[104];
            for (int i = 0; i <= ELEMENT_COUNT - 1; i++)
                ElementStats[i].Initialize();
            AbbrevStats = new udtAbbrevStatsType[501];
            CautionStatements = new string[101, 3];
            MessageStatements = new string[1601];
            mProgressStepDescription = string.Empty;
            mProgressPercentComplete = 0f;
            mLogFolderPath = string.Empty;
            mLogFilePath = string.Empty;
            mShowErrorMessageDialogs = false;
        }

        private void InitializeAbbrevSymbolStack(ref udtAbbrevSymbolStackType udtAbbrevSymbolStack)
        {
            udtAbbrevSymbolStack.Count = 0;
            udtAbbrevSymbolStack.SymbolReferenceStack = new short[1];
        }

        private void InitializeComputationStats(ref udtComputationStatsType udtComputationStats)
        {
            short intElementIndex;
            udtComputationStats.Initialize();
            udtComputationStats.Charge = 0.0f;
            udtComputationStats.StandardDeviation = 0.0d;
            udtComputationStats.TotalMass = 0.0d;
            for (intElementIndex = 0; intElementIndex <= ELEMENT_COUNT - 1; intElementIndex++)
            {
                {
                    var withBlock = udtComputationStats.Elements[intElementIndex];
                    withBlock.Used = false; // whether element is present
                    withBlock.Count = 0d; // # of each element
                    withBlock.IsotopicCorrection = 0d; // isotopic correction
                    withBlock.IsotopeCount = 0; // Count of the number of atoms defined as specific isotopes
                    withBlock.Isotopes = new usrIsotopicAtomInfoType[3]; // Default to have room for 2 explicitly defined isotopes
                }
            }
        }

        /// <summary>
        /// Determines the molecular weight and elemental composition of strFormula
        /// </summary>
        /// <param name="strFormula">Input/output: formula to parse</param>
        /// <param name="udtComputationStats">Output: additional information about the formula</param>
        /// <returns>Computed molecular weight if no error; otherwise -1</returns>
        public double ParseFormulaPublic(ref string strFormula, ref udtComputationStatsType udtComputationStats)
        {
            double argdblValueForX = 1d;
            return ParseFormulaPublic(ref strFormula, ref udtComputationStats, false, ref argdblValueForX);
        }

        /// <summary>
        /// Determines the molecular weight and elemental composition of strFormula
        /// </summary>
        /// <param name="strFormula">Input/output: formula to parse</param>
        /// <param name="udtComputationStats">Output: additional information about the formula</param>
        /// <param name="blnExpandAbbreviations"></param>
        /// <returns>Computed molecular weight if no error; otherwise -1</returns>
        public double ParseFormulaPublic(ref string strFormula, ref udtComputationStatsType udtComputationStats, bool blnExpandAbbreviations)
        {
            double argdblValueForX = 1d;
            return ParseFormulaPublic(ref strFormula, ref udtComputationStats, blnExpandAbbreviations, ref argdblValueForX);
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
        public double ParseFormulaPublic(ref string strFormula, ref udtComputationStatsType udtComputationStats, bool blnExpandAbbreviations, ref double dblValueForX)
        {
            short intElementIndex;
            double dblStdDevSum;
            var udtAbbrevSymbolStack = new udtAbbrevSymbolStackType();
            try
            {
                // Initialize the UDTs
                InitializeComputationStats(ref udtComputationStats);
                InitializeAbbrevSymbolStack(ref udtAbbrevSymbolStack);
                dblStdDevSum = 0.0d;

                // Reset ErrorParams to clear any prior errors
                ResetErrorParamsInternal();

                // Reset Caution Description
                mStrCautionDescription = "";
                if (Strings.Len(strFormula) > 0)
                {
                    int argCarbonOrSiliconReturnCount = 0;
                    strFormula = ParseFormulaRecursive(strFormula, ref udtComputationStats, ref udtAbbrevSymbolStack, blnExpandAbbreviations, ref dblStdDevSum, dblValueForX: dblValueForX, CarbonOrSiliconReturnCount: ref argCarbonOrSiliconReturnCount);
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
                    for (intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                        // Increase total weight by multiplying the count of each element by the element's mass
                        // In addition, add in the Isotopic Correction value
                        udtComputationStats.TotalMass = udtComputationStats.TotalMass + ElementStats[intElementIndex].Mass * udtComputationStats.Elements[intElementIndex].Count + udtComputationStats.Elements[intElementIndex].IsotopicCorrection;
                    return udtComputationStats.TotalMass;
                }
                else
                {
                    return -1;
                }
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
        private string ParseFormulaRecursive(string strFormula, ref udtComputationStatsType udtComputationStats, ref udtAbbrevSymbolStackType udtAbbrevSymbolStack, bool blnExpandAbbreviations, ref double dblStdDevSum, [Optional, DefaultParameterValue(0)] ref int CarbonOrSiliconReturnCount, double dblValueForX = 1.0d, int intCharCountPrior = 0, double dblParenthMultiplier = 1.0d, double dblDashMultiplierPrior = 1.0d, double dblBracketMultiplierPrior = 1.0d, short intParenthLevelPrevious = 0)
        {

            // ( and ) are 40 and 41   - is 45   { and } are 123 and 125
            // Numbers are 48 to 57    . is 46
            // Uppercase letters are 65 to 90
            // Lowercase letters are 97 to 122
            // [ and ] are 91 and 93
            // ^ is 94
            // is 95

            int intAddonCount, intSymbolLength = default;
            var blnCaretPresent = default(bool);
            int intElementIndex, intNumLength;
            int intCharIndex, intMinusSymbolLoc;
            string strLeftHalf, strRightHalf;
            bool blnMatchFound;
            string strNewFormulaRightHalf;
            var udtComputationStatsRightHalf = new udtComputationStatsType();
            udtComputationStatsRightHalf.Initialize();
            var udtAbbrevSymbolStackRightHalf = new udtAbbrevSymbolStackType();
            var dblStdDevSumRightHalf = default(double);
            double dblCaretVal = default, dblAdjacentNum, dblCaretValDifference;
            double dblAtomCountToAdd;
            double dblBracketMultiplier;
            bool blnInsideBrackets;
            int intDashPos;
            double dblDashMultiplier;
            float sngChargeSaved;
            string strChar1 = string.Empty;
            string strChar3;
            string strChar2, strCharRemain;
            string strFormulaExcerpt;
            string strCharVal;
            int intCharAsc;
            int LoneCarbonOrSilicon;
            double dblIsoDifferenceTop, dblIsoDifferenceBottom;
            short SymbolReference = default, PrevSymbolReference = default;
            string strNewFormula, strReplace, strSubFormula;
            int intParenthClose, intParenthLevel = default;
            int intExpandAbbrevAdd;
            try
            {
                dblDashMultiplier = dblDashMultiplierPrior; // Leading coefficient position and default value
                dblBracketMultiplier = dblBracketMultiplierPrior; // Bracket correction factor
                blnInsideBrackets = false; // Switch for in or out of brackets
                intDashPos = 0;
                strNewFormula = string.Empty;
                strNewFormulaRightHalf = string.Empty;
                LoneCarbonOrSilicon = 0; // The number of carbon or silicon atoms
                CarbonOrSiliconReturnCount = 0;

                // Look for the > symbol
                // If found, this means take First Part minus the Second Part
                intMinusSymbolLoc = Strings.InStr(strFormula, ">");
                if (intMinusSymbolLoc > 0)
                {
                    // Look for the first occurrence of >
                    intCharIndex = 1;
                    blnMatchFound = false;
                    do
                    {
                        if (Strings.Mid(strFormula, intCharIndex, 1) == ">")
                        {
                            blnMatchFound = true;
                            strLeftHalf = Strings.Left(strFormula, intCharIndex - 1);
                            strRightHalf = Strings.Mid(strFormula, intCharIndex + 1);

                            // Parse the first half
                            strNewFormula = ParseFormulaRecursive(strLeftHalf, ref udtComputationStats, ref udtAbbrevSymbolStack, blnExpandAbbreviations, ref dblStdDevSum, ref CarbonOrSiliconReturnCount, dblValueForX, intCharCountPrior, dblParenthMultiplier, dblDashMultiplier, dblBracketMultiplier, intParenthLevelPrevious);

                            // Parse the second half
                            InitializeComputationStats(ref udtComputationStatsRightHalf);
                            InitializeAbbrevSymbolStack(ref udtAbbrevSymbolStackRightHalf);
                            strNewFormulaRightHalf = ParseFormulaRecursive(strRightHalf, ref udtComputationStatsRightHalf, ref udtAbbrevSymbolStackRightHalf, blnExpandAbbreviations, ref dblStdDevSumRightHalf, ref CarbonOrSiliconReturnCount, dblValueForX, intCharCountPrior + intCharIndex, dblParenthMultiplier, dblDashMultiplier, dblBracketMultiplier, intParenthLevelPrevious);
                            break;
                        }

                        intCharIndex += 1;
                    }
                    while (intCharIndex <= Strings.Len(strFormula));
                    if (blnMatchFound)
                    {
                        // Update strFormula
                        strFormula = strNewFormula + ">" + strNewFormulaRightHalf;

                        // Update udtComputationStats by subtracting the atom counts of the first half minus the second half
                        // If any atom counts become < 0 then, then raise an error
                        for (intElementIndex = 1; intElementIndex <= ELEMENT_COUNT; intElementIndex++)
                        {
                            {
                                var withBlock = udtComputationStats.Elements[intElementIndex];
                                if (ElementStats[intElementIndex].Mass * withBlock.Count + withBlock.IsotopicCorrection >= ElementStats[intElementIndex].Mass * udtComputationStatsRightHalf.Elements[intElementIndex].Count + udtComputationStatsRightHalf.Elements[intElementIndex].IsotopicCorrection)
                                {
                                    withBlock.Count -= -udtComputationStatsRightHalf.Elements[intElementIndex].Count;
                                    if (withBlock.Count < 0d)
                                    {
                                        // This shouldn't happen
                                        Console.WriteLine(".Count is less than 0 in ParseFormulaRecursive; this shouldn't happen");
                                        withBlock.Count = 0d;
                                    }

                                    if (Math.Abs(udtComputationStatsRightHalf.Elements[intElementIndex].IsotopicCorrection) > float.Epsilon)
                                    {
                                        // This assertion is here simply because I want to check the code
                                        withBlock.IsotopicCorrection = withBlock.IsotopicCorrection - udtComputationStatsRightHalf.Elements[intElementIndex].IsotopicCorrection;
                                    }
                                }
                                else
                                {
                                    // Invalid Formula; raise error
                                    ErrorParams.ErrorID = 30;
                                    ErrorParams.ErrorPosition = intCharIndex;
                                }
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
                    intCharIndex = 1;
                    do
                    {
                        strChar1 = Strings.Mid(strFormula, intCharIndex, 1);
                        strChar2 = Strings.Mid(strFormula, intCharIndex + 1, 1);
                        strChar3 = Strings.Mid(strFormula, intCharIndex + 2, 1);
                        strCharRemain = Strings.Mid(strFormula, intCharIndex + 3);
                        if (gComputationOptions.CaseConversion != ccCaseConversionConstants.ccExactCase)
                            strChar1 = Strings.UCase(strChar1);
                        if (gComputationOptions.BracketsAsParentheses)
                        {
                            if (strChar1 == "[")
                                strChar1 = "(";
                            if (strChar1 == "]")
                                strChar1 = ")";
                        }

                        if (string.IsNullOrEmpty(strChar1))
                            strChar1 = Conversions.ToString(EMPTY_STRING_CHAR);
                        if (string.IsNullOrEmpty(strChar2))
                            strChar2 = Conversions.ToString(EMPTY_STRING_CHAR);
                        if (string.IsNullOrEmpty(strChar3))
                            strChar3 = Conversions.ToString(EMPTY_STRING_CHAR);
                        if (string.IsNullOrEmpty(strCharRemain))
                            strCharRemain = Conversions.ToString(EMPTY_STRING_CHAR);
                        strFormulaExcerpt = strChar1 + strChar2 + strChar3 + strCharRemain;

                        // Check for needed caution statements
                        CheckCaution(strFormulaExcerpt);
                        switch (Strings.Asc(strChar1))
                        {
                            case 40:
                            case 123: // (    Record its position
                                // See if a number is present just after the opening parenthesis
                                if (Information.IsNumeric(strChar2) || strChar2 == ".")
                                {
                                    // Misplaced number
                                    ErrorParams.ErrorID = 14;
                                    ErrorParams.ErrorPosition = intCharIndex;
                                }

                                if (ErrorParams.ErrorID == 0)
                                {
                                    // search for closing parenthesis
                                    intParenthLevel = 1;
                                    var loopTo = Strings.Len(strFormula);
                                    for (intParenthClose = intCharIndex + 1; intParenthClose <= loopTo; intParenthClose++)
                                    {
                                        switch (Strings.Mid(strFormula, intParenthClose, 1) ?? "")
                                        {
                                            case "(":
                                            case "{":
                                            case "[":
                                                // Another opening parentheses
                                                // increment parenthLevel
                                                if (!gComputationOptions.BracketsAsParentheses && Strings.Mid(strFormula, intParenthClose, 1) == "[")
                                                {
                                                }
                                                // Do not count the bracket
                                                else
                                                {
                                                    intParenthLevel += 1;
                                                }

                                                break;

                                            case ")":
                                            case "}":
                                            case "]":
                                                if (!gComputationOptions.BracketsAsParentheses && Strings.Mid(strFormula, intParenthClose, 1) == "]")
                                                {
                                                }
                                                // Do not count the bracket
                                                else
                                                {
                                                    intParenthLevel -= 1;
                                                    if (intParenthLevel == 0)
                                                    {
                                                        string argstrWork = Strings.Mid(strFormula, intParenthClose + 1);
                                                        dblAdjacentNum = ParseNum(ref argstrWork, out intNumLength);
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

                                                        strSubFormula = Strings.Mid(strFormula, intCharIndex + 1, intParenthClose - (intCharIndex + 1));

                                                        // Note, must pass dblParenthMultiplier * dblAdjacentNum to preserve previous parentheses stuff
                                                        strNewFormula = ParseFormulaRecursive(strSubFormula, ref udtComputationStats, ref udtAbbrevSymbolStack, blnExpandAbbreviations, ref dblStdDevSum, ref CarbonOrSiliconReturnCount, dblValueForX, intCharCountPrior + intCharIndex, dblParenthMultiplier * dblAdjacentNum, dblDashMultiplier, dblBracketMultiplier, (short)(intParenthLevelPrevious + 1));

                                                        // If expanding abbreviations, then strNewFormula might be longer than strFormula, must add this onto intCharIndex also
                                                        intExpandAbbrevAdd = Strings.Len(strNewFormula) - Strings.Len(strSubFormula);

                                                        // Must replace the part of the formula parsed with the strNewFormula part, in case the formula was expanded or elements were capitalized
                                                        strFormula = Strings.Left(strFormula, intCharIndex) + strNewFormula + Strings.Mid(strFormula, intParenthClose);
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
                                string argstrWork1 = strChar2 + strChar3 + strCharRemain;
                                dblAdjacentNum = ParseNum(ref argstrWork1, out intNumLength);
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
                                if (intCharIndex == 1)
                                {
                                    // Formula starts with a number -- multiply section by number (until next dash)
                                    dblAdjacentNum = ParseNum(ref strFormulaExcerpt, out intNumLength);
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
                                else if (NumberConverter.CDblSafe(Strings.Mid(strFormula, intCharIndex - 1, 1)) > 0d)
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
                                if (Strings.UCase(strChar2) == "X")
                                {
                                    if (strChar3 == "e")
                                    {
                                        string argstrWork2 = strChar2 + strChar3 + strCharRemain;
                                        dblAdjacentNum = ParseNum(ref argstrWork2, out intNumLength);
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
                                    string argstrWork3 = strChar2 + strChar3 + strCharRemain;
                                    dblAdjacentNum = ParseNum(ref argstrWork3, out intNumLength);
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
                                string argstrWork4 = strChar2 + strChar3 + strCharRemain;
                                dblAdjacentNum = ParseNum(ref argstrWork4, out intNumLength);
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
                                    case smtSymbolMatchTypeConstants.smtElement:
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
                                        string argstrWork5 = Strings.Mid(strFormula, intCharIndex + intSymbolLength);
                                        dblAdjacentNum = ParseNum(ref argstrWork5, out intNumLength);
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
                                            if (!blnCaretPresent)
                                            {
                                                dblAtomCountToAdd = dblAdjacentNum * dblBracketMultiplier * dblParenthMultiplier * dblDashMultiplier;
                                                {
                                                    var withBlock1 = udtComputationStats.Elements[SymbolReference];
                                                    withBlock1.Count = withBlock1.Count + dblAtomCountToAdd;
                                                    withBlock1.Used = true; // Element is present tag
                                                    dblStdDevSum += dblAtomCountToAdd * Math.Pow(ElementStats[SymbolReference].Uncertainty, 2d);
                                                }

                                                {
                                                    var withBlock2 = udtComputationStats;
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
                                                                withBlock2.Charge = (float)(withBlock2.Charge + dblAtomCountToAdd * -1);
                                                                break;
                                                            default:
                                                                withBlock2.Charge = (float)(withBlock2.Charge + dblAtomCountToAdd * ElementStats[SymbolReference].Charge);
                                                                break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        withBlock2.Charge = (float)(withBlock2.Charge + dblAtomCountToAdd * ElementStats[SymbolReference].Charge);
                                                    }
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
                                                dblIsoDifferenceTop = NumberConverter.CIntSafe(0.63d * SymbolReference + 6d);
                                                dblIsoDifferenceBottom = NumberConverter.CIntSafe(0.008d * Math.Pow(SymbolReference, 2d) - 0.4d * SymbolReference - 6d);
                                                dblCaretValDifference = dblCaretVal - SymbolReference * 2;
                                                if (dblCaretValDifference >= dblIsoDifferenceTop)
                                                {
                                                    // Probably too high isotopic mass
                                                    AddToCautionDescription(LookupMessage(660) + ": " + ElementStats[SymbolReference].Symbol + " - " + dblCaretVal.ToString() + " " + LookupMessage(665) + " " + ElementStats[SymbolReference].Mass.ToString());
                                                }
                                                else if (dblCaretVal < SymbolReference)
                                                {
                                                    // Definitely too low isotopic mass
                                                    AddToCautionDescription(LookupMessage(670) + ": " + ElementStats[SymbolReference].Symbol + " - " + SymbolReference.ToString() + " " + LookupMessage(675));
                                                }
                                                else if (dblCaretValDifference <= dblIsoDifferenceBottom)
                                                {
                                                    // Probably too low isotopic mass
                                                    AddToCautionDescription(LookupMessage(662) + ": " + ElementStats[SymbolReference].Symbol + " - " + dblCaretVal.ToString() + " " + LookupMessage(665) + " " + ElementStats[SymbolReference].Mass.ToString());
                                                }

                                                // Put in isotopic correction factor
                                                dblAtomCountToAdd = dblAdjacentNum * dblBracketMultiplier * dblParenthMultiplier * dblDashMultiplier;
                                                {
                                                    var withBlock3 = udtComputationStats.Elements[SymbolReference];
                                                    // Increment element counting bin
                                                    withBlock3.Count = withBlock3.Count + dblAtomCountToAdd;

                                                    // Store information in .Isotopes()
                                                    // Increment the isotope counting bin
                                                    withBlock3.IsotopeCount = (short)(withBlock3.IsotopeCount + 1);
                                                    if (Information.UBound(withBlock3.Isotopes) < withBlock3.IsotopeCount)
                                                    {
                                                        Array.Resize(ref withBlock3.Isotopes, Information.UBound(withBlock3.Isotopes) + 2 + 1);
                                                    }

                                                    {
                                                        var withBlock4 = withBlock3.Isotopes[withBlock3.IsotopeCount];
                                                        withBlock4.Count = withBlock4.Count + dblAtomCountToAdd;
                                                        withBlock4.Mass = dblCaretVal;
                                                    }

                                                    // Add correction amount to udtComputationStats.elements(SymbolReference).IsotopicCorrection
                                                    withBlock3.IsotopicCorrection = withBlock3.IsotopicCorrection + (dblCaretVal * dblAtomCountToAdd - ElementStats[SymbolReference].Mass * dblAtomCountToAdd);

                                                    // Set bit that element is present
                                                    withBlock3.Used = true;

                                                    // Assume no error in caret value, no need to change dblStdDevSum
                                                }

                                                // Reset blnCaretPresent
                                                blnCaretPresent = false;
                                            }

                                            if (gComputationOptions.CaseConversion == ccCaseConversionConstants.ccConvertCaseUp)
                                            {
                                                strFormula = Strings.Left(strFormula, intCharIndex - 1) + Strings.UCase(Strings.Mid(strFormula, intCharIndex, 1)) + Strings.Mid(strFormula, intCharIndex + 1);
                                            }

                                            intCharIndex += intAddonCount;
                                        }

                                        break;

                                    case smtSymbolMatchTypeConstants.smtAbbreviation:
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
                                            if (Strings.UCase(strChar1) == "D" && strChar2 != "y")
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
                                            intSymbolLength = Strings.Len(AbbrevStats[SymbolReference].Symbol);

                                            // Look for number after abbrev/amino
                                            string argstrWork6 = Strings.Mid(strFormula, intCharIndex + intSymbolLength);
                                            dblAdjacentNum = ParseNum(ref argstrWork6, out intNumLength);
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
                                            dblAtomCountToAdd = dblAdjacentNum * dblBracketMultiplier * dblParenthMultiplier * dblDashMultiplier;
                                            sngChargeSaved = (float)(udtComputationStats.Charge + dblAtomCountToAdd * AbbrevStats[SymbolReference].Charge);

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
                                                    strReplace = AbbrevStats[SymbolReference].Formula;

                                                    // Look for a number after the abbreviation or amino acid
                                                    string argstrWork7 = Strings.Mid(strFormula, intCharIndex + intSymbolLength);
                                                    dblAdjacentNum = ParseNum(ref argstrWork7, out intNumLength);
                                                    CatchParseNumError(dblAdjacentNum, intNumLength, intCharIndex, intSymbolLength);
                                                    if (Conversions.ToBoolean(Strings.InStr(strReplace, ">")))
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
                                                        if (intParenthLevelPrevious > 0 || intParenthLevel > 0 || intCharIndex + intSymbolLength <= Strings.Len(strFormula))
                                                        {
                                                            strReplace = ConvertFormulaToEmpirical(strReplace);
                                                        }
                                                    }

                                                    if (dblAdjacentNum < 0d)
                                                    {
                                                        // No number after abbreviation
                                                        strFormula = Strings.Left(strFormula, intCharIndex - 1) + strReplace + Strings.Mid(strFormula, intCharIndex + intSymbolLength);
                                                        intSymbolLength = Strings.Len(strReplace);
                                                        dblAdjacentNum = 1d;
                                                        intAddonCount = intSymbolLength - 1;
                                                    }
                                                    else
                                                    {
                                                        // Number after abbreviation -- must put abbreviation in parentheses
                                                        // Parentheses can handle integer or decimal number
                                                        strReplace = "(" + strReplace + ")";
                                                        strFormula = Strings.Left(strFormula, intCharIndex - 1) + strReplace + Strings.Mid(strFormula, intCharIndex + intSymbolLength);
                                                        intSymbolLength = Strings.Len(strReplace);
                                                        intAddonCount = intNumLength + intSymbolLength - 1;
                                                    }
                                                }

                                                if (gComputationOptions.CaseConversion == ccCaseConversionConstants.ccConvertCaseUp)
                                                {
                                                    strFormula = Strings.Left(strFormula, intCharIndex - 1) + Strings.UCase(Strings.Mid(strFormula, intCharIndex, 1)) + Strings.Mid(strFormula, intCharIndex + 1);
                                                }
                                            }
                                        }

                                        intCharIndex += intAddonCount;
                                        break;

                                    default:
                                        // Element not Found
                                        if (Strings.UCase(strChar1) == "X")
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
                                string argstrWork8 = strChar2 + strChar3 + strCharRemain;
                                dblAdjacentNum = ParseNum(ref argstrWork8, out intNumLength);
                                CatchParseNumError(dblAdjacentNum, intNumLength, intCharIndex, intSymbolLength);
                                if (ErrorParams.ErrorID != 0)
                                {
                                }
                                // Problem, don't go on.
                                else
                                {
                                    strCharVal = Strings.Mid(strFormula, intCharIndex + 1 + intNumLength, 1);
                                    if (Strings.Len(strCharVal) > 0)
                                        intCharAsc = Strings.Asc(strCharVal);
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
                                        if (Strings.Mid(strFormula, intCharIndex + 1, 1) == "-")
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
                                break;
                            // There shouldn't be anything else (except the ~ filler character). If there is, we'll just ignore it
                        }

                        if (intCharIndex == Strings.Len(strFormula))
                        {
                            // Need to make sure compounds are present after a leading coefficient after a dash
                            if (dblDashMultiplier > 0d)
                            {
                                if (intCharIndex != intDashPos)
                                {
                                }
                                // Things went fine, no need to set anything
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
                            intCharIndex = Strings.Len(strFormula);
                        }

                        intCharIndex += 1;
                    }
                    while (intCharIndex <= Strings.Len(strFormula));
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

                {
                    var withBlock5 = ErrorParams;
                    if (withBlock5.ErrorID != 0 && Strings.Len(withBlock5.ErrorCharacter) == 0)
                    {
                        if (string.IsNullOrEmpty(strChar1))
                            strChar1 = Conversions.ToString(EMPTY_STRING_CHAR);
                        withBlock5.ErrorCharacter = strChar1;
                        withBlock5.ErrorPosition = withBlock5.ErrorPosition + intCharCountPrior;
                    }
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
        /// <param name="strWork">Input/Output</param>
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
        private double ParseNum(ref string strWork, out int intNumLength, bool blnAllowNegative = false)
        {
            double ParseNumRet = default;
            string strWorking, strFoundNum;
            short intIndex, intDecPtCount;
            if (gComputationOptions.DecimalSeparator == default(char))
            {
                gComputationOptions.DecimalSeparator = MolecularWeightTool.DetermineDecimalPoint();
            }

            // Set intNumLength to -1 for now
            // If it doesn't get set to 0 (due to an error), it will get set to the
            // length of the matched number before exiting the sub
            intNumLength = -1;
            strFoundNum = string.Empty;
            if (string.IsNullOrEmpty(strWork))
                strWork = Conversions.ToString(EMPTY_STRING_CHAR);
            if ((Strings.Asc(Strings.Left(strWork, 1)) < 48 || Strings.Asc(Strings.Left(strWork, 1)) > 57) && Strings.Left(strWork, 1) != Conversions.ToString(gComputationOptions.DecimalSeparator) && !(Strings.Left(strWork, 1) == "-" && blnAllowNegative == true))
            {
                intNumLength = 0; // No number found
                ParseNumRet = -1;
            }
            else
            {
                // Start of string is a number or a decimal point, or (if allowed) a negative sign
                var loopTo = (short)Strings.Len(strWork);
                for (intIndex = 1; intIndex <= loopTo; intIndex++)
                {
                    strWorking = Strings.Mid(strWork, intIndex, 1);
                    if (Information.IsNumeric(strWorking) || strWorking == Conversions.ToString(gComputationOptions.DecimalSeparator) || blnAllowNegative == true && strWorking == "-")
                    {
                        strFoundNum += strWorking;
                    }
                    else
                    {
                        break;
                    }
                }

                if (Strings.Len(strFoundNum) == 0 || strFoundNum == Conversions.ToString(gComputationOptions.DecimalSeparator))
                {
                    // No number at all or (more likely) no number after decimal point
                    strFoundNum = (-3).ToString();
                    intNumLength = 0;
                }
                else
                {
                    // Check for more than one decimal point (. or ,)
                    intDecPtCount = 0;
                    var loopTo1 = (short)Strings.Len(strFoundNum);
                    for (intIndex = 1; intIndex <= loopTo1; intIndex++)
                    {
                        if (Strings.Mid(strFoundNum, intIndex, 1) == Conversions.ToString(gComputationOptions.DecimalSeparator))
                            intDecPtCount = (short)(intDecPtCount + 1);
                    }

                    if (intDecPtCount > 1)
                    {
                        // more than one intDecPtCount
                        strFoundNum = (-4).ToString();
                        intNumLength = 0;
                    }
                    else
                    {
                        // All is fine
                    }
                }

                if (intNumLength < 0)
                    intNumLength = (short)Strings.Len(strFoundNum);
                ParseNumRet = NumberConverter.CDblSafe(strFoundNum);
            }

            return ParseNumRet;
        }

        public string PlainTextToRtfInternal(string strWorkText)
        {
            return PlainTextToRtfInternal(strWorkText, false, true, false, 0);
        }

        public string PlainTextToRtfInternal(string strWorkText, bool calculatorMode)
        {
            return PlainTextToRtfInternal(strWorkText, calculatorMode, true, false, 0);
        }

        public string PlainTextToRtfInternal(string strWorkText, bool calculatorMode, bool blnHighlightCharFollowingPercentSign)
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
        public string PlainTextToRtfInternal(string strWorkText, bool calculatorMode, bool blnHighlightCharFollowingPercentSign, bool blnOverrideErrorID, int errorIDOverride)
        {
            string PlainTextToRtfInternalRet = default;
            string strWorkCharPrev, strWorkChar, strRTF;
            int intCharIndex, intCharIndex2;
            bool blnSuperFound;
            int errorID;

            // ReSharper disable CommentTypo

            // Rtf string must begin with {{\fonttbl{\f0\fcharset0\fprq2 Times New Roman;}}\pard\plain\fs25

            // and must end with } or {\fs30  }} if superscript was used

            // "{\super 3}C{\sub 6}H{\sub 6}{\fs30  }}"
            // strRTF = "{{\fonttbl{\f0\fcharset0\fprq2 " + rtfFormula(0).font + ";}}\pard\plain\fs25 ";
            // Old: strRTF = "{\rtf1\ansi\deff0\deftab720{\fonttbl{\f0\fswiss MS Sans Serif;}{\f1\froman\fcharset2 Symbol;}{\f2\froman\fcharset2 Times New Roman;}{\f3\froman " + lblMWT[0].FontName + ";}}{\colortbl\red0\green0\blue0;\red255\green0\blue0;}\deflang1033\pard\plain\f3\fs25 ";
            // old: strRTF = "{\rtf1\ansi\deff0\deftab720{\fonttbl{\f0\fswiss MS Sans Serif;}{\f1\froman\fcharset2 Symbol;}{\f2\froman " + lblMWT[0].FontName + ";}{\f3\fswiss\fprq2 System;}}{\colortbl\red0\green0\blue0;\red255\green0\blue0;}\deflang1033\pard\plain\f2\fs25 ";
            // f0                               f1                                 f2                          f3                               f4                      cf0 (black)        cf1 (red)          cf3 (white)
            // ReSharper disable StringLiteralTypo
            strRTF = @"{\rtf1\ansi\deff0\deftab720{\fonttbl{\f0\fswiss MS Sans Serif;}{\f1\froman\fcharset2 Symbol;}{\f2\froman " + gComputationOptions.RtfFontName + @";}{\f3\froman Times New Roman;}{\f4\fswiss\fprq2 System;}}{\colortbl\red0\green0\blue0;\red255\green0\blue0;\red255\green255\blue255;}\deflang1033\pard\plain\f2\fs" + Strings.Trim(Conversion.Str(NumberConverter.CShortSafe(gComputationOptions.RtfFontSize * 2.5d))) + " ";
            // ReSharper restore StringLiteralTypo

            // ReSharper restore CommentTypo

            if ((strWorkText ?? "") == (string.Empty ?? ""))
            {
                // Return a blank RTF string
                return strRTF + "}";
            }

            blnSuperFound = false;
            strWorkCharPrev = "";
            var loopTo = Strings.Len(strWorkText);
            for (intCharIndex = 1; intCharIndex <= loopTo; intCharIndex++)
            {
                strWorkChar = Strings.Mid(strWorkText, intCharIndex, 1);
                if (strWorkChar == "%" && blnHighlightCharFollowingPercentSign)
                {
                    // An error was found and marked by a % sign
                    // Highlight the character at the % sign, and remove the % sign
                    if (intCharIndex == Strings.Len(strWorkText))
                    {
                        // At end of line
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
                                for (intCharIndex2 = Strings.Len(strRTF); intCharIndex2 >= 2; intCharIndex2 -= 1)
                                {
                                    if (Strings.Mid(strRTF, intCharIndex2, 1) == "(")
                                    {
                                        strRTF = Strings.Left(strRTF, intCharIndex2 - 1) + @"{\cf1 (}" + Strings.Mid(strRTF, intCharIndex2 + 1);
                                        break;
                                    }
                                    else if (Strings.Mid(strRTF, intCharIndex2, 1) == "{")
                                    {
                                        strRTF = Strings.Left(strRTF, intCharIndex2 - 1) + @"{\cf1 \{}" + Strings.Mid(strRTF, intCharIndex2 + 1);
                                        break;
                                    }
                                }

                                break;

                            case 13:
                            case 15:
                                // Error involves a bracket, find last opening bracket, [
                                for (intCharIndex2 = Strings.Len(strRTF); intCharIndex2 >= 2; intCharIndex2 -= 1)
                                {
                                    if (Strings.Mid(strRTF, intCharIndex2, 1) == "[")
                                    {
                                        strRTF = Strings.Left(strRTF, intCharIndex2 - 1) + @"{\cf1 [}" + Strings.Mid(strRTF, intCharIndex2 + 1);
                                        break;
                                    }
                                }

                                break;

                            default:
                                break;
                                // Nothing to highlight
                        }
                    }
                    else
                    {
                        // Highlight next character and skip % sign
                        strWorkChar = Strings.Mid(strWorkText, intCharIndex + 1, 1);
                        // Handle curly brackets
                        if (strWorkChar == "{" || strWorkChar == "}")
                            strWorkChar = @"\" + strWorkChar;
                        strRTF = strRTF + @"{\cf1 " + strWorkChar + "}";
                        intCharIndex += 1;
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
                else if (strWorkChar == Conversions.ToString(EMPTY_STRING_CHAR))
                {
                }
                // skip it, the tilde sign is used to add additional height to the formula line when isotopes are used
                // If it's here from a previous time, we ignore it, adding it at the end if needed (if blnSuperFound = true)
                else if (Information.IsNumeric(strWorkChar) || strWorkChar == Conversions.ToString(gComputationOptions.DecimalSeparator))
                {
                    // Number or period, so super or subscript it if needed
                    if (intCharIndex == 1)
                    {
                        // at beginning of line, so leave it alone. Probably out of place
                        strRTF += strWorkChar;
                    }
                    else if (!calculatorMode && (char.IsLetter(Conversions.ToChar(strWorkCharPrev)) || strWorkCharPrev == ")" || strWorkCharPrev == @"\}" || strWorkCharPrev == "+" || strWorkCharPrev == "_" || Strings.Left(Strings.Right(strRTF, 6), 3) == "sub"))
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
                    else if (Strings.Left(Strings.Right(strRTF, 8), 5) == "super")
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
                }
                // Ignore it
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
                strRTF = strRTF + @"{\fs" + Strings.Trim(Conversion.Str(NumberConverter.CShortSafe(gComputationOptions.RtfFontSize * 3))) + @"\cf2 " + RTF_HEIGHT_ADJUST_CHAR + "}}";
            }
            else
            {
                strRTF += "}";
            }

            PlainTextToRtfInternalRet = strRTF;
            return PlainTextToRtfInternalRet;
        }

        /// <summary>
        /// Recomputes the Mass for all of the loaded abbreviations
        /// </summary>
        public void RecomputeAbbreviationMassesInternal()
        {
            for (int index = 1, loopTo = AbbrevAllCount; index <= loopTo; index++)
            {
                {
                    var withBlock = AbbrevStats[index];
                    withBlock.Mass = ComputeFormulaWeight(ref withBlock.Formula);
                }
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
            strAbbreviationSymbol = Strings.LCase(strAbbreviationSymbol);
            for (int index = 1, loopTo = AbbrevAllCount; index <= loopTo; index++)
            {
                if ((Strings.LCase(AbbrevStats[index].Symbol) ?? "") == (strAbbreviationSymbol ?? ""))
                {
                    RemoveAbbreviationByIDInternal(index);
                    blnRemoved = true;
                }
            }

            if (blnRemoved)
            {
                return 0;
            }
            else
            {
                return 1;
            }
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
                for (int indexRemove = abbreviationID, loopTo = AbbrevAllCount - 1; indexRemove <= loopTo; indexRemove++)
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
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Look for the caution statement and remove it
        /// </summary>
        /// <param name="strCautionSymbol"></param>
        /// <returns>0 if found and removed; 1 if error</returns>
        public int RemoveCautionStatementInternal(string strCautionSymbol)
        {
            short intIndex, intIndexRemove;
            var blnRemoved = default(bool);
            var loopTo = (short)CautionStatementCount;
            for (intIndex = 1; intIndex <= loopTo; intIndex++)
            {
                if ((CautionStatements[intIndex, 0] ?? "") == (strCautionSymbol ?? ""))
                {
                    var loopTo1 = (short)(CautionStatementCount - 1);
                    for (intIndexRemove = intIndex; intIndexRemove <= loopTo1; intIndexRemove++)
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
            else
            {
                return 1;
            }
        }

        public void ResetErrorParamsInternal()
        {
            {
                var withBlock = ErrorParams;
                withBlock.ErrorCharacter = "";
                withBlock.ErrorID = 0;
                withBlock.ErrorPosition = 0;
            }
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

        public string ReturnFormattedMassAndStdDev(double dblMass, double dblStdDev, bool blnIncludeStandardDeviation, bool blnIncludePctSign)
        {
            // Plan:
            // Round dblStdDev to 1 final digit.
            // Round dblMass to the appropriate place based on StdDev.

            // dblMass is the main number
            // dblStdDev is the standard deviation

            string strResult = string.Empty;
            string strStdDevShort;
            double dblRoundedStdDev, dblRoundedMain;
            string strWork;
            double dblWork;
            short intExponentValue;
            string strPctSign;
            try
            {
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
                    dblRoundedStdDev = Conversions.ToDouble(dblStdDev.ToString("0E+000"));

                    // Now round dblMass
                    // Simply divide dblMass by 10^Exponent of the Standard Deviation
                    // Next round
                    // Now multiply to get back the rounded dblMass
                    strWork = dblStdDev.ToString("0E+000");
                    strStdDevShort = Strings.Left(strWork, 1);
                    intExponentValue = NumberConverter.CShortSafe(Strings.Right(strWork, 4));
                    dblWork = dblMass / Math.Pow(10d, intExponentValue);
                    dblWork = Math.Round(dblWork, 0);
                    dblRoundedMain = dblWork * Math.Pow(10d, intExponentValue);
                    strWork = dblRoundedMain.ToString("0.0##E+00");
                    if (gComputationOptions.StdDevMode == smStdDevModeConstants.smShort)
                    {
                        // StdDevType Short (Type 0)
                        strResult = dblRoundedMain.ToString();
                        if (blnIncludeStandardDeviation)
                        {
                            strResult = strResult + "(" + '±' + strStdDevShort + ")";
                        }

                        strResult += strPctSign;
                    }
                    else if (gComputationOptions.StdDevMode == smStdDevModeConstants.smScientific)
                    {
                        // StdDevType Scientific (Type 1)
                        strResult = dblRoundedMain.ToString() + strPctSign;
                        if (blnIncludeStandardDeviation)
                        {
                            strResult += " (" + '±' + Strings.Trim(dblStdDev.ToString("0.000E+00")) + ")";
                        }
                    }
                    else
                    {
                        // StdDevType Decimal
                        strResult = dblMass.ToString("0.0####") + strPctSign;
                        if (blnIncludeStandardDeviation)
                        {
                            strResult += " (" + '±' + Strings.Trim(dblRoundedStdDev.ToString()) + ")";
                        }
                    }
                }

                return strResult;
            }
            catch (Exception ex)
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
            string strWork;
            double dblWork;
            int intExponentValue;

            // Round to nearest 1, 2, or 5 (or multiple of 10 thereof)
            // First, find the exponent of dblThisNum
            strWork = dblThisNum.ToString("0E+000");
            intExponentValue = NumberConverter.CIntSafe(Strings.Right(strWork, 4));
            dblWork = dblThisNum / Math.Pow(10d, intExponentValue);
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
            int intLoopCount;
            string strWork;
            double dblWork;
            int intExponentValue;

            // Find the exponent of MultipleValue
            strWork = MultipleValue.ToString("0E+000");
            intExponentValue = NumberConverter.CIntSafe(Strings.Right(strWork, 4));
            intLoopCount = 0;
            while (((dblValueToRound / MultipleValue).ToString().Trim() ?? "") != (Math.Round(dblValueToRound / MultipleValue, 0).ToString().Trim() ?? ""))
            {
                dblWork = dblValueToRound / Math.Pow(10d, intExponentValue);
                dblWork = Conversions.ToDouble(dblWork.ToString("0"));
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

        public int SetAbbreviationInternal(string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid)
        {
            return SetAbbreviationInternal(strSymbol, strFormula, sngCharge, blnIsAminoAcid, "", "", true);
        }

        public int SetAbbreviationInternal(string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid, string strOneLetterSymbol)
        {
            return SetAbbreviationInternal(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, "", true);
        }

        public int SetAbbreviationInternal(string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid, string strOneLetterSymbol, string strComment)
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
        public int SetAbbreviationInternal(string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid, string strOneLetterSymbol, string strComment, bool blnValidateFormula)
        {
            bool blnAlreadyPresent;
            var abbrevID = default(int);

            // See if the abbreviation is already present
            blnAlreadyPresent = false;
            for (int index = 1, loopTo = AbbrevAllCount; index <= loopTo; index++)
            {
                if ((Strings.UCase(AbbrevStats[index].Symbol) ?? "") == (Strings.UCase(strSymbol) ?? ""))
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

        public int SetAbbreviationByIDInternal(short intAbbrevID, string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid)
        {
            return SetAbbreviationByIDInternal(intAbbrevID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, "", "", true);
        }

        public int SetAbbreviationByIDInternal(short intAbbrevID, string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid, string strOneLetterSymbol)
        {
            return SetAbbreviationByIDInternal(intAbbrevID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, "", true);
        }

        public int SetAbbreviationByIDInternal(short intAbbrevID, string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid, string strOneLetterSymbol, string strComment)
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
        public int SetAbbreviationByIDInternal(short intAbbrevID, string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid, string strOneLetterSymbol, string strComment, bool blnValidateFormula)
        {
            var udtComputationStats = new udtComputationStatsType();
            udtComputationStats.Initialize();
            var udtAbbrevSymbolStack = new udtAbbrevSymbolStackType();
            var blnInvalidSymbolOrFormula = default(bool);
            smtSymbolMatchTypeConstants eSymbolMatchType;
            var intSymbolReference = default(short);
            ResetErrorParamsInternal();

            // Initialize the UDTs
            InitializeComputationStats(ref udtComputationStats);
            InitializeAbbrevSymbolStack(ref udtAbbrevSymbolStack);
            if (Strings.Len(strSymbol) < 1)
            {
                // Symbol length is 0
                ErrorParams.ErrorID = 192;
            }
            else if (Strings.Len(strSymbol) > MAX_ABBREV_LENGTH)
            {
                // Abbreviation symbol too long
                ErrorParams.ErrorID = 190;
            }
            else if (IsStringAllLetters(strSymbol))
            {
                if (Strings.Len(strFormula) > 0)
                {
                    // Convert symbol to proper case mode
                    strSymbol = Strings.UCase(Strings.Left(strSymbol, 1)) + Strings.LCase(Strings.Mid(strSymbol, 2));

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
                        eSymbolMatchType = CheckElemAndAbbrev(strSymbol, ref intSymbolReference);
                        if (eSymbolMatchType == smtSymbolMatchTypeConstants.smtElement)
                        {
                            if ((ElementStats[intSymbolReference].Symbol ?? "") == (strSymbol ?? ""))
                            {
                                blnInvalidSymbolOrFormula = true;
                            }
                        }

                        if (!blnInvalidSymbolOrFormula && blnValidateFormula)
                        {
                            // Make sure the abbreviation's formula is valid
                            // This will also auto-capitalize the formula if auto-capitalize is turned on
                            double argdblStdDevSum = 0d;
                            int argCarbonOrSiliconReturnCount = 0;
                            strFormula = ParseFormulaRecursive(strFormula, ref udtComputationStats, ref udtAbbrevSymbolStack, false, ref argdblStdDevSum, CarbonOrSiliconReturnCount: ref argCarbonOrSiliconReturnCount);
                            if (ErrorParams.ErrorID != 0)
                            {
                                // An error occurred while parsing
                                // Already present in ErrorParams.ErrorID
                                // We'll still add the formula, but mark it as invalid
                                blnInvalidSymbolOrFormula = true;
                            }
                        }

                        AddAbbreviationWork(intAbbrevID, strSymbol, ref strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, blnInvalidSymbolOrFormula);
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
            int intIndex;
            ResetErrorParamsInternal();
            if (Strings.Len(strSymbolCombo) >= 1 && Strings.Len(strSymbolCombo) <= MAX_ABBREV_LENGTH)
            {
                // Make sure all the characters in strSymbolCombo are letters
                if (IsStringAllLetters(strSymbolCombo))
                {
                    if (Strings.Len(strNewCautionStatement) > 0)
                    {
                        // See if strSymbolCombo is present in CautionStatements()
                        var loopTo = CautionStatementCount;
                        for (intIndex = 1; intIndex <= loopTo; intIndex++)
                        {
                            if ((CautionStatements[intIndex, 0] ?? "") == (strSymbolCombo ?? ""))
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
        public int SetElementInternal(string strSymbol, double dblMass, double dblUncertainty, float sngCharge, bool blnRecomputeAbbreviationMasses)
        {
            short intIndex;
            var blnFound = default(bool);
            for (intIndex = 1; intIndex <= ELEMENT_COUNT; intIndex++)
            {
                if ((Strings.LCase(strSymbol) ?? "") == (Strings.LCase(ElementStats[intIndex].Symbol) ?? ""))
                {
                    {
                        var withBlock = ElementStats[intIndex];
                        withBlock.Mass = dblMass;
                        withBlock.Uncertainty = dblUncertainty;
                        withBlock.Charge = sngCharge;
                    }

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
            else
            {
                return 1;
            }
        }

        public int SetElementIsotopesInternal(string strSymbol, short intIsotopeCount, ref double[] dblIsotopeMassesOneBased, ref float[] sngIsotopeAbundancesOneBased)
        {
            short intIndex, intIsotopeIndex;
            var blnFound = default(bool);
            for (intIndex = 1; intIndex <= ELEMENT_COUNT; intIndex++)
            {
                if ((Strings.LCase(strSymbol) ?? "") == (Strings.LCase(ElementStats[intIndex].Symbol) ?? ""))
                {
                    {
                        var withBlock = ElementStats[intIndex];
                        if (intIsotopeCount < 0)
                            intIsotopeCount = 0;
                        withBlock.IsotopeCount = intIsotopeCount;
                        var loopTo = withBlock.IsotopeCount;
                        for (intIsotopeIndex = 1; intIsotopeIndex <= loopTo; intIsotopeIndex++)
                        {
                            if (intIsotopeIndex > MAX_ISOTOPES)
                                break;
                            withBlock.Isotopes[intIsotopeIndex].Mass = dblIsotopeMassesOneBased[intIsotopeIndex];
                            withBlock.Isotopes[intIsotopeIndex].Abundance = sngIsotopeAbundancesOneBased[intIsotopeIndex];
                        }
                    }

                    blnFound = true;
                    break;
                }
            }

            if (blnFound)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        public void SetElementModeInternal(emElementModeConstants NewElementMode)
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
        public void SetElementModeInternal(emElementModeConstants NewElementMode, bool blnMemoryLoadElementValues)
        {
            try
            {
                if (NewElementMode >= emElementModeConstants.emAverageMass && NewElementMode <= emElementModeConstants.emIntegerMass)
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
            if (messageID >= 1 && messageID <= MESSAGE_STATEMENT_DIM_COUNT && Strings.Len(strNewMessage) > 0)
            {
                MessageStatements[messageID] = strNewMessage;
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private void ShellSortSymbols(int lowIndex, int highIndex)
        {
            int[] PointerArray;
            PointerArray = new int[highIndex + 1];
            string[,] SymbolsStore;
            SymbolsStore = new string[highIndex + 1, 2];

            // MasterSymbolsList starts at lowIndex
            for (int index = lowIndex, loopTo = highIndex; index <= loopTo; index++)
                PointerArray[index] = index;
            ShellSortSymbolsWork(ref PointerArray, lowIndex, highIndex);

            // Reassign MasterSymbolsList array according to PointerArray order
            // First, copy to a temporary array (I know it eats up memory, but I have no choice)
            for (int index = lowIndex, loopTo1 = highIndex; index <= loopTo1; index++)
            {
                SymbolsStore[index, 0] = MasterSymbolsList[index, 0];
                SymbolsStore[index, 1] = MasterSymbolsList[index, 1];
            }

            // Now, put them back into the MasterSymbolsList() array in the correct order
            // Use PointerArray() for this
            for (int index = lowIndex, loopTo2 = highIndex; index <= loopTo2; index++)
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
            int itemCount;
            int incrementAmount;
            int indexCompare;
            int pointerSwap;
            int Length1, Length2;

            // Sort PointerArray[lowIndex..highIndex] by comparing
            // Len(MasterSymbolsList(PointerArray(x)) and sorting by decreasing length
            // If same length, sort alphabetically (increasing)

            // Compute largest increment
            itemCount = highIndex - lowIndex + 1;
            incrementAmount = 1;
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
                for (int index = lowIndex + incrementAmount, loopTo = highIndex; index <= loopTo; index++)
                {
                    pointerSwap = PointerArray[index];
                    var loopTo1 = lowIndex;
                    for (indexCompare = index - incrementAmount; -incrementAmount >= 0 ? indexCompare <= loopTo1 : indexCompare >= loopTo1; indexCompare += -incrementAmount)
                    {
                        // Use <= to sort ascending; Use > to sort descending
                        // Sort by decreasing length
                        Length1 = Strings.Len(MasterSymbolsList[PointerArray[indexCompare], 0]);
                        Length2 = Strings.Len(MasterSymbolsList[pointerSwap, 0]);
                        if (Length1 > Length2)
                            break;
                        // If same length, sort alphabetically
                        if (Length1 == Length2)
                        {
                            if (Operators.CompareString(Strings.UCase(MasterSymbolsList[PointerArray[indexCompare], 0]), Strings.UCase(MasterSymbolsList[pointerSwap, 0]), false) <= 0)
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
            mShowErrorMessageDialogs = blnValue;
        }

        public void SortAbbreviationsInternal()
        {
            int lowIndex, highIndex;
            int itemCount;
            int incrementAmount;
            int indexCompare;
            udtAbbrevStatsType udtCompare;
            itemCount = AbbrevAllCount;
            lowIndex = 1;
            highIndex = itemCount;

            // sort array[lowIndex..highIndex]

            // compute largest increment
            itemCount = highIndex - lowIndex + 1;
            incrementAmount = 1;
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
                for (int index = lowIndex + incrementAmount, loopTo = highIndex; index <= loopTo; index++)
                {
                    udtCompare = AbbrevStats[index];
                    var loopTo1 = lowIndex;
                    for (indexCompare = index - incrementAmount; -incrementAmount >= 0 ? indexCompare <= loopTo1 : indexCompare >= loopTo1; indexCompare += -incrementAmount)
                    {
                        // Use <= to sort ascending; Use > to sort descending
                        if (Operators.CompareString(AbbrevStats[indexCompare].Symbol, udtCompare.Symbol, false) <= 0)
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
            while (Strings.Len(strWork) < intLength)
                strWork += " ";
            return strWork;
        }

        private string SpacePadFront(string strWork, short intLength)
        {
            while (Strings.Len(strWork) < intLength)
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
            bool blnDescriptionChanged = false;
            if ((strProgressStepDescription ?? "") != (mProgressStepDescription ?? ""))
            {
                blnDescriptionChanged = true;
            }

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
            short intAbbrevIndex;
            var intInvalidAbbreviationCount = default(short);
            var loopTo = AbbrevAllCount;
            for (intAbbrevIndex = 1; intAbbrevIndex <= loopTo; intAbbrevIndex++)
            {
                {
                    var withBlock = AbbrevStats[intAbbrevIndex];
                    SetAbbreviationByIDInternal(intAbbrevIndex, withBlock.Symbol, withBlock.Formula, withBlock.Charge, withBlock.IsAminoAcid, withBlock.OneLetterSymbol, withBlock.Comment, true);
                    if (withBlock.InvalidSymbolOrFormula)
                    {
                        intInvalidAbbreviationCount = (short)(intInvalidAbbreviationCount + 1);
                    }
                }
            }

            return intInvalidAbbreviationCount;
        }
    }
}
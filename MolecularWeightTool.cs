using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace MwtWinDll
{
    public class MolecularWeightTool
    {

        // Molecular Weight Calculator routines with ActiveX Class interfaces
        // Based on Molecular Weight Calculator, v6.20 code (VB6), written by Matthew Monroe 1995-2002
        //
        // ActiveX Dll version written by Matthew Monroe in Richland, WA (2002)
        // Ported to VB.NET by Nikša Blonder in Richland, WA (2005)

        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
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


        private const string PROGRAM_DATE = "January 17, 2020";

        /// <summary>
    /// Constructor, assumes the elements are using average masses
    /// </summary>
        public MolecularWeightTool()
        {
            mElementAndMassRoutines = new ElementAndMassTools();
            mElementAndMassRoutines.ProgressChanged += mElementAndMassRoutines_ProgressChanged;
            mElementAndMassRoutines.ProgressComplete += mElementAndMassRoutines_ProgressComplete;
            mElementAndMassRoutines.ProgressReset += mElementAndMassRoutines_ProgressReset;

            // LoadDefaults calls mElementAndMassRoutines.MemoryLoadAll, which is required prior to instantiating the Peptide class.
            // We need to get the three letter abbreviations defined prior to the Peptide class calling method UpdateStandardMasses
            if (!mDataInitialized)
                LoadDefaults();
            Compound = new Compound(mElementAndMassRoutines);
            Peptide = new Peptide(mElementAndMassRoutines);
            FormulaFinder = new FormulaFinder(mElementAndMassRoutines);
            CapFlow = new CapillaryFlow();
        }

        /// <summary>
    /// Constructor where the element mode can be defined
    /// </summary>
    /// <param name="elementMode">Mass mode for elements (average, monoisotopic, or integer)</param>
        public MolecularWeightTool(ElementAndMassTools.emElementModeConstants elementMode) : this()
        {
            SetElementMode(elementMode);
        }

        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public enum arAbbrevRecognitionModeConstants
        {
            arNormalOnly = 0,
            arNormalPlusAminoAcids = 1,
            arNoAbbreviations = 2
        }

        public enum esElementStatsConstants
        {
            esMass = 0,
            esUncertainty = 1,
            esCharge = 2
        }

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        private bool mDataInitialized;
        public Compound Compound;
        public Peptide Peptide;
        public FormulaFinder FormulaFinder;
        public CapillaryFlow CapFlow;

        private readonly ElementAndMassTools mElementAndMassRoutines;

        public event ProgressResetEventHandler ProgressReset;

        public delegate void ProgressResetEventHandler();

        public event ProgressChangedEventHandler ProgressChanged;

        public delegate void ProgressChangedEventHandler(string taskDescription, float percentComplete);     // PercentComplete ranges from 0 to 100, but can contain decimal percentage values

        public event ProgressCompleteEventHandler ProgressComplete;

        public delegate void ProgressCompleteEventHandler();

        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        /* TODO ERROR: Skipped RegionDirectiveTrivia */
        public arAbbrevRecognitionModeConstants AbbreviationRecognitionMode
        {
            get
            {
                return mElementAndMassRoutines.gComputationOptions.AbbrevRecognitionMode;
            }

            set
            {
                if (value >= arAbbrevRecognitionModeConstants.arNormalOnly & value <= arAbbrevRecognitionModeConstants.arNoAbbreviations)
                {
                    mElementAndMassRoutines.gComputationOptions.AbbrevRecognitionMode = value;
                    mElementAndMassRoutines.ConstructMasterSymbolsList();
                }
            }
        }

        public string AppDate
        {
            get
            {
                return PROGRAM_DATE;
            }
        }

        public string AppVersion
        {
            get
            {
                string strVersion;
                try
                {
                    strVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                catch (Exception ex)
                {
                    strVersion = "??.??.??.??";
                }

                return strVersion;
            }
        }

        public bool BracketsTreatedAsParentheses
        {
            get
            {
                return mElementAndMassRoutines.gComputationOptions.BracketsAsParentheses;
            }

            set
            {
                mElementAndMassRoutines.gComputationOptions.BracketsAsParentheses = value;
            }
        }

        public ElementAndMassTools.ccCaseConversionConstants CaseConversionMode
        {
            get
            {
                return mElementAndMassRoutines.gComputationOptions.CaseConversion;
            }

            set
            {
                if (value >= ElementAndMassTools.ccCaseConversionConstants.ccConvertCaseUp & value <= ElementAndMassTools.ccCaseConversionConstants.ccSmartCase)
                {
                    mElementAndMassRoutines.gComputationOptions.CaseConversion = value;
                }
            }
        }

        public char DecimalSeparator
        {
            get
            {
                return mElementAndMassRoutines.gComputationOptions.DecimalSeparator;
            }

            set
            {
                mElementAndMassRoutines.gComputationOptions.DecimalSeparator = value;
            }
        }

        public string ErrorDescription
        {
            get
            {
                return mElementAndMassRoutines.GetErrorDescription();
            }
        }

        public int ErrorID
        {
            get
            {
                return mElementAndMassRoutines.GetErrorID();
            }
        }

        public string ErrorCharacter
        {
            get
            {
                return mElementAndMassRoutines.GetErrorCharacter();
            }
        }

        public int ErrorPosition
        {
            get
            {
                return mElementAndMassRoutines.GetErrorPosition();
            }
        }

        public string LogFilePath
        {
            get
            {
                return mElementAndMassRoutines.LogFilePath;
            }
        }

        public string LogFolderPath
        {
            get
            {
                return mElementAndMassRoutines.LogFolderPath;
            }

            set
            {
                mElementAndMassRoutines.LogFolderPath = value;
            }
        }

        public bool LogMessagesToFile
        {
            get
            {
                return mElementAndMassRoutines.LogMessagesToFile;
            }

            set
            {
                mElementAndMassRoutines.LogMessagesToFile = value;
            }
        }

        public virtual string ProgressStepDescription
        {
            get
            {
                return mElementAndMassRoutines.ProgressStepDescription;
            }
        }

        /// <summary>
    /// Percent complete: ranges from 0 to 100, but can contain decimal percentage values
    /// </summary>
    /// <returns></returns>
        public float ProgressPercentComplete
        {
            get
            {
                return mElementAndMassRoutines.ProgressPercentComplete;
            }
        }

        public string RtfFontName
        {
            get
            {
                return mElementAndMassRoutines.gComputationOptions.RtfFontName;
            }

            set
            {
                if (Strings.Len(value) > 0)
                {
                    mElementAndMassRoutines.gComputationOptions.RtfFontName = value;
                }
            }
        }

        public short RtfFontSize
        {
            get
            {
                return mElementAndMassRoutines.gComputationOptions.RtfFontSize;
            }

            set
            {
                if (value > 0)
                {
                    mElementAndMassRoutines.gComputationOptions.RtfFontSize = value;
                }
            }
        }

        public bool ShowErrorDialogs
        {
            get
            {
                return mElementAndMassRoutines.ShowErrorMessageDialogs;
            }

            set
            {
                mElementAndMassRoutines.SetShowErrorMessageDialogs(value);
            }
        }

        public ElementAndMassTools.smStdDevModeConstants StdDevMode
        {
            get
            {
                return mElementAndMassRoutines.gComputationOptions.StdDevMode;
            }

            set
            {
                if (value >= ElementAndMassTools.smStdDevModeConstants.smShort & value <= ElementAndMassTools.smStdDevModeConstants.smDecimal)
                {
                    mElementAndMassRoutines.gComputationOptions.StdDevMode = value;
                }
            }
        }
        /* TODO ERROR: Skipped EndRegionDirectiveTrivia */
        public void ClearError()
        {
            mElementAndMassRoutines.ResetErrorParamsInternal();
        }

        /// <summary>
    /// Compute the mass of a formula
    /// </summary>
    /// <param name="strFormula"></param>
    /// <returns>Mass of the formula</returns>
        public double ComputeMass(string strFormula)
        {

            // Simply assigning strFormula to .Formula will update the Mass
            Compound.Formula = strFormula;
            return Compound.get_Mass(false);
        }


        /// <summary>
    /// Computes the Isotopic Distribution for a formula
    /// </summary>
    /// <param name="strFormulaIn">Input/output: The properly formatted formula to parse</param>
    /// <param name="intChargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
    /// <param name="strResults">Output: Table of results</param>
    /// <param name="ConvolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
    /// <param name="ConvolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
    /// <returns>0 if success, -1 if an error</returns>
    /// <remarks>
    /// Returns uncharged mass values if intChargeState=0,
    /// Returns M+H values if intChargeState=1
    /// Returns convoluted m/z if intChargeState is > 1
    /// </remarks>
        public short ComputeIsotopicAbundances(ref string strFormulaIn, short intChargeState, ref string strResults, ref double[,] ConvolutedMSData2DOneBased, ref int ConvolutedMSDataCount)
        {
            return ComputeIsotopicAbundances(ref strFormulaIn, intChargeState, ref strResults, ref ConvolutedMSData2DOneBased, ref ConvolutedMSDataCount, "Isotopic Abundances for", "Mass", "Fraction", "Intensity");
        }


        /// <summary>
    /// Computes the Isotopic Distribution for a formula
    /// </summary>
    /// <param name="strFormulaIn">Input/output: The properly formatted formula to parse</param>
    /// <param name="intChargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
    /// <param name="strResults">Output: Table of results</param>
    /// <param name="ConvolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
    /// <param name="ConvolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
    /// <param name="blnAddProtonChargeCarrier">If blnAddProtonChargeCarrier is False, then still convolutes by charge, but doesn't add a proton</param>
    /// <returns>0 if success, -1 if an error</returns>
    /// <remarks>
    /// Returns uncharged mass values if intChargeState=0,
    /// Returns M+H values if intChargeState=1
    /// Returns convoluted m/z if intChargeState is > 1
    /// </remarks>
        public short ComputeIsotopicAbundances(ref string strFormulaIn, short intChargeState, ref string strResults, ref double[,] ConvolutedMSData2DOneBased, ref int ConvolutedMSDataCount, bool blnAddProtonChargeCarrier)
        {
            return ComputeIsotopicAbundances(ref strFormulaIn, intChargeState, ref strResults, ref ConvolutedMSData2DOneBased, ref ConvolutedMSDataCount, "Isotopic Abundances for", "Mass", "Fraction", "Intensity", blnAddProtonChargeCarrier);
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
    /// <returns>0 if success, -1 if an error</returns>
    /// <remarks>
    /// Returns uncharged mass values if intChargeState=0,
    /// Returns M+H values if intChargeState=1
    /// Returns convoluted m/z if intChargeState is > 1
    /// </remarks>
        public short ComputeIsotopicAbundances(ref string strFormulaIn, short intChargeState, ref string strResults, ref double[,] ConvolutedMSData2DOneBased, ref int ConvolutedMSDataCount, string strHeaderIsotopicAbundances, string strHeaderMassToCharge, string strHeaderFraction, string strHeaderIntensity)
        {
            bool blnAddProtonChargeCarrier = true;
            return mElementAndMassRoutines.ComputeIsotopicAbundancesInternal(ref strFormulaIn, intChargeState, ref strResults, ref ConvolutedMSData2DOneBased, ref ConvolutedMSDataCount, strHeaderIsotopicAbundances, strHeaderMassToCharge, strHeaderFraction, strHeaderIntensity, false, blnAddProtonChargeCarrier);
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
    /// <param name="blnAddProtonChargeCarrier">If blnAddProtonChargeCarrier is False, then still convolutes by charge, but doesn't add a proton</param>
    /// <returns>0 if success, -1 if an error</returns>
    /// <remarks>
    /// Returns uncharged mass values if intChargeState=0,
    /// Returns M+H values if intChargeState=1
    /// Returns convoluted m/z if intChargeState is > 1
    /// </remarks>
        public short ComputeIsotopicAbundances(ref string strFormulaIn, short intChargeState, ref string strResults, ref double[,] ConvolutedMSData2DOneBased, ref int ConvolutedMSDataCount, string strHeaderIsotopicAbundances, string strHeaderMassToCharge, string strHeaderFraction, string strHeaderIntensity, bool blnAddProtonChargeCarrier)
        {
            return mElementAndMassRoutines.ComputeIsotopicAbundancesInternal(ref strFormulaIn, intChargeState, ref strResults, ref ConvolutedMSData2DOneBased, ref ConvolutedMSDataCount, strHeaderIsotopicAbundances, strHeaderMassToCharge, strHeaderFraction, strHeaderIntensity, false, blnAddProtonChargeCarrier);
        }

        /// <summary>
    /// Convert the centroided data (stick data) in XYVals to a Gaussian representation
    /// </summary>
    /// <param name="XYVals">XY data, as key-value pairs</param>
    /// <param name="intResolution">Effective instrument resolution (e.g. 1000 or 20000)</param>
    /// <param name="dblResolutionMass">The m/z value at which the resolution applies</param>
    /// <returns>Gaussian spectrum data</returns>
    /// <remarks></remarks>
        public List<KeyValuePair<double, double>> ConvertStickDataToGaussian2DArray(List<KeyValuePair<double, double>> XYVals, int intResolution, double dblResolutionMass)
        {
            int intQualityFactor = 50;
            return ConvertStickDataToGaussian2DArray(XYVals, intResolution, dblResolutionMass, intQualityFactor);
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
            return mElementAndMassRoutines.ConvertStickDataToGaussian2DArray(XYVals, intResolution, dblResolutionMass, intQualityFactor);
        }

        /// <summary>
    /// Converts a given mass or m/z value to the MH+ m/z value
    /// </summary>
    /// <param name="dblMassMZ">Mass or m/z value</param>
    /// <param name="intCurrentCharge">Current charge (0 means neutral mass)</param>
    /// <returns></returns>
    /// <remarks></remarks>
        public double ConvoluteMass(double dblMassMZ, short intCurrentCharge)
        {
            return ConvoluteMass(dblMassMZ, intCurrentCharge, 1, 0d);
        }

        /// <summary>
    /// Converts a given mass or m/z value to the MH+ m/z value
    /// </summary>
    /// <param name="dblMassMZ">Mass or m/z value</param>
    /// <param name="intCurrentCharge">Current charge (0 means neutral mass)</param>
    /// <param name="intDesiredCharge">Desired charge (0 means neutral mass)</param>
    /// <returns></returns>
    /// <remarks></remarks>
        public double ConvoluteMass(double dblMassMZ, short intCurrentCharge, short intDesiredCharge)
        {
            return ConvoluteMass(dblMassMZ, intCurrentCharge, intDesiredCharge, 0d);
        }

        /// <summary>
    /// Converts a given mass or m/z value to the MH+ m/z value
    /// </summary>
    /// <param name="dblMassMZ">Mass or m/z value</param>
    /// <param name="intCurrentCharge">Current charge (0 means neutral mass)</param>
    /// <param name="intDesiredCharge">Desired charge (0 means neutral mass)</param>
    /// <param name="dblChargeCarrierMass">Custom charge carrier mass (default is 1.00727649)</param>
    /// <returns></returns>
    /// <remarks></remarks>
        public double ConvoluteMass(double dblMassMZ, short intCurrentCharge, short intDesiredCharge, double dblChargeCarrierMass)
        {
            return mElementAndMassRoutines.ConvoluteMassInternal(dblMassMZ, intCurrentCharge, intDesiredCharge, dblChargeCarrierMass);
        }

        /// <summary>
    /// Determine the decimal point symbol (period or comma)
    /// </summary>
    /// <returns></returns>
        internal static char DetermineDecimalPoint()
        {
            string strTestNumber;
            double sglConversionResult;

            // In VB6, the Trim(Str(Cdbl(...))) statement causes an error when the
            // user's computer is configured for using , for decimal points but not . for the
            // thousand's separator (instead, perhaps, using a space for thousands)
            // Not sure of the behavior in VB.NET

            try
            {
                // Determine what locale we're in (. or , for decimal point)
                strTestNumber = "5,500";
                sglConversionResult = Conversions.ToDouble(strTestNumber);
                if (Math.Abs(sglConversionResult - 5.5d) < float.Epsilon)
                {
                    // Use comma as Decimal point
                    return ',';
                }
                else
                {
                    // Use period as Decimal point
                    return '.';
                }
            }
            catch (Exception ex)
            {
                return '.';
            }
        }

        /// <summary>
    /// Get an abbreviation, by ID
    /// </summary>
    /// <param name="intAbbreviationID"></param>
    /// <param name="strSymbol">Output: symbol</param>
    /// <param name="strFormula">Output: empirical formula</param>
    /// <param name="sngCharge">Output: charge</param>
    /// <param name="blnIsAminoAcid">Output: true if an amino acid</param>
    /// <returns> 0 if success, 1 if failure</returns>
        public int GetAbbreviation(int intAbbreviationID, ref string strSymbol, ref string strFormula, ref float sngCharge, ref bool blnIsAminoAcid)
        {
            string argstrOneLetterSymbol = "";
            string argstrComment = "";
            bool argblnInvalidSymbolOrFormula = false;
            return GetAbbreviation(intAbbreviationID, ref strSymbol, ref strFormula, ref sngCharge, ref blnIsAminoAcid, ref argstrOneLetterSymbol, ref argstrComment, ref argblnInvalidSymbolOrFormula);
        }

        /// <summary>
    /// Get an abbreviation, by ID
    /// </summary>
    /// <param name="intAbbreviationID"></param>
    /// <param name="strSymbol">Output: symbol</param>
    /// <param name="strFormula">Output: empirical formula</param>
    /// <param name="sngCharge">Output: charge</param>
    /// <param name="blnIsAminoAcid">Output: true if an amino acid</param>
    /// <param name="strOneLetterSymbol">Output: one letter symbol (only used by amino acids)</param>
    /// <param name="strComment">Output: comment</param>
    /// <returns> 0 if success, 1 if failure</returns>
        public int GetAbbreviation(int intAbbreviationID, ref string strSymbol, ref string strFormula, ref float sngCharge, ref bool blnIsAminoAcid, ref string strOneLetterSymbol, ref string strComment)
        {
            bool argblnInvalidSymbolOrFormula = false;
            return GetAbbreviation(intAbbreviationID, ref strSymbol, ref strFormula, ref sngCharge, ref blnIsAminoAcid, ref strOneLetterSymbol, ref strComment, ref argblnInvalidSymbolOrFormula);
        }

        /// <summary>
    /// Get an abbreviation, by ID
    /// </summary>
    /// <param name="intAbbreviationID"></param>
    /// <param name="strSymbol">Output: symbol</param>
    /// <param name="strFormula">Output: empirical formula</param>
    /// <param name="sngCharge">Output: charge</param>
    /// <param name="blnIsAminoAcid">Output: true if an amino acid</param>
    /// <param name="strOneLetterSymbol">Output: one letter symbol (only used by amino acids)</param>
    /// <param name="strComment">Output: comment</param>
    /// <param name="blnInvalidSymbolOrFormula">Output: true if an invalid symbol or formula</param>
    /// <returns> 0 if success, 1 if failure</returns>
        public int GetAbbreviation(int intAbbreviationID, ref string strSymbol, ref string strFormula, ref float sngCharge, ref bool blnIsAminoAcid, ref string strOneLetterSymbol, ref string strComment, ref bool blnInvalidSymbolOrFormula)
        {
            return mElementAndMassRoutines.GetAbbreviationInternal(intAbbreviationID, out strSymbol, out strFormula, out sngCharge, out blnIsAminoAcid, out strOneLetterSymbol, out strComment, out blnInvalidSymbolOrFormula);
        }

        /// <summary>
    /// Get the number of abbreviations in memory
    /// </summary>
    /// <returns></returns>
        public int GetAbbreviationCount()
        {
            return mElementAndMassRoutines.GetAbbreviationCountInternal();
        }

        public int GetAbbreviationCountMax()
        {
            return ElementAndMassTools.MAX_ABBREV_COUNT;
        }

        /// <summary>
    /// Get the abbreviation ID for the given abbreviation symbol
    /// </summary>
    /// <param name="strSymbol"></param>
    /// <returns>ID if found, otherwise 0</returns>
        public int GetAbbreviationID(string strSymbol)
        {
            return mElementAndMassRoutines.GetAbbreviationIDInternal(strSymbol);
        }

        public string GetAminoAcidSymbolConversion(string strSymbolToFind, bool bln1LetterTo3Letter)
        {
            // If bln1LetterTo3Letter = True, then converting 1 letter codes to 3 letter codes
            // Returns the symbol, if found
            // Otherwise, returns ""
            return mElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(strSymbolToFind, bln1LetterTo3Letter);
        }

        /// <summary>
    /// Get caution statement information
    /// </summary>
    /// <param name="intCautionStatementID"></param>
    /// <param name="strSymbolCombo">Output: symbol combo for the caution statement</param>
    /// <param name="strCautionStatement">Output: caution statement text</param>
    /// <returns>0 if success, 1 if an invalid ID</returns>
        public int GetCautionStatement(int intCautionStatementID, ref string strSymbolCombo, ref string strCautionStatement)
        {
            return mElementAndMassRoutines.GetCautionStatementInternal(intCautionStatementID, out strSymbolCombo, out strCautionStatement);
        }

        /// <summary>
    /// Get the number of Caution Statements in memory
    /// </summary>
    /// <returns></returns>
        public int GetCautionStatementCount()
        {
            return mElementAndMassRoutines.GetCautionStatementCountInternal();
        }

        /// <summary>
    /// Get the caution statement ID for the given symbol combo
    /// </summary>
    /// <param name="strSymbolCombo"></param>
    /// <returns>Statement ID if found, otherwise -1</returns>
        public int GetCautionStatementID(string strSymbolCombo)
        {
            return mElementAndMassRoutines.GetCautionStatementIDInternal(strSymbolCombo);
        }

        public double GetChargeCarrierMass()
        {
            return mElementAndMassRoutines.GetChargeCarrierMassInternal();
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
        public int GetElement(short intElementID, ref string strSymbol, ref double dblMass, ref double dblUncertainty, ref float sngCharge, ref short intIsotopeCount)
        {
            return mElementAndMassRoutines.GetElementInternal(intElementID, out strSymbol, out dblMass, out dblUncertainty, out sngCharge, out intIsotopeCount);
        }

        /// <summary>
    /// Returns the number of elements in memory
    /// </summary>
    /// <returns></returns>
        public int GetElementCount()
        {
            return mElementAndMassRoutines.GetElementCountInternal();
        }

        /// <summary>
    /// Get the element ID for the given symbol
    /// </summary>
    /// <param name="strSymbol"></param>
    /// <returns>ID if found, otherwise 0</returns>
        public int GetElementID(string strSymbol)
        {
            return mElementAndMassRoutines.GetElementIDInternal(strSymbol);
        }

        /// <summary>
    /// Returns the isotope masses and abundances for the element with intElementID
    /// </summary>
    /// <param name="intElementID"></param>
    /// <param name="intIsotopeCount"></param>
    /// <param name="dblIsotopeMasses"></param>
    /// <param name="sngIsotopeAbundances"></param>
    /// <returns>0 if a valid ID, 1 if invalid</returns>
        public int GetElementIsotopes(short intElementID, ref short intIsotopeCount, ref double[] dblIsotopeMasses, ref float[] sngIsotopeAbundances)
        {
            return mElementAndMassRoutines.GetElementIsotopesInternal(intElementID, ref intIsotopeCount, ref dblIsotopeMasses, ref sngIsotopeAbundances);
        }

        /// <summary>
    /// Get the current element mode
    /// </summary>
    /// <returns>
    /// emAverageMass  = 1
    /// emIsotopicMass = 2
    /// emIntegerMass  = 3
    /// </returns>
        public ElementAndMassTools.emElementModeConstants GetElementMode()
        {
            return mElementAndMassRoutines.GetElementModeInternal();
        }

        /// <summary>
    /// Return the element symbol for the given element ID
    /// </summary>
    /// <param name="intElementID"></param>
    /// <returns></returns>
    /// <remarks>1 is Hydrogen, 2 is Helium, etc.</remarks>
        public string GetElementSymbol(short intElementID)
        {
            return mElementAndMassRoutines.GetElementSymbolInternal(intElementID);
        }

        /// <summary>
    /// Returns a single bit of information about a single element
    /// </summary>
    /// <param name="intElementID">Element ID</param>
    /// <param name="eElementStat">Value to obtain: mass, charge, or uncertainty</param>
    /// <returns></returns>
    /// <remarks>Since a value may be negative, simply returns 0 if an error</remarks>
        public double GetElementStat(short intElementID, esElementStatsConstants eElementStat)
        {
            return mElementAndMassRoutines.GetElementStatInternal(intElementID, eElementStat);
        }

        /// <summary>
    /// Get message text using message ID
    /// </summary>
    /// <param name="messageID"></param>
    /// <returns></returns>
        public string GetMessageStatement(int messageID)
        {
            return GetMessageStatement(messageID, string.Empty);
        }

        /// <summary>
    /// Get message text using message ID
    /// </summary>
    /// <param name="messageID"></param>
    /// <param name="strAppendText"></param>
    /// <returns></returns>
        public string GetMessageStatement(int messageID, string strAppendText)
        {
            return mElementAndMassRoutines.GetMessageStatementInternal(messageID, strAppendText);
        }

        public int GetMessageStatementCount()
        {
            return mElementAndMassRoutines.GetMessageStatementCountInternal();
        }

        /// <summary>
    /// Returns True if the first letter of strTestChar is a ModSymbol
    /// </summary>
    /// <param name="strSymbol"></param>
    /// <returns></returns>
    /// <remarks>
    /// Invalid Mod Symbols are letters, numbers, ., -, space, (, or )
    /// Valid Mod Symbols are ! # $ % ampersand ' * + ? ^ ` ~
    /// </remarks>
        public bool IsModSymbol(string strSymbol)
        {
            return mElementAndMassRoutines.IsModSymbolInternal(strSymbol);
        }

        private void LoadDefaults()
        {
            mElementAndMassRoutines.MemoryLoadAll(ElementAndMassTools.emElementModeConstants.emAverageMass);
            SetElementMode(ElementAndMassTools.emElementModeConstants.emAverageMass);
            AbbreviationRecognitionMode = arAbbrevRecognitionModeConstants.arNormalPlusAminoAcids;
            BracketsTreatedAsParentheses = true;
            CaseConversionMode = ElementAndMassTools.ccCaseConversionConstants.ccConvertCaseUp;
            DecimalSeparator = '.';
            RtfFontName = "Arial";
            RtfFontSize = 10;
            StdDevMode = ElementAndMassTools.smStdDevModeConstants.smDecimal;
            mElementAndMassRoutines.gComputationOptions.DecimalSeparator = DetermineDecimalPoint();
            mDataInitialized = true;
        }

        public void RemoveAllAbbreviations()
        {
            mElementAndMassRoutines.RemoveAllAbbreviationsInternal();
        }

        public void RemoveAllCautionStatements()
        {
            mElementAndMassRoutines.RemoveAllCautionStatementsInternal();
        }

        public double MassToPPM(double dblMassToConvert, double dblCurrentMZ)
        {
            return mElementAndMassRoutines.MassToPPMInternal(dblMassToConvert, dblCurrentMZ);
        }

        public double MonoMassToMZ(double dblMonoisotopicMass, short intCharge)
        {
            return MonoMassToMZ(dblMonoisotopicMass, intCharge, 0d);
        }

        public double MonoMassToMZ(double dblMonoisotopicMass, short intCharge, double dblChargeCarrierMass)
        {
            return mElementAndMassRoutines.MonoMassToMZInternal(dblMonoisotopicMass, intCharge, dblChargeCarrierMass);
        }

        /// <summary>
    /// Recomputes the Mass for all of the loaded abbreviations
    /// </summary>
    /// <remarks>
    /// Useful if we just finished setting lots of element masses, and
    /// had blnRecomputeAbbreviationMasses = False when calling .SetElement()
    /// </remarks>
        public void RecomputeAbbreviationMasses()
        {
            mElementAndMassRoutines.RecomputeAbbreviationMassesInternal();
        }

        public int RemoveAbbreviation(string strAbbreviationSymbol)
        {
            return mElementAndMassRoutines.RemoveAbbreviationInternal(strAbbreviationSymbol);
        }

        public int RemoveAbbreviationByID(int intAbbreviationID)
        {
            return mElementAndMassRoutines.RemoveAbbreviationByIDInternal(intAbbreviationID);
        }

        public int RemoveCautionStatement(string strCautionSymbol)
        {
            return mElementAndMassRoutines.RemoveCautionStatementInternal(strCautionSymbol);
        }

        public void ResetAbbreviations()
        {
            mElementAndMassRoutines.MemoryLoadAbbreviations();
        }

        public void ResetCautionStatements()
        {
            mElementAndMassRoutines.MemoryLoadCautionStatements();
        }

        public void ResetElement(short intElementID, esElementStatsConstants eSpecificStatToReset)
        {
            mElementAndMassRoutines.MemoryLoadElements(GetElementMode(), intElementID, eSpecificStatToReset);
        }

        public void ResetMessageStatements()
        {
            mElementAndMassRoutines.MemoryLoadMessageStatements();
        }

        public int SetAbbreviation(string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid)
        {
            return SetAbbreviation(strSymbol, strFormula, sngCharge, blnIsAminoAcid, "", "", true);
        }

        public int SetAbbreviation(string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid, string strOneLetterSymbol, string strComment)
        {
            return SetAbbreviation(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, true);
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
        public int SetAbbreviation(string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid, string strOneLetterSymbol, string strComment, bool blnValidateFormula)
        {
            return mElementAndMassRoutines.SetAbbreviationInternal(strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, blnValidateFormula);
        }

        public int SetAbbreviationByID(int intAbbrevID, string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid)
        {
            return SetAbbreviationByID(intAbbrevID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, "", "", true);
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
    /// <param name="blnValidateFormula">If true, make sure the formula is valid</param>
    /// <returns>0 if successful, otherwise an error ID</returns>
    /// <remarks>
    /// It is useful to set blnValidateFormula = False when you're defining all of the abbreviations at once,
    /// since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
    /// defined, then the parsing of the first abbreviation will fail
    /// </remarks>
        public int SetAbbreviationByID(int intAbbrevID, string strSymbol, string strFormula, float sngCharge, bool blnIsAminoAcid, string strOneLetterSymbol, string strComment, bool blnValidateFormula)
        {
            return mElementAndMassRoutines.SetAbbreviationByIDInternal((short)intAbbrevID, strSymbol, strFormula, sngCharge, blnIsAminoAcid, strOneLetterSymbol, strComment, blnValidateFormula);
        }

        /// <summary>
    /// Adds a new caution statement or updates an existing one (based on strSymbolCombo)
    /// </summary>
    /// <param name="strSymbolCombo"></param>
    /// <param name="strNewCautionStatement"></param>
    /// <returns>0 if successful, otherwise, returns an Error ID</returns>
        public int SetCautionStatement(string strSymbolCombo, string strNewCautionStatement)
        {
            return mElementAndMassRoutines.SetCautionStatementInternal(strSymbolCombo, strNewCautionStatement);
        }

        public void SetChargeCarrierMass(double dblMass)
        {
            mElementAndMassRoutines.SetChargeCarrierMassInternal(dblMass);
        }

        public int SetElement(string strSymbol, double dblMass, double dblUncertainty, float sngCharge)
        {
            return SetElement(strSymbol, dblMass, dblUncertainty, sngCharge, true);
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
        public int SetElement(string strSymbol, double dblMass, double dblUncertainty, float sngCharge, bool blnRecomputeAbbreviationMasses)
        {
            return mElementAndMassRoutines.SetElementInternal(strSymbol, dblMass, dblUncertainty, sngCharge, blnRecomputeAbbreviationMasses);
        }

        public int SetElementIsotopes(string strSymbol, short intIsotopeCount, ref double[] dblIsotopeMassesOneBased, ref float[] sngIsotopeAbundancesOneBased)
        {
            return mElementAndMassRoutines.SetElementIsotopesInternal(strSymbol, intIsotopeCount, ref dblIsotopeMassesOneBased, ref sngIsotopeAbundancesOneBased);
        }

        public void SetElementMode(ElementAndMassTools.emElementModeConstants elementMode)
        {
            SetElementMode(elementMode, true);
        }

        public void SetElementMode(ElementAndMassTools.emElementModeConstants elementMode, bool blnMemoryLoadElementValues)
        {
            mElementAndMassRoutines.SetElementModeInternal(elementMode, blnMemoryLoadElementValues);
        }

        /// <summary>
    /// Used to replace the default message strings with foreign language equivalent ones
    /// </summary>
    /// <param name="messageID"></param>
    /// <param name="strNewMessage"></param>
    /// <returns>0 if success; 1 if failure</returns>
        public int SetMessageStatement(int messageID, string strNewMessage)
        {
            return mElementAndMassRoutines.SetMessageStatementInternal(messageID, strNewMessage);
        }

        public void SortAbbreviations()
        {
            mElementAndMassRoutines.SortAbbreviationsInternal();
        }

        public string TextToRTF(string strTextToConvert)
        {
            return TextToRTF(strTextToConvert, false, true, false, 0);
        }

        public string TextToRTF(string strTextToConvert, bool CalculatorMode)
        {
            return TextToRTF(strTextToConvert, CalculatorMode, true, false, 0);
        }

        public string TextToRTF(string strTextToConvert, bool CalculatorMode, bool blnHighlightCharFollowingPercentSign)
        {
            return TextToRTF(strTextToConvert, CalculatorMode, blnHighlightCharFollowingPercentSign, false, 0);
        }

        /// <summary>
    /// Converts plain text to formatted rtf text
    /// </summary>
    /// <param name="strTextToConvert"></param>
    /// <param name="calculatorMode">When true, does not superscript + signs and numbers following + signs</param>
    /// <param name="blnHighlightCharFollowingPercentSign">When true, change the character following a percent sign to red (and remove the percent sign)</param>
    /// <param name="blnOverrideErrorID"></param>
    /// <param name="errorIDOverride"></param>
    /// <returns></returns>
        public string TextToRTF(string strTextToConvert, bool CalculatorMode, bool blnHighlightCharFollowingPercentSign, bool blnOverrideErrorID, int errorIDOverride)
        {
            return mElementAndMassRoutines.PlainTextToRtfInternal(strTextToConvert, CalculatorMode, blnHighlightCharFollowingPercentSign, blnOverrideErrorID, errorIDOverride);
        }

        /// <summary>
    /// Checks the formula of all abbreviations to make sure it's valid
    /// Marks any abbreviations as Invalid if a problem is found or a circular reference exists
    /// </summary>
    /// <returns>Count of the number of invalid abbreviations found</returns>
        public int ValidateAllAbbreviations()
        {
            return mElementAndMassRoutines.ValidateAllAbbreviationsInternal();
        }

        ~MolecularWeightTool()
        {
            Peptide = null;
            CapFlow = null;
            Compound = null;
        }

        private void mElementAndMassRoutines_ProgressChanged(string taskDescription, float percentComplete)
        {
            ProgressChanged?.Invoke(taskDescription, percentComplete);
        }

        private void mElementAndMassRoutines_ProgressComplete()
        {
            ProgressComplete?.Invoke();
        }

        private void mElementAndMassRoutines_ProgressReset()
        {
            ProgressReset?.Invoke();
        }
    }
}
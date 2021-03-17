using System;
using System.Collections.Generic;

namespace MolecularWeightCalculator
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
        public MolecularWeightTool(ElementAndMassTools.ElementMassMode elementMode) : this()
        {
            SetElementMode(elementMode);
        }

        #region "Constants and Enums"

        public enum AbbrevRecognitionMode
        {
            NormalOnly = 0,
            NormalPlusAminoAcids = 1,
            NoAbbreviations = 2
        }

        public enum ElementStatsType
        {
            Mass = 0,
            Uncertainty = 1,
            Charge = 2
        }

        #endregion

        #region "Classwide Variables"
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

        #endregion

        #region "Interface Functions"
        public AbbrevRecognitionMode AbbreviationRecognitionMode
        {
            get => mElementAndMassRoutines.gComputationOptions.AbbrevRecognitionMode;
            set
            {
                if (value >= AbbrevRecognitionMode.NormalOnly & value <= AbbrevRecognitionMode.NoAbbreviations)
                {
                    mElementAndMassRoutines.gComputationOptions.AbbrevRecognitionMode = value;
                    mElementAndMassRoutines.ConstructMasterSymbolsList();
                }
            }
        }

        public string AppDate => PROGRAM_DATE;

        public string AppVersion
        {
            get
            {
                string strVersion;
                try
                {
                    strVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                catch
                {
                    strVersion = "??.??.??.??";
                }

                return strVersion;
            }
        }

        public bool BracketsTreatedAsParentheses
        {
            get => mElementAndMassRoutines.gComputationOptions.BracketsAsParentheses;
            set => mElementAndMassRoutines.gComputationOptions.BracketsAsParentheses = value;
        }

        public ElementAndMassTools.CaseConversionMode CaseConversionMode
        {
            get => mElementAndMassRoutines.gComputationOptions.CaseConversion;
            set
            {
                if (value >= ElementAndMassTools.CaseConversionMode.ConvertCaseUp & value <= ElementAndMassTools.CaseConversionMode.SmartCase)
                {
                    mElementAndMassRoutines.gComputationOptions.CaseConversion = value;
                }
            }
        }

        public char DecimalSeparator
        {
            get => mElementAndMassRoutines.gComputationOptions.DecimalSeparator;
            set => mElementAndMassRoutines.gComputationOptions.DecimalSeparator = value;
        }

        public string ErrorDescription => mElementAndMassRoutines.GetErrorDescription();

        public int ErrorID => mElementAndMassRoutines.GetErrorID();

        public string ErrorCharacter => mElementAndMassRoutines.GetErrorCharacter();

        public int ErrorPosition => mElementAndMassRoutines.GetErrorPosition();

        public string LogFilePath => mElementAndMassRoutines.LogFilePath;

        public string LogFolderPath
        {
            get => mElementAndMassRoutines.LogFolderPath;
            set => mElementAndMassRoutines.LogFolderPath = value;
        }

        public bool LogMessagesToFile
        {
            get => mElementAndMassRoutines.LogMessagesToFile;
            set => mElementAndMassRoutines.LogMessagesToFile = value;
        }

        public virtual string ProgressStepDescription => mElementAndMassRoutines.ProgressStepDescription;

        /// <summary>
        /// Percent complete: ranges from 0 to 100, but can contain decimal percentage values
        /// </summary>
        /// <returns></returns>
        public float ProgressPercentComplete => mElementAndMassRoutines.ProgressPercentComplete;

        public string RtfFontName
        {
            get => mElementAndMassRoutines.gComputationOptions.RtfFontName;
            set
            {
                if (value.Length > 0)
                {
                    mElementAndMassRoutines.gComputationOptions.RtfFontName = value;
                }
            }
        }

        public short RtfFontSize
        {
            get => mElementAndMassRoutines.gComputationOptions.RtfFontSize;
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
            get => mElementAndMassRoutines.ShowErrorMessageDialogs;
            set => mElementAndMassRoutines.SetShowErrorMessageDialogs(value);
        }

        public ElementAndMassTools.StdDevMode StdDevMode
        {
            get => mElementAndMassRoutines.gComputationOptions.StdDevMode;
            set
            {
                if (value >= ElementAndMassTools.StdDevMode.Short & value <= ElementAndMassTools.StdDevMode.Decimal)
                {
                    mElementAndMassRoutines.gComputationOptions.StdDevMode = value;
                }
            }
        }
        #endregion

        public void ClearError()
        {
            mElementAndMassRoutines.ResetErrorParamsInternal();
        }

        /// <summary>
        /// Compute the mass of a formula
        /// </summary>
        /// <param name="formula"></param>
        /// <returns>Mass of the formula</returns>
        public double ComputeMass(string formula)
        {
            // Simply assigning strFormula to .Formula will update the Mass
            Compound.Formula = formula;
            return Compound.get_Mass(false);
        }

        /// <summary>
        /// Computes the Isotopic Distribution for a formula
        /// </summary>
        /// <param name="formulaIn">Input/output: The properly formatted formula to parse</param>
        /// <param name="chargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
        /// <param name="results">Output: Table of results</param>
        /// <param name="convolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
        /// <param name="convolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
        /// <returns>0 if success, -1 if an error</returns>
        /// <remarks>
        /// Returns uncharged mass values if <paramref name="chargeState"/>=0,
        /// Returns M+H values if <paramref name="chargeState"/>=1
        /// Returns convoluted m/z if <paramref name="chargeState"/> is &gt; 1
        /// </remarks>
        public short ComputeIsotopicAbundances(
            ref string formulaIn, short chargeState, ref string results,
            ref double[,] convolutedMSData2DOneBased, ref int convolutedMSDataCount)
        {
            return ComputeIsotopicAbundances(ref formulaIn, chargeState, ref results, ref convolutedMSData2DOneBased, ref convolutedMSDataCount,
                                             "Isotopic Abundances for", "Mass", "Fraction", "Intensity");
        }

        /// <summary>
        /// Computes the Isotopic Distribution for a formula
        /// </summary>
        /// <param name="formulaIn">Input/output: The properly formatted formula to parse</param>
        /// <param name="chargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
        /// <param name="results">Output: Table of results</param>
        /// <param name="convolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
        /// <param name="convolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
        /// <param name="addProtonChargeCarrier">If blnAddProtonChargeCarrier is False, then still convolutes by charge, but doesn't add a proton</param>
        /// <returns>0 if success, -1 if an error</returns>
        /// <remarks>
        /// Returns uncharged mass values if <paramref name="chargeState"/>=0,
        /// Returns M+H values if <paramref name="chargeState"/>=1
        /// Returns convoluted m/z if <paramref name="chargeState"/> is &gt; 1
        /// </remarks>
        public short ComputeIsotopicAbundances(
            ref string formulaIn, short chargeState, ref string results,
            ref double[,] convolutedMSData2DOneBased, ref int convolutedMSDataCount, bool addProtonChargeCarrier)
        {
            return ComputeIsotopicAbundances(ref formulaIn, chargeState, ref results, ref convolutedMSData2DOneBased, ref convolutedMSDataCount,
                                             "Isotopic Abundances for", "Mass", "Fraction", "Intensity", addProtonChargeCarrier);
        }

        /// <summary>
        /// Computes the Isotopic Distribution for a formula
        /// </summary>
        /// <param name="formulaIn">Input/output: The properly formatted formula to parse</param>
        /// <param name="chargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
        /// <param name="results">Output: Table of results</param>
        /// <param name="convolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
        /// <param name="convolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
        /// <param name="headerIsotopicAbundances">Header to use in <paramref name="results"/></param>
        /// <param name="headerMassToCharge">Header to use in <paramref name="results"/></param>
        /// <param name="headerFraction">Header to use in <paramref name="results"/></param>
        /// <param name="headerIntensity">Header to use in <paramref name="results"/></param>
        /// <returns>0 if success, -1 if an error</returns>
        /// <remarks>
        /// Returns uncharged mass values if <paramref name="chargeState"/>=0,
        /// Returns M+H values if <paramref name="chargeState"/>=1
        /// Returns convoluted m/z if <paramref name="chargeState"/> is &gt; 1
        /// </remarks>
        public short ComputeIsotopicAbundances(
            ref string formulaIn, short chargeState, ref string results,
            ref double[,] convolutedMSData2DOneBased, ref int convolutedMSDataCount,
            string headerIsotopicAbundances,
            string headerMassToCharge,
            string headerFraction,
            string headerIntensity)
        {
            const bool blnAddProtonChargeCarrier = true;
            return mElementAndMassRoutines.ComputeIsotopicAbundancesInternal(ref formulaIn, chargeState, ref results, ref convolutedMSData2DOneBased, ref convolutedMSDataCount, headerIsotopicAbundances, headerMassToCharge, headerFraction, headerIntensity, false, blnAddProtonChargeCarrier);
        }

        /// <summary>
        /// Computes the Isotopic Distribution for a formula
        /// </summary>
        /// <param name="formulaIn">Input/output: The properly formatted formula to parse</param>
        /// <param name="chargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
        /// <param name="results">Output: Table of results</param>
        /// <param name="convolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
        /// <param name="convolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
        /// <param name="headerIsotopicAbundances">Header to use in <paramref name="results"/></param>
        /// <param name="headerMassToCharge">Header to use in <paramref name="results"/></param>
        /// <param name="headerFraction">Header to use in <paramref name="results"/></param>
        /// <param name="headerIntensity">Header to use in <paramref name="results"/></param>
        /// <param name="addProtonChargeCarrier">If blnAddProtonChargeCarrier is False, then still convolutes by charge, but doesn't add a proton</param>
        /// <returns>0 if success, -1 if an error</returns>
        /// <remarks>
        /// Returns uncharged mass values if <paramref name="chargeState"/>=0,
        /// Returns M+H values if <paramref name="chargeState"/>=1
        /// Returns convoluted m/z if <paramref name="chargeState"/> is &gt; 1
        /// </remarks>
        public short ComputeIsotopicAbundances(
            ref string formulaIn, short chargeState, ref string results,
            ref double[,] convolutedMSData2DOneBased, ref int convolutedMSDataCount,
            string headerIsotopicAbundances,
            string headerMassToCharge,
            string headerFraction,
            string headerIntensity,
            bool addProtonChargeCarrier)
        {
            return mElementAndMassRoutines.ComputeIsotopicAbundancesInternal(ref formulaIn, chargeState, ref results, ref convolutedMSData2DOneBased, ref convolutedMSDataCount, headerIsotopicAbundances, headerMassToCharge, headerFraction, headerIntensity, false, addProtonChargeCarrier);
        }

        /// <summary>
        /// Convert the centroided data (stick data) in XYVals to a Gaussian representation
        /// </summary>
        /// <param name="xyVals">XY data, as key-value pairs</param>
        /// <param name="resolution">Effective instrument resolution (e.g. 1000 or 20000)</param>
        /// <param name="resolutionMass">The m/z value at which the resolution applies</param>
        /// <returns>Gaussian spectrum data</returns>
        /// <remarks></remarks>
        public List<KeyValuePair<double, double>> ConvertStickDataToGaussian2DArray(List<KeyValuePair<double, double>> xyVals, int resolution, double resolutionMass)
        {
            const int intQualityFactor = 50;
            return ConvertStickDataToGaussian2DArray(xyVals, resolution, resolutionMass, intQualityFactor);
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
            return mElementAndMassRoutines.ConvertStickDataToGaussian2DArray(xyVals, resolution, resolutionMass, qualityFactor);
        }

        /// <summary>
        /// Converts a given mass or m/z value to the MH+ m/z value
        /// </summary>
        /// <param name="massMz">Mass or m/z value</param>
        /// <param name="currentCharge">Current charge (0 means neutral mass)</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public double ConvoluteMass(double massMz, short currentCharge)
        {
            return ConvoluteMass(massMz, currentCharge, 1, 0d);
        }

        /// <summary>
        /// Converts a given mass or m/z value to the MH+ m/z value
        /// </summary>
        /// <param name="massMz">Mass or m/z value</param>
        /// <param name="currentCharge">Current charge (0 means neutral mass)</param>
        /// <param name="desiredCharge">Desired charge (0 means neutral mass)</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public double ConvoluteMass(double massMz, short currentCharge, short desiredCharge)
        {
            return ConvoluteMass(massMz, currentCharge, desiredCharge, 0d);
        }

        /// <summary>
        /// Converts a given mass or m/z value to the MH+ m/z value
        /// </summary>
        /// <param name="massMz">Mass or m/z value</param>
        /// <param name="currentCharge">Current charge (0 means neutral mass)</param>
        /// <param name="desiredCharge">Desired charge (0 means neutral mass)</param>
        /// <param name="chargeCarrierMass">Custom charge carrier mass (default is 1.00727649)</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public double ConvoluteMass(double massMz, short currentCharge, short desiredCharge, double chargeCarrierMass)
        {
            return mElementAndMassRoutines.ConvoluteMassInternal(massMz, currentCharge, desiredCharge, chargeCarrierMass);
        }

        /// <summary>
        /// Determine the decimal point symbol (period or comma)
        /// </summary>
        /// <returns></returns>
        internal static char DetermineDecimalPoint()
        {
            // In VB6, the Trim(Str(Cdbl(...))) statement causes an error when the
            // user's computer is configured for using , for decimal points but not . for the
            // thousand's separator (instead, perhaps, using a space for thousands)
            // Not sure of the behavior in VB.NET

            try
            {
                // Determine what locale we're in (. or , for decimal point)
                const string strTestNumber = "5,500";
                var sglConversionResult = double.Parse(strTestNumber);
                if (Math.Abs(sglConversionResult - 5.5d) < float.Epsilon)
                {
                    // Use comma as Decimal point
                    return ',';
                }

                // Use period as Decimal point
                return '.';
            }
            catch
            {
                return '.';
            }
        }

        /// <summary>
        /// Get an abbreviation, by ID
        /// </summary>
        /// <param name="abbreviationId"></param>
        /// <param name="symbol">Output: symbol</param>
        /// <param name="formula">Output: empirical formula</param>
        /// <param name="charge">Output: charge</param>
        /// <param name="isAminoAcid">Output: true if an amino acid</param>
        /// <returns> 0 if success, 1 if failure</returns>
        public int GetAbbreviation(int abbreviationId, out string symbol,
            out string formula, out float charge,
            out bool isAminoAcid)
        {
            return GetAbbreviation(abbreviationId, out symbol, out formula, out charge, out isAminoAcid, out _, out _, out _);
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
        /// <returns> 0 if success, 1 if failure</returns>
        public int GetAbbreviation(int abbreviationId, out string symbol,
            out string formula, out float charge,
            out bool isAminoAcid,
            out string oneLetterSymbol,
            out string comment)
        {
            return GetAbbreviation(abbreviationId, out symbol, out formula, out charge, out isAminoAcid, out oneLetterSymbol, out comment, out _);
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
        public int GetAbbreviation(int abbreviationId, out string symbol,
            out string formula, out float charge,
            out bool isAminoAcid,
            out string oneLetterSymbol,
            out string comment,
            out bool invalidSymbolOrFormula)
        {
            return mElementAndMassRoutines.GetAbbreviationInternal(
                abbreviationId, out symbol, out formula,
                out charge, out isAminoAcid, out oneLetterSymbol,
                out comment, out invalidSymbolOrFormula);
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
        /// <param name="symbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        public int GetAbbreviationID(string symbol)
        {
            return mElementAndMassRoutines.GetAbbreviationIDInternal(symbol);
        }

        public string GetAminoAcidSymbolConversion(string symbolToFind, bool oneLetterTo3Letter)
        {
            // If bln1LetterTo3Letter = True, then converting 1 letter codes to 3 letter codes
            // Returns the symbol, if found
            // Otherwise, returns ""
            return mElementAndMassRoutines.GetAminoAcidSymbolConversionInternal(symbolToFind, oneLetterTo3Letter);
        }

        /// <summary>
        /// Get caution statement information
        /// </summary>
        /// <param name="cautionStatementId"></param>
        /// <param name="symbolCombo">Output: symbol combo for the caution statement</param>
        /// <param name="cautionStatement">Output: caution statement text</param>
        /// <returns>0 if success, 1 if an invalid ID</returns>
        public int GetCautionStatement(int cautionStatementId, out string symbolCombo, out string cautionStatement)
        {
            return mElementAndMassRoutines.GetCautionStatementInternal(cautionStatementId, out symbolCombo, out cautionStatement);
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
        /// <param name="symbolCombo"></param>
        /// <returns>Statement ID if found, otherwise -1</returns>
        public int GetCautionStatementID(string symbolCombo)
        {
            return mElementAndMassRoutines.GetCautionStatementIDInternal(symbolCombo);
        }

        public double GetChargeCarrierMass()
        {
            return mElementAndMassRoutines.GetChargeCarrierMassInternal();
        }

        /// <summary>
        /// Returns the settings for the element with intElementID in the ByRef variables
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="symbol"></param>
        /// <param name="mass"></param>
        /// <param name="uncertainty"></param>
        /// <param name="charge"></param>
        /// <param name="isotopeCount"></param>
        /// <returns>0 if success, 1 if failure</returns>
        public int GetElement(short elementId, out string symbol, out double mass, out double uncertainty, out float charge, out short isotopeCount)
        {
            return mElementAndMassRoutines.GetElementInternal(elementId, out symbol, out mass, out uncertainty, out charge, out isotopeCount);
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
        /// <param name="symbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        public int GetElementID(string symbol)
        {
            return mElementAndMassRoutines.GetElementIDInternal(symbol);
        }

        /// <summary>
        /// Returns the isotope masses and abundances for the element with intElementID
        /// </summary>
        /// <param name="elementId"></param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses"></param>
        /// <param name="isotopeAbundances"></param>
        /// <returns>0 if a valid ID, 1 if invalid</returns>
        public int GetElementIsotopes(short elementId, ref short isotopeCount, ref double[] isotopeMasses, ref float[] isotopeAbundances)
        {
            return mElementAndMassRoutines.GetElementIsotopesInternal(elementId, ref isotopeCount, ref isotopeMasses, ref isotopeAbundances);
        }

        /// <summary>
        /// Get the current element mode
        /// </summary>
        /// <returns>
        /// emAverageMass  = 1
        /// emIsotopicMass = 2
        /// emIntegerMass  = 3
        /// </returns>
        public ElementAndMassTools.ElementMassMode GetElementMode()
        {
            return mElementAndMassRoutines.GetElementModeInternal();
        }

        /// <summary>
        /// Return the element symbol for the given element ID
        /// </summary>
        /// <param name="elementID"></param>
        /// <returns></returns>
        /// <remarks>1 is Hydrogen, 2 is Helium, etc.</remarks>
        public string GetElementSymbol(short elementID)
        {
            return mElementAndMassRoutines.GetElementSymbolInternal(elementID);
        }

        /// <summary>
        /// Returns a single bit of information about a single element
        /// </summary>
        /// <param name="elementId">Element ID</param>
        /// <param name="elementStat">Value to obtain: mass, charge, or uncertainty</param>
        /// <returns></returns>
        /// <remarks>Since a value may be negative, simply returns 0 if an error</remarks>
        public double GetElementStat(short elementId, ElementStatsType elementStat)
        {
            return mElementAndMassRoutines.GetElementStatInternal(elementId, elementStat);
        }

        /// <summary>
        /// Get message text using message ID
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public string GetMessageStatement(int messageId)
        {
            return GetMessageStatement(messageId, string.Empty);
        }

        /// <summary>
        /// Get message text using message ID
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="appendText"></param>
        /// <returns></returns>
        public string GetMessageStatement(int messageId, string appendText)
        {
            return mElementAndMassRoutines.GetMessageStatementInternal(messageId, appendText);
        }

        public int GetMessageStatementCount()
        {
            return mElementAndMassRoutines.GetMessageStatementCountInternal();
        }

        /// <summary>
        /// Returns True if the first letter of strTestChar is a ModSymbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        /// <remarks>
        /// Invalid Mod Symbols are letters, numbers, ., -, space, (, or )
        /// Valid Mod Symbols are ! # $ % ampersand ' * + ? ^ ` ~
        /// </remarks>
        public bool IsModSymbol(string symbol)
        {
            return mElementAndMassRoutines.IsModSymbolInternal(symbol);
        }

        private void LoadDefaults()
        {
            mElementAndMassRoutines.MemoryLoadAll(ElementAndMassTools.ElementMassMode.Average);

            SetElementMode(ElementAndMassTools.ElementMassMode.Average);
            AbbreviationRecognitionMode = AbbrevRecognitionMode.NormalPlusAminoAcids;
            BracketsTreatedAsParentheses = true;
            CaseConversionMode = ElementAndMassTools.CaseConversionMode.ConvertCaseUp;
            DecimalSeparator = '.';
            RtfFontName = "Arial";
            RtfFontSize = 10;
            StdDevMode = ElementAndMassTools.StdDevMode.Decimal;

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

        public double MassToPPM(double massToConvert, double currentMz)
        {
            return mElementAndMassRoutines.MassToPPMInternal(massToConvert, currentMz);
        }

        public double MonoMassToMZ(double monoisotopicMass, short charge)
        {
            return MonoMassToMZ(monoisotopicMass, charge, 0d);
        }

        public double MonoMassToMZ(double monoisotopicMass, short charge, double chargeCarrierMass)
        {
            return mElementAndMassRoutines.MonoMassToMZInternal(monoisotopicMass, charge, chargeCarrierMass);
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

        public int RemoveAbbreviation(string abbreviationSymbol)
        {
            return mElementAndMassRoutines.RemoveAbbreviationInternal(abbreviationSymbol);
        }

        public int RemoveAbbreviationByID(int abbreviationId)
        {
            return mElementAndMassRoutines.RemoveAbbreviationByIDInternal(abbreviationId);
        }

        public int RemoveCautionStatement(string cautionSymbol)
        {
            return mElementAndMassRoutines.RemoveCautionStatementInternal(cautionSymbol);
        }

        public void ResetAbbreviations()
        {
            mElementAndMassRoutines.MemoryLoadAbbreviations();
        }

        public void ResetCautionStatements()
        {
            mElementAndMassRoutines.MemoryLoadCautionStatements();
        }

        public void ResetElement(short elementId, ElementStatsType specificStatToReset)
        {
            mElementAndMassRoutines.MemoryLoadElements(GetElementMode(), elementId, specificStatToReset);
        }

        public void ResetMessageStatements()
        {
            mElementAndMassRoutines.MemoryLoadMessageStatements();
        }

        public int SetAbbreviation(string symbol, string formula, float charge,
            bool blnIsAminoAcid)
        {
            return SetAbbreviation(symbol, formula, charge, blnIsAminoAcid, "", "", true);
        }

        public int SetAbbreviation(string symbol, string formula, float charge,
            bool isAminoAcid,
            string oneLetterSymbol,
            string comment)
        {
            return SetAbbreviation(symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, true);
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
        /// It is useful to set <paramref name="validateFormula"/> = False when you're defining all of the abbreviations at once,
        /// since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        /// defined, then the parsing of the first abbreviation will fail
        /// </remarks>
        public int SetAbbreviation(
            string symbol, string formula, float charge,
            bool isAminoAcid,
            string oneLetterSymbol,
            string comment,
            bool validateFormula)
        {
            return mElementAndMassRoutines.SetAbbreviationInternal(symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, validateFormula);
        }

        public int SetAbbreviationByID(
            int abbrevId, string symbol, string formula,
            float charge, bool isAminoAcid)
        {
            return SetAbbreviationByID(abbrevId, symbol, formula, charge, isAminoAcid, "", "", true);
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
        /// <param name="validateFormula">If true, make sure the formula is valid</param>
        /// <returns>0 if successful, otherwise an error ID</returns>
        /// <remarks>
        /// It is useful to set <paramref name="validateFormula"/> = False when you're defining all of the abbreviations at once,
        /// since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        /// defined, then the parsing of the first abbreviation will fail
        /// </remarks>
        public int SetAbbreviationByID(
            int abbrevId, string symbol, string formula,
            float charge, bool isAminoAcid,
            string oneLetterSymbol,
            string comment,
            bool validateFormula)
        {
            return mElementAndMassRoutines.SetAbbreviationByIDInternal((short)abbrevId, symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, validateFormula);
        }

        /// <summary>
        /// Adds a new caution statement or updates an existing one (based on <paramref name="symbolCombo"/>)
        /// </summary>
        /// <param name="symbolCombo"></param>
        /// <param name="newCautionStatement"></param>
        /// <returns>0 if successful, otherwise, returns an Error ID</returns>
        public int SetCautionStatement(string symbolCombo, string newCautionStatement)
        {
            return mElementAndMassRoutines.SetCautionStatementInternal(symbolCombo, newCautionStatement);
        }

        public void SetChargeCarrierMass(double mass)
        {
            mElementAndMassRoutines.SetChargeCarrierMassInternal(mass);
        }

        public int SetElement(string symbol, double mass,
            double uncertainty, float charge)
        {
            return SetElement(symbol, mass, uncertainty, charge, true);
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
        public int SetElement(string symbol, double mass, double uncertainty,
            float charge,
            bool recomputeAbbreviationMasses)
        {
            return mElementAndMassRoutines.SetElementInternal(symbol, mass, uncertainty, charge, recomputeAbbreviationMasses);
        }

        public int SetElementIsotopes(string symbol, short isotopeCount, ref double[] isotopeMassesOneBased, ref float[] isotopeAbundancesOneBased)
        {
            return mElementAndMassRoutines.SetElementIsotopesInternal(symbol, isotopeCount, ref isotopeMassesOneBased, ref isotopeAbundancesOneBased);
        }

        public void SetElementMode(ElementAndMassTools.ElementMassMode elementMode)
        {
            SetElementMode(elementMode, true);
        }

        public void SetElementMode(ElementAndMassTools.ElementMassMode elementMode, bool memoryLoadElementValues)
        {
            mElementAndMassRoutines.SetElementModeInternal(elementMode, memoryLoadElementValues);
        }

        /// <summary>
        /// Used to replace the default message strings with foreign language equivalent ones
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="newMessage"></param>
        /// <returns>0 if success; 1 if failure</returns>
        public int SetMessageStatement(int messageId, string newMessage)
        {
            return mElementAndMassRoutines.SetMessageStatementInternal(messageId, newMessage);
        }

        public void SortAbbreviations()
        {
            mElementAndMassRoutines.SortAbbreviationsInternal();
        }

        public string TextToRTF(string textToConvert)
        {
            return TextToRTF(textToConvert, false, true, false, 0);
        }

        public string TextToRTF(string textToConvert, bool calculatorMode)
        {
            return TextToRTF(textToConvert, calculatorMode, true, false, 0);
        }

        public string TextToRTF(string textToConvert, bool calculatorMode,
            bool highlightCharFollowingPercentSign)
        {
            return TextToRTF(textToConvert, calculatorMode, highlightCharFollowingPercentSign, false, 0);
        }

        /// <summary>
        /// Converts plain text to formatted rtf text
        /// </summary>
        /// <param name="textToConvert"></param>
        /// <param name="calculatorMode">When true, does not superscript + signs and numbers following + signs</param>
        /// <param name="highlightCharFollowingPercentSign">When true, change the character following a percent sign to red (and remove the percent sign)</param>
        /// <param name="overrideErrorId"></param>
        /// <param name="errorIdOverride"></param>
        /// <returns></returns>
        public string TextToRTF(
            string textToConvert,
            bool calculatorMode,
            bool highlightCharFollowingPercentSign,
            bool overrideErrorId,
            int errorIdOverride)
        {
            return mElementAndMassRoutines.PlainTextToRtfInternal(textToConvert, calculatorMode, highlightCharFollowingPercentSign, overrideErrorId, errorIdOverride);
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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.FormulaFinder;
using MolecularWeightCalculator.Sequence;

namespace MolecularWeightCalculator
{
    [Guid("D8D174CF-429F-4848-B372-6554672A2F86"), ComVisible(true)]
    public enum AbbrevRecognitionMode
    {
        NormalOnly = 0,
        NormalPlusAminoAcids = 1,
        NoAbbreviations = 2
    }

    [Guid("2947251C-9281-49BD-8D68-30934337FADC"), ComVisible(true)]
    public enum ElementStatsType
    {
        Mass = 0,
        Uncertainty = 1,
        Charge = 2
    }

    [Guid("9BB6C2DE-493F-48E8-BD5A-EE70ACC23C75"), ClassInterface(ClassInterfaceType.None), ComSourceInterfaces(typeof(IMolecularWeightToolEvents)), ComVisible(true)]
    public class MolecularWeightTool : IMolecularWeightTool
    {
        // Molecular Weight Calculator routines with ActiveX Class interfaces
        // Based on Molecular Weight Calculator, v6.20 code (VB6), written by Matthew Monroe 1995-2002
        //
        // ActiveX Dll version written by Matthew Monroe in Richland, WA (2002)
        // Ported to VB.NET by Nikša Blonder in Richland, WA (2005)
        // Converted to C# by Bryson Gibbons in 2021

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

        // Ignore Spelling: Arial, Bryson, interop, Nikša, xyVals

        private const string PROGRAM_DATE = "April 14, 2021";

        /// <summary>
        /// Constructor, assumes the elements are using average masses
        /// </summary>
        public MolecularWeightTool() : this(ElementMassMode.Average)
        {
            // DO NOT combine this constructor with the other one, using a default parameter will block use from COM.
        }

        /// <summary>
        /// Constructor where the element mode can be defined
        /// </summary>
        /// <param name="elementMode">Mass mode for elements (average, monoisotopic, or integer)</param>
        public MolecularWeightTool(ElementMassMode elementMode)
        {
            mElementAndMassRoutines = new ElementAndMassTools();
            mElementAndMassRoutines.ProgressChanged += mElementAndMassRoutines_ProgressChanged;
            mElementAndMassRoutines.ProgressComplete += mElementAndMassRoutines_ProgressComplete;
            mElementAndMassRoutines.ProgressReset += mElementAndMassRoutines_ProgressReset;

            // Call mElementAndMassRoutines.MemoryLoadAll, which is required prior to instantiating the Peptide class.
            // We need to get the three letter abbreviations defined prior to the Peptide class calling method UpdateStandardMasses
            // Call it with Isotopic mass mode because that is what Peptide needs to update standard masses
            mElementAndMassRoutines.MemoryLoadAll(ElementMassMode.Isotopic);

            AbbreviationRecognitionMode = AbbrevRecognitionMode.NormalPlusAminoAcids;
            BracketsTreatedAsParentheses = true;
            CaseConversionMode = CaseConversionMode.ConvertCaseUp;
            DecimalSeparator = '.';
            RtfFontName = "Arial";
            RtfFontSize = 10;
            StdDevMode = StdDevMode.Decimal;

            mElementAndMassRoutines.ComputationOptions.DecimalSeparator = DetermineDecimalPoint();

            Peptide = new Peptide(mElementAndMassRoutines);

            // Does not re-load the elements or cause extra re-calculations because the masses loaded above were already the average masses
            // Use "false" to avoid re-loading all of the elements and isotopes unless it is actually necessary
            SetElementMode(elementMode, false);

            Compound = new Compound(mElementAndMassRoutines);
            FormulaFinder = new FormulaSearcher(mElementAndMassRoutines);

            CapFlow = new CapillaryFlow();
        }

        #region "Class wide Variables"

        public Compound Compound { get; set; }
        public Peptide Peptide { get; set; }
        public FormulaSearcher FormulaFinder { get; set; }
        public CapillaryFlow CapFlow { get; set; }

        private readonly ElementAndMassTools mElementAndMassRoutines;

        public event ProgressResetEventHandler ProgressReset;
        public event ProgressChangedEventHandler ProgressChanged;
        public event ProgressCompleteEventHandler ProgressComplete;

        #endregion

        #region "Interface Functions"
        public AbbrevRecognitionMode AbbreviationRecognitionMode
        {
            get => mElementAndMassRoutines.ComputationOptions.AbbrevRecognitionMode;
            set
            {
                mElementAndMassRoutines.ComputationOptions.AbbrevRecognitionMode = value;
                mElementAndMassRoutines.Elements.ConstructMasterSymbolsList();
            }
        }

        public string AppDate => PROGRAM_DATE;

        public string AppVersion
        {
            get
            {
                string version;
                try
                {
                    version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                catch
                {
                    version = "??.??.??.??";
                }

                return version;
            }
        }

        public bool BracketsTreatedAsParentheses
        {
            get => mElementAndMassRoutines.ComputationOptions.BracketsAsParentheses;
            set => mElementAndMassRoutines.ComputationOptions.BracketsAsParentheses = value;
        }

        public CaseConversionMode CaseConversionMode
        {
            get => mElementAndMassRoutines.ComputationOptions.CaseConversion;
            set => mElementAndMassRoutines.ComputationOptions.CaseConversion = value;
        }

        public char DecimalSeparator
        {
            get => mElementAndMassRoutines.ComputationOptions.DecimalSeparator;
            set => mElementAndMassRoutines.ComputationOptions.DecimalSeparator = value;
        }

        public string ErrorDescription => mElementAndMassRoutines.GetErrorDescription();

        public int ErrorId => mElementAndMassRoutines.GetErrorId();

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
        public float ProgressPercentComplete => mElementAndMassRoutines.ProgressPercentComplete;

        public string RtfFontName
        {
            get => mElementAndMassRoutines.ComputationOptions.RtfFontName;
            set
            {
                if (value.Length > 0)
                {
                    mElementAndMassRoutines.ComputationOptions.RtfFontName = value;
                }
            }
        }

        public short RtfFontSize
        {
            get => mElementAndMassRoutines.ComputationOptions.RtfFontSize;
            set
            {
                if (value > 0)
                {
                    mElementAndMassRoutines.ComputationOptions.RtfFontSize = value;
                }
            }
        }

        public bool ShowErrorDialogs
        {
            get => mElementAndMassRoutines.ShowErrorMessageDialogs;
            set => mElementAndMassRoutines.SetShowErrorMessageDialogs(value);
        }

        public StdDevMode StdDevMode
        {
            get => mElementAndMassRoutines.ComputationOptions.StdDevMode;
            set => mElementAndMassRoutines.ComputationOptions.StdDevMode = value;
        }
        #endregion

        public void ClearError()
        {
            mElementAndMassRoutines.ResetErrorParams();
        }

        /// <summary>
        /// Compute the mass of a formula
        /// </summary>
        /// <param name="formula"></param>
        /// <returns>Mass of the formula</returns>
        public double ComputeMass(string formula)
        {
            // Simply assigning formula to .Formula will update the Mass
            Compound.Formula = formula;
            return Compound.GetMass(false);
        }

        /// <summary>
        /// Compute the mass of a formula
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="formulaParseData">Parsing information from parsing the formula. Includes mass, stdDev, errors, and cautions.</param>
        /// <returns>Mass of the formula</returns>
        [ComVisible(false)]
        public double ComputeMassExtra(string formula, out IFormulaParseData formulaParseData)
        {
            // Simply assigning formula to .Formula will update the Mass
            Compound.Formula = formula;
            formulaParseData = Compound.FormulaParseData;
            return Compound.GetMass(false);
        }

        /// <summary>
        /// Computes the Isotopic Distribution for a formula
        /// </summary>
        /// <param name="formulaIn">Input/output: The properly formatted formula to parse</param>
        /// <param name="chargeState">0 for monoisotopic (uncharged) masses; 1 or higher for convoluted m/z values</param>
        /// <param name="results">Output: Table of results</param>
        /// <param name="convolutedMSData2D">2D array of MSData (mass and intensity pairs)</param>
        /// <param name="convolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
        /// <param name="addProtonChargeCarrier">If addProtonChargeCarrier is false, then still convolute by charge, but doesn't add a proton</param>
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
            ref string formulaIn, short chargeState, out string results,
            out double[,] convolutedMSData2D, out int convolutedMSDataCount,
            bool addProtonChargeCarrier = true,
            string headerIsotopicAbundances = "Isotopic Abundances for",
            string headerMassToCharge = "Mass",
            string headerFraction = "Fraction",
            string headerIntensity = "Intensity")
        {
            return mElementAndMassRoutines.ComputeIsotopicAbundances(ref formulaIn, chargeState, out results, out convolutedMSData2D, out convolutedMSDataCount, addProtonChargeCarrier, headerIsotopicAbundances, headerMassToCharge, headerFraction, headerIntensity, false);
        }

        /// <summary>
        /// Convert the centroided data (stick data) in xyVals to a Gaussian representation
        /// </summary>
        /// <param name="xyVals">XY data, as key-value pairs</param>
        /// <param name="resolution">Effective instrument resolution (e.g. 1000 or 20000)</param>
        /// <param name="resolutionMass">The m/z value at which the resolution applies</param>
        /// <param name="qualityFactor">Gaussian quality factor (between 1 and 75, default is 50)</param>
        /// <returns>Gaussian spectrum data</returns>
        public List<KeyValuePair<double, double>> ConvertStickDataToGaussian2DArray(List<KeyValuePair<double, double>> xyVals, int resolution, double resolutionMass, int qualityFactor = 50)
        {
            return mElementAndMassRoutines.ConvertStickDataToGaussian2DArray(xyVals, resolution, resolutionMass, qualityFactor);
        }

        /// <summary>
        /// Converts a given mass or m/z value to the MH+ m/z value
        /// </summary>
        /// <param name="massMz">Mass or m/z value</param>
        /// <param name="currentCharge">Current charge (0 means neutral mass)</param>
        /// <param name="desiredCharge">Desired charge (0 means neutral mass)</param>
        /// <param name="chargeCarrierMass">Custom charge carrier mass (0 means use default, usually 1.00727649)</param>
        public double ConvoluteMass(double massMz, short currentCharge, short desiredCharge = 1, double chargeCarrierMass = 0)
        {
            return mElementAndMassRoutines.ConvoluteMass(massMz, currentCharge, desiredCharge, chargeCarrierMass);
        }

        /// <summary>
        /// Determine the decimal point symbol (period or comma)
        /// </summary>
        internal static char DetermineDecimalPoint()
        {
            try
            {
                // Determine what locale we're in (. or , for decimal point)
                const string testNumber = "5,500";
                var conversionResult = double.Parse(testNumber);
                if (Math.Abs(conversionResult - 5.5d) < float.Epsilon)
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
            return mElementAndMassRoutines.Elements.GetAbbreviation(
                abbreviationId, out symbol, out formula,
                out charge, out isAminoAcid, out oneLetterSymbol,
                out comment, out invalidSymbolOrFormula);
        }

        /// <summary>
        /// Get the number of abbreviations in memory
        /// </summary>
        public int GetAbbreviationCount()
        {
            return mElementAndMassRoutines.Elements.GetAbbreviationCount();
        }

        public int GetAbbreviationCountMax()
        {
            return ElementsAndAbbrevs.MAX_ABBREV_COUNT;
        }

        /// <summary>
        /// Get the abbreviation ID for the given abbreviation symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>ID if found, otherwise -1</returns>
        public int GetAbbreviationId(string symbol)
        {
            return mElementAndMassRoutines.Elements.GetAbbreviationId(symbol);
        }

        public string GetAminoAcidSymbolConversion(string symbolToFind, bool oneLetterTo3Letter)
        {
            // If bln1LetterTo3Letter = True, then converting 1 letter codes to 3 letter codes
            // Returns the symbol, if found
            // Otherwise, returns ""
            return mElementAndMassRoutines.Elements.GetAminoAcidSymbolConversion(symbolToFind, oneLetterTo3Letter);
        }

        /// <summary>
        /// Get caution statement information
        /// </summary>
        /// <param name="symbolCombo">symbol combo for the caution statement</param>
        /// <param name="cautionStatement">Output: caution statement text</param>
        /// <returns>0 if success, 1 if an invalid ID</returns>
        public int GetCautionStatement(string symbolCombo, out string cautionStatement)
        {
            return mElementAndMassRoutines.Messages.GetCautionStatement(symbolCombo, out cautionStatement);
        }

        /// <summary>
        /// Get the number of Caution Statements in memory
        /// </summary>
        public int GetCautionStatementCount()
        {
            return mElementAndMassRoutines.Messages.GetCautionStatementCount();
        }

        /// <summary>
        /// Get the symbolCombos for Caution Statements in memory
        /// </summary>
        public List<string> GetCautionStatementSymbols()
        {
            return mElementAndMassRoutines.Messages.GetCautionStatementSymbols();
        }

        /// <summary>
        /// Get the symbolCombos for Caution Statements in memory as an array. This version is for supporting COM interop because COM does not support generic types
        /// </summary>
        public string[] GetCautionStatementSymbolsArray()
        {
            return GetCautionStatementSymbols().ToArray();
        }

        public double GetChargeCarrierMass()
        {
            return mElementAndMassRoutines.Elements.GetChargeCarrierMass();
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
        public int GetElement(short elementId, out string symbol, out double mass, out double uncertainty, out float charge, out short isotopeCount)
        {
            return mElementAndMassRoutines.Elements.GetElement(elementId, out symbol, out mass, out uncertainty, out charge, out isotopeCount);
        }

        /// <summary>
        /// Returns the number of elements in memory
        /// </summary>
        public int GetElementCount()
        {
            return mElementAndMassRoutines.Elements.GetElementCount();
        }

        /// <summary>
        /// Get the element ID for the given symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        public int GetElementId(string symbol)
        {
            return mElementAndMassRoutines.Elements.GetElementId(symbol);
        }

        /// <summary>
        /// Returns the isotope masses and abundances for the element with <paramref name="elementId"/>
        /// </summary>
        /// <param name="elementId">Element ID, or atomic number</param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses"></param>
        /// <param name="isotopeAbundances"></param>
        /// <returns>0 if a valid ID, 1 if invalid</returns>
        public int GetElementIsotopes(short elementId, out short isotopeCount, out double[] isotopeMasses, out float[] isotopeAbundances)
        {
            return mElementAndMassRoutines.Elements.GetElementIsotopes(elementId, out isotopeCount, out isotopeMasses, out isotopeAbundances);
        }

        /// <summary>
        /// Get the current element mode
        /// </summary>
        /// <returns>
        /// ElementMassMode.Average  = 1
        /// ElementMassMode.Isotopic = 2
        /// ElementMassMode.Integer  = 3
        /// </returns>
        public ElementMassMode GetElementMode()
        {
            return mElementAndMassRoutines.Elements.GetElementMode();
        }

        /// <summary>
        /// Return the element symbol for the given element ID
        /// </summary>
        /// <param name="elementId"></param>
        /// <remarks>1 is Hydrogen, 2 is Helium, etc.</remarks>
        public string GetElementSymbol(short elementId)
        {
            return mElementAndMassRoutines.Elements.GetElementSymbol(elementId);
        }

        /// <summary>
        /// Returns a single bit of information about a single element
        /// </summary>
        /// <param name="elementId">Element ID</param>
        /// <param name="elementStat">Value to obtain: mass, charge, or uncertainty</param>
        /// <remarks>Since a value may be negative, simply returns 0 if an error</remarks>
        public double GetElementStat(short elementId, ElementStatsType elementStat)
        {
            return mElementAndMassRoutines.Elements.GetElementStat(elementId, elementStat);
        }

        /// <summary>
        /// Get message text using message ID
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="appendText"></param>
        public string GetMessageStatement(int messageId, string appendText = "")
        {
            return mElementAndMassRoutines.Messages.GetMessageStatement(messageId, appendText);
        }

        public int GetMessageStatementMaxId()
        {
            return mElementAndMassRoutines.Messages.GetMessageStatementMaxId();
        }

        /// <summary>
        /// Returns True if the first letter of <paramref name="symbol"/> is a ModSymbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <remarks>
        /// Invalid Mod Symbols are letters, numbers, ., -, space, (, or )
        /// Valid Mod Symbols are ! # $ % ampersand ' * + ? ^ ` ~
        /// </remarks>
        public bool IsModSymbol(string symbol)
        {
            return mElementAndMassRoutines.IsModSymbol(symbol);
        }

        public void RemoveAllAbbreviations()
        {
            mElementAndMassRoutines.Elements.RemoveAllAbbreviations();
        }

        public void RemoveAllCautionStatements()
        {
            mElementAndMassRoutines.Messages.RemoveAllCautionStatements();
        }

        public double MassToPPM(double massToConvert, double currentMz)
        {
            return mElementAndMassRoutines.MassToPPM(massToConvert, currentMz);
        }

        public double MonoMassToMz(double monoisotopicMass, short charge, double chargeCarrierMass = 0)
        {
            return mElementAndMassRoutines.MonoMassToMz(monoisotopicMass, charge, chargeCarrierMass);
        }

        /// <summary>
        /// Recomputes the Mass for all of the loaded abbreviations
        /// </summary>
        /// <remarks>
        /// Useful if we just finished setting lots of element masses, and
        /// had recomputeAbbreviationMasses = false when calling .SetElement()
        /// </remarks>
        public void RecomputeAbbreviationMasses()
        {
            mElementAndMassRoutines.Elements.RecomputeAbbreviationMasses();
        }

        public int RemoveAbbreviation(string abbreviationSymbol)
        {
            return mElementAndMassRoutines.Elements.RemoveAbbreviation(abbreviationSymbol);
        }

        public int RemoveAbbreviationById(int abbreviationId)
        {
            return mElementAndMassRoutines.Elements.RemoveAbbreviationById(abbreviationId);
        }

        public int RemoveCautionStatement(string cautionSymbol)
        {
            return mElementAndMassRoutines.Messages.RemoveCautionStatement(cautionSymbol);
        }

        public void ResetAbbreviations()
        {
            mElementAndMassRoutines.Elements.MemoryLoadAbbreviations();
        }

        public void ResetCautionStatements()
        {
            mElementAndMassRoutines.Messages.MemoryLoadCautionStatements();
        }

        public void ResetElement(short elementId, ElementStatsType specificStatToReset)
        {
            mElementAndMassRoutines.Elements.MemoryLoadElements(GetElementMode(), elementId, specificStatToReset);
        }

        public void ResetMessageStatements()
        {
            mElementAndMassRoutines.Messages.MemoryLoadMessageStatements();
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
        /// <returns>0 if success, otherwise an error ID</returns>
        /// <remarks>
        /// It is useful to set <paramref name="validateFormula"/> = False when you're defining all of the abbreviations at once,
        /// since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        /// defined, then the parsing of the first abbreviation will fail
        /// </remarks>
        public int SetAbbreviation(
            string symbol, string formula, float charge,
            bool isAminoAcid,
            string oneLetterSymbol = "",
            string comment = "",
            bool validateFormula = true)
        {
            return mElementAndMassRoutines.Elements.SetAbbreviation(symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, validateFormula);
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
        /// <returns>0 if success, otherwise an error ID</returns>
        /// <remarks>
        /// It is useful to set <paramref name="validateFormula"/> = False when you're defining all of the abbreviations at once,
        /// since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        /// defined, then the parsing of the first abbreviation will fail
        /// </remarks>
        public int SetAbbreviationById(
            int abbrevId, string symbol, string formula,
            float charge, bool isAminoAcid,
            string oneLetterSymbol = "",
            string comment = "",
            bool validateFormula = true)
        {
            return mElementAndMassRoutines.Elements.SetAbbreviationById((short)abbrevId, symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, validateFormula);
        }

        /// <summary>
        /// Adds a new caution statement or updates an existing one (based on <paramref name="symbolCombo"/>)
        /// </summary>
        /// <param name="symbolCombo"></param>
        /// <param name="newCautionStatement"></param>
        /// <returns>0 if success, otherwise, returns an Error ID</returns>
        public int SetCautionStatement(string symbolCombo, string newCautionStatement)
        {
            return mElementAndMassRoutines.Messages.SetCautionStatement(symbolCombo, newCautionStatement);
        }

        public void SetChargeCarrierMass(double mass)
        {
            mElementAndMassRoutines.Elements.SetChargeCarrierMass(mass);
        }

        /// <summary>
        /// Update the values for a single element (based on <paramref name="symbol"/>)
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="mass"></param>
        /// <param name="uncertainty"></param>
        /// <param name="charge"></param>
        /// <param name="recomputeAbbreviationMasses">Set to false if updating several elements</param>
        public int SetElement(string symbol, double mass, double uncertainty,
            float charge,
            bool recomputeAbbreviationMasses = true)
        {
            return mElementAndMassRoutines.Elements.SetElement(symbol, mass, uncertainty, charge, recomputeAbbreviationMasses);
        }

        /// <summary>
        /// Set the isotopes for the element
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses">0-based array</param>
        /// <param name="isotopeAbundances">0-based array</param>
        public int SetElementIsotopes(string symbol, short isotopeCount, double[] isotopeMasses, float[] isotopeAbundances)
        {
            return mElementAndMassRoutines.Elements.SetElementIsotopes(symbol, isotopeCount, isotopeMasses, isotopeAbundances);
        }

        /// <summary>
        /// Set the element mode used for mass calculations
        /// </summary>
        /// <param name="elementMode"></param>
        /// <param name="forceMemoryLoadElementValues">Set to true if you want to force a reload of element weights, even if not changing element modes</param>
        public void SetElementMode(ElementMassMode elementMode, bool forceMemoryLoadElementValues = false)
        {
            mElementAndMassRoutines.Elements.SetElementMode(elementMode, forceMemoryLoadElementValues);
        }

        /// <summary>
        /// Used to replace the default message strings with foreign language equivalent ones
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="newMessage"></param>
        /// <returns>0 if success; 1 if failure</returns>
        public int SetMessageStatement(int messageId, string newMessage)
        {
            return mElementAndMassRoutines.Messages.SetMessageStatement(messageId, newMessage);
        }

        public void SortAbbreviations()
        {
            mElementAndMassRoutines.Elements.SortAbbreviations();
        }

        /// <summary>
        /// Converts plain text to formatted RTF text
        /// </summary>
        /// <param name="textToConvert"></param>
        /// <param name="calculatorMode">When true, does not superscript + signs and numbers following + signs</param>
        /// <param name="highlightCharFollowingPercentSign">When true, change the character following a percent sign to red (and remove the percent sign)</param>
        /// <param name="overrideErrorId"></param>
        /// <param name="errorIdOverride"></param>
        // ReSharper disable once InconsistentNaming
        public string TextToRTF(
            string textToConvert,
            bool calculatorMode = false,
            bool highlightCharFollowingPercentSign = true,
            bool overrideErrorId = false,
            int errorIdOverride = 0)
        {
            return mElementAndMassRoutines.PlainTextToRtf(textToConvert, calculatorMode, highlightCharFollowingPercentSign, overrideErrorId, errorIdOverride);
        }

        /// <summary>
        /// Checks the formula of all abbreviations to make sure it's valid
        /// Marks any abbreviations as Invalid if a problem is found or a circular reference exists
        /// </summary>
        /// <returns>Count of the number of invalid abbreviations found</returns>
        public int ValidateAllAbbreviations()
        {
            return mElementAndMassRoutines.Elements.ValidateAllAbbreviations();
        }

        /// <summary>
        /// Destructor
        /// </summary>
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
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.FormulaFinder;
using MolecularWeightCalculator.Sequence;

namespace MolecularWeightCalculator
{
    /// <summary>
    /// Abbreviation recognition modes
    /// </summary>
    [Guid("D8D174CF-429F-4848-B372-6554672A2F86"), ComVisible(true)]
    public enum AbbrevRecognitionMode
    {
        NormalOnly = 0,
        NormalPlusAminoAcids = 1,
        NoAbbreviations = 2
    }

    /// <summary>
    /// Element stats types
    /// </summary>
    [Guid("2947251C-9281-49BD-8D68-30934337FADC"), ComVisible(true)]
    public enum ElementStatsType
    {
        Mass = 0,
        Uncertainty = 1,
        Charge = 2
    }

    /// <summary>
    /// Molecular Weight Calculator routines with ActiveX Class interfaces
    /// </summary>
    /// <remarks>
    /// <para>
    /// Based on Molecular Weight Calculator, v6.20 code (VB6), written by Matthew Monroe 1995-2002
    /// </para>
    /// <para>
    /// ActiveX DLL version written by Matthew Monroe in Richland, WA (2002)
    /// Ported to VB.NET by Nikša Blonder in Richland, WA (2005)
    /// Converted to C# by Bryson Gibbons in 2021
    /// </para>
    /// </remarks>
    [Guid("9BB6C2DE-493F-48E8-BD5A-EE70ACC23C75"), ClassInterface(ClassInterfaceType.None), ComSourceInterfaces(typeof(IMolecularWeightToolEvents)), ComVisible(true)]
    public class MolecularWeightTool : IMolecularWeightTool
    {
        // -------------------------------------------------------------------------------
        // Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
        // E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
        // Website: https://github.com/PNNL-Comp-Mass-Spec/Molecular-Weight-Calculator-DLL
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

        // Ignore Spelling: Arial, Bryson, centroided, interop, Nikša, xyVals

        private const string PROGRAM_DATE = "April 19, 2021";

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
            ElementAndMass = new ElementAndMassTools();
            ElementAndMass.ProgressChanged += ElementAndMassRoutines_ProgressChanged;
            ElementAndMass.ProgressComplete += ElementAndMassRoutines_ProgressComplete;
            ElementAndMass.ProgressReset += ElementAndMassRoutines_ProgressReset;

            // Call mElementAndMassRoutines.MemoryLoadAll, which is required prior to instantiating the Peptide class.
            // We need to get the three letter abbreviations defined prior to the Peptide class calling method UpdateStandardMasses
            // Call it with Isotopic mass mode because that is what Peptide needs to update standard masses
            ElementAndMass.MemoryLoadAll(ElementMassMode.Isotopic);

            AbbreviationRecognitionMode = AbbrevRecognitionMode.NormalPlusAminoAcids;
            BracketsTreatedAsParentheses = true;
            CaseConversionMode = CaseConversionMode.ConvertCaseUp;
            DecimalSeparator = '.';
            RtfFontName = "Arial";
            RtfFontSize = 10;
            StdDevMode = StdDevMode.Decimal;

            ElementAndMass.ComputationOptions.DecimalSeparator = DetermineDecimalPoint();

            Peptide = new Peptide(ElementAndMass);

            // Does not re-load the elements or cause extra re-calculations because the masses loaded above were already the average masses
            // Use "false" to avoid re-loading all of the elements and isotopes unless it is actually necessary
            SetElementMode(elementMode, false);

            Compound = new Compound(ElementAndMass);
            FormulaFinder = new FormulaSearcher(ElementAndMass);

            CapFlow = new CapillaryFlow();
        }

        #region "Class wide Variables"

        /// <summary>
        /// Compound
        /// </summary>
        public Compound Compound { get; set; }

        /// <summary>
        /// Peptide
        /// </summary>
        public Peptide Peptide { get; set; }

        /// <summary>
        /// Formula finder
        /// </summary>
        public FormulaSearcher FormulaFinder { get; set; }

        /// <summary>
        /// Capillary flow calculator
        /// </summary>
        public CapillaryFlow CapFlow { get; set; }

        /// <summary>
        /// Expose this internally for use with the GUI
        /// </summary>
        internal ElementAndMassTools ElementAndMass { get; }

        /// <summary>
        /// Progress reset
        /// </summary>
        public event ProgressResetEventHandler ProgressReset;

        /// <summary>
        /// Progress updated
        /// </summary>
        public event ProgressChangedEventHandler ProgressChanged;

        /// <summary>
        /// Processing complete
        /// </summary>
        public event ProgressCompleteEventHandler ProgressComplete;

        #endregion

        #region "Interface Properties and Methods"

        /// <summary>
        /// Abbreviation recognition mode
        /// </summary>
        public AbbrevRecognitionMode AbbreviationRecognitionMode
        {
            get => ElementAndMass.ComputationOptions.AbbrevRecognitionMode;
            set
            {
                ElementAndMass.ComputationOptions.AbbrevRecognitionMode = value;
                ElementAndMass.Elements.ConstructMasterSymbolsList();
            }
        }

        /// <summary>
        /// Program release date
        /// </summary>
        public string AppDate => PROGRAM_DATE;

        /// <summary>
        /// Program version
        /// </summary>
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

        /// <summary>
        /// When true, brackets are treated as parentheses
        /// </summary>
        public bool BracketsTreatedAsParentheses
        {
            get => ElementAndMass.ComputationOptions.BracketsAsParentheses;
            set => ElementAndMass.ComputationOptions.BracketsAsParentheses = value;
        }

        /// <summary>
        /// Case conversion mode (when true, auto-capitalize)
        /// </summary>
        public CaseConversionMode CaseConversionMode
        {
            get => ElementAndMass.ComputationOptions.CaseConversion;
            set => ElementAndMass.ComputationOptions.CaseConversion = value;
        }

        /// <summary>
        /// Decimal separator
        /// </summary>
        public char DecimalSeparator
        {
            get => ElementAndMass.ComputationOptions.DecimalSeparator;
            set => ElementAndMass.ComputationOptions.DecimalSeparator = value;
        }

        /// <summary>
        /// Description of the most recent error
        /// </summary>
        public string ErrorDescription => ElementAndMass.GetErrorDescription();

        /// <summary>
        /// ID of the most recent error
        /// </summary>
        public int ErrorId => ElementAndMass.GetErrorId();

        /// <summary>
        /// Symbol used to indicate an error in a formula
        /// </summary>
        public string ErrorCharacter => ElementAndMass.GetErrorCharacter();

        /// <summary>
        /// Error position in a formula
        /// </summary>
        public int ErrorPosition => ElementAndMass.GetErrorPosition();

        /// <summary>
        /// Log file path
        /// </summary>
        public string LogFilePath => ElementAndMass.LogFilePath;

        /// <summary>
        /// Directory where the log file should be created
        /// </summary>
        public string LogFolderPath
        {
            get => ElementAndMass.LogFolderPath;
            set => ElementAndMass.LogFolderPath = value;
        }

        /// <summary>
        /// When true, log messages to a log file
        /// </summary>
        public bool LogMessagesToFile
        {
            get => ElementAndMass.LogMessagesToFile;
            set => ElementAndMass.LogMessagesToFile = value;
        }

        /// <summary>
        /// Description of the current processing task
        /// </summary>
        public virtual string ProgressStepDescription => ElementAndMass.ProgressStepDescription;

        /// <summary>
        /// Percent complete: ranges from 0 to 100, but can contain decimal percentage values
        /// </summary>
        public float ProgressPercentComplete => ElementAndMass.ProgressPercentComplete;

        /// <summary>
        /// Font to use when copying a formula to the clipboard as rich formatted text
        /// </summary>
        public string RtfFontName
        {
            get => ElementAndMass.ComputationOptions.RtfFontName;
            set
            {
                if (value.Length > 0)
                {
                    ElementAndMass.ComputationOptions.RtfFontName = value;
                }
            }
        }

        /// <summary>
        /// Font size to use when copying a formula to the clipboard as rich formatted text
        /// </summary>
        public short RtfFontSize
        {
            get => ElementAndMass.ComputationOptions.RtfFontSize;
            set
            {
                if (value > 0)
                {
                    ElementAndMass.ComputationOptions.RtfFontSize = value;
                }
            }
        }

        /// <summary>
        /// If true, when an error occurs, show a message box with the error message, requiring the user to click OK to continue
        /// </summary>
        public bool ShowErrorDialogs
        {
            get => ElementAndMass.ShowErrorMessageDialogs;
            set => ElementAndMass.SetShowErrorMessageDialogs(value);
        }

        /// <summary>
        /// Standard deviation mode
        /// </summary>
        public StdDevMode StdDevMode
        {
            get => ElementAndMass.ComputationOptions.StdDevMode;
            set => ElementAndMass.ComputationOptions.StdDevMode = value;
        }

        #endregion

        /// <summary>
        /// Reset the error ID to 0
        /// </summary>
        public void ClearError()
        {
            ElementAndMass.ResetErrorParams();
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
        /// <remarks>
        /// Return results will have uncharged mass values if <paramref name="chargeState"/>=0,
        /// Return results will have M+H values if <paramref name="chargeState"/>=1
        /// Return results will have convoluted m/z if <paramref name="chargeState"/> is &gt; 1
        /// </remarks>
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
        /// <returns>True if success, false if an error</returns>
        public bool ComputeIsotopicAbundances(
            ref string formulaIn, short chargeState, out string results,
            out double[,] convolutedMSData2D, out int convolutedMSDataCount,
            bool addProtonChargeCarrier = true,
            string headerIsotopicAbundances = "Isotopic Abundances for",
            string headerMassToCharge = "Mass",
            string headerFraction = "Fraction",
            string headerIntensity = "Intensity")
        {
            return ElementAndMass.ComputeIsotopicAbundances(
                ref formulaIn, chargeState, out results, out convolutedMSData2D, out convolutedMSDataCount, addProtonChargeCarrier,
                headerIsotopicAbundances, headerMassToCharge, headerFraction, headerIntensity, false);
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
            return ElementAndMass.ConvertStickDataToGaussian2DArray(xyVals, resolution, resolutionMass, qualityFactor);
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
            return ElementAndMass.ConvoluteMass(massMz, currentCharge, desiredCharge, chargeCarrierMass);
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
        /// <returns>True if success, false if abbreviationId is invalid</returns>
        public bool GetAbbreviation(int abbreviationId, out string symbol,
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
        /// <returns>True if success, false if abbreviationId is invalid</returns>
        public bool GetAbbreviation(int abbreviationId, out string symbol,
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
        /// <returns>True if success, false if abbreviationId is invalid</returns>
        public bool GetAbbreviation(int abbreviationId, out string symbol,
            out string formula, out float charge,
            out bool isAminoAcid,
            out string oneLetterSymbol,
            out string comment,
            out bool invalidSymbolOrFormula)
        {
            return ElementAndMass.Elements.GetAbbreviation(
                abbreviationId, out symbol, out formula,
                out charge, out isAminoAcid, out oneLetterSymbol,
                out comment, out invalidSymbolOrFormula);
        }

        /// <summary>
        /// Get the number of abbreviations in memory
        /// </summary>
        public int GetAbbreviationCount()
        {
            return ElementAndMass.Elements.GetAbbreviationCount();
        }

        /// <summary>
        /// Get the maximum number of abbreviations that can be stored
        /// </summary>
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
            return ElementAndMass.Elements.GetAbbreviationId(symbol);
        }

        /// <summary>
        /// Convert an amino acid symbol to 1-letter or 3-letter notation
        /// </summary>
        /// <param name="symbolToFind">Amino acid to find</param>
        /// <param name="oneLetterTo3Letter">If true, assume symbolToFind is a one-letter amino acid symbol and return the 3-letter symbol</param>
        /// <returns>1-letter or 3-letter amino acid symbol if found, otherwise an empty string</returns>
        public string GetAminoAcidSymbolConversion(string symbolToFind, bool oneLetterTo3Letter)
        {
            return ElementAndMass.Elements.GetAminoAcidSymbolConversion(symbolToFind, oneLetterTo3Letter);
        }

        /// <summary>
        /// Get caution statement information
        /// </summary>
        /// <param name="symbolCombo">symbol combo for the caution statement</param>
        /// <param name="cautionStatement">Output: caution statement text</param>
        /// <returns>True if success, false symbolCombo is not recognized</returns>
        public bool GetCautionStatement(string symbolCombo, out string cautionStatement)
        {
            return ElementAndMass.Messages.GetCautionStatement(symbolCombo, out cautionStatement);
        }

        /// <summary>
        /// Get the number of Caution Statements in memory
        /// </summary>
        public int GetCautionStatementCount()
        {
            return ElementAndMass.Messages.GetCautionStatementCount();
        }

        /// <summary>
        /// Get the symbolCombos for Caution Statements in memory
        /// </summary>
        public List<string> GetCautionStatementSymbols()
        {
            return ElementAndMass.Messages.GetCautionStatementSymbols();
        }

        /// <summary>
        /// Get the symbolCombos for Caution Statements in memory as an array. This version is for supporting COM interop because COM does not support generic types
        /// </summary>
        public string[] GetCautionStatementSymbolsArray()
        {
            return GetCautionStatementSymbols().ToArray();
        }

        /// <summary>
        /// Get the current charge carrier mass
        /// </summary>
        public double GetChargeCarrierMass()
        {
            return ElementAndMass.Elements.GetChargeCarrierMass();
        }

        /// <summary>
        /// Returns the settings for the element with <paramref name="atomicNumber"/> in the out variables
        /// </summary>
        /// <param name="atomicNumber">Element atomic number (1 for hydrogen, 2 for helium, etc.)</param>
        /// <param name="symbol">Element symbol</param>
        /// <param name="mass">Mass</param>
        /// <param name="uncertainty">Uncertainty of the mass</param>
        /// <param name="charge">Charge</param>
        /// <param name="isotopeCount">Number of isotopes</param>
        /// <returns>True if success, false if atomicNumber is invalid</returns>
        public bool GetElement(short atomicNumber, out string symbol, out double mass, out double uncertainty, out float charge, out short isotopeCount)
        {
            return ElementAndMass.Elements.GetElement(atomicNumber, out symbol, out mass, out uncertainty, out charge, out isotopeCount);
        }

        /// <summary>
        /// Returns the number of elements in memory
        /// </summary>
        public int GetElementCount()
        {
            return ElementAndMass.Elements.GetElementCount();
        }

        /// <summary>
        /// Get the element ID for the given symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        public int GetAtomicNumber(string symbol)
        {
            return ElementAndMass.Elements.GetAtomicNumber(symbol);
        }

        /// <summary>
        /// Returns the isotope masses and abundances for the element with <paramref name="atomicNumber"/>
        /// </summary>
        /// <param name="atomicNumber">Element atomic number (1 for hydrogen, 2 for helium, etc.)</param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses">output, 0-based array</param>
        /// <param name="isotopeAbundances">output, 0-based array</param>
        /// <returns>True if success, false if atomicNumber is invalid</returns>
        public bool GetElementIsotopes(short atomicNumber, out short isotopeCount, out double[] isotopeMasses, out float[] isotopeAbundances)
        {
            return ElementAndMass.Elements.GetElementIsotopes(atomicNumber, out isotopeCount, out isotopeMasses, out isotopeAbundances);
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
            return ElementAndMass.Elements.GetElementMode();
        }

        /// <summary>
        /// Return the element symbol for the given element ID
        /// </summary>
        /// <remarks>1 is Hydrogen, 2 is Helium, etc.</remarks>
        /// <param name="atomicNumber"></param>
        public string GetElementSymbol(short atomicNumber)
        {
            return ElementAndMass.Elements.GetElementSymbol(atomicNumber);
        }

        /// <summary>
        /// Returns a single bit of information about a single element
        /// </summary>
        /// <remarks>Since a value may be negative, simply returns 0 if an error</remarks>
        /// <param name="atomicNumber">Element ID</param>
        /// <param name="elementStat">Value to obtain: mass, charge, or uncertainty</param>
        public double GetElementStat(short atomicNumber, ElementStatsType elementStat)
        {
            return ElementAndMass.Elements.GetElementStat(atomicNumber, elementStat);
        }

        /// <summary>
        /// Get message text using message ID
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="appendText"></param>
        public string GetMessageStatement(int messageId, string appendText = "")
        {
            return ElementAndMass.Messages.GetMessageStatement(messageId, appendText);
        }

        /// <summary>
        /// Get the maximum message ID
        /// </summary>
        public int GetMessageStatementMaxId()
        {
            return ElementAndMass.Messages.GetMessageStatementMaxId();
        }

        /// <summary>
        /// Returns True if the first letter of <paramref name="symbol"/> is a ModSymbol
        /// </summary>
        /// <remarks>
        /// Invalid Mod Symbols are letters, numbers, ., -, space, (, or )
        /// Valid Mod Symbols are ! # $ % ampersand ' * + ? ^ ` ~
        /// </remarks>
        /// <param name="symbol"></param>
        public bool IsModSymbol(string symbol)
        {
            return ElementAndMass.IsModSymbol(symbol);
        }

        /// <summary>
        /// Remove all abbreviations
        /// </summary>
        public void RemoveAllAbbreviations()
        {
            ElementAndMass.Elements.RemoveAllAbbreviations();
        }

        /// <summary>
        /// Remove all caution statements
        /// </summary>
        public void RemoveAllCautionStatements()
        {
            ElementAndMass.Messages.RemoveAllCautionStatements();
        }

        /// <summary>
        /// Convert a mass to ppm
        /// </summary>
        /// <param name="massToConvert"></param>
        /// <param name="currentMz"></param>
        public double MassToPPM(double massToConvert, double currentMz)
        {
            return ElementAndMass.MassToPPM(massToConvert, currentMz);
        }

        /// <summary>
        /// Convert a monoisotopic mass to m/z
        /// </summary>
        /// <param name="monoisotopicMass"></param>
        /// <param name="charge"></param>
        /// <param name="chargeCarrierMass"></param>
        public double MonoMassToMz(double monoisotopicMass, short charge, double chargeCarrierMass = 0)
        {
            return ElementAndMass.MonoMassToMz(monoisotopicMass, charge, chargeCarrierMass);
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
            ElementAndMass.Elements.RecomputeAbbreviationMasses();
        }

        /// <summary>
        /// Remove the given abbreviation
        /// </summary>
        /// <param name="abbreviationSymbol"></param>
        /// <returns>True if removed, false abbreviationSymbol was not found</returns>
        public bool RemoveAbbreviation(string abbreviationSymbol)
        {
            return ElementAndMass.Elements.RemoveAbbreviation(abbreviationSymbol);
        }

        /// <summary>
        /// Remove the abbreviation at index <paramref name="abbreviationId"/>
        /// </summary>
        /// <param name="abbreviationId"></param>
        /// <returns>True if removed, false abbreviationId is invalid</returns>
        public bool RemoveAbbreviationById(int abbreviationId)
        {
            return ElementAndMass.Elements.RemoveAbbreviationById(abbreviationId);
        }

        /// <summary>
        /// Remove the given caution statement
        /// </summary>
        /// <param name="cautionSymbol"></param>
        /// <returns>True if removed, otherwise false</returns>
        public bool RemoveCautionStatement(string cautionSymbol)
        {
            return ElementAndMass.Messages.RemoveCautionStatement(cautionSymbol);
        }

        /// <summary>
        /// Reset abbreviations to defaults
        /// </summary>
        public void ResetAbbreviations()
        {
            ElementAndMass.Elements.MemoryLoadAbbreviations();
        }

        /// <summary>
        /// Reset caution statements to defaults
        /// </summary>
        public void ResetCautionStatements()
        {
            ElementAndMass.Messages.MemoryLoadCautionStatements();
        }

        /// <summary>
        /// Reset an element's mass, uncertainty, or charge
        /// </summary>
        /// <param name="atomicNumber"></param>
        /// <param name="specificStatToReset"></param>
        public void ResetElement(short atomicNumber, ElementStatsType specificStatToReset)
        {
            ElementAndMass.Elements.MemoryLoadElements(GetElementMode(), atomicNumber, specificStatToReset);
        }

        /// <summary>
        /// Reset message statements to defaults
        /// </summary>
        public void ResetMessageStatements()
        {
            ElementAndMass.Messages.MemoryLoadMessageStatements();
        }

        /// <summary>
        /// Adds a new abbreviation or updates an existing one (based on <paramref name="symbol"/>)
        /// </summary>
        /// <remarks>
        /// It is useful to set <paramref name="validateFormula"/> = False when you're defining all of the abbreviations at once,
        /// since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        /// defined, then the parsing of the first abbreviation will fail
        /// </remarks>
        /// <param name="symbol"></param>
        /// <param name="formula"></param>
        /// <param name="charge"></param>
        /// <param name="isAminoAcid"></param>
        /// <param name="oneLetterSymbol"></param>
        /// <param name="comment"></param>
        /// <param name="validateFormula">If true, make sure the formula is valid</param>
        /// <returns>0 if success, otherwise an error ID</returns>
        public int SetAbbreviation(
            string symbol, string formula, float charge,
            bool isAminoAcid,
            string oneLetterSymbol = "",
            string comment = "",
            bool validateFormula = true)
        {
            return ElementAndMass.Elements.SetAbbreviation(symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, validateFormula);
        }

        /// <summary>
        /// Adds a new abbreviation or updates an existing one (based on <paramref name="abbrevId"/>)
        /// </summary>
        /// <remarks>
        /// It is useful to set <paramref name="validateFormula"/> = False when you're defining all of the abbreviations at once,
        /// since one abbreviation can depend upon another, and if the second abbreviation hasn't yet been
        /// defined, then the parsing of the first abbreviation will fail
        /// </remarks>
        /// <param name="abbrevId">If abbrevId is less than 1, adds as a new abbreviation</param>
        /// <param name="symbol"></param>
        /// <param name="formula"></param>
        /// <param name="charge"></param>
        /// <param name="isAminoAcid"></param>
        /// <param name="oneLetterSymbol"></param>
        /// <param name="comment"></param>
        /// <param name="validateFormula">If true, make sure the formula is valid</param>
        /// <returns>0 if success, otherwise an error ID</returns>
        public int SetAbbreviationById(
            int abbrevId, string symbol, string formula,
            float charge, bool isAminoAcid,
            string oneLetterSymbol = "",
            string comment = "",
            bool validateFormula = true)
        {
            return ElementAndMass.Elements.SetAbbreviationById((short)abbrevId, symbol, formula, charge, isAminoAcid, oneLetterSymbol, comment, validateFormula);
        }

        /// <summary>
        /// Adds a new caution statement or updates an existing one (based on <paramref name="symbolCombo"/>)
        /// </summary>
        /// <param name="symbolCombo"></param>
        /// <param name="newCautionStatement"></param>
        /// <returns>0 if success, otherwise an Error ID</returns>
        public int SetCautionStatement(string symbolCombo, string newCautionStatement)
        {
            return ElementAndMass.Messages.SetCautionStatement(symbolCombo, newCautionStatement);
        }

        /// <summary>
        /// Set the charge carrier mass
        /// </summary>
        /// <param name="mass"></param>
        public void SetChargeCarrierMass(double mass)
        {
            ElementAndMass.Elements.SetChargeCarrierMass(mass);
        }

        /// <summary>
        /// Update the values for a single element (based on <paramref name="symbol"/>)
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="mass"></param>
        /// <param name="uncertainty"></param>
        /// <param name="charge"></param>
        /// <param name="recomputeAbbreviationMasses">Set to false if updating several elements</param>
        /// <returns>True if success, false if symbol is not a valid element symbol</returns>
        public bool SetElement(string symbol, double mass, double uncertainty,
            float charge,
            bool recomputeAbbreviationMasses = true)
        {
            return ElementAndMass.Elements.SetElement(symbol, mass, uncertainty, charge, recomputeAbbreviationMasses);
        }

        /// <summary>
        /// Set the isotopes for the element
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="isotopeMasses">0-based array</param>
        /// <param name="isotopeAbundances">0-based array</param>
        /// <returns>True if success, false if symbol is not a valid element symbol</returns>
        public bool SetElementIsotopes(string symbol, double[] isotopeMasses, float[] isotopeAbundances)
        {
            return ElementAndMass.Elements.SetElementIsotopes(symbol, isotopeMasses, isotopeAbundances);
        }

        /// <summary>
        /// Set the element mode used for mass calculations
        /// </summary>
        /// <param name="elementMode"></param>
        /// <param name="forceMemoryLoadElementValues">Set to true if you want to force a reload of element weights, even if not changing element modes</param>
        public void SetElementMode(ElementMassMode elementMode, bool forceMemoryLoadElementValues = false)
        {
            ElementAndMass.Elements.SetElementMode(elementMode, forceMemoryLoadElementValues);
        }

        /// <summary>
        /// Used to replace the default message strings with foreign language equivalent ones
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="newMessage"></param>
        /// <returns>True if success, false if an error</returns>
        public bool SetMessageStatement(int messageId, string newMessage)
        {
            return ElementAndMass.Messages.SetMessageStatement(messageId, newMessage);
        }

        /// <summary>
        /// Sort abbreviations
        /// <remarks>
        /// The abbreviation collection includes amino acids
        /// </remarks>
        /// </summary>
        public void SortAbbreviations()
        {
            ElementAndMass.Elements.SortAbbreviations();
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
            return ElementAndMass.PlainTextToRtf(textToConvert, calculatorMode, highlightCharFollowingPercentSign, overrideErrorId, errorIdOverride);
        }

        /// <summary>
        /// Checks the formula of all abbreviations to make sure it's valid
        /// Marks any abbreviations as Invalid if a problem is found or a circular reference exists
        /// </summary>
        /// <returns>Count of the number of invalid abbreviations found</returns>
        public int ValidateAllAbbreviations()
        {
            return ElementAndMass.Elements.ValidateAllAbbreviations();
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

        private void ElementAndMassRoutines_ProgressChanged(string taskDescription, float percentComplete)
        {
            ProgressChanged?.Invoke(taskDescription, percentComplete);
        }

        private void ElementAndMassRoutines_ProgressComplete()
        {
            ProgressComplete?.Invoke();
        }

        private void ElementAndMassRoutines_ProgressReset()
        {
            ProgressReset?.Invoke();
        }
    }
}

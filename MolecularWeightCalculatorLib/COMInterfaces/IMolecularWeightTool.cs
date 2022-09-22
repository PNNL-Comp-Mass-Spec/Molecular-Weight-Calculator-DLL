using System.Runtime.InteropServices;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.FormulaFinder;
using MolecularWeightCalculator.Sequence;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    // Ignore Spelling: centroided, interop, xyVals

    [Guid("093B01E2-8759-4860-B408-8BEA36F8FF1D"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IMolecularWeightTool
    {
        Compound Compound { get; set; }
        Peptide Peptide { get; set; }
        FormulaSearcher FormulaFinder { get; set; }
        CapillaryFlow CapFlow { get; set; }
        AbbrevRecognitionMode AbbreviationRecognitionMode { get; set; }
        string AppDate { get; }
        string AppVersion { get; }
        bool BracketsTreatedAsParentheses { get; set; }
        CaseConversionMode CaseConversionMode { get; set; }
        char DecimalSeparator { get; set; }
        string ErrorDescription { get; }
        int ErrorId { get; }
        string ErrorCharacter { get; }
        int ErrorPosition { get; }
        string LogFilePath { get; }
        string LogFolderPath { get; set; }
        bool LogMessagesToFile { get; set; }
        string ProgressStepDescription { get; }

        /// <summary>
        /// Percent complete: ranges from 0 to 100, but can contain decimal percentage values
        /// </summary>
        float ProgressPercentComplete { get; }

        string RtfFontName { get; set; }
        int RtfFontSize { get; set; }
        StdDevMode StdDevMode { get; set; }

        void ClearError();

        /// <summary>
        /// Compute the mass of a formula
        /// </summary>
        /// <param name="formula"></param>
        /// <returns>Mass of the formula</returns>
        double ComputeMass(string formula);

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
        /// <param name="convolutedMSData2DOneBased">2D array of MSData (mass and intensity pairs)</param>
        /// <param name="convolutedMSDataCount">Number of data points in ConvolutedMSData2DOneBased</param>
        /// <param name="addProtonChargeCarrier">If addProtonChargeCarrier is false, then still convolute by charge, but doesn't add a proton</param>
        /// <param name="headerIsotopicAbundances">Header to use in <paramref name="results"/></param>
        /// <param name="headerMassToCharge">Header to use in <paramref name="results"/></param>
        /// <param name="headerFraction">Header to use in <paramref name="results"/></param>
        /// <param name="headerIntensity">Header to use in <paramref name="results"/></param>
        /// <returns>True if success, false if an error</returns>
        bool ComputeIsotopicAbundances(
            ref string formulaIn, int chargeState, out string results,
            out double[,] convolutedMSData2DOneBased, out int convolutedMSDataCount,
            bool addProtonChargeCarrier = true,
            string headerIsotopicAbundances = "Isotopic Abundances for",
            string headerMassToCharge = "Mass",
            string headerFraction = "Fraction",
            string headerIntensity = "Intensity");

        ///// <summary>
        ///// Convert the centroided data (stick data) in xyVals to a Gaussian representation
        ///// </summary>
        ///// <param name="xyVals">XY data, as key-value pairs</param>
        ///// <param name="resolution">Effective instrument resolution (e.g. 1000 or 20000)</param>
        ///// <param name="resolutionMass">The m/z value at which the resolution applies</param>
        ///// <param name="qualityFactor">Gaussian quality factor (between 1 and 75, default is 50)</param>
        ///// <returns>Gaussian spectrum data</returns>
        ///// <remarks></remarks>
        //// Commented out because COM does not support generic types, and supporting this method with 2D arrays would take extra work and testing
        //List<KeyValuePair<double, double>> ConvertStickDataToGaussian2DArray(List<KeyValuePair<double, double>> xyVals, int resolution, double resolutionMass, int qualityFactor = 50);

        /// <summary>
        /// Converts a given mass or m/z value to the MH+ m/z value
        /// </summary>
        /// <param name="massMz">Mass or m/z value</param>
        /// <param name="currentCharge">Current charge (0 means neutral mass)</param>
        /// <param name="desiredCharge">Desired charge (0 means neutral mass)</param>
        /// <param name="chargeCarrierMass">Custom charge carrier mass (0 means use default, usually 1.00727649)</param>
        double ConvoluteMass(double massMz, int currentCharge, int desiredCharge = 1, double chargeCarrierMass = 0);

        /// <summary>
        /// Get an abbreviation, by ID
        /// </summary>
        /// <param name="abbreviationId"></param>
        /// <param name="symbol">Output: symbol</param>
        /// <param name="formula">Output: empirical formula</param>
        /// <param name="charge">Output: charge</param>
        /// <param name="isAminoAcid">Output: true if an amino acid</param>
        /// <returns>True if success, false if abbreviationId is invalid</returns>
        bool GetAbbreviation(int abbreviationId, out string symbol,
            out string formula, out float charge,
            out bool isAminoAcid);

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
        bool GetAbbreviation(int abbreviationId, out string symbol,
            out string formula, out float charge,
            out bool isAminoAcid,
            out string oneLetterSymbol,
            out string comment);

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
        bool GetAbbreviation(int abbreviationId, out string symbol,
            out string formula, out float charge,
            out bool isAminoAcid,
            out string oneLetterSymbol,
            out string comment,
            out bool invalidSymbolOrFormula);

        /// <summary>
        /// Get the number of abbreviations in memory
        /// </summary>
        int GetAbbreviationCount();

        int GetAbbreviationCountMax();

        /// <summary>
        /// Get the abbreviation ID for the given abbreviation symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        int GetAbbreviationId(string symbol);

        string GetAminoAcidSymbolConversion(string symbolToFind, bool oneLetterTo3Letter);

        /// <summary>
        /// Get caution statement information
        /// </summary>
        /// <param name="symbolCombo">symbol combo for the caution statement</param>
        /// <param name="cautionStatement">Output: caution statement text</param>
        /// <returns>True if success, false symbolCombo is not recognized</returns>
        bool GetCautionStatement(string symbolCombo, out string cautionStatement);

        /// <summary>
        /// Get the number of Caution Statements in memory
        /// </summary>
        int GetCautionStatementCount();

        /// <summary>
        /// Get the symbolCombos for Caution Statements in memory as an array. This version is for supporting COM interop because COM does not support generic types
        /// </summary>
        string[] GetCautionStatementSymbolsArray();

        double GetChargeCarrierMass();

        /// <summary>
        /// Returns the settings for the element with <paramref name="atomicNumber"/> in the output variables
        /// </summary>
        /// <param name="atomicNumber">Element atomic number (1 for hydrogen, 2 for helium, etc.)</param>
        /// <param name="symbol">Element symbol</param>
        /// <param name="mass">Mass</param>
        /// <param name="uncertainty">Uncertainty of the mass</param>
        /// <param name="charge">Charge</param>
        /// <param name="isotopeCount">Number of isotopes</param>
        /// <returns>True if success, false if atomicNumber is invalid</returns>
        bool GetElement(int atomicNumber, out string symbol, out double mass, out double uncertainty, out float charge, out short isotopeCount);

        /// <summary>
        /// Returns the number of elements in memory
        /// </summary>
        int GetElementCount();

        /// <summary>
        /// Get the element ID for the given symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        int GetAtomicNumber(string symbol);

        /// <summary>
        /// Returns the isotope masses and relative abundances for the element with <paramref name="atomicNumber"/>
        /// </summary>
        /// <param name="atomicNumber">Element ID, or atomic number</param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses"></param>
        /// <param name="isotopeAbundances"></param>
        /// <returns>True if success, false if atomicNumber is invalid</returns>
        bool GetElementIsotopes(int atomicNumber, out short isotopeCount, out double[] isotopeMasses, out float[] isotopeAbundances);

        /// <summary>
        /// Returns the isotope masses and abundances for the given element
        /// </summary>
        /// <param name="elementSymbol">Element symbol, e.g. C or Li or P</param>
        /// <param name="isotopeCount">Output: isotope count</param>
        /// <param name="isotopeMasses">Output: 0-based array of isotope masses</param>
        /// <param name="isotopeAbundances">Output: 0-based array of relative abundances</param>
        /// <returns>True if success, false if elementSymbol is invalid</returns>
        bool GetElementIsotopes(string elementSymbol, out short isotopeCount, out double[] isotopeMasses, out float[] isotopeAbundances);

        /// <summary>
        /// Get the current element mode
        /// </summary>
        /// <returns>
        /// emAverageMass  = 1
        /// emIsotopicMass = 2
        /// emIntegerMass  = 3
        /// </returns>
        ElementMassMode GetElementMode();

        /// <summary>
        /// Return the element symbol for the given element ID
        /// </summary>
        /// <remarks>1 is Hydrogen, 2 is Helium, etc.</remarks>
        /// <param name="atomicNumber"></param>
        string GetElementSymbol(int atomicNumber);

        /// <summary>
        /// Returns a single bit of information about a single element
        /// </summary>
        /// <remarks>Since a value may be negative, simply returns 0 if an error</remarks>
        /// <param name="atomicNumber">Element ID</param>
        /// <param name="elementStat">Value to obtain: mass, charge, or uncertainty</param>
        double GetElementStat(int atomicNumber, ElementStatsType elementStat);

        /// <summary>
        /// Get message text using message ID
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="appendText"></param>
        string GetMessageStatement(int messageId, string appendText = "");

        int GetMessageStatementMaxId();

        /// <summary>
        /// Returns True if the first letter of <paramref name="symbol"/> is a ModSymbol
        /// </summary>
        /// <remarks>
        /// Invalid Mod Symbols are letters, numbers, ., -, space, (, or )
        /// Valid Mod Symbols are ! # $ % ampersand ' * + ? ^ ` ~
        /// </remarks>
        /// <param name="symbol"></param>
        bool IsModSymbol(string symbol);

        void RemoveAllAbbreviations();
        void RemoveAllCautionStatements();
        double MassToPPM(double massToConvert, double currentMz);
        double MonoMassToMz(double monoisotopicMass, int charge, double chargeCarrierMass = 0);

        /// <summary>
        /// Recomputes the Mass for all of the loaded abbreviations
        /// </summary>
        /// <remarks>
        /// Useful if we just finished setting lots of element masses, and
        /// had recomputeAbbreviationMasses = false when calling .SetElement()
        /// </remarks>
        void RecomputeAbbreviationMasses();

        bool RemoveAbbreviation(string abbreviationSymbol);
        bool RemoveAbbreviationById(int abbreviationId);
        bool RemoveCautionStatement(string cautionSymbol);
        void ResetAbbreviations();
        void ResetCautionStatements();
        void ResetElement(int atomicNumber, ElementStatsType specificStatToReset);
        void ResetMessageStatements();

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
        int SetAbbreviation(
            string symbol, string formula, float charge,
            bool isAminoAcid,
            string oneLetterSymbol = "",
            string comment = "",
            bool validateFormula = true);

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
        int SetAbbreviationById(
            int abbrevId, string symbol, string formula,
            float charge, bool isAminoAcid,
            string oneLetterSymbol = "",
            string comment = "",
            bool validateFormula = true);

        /// <summary>
        /// Adds a new caution statement or updates an existing one (based on <paramref name="symbolCombo"/>)
        /// </summary>
        /// <param name="symbolCombo"></param>
        /// <param name="newCautionStatement"></param>
        /// <returns>0 if success, otherwise an error ID</returns>
        int SetCautionStatement(string symbolCombo, string newCautionStatement);

        void SetChargeCarrierMass(double mass);

        /// <summary>
        /// Update the values for a single element (based on <paramref name="symbol"/>)
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="mass"></param>
        /// <param name="uncertainty"></param>
        /// <param name="charge"></param>
        /// <param name="recomputeAbbreviationMasses">Set to false if updating several elements</param>
        /// <returns>True if success, false if symbol is not a valid element symbol</returns>
        bool SetElement(string symbol, double mass, double uncertainty,
            float charge,
            bool recomputeAbbreviationMasses = true);

        /// <summary>
        /// Set the isotopes for the element
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="isotopeMasses">0-based array</param>
        /// <param name="isotopeAbundances">0-based array</param>
        /// <returns>True if success, false if symbol is not a valid element symbol</returns>
        bool SetElementIsotopes(string symbol, double[] isotopeMasses, float[] isotopeAbundances);

        /// <summary>
        /// Set the element mode used for mass calculations
        /// </summary>
        /// <param name="elementMode"></param>
        /// <param name="forceMemoryLoadElementValues">Set to true if you want to force a reload of element weights, even if not changing element modes</param>
        void SetElementMode(ElementMassMode elementMode, bool forceMemoryLoadElementValues = false);

        /// <summary>
        /// Used to replace the default message strings with foreign language equivalent ones
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="newMessage"></param>
        /// <returns>True if success, false if an error</returns>
        bool SetMessageStatement(int messageId, string newMessage);

        void SortAbbreviations();

        /// <summary>
        /// Converts plain text to formatted RTF text
        /// </summary>
        /// <param name="textToConvert"></param>
        /// <param name="calculatorMode">When true, does not superscript + signs and numbers following + signs</param>
        /// <param name="highlightCharFollowingPercentSign">When true, change the character following a percent sign to red (and remove the percent sign)</param>
        /// <param name="overrideErrorId"></param>
        /// <param name="errorIdOverride"></param>
        // ReSharper disable once InconsistentNaming
        string TextToRTF(
            string textToConvert,
            bool calculatorMode = false,
            bool highlightCharFollowingPercentSign = true,
            bool overrideErrorId = false,
            int errorIdOverride = 0);

        /// <summary>
        /// Checks the formula of all abbreviations to make sure it's valid
        /// Marks any abbreviations as Invalid if a problem is found or a circular reference exists
        /// </summary>
        /// <returns>Count of the number of invalid abbreviations found</returns>
        int ValidateAllAbbreviations();
    }
}

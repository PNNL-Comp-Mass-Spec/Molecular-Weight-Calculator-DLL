using System.Runtime.InteropServices;
using MolecularWeightCalculator.Formula;
using MolecularWeightCalculator.FormulaFinder;
using MolecularWeightCalculator.Sequence;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("5A9D3A30-60D4-4C36-AD67-C734678D1CEB"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), ComVisible(true)]
    public interface IMolecularWeightToolEvents
    {
        event ProgressResetEventHandler ProgressReset;
        event ProgressChangedEventHandler ProgressChanged;
        event ProgressCompleteEventHandler ProgressComplete;
    }

    [Guid("E19152E6-AE5D-4C9F-94F5-91B4D1655BC2"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
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
        /// <returns></returns>
        float ProgressPercentComplete { get; }

        string RtfFontName { get; set; }
        short RtfFontSize { get; set; }
        bool ShowErrorDialogs { get; set; }
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
        /// <returns>0 if success, -1 if an error</returns>
        /// <remarks>
        /// Returns uncharged mass values if <paramref name="chargeState"/>=0,
        /// Returns M+H values if <paramref name="chargeState"/>=1
        /// Returns convoluted m/z if <paramref name="chargeState"/> is &gt; 1
        /// </remarks>
        short ComputeIsotopicAbundances(
            ref string formulaIn, short chargeState, out string results,
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
        /// <returns></returns>
        /// <remarks></remarks>
        double ConvoluteMass(double massMz, short currentCharge, short desiredCharge = 1, double chargeCarrierMass = 0);

        /// <summary>
        /// Get an abbreviation, by ID
        /// </summary>
        /// <param name="abbreviationId"></param>
        /// <param name="symbol">Output: symbol</param>
        /// <param name="formula">Output: empirical formula</param>
        /// <param name="charge">Output: charge</param>
        /// <param name="isAminoAcid">Output: true if an amino acid</param>
        /// <returns> 0 if success, 1 if failure</returns>
        int GetAbbreviation(int abbreviationId, out string symbol,
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
        /// <returns> 0 if success, 1 if failure</returns>
        int GetAbbreviation(int abbreviationId, out string symbol,
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
        /// <returns> 0 if success, 1 if failure</returns>
        int GetAbbreviation(int abbreviationId, out string symbol,
            out string formula, out float charge,
            out bool isAminoAcid,
            out string oneLetterSymbol,
            out string comment,
            out bool invalidSymbolOrFormula);

        /// <summary>
        /// Get the number of abbreviations in memory
        /// </summary>
        /// <returns></returns>
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
        /// <returns>0 if success, 1 if an invalid ID</returns>
        int GetCautionStatement(string symbolCombo, out string cautionStatement);

        /// <summary>
        /// Get the number of Caution Statements in memory
        /// </summary>
        /// <returns></returns>
        int GetCautionStatementCount();

        /// <summary>
        /// Get the symbolCombos for Caution Statements in memory as an array. This version is for supporting COM interop because COM does not support generic types
        /// </summary>
        /// <returns></returns>
        string[] GetCautionStatementSymbolsArray();

        double GetChargeCarrierMass();

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
        int GetElement(short elementId, out string symbol, out double mass, out double uncertainty, out float charge, out short isotopeCount);

        /// <summary>
        /// Returns the number of elements in memory
        /// </summary>
        /// <returns></returns>
        int GetElementCount();

        /// <summary>
        /// Get the element ID for the given symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>ID if found, otherwise 0</returns>
        int GetElementId(string symbol);

        /// <summary>
        /// Returns the isotope masses and abundances for the element with <paramref name="elementId"/>
        /// </summary>
        /// <param name="elementId">Element ID, or atomic number</param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses"></param>
        /// <param name="isotopeAbundances"></param>
        /// <returns>0 if a valid ID, 1 if invalid</returns>
        int GetElementIsotopes(short elementId, out short isotopeCount, out double[] isotopeMasses, out float[] isotopeAbundances);

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
        /// <param name="elementId"></param>
        /// <returns></returns>
        /// <remarks>1 is Hydrogen, 2 is Helium, etc.</remarks>
        string GetElementSymbol(short elementId);

        /// <summary>
        /// Returns a single bit of information about a single element
        /// </summary>
        /// <param name="elementId">Element ID</param>
        /// <param name="elementStat">Value to obtain: mass, charge, or uncertainty</param>
        /// <returns></returns>
        /// <remarks>Since a value may be negative, simply returns 0 if an error</remarks>
        double GetElementStat(short elementId, ElementStatsType elementStat);

        /// <summary>
        /// Get message text using message ID
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="appendText"></param>
        /// <returns></returns>
        string GetMessageStatement(int messageId, string appendText = "");

        int GetMessageStatementMaxId();

        /// <summary>
        /// Returns True if the first letter of <paramref name="symbol"/> is a ModSymbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        /// <remarks>
        /// Invalid Mod Symbols are letters, numbers, ., -, space, (, or )
        /// Valid Mod Symbols are ! # $ % ampersand ' * + ? ^ ` ~
        /// </remarks>
        bool IsModSymbol(string symbol);

        void RemoveAllAbbreviations();
        void RemoveAllCautionStatements();
        double MassToPPM(double massToConvert, double currentMz);
        double MonoMassToMz(double monoisotopicMass, short charge, double chargeCarrierMass = 0);

        /// <summary>
        /// Recomputes the Mass for all of the loaded abbreviations
        /// </summary>
        /// <remarks>
        /// Useful if we just finished setting lots of element masses, and
        /// had recomputeAbbreviationMasses = false when calling .SetElement()
        /// </remarks>
        void RecomputeAbbreviationMasses();

        int RemoveAbbreviation(string abbreviationSymbol);
        int RemoveAbbreviationById(int abbreviationId);
        int RemoveCautionStatement(string cautionSymbol);
        void ResetAbbreviations();
        void ResetCautionStatements();
        void ResetElement(short elementId, ElementStatsType specificStatToReset);
        void ResetMessageStatements();

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
        int SetAbbreviation(
            string symbol, string formula, float charge,
            bool isAminoAcid,
            string oneLetterSymbol = "",
            string comment = "",
            bool validateFormula = true);

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
        /// <returns>0 if successful, otherwise, returns an Error ID</returns>
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
        /// <returns></returns>
        int SetElement(string symbol, double mass, double uncertainty,
            float charge,
            bool recomputeAbbreviationMasses = true);

        /// <summary>
        /// Set the isotopes for the element
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="isotopeCount"></param>
        /// <param name="isotopeMasses">0-based array</param>
        /// <param name="isotopeAbundances">0-based array</param>
        /// <returns></returns>
        int SetElementIsotopes(string symbol, short isotopeCount, double[] isotopeMasses, float[] isotopeAbundances);

        void SetElementMode(ElementMassMode elementMode, bool memoryLoadElementValues = true);

        /// <summary>
        /// Used to replace the default message strings with foreign language equivalent ones
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="newMessage"></param>
        /// <returns>0 if success; 1 if failure</returns>
        int SetMessageStatement(int messageId, string newMessage);

        void SortAbbreviations();

        /// <summary>
        /// Converts plain text to formatted RTF text
        /// </summary>
        /// <param name="textToConvert"></param>
        /// <param name="calculatorMode">When true, does not superscript + signs and numbers following + signs</param>
        /// <param name="highlightCharFollowingPercentSign">When true, change the character following a percent sign to red (and remove the percent sign)</param>
        /// <param name="overrideErrorId"></param>
        /// <param name="errorIdOverride"></param>
        /// <returns></returns>
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
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

        // Ignore Spelling: ansi, alph, Bryson, Convoluting, ElementsAndAbbrevs, func, gaussian, Isoelectric, isoStats, Mwt
        // Ignore Spelling: parenth, pos, prepend, Rtf, struct, xyVals, yyyy-MM-dd, hh:mm:ss, tt

        /// <summary>
        /// Constructor
        /// </summary>
        public ElementAndMassTools()
        {
            // Messages: no reverse dependencies.
            Messages = new Messages(ComputationOptions);

            // Load the parser before elements - initialization does not rely on existing data.
            Parser = new FormulaParser(this);

            // ElementsAndAbbrevs: requires a valid parser to load abbreviations.
            Elements = new ElementsAndAbbrevs(this);

            mProgressStepDescription = string.Empty;
            mProgressPercentComplete = 0f;

            mLogFolderPath = string.Empty;
            mLogFilePath = string.Empty;

            ShowErrorMessageDialogs = false;
        }

        #region "Constants and Enums"

        private const char RTF_HEIGHT_ADJUST_CHAR = '~'; // A hidden character to adjust the height of RTF Text Boxes when using superscripts

        private enum MessageType
        {
            Normal = 0,
            Error = 1,
            Warning = 2
        }

        #endregion

        #region "Data classes"

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

        #endregion

        #region "Class wide Variables"

        public FormulaOptions ComputationOptions { get; } = new();

        internal Messages Messages { get; }

        internal ElementsAndAbbrevs Elements { get; }

        public FormulaParser Parser { get; }

        private int mLastErrorId = 0;

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
        public float ProgressPercentComplete => (float)Math.Round(mProgressPercentComplete, 2);

        public bool ShowErrorMessageDialogs { get; set; }

        #endregion

        public virtual void AbortProcessingNow()
        {
            mAbortProcessing = true;
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
        internal short ComputeIsotopicAbundances(
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
            results = string.Empty;

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
                var data = Parser.ParseFormula(formula, false);
                var workingFormulaMass = data.Mass;
                mLastErrorId = data.ErrorData.ErrorId;
                var computationStats = data.Stats;

                if (workingFormulaMass < 0d)
                {
                    // Error occurred; information is stored in ErrorParams
                    results = Messages.LookupMessage(350) + ": " + Messages.LookupMessage(mLastErrorId);
                    return -1;
                }

                // See if Deuterium is present by looking for a fractional amount of Hydrogen
                // formula will contain a capital D followed by a number or another letter (or the end of formula)
                // If found, replace each D with ^2.014H and re-compute
                var count = computationStats.Elements[1].Count;
                if (Math.Abs(count - (int)Math.Round(count)) > float.Epsilon)
                {
                    // Deuterium is present
                    var modifiedFormula = string.Empty;
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
                                var asciiOfNext = formula[index + 1];
                                if (asciiOfNext < 'a' || asciiOfNext > 'z')
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
                    data = Parser.ParseFormula(formula, false);
                    workingFormulaMass = data.Mass;
                    mLastErrorId = data.ErrorData.ErrorId;
                    computationStats = data.Stats;

                    if (workingFormulaMass < 0d)
                    {
                        // Error occurred; information is stored in ErrorParams
                        results = Messages.LookupMessage(350) + ": " + Messages.LookupMessage(mLastErrorId);
                        return -1;
                    }
                }

                // Make sure there are no fractional atoms present (need to specially handle Deuterium)
                for (var elementIndex = 1; elementIndex <= ElementsAndAbbrevs.ELEMENT_COUNT; elementIndex++)
                {
                    count = computationStats.Elements[elementIndex].Count;
                    if (Math.Abs(count - (int)Math.Round(count)) > float.Epsilon)
                    {
                        results = Messages.LookupMessage(350) + ": " + Messages.LookupMessage(805) + ": " + Elements.ElementStats[elementIndex].Symbol + count;
                        return -1;
                    }
                }

                // Remove occurrences of explicitly defined isotopes from the formula
                for (var elementIndex = 1; elementIndex <= ElementsAndAbbrevs.ELEMENT_COUNT; elementIndex++)
                {
                    var element = computationStats.Elements[elementIndex];
                    if (element.Isotopes.Count > 0)
                    {
                        explicitIsotopesPresent = true;
                        explicitIsotopeCount += (short)element.Isotopes.Count;
                        foreach (var item in element.Isotopes)
                            element.Count -= item.Count;
                    }
                }

                // Determine the number of elements present in formula
                short elementCount = 0;
                for (var elementIndex = 1; elementIndex <= ElementsAndAbbrevs.ELEMENT_COUNT; elementIndex++)
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
                for (var elementIndex = 1; elementIndex <= ElementsAndAbbrevs.ELEMENT_COUNT; elementIndex++)
                {
                    if (computationStats.Elements[elementIndex].Used)
                    {
                        if (computationStats.Elements[elementIndex].Count > 0d)
                        {
                            var stats = Elements.ElementStats[elementIndex];
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
                    for (var elementIndex = 1; elementIndex <= ElementsAndAbbrevs.ELEMENT_COUNT; elementIndex++)
                    {
                        var element = computationStats.Elements[elementIndex];
                        if (element.Isotopes.Count > 0)
                        {
                            foreach (var item in element.Isotopes)
                            {
                                var isoStat = new IsoResultsByElement(elementIndex, (int)Math.Round(item.Count), item.Mass, true);
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
                foreach (var item in isoStats)
                {
                    var masterElementIndex = item.ElementIndex;
                    atomCount = item.AtomCount;
                    isotopeCount = (short)Elements.ElementStats[masterElementIndex].Isotopes.Count;

                    predictedCombos = FindCombosPredictIterations(atomCount, isotopeCount);
                    predictedTotalComboCalcs += predictedCombos;
                }

                ResetProgress("Finding Isotopic Abundances: Computing abundances");

                // For each element, compute all of the possible combinations
                var completedComboCalcs = 0;
                foreach (var isoStat in isoStats)
                {
                    short isotopeStartingMass;
                    short isotopeEndingMass;
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
                        var stats = Elements.ElementStats[masterElementIndex];
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

                                // Abundance denominator and abundance suffix are only needed if using the easily-overflowed factorial method
                                var abundDenom = 1d;
                                var abundSuffix = 1d;
                                var stats = Elements.ElementStats[masterElementIndex];
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

                                    var stats = Elements.ElementStats[masterElementIndex];
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

                                            logRho = logSigma - (m - mPrime) * Math.Log(Elements.ElementStats[masterElementIndex].Isotopes[isotopeIndex].Abundance);
                                        }
                                        else if (m < mPrime)
                                        {
                                            var logSigma = 0d;
                                            for (var subIndex = (int)Math.Round(m) + 1; subIndex <= (int)Math.Round(mPrime); subIndex++)
                                                logSigma += Math.Log(subIndex);

                                            var stats = Elements.ElementStats[masterElementIndex];
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
                                indexToStoreAbundance = FindIndexForNominalMass(isoCombos, comboIndex, isotopeCount, atomCount, Elements.ElementStats[masterElementIndex].Isotopes);

                                // Store the abundance in .MassAbundances[] at location IndexToStoreAbundance
                                // TODO: Use +=
                                isoStat.MassAbundances[indexToStoreAbundance] = (float)(isoStat.MassAbundances[indexToStoreAbundance] + thisComboFractionalAbundance);
                            }

                            if (ratioMethodUsed)
                            {
                                // Store abundance for next Combo
                                indexToStoreAbundance = FindIndexForNominalMass(isoCombos, comboIndex + 1, isotopeCount, atomCount, Elements.ElementStats[masterElementIndex].Isotopes);

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
                    results = Messages.LookupMessage(940);
                    return -1;
                }

                // Step Through IsoStats from the end to the beginning, shortening the length to the
                // first value greater than MIN_ABUNDANCE_TO_KEEP
                // This greatly speeds up the convolution
                foreach (var statItem in isoStats)
                {
                    var index = statItem.ResultsCount - 1;
                    while (statItem.MassAbundances[index] < minAbundanceToKeep)
                    {
                        index -= 1;
                        if (index == 0)
                            break;
                    }

                    statItem.ResultsCount = index + 1;
                }

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
                    results = Messages.LookupMessage(940);
                    return -1;
                }

                // Compute mass defect (difference of initial isotope's mass from integer mass)
                var exactBaseIsoMass = 0d;
                foreach (var statItem in isoStats)
                {
                    if (statItem.ExplicitIsotope)
                    {
                        exactBaseIsoMass += statItem.AtomCount * statItem.ExplicitMass;
                    }
                    else
                    {
                        exactBaseIsoMass += statItem.AtomCount * Elements.ElementStats[statItem.ElementIndex].Isotopes[0].Mass;
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
                            convolutedMSData2D[massIndex, 0] = ConvoluteMass(convolutedMSData2D[massIndex, 0], 0, chargeState);
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
                mLastErrorId = 590;
                return -1;
            }

            return 0; // Success
        }

        /// <summary>
        /// Convert the centroided data (stick data) in XYVals to a Gaussian representation
        /// </summary>
        /// <param name="xyVals">XY data, as key-value pairs</param>
        /// <param name="resolution">Effective instrument resolution (e.g. 1000 or 20000)</param>
        /// <param name="resolutionMass">The m/z value at which the resolution applies</param>
        /// <param name="qualityFactor">Gaussian quality factor (between 1 and 75, default is 50)</param>
        /// <returns>Gaussian spectrum data</returns>
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
                        if (xySummation.Count == 0)
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
                    if (xySummation.Count == 0)
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

        /// <summary>
        /// Converts <paramref name="massMz"/> to the MZ that would appear at the given <paramref name="desiredCharge"/>
        /// </summary>
        /// <param name="massMz"></param>
        /// <param name="currentCharge"></param>
        /// <param name="desiredCharge"></param>
        /// <param name="chargeCarrierMass">Charge carrier mass.  If 0, this function will use mChargeCarrierMass instead</param>
        /// <returns>The new m/z value</returns>
        /// <remarks>To return the neutral mass, set <paramref name="desiredCharge"/> to 0</remarks>
        internal double ConvoluteMass(
            double massMz,
            short currentCharge,
            short desiredCharge = 1,
            double chargeCarrierMass = 0)
        {
            const double defaultChargeCarrierMassMonoiso = 1.00727649d;

            double newMz;

            if (Math.Abs(chargeCarrierMass - 0d) < float.Epsilon)
                chargeCarrierMass = Elements.ChargeCarrierMass;
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
                mLastErrorId = 590;
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
                using var outFile = new System.IO.StreamWriter(errorFilePath, true);
                outFile.WriteLine(DateTime.Now + " -- " + message + Environment.NewLine);
            }
            catch
            {
                // Ignore errors here
            }
        }

        public string GetErrorDescription()
        {
            var errorId = GetErrorId();
            if (errorId != 0)
            {
                return Messages.LookupMessage(errorId);
            }

            return "";
        }

        public int GetErrorId()
        {
            return mLastErrorId == 0 ? Parser.GetErrorId() : mLastErrorId;
        }

        public string GetErrorCharacter()
        {
            return Parser.GetErrorCharacter();
        }

        public int GetErrorPosition()
        {
            return Parser.GetErrorPosition();
        }

        /// <summary>
        /// Returns True if the first letter of <paramref name="testChar"/> is a ModSymbol
        /// </summary>
        /// <param name="testChar"></param>
        /// <remarks>
        /// Invalid Mod Symbols are letters, numbers, ., -, space, (, or )
        /// Valid Mod Symbols are ! # $ % ampersand ' * + ? ^ ` ~
        /// </remarks>
        internal bool IsModSymbol(string testChar)
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
                    case 40:
                    case 41: // ( and ) are not allowed
                        isModSymbol = false;
                        break;
                    case >= 44 and <= 62: // . and - and , and / and numbers and : and ; and < and = and > are not allowed
                        isModSymbol = false;
                        break;
                    case >= 33 and <= 43:
                    case 63:    // ?
                    case 64:    // @
                    case >= 94 and <= 96:   // ^ and _ and `
                    case 126:   // ~
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

            var messageTypeText = messageType switch
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

        /// <summary>
        /// Converts <paramref name="massToConvert"/> to ppm, based on the value of <paramref name="currentMz"/>
        /// </summary>
        /// <param name="massToConvert"></param>
        /// <param name="currentMz"></param>
        internal double MassToPPM(double massToConvert, double currentMz)
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
        internal double MonoMassToMz(
            double monoisotopicMass,
            short charge,
            double chargeCarrierMass = 0)
        {
            if (Math.Abs(chargeCarrierMass) < float.Epsilon)
                chargeCarrierMass = Elements.ChargeCarrierMass;

            // Call ConvoluteMass to convert to the desired charge state
            return ConvoluteMass(monoisotopicMass + chargeCarrierMass, 1, charge, chargeCarrierMass);
        }

        public void MemoryLoadAll(ElementMassMode elementMode)
        {
            Elements.MemoryLoadAllElements(elementMode);

            Messages.MemoryLoadAllStatements();
        }

        internal void MwtWinDllErrorHandler(string callingProcedure, Exception ex)
        {
            string message;

            if (ex is OverflowException)
            {
                message = Messages.LookupMessage(590);
                if (ShowErrorMessageDialogs)
                {
                    MessageBox.Show(Messages.LookupMessage(590), Messages.LookupMessage(350), MessageBoxButtons.OK);
                }

                LogMessage(message, MessageType.Error);
            }
            else
            {
                message = Messages.LookupMessage(600) + ": " + ex.Message + Environment.NewLine + " (" + callingProcedure + " handler)";
                message += Environment.NewLine + Messages.LookupMessage(605);

                if (ShowErrorMessageDialogs)
                {
                    MessageBox.Show(message, Messages.LookupMessage(350), MessageBoxButtons.OK);
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
        /// Converts plain text to formatted RTF text
        /// </summary>
        /// <param name="workText"></param>
        /// <param name="calculatorMode">When true, does not superscript + signs and numbers following + signs</param>
        /// <param name="highlightCharFollowingPercentSign">When true, change the character following a percent sign to red (and remove the percent sign)</param>
        /// <param name="overrideErrorId"></param>
        /// <param name="errorIdOverride"></param>
        internal string PlainTextToRtf(
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

            if (string.IsNullOrWhiteSpace(workText))
            {
                // Return a blank RTF string
                return rtf + "}";
            }

            var superFound = false;
            var workCharPrev = string.Empty;
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
                            errorId = mLastErrorId;
                        }

                        switch (errorId)
                        {
                            case 2:
                            case 3:
                            case 4:
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
                else if (workChar == FormulaParser.EMPTY_STRING_CHAR.ToString())
                {
                    // skip it, the tilde sign is used to add additional height to the formula line when isotopes are used
                    // If it's here from a previous time, we ignore it, adding it at the end if needed (if superFound = true)
                }
                else if (char.IsDigit(workChar[0]) || workChar == ComputationOptions.DecimalSeparator.ToString())
                {
                    // Number or period, so super or subscript it if needed
                    if (charIndex == 0)
                    {
                        // at beginning of line, so leave it alone. Probably out of place
                        rtf += workChar;
                    }
                    else if (!calculatorMode && (char.IsLetter(workCharPrev[0]) || workCharPrev == ")" || workCharPrev == @"\}" || workCharPrev == "+" || workCharPrev == "_" || rtf.Substring(rtf.Length - 6, 3) == "sub"))
                    {
                        // subscript if previous character was a character, parentheses, curly bracket, plus sign, or was already subscript
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

        internal void ResetErrorParams()
        {
            mLastErrorId = 0;
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
                    pctSign = string.Empty;
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
                mLastErrorId = -10;
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
                case 2d:
                case 3d:
                case 4d:
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

        public void SetShowErrorMessageDialogs(bool value)
        {
            ShowErrorMessageDialogs = value;
        }

        /// <summary>
        /// Adds spaces to <paramref name="work"/> until the length is <paramref name="length"/>
        /// </summary>
        /// <param name="work"></param>
        /// <param name="length"></param>
        public string SpacePad(string work, short length)
        {
            if (work.Length < length)
            {
                work += new string(' ', length - work.Length);
            }

            while (work.Length < length)
                work += " ";

            return work;
        }

        private string SpacePadFront(string work, short length)
        {
            if (work.Length < length)
            {
                work = new string(' ', length - work.Length) + work;
            }

            while (work.Length < length)
                work = " " + work;

            return work;
        }

        /// <summary>
        /// Update the progress description
        /// </summary>
        /// <param name="progressStepDescription">Description of the current processing occurring</param>
        protected void UpdateProgress(string progressStepDescription)
        {
            UpdateProgress(progressStepDescription, mProgressPercentComplete);
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <param name="percentComplete">Value between 0 and 100, indicating percent complete</param>
        protected void UpdateProgress(float percentComplete)
        {
            UpdateProgress(ProgressStepDescription, percentComplete);
        }

        /// <summary>
        /// Update the progress
        /// </summary>
        /// <param name="progressStepDescription">Description of the current processing occurring</param>
        /// <param name="percentComplete">Value between 0 and 100, indicating percent complete</param>
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
    }
}
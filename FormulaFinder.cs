using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MolecularWeightCalculator
{
    public class FormulaFinder
    {
        #region "Constants"
        private const int MAX_MATCHING_ELEMENTS = 10;
        public const int DEFAULT_RESULTS_TO_FIND = 1000;
        public const int MAXIMUM_ALLOWED_RESULTS_TO_FIND = 1000000;

        public const int MAX_BOUNDED_SEARCH_COUNT = 65565;
        #endregion

        #region "Data classes and Enums"

        /// <summary>
        /// Search tolerances for each element
        /// </summary>
        /// <remarks>
        /// Target percent composition values must be between 0 and 100; they are only used when calling FindMatchesByPercentComposition
        /// MinimumCount and MaximumCount are only used when the search mode is Bounded; they are ignored for Thorough search
        /// </remarks>
        public class CandidateElementTolerances
        {
            public double TargetPercentComposition;
            public int MinimumCount;
            public int MaximumCount;
        }

        private class ElementNum
        {
            public int H;
            public int C;
            public int Si;
            public int N;
            public int P;
            public int O;
            public int S;
            public int Cl;
            public int I;
            public int F;
            public int Br;
            public int Other;
        }

        private class BoundedSearchRange
        {
            public int Min;
            public int Max;
        }

        private enum CalculationMode
        {
            MatchMolecularWeight = 0,
            MatchPercentComposition = 1
        }

        #endregion

        #region "Member Variables"

        private bool mAbortProcessing;

        /// <summary>
        /// Keys are element symbols, abbreviations, or even simply a mass value
        /// Values are target percent composition values, between 0 and 100
        /// </summary>
        /// <remarks>The target percent composition values are only used when FindMatchesByPercentComposition is called</remarks>
        private Dictionary<string, CandidateElementTolerances> mCandidateElements;

        private readonly ElementAndMassTools mElementAndMassRoutines;

        private int mMaximumHits;

        private int mRecursiveCount;
        private int mMaxRecursiveCount;

        #endregion

        #region "Properties"

        /// <summary>
        /// Element symbols to consider when finding empirical formulas
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>The values in the dictionary are target percent composition values; only used if you call FindMatchesByPercentComposition</remarks>
        public Dictionary<string, CandidateElementTolerances> CandidateElements
        {
            get => mCandidateElements;
            set
            {
                if (value != null)
                {
                    mCandidateElements = value;

                    ValidateBoundedSearchValues();
                    ValidatePercentCompositionValues();
                }
            }
        }

        public bool EchoMessagesToConsole { get; set; }

        public int MaximumHits
        {
            get => mMaximumHits;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }

                if (value > MAXIMUM_ALLOWED_RESULTS_TO_FIND)
                {
                    value = MAXIMUM_ALLOWED_RESULTS_TO_FIND;
                }

                mMaximumHits = value;
            }
        }

        /// <summary>
        /// Percent complete, between 0 and 100
        /// </summary>
        /// <remarks></remarks>
        public double PercentComplete { get; private set; }

        #endregion

        #region "Events"
        public event MessageEventEventHandler MessageEvent;

        public delegate void MessageEventEventHandler(string strMessage);

        public event ErrorEventEventHandler ErrorEvent;

        public delegate void ErrorEventEventHandler(string strErrorMessage);

        public event WarningEventEventHandler WarningEvent;

        public delegate void WarningEventEventHandler(string strWarningMessage);
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks></remarks>
        public FormulaFinder(ElementAndMassTools oElementAndMassTools)
        {
            mElementAndMassRoutines = oElementAndMassTools;
            mCandidateElements = new Dictionary<string, CandidateElementTolerances>();

            EchoMessagesToConsole = true;

            Reset();
        }

        #region "Public Methods"

        /// <summary>
        /// Abort processing
        /// </summary>
        /// <remarks>Only useful if the formula finder is running on a separate thread from the calling program</remarks>
        public void AbortProcessingNow()
        {
            mAbortProcessing = true;
        }

        /// <summary>
        /// Add a candidate element, abbreviation, or monoisotopic mass
        /// </summary>
        /// <param name="elementSymbolAbbrevOrMass">Element symbol, abbreviation symbol, or monoisotopic mass</param>
        /// <remarks></remarks>
        public void AddCandidateElement(string elementSymbolAbbrevOrMass)
        {
            var udtElementTolerances = GetDefaultCandidateElementTolerance();

            AddCandidateElement(elementSymbolAbbrevOrMass, udtElementTolerances);
        }

        /// <summary>
        /// Add a candidate element, abbreviation, or monoisotopic mass
        /// </summary>
        /// <param name="elementSymbolAbbrevOrMass">Element symbol, abbreviation symbol, or monoisotopic mass</param>
        /// <param name="targetPercentComposition">Target percent composition</param>
        /// <remarks></remarks>
        public void AddCandidateElement(string elementSymbolAbbrevOrMass, double targetPercentComposition)
        {
            var udtElementTolerances = GetDefaultCandidateElementTolerance(targetPercentComposition);
            AddCandidateElement(elementSymbolAbbrevOrMass, udtElementTolerances);
        }

        /// <summary>
        /// Add a candidate element, abbreviation, or monoisotopic mass
        /// </summary>
        /// <param name="elementSymbolAbbrevOrMass">Element symbol, abbreviation symbol, or monoisotopic mass</param>
        /// <param name="minimumCount">Minimum occurrence count</param>
        /// <param name="maximumCount">Maximum occurrence count</param>
        /// <remarks>This method should be used when defining elements for a bounded search</remarks>
        public void AddCandidateElement(string elementSymbolAbbrevOrMass, int minimumCount, int maximumCount)
        {
            var udtElementTolerances = GetDefaultCandidateElementTolerance(minimumCount, maximumCount);
            AddCandidateElement(elementSymbolAbbrevOrMass, udtElementTolerances);
        }

        /// <summary>
        /// Add a candidate element, abbreviation, or monoisotopic mass
        /// </summary>
        /// <param name="elementSymbolAbbrevOrMass">Element symbol, abbreviation symbol, or monoisotopic mass</param>
        /// <param name="udtElementTolerances">Search tolerances, including % composition range and Min/Max count when using a bounded search</param>
        /// <remarks></remarks>
        public void AddCandidateElement(string elementSymbolAbbrevOrMass, CandidateElementTolerances udtElementTolerances)
        {
            if (mCandidateElements.ContainsKey(elementSymbolAbbrevOrMass))
            {
                mCandidateElements[elementSymbolAbbrevOrMass] = udtElementTolerances;
            }
            else
            {
                mCandidateElements.Add(elementSymbolAbbrevOrMass, udtElementTolerances);
            }
        }

        /// <summary>
        /// Find empirical formulas that match the given target mass, with the given ppm tolerance
        /// </summary>
        /// <param name="targetMass"></param>
        /// <param name="massTolerancePPM"></param>
        /// <returns></returns>
        /// <remarks>Uses default search options</remarks>
        public List<FormulaFinderResult> FindMatchesByMassPPM(double targetMass, double massTolerancePPM)
        {
            var lstResults = FindMatchesByMassPPM(targetMass, massTolerancePPM, null);

            // No need to sort because FindMatchesByMassPPM has already done so
            return lstResults;
        }

        /// <summary>
        /// Find empirical formulas that match the given target mass, with the given ppm tolerance
        /// </summary>
        /// <param name="targetMass"></param>
        /// <param name="massTolerancePPM"></param>
        /// <param name="searchOptions"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<FormulaFinderResult> FindMatchesByMassPPM(double targetMass, double massTolerancePPM, FormulaFinderOptions searchOptions)
        {
            var massToleranceDa = massTolerancePPM * targetMass / 1000000.0d;
            if (searchOptions == null)
                searchOptions = new FormulaFinderOptions();

            var lstResults = FindMatchesByMass(targetMass, massToleranceDa, searchOptions, true);

            var sortedResults = (from item in lstResults orderby item.SortKey select item).ToList();
            return sortedResults;
        }

        /// <summary>
        /// Find empirical formulas that match the given target mass, with the given tolerance
        /// </summary>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <returns></returns>
        /// <remarks>Uses default search options</remarks>
        public List<FormulaFinderResult> FindMatchesByMass(double targetMass, double massToleranceDa)
        {
            var lstResults = FindMatchesByMass(targetMass, massToleranceDa, null);

            // No need to sort because FindMatchesByMassPPM has already done so
            return lstResults;
        }

        /// <summary>
        /// Find empirical formulas that match the given target mass, with the given tolerance
        /// </summary>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <param name="searchOptions"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public List<FormulaFinderResult> FindMatchesByMass(double targetMass, double massToleranceDa, FormulaFinderOptions searchOptions)
        {
            if (searchOptions == null)
                searchOptions = new FormulaFinderOptions();

            var lstResults = FindMatchesByMass(targetMass, massToleranceDa, searchOptions, false);

            var sortedResults = (from item in lstResults orderby item.SortKey select item).ToList();
            return sortedResults;
        }

        public List<FormulaFinderResult> FindMatchesByPercentComposition(
            double maximumFormulaMass,
            double percentTolerance,
            FormulaFinderOptions searchOptions)
        {
            if (searchOptions == null)
                searchOptions = new FormulaFinderOptions();

            var lstResults = FindMatchesByPercentCompositionWork(maximumFormulaMass, percentTolerance, searchOptions);

            var sortedResults = (from item in lstResults orderby item.SortKey select item).ToList();
            return sortedResults;
        }

        /// <summary>
        /// Reset to defaults
        /// </summary>
        /// <remarks></remarks>
        public void Reset()
        {
            mCandidateElements.Clear();
            mCandidateElements.Add("C", GetDefaultCandidateElementTolerance(70d));
            mCandidateElements.Add("H", GetDefaultCandidateElementTolerance(10d));
            mCandidateElements.Add("N", GetDefaultCandidateElementTolerance(10d));
            mCandidateElements.Add("O", GetDefaultCandidateElementTolerance(10d));

            mAbortProcessing = false;

            MaximumHits = DEFAULT_RESULTS_TO_FIND;
        }

        #endregion

        private void AppendToEmpiricalFormula(StringBuilder sbEmpiricalFormula, string elementSymbol, int elementCount)
        {
            if (elementCount != 0)
            {
                sbEmpiricalFormula.Append(elementSymbol);

                if (elementCount > 1)
                {
                    sbEmpiricalFormula.Append(elementCount);
                }
            }
        }

        private void AppendPercentCompositionResult(
            FormulaFinderResult searchResult,
            int elementCount,
            IList<FormulaFinderCandidateElement> sortedElementStats,
            int targetIndex,
            double percentComposition)
        {
            if (elementCount != 0 && targetIndex < sortedElementStats.Count)
            {
                searchResult.PercentComposition.Add(sortedElementStats[targetIndex].Symbol, percentComposition);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="targetMass">Only used when calculationMode is MatchMolecularWeight</param>
        /// <param name="massToleranceDa">Only used when calculationMode is MatchMolecularWeigh</param>
        /// <param name="maximumFormulaMass">Only used when calculationMode is MatchPercentComposition</param>
        /// <param name="searchOptions"></param>
        /// <param name="ppmMode"></param>
        /// <param name="calculationMode"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private List<FormulaFinderResult> BoundedSearch(
            double targetMass,
            double massToleranceDa,
            double maximumFormulaMass,
            FormulaFinderOptions searchOptions,
            bool ppmMode,
            CalculationMode calculationMode,
            IList<FormulaFinderCandidateElement> sortedElementStats)
        {
            List<FormulaFinderResult> lstResults;

            if (searchOptions.FindTargetMZ)
            {
                // Searching for target m/z rather than target mass

                int mzSearchChargeMin;
                int mzSearchChargeMax;

                MultipleSearchMath(sortedElementStats.Count, searchOptions, out mzSearchChargeMin, out mzSearchChargeMax);

                lstResults = OldFormulaFinder(searchOptions, ppmMode, calculationMode, sortedElementStats, targetMass, massToleranceDa, maximumFormulaMass);
            }
            else
            {
                searchOptions.ChargeMin = 1;
                searchOptions.ChargeMax = 1;

                lstResults = OldFormulaFinder(searchOptions, ppmMode, calculationMode, sortedElementStats, targetMass, massToleranceDa, maximumFormulaMass);
            }

            ComputeSortKeys(lstResults);

            return lstResults;
        }

        private void ComputeSortKeys(IEnumerable<FormulaFinderResult> lstResults)
        {
            // Compute the sort key for each result
            var sbCodeString = new StringBuilder();

            foreach (var item in lstResults)
                item.SortKey = ComputeSortKey(sbCodeString, item.EmpiricalFormula);
        }

        private string ComputeSortKey(StringBuilder sbCodeString, string empiricalFormula)
        {
            // Precedence order for sbCodeString
            // C1_ C2_ C3_ C4_ C5_ C6_ C7_ C8_ C9_  a   z    1,  2,  3...
            // 1   2   3   4   5   6   7   8   9   10  35   36  37  38
            //
            // Custom elements are converted to Chr(1), Chr(2), etc.
            // Letters are converted to Chr(10) through Chr(35)
            // Number are converted to Chr(36) through Chr(255)
            //
            // 220 = Chr(0) + Chr(220+35) = Chr(0) + Chr(255)

            // 221 = Chr(CInt(Math.Floor(221+34/255))) + Chr((221 + 34) Mod 255 + 1)

            var charIndex = 0;
            var formulaLength = empiricalFormula.Length;

            sbCodeString.Clear();

            while (charIndex < formulaLength)
            {
                var strCurrentLetter = char.ToUpper(empiricalFormula[charIndex]);

                int parsedValue;
                if (char.IsLetter(strCurrentLetter))
                {
                    sbCodeString.Append('\0');

                    if (charIndex + 2 < formulaLength && empiricalFormula.Substring(charIndex + 2, 1) == "_")
                    {
                        // At a custom element, which are notated as "C1_", "C2_", etc.
                        // Give it a value of Chr(1) through Chr(10)
                        // Also, need to bump up charIndex by 2

                        var customElementNum = empiricalFormula.Substring(charIndex + 1, 1);

                        if (int.TryParse(customElementNum, out parsedValue))
                        {
                            sbCodeString.Append((char)parsedValue);
                        }
                        else
                        {
                            sbCodeString.Append('\u0001');
                        }

                        charIndex += 2;
                    }
                    else
                    {
                        // 65 is the ascii code for the letter a
                        // Thus, 65-55 = 10
                        int asciiValue = strCurrentLetter;
                        sbCodeString.Append((char)(asciiValue - 55));
                    }
                }
                else if (strCurrentLetter.ToString() != "_")
                {
                    // At a number, since empirical formulas can only have letters or numbers or underscores

                    var endIndex = charIndex;
                    while (endIndex + 1 < formulaLength)
                    {
                        var nextChar = empiricalFormula[endIndex + 1];
                        if (!int.TryParse(nextChar.ToString(), out parsedValue))
                        {
                            break;
                        }

                        endIndex += 1;
                    }

                    if (int.TryParse(empiricalFormula.Substring(charIndex, endIndex - charIndex + 1), out parsedValue))
                    {
                        if (parsedValue < 221)
                        {
                            sbCodeString.Append('\0');
                            sbCodeString.Append((char)(parsedValue + 35));
                        }
                        else
                        {
                            sbCodeString.Append((char)(int)Math.Round(Math.Floor(parsedValue + 34d / 255d)));
                            sbCodeString.Append((char)((parsedValue + 34) % 255 + 1));
                        }
                    }

                    charIndex = endIndex;
                }

                charIndex += 1;
            }

            return sbCodeString.ToString();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="totalMass"></param>
        /// <param name="totalCharge"></param>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <param name="intMultipleMtoZCharge"></param>
        /// <remarks>True if the m/z is within tolerance of the target</remarks>
        private bool CheckMtoZWithTarget(
            double totalMass,
            double totalCharge,
            double targetMass,
            double massToleranceDa,
            int intMultipleMtoZCharge)
        {
            double dblMtoZ;

            // The original target is the target m/z; assure this compound has that m/z
            if (Math.Abs(totalCharge) > 0.5d)
            {
                dblMtoZ = Math.Abs(totalMass / totalCharge);
            }
            else
            {
                dblMtoZ = 0d;
            }

            if (intMultipleMtoZCharge == 0)
            {
                return false;
            }

            var dblOriginalMtoZ = targetMass / intMultipleMtoZCharge;
            if (dblMtoZ < dblOriginalMtoZ - massToleranceDa || dblMtoZ > dblOriginalMtoZ + massToleranceDa)
            {
                // dblMtoZ is not within tolerance of dblOriginalMtoZ, so don't report the result
                return false;
            }

            return true;
        }

        private double Combinatorial(int a, int b)
        {
            if (a > 170 || b > 170)
            {
                Console.WriteLine("Cannot compute factorial of a number over 170.  Thus, cannot compute the combination.");
                return -1;
            }

            if (a < b)
            {
                Console.WriteLine("First number should be greater than or equal to the second number");
                return -1;
            }
            return Factorial(a) / (Factorial(b) * Factorial(a - b));
        }

        /// <summary>
        /// Construct the empirical formula and verify hydrogens
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="sbEmpiricalFormula"></param>
        /// <param name="count1"></param>
        /// <param name="count2"></param>
        /// <param name="count3"></param>
        /// <param name="count4"></param>
        /// <param name="count5"></param>
        /// <param name="count6"></param>
        /// <param name="count7"></param>
        /// <param name="count8"></param>
        /// <param name="count9"></param>
        /// <param name="count10"></param>
        /// <param name="totalMass"></param>
        /// <param name="targetMass">Only used when searchOptions.FindTargetMZ is true, and that is only valid when matching a target mass, not when matching percent composition values</param>
        /// <param name="massToleranceDa">Only used when searchOptions.FindTargetMZ is true</param>
        /// <param name="totalCharge"></param>
        /// <param name="intMultipleMtoZCharge">When searchOptions.FindTargetMZ is false, this will be 1; otherwise, the current charge being searched for</param>
        /// <returns>False if compound has too many hydrogens AND hydrogen checking is on, otherwise returns true</returns>
        /// <remarks>Common function to both molecular weight and percent composition matching</remarks>
        private bool ConstructAndVerifyCompound(
            FormulaFinderOptions searchOptions,
            StringBuilder sbEmpiricalFormula,
            int count1, int count2, int count3, int count4, int count5, int count6, int count7, int count8, int count9, int count10,
            IList<FormulaFinderCandidateElement> sortedElementStats,
            double totalMass,
            double targetMass,
            double massToleranceDa,
            double totalCharge,
            int intMultipleMtoZCharge,
            out Dictionary<string, int> empiricalResultSymbols,
            out double correctedCharge)
        {
            // This dictionary tracks the elements and abbreviations of the found formula so that they can be properly ordered according to empirical formula conventions
            // Key is the element or abbreviation symbol, value is the number of each element or abbreviation
            empiricalResultSymbols = new Dictionary<string, int>();

            sbEmpiricalFormula.Clear();

            try
            {
                // Convert to empirical formula and sort
                ConstructAndVerifyAddIfValid(sortedElementStats, empiricalResultSymbols, 0, count1);
                ConstructAndVerifyAddIfValid(sortedElementStats, empiricalResultSymbols, 1, count2);
                ConstructAndVerifyAddIfValid(sortedElementStats, empiricalResultSymbols, 2, count3);
                ConstructAndVerifyAddIfValid(sortedElementStats, empiricalResultSymbols, 3, count4);
                ConstructAndVerifyAddIfValid(sortedElementStats, empiricalResultSymbols, 4, count5);
                ConstructAndVerifyAddIfValid(sortedElementStats, empiricalResultSymbols, 5, count6);
                ConstructAndVerifyAddIfValid(sortedElementStats, empiricalResultSymbols, 6, count7);
                ConstructAndVerifyAddIfValid(sortedElementStats, empiricalResultSymbols, 7, count8);
                ConstructAndVerifyAddIfValid(sortedElementStats, empiricalResultSymbols, 8, count9);
                ConstructAndVerifyAddIfValid(sortedElementStats, empiricalResultSymbols, 9, count10);

                var valid = ConstructAndVerifyCompoundWork(searchOptions,
                                                           sbEmpiricalFormula,
                                                           totalMass, targetMass, massToleranceDa,
                                                           totalCharge, intMultipleMtoZCharge,
                                                           empiricalResultSymbols, out correctedCharge);

                return valid;
            }
            catch (Exception ex)
            {
                mElementAndMassRoutines.GeneralErrorHandler("ConstructAndVerifyCompound", ex);
                correctedCharge = totalCharge;
                return false;
            }
        }

        private void ConstructAndVerifyAddIfValid(
            IList<FormulaFinderCandidateElement> sortedElementStats,
            IDictionary<string, int> empiricalResultSymbols,
            int targetElementIndex,
            int currentCount)
        {
            if (currentCount != 0 && targetElementIndex < sortedElementStats.Count)
            {
                empiricalResultSymbols.Add(sortedElementStats[targetElementIndex].Symbol, currentCount);
            }
        }

        /// <summary>
        /// Construct the empirical formula and verify hydrogens
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="sbEmpiricalFormula"></param>
        /// <param name="lstPotentialElementPointers"></param>
        /// <param name="totalMass"></param>
        /// <param name="targetMass">Only used when searchOptions.FindTargetMZ is true, and that is only valid when matching a target mass, not when matching percent composition values</param>
        /// <param name="massToleranceDa">Only used when searchOptions.FindTargetMZ is true</param>
        /// <param name="totalCharge"></param>
        /// <param name="intMultipleMtoZCharge">When searchOptions.FindTargetMZ is false, this will be 0; otherwise, the current charge being searched for</param>
        /// <returns>False if compound has too many hydrogens AND hydrogen checking is on, otherwise returns true</returns>
        /// <remarks>Common function to both molecular weight and percent composition matching</remarks>
        private bool ConstructAndVerifyCompoundRecursive(
            FormulaFinderOptions searchOptions,
            StringBuilder sbEmpiricalFormula,
            IList<FormulaFinderCandidateElement> sortedElementStats,
            IEnumerable<int> lstPotentialElementPointers,
            double totalMass,
            double targetMass,
            double massToleranceDa,
            double totalCharge,
            int intMultipleMtoZCharge,
            out Dictionary<string, int> empiricalResultSymbols,
            out double correctedCharge)
        {
            sbEmpiricalFormula.Clear();

            try
            {
                // The empiricalResultSymbols dictionary tracks the elements and abbreviations of the found formula
                // so that they can be properly ordered according to empirical formula conventions
                // Keys are the element or abbreviation symbol, value is the number of each element or abbreviation
                empiricalResultSymbols = ConvertElementPointersToElementStats(sortedElementStats, lstPotentialElementPointers);

                var valid = ConstructAndVerifyCompoundWork(searchOptions,
                                                           sbEmpiricalFormula,
                                                           totalMass, targetMass, massToleranceDa,
                                                           totalCharge, intMultipleMtoZCharge,
                                                           empiricalResultSymbols, out correctedCharge);

                // Uncomment to debug
                // Dim computedMass = mElementAndMassRoutines.ComputeFormulaWeight(sbEmpiricalFormula.ToString())
                // If Math.Abs(computedMass - totalMass) > massToleranceDa Then
                // Console.WriteLine("Wrong result: " + sbEmpiricalFormula.ToString())
                // End If

                return valid;
            }
            catch (Exception ex)
            {
                mElementAndMassRoutines.GeneralErrorHandler("ConstructAndVerifyCompoundRecursive", ex);
                empiricalResultSymbols = new Dictionary<string, int>();
                correctedCharge = totalCharge;
                return false;
            }
        }

        private int[] GetElementCountArray(
            int potentialElementCount,
            IEnumerable<int> lstNewPotentialElementPointers)
        {
            // Store the occurrence count of each element
            var elementCountArray = new int[potentialElementCount];

            foreach (var elementIndex in lstNewPotentialElementPointers)
                elementCountArray[elementIndex] += 1;

            return elementCountArray;
        }

        private bool ConstructAndVerifyCompoundWork(
            FormulaFinderOptions searchOptions,
            StringBuilder sbEmpiricalFormula,
            double totalMass,
            double targetMass,
            double massToleranceDa,
            double totalCharge,
            int intMultipleMtoZCharge,
            Dictionary<string, int> empiricalResultSymbols,
            out double correctedCharge)
        {
            correctedCharge = totalCharge;

            // Convert to a formatted empirical formula (elements order by C, H, then alphabetical)

            // First find C
            if (empiricalResultSymbols.TryGetValue("C", out var matchCount))
            {
                sbEmpiricalFormula.Append("C");
                if (matchCount > 1)
                    sbEmpiricalFormula.Append(matchCount);
            }

            // Next find H
            if (empiricalResultSymbols.TryGetValue("H", out matchCount))
            {
                sbEmpiricalFormula.Append("H");
                if (matchCount > 1)
                    sbEmpiricalFormula.Append(matchCount);
            }

            var query = from item in empiricalResultSymbols where item.Key != "C" && item.Key != "H" orderby item.Key select item;

            foreach (var result in query)
            {
                sbEmpiricalFormula.Append(result.Key);
                if (result.Value > 1)
                    sbEmpiricalFormula.Append(result.Value);
            }

            if (!searchOptions.VerifyHydrogens && !searchOptions.FindTargetMZ)
            {
                return true;
            }

            // Verify that the formula does not have too many hydrogens

            // Counters for elements of interest (hydrogen, carbon, silicon, nitrogen, phosphorus, chlorine, iodine, florine, bromine, and other)
            var udtElementNum = new ElementNum();

            // Determine number of C, Si, N, P, O, S, Cl, I, F, Br and H atoms
            foreach (var item in empiricalResultSymbols)
            {
                switch (item.Key ?? "")
                {
                    case "C":
                        udtElementNum.C += item.Value;
                        break;
                    case "Si":
                        udtElementNum.Si += item.Value;
                        break;
                    case "N":
                        udtElementNum.N += item.Value;
                        break;
                    case "P":
                        udtElementNum.P += item.Value;
                        break;
                    case "O":
                        udtElementNum.O += item.Value;
                        break;
                    case "S":
                        udtElementNum.S += item.Value;
                        break;
                    case "Cl":
                        udtElementNum.Cl += item.Value;
                        break;
                    case "I":
                        udtElementNum.I += item.Value;
                        break;
                    case "F":
                        udtElementNum.F += item.Value;
                        break;
                    case "Br":
                        udtElementNum.Br += item.Value;
                        break;
                    case "H":
                        udtElementNum.H += item.Value;
                        break;
                    default:
                        udtElementNum.Other += item.Value;
                        break;
                }
            }

            var maxH = 0;

            // Compute maximum number of hydrogens
            if (udtElementNum.Si == 0 && udtElementNum.C == 0 && udtElementNum.N == 0 &&
                udtElementNum.P == 0 && udtElementNum.Other == 0 &&
                (udtElementNum.O > 0 || udtElementNum.S > 0))
            {
                // Only O and S
                maxH = 3;
            }
            else
            {
                // Formula is: [#C*2 + 3 - (2 if N or P present)] + [#N + 3 - (1 if C or Si present)] + [#other elements * 4 + 3], where we assume other elements can have a coordination Number of up to 7
                if (udtElementNum.C > 0 || udtElementNum.Si > 0)
                {
                    maxH += (udtElementNum.C + udtElementNum.Si) * 2 + 3;
                    // If udtElementNum.N > 0 Or udtElementNum.P > 0 Then maxH = maxH - 2
                }

                if (udtElementNum.N > 0 || udtElementNum.P > 0)
                {
                    maxH += udtElementNum.N + udtElementNum.P + 3;
                    // If udtElementNum.C > 0 Or udtElementNum.Si > 0 Then maxH = maxH - 1
                }

                // Correction for carbon contribution
                // If (udtElementNum.C > 0 Or udtElementNum.Si > 0) And (udtElementNum.N > 0 Or udtElementNum.P > 0) Then udtElementNum.H = udtElementNum.H - 2

                // Correction for nitrogen contribution
                // If (udtElementNum.N > 0 Or udtElementNum.P > 0) And (udtElementNum.C > 0 Or udtElementNum.Si > 0) Then udtElementNum.H = udtElementNum.H - 1

                // Combine the above two commented out if's to obtain:
                if ((udtElementNum.N > 0 || udtElementNum.P > 0) && (udtElementNum.C > 0 || udtElementNum.Si > 0))
                {
                    maxH -= 3;
                }

                if (udtElementNum.Other > 0)
                    maxH += udtElementNum.Other * 4 + 3;
            }

            // correct for if H only
            if (maxH < 3)
                maxH = 3;

            // correct for halogens
            maxH = maxH - udtElementNum.F - udtElementNum.Cl - udtElementNum.Br - udtElementNum.I;

            // correct for negative udtElementNum.H
            if (maxH < 0)
                maxH = 0;

            // Verify H's
            var blnHOK = udtElementNum.H <= maxH;

            // Only proceed if hydrogens check out
            if (!blnHOK)
            {
                return false;
            }

            bool chargeOK;

            // See if totalCharge is within charge limits (chargeOK will be set to True or False by CorrectChargeEmpirical)
            if (searchOptions.FindCharge)
            {
                correctedCharge = CorrectChargeEmpirical(searchOptions, totalCharge, udtElementNum, out chargeOK);
            }
            else
            {
                chargeOK = true;
            }

            // If charge is within range and checking for multiples, see if correct m/z too
            if (chargeOK && searchOptions.FindTargetMZ)
            {
                chargeOK = CheckMtoZWithTarget(totalMass, correctedCharge, targetMass,
                                               massToleranceDa, intMultipleMtoZCharge);
            }

            return chargeOK;
        }

        private Dictionary<string, int> ConvertElementPointersToElementStats(
            IList<FormulaFinderCandidateElement> sortedElementStats,
            IEnumerable<int> lstPotentialElementPointers)
        {
            // This dictionary tracks the elements and abbreviations of the found formula so that they can be properly ordered according to empirical formula conventions
            // Key is the element or abbreviation symbol, value is the number of each element or abbreviation
            var empiricalResultSymbols = new Dictionary<string, int>();

            var elementCountArray = GetElementCountArray(sortedElementStats.Count, lstPotentialElementPointers);

            for (var intIndex = 0; intIndex < sortedElementStats.Count; intIndex++)
            {
                if (elementCountArray[intIndex] != 0)
                {
                    empiricalResultSymbols.Add(sortedElementStats[intIndex].Symbol, elementCountArray[intIndex]);
                }
            }

            return empiricalResultSymbols;
        }

        /// <summary>
        /// Correct charge using rules for an empirical formula
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="totalCharge"></param>
        /// <param name="udtElementNum"></param>
        /// <param name="chargeOK"></param>
        /// <returns>Corrected charge</returns>
        /// <remarks></remarks>
        private double CorrectChargeEmpirical(
            FormulaFinderOptions searchOptions,
            double totalCharge,
            ElementNum udtElementNum,
            out bool chargeOK)
        {
            var correctedCharge = totalCharge;

            if (udtElementNum.C + udtElementNum.Si >= 1)
            {
                if (udtElementNum.H > 0 && Math.Abs(mElementAndMassRoutines.GetElementStatInternal(1, MolecularWeightTool.ElementStatsType.Charge) - 1d) < float.Epsilon)
                {
                    // Since carbon or silicon are present, assume the hydrogens should be negative
                    // Subtract udtElementNum.H * 2 since hydrogen is assigned a +1 charge if ElementStats(1).Charge = 1
                    correctedCharge -= udtElementNum.H * 2;
                }

                // Correct for udtElementNumber of C and Si
                if (udtElementNum.C + udtElementNum.Si > 1)
                {
                    correctedCharge -= (udtElementNum.C + udtElementNum.Si - 1) * 2;
                }
            }

            if (udtElementNum.N + udtElementNum.P > 0 && udtElementNum.C > 0)
            {
                // Assume 2 hydrogens around each Nitrogen or Phosphorus, thus add back +2 for each H
                // First, decrease udtElementNumber of halogens by udtElementNumber of hydrogens & halogens taken up by the carbons
                // Determine # of H taken up by all the carbons in a compound without N or P, then add back 1 H for each N and P
                var intNumHalogens = udtElementNum.H + udtElementNum.F + udtElementNum.Cl + udtElementNum.Br + udtElementNum.I;
                intNumHalogens = intNumHalogens - (udtElementNum.C * 2 + 2) + udtElementNum.N + udtElementNum.P;

                if (intNumHalogens >= 0)
                {
                    for (var intIndex = 1; intIndex <= udtElementNum.N + udtElementNum.P; intIndex++)
                    {
                        correctedCharge += 2d;
                        intNumHalogens -= 1;

                        if (intNumHalogens <= 0)
                        {
                            break;
                        }

                        correctedCharge += 2d;
                        intNumHalogens -= 1;
                        if (intNumHalogens <= 0)
                            break;
                    }
                }
            }

            if (searchOptions.LimitChargeRange)
            {
                // Make sure correctedCharge is within the specified range
                if (correctedCharge >= searchOptions.ChargeMin &&
                    correctedCharge <= searchOptions.ChargeMax)
                {
                    // Charge is within range
                    chargeOK = true;
                }
                else
                {
                    chargeOK = false;
                }
            }
            else
            {
                chargeOK = true;
            }

            return correctedCharge;
        }

        /// <summary>
        /// Search empiricalResultSymbols for the elements in targetCountStats
        /// </summary>
        /// <param name="empiricalResultSymbols"></param>
        /// <param name="targetCountStats"></param>
        /// <returns>True if all of the elements are present in the given counts (extra elements may also be present),
        /// false one or more is not found or has the wrong occurrence count</returns>
        /// <remarks></remarks>
        private bool EmpiricalFormulaHasElementCounts(
            IDictionary<string, int> empiricalResultSymbols,
            Dictionary<string, int> targetCountStats)
        {
            foreach (var targetElement in targetCountStats)
            {
                if (!empiricalResultSymbols.TryGetValue(targetElement.Key, out var empiricalElementCount))
                {
                    return false;
                }

                if (empiricalElementCount != targetElement.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private void EstimateNumberOfOperations(int potentialElementCount, int multipleSearchMax = 0)
        {
            // Estimate the number of operations that will be performed
            mRecursiveCount = 0;

            if (potentialElementCount == 1)
            {
                mMaxRecursiveCount = 1;
                return;
            }

            const int NUM_POINTERS = 3;

            // Calculate lngMaxRecursiveCount based on a combination function
            var maxRecursiveCount = Combinatorial(NUM_POINTERS + potentialElementCount, potentialElementCount - 1) - Combinatorial(potentialElementCount + NUM_POINTERS - 2, NUM_POINTERS - 1);
            if (maxRecursiveCount > int.MaxValue)
            {
                mMaxRecursiveCount = int.MaxValue;
            }
            else
            {
                mMaxRecursiveCount = (int)Math.Round(maxRecursiveCount);
            }

            if (multipleSearchMax > 0)
            {
                // Correct lngMaxRecursiveCount for searching for m/z values
                mMaxRecursiveCount *= multipleSearchMax;
            }
        }

        /// <summary>
        /// Compute the factorial of a number; uses recursion
        /// </summary>
        /// <param name="value">Integer between 0 and 170</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private double Factorial(int value)
        {
            if (value > 170)
            {
                throw new Exception("Cannot compute factorial of a number over 170");
            }

            if (value < 0)
            {
                throw new Exception("Cannot compute factorial of a negative number");
            }

            if (value == 0)
            {
                return 1d;
            }

            return value * Factorial(value - 1);
        }

        private List<FormulaFinderResult> FindMatchesByMass(
            double targetMass,
            double massToleranceDa,
            FormulaFinderOptions searchOptions,
            bool ppmMode)
        {
            // Validate the Inputs
            if (!ValidateSettings(CalculationMode.MatchMolecularWeight))
            {
                return new List<FormulaFinderResult>();
            }

            if (targetMass <= 0d)
            {
                ReportError("Target molecular weight must be greater than 0");
                return new List<FormulaFinderResult>();
            }

            if (massToleranceDa < 0d)
            {
                ReportError("Mass tolerance cannot be negative");
                return new List<FormulaFinderResult>();
            }

            var candidateElementsStats = GetCandidateElements();

            if (candidateElementsStats.Count == 0)
            {
                return new List<FormulaFinderResult>();
            }

            var sortedElementStats = (from item in candidateElementsStats orderby item.Mass descending select item).ToList();

            if (searchOptions.SearchMode == FormulaFinderOptions.SearchModes.Thorough)
            {
                // Thorough search

                EstimateNumberOfOperations(sortedElementStats.Count);

                // Pointers to the potential elements
                var lstPotentialElementPointers = new List<int>();

                var lstResults = new List<FormulaFinderResult>();

                if (searchOptions.FindTargetMZ)
                {
                    // Searching for target m/z rather than target mass

                    MultipleSearchMath(sortedElementStats.Count, searchOptions, out var mzSearchChargeMin, out var mzSearchChargeMax);

                    for (var currentMzCharge = mzSearchChargeMin; currentMzCharge <= mzSearchChargeMax; currentMzCharge++)
                        // Call the RecursiveMWFinder repeatedly, sending dblTargetWeight * x each time to search for target, target*2, target*3, etc.
                        RecursiveMWFinder(lstResults, searchOptions, ppmMode, sortedElementStats, 0, lstPotentialElementPointers, 0d, targetMass * currentMzCharge, massToleranceDa, 0d, currentMzCharge);
                }
                else
                {
                    // RecursiveMWFinder(lstResults, searchOptions, ppmMode, strPotentialElements, dblPotentialElementStats, 0, potentialElementCount, lstPotentialElementPointers, 0, targetMass, massToleranceDa, 0, 0)
                    RecursiveMWFinder(lstResults, searchOptions, ppmMode, sortedElementStats, 0, lstPotentialElementPointers, 0d, targetMass, massToleranceDa, 0d, 0);
                }

                ComputeSortKeys(lstResults);

                return lstResults;
            }

            // Bounded search
            const int maximumFormulaMass = 0;

            var boundedSearchResults = BoundedSearch(targetMass, massToleranceDa, maximumFormulaMass,
                searchOptions, ppmMode, CalculationMode.MatchMolecularWeight,
                sortedElementStats);

            ComputeSortKeys(boundedSearchResults);

            return boundedSearchResults;
        }

        private List<FormulaFinderResult> FindMatchesByPercentCompositionWork(
            double maximumFormulaMass,
            double percentTolerance,
            FormulaFinderOptions searchOptions)
        {
            // Validate the Inputs
            if (!ValidateSettings(CalculationMode.MatchPercentComposition))
            {
                return new List<FormulaFinderResult>();
            }

            if (maximumFormulaMass <= 0d)
            {
                ReportError("Maximum molecular weight must be greater than 0");
                return new List<FormulaFinderResult>();
            }

            if (percentTolerance < 0d)
            {
                ReportError("Percent tolerance cannot be negative");
                return new List<FormulaFinderResult>();
            }

            var candidateElementsStats = GetCandidateElements(percentTolerance);

            if (candidateElementsStats.Count == 0)
            {
                return new List<FormulaFinderResult>();
            }

            var sortedElementStats = (from item in candidateElementsStats orderby item.Mass descending select item).ToList();

            if (searchOptions.SearchMode == FormulaFinderOptions.SearchModes.Thorough)
            {
                // Thorough search

                EstimateNumberOfOperations(sortedElementStats.Count);

                // Pointers to the potential elements
                var lstPotentialElementPointers = new List<int>();

                var lstResults = new List<FormulaFinderResult>();

                RecursivePCompFinder(lstResults, searchOptions, sortedElementStats, 0, lstPotentialElementPointers, 0d, maximumFormulaMass, 9d);

                ComputeSortKeys(lstResults);

                return lstResults;
            }
            // Bounded search

            const int targetMass = 0;
            const int massToleranceDa = 0;
            const bool ppmMode = false;

            var boundedSearchResults = BoundedSearch(targetMass, massToleranceDa, maximumFormulaMass,
                searchOptions, ppmMode, CalculationMode.MatchPercentComposition,
                sortedElementStats);

            ComputeSortKeys(boundedSearchResults);

            return boundedSearchResults;
        }

        private List<FormulaFinderCandidateElement> GetCandidateElements(double percentTolerance = 0d)
        {
            var candidateElementsStats = new List<FormulaFinderCandidateElement>();

            var customElementCounter = 0;

            foreach (var item in mCandidateElements)
            {
                var candidateElement = new FormulaFinderCandidateElement(item.Key)
                {
                    CountMinimum = item.Value.MinimumCount,
                    CountMaximum = item.Value.MaximumCount
                };

                float sngCharge;
                double dblMass;
                if (mElementAndMassRoutines.IsValidElementSymbol(item.Key))
                {
                    var elementID = mElementAndMassRoutines.GetElementIDInternal(item.Key);

                    mElementAndMassRoutines.GetElementInternal(elementID, out _, out dblMass, out _, out sngCharge, out _);

                    candidateElement.Mass = dblMass;
                    candidateElement.Charge = sngCharge;
                }
                else
                {
                    // Working with an abbreviation or simply a mass

                    if (double.TryParse(item.Key, out var customMass))
                    {
                        // Custom element, only weight given so charge is 0
                        candidateElement.Mass = customMass;
                        candidateElement.Charge = 0d;

                        customElementCounter += 1;

                        // Custom elements are named C1_ or C2_ or C3_ etc.
                        candidateElement.Symbol = "C" + customElementCounter + "_";
                    }
                    else
                    {
                        // A single element or abbreviation was entered

                        // Convert input to default format of first letter capitalized and rest lowercase
                        var abbrevSymbol = item.Key.Substring(0, 1).ToUpper() + item.Key.Substring(1).ToLower();

                        foreach (var currentChar in abbrevSymbol)
                        {
                            if (!(char.IsLetter(currentChar) || currentChar.ToString() == "+" || currentChar.ToString() == "_"))
                            {
                                ReportError("Custom elemental weights must contain only numbers or only letters; if letters are used, they must be for a single valid elemental symbol or abbreviation");
                                return new List<FormulaFinderCandidateElement>();
                            }
                        }

                        if (string.IsNullOrWhiteSpace(abbrevSymbol))
                        {
                            // Too short
                            ReportError("Custom elemental weight is empty; if letters are used, they must be for a single valid elemental symbol or abbreviation");
                            return new List<FormulaFinderCandidateElement>();
                        }

                        // See if this is an abbreviation
                        var intSymbolReference = mElementAndMassRoutines.GetAbbreviationIDInternal(abbrevSymbol);
                        if (intSymbolReference < 1)
                        {
                            ReportError("Unknown element or abbreviation for custom elemental weight: " + abbrevSymbol);
                            return new List<FormulaFinderCandidateElement>();
                        }

                        // Found a normal abbreviation
                        mElementAndMassRoutines.GetAbbreviationInternal(intSymbolReference, out _, out var abbrevFormula, out sngCharge, out _);

                        dblMass = mElementAndMassRoutines.ComputeFormulaWeight(abbrevFormula);

                        candidateElement.Mass = dblMass;

                        candidateElement.Charge = sngCharge;
                    }
                }

                candidateElement.PercentCompMinimum = item.Value.TargetPercentComposition - percentTolerance;  // Lower bound of target percentage
                candidateElement.PercentCompMaximum = item.Value.TargetPercentComposition + percentTolerance;  // Upper bound of target percentage

                candidateElementsStats.Add(candidateElement);
            }

            return candidateElementsStats;
        }

        [Obsolete("Deprecated")]
        private int GetCandidateElements(
            double percentTolerance,
            int[,] intRange,
            double[,] dblPotentialElementStats,
            IList<string> strPotentialElements,
            double[,] dblTargetPercents)
        {
            var potentialElementCount = 0;
            var customElementCounter = 0;

            foreach (var item in mCandidateElements)
            {
                intRange[potentialElementCount, 0] = item.Value.MinimumCount;
                intRange[potentialElementCount, 1] = item.Value.MaximumCount;

                float sngCharge;
                double dblMass;
                if (mElementAndMassRoutines.IsValidElementSymbol(item.Key))
                {
                    var elementID = mElementAndMassRoutines.GetElementIDInternal(item.Key);

                    mElementAndMassRoutines.GetElementInternal(elementID, out _, out dblMass, out _, out sngCharge, out _);

                    dblPotentialElementStats[potentialElementCount, 0] = dblMass;
                    dblPotentialElementStats[potentialElementCount, 1] = sngCharge;

                    strPotentialElements[potentialElementCount] = item.Key;
                }
                else
                {
                    // Working with an abbreviation or simply a mass

                    if (double.TryParse(item.Key, out var customMass))
                    {
                        // Custom element, only weight given so charge is 0
                        dblPotentialElementStats[potentialElementCount, 0] = customMass;
                        dblPotentialElementStats[potentialElementCount, 1] = 0d;

                        customElementCounter += 1;

                        // Custom elements are named C1_ or C2_ or C3_ etc.
                        strPotentialElements[potentialElementCount] = "C" + customElementCounter + "_";
                    }
                    else
                    {
                        // A single element or abbreviation was entered

                        // Convert input to default format of first letter capitalized and rest lowercase
                        var abbrevSymbol = item.Key.Substring(0).ToUpper() + item.Key.Substring(1).ToLower();

                        foreach (var currentChar in abbrevSymbol)
                        {
                            if (!(char.IsLetter(currentChar) || currentChar.ToString() == "+" || currentChar.ToString() == "_"))
                            {
                                ReportError("Custom elemental weights must contain only numbers or only letters; if letters are used, they must be for a single valid elemental symbol or abbreviation");
                                return 0;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(abbrevSymbol))
                        {
                            // Too short
                            ReportError("Custom elemental weight is empty; if letters are used, they must be for a single valid elemental symbol or abbreviation");
                            return 0;
                        }

                        const int charge = 0;

                        // See if this is an abbreviation
                        var intSymbolReference = mElementAndMassRoutines.GetAbbreviationIDInternal(abbrevSymbol);
                        if (intSymbolReference < 1)
                        {
                            ReportError("Unknown element or abbreviation for custom elemental weight: " + abbrevSymbol);
                            return 0;
                        }

                        // Found a normal abbreviation
                        mElementAndMassRoutines.GetAbbreviationInternal(intSymbolReference, out var matchedAbbrevSymbol, out var abbrevFormula, out sngCharge, out _);

                        dblMass = mElementAndMassRoutines.ComputeFormulaWeight(abbrevFormula);

                        // Returns weight of element/abbreviation, but also charge
                        dblPotentialElementStats[potentialElementCount, 0] = dblMass;

                        dblPotentialElementStats[potentialElementCount, 1] = charge;

                        // No problems, store symbol
                        strPotentialElements[potentialElementCount] = matchedAbbrevSymbol;
                    }
                }

                dblTargetPercents[potentialElementCount, 0] = item.Value.TargetPercentComposition - percentTolerance;  // Lower bound of target percentage
                dblTargetPercents[potentialElementCount, 1] = item.Value.TargetPercentComposition + percentTolerance;  // Upper bound of target percentage

                potentialElementCount += 1;
            }

            return potentialElementCount;
        }

        private CandidateElementTolerances GetDefaultCandidateElementTolerance()
        {
            return GetDefaultCandidateElementTolerance(0d);
        }

        private CandidateElementTolerances GetDefaultCandidateElementTolerance(int minimumCount, int maximumCount)
        {
            var udtElementTolerances = new CandidateElementTolerances()
            {
                MinimumCount = minimumCount,    // Only used with the Bounded search mode
                MaximumCount = maximumCount,    // Only used with the Bounded search mode
                TargetPercentComposition = 0d   // Only used when searching for percent compositions
            };

            return udtElementTolerances;
        }

        private CandidateElementTolerances GetDefaultCandidateElementTolerance(double targetPercentComposition)
        {
            var udtElementTolerances = new CandidateElementTolerances()
            {
                MinimumCount = 0,               // Only used with the Bounded search mode
                MaximumCount = 10,              // Only used with the Bounded search mode
                TargetPercentComposition = targetPercentComposition   // Only used when searching for percent compositions
            };

            return udtElementTolerances;
        }

        /// <summary>
        /// Initializes a new search result
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="ppmMode"></param>
        /// <param name="sbEmpiricalFormula"></param>
        /// <param name="totalMass">If 0 or negative, means matching percent compositions, so don't want to add dm= to line</param>
        /// <param name="targetMass"></param>
        /// <param name="totalCharge"></param>
        /// <remarks></remarks>
        private FormulaFinderResult GetSearchResult(
            FormulaFinderOptions searchOptions,
            bool ppmMode,
            StringBuilder sbEmpiricalFormula,
            double totalMass,
            double targetMass,
            double totalCharge,
            Dictionary<string, int> empiricalResultSymbols)
        {
            try
            {
                var searchResult = new FormulaFinderResult(sbEmpiricalFormula.ToString(), empiricalResultSymbols);

                if (searchOptions.FindCharge)
                {
                    searchResult.ChargeState = (int)Math.Round(Math.Round(totalCharge));
                }

                if (targetMass > 0d)
                {
                    if (ppmMode)
                    {
                        searchResult.Mass = totalMass;
                        searchResult.DeltaMass = (totalMass / targetMass - 1d) * 1000000.0d;
                        searchResult.DeltaMassIsPPM = true;
                    }
                    else
                    {
                        searchResult.Mass = totalMass;
                        searchResult.DeltaMass = totalMass - targetMass;
                        searchResult.DeltaMassIsPPM = false;
                    }
                }
                else
                {
                    searchResult.Mass = totalMass;
                }

                if (searchOptions.FindCharge && Math.Abs(totalCharge) > 0.1d)
                {
                    // Compute m/z value
                    searchResult.MZ = Math.Abs(totalMass / totalCharge);
                }

                return searchResult;
            }
            catch (Exception ex)
            {
                mElementAndMassRoutines.GeneralErrorHandler("GetSearchResult", ex);
                return new FormulaFinderResult(string.Empty, new Dictionary<string, int>());
            }
        }

        private double GetTotalPercentComposition()
        {
            var totalTargetPercentComp = mCandidateElements.Sum(item => item.Value.TargetPercentComposition);
            return totalTargetPercentComp;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="potentialElementCount"></param>
        /// <param name="searchOptions"></param>
        /// <remarks>searchOptions is passed ByRef because it is a value type and .MzChargeMin and .MzChargeMax are updated</remarks>
        private void MultipleSearchMath(
            int potentialElementCount,
            FormulaFinderOptions searchOptions,
            out int mzSearchChargeMin,
            out int mzSearchChargeMax)
        {
            mzSearchChargeMin = searchOptions.ChargeMin;
            mzSearchChargeMax = searchOptions.ChargeMax;

            mzSearchChargeMax = Math.Max(Math.Abs(mzSearchChargeMin), Math.Abs(mzSearchChargeMax));
            mzSearchChargeMin = 1;

            if (mzSearchChargeMax < mzSearchChargeMin)
                mzSearchChargeMax = mzSearchChargeMin;

            EstimateNumberOfOperations(potentialElementCount, mzSearchChargeMax - mzSearchChargeMin + 1);
        }

        /// <summary>
        /// Formula finder that uses a series of nested for loops and is thus slow when a large number of candidate elements
        /// or when elements have a large range of potential counts
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="ppmMode"></param>
        /// <param name="calculationMode"></param>
        /// <param name="targetMass">Only used when calculationMode is MatchMolecularWeight</param>
        /// <param name="massToleranceDa">Only used when calculationMode is MatchMolecularWeigh</param>
        /// <param name="maximumFormulaMass">Only used when calculationMode is MatchPercentComposition</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private List<FormulaFinderResult> OldFormulaFinder(
            FormulaFinderOptions searchOptions,
            bool ppmMode,
            CalculationMode calculationMode,
            IList<FormulaFinderCandidateElement> sortedElementStats,
            double targetMass,
            double massToleranceDa,
            double maximumFormulaMass)
        {
            // The calculated percentages for the specific compound
            var Percent = new double[11];

            var lstResults = new List<FormulaFinderResult>();
            try
            {

                // Only used when calculationMode is MatchMolecularWeight
                var dblMultipleSearchMaxWeight = targetMass * searchOptions.ChargeMax;

                var sbEmpiricalFormula = new StringBuilder();

                var lstRanges = new List<BoundedSearchRange>();

                for (var elementIndex = 0; elementIndex < sortedElementStats.Count; elementIndex++)
                {
                    var udtBoundedSearchRange = new BoundedSearchRange()
                    {
                        Min = sortedElementStats[elementIndex].CountMinimum,
                        Max = sortedElementStats[elementIndex].CountMaximum
                    };
                    lstRanges.Add(udtBoundedSearchRange);
                }

                while (lstRanges.Count < MAX_MATCHING_ELEMENTS)
                {
                    var udtBoundedSearchRange = new BoundedSearchRange()
                    {
                        Min = 0,
                        Max = 0
                    };
                    lstRanges.Add(udtBoundedSearchRange);
                }

                var potentialElementCount = sortedElementStats.Count;

                // Determine the valid compounds
                for (var j = lstRanges[0].Min; j <= lstRanges[0].Max; j++)
                {
                    for (var k = lstRanges[1].Min; k <= lstRanges[1].Max; k++)
                    {
                        for (var l = lstRanges[2].Min; l <= lstRanges[2].Max; l++)
                        {
                            for (var m = lstRanges[3].Min; m <= lstRanges[3].Max; m++)
                            {
                                for (var N = lstRanges[4].Min; N <= lstRanges[4].Max; N++)
                                {
                                    for (var O = lstRanges[5].Min; O <= lstRanges[5].Max; O++)
                                    {
                                        for (var P = lstRanges[6].Min; P <= lstRanges[6].Max; P++)
                                        {
                                            for (var q = lstRanges[7].Min; q <= lstRanges[7].Max; q++)
                                            {
                                                for (var r = lstRanges[8].Min; r <= lstRanges[8].Max; r++)
                                                {
                                                    for (var s = lstRanges[9].Min; s <= lstRanges[9].Max; s++)
                                                    {
                                                        var totalMass = j * sortedElementStats[0].Mass;
                                                        var totalCharge = j * sortedElementStats[0].Charge;

                                                        if (potentialElementCount > 1)
                                                        {
                                                            totalMass += k * sortedElementStats[1].Mass;
                                                            totalCharge += k * sortedElementStats[1].Charge;

                                                            if (potentialElementCount > 2)
                                                            {
                                                                totalMass += l * sortedElementStats[2].Mass;
                                                                totalCharge += l * sortedElementStats[2].Charge;

                                                                if (potentialElementCount > 3)
                                                                {
                                                                    totalMass += m * sortedElementStats[3].Mass;
                                                                    totalCharge += m * sortedElementStats[3].Charge;

                                                                    if (potentialElementCount > 4)
                                                                    {
                                                                        totalMass += N * sortedElementStats[4].Mass;
                                                                        totalCharge += N * sortedElementStats[4].Charge;

                                                                        if (potentialElementCount > 5)
                                                                        {
                                                                            totalMass += O * sortedElementStats[5].Mass;
                                                                            totalCharge += O * sortedElementStats[5].Charge;

                                                                            if (potentialElementCount > 6)
                                                                            {
                                                                                totalMass += P * sortedElementStats[6].Mass;
                                                                                totalCharge += P * sortedElementStats[6].Charge;

                                                                                if (potentialElementCount > 7)
                                                                                {
                                                                                    totalMass += q * sortedElementStats[7].Mass;
                                                                                    totalCharge += q * sortedElementStats[7].Charge;

                                                                                    if (potentialElementCount > 8)
                                                                                    {
                                                                                        totalMass += r * sortedElementStats[8].Mass;
                                                                                        totalCharge += r * sortedElementStats[8].Charge;

                                                                                        if (potentialElementCount > 9)
                                                                                        {
                                                                                            totalMass += s * sortedElementStats[9].Mass;
                                                                                            totalCharge += s * sortedElementStats[9].Charge;
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        if (calculationMode == CalculationMode.MatchPercentComposition)
                                                        {
                                                            // Matching Percent Compositions
                                                            if (totalMass > 0d && totalMass <= maximumFormulaMass)
                                                            {
                                                                Percent[0] = j * sortedElementStats[0].Mass / totalMass * 100d;
                                                                if (potentialElementCount > 1)
                                                                {
                                                                    Percent[1] = k * sortedElementStats[1].Mass / totalMass * 100d;

                                                                    if (potentialElementCount > 1)
                                                                    {
                                                                        Percent[2] = l * sortedElementStats[2].Mass / totalMass * 100d;

                                                                        if (potentialElementCount > 1)
                                                                        {
                                                                            Percent[3] = m * sortedElementStats[3].Mass / totalMass * 100d;

                                                                            if (potentialElementCount > 1)
                                                                            {
                                                                                Percent[4] = N * sortedElementStats[4].Mass / totalMass * 100d;

                                                                                if (potentialElementCount > 1)
                                                                                {
                                                                                    Percent[5] = O * sortedElementStats[5].Mass / totalMass * 100d;

                                                                                    if (potentialElementCount > 1)
                                                                                    {
                                                                                        Percent[6] = P * sortedElementStats[6].Mass / totalMass * 100d;

                                                                                        if (potentialElementCount > 1)
                                                                                        {
                                                                                            Percent[7] = q * sortedElementStats[7].Mass / totalMass * 100d;

                                                                                            if (potentialElementCount > 1)
                                                                                            {
                                                                                                Percent[8] = r * sortedElementStats[8].Mass / totalMass * 100d;

                                                                                                if (potentialElementCount > 1)
                                                                                                {
                                                                                                    Percent[9] = s * sortedElementStats[9].Mass / totalMass * 100d;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                var intSubTrack = 0;
                                                                for (var intIndex = 0; intIndex < potentialElementCount; intIndex++)
                                                                {
                                                                    if (Percent[intIndex] >= sortedElementStats[intIndex].PercentCompMinimum && Percent[intIndex] <= sortedElementStats[intIndex].PercentCompMaximum)
                                                                    {
                                                                        intSubTrack += 1;
                                                                    }
                                                                }

                                                                if (intSubTrack == potentialElementCount)
                                                                {
                                                                    // All of the elements have percent compositions matching the target

                                                                    // Construct the empirical formula and verify hydrogens
                                                                    var blnHOK = ConstructAndVerifyCompound(searchOptions,
                                                                                                            sbEmpiricalFormula,
                                                                                                            j, k, l, m, N, O, P, q, r, s,
                                                                                                            sortedElementStats,
                                                                                                            totalMass, targetMass, massToleranceDa,
                                                                                                            totalCharge, 0,
                                                                                                            out var empiricalResultSymbols,
                                                                                                            out var correctedCharge);

                                                                    if (sbEmpiricalFormula.Length > 0 && blnHOK)
                                                                    {
                                                                        var searchResult = GetSearchResult(searchOptions, ppmMode, sbEmpiricalFormula, totalMass, -1, correctedCharge, empiricalResultSymbols);

                                                                        // Add % composition info

                                                                        AppendPercentCompositionResult(searchResult, j, sortedElementStats, 0, Percent[0]);
                                                                        AppendPercentCompositionResult(searchResult, k, sortedElementStats, 1, Percent[1]);
                                                                        AppendPercentCompositionResult(searchResult, l, sortedElementStats, 2, Percent[2]);
                                                                        AppendPercentCompositionResult(searchResult, m, sortedElementStats, 3, Percent[3]);
                                                                        AppendPercentCompositionResult(searchResult, N, sortedElementStats, 4, Percent[4]);
                                                                        AppendPercentCompositionResult(searchResult, O, sortedElementStats, 5, Percent[5]);
                                                                        AppendPercentCompositionResult(searchResult, P, sortedElementStats, 6, Percent[6]);
                                                                        AppendPercentCompositionResult(searchResult, q, sortedElementStats, 7, Percent[7]);
                                                                        AppendPercentCompositionResult(searchResult, r, sortedElementStats, 8, Percent[8]);
                                                                        AppendPercentCompositionResult(searchResult, s, sortedElementStats, 9, Percent[9]);

                                                                        lstResults.Add(searchResult);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        // Matching Molecular Weights

                                                        else if (totalMass <= dblMultipleSearchMaxWeight + massToleranceDa)
                                                        {

                                                            // When searchOptions.FindTargetMZ is false, ChargeMin and ChargeMax will be 1
                                                            for (var intCurrentCharge = searchOptions.ChargeMin; intCurrentCharge <= searchOptions.ChargeMax; intCurrentCharge++)
                                                            {
                                                                var dblMatchWeight = targetMass * intCurrentCharge;
                                                                if (totalMass <= dblMatchWeight + massToleranceDa && totalMass >= dblMatchWeight - massToleranceDa)
                                                                {
                                                                    // Within massToleranceDa

                                                                    // Construct the empirical formula and verify hydrogens
                                                                    var blnHOK = ConstructAndVerifyCompound(searchOptions,
                                                                                                            sbEmpiricalFormula,
                                                                                                            j, k, l, m, N, O, P, q, r, s,
                                                                                                            sortedElementStats,
                                                                                                            totalMass, targetMass * intCurrentCharge, massToleranceDa,
                                                                                                            totalCharge, intCurrentCharge,
                                                                                                            out var empiricalResultSymbols,
                                                                                                            out var correctedCharge);

                                                                    if (sbEmpiricalFormula.Length > 0 && blnHOK)
                                                                    {
                                                                        var searchResult = GetSearchResult(searchOptions, ppmMode, sbEmpiricalFormula, totalMass, targetMass, correctedCharge, empiricalResultSymbols);

                                                                        lstResults.Add(searchResult);
                                                                    }

                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {

                                                            // Jump out of loop since weight is too high
                                                            // Determine which variable is causing the weight to be too high
                                                            // Incrementing "s" would definitely make the weight too high, so set it to its max (so it will zero and increment "r")
                                                            s = lstRanges[9].Max;
                                                            if (j * lstRanges[0].Min + k * lstRanges[1].Min + l * lstRanges[2].Min + m * lstRanges[3].Min + N * lstRanges[4].Min + O * lstRanges[5].Min + P * lstRanges[6].Min + q * lstRanges[7].Min + (r + 1) * lstRanges[8].Min > massToleranceDa + dblMultipleSearchMaxWeight)
                                                            {
                                                                // Incrementing r would make the weight too high, so set it to its max (so it will zero and increment q)
                                                                r = lstRanges[8].Max;
                                                                if (j * lstRanges[0].Min + k * lstRanges[1].Min + l * lstRanges[2].Min + m * lstRanges[3].Min + N * lstRanges[4].Min + O * lstRanges[5].Min + P * lstRanges[6].Min + (q + 1) * lstRanges[7].Min > massToleranceDa + dblMultipleSearchMaxWeight)
                                                                {
                                                                    q = lstRanges[7].Max;
                                                                    if (j * lstRanges[0].Min + k * lstRanges[1].Min + l * lstRanges[2].Min + m * lstRanges[3].Min + N * lstRanges[4].Min + O * lstRanges[5].Min + (P + 1) * lstRanges[6].Min > massToleranceDa + dblMultipleSearchMaxWeight)
                                                                    {
                                                                        P = lstRanges[6].Max;
                                                                        if (j * lstRanges[0].Min + k * lstRanges[1].Min + l * lstRanges[2].Min + m * lstRanges[3].Min + N * lstRanges[4].Min + (O + 1) * lstRanges[5].Min > massToleranceDa + dblMultipleSearchMaxWeight)
                                                                        {
                                                                            O = lstRanges[5].Max;
                                                                            if (j * lstRanges[0].Min + k * lstRanges[1].Min + l * lstRanges[2].Min + m * lstRanges[3].Min + (N + 1) * lstRanges[4].Min > massToleranceDa + dblMultipleSearchMaxWeight)
                                                                            {
                                                                                N = lstRanges[4].Max;
                                                                                if (j * lstRanges[0].Min + k * lstRanges[1].Min + l * lstRanges[2].Min + (m + 1) * lstRanges[3].Min > massToleranceDa + dblMultipleSearchMaxWeight)
                                                                                {
                                                                                    m = lstRanges[3].Max;
                                                                                    if (j * lstRanges[0].Min + k * lstRanges[1].Min + (l + 1) * lstRanges[2].Min > massToleranceDa + dblMultipleSearchMaxWeight)
                                                                                    {
                                                                                        l = lstRanges[2].Max;
                                                                                        if (j * lstRanges[0].Min + (k + 1) * lstRanges[1].Min > massToleranceDa + dblMultipleSearchMaxWeight)
                                                                                        {
                                                                                            k = lstRanges[1].Max;
                                                                                            if ((j + 1) * lstRanges[0].Min > massToleranceDa + dblMultipleSearchMaxWeight)
                                                                                            {
                                                                                                j = lstRanges[0].Max;
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }

                                                        if (mAbortProcessing)
                                                        {
                                                            return lstResults;
                                                        }

                                                        if (lstResults.Count >= mMaximumHits)
                                                        {

                                                            // Set variables to their maximum so all the loops will end
                                                            j = lstRanges[0].Max;
                                                            k = lstRanges[1].Max;
                                                            l = lstRanges[2].Max;
                                                            m = lstRanges[3].Max;
                                                            N = lstRanges[4].Max;
                                                            O = lstRanges[5].Max;
                                                            P = lstRanges[6].Max;
                                                            q = lstRanges[7].Max;
                                                            r = lstRanges[8].Max;
                                                            s = lstRanges[9].Max;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (lstRanges[0].Max != 0)
                    {
                        if (searchOptions.ChargeMin == 0)
                        {
                            PercentComplete = j / (double)lstRanges[0].Max * 100d;
                        }
                        else
                        {
                            PercentComplete = j / (double)lstRanges[0].Max * 100d * searchOptions.ChargeMax;
                        }

                        Console.WriteLine("Bounded search: " + PercentComplete.ToString("0") + "% complete");
                    }
                }
            }
            catch (Exception ex)
            {
                mElementAndMassRoutines.GeneralErrorHandler("OldFormulaFinder", ex);
            }

            return lstResults;
        }

        [Obsolete("Deprecated")]
        private void SortCandidateElements(
            CalculationMode calculationMode,
            int potentialElementCount,
            double[,] dblPotentialElementStats,
            IList<string> strPotentialElements,
            double[,] dblTargetPercents)
        {
            // Reorder dblPotentialElementStats pointer array in order from heaviest to lightest element
            // Greatly speeds up the recursive routine

            // Bubble sort
            for (var y = potentialElementCount - 1; y >= 1; y -= 1)       // Sort from end to start
            {
                for (var x = 0; x < y; x++)
                {
                    if (dblPotentialElementStats[x, 0] < dblPotentialElementStats[x + 1, 0])
                    {
                        // Swap the element symbols
                        var strSwap = strPotentialElements[x];
                        strPotentialElements[x] = strPotentialElements[x + 1];
                        strPotentialElements[x + 1] = strSwap;

                        // and their weights
                        var dblSwapVal = dblPotentialElementStats[x, 0];
                        dblPotentialElementStats[x, 0] = dblPotentialElementStats[x + 1, 0];
                        dblPotentialElementStats[x + 1, 0] = dblSwapVal;

                        // and their charge
                        dblSwapVal = dblPotentialElementStats[x, 1];
                        dblPotentialElementStats[x, 1] = dblPotentialElementStats[x + 1, 1];
                        dblPotentialElementStats[x + 1, 1] = dblSwapVal;

                        if (calculationMode == CalculationMode.MatchPercentComposition)
                        {
                            // and the dblTargetPercents array
                            dblSwapVal = dblTargetPercents[x, 0];
                            dblTargetPercents[x, 0] = dblTargetPercents[x + 1, 0];
                            dblTargetPercents[x + 1, 0] = dblSwapVal;

                            dblSwapVal = dblTargetPercents[x, 1];
                            dblTargetPercents[x, 1] = dblTargetPercents[x + 1, 1];
                            dblTargetPercents[x + 1, 1] = dblSwapVal;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recursively search for a target mass
        /// </summary>
        /// <param name="lstResults"></param>
        /// <param name="searchOptions"></param>
        /// <param name="sortedElementStats">Candidate elements, including mass and charge. Sorted by de</param>
        /// <param name="intStartIndex">Index in candidateElementsStats to start at</param>
        /// <param name="lstPotentialElementPointers">Pointers to the elements that have been added to the potential formula so far</param>
        /// <param name="dblPotentialMassTotal">Weight of the potential formula</param>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <param name="potentialChargeTotal"></param>
        /// <param name="intMultipleMtoZCharge">When searchOptions.FindTargetMZ is false, this will be 0; otherwise, the current charge being searched for</param>
        /// <remarks></remarks>
        private void RecursiveMWFinder(
            ICollection<FormulaFinderResult> lstResults,
            FormulaFinderOptions searchOptions,
            bool ppmMode,
            IList<FormulaFinderCandidateElement> sortedElementStats,
            int intStartIndex,
            IReadOnlyCollection<int> lstPotentialElementPointers,
            double dblPotentialMassTotal,
            double targetMass,
            double massToleranceDa,
            double potentialChargeTotal,
            int intMultipleMtoZCharge)
        {
            try
            {
                var lstNewPotentialElementPointers = new List<int>(lstPotentialElementPointers.Count + 1);

                if (mAbortProcessing || lstResults.Count >= mMaximumHits)
                {
                    return;
                }

                var sbEmpiricalFormula = new StringBuilder();

                for (var intCurrentIndex = intStartIndex; intCurrentIndex < sortedElementStats.Count; intCurrentIndex++)
                {
                    var totalMass = dblPotentialMassTotal + sortedElementStats[intCurrentIndex].Mass;
                    var totalCharge = potentialChargeTotal + sortedElementStats[intCurrentIndex].Charge;

                    lstNewPotentialElementPointers.Clear();

                    if (totalMass <= targetMass + massToleranceDa)
                    {
                        // Below or within dblMassTolerance, add current element's pointer to pointer array
                        lstNewPotentialElementPointers.AddRange(lstPotentialElementPointers);

                        // Append the current element's number
                        lstNewPotentialElementPointers.Add(intCurrentIndex);

                        // Update status
                        UpdateStatus();

                        // Uncomment to add a breakpoint when a certain empirical formula is encountered
                        if (lstPotentialElementPointers.Count >= 3)
                        {
                            var empiricalResultSymbols = ConvertElementPointersToElementStats(sortedElementStats, lstPotentialElementPointers);
                            var debugCompound = new Dictionary<string, int>()
                            {
                                { "C", 7 },
                                { "H", 4 },
                                { "O", 7 }
                            };

                            if (EmpiricalFormulaHasElementCounts(empiricalResultSymbols, debugCompound))
                            {
                                Console.WriteLine("Debug: Check this formula");
                            }
                        }

                        if (mAbortProcessing || lstResults.Count >= mMaximumHits)
                        {
                            return;
                        }

                        if (totalMass >= targetMass - massToleranceDa)
                        {
                            // Matching compound

                            // Construct the empirical formula and verify hydrogens
                            var blnHOK = ConstructAndVerifyCompoundRecursive(searchOptions,
                                                                             sbEmpiricalFormula, sortedElementStats,
                                                                             lstNewPotentialElementPointers,
                                                                             totalMass, targetMass, massToleranceDa,
                                                                             totalCharge, intMultipleMtoZCharge,
                                                                             out var empiricalResultSymbols,
                                                                             out var correctedCharge);

                            if (sbEmpiricalFormula.Length > 0 && blnHOK)
                            {
                                var searchResult = GetSearchResult(searchOptions, ppmMode, sbEmpiricalFormula, totalMass, targetMass, correctedCharge, empiricalResultSymbols);

                                lstResults.Add(searchResult);
                            }
                        }

                        // Haven't reached targetMass - dblMassTolerance region, so call RecursiveFinder again

                        // But first, if adding the lightest element (i.e. the last in the list),
                        // add a bunch of it until the potential compound's weight is close to the target
                        if (intCurrentIndex == sortedElementStats.Count - 1)
                        {
                            var intExtra = 0;
                            while (totalMass < targetMass - massToleranceDa - sortedElementStats[intCurrentIndex].Mass)
                            {
                                intExtra += 1;
                                totalMass += sortedElementStats[intCurrentIndex].Mass;
                                totalCharge += sortedElementStats[intCurrentIndex].Charge;
                            }

                            if (intExtra > 0)
                            {
                                for (var intPointer = 1; intPointer <= intExtra; intPointer++)
                                    lstNewPotentialElementPointers.Add(intCurrentIndex);
                            }
                        }

                        // Now recursively call this sub
                        RecursiveMWFinder(lstResults, searchOptions, ppmMode, sortedElementStats, intCurrentIndex, lstNewPotentialElementPointers, totalMass, targetMass, massToleranceDa, totalCharge, intMultipleMtoZCharge);
                    }
                }
            }
            catch (Exception ex)
            {
                mElementAndMassRoutines.GeneralErrorHandler("RecursiveMWFinder", ex);
                mAbortProcessing = true;
            }
        }

        /// <summary>
        /// Recursively search for target percent composition values
        /// </summary>
        /// <param name="lstResults"></param>
        /// <param name="intStartIndex"></param>
        /// <param name="lstPotentialElementPointers">Pointers to the elements that have been added to the potential formula so far</param>
        /// <param name="dblPotentialMassTotal">>Weight of the potential formula</param>
        /// <param name="maximumFormulaMass"></param>
        /// <param name="potentialChargeTotal"></param>
        /// <remarks></remarks>
        private void RecursivePCompFinder(
            ICollection<FormulaFinderResult> lstResults,
            FormulaFinderOptions searchOptions,
            IList<FormulaFinderCandidateElement> sortedElementStats,
            int intStartIndex,
            ICollection<int> lstPotentialElementPointers,
            double dblPotentialMassTotal,
            double maximumFormulaMass,
            double potentialChargeTotal)
        {
            try
            {
                var lstNewPotentialElementPointers = new List<int>(lstPotentialElementPointers.Count + 1);

                var dblPotentialPercents = new double[sortedElementStats.Count + 1];

                if (mAbortProcessing || lstResults.Count >= mMaximumHits)
                {
                    return;
                }

                var sbEmpiricalFormula = new StringBuilder();
                const bool ppmMode = false;

                for (var intCurrentIndex = intStartIndex; intCurrentIndex < sortedElementStats.Count; intCurrentIndex++)  // potentialElementCount >= 1, if 1, means just dblPotentialElementStats[0,0], etc.
                {
                    var totalMass = dblPotentialMassTotal + sortedElementStats[intCurrentIndex].Mass;
                    var totalCharge = potentialChargeTotal + sortedElementStats[intCurrentIndex].Charge;

                    lstNewPotentialElementPointers.Clear();

                    if (totalMass <= maximumFormulaMass)
                    {
                        // only proceed if weight is less than max weight

                        lstNewPotentialElementPointers.AddRange(lstPotentialElementPointers);

                        // Append the current element's number
                        lstNewPotentialElementPointers.Add(intCurrentIndex);

                        // Compute the number of each element
                        var elementCountArray = GetElementCountArray(sortedElementStats.Count, lstNewPotentialElementPointers);

                        var nonZeroCount = (from item in elementCountArray where item > 0 select item).Count();

                        // Only proceed if all elements are present
                        if (nonZeroCount == sortedElementStats.Count)
                        {
                            // Compute % comp of each element
                            for (var intIndex = 0; intIndex < sortedElementStats.Count; intIndex++)
                                dblPotentialPercents[intIndex] = elementCountArray[intIndex] * sortedElementStats[intIndex].Mass / totalMass * 100d;

                            // If intPointerCount = 0 Then dblPotentialPercents(0) = 100

                            var intPercentTrack = 0;
                            for (var intIndex = 0; intIndex < sortedElementStats.Count; intIndex++)
                            {
                                if (dblPotentialPercents[intIndex] >= sortedElementStats[intIndex].PercentCompMinimum &&
                                    dblPotentialPercents[intIndex] <= sortedElementStats[intIndex].PercentCompMaximum)
                                {
                                    intPercentTrack += 1;
                                }
                            }

                            if (intPercentTrack == sortedElementStats.Count)
                            {
                                // Matching compound

                                // Construct the empirical formula and verify hydrogens
                                var blnHOK = ConstructAndVerifyCompoundRecursive(searchOptions,
                                                                                 sbEmpiricalFormula, sortedElementStats,
                                                                                 lstNewPotentialElementPointers,
                                                                                 totalMass, 0d, 0d,
                                                                                 totalCharge, 0,
                                                                                 out var empiricalResultSymbols,
                                                                                 out var correctedCharge);

                                if (sbEmpiricalFormula.Length > 0 && blnHOK)
                                {
                                    var searchResult = GetSearchResult(searchOptions, ppmMode, sbEmpiricalFormula, totalMass, -1, correctedCharge, empiricalResultSymbols);

                                    // Add % composition info
                                    for (var intIndex = 0; intIndex < sortedElementStats.Count; intIndex++)
                                    {
                                        if (elementCountArray[intIndex] != 0)
                                        {
                                            var percentComposition = elementCountArray[intIndex] * sortedElementStats[intIndex].Mass / totalMass * 100d;

                                            AppendPercentCompositionResult(searchResult, elementCountArray[intIndex], sortedElementStats, intIndex, percentComposition);
                                        }
                                    }

                                    lstResults.Add(searchResult);
                                }
                            }
                        }

                        // Update status
                        UpdateStatus();

                        if (mAbortProcessing || lstResults.Count >= mMaximumHits)
                        {
                            return;
                        }

                        // Haven't reached maximumFormulaMass
                        // Now recursively call this sub
                        RecursivePCompFinder(lstResults, searchOptions, sortedElementStats, intCurrentIndex, lstNewPotentialElementPointers, totalMass, maximumFormulaMass, totalCharge);
                    }
                }
            }
            catch (Exception ex)
            {
                mElementAndMassRoutines.GeneralErrorHandler("RecursivePCompFinder", ex);
                mAbortProcessing = true;
            }
        }

        protected void ReportError(string strErrorMessage)
        {
            if (EchoMessagesToConsole)
                Console.WriteLine(strErrorMessage);

            ErrorEvent?.Invoke(strErrorMessage);
        }

        protected void ReportWarning(string strWarningMessage)
        {
            if (EchoMessagesToConsole)
                Console.WriteLine(strWarningMessage);

            WarningEvent?.Invoke(strWarningMessage);
        }

        protected void ShowMessage(string strMessage)
        {
            if (EchoMessagesToConsole)
                Console.WriteLine(strMessage);
            MessageEvent?.Invoke(strMessage);
        }

        private void UpdateStatus()
        {
            mRecursiveCount += 1;

            if (mRecursiveCount <= mMaxRecursiveCount)
            {
                PercentComplete = mRecursiveCount / (float)mMaxRecursiveCount * 100f;
            }
        }

        private void ValidateBoundedSearchValues()
        {
            foreach (var elementSymbol in mCandidateElements.Keys)
            {
                var udtElementTolerances = mCandidateElements[elementSymbol];

                if (udtElementTolerances.MinimumCount < 0 || udtElementTolerances.MaximumCount > MAX_BOUNDED_SEARCH_COUNT)
                {
                    if (udtElementTolerances.MinimumCount < 0)
                        udtElementTolerances.MinimumCount = 0;
                    if (udtElementTolerances.MaximumCount > MAX_BOUNDED_SEARCH_COUNT)
                        udtElementTolerances.MaximumCount = MAX_BOUNDED_SEARCH_COUNT;

                    mCandidateElements[elementSymbol] = udtElementTolerances;
                }
            }
        }

        private void ValidatePercentCompositionValues()
        {
            foreach (var elementSymbol in mCandidateElements.Keys)
            {
                var udtElementTolerances = mCandidateElements[elementSymbol];

                if (udtElementTolerances.TargetPercentComposition < 0d || udtElementTolerances.TargetPercentComposition > 100d)
                {
                    if (udtElementTolerances.TargetPercentComposition < 0d)
                        udtElementTolerances.TargetPercentComposition = 0d;
                    if (udtElementTolerances.TargetPercentComposition > 100d)
                        udtElementTolerances.TargetPercentComposition = 100d;

                    mCandidateElements[elementSymbol] = udtElementTolerances;
                }
            }
        }

        private bool ValidateSettings(CalculationMode calculationMode)
        {
            if (mCandidateElements.Count == 0)
            {
                ReportError("No candidate elements are defined; use method AddCandidateElement or property CandidateElements");
                return false;
            }

            ValidateBoundedSearchValues();

            if (calculationMode == CalculationMode.MatchPercentComposition)
            {
                var totalTargetPercentComp = GetTotalPercentComposition();

                if (Math.Abs(totalTargetPercentComp - 100d) > float.Epsilon)
                {
                    ReportError("Sum of the target percentages must be 100%; it is currently " + totalTargetPercentComp.ToString("0.0") + "%");
                    return false;
                }
            }

            return true;
        }
    }
}
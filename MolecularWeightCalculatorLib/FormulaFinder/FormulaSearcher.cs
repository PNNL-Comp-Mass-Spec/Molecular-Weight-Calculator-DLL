using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MolecularWeightCalculator.COMInterfaces;
using MolecularWeightCalculator.Formula;

namespace MolecularWeightCalculator.FormulaFinder
{
    [Guid("B24497AD-A29D-4266-A33A-EF97B86EA578"), ClassInterface(ClassInterfaceType.None), ComSourceInterfaces(typeof(IFormulaSearcherEvents)), ComVisible(true)]
    public class FormulaSearcher : IFormulaSearcher, IFormulaSearcherEvents
    {
        // Ignore Spelling: Da, interop, MtoZ

        #region "Constants"
        private const int MAX_MATCHING_ELEMENTS = 10;
        public const int DEFAULT_RESULTS_TO_FIND = 1000;
        public const int MAXIMUM_ALLOWED_RESULTS_TO_FIND = 1000000;

        public const int MAX_BOUNDED_SEARCH_COUNT = 65565;
        #endregion

        #region "Private data classes and Enums"

        private class ElementNum
        {
            public int H { get; set; }
            public int C { get; set; }
            public int Si { get; set; }
            public int N { get; set; }
            public int P { get; set; }
            public int O { get; set; }
            public int S { get; set; }
            public int Cl { get; set; }
            public int I { get; set; }
            public int F { get; set; }
            public int Br { get; set; }
            public int Other { get; set; }
        }

        private class BoundedSearchRange
        {
            public int Min { get; set; }
            public int Max { get; set; }
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
        public double PercentComplete { get; private set; }

        #endregion

        #region "Events"
        public event MessageEventEventHandler MessageEvent;
        public event ErrorEventEventHandler ErrorEvent;
        public event WarningEventEventHandler WarningEvent;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public FormulaSearcher(ElementAndMassTools elementAndMassTools)
        {
            mElementAndMassRoutines = elementAndMassTools;
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
        public void AddCandidateElement(string elementSymbolAbbrevOrMass)
        {
            var elementTolerances = GetDefaultCandidateElementTolerance();

            AddCandidateElement(elementSymbolAbbrevOrMass, elementTolerances);
        }

        /// <summary>
        /// Add a candidate element, abbreviation, or monoisotopic mass
        /// </summary>
        /// <param name="elementSymbolAbbrevOrMass">Element symbol, abbreviation symbol, or monoisotopic mass</param>
        /// <param name="targetPercentComposition">Target percent composition</param>
        public void AddCandidateElement(string elementSymbolAbbrevOrMass, double targetPercentComposition)
        {
            var elementTolerances = GetDefaultCandidateElementTolerance(targetPercentComposition);
            AddCandidateElement(elementSymbolAbbrevOrMass, elementTolerances);
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
            var elementTolerances = GetDefaultCandidateElementTolerance(minimumCount, maximumCount);
            AddCandidateElement(elementSymbolAbbrevOrMass, elementTolerances);
        }

        /// <summary>
        /// Add a candidate element, abbreviation, or monoisotopic mass
        /// </summary>
        /// <param name="elementSymbolAbbrevOrMass">Element symbol, abbreviation symbol, or monoisotopic mass</param>
        /// <param name="elementTolerances">Search tolerances, including % composition range and Min/Max count when using a bounded search</param>
        public void AddCandidateElement(string elementSymbolAbbrevOrMass, CandidateElementTolerances elementTolerances)
        {
            if (mCandidateElements.ContainsKey(elementSymbolAbbrevOrMass))
            {
                mCandidateElements[elementSymbolAbbrevOrMass] = elementTolerances;
            }
            else
            {
                mCandidateElements.Add(elementSymbolAbbrevOrMass, elementTolerances);
            }
        }

        /// <summary>
        /// Find empirical formulas that match the given target mass, with the given ppm tolerance
        /// </summary>
        /// <param name="targetMass"></param>
        /// <param name="massTolerancePPM"></param>
        /// <param name="searchOptions">If null, uses default search options</param>
        public List<SearchResult> FindMatchesByMassPPM(double targetMass, double massTolerancePPM, SearchOptions searchOptions = null)
        {
            var massToleranceDa = massTolerancePPM * targetMass / 1000000.0d;
            if (searchOptions == null)
                searchOptions = new SearchOptions();

            var results = FindMatchesByMass(targetMass, massToleranceDa, searchOptions, true);

            var sortedResults = (from item in results orderby item.SortKey select item).ToList();
            return sortedResults;
        }

        /// <summary>
        /// Find empirical formulas that match the given target mass, with the given ppm tolerance, getting results in an array for COM interop support
        /// </summary>
        /// <param name="targetMass"></param>
        /// <param name="massTolerancePPM"></param>
        /// <param name="searchOptions">If null, uses default search options</param>
        public SearchResult[] FindMatchesByMassPPMGetArray(double targetMass, double massTolerancePPM, SearchOptions searchOptions = null)
        {
            return FindMatchesByMassPPM(targetMass, massTolerancePPM, searchOptions).ToArray();
        }

        /// <summary>
        /// Find empirical formulas that match the given target mass, with the given tolerance
        /// </summary>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <param name="searchOptions">If null, uses default search options</param>
        public List<SearchResult> FindMatchesByMass(double targetMass, double massToleranceDa, SearchOptions searchOptions = null)
        {
            if (searchOptions == null)
                searchOptions = new SearchOptions();

            var results = FindMatchesByMass(targetMass, massToleranceDa, searchOptions, false);

            var sortedResults = (from item in results orderby item.SortKey select item).ToList();
            return sortedResults;
        }

        /// <summary>
        /// Find empirical formulas that match the given target mass, with the given tolerance, getting results in an array for COM interop support
        /// </summary>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <param name="searchOptions">If null, uses default search options</param>
        public SearchResult[] FindMatchesByMassGetArray(double targetMass, double massToleranceDa, SearchOptions searchOptions = null)
        {
            return FindMatchesByMass(targetMass, massToleranceDa, searchOptions).ToArray();
        }

        public List<SearchResult> FindMatchesByPercentComposition(
            double maximumFormulaMass,
            double percentTolerance,
            SearchOptions searchOptions)
        {
            if (searchOptions == null)
                searchOptions = new SearchOptions();

            var results = FindMatchesByPercentCompositionWork(maximumFormulaMass, percentTolerance, searchOptions);

            var sortedResults = (from item in results orderby item.SortKey select item).ToList();
            return sortedResults;
        }

        public SearchResult[] FindMatchesByPercentCompositionGetArray(double maximumFormulaMass, double percentTolerance, SearchOptions searchOptions)
        {
            // This version exists solely for COM interop support
            return FindMatchesByPercentComposition(maximumFormulaMass, percentTolerance, searchOptions).ToArray();
        }

        /// <summary>
        /// Reset to defaults
        /// </summary>
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

        private void AppendToEmpiricalFormula(StringBuilder empiricalFormula, string elementSymbol, int elementCount)
        {
            if (elementCount != 0)
            {
                empiricalFormula.Append(elementSymbol);

                if (elementCount > 1)
                {
                    empiricalFormula.Append(elementCount);
                }
            }
        }

        private void AppendPercentCompositionResult(
            SearchResult searchResult,
            int elementCount,
            IList<CandidateElement> sortedElementStats,
            int targetIndex,
            double percentComposition)
        {
            if (elementCount != 0 && targetIndex < sortedElementStats.Count)
            {
                searchResult.PercentComposition.Add(sortedElementStats[targetIndex].Symbol, percentComposition);
            }
        }

        /// <summary>
        /// Perform a bounded search
        /// </summary>
        /// <param name="targetMass">Only used when calculationMode is MatchMolecularWeight</param>
        /// <param name="massToleranceDa">Only used when calculationMode is MatchMolecularWeigh</param>
        /// <param name="maximumFormulaMass">Only used when calculationMode is MatchPercentComposition</param>
        /// <param name="searchOptions"></param>
        /// <param name="ppmMode"></param>
        /// <param name="calculationMode"></param>
        /// <param name="sortedElementStats"></param>
        private List<SearchResult> BoundedSearch(
            double targetMass,
            double massToleranceDa,
            double maximumFormulaMass,
            SearchOptions searchOptions,
            bool ppmMode,
            CalculationMode calculationMode,
            IList<CandidateElement> sortedElementStats)
        {
            List<SearchResult> results;

            if (searchOptions.FindTargetMz)
            {
                // Searching for target m/z rather than target mass

                MultipleSearchMath(sortedElementStats.Count, searchOptions, out _, out _);

                results = OldFormulaFinder(searchOptions, ppmMode, calculationMode, sortedElementStats, targetMass, massToleranceDa, maximumFormulaMass);
            }
            else
            {
                searchOptions.ChargeMin = 1;
                searchOptions.ChargeMax = 1;

                results = OldFormulaFinder(searchOptions, ppmMode, calculationMode, sortedElementStats, targetMass, massToleranceDa, maximumFormulaMass);
            }

            ComputeSortKeys(results);

            return results;
        }

        private void ComputeSortKeys(IEnumerable<SearchResult> results)
        {
            // Compute the sort key for each result
            var codeString = new StringBuilder();

            foreach (var item in results)
                item.SortKey = ComputeSortKey(codeString, item.EmpiricalFormula);
        }

        private string ComputeSortKey(StringBuilder codeString, string empiricalFormula)
        {
            // Precedence order for codeString
            // C1_ C2_ C3_ C4_ C5_ C6_ C7_ C8_ C9_  a   z    1,  2,  3...
            // 1   2   3   4   5   6   7   8   9   10  35   36  37  38
            //
            // Custom elements are converted to (char)1, (char)2, etc.
            // Letters are converted to (char)10 through (char)35
            // Number are converted to (char)36 through (char)255
            //
            // 220 = (char)0 + (char)(220+35) = (char)0 + (char)255

            // 221 = (char)CInt(Math.Floor(221 + 34/255))) + (char)((221 + 34) % 255 + 1))

            var charIndex = 0;
            var formulaLength = empiricalFormula.Length;

            codeString.Clear();

            while (charIndex < formulaLength)
            {
                var currentLetter = char.ToUpper(empiricalFormula[charIndex]);

                int parsedValue;
                if (char.IsLetter(currentLetter))
                {
                    codeString.Append('\0');

                    if (charIndex + 2 < formulaLength && empiricalFormula.Substring(charIndex + 2, 1) == "_")
                    {
                        // At a custom element, which are notated as "C1_", "C2_", etc.
                        // Give it a value of (char)1) through (char)10)
                        // Also, need to bump up charIndex by 2

                        var customElementNum = empiricalFormula.Substring(charIndex + 1, 1);

                        if (int.TryParse(customElementNum, out parsedValue))
                        {
                            codeString.Append((char)parsedValue);
                        }
                        else
                        {
                            codeString.Append('\u0001');
                        }

                        charIndex += 2;
                    }
                    else
                    {
                        // 65 is the ASCII code for the letter a
                        // Thus, 65-55 = 10
                        int asciiValue = currentLetter;
                        codeString.Append((char)(asciiValue - 55));
                    }
                }
                else if (currentLetter.ToString() != "_")
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

                        endIndex++;
                    }

                    if (int.TryParse(empiricalFormula.Substring(charIndex, endIndex - charIndex + 1), out parsedValue))
                    {
                        if (parsedValue < 221)
                        {
                            codeString.Append('\0');
                            codeString.Append((char)(parsedValue + 35));
                        }
                        else
                        {
                            codeString.Append((char)(int)Math.Round(Math.Floor(parsedValue + 34d / 255d)));
                            codeString.Append((char)((parsedValue + 34) % 255 + 1));
                        }
                    }

                    charIndex = endIndex;
                }

                charIndex++;
            }

            return codeString.ToString();
        }

        /// <summary>
        /// Compare m/z to target
        /// </summary>
        /// <param name="totalMass"></param>
        /// <param name="totalCharge"></param>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <param name="multipleMtoZCharge"></param>
        /// <remarks>True if the m/z is within tolerance of the target</remarks>
        private bool CheckMtoZWithTarget(
            double totalMass,
            double totalCharge,
            double targetMass,
            double massToleranceDa,
            int multipleMtoZCharge)
        {
            double mToZ;

            // The original target is the target m/z; assure this compound has that m/z
            if (Math.Abs(totalCharge) > 0.5d)
            {
                mToZ = Math.Abs(totalMass / totalCharge);
            }
            else
            {
                mToZ = 0d;
            }

            if (multipleMtoZCharge == 0)
            {
                return false;
            }

            var originalMtoZ = targetMass / multipleMtoZCharge;

            // mToZ is not within tolerance of originalMtoZ, so don't report the result
            return !(mToZ < originalMtoZ - massToleranceDa || mToZ > originalMtoZ + massToleranceDa);
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
            return MathUtils.Factorial(a) / (MathUtils.Factorial(b) * MathUtils.Factorial(a - b));
        }

        /// <summary>
        /// Construct the empirical formula and verify hydrogens
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="empiricalFormula"></param>
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
        /// <param name="sortedElementStats"></param>
        /// <param name="totalMass"></param>
        /// <param name="targetMass">Only used when searchOptions.FindTargetMZ is true, and that is only valid when matching a target mass, not when matching percent composition values</param>
        /// <param name="massToleranceDa">Only used when searchOptions.FindTargetMZ is true</param>
        /// <param name="totalCharge"></param>
        /// <param name="multipleMtoZCharge">When searchOptions.FindTargetMZ is false, this will be 1; otherwise, the current charge being searched for</param>
        /// <param name="empiricalResultSymbols"></param>
        /// <param name="correctedCharge"></param>
        /// <returns>False if compound has too many hydrogens AND hydrogen checking is on, otherwise returns true</returns>
        /// <remarks>Common function to both molecular weight and percent composition matching</remarks>
        private bool ConstructAndVerifyCompound(
            SearchOptions searchOptions,
            StringBuilder empiricalFormula,
            int count1, int count2, int count3, int count4, int count5, int count6, int count7, int count8, int count9, int count10,
            IList<CandidateElement> sortedElementStats,
            double totalMass,
            double targetMass,
            double massToleranceDa,
            double totalCharge,
            int multipleMtoZCharge,
            out Dictionary<string, int> empiricalResultSymbols,
            out double correctedCharge)
        {
            // This dictionary tracks the elements and abbreviations of the found formula so that they can be properly ordered according to empirical formula conventions
            // Key is the element or abbreviation symbol, value is the number of each element or abbreviation
            empiricalResultSymbols = new Dictionary<string, int>();

            empiricalFormula.Clear();

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
                                                           empiricalFormula,
                                                           totalMass, targetMass, massToleranceDa,
                                                           totalCharge, multipleMtoZCharge,
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
            IList<CandidateElement> sortedElementStats,
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
        /// <param name="empiricalFormula"></param>
        /// <param name="sortedElementStats"></param>
        /// <param name="potentialElementPointers"></param>
        /// <param name="totalMass"></param>
        /// <param name="targetMass">Only used when searchOptions.FindTargetMZ is true, and that is only valid when matching a target mass, not when matching percent composition values</param>
        /// <param name="massToleranceDa">Only used when searchOptions.FindTargetMZ is true</param>
        /// <param name="totalCharge"></param>
        /// <param name="multipleMtoZCharge">When searchOptions.FindTargetMZ is false, this will be 0; otherwise, the current charge being searched for</param>
        /// <param name="empiricalResultSymbols"></param>
        /// <param name="correctedCharge"></param>
        /// <returns>False if compound has too many hydrogens AND hydrogen checking is on, otherwise returns true</returns>
        /// <remarks>Common function to both molecular weight and percent composition matching</remarks>
        private bool ConstructAndVerifyCompoundRecursive(
            SearchOptions searchOptions,
            StringBuilder empiricalFormula,
            IList<CandidateElement> sortedElementStats,
            IEnumerable<int> potentialElementPointers,
            double totalMass,
            double targetMass,
            double massToleranceDa,
            double totalCharge,
            int multipleMtoZCharge,
            out Dictionary<string, int> empiricalResultSymbols,
            out double correctedCharge)
        {
            empiricalFormula.Clear();

            try
            {
                // The empiricalResultSymbols dictionary tracks the elements and abbreviations of the found formula
                // so that they can be properly ordered according to empirical formula conventions
                // Keys are the element or abbreviation symbol, value is the number of each element or abbreviation
                empiricalResultSymbols = ConvertElementPointersToElementStats(sortedElementStats, potentialElementPointers);

                var valid = ConstructAndVerifyCompoundWork(searchOptions,
                                                           empiricalFormula,
                                                           totalMass, targetMass, massToleranceDa,
                                                           totalCharge, multipleMtoZCharge,
                                                           empiricalResultSymbols, out correctedCharge);

                // Uncomment to debug
                //var computedMass = mElementAndMassRoutines.ComputeFormulaWeight(empiricalFormula.ToString());
                //if (Math.Abs(computedMass - totalMass) > massToleranceDa)
                //    Console.WriteLine("Wrong result: " + empiricalFormula);

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
            IEnumerable<int> newPotentialElementPointers)
        {
            // Store the occurrence count of each element
            var elementCountArray = new int[potentialElementCount];

            foreach (var elementIndex in newPotentialElementPointers)
                elementCountArray[elementIndex]++;

            return elementCountArray;
        }

        private bool ConstructAndVerifyCompoundWork(
            SearchOptions searchOptions,
            StringBuilder empiricalFormula,
            double totalMass,
            double targetMass,
            double massToleranceDa,
            double totalCharge,
            int multipleMtoZCharge,
            Dictionary<string, int> empiricalResultSymbols,
            out double correctedCharge)
        {
            correctedCharge = totalCharge;

            // Convert to a formatted empirical formula (elements order by C, H, then alphabetical)

            // First find C
            if (empiricalResultSymbols.TryGetValue("C", out var matchCount))
            {
                empiricalFormula.Append("C");
                if (matchCount > 1)
                    empiricalFormula.Append(matchCount);
            }

            // Next find H
            if (empiricalResultSymbols.TryGetValue("H", out matchCount))
            {
                empiricalFormula.Append("H");
                if (matchCount > 1)
                    empiricalFormula.Append(matchCount);
            }

            var query = from item in empiricalResultSymbols where item.Key != "C" && item.Key != "H" orderby item.Key select item;

            foreach (var result in query)
            {
                empiricalFormula.Append(result.Key);
                if (result.Value > 1)
                    empiricalFormula.Append(result.Value);
            }

            if (!searchOptions.VerifyHydrogens && !searchOptions.FindTargetMz)
            {
                return true;
            }

            // Verify that the formula does not have too many hydrogens

            // Counters for elements of interest (hydrogen, carbon, silicon, nitrogen, phosphorus, chlorine, iodine, fluorine, bromine, and other)
            var elementNum = new ElementNum();

            // Determine number of C, Si, N, P, O, S, Cl, I, F, Br and H atoms
            foreach (var item in empiricalResultSymbols)
            {
                switch (item.Key ?? string.Empty)
                {
                    case "C":
                        elementNum.C += item.Value;
                        break;
                    case "Si":
                        elementNum.Si += item.Value;
                        break;
                    case "N":
                        elementNum.N += item.Value;
                        break;
                    case "P":
                        elementNum.P += item.Value;
                        break;
                    case "O":
                        elementNum.O += item.Value;
                        break;
                    case "S":
                        elementNum.S += item.Value;
                        break;
                    case "Cl":
                        elementNum.Cl += item.Value;
                        break;
                    case "I":
                        elementNum.I += item.Value;
                        break;
                    case "F":
                        elementNum.F += item.Value;
                        break;
                    case "Br":
                        elementNum.Br += item.Value;
                        break;
                    case "H":
                        elementNum.H += item.Value;
                        break;
                    default:
                        elementNum.Other += item.Value;
                        break;
                }
            }

            var maxH = 0;

            // Compute maximum number of hydrogens
            if (elementNum.Si == 0 && elementNum.C == 0 && elementNum.N == 0 &&
                elementNum.P == 0 && elementNum.Other == 0 &&
                (elementNum.O > 0 || elementNum.S > 0))
            {
                // Only O and S
                maxH = 3;
            }
            else
            {
                // Formula is: [#C*2 + 3 - (2 if N or P present)] + [#N + 3 - (1 if C or Si present)] + [#other elements * 4 + 3], where we assume other elements can have a coordination Number of up to 7
                if (elementNum.C > 0 || elementNum.Si > 0)
                {
                    maxH += (elementNum.C + elementNum.Si) * 2 + 3;
                    //if (elementNum.N > 0 || elementNum.P > 0) maxH -= 2;
                }

                if (elementNum.N > 0 || elementNum.P > 0)
                {
                    maxH += elementNum.N + elementNum.P + 3;
                    //if (elementNum.C > 0 || elementNum.Si > 0) maxH -= 1;
                }

                // Correction for carbon contribution
                //if ((elementNum.C > 0 || elementNum.Si > 0) && (elementNum.N > 0 || elementNum.P > 0)) elementNum.H -= 2;

                // Correction for nitrogen contribution
                //if ((elementNum.N > 0 || elementNum.P > 0) && (elementNum.C > 0 || elementNum.Si > 0)) elementNum.H -= 1;

                // Combine the above two commented out if's to obtain:
                if ((elementNum.N > 0 || elementNum.P > 0) && (elementNum.C > 0 || elementNum.Si > 0))
                {
                    maxH -= 3;
                }

                if (elementNum.Other > 0)
                    maxH += elementNum.Other * 4 + 3;
            }

            // correct for if H only
            if (maxH < 3)
                maxH = 3;

            // correct for halogens
            maxH = maxH - elementNum.F - elementNum.Cl - elementNum.Br - elementNum.I;

            // correct for negative elementNum.H
            if (maxH < 0)
                maxH = 0;

            // Verify H's
            var hOkay = elementNum.H <= maxH;

            // Only proceed if hydrogens check out
            if (!hOkay)
            {
                return false;
            }

            bool chargeOkay;

            // See if totalCharge is within charge limits (chargeOkay will be set to True or False by CorrectChargeEmpirical)
            if (searchOptions.FindCharge)
            {
                correctedCharge = CorrectChargeEmpirical(searchOptions, totalCharge, elementNum, out chargeOkay);
            }
            else
            {
                chargeOkay = true;
            }

            // If charge is within range and checking for multiples, see if correct m/z too
            if (chargeOkay && searchOptions.FindTargetMz)
            {
                chargeOkay = CheckMtoZWithTarget(totalMass, correctedCharge, targetMass,
                                               massToleranceDa, multipleMtoZCharge);
            }

            return chargeOkay;
        }

        private Dictionary<string, int> ConvertElementPointersToElementStats(
            IList<CandidateElement> sortedElementStats,
            IEnumerable<int> potentialElementPointers)
        {
            // This dictionary tracks the elements and abbreviations of the found formula so that they can be properly ordered according to empirical formula conventions
            // Key is the element or abbreviation symbol, value is the number of each element or abbreviation
            var empiricalResultSymbols = new Dictionary<string, int>();

            var elementCountArray = GetElementCountArray(sortedElementStats.Count, potentialElementPointers);

            for (var index = 0; index < sortedElementStats.Count; index++)
            {
                if (elementCountArray[index] != 0)
                {
                    empiricalResultSymbols.Add(sortedElementStats[index].Symbol, elementCountArray[index]);
                }
            }

            return empiricalResultSymbols;
        }

        /// <summary>
        /// Correct charge using rules for an empirical formula
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="totalCharge"></param>
        /// <param name="elementCounts"></param>
        /// <param name="chargeOk"></param>
        /// <returns>Corrected charge</returns>
        private double CorrectChargeEmpirical(
            SearchOptions searchOptions,
            double totalCharge,
            ElementNum elementCounts,
            out bool chargeOk)
        {
            var correctedCharge = totalCharge;

            if (elementCounts.C + elementCounts.Si >= 1)
            {
                if (elementCounts.H > 0 && Math.Abs(mElementAndMassRoutines.Elements.GetElementStat(1, ElementStatsType.Charge) - 1d) < float.Epsilon)
                {
                    // Since carbon or silicon are present, assume the hydrogens should be negative
                    // Subtract elementNum.H * 2 since hydrogen is assigned a +1 charge if ElementStats[1].Charge = 1
                    correctedCharge -= elementCounts.H * 2;
                }

                // Correct for number of C and Si atoms
                if (elementCounts.C + elementCounts.Si > 1)
                {
                    correctedCharge -= (elementCounts.C + elementCounts.Si - 1) * 2;
                }
            }

            if (elementCounts.N + elementCounts.P > 0 && elementCounts.C > 0)
            {
                // Assume 2 hydrogens around each Nitrogen or Phosphorus, thus add back +2 for each H
                // First, decrease number of halogens by the number of hydrogens & halogens taken up by the carbons
                // Determine # of H taken up by all the carbons in a compound without N or P, then add back 1 H for each N and P
                var numHalogens = elementCounts.H + elementCounts.F + elementCounts.Cl + elementCounts.Br + elementCounts.I;
                numHalogens = numHalogens - (elementCounts.C * 2 + 2) + elementCounts.N + elementCounts.P;

                if (numHalogens >= 0)
                {
                    for (var index = 1; index <= elementCounts.N + elementCounts.P; index++)
                    {
                        correctedCharge += 2d;
                        numHalogens--;

                        if (numHalogens <= 0)
                        {
                            break;
                        }

                        correctedCharge += 2d;
                        numHalogens--;
                        if (numHalogens <= 0)
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
                    chargeOk = true;
                }
                else
                {
                    chargeOk = false;
                }
            }
            else
            {
                chargeOk = true;
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

            const int numPointers = 3;

            // Calculate maxRecursiveCount based on a combination function
            var maxRecursiveCount = Combinatorial(numPointers + potentialElementCount, potentialElementCount - 1) - Combinatorial(potentialElementCount + numPointers - 2, numPointers - 1);
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
                // Correct maxRecursiveCount for searching for m/z values
                mMaxRecursiveCount *= multipleSearchMax;
            }
        }

        private List<SearchResult> FindMatchesByMass(
            double targetMass,
            double massToleranceDa,
            SearchOptions searchOptions,
            bool ppmMode)
        {
            // Validate the Inputs
            if (!ValidateSettings(CalculationMode.MatchMolecularWeight))
            {
                return new List<SearchResult>();
            }

            if (targetMass <= 0d)
            {
                ReportError("Target molecular weight must be greater than 0");
                return new List<SearchResult>();
            }

            if (massToleranceDa < 0d)
            {
                ReportError("Mass tolerance cannot be negative");
                return new List<SearchResult>();
            }

            var candidateElementsStats = GetCandidateElements();

            if (candidateElementsStats.Count == 0)
            {
                return new List<SearchResult>();
            }

            var sortedElementStats = (from item in candidateElementsStats orderby item.Mass descending select item).ToList();

            if (searchOptions.SearchMode == FormulaSearchModes.Thorough)
            {
                // Thorough search

                EstimateNumberOfOperations(sortedElementStats.Count);

                var results = new List<SearchResult>();

                if (searchOptions.FindTargetMz)
                {
                    // Searching for target m/z rather than target mass

                    MultipleSearchMath(sortedElementStats.Count, searchOptions, out var mzSearchChargeMin, out var mzSearchChargeMax);

                    for (var currentMzCharge = mzSearchChargeMin; currentMzCharge <= mzSearchChargeMax; currentMzCharge++)
                    {
                        // Call the RecursiveMWFinder repeatedly, sending targetWeight * x each time to search for target, target*2, target*3, etc.
                        RecursiveMWFinder(results, searchOptions, ppmMode, sortedElementStats, 
                            0, 0d, targetMass * currentMzCharge, massToleranceDa,
                            0d, currentMzCharge);
                    }
                }
                else
                {
                    //RecursiveMWFinder(results, searchOptions, ppmMode, potentialElements, potentialElementStats, 0, potentialElementCount, potentialElementPointers, 0, targetMass, massToleranceDa, 0, 0)
                    RecursiveMWFinder(results, searchOptions, ppmMode, sortedElementStats, 
                        0, 0d, targetMass, massToleranceDa, 
                        0d, 0);
                }

                ComputeSortKeys(results);

                return results;
            }

            // Bounded search
            const int maximumFormulaMass = 0;

            var boundedSearchResults = BoundedSearch(targetMass, massToleranceDa, maximumFormulaMass,
                searchOptions, ppmMode, CalculationMode.MatchMolecularWeight,
                sortedElementStats);

            ComputeSortKeys(boundedSearchResults);

            return boundedSearchResults;
        }

        private List<SearchResult> FindMatchesByPercentCompositionWork(
            double maximumFormulaMass,
            double percentTolerance,
            SearchOptions searchOptions)
        {
            // Validate the Inputs
            if (!ValidateSettings(CalculationMode.MatchPercentComposition))
            {
                return new List<SearchResult>();
            }

            if (maximumFormulaMass <= 0d)
            {
                ReportError("Maximum molecular weight must be greater than 0");
                return new List<SearchResult>();
            }

            if (percentTolerance < 0d)
            {
                ReportError("Percent tolerance cannot be negative");
                return new List<SearchResult>();
            }

            var candidateElementsStats = GetCandidateElements(percentTolerance);

            if (candidateElementsStats.Count == 0)
            {
                return new List<SearchResult>();
            }

            var sortedElementStats = (from item in candidateElementsStats orderby item.Mass descending select item).ToList();

            if (searchOptions.SearchMode == FormulaSearchModes.Thorough)
            {
                // Thorough search

                EstimateNumberOfOperations(sortedElementStats.Count);

                // Pointers to the potential elements
                var potentialElementPointers = new List<int>();

                var results = new List<SearchResult>();

                RecursivePCompFinder(results, searchOptions, sortedElementStats, 0, potentialElementPointers, 0d, maximumFormulaMass, 9d);

                ComputeSortKeys(results);

                return results;
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

        private List<CandidateElement> GetCandidateElements(double percentTolerance = 0d)
        {
            var candidateElementsStats = new List<CandidateElement>();

            var customElementCounter = 0;

            foreach (var item in mCandidateElements)
            {
                var candidateElement = new CandidateElement(item.Key)
                {
                    CountMinimum = item.Value.MinimumCount,
                    CountMaximum = item.Value.MaximumCount
                };

                float charge;
                double mass;
                if (mElementAndMassRoutines.Elements.IsValidElementSymbol(item.Key))
                {
                    var atomicNumber = mElementAndMassRoutines.Elements.GetAtomicNumber(item.Key);

                    mElementAndMassRoutines.Elements.GetElement(atomicNumber, out _, out mass, out _, out charge, out _);

                    candidateElement.Mass = mass;
                    candidateElement.Charge = charge;
                }
                else
                {
                    // Working with an abbreviation or simply a mass

                    if (double.TryParse(item.Key, out var customMass))
                    {
                        // Custom element, only weight given so charge is 0
                        candidateElement.Mass = customMass;
                        candidateElement.Charge = 0d;

                        customElementCounter++;

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
                                return new List<CandidateElement>();
                            }
                        }

                        if (string.IsNullOrWhiteSpace(abbrevSymbol))
                        {
                            // Too short
                            ReportError("Custom elemental weight is empty; if letters are used, they must be for a single valid elemental symbol or abbreviation");
                            return new List<CandidateElement>();
                        }

                        // See if this is an abbreviation
                        var symbolReference = mElementAndMassRoutines.Elements.GetAbbreviationId(abbrevSymbol);
                        if (symbolReference < 0)
                        {
                            ReportError("Unknown element or abbreviation for custom elemental weight: " + abbrevSymbol);
                            return new List<CandidateElement>();
                        }

                        // Found a normal abbreviation
                        mElementAndMassRoutines.Elements.GetAbbreviation(symbolReference, out _, out var abbrevFormula, out charge, out _);

                        mass = mElementAndMassRoutines.Parser.ComputeFormulaWeight(abbrevFormula);

                        candidateElement.Mass = mass;

                        candidateElement.Charge = charge;
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
            int[,] range,
            double[,] potentialElementStats,
            IList<string> potentialElements,
            double[,] targetPercents)
        {
            var potentialElementCount = 0;
            var customElementCounter = 0;

            foreach (var item in mCandidateElements)
            {
                range[potentialElementCount, 0] = item.Value.MinimumCount;
                range[potentialElementCount, 1] = item.Value.MaximumCount;

                float charge;
                double mass;
                if (mElementAndMassRoutines.Elements.IsValidElementSymbol(item.Key))
                {
                    var atomicNumber = mElementAndMassRoutines.Elements.GetAtomicNumber(item.Key);

                    mElementAndMassRoutines.Elements.GetElement(atomicNumber, out _, out mass, out _, out charge, out _);

                    potentialElementStats[potentialElementCount, 0] = mass;
                    potentialElementStats[potentialElementCount, 1] = charge;

                    potentialElements[potentialElementCount] = item.Key;
                }
                else
                {
                    // Working with an abbreviation or simply a mass

                    if (double.TryParse(item.Key, out var customMass))
                    {
                        // Custom element, only weight given so charge is 0
                        potentialElementStats[potentialElementCount, 0] = customMass;
                        potentialElementStats[potentialElementCount, 1] = 0d;

                        customElementCounter++;

                        // Custom elements are named C1_ or C2_ or C3_ etc.
                        potentialElements[potentialElementCount] = "C" + customElementCounter + "_";
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

                        const int defaultCharge = 0;

                        // See if this is an abbreviation
                        var symbolReference = mElementAndMassRoutines.Elements.GetAbbreviationId(abbrevSymbol);
                        if (symbolReference < 0)
                        {
                            ReportError("Unknown element or abbreviation for custom elemental weight: " + abbrevSymbol);
                            return 0;
                        }

                        // Found a normal abbreviation
                        mElementAndMassRoutines.Elements.GetAbbreviation(symbolReference, out var matchedAbbrevSymbol, out var abbrevFormula, out charge, out _);

                        mass = mElementAndMassRoutines.Parser.ComputeFormulaWeight(abbrevFormula);

                        // Returns weight of element/abbreviation, but also charge
                        potentialElementStats[potentialElementCount, 0] = mass;

                        potentialElementStats[potentialElementCount, 1] = defaultCharge;

                        // No problems, store symbol
                        potentialElements[potentialElementCount] = matchedAbbrevSymbol;
                    }
                }

                targetPercents[potentialElementCount, 0] = item.Value.TargetPercentComposition - percentTolerance;  // Lower bound of target percentage
                targetPercents[potentialElementCount, 1] = item.Value.TargetPercentComposition + percentTolerance;  // Upper bound of target percentage

                potentialElementCount++;
            }

            return potentialElementCount;
        }

        private CandidateElementTolerances GetDefaultCandidateElementTolerance(int minimumCount, int maximumCount)
        {
            var elementTolerances = new CandidateElementTolerances
            {
                MinimumCount = minimumCount,    // Only used with the Bounded search mode
                MaximumCount = maximumCount,    // Only used with the Bounded search mode
                TargetPercentComposition = 0d   // Only used when searching for percent compositions
            };

            return elementTolerances;
        }

        private CandidateElementTolerances GetDefaultCandidateElementTolerance(double targetPercentComposition = 0)
        {
            var elementTolerances = new CandidateElementTolerances
            {
                MinimumCount = 0,               // Only used with the Bounded search mode
                MaximumCount = 10,              // Only used with the Bounded search mode
                TargetPercentComposition = targetPercentComposition   // Only used when searching for percent compositions
            };

            return elementTolerances;
        }

        /// <summary>
        /// Initializes a new search result
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="ppmMode"></param>
        /// <param name="empiricalFormula"></param>
        /// <param name="totalMass">If 0 or negative, means matching percent compositions</param>
        /// <param name="targetMass"></param>
        /// <param name="totalCharge"></param>
        /// <param name="empiricalResultSymbols"></param>
        private SearchResult GetSearchResult(
            SearchOptions searchOptions,
            bool ppmMode,
            StringBuilder empiricalFormula,
            double totalMass,
            double targetMass,
            double totalCharge,
            Dictionary<string, int> empiricalResultSymbols)
        {
            try
            {
                var searchResult = new SearchResult(empiricalFormula.ToString(), empiricalResultSymbols);

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
                    searchResult.Mz = Math.Abs(totalMass / totalCharge);
                }

                return searchResult;
            }
            catch (Exception ex)
            {
                mElementAndMassRoutines.GeneralErrorHandler("GetSearchResult", ex);
                return new SearchResult(string.Empty, new Dictionary<string, int>());
            }
        }

        private double GetTotalPercentComposition()
        {
            var totalTargetPercentComp = mCandidateElements.Sum(item => item.Value.TargetPercentComposition);
            return totalTargetPercentComp;
        }

        /// <summary>
        /// Estimate the number of operations required to search for a target m/z rather than a target mass
        /// </summary>
        /// <param name="potentialElementCount"></param>
        /// <param name="searchOptions"></param>
        /// <param name="mzSearchChargeMin"></param>
        /// <param name="mzSearchChargeMax"></param>
        /// <remarks>searchOptions is passed ByRef because it is a value type and .MzChargeMin and .MzChargeMax are updated</remarks>
        private void MultipleSearchMath(
            int potentialElementCount,
            SearchOptions searchOptions,
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
        /// <param name="sortedElementStats"></param>
        /// <param name="targetMass">Only used when calculationMode is MatchMolecularWeight</param>
        /// <param name="massToleranceDa">Only used when calculationMode is MatchMolecularWeigh</param>
        /// <param name="maximumFormulaMass">Only used when calculationMode is MatchPercentComposition</param>
        private List<SearchResult> OldFormulaFinder(
            SearchOptions searchOptions,
            bool ppmMode,
            CalculationMode calculationMode,
            IList<CandidateElement> sortedElementStats,
            double targetMass,
            double massToleranceDa,
            double maximumFormulaMass)
        {
            // The calculated percentages for the specific compound
            var percent = new double[11];
            var results = new List<SearchResult>();

            try
            {
                // Only used when calculationMode is MatchMolecularWeight
                var multipleSearchMaxWeight = targetMass * searchOptions.ChargeMax;

                var empiricalFormula = new StringBuilder();

                var ranges = new List<BoundedSearchRange>(MAX_MATCHING_ELEMENTS);

                foreach (var element in sortedElementStats)
                {
                    var boundedSearchRange = new BoundedSearchRange
                    {
                        Min = element.CountMinimum,
                        Max = element.CountMaximum
                    };
                    ranges.Add(boundedSearchRange);
                }

                while (ranges.Count < MAX_MATCHING_ELEMENTS)
                {
                    var boundedSearchRange = new BoundedSearchRange
                    {
                        Min = 0,
                        Max = 0
                    };
                    ranges.Add(boundedSearchRange);
                }

                var potentialElementCount = sortedElementStats.Count;

                // Determine the valid compounds
                for (var j = ranges[0].Min; j <= ranges[0].Max; j++)
                {
                    for (var k = ranges[1].Min; k <= ranges[1].Max; k++)
                    {
                        for (var l = ranges[2].Min; l <= ranges[2].Max; l++)
                        {
                            for (var m = ranges[3].Min; m <= ranges[3].Max; m++)
                            {
                                for (var N = ranges[4].Min; N <= ranges[4].Max; N++)
                                {
                                    for (var O = ranges[5].Min; O <= ranges[5].Max; O++)
                                    {
                                        for (var P = ranges[6].Min; P <= ranges[6].Max; P++)
                                        {
                                            for (var q = ranges[7].Min; q <= ranges[7].Max; q++)
                                            {
                                                for (var r = ranges[8].Min; r <= ranges[8].Max; r++)
                                                {
                                                    for (var s = ranges[9].Min; s <= ranges[9].Max; s++)
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
                                                                percent[0] = j * sortedElementStats[0].Mass / totalMass * 100d;
                                                                if (potentialElementCount > 1)
                                                                {
                                                                    percent[1] = k * sortedElementStats[1].Mass / totalMass * 100d;

                                                                    if (potentialElementCount > 1)
                                                                    {
                                                                        percent[2] = l * sortedElementStats[2].Mass / totalMass * 100d;

                                                                        if (potentialElementCount > 1)
                                                                        {
                                                                            percent[3] = m * sortedElementStats[3].Mass / totalMass * 100d;

                                                                            if (potentialElementCount > 1)
                                                                            {
                                                                                percent[4] = N * sortedElementStats[4].Mass / totalMass * 100d;

                                                                                if (potentialElementCount > 1)
                                                                                {
                                                                                    percent[5] = O * sortedElementStats[5].Mass / totalMass * 100d;

                                                                                    if (potentialElementCount > 1)
                                                                                    {
                                                                                        percent[6] = P * sortedElementStats[6].Mass / totalMass * 100d;

                                                                                        if (potentialElementCount > 1)
                                                                                        {
                                                                                            percent[7] = q * sortedElementStats[7].Mass / totalMass * 100d;

                                                                                            if (potentialElementCount > 1)
                                                                                            {
                                                                                                percent[8] = r * sortedElementStats[8].Mass / totalMass * 100d;

                                                                                                if (potentialElementCount > 1)
                                                                                                {
                                                                                                    percent[9] = s * sortedElementStats[9].Mass / totalMass * 100d;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                var subTrack = 0;
                                                                for (var index = 0; index < potentialElementCount; index++)
                                                                {
                                                                    if (percent[index] >= sortedElementStats[index].PercentCompMinimum && percent[index] <= sortedElementStats[index].PercentCompMaximum)
                                                                    {
                                                                        subTrack++;
                                                                    }
                                                                }

                                                                if (subTrack == potentialElementCount)
                                                                {
                                                                    // All of the elements have percent compositions matching the target

                                                                    // Construct the empirical formula and verify hydrogens
                                                                    var hOkay = ConstructAndVerifyCompound(searchOptions,
                                                                                                            empiricalFormula,
                                                                                                            j, k, l, m, N, O, P, q, r, s,
                                                                                                            sortedElementStats,
                                                                                                            totalMass, targetMass, massToleranceDa,
                                                                                                            totalCharge, 0,
                                                                                                            out var empiricalResultSymbols,
                                                                                                            out var correctedCharge);

                                                                    if (empiricalFormula.Length > 0 && hOkay)
                                                                    {
                                                                        var searchResult = GetSearchResult(searchOptions, ppmMode, empiricalFormula, totalMass, -1, correctedCharge, empiricalResultSymbols);

                                                                        // Add % composition info

                                                                        AppendPercentCompositionResult(searchResult, j, sortedElementStats, 0, percent[0]);
                                                                        AppendPercentCompositionResult(searchResult, k, sortedElementStats, 1, percent[1]);
                                                                        AppendPercentCompositionResult(searchResult, l, sortedElementStats, 2, percent[2]);
                                                                        AppendPercentCompositionResult(searchResult, m, sortedElementStats, 3, percent[3]);
                                                                        AppendPercentCompositionResult(searchResult, N, sortedElementStats, 4, percent[4]);
                                                                        AppendPercentCompositionResult(searchResult, O, sortedElementStats, 5, percent[5]);
                                                                        AppendPercentCompositionResult(searchResult, P, sortedElementStats, 6, percent[6]);
                                                                        AppendPercentCompositionResult(searchResult, q, sortedElementStats, 7, percent[7]);
                                                                        AppendPercentCompositionResult(searchResult, r, sortedElementStats, 8, percent[8]);
                                                                        AppendPercentCompositionResult(searchResult, s, sortedElementStats, 9, percent[9]);

                                                                        results.Add(searchResult);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        // Matching Molecular Weights

                                                        else if (totalMass <= multipleSearchMaxWeight + massToleranceDa)
                                                        {
                                                            // When searchOptions.FindTargetMZ is false, ChargeMin and ChargeMax will be 1
                                                            for (var currentCharge = searchOptions.ChargeMin; currentCharge <= searchOptions.ChargeMax; currentCharge++)
                                                            {
                                                                var matchWeight = targetMass * currentCharge;
                                                                if (totalMass <= matchWeight + massToleranceDa && totalMass >= matchWeight - massToleranceDa)
                                                                {
                                                                    // Within massToleranceDa

                                                                    // Construct the empirical formula and verify hydrogens
                                                                    var hOkay = ConstructAndVerifyCompound(searchOptions,
                                                                                                            empiricalFormula,
                                                                                                            j, k, l, m, N, O, P, q, r, s,
                                                                                                            sortedElementStats,
                                                                                                            totalMass, targetMass * currentCharge, massToleranceDa,
                                                                                                            totalCharge, currentCharge,
                                                                                                            out var empiricalResultSymbols,
                                                                                                            out var correctedCharge);

                                                                    if (empiricalFormula.Length > 0 && hOkay)
                                                                    {
                                                                        var searchResult = GetSearchResult(searchOptions, ppmMode, empiricalFormula, totalMass, targetMass, correctedCharge, empiricalResultSymbols);

                                                                        results.Add(searchResult);
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
                                                            s = ranges[9].Max;
                                                            if (j * ranges[0].Min + k * ranges[1].Min + l * ranges[2].Min + m * ranges[3].Min + N * ranges[4].Min + O * ranges[5].Min + P * ranges[6].Min + q * ranges[7].Min + (r + 1) * ranges[8].Min > massToleranceDa + multipleSearchMaxWeight)
                                                            {
                                                                // Incrementing r would make the weight too high, so set it to its max (so it will zero and increment q)
                                                                r = ranges[8].Max;
                                                                if (j * ranges[0].Min + k * ranges[1].Min + l * ranges[2].Min + m * ranges[3].Min + N * ranges[4].Min + O * ranges[5].Min + P * ranges[6].Min + (q + 1) * ranges[7].Min > massToleranceDa + multipleSearchMaxWeight)
                                                                {
                                                                    q = ranges[7].Max;
                                                                    if (j * ranges[0].Min + k * ranges[1].Min + l * ranges[2].Min + m * ranges[3].Min + N * ranges[4].Min + O * ranges[5].Min + (P + 1) * ranges[6].Min > massToleranceDa + multipleSearchMaxWeight)
                                                                    {
                                                                        P = ranges[6].Max;
                                                                        if (j * ranges[0].Min + k * ranges[1].Min + l * ranges[2].Min + m * ranges[3].Min + N * ranges[4].Min + (O + 1) * ranges[5].Min > massToleranceDa + multipleSearchMaxWeight)
                                                                        {
                                                                            O = ranges[5].Max;
                                                                            if (j * ranges[0].Min + k * ranges[1].Min + l * ranges[2].Min + m * ranges[3].Min + (N + 1) * ranges[4].Min > massToleranceDa + multipleSearchMaxWeight)
                                                                            {
                                                                                N = ranges[4].Max;
                                                                                if (j * ranges[0].Min + k * ranges[1].Min + l * ranges[2].Min + (m + 1) * ranges[3].Min > massToleranceDa + multipleSearchMaxWeight)
                                                                                {
                                                                                    m = ranges[3].Max;
                                                                                    if (j * ranges[0].Min + k * ranges[1].Min + (l + 1) * ranges[2].Min > massToleranceDa + multipleSearchMaxWeight)
                                                                                    {
                                                                                        l = ranges[2].Max;
                                                                                        if (j * ranges[0].Min + (k + 1) * ranges[1].Min > massToleranceDa + multipleSearchMaxWeight)
                                                                                        {
                                                                                            k = ranges[1].Max;
                                                                                            if ((j + 1) * ranges[0].Min > massToleranceDa + multipleSearchMaxWeight)
                                                                                            {
                                                                                                j = ranges[0].Max;
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
                                                            return results;
                                                        }

                                                        if (results.Count >= mMaximumHits)
                                                        {
                                                            // Set variables to their maximum so all the loops will end
                                                            j = ranges[0].Max;
                                                            k = ranges[1].Max;
                                                            l = ranges[2].Max;
                                                            m = ranges[3].Max;
                                                            N = ranges[4].Max;
                                                            O = ranges[5].Max;
                                                            P = ranges[6].Max;
                                                            q = ranges[7].Max;
                                                            r = ranges[8].Max;
                                                            s = ranges[9].Max;
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

                    if (ranges[0].Max != 0)
                    {
                        if (searchOptions.ChargeMin == 0)
                        {
                            PercentComplete = j / (double)ranges[0].Max * 100d;
                        }
                        else
                        {
                            PercentComplete = j / (double)ranges[0].Max * 100d * searchOptions.ChargeMax;
                        }

                        Console.WriteLine("Bounded search: " + PercentComplete.ToString("0") + "% complete");
                    }
                }
            }
            catch (Exception ex)
            {
                mElementAndMassRoutines.GeneralErrorHandler("OldFormulaFinder", ex);
            }

            return results;
        }

        [Obsolete("Deprecated")]
        private void SortCandidateElements(
            CalculationMode calculationMode,
            int potentialElementCount,
            double[,] potentialElementStats,
            IList<string> potentialElements,
            double[,] targetPercents)
        {
            // Reorder potentialElementStats pointer array in order from heaviest to lightest element
            // Greatly speeds up the recursive routine

            // Bubble sort
            for (var y = potentialElementCount - 1; y >= 1; --y)       // Sort from end to start
            {
                for (var x = 0; x < y; x++)
                {
                    if (potentialElementStats[x, 0] < potentialElementStats[x + 1, 0])
                    {
                        // Swap the element symbols
                        var swap = potentialElements[x];
                        potentialElements[x] = potentialElements[x + 1];
                        potentialElements[x + 1] = swap;

                        // and their weights
                        var swapVal = potentialElementStats[x, 0];
                        potentialElementStats[x, 0] = potentialElementStats[x + 1, 0];
                        potentialElementStats[x + 1, 0] = swapVal;

                        // and their charge
                        swapVal = potentialElementStats[x, 1];
                        potentialElementStats[x, 1] = potentialElementStats[x + 1, 1];
                        potentialElementStats[x + 1, 1] = swapVal;

                        if (calculationMode == CalculationMode.MatchPercentComposition)
                        {
                            // and the targetPercents array
                            swapVal = targetPercents[x, 0];
                            targetPercents[x, 0] = targetPercents[x + 1, 0];
                            targetPercents[x + 1, 0] = swapVal;

                            swapVal = targetPercents[x, 1];
                            targetPercents[x, 1] = targetPercents[x + 1, 1];
                            targetPercents[x + 1, 1] = swapVal;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recursively search for a target mass
        /// </summary>
        /// <param name="results"></param>
        /// <param name="searchOptions"></param>
        /// <param name="ppmMode"></param>
        /// <param name="sortedElementStats">Candidate elements, including mass and charge. Sorted by descending mass</param>
        /// <param name="startIndex">Index in candidateElementsStats to start at</param>
        /// <param name="potentialMassTotal">Weight of the potential formula</param>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <param name="potentialChargeTotal"></param>
        /// <param name="multipleMtoZCharge">When searchOptions.FindTargetMZ is false, this will be 0; otherwise, the current charge being searched for</param>
        /// <param name="potentialElementPointers">Pointers to the elements that have been added to the potential formula so far</param>
        private void RecursiveMWFinder(
            ICollection<SearchResult> results,
            SearchOptions searchOptions,
            bool ppmMode,
            IList<CandidateElement> sortedElementStats,
            int startIndex,
            double potentialMassTotal,
            double targetMass,
            double massToleranceDa,
            double potentialChargeTotal,
            int multipleMtoZCharge,
            IReadOnlyCollection<int> potentialElementPointers = null)
        {
            if (potentialElementPointers == null)
            {
                potentialElementPointers = new List<int>();
            }

            try
            {
                var newPotentialElementPointers = new List<int>(potentialElementPointers.Count + 1);

                if (mAbortProcessing || results.Count >= mMaximumHits)
                {
                    return;
                }

                var empiricalFormula = new StringBuilder();

                for (var currentIndex = startIndex; currentIndex < sortedElementStats.Count; currentIndex++)
                {
                    var totalMass = potentialMassTotal + sortedElementStats[currentIndex].Mass;
                    var totalCharge = potentialChargeTotal + sortedElementStats[currentIndex].Charge;

                    newPotentialElementPointers.Clear();

                    if (totalMass <= targetMass + massToleranceDa)
                    {
                        // Below or within massTolerance, add current element's pointer to pointer array
                        newPotentialElementPointers.AddRange(potentialElementPointers);

                        // Append the current element's number
                        newPotentialElementPointers.Add(currentIndex);

                        // Update status
                        UpdateStatus();

                        // Uncomment to add a breakpoint when a certain empirical formula is encountered
                        if (potentialElementPointers.Count >= 3)
                        {
                            var empiricalResultSymbols = ConvertElementPointersToElementStats(sortedElementStats, potentialElementPointers);
                            var debugCompound = new Dictionary<string, int>
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

                        if (mAbortProcessing || results.Count >= mMaximumHits)
                        {
                            return;
                        }

                        if (totalMass >= targetMass - massToleranceDa)
                        {
                            // Matching compound

                            // Construct the empirical formula and verify hydrogens
                            var hOkay = ConstructAndVerifyCompoundRecursive(searchOptions,
                                                                             empiricalFormula, sortedElementStats,
                                                                             newPotentialElementPointers,
                                                                             totalMass, targetMass, massToleranceDa,
                                                                             totalCharge, multipleMtoZCharge,
                                                                             out var empiricalResultSymbols,
                                                                             out var correctedCharge);

                            if (empiricalFormula.Length > 0 && hOkay)
                            {
                                var searchResult = GetSearchResult(searchOptions, ppmMode, empiricalFormula, totalMass, targetMass, correctedCharge, empiricalResultSymbols);

                                results.Add(searchResult);
                            }
                        }

                        // Haven't reached targetMass - massTolerance region, so call RecursiveFinder again

                        // But first, if adding the lightest element (i.e. the last in the list),
                        // add a bunch of it until the potential compound's weight is close to the target
                        if (currentIndex == sortedElementStats.Count - 1)
                        {
                            var extra = 0;
                            while (totalMass < targetMass - massToleranceDa - sortedElementStats[currentIndex].Mass)
                            {
                                extra++;
                                totalMass += sortedElementStats[currentIndex].Mass;
                                totalCharge += sortedElementStats[currentIndex].Charge;
                            }

                            if (extra > 0)
                            {
                                for (var pointer = 1; pointer <= extra; pointer++)
                                    newPotentialElementPointers.Add(currentIndex);
                            }
                        }

                        // Now recursively call this sub
                        RecursiveMWFinder(results, searchOptions, ppmMode, sortedElementStats, currentIndex, totalMass, targetMass, massToleranceDa, totalCharge, multipleMtoZCharge, newPotentialElementPointers);
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
        /// <param name="results"></param>
        /// <param name="sortedElementStats"></param>
        /// <param name="startIndex"></param>
        /// <param name="potentialElementPointers">Pointers to the elements that have been added to the potential formula so far</param>
        /// <param name="potentialMassTotal">>Weight of the potential formula</param>
        /// <param name="maximumFormulaMass"></param>
        /// <param name="potentialChargeTotal"></param>
        /// <param name="searchOptions"></param>
        private void RecursivePCompFinder(
            ICollection<SearchResult> results,
            SearchOptions searchOptions,
            IList<CandidateElement> sortedElementStats,
            int startIndex,
            ICollection<int> potentialElementPointers,
            double potentialMassTotal,
            double maximumFormulaMass,
            double potentialChargeTotal)
        {
            try
            {
                var newPotentialElementPointers = new List<int>(potentialElementPointers.Count + 1);

                var potentialPercents = new double[sortedElementStats.Count + 1];

                if (mAbortProcessing || results.Count >= mMaximumHits)
                {
                    return;
                }

                var empiricalFormula = new StringBuilder();
                const bool ppmMode = false;

                for (var currentIndex = startIndex; currentIndex < sortedElementStats.Count; currentIndex++)  // potentialElementCount >= 1, if 1, means just potentialElementStats[0,0], etc.
                {
                    var totalMass = potentialMassTotal + sortedElementStats[currentIndex].Mass;
                    var totalCharge = potentialChargeTotal + sortedElementStats[currentIndex].Charge;

                    newPotentialElementPointers.Clear();

                    if (totalMass <= maximumFormulaMass)
                    {
                        // only proceed if weight is less than max weight

                        newPotentialElementPointers.AddRange(potentialElementPointers);

                        // Append the current element's number
                        newPotentialElementPointers.Add(currentIndex);

                        // Compute the number of each element
                        var elementCountArray = GetElementCountArray(sortedElementStats.Count, newPotentialElementPointers);

                        var nonZeroCount = (from item in elementCountArray where item > 0 select item).Count();

                        // Only proceed if all elements are present
                        if (nonZeroCount == sortedElementStats.Count)
                        {
                            // Compute % comp of each element
                            for (var index = 0; index < sortedElementStats.Count; index++)
                                potentialPercents[index] = elementCountArray[index] * sortedElementStats[index].Mass / totalMass * 100d;

                            //if (pointerCount == 0) potentialPercents[0] = 100;

                            var percentTrack = 0;
                            for (var index = 0; index < sortedElementStats.Count; index++)
                            {
                                if (potentialPercents[index] >= sortedElementStats[index].PercentCompMinimum &&
                                    potentialPercents[index] <= sortedElementStats[index].PercentCompMaximum)
                                {
                                    percentTrack++;
                                }
                            }

                            if (percentTrack == sortedElementStats.Count)
                            {
                                // Matching compound

                                // Construct the empirical formula and verify hydrogens
                                var hOkay = ConstructAndVerifyCompoundRecursive(searchOptions,
                                                                                 empiricalFormula, sortedElementStats,
                                                                                 newPotentialElementPointers,
                                                                                 totalMass, 0d, 0d,
                                                                                 totalCharge, 0,
                                                                                 out var empiricalResultSymbols,
                                                                                 out var correctedCharge);

                                if (empiricalFormula.Length > 0 && hOkay)
                                {
                                    var searchResult = GetSearchResult(searchOptions, ppmMode, empiricalFormula, totalMass, -1, correctedCharge, empiricalResultSymbols);

                                    // Add % composition info
                                    for (var index = 0; index < sortedElementStats.Count; index++)
                                    {
                                        if (elementCountArray[index] != 0)
                                        {
                                            var percentComposition = elementCountArray[index] * sortedElementStats[index].Mass / totalMass * 100d;

                                            AppendPercentCompositionResult(searchResult, elementCountArray[index], sortedElementStats, index, percentComposition);
                                        }
                                    }

                                    results.Add(searchResult);
                                }
                            }
                        }

                        // Update status
                        UpdateStatus();

                        if (mAbortProcessing || results.Count >= mMaximumHits)
                        {
                            return;
                        }

                        // Haven't reached maximumFormulaMass
                        // Now recursively call this sub
                        RecursivePCompFinder(results, searchOptions, sortedElementStats, currentIndex, newPotentialElementPointers, totalMass, maximumFormulaMass, totalCharge);
                    }
                }
            }
            catch (Exception ex)
            {
                mElementAndMassRoutines.GeneralErrorHandler("RecursivePCompFinder", ex);
                mAbortProcessing = true;
            }
        }

        protected void ReportError(string errorMessage)
        {
            if (EchoMessagesToConsole)
                Console.WriteLine(errorMessage);

            ErrorEvent?.Invoke(errorMessage);
        }

        protected void ReportWarning(string warningMessage)
        {
            if (EchoMessagesToConsole)
                Console.WriteLine(warningMessage);

            WarningEvent?.Invoke(warningMessage);
        }

        protected void ShowMessage(string message)
        {
            if (EchoMessagesToConsole)
                Console.WriteLine(message);
            MessageEvent?.Invoke(message);
        }

        private void UpdateStatus()
        {
            mRecursiveCount++;

            if (mRecursiveCount <= mMaxRecursiveCount)
            {
                PercentComplete = mRecursiveCount / (float)mMaxRecursiveCount * 100f;
            }
        }

        private void ValidateBoundedSearchValues()
        {
            foreach (var elementSymbol in mCandidateElements.Keys)
            {
                var elementTolerances = mCandidateElements[elementSymbol];

                if (elementTolerances.MinimumCount < 0 || elementTolerances.MaximumCount > MAX_BOUNDED_SEARCH_COUNT)
                {
                    if (elementTolerances.MinimumCount < 0)
                        elementTolerances.MinimumCount = 0;
                    if (elementTolerances.MaximumCount > MAX_BOUNDED_SEARCH_COUNT)
                        elementTolerances.MaximumCount = MAX_BOUNDED_SEARCH_COUNT;

                    mCandidateElements[elementSymbol] = elementTolerances;
                }
            }
        }

        private void ValidatePercentCompositionValues()
        {
            foreach (var elementSymbol in mCandidateElements.Keys)
            {
                var elementTolerances = mCandidateElements[elementSymbol];

                if (elementTolerances.TargetPercentComposition < 0d || elementTolerances.TargetPercentComposition > 100d)
                {
                    if (elementTolerances.TargetPercentComposition < 0d)
                        elementTolerances.TargetPercentComposition = 0d;
                    if (elementTolerances.TargetPercentComposition > 100d)
                        elementTolerances.TargetPercentComposition = 100d;

                    mCandidateElements[elementSymbol] = elementTolerances;
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
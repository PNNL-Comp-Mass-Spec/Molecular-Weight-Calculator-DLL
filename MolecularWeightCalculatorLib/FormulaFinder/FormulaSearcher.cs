using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;
using MolecularWeightCalculator.Formula;

namespace MolecularWeightCalculator.FormulaFinder
{
    /// <summary>
    /// Formula finder class
    /// </summary>
    [Guid("B24497AD-A29D-4266-A33A-EF97B86EA578"), ClassInterface(ClassInterfaceType.None), ComSourceInterfaces(typeof(IFormulaSearcherEvents)), ComVisible(true)]
    public class FormulaSearcher : IFormulaSearcher, IFormulaSearcherEvents
    {
        // Ignore Spelling: Da, interop, MtoZ

        public const int DEFAULT_RESULTS_TO_FIND = 1000;
        public const int MAXIMUM_ALLOWED_RESULTS_TO_FIND = 1000000;

        public const int MAX_BOUNDED_SEARCH_COUNT = 65535;

        private enum CalculationMode
        {
            MatchMolecularWeight = 0,
            MatchPercentComposition = 1
        }

        private bool mAbortProcessing;

        /// <summary>
        /// Keys are element symbols, abbreviations, or even simply a mass value
        /// Values are target percent composition values, between 0 and 100
        /// </summary>
        /// <remarks>The target percent composition values are only used when FindMatchesByPercentComposition is called</remarks>
        private Dictionary<string, CandidateElementTolerances> candidateElements;

        private readonly ElementAndMassTools elementAndMassRoutines;

        private int maximumHits;

        private int recursiveCount;
        private int maxRecursiveCount;

        /// <summary>
        /// Element symbols to consider when finding empirical formulas
        /// </summary>
        /// <remarks>The values in the dictionary are target percent composition values; only used if you call FindMatchesByPercentComposition</remarks>
        public Dictionary<string, CandidateElementTolerances> CandidateElements
        {
            get => candidateElements;
            set
            {
                if (value != null)
                {
                    candidateElements = value;

                    ValidateBoundedSearchValues();
                    ValidatePercentCompositionValues();
                }
            }
        }

        public bool EchoMessagesToConsole { get; set; }

        public int MaximumHits
        {
            get => maximumHits;
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

                maximumHits = value;
            }
        }

        /// <summary>
        /// Percent complete, between 0 and 100
        /// </summary>
        public double PercentComplete { get; private set; }

        /// <summary>
        /// Progress reporter, reports <see cref="PercentComplete"/> whenever it is updated, if it's not null
        /// </summary>
        public IProgress<double> ProgressReporter { get; set; }

        public event MessageEventEventHandler MessageEvent;
        public event ErrorEventEventHandler ErrorEvent;
        public event WarningEventEventHandler WarningEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        public FormulaSearcher(ElementAndMassTools elementAndMassTools)
        {
            elementAndMassRoutines = elementAndMassTools;
            candidateElements = new Dictionary<string, CandidateElementTolerances>();

            EchoMessagesToConsole = true;

            Reset();
        }

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
        /// <remarks>This method should be used when defining elements for a bounded search</remarks>
        /// <param name="elementSymbolAbbrevOrMass">Element symbol, abbreviation symbol, or monoisotopic mass</param>
        /// <param name="minimumCount">Minimum occurrence count</param>
        /// <param name="maximumCount">Maximum occurrence count</param>
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
            // Add or update the dictionary
            candidateElements[elementSymbolAbbrevOrMass] = elementTolerances;
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

            searchOptions ??= new SearchOptions();

            var candidateElementsStats = ValidateMatchesByMassInputs(targetMass, massToleranceDa, searchOptions);

            if (candidateElementsStats.Count == 0)
            {
                return new List<SearchResult>();
            }

            var results = FindMatchesByMass(targetMass, massToleranceDa, searchOptions, candidateElementsStats, true);
            results.Sort();
            return results;
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
            searchOptions ??= new SearchOptions();

            var candidateElementsStats = ValidateMatchesByMassInputs(targetMass, massToleranceDa, searchOptions);

            if (candidateElementsStats.Count == 0)
            {
                return new List<SearchResult>();
            }

            var results = FindMatchesByMass(targetMass, massToleranceDa, searchOptions, candidateElementsStats, false);

            results.Sort();
            return results;
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

        /// <summary>
        /// Find empirical formulas that match target percent composition values
        /// </summary>
        /// <param name="maximumFormulaMass"></param>
        /// <param name="percentTolerance"></param>
        /// <param name="searchOptions"></param>
        /// <returns>Search results, as a list</returns>
        public List<SearchResult> FindMatchesByPercentComposition(
            double maximumFormulaMass,
            double percentTolerance,
            SearchOptions searchOptions)
        {
            searchOptions ??= new SearchOptions();

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

            var candidateElementsStats = GetCandidateElements(searchOptions.SearchMode, percentTolerance);

            if (candidateElementsStats.Count == 0)
            {
                return new List<SearchResult>();
            }

            var results = FindMatchesByPercentComposition(maximumFormulaMass, searchOptions, candidateElementsStats);

            results.Sort();
            return results;
        }

        /// <summary>
        /// Find empirical formulas that match target percent composition values
        /// </summary>
        /// <param name="maximumFormulaMass"></param>
        /// <param name="percentTolerance"></param>
        /// <param name="searchOptions"></param>
        /// <returns>Search results, as an array</returns>
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
            candidateElements.Clear();
            candidateElements.Add("C", GetDefaultCandidateElementTolerance(70d));
            candidateElements.Add("H", GetDefaultCandidateElementTolerance(10d));
            candidateElements.Add("N", GetDefaultCandidateElementTolerance(10d));
            candidateElements.Add("O", GetDefaultCandidateElementTolerance(10d));

            mAbortProcessing = false;

            MaximumHits = DEFAULT_RESULTS_TO_FIND;
        }

        /// <summary>
        /// Compare m/z to target
        /// </summary>
        /// <remarks>True if the m/z is within tolerance of the target</remarks>
        /// <param name="totalMass"></param>
        /// <param name="totalCharge"></param>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <param name="multipleMtoZCharge"></param>
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

        /// <summary>
        /// Construct the empirical formula and verify hydrogens
        /// </summary>
        /// <remarks>Common method to both molecular weight and percent composition matching</remarks>
        /// <param name="searchOptions"></param>
        /// <param name="sortedElementStats"></param>
        /// <param name="elementCounts"></param>
        /// <param name="totalMass"></param>
        /// <param name="targetMass">Only used when searchOptions.FindTargetMZ is true, and that is only valid when matching a target mass, not when matching percent composition values</param>
        /// <param name="massToleranceDa">Only used when searchOptions.FindTargetMZ is true</param>
        /// <param name="totalCharge"></param>
        /// <param name="multipleMtoZCharge">When searchOptions.FindTargetMZ is false, this will be 0; otherwise, the current charge being searched for</param>
        /// <param name="empiricalResultSymbols"></param>
        /// <param name="correctedCharge"></param>
        /// <returns>False if compound has too many hydrogens AND hydrogen checking is on, otherwise returns true</returns>
        private bool ConstructAndVerifyCompound(
            SearchOptions searchOptions,
            IReadOnlyList<CandidateElement> sortedElementStats,
            IReadOnlyList<int> elementCounts,
            double totalMass,
            double totalCharge,
            out List<ElementCount> empiricalResultSymbols,
            out double correctedCharge,
            double targetMass = 0,
            double massToleranceDa = 0,
            int multipleMtoZCharge = 0)
        {
            try
            {
                // The empiricalResultSymbols dictionary tracks the elements and abbreviations of the found formula
                // so that they can be properly ordered according to empirical formula conventions
                // Keys are the element or abbreviation symbol, value is the number of each element or abbreviation
                empiricalResultSymbols = new List<ElementCount>();
                for (var i = 0; i < sortedElementStats.Count; i++)
                {
                    if (elementCounts[i] > 0)
                    {
                        empiricalResultSymbols.Add(new ElementCount(sortedElementStats[i].Symbol, elementCounts[i]));
                    }
                }

                var valid = ConstructAndVerifyCompoundWork(searchOptions,
                                                           totalMass, targetMass, massToleranceDa,
                                                           totalCharge, multipleMtoZCharge,
                                                           empiricalResultSymbols, out correctedCharge);

                // Uncomment to debug
                //var empiricalFormula = string.Concat(empiricalResultSymbols.Select(x => x.ToString()));
                //var computedMass = mElementAndMassRoutines.ComputeFormulaWeight(empiricalFormula);
                //if (Math.Abs(computedMass - totalMass) > massToleranceDa)
                //    Console.WriteLine("Wrong result: " + empiricalFormula);

                return valid;
            }
            catch (Exception ex)
            {
                elementAndMassRoutines.GeneralErrorHandler("ConstructAndVerifyCompound", ex);
                empiricalResultSymbols = new List<ElementCount>();
                correctedCharge = totalCharge;
                return false;
            }
        }

        private bool ConstructAndVerifyCompoundWork(
            SearchOptions searchOptions,
            double totalMass,
            double targetMass,
            double massToleranceDa,
            double totalCharge,
            int multipleMtoZCharge,
            List<ElementCount> empiricalResultSymbols,
            out double correctedCharge)
        {
            correctedCharge = totalCharge;

            // Convert to a formatted empirical formula (elements order by C, H, then alphabetical)

            // Sorts using the implemented IComparable interface for ElementCount, which sort 'C', then 'H', then alphabetical
            empiricalResultSymbols.Sort();

            if (!searchOptions.VerifyHydrogens && !searchOptions.FindTargetMz)
            {
                return true;
            }

            // Verify that the formula does not have too many hydrogens

            // Counters for elements of interest (hydrogen, carbon, silicon, nitrogen, phosphorus, chlorine, iodine, fluorine, bromine, and other)
            // Determine number of C, Si, N, P, O, S, Cl, I, F, Br and H atoms
            var validator = new FormulaValidator(empiricalResultSymbols);

            // Only proceed if hydrogens check out
            if (!validator.HydrogensValid())
            {
                return false;
            }

            bool chargeOkay;

            // See if totalCharge is within charge limits (chargeOkay will be set to True or False by CorrectChargeEmpirical)
            if (searchOptions.FindCharge)
            {
                correctedCharge = totalCharge;
                chargeOkay = validator.CorrectChargeEmpirical(ref correctedCharge, searchOptions.LimitChargeRange,
                    searchOptions.ChargeMin, searchOptions.ChargeMax,
                    elementAndMassRoutines.Elements.GetElementStat(1, ElementStatsType.Charge));
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

        /// <summary>
        /// Search empiricalResultSymbols for the elements in targetCountStats
        /// </summary>
        /// <param name="elementStats"></param>
        /// <param name="elementCounts"></param>
        /// <param name="targetCountStats"></param>
        /// <param name="incompleteMatch">If true, will "match" when there are additional elements in <paramref name="elementStats"/>/<paramref name="elementCounts"/> that are not specified in <paramref name="targetCountStats"/></param>
        /// <returns>True if all of the elements are present in the given counts (extra elements may also be present),
        /// false one or more is not found or has the wrong occurrence count</returns>
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable RCS1213 // Remove unused member declaration.
        private bool EmpiricalFormulaHasElementCounts(
#pragma warning restore RCS1213 // Remove unused member declaration.
#pragma warning restore IDE0051 // Remove unused private members
            IReadOnlyList<CandidateElement> elementStats,
            IReadOnlyList<int> elementCounts,
            IReadOnlyList<ElementCount> targetCountStats, bool incompleteMatch = false)
        {
            var matchedCount = 0;
            var unmatchedCount = 0;

            for (var i = 0; i < elementStats.Count; i++)
            {
                var symbol = elementStats[i].Symbol;
                var matchIndex = -1;
                for (var j = 0; j < targetCountStats.Count; j++)
                {
                    if (targetCountStats[j].Symbol == symbol)
                    {
                        matchIndex = j;
                        break;
                    }
                }

                if (matchIndex >= 0)
                {
                    if (elementCounts[i] == targetCountStats[matchIndex].Count)
                    {
                        matchedCount++;
                    }
                }
                else if (elementCounts[i] > 0)
                {
                    unmatchedCount++;
                }
                // If the count is '0' and there is not a matched symbol in targetCountStats, we always ignore it
            }

            if (matchedCount == targetCountStats.Count && (incompleteMatch || unmatchedCount == 0))
            {
                return true;
            }

            return false;
        }

        private void EstimateNumberOfOperations(int potentialElementCount, int multipleSearchMax = 0)
        {
            // Estimate the number of operations that will be performed
            recursiveCount = 0;

            if (potentialElementCount == 1)
            {
                this.maxRecursiveCount = 1;
                return;
            }

            const int numPointers = 3;

            // TODO: Overhaul this calculation, since it's no longer correct? At the very least, "maxRecursiveCount" is no longer accurate, because the maximum recursion depth is Count(candidateElements)
            // Calculate maxRecursiveCount
            var maxRecursiveCount =
                MathUtils.Combination(numPointers + potentialElementCount, potentialElementCount - 1) -
                MathUtils.Combination(potentialElementCount + numPointers - 2, numPointers - 1);

            if (maxRecursiveCount > int.MaxValue)
            {
                this.maxRecursiveCount = int.MaxValue;
            }
            else
            {
                this.maxRecursiveCount = (int)Math.Round(maxRecursiveCount);
            }

            if (multipleSearchMax > 0)
            {
                // Correct maxRecursiveCount for searching for m/z values
                this.maxRecursiveCount *= multipleSearchMax;
            }
        }

        /// <summary>
        /// Perform input validation for finding matches by mass
        /// </summary>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <param name="searchOptions"></param>
        /// <returns>List of <see cref="CandidateElements"/> if inputs of valid. List is empty if validation failed.</returns>
        private List<CandidateElement> ValidateMatchesByMassInputs(
            double targetMass,
            double massToleranceDa,
            SearchOptions searchOptions)
        {
            // Validate the Inputs
            if (!ValidateSettings(CalculationMode.MatchMolecularWeight))
            {
                return new List<CandidateElement>();
            }

            if (targetMass <= 0d)
            {
                ReportError("Target molecular weight must be greater than 0");
                return new List<CandidateElement>();
            }

            if (massToleranceDa < 0d)
            {
                ReportError("Mass tolerance cannot be negative");
                return new List<CandidateElement>();
            }

            var candidateElementsStats = GetCandidateElements(searchOptions.SearchMode);

            if (candidateElementsStats.Count == 0)
            {
                return new List<CandidateElement>();
            }

            return candidateElementsStats;
        }

        /// <summary>
        /// Find matches by mass, using already-validated inputs
        /// </summary>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <param name="searchOptions"></param>
        /// <param name="candidateElementsStats"></param>
        /// <param name="ppmMode"></param>
        /// <returns></returns>
        internal List<SearchResult> FindMatchesByMass(
            double targetMass,
            double massToleranceDa,
            SearchOptions searchOptions,
            List<CandidateElement> candidateElementsStats,
            bool ppmMode)
        {
            // NOTE: Assumes the inputs have already been validated!!!
            var sortedElementStats = candidateElementsStats.OrderByDescending(x => x.Mass).ToList();

            EstimateNumberOfOperations(sortedElementStats.Count);

            var results = new List<SearchResult>();

            if (searchOptions.FindTargetMz)
            {
                // Searching for target m/z rather than target mass

                MultipleSearchMath(sortedElementStats.Count, searchOptions, out var mzSearchChargeMin, out var mzSearchChargeMax);

                for (var currentMzCharge = mzSearchChargeMin; currentMzCharge <= mzSearchChargeMax; currentMzCharge++)
                {
                    // Call the RecursiveMWFinder repeatedly, sending targetWeight * x each time to search for target, target*2, target*3, etc.
                    RecursiveMWFinder(results, searchOptions, ppmMode, sortedElementStats, targetMass * currentMzCharge, massToleranceDa, currentMzCharge);
                }
            }
            else
            {
                RecursiveMWFinder(results, searchOptions, ppmMode, sortedElementStats, targetMass, massToleranceDa);
            }

            return results;
        }

        /// <summary>
        /// Find matched by percent composition, using already-validated inputs
        /// </summary>
        /// <param name="maximumFormulaMass"></param>
        /// <param name="searchOptions"></param>
        /// <param name="candidateElementsStats"></param>
        /// <returns></returns>
        internal List<SearchResult> FindMatchesByPercentComposition(
            double maximumFormulaMass,
            SearchOptions searchOptions,
            List<CandidateElement> candidateElementsStats)
        {
            // NOTE: Assumes the inputs have already been validated!!!
            if (candidateElementsStats.Count == 0)
            {
                return new List<SearchResult>();
            }

            var sortedElementStats = candidateElementsStats.OrderByDescending(x => x.Mass).ToList();

            EstimateNumberOfOperations(sortedElementStats.Count);

            var results = new List<SearchResult>();

            // TODO: Test this at the limits!!!! It reduced calculation times by half in the cases tested.
            // This tries to be more intelligent with percent composition calculations, turning them from a thorough search into a bounded search
            // Get the absolute minimum weight, by calculating the maximum weight out of all candidate elements using the mass and max percent composition, if there was only one instance of the element in the formula
            var minWeight = sortedElementStats.Max(x => x.Mass * Math.Max(100 / x.PercentCompositionMaximum, 1));
            foreach (var element in sortedElementStats)
            {
                element.CalculateCountRange(minWeight, maximumFormulaMass);
            }

            RecursivePCompFinder(results, searchOptions, sortedElementStats, maximumFormulaMass);

            return results;
        }

        private List<CandidateElement> GetCandidateElements(FormulaSearchModes searchMode, double percentTolerance = 0d)
        {
            var candidateElementsStats = new List<CandidateElement>();

            var customElementCounter = 0;

            foreach (var item in candidateElements)
            {
                float charge;
                double mass;
                var symbol = item.Key;
                if (elementAndMassRoutines.Elements.IsValidElementSymbol(item.Key))
                {
                    var atomicNumber = elementAndMassRoutines.Elements.GetAtomicNumber(item.Key);

                    elementAndMassRoutines.Elements.GetElement(atomicNumber, out _, out mass, out _, out charge, out _);
                }
                else
                {
                    // Working with an abbreviation or simply a mass

                    if (double.TryParse(item.Key, out var customMass))
                    {
                        // Custom element, only weight given so charge is 0
                        mass = customMass;
                        charge = 0;

                        customElementCounter++;

                        // Custom elements are named C1_ or C2_ or C3_ etc.
                        symbol = "C" + customElementCounter + "_";
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
                        var symbolReference = elementAndMassRoutines.Elements.GetAbbreviationId(abbrevSymbol);
                        if (symbolReference < 0)
                        {
                            ReportError("Unknown element or abbreviation for custom elemental weight: " + abbrevSymbol);
                            return new List<CandidateElement>();
                        }

                        // Found a normal abbreviation
                        elementAndMassRoutines.Elements.GetAbbreviation(symbolReference, out _, out var abbrevFormula, out charge, out _);

                        mass = elementAndMassRoutines.Parser.ComputeFormulaWeight(abbrevFormula);
                    }
                }

                var minCount = item.Value.MinimumCount;
                var maxCount = item.Value.MaximumCount;

                if (searchMode == FormulaSearchModes.Thorough)
                {
                    // Thorough search mode: set the bounds to the max; this means the count of an element is limited by the (maximum) mass of the target formula
                    // Just use int.MaxValue, since we don't want the maxCount to affect any of the calculation at all.
                    minCount = 0;
                    maxCount = int.MaxValue;
                }

                var candidateElement = new CandidateElement(item.Key, symbol, mass, charge, minCount, maxCount, item.Value.TargetPercentComposition, percentTolerance);

                candidateElementsStats.Add(candidateElement);
            }

            return candidateElementsStats;
        }

        private CandidateElementTolerances GetDefaultCandidateElementTolerance(int minimumCount, int maximumCount)
        {
            return new CandidateElementTolerances
            {
                MinimumCount = minimumCount,    // Only used with the Bounded search mode
                MaximumCount = maximumCount,    // Only used with the Bounded search mode
                TargetPercentComposition = 0d   // Only used when searching for percent compositions
            };
        }

        private CandidateElementTolerances GetDefaultCandidateElementTolerance(double targetPercentComposition = 0)
        {
            return new CandidateElementTolerances
            {
                MinimumCount = 0,               // Only used with the Bounded search mode; overwritten for thorough search
                MaximumCount = 10,              // Only used with the Bounded search mode; overwritten for thorough search
                TargetPercentComposition = targetPercentComposition   // Only used when searching for percent compositions
            };
        }

        /// <summary>
        /// Initializes a new search result
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <param name="ppmMode"></param>
        /// <param name="totalMass">If 0 or negative, means matching percent compositions</param>
        /// <param name="targetMass"></param>
        /// <param name="totalCharge"></param>
        /// <param name="empiricalResultSymbols"></param>
        private SearchResult GetSearchResult(
            SearchOptions searchOptions,
            bool ppmMode,
            double totalMass,
            double targetMass,
            double totalCharge,
            IEnumerable<ElementCount> empiricalResultSymbols)
        {
            try
            {
                return new SearchResult(empiricalResultSymbols, totalMass, searchOptions.FindCharge ? totalCharge : 0, targetMass, ppmMode);
            }
            catch (Exception ex)
            {
                elementAndMassRoutines.GeneralErrorHandler("GetSearchResult", ex);
                return new SearchResult(new List<ElementCount>(), 0, 0);
            }
        }

        private double GetTotalPercentComposition()
        {
            var totalTargetPercentComp = candidateElements.Sum(item => item.Value.TargetPercentComposition);
            return totalTargetPercentComp;
        }

        /// <summary>
        /// Estimate the number of operations required to search for a target m/z rather than a target mass
        /// </summary>
        /// <param name="potentialElementCount"></param>
        /// <param name="searchOptions"></param>
        /// <param name="mzSearchChargeMin"></param>
        /// <param name="mzSearchChargeMax"></param>
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
        /// Recursively search for a target mass
        /// </summary>
        /// <param name="results"></param>
        /// <param name="searchOptions"></param>
        /// <param name="ppmMode">Used to properly flag the units for the results DeltaMass</param>
        /// <param name="sortedElementStats">Candidate elements, including mass and charge. Sorted by descending mass</param>
        /// <param name="targetMass"></param>
        /// <param name="massToleranceDa"></param>
        /// <param name="multipleMtoZCharge">When searchOptions.FindTargetMZ is false, this will be 0; otherwise, the current charge being searched for</param>
        /// <param name="elementCounts">Recursion parameter: Counts of the elements that have been added to the potential formula so far</param>
        /// <param name="currentIndex">Index in candidateElementsStats to start at</param>
        /// <param name="startingMass">Recursion parameter: Weight of the potential formula, as determined by the calling level of recursion</param>
        /// <param name="startingCharge">Recursion parameter: Charge of the potential formula, as determined by the calling level of recursion</param>
        private void RecursiveMWFinder(ICollection<SearchResult> results, SearchOptions searchOptions, bool ppmMode, IReadOnlyList<CandidateElement> sortedElementStats, double targetMass, double massToleranceDa, int multipleMtoZCharge = 0, IReadOnlyList<int> elementCounts = null, int currentIndex = 0, double startingMass = 0, double startingCharge = 0)
        {
            if (elementCounts == null)
            {
                // Recursion entry state
                elementCounts = new int[sortedElementStats.Count];
            }

            if (currentIndex >= sortedElementStats.Count)
            {
                // Recursion exit condition
                return;
            }

            if (mAbortProcessing || results.Count >= maximumHits)
            {
                return;
            }

            try
            {
                var newCounts = new int[sortedElementStats.Count];

                for (var i = 0; i < sortedElementStats.Count; i++)
                {
                    newCounts[i] = elementCounts[i];
                }

                var lastMin = 0;
                if (currentIndex == sortedElementStats.Count - 1)
                {
                    // On the last/lightest candidate element; just add a bunch of it until we are near in range, to minimize additional recursion/looping
                    var diff = targetMass - massToleranceDa - startingMass;
                    lastMin = (int)Math.Floor(diff / sortedElementStats[currentIndex].Mass);
                }

                var minCurrent = Math.Max(sortedElementStats[currentIndex].CountMinimum, lastMin);
                var maxCurrent = Math.Min(sortedElementStats[currentIndex].CountMaximum, Math.Ceiling((targetMass + massToleranceDa - startingMass) / sortedElementStats[currentIndex].Mass));
                for (var currentCount = minCurrent; currentCount <= maxCurrent; currentCount++)
                {
                    var newMassTotal = startingMass + (sortedElementStats[currentIndex].Mass * currentCount);
                    var newChargeTotal = startingCharge + (sortedElementStats[currentIndex].Charge * currentCount);
                    if (newMassTotal <= targetMass + massToleranceDa)
                    {
                        // Below or within massToleranceDa, add current element's count to array
                        newCounts[currentIndex] = currentCount;   // Store current element's count in the array

                        // Update status
                        UpdateStatus();

                        // Uncomment to add a breakpoint when a certain empirical formula is encountered
                        //if (newCounts.Count(x => x > 0) >= 3)
                        //{
                        //    var debugCompound = new List<ElementCount>
                        //    {
                        //        new ElementCount("C", 7),
                        //        new ElementCount("H", 4),
                        //        new ElementCount("O", 7)
                        //    };
                        //
                        //    if (EmpiricalFormulaHasElementCounts(sortedElementStats, newCounts, debugCompound))
                        //    {
                        //        Console.WriteLine("Debug: Check this formula");
                        //    }
                        //}

                        if (newMassTotal >= targetMass - massToleranceDa && currentCount != 0)
                        {
                            // Matching compound
                            // Construct the empirical formula and verify hydrogens
                            var hOkay = ConstructAndVerifyCompound(searchOptions, sortedElementStats, newCounts, newMassTotal, newChargeTotal, out var empiricalResultSymbols, out var correctedCharge, targetMass, massToleranceDa, multipleMtoZCharge);

                            if (empiricalResultSymbols.Count > 0 && hOkay)
                            {
                                var searchResult = GetSearchResult(searchOptions, ppmMode, newMassTotal, targetMass, correctedCharge, empiricalResultSymbols);

                                results.Add(searchResult);
                            }
                        }

                        // Now recursively call this method
                        RecursiveMWFinder(results, searchOptions, ppmMode, sortedElementStats, targetMass, massToleranceDa, multipleMtoZCharge, newCounts, currentIndex + 1, newMassTotal, newChargeTotal);
                    }
                    else
                    {
                        // Mass total is beyond limit, go back/up one level to next larger element
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                elementAndMassRoutines.GeneralErrorHandler("RecursiveMWFinder", ex);
                mAbortProcessing = true;
            }
        }

        /// <summary>
        /// Recursively search for target percent composition values
        /// </summary>
        /// <param name="results"></param>
        /// <param name="searchOptions"></param>
        /// <param name="sortedElementStats"></param>
        /// <param name="maximumFormulaMass"></param>
        /// <param name="elementCounts">Recursion parameter: Counts of the elements that have been added to the potential formula so far</param>
        /// <param name="currentIndex">Recursion parameter: Index in <paramref name="sortedElementStats"/> the current level of recursion is processing</param>
        /// <param name="startingMass">Recursion parameter: Weight of the potential formula, as determined by the calling level of recursion</param>
        /// <param name="startingCharge">Recursion parameter: Charge of the potential formula, as determined by the calling level of recursion</param>
        private void RecursivePCompFinder(ICollection<SearchResult> results, SearchOptions searchOptions, IReadOnlyList<CandidateElement> sortedElementStats, double maximumFormulaMass, IReadOnlyList<int> elementCounts = null, int currentIndex = 0, double startingMass = 0, double startingCharge = 0)
        {
            const bool ppmMode = false;

            if (elementCounts == null)
            {
                // Recursion entry state
                elementCounts = new int[sortedElementStats.Count];
            }

            if (currentIndex >= sortedElementStats.Count)
            {
                // Recursion exit condition
                return;
            }

            if (mAbortProcessing || results.Count >= maximumHits)
            {
                return;
            }

            try
            {
                var newCounts = new int[sortedElementStats.Count];

                for (var i = 0; i < sortedElementStats.Count; i++)
                {
                    newCounts[i] = elementCounts[i];
                }

                // Could possibly always use '1' as the minimum, but 0% could be hit with a small enough value and wide enough tolerance, so...
                var min = 1;
                if (sortedElementStats[currentIndex].PercentCompositionMinimum <= 0)
                {
                    min = 0;
                }
                var minCurrent = Math.Max(sortedElementStats[currentIndex].CountMinimum, min);
                var maxCurrent = Math.Min(sortedElementStats[currentIndex].CountMaximum, Math.Ceiling((maximumFormulaMass - startingMass) / sortedElementStats[currentIndex].Mass));
                for (var currentCount = minCurrent; currentCount <= maxCurrent; currentCount++)
                {
                    var newMassTotal = startingMass + (sortedElementStats[currentIndex].Mass * currentCount);
                    var newChargeTotal = startingCharge + (sortedElementStats[currentIndex].Charge * currentCount);

                    if (newMassTotal <= maximumFormulaMass)
                    {
                        // only proceed if weight is less than max weight

                        newCounts[currentIndex] = currentCount;   // Store current element's number at end of intNewPotentialElementPointers array

                        if (newCounts.All(x => x > 0))
                        {
                            // Only proceed if all elements are present
                            // Compute % comp of each element
                            var countInRange = 0;
                            for (var i = 0; i < sortedElementStats.Count; i++)
                            {
                                var pctComp = newCounts[i] * sortedElementStats[i].Mass / newMassTotal * 100d;
                                if (pctComp >= sortedElementStats[i].PercentCompositionMinimum && pctComp <= sortedElementStats[i].PercentCompositionMaximum)
                                {
                                    countInRange++;
                                }
                            }

                            if (countInRange == sortedElementStats.Count)
                            {
                                // Matching compound

                                // Uncomment to add a breakpoint when a certain empirical formula is encountered
                                //var debugCompound = new List<ElementCount>
                                //{
                                //    new ElementCount("C", 13),
                                //    new ElementCount("H", 2),
                                //    new ElementCount("N", 3),
                                //    new ElementCount("O", 1)
                                //};
                                //
                                //if (EmpiricalFormulaHasElementCounts(sortedElementStats, newCounts, debugCompound))
                                //{
                                //    Console.WriteLine("Debug: Check this formula");
                                //}

                                // Construct the empirical formula and verify hydrogens
                                var hOkay = ConstructAndVerifyCompound(searchOptions,
                                    sortedElementStats, newCounts,
                                    newMassTotal, newChargeTotal,
                                    out var empiricalResultSymbols,
                                    out var correctedCharge);

                                if (empiricalResultSymbols.Count > 0 && hOkay)
                                {
                                    var searchResult = GetSearchResult(searchOptions, ppmMode, newMassTotal, -1, correctedCharge, empiricalResultSymbols);

                                    // Add % composition info
                                    for (var i = 0; i < sortedElementStats.Count; i++)
                                    {
                                        var count = newCounts[i];
                                        var element = sortedElementStats[i];
                                        if (count != 0)
                                        {
                                            var percentComposition = count * element.Mass / newMassTotal * 100d;
                                            searchResult.AddPercentComposition(element.Symbol, percentComposition);
                                        }
                                    }

                                    results.Add(searchResult);
                                }
                            }
                        }

                        // Update status
                        UpdateStatus();

                        if (mAbortProcessing || results.Count >= maximumHits)
                        {
                            return;
                        }

                        // Haven't reached maximumMass
                        // Now recursively call this sub
                        RecursivePCompFinder(results, searchOptions, sortedElementStats, maximumFormulaMass, newCounts, currentIndex + 1, newMassTotal, newChargeTotal);
                    }
                    else
                    {
                        // Mass total is beyond limit, go back to next larger element
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                elementAndMassRoutines.GeneralErrorHandler("RecursivePCompFinder", ex);
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
            recursiveCount++;

            if (recursiveCount <= maxRecursiveCount)
            {
                PercentComplete = recursiveCount / (float)maxRecursiveCount * 100f;

                ProgressReporter?.Report(PercentComplete);
            }
        }

        private void ValidateBoundedSearchValues()
        {
            foreach (var elementSymbol in candidateElements.Keys.ToList())
            {
                var elementTolerances = candidateElements[elementSymbol];

                if (elementTolerances.MinimumCount < 0 || elementTolerances.MaximumCount > MAX_BOUNDED_SEARCH_COUNT)
                {
                    if (elementTolerances.MinimumCount < 0)
                        elementTolerances.MinimumCount = 0;
                    if (elementTolerances.MaximumCount > MAX_BOUNDED_SEARCH_COUNT)
                        elementTolerances.MaximumCount = MAX_BOUNDED_SEARCH_COUNT;

                    candidateElements[elementSymbol] = elementTolerances;
                }
            }
        }

        private void ValidatePercentCompositionValues()
        {
            foreach (var elementSymbol in candidateElements.Keys)
            {
                var elementTolerances = candidateElements[elementSymbol];

                if (elementTolerances.TargetPercentComposition < 0d || elementTolerances.TargetPercentComposition > 100d)
                {
                    if (elementTolerances.TargetPercentComposition < 0d)
                        elementTolerances.TargetPercentComposition = 0d;
                    if (elementTolerances.TargetPercentComposition > 100d)
                        elementTolerances.TargetPercentComposition = 100d;

                    candidateElements[elementSymbol] = elementTolerances;
                }
            }
        }

        private bool ValidateSettings(CalculationMode calculationMode)
        {
            if (candidateElements.Count == 0)
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

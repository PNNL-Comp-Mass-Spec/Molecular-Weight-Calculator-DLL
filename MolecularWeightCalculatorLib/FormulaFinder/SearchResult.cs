using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator.FormulaFinder
{
    /// <summary>
    /// Formula finder search result
    /// </summary>
    [Guid("4ACF7379-E609-4331-856E-45B06F408212"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class SearchResult : IFormulaFinderSearchResult, IComparable<SearchResult>
    {
        // Ignore Spelling: dm, interop

        public SearchResult(IEnumerable<ElementCount> empiricalResultSymbols, double totalMass, double totalCharge = 0, double targetMass = 0, bool isPpmMode = false)
        {
            var counts = empiricalResultSymbols.Where(x => !string.IsNullOrWhiteSpace(x.Symbol) && x.Count > 0).ToList();
            counts.Sort();
            CountsByElement = counts;
            EmpiricalFormula = string.Concat(CountsByElement.Select(x => x.ToString()));

            percentComposition = new List<ElementPercent>();

            Mass = totalMass;

            ChargeState = (int)Math.Round(totalCharge);

            if (targetMass > 0d)
            {
                if (isPpmMode)
                {
                    DeltaMass = (totalMass / targetMass - 1d) * 1000000.0d;
                    DeltaMassIsPPM = true;
                }
                else
                {
                    DeltaMass = totalMass - targetMass;
                    DeltaMassIsPPM = false;
                }
            }

            if (Math.Abs(totalCharge) > 0.1d)
            {
                // Compute m/z value
                Mz = Math.Abs(totalMass / totalCharge);
            }

            if (CountsByElement.Count > 0)
            {
                SortKey = ComputeSortKey();
            }
            else
            {
                SortKey = string.Empty;
            }
        }

        private readonly List<ElementPercent> percentComposition;

        public string EmpiricalFormula { get; }

        public IReadOnlyList<ElementCount> CountsByElement { get; }

        public double Mass { get; }
        public double DeltaMass { get; }
        public bool DeltaMassIsPPM { get; }
        public double Mz { get; }

        public int ChargeState { get; }

        /// <summary>
        /// Percent composition results (only valid if matching percent compositions)
        /// </summary>
        /// <remarks>Keys are element or abbreviation symbols, values are percent composition, between 0 and 100</remarks>
        public IReadOnlyList<ElementPercent> PercentComposition => percentComposition;

        public string SortKey { get; }

        public override string ToString()
        {
            if (DeltaMassIsPPM)
            {
                return EmpiricalFormula + "   MW=" + Mass.ToString("0.0000") + "   dm=" + DeltaMass.ToString("0.00") + " ppm";
            }

            return EmpiricalFormula + "   MW=" + Mass.ToString("0.0000") + "   dm=" + DeltaMass.ToString("0.0000");
        }

        internal void AddPercentComposition(string symbol, double percent)
        {
            percentComposition.Add(new ElementPercent(symbol, percent));
            percentComposition.Sort();
        }

        /// <summary>
        /// Method to get data from CountsByElement that is compatible with COM interop
        /// </summary>
        /// <param name="symbols"></param>
        /// <param name="counts"></param>
        /// <returns>Count of symbols/counts returned</returns>
        public int GetElementCounts(out string[] symbols, out int[] counts)
        {
            symbols = new string[CountsByElement.Count];
            counts = new int[CountsByElement.Count];
            var counter = 0;

            foreach (var entry in CountsByElement)
            {
                symbols[counter] = entry.Symbol;
                counts[counter] = entry.Count;
                counter++;
            }

            return CountsByElement.Count;
        }

        /// <summary>
        /// Method to get data from PercentComposition that is compatible with COM interop
        /// </summary>
        /// <param name="symbols"></param>
        /// <param name="percentCompositions"></param>
        /// <returns>Count of symbols/percentCompositions returned</returns>
        public int GetPercentCompositions(out string[] symbols, out double[] percentCompositions)
        {
            symbols = new string[PercentComposition.Count];
            percentCompositions = new double[PercentComposition.Count];
            var counter = 0;

            foreach (var entry in PercentComposition)
            {
                symbols[counter] = entry.Symbol;
                percentCompositions[counter] = entry.Percent;
                counter++;
            }

            return PercentComposition.Count;
        }

        private static readonly Regex customElementMatchRegex = new Regex(@"^C(?<num>\d)_", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private const int maxNumberCode = char.MaxValue - 40;

        private string ComputeSortKey(StringBuilder codeString = null)
        {
            // Precedence order for codeString
            // C1_ C2_ C3_ C4_ C5_ C6_ C7_ C8_ C9_  a   z    1,  2,  3...
            // 1   2   3   4   5   6   7   8   9   11  36   41  42  43
            //
            // C# char is 16-bits, so larger numbers can be represented without funny business, with a max value of 'char.MaxValue - 40'
            // Custom elements are converted to (char)1, (char)2, etc.
            // Letters are converted to (char)11 through (char)36
            // Number are converted to (char)40 through (char)65535

            if (codeString == null)
            {
                codeString = new StringBuilder();
            }
            else
            {
                codeString.Clear();
            }
            // Process the formula for the result to get the formula sort code
            foreach (var elementCount in CountsByElement)
            {
                var customMatch = customElementMatchRegex.Match(elementCount.Symbol);
                if (customMatch.Success)
                {
                    // at a 'custom element'
                    var customElementNum = int.Parse(customMatch.Groups["num"].Value);
                    // At a custom element, give it a 'char' value of 1 to 6
                    codeString.Append((char)customElementNum);
                }
                else
                {
                    // 65 is the ascii code for the letter 'A'
                    // Thus, 65-54 = 22; subtract 60 since custom elements are (char)1 to (char)6, so A is (char)11
                    // Drop anything here that is not a letter
                    foreach (var c in elementCount.Symbol.ToUpper().Where(char.IsLetter))
                    {
                        codeString.Append((char)(c - 54));
                    }
                }

                // Add 40 so that the number 1 is represented as 41
                // Force number within allowed range...
                var number = Math.Min(elementCount.Count + 40, maxNumberCode);
                codeString.Append((char)number);
            }

            return codeString.ToString();
        }

        public int CompareTo(SearchResult other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(SortKey, other.SortKey, StringComparison.Ordinal); // Ordinal sort, because we want to sort by binary values
        }

        public class FormulaSort : IComparer<SearchResult>
        {
            public int Compare(SearchResult x, SearchResult y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return string.Compare(x.SortKey, y.SortKey, StringComparison.Ordinal); // Ordinal sort, because we want to sort by binary values
            }
        }

        public class MolecularWeightSort : IComparer<SearchResult>
        {
            public int Compare(SearchResult x, SearchResult y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Mass.CompareTo(y.Mass);
            }
        }

        public class DeltaMassSort : IComparer<SearchResult>
        {
            public int Compare(SearchResult x, SearchResult y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.DeltaMass.CompareTo(y.DeltaMass);
            }
        }

        public class ChargeSort : IComparer<SearchResult>
        {
            public int Compare(SearchResult x, SearchResult y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.ChargeState.CompareTo(y.ChargeState);
            }
        }

        public class MzSort : IComparer<SearchResult>
        {
            public int Compare(SearchResult x, SearchResult y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Mz.CompareTo(y.Mz);
            }
        }
    }
}

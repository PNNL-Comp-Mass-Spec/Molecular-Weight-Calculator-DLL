using System;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.FormulaFinder
{
    [ComVisible(false)]
    internal class CandidateElement : IComparable<CandidateElement>
    {
        // Ignore Spelling: Da

        public double Mass { get; }
        public double Charge { get; }

        /// <summary>
        /// User-provided (or default) minimum element count
        /// </summary>
        public int CountMinimumUser { get; }

        /// <summary>
        /// User-provided (or default) maximum element count
        /// </summary>
        public int CountMaximumUser { get; }

        /// <summary>
        /// User-provided (or default) minimum element count, or calculated minimum if set.
        /// </summary>
        public int CountMinimum => useCalculatedCountRange ? countCalculatedMinimum : CountMinimumUser;

        /// <summary>
        /// User-provided (or default) maximum element count, or calculated maximum if set.
        /// </summary>
        public int CountMaximum => useCalculatedCountRange ? countCalculatedMaximum : CountMaximumUser;

        public double PercentCompositionMinimum { get; }
        public double PercentCompositionMaximum { get; }

        public string OriginalName { get; }

        public string Symbol { get; }

        public CandidateElement(string elementOrAbbrevSymbol, string symbol, double mass, double charge, int countMinimum, int countMaximum, double percent, double tolerance)
        {
            OriginalName = elementOrAbbrevSymbol;
            Symbol = symbol;
            Mass = mass;
            Charge = charge;
            CountMinimumUser = countMinimum;
            CountMaximumUser = countMaximum;
            PercentCompositionMinimum = percent - tolerance;  // Lower bound of target percentage
            PercentCompositionMaximum = percent + tolerance;  // Upper bound of target percentage
        }

        private bool useCalculatedCountRange = false;
        private int countCalculatedMinimum = 0;
        private int countCalculatedMaximum = 0;

        /// <summary>
        /// Set the calculated count range for this candidate element based on the percent composition
        /// </summary>
        /// <param name="minimumFormulaMass"></param>
        /// <param name="maximumFormulaMass"></param>
        public void CalculateCountRange(double minimumFormulaMass, double maximumFormulaMass)
        {
            // Guarantee that the used values are also within the provided bounds, with Math.Max(CountMinimumUser, [calculation]) and Math.Min(CountMaximumUser, [calculation])
            // Calculated minimum count (subtract one from the floor calculation to guarantee coverage). Uses minimum percent composition for maximum coverage.
            countCalculatedMinimum = (int)Math.Max(CountMinimumUser, Math.Floor((PercentCompositionMinimum * minimumFormulaMass / 100d) / Mass) - 1);
            // Calculated maximum count (add one to the ceiling calculation to guarantee coverage). Uses maximum percent composition for maximum coverage.
            countCalculatedMaximum = (int)Math.Min(CountMaximumUser, Math.Ceiling((PercentCompositionMaximum * maximumFormulaMass / 100d) / Mass) + 1);
            useCalculatedCountRange = true;
        }

        public void ResetCalculatedCountRange()
        {
            useCalculatedCountRange = false;
        }

        public int CompareTo(CandidateElement other)
        {
            var result = string.Compare(Symbol, other.Symbol, StringComparison.Ordinal);
            if (result != 0)
            {
                // Always sort C/Carbon first
                if (Symbol == "C") return -1;
                if (other.Symbol == "C") return 1;

                // Always sort H/Hydrogen second
                if (Symbol == "H") return -1;
                if (other.Symbol == "H") return 1;
            }

            // Sort everything else alphabetical
            return result;
        }

        public override string ToString()
        {
            if (string.Equals(Symbol, OriginalName))
            {
                return Symbol + ": " + Mass.ToString("0.0000") + " Da, charge " + Charge;
            }

            return OriginalName + "(" + Symbol + "): " + Mass.ToString("0.0000") + " Da, charge " + Charge;
        }
    }
}
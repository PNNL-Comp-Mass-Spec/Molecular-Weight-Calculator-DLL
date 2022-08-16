using System;

namespace MolecularWeightCalculator.FormulaFinder
{
    /// <summary>
    /// Simple container object to use instead of KeyValuePair&lt;string, int&gt; to provide the desired ToString() formatting
    /// </summary>
    // Implement IComparable for sorting, and IEquatable to support use of List.Contains(ElementCount obj)
    public readonly struct ElementCount : IComparable<ElementCount>, IEquatable<ElementCount>
    {
        public string Symbol { get; }
        public int Count { get; }

        public ElementCount(string symbol, int count)
        {
            Symbol = symbol;
            Count = count;
        }

        /// <summary>
        /// ToString override - allows use of string.Concat(IEnumerable&lt;ElementCount&gt;) to create an empirical formula
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Symbol + (Count > 1 ? Count : "");
        }

        public int CompareTo(ElementCount other)
        {
            var symbolComparison = string.Compare(Symbol, other.Symbol, StringComparison.Ordinal);
            if (symbolComparison != 0)
            {
                // Always sort C/Carbon first
                if (Symbol == "C") return -1;
                if (other.Symbol == "C") return 1;

                // Always sort H/Hydrogen second
                if (Symbol == "H") return -1;
                if (other.Symbol == "H") return 1;

                // Sort everything else alphabetical
                return symbolComparison;
            }

            return Count.CompareTo(other.Count);
        }

        public bool Equals(ElementCount other)
        {
            return Symbol == other.Symbol && Count == other.Count;
        }

        public override bool Equals(object obj)
        {
            return obj is ElementCount other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Symbol != null ? Symbol.GetHashCode() : 0) * 397) ^ Count;
            }
        }
    }
}

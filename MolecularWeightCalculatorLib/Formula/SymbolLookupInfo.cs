using System;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    internal enum SymbolMatchMode
    {
        Unknown = 0,
        Element = 1,
        Abbreviation = 2
    }

    /// <summary>
    /// struct for data for mMasterSymbolsList; using a struct because it means less space, and we don't edit the struct
    /// </summary>
    [ComVisible(false)]
    internal struct SymbolLookupInfo : IComparable<SymbolLookupInfo>
    {
        /// <summary>
        /// Symbol to match - can be an abbreviation or a chemical/atomic symbol for an element
        /// </summary>
        public readonly string Symbol;

        /// <summary>
        /// Basically, a reference to which list <see cref="Index"/> contains this symbol
        /// </summary>
        public readonly SymbolMatchMode MatchType;

        /// <summary>
        /// The index of this symbol in the list referred to by <see cref="MatchType"/>
        /// </summary>
        public readonly int Index;

        public SymbolLookupInfo(string symbol, int index, SymbolMatchMode matchType = SymbolMatchMode.Unknown)
        {
            Symbol = symbol;
            Index = index;
            MatchType = matchType;
        }

        public int CompareTo(SymbolLookupInfo other)
        {
            // For sorting: sort longest to shortest, then alphabetically
            // 'other' first to sort by length descending
            var lengthCompare = other.Symbol.Length.CompareTo(Symbol.Length);
            if (lengthCompare == 0)
            {
                return string.Compare(Symbol, other.Symbol, StringComparison.Ordinal);
            }

            return lengthCompare;
        }

        public override string ToString()
        {
            return $"{Symbol}: {MatchType} index {Index}";
        }
    }
}
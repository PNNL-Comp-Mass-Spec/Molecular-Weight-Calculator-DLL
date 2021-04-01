using System;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    internal class AbbrevStatsData : IComparable<AbbrevStatsData>
    {
        /// <summary>
        /// The symbol for the abbreviation, e.g. Ph for the phenyl group or Ala for alanine (3 letter codes for amino acids)
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Empirical formula
        /// Cannot contain other abbreviations
        /// </summary>
        public string Formula { get; set; }

        /// <summary>
        /// Computed mass for quick reference
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// Computed standard deviation for quick reference
        /// </summary>
        public double StdDev { get; set; }

        /// <summary>
        /// Charge state
        /// </summary>
        public float Charge { get; set; }

        /// <summary>
        /// True if an amino acid
        /// </summary>
        public bool IsAminoAcid { get; set; }

        /// <summary>
        /// One letter symbol (only used for amino acids)
        /// </summary>
        public string OneLetterSymbol { get; set; }

        /// <summary>
        /// Description of the abbreviation
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// True if this abbreviation has an invalid symbol or formula
        /// </summary>
        public bool InvalidSymbolOrFormula { get; set; }

        public AbbrevStatsData(string symbol, string formula, float charge, bool isAminoAcid, string oneLetterSymbol = "", string comment = "", bool invalidSymbolOrFormula = false)
        {
            InvalidSymbolOrFormula = invalidSymbolOrFormula;
            Symbol = symbol;
            Formula = formula;
            Mass = 0d;
            StdDev = 0;
            Charge = charge;
            OneLetterSymbol = oneLetterSymbol.ToUpper();
            IsAminoAcid = isAminoAcid;
            Comment = comment;
        }

        public override string ToString()
        {
            return Symbol + ": " + Formula;
        }

        public int CompareTo(AbbrevStatsData other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Symbol, other.Symbol, StringComparison.Ordinal);
        }
    }
}
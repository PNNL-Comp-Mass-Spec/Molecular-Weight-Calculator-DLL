using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    internal class AbbrevStatsData : IComparable<AbbrevStatsData>
    {
        /// <summary>
        /// The symbol for the abbreviation, e.g. Ph for the phenyl group or Ala for alanine (3 letter codes for amino acids)
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Empirical formula
        /// Cannot contain other abbreviations
        /// </summary>
        public string Formula { get; }

        /// <summary>
        /// Computed mass for quick reference
        /// </summary>
        public double Mass { get; internal set; }

        /// <summary>
        /// Computed standard deviation for quick reference
        /// </summary>
        public double StdDev { get; internal set; }

        /// <summary>
        /// Charge state
        /// </summary>
        public float Charge { get; internal set; }

        /// <summary>
        /// True if an amino acid
        /// </summary>
        public bool IsAminoAcid { get; }

        /// <summary>
        /// One letter symbol (only used for amino acids)
        /// </summary>
        public string OneLetterSymbol { get; }

        /// <summary>
        /// Description of the abbreviation
        /// </summary>
        public string Comment { get; }

        /// <summary>
        /// True if this abbreviation has an invalid symbol or formula
        /// </summary>
        public bool InvalidSymbolOrFormula { get; internal set; }

        /// <summary>
        /// Element counts for this abbreviation. Key is element symbol reference, value element use information.
        /// </summary>
        public IReadOnlyDictionary<int, IElementUseStats> ElementsUsed => elementsUsed;

        public IReadOnlyList<string> AbbreviationsUsed => abbreviationsUsed;

        private Dictionary<int, IElementUseStats> elementsUsed = new Dictionary<int, IElementUseStats>();
        private List<string> abbreviationsUsed = new List<string>();

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

        public void AddUsedElements(IReadOnlyList<ElementUseStats> elements)
        {
            elementsUsed.Clear();

            for (var i = 1; i <= ElementsAndAbbrevs.ELEMENT_COUNT; i++)
            {
                var element = elements[i];
                if (element.Used)
                {
                    elementsUsed.Add(i, element.Clone());
                }
            }
        }

        public void ClearElements()
        {
            elementsUsed.Clear();
        }

        public void SetUsedAbbreviations(IReadOnlyList<string> abbrevsUsed)
        {
            abbreviationsUsed.Clear();
            abbreviationsUsed.Capacity = 0;
            abbreviationsUsed.AddRange(abbrevsUsed);
        }

        public override string ToString()
        {
            return Symbol + ": " + Formula;
        }

        /// <summary>
        /// Sort comparator: sort alphabetically.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(AbbrevStatsData other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(Symbol, other.Symbol, StringComparison.Ordinal);
        }
    }
}
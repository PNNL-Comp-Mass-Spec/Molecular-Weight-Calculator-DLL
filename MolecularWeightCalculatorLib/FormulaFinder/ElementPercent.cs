using System;

namespace MolecularWeightCalculator.FormulaFinder
{
    /// <summary>
    /// Simple container object to use instead of KeyValuePair&lt;string, double&gt; to provide the desired ToString() formatting
    /// </summary>
    public readonly struct ElementPercent : IComparable<ElementPercent>
    {
        public string Symbol { get; }
        public double Percent { get; }

        public ElementPercent(string symbol, double percent)
        {
            Symbol = symbol;
            Percent = percent;
        }

        public override string ToString()
        {
            return $"{Symbol}={Percent:##.##}%";
        }

        public int CompareTo(ElementPercent other)
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

            return Percent.CompareTo(other.Percent);
        }
    }
}

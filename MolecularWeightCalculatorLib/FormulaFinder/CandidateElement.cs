using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.FormulaFinder
{
    [ComVisible(false)]
    internal class CandidateElement
    {
        // Ignore Spelling: Da

        public double Mass { get; set; }
        public double Charge { get; set; }

        public int CountMinimum { get; set; }
        public int CountMaximum { get; set; }

        public double PercentCompMinimum { get; set; }
        public double PercentCompMaximum { get; set; }

        public string OriginalName { get; }

        public string Symbol { get; set; }

        public CandidateElement(string elementOrAbbrevSymbol)
        {
            OriginalName = string.Copy(elementOrAbbrevSymbol);
            Symbol = string.Copy(elementOrAbbrevSymbol);
        }

        public override string ToString()
        {
            if ((Symbol ?? string.Empty) == (OriginalName ?? string.Empty))
            {
                return Symbol + ": " + Mass.ToString("0.0000") + " Da, charge " + Charge;
            }

            return OriginalName + "(" + Symbol + "): " + Mass.ToString("0.0000") + " Da, charge " + Charge;
        }
    }
}
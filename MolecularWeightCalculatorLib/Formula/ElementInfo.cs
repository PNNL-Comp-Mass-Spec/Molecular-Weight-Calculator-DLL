using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    public class ElementInfo
    {
        public string Symbol { get; }
        public double Mass { get; set; }
        public double Uncertainty { get; set; }
        public float Charge { get; set; }
        public List<IsotopeInfo> Isotopes { get; } // Masses and Abundances of the isotopes; 0-based array, ranging from 0 to MAX_Isotopes - 1 (at most)

        public ElementInfo()
        {
            Symbol = "";
            Isotopes = new List<IsotopeInfo>();
        }

        public ElementInfo(string symbol, float charge, double mass, double uncertainty = 0)
        {
            Symbol = symbol;
            Charge = charge;
            Mass = mass;
            Uncertainty = uncertainty;
            Isotopes = new List<IsotopeInfo>(ElementsAndAbbrevs.MAX_ISOTOPES);
        }

        public override string ToString()
        {
            return Symbol + ": " + Mass.ToString("0.0000");
        }
    }
}
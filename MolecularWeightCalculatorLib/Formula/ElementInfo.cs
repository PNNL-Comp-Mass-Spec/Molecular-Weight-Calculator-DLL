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

        /// <summary>
        /// Masses and relative abundance of isotopes tracked for this element
        /// </summary>
        /// <remarks>
        /// 0-based list, ranging from 0 to MAX_ISOTOPES - 1 (at most)
        /// </remarks>
        public List<IsotopeInfo> Isotopes { get; }

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public ElementInfo()
        {
            Symbol = string.Empty;
            Isotopes = new List<IsotopeInfo>();
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="charge"></param>
        /// <param name="mass"></param>
        /// <param name="uncertainty"></param>
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
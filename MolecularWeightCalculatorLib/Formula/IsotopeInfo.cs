using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    /// <summary>
    /// Isotope info
    /// </summary>
    [ComVisible(false)]
    public class IsotopeInfo
    {
        /// <summary>
        /// Monoisotopic mass
        /// </summary>
        public double Mass { get; }

        /// <summary>
        /// Relative abundance of this isotope (aka isotopic composition)
        /// </summary>
        /// <remarks>Value between 0 and 1</remarks>
        public float Abundance { get; }

        public IsotopeInfo(double mass, float abundance)
        {
            Mass = mass;
            Abundance = abundance;
        }

        public override string ToString()
        {
            return Mass.ToString("0.0000");
        }
    }
}
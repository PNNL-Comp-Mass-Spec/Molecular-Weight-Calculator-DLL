using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    /// <summary>
    /// Percent composition info
    /// </summary>
    [ComVisible(false)]
    public class PercentCompositionInfo
    {
        public double PercentComposition { get; set; }
        public double StdDeviation { get; set; }

        public override string ToString()
        {
            return PercentComposition.ToString("0.0000");
        }

        public PercentCompositionInfo Clone()
        {
            // Shallow copy is sufficient - no reference members
            return (PercentCompositionInfo) MemberwiseClone();
        }
    }
}
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    public class IsotopeInfo
    {
        public double Mass { get; }
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
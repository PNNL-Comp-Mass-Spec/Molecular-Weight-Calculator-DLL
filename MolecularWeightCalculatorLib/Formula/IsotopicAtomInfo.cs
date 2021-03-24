using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    public class IsotopicAtomInfo
    {
        public double Count { get; set; } // Can have non-integer counts of atoms, e.g. ^13C5.5
        public double Mass { get; set; }

        public IsotopicAtomInfo Clone()
        {
            // Shallow copy is sufficient - no reference members
            return (IsotopicAtomInfo) MemberwiseClone();
        }

        public override string ToString()
        {
            return $"{Count}x{Mass:F2}";
        }
    }
}
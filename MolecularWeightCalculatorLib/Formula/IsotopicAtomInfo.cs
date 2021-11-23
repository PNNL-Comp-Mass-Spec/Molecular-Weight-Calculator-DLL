using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    /// <summary>
    /// Isotope atom info
    /// </summary>
    [ComVisible(false)]
    public class IsotopicAtomInfo : IIsotopicAtomInfo
    {
        /// <summary>
        /// Atom counts
        /// </summary>
        /// <remarks>Double to allow for non-integer counts of atoms, e.g. ^13C5.5</remarks>
        public double Count { get; set; }

        /// <summary>
        /// Atom mass
        /// </summary>
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

    [ComVisible(false)]
    public interface IIsotopicAtomInfo
    {
        /// <summary>
        /// Atom counts
        /// </summary>
        /// <remarks>Double to allow for non-integer counts of atoms, e.g. ^13C5.5</remarks>
        public double Count { get; }

        /// <summary>
        /// Atom mass
        /// </summary>
        public double Mass { get; }
    }
}
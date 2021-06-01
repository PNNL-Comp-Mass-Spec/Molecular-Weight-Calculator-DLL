using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Sequence
{
    /// <summary>
    /// Information on the N or C terminus of a peptide
    /// </summary>
    [ComVisible(false)]
    internal class Terminus
    {
        /// <summary>
        /// Formula
        /// </summary>
        public string Formula { get; set; }

        /// <summary>
        /// Mass
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// Amino acid just before this peptide in the protein
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Residue PrecedingResidue { get; set; } = new();

        /// <summary>
        /// Amino acid just after this peptide in the protein
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Residue FollowingResidue { get; set; } = new();
    }
}

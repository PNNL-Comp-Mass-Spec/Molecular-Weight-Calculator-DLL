using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Sequence
{
    /// <summary>
    /// Amino acid residue
    /// </summary>
    [ComVisible(false)]
    internal class Residue
    {
        /// <summary>
        /// 3 letter symbol
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// The mass of the residue alone (excluding any modification)
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// The mass of the residue, including any modification (e.g. phosphorylation)
        /// </summary>
        public double MassWithMods { get; set; }

        /// <summary>
        /// The masses that the a, b, and y ions ending/starting with this residue will produce in the mass spectrum (includes H+)
        /// </summary>
        /// <remarks>
        /// 0-based array
        /// </remarks>
        public double[] IonMass { get; }

        /// <summary>
        /// Technically, only Ser, Thr, or Tyr residues can be phosphorylated (H3PO4), but if the user phosphorylates other residues, we'll allow that
        /// </summary>
        public bool Phosphorylated { get; set; }

        public List<int> ModificationIDs { get; }

        public Residue()
        {
            IonMass = new double[Enum.GetNames(typeof(IonType)).Length];
            ModificationIDs = new List<int>(Peptide.MAX_MODIFICATIONS);
        }

        public Residue(string symbol) : this()
        {
            Symbol = symbol;
            Phosphorylated = false;
        }

        /// <summary>
        /// Show the residue symbol, mass, b ion m/z, and y ion m/z
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}: {1:F2}, b {2:F2}, y {3:F2}", Symbol, Mass, IonMass[1], IonMass[2]);
        }
    }
}

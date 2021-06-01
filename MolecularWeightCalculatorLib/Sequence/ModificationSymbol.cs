using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Sequence
{
    [ComVisible(false)]
    internal class ModificationSymbol
    {
        /// <summary>
        /// Modification symbol
        /// </summary>
        /// <remarks>
        /// May be 1 or more characters; for example: + ++ * ** etc.
        /// </remarks>
        public string Symbol { get; }

        /// <summary>
        /// Modification mass
        /// </summary>
        /// <remarks>
        /// Typically positive, but can be negative
        /// </remarks>
        public double ModificationMass { get; set; }

        /// <summary>
        /// When true, this symbol indicates a phosphorylated residue
        /// </summary>
        public bool IndicatesPhosphorylation { get; set; }

        public string Comment { get; set; }

        public ModificationSymbol(string symbol, double modMass, bool indicatesPhosphorylation, string comment = "")
        {
            Symbol = symbol;
            ModificationMass = modMass;
            IndicatesPhosphorylation = indicatesPhosphorylation;
            Comment = comment;
        }
    }
}

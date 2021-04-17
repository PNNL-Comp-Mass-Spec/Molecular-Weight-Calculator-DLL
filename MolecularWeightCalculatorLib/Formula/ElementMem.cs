using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    internal struct ElementMem
    {
        /// <summary>
        /// Elemental symbol
        /// </summary>
        public readonly string Symbol;

        /// <summary>
        /// Oxidation state
        /// </summary>
        public readonly int Charge;

        /// <summary>
        /// Integer mass closest to the isotopic mass
        /// </summary>
        public readonly int MassInteger;

        /// <summary>
        /// Monoisotopic mass
        /// </summary>
        public readonly double MassIsotopic;

        /// <summary>
        /// Atomic weight (average mass)
        /// </summary>
        public readonly double MassAverage;

        /// <summary>
        /// atomic weight uncertainty
        /// </summary>
        public readonly double UncertaintyAverageMass;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="charge"></param>
        /// <param name="nominalMass"></param>
        /// <param name="isotopicMass"></param>
        /// <param name="averageMass"></param>
        /// <param name="uncertaintyAverageMass"></param>
        public ElementMem(string symbol, int charge, int nominalMass, double isotopicMass, double averageMass, double uncertaintyAverageMass)
        {
            Symbol = symbol;
            Charge = charge;
            MassInteger = nominalMass;
            MassIsotopic = isotopicMass;
            MassAverage = averageMass;
            UncertaintyAverageMass = uncertaintyAverageMass;
        }
    }
}
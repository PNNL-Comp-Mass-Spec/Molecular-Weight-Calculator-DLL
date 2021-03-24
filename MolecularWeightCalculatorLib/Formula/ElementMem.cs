using System.Runtime.InteropServices;

namespace MolecularWeightCalculator.Formula
{
    [ComVisible(false)]
    internal struct ElementMem
    {
        public readonly string Symbol;
        public readonly int Charge;
        public readonly int MassInteger;
        public readonly double MassIsotopic;
        public readonly double MassAverage;
        public readonly double UncertaintyAverageMass;

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
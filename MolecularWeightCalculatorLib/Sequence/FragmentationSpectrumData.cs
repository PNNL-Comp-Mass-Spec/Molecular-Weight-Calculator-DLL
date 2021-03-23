using System;

namespace MolecularWeightCalculator.Sequence
{
    public class FragmentationSpectrumData : IComparable<FragmentationSpectrumData>
    {
        public double Mass { get; }
        public double Intensity { get; }
        public string Symbol { get; } // The symbol, with the residue number (e.g. y1, y2, b3-H2O, Shoulder-y1, etc.)
        public string SymbolGeneric { get; } // The symbol, without the residue number (e.g. a, b, y, b++, Shoulder-y, etc.)
        public int SourceResidueNumber { get; } // The residue number that resulted in this mass
        public string SourceResidueSymbol3Letter { get; } // The residue symbol that resulted in this mass
        public short Charge { get; }
        public IonType IonType { get; }
        public bool IsShoulderIon { get; } // B and Y ions can have Shoulder ions at +-1

        public FragmentationSpectrumData()
        {
        }

        public FragmentationSpectrumData(float mass, float intensity, string ionSymbol, string ionSymbolGeneric,
            int sourceResidue, string sourceResidueSymbol3Letter, short charge, IonType ionType, bool isShoulderIon = false)
        {
            Mass = mass;
            Intensity = intensity;
            Symbol = ionSymbol;
            SymbolGeneric = ionSymbolGeneric;
            SourceResidueNumber = sourceResidue;
            SourceResidueSymbol3Letter = sourceResidueSymbol3Letter;
            Charge = charge;
            IonType = ionType;
            IsShoulderIon = isShoulderIon;
        }

        public override string ToString()
        {
            return Symbol + ", " + Mass.ToString("0.00");
        }

        public int CompareTo(FragmentationSpectrumData other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Mass.CompareTo(other.Mass);
        }
    }
}
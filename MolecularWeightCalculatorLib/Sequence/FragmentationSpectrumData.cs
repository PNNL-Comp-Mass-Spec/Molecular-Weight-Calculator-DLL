using System;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator.Sequence
{
    /// <summary>
    /// MS/MS fragmentation spectrum data
    /// </summary>
    [Guid("F558EA24-E939-4062-9B18-58CE27972B33"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class FragmentationSpectrumData : IFragmentationSpectrumData, IComparable<FragmentationSpectrumData>
    {
        /// <summary>
        /// Fragment ion mass
        /// </summary>
        public double Mass { get; }

        /// <summary>
        /// Fragment ion intensity
        /// </summary>
        public double Intensity { get; }

        /// <summary>
        /// Symbol
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Generic symbol: the symbol without the residue number (e.g. a, b, y, b++, Shoulder-y, etc.)
        /// </summary>
        public string SymbolGeneric { get; }

        /// <summary>
        /// The residue number that resulted in this fragment ion
        /// </summary>
        public int SourceResidueNumber { get; }

        /// <summary>
        /// The 3-letter residue name that resulted in this mass
        /// </summary>
        public string SourceResidueSymbol3Letter { get; }

        /// <summary>
        /// Charge
        /// </summary>
        public short Charge { get; }

        /// <summary>
        /// Ion type
        /// </summary>
        public IonType IonType { get; }

        /// <summary>
        /// True if a should ion
        /// </summary>
        /// <remarks>
        /// B and Y ions can have Shoulder ions at +-1
        /// </remarks>
        public bool IsShoulderIon { get; }

        /// <summary>
        /// Parameterless Constructor
        /// </summary>
        public FragmentationSpectrumData()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FragmentationSpectrumData(
            float mass, float intensity, string ionSymbol, string ionSymbolGeneric,
            int sourceResidue, string sourceResidueSymbol3Letter, short charge, IonType ionType,
            bool isShoulderIon = false)
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
            if (other is null) return 1;
            var massComparison = Mass.CompareTo(other.Mass);
            if (massComparison != 0) return massComparison;
            return Intensity.CompareTo(other.Intensity);
        }
    }
}
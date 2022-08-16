using System.Runtime.InteropServices;
using MolecularWeightCalculator.Sequence;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("C99EEDE1-39F5-45D5-9B49-407BF6E3BF69"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IFragmentationSpectrumData
    {
        /// <summary>
        /// Ion mass
        /// </summary>
        double Mass { get; }

        /// <summary>
        /// Ion intensity
        /// </summary>
        double Intensity { get; }

        /// <summary>
        /// The symbol, with the residue number (e.g. y1, y2, b3-H2O, Shoulder-y1, etc.)
        /// </summary>
        string Symbol { get; }

        /// <summary>
        /// The symbol, without the residue number (e.g. a, b, y, b++, Shoulder-y, etc.)
        /// </summary>
        string SymbolGeneric { get; }

        /// <summary>
        /// The residue number that resulted in this mass
        /// </summary>
        int SourceResidueNumber { get; }

        /// <summary>
        /// The residue symbol that resulted in this mass
        /// </summary>
        string SourceResidueSymbol3Letter { get; }

        /// <summary>
        /// Charge state
        /// </summary>
        short Charge { get; }

        /// <summary>
        /// Ion type
        /// </summary>
        IonType IonType { get; }

        /// <summary>
        /// True if a shoulder ion
        /// </summary>
        /// <remarks>
        /// B and Y ions can have Shoulder ions at +-1
        /// </remarks>
        bool IsShoulderIon { get; }

        string ToString();

        int CompareTo(FragmentationSpectrumData other);
    }
}

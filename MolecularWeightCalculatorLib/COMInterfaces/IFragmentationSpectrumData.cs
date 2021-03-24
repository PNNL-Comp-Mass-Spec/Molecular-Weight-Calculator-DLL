using System.Runtime.InteropServices;
using MolecularWeightCalculator.Sequence;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("C99EEDE1-39F5-45D5-9B49-407BF6E3BF69"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IFragmentationSpectrumData
    {
        double Mass { get; }
        double Intensity { get; }
        string Symbol { get; } // The symbol, with the residue number (e.g. y1, y2, b3-H2O, Shoulder-y1, etc.)
        string SymbolGeneric { get; } // The symbol, without the residue number (e.g. a, b, y, b++, Shoulder-y, etc.)
        int SourceResidueNumber { get; } // The residue number that resulted in this mass
        string SourceResidueSymbol3Letter { get; } // The residue symbol that resulted in this mass
        short Charge { get; }
        IonType IonType { get; }
        bool IsShoulderIon { get; } // B and Y ions can have Shoulder ions at +-1
        string ToString();
        int CompareTo(FragmentationSpectrumData other);
    }
}
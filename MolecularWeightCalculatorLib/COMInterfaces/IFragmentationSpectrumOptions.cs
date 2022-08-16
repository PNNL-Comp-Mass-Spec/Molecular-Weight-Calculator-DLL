using System.Runtime.InteropServices;
using MolecularWeightCalculator.Sequence;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("93545CDB-5C21-4E78-B92F-3C180482A162"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IFragmentationSpectrumOptions
    {
        FragmentationSpectrumIntensities IntensityOptions { get; set; }
        IonTypeOptions[] IonTypeOptions { get; }
        bool DoubleChargeIonsShow { get; set; }
        double DoubleChargeIonsThreshold { get; set; }
        bool TripleChargeIonsShow { get; set; }
        double TripleChargeIonsThreshold { get; set; }
    }
}

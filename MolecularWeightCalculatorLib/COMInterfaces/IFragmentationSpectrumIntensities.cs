using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("A7468A41-58E2-45BF-A39D-03A2CFE48940"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IFragmentationSpectrumIntensities
    {
        double[] IonType { get; } // 0-based array
        double BYIonShoulder { get; set; } // If > 0 then shoulder ions will be created by B and Y ions
        double NeutralLoss { get; set; }
    }
}
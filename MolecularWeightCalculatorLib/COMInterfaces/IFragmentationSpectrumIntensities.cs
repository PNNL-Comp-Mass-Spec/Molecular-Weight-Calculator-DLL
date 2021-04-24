using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMemberInSuper.Global

namespace MolecularWeightCalculator.COMInterfaces
{
    [Guid("A7468A41-58E2-45BF-A39D-03A2CFE48940"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IFragmentationSpectrumIntensities
    {
        /// <summary>
        /// Intensity to use for each ion type (0-based array)
        /// </summary>
        double[] IonType { get; }

        /// <summary>
        /// If greater than zero, the intensity to use for shoulder ions (for b and y ions)
        /// </summary>
        double BYIonShoulder { get; set; }

        /// <summary>
        /// Intensity to use for neutral loss ions
        /// </summary>
        double NeutralLoss { get; set; }
    }
}
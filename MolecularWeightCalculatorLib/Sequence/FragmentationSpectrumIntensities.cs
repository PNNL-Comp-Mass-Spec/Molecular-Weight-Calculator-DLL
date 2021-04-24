using System;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator.Sequence
{
    [Guid("9CAAFE11-4A2C-4E6E-ACD2-10E7B9C870F4"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class FragmentationSpectrumIntensities : IFragmentationSpectrumIntensities
    {
        /// <summary>
        /// Intensity to use for each ion type (0-based array)
        /// </summary>
        public double[] IonType { get; }

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// If greater than zero, the intensity to use for shoulder ions (for b and y ions)
        /// </summary>
        public double BYIonShoulder { get; set; }

        /// <summary>
        /// Intensity to use for neutral loss ions
        /// </summary>
        public double NeutralLoss { get; set; }

        public FragmentationSpectrumIntensities()
        {
            IonType = new double[Enum.GetNames(typeof(IonType)).Length];
        }
    }
}
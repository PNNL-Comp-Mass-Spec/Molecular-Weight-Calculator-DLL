using System;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator.Sequence
{
    [Guid("9CAAFE11-4A2C-4E6E-ACD2-10E7B9C870F4"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class FragmentationSpectrumIntensities : IFragmentationSpectrumIntensities
    {
        public double[] IonType { get; } // 0-based array
        // ReSharper disable once InconsistentNaming
        public double BYIonShoulder { get; set; } // If > 0 then shoulder ions will be created by B and Y ions
        public double NeutralLoss { get; set; }

        public FragmentationSpectrumIntensities()
        {
            IonType = new double[Enum.GetNames(typeof(IonType)).Length];
        }
    }
}
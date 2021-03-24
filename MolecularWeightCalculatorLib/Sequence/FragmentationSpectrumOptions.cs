using System;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator.Sequence
{
    [Guid("926548CB-BE68-49E7-BA7D-4693D31AE15C"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class FragmentationSpectrumOptions : IFragmentationSpectrumOptions
    {
        public FragmentationSpectrumIntensities IntensityOptions { get; set; } = new FragmentationSpectrumIntensities();
        public IonTypeOptions[] IonTypeOptions { get; }
        public bool DoubleChargeIonsShow { get; set; }
        public float DoubleChargeIonsThreshold { get; set; }
        public bool TripleChargeIonsShow { get; set; }
        public float TripleChargeIonsThreshold { get; set; }

        public FragmentationSpectrumOptions()
        {
            IonTypeOptions = new IonTypeOptions[Enum.GetNames(typeof(IonType)).Length];
            for (var i = 0; i < IonTypeOptions.Length; i++)
            {
                IonTypeOptions[i] = new IonTypeOptions();
            }
        }
    }
}
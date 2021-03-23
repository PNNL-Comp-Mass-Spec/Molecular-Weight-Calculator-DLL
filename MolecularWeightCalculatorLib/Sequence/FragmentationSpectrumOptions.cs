using System;

namespace MolecularWeightCalculator.Sequence
{
    public class FragmentationSpectrumOptions
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
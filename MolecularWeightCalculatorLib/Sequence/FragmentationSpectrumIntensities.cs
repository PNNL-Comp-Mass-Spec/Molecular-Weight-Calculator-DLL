using System;

namespace MolecularWeightCalculator.Sequence
{
    public class FragmentationSpectrumIntensities
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
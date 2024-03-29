﻿using System;
using System.Runtime.InteropServices;
using MolecularWeightCalculator.COMInterfaces;

namespace MolecularWeightCalculator.Sequence
{
    /// <summary>
    /// MS/MS fragmentation spectrum options
    /// </summary>
    [Guid("926548CB-BE68-49E7-BA7D-4693D31AE15C"), ClassInterface(ClassInterfaceType.None), ComVisible(true)]
    public class FragmentationSpectrumOptions : IFragmentationSpectrumOptions
    {
        public FragmentationSpectrumIntensities IntensityOptions { get; set; } = new();
        public IonTypeOptions[] IonTypeOptions { get; }
        public bool DoubleChargeIonsShow { get; set; }
        public double DoubleChargeIonsThreshold { get; set; }
        public bool TripleChargeIonsShow { get; set; }
        public double TripleChargeIonsThreshold { get; set; }

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
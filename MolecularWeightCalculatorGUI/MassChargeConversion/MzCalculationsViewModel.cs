using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using MolecularWeightCalculator.Formula;
using ReactiveUI;

namespace MolecularWeightCalculatorGUI.MassChargeConversion
{
    internal class MzCalculationsViewModel : ReactiveObject
    {
        private bool elementModeAverage = false;
        private bool elementModeIsotopic = true;
        private double massError = 100;
        private MassErrorMode massErrorMode = MassErrorMode.Ppm;
        private MassChargeLevel massChargeLevel1 = MassChargeLevel.M;
        private double massCharge1 = 1000;
        private double massCharge1Start = 999.9;
        private double massCharge1End = 1000.1;
        private MassChargeLevel massChargeLevel2 = MassChargeLevel.MPlus1H;
        private double massCharge2 = 1001.007276;
        private double massCharge2Start = 1000.907175;
        private double massCharge2End = 1001.107377;
        private readonly ElementAndMassTools massCalc = new ElementAndMassTools();
        private bool calculating = false;

        public MzCalculationsViewModel()
        {
            MassErrorModeOptions = Enum.GetValues(typeof(MassErrorMode)).Cast<MassErrorMode>().ToList();
            MassChargeLevelOptions = Enum.GetValues(typeof(MassChargeLevel)).Cast<MassChargeLevel>().ToList();

            this.WhenAnyValue(x => x.ElementModeAverage).Where(x => x).Subscribe(x => massCalc.Elements.SetElementMode(ElementMassMode.Average));
            this.WhenAnyValue(x => x.ElementModeIsotopic).Where(x => x).Subscribe(x => massCalc.Elements.SetElementMode(ElementMassMode.Isotopic));
            this.WhenAnyValue(x => x.MassCharge1, x => x.MassChargeLevel2, x => x.ElementModeAverage, x => x.ElementModeIsotopic)
                .Subscribe(x => CalculateWithMassCharge1());
            this.WhenAnyValue(x => x.MassCharge2, x => x.MassChargeLevel1)
                .Subscribe(x => CalculateWithMassCharge2());
            this.WhenAnyValue(x => x.MassError, x => x.MassErrorMode).Subscribe(x => UpdateMassErrorRanges());
        }

        public IReadOnlyList<MassErrorMode> MassErrorModeOptions { get; }
        public IReadOnlyList<MassChargeLevel> MassChargeLevelOptions { get; }

        public bool ElementModeAverage
        {
            get => elementModeAverage;
            set => this.RaiseAndSetIfChanged(ref elementModeAverage, value);
        }

        public bool ElementModeIsotopic
        {
            get => elementModeIsotopic;
            set => this.RaiseAndSetIfChanged(ref elementModeIsotopic, value);
        }

        public double MassError
        {
            get => massError;
            set => this.RaiseAndSetIfChanged(ref massError, value);
        }

        public MassErrorMode MassErrorMode
        {
            get => massErrorMode;
            set => this.RaiseAndSetIfChanged(ref massErrorMode, value);
        }

        public MassChargeLevel MassChargeLevel1
        {
            get => massChargeLevel1;
            set => this.RaiseAndSetIfChanged(ref massChargeLevel1, value);
        }

        public double MassCharge1
        {
            get => massCharge1;
            set => this.RaiseAndSetIfChanged(ref massCharge1, value);
        }

        public double MassCharge1Start
        {
            get => massCharge1Start;
            private set => this.RaiseAndSetIfChanged(ref massCharge1Start, value);
        }

        public double MassCharge1End
        {
            get => massCharge1End;
            private set => this.RaiseAndSetIfChanged(ref massCharge1End, value);
        }

        public MassChargeLevel MassChargeLevel2
        {
            get => massChargeLevel2;
            set => this.RaiseAndSetIfChanged(ref massChargeLevel2, value);
        }

        public double MassCharge2
        {
            get => massCharge2;
            set => this.RaiseAndSetIfChanged(ref massCharge2, value);
        }

        public double MassCharge2Start
        {
            get => massCharge2Start;
            private set => this.RaiseAndSetIfChanged(ref massCharge2Start, value);
        }

        public double MassCharge2End
        {
            get => massCharge2End;
            private set => this.RaiseAndSetIfChanged(ref massCharge2End, value);
        }

        private void CalculateWithMassCharge1()
        {
            // Calculate mass error range
            CalculateMassErrorRange1();

            // Only calculate MassCharge2 if not calculating
            if (calculating)
            {
                return;
            }

            calculating = true;

            MassCharge2 = massCalc.ConvoluteMass(MassCharge1, (short)MassChargeLevel1, (short)MassChargeLevel2);

            calculating = false;
        }

        private void CalculateWithMassCharge2()
        {
            // Calculate mass error range
            CalculateMassErrorRange2();

            // Only calculate MassCharge1 if not calculating
            if (calculating)
            {
                return;
            }

            calculating = true;

            MassCharge1 = massCalc.ConvoluteMass(MassCharge2, (short)MassChargeLevel2, (short)MassChargeLevel1);

            calculating = false;
        }

        private void UpdateMassErrorRanges()
        {
            CalculateMassErrorRange1();
            CalculateMassErrorRange2();
        }

        private void CalculateMassErrorRange1()
        {
            // Calculate mass error range
            ComputeMassRange(MassCharge1, out var start, out var end, MassError, MassErrorMode);
            MassCharge1Start = start;
            MassCharge1End = end;
        }

        private void CalculateMassErrorRange2()
        {
            // Calculate mass error range
            ComputeMassRange(MassCharge2, out var start, out var end, MassError, MassErrorMode);
            MassCharge2Start = start;
            MassCharge2End = end;
        }

        private static void ComputeMassRange(double mass, out double massRangeStart, out double massRangeEnd, double massError, MassErrorMode massErrorMode)
        {
            var massErrorDelta = massError;
            if (massErrorMode == MassErrorMode.Ppm)
            {
                massErrorDelta = massError * mass / 1e6;
            }

            massRangeStart = mass - massErrorDelta;
            massRangeEnd = mass + massErrorDelta;
        }
    }
}

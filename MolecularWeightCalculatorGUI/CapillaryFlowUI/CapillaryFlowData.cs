using MolecularWeightCalculator;
using ReactiveUI;

namespace MolecularWeightCalculatorGUI.CapillaryFlowUI
{
    internal class CapillaryFlowData : ReactiveObject
    {
        public CapillaryFlowData(CapillaryType capType)
        {
            capillaryType = capType;
            LoadDefaults();
            // TODO: Allow setting SampleMass for MassRate somewhere? Original has code to allow a custom entry or current formula, but no way to change the choice
        }

        private readonly CapillaryType capillaryType;
        private double backPressure;
        private UnitOfPressure backPressureUnits;
        private double volumetricFlowRate;
        private UnitOfFlowRate volumetricFlowRateUnits;
        private double columnLength;
        private UnitOfLength columnLengthUnits;
        private double linearVelocity;
        private UnitOfLinearVelocity linearVelocityUnits;
        private double columnInnerDiameter;
        private UnitOfLength columnInnerDiameterUnits;
        private double columnDeadTime;
        private UnitOfTime columnDeadTimeUnits;
        private double solventViscosity;
        private UnitOfViscosity solventViscosityUnits;
        private double columnVolume;
        private UnitOfVolume columnVolumeUnits;
        private double particleDiameter;
        private UnitOfLength particleDiameterUnits;
        private double interparticlePorosity;
        private double sampleConcentration;
        private UnitOfConcentration sampleConcentrationUnits;
        private double massFlowRate;
        private UnitOfMassFlowRate massFlowRateUnits;
        private double massRateVolumetricFlowRate;
        private UnitOfFlowRate massRateVolumetricFlowRateUnits;
        private double injectionTime;
        private UnitOfTime injectionTimeUnits;
        private double molesInjected;
        private UnitOfMolarAmount molesInjectedUnits;
        private double broadeningLinearVelocity;
        private UnitOfLinearVelocity broadeningLinearVelocityUnits;
        private double optimumLinearVelocity;
        private double diffusionCoefficient;
        private UnitOfDiffusionCoefficient diffusionCoefficientUnits;
        private double temporalVarianceSquareSeconds;
        private double openTubeLength;
        private UnitOfLength openTubeLengthUnits;
        private double additionalVarianceSquareSeconds;
        private double openTubeInnerDiameter;
        private UnitOfLength openTubeInnerDiameterUnits;
        private double resultingPeakWidth;
        private UnitOfTime resultingPeakWidthUnits;
        private double initialPeakBaseWidth;
        private UnitOfTime initialPeakBaseWidthUnits;
        private double percentIncrease;

        public double BackPressure
        {
            get => backPressure;
            set => this.RaiseAndSetIfChanged(ref backPressure, value);
        }

        public UnitOfPressure BackPressureUnits
        {
            get => backPressureUnits;
            set => this.RaiseAndSetIfChanged(ref backPressureUnits, value);
        }

        public double VolumetricFlowRate
        {
            get => volumetricFlowRate;
            set => this.RaiseAndSetIfChanged(ref volumetricFlowRate, value);
        }

        public UnitOfFlowRate VolumetricFlowRateUnits
        {
            get => volumetricFlowRateUnits;
            set => this.RaiseAndSetIfChanged(ref volumetricFlowRateUnits, value);
        }

        public double ColumnLength
        {
            get => columnLength;
            set => this.RaiseAndSetIfChanged(ref columnLength, value);
        }

        public UnitOfLength ColumnLengthUnits
        {
            get => columnLengthUnits;
            set => this.RaiseAndSetIfChanged(ref columnLengthUnits, value);
        }

        public double LinearVelocity
        {
            get => linearVelocity;
            set => this.RaiseAndSetIfChanged(ref linearVelocity, value);
        }

        public UnitOfLinearVelocity LinearVelocityUnits
        {
            get => linearVelocityUnits;
            set => this.RaiseAndSetIfChanged(ref linearVelocityUnits, value);
        }

        public double ColumnInnerDiameter
        {
            get => columnInnerDiameter;
            set => this.RaiseAndSetIfChanged(ref columnInnerDiameter, value);
        }

        public UnitOfLength ColumnInnerDiameterUnits
        {
            get => columnInnerDiameterUnits;
            set => this.RaiseAndSetIfChanged(ref columnInnerDiameterUnits, value);
        }

        public double ColumnDeadTime
        {
            get => columnDeadTime;
            set => this.RaiseAndSetIfChanged(ref columnDeadTime, value);
        }

        public UnitOfTime ColumnDeadTimeUnits
        {
            get => columnDeadTimeUnits;
            set => this.RaiseAndSetIfChanged(ref columnDeadTimeUnits, value);
        }

        public double SolventViscosity
        {
            get => solventViscosity;
            set => this.RaiseAndSetIfChanged(ref solventViscosity, value);
        }

        public UnitOfViscosity SolventViscosityUnits
        {
            get => solventViscosityUnits;
            set => this.RaiseAndSetIfChanged(ref solventViscosityUnits, value);
        }

        public double ColumnVolume
        {
            get => columnVolume;
            set => this.RaiseAndSetIfChanged(ref columnVolume, value);
        }

        public UnitOfVolume ColumnVolumeUnits
        {
            get => columnVolumeUnits;
            set => this.RaiseAndSetIfChanged(ref columnVolumeUnits, value);
        }

        public double ParticleDiameter
        {
            get => particleDiameter;
            set => this.RaiseAndSetIfChanged(ref particleDiameter, value);
        }

        public UnitOfLength ParticleDiameterUnits
        {
            get => particleDiameterUnits;
            set => this.RaiseAndSetIfChanged(ref particleDiameterUnits, value);
        }

        public double InterparticlePorosity
        {
            get => interparticlePorosity;
            set => this.RaiseAndSetIfChanged(ref interparticlePorosity, value);
        }

        public double SampleConcentration
        {
            get => sampleConcentration;
            set => this.RaiseAndSetIfChanged(ref sampleConcentration, value);
        }

        public UnitOfConcentration SampleConcentrationUnits
        {
            get => sampleConcentrationUnits;
            set => this.RaiseAndSetIfChanged(ref sampleConcentrationUnits, value);
        }

        public double MassFlowRate
        {
            get => massFlowRate;
            set => this.RaiseAndSetIfChanged(ref massFlowRate, value);
        }

        public UnitOfMassFlowRate MassFlowRateUnits
        {
            get => massFlowRateUnits;
            set => this.RaiseAndSetIfChanged(ref massFlowRateUnits, value);
        }

        public double MassRateVolumetricFlowRate
        {
            get => massRateVolumetricFlowRate;
            set => this.RaiseAndSetIfChanged(ref massRateVolumetricFlowRate, value);
        }

        public UnitOfFlowRate MassRateVolumetricFlowRateUnits
        {
            get => massRateVolumetricFlowRateUnits;
            set => this.RaiseAndSetIfChanged(ref massRateVolumetricFlowRateUnits, value);
        }

        public double InjectionTime
        {
            get => injectionTime;
            set => this.RaiseAndSetIfChanged(ref injectionTime, value);
        }

        public UnitOfTime InjectionTimeUnits
        {
            get => injectionTimeUnits;
            set => this.RaiseAndSetIfChanged(ref injectionTimeUnits, value);
        }

        public double MolesInjected
        {
            get => molesInjected;
            set => this.RaiseAndSetIfChanged(ref molesInjected, value);
        }

        public UnitOfMolarAmount MolesInjectedUnits
        {
            get => molesInjectedUnits;
            set => this.RaiseAndSetIfChanged(ref molesInjectedUnits, value);
        }

        public double BroadeningLinearVelocity
        {
            get => broadeningLinearVelocity;
            set => this.RaiseAndSetIfChanged(ref broadeningLinearVelocity, value);
        }

        public UnitOfLinearVelocity BroadeningLinearVelocityUnits
        {
            get => broadeningLinearVelocityUnits;
            set => this.RaiseAndSetIfChanged(ref broadeningLinearVelocityUnits, value);
        }

        public double OptimumLinearVelocity
        {
            get => optimumLinearVelocity;
            set => this.RaiseAndSetIfChanged(ref optimumLinearVelocity, value);
        }

        public double DiffusionCoefficient
        {
            get => diffusionCoefficient;
            set => this.RaiseAndSetIfChanged(ref diffusionCoefficient, value);
        }

        public UnitOfDiffusionCoefficient DiffusionCoefficientUnits
        {
            get => diffusionCoefficientUnits;
            set => this.RaiseAndSetIfChanged(ref diffusionCoefficientUnits, value);
        }

        public double TemporalVarianceSquareSeconds
        {
            get => temporalVarianceSquareSeconds;
            set => this.RaiseAndSetIfChanged(ref temporalVarianceSquareSeconds, value);
        }

        public double OpenTubeLength
        {
            get => openTubeLength;
            set => this.RaiseAndSetIfChanged(ref openTubeLength, value);
        }

        public UnitOfLength OpenTubeLengthUnits
        {
            get => openTubeLengthUnits;
            set => this.RaiseAndSetIfChanged(ref openTubeLengthUnits, value);
        }

        public double AdditionalVarianceSquareSeconds
        {
            get => additionalVarianceSquareSeconds;
            set => this.RaiseAndSetIfChanged(ref additionalVarianceSquareSeconds, value);
        }

        public double OpenTubeInnerDiameter
        {
            get => openTubeInnerDiameter;
            set => this.RaiseAndSetIfChanged(ref openTubeInnerDiameter, value);
        }

        public UnitOfLength OpenTubeInnerDiameterUnits
        {
            get => openTubeInnerDiameterUnits;
            set => this.RaiseAndSetIfChanged(ref openTubeInnerDiameterUnits, value);
        }

        public double ResultingPeakWidth
        {
            get => resultingPeakWidth;
            set => this.RaiseAndSetIfChanged(ref resultingPeakWidth, value);
        }

        public UnitOfTime ResultingPeakWidthUnits
        {
            get => resultingPeakWidthUnits;
            set => this.RaiseAndSetIfChanged(ref resultingPeakWidthUnits, value);
        }

        public double InitialPeakBaseWidth
        {
            get => initialPeakBaseWidth;
            set => this.RaiseAndSetIfChanged(ref initialPeakBaseWidth, value);
        }

        public UnitOfTime InitialPeakBaseWidthUnits
        {
            get => initialPeakBaseWidthUnits;
            set => this.RaiseAndSetIfChanged(ref initialPeakBaseWidthUnits, value);
        }

        public double PercentIncrease
        {
            get => percentIncrease;
            set => this.RaiseAndSetIfChanged(ref percentIncrease, value);
        }

        public void LoadDefaults()
        {
            if (capillaryType == CapillaryType.OpenTubularCapillary)
            {
                BackPressure = 50;
                ColumnInnerDiameter = 30;

                // default calculated values
                VolumetricFlowRate = 924.070551;
                LinearVelocity = 2.1788;
                ColumnDeadTime = 22.948194;
                ColumnVolume = 353.4292;

                MassFlowRate = 15.4012;
                MolesInjected = 462.0353;

                TemporalVarianceSquareSeconds = 0.1195;
                ResultingPeakWidth = 30.0319;
                PercentIncrease = 0.1;
            }
            else
            {
                // Most of the packed capillary values are identical to the open capillary values
                BackPressure = 3000;
                ColumnInnerDiameter = 75;

                // default calculated values
                VolumetricFlowRate = 304.220758;
                LinearVelocity = 0.2869;
                ColumnDeadTime = 174.262849;
                ColumnVolume = 883.5729;

                MassFlowRate = 5.0703;
                MolesInjected = 152.1104;

                TemporalVarianceSquareSeconds = 0.9077;
                ResultingPeakWidth = 30.2411;
                PercentIncrease = 0.8;
            }

            BackPressureUnits = UnitOfPressure.Psi;
            ColumnInnerDiameterUnits = UnitOfLength.Microns;
            VolumetricFlowRateUnits = UnitOfFlowRate.NLPerMin;
            ColumnLength = 50;
            ColumnLengthUnits = UnitOfLength.CM;
            LinearVelocityUnits = UnitOfLinearVelocity.CmPerSec;
            ColumnDeadTimeUnits = UnitOfTime.Seconds;
            SolventViscosity = 0.0089;
            SolventViscosityUnits = UnitOfViscosity.Poise;
            ColumnVolumeUnits = UnitOfVolume.NL;
            ParticleDiameter = 5;
            ParticleDiameterUnits = UnitOfLength.Microns;
            InterparticlePorosity = 0.4;

            SampleConcentration = 1;
            SampleConcentrationUnits = UnitOfConcentration.MicroMolar;
            MassFlowRateUnits = UnitOfMassFlowRate.FmolPerSec;
            MassRateVolumetricFlowRate = VolumetricFlowRate;
            MassRateVolumetricFlowRateUnits = UnitOfFlowRate.NLPerMin;
            InjectionTime = 30;
            InjectionTimeUnits = UnitOfTime.Seconds;
            MolesInjectedUnits = UnitOfMolarAmount.FemtoMoles;

            BroadeningLinearVelocity = LinearVelocity;
            BroadeningLinearVelocityUnits = UnitOfLinearVelocity.CmPerSec;
            OptimumLinearVelocity = 0.03;
            DiffusionCoefficient = 0.000005;
            DiffusionCoefficientUnits = UnitOfDiffusionCoefficient.CmSquaredPerSec;
            OpenTubeLength = 5;
            OpenTubeLengthUnits = UnitOfLength.CM;
            AdditionalVarianceSquareSeconds = 0;
            OpenTubeInnerDiameter = 50;
            OpenTubeInnerDiameterUnits = UnitOfLength.Microns;
            ResultingPeakWidthUnits = UnitOfTime.Seconds;
            InitialPeakBaseWidth = 30;
            InitialPeakBaseWidthUnits = UnitOfTime.Seconds;
            PercentIncrease = 0.8;
        }
    }
}

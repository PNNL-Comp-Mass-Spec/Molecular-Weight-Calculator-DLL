using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using MolecularWeightCalculator;
using MolecularWeightCalculator.CapillaryFlowTools;
using ReactiveUI;
using CapillaryFlow = MolecularWeightCalculator.CapillaryFlowTools.CapillaryFlow;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.CapillaryFlowUI
{
    // TODO: The VB6 GUI apparently keeps 2 sets of backing data objects, one for open capillary and one for packed
    internal class CapillaryFlowViewModel : ReactiveObject
    {
        public CapillaryFlowViewModel()
        {
            CapillaryTypeOptions = Enum.GetValues(typeof(CapillaryType)).Cast<CapillaryType>().ToList();
            CalculationModeOptions = Enum.GetValues(typeof(AutoComputeMode)).Cast<AutoComputeMode>().Where(x => x != AutoComputeMode.DeadTime && x != AutoComputeMode.LinearVelocity).ToList();
            PressureUnitOptions = Enum.GetValues(typeof(UnitOfPressure)).Cast<UnitOfPressure>().ToList();
            FlowRateUnitOptions = Enum.GetValues(typeof(UnitOfFlowRate)).Cast<UnitOfFlowRate>().ToList();
            LengthUnitOptions = Enum.GetValues(typeof(UnitOfLength)).Cast<UnitOfLength>().ToList();
            LinearVelocityUnitOptions = Enum.GetValues(typeof(UnitOfLinearVelocity)).Cast<UnitOfLinearVelocity>().ToList();
            TimeUnitOptions = Enum.GetValues(typeof(UnitOfTime)).Cast<UnitOfTime>().ToList();
            ViscosityUnitOptions = Enum.GetValues(typeof(UnitOfViscosity)).Cast<UnitOfViscosity>().ToList();
            VolumeUnitOptions = Enum.GetValues(typeof(UnitOfVolume)).Cast<UnitOfVolume>().ToList();
            ConcentrationUnitOptions = Enum.GetValues(typeof(UnitOfConcentration)).Cast<UnitOfConcentration>().ToList();
            MassFlowRateUnitOptions = Enum.GetValues(typeof(UnitOfMassFlowRate)).Cast<UnitOfMassFlowRate>().ToList();
            MolarAmountUnitOptions = Enum.GetValues(typeof(UnitOfMolarAmount)).Cast<UnitOfMolarAmount>().ToList();

            // TODO: Allow setting this somewhere? Original has code to allow a custom entry or current formula, but no way to change the choice
            massRate.SetSampleMass(100);

            backPressureReadOnly = this.WhenAnyValue(x => x.CalculationMode).ObserveOn(RxApp.MainThreadScheduler).Select(x => x == AutoComputeMode.VolFlowRateUsingDeadTime || x == AutoComputeMode.BackPressure).ToProperty(this, x => x.BackPressureReadOnly);
            volumetricFlowRateReadOnly = this.WhenAnyValue(x => x.CalculationMode).ObserveOn(RxApp.MainThreadScheduler).Select(x => x == AutoComputeMode.VolFlowRate || x == AutoComputeMode.VolFlowRateUsingDeadTime).ToProperty(this, x => x.VolumetricFlowRateReadOnly);
            columnLengthReadOnly = this.WhenAnyValue(x => x.CalculationMode).ObserveOn(RxApp.MainThreadScheduler).Select(x => x == AutoComputeMode.ColumnLength).ToProperty(this, x => x.ColumnLengthReadOnly);
            columnInnerDiameterReadOnly = this.WhenAnyValue(x => x.CalculationMode).ObserveOn(RxApp.MainThreadScheduler).Select(x => x == AutoComputeMode.ColumnId).ToProperty(this, x => x.ColumnInnerDiameterReadOnly);
            columnDeadTimeReadOnly = this.WhenAnyValue(x => x.CalculationMode).ObserveOn(RxApp.MainThreadScheduler).Select(x => x == AutoComputeMode.VolFlowRate || x == AutoComputeMode.ColumnId || x == AutoComputeMode.ColumnLength || x == AutoComputeMode.BackPressure).ToProperty(this, x => x.ColumnDeadTimeReadOnly);
            inPackedCapillaryMode = this.WhenAnyValue(x => x.CapillaryType).ObserveOn(RxApp.MainThreadScheduler).Select(x => x == CapillaryType.PackedCapillary).ToProperty(this, x => x.InPackedCapillaryMode);
            hideExtraPeakBroadeningCalculations = this.WhenAnyValue(x => x.ShowExtraPeakBroadeningCalculations).ObserveOn(RxApp.MainThreadScheduler).Select(x => !x).ToProperty(this, x => x.HideExtraPeakBroadeningCalculations);

            this.WhenAnyValue(x => x.CapillaryType).Subscribe(x => SetCapillaryType());
            this.WhenAnyValue(x => x.CalculationMode).Subscribe(x => capFlow.SetAutoComputeMode(x));

            this.WhenAnyValue(x => x.Data.BackPressure, x => x.Data.BackPressureUnits).Subscribe(x => SetBackPressure());
            this.WhenAnyValue(x => x.Data.VolumetricFlowRate, x => x.Data.VolumetricFlowRateUnits).Subscribe(x => SetVolumetricFlowRate());
            this.WhenAnyValue(x => x.Data.ColumnLength, x => x.Data.ColumnLengthUnits).Subscribe(x => SetColumnLength());
            this.WhenAnyValue(x => x.Data.ColumnInnerDiameter).Subscribe(x => SetColumnInnerDiameter());
            this.WhenAnyValue(x => x.Data.ColumnInnerDiameterUnits).Subscribe(x => Data.ColumnInnerDiameter = capFlow.GetColumnInnerDiameter(x));
            this.WhenAnyValue(x => x.Data.ColumnDeadTime, x => x.Data.ColumnDeadTimeUnits).Subscribe(x => SetColumnDeadTime());
            this.WhenAnyValue(x => x.Data.SolventViscosity, x => x.Data.SolventViscosityUnits).Subscribe(x => SetSolventViscosity());
            this.WhenAnyValue(x => x.Data.ParticleDiameter, x => x.Data.ParticleDiameterUnits).Subscribe(x => SetParticleDiameter());
            this.WhenAnyValue(x => x.Data.InterparticlePorosity).Subscribe(x => SetInterparticlePorosity());
            this.WhenAnyValue(x => x.Data.LinearVelocityUnits, x => x.Data.ColumnVolumeUnits).Subscribe(x => ReadCapillaryFlowValues());

            this.WhenAnyValue(x => x.Data.SampleConcentration, x => x.Data.SampleConcentrationUnits).Subscribe(x => SetMassRateSampleConcentration());
            this.WhenAnyValue(x => x.Data.MassRateVolumetricFlowRate, x => x.Data.MassRateVolumetricFlowRateUnits).Subscribe(x => SetMassRateVolumetricFlowRate());
            this.WhenAnyValue(x => x.Data.InjectionTime, x => x.Data.InjectionTimeUnits).Subscribe(x => SetMassRateInjectionTime());
            this.WhenAnyValue(x => x.Data.MassFlowRateUnits, x => x.Data.MolesInjectedUnits).Subscribe(x => ReadMassRateValues());

            this.WhenAnyValue(x => x.Data.BroadeningLinearVelocity, x => x.Data.BroadeningLinearVelocityUnits).Subscribe(x => SetBroadeningLinearVelocity());
            this.WhenAnyValue(x => x.Data.DiffusionCoefficient, x => x.Data.DiffusionCoefficientUnits).Subscribe(x => SetDiffusionCoefficient());
            this.WhenAnyValue(x => x.Data.OpenTubeLength, x => x.Data.OpenTubeLengthUnits).Subscribe(x => SetOpenTubeLength());
            this.WhenAnyValue(x => x.Data.OpenTubeInnerDiameter, x => x.Data.OpenTubeInnerDiameterUnits).Subscribe(x => SetOpenTubeInnerDiameter());
            this.WhenAnyValue(x => x.Data.AdditionalVarianceSquareSeconds).Subscribe(x => SetAdditionalVarianceSquareSeconds());
            this.WhenAnyValue(x => x.Data.InitialPeakBaseWidth, x => x.Data.InitialPeakBaseWidth).Subscribe(x => SetInitialPeakBaseWidth());
            this.WhenAnyValue(x => x.Data.ResultingPeakWidthUnits).Subscribe(x => Data.ResultingPeakWidth = extraCol.GetResultantPeakWidth(x));

            TogglePeakBroadeningCalculationsCommand = ReactiveCommand.Create(() => ShowExtraPeakBroadeningCalculations = !ShowExtraPeakBroadeningCalculations);
            ShowExplanatoryEquationsCommand = ReactiveCommand.Create(() => ShowExplanatoryEquations());
            ShowComputeWaterViscosityCommand = ReactiveCommand.Create(() => ShowComputeWaterViscosity());
            ShowPeakBroadeningEquationsCommand = ReactiveCommand.Create(() => ShowPeakBroadeningEquations());

            LoadDefaults();
        }

        private CapillaryType capillaryType = CapillaryType.PackedCapillary;
        private AutoComputeMode calculationMode = AutoComputeMode.VolFlowRate;
        private readonly ObservableAsPropertyHelper<bool> backPressureReadOnly;
        private readonly ObservableAsPropertyHelper<bool> volumetricFlowRateReadOnly;
        private readonly ObservableAsPropertyHelper<bool> columnLengthReadOnly;
        private readonly ObservableAsPropertyHelper<bool> columnInnerDiameterReadOnly;
        private readonly ObservableAsPropertyHelper<bool> columnDeadTimeReadOnly;
        private readonly ObservableAsPropertyHelper<bool> inPackedCapillaryMode;
        private bool showExtraPeakBroadeningCalculations = false;
        private readonly ObservableAsPropertyHelper<bool> hideExtraPeakBroadeningCalculations;
        private bool linkVolumetricFlowRate = true;
        private bool linkLinearVelocity = true;
        private readonly CapillaryFlow capFlow = new CapillaryFlow();
        private readonly MassRate massRate = new MassRate();
        private readonly ExtraColumnBroadening extraCol = new ExtraColumnBroadening();
        private readonly CapillaryFlowData openCapillaryData = new CapillaryFlowData(CapillaryType.OpenTubularCapillary);
        private readonly CapillaryFlowData packedCapillaryData = new CapillaryFlowData(CapillaryType.PackedCapillary);
        private CapillaryFlowData data;

        public IReadOnlyList<CapillaryType> CapillaryTypeOptions { get; }
        public IReadOnlyList<AutoComputeMode> CalculationModeOptions { get; }
        public IReadOnlyList<UnitOfPressure> PressureUnitOptions { get; }
        public IReadOnlyList<UnitOfFlowRate> FlowRateUnitOptions { get; }
        public IReadOnlyList<UnitOfLength> LengthUnitOptions { get; }
        public IReadOnlyList<UnitOfLinearVelocity> LinearVelocityUnitOptions { get; }
        public IReadOnlyList<UnitOfTime> TimeUnitOptions { get; }
        public IReadOnlyList<UnitOfViscosity> ViscosityUnitOptions { get; }
        public IReadOnlyList<UnitOfVolume> VolumeUnitOptions { get; }
        public IReadOnlyList<UnitOfConcentration> ConcentrationUnitOptions { get; }
        public IReadOnlyList<UnitOfMassFlowRate> MassFlowRateUnitOptions { get; }
        public IReadOnlyList<UnitOfMolarAmount> MolarAmountUnitOptions { get; }

        public ReactiveCommand<RxUnit, bool> TogglePeakBroadeningCalculationsCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> ShowExplanatoryEquationsCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> ShowComputeWaterViscosityCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> ShowPeakBroadeningEquationsCommand { get; }

        public CapillaryType CapillaryType
        {
            get => capillaryType;
            set => this.RaiseAndSetIfChanged(ref capillaryType, value);
        }

        public AutoComputeMode CalculationMode
        {
            get => calculationMode;
            set => this.RaiseAndSetIfChanged(ref calculationMode, value);
        }

        public CapillaryFlowData Data
        {
            get => data;
            private set => this.RaiseAndSetIfChanged(ref data, value);
        }

        public bool BackPressureReadOnly => backPressureReadOnly.Value;
        public bool VolumetricFlowRateReadOnly => volumetricFlowRateReadOnly.Value;
        public bool ColumnLengthReadOnly => columnLengthReadOnly.Value;
        public bool ColumnInnerDiameterReadOnly => columnInnerDiameterReadOnly.Value;
        public bool ColumnDeadTimeReadOnly => columnDeadTimeReadOnly.Value;
        public bool InPackedCapillaryMode => inPackedCapillaryMode.Value;

        public bool ShowExtraPeakBroadeningCalculations
        {
            get => showExtraPeakBroadeningCalculations;
            private set => this.RaiseAndSetIfChanged(ref showExtraPeakBroadeningCalculations, value);
        }

        public bool HideExtraPeakBroadeningCalculations => hideExtraPeakBroadeningCalculations.Value;

        public bool LinkVolumetricFlowRate
        {
            get => linkVolumetricFlowRate;
            set => this.RaiseAndSetIfChanged(ref linkVolumetricFlowRate, value);
        }

        public bool LinkLinearVelocity
        {
            get => linkLinearVelocity;
            set => this.RaiseAndSetIfChanged(ref linkLinearVelocity, value);
        }

        private void LoadDefaults()
        {
            capFlow.SetAutoComputeEnabled(false);
            CalculationMode = AutoComputeMode.VolFlowRate;
            LinkVolumetricFlowRate = true;
            LinkLinearVelocity = true;
            ShowExtraPeakBroadeningCalculations = false;

            openCapillaryData.LoadDefaults();
            packedCapillaryData.LoadDefaults();
            capFlow.SetAutoComputeEnabled(true);
            ReadCapillaryFlowValues();
        }

        private void SetCapillaryType()
        {
            capFlow.SetCapillaryType(CapillaryType);
            if (CapillaryType == CapillaryType.OpenTubularCapillary)
                Data = openCapillaryData;
            else
                Data = packedCapillaryData;

            ReadCapillaryFlowValues();
        }

        private void SetBackPressure()
        {
            if (CalculationMode != AutoComputeMode.BackPressure && CalculationMode != AutoComputeMode.VolFlowRateUsingDeadTime)
            {
                capFlow.SetBackPressure(Data.BackPressure, Data.BackPressureUnits);
                ReadCapillaryFlowValues();
            }
            else
            {
                Data.BackPressure = capFlow.GetBackPressure(Data.BackPressureUnits);
            }
        }

        private void SetVolumetricFlowRate()
        {
            if (CalculationMode != AutoComputeMode.VolFlowRate && CalculationMode != AutoComputeMode.VolFlowRateUsingDeadTime)
            {
                capFlow.SetVolumetricFlowRate(Data.VolumetricFlowRate, Data.VolumetricFlowRateUnits);
                ReadCapillaryFlowValues();
            }
            else
            {
                Data.VolumetricFlowRate = capFlow.GetVolumetricFlowRate(Data.VolumetricFlowRateUnits);
            }
        }

        private void SetColumnLength()
        {
            if (CalculationMode != AutoComputeMode.ColumnLength)
            {
                capFlow.SetColumnLength(Data.ColumnLength, Data.ColumnLengthUnits);
                ReadCapillaryFlowValues();
            }
            else
            {
                Data.ColumnLength = capFlow.GetColumnLength(Data.ColumnLengthUnits);
            }
        }

        private void SetColumnInnerDiameter()
        {
            if (CalculationMode != AutoComputeMode.ColumnId)
            {
                capFlow.SetColumnInnerDiameter(Data.ColumnInnerDiameter, Data.ColumnInnerDiameterUnits);
                ReadCapillaryFlowValues();
            }
        }

        private void SetColumnDeadTime()
        {
            if (CalculationMode == AutoComputeMode.DeadTime)
            {
                capFlow.SetDeadTime(Data.ColumnDeadTime, Data.ColumnDeadTimeUnits);
                ReadCapillaryFlowValues();
            }
            else
            {
                Data.ColumnDeadTime = capFlow.GetDeadTime(Data.ColumnDeadTimeUnits);
            }
        }

        private void SetSolventViscosity()
        {
            capFlow.SetSolventViscosity(Data.SolventViscosity, Data.SolventViscosityUnits);
            ReadCapillaryFlowValues();
        }

        private void SetParticleDiameter()
        {
            capFlow.SetParticleDiameter(Data.ParticleDiameter, Data.ParticleDiameterUnits);
            ReadCapillaryFlowValues();
        }

        private void SetInterparticlePorosity()
        {
            capFlow.SetInterparticlePorosity(Data.InterparticlePorosity);
            ReadCapillaryFlowValues();
        }

        private void ReadCapillaryFlowValues()
        {
            switch (CalculationMode)
            {
                case AutoComputeMode.BackPressure:
                    Data.BackPressure = capFlow.GetBackPressure(Data.BackPressureUnits);
                    Data.ColumnDeadTime = capFlow.GetDeadTime(Data.ColumnDeadTimeUnits);
                    break;
                case AutoComputeMode.ColumnLength:
                    Data.ColumnLength = capFlow.GetColumnLength(Data.ColumnLengthUnits);
                    Data.ColumnDeadTime = capFlow.GetDeadTime(Data.ColumnDeadTimeUnits);
                    break;
                case AutoComputeMode.ColumnId:
                    Data.ColumnInnerDiameter = capFlow.GetColumnInnerDiameter(Data.ColumnInnerDiameterUnits);
                    Data.ColumnDeadTime = capFlow.GetDeadTime(Data.ColumnDeadTimeUnits);
                    break;
                case AutoComputeMode.VolFlowRate:
                    Data.VolumetricFlowRate = capFlow.GetVolumetricFlowRate(Data.VolumetricFlowRateUnits);
                    Data.ColumnDeadTime = capFlow.GetDeadTime(Data.ColumnDeadTimeUnits);
                    break;
                case AutoComputeMode.VolFlowRateUsingDeadTime:
                    Data.BackPressure = capFlow.GetBackPressure(Data.BackPressureUnits);
                    Data.VolumetricFlowRate = capFlow.GetVolumetricFlowRate(Data.VolumetricFlowRateUnits);
                    break;
            }

            Data.LinearVelocity = capFlow.GetLinearVelocity(Data.LinearVelocityUnits);
            Data.ColumnVolume = capFlow.GetColumnVolume(Data.ColumnVolumeUnits);

            if (LinkVolumetricFlowRate)
            {
                Data.MassRateVolumetricFlowRate = Data.VolumetricFlowRate;
                Data.MassRateVolumetricFlowRateUnits = Data.VolumetricFlowRateUnits;
                // Other values should auto-update from just setting the above (when needed)
            }

            if (LinkLinearVelocity)
            {
                Data.BroadeningLinearVelocity = Data.LinearVelocity;
                Data.BroadeningLinearVelocityUnits = Data.LinearVelocityUnits;
                // Other values should auto-update from just setting the above (when needed)
            }
        }

        private void SetMassRateSampleConcentration()
        {
            massRate.SetConcentration(Data.SampleConcentration, Data.SampleConcentrationUnits);
            ReadMassRateValues();
        }

        private void SetMassRateVolumetricFlowRate()
        {
            massRate.SetVolumetricFlowRate(Data.MassRateVolumetricFlowRate, Data.MassRateVolumetricFlowRateUnits);
            ReadMassRateValues();
        }

        private void SetMassRateInjectionTime()
        {
            massRate.SetInjectionTime(Data.InjectionTime, Data.InjectionTimeUnits);
            ReadMassRateValues();
        }

        private void ReadMassRateValues()
        {
            Data.MassFlowRate = massRate.GetMassFlowRate(Data.MassFlowRateUnits);
            Data.MolesInjected = massRate.GetMolesInjected(Data.MolesInjectedUnits);
        }

        private void SetBroadeningLinearVelocity()
        {
            extraCol.SetLinearVelocity(Data.BroadeningLinearVelocity, Data.BroadeningLinearVelocityUnits);
            ReadExtraColumnBroadeningValues();
        }

        private void SetDiffusionCoefficient()
        {
            extraCol.SetDiffusionCoefficient(Data.DiffusionCoefficient, Data.DiffusionCoefficientUnits);
            ReadExtraColumnBroadeningValues();
        }

        private void SetOpenTubeLength()
        {
            extraCol.SetOpenTubeLength(Data.OpenTubeLength, Data.OpenTubeLengthUnits);
            ReadExtraColumnBroadeningValues();
        }

        private void SetOpenTubeInnerDiameter()
        {
            extraCol.SetOpenTubeInnerDiameter(Data.OpenTubeInnerDiameter, Data.OpenTubeInnerDiameterUnits);
            ReadExtraColumnBroadeningValues();
        }

        private void SetAdditionalVarianceSquareSeconds()
        {
            extraCol.SetAdditionalVariance(Data.AdditionalVarianceSquareSeconds);
            ReadExtraColumnBroadeningValues();
        }

        private void SetInitialPeakBaseWidth()
        {
            extraCol.SetInitialPeakWidthAtBase(Data.InitialPeakBaseWidth, Data.InitialPeakBaseWidthUnits);
            ReadExtraColumnBroadeningValues();
        }

        private void ReadExtraColumnBroadeningValues()
        {
            Data.OptimumLinearVelocity = extraCol.ComputeOptimumLinearVelocityUsingParticleDiamAndDiffusionCoeff(capFlow.GetParticleDiameter(UnitOfLength.CM), Data.LinearVelocityUnits);
            Data.TemporalVarianceSquareSeconds = extraCol.GetTemporalVarianceInSquareSeconds();
            Data.ResultingPeakWidth = extraCol.GetResultantPeakWidth(Data.ResultingPeakWidthUnits);
            Data.PercentIncrease = (extraCol.GetResultantPeakWidth(Data.InitialPeakBaseWidthUnits) - Data.InitialPeakBaseWidth) / Data.InitialPeakBaseWidth * 100;
        }

        private void ShowExplanatoryEquations()
        {
            var window = new ExplanatoryEquationsWindow
            {
                DataContext = new EquationsViewModel(CapillaryType)
            };
            window.ShowDialog();
        }

        private void ShowComputeWaterViscosity()
        {
            var window = new MeCNViscosityWindow
            {
                // TODO: cache this to avoid resetting values whenever closed?
                DataContext = new MeCNViscosityViewModel(this)
            };
            window.Show();
        }

        private void ShowPeakBroadeningEquations()
        {
            var window = new BroadeningEquationsWindow
            {
                DataContext = new EquationsViewModel(CapillaryType)
            };
            window.ShowDialog();
        }
    }
}

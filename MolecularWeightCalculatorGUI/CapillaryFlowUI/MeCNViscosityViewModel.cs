using System;
using System.Collections.Generic;
using System.Linq;
using MolecularWeightCalculator;
using ReactiveUI;
using RxUnit = System.Reactive.Unit;

namespace MolecularWeightCalculatorGUI.CapillaryFlowUI
{
    internal class MeCNViscosityViewModel : ReactiveObject
    {
        [Obsolete("For WPF design-time use only", true)]
        public MeCNViscosityViewModel() : this(new CapillaryFlowViewModel())
        {
        }

        public MeCNViscosityViewModel(CapillaryFlowViewModel capillaryFlow)
        {
            capillaryFlowVm = capillaryFlow;
            TemperatureUnitOptions = Enum.GetValues(typeof(UnitOfTemperature)).Cast<UnitOfTemperature>().ToList();
            ViscosityUnitOptions = capillaryFlowVm.ViscosityUnitOptions;

            SetDefaults();
            Calculate();

            this.WhenAnyValue(x => x.PercentAcetonitrile, x => x.Temperature, x => x.TemperatureUnits,
                x => x.ViscosityUnits).Subscribe(x => Calculate());

            CopyMeCNViscosityToCapillaryFlowCommand = ReactiveCommand.Create(CopyMeCNViscosityToCapillaryFlow);
            //ShowViscosityPlotCommand = ReactiveCommand.Create();
            SetDefaultsCommand = ReactiveCommand.Create(SetDefaults);
        }

        private readonly CapillaryFlow capFlow = new CapillaryFlow();
        private readonly CapillaryFlowViewModel capillaryFlowVm;
        private double percentAcetonitrile;
        private double temperature;
        private UnitOfTemperature temperatureUnits;
        private double solventViscosity;
        private UnitOfViscosity viscosityUnits;

        public IReadOnlyList<UnitOfTemperature> TemperatureUnitOptions { get; }
        public IReadOnlyList<UnitOfViscosity> ViscosityUnitOptions { get; }

        // Version of the equation that works properly in a full LaTeX processor, but doesn't work with WPFMath.
        //public string ChenHorvathEquation { get; } = @"Viscosity=exp$\left[\phi\left(-3.476+{\displaystyle\frac{726}{T}}\right)+(1-\phi)\left(-5.414+{\displaystyle\frac{1566}{T}}\right)+\phi(1-\phi)\left(-1.762+{\displaystyle\frac{929}{T}}\right)\right]$";
        public string ChenHorvathEquation { get; } = @"\mathrm{Viscosity=exp}\left[\phi\left(-3.476+{\frac{726}{T}}\right)+" +
                                                     @"(1-\phi)\left(-5.414+{\frac{1566}{T}}\right)+" +
                                                     @"\phi(1-\phi)\left(-1.762+{\frac{929}{T}}\right)\right]";

        public ReactiveCommand<RxUnit, RxUnit> CopyMeCNViscosityToCapillaryFlowCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> ShowViscosityPlotCommand { get; }
        public ReactiveCommand<RxUnit, RxUnit> SetDefaultsCommand { get; }

        public double PercentAcetonitrile
        {
            get => percentAcetonitrile;
            set => this.RaiseAndSetIfChanged(ref percentAcetonitrile, value);
        }

        public double Temperature
        {
            get => temperature;
            set => this.RaiseAndSetIfChanged(ref temperature, value);
        }

        public UnitOfTemperature TemperatureUnits
        {
            get => temperatureUnits;
            set => this.RaiseAndSetIfChanged(ref temperatureUnits, value);
        }

        public double SolventViscosity
        {
            get => solventViscosity;
            private set => this.RaiseAndSetIfChanged(ref solventViscosity, value);
        }

        public UnitOfViscosity ViscosityUnits
        {
            get => viscosityUnits;
            set => this.RaiseAndSetIfChanged(ref viscosityUnits, value);
        }

        public void SetDefaults()
        {
            PercentAcetonitrile = 20;
            Temperature = 25;
            TemperatureUnits = UnitOfTemperature.Celsius;
            SolventViscosity = 0.008885;
            ViscosityUnits = UnitOfViscosity.Poise;
        }

        private void CopyMeCNViscosityToCapillaryFlow()
        {
            capillaryFlowVm.Data.SolventViscosity = SolventViscosity;
            capillaryFlowVm.Data.SolventViscosityUnits = ViscosityUnits;
        }

        private void Calculate()
        {
            SolventViscosity = capFlow.ComputeMeCNViscosity(PercentAcetonitrile, Temperature, TemperatureUnits, ViscosityUnits);
        }
    }
}

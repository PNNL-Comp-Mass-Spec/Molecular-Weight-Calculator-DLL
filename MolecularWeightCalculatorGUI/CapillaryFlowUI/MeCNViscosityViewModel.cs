using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DynamicData.Binding;
using MolecularWeightCalculator;
using MolecularWeightCalculatorGUI.Utilities;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
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
            CreatePlot();
            Calculate();

            this.WhenAnyValue(x => x.PercentAcetonitrile, x => x.Temperature, x => x.TemperatureUnits,
                x => x.ViscosityUnits).Subscribe(x => Calculate());

            CopyMeCNViscosityToCapillaryFlowCommand = ReactiveCommand.Create(CopyMeCNViscosityToCapillaryFlow);
            //ShowViscosityPlotCommand = ReactiveCommand.Create(ShowViscosityPlot);
            SetDefaultsCommand = ReactiveCommand.Create(SetDefaults);
        }

        private readonly CapillaryFlow capFlow = new CapillaryFlow();
        private readonly CapillaryFlowViewModel capillaryFlowVm;
        private double percentAcetonitrile;
        private double temperature;
        private UnitOfTemperature temperatureUnits;
        private double solventViscosity;
        private UnitOfViscosity viscosityUnits;
        private PlotModel viscosityPlot = null;
        private LinearAxis viscosityPlotYAxis = null;

        public IReadOnlyList<UnitOfTemperature> TemperatureUnitOptions { get; }
        public IReadOnlyList<UnitOfViscosity> ViscosityUnitOptions { get; }

        // Version of the equation that works properly in a full LaTeX processor, but doesn't work with WPFMath.
        //public string ChenHorvathEquation { get; } = @"Viscosity=exp$\left[\phi\left(-3.476+{\displaystyle\frac{726}{T}}\right)+(1-\phi)\left(-5.414+{\displaystyle\frac{1566}{T}}\right)+\phi(1-\phi)\left(-1.762+{\displaystyle\frac{929}{T}}\right)\right]$";
        public string ChenHorvathEquation { get; } = @"\mathrm{Viscosity=exp}\left[\phi\left(-3.476+{\frac{726}{T}}\right)+" +
                                                     @"(1-\phi)\left(-5.414+{\frac{1566}{T}}\right)+" +
                                                     @"\phi(1-\phi)\left(-1.762+{\frac{929}{T}}\right)\right]";

        public ReactiveCommand<RxUnit, RxUnit> CopyMeCNViscosityToCapillaryFlowCommand { get; }
        //public ReactiveCommand<RxUnit, RxUnit> ShowViscosityPlotCommand { get; }
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

        public ObservableCollectionExtended<GraphPoint> PlotPoints { get; } = new ObservableCollectionExtended<GraphPoint>();

        public PlotModel ViscosityPlot
        {
            get => viscosityPlot;
            set => this.RaiseAndSetIfChanged(ref viscosityPlot, value);
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
        //private void ShowViscosityPlot()
        //{
        //    // TODO: Show plot
        //    CreatePlot();
        //}

        private void Calculate()
        {
            SolventViscosity = capFlow.ComputeMeCNViscosity(PercentAcetonitrile, Temperature, TemperatureUnits, ViscosityUnits);

            var points = new List<GraphPoint>(401);
            var xUnits = "%";
            var yUnits = ViscosityUnits.ToString();
            var maxY = 0.0;
            var minY = double.MaxValue;
            for (var i = 0; i <= 400; i++)
            {
                var pct = i / 4.0;
                var viscosity = capFlow.ComputeMeCNViscosity(pct, Temperature, TemperatureUnits, ViscosityUnits);
                maxY = Math.Max(maxY, viscosity);
                minY = Math.Min(minY, viscosity);
                var pt = new GraphPoint(pct, viscosity, xUnits, yUnits);
                points.Add(pt);
            }

            PlotPoints.Load(points);

            // Adjust the Y axis according to the values being plotted
            if (viscosityPlotYAxis != null)
            {
                var diff = maxY - minY;

                // Determine the tick label interval
                var step = 1.0;
                if (diff < 1)
                {
                    while (step > diff)
                    {
                        step /= 10.0;
                    }
                }
                else if (diff > 1)
                {
                    while (step < diff)
                    {
                        step *= 10.0;
                    }

                    step /= 10.0;
                }

                // Use the next lowest and highest tick labels for the minimum and maximum, respectively
                viscosityPlotYAxis.Minimum = Math.Floor(minY / step) * step;
                viscosityPlotYAxis.Maximum = Math.Ceiling(maxY / step) * step;
                viscosityPlotYAxis.MajorStep = step;

                ViscosityPlot?.InvalidatePlot(true);
            }
        }

        private void CreatePlot()
        {
            var plot = new PlotModel();

            var xAxis = new LinearAxis
            {
                Title = "Percent Acetonitrile",
                TitleFontSize = 10,
                Position = AxisPosition.Bottom,
                FontSize = 10,
                AxisTickToLabelDistance = 0,
                MajorStep = 10,
                Minimum = 0,
                AbsoluteMinimum = 0,
                Maximum = 100,
                AbsoluteMaximum = 100,
            };

            var title = "Viscosity";
            var desc = ViscosityUnits.GetType().GetField(ViscosityUnits.ToString()).GetCustomAttributes(false).OfType<DescriptionAttribute>().FirstOrDefault();
            if (desc != null && !string.IsNullOrWhiteSpace(desc.Description))
            {
                title += $" ({desc.Description})";
            }
            else
            {
                title += $" ({ViscosityUnits})";
            }

            var yAxis = new LinearAxis
            {
                Title = title,
                TitleFontSize = 10,
                Position = AxisPosition.Left,
                FontSize = 10,
                AxisTickToLabelDistance = 0,
                Minimum = 0,
                AbsoluteMinimum = 0
            };

            plot.Axes.Add(xAxis);
            plot.Axes.Add(yAxis);

            var line = new LineSeries
            {
                Title = "Viscosity",
                MarkerType = MarkerType.None,
                Color = OxyColors.Red,
                ItemsSource = PlotPoints,
                //Mapping = item => new DataPoint(((GraphPoint)item).X, ((GraphPoint)item).Y),
                DataFieldX = "X",
                DataFieldY = "Y",
                RenderInLegend = false,
                //TrackerFormatString = $"X: {X} {XUnits}; Y: {Y} {YUnits}"
                //TrackerFormatString = $"X: {0} {2}; Y: {1} {3}"
            };

            plot.Series.Add(line);
            plot.InvalidatePlot(true);

            ViscosityPlot = plot;
            viscosityPlotYAxis = yAxis;
        }
    }
}

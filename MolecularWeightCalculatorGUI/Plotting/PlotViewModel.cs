using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Windows.Media;
using DynamicData;
using MolecularWeightCalculator.Data;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using ReactiveUI;
using HorizontalAlignment = OxyPlot.HorizontalAlignment;
using VerticalAlignment = OxyPlot.VerticalAlignment;

namespace MolecularWeightCalculatorGUI.Plotting
{
    internal class PlotViewModel : ReactiveObject
    {
        private PlotModel plot = null;
        private LinearAxis plotYAxis = null;
        private LinearAxis plotXAxis = null;
        private readonly List<SeriesData> seriesList = new List<SeriesData>();
        private int seriesCount = 1;

        public PlotViewModel()
        {
            var seriesData = new SeriesData(1);
            seriesList.Add(seriesData);
            PreparePlot();

            ResetZoomCommand = ReactiveCommand.Create(ResetZoom);
        }

        public PlotModel Plot
        {
            get => plot;
            set => this.RaiseAndSetIfChanged(ref plot, value);
        }

        public ReactiveCommand<Unit, Unit> ResetZoomCommand { get; }

        public void UpdatePlot(bool resetZoom = false)
        {
            if (resetZoom)
            {
                Plot.ResetAllAxes();
            }

            Plot.InvalidatePlot(true);
        }

        public void ResetZoom()
        {
            Plot.ResetAllAxes();
            Plot.InvalidatePlot(false);
        }

        public void ResetZoomOnNextUpdate()
        {
            Plot.ResetAllAxes();
        }

        public int GetSeriesCount()
        {
            return seriesCount;
        }

        public void SetSeriesCount(int count)
        {
            if (count > seriesCount)
            {
                seriesList.Capacity = count;
                for (var i = seriesCount; i < count; i++)
                {
                    var seriesData = new SeriesData(i + 1);
                    seriesList.Add(seriesData);
                    Plot.Series.Add(seriesData.Series);
                }
            }
            seriesCount = count;
            // TODO: handle reduction, also removing now-removed series from the plot...
        }

        public void SetSeriesPlotMode(int seriesNumber, PlotMode plotMode, bool makeDefault)
        {
            if (seriesNumber > seriesCount)
            {
                SetSeriesCount(seriesNumber);
            }

            var color = OxyColors.Blue;
            var seriesData = seriesList[seriesNumber - 1];
            var oldSeries = seriesData.Series;
            if (oldSeries is LineSeries ols)
            {
                color = ols.Color;
            }
            else if (oldSeries is BarSeries obs)
            {
                color = obs.StrokeColor;
            }

            seriesData.DataMode = PlotDataMode.Discrete;
            XYAxisSeries series;
            switch (plotMode)
            {
                case PlotMode.Bar:
                    series = new BarSeries { FillColor = color, StrokeColor = color };
                    break;
                case PlotMode.Lines:
                    series = new LineSeries { MarkerType = MarkerType.None, Color = color, DataFieldX = "X", DataFieldY = "Y" };
                    seriesData.DataMode = PlotDataMode.Continuous;
                    break;
                case PlotMode.StickToZero:
                    series = new StemSeries { MarkerType = MarkerType.None, Color = color, DataFieldX = "X", DataFieldY = "Y" };
                    break;
                case PlotMode.Points:
                    series = new ScatterSeries { DataFieldX = "X", DataFieldY = "Y" };
                    break;
                case PlotMode.PointsAndLines:
                default:
                    series = new LineSeries { MarkerType = MarkerType.Circle, Color = color, MarkerStroke = color, MarkerFill = color, DataFieldX = "X", DataFieldY = "Y" };
                    break;
            }

            series.ItemsSource = seriesData.Points;
            //Mapping = item => new DataPoint(((GraphPoint)item).X, ((GraphPoint)item).Y),
            series.Title = oldSeries.Title;

            seriesData.Series = series;

            Plot.Series.Clear();
            Plot.Series.AddRange(seriesList.Select(x => x.Series));
        }

        public void ClearAnnotations()
        {
            Plot.Annotations.Clear();
        }

        public void SetAnnotations(int seriesNumber, IEnumerable<XYPointImmutable> points)
        {
            if (seriesNumber <= 0 && seriesNumber > seriesCount)
            {
                return;
            }

            var seriesData = seriesList[seriesNumber - 1];

            foreach (var point in points)
            {
                var ann = new LineAnnotation
                {
                    Type = LineAnnotationType.Vertical,
                    X = point.X,
                    MinimumY = point.Y + 1,
                    MaximumY = point.Y + 30,
                    Color = OxyColors.Transparent,
                    Text = point.X.ToString("F2"),
                    TextOrientation = AnnotationTextOrientation.Vertical,
                    TextPosition = new DataPoint(point.X, point.Y + 2),
                    TextRotation = 270,
                    TextHorizontalAlignment = HorizontalAlignment.Left,
                    TextVerticalAlignment = VerticalAlignment.Middle,
                };

                seriesData.Annotations.Add(ann);
                Plot.Annotations.Add(ann);
            }
        }

        public void SetMaxY(double maxY)
        {
            plotYAxis.Maximum = maxY;
        }

        public void ClearData(int seriesToClear)
        {
            if (seriesToClear > 0 && seriesToClear <= seriesCount)
            {
                seriesList[seriesToClear - 1].Points.Clear();
                seriesList[seriesToClear - 1].Initialized = false;
            }
        }

        public void SetSeriesColor(int seriesNumber, Color newColor)
        {
            if (seriesNumber > 0 && seriesNumber <= seriesCount)
            {
                var seriesData = seriesList[seriesNumber - 1];
                var series = seriesData.Series;
                seriesData.Color = newColor;
                var color = OxyColor.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);
                if (series is LineSeries ls)
                {
                    // StemSeries inherits from LineSeries
                    ls.Color = color;
                }
                else if (series is BarSeries bs)
                {
                    bs.FillColor = color;
                    bs.StrokeColor = color;
                }
            }
        }

        public void SetDataXvsY(int seriesNumber, IReadOnlyList<double> xData, IReadOnlyList<double> yData, string legendCaption = "", double originalMaximumIntensity = 0d)
        {
            if (seriesNumber > seriesCount)
            {
                SetSeriesCount(seriesNumber);
            }

            var points = new List<GraphPoint>(xData.Count);
            for (var i = 0; i < xData.Count; i++)
            {
                points.Add(new GraphPoint(xData[i], yData[i]));
            }

            var seriesData = seriesList[seriesNumber - 1];
            seriesData.Points.Load(points);
            seriesData.Series.Title = legendCaption;
            seriesData.Initialized = true;
        }

        public void SetDataXvsY(int seriesNumber, IReadOnlyList<XYPointImmutable> data, string legendCaption = "", double originalMaximumIntensity = 0d)
        {
            if (seriesNumber > seriesCount)
            {
                SetSeriesCount(seriesNumber);
            }

            var points = new List<GraphPoint>(data.Count);
            points.AddRange(data.Select(x => new GraphPoint(x.X, x.Y)));

            var seriesData = seriesList[seriesNumber - 1];
            seriesData.Points.Load(points);
            seriesData.Series.Title = legendCaption;
        }

        public void SetDataXvsY(int seriesNumber, IReadOnlyList<GraphPoint> data, string legendCaption = "", double originalMaximumIntensity = 0d)
        {
            if (seriesNumber > seriesCount)
            {
                SetSeriesCount(seriesNumber);
            }

            var seriesData = seriesList[seriesNumber - 1];
            seriesData.Points.Load(data);
            seriesData.Series.Title = legendCaption;
        }

        public void GetRangeX(out double minimum, out double maximum)
        {
            minimum = plotXAxis.ActualMinimum;
            maximum = plotXAxis.ActualMaximum;
        }

        private void PreparePlot()
        {
            var plot = new ReusablePlotModel { IsLegendVisible = true };

            plot.Legends.Add(new Legend
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
            });

            var xAxis = new LinearAxis
            {
                //Title = "m/z",
                //TitleFontSize = 10,
                Position = AxisPosition.Bottom,
                FontSize = 10,
                AxisTickToLabelDistance = 0,
                //MajorStep = 10,
                //Minimum = 0,
                //AbsoluteMinimum = 0,
                //Maximum = 100,
                //AbsoluteMaximum = 100,
            };

            var yAxis = new LinearAxis
            {
                //Title = "Intensity",
                //TitleFontSize = 10,
                Position = AxisPosition.Left,
                FontSize = 10,
                AxisTickToLabelDistance = 0,
                Minimum = 0,
                AbsoluteMinimum = 0
            };

            plot.Axes.Add(xAxis);
            plot.Axes.Add(yAxis);

            plot.Series.AddRange(seriesList.Select(x => x.Series));
            plot.InvalidatePlot(true);

            Plot = plot;
            plotYAxis = yAxis;
            plotXAxis = xAxis;

            // TODO: Ref: var line = new LineSeries
            // TODO: Ref: {
            // TODO: Ref:     Title = "Viscosity",
            // TODO: Ref:     MarkerType = MarkerType.None,
            // TODO: Ref:     Color = OxyColors.Red,
            // TODO: Ref:     ItemsSource = PlotPoints,
            // TODO: Ref:     //Mapping = item => new DataPoint(((GraphPoint)item).X, ((GraphPoint)item).Y),
            // TODO: Ref:     DataFieldX = "X",
            // TODO: Ref:     DataFieldY = "Y",
            // TODO: Ref:     RenderInLegend = false,
            // TODO: Ref:     //TrackerFormatString = $"X: {X} {XUnits}; Y: {Y} {YUnits}"
            // TODO: Ref:     //TrackerFormatString = $"X: {0} {2}; Y: {1} {3}"
            // TODO: Ref: };
        }
    }
}

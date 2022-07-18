using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Windows.Media;
using DynamicData;
using DynamicData.Binding;
using MolecularWeightCalculator.Data;
using MolecularWeightCalculatorGUI.Utilities;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using ReactiveUI;

namespace MolecularWeightCalculatorGUI.Plotting
{
    internal class PlotViewModel : ReactiveObject
    {
        private PlotModel plot = null;
        private LinearAxis plotYAxis = null;
        private LinearAxis plotXAxis = null;
        private readonly List<Series> seriesList = new List<Series>();
        private readonly List<ObservableCollectionExtended<GraphPoint>> seriesDataList = new List<ObservableCollectionExtended<GraphPoint>>();
        private int seriesCount = 1;

        public PlotViewModel()
        {
            var series = new LineSeries { MarkerType = MarkerType.None, DataFieldX = "X", DataFieldY = "Y" };
            var data = new ObservableCollectionExtended<GraphPoint>();
            series.ItemsSource = data;
            seriesList.Add(series);
            seriesDataList.Add(data);
            PreparePlot();

            ResetZoomCommand = ReactiveCommand.Create(ResetZoom);
        }

        public ObservableCollectionExtended<GraphPoint> PlotPoints { get; } = new ObservableCollectionExtended<GraphPoint>();

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
            return seriesCount = 2;
        }

        public void SetSeriesCount(int count)
        {
            if (count > seriesCount)
            {
                seriesList.Capacity = count;
                seriesDataList.Capacity = count;
                for (var i = seriesCount; i < count; i++)
                {
                    var series = new LineSeries { MarkerType = MarkerType.None, DataFieldX = "X", DataFieldY = "Y" };
                    var data = new ObservableCollectionExtended<GraphPoint>();
                    series.ItemsSource = data;
                    Plot.Series.Add(series);
                    seriesList.Add(series);
                    seriesDataList.Add(data);
                }
            }
            seriesCount = 2;
            // TODO: handle reduction, also removing now-removed series from the plot...
        }

        public void SetSeriesPlotMode(int seriesNumber, PlotMode plotMode, bool makeDefault)
        {
            if (seriesNumber > seriesCount)
            {
                SetSeriesCount(seriesNumber);
            }

            var color = OxyColors.Blue;
            var oldSeries = seriesList[seriesNumber - 1];
            if (oldSeries is LineSeries ols)
            {
                color = ols.Color;
            }
            else if (oldSeries is BarSeries obs)
            {
                color = obs.StrokeColor;
            }

            XYAxisSeries series;
            switch (plotMode)
            {
                case PlotMode.Bar:
                    series = new BarSeries { FillColor = color, StrokeColor = color };
                    break;
                case PlotMode.Lines:
                    series = new LineSeries { MarkerType = MarkerType.None, Color = color, DataFieldX = "X", DataFieldY = "Y" };
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

            series.ItemsSource = seriesDataList[seriesNumber - 1];
            //Mapping = item => new DataPoint(((GraphPoint)item).X, ((GraphPoint)item).Y),
            series.Title = oldSeries.Title;

            seriesList[seriesNumber - 1] = series;

            //Plot.Series.Remove(oldSeries);
            //Plot.Series.Add(series);
            Plot.Series.Clear();
            Plot.Series.AddRange(seriesList);
        }

        public void ClearAnnotations()
        {
            Plot.Annotations.Clear();
        }

        public void SetAnnotations(IEnumerable<XYPointImmutable> points)
        {
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
                seriesDataList[seriesToClear - 1].Clear();
            }
        }

        public void SetSeriesColor(int seriesNumber, Color newColor)
        {
            if (seriesNumber > 0 && seriesNumber <= seriesCount)
            {
                var series = seriesList[seriesNumber - 1];
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

        public void SetDataXvsY(int seriesNumber, List<double> xData, List<double> yData, int dataCount, string legendCaption = "", double originalMaximumIntensity = 0d)
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

            seriesDataList[seriesNumber - 1].Load(points);
            seriesList[seriesNumber - 1].Title = legendCaption;
        }

        public void SetDataXvsY(int seriesNumber, IReadOnlyList<XYPointImmutable> data, string legendCaption = "", double originalMaximumIntensity = 0d)
        {
            if (seriesNumber > seriesCount)
            {
                SetSeriesCount(seriesNumber);
            }

            var points = new List<GraphPoint>(data.Count);
            points.AddRange(data.Select(x => new GraphPoint(x.X, x.Y)));

            seriesDataList[seriesNumber - 1].Load(points);
            seriesList[seriesNumber - 1].Title = legendCaption;
        }

        public void GetRangeX(out double minimum, out double maximum)
        {
            minimum = plotXAxis.ActualMinimum;
            maximum = plotXAxis.ActualMaximum;
        }

        private void Calculate()
        {
            var points = new List<GraphPoint>(401);
            const string xUnits = "%";
            var yUnits = "Intensity";
            var maxY = 0.0;
            var minY = double.MaxValue;
            for (var i = 0; i <= 400; i++)
            {
                var pct = i / 4.0;
                var viscosity = 300;
                maxY = Math.Max(maxY, viscosity);
                minY = Math.Min(minY, viscosity);
                var pt = new GraphPoint(pct, viscosity, xUnits, yUnits);
                points.Add(pt);
            }

            PlotPoints.Load(points);

            // Adjust the Y axis according to the values being plotted
            if (plotYAxis != null)
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
                plotYAxis.Minimum = Math.Floor(minY / step) * step;
                plotYAxis.Maximum = Math.Ceiling(maxY / step) * step;
                plotYAxis.MajorStep = step;

                Plot?.InvalidatePlot(true);
            }
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

            plot.Series.AddRange(seriesList);
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

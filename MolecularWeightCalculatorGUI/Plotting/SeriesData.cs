using System.Collections.Generic;
using System.Windows.Media;
using DynamicData.Binding;
using MolecularWeightCalculator.Data;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;

namespace MolecularWeightCalculatorGUI.Plotting
{
    internal class SeriesData
    {
        public SeriesData(int id)
        {
            ID = id;
            Points = new ObservableCollectionExtended<GraphPoint>();
            AnnotationPoints = new List<XYPointImmutable>();
            Annotations = new List<Annotation>();
            Color = Colors.Blue;
            PlotMode = PlotMode.StickToZero;
            Series = new LineSeries
            {
                MarkerType = MarkerType.None, DataFieldX = "X", DataFieldY = "Y",
                Color = OxyColor.FromArgb(Color.A, Color.R, Color.G, Color.B), ItemsSource = Points
            };
        }

        public int ID { get; }
        public Series Series { get; set; }

        public ObservableCollectionExtended<GraphPoint> Points { get; }

        public List<XYPointImmutable> AnnotationPoints { get; }
        public List<Annotation> Annotations { get; }
        public Color Color { get; set; }
        public PlotMode PlotMode { get; set; }
        public string LegendCaption { get; set; }
        public double OriginalMaximumIntensity { get; set; }
        public bool Initialized { get; set; } = false;

        public PlotDataMode DataMode { get; set; } = PlotDataMode.Discrete;
    }
}

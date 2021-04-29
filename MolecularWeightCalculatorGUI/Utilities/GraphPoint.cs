using OxyPlot;

namespace MolecularWeightCalculatorGUI.Utilities
{
    internal class GraphPoint : IDataPointProvider
    {
        public double X { get; }
        public double Y { get; }
        public string XUnits { get; }
        public string YUnits { get; }

        public GraphPoint(double x, double y, string xUnits = "", string yUnits = "")
        {
            X = x;
            Y = y;
            XUnits = xUnits;
            YUnits = yUnits;
        }

        public override string ToString()
        {
            return $"X: {X} {XUnits}; Y: {Y} {YUnits}";
        }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(X, Y);
        }
    }
}

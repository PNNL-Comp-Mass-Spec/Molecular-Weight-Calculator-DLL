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

        /// <summary>
        /// Show the X and Y values, with units
        /// </summary>
        public override string ToString()
        {
            return string.Format("X: {0:F2} {1}; Y: {2:F2} {3}", X, XUnits, Y, YUnits);
        }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(X, Y);
        }
    }
}

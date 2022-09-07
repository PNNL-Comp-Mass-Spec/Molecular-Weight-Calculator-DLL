using OxyPlot;

namespace MolecularWeightCalculatorGUI.Plotting
{
    internal class GraphPoint : IDataPointProvider
    {
        private const string DefaultXLabel = "X";
        private const string DefaultYLabel = "Y";

        private readonly bool hasLabel;
        private readonly bool hasUnits;
        private readonly bool hasAxisLabel;

        public double X { get; }
        public double Y { get; }
        public string XUnits { get; }
        public string YUnits { get; }
        public string Label { get; }
        public string XLabel { get; } = DefaultXLabel;
        public string YLabel { get; } = DefaultYLabel;

        public GraphPoint(string label, double x, double y, string xLabel = "", string yLabel = "", string xUnits = "", string yUnits = "")
        {
            Label = label;
            X = x;
            Y = y;
            XUnits = xUnits;
            YUnits = yUnits;

            if (!string.IsNullOrWhiteSpace(xLabel))
            {
                XLabel = xLabel;
                hasAxisLabel = true;
            }

            if (!string.IsNullOrWhiteSpace(yLabel))
            {
                YLabel = yLabel;
                hasAxisLabel = true;
            }

            hasLabel = !string.IsNullOrWhiteSpace(Label);
            hasUnits = !string.IsNullOrWhiteSpace(XUnits) || !string.IsNullOrWhiteSpace(YUnits);
        }

        public GraphPoint(double x, double y, string xUnits = "", string yUnits = "") : this("", x, y, xUnits, yUnits)
        { }

        /// <summary>
        /// Show the X and Y values, with units
        /// </summary>
        public override string ToString()
        {
            if (hasLabel)
            {
                if (hasUnits)
                {
                    return $"{Label}: {XLabel}: {X:F2} {XUnits}; {YLabel}: {Y:F2} {YUnits}";
                }

                if (hasAxisLabel)
                {
                    return $"{Label}: {XLabel}: {X:F2}, {YLabel}: {Y:F2}";
                }

                return $"{Label}: {X:F2}, {Y:F2}";
            }

            if (hasUnits)
            {
                return $"{XLabel}: {X:F2} {XUnits}; {YLabel}: {Y:F2} {YUnits}";
            }

            if (hasAxisLabel)
            {
                return $"{XLabel}: {X:F2}, {YLabel}: {Y:F2}";
            }

            return $"{X:F2}, {Y:F2}";
        }

        public DataPoint GetDataPoint()
        {
            return new DataPoint(X, Y);
        }
    }
}

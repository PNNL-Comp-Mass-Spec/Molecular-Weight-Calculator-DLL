using System.Collections.Generic;

namespace MolecularWeightCalculator.Data
{
    public readonly struct XYPointImmutable
    {
        public double X { get; }
        public double Y { get; }

        public XYPointImmutable(double x, double y)
        {
            X = x;
            Y = y;
        }

        public XYPointImmutable(KeyValuePair<double, double> pair)
        {
            X = pair.Key;
            Y = pair.Value;
        }

        public XYPoint ToXYPoint()
        {
            return new XYPoint(X, Y);
        }

        public KeyValuePair<double, double> ToKeyValuePair()
        {
            return new KeyValuePair<double, double>(X, Y);
        }

        /// <summary>
        /// Show the x and y values
        /// </summary>
        public override string ToString()
        {
            return $"{X:F2}, {Y:F2}";
        }
    }
}
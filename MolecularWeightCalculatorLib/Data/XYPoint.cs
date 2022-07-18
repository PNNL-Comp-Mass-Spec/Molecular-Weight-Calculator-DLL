using System;

namespace MolecularWeightCalculator.Data
{
    public class XYPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public XYPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public XYPointImmutable ToReadOnly()
        {
            return new XYPointImmutable(X, Y);
        }

        public XYPointImmutable ToReadOnly(Func<double, double> xTransform, Func<double, double> yTransform)
        {
            return new XYPointImmutable(xTransform(X), yTransform(Y));
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

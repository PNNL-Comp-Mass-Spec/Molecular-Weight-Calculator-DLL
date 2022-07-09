using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MolecularWeightCalculator.Formula;

namespace MolecularWeightCalculator.Tools
{
    public class Gaussian
    {

        // ReSharper disable once InconsistentNaming
        private class XYData
        {
            public double X { get; set; }
            public double Y { get; set; }

            /// <summary>
            /// Show the x and y values
            /// </summary>
            public override string ToString()
            {
                return string.Format("{0:F2}, {1:F2}", X, Y);
            }
        }

        /// <summary>
        /// Convert the centroided data (stick data) in XYVals to a Gaussian representation
        /// </summary>
        /// <param name="xyVals">XY data, as key-value pairs</param>
        /// <param name="resolution">Effective instrument resolution (e.g. 1000 or 20000)</param>
        /// <param name="resolutionMass">The m/z value at which the resolution applies</param>
        /// <param name="qualityFactor">Gaussian quality factor (between 1 and 75, default is 50)</param>
        /// <returns>Gaussian spectrum data</returns>
        public static List<KeyValuePair<double, double>> ConvertStickDataToGaussian2DArray(List<KeyValuePair<double, double>> xyVals, int resolution, double resolutionMass, int qualityFactor, ElementAndMassTools elementAndMass = null)
        {
            // xyVals is 0-based (thus ranging from 0 to xyVals.count-1)
            // The arrays should contain stick data
            // The original data in the arrays will be replaced with Gaussian peaks in place of each "stick"
            // Note: Assumes xyVals is sorted in the 'x' direction

            const int maxDataPoints = 1000000;
            const short massPrecision = 7;

            var thisDataPoint = new XYData();

            var gaussianData = new List<KeyValuePair<double, double>>();

            try
            {
                double xValRange;

                if (xyVals == null || xyVals.Count == 0)
                {
                    return gaussianData;
                }

                var xySummation = new List<XYData>(xyVals.Count * 10);

                // Determine the data range for xyVals
                if (xyVals.Count > 1)
                {
                    xValRange = xyVals.Last().Key - xyVals.First().Key;
                }
                else
                {
                    xValRange = 1d;
                }

                if (xValRange < 1d)
                    xValRange = 1d;

                if (resolution < 1)
                    resolution = 1;

                if (qualityFactor is < 1 or > 75)
                    qualityFactor = 50;

                // Compute deltaX using resolution and resolutionMass
                // Do not allow the DeltaX to be so small that the total points required > MAX_DATA_POINTS
                var deltaX = resolutionMass / resolution / qualityFactor;

                // Make sure DeltaX is a reasonable number
                deltaX = MathTools.RoundToMultipleOf10(deltaX);

                if (Math.Abs(deltaX) < float.Epsilon)
                    deltaX = 1d;

                var sigma = resolutionMass / resolution / Math.Sqrt(5.54d);

                // Set the window range (the xValue window width range) to calculate the Gaussian representation for each data point
                // The width at the base of a peak is 4 sigma
                // Use a width of 2 * 6 sigma
                var xValWindowRange = 2 * 6 * sigma;

                if (xValRange / deltaX > maxDataPoints)
                {
                    // Delta x is too small; change to a reasonable value
                    // This isn't a bug, but it may mean one of the default settings is inappropriate
                    deltaX = xValRange / maxDataPoints;
                }

                var dataToAddCount = (int)Math.Round(xValWindowRange / deltaX);

                // Make sure dataToAddCount is odd
                if (dataToAddCount % 2 == 0)
                {
                    dataToAddCount++;
                }

                var dataToAdd = new List<XYData>(dataToAddCount);
                var midPointIndex = (int)Math.Round((dataToAddCount + 1) / 2d - 1d);

                // Compute the Gaussian data for each point in xyVals[]
                for (var stickIndex = 0; stickIndex < xyVals.Count; stickIndex++)
                {
                    if (stickIndex % 25 == 0)
                    {
                        if (elementAndMass?.AbortProcessing ?? false)
                            break;
                    }

                    // Search through xySummation to determine the index of the smallest XValue with which
                    // data in dataToAdd could be combined
                    var minimalSummationIndex = 0;
                    dataToAdd.Clear();

                    var minimalXValOfWindow = xyVals[stickIndex].Key - midPointIndex * deltaX;

                    var searchForMinimumXVal = true;
                    if (xySummation.Count > 0)
                    {
                        if (minimalXValOfWindow > xySummation[xySummation.Count - 1].X)
                        {
                            minimalSummationIndex = xySummation.Count - 1;
                            searchForMinimumXVal = false;
                        }
                    }

                    if (searchForMinimumXVal)
                    {
                        if (xySummation.Count == 0)
                        {
                            minimalSummationIndex = 0;
                        }
                        else
                        {
                            int summationIndex;
                            for (summationIndex = 0; summationIndex < xySummation.Count; summationIndex++)
                            {
                                if (xySummation[summationIndex].X >= minimalXValOfWindow)
                                {
                                    minimalSummationIndex = summationIndex - 1;
                                    if (minimalSummationIndex < 0)
                                        minimalSummationIndex = 0;
                                    break;
                                }
                            }

                            if (summationIndex >= xySummation.Count)
                            {
                                minimalSummationIndex = xySummation.Count - 1;
                            }
                        }
                    }

                    // Construct the Gaussian representation for this Data Point
                    thisDataPoint.X = xyVals[stickIndex].Key;
                    thisDataPoint.Y = xyVals[stickIndex].Value;

                    // Round ThisDataPoint.XVal to the nearest DeltaX
                    // If .XVal is not an even multiple of DeltaX then bump up .XVal until it is
                    thisDataPoint.X = MathTools.RoundToEvenMultiple(thisDataPoint.X, deltaX, true);

                    for (var index = 0; index < dataToAddCount; index++)
                    {
                        // Equation for Gaussian is: Amplitude * Exp[ -(x - mu)^2 / (2*sigma^2) ]
                        // Use index, .YVal, and deltaX
                        var xOffSet = (midPointIndex - index) * deltaX;

                        var newPoint = new XYData
                        {
                            X = thisDataPoint.X - xOffSet,
                            Y = thisDataPoint.Y * Math.Exp(-Math.Pow(xOffSet, 2d) / (2d * Math.Pow(sigma, 2d)))
                        };

                        dataToAdd.Add(newPoint);
                    }

                    // Now merge dataToAdd into xySummation
                    // XValues in dataToAdd and those in xySummation have the same DeltaX value
                    // The XValues in dataToAdd might overlap partially with those in xySummation

                    var dataIndex = 0;
                    bool appendNewData;

                    // First, see if the first XValue in dataToAdd is larger than the last XValue in xySummation
                    if (xySummation.Count == 0)
                    {
                        appendNewData = true;
                    }
                    else if (dataToAdd[dataIndex].X > xySummation.Last().X)
                    {
                        appendNewData = true;
                    }
                    else
                    {
                        appendNewData = false;
                        // Step through xySummation starting at minimalSummationIndex, looking for
                        // the index to start combining data at
                        for (var summationIndex = minimalSummationIndex; summationIndex < xySummation.Count; summationIndex++)
                        {
                            if (Math.Round(dataToAdd[dataIndex].X, massPrecision) <= Math.Round(xySummation[summationIndex].X, massPrecision))
                            {
                                // Within Tolerance; start combining the values here
                                while (summationIndex < xySummation.Count)
                                {
                                    var currentVal = xySummation[summationIndex];
                                    currentVal.Y += dataToAdd[dataIndex].Y;

                                    xySummation[summationIndex] = currentVal;

                                    summationIndex++;
                                    dataIndex++;
                                    if (dataIndex >= dataToAddCount)
                                    {
                                        // Successfully combined all of the data
                                        break;
                                    }
                                }

                                if (dataIndex < dataToAddCount)
                                {
                                    // Data still remains to be added
                                    appendNewData = true;
                                }

                                break;
                            }
                        }
                    }

                    if (appendNewData)
                    {
                        while (dataIndex < dataToAddCount)
                        {
                            xySummation.Add(dataToAdd[dataIndex]);
                            dataIndex++;
                        }
                    }
                }

                // Assure there is a data point at each 1% point along x range (to give better looking plots)
                // Probably need to add data, but may need to remove some
                var minimalXValSpacing = xValRange / 100d;

                for (var summationIndex = 0; summationIndex < xySummation.Count - 1; summationIndex++)
                {
                    if (xySummation[summationIndex].X + minimalXValSpacing < xySummation[summationIndex + 1].X)
                    {
                        // Need to insert a data point

                        // Choose the appropriate new .XVal
                        var rangeWork = xySummation[summationIndex + 1].X - xySummation[summationIndex].X;
                        if (rangeWork < minimalXValSpacing * 2d)
                        {
                            rangeWork /= 2d;
                        }
                        else
                        {
                            rangeWork = minimalXValSpacing;
                        }

                        // The new .YVal is the average of that at summationIndex and that at summationIndex + 1
                        var newDataPoint = new XYData
                        {
                            X = xySummation[summationIndex].X + rangeWork,
                            Y = (xySummation[summationIndex].Y + xySummation[summationIndex + 1].Y) / 2d
                        };

                        xySummation.Insert(summationIndex + 1, newDataPoint);
                    }
                }

                // Copy data from xySummation to gaussianData
                gaussianData.Capacity = Math.Max(gaussianData.Count, xySummation.Count);
                gaussianData.AddRange(xySummation.Select(item => new KeyValuePair<double, double>(item.X, item.Y)));
            }
            catch (Exception ex)
            {
                if (elementAndMass != null)
                {
                    elementAndMass.GeneralErrorHandler("ConvertStickDataToGaussian", ex);
                }
                else
                {
                    throw;
                }
            }

            return gaussianData;
        }
    }
}

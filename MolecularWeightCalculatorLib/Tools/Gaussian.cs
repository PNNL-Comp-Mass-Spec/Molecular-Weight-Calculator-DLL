using System;
using System.Collections.Generic;
using System.Linq;
using MolecularWeightCalculator.Data;
using MolecularWeightCalculator.Formula;

namespace MolecularWeightCalculator.Tools
{
    public static class Gaussian
    {
        private static void CheckListCapacity<T>(ref List<T> list, int neededCapacity, int increment = 200)
        {
            if (neededCapacity > list.Capacity)
            {
                list.Capacity += increment;
                if (list.Capacity < neededCapacity)
                {
                    list.Capacity = neededCapacity;
                }
            }
        }

        /// <summary>
        /// Convert the centroided data (stick data) in XYVals to a Gaussian representation
        /// </summary>
        /// <param name="inputData">XY data, as key-value pairs</param>
        /// <param name="resolution">Effective instrument resolution (e.g. 1000 or 20000)</param>
        /// <param name="resolutionMass">The m/z value at which the resolution applies</param>
        /// <param name="qualityFactor">Gaussian quality factor (between 1 and 75, default is 50)</param>
        /// <param name="fillGaps">When true, ensures at least one data point in the output for every 1% point in the data range</param>
        /// <param name="elementAndMass">Reference to a <see cref="ElementAndMassTools"/> object for process abortion and error logging/messaging</param>
        /// <returns>Gaussian spectrum data</returns>
        public static List<KeyValuePair<double, double>> ConvertStickDataToGaussian2DArray(List<KeyValuePair<double, double>> inputData, int resolution, double resolutionMass, int qualityFactor, bool fillGaps = true, ElementAndMassTools elementAndMass = null)
        {
            return ConvertStickDataToGaussian2DArray(inputData.ConvertAll(x => new XYPointImmutable(x)), resolution, resolutionMass, qualityFactor, fillGaps, elementAndMass).ConvertAll(x => x.ToKeyValuePair());
        }

        /// <summary>
        /// Convert the centroided data (stick data) in XYVals to a Gaussian representation
        /// </summary>
        /// <param name="inputData">XY data, as key-value pairs</param>
        /// <param name="resolution">Effective instrument resolution (e.g. 1000 or 20000)</param>
        /// <param name="resolutionMass">The m/z value at which the resolution applies</param>
        /// <param name="qualityFactor">Gaussian quality factor (between 1 and 75, default is 50)</param>
        /// <param name="fillGaps">When true, ensures at least one data point in the output for every 1% point in the data range</param>
        /// <param name="elementAndMass">Reference to a <see cref="ElementAndMassTools"/> object for process abortion and error logging/messaging</param>
        /// <returns>Gaussian spectrum data</returns>
        public static List<XYPointImmutable> ConvertStickDataToGaussian2DArray(IReadOnlyList<XYPointImmutable> inputData, int resolution, double resolutionMass, int qualityFactor, bool fillGaps = true, ElementAndMassTools elementAndMass = null)
        {
            // inputData[] is 0-based (thus ranging from 0 to inputData.Count-1)
            // The arrays should contain stick data
            // A Gaussian peak will be added to the output data for each "stick" in the original data
            // Note: Assumes inputData[] is sorted in the 'x' direction

            const int maxDataPoints = 1000000;

            if (inputData == null || inputData.Count == 0)
            {
                return new List<XYPointImmutable>();
            }

            try
            {
                // Determine the data range for inputData[].X
                double xRange;
                if (inputData.Count > 1)
                {
                    xRange = inputData.Last().X - inputData[0].X;
                }
                else
                {
                    xRange = 1d;
                }

                if (xRange < 1d)
                    xRange = 1d;

                if (resolution < 1)
                    resolution = 1;

                if (qualityFactor is < 1 or > 75)
                    qualityFactor = 50;

                // Compute deltaX using resolution and resolutionMass
                // Do not allow the deltaX to be so small that the total points required > maxDataPoints
                var deltaX = resolutionMass / resolution / qualityFactor;

                // Make sure deltaX is a reasonable number
                deltaX = MathTools.RoundToMultipleOf10(deltaX);

                if (Math.Abs(deltaX) < float.Epsilon)
                    deltaX = 1d;

                var sigma = resolutionMass / resolution / Math.Sqrt(5.54d);

                // Set the window range (the xValue window width range) to calculate the Gaussian representation for each data point
                // The width at the base of a peak is 4 sigma
                // Use a width of 2 * 6 sigma
                var xWindowRange = 2 * 6 * sigma;

                if (xRange / deltaX > maxDataPoints)
                {
                    // Delta x is too small; change to a reasonable value
                    // This isn't a bug, but it may mean one of the default settings is inappropriate
                    deltaX = xRange / maxDataPoints;
                }

                var gaussianPointCount = (int)Math.Round(xWindowRange / deltaX);

                // Make sure gaussianPointCount is odd
                if (gaussianPointCount % 2 == 0)
                {
                    gaussianPointCount++;
                }

                var gaussianPeak = new List<XYPointImmutable>(gaussianPointCount);
                var apexIndex = gaussianPointCount / 2;

                // Initialize summedData
                var summedData = new List<XYPointImmutable>(inputData.Count * 10);

                // Compute the Gaussian data for each point in inputData[].X
                for (var stickIndex = 0; stickIndex < inputData.Count; stickIndex++)
                {
                    if (stickIndex % 25 == 0)
                    {
                        if (elementAndMass?.AbortProcessing ?? false)
                            break;
                    }

                    gaussianPeak.Clear();

                    // Construct the Gaussian representation for this data point
                    var currentX = inputData[stickIndex].X;

                    // Round currentX to the nearest deltaX
                    // If currentX is not an even multiple of deltaX then bump up currentX until it is
                    currentX = MathTools.RoundToEvenMultiple(currentX, deltaX, true);
                    var refPoint = new XYPointImmutable(currentX, inputData[stickIndex].Y);

                    for (var j = 0; j < gaussianPointCount; j++)
                    {
                        // Equation for Gaussian is: Amplitude * Exp[ -(x - mu)^2 / (2*sigma^2) ]
                        // Use index, .Y, and deltaX
                        var xOffSet = (apexIndex - j) * deltaX;

                        var point = new XYPointImmutable(refPoint.X - xOffSet,
                            refPoint.Y * Math.Exp(-Math.Pow(xOffSet, 2d) / (2d * Math.Pow(sigma, 2d))));

                        gaussianPeak.Add(point);
                    }

                    // Now merge gaussianPeak into summedData
                    var windowMinX = inputData[stickIndex].X - apexIndex * deltaX;
                    CombineLists(summedData, gaussianPeak, windowMinX);
                }

                if (fillGaps)
                {
                    // return the filled gaussian data
                    return FillGapsToOnePercent(summedData, xRange);
                }

                return summedData;
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

            return new List<XYPointImmutable>();
        }

        private static void CombineLists(List<XYPointImmutable> summedData, IReadOnlyList<XYPointImmutable> newData, double windowMinX)
        {
            const int massPrecision = 7;

            // Search through summedData to determine the index of the smallest XValue with which
            // data in newData could be summedData
            var minSumIndex = 0;
            if (summedData.Count > 0 && windowMinX > summedData.Last().X)
            {
                minSumIndex = summedData.Count - 1;
            }
            else if (summedData.Count == 0)
            {
                minSumIndex = 0;
            }
            else
            {
                var found = false;
                for (var j = 0; j < summedData.Count; j++)
                {
                    if (summedData[j].X >= windowMinX)
                    {
                        minSumIndex = j - 1;
                        if (minSumIndex < 0)
                            minSumIndex = 0;

                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    minSumIndex = summedData.Count - 1;
                }
            }

            // Now merge newData into summedData
            // XValues in newData and those in summedData have the same deltaX value
            // The XValues in newData might overlap partially with those in summedData

            var peakIndex = 0;

            // First, see if the first XValue in newData is larger than the last XValue in summedData
            bool appendNewData;
            if (summedData.Count == 0 || newData[0].X > summedData.Last().X)
            {
                appendNewData = true;
            }
            else
            {
                appendNewData = false;
                // Step through summedData[] starting at minCombineIndex, looking for
                // the index to start combining data at
                for (var i = minSumIndex; i < summedData.Count; i++)
                {
                    if (Math.Round(newData[peakIndex].X, massPrecision) <= Math.Round(summedData[i].X, massPrecision))
                    {
                        // Within tolerance; start combining the values here
                        while (i < summedData.Count)
                        {
                            var sumTo = summedData[i];
                            //summedData[index].Y += newData[peakIndex].Y;
                            summedData[i] = new XYPointImmutable(sumTo.X, sumTo.Y + newData[peakIndex].Y);
                            i++;
                            peakIndex++;
                            if (peakIndex >= newData.Count)
                            {
                                // Successfully combined all of the data
                                break;
                            }
                        }

                        if (peakIndex < newData.Count)
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
                CheckListCapacity(ref summedData, summedData.Count + newData.Count - peakIndex);
                while (peakIndex < newData.Count)
                {
                    summedData.Add(newData[peakIndex]);
                    peakIndex++;
                }
            }
        }

        private static List<XYPointImmutable> FillGapsToOnePercent(IReadOnlyList<XYPointImmutable> input, double xRange)
        {
            // Assure there is a data point at each 1% point along x range (to give better looking plots)
            // Probably need to add data, but may need to remove some
            var minimalXValSpacing = xRange / 100d;

            var output = new List<XYPointImmutable>(input.Count);
            for (var i = 0; i < input.Count - 1; i++)
            {
                // Add the current data point to the new list
                output.Add(input[i]);

                var lowPoint = input[i];
                var highPoint = input[i + 1];

                if (lowPoint.X + minimalXValSpacing < highPoint.X)
                {
                    // Need to add data point(s)
                    // Determine how many points we need to add
                    var xDiff = highPoint.X - lowPoint.X;
                    var yDiff = highPoint.Y - lowPoint.Y;
                    var xInterval = xDiff / 2d;
                    var pointsToAdd = 1;
                    for (var j = 3; xInterval > minimalXValSpacing; j++)
                    {
                        xInterval = xDiff / j;
                        pointsToAdd++;
                    }

                    var yInterval = yDiff / (pointsToAdd + 1);

                    CheckListCapacity(ref output, output.Count + pointsToAdd + input.Count - i + 2);

                    // Add in the new interpolated (linear) values
                    for (var j = 1; j <= pointsToAdd; j++)
                    {
                        var interpolatedPoint = new XYPointImmutable(lowPoint.X + xInterval * j, lowPoint.Y + yInterval * j);
                        output.Add(interpolatedPoint);
                    }
                }
            }

            output.Add(input.Last());

            return output;
        }
    }
}

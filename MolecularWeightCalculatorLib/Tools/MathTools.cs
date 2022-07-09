using System;
using System.Globalization;

namespace MolecularWeightCalculator.Tools
{
    public static class MathTools
    {
        public static double RoundToEvenMultiple(double valueToRound, double multipleValue, bool roundUp)
        {
            // Find the exponent of multipleValue
            var workText = multipleValue.ToString("0E+000");
            var exponentValue = NumberConverter.CIntSafe(workText.Substring(workText.Length - 4));

            var loopCount = 0;
            while ((valueToRound / multipleValue).ToString(CultureInfo.InvariantCulture) != Math.Round(valueToRound / multipleValue, 0).ToString(CultureInfo.InvariantCulture))
            {
                var work = valueToRound / Math.Pow(10d, exponentValue);
                work = double.Parse(work.ToString("0"));
                work *= Math.Pow(10d, exponentValue);
                if (roundUp)
                {
                    if (work <= valueToRound)
                    {
                        work += Math.Pow(10d, exponentValue);
                    }
                }
                else if (work >= valueToRound)
                {
                    work -= Math.Pow(10d, exponentValue);
                }

                valueToRound = work;
                loopCount++;
                if (loopCount > 500)
                {
                    // Debug.Assert False
                    break;
                }
            }

            return valueToRound;
        }

        public static double RoundToMultipleOf10(double number)
        {
            // Round to nearest 1, 2, or 5 (or multiple of 10 thereof)
            // First, find the exponent of number
            var workText = number.ToString("0E+000");
            var exponentValue = NumberConverter.CIntSafe(workText.Substring(workText.Length - 4));
            var work = number / Math.Pow(10d, exponentValue);
            work = NumberConverter.CIntSafe(work);

            // work should now be between 0 and 9
            number = work switch
            {
                0d or 1d => 1d,
                2d or 3d or 4d => 2d,
                _ => 5d,
            };

            // Convert number back to the correct magnitude
            number *= Math.Pow(10d, exponentValue);

            return number;
        }

        public static int RoundToNearest(double numberToRound, int multipleToRoundTo, bool roundUp)
        {
            // Rounds a number to the nearest multiple specified
            // If roundUp = True, then always rounds up
            // If roundUp = False, then always rounds down

            if (multipleToRoundTo == 0)
                multipleToRoundTo = 1;

            // Cast to int to get the floor of the number
            var roundedNumber = (int)Math.Round((double)((int)(numberToRound / multipleToRoundTo)) * multipleToRoundTo);

            if (roundUp & roundedNumber < numberToRound)
            {
                roundedNumber = roundedNumber + multipleToRoundTo;
            }
            return roundedNumber;
        }
    }
}

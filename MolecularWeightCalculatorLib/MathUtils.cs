using System;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator
{
    [ComVisible(false)]
    public static class MathUtils
    {
        /// <summary>
        /// Compute the factorial of a number; uses recursion
        /// </summary>
        /// <param name="number">Integer number between 0 and 170</param>
        /// <returns>The factorial, or -1 if an error</returns>
        public static double Factorial(int number)
        {
            try
            {
                if (number > 170)
                {
                    Console.WriteLine("Cannot compute factorial of a number over 170");
                    return -1;
                }

                if (number < 0)
                {
                    Console.WriteLine("Cannot compute factorial of a negative number");
                    return -1;
                }

                if (number == 0)
                {
                    return 1d;
                }

                return number * Factorial((number - 1));
            }
            catch
            {
                Console.WriteLine("Number too large");
                return -1;
            }
        }
    }
}

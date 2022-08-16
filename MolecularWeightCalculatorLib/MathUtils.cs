using System;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator
{
    [ComVisible(false)]
    public static class MathUtils
    {
        /// <summary>
        /// Compute the combination, n choose r
        /// </summary>
        /// <param name="n">Number of choices</param>
        /// <param name="r">Number of items to choose</param>
        /// <returns>The combination, or -1 if an error</returns>
        public static double Combination(int n, int r)
        {
            // TODO: Redo these limits?
            if (n > 170 || r > 170)
            {
                Console.WriteLine("Cannot compute factorial of a number over 170.  Thus, cannot compute the combination.");
                return -1;
            }

            if (n < r)
            {
                Console.WriteLine("First number should be greater than or equal to the second number");
                return -1;
            }

            return Factorial(n) / (Factorial(r) * Factorial(n - r));
        }

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

                return number * Factorial(number - 1);
            }
            catch
            {
                Console.WriteLine("Number too large");
                return -1;
            }
        }
    }
}

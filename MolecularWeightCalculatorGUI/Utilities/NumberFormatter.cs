using System;

namespace MolecularWeightCalculatorGUI.Utilities
{
    internal static class NumberFormatter
    {
        public static string GetDisplayFormatStringLong(double number)
        {
            // Equivalent(ish) to VB6 'FormatTextBox'
            if (Math.Abs(number) < float.Epsilon)
            {
                return "0";
            }

            // TODO: VB6 has an option to prefer scientific notation for some things
            if (number < 0.0001 || number > 100000)
            {
                // Use scientific notation
                return "0.0###E+00";
            }

            // Display result in 0.000 notation rather than exponential
            return "0.0#####";
        }

        public static string GetDisplayFormatStringShort(double number)
        {
            // Equivalent(ish) to VB6 'FormatLabel'
            if (Math.Abs(number) < float.Epsilon)
            {
                return "0";
            }

            // TODO: VB6 has an option to prefer scientific notation for some things
            if (number < 0.0001 || number > 100000)
            {
                // Use scientific notation
                return "0.0###E+00";
            }

            // Display result in 0.000 notation rather than exponential
            return "0.0###";
        }

        public static string FormatNumberForDisplayLong(double number)
        {
            var format = GetDisplayFormatStringLong(number);
            return number.ToString(format);
        }

        public static string FormatNumberForDisplayShort(double number)
        {
            var format = GetDisplayFormatStringShort(number);
            return number.ToString(format);
        }
    }
}

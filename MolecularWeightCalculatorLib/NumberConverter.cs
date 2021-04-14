using System;
using System.Runtime.InteropServices;

namespace MolecularWeightCalculator
{
    [ComVisible(false)]
    internal class NumberConverter
    {
        public static double CDblSafe(string work)
        {
            if (double.TryParse(work, out var value))
            {
                return value;
            }

            return 0d;
        }

        public static short CShortSafe(double work)
        {
            if (work is >= -32767 and <= 32767d)
            {
                return (short)Math.Round(work);
            }

            if (work < 0d)
            {
                return -32767;
            }
            return 32767;
        }

        public static short CShortSafe(string work)
        {
            if (double.TryParse(work, out var value))
            {
                return CShortSafe(value);
            }

            if (work.ToLower() == "true")
            {
                return -1;
            }
            return 0;
        }

        public static int CIntSafe(double work)
        {
            if (work is >= int.MinValue and <= int.MaxValue)
            {
                return (int)Math.Round(work);
            }

            if (work < 0d)
            {
                return int.MinValue;
            }
            return int.MaxValue;
        }

        public static int CIntSafe(string work)
        {
            if (double.TryParse(work, out var value))
            {
                return CIntSafe(value);
            }

            if (work.ToLower() == "true")
            {
                return -1;
            }
            return 0;
        }

        public static string CStrSafe(object item)
        {
            try
            {
                if (item == null)
                {
                    return string.Empty;
                }

                if (Convert.IsDBNull(item))
                {
                    return string.Empty;
                }
                return item.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool IsNumber(string value)
        {
            try
            {
                return double.TryParse(value, out _);
            }
            catch
            {
                return false;
            }
        }
    }
}
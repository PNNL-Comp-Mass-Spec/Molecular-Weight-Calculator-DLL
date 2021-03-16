using System;
using Microsoft.VisualBasic.CompilerServices;

namespace MolecularWeightCalculator
{
    class NumberConverter
    {
        public static double CDblSafe(string strWork)
        {
            if (double.TryParse(strWork, out var dblValue))
            {
                return dblValue;
            }

            return 0d;
        }

        public static short CShortSafe(double dblWork)
        {
            if (dblWork <= 32767d && dblWork >= -32767)
            {
                return (short)Math.Round(dblWork);
            }

            if (dblWork < 0d)
            {
                return -32767;
            }
            return 32767;
        }

        public static short CShortSafe(string strWork)
        {
            if (double.TryParse(strWork, out var dblValue))
            {
                return CShortSafe(dblValue);
            }

            if (strWork.ToLower() == "true")
            {
                return -1;
            }
            return 0;
        }

        public static int CIntSafe(double dblWork)
        {
            if (dblWork <= int.MaxValue && dblWork >= int.MinValue)
            {
                return (int)Math.Round(dblWork);
            }

            if (dblWork < 0d)
            {
                return int.MinValue;
            }
            return int.MaxValue;
        }

        public static int CIntSafe(string strWork)
        {
            if (double.TryParse(strWork, out var dblValue))
            {
                return CIntSafe(dblValue);
            }

            if (strWork.ToLower() == "true")
            {
                return -1;
            }
            return 0;
        }

        public static string CStrSafe(object Item)
        {
            try
            {
                if (Item == null)
                {
                    return string.Empty;
                }

                if (Convert.IsDBNull(Item))
                {
                    return string.Empty;
                }
                return Item.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool IsNumber(string strValue)
        {
            try
            {
                return double.TryParse(strValue, out _);
            }
            catch
            {
                return false;
            }
        }
    }
}
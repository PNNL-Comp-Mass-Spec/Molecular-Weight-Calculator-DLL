using System;
using Microsoft.VisualBasic.CompilerServices;

namespace MwtWinDll
{
    class NumberConverter
    {
        public static double CDblSafe(string strWork)
        {
            double dblValue = 0d;
            if (double.TryParse(strWork, out dblValue))
            {
                return dblValue;
            }
            else
            {
                return 0d;
            }
        }

        public static short CShortSafe(double dblWork)
        {
            if (dblWork <= 32767d && dblWork >= -32767)
            {
                return (short)Math.Round(dblWork);
            }
            else if (dblWork < 0d)
            {
                return -32767;
            }
            else
            {
                return 32767;
            }
        }

        public static short CShortSafe(string strWork)
        {
            double dblValue = 0d;
            if (double.TryParse(strWork, out dblValue))
            {
                return CShortSafe(dblValue);
            }
            else if (strWork.ToLower() == "true")
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        public static int CIntSafe(double dblWork)
        {
            if (dblWork <= int.MaxValue && dblWork >= int.MinValue)
            {
                return (int)Math.Round(dblWork);
            }
            else if (dblWork < 0d)
            {
                return int.MinValue;
            }
            else
            {
                return int.MaxValue;
            }
        }

        public static int CIntSafe(string strWork)
        {
            double dblValue = 0d;
            if (double.TryParse(strWork, out dblValue))
            {
                return CIntSafe(dblValue);
            }
            else if (strWork.ToLower() == "true")
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        public static string CStrSafe(object Item)
        {
            try
            {
                if (Item is null)
                {
                    return string.Empty;
                }
                else if (Convert.IsDBNull(Item))
                {
                    return string.Empty;
                }
                else
                {
                    return Conversions.ToString(Item);
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static bool IsNumber(string strValue)
        {
            try
            {
                double argresult = 0d;
                return double.TryParse(strValue, out argresult);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
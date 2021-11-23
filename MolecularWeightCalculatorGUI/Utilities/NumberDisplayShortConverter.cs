using System;
using System.Globalization;
using System.Windows.Data;

namespace MolecularWeightCalculatorGUI.Utilities
{
    internal class NumberDisplayShortConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string str && double.TryParse(str, out var num))
            {
                return NumberFormatter.FormatNumberForDisplayShort(num);
            }

            var valueType = value.GetType();
            if (!(valueType.IsValueType || valueType.IsPrimitive))
            {
                return value;
            }

            try
            {
                var number = (double)System.Convert.ChangeType(value, TypeCode.Double);
                return NumberFormatter.FormatNumberForDisplayShort(number);
            }
            catch
            {
                if (double.TryParse(value.ToString(), out var num2))
                {
                    return NumberFormatter.FormatNumberForDisplayShort(num2);
                }

                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}

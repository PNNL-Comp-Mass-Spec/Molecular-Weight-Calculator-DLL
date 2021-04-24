using System;
using System.Globalization;
using System.Windows.Data;

namespace MolecularWeightCalculatorGUI.Utilities
{
    internal class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(bool))
            {
                return value;
            }

            return !(bool) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}

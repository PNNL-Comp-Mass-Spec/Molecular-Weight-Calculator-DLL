using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MolecularWeightCalculatorGUI.Utilities
{
    internal class ValueVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            if (value is string str)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }

            if (value is bool b)
            {
                return b ? Visibility.Visible : Visibility.Collapsed;
            }

            if (double.TryParse(value.ToString(), out var number))
            {
                return Math.Abs(number) < float.Epsilon ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}

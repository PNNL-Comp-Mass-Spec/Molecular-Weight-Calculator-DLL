using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MolecularWeightCalculatorGUI.Utilities
{
    internal class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.GetType().IsEnum != true)
            {
                return string.Empty;
            }

            var attrib = value.GetType().GetField(value.ToString()).GetCustomAttributes(false);
            var desc = attrib.OfType<DescriptionAttribute>().FirstOrDefault();

            if (desc == null)
            {
                return value.ToString();
            }
            else
            {
                return desc.Description;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}

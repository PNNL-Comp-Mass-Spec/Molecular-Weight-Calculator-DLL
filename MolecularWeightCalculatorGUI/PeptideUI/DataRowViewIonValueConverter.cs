using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace MolecularWeightCalculatorGUI.PeptideUI
{
    internal class DataRowViewIonValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is DataGridCell cell))
                return "";

            if (!(cell.DataContext is System.Data.DataRowView drv))
                return "";

            var content = drv.Row[cell.Column.SortMemberPath];
            if (!(content is FragmentationGridIon fgi))
                return "";

            return fgi.Value.ToString("F4", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}

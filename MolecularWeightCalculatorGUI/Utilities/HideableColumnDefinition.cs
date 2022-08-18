using System.Windows;
using System.Windows.Controls;

namespace MolecularWeightCalculatorGUI.Utilities
{
    // Sourced from https://www.codeproject.com/Articles/437237/WPF-Grid-Column-and-Row-Hiding
    public class HideableColumnDefinition : ColumnDefinition
    {
        // Variables
        public static DependencyProperty VisibleProperty;

        // Properties
        public bool Visible
        {
            get => (bool)GetValue(VisibleProperty);
            set => SetValue(VisibleProperty, value);
        }

        // Constructors
        static HideableColumnDefinition()
        {
            VisibleProperty = DependencyProperty.Register("Visible",
                typeof(bool),
                typeof(HideableColumnDefinition),
                new PropertyMetadata(true, new PropertyChangedCallback(OnVisibleChanged)));

            ColumnDefinition.WidthProperty.OverrideMetadata(typeof(HideableColumnDefinition),
                new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null,
                    new CoerceValueCallback(CoerceWidth)));

            ColumnDefinition.MinWidthProperty.OverrideMetadata(typeof(HideableColumnDefinition),
                new FrameworkPropertyMetadata((double)0, null,
                    new CoerceValueCallback(CoerceMinWidth)));
        }

        // Get/Set
        public static void SetVisible(DependencyObject obj, bool nVisible)
        {
            obj.SetValue(VisibleProperty, nVisible);
        }

        public static bool GetVisible(DependencyObject obj)
        {
            return (bool)obj.GetValue(VisibleProperty);
        }

        static void OnVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            obj.CoerceValue(ColumnDefinition.WidthProperty);
            obj.CoerceValue(ColumnDefinition.MinWidthProperty);
        }

        static object CoerceWidth(DependencyObject obj, object nValue)
        {
            return (((HideableColumnDefinition)obj).Visible) ? nValue : new GridLength(0);
        }

        static object CoerceMinWidth(DependencyObject obj, object nValue)
        {
            return (((HideableColumnDefinition)obj).Visible) ? nValue : (double)0;
        }
    }
}

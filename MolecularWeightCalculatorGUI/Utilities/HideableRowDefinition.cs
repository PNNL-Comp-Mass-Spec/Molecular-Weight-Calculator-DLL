using System.Windows;
using System.Windows.Controls;

namespace MolecularWeightCalculatorGUI.Utilities
{
    // Sourced from https://www.codeproject.com/Articles/437237/WPF-Grid-Column-and-Row-Hiding
    public class HideableRowDefinition : RowDefinition
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
        static HideableRowDefinition()
        {
            VisibleProperty = DependencyProperty.Register("Visible",
                typeof(bool),
                typeof(HideableRowDefinition),
                new PropertyMetadata(true, new PropertyChangedCallback(OnVisibleChanged)));

            RowDefinition.HeightProperty.OverrideMetadata(typeof(HideableRowDefinition),
                new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null,
                    new CoerceValueCallback(CoerceHeight)));

            RowDefinition.MinHeightProperty.OverrideMetadata(typeof(HideableRowDefinition),
                new FrameworkPropertyMetadata((double)0, null,
                    new CoerceValueCallback(CoerceMinHeight)));
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
            obj.CoerceValue(RowDefinition.HeightProperty);
            obj.CoerceValue(RowDefinition.MinHeightProperty);
        }

        static object CoerceHeight(DependencyObject obj, object nValue)
        {
            return (((HideableRowDefinition)obj).Visible) ? nValue : new GridLength(0);
        }

        static object CoerceMinHeight(DependencyObject obj, object nValue)
        {
            return (((HideableRowDefinition)obj).Visible) ? nValue : (double)0;
        }
    }
}

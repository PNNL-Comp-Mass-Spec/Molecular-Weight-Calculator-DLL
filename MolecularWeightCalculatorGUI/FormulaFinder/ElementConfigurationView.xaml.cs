using System.Windows;
using System.Windows.Controls;

namespace MolecularWeightCalculatorGUI.FormulaFinder
{
    /// <summary>
    /// Interaction logic for ElementConfigurationView.xaml
    /// </summary>
    public partial class ElementConfigurationView : UserControl
    {
        public ElementConfigurationView()
        {
            InitializeComponent();
        }

        public bool MinMaxVisible
        {
            get => (bool)GetValue(MinMaxVisibleProperty);
            set => SetValue(MinMaxVisibleProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinMaxVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinMaxVisibleProperty =
            DependencyProperty.Register("MinMaxVisible", typeof(bool), typeof(ElementConfigurationView), new PropertyMetadata(false));

        public bool PercentVisible
        {
            get => (bool)GetValue(PercentVisibleProperty);
            set => SetValue(PercentVisibleProperty, value);
        }

        // Using a DependencyProperty as the backing store for PercentVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PercentVisibleProperty =
            DependencyProperty.Register("PercentVisible", typeof(bool), typeof(ElementConfigurationView), new PropertyMetadata(false));
    }
}

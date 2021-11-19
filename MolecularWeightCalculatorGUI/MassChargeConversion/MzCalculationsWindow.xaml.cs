using System.Windows;

namespace MolecularWeightCalculatorGUI.MassChargeConversion
{
    /// <summary>
    /// Interaction logic for MzCalculationsWindow.xaml
    /// </summary>
    public partial class MzCalculationsWindow : Window
    {
        public MzCalculationsWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

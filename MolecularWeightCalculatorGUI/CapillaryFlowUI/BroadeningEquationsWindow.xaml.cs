using System.Windows;

namespace MolecularWeightCalculatorGUI.CapillaryFlowUI
{
    /// <summary>
    /// Interaction logic for BroadeningEquationsWindow.xaml
    /// </summary>
    public partial class BroadeningEquationsWindow : Window
    {
        public BroadeningEquationsWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

using System.Windows;

namespace MolecularWeightCalculatorGUI.CapillaryFlowUI
{
    /// <summary>
    /// Interaction logic for CapillaryFlowWindow.xaml
    /// </summary>
    public partial class CapillaryFlowWindow : Window
    {
        public CapillaryFlowWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

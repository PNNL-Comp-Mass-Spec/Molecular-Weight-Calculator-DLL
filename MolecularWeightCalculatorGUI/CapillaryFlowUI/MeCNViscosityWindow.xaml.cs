using System.Windows;

namespace MolecularWeightCalculatorGUI.CapillaryFlowUI
{
    /// <summary>
    /// Interaction logic for MeCNViscosityWindow.xaml
    /// </summary>
    public partial class MeCNViscosityWindow : Window
    {
        public MeCNViscosityWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

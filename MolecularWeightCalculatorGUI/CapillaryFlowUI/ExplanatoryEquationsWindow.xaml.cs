using System.Windows;

namespace MolecularWeightCalculatorGUI.CapillaryFlowUI
{
    /// <summary>
    /// Interaction logic for ExplanatoryEquationsWindow.xaml
    /// </summary>
    public partial class ExplanatoryEquationsWindow : Window
    {
        public ExplanatoryEquationsWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

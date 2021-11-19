using System.Windows;

namespace MolecularWeightCalculatorGUI.Utilities
{
    /// <summary>
    /// Interaction logic for SwitchElementModesWindow.xaml
    /// </summary>
    public partial class SwitchElementModesWindow : Window
    {
        public SwitchElementModesWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

using System.Windows;

namespace MolecularWeightCalculatorGUI.PeptideUI
{
    /// <summary>
    /// Interaction logic for AminoAcidConverterWindow.xaml
    /// </summary>
    public partial class AminoAcidConverterWindow : Window
    {
        public AminoAcidConverterWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

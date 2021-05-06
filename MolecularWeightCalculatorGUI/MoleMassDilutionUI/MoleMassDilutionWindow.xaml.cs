using System.Windows;

namespace MolecularWeightCalculatorGUI.MoleMassDilutionUI
{
    /// <summary>
    /// Interaction logic for MoleMassDilutionWindow.xaml
    /// </summary>
    public partial class MoleMassDilutionWindow : Window
    {
        public MoleMassDilutionWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

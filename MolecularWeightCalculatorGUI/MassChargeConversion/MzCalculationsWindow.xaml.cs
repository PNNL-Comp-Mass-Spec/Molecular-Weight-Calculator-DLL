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

        private void MzCalculationsWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Allow the auto-sizing when the window is first opened, but disable it afterward
            Width = ActualWidth;
            Height = ActualHeight;
            SizeToContent = SizeToContent.Manual;
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;

namespace MolecularWeightCalculatorGUI.FormulaCalc
{
    /// <summary>
    /// Interaction logic for FormulaView.xaml
    /// </summary>
    public partial class FormulaView : UserControl
    {
        public FormulaView()
        {
            InitializeComponent();
        }

        private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is FormulaViewModel fvm)
            {
                fvm.LastFocusTime = DateTime.Now;
            }
        }
    }
}

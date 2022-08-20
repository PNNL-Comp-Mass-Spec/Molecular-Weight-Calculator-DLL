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

        private void Control_OnClickOrGotFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is FormulaViewModel fvm)
            {
                fvm.LastFocusTime = DateTime.Now;
            }
        }

        private void FormulaView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is FormulaViewModel fvmOld)
            {
                fvmOld.SetRichTextBoxControl(null);
            }

            if (e.NewValue is FormulaViewModel fvm)
            {
                fvm.SetRichTextBoxControl(RichText);
            }
        }
    }
}

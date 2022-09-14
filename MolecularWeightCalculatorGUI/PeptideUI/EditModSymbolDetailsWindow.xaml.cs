using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MolecularWeightCalculatorGUI.PeptideUI
{
    /// <summary>
    /// Interaction logic for EditModSymbolDetailsWindow.xaml
    /// </summary>
    public partial class EditModSymbolDetailsWindow : Window
    {
        public EditModSymbolDetailsWindow()
        {
            InitializeComponent();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

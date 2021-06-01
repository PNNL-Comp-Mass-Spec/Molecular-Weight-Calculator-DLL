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

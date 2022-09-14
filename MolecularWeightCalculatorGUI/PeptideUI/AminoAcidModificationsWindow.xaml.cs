using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for AminoAcidModificationsWindow.xaml
    /// </summary>
    public partial class AminoAcidModificationsWindow : Window
    {
        public AminoAcidModificationsWindow()
        {
            InitializeComponent();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (DataContext is AminoAcidModificationsViewModel aamvm)
            {
                aamvm.CancelCommand.Execute(this);
            }
        }
    }
}

using System;
using System.Windows;
using System.Windows.Input;

namespace MolecularWeightCalculatorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CanClose(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MainWindow_OnActivated(object sender, EventArgs e)
        {
            if (DataContext is MainViewModel mvm)
            {
                mvm.WindowActivated();
            }
        }
    }
}

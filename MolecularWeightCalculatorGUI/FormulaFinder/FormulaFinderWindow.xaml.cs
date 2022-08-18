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

namespace MolecularWeightCalculatorGUI.FormulaFinder
{
    /// <summary>
    /// Interaction logic for FormulaFinderWindow.xaml
    /// </summary>
    public partial class FormulaFinderWindow : Window
    {
        public FormulaFinderWindow()
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is FormulaFinderViewModel ffvm && ffvm.IsCalculating)
            {
                ffvm.AbortCommand.Execute();
            }
        }

        private void FormulaFinderWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Load initial size, then change 'SizeToContent' to 'Manual'
            Height = ActualHeight;
            SizeToContent = SizeToContent.Manual;
        }

        private void ResultsBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is FormulaFinderViewModel ffvm)
            {
                if (ResultsBox.SelectedItems.Count > 0)
                {
                    ffvm.SelectedResultSet.Load(ResultsBox.SelectedItems.Cast<FinderResult>());
                }
                else
                {
                    ffvm.SelectedResultSet.Clear();
                }
            }
        }

        private void SelectAll_OnClick(object sender, RoutedEventArgs e)
        {
            ResultsBox.SelectAll();
        }

        private void FormulaFinderWindow_OnActivated(object sender, EventArgs e)
        {
            if (DataContext is FormulaFinderViewModel ffvm)
            {
                ffvm.WindowActivated();
            }
        }
    }
}

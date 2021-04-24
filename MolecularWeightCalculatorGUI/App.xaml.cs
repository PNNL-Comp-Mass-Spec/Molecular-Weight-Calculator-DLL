using System.Windows;
using MolecularWeightCalculatorGUI.FormulaCalc;

namespace MolecularWeightCalculatorGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var mainVm = new MainViewModel();
            var main = new MainWindow { DataContext = mainVm };
            main.Show();
        }
    }
}

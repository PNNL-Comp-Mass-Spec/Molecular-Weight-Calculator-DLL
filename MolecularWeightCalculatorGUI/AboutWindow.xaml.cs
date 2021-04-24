using System.Reflection;
using System.Windows;
using MolecularWeightCalculatorGUI.Properties;

namespace MolecularWeightCalculatorGUI
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Version.Text = version.ToString(2);
            Build.Text = $"(Build {version.Build})";
            Date.Text = AssemblyDetails.GetCommitDate().ToString("MMMM d, yyyy");
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

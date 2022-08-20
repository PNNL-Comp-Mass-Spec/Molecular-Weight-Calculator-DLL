using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
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

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // .NET Framework: UseShellExecute defaults to true, so we don't need to directly set it
            // .NET Core: UseShellExecute defaults to false, and we need ShellExecute
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }

        private void CopyEmailLink(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Email.NavigateUri.AbsoluteUri.Replace("mailto:", ""), TextDataFormat.Text);
        }

        private void CopyGitHubRepoLink(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GitHubRepo.NavigateUri.AbsoluteUri, TextDataFormat.Text);
        }

        private void CopyGitHubPageLink(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GitHubPage.NavigateUri.AbsoluteUri, TextDataFormat.Text);
        }

        private void CopyAlchemistMattPageLink(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AlchemistMatt.NavigateUri.AbsoluteUri, TextDataFormat.Text);
        }
    }
}

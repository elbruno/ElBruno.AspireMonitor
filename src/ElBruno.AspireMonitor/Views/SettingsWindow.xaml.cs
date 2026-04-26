using System.Windows;
using System.Diagnostics;
using System.Windows.Forms;
using ElBruno.AspireMonitor.ViewModels;
using ElBruno.AspireMonitor.Services;

namespace ElBruno.AspireMonitor.Views;

public partial class SettingsWindow : Window
{
    private SettingsViewModel? ViewModel => DataContext as SettingsViewModel;

    public SettingsWindow() : this(null!)
    {
    }

    public SettingsWindow(IConfigurationService configService)
    {
        InitializeComponent();
        if (configService != null)
        {
            DataContext = new SettingsViewModel(configService);
        }
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        using (var dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Select your Aspire project folder";
            dialog.ShowNewFolderButton = false;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (ViewModel != null)
                {
                    ViewModel.ProjectFolder = dialog.SelectedPath;
                }
            }
        }
    }

    private void OpenGitHub_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null && !string.IsNullOrWhiteSpace(ViewModel.RepositoryUrl))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = ViewModel.RepositoryUrl,
                    UseShellExecute = true
                });
            }
            catch
            {
                System.Windows.MessageBox.Show("Could not open the URL. Please check that it is a valid GitHub repository URL.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel?.Validate() == true)
        {
            ViewModel.SaveSettings();
            DialogResult = true;
            Close();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

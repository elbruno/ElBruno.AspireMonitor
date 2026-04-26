using System.Windows;
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

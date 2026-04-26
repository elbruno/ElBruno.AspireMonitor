using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ElBruno.AspireMonitor.ViewModels;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.Infrastructure;

namespace ElBruno.AspireMonitor.Views;

public partial class MainWindow : Window
{
    private MiniMonitorWindow? _miniMonitorWindow;
    private MainViewModel? ViewModel => DataContext as MainViewModel;
    private readonly IConfigurationService? _configService;

    public MainWindow() : this(null, null, null, null)
    {
    }

    public MainWindow(IAspirePollingService? pollingService, IConfigurationService? configService, MainViewModel? viewModel = null, IAspireCommandService? commandService = null)
    {
        InitializeComponent();
        _configService = configService;
        DataContext = viewModel ?? new MainViewModel(pollingService, configService, commandService);
        
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.Start();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Tray icon update is now handled by App.xaml.cs
    }

    private void ShowWindow()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void HideWindow()
    {
        Hide();
    }

    private void ToggleWindow()
    {
        if (IsVisible)
            HideWindow();
        else
            ShowWindow();
    }

    private void ExitApplication()
    {
        ViewModel?.Stop();
        _miniMonitorWindow?.Close();
        // NotifyIcon disposal is now handled by App.xaml.cs
        System.Windows.Application.Current.Shutdown();
    }

    private void ShowSettings()
    {
        // Get config service from ViewModel if not already set
        var configService = _configService;
        if (configService == null && ViewModel != null)
        {
            // Try to get from application's service provider or create default
            configService = new ConfigurationService();
        }
        
        if (configService != null)
        {
            var settingsWindow = new SettingsWindow(configService);
            
            // Only set Owner if this window is visible
            if (IsVisible)
            {
                settingsWindow.Owner = this;
            }
            
            if (settingsWindow.ShowDialog() == true)
            {
                // Reload ProjectFolder from updated config
                if (configService != null && ViewModel != null)
                {
                    var updatedConfig = configService.LoadConfiguration();
                    ViewModel.ProjectFolder = updatedConfig.ProjectFolder ?? string.Empty;
                    // Push the new working directory into the Aspire CLI service
                    // so 'aspire describe' runs from the correct folder.
                    (System.Windows.Application.Current as App)?.UpdateAspireWorkingDirectory(updatedConfig.ProjectFolder);
                }
                // Configuration changed, restart polling service
                ViewModel?.Stop();
                ViewModel?.Start();
            }
        }
    }

    private void ToggleMiniMonitor()
    {
        if (_miniMonitorWindow == null)
        {
            var miniMonitorVm = new MiniMonitorViewModel(ViewModel);
            _miniMonitorWindow = new MiniMonitorWindow
            {
                DataContext = miniMonitorVm
            };
            
            // Set the MiniMonitorViewModel reference in MainViewModel
            if (ViewModel != null)
            {
                ViewModel.MiniMonitorViewModel = miniMonitorVm;
            }
            
            _miniMonitorWindow.Closed += (s, e) => 
            {
                // Clear the reference when the MiniMonitorWindow closes
                if (ViewModel != null)
                {
                    ViewModel.MiniMonitorViewModel = null;
                }
                _miniMonitorWindow = null;
            };
            _miniMonitorWindow.Show();
        }
        else if (_miniMonitorWindow.IsVisible)
        {
            _miniMonitorWindow.Hide();
        }
        else
        {
            _miniMonitorWindow.Show();
            _miniMonitorWindow.Activate();
        }
    }

    private void OpenGitHub()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/elbruno/ElBruno.AspireMonitor",
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail if browser can't be opened
        }
    }

    private void Window_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            HideWindow();
        }
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        HideWindow();
    }

    private void HostUrl_Click(object sender, MouseButtonEventArgs e)
    {
        if (ViewModel?.HostUrl != null)
        {
            OpenUrl(ViewModel.HostUrl);
        }
    }

    private void ResourceUrl_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBlock textBlock && 
            textBlock.DataContext is ResourceViewModel resource && 
            !string.IsNullOrEmpty(resource.Url))
        {
            OpenUrl(resource.Url);
        }
    }

    private void OpenUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return;

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail for now
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.RefreshCommand?.Execute(null);
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        HideWindow();
    }

    private void MiniMonitor_Click(object sender, RoutedEventArgs e)
    {
        ToggleMiniMonitor();
    }

    private void Resource_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.Border border && 
            border.DataContext is ResourceViewModel resource)
        {
            ViewModel?.SelectResource(resource);
        }
    }

    private void ClearLogs_Click(object sender, RoutedEventArgs e)
    {
        ViewModel?.ClearLogs();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (ViewModel != null)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            ViewModel.Stop();
        }
        _miniMonitorWindow?.Close();
        // NotifyIcon disposal is now handled by App.xaml.cs
        base.OnClosed(e);
    }
}

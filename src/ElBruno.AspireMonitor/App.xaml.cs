using System.Windows;
using ElBruno.AspireMonitor.Models;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.ViewModels;
using ElBruno.AspireMonitor.Views;

namespace ElBruno.AspireMonitor;

public partial class App : System.Windows.Application
{
    private IAspirePollingService? _pollingService;
    private IConfigurationService? _configService;
    private AspireApiClient? _apiClient;
    private IAspireCommandService? _commandService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Initialize configuration service
        _configService = new ConfigurationService();
        var configuration = _configService.LoadConfiguration();
        
        // Initialize API client and polling service
        _apiClient = new AspireApiClient(configuration);
        _pollingService = new AspirePollingService(_apiClient, configuration);
        _commandService = new AspireCommandService();
        
        // Create MainViewModel and MainWindow with dependencies
        var viewModel = new MainViewModel(_pollingService, _configService, _commandService);
        var mainWindow = new MainWindow(_pollingService, _configService, viewModel, _commandService);
        
        // Set as application main window
        MainWindow = mainWindow;
        
        // Hide the main window on startup (run silently in background)
        // Only the system tray icon should be visible
        MainWindow.Visibility = Visibility.Hidden;
        MainWindow.ShowInTaskbar = false;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Clean up resources
        _pollingService?.Stop();
        (_pollingService as IDisposable)?.Dispose();
        _apiClient?.Dispose();
        
        base.OnExit(e);
    }
}

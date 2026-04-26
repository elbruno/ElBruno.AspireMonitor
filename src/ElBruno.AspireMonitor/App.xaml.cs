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
    private AspireCliService? _cliService;
    private AspireLiveLogsService? _logsService;
    private IAspireCommandService? _commandService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        System.Diagnostics.Debug.WriteLine("[App] ========== APPLICATION STARTUP ==========");
        System.Diagnostics.Debug.WriteLine("[App] Initializing CLI-based Aspire Monitor...");
        
        // Initialize configuration service
        _configService = new ConfigurationService();
        var configuration = _configService.LoadConfiguration();
        
        System.Diagnostics.Debug.WriteLine($"[App] Loaded configuration:");
        System.Diagnostics.Debug.WriteLine($"[App]   Polling Interval: {configuration.PollingIntervalMs}ms");
        
        // Initialize CLI service (NO HTTP)
        System.Diagnostics.Debug.WriteLine("[App] Creating CLI service...");
        _cliService = new AspireCliService();
        
        // Initialize live logs service
        System.Diagnostics.Debug.WriteLine("[App] Creating live logs service...");
        _logsService = new AspireLiveLogsService(_cliService);
        
        // Initialize polling service with CLI backend
        System.Diagnostics.Debug.WriteLine("[App] Creating polling service with CLI backend...");
        _pollingService = new AspirePollingService(_cliService, configuration.PollingIntervalMs);
        
        // Initialize command service
        _commandService = new AspireCommandService();
        
        System.Diagnostics.Debug.WriteLine("[App] Creating MainViewModel and MainWindow...");
        // Create MainViewModel and MainWindow with dependencies
        var viewModel = new MainViewModel(_pollingService, _configService, _commandService);
        var mainWindow = new MainWindow(_pollingService, _configService, viewModel, _commandService);
        
        // Set as application main window
        MainWindow = mainWindow;
        
        // Hide the main window on startup (run silently in background)
        // Only the system tray icon should be visible
        MainWindow.Visibility = Visibility.Hidden;
        MainWindow.ShowInTaskbar = false;
        
        // Start polling
        System.Diagnostics.Debug.WriteLine("[App] Starting polling service...");
        _pollingService.Start();
        
        // Subscribe to errors to show user notifications
        _pollingService.ErrorOccurred += (sender, error) =>
        {
            System.Diagnostics.Debug.WriteLine($"[App] Polling error: {error}");
            if (error.Contains("not found") || error.Contains("not running"))
            {
                System.Diagnostics.Debug.WriteLine("[App] Aspire CLI or app not available");
            }
        };
        
        System.Diagnostics.Debug.WriteLine("[App] ========== STARTUP COMPLETE (CLI MODE) ==========");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("[App] ========== APPLICATION SHUTDOWN ==========");
        
        // Clean up resources
        _pollingService?.Stop();
        (_pollingService as IDisposable)?.Dispose();
        _logsService?.Dispose();
        
        base.OnExit(e);
    }
}

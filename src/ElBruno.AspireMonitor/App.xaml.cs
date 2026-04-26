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
        
        System.Diagnostics.Debug.WriteLine("[App] ========== APPLICATION STARTUP ==========");
        System.Diagnostics.Debug.WriteLine("[App] Initializing configuration service...");
        
        // Initialize configuration service
        _configService = new ConfigurationService();
        var configuration = _configService.LoadConfiguration();
        
        System.Diagnostics.Debug.WriteLine($"[App] Loaded configuration:");
        System.Diagnostics.Debug.WriteLine($"[App]   Aspire Endpoint: {configuration.AspireEndpoint}");
        System.Diagnostics.Debug.WriteLine($"[App]   Polling Interval: {configuration.PollingIntervalMs}ms");
        
        // Try to auto-detect running Aspire endpoint
        _commandService = new AspireCommandService();
        try
        {
            System.Diagnostics.Debug.WriteLine("[App] Attempting to auto-detect Aspire endpoint...");
            var detectTask = _commandService.DetectAspireEndpointAsync();
            if (detectTask.Wait(TimeSpan.FromSeconds(5)))
            {
                var detectedEndpoint = detectTask.Result;
                if (!string.IsNullOrWhiteSpace(detectedEndpoint))
                {
                    configuration.AspireEndpoint = detectedEndpoint;
                    _configService.SaveConfiguration(configuration);
                    System.Diagnostics.Debug.WriteLine($"[App] ✓ Auto-detected and saved Aspire endpoint: {detectedEndpoint}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[App] Auto-detect returned empty endpoint");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[App] Auto-detect timed out");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Failed to auto-detect endpoint: {ex.GetType().Name}: {ex.Message}");
        }
        
        // If detection failed, try to find Aspire by probing common ports
        System.Diagnostics.Debug.WriteLine("[App] Checking if current endpoint is responding...");
        if (!IsAspireEndpointResponding(configuration.AspireEndpoint))
        {
            System.Diagnostics.Debug.WriteLine($"[App] Current endpoint {configuration.AspireEndpoint} not responding, probing common ports...");
            var foundEndpoint = ProbeForAspireEndpoint();
            if (!string.IsNullOrWhiteSpace(foundEndpoint))
            {
                configuration.AspireEndpoint = foundEndpoint;
                _configService.SaveConfiguration(configuration);
                System.Diagnostics.Debug.WriteLine($"[App] ✓ Found Aspire endpoint via probing: {foundEndpoint}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[App] No Aspire endpoint found via probing");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[App] ✓ Current endpoint is responding: {configuration.AspireEndpoint}");
        }
        
        // Initialize API client and polling service
        System.Diagnostics.Debug.WriteLine("[App] Creating API client and polling service...");
        _apiClient = new AspireApiClient(configuration);
        _pollingService = new AspirePollingService(_apiClient, configuration);
        
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
        
        System.Diagnostics.Debug.WriteLine("[App] ========== STARTUP COMPLETE ==========");
    }

    private bool IsAspireEndpointResponding(string endpoint)
    {
        try
        {
            var handler = new System.Net.Http.HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            
            using (var client = new System.Net.Http.HttpClient(handler) { Timeout = TimeSpan.FromSeconds(2) })
            {
                var response = client.GetAsync($"{endpoint}/api/health").Result;
                return response.IsSuccessStatusCode;
            }
        }
        catch
        {
            return false;
        }
    }

    private string? ProbeForAspireEndpoint()
    {
        // Common Aspire dashboard ports (typically in range 17000-18000)
        var commonPorts = new[] { 17195, 17196, 17197, 17000, 17500, 18000, 18888 };
        
        foreach (var port in commonPorts)
        {
            // Try HTTPS first (Aspire dashboard uses HTTPS)
            var endpoint = $"https://localhost:{port}";
            if (IsAspireEndpointResponding(endpoint))
            {
                System.Diagnostics.Debug.WriteLine($"[App] Found Aspire endpoint: {endpoint}");
                return endpoint;
            }
            
            // Fallback to HTTP
            endpoint = $"http://localhost:{port}";
            if (IsAspireEndpointResponding(endpoint))
            {
                System.Diagnostics.Debug.WriteLine($"[App] Found Aspire endpoint: {endpoint}");
                return endpoint;
            }
        }
        
        return null;
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

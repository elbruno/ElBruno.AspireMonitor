using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ElBruno.AspireMonitor.Infrastructure;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.Models;
using ElBruno.AspireMonitor.Helpers;

namespace ElBruno.AspireMonitor.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IAspirePollingService? _pollingService;
    private readonly IConfigurationService? _configService;
    private readonly IAspireCommandService? _commandService;
    private readonly AspireLiveLogsService? _liveLogsService;
    private string _currentStatus = "Disconnected";
    private bool _isConnected;
    private DateTime _lastUpdated = DateTime.Now;
    private ObservableCollection<ResourceViewModel> _resources = new();
    private string _projectFolder = string.Empty;
    private string _miniWindowResourcesSetting = string.Empty;
    private bool _showMiniWindowResourceTelemetry = true;
    private bool _showOnlyMainMiniWindowResources = true;
    private string _hostUrl = Configuration.DefaultAspireEndpoint;
    private bool _isExecutingCommand;
    private string _commandStatus = string.Empty;
    private MiniMonitorViewModel? _miniMonitorViewModel;
    private ResourceViewModel? _selectedResource;
    private ObservableCollection<string> _logLines = new();
    private bool _isLogPaused;
    private bool _hostUrlDetected;
    private string? _activeLogStreamResourceName;
    private bool _suppressSelectedResourceLogReload;
    private const int MaxLogLines = 50;

    public MainViewModel() : this(null, null, null)
    {
        // Design-time constructor
        InitializeSampleData();
    }

    public MainViewModel(
        IAspirePollingService? pollingService,
        IConfigurationService? configService,
        IAspireCommandService? commandService = null,
        AspireLiveLogsService? liveLogsService = null)
    {
        _pollingService = pollingService;
        _configService = configService;
        _commandService = commandService;
        _liveLogsService = liveLogsService;
        
        RefreshCommand = new RelayCommand(_ => RefreshData());
        OpenUrlCommand = new RelayCommand(param => OpenUrl(param?.ToString() ?? string.Empty));
        StartAspireCommand = new RelayCommand(_ => _ = StartAspireAsync(), _ => !_isExecutingCommand && !_isConnected);
        StopAspireCommand = new RelayCommand(_ => _ = StopAspireAsync(), _ => !_isExecutingCommand && _isConnected);
        
        // Load project folder from config
        if (_configService != null)
        {
            var config = _configService.LoadConfiguration();
            HostUrl = string.IsNullOrWhiteSpace(config.AspireEndpoint)
                ? Configuration.DefaultAspireEndpoint
                : config.AspireEndpoint;
            ProjectFolder = config.ProjectFolder ?? string.Empty;
            MiniWindowResourcesSetting = config.MiniWindowResources ?? string.Empty;
            ShowMiniWindowResourceTelemetry = config.ShowMiniWindowResourceTelemetry;
            ShowOnlyMainMiniWindowResources = config.ShowOnlyMainMiniWindowResources;
        }
        
        if (_pollingService != null)
        {
            _pollingService.ResourcesUpdated += OnResourcesUpdated;
            _pollingService.StatusChanged += OnStatusChanged;
            _pollingService.ErrorOccurred += OnError;
        }

        if (_liveLogsService != null)
        {
            _liveLogsService.LogLineReceived += OnLiveLogLineReceived;
            _liveLogsService.LogStreamClosed += OnLiveLogStreamClosed;
            _liveLogsService.ErrorOccurred += OnLiveLogsError;
        }
    }

    public string AppVersion => VersionHelper.GetAppVersion();

    public string AppVersionTitle => $"Aspire Monitor {AppVersion}";

    public string CurrentStatus
    {
        get => _currentStatus;
        set
        {
            if (SetProperty(ref _currentStatus, value))
            {
                OnPropertyChanged(nameof(ConnectionStatus));
                OnPropertyChanged(nameof(ErrorTitle));
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(OverallStatusColor));
            }
        }
    }

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (SetProperty(ref _isConnected, value))
            {
                OnPropertyChanged(nameof(ConnectionStatus));
                OnPropertyChanged(nameof(ErrorTitle));
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(OverallStatusColor));
                OnPropertyChanged(nameof(LogEmptyStateMessage));
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string ConnectionStatus => CurrentStatus;

    public string ErrorTitle => IsConnected ? "" : "Aspire Not Connected";

    public string ErrorMessage
    {
        get
        {
            if (IsConnected)
                return "";

            if (CurrentStatus.Contains("Error"))
                return $"{CurrentStatus}. Retrying...";

            return "No Aspire instance found. Start Aspire with: aspire start";
        }
    }

    public System.Windows.Media.Brush OverallStatusColor
    {
        get
        {
            if (!IsConnected)
                return System.Windows.Media.Brushes.Gray;

            // Calculate overall status based on resource health
            var redBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF4, 0x43, 0x36));
            var yellowBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xC1, 0x07));
            var greenBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50));

            foreach (var resource in Resources)
            {
                var brush = resource.StatusColor as SolidColorBrush;
                if (brush != null && brush.Color == redBrush.Color)
                    return redBrush;
            }
            
            foreach (var resource in Resources)
            {
                var brush = resource.StatusColor as SolidColorBrush;
                if (brush != null && brush.Color == yellowBrush.Color)
                    return yellowBrush;
            }
            
            return greenBrush;
        }
    }

    public DateTime LastUpdated
    {
        get => _lastUpdated;
        set
        {
            if (SetProperty(ref _lastUpdated, value))
            {
                OnPropertyChanged(nameof(LastUpdatedText));
            }
        }
    }

    public string LastUpdatedText => $"Last updated: {LastUpdated:HH:mm:ss}";

    public ObservableCollection<ResourceViewModel> Resources
    {
        get => _resources;
        set => SetProperty(ref _resources, value);
    }

    public string ProjectFolder
    {
        get => _projectFolder;
        set
        {
            if (SetProperty(ref _projectFolder, value))
            {
                OnPropertyChanged(nameof(ProjectFolderDisplay));
            }
        }
    }

    public string MiniWindowResourcesSetting
    {
        get => _miniWindowResourcesSetting;
        set => SetProperty(ref _miniWindowResourcesSetting, value);
    }

    public bool ShowMiniWindowResourceTelemetry
    {
        get => _showMiniWindowResourceTelemetry;
        set => SetProperty(ref _showMiniWindowResourceTelemetry, value);
    }

    public bool ShowOnlyMainMiniWindowResources
    {
        get => _showOnlyMainMiniWindowResources;
        set => SetProperty(ref _showOnlyMainMiniWindowResources, value);
    }

    public string ProjectFolderDisplay => PathHumanizer.Humanize(_projectFolder, 50);

    public string HostUrl
    {
        get => _hostUrl;
        set => SetProperty(ref _hostUrl, value);
    }

    public string CommandStatus
    {
        get => _commandStatus;
        set => SetProperty(ref _commandStatus, value);
    }

    public bool IsExecutingCommand
    {
        get => _isExecutingCommand;
        set
        {
            if (SetProperty(ref _isExecutingCommand, value))
            {
                // Trigger requery of commands
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public ICommand? RefreshCommand { get; }
    public ICommand? OpenUrlCommand { get; }
    public ICommand? StartAspireCommand { get; }
    public ICommand? StopAspireCommand { get; }

    public MiniMonitorViewModel? MiniMonitorViewModel
    {
        get => _miniMonitorViewModel;
        set => _miniMonitorViewModel = value;
    }

    public ResourceViewModel? SelectedResource
    {
        get => _selectedResource;
        set
        {
            if (SetProperty(ref _selectedResource, value))
            {
                OnPropertyChanged(nameof(LogHeader));
                OnPropertyChanged(nameof(LogStatus));
                OnPropertyChanged(nameof(LogEmptyStateMessage));

                if (!_suppressSelectedResourceLogReload)
                {
                    LoadResourceLogs();
                }
            }
        }
    }

    public ObservableCollection<string> LogLines
    {
        get => _logLines;
        set => SetProperty(ref _logLines, value);
    }

    public bool IsLogPaused
    {
        get => _isLogPaused;
        set
        {
            if (SetProperty(ref _isLogPaused, value))
            {
                OnPropertyChanged(nameof(LogStatus));
                OnPropertyChanged(nameof(LogEmptyStateMessage));
            }
        }
    }

    public string LogHeader => SelectedResource != null 
        ? $"Logs - {SelectedResource.Name}" 
        : "Logs - Select a resource";

    public string LogStatus => $"{LogLines.Count} lines | {(IsLogPaused ? "Paused" : "Live")}";

    public bool HasLogLines => LogLines.Count > 0;

    public string LogEmptyStateMessage
    {
        get
        {
            if (!IsConnected)
            {
                return "Aspire is not running yet. Start Aspire to stream live logs.";
            }

            if (SelectedResource == null)
            {
                return _liveLogsService == null
                    ? "Live log streaming is unavailable."
                    : "Select a resource to begin streaming live logs.";
            }

            if (_liveLogsService == null)
            {
                return $"Live log streaming is unavailable for {SelectedResource.Name}.";
            }

            if (IsLogPaused)
            {
                return $"Live logs are paused for {SelectedResource.Name}.";
            }

            if (LogLines.Count == 0)
            {
                return $"Waiting for live output from {SelectedResource.Name}...";
            }

            return $"Showing live output for {SelectedResource.Name}.";
        }
    }

    private void RefreshData()
    {
        ReloadMiniWindowConfiguration();

        if (_pollingService != null)
        {
            _ = _pollingService.RefreshAsync();
        }
        else
        {
            // Design-time: Update sample data with random values
            LastUpdated = DateTime.Now;
            var random = new Random();
            foreach (var resource in Resources)
            {
                resource.CpuUsage = random.Next(0, 100);
                resource.MemoryUsage = random.Next(0, 100);
                resource.DiskUsagePercent = random.Next(0, 100);
            }
            OnPropertyChanged(nameof(OverallStatusColor));
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Failed to open URL '{url}': {ex.Message}");
        }
    }

    private void OnResourcesUpdated(object? sender, List<AspireResource> resources)
    {
        InvokeOnUiThread(() =>
        {
            var selectedResourceName = SelectedResource?.Name;
            var config = _configService?.LoadConfiguration();
            var hideDevelopmentResources = config?.HideDevelopmentResources ?? false;
            ReloadMiniWindowConfiguration(config);
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] OnResourcesUpdated received: {resources.Count} resources");

            Resources.Clear();
            foreach (var resource in resources)
            {
                // Get primary endpoint URL
                var endpoints = resource.Endpoints ?? new List<AspireEndpoint>();
                var primaryEndpoint = endpoints.FirstOrDefault();
                string? url = primaryEndpoint?.DisplayUrl;

                var metrics = resource.Metrics ?? new ResourceMetrics();
                var environment = resource.Environment ?? new List<AspireEnvironmentEntry>();
                var resourceViewModel = new ResourceViewModel
                {
                    Name = resource.Name,
                    ResourceType = resource.Type,
                    Status = resource.Status,
                    CpuUsage = metrics.CpuUsagePercent,
                    MemoryUsage = metrics.MemoryUsagePercent,
                    DiskUsagePercent = metrics.DiskUsagePercent,
                    EndpointCount = endpoints.Count,
                    Url = url,
                    Environment = environment
                };

                if (hideDevelopmentResources && resourceViewModel.IsDevelopmentOnly)
                {
                    continue;
                }
                
                Resources.Add(resourceViewModel);
                System.Diagnostics.Debug.WriteLine($"[MainViewModel]   Added resource: {resource.Name} (CPU: {metrics.CpuUsagePercent:F1}%, Mem: {metrics.MemoryUsagePercent:F1}%)");
            }
            
            LastUpdated = DateTime.Now;
            IsConnected = Resources.Count > 0;
            OnPropertyChanged(nameof(OverallStatusColor));

            // Auto-detect the AppHost dashboard URL the first time we see Aspire running, so the
            // mini and main windows can show a clickable link without requiring the user to start
            // Aspire from the app itself.
            if (IsConnected && !_hostUrlDetected && _commandService != null)
            {
                _hostUrlDetected = true;
                _ = DetectAndUpdateHostUrlAsync();
            }
            else if (!IsConnected)
            {
                _hostUrlDetected = false;
            }

            System.Diagnostics.Debug.WriteLine($"[MainViewModel] UI updated with {Resources.Count} resources, IsConnected={IsConnected}");

            if (!string.IsNullOrWhiteSpace(selectedResourceName))
            {
                var updatedResource = Resources.FirstOrDefault(r => r.Name == selectedResourceName);
                if (updatedResource != null)
                {
                    if (!ReferenceEquals(SelectedResource, updatedResource))
                    {
                        _suppressSelectedResourceLogReload = true;
                        try
                        {
                            SelectedResource = updatedResource;
                        }
                        finally
                        {
                            _suppressSelectedResourceLogReload = false;
                        }
                    }

                    UpdateSelectedResourceDetails(updatedResource);
                }
                else if (SelectedResource != null)
                {
                    _suppressSelectedResourceLogReload = true;
                    try
                    {
                        SelectedResource = null;
                    }
                    finally
                    {
                        _suppressSelectedResourceLogReload = false;
                    }

                    StopActiveLogStream();
                    ClearLogs();
                }
            }
        });
    }

    private void ReloadMiniWindowConfiguration(Configuration? config = null)
    {
        if (_configService == null)
            return;

        config ??= _configService.LoadConfiguration();
        MiniWindowResourcesSetting = config.MiniWindowResources ?? string.Empty;
        ShowMiniWindowResourceTelemetry = config.ShowMiniWindowResourceTelemetry;
        ShowOnlyMainMiniWindowResources = config.ShowOnlyMainMiniWindowResources;
    }

    private void OnStatusChanged(object? sender, string status)
    {
        InvokeOnUiThread(() =>
        {
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] OnStatusChanged: {status}");
            CurrentStatus = status;
            IsConnected = status == "Connected";

            // When Aspire is reported as not running, reset the dashboard URL to the default
            // so we re-detect it when Aspire starts again.
            if (status == "Not Running")
            {
                HostUrl = Configuration.DefaultAspireEndpoint;
                _hostUrlDetected = false;
            }
        });
    }

    private void OnError(object? sender, string error)
    {
        InvokeOnUiThread(() =>
        {
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] OnError: {error}");
            CurrentStatus = $"Error: {error}";
            IsConnected = false;
        });
    }

    private static void InvokeOnUiThread(Action action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

        if (dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }

    private async Task DetectAndUpdateHostUrlAsync()
    {
        try
        {
            if (_commandService == null) return;
            var endpoint = await _commandService.DetectAspireEndpointAsync();
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    HostUrl = endpoint!;
                    System.Diagnostics.Debug.WriteLine($"[MainViewModel] Detected dashboard URL: {endpoint}");
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Dashboard URL detection failed: {ex.Message}");
            _hostUrlDetected = false;
        }
    }

    public void Start()
    {
        System.Diagnostics.Debug.WriteLine("[MainViewModel] Starting polling...");
        _pollingService?.Start();
        // Trigger initial refresh to get resources immediately
        _ = _pollingService?.RefreshAsync();
    }

    public void Stop()
    {
        StopActiveLogStream();
        _pollingService?.Stop();
    }

    private async Task StartAspireAsync()
    {
        if (_commandService == null || string.IsNullOrWhiteSpace(ProjectFolder))
        {
            CommandStatus = "❌ Project folder not configured";
            return;
        }

        try
        {
            IsExecutingCommand = true;
            CommandStatus = "🚀 Starting Aspire...";
            
            // Clear logs before starting new command
            _miniMonitorViewModel?.ClearLog();
            
            var success = await _commandService.StartAspireAsync(ProjectFolder, _miniMonitorViewModel?.LogCallback);
            
            if (success)
            {
                CommandStatus = "🔍 Detecting Aspire endpoint...";
                
                // Wait a moment then detect the endpoint from running Aspire instance
                await Task.Delay(2000);
                var endpoint = await _commandService.DetectAspireEndpointAsync(_miniMonitorViewModel?.LogCallback);
                
                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    // CRITICAL: Update the polling service's API client endpoint before refreshing
                    if (_pollingService is AspirePollingService pollingService)
                    {
                        pollingService.UpdateEndpoint(endpoint);
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"[MainViewModel] Aspire started with endpoint: {endpoint}");
                    
                    // Trigger refresh to immediately connect
                    _pollingService?.RefreshAsync();

                    // Wait for resources to actually appear (Aspire can take 30-60s to spin up)
                    // Keep IsExecutingCommand = true so START button stays disabled while we wait.
                    const int readinessTimeoutSeconds = 90;
                    var readinessStart = DateTime.UtcNow;
                    while ((DateTime.UtcNow - readinessStart).TotalSeconds < readinessTimeoutSeconds)
                    {
                        if (Resources.Count > 0)
                        {
                            break;
                        }
                        var elapsed = (int)(DateTime.UtcNow - readinessStart).TotalSeconds;
                        CommandStatus = $"⏳ Starting Aspire... ({elapsed}s / {readinessTimeoutSeconds}s)";
                        await Task.Delay(1000);
                    }

                    if (Resources.Count > 0)
                    {
                        CommandStatus = "✅ Aspire started successfully";
                    }
                    else
                    {
                        CommandStatus = "⚠️ Aspire is taking longer than expected — still waiting for resources";
                        System.Diagnostics.Debug.WriteLine("[MainViewModel] Aspire start timed out waiting for resources");
                    }
                }
                else
                {
                    CommandStatus = "⚠️ Aspire started but endpoint not detected";
                    System.Diagnostics.Debug.WriteLine("[MainViewModel] Aspire started but could not detect endpoint");
                }
            }
            else
            {
                CommandStatus = "❌ Failed to start Aspire";
                System.Diagnostics.Debug.WriteLine("[MainViewModel] Failed to start Aspire");
            }
        }
        catch (Exception ex)
        {
            CommandStatus = $"❌ Error: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Error starting Aspire: {ex.Message}");
        }
        finally
        {
            IsExecutingCommand = false;
            // Clear status after 5 seconds
            _ = Task.Delay(5000).ContinueWith(_ =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (CommandStatus != "✅ Aspire started successfully" && !CommandStatus.StartsWith("🚀"))
                        CommandStatus = string.Empty;
                });
            });
        }
    }

    private async Task StopAspireAsync()
    {
        if (_commandService == null)
        {
            CommandStatus = "❌ Command service not available";
            return;
        }

        try
        {
            IsExecutingCommand = true;
            CommandStatus = "⏹️ Stopping Aspire...";
            
            // Clear logs before starting new command
            _miniMonitorViewModel?.ClearLog();
            
            var success = await _commandService.StopAspireAsync(_miniMonitorViewModel?.LogCallback);
            
            if (success)
            {
                CommandStatus = "✅ Aspire stopped successfully";
                System.Diagnostics.Debug.WriteLine("[MainViewModel] Aspire stopped successfully");
            }
            else
            {
                CommandStatus = "❌ Failed to stop Aspire";
                System.Diagnostics.Debug.WriteLine("[MainViewModel] Failed to stop Aspire");
            }
        }
        catch (Exception ex)
        {
            CommandStatus = $"❌ Error: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Error stopping Aspire: {ex.Message}");
        }
        finally
        {
            IsExecutingCommand = false;
            // Clear status after 5 seconds
            _ = Task.Delay(5000).ContinueWith(_ =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!CommandStatus.Contains("Error") && CommandStatus != "✅ Aspire stopped successfully")
                        CommandStatus = string.Empty;
                });
            });
        }
    }

    private void InitializeSampleData()
    {
        Resources.Add(new ResourceViewModel
        {
            Name = "webfrontend",
            ResourceType = "Container",
            Status = Models.ResourceStatus.Running,
            CpuUsage = 45.2,
            MemoryUsage = 62.8,
            DiskUsagePercent = 12.5,
            EndpointCount = 1,
            Url = "http://localhost:5000",
            Environment = new List<AspireEnvironmentEntry>
            {
                new()
                {
                    Name = "ASPNETCORE_ENVIRONMENT",
                    Value = "Development"
                }
            }
        });

        Resources.Add(new ResourceViewModel
        {
            Name = "apiservice",
            ResourceType = "Project",
            Status = Models.ResourceStatus.Running,
            CpuUsage = 28.5,
            MemoryUsage = 48.3,
            DiskUsagePercent = 7.3,
            EndpointCount = 2,
            Url = "http://localhost:5001"
        });

        Resources.Add(new ResourceViewModel
        {
            Name = "cache",
            ResourceType = "Container",
            Status = Models.ResourceStatus.Running,
            CpuUsage = 12.1,
            MemoryUsage = 35.7,
            DiskUsagePercent = 1.2,
            EndpointCount = 0,
            Url = null
        });

        IsConnected = true;
    }

    public void SelectResource(ResourceViewModel resource)
    {
        // Deselect all resources first
        foreach (var r in Resources)
        {
            r.IsSelected = false;
        }

        // Select the clicked resource
        resource.IsSelected = true;
        SelectedResource = resource;
    }

    public void ClearLogs()
    {
        LogLines.Clear();
        OnPropertyChanged(nameof(LogStatus));
        OnPropertyChanged(nameof(HasLogLines));
        OnPropertyChanged(nameof(LogEmptyStateMessage));
    }

    private void LoadResourceLogs()
    {
        StopActiveLogStream();
        LogLines.Clear();
        OnPropertyChanged(nameof(HasLogLines));
        OnPropertyChanged(nameof(LogEmptyStateMessage));

        if (SelectedResource == null)
        {
            OnPropertyChanged(nameof(LogStatus));
            return;
        }

        AddLogLine($"[{DateTime.Now:HH:mm:ss}] Streaming live logs for: {SelectedResource.Name}");

        if (_liveLogsService == null)
        {
            AddLogLine($"[{DateTime.Now:HH:mm:ss}] Live log streaming is unavailable.");
            OnPropertyChanged(nameof(LogStatus));
            OnPropertyChanged(nameof(LogEmptyStateMessage));
            return;
        }

        _activeLogStreamResourceName = SelectedResource.Name;
        _ = _liveLogsService.StartStreamingAsync(SelectedResource.Name);
        OnPropertyChanged(nameof(LogStatus));
        OnPropertyChanged(nameof(LogEmptyStateMessage));
    }

    private void AddLogLine(string line)
    {
        if (IsLogPaused)
            return;

        var dispatcher = System.Windows.Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

        if (dispatcher.CheckAccess())
        {
            LogLines.Add(line);

            // Keep only the last 50 lines
            while (LogLines.Count > MaxLogLines)
            {
                LogLines.RemoveAt(0);
            }

            OnPropertyChanged(nameof(LogStatus));
            OnPropertyChanged(nameof(HasLogLines));
            OnPropertyChanged(nameof(LogEmptyStateMessage));
        }
        else
        {
            dispatcher.Invoke(() =>
            {
                LogLines.Add(line);

                while (LogLines.Count > MaxLogLines)
                {
                    LogLines.RemoveAt(0);
                }

                OnPropertyChanged(nameof(LogStatus));
                OnPropertyChanged(nameof(HasLogLines));
                OnPropertyChanged(nameof(LogEmptyStateMessage));
            });
        }
    }

    private void UpdateSelectedResourceDetails(ResourceViewModel resource)
    {
        if (SelectedResource == null)
            return;

        SelectedResource.Name = resource.Name;
        SelectedResource.ResourceType = resource.ResourceType;
        SelectedResource.Status = resource.Status;
        SelectedResource.CpuUsage = resource.CpuUsage;
        SelectedResource.MemoryUsage = resource.MemoryUsage;
        SelectedResource.DiskUsagePercent = resource.DiskUsagePercent;
        SelectedResource.EndpointCount = resource.EndpointCount;
        SelectedResource.Url = resource.Url;
        SelectedResource.Environment = resource.Environment;
    }

    private void StopActiveLogStream()
    {
        if (_liveLogsService != null && !string.IsNullOrWhiteSpace(_activeLogStreamResourceName))
        {
            _liveLogsService.StopStreaming(_activeLogStreamResourceName);
        }

        _activeLogStreamResourceName = null;
    }

    private void OnLiveLogLineReceived(object? sender, LogLineReceivedEventArgs args)
    {
        if (!string.Equals(args.ResourceName, _activeLogStreamResourceName, StringComparison.OrdinalIgnoreCase))
            return;

        AddLogLine(args.LogLine);
    }

    private void OnLiveLogStreamClosed(object? sender, LogStreamClosedEventArgs args)
    {
        if (!string.Equals(args.ResourceName, _activeLogStreamResourceName, StringComparison.OrdinalIgnoreCase))
            return;

        _activeLogStreamResourceName = null;

        if (args.IsError && !string.IsNullOrWhiteSpace(args.ErrorMessage))
        {
            AddLogLine($"[{DateTime.Now:HH:mm:ss}] Log stream closed: {args.ErrorMessage}");
        }

        OnPropertyChanged(nameof(LogStatus));
        OnPropertyChanged(nameof(LogEmptyStateMessage));
    }

    private void OnLiveLogsError(object? sender, string error)
    {
        if (!string.IsNullOrWhiteSpace(_activeLogStreamResourceName))
        {
            AddLogLine($"[{DateTime.Now:HH:mm:ss}] {error}");
        }
    }
}

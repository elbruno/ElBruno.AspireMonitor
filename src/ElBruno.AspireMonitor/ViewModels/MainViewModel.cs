using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using ElBruno.AspireMonitor.Infrastructure;
using ElBruno.AspireMonitor.Services;
using ElBruno.AspireMonitor.Models;

namespace ElBruno.AspireMonitor.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IAspirePollingService? _pollingService;
    private readonly IConfigurationService? _configService;
    private readonly IAspireCommandService? _commandService;
    private string _currentStatus = "Disconnected";
    private bool _isConnected;
    private DateTime _lastUpdated = DateTime.Now;
    private ObservableCollection<ResourceViewModel> _resources = new();
    private string _projectFolder = string.Empty;
    private string _hostUrl = "http://localhost:18888";
    private bool _isExecutingCommand;
    private string _commandStatus = string.Empty;
    private MiniMonitorViewModel? _miniMonitorViewModel;
    private ResourceViewModel? _selectedResource;
    private ObservableCollection<string> _logLines = new();
    private bool _isLogPaused;
    private const int MaxLogLines = 50;

    public MainViewModel() : this(null, null, null)
    {
        // Design-time constructor
        InitializeSampleData();
    }

    public MainViewModel(IAspirePollingService? pollingService, IConfigurationService? configService, IAspireCommandService? commandService = null)
    {
        _pollingService = pollingService;
        _configService = configService;
        _commandService = commandService;
        
        RefreshCommand = new RelayCommand(_ => RefreshData());
        OpenUrlCommand = new RelayCommand(param => OpenUrl(param?.ToString() ?? string.Empty));
        StartAspireCommand = new RelayCommand(_ => _ = StartAspireAsync(), _ => !_isExecutingCommand);
        StopAspireCommand = new RelayCommand(_ => _ = StopAspireAsync(), _ => !_isExecutingCommand);
        
        // Load project folder from config
        if (_configService != null)
        {
            var config = _configService.LoadConfiguration();
            ProjectFolder = config.ProjectFolder ?? string.Empty;
        }
        
        if (_pollingService != null)
        {
            _pollingService.ResourcesUpdated += OnResourcesUpdated;
            _pollingService.StatusChanged += OnStatusChanged;
            _pollingService.ErrorOccurred += OnError;
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
        set => SetProperty(ref _projectFolder, value);
    }

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
                LoadResourceLogs();
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
        set => SetProperty(ref _isLogPaused, value);
    }

    public string LogHeader => SelectedResource != null 
        ? $"Logs - {SelectedResource.Name}" 
        : "Logs - Select a resource";

    public string LogStatus => $"{LogLines.Count} lines | {(IsLogPaused ? "Paused" : "Live")}";

    private void RefreshData()
    {
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
        catch
        {
            // Silently fail for now
        }
    }

    private void OnResourcesUpdated(object? sender, List<AspireResource> resources)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] OnResourcesUpdated received: {resources.Count} resources");

            Resources.Clear();
            foreach (var resource in resources)
            {
                // Get primary endpoint URL
                string? url = resource.Endpoints.Count > 0 ? resource.Endpoints[0] : null;
                
                var vm = new ResourceViewModel
                {
                    Name = resource.Name,
                    Status = resource.Status,
                    CpuUsage = resource.Metrics.CpuUsagePercent,
                    MemoryUsage = resource.Metrics.MemoryUsagePercent,
                    Url = url
                };

                Resources.Add(vm);
                System.Diagnostics.Debug.WriteLine($"[MainViewModel]   Added resource: {resource.Name} (CPU: {resource.Metrics.CpuUsagePercent:F1}%, Mem: {resource.Metrics.MemoryUsagePercent:F1}%)");
            }
            
            LastUpdated = DateTime.Now;
            IsConnected = true;
            OnPropertyChanged(nameof(OverallStatusColor));

            System.Diagnostics.Debug.WriteLine($"[MainViewModel] UI updated with {Resources.Count} resources, IsConnected={IsConnected}");

            // If a resource is selected, update its logs
            if (SelectedResource != null)
            {
                var updatedResource = Resources.FirstOrDefault(r => r.Name == SelectedResource.Name);
                if (updatedResource != null)
                {
                    SelectedResource = updatedResource;
                }
            }
        });
    }

    private void OnStatusChanged(object? sender, string status)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] OnStatusChanged: {status}");
            CurrentStatus = status;
            IsConnected = status == "Connected";
        });
    }

    private void OnError(object? sender, string error)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] OnError: {error}");
            CurrentStatus = $"Error: {error}";
            IsConnected = false;
        });
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
                    
                    CommandStatus = "✅ Aspire started successfully";
                    System.Diagnostics.Debug.WriteLine($"[MainViewModel] Aspire started with endpoint: {endpoint}");
                    
                    // Trigger refresh to immediately connect
                    _pollingService?.RefreshAsync();
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
            Status = Models.ResourceStatus.Running,
            CpuUsage = 45.2,
            MemoryUsage = 62.8,
            Url = "http://localhost:5000"
        });

        Resources.Add(new ResourceViewModel
        {
            Name = "apiservice",
            Status = Models.ResourceStatus.Running,
            CpuUsage = 28.5,
            MemoryUsage = 48.3,
            Url = "http://localhost:5001"
        });

        Resources.Add(new ResourceViewModel
        {
            Name = "cache",
            Status = Models.ResourceStatus.Running,
            CpuUsage = 12.1,
            MemoryUsage = 35.7,
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
    }

    private void LoadResourceLogs()
    {
        if (SelectedResource == null)
        {
            LogLines.Clear();
            OnPropertyChanged(nameof(LogStatus));
            return;
        }

        // Clear current logs
        LogLines.Clear();

        // Add initial log entry
        AddLogLine($"[{DateTime.Now:HH:mm:ss}] Monitoring logs for: {SelectedResource.Name}");
        AddLogLine($"[{DateTime.Now:HH:mm:ss}] Status: {SelectedResource.Status}");
        AddLogLine($"[{DateTime.Now:HH:mm:ss}] CPU: {SelectedResource.CpuUsageText}, Memory: {SelectedResource.MemoryUsageText}");
        
        if (SelectedResource.HasUrl)
        {
            AddLogLine($"[{DateTime.Now:HH:mm:ss}] Endpoint: {SelectedResource.Url}");
        }

        AddLogLine($"[{DateTime.Now:HH:mm:ss}] ---");
        AddLogLine($"[{DateTime.Now:HH:mm:ss}] NOTE: Live log streaming from Aspire CLI will be implemented in Phase 2");
        AddLogLine($"[{DateTime.Now:HH:mm:ss}] For now, this panel shows resource status updates");

        OnPropertyChanged(nameof(LogStatus));
    }

    private void AddLogLine(string line)
    {
        if (IsLogPaused)
            return;

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            LogLines.Add(line);

            // Keep only the last 50 lines
            while (LogLines.Count > MaxLogLines)
            {
                LogLines.RemoveAt(0);
            }

            OnPropertyChanged(nameof(LogStatus));
        });
    }
}

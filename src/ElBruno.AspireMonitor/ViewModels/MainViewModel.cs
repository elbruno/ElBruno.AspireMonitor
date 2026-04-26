using System.Collections.ObjectModel;
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
    private string _hostUrl = "http://localhost:18888";
    private string _currentStatus = "Disconnected";
    private bool _isConnected;
    private DateTime _lastUpdated = DateTime.Now;
    private ObservableCollection<ResourceViewModel> _resources = new();
    private string _projectFolder = string.Empty;
    private bool _isExecutingCommand;
    private string _commandStatus = string.Empty;

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

    public string HostUrl
    {
        get => _hostUrl;
        set => SetProperty(ref _hostUrl, value);
    }

    public string CurrentStatus
    {
        get => _currentStatus;
        set
        {
            if (SetProperty(ref _currentStatus, value))
            {
                OnPropertyChanged(nameof(ConnectionStatus));
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
                OnPropertyChanged(nameof(OverallStatusColor));
            }
        }
    }

    public string ConnectionStatus => CurrentStatus;

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
            Resources.Clear();
            foreach (var resource in resources)
            {
                // Get primary endpoint URL
                string? url = resource.Endpoints.Count > 0 ? resource.Endpoints[0] : null;
                
                Resources.Add(new ResourceViewModel
                {
                    Name = resource.Name,
                    Status = resource.Status,
                    CpuUsage = resource.Metrics.CpuUsagePercent,
                    MemoryUsage = resource.Metrics.MemoryUsagePercent,
                    Url = url
                });
            }
            
            LastUpdated = DateTime.Now;
            IsConnected = true;
            OnPropertyChanged(nameof(OverallStatusColor));
        });
    }

    private void OnStatusChanged(object? sender, string status)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            CurrentStatus = status;
            IsConnected = status == "Connected";
        });
    }

    private void OnError(object? sender, string error)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            CurrentStatus = $"Error: {error}";
            IsConnected = false;
        });
    }

    public void Start()
    {
        _pollingService?.Start();
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
            
            var success = await _commandService.StartAspireAsync(ProjectFolder);
            
            if (success)
            {
                CommandStatus = "🔍 Detecting Aspire endpoint...";
                
                // Wait a moment then detect the endpoint from running Aspire instance
                await Task.Delay(2000);
                var endpoint = await _commandService.DetectAspireEndpointAsync();
                
                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    // Update the API client endpoint
                    HostUrl = endpoint;
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
            
            var success = await _commandService.StopAspireAsync();
            
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
}

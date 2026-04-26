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
    private string _hostUrl = "http://localhost:15888";
    private string _currentStatus = "Disconnected";
    private bool _isConnected;
    private DateTime _lastUpdated = DateTime.Now;
    private ObservableCollection<ResourceViewModel> _resources = new();

    public MainViewModel() : this(null, null)
    {
        // Design-time constructor
        InitializeSampleData();
    }

    public MainViewModel(IAspirePollingService? pollingService, IConfigurationService? configService)
    {
        _pollingService = pollingService;
        _configService = configService;
        
        RefreshCommand = new RelayCommand(_ => RefreshData());
        OpenUrlCommand = new RelayCommand(param => OpenUrl(param?.ToString() ?? string.Empty));
        
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

    public ICommand? RefreshCommand { get; }
    public ICommand? OpenUrlCommand { get; }

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

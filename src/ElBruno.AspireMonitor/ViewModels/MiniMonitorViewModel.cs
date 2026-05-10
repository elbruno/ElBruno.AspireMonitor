using System.Windows.Media;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq;
using ElBruno.AspireMonitor.Infrastructure;
using ElBruno.AspireMonitor.Helpers;

namespace ElBruno.AspireMonitor.ViewModels;

public enum PinnedResourceStatus
{
    Found,
    FoundNoUrl,
    Missing
}

public class MiniResourceItem
{
    public string Name { get; set; } = "";
    public string? Url { get; set; }
    public bool HasUrl => !string.IsNullOrWhiteSpace(Url);
    public string FallbackText { get; set; } = "";
    public PinnedResourceStatus Status { get; set; } = PinnedResourceStatus.Found;
    public bool IsMissing => Status == PinnedResourceStatus.Missing;
}

public class MiniMonitorViewModel : ViewModelBase
{
    private readonly MainViewModel? _mainViewModel;
    private string _resourceCount = "No Aspire Running";
    private string _statusEmoji = "⚫";
    private System.Windows.Media.Brush _statusBackground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF5, 0xF5, 0xF5));
    private System.Windows.Media.Brush _statusBorder = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xCC, 0xCC, 0xCC));
    private System.Windows.Media.Brush _statusForeground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66));
    private string _workingFolder = "Not running";
    private DateTime _lastUpdate = DateTime.Now;
    private ObservableCollection<MiniResourceItem> _pinnedResources = new();

    public MiniMonitorViewModel() : this(null)
    {
    }

    public MiniMonitorViewModel(MainViewModel? mainViewModel)
    {
        _mainViewModel = mainViewModel;
        
        if (_mainViewModel != null)
        {
            _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
            _mainViewModel.Resources.CollectionChanged += Resources_CollectionChanged;
            UpdateMiniMonitorData();
        }
    }

    public MainViewModel? MainViewModel => _mainViewModel;

    public string AppVersion => VersionHelper.GetAppVersion();

    public Action<string> LogCallback => _ => { }; // No-op for mini monitor

    public void ClearLog() { } // No-op for mini monitor

    public string ResourceCount
    {
        get => _resourceCount;
        set => SetProperty(ref _resourceCount, value);
    }

    public string StatusEmoji
    {
        get => _statusEmoji;
        set => SetProperty(ref _statusEmoji, value);
    }

    public System.Windows.Media.Brush StatusBackground
    {
        get => _statusBackground;
        set => SetProperty(ref _statusBackground, value);
    }

    public System.Windows.Media.Brush StatusBorderColor
    {
        get => _statusBorder;
        set => SetProperty(ref _statusBorder, value);
    }

    public System.Windows.Media.Brush StatusForeground
    {
        get => _statusForeground;
        set => SetProperty(ref _statusForeground, value);
    }

    public string WorkingFolder
    {
        get => _workingFolder;
        set
        {
            if (SetProperty(ref _workingFolder, value))
            {
                OnPropertyChanged(nameof(WorkingFolderDisplay));
            }
        }
    }

    public string WorkingFolderDisplay => PathHumanizer.Humanize(_workingFolder, 35);

    public ObservableCollection<MiniResourceItem> PinnedResources
    {
        get => _pinnedResources;
        set
        {
            if (SetProperty(ref _pinnedResources, value))
            {
                OnPropertyChanged(nameof(HasPinnedResources));
            }
        }
    }

    public bool HasPinnedResources => _pinnedResources.Count > 0;

    public DateTime LastUpdate
    {
        get => _lastUpdate;
        set
        {
            if (SetProperty(ref _lastUpdate, value))
            {
                OnPropertyChanged(nameof(LastUpdateText));
            }
        }
    }

    public string LastUpdateText
    {
        get
        {
            var seconds = (DateTime.Now - LastUpdate).TotalSeconds;
            string timeText;
            if (seconds < 5)
                timeText = "just now";
            else if (seconds < 60)
                timeText = $"{(int)seconds}s ago";
            else if (seconds < 120)
                timeText = "1m ago";
            else
                timeText = $"{(int)(seconds / 60)}m ago";
            
            // Add refresh indicator
            var isRefreshing = seconds < 3;
            var indicator = isRefreshing ? "🔄" : "✓";
            return $"{indicator} {timeText}";
        }
    }

    public string StatusLine
    {
        get
        {
            if (_mainViewModel == null)
                return "Aspire not running";

            // Check if Aspire is running by looking at resource count
            int count = _mainViewModel.Resources.Count;
            
            if (count == 0)
                return "Aspire not running";

            var status = _statusEmoji switch
            {
                "🟢" => "healthy",
                "🟡" => "warning",
                "🔴" => "critical",
                _ => "running"
            };

            return $"{count} resource{(count != 1 ? "s" : "")} | {status}";
        }
    }

    public bool CanStartAspire
    {
        get => _mainViewModel?.Resources.Count == 0;
    }

    public bool CanStopAspire
    {
        get => _mainViewModel?.Resources.Count > 0;
    }

    private void MainViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel] MainViewModel property changed: {e.PropertyName}");

        if (e.PropertyName == nameof(MainViewModel.Resources) ||
            e.PropertyName == nameof(MainViewModel.OverallStatusColor) ||
            e.PropertyName == nameof(MainViewModel.ProjectFolder) ||
            e.PropertyName == nameof(MainViewModel.IsConnected) ||
            e.PropertyName == nameof(MainViewModel.HostUrl) ||
            e.PropertyName == nameof(MainViewModel.MiniWindowResourcesSetting))
        {
            System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel] Triggering UI update due to {e.PropertyName} change");
            UpdateMiniMonitorData();
        }
    }

    private void Resources_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel] Resources collection changed: {e.Action}");
        UpdateMiniMonitorData();
    }

    private void UpdateMiniMonitorData()
    {
        if (_mainViewModel == null)
            return;

        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel] UpdateMiniMonitorData called");
        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Resources: {_mainViewModel.Resources.Count}");
        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   ProjectFolder: {_mainViewModel.ProjectFolder}");

        // Update resource count and status. Treat "not connected" as "not running" so the UI
        // recovers immediately when Aspire stops, even if the resources collection wasn't cleared yet.
        int count = _mainViewModel.Resources.Count;
        bool aspireRunning = count > 0;

        if (!aspireRunning)
        {
            // Aspire not running
            ResourceCount = "No Aspire Running";
            StatusEmoji = "⚫";
            // Show configured folder if set, otherwise default message
            WorkingFolder = !string.IsNullOrEmpty(_mainViewModel.ProjectFolder)
                ? _mainViewModel.ProjectFolder
                : "(no working folder set)";
            StatusBackground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF5, 0xF5, 0xF5)); // Light gray
            StatusBorderColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xCC, 0xCC, 0xCC)); // Gray
            StatusForeground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66)); // Dark gray
            // Clear pinned resources when not running
            PinnedResources.Clear();
            OnPropertyChanged(nameof(HasPinnedResources));
            System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: Not Running");
        }
        else
        {
            // Aspire is running
            ResourceCount = count == 1 ? "1 Resource" : $"{count} Resources";
            WorkingFolder = string.IsNullOrEmpty(_mainViewModel.ProjectFolder) 
                ? "Running..." 
                : _mainViewModel.ProjectFolder;

            // Update pinned resources based on setting
            UpdatePinnedResources();

            // Update status indicator based on overall health
            var statusBrush = _mainViewModel.OverallStatusColor as SolidColorBrush;
            
            if (statusBrush != null)
            {
                var redColor = System.Windows.Media.Color.FromRgb(0xF4, 0x43, 0x36);
                var yellowColor = System.Windows.Media.Color.FromRgb(0xFF, 0xC1, 0x07);
                var greenColor = System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50);

                if (statusBrush.Color == redColor)
                {
                    StatusEmoji = "🔴";
                    StatusBackground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xEB, 0xEE)); // Light red
                    StatusBorderColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF4, 0x43, 0x36)); // Red
                    StatusForeground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xC6, 0x28, 0x28)); // Dark red
                    System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: Critical (🔴)");
                }
                else if (statusBrush.Color == yellowColor)
                {
                    StatusEmoji = "🟡";
                    StatusBackground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xF8, 0xE1)); // Light yellow
                    StatusBorderColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xC1, 0x07)); // Yellow
                    StatusForeground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF5, 0x7C, 0x00)); // Dark yellow
                    System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: Warning (🟡)");
                }
                else if (statusBrush.Color == greenColor)
                {
                    StatusEmoji = "🟢";
                    StatusBackground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE8, 0xF5, 0xE9)); // Light green
                    StatusBorderColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50)); // Green
                    StatusForeground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x1B, 0x5E, 0x20)); // Dark green
                    System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: Healthy (🟢)");
                }
                else
                {
                    StatusEmoji = "🟡";
                    StatusBackground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xF8, 0xE1)); // Light yellow
                    StatusBorderColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xC1, 0x07)); // Yellow
                    StatusForeground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF5, 0x7C, 0x00)); // Dark yellow
                    System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: Unknown (🟡)");
                }
            }
            else
            {
                StatusEmoji = "🟡";
                StatusBackground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xF8, 0xE1)); // Light yellow
                StatusBorderColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xC1, 0x07)); // Yellow
                StatusForeground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF5, 0x7C, 0x00)); // Dark yellow
                System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: Running (🟡)");
            }
        }

        // Update last update time
        LastUpdate = _mainViewModel.LastUpdated;

        // Update all status-dependent properties
        OnPropertyChanged(nameof(StatusLine));
        OnPropertyChanged(nameof(CanStartAspire));
        OnPropertyChanged(nameof(CanStopAspire));
        OnPropertyChanged(nameof(DashboardUrl));
        OnPropertyChanged(nameof(HasDashboard));
        
        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel] ResourceCount: {ResourceCount}, StatusEmoji: {StatusEmoji}");
    }

    private void UpdatePinnedResources()
    {
        if (_mainViewModel == null)
            return;

        PinnedResources.Clear();

        var settingValue = _mainViewModel.MiniWindowResourcesSetting ?? string.Empty;
        if (string.IsNullOrWhiteSpace(settingValue))
        {
            OnPropertyChanged(nameof(HasPinnedResources));
            return;
        }

        // Don't show missing entries when Aspire isn't running
        int resourceCount = _mainViewModel.Resources.Count;
        if (resourceCount == 0)
        {
            OnPropertyChanged(nameof(HasPinnedResources));
            return;
        }

        // Parse tokens: split on comma, trim, drop empties, dedupe (case-insensitive)
        var tokenPairs = settingValue
            .Split(',')
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(t => new { Original = t, Lower = t.ToLowerInvariant() })
            .GroupBy(pair => pair.Lower)
            .Select(g => g.First())
            .ToList();

        // For each token, find matching resources (case-insensitive prefix match)
        foreach (var tokenPair in tokenPairs)
        {
            var matches = _mainViewModel.Resources
                .Where(r => r.Name.ToLowerInvariant().StartsWith(tokenPair.Lower))
                .ToList();

            if (matches.Count > 0)
            {
                // Found one or more matching resources
                foreach (var match in matches)
                {
                    var item = new MiniResourceItem
                    {
                        Name = match.Name,
                        Url = match.Url
                    };

                    if (!item.HasUrl)
                    {
                        // No URL: show Type or fallback
                        item.FallbackText = match.Type ?? "(no endpoint)";
                        item.Status = PinnedResourceStatus.FoundNoUrl;
                    }
                    else
                    {
                        item.Status = PinnedResourceStatus.Found;
                    }

                    PinnedResources.Add(item);
                }
            }
            else
            {
                // No matching resource found - add a "missing" entry
                var missingItem = new MiniResourceItem
                {
                    Name = tokenPair.Original,
                    Url = null,
                    FallbackText = "not found",
                    Status = PinnedResourceStatus.Missing
                };

                PinnedResources.Add(missingItem);
            }
        }

        OnPropertyChanged(nameof(HasPinnedResources));
    }

    public string DashboardUrl => _mainViewModel?.HostUrl ?? string.Empty;

    public bool HasDashboard =>
        _mainViewModel != null
        && _mainViewModel.IsConnected
        && _mainViewModel.Resources.Count > 0
        && !string.IsNullOrWhiteSpace(_mainViewModel.HostUrl);
}

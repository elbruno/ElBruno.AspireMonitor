using System.Windows.Media;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ElBruno.AspireMonitor.Infrastructure;

namespace ElBruno.AspireMonitor.ViewModels;

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

    public MiniMonitorViewModel() : this(null)
    {
    }

    public MiniMonitorViewModel(MainViewModel? mainViewModel)
    {
        _mainViewModel = mainViewModel;
        
        if (_mainViewModel != null)
        {
            _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
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
        set => SetProperty(ref _workingFolder, value);
    }

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
            e.PropertyName == nameof(MainViewModel.ProjectFolder))
        {
            System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel] Triggering UI update due to {e.PropertyName} change");
            UpdateMiniMonitorData();
        }
    }

    private void UpdateMiniMonitorData()
    {
        if (_mainViewModel == null)
            return;

        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel] UpdateMiniMonitorData called");
        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Resources: {_mainViewModel.Resources.Count}");
        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   ProjectFolder: {_mainViewModel.ProjectFolder}");

        // Update resource count and status
        int count = _mainViewModel.Resources.Count;
        
        if (count == 0)
        {
            // Aspire not running
            ResourceCount = "No Aspire Running";
            StatusEmoji = "⚫";
            WorkingFolder = "Aspire is not running";
            StatusBackground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF5, 0xF5, 0xF5)); // Light gray
            StatusBorderColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xCC, 0xCC, 0xCC)); // Gray
            StatusForeground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x66, 0x66, 0x66)); // Dark gray
            System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: Not Running");
        }
        else
        {
            // Aspire is running
            ResourceCount = count == 1 ? "1 Resource" : $"{count} Resources";
            WorkingFolder = string.IsNullOrEmpty(_mainViewModel.ProjectFolder) 
                ? "Running..." 
                : _mainViewModel.ProjectFolder;

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
        
        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel] ResourceCount: {ResourceCount}, StatusEmoji: {StatusEmoji}");
    }
}

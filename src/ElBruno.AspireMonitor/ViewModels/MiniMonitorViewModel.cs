using System.Windows.Media;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ElBruno.AspireMonitor.Infrastructure;

namespace ElBruno.AspireMonitor.ViewModels;

public class MiniMonitorViewModel : ViewModelBase
{
    private readonly MainViewModel? _mainViewModel;
    private string _resourceCount = "0 Resources";
    private string _statusEmoji = "⚪";
    private System.Windows.Media.Brush _statusColor = System.Windows.Media.Brushes.Gray;
    private string _workingFolder = "Not configured";
    private const int MaxLogLines = 5;
    private ObservableCollection<string> _logLines = new();

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

    public string AppVersion => VersionHelper.GetAppVersion();

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

    public System.Windows.Media.Brush StatusColor
    {
        get => _statusColor;
        set => SetProperty(ref _statusColor, value);
    }

    public string WorkingFolder
    {
        get => _workingFolder;
        set => SetProperty(ref _workingFolder, value);
    }

    public ICommand? StartAspireCommand => _mainViewModel?.StartAspireCommand;

    public ICommand? StopAspireCommand => _mainViewModel?.StopAspireCommand;

    public ObservableCollection<string> LogLines
    {
        get => _logLines;
        set => SetProperty(ref _logLines, value);
    }

    public Action<string> LogCallback => AddLogLine;

    public void AddLogLine(string line)
    {
        if (string.IsNullOrEmpty(line))
            return;

        _logLines.Add(line);
        
        // Keep only the last 5 lines
        while (_logLines.Count > MaxLogLines)
        {
            _logLines.RemoveAt(0);
        }
    }

    public void ClearLog()
    {
        _logLines.Clear();
    }

    public string DetailsSummary
    {
        get
        {
            if (_mainViewModel == null)
                return "Not connected";

            if (!_mainViewModel.IsConnected)
            {
                var status = _mainViewModel.CurrentStatus;
                if (status.StartsWith("Error:"))
                    return status;
                return "Disconnected - waiting to connect...";
            }

            int resourceCount = _mainViewModel.Resources.Count;
            if (resourceCount == 0)
                return "✓ Connected but no resources found";

            return $"{resourceCount} resources running";
        }
    }

    private void MainViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel] MainViewModel property changed: {e.PropertyName}");

        if (e.PropertyName == nameof(MainViewModel.Resources) ||
            e.PropertyName == nameof(MainViewModel.OverallStatusColor) ||
            e.PropertyName == nameof(MainViewModel.IsConnected) ||
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
        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   IsConnected: {_mainViewModel.IsConnected}");
        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   ProjectFolder: {_mainViewModel.ProjectFolder}");

        // Update resource count
        int count = _mainViewModel.Resources.Count;
        ResourceCount = count == 1 ? "1 Resource" : $"{count} Resources";

        // Update working folder
        WorkingFolder = string.IsNullOrEmpty(_mainViewModel.ProjectFolder) 
            ? "Not configured" 
            : _mainViewModel.ProjectFolder;

        // Update status indicator
        var statusBrush = _mainViewModel.OverallStatusColor as SolidColorBrush;
        StatusColor = statusBrush ?? System.Windows.Media.Brushes.Gray;

        // Set emoji based on status
        if (!_mainViewModel.IsConnected)
        {
            StatusEmoji = "❌";
            System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: Disconnected (❌)");
        }
        else
        {
            if (statusBrush != null)
            {
                var redColor = System.Windows.Media.Color.FromRgb(0xF4, 0x43, 0x36);
                var yellowColor = System.Windows.Media.Color.FromRgb(0xFF, 0xC1, 0x07);
                var greenColor = System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50);

                if (statusBrush.Color == redColor)
                {
                    StatusEmoji = "🔴";
                    System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: Critical (🔴)");
                }
                else if (statusBrush.Color == yellowColor)
                {
                    StatusEmoji = "🟡";
                    System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: Warning (🟡)");
                }
                else if (statusBrush.Color == greenColor)
                {
                    StatusEmoji = "🟢";
                    System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: Healthy (🟢)");
                }
                else
                {
                    StatusEmoji = "⚪";
                    System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: Unknown (⚪)");
                }
            }
            else
            {
                StatusEmoji = "⚪";
                System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel]   Status: No brush (⚪)");
            }
        }

        OnPropertyChanged(nameof(DetailsSummary));
        System.Diagnostics.Debug.WriteLine($"[MiniMonitorViewModel] ResourceCount: {ResourceCount}, StatusEmoji: {StatusEmoji}");
    }
}

using System.Windows.Media;
using ElBruno.AspireMonitor.Infrastructure;

namespace ElBruno.AspireMonitor.ViewModels;

public class MiniMonitorViewModel : ViewModelBase
{
    private readonly MainViewModel? _mainViewModel;
    private string _resourceCount = "0 Resources";
    private string _statusEmoji = "⚪";
    private System.Windows.Media.Brush _statusColor = System.Windows.Media.Brushes.Gray;

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

    public string DetailsSummary
    {
        get
        {
            if (_mainViewModel == null)
                return "Not connected";

            if (!_mainViewModel.IsConnected)
                return "Disconnected";

            int resourceCount = _mainViewModel.Resources.Count;
            return $"{resourceCount} resources running";
        }
    }

    private void MainViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.Resources) ||
            e.PropertyName == nameof(MainViewModel.OverallStatusColor) ||
            e.PropertyName == nameof(MainViewModel.IsConnected))
        {
            UpdateMiniMonitorData();
        }
    }

    private void UpdateMiniMonitorData()
    {
        if (_mainViewModel == null)
            return;

        // Update resource count
        int count = _mainViewModel.Resources.Count;
        ResourceCount = count == 1 ? "1 Resource" : $"{count} Resources";

        // Update status indicator
        var statusBrush = _mainViewModel.OverallStatusColor as SolidColorBrush;
        StatusColor = statusBrush ?? System.Windows.Media.Brushes.Gray;

        // Set emoji based on status
        if (!_mainViewModel.IsConnected)
        {
            StatusEmoji = "❌";
        }
        else
        {
            if (statusBrush != null)
            {
                var redColor = System.Windows.Media.Color.FromRgb(0xF4, 0x43, 0x36);
                var yellowColor = System.Windows.Media.Color.FromRgb(0xFF, 0xC1, 0x07);
                var greenColor = System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50);

                if (statusBrush.Color == redColor)
                    StatusEmoji = "🔴";
                else if (statusBrush.Color == yellowColor)
                    StatusEmoji = "🟡";
                else if (statusBrush.Color == greenColor)
                    StatusEmoji = "🟢";
                else
                    StatusEmoji = "⚪";
            }
            else
            {
                StatusEmoji = "⚪";
            }
        }

        OnPropertyChanged(nameof(DetailsSummary));
    }
}

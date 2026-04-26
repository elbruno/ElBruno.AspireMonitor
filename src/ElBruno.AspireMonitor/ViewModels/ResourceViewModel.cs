using System.Windows.Media;
using ElBruno.AspireMonitor.Infrastructure;
using ElBruno.AspireMonitor.Models;

namespace ElBruno.AspireMonitor.ViewModels;

public class ResourceViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private ResourceStatus _status = ResourceStatus.Unknown;
    private double _cpuUsage;
    private double _memoryUsage;
    private string? _url;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public ResourceStatus Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(StatusColor));
            }
        }
    }

    public System.Windows.Media.Brush StatusColor
    {
        get
        {
            // Calculate color based on status and resource usage
            if (Status != ResourceStatus.Running)
            {
                return Status switch
                {
                    ResourceStatus.Unknown => System.Windows.Media.Brushes.Gray,
                    ResourceStatus.Stopped => System.Windows.Media.Brushes.Gray,
                    _ => System.Windows.Media.Brushes.Orange
                };
            }

            // If running, check CPU and Memory thresholds
            double combinedUsage = (CpuUsage + MemoryUsage) / 2;
            
            if (combinedUsage >= 90)
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF4, 0x43, 0x36)); // Red
            if (combinedUsage >= 70)
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xC1, 0x07)); // Yellow
            
            return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50)); // Green
        }
    }

    public double CpuUsage
    {
        get => _cpuUsage;
        set
        {
            if (SetProperty(ref _cpuUsage, value))
            {
                OnPropertyChanged(nameof(CpuUsageText));
                OnPropertyChanged(nameof(StatusColor));
            }
        }
    }

    public double MemoryUsage
    {
        get => _memoryUsage;
        set
        {
            if (SetProperty(ref _memoryUsage, value))
            {
                OnPropertyChanged(nameof(MemoryUsageText));
                OnPropertyChanged(nameof(StatusColor));
            }
        }
    }

    public string CpuUsageText => $"{CpuUsage:F1}%";
    
    public string MemoryUsageText => $"{MemoryUsage:F1}%";

    public string? Url
    {
        get => _url;
        set
        {
            if (SetProperty(ref _url, value))
            {
                OnPropertyChanged(nameof(HasUrl));
                OnPropertyChanged(nameof(UrlDisplay));
            }
        }
    }

    public bool HasUrl => !string.IsNullOrEmpty(Url);

    public string UrlDisplay => HasUrl ? "🔗 Open" : string.Empty;
}

using System.Windows.Media;
using ElBruno.AspireMonitor.Infrastructure;
using ElBruno.AspireMonitor.Models;

namespace ElBruno.AspireMonitor.ViewModels;

public class ResourceViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private string? _type;
    private ResourceStatus _status = ResourceStatus.Unknown;
    private double _cpuUsage;
    private double _memoryUsage;
    private double _diskUsage;
    private int _endpointCount;
    private string? _url;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string? Type
    {
        get => _type;
        set
        {
            if (SetProperty(ref _type, value))
            {
                OnPropertyChanged(nameof(HasResourceType));
                OnPropertyChanged(nameof(TypeDisplay));
                OnPropertyChanged(nameof(ResourceTypeText));
            }
        }
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
            if (Status != ResourceStatus.Running)
            {
                return Status switch
                {
                    ResourceStatus.Unknown => System.Windows.Media.Brushes.Gray,
                    ResourceStatus.Stopped => System.Windows.Media.Brushes.Gray,
                    _ => System.Windows.Media.Brushes.Orange
                };
            }

            var combinedUsage = (CpuUsage + MemoryUsage) / 2;

            if (combinedUsage >= 90)
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xF4, 0x43, 0x36));
            if (combinedUsage >= 70)
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xC1, 0x07));

            return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50));
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

    public double DiskUsage
    {
        get => _diskUsage;
        set
        {
            if (SetProperty(ref _diskUsage, value))
            {
                OnPropertyChanged(nameof(DiskUsageText));
            }
        }
    }

    public string CpuUsageText => $"{CpuUsage:F1}%";

    public string MemoryUsageText => $"{MemoryUsage:F1}%";

    public string DiskUsageText => $"{DiskUsage:F1}%";

    public int EndpointCount
    {
        get => _endpointCount;
        set
        {
            if (SetProperty(ref _endpointCount, value))
            {
                OnPropertyChanged(nameof(EndpointCountText));
            }
        }
    }

    public string TypeDisplay => Type ?? string.Empty;

    public string? ResourceType
    {
        get => Type;
        set => Type = value;
    }

    public bool HasResourceType => !string.IsNullOrWhiteSpace(Type);

    public string ResourceTypeText => TypeDisplay;

    public double DiskUsagePercent
    {
        get => DiskUsage;
        set => DiskUsage = value;
    }

    public string EndpointCountText => EndpointCount == 1 ? "1 endpoint" : $"{EndpointCount} endpoints";

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

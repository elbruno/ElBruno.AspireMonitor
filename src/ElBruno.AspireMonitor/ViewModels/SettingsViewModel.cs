using System.IO;
using ElBruno.AspireMonitor.Infrastructure;
using ElBruno.AspireMonitor.Services;

namespace ElBruno.AspireMonitor.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private string _aspireEndpoint = "http://localhost:15888";
    private int _pollingInterval = 5000;
    private int _cpuThresholdWarning = 70;
    private int _cpuThresholdCritical = 90;
    private int _memoryThresholdWarning = 70;
    private int _memoryThresholdCritical = 90;
    private bool _startWithWindows;
    private string _projectFolder = string.Empty;
    private string _repositoryUrl = string.Empty;
    private string _validationMessage = string.Empty;

    public SettingsViewModel(IConfigurationService configService)
    {
        _configService = configService;
        LoadSettings();
    }

    public string AspireEndpoint
    {
        get => _aspireEndpoint;
        set => SetProperty(ref _aspireEndpoint, value);
    }

    public int PollingInterval
    {
        get => _pollingInterval;
        set => SetProperty(ref _pollingInterval, value);
    }

    public int CpuThresholdWarning
    {
        get => _cpuThresholdWarning;
        set => SetProperty(ref _cpuThresholdWarning, value);
    }

    public int CpuThresholdCritical
    {
        get => _cpuThresholdCritical;
        set => SetProperty(ref _cpuThresholdCritical, value);
    }

    public int MemoryThresholdWarning
    {
        get => _memoryThresholdWarning;
        set => SetProperty(ref _memoryThresholdWarning, value);
    }

    public int MemoryThresholdCritical
    {
        get => _memoryThresholdCritical;
        set => SetProperty(ref _memoryThresholdCritical, value);
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetProperty(ref _startWithWindows, value);
    }

    public string ProjectFolder
    {
        get => _projectFolder;
        set => SetProperty(ref _projectFolder, value);
    }

    public string RepositoryUrl
    {
        get => _repositoryUrl;
        set => SetProperty(ref _repositoryUrl, value);
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        set => SetProperty(ref _validationMessage, value);
    }

    public bool Validate()
    {
        ValidationMessage = string.Empty;

        // Validate URL
        if (string.IsNullOrWhiteSpace(AspireEndpoint))
        {
            ValidationMessage = "Aspire Endpoint cannot be empty.";
            return false;
        }

        if (!Uri.TryCreate(AspireEndpoint, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            ValidationMessage = "Aspire Endpoint must be a valid HTTP or HTTPS URL.";
            return false;
        }

        // Validate polling interval
        if (PollingInterval < 1000 || PollingInterval > 60000)
        {
            ValidationMessage = "Polling Interval must be between 1000 and 60000 ms.";
            return false;
        }

        // Validate thresholds
        if (CpuThresholdWarning < 0 || CpuThresholdWarning > 100)
        {
            ValidationMessage = "CPU Warning Threshold must be between 0 and 100.";
            return false;
        }

        if (CpuThresholdCritical < 0 || CpuThresholdCritical > 100)
        {
            ValidationMessage = "CPU Critical Threshold must be between 0 and 100.";
            return false;
        }

        if (CpuThresholdCritical <= CpuThresholdWarning)
        {
            ValidationMessage = "CPU Critical Threshold must be greater than Warning Threshold.";
            return false;
        }

        if (MemoryThresholdWarning < 0 || MemoryThresholdWarning > 100)
        {
            ValidationMessage = "Memory Warning Threshold must be between 0 and 100.";
            return false;
        }

        if (MemoryThresholdCritical < 0 || MemoryThresholdCritical > 100)
        {
            ValidationMessage = "Memory Critical Threshold must be between 0 and 100.";
            return false;
        }

        if (MemoryThresholdCritical <= MemoryThresholdWarning)
        {
            ValidationMessage = "Memory Critical Threshold must be greater than Warning Threshold.";
            return false;
        }

        // Validate ProjectFolder if set
        if (!string.IsNullOrWhiteSpace(ProjectFolder))
        {
            if (!Directory.Exists(ProjectFolder))
            {
                ValidationMessage = "ProjectFolder does not exist.";
                return false;
            }
        }

        // Validate RepositoryUrl if set
        if (!string.IsNullOrWhiteSpace(RepositoryUrl))
        {
            if (!Uri.TryCreate(RepositoryUrl, UriKind.Absolute, out var repoUri) ||
                (repoUri.Scheme != Uri.UriSchemeHttp && repoUri.Scheme != Uri.UriSchemeHttps))
            {
                ValidationMessage = "RepositoryUrl must be a valid HTTP or HTTPS URL";
                return false;
            }
        }

        return true;
    }

    public void SaveSettings()
    {
        if (!Validate())
            return;

        var config = new Models.Configuration
        {
            AspireEndpoint = AspireEndpoint,
            PollingIntervalMs = PollingInterval,
            CpuThresholdWarning = CpuThresholdWarning,
            CpuThresholdCritical = CpuThresholdCritical,
            MemoryThresholdWarning = MemoryThresholdWarning,
            MemoryThresholdCritical = MemoryThresholdCritical,
            StartWithWindows = StartWithWindows,
            ProjectFolder = ProjectFolder ?? string.Empty,
            RepositoryUrl = RepositoryUrl ?? string.Empty
        };

        _configService.SaveConfiguration(config);
    }

    private void LoadSettings()
    {
        var config = _configService.LoadConfiguration();
        
        AspireEndpoint = config.AspireEndpoint;
        PollingInterval = config.PollingIntervalMs;
        CpuThresholdWarning = config.CpuThresholdWarning;
        CpuThresholdCritical = config.CpuThresholdCritical;
        MemoryThresholdWarning = config.MemoryThresholdWarning;
        MemoryThresholdCritical = config.MemoryThresholdCritical;
        StartWithWindows = config.StartWithWindows;
        ProjectFolder = config.ProjectFolder ?? string.Empty;
        RepositoryUrl = config.RepositoryUrl ?? string.Empty;
    }

    public void DetectAspireEndpointFromFolder()
    {
        if (string.IsNullOrWhiteSpace(ProjectFolder) || !Directory.Exists(ProjectFolder))
            return;

        var detectedEndpoint = Models.AppConfiguration.DetectAspireEndpoint(ProjectFolder);
        if (!string.IsNullOrWhiteSpace(detectedEndpoint))
        {
            AspireEndpoint = detectedEndpoint;
        }
    }
}

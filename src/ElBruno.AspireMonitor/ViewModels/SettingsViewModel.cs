using System.IO;
using ElBruno.AspireMonitor.Infrastructure;
using ElBruno.AspireMonitor.Services;

namespace ElBruno.AspireMonitor.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private string _aspireEndpoint = Models.Configuration.DefaultAspireEndpoint;
    private int _pollingInterval = 5000;
    private bool _startWithWindows;
    private bool _hideDevelopmentResources;
    private bool _showMiniWindowResourceTelemetry = true;
    private bool _showOnlyMainMiniWindowResources = true;
    private string _projectFolder = string.Empty;
    private string _miniWindowResources = string.Empty;
    private string _validationMessage = string.Empty;

    public SettingsViewModel(IConfigurationService configService)
    {
        _configService = configService;
        LoadSettings();
    }

    public int PollingInterval
    {
        get => _pollingInterval;
        set => SetProperty(ref _pollingInterval, value);
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetProperty(ref _startWithWindows, value);
    }

    public string AspireEndpoint
    {
        get => _aspireEndpoint;
        set => SetProperty(ref _aspireEndpoint, string.IsNullOrWhiteSpace(value) ? Models.Configuration.DefaultAspireEndpoint : value);
    }

    public bool HideDevelopmentResources
    {
        get => _hideDevelopmentResources;
        set => SetProperty(ref _hideDevelopmentResources, value);
    }

    public string ProjectFolder
    {
        get => _projectFolder;
        set => SetProperty(ref _projectFolder, value);
    }

    public string MiniWindowResources
    {
        get => _miniWindowResources;
        set => SetProperty(ref _miniWindowResources, value);
    }

    public bool ShowMiniWindowResourceTelemetry
    {
        get => _showMiniWindowResourceTelemetry;
        set => SetProperty(ref _showMiniWindowResourceTelemetry, value);
    }

    public bool ShowOnlyMainMiniWindowResources
    {
        get => _showOnlyMainMiniWindowResources;
        set => SetProperty(ref _showOnlyMainMiniWindowResources, value);
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        set => SetProperty(ref _validationMessage, value);
    }

    public bool Validate()
    {
        ValidationMessage = string.Empty;

        // Validate polling interval
        if (PollingInterval < 1000 || PollingInterval > 60000)
        {
            ValidationMessage = "Polling Interval must be between 1000 and 60000 ms.";
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
            StartWithWindows = StartWithWindows,
            HideDevelopmentResources = HideDevelopmentResources,
            ProjectFolder = ProjectFolder ?? string.Empty,
            MiniWindowResources = MiniWindowResources ?? string.Empty,
            ShowMiniWindowResourceTelemetry = ShowMiniWindowResourceTelemetry,
            ShowOnlyMainMiniWindowResources = ShowOnlyMainMiniWindowResources
        };

        _configService.SaveConfiguration(config);
    }

    private void LoadSettings()
    {
        var config = _configService.LoadConfiguration();
        
        AspireEndpoint = string.IsNullOrWhiteSpace(config.AspireEndpoint)
            ? Models.Configuration.DefaultAspireEndpoint
            : config.AspireEndpoint;
        PollingInterval = config.PollingIntervalMs;
        StartWithWindows = config.StartWithWindows;
        HideDevelopmentResources = config.HideDevelopmentResources;
        ProjectFolder = config.ProjectFolder ?? string.Empty;
        MiniWindowResources = config.MiniWindowResources ?? string.Empty;
        ShowMiniWindowResourceTelemetry = config.ShowMiniWindowResourceTelemetry;
        ShowOnlyMainMiniWindowResources = config.ShowOnlyMainMiniWindowResources;
    }
}

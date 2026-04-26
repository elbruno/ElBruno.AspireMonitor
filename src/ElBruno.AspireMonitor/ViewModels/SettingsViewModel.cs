using System.IO;
using ElBruno.AspireMonitor.Infrastructure;
using ElBruno.AspireMonitor.Services;

namespace ElBruno.AspireMonitor.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private int _pollingInterval = 5000;
    private bool _startWithWindows;
    private string _projectFolder = string.Empty;
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

    public string ProjectFolder
    {
        get => _projectFolder;
        set => SetProperty(ref _projectFolder, value);
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
            PollingIntervalMs = PollingInterval,
            StartWithWindows = StartWithWindows,
            ProjectFolder = ProjectFolder ?? string.Empty
        };

        _configService.SaveConfiguration(config);
    }

    private void LoadSettings()
    {
        var config = _configService.LoadConfiguration();
        
        PollingInterval = config.PollingIntervalMs;
        StartWithWindows = config.StartWithWindows;
        ProjectFolder = config.ProjectFolder ?? string.Empty;
    }
}

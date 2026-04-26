using ElBruno.AspireMonitor.Infrastructure;

namespace ElBruno.AspireMonitor.ViewModels;

public class ConfigurationViewModel : ViewModelBase
{
    private string _aspireEndpoint = "http://localhost:15888";
    private int _pollingInterval = 5000;
    private int _cpuThreshold = 70;
    private int _memoryThreshold = 70;
    private bool _startWithWindows;
    private string _validationMessage = string.Empty;

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

    public int CpuThreshold
    {
        get => _cpuThreshold;
        set => SetProperty(ref _cpuThreshold, value);
    }

    public int MemoryThreshold
    {
        get => _memoryThreshold;
        set => SetProperty(ref _memoryThreshold, value);
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetProperty(ref _startWithWindows, value);
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
        if (CpuThreshold < 0 || CpuThreshold > 100)
        {
            ValidationMessage = "CPU Threshold must be between 0 and 100.";
            return false;
        }

        if (MemoryThreshold < 0 || MemoryThreshold > 100)
        {
            ValidationMessage = "Memory Threshold must be between 0 and 100.";
            return false;
        }

        return true;
    }

    public void Save()
    {
        // TODO: Persist configuration to file or registry
        // For now, this is a placeholder
    }

    public void Load()
    {
        // TODO: Load configuration from file or registry
        // For now, this uses defaults
    }
}

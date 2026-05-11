using System.Text.Json.Serialization;

namespace ElBruno.AspireMonitor.Models;

public class Configuration
{
    public const string DefaultAspireEndpoint = "http://localhost:18888";

    public string AspireEndpoint { get; set; } = DefaultAspireEndpoint;
    public int PollingIntervalMs { get; set; } = 5000;
    public bool StartWithWindows { get; set; }
    public string ProjectFolder { get; set; } = string.Empty;
    public int CpuThresholdWarning { get; set; } = 70;
    public int CpuThresholdCritical { get; set; } = 90;
    public int MemoryThresholdWarning { get; set; } = 70;
    public int MemoryThresholdCritical { get; set; } = 90;
    public bool HideDevelopmentResources { get; set; }
    public string MiniWindowResources { get; set; } = string.Empty;
    public bool ShowMiniWindowResourceTelemetry { get; set; } = true;
    public bool ShowOnlyMainMiniWindowResources { get; set; } = true;
    [JsonPropertyName("notifyOnStateChange")]
    public bool EnableAspireStateNotifications { get; set; } = true;
}

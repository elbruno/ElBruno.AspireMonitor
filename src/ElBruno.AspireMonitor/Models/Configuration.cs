namespace ElBruno.AspireMonitor.Models;

public class Configuration
{
    public string AspireEndpoint { get; set; } = "http://localhost:18888";
    public int PollingIntervalMs { get; set; } = 5000;
    public int CpuThresholdWarning { get; set; } = 70;
    public int CpuThresholdCritical { get; set; } = 90;
    public int MemoryThresholdWarning { get; set; } = 70;
    public int MemoryThresholdCritical { get; set; } = 90;
    public bool StartWithWindows { get; set; }
    public string ProjectFolder { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
}

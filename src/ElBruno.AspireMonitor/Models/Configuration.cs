namespace ElBruno.AspireMonitor.Models;

public class Configuration
{
    public int PollingIntervalMs { get; set; } = 5000;
    public bool StartWithWindows { get; set; }
    public string ProjectFolder { get; set; } = string.Empty;
}

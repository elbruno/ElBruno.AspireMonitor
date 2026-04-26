namespace ElBruno.AspireMonitor.Models;

public class AspireHost
{
    public string Url { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Version { get; set; }
    public List<AspireResource> Resources { get; set; } = new();
    public StatusColor OverallStatus { get; set; } = StatusColor.Unknown;

    public AspireHost()
    {
    }

    public AspireHost(string url, string name)
    {
        Url = url;
        Name = name;
    }
}

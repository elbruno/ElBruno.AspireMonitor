namespace ElBruno.AspireMonitor.Models;

public class AspireEndpoint
{
    public string? EndpointUrl { get; set; }
    public string? ProxyUrl { get; set; }

    public string? DisplayUrl => !string.IsNullOrWhiteSpace(ProxyUrl)
        ? ProxyUrl
        : EndpointUrl;
}

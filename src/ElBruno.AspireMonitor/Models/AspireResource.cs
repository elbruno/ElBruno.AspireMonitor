using System.Text.Json.Serialization;

namespace ElBruno.AspireMonitor.Models;

public class AspireResource
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("resourceType")]
    public string? Type { get; set; }
    public ResourceStatus Status { get; set; } = ResourceStatus.Unknown;
    [JsonPropertyName("properties")]
    public ResourceMetrics Metrics { get; set; } = new();
    [JsonPropertyName("environment")]
    public List<AspireEnvironmentEntry> Environment { get; set; } = new();
    public List<AspireEndpoint> Endpoints { get; set; } = new();

    [JsonIgnore]
    public bool IsDevelopmentOnly => Environment.Any(environment => environment.IsDevelopmentEnvironment);

    public AspireResource()
    {
    }

    public AspireResource(string id, string name, ResourceStatus status = ResourceStatus.Unknown)
    {
        Id = id;
        Name = name;
        Status = status;
    }
}

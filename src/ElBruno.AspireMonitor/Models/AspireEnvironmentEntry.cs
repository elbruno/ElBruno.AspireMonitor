using System.Text.Json.Serialization;

namespace ElBruno.AspireMonitor.Models;

public class AspireEnvironmentEntry
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonIgnore]
    public bool IsDevelopmentEnvironment =>
        string.Equals(Value, "Development", StringComparison.OrdinalIgnoreCase) &&
        (string.Equals(Name, "ASPNETCORE_ENVIRONMENT", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(Name, "DOTNET_ENVIRONMENT", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(Name, "environment", StringComparison.OrdinalIgnoreCase));

    [JsonIgnore]
    public string DisplayText => string.IsNullOrWhiteSpace(Name)
        ? Value ?? string.Empty
        : $"{Name}={Value}";
}

using System.Text.Json.Serialization;

namespace ElBruno.AspireMonitor.Models;

public class ResourceMetrics
{
    [JsonPropertyName("cpuUsage")]
    public double CpuUsagePercent { get; set; }

    [JsonPropertyName("memoryUsage")]
    public double MemoryUsage { get; set; }

    [JsonPropertyName("memoryLimit")]
    public double MemoryLimit { get; set; }

    [JsonPropertyName("diskUsage")]
    public double DiskUsagePercent { get; set; }

    [JsonIgnore]
    public double MemoryUsagePercent => MemoryLimit > 0
        ? (MemoryUsage / MemoryLimit) * 100
        : MemoryUsage;

    public ResourceMetrics()
    {
        MemoryLimit = 100;
    }

    public ResourceMetrics(double cpuUsage, double memoryUsage, double diskUsage = 0)
    {
        CpuUsagePercent = cpuUsage;
        MemoryUsage = memoryUsage;
        MemoryLimit = 100;
        DiskUsagePercent = diskUsage;
    }
}

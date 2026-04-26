namespace ElBruno.AspireMonitor.Models;

public class ResourceMetrics
{
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public double DiskUsagePercent { get; set; }

    public ResourceMetrics()
    {
        CpuUsagePercent = 0;
        MemoryUsagePercent = 0;
        DiskUsagePercent = 0;
    }

    public ResourceMetrics(double cpuUsage, double memoryUsage, double diskUsage = 0)
    {
        CpuUsagePercent = cpuUsage;
        MemoryUsagePercent = memoryUsage;
        DiskUsagePercent = diskUsage;
    }
}

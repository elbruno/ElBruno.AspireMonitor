using ElBruno.AspireMonitor.Models;

namespace ElBruno.AspireMonitor.Services;

public class StatusCalculator
{
    private readonly Configuration _configuration;

    public StatusCalculator(Configuration configuration)
    {
        _configuration = configuration;
    }

    public StatusCalculator() : this(new Configuration())
    {
    }

    public StatusColor CalculateStatus(double cpuPercent, double memoryPercent)
    {
        if (cpuPercent < 0 || memoryPercent < 0)
            return StatusColor.Unknown;

        if (cpuPercent >= _configuration.CpuThresholdCritical || 
            memoryPercent >= _configuration.MemoryThresholdCritical)
            return StatusColor.Red;

        if (cpuPercent >= _configuration.CpuThresholdWarning || 
            memoryPercent >= _configuration.MemoryThresholdWarning)
            return StatusColor.Yellow;

        return StatusColor.Green;
    }

    public StatusColor CalculateStatusFromMetrics(ResourceMetrics metrics)
    {
        return CalculateStatus(metrics.CpuUsagePercent, metrics.MemoryUsagePercent);
    }

    public StatusColor CalculateOverallStatus(IEnumerable<AspireResource> resources)
    {
        if (!resources.Any())
            return StatusColor.Unknown;

        var hasRed = false;
        var hasYellow = false;

        foreach (var resource in resources)
        {
            if (resource.Status == ResourceStatus.Stopped || resource.Status == ResourceStatus.Unknown)
            {
                hasRed = true;
                continue;
            }

            var status = CalculateStatusFromMetrics(resource.Metrics);
            if (status == StatusColor.Red)
                hasRed = true;
            else if (status == StatusColor.Yellow)
                hasYellow = true;
        }

        if (hasRed) return StatusColor.Red;
        if (hasYellow) return StatusColor.Yellow;
        return StatusColor.Green;
    }
}

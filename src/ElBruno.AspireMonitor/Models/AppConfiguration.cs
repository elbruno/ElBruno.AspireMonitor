namespace ElBruno.AspireMonitor.Models;

public class AppConfiguration
{
    public string AspireEndpoint { get; set; } = "http://localhost:18888";
    public int PollingIntervalMs { get; set; } = 2000;
    public int CpuThresholdWarning { get; set; } = 70;
    public int CpuThresholdCritical { get; set; } = 90;
    public int MemoryThresholdWarning { get; set; } = 70;
    public int MemoryThresholdCritical { get; set; } = 90;
    public int HttpTimeoutSeconds { get; set; } = 5;
    public int MaxRetries { get; set; } = 3;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(AspireEndpoint))
            throw new InvalidOperationException("AspireEndpoint cannot be empty");

        if (!Uri.TryCreate(AspireEndpoint, UriKind.Absolute, out _))
            throw new InvalidOperationException("AspireEndpoint must be a valid URL");

        if (PollingIntervalMs < 500 || PollingIntervalMs > 60000)
            throw new InvalidOperationException("PollingIntervalMs must be between 500 and 60000");

        if (CpuThresholdWarning < 0 || CpuThresholdWarning > 100)
            throw new InvalidOperationException("CpuThresholdWarning must be between 0 and 100");

        if (CpuThresholdCritical < 0 || CpuThresholdCritical > 100)
            throw new InvalidOperationException("CpuThresholdCritical must be between 0 and 100");

        if (MemoryThresholdWarning < 0 || MemoryThresholdWarning > 100)
            throw new InvalidOperationException("MemoryThresholdWarning must be between 0 and 100");

        if (MemoryThresholdCritical < 0 || MemoryThresholdCritical > 100)
            throw new InvalidOperationException("MemoryThresholdCritical must be between 0 and 100");

        if (CpuThresholdWarning >= CpuThresholdCritical)
            throw new InvalidOperationException("CpuThresholdWarning must be less than CpuThresholdCritical");

        if (MemoryThresholdWarning >= MemoryThresholdCritical)
            throw new InvalidOperationException("MemoryThresholdWarning must be less than MemoryThresholdCritical");

        if (HttpTimeoutSeconds < 1 || HttpTimeoutSeconds > 30)
            throw new InvalidOperationException("HttpTimeoutSeconds must be between 1 and 30");

        if (MaxRetries < 0 || MaxRetries > 10)
            throw new InvalidOperationException("MaxRetries must be between 0 and 10");
    }
}

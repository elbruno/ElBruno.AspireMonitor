namespace ElBruno.AspireMonitor.SampleHarness.Worker;

public static class HeartbeatFormatter
{
    public static string Format(DateTimeOffset timestamp) =>
        $"worker heartbeat at {timestamp:O}";
}

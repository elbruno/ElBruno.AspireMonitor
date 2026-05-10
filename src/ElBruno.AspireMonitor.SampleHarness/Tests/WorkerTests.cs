namespace ElBruno.AspireMonitor.SampleHarness.Tests;

public class WorkerTests
{
    [Fact]
    public void Heartbeat_formatter_uses_invariant_roundtrip_format()
    {
        var timestamp = new DateTimeOffset(2026, 5, 10, 12, 34, 56, TimeSpan.Zero);

        var message = ElBruno.AspireMonitor.SampleHarness.Worker.HeartbeatFormatter.Format(timestamp);

        Assert.Equal("worker heartbeat at 2026-05-10T12:34:56.0000000+00:00", message);
    }
}

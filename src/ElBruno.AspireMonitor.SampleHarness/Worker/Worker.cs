namespace ElBruno.AspireMonitor.SampleHarness.Worker;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("{message}", HeartbeatFormatter.Format(DateTimeOffset.UtcNow));
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}

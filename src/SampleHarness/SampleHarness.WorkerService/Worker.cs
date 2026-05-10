namespace SampleHarness.WorkerService;

/// <summary>
/// Background service that periodically logs a heartbeat and emits a
/// counter metric — gives AspireMonitor a live resource with observable
/// CPU/memory activity rather than an idle process.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private int _iteration;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _iteration++;
            _logger.LogInformation(
                "SampleHarness heartbeat #{Iteration} at {Time}",
                _iteration,
                DateTimeOffset.UtcNow);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}

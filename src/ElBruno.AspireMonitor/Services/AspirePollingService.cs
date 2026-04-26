using System.Timers;
using ElBruno.AspireMonitor.Models;

namespace ElBruno.AspireMonitor.Services;

public enum PollingServiceState
{
    Idle,
    Connecting,
    Polling,
    Error,
    Reconnecting
}

public class AspirePollingService : IAspirePollingService, IDisposable
{
    private readonly AspireCliService _cliService;
    private readonly System.Timers.Timer _pollingTimer;
    private PollingServiceState _state;
    private List<AspireResource> _lastKnownResources;
    private int _reconnectAttempts;
    private bool _disposed;
    private readonly int _pollingIntervalMs;

    public event EventHandler<List<AspireResource>>? ResourcesUpdated;
    public event EventHandler<string>? StatusChanged;
    public event EventHandler<string>? ErrorOccurred;

    public PollingServiceState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                StatusChanged?.Invoke(this, GetStatusMessage(value));
            }
        }
    }

    public AspirePollingService(AspireCliService cliService, int pollingIntervalMs = 2000)
    {
        _cliService = cliService;
        _pollingIntervalMs = pollingIntervalMs;
        _state = PollingServiceState.Idle;
        _lastKnownResources = new List<AspireResource>();
        _reconnectAttempts = 0;

        _pollingTimer = new System.Timers.Timer(_pollingIntervalMs);
        _pollingTimer.Elapsed += OnPollingTimerElapsed;
        _pollingTimer.AutoReset = true;
    }

    public void Start()
    {
        if (_state == PollingServiceState.Polling || _state == PollingServiceState.Connecting)
            return;

        System.Diagnostics.Debug.WriteLine($"[AspirePollingService] Starting polling service. Interval: {_pollingIntervalMs}ms");
        State = PollingServiceState.Connecting;
        _reconnectAttempts = 0;
        _pollingTimer.Start();
    }

    public void Stop()
    {
        _pollingTimer.Stop();
        State = PollingServiceState.Idle;
    }

    public async Task RefreshAsync()
    {
        System.Diagnostics.Debug.WriteLine("[AspirePollingService] RefreshAsync called");
        await PollResourcesAsync();
    }

    public void UpdateEndpoint(string newEndpoint)
    {
        System.Diagnostics.Debug.WriteLine($"[AspirePollingService] UpdateEndpoint called with: {newEndpoint} (CLI-based, endpoint not used)");
    }

    private async void OnPollingTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await PollResourcesAsync();
    }

    private async Task PollResourcesAsync()
    {
        if (_state == PollingServiceState.Idle)
            return;

        try
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            System.Diagnostics.Debug.WriteLine($"[AspirePollingService] [{timestamp}] Polling cycle started. State: {_state}");

            var resourceCollection = await _cliService.ParseResourcesFromDescribeJsonAsync();

            if (!string.IsNullOrEmpty(resourceCollection.ErrorMessage))
            {
                System.Diagnostics.Debug.WriteLine($"[AspirePollingService] [{timestamp}] Error: {resourceCollection.ErrorMessage}");
                HandleError(resourceCollection.ErrorMessage);
                return;
            }

            var resources = resourceCollection.Resources;
            System.Diagnostics.Debug.WriteLine($"[AspirePollingService] [{timestamp}] Poll completed. Resources returned: {resources.Count}");

            if (resources.Count > 0 || _state == PollingServiceState.Connecting)
            {
                _lastKnownResources = resources;
                _reconnectAttempts = 0;

                if (_state != PollingServiceState.Polling)
                {
                    State = PollingServiceState.Polling;
                }

                foreach (var resource in resources)
                {
                    System.Diagnostics.Debug.WriteLine($"[AspirePollingService] Resource: {resource.Name} (Status: {resource.Status}, CPU: {resource.Metrics.CpuUsagePercent:F1}%, Memory: {resource.Metrics.MemoryUsagePercent:F1}%)");
                }

                ResourcesUpdated?.Invoke(this, resources);
            }
            else if (_lastKnownResources.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[AspirePollingService] [{timestamp}] No resources available and no last-known state");
                HandleError("No Aspire resources detected");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AspirePollingService] [{timestamp}] Empty response, using last-known state with {_lastKnownResources.Count} resources");
                ResourcesUpdated?.Invoke(this, _lastKnownResources);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspirePollingService] Polling exception: {ex.GetType().Name}: {ex.Message}");
            HandleError($"Polling error: {ex.Message}");
        }
    }

    private void HandleError(string message)
    {
        State = PollingServiceState.Error;
        _reconnectAttempts++;
        var backoffDelay = CalculateBackoffDelay(_reconnectAttempts);

        System.Diagnostics.Debug.WriteLine($"[AspirePollingService] ERROR: {message}");
        System.Diagnostics.Debug.WriteLine($"[AspirePollingService] Reconnect attempt #{_reconnectAttempts}, waiting {backoffDelay.TotalSeconds}s before retry");
        
        ErrorOccurred?.Invoke(this, message);

        Task.Delay(backoffDelay).ContinueWith(_ =>
        {
            if (_state == PollingServiceState.Error)
            {
                System.Diagnostics.Debug.WriteLine($"[AspirePollingService] Backoff delay complete, transitioning to Reconnecting");
                State = PollingServiceState.Reconnecting;
                State = PollingServiceState.Connecting;
            }
        });
    }

    private TimeSpan CalculateBackoffDelay(int attempts)
    {
        return attempts switch
        {
            1 => TimeSpan.FromSeconds(5),
            2 => TimeSpan.FromSeconds(10),
            _ => TimeSpan.FromSeconds(30)
        };
    }

    private string GetStatusMessage(PollingServiceState state)
    {
        return state switch
        {
            PollingServiceState.Idle => "Idle",
            PollingServiceState.Connecting => "Connecting",
            PollingServiceState.Polling => "Connected",
            PollingServiceState.Error => "Error",
            PollingServiceState.Reconnecting => "Reconnecting",
            _ => "Unknown"
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _pollingTimer?.Stop();
            _pollingTimer?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

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
    private readonly AspireApiClient _apiClient;
    private readonly Configuration _configuration;
    private readonly System.Timers.Timer _pollingTimer;
    private PollingServiceState _state;
    private List<AspireResource> _lastKnownResources;
    private int _reconnectAttempts;
    private bool _disposed;

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

    public AspirePollingService(AspireApiClient apiClient, Configuration configuration)
    {
        _apiClient = apiClient;
        _configuration = configuration;
        _state = PollingServiceState.Idle;
        _lastKnownResources = new List<AspireResource>();
        _reconnectAttempts = 0;

        _pollingTimer = new System.Timers.Timer(_configuration.PollingIntervalMs);
        _pollingTimer.Elapsed += OnPollingTimerElapsed;
        _pollingTimer.AutoReset = true;
    }

    public void Start()
    {
        if (_state == PollingServiceState.Polling || _state == PollingServiceState.Connecting)
            return;

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
        await PollResourcesAsync();
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
            var resources = await _apiClient.GetResourcesAsync();

            if (resources.Count > 0 || _state == PollingServiceState.Connecting)
            {
                _lastKnownResources = resources;
                _reconnectAttempts = 0;

                if (_state != PollingServiceState.Polling)
                {
                    State = PollingServiceState.Polling;
                }

                ResourcesUpdated?.Invoke(this, resources);
            }
            else if (_lastKnownResources.Count == 0)
            {
                HandleError("No resources available");
            }
            else
            {
                ResourcesUpdated?.Invoke(this, _lastKnownResources);
            }
        }
        catch (Exception ex)
        {
            HandleError($"Polling error: {ex.Message}");
        }
    }

    private void HandleError(string message)
    {
        State = PollingServiceState.Error;
        ErrorOccurred?.Invoke(this, message);

        _reconnectAttempts++;
        var backoffDelay = CalculateBackoffDelay(_reconnectAttempts);

        Task.Delay(backoffDelay).ContinueWith(_ =>
        {
            if (_state == PollingServiceState.Error)
            {
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

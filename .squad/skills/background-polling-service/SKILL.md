# Skill: Background Polling Service with Auto-Reconnect

**Category:** Architecture Pattern  
**Language:** C# / .NET  
**Use Cases:** Real-time data synchronization, API polling, health monitoring  
**Complexity:** Medium

---

## Overview

Reusable pattern for implementing a background polling service with state machine, exponential backoff retry, and graceful error recovery. Suitable for WPF/WinForms apps that need to periodically fetch data from APIs without blocking the UI.

---

## Pattern Components

### 1. Service State Machine

```csharp
public enum PollingServiceState
{
    Idle,          // Not polling (initial, after Stop())
    Connecting,    // First connection attempt
    Polling,       // Normal operation
    Error,         // Failed, preparing to reconnect
    Reconnecting   // Re-attempting connection
}
```

### 2. Event-Driven Updates

```csharp
public event EventHandler<List<TData>>? DataUpdated;
public event EventHandler<string>? StatusChanged;
public event EventHandler<string>? ErrorOccurred;
```

### 3. Timer-Based Polling

```csharp
private readonly System.Timers.Timer _pollingTimer;

public PollingService(int intervalMs)
{
    _pollingTimer = new System.Timers.Timer(intervalMs);
    _pollingTimer.Elapsed += OnPollingTimerElapsed;
    _pollingTimer.AutoReset = true;
}

public void Start()
{
    if (_state == PollingServiceState.Polling) return;
    
    _state = PollingServiceState.Connecting;
    _reconnectAttempts = 0;
    _pollingTimer.Start();
}

public void Stop()
{
    _pollingTimer.Stop();
    _state = PollingServiceState.Idle;
}
```

### 4. Exponential Backoff Retry

```csharp
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
        _ => TimeSpan.FromSeconds(30) // capped
    };
}
```

### 5. Last-Known-Good State

```csharp
private List<TData> _lastKnownData = new();

private async void OnPollingTimerElapsed(object? sender, ElapsedEventArgs e)
{
    try
    {
        var data = await FetchDataAsync();
        
        if (data.Count > 0 || _state == PollingServiceState.Connecting)
        {
            _lastKnownData = data;
            _reconnectAttempts = 0;
            
            if (_state != PollingServiceState.Polling)
                State = PollingServiceState.Polling;
            
            DataUpdated?.Invoke(this, data);
        }
        else if (_lastKnownData.Count == 0)
        {
            HandleError("No data available");
        }
        else
        {
            // Use cached data during transient failure
            DataUpdated?.Invoke(this, _lastKnownData);
        }
    }
    catch (Exception ex)
    {
        HandleError($"Polling error: {ex.Message}");
    }
}
```

### 6. IDisposable Pattern

```csharp
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
```

---

## Usage in ViewModel

```csharp
public class MainViewModel : IDisposable
{
    private readonly PollingService _pollingService;

    public MainViewModel()
    {
        _pollingService = new PollingService(intervalMs: 2000);
        
        _pollingService.DataUpdated += OnDataUpdated;
        _pollingService.StatusChanged += OnStatusChanged;
        _pollingService.ErrorOccurred += OnError;
        
        _pollingService.Start();
    }

    private void OnDataUpdated(object? sender, List<MyData> data)
    {
        // Marshal to UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            MyCollection.Clear();
            foreach (var item in data)
                MyCollection.Add(item);
        });
    }

    public void Dispose()
    {
        _pollingService?.Stop();
        _pollingService?.Dispose();
    }
}
```

---

## Key Benefits

1. **Non-Blocking:** Runs on background thread, never blocks UI
2. **Resilient:** Auto-recovers from transient failures
3. **Graceful:** Preserves last-known-good data during outages
4. **Configurable:** Interval, backoff delays, max retries
5. **Testable:** Events can be mocked, state transitions observable
6. **Resource-Safe:** Proper disposal prevents leaks

---

## When to Use

✅ **Use when:**
- Polling REST APIs for updates (every 1-60 seconds)
- Monitoring external services (health checks, status updates)
- Fetching data that changes frequently but not real-time
- Building dashboard/monitoring apps
- Need auto-reconnect on network issues

❌ **Don't use when:**
- Real-time updates required (use SignalR/WebSockets instead)
- Polling interval <500ms (too aggressive, consider push)
- One-time data fetch (use simple async/await)
- Data rarely changes (poll less frequently or use webhooks)

---

## Related Patterns

- **Observer Pattern:** Events notify subscribers of data changes
- **State Machine:** Explicit states for connection lifecycle
- **Circuit Breaker:** Exponential backoff prevents cascade failures
- **Repository Pattern:** Abstract data source behind interface

---

**Last Updated:** 2026-04-26  
**Author:** Luke (Backend Developer)  
**Project:** ElBruno.AspireMonitor

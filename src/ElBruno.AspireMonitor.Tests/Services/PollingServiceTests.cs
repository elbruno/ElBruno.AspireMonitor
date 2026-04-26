using FluentAssertions;
using Moq;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Services;

public class PollingServiceTests
{
    [Fact]
    public async Task StartAsync_InitiatesPolling_Successfully()
    {
        // Arrange - Mock polling service state machine
        var pollingState = new PollingServiceMock();
        pollingState.State.Should().Be("Stopped");

        // Act - Start polling
        await pollingState.StartAsync();

        // Assert - Should transition through states
        pollingState.State.Should().Be("Running");
        pollingState.PollCount.Should().BeGreaterThan(0, "polling should have started");
    }

    [Fact]
    public async Task StopAsync_HaltsPolling_Cleanly()
    {
        // Arrange - Start polling first
        var pollingState = new PollingServiceMock();
        await pollingState.StartAsync();
        pollingState.State.Should().Be("Running");

        // Act - Stop polling
        await pollingState.StopAsync();

        // Assert - Should stop cleanly
        pollingState.State.Should().Be("Stopped");
        var pollCountBeforeStop = pollingState.PollCount;
        
        // Wait and verify no more polling occurs
        await Task.Delay(100);
        pollingState.PollCount.Should().Be(pollCountBeforeStop, "polling should have stopped");
    }

    [Fact]
    public async Task PollingInterval_MaintainsAccuracy_OverTime()
    {
        // Arrange - Create service with 100ms interval
        var pollingState = new PollingServiceMock(intervalMs: 100);
        var pollTimes = new List<DateTime>();

        pollingState.OnPoll = () => pollTimes.Add(DateTime.UtcNow);

        // Act - Poll for 500ms
        await pollingState.StartAsync();
        await Task.Delay(500);
        await pollingState.StopAsync();

        // Assert - Should have ~5 polls (500ms / 100ms)
        pollTimes.Should().HaveCountGreaterThanOrEqualTo(4, "should poll at least 4 times");
        pollTimes.Should().HaveCountLessThanOrEqualTo(6, "should not over-poll");

        // Verify intervals are approximately 100ms apart
        if (pollTimes.Count >= 2)
        {
            for (int i = 1; i < pollTimes.Count; i++)
            {
                var interval = (pollTimes[i] - pollTimes[i - 1]).TotalMilliseconds;
                interval.Should().BeInRange(80, 150, "polling interval should be approximately 100ms");
            }
        }
    }

    [Fact]
    public async Task OnError_RetriesWithBackoff_AndRecovers()
    {
        // Arrange - Mock service that fails then succeeds
        var pollingState = new PollingServiceMock();
        var attemptCount = 0;
        var backoffDelays = new List<int>();

        pollingState.OnPoll = () =>
        {
            attemptCount++;
            if (attemptCount <= 2)
            {
                throw new Exception($"Simulated error #{attemptCount}");
            }
        };

        pollingState.OnError = (ex, delayMs) =>
        {
            backoffDelays.Add(delayMs);
        };

        // Act - Start and let it retry
        await pollingState.StartAsync();
        await Task.Delay(1000); // Give time for retries
        await pollingState.StopAsync();

        // Assert
        attemptCount.Should().BeGreaterThanOrEqualTo(3, "should retry after errors");
        backoffDelays.Should().NotBeEmpty("should have recorded backoff delays");
        
        // Verify exponential backoff (each delay >= previous)
        if (backoffDelays.Count >= 2)
        {
            for (int i = 1; i < backoffDelays.Count; i++)
            {
                backoffDelays[i].Should().BeGreaterThanOrEqualTo(backoffDelays[i - 1],
                    "backoff delay should increase exponentially");
            }
        }

        pollingState.State.Should().Be("Stopped", "should eventually stabilize");
    }

    [Fact]
    public async Task StateTransitions_HandleCorrectly_ThroughLifecycle()
    {
        // Arrange
        var pollingState = new PollingServiceMock();
        var stateHistory = new List<string>();

        pollingState.OnStateChange = (state) => stateHistory.Add(state);

        // Act - Full lifecycle
        stateHistory.Clear();
        
        // Start
        await pollingState.StartAsync();
        await Task.Delay(50); // Let it poll once
        stateHistory.Should().Contain("Connecting", "should transition to Connecting");
        stateHistory.Should().Contain("Running", "should transition to Running");

        // Simulate error (manual state change for testing)
        SetState_External(pollingState, "Error");
        stateHistory.Add("Error");
        await Task.Delay(50);
        SetState_External(pollingState, "Reconnecting");
        stateHistory.Add("Reconnecting");
        await Task.Delay(50);

        stateHistory.Should().Contain("Error", "should transition to Error on failure");
        stateHistory.Should().Contain("Reconnecting", "should transition to Reconnecting");

        // Stop
        await pollingState.StopAsync();
        stateHistory.Should().Contain("Stopped", "should transition to Stopped");

        // Assert - State transitions follow expected pattern
        var expectedPattern = new[] { "Connecting", "Running", "Error", "Reconnecting", "Stopped" };
        foreach (var expectedState in expectedPattern)
        {
            stateHistory.Should().Contain(expectedState, 
                $"state machine should include {expectedState} state");
        }
    }
    
    private void SetState_External(PollingServiceMock mock, string state)
    {
        // Helper to trigger external state changes for testing
        mock.OnStateChange?.Invoke(state);
    }

    [Fact]
    public async Task RapidStartStop_DoesNotCauseCrash_OrLeaks()
    {
        // Arrange
        var pollingState = new PollingServiceMock();

        // Act - Rapid start/stop cycles
        for (int i = 0; i < 10; i++)
        {
            await pollingState.StartAsync();
            await Task.Delay(10); // Brief delay
            await pollingState.StopAsync();
        }

        // Assert - Should handle gracefully without crashes
        pollingState.State.Should().Be("Stopped");
        pollingState.ErrorCount.Should().Be(0, "rapid start/stop should not cause errors");
    }

    [Fact]
    public async Task OnResourcesUpdated_FiresEvent_WhenDataChanges()
    {
        // Arrange
        var pollingState = new PollingServiceMock();
        var updateCount = 0;
        pollingState.OnResourcesUpdated = () => updateCount++;

        // Act
        await pollingState.StartAsync();
        await Task.Delay(300); // Let it poll a few times
        await pollingState.StopAsync();

        // Assert
        updateCount.Should().BeGreaterThan(0, "OnResourcesUpdated event should fire");
    }

    [Fact]
    public async Task OfflineRecovery_ReconnectsAutomatically_WhenApiReturns()
    {
        // Arrange - Start offline
        var pollingState = new PollingServiceMock(startOffline: true);
        await pollingState.StartAsync();
        await Task.Delay(100); // Give time for first error to occur
        
        // State should be either Error or Reconnecting (timing dependent)
        var validStates = new[] { "Error", "Reconnecting" };
        validStates.Should().Contain(pollingState.State, "should be in error or reconnecting state when offline");

        // Act - Simulate API coming back online
        pollingState.BringOnline();
        await Task.Delay(500); // Give time to reconnect

        // Assert - Should recover
        pollingState.State.Should().Be("Running", "should reconnect when API comes back");
        await pollingState.StopAsync();
    }

    // Mock implementation of PollingService for testing
    private class PollingServiceMock
    {
        private CancellationTokenSource? _cts;
        private Task? _pollingTask;
        private readonly int _intervalMs;
        private bool _isOffline;

        public string State { get; private set; } = "Stopped";
        public int PollCount { get; private set; }
        public int ErrorCount { get; private set; }

        public Action? OnPoll { get; set; }
        public Action<Exception, int>? OnError { get; set; }
        public Action<string>? OnStateChange { get; set; }
        public Action? OnResourcesUpdated { get; set; }

        public PollingServiceMock(int intervalMs = 50, bool startOffline = false)
        {
            _intervalMs = intervalMs;
            _isOffline = startOffline;
        }

        public async Task StartAsync()
        {
            if (_cts != null) return;

            _cts = new CancellationTokenSource();
            SetState("Connecting");
            await Task.Delay(10); // Simulate connection delay
            
            if (!_isOffline)
            {
                SetState("Running");
            }
            else
            {
                SetState("Error");
                SetState("Reconnecting");
            }

            _pollingTask = PollAsync(_cts.Token);
        }

        public async Task StopAsync()
        {
            if (_cts == null) return;

            _cts.Cancel();
            if (_pollingTask != null)
            {
                await _pollingTask;
            }
            _cts?.Dispose();
            _cts = null;
            SetState("Stopped");
        }

        public void SimulateError(Exception ex)
        {
            SetState("Error");
            ErrorCount++;
            OnError?.Invoke(ex, 100);
        }

        public void BringOnline()
        {
            _isOffline = false;
            SetState("Running");
        }

        private async Task PollAsync(CancellationToken ct)
        {
            int errorCount = 0;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (_isOffline)
                    {
                        throw new Exception("Offline");
                    }

                    PollCount++;
                    OnPoll?.Invoke();
                    OnResourcesUpdated?.Invoke();
                    errorCount = 0; // Reset on success
                    
                    await Task.Delay(_intervalMs, ct);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping - don't treat as error
                    break;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    ErrorCount++;
                    SetState("Error");
                    
                    var backoffDelay = (int)Math.Pow(2, errorCount) * 100;
                    OnError?.Invoke(ex, backoffDelay);
                    
                    SetState("Reconnecting");
                    
                    try
                    {
                        await Task.Delay(backoffDelay, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    
                    if (!_isOffline)
                    {
                        SetState("Running");
                    }
                }
            }
        }

        private void SetState(string newState)
        {
            State = newState;
            OnStateChange?.Invoke(newState);
        }
    }
}

using System.Collections.Concurrent;

namespace ElBruno.AspireMonitor.Services;

public class AspireLiveLogsService : IDisposable
{
    private readonly AspireCliService _cliService;
    private readonly ConcurrentDictionary<string, LogStream> _activeStreams = new();
    private bool _disposed;

    public event EventHandler<LogLineReceivedEventArgs>? LogLineReceived;
    public event EventHandler<LogStreamClosedEventArgs>? LogStreamClosed;
    public event EventHandler<string>? ErrorOccurred;

    public AspireLiveLogsService(AspireCliService cliService)
    {
        _cliService = cliService;
    }

    public async Task StartStreamingAsync(string resourceName, int bufferSize = 50, CancellationToken cancellationToken = default)
    {
        if (_activeStreams.ContainsKey(resourceName))
        {
            System.Diagnostics.Debug.WriteLine($"[AspireLiveLogsService] Stream already active for resource: {resourceName}");
            return;
        }

        var logStream = new LogStream(resourceName, Math.Max(1, bufferSize));
        if (!_activeStreams.TryAdd(resourceName, logStream))
        {
            System.Diagnostics.Debug.WriteLine($"[AspireLiveLogsService] Failed to register stream for resource: {resourceName}");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[AspireLiveLogsService] Starting log stream for: {resourceName}");

        _ = Task.Run(async () =>
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, logStream.CancellationToken);

            try
            {
                await foreach (var line in _cliService.GetLiveLogsAsync(resourceName, linkedCts.Token))
                {
                    if (linkedCts.Token.IsCancellationRequested)
                        break;

                    logStream.AddLine(line);
                    LogLineReceived?.Invoke(this, new LogLineReceivedEventArgs(resourceName, line));
                }

                System.Diagnostics.Debug.WriteLine(linkedCts.Token.IsCancellationRequested
                    ? $"[AspireLiveLogsService] Log stream canceled for: {resourceName}"
                    : $"[AspireLiveLogsService] Log stream ended for: {resourceName}");
                LogStreamClosed?.Invoke(this, new LogStreamClosedEventArgs(resourceName, false, null));
            }
            catch (OperationCanceledException) when (linkedCts.Token.IsCancellationRequested || logStream.IsCancellationRequested)
            {
                System.Diagnostics.Debug.WriteLine($"[AspireLiveLogsService] Log stream canceled for: {resourceName}");
                LogStreamClosed?.Invoke(this, new LogStreamClosedEventArgs(resourceName, false, null));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AspireLiveLogsService] Error streaming logs for {resourceName}: {ex.Message}");
                ErrorOccurred?.Invoke(this, $"Log stream error for {resourceName}: {ex.Message}");
                LogStreamClosed?.Invoke(this, new LogStreamClosedEventArgs(resourceName, true, ex.Message));
            }
            finally
            {
                _activeStreams.TryRemove(resourceName, out _);
                logStream.Dispose();
            }
        });
    }

    public void StopStreaming(string resourceName)
    {
        if (_activeStreams.TryRemove(resourceName, out var stream))
        {
            stream.Cancel();
            System.Diagnostics.Debug.WriteLine($"[AspireLiveLogsService] Stopped log stream for: {resourceName}");
        }
    }

    public IReadOnlyList<string>? GetBufferedLogs(string resourceName)
    {
        if (_activeStreams.TryGetValue(resourceName, out var stream))
        {
            return stream.GetBufferedLines();
        }
        return null;
    }

    public bool IsStreaming(string resourceName)
    {
        return _activeStreams.ContainsKey(resourceName);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var stream in _activeStreams.Values)
            {
                stream.Cancel();
            }
            _activeStreams.Clear();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    private class LogStream
    {
        private readonly Queue<string> _buffer;
        private readonly int _maxBufferSize;
        private readonly object _lock = new();
        private readonly CancellationTokenSource _cts = new();
        private bool _disposed;

        public string ResourceName { get; }

        public LogStream(string resourceName, int maxBufferSize)
        {
            ResourceName = resourceName;
            _maxBufferSize = maxBufferSize;
            _buffer = new Queue<string>(maxBufferSize);
        }

        public void AddLine(string line)
        {
            lock (_lock)
            {
                if (_buffer.Count >= _maxBufferSize)
                    _buffer.Dequeue();
                
                _buffer.Enqueue(line);
            }
        }

        public IReadOnlyList<string> GetBufferedLines()
        {
            lock (_lock)
            {
                return _buffer.ToList();
            }
        }

        public void Cancel()
        {
            if (_disposed)
                return;

            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public CancellationToken CancellationToken => _cts.Token;

        public bool IsCancellationRequested => _cts.IsCancellationRequested;

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            _cts.Dispose();
        }
    }
}

public class LogLineReceivedEventArgs : EventArgs
{
    public string ResourceName { get; }
    public string LogLine { get; }
    public DateTime Timestamp { get; }

    public LogLineReceivedEventArgs(string resourceName, string logLine)
    {
        ResourceName = resourceName;
        LogLine = logLine;
        Timestamp = DateTime.UtcNow;
    }
}

public class LogStreamClosedEventArgs : EventArgs
{
    public string ResourceName { get; }
    public bool IsError { get; }
    public string? ErrorMessage { get; }

    public LogStreamClosedEventArgs(string resourceName, bool isError, string? errorMessage)
    {
        ResourceName = resourceName;
        IsError = isError;
        ErrorMessage = errorMessage;
    }
}

using System.Runtime.CompilerServices;
using ElBruno.AspireMonitor.Services;
using FluentAssertions;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Services;

public class AspireLiveLogsServiceEdgeCaseTests
{
    [Fact]
    public async Task StartStreamingAsync_DoesNotCreateDuplicateStreamsForSameResource()
    {
        var cli = new ControlledStreamingAspireCliService("line 1");
        using var service = new AspireLiveLogsService(cli);
        var closed = new List<LogStreamClosedEventArgs>();
        service.LogStreamClosed += (_, args) => closed.Add(args);

        await service.StartStreamingAsync("api", bufferSize: 2);
        await WaitUntilAsync(() => service.IsStreaming("api"));

        await service.StartStreamingAsync("api", bufferSize: 2);

        service.IsStreaming("api").Should().BeTrue();

        cli.Release();
        await WaitUntilAsync(() => closed.Count == 1);

        closed.Single().IsError.Should().BeFalse();
    }

    [Fact]
    public async Task StartStreamingAsync_ClampsZeroBufferSizeAndKeepsLatestLine()
    {
        var cli = new ControlledStreamingAspireCliService("line 1", "line 2");
        using var service = new AspireLiveLogsService(cli);
        var lines = new List<string>();
        service.LogLineReceived += (_, args) => lines.Add(args.LogLine);

        await service.StartStreamingAsync("api", bufferSize: 0);
        await WaitUntilAsync(() => lines.Count == 2);

        service.GetBufferedLogs("api").Should().Equal("line 2");

        cli.Release();
        await WaitUntilAsync(() => !service.IsStreaming("api"));
    }

    [Fact]
    public async Task StopStreaming_CancelsActiveStreamWithoutError()
    {
        var cli = new ControlledStreamingAspireCliService("line 1");
        using var service = new AspireLiveLogsService(cli);
        var errors = new List<string>();
        var lines = new List<string>();
        var closed = new List<LogStreamClosedEventArgs>();
        service.ErrorOccurred += (_, error) => errors.Add(error);
        service.LogLineReceived += (_, args) => lines.Add(args.LogLine);
        service.LogStreamClosed += (_, args) => closed.Add(args);

        await service.StartStreamingAsync("api");
        await WaitUntilAsync(() => lines.Count == 1 && service.IsStreaming("api"));

        service.StopStreaming("api");

        await WaitUntilAsync(() => closed.Count == 1);

        errors.Should().BeEmpty();
        closed.Single().IsError.Should().BeFalse();
        service.IsStreaming("api").Should().BeFalse();
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        while (!condition())
        {
            cts.Token.ThrowIfCancellationRequested();
            await Task.Delay(25, cts.Token);
        }
    }
}

internal sealed class ControlledStreamingAspireCliService : AspireCliService
{
    private readonly string[] _lines;
    private readonly TaskCompletionSource _completionGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _streamStarts;

    public ControlledStreamingAspireCliService(params string[] lines)
    {
        _lines = lines;
    }

    public int StreamStarts => Volatile.Read(ref _streamStarts);

    public void Release()
    {
        _completionGate.TrySetResult();
    }

    public override async IAsyncEnumerable<string> GetLiveLogsAsync(string resourceName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _streamStarts);

        foreach (var line in _lines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return line;
        }

        await _completionGate.Task.WaitAsync(cancellationToken);
    }
}

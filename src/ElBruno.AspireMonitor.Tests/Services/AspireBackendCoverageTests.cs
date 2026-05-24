using System.Runtime.CompilerServices;
using System.Text.Json;
using ElBruno.AspireMonitor.Models;
using AppConfig = ElBruno.AspireMonitor.Models.Configuration;
using ElBruno.AspireMonitor.Services;
using FluentAssertions;
using Xunit;

namespace ElBruno.AspireMonitor.Tests.Services;

public class AspireCliServiceParsingTests
{
    [Fact]
    public async Task DiscoverAspireConfigWorkingDirectoriesAsync_ReturnsOnlyValidWorktreesUnderBasePath()
    {
        var root = Path.Combine(Path.GetTempPath(), $"AspireCliService_Discovery_{Guid.NewGuid():N}");
        var worktreeA = Path.Combine(root, "wt-a");
        var worktreeB = Path.Combine(root, "wt-b");
        var outside = Path.Combine(Path.GetTempPath(), $"AspireCliService_Outside_{Guid.NewGuid():N}");

        try
        {
            Directory.CreateDirectory(worktreeA);
            Directory.CreateDirectory(worktreeB);
            Directory.CreateDirectory(outside);

            File.WriteAllText(Path.Combine(worktreeA, "aspire.config.json"), """{ "appHost": { "path": "AppHost.cs" } }""");
            File.WriteAllText(Path.Combine(worktreeA, "AppHost.cs"), "// apphost");

            File.WriteAllText(Path.Combine(worktreeB, "aspire.config.json"), """{ "appHost": { "path": "missing/AppHost.cs" } }""");

            File.WriteAllText(Path.Combine(outside, "aspire.config.json"), """{ "appHost": { "path": "AppHost.cs" } }""");
            File.WriteAllText(Path.Combine(outside, "AppHost.cs"), "// apphost");

            var gitOutput = $"""
            worktree {worktreeA}
            worktree {worktreeB}
            worktree {outside}
            """;

            var service = new WorktreeTestAspireCliService(gitOutput, new Dictionary<string, string>())
            {
                EnableWorktreeDiscovery = true,
                WorktreeBasePath = root
            };

            var directories = await service.DiscoverAspireConfigWorkingDirectoriesAsync();

            directories.Should().ContainSingle();
            directories[0].Should().Be(Path.GetFullPath(worktreeA));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
            if (Directory.Exists(outside))
                Directory.Delete(outside, recursive: true);
        }
    }

    [Fact]
    public async Task ParseResourcesFromDescribeJsonAsync_WithMultipleWorktrees_AggregatesAndPrefixesResources()
    {
        var root = Path.Combine(Path.GetTempPath(), $"AspireCliService_Parse_{Guid.NewGuid():N}");
        var worktreeA = Path.Combine(root, "wt-a");
        var worktreeB = Path.Combine(root, "wt-b");

        try
        {
            Directory.CreateDirectory(worktreeA);
            Directory.CreateDirectory(worktreeB);
            File.WriteAllText(Path.Combine(worktreeA, "aspire.config.json"), """{ "appHost": { "path": "AppHost.cs" } }""");
            File.WriteAllText(Path.Combine(worktreeA, "AppHost.cs"), "// apphost");
            File.WriteAllText(Path.Combine(worktreeB, "aspire.config.json"), """{ "appHost": { "path": "AppHost.cs" } }""");
            File.WriteAllText(Path.Combine(worktreeB, "AppHost.cs"), "// apphost");

            var gitOutput = $"""
            worktree {worktreeA}
            worktree {worktreeB}
            """;

            var responses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [Path.GetFullPath(worktreeA)] = """{ "resources": [ { "name": "api", "state": "running" } ] }""",
                [Path.GetFullPath(worktreeB)] = """{ "resources": [ { "name": "worker", "state": "running" } ] }"""
            };

            var service = new WorktreeTestAspireCliService(gitOutput, responses)
            {
                EnableWorktreeDiscovery = true,
                WorktreeBasePath = root
            };

            var result = await service.ParseResourcesFromDescribeJsonAsync();

            result.ErrorMessage.Should().BeNull();
            result.Resources.Select(resource => resource.Name)
                .Should().BeEquivalentTo("wt-a/api", "wt-b/worker");
            result.Resources.All(resource => resource.Environment.Any(entry =>
                entry.Name == "ASPIREMON_WORKTREE" && !string.IsNullOrWhiteSpace(entry.Value))).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ParseResourcesFromDescribeJsonAsync_MapsCurrentAndLegacyEndpointShapes()
    {
        var json = """
        {
          "resources": [
            {
              "name": "api",
              "type": "project",
              "state": "running",
              "urls": [ "http://localhost:5000", { "url": "https://localhost:5001" } ],
              "cpu": 12.5,
              "memory": 256
            },
            {
              "name": "worker",
              "resourceType": "container",
              "state": "finishing",
              "endpoints": [
                "http://localhost:6000",
                { "url": "http://localhost:6001" },
                { "endpointUrl": "http://localhost:6002", "proxyUrl": "http://localhost:7002" }
              ]
            },
            { "type": "ignored" }
          ]
        }
        """;

        var service = new JsonAspireCliService(json);

        var result = await service.ParseResourcesFromDescribeJsonAsync();

        result.ErrorMessage.Should().BeNull();
        result.Resources.Should().HaveCount(2);
        result.Resources[0].Name.Should().Be("api");
        result.Resources[0].Id.Should().Be("api");
        result.Resources[0].Type.Should().Be("project");
        result.Resources[0].Status.Should().Be(ResourceStatus.Running);
        result.Resources[0].Metrics.CpuUsagePercent.Should().Be(12.5);
        result.Resources[0].Metrics.MemoryUsage.Should().Be(256);
        result.Resources[0].Endpoints.Should().HaveCount(2);
        result.Resources[1].Type.Should().Be("container");
        result.Resources[1].Status.Should().Be(ResourceStatus.Stopping);
        result.Resources[1].Endpoints.Should().HaveCount(3);
        result.Resources[1].Endpoints[2].ProxyUrl.Should().Be("http://localhost:7002");
    }

    [Fact]
    public async Task ParseResourcesFromDescribeJsonAsync_MapsTelemetryRichResourceShape()
    {
        var json = """
        {
          "resources": [
            {
              "name": "api-service",
              "resourceType": "Container",
              "state": "Running",
              "endpoints": [
                { "endpointUrl": "http://localhost:5000" },
                { "endpointUrl": "https://localhost:5001" }
              ],
              "properties": {
                "cpuUsage": 45.5,
                "memoryUsage": 512000000,
                "memoryLimit": 2147483648,
                "diskUsage": 12.3
              },
              "environment": [
                { "name": "ASPNETCORE_ENVIRONMENT", "value": "Development" }
              ]
            }
          ]
        }
        """;

        var service = new JsonAspireCliService(json);

        var result = await service.ParseResourcesFromDescribeJsonAsync();

        var resource = result.Resources.Should().ContainSingle().Subject;
        resource.Type.Should().Be("Container");
        resource.Status.Should().Be(ResourceStatus.Running);
        resource.Metrics.CpuUsagePercent.Should().Be(45.5);
        resource.Metrics.MemoryUsage.Should().Be(512000000);
        resource.Metrics.MemoryLimit.Should().Be(2147483648);
        resource.Metrics.MemoryUsagePercent.Should().BeApproximately(23.8418579, 0.0001);
        resource.Metrics.DiskUsagePercent.Should().Be(12.3);
        resource.Endpoints.Should().HaveCount(2);
        resource.Environment.Should().ContainSingle()
            .Which.IsDevelopmentEnvironment.Should().BeTrue();
    }

    [Fact]
    public async Task ParseResourcesFromDescribeJsonAsync_MapsLegacyMetricsAndEnvironmentObject()
    {
        var json = """
        {
          "resources": [
            {
              "id": "apiservice",
              "name": "apiservice",
              "type": "project.v0",
              "status": "Running",
              "metrics": {
                "cpuUsagePercent": 25.5,
                "memoryUsagePercent": 40.2,
                "diskUsagePercent": 15.0
              },
              "environment": {
                "DOTNET_ENVIRONMENT": "Production"
              }
            }
          ]
        }
        """;

        var service = new JsonAspireCliService(json);

        var result = await service.ParseResourcesFromDescribeJsonAsync();

        var resource = result.Resources.Should().ContainSingle().Subject;
        resource.Status.Should().Be(ResourceStatus.Running);
        resource.Metrics.CpuUsagePercent.Should().Be(25.5);
        resource.Metrics.MemoryUsagePercent.Should().Be(40.2);
        resource.Metrics.DiskUsagePercent.Should().Be(15.0);
        resource.Environment.Should().ContainSingle(entry =>
            entry.Name == "DOTNET_ENVIRONMENT" && entry.Value == "Production");
    }

    [Theory]
    [InlineData("starting", ResourceStatus.Starting)]
    [InlineData("stopped", ResourceStatus.Stopped)]
    [InlineData("failed", ResourceStatus.Failed)]
    [InlineData("unexpected", ResourceStatus.Unknown)]
    public async Task ParseResourcesFromDescribeJsonAsync_MapsStates(string state, ResourceStatus expected)
    {
        var service = new JsonAspireCliService($$"""
        { "resources": [ { "name": "resource", "state": "{{state}}" } ] }
        """);

        var result = await service.ParseResourcesFromDescribeJsonAsync();

        result.Resources.Single().Status.Should().Be(expected);
    }

    [Fact]
    public async Task ParseResourcesFromDescribeJsonAsync_NoJson_ReturnsNoOutputError()
    {
        var service = new JsonAspireCliService(null);

        var result = await service.ParseResourcesFromDescribeJsonAsync();

        result.ErrorMessage.Should().Be("No output from 'aspire describe'");
        result.Resources.Should().BeEmpty();
    }

    [Theory]
    [InlineData("not found", "Aspire CLI not found. Please install Aspire.")]
    [InlineData("cannot be executed", "Aspire CLI not found. Please install Aspire.")]
    [InlineData("Command failed: no app", "No Aspire app is currently running.")]
    public async Task ParseResourcesFromDescribeJsonAsync_KnownInvalidOperationErrors_ReturnFriendlyMessages(
        string exceptionMessage,
        string expectedMessage)
    {
        var service = new ThrowingAspireCliService(new InvalidOperationException(exceptionMessage));

        var result = await service.ParseResourcesFromDescribeJsonAsync();

        result.ErrorMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public async Task ParseResourcesFromDescribeJsonAsync_Timeout_ReturnsTimeoutError()
    {
        var service = new ThrowingAspireCliService(new TimeoutException("slow"));

        var result = await service.ParseResourcesFromDescribeJsonAsync();

        result.ErrorMessage.Should().Be("Command timed out. Aspire might be unresponsive.");
    }

    [Fact]
    public async Task ParseResourcesFromDescribeJsonAsync_UnexpectedError_ReturnsErrorMessage()
    {
        var service = new ThrowingAspireCliService(new JsonException("bad json"));

        var result = await service.ParseResourcesFromDescribeJsonAsync();

        result.ErrorMessage.Should().Be("Error: bad json");
    }
}

public class AspirePollingServiceCoverageTests
{
    [Fact]
    public async Task RefreshAsync_WithResources_PublishesResourcesAndConnectedStatus()
    {
        var cli = new QueueAspireCliService(new ResourceCollection(new List<AspireResource>
        {
            new() { Name = "api", Status = ResourceStatus.Running }
        }));
        using var service = new AspirePollingService(cli, 60_000);
        var statuses = new List<string>();
        var updates = new List<List<AspireResource>>();
        service.StatusChanged += (_, status) => statuses.Add(status);
        service.ResourcesUpdated += (_, resources) => updates.Add(resources);

        service.Start();
        await service.RefreshAsync();

        service.State.Should().Be(PollingServiceState.Polling);
        statuses.Should().ContainInOrder("Connecting", "Connected");
        updates.Should().ContainSingle().Which.Single().Name.Should().Be("api");
    }

    [Fact]
    public async Task RefreshAsync_WhenAspireStopped_ClearsResourcesAndReportsNotRunning()
    {
        var cli = new QueueAspireCliService(
            new ResourceCollection(new List<AspireResource> { new() { Name = "api", Status = ResourceStatus.Running } }),
            new ResourceCollection { ErrorMessage = "No Aspire app is currently running." });
        using var service = new AspirePollingService(cli, 60_000);
        var statuses = new List<string>();
        var updates = new List<List<AspireResource>>();
        service.StatusChanged += (_, status) => statuses.Add(status);
        service.ResourcesUpdated += (_, resources) => updates.Add(resources);

        service.Start();
        await service.RefreshAsync();
        await service.RefreshAsync();

        service.State.Should().Be(PollingServiceState.Connecting);
        statuses.Should().Contain("Not Running");
        updates.Last().Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAsync_WithTransientError_RaisesErrorAndTransitionsToError()
    {
        var cli = new QueueAspireCliService(new ResourceCollection { ErrorMessage = "Command timed out. Aspire might be unresponsive." });
        using var service = new AspirePollingService(cli, 60_000);
        var errors = new List<string>();
        service.ErrorOccurred += (_, error) => errors.Add(error);

        service.Start();
        await service.RefreshAsync();

        service.State.Should().Be(PollingServiceState.Error);
        errors.Should().ContainSingle().Which.Should().Contain("timed out");
        service.Stop();
        service.State.Should().Be(PollingServiceState.Idle);
    }

    [Fact]
    public async Task RefreshAsync_WithEmptyFirstResponse_PublishesEmptySnapshotForInitialConnection()
    {
        var cli = new QueueAspireCliService(new ResourceCollection(new List<AspireResource>()));
        using var service = new AspirePollingService(cli, 60_000);
        var updates = new List<List<AspireResource>>();
        service.ResourcesUpdated += (_, resources) => updates.Add(resources);

        service.Start();
        await service.RefreshAsync();

        service.State.Should().Be(PollingServiceState.Polling);
        updates.Should().ContainSingle().Which.Should().BeEmpty();
    }

    [Fact]
    public async Task RefreshAsync_WhenCliThrows_RaisesPollingError()
    {
        var cli = new ThrowingPollingCliService(new InvalidOperationException("boom"));
        using var service = new AspirePollingService(cli, 60_000);
        var errors = new List<string>();
        service.ErrorOccurred += (_, error) => errors.Add(error);

        service.Start();
        await service.RefreshAsync();

        service.State.Should().Be(PollingServiceState.Error);
        errors.Should().ContainSingle().Which.Should().Be("Polling error: boom");
    }

    [Fact]
    public async Task RefreshAsync_RaisesAspireRunningStateChangedOnlyAfterEstablishedStateChanges()
    {
        var cli = new QueueAspireCliService(
            new ResourceCollection(new List<AspireResource> { new() { Name = "api", Status = ResourceStatus.Running } }),
            new ResourceCollection { ErrorMessage = "No Aspire app is currently running." },
            new ResourceCollection { ErrorMessage = "No Aspire app is currently running." },
            new ResourceCollection(new List<AspireResource> { new() { Name = "api", Status = ResourceStatus.Running } }));
        using var service = new AspirePollingService(cli, 60_000);
        var stateChanges = new List<bool>();
        service.AspireRunningStateChanged += (_, args) => stateChanges.Add(args.IsRunning);

        service.Start();
        await service.RefreshAsync();
        stateChanges.Should().BeEmpty("the first poll establishes the baseline state");

        await service.RefreshAsync();
        await service.RefreshAsync();
        await service.RefreshAsync();

        stateChanges.Should().Equal(false, true);
    }
}


public class AspireLiveLogsServiceTests
{
    [Fact]
    public async Task StartStreamingAsync_BuffersLinesPublishesEventsAndClosesStream()
    {
        var cli = new StreamingAspireCliService("line 1", "line 2", "line 3");
        using var service = new AspireLiveLogsService(cli);
        var lines = new List<string>();
        var closed = new List<LogStreamClosedEventArgs>();
        service.LogLineReceived += (_, args) => lines.Add($"{args.ResourceName}:{args.LogLine}");
        service.LogStreamClosed += (_, args) => closed.Add(args);

        await service.StartStreamingAsync("api", bufferSize: 2);
        await WaitUntilAsync(() => closed.Count == 1);

        lines.Should().Equal("api:line 1", "api:line 2", "api:line 3");
        service.IsStreaming("api").Should().BeFalse();
        service.GetBufferedLogs("api").Should().BeNull();
        closed.Single().IsError.Should().BeFalse();
    }

    [Fact]
    public async Task StartStreamingAsync_WhenCliThrows_PublishesErrorAndClosedEvent()
    {
        var cli = new ThrowingLogsAspireCliService(new InvalidOperationException("log failure"));
        using var service = new AspireLiveLogsService(cli);
        var errors = new List<string>();
        var closed = new List<LogStreamClosedEventArgs>();
        service.ErrorOccurred += (_, error) => errors.Add(error);
        service.LogStreamClosed += (_, args) => closed.Add(args);

        await service.StartStreamingAsync("api");
        await WaitUntilAsync(() => closed.Count == 1);

        errors.Single().Should().Contain("log failure");
        closed.Single().IsError.Should().BeTrue();
        closed.Single().ErrorMessage.Should().Be("log failure");
    }

    [Fact]
    public async Task StopStreaming_RemovesActiveStream()
    {
        var cli = new StreamingAspireCliService("line 1", "line 2") { DelayBetweenLines = TimeSpan.FromMilliseconds(200) };
        using var service = new AspireLiveLogsService(cli);
        var lines = new List<string>();
        service.LogLineReceived += (_, args) => lines.Add(args.LogLine);

        await service.StartStreamingAsync("api");
        await WaitUntilAsync(() => service.IsStreaming("api"));
        await Task.Delay(250);
        service.StopStreaming("api");

        service.IsStreaming("api").Should().BeFalse();
        var lineCountAfterStop = lines.Count;
        await Task.Delay(350);
        lines.Count.Should().Be(lineCountAfterStop, "stream cancellation should stop additional log lines");
    }

    [Fact]
    public void EventArgs_ExposeConstructorValues()
    {
        var line = new LogLineReceivedEventArgs("api", "hello");
        var closed = new LogStreamClosedEventArgs("api", true, "error");

        line.ResourceName.Should().Be("api");
        line.LogLine.Should().Be("hello");
        line.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        closed.ResourceName.Should().Be("api");
        closed.IsError.Should().BeTrue();
        closed.ErrorMessage.Should().Be("error");
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

internal sealed class JsonAspireCliService : AspireCliService
{
    private readonly string? _json;

    public JsonAspireCliService(string? json)
    {
        _json = json;
    }

    public override Task<JsonDocument?> ExecuteJsonAsync(string command, string arguments = "", CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_json is null ? null : JsonDocument.Parse(_json));
    }
}

internal sealed class WorktreeTestAspireCliService : AspireCliService
{
    private readonly string _worktreeListOutput;
    private readonly Dictionary<string, string> _jsonByWorkingDirectory;

    public WorktreeTestAspireCliService(string worktreeListOutput, Dictionary<string, string> jsonByWorkingDirectory)
    {
        _worktreeListOutput = worktreeListOutput;
        _jsonByWorkingDirectory = jsonByWorkingDirectory;
    }

    protected override Task<string> GetGitWorktreeListAsync(string rootPath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_worktreeListOutput);
    }

    protected override Task<JsonDocument?> ExecuteDescribeJsonForDirectoryAsync(string workingDirectory, CancellationToken cancellationToken = default)
    {
        var key = Path.GetFullPath(workingDirectory);
        if (_jsonByWorkingDirectory.TryGetValue(key, out var json))
            return Task.FromResult<JsonDocument?>(JsonDocument.Parse(json));

        return Task.FromResult<JsonDocument?>(null);
    }

    public override Task<JsonDocument?> ExecuteJsonAsync(string command, string arguments = "", CancellationToken cancellationToken = default)
    {
        return Task.FromResult<JsonDocument?>(null);
    }
}

internal sealed class ThrowingAspireCliService : AspireCliService
{
    private readonly Exception _exception;

    public ThrowingAspireCliService(Exception exception)
    {
        _exception = exception;
    }

    public override Task<JsonDocument?> ExecuteJsonAsync(string command, string arguments = "", CancellationToken cancellationToken = default)
    {
        throw _exception;
    }
}

internal sealed class QueueAspireCliService : AspireCliService
{
    private readonly Queue<ResourceCollection> _responses;

    public QueueAspireCliService(params ResourceCollection[] responses)
    {
        _responses = new Queue<ResourceCollection>(responses);
    }

    public override Task<ResourceCollection> ParseResourcesFromDescribeJsonAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_responses.Count > 0 ? _responses.Dequeue() : new ResourceCollection(new List<AspireResource>()));
    }
}

internal sealed class ThrowingPollingCliService : AspireCliService
{
    private readonly Exception _exception;

    public ThrowingPollingCliService(Exception exception)
    {
        _exception = exception;
    }

    public override Task<ResourceCollection> ParseResourcesFromDescribeJsonAsync(CancellationToken cancellationToken = default)
    {
        throw _exception;
    }
}

internal class StreamingAspireCliService : AspireCliService
{
    private readonly string[] _lines;

    public TimeSpan DelayBetweenLines { get; set; } = TimeSpan.Zero;

    public StreamingAspireCliService(params string[] lines)
    {
        _lines = lines;
    }

    public override async IAsyncEnumerable<string> GetLiveLogsAsync(string resourceName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var line in _lines)
        {
            if (DelayBetweenLines > TimeSpan.Zero)
            {
                await Task.Delay(DelayBetweenLines, cancellationToken);
            }

            yield return line;
        }
    }
}

internal sealed class ThrowingLogsAspireCliService : StreamingAspireCliService
{
    private readonly Exception _exception;

    public ThrowingLogsAspireCliService(Exception exception)
    {
        _exception = exception;
    }

    public override async IAsyncEnumerable<string> GetLiveLogsAsync(string resourceName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        throw _exception;
#pragma warning disable CS0162
        yield break;
#pragma warning restore CS0162
    }
}
public class AspireCliServiceCommandExecutionTests
{
    [Fact]
    public async Task ExecuteCommandAsync_WithDotnetVersion_ReturnsOutput()
    {
        var service = new AspireCliService();

        var output = await service.ExecuteCommandAsync("dotnet", "--version");

        output.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ExecuteCommandAsync_WithMissingCommand_ThrowsFriendlyException()
    {
        var service = new AspireCliService();

        Func<Task> act = () => service.ExecuteCommandAsync("definitely-not-a-real-command-elbruno", "");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found or cannot be executed*");
    }

    [Fact]
    public async Task ExecuteJsonAsync_WithNonJsonOutput_ThrowsParseException()
    {
        var service = new AspireCliService();

        Func<Task> act = () => service.ExecuteJsonAsync("dotnet", "--version");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to parse JSON output from command:*");
    }
}

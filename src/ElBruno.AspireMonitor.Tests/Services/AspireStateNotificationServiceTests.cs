using System.Reflection;
using ElBruno.AspireMonitor.Services;
using FluentAssertions;
using Moq;
using Xunit;
using AppConfig = ElBruno.AspireMonitor.Models.Configuration;

namespace ElBruno.AspireMonitor.Tests.Services;

public class AspireStateNotificationBehaviorTests
{
    [Fact]
    public void RunningStateChanged_FirstObservedState_DoesNotRaiseNotificationEvent()
    {
        var events = RecordRunningStateEvents();

        events.RaiseState(isRunning: false);

        events.States.Should().BeEmpty("the first observed Aspire state is the baseline, not a user-visible change");
    }

    [Fact]
    public void RunningStateChanged_NotRunningToRunning_RaisesRunningEvent()
    {
        var events = RecordRunningStateEvents();

        events.RaiseState(isRunning: false);
        events.RaiseState(isRunning: true);

        events.States.Should().Equal(true);
    }

    [Fact]
    public void RunningStateChanged_RunningToNotRunning_RaisesNotRunningEvent()
    {
        var events = RecordRunningStateEvents();

        events.RaiseState(isRunning: true);
        events.RaiseState(isRunning: false);

        events.States.Should().Equal(false);
    }

    [Fact]
    public void RunningStateChanged_RepeatedSameState_DoesNotDuplicateEvents()
    {
        var events = RecordRunningStateEvents();

        events.RaiseState(isRunning: false);
        events.RaiseState(isRunning: true);
        events.RaiseState(isRunning: true);
        events.RaiseState(isRunning: true);

        events.States.Should().Equal(true);
    }

    [Fact]
    public void NotifyAspireStateChanged_WhenEnabled_ForwardsToNotificationSink()
    {
        var sink = new RecordingNotificationSink();
        var service = CreateService(sink);

        service.NotifyAspireStateChanged(isRunning: true);

        sink.States.Should().Equal(true);
    }

    [Fact]
    public void NotifyAspireStateChanged_WhenDisabled_DoesNotNotify()
    {
        var sink = new RecordingNotificationSink();
        var service = CreateService(sink, enableNotifications: false);

        service.NotifyAspireStateChanged(isRunning: true);

        sink.States.Should().BeEmpty("disabled settings should suppress Windows notifications");
    }

    [Fact]
    public void NotifyAspireStateChanged_WithDocumentedNotifyOnStateChangeFalseInJson_DoesNotNotify()
    {
        var root = Path.Combine(AppContext.BaseDirectory, "AspireStateNotificationBehavior", Guid.NewGuid().ToString("N"));
        var configPath = Path.Combine(root, "config.json");

        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(configPath, """
            {
              "notifyOnStateChange": false
            }
            """);
            var sink = new RecordingNotificationSink();
            var service = new AspireStateNotificationService(new ConfigurationService(configPath), sink);

            service.NotifyAspireStateChanged(isRunning: true);

            sink.States.Should().BeEmpty("the documented JSON setting should disable Windows notifications");
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    private static AspireStateNotificationService CreateService(
        RecordingNotificationSink sink,
        bool enableNotifications = true)
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new AppConfig { EnableAspireStateNotifications = enableNotifications });

        return new AspireStateNotificationService(configService.Object, sink);
    }

    private static RunningStateEventRecorder RecordRunningStateEvents()
    {
        var pollingService = new AspirePollingService(new AspireCliService());
        return new RunningStateEventRecorder(pollingService);
    }

    private sealed class RunningStateEventRecorder
    {
        private static readonly MethodInfo UpdateStateMethod = typeof(AspirePollingService).GetMethod(
            "UpdateAspireRunningState",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        private readonly AspirePollingService _pollingService;

        public RunningStateEventRecorder(AspirePollingService pollingService)
        {
            _pollingService = pollingService;
            _pollingService.AspireRunningStateChanged += (_, args) => States.Add(args.IsRunning);
        }

        public List<bool> States { get; } = new();

        public void RaiseState(bool isRunning)
        {
            UpdateStateMethod.Invoke(_pollingService, new object[] { isRunning });
        }
    }

    private sealed class RecordingNotificationSink : IAspireStateNotificationSink
    {
        public List<bool> States { get; } = new();

        public void ShowAspireStateChanged(bool isRunning)
        {
            States.Add(isRunning);
        }
    }
}

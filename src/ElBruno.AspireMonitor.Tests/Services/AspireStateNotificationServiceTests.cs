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
        sink.DashboardUrls.Should().Equal(AppConfig.DefaultAspireEndpoint);
    }

    [Fact]
    public void NotifyAspireStateChanged_WhenDisabled_DoesNotNotify()
    {
        var sink = new RecordingNotificationSink();
        var service = CreateService(sink, enableNotifications: false);

        service.NotifyAspireStateChanged(isRunning: true);

        sink.States.Should().BeEmpty("disabled settings should suppress Windows notifications");
        sink.DashboardUrls.Should().BeEmpty("disabled settings should suppress clickable dashboard targets");
    }

    [Fact]
    public void NotifyAspireStateChanged_WhenRunning_ForwardsConfiguredDashboardUrl()
    {
        var sink = new RecordingNotificationSink();
        var service = CreateService(sink, aspireEndpoint: "http://localhost:19999/login?t=test");

        service.NotifyAspireStateChanged(isRunning: true);

        sink.States.Should().Equal(true);
        sink.DashboardUrls.Should().Equal("http://localhost:19999/login?t=test");
    }

    [Fact]
    public void NotifyAspireStateChanged_WhenStopped_DoesNotForwardDashboardUrl()
    {
        var sink = new RecordingNotificationSink();
        var service = CreateService(sink, aspireEndpoint: "http://localhost:19999/login?t=test");

        service.NotifyAspireStateChanged(isRunning: false);

        sink.States.Should().Equal(false);
        sink.DashboardUrls.Should().Equal((string?)null);
    }

    [Fact]
    public void NotifyAspireStateChanged_WhenDashboardUrlBlank_UsesDefaultDashboardUrl()
    {
        var sink = new RecordingNotificationSink();
        var service = CreateService(sink, aspireEndpoint: " ");

        service.NotifyAspireStateChanged(isRunning: true);

        sink.DashboardUrls.Should().Equal(AppConfig.DefaultAspireEndpoint);
    }

    [Fact]
    public void NotifyAspireStateChanged_WhenHostUrlProviderHasValue_PrefersHostUrl()
    {
        var sink = new RecordingNotificationSink();
        var service = CreateService(
            sink,
            aspireEndpoint: "http://localhost:18888",
            dashboardUrlProvider: () => "http://localhost:19999/login?t=detected");

        service.NotifyAspireStateChanged(isRunning: true);

        sink.DashboardUrls.Should().Equal("http://localhost:19999/login?t=detected");
    }

    [Fact]
    public void NotifyIconAspireStateNotificationSink_BalloonClick_OpensRunningDashboardUrl()
    {
        using var notifyIcon = new System.Windows.Forms.NotifyIcon();
        var urlLauncher = new RecordingUrlLauncher();
        var sink = new NotifyIconAspireStateNotificationSink(() => notifyIcon, urlLauncher);

        sink.ShowAspireStateChanged(isRunning: true, "http://localhost:19999/login?t=test");
        RaiseBalloonTipClicked(sink);

        urlLauncher.Urls.Should().Equal("http://localhost:19999/login?t=test");
    }

    [Fact]
    public void NotifyIconAspireStateNotificationSink_BalloonClick_AfterStoppedNotification_DoesNotOpenDashboard()
    {
        using var notifyIcon = new System.Windows.Forms.NotifyIcon();
        var urlLauncher = new RecordingUrlLauncher();
        var sink = new NotifyIconAspireStateNotificationSink(() => notifyIcon, urlLauncher);

        sink.ShowAspireStateChanged(isRunning: true, "http://localhost:19999/login?t=test");
        sink.ShowAspireStateChanged(isRunning: false, null);
        RaiseBalloonTipClicked(sink);

        urlLauncher.Urls.Should().BeEmpty();
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
        bool enableNotifications = true,
        string? aspireEndpoint = null,
        Func<string?>? dashboardUrlProvider = null)
    {
        var configService = new Mock<IConfigurationService>();
        configService.Setup(service => service.LoadConfiguration())
            .Returns(new AppConfig
            {
                EnableAspireStateNotifications = enableNotifications,
                AspireEndpoint = aspireEndpoint ?? AppConfig.DefaultAspireEndpoint
            });

        return new AspireStateNotificationService(configService.Object, sink, dashboardUrlProvider);
    }

    private static void RaiseBalloonTipClicked(NotifyIconAspireStateNotificationSink sink)
    {
        var method = typeof(NotifyIconAspireStateNotificationSink).GetMethod(
            "OpenDashboardFromNotification",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        method.Invoke(sink, new object?[] { null, EventArgs.Empty });
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
        public List<string?> DashboardUrls { get; } = new();

        public void ShowAspireStateChanged(bool isRunning, string? dashboardUrl)
        {
            States.Add(isRunning);
            DashboardUrls.Add(dashboardUrl);
        }
    }

    private sealed class RecordingUrlLauncher : IUrlLauncher
    {
        public List<string> Urls { get; } = new();

        public void OpenUrl(string url)
        {
            Urls.Add(url);
        }
    }
}

using System.Windows.Forms;
using ElBruno.AspireMonitor.Models;

namespace ElBruno.AspireMonitor.Services;

public interface IAspireStateNotificationSink
{
    void ShowAspireStateChanged(bool isRunning, string? dashboardUrl);
}

public interface IUrlLauncher
{
    void OpenUrl(string url);
}

public sealed class AspireStateNotificationService : IAspireStateNotificationService
{
    private readonly IConfigurationService _configurationService;
    private readonly IAspireStateNotificationSink _notificationSink;
    private readonly Func<string?>? _dashboardUrlProvider;

    public AspireStateNotificationService(
        IConfigurationService configurationService,
        IAspireStateNotificationSink notificationSink,
        Func<string?>? dashboardUrlProvider = null)
    {
        _configurationService = configurationService;
        _notificationSink = notificationSink;
        _dashboardUrlProvider = dashboardUrlProvider;
    }

    public void NotifyAspireStateChanged(bool isRunning)
    {
        if (!AreNotificationsEnabled())
        {
            return;
        }

        _notificationSink.ShowAspireStateChanged(isRunning, isRunning ? GetDashboardUrl() : null);
    }

    private bool AreNotificationsEnabled()
    {
        try
        {
            return _configurationService.LoadConfiguration().EnableAspireStateNotifications;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireStateNotificationService] Failed to read notification setting: {ex.Message}");
            return new Configuration().EnableAspireStateNotifications;
        }
    }

    private string GetDashboardUrl()
    {
        var providedUrl = _dashboardUrlProvider?.Invoke();
        if (!string.IsNullOrWhiteSpace(providedUrl))
        {
            return providedUrl;
        }

        try
        {
            var endpoint = _configurationService.LoadConfiguration().AspireEndpoint;
            return string.IsNullOrWhiteSpace(endpoint)
                ? Configuration.DefaultAspireEndpoint
                : endpoint;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireStateNotificationService] Failed to read dashboard URL: {ex.Message}");
            return Configuration.DefaultAspireEndpoint;
        }
    }
}

public sealed class NotifyIconAspireStateNotificationSink : IAspireStateNotificationSink
{
    private readonly Func<NotifyIcon?> _notifyIconProvider;
    private readonly IUrlLauncher _urlLauncher;
    private NotifyIcon? _attachedNotifyIcon;
    private string? _dashboardUrlToOpen;

    public NotifyIconAspireStateNotificationSink(Func<NotifyIcon?> notifyIconProvider)
        : this(notifyIconProvider, new SystemUrlLauncher())
    {
    }

    public NotifyIconAspireStateNotificationSink(Func<NotifyIcon?> notifyIconProvider, IUrlLauncher urlLauncher)
    {
        _notifyIconProvider = notifyIconProvider;
        _urlLauncher = urlLauncher;
    }

    public void ShowAspireStateChanged(bool isRunning, string? dashboardUrl)
    {
        var notifyIcon = _notifyIconProvider();
        if (notifyIcon == null)
        {
            return;
        }

        AttachClickHandler(notifyIcon);
        _dashboardUrlToOpen = isRunning && !string.IsNullOrWhiteSpace(dashboardUrl) ? dashboardUrl : null;

        var message = isRunning
            ? $"Aspire started and is running.{Environment.NewLine}Dashboard: {dashboardUrl}"
            : "Aspire stopped or is not running.";
        var icon = isRunning ? ToolTipIcon.Info : ToolTipIcon.Warning;

        try
        {
            notifyIcon.ShowBalloonTip(4000, "Aspire Monitor", message, icon);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NotifyIconAspireStateNotificationSink] Failed to show Aspire state notification: {ex.Message}");
        }
    }

    private void AttachClickHandler(NotifyIcon notifyIcon)
    {
        if (ReferenceEquals(_attachedNotifyIcon, notifyIcon))
        {
            return;
        }

        if (_attachedNotifyIcon != null)
        {
            _attachedNotifyIcon.BalloonTipClicked -= OpenDashboardFromNotification;
        }

        _attachedNotifyIcon = notifyIcon;
        _attachedNotifyIcon.BalloonTipClicked += OpenDashboardFromNotification;
    }

    private void OpenDashboardFromNotification(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_dashboardUrlToOpen))
        {
            return;
        }

        try
        {
            _urlLauncher.OpenUrl(_dashboardUrlToOpen);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NotifyIconAspireStateNotificationSink] Failed to open Aspire dashboard URL: {ex.Message}");
        }
    }
}

public sealed class SystemUrlLauncher : IUrlLauncher
{
    public void OpenUrl(string url)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}

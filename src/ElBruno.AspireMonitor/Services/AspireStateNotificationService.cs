using System.Windows.Forms;
using ElBruno.AspireMonitor.Models;

namespace ElBruno.AspireMonitor.Services;

public interface IAspireStateNotificationSink
{
    void ShowAspireStateChanged(bool isRunning);
}

public sealed class AspireStateNotificationService : IAspireStateNotificationService
{
    private readonly IConfigurationService _configurationService;
    private readonly IAspireStateNotificationSink _notificationSink;

    public AspireStateNotificationService(
        IConfigurationService configurationService,
        IAspireStateNotificationSink notificationSink)
    {
        _configurationService = configurationService;
        _notificationSink = notificationSink;
    }

    public void NotifyAspireStateChanged(bool isRunning)
    {
        if (!AreNotificationsEnabled())
        {
            return;
        }

        _notificationSink.ShowAspireStateChanged(isRunning);
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
}

public sealed class NotifyIconAspireStateNotificationSink : IAspireStateNotificationSink
{
    private readonly Func<NotifyIcon?> _notifyIconProvider;

    public NotifyIconAspireStateNotificationSink(Func<NotifyIcon?> notifyIconProvider)
    {
        _notifyIconProvider = notifyIconProvider;
    }

    public void ShowAspireStateChanged(bool isRunning)
    {
        var notifyIcon = _notifyIconProvider();
        if (notifyIcon == null)
        {
            return;
        }

        var message = isRunning
            ? "Aspire started and is running."
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
}

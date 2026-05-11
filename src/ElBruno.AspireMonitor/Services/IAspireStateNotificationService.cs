namespace ElBruno.AspireMonitor.Services;

public interface IAspireStateNotificationService
{
    void NotifyAspireStateChanged(bool isRunning);
}

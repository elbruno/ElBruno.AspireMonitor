namespace ElBruno.AspireMonitor.Services;

public class AspireRunningStateChangedEventArgs : EventArgs
{
    public AspireRunningStateChangedEventArgs(bool isRunning)
    {
        IsRunning = isRunning;
    }

    public bool IsRunning { get; }
}

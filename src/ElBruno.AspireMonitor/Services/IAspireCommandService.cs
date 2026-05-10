namespace ElBruno.AspireMonitor.Services;

public interface IAspireCommandService
{
    Task<bool> StartAspireAsync(string workingFolder, Action<string>? logCallback = null);
    Task<bool> StopAspireAsync(Action<string>? logCallback = null);
    Task<string> GetStatusAsync();
    Task<string?> DetectAspireEndpointAsync(Action<string>? logCallback = null);
    Task<string> GetRunningInstancesAsync(Action<string>? logCallback = null);
    Task<string> DescribeResourcesAsync(Action<string>? logCallback = null);
}

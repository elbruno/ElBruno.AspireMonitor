namespace ElBruno.AspireMonitor.Services;

public interface IAspireCommandService
{
    Task<bool> StartAspireAsync(string workingFolder);
    Task<bool> StopAspireAsync();
    Task<string> GetStatusAsync();
    Task<string?> DetectAspireEndpointAsync();
}

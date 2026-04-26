using ElBruno.AspireMonitor.Models;

namespace ElBruno.AspireMonitor.Services;

public interface IAspirePollingService
{
    event EventHandler<List<AspireResource>>? ResourcesUpdated;
    event EventHandler<string>? StatusChanged;
    event EventHandler<string>? ErrorOccurred;
    
    void Start();
    void Stop();
    Task RefreshAsync();
    void UpdateEndpoint(string newEndpoint);
}

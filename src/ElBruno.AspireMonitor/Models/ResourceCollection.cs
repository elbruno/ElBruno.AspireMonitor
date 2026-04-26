namespace ElBruno.AspireMonitor.Models;

public class ResourceCollection
{
    public List<AspireResource> Resources { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsHealthy { get; set; } = true;
    public string? ErrorMessage { get; set; }
    
    public ResourceCollection()
    {
    }
    
    public ResourceCollection(List<AspireResource> resources)
    {
        Resources = resources;
        LastUpdated = DateTime.UtcNow;
        IsHealthy = CalculateHealth();
    }
    
    private bool CalculateHealth()
    {
        if (Resources.Count == 0)
            return false;
            
        return Resources.All(r => 
            r.Status != ResourceStatus.Failed && 
            r.Status != ResourceStatus.Unknown);
    }
    
    public void UpdateHealth()
    {
        IsHealthy = CalculateHealth();
        LastUpdated = DateTime.UtcNow;
    }
}

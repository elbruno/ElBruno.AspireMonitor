namespace ElBruno.AspireMonitor.Models;

public class AspireInstance
{
    public int ProcessId { get; set; }
    public int Port { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public bool IsRunning => Status.Equals("Running", StringComparison.OrdinalIgnoreCase);
    
    public AspireInstance()
    {
    }
    
    public AspireInstance(int processId, int port, string status)
    {
        ProcessId = processId;
        Port = port;
        Status = status;
    }
}

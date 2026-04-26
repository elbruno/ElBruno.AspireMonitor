namespace ElBruno.AspireMonitor.Models;

public class HealthStatus
{
    public StatusColor Color { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;

    public HealthStatus()
    {
        Color = StatusColor.Unknown;
        Timestamp = DateTime.Now;
    }

    public HealthStatus(StatusColor color, string message)
    {
        Color = color;
        Message = message;
        Timestamp = DateTime.Now;
    }
}

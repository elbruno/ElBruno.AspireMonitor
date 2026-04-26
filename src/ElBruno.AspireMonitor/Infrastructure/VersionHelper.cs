using System.Reflection;

namespace ElBruno.AspireMonitor.Infrastructure;

public static class VersionHelper
{
    public static string GetAppVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        
        if (version != null)
        {
            return $"v{version.Major}.{version.Minor}.{version.Build}";
        }
        
        // Fallback to informational version attribute
        var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (infoVersion != null)
        {
            return $"v{infoVersion.InformationalVersion}";
        }
        
        return "v1.0.0";
    }
}

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
            return NormalizeVersion($"{version.Major}.{version.Minor}.{version.Build}");
        }
        
        // Fallback to informational version attribute
        var infoVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (infoVersion != null)
        {
            return NormalizeVersion(infoVersion.InformationalVersion);
        }
        
        return "1.0.0";
    }

    private static string NormalizeVersion(string version)
    {
        var normalized = version.Trim();

        if (normalized.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[1..];
        }

        var metadataIndex = normalized.IndexOf('+');
        if (metadataIndex >= 0)
        {
            normalized = normalized[..metadataIndex];
        }

        return normalized;
    }
}

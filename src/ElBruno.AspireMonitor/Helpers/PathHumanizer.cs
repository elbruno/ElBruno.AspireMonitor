using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ElBruno.AspireMonitor.Helpers;

/// <summary>
/// Truncates long file paths to human-readable form using middle-ellipsis.
/// Uses Win32 PathCompactPathEx when available, with a segment-based fallback.
/// </summary>
public static class PathHumanizer
{
    [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
    private static extern bool PathCompactPathEx(
        [Out] StringBuilder pszOut,
        string szPath,
        int cchMax,
        int dwFlags);

    /// <summary>
    /// Truncates a file path to a maximum character count using middle-ellipsis.
    /// Example: "C:\very\long\path\to\file.txt" -> "C:\very\...\file.txt"
    /// </summary>
    /// <param name="path">Full path to truncate</param>
    /// <param name="maxChars">Maximum character count (default: 50)</param>
    /// <returns>Truncated path with ellipsis, or original path if already short</returns>
    public static string Humanize(string? path, int maxChars = 50)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        // If already short enough, return as-is
        if (path.Length <= maxChars)
            return path;

        try
        {
            // Try Win32 PathCompactPathEx (built into Windows)
            var sb = new StringBuilder(maxChars + 1);
            if (PathCompactPathEx(sb, path, maxChars, 0))
            {
                var result = sb.ToString();
                if (!string.IsNullOrEmpty(result))
                    return result;
            }
        }
        catch (Exception ex)
        {
            // P/Invoke may fail on non-Windows or if shlwapi.dll is missing
            System.Diagnostics.Debug.WriteLine($"[PathHumanizer] Win32 fallback failed: {ex.Message}");
        }

        // Fallback: Segment-based middle-ellipsis
        return HumanizeSegmentBased(path, maxChars);
    }

    private static string HumanizeSegmentBased(string path, int maxChars)
    {
        try
        {
            var separator = Path.DirectorySeparatorChar;
            var segments = path.Split(separator);

            // Keep drive/root + first segment + last 1-2 segments
            if (segments.Length <= 3)
                return path; // Too short to truncate meaningfully

            var root = segments[0]; // e.g., "C:"
            var first = segments.Length > 1 ? segments[1] : "";
            var secondToLast = segments.Length > 1 ? segments[^2] : "";
            var last = segments[^1];

            // Build: "C:\first\...\secondToLast\last"
            var truncated = $"{root}{separator}{first}{separator}...{separator}{secondToLast}{separator}{last}";

            // If still too long, drop first and/or secondToLast
            if (truncated.Length > maxChars)
            {
                truncated = $"{root}{separator}...{separator}{last}";
            }

            return truncated;
        }
        catch
        {
            // If all else fails, just truncate the end
            return path.Length > maxChars
                ? path.Substring(0, maxChars - 3) + "..."
                : path;
        }
    }
}

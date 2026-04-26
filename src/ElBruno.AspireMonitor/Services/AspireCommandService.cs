using System.Diagnostics;
using System.IO;

namespace ElBruno.AspireMonitor.Services;

public class AspireCommandService : IAspireCommandService
{
    public async Task<bool> StartAspireAsync(string workingFolder)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(workingFolder) || !Directory.Exists(workingFolder))
            {
                System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Invalid working folder: {workingFolder}");
                return false;
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = "aspire",
                Arguments = "start",
                WorkingDirectory = workingFolder,
                UseShellExecute = true,
                CreateNoWindow = false,
                Verb = "open"
            };

            using (var process = Process.Start(processInfo))
            {
                if (process == null)
                {
                    System.Diagnostics.Debug.WriteLine("[AspireCommandService] Failed to start aspire process");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Started aspire process (PID: {process.Id})");
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Error starting aspire: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> StopAspireAsync()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "aspire",
                Arguments = "stop",
                UseShellExecute = true,
                CreateNoWindow = false,
                RedirectStandardOutput = false
            };

            using (var process = Process.Start(processInfo))
            {
                if (process == null)
                {
                    System.Diagnostics.Debug.WriteLine("[AspireCommandService] Failed to stop aspire");
                    return false;
                }

                await Task.Run(() => process.WaitForExit(5000));
                System.Diagnostics.Debug.WriteLine("[AspireCommandService] Stopped aspire");
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Error stopping aspire: {ex.Message}");
            return false;
        }
    }

    public async Task<string> GetStatusAsync()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "aspire",
                Arguments = "ps",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                if (process == null)
                    return "Unable to check status";

                var output = await process.StandardOutput.ReadToEndAsync();
                await Task.Run(() => process.WaitForExit(5000));

                return string.IsNullOrWhiteSpace(output) ? "No running instances" : output;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Error getting status: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
}

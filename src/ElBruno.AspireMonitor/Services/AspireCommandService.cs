using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace ElBruno.AspireMonitor.Services;

public class AspireCommandService : IAspireCommandService
{
    /// <summary>
    /// Starts Aspire silently (no terminal window) in the specified working folder.
    /// </summary>
    public async Task<bool> StartAspireAsync(string workingFolder)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(workingFolder) || !Directory.Exists(workingFolder))
            {
                System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Invalid working folder: {workingFolder}");
                return false;
            }

            // Start aspire silently (no window, detached from parent process)
            var processInfo = new ProcessStartInfo
            {
                FileName = "aspire",
                Arguments = "start",
                WorkingDirectory = workingFolder,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(processInfo))
            {
                if (process == null)
                {
                    System.Diagnostics.Debug.WriteLine("[AspireCommandService] Failed to start aspire process");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Started aspire silently (PID: {process.Id}) in {workingFolder}");
                
                // Don't wait for completion - aspire start runs the server in background
                // Give it a moment to initialize
                await Task.Delay(1000);
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Error starting aspire: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Stops all running Aspire instances silently.
    /// </summary>
    public async Task<bool> StopAspireAsync()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "aspire",
                Arguments = "stop",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
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

    /// <summary>
    /// Gets the Aspire endpoint URL by running 'aspire ps' and parsing the output.
    /// Returns the dashboard URL if found, null otherwise.
    /// </summary>
    public async Task<string?> DetectAspireEndpointAsync()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "aspire",
                Arguments = "ps",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                if (process == null)
                    return null;

                var output = await process.StandardOutput.ReadToEndAsync();
                await Task.Run(() => process.WaitForExit(5000));

                return ParseEndpointFromAspirePs(output);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Error detecting Aspire endpoint: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Parses 'aspire ps' output to extract the dashboard endpoint URL.
    /// The output format typically shows AppHost with a URL like: http://localhost:17195
    /// </summary>
    private string? ParseEndpointFromAspirePs(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return null;

        try
        {
            // Look for URLs in the format http://localhost:PORT or https://localhost:PORT
            var urlPattern = @"https?://localhost:\d+";
            var match = Regex.Match(output, urlPattern);
            
            if (match.Success)
            {
                var endpoint = match.Value;
                System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Detected endpoint: {endpoint}");
                return endpoint;
            }

            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Could not parse endpoint from aspire ps output: {output}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Error parsing aspire ps output: {ex.Message}");
            return null;
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

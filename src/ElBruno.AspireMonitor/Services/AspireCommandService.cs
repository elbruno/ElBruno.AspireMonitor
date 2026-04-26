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

                await Task.Run(() => process.WaitForExit(10000));
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
            var output = await RunAspireCommandAsync("ps");
            return ParseEndpointFromAspirePs(output);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Error detecting Aspire endpoint: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets running Aspire instances information via 'aspire ps' command.
    /// </summary>
    public async Task<string> GetRunningInstancesAsync()
    {
        try
        {
            var output = await RunAspireCommandAsync("ps");
            return string.IsNullOrWhiteSpace(output) ? "No running instances" : output;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Error getting running instances: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Gets detailed resource information via 'aspire describe' command.
    /// </summary>
    public async Task<string> DescribeResourcesAsync()
    {
        try
        {
            var output = await RunAspireCommandAsync("describe");
            return string.IsNullOrWhiteSpace(output) ? "No resources found" : output;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Error describing resources: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }

    /// <summary>
    /// Generic method to run Aspire commands silently and capture output.
    /// </summary>
    private async Task<string> RunAspireCommandAsync(string arguments)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = "aspire",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processInfo))
        {
            if (process == null)
                return string.Empty;

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            
            await Task.Run(() => process.WaitForExit(10000));

            if (!string.IsNullOrWhiteSpace(error))
            {
                System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Command 'aspire {arguments}' error: {error}");
            }

            return output ?? string.Empty;
        }
    }

    /// <summary>
    /// Parses 'aspire ps' output to extract the dashboard endpoint URL.
    /// Looks for URLs in the format http://localhost:PORT or https://localhost:PORT
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

            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Could not parse endpoint from aspire ps output");
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
        return await GetRunningInstancesAsync();
    }
}

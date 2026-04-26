using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace ElBruno.AspireMonitor.Services;

public class AspireCommandService : IAspireCommandService
{
    /// <summary>
    /// Starts Aspire silently (no terminal window) in the specified working folder.
    /// Optionally streams output lines to a callback for real-time logging.
    /// </summary>
    public async Task<bool> StartAspireAsync(string workingFolder, Action<string>? logCallback = null)
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
                
                // Stream output asynchronously if callback provided
                if (logCallback != null)
                {
                    _ = StreamProcessOutputAsync(process, logCallback);
                }
                
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
    /// Optionally streams output lines to a callback for real-time logging.
    /// </summary>
    public async Task<bool> StopAspireAsync(Action<string>? logCallback = null)
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

                // Stream output asynchronously if callback provided
                if (logCallback != null)
                {
                    _ = StreamProcessOutputAsync(process, logCallback);
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
    /// Optionally streams output lines to a callback for real-time logging.
    /// </summary>
    public async Task<string?> DetectAspireEndpointAsync(Action<string>? logCallback = null)
    {
        try
        {
            var output = await RunAspireCommandAsync("ps", logCallback);
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
    /// Optionally streams output lines to a callback for real-time logging.
    /// </summary>
    public async Task<string> GetRunningInstancesAsync(Action<string>? logCallback = null)
    {
        try
        {
            var output = await RunAspireCommandAsync("ps", logCallback);
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
    /// Optionally streams output lines to a callback for real-time logging.
    /// </summary>
    public async Task<string> DescribeResourcesAsync(Action<string>? logCallback = null)
    {
        try
        {
            var output = await RunAspireCommandAsync("describe", logCallback);
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
    /// Optionally streams output lines to a callback for real-time logging.
    /// </summary>
    private async Task<string> RunAspireCommandAsync(string arguments, Action<string>? logCallback = null)
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

            // Stream output asynchronously if callback provided
            if (logCallback != null)
            {
                _ = StreamProcessOutputAsync(process, logCallback);
            }

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
    /// Streams process output line-by-line to a callback for real-time logging.
    /// Reads from both stdout and stderr simultaneously.
    /// </summary>
    private async Task StreamProcessOutputAsync(Process process, Action<string> logCallback)
    {
        try
        {
            var stdoutTask = ReadStreamAsync(process.StandardOutput, logCallback);
            var stderrTask = ReadStreamAsync(process.StandardError, logCallback);
            
            await Task.WhenAll(stdoutTask, stderrTask);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Error streaming process output: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads from a stream line-by-line and invokes callback for each non-empty line.
    /// </summary>
    private async Task ReadStreamAsync(StreamReader reader, Action<string> callback)
    {
        try
        {
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                line = line.Trim();
                if (!string.IsNullOrEmpty(line))
                {
                    callback(line);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Error reading stream: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses 'aspire ps' output to extract the dashboard endpoint URL.
    /// Looks for URLs in the format http://localhost:PORT or https://localhost:PORT
    /// </summary>
    private string? ParseEndpointFromAspirePs(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            System.Diagnostics.Debug.WriteLine("[AspireCommandService] ParseEndpointFromAspirePs: output is null or empty");
            return null;
        }

        try
        {
            // Look for URLs in the format http://localhost:PORT or https://localhost:PORT
            // Also support http://127.0.0.1:PORT
            var urlPattern = @"https?://(?:localhost|127\.0\.0\.1):\d+";
            var match = Regex.Match(output, urlPattern);
            
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] ParseEndpointFromAspirePs: pattern={urlPattern}, match.Success={match.Success}");
            
            if (match.Success)
            {
                var endpoint = match.Value;
                // Remove any query parameters (e.g., /login?t=...)
                if (endpoint.Contains("?"))
                {
                    endpoint = endpoint.Split('?')[0];
                }
                System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Detected endpoint: {endpoint}");
                return endpoint;
            }

            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Could not parse endpoint from aspire ps output");
            System.Diagnostics.Debug.WriteLine($"[AspireCommandService] Output preview: {(output.Length > 200 ? output.Substring(0, 200) : output)}");
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

using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using ElBruno.AspireMonitor.Models;

namespace ElBruno.AspireMonitor.Services;

public class AspireCliService
{
    private const int CommandTimeoutSeconds = 10;

    /// <summary>
    /// Working directory used when invoking the Aspire CLI. The CLI auto-discovers
    /// running AppHost instances via lock files in this directory tree, so it MUST
    /// match the folder where the user launched 'aspire start'.
    /// </summary>
    public string? WorkingDirectory { get; set; }

    public async Task<string> ExecuteCommandAsync(string command, string arguments = "", CancellationToken cancellationToken = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            if (!string.IsNullOrWhiteSpace(WorkingDirectory) && Directory.Exists(WorkingDirectory))
            {
                startInfo.WorkingDirectory = WorkingDirectory;
            }

            using var process = new Process { StartInfo = startInfo };
            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    output.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    error.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(CommandTimeoutSeconds), cancellationToken);
            var processTask = process.WaitForExitAsync(cancellationToken);
            
            var completedTask = await Task.WhenAny(processTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Ignore kill errors
                }
                throw new TimeoutException($"Command '{command} {arguments}' timed out after {CommandTimeoutSeconds} seconds");
            }

            await processTask;

            if (process.ExitCode != 0)
            {
                var errorMessage = error.ToString().Trim();
                if (string.IsNullOrWhiteSpace(errorMessage))
                    errorMessage = $"Command exited with code {process.ExitCode}";
                throw new InvalidOperationException($"Command failed: {errorMessage}");
            }

            return output.ToString();
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            throw new InvalidOperationException($"Command '{command}' not found or cannot be executed. Ensure Aspire CLI is installed.", ex);
        }
    }

    public async Task<JsonDocument?> ExecuteJsonAsync(string command, string arguments = "", CancellationToken cancellationToken = default)
    {
        var output = await ExecuteCommandAsync(command, arguments, cancellationToken);
        
        if (string.IsNullOrWhiteSpace(output))
            return null;

        try
        {
            return JsonDocument.Parse(output);
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCliService] Failed to parse JSON from '{command} {arguments}': {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AspireCliService] Output: {output}");
            throw new InvalidOperationException($"Failed to parse JSON output from command: {ex.Message}", ex);
        }
    }

    public async Task<ResourceCollection> ParseResourcesFromDescribeJsonAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var jsonDoc = await ExecuteJsonAsync("aspire", "describe --format json", cancellationToken);
            
            if (jsonDoc == null)
                return new ResourceCollection { ErrorMessage = "No output from 'aspire describe'" };

            var resources = new List<AspireResource>();
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("resources", out var resourcesArray) && resourcesArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var resourceElement in resourcesArray.EnumerateArray())
                {
                    var resource = ParseResource(resourceElement);
                    if (resource != null)
                        resources.Add(resource);
                }
            }

            return new ResourceCollection(resources);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found") || ex.Message.Contains("cannot be executed"))
        {
            return new ResourceCollection { ErrorMessage = "Aspire CLI not found. Please install .NET Aspire." };
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Command failed"))
        {
            return new ResourceCollection { ErrorMessage = "No Aspire app is currently running." };
        }
        catch (TimeoutException)
        {
            return new ResourceCollection { ErrorMessage = "Command timed out. Aspire might be unresponsive." };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCliService] Unexpected error: {ex}");
            return new ResourceCollection { ErrorMessage = $"Error: {ex.Message}" };
        }
    }

    private AspireResource? ParseResource(JsonElement element)
    {
        try
        {
            var name = element.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var resource = new AspireResource
            {
                Name = name,
                Id = name
            };

            if (element.TryGetProperty("type", out var typeEl))
                resource.Type = typeEl.GetString();
            else if (element.TryGetProperty("resourceType", out var resourceTypeEl))
                resource.Type = resourceTypeEl.GetString();

            if (element.TryGetProperty("state", out var stateEl))
            {
                var state = stateEl.GetString();
                resource.Status = ParseResourceStatus(state);
            }

            // Aspire emits "urls": [ { "name": "...", "url": "http://..." } ].
            // Older/alternative shape used "endpoints" (string[] or [{url:...}]).
            // Accept both so the monitor works across CLI versions.
            if (element.TryGetProperty("urls", out var urlsEl) && urlsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var entry in urlsEl.EnumerateArray())
                {
                    if (entry.ValueKind == JsonValueKind.String)
                    {
                        var url = entry.GetString();
                        if (!string.IsNullOrWhiteSpace(url))
                            resource.Endpoints.Add(url);
                    }
                    else if (entry.ValueKind == JsonValueKind.Object && entry.TryGetProperty("url", out var urlEl))
                    {
                        var url = urlEl.GetString();
                        if (!string.IsNullOrWhiteSpace(url))
                            resource.Endpoints.Add(url);
                    }
                }
            }

            if (element.TryGetProperty("endpoints", out var endpointsEl) && endpointsEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var endpoint in endpointsEl.EnumerateArray())
                {
                    if (endpoint.ValueKind == JsonValueKind.String)
                    {
                        var url = endpoint.GetString();
                        if (!string.IsNullOrWhiteSpace(url))
                            resource.Endpoints.Add(url);
                    }
                    else if (endpoint.TryGetProperty("url", out var urlEl))
                    {
                        var url = urlEl.GetString();
                        if (!string.IsNullOrWhiteSpace(url))
                            resource.Endpoints.Add(url);
                    }
                }
            }

            if (element.TryGetProperty("cpu", out var cpuEl) && cpuEl.TryGetDouble(out var cpu))
                resource.Metrics.CpuUsagePercent = cpu;

            if (element.TryGetProperty("memory", out var memEl) && memEl.TryGetDouble(out var mem))
                resource.Metrics.MemoryUsagePercent = mem;

            return resource;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCliService] Failed to parse resource: {ex.Message}");
            return null;
        }
    }

    private ResourceStatus ParseResourceStatus(string? state)
    {
        if (string.IsNullOrWhiteSpace(state))
            return ResourceStatus.Unknown;

        return state.ToLowerInvariant() switch
        {
            "running" => ResourceStatus.Running,
            "starting" => ResourceStatus.Starting,
            "stopped" => ResourceStatus.Stopped,
            "failed" => ResourceStatus.Failed,
            "finishing" => ResourceStatus.Stopping,
            _ => ResourceStatus.Unknown
        };
    }

    public async IAsyncEnumerable<string> GetLiveLogsAsync(string resourceName, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "aspire",
            Arguments = $"logs {resourceName}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };

        if (!string.IsNullOrWhiteSpace(WorkingDirectory) && Directory.Exists(WorkingDirectory))
        {
            startInfo.WorkingDirectory = WorkingDirectory;
        }

        using var process = new Process { StartInfo = startInfo };
        
        try
        {
            process.Start();

            while (!process.StandardOutput.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await process.StandardOutput.ReadLineAsync(cancellationToken);
                if (line != null)
                    yield return line;
            }
        }
        finally
        {
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Ignore kill errors
                }
            }
        }
    }
}

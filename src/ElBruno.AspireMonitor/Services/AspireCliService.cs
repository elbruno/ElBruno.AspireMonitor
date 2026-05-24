using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using ElBruno.AspireMonitor.Models;

namespace ElBruno.AspireMonitor.Services;

public class AspireCliService
{
    private const int CommandTimeoutSeconds = 10;
    private const string AspireConfigFileName = "aspire.config.json";

    /// <summary>
    /// Working directory used when invoking the Aspire CLI. The CLI auto-discovers
    /// running AppHost instances via lock files in this directory tree, so it MUST
    /// match the folder where the user launched 'aspire start'.
    /// </summary>
    public string? WorkingDirectory { get; set; }
    public bool EnableWorktreeDiscovery { get; set; }
    public string? WorktreeBasePath { get; set; }

    public virtual async Task<string> ExecuteCommandAsync(string command, string arguments = "", CancellationToken cancellationToken = default)
    {
        return await ExecuteCommandCoreAsync(command, arguments, null, cancellationToken);
    }

    private async Task<string> ExecuteCommandCoreAsync(
        string command,
        string arguments,
        string? explicitWorkingDirectory,
        CancellationToken cancellationToken = default)
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

            var resolvedWorkingDirectory = explicitWorkingDirectory;
            if (string.IsNullOrWhiteSpace(resolvedWorkingDirectory))
            {
                resolvedWorkingDirectory = WorkingDirectory;
            }

            if (!string.IsNullOrWhiteSpace(resolvedWorkingDirectory) && Directory.Exists(resolvedWorkingDirectory))
            {
                startInfo.WorkingDirectory = resolvedWorkingDirectory;
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

    public virtual async Task<JsonDocument?> ExecuteJsonAsync(string command, string arguments = "", CancellationToken cancellationToken = default)
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

    protected virtual async Task<JsonDocument?> ExecuteDescribeJsonForDirectoryAsync(string workingDirectory, CancellationToken cancellationToken = default)
    {
        var output = await ExecuteCommandCoreAsync("aspire", "describe --format json", workingDirectory, cancellationToken);
        if (string.IsNullOrWhiteSpace(output))
            return null;

        return JsonDocument.Parse(output);
    }

    protected virtual Task<string> GetGitWorktreeListAsync(string rootPath, CancellationToken cancellationToken = default)
    {
        var escapedRoot = rootPath.Replace("\"", "\\\"");
        return ExecuteCommandCoreAsync("git", $"-C \"{escapedRoot}\" worktree list --porcelain", rootPath, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<string>> DiscoverAspireConfigWorkingDirectoriesAsync(CancellationToken cancellationToken = default)
    {
        var basePath = ResolveWorktreeBasePath();
        if (string.IsNullOrWhiteSpace(basePath) || !Directory.Exists(basePath))
            return Array.Empty<string>();

        var discoveredDirectories = new List<string>();

        if (HasValidAspireConfig(basePath))
        {
            discoveredDirectories.Add(Path.GetFullPath(basePath));
        }

        try
        {
            var worktreeOutput = await GetGitWorktreeListAsync(basePath, cancellationToken);
            foreach (var worktreePath in ParseWorktreePaths(worktreeOutput))
            {
                if (!IsWithinBasePath(worktreePath, basePath))
                    continue;

                if (!HasValidAspireConfig(worktreePath))
                    continue;

                discoveredDirectories.Add(Path.GetFullPath(worktreePath));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCliService] Worktree discovery fallback: {ex.Message}");
        }

        return discoveredDirectories
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public virtual async Task<ResourceCollection> ParseResourcesFromDescribeJsonAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var workingDirectories = EnableWorktreeDiscovery
                ? (await DiscoverAspireConfigWorkingDirectoriesAsync(cancellationToken)).ToList()
                : new List<string>();

            if (workingDirectories.Count > 1)
            {
                return await ParseResourcesAcrossWorktreesAsync(workingDirectories, cancellationToken);
            }

            using var jsonDoc = workingDirectories.Count == 1
                ? await ExecuteDescribeJsonForDirectoryAsync(workingDirectories[0], cancellationToken)
                : await ExecuteJsonAsync("aspire", "describe --format json", cancellationToken);
             
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
            return new ResourceCollection { ErrorMessage = "Aspire CLI not found. Please install Aspire." };
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

    private async Task<ResourceCollection> ParseResourcesAcrossWorktreesAsync(
        IReadOnlyList<string> workingDirectories,
        CancellationToken cancellationToken = default)
    {
        var allResources = new List<AspireResource>();

        foreach (var workingDirectory in workingDirectories)
        {
            try
            {
                using var jsonDoc = await ExecuteDescribeJsonForDirectoryAsync(workingDirectory, cancellationToken);
                if (jsonDoc == null)
                    continue;

                if (!jsonDoc.RootElement.TryGetProperty("resources", out var resourcesArray) || resourcesArray.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var resourceElement in resourcesArray.EnumerateArray())
                {
                    var resource = ParseResource(resourceElement);
                    if (resource == null)
                        continue;

                    PrefixResourceWithWorktree(resource, workingDirectory);
                    allResources.Add(resource);
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Command failed", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"[AspireCliService] Skipping inactive worktree '{workingDirectory}': {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AspireCliService] Failed to parse worktree '{workingDirectory}': {ex.Message}");
            }
        }

        if (allResources.Count == 0)
            return new ResourceCollection { ErrorMessage = "No Aspire app is currently running." };

        return new ResourceCollection(allResources);
    }

    private static void PrefixResourceWithWorktree(AspireResource resource, string workingDirectory)
    {
        var worktreeName = Path.GetFileName(workingDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrWhiteSpace(worktreeName))
            return;

        resource.Name = $"{worktreeName}/{resource.Name}";
        resource.Id = $"{worktreeName}/{resource.Id}";
        resource.Environment.Add(new AspireEnvironmentEntry
        {
            Name = "ASPIREMON_WORKTREE",
            Value = workingDirectory
        });
    }

    private string? ResolveWorktreeBasePath()
    {
        if (!EnableWorktreeDiscovery)
            return null;

        if (!string.IsNullOrWhiteSpace(WorktreeBasePath))
            return WorktreeBasePath;

        return WorkingDirectory;
    }

    private static IEnumerable<string> ParseWorktreePaths(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            yield break;

        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            const string prefix = "worktree ";
            if (!line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            var path = line[prefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(path))
                continue;

            yield return path;
        }
    }

    private static bool IsWithinBasePath(string candidatePath, string basePath)
    {
        var fullBasePath = EnsureTrailingSeparator(Path.GetFullPath(basePath));
        var fullCandidatePath = EnsureTrailingSeparator(Path.GetFullPath(candidatePath));
        return fullCandidatePath.StartsWith(fullBasePath, StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingSeparator(string path)
    {
        if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
            path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            return path;

        return path + Path.DirectorySeparatorChar;
    }

    private static bool HasValidAspireConfig(string directory)
    {
        var configPath = Path.Combine(directory, AspireConfigFileName);
        if (!File.Exists(configPath))
            return false;

        try
        {
            using var stream = File.OpenRead(configPath);
            using var document = JsonDocument.Parse(stream);
            if (!document.RootElement.TryGetProperty("appHost", out var appHostElement))
                return false;

            if (!appHostElement.TryGetProperty("path", out var appHostPathElement))
                return false;

            var appHostPath = appHostPathElement.GetString();
            if (string.IsNullOrWhiteSpace(appHostPath))
                return false;

            var resolvedAppHostPath = Path.GetFullPath(Path.Combine(directory, appHostPath));
            return File.Exists(resolvedAppHostPath) || Directory.Exists(resolvedAppHostPath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCliService] Invalid aspire.config.json at '{configPath}': {ex.Message}");
            return false;
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
            else if (element.TryGetProperty("status", out var statusEl))
            {
                var status = statusEl.GetString();
                resource.Status = ParseResourceStatus(status);
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
                            resource.Endpoints.Add(new AspireEndpoint { EndpointUrl = url });
                    }
                    else if (entry.ValueKind == JsonValueKind.Object && entry.TryGetProperty("url", out var urlEl))
                    {
                        var url = urlEl.GetString();
                        if (!string.IsNullOrWhiteSpace(url))
                            resource.Endpoints.Add(new AspireEndpoint { EndpointUrl = url });
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
                            resource.Endpoints.Add(new AspireEndpoint { EndpointUrl = url });
                    }
                    else if (endpoint.TryGetProperty("url", out var urlEl))
                    {
                        var url = urlEl.GetString();
                        if (!string.IsNullOrWhiteSpace(url))
                            resource.Endpoints.Add(new AspireEndpoint { EndpointUrl = url });
                    }
                    else if (endpoint.ValueKind == JsonValueKind.Object)
                    {
                        var endpointUrl = endpoint.TryGetProperty("endpointUrl", out var endpointUrlEl)
                            ? endpointUrlEl.GetString()
                            : null;
                        var proxyUrl = endpoint.TryGetProperty("proxyUrl", out var proxyUrlEl)
                            ? proxyUrlEl.GetString()
                            : null;

                        if (!string.IsNullOrWhiteSpace(endpointUrl) || !string.IsNullOrWhiteSpace(proxyUrl))
                        {
                            resource.Endpoints.Add(new AspireEndpoint
                            {
                                EndpointUrl = endpointUrl,
                                ProxyUrl = proxyUrl
                            });
                        }
                    }
                }
            }

            ParseMetrics(element, resource.Metrics);
            ParseEnvironment(element, resource.Environment);

            return resource;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireCliService] Failed to parse resource: {ex.Message}");
            return null;
        }
    }

    private static void ParseMetrics(JsonElement element, ResourceMetrics metrics)
    {
        if (element.TryGetProperty("properties", out var propertiesEl) && propertiesEl.ValueKind == JsonValueKind.Object)
        {
            ApplyMetricsObject(propertiesEl, metrics);
        }

        if (element.TryGetProperty("metrics", out var metricsEl) && metricsEl.ValueKind == JsonValueKind.Object)
        {
            ApplyMetricsObject(metricsEl, metrics);
        }

        if (element.TryGetProperty("cpu", out var cpuEl) && cpuEl.TryGetDouble(out var cpu))
            metrics.CpuUsagePercent = cpu;

        if (element.TryGetProperty("memory", out var memEl) && memEl.TryGetDouble(out var mem))
            metrics.MemoryUsage = mem;

        if (element.TryGetProperty("disk", out var diskEl) && diskEl.TryGetDouble(out var disk))
            metrics.DiskUsagePercent = disk;
    }

    private static void ApplyMetricsObject(JsonElement metricsElement, ResourceMetrics metrics)
    {
        if (TryGetDouble(metricsElement, "cpuUsage", out var cpuUsage) ||
            TryGetDouble(metricsElement, "cpuUsagePercent", out cpuUsage))
        {
            metrics.CpuUsagePercent = cpuUsage;
        }

        if (TryGetDouble(metricsElement, "memoryLimit", out var memoryLimit))
        {
            metrics.MemoryLimit = memoryLimit;
        }

        if (TryGetDouble(metricsElement, "memoryUsage", out var memoryUsage))
        {
            metrics.MemoryUsage = memoryUsage;
        }
        else if (TryGetDouble(metricsElement, "memoryUsagePercent", out var memoryUsagePercent))
        {
            metrics.MemoryUsage = memoryUsagePercent;
            metrics.MemoryLimit = 100;
        }

        if (TryGetDouble(metricsElement, "diskUsage", out var diskUsage) ||
            TryGetDouble(metricsElement, "diskUsagePercent", out diskUsage))
        {
            metrics.DiskUsagePercent = diskUsage;
        }
    }

    private static bool TryGetDouble(JsonElement element, string propertyName, out double value)
    {
        value = default;

        return element.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.Number
            && property.TryGetDouble(out value);
    }

    private static void ParseEnvironment(JsonElement element, List<AspireEnvironmentEntry> environment)
    {
        if (!element.TryGetProperty("environment", out var environmentEl))
            return;

        if (environmentEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in environmentEl.EnumerateArray())
            {
                if (entry.ValueKind == JsonValueKind.Object)
                {
                    var name = entry.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null;
                    var value = entry.TryGetProperty("value", out var valueEl) ? valueEl.GetString() : null;

                    if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(value))
                    {
                        environment.Add(new AspireEnvironmentEntry
                        {
                            Name = name,
                            Value = value
                        });
                    }
                }
            }
        }
        else if (environmentEl.ValueKind == JsonValueKind.Object)
        {
            foreach (var entry in environmentEl.EnumerateObject())
            {
                environment.Add(new AspireEnvironmentEntry
                {
                    Name = entry.Name,
                    Value = entry.Value.ValueKind == JsonValueKind.String
                        ? entry.Value.GetString()
                        : entry.Value.ToString()
                });
            }
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

    public virtual async IAsyncEnumerable<string> GetLiveLogsAsync(string resourceName, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
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

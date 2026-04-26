using System.Net.Http;
using System.Text.Json;
using ElBruno.AspireMonitor.Models;
using Polly;
using Polly.Retry;

namespace ElBruno.AspireMonitor.Services;

public class AspireApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly Configuration _configuration;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private bool _disposed;

    public AspireApiClient(Configuration configuration)
    {
        _configuration = configuration;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_configuration.AspireEndpoint),
            Timeout = TimeSpan.FromSeconds(5)
        };

        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1))
            );
    }

    public AspireApiClient(HttpClient httpClient, Configuration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1))
            );
    }

    public async Task<List<AspireResource>> GetResourcesAsync()
    {
        try
        {
            var endpoint = _httpClient.BaseAddress?.ToString() ?? "unknown";
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient] GET /api/resources from {endpoint}");

            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync("/api/resources")
            );

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[AspireApiClient] ❌ Failed to get resources: HTTP {(int)response.StatusCode} {response.StatusCode}");
                return new List<AspireResource>();
            }

            var content = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient] ✓ Response received, content length: {content.Length} bytes");

            var resources = JsonSerializer.Deserialize<List<AspireResource>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var count = resources?.Count ?? 0;
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient] ✓ Deserialized {count} resources");

            return resources ?? new List<AspireResource>();
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient] ❌ HTTP Request Exception: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient]    Endpoint: {_httpClient.BaseAddress}");
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient]    InnerException: {ex.InnerException?.Message}");
            return new List<AspireResource>();
        }
        catch (TaskCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient] ❌ Request timeout (5 seconds): {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient]    Endpoint: {_httpClient.BaseAddress}");
            return new List<AspireResource>();
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient] ❌ JSON parse error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient]    Path: {ex.Path}");
            return new List<AspireResource>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient] ❌ Unexpected error: {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient]    StackTrace: {ex.StackTrace}");
            return new List<AspireResource>();
        }
    }

    public async Task<AspireResource?> GetResourceAsync(string id)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync($"/api/resources/{id}")
            );

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[AspireApiClient] Failed to get resource {id}: {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var resource = JsonSerializer.Deserialize<AspireResource>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return resource;
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient] HTTP Request Exception for resource {id}: {ex.Message}");
            return null;
        }
        catch (TaskCanceledException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient] Request timeout for resource {id}: {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient] JSON parse error for resource {id}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient] Unexpected error for resource {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<HealthStatus> GetHealthAsync()
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync("/api/health")
            );

            if (!response.IsSuccessStatusCode)
            {
                return new HealthStatus(StatusColor.Red, "Health check failed");
            }

            return new HealthStatus(StatusColor.Green, "Healthy");
        }
        catch (HttpRequestException ex)
        {
            return new HealthStatus(StatusColor.Red, $"Connection error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return new HealthStatus(StatusColor.Red, "Request timeout");
        }
        catch (Exception ex)
        {
            return new HealthStatus(StatusColor.Red, $"Error: {ex.Message}");
        }
    }

    public void UpdateEndpoint(string newEndpoint)
    {
        if (!string.IsNullOrWhiteSpace(newEndpoint))
        {
            _httpClient.BaseAddress = new Uri(newEndpoint);
            System.Diagnostics.Debug.WriteLine($"[AspireApiClient] Endpoint updated to: {newEndpoint}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

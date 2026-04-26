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
            var response = await _retryPolicy.ExecuteAsync(async () =>
                await _httpClient.GetAsync("/api/resources")
            );

            if (!response.IsSuccessStatusCode)
            {
                return new List<AspireResource>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var resources = JsonSerializer.Deserialize<List<AspireResource>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return resources ?? new List<AspireResource>();
        }
        catch (HttpRequestException)
        {
            return new List<AspireResource>();
        }
        catch (TaskCanceledException)
        {
            return new List<AspireResource>();
        }
        catch (JsonException)
        {
            return new List<AspireResource>();
        }
        catch (Exception)
        {
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
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var resource = JsonSerializer.Deserialize<AspireResource>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return resource;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (Exception)
        {
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

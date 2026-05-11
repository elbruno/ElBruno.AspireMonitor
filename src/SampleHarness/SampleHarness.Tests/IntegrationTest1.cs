using Aspire.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;

namespace SampleHarness.Tests;

/// <summary>
/// Validates the AppHost topology without starting any real infrastructure.
/// These tests run in milliseconds and require no Docker or network access.
/// </summary>
public class AppHostTopologyTests
{
    [Fact]
    public async Task AppHost_RegistersNamedResourcesForMiniMonitorFiltering()
    {
        // Arrange & Act
        await using var app = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.SampleHarness_AppHost>();

        var resources = app.Resources.ToList();

        // Assert – service resources and the no-endpoint duplicate probe must always be present
        resources.Should().Contain(r => r.Name == "api-service",
            "AppHost must register api-service");
        resources.Should().Contain(r => r.Name == "catalog-api",
            "AppHost must register catalog-api");
        resources.Should().Contain(r => r.Name == "filter-probe-api",
            "AppHost must register an endpoint-bearing web project for mini monitor filtering");
        resources.Should().Contain(r => r.Name == "filter-probe-api-executable",
            "AppHost must register a no-endpoint executable with the same prefix to cover duplicate filtering");
        resources.Should().Contain(r => r.Name == "worker-service",
            "AppHost must register worker-service");
    }

    [Fact]
    public async Task AppHost_ApiService_HasExternalHttpEndpoints()
    {
        await using var app = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.SampleHarness_AppHost>();

        var apiService = app.Resources
            .OfType<IResourceWithEndpoints>()
            .FirstOrDefault(r => r.Name == "api-service");

        apiService.Should().NotBeNull("api-service must be present in the topology");
    }

    [Fact]
    public async Task AppHost_CatalogApi_HasExternalHttpEndpoints()
    {
        await using var app = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.SampleHarness_AppHost>();

        var catalogApi = app.Resources
            .OfType<IResourceWithEndpoints>()
            .FirstOrDefault(r => r.Name == "catalog-api");

        catalogApi.Should().NotBeNull("catalog-api must be present in the topology");
    }

    [Fact]
    public async Task AppHost_FilterProbeApi_HasEndpointBearingProjectAndNoEndpointExecutable()
    {
        await using var app = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.SampleHarness_AppHost>();

        var filterProbeApi = app.Resources
            .OfType<IResourceWithEndpoints>()
            .FirstOrDefault(r => r.Name == "filter-probe-api");
        var executable = app.Resources.FirstOrDefault(r => r.Name == "filter-probe-api-executable");

        filterProbeApi.Should().NotBeNull("filter-probe-api must be the endpoint-bearing project entry");
        executable.Should().NotBeNull("filter-probe-api-executable must be the paired no-endpoint entry");
        filterProbeApi!.Annotations.OfType<EndpointAnnotation>().Should().NotBeEmpty(
            "the filter probe web project must expose endpoints");
        executable!.Annotations.OfType<EndpointAnnotation>().Should().BeEmpty(
            "the mini monitor needs a same-prefix resource without endpoints to hide by default");
    }

    [Fact]
    public async Task AppHost_WorkerService_IsRegistered()
    {
        await using var app = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.SampleHarness_AppHost>();

        var worker = app.Resources.FirstOrDefault(r => r.Name == "worker-service");

        worker.Should().NotBeNull("worker-service must be present in the topology");
    }

    [Fact]
    public async Task AppHost_WorkerService_HasReferenceToApiService()
    {
        await using var app = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.SampleHarness_AppHost>();

        var worker = app.Resources.FirstOrDefault(r => r.Name == "worker-service");

        worker.Should().NotBeNull();

        // Worker should carry ServiceReferencesAnnotation or EnvironmentCallbackAnnotation
        // that proves it received a reference to api-service.
        var annotations = worker!.Annotations;
        annotations.Should().NotBeEmpty("worker-service must have annotations from WaitFor/WithReference");
    }
}

/// <summary>
/// Fast in-process tests for the ApiService using WebApplicationFactory.
/// No Aspire infrastructure required — these are pure service-level tests.
/// </summary>
public class ApiServiceEndpointTests : IClassFixture<WebApplicationFactory<ApiServiceProgram>>
{
    private readonly WebApplicationFactory<ApiServiceProgram> _factory;

    public ApiServiceEndpointTests(WebApplicationFactory<ApiServiceProgram> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRoot_ReturnsOkWithServiceName()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("service").GetString().Should().Be("api-service");
        body.GetProperty("status").GetString().Should().Be("healthy");
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsFiveItems()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/weatherforecast");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var forecasts = await response.Content.ReadFromJsonAsync<JsonElement[]>();
        forecasts.Should().HaveCount(5, "the endpoint always returns exactly 5 forecast items");
    }

    [Fact]
    public async Task GetWeatherForecast_EachItemHasRequiredFields()
    {
        var client = _factory.CreateClient();

        var forecasts = await client.GetFromJsonAsync<JsonElement[]>("/weatherforecast");

        forecasts.Should().NotBeNull();
        foreach (var item in forecasts!)
        {
            item.TryGetProperty("date", out _).Should().BeTrue("each forecast needs a date");
            item.TryGetProperty("temperatureC", out _).Should().BeTrue("each forecast needs temperatureC");
            item.TryGetProperty("temperatureF", out _).Should().BeTrue("each forecast needs temperatureF");
            item.TryGetProperty("summary", out _).Should().BeTrue("each forecast needs a summary");
        }
    }
}

/// <summary>
/// Fast in-process tests for the CatalogApi using WebApplicationFactory.
/// </summary>
public class CatalogApiEndpointTests
    : IClassFixture<WebApplicationFactory<CatalogApiProgram>>
{
    private readonly WebApplicationFactory<CatalogApiProgram> _factory;

    public CatalogApiEndpointTests(WebApplicationFactory<CatalogApiProgram> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRoot_ReturnsOkWithServiceName()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("service").GetString().Should().Be("catalog-api");
    }

    [Fact]
    public async Task GetProducts_ReturnsThreeStaticProducts()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<JsonElement[]>();
        products.Should().HaveCount(3, "the catalog has exactly 3 seed products");
    }

    [Fact]
    public async Task GetProductById_ReturnsCorrectProduct()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<JsonElement>();
        product.GetProperty("id").GetInt32().Should().Be(1);
        product.GetProperty("name").GetString().Should().Be("Widget Alpha");
    }

    [Fact]
    public async Task GetProductById_ReturnsNotFoundForMissingId()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products/9999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

/// <summary>
/// Fast in-process tests for the filter probe web project used by mini monitor resource filtering.
/// </summary>
public class FilterProbeApiEndpointTests
    : IClassFixture<WebApplicationFactory<FilterProbeApiProgram>>
{
    private readonly WebApplicationFactory<FilterProbeApiProgram> _factory;

    public FilterProbeApiEndpointTests(WebApplicationFactory<FilterProbeApiProgram> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRoot_ReturnsOkWithServiceName()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("service").GetString().Should().Be("filter-probe-api");
        body.GetProperty("status").GetString().Should().Be("healthy");
    }
}

using Microsoft.AspNetCore.Mvc.Testing;

namespace ElBruno.AspireMonitor.SampleHarness.Tests;

public class CatalogApiTests
{
    [Fact]
    public async Task Root_endpoint_returns_catalog_summary()
    {
        await using var factory = new WebApplicationFactory<ElBruno.AspireMonitor.SampleHarness.CatalogApi.CatalogApiMarker>();
        var client = factory.CreateClient();

        var response = await client.GetStringAsync("/");

        Assert.Contains("catalog-api", response);
        Assert.Contains("3", response);
    }

    [Fact]
    public async Task Catalog_items_endpoint_returns_seed_data()
    {
        await using var factory = new WebApplicationFactory<ElBruno.AspireMonitor.SampleHarness.CatalogApi.CatalogApiMarker>();
        var client = factory.CreateClient();

        var response = await client.GetStringAsync("/catalog/items");

        Assert.Contains("coffee", response);
        Assert.Contains("syrup", response);
    }
}

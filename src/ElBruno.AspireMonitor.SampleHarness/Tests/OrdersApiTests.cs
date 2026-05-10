using Microsoft.AspNetCore.Mvc.Testing;

namespace ElBruno.AspireMonitor.SampleHarness.Tests;

public class OrdersApiTests
{
    [Fact]
    public async Task Root_endpoint_returns_orders_summary()
    {
        await using var factory = new WebApplicationFactory<ElBruno.AspireMonitor.SampleHarness.OrdersApi.OrdersApiMarker>();
        var client = factory.CreateClient();

        var response = await client.GetStringAsync("/");

        Assert.Contains("orders-api", response);
        Assert.Contains("3", response);
    }

    [Fact]
    public async Task Orders_endpoint_returns_seed_data()
    {
        await using var factory = new WebApplicationFactory<ElBruno.AspireMonitor.SampleHarness.OrdersApi.OrdersApiMarker>();
        var client = factory.CreateClient();

        var response = await client.GetStringAsync("/orders");

        Assert.Contains("order-001", response);
        Assert.Contains("order-003", response);
    }
}

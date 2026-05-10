var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject("catalog-api", @"..\CatalogApi\ElBruno.AspireMonitor.SampleHarness.CatalogApi.csproj");
builder.AddProject("orders-api", @"..\OrdersApi\ElBruno.AspireMonitor.SampleHarness.OrdersApi.csproj");
builder.AddProject("worker", @"..\Worker\ElBruno.AspireMonitor.SampleHarness.Worker.csproj");

builder.Build().Run();

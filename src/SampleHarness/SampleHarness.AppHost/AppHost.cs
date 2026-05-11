var builder = DistributedApplication.CreateBuilder(args);

// Orchestrate representative services so AspireMonitor always has
// a deterministic topology to monitor during development and CI.
var apiService = builder.AddProject<Projects.SampleHarness_ApiService>("api-service")
    .WithExternalHttpEndpoints();

var catalogApi = builder.AddProject<Projects.SampleHarness_CatalogApi>("catalog-api")
    .WithExternalHttpEndpoints();

var filterProbeApi = builder.AddProject<Projects.SampleHarness_FilterProbeApi>("filter-probe-api")
    .WithExternalHttpEndpoints();

builder.AddExecutable(
        "filter-probe-api-executable",
        "dotnet",
        Path.Combine("..", "SampleHarness.FilterProbeApi"),
        "--info")
    .WaitFor(filterProbeApi);

// Worker depends on the two HTTP services (shows resource relationships
// in the Aspire dashboard and exercises the monitor's topology view).
builder.AddProject<Projects.SampleHarness_WorkerService>("worker-service")
    .WithReference(apiService)
    .WithReference(catalogApi)
    .WithReference(filterProbeApi)
    .WaitFor(apiService)
    .WaitFor(catalogApi)
    .WaitFor(filterProbeApi);

builder.Build().Run();

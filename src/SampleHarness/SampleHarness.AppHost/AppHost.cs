var builder = DistributedApplication.CreateBuilder(args);

// Orchestrate three representative services so AspireMonitor always has
// a deterministic topology to monitor during development and CI.
var apiService = builder.AddProject<Projects.SampleHarness_ApiService>("api-service")
    .WithExternalHttpEndpoints();

var catalogApi = builder.AddProject<Projects.SampleHarness_CatalogApi>("catalog-api")
    .WithExternalHttpEndpoints();

// Worker depends on the two HTTP services (shows resource relationships
// in the Aspire dashboard and exercises the monitor's topology view).
builder.AddProject<Projects.SampleHarness_WorkerService>("worker-service")
    .WithReference(apiService)
    .WithReference(catalogApi)
    .WaitFor(apiService)
    .WaitFor(catalogApi);

builder.Build().Run();

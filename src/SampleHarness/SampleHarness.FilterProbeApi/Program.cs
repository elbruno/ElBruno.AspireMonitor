var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Ok(new { service = "filter-probe-api", status = "healthy" }))
   .WithName("GetRoot");

app.Run();

// Unique anchor so WebApplicationFactory in tests can reference this project.
public class FilterProbeApiProgram { }

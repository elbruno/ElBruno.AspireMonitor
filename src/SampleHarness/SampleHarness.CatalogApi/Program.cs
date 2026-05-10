var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Static product catalog — deterministic data makes it a reliable E2E target.
var products = new[]
{
    new Product(1, "Widget Alpha", 9.99m, 100),
    new Product(2, "Widget Beta",  14.99m, 75),
    new Product(3, "Gadget Gamma", 24.99m, 50),
};

app.MapGet("/api/products", () => products)
   .WithName("GetProducts");

app.MapGet("/api/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
.WithName("GetProductById");

app.MapGet("/", () => Results.Ok(new { service = "catalog-api", status = "healthy" }))
   .WithName("GetRoot");

app.Run();

record Product(int Id, string Name, decimal Price, int Stock);

// Unique anchor so WebApplicationFactory in tests can reference this project.
public class CatalogApiProgram { }

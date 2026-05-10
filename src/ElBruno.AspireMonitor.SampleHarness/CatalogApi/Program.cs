var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var catalogItems = new[]
{
    new CatalogItem("coffee", "Coffee Beans", 12.50m),
    new CatalogItem("tea", "Green Tea", 8.25m),
    new CatalogItem("syrup", "Caramel Syrup", 6.00m)
};

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "catalog-api", count = catalogItems.Length }));
app.MapGet("/catalog/items", () => Results.Ok(catalogItems));
app.MapDefaultEndpoints();

app.Run();

public record CatalogItem(string Id, string Name, decimal Price);

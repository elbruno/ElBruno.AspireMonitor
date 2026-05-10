var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

var orders = new[]
{
    new OrderSummary("order-001", "coffee", 2),
    new OrderSummary("order-002", "tea", 1),
    new OrderSummary("order-003", "syrup", 3)
};

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new { service = "orders-api", count = orders.Length }));
app.MapGet("/orders", () => Results.Ok(orders));
app.MapDefaultEndpoints();

app.Run();

public record OrderSummary(string OrderId, string ItemId, int Quantity);

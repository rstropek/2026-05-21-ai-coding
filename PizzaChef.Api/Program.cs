using PizzaChef.Core.Models;
using PizzaChef.Core.Services;
using PizzaChef.Core.Storage;
using PizzaChef.Core.Time;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<ClockOptions>(builder.Configuration.GetSection("Clock"));
builder.Services.AddSingleton<IClock>(sp =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ClockOptions>>().Value;
    return options.FixedNowUtc is { } fixedTime
        ? new FixedClock(fixedTime)
        : new SystemClock();
});
builder.Services.AddSingleton<IMenuRepository, FileMenuRepository>();
builder.Services.AddSingleton<IOrderRepository, FileOrderRepository>();
builder.Services.AddSingleton<IOrderingService, OrderingService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/api/menu", async (IMenuRepository repo, CancellationToken ct) =>
{
    var menu = await repo.GetMenuAsync(ct);
    return TypedResults.Ok(menu);
})
.WithName("GetMenu")
.WithSummary("Gibt die aktuelle Speisekarte zurück.")
.WithTags("Menu");

app.MapGet("/api/ordering-window", (IOrderingService service) =>
{
    var window = service.GetCurrentWindow();
    return TypedResults.Ok(window);
})
.WithName("GetOrderingWindow")
.WithSummary("Gibt das aktuelle Bestellfenster zurück (Tag, ob offen, Cutoff-Zeit).")
.WithTags("Orders");

app.MapGet("/api/orders/today", async (IOrderingService service, IOrderRepository repo, CancellationToken ct) =>
{
    var window = service.GetCurrentWindow();
    var orders = await repo.GetByDayAsync(window.Day, ct);
    return TypedResults.Ok(orders);
})
.WithName("GetOrdersForToday")
.WithSummary("Listet alle Bestellungen des aktuellen Tages.")
.WithTags("Orders");

app.MapPost("/api/orders", async (NewOrder newOrder, IOrderingService service, CancellationToken ct) =>
{
    try
    {
        var order = await service.PlaceOrderAsync(newOrder, ct);
        return Results.Created($"/api/orders/{order.Id:N}", order);
    }
    catch (OrderingClosedException ex)
    {
        return Results.Problem(
            title: "Bestellung nicht möglich",
            detail: ex.Message,
            statusCode: StatusCodes.Status409Conflict);
    }
    catch (OrderValidationException ex)
    {
        return Results.Problem(
            title: "Ungültige Bestellung",
            detail: ex.Message,
            statusCode: StatusCodes.Status400BadRequest);
    }
})
.WithName("PlaceOrder")
.WithSummary("Erstellt eine neue Bestellung für den aktuellen Tag.")
.WithTags("Orders");

app.MapGet("/", () => Results.Redirect("/openapi/v1.json"))
    .ExcludeFromDescription();

app.Run();

internal sealed partial class Program { }

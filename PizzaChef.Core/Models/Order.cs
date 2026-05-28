namespace PizzaChef.Core.Models;

public sealed record OrderLine(string ItemId, string? VariantId, int Quantity);

public sealed record Order(
    Guid Id,
    DateOnly Day,
    DateTimeOffset CreatedAt,
    string EmployeeName,
    IReadOnlyList<OrderLine> Items,
    string? Notes);

public sealed record NewOrder(
    string EmployeeName,
    IReadOnlyList<OrderLine> Items,
    string? Notes);

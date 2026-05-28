namespace PizzaChef.Core.Models;

public sealed record MenuCategory(string Id, string Name, int SortOrder);

public sealed record MenuItemVariant(string Id, string Name, decimal PriceDelta);

public sealed record MenuItem(
    string Id,
    string CategoryId,
    string Name,
    string? Description,
    decimal Price,
    IReadOnlyList<MenuItemVariant> Variants,
    IReadOnlyList<string> Allergens,
    bool Available);

public sealed record Menu(
    int SchemaVersion,
    DateTimeOffset ScrapedAt,
    string Source,
    string Currency,
    IReadOnlyList<MenuCategory> Categories,
    IReadOnlyList<MenuItem> Items,
    IReadOnlyDictionary<string, string> AllergenLegend);

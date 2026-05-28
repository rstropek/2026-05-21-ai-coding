using Microsoft.Extensions.Options;
using PizzaChef.Core.Models;
using PizzaChef.Core.Storage;

namespace PizzaChef.Core.Tests;

internal static class TestData
{
    public static Menu SampleMenu() => new(
        SchemaVersion: 1,
        ScrapedAt: DateTimeOffset.Parse("2026-05-28T08:00:00+02:00"),
        Source: "test",
        Currency: "EUR",
        Categories: new List<MenuCategory>
        {
            new("pizza", "Pizza", 10),
            new("bibite", "Getränke", 40),
        },
        Items: new List<MenuItem>
        {
            new(
                "pizza-margherita",
                "pizza",
                "Pizza Margherita",
                "Tomate, Mozzarella, Basilikum",
                9.90m,
                new List<MenuItemVariant>
                {
                    new("normal", "Normal", 0m),
                    new("familie", "Familie", 5m),
                },
                new List<string> { "A", "G" },
                true),
            new(
                "coca-cola-033",
                "bibite",
                "Coca-Cola 0,33 l",
                null,
                3.20m,
                new List<MenuItemVariant>(),
                new List<string>(),
                true),
            new(
                "pizza-quattro-stagioni",
                "pizza",
                "Pizza Quattro Stagioni",
                null,
                12.50m,
                new List<MenuItemVariant>(),
                new List<string>(),
                false),
        },
        AllergenLegend: new Dictionary<string, string>
        {
            ["A"] = "Glutenhaltiges Getreide",
            ["G"] = "Milch/Laktose",
        });

    public static IOptions<StorageOptions> Options(string? timeZoneId = null, int cutoffHour = 10) =>
        Microsoft.Extensions.Options.Options.Create(new StorageOptions
        {
            TimeZoneId = timeZoneId ?? "Europe/Vienna",
            OrderCutoffHour = cutoffHour,
        });
}

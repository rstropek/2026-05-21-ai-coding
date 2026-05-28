namespace PizzaChef.Core.Storage;

public sealed class StorageOptions
{
    public string DataDirectory { get; set; } = "data";
    public string MenuFileName { get; set; } = "speisekarte.json";
    public string OrdersDirectoryName { get; set; } = "orders";
    public string TimeZoneId { get; set; } = "Europe/Vienna";
    public int OrderCutoffHour { get; set; } = 10;
}

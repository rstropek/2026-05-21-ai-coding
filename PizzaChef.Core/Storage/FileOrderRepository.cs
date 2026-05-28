using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PizzaChef.Core.Models;

namespace PizzaChef.Core.Storage;

public sealed class FileOrderRepository : IOrderRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    private readonly StorageOptions _options;

    public FileOrderRepository(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        var dir = GetDayDirectory(order.Day);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"{order.Id:N}.json");

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, order, JsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByDayAsync(DateOnly day, CancellationToken cancellationToken = default)
    {
        var dir = GetDayDirectory(day);
        if (!Directory.Exists(dir))
        {
            return Array.Empty<Order>();
        }

        var results = new List<Order>();
        foreach (var file in Directory.EnumerateFiles(dir, "*.json"))
        {
            await using var stream = File.OpenRead(file);
            var order = await JsonSerializer.DeserializeAsync<Order>(stream, JsonOptions, cancellationToken);
            if (order is not null)
            {
                results.Add(order);
            }
        }

        return results
            .OrderBy(o => o.CreatedAt)
            .ToList();
    }

    private string GetDayDirectory(DateOnly day)
    {
        var dataDir = Path.IsPathRooted(_options.DataDirectory)
            ? _options.DataDirectory
            : Path.Combine(AppContext.BaseDirectory, _options.DataDirectory);
        var dayFolder = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return Path.Combine(dataDir, _options.OrdersDirectoryName, dayFolder);
    }
}

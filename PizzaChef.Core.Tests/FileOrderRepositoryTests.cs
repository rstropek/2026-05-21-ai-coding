using Microsoft.Extensions.Options;
using PizzaChef.Core.Models;
using PizzaChef.Core.Storage;

namespace PizzaChef.Core.Tests;

public sealed class FileOrderRepositoryTests : IDisposable
{
    private readonly string _tempDir;

    public FileOrderRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "pizzachef-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    private FileOrderRepository CreateRepo() => new(
        Options.Create(new StorageOptions { DataDirectory = _tempDir, OrdersDirectoryName = "orders" }));

    [Fact]
    public async Task SaveAndLoad_RoundTrips()
    {
        var repo = CreateRepo();
        var day = new DateOnly(2026, 5, 28);
        var order = new Order(
            Id: Guid.NewGuid(),
            Day: day,
            CreatedAt: DateTimeOffset.UtcNow,
            EmployeeName: "Rainer",
            Items:
            [
                new("pizza-margherita", "familie", 2),
                new("coca-cola-033", null, 1),
            ],
            Notes: "Ohne Zwiebeln");

        await repo.SaveAsync(order);
        var loaded = await repo.GetByDayAsync(day);

        Assert.Single(loaded);
        Assert.Equal(order.Id, loaded[0].Id);
        Assert.Equal("Rainer", loaded[0].EmployeeName);
        Assert.Equal(2, loaded[0].Items.Count);
        Assert.Equal("Ohne Zwiebeln", loaded[0].Notes);
    }

    [Fact]
    public async Task GetByDay_NoOrders_ReturnsEmpty()
    {
        var repo = CreateRepo();

        var result = await repo.GetByDayAsync(new DateOnly(2026, 1, 1));

        Assert.Empty(result);
    }

    [Fact]
    public async Task SaveMultiple_OrdersOrderedByCreatedAt()
    {
        var repo = CreateRepo();
        var day = new DateOnly(2026, 5, 28);
        var first = new Order(Guid.NewGuid(), day, DateTimeOffset.UtcNow.AddMinutes(-10), "A",
            [new("pizza-margherita", null, 1)], null);
        var second = new Order(Guid.NewGuid(), day, DateTimeOffset.UtcNow, "B",
            [new("pizza-margherita", null, 1)], null);

        await repo.SaveAsync(second);
        await repo.SaveAsync(first);
        var loaded = await repo.GetByDayAsync(day);

        Assert.Equal(2, loaded.Count);
        Assert.Equal("A", loaded[0].EmployeeName);
        Assert.Equal("B", loaded[1].EmployeeName);
    }
}

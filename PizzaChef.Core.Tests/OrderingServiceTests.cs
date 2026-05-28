using PizzaChef.Core.Models;
using PizzaChef.Core.Services;

namespace PizzaChef.Core.Tests;

public class OrderingServiceTests
{
    private static OrderingService BuildService(DateTimeOffset utcNow, out InMemoryOrderRepository orderRepo)
    {
        orderRepo = new InMemoryOrderRepository();
        return new OrderingService(
            new TestClock(utcNow),
            new StubMenuRepository(TestData.SampleMenu()),
            orderRepo,
            TestData.Options());
    }

    [Fact]
    public void GetCurrentWindow_BeforeCutoff_IsOpen()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero); // 09:00 Vienna
        var service = BuildService(utc, out _);

        var window = service.GetCurrentWindow();

        Assert.True(window.IsOpen);
        Assert.Equal(new DateOnly(2026, 5, 28), window.Day);
    }

    [Fact]
    public void GetCurrentWindow_AtCutoff_IsClosed()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 8, 0, 0, TimeSpan.Zero); // 10:00 Vienna
        var service = BuildService(utc, out _);

        var window = service.GetCurrentWindow();

        Assert.False(window.IsOpen);
    }

    [Fact]
    public void GetCurrentWindow_AfterCutoff_IsClosed()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 12, 0, 0, TimeSpan.Zero); // 14:00 Vienna
        var service = BuildService(utc, out _);

        var window = service.GetCurrentWindow();

        Assert.False(window.IsOpen);
    }

    [Fact]
    public void GetCurrentWindow_AfterMidnightLocal_ReopensForNewDay()
    {
        var utc = new DateTimeOffset(2026, 5, 27, 22, 30, 0, TimeSpan.Zero); // 28.05. 00:30 Vienna
        var service = BuildService(utc, out _);

        var window = service.GetCurrentWindow();

        Assert.True(window.IsOpen);
        Assert.Equal(new DateOnly(2026, 5, 28), window.Day);
    }

    [Fact]
    public async Task PlaceOrder_BeforeCutoff_PersistsOrder()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var service = BuildService(utc, out var repo);

        var newOrder = new NewOrder(
            "Rainer",
            new List<OrderLine>
            {
                new("pizza-margherita", "familie", 2),
                new("coca-cola-033", null, 1),
            },
            "Ohne Zwiebeln");

        var order = await service.PlaceOrderAsync(newOrder);

        Assert.Single(repo.Saved);
        Assert.Equal(new DateOnly(2026, 5, 28), order.Day);
        Assert.Equal("Rainer", order.EmployeeName);
        Assert.Equal(2, order.Items.Count);
        Assert.Equal("Ohne Zwiebeln", order.Notes);
    }

    [Fact]
    public async Task PlaceOrder_AfterCutoff_Throws()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 9, 0, 0, TimeSpan.Zero); // 11:00 Vienna
        var service = BuildService(utc, out var repo);

        var newOrder = new NewOrder(
            "Rainer",
            new List<OrderLine> { new("pizza-margherita", null, 1) },
            null);

        await Assert.ThrowsAsync<OrderingClosedException>(() => service.PlaceOrderAsync(newOrder));
        Assert.Empty(repo.Saved);
    }

    [Fact]
    public async Task PlaceOrder_EmptyName_Throws()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var service = BuildService(utc, out _);

        var newOrder = new NewOrder(
            "  ",
            new List<OrderLine> { new("pizza-margherita", null, 1) },
            null);

        await Assert.ThrowsAsync<OrderValidationException>(() => service.PlaceOrderAsync(newOrder));
    }

    [Fact]
    public async Task PlaceOrder_NoItems_Throws()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var service = BuildService(utc, out _);

        var newOrder = new NewOrder("Rainer", new List<OrderLine>(), null);

        await Assert.ThrowsAsync<OrderValidationException>(() => service.PlaceOrderAsync(newOrder));
    }

    [Fact]
    public async Task PlaceOrder_QuantityZero_Throws()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var service = BuildService(utc, out _);

        var newOrder = new NewOrder(
            "Rainer",
            new List<OrderLine> { new("pizza-margherita", null, 0) },
            null);

        await Assert.ThrowsAsync<OrderValidationException>(() => service.PlaceOrderAsync(newOrder));
    }

    [Fact]
    public async Task PlaceOrder_UnknownItem_Throws()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var service = BuildService(utc, out _);

        var newOrder = new NewOrder(
            "Rainer",
            new List<OrderLine> { new("pizza-mystery", null, 1) },
            null);

        await Assert.ThrowsAsync<OrderValidationException>(() => service.PlaceOrderAsync(newOrder));
    }

    [Fact]
    public async Task PlaceOrder_UnavailableItem_Throws()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var service = BuildService(utc, out _);

        var newOrder = new NewOrder(
            "Rainer",
            new List<OrderLine> { new("pizza-quattro-stagioni", null, 1) },
            null);

        await Assert.ThrowsAsync<OrderValidationException>(() => service.PlaceOrderAsync(newOrder));
    }

    [Fact]
    public async Task PlaceOrder_UnknownVariant_Throws()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var service = BuildService(utc, out _);

        var newOrder = new NewOrder(
            "Rainer",
            new List<OrderLine> { new("pizza-margherita", "xxl", 1) },
            null);

        await Assert.ThrowsAsync<OrderValidationException>(() => service.PlaceOrderAsync(newOrder));
    }
}

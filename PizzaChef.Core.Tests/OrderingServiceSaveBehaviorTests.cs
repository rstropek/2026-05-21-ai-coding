using Moq;
using PizzaChef.Core.Models;
using PizzaChef.Core.Services;
using PizzaChef.Core.Storage;

namespace PizzaChef.Core.Tests;

public class OrderingServiceSaveBehaviorTests
{
    private static (OrderingService Service, Mock<IOrderRepository> OrderRepo) BuildService(DateTimeOffset utcNow)
    {
        var orderRepo = new Mock<IOrderRepository>(MockBehavior.Strict);
        orderRepo
            .Setup(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new OrderingService(
            new TestClock(utcNow),
            new StubMenuRepository(TestData.SampleMenu()),
            orderRepo.Object,
            TestData.Options());

        return (service, orderRepo);
    }

    [Fact]
    public async Task PlaceOrder_Valid_CallsSaveAsyncExactlyOnce()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var (service, orderRepo) = BuildService(utc);

        var newOrder = new NewOrder(
            "Rainer",
            [new("pizza-margherita", "familie", 2)],
            null);

        var order = await service.PlaceOrderAsync(newOrder);

        orderRepo.Verify(
            r => r.SaveAsync(
                It.Is<Order>(o => o.Id == order.Id && o.EmployeeName == "Rainer"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        orderRepo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PlaceOrder_AfterCutoff_DoesNotCallSaveAsync()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 9, 0, 0, TimeSpan.Zero); // 11:00 Vienna
        var (service, orderRepo) = BuildService(utc);

        var newOrder = new NewOrder(
            "Rainer",
            [new("pizza-margherita", null, 1)],
            null);

        await Assert.ThrowsAsync<OrderingClosedException>(() => service.PlaceOrderAsync(newOrder));

        orderRepo.Verify(
            r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PlaceOrder_EmptyName_DoesNotCallSaveAsync()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var (service, orderRepo) = BuildService(utc);

        var newOrder = new NewOrder(
            "  ",
            [new("pizza-margherita", null, 1)],
            null);

        await Assert.ThrowsAsync<OrderValidationException>(() => service.PlaceOrderAsync(newOrder));

        orderRepo.Verify(
            r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PlaceOrder_NoItems_DoesNotCallSaveAsync()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var (service, orderRepo) = BuildService(utc);

        var newOrder = new NewOrder("Rainer", [], null);

        await Assert.ThrowsAsync<OrderValidationException>(() => service.PlaceOrderAsync(newOrder));

        orderRepo.Verify(
            r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PlaceOrder_QuantityZero_DoesNotCallSaveAsync()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var (service, orderRepo) = BuildService(utc);

        var newOrder = new NewOrder(
            "Rainer",
            [new("pizza-margherita", null, 0)],
            null);

        await Assert.ThrowsAsync<OrderValidationException>(() => service.PlaceOrderAsync(newOrder));

        orderRepo.Verify(
            r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PlaceOrder_UnknownItem_DoesNotCallSaveAsync()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var (service, orderRepo) = BuildService(utc);

        var newOrder = new NewOrder(
            "Rainer",
            [new("pizza-mystery", null, 1)],
            null);

        await Assert.ThrowsAsync<OrderValidationException>(() => service.PlaceOrderAsync(newOrder));

        orderRepo.Verify(
            r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PlaceOrder_UnavailableItem_DoesNotCallSaveAsync()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var (service, orderRepo) = BuildService(utc);

        var newOrder = new NewOrder(
            "Rainer",
            [new("pizza-quattro-stagioni", null, 1)],
            null);

        await Assert.ThrowsAsync<OrderValidationException>(() => service.PlaceOrderAsync(newOrder));

        orderRepo.Verify(
            r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PlaceOrder_UnknownVariant_DoesNotCallSaveAsync()
    {
        var utc = new DateTimeOffset(2026, 5, 28, 7, 0, 0, TimeSpan.Zero);
        var (service, orderRepo) = BuildService(utc);

        var newOrder = new NewOrder(
            "Rainer",
            [new("pizza-margherita", "xxl", 1)],
            null);

        await Assert.ThrowsAsync<OrderValidationException>(() => service.PlaceOrderAsync(newOrder));

        orderRepo.Verify(
            r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

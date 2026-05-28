using PizzaChef.Core.Models;

namespace PizzaChef.Core.Services;

public interface IOrderingService
{
    OrderingWindow GetCurrentWindow();

    Task<Order> PlaceOrderAsync(NewOrder newOrder, CancellationToken cancellationToken = default);
}

using PizzaChef.Core.Models;

namespace PizzaChef.Core.Storage;

public interface IOrderRepository
{
    Task SaveAsync(Order order, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetByDayAsync(DateOnly day, CancellationToken cancellationToken = default);
}

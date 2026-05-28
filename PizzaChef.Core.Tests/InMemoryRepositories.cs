using PizzaChef.Core.Models;
using PizzaChef.Core.Storage;

namespace PizzaChef.Core.Tests;

internal sealed class StubMenuRepository : IMenuRepository
{
    private readonly Menu _menu;

    public StubMenuRepository(Menu menu) => _menu = menu;

    public Task<Menu> GetMenuAsync(CancellationToken cancellationToken = default) => Task.FromResult(_menu);
}

internal sealed class InMemoryOrderRepository : IOrderRepository
{
    public List<Order> Saved { get; } = new();

    public Task SaveAsync(Order order, CancellationToken cancellationToken = default)
    {
        Saved.Add(order);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Order>> GetByDayAsync(DateOnly day, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Order> result = Saved.Where(o => o.Day == day).OrderBy(o => o.CreatedAt).ToList();
        return Task.FromResult(result);
    }
}

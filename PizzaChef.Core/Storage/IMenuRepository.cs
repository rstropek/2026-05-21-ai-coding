using PizzaChef.Core.Models;

namespace PizzaChef.Core.Storage;

public interface IMenuRepository
{
    Task<Menu> GetMenuAsync(CancellationToken cancellationToken = default);
}

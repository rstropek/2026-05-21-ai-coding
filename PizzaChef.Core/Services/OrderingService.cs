using Microsoft.Extensions.Options;
using PizzaChef.Core.Models;
using PizzaChef.Core.Storage;
using PizzaChef.Core.Time;

namespace PizzaChef.Core.Services;

public sealed class OrderingClosedException : InvalidOperationException
{
    public OrderingClosedException(string message) : base(message) { }
}

public sealed class OrderValidationException : Exception
{
    public OrderValidationException(string message) : base(message) { }
}

public sealed class OrderingService : IOrderingService
{
    private readonly IClock _clock;
    private readonly IMenuRepository _menuRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly StorageOptions _options;
    private readonly TimeZoneInfo _timeZone;

    public OrderingService(
        IClock clock,
        IMenuRepository menuRepository,
        IOrderRepository orderRepository,
        IOptions<StorageOptions> options)
    {
        _clock = clock;
        _menuRepository = menuRepository;
        _orderRepository = orderRepository;
        _options = options.Value;
        _timeZone = TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZoneId);
    }

    public OrderingWindow GetCurrentWindow()
    {
        var nowLocal = TimeZoneInfo.ConvertTime(_clock.UtcNow, _timeZone);
        var day = DateOnly.FromDateTime(nowLocal.DateTime);
        var cutoff = new DateTimeOffset(day.Year, day.Month, day.Day, _options.OrderCutoffHour, 0, 0, nowLocal.Offset);
        var isOpen = nowLocal < cutoff;
        return new OrderingWindow(day, isOpen, cutoff, nowLocal);
    }

    public async Task<Order> PlaceOrderAsync(NewOrder newOrder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(newOrder);

        var window = GetCurrentWindow();
        if (!window.IsOpen)
        {
            throw new OrderingClosedException(
                $"Bestellungen für {window.Day:yyyy-MM-dd} sind seit {window.CutoffLocal:HH:mm} geschlossen.");
        }

        if (string.IsNullOrWhiteSpace(newOrder.EmployeeName))
        {
            throw new OrderValidationException("Name darf nicht leer sein.");
        }

        if (newOrder.Items is null || newOrder.Items.Count == 0)
        {
            throw new OrderValidationException("Bestellung muss mindestens einen Artikel enthalten.");
        }

        foreach (var line in newOrder.Items)
        {
            if (line.Quantity <= 0)
            {
                throw new OrderValidationException($"Menge für Artikel '{line.ItemId}' muss größer als 0 sein.");
            }
        }

        var menu = await _menuRepository.GetMenuAsync(cancellationToken);
        var itemsById = menu.Items.ToDictionary(i => i.Id);

        foreach (var line in newOrder.Items)
        {
            if (!itemsById.TryGetValue(line.ItemId, out var menuItem))
            {
                throw new OrderValidationException($"Unbekannter Artikel '{line.ItemId}'.");
            }

            if (!menuItem.Available)
            {
                throw new OrderValidationException($"Artikel '{menuItem.Name}' ist nicht verfügbar.");
            }

            if (!string.IsNullOrEmpty(line.VariantId))
            {
                if (menuItem.Variants.All(v => v.Id != line.VariantId))
                {
                    throw new OrderValidationException(
                        $"Unbekannte Variante '{line.VariantId}' für Artikel '{menuItem.Name}'.");
                }
            }
        }

        var order = new Order(
            Id: Guid.NewGuid(),
            Day: window.Day,
            CreatedAt: _clock.UtcNow,
            EmployeeName: newOrder.EmployeeName.Trim(),
            Items: newOrder.Items.ToList(),
            Notes: string.IsNullOrWhiteSpace(newOrder.Notes) ? null : newOrder.Notes.Trim());

        await _orderRepository.SaveAsync(order, cancellationToken);
        return order;
    }
}

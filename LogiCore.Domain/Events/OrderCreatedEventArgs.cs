using LogiCore.Domain.Entities;

namespace LogiCore.Domain.Events;

/// <summary>
/// Аргументы события создания заказа.
/// </summary>
public class OrderCreatedEventArgs : EventArgs
{
    public Order Order { get; }

    public DateTime CreatedAt { get; }

    public OrderCreatedEventArgs(Order order)
    {
        Order = order ?? throw new ArgumentNullException(nameof(order));
        CreatedAt = DateTime.UtcNow;
    }
}

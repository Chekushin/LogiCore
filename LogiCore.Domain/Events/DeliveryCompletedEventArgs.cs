using LogiCore.Domain.Entities;

namespace LogiCore.Domain.Events;

/// <summary>
/// Аргументы события завершения доставки.
/// </summary>
public class DeliveryCompletedEventArgs : EventArgs
{
    public Order Order { get; }

    public Vehicle Vehicle { get; }

    public DateTime CompletedAt { get; }

    public decimal TotalCost { get; }

    public DeliveryCompletedEventArgs(
        Order order,
        Vehicle vehicle,
        decimal totalCost)
    {
        Order = order ?? throw new ArgumentNullException(nameof(order));
        Vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
        TotalCost = totalCost;
        CompletedAt = DateTime.UtcNow;
    }
}

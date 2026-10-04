using LogiCore.Domain.Enums;

namespace LogiCore.Domain.Events;

public class OrderStatusChangedEventArgs : EventArgs
{
    public OrderStatus OldStatus { get; }

    public OrderStatus NewStatus { get; }

    public OrderStatusChangedEventArgs(
        OrderStatus oldStatus,
        OrderStatus newStatus)
    {
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}
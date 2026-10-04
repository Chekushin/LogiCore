using LogiCore.Domain.Entities;
using LogiCore.Domain.Enums;
using LogiCore.Domain.Events;

namespace LogiCore.App.Subscribers;

/// <summary>
/// Подписчик, выводящий уведомления о событиях в консоль.
/// </summary>
public class ConsoleNotifier
{
    /// <summary>
    /// Обработчик события создания заказа.
    /// </summary>
    public void OnOrderCreated(object? sender, OrderCreatedEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[{e.CreatedAt:HH:mm:ss}] ORDER CREATED: {e.Order.Number} for customer {e.Order.Customer.Name}");
        Console.ResetColor();
    }

    /// <summary>
    /// Обработчик события изменения статуса заказа.
    /// </summary>
    public void OnOrderStatusChanged(object? sender, OrderStatusChangedEventArgs e)
    {
        var color = e.NewStatus switch
        {
            OrderStatus.Assigned => ConsoleColor.Cyan,
            OrderStatus.InTransit => ConsoleColor.Yellow,
            OrderStatus.Delivered => ConsoleColor.Green,
            OrderStatus.Cancelled => ConsoleColor.Red,
            _ => ConsoleColor.Gray
        };

        Console.ForegroundColor = color;
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] ORDER STATUS CHANGED: {e.OldStatus} → {e.NewStatus}");
        Console.ResetColor();
    }

    /// <summary>
    /// Обработчик события попытки перегрузки транспорта.
    /// </summary>
    public void OnVehicleOverloadAttempt(object? sender, VehicleOverloadAttemptEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{e.OccurredAt:HH:mm:ss}] OVERLOAD ATTEMPT: Vehicle {e.Vehicle.RegistrationNumber} " +
                         $"cannot carry cargo '{e.Cargo.Description}' ({e.Cargo.WeightKg} kg). " +
                         $"Capacity: {e.VehicleCapacity} kg, Attempted: {e.AttemptedWeight} kg");
        Console.ResetColor();
    }

    /// <summary>
    /// Обработчик события завершения доставки.
    /// </summary>
    public void OnDeliveryCompleted(object? sender, DeliveryCompletedEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[{e.CompletedAt:HH:mm:ss}] DELIVERY COMPLETED: Order {e.Order.Number} " +
                         $"by {e.Vehicle.RegistrationNumber}, Cost: {e.TotalCost:C}");
        Console.ResetColor();
    }

    /// <summary>
    /// Обработчик собственного делегата LogisticsEventHandler.
    /// </summary>
    public void OnLogisticsEvent(object sender, LogisticsEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"[{e.Timestamp:HH:mm:ss}] LOGISTICS EVENT: {e.Message}");
        Console.ResetColor();
    }
}

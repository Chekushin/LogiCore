using LogiCore.Domain.Enums;
using LogiCore.Domain.Events;
using LogiCore.Domain.Exceptions;
using LogiCore.Domain.Interfaces;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Заказ на доставку с машиной состояний.
/// </summary>
public class Order : IEntity
{
    private readonly List<Cargo> _cargo;

    public Guid Id { get; }

    /// <summary>
    /// Номер заказа.
    /// </summary>
    public string Number { get; }

    /// <summary>
    /// Клиент, оформивший заказ.
    /// </summary>
    public Customer Customer { get; }

    /// <summary>
    /// Маршрут доставки.
    /// </summary>
    public Route? Route { get; private set; }

    /// <summary>
    /// Назначенное транспортное средство.
    /// </summary>
    public Vehicle? AssignedVehicle { get; private set; }

    /// <summary>
    /// Список грузов в заказе (только для чтения извне).
    /// </summary>
    public IReadOnlyCollection<Cargo> Cargo => _cargo.AsReadOnly();

    /// <summary>
    /// Итоговая стоимость доставки.
    /// </summary>
    public decimal TotalCost { get; private set; }

    /// <summary>
    /// Текущий статус заказа.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Событие изменения статуса заказа.
    /// </summary>
    public event EventHandler<OrderStatusChangedEventArgs>? StatusChanged;

    public Order(string number, Customer customer)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentException(
                "Order number cannot be empty.",
                nameof(number));
        }

        ArgumentNullException.ThrowIfNull(customer);

        Id = Guid.NewGuid();
        Number = number;
        Customer = customer;
        Status = OrderStatus.Created;
        _cargo = new List<Cargo>();
        TotalCost = 0m;
    }

    /// <summary>
    /// Добавляет груз в заказ.
    /// </summary>
    public void AddCargo(Cargo cargo)
    {
        ArgumentNullException.ThrowIfNull(cargo);

        if (Status != OrderStatus.Created)
        {
            throw new InvalidOrderStateException(
                $"Cannot add cargo to order in status {Status}.");
        }

        _cargo.Add(cargo);
    }

    /// <summary>
    /// Назначает маршрут для заказа.
    /// </summary>
    public void AssignRoute(Route route)
    {
        ArgumentNullException.ThrowIfNull(route);

        if (Status != OrderStatus.Created)
        {
            throw new InvalidOrderStateException(
                $"Cannot assign route to order in status {Status}.");
        }

        Route = route;
    }

    /// <summary>
    /// Назначает транспортное средство и переводит заказ в статус Assigned.
    /// </summary>
    public void Assign(Vehicle vehicle, decimal totalCost)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        if (totalCost < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalCost),
                "Total cost cannot be negative.");
        }

        if (!IsValidTransition(Status, OrderStatus.Assigned))
        {
            throw new InvalidOrderStateException(
                $"Cannot assign order from status {Status}.");
        }

        AssignedVehicle = vehicle;
        TotalCost = totalCost;
        ChangeStatus(OrderStatus.Assigned);
    }

    /// <summary>
    /// Запускает доставку, переводя заказ в статус InTransit.
    /// </summary>
    public void StartDelivery()
    {
        if (!IsValidTransition(Status, OrderStatus.InTransit))
        {
            throw new InvalidOrderStateException(
                $"Cannot start delivery from status {Status}.");
        }

        if (AssignedVehicle == null)
        {
            throw new InvalidOperationException(
                "Cannot start delivery without assigned vehicle.");
        }

        ChangeStatus(OrderStatus.InTransit);
    }

    /// <summary>
    /// Завершает доставку, переводя заказ в статус Delivered.
    /// </summary>
    public void Complete()
    {
        if (!IsValidTransition(Status, OrderStatus.Delivered))
        {
            throw new InvalidOrderStateException(
                $"Cannot complete order from status {Status}.");
        }

        ChangeStatus(OrderStatus.Delivered);
    }

    /// <summary>
    /// Отменяет заказ, переводя его в статус Cancelled.
    /// </summary>
    public void Cancel()
    {
        if (!IsValidTransition(Status, OrderStatus.Cancelled))
        {
            throw new InvalidOrderStateException(
                $"Cannot cancel order from status {Status}.");
        }

        ChangeStatus(OrderStatus.Cancelled);
    }

    private void ChangeStatus(OrderStatus newStatus)
    {
        if (!IsValidTransition(Status, newStatus))
        {
            throw new InvalidOrderStateException(
                $"Invalid order status transition: {Status} -> {newStatus}.");
        }

        var oldStatus = Status;
        Status = newStatus;

        StatusChanged?.Invoke(
            this,
            new OrderStatusChangedEventArgs(oldStatus, newStatus));
    }

    private static bool IsValidTransition(
        OrderStatus current,
        OrderStatus next)
    {
        return current switch
        {
            OrderStatus.Created =>
                next is OrderStatus.Assigned or OrderStatus.Cancelled,

            OrderStatus.Assigned =>
                next is OrderStatus.InTransit or OrderStatus.Cancelled,

            OrderStatus.InTransit =>
                next is OrderStatus.Delivered,

            OrderStatus.Delivered => false,

            OrderStatus.Cancelled => false,

            _ => false
        };
    }

    public override string ToString()
    {
        return $"Order {Number}: {Status}, Customer: {Customer.Name}, Items: {_cargo.Count}, Cost: {TotalCost:C}";
    }
}

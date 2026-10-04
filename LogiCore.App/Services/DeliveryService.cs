using LogiCore.Domain.Entities;
using LogiCore.Domain.Enums;
using LogiCore.Domain.Events;
using LogiCore.Domain.Exceptions;

namespace LogiCore.App.Services;

/// <summary>
/// Центральный сервис управления доставками.
/// Выполняет функции диспетчера: подбор транспорта, расчёт стоимости, 
/// управление жизненным циклом заказов, публикация событий.
/// </summary>
public class DeliveryService
{
    private readonly CargoCompatibilityValidator _compatibilityValidator;
    private decimal _totalRevenue;

    /// <summary>
    /// Общая выручка компании.
    /// </summary>
    public decimal TotalRevenue => _totalRevenue;

    // События
    public event EventHandler<OrderCreatedEventArgs>? OrderCreated;
    public event EventHandler<VehicleOverloadAttemptEventArgs>? VehicleOverloadAttempt;
    public event EventHandler<DeliveryCompletedEventArgs>? DeliveryCompleted;

    // Собственный делегат
    public event LogisticsEventHandler? LogisticsEvent;

    public DeliveryService(CargoCompatibilityValidator compatibilityValidator)
    {
        _compatibilityValidator = compatibilityValidator ?? 
            throw new ArgumentNullException(nameof(compatibilityValidator));
        _totalRevenue = 0m;
    }

    /// <summary>
    /// Создаёт новый заказ и публикует событие OrderCreated.
    /// </summary>
    public Order CreateOrder(string orderNumber, Customer customer, Route route, IEnumerable<Cargo> cargoItems)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(cargoItems);

        var order = new Order(orderNumber, customer);
        order.AssignRoute(route);

        foreach (var cargo in cargoItems)
        {
            order.AddCargo(cargo);
        }

        customer.AddOrder(order);

        // Публикуем событие создания заказа
        OrderCreated?.Invoke(this, new OrderCreatedEventArgs(order));

        // Публикуем через собственный делегат
        LogisticsEvent?.Invoke(this, new LogisticsEventArgs($"Order {orderNumber} created"));

        return order;
    }

    /// <summary>
    /// Подбирает подходящее транспортное средство для заказа.
    /// Возвращает транспорт с минимальной стоимостью доставки среди подходящих.
    /// </summary>
    public Vehicle? SelectVehicle(Order order, IEnumerable<Vehicle> availableVehicles)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(availableVehicles);

        if (order.Route == null)
        {
            throw new InvalidOperationException("Order must have a route assigned.");
        }

        var cargoList = order.Cargo.ToList();
        if (cargoList.Count == 0)
        {
            throw new InvalidOperationException("Order must contain at least one cargo item.");
        }

        Vehicle? bestVehicle = null;
        decimal bestCost = decimal.MaxValue;

        foreach (var vehicle in availableVehicles)
        {
            // Проверяем, свободно ли транспортное средство
            if (vehicle.State != VehicleState.Free)
            {
                continue;
            }

            // Проверяем совместимость грузов с транспортом
            try
            {
                _compatibilityValidator.ValidateCargoCompatibility(cargoList, vehicle);
            }
            catch (LogisticsException)
            {
                // Этот транспорт не подходит, пробуем следующий
                continue;
            }

            // Проверяем, может ли транспорт перевезти каждый груз
            bool canCarryAll = true;
            foreach (var cargo in cargoList)
            {
                if (!vehicle.CanCarry(cargo))
                {
                    canCarryAll = false;
                    break;
                }
            }

            if (!canCarryAll)
            {
                continue;
            }

            // Дополнительная проверка для дронов
            if (vehicle is DroneCourier drone && !drone.CanReach(order.Route))
            {
                continue;
            }

            // Рассчитываем стоимость доставки
            decimal cost = vehicle.CalculateDeliveryCost(order.Route, cargoList);

            // Выбираем самый дешёвый вариант
            if (cost < bestCost)
            {
                bestCost = cost;
                bestVehicle = vehicle;
            }
        }

        return bestVehicle;
    }

    /// <summary>
    /// Назначает транспортное средство заказу и переводит заказ в статус Assigned.
    /// </summary>
    public void AssignVehicle(Order order, Vehicle vehicle, decimal totalCost)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(vehicle);

        if (vehicle.State != VehicleState.Free)
        {
            throw new InvalidOperationException(
                $"Vehicle {vehicle.RegistrationNumber} is not available.");
        }

        // Проверяем совместимость грузов
        _compatibilityValidator.ValidateCargoCompatibility(order.Cargo.ToList(), vehicle);

        // Пытаемся загрузить грузы
        foreach (var cargo in order.Cargo)
        {
            try
            {
                if (!vehicle.CanCarry(cargo))
                {
                    // Публикуем событие попытки перегрузки
                    VehicleOverloadAttempt?.Invoke(this, 
                        new VehicleOverloadAttemptEventArgs(
                            vehicle, 
                            cargo, 
                            vehicle.CurrentLoad + cargo.WeightKg, 
                            vehicle.MaxLoadKg));

                    throw new VehicleOverloadException(
                        $"Vehicle {vehicle.RegistrationNumber} cannot carry cargo '{cargo.Description}'.");
                }

                vehicle.LoadCargo(cargo);
            }
            catch (VehicleOverloadException)
            {
                // Публикуем событие попытки перегрузки
                VehicleOverloadAttempt?.Invoke(this, 
                    new VehicleOverloadAttemptEventArgs(
                        vehicle, 
                        cargo, 
                        vehicle.CurrentLoad + cargo.WeightKg, 
                        vehicle.MaxLoadKg));

                // Перебрасываем исключение дальше
                throw;
            }
        }

        // Назначаем транспорт заказу
        order.Assign(vehicle, totalCost);
        vehicle.SetState(VehicleState.InTransit);

        LogisticsEvent?.Invoke(this, 
            new LogisticsEventArgs($"Vehicle {vehicle.RegistrationNumber} assigned to order {order.Number}"));
    }

    /// <summary>
    /// Запускает доставку, переводя заказ в статус InTransit.
    /// </summary>
    public void StartDelivery(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        order.StartDelivery();

        LogisticsEvent?.Invoke(this, 
            new LogisticsEventArgs($"Delivery started for order {order.Number}"));
    }

    /// <summary>
    /// Завершает доставку, переводя заказ в статус Delivered.
    /// Обновляет выручку компании и публикует событие DeliveryCompleted.
    /// </summary>
    public void CompleteDelivery(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (order.AssignedVehicle == null)
        {
            throw new InvalidOperationException(
                "Cannot complete delivery without assigned vehicle.");
        }

        var vehicle = order.AssignedVehicle;

        // Завершаем заказ
        order.Complete();

        // Освобождаем транспорт
        vehicle.SetState(VehicleState.Free);
        foreach (var cargo in order.Cargo)
        {
            vehicle.UnloadCargo(cargo);
        }

        // Увеличиваем выручку
        _totalRevenue += order.TotalCost;

        // Публикуем событие завершения доставки
        DeliveryCompleted?.Invoke(this, 
            new DeliveryCompletedEventArgs(order, vehicle, order.TotalCost));

        LogisticsEvent?.Invoke(this, 
            new LogisticsEventArgs($"Delivery completed for order {order.Number}, revenue: {order.TotalCost:C}"));
    }

    /// <summary>
    /// Отменяет заказ.
    /// </summary>
    public void CancelOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        // Освобождаем транспорт, если он был назначен
        if (order.AssignedVehicle != null && order.Status == OrderStatus.Assigned)
        {
            var vehicle = order.AssignedVehicle;
            vehicle.SetState(VehicleState.Free);
            
            foreach (var cargo in order.Cargo)
            {
                vehicle.UnloadCargo(cargo);
            }
        }

        order.Cancel();

        LogisticsEvent?.Invoke(this, 
            new LogisticsEventArgs($"Order {order.Number} cancelled"));
    }
}

using LogiCore.Domain.Entities;
using LogiCore.Domain.Enums;
using System.Text;

namespace LogiCore.App.Services;

/// <summary>
/// Сервис для генерации аналитических отчётов с использованием LINQ.
/// Демонстрирует применение различных LINQ-операторов: Where, Select, OrderBy, GroupBy, Join, агрегаты.
/// </summary>
public class ReportService
{
    /// <summary>
    /// Отчёт 1: Топ-3 транспортных средств по принесённой выручке.
    /// Использует: OrderByDescending, Take, Sum.
    /// </summary>
    public string GetTopVehiclesByRevenue(IEnumerable<Order> orders)
    {
        var vehicleRevenue = orders
            .Where(o => o.Status == OrderStatus.Delivered && o.AssignedVehicle != null)
            .GroupBy(o => o.AssignedVehicle!)
            .Select(g => new
            {
                Vehicle = g.Key,
                TotalRevenue = g.Sum(o => o.TotalCost),
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(3)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("=== TOP-3 VEHICLES BY REVENUE ===");
        
        int rank = 1;
        foreach (var item in vehicleRevenue)
        {
            sb.AppendLine($"{rank}. {item.Vehicle.RegistrationNumber} ({item.Vehicle.Type}): " +
                         $"{item.TotalRevenue:C}, Orders: {item.OrderCount}");
            rank++;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Отчёт 2: Заказы по статусам с количеством и суммарной стоимостью.
    /// Использует: GroupBy, Count, Sum, OrderBy.
    /// </summary>
    public string GetOrdersByStatus(IEnumerable<Order> orders)
    {
        var statusGroups = orders
            .GroupBy(o => o.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count(),
                TotalCost = g.Sum(o => o.TotalCost)
            })
            .OrderBy(x => x.Status)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("=== ORDERS BY STATUS ===");
        
        foreach (var group in statusGroups)
        {
            sb.AppendLine($"{group.Status}: {group.Count} orders, Total: {group.TotalCost:C}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Отчёт 3: Средняя загрузка транспорта по типам (в % от MaxLoadKg).
    /// Использует: GroupBy, Average, Where.
    /// </summary>
    public string GetAverageLoadByVehicleType(IEnumerable<Vehicle> vehicles)
    {
        var loadByType = vehicles
            .Where(v => v.MaxLoadKg > 0)
            .GroupBy(v => v.Type)
            .Select(g => new
            {
                Type = g.Key,
                AverageLoadPercent = g.Average(v => (v.CurrentLoad / v.MaxLoadKg) * 100),
                VehicleCount = g.Count()
            })
            .OrderBy(x => x.Type)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("=== AVERAGE LOAD BY VEHICLE TYPE ===");
        
        foreach (var item in loadByType)
        {
            sb.AppendLine($"{item.Type}: {item.AverageLoadPercent:F2}% avg load, {item.VehicleCount} vehicles");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Отчёт 4: Клиенты с суммой заказов выше порога.
    /// Использует: Where, Sum, OrderByDescending.
    /// </summary>
    public string GetCustomersAboveThreshold(IEnumerable<Customer> customers, decimal threshold)
    {
        var highValueCustomers = customers
            .Select(c => new
            {
                Customer = c,
                TotalSpent = c.Orders.Sum(o => o.TotalCost),
                OrderCount = c.Orders.Count
            })
            .Where(x => x.TotalSpent > threshold)
            .OrderByDescending(x => x.TotalSpent)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"=== CUSTOMERS WITH ORDERS ABOVE {threshold:C} ===");
        
        foreach (var item in highValueCustomers)
        {
            sb.AppendLine($"{item.Customer.Name}: {item.TotalSpent:C} ({item.OrderCount} orders)");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Отчёт 5: Join Cargo → Order → Customer.
    /// Использует: Join (два раза), Select.
    /// Демонстрация query syntax (from ... select).
    /// </summary>
    public string GetCargoToCustomerReport(
        IEnumerable<Order> orders,
        IEnumerable<Customer> customers)
    {
        // Query syntax для демонстрации
        var cargoDetails = 
            from order in orders
            where order.Status == OrderStatus.Delivered
            from cargo in order.Cargo
            select new
            {
                CustomerName = order.Customer.Name,
                OrderNumber = order.Number,
                CargoDescription = cargo.Description,
                CargoWeight = cargo.WeightKg,
                CargoValue = cargo.DeclaredValue
            };

        var sb = new StringBuilder();
        sb.AppendLine("=== CARGO TO CUSTOMER REPORT (Delivered Orders) ===");
        
        foreach (var item in cargoDetails.Take(20)) // Ограничиваем для читаемости
        {
            sb.AppendLine($"Customer: {item.CustomerName}, Order: {item.OrderNumber}, " +
                         $"Cargo: {item.CargoDescription} ({item.CargoWeight} kg, Value: {item.CargoValue:C})");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Отчёт 6: Словарь "класс опасности → количество грузов".
    /// Использует: OfType, GroupBy, ToDictionary.
    /// </summary>
    public string GetDangerousCargoByClass(IEnumerable<Order> orders)
    {
        var dangerousCargoDict = orders
            .SelectMany(o => o.Cargo)
            .OfType<DangerousCargo>()
            .GroupBy(c => c.DangerousClass)
            .ToDictionary(
                g => g.Key,
                g => g.Count());

        var sb = new StringBuilder();
        sb.AppendLine("=== DANGEROUS CARGO BY CLASS ===");
        
        if (dangerousCargoDict.Count == 0)
        {
            sb.AppendLine("No dangerous cargo found.");
        }
        else
        {
            foreach (var kvp in dangerousCargoDict.OrderBy(x => x.Key))
            {
                sb.AppendLine($"Class {(int)kvp.Key}: {kvp.Value} items");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Отчёт 7 (бонус): Статистика по типам грузов.
    /// Использует: GroupBy, Count, Average, ToLookup.
    /// </summary>
    public string GetCargoTypeStatistics(IEnumerable<Order> orders)
    {
        var cargoLookup = orders
            .SelectMany(o => o.Cargo)
            .ToLookup(c => c.GetType().Name);

        var sb = new StringBuilder();
        sb.AppendLine("=== CARGO TYPE STATISTICS ===");
        
        foreach (var group in cargoLookup)
        {
            var avgWeight = group.Average(c => c.WeightKg);
            var avgValue = group.Average(c => c.DeclaredValue);
            
            sb.AppendLine($"{group.Key}: {group.Count()} items, " +
                         $"Avg Weight: {avgWeight:F2} kg, Avg Value: {avgValue:C}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Сводный отчёт со всей статистикой.
    /// </summary>
    public string GenerateFullReport(
        IEnumerable<Order> orders,
        IEnumerable<Vehicle> vehicles,
        IEnumerable<Customer> customers,
        decimal customerThreshold = 10000m)
    {
        var sb = new StringBuilder();
        sb.AppendLine("╔════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║           LOGICORE LOGISTICS SYSTEM REPORT                     ║");
        sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");
        sb.AppendLine();

        sb.AppendLine(GetTopVehiclesByRevenue(orders));
        sb.AppendLine();

        sb.AppendLine(GetOrdersByStatus(orders));
        sb.AppendLine();

        sb.AppendLine(GetAverageLoadByVehicleType(vehicles));
        sb.AppendLine();

        sb.AppendLine(GetCustomersAboveThreshold(customers, customerThreshold));
        sb.AppendLine();

        sb.AppendLine(GetCargoToCustomerReport(orders, customers));
        sb.AppendLine();

        sb.AppendLine(GetDangerousCargoByClass(orders));
        sb.AppendLine();

        sb.AppendLine(GetCargoTypeStatistics(orders));
        sb.AppendLine();

        return sb.ToString();
    }
}

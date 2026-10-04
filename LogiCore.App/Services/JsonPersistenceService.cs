using System.Text.Json;
using LogiCore.Domain.Entities;
using LogiCore.Domain.Enums;

namespace LogiCore.App.Services;

/// <summary>
/// Сервис для сериализации и десериализации состояния системы в JSON.
/// Использует System.Text.Json для сохранения парка транспорта, клиентов и заказов.
/// </summary>
public class JsonPersistenceService
{
    private readonly JsonSerializerOptions _options;

    public JsonPersistenceService()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Сохраняет состояние системы в JSON-файл.
    /// </summary>
    public void SaveState(
        string filePath,
        IEnumerable<Vehicle> vehicles,
        IEnumerable<Customer> customers,
        IEnumerable<Order> orders)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(vehicles);
        ArgumentNullException.ThrowIfNull(customers);
        ArgumentNullException.ThrowIfNull(orders);

        try
        {
            var snapshot = new SystemSnapshot
            {
                Vehicles = vehicles.Select(v => VehicleDto.FromEntity(v)).ToList(),
                Customers = customers.Select(c => CustomerDto.FromEntity(c)).ToList(),
                Orders = orders.Select(o => OrderDto.FromEntity(o)).ToList(),
                SavedAt = DateTime.UtcNow
            };

            string json = JsonSerializer.Serialize(snapshot, _options);

            // Используем using для автоматического закрытия файла
            using (var writer = new StreamWriter(filePath, false))
            {
                writer.Write(json);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"Failed to save state to file '{filePath}': {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Загружает состояние системы из JSON-файла.
    /// </summary>
    public SystemSnapshot LoadState(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"State file not found: {filePath}");
        }

        try
        {
            string json;
            
            // Используем using для автоматического закрытия файла
            using (var reader = new StreamReader(filePath))
            {
                json = reader.ReadToEnd();
            }

            var snapshot = JsonSerializer.Deserialize<SystemSnapshot>(json, _options);

            if (snapshot == null)
            {
                throw new InvalidOperationException("Failed to deserialize state: result is null.");
            }

            return snapshot;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse JSON from file '{filePath}': {ex.Message}",
                ex);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"Failed to load state from file '{filePath}': {ex.Message}",
                ex);
        }
    }
}

/// <summary>
/// Снимок состояния системы для сериализации.
/// </summary>
public class SystemSnapshot
{
    public List<VehicleDto> Vehicles { get; set; } = new();
    public List<CustomerDto> Customers { get; set; } = new();
    public List<OrderDto> Orders { get; set; } = new();
    public DateTime SavedAt { get; set; }
}

/// <summary>
/// DTO для сериализации Vehicle.
/// Упрощённая модель без полиморфизма для базового варианта T9.
/// </summary>
public class VehicleDto
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public decimal MaxLoadKg { get; set; }
    public decimal MaxVolumeM3 { get; set; }
    public decimal AverageSpeedKmH { get; set; }
    public decimal BaseRatePerKm { get; set; }
    public decimal CurrentLoad { get; set; }

    public static VehicleDto FromEntity(Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            RegistrationNumber = vehicle.RegistrationNumber,
            Type = vehicle.Type.ToString(),
            State = vehicle.State.ToString(),
            MaxLoadKg = vehicle.MaxLoadKg,
            MaxVolumeM3 = vehicle.MaxVolumeM3,
            AverageSpeedKmH = vehicle.AverageSpeedKmH,
            BaseRatePerKm = vehicle.BaseRatePerKm,
            CurrentLoad = vehicle.CurrentLoad
        };
    }
}

/// <summary>
/// DTO для сериализации Customer.
/// </summary>
public class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public int OrderCount { get; set; }

    public static CustomerDto FromEntity(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Contact = customer.Contact,
            OrderCount = customer.Orders.Count
        };
    }
}

/// <summary>
/// DTO для сериализации Order.
/// </summary>
public class OrderDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public int CargoCount { get; set; }
    public string? AssignedVehicleRegNumber { get; set; }

    public static OrderDto FromEntity(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            Number = order.Number,
            CustomerId = order.Customer.Id,
            CustomerName = order.Customer.Name,
            Status = order.Status.ToString(),
            TotalCost = order.TotalCost,
            CargoCount = order.Cargo.Count,
            AssignedVehicleRegNumber = order.AssignedVehicle?.RegistrationNumber
        };
    }
}

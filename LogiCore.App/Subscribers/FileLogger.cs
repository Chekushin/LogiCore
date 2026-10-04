using LogiCore.Domain.Events;

namespace LogiCore.App.Subscribers;

/// <summary>
/// Подписчик, записывающий события в файл журнала.
/// Демонстрирует использование using для автоматического освобождения ресурсов.
/// </summary>
public class FileLogger : IDisposable
{
    private readonly string _logFilePath;
    private StreamWriter? _writer;
    private bool _disposed;

    public FileLogger(string logFilePath)
    {
        if (string.IsNullOrWhiteSpace(logFilePath))
        {
            throw new ArgumentException(
                "Log file path cannot be empty.",
                nameof(logFilePath));
        }

        _logFilePath = logFilePath;
        InitializeWriter();
    }

    private void InitializeWriter()
    {
        try
        {
            // Открываем файл для добавления записей
            _writer = new StreamWriter(_logFilePath, append: true);
            _writer.AutoFlush = true; // Автоматическая запись в файл
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize log file: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Обработчик события создания заказа.
    /// </summary>
    public void OnOrderCreated(object? sender, OrderCreatedEventArgs e)
    {
        WriteLog($"ORDER CREATED: {e.Order.Number} for customer {e.Order.Customer.Name} at {e.CreatedAt:yyyy-MM-dd HH:mm:ss}");
    }

    /// <summary>
    /// Обработчик события изменения статуса заказа.
    /// </summary>
    public void OnOrderStatusChanged(object? sender, OrderStatusChangedEventArgs e)
    {
        WriteLog($"ORDER STATUS CHANGED: {e.OldStatus} → {e.NewStatus}");
    }

    /// <summary>
    /// Обработчик события попытки перегрузки транспорта.
    /// </summary>
    public void OnVehicleOverloadAttempt(object? sender, VehicleOverloadAttemptEventArgs e)
    {
        WriteLog($"OVERLOAD ATTEMPT: Vehicle {e.Vehicle.RegistrationNumber} " +
                $"cannot carry cargo '{e.Cargo.Description}' ({e.Cargo.WeightKg} kg). " +
                $"Capacity: {e.VehicleCapacity} kg at {e.OccurredAt:yyyy-MM-dd HH:mm:ss}");
    }

    /// <summary>
    /// Обработчик события завершения доставки.
    /// </summary>
    public void OnDeliveryCompleted(object? sender, DeliveryCompletedEventArgs e)
    {
        WriteLog($"DELIVERY COMPLETED: Order {e.Order.Number} " +
                $"by {e.Vehicle.RegistrationNumber}, Cost: {e.TotalCost:C}, " +
                $"Completed at {e.CompletedAt:yyyy-MM-dd HH:mm:ss}");
    }

    /// <summary>
    /// Обработчик собственного делегата LogisticsEventHandler.
    /// </summary>
    public void OnLogisticsEvent(object sender, LogisticsEventArgs e)
    {
        WriteLog($"LOGISTICS EVENT: {e.Message} at {e.Timestamp:yyyy-MM-dd HH:mm:ss}");
    }

    private void WriteLog(string message)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(FileLogger));
        }

        try
        {
            _writer?.WriteLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write to log file: {ex.Message}");
        }
    }

    /// <summary>
    /// Освобождает ресурсы, используемые FileLogger.
    /// Демонстрирует паттерн Dispose.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Освобождаем управляемые ресурсы
            _writer?.Dispose();
            _writer = null;
        }

        _disposed = true;
    }

    ~FileLogger()
    {
        Dispose(false);
    }
}

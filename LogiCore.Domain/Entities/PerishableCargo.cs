using LogiCore.Domain.Interfaces;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Скоропортящийся груз с требованиями по температуре и сроку годности.
/// </summary>
public class PerishableCargo : Cargo, ITemperatureSensitive, IInsurable
{
    /// <summary>
    /// Срок годности.
    /// </summary>
    public DateTime ExpirationDate { get; }

    /// <summary>
    /// Требуемая минимальная температура хранения.
    /// </summary>
    public decimal RequiredMinTemperatureC { get; }

    /// <summary>
    /// Требуемая максимальная температура хранения.
    /// </summary>
    public decimal RequiredMaxTemperatureC { get; }

    public PerishableCargo(
        string description,
        decimal weightKg,
        decimal volumeM3,
        decimal declaredValue,
        DateTime expirationDate,
        decimal requiredMinTemperatureC,
        decimal requiredMaxTemperatureC)
        : base(description, weightKg, volumeM3, declaredValue)
    {
        if (requiredMinTemperatureC > requiredMaxTemperatureC)
        {
            throw new ArgumentException(
                "Min temperature cannot be greater than max temperature.");
        }

        ExpirationDate = expirationDate;
        RequiredMinTemperatureC = requiredMinTemperatureC;
        RequiredMaxTemperatureC = requiredMaxTemperatureC;
    }

    /// <summary>
    /// Проверяет, не истёк ли срок годности груза.
    /// </summary>
    public bool IsExpired()
    {
        return DateTime.UtcNow > ExpirationDate;
    }

    // Явная реализация интерфейса ITemperatureSensitive.
    // Используется explicit implementation, чтобы избежать конфликта имён
    // с собственными свойствами класса RequiredMinTemperatureC и RequiredMaxTemperatureC.
    decimal ITemperatureSensitive.MinTemperature => RequiredMinTemperatureC;

    decimal ITemperatureSensitive.MaxTemperature => RequiredMaxTemperatureC;

    bool ITemperatureSensitive.IsTemperatureAllowed(decimal temperature)
    {
        return temperature >= RequiredMinTemperatureC &&
               temperature <= RequiredMaxTemperatureC;
    }

    // Явная реализация интерфейса IInsurable.
    // Используется explicit implementation для демонстрации паттерна,
    // когда интерфейсный контракт отделён от публичного API класса.
    decimal IInsurable.InsuranceValue => DeclaredValue;

    decimal IInsurable.CalculateInsuranceCost()
    {
        // Страховка для скоропортящихся грузов — 5% от объявленной стоимости
        return DeclaredValue * 0.05m;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, Expires: {ExpirationDate:yyyy-MM-dd}, " +
               $"Temp: {RequiredMinTemperatureC}°C to {RequiredMaxTemperatureC}°C";
    }
}

using LogiCore.Domain.Interfaces;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Абстрактный базовый класс для всех типов грузов.
/// </summary>
public abstract class Cargo : IEntity
{
    public Guid Id { get; }

    /// <summary>
    /// Описание груза.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Вес в килограммах.
    /// </summary>
    public decimal WeightKg { get; }

    /// <summary>
    /// Объём в кубических метрах.
    /// </summary>
    public decimal VolumeM3 { get; }

    /// <summary>
    /// Объявленная стоимость для страхования.
    /// </summary>
    public decimal DeclaredValue { get; }

    protected Cargo(
        string description,
        decimal weightKg,
        decimal volumeM3,
        decimal declaredValue)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Cargo description cannot be empty.",
                nameof(description));
        }

        if (weightKg <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(weightKg),
                "Cargo weight must be greater than zero.");
        }

        if (volumeM3 <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(volumeM3),
                "Cargo volume must be greater than zero.");
        }

        if (declaredValue < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(declaredValue),
                "Declared value cannot be negative.");
        }

        Id = Guid.NewGuid();
        Description = description;
        WeightKg = weightKg;
        VolumeM3 = volumeM3;
        DeclaredValue = declaredValue;
    }

    public override string ToString()
    {
        return $"{GetType().Name}: {Description}, {WeightKg} kg, {VolumeM3} m³";
    }
}

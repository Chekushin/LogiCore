using LogiCore.Domain.Enums;
using LogiCore.Domain.Exceptions;
using LogiCore.Domain.Interfaces;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Абстрактный базовый класс для всех транспортных средств.
/// </summary>
public abstract class Vehicle : IEntity
{
    public Guid Id { get; }

    /// <summary>
    /// Регистрационный номер транспортного средства.
    /// </summary>
    public string RegistrationNumber { get; }

    public VehicleType Type { get; }

    /// <summary>
    /// Состояние транспортного средства.
    /// </summary>
    public VehicleState State { get; protected set; }

    /// <summary>
    /// Максимальная грузоподъёмность в килограммах.
    /// </summary>
    public decimal MaxLoadKg { get; }

    /// <summary>
    /// Максимальный объём в кубических метрах.
    /// </summary>
    public decimal MaxVolumeM3 { get; }

    /// <summary>
    /// Средняя скорость в км/ч.
    /// </summary>
    public decimal AverageSpeedKmH { get; }

    /// <summary>
    /// Базовая ставка за километр.
    /// </summary>
    public decimal BaseRatePerKm { get; }

    /// <summary>
    /// Текущая загрузка в килограммах.
    /// </summary>
    public decimal CurrentLoad { get; protected set; }

    [Obsolete("Используйте MaxLoadKg вместо Capacity")]
    public decimal Capacity => MaxLoadKg;

    [Obsolete("Используйте State == VehicleState.Free вместо IsAvailable")]
    public bool IsAvailable => State == VehicleState.Free;

    protected Vehicle(
        string registrationNumber,
        VehicleType type,
        decimal maxLoadKg,
        decimal maxVolumeM3,
        decimal averageSpeedKmH,
        decimal baseRatePerKm)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
        {
            throw new InvalidVehicleException(
                "Registration number cannot be empty.");
        }

        if (maxLoadKg <= 0)
        {
            throw new InvalidVehicleException(
                "Max load must be greater than zero.");
        }

        if (maxVolumeM3 <= 0)
        {
            throw new InvalidVehicleException(
                "Max volume must be greater than zero.");
        }

        if (averageSpeedKmH <= 0)
        {
            throw new InvalidVehicleException(
                "Average speed must be greater than zero.");
        }

        if (baseRatePerKm < 0)
        {
            throw new InvalidVehicleException(
                "Base rate cannot be negative.");
        }

        Id = Guid.NewGuid();
        RegistrationNumber = registrationNumber;
        Type = type;
        MaxLoadKg = maxLoadKg;
        MaxVolumeM3 = maxVolumeM3;
        AverageSpeedKmH = averageSpeedKmH;
        BaseRatePerKm = baseRatePerKm;
        CurrentLoad = 0;
        State = VehicleState.Free;
    }

    /// <summary>
    /// Абстрактный метод расчёта стоимости доставки. 
    /// Каждый тип транспорта реализует свою формулу.
    /// </summary>
    public abstract decimal CalculateDeliveryCost(
        Route route,
        IReadOnlyCollection<Cargo> cargo);

    /// <summary>
    /// Виртуальный метод проверки возможности перевозки груза.
    /// Базовая проверка веса и объёма. Наследники расширяют через base.CanCarry().
    /// </summary>
    public virtual bool CanCarry(Cargo cargo)
    {
        ArgumentNullException.ThrowIfNull(cargo);

        if (cargo.WeightKg <= 0 || cargo.VolumeM3 <= 0)
        {
            return false;
        }

        if (CurrentLoad + cargo.WeightKg > MaxLoadKg)
        {
            return false;
        }

        return true;
    }

    public void LoadCargo(Cargo cargo)
    {
        ArgumentNullException.ThrowIfNull(cargo);

        if (cargo.WeightKg <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cargo),
                "Cargo weight must be greater than zero.");
        }

        if (cargo.VolumeM3 <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cargo),
                "Cargo volume must be greater than zero.");
        }

        if (CurrentLoad + cargo.WeightKg > MaxLoadKg)
        {
            throw new VehicleOverloadException(
                $"Vehicle {RegistrationNumber} cannot carry cargo " +
                $"with weight {cargo.WeightKg}. " +
                $"Current load: {CurrentLoad}, capacity: {MaxLoadKg}.");
        }

        if (!CanCarry(cargo))
        {
            throw new InvalidOperationException(
                $"Vehicle {RegistrationNumber} cannot carry cargo " +
                $"'{cargo.Description}'.");
        }

        CurrentLoad += cargo.WeightKg;
    }

    public void UnloadCargo(Cargo cargo)
    {
        ArgumentNullException.ThrowIfNull(cargo);

        if (cargo.WeightKg <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cargo),
                "Cargo weight must be greater than zero.");
        }

        CurrentLoad = Math.Max(0, CurrentLoad - cargo.WeightKg);
    }

    public void SetState(VehicleState newState)
    {
        State = newState;
    }

    /// <summary>
    /// Сравнение транспортных средств по Id.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Vehicle other)
        {
            return false;
        }

        return Id == other.Id;
    }

    /// <summary>
    /// Хэш-код на основе Id.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Type} [{RegistrationNumber}] - {State}, Load: {CurrentLoad}/{MaxLoadKg} kg";
    }
}

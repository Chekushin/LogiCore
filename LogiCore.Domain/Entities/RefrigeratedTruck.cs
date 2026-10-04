using LogiCore.Domain.Enums;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Рефрижератор с контролем температуры для скоропортящихся грузов.
/// </summary>
public class RefrigeratedTruck : Vehicle
{
    /// <summary>
    /// Минимальная поддерживаемая температура.
    /// </summary>
    public decimal MinTemperature { get; }

    /// <summary>
    /// Максимальная поддерживаемая температура.
    /// </summary>
    public decimal MaxTemperature { get; }

    public RefrigeratedTruck(
        string registrationNumber,
        decimal maxLoadKg,
        decimal maxVolumeM3,
        decimal averageSpeedKmH,
        decimal baseRatePerKm,
        decimal minTemperature,
        decimal maxTemperature)
        : base(
            registrationNumber,
            VehicleType.RefrigeratedTruck,
            maxLoadKg,
            maxVolumeM3,
            averageSpeedKmH,
            baseRatePerKm)
    {
        if (minTemperature > maxTemperature)
        {
            throw new ArgumentException(
                "Min temperature cannot be greater than max temperature.");
        }

        MinTemperature = minTemperature;
        MaxTemperature = maxTemperature;
    }

    public override decimal CalculateDeliveryCost(
        Route route,
        IReadOnlyCollection<Cargo> cargo)
    {
        // Рефрижератор дороже из-за затрат на охлаждение
        decimal baseCost = route.DistanceKm * BaseRatePerKm;
        return baseCost * 1.5m; // Надбавка за рефрижерацию
    }

    public override bool CanCarry(Cargo cargo)
    {
        // Базовая проверка веса и объёма
        if (!base.CanCarry(cargo))
        {
            return false;
        }

        // Рефрижератор специализируется на скоропортящихся грузах
        // Но может возить и обычные грузы, если это экономически оправдано
        return true;
    }
}

using LogiCore.Domain.Enums;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Грузовой автомобиль (фура) с учётом платных дорог.
/// </summary>
public class Truck : Vehicle
{
    /// <summary>
    /// Коэффициент за платные дороги (обычно 1.1 - 1.3).
    /// </summary>
    public decimal TollRoadCoefficient { get; }

    public Truck(
        string registrationNumber,
        decimal maxLoadKg,
        decimal maxVolumeM3,
        decimal averageSpeedKmH,
        decimal baseRatePerKm,
        decimal tollRoadCoefficient = 1.15m)
        : base(
            registrationNumber,
            VehicleType.Truck,
            maxLoadKg,
            maxVolumeM3,
            averageSpeedKmH,
            baseRatePerKm)
    {
        if (tollRoadCoefficient < 1.0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tollRoadCoefficient),
                "Toll road coefficient must be at least 1.0.");
        }

        TollRoadCoefficient = tollRoadCoefficient;
    }

    public override decimal CalculateDeliveryCost(
        Route route,
        IReadOnlyCollection<Cargo> cargo)
    {
        decimal baseCost = route.DistanceKm * BaseRatePerKm;
        return baseCost * TollRoadCoefficient;
    }

    public override bool CanCarry(Cargo cargo)
    {
        // Фура не возит скоропортящиеся грузы (для них есть рефрижератор)
        if (cargo is PerishableCargo)
        {
            return false;
        }

        return base.CanCarry(cargo);
    }
}

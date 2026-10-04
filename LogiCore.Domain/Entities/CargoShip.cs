using LogiCore.Domain.Enums;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Грузовое судно для морских перевозок больших объёмов.
/// </summary>
public class CargoShip : Vehicle
{
    /// <summary>
    /// Надбавка за негабаритный груз (фиксированная сумма).
    /// </summary>
    public decimal OversizedSurcharge { get; }

    public CargoShip(
        string registrationNumber,
        decimal maxLoadKg,
        decimal maxVolumeM3,
        decimal averageSpeedKmH,
        decimal baseRatePerKm,
        decimal oversizedSurcharge = 5000m)
        : base(
            registrationNumber,
            VehicleType.CargoShip,
            maxLoadKg,
            maxVolumeM3,
            averageSpeedKmH,
            baseRatePerKm)
    {
        if (oversizedSurcharge < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(oversizedSurcharge),
                "Oversized surcharge cannot be negative.");
        }

        OversizedSurcharge = oversizedSurcharge;
    }

    public override decimal CalculateDeliveryCost(
        Route route,
        IReadOnlyCollection<Cargo> cargo)
    {
        // Судно — самый дешёвый тариф
        decimal baseCost = route.DistanceKm * BaseRatePerKm;

        // Надбавка за негабаритные грузы
        int oversizedCount = cargo.OfType<OversizedCargo>().Count();
        decimal oversizedCost = oversizedCount * OversizedSurcharge;

        return baseCost + oversizedCost;
    }

    public override bool CanCarry(Cargo cargo)
    {
        return base.CanCarry(cargo);
    }
}

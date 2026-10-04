using LogiCore.Domain.Enums;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Небольшой фургон для лёгких грузов.
/// </summary>
public class Van : Vehicle
{
    public Van(
        string registrationNumber,
        decimal maxLoadKg,
        decimal maxVolumeM3,
        decimal averageSpeedKmH,
        decimal baseRatePerKm)
        : base(
            registrationNumber,
            VehicleType.Van,
            maxLoadKg,
            maxVolumeM3,
            averageSpeedKmH,
            baseRatePerKm)
    {
    }

    public override decimal CalculateDeliveryCost(
        Route route,
        IReadOnlyCollection<Cargo> cargo)
    {
        return route.DistanceKm * BaseRatePerKm;
    }

    public override bool CanCarry(Cargo cargo)
    {
        // Фургон не возит опасные грузы
        if (cargo is DangerousCargo)
        {
            return false;
        }

        return base.CanCarry(cargo);
    }
}

using LogiCore.Domain.Enums;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Грузовой самолёт для быстрой доставки на дальние расстояния.
/// </summary>
public class CargoPlane : Vehicle
{
    /// <summary>
    /// Надбавка за каждый килограмм груза.
    /// </summary>
    public decimal SurchargePerKg { get; }

    public CargoPlane(
        string registrationNumber,
        decimal maxLoadKg,
        decimal maxVolumeM3,
        decimal averageSpeedKmH,
        decimal baseRatePerKm,
        decimal surchargePerKg = 5.0m)
        : base(
            registrationNumber,
            VehicleType.CargoPlane,
            maxLoadKg,
            maxVolumeM3,
            averageSpeedKmH,
            baseRatePerKm)
    {
        if (surchargePerKg < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(surchargePerKg),
                "Surcharge per kg cannot be negative.");
        }

        SurchargePerKg = surchargePerKg;
    }

    public override decimal CalculateDeliveryCost(
        Route route,
        IReadOnlyCollection<Cargo> cargo)
    {
        decimal baseCost = route.DistanceKm * BaseRatePerKm;
        decimal weightSurcharge = cargo.Sum(c => c.WeightKg) * SurchargePerKg;
        return baseCost + weightSurcharge;
    }

    public override bool CanCarry(Cargo cargo)
    {
        // Базовая проверка
        if (!base.CanCarry(cargo))
        {
            return false;
        }

        // Самолёт не возит опасные грузы классов 1, 2, 7 (взрывчатые, газы, радиоактивные)
        if (cargo is DangerousCargo dangerous)
        {
            var forbiddenClasses = new[]
            {
                DangerousCargoClass.Class1,
                DangerousCargoClass.Class2,
                DangerousCargoClass.Class7
            };

            if (forbiddenClasses.Contains(dangerous.DangerousClass))
            {
                return false;
            }
        }

        return true;
    }
}

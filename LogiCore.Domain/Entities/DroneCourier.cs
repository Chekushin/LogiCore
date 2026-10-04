using LogiCore.Domain.Enums;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Дрон-курьер для быстрой доставки лёгких грузов на короткие расстояния.
/// Класс объявлен sealed, так как дальнейшая специализация не предполагается.
/// </summary>
public sealed class DroneCourier : Vehicle
{
    /// <summary>
    /// Максимальная дальность полёта в километрах.
    /// </summary>
    public decimal MaxRangeKm { get; }

    public DroneCourier(
        string registrationNumber,
        decimal maxLoadKg,
        decimal maxVolumeM3,
        decimal averageSpeedKmH,
        decimal baseRatePerKm,
        decimal maxRangeKm)
        : base(
            registrationNumber,
            VehicleType.DroneCourier,
            maxLoadKg,
            maxVolumeM3,
            averageSpeedKmH,
            baseRatePerKm)
    {
        if (maxRangeKm <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxRangeKm),
                "Max range must be greater than zero.");
        }

        MaxRangeKm = maxRangeKm;
    }

    public override decimal CalculateDeliveryCost(
        Route route,
        IReadOnlyCollection<Cargo> cargo)
    {
        // Дрон берёт фиксированную ставку за рейс
        return route.DistanceKm * BaseRatePerKm * 2.0m; // Премиум-цена за скорость
    }

    public override bool CanCarry(Cargo cargo)
    {
        // Базовая проверка веса и объёма
        if (!base.CanCarry(cargo))
        {
            return false;
        }

        // Дрон имеет очень жёсткие ограничения по весу
        if (cargo.WeightKg > MaxLoadKg)
        {
            return false;
        }

        // Дрон не возит опасные грузы
        if (cargo is DangerousCargo)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Проверяет, может ли дрон долететь до пункта назначения.
    /// </summary>
    public bool CanReach(Route route)
    {
        return route.DistanceKm <= MaxRangeKm;
    }
}

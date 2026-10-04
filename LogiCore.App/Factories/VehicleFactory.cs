using LogiCore.Domain.Entities;
using LogiCore.Domain.Enums;

namespace LogiCore.App.Factories;

/// <summary>
/// Фабрика для создания транспортных средств различных типов.
/// Паттерн Factory Method.
/// </summary>
public class VehicleFactory
{
    public Vehicle Create(
        VehicleType type,
        string registrationNumber,
        decimal maxLoadKg,
        decimal maxVolumeM3,
        decimal averageSpeedKmH,
        decimal baseRatePerKm,
        decimal minTemperature = 0m,
        decimal maxTemperature = 0m,
        decimal tollRoadCoefficient = 1.15m,
        decimal surchargePerKg = 5.0m,
        decimal oversizedSurcharge = 5000m,
        decimal maxRangeKm = 50m)
    {
        return type switch
        {
            VehicleType.Van =>
                new Van(
                    registrationNumber,
                    maxLoadKg,
                    maxVolumeM3,
                    averageSpeedKmH,
                    baseRatePerKm),

            VehicleType.Truck =>
                new Truck(
                    registrationNumber,
                    maxLoadKg,
                    maxVolumeM3,
                    averageSpeedKmH,
                    baseRatePerKm,
                    tollRoadCoefficient),

            VehicleType.RefrigeratedTruck =>
                new RefrigeratedTruck(
                    registrationNumber,
                    maxLoadKg,
                    maxVolumeM3,
                    averageSpeedKmH,
                    baseRatePerKm,
                    minTemperature,
                    maxTemperature),

            VehicleType.CargoPlane =>
                new CargoPlane(
                    registrationNumber,
                    maxLoadKg,
                    maxVolumeM3,
                    averageSpeedKmH,
                    baseRatePerKm,
                    surchargePerKg),

            VehicleType.CargoShip =>
                new CargoShip(
                    registrationNumber,
                    maxLoadKg,
                    maxVolumeM3,
                    averageSpeedKmH,
                    baseRatePerKm,
                    oversizedSurcharge),

            VehicleType.DroneCourier =>
                new DroneCourier(
                    registrationNumber,
                    maxLoadKg,
                    maxVolumeM3,
                    averageSpeedKmH,
                    baseRatePerKm,
                    maxRangeKm),

            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unsupported vehicle type.")
        };
    }
}

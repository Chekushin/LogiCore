using LogiCore.Domain.Entities;
using LogiCore.Domain.Exceptions;
using LogiCore.Domain.Interfaces;

namespace LogiCore.App.Services;

/// <summary>
/// Сервис проверки совместимости грузов и их соответствия требованиям перевозки.
/// Централизует все правила совместимости в одном месте.
/// </summary>
public class CargoCompatibilityValidator
{
    /// <summary>
    /// Проверяет, можно ли перевозить набор грузов в одном транспортном средстве.
    /// </summary>
    public void ValidateCargoCompatibility(IReadOnlyCollection<Cargo> cargoList, Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(cargoList);
        ArgumentNullException.ThrowIfNull(vehicle);

        // Правило 1: Опасные грузы нельзя перевозить вместе со скоропортящимися
        ValidateDangerousAndPerishableCompatibility(cargoList);

        // Правило 2: Скоропортящиеся грузы можно перевозить только в транспорте с подходящим температурным режимом
        ValidateTemperatureRequirements(cargoList, vehicle);

        // Правило 3: Суммарные вес и объём не должны превышать лимиты ТС
        ValidateWeightAndVolumeConstraints(cargoList, vehicle);

        // Правило 4: Груз с истёкшим сроком годности не принимается к перевозке
        ValidateExpirationDates(cargoList);
    }

    /// <summary>
    /// Правило 1: Опасные грузы нельзя перевозить вместе со скоропортящимися.
    /// </summary>
    private void ValidateDangerousAndPerishableCompatibility(IReadOnlyCollection<Cargo> cargoList)
    {
        bool hasDangerous = cargoList.Any(c => c is DangerousCargo);
        bool hasPerishable = cargoList.Any(c => c is PerishableCargo);

        if (hasDangerous && hasPerishable)
        {
            throw new IncompatibleCargoException(
                "Dangerous cargo cannot be transported together with perishable cargo.");
        }
    }

    /// <summary>
    /// Правило 2: Скоропортящиеся грузы можно перевозить только в транспорте с подходящим температурным режимом.
    /// </summary>
    private void ValidateTemperatureRequirements(IReadOnlyCollection<Cargo> cargoList, Vehicle vehicle)
    {
        var perishableItems = cargoList.OfType<PerishableCargo>().ToList();

        if (perishableItems.Count == 0)
        {
            return;
        }

        // Проверяем, что транспорт поддерживает температурный режим
        if (vehicle is not RefrigeratedTruck refrigeratedTruck)
        {
            throw new IncompatibleCargoException(
                "Perishable cargo requires refrigerated transport.");
        }

        // Проверяем, что температурный диапазон транспорта подходит для всех скоропортящихся грузов
        foreach (var perishable in perishableItems)
        {
            var tempSensitive = (ITemperatureSensitive)perishable;

            if (tempSensitive.MinTemperature < refrigeratedTruck.MinTemperature ||
                tempSensitive.MaxTemperature > refrigeratedTruck.MaxTemperature)
            {
                throw new IncompatibleCargoException(
                    $"Perishable cargo '{perishable.Description}' requires temperature range " +
                    $"{tempSensitive.MinTemperature}°C to {tempSensitive.MaxTemperature}°C, " +
                    $"but vehicle supports {refrigeratedTruck.MinTemperature}°C to {refrigeratedTruck.MaxTemperature}°C.");
            }
        }
    }

    /// <summary>
    /// Правило 3: Суммарные вес и объём не должны превышать лимиты транспортного средства.
    /// </summary>
    private void ValidateWeightAndVolumeConstraints(IReadOnlyCollection<Cargo> cargoList, Vehicle vehicle)
    {
        decimal totalWeight = cargoList.Sum(c => c.WeightKg);
        decimal totalVolume = cargoList.Sum(c => c.VolumeM3);

        if (totalWeight > vehicle.MaxLoadKg)
        {
            throw new VehicleOverloadException(
                $"Total cargo weight {totalWeight} kg exceeds vehicle capacity {vehicle.MaxLoadKg} kg.");
        }

        if (totalVolume > vehicle.MaxVolumeM3)
        {
            throw new VehicleOverloadException(
                $"Total cargo volume {totalVolume} m³ exceeds vehicle capacity {vehicle.MaxVolumeM3} m³.");
        }
    }

    /// <summary>
    /// Правило 4: Груз с истёкшим сроком годности не принимается к перевозке.
    /// </summary>
    private void ValidateExpirationDates(IReadOnlyCollection<Cargo> cargoList)
    {
        var expiredItems = cargoList
            .OfType<PerishableCargo>()
            .Where(p => p.IsExpired())
            .ToList();

        if (expiredItems.Count > 0)
        {
            var descriptions = string.Join(", ", expiredItems.Select(c => c.Description));
            throw new CargoValidationException(
                $"The following perishable cargo items have expired: {descriptions}");
        }
    }
}

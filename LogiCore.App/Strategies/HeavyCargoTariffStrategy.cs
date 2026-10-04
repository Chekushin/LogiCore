using LogiCore.Domain.Entities;
using LogiCore.Domain.Interfaces;

namespace LogiCore.App.Strategies;

/// <summary>
/// Стратегия для тяжёлых грузов с надбавкой за вес.
/// </summary>
public class HeavyCargoTariffStrategy : ITariffStrategy
{
    public string Name => "Heavy Cargo";

    private const decimal HeavyCargoThresholdKg = 1000m;
    private const decimal SurchargePerKgOver = 0.5m;

    public decimal Calculate(
        decimal baseCost,
        Route route,
        IReadOnlyCollection<Cargo> cargo)
    {
        decimal totalWeight = cargo.Sum(c => c.WeightKg);

        if (totalWeight <= HeavyCargoThresholdKg)
        {
            return baseCost;
        }

        // Надбавка за каждый килограмм сверх порога
        decimal excessWeight = totalWeight - HeavyCargoThresholdKg;
        decimal surcharge = excessWeight * SurchargePerKgOver;

        return baseCost + surcharge;
    }
}

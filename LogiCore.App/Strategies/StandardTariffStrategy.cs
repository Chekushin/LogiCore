using LogiCore.Domain.Entities;
using LogiCore.Domain.Interfaces;

namespace LogiCore.App.Strategies;

public class StandardTariffStrategy : ITariffStrategy
{
    public string Name => "Standard";

    public decimal Calculate(
        decimal baseCost,
        Route route,
        IReadOnlyCollection<Cargo> cargo)
    {
        return baseCost;
    }
}
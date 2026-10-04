using LogiCore.Domain.Entities;
using LogiCore.Domain.Interfaces;

namespace LogiCore.App.Strategies;

/// <summary>
/// Стратегия экспресс-доставки с повышенным тарифом.
/// </summary>
public class ExpressTariffStrategy : ITariffStrategy
{
    public string Name => "Express";

    public decimal Calculate(
        decimal baseCost,
        Route route,
        IReadOnlyCollection<Cargo> cargo)
    {
        // Экспресс-доставка стоит в 1.5 раза дороже
        return baseCost * 1.5m;
    }
}

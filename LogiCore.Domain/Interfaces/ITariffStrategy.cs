using LogiCore.Domain.Entities;

namespace LogiCore.Domain.Interfaces;

public interface ITariffStrategy
{
    string Name { get; }

    decimal Calculate(
        decimal baseCost,
        Route route,
        IReadOnlyCollection<Cargo> cargo);
}
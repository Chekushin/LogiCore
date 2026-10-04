using LogiCore.Domain.Interfaces;

namespace LogiCore.App.Strategies;

public sealed class TariffStrategyRegistry
{
    private static readonly Lazy<TariffStrategyRegistry> _instance =
        new(() => new TariffStrategyRegistry());

    private readonly Dictionary<string, ITariffStrategy> _strategies = new();

    public static TariffStrategyRegistry Instance => _instance.Value;

    private TariffStrategyRegistry()
    {
    }

    public void Register(ITariffStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);

        _strategies[strategy.Name] = strategy;
    }

    public ITariffStrategy? Get(string name)
    {
        _strategies.TryGetValue(name, out var strategy);

        return strategy;
    }

    public IReadOnlyCollection<ITariffStrategy> GetAll()
    {
        return _strategies.Values.ToList().AsReadOnly();
    }
}
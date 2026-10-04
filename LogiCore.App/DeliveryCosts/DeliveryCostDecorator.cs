using LogiCore.Domain.Interfaces;

namespace LogiCore.App.DeliveryCosts;

public abstract class DeliveryCostDecorator : IDeliveryCost
{
    protected readonly IDeliveryCost WrappedCost;

    protected DeliveryCostDecorator(IDeliveryCost wrappedCost)
    {
        WrappedCost = wrappedCost;
    }

    public abstract decimal Total { get; }

    public abstract string Describe();
}
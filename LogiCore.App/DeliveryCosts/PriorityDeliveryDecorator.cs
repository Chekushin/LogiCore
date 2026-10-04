using LogiCore.Domain.Interfaces;

namespace LogiCore.App.DeliveryCosts;

public class PriorityDeliveryDecorator : DeliveryCostDecorator
{
    private readonly decimal _priorityCost;

    public PriorityDeliveryDecorator(
        IDeliveryCost wrappedCost,
        decimal priorityCost)
        : base(wrappedCost)
    {
        if (priorityCost < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priorityCost),
                "Priority delivery cost cannot be negative.");
        }

        _priorityCost = priorityCost;
    }

    public override decimal Total =>
        WrappedCost.Total + _priorityCost;

    public override string Describe() =>
        $"{WrappedCost.Describe()}; Priority delivery: {_priorityCost}";
}
using LogiCore.Domain.Interfaces;

namespace LogiCore.App.DeliveryCosts;

public class InsuranceDecorator : DeliveryCostDecorator
{
    private readonly decimal _insuranceCost;

    public InsuranceDecorator(
        IDeliveryCost wrappedCost,
        decimal insuranceCost)
        : base(wrappedCost)
    {
        if (insuranceCost < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(insuranceCost),
                "Insurance cost cannot be negative.");
        }

        _insuranceCost = insuranceCost;
    }

    public override decimal Total =>
        WrappedCost.Total + _insuranceCost;

    public override string Describe() =>
        $"{WrappedCost.Describe()}; Insurance: {_insuranceCost}";
}
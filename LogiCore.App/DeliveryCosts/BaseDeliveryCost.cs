using LogiCore.Domain.Interfaces;

namespace LogiCore.App.DeliveryCosts;

public class BaseDeliveryCost : IDeliveryCost
{
    public decimal Total { get; }

    public BaseDeliveryCost(decimal total)
    {
        if (total < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(total),
                "Delivery cost cannot be negative.");
        }

        Total = total;
    }

    public string Describe()
    {
        return $"Base delivery cost: {Total}";
    }
}
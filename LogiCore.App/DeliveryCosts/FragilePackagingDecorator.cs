using LogiCore.Domain.Interfaces;

namespace LogiCore.App.DeliveryCosts;

/// <summary>
/// Декоратор для добавления стоимости упаковки хрупкого груза.
/// </summary>
public class FragilePackagingDecorator : DeliveryCostDecorator
{
    private readonly decimal _packagingCost;

    public FragilePackagingDecorator(
        IDeliveryCost wrappedCost,
        decimal packagingCost)
        : base(wrappedCost)
    {
        if (packagingCost < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(packagingCost),
                "Packaging cost cannot be negative.");
        }

        _packagingCost = packagingCost;
    }

    public override decimal Total =>
        WrappedCost.Total + _packagingCost;

    public override string Describe() =>
        $"{WrappedCost.Describe()}; Fragile packaging: {_packagingCost}";
}

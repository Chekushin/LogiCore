using LogiCore.Domain.Interfaces;

namespace LogiCore.App.Adapters;

public class ExternalDeliveryPriceAdapter : IDeliveryCost
{
    private readonly ExternalDeliveryPrice _externalPrice;

    public ExternalDeliveryPriceAdapter(
        ExternalDeliveryPrice externalPrice)
    {
        ArgumentNullException.ThrowIfNull(externalPrice);

        _externalPrice = externalPrice;
    }

    public decimal Total => _externalPrice.Amount;

    public string Describe()
    {
        return $"External delivery price: {_externalPrice.GetFormattedPrice()}";
    }
}
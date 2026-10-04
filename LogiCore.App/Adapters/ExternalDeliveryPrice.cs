namespace LogiCore.App.Adapters;

public class ExternalDeliveryPrice
{
    public decimal Amount { get; }

    public string Currency { get; }

    public ExternalDeliveryPrice(
        decimal amount,
        string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public string GetFormattedPrice()
    {
        return $"{Amount} {Currency}";
    }
}
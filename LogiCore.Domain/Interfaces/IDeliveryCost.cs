namespace LogiCore.Domain.Interfaces;

public interface IDeliveryCost
{
    decimal Total { get; }

    string Describe();
}
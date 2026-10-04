using LogiCore.Domain.Interfaces;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Клиент, оформляющий заказы на доставку.
/// </summary>
public class Customer : IEntity
{
    private readonly List<Order> _orders;

    public Guid Id { get; }

    /// <summary>
    /// Имя клиента.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Контактная информация.
    /// </summary>
    public string Contact { get; }

    /// <summary>
    /// История заказов клиента (только для чтения).
    /// </summary>
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    public Customer(string name, string contact)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Customer name cannot be empty.",
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(contact))
        {
            throw new ArgumentException(
                "Customer contact cannot be empty.",
                nameof(contact));
        }

        Id = Guid.NewGuid();
        Name = name;
        Contact = contact;
        _orders = new List<Order>();
    }

    /// <summary>
    /// Добавляет заказ в историю клиента.
    /// </summary>
    public void AddOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        _orders.Add(order);
    }

    public override string ToString()
    {
        return $"Customer: {Name} ({Contact}), Orders: {_orders.Count}";
    }
}

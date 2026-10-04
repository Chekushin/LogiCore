using LogiCore.Domain.Interfaces;

namespace LogiCore.App.Repositories;

public class InMemoryRepository<T> : IReadOnlyRepository<T>
    where T : class, IEntity
{
    private readonly List<T> _items = new();

    public T? GetById(Guid id)
    {
        return _items.FirstOrDefault(item => item.Id == id);
    }

    public IEnumerable<T> GetAll()
    {
        return _items;
    }

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _items.Add(item);
    }

    public bool Remove(Guid id)
    {
        var item = GetById(id);

        if (item is null)
        {
            return false;
        }

        return _items.Remove(item);
    }
}
using System.Collections;

namespace LogiCore.App.Collections;

public class EntityCollection<T> : IEnumerable<T>
    where T : class
{
    private readonly List<T> _items = new();

    public int Count => _items.Count;

    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _items.Add(item);
    }

    public bool Remove(T item)
    {
        return _items.Remove(item);
    }

    public T? GetAt(int index)
    {
        if (index < 0 || index >= _items.Count)
        {
            return null;
        }

        return _items[index];
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (var item in _items)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
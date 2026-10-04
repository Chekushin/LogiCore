using System.Collections;
using LogiCore.Domain.Interfaces;

namespace LogiCore.App.Repositories;

/// <summary>
/// Обобщённый репозиторий для хранения сущностей в памяти.
/// Демонстрирует использование generics, ограничений типов, индексатора и собственного итератора.
/// </summary>
public class Repository<T> : IEnumerable<T>
    where T : class, IEntity
{
    private readonly List<T> _items = new();

    /// <summary>
    /// Индексатор для доступа к сущности по Id.
    /// </summary>
    public T? this[Guid id] => GetById(id);

    /// <summary>
    /// Добавляет сущность в репозиторий.
    /// </summary>
    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _items.Add(item);
    }

    /// <summary>
    /// Удаляет сущность из репозитория по Id.
    /// </summary>
    public bool Remove(Guid id)
    {
        var item = GetById(id);
        if (item is null)
        {
            return false;
        }

        return _items.Remove(item);
    }

    /// <summary>
    /// Находит все сущности, удовлетворяющие предикату.
    /// </summary>
    public IEnumerable<T> FindAll(Predicate<T> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        // Используем собственный итератор через yield return
        foreach (var item in _items)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Получает сущность по Id.
    /// </summary>
    public T? GetById(Guid id)
    {
        return _items.FirstOrDefault(item => item.Id == id);
    }

    /// <summary>
    /// Получает все сущности.
    /// </summary>
    public IEnumerable<T> GetAll()
    {
        // Реализация IEnumerable через собственный итератор с yield return
        foreach (var item in _items)
        {
            yield return item;
        }
    }

    /// <summary>
    /// Количество сущностей в репозитории.
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    /// Реализация IEnumerable<T> через собственный итератор.
    /// Позволяет использовать репозиторий в foreach.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        // Используем yield return для ленивой итерации
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

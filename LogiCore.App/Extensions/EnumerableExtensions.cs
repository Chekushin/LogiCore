using System.Text;
using LogiCore.Domain.Interfaces;

namespace LogiCore.App.Extensions;

/// <summary>
/// Методы расширения для работы с коллекциями.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Обобщённый метод расширения для форматирования коллекции в табличный вид.
    /// Используется для вывода отчётов.
    /// </summary>
    public static string ToReportTable<T>(this IEnumerable<T> items, params Func<T, object>[] columns)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(columns);

        var itemsList = items.ToList();
        
        if (!itemsList.Any() || columns.Length == 0)
        {
            return "No data to display.";
        }

        var sb = new StringBuilder();
        var separator = new string('-', 80);
        
        sb.AppendLine(separator);
        
        // Заголовок таблицы
        sb.AppendLine($"Report: {typeof(T).Name} ({itemsList.Count} items)");
        sb.AppendLine(separator);

        // Данные
        foreach (var item in itemsList)
        {
            var values = columns.Select(col => col(item)?.ToString() ?? "N/A");
            sb.AppendLine(string.Join(" | ", values));
        }
        
        sb.AppendLine(separator);

        return sb.ToString();
    }

    /// <summary>
    /// Упрощённая версия ToReportTable для IEntity, использующая ToString().
    /// </summary>
    public static string ToReportList<T>(this IEnumerable<T> items)
        where T : IEntity
    {
        ArgumentNullException.ThrowIfNull(items);

        var itemsList = items.ToList();
        
        if (!itemsList.Any())
        {
            return "No items to display.";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"=== {typeof(T).Name} Report ({itemsList.Count} items) ===");
        
        int index = 1;
        foreach (var item in itemsList)
        {
            sb.AppendLine($"{index}. {item}");
            index++;
        }

        return sb.ToString();
    }
}

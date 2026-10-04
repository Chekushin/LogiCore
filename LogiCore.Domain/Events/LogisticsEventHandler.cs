namespace LogiCore.Domain.Events;

/// <summary>
/// Собственный тип делегата для обработки событий логистической системы.
/// Демонстрирует объявление делегата через ключевое слово delegate.
/// </summary>
public delegate void LogisticsEventHandler(object sender, LogisticsEventArgs e);

/// <summary>
/// Базовый класс для аргументов событий логистической системы.
/// </summary>
public class LogisticsEventArgs : EventArgs
{
    public string Message { get; }

    public DateTime Timestamp { get; }

    public LogisticsEventArgs(string message)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Timestamp = DateTime.UtcNow;
    }
}

namespace LogiCore.Domain.Exceptions;

/// <summary>
/// Базовый класс для всех исключений логистической системы.
/// </summary>
public class LogisticsException : Exception
{
    public LogisticsException(string message)
        : base(message)
    {
    }

    public LogisticsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

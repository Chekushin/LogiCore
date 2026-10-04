namespace LogiCore.Domain.Exceptions;

/// <summary>
/// Исключение, возникающее при недопустимом переходе состояния заказа.
/// </summary>
public class InvalidOrderStateException : LogisticsException
{
    public InvalidOrderStateException(string message)
        : base(message)
    {
    }

    public InvalidOrderStateException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

namespace LogiCore.Domain.Exceptions;

/// <summary>
/// Исключение, возникающее при попытке совместной перевозки несовместимых грузов.
/// </summary>
public class IncompatibleCargoException : LogisticsException
{
    public IncompatibleCargoException(string message)
        : base(message)
    {
    }

    public IncompatibleCargoException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

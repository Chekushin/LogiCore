namespace LogiCore.Domain.Exceptions;

/// <summary>
/// Исключение, возникающее при ошибке валидации груза.
/// </summary>
public class CargoValidationException : LogisticsException
{
    public CargoValidationException(string message)
        : base(message)
    {
    }

    public CargoValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

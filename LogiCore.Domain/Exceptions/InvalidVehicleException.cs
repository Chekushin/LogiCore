namespace LogiCore.Domain.Exceptions;

/// <summary>
/// Исключение, возникающее при попытке создать невалидное транспортное средство.
/// </summary>
public class InvalidVehicleException : LogisticsException
{
    public InvalidVehicleException(string message)
        : base(message)
    {
    }

    public InvalidVehicleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

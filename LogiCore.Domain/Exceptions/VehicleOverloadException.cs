namespace LogiCore.Domain.Exceptions;

/// <summary>
/// Исключение, возникающее при попытке перегрузки транспортного средства.
/// </summary>
public class VehicleOverloadException : LogisticsException
{
    public VehicleOverloadException(string message)
        : base(message)
    {
    }

    public VehicleOverloadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

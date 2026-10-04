namespace LogiCore.Domain.Exceptions;

/// <summary>
/// Исключение, возникающее когда маршрут не может быть найден или построен.
/// </summary>
public class RouteNotFoundException : LogisticsException
{
    public RouteNotFoundException(string message)
        : base(message)
    {
    }

    public RouteNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

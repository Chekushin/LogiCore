namespace LogiCore.Domain.Exceptions;

public class VehicleOverloadAttempt : Exception
{
    public VehicleOverloadAttempt(string message)
        : base(message)
    {
    }
}
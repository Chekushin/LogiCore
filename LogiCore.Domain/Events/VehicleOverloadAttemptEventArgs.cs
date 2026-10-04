using LogiCore.Domain.Entities;

namespace LogiCore.Domain.Events;

/// <summary>
/// Аргументы события попытки перегрузки транспортного средства.
/// </summary>
public class VehicleOverloadAttemptEventArgs : EventArgs
{
    public Vehicle Vehicle { get; }

    public Cargo Cargo { get; }

    public decimal AttemptedWeight { get; }

    public decimal VehicleCapacity { get; }

    public DateTime OccurredAt { get; }

    public VehicleOverloadAttemptEventArgs(
        Vehicle vehicle,
        Cargo cargo,
        decimal attemptedWeight,
        decimal vehicleCapacity)
    {
        Vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
        Cargo = cargo ?? throw new ArgumentNullException(nameof(cargo));
        AttemptedWeight = attemptedWeight;
        VehicleCapacity = vehicleCapacity;
        OccurredAt = DateTime.UtcNow;
    }
}

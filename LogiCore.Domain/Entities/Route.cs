using LogiCore.Domain.Interfaces;
using LogiCore.Domain.ValueObjects;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Маршрут доставки с последовательностью точек.
/// </summary>
public class Route : IEntity
{
    private readonly List<RoutePoint> _points;

    public Guid Id { get; }

    public string Name { get; }

    /// <summary>
    /// Последовательность точек маршрута.
    /// </summary>
    public IReadOnlyCollection<RoutePoint> Points => _points.AsReadOnly();

    /// <summary>
    /// Общая дистанция маршрута в километрах (вычисляемое свойство).
    /// </summary>
    public decimal DistanceKm
    {
        get
        {
            if (_points.Count < 2)
            {
                return 0m;
            }

            decimal totalDistance = 0m;
            for (int i = 0; i < _points.Count - 1; i++)
            {
                totalDistance += _points[i + 1] - _points[i];
            }

            return totalDistance;
        }
    }

    public Route(string name, IEnumerable<RoutePoint> points)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Route name cannot be empty.",
                nameof(name));
        }

        _points = points?.ToList() ?? throw new ArgumentNullException(nameof(points));

        if (_points.Count < 2)
        {
            throw new ArgumentException(
                "Route must contain at least 2 points.",
                nameof(points));
        }

        Id = Guid.NewGuid();
        Name = name;
    }

    /// <summary>
    /// Оценивает время доставки для заданного транспортного средства.
    /// </summary>
    public TimeSpan EstimateTime(Vehicle vehicle)
    {
        if (vehicle == null)
        {
            throw new ArgumentNullException(nameof(vehicle));
        }

        if (vehicle.AverageSpeedKmH <= 0)
        {
            throw new ArgumentException(
                "Vehicle speed must be greater than zero.",
                nameof(vehicle));
        }

        double hours = (double)DistanceKm / (double)vehicle.AverageSpeedKmH;
        return TimeSpan.FromHours(hours);
    }
}

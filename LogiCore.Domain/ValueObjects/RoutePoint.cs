namespace LogiCore.Domain.ValueObjects;

/// <summary>
/// Точка маршрута с географическими координатами.
/// </summary>
public struct RoutePoint
{
    /// <summary>
    /// Широта (Latitude).
    /// </summary>
    public double Latitude { get; }

    /// <summary>
    /// Долгота (Longitude).
    /// </summary>
    public double Longitude { get; }

    /// <summary>
    /// Название точки.
    /// </summary>
    public string Name { get; }

    public RoutePoint(double latitude, double longitude, string name)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new ArgumentOutOfRangeException(
                nameof(latitude),
                "Latitude must be between -90 and 90.");
        }

        if (longitude < -180 || longitude > 180)
        {
            throw new ArgumentOutOfRangeException(
                nameof(longitude),
                "Longitude must be between -180 and 180.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Name cannot be empty.",
                nameof(name));
        }

        Latitude = latitude;
        Longitude = longitude;
        Name = name;
    }

    /// <summary>
    /// Вычисляет расстояние между двумя точками по формуле гаверсинусов (в километрах).
    /// </summary>
    public static decimal operator -(RoutePoint pointA, RoutePoint pointB)
    {
        // Радиус Земли в километрах
        const double earthRadiusKm = 6371.0;

        double lat1Rad = DegreesToRadians(pointA.Latitude);
        double lat2Rad = DegreesToRadians(pointB.Latitude);
        double deltaLat = DegreesToRadians(pointB.Latitude - pointA.Latitude);
        double deltaLon = DegreesToRadians(pointB.Longitude - pointA.Longitude);

        double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double distance = earthRadiusKm * c;

        return (decimal)distance;
    }

    /// <summary>
    /// Явное приведение к строке для получения текстового представления координат.
    /// </summary>
    public static explicit operator string(RoutePoint point)
    {
        return point.ToString();
    }

    public override string ToString()
    {
        return $"{Name} ({Latitude:F4}, {Longitude:F4})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not RoutePoint other)
        {
            return false;
        }

        return Latitude.Equals(other.Latitude) &&
               Longitude.Equals(other.Longitude) &&
               Name == other.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Latitude, Longitude, Name);
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}

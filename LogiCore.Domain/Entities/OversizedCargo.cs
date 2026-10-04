namespace LogiCore.Domain.Entities;

/// <summary>
/// Негабаритный груз, превышающий стандартные размеры.
/// </summary>
public class OversizedCargo : Cargo
{
    /// <summary>
    /// Длина в метрах.
    /// </summary>
    public decimal LengthM { get; }

    /// <summary>
    /// Ширина в метрах.
    /// </summary>
    public decimal WidthM { get; }

    /// <summary>
    /// Высота в метрах.
    /// </summary>
    public decimal HeightM { get; }

    public OversizedCargo(
        string description,
        decimal weightKg,
        decimal volumeM3,
        decimal declaredValue,
        decimal lengthM,
        decimal widthM,
        decimal heightM)
        : base(description, weightKg, volumeM3, declaredValue)
    {
        if (lengthM <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lengthM),
                "Length must be greater than zero.");
        }

        if (widthM <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(widthM),
                "Width must be greater than zero.");
        }

        if (heightM <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(heightM),
                "Height must be greater than zero.");
        }

        LengthM = lengthM;
        WidthM = widthM;
        HeightM = heightM;
    }

    /// <summary>
    /// Проверяет, является ли груз действительно негабаритным.
    /// </summary>
    public bool IsOversized()
    {
        // Груз считается негабаритным, если хотя бы один размер превышает 3 метра
        return LengthM > 3.0m || WidthM > 3.0m || HeightM > 3.0m;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, Dimensions: {LengthM}x{WidthM}x{HeightM} m";
    }
}

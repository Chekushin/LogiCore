using LogiCore.Domain.Enums;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Опасный груз с классом опасности по международной классификации.
/// </summary>
public class DangerousCargo : Cargo
{
    /// <summary>
    /// Класс опасности груза (1-9 по международной классификации).
    /// </summary>
    public DangerousCargoClass DangerousClass { get; }

    public DangerousCargo(
        string description,
        decimal weightKg,
        decimal volumeM3,
        decimal declaredValue,
        DangerousCargoClass dangerousClass)
        : base(description, weightKg, volumeM3, declaredValue)
    {
        DangerousClass = dangerousClass;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, Dangerous Class: {(int)DangerousClass}";
    }
}

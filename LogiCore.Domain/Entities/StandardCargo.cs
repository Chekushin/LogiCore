using LogiCore.Domain.Interfaces;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Обычный стандартный груз без специальных требований.
/// </summary>
public class StandardCargo : Cargo, IStackable
{
    public StandardCargo(
        string description,
        decimal weightKg,
        decimal volumeM3,
        decimal declaredValue)
        : base(description, weightKg, volumeM3, declaredValue)
    {
    }

    public bool CanStack(Cargo cargo)
    {
        // Стандартный груз можно укладывать поверх другого стандартного груза
        return cargo is StandardCargo;
    }
}

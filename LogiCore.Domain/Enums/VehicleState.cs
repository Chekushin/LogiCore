namespace LogiCore.Domain.Enums;

/// <summary>
/// Состояние транспортного средства.
/// </summary>
public enum VehicleState
{
    /// <summary>
    /// Свободен и доступен для назначения.
    /// </summary>
    Free,

    /// <summary>
    /// В процессе доставки.
    /// </summary>
    InTransit,

    /// <summary>
    /// На техническом обслуживании.
    /// </summary>
    UnderMaintenance
}

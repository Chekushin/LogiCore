namespace LogiCore.Domain.Enums;

[Flags]
public enum TransportConditions
{
    None = 0,
    Refrigerated = 1,
    Sealed = 2,
    Pressurized = 4,
    LongRange = 8
}
namespace LogiCore.Domain.Interfaces;

public interface ITemperatureSensitive
{
    decimal MinTemperature { get; }

    decimal MaxTemperature { get; }

    bool IsTemperatureAllowed(decimal temperature);
}
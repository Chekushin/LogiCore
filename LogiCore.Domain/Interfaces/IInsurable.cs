namespace LogiCore.Domain.Interfaces;

public interface IInsurable
{
    decimal InsuranceValue { get; }

    decimal CalculateInsuranceCost();
}
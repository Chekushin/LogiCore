using LogiCore.Domain.Interfaces;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Хрупкий груз, требующий особой осторожности при перевозке.
/// </summary>
public class FragileCargo : Cargo, IInsurable
{
    /// <summary>
    /// Коэффициент риска повреждения (от 1.0 до 3.0).
    /// </summary>
    public decimal RiskFactor { get; }

    public FragileCargo(
        string description,
        decimal weightKg,
        decimal volumeM3,
        decimal declaredValue,
        decimal riskFactor)
        : base(description, weightKg, volumeM3, declaredValue)
    {
        if (riskFactor < 1.0m || riskFactor > 3.0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(riskFactor),
                "Risk factor must be between 1.0 and 3.0.");
        }

        RiskFactor = riskFactor;
    }

    decimal IInsurable.InsuranceValue => DeclaredValue;

    decimal IInsurable.CalculateInsuranceCost()
    {
        // Страховка для хрупких грузов зависит от коэффициента риска
        return DeclaredValue * 0.03m * RiskFactor;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, Risk Factor: {RiskFactor:F2}";
    }
}

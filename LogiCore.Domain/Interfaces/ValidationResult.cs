namespace LogiCore.Domain.Interfaces;

public class ValidationResult
{
    public bool IsValid { get; }

    public IReadOnlyCollection<string> Errors { get; }

    private ValidationResult(
        bool isValid,
        IReadOnlyCollection<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public static ValidationResult Success()
    {
        return new ValidationResult(
            true,
            []);
    }

    public static ValidationResult Failure(
        params string[] errors)
    {
        return new ValidationResult(
            false,
            errors);
    }
}
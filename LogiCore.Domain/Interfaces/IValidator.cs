namespace LogiCore.Domain.Interfaces;

public interface IValidator<in T>
{
    ValidationResult Validate(T item);
}
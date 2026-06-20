namespace TailorApp.Application.Exceptions;

/// <summary>
/// Thrown by services when an input model fails validation. Named distinctly
/// from <see cref="System.ComponentModel.DataAnnotations.ValidationException"/>
/// to avoid ambiguity in files that use the DataAnnotations namespace.
/// </summary>
public sealed class ValidationFailedException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationFailedException(IReadOnlyList<string> errors)
        : base(errors.Count > 0 ? string.Join("; ", errors) : "Validation failed.")
    {
        Errors = errors;
    }
}

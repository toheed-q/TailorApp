using System.ComponentModel.DataAnnotations;
using TailorApp.Application.Exceptions;

namespace TailorApp.Application.Common;

/// <summary>
/// Runs DataAnnotations validation on an input model and throws a typed
/// <see cref="ValidationFailedException"/> on failure. Lets services enforce
/// the same rules the UI form uses, with no duplicated logic.
/// </summary>
public static class ModelValidator
{
    public static void Validate(object model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(model, context, results, validateAllProperties: true))
        {
            var errors = results
                .Select(r => r.ErrorMessage ?? "Invalid value.")
                .ToList();
            throw new ValidationFailedException(errors);
        }
    }
}

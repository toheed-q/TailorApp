namespace TailorApp.Application.Exceptions;

/// <summary>
/// Thrown when an operation targets an entity that does not exist (or has been
/// soft-deleted).
/// </summary>
public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entity, string id)
        : base($"{entity} with id '{id}' was not found.")
    {
    }
}

namespace TailorApp.Application.Models;

/// <summary>
/// A neutral representation of a Firestore document: its id plus a flat map of
/// field name → CLR value (string, bool, long, double, DateTime, or null).
/// Keeps the Firestore wire format out of the sync/mapping code.
/// </summary>
public sealed class FirestoreDocument
{
    public required string Id { get; init; }
    public required IReadOnlyDictionary<string, object?> Fields { get; init; }
}

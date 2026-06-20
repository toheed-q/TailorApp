namespace TailorApp.Application.Exceptions;

/// <summary>
/// Thrown when a Firestore REST operation fails (non-success status, transport
/// error, or unexpected payload). Carries a message suitable for surfacing to
/// the sync layer / UI.
/// </summary>
public sealed class FirestoreException : Exception
{
    public FirestoreException(string message) : base(message) { }
    public FirestoreException(string message, Exception inner) : base(message, inner) { }
}

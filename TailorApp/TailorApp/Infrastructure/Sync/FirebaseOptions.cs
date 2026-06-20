namespace TailorApp.Infrastructure.Sync;

/// <summary>
/// Firebase project configuration for the REST integration. The API key is a
/// public client identifier (not a secret), so it is safe to embed and ship.
/// Access to data is enforced by Firebase Auth + Firestore security rules,
/// not by hiding this key.
/// </summary>
public sealed class FirebaseOptions
{
    public required string ApiKey { get; init; }
    public required string ProjectId { get; init; }
}

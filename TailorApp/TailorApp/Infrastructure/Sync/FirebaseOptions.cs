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

    /// <summary>
    /// The shop's Firebase account. The app signs in with this silently at
    /// startup so cloud backup needs no login screen. Because every install
    /// uses the same account, the data lands under one stable <c>uid</c> and
    /// stays reachable after a reinstall or on a replacement phone.
    /// This account must exist in Firebase Console → Authentication.
    /// </summary>
    public required string ShopEmail { get; init; }

    public required string ShopPassword { get; init; }
}

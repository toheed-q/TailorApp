namespace TailorApp.Application.Interfaces;

/// <summary>
/// Handles authentication against Firebase (Identity Toolkit) over REST for the
/// single shop account. Owns token lifecycle: sign-in, secure persistence,
/// silent refresh, and providing a valid ID token to the Firestore client.
/// </summary>
public interface IFirebaseAuthService
{
    /// <summary>True if a session (refresh token) is available.</summary>
    bool IsSignedIn { get; }

    /// <summary>The signed-in user's Firebase UID, used to scope Firestore data.</summary>
    string? CurrentUserId { get; }

    /// <summary>
    /// Signs in with the shop's email/password. Persists tokens to secure
    /// storage. Throws <see cref="Exceptions.AuthenticationFailedException"/> on failure.
    /// </summary>
    Task SignInAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a non-expired ID token, refreshing silently if needed. Returns
    /// null if there is no valid session (caller must sign in).
    /// </summary>
    Task<string?> GetValidIdTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>Loads any persisted session from secure storage (call at startup).</summary>
    Task<bool> TryRestoreSessionAsync();

    /// <summary>
    /// Ensures a usable session exists, signing in silently with the configured
    /// shop account if needed. Returns false when it could not authenticate
    /// (e.g. offline or bad credentials) — callers should skip cloud work.
    /// Never throws.
    /// </summary>
    Task<bool> EnsureSignedInAsync(CancellationToken cancellationToken = default);

    /// <summary>Clears the session and secure storage.</summary>
    Task SignOutAsync();
}

namespace TailorApp.Application.Exceptions;

/// <summary>
/// Thrown when Firebase email/password sign-in fails (bad credentials,
/// disabled account, network error, etc.). Carries a user-friendly message.
/// </summary>
public sealed class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException(string message) : base(message) { }
}

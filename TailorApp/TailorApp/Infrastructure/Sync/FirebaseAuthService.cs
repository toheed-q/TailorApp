using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Storage;
using TailorApp.Application.Exceptions;
using TailorApp.Application.Interfaces;

namespace TailorApp.Infrastructure.Sync;

/// <summary>
/// REST-based <see cref="IFirebaseAuthService"/> using the Identity Toolkit and
/// Secure Token endpoints. Tokens live in encrypted <see cref="SecureStorage"/>;
/// refresh is guarded by a semaphore so concurrent callers refresh once.
/// </summary>
public sealed class FirebaseAuthService : IFirebaseAuthService
{
    private const string SignInUrl = "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=";
    private const string RefreshUrl = "https://securetoken.googleapis.com/v1/token?key=";

    // SecureStorage keys.
    private const string KeyIdToken = "fb_id_token";
    private const string KeyRefreshToken = "fb_refresh_token";
    private const string KeyExpiry = "fb_token_expiry_ticks";
    private const string KeyUid = "fb_uid";

    // Refresh slightly before actual expiry to avoid edge-of-expiry failures.
    private static readonly TimeSpan ExpiryBuffer = TimeSpan.FromSeconds(60);

    private readonly HttpClient _http;
    private readonly FirebaseOptions _options;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private string? _idToken;
    private string? _refreshToken;
    private string? _uid;
    private DateTime _expiresAtUtc;

    public FirebaseAuthService(HttpClient http, FirebaseOptions options)
    {
        _http = http;
        _options = options;
    }

    public bool IsSignedIn => !string.IsNullOrEmpty(_refreshToken);

    public string? CurrentUserId => _uid;

    public async Task SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var payload = new { email, password, returnSecureToken = true };

        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync(SignInUrl + _options.ApiKey, payload, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            throw new AuthenticationFailedException("Could not reach the server. Check your internet connection. " + ex.Message);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw new AuthenticationFailedException(await ReadErrorMessageAsync(response, cancellationToken).ConfigureAwait(false));

            var data = await response.Content
                .ReadFromJsonAsync<SignInResponse>(cancellationToken)
                .ConfigureAwait(false)
                ?? throw new AuthenticationFailedException("Empty sign-in response from server.");

            ApplyTokens(data.IdToken, data.RefreshToken, data.ExpiresIn, data.LocalId);
            await PersistAsync().ConfigureAwait(false);
        }
    }

    public async Task<string?> GetValidIdTokenAsync(CancellationToken cancellationToken = default)
    {
        if (IsTokenFresh())
            return _idToken;

        if (string.IsNullOrEmpty(_refreshToken))
            return null;

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Re-check: another caller may have refreshed while we waited.
            if (IsTokenFresh())
                return _idToken;

            await RefreshAsync(cancellationToken).ConfigureAwait(false);
            return _idToken;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        _idToken = await SecureStorage.Default.GetAsync(KeyIdToken).ConfigureAwait(false);
        _refreshToken = await SecureStorage.Default.GetAsync(KeyRefreshToken).ConfigureAwait(false);
        _uid = await SecureStorage.Default.GetAsync(KeyUid).ConfigureAwait(false);

        var expiry = await SecureStorage.Default.GetAsync(KeyExpiry).ConfigureAwait(false);
        _expiresAtUtc = long.TryParse(expiry, out var ticks)
            ? new DateTime(ticks, DateTimeKind.Utc)
            : default;

        return IsSignedIn;
    }

    public Task SignOutAsync()
    {
        _idToken = null;
        _refreshToken = null;
        _uid = null;
        _expiresAtUtc = default;

        SecureStorage.Default.Remove(KeyIdToken);
        SecureStorage.Default.Remove(KeyRefreshToken);
        SecureStorage.Default.Remove(KeyExpiry);
        SecureStorage.Default.Remove(KeyUid);

        return Task.CompletedTask;
    }

    private bool IsTokenFresh()
        => !string.IsNullOrEmpty(_idToken) && DateTime.UtcNow < _expiresAtUtc - ExpiryBuffer;

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = _refreshToken!
        });

        using (form)
        {
            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsync(RefreshUrl + _options.ApiKey, form, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (HttpRequestException)
            {
                // Offline: keep the session; caller handles a null token gracefully.
                return;
            }

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                {
                    // Refresh token revoked/expired → session is dead.
                    await SignOutAsync().ConfigureAwait(false);
                    return;
                }

                var data = await response.Content
                    .ReadFromJsonAsync<RefreshResponse>(cancellationToken)
                    .ConfigureAwait(false);

                if (data is null)
                {
                    await SignOutAsync().ConfigureAwait(false);
                    return;
                }

                ApplyTokens(data.IdToken, data.RefreshToken, data.ExpiresIn, data.UserId);
                await PersistAsync().ConfigureAwait(false);
            }
        }
    }

    private void ApplyTokens(string idToken, string refreshToken, string expiresInSeconds, string uid)
    {
        _idToken = idToken;
        _refreshToken = refreshToken;
        _uid = uid;

        var seconds = int.TryParse(expiresInSeconds, out var parsed) ? parsed : 3600;
        _expiresAtUtc = DateTime.UtcNow.AddSeconds(seconds);
    }

    private async Task PersistAsync()
    {
        await SecureStorage.Default.SetAsync(KeyIdToken, _idToken!).ConfigureAwait(false);
        await SecureStorage.Default.SetAsync(KeyRefreshToken, _refreshToken!).ConfigureAwait(false);
        await SecureStorage.Default.SetAsync(KeyUid, _uid!).ConfigureAwait(false);
        await SecureStorage.Default.SetAsync(KeyExpiry, _expiresAtUtc.Ticks.ToString()).ConfigureAwait(false);
    }

    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var envelope = await response.Content
                .ReadFromJsonAsync<ErrorEnvelope>(cancellationToken)
                .ConfigureAwait(false);

            return envelope?.Error?.Message switch
            {
                "EMAIL_NOT_FOUND" => "No account found for this email.",
                "INVALID_PASSWORD" => "Incorrect password.",
                "INVALID_LOGIN_CREDENTIALS" => "Invalid email or password.",
                "USER_DISABLED" => "This account has been disabled.",
                "TOO_MANY_ATTEMPTS_TRY_LATER" => "Too many attempts. Please try again later.",
                null or "" => "Sign-in failed. Please try again.",
                var message => message
            };
        }
        catch
        {
            return "Sign-in failed. Please try again.";
        }
    }

    // ---- REST response DTOs ----

    private sealed class SignInResponse
    {
        [JsonPropertyName("idToken")] public string IdToken { get; set; } = string.Empty;
        [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = string.Empty;
        [JsonPropertyName("expiresIn")] public string ExpiresIn { get; set; } = "3600";
        [JsonPropertyName("localId")] public string LocalId { get; set; } = string.Empty;
    }

    private sealed class RefreshResponse
    {
        [JsonPropertyName("id_token")] public string IdToken { get; set; } = string.Empty;
        [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")] public string ExpiresIn { get; set; } = "3600";
        [JsonPropertyName("user_id")] public string UserId { get; set; } = string.Empty;
    }

    private sealed class ErrorEnvelope
    {
        [JsonPropertyName("error")] public ErrorBody? Error { get; set; }

        public sealed class ErrorBody
        {
            [JsonPropertyName("message")] public string? Message { get; set; }
        }
    }
}

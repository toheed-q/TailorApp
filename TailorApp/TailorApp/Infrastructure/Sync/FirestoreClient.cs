using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using TailorApp.Application.Exceptions;
using TailorApp.Application.Interfaces;
using TailorApp.Application.Models;

namespace TailorApp.Infrastructure.Sync;

/// <summary>
/// REST implementation of <see cref="IFirestoreClient"/>. Converts between CLR
/// field maps and Firestore's typed-value JSON, authenticates every call with a
/// Bearer ID token, and retries transient failures (timeouts, 429, 5xx) with
/// exponential backoff. Non-transient failures throw <see cref="FirestoreException"/>.
/// </summary>
public sealed class FirestoreClient : IFirestoreClient
{
    private const int MaxAttempts = 3;

    private readonly HttpClient _http;
    private readonly FirebaseOptions _options;
    private readonly IFirebaseAuthService _auth;

    public FirestoreClient(HttpClient http, FirebaseOptions options, IFirebaseAuthService auth)
    {
        _http = http;
        _options = options;
        _auth = auth;
    }

    private string DocumentsBase =>
        $"https://firestore.googleapis.com/v1/projects/{_options.ProjectId}/databases/(default)/documents";

    public async Task UpsertAsync(string collection, string documentId,
        IReadOnlyDictionary<string, object?> fields, CancellationToken cancellationToken = default)
    {
        var (token, uid) = await RequireAuthAsync(cancellationToken).ConfigureAwait(false);

        var url = $"{DocumentsBase}/shops/{uid}/{collection}/{documentId}";
        var json = new JsonObject { ["fields"] = BuildFields(fields) }.ToJsonString();

        await SendWithRetryAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }, "upsert document", cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<FirestoreDocument>> GetChangedSinceAsync(string collection,
        DateTime sinceUtc, CancellationToken cancellationToken = default)
    {
        var (token, uid) = await RequireAuthAsync(cancellationToken).ConfigureAwait(false);

        var url = $"{DocumentsBase}/shops/{uid}:runQuery";
        var query = new JsonObject
        {
            ["structuredQuery"] = new JsonObject
            {
                ["from"] = new JsonArray { new JsonObject { ["collectionId"] = collection } },
                ["where"] = new JsonObject
                {
                    ["fieldFilter"] = new JsonObject
                    {
                        ["field"] = new JsonObject { ["fieldPath"] = "updatedAt" },
                        ["op"] = "GREATER_THAN",
                        ["value"] = new JsonObject { ["timestampValue"] = ToRfc3339(sinceUtc) }
                    }
                },
                ["orderBy"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["field"] = new JsonObject { ["fieldPath"] = "updatedAt" },
                        ["direction"] = "ASCENDING"
                    }
                }
            }
        }.ToJsonString();

        var responseBody = await SendWithRetryAsync(() =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(query, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }, "query documents", cancellationToken).ConfigureAwait(false);

        return ParseQueryResults(responseBody);
    }

    // ---- Auth ----

    private async Task<(string Token, string Uid)> RequireAuthAsync(CancellationToken cancellationToken)
    {
        var token = await _auth.GetValidIdTokenAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(_auth.CurrentUserId))
            throw new FirestoreException("Not signed in. Cloud sync requires the shop account to be signed in.");
        return (token, _auth.CurrentUserId!);
    }

    // ---- HTTP send with transient retry ----

    private async Task<string> SendWithRetryAsync(Func<HttpRequestMessage> requestFactory, string operation, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var request = requestFactory();
                using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                if (IsTransient(response.StatusCode) && attempt < MaxAttempts)
                {
                    await DelayForAttemptAsync(attempt, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                throw await BuildExceptionAsync(response, operation, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException) when (attempt < MaxAttempts)
            {
                // Transient transport error — back off and retry.
                await DelayForAttemptAsync(attempt, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                throw new FirestoreException("Could not reach Firestore. Check your internet connection.", ex);
            }
        }
    }

    private static Task DelayForAttemptAsync(int attempt, CancellationToken cancellationToken)
        => Task.Delay(TimeSpan.FromMilliseconds(500 * Math.Pow(2, attempt - 1)), cancellationToken);

    private static bool IsTransient(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.RequestTimeout => true,        // 408
        (HttpStatusCode)429 => true,                  // Too Many Requests
        HttpStatusCode.InternalServerError => true,   // 500
        HttpStatusCode.BadGateway => true,            // 502
        HttpStatusCode.ServiceUnavailable => true,    // 503
        HttpStatusCode.GatewayTimeout => true,        // 504
        _ => false
    };

    private static async Task<FirestoreException> BuildExceptionAsync(HttpResponseMessage response, string operation, CancellationToken cancellationToken)
    {
        string detail;
        try
        {
            var payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            detail = JsonNode.Parse(payload)?["error"]?["message"]?.GetValue<string>() ?? payload;
        }
        catch
        {
            detail = response.ReasonPhrase ?? "unknown error";
        }

        return new FirestoreException($"Firestore failed to {operation} ({(int)response.StatusCode}): {detail}");
    }

    // ---- Query parsing ----

    private static IReadOnlyList<FirestoreDocument> ParseQueryResults(string json)
    {
        if (JsonNode.Parse(json) is not JsonArray root)
            return Array.Empty<FirestoreDocument>();

        var results = new List<FirestoreDocument>();
        foreach (var entry in root)
        {
            // runQuery streams entries; some contain only readTime (no document).
            if (entry?["document"] is not JsonObject document)
                continue;

            var name = document["name"]?.GetValue<string>();
            if (string.IsNullOrEmpty(name))
                continue;

            var id = name[(name.LastIndexOf('/') + 1)..];
            var fields = ParseFields(document["fields"] as JsonObject);
            results.Add(new FirestoreDocument { Id = id, Fields = fields });
        }

        return results;
    }

    // ---- Typed-value conversion ----

    private static JsonObject BuildFields(IReadOnlyDictionary<string, object?> fields)
    {
        var result = new JsonObject();
        foreach (var (key, value) in fields)
            result[key] = BuildValue(value);
        return result;
    }

    private static JsonObject BuildValue(object? value) => value switch
    {
        null => new JsonObject { ["nullValue"] = null },
        string s => new JsonObject { ["stringValue"] = s },
        bool b => new JsonObject { ["booleanValue"] = b },
        int i => new JsonObject { ["integerValue"] = i.ToString(CultureInfo.InvariantCulture) },
        long l => new JsonObject { ["integerValue"] = l.ToString(CultureInfo.InvariantCulture) },
        double d => new JsonObject { ["doubleValue"] = d },
        float f => new JsonObject { ["doubleValue"] = (double)f },
        DateTime dt => new JsonObject { ["timestampValue"] = ToRfc3339(dt) },
        _ => new JsonObject { ["stringValue"] = value.ToString() }
    };

    private static IReadOnlyDictionary<string, object?> ParseFields(JsonObject? fields)
    {
        var result = new Dictionary<string, object?>();
        if (fields is null)
            return result;

        foreach (var (key, valueNode) in fields)
        {
            if (valueNode is JsonObject valueObject)
                result[key] = ParseValue(valueObject);
        }

        return result;
    }

    private static object? ParseValue(JsonObject value)
    {
        if (value.ContainsKey("nullValue"))
            return null;
        if (value.TryGetPropertyValue("stringValue", out var s))
            return s?.GetValue<string>();
        if (value.TryGetPropertyValue("booleanValue", out var b))
            return b?.GetValue<bool>();
        if (value.TryGetPropertyValue("integerValue", out var i))
            return long.Parse(i!.GetValue<string>(), CultureInfo.InvariantCulture);
        if (value.TryGetPropertyValue("doubleValue", out var d))
            return d?.GetValue<double>();
        if (value.TryGetPropertyValue("timestampValue", out var t))
            return DateTimeOffset.Parse(t!.GetValue<string>(), CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind).UtcDateTime;

        // Unsupported types (map/array/geo/etc.) are not used by this app.
        return null;
    }

    private static string ToRfc3339(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        return utc.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ", CultureInfo.InvariantCulture);
    }
}

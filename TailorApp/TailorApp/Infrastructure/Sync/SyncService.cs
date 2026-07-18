using Microsoft.Extensions.Logging;
using SQLite;
using TailorApp.Application.Interfaces;
using TailorApp.Application.Models;
using TailorApp.Domain.Entities;
using TailorApp.Domain.Enums;

namespace TailorApp.Infrastructure.Sync;

/// <summary>
/// Default <see cref="ISyncService"/>. Push uploads every row marked
/// <see cref="SyncStatus.Pending"/> and flips it to Synced; pull downloads rows
/// changed since a per-collection cursor (stored in <see cref="SyncMetadata"/>)
/// and applies them with last-writer-wins by <c>UpdatedAt</c>. A semaphore
/// prevents overlapping runs.
/// </summary>
public sealed class SyncService : ISyncService
{
    private const string CustomersCollection = "customers";
    private const string MeasurementsCollection = "measurementSets";
    private const string DesignsCollection = "designPreferences";
    private const string OrdersCollection = "orders";

    private readonly IDatabaseService _database;
    private readonly IFirestoreClient _firestore;
    private readonly IFirebaseAuthService _auth;
    private readonly IConnectivityService _connectivity;
    private readonly ILogger<SyncService> _logger;

    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private bool _autoSyncStarted;

    public SyncService(
        IDatabaseService database,
        IFirestoreClient firestore,
        IFirebaseAuthService auth,
        IConnectivityService connectivity,
        ILogger<SyncService> logger)
    {
        _database = database;
        _firestore = firestore;
        _auth = auth;
        _connectivity = connectivity;
        _logger = logger;
    }

    public bool IsSyncing { get; private set; }

    public event EventHandler<SyncResult>? SyncCompleted;

    public void StartAutoSync()
    {
        if (_autoSyncStarted)
            return;
        _autoSyncStarted = true;
        _connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    public async Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default)
    {
        // Skip silently if a run is already in progress.
        if (!await _syncLock.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            return SyncResult.Failed("A sync is already in progress.", DateTime.UtcNow);

        IsSyncing = true;
        try
        {
            if (!_connectivity.IsConnected)
                return Complete(SyncResult.Failed("No internet connection.", DateTime.UtcNow));

            // Signs in silently with the shop account the first time; afterwards
            // the persisted session is refreshed automatically. No login screen.
            if (!await _auth.EnsureSignedInAsync(cancellationToken).ConfigureAwait(false))
                return Complete(SyncResult.Failed("Could not sign in to the cloud.", DateTime.UtcNow));

            _logger.LogInformation("Sync started.");
            var connection = await _database.GetConnectionAsync().ConfigureAwait(false);

            // Push first so local edits win ties, then pull remote changes.
            var pushed = await PushAllAsync(connection, cancellationToken).ConfigureAwait(false);
            var pulled = await PullAllAsync(connection, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Sync completed: pushed {Pushed}, pulled {Pulled}.", pushed, pulled);
            return Complete(SyncResult.Ok(pushed, pulled, DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sync failed: {Message}", ex.Message);
            return Complete(SyncResult.Failed(ex.Message, DateTime.UtcNow));
        }
        finally
        {
            IsSyncing = false;
            _syncLock.Release();
        }
    }

    // ---- Push ----

    private async Task<int> PushAllAsync(SQLiteAsyncConnection connection, CancellationToken cancellationToken)
    {
        var pushed = 0;
        pushed += await PushTableAsync<Customer>(connection, CustomersCollection, CloudMappers.ToFields, cancellationToken).ConfigureAwait(false);
        pushed += await PushTableAsync<MeasurementSet>(connection, MeasurementsCollection, CloudMappers.ToFields, cancellationToken).ConfigureAwait(false);
        pushed += await PushTableAsync<DesignPreference>(connection, DesignsCollection, CloudMappers.ToFields, cancellationToken).ConfigureAwait(false);
        pushed += await PushTableAsync<Order>(connection, OrdersCollection, CloudMappers.ToFields, cancellationToken).ConfigureAwait(false);
        return pushed;
    }

    private async Task<int> PushTableAsync<T>(
        SQLiteAsyncConnection connection,
        string collection,
        Func<T, Dictionary<string, object?>> toFields,
        CancellationToken cancellationToken) where T : EntityBase, new()
    {
        var pending = await connection.Table<T>()
            .Where(e => e.SyncStatus == SyncStatus.Pending)
            .ToListAsync()
            .ConfigureAwait(false);

        foreach (var entity in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _firestore.UpsertAsync(collection, entity.Id, toFields(entity), cancellationToken).ConfigureAwait(false);

            entity.SyncStatus = SyncStatus.Synced;
            entity.SyncedAt = DateTime.UtcNow;
            await connection.UpdateAsync(entity).ConfigureAwait(false);
        }

        return pending.Count;
    }

    // ---- Pull ----

    private async Task<int> PullAllAsync(SQLiteAsyncConnection connection, CancellationToken cancellationToken)
    {
        var pulled = 0;
        pulled += await PullTableAsync(connection, CustomersCollection, CloudMappers.ToCustomer, cancellationToken).ConfigureAwait(false);
        pulled += await PullTableAsync(connection, MeasurementsCollection, CloudMappers.ToMeasurementSet, cancellationToken).ConfigureAwait(false);
        pulled += await PullTableAsync(connection, DesignsCollection, CloudMappers.ToDesignPreference, cancellationToken).ConfigureAwait(false);
        pulled += await PullTableAsync(connection, OrdersCollection, CloudMappers.ToOrder, cancellationToken).ConfigureAwait(false);
        return pulled;
    }

    private async Task<int> PullTableAsync<T>(
        SQLiteAsyncConnection connection,
        string collection,
        Func<FirestoreDocument, T> fromDocument,
        CancellationToken cancellationToken) where T : EntityBase, new()
    {
        var since = await GetCursorAsync(connection, collection).ConfigureAwait(false);
        var documents = await _firestore.GetChangedSinceAsync(collection, since, cancellationToken).ConfigureAwait(false);
        if (documents.Count == 0)
            return 0;

        var applied = 0;
        var maxUpdated = since;

        foreach (var document in documents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var incoming = fromDocument(document);
            if (incoming.UpdatedAt > maxUpdated)
                maxUpdated = incoming.UpdatedAt;

            var local = await connection.FindAsync<T>(incoming.Id).ConfigureAwait(false);

            // Last-writer-wins: apply only if remote is strictly newer (or new locally).
            if (local is null || incoming.UpdatedAt > local.UpdatedAt)
            {
                incoming.SyncStatus = SyncStatus.Synced;
                incoming.SyncedAt = DateTime.UtcNow;
                await connection.InsertOrReplaceAsync(incoming).ConfigureAwait(false);
                applied++;
            }
        }

        await SetCursorAsync(connection, collection, maxUpdated).ConfigureAwait(false);
        return applied;
    }

    // ---- Cursor (last successful pull timestamp per collection) ----

    private static async Task<DateTime> GetCursorAsync(SQLiteAsyncConnection connection, string collection)
    {
        var row = await connection.FindAsync<SyncMetadata>(CursorKey(collection)).ConfigureAwait(false);
        if (row?.Value is { } value &&
            DateTimeOffset.TryParse(value, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
        {
            return parsed.UtcDateTime;
        }

        return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
    }

    private static Task SetCursorAsync(SQLiteAsyncConnection connection, string collection, DateTime cursorUtc)
        => connection.InsertOrReplaceAsync(new SyncMetadata
        {
            Key = CursorKey(collection),
            Value = cursorUtc.ToString("o")
        });

    private static string CursorKey(string collection) => $"lastPull:{collection}";

    // ---- Helpers ----

    private SyncResult Complete(SyncResult result)
    {
        SyncCompleted?.Invoke(this, result);
        return result;
    }

    private async void OnConnectivityChanged(object? sender, bool isOnline)
    {
        // SyncAsync signs in silently when needed, so no auth check here.
        if (!isOnline)
            return;

        try
        {
            await SyncAsync().ConfigureAwait(false);
        }
        catch
        {
            // Auto-sync is best-effort; failures are reported via SyncCompleted.
        }
    }
}

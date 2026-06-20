using SQLite;
using TailorApp.Application.Interfaces;
using TailorApp.Domain.Entities;

namespace TailorApp.Infrastructure.Data;

/// <summary>
/// Default <see cref="IDatabaseService"/> implementation.
///
/// Holds one <see cref="SQLiteAsyncConnection"/> for the whole app lifetime
/// (registered as a singleton). Opening one connection and reusing it avoids
/// the per-open cost and keeps memory low on cheap devices. Initialization is
/// guarded by a semaphore so concurrent first-callers create the schema once.
/// </summary>
public sealed class DatabaseService : IDatabaseService
{
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private SQLiteAsyncConnection? _connection;

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
            return _connection;

        await InitializeAsync().ConfigureAwait(false);
        return _connection!;
    }

    public async Task InitializeAsync()
    {
        if (_connection is not null)
            return;

        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Double-check after acquiring the lock.
            if (_connection is not null)
                return;

            var connection = new SQLiteAsyncConnection(
                DatabaseConstants.DatabasePath,
                DatabaseConstants.Flags);

            // Schema creation is idempotent; indexes declared via [Indexed] on
            // entities are created here too (Name/PhoneNumber for fast search,
            // CustomerId / MeasurementSetId for history lookups).
            await connection.CreateTableAsync<Customer>().ConfigureAwait(false);
            await connection.CreateTableAsync<MeasurementSet>().ConfigureAwait(false);
            await connection.CreateTableAsync<DesignPreference>().ConfigureAwait(false);
            await connection.CreateTableAsync<SyncMetadata>().ConfigureAwait(false);

            _connection = connection;
        }
        finally
        {
            _initLock.Release();
        }
    }
}

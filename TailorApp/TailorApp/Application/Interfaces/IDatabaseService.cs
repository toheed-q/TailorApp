using SQLite;

namespace TailorApp.Application.Interfaces;

/// <summary>
/// Owns the single shared SQLite connection and schema lifecycle.
///
/// We deliberately do NOT use the Repository pattern. Feature services
/// (e.g. CustomerService) consume the connection returned here and run their
/// own typed queries directly — this is the "service layer over data access"
/// the project asks for, with one less abstraction to pay for in memory.
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    /// Returns the initialized, shared async connection, creating tables on
    /// first access. Safe to call concurrently.
    /// </summary>
    Task<SQLiteAsyncConnection> GetConnectionAsync();

    /// <summary>
    /// Eagerly initializes the database (file + schema). Optional to call;
    /// <see cref="GetConnectionAsync"/> initializes lazily on demand.
    /// </summary>
    Task InitializeAsync();
}

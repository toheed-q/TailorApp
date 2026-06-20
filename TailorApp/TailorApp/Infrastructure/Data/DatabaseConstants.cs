using SQLite;

namespace TailorApp.Infrastructure.Data;

/// <summary>
/// Central place for SQLite configuration. Keeping this here avoids magic
/// strings/flags scattered across the data layer.
/// </summary>
public static class DatabaseConstants
{
    public const string DatabaseFilename = "tailorapp.db3";

    /// <summary>
    /// ReadWrite + Create so the file is created on first run.
    /// SharedCache keeps memory usage down by sharing pages across connections.
    /// </summary>
    public const SQLiteOpenFlags Flags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    /// <summary>Full path to the database file in the app's private data directory.</summary>
    public static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
}

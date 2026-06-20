using SQLite;

namespace TailorApp.Domain.Entities;

/// <summary>
/// Key/value bookkeeping for the sync engine (e.g. the last successful pull
/// timestamp per collection). Kept as a tiny standalone table rather than
/// reusing <see cref="EntityBase"/>, because this state is local-only and
/// never itself synced to the cloud.
/// </summary>
[Table("SyncMetadata")]
public class SyncMetadata
{
    /// <summary>Logical key, e.g. "lastPull:customers".</summary>
    [PrimaryKey, MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    /// <summary>Opaque value (typically an ISO-8601 UTC timestamp).</summary>
    public string? Value { get; set; }
}

using SQLite;
using TailorApp.Domain.Enums;

namespace TailorApp.Domain.Entities;

/// <summary>
/// Base type for every persisted entity. Carries the identity and the
/// sync metadata required for offline-first Firestore synchronization
/// (last-writer-wins by <see cref="UpdatedAt"/>, soft-delete propagation
/// via <see cref="IsDeleted"/>, and dirty-row detection via <see cref="SyncStatus"/>).
///
/// Note: this base takes a lightweight dependency on the SQLite attribute
/// package only. We intentionally annotate domain entities directly instead
/// of maintaining separate persistence models, to keep memory and code low
/// on low-end Android devices.
/// </summary>
public abstract class EntityBase
{
    /// <summary>
    /// Stable identifier shared between SQLite and Firestore (GUID, no dashes).
    /// Generated client-side so records can be created fully offline.
    /// </summary>
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    /// <summary>UTC timestamp the record was first created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp of the last local modification. Drives conflict resolution.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Soft-delete flag. We never hard-delete, so deletions can be pushed to
    /// the cloud and reconciled across devices.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>Current cloud synchronization state of this record.</summary>
    public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;

    /// <summary>UTC timestamp the record was last successfully pushed to the cloud.</summary>
    public DateTime? SyncedAt { get; set; }
}

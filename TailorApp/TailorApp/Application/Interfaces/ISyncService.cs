using TailorApp.Application.Models;

namespace TailorApp.Application.Interfaces;

/// <summary>
/// Orchestrates offline-first synchronization between SQLite (source of truth)
/// and Firestore (mirror): pushes locally-changed rows, pulls remote changes,
/// and resolves conflicts by last-writer-wins on <c>UpdatedAt</c>.
/// </summary>
public interface ISyncService
{
    /// <summary>True while a sync run is in progress.</summary>
    bool IsSyncing { get; }

    /// <summary>Raised when a sync run finishes (success or failure).</summary>
    event EventHandler<SyncResult>? SyncCompleted;

    /// <summary>Push local changes, then pull remote changes. Never throws — failures come back in the result.</summary>
    Task<SyncResult> SyncAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins listening for connectivity changes to auto-sync when the device
    /// comes online. Idempotent.
    /// </summary>
    void StartAutoSync();
}

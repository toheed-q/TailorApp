namespace TailorApp.Domain.Enums;

/// <summary>
/// Tracks the cloud (Firestore) synchronization state of a record.
/// Used by the sync engine (Phase 5) to find and reconcile dirty rows.
/// </summary>
public enum SyncStatus
{
    Pending = 0,
    Synced = 1,
    Failed = 2
}

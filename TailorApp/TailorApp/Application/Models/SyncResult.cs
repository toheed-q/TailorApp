namespace TailorApp.Application.Models;

/// <summary>
/// Outcome of a sync run, surfaced to the UI ("Sync now" + status line).
/// </summary>
public sealed class SyncResult
{
    public bool Success { get; init; }
    public int Pushed { get; init; }
    public int Pulled { get; init; }
    public string? Error { get; init; }
    public DateTime CompletedAtUtc { get; init; }

    public static SyncResult Ok(int pushed, int pulled, DateTime completedAtUtc) => new()
    {
        Success = true,
        Pushed = pushed,
        Pulled = pulled,
        CompletedAtUtc = completedAtUtc
    };

    public static SyncResult Failed(string error, DateTime completedAtUtc) => new()
    {
        Success = false,
        Error = error,
        CompletedAtUtc = completedAtUtc
    };
}

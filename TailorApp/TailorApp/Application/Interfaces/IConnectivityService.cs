namespace TailorApp.Application.Interfaces;

/// <summary>
/// Abstraction over network availability. Lets the sync engine react to the
/// device coming online without taking a hard dependency on MAUI Essentials,
/// keeping the Application layer testable.
/// </summary>
public interface IConnectivityService
{
    /// <summary>True if the device currently has internet access.</summary>
    bool IsConnected { get; }

    /// <summary>
    /// Raised when connectivity changes. The bool payload is the new
    /// <see cref="IsConnected"/> value (true = came online).
    /// </summary>
    event EventHandler<bool>? ConnectivityChanged;
}

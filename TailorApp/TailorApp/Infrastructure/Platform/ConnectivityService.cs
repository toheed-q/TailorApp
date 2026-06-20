using Microsoft.Maui.Networking;
using TailorApp.Application.Interfaces;

namespace TailorApp.Infrastructure.Platform;

/// <summary>
/// MAUI Essentials-backed <see cref="IConnectivityService"/>. Wraps
/// <see cref="Connectivity"/> and re-raises its change events as a simple
/// online/offline bool the sync engine can subscribe to.
/// </summary>
public sealed class ConnectivityService : IConnectivityService, IDisposable
{
    private readonly IConnectivity _connectivity;

    public ConnectivityService(IConnectivity connectivity)
    {
        _connectivity = connectivity;
        _connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    public bool IsConnected =>
        _connectivity.NetworkAccess == NetworkAccess.Internet;

    public event EventHandler<bool>? ConnectivityChanged;

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        => ConnectivityChanged?.Invoke(this, e.NetworkAccess == NetworkAccess.Internet);

    public void Dispose()
        => _connectivity.ConnectivityChanged -= OnConnectivityChanged;
}

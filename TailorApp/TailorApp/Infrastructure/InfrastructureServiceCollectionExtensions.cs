using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Networking;
using TailorApp.Application.Interfaces;
using TailorApp.Infrastructure.Data;
using TailorApp.Infrastructure.Platform;
using TailorApp.Infrastructure.Services;
using TailorApp.Infrastructure.Sync;

namespace TailorApp.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer. Keeps DI wiring out of
/// <c>MauiProgram</c> and gives each layer one place to register itself.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Singleton: one shared SQLite connection for the whole app lifetime.
        services.AddSingleton<IDatabaseService, DatabaseService>();

        // Platform abstractions.
        services.AddSingleton(Connectivity.Current);
        services.AddSingleton<IConnectivityService, ConnectivityService>();
        services.AddSingleton<ILocalizationService, LocalizationService>();
        services.AddSingleton<IAppSettingsService, AppSettingsService>();

        // Feature services. Stateless, so singleton keeps allocations low.
        services.AddSingleton<ICustomerService, CustomerService>();
        services.AddSingleton<IMeasurementService, MeasurementService>();
        services.AddSingleton<IOrderService, OrderService>();

        // ---- Cloud sync (Phase 5) ----
        // API key is a public client identifier; data access is gated by Auth +
        // Firestore security rules, not by hiding this value.
        // The shop account below must exist in Firebase Console → Authentication
        // (Email/Password provider). The app signs in with it silently, so there
        // is no login screen and all installs share one stable uid.
        services.AddSingleton(new FirebaseOptions
        {
            ApiKey = "AIzaSyC4eb73mljQpnpWk96gv6pPsFPRRMaiXkI",
            ProjectId = "tailorapp-6b63d",
            ShopEmail = "shani@gmail.com",
            ShopPassword = "shani112233$"
        });

        // One shared HttpClient for the Firebase REST calls.
        services.AddSingleton<HttpClient>();
        services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
        services.AddSingleton<IFirestoreClient, FirestoreClient>();
        services.AddSingleton<ISyncService, SyncService>();

        return services;
    }
}

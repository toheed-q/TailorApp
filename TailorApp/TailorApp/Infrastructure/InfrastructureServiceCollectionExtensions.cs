using Microsoft.Extensions.DependencyInjection;
using TailorApp.Application.Interfaces;
using TailorApp.Infrastructure.Data;
using TailorApp.Infrastructure.Services;

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

        // Feature services. Stateless, so singleton keeps allocations low.
        services.AddSingleton<ICustomerService, CustomerService>();
        services.AddSingleton<IMeasurementService, MeasurementService>();

        // SyncService, … registered here in later phases.

        return services;
    }
}

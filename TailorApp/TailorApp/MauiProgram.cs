using Microsoft.Extensions.Logging;
using TailorApp.Application.Interfaces;
using TailorApp.Infrastructure;

namespace TailorApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Layer composition roots (Infrastructure registers SQLite + services).
            builder.Services.AddInfrastructure();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Resume a saved session and begin auto-sync when the device is online.
            // Fire-and-forget: startup must not block on the network.
            _ = InitializeAsync(app.Services);

            return app;
        }

        private static async Task InitializeAsync(IServiceProvider services)
        {
            try
            {
                // Apply the saved language/culture before any UI renders.
                services.GetRequiredService<ILocalizationService>().Initialize();

                await services.GetRequiredService<IFirebaseAuthService>()
                    .TryRestoreSessionAsync();

                services.GetRequiredService<ISyncService>().StartAutoSync();
            }
            catch
            {
                // Best-effort startup init; the app remains fully usable offline.
            }
        }
    }
}

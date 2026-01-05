using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using apppasteleriav04.Data.Local.Database;
using apppasteleriav04.Data.Local.Repositories;
using apppasteleriav04.Services.Connectivity;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Services.Sync;

namespace apppasteleriav04
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiMaps()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register Database
            builder.Services.AddSingleton<AppDatabase>(sp => AppDatabase.Instance);

            // Register Repositories
            builder.Services.AddSingleton<LocalProductRepository>();
            builder.Services.AddSingleton<LocalOrderRepository>();

            // Register Core Services
            builder.Services.AddSingleton<SupabaseService>(sp => SupabaseService.Instance);
            builder.Services.AddSingleton<AuthService>(sp => AuthService.Instance);

            // Register Connectivity Service
            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();

            // Register Sync Service
            builder.Services.AddSingleton<ISyncService, SyncService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Initialize database and start auto-sync on startup
            Task.Run(async () =>
            {
                try
                {
                    var database = app.Services.GetRequiredService<AppDatabase>();
                    await database.InitializeAsync();

                    // Start auto-sync
                    var syncService = app.Services.GetRequiredService<ISyncService>();
                    syncService.StartAutoSync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MauiProgram] Error initializing services: {ex}");
                }
            });

            return app;
        }
    }
}

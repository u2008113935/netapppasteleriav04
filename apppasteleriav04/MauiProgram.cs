// REEMPLAZAR COMPLETO
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using apppasteleriav04.Services.Payment;
using apppasteleriav04.Services.Billing;
using apppasteleriav04.Services.Connectivity;
using apppasteleriav04.Services.Sync;
using apppasteleriav04.Data.Local.Database;

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

            // Register Services
            builder.Services.AddSingleton<IPaymentService, PaymentService>();
            builder.Services.AddSingleton<IBillingService, BillingService>();
            builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
            builder.Services.AddSingleton<ISyncService, SyncService>();
            
            // Core services are already singletons, no need to register them here
            // AuthService.Instance, SupabaseService.Instance, etc. are accessed directly

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // Initialize database on startup
            // Note: This is fire-and-forget, but database initialization is fast and services handle uninitialized state gracefully
            Task.Run(async () =>
            {
                try
                {
                    await AppDatabase.Instance.InitializeAsync();
                    System.Diagnostics.Debug.WriteLine("[MauiProgram] Database initialized successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[MauiProgram] Database initialization error: {ex}");
                }
            });

            return app;
        }
    }
}
using apppasteleriav04.Services.Core;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using apppasteleriav04.Data.Local.Database;

namespace apppasteleriav04
{
    public partial class App : Application
    {
        public static bool IsInitialized { get; private set; } = false;
        public static SupabaseService Database => SupabaseService.Instance;

        public App()
        {
            InitializeComponent();

            ShowDatabaseLocation();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var splashPage = new Views.Shared.SplashPage();
            var window = new Window(splashPage);
            splashPage.SetWindow(window);
            return window;
        }

        //Ubicacion exacta de la base de datos local
        private void ShowDatabaseLocation()
        {
            try
            {
                var dbPath = DatabaseConstants.DatabasePath;
                var appDataDir = FileSystem.AppDataDirectory;
                //System.Diagnostics.Debug.WriteLine($"[App] Database path:  {dbPath}");
                System.Diagnostics.Debug.WriteLine($"DB: {dbPath} | {DeviceInfo.Platform} ({DeviceInfo.VersionString}) " +
                    $"| Existe: {File.Exists(dbPath)}{(File.Exists(dbPath) ? $" | " +
                    $"{new FileInfo(dbPath).Length / 1024:F1} KB" : "")}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error DB location:  {ex.Message}");
            }

        }



        public static async Task InitializeAppAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[App] Cargando estado de autenticacion...");

                // ========== TEMPORAL PARA TESTING:  LIMPIAR DATOS ==========
                // Descomenta estas lineas para forzar un inicio limpio
                // SecureStorage.Default. RemoveAll();
                // System. Diagnostics.Debug. WriteLine("[App] SecureStorage limpiado para testing");
                // ===========================================================

                await AuthService.Instance.LoadFromStorageAsync();

                // Log del estado actual
                AuthService.Instance.LogCurrentState();

                var token = await AuthService.Instance.GetAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    SupabaseService.Instance.SetUserToken(token);
                    System.Diagnostics.Debug.WriteLine("[App] Token cargado y configurado");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[App] No se encontro token almacenado");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] InitializeAppAsync error:  {ex}");
            }
            finally
            {
                IsInitialized = true;
            }
        }
    }
}
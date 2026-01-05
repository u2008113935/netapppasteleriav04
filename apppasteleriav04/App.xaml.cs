using apppasteleriav04.Services.Core;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace apppasteleriav04
{
    public partial class App : Application
    {
        public static bool IsInitialized { get; private set; } = false;
        public static SupabaseService Database => SupabaseService.Instance;

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var splashPage = new Views.Shared.SplashPage();
            var window = new Window(splashPage);
            splashPage.SetWindow(window);
            return window;
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
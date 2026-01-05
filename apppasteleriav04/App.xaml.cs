using apppasteleriav04.Services.Core;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

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

            // Pasar la referencia de window al SplashPage
            splashPage.SetWindow(window);

            return window;
        }

        // Inicialización asíncrona de la aplicación (sin parámetro Window)
        public static async Task InitializeAppAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[App] Cargando estado de autenticación...");
                await AuthService.Instance.LoadFromStorageAsync();

                var token = await AuthService.Instance.GetAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    SupabaseService.Instance.SetUserToken(token);
                    System.Diagnostics.Debug.WriteLine("[App] Token cargado y configurado");
                    System.Diagnostics.Debug.WriteLine($"[App] Usuario autenticado: {AuthService.Instance.UserEmail}");
                    System.Diagnostics.Debug.WriteLine($"[App] IsAuthenticated: {AuthService.Instance.IsAuthenticated}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[App] No se encontró token almacenado");
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
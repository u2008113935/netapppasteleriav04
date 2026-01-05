using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace apppasteleriav04.Views.Shared
{
    public partial class SplashPage : ContentPage
    {
        private Window? _window;

        public SplashPage()
        {
            InitializeComponent();
        }

        public void SetWindow(Window window)
        {
            _window = window;
            _ = InitializeAndNavigate();
        }

        private async Task InitializeAndNavigate()
        {
            try
            {
                // Esperar inicialización
                await App.InitializeAppAsync();

                // Espera adicional para que el splash dure al menos 3 segundos
                await Task.Delay(3000);

                // Navegar al Shell principal en el hilo de UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (_window != null)
                    {
                        _window.Page = new AppShell();
                        System.Diagnostics.Debug.WriteLine("[SplashPage] Navegación a AppShell completada");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SplashPage] Error:  {ex}");

                // En caso de error, navegar igual a AppShell
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (_window != null)
                    {
                        _window.Page = new AppShell();
                    }
                });
            }
        }
    }
}
using System.Threading.Tasks;

namespace apppasteleriav04.Views.Shared
{
    public partial class SplashPage : ContentPage
    {
        public SplashPage()
        {
            InitializeComponent();
            _ = InitializeAndNavigate();
        }

        private async Task InitializeAndNavigate()
        {
            // Esperar inicialización (si tu método tarda menos de 5 segundos)
            await App.InitializeAppAsync();

            // Espera adicional para que el splash dure al menos 5 segundos
            await Task.Delay(5000);

            // Navegar al Shell principal
            Application.Current!.MainPage = new AppShell();
        }
    }
}
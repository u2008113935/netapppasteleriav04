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
            // Esperar inicialización
            await App.InitializeAppAsync();

            // Navegar al Shell principal
            Application.Current!.MainPage = new AppShell();
        }
    }
}
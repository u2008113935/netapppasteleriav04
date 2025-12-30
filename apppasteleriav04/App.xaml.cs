using apppasteleriav04.Services;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace apppasteleriav04
{
    public partial class App : Application
    {
        public static bool IsInitialized { get; private set; } = false;
        public static SupabaseService Database { get; } = new SupabaseService();

        public App()

        {
            InitializeComponent();

            // Inicialización asíncrona (por ejemplo: cargar token)
            InitializeAsync();

            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Mostrar SplashPage primero
            return new Window(new Views.Shared.SplashPage());
        }

        /// <summary>
        /// Inicializar datos de la app (mínimo 2 segundos para mostrar splash)
        /// </summary>
        public static async Task InitializeAppAsync()
        {
            // Simular carga
            await Task.Delay(2000);

            // TODO: Cargar datos iniciales
            IsInitialized = true;
        }

        async void InitializeAsync()
        {
            try
            {
                await AuthService.Instance.LoadFromStorageAsync();
                var token = await AuthService.Instance.GetAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                    SupabaseService.Instance.SetUserToken(token);
            }
            catch { }
        }
    }
}
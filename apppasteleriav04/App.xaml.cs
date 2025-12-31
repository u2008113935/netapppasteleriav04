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
        public static SupabaseService Database { get; } = new SupabaseService();

        public App()

        {
            InitializeComponent();
            MainPage = new Views.Shared.SplashPage();  // ⬅ Splash primero
            //InitializeAppAsync(); // Maneja la "transición" al real
        }

        // Inicialización asíncrona de la aplicación
        public static async Task InitializeAppAsync()
        {
            // Simular carga
            await Task.Delay(2000);
      
            try
            {
                await AuthService.Instance.LoadFromStorageAsync();
                var token = await AuthService.Instance.GetAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                    SupabaseService.Instance.SetUserToken(token);
            }
            catch { }

            IsInitialized = true;
        }
    }
}
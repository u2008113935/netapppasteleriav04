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
                await AuthService.Instance.LoadFromStorageAsync();

                var token = await AuthService.Instance.GetAccessTokenAsync();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    SupabaseService.Instance.SetUserToken(token);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] InitializeAppAsync error: {ex.Message}");
            }
            finally
            {
                IsInitialized = true;
            }
        }
    }
}
using System.Threading.Tasks;

namespace apppasteleriav04
{
    public partial class App : Application
    {
        public static bool IsInitialized { get; private set; } = false;

        public App()
        {
            InitializeComponent();
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
    }
}
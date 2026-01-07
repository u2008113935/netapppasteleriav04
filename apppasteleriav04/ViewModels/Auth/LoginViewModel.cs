using System;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Auth
{
    public class LoginViewModel : BaseViewModel
    {
        
        //Inyección de dependencias (mejor práctica MVVM)
        private readonly AuthService _authService;
        private readonly CartService _cartService;

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        //Propiedad para mostrar mensaje de carrito restaurado
        private string _cartRestoredMessage = string.Empty;
        public string CartRestoredMessage
        {
            get => _cartRestoredMessage;
            set => SetProperty(ref _cartRestoredMessage, value);
        }

        // Events
        public event EventHandler<LoginCompletedEventArgs>? LoginCompleted;
        public event EventHandler? RegisterRequested;

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            Title = "Iniciar Sesión";

            // Usar Singleton (patrón actual del proyecto)
            _authService = AuthService.Instance;
            _cartService = CartService.Instance;

            LoginCommand = new AsyncRelayCommand(LoginAsync, () => !IsLoading);
            RegisterCommand = new RelayCommand(() => RegisterRequested?.Invoke(this, EventArgs.Empty));
        }

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;
            CartRestoredMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Por favor ingrese su correo electrónico";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Por favor ingrese su contraseña";
                return;
            }

            IsLoading = true;
            IsBusy = true;

            try
            {
                // PASO 1: Autenticar usuario
                var success = await AuthService.Instance.SignInAsync(Email, Password);

                if (success)
                {
                    // PASO 2: Cargar carrito guardado (RESPONSABILIDAD DEL VIEWMODEL)
                    await LoadCartAfterLoginAsync();

                    // PASO 3: Notificar éxito a la View
                    LoginCompleted?.Invoke(this, new LoginCompletedEventArgs
                    {
                        Success = true,
                        Message = "Sesión iniciada correctamente",
                        UserId = AuthService.Instance.UserId?.ToString(),
                        Email = Email
                    });
                }
                else
                {
                    ErrorMessage = "Credenciales inválidas. Por favor intente nuevamente.";
                    LoginCompleted?.Invoke(this, new LoginCompletedEventArgs
                    {
                        Success = false,
                        Message = ErrorMessage
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al iniciar sesión: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                IsBusy = false;
            }
        }

        // NUEVO: Método para cargar carrito tras login (LÓGICA EN VIEWMODEL)
        private async Task LoadCartAfterLoginAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[LoginViewModel] Cargando carrito guardado...");

                // Cargar carrito desde Preferences
                await _cartService.LoadLocalAsync();

                var itemCount = _cartService.Count;
                var totalAmount = _cartService.Total;

                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Carrito cargado: {itemCount} items, Total: S/ {totalAmount: F2}");

                // ✅ Actualizar propiedad para mostrar en UI (opcional)
                if (itemCount > 0)
                {
                    CartRestoredMessage = $"Se restauraron {itemCount} productos (S/ {totalAmount:F2})";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Error cargando carrito: {ex.Message}");
                // No lanzar excepción - la carga del carrito no debe impedir el login
            }
        }


    }
}

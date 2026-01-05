using System;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Auth
{
    public class LoginViewModel : BaseViewModel
    {
        private string _email = string.Empty;
        private string _password = string.Empty;
        private bool _isLoading;
        private AsyncRelayCommand? _loginCommand;

        public event EventHandler<LoginCompletedEventArgs>? LoginCompleted;

        /// <summary>
        /// Gets or sets the email address
        /// </summary>
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    _loginCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the password
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    _loginCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a login operation is in progress
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Command to perform login
        /// </summary>
        public ICommand LoginCommand => _loginCommand ??= new AsyncRelayCommand(LoginAsync, CanLogin);

        /// <summary>
        /// Command to navigate to registration
        /// </summary>
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            Title = "Iniciar sesión";
            RegisterCommand = new RelayCommand(OnRegister);
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password) && !IsLoading;
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Ingrese correo y contraseña";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Intentando login con: {Email}");

                bool success = await AuthService.Instance.SignInAsync(Email.Trim(), Password);

                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Resultado del login: {success}");
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] IsAuthenticated: {AuthService.Instance.IsAuthenticated}");
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] UserEmail: {AuthService.Instance.UserEmail}");
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] UserId: {AuthService.Instance.UserId}");

                if (success && AuthService.Instance.IsAuthenticated)
                {
                    // Raise event to notify the view
                    LoginCompleted?.Invoke(this, new LoginCompletedEventArgs(true, string.Empty));
                }
                else
                {
                    ErrorMessage = "Credenciales incorrectas";
                    LoginCompleted?.Invoke(this, new LoginCompletedEventArgs(false, "Credenciales incorrectas"));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginViewModel] Error en login: {ex}");
                ErrorMessage = $"Error de conexión: {ex.Message}";
                LoginCompleted?.Invoke(this, new LoginCompletedEventArgs(false, ex.Message));
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnRegister()
        {
            // This will be handled by the view for navigation
            System.Diagnostics.Debug.WriteLine("[LoginViewModel] Register clicked");
        }
    }

    /// <summary>
    /// Event args for login completion
    /// </summary>
    public class LoginCompletedEventArgs : EventArgs
    {
        public bool Success { get; }
        public string Message { get; }

        public LoginCompletedEventArgs(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}

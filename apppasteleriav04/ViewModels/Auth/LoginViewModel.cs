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

        public event EventHandler<LoginCompletedEventArgs>? LoginCompleted;
        public event EventHandler? RegisterRequested;

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            Title = "Iniciar Sesión";
            LoginCommand = new AsyncRelayCommand(LoginAsync, () => !IsLoading);
            RegisterCommand = new RelayCommand(() => RegisterRequested?.Invoke(this, EventArgs.Empty));
        }

        private async Task LoginAsync()
        {
            ErrorMessage = string.Empty;

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
                var success = await AuthService.Instance.SignInAsync(Email, Password);

                if (success)
                {
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
    }
}

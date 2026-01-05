using System;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Auth
{
    public class RegisterViewModel : BaseViewModel
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

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public event EventHandler? RegistrationCompleted;
        public event EventHandler? GoToLoginRequested;

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public RegisterViewModel()
        {
            Title = "Registrarse";
            RegisterCommand = new AsyncRelayCommand(RegisterAsync, () => !IsBusy);
            GoToLoginCommand = new RelayCommand(() => GoToLoginRequested?.Invoke(this, EventArgs.Empty));
        }

        private async Task RegisterAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "Por favor ingrese su nombre completo";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Por favor ingrese su correo electrónico";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Por favor ingrese una contraseña";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "La contraseña debe tener al menos 6 caracteres";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Las contraseñas no coinciden";
                return;
            }

            IsBusy = true;

            try
            {
                var success = await AuthService.Instance.SignUpAsync(Email, Password, FullName, Phone);

                if (success)
                {
                    RegistrationCompleted?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ErrorMessage = "No se pudo crear la cuenta. Por favor intente nuevamente.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al registrarse: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

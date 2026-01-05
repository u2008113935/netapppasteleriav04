using System;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Profile
{
    public class ProfileViewModel : BaseViewModel
    {
        private UserProfile? _userProfile;
        public UserProfile? UserProfile
        {
            get => _userProfile;
            set => SetProperty(ref _userProfile, value);
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public bool IsAuthenticated => AuthService.Instance.IsAuthenticated;

        public ICommand LogoutCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand LoadProfileCommand { get; }

        public event EventHandler? AuthenticationRequired;
        public event EventHandler? LogoutCompleted;
        public event EventHandler? EditProfileRequested;

        public ProfileViewModel()
        {
            Title = "Perfil";
            LogoutCommand = new RelayCommand(Logout);
            EditProfileCommand = new RelayCommand(() => EditProfileRequested?.Invoke(this, EventArgs.Empty));
            LoadProfileCommand = new AsyncRelayCommand(LoadProfileAsync);
        }

        public bool CheckAuthentication()
        {
            if (!AuthService.Instance.IsAuthenticated)
            {
                AuthenticationRequired?.Invoke(this, EventArgs.Empty);
                return false;
            }
            return true;
        }

        public async Task LoadProfileAsync()
        {
            if (!IsAuthenticated)
            {
                ErrorMessage = "No ha iniciado sesión";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                Email = AuthService.Instance.UserEmail ?? string.Empty;
                
                var userId = AuthService.Instance.UserId;
                if (!string.IsNullOrEmpty(userId))
                {
                    var profile = await SupabaseService.Instance.GetProfileAsync(Guid.Parse(userId));
                    UserProfile = profile;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar perfil: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Logout()
        {
            AuthService.Instance.SignOut();
            UserProfile = null;
            Email = string.Empty;
            OnPropertyChanged(nameof(IsAuthenticated));
            LogoutCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}

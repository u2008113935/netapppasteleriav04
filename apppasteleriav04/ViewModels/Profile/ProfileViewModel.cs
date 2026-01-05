using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Core;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Profile
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly SupabaseService _supabase = SupabaseService.Instance;
        private UserProfile? _userProfile;
        private string _email = string.Empty;
        private bool _isAuthenticated;
        private string _fullName = string.Empty;
        private string _avatarUrl = string.Empty;
        private bool _isEditing;

        public event EventHandler? LogoutCompleted;
        public event EventHandler? AuthenticationRequired;

        /// <summary>
        /// Gets or sets the user profile
        /// </summary>
        public UserProfile? UserProfile
        {
            get => _userProfile;
            set => SetProperty(ref _userProfile, value);
        }

        /// <summary>
        /// Gets or sets the user email
        /// </summary>
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is authenticated
        /// </summary>
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set => SetProperty(ref _isAuthenticated, value);
        }

        /// <summary>
        /// Gets or sets the full name
        /// </summary>
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        /// <summary>
        /// Gets or sets the avatar URL
        /// </summary>
        public string AvatarUrl
        {
            get => _avatarUrl;
            set => SetProperty(ref _avatarUrl, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the profile is in edit mode
        /// </summary>
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        /// <summary>
        /// Command to logout
        /// </summary>
        public ICommand LogoutCommand { get; }

        /// <summary>
        /// Command to edit profile
        /// </summary>
        public ICommand EditProfileCommand { get; }

        /// <summary>
        /// Command to save profile
        /// </summary>
        public ICommand SaveProfileCommand { get; }

        /// <summary>
        /// Command to cancel editing
        /// </summary>
        public ICommand CancelEditCommand { get; }

        public ProfileViewModel()
        {
            Title = "Perfil";
            LogoutCommand = new AsyncRelayCommand(LogoutAsync);
            EditProfileCommand = new RelayCommand(StartEditing);
            SaveProfileCommand = new AsyncRelayCommand(SaveProfileAsync);
            CancelEditCommand = new RelayCommand(CancelEditing);
        }

        /// <summary>
        /// Check authentication status
        /// </summary>
        public bool CheckAuthentication()
        {
            System.Diagnostics.Debug.WriteLine($"[ProfileViewModel] CheckAuthentication");
            System.Diagnostics.Debug.WriteLine($"[ProfileViewModel] IsAuthenticated: {AuthService.Instance.IsAuthenticated}");
            System.Diagnostics.Debug.WriteLine($"[ProfileViewModel] UserId: {AuthService.Instance.UserId}");

            IsAuthenticated = AuthService.Instance.IsAuthenticated;

            if (!IsAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("[ProfileViewModel] Usuario NO autenticado");
                AuthenticationRequired?.Invoke(this, EventArgs.Empty);
                return false;
            }

            Email = AuthService.Instance.UserEmail ?? string.Empty;
            return true;
        }

        /// <summary>
        /// Load profile data
        /// </summary>
        public async Task LoadProfileAsync()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var userIdStr = AuthService.Instance.UserId;

                if (string.IsNullOrEmpty(userIdStr))
                {
                    userIdStr = await Microsoft.Maui.Storage.SecureStorage.Default.GetAsync("auth_user_id");
                }

                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                {
                    ErrorMessage = "No se encontró sesión. Inicia sesión primero.";
                    AuthenticationRequired?.Invoke(this, EventArgs.Empty);
                    return;
                }

                UserProfile = await _supabase.GetProfileAsync(userId);
                
                if (UserProfile == null)
                {
                    ErrorMessage = "Perfil no encontrado. Puedes crear tus datos.";
                    FullName = string.Empty;
                    AvatarUrl = string.Empty;
                }
                else
                {
                    FullName = UserProfile.FullName ?? string.Empty;
                    AvatarUrl = UserProfile.AvatarUrl ?? string.Empty;
                    System.Diagnostics.Debug.WriteLine($"[ProfileViewModel] Perfil cargado - {Email}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error cargando perfil: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[ProfileViewModel] Error: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void StartEditing()
        {
            if (!AuthService.Instance.IsAuthenticated)
            {
                ErrorMessage = "Debes iniciar sesión para editar.";
                return;
            }

            IsEditing = true;
        }

        private void CancelEditing()
        {
            if (UserProfile != null)
            {
                FullName = UserProfile.FullName ?? string.Empty;
                AvatarUrl = UserProfile.AvatarUrl ?? string.Empty;
            }
            else
            {
                FullName = string.Empty;
                AvatarUrl = string.Empty;
            }

            IsEditing = false;
            ErrorMessage = string.Empty;
        }

        private async Task SaveProfileAsync()
        {
            if (string.IsNullOrEmpty(FullName?.Trim()))
            {
                ErrorMessage = "El nombre no puede estar vacío.";
                return;
            }

            if (!AuthService.Instance.IsAuthenticated || string.IsNullOrEmpty(AuthService.Instance.UserId))
            {
                ErrorMessage = "Sesión no válida. Inicia sesión nuevamente.";
                AuthenticationRequired?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (!Guid.TryParse(AuthService.Instance.UserId, out var userId))
            {
                ErrorMessage = "UserId inválido.";
                return;
            }

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                var payload = new { full_name = FullName.Trim(), avatar_url = string.IsNullOrEmpty(AvatarUrl?.Trim()) ? null : AvatarUrl.Trim() };
                
                var baseUrl = SupabaseConfig.SUPABASE_URL.TrimEnd('/');
                var url = $"{baseUrl}/rest/v1/profiles?id=eq.{Uri.EscapeDataString(userId.ToString())}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("apikey", SupabaseConfig.SUPABASE_ANON_KEY);

                if (!string.IsNullOrEmpty(AuthService.Instance.AccessToken))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthService.Instance.AccessToken);
                else
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseConfig.SUPABASE_ANON_KEY);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                using var req = new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = content };
                req.Headers.Add("Prefer", "return=representation");

                var resp = await client.SendAsync(req);
                var respText = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    ErrorMessage = $"No se pudo actualizar el perfil: {resp.StatusCode}";
                    System.Diagnostics.Debug.WriteLine($"[ProfileViewModel] Error: {respText}");
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var list = JsonSerializer.Deserialize<UserProfile[]>(respText, options);
                    
                    if (list != null && list.Length > 0)
                    {
                        UserProfile = list[0];
                        FullName = UserProfile.FullName ?? string.Empty;
                        AvatarUrl = UserProfile.AvatarUrl ?? string.Empty;
                    }

                    IsEditing = false;
                    System.Diagnostics.Debug.WriteLine("[ProfileViewModel] Perfil actualizado correctamente");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[ProfileViewModel] Exception: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LogoutAsync()
        {
            // Logout logic would go here
            await AuthService.Instance.SignOutAsync();
            LogoutCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}

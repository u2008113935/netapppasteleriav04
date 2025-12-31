using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Diagnostics;

namespace apppasteleriav04.Services.Core
{
    public class AuthUser
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
    }

    public class AuthService
    {
        public static AuthService Instance { get; } = new AuthService();

        const string TokenKey = "auth_token";
        const string RefreshKey = "auth_refresh";
        const string UserIdKey = "auth_user_id";

        public string? AccessToken { get; private set; }
        public string? UserId { get; private set; }
        public string? UserEmail { get; set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(AccessToken);

        private AuthService()
        {
        }

        public async Task<bool> SignInAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Debug.WriteLine("AuthService. SignInAsync: email o password vacios");
                return false;
            }

            try
            {
                var res = await SupabaseService.Instance.SignInAsync(email.Trim(), password);
                if (!res.Success)
                {
                    Debug.WriteLine($"AuthService.SignInAsync: supabase error:  {res.Error}");
                    return false;
                }

                AccessToken = res.AccessToken;
                UserId = res.UserId?.ToString();
                UserEmail = !string.IsNullOrWhiteSpace(res.Email) ? res.Email : email.Trim();

                try
                {
                    if (!string.IsNullOrEmpty(AccessToken))
                        await SecureStorage.Default.SetAsync(TokenKey, AccessToken);

                    if (!string.IsNullOrEmpty(UserId))
                        await SecureStorage.Default.SetAsync(UserIdKey, UserId);

                    if (!string.IsNullOrWhiteSpace(res.RefreshToken))
                        await SecureStorage.Default.SetAsync(RefreshKey, res.RefreshToken);

                    SupabaseService.Instance.SetUserToken(AccessToken);
                }
                catch (Exception secEx)
                {
                    Debug.WriteLine($"AuthService.SignInAsync: SecureStorage error: {secEx.Message}");
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AuthService.SignInAsync: exception: {ex.Message}");
                return false;
            }
        }

        public async Task LoadFromStorageAsync()
        {
            try
            {
                AccessToken = await SecureStorage.Default.GetAsync(TokenKey);
                UserId = await SecureStorage.Default.GetAsync(UserIdKey);

                if (!string.IsNullOrWhiteSpace(AccessToken))
                    SupabaseService.Instance.SetUserToken(AccessToken);
            }
            catch
            {
            }
        }

        public async Task<bool> SignUpAsync(string email, string password, string name, string? phone = null)
        {
            await Task.Delay(300);
            return true;
        }

        public void Logout()
        {
            AccessToken = null;
            UserId = null;
            UserEmail = null;

            try
            {
                SecureStorage.Default.Remove(TokenKey);
                SecureStorage.Default.Remove(UserIdKey);
                SecureStorage.Default.Remove(RefreshKey);
            }
            catch
            {
            }

            SupabaseService.Instance.SetUserToken(null);
        }

        public async Task SignOutAsync()
        {
            Logout();
            await Task.CompletedTask;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            if (!string.IsNullOrWhiteSpace(AccessToken))
                return AccessToken;

            try
            {
                var tokenFromStorage = await SecureStorage.Default.GetAsync(TokenKey);
                return tokenFromStorage;
            }
            catch
            {
                return null;
            }
        }
    }
}
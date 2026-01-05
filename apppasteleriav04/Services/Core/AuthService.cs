using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Text;         // Para Encoding.UTF8
using System.Net.Http;     // Para HttpRequestMessage, StringContent
using System.Net.Http.Headers; // Para MediaTypeWithQualityHeaderValue
using System.Text.Json;    // Para JsonElement y JsonSerializer

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

        public async Task<bool> RefreshAccessTokenAsync()
        {
            var refreshToken = await SecureStorage.Default.GetAsync(RefreshKey); // O la propiedad
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            try
            {
                var payload = new
                {
                    refresh_token = refreshToken
                };

                // Cambia el endpoint de grant_type aquí:
                using var req = new HttpRequestMessage(HttpMethod.Post, $"{SupabaseConfig.SUPABASE_URL.TrimEnd('/')}/auth/v1/token?grant_type=refresh_token")
                {
                    Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };

                req.Headers.Add("apikey", SupabaseConfig.SUPABASE_ANON_KEY);
                req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var resp = await SupabaseService.Instance._http.SendAsync(req);
                var json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"RefreshAccessTokenAsync failed: {resp.StatusCode}: {json}");
                    return false;
                }

                var result = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(json);
                var newAccessToken = result.GetProperty("access_token").GetString();
                var newRefreshToken = result.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;

                if (!string.IsNullOrWhiteSpace(newAccessToken))
                {
                    AccessToken = newAccessToken;
                    await SecureStorage.Default.SetAsync(TokenKey, newAccessToken);

                    if (!string.IsNullOrWhiteSpace(newRefreshToken))
                        await SecureStorage.Default.SetAsync(RefreshKey, newRefreshToken);

                    SupabaseService.Instance.SetUserToken(newAccessToken);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RefreshAccessTokenAsync error: {ex.Message}");
                return false;
            }
        }

    }
}
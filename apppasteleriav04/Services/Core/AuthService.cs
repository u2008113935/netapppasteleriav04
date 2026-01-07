using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Diagnostics;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

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
        const string UserEmailKey = "auth_user_email";

        public string? AccessToken { get; private set; }
        public string? UserId { get; private set; }
        public string? UserEmail { get; private set; }

        // IMPORTANTE: Verificar que AccessToken NO este vacio
        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(UserId);

        private AuthService()
        {
        }

        public async Task<bool> SignInAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            try
            {
                var res = await SupabaseService.Instance.SignInAsync(email.Trim(), password);

                if (!res.Success)
                {
                    Debug.WriteLine($"[AuthService] Login failed: {res.Error}");
                    return false;
                }

                // Guardar en memoria
                AccessToken = res.AccessToken;
                UserId = res.UserId?.ToString();
                UserEmail = !string.IsNullOrWhiteSpace(res.Email) ? res.Email : email.Trim();

                // Guardar en SecureStorage
                try
                {
                    if (!string.IsNullOrEmpty(AccessToken))
                        await SecureStorage.Default.SetAsync(TokenKey, AccessToken);

                    if (!string.IsNullOrEmpty(UserId))
                        await SecureStorage.Default.SetAsync(UserIdKey, UserId);

                    if (!string.IsNullOrEmpty(UserEmail))
                        await SecureStorage.Default.SetAsync(UserEmailKey, UserEmail);

                    if (!string.IsNullOrWhiteSpace(res.RefreshToken))
                        await SecureStorage.Default.SetAsync(RefreshKey, res.RefreshToken);
                }
                catch (Exception secEx)
                {
                    Debug.WriteLine($"[AuthService] SecureStorage error: {secEx.Message}");
                }

                // Configurar token en SupabaseService
                SupabaseService.Instance.SetUserToken(AccessToken);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] SignInAsync exception: {ex.Message}");
                return false;
            }
        }

        public async Task LoadFromStorageAsync()
        {
            try
            {
                AccessToken = await SecureStorage.Default.GetAsync(TokenKey);
                UserId = await SecureStorage.Default.GetAsync(UserIdKey);
                UserEmail = await SecureStorage.Default.GetAsync(UserEmailKey);

                if (!string.IsNullOrWhiteSpace(AccessToken))
                {
                    SupabaseService.Instance.SetUserToken(AccessToken);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] LoadFromStorageAsync error: {ex.Message}");
                AccessToken = null;
                UserId = null;
                UserEmail = null;
            }
        }

        public async Task<bool> SignUpAsync(string email, string password, string name, string? phone = null)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            try
            {
                var supabaseUrl = Constants.SupabaseConstants.Url;
                var supabaseKey = Constants.SupabaseConstants.AnonKey;
                
                if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
                {
                    Debug.WriteLine("[AuthService] SignUpAsync: Supabase not configured");
                    return false;
                }

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("apikey", supabaseKey);

                var signUpUrl = $"{supabaseUrl}/auth/v1/signup";
                var payload = new
                {
                    email,
                    password,
                    data = new
                    {
                        full_name = name,
                        phone = phone ?? ""
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(signUpUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[AuthService] SignUpAsync failed: {response.StatusCode} - {responseBody}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] SignUpAsync exception: {ex.Message}");
                return false;
            }
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
                SecureStorage.Default.Remove(UserEmailKey);
                SecureStorage.Default.Remove(RefreshKey);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] Logout error: {ex.Message}");
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
            // Si ya tenemos token en memoria, devolverlo
            if (!string.IsNullOrWhiteSpace(AccessToken))
                return AccessToken;

            try
            {
                // Intentar cargar desde storage
                var tokenFromStorage = await SecureStorage.Default.GetAsync(TokenKey);

                if (!string.IsNullOrWhiteSpace(tokenFromStorage))
                {
                    // IMPORTANTE: Actualizar la propiedad en memoria
                    AccessToken = tokenFromStorage;

                    // Tambien cargar UserId y UserEmail si no estan en memoria
                    if (string.IsNullOrWhiteSpace(UserId))
                        UserId = await SecureStorage.Default.GetAsync(UserIdKey);

                    if (string.IsNullOrWhiteSpace(UserEmail))
                        UserEmail = await SecureStorage.Default.GetAsync(UserEmailKey);

                    SupabaseService.Instance.SetUserToken(AccessToken);
                }

                return tokenFromStorage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] GetAccessTokenAsync error: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> RefreshAccessTokenAsync()
        {
            try
            {
                var refreshToken = await SecureStorage.Default.GetAsync(RefreshKey);

                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    return false;
                }

                var payload = new { refresh_token = refreshToken };

                using var req = new HttpRequestMessage(HttpMethod.Post,
                    $"{SupabaseConfig.SUPABASE_URL.TrimEnd('/')}/auth/v1/token? grant_type=refresh_token")
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };

                req.Headers.Add("apikey", SupabaseConfig.SUPABASE_ANON_KEY);
                req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var resp = await SupabaseService.Instance._http.SendAsync(req);
                
                if (!resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[AuthService] RefreshAccessTokenAsync failed: {resp.StatusCode}");
                    return false;
                }

                var json = await resp.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(json);
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
                Debug.WriteLine($"[AuthService] RefreshAccessTokenAsync error:  {ex.Message}");
                return false;
            }
        }
    }
}
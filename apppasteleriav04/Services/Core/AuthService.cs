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
        const string UserRoleKey = "auth_user_role";
        const string UserFullNameKey = "auth_user_fullname";


        public string? AccessToken { get; private set; }
        public string? UserId { get; private set; }
        public string? UserEmail { get; private set; }
        public string? UserRole { get; private set; }
        public string? UserFullName { get; private set; }

        // IMPORTANTE: Verificar que AccessToken NO este vacio
        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(AccessToken) && !string.IsNullOrWhiteSpace(UserId);

        private AuthService()
        {
            Debug.WriteLine("[AuthService] Instancia creada");
        }

        public async Task<bool> SignInAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                Debug.WriteLine("[AuthService] SignInAsync: email o password vacios");
                return false;
            }

            try
            {
                Debug.WriteLine($"[AuthService] SignInAsync: intentando login con {email}");

                var res = await SupabaseService.Instance.SignInAsync(email.Trim(), password);

                if (!res.Success)
                {
                    Debug.WriteLine($"[AuthService] SignInAsync:  supabase error:  {res.Error}");
                    return false;
                }

                // Guardar en memoria
                AccessToken = res.AccessToken;
                UserId = res.UserId?.ToString();
                UserEmail = !string.IsNullOrWhiteSpace(res.Email) ? res.Email : email.Trim();

                //Obetener perfil con rol y nombre completo
                if (Guid.TryParse(UserId, out var userId))
                {
                    // Obtener perfil desde SupabaseService
                    var profile = await SupabaseService.Instance.GetProfileAsync(userId);

                    // Si existe perfil, guardar rol y nombre completo
                    if (profile != null) 
                    {
                        UserRole = profile.Role?? "cliente";
                        UserFullName = profile.FullName;

                        Debug.WriteLine($"[AuthService] - Perfil: {UserRole}");
                        Debug.WriteLine($"[AuthService] - Nombre: {UserFullName}");
                        Debug.Write($"[AuthService] - Rol: {UserRole}");
                    }
                    else
                    {
                        UserRole = "cliente";
                        Debug.WriteLine("[AuthService] - Perfil no encontrado, rol por defecto: cliente");
                    }
                }


                Debug.WriteLine($"[AuthService] SignInAsync exitoso:");
                Debug.WriteLine($"[AuthService] - AccessToken: {(string.IsNullOrEmpty(AccessToken) ? "NULL" : AccessToken.Substring(0, Math.Min(20, AccessToken.Length)) + "...")}");
                Debug.WriteLine($"[AuthService] - UserId: {UserId}");
                Debug.WriteLine($"[AuthService] - UserEmail: {UserEmail}");
                Debug.WriteLine($"[AuthService] - UserRole: {UserRole}");
                Debug.WriteLine($"[AuthService] - IsAuthenticated: {IsAuthenticated}");

                // Guardar en SecureStorage
                try
                {
                    if (!string.IsNullOrEmpty(AccessToken))
                        await SecureStorage.Default.SetAsync(TokenKey, AccessToken);

                    if (!string.IsNullOrEmpty(UserId))
                        await SecureStorage.Default.SetAsync(UserIdKey, UserId);

                    if (!string.IsNullOrEmpty(UserEmail))
                        await SecureStorage.Default.SetAsync(UserEmailKey, UserEmail);

                    if (!string.IsNullOrWhiteSpace(UserRole))
                        await SecureStorage.Default.SetAsync(UserRoleKey, UserRole);

                    if (!string.IsNullOrWhiteSpace(UserFullName))
                        await SecureStorage.Default.SetAsync(UserFullNameKey, UserFullName);

                    if (!string.IsNullOrWhiteSpace(res.RefreshToken))
                        await SecureStorage.Default.SetAsync(RefreshKey, res.RefreshToken);

                    Debug.WriteLine("[AuthService] Datos guardados en SecureStorage");
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

        // Cargar datos desde SecureStorage
        public async Task LoadFromStorageAsync()
        {
            try
            {
                Debug.WriteLine("[AuthService] LoadFromStorageAsync:  cargando datos.. .");

                AccessToken = await SecureStorage.Default.GetAsync(TokenKey);
                UserId = await SecureStorage.Default.GetAsync(UserIdKey);
                UserEmail = await SecureStorage.Default.GetAsync(UserEmailKey);
                UserRole = await SecureStorage.Default.GetAsync(UserRoleKey);
                UserFullName = await SecureStorage.Default.GetAsync(UserFullNameKey);

                Debug.WriteLine($"[AuthService] LoadFromStorageAsync resultados:");
                Debug.WriteLine($"[AuthService] - AccessToken: {(string.IsNullOrEmpty(AccessToken) ? "NULL" : "presente")}");
                Debug.WriteLine($"[AuthService] - UserId: {UserId ?? "NULL"}");
                Debug.WriteLine($"[AuthService] - UserEmail: {UserEmail ?? "NULL"}");
                Debug.WriteLine($"[AuthService] - UserRole: {UserRole ?? "NULL"}");
                Debug.WriteLine($"[AuthService] - UserFullName: {UserFullName ?? "NULL"}");
                Debug.WriteLine($"[AuthService] - IsAuthenticated: {IsAuthenticated}");

                if (!string.IsNullOrWhiteSpace(AccessToken))
                {
                    SupabaseService.Instance.SetUserToken(AccessToken);
                    Debug.WriteLine("[AuthService] Token configurado en SupabaseService");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] LoadFromStorageAsync error: {ex.Message}");
                AccessToken = null;
                UserId = null;
                UserEmail = null;
                UserRole = null;
                UserFullName = null;
            }
        }

        public async Task<bool> SignUpAsync(string email, string password, string name, string? phone = null)
        {
            // TODO: Implementar registro real
            await Task.Delay(300);
            return true;
        }

        public void Logout()
        {
            Debug.WriteLine("[AuthService] Logout: limpiando datos...");

            AccessToken = null;
            UserId = null;
            UserEmail = null;
            UserRole = null;
            UserFullName = null;

            try
            {
                SecureStorage.Default.Remove(TokenKey);
                SecureStorage.Default.Remove(UserIdKey);
                SecureStorage.Default.Remove(UserEmailKey);
                SecureStorage.Default.Remove(UserRoleKey);
                SecureStorage.Default.Remove(UserFullNameKey);
                SecureStorage.Default.Remove(RefreshKey);
                Debug.WriteLine("[AuthService] SecureStorage limpiado");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AuthService] Logout SecureStorage error: {ex.Message}");
            }

            SupabaseService.Instance.SetUserToken(null);
            Debug.WriteLine($"[AuthService] IsAuthenticated despues de logout: {IsAuthenticated}");
        }

        public async Task SignOutAsync()
        {
            Logout();
            await Task.CompletedTask;
        }

        // Obtener token de acceso, primero desde memoria, luego desde SecureStorage
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

                    if (string.IsNullOrWhiteSpace(UserRole))
                        UserRole = await SecureStorage.Default.GetAsync(UserRoleKey);

                    if (string.IsNullOrWhiteSpace(UserFullName))
                        UserFullName = await SecureStorage.Default.GetAsync(UserFullNameKey);

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
                    Debug.WriteLine("[AuthService] RefreshAccessTokenAsync: no hay refresh token");
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
                var json = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[AuthService] RefreshAccessTokenAsync failed: {resp.StatusCode}:  {json}");
                    return false;
                }

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

                    Debug.WriteLine("[AuthService] Token refrescado exitosamente");
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

        // Metodo auxiliar para verificar estado actual        
        public void LogCurrentState()
        {
            Debug.WriteLine("========== AuthService Estado Actual ==========");
            Debug.WriteLine($"AccessToken: {(string.IsNullOrEmpty(AccessToken) ? "NULL" : "presente")}");
            Debug.WriteLine($"UserId: {UserId ?? "NULL"}");
            Debug.WriteLine($"UserEmail: {UserEmail ?? "NULL"}");
            Debug.WriteLine($"UserRole: {UserRole ?? "NULL"}");
            Debug.WriteLine($"UserFullName: {UserFullName ?? "NULL"}");
            Debug.WriteLine($"IsAuthenticated:  {IsAuthenticated}");
            Debug.WriteLine("===============================================");
        }

        //Metodos auxiliares para verificar rol
        public bool IsEmployee() => UserRole == "cocina" || UserRole == "reparto" || UserRole == "backoffice";
        public bool IsCliente() => UserRole == "cliente";
        public bool IsCocina() => UserRole == "cocina";
        public bool IsReparto() => UserRole == "reparto";
        public bool IsBackoffice() => UserRole == "backoffice";

    }
}
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using apppasteleriav04.Services.Core;
using apppasteleriav04.Models.Domain;

namespace apppasteleriav04.Views.Profile
{
    public partial class ProfilePage : ContentPage
    {
        private UserProfile _profile;   // Usa SIEMPRE apppasteleriav04.Models.Profile como modelo
        private readonly SupabaseService _supabase = new SupabaseService();

        public ProfilePage()
        {
            InitializeComponent();
            _ = LoadProfileAsync();
        }

        async Task LoadProfileAsync()
        {
            try
            {
                InfoLabel.Text = "Cargando perfil...";
                var userIdStr = await SecureStorage.Default.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                {
                    InfoLabel.Text = "No se encontró sesión. Inicia sesión primero.";
                    return;
                }

                UserIdEntry.Text = userId.ToString();

                _profile = await _supabase.GetProfileAsync(userId);
                if (_profile == null)
                {
                    InfoLabel.Text = "Perfil no encontrado. Puedes crear tus datos.";
                    FullNameEntry.Text = string.Empty;
                    AvatarPathEntry.Text = string.Empty;
                    SetAvatarImage(null);
                    return;
                }

                FullNameEntry.Text = _profile.FullName ?? string.Empty;
                AvatarPathEntry.Text = _profile.AvatarUrl ?? string.Empty;
                SetAvatarImage(_profile.AvatarPublicUrl);

                InfoLabel.Text = "Perfil cargado";
            }
            catch (Exception ex)
            {
                InfoLabel.Text = $"Error cargando perfil: {ex.Message}";
            }
        }

        void SetAvatarImage(string avatarPublicUrl)
        {
            if (!string.IsNullOrWhiteSpace(avatarPublicUrl))
                AvatarImage.Source = ImageSource.FromUri(new Uri(avatarPublicUrl));
            else
                AvatarImage.Source = "avatar_placeholder.png";
        }

        void OnEditClicked(object sender, EventArgs e)
        {
            FullNameEntry.IsEnabled = true;
            AvatarPathEntry.IsEnabled = true;
            SaveButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
            EditButton.IsEnabled = false;
            InfoLabel.Text = "Modo edición activo";
        }

        void OnCancelClicked(object sender, EventArgs e)
        {
            if (_profile != null)
            {
                FullNameEntry.Text = _profile.FullName;
                AvatarPathEntry.Text = _profile.AvatarUrl;
                SetAvatarImage(_profile.AvatarPublicUrl);
            }
            else
            {
                FullNameEntry.Text = string.Empty;
                AvatarPathEntry.Text = string.Empty;
                SetAvatarImage(null);
            }

            FullNameEntry.IsEnabled = false;
            AvatarPathEntry.IsEnabled = false;
            SaveButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            EditButton.IsEnabled = true;
            InfoLabel.Text = "Edición cancelada";
        }

        async void OnSaveClicked(object sender, EventArgs e)
        {
            var newName = FullNameEntry.Text?.Trim();
            var newAvatarPath = AvatarPathEntry.Text?.Trim();

            if (string.IsNullOrEmpty(newName))
            {
                await DisplayAlert("Validación", "El nombre no puede estar vacío.", "OK");
                return;
            }

            if (!Guid.TryParse(UserIdEntry.Text, out var userId))
            {
                await DisplayAlert("Error", "UserId inválido.", "OK");
                return;
            }

            var payload = new { full_name = newName, avatar_url = string.IsNullOrEmpty(newAvatarPath) ? null : newAvatarPath };

            InfoLabel.Text = "Guardando cambios...";
            SaveButton.IsEnabled = false;
            CancelButton.IsEnabled = false;

            try
            {
                var baseUrl = SupabaseConfig.SUPABASE_URL.TrimEnd('/');
                var url = $"{baseUrl}/rest/v1/profiles?id=eq.{Uri.EscapeDataString(userId.ToString())}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("apikey", SupabaseConfig.SUPABASE_ANON_KEY);
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
                    await DisplayAlert("Error", $"No se pudo actualizar el perfil: {resp.StatusCode}\n{respText}", "OK");
                    InfoLabel.Text = "Error al guardar";
                }
                else
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var list = JsonSerializer.Deserialize<UserProfile[]>(respText, options);
                    if (list != null && list.Length > 0)
                    {
                        _profile = list[0];
                        FullNameEntry.Text = _profile.FullName;
                        AvatarPathEntry.Text = _profile.AvatarUrl;
                        SetAvatarImage(_profile.AvatarPublicUrl);
                        InfoLabel.Text = "Perfil actualizado correctamente";
                    }
                    else
                    {
                        InfoLabel.Text = "Perfil actualizado (sin representación devuelta).";
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Exception: {ex.Message}", "OK");
                InfoLabel.Text = $"Error: {ex.Message}";
            }
            finally
            {
                FullNameEntry.IsEnabled = false;
                AvatarPathEntry.IsEnabled = false;
                SaveButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
                EditButton.IsEnabled = true;
            }
        }
    }
}
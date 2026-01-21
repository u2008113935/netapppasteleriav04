using System;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    public class UserProfile
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        
        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        
        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("email")]        
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Propiedad calculada para obtener la URL pública completa del avatar
        public string AvatarPublicUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AvatarUrl)) return null;

                var baseUrl = apppasteleriav04.Services.Core.SupabaseConfig.SUPABASE_URL?.TrimEnd('/');
                if (string.IsNullOrWhiteSpace(baseUrl)) return AvatarUrl;

                return $"{baseUrl}/storage/v1/object/public/{apppasteleriav04.Services.Core.SupabaseConfig.BUCKET_NAME}/{Uri.EscapeDataString(AvatarUrl)}";
            }
        }
    }
}




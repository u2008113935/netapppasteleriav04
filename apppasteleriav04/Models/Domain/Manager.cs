using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    public class Manager
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = "gerente"; // gerente, operaciones, ti

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = new List<string>();

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("last_login_at")]
        public DateTime? LastLoginAt { get; set; }
    }
}

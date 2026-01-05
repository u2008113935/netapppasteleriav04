using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    /// <summary>
    /// Representa un usuario administrador (gerente, operaciones, TI)
    /// </summary>
    public class Manager
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }

        [JsonPropertyName("full_name")]
        public string? FullName { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; } // gerente, operaciones, ti

        [JsonPropertyName("permissions")]
        public List<string> Permissions { get; set; } = new List<string>();

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        // Display property
        public string DisplayName => string.IsNullOrWhiteSpace(FullName) ? Email ?? "Manager" : FullName;
    }
}

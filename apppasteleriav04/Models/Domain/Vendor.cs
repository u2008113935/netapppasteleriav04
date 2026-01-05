using System;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    public class Vendor
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("contact_name")]
        public string ContactName { get; set; } = string.Empty;

        [JsonPropertyName("contact_email")]
        public string ContactEmail { get; set; } = string.Empty;

        [JsonPropertyName("contact_phone")]
        public string? ContactPhone { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}

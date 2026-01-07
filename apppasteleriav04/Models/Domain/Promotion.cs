using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    public class Promotion
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("discount_type")]
        public string DiscountType { get; set; } = "percentage"; // percentage, fixed

        [JsonPropertyName("discount_value")]
        public decimal DiscountValue { get; set; }

        [JsonPropertyName("min_order_amount")]
        public decimal MinOrderAmount { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("applicable_product_ids")]
        public List<Guid> ApplicableProductIds { get; set; } = new List<Guid>();

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}

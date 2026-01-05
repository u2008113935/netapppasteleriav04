using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    /// <summary>
    /// Representa una promoción o descuento aplicable a productos
    /// </summary>
    public class Promotion
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("discount_type")]
        public string? DiscountType { get; set; } // percentage, fixed

        [JsonPropertyName("discount_value")]
        public decimal DiscountValue { get; set; }

        [JsonPropertyName("min_order_amount")]
        public decimal? MinOrderAmount { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; }

        [JsonPropertyName("applicable_products")]
        public List<Guid> ApplicableProducts { get; set; } = new List<Guid>();

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        // Display properties
        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "Promoción" : Name;
        
        public bool IsCurrentlyActive => IsActive && 
                                          DateTime.Now >= StartDate && 
                                          DateTime.Now <= EndDate;
    }
}

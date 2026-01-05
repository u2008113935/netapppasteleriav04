using System;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    public class Payment
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("order_id")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; } = "efectivo"; // efectivo, tarjeta, yape, plin

        [JsonPropertyName("status")]
        public string Status { get; set; } = "pendiente"; // pendiente, procesando, completado, fallido

        [JsonPropertyName("external_reference")]
        public string? ExternalReference { get; set; }

        [JsonPropertyName("gateway")]
        public string? Gateway { get; set; }

        [JsonPropertyName("last_four_digits")]
        public string? LastFourDigits { get; set; }

        [JsonPropertyName("card_brand")]
        public string? CardBrand { get; set; }

        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("processed_at")]
        public DateTime? ProcessedAt { get; set; }
    }
}

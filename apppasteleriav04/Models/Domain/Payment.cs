using System;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    public class Payment
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("pedido_id")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("monto")]
        public decimal Amount { get; set; }

        [JsonPropertyName("metodo_pago")]
        public string PaymentMethod { get; set; } = string.Empty; // efectivo, tarjeta, yape, plin

        [JsonPropertyName("estado")]
        public string Status { get; set; } = "pendiente"; // pendiente, procesando, completado, fallido, reembolsado

        [JsonPropertyName("referencia_externa")]
        public string? ExternalReference { get; set; } // ID de Stripe/MercadoPago/Culqi

        [JsonPropertyName("gateway")]
        public string? Gateway { get; set; } // stripe, mercadopago, culqi, manual

        [JsonPropertyName("last_four")]
        public string? LastFourDigits { get; set; } // Últimos 4 dígitos de tarjeta

        [JsonPropertyName("card_brand")]
        public string? CardBrand { get; set; } // visa, mastercard, amex

        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("processed_at")]
        public DateTime? ProcessedAt { get; set; }

        [JsonPropertyName("metadata")]
        public string? MetadataJson { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Refunded,
        Cancelled
    }

    public enum PaymentMethod
    {
        Cash,
        CreditCard,
        DebitCard,
        Yape,
        Plin,
        BankTransfer
    }
}

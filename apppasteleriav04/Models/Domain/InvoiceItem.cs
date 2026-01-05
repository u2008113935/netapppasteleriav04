using System;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    public class InvoiceItem
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("invoice_id")]
        public Guid InvoiceId { get; set; }

        [JsonPropertyName("product_id")]
        public Guid ProductId { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("igv")]
        public decimal Igv { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("product_code")]
        public string? ProductCode { get; set; }
    }
}

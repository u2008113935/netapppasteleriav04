using System;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    public class Invoice
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("order_id")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("payment_id")]
        public Guid? PaymentId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "boleta"; // boleta, factura

        [JsonPropertyName("serial_number")]
        public string SerialNumber { get; set; } = string.Empty;

        [JsonPropertyName("correlative_number")]
        public string CorrelativeNumber { get; set; } = string.Empty;

        [JsonPropertyName("customer_ruc")]
        public string? CustomerRuc { get; set; }

        [JsonPropertyName("customer_name")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("customer_address")]
        public string? CustomerAddress { get; set; }

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("igv")]
        public decimal Igv { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("pdf_url")]
        public string? PdfUrl { get; set; }

        [JsonPropertyName("xml_url")]
        public string? XmlUrl { get; set; }

        [JsonPropertyName("sunat_status")]
        public string? SunatStatus { get; set; }

        [JsonPropertyName("sunat_ticket")]
        public string? SunatTicket { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("sent_at")]
        public DateTime? SentAt { get; set; }
    }
}

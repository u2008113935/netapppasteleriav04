using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace apppasteleriav04.Models.Domain
{
    public class Invoice
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("pedido_id")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("payment_id")]
        public Guid? PaymentId { get; set; }

        [JsonPropertyName("tipo")]
        public string Type { get; set; } = string.Empty; // boleta, factura

        [JsonPropertyName("serie")]
        public string SerialNumber { get; set; } = string.Empty; // B001, F001

        [JsonPropertyName("correlativo")]
        public int CorrelativeNumber { get; set; } // 00001, 00002

        [JsonPropertyName("cliente_ruc")]
        public string? CustomerRuc { get; set; }

        [JsonPropertyName("cliente_nombre")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("cliente_direccion")]
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
        public string SunatStatus { get; set; } = "pendiente"; // pendiente, enviado, aceptado, rechazado

        [JsonPropertyName("sunat_ticket")]
        public string? SunatTicket { get; set; }

        [JsonPropertyName("sunat_cdr")]
        public string? SunatCdr { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("sent_at")]
        public DateTime? SentAt { get; set; }

        [JsonIgnore]
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }

    public enum InvoiceType
    {
        Boleta,
        Factura
    }

    public enum SunatStatus
    {
        Pending,
        Sent,
        Accepted,
        Rejected
    }
}

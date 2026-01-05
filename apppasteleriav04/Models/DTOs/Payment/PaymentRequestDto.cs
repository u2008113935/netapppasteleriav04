using System;
using System.Collections.Generic;

namespace apppasteleriav04.Models.DTOs.Payment
{
    public class PaymentRequestDto
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "PEN";
        public string PaymentMethod { get; set; } = string.Empty; // efectivo, tarjeta, yape, plin
        public string? CardToken { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}

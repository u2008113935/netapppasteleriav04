using System;

namespace apppasteleriav04.Models.DTOs.Payment
{
    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public Guid? PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ExternalReference { get; set; }
        public string? AuthorizationCode { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace apppasteleriav04.Models.DTOs.Payment
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public Guid? PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
        public string? ExternalReference { get; set; }
        public string? AuthorizationCode { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }
}

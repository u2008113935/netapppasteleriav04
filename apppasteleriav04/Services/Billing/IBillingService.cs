using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apppasteleriav04.Models.Domain;

namespace apppasteleriav04.Services.Billing
{
    public interface IBillingService
    {
        Task<Invoice> GenerateInvoiceAsync(Guid orderId, InvoiceType type, CustomerData? customerData = null);
        Task<Invoice?> GetInvoiceAsync(Guid invoiceId);
        Task<List<Invoice>> GetInvoicesByUserAsync(Guid userId);
        Task<List<Invoice>> GetInvoicesByOrderAsync(Guid orderId);
        Task<byte[]> GeneratePdfAsync(Guid invoiceId);
        Task<bool> SendToSunatAsync(Guid invoiceId);
        Task<bool> SendEmailAsync(Guid invoiceId, string email);
        Task<string> GetNextSerialNumberAsync(InvoiceType type);
    }

    public class CustomerData
    {
        public string? Ruc { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? DocumentType { get; set; } // DNI, RUC
        public string? DocumentNumber { get; set; }
    }
}

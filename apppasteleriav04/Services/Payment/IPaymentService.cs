using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Models.DTOs.Payment;

namespace apppasteleriav04.Services.Payment
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequestDto request);
        Task<PaymentResult> ProcessCashPaymentAsync(Guid orderId, decimal amount);
        Task<PaymentResult> RefundPaymentAsync(Guid paymentId, decimal? amount = null);
        Task<Payment?> GetPaymentAsync(Guid paymentId);
        Task<List<Payment>> GetPaymentsByOrderAsync(Guid orderId);
        Task<bool> ValidateCardAsync(string cardNumber);
    }
}

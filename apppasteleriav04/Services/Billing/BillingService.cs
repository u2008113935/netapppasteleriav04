using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Models.Enums;

namespace apppasteleriav04.Services.Billing
{
    public class BillingService : IBillingService
    {
        private const decimal IGV_RATE = 0.18m; // 18% IGV in Peru
        private readonly PdfGenerator _pdfGenerator;

        // In-memory storage for demo (replace with database)
        private static int _boletaCorrelative = 0;
        private static int _facturaCorrelative = 0;

        public BillingService()
        {
            _pdfGenerator = new PdfGenerator();
        }

        public async Task<Invoice> GenerateInvoiceAsync(Guid orderId, InvoiceType type, CustomerData? customerData = null)
        {
            try
            {
                // TODO: Get order from database
                // var order = await _supabaseService.GetAsync<Order>("pedidos", orderId);
                // var orderItems = await _supabaseService.QueryAsync<OrderItem>("order_items", 
                //     item => item.OrderId == orderId);

                // For now, simulate order data
                var orderTotal = 100m; // Replace with actual order total

                // Calculate subtotal and IGV
                var subtotal = orderTotal / (1 + IGV_RATE);
                var igv = orderTotal - subtotal;

                // Generate serial number
                var serialNumber = await GetNextSerialNumberAsync(type);

                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    Type = type == InvoiceType.Boleta ? "boleta" : "factura",
                    SerialNumber = type == InvoiceType.Boleta ? "B001" : "F001",
                    CorrelativeNumber = serialNumber,
                    CustomerName = customerData?.Name ?? "Cliente",
                    CustomerRuc = customerData?.Ruc,
                    CustomerAddress = customerData?.Address,
                    Subtotal = Math.Round(subtotal, 2),
                    Igv = Math.Round(igv, 2),
                    Total = Math.Round(orderTotal, 2),
                    SunatStatus = "pendiente",
                    CreatedAt = DateTime.UtcNow
                };

                // TODO: Save to database
                // await _supabaseService.InsertAsync("invoices", invoice);

                return invoice;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating invoice: {ex.Message}", ex);
            }
        }

        public async Task<Invoice?> GetInvoiceAsync(Guid invoiceId)
        {
            await Task.CompletedTask;
            // TODO: Get from database
            // return await _supabaseService.GetAsync<Invoice>("invoices", invoiceId);
            return null;
        }

        public async Task<List<Invoice>> GetInvoicesByUserAsync(Guid userId)
        {
            await Task.CompletedTask;
            // TODO: Query database
            // Get orders for user, then get invoices for those orders
            return new List<Invoice>();
        }

        public async Task<List<Invoice>> GetInvoicesByOrderAsync(Guid orderId)
        {
            await Task.CompletedTask;
            // TODO: Query database
            // return await _supabaseService.QueryAsync<Invoice>("invoices",
            //     inv => inv.OrderId == orderId);
            return new List<Invoice>();
        }

        public async Task<byte[]> GeneratePdfAsync(Guid invoiceId)
        {
            // TODO: Get invoice from database
            // var invoice = await GetInvoiceAsync(invoiceId);

            // For now, generate a simple PDF
            var invoice = new Invoice
            {
                Id = invoiceId,
                Type = "boleta",
                SerialNumber = "B001",
                CorrelativeNumber = "1",
                CustomerName = "Cliente Demo",
                Subtotal = 84.75m,
                Igv = 15.25m,
                Total = 100.00m,
                CreatedAt = DateTime.UtcNow
            };

            var pdfBytes = await _pdfGenerator.GenerateInvoicePdfAsync(invoice);

            // TODO: Upload to Supabase Storage and save URL
            // invoice.PdfUrl = uploadedUrl;
            // await _supabaseService.UpdateAsync("invoices", invoice);

            return pdfBytes;
        }

        public async Task<bool> SendToSunatAsync(Guid invoiceId)
        {
            await Task.Delay(2000); // Simulate SUNAT API call

            // TODO: Integrate with SUNAT (Nubefact, FacturaDirecta, etc.)
            // For now, simulate success

            // Update invoice status
            // var invoice = await GetInvoiceAsync(invoiceId);
            // invoice.SunatStatus = "aceptado";
            // invoice.SentAt = DateTime.UtcNow;
            // await _supabaseService.UpdateAsync("invoices", invoice);

            return true;
        }

        public async Task<bool> SendEmailAsync(Guid invoiceId, string email)
        {
            await Task.Delay(1000); // Simulate email sending

            // TODO: Generate PDF and send via email service
            // var pdfBytes = await GeneratePdfAsync(invoiceId);
            // await _emailService.SendAsync(email, "Your Invoice", pdfBytes);

            return true;
        }

        public async Task<string> GetNextSerialNumberAsync(InvoiceType type)
        {
            await Task.CompletedTask;

            // TODO: Get last correlative from database
            // For now, use in-memory counter
            if (type == InvoiceType.Boleta)
            {
                _boletaCorrelative++;
                return $"B001-{_boletaCorrelative:D5}";
            }
            else
            {
                _facturaCorrelative++;
                return $"F001-{_facturaCorrelative:D5}";
            }
        }

        public static decimal CalculateSubtotal(decimal total)
        {
            return total / (1 + IGV_RATE);
        }

        public static decimal CalculateIgv(decimal subtotal)
        {
            return subtotal * IGV_RATE;
        }
    }
}

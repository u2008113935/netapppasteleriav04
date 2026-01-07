using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using apppasteleriav04.Models.Domain;

namespace apppasteleriav04.Services.Billing
{
    public class PdfGenerator
    {
        public async Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice)
        {
            await Task.CompletedTask;

            // For a real implementation, use a PDF library like:
            // - QuestPDF
            // - iTextSharp
            // - PdfSharpCore
            
            // For now, generate a simple text-based representation
            var sb = new StringBuilder();
            sb.AppendLine("===========================================");
            sb.AppendLine($"        {(invoice.Type == "boleta" ? "BOLETA" : "FACTURA")} ELECTRÓNICA");
            sb.AppendLine("===========================================");
            sb.AppendLine();
            sb.AppendLine("PASTELERÍA DELICIA");
            sb.AppendLine("RUC: 20123456789");
            sb.AppendLine("Av. Principal 123, Lima");
            sb.AppendLine();
            sb.AppendLine("-------------------------------------------");
            sb.AppendLine($"Serie: {invoice.SerialNumber}-{invoice.CorrelativeNumber:D5}");
            sb.AppendLine($"Fecha: {invoice.CreatedAt:dd/MM/yyyy HH:mm}");
            sb.AppendLine("-------------------------------------------");
            sb.AppendLine();
            sb.AppendLine($"Cliente: {invoice.CustomerName}");
            if (!string.IsNullOrEmpty(invoice.CustomerRuc))
                sb.AppendLine($"RUC: {invoice.CustomerRuc}");
            if (!string.IsNullOrEmpty(invoice.CustomerAddress))
                sb.AppendLine($"Dirección: {invoice.CustomerAddress}");
            sb.AppendLine();
            sb.AppendLine("-------------------------------------------");
            sb.AppendLine("DETALLE");
            sb.AppendLine("-------------------------------------------");
            
            // Add invoice items
            sb.AppendLine("Item                     Cant    Precio");
            
            // If invoice has items, display them; otherwise show calculated subtotal
            if (invoice.Items != null && invoice.Items.Any())
            {
                foreach (var item in invoice.Items)
                {
                    var itemName = item.ProductName ?? "Producto";
                    // Truncate or pad to exactly 20 characters
                    if (itemName.Length > 20)
                        itemName = itemName.Substring(0, 20);
                    else
                        itemName = itemName.PadRight(20);
                    
                    sb.AppendLine($"{itemName}    {item.Quantity,4}    {item.UnitPrice,7:F2}");
                }
            }
            else
            {
                // Fallback when no items available
                sb.AppendLine($"{"Producto",20}    {1,4}    {invoice.Subtotal,7:F2}");
            }
            
            sb.AppendLine("-------------------------------------------");
            sb.AppendLine($"Subtotal:                S/ {invoice.Subtotal:F2}");
            sb.AppendLine($"IGV (18%):              S/ {invoice.Igv:F2}");
            sb.AppendLine($"TOTAL:                  S/ {invoice.Total:F2}");
            sb.AppendLine("===========================================");
            sb.AppendLine();
            sb.AppendLine($"Estado SUNAT: {invoice.SunatStatus.ToUpper()}");
            sb.AppendLine();
            sb.AppendLine("Gracias por su compra!");
            sb.AppendLine("===========================================");

            // Convert to bytes (UTF-8)
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> GenerateBoletaPdfAsync(Invoice invoice)
        {
            return await GenerateInvoicePdfAsync(invoice);
        }

        public async Task<byte[]> GenerateFacturaPdfAsync(Invoice invoice)
        {
            return await GenerateInvoicePdfAsync(invoice);
        }
    }
}


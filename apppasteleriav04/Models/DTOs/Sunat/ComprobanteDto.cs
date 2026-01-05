using System;
using System.Collections.Generic;
using System.Text;

namespace apppasteleriav04.Models.DTOs.Sunat
{
    public class ComprobanteDto
    {
        public string TipoComprobante { get; set; } = string.Empty; // 01=Factura, 03=Boleta
        public string Serie { get; set; } = string.Empty;
        public string Correlativo { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public string HoraEmision { get; set; } = string.Empty;
        public string Moneda { get; set; } = "PEN";
        public decimal TipoCambio { get; set; } = 1.0m;
        
        public EmisorDto Emisor { get; set; } = new EmisorDto();
        public ReceptorDto Receptor { get; set; } = new ReceptorDto();
        
        public List<DetalleComprobanteDto> Items { get; set; } = new List<DetalleComprobanteDto>();
        
        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }
        
        public string FormaPago { get; set; } = "Contado"; // Contado, Credito
    }
}

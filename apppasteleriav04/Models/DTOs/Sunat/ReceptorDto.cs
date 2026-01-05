using System;
using System.Collections.Generic;
using System.Text;

namespace apppasteleriav04.Models.DTOs.Sunat
{
    public class ReceptorDto
    {
        public string TipoDocumento { get; set; } = string.Empty; // 6=RUC, 1=DNI, 0=SIN RUC
        public string NumeroDocumento { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string? Direccion { get; set; }
    }
}

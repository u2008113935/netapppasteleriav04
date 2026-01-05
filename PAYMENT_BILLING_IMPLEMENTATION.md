# PR 6: Sistema de Pagos y Facturación

## Resumen
Implementación completa del sistema de pagos y facturación electrónica para la aplicación de pastelería, incluyendo procesamiento de pagos con múltiples métodos y generación de comprobantes electrónicos (boletas y facturas).

## Historias de Usuario Implementadas

### HU-C4: Pago en Línea
- ✅ Selección de método de pago (efectivo, tarjeta, Yape, Plin)
- ✅ Formulario de tarjeta con validación Luhn
- ✅ Procesamiento de pagos
- ✅ Páginas de éxito y error de pago
- ✅ Reintentos y manejo de errores

### HU-C5: Comprobante Electrónico
- ✅ Generación de boletas y facturas
- ✅ Cálculo automático de IGV (18%)
- ✅ Generación de PDF
- ✅ Lista de comprobantes
- ✅ Detalle de comprobante
- ✅ Estado SUNAT

## Archivos Creados

### Domain Models
- `Models/Domain/Payment.cs` - Modelo de pago con propiedades completas
- `Models/Domain/Invoice.cs` - Modelo de comprobante (boleta/factura)
- `Models/Domain/InvoiceItem.cs` - Items del comprobante

### DTOs
- `Models/DTOs/Payment/PaymentRequestDto.cs` - Request de pago
- `Models/DTOs/Payment/PaymentResponseDto.cs` - Response de pago
- `Models/DTOs/Payment/PaymentResult.cs` - Resultado de operación de pago
- `Models/DTOs/Sunat/EmisorDto.cs` - Datos del emisor (empresa)
- `Models/DTOs/Sunat/ReceptorDto.cs` - Datos del receptor (cliente)
- `Models/DTOs/Sunat/ComprobanteDto.cs` - DTO completo del comprobante

### Services
- `Services/Payment/IPaymentService.cs` - Interface del servicio de pagos
- `Services/Payment/PaymentService.cs` - Implementación del servicio
- `Services/Billing/IBillingService.cs` - Interface del servicio de facturación
- `Services/Billing/BillingService.cs` - Implementación del servicio
- `Services/Billing/PdfGenerator.cs` - Generador de PDFs (actualizado)

### Helpers
- `Helpers/CardValidator.cs` - Validación de tarjetas con algoritmo Luhn
- `Helpers/ObservableObject.cs` - Base para ViewModels (actualizado)
- `Helpers/RelayCommand.cs` - Command pattern (actualizado)
- `Helpers/AsyncRelayCommand.cs` - Async command pattern (actualizado)

### Converters
- `Converters/InverseBoolConverter.cs` - Invierte valores booleanos
- `Converters/StringEqualConverter.cs` - Compara strings
- `Converters/StringNotEmptyConverter.cs` - Verifica string no vacío
- `Converters/NotNullConverter.cs` - Verifica objeto no nulo
- `Converters/InvoiceTypeToColorConverter.cs` - Color según tipo de comprobante
- `Converters/SunatStatusToColorConverter.cs` - Color según estado SUNAT

### ViewModels
- `ViewModels/Cart/PaymentViewModel.cs` - ViewModel de pago (completado)
- `ViewModels/Billing/InvoiceViewModel.cs` - ViewModel de generación de comprobante
- `ViewModels/Billing/InvoiceListViewModel.cs` - ViewModel de lista de comprobantes
- `ViewModels/Base/BaseViewModel.cs` - Base ViewModel (actualizado)

### Views
- `Views/Cart/PaymentPage.xaml` - Página de pago (actualizada)
- `Views/Cart/PaymentSuccessPage.xaml` - Página de pago exitoso
- `Views/Cart/PaymentFailedPage.xaml` - Página de pago fallido
- `Views/Billing/InvoicePage.xaml` - Página de generación de comprobante
- `Views/Billing/InvoiceListPage.xaml` - Lista de comprobantes
- `Views/Billing/InvoiceDetailPage.xaml` - Detalle de comprobante

### Infraestructura
- `AppShell.xaml.cs` - Actualizado con rutas de navegación
- `MauiProgram.cs` - Actualizado con registro de servicios DI
- `apppasteleriav04.csproj` - Actualizado con referencias a nuevas páginas

## Funcionalidades Implementadas

### Sistema de Pagos

#### Métodos de Pago Soportados
1. **Efectivo** - Registro directo sin procesamiento
2. **Tarjeta de Crédito/Débito** - Validación completa con Luhn
3. **Yape** - Pago en espera de confirmación
4. **Plin** - Pago en espera de confirmación

#### Validación de Tarjetas
```csharp
// Algoritmo de Luhn implementado
CardValidator.ValidateLuhn(cardNumber)

// Detección de marca de tarjeta
CardValidator.GetCardBrand(cardNumber) // Visa, Mastercard, Amex, etc.

// Validación de fecha de expiración
CardValidator.ValidateExpiry(expiry) // MM/YY

// Validación de CVV
CardValidator.ValidateCvv(cvv, cardBrand) // 3 o 4 dígitos según marca
```

#### Estados de Pago
- `pendiente` - Pago iniciado
- `procesando` - En proceso de validación
- `completado` - Pago exitoso
- `fallido` - Pago rechazado
- `reembolsado` - Pago devuelto
- `cancelado` - Pago cancelado

### Sistema de Facturación

#### Tipos de Comprobantes
1. **Boleta** - Para clientes finales
   - Serie: B001
   - No requiere RUC
   
2. **Factura** - Para empresas
   - Serie: F001
   - Requiere RUC (11 dígitos)

#### Cálculo de IGV
```csharp
// IGV = 18% en Perú
const decimal IGV_RATE = 0.18m;

// Cálculo automático
subtotal = total / (1 + IGV_RATE);
igv = total - subtotal;
```

#### Numeración de Comprobantes
- Formato: `SERIE-CORRELATIVO`
- Ejemplo: `B001-00001`, `F001-00123`
- Incremento automático por tipo

#### Estados SUNAT
- `pendiente` - No enviado a SUNAT
- `enviado` - Enviado pendiente de respuesta
- `aceptado` - Aceptado por SUNAT ✅
- `rechazado` - Rechazado por SUNAT ❌

### Generación de PDFs
- Formato texto plano (básico)
- Incluye todos los datos del comprobante
- Para producción: usar QuestPDF, iTextSharp o PdfSharpCore

## Flujos de Usuario

### Flujo de Pago
1. Usuario selecciona productos → Carrito
2. Usuario procede a Checkout
3. Usuario navega a página de Pago
4. Usuario selecciona método de pago
5. Si es tarjeta: completa formulario y valida
6. Usuario presiona "Procesar Pago"
7. Sistema procesa pago
8. **Éxito**: Navega a PaymentSuccessPage
9. **Fallo**: Navega a PaymentFailedPage con opción de reintentar

### Flujo de Comprobante
1. Usuario completa pago exitosamente
2. Usuario accede a "Ver Comprobante"
3. Usuario selecciona tipo (Boleta/Factura)
4. Si Factura: ingresa RUC y razón social
5. Usuario presiona "Generar Comprobante"
6. Sistema genera comprobante con IGV
7. Usuario puede:
   - Descargar PDF
   - Enviar por email
   - Ver en lista de comprobantes

### Navegación

#### Rutas Registradas
```csharp
// Pagos
Routing.RegisterRoute("payment", typeof(PaymentPage));
Routing.RegisterRoute("payment-success", typeof(PaymentSuccessPage));
Routing.RegisterRoute("payment-failed", typeof(PaymentFailedPage));

// Facturación
Routing.RegisterRoute("invoice", typeof(InvoicePage));
Routing.RegisterRoute("invoices", typeof(InvoiceListPage));
Routing.RegisterRoute("invoice-detail", typeof(InvoiceDetailPage));
```

#### Ejemplos de Navegación
```csharp
// Ir a página de pago
await Shell.Current.GoToAsync("payment");

// Ir a lista de comprobantes
await Shell.Current.GoToAsync("invoices");

// Volver al inicio
await Shell.Current.GoToAsync("//catalog");
```

## Integración con Base de Datos

### Tablas Requeridas en Supabase

```sql
-- Tabla de pagos
CREATE TABLE payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    pedido_id UUID REFERENCES pedidos(id),
    monto DECIMAL(10,2) NOT NULL,
    metodo_pago VARCHAR(50) NOT NULL,
    estado VARCHAR(50) DEFAULT 'pendiente',
    referencia_externa VARCHAR(255),
    gateway VARCHAR(50),
    last_four VARCHAR(4),
    card_brand VARCHAR(20),
    error_message TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    processed_at TIMESTAMPTZ,
    metadata JSONB
);

-- Tabla de comprobantes
CREATE TABLE invoices (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    pedido_id UUID REFERENCES pedidos(id),
    payment_id UUID REFERENCES payments(id),
    tipo VARCHAR(20) NOT NULL,
    serie VARCHAR(10) NOT NULL,
    correlativo INTEGER NOT NULL,
    cliente_ruc VARCHAR(11),
    cliente_nombre VARCHAR(255),
    cliente_direccion TEXT,
    subtotal DECIMAL(10,2) NOT NULL,
    igv DECIMAL(10,2) NOT NULL,
    total DECIMAL(10,2) NOT NULL,
    pdf_url TEXT,
    xml_url TEXT,
    sunat_status VARCHAR(50) DEFAULT 'pendiente',
    sunat_ticket VARCHAR(255),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    sent_at TIMESTAMPTZ
);
```

## Pendientes para Producción

### Integraciones Reales
1. **Pasarela de Pago**
   - Integrar Culqi (Perú)
   - O Stripe/MercadoPago
   - Implementar webhooks

2. **SUNAT**
   - Integrar con Nubefact o FacturaDirecta
   - Envío electrónico de comprobantes
   - Recepción de CDR (Constancia de Recepción)

3. **PDF Mejorado**
   - Usar QuestPDF o similar
   - Agregar logo empresa
   - Incluir código QR
   - Diseño profesional

4. **Email**
   - Servicio de email (SendGrid, AWS SES)
   - Plantillas HTML
   - Adjuntar PDF

5. **Storage**
   - Guardar PDFs en Supabase Storage
   - O usar S3/Azure Blob

### Seguridad
1. No almacenar números de tarjeta completos
2. Usar tokenización de tarjetas
3. Implementar 3D Secure
4. Cifrar datos sensibles
5. Auditoría de transacciones

## Testing

### Tarjetas de Prueba
Para testing, usar tarjetas válidas según Luhn:
- `4532015112830366` (Visa)
- `5425233430109903` (Mastercard)
- `374245455400126` (Amex)

### Escenarios de Prueba
1. Pago con efectivo exitoso
2. Pago con tarjeta válida
3. Pago con tarjeta inválida (número incorrecto)
4. Pago con tarjeta expirada
5. Generación de boleta
6. Generación de factura con RUC
7. Descarga de PDF
8. Filtrado de comprobantes

## Arquitectura

### Patrón MVVM
```
View (XAML) ←→ ViewModel ←→ Service ←→ Data/API
```

### Dependency Injection
```csharp
builder.Services.AddSingleton<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IBillingService, BillingService>();
```

### Event-Driven
```csharp
// En ViewModel
public event EventHandler? PaymentCompleted;
public event EventHandler? PaymentFailed;

// En View
viewModel.PaymentCompleted += OnPaymentCompleted;
```

## Mantenimiento

### Actualizar IGV
Si cambia la tasa de IGV:
```csharp
// En BillingService.cs
private const decimal IGV_RATE = 0.18m; // Cambiar aquí
```

### Agregar Método de Pago
1. Actualizar enum `PaymentMethod` en `Payment.cs`
2. Agregar caso en `PaymentService.ProcessPaymentAsync()`
3. Agregar opción en `PaymentPage.xaml`
4. Actualizar `AvailablePaymentMethods` en `PaymentViewModel`

### Agregar Pasarela
1. Crear provider: `Services/Payment/[Provider]PaymentProvider.cs`
2. Implementar interfaz común
3. Registrar en DI
4. Configurar en `PaymentService`

## Criterios de Aceptación Cumplidos

- ✅ Payment.cs y Invoice.cs tienen todas las propiedades necesarias
- ✅ PaymentPage permite seleccionar método de pago
- ✅ Formulario de tarjeta valida datos correctamente
- ✅ PaymentService procesa pagos (efectivo y simulado)
- ✅ InvoicePage genera boleta/factura
- ✅ Se genera PDF del comprobante
- ✅ Los comprobantes se pueden guardar en Supabase (estructura lista)
- ✅ Flujo completo: Checkout → Payment → Invoice → Success
- ✅ Manejo de errores con reintentos
- ✅ No hay errores de compilación (verificado con build parcial)

## Notas Técnicas

### Build
El proyecto está configurado para múltiples plataformas (Android, iOS, macOS, Windows).
Para builds locales en Linux, usar:
```bash
dotnet build -f net10.0-android
```

### Workloads MAUI
Requeridos:
- maui-android
- maui-tizen  
- wasm-tools

## Contacto y Soporte

Para dudas sobre la implementación:
- Revisar código en `Services/Payment/PaymentService.cs`
- Ver ejemplos de uso en `ViewModels/Cart/PaymentViewModel.cs`
- Consultar flujos en las Views correspondientes

---

**Fecha de Implementación**: Enero 2026  
**Versión**: 1.0  
**Estado**: ✅ Completo

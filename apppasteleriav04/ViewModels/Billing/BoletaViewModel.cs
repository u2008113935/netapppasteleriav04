using System;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Billing
{
    public class BoletaViewModel : BaseViewModel
    {
        private Invoice? _invoice;
        public Invoice? Invoice
        {
            get => _invoice;
            set => SetProperty(ref _invoice, value);
        }

        private Order? _order;
        public Order? Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        private string _invoiceType = "boleta";
        public string InvoiceType
        {
            get => _invoiceType;
            set
            {
                SetProperty(ref _invoiceType, value);
                OnPropertyChanged(nameof(IsFactura));
            }
        }

        public bool IsFactura => InvoiceType == "factura";

        private string _customerRuc = string.Empty;
        public string CustomerRuc
        {
            get => _customerRuc;
            set => SetProperty(ref _customerRuc, value);
        }

        private string _customerName = string.Empty;
        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        private bool _isGenerating;
        public bool IsGenerating
        {
            get => _isGenerating;
            set => SetProperty(ref _isGenerating, value);
        }

        public ICommand GenerateInvoiceCommand { get; }
        public ICommand DownloadPdfCommand { get; }
        public ICommand SendEmailCommand { get; }

        public event EventHandler<Invoice>? InvoiceGenerated;

        public BoletaViewModel()
        {
            Title = "Generar Comprobante";
            GenerateInvoiceCommand = new AsyncRelayCommand(GenerateInvoiceAsync, () => !IsGenerating);
            DownloadPdfCommand = new AsyncRelayCommand(DownloadPdfAsync, () => Invoice != null);
            SendEmailCommand = new AsyncRelayCommand(SendEmailAsync, () => Invoice != null);
        }

        public void Initialize(Order order)
        {
            Order = order;
        }

        private async Task GenerateInvoiceAsync()
        {
            ErrorMessage = string.Empty;

            if (Order == null)
            {
                ErrorMessage = "Orden no válida";
                return;
            }

            if (IsFactura)
            {
                if (string.IsNullOrWhiteSpace(CustomerRuc) || CustomerRuc.Length != 11)
                {
                    ErrorMessage = "RUC debe tener 11 dígitos";
                    return;
                }

                if (string.IsNullOrWhiteSpace(CustomerName))
                {
                    ErrorMessage = "Razón social requerida";
                    return;
                }
            }

            IsGenerating = true;
            IsBusy = true;

            try
            {
                var igvRate = 0.18m;
                var subtotal = Order.Total / (1 + igvRate);
                var igv = Order.Total - subtotal;

                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    OrderId = Order.Id,
                    Type = InvoiceType,
                    SerialNumber = InvoiceType == "boleta" ? "B001" : "F001",
                    CorrelativeNumber = GenerateCorrelativeNumber(),
                    CustomerName = IsFactura ? CustomerName : "Cliente",
                    CustomerRuc = IsFactura ? CustomerRuc : null,
                    Subtotal = Math.Round(subtotal, 2),
                    Igv = Math.Round(igv, 2),
                    Total = Order.Total,
                    CreatedAt = DateTime.UtcNow
                };

                // Simulate invoice generation
                await Task.Delay(1500);

                Invoice = invoice;
                InvoiceGenerated?.Invoke(this, invoice);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al generar comprobante: {ex.Message}";
            }
            finally
            {
                IsGenerating = false;
                IsBusy = false;
            }
        }

        private async Task DownloadPdfAsync()
        {
            if (Invoice == null) return;

            IsBusy = true;
            try
            {
                // Simulate PDF download
                await Task.Delay(1000);
                // In a real app, this would download the PDF
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al descargar PDF: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SendEmailAsync()
        {
            if (Invoice == null) return;

            IsBusy = true;
            try
            {
                // Simulate email sending
                await Task.Delay(1000);
                // In a real app, this would send the email
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al enviar correo: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string GenerateCorrelativeNumber()
        {
            // In a real app, this would get the next number from the database
            var random = new Random();
            return random.Next(1, 99999).ToString("D8");
        }
    }
}

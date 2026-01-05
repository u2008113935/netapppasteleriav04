using System;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Models.Enums;
using apppasteleriav04.Services.Billing;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Billing
{
    public class InvoiceViewModel : BaseViewModel
    {
        private readonly IBillingService _billingService;

        private Invoice? _invoice;
        private Order? _order;
        private bool _isLoading;
        private string _invoiceType = "boleta";
        private string _customerRuc = string.Empty;
        private string _customerName = string.Empty;
        private string _customerAddress = string.Empty;

        public InvoiceViewModel()
        {
            _billingService = new BillingService();

            GenerateInvoiceCommand = new AsyncRelayCommand(GenerateInvoiceAsync, CanGenerateInvoice);
            DownloadPdfCommand = new AsyncRelayCommand(DownloadPdfAsync, CanDownloadPdf);
            SendEmailCommand = new AsyncRelayCommand<string>(SendEmailAsync, CanSendEmail);
        }

        public Invoice? Invoice
        {
            get => _invoice;
            set => SetProperty(ref _invoice, value);
        }

        public Order? Order
        {
            get => _order;
            set => SetProperty(ref _order, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string InvoiceType
        {
            get => _invoiceType;
            set
            {
                SetProperty(ref _invoiceType, value);
                ((AsyncRelayCommand)GenerateInvoiceCommand).RaiseCanExecuteChanged();
            }
        }

        public string CustomerRuc
        {
            get => _customerRuc;
            set => SetProperty(ref _customerRuc, value);
        }

        public string CustomerName
        {
            get => _customerName;
            set => SetProperty(ref _customerName, value);
        }

        public string CustomerAddress
        {
            get => _customerAddress;
            set => SetProperty(ref _customerAddress, value);
        }

        public ICommand GenerateInvoiceCommand { get; }
        public ICommand DownloadPdfCommand { get; }
        public ICommand SendEmailCommand { get; }

        public async Task GenerateInvoiceAsync()
        {
            try
            {
                IsLoading = true;

                if (Order == null)
                {
                    throw new InvalidOperationException("No hay pedido seleccionado");
                }

                var customerData = new CustomerData
                {
                    Name = CustomerName,
                    Ruc = InvoiceType == "factura" ? CustomerRuc : null,
                    Address = CustomerAddress
                };

                var type = InvoiceType == "boleta" ? InvoiceType.Boleta : InvoiceType.Factura;
                Invoice = await _billingService.GenerateInvoiceAsync(Order.Id, type, customerData);

                Title = $"Comprobante generado: {Invoice.SerialNumber}-{Invoice.CorrelativeNumber}";
            }
            catch (Exception ex)
            {
                // TODO: Show error message
                System.Diagnostics.Debug.WriteLine($"Error generating invoice: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task DownloadPdfAsync()
        {
            try
            {
                if (Invoice == null)
                    return;

                IsLoading = true;

                var pdfBytes = await _billingService.GeneratePdfAsync(Invoice.Id);

                // TODO: Save PDF to device
                // For now, just simulate the download
                await Task.Delay(1000);

                // TODO: Show success message
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error downloading PDF: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SendEmailAsync(string? email)
        {
            try
            {
                if (Invoice == null || string.IsNullOrEmpty(email))
                    return;

                IsLoading = true;

                var success = await _billingService.SendEmailAsync(Invoice.Id, email);

                if (success)
                {
                    // TODO: Show success message
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending email: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanGenerateInvoice()
        {
            if (InvoiceType == "factura")
            {
                return !string.IsNullOrEmpty(CustomerRuc) && 
                       !string.IsNullOrEmpty(CustomerName) &&
                       Order != null;
            }
            return Order != null;
        }

        private bool CanDownloadPdf(object? parameter)
        {
            return Invoice != null && !IsLoading;
        }

        private bool CanSendEmail(string? email)
        {
            return Invoice != null && !string.IsNullOrEmpty(email) && !IsLoading;
        }

        public void Initialize(Order order)
        {
            Order = order;
            CustomerName = "Cliente"; // Get from user profile if available
        }
    }
}

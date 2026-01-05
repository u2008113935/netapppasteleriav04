using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using apppasteleriav04.Helpers;
using apppasteleriav04.Models.Domain;
using apppasteleriav04.Services.Billing;
using apppasteleriav04.ViewModels.Base;

namespace apppasteleriav04.ViewModels.Billing
{
    public class InvoiceListViewModel : BaseViewModel
    {
        private readonly IBillingService _billingService;

        private ObservableCollection<Invoice> _invoices;
        private Invoice? _selectedInvoice;
        private DateTime _filterDateFrom = DateTime.Now.AddMonths(-1);
        private DateTime _filterDateTo = DateTime.Now;
        private string _filterType = "all";

        public InvoiceListViewModel()
        {
            _billingService = new BillingService();
            _invoices = new ObservableCollection<Invoice>();

            LoadInvoicesCommand = new AsyncRelayCommand(LoadInvoicesAsync);
            ViewInvoiceCommand = new AsyncRelayCommand<Invoice>(ViewInvoiceAsync);
            DownloadCommand = new AsyncRelayCommand<Invoice>(DownloadInvoiceAsync);
        }

        public ObservableCollection<Invoice> Invoices
        {
            get => _invoices;
            set => SetProperty(ref _invoices, value);
        }

        public Invoice? SelectedInvoice
        {
            get => _selectedInvoice;
            set => SetProperty(ref _selectedInvoice, value);
        }

        public DateTime FilterDateFrom
        {
            get => _filterDateFrom;
            set
            {
                SetProperty(ref _filterDateFrom, value);
                _ = LoadInvoicesAsync();
            }
        }

        public DateTime FilterDateTo
        {
            get => _filterDateTo;
            set
            {
                SetProperty(ref _filterDateTo, value);
                _ = LoadInvoicesAsync();
            }
        }

        public string FilterType
        {
            get => _filterType;
            set
            {
                SetProperty(ref _filterType, value);
                _ = LoadInvoicesAsync();
            }
        }

        public ICommand LoadInvoicesCommand { get; }
        public ICommand ViewInvoiceCommand { get; }
        public ICommand DownloadCommand { get; }

        public async Task LoadInvoicesAsync()
        {
            try
            {
                IsBusy = true;

                // TODO: Get current user ID
                var userId = Guid.NewGuid();

                var allInvoices = await _billingService.GetInvoicesByUserAsync(userId);

                // Apply filters
                var filtered = allInvoices
                    .Where(i => i.CreatedAt >= FilterDateFrom && i.CreatedAt <= FilterDateTo);

                if (FilterType != "all")
                {
                    filtered = filtered.Where(i => i.Type == FilterType);
                }

                Invoices.Clear();
                foreach (var invoice in filtered.OrderByDescending(i => i.CreatedAt))
                {
                    Invoices.Add(invoice);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading invoices: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ViewInvoiceAsync(Invoice? invoice)
        {
            if (invoice == null)
                return;

            SelectedInvoice = invoice;

            // TODO: Navigate to invoice detail page
            await Task.CompletedTask;
        }

        public async Task DownloadInvoiceAsync(Invoice? invoice)
        {
            if (invoice == null)
                return;

            try
            {
                IsBusy = true;

                var pdfBytes = await _billingService.GeneratePdfAsync(invoice.Id);

                // TODO: Save to device
                await Task.Delay(500);

                // TODO: Show success message
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error downloading invoice: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task InitializeAsync()
        {
            await LoadInvoicesAsync();
        }
    }
}

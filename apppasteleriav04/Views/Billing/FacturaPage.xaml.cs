using apppasteleriav04.ViewModels.Billing;
using apppasteleriav04.Models.Domain;

namespace apppasteleriav04.Views.Billing;

[QueryProperty(nameof(OrderId), "orderId")]
public partial class FacturaPage : ContentPage
{
    private readonly BoletaViewModel _viewModel;
    
    public string OrderId { get; set; } = string.Empty;

    public FacturaPage()
    {
        InitializeComponent();
        _viewModel = new BoletaViewModel();
        _viewModel.InvoiceType = "factura"; // Default to factura
        BindingContext = _viewModel;

        _viewModel.InvoiceGenerated += OnInvoiceGenerated;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (!string.IsNullOrEmpty(OrderId) && Guid.TryParse(OrderId, out var orderId))
        {
            var order = new Order
            {
                Id = orderId,
                Total = 100.00m,
                CreatedAt = DateTime.Now
            };
            _viewModel.Initialize(order);
        }
    }

    private async void OnInvoiceGenerated(object? sender, Invoice invoice)
    {
        await DisplayAlert("Éxito", 
            $"Factura {invoice.SerialNumber}-{invoice.CorrelativeNumber} generada correctamente", 
            "OK");
    }

    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        // Validate RUC
        if (string.IsNullOrWhiteSpace(_viewModel.CustomerRuc) || _viewModel.CustomerRuc.Length != 11)
        {
            await DisplayAlert("Error", "El RUC debe tener 11 dígitos", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(_viewModel.CustomerName))
        {
            await DisplayAlert("Error", "La razón social es requerida", "OK");
            return;
        }

        _viewModel.GenerateInvoiceCommand.Execute(null);
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        _viewModel.DownloadPdfCommand.Execute(null);
        await DisplayAlert("Descarga", "PDF de factura descargado correctamente", "OK");
    }

    private async void OnSendEmailClicked(object sender, EventArgs e)
    {
        string? email = await DisplayPromptAsync("Enviar Email", 
            "Ingrese el correo electrónico del cliente:", 
            placeholder: "empresa@email.com", 
            keyboard: Keyboard.Email);
        
        if (!string.IsNullOrWhiteSpace(email))
        {
            _viewModel.SendEmailCommand.Execute(null);
            await DisplayAlert("Email", $"Factura enviada a {email}", "OK");
        }
    }
}
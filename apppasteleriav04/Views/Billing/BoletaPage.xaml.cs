using apppasteleriav04.ViewModels.Billing;
using apppasteleriav04.Models.Domain;

namespace apppasteleriav04.Views.Billing;

[QueryProperty(nameof(OrderId), "orderId")]
public partial class BoletaPage : ContentPage
{
    private readonly BoletaViewModel _viewModel;
    
    public string OrderId { get; set; } = string.Empty;

    public BoletaPage()
    {
        InitializeComponent();
        _viewModel = new BoletaViewModel();
        BindingContext = _viewModel;

        _viewModel.InvoiceGenerated += OnInvoiceGenerated;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (!string.IsNullOrEmpty(OrderId) && Guid.TryParse(OrderId, out var orderId))
        {
            // Load order and initialize viewmodel
            // For now, create a sample order
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
            $"Boleta {invoice.SerialNumber}-{invoice.CorrelativeNumber} generada correctamente", 
            "OK");
    }

    private void OnBoletaSelected(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            _viewModel.InvoiceType = "boleta";
            if (FindByName("RucSection") is VisualElement rucSection)
                rucSection.IsVisible = false;
        }
    }

    private void OnFacturaSelected(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            _viewModel.InvoiceType = "factura";
            if (FindByName("RucSection") is VisualElement rucSection)
                rucSection.IsVisible = true;
        }
    }

    private async void OnGenerateClicked(object sender, EventArgs e)
    {
        _viewModel.GenerateInvoiceCommand.Execute(null);
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        _viewModel.DownloadPdfCommand.Execute(null);
        await DisplayAlert("Descarga", "PDF descargado correctamente", "OK");
    }

    private async void OnSendEmailClicked(object sender, EventArgs e)
    {
        string? email = await DisplayPromptAsync("Enviar Email", 
            "Ingrese su correo electrónico:", 
            placeholder: "ejemplo@email.com", 
            keyboard: Keyboard.Email);
        
        if (!string.IsNullOrWhiteSpace(email))
        {
            _viewModel.SendEmailCommand.Execute(null);
            await DisplayAlert("Email", $"Boleta enviada a {email}", "OK");
        }
    }
}
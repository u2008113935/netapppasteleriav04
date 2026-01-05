using apppasteleriav04.ViewModels.Billing;

namespace apppasteleriav04.Views.Billing;

public partial class InvoicePage : ContentPage
{
    private readonly InvoiceViewModel _viewModel;

    public InvoicePage()
    {
        InitializeComponent();
        _viewModel = new InvoiceViewModel();
        BindingContext = _viewModel;

        // Handle property changes
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(InvoiceViewModel.Invoice))
            {
                UpdateInvoicePreview();
            }
        };
    }

    public InvoicePage(InvoiceViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private void OnInvoiceTypeChanged(object sender, CheckedChangedEventArgs e)
    {
        if (BoletaRadio.IsChecked)
        {
            _viewModel.InvoiceType = "boleta";
            RucSection.IsVisible = false;
        }
        else if (FacturaRadio.IsChecked)
        {
            _viewModel.InvoiceType = "factura";
            RucSection.IsVisible = true;
        }
    }

    private void UpdateInvoicePreview()
    {
        if (_viewModel.Invoice != null)
        {
            InvoicePreview.IsVisible = true;
            InvoiceActions.IsVisible = true;

            PreviewType.Text = _viewModel.Invoice.Type;
            PreviewNumber.Text = $"{_viewModel.Invoice.SerialNumber}-{_viewModel.Invoice.CorrelativeNumber:D5}";
            PreviewCustomer.Text = _viewModel.Invoice.CustomerName;
            PreviewSubtotal.Text = $"S/ {_viewModel.Invoice.Subtotal:F2}";
            PreviewIgv.Text = $"S/ {_viewModel.Invoice.Igv:F2}";
            PreviewTotal.Text = $"S/ {_viewModel.Invoice.Total:F2}";
        }
        else
        {
            InvoicePreview.IsVisible = false;
            InvoiceActions.IsVisible = false;
        }
    }

    private async void OnSendEmailClicked(object sender, EventArgs e)
    {
        string? email = await DisplayPromptAsync("Enviar Email", 
            "Ingrese su correo electrónico:", 
            placeholder: "ejemplo@email.com", 
            keyboard: Keyboard.Email);
        
        if (!string.IsNullOrWhiteSpace(email))
        {
            await _viewModel.SendEmailAsync(email);
        }
    }
}


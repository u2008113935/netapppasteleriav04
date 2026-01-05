namespace apppasteleriav04.Views.Billing;

public partial class InvoiceDetailPage : ContentPage
{
    public InvoiceDetailPage()
    {
        InitializeComponent();
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        // TODO: Download PDF
        await DisplayAlert("Descargar", "Descargando PDF...", "OK");
    }

    private async void OnSendEmailClicked(object sender, EventArgs e)
    {
        // TODO: Send email
        string email = await DisplayPromptAsync("Enviar Email", "Ingrese su correo electrónico:", 
            placeholder: "ejemplo@email.com", keyboard: Keyboard.Email);
        
        if (!string.IsNullOrWhiteSpace(email))
        {
            await DisplayAlert("Email", $"Comprobante enviado a {email}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Anular Comprobante", 
            "¿Está seguro de anular este comprobante?", "Sí", "No");
        
        if (confirm)
        {
            // TODO: Cancel invoice
            await DisplayAlert("Anulado", "Comprobante anulado exitosamente", "OK");
        }
    }
}

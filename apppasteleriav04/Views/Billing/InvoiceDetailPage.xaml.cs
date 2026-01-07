namespace apppasteleriav04.Views.Billing;

public partial class InvoiceDetailPage : ContentPage
{
    public InvoiceDetailPage()
    {
        InitializeComponent();
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        try
        {
            // Get the invoice ID from binding context
            var invoice = BindingContext as Models.Domain.Invoice;
            if (invoice == null)
            {
                await DisplayAlert("Error", "No se pudo obtener el comprobante", "OK");
                return;
            }

            // Generate PDF
            var billingService = new Services.Billing.BillingService();
            var pdfBytes = await billingService.GeneratePdfAsync(invoice.Id);
            
            // Sanitize filename to remove invalid characters
            var serial = string.Join("_", invoice.SerialNumber.Split(Path.GetInvalidFileNameChars()));
            var correlative = string.Join("_", invoice.CorrelativeNumber.ToString().Split(Path.GetInvalidFileNameChars()));
            var fileName = $"comprobante_{serial}_{correlative}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            
            await DisplayAlert("Descarga", $"Comprobante guardado como {fileName}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al descargar: {ex.Message}", "OK");
        }
    }

    private async void OnSendEmailClicked(object sender, EventArgs e)
    {
        try
        {
            string email = await DisplayPromptAsync("Enviar Email", "Ingrese su correo electrónico:", 
                placeholder: "ejemplo@email.com", keyboard: Keyboard.Email);
            
            if (string.IsNullOrWhiteSpace(email))
                return;

            // Get the invoice ID from binding context
            var invoice = BindingContext as Models.Domain.Invoice;
            if (invoice == null)
            {
                await DisplayAlert("Error", "No se pudo obtener el comprobante", "OK");
                return;
            }

            // Send email through billing service
            var billingService = new Services.Billing.BillingService();
            var success = await billingService.SendEmailAsync(invoice.Id, email);
            
            if (success)
                await DisplayAlert("Email", $"Comprobante enviado a {email}", "OK");
            else
                await DisplayAlert("Error", "No se pudo enviar el email", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al enviar: {ex.Message}", "OK");
        }
    }

    // Removed OnCancelClicked since the button is commented out
}
